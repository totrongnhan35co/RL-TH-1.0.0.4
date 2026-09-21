//using BarcodeVerificationSystem.Controller;
//using BarcodeVerificationSystem.Model;
//using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;
//using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
//using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
//using BarcodeVerificationSystem.Utils;
//using CommonVariable;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco
//{
//    public class DrocoAllValueProcess
//    {
//        private readonly string _filePath;
//        private readonly object _fileLock = new object();
//        private readonly string _databasePath;
//        private string productCode;
//        private string production_batch_code;
//        private double weight;
//        char _us = '\x1F';

//        public string getFilePath()
//        {
//            ProjectLogger.WriteInfo("[DrocoAllValueProcess] getFilePath called: " + _filePath);
//            return _filePath;
//        }

//        public DrocoAllValueProcess(JobModel jobModel)
//        {
//            string fileName = jobModel.FileName +"_"+ jobModel.BatchNumber + "_AllValues" + ".csv";
//            string sentAllValuePath = CommVariables.PathAllValues + fileName;
//            if (!Directory.Exists(CommVariables.PathAllValues))
//            {
//                Directory.CreateDirectory(CommVariables.PathAllValues);
//                ProjectLogger.WriteInfo("[DrocoAllValueProcess] Created directory: " + CommVariables.PathAllValues);
//            }
//            _filePath = sentAllValuePath;
//            _databasePath = jobModel.DirectoryDatabase;
//            productCode = jobModel.CaoSuProduct?.product_code ?? "";
//            production_batch_code = jobModel?.LOTNumber ?? "";
//            weight = jobModel.productWeight;

//            if (!File.Exists(_filePath))
//            {
//                File.Create(_filePath).Dispose();
//                ProjectLogger.WriteInfo("[DrocoAllValueProcess] Created new AllValues file: " + _filePath);
//                InitializeDefaultDatabaseFormat();
//            }
//            else
//            {
//                ProjectLogger.WriteInfo("[DrocoAllValueProcess] Using existing AllValues file: " + _filePath);
//            }
//        }

//        public void InitializeDefaultDatabaseFormat()
//        {
//            try
//            {
//                lock (_fileLock)
//                {
//                    ProjectLogger.WriteInfo("[DrocoAllValueProcess] Initializing default database format for: " + _filePath);

//                    const string us = "\x1F";
//                    var lines = new List<string>();

//                    string extension = Path.GetExtension(_databasePath);
//                    if (!string.IsNullOrEmpty(extension))
//                        extension = extension.ToLowerInvariant();

//                    List<string[]> dataRows = new List<string[]>();

//                    if (extension == ".xlsx" || extension == ".xls")
//                    {
//                        dataRows = FileFuncs.ReadExcelData(_databasePath, null);
//                    }
//                    else
//                    {
//                        // Đọc từng dòng, tách theo dấu tab hoặc dấu phân cách nếu có
//                        string[] allLines = File.ReadAllLines(_databasePath, Encoding.UTF8);
//                        foreach (string line in allLines)
//                        {
//                            if (string.IsNullOrWhiteSpace(line)) continue;
//                            // Ưu tiên tách theo tab, nếu không thì lấy nguyên dòng
//                            var arr = line.Contains("\t") ? line.Split('\t') : new string[] { line };
//                            dataRows.Add(arr);
//                        }
//                    }

//                    var newLines = new List<string>();
//                    newLines.Add(string.Join(us,
//                        "Index", "QRcode", "Status", "Created Time", "Printed Time", "Checked Time",
//                        "Mapped Time", "QRCode Box", "QRCode Carton", "QRCode Pallet"));

//                    for (int i = 0; i < dataRows.Count; i++)
//                    {
//                        var row = dataRows[i];
//                        if (row == null || row.Length == 0 || string.IsNullOrWhiteSpace(row[0]))
//                            continue;

//                        string qrcode = row[0].Trim();
//                        // Nếu có cột thời gian tạo (giả sử cột 1 hoặc 2), lấy giá trị đó, nếu không thì lấy DateTime.Now
//                        string createdTime = (row.Length > 1 && DateTime.TryParse(row[1], out _)) ? row[1] :
//                                            (row.Length > 2 && DateTime.TryParse(row[2], out _)) ? row[2] :
//                                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

//                        string[] fields = new string[]
//                        {
//                    (i + 1).ToString(), qrcode, "created", createdTime,
//                    "", "", "", "", "", ""
//                        };

//                        newLines.Add(string.Join(us, fields));
//                    }

//                    File.WriteAllLines(_filePath, newLines, new UTF8Encoding(true));
//                    ProjectLogger.WriteInfo("[DrocoAllValueProcess] Default database format initialized with " + newLines.Count + " lines.");
//                }
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in InitializeDefaultDatabaseFormat: " + ex.ToString());
//            }
//        }

//        public void UpdatePrint(string qrcode, string printedDate)
//        {
//            try
//            {
//                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdatePrint called for QR: {qrcode}, Date: {printedDate}");
//                FileHelper.UpdateWokaAllValuePrint(_filePath, qrcode, printedDate);
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in UpdatePrint: " + ex.Message);
//            }
//        }

//        public void UpdateCheck(string qrcode, string checkedDate)
//        {
//            try
//            {
//                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdateCheck called for QR: {qrcode}, Date: {checkedDate}");
//                FileHelper.UpdateWokaAllValueCheck(_filePath, qrcode, checkedDate);
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in UpdateCheck: " + ex.Message);
//            }
//        }

//        public void UpdatePallet(string qrcode, string qrCodePallet, string mappedDate)
//        {
//            try
//            {
//                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdatePallet called for QR: {qrcode}, Pallet: {qrCodePallet}, Date: {mappedDate}");
//                FileHelper.UpdateWokaAllValuePallet(_filePath, qrcode, qrCodePallet, mappedDate);
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in UpdatePallet: " + ex.Message);
//            }
//        }
//        public void UpdateBox(string qrcode, string qrCodeBox, string mappedDate)
//        {
//            try
//            {
//                lock (_fileLock)
//                {
//                    var lines = File.ReadAllLines(_filePath).ToList();
//                    for (int i = 1; i < lines.Count; i++)
//                    {
//                        var parts = lines[i].Split(_us);
//                        if (parts.Length > 1 && parts[1] == qrcode)
//                        {
//                            while (parts.Length < 10)
//                            {
//                                Array.Resize(ref parts, parts.Length + 1);
//                                parts[parts.Length - 1] = "";
//                            }
//                            parts[2] = "mapped";
//                            parts[6] = mappedDate; // Cập nhật thời gian mapping
//                            parts[7] = qrCodeBox;
//                            lines[i] = string.Join(_us.ToString(), parts);
//                            break;
//                        }
//                    }
//                    File.WriteAllLines(_filePath, lines, new UTF8Encoding(true));
//                }
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in UpdateBox: " + ex.Message);
//            }
//        }

