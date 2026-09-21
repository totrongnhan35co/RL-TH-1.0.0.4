using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Droco
{
    public class BoxModel
    {
        public string QrCode { get; set; } = "";
        public bool IsSent { get; set; } = false;
        public bool sentToPrinter { get; set; } = false;

        /// <summary>
        /// Danh sách mã GS1 sản phẩm trong hộp này
        /// </summary>
        public List<string> ProductCodes { get; set; } = new List<string>();
    }
}