using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request
{
    public class RequestChecked
    {
        public int index_qr_code { get; set; }
        public string qr_code { get; set; }
        public string process_order { get; set; }
        public string material_doc { get; set; }
        public string material_number { get; set; }
        public string check_date { get; set; }
        public string status { get; set; }
        public string print_type { get; set; }
        public string batch { get; set; }
        public DateTime mauf_date { get; set; }
        public DateTime expired_date { get; set; }
        public string device_name { get; set; } = Shared.Settings.RLinkName;
        public string username { get; set; } = CurrentUser.UserCode;
        public string plant { get; set; } = Shared.Settings.FactoryCode;
        public string resource_code { get; set; } = Shared.Settings.LineId;
        public string resource_name { get; set; } = Shared.Settings.LineName;
        public string job_name { get; set; } = Shared.CurrentJob.FileName;
    }
}
