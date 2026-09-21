using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using BarcodeVerificationSystem.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.THTrueMilk
{
    public class VerificationStorageService : IStorageService<VerificationDataEntry>
    {
        private readonly string _filePath;
        private readonly object _fileLock = new object();
        private readonly string _unsentStatus = "NotSent";
        private readonly string _sentStatus = "Sent";
        private readonly string _databasePath;

        public VerificationStorageService(string filePath, string databasePath)
        {
            _filePath = filePath;
            _databasePath = databasePath;
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose(); // Dispose to release the handle immediately
            }

        }

        public void AppendEntry(VerificationDataEntry entry)  
        {
            lock (_fileLock)
            {
                var entryLine = $"{entry.Id},{entry.Code},{entry.VerifiedStatus},{entry.VerifiedDate},{entry.SaasStatus},{entry.SAPStatus},{entry.SaasError},{entry.SAPError},{_unsentStatus}";
                File.AppendAllLines(_filePath, new[] { entryLine }, Encoding.UTF8);
            }
        }

        public List<VerificationDataEntry> LoadUnsentEntries()
        {
            try
            {
                lock (_fileLock)
                {
                    return File.ReadAllLines(_filePath)
                    .Select((line, index) =>
                    {
                        var parts = line.Split(',');
                        return new VerificationDataEntry
                        {
                            Id = int.Parse(parts[VerifyingValues.Index]), // parts[0]}
                            Code = parts[VerifyingValues.Code],
                            VerifiedStatus = parts[VerifyingValues.VerifiedStatus],
                            VerifiedDate = parts[VerifyingValues.VerifiedDate],
                            SaasStatus = parts[VerifyingValues.SaaSStatus],
                            SAPStatus = parts[VerifyingValues.SAPStatus],
                            SaasError = parts[VerifyingValues.SaaSError],
                            SAPError = parts[VerifyingValues.SAPError],
                            Status = parts[VerifyingValues.SentStatus],
                        };
                    })
                    .Where(e => e.Status == _unsentStatus)
                    .ToList();
                }
            }
            catch (System.Exception ex)
            {
                ProjectLogger.WriteError("Error occurred in LoadUnsentEntries" + ex.Message);
                return default;
            }
        }

        public void MarkAsFailed(StorageUpdate storageUpdate)
        {
            try
            {
                lock (_fileLock)
                {
                    FileHelper.UpdateVerifyingEntry(_filePath, storageUpdate, _unsentStatus);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in MarkAsFailed: " + ex.Message);
            }
        }

        public void MarkAsSent(StorageUpdate storageUpdate)
        {
            try
            {
                lock (_fileLock)
                {
                    FileHelper.UpdateVerifyingEntry(_filePath, storageUpdate, _sentStatus);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in MarkAsSent: " + ex.Message);
            }
        }

        public void MarkCodeWithPalletAsSent(StorageUpdate storageUpdate)
        {
            try
            {
                lock (_fileLock)
                {
                    FileHelper.UpdateCodeWithPalletVerifyingEntry(_filePath, storageUpdate, _sentStatus);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in MarkAsSent: " + ex.Message);
            }
        }

        public void MarkCodeWithPalletAsFailed(StorageUpdate storageUpdate)
        {
            try
            {
                lock (_fileLock)
                {
                    FileHelper.UpdateCodeWithPalletVerifyingEntry(_filePath, storageUpdate, _unsentStatus);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in MarkAsSent: " + ex.Message);
            }
        }

        public void MarkCodeWithCartonAsSent(StorageUpdate storageUpdate)
        {
            try
            {
                lock (_fileLock)
                {
                    FileHelper.UpdateCodeWithCartonVerifyingEntry(_filePath, storageUpdate, _unsentStatus);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in MarkAsSent: " + ex.Message);
            }
        }

        public void MarkCodeWithCartonAsFailed(StorageUpdate update)
        {
            throw new NotImplementedException();
        }
    }

}
