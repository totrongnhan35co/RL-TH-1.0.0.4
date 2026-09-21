using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Droco
{
    public class DrocoCartonModel
    {
        public string QrCode { get; set; } = "";
        public bool IsSent { get; set; } = false;
        public bool IsCodeCartonSent { get; set; } = false;
        public bool sentToPrinter { get; set; } = false;

        /// <summary>
        /// Danh sách QR Hộp trong thùng này
        /// </summary>
        public List<string> BoxCodes { get; set; } = new List<string>();
    }

}
