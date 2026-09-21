using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload
{
    internal class PermissionDatum
    {
        public string _id { get; set; }
        public string ma_chuc_nang { get; set; }
        public string ten_chuc_nang { get; set; }
        public object ghi_chu { get; set; }
    }

    internal class PermissionPayload
    {
        public string msg { get; set; }
        public bool success { get; set; }
        public int total { get; set; }
        public int count { get; set; }
        public List<object> DisplayFields { get; set; }
        public List<PermissionDatum> data { get; set; }
    }

}
