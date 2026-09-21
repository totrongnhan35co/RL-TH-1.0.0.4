using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response
{
    public class ResponsePrintedAmount
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public int amount_printed { get; set; }
        public int amount_checked { get; set; }
        public bool is_exceed { get; set; }
        public string error_code { get; set; }
    }
}
