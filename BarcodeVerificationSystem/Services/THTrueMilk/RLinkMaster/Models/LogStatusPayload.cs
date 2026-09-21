using System;

using System.Collections.Generic;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public class LogStatusPayload
    {
        public string line_id { get; set; } = "";
        public string rlink_name { get; set; } = "";
        public string job_name { get; set; } = "";
        public string batch { get; set; } = "";
        public string product_id { get; set; } = "";
        public string product_name { get; set; } = "";
        /// <summary>start | running | stop | completed</summary>
        public string status { get; set; } = "";
        /// <summary>Trạng thái R-Link (Idle/Running/Paused/Error...)</summary>
        public string rlink_status { get; set; } = "";
        /// <summary>Tài khoản vận hành đang đăng nhập</summary>
        public string operator_user { get; set; } = "";
        public int qr_used { get; set; }
        public int produced { get; set; }
        public string qr_code { get; set; } = "";
        public List<QrDetailItem> qr_detail { get; set; } = new List<QrDetailItem>();
        public string manufactured_date { get; set; } = "";
        public string expiry_date { get; set; } = "";
        public string last_printed_at { get; set; } = "";
        public string printer_last_product_manufactured_date { get; set; } = "";
        public DateTime timestamp { get; set; } = DateTime.Now;
    }
}