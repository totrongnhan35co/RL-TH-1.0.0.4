using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Woka.Request
{
    public class RequestCargo
    {
        public string cargoCode { get; set; }
        public List<string> qrCodeList { get; set; }
        public string note { get; set; }
    }
}
