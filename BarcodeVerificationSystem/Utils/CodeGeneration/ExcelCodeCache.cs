//using ExcelDataReader;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace BarcodeVerificationSystem.Utils.CodeGeneration
//{
//    public class ExcelCodeCache
//    {
//        private readonly object _lock = new object();
//        private List<string[]> _rows = new List<string[]>();
//        private int _currentRowIndex = 1;

//        public int TotalRows => Math.Max(0, _rows.Count - 1);
//        public int RemainingRows => Math.Max(0, _rows.Count - _currentRowIndex);

//        public void Load(string filePath)
//        {
//            lock (_lock)
//            {
//                _rows.Clear();
//                _currentRowIndex = 1;

//                string ext = Path.GetExtension(filePath).ToLower();
//                if (ext == ".csv")
//                {
//                    char sep = ',';
//                    var lines = File.ReadAllLines(filePath, Encoding.UTF8);
//                    if (lines.Length > 0)
//                        sep = lines[0].Contains(';') ? ';' : ',';
//                    foreach (var line in lines)
//                        _rows.Add(line.Split(sep).Select(c => c.Trim('"', ' ')).ToArray());
//                    return;
//                }

//                // Detect format qua magic bytes
//                bool isXls = false;
//                using (var probe = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
//                {
//                    var h = new byte[4];
//                    probe.Read(h, 0, 4);
//                    isXls = (h[0] == 0xD0 && h[1] == 0xCF); // OLE2 = .xls
//                }

//                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
//                {
//                    IExcelDataReader reader = isXls
//                        ? ExcelReaderFactory.CreateBinaryReader(stream)
//                        : ExcelReaderFactory.CreateOpenXmlReader(stream);

//                    using (reader)
//                    {
//                        while (reader.Read())
//                        {
//                            var row = new string[reader.FieldCount];
//                            for (int i = 0; i < reader.FieldCount; i++)
//                                row[i] = reader.GetValue(i)?.ToString()?.Trim() ?? "";
//                            _rows.Add(row);
//                        }
//                    }
//                }
//            }
//        }

//        public string GetNextCode(int columnIndex)
//        {
//            lock (_lock)
//            {
//                if (_currentRowIndex >= _rows.Count) return null;
//                var row = _rows[_currentRowIndex++];
//                return columnIndex < row.Length ? row[columnIndex] : null;
//            }
//        }

//        public void Reset() { lock (_lock) { _currentRowIndex = 1; } }
//    }
//}


using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BarcodeVerificationSystem.Controller;

namespace BarcodeVerificationSystem.Utils.CodeGeneration
{
    public class ExcelCodeCache
    {
        private readonly object _lock = new object();
        private List<string[]> _rows = new List<string[]>();
        private int _currentRowIndex = 1;
        private int _startRowIndex = 1;

        public int TotalRows => Math.Max(0, _rows.Count - _startRowIndex);
        public int RemainingRows => Math.Max(0, _rows.Count - _currentRowIndex);

