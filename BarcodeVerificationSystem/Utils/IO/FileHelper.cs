using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using BarcodeVerificationSystem.View.CustomDialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace BarcodeVerificationSystem.Utils
{
    public static class FileHelper
    {
        private static readonly object _fileLock = new object();

        public static string[] ReadAllLinesWithLock(string filePath)
        {
            lock (_fileLock)
            {
                return File.ReadAllLines(filePath, Encoding.UTF8);
            }
        }

        public static void UpdatePrintingEntry(string filePath, StorageUpdate storageUpdate,
                                       string status)
        {
            lock (_fileLock)
            {

                var lines = File.ReadAllLines(filePath).ToList();
                for (int i = 0; i < lines.Count; i++) // int = 1
                {
                    var parts = lines[i].Split(',');
                    if (int.Parse(parts[0]) == storageUpdate.Id)
                    {
                        lines[i] = $"{parts[0]},{parts[1]},{parts[2]},{storageUpdate.PrintedDate},{storageUpdate.SaaSStatus}" +
                                    $",{storageUpdate.SAPStatus},{storageUpdate.SaaSError},{storageUpdate.SAPError},{status}";
                        break;
                    }
                }
                File.WriteAllLines(filePath, lines, Encoding.UTF8);


                //var lines = File.ReadAllLines(filePath).ToList();

                //if (storageUpdate.Id < 1 || storageUpdate.Id > lines.Count)
                //    throw new ArgumentOutOfRangeException(nameof(storageUpdate.Id));

                //var parts = lines[storageUpdate.Id - 1].Split(',');

                //lines[storageUpdate.Id - 1] =
                //    $"{parts[0]},{parts[1]},{parts[2]},{storageUpdate.PrintedDate},{storageUpdate.SaaSStatus}" +
                //    $",{storageUpdate.SAPStatus},{storageUpdate.SaaSError},{storageUpdate.SAPError},{status}";

                //string tempFile = filePath + ".tmp";

                //// Write to temp file first
                //File.WriteAllLines(tempFile, lines, Encoding.UTF8);

                //// Atomically replace the original file
                //File.Replace(tempFile, filePath, null);
            }
        }

        public static void AppendPrintingEntry(string filePath, PrintingDataEntry entry, string unsentStatus)
        {
            lock (_fileLock)
            {
                 var lines = File.ReadAllLines(filePath).ToList();

                if (entry.Id < 1 || entry.Id > lines.Count)
                    throw new ArgumentOutOfRangeException(nameof(entry.Id));

                lines[entry.Id - 1] =
                    $"{entry.Id},{entry.Code},{entry.UniqueCode},{entry.PrintedDate},,,,,{unsentStatus}";

                string tempFile = filePath + ".tmp";

                // Write to temp file first
                File.WriteAllLines(tempFile, lines, Encoding.UTF8);

                // Replace atomically
                File.Replace(tempFile, filePath, null);
            }
        }

        public static void UpdateVerifyingEntry(string filePath, StorageUpdate storageUpdate,
                               string status)
        {
            lock (_fileLock)
            {

                var lines = File.ReadAllLines(filePath).ToList();
                for (int i = 0; i < lines.Count; i++) // int = 1
                {
                    var parts = lines[i].Split(',');
                    if (int.Parse(parts[0]) == storageUpdate.Id)
                    {
                        lines[i] =
                                    $"{parts[0]},{parts[1]},{storageUpdate.VerifiedStatus},{storageUpdate.VerifiedDate}" +
                                    $",{storageUpdate.SaaSStatus},{storageUpdate.SAPStatus},{storageUpdate.SaaSError},{storageUpdate.SAPError},{status}";
                        break;
                    }
                }
                File.WriteAllLines(filePath, lines, Encoding.UTF8);

                //var lines = File.ReadAllLines(filePath).ToList();

                //if (storageUpdate.Id < 1 || storageUpdate.Id > lines.Count)
                //    throw new ArgumentOutOfRangeException(nameof(storageUpdate.Id));


                //var parts = lines[storageUpdate.Id - 1].Split(',');

                //lines[storageUpdate.Id - 1] =
                //    $"{parts[0]},{parts[1]},{storageUpdate.VerifiedStatus},{storageUpdate.VerifiedDate}" +
                //    $",{storageUpdate.SaaSStatus},{storageUpdate.SAPStatus},{storageUpdate.SaaSError},{storageUpdate.SAPError},{status}";

                //string tempFile = filePath + ".tmp";

                //// Write to temp file first
                //File.WriteAllLines(tempFile, lines, Encoding.UTF8);

                //// Atomically replace the original file
                //File.Replace(tempFile, filePath, null);
            }
        }
        public static void UpdateCodeWithCartonVerifyingEntry(string filePath, StorageUpdate storageUpdate, string status)
        {
            if (storageUpdate == null) throw new ArgumentNullException(nameof(storageUpdate));

            lock (_fileLock)
            {
                const char us = '\x1F'; // Unit Separator
                var lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
                string idText = storageUpdate.Id.ToString();
                bool updated = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];

                    // Fast early skip for empty or obviously wrong lines
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Find the first Unit Separator to extract the ID safely and quickly
                    int firstSeparatorIndex = line.IndexOf(us);
                    if (firstSeparatorIndex <= 0) continue; // no separator or separator at start → invalid

                    string idInFile = line.Substring(0, firstSeparatorIndex);

                    if (idInFile != idText) continue;

                    // We found the correct line by ID
                    // Extract only what we know exists: ID and the big code (parts[1])
                    // Need to find the second separator to extract just the code field
                    int secondSeparatorIndex = line.IndexOf(us, firstSeparatorIndex + 1);
                    string code = secondSeparatorIndex > 0
                        ? line.Substring(firstSeparatorIndex + 1, secondSeparatorIndex - firstSeparatorIndex - 1)
                        : (firstSeparatorIndex + 1 < line.Length ? line.Substring(firstSeparatorIndex + 1) : ""); // everything after first separator if no second separator

                    // Build the FULL 9-column line (even if some fields are empty)
                    lines[i] =
                        $"{idText}{us}" +                           // 0: Id
                        $"{code}{us}" +                              // 1: Original Code (with GS1 chars)
                        $"{storageUpdate.VerifiedStatus ?? ""}{us}" + // 2: VerifiedStatus
                        $"{storageUpdate.VerifiedDate ?? ""}{us}" +   // 3: VerifiedDate
                        $"{storageUpdate.SaaSStatus ?? ""}{us}" +     // 4: SaaSStatus
                        $"{storageUpdate.SAPStatus ?? ""}{us}" +      // 5: SAPStatus
                        $"{storageUpdate.SaaSError ?? ""}{us}" +      // 6: SaaSError
                        $"{storageUpdate.SAPError ?? ""}{us}" +       // 7: SAPError
                        $"{status}";                               // 8: Final status (e.g. "Unsent")

                    updated = true;
                    break;
                }

                if (!updated)
                    throw new KeyNotFoundException($"Entry with ID {storageUpdate.Id} not found in file.");

                // Atomic write to prevent corruption
                string tempFile = filePath + ".tmp";
                File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                File.Replace(tempFile, filePath, null); // null = no backup
            }
        }

        public static void UpdateCodeWithPalletVerifyingEntry(string filePath, StorageUpdate storageUpdate, string status)
        {
            lock (_fileLock)
            {
                var lines = File.ReadAllLines(filePath).ToList();
                string idText = storageUpdate.Id.ToString();
                bool updated = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    var parts = lines[i].Split(',');

                    if (parts.Length == 0) continue;

                    if (parts[0] == idText)
                    {
                        lines[i] =
                            $"{parts[0]},{parts[1]},{storageUpdate.VerifiedStatus},{storageUpdate.VerifiedDate}" +
                            $",{storageUpdate.SaaSStatus},{storageUpdate.SAPStatus},{storageUpdate.SaaSError},{storageUpdate.SAPError},{status}";

                        updated = true;
                        break;
                    }
                }

                if (!updated)
                    throw new Exception("Entry with matching ID not found.");

                string tempFile = filePath + ".tmp";

                File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                File.Replace(tempFile, filePath, null);
            }
        }

        public static void UpdateWokaAllValuePrint(string filePath, string qrcode, string printedDate)
        {
            lock (_fileLock)
            {
                const char us = '\x1F'; // Unit Separator
                var lines = File.ReadAllLines(filePath).ToList();
                bool updated = false;

                // Skip header row (index 0), start from index 1
                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = lines[i].Split(us);

                    // Need at least 8 fields (indices 0-7)
                    if (parts.Length < 8) continue;

                    // So sánh raw - KHÔNG normalize
                    if (parts[CaoSuAllValueValues.QRcodeIndex] == qrcode)
                    {
                        lines[i] =
                            $"{parts[0]}{us}{parts[1]}{us}printed{us}{parts[3]}{us}{printedDate}{us}" +
                            $"{(parts.Length > 5 ? parts[5] : "")}{us}{(parts.Length > 6 ? parts[6] : "")}{us}{(parts.Length > 7 ? parts[7] : "")}";

                        updated = true;
                        break;
                    }
                }

                if (!updated)
                    throw new Exception($"Entry with QRcode '{qrcode}' not found.");

                string tempFile = filePath + ".tmp";

                File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                File.Replace(tempFile, filePath, null);
            }
        }

        public static void UpdateWokaAllValueCheck(string filePath, string qrcode, string checkedDate)
        {
            lock (_fileLock)
            {
                const char us = '\x1F'; // Unit Separator
                var lines = File.ReadAllLines(filePath).ToList();
                bool updated = false;
                // Skip header row (index 0), start from index 1
                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = lines[i].Split(us);

                    // Need at least 8 fields (indices 0-7)
                    if (parts.Length < 8) continue;

                    // So sánh raw - KHÔNG normalize
                    if (parts[CaoSuAllValueValues.QRcodeIndex] == qrcode)
                    {
                        lines[i] =
                            $"{parts[0]}{us}{parts[1]}{us}valid{us}{parts[3]}{us}{(parts.Length > 4 ? parts[4] : "")}{us}{checkedDate}{us}" +
                            $"{(parts.Length > 6 ? parts[6] : "")}{us}{(parts.Length > 7 ? parts[7] : "")}";

                        updated = true;
                        break;
                    }
                }

                if (!updated)
                    throw new Exception($"Entry with QRcode '{qrcode}' not found.");

                string tempFile = filePath + ".tmp";

                File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                File.Replace(tempFile, filePath, null);
            }
        }

        public static void UpdateWokaAllValuePallet(string filePath, string qrcode, string qrCodePallet, string mappedDate)
        {
            lock (_fileLock)
            {
                const char us = '\x1F'; // Unit Separator
                var lines = File.ReadAllLines(filePath).ToList();
                bool updated = false;

                // Skip header row (index 0), start from index 1
                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = lines[i].Split(us);

                    // Need at least 8 fields (indices 0-7)
                    if (parts.Length < 8) continue;

                    // So sánh raw - KHÔNG normalize
                    if (parts[CaoSuAllValueValues.QRcodeIndex] == qrcode)
                    {
                        lines[i] =
                            $"{parts[0]}{us}{parts[1]}{us}mapped{us}{parts[3]}{us}{(parts.Length > 4 ? parts[4] : "")}{us}" +
                            $"{(parts.Length > 5 ? parts[5] : "")}{us}{mappedDate}{us}{qrCodePallet}";

                        updated = true;
                        break;
                    }
                }

                if (!updated)
                    throw new Exception($"Entry with QRcode '{qrcode}' not found.");

                string tempFile = filePath + ".tmp";

                File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                File.Replace(tempFile, filePath, null);
            }
        }

        public static void UpdateCaoSuAllValuePrint(string filePath, string qrcode, string printedDate)
        {
            lock (_fileLock)
            {
                var lines = File.ReadAllLines(filePath).ToList();
                bool updated = false;

                // Skip header row (index 0), start from index 1
                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = lines[i].Split(',');

                    // Need at least 11 fields (indices 0-10) to access parts[10]
                    if (parts.Length <= CaoSuAllValueValues.QRCodePalletIndex) continue;

                    // Find entry by QRcode (index 1)
                    if (parts[CaoSuAllValueValues.QRcodeIndex] == qrcode)
                    {
                        // Update: preserve all fields, change Status to "printed" and PrintedTime
                        lines[i] =
                            $"{parts[0]},{parts[1]},printed,{parts[3]},{parts[4]}," +
                            $"{parts[5]},{parts[6]},{printedDate}," +
                            $"{parts[8]},{parts[9]},{parts[10]}";

                        updated = true;
                        break;
                    }
                }

                if (!updated)
                    throw new Exception($"Entry with QRcode '{qrcode}' not found.");

                string tempFile = filePath + ".tmp";

                File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                File.Replace(tempFile, filePath, null);
            }
        }

        public static void UpdateCaoSuAllValueCheck(string filePath, string qrcode, string checkedDate)
        {
            lock (_fileLock)
            {
                var lines = File.ReadAllLines(filePath).ToList();
                bool updated = false;

                // Skip header row (index 0), start from index 1
                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = lines[i].Split(',');

                    // Need at least 11 fields (indices 0-10) to access parts[10]
                    if (parts.Length <= CaoSuAllValueValues.QRCodePalletIndex) continue;

                    // Find entry by QRcode (index 1)
                    if (parts[CaoSuAllValueValues.QRcodeIndex] == qrcode)
                    {
                        // Update: preserve all fields, change Status to "valid" and CheckedTime
                        lines[i] =
                            $"{parts[0]},{parts[1]},valid,{parts[3]},{parts[4]}," +
                            $"{parts[5]},{parts[6]},{parts[7]},{checkedDate}," +
                            $"{parts[9]},{parts[10]}";

                        updated = true;
                        break;
                    }
                }

                if (!updated)
                    throw new Exception($"Entry with QRcode '{qrcode}' not found.");

                string tempFile = filePath + ".tmp";

                File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                File.Replace(tempFile, filePath, null);
            }
        }

        public static void UpdateCaoSuAllValuePallet(string filePath, string qrcode, string qrCodePallet, string mappedDate)
        {
            lock (_fileLock)
            {
                var lines = File.ReadAllLines(filePath).ToList();
                bool updated = false;

                // Skip header row (index 0), start from index 1
                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = lines[i].Split(',');

                    // Need at least 11 fields (indices 0-10) to access parts[10]
                    if (parts.Length <= CaoSuAllValueValues.QRCodePalletIndex) continue;

                    // Find entry by QRcode (index 1)
                    if (parts[CaoSuAllValueValues.QRcodeIndex] == qrcode)
                    {
                        // Update: preserve all fields, change Status to "mapped", set QRCodePallet and MappedTime
                        lines[i] =
                            $"{parts[0]},{parts[1]},mapped,{parts[3]},{parts[4]}," +
                            $"{parts[5]},{parts[6]},{parts[7]},{parts[8]}," +
                            $"{mappedDate},{qrCodePallet}";

                        updated = true;
                        break;
                    }
                }

                if (!updated)
                    throw new Exception($"Entry with QRcode '{qrcode}' not found.");

                string tempFile = filePath + ".tmp";

                File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                File.Replace(tempFile, filePath, null);
            }
        }


    }
}

