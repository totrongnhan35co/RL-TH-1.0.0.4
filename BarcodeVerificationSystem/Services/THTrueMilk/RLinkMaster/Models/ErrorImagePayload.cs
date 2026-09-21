using System;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public class ErrorImagePayload
    {
        public string line_id { get; set; }
        public string job_name { get; set; }
        public string batch { get; set; }
        public string qr_code { get; set; }
        /// <summary>Ảnh lỗi encode Base64</summary>
        public string image_base64 { get; set; }
        public DateTime timestamp { get; set; } = DateTime.Now;
    }
}