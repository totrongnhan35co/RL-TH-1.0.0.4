using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response
{
    public class ResponseCurrentPrintedCodeInfo
    {
        public bool is_success { get; set; } // Có vượt ngưỡng hay không
        public bool is_exceed { get; set; } // Có vượt ngưỡng hay không
        public int amount { get; set; } // Số lượng mã đã in
        public string message { get; set; } // Số lượng mã đã in
        public string error_code { get; set; } // Số lượng mã đã in

    }
}
