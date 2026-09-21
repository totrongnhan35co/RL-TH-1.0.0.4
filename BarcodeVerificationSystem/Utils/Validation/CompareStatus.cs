using System.Collections.Generic;

namespace BarcodeVerificationSystem.Controller
{
    public class CompareStatus
    {
        public CompareStatus()
        {
            Index = -1;
            Status = false;
            DuplicateId = new List<int>();
        }

        public CompareStatus(int index, bool v)
        {
            Index = index;
            Status = v;
            DuplicateId = new List<int>();
        }

        public int Index { get; set; }

        public bool Status { get; set; }

        public List<int> DuplicateId { get; set; }

    }
}
