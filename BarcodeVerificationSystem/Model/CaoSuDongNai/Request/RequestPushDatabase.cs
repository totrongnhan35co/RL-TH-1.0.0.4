using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Request
{
    public class RequestPushDatabase
    {
        public string production_batch_code { get; set; }
        public string product_code { get; set; }
        public double weight { get; set; }
        public int printed_number { get; set; }
        public string execute_date { get; set; }
        public List<DatabaseQrCode> qr_code { get; set; }
    }

    public class DatabaseQrCode
    {
        public string qrcode_value { get; set; }
        public int index_in_lot { get; set; }
        public string execute_date { get; set; }
    }
}
