using System;

namespace BarcodeVerificationSystem.Model
{
    public class BatchRecheckModel
    {
        public string Batch { get; set; }
        public int RecheckCount { get; set; }

        public BatchRecheckModel()
        {
            Batch = string.Empty;
            RecheckCount = 0;
        }

        public BatchRecheckModel(string batch, int recheckCount)
        {
            Batch = batch;
            RecheckCount = recheckCount;
        }
    }
}





