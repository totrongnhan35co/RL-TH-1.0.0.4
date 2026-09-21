using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public class LogCameraPayload
    {
        public string line_id { get; set; } = "";
        public string rlink_name { get; set; } = "";
        public string job_name { get; set; } = "";
        public string batch { get; set; } = "";
        public string product_id { get; set; } = "";
        public string product_name { get; set; } = "";
        public string status { get; set; } = "";
        public string rlink_status { get; set; } = "";
        public string operator_user { get; set; } = "";
        public int camera_ok { get; set; }
        public int camera_fail { get; set; }
        [JsonProperty("camera_total_check")]
        public int total_check => camera_ok + camera_fail;
        public string qr_code { get; set; } = "";
        public List<QrDetailItem> qr_detail { get; set; } = new List<QrDetailItem>();
        public string frame_info { get; set; } = "";
        public string camera_manufactured_date { get; set; } = "";
        public string camera_expiry_date { get; set; } = "";
        public string camera_last_packet_received_at { get; set; } = "";
        public string camera_last_product_manufactured_date { get; set; } = "";
        public DateTime timestamp { get; set; } = DateTime.UtcNow;
    }
}