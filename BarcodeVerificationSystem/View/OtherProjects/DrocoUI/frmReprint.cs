using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.View.CustomDialogs;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using static BarcodeVerificationSystem.Controller.Shared;

namespace BarcodeVerificationSystem.View.OtherProjects.DrocoUI
{
    public partial class frmReprint : Form
    {
        private readonly JobModel _jobModel;

        public JobModel CurrentJob { get; set; }
        private BindingList<ReprintItem> _reprintList = new BindingList<ReprintItem>();
        private BindingList<ReprintItem> _reprintQrList = new BindingList<ReprintItem>();
        private ManualResetEvent _printResponseEvent = new ManualResetEvent(false);

        private readonly List<string[]> _printedCodeList;

        private volatile bool _lastPrintSuccess = false;
        public frmReprint(JobModel jobModel, List<string[]> printedCodeList)
        {

            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            _jobModel = jobModel;
            _printedCodeList = printedCodeList;
         
            dgvListQrReprint.DataSource = _reprintQrList;
            Shared.OnSerialDeviceReadDataChange += Shared_OnSerialDeviceReadDataChange;
            Shared.OnPrinterDataChange += OnPrinterDataChange;
       ;
            SetColumnWidths(dgvListQrReprint);
            LoadReprintListFromFile();
        }
        private void OnPrinterDataChange(object sender, EventArgs e)
        {
            // Kiểm tra phản hồi từ máy in thường (ForProduct)
            if (sender is PODDataModel podDataModel)
            {
                Debug.WriteLine("Printer response: " + podDataModel.Text);
                // phản hồi thành công là "RYES" hoặc "OK"
                if (podDataModel.Text.Contains("RYES") || podDataModel.Text.Contains("OK"))
                {
                    _lastPrintSuccess = true;
                    _printResponseEvent.Set();
                }
                else
                {
                    _lastPrintSuccess = false;
                    _printResponseEvent.Set();
                }
            }
        }
        private void SetColumnWidths(DataGridView dgv)
        {
            // Đảm bảo DataGridView đã có cột
            if (dgv.Columns.Count == 0) return;

            // Có thể set theo tên hoặc index
            // Căn giữa cột CodeType
            if (dgv.Columns.Contains("CodeType"))
            {
                dgv.Columns["CodeType"].Width = 55;
                //dgv.Columns["CodeType"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dgv.Columns["CodeType"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (dgv.Columns.Contains("CodeValue"))
                dgv.Columns["CodeValue"].Width = 400;
            if (dgv.Columns.Contains("Time"))
                dgv.Columns["Time"].Width = 100;
            if (dgv.Columns.Contains("Status"))
                dgv.Columns["Status"].Width = 100;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

        }
        /// <summary>
        /// Normalize GS1 separator về dạng chuẩn \F để so sánh nhất quán.
        /// </summary>
        private static string NormalizeCode(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // ── [FIX] Strip GS1 AI parentheses: (00)046071... → 00046071... ──────
            var match = System.Text.RegularExpressions.Regex.Match(input, @"^\((\d{2,4})\)(.+)$");
            if (match.Success)
                input = match.Groups[1].Value + match.Groups[2].Value;
            // ─────────────────────────────────────────────────────────────────────

            return input
                .Replace("\u001D", "\\F")
                .Replace("<0x1D>", "\\F")
                .Replace("_x001d_", "\\F")
                .Replace("%1d", "\\F")
                .Replace("/F", "\\F")
                .Replace("|F", "\\F");
        }

        /// <summary>
        /// Nhận diện loại mã QR dựa vào dữ liệu thực từ job (BoxList / DrocoCartonList / DrocoPalletList).
        /// Thay thế cách cũ dùng prefix số (không tương thích với mã Excel SSCC).
        /// </summary>
        private void DetectCodeType(string code, string normalizedCode,
            out string type, out bool isGS1, out bool isQR)
        {
            type = "Không xác định";
            isGS1 = false;
            isQR = false;

            // --- Ưu tiên: kiểm tra QR Hộp ---
            bool isBox = _jobModel.BoxList?.Any(b =>
                string.Equals(b.QrCode, code, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(b.QrCode, normalizedCode, StringComparison.OrdinalIgnoreCase)) == true;
            if (isBox) { type = "QR Hộp"; isQR = true; return; }

            // --- Kiểm tra QR Thùng ---
            bool isCarton = _jobModel.DrocoCartonList?.Any(c =>
                string.Equals(c.QrCode, code, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.QrCode, normalizedCode, StringComparison.OrdinalIgnoreCase)) == true;
            if (isCarton) { type = "QR Thùng"; isQR = true; return; }

            // --- Kiểm tra QR Pallet ---
            bool isPallet = _jobModel.DrocoPalletList?.Any(p =>
                string.Equals(p.QrCode, code, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.QrCode, normalizedCode, StringComparison.OrdinalIgnoreCase)) == true;
            if (isPallet) { type = "QR Pallet"; isQR = true; return; }

            // --- Kiểm tra GS1 (mã sản phẩm trong ProductCodes của hộp) ---
            bool isProductCode = _jobModel.BoxList?.Any(b =>
                b.ProductCodes != null &&
                b.ProductCodes.Any(pc =>
                    string.Equals(pc, code, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeCode(pc), normalizedCode, StringComparison.OrdinalIgnoreCase))) == true;
            if (isProductCode) { type = "GS1"; isGS1 = true; }
        }
        private void Shared_OnSerialDeviceReadDataChange(object sender, EventArgs e)
        {
            if (IsDisposed || !Visible || !IsHandleCreated) return;
            if (!(sender is DetectModel detectModel)) return;

            string code = detectModel.Text?.Trim();
            if (string.IsNullOrEmpty(code)) return;

            string normalizedCode = NormalizeCode(code);

            // Nhận diện loại mã theo cấu trúc job thực tế
            string type;
            bool isGS1, isQR;
            DetectCodeType(code, normalizedCode, out type, out isGS1, out isQR);

            bool isQrTab = false;
            Invoke(new Action(() => isQrTab = tabRePrint.SelectedTab == tabRePrintQr));

            // ── Tab QR: in lại Hộp / Thùng / Pallet ─────────────────────────────
            if (isQrTab)
            {
                if (!isQR)
                {
                    Invoke(new Action(() =>
                        CustomMessageBox.Show(
                            $"Mã vừa quét không phải QR Hộp/Thùng/Pallet hoặc không thuộc job này!\nLoại nhận diện: {type}",
                            "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                    return;
                }

                DialogResult confirm = DialogResult.No;
                Invoke(new Action(() =>
                {
                    confirm = CustomMessageBox.Show(
                        $"Bạn có muốn in lại mã {type}:\n{code}",
                        "Xác nhận in lại", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }));
                if (confirm != DialogResult.Yes) return;
                if (Shared.IsZebraPrinterReady())
                // if (Shared.Settings.ZebraPrinter.PODController.IsConnected())
                {
                    QRType qrType;
                    switch (type)
                    {
                        case "QR Thùng": qrType = QRType.Carton; break;
                        case "QR Pallet": qrType = QRType.Pallet; break;
                        default: qrType = QRType.Box; break;
                    }

                    // ── [FIX] Dùng normalizedCode (đã strip `()`) thay vì code gốc ──
                    Shared.PrintZebraDroco(normalizedCode, qrType);
                    WriteReprintLog($"Đã in lại mã: {normalizedCode} | Loại: {type}");

                    Invoke(new Action(() =>
                    {
                        _reprintQrList.Insert(0, new ReprintItem
                        {
                            CodeType = type,
                            CodeValue = code,
                            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Status = "Đã gửi in lại"
                        });
                        SaveReprintListToFile();
                        DesignUI.CuzAlert.CuzAlert.Show(
                            $"In lại thành công: {code}",
                            DesignUI.CuzAlert.Alert.enmType.Success,
                            new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                    }));
                }
                else
                {
                    WriteReprintLog($"Lỗi khi in lại mã: {code} | Loại: {type} | Zebra chưa kết nối");
                    Invoke(new Action(() =>
                    {
                        _reprintQrList.Insert(0, new ReprintItem
                        {
                            CodeType = type,
                            CodeValue = code,
                            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Status = "Lỗi máy in"
                        });
                        CustomMessageBox.Show("Máy in Zebra chưa kết nối!", "Cảnh báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                }
                return;
            }

            // ── Tab GS1: in lại mã sản phẩm ─────────────────────────────────────
            if (!isGS1) return;

            string codeCopy = code;
            string normalizedCopy = normalizedCode;
            string typeCopy = type;

            Task.Run(() =>
            {
                bool printSuccess = false;
                var productPrinter = Shared.Settings.PrinterList
                    .FirstOrDefault(p => p.RoleOfPrinter == RoleOfStation.ForProduct);

                if (productPrinter?.PODController != null && productPrinter.PODController.IsConnected())
                {
                    // Tìm dòng dữ liệu trong PrintedCodeList, normalize trước khi so sánh
                    string[] dataRow = _printedCodeList?.FirstOrDefault(row =>
                        row.Skip(2).Any(field =>
                            string.Equals(field, codeCopy, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(NormalizeCode(field), normalizedCopy, StringComparison.OrdinalIgnoreCase)));

                    if (dataRow != null)
                    {
                        string command = "DATA;";
                        var arrSkip = dataRow.Skip(2).ToArray();

                        if (Shared.Settings.PrintFieldForVerifyAndPrint.Count() == 0)
                        {
                            command += string.Join(Shared.Settings.SplitCharacter.ToString(),
                                arrSkip.Select(x => x ?? Shared.Settings.FailedDataSentToPrinter));
                        }
                        else
                        {
                            command += string.Join(Shared.Settings.SplitCharacter.ToString(),
                                Shared.Settings.PrintFieldForVerifyAndPrint
                                    .Where(x => x.Index < arrSkip.Length + 1)
                                    .Select(x => arrSkip[x.Index - 1] ?? Shared.Settings.FailedDataSentToPrinter));
                        }

                        _printResponseEvent.Reset();
                        _lastPrintSuccess = false;
                        productPrinter.PODController.Send(command);
                        printSuccess = _printResponseEvent.WaitOne(5000) && _lastPrintSuccess;
                    }
                    else
                    {
                        WriteReprintLog($"Không tìm thấy dữ liệu trong database cho mã: {codeCopy}");
                    }
                }

                WriteReprintLog(printSuccess
                    ? $"Đã in lại mã: {codeCopy} | Loại: {typeCopy}"
                    : $"Lỗi khi in lại mã: {codeCopy} | Loại: {typeCopy}");

                if (IsDisposed) return;
                Invoke(new Action(() =>
                {
                    _reprintQrList.Insert(0, new ReprintItem
                    {
                        CodeType = typeCopy,
                        CodeValue = codeCopy,
                        Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = printSuccess ? "Đã gửi in lại" : "Lỗi máy in"
                    });
                    SaveReprintListToFile();

                    if (printSuccess)
                    {
                        DesignUI.CuzAlert.CuzAlert.Show(
                            $"In lại thành công: {codeCopy}",
                            DesignUI.CuzAlert.Alert.enmType.Success,
                            new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                    }
                    else
                    {
                        CustomMessageBox.Show(
                            $"In lại thất bại hoặc máy in không phản hồi!\nMã: {codeCopy}",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            });
        }
        private void LoadReprintListFromFile()
        {
            try
            {
                string baseDirectory = @"C:\ProgramData\R-Link";
                string logDirectory = System.IO.Path.Combine(baseDirectory, "Reprint");
                string jobName = _jobModel?.FileName ?? "UnknownJob";
                //string fileGs1 = System.IO.Path.Combine(logDirectory, $"ReprintList_GS1_{jobName}.txt");
                string fileQr = System.IO.Path.Combine(logDirectory, $"ReprintList_QR_{jobName}.txt");

                // Load GS1
                //if (System.IO.File.Exists(fileGs1))
                //{
                //    var lines = System.IO.File.ReadAllLines(fileGs1, Encoding.UTF8);
                //    foreach (var line in lines)
                //    {
                //        var parts = line.Split('|');
                //        if (parts.Length == 4)
                //        {
                //            _reprintList.Add(new ReprintItem
                //            {
                //                CodeType = parts[0],
                //                CodeValue = parts[1],
                //                Time = parts[2],
                //                Status = parts[3]
                //            });
                //        }
                //    }
                //}

                // Load QR
                if (System.IO.File.Exists(fileQr))
                {
                    var lines = System.IO.File.ReadAllLines(fileQr, Encoding.UTF8);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('|');
                        if (parts.Length == 4)
                        {
                            _reprintQrList.Add(new ReprintItem
                            {
                                CodeType = parts[0],
                                CodeValue = parts[1],
                                Time = parts[2],
                                Status = parts[3]
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadReprintListFromFile failed: " + ex.Message);
            }
        }
        private void SaveReprintListToFile()
        {
            try
            {
                string baseDirectory = @"C:\ProgramData\R-Link";
                string logDirectory = System.IO.Path.Combine(baseDirectory, "Reprint");
                if (!System.IO.Directory.Exists(logDirectory))
                    System.IO.Directory.CreateDirectory(logDirectory);

                string jobName = _jobModel?.FileName ?? "UnknownJob";
                // Lưu mã sản phẩm (GS1)
                string fileGs1 = System.IO.Path.Combine(logDirectory, $"ReprintList_GS1_{jobName}.txt");
                var linesGs1 = _reprintList.Select(x => $"{x.CodeType}|{x.CodeValue}|{x.Time}|{x.Status}");
                System.IO.File.WriteAllLines(fileGs1, linesGs1, Encoding.UTF8);

                // Lưu mã QR
                string fileQr = System.IO.Path.Combine(logDirectory, $"ReprintList_QR_{jobName}.txt");
                var linesQr = _reprintQrList.Select(x => $"{x.CodeType}|{x.CodeValue}|{x.Time}|{x.Status}");
                System.IO.File.WriteAllLines(fileQr, linesQr, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveReprintListToFile failed: " + ex.Message);
            }
        }
        private void WriteReprintLog(string message)
        {
            try
            {
                // 1. Xác định thư mục log
                string baseDirectory = @"C:\ProgramData\R-Link";
                string logDirectory = System.IO.Path.Combine(baseDirectory, "Reprint");

                // 2. Tạo thư mục nếu chưa tồn tại
                if (!System.IO.Directory.Exists(logDirectory))
                {
                    System.IO.Directory.CreateDirectory(logDirectory);
                }

                // 3. Đặt tên file log theo ngày
                string fileName = $"Reprint_{DateTime.Now:yyyy-MM-dd}.txt";
                string filePath = System.IO.Path.Combine(logDirectory, fileName);

                // 4. Ghi log với timestamp chi tiết
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                System.IO.File.AppendAllText(filePath, logEntry, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteReprintLog failed: " + ex.Message);
            }
        }
    }

    // Model cho DataGridView
    public class ReprintItem
    {
        public string CodeType { get; set; }
        public string CodeValue { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
    }
}
