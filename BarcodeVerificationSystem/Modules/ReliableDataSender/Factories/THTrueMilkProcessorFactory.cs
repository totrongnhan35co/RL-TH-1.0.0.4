using BarcodeVerificationSystem.Modules.ReliableDataSender.Core;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using System.Collections.Concurrent;
using THTrueMilkSvc = BarcodeVerificationSystem.Modules.ReliableDataSender.Services.THTrueMilk;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Factories
{
    public class THTrueMilkProcessorFactory
    {
        public static PrintingQueueProcessor CreatePrintingProcessor(
            string filePath, string endpoint, string databasePath)
        {
            var queue       = new BlockingCollection<PrintingDataEntry>();
            var fileStorage = new THTrueMilkSvc.PrintingStorageService(filePath, databasePath);
            var sender      = new THTrueMilkSvc.PrintingSenderService(queue, fileStorage, endpoint);
            return new PrintingQueueProcessor(queue, fileStorage, sender);
        }

        public static VerificationQueueProcessor CreateVerificationProcessor(
            string filePath, string endpoint, string databasePath)
        {
            var queue       = new BlockingCollection<VerificationDataEntry>();
            var fileStorage = new THTrueMilkSvc.VerificationStorageService(filePath, databasePath);
            var sender      = new THTrueMilkSvc.VerificationSenderService(queue, fileStorage, endpoint);
            return new VerificationQueueProcessor(queue, fileStorage, sender);
        }
    }
}