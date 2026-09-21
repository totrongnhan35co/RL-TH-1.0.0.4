using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using BarcodeVerificationSystem.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai
{
    public class CaoSuAllValueStorage
    {
        private readonly string _filePath;
        private readonly object _fileLock = new object();
        private readonly string _databasePath;

        public string getFilePath()
        {
            return _filePath;
        }

        public CaoSuAllValueStorage(string filePath, string databasePath)
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
                    var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // Add header row
                    newLines.Add("Index,QRcode,Status,Product Code,Production Batch Code,Weight,Created Time,Printed Time,Checked Time,Mapped Time,QRCode Pallet");

                    for (int i = 0; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(',');
                        // Initialize with: Id, QRcode (from parts[0]), Status="created", empty fields (3-5), CreatedTime, empty fields (7-10)
                        // Format: Index,QRcode,Status,ProductCode,ProductionBatchCode,Weight,CreatedTime,PrintedTime,CheckedTime,MappedTime,QRCodePallet (11 fields total)
                        var newLine = $"{i + 1},{parts[0]},created,,,{currentTime},,,,,";
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
    }
}

