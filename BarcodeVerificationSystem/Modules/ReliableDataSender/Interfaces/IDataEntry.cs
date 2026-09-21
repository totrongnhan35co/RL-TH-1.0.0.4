using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces
{
    public interface IDataEntry
    {
        int Id { get; set; }
        string Code { get; set; }
    }
}
