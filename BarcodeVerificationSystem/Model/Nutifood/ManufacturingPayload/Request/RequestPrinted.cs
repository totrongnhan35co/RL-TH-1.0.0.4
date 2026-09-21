using System;
using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;

namespace BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request
{
    public class RequestPrinted
    {
        public int index_qr_code { get; set; }
        public string qr_code { get; set; }
        public string unique_code { get; set; }
        public string process_order { get; set; }
        public string material_doc { get; set; }
        public string material_number { get; set; }
        public string print_type { get; set; }
        public string batch { get; set; }
        public DateTime mauf_date { get; set; }
        public DateTime expired_date { get; set; }
        public DateTime printed_date { get; set; }
        public string device_name { get; set; } = Shared.Settings.RLinkName;
        public string job_name { get; set; } = Shared.CurrentJob.FileName;
        public string status { get; set; } = "Printed";
        public string plant { get; set; } = Shared.Settings.FactoryCode;
        public string resource_code { get; set; } = Shared.Settings.LineId;
        public string resource_name { get; set; } = Shared.Settings.LineName;
        public string username { get; set; } = CurrentUser.UserCode;
    }
}
