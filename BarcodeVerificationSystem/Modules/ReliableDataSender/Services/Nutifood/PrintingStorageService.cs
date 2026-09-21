using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.CodeGeneration;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using BarcodeVerificationSystem.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services
{
    public class PrintingStorageService : IStorageService<PrintingDataEntry>
    {
        private readonly string _filePath;
        private readonly object _fileLock = new object();
        private readonly string _unsentStatus = "NotSent";
        private readonly string _sentStatus = "Sent";
        private readonly string _databasePath;

        public string getFilePath()
        {
            return _filePath;
        }

        public PrintingStorageService(string filePath, string databasePath)
        {
            _filePath = filePath;
            _databasePath = databasePath;
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose(); // Dispose to release the handle immediately
                InitializeDefaultDatabaseFormat();
            }

        }

        public void InitializeDefaultDatabaseFormat()
        {
            try
            {
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_databasePath).ToList();
                    var newLines = new List<string>();

                    for (int i = 0; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(',');
                        var newLine = $"{i + 1},{parts[0]},{parts[1]},,,,,,";
                        newLines.Add(newLine);
                    }

                    File.WriteAllLines(_filePath, newLines, Encoding.UTF8);
                }
            }
            catch (System.Exception ex)
            {
                ProjectLogger.WriteError("Error occurred in InitializeDefaultDatabaseFormat" + ex.Message);
            }
           
        }

        public void AppendEntry(PrintingDataEntry entry)
        {
            try
            {
                lock (_fileLock)
                {
                    FileHelper.AppendPrintingEntry(_filePath, entry, _unsentStatus);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in AppendNewEntry: " + ex.Message);
            }
        }

        public List<PrintingDataEntry> LoadUnsentEntries()
        {
            try
            {
                lock (_fileLock)
                {
                    return File.ReadAllLines(_filePath)
                    .Select((line, index) =>
                    {
                        var parts = line.Split(',');
                        return new PrintingDataEntry
                        {
                            Id = int.Parse(parts[PrintingValues.Index]), // parts[0]}
                            Code = parts[PrintingValues.QrCodeIndex], // parts[1]
                            UniqueCode = parts[PrintingValues.UniqueCode],
                            PrintedDate = parts[PrintingValues.PrintedDate],
                            SaaSStatus = parts[PrintingValues.SaaSStatus],
                            SAPStatus = parts[PrintingValues.SAPStatus],
                            SaasError = parts[PrintingValues.SaaSError],
                            SAPError = parts[PrintingValues.SAPError],
                            Status = parts[PrintingValues.SentStatus],
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
                FileHelper.UpdatePrintingEntry(_filePath, storageUpdate, _unsentStatus);
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
                FileHelper.UpdatePrintingEntry(_filePath, storageUpdate, _sentStatus);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in MarkAsSent: " + ex.Message);
            }
        }

        public void MarkCodeWithPalletAsSent(StorageUpdate update)
        {
        }

        public void MarkCodeWithPalletAsFailed(StorageUpdate update)
        {
        }

        public void MarkCodeWithCartonAsSent(StorageUpdate update)
        {
        }

        public void MarkCodeWithCartonAsFailed(StorageUpdate update)
        {
        }
    }

}

#region Comments Code

//public void AppendEntry(PrintingDataEntry entry)
//{
//    try
//    {
//        lock (_fileLock)
//        {
//            var lines = File.ReadAllLines(_filePath).ToList();

//            // This will throw if entryId is out of range
//            var parts = lines[entry.Id - 1].Split(',');

//            // Blindly replace the line like MarkAsSent
//            lines[entry.Id - 1] = $"{entry.Id},{entry.Code},{entry.UniqueCode},{entry.PrintedDate},,,,,{_unsentStatus}";

//            File.WriteAllLines(_filePath, lines, Encoding.UTF8);
//        }
//    }
//    catch (System.Exception ex)
//    {
//        ProjectLogger.WriteError("Error occurred in InitializeDefaultDatabaseFormat" + ex.Message);
//    }
//}

//public void MarkAsFailed(int entryId, string PrintedDate, string SaasStatus, string SAPStatus, string SaasSError, string SAPError)
//{
//    try
//    {
//        lock (_fileLock)
//        {
//            var lines = File.ReadAllLines(_filePath).ToList();

//            // This will throw if entryId is out of range
//            var parts = lines[entryId - 1].Split(',');

//            // Blindly replace the line like MarkAsSent
//            lines[entryId - 1] = $"{parts[0]},{parts[1]},{parts[2]},{PrintedDate},{SaasStatus},{SAPStatus},{SaasSError},{SAPError},{_unsentStatus}";

//            File.WriteAllLines(_filePath, lines, Encoding.UTF8);
//        }
//    }
//    catch (System.Exception ex)
//    {
//        ProjectLogger.WriteError("Error occurred in MarkAsFailed" + ex.Message);
//    }
//}


//public void MarkAsSent(int entryId, string PrintedDate, string SaasStatus, string SAPStatus, string SaasSError, string SAPError)
//{
//    try
//    {
//        lock (_fileLock)
//        {
//            var lines = File.ReadAllLines(_filePath).ToList();

//            // This will throw if entryId is out of range
//            var parts = lines[entryId - 1].Split(',');

//            // Blindly replace the line
//            lines[entryId - 1] = $"{parts[0]},{parts[1]},{parts[2]},{PrintedDate},{SaasStatus},{SAPStatus},{SaasSError},{SAPError},{_sentStatus}";

//            File.WriteAllLines(_filePath, lines, Encoding.UTF8);
//        }
//    }
//    catch (System.Exception ex)
//    {
//        ProjectLogger.WriteError("Error occurred in MarkAsSent" + ex.Message);
//    }

//}


#endregion
