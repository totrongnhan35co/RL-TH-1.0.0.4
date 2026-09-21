using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Request
{
    public class RequestUpdatePallet
    {
        public string qr_code_pallet { get; set; }
        public List<QrCode> qr_code { get; set; }
    }
    public class QrCode
    {
        public string qrcode_value { get; set; }
        public string execute_date { get; set; }
    }

}
