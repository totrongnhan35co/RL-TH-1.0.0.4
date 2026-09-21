using System;
using System.IO;

namespace BarcodeVerificationSystem.Utils
{
    public class Logger
    {
        private static string _logDirectory;

        public static void LogError(string message)
        {
            Log("ERROR", message);
        }
        public static void LogWarning(string message)
        {
            Log("WARNING", message);
        }
        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        private static void Log(string logType, string message)
        {
            string logFilePath = GetLogFilePath();
            if (logFilePath == string.Empty) return;
            try
            {
                using(var writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now} [{logType}]: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log message: {ex.Message}");
            }
        }

        private static string GetLogFilePath()
        {
            try
            {
                _logDirectory = Path.Combine(CommonVariable.CommVariables.PathProgramDataApp, "Logs", "Exception");
                Directory.CreateDirectory(_logDirectory);
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                return Path.Combine(_logDirectory, $"log_{date}.txt");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
