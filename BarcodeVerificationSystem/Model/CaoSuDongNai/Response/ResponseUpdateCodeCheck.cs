using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Response
{
    public class ResponseUpdateCodeCheck
    {
        public bool success { get; set; }
        public string message { get; set; }
        public ResponseUpdateCodeCheckData data { get; set; }
        public DateTime timestamp { get; set; }
        public int status { get; set; }
    }

    public class ResponseUpdateCodeCheckData
    {
        public Summary summary { get; set; }
        public Results results { get; set; }
        public string updated_user { get; set; }
        public DateTime processed_at { get; set; }
    }

    public class Detail
    {
        public string qrcode_value { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class NotPass
    {
        public int total { get; set; }
        public int updated { get; set; }
        public int failed { get; set; }
        public List<Detail> details { get; set; }
    }

    public class Pass
    {
        public int total { get; set; }
        public int updated { get; set; }
        public int failed { get; set; }
        public List<Detail> details { get; set; }
    }

    public class Results
    {
        public Pass pass { get; set; }
        public NotPass notPass { get; set; }
    }

    public class Summary
    {
        public int total_processed { get; set; }
        public int pass_updated { get; set; }
        public int pass_failed { get; set; }
        public int notpass_updated { get; set; }
        public int notpass_failed { get; set; }
    }
}
