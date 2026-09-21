using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Models
{
    public class PrintingDataEntry : IDataEntry
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string UniqueCode { get; set; }
        public string PrintedDate { get; set; }
        public string PrintedStatus { get; set; }
        public string SaaSStatus { get; set; } 
        public string SAPStatus { get; set; }
        public string SaasError { get; set; }
        public string SAPError { get; set; }
        public string Status { get; set; } // "NotSent" or "Sent"
    }
}
