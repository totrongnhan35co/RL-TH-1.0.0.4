using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response
{
    internal class ResponseConfirmCompletion
    {
        public bool isSuccessed { get; set; }
        public int status_code { get; set; }
        public string error_code { get; set; }
        public string message { get; set; }
    }
}
