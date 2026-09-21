using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload
{
    public class RequestDisposal
    {
        public string qr_code { get; set; }
        public string unique_code { get; set; }
        public string notes { get; set; }
        public DateTime destroy_date { get; set; } 
        public DateTime sync_date { get; set; } = DateTime.Now;  // ?????
        public string username { get; set; } = CurrentUser.UserCode;
        public DateTime scan_date { get; set; } = DateTime.Now;
        public string status { get; set; } = "Dispose";

        //public int id { get; set; }
        //public int index_qr_code { get; set; }
        //public string qr_code { get; set; }
        //public string human_qr_code { get; set; }
        //public string username { get; set; }
        //public string notes { get; set; }
        //public string destroy_date { get; set; }
    }
}
