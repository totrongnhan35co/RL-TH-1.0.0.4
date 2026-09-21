using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request
{
    public class RequestDestroyCodes
    {
        public List<Qrcode> qrcodes { get; set; }
        public string username { get; set; } = CurrentUser.UserCode;
        public string notes { get; set; }
        public string plant { get; set; } = Shared.Settings.FactoryCode;
        public string device_name { get; set; } = Shared.Settings.RLinkName;
        public string cancellation_schedule = Shared.CurrentJob.FileName;
    }
    public class Qrcode
    {
        public string qr_code { get; set; }
        public DateTime scan_date { get; set; }
        public string notes { get; set; }

    }
}
