using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ReceiveModel
{
    public class QrBankReceiveLogin
    {
        public string username { get; set; }
        public string password { get; set; }
        public string secret_key { get; set; }
    }
}