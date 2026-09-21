using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Woka
{
    public class CartonModel
    {
        public string QrCode { get; set; } = "";
        public bool IsSent { get; set; } = false;
        public bool IsCodeCartonSent { get; set; } = false;
        public bool sentToPrinter { get; set; } = false;

    }
}
