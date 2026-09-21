using System.Collections.Generic;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public class SyncReportPayload
    {
        public string line_id { get; set; } = "";
        public string rlink_name { get; set; } = "";
        public int pending_in { get; set; }
        public int pending_camera { get; set; }
        public int pending_error { get; set; }
        /// <summary>Chi tiết theo từng job — gửi kèm lên server.</summary>
        public List<SyncJobDetail> job_details { get; set; } = new List<SyncJobDetail>();
    }

    public class SyncJobDetail
    {
        public string job_name { get; set; } = "";
        public string batch { get; set; } = "";
        public int pending_in { get; set; }
        public int pending_camera { get; set; }
        public int pending_error { get; set; }
    }
}