using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Woka.Response
{
    public class Payload
    {
        public string id { get; set; }
        public string sendId { get; set; }
        public List<string> qrIds { get; set; }
        public string purchaseOrderFactoryId { get; set; }
        public string packagingSessionId { get; set; }
        public int status { get; set; }
        public string note { get; set; }
        public int createdAt { get; set; }
        public int updatedAt { get; set; }
        public string metaData { get; set; }
        public string outboundId { get; set; }
        public string lotNumber { get; set; }
        public int productionDate { get; set; }
        public int expiryDate { get; set; }
    }

    public class ResponseCargo
    {
        public int code { get; set; }
        public string message { get; set; }
        public Payload payload { get; set; }
    }
}
