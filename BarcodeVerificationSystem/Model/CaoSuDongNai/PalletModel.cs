using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai
{
    public class PalletModel
    {
        public string QrCode { get; set; } = "";
        public bool IsSent { get; set; } = false;
    }
}
