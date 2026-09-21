using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request
{
    internal class RequestListRePrint
    {
        public string username { get; set; } = CurrentUser.UserCode;
        public string plant { get; set; } = Shared.Settings.FactoryCode;
   
        public List<QrCode> qrCodes { get; set; } = new List<QrCode>();
    }
}
