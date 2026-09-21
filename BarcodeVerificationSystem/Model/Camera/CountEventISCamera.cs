using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model
{
    public class CountEventISCamera
    {
        public CountEventISCamera(int index, int count)
        {
            Index = index;
            Count = count;
        }
        public int Index { get; private set; }
        public int Count { get; private set; }
    }
}
