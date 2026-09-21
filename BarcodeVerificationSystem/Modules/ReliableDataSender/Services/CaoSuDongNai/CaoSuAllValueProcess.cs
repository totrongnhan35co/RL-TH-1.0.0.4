using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using BarcodeVerificationSystem.Utils;
using CommonVariable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai
{
    public class CaoSuAllValueProcess
    {
        private readonly string _filePath;
        private readonly object _fileLock = new object();
        private readonly string _databasePath;
        private string productCode;
        private string production_batch_code;
        private double weight;

        public string getFilePath()
        {
            return _filePath;
        }

        // string filePath, string databasePath , string productCode, string production_batch_code, double weight
        //public CaoSuAllValueProcess(JobModel jobModel)
        //{
        //    string fileName = jobModel.FileName + "_AllValues" + ".csv";
        //    string sentAllValuePath = CommVariables.PathAllValues + fileName;
        //    if (!Directory.Exists(CommVariables.PathAllValues))
        //    {
        //        Directory.CreateDirectory(CommVariables.PathAllValues);
        //    }
        //    _filePath = sentAllValuePath;
        //    _databasePath = jobModel.DirectoryDatabase;
        //    productCode = jobModel.CaoSuProduct?.product_code ?? "";
        //    production_batch_code = jobModel?.LOTNumber ?? "";
        //    weight = jobModel.productWeight;

        //    if (!File.Exists(_filePath))
        //    {
        //        File.Create(_filePath).Dispose(); // Dispose to release the handle immediately
        //        InitializeDefaultDatabaseFormat(jobModel.IsFirstRowHeader);
        //    }
        //}
              public CaoSuAllValueProcess(JobModel jobModel)
        {
            string fileName = jobModel.FileName + "_AllValues" + ".csv";
            string sentAllValuePath = CommVariables.PathAllValues + fileName;
            if (!Directory.Exists(CommVariables.PathAllValues))
            {
                Directory.CreateDirectory(CommVariables.PathAllValues);
            }
            _filePath = sentAllValuePath;
            _databasePath = jobModel.DirectoryDatabase;

            productCode = !string.IsNullOrEmpty(jobModel.PrintJobProductCode)
                ? jobModel.PrintJobProductCode
                : (jobModel.CaoSuProduct?.product_code
                    ?? jobModel.LastCheckedProductCode
                    ?? "");

            production_batch_code = jobModel?.LOTNumber ?? "";
            weight = jobModel.productWeight;

            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose();
                InitializeDefaultDatabaseFormat(jobModel.IsFirstRowHeader);
            }
            else
            {
                // File đã tồn tại → sync các mã mới được in thêm từ database vào AllValues
                SyncMissingCodesFromDatabase(jobModel.IsFirstRowHeader);
            }
        }

        /// <summary>
        /// Đọc database gốc và thêm vào AllValues những mã chưa có.
        /// Dùng để xử lý trường hợp mã được in thêm ở JobPrint sau khi AllValues đã tạo.
        /// </summary>
        private void SyncMissingCodesFromDatabase(bool isFirstRowHeader)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_databasePath) || !File.Exists(_databasePath))
                    return;

                lock (_fileLock)
                {
                    var existingLines = File.ReadAllLines(_filePath).ToList();
                    var existingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    int lastIndex = 0;

                    // Đọc mã đã có trong AllValues (bỏ qua header)
                    for (int i = 1; i < existingLines.Count; i++)
                    {
                        var p = existingLines[i].Split(',');
                        if (p.Length > 1 && !string.IsNullOrWhiteSpace(p[1]))
                        {
                            existingCodes.Add(p[1].Trim());
                            if (int.TryParse(p[0], out int idx) && idx > lastIndex)
                                lastIndex = idx;
                        }
                    }

                    // Đọc database gốc, tìm mã chưa có trong AllValues
                    var dbLines = File.ReadAllLines(_databasePath).ToList();
                    var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var newLines = new List<string>();

                    int start = isFirstRowHeader ? 1 : 0;
                    for (int i = start; i < dbLines.Count; i++)
                    {
                        var parts = dbLines[i].Split(',');
                        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0])) continue;

                        string qrCode = parts[0].Trim();
                        if (existingCodes.Contains(qrCode)) continue;

                        lastIndex++;
                        newLines.Add($"{lastIndex},{qrCode},created,{productCode},{production_batch_code},{weight},{currentTime},,,,");
                    }

                    if (newLines.Count > 0)
                    {
                        File.AppendAllLines(_filePath, newLines, Encoding.UTF8);
                        ProjectLogger.WriteInfo($"[AllValues] Sync {newLines.Count} mã mới từ database vào AllValues.");
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("SyncMissingCodesFromDatabase error: " + ex.Message);
            }
        }

        public void InitializeDefaultDatabaseFormat(bool IsFirstRowHeader)
        {
            try
            {
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_databasePath).ToList();
                    var newLines = new List<string>();
                    var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    newLines.Add("Index,QRcode,Status,Product Code,Production Batch Code,Weight,Created Time,Printed Time,Checked Time,Mapped Time,QRCode Pallet");

                    for (int i = IsFirstRowHeader ? 1 : 0; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(',');
                        // Initialize with: Id, QRcode (from parts[0]), Status="created", empty fields (3-6), CreatedTime, empty fields (8-12)
                        // Format: Id,QRcode,Status,ProductCode,ProductionBatchCode,Weight,CreatedTime,PrintedTime,CheckedTime,MappedTime,QRCodePallet,StorageStatus
                        var newLine = $"{i + (IsFirstRowHeader ? 0 : 1)},{parts[0]},created,{productCode},{production_batch_code},{weight},{currentTime},,,,";
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

        /// <summary>
        /// Updates the status to "printed" and sets the PrintedTime when a code is printed
        /// </summary>
        /// <param name="qrcode">The QR code to update</param>
        /// <param name="printedDate">The date/time when the code was printed</param>
        public void UpdatePrint(string qrcode, string printedDate)
        {
            try
            {
                FileHelper.UpdateCaoSuAllValuePrint(_filePath, qrcode, printedDate);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdatePrint: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates the status to "valid" and sets the CheckedTime when a code is checked
        /// </summary>
        /// <param name="qrcode">The QR code to update</param>
        /// <param name="checkedDate">The date/time when the code was checked</param>
        public void UpdateCheck(string qrcode, string checkedDate)
        {
            try
            {
                FileHelper.UpdateCaoSuAllValueCheck(_filePath, qrcode, checkedDate);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdateCheck: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates the status to "mapped", sets the QRCodePallet and MappedTime when a pallet is scanned
        /// </summary>
        /// <param name="qrcode">The QR code to update</param>
        /// <param name="qrCodePallet">The pallet QR code</param>
        /// <param name="mappedDate">The date/time when the code was mapped to pallet</param>
        public void UpdatePallet(string qrcode, string qrCodePallet, string mappedDate)
        {
            try
            {
                FileHelper.UpdateCaoSuAllValuePallet(_filePath, qrcode, qrCodePallet, mappedDate);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdatePallet: " + ex.Message);
            }
        }

        /// <summary>
        /// Adds new QR codes to the end of the file with continued index
        /// </summary>
        /// <param name="list_qrcode">List of QR codes to add</param>
        public void AddNewQrCode(List<string> list_qrcode)
        {
            try
            {
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // Find the last index (skip header row)
                    int lastIndex = 0;
                    if (lines.Count > 1)
                    {
                        // Start from index 1 to skip header
                        for (int i = 1; i < lines.Count; i++)
                        {
                            var parts = lines[i].Split(',');
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
                        var newLine = $"{lastIndex},{qrcode},created,{productCode},{production_batch_code},{weight},{currentTime},,,,";
                        lines.Add(newLine);
                    }

                    // Write all lines back to file
                    string tempFile = _filePath + ".tmp";
                    File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                    File.Replace(tempFile, _filePath, null);
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
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    var qrList = new List<QrList>();

                    // Skip header row (index 0), start from index 1
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(',');

                        // ✅ Giảm điều kiện từ 11 xuống 7 (các cột cuối có thể rỗng)
                        if (parts.Length < 7) continue;

                        int weightValue = 0;
                        if (!string.IsNullOrEmpty(parts[CaoSuAllValueValues.WeightIndex]))
                        {
                            if (!int.TryParse(parts[CaoSuAllValueValues.WeightIndex], out weightValue))
                            {
                                if (double.TryParse(parts[CaoSuAllValueValues.WeightIndex], out double weightDouble))
                                    weightValue = (int)weightDouble;
                            }
                        }

                        int indexInLot = 0;
                        if (!string.IsNullOrEmpty(parts[CaoSuAllValueValues.Index]))
                        {
                            if (!int.TryParse(parts[CaoSuAllValueValues.Index], out indexInLot))
                                continue;
                        }

                        // ✅ Safe-read: dùng helper tránh IndexOutOfRange
                        string SafeGet(int idx) => parts.Length > idx ? (parts[idx] ?? "") : "";

                        var qrItem = new QrList
                        {
                            index_in_lot = indexInLot,
                            qrcode_value = SafeGet(CaoSuAllValueValues.QRcodeIndex),
                            status = SafeGet(CaoSuAllValueValues.StatusIndex),
                            product_code = SafeGet(CaoSuAllValueValues.ProductCodeIndex),
                            production_batch_code = SafeGet(CaoSuAllValueValues.ProductionBatchCodeIndex),
                            weight = weightValue,
                            created_time = SafeGet(CaoSuAllValueValues.CreatedTimeIndex),
                            printed_at = SafeGet(CaoSuAllValueValues.PrintedTimeIndex),
                            qr_checked_at = SafeGet(CaoSuAllValueValues.CheckedTimeIndex),
                            mapped_at = SafeGet(CaoSuAllValueValues.MappedTimeIndex),
                            qr_pallet = SafeGet(CaoSuAllValueValues.QRCodePalletIndex),
                        };

                        // ✅ Bỏ qua dòng không có qrcode_value (dữ liệu không hợp lệ)
                        if (string.IsNullOrWhiteSpace(qrItem.qrcode_value)) continue;

                        qrList.Add(qrItem);
                    }

                    return new RequestSyncOffline { qr_list = qrList };
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in GetAllValuePayload: " + ex.Message);
                return new RequestSyncOffline { qr_list = new List<QrList>() };
            }
        }

        public List<string> GetPrintedQRCodes()
        {
            try
            {
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    var printedCodes = new List<string>();
                   

                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(',');

                        if (parts.Length < 8) continue;

                        string status = parts.Length > CaoSuAllValueValues.StatusIndex
                            ? (parts[CaoSuAllValueValues.StatusIndex] ?? string.Empty)
                            : string.Empty;

                        if (status.Equals("printed", StringComparison.OrdinalIgnoreCase)) //  && !status.Equals("created", StringComparison.OrdinalIgnoreCase)
                        {
                            string qrcode = parts.Length > CaoSuAllValueValues.QRcodeIndex
                                ? (parts[CaoSuAllValueValues.QRcodeIndex] ?? string.Empty)
                                : string.Empty;

                            string printedDate = parts.Length > CaoSuAllValueValues.PrintedTimeIndex
                               ? (parts[CaoSuAllValueValues.PrintedTimeIndex] ?? string.Empty)
                               : string.Empty;

                            printedCodes.Add(qrcode + ',' + printedDate); //
                        }
                    }

                    return printedCodes;
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in GetPrintedQRCodes: " + ex.Message);
                return new List<string>();
            }
        }
    }
}
