using System;
using System.Text;

namespace BarcodeVerificationSystem.Utils
{
    /// <summary>
    ///集中管理 transform logic cho Woka data.
    /// Dữ liệu gốc (raw \x1D) được giữ nguyên từ import đến so sánh.
    /// Chỉ transform khi gửi API hoặc hiển thị.
    /// </summary>
    public static class WokaDataNormalizer
    {
        /// <summary>
        /// Gửi API WMS: chỉ base64 encode raw data.
        /// File import đã dùng \x1D thật → giữ nguyên, chỉ base64.
        /// </summary>
        public static string ToWokaApiFormat(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        }

        /// <summary>
        /// Gửi printer: giữ nguyên raw data.
        /// File import đã dùng \x1D thật → giữ nguyên.
        /// </summary>
        public static string ToPrinterFormat(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            return raw;
        }
    }
}
