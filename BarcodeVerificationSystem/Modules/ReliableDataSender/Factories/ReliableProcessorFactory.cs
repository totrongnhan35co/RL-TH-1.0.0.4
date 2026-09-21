using BarcodeVerificationSystem.Modules.ReliableDataSender.Core;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Woka;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Factories
{
    public class ReliableProcessorFactory
    {
        public static VerificationQueueProcessor CreateWokaCargoStorageProcessor(string filePath, string endpoint, string databasePath)
        {
            var queue = new BlockingCollection<VerificationDataEntry>();
            var fileStorage = new WokaStorageService(filePath, databasePath);
            var senderWorker = new WokaCargoStorage(queue, fileStorage, endpoint);
            var processor = new VerificationQueueProcessor(queue, fileStorage, senderWorker);
            return processor;
        }

        public static VerificationQueueProcessor CreateWokaCargoSenderProcessor(string filePath, string endpoint, string databasePath)
        {
            var queue = new BlockingCollection<VerificationDataEntry>();
            var fileStorage = new WokaStorageService(filePath, databasePath);
            var senderWorker = new WokaCargoSender(queue, fileStorage, endpoint);
            var processor = new VerificationQueueProcessor(queue, fileStorage, senderWorker);
            return processor;
        }

        public static VerificationQueueProcessor CreateDrocoCargoStorageProcessor(string filePath, string endpoint, string databasePath)
        {
            var queue = new BlockingCollection<VerificationDataEntry>();
            var fileStorage = new DrocoStorageService(filePath, databasePath);
            var senderWorker = new DrocoCargoStorage(queue, fileStorage, endpoint);
            var processor = new VerificationQueueProcessor(queue, fileStorage, senderWorker);
            return processor;
        }

        public static VerificationQueueProcessor CreateDrocoCargoSenderProcessor(string filePath, string endpoint, string databasePath)
        {
            var queue = new BlockingCollection<VerificationDataEntry>();
            var fileStorage = new DrocoStorageService(filePath, databasePath);
            var senderWorker = new DrocoCargoSender(queue, fileStorage, endpoint);
            var processor = new VerificationQueueProcessor(queue, fileStorage, senderWorker);
            return processor;
        }

        public static PrintingQueueProcessor CreateCaoSuPrintingProcessor(string filePath, string endpoint, string databasePath)
        {
            var queue = new BlockingCollection<PrintingDataEntry>();
            var fileStorage = new PrintingStorageService(filePath, databasePath);
            var senderWorker = new CaoSuPrintingSender(queue, fileStorage, endpoint);
            var processor = new PrintingQueueProcessor(queue, fileStorage, senderWorker);
            return processor;
        }

        public static VerificationQueueProcessor CreateCaoSuVerificationProcessor(string filePath, string endpoint, string databasePath)
        {
            var queue = new BlockingCollection<VerificationDataEntry>();
            var fileStorage = new VerificationStorageService(filePath, databasePath);
            var senderWorker = new CaoSuVerificationSender(queue, fileStorage, endpoint);
            var processor = new VerificationQueueProcessor(queue, fileStorage, senderWorker);
            return processor;
        }

        public static VerificationQueueProcessor CreateCaoSuPalletProcessor(string filePath, string endpoint, string databasePath)
        {
            var queue = new BlockingCollection<VerificationDataEntry>();
            var fileStorage = new VerificationStorageService(filePath, databasePath);
            var senderWorker = new CaoSuPalletSender(queue, fileStorage, endpoint);
            var processor = new VerificationQueueProcessor(queue, fileStorage, senderWorker);
            return processor;
        }

        public static PrintingQueueProcessor CreatePrintingProcessor(string filePath, string endpoint, string databasePath)
        {
            var queue = new BlockingCollection<PrintingDataEntry>();
            var fileStorage = new PrintingStorageService(filePath, databasePath);
            var senderWorker = new PrintingSenderService(queue, fileStorage, endpoint);
            var processor = new PrintingQueueProcessor(queue, fileStorage, senderWorker);
            return processor;
        }

        public static VerificationQueueProcessor CreateVerificationProcessor(string filePath, string endpoint, string databasePath)
        {
            var queue = new BlockingCollection<VerificationDataEntry>();
            var fileStorage = new VerificationStorageService(filePath, databasePath);
            var senderWorker = new VerificationSenderService(queue, fileStorage, endpoint);
            var processor = new VerificationQueueProcessor(queue, fileStorage, senderWorker);
            return processor;
        }
    }
}