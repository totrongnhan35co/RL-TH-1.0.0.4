using System;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Response
{
    public class ResponseRefreshToken
    {
        public bool success { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
        public DateTime timestamp { get; set; }
        public int status { get; set; }
    }
}
