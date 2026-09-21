using System.Runtime.Serialization;

namespace BarcodeVerificationSystem
{
    [DataContract]
    public class GetInfoDataResponseModel
    {
        [DataMember(Name = "model")]
        public string Model { get; set; }
        [DataMember(Name = "headNumber")]
        public string HeadNumber { get; set; }
        [DataMember(Name = "software")]
        public string Software { get; set; }
        [DataMember(Name = "buildDate")]
        public string BuildDate { get; set; }
        [DataMember(Name = "buildTime")]
        public string BuildTime { get; set; }
        [DataMember(Name = "activeExtFunc")]
        public string ActiveExtFunc { get; set; }
        [DataMember(Name = "kernelVersion")]
        public string KernelVersion { get; set; }
        [DataMember(Name = "firmware")]
        public string Firmware { get; set; }
        [DataMember(Name = "hardware")]
        public string Hardware { get; set; }
        [DataMember(Name = "serialNumber")]
        public string SerialNumber { get; set; }
        [DataMember(Name = "uuid")]
        public string Uuid { get; set; }
        [DataMember(Name = "dateActive")]
        public string DateActive { get; set; }
        [DataMember(Name = "printStatus")]
        public int PrintStatus { get; set; }
        [DataMember(Name = "printerName")]
        public string PrinterName { get; set; }
    }
}
