using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View
{
    /// <summary>
    /// Hiển thị nội dung file log theo ngày từ C:\ProgramData\R-Link\Logs\log_{date}.txt
    /// Format: [yyyy-MM-dd HH:mm:ss.fff] [LEVEL] message
    /// </summary>
    public partial class FrmViewLogFile : Form
    {
        private const int CS_DropShadow = 0x00020000;
        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ClassStyle |= CS_DropShadow; return cp; }
        }

        private DateTimePicker _datLog;
        private CheckBox _chbError, _chbWarning, _chbInfo, _chbDebug;
        private TextBox _txtSearch;
        private Button _btnRefresh, _btnOpenFile;
        private Label _lblCount;
        private DataGridView _dgvLog;
        private RichTextBox _rtbDetail;
        private readonly List<LogEntry> _allEntries = new List<LogEntry>();

        private sealed class LogEntry
        {
            public string Level { get; set; }
            public string Time { get; set; }
            public string Message { get; set; }

            public string ShortMessage
            {
                get
                {
                    int idx = Message.IndexOf('\n');
                    return idx > 0 ? Message.Substring(0, idx).TrimEnd('\r') : Message;
                }
            }
        }

        public FrmViewLogFile()
        {
            InitializeComponent();
            BuildUI();
            UpdateIcon();
            LoadData();
        }

        private void UpdateIcon()
        {
            try
            {
                string iconPath = Application.StartupPath + "\\Label\\icon.ico";
                if (File.Exists(iconPath))
                    Icon = Icon.ExtractAssociatedIcon(iconPath);
                else
                    ShowIcon = false;
            }
            catch { ShowIcon = false; }
        }

        private void BuildUI()
        {
            SuspendLayout();
            Font = new Font("Segoe UI", 9f);

            // ── Title ──────────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "  Program Log",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 171, 230),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // ── Filter panel ───────────────────────────────────────────────────
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(248, 248, 248)
            };

            var lblDate = new Label
            {
                Text = "Ngày:",
                AutoSize = true,
                Location = new Point(10, 16),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _datLog = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Location = new Point(55, 13),
                Width = 100
            };

            _chbError = MakeCheckBox("ERROR", 170, Color.FromArgb(255, 220, 220));
            _chbWarning = MakeCheckBox("WARNING", 252, Color.FromArgb(255, 245, 200));
            _chbInfo = MakeCheckBox("INFO", 358, Color.FromArgb(220, 245, 220));
            _chbDebug = MakeCheckBox("DEBUG", 415, Color.FromArgb(240, 240, 240));

            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, Location = new Point(490, 16) };
            _txtSearch = new TextBox { Location = new Point(522, 13), Width = 185 };

            _btnRefresh = new Button
            {
                Text = "Làm mới",
                Location = new Point(716, 12),
                Size = new Size(78, 26),
                FlatStyle = FlatStyle.Flat
            };
            _btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(0, 171, 230);

            _btnOpenFile = new Button
            {
                Text = "Mở Notepad",
                Location = new Point(802, 12),
                Size = new Size(88, 26),
                FlatStyle = FlatStyle.Flat
            };
            _btnOpenFile.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);

            _lblCount = new Label
            {
                Text = "0 dòng",
                AutoSize = true,
                Location = new Point(900, 16),
                ForeColor = Color.Gray
            };

            pnlTop.Controls.AddRange(new Control[]
            {
                lblDate, _datLog,
                _chbError, _chbWarning, _chbInfo, _chbDebug,
                lblSearch, _txtSearch,
                _btnRefresh, _btnOpenFile, _lblCount
            });

            // ── Detail panel ───────────────────────────────────────────────────
            var pnlDetail = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                BackColor = Color.FromArgb(250, 250, 250)
            };
            var lblDetailTitle = new Label
            {
                Text = "  Chi tiết:",
                Dock = DockStyle.Top,
                Height = 20,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(228, 228, 228),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _rtbDetail = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(250, 250, 250),
                Font = new Font("Consolas", 8.5f),
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            pnlDetail.Controls.Add(_rtbDetail);
            pnlDetail.Controls.Add(lblDetailTitle);

            // ── Divider ────────────────────────────────────────────────────────
            var divider = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 3,
                BackColor = Color.FromArgb(200, 200, 200)
            };

            // ── DataGridView ───────────────────────────────────────────────────
            _dgvLog = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 30,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                GridColor = Color.FromArgb(220, 220, 220),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(210, 232, 255),
                    SelectionForeColor = Color.Black,
                    Padding = new Padding(3, 0, 3, 0)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 171, 230),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            _dgvLog.EnableHeadersVisualStyles = false;
            _dgvLog.RowTemplate.Height = 28;

            _dgvLog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colLevel",
                HeaderText = "Loại",
                Width = 85,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });
            _dgvLog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTime",
                HeaderText = "Thời gian",
                Width = 185,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Consolas", 8.5f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });
            _dgvLog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMessage",
                HeaderText = "Nội dung",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });

            // ── Events ─────────────────────────────────────────────────────────
            _datLog.ValueChanged += (s, e) => LoadData();
            _chbError.CheckedChanged += (s, e) => ApplyFilter();
            _chbWarning.CheckedChanged += (s, e) => ApplyFilter();
            _chbInfo.CheckedChanged += (s, e) => ApplyFilter();
            _chbDebug.CheckedChanged += (s, e) => ApplyFilter();
            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _btnRefresh.Click += (s, e) => LoadData();
            _btnOpenFile.Click += (s, e) => OpenLogFile();
            _dgvLog.CellFormatting += DgvLog_CellFormatting;
            _dgvLog.SelectionChanged += DgvLog_SelectionChanged;

            KeyPreview = true;
            KeyDown += (s, e) => { if (((KeyEventArgs)e).KeyCode == Keys.Escape) Close(); };

            // Thêm vào form — phần tử thêm SAU được dock TRÊN cùng
            Controls.Add(_dgvLog);    // Fill
            Controls.Add(divider);    // Bottom
            Controls.Add(pnlDetail);  // Bottom
            Controls.Add(pnlTop);     // Top (dưới title)
            Controls.Add(lblTitle);   // Top (trên cùng)

            ResumeLayout(false);
            PerformLayout();
        }

        private static CheckBox MakeCheckBox(string text, int x, Color checkedColor)
        {
            var chb = new CheckBox
            {
                Text = text,
                Checked = true,
                AutoSize = true,
                Location = new Point(x, 16),
                BackColor = checkedColor
            };
            chb.CheckedChanged += (s, e) =>
            {
                var cb = (CheckBox)s;
                cb.BackColor = cb.Checked ? checkedColor : Color.White;
            };
            return chb;
        }

        private static string GetLogFilePath(DateTime date)
        {
            string dir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "R-Link", "Logs");
            return System.IO.Path.Combine(dir, string.Format("log_{0:yyyy-MM-dd}.txt", date));
        }

        private void LoadData()
        {
            _allEntries.Clear();
            string path = GetLogFilePath(_datLog.Value);

            if (!File.Exists(path))
            {
                ApplyFilter();
                return;
            }

            string content;
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                    content = sr.ReadToEnd();
            }
            catch { ApplyFilter(); return; }

            string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var regex = new Regex(
                @"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})\] \[([A-Z]+)\s*\] (.*)$",
                RegexOptions.Compiled);

            LogEntry current = null;
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                string trimmed = line.TrimStart();
                if (trimmed.StartsWith("===") || trimmed.StartsWith("Session Started")) continue;

                var m = regex.Match(line);
                if (m.Success)
                {
                    current = new LogEntry
                    {
                        Time = m.Groups[1].Value,
                        Level = m.Groups[2].Value,
                        Message = m.Groups[3].Value
                    };
                    _allEntries.Add(current);
                }
                else if (current != null && (line.StartsWith(" ") || line.StartsWith("\t")))
                {
                    current.Message += "\n" + line.TrimEnd();
                }
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_dgvLog == null) return;
            if (InvokeRequired) { Invoke(new Action(ApplyFilter)); return; }

            var levels = new HashSet<string>();
            if (_chbError.Checked) levels.Add("ERROR");
            if (_chbWarning.Checked) levels.Add("WARNING");
            if (_chbInfo.Checked) levels.Add("INFO");
            if (_chbDebug.Checked) levels.Add("DEBUG");

            string kw = _txtSearch.Text;
            bool hasKw = !string.IsNullOrEmpty(kw);

            _dgvLog.SuspendLayout();
            _dgvLog.Rows.Clear();

            var rows = new List<DataGridViewRow>();
            foreach (LogEntry entry in _allEntries)
            {
                if (!levels.Contains(entry.Level)) continue;
                if (hasKw)
                {
                    bool matched =
                        entry.Message.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        entry.Time.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!matched) continue;
                }

                var row = new DataGridViewRow();
                row.CreateCells(_dgvLog, entry.Level, entry.Time, entry.ShortMessage);
                row.Height = 28;
                row.Tag = entry;
                rows.Add(row);
            }

            _dgvLog.Rows.AddRange(rows.ToArray());
            _dgvLog.ResumeLayout();
            _lblCount.Text = string.Format("{0} dòng", _dgvLog.RowCount);
        }

        private void DgvLog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _dgvLog.RowCount) return;
            if (_dgvLog.Rows[e.RowIndex].Selected) return;

            var entry = _dgvLog.Rows[e.RowIndex].Tag as LogEntry;
            if (entry == null) return;

            switch (entry.Level)
            {
                case "ERROR":
                    e.CellStyle.BackColor = Color.FromArgb(255, 220, 220);
                    e.CellStyle.ForeColor = Color.DarkRed;
                    break;
                case "WARNING":
                    e.CellStyle.BackColor = Color.FromArgb(255, 245, 200);
                    e.CellStyle.ForeColor = Color.DarkOrange;
                    break;
                case "DEBUG":
                    e.CellStyle.BackColor = Color.FromArgb(240, 240, 240);
                    e.CellStyle.ForeColor = Color.DimGray;
                    break;
                default:
                    e.CellStyle.BackColor = Color.White;
                    e.CellStyle.ForeColor = Color.Black;
                    break;
            }
        }

        private void DgvLog_SelectionChanged(object sender, EventArgs e)
        {
            if (_dgvLog.SelectedRows.Count == 0 || _rtbDetail == null) return;
            var entry = _dgvLog.SelectedRows[0].Tag as LogEntry;
            if (entry != null)
                _rtbDetail.Text = string.Format("[{0}] [{1}]\r\n{2}", entry.Time, entry.Level, entry.Message);
        }

        private void OpenLogFile()
        {
            string path = GetLogFilePath(_datLog.Value);
            if (!File.Exists(path))
            {
                MessageBox.Show(this,
                    "Không tìm thấy file log:\n" + path,
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try { System.Diagnostics.Process.Start("notepad.exe", path); }
            catch { }
        }
    }
}