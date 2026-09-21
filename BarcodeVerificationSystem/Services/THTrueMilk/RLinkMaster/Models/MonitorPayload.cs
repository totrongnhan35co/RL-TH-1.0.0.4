using Newtonsoft.Json;
using System;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    /// <summary>
    /// Payload cho POST /api/rlink/monitor — heartbeat định kỳ từ R-Link client.
    /// </summary>
    public class MonitorPayload
    {
        // ── Định danh ────────────────────────────────────────────────
        [JsonProperty("rLinkName")]
        public string RLinkName { get; set; }
        [JsonProperty("lineId")]
        public string LineId { get; set; }
        [JsonProperty("factoryCode")]
        public string FactoryCode { get; set; }
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }

        // ── Trạng thái thiết bị ──────────────────────────────────────
        /// <summary>idle | running | paused | error</summary>
        [JsonProperty("status")]
        public string Status { get; set; } = "idle";
        [JsonProperty("isPrinterConnected")]
        public bool IsPrinterConnected { get; set; }
        [JsonProperty("isCameraConnected")]
        public bool IsCameraConnected { get; set; }
        [JsonProperty("isPlcConnected")]
        public bool IsPlcConnected { get; set; }
        [JsonProperty("isDatabaseConnected")]
        public bool IsDatabaseConnected { get; set; }

        [JsonProperty("ipAddressPrinter")]
        public string IpAddressPrinter { get; set; }
        [JsonProperty("ipAddressCamera")]
        public string IpAddressCamera { get; set; }
        [JsonProperty("ipAddressPlc")]
        public string IpAddressPlc { get; set; }

        // ── Thông tin job / batch ────────────────────────────────────
        [JsonProperty("currentJobName")]
        public string CurrentJobName { get; set; }
        [JsonProperty("currentBatch")]
        public string CurrentBatch { get; set; }

        // ── Người vận hành ───────────────────────────────────────────
        [JsonProperty("operator_user")]
        public string OperatorUser { get; set; }
        [JsonIgnore]
        public string Operator { get => OperatorUser; set => OperatorUser = value; }

        // ── Tiến độ QR ───────────────────────────────────────────────
        [JsonProperty("totalQrAllocated")]
        public int TotalQrAllocated { get; set; }
        [JsonProperty("totalQrUsed")]
        public int TotalQrUsed { get; set; }
        [JsonProperty("totalQrFailed")]
        public int TotalQrFailed { get; set; }
        [JsonProperty("totalProduced")]
        public int TotalProduced { get; set; }

        // ── Ảnh lỗi ──────────────────────────────────────────────────
        [JsonProperty("image_error_folder")]
        public string ImageErrorFolder { get; set; }
        [JsonProperty("total_image_error_job")]
        public int TotalImageErrorJob { get; set; }
        [JsonProperty("total_image_error")]
        public int TotalImageError { get; set; }

        // ── Thống kê camera chi tiết ─────────────────────────────────
        [JsonProperty("stats")]
        public MonitorStats Stats { get; set; }

        // ── Thống kê sản xuất (QR + Date) ──────────────────────────
        [JsonProperty("stats2")]
        public MonitorStats2 Stats2 { get; set; }

        // ── Ảnh lỗi gần nhất ────────────────────────────────────────
        /// <summary>null nếu không có lỗi</summary>
        [JsonProperty("lastErrorImageBase64")]
        public string LastErrorImageBase64 { get; set; }

        // ── Phân loại sản phẩm ─────────────────────────────────────────
        [JsonProperty("productA")]
        public int ProductA { get; set; }
        [JsonProperty("productB")]
        public int ProductB { get; set; }
        [JsonProperty("productF")]
        public int ProductF { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = "00:00:00";
    }

    /// <summary>
    /// Thống kê chi tiết kiểm tra camera trong kỳ monitor.
    /// </summary>
    public class MonitorStats
    {
        [JsonProperty("totalCode")]
        public int TotalCode { get; set; }
        [JsonProperty("totalChecked")]
        public int TotalChecked { get; set; }
        [JsonProperty("checkPassed")]
        public int CheckPassed { get; set; }
        [JsonProperty("checkFailed")]
        public int CheckFailed { get; set; }
        [JsonProperty("numberPrinted")]
        public int NumberPrinted { get; set; }
    }

    /// <summary>
    /// Thống kê sản xuất: QR check + Date check.
    /// </summary>
    public class MonitorStats2
    {
        [JsonProperty("totalChecked")]
        public int TotalChecked { get; set; }
        [JsonProperty("qrCheckPassed")]
        public int QrCheckPassed { get; set; }
        [JsonProperty("qrCheckFailed")]
        public int QrCheckFailed { get; set; }
        [JsonProperty("dateCheckPassed")]
        public int DateCheckPassed { get; set; }
        [JsonProperty("dateCheckFailed")]
        public int DateCheckFailed { get; set; }
    }
}
