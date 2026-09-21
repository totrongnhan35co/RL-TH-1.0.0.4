using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Models
{
    public class VerificationDataEntry : IDataEntry
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string UniqueCode { get; set; }
        public string VerifiedDate { get; set; }
        public string VerifiedStatus { get; set; }
        public string SaasStatus { get; set; }
        public string SAPStatus { get; set; }
        public string SaasError { get; set; }
        public string SAPError { get; set; }
        public string Status { get; set; }
        // === THÊM MỚI ===
        public string LotNumber { get; set; }
        public string ProductCode { get; set; }
        public string Weight { get; set; }
    }
}
