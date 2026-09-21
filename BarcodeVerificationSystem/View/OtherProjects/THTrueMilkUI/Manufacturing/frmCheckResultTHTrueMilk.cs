// Copy từ frmCheckedResult.cs — chỉ dùng cho THTrueMilk
// Thay FrmMain _frmParent → frmMainTHTrueMilk _frmParent

using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.ExportData;
using BarcodeVerificationSystem.Utils.ExportData.models;
using BarcodeVerificationSystem.View.CustomDialogs;
using CommonVariable;
using DesignUI.CuzAlert;
using DesignUI.CuzUI;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Spreadsheet;
using NSAX.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing
{
    public partial class FrmCheckedResultTHTrueMilk : Form
    {
        // ── Chỉ khác frmCheckedResult ở chỗ này ──────────────────────────
        public frmMainTHTrueMilk _frmParent = null;
        // ─────────────────────────────────────────────────────────────────

        private Thread _ThreadUpdateCheckedResult;
        public int _TotalColumns = 0;
        public IList<string[]> _CheckedResult = new List<string[]>();
        public ConcurrentDictionary<string, CompareStatus> _CheckedData = new ConcurrentDictionary<string, CompareStatus>();
        public IList<string[]> _CodeData = new List<string[]>();
        public IList<string> _ColumnNames = new List<string>();
        public List<PODModel> _PODFormat = new List<PODModel>();
        public string _JobName = "";
        public bool _IsAfterProduction = false;
        public bool _IsRSeries = false;
        public int _TotalCode = 0;
        public int _TotalChecked = 0;
        public int _NumberOfCheckedPassed = 0;
        public int _NumberOfCheckedFailed = 0;
        public int _NumberOfPrinted = 0;
        private List<string> _ImageNameList = null;
        public string _FillValue = "All";
        private int _CurrentPage = 1;
        private int _PagesCount = 1;
        private readonly int _PageRows = 25;
        private bool _isPopulatingCombo = false;
        private bool _allowClose = false;
        // private readonly string[] _FilterFailed = new string[] { "All", "Valid", "Invalided", "Duplicated", "Null", "Unknown/Missed", "Failed", "Camera", "Barcode Scanner" };
        private readonly string[] _FilterFailed = new string[] { "Tất cả", "Đúng", "Sai" };
        private const int CS_DropShadow = 0x00020000;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle = CS_DropShadow;
                return cp;
            }
        }

        public FrmCheckedResultTHTrueMilk()
        {
            InitializeComponent();
            //this.Resize += FrmCheckedResult_Resize;
        }

        //private void FrmCheckedResult_Resize(object sender, EventArgs e)
        //{
        //    int parentW = grbDatabase.ClientSize.Width;
        //    if (parentW <= 0) return;

        //    // ── Top-right buttons: anchor right ──
        //    btnRefeshDatabase.Left = parentW - 108 - 10;
        //    btnSearch.Left = btnRefeshDatabase.Left - 111 - 5;
        //    // RePrint & ReCheck buttons (hidden) stay at their positions
        //    btnRePrint.Left = parentW - 450;  // approximate, hidden anyway
        //    btnReCheck.Left = btnRePrint.Left + 74 + 5;

        //    // ── Search textbox: stretch horizontally ──
        //    txtSearchDatabase.Left = 573;
        //    txtSearchDatabase.Width = parentW - 573 - 10;

        //    // ── Filter panel: stretch horizontally ──
        //    cuzPanel5.Width = btnSearch.Left - cuzPanel5.Left - 10;
        //    if (cuzPanel5.Width < 310) cuzPanel5.Width = 310;
        //    cbxFilter.Width = cuzPanel5.Width - 36;
        //    cuzTextBox1.Width = cuzPanel5.Width - 30;

        //    // ── Export section: anchor right + stretch ──
        //    int exportY = grbDatabase.ClientSize.Height - 60;
        //    btnExportData.Left = parentW - 114 - 10;
        //    int exportMidX = 11 + (parentW - 22 - 114 - 10) / 2;
        //    customExportPanel.Left = 11;
        //    customExportPanel.Width = exportMidX - 11 - 5;
        //    cuzExportType.Left = exportMidX + 5;
        //    cuzExportType.Width = btnExportData.Left - exportMidX - 10;

        //    // ── Paging bar: re-anchor pages to fit ──
        //    if (pnlPaging.ClientSize.Width > 0)
        //    {
        //        int totalRight = 716; // tổng width của tất cả control Dock=Right
        //        lblPagePerTotals.Width = pnlPaging.ClientSize.Width - totalRight - 30;
        //        if (lblPagePerTotals.Width < 200) lblPagePerTotals.Width = 200;
        //    }
        //}

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitControls();
            InitEvents();
            SetLangguage();
        }

        public void Reload(bool forceReload = false)
        {
            _ImageNameList = GetImageNameList();
            txtSearchDatabase.Text = "";
            int index = _FilterFailed.ToList().FindIndex(x => x == _FillValue);
            if (cbxFilter.SelectedIndex != index)
                cbxFilter.SelectedIndex = index;
            else if (forceReload)
                GetNeededDataToUpdateAsync();
        }

        private void SetLangguage()
        {
            lblFormName.Text = Lang.CheckedResult1.ToUpper();
            this.Text = Lang.CheckedResult1.ToUpper();
            lblTotalCodeOfTheJob.Text = Lang.TotalCodeOfTheJob;
            lblNumberOfCodesPrinted.Text = Lang.NumberOfCodesPrinted;
            lblNumberOfFailed.Text = Lang.NumberOfFailedCodes;
            lblNumberOfVerifiedCodes.Text = Lang.NumberOfVerifiedCode;
            lblValidCode.Text = Lang.NumberOfValidCodes;
            btnRePrint.Text = Lang.RePrint;
            btnSearch.Text = Lang.Search;
            btnRefeshDatabase.Text = Lang.Refresh;
            btnReCheck.Text = Lang.Recheck;
            lblFilter.Text = Lang.Filter;
            DefaultExportRad.Text = Lang.DefaultExport;
            btnExportData.Text = Lang.ExportData;
            CustomExportRad.Text = Lang.CustomExport;
        }

        private void InitControls()
        {
            _ImageNameList = GetImageNameList();
            AssignColumnNameToTable(_ColumnNames);
            var exportOptions = new[]
            {
                "Tất cả",
                "Mã đúng",
                "Mã lỗi"
            };

            var selectedJob = Shared.GetJob(Shared.JobNameSelected);
            cuzExportType.Enabled = selectedJob.CompareType == CompareType.Database && !Shared.Settings.ExportOneForAllEnable && Shared.UserPermission.Exports;
            btnExportData.Enabled = cuzExportType.Enabled;

            cbxExportType.Items.AddRange(exportOptions);
            cbxExportType.SelectedIndex = 0;

            cbxFilter.SelectedIndexChanged -= CbxFilter_SelectedIndexChanged;
            for (int i = 0; i < _FilterFailed.Length; i++)
            {
                cbxFilter.Items.Add(_FilterFailed[i]);
                if (_FilterFailed[i] == _FillValue)
                    cbxFilter.SelectedIndex = i;
            }
            cbxFilter.SelectedIndexChanged += CbxFilter_SelectedIndexChanged;
            BeginInvoke(new Action(() => GetNeededDataToUpdateAsync()));
            LoadTemplateNames();
        }

        private void InitEvents()
        {
            cbxFilter.DrawMode = DrawMode.OwnerDrawVariable;
            cbxFilter.Height = 40;
            cbxFilter.DropDownHeight = 150;
            cbxFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxFilter.DrawItem += ComboBoxCustom.MyComboBox_DrawItem;
            cbxFilter.MeasureItem += ComboBoxCustom.Cbo_MeasureItem;

            btnFirst.Click += ToolStripButtonClick;
            btnBack.Click += ToolStripButtonClick;
            Number1.Click += ToolStripButtonClick;
            Number2.Click += ToolStripButtonClick;
            Number3.Click += ToolStripButtonClick;
            Number4.Click += ToolStripButtonClick;
            Number5.Click += ToolStripButtonClick;
            btnNext.Click += ToolStripButtonClick;
            btnLast.Click += ToolStripButtonClick;

            cbxFilter.SelectedIndexChanged += CbxFilter_SelectedIndexChanged;
            btnSearch.Click += ActionResult;
            btnRePrint.Click += ActionResult;
            btnReCheck.Click += ActionResult;
            btnExportData.Click += ActionResult;

            CustomExportRad.CheckedChanged += RadioButton_CheckedChanged;
            DefaultExportRad.CheckedChanged += RadioButton_CheckedChanged;

            btnRefeshDatabase.Click += ActionResult;
            txtSearchDatabase.KeyDown += TxtSearchDatabase_KeyDown;

            FormClosing += FrmPreviewDatabase_FormClosing;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Load += FrmCheckedResult_Load;
            comboBox1.SelectedValueChanged += (s, e) =>
            {
                if (_isPopulatingCombo) return;
                _CurrentPage = comboBox1.SelectedIndex + 1;
                UpdateGridRowCount();
                dgvCheckedResult.Invalidate();
                RefreshPagination();
                dgvCheckedResult.FirstDisplayedScrollingRowIndex = 0;
            };

            DefaultExportRad.Checked = true;
            UIControlsFuncs.SetAbleControls(false, ComboExportTemplates);
            UIControlsFuncs.SetAbleControls(true, cbxExportType);
        }

        private void FrmCheckedResult_Load(object sender, EventArgs e)
        {
            //btnRePrint.Visible = _IsAfterProduction && _IsRSeries;
                btnRePrint.Visible = false;
        }

        private void CbxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetNeededDataToUpdateAsync();
        }

        public void UpdateLabel()
        {
            if (InvokeRequired) { Invoke(new Action(UpdateLabel)); return; }
            int totalChecked = _frmParent?.TotalChecked ?? _TotalChecked;
            int passed = _frmParent?.NumberOfCheckPassed ?? _NumberOfCheckedPassed;
            int failed = totalChecked - passed;
            // int printed = _frmParent?.NumberPrinted ?? _NumberOfPrinted;
            int printed = _frmParent?.TotalRlinkPrinted ?? _NumberOfPrinted;
            lblNumberOfTotalCodeValue.Text = string.Format("{0:N0}", _TotalCode);
            lblNumberOfPrintedValue.Text = string.Format("{0:N0}", printed);
            lblNumberOfTotalCheckValue.Text = string.Format("{0:N0}", totalChecked);
            lblValidCodeValue.Text = string.Format("{0:N0}", passed);
            lblNumberOfFailedCodeValue.Text = string.Format("{0:N0}", failed);
        }

        private List<string> GetImageNameList()
        {
            try
            {
                string folderPath = Shared.Settings.ExportImagePath + "\\" + _JobName;
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                var dir = new DirectoryInfo(folderPath);
                var result = dir.GetFiles("*.bmp").Select(f => f.Name).ToList();
                result.Sort((a, b) => b.CompareTo(a));
                return result;
            }
            catch { return null; }
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e) => SetLangguage();

        private void TxtSearchDatabase_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) GetNeededDataToUpdateAsync();
        }

        private void ActionResult(object sender, EventArgs e)
        {
            if (sender == btnSearch)
            {
                GetNeededDataToUpdateAsync();
            }
            else if (sender == btnRefeshDatabase)
            {
                _CurrentPage = 1;
                Reload(forceReload: true);
            }
            else if (sender == btnRePrint)
            {
                _frmParent?.ReprintAsync();
            }
            else if (sender == btnReCheck)
            {
                _frmParent?.ReCheck();
            }
            else if (sender == btnExportData)
            {
                if (CustomExportRad.Checked) { ExportCustom(); return; }
                _frmParent?.ExportSelectedType(cbxExportType.SelectedIndex);
            }
        }

        private void ToolStripButtonClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (sender == btnBack) _CurrentPage--;
            else if (sender == btnNext) _CurrentPage++;
            else if (sender == btnFirst) _CurrentPage = 1;
            else if (sender == btnLast) _CurrentPage = _PagesCount;
            else _CurrentPage = Convert.ToInt32(btn.Text, CultureInfo.InvariantCulture);

            _CurrentPage = Math.Max(1, Math.Min(_CurrentPage, _PagesCount));
            UpdateGridRowCount();
            dgvCheckedResult.Invalidate();
            RefreshPagination();
            dgvCheckedResult.FirstDisplayedScrollingRowIndex = 0;
        }

        private void UpdateGridRowCount()
        {
            int startIdx = (_CurrentPage - 1) * _PageRows;
            int remaining = _SearchResultList.Count - startIdx;
            dgvCheckedResult.RowCount = Math.Max(0, Math.Min(_PageRows, remaining));
        }

        private void RefreshPagination()
        {
            var items = new Button[] { Number1, Number2, Number3, Number4, Number5 };
            int pageStartIndex = 1;
            if (_PagesCount > 5 && _CurrentPage > 2) pageStartIndex = _CurrentPage - 2;
            if (_PagesCount > 5 && _CurrentPage > _PagesCount - 2) pageStartIndex = _PagesCount - 4;

            for (int i = pageStartIndex; i < pageStartIndex + 5; i++)
            {
                if (i > _PagesCount)
                {
                    items[i - pageStartIndex].Enabled = false;
                    items[i - pageStartIndex].BackColor = Color.White;
                    items[i - pageStartIndex].ForeColor = Color.Black;
                }
                else
                {
                    items[i - pageStartIndex].Enabled = true;
                    items[i - pageStartIndex].Text = i.ToString(CultureInfo.InvariantCulture);
                    items[i - pageStartIndex].BackColor = i == _CurrentPage ? Color.Black : Color.White;
                    items[i - pageStartIndex].ForeColor = i == _CurrentPage ? Color.White : Color.Black;
                }
            }

            if (_PagesCount == 0)
            {
                btnBack.Enabled = btnFirst.Enabled = btnNext.Enabled = btnLast.Enabled = false;
            }
            else
            {
                btnBack.Enabled = btnFirst.Enabled = _CurrentPage != 1;
                btnNext.Enabled = btnLast.Enabled = _CurrentPage != _PagesCount;
            }

            Invoke(new Action(() =>
            {
                // Populate GoToPage combobox
                _isPopulatingCombo = true;
                comboBox1.Items.Clear();
                for (int i = 1; i <= _PagesCount; i++)
                    comboBox1.Items.Add(i);
                if (_CurrentPage <= comboBox1.Items.Count)
                    comboBox1.SelectedIndex = _CurrentPage - 1;
                _isPopulatingCombo = false;

                lblPagePerTotals.Text = $"{Lang.Page} {_CurrentPage} {Lang.Per} {_PagesCount} ({_SearchResultList.Count} {Lang.Items})";
                lblGoToPage.Text = Lang.GoToPage;
                lblGo.Text = Lang.Go;
            }));
        }

        private void FrmPreviewDatabase_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                this.Hide();
                KillThreadUpdateCheckedResult();
                _SearchResultList.Clear();
                _UpdateUICST?.Cancel();
                _UpdateUICST?.Dispose();
                return;
            }
            KillThreadUpdateCheckedResult();
            _SearchResultList.Clear();
            if (_UpdateUICST != null)
            {
                try { _UpdateUICST.Cancel(); } catch (ObjectDisposedException) { }
                try { _UpdateUICST.Dispose(); } catch (ObjectDisposedException) { }
                _UpdateUICST = null;
            }
        }

        public void ForceClose()
        {
            _allowClose = true;
            this.Close();
        }

        private void EnabledWhenLoadDatabase(bool isEnable)
        {
            pnlPaging.Enabled = isEnable;
            pnlDrag.Enabled = isEnable;
            btnRePrint.Enabled = isEnable;
            btnSearch.Enabled = isEnable;
            btnRefeshDatabase.Enabled = isEnable;
        }

        private void KillThreadUpdateCheckedResult()
        {
            if (_ThreadUpdateCheckedResult != null && _ThreadUpdateCheckedResult.IsAlive)
            {
                _ThreadUpdateCheckedResult.Abort();
                _ThreadUpdateCheckedResult = null;
            }
        }

        private void AssignColumnNameToTable(IList<string> values)
        {
            if (InvokeRequired) { Invoke(new Action(() => AssignColumnNameToTable(values))); return; }
            string[] columns = values.ToArray();
            dgvCheckedResult.Columns.Clear();
            dgvCheckedResult.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCheckedResult.ScrollBars = ScrollBars.Vertical; // chỉ cuộn dọc, ngang fill theo width

            int selectedIndex = -1;
            dgvCheckedResult.CellClick += (obj, e) =>
            {
                if (e.RowIndex == -1) return;
                if (e.RowIndex == selectedIndex) { dgvCheckedResult.Rows[e.RowIndex].Selected = false; selectedIndex = -1; }
                else { selectedIndex = e.RowIndex; }
            };

            dgvCheckedResult.CellValueNeeded += (obj, e) =>
            {
                try
                {
                    int count = _SearchResultList.Count;
                    if (e.RowIndex == -1) return;
                    int lineIndex = e.RowIndex + (_CurrentPage - 1) * _PageRows;
                    if (count == 0 || count <= lineIndex) return;
                    int codeIndex = _SearchResultList[lineIndex];
                    if (codeIndex < 0 || codeIndex >= _CheckedResult.Count) return;
                    string cell = "";

                    if (cbxFilter.Text == "Unknown/Missed")
                    {
                        if (e.ColumnIndex == 0) cell = "" + (codeIndex + 1);
                        else if (e.ColumnIndex == 1) cell = ComparisonResult.Missed.ToString();
                        else if (e.ColumnIndex == 2) cell = _frmParent?.GetCompareDataByPODFormat(_CodeData[codeIndex], _PODFormat) ?? "";
                        else if (e.ColumnIndex == 6) cell = "Unknown";
                        else if (e.ColumnIndex == 7) cell = "Unknown";
                    }
                    else
                    {
                        cell = _CheckedResult[codeIndex][e.ColumnIndex];
                    }

                    if (cell == "" && e.ColumnIndex != 2) return;
                    if (e.ColumnIndex != 1)
                    {
                        e.Value = e.ColumnIndex == 2 ? MaskData.MaskString(cell) : (object)cell;
                    }
                    else
                    {
                        if (cell == ComparisonResult.Valid.ToString())
                        {
                            e.Value = Properties.Resources.icons8_done_24px_result;
                        }
                        else
                        {
                            if (cell == ComparisonResult.Duplicated.ToString()) e.Value = Properties.Resources.icon_Duplicated_Barcode;
                            else if (cell == ComparisonResult.Missed.ToString()) e.Value = Properties.Resources.icon_Missed_Barcode;
                            else if (cell == ComparisonResult.Null.ToString()) e.Value = Properties.Resources.icon_CantDetect_Barcode;
                            else e.Value = Properties.Resources.icons8_multiply_20px;

                            if (dgvCheckedResult.Rows[e.RowIndex].Selected)
                            {
                                dgvCheckedResult.Rows[e.RowIndex].Height = 106;
                                string imgIndex = string.Format("{0:D7}", dgvCheckedResult.Rows[e.RowIndex].Cells[0].Value);
                                string imgFileName = _ImageNameList?.Find(x => x.Contains(imgIndex));
                                string path = Shared.Settings.ExportImagePath + "\\" + Shared.JobNameSelected.Split('.')[0] + "\\" + imgFileName;
                                if (imgFileName != null)
                                {
                                    try { var bmp = new Bitmap(Image.FromFile(path), 300, 250); e.Value = bmp; }
                                    catch { e.Value = new Bitmap(100, 100); }
                                }
                                else { e.Value = Properties.Resources.icon_NoImage; }
                            }
                            else
                            {
                                dgvCheckedResult.Rows[e.RowIndex].Height = dgvCheckedResult.RowHeight;
                            }
                        }
                    }
                }
                catch { }
            };

            int tableCodeProductListWidth = dgvCheckedResult.Width - 39;
            for (int index = 0; index < columns.Length; index++)
            {
                if (index == 1)
                {
                    var col = new DataGridViewImageColumn { HeaderText = columns[index], Name = columns[index].Trim(), SortMode = DataGridViewColumnSortMode.NotSortable };
                    col.DefaultCellStyle.NullValue = null;
                    col.MinimumWidth = 110;
                    col.FillWeight = 8;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvCheckedResult.Columns.Add(col);
                }
                else
                {
                    var col = new DataGridViewTextBoxColumn { HeaderText = columns[index], Name = columns[index].Trim(), SortMode = DataGridViewColumnSortMode.NotSortable, MinimumWidth = 60 };
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    switch (index)
                    {
                        case 0: col.MinimumWidth = 60; col.FillWeight = 5; break;     // STT
                        case 2: col.MinimumWidth = 300; col.FillWeight = 20; break;   // QR
                        case 3: col.MinimumWidth = 120; col.FillWeight = 10; break;   // NSX
                        case 4: col.MinimumWidth = 120; col.FillWeight = 10; break;   // HSD
                        case 5: col.MinimumWidth = 80; col.FillWeight = 8; break;     // Batch
                        case 6: col.MinimumWidth = 120; col.FillWeight = 10; break;   // Thời gian xử lý
                        case 7: col.MinimumWidth = 160; col.FillWeight = 12; break;   // Thời gian nhận
                        case 8: col.MinimumWidth = 400; col.FillWeight = 15; break;   // Camera details
                        default: col.MinimumWidth = 80; col.FillWeight = 10; break;
                    }

                    dgvCheckedResult.Columns.Add(col);
                }
            }
            dgvCheckedResult.VirtualMode = true;

            dgvCheckedResult.RowCount = _PageRows;
        }

        private readonly string[] _defaultRecord = { "100000", "Valid", "abcdefghijk123456789abcdefhgh", "01/01/2026", "01/07/2026", "01 01 26", "100", DateTime.Now.ToString(), "raw_camera_frame", "A" };
        private List<int> _SearchResultList = new List<int>();
        CancellationTokenSource _UpdateUICST;

        private async void GetNeededDataToUpdateAsync()
        {
            try
            {
                Invoke(new Action(() => lblPagePerTotals.Text = "Đang tải..."));
                if (_UpdateUICST != null)
                {
                    try { _UpdateUICST.Cancel(); } catch (ObjectDisposedException) { }
                    _UpdateUICST = null;
                }
                _SearchResultList.Clear();
                EnabledWhenLoadDatabase(false);
                string filler = cbxFilter.Text == "Tất cả" ? "" : cbxFilter.Text == "Đúng" ? "valid" : "failed";
                filler = filler.ToLower();
                string keyWork = txtSearchDatabase.Text.ToLower();
                await Task.Run(() => GetNeededDataToUpdate(filler, keyWork));

                _PagesCount = Convert.ToInt32(Math.Ceiling(_SearchResultList.Count * 1.0 / _PageRows));
                UpdateGridRowCount();
                RefreshPagination();
                EnabledWhenLoadDatabase(true);
                dgvCheckedResult.FirstDisplayedScrollingRowIndex = 0;
                dgvCheckedResult.ClearSelection();
                dgvCheckedResult.Invalidate();
                UpdateLabel();
            }
            catch (OperationCanceledException)
            {
                // Bị cancel bởi operation mới — không cập nhật UI để tránh đè kết quả của operation mới
            }
            catch (Exception)
            {
                EnabledWhenLoadDatabase(true);
            }
        }

        private void GetNeededDataToUpdate(string filler, string keyWork)
        {
            _UpdateUICST = new CancellationTokenSource();
            var token = _UpdateUICST.Token;

            if (filler == "missed")
            {
                foreach (var item in _CheckedData)
                {
                    if (!item.Value.Status)
                    {
                        string tmp = _frmParent?.GetCompareDataByPODFormat(_CodeData[item.Value.Index], _PODFormat) ?? "";
                        if (tmp.Contains(keyWork))
                            _SearchResultList.Add(item.Value.Index);
                    }
                    token.ThrowIfCancellationRequested();
                }
                _SearchResultList = _SearchResultList.OrderBy(x => x).ToList();
            }
            else
            {
                int indexResult = frmMainTHTrueMilk.Index_Result;
                bool hasSearch = !string.IsNullOrEmpty(keyWork);
                for (int i = 0; i < _CheckedResult.Count; i++)
                {
                    token.ThrowIfCancellationRequested();

                    string checkedResult = indexResult >= 0 && indexResult < _CheckedResult[i].Length
                        ? _CheckedResult[i][indexResult]
                        : "";

                    if (hasSearch)
                    {
                        string tmp = string.Join("", _CheckedResult[i]).ToLower();
                        if (!tmp.Contains(keyWork))
                            continue;
                    }

                    if (filler == "")
                    {
                        _SearchResultList.Add(i);
                    }
                    else if (filler == "failed")
                    {
                        if (!checkedResult.Equals("Valid", StringComparison.OrdinalIgnoreCase))
                            _SearchResultList.Add(i);
                    }
                    else if (filler == "camera" || filler == "barcode scanner")
                    {
                        // THTrueMilk không có cột Device → bỏ qua filter này
                    }
                    else
                    {
                        if (checkedResult.Equals(filler, StringComparison.OrdinalIgnoreCase))
                            _SearchResultList.Add(i);
                    }
                }
            }
        }

        public void LoadTemplateNames()
        {
            try
            {
                if (!Directory.Exists(CommVariables.PathExportTemplates)) { ComboExportTemplates.Items.Clear(); return; }
                string[] files = Directory.GetFiles(CommVariables.PathExportTemplates, "*.rvis");
                ComboExportTemplates.Items.Clear();
                foreach (string f in files) ComboExportTemplates.Items.Add(Path.GetFileNameWithoutExtension(f));
                if (ComboExportTemplates.Items.Count > 0) ComboExportTemplates.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Lỗi khi tải danh sách mẫu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ComboExportTemplates.Items.Clear();
            }
        }

        private void ExportCustom()
        {
            if (ComboExportTemplates.SelectedItem == null) { CustomMessageBox.Show("Vui lòng chọn một mẫu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            LoadExportSettings(ComboExportTemplates.SelectedItem.ToString());
        }

        public void LoadExportSettings(string templateName)
        {
            string filePath = Path.Combine(CommVariables.PathExportTemplates, $"{templateName}.rvis");
            if (!File.Exists(filePath)) { CustomMessageBox.Show($"Không tìm thấy mẫu: {filePath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            try
            {
                var settings = JsonConvert.DeserializeObject<ExportSettings>(File.ReadAllText(filePath));
                var filterChecked = new FilterChecked(settings.FilterStatus, settings.FilterDevice, settings.FilterStatusDb, settings.FilterDeviceDb);
                ExportSharedEvents.RaiseSendMultiStatusDbEvent(settings.StatusLabelGl);
                ExportSharedEvents.RaiseCustomHeaderDbEvent(settings.CustomHeader);
                ExportSharedEvents.RaiseCheckedResultEvent(settings.StatusLabelResGl);
                ExportSharedEvents.RaiseDeviceFilterCheckResEvent(settings.DeviceCheckedResult);
                ExportSharedEvents.RaiseSampleFilterEvent(settings.SampleFilter);
                ExportSharedEvents.RaiseCheckedResultLbEvent(settings.CheckedExportResult);
                ExportSharedEvents.RaiseHeaderCheckedEvent(settings.CheckedExportHeader);
                ExportSharedEvents.RaiseCheckedHeaderListEvent(settings.CheckedHeaderList);
                ExportSharedEvents.RaiseExportAllProgress(settings.Results);
                ExportSharedEvents.RaiseFilterEvent(filterChecked);
                ExportSharedEvents.RaiseCustomStatusEvent(settings.CustomStatus);
                ExportSharedEvents.RaiseExportModeEvent(settings.ExportMode);
                ExportSharedEvents.RaiseExportTemplateEvent();
            }
            catch (Exception ex) { CustomMessageBox.Show($"Lỗi khi tải mẫu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private bool isProcessing = false;
        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (isProcessing) return;
            isProcessing = true;
            try
            {
                if (sender == CustomExportRad && CustomExportRad.Checked) { DefaultExportRad.Checked = false; UIControlsFuncs.SetAbleControls(true, ComboExportTemplates); UIControlsFuncs.SetAbleControls(false, cbxExportType); }
                else if (sender == DefaultExportRad && DefaultExportRad.Checked) { CustomExportRad.Checked = false; UIControlsFuncs.SetAbleControls(false, ComboExportTemplates); UIControlsFuncs.SetAbleControls(true, cbxExportType); }
            }
            finally { isProcessing = false; }
        }

        private void btnRefeshDatabase_Click(object sender, EventArgs e)
        {

        }
    }
}