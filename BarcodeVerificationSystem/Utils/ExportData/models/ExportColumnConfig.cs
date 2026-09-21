using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils.ExportData.models
{
    public  class ExportColumnConfig
    {
        public string OriginalName { get; set; }   // tên gốc trong _DatabaseColunms
        public string DisplayName { get; set; }   // tên custom
        public bool IsExport { get; set; }        // có xuất không
        public int Index { get; set; }            // vị trí trong record
    }
}
