using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.THTrueMilk
{
    public class SyncDataParams
    {
        public enum SyncDataType
        {
            SentData,
            SAPSuccess,
            SaaSSuccess,
            SAPFailed,
            SaaSFailed,
            SentSuccess,
            PalletSuccess,
            CodeInCarton,
            SyncCodeInCarton
        }
        public SyncDataType DataType { get; set; }
        public int CodeIndex { get; set; }
        public object Data { get; set; } // For more generic data

        public SyncDataParams(SyncDataType name, int value = 0)
        {
            DataType = name;
            CodeIndex = value; // Default value
        }


    }
}
