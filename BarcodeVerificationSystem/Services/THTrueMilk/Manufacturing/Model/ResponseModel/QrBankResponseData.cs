using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ResponseModel
{
    public class QrBankResponseData
    {
        public bool is_success { get; set; }
        public bool duplicate { get; set; }
        public int total_inserted { get; set; }
        public int total_duplicate { get; set; }
        public int total_empty { get; set; }
        public int total_error { get; set; }
        public List<string> duplicate_list { get; set; }
        public List<string> empty_list { get; set; }
        public string message { get; set; }
        public string saved_at { get; set; }
    }
}