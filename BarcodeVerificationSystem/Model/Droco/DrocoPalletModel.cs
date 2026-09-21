using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Droco
{
    public class DrocoPalletModel
    {
        public string QrCode { get; set; } = "";
        public bool IsSent { get; set; } = false;
        public bool sentToPrinter { get; set; } = false;
        /// <summary>
        /// Danh sách QR Thùng trong pallet này
        /// </summary>
        public List<string> CartonCodes { get; set; } = new List<string>();
    }

}