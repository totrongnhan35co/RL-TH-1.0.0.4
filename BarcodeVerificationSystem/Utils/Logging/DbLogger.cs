using System;
using System.IO;
using System.Text;

namespace BarcodeVerificationSystem.Utils.Logging
{
    /// <summary>
    /// Ghi log database riêng vào file db_log_{date}.txt
    /// </summary>
    public static class DbLogger
    {
        private static readonly string LogDirectory =
            Path.Combine(CommonVariable.CommVariables.PathProgramDataApp, "Logs", "DB");

        private static readonly object _lock = new object();

        static DbLogger()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);
            }
            catch { }
        }

        private static string GetFilePath()
            => Path.Combine(LogDirectory, $"db_log_{DateTime.Now:yyyy-MM-dd}.txt");

        public static void Info(string message)
        {
            Write("INFO", message, null);
        }

        public static void Warning(string message)
        {
            Write("WARN", message, null);
        }

        public static void Error(string message, Exception ex = null)
        {
            Write("ERROR", message, ex);
        }

        private static void Write(string level, string message, Exception ex)
        {
            try
            {
                lock (_lock)
                {
                    string file = GetFilePath();
                    var sb = new StringBuilder();
                    sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
                    if (ex != null)
                    {
                        sb.AppendLine();
                        sb.Append($"  Exception: {ex.GetType().Name}: {ex.Message}");
                        if (ex.InnerException != null)
                            sb.Append($" | Inner: {ex.InnerException.Message}");
                    }
                    sb.AppendLine();
                    File.AppendAllText(file, sb.ToString(), Encoding.UTF8);
                }
            }
            catch { }
        }
    }
}
