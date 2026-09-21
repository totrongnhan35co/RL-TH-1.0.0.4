using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.THTrueMilk.Payload.DispatchingPayload.Response
{
    public class ResponseCurrentPrintedCodeInfo
    {
        public bool is_success { get; set; } // Có vý?t ngý?ng hay không
        public bool is_exceed { get; set; } // Có vý?t ngý?ng hay không
        public int amount { get; set; } // S? lý?ng m? ð? in
        public string message { get; set; } // S? lý?ng m? ð? in
        public string error_code { get; set; } // S? lý?ng m? ð? in

    }
}
