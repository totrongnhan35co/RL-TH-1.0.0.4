using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ResponseModel
{
    public class QrBankResponseLogin
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }
}