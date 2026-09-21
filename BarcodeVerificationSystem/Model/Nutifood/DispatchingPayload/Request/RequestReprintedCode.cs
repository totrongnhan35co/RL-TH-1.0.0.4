using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request
{
    internal class RequestReprintedCode
    {
        public int id { get; set; }
        public string job_name { get; set; }
        public string qr_code { get; set; }
        public string unique_code { get; set; }
        public string material { get; set; }
        public string resource_code { get; set; }
        public string resource_name { get; set; }
        public string username { get; set; }
        public string status { get; set; }
        public DateTime printed_date { get; set; }
    }
}
