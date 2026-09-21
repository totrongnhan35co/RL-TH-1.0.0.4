using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Response
{
    public class ResponseLogin
    {
        public bool success { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
        public DateTime timestamp { get; set; }
        public int status { get; set; }
    }

    public class Data
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public int expiresIn { get; set; }
        public string domainQRCode { get; set; }
    }

}
