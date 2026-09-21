using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request
{
    internal class RequestBasedData
    {
        public string wms_number { get; set; }
        public string username { get; set; } = CurrentUser.UserCode;
        public string plant { get; set; } = Shared.Settings.FactoryCode;
        public string job_name { get; set; }
        public string material_number { get; set; }
        public string wave_key { get; set; }
        public string first_index { get; set; }
        public string last_index { get; set; }
        public string device_name { get; set; } = Shared.Settings.RLinkName;
        public List<QrCode> qrcodes { get; set; } = new List<QrCode>();
    }

}
