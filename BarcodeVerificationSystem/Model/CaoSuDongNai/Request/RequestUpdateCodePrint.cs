using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Request
{
    public class RequestUpdateCodePrint
    {

        public List<QrCodePrint> qr_list { get; set; }

    }

    public class QrCodePrint
    {
        public string qrcode_value { get; set; }
        public string execute_date { get; set; }
    }
}
