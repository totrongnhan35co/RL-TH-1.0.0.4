using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request
{
    public class RequestGeneratedCodes
    {
        public string batch { get; set; }
        public DateTime mauf_date { get; set; }
        public DateTime expired_date { get; set; }
        public string material_doc { get; set; }
        public string process_order { get; set; }
        public string job_name { get; set; }
        public string print_type { get; set; }
        public string material_number { get; set; }
        public List<Qrcode> qrcodes { get; set; }
        public int first_index { get; set; }
        public int last_index { get; set; }
        public string device_name { get; set; } = Shared.Settings.RLinkName;
        public string username { get; set; } = CurrentUser.UserCode;
        public string plant { get; set; } = Shared.Settings.FactoryCode;

        public class Qrcode
        {
            public string unique_code { get; set; }
            public string qr_code { get; set; }
            public int index_qr_code { get; set; }
            public DateTime create_date { get; set; }
        }

    }
}
