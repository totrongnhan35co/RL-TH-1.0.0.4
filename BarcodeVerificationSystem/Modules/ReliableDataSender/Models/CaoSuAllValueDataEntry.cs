using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Models
{
    public class CaoSuAllValueDataEntry 
    {
        public int Id { get; set; }
        public string QRcode { get; set; }
        public string Status { get; set; } // created, printed, valid, mapped
        public string ProductCode { get; set; }
        public string ProductionBatchCode { get; set; }
        public string Weight { get; set; }
        public string CreatedTime { get; set; }
        public string PrintedTime { get; set; }
        public string CheckedTime { get; set; }
        public string MappedTime { get; set; }
        public string QRCodePallet { get; set; }
        public string StorageStatus { get; set; } // "NotSent" or "Sent"
    }
}

