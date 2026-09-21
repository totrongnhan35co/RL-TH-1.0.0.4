using System.Runtime.Serialization;

namespace BarcodeVerificationSystem
{
    [DataContract]
    public class ResponseGetInfoModel
    {
        [DataMember(Name = "success")]
        public string Success { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
        [DataMember(Name = "data")]
        public GetInfoDataResponseModel data { get; set; }

    }
}
