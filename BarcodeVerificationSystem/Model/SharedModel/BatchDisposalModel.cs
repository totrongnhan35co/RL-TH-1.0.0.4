using System;

namespace BarcodeVerificationSystem.Model
{
    public class BatchDisposalModel
    {
        public string Batch { get; set; }
        public int DisposalCount { get; set; }

        public BatchDisposalModel()
        {
            Batch = string.Empty;
            DisposalCount = 0;
        }

        public BatchDisposalModel(string batch, int disposalCount)
        {
            Batch = batch;
            DisposalCount = disposalCount;
        }
    }
}





