using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request
{
    public class RequestListDisposal
    {
        public string username { get; set; } = CurrentUser.UserCode;
        public string plant { get; set; } = Shared.Settings.FactoryCode;
        public string device_name { get; set; } = Shared.Settings.RLinkName;
        public List<RequestDisposal> qrcodes { get; set; } = new List<RequestDisposal>();
    }
}
