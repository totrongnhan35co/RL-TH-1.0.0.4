using System;
using System.IO;
using System.Linq;
using System.Text;

namespace BarcodeVerificationSystem.Controller.ZebraPrinter
{
    internal static class PrinterLogger
    {
        private static readonly string _baseDir =
            Path.Combine(CommonVariable.CommVariables.PathProgramDataApp, "PrinterLogs");

        internal static void Log(string jobName, string printerIp, string direction,
                                 string command, string detail, string errorType,
                                 string note = "")
        {
            try
            {
                string safeJob = string.IsNullOrWhiteSpace(jobName) ? "_NoJob"
                    : string.Join("_", jobName.Split(Path.GetInvalidFileNameChars()));
                string dir = Path.Combine(_baseDir, safeJob);
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, $"printer_{DateTime.Now:yyyyMMdd}.csv");
                bool exists = File.Exists(path);
                using (var sw = new StreamWriter(path, append: true, Encoding.UTF8))
                {
                    if (!exists)
                        sw.WriteLine("timestamp,printer_ip,direction,command,detail,error_type,ghi_chu");
                    sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{printerIp},{direction},{Csv.Escape(command)},{Csv.Escape(detail)},{errorType},{Csv.Escape(note)}");
                }
            }
            catch { }
        }

        internal static void LogRaw(string ip, string text, int length, string jobName = "")
        {
            try
            {
                string safeJob = string.IsNullOrWhiteSpace(jobName) ? "_NoJob"
                    : string.Join("_", jobName.Split(Path.GetInvalidFileNameChars()));
                string dir = Path.Combine(_baseDir, safeJob);
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, $"printer_raw_{DateTime.Now:yyyyMMdd}.csv");

                string rawEscaped = Csv.Escape(text);
                string warning = "";
                if (length > 0 && !string.IsNullOrEmpty(text))
                {
                    int etxCount = text.Count(c => c == '\x03');
                    int etxtagCount = text.Split(new[] { "<ETX>" }, StringSplitOptions.None).Length - 1;
                    if (etxCount > 1 || etxtagCount > 1)
                        warning = "STICKING";
                    else if (etxtagCount == 0 && etxCount == 0 && !text.EndsWith("\r\n"))
                        warning = "FRAGMENT";
                }

                bool exists = File.Exists(path);
                using (var sw = new StreamWriter(path, append: true, Encoding.UTF8))
                {
                    if (!exists)
                        sw.WriteLine("timestamp,ip,length,warning,data");
                    sw.Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{Csv.Escape(ip)},{length},{Csv.Escape(warning)},{rawEscaped}");
                    sw.Write("\r\n\r\n"); // blank line to separate packets
                }
            }
            catch { }
        }
    }
}
