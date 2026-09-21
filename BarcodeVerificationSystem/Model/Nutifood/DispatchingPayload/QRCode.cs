using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload
{
    public class QrCode
    {
        public int index_qr_code { get; set; }
        public string unique_code { get; set; }
        public string qr_code { get; set; }
        public string job_name { get; set; }
    }

}
