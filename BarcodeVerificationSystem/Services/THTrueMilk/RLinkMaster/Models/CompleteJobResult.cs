using System.Collections.Generic;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public class CompleteJobResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<string> AllocatedQrCodes { get; set; } = new List<string>(); // QR cấp phát mới
    }
}