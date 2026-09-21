using BarcodeVerificationSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils.ExportData
{
    public class ExportDataModel
    {
        public List<string[]> CheckedResultCodeList { get; set; }
        public int IndexDevice { get; set; }
        public int IndexResultData { get; set; }
        public int IndexDateTime { get; set; }
        public int IndexResult { get; set; }
        public string[] DatabaseColumn { get; set; }
        public int TotalCode { get; set; }
        public List<string[]> PrintedCodeObtainFromFile { get; set; }
        public JobModel SelectedJob { get; set; }
        public string compareString { get; set; }



    }
}