        // ── Regex CSV splitter — tôn trọng trường được bao bởi dấu nháy kép ──
        private static readonly Regex _rexCsvComma = new Regex(
            @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))",
            RegexOptions.Compiled);
        private static readonly Regex _rexCsvSemicolon = new Regex(
            @";(?=(?:[^""]*""[^""]*"")*(?![^""]*""))",
            RegexOptions.Compiled);
        // ── Phát hiện separator thông minh: kiểm tra NHẤT QUÁN qua nhiều dòng ──
        // Tránh nhầm ';' trong mã GS1 (vd: 0108801038562476215!";"r93oatd) thành separator
        private static char DetectSeparator(string[] lines)
        {
            int checkCount = 0;
            int semiPerLine = -1;
            bool semiConsistent = true;

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                if (++checkCount > 5) break;

                // Đếm ';' ngoài dấu nháy kép (unquoted)
                int unquotedSemi = 0;
                bool inQuote = false;
                foreach (char c in line)
                {
                    if (c == '"') inQuote = !inQuote;
                    else if (!inQuote && c == ';') unquotedSemi++;
                }

                if (semiPerLine == -1)
                    semiPerLine = unquotedSemi;
                else if (semiPerLine == 0 || semiPerLine != unquotedSemi)
                {
                    semiConsistent = false;
                    break;
                }
            }

            // Chỉ dùng ';' làm separator khi xuất hiện nhất quán (≥1) ở MỌI dòng mẫu
            return (semiConsistent && semiPerLine > 0) ? ';' : ',';
        }

        // ── Dùng chung: đọc tất cả dòng từ file (xlsx/xls/csv/txt) ──
        private static List<string[]> ReadAllRows(string filePath)
        {
            var rows = new List<string[]>();
            string ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".csv" || ext == ".txt")
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                // Dùng DetectSeparator thay vì chỉ kiểm tra lines[0].Contains(';')
                char sep = DetectSeparator(lines);
                var rexSplit = sep == ';' ? _rexCsvSemicolon : _rexCsvComma;

                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    var fields = rexSplit.Split(line)
                        .Select(c => Csv.Unescape(c.Trim()))
                        .ToArray();
                    rows.Add(fields);
                }
                return rows;
            }

            bool isXls = false;
            using (var probe = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var h = new byte[4];
                probe.Read(h, 0, 4);
                isXls = (h[0] == 0xD0 && h[1] == 0xCF);
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IExcelDataReader reader = isXls
                    ? ExcelReaderFactory.CreateBinaryReader(stream)
                    : ExcelReaderFactory.CreateOpenXmlReader(stream);
                using (reader)
                {
                    while (reader.Read())
                    {
                        var row = new string[reader.FieldCount];
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[i] = reader.GetValue(i)?.ToString()?.Trim() ?? "";
                        rows.Add(row);
                    }
                }
            }
            return rows;
        }

        public void Load(string filePath)
        {
            lock (_lock)
            {
                _rows = ReadAllRows(filePath);
                _startRowIndex = 1;
                _currentRowIndex = 1;
            }
        }

        // ── Load không có header (Droco SSCC) ──
        public void LoadNoHeader(string filePath)
        {
            lock (_lock)
            {
                _rows = ReadAllRows(filePath);
                _startRowIndex = 0;
                _currentRowIndex = 0;
            }
        }

        /// <summary>
        /// Khôi phục vị trí đọc khi tiếp tục job cũ.
        /// alreadyUsedCount = tổng số mã đã lấy từ cache trong phiên trước (hộp + thùng).
        /// Gọi ngay sau LoadNoHeader khi resume.
        /// </summary>
        public void Resume(int alreadyUsedCount)
        {
            lock (_lock)
            {
                _currentRowIndex = _startRowIndex + alreadyUsedCount;
                if (_currentRowIndex > _rows.Count)
                    _currentRowIndex = _rows.Count;
            }
        }

        public string GetNextCode(int columnIndex)
        {
            lock (_lock)
            {
                if (_currentRowIndex >= _rows.Count) return null;
                var row = _rows[_currentRowIndex++];
                return columnIndex < row.Length ? row[columnIndex] : null;
            }
        }

        public void Reset() { lock (_lock) { _currentRowIndex = _startRowIndex; } }

        

        /// <summary>
        /// Đọc file nguồn (xlsx/xls/csv/txt), lấy 1 cột chỉ định,
        /// lưu thành file CSV tại đường dẫn đích.
        /// Với file CSV/TXT nguồn: dùng ReadSingleColumnFile để tránh split sai
        /// khi giá trị chứa dấu `,` hay `;`.
        /// </summary>
        /// <summary>
        /// Đọc file nguồn (xlsx/xls/csv/txt), lấy 1 cột chỉ định,
        /// lưu thành file văn bản tại đường dẫn đích.
        /// Mỗi dòng = 1 giá trị thuần — KHÔNG dùng Csv.Escape để tránh thêm dấu nháy kép thừa.
        /// </summary>
        public static void ExportColumnAsCsv(string sourcePath, string destCsvPath,
                                             bool hasHeader, int columnIndex = 0)
        {
            List<string> values;
            string srcExt = Path.GetExtension(sourcePath).ToLowerInvariant();

            if ((srcExt == ".csv" || srcExt == ".txt") && columnIndex == 0)
            {
                // File CSV/TXT 1 cột: đọc từng dòng, KHÔNG split theo dấu phân cách
                values = ReadSingleColumnFile(sourcePath, hasHeader);
            }
            else
            {
                // Excel hoặc multi-column: dùng ReadAllRows như cũ
                var rows = ReadAllRows(sourcePath);
                int startIdx = hasHeader ? 1 : 0;
                values = new List<string>();
                for (int i = startIdx; i < rows.Count; i++)
                {
                    var row = rows[i];
                    string val = columnIndex < row.Length ? row[columnIndex] : "";
                    if (!string.IsNullOrWhiteSpace(val))
                        values.Add(val);
                }
            }

            string dir = Path.GetDirectoryName(destCsvPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Ghi raw value — không Csv.Escape
            // ReadGs1DatabaseFile đọc lại bằng Csv.Unescape (safe cho cả raw lẫn quoted)
            File.WriteAllLines(destCsvPath, values, Encoding.UTF8);
        }

        /// <summary>
        /// Đọc file 1 cột (GS1/SSCC) — mỗi dòng = 1 giá trị.
        /// KHÔNG tách theo dấu `,` hay `;` để tránh split sai khi giá trị chứa các ký tự đó.
        /// Hỗ trợ: xlsx, xls, csv, txt.
        /// </summary>
        public static List<string> ReadSingleColumnFile(string filePath, bool hasHeader = false)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return result;

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".xlsx" || ext == ".xls")
            {
                var rows = ReadAllRows(filePath);
                int start = hasHeader ? 1 : 0;
                for (int i = start; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row != null && row.Length > 0 && !string.IsNullOrWhiteSpace(row[0]))
                        result.Add(row[0].Trim());
                }
            }
            else
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                int start = hasHeader ? 1 : 0;
                for (int i = start; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Unescape phòng trường hợp file cũ còn dạng RFC4180 quoted
                    string code = Csv.Unescape(line.Trim());
                    if (!string.IsNullOrWhiteSpace(code))
                        result.Add(code);
                }
            }
            return result;
        }
        /// <summary>
        /// Đếm số dòng dữ liệu trong file (không đọc vào bộ nhớ cache).
        /// CSV/TXT: dùng ReadSingleColumnFile để tránh split sai khi mã chứa ',' hay ';'
        /// </summary>
        public static int CountRows(string filePath, bool hasHeader = true)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".csv" || ext == ".txt")
                return ReadSingleColumnFile(filePath, hasHeader).Count;

            var rows = ReadAllRows(filePath);
            int start = hasHeader ? 1 : 0;
            return Math.Max(0, rows.Count - start);
        }

        /// <summary>
        /// Đọc tất cả giá trị của 1 cột từ file, trả về danh sách string.
        /// CSV/TXT + columnIndex=0: dùng ReadSingleColumnFile để an toàn với ký tự đặc biệt.
        /// </summary>
        public static List<string> GetColumnValues(string filePath, bool hasHeader, int columnIndex = 0)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            // Single-column CSV/TXT: đọc từng dòng, KHÔNG split theo separator
            if ((ext == ".csv" || ext == ".txt") && columnIndex == 0)
                return ReadSingleColumnFile(filePath, hasHeader);

            var rows = ReadAllRows(filePath);
            int startIdx = hasHeader ? 1 : 0;
            var values = new List<string>();
            for (int i = startIdx; i < rows.Count; i++)
            {
                var row = rows[i];
                if (columnIndex < row.Length && !string.IsNullOrWhiteSpace(row[columnIndex]))
                    values.Add(row[columnIndex].Trim());
            }
            return values;
        }
    }
}