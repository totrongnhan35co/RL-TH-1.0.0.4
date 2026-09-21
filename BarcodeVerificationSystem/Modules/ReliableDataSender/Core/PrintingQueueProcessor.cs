using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Core
{
    public class PrintingQueueProcessor
    {
        private readonly BlockingCollection<PrintingDataEntry> _queue;
        private readonly IStorageService<PrintingDataEntry> _storage;
        private readonly ISenderService<PrintingDataEntry> _sender;

        public PrintingQueueProcessor(BlockingCollection<PrintingDataEntry> queue,  IStorageService<PrintingDataEntry> storage, ISenderService<PrintingDataEntry> sender)
        {
            _storage = storage;
            _sender = sender;
            _queue = queue;
        }

        public async void Start()
        {
            _sender.Start();
            await Task.Delay(200); 

            foreach (var entry in _storage.LoadUnsentEntries())
            {
                _queue.Add(entry);
            }

        }

        public void Enqueue(int id,string code, string humanCode)
        {

            var entry = new PrintingDataEntry
            {
                Id = id,
                Code = code,
                UniqueCode = humanCode,
                PrintedStatus = "Printed",
                PrintedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Status = "NotSent",
                SaaSStatus = string.Empty,
                SAPStatus = string.Empty,
                SaasError = string.Empty,
                SAPError = string.Empty
            };
            _storage.AppendEntry(entry);
            _queue.Add(entry);
            
        }

        public void Stop()
        {
            _sender.Stop();
        }
    }

}
