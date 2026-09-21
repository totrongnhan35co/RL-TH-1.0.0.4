using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response
{
    public class ResponseDisposal
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public int destroyed_qty { get; set; }
        public List<Qrcode> qrcodes { get; set; }
    }
    public class Qrcode
    {
        public string qr_code { get; set; }
        //public DateTime? scan_date { get; set; }
        public string status_code { get; set; }
        public string message { get; set; }
    }
}
