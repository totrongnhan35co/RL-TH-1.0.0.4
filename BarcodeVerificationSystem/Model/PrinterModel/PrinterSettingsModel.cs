using System.Runtime.Serialization;

namespace BarcodeVerificationSystem.Model
{
    [DataContract]
    public class PrinterSettingsModel
    {
        [DataMember(Name = "podMode")]
        public int PodMode { get; set; } 
        [DataMember(Name = "podDataType")]
        public int PodDataType { get; set; }
        [DataMember(Name = "monitorResponse")]
        public int MonitorResponse { get; set; } 
        [DataMember(Name= "enablePOD")]
        public bool EnablePOD { get; set; }
        [DataMember(Name = "responsePODData")]
        public bool ResponsePODData { get; set; } 
        [DataMember(Name = "responsePODCommand")]
        public bool ResponsePODCommand { get; set; } 
        [DataMember(Name = "enableMonitor")]
        public bool EnableMonitor { get; set; } 
        [DataMember(Name = "isSupportHttpRequest")] 
        public bool IsSupportHttpRequest { get; set; }
    }
}
