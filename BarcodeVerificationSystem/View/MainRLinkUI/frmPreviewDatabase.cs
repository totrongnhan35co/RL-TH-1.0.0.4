using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmPreviewDatabase : Form
    {
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
        public int _TotalColumns = 0;
        public List<string[]> _ObtainCodeList = new List<string[]>();
        public List<string> _DatabaseColunms = new List<string>();
        public int _Totals = 0;
        public int _NumberPrinted = 0;

        private int _CurrentPage = 1;
        private int _PagesCount = 1;
        private readonly int _PageRows = 5000;

        public FrmPreviewDatabase()
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

        private void SetLangguage()
        {
            Text = Lang.previewdatabase;
            lblFormName.Text = Lang.previewdatabase.ToUpper();
            lblPagePerTotals.Text = $"{Lang.Page} {_CurrentPage} {Lang.Per} {_PagesCount} ({_SearchResultList.Count()} {Lang.Items})";
            lblGoToPage.Text = Lang.GoToPage;
            lblGo.Text = Lang.Go;
            lblTotalNumberOfCodesInDatabase.Text = Lang.TotalNumberOfCodeInTheDatabase;
            btnSearch.Text = Lang.Search;
            btnRefeshDatabase.Text = Lang.Refresh;
        }

        private void InitControls()
        {
            AssignColumnNameToTable(_DatabaseColunms, _DatabaseColunms.Count());

            _CurrentPage = 1;

            GetNeededDataToUpdateAsync();

            lblTotalDatabase.Text = string.Format("{0:N0}", _Totals);
        }

        private void InitEvents()
        {
            btnFirst.Click += ToolStripButtonClick;
            btnLast.Click += ToolStripButtonClick;
            btnNext.Click += ToolStripButtonClick;
            btnBack.Click += ToolStripButtonClick;
            Number1.Click += ToolStripButtonClick;
            Number2.Click += ToolStripButtonClick;
            Number3.Click += ToolStripButtonClick;
            Number4.Click += ToolStripButtonClick;
            Number5.Click += ToolStripButtonClick;

            comboBox1.DrawMode = DrawMode.OwnerDrawVariable;
            comboBox1.Height = 30;
            comboBox1.DropDownHeight = 300;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.DrawItem += ComboBoxCustom.MyComboBox_DrawItem;
            comboBox1.MeasureItem += ComboBoxCustom.Cbo_MeasureItem;

            btnSearch.Click += ActionResult;
            btnRefeshDatabase.Click += ActionResult;
            txtSearchDatabase.KeyDown += TxtSearchDatabase_KeyDown;

            FormClosing += FrmPreviewDatabase_FormClosing;
            Shared.OnLanguageChange += Shared_OnLanguageChange;

            comboBox1.SelectedValueChanged += (s, e) =>
            {
                _CurrentPage = (int)comboBox1.SelectedIndex + 1;
                UpdateRowCount();  // ← thêm dòng này
                dgvDatabase.Invalidate();
                RefreshPagination();
                dgvDatabase.FirstDisplayedScrollingRowIndex = 0;
            };
        }

        private void DgvDatabase_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
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
                _CurrentPage = 1;

                GetNeededDataToUpdateAsync();
            }
        }

        private void ActionResult(object sender, EventArgs e)
        {
            if (sender == btnSearch)
            {
                _CurrentPage = 1;
                GetNeededDataToUpdateAsync();
            }
            if (sender == btnRefeshDatabase)
            {
                _CurrentPage = 1;
                txtSearchDatabase.Text = "";
                GetNeededDataToUpdateAsync();
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
            UpdateRowCount();  // ← cập nhật RowCount khi đổi trang
            dgvDatabase.Invalidate();
            RefreshPagination();
            dgvDatabase.FirstDisplayedScrollingRowIndex = 0;
            dgvDatabase.Rows[0].Cells[0].Selected = true;
        }

        private void RefreshPagination()
        {
           var items = new Button[] { Number1, Number2, Number3, Number4, Number5 };
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
                    items[i - pageStartIndex].Text = i.ToString(CultureInfo.InvariantCulture);

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
            _SearchResultList.Clear();
            dgvDatabase.Rows.Clear();
            _ObtainCodeList.Clear();
            _UpdateUICST?.Cancel();
            _UpdateUICST?.Dispose();
            
        }

        private void EnabledWhenLoadDatabase(bool isEnable)
        {
            pnlPaging.Enabled = isEnable;
            btnSearch.Enabled = isEnable;
            btnRefeshDatabase.Enabled = isEnable;
            txtSearchDatabase.Enabled = isEnable;
        }
        private void AssignColumnNameToTable(List<string> values, int imgIndex)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AssignColumnNameToTable(values, imgIndex)));
                return;
            }
            var columns = values.ToArray();
            dgvDatabase.Columns.Clear();

            dgvDatabase.VirtualMode = true;
            dgvDatabase.CellValueNeeded += (obj, e) =>
            {
                try
                {
                    if (e.RowIndex == -1) return;
                    string cell = "";
                    int lineIndex = e.RowIndex + (_CurrentPage - 1) * _PageRows;
                    if (_SearchResultList.Count() == 0 || _SearchResultList.Count() <= lineIndex) return;
                    var codeIndex = _SearchResultList[lineIndex];
                    cell = lineIndex < _ObtainCodeList.Count() ? _ObtainCodeList[codeIndex][e.ColumnIndex] : "";
                    if (cell == "") return;
                    if (e.ColumnIndex != imgIndex)
                    {
                        e.Value = e.ColumnIndex > 1 ? MaskData.MaskString(cell) : cell;
                    }
                    else
                    {
                        switch (cell)
                        {
                            case "Printed":
                                e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_done_24px_result;
                                break;
                            case "Waiting":
                                e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_in_progress_20px_4;
                                break;
                            case "Sent":
                                e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_in_progress_20px_4;
                                break;
                            case "Reprint":
                                e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_done_24px_result;
                                break;
                            case "Duplicate":
                                dgvDatabase.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Red;
                                e.Value = BarcodeVerificationSystem.Properties.Resources.icon_check_241;
                                break;
                        }
                    }
                }
                catch
                {

                }
            };


            try
            {
                for (int index = 0; index < columns.Length; index++)
                {
                    if (index == imgIndex && imgIndex != -1)
                    {
                        var col = new DataGridViewImageColumn
                        {
                            HeaderText = columns[index],
                            Name = columns[index].Trim()

                        };
                        col.DefaultCellStyle.NullValue = null;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                        Size textSize = TextRenderer.MeasureText(col.HeaderText, dgvDatabase.Font);
                        col.Width = textSize.Width + 40;
                        col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvDatabase.Columns.Add(col);

                    }
                    else
                    {

                        var col = new DataGridViewTextBoxColumn
                        {
                            HeaderText = columns[index],
                            Name = columns[index].Trim(),
                            SortMode = DataGridViewColumnSortMode.NotSortable
                        };

                        Size textSize = TextRenderer.MeasureText(col.HeaderText, dgvDatabase.Font);
                        col.Width = textSize.Width + 25;
                        col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvDatabase.Columns.Add(col);
                    }
                }
            }
            catch { }

            if (_ObtainCodeList.Count() > 0)
            {
                var firstRowWith = _ObtainCodeList[0];
                FrmMain.AutoResizeColumnWith(dgvDatabase, firstRowWith, _DatabaseColunms.Count - 1);
            }

            SetPreviewDatabaseColumnWidths(dgvDatabase);  // ← THÊM DÒNG NÀY
            dgvDatabase.RowCount = _PageRows;
        }

        private void SetPreviewDatabaseColumnWidths(DataGridView dgv)
        {
            if (dgv.Columns.Count == 0) return;

            int n = dgv.Columns.Count;

            // Reset tất cả cột về Fill mode
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.MinimumWidth = 5;
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            // [0] Index — 5% width
            dgv.Columns[0].FillWeight = 5f;
            dgv.Columns[0].MinimumWidth = 60;

            // [1..n-2] Cột giữa (Status...) — 10% mỗi cột
            for (int i = 1; i < n - 1; i++)
            {
                dgv.Columns[i].FillWeight = 10f;
                dgv.Columns[i].MinimumWidth = 70;
            }

            // [n-1] Cột Data Content — chiếm phần còn lại
            float lastWeight = Math.Max(30f, 100f - 5f - (n - 2) * 10f);
            dgv.Columns[n - 1].FillWeight = lastWeight;
            dgv.Columns[n - 1].MinimumWidth = 120;

            // Fill toàn bộ grid — không còn blank corner
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        //private void AssignColumnNameToTable(List<string> values, int imgIndex)
        //{
        //    if (InvokeRequired)
        //    {
        //        Invoke(new Action(() => AssignColumnNameToTable(values, imgIndex)));
        //        return;
        //    }
        //    var columns = values.ToArray();
        //    dgvDatabase.Columns.Clear();

        //    dgvDatabase.VirtualMode = true;
        //    dgvDatabase.CellValueNeeded += (obj, e) =>
        //    {
        //        try
        //        {
        //            if (e.RowIndex == -1) return;
        //            string cell = "";
        //            int lineIndex = e.RowIndex + (_CurrentPage - 1) * _PageRows;
        //            if (_SearchResultList.Count() == 0 || _SearchResultList.Count() <= lineIndex) return;
        //            var codeIndex = _SearchResultList[lineIndex];
        //            cell = lineIndex < _ObtainCodeList.Count() ? _ObtainCodeList[codeIndex][e.ColumnIndex] : "";
        //            if (cell == "") return;
        //            if (e.ColumnIndex != imgIndex)
        //            {
        //                e.Value = e.ColumnIndex > 1 ? MaskData.MaskString(cell) : cell;
        //            }
        //            else
        //            {
        //                switch (cell)
        //                {
        //                    case "Printed":
        //                        e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_done_24px_result;
        //                        break;
        //                    case "Waiting":
        //                        e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_in_progress_20px_4;
        //                        break;
        //                    case "Sent":
        //                        e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_in_progress_20px_4;
        //                        break;
        //                    case "Reprint":
        //                        e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_done_24px_result;
        //                        break;
        //                    case "Duplicate":
        //                        dgvDatabase.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Red;
        //                        e.Value = BarcodeVerificationSystem.Properties.Resources.icon_check_241;
        //                        break;
        //                }
        //            }
        //        }
        //        catch
        //        {

        //        }
        //    };


        //    try
        //    {
        //        for (int index = 0; index < columns.Length; index++)
        //        {
        //            if (index == imgIndex && imgIndex != -1)
        //            {
        //                var col = new DataGridViewImageColumn
        //                {
        //                    HeaderText = columns[index],
        //                    Name = columns[index].Trim()

        //                };
        //                col.DefaultCellStyle.NullValue = null;
        //                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        //                Size textSize = TextRenderer.MeasureText(col.HeaderText, dgvDatabase.Font);
        //                col.Width = textSize.Width + 40;
        //                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        //                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        //                dgvDatabase.Columns.Add(col);

        //            }
        //            else
        //            {

        //                var col = new DataGridViewTextBoxColumn
        //                {
        //                    HeaderText = columns[index],
        //                    Name = columns[index].Trim(),
        //                    SortMode = DataGridViewColumnSortMode.NotSortable
        //                };

        //                Size textSize = TextRenderer.MeasureText(col.HeaderText, dgvDatabase.Font);
        //                col.Width = textSize.Width + 25;
        //                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        //                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        //                dgvDatabase.Columns.Add(col);
        //            }
        //        }
        //    }
        //    catch { }

        //    if (_ObtainCodeList.Count() > 0)
        //    {
        //        var firstRowWith = _ObtainCodeList[0];
        //        FrmMain.AutoResizeColumnWith(dgvDatabase, firstRowWith, _DatabaseColunms.Count-1);
        //    }

        //    dgvDatabase.RowCount = _PageRows;
        //}


        private readonly List<int> _SearchResultList = new List<int>();

        CancellationTokenSource _UpdateUICST;
        private void UpdateRowCount()
        {
            int pageStart = (_CurrentPage - 1) * _PageRows;
            int pageRowCount = Math.Max(1, Math.Min(_SearchResultList.Count - pageStart, _PageRows));
            if (dgvDatabase.RowCount != pageRowCount)
                dgvDatabase.RowCount = pageRowCount;
        }
        private async void GetNeededDataToUpdateAsync()
        {
            Invoke(new Action(() =>
            {
                lblPagePerTotals.Text = "";
            }));
            _UpdateUICST?.Cancel();
            _SearchResultList.Clear();
            string keyWork = txtSearchDatabase.Text.ToLower();
            EnabledWhenLoadDatabase(false);

            await Task.Run(() => { GetNeededDataToUpdate(keyWork); });

            _PagesCount = Convert.ToInt32(Math.Ceiling(_SearchResultList.Count * 1.0 / _PageRows));
            UpdateRowCount();  // ← cập nhật RowCount theo dữ liệu thực
            RefreshPagination();
            EnabledWhenLoadDatabase(true);
            dgvDatabase.FirstDisplayedScrollingRowIndex = 0;
            dgvDatabase.ClearSelection();
            dgvDatabase.Invalidate();
        }

        private void GetNeededDataToUpdate(string keyWork)
        {
            try
            {
                _UpdateUICST = new CancellationTokenSource();
                CancellationToken token = _UpdateUICST.Token;

                for (int i = 0; i < _ObtainCodeList.Count(); i++)
                {
                    token.ThrowIfCancellationRequested();
                    if (keyWork == "")
                    {
                        _SearchResultList.Add(i);
                    }
                    else
                    {
                        string tmp = string.Join("", _ObtainCodeList[i]).ToLower();
                        if (tmp.Contains(keyWork))
                        {
                            _SearchResultList.Add(i);
                        }
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
                    dgvDatabase.FirstDisplayedScrollingRowIndex = 0;
                    dgvDatabase.ClearSelection();
                    dgvDatabase.Invalidate();
                }));
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

    }
}
