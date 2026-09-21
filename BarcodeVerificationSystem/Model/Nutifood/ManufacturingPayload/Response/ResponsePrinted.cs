using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response
{
    public class ResponsePrinted
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public string error_code { get; set; }
        public bool is_success_sap { get; set; }
        public string message_sap { get; set; }
        public string error_code_sap { get; set; }
    }
}
