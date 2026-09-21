using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing
{
    public partial class frmPreviewQrDetail : Form
    {
        private readonly List<string[]> _rows;
        private readonly string _receivedAt;
        private DataTable _allDataTable;
        private DataTable _filteredTable;

        private int _currentPage = 1;
        private const int _pageSize = 15;
        private int _totalPages = 1;

        public frmPreviewQrDetail()
        {
            InitializeComponent();
            _rows = new List<string[]>();
            _receivedAt = "";
            InitEvents();
        }

        public frmPreviewQrDetail(List<string[]> rows, string receivedAt)
        {
            InitializeComponent();
           // MaximizeBox = false;
            InitEvents();
            cmbStatus.SelectedIndex = 0;
            _rows = rows;
            _receivedAt = receivedAt;
            LoadData();
        }

        private void InitEvents()
        {
            btnClose.Click += (s, e) => Close();
            btnPrevious.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; LoadCurrentPage(); UpdatePageInfo(); } };
            btnNext.Click += (s, e) => { if (_currentPage < _totalPages) { _currentPage++; LoadCurrentPage(); UpdatePageInfo(); } };
            btnFirst.Click += (s, e) => { _currentPage = 1; LoadCurrentPage(); UpdatePageInfo(); };
            btnLast.Click += (s, e) => { _currentPage = _totalPages; LoadCurrentPage(); UpdatePageInfo(); };

            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text == "Nhập từ khóa...") { txtSearch.Text = ""; txtSearch.ForeColor = System.Drawing.Color.Black; } };
            txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Nhập từ khóa..."; txtSearch.ForeColor = System.Drawing.Color.Gray; } };
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            cmbStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            ApplyDgvStyle();
        }

        private void ApplyDgvStyle()
        {
            dataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.GridColor = System.Drawing.Color.FromArgb(224, 224, 224);
            dataGridView.ColumnHeadersHeight = 40;
            dataGridView.RowTemplate.Height = 36;

            dataGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = System.Drawing.Color.FromArgb(0, 171, 230),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F),
                ForeColor = System.Drawing.Color.White,
                Padding = new Padding(0, 5, 0, 5),
                SelectionBackColor = System.Drawing.Color.FromArgb(0, 171, 230),
                SelectionForeColor = System.Drawing.SystemColors.HighlightText,
                WrapMode = DataGridViewTriState.False
            };

            dataGridView.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Padding = new Padding(5),
                SelectionBackColor = System.Drawing.Color.FromArgb(66, 165, 245),
                SelectionForeColor = System.Drawing.Color.White,
                WrapMode = DataGridViewTriState.False
            };
        }

        private void LoadData()
        {
            if (_rows == null || _rows.Count == 0) return;

            _allDataTable = new DataTable();
            _allDataTable.Columns.Add("ID", typeof(int));
            _allDataTable.Columns.Add("QR Code", typeof(string));
            _allDataTable.Columns.Add("Trạng thái", typeof(string));
            _allDataTable.Columns.Add("Đã in", typeof(string));
           // _allDataTable.Columns.Add("Batch", typeof(string));
            _allDataTable.Columns.Add("Line", typeof(string));
            _allDataTable.Columns.Add("Thời gian nhận", typeof(string));

            foreach (var r in _rows)
            {
                if (r.Length < 7) continue;
                _allDataTable.Rows.Add(
                    int.TryParse(r[0], out int id) ? id : 0,
                    r[1], r[2], r[3], r[5], DateTime.TryParse(r[6], out DateTime dt)
                                                ? dt.ToString("yyyy-MM-dd HH:mm:ss")
                                                : r[6]
                );
            }

            ApplyFilter();
            Text = $"Chi tiết QR nhận lúc {_receivedAt} — {_rows.Count} mã";
        }

        private void ApplyFilter()
        {
            if (_allDataTable == null) return;

            string search = (txtSearch.Text == "Nhập từ khóa..." ? "" : txtSearch.Text).Trim().ToLower();
            string selectedItem = cmbStatus.SelectedItem?.ToString() ?? "";
            string status = (selectedItem == "Chưa dùng" || selectedItem == "Đã dùng")
                ? selectedItem : "";

            _filteredTable = _allDataTable.Clone();

            foreach (DataRow row in _allDataTable.Rows)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    if (row["Trạng thái"].ToString() != status) continue;
                }

                if (!string.IsNullOrEmpty(search))
                {
                    bool match = false;
                    foreach (DataColumn col in _allDataTable.Columns)
                    {
                        if (row[col].ToString().ToLower().Contains(search))
                        { match = true; break; }
                    }
                    if (!match) continue;
                }

                _filteredTable.ImportRow(row);
            }

            _totalPages = Math.Max(1, (int)Math.Ceiling((double)_filteredTable.Rows.Count / _pageSize));
            _currentPage = 1;
            LoadCurrentPage();
            UpdatePageInfo();
        }

        private void LoadCurrentPage()
        {
            var source = _filteredTable ?? _allDataTable;
            if (source == null) return;

            var pageData = source.Clone();
            int start = (_currentPage - 1) * _pageSize;
            int end = Math.Min(start + _pageSize, source.Rows.Count);

            for (int i = start; i < end; i++)
                pageData.ImportRow(source.Rows[i]);

            dataGridView.DataSource = pageData;
        }

        private void UpdatePageInfo()
        {
            var source = _filteredTable ?? _allDataTable;
            int total = source?.Rows.Count ?? 0;
            int start = (_currentPage - 1) * _pageSize + 1;
            int end = Math.Min(_currentPage * _pageSize, total);

            lblPageInfo.Text = total > 0
                ? $"Trang {_currentPage} / {_totalPages}  (dòng {start}-{end} / {total})"
                : "Không có dữ liệu";

            btnPrevious.Enabled = _currentPage > 1;
            btnNext.Enabled = _currentPage < _totalPages;
            btnFirst.Enabled = _currentPage > 1;
            btnLast.Enabled = _currentPage < _totalPages;
        }
    }
}
