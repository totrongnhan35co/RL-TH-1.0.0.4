using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces
{
    public interface ISenderService<T>
    {
        void Start();
        void Stop();
    }

}
