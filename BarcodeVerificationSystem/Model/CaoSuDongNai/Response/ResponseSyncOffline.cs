using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.Response
{
    public class ResponseSyncOffline
    {
        public bool success { get; set; }
        public string message { get; set; }
        public Result result { get; set; }
    }
    public class Result
    {
        public int checked_count { get; set; }
        public int printed_count { get; set; }
        public int qr_with_pallet_count { get; set; }
    }
}
