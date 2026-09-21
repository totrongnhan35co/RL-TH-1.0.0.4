using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using BarcodeVerificationSystem.Utils;
using CommonVariable;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows.Documents;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Woka
{
    public class WokaAllValueProcess
    {
        private readonly string _filePath;
        // STATIC lock: mọi instance của WokaAllValueProcess (cùng một process) đều dùng chung lock,
        // tránh xung đột file khi nhiều thread khởi tạo instance khác nhau.
        private static readonly object _fileLock = new object();
        private readonly string _databasePath;
        private string productCode;
        private string production_batch_code;
        private double weight;
        char _us = '\x1F';

        // Retry configuration cho các thao tác file
        private const int FileOperationMaxRetries = 5;
        private const int FileOperationRetryDelayMs = 100;

        public string getFilePath()
        {
            return _filePath;
        }

        // string filePath, string databasePath , string productCode, string production_batch_code, double weight
        public WokaAllValueProcess(JobModel jobModel)
        {
            string fileName = jobModel.FileName + "_AllValues" + ".csv";
            string sentAllValuePath = CommVariables.PathAllValues + fileName;
            if (!Directory.Exists(CommVariables.PathAllValues))
            {
                Directory.CreateDirectory(CommVariables.PathAllValues);
            }
            _filePath = sentAllValuePath;
            _databasePath = jobModel.DirectoryDatabase;
            productCode = jobModel.CaoSuProduct?.product_code ?? "";
            production_batch_code = jobModel?.LOTNumber ?? "";
            weight = jobModel.productWeight;

            if (!File.Exists(_filePath))
            {
                CreateFileWithRetry(_filePath);
                InitializeDefaultDatabaseFormat();
            }
        }

        private void CreateFileWithRetry(string filePath)
        {
            const int maxRetries = 5;
            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    File.Create(filePath).Dispose();
                    return;
                }
                catch (IOException) when (retry < maxRetries - 1)
                {
                    Thread.Sleep(500);
                }
            }
        }

        public void InitializeDefaultDatabaseFormat()
        {
            try
            {
                lock (_fileLock)
                {
                    const string us = "\x1F"; // Unit Separator - best delimiter
                    var lines = new List<string>();

                    string extension = System.IO.Path.GetExtension(_databasePath);
                    if (!string.IsNullOrEmpty(extension))
                        extension = extension.ToLowerInvariant();

                    // 1. Read input file (Excel or any text format)
                    if (extension == ".xlsx" || extension == ".xls")
                    {
                        // Read Excel file
                        List<string[]> excelRows = FileFuncs.ReadExcelData(_databasePath, null);

                        foreach (string[] row in excelRows)
                        {
                            if (row == null || row.Length == 0)
                                continue;

                            string cell = row[0] ?? string.Empty;
                            // Lưu raw - KHÔNG trim
                            if (!string.IsNullOrWhiteSpace(cell))
                                lines.Add(cell); // Take first column as QR code
                        }
                    }
                    else
                    {
                        // Read any text file (CSV, TSV, raw, etc.)
                        string[] allLines = File.ReadAllLines(_databasePath, Encoding.UTF8);

                        foreach (string line in allLines)
                        {
                            // Lưu raw - KHÔNG trim
                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            // If line contains our delimiter → take first field only
                            int separatorIndex = line.IndexOf(us);
                            string qrCode = (separatorIndex >= 0)
                                ? line.Substring(0, separatorIndex)
                                : line;

                            lines.Add(qrCode);
                        }
                    }

                    // 2. Build new standardized database using Unit Separator
                    var newLines = new List<string>();

                    // Header
                    newLines.Add(string.Join(us,
                        "Index", "QRcode", "Status", "Created Time", "Printed Time", "Checked Time", "Mapped Time", "QRCode Pallet"));

                    // Data rows
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string qrcode = lines[i];
                        if (string.IsNullOrWhiteSpace(qrcode))
                            continue;

                        string createdTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        string[] fields = new string[]
                        {
                            (i + 1).ToString(),
                            qrcode,
                            "created",
                            createdTime,
                            "", // Printed Time
                            "", // Checked Time
                            "", // Mapped Time
                            ""  // QRCode Pallet
                        };

                        newLines.Add(string.Join(us, fields));
                    }

                    // 3. Write output file - UTF-8 with BOM (Excel loves it)
                    File.WriteAllLines(_filePath, newLines, new UTF8Encoding(true));
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in InitializeDefaultDatabaseFormat: " + ex.ToString());
            }
        }
        /// <summary>
        /// Thực thi hành động đọc/ghi file với retry khi gặp IOException.
        /// Giúp giảm thiểu lỗi transient do file bị lock bởi process khác.
        /// </summary>
        private T ExecuteFileOperationWithRetry<T>(Func<T> operation, string operationName)
        {
            for (int retry = 0; retry < FileOperationMaxRetries; retry++)
            {
                try
                {
                    return operation();
                }
                catch (IOException ioEx) when (retry < FileOperationMaxRetries - 1)
                {
                    ProjectLogger.WriteWarning($"[ALLVALUE] {operationName} bị lỗi IO, thử lại {retry + 1}/{FileOperationMaxRetries} | {ioEx.Message}");
                    Thread.Sleep(FileOperationRetryDelayMs * (retry + 1));
                }
            }
            // Lần cuối để throw nếu vẫn lỗi
            return operation();
        }

        private void ExecuteFileOperationWithRetry(Action operation, string operationName)
        {
            ExecuteFileOperationWithRetry<object>(() => { operation(); return null; }, operationName);
        }

        public bool UpdatePrint(string qrcode, string printedDate)
        {
            try
            {
                FileHelper.UpdateWokaAllValuePrint(_filePath, qrcode, printedDate);
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[ALLVALUE] UpdatePrint failed | qrcode={qrcode} | {ex.Message}");
                return false;
            }
        }

        public bool UpdateCheck(string qrcode, string checkedDate)
        {
            try
            {
                FileHelper.UpdateWokaAllValueCheck(_filePath, qrcode, checkedDate);
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[ALLVALUE] UpdateCheck failed | qrcode={qrcode} | {ex.Message}");
                return false;
            }
        }

        public bool UpdatePallet(string qrcode, string qrCodePallet, string mappedDate)
        {
            try
            {
                FileHelper.UpdateWokaAllValuePallet(_filePath, qrcode, qrCodePallet, mappedDate);
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[ALLVALUE] UpdatePallet failed | qrcode={qrcode} | {ex.Message}");
                return false;
            }
        }
        //public void UpdatePrint(string qrcode, string printedDate)
        //{
        //    try
        //    {
        //        FileHelper.UpdateWokaAllValuePrint(_filePath, qrcode, printedDate);
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError("Error in UpdatePrint: " + ex.Message);
        //    }
        //}

        //public void UpdateCheck(string qrcode, string checkedDate)
        //{
        //    try
        //    {
        //        FileHelper.UpdateWokaAllValueCheck(_filePath, qrcode, checkedDate);
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError("Error in UpdateCheck: " + ex.Message);
        //    }
        //}

        //public void UpdatePallet(string qrcode, string qrCodePallet, string mappedDate)
        //{
        //    try
        //    {
        //        FileHelper.UpdateWokaAllValuePallet(_filePath, qrcode, qrCodePallet, mappedDate);
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError("Error in UpdatePallet: " + ex.Message);
        //    }
        //}

        public void AddNewQrCode(List<string> list_qrcode)
        {
            try
            {
                lock (_fileLock)
                {
                    ExecuteFileOperationWithRetry(() =>
                    {
                        var lines = File.ReadAllLines(_filePath, Encoding.UTF8).ToList();
                        var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        // Find the last index (skip header row)
                        int lastIndex = 0;
                        if (lines.Count > 1)
                        {
                            // Start from index 1 to skip header
                            for (int i = 1; i < lines.Count; i++)
                            {
                                var parts = lines[i].Split(_us);
                                if (parts.Length > 0 && int.TryParse(parts[CaoSuAllValueValues.Index], out int currentIndex))
                                {
                                    if (currentIndex > lastIndex)
                                    {
                                        lastIndex = currentIndex;
                                    }
                                }
                            }
                        }

                        // Add new QR codes with continued index
                        foreach (var qrcode in list_qrcode)
                        {
                            if (string.IsNullOrEmpty(qrcode)) continue; // Skip empty QR codes

                            lastIndex++;
                            // Lưu raw QR code - KHÔNG normalize
                            var newLine = $"{lastIndex}{_us}{qrcode}{_us}created{_us}{currentTime}{_us}{_us}{_us}{_us}";
                            lines.Add(newLine);
                        }

                        // Write all lines back to file atomically, đồng nhất UTF-8 BOM như lúc khởi tạo
                        string tempFile = _filePath + ".tmp";
                        File.WriteAllLines(tempFile, lines, new UTF8Encoding(true));
                        File.Replace(tempFile, _filePath, null);
                    }, "AddNewQrCode");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in AddNewQrCode: " + ex.Message);
            }
        }

        /// <summary>
        /// Reads all lines from the file and converts them to RequestSyncOffline class
        /// </summary>
        /// <returns>RequestSyncOffline object containing all QR code data</returns>
        public RequestSyncOffline GetAllValuePayload()
        {
            try
            {
                var lines = FileHelper.ReadAllLinesWithLock(_filePath).ToList();
                var qrList = new List<QrList>();

                // Skip header row (index 0), start from index 1
                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = lines[i].Split(_us);

                        if (parts.Length < 8) continue; // Skip invalid lines (now 8 columns instead of 11)

                        // Parse index_in_lot to int
                        int indexInLot = 0;
                        if (!string.IsNullOrEmpty(parts[CaoSuAllValueValues.Index]) && 
                            !int.TryParse(parts[CaoSuAllValueValues.Index], out indexInLot))
                        {
                            continue; // Skip if index cannot be parsed
                        }

                        // Column indices after removing Product Code, Production Batch Code, Weight:
                        // 0: Index, 1: QRcode, 2: Status, 3: Created Time, 4: Printed Time, 5: Checked Time, 6: Mapped Time, 7: QRCode Pallet
                        var qrItem = new QrList
                        {
                            index_in_lot = indexInLot,
                            qrcode_value = parts[CaoSuAllValueValues.QRcodeIndex] ?? string.Empty,
                            status = parts[CaoSuAllValueValues.StatusIndex] ?? string.Empty,
                            product_code = "", // Removed from file
                            production_batch_code = "", // Removed from file
                            weight = 0, // Removed from file
                            created_time = parts.Length > 3 ? (parts[3] ?? string.Empty) : string.Empty,
                            printed_at = parts.Length > 4 ? (parts[4] ?? string.Empty) : string.Empty,
                            qr_checked_at = parts.Length > 5 ? (parts[5] ?? string.Empty) : string.Empty,
                            mapped_at = parts.Length > 6 ? (parts[6] ?? string.Empty) : string.Empty,
                            qr_pallet = parts.Length > 7 ? (parts[7] ?? string.Empty) : string.Empty
                        };

                        qrList.Add(qrItem);
                    }

                    return new RequestSyncOffline
                    {
                        qr_list = qrList
                    };
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in GetAllValuePayload: " + ex.Message);
                return new RequestSyncOffline
                {
                    qr_list = new List<QrList>()
                };
            }
        }

        /// <summary>
        /// Returns a list of QR codes where the status is not equal to "mapped"
        /// </summary>
        /// <returns>List of QR codes that are not mapped</returns>
        public List<string> GetUnmappedQRCodes()
        {
            try
            {
                lock (_fileLock)
                {
                    return ExecuteFileOperationWithRetry(() =>
                    {
                        var lines = File.ReadAllLines(_filePath, Encoding.UTF8).ToList();
                        var unmappedCodes = new List<string>();

                        for (int i = 1; i < lines.Count; i++)
                        {
                            var parts = lines[i].Split(_us);

                            if (parts.Length < 8) continue;

                            string status = parts.Length > CaoSuAllValueValues.StatusIndex
                                ? (parts[CaoSuAllValueValues.StatusIndex] ?? string.Empty)
                                : string.Empty;

                            if (!status.Equals("mapped", StringComparison.OrdinalIgnoreCase))
                            {
                                string qrcode = parts.Length > CaoSuAllValueValues.QRcodeIndex
                                    ? (parts[CaoSuAllValueValues.QRcodeIndex] ?? string.Empty)
                                    : string.Empty;

                                if (!string.IsNullOrEmpty(qrcode))
                                {
                                    // Lưu raw - KHÔNG normalize
                                    string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(qrcode));
                                    unmappedCodes.Add(base64Code);
                                }
                            }
                        }

                        return unmappedCodes;
                    }, "GetUnmappedQRCodes");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in GetUnmappedQRCodes: " + ex.Message);
                return new List<string>();
            }
        }
    }
}
