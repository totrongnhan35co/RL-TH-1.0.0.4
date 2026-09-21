using System.Runtime.Serialization;

namespace BarcodeVerificationSystem.Model
{
    [DataContract]
    public class PrinterSettingsResponseModel
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; }
        [DataMember(Name = "data")]
        public PrinterSettingsModel data { get; set; }
        [DataMember(Name ="code")]
        public string Code { get; set; }
    }
}
