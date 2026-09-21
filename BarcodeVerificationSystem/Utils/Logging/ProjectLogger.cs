//using Serilog;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace BarcodeVerificationSystem.Utils
//{
//    public class ProjectLogger
//    {
//        private static readonly string LogDirectory =
//            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "R-Link", "Logs");

//        private static readonly string LogFilePath =
//            Path.Combine(LogDirectory, "log.txt");

//        static ProjectLogger()
//        {
//            try
//            {
//                // Ensure folder exists
//                if (!Directory.Exists(LogDirectory))
//                {
//                    Directory.CreateDirectory(LogDirectory);
//                }

//                // Ensure file exists
//                if (!File.Exists(LogFilePath))
//                {
//                    using (File.Create(LogFilePath)) { }
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("Logger initialization failed: " + ex.Message);
//            }
//        }

//        public static void OpenErrorFile()
//        {
//            try
//            {
//                if (!File.Exists(LogFilePath))
//                {
//                    using (File.Create(LogFilePath)) { }
//                }

//                Process.Start("notepad.exe", LogFilePath);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("Failed to open log file: " + ex.Message);
//            }
//        }

//        public static void WriteError(string message, Exception ex = null)
//        {
//            try
//            {
//                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
//                if (ex != null)
//                {
//                    logMessage += Environment.NewLine + ex.ToString();
//                }
//                logMessage += Environment.NewLine;

//                // Read existing content
//                string existingContent = File.Exists(LogFilePath) ? File.ReadAllText(LogFilePath) : string.Empty;

//                // Prepend new message
//                File.WriteAllText(LogFilePath, logMessage + existingContent);
//            }
//            catch (Exception logEx)
//            {
//                Debug.WriteLine("Logging failed: " + logEx.Message);
//            }
//        }

//        public static void WriteInfo(string message)
//        {
//            try
//            {
//                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
//                logMessage += Environment.NewLine;

//                // Read existing content
//                string existingContent = File.Exists(LogFilePath) ? File.ReadAllText(LogFilePath) : string.Empty;

//                // Prepend new message
//                File.WriteAllText(LogFilePath, logMessage + existingContent);
//            }
//            catch (Exception logEx)
//            {
//                Debug.WriteLine("Logging failed: " + logEx.Message);
//            }
//        }

//        public static void WriteWarning(string message)
//        {
//            try
//            {
//                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARNING: {message}";
//                logMessage += Environment.NewLine;

//                // Read existing content
//                string existingContent = File.Exists(LogFilePath) ? File.ReadAllText(LogFilePath) : string.Empty;

//                // Prepend new message
//                File.WriteAllText(LogFilePath, logMessage + existingContent);
//            }
//            catch (Exception logEx)
//            {
//                Debug.WriteLine("Logging failed: " + logEx.Message);
//            }
//        }

//        public static void WriteDebug(string message)
//        {
//            try
//            {
//                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DEBUG: {message}";
//                logMessage += Environment.NewLine;

//                // Read existing content
//                string existingContent = File.Exists(LogFilePath) ? File.ReadAllText(LogFilePath) : string.Empty;

//                // Prepend new message
//                File.WriteAllText(LogFilePath, logMessage + existingContent);
//            }
//            catch (Exception logEx)
//            {
//                Debug.WriteLine("Logging failed: " + logEx.Message);
//            }
//        }

//        public static void Close()
//        {
//            // No resources to close in this implementation
//        }
//    }
//}

////  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log-.txt");

