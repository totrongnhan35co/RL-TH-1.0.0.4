using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils.ExportData
{
    public class ExportDataEventArgs : EventArgs
    {
        public object Data { get; }

        public ExportDataEventArgs(object data)
        {
            Data = data;
        }
    }
}
