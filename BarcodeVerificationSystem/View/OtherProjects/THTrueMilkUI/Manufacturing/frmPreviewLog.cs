using BarcodeVerificationSystem.Controller;
using CommonVariable;
using FluentFTP.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing
{
    public enum LogType { Print, Camera, CameraError }

    public partial class frmPreviewLog : Form
    {
        private List<string[]> _rows;
        private readonly LogType _logType;
        private readonly Func<System.Threading.Tasks.Task<List<string[]>>> _refreshCallback;
        private DataTable _allDataTable;
        private DataTable _filteredTable;
        private System.Windows.Forms.Timer _refreshTimer;

        private int _currentPage = 1;
        private const int _pageSize = 25;
        private int _totalPages = 1;

        public frmPreviewLog()
        {
            InitializeComponent();
            dataGridView.Dock = DockStyle.Fill;
            _rows = new List<string[]>();
            _refreshCallback = null;
            InitEvents();
            cmbStatus.SelectedIndex = 0;
        }

        public frmPreviewLog(List<string[]> rows, bool isCameraLog = false, bool isCameraError = false,
            Func<System.Threading.Tasks.Task<List<string[]>>> refreshCallback = null)
        {
            InitializeComponent();
            dataGridView.Dock = DockStyle.Fill;
            InitEvents();
            Task.Run(() => EnsureUserCacheLoaded());
            _rows = rows;
            _refreshCallback = refreshCallback;
            cmbStatus.SelectedIndex = 0;
            _logType = isCameraError ? LogType.CameraError
                     : isCameraLog ? LogType.Camera
                     : LogType.Print;
            LoadData();
            StartAutoRefresh();
        }

        private void StartAutoRefresh()
        {
            if (_refreshCallback == null) return;
            _refreshTimer = new System.Windows.Forms.Timer { Interval = 60000 };
            _refreshTimer.Tick += async (s, e) =>
            {
                var newRows = await _refreshCallback();
                if (newRows != null)
                {
                    _rows = newRows;
                    LoadData();
                }
            };
            _refreshTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            base.OnFormClosed(e);
        }

        private void InitEvents()
        {
            btnBack.Click += (s, e) => Close();
            btnPrevious.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; LoadCurrentPage(); UpdatePageInfo(); } };
            btnNext.Click += (s, e) => { if (_currentPage < _totalPages) { _currentPage++; LoadCurrentPage(); UpdatePageInfo(); } };
            btnFirst.Click += (s, e) => { _currentPage = 1; LoadCurrentPage(); UpdatePageInfo(); };
            btnLast.Click += (s, e) => { _currentPage = _totalPages; LoadCurrentPage(); UpdatePageInfo(); };

            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text == "Nhập từ khóa...") { txtSearch.Text = ""; txtSearch.ForeColor = System.Drawing.Color.Black; } };
            txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Nhập từ khóa..."; txtSearch.ForeColor = System.Drawing.Color.Gray; } };
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            cmbStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            dataGridView.CellFormatting += DataGridView_CellFormatting;
            dataGridView.CellClick += DataGridView_CellClick;

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

            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView.ScrollBars = ScrollBars.Both;

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

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_logType != LogType.CameraError) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dataGridView.Columns[e.ColumnIndex].HeaderText != "Hình ảnh") return;

            var source = _filteredTable ?? _allDataTable;
            if (source == null) return;
            int realRow = (_currentPage - 1) * _pageSize + e.RowIndex;
            if (realRow < 0 || realRow >= source.Rows.Count) return;

            //string imagePath = source.Rows[realRow]["Hình ảnh"]?.ToString();
            //if (!string.IsNullOrWhiteSpace(imagePath) && System.IO.File.Exists(imagePath))
            //{
            //    try { System.Diagnostics.Process.Start(imagePath); }
            //    catch { }
            //}

            string imagePath = source.Rows[realRow]["Hình ảnh"]?.ToString();
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                MessageBox.Show("Không có đường dẫn ảnh.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!System.IO.File.Exists(imagePath))
            {
                MessageBox.Show($"Ảnh không tồn tại:\n{imagePath}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo(imagePath)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở ảnh:\n{imagePath}\n\n{ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null) return;
            string colName = dataGridView.Columns[e.ColumnIndex].HeaderText;

            if (colName == "Đồng bộ")
            {
                string v = e.Value.ToString();
                e.CellStyle.ForeColor = v == "Đã đồng bộ"
                    ? System.Drawing.Color.FromArgb(0, 140, 70)
                    : System.Drawing.Color.FromArgb(192, 0, 0);
            }
            else if (colName == "Người dùng")
            {
                e.Value = ResolveUserName(e.Value?.ToString());
                e.FormattingApplied = true;
            }
            else if (colName == "Thời gian")
            {
                if (DateTime.TryParse(e.Value.ToString(), out DateTime dt))
                {
                    e.Value = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    e.FormattingApplied = true;
                }
            }
            else if (colName == "Hình ảnh")
            {
                string path = e.Value?.ToString();
                e.Value = string.IsNullOrWhiteSpace(path) ? "" : "Xem ảnh";
                e.CellStyle.ForeColor = System.Drawing.Color.Blue;
                e.CellStyle.Font = new System.Drawing.Font(dataGridView.Font, System.Drawing.FontStyle.Underline);
                //e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Cursor = Cursors.Hand;
                e.FormattingApplied = true;
            }
            else if (colName == "QR Detail")
            {
                string json = e.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    e.Value = FormatQrDetail(json);
                    e.FormattingApplied = true;
                }
            }
        }

        private void LoadData()
        {
            if (_rows == null || _rows.Count == 0)
            {
                _allDataTable = null;
                _filteredTable = null;
                dataGridView.DataSource = null;
                UpdatePageInfo();
                return;
            }

            int prevPage = _currentPage;
            string prevSearch = txtSearch.Text == "Nhập từ khóa..." ? "" : txtSearch.Text;
            int prevCmbIdx = cmbStatus.SelectedIndex;

            _allDataTable = new DataTable();
            _allDataTable.Columns.Add("ID", typeof(int));

            switch (_logType)
            {
                case LogType.Camera:
                    _allDataTable.Columns.Add("Người dùng", typeof(string));
                    _allDataTable.Columns.Add("Mã / Batch", typeof(string));
                    _allDataTable.Columns.Add("Thời gian", typeof(string));
                    _allDataTable.Columns.Add("Line ID", typeof(string));
                    _allDataTable.Columns.Add("Sản phẩm", typeof(string));
                    _allDataTable.Columns.Add("Tên SP", typeof(string));
                    _allDataTable.Columns.Add("OK", typeof(string));
                    _allDataTable.Columns.Add("Fail", typeof(string));
                    _allDataTable.Columns.Add("Tổng", typeof(string));
                    _allDataTable.Columns.Add("QR Code", typeof(string));
                    _allDataTable.Columns.Add("QR Detail", typeof(string));
                    _allDataTable.Columns.Add("Trạng thái", typeof(string));
                    _allDataTable.Columns.Add("Đồng bộ", typeof(string));
                    foreach (var r in _rows)
                    {
                        if (r.Length < 16) continue;
                        _allDataTable.Rows.Add(
                            int.TryParse(r[0], out int id) ? id : 0,
                            r[3], r[2], r[10], r[13], r[4], r[14], r[5], r[6], r[7], r[8], r[9], r[15],
                            string.IsNullOrWhiteSpace(r[12]) || r[12] == "0" ? "Chưa đồng bộ" : "Đã đồng bộ"
                        );
                    }
                    break;

                case LogType.CameraError:
                    _allDataTable.Columns.Add("Người dùng", typeof(string));
                    _allDataTable.Columns.Add("QR Code", typeof(string));
                    _allDataTable.Columns.Add("NSX", typeof(string));
                    _allDataTable.Columns.Add("HSD", typeof(string));
                    _allDataTable.Columns.Add("Loại lỗi", typeof(string));
                    _allDataTable.Columns.Add("Chi tiết gói tin", typeof(string));
                    _allDataTable.Columns.Add("Sản phẩm", typeof(string));
                    _allDataTable.Columns.Add("Tên SP", typeof(string));
                    _allDataTable.Columns.Add("Batch", typeof(string));
                    _allDataTable.Columns.Add("Thời gian", typeof(string));
                    _allDataTable.Columns.Add("Đồng bộ", typeof(string));
                    _allDataTable.Columns.Add("Hình ảnh", typeof(string));
                    foreach (var r in _rows)
                    {
                        if (r.Length < 14) continue;
                        _allDataTable.Rows.Add(
                            int.TryParse(r[0], out int id) ? id : 0,
                            r[3],
                            r[4],
                            r[5],
                            r[6],
                            r[7],
                            r[9] ?? "",
                            r[12],
                            r[13],
                            r[2],
                            r[8],
                            string.IsNullOrWhiteSpace(r[10]) || r[10] == "0" ? "Chưa đồng bộ" : "Đã đồng bộ",
                            r[11] ?? ""
                        );
                    }
                    break;

                default:
                    _allDataTable.Columns.Add("Người dùng", typeof(string));
                    _allDataTable.Columns.Add("Mã / Batch", typeof(string));
                    _allDataTable.Columns.Add("Thời gian", typeof(string));
                    _allDataTable.Columns.Add("Line ID", typeof(string));
                    _allDataTable.Columns.Add("Sản phẩm", typeof(string));
                    _allDataTable.Columns.Add("Tên SP", typeof(string));
                    _allDataTable.Columns.Add("Số lượng", typeof(string));
                    _allDataTable.Columns.Add("QR Code", typeof(string));
                    _allDataTable.Columns.Add("QR Detail", typeof(string));
                    _allDataTable.Columns.Add("Trạng thái", typeof(string));
                    _allDataTable.Columns.Add("Đồng bộ", typeof(string));
                    foreach (var r in _rows)
                    {
                        if (r.Length < 14) continue;
                        _allDataTable.Rows.Add(
                            int.TryParse(r[0], out int id) ? id : 0,
                            r[4], r[2], r[9], r[12], r[5], r[13], r[6], r[7], r[8], r[3],
                            string.IsNullOrWhiteSpace(r[11]) || r[11] == "0" ? "Chưa đồng bộ" : "Đã đồng bộ"
                        );
                    }
                    break;
            }

            _allDataTable.DefaultView.Sort = "ID DESC";
            _allDataTable = _allDataTable.DefaultView.ToTable();

            ApplyFilter();

            if (prevPage <= _totalPages)
                _currentPage = prevPage;
            LoadCurrentPage();
            UpdatePageInfo();
            Text = $"Lịch sử Log — {_filteredTable?.Rows.Count ?? 0} / {_rows.Count} dòng";
        }

        private void ApplyFilter()
        {
            if (_allDataTable == null) return;

            string search = (txtSearch.Text == "Nhập từ khóa..." ? "" : txtSearch.Text).Trim().ToLower();
            string status = cmbStatus.SelectedIndex > 0 ? cmbStatus.SelectedItem.ToString() : "";

            bool hasStatusColumn = _allDataTable.Columns.Contains("Trạng thái");

            _filteredTable = _allDataTable.Clone();

            foreach (DataRow row in _allDataTable.Rows)
            {
                if (!string.IsNullOrEmpty(status) && hasStatusColumn)
                {
                    string rowStatus = row["Trạng thái"].ToString().ToLower();
                    if (rowStatus != status.ToLower()) continue;
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
            _currentPage = Math.Min(_currentPage, _totalPages);
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
            dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            int totalColWidth = 0;
            foreach (DataGridViewColumn col in dataGridView.Columns)
                totalColWidth += col.Width;

            if (totalColWidth < dataGridView.ClientSize.Width)
            {
                foreach (DataGridViewColumn col in dataGridView.Columns)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
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
        private static string FormatQrDetail(string json)
        {
            try
            {
                var items = JsonConvert.DeserializeObject<List<QrDetailItem>>(json);
                if (items == null || items.Count == 0) return "";
                return string.Join(", ", items.Select(i => $"{i.qr}({i.count:n0})"));
            }
            catch { return json; }
        }

        private static readonly ConcurrentDictionary<string, string> _userNameCache
            = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static bool _cacheLoaded;

        private static void EnsureUserCacheLoaded()
        {
            if (_cacheLoaded) return;
            lock (_userNameCache)
            {
                if (_cacheLoaded) return;
                try
                {
                    string path = CommVariables.PathAccountsApp;
                    var builder = new SQLiteConnectionStringBuilder
                    {
                        DataSource = path + "AccountDB.db",
                        Version = 3,
                        Password = "pass.security.Rynan@0988345294",
                    };
                    using (var conn = new SQLiteConnection(builder.ToString()))
                    {
                        conn.Open();
                        using (var cmd = new SQLiteCommand("SELECT username, fullname FROM tbl_account", conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string uname = SecurityController.Decrypt(
                                    reader.GetString(0), "rynan_encrypt_remember");
                                string fname = reader.GetString(1);
                                _userNameCache[uname] = fname;
                            }
                        }
                    }
                }
                catch { }
                _cacheLoaded = true;
            }
        }

        private static string ResolveUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return userName;
            if (_userNameCache.TryGetValue(userName, out string cached)) return cached;
            if (!_cacheLoaded) return userName;
            return userName;
        }

        private class QrDetailItem
        {
            public string qr { get; set; }
            public int count { get; set; }
        }
    }
}
