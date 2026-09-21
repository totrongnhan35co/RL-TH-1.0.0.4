using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Request
{
    public class RequestUpdateCodeCheck
    {
        public string production_batch_code { get; set; }
        public string product_code { get; set; }
        public string weight { get; set; }
        public List<QrCodeCheck> qr_list { get; set; }
    }

    public class QrCodeCheck
    {
        public string qrcode_value { get; set; }
        public string status { get; set; }
        public string execute_date { get; set; }
    }
}