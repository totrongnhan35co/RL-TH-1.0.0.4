using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request
{
    public class RequestPrintedAmount
    {
        public string plant { get; set; }
        public string process_order { get; set; }
        public string material_number { get; set; }
        public string username { get; set; }
    }
}
