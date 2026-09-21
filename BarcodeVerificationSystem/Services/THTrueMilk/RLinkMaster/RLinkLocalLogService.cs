using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using CommonVariable;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
{
    /// <summary>
    /// Lưu các loại dữ liệu R-Link ra file local tại C:\ProgramData\R-Link\RLinkLogs\
    /// Mỗi bản ghi = 1 file JSON (camera/print/monitor) hoặc ghi đè file duy nhất (config/products/qrcodes).
    /// </summary>
    public static class RLinkLocalLogService
    {
        // ── Camera ────────────────────────────────────────────────────────────
        public static void SaveCamera(LogCameraPayload payload)
        {
            try
            {
                string dir = CommVariables.PathRLinkLogCamera;
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, DateTime.Now.Ticks + "_camera.json");
                File.WriteAllText(file, JsonConvert.SerializeObject(payload, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RLinkLocalLog] SaveCamera lỗi: " + ex.Message);
            }
        }

        // ── Print ─────────────────────────────────────────────────────────────
        public static void SavePrint(LogPrintPayload payload)
        {
            try
            {
                string dir = CommVariables.PathRLinkLogPrint;
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, DateTime.Now.Ticks + "_print.json");
                File.WriteAllText(file, JsonConvert.SerializeObject(payload, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RLinkLocalLog] SavePrint lỗi: " + ex.Message);
            }
        }

        // ── Config / Settings ─────────────────────────────────────────────────
        /// <summary>
        /// Lưu RLinkSettings thành file config.json (ghi đè, luôn là bản mới nhất).
        /// </summary>
        public static void SaveConfig(RLinkSettings settings, string lineId = "")
        {
            try
            {
                string dir = CommVariables.PathRLinkConfig;
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, "config.json");
                var wrapper = new
                {
                    LineId = lineId,
                    SavedAt = DateTime.Now,
                    Settings = settings
                };
                File.WriteAllText(file, JsonConvert.SerializeObject(wrapper, Formatting.Indented));
                Console.WriteLine("[RLinkLocalLog] SaveConfig ✔ → " + file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RLinkLocalLog] SaveConfig lỗi: " + ex.Message);
            }
        }

        // ── QR Codes ──────────────────────────────────────────────────────────
        /// <summary>
        /// Lưu danh sách QR codes được cấp phát thành file theo jobName + timestamp.
        /// Thêm vào cuối file nếu đã tồn tại (append), nếu chưa thì tạo mới.
        /// </summary>
        public static void SaveQrCodes(IEnumerable<string> codes, string jobName)
        {
            try
            {
                string dir = CommVariables.PathRLinkQrCodes;
                Directory.CreateDirectory(dir);
                string safeName = string.IsNullOrWhiteSpace(jobName) ? "unknown" : jobName;
                string file = Path.Combine(dir, $"QR_{safeName}_{DateTime.Now:yyMMddHHmmss}.txt");
                File.WriteAllLines(file, codes, new System.Text.UTF8Encoding(true));
                Console.WriteLine("[RLinkLocalLog] SaveQrCodes ✔ → " + file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RLinkLocalLog] SaveQrCodes lỗi: " + ex.Message);
            }
        }

        // ── Products ──────────────────────────────────────────────────────────
        /// <summary>
        /// Lưu danh mục sản phẩm thành products.json (ghi đè, luôn là bản mới nhất).
        /// </summary>
        public static void SaveProducts(IEnumerable<ProductItem> products)
        {
            try
            {
                string dir = CommVariables.PathRLinkProducts;
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, "products.json");
                var wrapper = new
                {
                    SavedAt = DateTime.Now,
                    Products = products
                };
                File.WriteAllText(file, JsonConvert.SerializeObject(wrapper, Formatting.Indented));
                Console.WriteLine("[RLinkLocalLog] SaveProducts ✔ → " + file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RLinkLocalLog] SaveProducts lỗi: " + ex.Message);
            }
        }

        // ── Monitor snapshot ──────────────────────────────────────────────────
        /// <summary>
        /// Lưu snapshot trạng thái monitor, mỗi snapshot = 1 file JSON (không ghi đè).
        /// </summary>
        public static void SaveMonitor(MonitorPayload payload)
        {
            try
            {
                string dir = CommVariables.PathRLinkMonitor;
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, DateTime.Now.Ticks + "_monitor.json");
                File.WriteAllText(file, JsonConvert.SerializeObject(payload, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RLinkLocalLog] SaveMonitor lỗi: " + ex.Message);
            }
        }
    }
}