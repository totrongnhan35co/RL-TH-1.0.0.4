using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using BarcodeVerificationSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Woka
{
    public class WokaStorageService : IStorageService<VerificationDataEntry>
    {
        private readonly string _filePath;
        private readonly object _fileLock = new object();
        private readonly string _unsentStatus = "NotSent";
        private readonly string _sentStatus = "Sent";
        private readonly string _databasePath;
        private const char _us = '\x1F'; // Unit Separator

        public WokaStorageService(string filePath, string databasePath)
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
                var entryLine = $"{entry.Id}{_us}{entry.Code}{_us}{entry.VerifiedStatus}{_us}{entry.VerifiedDate}{_us}{entry.SaasStatus}{_us}{entry.SAPStatus}{_us}{entry.SaasError}{_us}{entry.SAPError}{_us}{_unsentStatus}";
                File.AppendAllLines(_filePath, new[] { entryLine }, Encoding.UTF8);
            }
        }

        public List<VerificationDataEntry> LoadUnsentEntries()
        {
            try
            {
                lock (_fileLock)
                {
                    var entries = new List<VerificationDataEntry>();
                    var lines = File.ReadAllLines(_filePath, Encoding.UTF8);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var parts = line.Split(_us);
                        // File Woka có 9 cột: Id, Code, VerifiedStatus, VerifiedDate, SaaSStatus, SAPStatus, SaaSError, SAPError, Status
                        if (parts.Length < 9)
                            continue;

                        // Bỏ qua dòng header nếu có
                        if (!int.TryParse(parts[VerifyingValues.Index], out int id))
                            continue;

                        entries.Add(new VerificationDataEntry
                        {
                            Id = id,
                            Code = parts[VerifyingValues.Code],
                            VerifiedStatus = parts[VerifyingValues.VerifiedStatus],
                            VerifiedDate = parts[VerifyingValues.VerifiedDate],
                            SaasStatus = parts[VerifyingValues.SaaSStatus],
                            SAPStatus = parts[VerifyingValues.SAPStatus],
                            SaasError = parts[VerifyingValues.SaaSError],
                            SAPError = parts[VerifyingValues.SAPError],
                            Status = parts[VerifyingValues.SentStatus],
                        });
                    }

                    return entries.Where(e => e.Status == _unsentStatus).ToList();
                }
            }
            catch (System.Exception ex)
            {
                ProjectLogger.WriteError("Error occurred in LoadUnsentEntries: " + ex.Message);
                return new List<VerificationDataEntry>();
            }
        }

        public void MarkCodeWithCartonAsSent(StorageUpdate storageUpdate)
        {
            try
            {
                lock (_fileLock)
                {
                    FileHelper.UpdateCodeWithCartonVerifyingEntry(_filePath, storageUpdate, _sentStatus);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in MarkAsSent: " + ex.Message);
            }
        }

        public void MarkCodeWithCartonAsFailed(StorageUpdate storageUpdate)
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

        public void MarkAsFailed(StorageUpdate storageUpdate){}

        public void MarkAsSent(StorageUpdate storageUpdate){}

        public void MarkCodeWithPalletAsSent(StorageUpdate storageUpdate)
        {
        }

        public void MarkCodeWithPalletAsFailed(StorageUpdate storageUpdate)
        {
        }
    }

}