//        public void UpdateCarton(string qrCodeBox, string qrCodeCarton)
//        {
//            try
//            {
//                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdateCarton called for Box: {qrCodeBox}, Carton: {qrCodeCarton}");
//                lock (_fileLock)
//                {
//                    var lines = File.ReadAllLines(_filePath).ToList();
//                    for (int i = 1; i < lines.Count; i++)
//                    {
//                        var parts = lines[i].Split(_us);
//                        if (parts.Length > 7 && parts[7] == qrCodeBox)
//                        {
//                            while (parts.Length < 10)
//                            {
//                                Array.Resize(ref parts, parts.Length + 1);
//                                parts[parts.Length - 1] = "";
//                            }
//                            parts[8] = qrCodeCarton;
//                            lines[i] = string.Join(_us.ToString(), parts);
//                        }
//                    }
//                    File.WriteAllLines(_filePath, lines, new UTF8Encoding(true));
//                }
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in UpdateCarton: " + ex.Message);
//            }
//        }

//        public void UpdatePalletForCarton(string qrCodeCarton, string qrCodePallet)
//        {
//            try
//            {
//                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdatePalletForCarton called for Carton: {qrCodeCarton}, Pallet: {qrCodePallet}");
//                lock (_fileLock)
//                {
//                    var lines = File.ReadAllLines(_filePath).ToList();
//                    for (int i = 1; i < lines.Count; i++)
//                    {
//                        var parts = lines[i].Split(_us);
//                        if (parts.Length > 8 && parts[8] == qrCodeCarton)
//                        {
//                            while (parts.Length < 10)
//                            {
//                                Array.Resize(ref parts, parts.Length + 1);
//                                parts[parts.Length - 1] = "";
//                            }
//                            parts[9] = qrCodePallet;
//                            lines[i] = string.Join(_us.ToString(), parts);
//                        }
//                    }
//                    File.WriteAllLines(_filePath, lines, new UTF8Encoding(true));
//                }
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in UpdatePalletForCarton: " + ex.Message);
//            }
//        }
//        public void AddNewQrCode(List<string> list_qrcode)
//        {
//            try
//            {
//                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] AddNewQrCode called. Count: {list_qrcode?.Count ?? 0}");
//                lock (_fileLock)
//                {
//                    var lines = File.ReadAllLines(_filePath).ToList();
//                    var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

//                    int lastIndex = 0;
//                    if (lines.Count > 1)
//                    {
//                        for (int i = 1; i < lines.Count; i++)
//                        {
//                            var parts = lines[i].Split(_us);
//                            if (parts.Length > 0 && int.TryParse(parts[CaoSuAllValueValues.Index], out int currentIndex))
//                            {
//                                if (currentIndex > lastIndex)
//                                {
//                                    lastIndex = currentIndex;
//                                }
//                            }
//                        }
//                    }

//                    foreach (var qrcode in list_qrcode)
//                    {
//                        if (string.IsNullOrEmpty(qrcode)) continue;

//                        lastIndex++;
//                        var newLine = $"{lastIndex}{_us}{qrcode}{_us}created{_us}{currentTime}{_us}{_us}{_us}{_us}";
//                        lines.Add(newLine);
//                    }

//                    string tempFile = _filePath + ".tmp";
//                    File.WriteAllLines(tempFile, lines, Encoding.UTF8);
//                    File.Replace(tempFile, _filePath, null);
//                    ProjectLogger.WriteInfo($"[DrocoAllValueProcess] Added {list_qrcode?.Count ?? 0} new QR codes.");

//                }
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in AddNewQrCode: " + ex.Message);
//            }
//        }

//        public RequestSyncOffline GetAllValuePayload()
//        {
//            try
//            {
//                ProjectLogger.WriteInfo("[DrocoAllValueProcess] GetAllValuePayload called.");
//                lock (_fileLock)
//                {
//                    var lines = File.ReadAllLines(_filePath).ToList();
//                    var qrList = new List<QrList>();

//                    for (int i = 1; i < lines.Count; i++)
//                    {
//                        var parts = lines[i].Split(_us);

//                        if (parts.Length < 8) continue;

//                        int indexInLot = 0;
//                        if (!string.IsNullOrEmpty(parts[CaoSuAllValueValues.Index]) &&
//                            !int.TryParse(parts[CaoSuAllValueValues.Index], out indexInLot))
//                        {
//                            continue;
//                        }

//                        var qrItem = new QrList
//                        {
//                            index_in_lot = indexInLot,
//                            qrcode_value = parts[CaoSuAllValueValues.QRcodeIndex] ?? string.Empty,
//                            status = parts[CaoSuAllValueValues.StatusIndex] ?? string.Empty,
//                            product_code = "",
//                            production_batch_code = "",
//                            weight = 0,
//                            created_time = parts.Length > 3 ? (parts[3] ?? string.Empty) : string.Empty,
//                            printed_at = parts.Length > 4 ? (parts[4] ?? string.Empty) : string.Empty,
//                            qr_checked_at = parts.Length > 5 ? (parts[5] ?? string.Empty) : string.Empty,
//                            mapped_at = parts.Length > 6 ? (parts[6] ?? string.Empty) : string.Empty,
//                            qr_pallet = parts.Length > 7 ? (parts[7] ?? string.Empty) : string.Empty
//                        };

//                        qrList.Add(qrItem);
//                    }
//                    ProjectLogger.WriteInfo($"[DrocoAllValueProcess] GetAllValuePayload returning {qrList.Count} items.");
//                    return new RequestSyncOffline
//                    {
//                        qr_list = qrList
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in GetAllValuePayload: " + ex.Message);
//                return new RequestSyncOffline
//                {
//                    qr_list = new List<QrList>()
//                };
//            }
//        }

//        public List<string> GetUnmappedQRCodes()
//        {
//            try
//            {
//                ProjectLogger.WriteInfo("[DrocoAllValueProcess] GetUnmappedQRCodes called.");
//                lock (_fileLock)
//                {
//                    var lines = File.ReadAllLines(_filePath).ToList();
//                    var unmappedCodes = new List<string>();

//                    for (int i = 1; i < lines.Count; i++)
//                    {
//                        var parts = lines[i].Split(_us);

//                        if (parts.Length < 8) continue;

//                        string status = parts.Length > CaoSuAllValueValues.StatusIndex
//                            ? (parts[CaoSuAllValueValues.StatusIndex] ?? string.Empty)
//                            : string.Empty;

