using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.THTrueMilk;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.View.UcSettings;
using CommonVariable;
using System;
using System.IO;
using System.Threading;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
{
    /// <summary>
    /// Background service tự động dọn dẹp dữ liệu cũ:<br/>
    /// - Xóa file job (.rvis) và dữ liệu liên quan cũ hơn N ngày<br/>
    /// - Xóa ảnh lỗi cũ hơn N ngày<br/>
    /// - Xóa pending_logs SQLite đã gửi cũ hơn N ngày<br/>
    /// - Xóa bản ghi PostgreSQL rlink_log_* cũ hơn N ngày<br/>
    /// Chạy mỗi 6 giờ, ưu tiên BelowNormal — không ảnh hưởng sản xuất.
    /// </summary>
    public class RLinkAutoCleanupService : IDisposable
    {
        private static RLinkAutoCleanupService _instance;
        public static RLinkAutoCleanupService Instance
            => _instance ?? (_instance = new RLinkAutoCleanupService());

        private Thread _thread;
        private volatile bool _running;
        private const int RunIntervalHours = 6;

        private RLinkAutoCleanupService() { }

        public void Start()
        {
            if (_running) return;
            _running = true;
            _thread = new Thread(CleanupLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "RLinkAutoCleanupThread"
            };
            _thread.Start();
            ProjectLogger.WriteInfo("[AutoCleanup] Khởi động — chạy mỗi " + RunIntervalHours + " giờ.");
        }

        public void Stop() => _running = false;

        private void CleanupLoop()
        {
            // Chờ 5 phút sau khi khởi động để tránh ảnh hưởng load ban đầu
            SleepInterruptible(TimeSpan.FromMinutes(5));

            while (_running)
            {
                try
                {
                    int days = Shared.Settings?.THRetentionDays > 0
                               ? Shared.Settings.THRetentionDays
                               : 180;

                    ProjectLogger.WriteInfo($"[AutoCleanup] Bắt đầu dọn dữ liệu cũ hơn {days} ngày...");
                    DateTime threshold = DateTime.Now.AddDays(-days);

                    CleanupOldJobs(threshold);
                    CleanupErrorImages(threshold);
                    //RLinkLogService.DeleteOldPendingLogs(days);
                    CleanupOldJobDataFiles(threshold);
                    CleanupPostgresLogs(days);

                    ProjectLogger.WriteInfo("[AutoCleanup] Hoàn tất.");
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[AutoCleanup] CleanupLoop lỗi: " + ex.Message, ex);
                }

                SleepInterruptible(TimeSpan.FromHours(RunIntervalHours));
            }
        }

        // ── Xóa file job (.rvis) cũ ──────────────────────────────────────────

        private static void CleanupOldJobs(DateTime threshold)
        {
            try
            {
                string jobDir = CommVariables.PathJobsApp;
                if (!Directory.Exists(jobDir)) return;

                string ext = Shared.Settings?.JobFileExtension ?? ".rvis";
                var files = Directory.GetFiles(jobDir, "*" + ext);
                int deleted = 0;
                foreach (var file in files)
                {
                    try
                    {
                        if (File.GetLastWriteTime(file) < threshold)
                        {
                            File.Delete(file);
                            deleted++;
                        }
                    }
                    catch { }
                }
                if (deleted > 0)
                    ProjectLogger.WriteInfo($"[AutoCleanup] Đã xóa {deleted} file job cũ.");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[AutoCleanup] CleanupOldJobs lỗi: " + ex.Message);
            }
        }

        // ── Xóa ảnh lỗi cũ ──────────────────────────────────────────────────

        private static void CleanupErrorImages(DateTime threshold)
        {
            try
            {
                string folder = Shared.Settings?.THErrorImageFolder;
                if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return;

                var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
                int deleted = 0;
                foreach (var file in files)
                {
                    try
                    {
                        if (File.GetCreationTime(file) < threshold)
                        {
                            File.Delete(file);
                            deleted++;
                        }
                    }
                    catch { }
                }
                if (deleted > 0)
                    ProjectLogger.WriteInfo($"[AutoCleanup] Đã xóa {deleted} ảnh lỗi cũ.");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[AutoCleanup] CleanupErrorImages lỗi: " + ex.Message);
            }
        }

        // ── Xóa CSV/file dữ liệu job cũ (checked, sent, printed) ────────────

        private static void CleanupOldJobDataFiles(DateTime threshold)
        {
            string[] dataDirs =
            {
                CommVariables.PathCheckedResult,
                CommVariables.PathSentDataChecked,
                CommVariables.PathSentDataPrinted,
                CommVariables.PathPrintedResponse
            };

            int total = 0;
            foreach (var dir in dataDirs)
            {
                if (!Directory.Exists(dir)) continue;
                try
                {
                    foreach (var file in Directory.GetFiles(dir, "*.*"))
                    {
                        try
                        {
                            if (File.GetLastWriteTime(file) < threshold)
                            {
                                File.Delete(file);
                                total++;
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
            if (total > 0)
                ProjectLogger.WriteInfo($"[AutoCleanup] Đã xóa {total} file dữ liệu job cũ.");
        }

        // ── Xóa bản ghi PostgreSQL log cũ ────────────────────────────────────

        private static void CleanupPostgresLogs(int retentionDays)
        {
            try
            {
                string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                if (string.IsNullOrWhiteSpace(connStr)) return;

                string[] tables = { THDb.LogIn, THDb.LogCamera, THDb.LogCameraError };
                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    foreach (var table in tables)
                    {
                        try
                        {
                            using (var cmd = new Npgsql.NpgsqlCommand(
                                $"DELETE FROM \"{table}\" WHERE {THDb.CreatedAt} < NOW() - INTERVAL '{retentionDays} days'", conn))
                            {
                                int deleted = cmd.ExecuteNonQuery();
                                if (deleted > 0)
                                    ProjectLogger.WriteInfo($"[AutoCleanup] PG {table}: xóa {deleted} bản ghi cũ.");
                            }
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteWarning($"[AutoCleanup] PG {table} lỗi: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[AutoCleanup] CleanupPostgresLogs lỗi: " + ex.Message);
            }
        }

        // ── Helper sleep có thể ngắt ─────────────────────────────────────────

        private void SleepInterruptible(TimeSpan duration)
        {
            var end = DateTime.Now + duration;
            while (_running && DateTime.Now < end)
                Thread.Sleep(1000);
        }

        public void Dispose() => Stop();
    }
}