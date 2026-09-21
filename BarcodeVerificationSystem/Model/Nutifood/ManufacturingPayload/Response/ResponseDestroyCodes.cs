using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response
{
    public class ResponseDestroyCodes
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public int destroyed_qty { get; set; }
        public List<Qrcode> qrcodes { get; set; }
        public string error_code { get; set; }
        public bool is_success_sap { get; set; }
        public string message_sap { get; set; }
        public string error_code_sap { get; set; }
    }
    public class Qrcode
    {
        public string qr_code { get; set; }
        public string scan_date { get; set; }
        public string status_code { get; set; }
        public string message { get; set; }
    }
}
