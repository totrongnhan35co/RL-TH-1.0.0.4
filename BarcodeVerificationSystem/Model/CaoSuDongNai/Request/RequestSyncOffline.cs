using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Request
{
    public class RequestSyncOffline
    {
        public List<QrList> qr_list { get; set; }
    }
    public class QrList
    {
        public string production_batch_code { get; set; }
        public string product_code { get; set; }
        public string qr_pallet { get; set; }
        public int weight { get; set; }
        public int index_in_lot { get; set; }
        public string qrcode_value { get; set; }
        public string status { get; set; }
        public string created_time { get; set; }
        public string printed_at { get; set; }
        public string qr_checked_at { get; set; }
        public string mapped_at { get; set; }
    }
}
