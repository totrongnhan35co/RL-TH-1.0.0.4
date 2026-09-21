using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues
{
    internal static class VerifyingValues
    {
        private static readonly List<string> Columns = new List<string>
            {
                "Index",
                "UniqueCode",
                "VerifiedStatus",
                "VerifiedDate",
                "SaaSStatus",
                "SAPStatus",
                "SaaSError",
                "SAPError",
                "SentStatus"
            };

        public static int Index => Columns.IndexOf("Index");
        public static int Code => Columns.IndexOf("UniqueCode");
        public static int VerifiedStatus => Columns.IndexOf("VerifiedStatus");
        public static int VerifiedDate => Columns.IndexOf("VerifiedDate");
        public static int SaaSStatus => Columns.IndexOf("SaaSStatus");
        public static int SAPStatus => Columns.IndexOf("SAPStatus");
        public static int SaaSError => Columns.IndexOf("SaaSError");
        public static int SAPError => Columns.IndexOf("SAPError");
        public static int SentStatus => Columns.IndexOf("SentStatus");
    }

}
