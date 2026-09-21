using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ReceiveModel
{
    /// <summary>
    /// Payload nhận từ QrBank khi gửi dữ liệu tem mã.
    /// </summary>
    public class QrBankReceiveData
    {
        public List<string> qr_codes { get; set; }
        public string qr_code { get; set; }
        public string factory_code { get; set; }
        public string line_id { get; set; }
        public string batch { get; set; }
        public string line_name { get; set; }
        public string line_ip { get; set; }
        public string job_name { get; set; }
        public string sender { get; set; }
        public string product_gtin { get; set; }
    }
}
