using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response
{
    public class ResponseListRePrint
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public List<ProcessOrder> process_orders { get; set; }
    
    }
    public class ProcessOrder
    {
        public string process_order { get; set; }
        public string material_number { get; set; }
        public string material_name { get; set; }
        public List<QrCode> qrcodes { get; set; } = new List<QrCode>();
    }

}
