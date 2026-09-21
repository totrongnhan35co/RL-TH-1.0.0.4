using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.Droco;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.DrocoUI
{
    public partial class ucDrocoQRFieldMapping : UserControl
    {
        private List<string> _headers = new List<string>();

        private static readonly string[] _kProduct = { "product", "con", "gs1", "item", "san pham", "code_child" };
        private static readonly string[] _kBox = { "box", "hop", "code_box" };
        private static readonly string[] _kCarton = { "carton", "thung", "case", "code_carton" };
        private static readonly string[] _kPallet = { "pallet", "plt", "code_pallet" };

        private bool _isLoading = false;

        public ucDrocoQRFieldMapping()
        {
            InitializeComponent();
            InitEvents();
            LoadSavedConfig();
        }

        // ── Events ────────────────────────────────────────────────────────────

        private void InitEvents()
        {
            chkUseExcelMode.CheckedChanged += ChkUseExcelMode_Changed;
            btnBrowse.Click += BtnBrowse_Click;
            btnSave.Click += BtnSave_Click;
            cboProduct.SelectedIndexChanged += (s, e) => UpdatePreview();
            cboBox.SelectedIndexChanged += (s, e) => UpdatePreview();
            cboCarton.SelectedIndexChanged += (s, e) => UpdatePreview();
            cboPallet.SelectedIndexChanged += (s, e) => UpdatePreview();
        }

        // ── Toggle ────────────────────────────────────────────────────────────

        private void ChkUseExcelMode_Changed(object sender, EventArgs e)
        {
            bool on = chkUseExcelMode.Checked;
            grpMapping.Enabled = on;
            grpPreview.Enabled = on;
            lblModeStatus.Text = on ? "Dang dung QR tu Excel" : "Dang dung sinh tu dong";
            lblModeStatus.ForeColor = on
                ? System.Drawing.Color.FromArgb(40, 167, 69)
                : System.Drawing.Color.FromArgb(100, 100, 100);

            if (!_isLoading)
            {
                Shared.Settings.DrocoQRFieldMapping.UseExcelMode = on;
                Shared.SaveSettings();
            }
        }

        // ── Browse ────────────────────────────────────────────────────────────

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Excel/CSV (*.xlsx;*.xls;*.csv)|*.xlsx;*.xls;*.csv|All files (*.*)|*.*";
                dlg.Title = "Chon file Excel mau de doc cau truc cot";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                lblFilePath.Text = dlg.FileName;
                lblFilePath.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
                try
                {
                    _headers = ReadHeaders(dlg.FileName);
                    PopulateComboBoxes(_headers);
                    AutoDetectAndSelect(_headers);
                    UpdatePreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Loi doc file:\n" + ex.Message +
                        "\n\nGoi y: Dam bao file khong dang mo trong Excel, thu luu lai .xlsx hoac .csv",
                        "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Read headers ──────────────────────────────────────────────────────

        private List<string> ReadHeaders(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            if (ext == ".csv") return ReadCsvHeaders(path);

            bool isXls = false;
            using (var probe = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var h = new byte[4];
                probe.Read(h, 0, 4);
                isXls = (h[0] == 0xD0 && h[1] == 0xCF);
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IExcelDataReader reader = isXls
                    ? ExcelReaderFactory.CreateBinaryReader(stream)
                    : ExcelReaderFactory.CreateOpenXmlReader(stream);

                using (reader)
                {
                    if (!reader.Read()) return new List<string>();
                    var result = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        result.Add(reader.GetValue(i)?.ToString()?.Trim() ?? $"Col{i + 1}");
                    return result;
                }
            }
        }

        private static List<string> ReadCsvHeaders(string path)
        {
            string firstLine = null;
            try { firstLine = File.ReadLines(path, Encoding.UTF8).FirstOrDefault() ?? ""; }
            catch { firstLine = File.ReadLines(path, Encoding.GetEncoding(1252)).FirstOrDefault() ?? ""; }
            char sep = firstLine.Contains(';') ? ';' : ',';
            return firstLine.Split(sep).Select(h => h.Trim('"', ' ')).ToList();
        }

        // ── ComboBox ──────────────────────────────────────────────────────────

        private void PopulateComboBoxes(List<string> headers)
        {
            // item[0] = "(Khong dung)", item[1] = "Cot 1: Ten", item[2] = "Cot 2: Ten"...
            // SelectedIndex == số cột 1-based
            var items = new List<string> { "(Khong dung)" };
            for (int i = 0; i < headers.Count; i++)
                items.Add($"Cot {i + 1}: {headers[i]}");

            foreach (var cbo in new[] { cboProduct, cboBox, cboCarton, cboPallet })
            {
                cbo.Tag = null; // xóa Tag cũ khi load file mới
                cbo.Items.Clear();
                cbo.Items.AddRange(items.ToArray());
                cbo.SelectedIndex = 0;
            }
        }

        private void AutoDetectAndSelect(List<string> headers)
        {
            cboProduct.SelectedIndex = FindBestMatch(headers, _kProduct);
            cboBox.SelectedIndex = FindBestMatch(headers, _kBox);
            cboCarton.SelectedIndex = FindBestMatch(headers, _kCarton);
            cboPallet.SelectedIndex = FindBestMatch(headers, _kPallet);
        }

        private int FindBestMatch(List<string> headers, string[] keywords)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                string h = headers[i].ToLower();
                if (keywords.Any(k => h.Contains(k)))
                    return i + 1; // +1 vì item[0] = placeholder
            }
            return 0;
        }

        // ── Helpers lấy số cột & tên cột ─────────────────────────────────────

        /// <summary>
        /// Trả về số cột 1-based.
        /// - Khi load file mới: SelectedIndex trực tiếp = số cột (vì item[0]=placeholder)
        /// - Khi restore từ Settings (Tag): đọc Tag
        /// </summary>
        private static int GetColumnNumber(ComboBox cbo)
        {
            // Restore từ Settings: Tag chứa số cột thật, Items chỉ có 2 phần tử
            if (cbo.Tag is int tagVal && cbo.Items.Count == 2 && cbo.SelectedIndex == 1)
                return tagVal;

            // Load file mới: SelectedIndex == số cột 1-based
            return cbo.SelectedIndex;
        }

        /// <summary>Trả về tên cột (phần sau ": " trong item text).</summary>
        private static string GetColumnName(ComboBox cbo)
        {
            if (cbo.SelectedIndex <= 0) return "";
            string text = cbo.SelectedItem?.ToString() ?? "";
            int idx = text.IndexOf(':');
            return idx >= 0 && idx + 2 < text.Length
                ? text.Substring(idx + 2).Trim()
                : text;
        }

        // ── Preview ───────────────────────────────────────────────────────────

        private void UpdatePreview()
        {
            string Fmt(ComboBox cbo)
            {
                int col = GetColumnNumber(cbo);
                return col <= 0 ? "(chua chon)" : $"Cot {col} [{GetColumnName(cbo)}]";
            }

            lblPreview.Text =
                $"San pham: {Fmt(cboProduct)}   " +
                $"Hop: {Fmt(cboBox)}   " +
                $"Thung: {Fmt(cboCarton)}   " +
                $"Pallet: {Fmt(cboPallet)}";
        }

        // ── Save ──────────────────────────────────────────────────────────────

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Shared.Settings.DrocoQRFieldMapping = new DrocoQRFieldMapping
            {
                UseExcelMode = chkUseExcelMode.Checked,
                ProductColumnIndex = GetColumnNumber(cboProduct),
                BoxColumnIndex = GetColumnNumber(cboBox),
                CartonColumnIndex = GetColumnNumber(cboCarton),
                PalletColumnIndex = GetColumnNumber(cboPallet),
                ProductColumnName = GetColumnName(cboProduct),
                BoxColumnName = GetColumnName(cboBox),
                CartonColumnName = GetColumnName(cboCarton),
                PalletColumnName = GetColumnName(cboPallet),
            };
            Shared.SaveSettings();

            var m = Shared.Settings.DrocoQRFieldMapping;
            MessageBox.Show(
                "Da luu!\n\n" +
                $"Che do   : {(m.UseExcelMode ? "Sinh QR tu Excel" : "Tu dong")}\n" +
                $"San pham : Cot {m.ProductColumnIndex} [{m.ProductColumnName}]\n" +
                $"Hop      : Cot {m.BoxColumnIndex} [{m.BoxColumnName}]\n" +
                $"Thung    : Cot {m.CartonColumnIndex} [{m.CartonColumnName}]\n" +
                $"Pallet   : Cot {m.PalletColumnIndex} [{m.PalletColumnName}]",
                "Luu thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Load saved config ─────────────────────────────────────────────────

        private void LoadSavedConfig()
        {
            _isLoading = true;
            try
            {
                var saved = Shared.Settings.DrocoQRFieldMapping;
                if (saved == null) return;

                chkUseExcelMode.Checked = saved.UseExcelMode;

                RestoreCbo(cboProduct, saved.ProductColumnName, saved.ProductColumnIndex);
                RestoreCbo(cboBox, saved.BoxColumnName, saved.BoxColumnIndex);
                RestoreCbo(cboCarton, saved.CartonColumnName, saved.CartonColumnIndex);
                RestoreCbo(cboPallet, saved.PalletColumnName, saved.PalletColumnIndex);

                if (!string.IsNullOrWhiteSpace(saved.BoxColumnName))
                {
                    lblFilePath.Text = "(Cau hinh da luu tu lan truoc)";
                    lblFilePath.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
                }

                UpdatePreview();
                grpMapping.Enabled = saved.UseExcelMode;
                grpPreview.Enabled = saved.UseExcelMode;
                lblModeStatus.Text = saved.UseExcelMode ? "Dang dung QR tu Excel" : "Dang dung sinh tu dong";
                lblModeStatus.ForeColor = saved.UseExcelMode
                    ? System.Drawing.Color.FromArgb(40, 167, 69)
                    : System.Drawing.Color.FromArgb(100, 100, 100);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void RestoreCbo(ComboBox cbo, string name, int colNumber)
        {
            if (string.IsNullOrWhiteSpace(name) || colNumber <= 0) return;
            cbo.Items.Clear();
            cbo.Items.Add("(Khong dung)");
            cbo.Items.Add($"Cot {colNumber}: {name}"); // format chuẩn để GetColumnName đọc được
            cbo.SelectedIndex = 1;
            cbo.Tag = colNumber; // lưu số cột thật để GetColumnNumber đọc khi SelectedIndex=1 nhưng colNumber!=1
        }
    }
}