//// string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "R-Link", "Database");
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BarcodeVerificationSystem.Utils
{
    /// <summary>
    /// Ghi log đồng thời vào 2 file:
    ///   - log.txt         : file cũ, tương thích với code trước (append, mới nhất ở dưới)
    ///   - log_{date}.txt  : file theo ngày, dễ tra cứu theo ca / ngày
    /// Dùng StreamWriter append mode → O(1) mỗi lần ghi, không drop log dưới tải cao.
    /// </summary>
    public class ProjectLogger
    {
        private static readonly string LogDirectory =
            Path.Combine(CommonVariable.CommVariables.PathProgramDataApp, "Logs");

        // ── File cũ (tương thích ngược) ──────────────────────────────────────
        private static readonly string LegacyLogFilePath;

        // ── File theo ngày ────────────────────────────────────────────────────
        private static string GetDatedLogFilePath()
            => Path.Combine(LogDirectory, string.Format("log_{0:yyyy-MM-dd}.txt", DateTime.Now));

        private static readonly object _fileLock = new object();
        private static readonly int _keepDays = 180;

        // Dùng DateTime? để tự reset khi sang ngày mới (thay cho bool không bao giờ reset)
        private static DateTime? _lastSessionDate = null;

        static ProjectLogger()
        {
            LegacyLogFilePath = Path.Combine(LogDirectory, "log.txt");
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                // Đảm bảo file log.txt tồn tại (tương thích code cũ)
                if (!File.Exists(LegacyLogFilePath))
                    File.WriteAllText(LegacyLogFilePath, "", Encoding.UTF8);

                CleanupOldLogs();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Logger initialization failed: " + ex.Message);
            }
        }

        private static void WriteLog(string level, string message, Exception ex = null)
        {
            try
            {
                lock (_fileLock)
                {
                    DateTime now = DateTime.Now;
                    string timestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    var sb = new StringBuilder();

                    // Session header — tự reset khi sang ngày mới
                    bool isNewSession = (_lastSessionDate == null || _lastSessionDate.Value.Date != now.Date);
                    if (isNewSession)
                    {
                        _lastSessionDate = now;
                        sb.AppendLine();
                        sb.AppendLine("================================================================");
                        sb.AppendLine(string.Format("  Session Started: {0:yyyy-MM-dd HH:mm:ss}", now));
                        sb.AppendLine("================================================================");
                    }

                    sb.AppendFormat("[{0}] [{1}] {2}", timestamp, level, message);

                    if (ex != null)
                    {
                        sb.AppendLine();
                        sb.AppendFormat("             Exception : {0} - {1}", ex.GetType().Name, ex.Message);
                        if (ex.StackTrace != null)
                        {
                            sb.AppendLine();
                            sb.AppendFormat("             StackTrace: {0}", ex.StackTrace.Trim());
                        }
                    }

                    sb.AppendLine();

                    string entry = sb.ToString();

                    // ── 1. Ghi vào log.txt (file cũ — tương thích ngược) ─────
                    using (var w = new StreamWriter(LegacyLogFilePath, append: true, Encoding.UTF8))
                        w.Write(entry);

                    // ── 2. Ghi vào log_{date}.txt (file theo ngày) ────────────
                    using (var w = new StreamWriter(GetDatedLogFilePath(), append: true, Encoding.UTF8))
                        w.Write(entry);
                }
            }
            catch (Exception logEx)
            {
                Debug.WriteLine("Logging failed: " + logEx.Message);
            }
        }

        private static void CleanupOldLogs()
        {
            try
            {
                DateTime cutoff = DateTime.Now.AddDays(-_keepDays);
                foreach (string file in Directory.GetFiles(LogDirectory, "log_*.txt"))
                {
                    string datePart = Path.GetFileNameWithoutExtension(file).Substring(4);
                    DateTime fileDate;
                    if (DateTime.TryParse(datePart, out fileDate) && fileDate < cutoff)
                        File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CleanupOldLogs failed: " + ex.Message);
            }
        }

        public static void WriteError(string message, Exception ex = null)
            => WriteLog("ERROR  ", message, ex);

        public static void WriteInfo(string message)
            => WriteLog("INFO   ", message);

        public static void WriteWarning(string message)
            => WriteLog("WARNING", message);

        public static void WriteDebug(string message)
            => WriteLog("DEBUG  ", message);

        /// <summary>Mở log.txt bằng Notepad (tương thích nút cũ trên UI).</summary>
        public static void OpenErrorFile()
        {
            try { Process.Start("notepad.exe", LegacyLogFilePath); }
            catch (Exception ex) { Debug.WriteLine("Failed to open log file: " + ex.Message); }
        }

        /// <summary>Mở file log theo ngày hôm nay bằng Notepad.</summary>
        public static void OpenTodayLogFile()
        {
            try
            {
                string path = GetDatedLogFilePath();
                if (!File.Exists(path))
                    File.WriteAllText(path, "", Encoding.UTF8);
                Process.Start("notepad.exe", path);
            }
            catch (Exception ex) { Debug.WriteLine("Failed to open today log file: " + ex.Message); }
        }

        public static void OpenLogDirectory()
        {
            try { Process.Start("explorer.exe", LogDirectory); }
            catch (Exception ex) { Debug.WriteLine("Failed to open log directory: " + ex.Message); }
        }

        public static void Close() { }
    }
}