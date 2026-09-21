using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils.ExportData.models
{
    [Serializable]
    public class ExportSettings
    {
        public string FilterStatus { get; set; } // Added for FilterChecked.filterStatus
        public string FilterDevice { get; set; } // Added for FilterChecked.filterDevice
        public string FilterStatusDb { get; set; }
        public string FilterDeviceDb { get; set; }
        public List<string> StatusLabelGl { get; set; }
        public List<string> StatusLabelResGl { get; set; }
        public string DeviceCheckedResult { get; set; }
        public string SampleFilter { get; set; }
        public OtherHeader CustomHeader { get; set; }
        public CheckedExportHeader CheckedExportHeader { get; set; }
        public CheckedHeaderList CheckedHeaderList { get; set; }
        public CheckedExportResult CheckedExportResult { get; set; }
        public List<(bool IsChecked, string Text, string OrgText)> Results { get; set; }
        public CustomStatusValue CustomStatus { get; set; }
        public ExportMode ExportMode { get; set; }
        public string TemplateName { get; set; }
    }

    public class CheckedHeaderList
    {
        public bool isStatusChecked = true;
        public bool isDeviceChecked = true;
        public bool isPositionChecked = true;
        public bool isCheckingCodeChecked = true;
        public bool isVerifyDateChecked = true;
    }

    public enum ExportMode {
        ExportAll,
        ExportDatabase,
        ExportResult
    }

    public class ExportAllStatus
    {
       public static string PrintedVerified = "Printed-Verified";
       public static string UnprintedVerified = "Unprinted-Verified";
       public static string PrintedDuplicate = "Printed-Duplicate";
       public static string PrintedUnverified = "Printed-Unverified";
       public static string UnprintedUnverified = "Unprinted-Unverified";
       public static string UnprintedChecked = "Unprinted-Checked";
    }
   
    public class FilterChecked
    {
       
        public string filterStatus { get; set; }
        public string filterDevice { get; set; }
        public string filterStatusDb { get; set; }
        public string filterDeviceDb { get; set; }

        public FilterChecked(string sts, string dev, string filterStatusDb, string filterDeviceDb)
        {
            filterStatus = sts;
            filterDevice = dev;
            this.filterStatusDb = filterStatusDb;
            this.filterDeviceDb = filterDeviceDb;
        }


    }
    public class OtherHeader
    {
        public string statusHeader { get; set; }
        public string deviceHeader { get; set; }
        public string verifydateHeader { get; set; }
        public string positionHeader { get; set; }
        public string numCheckHeader { get; set; }
    }

    public class CheckedExportHeader
    {
        public string IndexHeader { get; set; } = "Index";
        public string DataHeader { get; set; }
        public string DateVerifyHeader { get; set; }
        public string IsPositionHeader { get; set; }
        public string DeviceNameHeader { get; set; }
        public string ResultHeader { get; set; }
        public string SampledHeader { get; set; }
    }

    public class CheckedExportResult
    {
        public string Valid { get; set; }
        public string Invalided { get; set; }
        public string Duplicated { get; set; }
        public string Null { get; set; }
        public string Missed { get; set; }
      
    }
}