//public static void UpdateVerifyingEntry(string filePath, StorageUpdate storageUpdate, string status)
//{
//    const int maxRetries = 3;
//    const int delayMs = 200;

//    for (int attempt = 1; attempt <= maxRetries; attempt++)
//    {
//        var lines = File.ReadAllLines(filePath).ToList();

//        if (storageUpdate.Id < 1 || storageUpdate.Id > lines.Count)
//            return;

//        var parts = lines[storageUpdate.Id - 1].Split(',');

//        if (storageUpdate.Id - 1 != int.Parse(parts[0]))
//        {
//            CustomMessageBox.Show("Lỗi dữ liệu đồng bộ kiểm tra bị sai Index", "Lỗi Index",
//                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
//            throw new ArgumentOutOfRangeException(nameof(storageUpdate.Id));
//        }


//        lines[storageUpdate.Id - 1] =
//            $"{parts[0]},{parts[1]},{storageUpdate.VerifiedStatus},{storageUpdate.VerifiedDate}" +
//            $",{storageUpdate.SaaSStatus},{storageUpdate.SAPStatus},{storageUpdate.SaaSError},{storageUpdate.SAPError},{status}";

//        string tempFile = filePath + ".tmp";

//        File.WriteAllLines(tempFile, lines, Encoding.UTF8);

//        File.Replace(tempFile, filePath, null);

//        return; // ✅ success, exit method
//        //try
//        //{

//        //}
//        //catch (IOException)
//        //{
//        //}
//    }
//}



//if (storageUpdate.Id - 1 != int.Parse(parts[0]))
//{
//    CustomMessageBox.Show("Lỗi dữ liệu đồng bộ kiểm tra bị sai Index", "Lỗi Index",
//            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
//    throw new ArgumentOutOfRangeException(nameof(storageUpdate.Id));
//}
