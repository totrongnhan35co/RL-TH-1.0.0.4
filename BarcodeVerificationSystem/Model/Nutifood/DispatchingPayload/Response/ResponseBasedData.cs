using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response
{
    internal class ResponseBasedData
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public int total { get; set; }
        public string error_code { get; set; }
    }
}
