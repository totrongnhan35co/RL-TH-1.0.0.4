using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using BarcodeVerificationSystem.Utils;
using System;
using System.Threading;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
{
    /// <summary>
    /// Định kỳ quét is_sent=0 từ 3 bảng log SQLite và gửi lên R-Link Master API.
    /// Không dùng bảng pending_logs nữa — retry trực tiếp từ rlink_log_*.
    /// </summary>
    public class RLinkLogRetrySenderService : IDisposable
    {
        private static RLinkLogRetrySenderService _instance;
        public static RLinkLogRetrySenderService Instance
            => _instance ?? (_instance = new RLinkLogRetrySenderService());

        private Thread _retryThread;
        private volatile bool _running;
        private volatile bool _paused;

        private readonly ManualResetEventSlim _retryTrigger = new ManualResetEventSlim(false);

        public int IntervalSeconds { get; set; } = 60;
        public bool IsPaused => _paused;

        public void Pause() => _paused = true;
        public void Resume() => _paused = false;

        private RLinkLogRetrySenderService() { }

        public void Start()
        {
            if (_running) return;
            _running = true;
            _retryThread = new Thread(RetryLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "RLinkLogRetryThread"
            };
            _retryThread.Start();
            ProjectLogger.WriteInfo("[LogRetrySender] Khởi động — retry mỗi " + IntervalSeconds + "s.");
        }

        public void Stop() => _running = false;

        public void TriggerRetryNow()
        {
            _retryTrigger.Set();
        }

        private void RetryLoop()
        {
            while (_running)
            {
                if (_paused)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                try
                {
                    var svc = RLinkMasterServiceFactory.Instance;

                    // ── rlink_log_in ── PG trước, SQLite fallback
                    var logInPg = RLinkLogService.GetUnsentLogInPg(50);
                    var logInRows = logInPg.Count > 0
                        ? logInPg
                        : RLinkLogService.GetUnsentLogIn(50);
                    foreach (var row in logInRows)
                    {
                        if (!_running) break;
                        try
                        {
                            bool ok = svc.SendLogStatusAsync(row.payload).GetAwaiter().GetResult();
                            if (ok)
                            {
                                RLinkLogService.MarkLogRowSentPg(THDb.LogIn, row.id);
                                RLinkLogService.MarkLogRowSent(THDb.LogIn, row.id);
                                ProjectLogger.WriteInfo($"[LogRetrySender] ✔ {THDb.LogIn} id={row.id}");
                            }
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteError($"[LogRetrySender] {THDb.LogIn} id={row.id} lỗi: {ex.Message}");
                        }
                    }

                    // ── rlink_log_camera ── PG trước, SQLite fallback
                    var logCamPg = RLinkLogService.GetUnsentLogCameraPg(50);
                    var logCamRows = logCamPg.Count > 0
                        ? logCamPg
                        : RLinkLogService.GetUnsentLogCamera(50);
                    foreach (var row in logCamRows)
                    {
                        if (!_running) break;
                        try
                        {
                            bool ok = svc.SendLogCameraAsync(row.payload).GetAwaiter().GetResult();
                            if (ok)
                            {
                                RLinkLogService.MarkLogRowSentPg(THDb.LogCamera, row.id);
                                RLinkLogService.MarkLogRowSent(THDb.LogCamera, row.id);
                                ProjectLogger.WriteInfo($"[LogRetrySender] ✔ {THDb.LogCamera} id={row.id}");
                            }
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteError($"[LogRetrySender] {THDb.LogCamera} id={row.id} lỗi: {ex.Message}");
                        }
                    }

                    // ── rlink_log_camera_error ── PG trước, SQLite fallback
                    var logErrPg = RLinkLogService.GetUnsentLogCameraErrorPg(50);
                    var logErrRows = logErrPg.Count > 0
                        ? logErrPg
                        : RLinkLogService.GetUnsentLogCameraError(50);
                    foreach (var row in logErrRows)
                    {
                        if (!_running) break;
                        try
                        {
                            bool ok = svc.SendLogCameraErrorAsync(row.payload).GetAwaiter().GetResult();
                            if (ok)
                            {
                                RLinkLogService.MarkLogRowSentPg(THDb.LogCameraError, row.id);
                                RLinkLogService.MarkLogRowSent(THDb.LogCameraError, row.id);
                                ProjectLogger.WriteInfo($"[LogRetrySender] ✔ {THDb.LogCameraError} id={row.id}");
                            }
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteError($"[LogRetrySender] {THDb.LogCameraError} id={row.id} lỗi: {ex.Message}");
                        }
                    }

                    // ── Báo cáo sync status lên server ───────────────
                    var pgIn = RLinkLogService.GetUnsentLogInPg(1000);
                    var pgCam = RLinkLogService.GetUnsentLogCameraPg(1000);
                    var pgErr = RLinkLogService.GetUnsentLogCameraErrorPg(1000);
                    int pendingIn = pgIn.Count > 0 ? pgIn.Count : RLinkLogService.GetUnsentLogIn(1000).Count;
                    int pendingCam = pgCam.Count > 0 ? pgCam.Count : RLinkLogService.GetUnsentLogCamera(1000).Count;
                    int pendingErr = pgErr.Count > 0 ? pgErr.Count : RLinkLogService.GetUnsentLogCameraError(1000).Count;
                    ReportSyncStatus(svc, pendingIn, pendingCam, pendingErr);

                    // ── Poll resync request từ web UI ─────────────────
                    bool resyncNow = PollResyncRequest(svc);
                    if (resyncNow)
                    {
                        ProjectLogger.WriteInfo("[LogRetrySender] ← Web UI yêu cầu đồng bộ ngay.");
                        continue; // bỏ qua sleep, chạy lại ngay
                    }
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[LogRetrySender] RetryLoop lỗi: " + ex.Message, ex);
                }

                // ── Chờ đến interval hoặc trigger thủ công ────────────
                _retryTrigger.Wait(IntervalSeconds * 1000); // thoát ngay khi TriggerRetryNow()
                _retryTrigger.Reset();                      // reset để lần sau còn dùng được
            }
        }

        private void ReportSyncStatus(IRLinkMasterService svc, int pendingIn, int pendingCam, int pendingErr)
        {
            try
            {
                var jobDetailsPg = RLinkLogService.GetUnsentCountsByJobPg();
                var jobDetails = jobDetailsPg.Count > 0
                    ? jobDetailsPg
                    : RLinkLogService.GetUnsentCountsByJob();
                var report = new SyncReportPayload
                {
                    line_id = Shared.Settings?.LineId ?? "",
                    rlink_name = Shared.Settings?.RLinkName ?? "",
                    pending_in = pendingIn,
                    pending_camera = pendingCam,
                    pending_error = pendingErr,
                    job_details = jobDetails
                };
                svc.PostSyncReportAsync(report).GetAwaiter().GetResult();
            }
            catch { }
        }

        private bool PollResyncRequest(IRLinkMasterService svc)
        {
            try
            {
                string lineId = Shared.Settings?.LineId ?? "";
                return svc.CheckResyncRequestAsync(lineId).GetAwaiter().GetResult();
            }
            catch { return false; }
        }

        public void Dispose() => Stop();
    }
}