using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Models
{
    public class StorageUpdate
    {
        public int Id { get; set; }
        public string PrintedDate { get; set; }
        public string SaaSStatus { get; set; }
        public string SAPStatus { get; set; }
        public string SaaSError { get; set; }
        public string SAPError { get; set; }

        // Extra parameter only for verify
        public string VerifiedDate { get; set; }
        public string VerifiedStatus { get; set; }
    }

}
