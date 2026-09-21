using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.ExportData.models;
using BarcodeVerificationSystem.Utils.ExportData;
using CommonVariable;
using DesignUI.CuzAlert;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;
using DesignUI.CuzUI;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.View.DrocoUI;

namespace BarcodeVerificationSystem.View.DrocoUI
{
    public partial class FrmCheckedResultDroco : Form
    {
        public FrmMainDroco _frmParent = null;
        private Thread _ThreadUpdateCheckedResult;
        public int _TotalColumns = 0;
        public List<string[]> _CheckedResult = new List<string[]>();
        public ConcurrentDictionary<string, CompareStatus> _CheckedData = new ConcurrentDictionary<string, CompareStatus>();
        public List<string[]> _CodeData = new List<string[]>();
        public List<string> _ColumnNames = new List<string>();
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
        // fillValue = 0: Load all
        // fillValue = 1: Load passed result
        // fillValue > 1: Load failed
        public string _FillValue = "All";
        //Paging dataGridview
        private int _CurrentPage = 1;
        private int _PagesCount = 1;
        private readonly int _PageRows = 5000;
        private readonly string[] _FilterFailed = new string[] { "All", "Valid", "Invalided", "Duplicated", "Null", "Unknown/Missed", "Failed" };
        // END

        private const int CS_DropShadow = 0x00020000;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ClassStyle = CS_DropShadow;
                return createParams;
            }
        }

        public FrmCheckedResultDroco()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitControls();
            InitEvents();
            SetLangguage();
        }

        public void Reload()
        {
            txtSearchDatabase.Text = "";
            int index = _FilterFailed.ToList().FindIndex(x => x == _FillValue);
            if (cbxFilter.SelectedIndex != index)
            {
                cbxFilter.SelectedIndex = index;
            }
            else
            {
                GetNeededDataToUpdateAsync();
            }
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
                Lang.AllPassedData,
                Lang.PassedDataByCameraScanner,
                Lang.PassedDataByScanner,
                Lang.UnverifiedData,
                Lang.SampledData,
                Lang.FailedData,
                Lang.WaitingData,
                Lang.WaitingData_2
            };

            var _SelectedJob = Shared.GetJob(Shared.JobNameSelected);
            cuzExportType.Enabled = _SelectedJob.CompareType == CompareType.Database && !Shared.Settings.ExportOneForAllEnable && Shared.UserPermission.Exports ? true : false;
            btnExportData.Enabled = _SelectedJob.CompareType == CompareType.Database && !Shared.Settings.ExportOneForAllEnable && Shared.UserPermission.Exports ? true : false;

            cbxExportType.Items.AddRange(exportOptions);
            cbxExportType.SelectedIndex = 0;

            for (int i = 0; i < _FilterFailed.Count(); i++)
            {
                cbxFilter.Items.Add(_FilterFailed[i]);
                if (_FilterFailed[i] == _FillValue)
                {
                    cbxFilter.SelectedIndex = i;
                }
            }
            GetNeededDataToUpdateAsync();
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
            txtSearchDatabase.KeyDown += TxtSearchDatabase_KeyDown; ;

            FormClosing += FrmPreviewDatabase_FormClosing;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Load += FrmCheckedResult_Load;
            comboBox1.SelectedValueChanged += (s, e) =>
            {
                _CurrentPage = (int)comboBox1.SelectedIndex + 1;
                dgvCheckedResult.Invalidate();
                RefreshPagination();
                dgvCheckedResult.FirstDisplayedScrollingRowIndex = 0;
            };

            DefaultExportRad.Checked = true;
            UIControlsFuncs.SetAbleControls(false, ComboExportTemplates);
            UIControlsFuncs.SetAbleControls(true, cbxExportType);
        }

        void visControl()
        {

            UIControlsFuncs.SetAbleControls(CustomExportRad.Checked, ComboExportTemplates);
            UIControlsFuncs.SetAbleControls(DefaultExportRad.Checked, cbxExportType);
        }

        private void FrmCheckedResult_Load(object sender, EventArgs e)
        {
            btnRePrint.Visible = _IsAfterProduction && _IsRSeries;
        }

        private void CbxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetNeededDataToUpdateAsync();
        }

        public void UpdateLabel()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateLabel()));
                return;
            }
            lblNumberOfTotalCodeValue.Text = string.Format("{0:N0}", _TotalCode);
            lblNumberOfPrintedValue.Text = string.Format("{0:N0}", _NumberOfPrinted);
            lblNumberOfTotalCheckValue.Text = string.Format("{0:N0}", _TotalChecked);
            lblValidCodeValue.Text = string.Format("{0:N0}", _NumberOfCheckedPassed);
            lblNumberOfFailedCodeValue.Text = string.Format("{0:N0}", _NumberOfCheckedFailed);
        }

        private List<string> GetImageNameList()
        {
            try
            {
                string folderPath = Shared.Settings.ExportImagePath + "\\" + _JobName;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var dir = new DirectoryInfo(folderPath);
                string strFileNameExtension = string.Format("*{0}", "bmp");
                FileInfo[] files = dir.GetFiles(strFileNameExtension); //Getting Text files
                var result = new List<string>();
                foreach (FileInfo file in files)
                {
                    result.Add(file.Name);
                }
                result.Sort((a, b) => b.CompareTo(a));
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void DataGridViewDatabase_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var dgv = sender as DataGridView;

            e.Graphics.DrawLine(SystemPens.ScrollBar, 0, 0, dgv.Width, 0);
            if (e.RowIndex != -1)
            {
                Rectangle rowRectangle = dgv.GetRowDisplayRectangle(e.RowIndex, true);
                e.Graphics.DrawLine(SystemPens.ScrollBar, rowRectangle.X, rowRectangle.Y, rowRectangle.Width, rowRectangle.Y);
            }
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLangguage();
        }

        private void TxtSearchDatabase_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GetNeededDataToUpdateAsync();
            }
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
                _FillValue = "All";
                Reload();
            }
            else if (sender == btnRePrint)
            {
                if (_frmParent != null)
                    _frmParent.ReprintAsync();
            }
            else if (sender == btnReCheck)
            {
                if (_frmParent != null)
                    _frmParent.ReCheck();
            }
            else if (sender == btnExportData)
            {
                if (CustomExportRad.Checked)
                {
                    ExportCustom();
                    return;
                }

                if (_frmParent != null)
                    _frmParent.ExportSelectedType(cbxExportType.SelectedIndex);
            }

        }

        private void ToolStripButtonClick(object sender, EventArgs e)
        {
            var ToolStripButton = ((Button)sender);
            if (sender == btnBack)
            {
                _CurrentPage--;
            }
            else if (sender == btnNext)
            {
                _CurrentPage++;
            }
            else if (sender == btnFirst)
            {
                _CurrentPage = 1;
            }
            else if (sender == btnLast)
            {
                _CurrentPage = _PagesCount;
            }
            else
            {
                _CurrentPage = Convert.ToInt32(ToolStripButton.Text, CultureInfo.InvariantCulture);
            }

            if (_CurrentPage < 1)
            {
                _CurrentPage = 1;
            }
            else if (_CurrentPage > _PagesCount)
            {
                _CurrentPage = _PagesCount;
            }
            dgvCheckedResult.Invalidate();
            RefreshPagination();
            dgvCheckedResult.FirstDisplayedScrollingRowIndex = 0;
        }

        private void RefreshPagination()
        {
            var items = new Button[] { Number1, Number2, Number3, Number4, Number5 };
            //pageStartIndex contains the first button number of pagination.
            int pageStartIndex = 1;

            if (_PagesCount > 5 && _CurrentPage > 2)
                pageStartIndex = _CurrentPage - 2;

            if (_PagesCount > 5 && _CurrentPage > _PagesCount - 2)
                pageStartIndex = _PagesCount - 4;

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
                    //Changing the page numbers
                    items[i - pageStartIndex].Text = i.ToString(CultureInfo.InvariantCulture);

                    //Setting the Appearance of the page number buttons
                    if (i == _CurrentPage)
                    {
                        items[i - pageStartIndex].BackColor = Color.Black;
                        items[i - pageStartIndex].ForeColor = Color.White;
                    }
                    else
                    {
                        items[i - pageStartIndex].BackColor = Color.White;
                        items[i - pageStartIndex].ForeColor = Color.Black;
                    }
                }

            }

            if (_PagesCount == 0)
            {
                btnBack.Enabled = btnFirst.Enabled = false;
                btnNext.Enabled = btnLast.Enabled = false;
            }
            else
            {
                //Enabling or Disalbing pagination first, last, previous , next buttons
                if (_CurrentPage == 1)
                    btnBack.Enabled = btnFirst.Enabled = false;
                else
                    btnBack.Enabled = btnFirst.Enabled = true;
                if (_CurrentPage == _PagesCount)
                    btnNext.Enabled = btnLast.Enabled = false;
                else
                    btnNext.Enabled = btnLast.Enabled = true;
            }

            Invoke(new Action(() =>
            {
                lblPagePerTotals.Text = $"{Lang.Page} {_CurrentPage} {Lang.Per} {_PagesCount} ({_SearchResultList.Count()} {Lang.Items})";
                lblGoToPage.Text = Lang.GoToPage;
                lblGo.Text = Lang.Go;
            }));
        }

        private void FrmPreviewDatabase_FormClosing(object sender, FormClosingEventArgs e)
        {
            KillThreadUpdateCheckedResult();
            _SearchResultList.Clear();
            _CheckedResult.Clear();
            _UpdateUICST?.Cancel();
            _UpdateUICST.Dispose();
            _CodeData.Clear();
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

        private void AssignColumnNameToTable(List<string> values)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AssignColumnNameToTable(values)));
                return;
            }
            string[] columns = values.ToArray();
            dgvCheckedResult.Columns.Clear();
            dgvCheckedResult.ScrollBars = ScrollBars.Both;
            dgvCheckedResult.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            int selectedIndex = -1;
            dgvCheckedResult.CellClick += (obj, e) =>
            {
                if (e.RowIndex == -1) return;
                if (e.RowIndex == selectedIndex)
                {
                    dgvCheckedResult.Rows[e.RowIndex].Selected = false;
                    selectedIndex = -1;
                }
                else
                {
                    selectedIndex = e.RowIndex;
                }
            };

            dgvCheckedResult.CellValueNeeded += (obj, e) =>
            {
                try
                {
                    int searchResultLinesCount = _SearchResultList.Count();
                    if (e.RowIndex == -1) return;
                    string cell = "";
                    int lineIndex = e.RowIndex + (_CurrentPage - 1) * _PageRows;
                    if (searchResultLinesCount == 0 || searchResultLinesCount <= lineIndex) return;
                    int codeIndex = _SearchResultList[lineIndex];
                    if (cbxFilter.Text == "Unknown/Missed")
                    {
                        if (e.ColumnIndex == 0)
                        {
                            cell = "" + (codeIndex + 1);
                        }
                        else if (e.ColumnIndex == 1)
                        {
                            cell = _frmParent.GetCompareDataByPODFormat(_CodeData[codeIndex], _PODFormat);
                        }
                        else if (e.ColumnIndex == 2)
                        {
                            cell = ComparisonResult.Missed.ToString();
                        }
                        else if (e.ColumnIndex == 3)
                        {
                            cell = "Unknown";
                        }
                        else if (e.ColumnIndex == 4)
                        {
                            cell = "Unknown";
                        }
                    }
                    else
                    {
                        cell = lineIndex < _CheckedResult.Count() ? _CheckedResult[codeIndex][e.ColumnIndex] : "";
                    }

                    if (cell == "" && e.ColumnIndex != 1) return;
                    if (e.ColumnIndex != 2)
                    {
                        if (e.ColumnIndex == 1)
                        {
                            string dataValue = MaskData.MaskString(cell);
                            e.Value = dataValue;
                        }
                        else
                            e.Value = cell;
                    }
                    else
                    {
                        if (cell == ComparisonResult.Valid.ToString())
                        {
                            e.Value = Properties.Resources.icons8_done_24px_result;
                        }
                        else
                        {
                            if (cell == ComparisonResult.Duplicated.ToString())
                            {
                                e.Value = Properties.Resources.icon_Duplicated_Barcode;
                            }
                            else if (cell == ComparisonResult.Missed.ToString())
                            {
                                e.Value = Properties.Resources.icon_Missed_Barcode;
                            }
                            else if (cell == ComparisonResult.Null.ToString())
                            {
                                e.Value = Properties.Resources.icon_CantDetect_Barcode;
                            }
                            else
                            {
                                e.Value = Properties.Resources.icons8_multiply_20px;
                            }

                            if (dgvCheckedResult.Rows[e.RowIndex].Selected == true)
                            {
                                dgvCheckedResult.Rows[e.RowIndex].Height = 106;
                                string[] txt = string.Format("{0:D7}", dgvCheckedResult.Rows[e.RowIndex].Cells[0].Value.ToString()).Split(',');
                                string imgIndex = "";
                                foreach (string s in txt)
                                {
                                    imgIndex += s;
                                }
                                int imgLenght = imgIndex.Length;
                                for (int i = 0; i < 7 - imgLenght; i++)
                                {
                                    imgIndex = "0" + imgIndex;
                                }
                                string imgFileName = _ImageNameList.Find(x => x.Contains(imgIndex));
                                string path = Shared.Settings.ExportImagePath + "\\" + Shared.JobNameSelected.Split('.')[0] + "\\" + imgFileName;
                                if (imgFileName != null)
                                {
                                    try
                                    {
                                        var bmp = new Bitmap(Image.FromFile(path), 300, 250);
                                        e.Value = bmp;
                                        bmp = null;
                                    }
                                    catch
                                    {
                                        var bmp = new Bitmap(100, 100);
                                        e.Value = bmp;
                                        bmp = null;
                                    }
                                }
                                else
                                {
                                    e.Value = Properties.Resources.icon_NoImage;
                                }
                            }
                            else
                            {
                                dgvCheckedResult.Rows[e.RowIndex].Height = dgvCheckedResult.RowHeight;
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
            };

            int tableWidth = dgvCheckedResult.Width;
            var percentWidth = (float)1 / columns.Length;
            int tableCodeProductListWidth = dgvCheckedResult.Width - 39;
            for (int index = 0; index < columns.ToArray().Length; index++)
            {
                if (index == 2)
                {
                    var col = new DataGridViewImageColumn
                    {
                        HeaderText = columns[index],
                        Name = columns[index].Trim()
                    };
                    col.DefaultCellStyle.NullValue = null;
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    if (index == 0)
                    {
                        col.Width = (int)(0.75 * tableCodeProductListWidth);
                    }
                    Size textSize = TextRenderer.MeasureText(col.HeaderText, dgvCheckedResult.Font);
                    col.Width = textSize.Width + 40;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvCheckedResult.Columns.Add(col);
                }
                else
                {
                    var col = new DataGridViewTextBoxColumn
                    {
                        HeaderText = columns[index],
                        Name = columns[index].Trim(),
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    if (index == 0)
                    {
                        col.Width = (int)(0.75 * tableCodeProductListWidth);
                    }
                    Size textSize = TextRenderer.MeasureText(col.HeaderText, dgvCheckedResult.Font);
                    col.Width = textSize.Width + 25;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvCheckedResult.Columns.Add(col);
                    dgvCheckedResult.VirtualMode = true;
                }
            }

            FrmMainDroco.AutoResizeColumnWith(dgvCheckedResult, defaultRecord, 2);
            DataGridViewCustom.AdjustColumnWidthsToFitContent(dgvCheckedResult);
            SetCheckedResultColumnWidths(dgvCheckedResult);
            dgvCheckedResult.RowCount = _PageRows;
        }

        private void SetCheckedResultColumnWidths(DataGridView dgv)
        {
            // Reset MinimumWidth — đây là nguyên nhân cột bị lock rộng
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.MinimumWidth = 5;
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // [0] Index — hẹp cố định
            if (dgv.Columns.Count > 0)
                dgv.Columns[0].Width = 70;

            // [2] Result (image icon) — cố định
            if (dgv.Columns.Count > 2)
                dgv.Columns[2].Width = 70;

            // [3] ProcessingTime — vừa
            if (dgv.Columns.Count > 3)
                dgv.Columns[3].Width = 120;

            // [4] DateTime — vừa
            if (dgv.Columns.Count > 4)
                dgv.Columns[4].Width = 150;

            // [1] ResultData — Fill, chiếm phần còn lại
            if (dgv.Columns.Count > 1)
            {
                dgv.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgv.Columns[1].MinimumWidth = 80;
            }
        }

        private readonly string[] defaultRecord = new string[] { "100000", "abcdefghijk123456789abcdefhgh", "Valid", "100", DateTime.Now.ToString() };

        private List<int> _SearchResultList = new List<int>();

        CancellationTokenSource _UpdateUICST;

        private async void GetNeededDataToUpdateAsync()
        {
            Invoke(new Action(() =>
            {
                lblPagePerTotals.Text = "";
            }));
            _UpdateUICST?.Cancel();
            _SearchResultList.Clear();
            EnabledWhenLoadDatabase(false);
            string filler = cbxFilter.Text == "All" ? "" : cbxFilter.Text == "Unknown/Missed" ? ComparisonResult.Missed.ToString() : cbxFilter.Text;
            string keyWork = txtSearchDatabase.Text.ToLower();
            await Task.Run(() => { GetNeededDataToUpdate(filler, keyWork); });

            _PagesCount = Convert.ToInt32(Math.Ceiling(_SearchResultList.Count * 1.0 / _PageRows));
            RefreshPagination();
            EnabledWhenLoadDatabase(true);
            dgvCheckedResult.FirstDisplayedScrollingRowIndex = 0;
            dgvCheckedResult.ClearSelection();
            dgvCheckedResult.Invalidate();
            UpdateLabel();
        }

        private void GetNeededDataToUpdate(string filler, string keyWork)
        {
            try
            {
                _UpdateUICST = new CancellationTokenSource();
                var token = _UpdateUICST.Token;
                if (filler == ComparisonResult.Missed.ToString())
                {
                    _SearchResultList.Clear();
                    foreach (KeyValuePair<string, CompareStatus> item in _CheckedData)
                    {
                        if (!item.Value.Status)
                        {
                            string tmp = _frmParent.GetCompareDataByPODFormat(_CodeData[item.Value.Index], _PODFormat);
                            if (tmp.ToLower().Contains(keyWork))
                            {
                                _SearchResultList.Add(item.Value.Index);
                            }
                        }
                        token.ThrowIfCancellationRequested();
                    }
                    _SearchResultList = _SearchResultList.OrderBy(x => x).ToList();
                }
                else
                {
                    for (int i = 0; i < _CheckedResult.Count(); i++)
                    {
                        string tmp = string.Join("", _CheckedResult[i]).ToLower();
                        string checkedResult = _CheckedResult[i][2];//_frmParent.Index_Result

                        // Check if the row matches the search keyword (empty keyword matches all)
                        bool matchesSearch = string.IsNullOrEmpty(keyWork) || tmp.Contains(keyWork);
                        
                        if (matchesSearch)
                        {
                            if (string.IsNullOrEmpty(filler))
                            {
                                // Show all
                                _SearchResultList.Add(i);
                            }
                            else if (filler.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                            {
                                // Show all non-Valid results
                                if (!string.IsNullOrEmpty(checkedResult) && 
                                    !checkedResult.Equals(ComparisonResult.Valid.ToString(), StringComparison.OrdinalIgnoreCase))
                                {
                                    _SearchResultList.Add(i);
                                }
                            }
                            else
                            {
                                // Match specific result type (case-insensitive)
                                if (!string.IsNullOrEmpty(checkedResult) && 
                                    checkedResult.Equals(filler, StringComparison.OrdinalIgnoreCase))
                                {
                                    _SearchResultList.Add(i);
                                }
                            }
                        }

                        token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _SearchResultList.Clear();
                _PagesCount = Convert.ToInt32(Math.Ceiling(_SearchResultList.Count * 1.0 / _PageRows));
                Invoke(new Action(() =>
                {
                    RefreshPagination();
                    EnabledWhenLoadDatabase(true);
                    dgvCheckedResult.FirstDisplayedScrollingRowIndex = 0;
                    dgvCheckedResult.ClearSelection();
                    dgvCheckedResult.Invalidate();
                    UpdateLabel();
                }));
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        public void LoadTemplateNames()
        {
            try
            {
                if (!Directory.Exists(CommVariables.PathExportTemplates))
                {
                    //MessageBox.Show("Thư mục chứa mẫu không tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ComboExportTemplates.Items.Clear();
                    return;
                }

                string[] templateFiles = Directory.GetFiles(CommVariables.PathExportTemplates, "*.rvis");

                ComboExportTemplates.Items.Clear();

                foreach (string filePath in templateFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    ComboExportTemplates.Items.Add(fileName);
                }

                if (ComboExportTemplates.Items.Count > 0)
                {
                    ComboExportTemplates.SelectedIndex = 0;
                }
                else
                {
                    CustomMessageBox.Show("Không tìm thấy mẫu nào trong thư mục.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Lỗi khi tải danh sách mẫu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ComboExportTemplates.Items.Clear();
            }
        }

        private void ExportCustom()
        {
            if (ComboExportTemplates.SelectedItem == null)
            {
                CustomMessageBox.Show("Vui lòng chọn một mẫu từ danh sách.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string templateName = ComboExportTemplates.SelectedItem.ToString();
            LoadExportSettings(templateName);
        }

        public void LoadExportSettings(string templateName)
        {
            string filePath = Path.Combine(CommVariables.PathExportTemplates, $"{templateName}.rvis");

            if (!File.Exists(filePath))
            {
                CustomMessageBox.Show($"Không tìm thấy mẫu: {filePath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var settings = JsonConvert.DeserializeObject<ExportSettings>(json);

                // Create FilterChecked object with all four properties
                var filterChecked = new FilterChecked(
                    sts: settings.FilterStatus,
                    dev: settings.FilterDevice,
                    filterStatusDb: settings.FilterStatusDb,
                    filterDeviceDb: settings.FilterDeviceDb
                );

                // Raise events with loaded settings
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
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Lỗi khi tải mẫu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool isProcessing = false;

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (isProcessing) return;
            isProcessing = true;
            try
            {
                if (sender == CustomExportRad && CustomExportRad.Checked)
                {
                    DefaultExportRad.Checked = false;
                    UIControlsFuncs.SetAbleControls(true, ComboExportTemplates);
                    UIControlsFuncs.SetAbleControls(false, cbxExportType);
                }
                else if (sender == DefaultExportRad && DefaultExportRad.Checked)
                {
                    CustomExportRad.Checked = false;
                    UIControlsFuncs.SetAbleControls(false, ComboExportTemplates);
                    UIControlsFuncs.SetAbleControls(true, cbxExportType);
                }
            }
            finally
            {
                isProcessing = false;
            }
        }
    }
}
