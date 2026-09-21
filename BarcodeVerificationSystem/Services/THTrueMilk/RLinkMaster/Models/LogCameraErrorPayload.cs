using System;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    /// <summary>
    /// Payload cho 1 dòng log lỗi camera (mỗi sản phẩm bị lỗi -> 1 record).
    /// </summary>
    public class LogCameraErrorPayload
    {
        public string line_id { get; set; } = "";
        public string rlink_name { get; set; } = "";
        public string job_name { get; set; } = "";
        public string batch { get; set; } = "";
        public string product_id { get; set; } = "";
        public string product_name { get; set; } = "";
        public string rlink_status { get; set; } = "";
        public string operator_user { get; set; } = "";

        /// <summary>Mã QR camera đọc được (rỗng nếu NotRead).</summary>
        public string qr_code { get; set; } = "";
        /// <summary>NSX từ camera (dd MM yy).</summary>
        public string error_manufactured_date { get; set; } = "";
        /// <summary>HSD từ camera (dd MM yy).</summary>
        public string error_expiry_date { get; set; } = "";
        /// <summary>NotRead | NotMatch | Duplicated | Invalid | Format | Other</summary>
        public string error_type { get; set; } = "";
        /// <summary>Đường dẫn ảnh lỗi.</summary>
        public string image_path { get; set; } = "";
        /// <summary>Chuỗi dữ liệu Camera raw.</summary>
        public string error_frame_info { get; set; } = "";

        public DateTime timestamp { get; set; } = DateTime.UtcNow;
    }
}