//                        if (!status.Equals("mapped", StringComparison.OrdinalIgnoreCase))
//                        {
//                            string qrcode = parts.Length > CaoSuAllValueValues.QRcodeIndex
//                                ? (parts[CaoSuAllValueValues.QRcodeIndex] ?? string.Empty)
//                                : string.Empty;

//                            if (!string.IsNullOrEmpty(qrcode))
//                            {
//                                if (qrcode.Contains("\\F"))
//                                {
//                                    qrcode = qrcode.Replace("\\F", "\x1D");
//                                }
//                                string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(qrcode));
//                                unmappedCodes.Add(base64Code);
//                            }
//                        }
//                    }

//                    return unmappedCodes;
//                }
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in GetUnmappedQRCodes: " + ex.Message);
//                return new List<string>();
//            }
//        }
//    }
//}


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
using System.Text;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco
{
    public enum QRCodeLevel
    {
        Unknown = 0,
        GS1 = 1,
        Box = 2,
        Carton = 3,
        Pallet = 4
    }

    public class DrocoLookupResult
    {
        public bool IsFound { get; set; }
        public string ScannedCode { get; set; }
        public QRCodeLevel Level { get; set; }
        public string LevelLabel { get; set; }
        public string Status { get; set; }
        public string BoxQR { get; set; }
        public string CartonQR { get; set; }
        public string PalletQR { get; set; }

        // Danh sách mã liên kết
        public List<string> LinkedGS1Codes { get; set; }
        public List<string> LinkedBoxQRs { get; set; }
        public List<string> LinkedCartonQRs { get; set; }

        // Alias — dùng trong frmJobDroco ShowLookupResult
        public List<string> ProductCodes { get { return LinkedGS1Codes; } }
        public List<string> BoxCodes { get { return LinkedBoxQRs; } }
        public List<string> CartonCodes { get { return LinkedCartonQRs; } }

        public int TotalGS1 { get { return LinkedGS1Codes != null ? LinkedGS1Codes.Count : 0; } }

        public DrocoLookupResult()
        {
            LinkedGS1Codes = new List<string>();
            LinkedBoxQRs = new List<string>();
            LinkedCartonQRs = new List<string>();
        }
    }

    public class DrocoAllValueProcess
    {
        private readonly string _filePath;
        private readonly object _fileLock = new object();
        private readonly string _databasePath;
        private string productCode;
        private string production_batch_code;
        private double weight;
        char _us = '\x1F';
        private bool _cacheBuilt = false;
        private Dictionary<string, string[]> _gs1Cache;
        private Dictionary<string, List<string[]>> _boxCache;
        private Dictionary<string, List<string[]>> _cartonCache;
        private Dictionary<string, List<string[]>> _palletCache;

        // Chỉ số cột — header: Index|QRcode|Status|Created|Printed|Checked|Mapped|QRCode Box|QRCode Carton|QRCode Pallet
        private const int COL_INDEX = 0;
        private const int COL_QRCODE = 1;
        private const int COL_STATUS = 2;
        private const int COL_CREATED = 3;
        private const int COL_PRINTED = 4;
        private const int COL_CHECKED = 5;
        private const int COL_MAPPED = 6;
        private const int COL_BOX = 7;
        private const int COL_CARTON = 8;
        private const int COL_PALLET = 9;

        public string getFilePath()
        {
            ProjectLogger.WriteInfo("[DrocoAllValueProcess] getFilePath called: " + _filePath);
            return _filePath;
        }

        //public DrocoAllValueProcess(JobModel jobModel, bool readOnly = false)
        //{
        //    string fileName = jobModel.FileName + "_" + jobModel.BatchNumber + "_AllValues" + ".csv";
        //    string sentAllValuePath = CommVariables.PathAllValues + fileName;

        //    if (!Directory.Exists(CommVariables.PathAllValues))
        //    {
        //        Directory.CreateDirectory(CommVariables.PathAllValues);
        //        ProjectLogger.WriteInfo("[DrocoAllValueProcess] Created directory: " + CommVariables.PathAllValues);
        //    }

        //    _filePath = sentAllValuePath;
        //    _databasePath = jobModel.DirectoryDatabase;
        //    productCode = jobModel.CaoSuProduct?.product_code ?? "";
        //    production_batch_code = jobModel?.LOTNumber ?? "";
        //    weight = jobModel.productWeight;

        //    if (!File.Exists(_filePath))
        //    {
        //        //if (readOnly)
        //        //{
        //        //    // Tab Check: không tạo file — lookup methods sẽ trả về IsFound=false
        //        //    ProjectLogger.WriteInfo("[DrocoAllValueProcess] ReadOnly mode, file not found: " + _filePath);
        //        //    return;
        //        //}
        //        File.Create(_filePath).Dispose();
        //        ProjectLogger.WriteInfo("[DrocoAllValueProcess] Created new AllValues file: " + _filePath);
        //        InitializeDefaultDatabaseFormat();
        //    }
        //    else
        //    {
        //        ProjectLogger.WriteInfo("[DrocoAllValueProcess] Using existing AllValues file: " + _filePath);
        //    }
        //}
        public DrocoAllValueProcess(JobModel jobModel)
        {
            string fileName = jobModel.FileName + "_" + jobModel.BatchNumber + "_AllValues" + ".csv";
            string sentAllValuePath = CommVariables.PathAllValues + fileName;
            if (!Directory.Exists(CommVariables.PathAllValues))
            {
                Directory.CreateDirectory(CommVariables.PathAllValues);
                ProjectLogger.WriteInfo("[DrocoAllValueProcess] Created directory: " + CommVariables.PathAllValues);
            }
            _filePath = sentAllValuePath;
            _databasePath = jobModel.DirectoryDatabase;
            productCode = jobModel.CaoSuProduct?.product_code ?? "";
            production_batch_code = jobModel?.LOTNumber ?? "";
            weight = jobModel.productWeight;

            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose();
                ProjectLogger.WriteInfo("[DrocoAllValueProcess] Created new AllValues file: " + _filePath);
                InitializeDefaultDatabaseFormat();
            }
            else
            {
                ProjectLogger.WriteInfo("[DrocoAllValueProcess] Using existing AllValues file: " + _filePath);
            }
        }
        // <summary>
        /// Đọc file GS1 database chuyên dụng — mỗi dòng = 1 mã GS1.
        /// KHÔNG tách theo dấu `,` hay `;` để tránh split sai khi mã chứa các ký tự đó.
        /// Hỗ trợ: xlsx, xls, csv, txt.
        /// </summary>
        private static List<string> ReadGs1DatabaseFile(string filePath)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return result;

            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext == ".xlsx" || ext == ".xls")
            {
                // Excel: ExcelDataReader đọc từng ô — không bị ảnh hưởng bởi ký tự phân cách
                var rows = FileFuncs.ReadExcelData(filePath, null);
                foreach (var row in rows)
                {
                    if (row != null && row.Length > 0 && !string.IsNullOrWhiteSpace(row[0]))
                        result.Add(row[0].Trim());
                }
            }
            else
            {
                // CSV/TXT: mỗi dòng = 1 mã, KHÔNG split theo dấu phân cách bất kỳ
                // Áp dụng Csv.Unescape để xử lý RFC4180 quoting (nếu file được tạo bởi ExportColumnAsCsv)
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string code = Csv.Unescape(line.Trim());
                    if (!string.IsNullOrWhiteSpace(code))
                        result.Add(code);
                }
            }

            return result;
        }


        public void InitializeDefaultDatabaseFormat()
        {
            try
            {
                lock (_fileLock)
                {
                    ProjectLogger.WriteInfo("[DrocoAllValueProcess] Initializing default database format for: " + _filePath);

                    // Dùng hàm đọc chuyên dụng — an toàn với mọi ký tự đặc biệt trong mã GS1
                    List<string> gs1Codes = ReadGs1DatabaseFile(_databasePath);

                    var newLines = new List<string>();
                    newLines.Add(string.Join(_us.ToString(),
                        "Index", "QRcode", "Status", "Created Time", "Printed Time", "Checked Time",
                        "Mapped Time", "QRCode Box", "QRCode Carton", "QRCode Pallet"));

                    string createdTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    for (int i = 0; i < gs1Codes.Count; i++)
                    {
                        string[] fields = new string[]
                        {
                            (i + 1).ToString(), gs1Codes[i], "created", createdTime,
                            "", "", "", "", "", ""
                        };
                        newLines.Add(string.Join(_us.ToString(), fields));
                    }

                    File.WriteAllLines(_filePath, newLines, new UTF8Encoding(true));
                    ProjectLogger.WriteInfo($"[DrocoAllValueProcess] Initialized with {gs1Codes.Count} GS1 codes.");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in InitializeDefaultDatabaseFormat: " + ex.ToString());
            }
        }
        //public void InitializeDefaultDatabaseFormat()
        //{
        //    try
        //    {
        //        lock (_fileLock)
        //        {
        //            ProjectLogger.WriteInfo("[DrocoAllValueProcess] Initializing default database format for: " + _filePath);

        //            const string us = "\x1F";

        //            string extension = Path.GetExtension(_databasePath);
        //            if (!string.IsNullOrEmpty(extension))
        //                extension = extension.ToLowerInvariant();

        //            List<string[]> dataRows = new List<string[]>();

        //            if (extension == ".xlsx" || extension == ".xls")
        //            {
        //                dataRows = FileFuncs.ReadExcelData(_databasePath, null);
        //            }
        //            else
        //            {
        //                string[] allLines = File.ReadAllLines(_databasePath, Encoding.UTF8);
        //                foreach (string line in allLines)
        //                {
        //                    if (string.IsNullOrWhiteSpace(line)) continue;
        //                    string[] arr;
        //                    if (line.Contains("\t"))
        //                        arr = line.Split('\t').Select(f => Csv.Unescape(f.Trim())).ToArray();
        //                    else
        //                        arr = new string[] { Csv.Unescape(line.Trim()) };
        //                    dataRows.Add(arr);
        //                }
        //            }

        //            var newLines = new List<string>();
        //            newLines.Add(string.Join(us,
        //                "Index", "QRcode", "Status", "Created Time", "Printed Time", "Checked Time",
        //                "Mapped Time", "QRCode Box", "QRCode Carton", "QRCode Pallet"));

        //            for (int i = 0; i < dataRows.Count; i++)
        //            {
        //                var row = dataRows[i];
        //                if (row == null || row.Length == 0 || string.IsNullOrWhiteSpace(row[0]))
        //                    continue;

        //                string qrcode = row[0].Trim();
        //                string createdTime = (row.Length > 1 && DateTime.TryParse(row[1], out _)) ? row[1] :
        //                                    (row.Length > 2 && DateTime.TryParse(row[2], out _)) ? row[2] :
        //                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        //                string[] fields = new string[]
        //                {
        //                    (i + 1).ToString(), qrcode, "created", createdTime,
        //                    "", "", "", "", "", ""
        //                };

        //                newLines.Add(string.Join(us, fields));
        //            }

        //            File.WriteAllLines(_filePath, newLines, new UTF8Encoding(true));
        //            ProjectLogger.WriteInfo("[DrocoAllValueProcess] Default database format initialized with " + newLines.Count + " lines.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError("Error in InitializeDefaultDatabaseFormat: " + ex.ToString());
        //    }
        //}

        public void UpdatePrint(string qrcode, string printedDate)
        {
            try
            {
                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdatePrint called for QR: {qrcode}, Date: {printedDate}");
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    bool found = false;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length > COL_QRCODE && parts[COL_QRCODE] == qrcode)
                        {
                            while (parts.Length < 10)
                            {
                                Array.Resize(ref parts, parts.Length + 1);
                                parts[parts.Length - 1] = "";
                            }

                            // === FIX: Chỉ set status = "printed" nếu chưa tiến xa hơn ===
                            // Status progression: created → printed → valid → mapped
                            // Nếu đã là "valid" hoặc "mapped", không được lùi về "printed"
                            string currentStatus = (parts[COL_STATUS] ?? "").Trim().ToLowerInvariant();
                            if (currentStatus != "valid" && currentStatus != "mapped")
                            {
                                parts[COL_STATUS] = "printed";
                            }

                            // Luôn ghi nhận thời gian in (dù status không đổi)
                            parts[COL_PRINTED] = printedDate;
                            lines[i] = string.Join(_us.ToString(), parts);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        ProjectLogger.WriteWarning($"[DrocoAllValueProcess] UpdatePrint: QR '{qrcode}' not found.");

                    string tempFile = _filePath + ".tmp";
                    File.WriteAllLines(tempFile, lines, new UTF8Encoding(true));
                    File.Replace(tempFile, _filePath, null);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdatePrint: " + ex.Message);
            }
        }

        /// <summary>
        /// Batch update printed status for multiple QR codes — reads/writes file only once.
        /// </summary>
        public void UpdatePrintBatch(List<(string qrcode, string printedDate)> updates)
        {
            if (updates == null || updates.Count == 0) return;
            try
            {
                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdatePrintBatch: {updates.Count} updates.");
                var qrToUpdate = new Dictionary<string, string>(updates.Count, StringComparer.Ordinal);
                foreach (var u in updates)
                    qrToUpdate[u.qrcode] = u.printedDate;

                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    int foundCount = 0;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length > COL_QRCODE && qrToUpdate.TryGetValue(parts[COL_QRCODE], out string printedDate))
                        {
                            while (parts.Length < 10)
                                Array.Resize(ref parts, parts.Length + 1);

                            string currentStatus = (parts[COL_STATUS] ?? "").Trim().ToLowerInvariant();
                            if (currentStatus != "valid" && currentStatus != "mapped")
                                parts[COL_STATUS] = "printed";

                            parts[COL_PRINTED] = printedDate;
                            lines[i] = string.Join(_us.ToString(), parts);
                            foundCount++;
                        }
                    }

                    if (foundCount > 0)
                    {
                        string tempFile = _filePath + ".tmp";
                        File.WriteAllLines(tempFile, lines, new UTF8Encoding(true));
                        File.Replace(tempFile, _filePath, null);
                    }

                    if (foundCount < updates.Count)
                        ProjectLogger.WriteWarning($"[DrocoAllValueProcess] UpdatePrintBatch: {updates.Count - foundCount}/{updates.Count} QR codes not found.");
                    else
                        ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdatePrintBatch: Updated {foundCount} codes, 1 file write.");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdatePrintBatch: " + ex.Message);
            }
        }

        public void UpdateCheckBatch(List<(string qrcode, string checkedDate)> updates)
        {
            if (updates == null || updates.Count == 0) return;
            try
            {
                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdateCheckBatch: {updates.Count} updates.");
                var qrToUpdate = new Dictionary<string, string>(updates.Count, StringComparer.Ordinal);
                foreach (var u in updates)
                    qrToUpdate[u.qrcode] = u.checkedDate;

                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    int foundCount = 0;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length > COL_QRCODE && qrToUpdate.TryGetValue(parts[COL_QRCODE], out string checkedDate))
                        {
                            while (parts.Length < 10)
                            {
                                Array.Resize(ref parts, parts.Length + 1);
                                parts[parts.Length - 1] = "";
                            }

                            // Không ghi đè nếu đã mapped (tiến xa hơn valid)
                            string currentStatus = (parts[COL_STATUS] ?? "").Trim().ToLowerInvariant();
                            if (currentStatus == "mapped")
                            {
                                foundCount++;
                                if (foundCount == updates.Count) break;
                                continue;
                            }

                            parts[COL_STATUS] = "valid";
                            parts[COL_CHECKED] = checkedDate;
                            lines[i] = string.Join(_us.ToString(), parts);
                            foundCount++;
                            if (foundCount == updates.Count)
                                break;
                        }
                    }
                    if (foundCount > 0)
                    {
                        File.WriteAllLines(_filePath, lines);
                        ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdateCheckBatch: Updated {foundCount} codes, 1 file write.");
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdateCheckBatch: " + ex.Message);
            }
        }

        public void UpdateCheck(string qrcode, string checkedDate)
        {
            try
            {
                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdateCheck called for QR: {qrcode}, Date: {checkedDate}");
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    bool found = false;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length > COL_QRCODE && parts[COL_QRCODE] == qrcode)
                        {
                            while (parts.Length < 10)
                            {
                                Array.Resize(ref parts, parts.Length + 1);
                                parts[parts.Length - 1] = "";
                            }

                            // Không ghi đè nếu đã mapped (tiến xa hơn valid)
                            string currentStatus = (parts[COL_STATUS] ?? "").Trim().ToLowerInvariant();
                            if (currentStatus != "mapped")
                            {
                                parts[COL_STATUS] = "valid";
                                parts[COL_CHECKED] = checkedDate;
                            }

                            lines[i] = string.Join(_us.ToString(), parts);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        ProjectLogger.WriteWarning($"[DrocoAllValueProcess] UpdateCheck: QR '{qrcode}' not found.");

                    string tempFile = _filePath + ".tmp";
                    File.WriteAllLines(tempFile, lines, new UTF8Encoding(true));
                    File.Replace(tempFile, _filePath, null);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdateCheck: " + ex.Message);
            }
        }

        public void UpdatePallet(string qrcode, string qrCodePallet, string mappedDate)
        {
            try
            {
                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] UpdatePallet called for QR: {qrcode}, Pallet: {qrCodePallet}, Date: {mappedDate}");
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    bool found = false;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length > COL_QRCODE && parts[COL_QRCODE] == qrcode)
                        {
                            while (parts.Length < 10)
                            {
                                Array.Resize(ref parts, parts.Length + 1);
                                parts[parts.Length - 1] = "";
                            }
                            parts[COL_STATUS] = "mapped";
                            parts[COL_MAPPED] = mappedDate;
                            parts[COL_PALLET] = qrCodePallet;
                            lines[i] = string.Join(_us.ToString(), parts);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        ProjectLogger.WriteWarning($"[DrocoAllValueProcess] UpdatePallet: QR '{qrcode}' not found.");

                    string tempFile = _filePath + ".tmp";
                    File.WriteAllLines(tempFile, lines, new UTF8Encoding(true));
                    File.Replace(tempFile, _filePath, null);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdatePallet: " + ex.Message);
            }
        }
        public void UpdateBox(string qrcode, string qrCodeBox, string mappedDate)
        {
            try
            {
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length > COL_QRCODE && parts[COL_QRCODE] == qrcode)
                        {
                            while (parts.Length < 10)
                            {
                                Array.Resize(ref parts, parts.Length + 1);
                                parts[parts.Length - 1] = "";
                            }
                            parts[COL_STATUS] = "mapped";
                            parts[COL_MAPPED] = mappedDate;
                            parts[COL_BOX] = qrCodeBox;
                            lines[i] = string.Join(_us.ToString(), parts);
                            break;
                        }
                    }
                    // ✅ Dùng temp file để tránh hỏng dữ liệu khi crash
                    string tempFile = _filePath + ".tmp";
                    File.WriteAllLines(tempFile, lines, new UTF8Encoding(true));
                    File.Replace(tempFile, _filePath, null);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdateBox: " + ex.Message);
            }
        }
        /// <summary>
        /// Cập nhật BoxQR cho nhiều GS1 code cùng lúc — đọc/ghi file chỉ 1 lần.
        /// </summary>
        public void UpdateBoxBatch(IEnumerable<string> qrcodes, string qrCodeBox, string mappedDate)
        {
            try
            {
                lock (_fileLock)
                {
                    var codeSet = new HashSet<string>(qrcodes);
                    ProjectLogger.WriteInfo(string.Format("[DrocoAllValueProcess] UpdateBoxBatch: {0} codes → Box '{1}', Date: {2}",
               codeSet.Count, qrCodeBox, mappedDate));
                    var lines = File.ReadAllLines(_filePath).ToList();
                    int matchCount = 0;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length > COL_QRCODE && codeSet.Contains(parts[COL_QRCODE]))
                        {
                            while (parts.Length < 10)
                            {
                                Array.Resize(ref parts, parts.Length + 1);
                                parts[parts.Length - 1] = "";
                            }
                            parts[COL_STATUS] = "mapped";
                            parts[COL_MAPPED] = mappedDate;
                            parts[COL_BOX] = qrCodeBox;
                            lines[i] = string.Join(_us.ToString(), parts);
                            matchCount++;
                        }
                    }
                    if (matchCount == 0)
                        ProjectLogger.WriteWarning(string.Format("[DrocoAllValueProcess] UpdateBoxBatch: No match found for Box '{0}'. Codes: [{1}]",
                            qrCodeBox, string.Join(", ", codeSet)));
                    else
                        ProjectLogger.WriteInfo(string.Format("[DrocoAllValueProcess] UpdateBoxBatch: Matched {0}/{1} codes for Box '{2}'.",
                            matchCount, codeSet.Count, qrCodeBox));

                    string tempFile = _filePath + ".tmp";
                    File.WriteAllLines(tempFile, lines, new UTF8Encoding(true));
                    File.Replace(tempFile, _filePath, null);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdateBoxBatch: " + ex.Message);
            }
        }
        public void UpdateCarton(string qrCodeBox, string qrCodeCarton)
        {
            try
            {
          
                ProjectLogger.WriteInfo(string.Format("[DrocoAllValueProcess] UpdateCarton: Box '{0}' → Carton '{1}'", qrCodeBox, qrCodeCarton));
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    int matchCount = 0;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length > COL_BOX && parts[COL_BOX] == qrCodeBox)
                        {
                            while (parts.Length < 10)
                            {
                                Array.Resize(ref parts, parts.Length + 1);
                                parts[parts.Length - 1] = "";
                            }
                            parts[COL_CARTON] = qrCodeCarton;
                            lines[i] = string.Join(_us.ToString(), parts);
                            matchCount++;
                        }
                    }
                    if (matchCount == 0)
                        ProjectLogger.WriteWarning(string.Format("[DrocoAllValueProcess] UpdateCarton: Box '{0}' not found.", qrCodeBox));

                    // ✅ Dùng temp file để tránh corrupt
                    string tempFile = _filePath + ".tmp";
                    File.WriteAllLines(tempFile, lines, new UTF8Encoding(true));
                    File.Replace(tempFile, _filePath, null);
                  
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdateCarton: " + ex.Message);
            }
        }

        public void UpdatePalletForCarton(string qrCodeCarton, string qrCodePallet)
        {
            try
            {
                ProjectLogger.WriteInfo(string.Format("[DrocoAllValueProcess] UpdatePalletForCarton: Carton '{0}' → Pallet '{1}'", qrCodeCarton, qrCodePallet));
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    int matchCount = 0;
                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length > COL_CARTON && parts[COL_CARTON] == qrCodeCarton)
                        {
                            while (parts.Length < 10)
                            {
                                Array.Resize(ref parts, parts.Length + 1);
                                parts[parts.Length - 1] = "";
                            }
                            parts[COL_PALLET] = qrCodePallet;
                            lines[i] = string.Join(_us.ToString(), parts);
                            matchCount++;
                        }
                    }
                    if (matchCount == 0)
                        ProjectLogger.WriteWarning(string.Format("[DrocoAllValueProcess] UpdatePalletForCarton: Carton '{0}' not found.", qrCodeCarton));

                
                    string tempFile = _filePath + ".tmp";
                    File.WriteAllLines(tempFile, lines, new UTF8Encoding(true));
                    File.Replace(tempFile, _filePath, null);
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in UpdatePalletForCarton: " + ex.Message);
            }
        }

        public void AddNewQrCode(List<string> list_qrcode)
        {
            try
            {
                ProjectLogger.WriteInfo($"[DrocoAllValueProcess] AddNewQrCode called. Count: {list_qrcode?.Count ?? 0}");
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    int lastIndex = 0;
                    if (lines.Count > 1)
                    {
                        for (int i = 1; i < lines.Count; i++)
                        {
                            var parts = lines[i].Split(_us);
                            if (parts.Length > COL_INDEX && int.TryParse(parts[COL_INDEX], out int currentIndex))
                            {
                                if (currentIndex > lastIndex)
                                    lastIndex = currentIndex;
                            }
                        }
                    }

                    foreach (var qrcode in list_qrcode)
                    {
                        if (string.IsNullOrEmpty(qrcode)) continue;
                        lastIndex++;
                        // 10 cột: Index|QR|Status|Created|Printed|Checked|Mapped|Box|Carton|Pallet
                        var newLine = string.Format("{0}{1}{2}{1}created{1}{3}{1}{1}{1}{1}{1}{1}",
                            lastIndex, _us, qrcode, currentTime);
                        lines.Add(newLine);
                    }

                    string tempFile = _filePath + ".tmp";
                    File.WriteAllLines(tempFile, lines, Encoding.UTF8);
                    File.Replace(tempFile, _filePath, null);
                    ProjectLogger.WriteInfo($"[DrocoAllValueProcess] Added {list_qrcode?.Count ?? 0} new QR codes.");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in AddNewQrCode: " + ex.Message);
            }
        }

        public RequestSyncOffline GetAllValuePayload()
        {
            try
            {
                ProjectLogger.WriteInfo("[DrocoAllValueProcess] GetAllValuePayload called.");
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    var qrList = new List<QrList>();

                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length < 8) continue;

                        int indexInLot = 0;
                        if (!string.IsNullOrEmpty(parts[COL_INDEX]) &&
                            !int.TryParse(parts[COL_INDEX], out indexInLot))
                            continue;

                        var qrItem = new QrList
                        {
                            index_in_lot = indexInLot,
                            qrcode_value = parts[COL_QRCODE] ?? string.Empty,
                            status = parts[COL_STATUS] ?? string.Empty,
                            product_code = "",
                            production_batch_code = "",
                            weight = 0,
                            created_time = parts.Length > COL_CREATED ? (parts[COL_CREATED] ?? string.Empty) : string.Empty,
                            printed_at = parts.Length > COL_PRINTED ? (parts[COL_PRINTED] ?? string.Empty) : string.Empty,
                            qr_checked_at = parts.Length > COL_CHECKED ? (parts[COL_CHECKED] ?? string.Empty) : string.Empty,
                            mapped_at = parts.Length > COL_MAPPED ? (parts[COL_MAPPED] ?? string.Empty) : string.Empty,
                            qr_pallet = parts.Length > COL_PALLET ? (parts[COL_PALLET] ?? string.Empty) : string.Empty
                        };

                        qrList.Add(qrItem);
                    }

                    ProjectLogger.WriteInfo($"[DrocoAllValueProcess] GetAllValuePayload returning {qrList.Count} items.");
                    return new RequestSyncOffline { qr_list = qrList };
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in GetAllValuePayload: " + ex.Message);
                return new RequestSyncOffline { qr_list = new List<QrList>() };
            }
        }

        public List<string> GetUnmappedQRCodes()
        {
            try
            {
                ProjectLogger.WriteInfo("[DrocoAllValueProcess] GetUnmappedQRCodes called.");
                lock (_fileLock)
                {
                    var lines = File.ReadAllLines(_filePath).ToList();
                    var unmappedCodes = new List<string>();

                    for (int i = 1; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(_us);
                        if (parts.Length < 8) continue;

                        string status = parts.Length > COL_STATUS
                            ? (parts[COL_STATUS] ?? string.Empty)
                            : string.Empty;

                        if (!status.Equals("mapped", StringComparison.OrdinalIgnoreCase))
                        {
                            string qrcode = parts.Length > COL_QRCODE
                                ? (parts[COL_QRCODE] ?? string.Empty)
                                : string.Empty;

                            if (!string.IsNullOrEmpty(qrcode))
                            {
                                if (qrcode.Contains("\\F"))
                                    qrcode = qrcode.Replace("\\F", "\x1D");

                                string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(qrcode));
                                unmappedCodes.Add(base64Code);
                            }
                        }
                    }

                    return unmappedCodes;
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in GetUnmappedQRCodes: " + ex.Message);
                return new List<string>();
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // TAB CHECK — Tra cứu thủ công theo cấp độ đóng gói
        // ═══════════════════════════════════════════════════════════════════

        private string SafeGet(string[] parts, int idx)
        {
            return parts.Length > idx ? (parts[idx] ?? "").Trim() : "";
        }

        /// <summary>
        /// Scan mã GS1 sản phẩm → trả về Hộp, Thùng, Pallet và trạng thái.
        /// </summary>
        // ──── Cache helpers (tăng tốc lookup Mapped tab) ────
        public void BuildCache()
        {
            if (_cacheBuilt) return;
            lock (_fileLock)
            {
                if (_cacheBuilt) return;
                _gs1Cache = new Dictionary<string, string[]>(StringComparer.Ordinal);
                _boxCache = new Dictionary<string, List<string[]>>(StringComparer.Ordinal);
                _cartonCache = new Dictionary<string, List<string[]>>(StringComparer.Ordinal);
                _palletCache = new Dictionary<string, List<string[]>>(StringComparer.Ordinal);

                if (!File.Exists(_filePath)) { _cacheBuilt = true; return; }

                foreach (var line in File.ReadAllLines(_filePath).Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var p = line.Split(_us);
                    if (p.Length <= COL_QRCODE) continue;

                    // GS1 cache: qrcode → full row
                    string gs1 = p[COL_QRCODE].Trim();
                    if (!string.IsNullOrEmpty(gs1) && !_gs1Cache.ContainsKey(gs1))
                        _gs1Cache[gs1] = p;

                    // Box cache: boxQr → list of rows
                    string box = SafeGet(p, COL_BOX);
                    if (!string.IsNullOrEmpty(box))
                    {
                        if (!_boxCache.TryGetValue(box, out var boxList))
                            _boxCache[box] = boxList = new List<string[]>();
                        boxList.Add(p);
                    }

                    // Carton cache: cartonQr → list of rows
                    string carton = SafeGet(p, COL_CARTON);
                    if (!string.IsNullOrEmpty(carton))
                    {
                        if (!_cartonCache.TryGetValue(carton, out var cartonList))
                            _cartonCache[carton] = cartonList = new List<string[]>();
                        cartonList.Add(p);
                    }

                    // Pallet cache: palletQr → list of rows
                    string pallet = SafeGet(p, COL_PALLET);
                    if (!string.IsNullOrEmpty(pallet))
                    {
                        if (!_palletCache.TryGetValue(pallet, out var palletList))
                            _palletCache[pallet] = palletList = new List<string[]>();
                        palletList.Add(p);
                    }
                }
                _cacheBuilt = true;
            }
        }

        /// <summary>
        /// Force rebuild cache từ file — dùng khi cần dữ liệu realtime (tab check/mapped).
        /// </summary>
        public void RebuildCache()
        {
            _cacheBuilt = false;
            BuildCache();
        }

        public DrocoLookupResult LookupProductCode(string scannedCode)
        {
            var result = new DrocoLookupResult { ScannedCode = scannedCode };
            if (string.IsNullOrWhiteSpace(scannedCode)) return result;

            try
            {
                BuildCache();
                lock (_fileLock)
                {
                    if (_gs1Cache.TryGetValue(scannedCode, out var p))
                    {
                        result.IsFound = true;
                        result.Level = QRCodeLevel.GS1;
                        result.LevelLabel = "Mã sản phẩm";
                        result.Status = SafeGet(p, COL_STATUS);
                        result.BoxQR = SafeGet(p, COL_BOX);
                        result.CartonQR = SafeGet(p, COL_CARTON);
                        result.PalletQR = SafeGet(p, COL_PALLET);
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in LookupProductCode: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Scan mã Hộp → trả về Thùng, Pallet và danh sách mã GS1 trong hộp.
        /// </summary>
        public DrocoLookupResult LookupBoxCode(string scannedCode)
        {
            var result = new DrocoLookupResult { ScannedCode = scannedCode };
            if (string.IsNullOrWhiteSpace(scannedCode)) return result;

            try
            {
                BuildCache();
                lock (_fileLock)
                {
                    if (_boxCache.TryGetValue(scannedCode, out var rows))
                    {
                        result.IsFound = true;
                        result.Level = QRCodeLevel.Box;
                        result.LevelLabel = "Mã Hộp";
                        result.BoxQR = scannedCode;
                        foreach (var p in rows)
                        {
                            if (string.IsNullOrEmpty(result.CartonQR))
                                result.CartonQR = SafeGet(p, COL_CARTON);
                            if (string.IsNullOrEmpty(result.PalletQR))
                                result.PalletQR = SafeGet(p, COL_PALLET);
                            string gs1 = SafeGet(p, COL_QRCODE);
                            if (!string.IsNullOrEmpty(gs1) && !result.LinkedGS1Codes.Contains(gs1))
                                result.LinkedGS1Codes.Add(gs1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in LookupBoxCode: " + ex.Message);
            }

            return result;
        }

        public DrocoLookupResult LookupCartonCode(string scannedCode)
        {
            var result = new DrocoLookupResult { ScannedCode = scannedCode };
            if (string.IsNullOrWhiteSpace(scannedCode)) return result;

            try
            {
                BuildCache();
                lock (_fileLock)
                {
                    if (_cartonCache.TryGetValue(scannedCode, out var rows))
                    {
                        result.IsFound = true;
                        result.Level = QRCodeLevel.Carton;
                        result.LevelLabel = "Mã Thùng";
                        result.CartonQR = scannedCode;
                        foreach (var p in rows)
                        {
                            if (string.IsNullOrEmpty(result.PalletQR))
                                result.PalletQR = SafeGet(p, COL_PALLET);
                            string boxQR = SafeGet(p, COL_BOX);
                            if (!string.IsNullOrEmpty(boxQR) && !result.LinkedBoxQRs.Contains(boxQR))
                                result.LinkedBoxQRs.Add(boxQR);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in LookupCartonCode: " + ex.Message);
            }

            return result;
        }

        public DrocoLookupResult LookupPalletCode(string scannedCode)
        {
            var result = new DrocoLookupResult { ScannedCode = scannedCode };
            if (string.IsNullOrWhiteSpace(scannedCode)) return result;

            try
            {
                BuildCache();
                lock (_fileLock)
                {
                    if (_palletCache.TryGetValue(scannedCode, out var rows))
                    {
                        result.IsFound = true;
                        result.Level = QRCodeLevel.Pallet;
                        result.LevelLabel = "Mã Pallet";
                        result.PalletQR = scannedCode;
                        foreach (var p in rows)
                        {
                            string cartonQR = SafeGet(p, COL_CARTON);
                            if (!string.IsNullOrEmpty(cartonQR) && !result.LinkedCartonQRs.Contains(cartonQR))
                                result.LinkedCartonQRs.Add(cartonQR);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in LookupPalletCode: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Tra cứu tổng hợp — tự động xác định cấp độ mã quét.
        /// </summary>
        public DrocoLookupResult LookupQRCode(string scannedCode)
        {
            var result = new DrocoLookupResult { ScannedCode = scannedCode, Level = QRCodeLevel.Unknown };
            if (string.IsNullOrEmpty(scannedCode)) return result;

            try
            {
                lock (_fileLock)
                {
                    if (!File.Exists(_filePath)) return result;

                    string[] lines = File.ReadAllLines(_filePath);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var p = lines[i].Split(_us);
                        if (p.Length < 2) continue;

                        string gs1 = SafeGet(p, COL_QRCODE);
                        string status = SafeGet(p, COL_STATUS);
                        string boxQR = SafeGet(p, COL_BOX);
                        string cartonQR = SafeGet(p, COL_CARTON);
                        string palletQR = SafeGet(p, COL_PALLET);

                        if (!string.IsNullOrEmpty(gs1) && gs1 == scannedCode)
                        {
                            result.IsFound = true;
                            result.Level = QRCodeLevel.GS1;
                            result.LevelLabel = "Mã GS1 sản phẩm";
                            result.Status = status;
                            result.BoxQR = boxQR;
                            result.CartonQR = cartonQR;
                            result.PalletQR = palletQR;
                            return result;
                        }

                        if (!string.IsNullOrEmpty(boxQR) && boxQR == scannedCode)
                        {
                            result.IsFound = true;
                            result.Level = QRCodeLevel.Box;
                            result.LevelLabel = "QR Hộp";
                            result.BoxQR = boxQR;
                            if (string.IsNullOrEmpty(result.CartonQR)) result.CartonQR = cartonQR;
                            if (string.IsNullOrEmpty(result.PalletQR)) result.PalletQR = palletQR;
                            if (!string.IsNullOrEmpty(gs1) && !result.LinkedGS1Codes.Contains(gs1))
                                result.LinkedGS1Codes.Add(gs1);
                        }
                        else if (!string.IsNullOrEmpty(cartonQR) && cartonQR == scannedCode)
                        {
                            result.IsFound = true;
                            result.Level = QRCodeLevel.Carton;
                            result.LevelLabel = "QR Thùng";
                            result.CartonQR = cartonQR;
                            if (string.IsNullOrEmpty(result.PalletQR)) result.PalletQR = palletQR;
                            if (!string.IsNullOrEmpty(gs1) && !result.LinkedGS1Codes.Contains(gs1)) result.LinkedGS1Codes.Add(gs1);
                            if (!string.IsNullOrEmpty(boxQR) && !result.LinkedBoxQRs.Contains(boxQR)) result.LinkedBoxQRs.Add(boxQR);
                        }
                        else if (!string.IsNullOrEmpty(palletQR) && palletQR == scannedCode)
                        {
                            result.IsFound = true;
                            result.Level = QRCodeLevel.Pallet;
                            result.LevelLabel = "QR Pallet";
                            result.PalletQR = palletQR;
                            if (!string.IsNullOrEmpty(gs1) && !result.LinkedGS1Codes.Contains(gs1)) result.LinkedGS1Codes.Add(gs1);
                            if (!string.IsNullOrEmpty(boxQR) && !result.LinkedBoxQRs.Contains(boxQR)) result.LinkedBoxQRs.Add(boxQR);
                            if (!string.IsNullOrEmpty(cartonQR) && !result.LinkedCartonQRs.Contains(cartonQR)) result.LinkedCartonQRs.Add(cartonQR);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in LookupQRCode: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Đếm số mã GS1 đã được mapped vào 1 Box QR cụ thể.
        /// Dùng để verify hộp có đủ số lượng hay không.
        /// </summary>
        public int CountCodesInBox(string boxQrCode)
        {
            RebuildCache();
            if (_boxCache.TryGetValue(boxQrCode, out var rows))
                return rows.Count;
            return 0;
        }

        /// <summary>
        /// Đếm số Box đã được mapped vào 1 Carton QR cụ thể.
        /// </summary>
        public int CountBoxesInCarton(string cartonQrCode)
        {
            RebuildCache();
            if (_cartonCache.TryGetValue(cartonQrCode, out var rows))
                return rows.Count;
            return 0;
        }

        /// <summary>
        /// Đếm số Carton đã được mapped vào 1 Pallet QR cụ thể.
        /// </summary>
        public int CountCartonsInPallet(string palletQrCode)
        {
            RebuildCache();
            if (_palletCache.TryGetValue(palletQrCode, out var rows))
                return rows.Count;
            return 0;
        }
    }
}