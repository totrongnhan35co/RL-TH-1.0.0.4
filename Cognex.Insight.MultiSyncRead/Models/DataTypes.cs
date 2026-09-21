using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.Insight.MultiSyncRead.Models
{
    public class DataTypes
    {
        public enum OperationStatus
        {
            Running = 1,
            Processing = 3,
            Stopped = 2
        }
    }
}
