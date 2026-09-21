using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.THTrueMilk;
using BarcodeVerificationSystem.Services.THTrueMilk;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.View;
using BarcodeVerificationSystem.View.CustomDialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using UILanguage;

namespace BarcodeVerificationSystem.View.UcSettings
{
    public partial class ucProductionTHTrueMilkSetting : UserControl
    {
        private string[] RLinkNames;
        private bool _isLoadingDbFields = false;

        // ── Debounce timer ────────────────────────────────────────────────
        private System.Windows.Forms.Timer _dbDebounceTimer;
        private const int DebounceMs = 1500;

        // ── Static DB connection ──────────────────────────────────────────
        private static IDbConnection _activeConnection = null;
        private static string _lastConnStr = string.Empty;
        private static string _lastDbType = string.Empty;
        private static int _tableCount = 0;
        private static readonly object _connectionLock = new object();
        private static bool _manualDisconnect = false;
        private static bool _lastAliveResult = false;
        private static DateTime _lastAliveCheckTime = DateTime.MinValue;
        private static readonly TimeSpan _aliveCacheDuration = TimeSpan.FromSeconds(5);

        /// <summary>Connection string của DB đang active (dùng cho các module khác như frmJobTHTrueMilk).</summary>
        public static string ActiveConnectionString => _lastConnStr;
        public static string ActiveDbType => _lastDbType;
        public static IDbConnection ActiveConnection => _activeConnection;
        // ── API Ping debounce ─────────────────────────────────────────────────
        private System.Windows.Forms.Timer _apiPingDebounceTimer;
        private const int ApiPingDebounceMs = 1500;

        // ── DB size refresh timer ──────────────────────────────────────────
        private System.Windows.Forms.Timer _dbSizeRefreshTimer;
        private const int DbSizeRefreshMs = 60000;

        // ── Lines group by factory từ server ─────────────────────────────────
        private Dictionary<string, List<Services.THTrueMilk.RLinkMaster.Models.LineInfo>> _linesByFactory
            = new Dictionary<string, List<Services.THTrueMilk.RLinkMaster.Models.LineInfo>>();
        private bool _isRemoteFactories = false;
        // ── Lines từ R-Link Master ────────────────────────────────────────
        private List<Services.THTrueMilk.RLinkMaster.Models.LineInfo> _remoteLines
            = new List<Services.THTrueMilk.RLinkMaster.Models.LineInfo>();
        private bool _isRemoteLines = false;
     

        // ── Nút Hủy gán (tạo programmatically) ───────────────────────
        private System.Windows.Forms.Button btnUnassignLine;
        public class Factory
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public override string ToString() => $"{Code} ({Name})";
        }

        private List<Factory> factories = new List<Factory>
        {
          
        };

        // ════════════════════════════════════════════════════════════════
        // Constructor
        // ════════════════════════════════════════════════════════════════
        public ucProductionTHTrueMilkSetting()
        {
            InitializeComponent();
            WrapGroupBoxWithScrollPanel(groupBoxDatabaseSettings);
            InitDebounceTimer();
            InitControls();
            InitEvents();
            InitLanguage();
        }

        // ════════════════════════════════════════════════════════════════
        // Init
        // ════════════════════════════════════════════════════════════════
        private static void MoveRadioToPanel(RadioButton rb, Panel panel, int index)
        {
            rb.Parent?.Controls.Remove(rb);
            rb.Location = new System.Drawing.Point(0, index * 30);
            panel.Controls.Add(rb);
        }
        private void InitDebounceTimer()
        {
            _dbDebounceTimer = new System.Windows.Forms.Timer { Interval = DebounceMs };
            _dbDebounceTimer.Tick += DbDebounceTimer_Tick;

            _apiPingDebounceTimer = new System.Windows.Forms.Timer { Interval = ApiPingDebounceMs };
            _apiPingDebounceTimer.Tick += ApiPingDebounceTimer_Tick;

            _dbSizeRefreshTimer = new System.Windows.Forms.Timer { Interval = DbSizeRefreshMs };
            _dbSizeRefreshTimer.Tick += DbSizeRefreshTimer_Tick;
        }
        private void InitLanguage()
        {
            lineIdLabel.Text = Lang.LineID;
            lineNameLabel.Text = Lang.LineName;
            factoryCodeLabel.Text = Lang.FactoryCode + ":";
            manufacturingRad.Text = Lang.Manufacturing;
            dispatchingRad.Text = Lang.Dispatching;
            productionMode.Text = Lang.ProductionMode;
            dataDisplay.Text = Lang.DisplayData;
            maskData.Text = Lang.MaskData;
            dataIncrease.Text = Lang.IncreasedData;
           // groupBoxProductionSettings.Text = "Cài đặt Line";
            labelApi.Text = "URL máy chủ:";
            //btnGetInfoLine.Text = "Lấy danh sách Line";
            //btnSetLine.Text = "Gán Line Active";
        }



        private void InitControls()
        {
            apiTextbox.Text = Shared.Settings.ApiUrl;
           // numIncreasedData.Value = Shared.Settings.IncreasedDataPercent;
            manufacturingRad.Checked = Shared.Settings.IsManufacturingMode;
            dispatchingRad.Checked = !Shared.Settings.IsManufacturingMode;
            maskData.Checked = Shared.Settings.MaskData;
            HideFunctions.Checked = Shared.Settings.HideFunctions;
            ckbCheckStart.Checked = Shared.Settings.CheckAllWhenStart;
            lineName.Text = Shared.Settings.LineName;
            LineId.Text = Shared.Settings.LineId;
            lblpAddressLocal.Text= GetLocalIpAddress();
            // ── Restore FactoryCodeCombox ────────────────────────────
            FactoryCodeCombox.Items.Clear();
            FactoryCodeCombox.Items.AddRange(factories.ToArray());
            var selected = factories.FirstOrDefault(f => f.Code == Shared.Settings.FactoryCode);
            if (selected != null)
            {
                FactoryCodeCombox.SelectedItem = selected;
            }
            else if (!string.IsNullOrEmpty(Shared.Settings.FactoryCode))
            {
                // Factory đến từ server — dùng FactoryName đã lưu
                var savedFactory = new Factory
                {
                    Code = Shared.Settings.FactoryCode,
                    Name = !string.IsNullOrEmpty(Shared.Settings.FactoryName)
                           ? Shared.Settings.FactoryName
                           : Shared.Settings.FactoryCode
                };
                FactoryCodeCombox.Items.Insert(0, savedFactory);
                FactoryCodeCombox.SelectedIndex = 0;
            }

            onlineProductionSettings.Enabled = !Shared.UserPermission.isOnline;
            InitDeviceName();

            // ── Restore RLinkNamescombox ─────────────────────────────
            if (!string.IsNullOrEmpty(Shared.Settings.LineId))
            {
                string display = string.IsNullOrEmpty(Shared.Settings.LineName)
                    ? Shared.Settings.LineId
                    : $"{Shared.Settings.LineId} — {Shared.Settings.LineName}";
                RLinkNamescombox.Items.Insert(0, display);
                RLinkNamescombox.SelectedIndex = 0;
            }

            cmbDatabaseType.Items.Clear();
            //cmbDatabaseType.Items.Add("SQL Server");
            //cmbDatabaseType.Items.Add("MySQL");
            cmbDatabaseType.Items.Add("PostgreSQL");

            int savedIndex = cmbDatabaseType.Items.IndexOf(Shared.Settings.THSelectedDbType);
            cmbDatabaseType.SelectedIndex = savedIndex >= 0 ? savedIndex : 0;

            LoadDbFields();

            if (Shared.IsDatabaseConnected && IsConnectionAlive())
            {
                RestoreConnectedUi();
            }
            else
            {
                if (Shared.IsDatabaseConnected && !IsConnectionAlive())
                {
                    Shared.IsDatabaseConnected = false;
                    Shared.RaiseOnDatabaseStatusChangeEvent();
                }
                SetDbUiDisconnected();
                bool hasDbSettings = !string.IsNullOrEmpty(txtDbServerName.Text.Trim())
                    && !string.IsNullOrEmpty(txtDbDatabaseName.Text.Trim());
                if (hasDbSettings)
                    _ = ConnectWithCurrentFieldsAsync();
            }
            InitOperatingModeControls();
            btnGetInfoLine.Visible = false;
            btnSetLine.Visible = false;

            // Hiển thị thông tin line đã gán (nếu có)
            if (!string.IsNullOrEmpty(Shared.Settings.LineId))
            {
                lblStatusLine.ForeColor = Color.Green;
                lblStatusLine.Text = $"◈ Đã kích hoạt {Shared.Settings.LineId}";
                            
                UpdateSetLineButtonState();
            }
            else
            {
                btnUnActiveLine.Visible = false; // ẩn hoàn toàn nút cũ
            }

            // Auto ping ngay khi mở nếu đã có URL
            if (!string.IsNullOrEmpty(Shared.Settings.ApiUrl))
                TriggerApiPing();


            // Hiển thị dung lượng ổ cứng trên panel3
            InitDiskSpaceDisplay();

        }
        // ════════════════════════════════════════════════════════════════
        // Restore line config từ PostgreSQL configline khi khởi động
        // ════════════════════════════════════════════════════════════════

        public static async Task<bool> RestoreLineConfigFromDbAsync()
        {
            if (_activeConnection == null) return false;

            try
            {
                string lineId = null, lineName = null, factoryCode = null, factoryName = null;
                int operatingMode = 0, bufferCount = 0;

                await Task.Run(() =>
                {
                    lock (_connectionLock)
                    {
                        if (_activeConnection == null) return;
                        using (IDbCommand cmd = _activeConnection.CreateCommand())
                        {
                            cmd.CommandText = $@"
                                SELECT {THDb.LineId}, {THDb.LineName}, {THDb.FactoryCode}, {THDb.FactoryName},
                                       {THDb.OperatingMode}, {THDb.BufferCount}
                                FROM {THDb.ConfigLine}
                                WHERE {THDb.Id} = 1
                                LIMIT 1";
                            using (IDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read()) return;
                                lineId = reader.IsDBNull(0) ? null : reader.GetString(0);
                                lineName = reader.IsDBNull(1) ? null : reader.GetString(1);
                                factoryCode = reader.IsDBNull(2) ? null : reader.GetString(2);
                                factoryName = reader.IsDBNull(3) ? null : reader.GetString(3);
                                operatingMode = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                                bufferCount = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                            }
                        }
                    }
                });

                if (string.IsNullOrEmpty(lineId)) return false;

                Shared.Settings.LineId = lineId;
                Shared.Settings.LineName = lineName ?? string.Empty;
                Shared.Settings.RLinkName = lineId;
                Shared.Settings.FactoryCode = factoryCode ?? string.Empty;
                Shared.Settings.FactoryName = factoryName ?? string.Empty;
                if (operatingMode >= 1 && operatingMode <= 3)
                    Shared.Settings.THOperatingMode = (Model.THTrueMilk.THTrueMilkOperatingMode)operatingMode;
                if (bufferCount > 0)
                    Shared.Settings.IncreasedDataPercent = bufferCount;

                Shared.SaveSettings();
                Console.WriteLine($"[RestoreLineConfig] [{factoryCode}] {lineId} — {lineName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestoreLineConfig] Bỏ qua: {ex.Message}");
                return false;
            }
        }
        private async void BtnUnassignLine_Click(object sender, EventArgs e)
        {
            string lineId = Shared.Settings.LineId;
            string factoryCode = Shared.Settings.FactoryCode;

            if (string.IsNullOrEmpty(lineId)) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn hủy gán line '{lineId}' [{factoryCode}] không?",
                "Xác nhận hủy gán",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            btnUnassignLine.Enabled = false;
            lblStatusLine.ForeColor = Color.DodgerBlue;
            lblStatusLine.Text = $"⟳ Đang hủy gán line '{lineId}'...";

            try
            {
                bool ok = await Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory
                              .Instance.UnassignLineAsync(lineId, factoryCode);

                if (!ok)
                {
                   lblStatusLine.ForeColor = Color.OrangeRed;
                    lblStatusLine.Text = "✘ Hủy gán thất bại — kiểm tra kết nối máy chủ.";
                    return;
                }

                // Xóa thông tin line khỏi Settings
                Shared.Settings.LineId = string.Empty;
                Shared.Settings.LineName = string.Empty;
                Shared.Settings.RLinkName = string.Empty;
                Shared.Settings.FactoryCode = string.Empty;
                Shared.Settings.FactoryName = string.Empty;
                Shared.SaveSettings();

                // Reset UI
                lblStatusLine.ForeColor = Color.DimGray;
                lblStatusLine.Text = "○ Chưa có line được gán";
                btnUnassignLine.Visible = false;
                btnSetLine.Visible = false;

                LineId.TextChanged -= AdjustData;
                lineName.TextChanged -= AdjustData;
                LineId.Text = string.Empty;
                lineName.Text = string.Empty;
                LineId.TextChanged += AdjustData;
                lineName.TextChanged += AdjustData;

                RestoreLocalRLinkNames();

                lblStatusLine.ForeColor = Color.Green;
                lblStatusLine.Text = "● Đã hủy gán thành công — line có thể được gán cho máy khác.";
            }
            catch (Exception ex)
            {
                lblStatusLine.ForeColor = Color.Red;
                lblStatusLine.Text = $"✘ Lỗi: {ex.Message}";
            }
            finally
            {
                btnUnassignLine.Enabled = true;
            }
        }
        private async void BtnSetLine_Click(object sender, EventArgs e)
        {
            // ── Nếu đã có line → hủy gán ──
            if (!string.IsNullOrEmpty(Shared.Settings.LineId))
            {
                await UnassignCurrentLineAsync();
                return;
            }

            // ── Gán line mới ──
            int idx = RLinkNamescombox.SelectedIndex;
            if (!_isRemoteLines || idx < 0 || idx >= _remoteLines.Count) return;

            var chosenLine = _remoteLines[idx];
            var selectedFactory = FactoryCodeCombox.SelectedItem as Factory;
            string factoryCode = chosenLine.factory_code ?? selectedFactory?.Code ?? "";
            string machineIp = GetLocalIpAddress();

            btnSetLine.Enabled = false;
            lblStatusLine.ForeColor = Color.DodgerBlue;
            lblStatusLine.Text = $"⟳ Đang gán line '{chosenLine.line_id}' (IP: {machineIp})...";

            try
            {
                bool ok = await Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory
                              .Instance.SetLineStatusAsync(chosenLine.line_id, machineIp, factoryCode);

                if (!ok)
                {
                    lblStatusLine.ForeColor = Color.OrangeRed;
                    lblStatusLine.Text = "✘ Gán line thất bại — IP này có thể đã được dùng cho line khác.";
                    return;
                }

                // ── 1. Lưu thông tin line vào Settings ──────────────────
                Shared.Settings.LineId = chosenLine.line_id;
                Shared.Settings.LineName = chosenLine.line_name;
                Shared.Settings.RLinkName = chosenLine.line_id;
                if (!string.IsNullOrEmpty(chosenLine.factory_code))
                    Shared.Settings.FactoryCode = chosenLine.factory_code;
                if (!string.IsNullOrEmpty(chosenLine.factory_name))
                    Shared.Settings.FactoryName = chosenLine.factory_name;

                // ── 2. Lấy config từ server và áp vào Settings ──────────
                lblStatusLine.Text = $"⟳ Đang tải config cho line '{chosenLine.line_id}'...";
                var settings = await Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory
                                   .Instance.GetSettingsAsync(chosenLine.line_id);

                if (settings != null)
                    ApplyRLinkSettings(settings);

                Shared.SaveSettings();

                // ── 3. Lưu vào bảng configline PostgreSQL ────────────────
                await SaveLineConfigToDbAsync(chosenLine, settings, machineIp);

                // ── 3b. Lưu vào SQLite configline ──────────────────────────
                try
                {
                    await System.Threading.Tasks.Task.Run(() =>
                        Services.THTrueMilk.RLinkMaster.RLinkLogService.SaveConfigLine(
                            chosenLine.line_id ?? "",
                            chosenLine.line_name ?? "",
                            chosenLine.factory_code ?? "",
                            chosenLine.factory_name ?? "",
                            machineIp ?? "",
                            settings?.OperatingMode ?? 0,
                            settings?.BufferCount ?? Shared.Settings?.THBufferCount ?? 0));
                }
                catch { }

                // ── 4. Cập nhật UI ───────────────────────────────────────
                LineId.Text = chosenLine.line_id;
                lineName.Text = chosenLine.line_name;

                string lineDisplay = string.IsNullOrEmpty(chosenLine.line_name)
                    ? chosenLine.line_id
                    : $"{chosenLine.line_id} — {chosenLine.line_name}";
                RLinkNamescombox.SelectedIndexChanged -= AdjustData;
                RLinkNamescombox.Items.Clear();
                RLinkNamescombox.Items.Add(lineDisplay);
                RLinkNamescombox.SelectedIndex = 0;
                RLinkNamescombox.SelectedIndexChanged += AdjustData;

                lblStatusLine.ForeColor = Color.Green;
                lblStatusLine.Text = $"● Đã gán: {machineIp}"
                                  + $" cho {chosenLine.line_name}";

                _remoteLines.Clear();
                _isRemoteLines = false;
                RestoreFactoryComboxAfterSetLine(chosenLine);
                UpdateSetLineButtonState();
            }
            catch (Exception ex)
            {
                lblStatusLine.ForeColor = Color.Red;
                lblStatusLine.Text = $"✘ Lỗi: {ex.Message}";
            }
            finally
            {
                btnSetLine.Enabled = true;
            }
        }

        /// <summary>Sau SetLine: restore FactoryCodeCombox hiển thị nhà máy đã gán, không clear _linesByFactory.</summary>
        private void RestoreFactoryComboxAfterSetLine(Services.THTrueMilk.RLinkMaster.Models.LineInfo assignedLine)
        {
            _isRemoteLines = false;
            _isRemoteFactories = false;
            _linesByFactory.Clear();

            FactoryCodeCombox.SelectedIndexChanged -= FactoryCodeCombox_SelectionChanged;
            FactoryCodeCombox.Items.Clear();

            // Hiển thị nhà máy đã gán (dùng tên từ server)
            var assignedFactory = new Factory
            {
                Code = assignedLine.factory_code ?? "",
                Name = !string.IsNullOrEmpty(assignedLine.factory_name)
                       ? assignedLine.factory_name
                       : assignedLine.factory_code ?? ""
            };
            FactoryCodeCombox.Items.Add(assignedFactory);
            FactoryCodeCombox.SelectedIndex = 0;
            FactoryCodeCombox.SelectedIndexChanged += FactoryCodeCombox_SelectionChanged;
        }

        private void RestoreLocalRLinkNames()
        {
            _isRemoteLines = false;
            _isRemoteFactories = false;
            _linesByFactory.Clear();

            RLinkNamescombox.SelectedIndexChanged -= RLinkNamescombox_RemoteSelectionChanged;
            RLinkNamescombox.SelectedIndexChanged -= AdjustData;
            InitDeviceName();
            // Hiển thị lại line đã lưu
            if (!string.IsNullOrEmpty(Shared.Settings.LineId))
            {
                string display = string.IsNullOrEmpty(Shared.Settings.LineName)
                    ? Shared.Settings.LineId
                    : $"{Shared.Settings.LineId} — {Shared.Settings.LineName}";
                RLinkNamescombox.Items.Insert(0, display);
                RLinkNamescombox.SelectedIndex = 0;
            }
            RLinkNamescombox.SelectedIndexChanged += AdjustData;

            FactoryCodeCombox.SelectedIndexChanged -= FactoryCodeCombox_SelectionChanged;
            FactoryCodeCombox.Items.Clear();
            FactoryCodeCombox.Items.AddRange(factories.ToArray());
            var savedFactory = factories.FirstOrDefault(f => f.Code == Shared.Settings.FactoryCode);
            if (savedFactory != null)
            {
                FactoryCodeCombox.SelectedItem = savedFactory;
            }
            else if (!string.IsNullOrEmpty(Shared.Settings.FactoryCode))
            {
                var f = new Factory
                {
                    Code = Shared.Settings.FactoryCode,
                    Name = !string.IsNullOrEmpty(Shared.Settings.FactoryName)
                           ? Shared.Settings.FactoryName
                           : Shared.Settings.FactoryCode
                };
                FactoryCodeCombox.Items.Insert(0, f);
                FactoryCodeCombox.SelectedIndex = 0;
            }
            FactoryCodeCombox.SelectedIndexChanged += FactoryCodeCombox_SelectionChanged;
        }

        private void InitOperatingModeControls()
        {
            var mode = Shared.Settings.THOperatingMode;
            radModeBatchQr.Checked = mode == THTrueMilkOperatingMode.BatchOneQrCode;
            radModeAutoRefresh.Checked = mode == THTrueMilkOperatingMode.AutoRefreshByTime;
            radModeProductQr.Checked = mode == THTrueMilkOperatingMode.ProductOneQrCode;
        }

        private void InitOperatingModeEvents()
        {
            radModeBatchQr.CheckedChanged += OperatingMode_CheckedChanged;
            radModeAutoRefresh.CheckedChanged += OperatingMode_CheckedChanged;
            radModeProductQr.CheckedChanged += OperatingMode_CheckedChanged;
        }

        private void OperatingMode_CheckedChanged(object sender, EventArgs e)
        {
            if (radModeBatchQr.Checked)
                Shared.Settings.THOperatingMode = THTrueMilkOperatingMode.BatchOneQrCode;
            else if (radModeAutoRefresh.Checked)
                Shared.Settings.THOperatingMode = THTrueMilkOperatingMode.AutoRefreshByTime;
            else if (radModeProductQr.Checked)
                Shared.Settings.THOperatingMode = THTrueMilkOperatingMode.ProductOneQrCode;
            Shared.SaveSettings();
        }

        private void WrapGroupBoxWithScrollPanel(System.Windows.Forms.GroupBox groupBox)
        {
            var controls = new System.Windows.Forms.Control[groupBox.Controls.Count];
            groupBox.Controls.CopyTo(controls, 0);

            var scrollPanel = new System.Windows.Forms.Panel
            {
                AutoScroll = true,
                Dock = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(0)
            };

            int maxRight = 0, maxBottom = 0;
            foreach (System.Windows.Forms.Control ctrl in controls)
            {
                if (ctrl.Right > maxRight) maxRight = ctrl.Right;
                if (ctrl.Bottom > maxBottom) maxBottom = ctrl.Bottom;
            }
            scrollPanel.AutoScrollMinSize = new System.Drawing.Size(maxRight + 4, maxBottom + 8);

            groupBox.Controls.Clear();
            foreach (System.Windows.Forms.Control ctrl in controls)
                scrollPanel.Controls.Add(ctrl);

            groupBox.Controls.Add(scrollPanel);
        }

        // ── InitEvents: thay FactoryCodeCombox.SelectedIndexChanged += AdjustData ──
        private void InitEvents()
        {
            apiTextbox.TextChanged += AdjustData;
            RLinkNamescombox.SelectedIndexChanged += AdjustData;
            FactoryCodeCombox.SelectedIndexChanged += FactoryCodeCombox_SelectionChanged; // ← đổi
            numIncreasedData.ValueChanged += AdjustData;
            manufacturingRad.CheckedChanged += AdjustData;
            dispatchingRad.CheckedChanged += AdjustData;
            maskData.CheckedChanged += AdjustData;
            HideFunctions.CheckedChanged += AdjustData;
            ckbCheckStart.CheckedChanged += AdjustData;
            lineName.TextChanged += AdjustData;
            LineId.TextChanged += AdjustData;
            cmbPreviewTable.DropDown += CmbPreviewTable_DropDown;
            cmbDatabaseType.SelectedIndexChanged += CmbDatabaseType_SelectedIndexChanged;
            btnPreviewTable.Click += BtnPreviewTable_Click;
            cmbPreviewTable.SelectedIndexChanged += CmbPreviewTable_SelectedIndexChanged;
            txtDbServerName.TextChanged += DbField_TextChanged;
            txtDbPort.TextChanged += DbField_TextChanged;
            txtDbDatabaseName.TextChanged += DbField_TextChanged;
          
            txtDbUsername.TextChanged += DbField_TextChanged;
            txtDbPassword.TextChanged += DbField_TextChanged;
            btnGetInfoLine.Click += BtnGetInfoLine_Click;
            btnSetLine.Click += BtnSetLine_Click;
            btnTestDbConnection.Click += BtnTestDbConnection_Click;
            btnCheckConnectDB.Click += BtnCheckConnectDB_Click;
            InitOperatingModeEvents();
        }

        // ── Handler duy nhất cho FactoryCodeCombox ──────────────────────────────────
        private void FactoryCodeCombox_SelectionChanged(object sender, EventArgs e)
        {
            var f = FactoryCodeCombox.SelectedItem as Factory;
            if (f == null) return;

            // Luôn lưu settings
            Shared.Settings.FactoryCode = f.Code;
            Shared.SaveSettings();

            // Nếu đã có data lines từ server → filter ngay, không cần bấm GetInfoLine lại
            if (_linesByFactory.Count > 0)
                FilterLinesBySelectedFactory();
        }

        // ── BtnGetInfoLine_Click: bỏ subscribe FactoryCodeCombox_RemoteSelectionChanged ──
        private async void BtnGetInfoLine_Click(object sender, EventArgs e)
        {
            btnGetInfoLine.Enabled = false;
            lblStatusLine.ForeColor = Color.DodgerBlue;
            lblStatusLine.Text = "⟳ Đang lấy danh sách line...";
            btnSetLine.Visible = false;

            try
            {
                var allLines = await Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory
                                   .Instance.GetLinesAsync(null);

                if (allLines == null || allLines.Count == 0)
                {
                    lblStatusLine.ForeColor = Color.OrangeRed;
                    lblStatusLine.Text = "⚠ Không có line nào trên server.";
                    CustomMessageBox.Show("Không có line nào trên server!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ── Group inactive lines by factory ─────────────────────────
                _linesByFactory.Clear();
                var inactiveLines = allLines.Where(l => !l.is_active).ToList();
                foreach (var line in inactiveLines)
                {
                    string fc = line.factory_code ?? "UNKNOWN";
                    if (!_linesByFactory.ContainsKey(fc))
                        _linesByFactory[fc] = new List<Services.THTrueMilk.RLinkMaster.Models.LineInfo>();
                    _linesByFactory[fc].Add(line);
                }

                // ── Populate FactoryCodeCombox — lấy tên từ server ──────────
                var allFactoryCodes = allLines
                    .Select(l => l.factory_code ?? "UNKNOWN")
                    .Distinct()
                    .ToList();

                _isRemoteFactories = true;
                FactoryCodeCombox.SelectedIndexChanged -= FactoryCodeCombox_SelectionChanged;
                FactoryCodeCombox.Items.Clear();
                foreach (var fc in allFactoryCodes)
                {
                    // Lấy factory_name từ server trả về trong LineInfo
                    string serverName = allLines
                        .Where(l => l.factory_code == fc && !string.IsNullOrEmpty(l.factory_name))
                        .Select(l => l.factory_name)
                        .FirstOrDefault();

                    FactoryCodeCombox.Items.Add(new Factory
                    {
                        Code = fc,
                        Name = serverName ?? fc   // server name, fallback về code
                    });
                }

                // Ưu tiên chọn factory đã lưu
                int savedFcIdx = -1;
                for (int i = 0; i < FactoryCodeCombox.Items.Count; i++)
                {
                    var ff = FactoryCodeCombox.Items[i] as Factory;
                    if (ff != null && ff.Code == Shared.Settings.FactoryCode) { savedFcIdx = i; break; }
                }
                FactoryCodeCombox.SelectedIndex = savedFcIdx >= 0 ? savedFcIdx : 0;
                FactoryCodeCombox.SelectedIndexChanged += FactoryCodeCombox_SelectionChanged;

                lblStatusLine.ForeColor = Color.Green;
                lblStatusLine.Text = $"● {allLines.Count} line / {inactiveLines.Count} chưa active";
                CustomMessageBox.Show($"Lấy danh sách line thành công!\n{allLines.Count} line / {inactiveLines.Count} chưa active.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                FilterLinesBySelectedFactory();
            }
            catch (Exception ex)
            {
                lblStatusLine.ForeColor = Color.Red;
                lblStatusLine.Text = $"✘ Lỗi: {ex.Message}";
                CustomMessageBox.Show($"Lấy danh sách line thất bại!\n{ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGetInfoLine.Enabled = true;
            }
        }

      

        // ── Xóa FactoryCodeCombox_RemoteSelectionChanged (không dùng nữa) ────────────
        // private void FactoryCodeCombox_RemoteSelectionChanged(...) → ĐÃ BỎ
        private void FactoryCodeCombox_RemoteSelectionChanged(object sender, EventArgs e)
        {
            FilterLinesBySelectedFactory();
        }

        private void FilterLinesBySelectedFactory()
        {
            var selectedFactory = FactoryCodeCombox.SelectedItem as Factory;
            if (selectedFactory == null)
            {
                RLinkNamescombox.Items.Clear();
                _remoteLines.Clear();
                _isRemoteLines = false;
                btnSetLine.Visible = false;
                return;
            }

            // Factory này có line chưa active không?
            if (!_linesByFactory.ContainsKey(selectedFactory.Code)
                || _linesByFactory[selectedFactory.Code].Count == 0)
            {
                RLinkNamescombox.SelectedIndexChanged -= AdjustData;
                RLinkNamescombox.SelectedIndexChanged -= RLinkNamescombox_RemoteSelectionChanged;
                RLinkNamescombox.Items.Clear();
                RLinkNamescombox.Items.Add("— Tất cả line đã được active —");
                RLinkNamescombox.SelectedIndex = 0;
                RLinkNamescombox.SelectedIndexChanged += AdjustData;
                _remoteLines.Clear();
                _isRemoteLines = false;
                btnSetLine.Visible = false;
                return;
            }

            var lines = _linesByFactory[selectedFactory.Code];
            _remoteLines = lines;
            _isRemoteLines = true;

            RLinkNamescombox.SelectedIndexChanged -= AdjustData;
            RLinkNamescombox.SelectedIndexChanged -= RLinkNamescombox_RemoteSelectionChanged;
            RLinkNamescombox.Items.Clear();
            foreach (var l in lines)
                RLinkNamescombox.Items.Add($"{l.line_id} — {l.line_name}");

            int savedIdx = lines.FindIndex(l => l.line_id == Shared.Settings.LineId
                                             || l.line_id == Shared.Settings.RLinkName);
            RLinkNamescombox.SelectedIndex = savedIdx >= 0 ? savedIdx : 0;
            RLinkNamescombox.SelectedIndexChanged += RLinkNamescombox_RemoteSelectionChanged;

            RLinkNamescombox_RemoteSelectionChanged(RLinkNamescombox, EventArgs.Empty);
            UpdateSetLineButtonState();
        }
          
        // ════════════════════════════════════════════════════════════════
        // ConfigLine — Lưu thông tin line mới nhất vào PostgreSQL
        // ════════════════════════════════════════════════════════════════

        private async Task SaveLineConfigToDbAsync(
            Services.THTrueMilk.RLinkMaster.Models.LineInfo line,
            Services.THTrueMilk.RLinkMaster.Models.RLinkSettings s,
            string machineIp)
        {
            if (_activeConnection == null || _lastDbType != "postgresql") return;

            try
            {
                await Task.Run(() =>
                {
                    lock (_connectionLock)
                    {
                        if (_activeConnection == null) return;

                        using (IDbCommand cmd = _activeConnection.CreateCommand())
                        {
                            // Tạo bảng nếu chưa có
                            cmd.CommandText = $@"
CREATE TABLE IF NOT EXISTS {THDb.ConfigLine} (
    id          INTEGER PRIMARY KEY DEFAULT 1,
    {THDb.LineId}     TEXT,
    {THDb.LineName}   TEXT,
    {THDb.FactoryCode} TEXT,
    {THDb.FactoryName} TEXT,
    {THDb.MachineIp}  TEXT,
    {THDb.OperatingMode} INTEGER,
    {THDb.BufferCount} INTEGER,
    {THDb.AssignedAt} TIMESTAMP
)";
                            cmd.ExecuteNonQuery();
                        }

                        using (IDbCommand cmd = _activeConnection.CreateCommand())
                        {
                            // Upsert — chỉ giữ 1 row với id = 1
                            cmd.CommandText = $@"
INSERT INTO {THDb.ConfigLine} (id, {THDb.LineId}, {THDb.LineName}, {THDb.FactoryCode},{THDb.FactoryName}, {THDb.MachineIp}, {THDb.OperatingMode}, {THDb.BufferCount}, {THDb.AssignedAt})
VALUES (1, @lid, @lname, @fc, @fname, @ip, @mode, @buf, NOW())
ON CONFLICT (id) DO UPDATE SET
    {THDb.LineId}       = EXCLUDED.{THDb.LineId},
    {THDb.LineName}     = EXCLUDED.{THDb.LineName},
    {THDb.FactoryCode}  = EXCLUDED.{THDb.FactoryCode},
    {THDb.FactoryName}  = EXCLUDED.{THDb.FactoryName},
    {THDb.MachineIp}    = EXCLUDED.{THDb.MachineIp},
    {THDb.OperatingMode} = EXCLUDED.{THDb.OperatingMode},
    {THDb.BufferCount}  = EXCLUDED.{THDb.BufferCount},
    {THDb.AssignedAt}   = NOW()";

                            AddDbParam(cmd, "@lid",  line.line_id   ?? string.Empty);
                            AddDbParam(cmd, "@lname", line.line_name ?? string.Empty);
                            AddDbParam(cmd, "@fc",   line.factory_code ?? string.Empty);
                            AddDbParam(cmd, "@fname", line.factory_name ?? string.Empty);
                            AddDbParam(cmd, "@ip",   machineIp);
                            AddDbParam(cmd, "@mode", (object)(s?.OperatingMode ?? 0));
                            AddDbParam(cmd, "@buf",  (object)(s?.BufferCount   ?? 0));
                            cmd.ExecuteNonQuery();
                        }
                    }
                });

                Console.WriteLine("[SaveLineConfig] Đã lưu configline thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveLineConfig] Lỗi: {ex.Message}");
            }
        }

        private static void AddDbParam(IDbCommand cmd, string name, object value)
        {
            IDbDataParameter p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
        private void ApplyRLinkSettings(Services.THTrueMilk.RLinkMaster.Models.RLinkSettings s)
        {
            // Operating mode
            if (s.OperatingMode >= 1 && s.OperatingMode <= 4)
            {
                var mode = (Model.THTrueMilk.THTrueMilkOperatingMode)s.OperatingMode;
                Shared.Settings.THOperatingMode = mode;
                radModeBatchQr.Checked = mode == Model.THTrueMilk.THTrueMilkOperatingMode.BatchOneQrCode ||
                                        mode == Model.THTrueMilk.THTrueMilkOperatingMode.BatchOneQrCodeNoChange;
                radModeAutoRefresh.Checked = mode == Model.THTrueMilk.THTrueMilkOperatingMode.AutoRefreshByTime;
                radModeProductQr.Checked = mode == Model.THTrueMilk.THTrueMilkOperatingMode.ProductOneQrCode;
            }

            if (s.BufferCount > 0)
                Shared.Settings.IncreasedDataPercent = s.BufferCount;

            Console.WriteLine($"[ApplyRLinkSettings] Mode={s.OperatingMode} Monitor={s.MonitoringIntervalMinutes}m Log={s.LogIntervalMinutes}m Buffer={s.BufferCount}");
        }
        private void RLinkNamescombox_RemoteSelectionChanged(object sender, EventArgs e)
        {
            int idx = RLinkNamescombox.SelectedIndex;
            if (!_isRemoteLines || idx < 0 || idx >= _remoteLines.Count) return;

            var line = _remoteLines[idx];

            lineName.TextChanged -= AdjustData;
            LineId.TextChanged -= AdjustData;
            lineName.Text = line.line_name;
            LineId.Text = line.line_id;
            lineName.TextChanged += AdjustData;
            LineId.TextChanged += AdjustData;
        }
        private static string GetLocalIpAddress()
        {
            try
            {
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                        continue;
                    if (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                        continue;
                    var props = ni.GetIPProperties();
                    if (props.GatewayAddresses == null || props.GatewayAddresses.Count == 0)
                        continue;
                    foreach (var ip in props.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            return ip.Address.ToString();
                    }
                }
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var addr in host.AddressList)
                {
                    if (addr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                        continue;
                    if (System.Net.IPAddress.IsLoopback(addr))
                        continue;
                    string s = addr.ToString();
                    if (s.StartsWith("169.254."))
                        continue;
                    return s;
                }
            }
            catch { }
            return "127.0.0.1";
        }


      

        private void UpdateLineButtonsState(bool apiConnected)
        {
            btnGetInfoLine.Visible = apiConnected || IsApiConnected();
           
        }

        private bool IsApiConnected()
        {
            return lblApiStatus.ForeColor == Color.Green
                && lblApiStatus.Text.StartsWith("●");
        }

        // ════════════════════════════════════════════════════════════════
        // API Health Check
        // ════════════════════════════════════════════════════════════════
        /// <summary>
        /// Kiểm tra URL hợp lệ: phải là http/https, không có query string, không có fragment.
        /// Ví dụ hợp lệ: http://localhost:5130, https://192.168.1.100:8080
        /// </summary>
        private static bool IsValidApiUrl(string url, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(url)) return false;

            // Trim và bỏ trailing slash
            url = url.Trim().TrimEnd('/');

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri)) return false;

            // Chỉ chấp nhận http hoặc https
            if (uri.Scheme != "http" && uri.Scheme != "https") return false;

            // Không cho phép query string (?...) hoặc fragment (#...)
            if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)) return false;

            // Path phải là rỗng hoặc "/"
            if (uri.AbsolutePath != "/" && uri.AbsolutePath != string.Empty) return false;

            int defaultPort = uri.Scheme == "https" ? 443 : 80;
            normalized = uri.Port == defaultPort
                ? $"{uri.Scheme}://{uri.Host}"
                : $"{uri.Scheme}://{uri.Host}:{uri.Port}";
            return true;
        }
        private async void btnCheckHealth_Click(object sender, EventArgs e)
        {
            string rawUrl = apiTextbox.Text.Trim();

            string normalizedUrl;
            if (!IsValidApiUrl(rawUrl, out normalizedUrl))
            {
                lblApiStatus.ForeColor = Color.OrangeRed;
                lblApiStatus.Text = "⚠ URL không hợp lệ. Định dạng: http://host:port";
                UpdateLineButtonsState(apiConnected: false);
                return;
            }

            // Cập nhật textbox với URL đã normalize
            if (rawUrl != normalizedUrl)
                apiTextbox.Text = normalizedUrl;

            btnCheckHealth.Enabled = false;
            lblApiStatus.ForeColor = Color.DodgerBlue;
            lblApiStatus.Text = "⟳ Đang kiểm tra...";
            UpdateLineButtonsState(apiConnected: false);

            try
            {
                string savedUrl = Shared.Settings.ApiUrl;
                Shared.Settings.ApiUrl = normalizedUrl;
                Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory.Reset();

                bool ok = await Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory
                              .Instance.PingAsync();

                if (ok)
                {
                    lblApiStatus.ForeColor = Color.Green;
                    lblApiStatus.Text = "● Kết nối thành công!";
                    Shared.SaveSettings();
                    UpdateLineButtonsState(apiConnected: true);
                    Shared.Settings.ApiPingSuccess = true;
                    Shared.SaveSettings();
                }
                else
                {
                    lblApiStatus.ForeColor = Color.OrangeRed;
                    lblApiStatus.Text = "✘ Không phản hồi (server offline)";
                    Shared.Settings.ApiUrl = savedUrl;
                    Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory.Reset();
                    UpdateLineButtonsState(apiConnected: false);
                }
            }
            catch (Exception ex)
            {
                lblApiStatus.ForeColor = Color.Red;
                lblApiStatus.Text = $"✘ Lỗi: {ex.Message}";
                UpdateLineButtonsState(apiConnected: false);
            }
            finally
            {
                btnCheckHealth.Enabled = true;
            }
        }

        // ════════════════════════════════════════════════════════════════
        // Database
        // ════════════════════════════════════════════════════════════════

        public static async Task AutoConnectFromSettingsAsync()
        {
            if (_manualDisconnect) return;
            if (IsConnectionAlive()) return;

            string dbTypeDisplay = Shared.Settings.THSelectedDbType ?? "SQL Server";
            string dbType;
            switch (dbTypeDisplay)
            {
                case "SQL Server": dbType = "sql"; break;
                case "MySQL": dbType = "mysql"; break;
                default: dbType = "postgresql"; break;
            }

            string server, port, username, password, database;
            switch (dbType)
            {
                case "sql":
                    server = Shared.Settings.THQrBankServer;
                    port = Shared.Settings.THQrBankPort;
                    database = Shared.Settings.THQrBankDatabase;
                    username = Shared.Settings.THQrBankUsername;
                    password = Shared.Settings.THQrBankPassword;
                    break;
                case "mysql":
                    server = Shared.Settings.THMySqlServer;
                    port = Shared.Settings.THMySqlPort;
                    database = Shared.Settings.THMySqlDatabase;
                    username = Shared.Settings.THMySqlUsername;
                    password = Shared.Settings.THMySqlPassword;
                    break;
                default:
                    server = Shared.Settings.THLocalDbServer;
                    port = Shared.Settings.THLocalDbPort;
                    database = Shared.Settings.THLocalDbDatabase;
                    username = Shared.Settings.THLocalDbUsername;
                    password = Shared.Settings.THLocalDbPassword;
                    break;
            }

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database)) return;

            string connStr = frmDatabase.GetConnectionString(dbType, server, port, username, password, database);
            try
            {
                IDbConnection conn = frmDatabase.GetDatabaseConnection(dbType, connStr);
                if (conn == null) return;
                var task = Task.Run(() => conn.Open());
                if (await Task.WhenAny(task, Task.Delay(5000)) == task)
                {
                    await task;
                    lock (_connectionLock)
                    {
                        _activeConnection = conn;
                        _lastConnStr = connStr;
                        _lastDbType = dbType;
                    }
                    InvalidateAliveCache();
                    Shared.IsDatabaseConnected = true;
                    Shared.RaiseOnDatabaseStatusChangeEvent();

                    // Đọc config line từ DB → cập nhật Shared.Settings
                    await RestoreLineConfigFromDbAsync();
                }
                else
                {
                    conn.Dispose();
                    Shared.IsDatabaseConnected = false;
                    Shared.RaiseOnDatabaseStatusChangeEvent();
                }
            }
            catch
            {
                Shared.IsDatabaseConnected = false;
                Shared.RaiseOnDatabaseStatusChangeEvent();
            }
        }

        private void TriggerDebounce()
        {
            _dbDebounceTimer.Stop();
            _dbDebounceTimer.Start();
        }

        private async void DbDebounceTimer_Tick(object sender, EventArgs e)
        {
            _dbDebounceTimer.Stop();
            if (_manualDisconnect) return;
            try { _activeConnection?.Close(); _activeConnection?.Dispose(); } catch { }
            lock (_connectionLock)
            {
                _activeConnection = null;
                _lastConnStr = string.Empty;
                _lastDbType = string.Empty;
                _tableCount = 0;
            }
            lblDbConnectionStatus.ForeColor = Color.DodgerBlue;
            lblDbConnectionStatus.Text = "⟳ Đang kết nối...";
            cmbPreviewTable.Items.Clear();
            cmbPreviewTable.Enabled = false;
            btnPreviewTable.Enabled = false;
            await ConnectWithCurrentFieldsAsync();
        }

        private async Task ConnectWithCurrentFieldsAsync()
        {
            string dbType = SelectedDbType;
            string server = txtDbServerName.Text.Trim();
            string database = txtDbDatabaseName.Text.Trim();

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
            {
                SetDbUiDisconnected();
                return;
            }

            string connStr = frmDatabase.GetConnectionString(
                dbType, server,
                txtDbPort.Text.Trim(),
                txtDbUsername.Text.Trim(),
                txtDbPassword.Text.Trim(),
                database);

            try
            {
                IDbConnection conn = frmDatabase.GetDatabaseConnection(dbType, connStr);
                if (conn == null)
                {
                    lblDbConnectionStatus.ForeColor = Color.OrangeRed;
                    lblDbConnectionStatus.Text = "✘ Loại DB chưa được hỗ trợ.";
                    Shared.IsDatabaseConnected = false;
                    Shared.RaiseOnDatabaseStatusChangeEvent();
                    return;
                }
                var task = Task.Run(() => conn.Open());
                if (await Task.WhenAny(task, Task.Delay(5000)) == task)
                {
                    await task;
                    lock (_connectionLock)
                    {
                        _activeConnection = conn;
                        _lastConnStr = connStr;
                        _lastDbType = dbType;
                    }
                    SetDbUiConnected();
                    await PopulateTableListAsync();
                }
                else
                {
                    conn.Dispose();
                    lblDbConnectionStatus.ForeColor = Color.OrangeRed;
                    lblDbConnectionStatus.Text = "✘ Hết thời gian kết nối (5 giây).";
                    Shared.IsDatabaseConnected = false;
                    Shared.RaiseOnDatabaseStatusChangeEvent();
                }
            }
            catch (Exception ex)
            {
                lblDbConnectionStatus.ForeColor = Color.Red;
                lblDbConnectionStatus.Text = $"✘ {ex.Message}";
                Shared.IsDatabaseConnected = false;
                Shared.RaiseOnDatabaseStatusChangeEvent();
            }
        }

        private async void AutoConnectAndUpdateUiAsync()
        {
            lblDbConnectionStatus.ForeColor = Color.DodgerBlue;
            lblDbConnectionStatus.Text = "⟳ Đang kết nối...";
            await ConnectWithCurrentFieldsAsync();
        }

        private void DbField_TextChanged(object sender, EventArgs e)
        {
            if (_isLoadingDbFields) return;
            SaveDbFields();
        }

        private void CmbDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Shared.Settings.THSelectedDbType = cmbDatabaseType.SelectedItem?.ToString() ?? "SQL Server";
            Shared.SaveSettings();
            try { _activeConnection?.Close(); _activeConnection?.Dispose(); } catch { }
            lock (_connectionLock)
            {
                _activeConnection = null;
                _lastConnStr = string.Empty;
                _lastDbType = string.Empty;
                _tableCount = 0;
            }
            Shared.IsDatabaseConnected = false;
            Shared.RaiseOnDatabaseStatusChangeEvent();
            SetDbUiDisconnected();
            LoadDbFields();
        }

        private void DisconnectDb()
        {
            try { _activeConnection?.Close(); _activeConnection?.Dispose(); } catch { }
            _activeConnection = null;
            _lastConnStr = string.Empty;
            _lastDbType = string.Empty;
            _tableCount = 0;
            _manualDisconnect = true;
            StopDbSizeRefresh();
            SetDbUiDisconnected();
            Shared.IsDatabaseConnected = false;
            Shared.RaiseOnDatabaseStatusChangeEvent();
        }

        private async void BtnTestDbConnection_Click(object sender, EventArgs e)
        {
            if (_activeConnection != null && IsConnectionAlive())
            {
                DisconnectDb();
            }
            else
            {
                _manualDisconnect = false;
                await ConnectWithCurrentFieldsAsync();
            }
        }

        private static string TranslateDbError(Exception ex)
        {
            string msg = ex?.Message ?? "Lỗi không xác định";
            if (ex is PostgresException)
            {
                var pgEx = (PostgresException)ex;
                if (pgEx.SqlState == "28P01")
                    return "✘ Sai mật khẩu";
                if (pgEx.SqlState == "3D000")
                    return "✘ Tên Database không tồn tại";
                if (pgEx.SqlState == "28000")
                    return "✘ Sai tài khoản / mật khẩu";
                if (pgEx.SqlState == "08001" || pgEx.SqlState == "08006")
                    return "✘ Không kết nối được — Sai IP hoặc Port?";
                if (pgEx.SqlState == "57P03")
                    return "✘ Server quá tải — Thử lại sau";
                if (pgEx.SqlState == "53300")
                    return "✘ Quá nhiều kết nối — Thử lại sau";
                return $"✘ Lỗi CSDL [{pgEx.SqlState}]: {pgEx.MessageText}";
            }
            if (msg.Contains("no such host") || msg.Contains("could not resolve"))
                return "✘ Không tìm thấy Hostname — Sai IP?";
            if (msg.Contains("timeout") || msg.Contains("timed out"))
                return "✘ Không phản hồi — Kiểm tra IP, Port, Firewall";
            if (msg.Contains("refused") || msg.Contains("actively refused"))
                return "✘ Bị từ chối — Sai Port hoặc Firewall chặn?";
            if (msg.Contains("password") || msg.Contains("authentication"))
                return "✘ Sai tài khoản / mật khẩu";
            return $"✘ {msg}";
        }

        private async void BtnCheckConnectDB_Click(object sender, EventArgs e)
        {
            lblDbConnectionStatus.ForeColor = Color.DodgerBlue;
            lblDbConnectionStatus.Text = "⟳ Đang thử kết nối đến CSDL...";
            btnCheckConnectDB.Enabled = false;

            try
            {
                string dbType = SelectedDbType;
                string server = txtDbServerName.Text.Trim();
                string database = txtDbDatabaseName.Text.Trim();

                if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
                {
                    lblDbConnectionStatus.ForeColor = Color.OrangeRed;
                    lblDbConnectionStatus.Text = "⚠ Chưa nhập Server hoặc tên Database.";
                    return;
                }

                string connStr = frmDatabase.GetConnectionString(
                    dbType, server, txtDbPort.Text.Trim(),
                    txtDbUsername.Text.Trim(), txtDbPassword.Text.Trim(), database);

                IDbConnection conn = frmDatabase.GetDatabaseConnection(dbType, connStr);
                if (conn == null)
                {
                    lblDbConnectionStatus.ForeColor = Color.OrangeRed;
                    lblDbConnectionStatus.Text = "⚠ Loại CSDL chưa được hỗ trợ.";
                    return;
                }

                var task = Task.Run(() => conn.Open());
                if (await Task.WhenAny(task, Task.Delay(5000)) == task)
                {
                    await task;
                    conn.Close();
                    conn.Dispose();
                    lblDbConnectionStatus.ForeColor = Color.Green;
                    lblDbConnectionStatus.Text = "● Kết nối thử thành công! Bạn có thể nhấn \"Kết nối\" để dùng.";
                }
                else
                {
                    conn.Dispose();
                    lblDbConnectionStatus.ForeColor = Color.OrangeRed;
                    lblDbConnectionStatus.Text = "⚠ Không phản hồi sau 5 giây. Kiểm tra IP, Port và firewall.";
                }
            }
            catch (Exception ex)
            {
                lblDbConnectionStatus.ForeColor = Color.Red;
                lblDbConnectionStatus.Text = TranslateDbError(ex);
            }
            finally
            {
                btnCheckConnectDB.Enabled = true;
            }
        }

        public static bool IsConnectionAlive()
        {
            if (_activeConnection == null) return false;
            if (DateTime.Now - _lastAliveCheckTime < _aliveCacheDuration)
                return _lastAliveResult;

            lock (_connectionLock)
            {
                try
                {
                    if (_activeConnection == null) return false;
                    if (_activeConnection.State != ConnectionState.Open)
                    {
                        _lastAliveResult = false;
                        _lastAliveCheckTime = DateTime.Now;
                        return false;
                    }
                    using (IDbCommand cmd = _activeConnection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT 1";
                        cmd.CommandTimeout = 1;
                        cmd.ExecuteScalar();
                    }
                    _lastAliveResult = true;
                    _lastAliveCheckTime = DateTime.Now;
                    return true;
                }
                catch
                {
                    _lastAliveResult = false;
                    _lastAliveCheckTime = DateTime.Now;
                    return false;
                }
            }
        }

        public static void InvalidateAliveCache()
        {
            _lastAliveCheckTime = DateTime.MinValue;
        }

        private void LoadDbFields()
        {
            _isLoadingDbFields = true;
            switch (SelectedDbType)
            {
                case "sql":
                    txtDbServerName.Text = Shared.Settings.THQrBankServer;
                    txtDbPort.Text = Shared.Settings.THQrBankPort;
                    txtDbDatabaseName.Text = Shared.Settings.THQrBankDatabase;
                   
                    txtDbUsername.Text = Shared.Settings.THQrBankUsername;
                    txtDbPassword.Text = Shared.Settings.THQrBankPassword;
                    break;
                case "mysql":
                    txtDbServerName.Text = Shared.Settings.THMySqlServer;
                    txtDbPort.Text = Shared.Settings.THMySqlPort;
                    txtDbDatabaseName.Text = Shared.Settings.THMySqlDatabase;
                  
                    txtDbUsername.Text = Shared.Settings.THMySqlUsername;
                    txtDbPassword.Text = Shared.Settings.THMySqlPassword;
                    break;
                default:
                    txtDbServerName.Text = Shared.Settings.THLocalDbServer;
                    txtDbPort.Text = Shared.Settings.THLocalDbPort;
                    txtDbDatabaseName.Text = Shared.Settings.THLocalDbDatabase;
                 
                    txtDbUsername.Text = Shared.Settings.THLocalDbUsername;
                    txtDbPassword.Text = Shared.Settings.THLocalDbPassword;
                    break;
            }
            _isLoadingDbFields = false;
        }

        private void SaveDbFields()
        {
            switch (SelectedDbType)
            {
                case "sql":
                    Shared.Settings.THQrBankServer = txtDbServerName.Text;
                    Shared.Settings.THQrBankPort = txtDbPort.Text;
                    Shared.Settings.THQrBankDatabase = txtDbDatabaseName.Text;
                   
                    Shared.Settings.THQrBankUsername = txtDbUsername.Text;
                    Shared.Settings.THQrBankPassword = txtDbPassword.Text;
                    break;
                case "mysql":
                    Shared.Settings.THMySqlServer = txtDbServerName.Text;
                    Shared.Settings.THMySqlPort = txtDbPort.Text;
                    Shared.Settings.THMySqlDatabase = txtDbDatabaseName.Text;
                   
                    Shared.Settings.THMySqlUsername = txtDbUsername.Text;
                    Shared.Settings.THMySqlPassword = txtDbPassword.Text;
                    break;
                default:
                    Shared.Settings.THLocalDbServer = txtDbServerName.Text;
                    Shared.Settings.THLocalDbPort = txtDbPort.Text;
                    Shared.Settings.THLocalDbDatabase = txtDbDatabaseName.Text;
                   
                    Shared.Settings.THLocalDbUsername = txtDbUsername.Text;
                    Shared.Settings.THLocalDbPassword = txtDbPassword.Text;
                    break;
            }
            Shared.SaveSettings();
        }

        private async Task RestoreAndUpdateLineUiAsync()
        {
            bool restored = await RestoreLineConfigFromDbAsync();
            if (!restored) return;

            // Cập nhật UI trên UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateLineUiFromSettings));
            }
            else
            {
                UpdateLineUiFromSettings();
            }
        }

        private void UpdateLineUiFromSettings()
        {
            string lineId = Shared.Settings.LineId;
            if (string.IsNullOrEmpty(lineId)) return;

            // Cập nhật textbox
            LineId.TextChanged -= AdjustData;
            lineName.TextChanged -= AdjustData;
            LineId.Text = lineId;
            lineName.Text = Shared.Settings.LineName;
            LineId.TextChanged += AdjustData;
            lineName.TextChanged += AdjustData;

            // Cập nhật RLinkNamescombox
            string display = string.IsNullOrEmpty(Shared.Settings.LineName)
                ? lineId
                : $"{lineId} — {Shared.Settings.LineName}";
            RLinkNamescombox.SelectedIndexChanged -= AdjustData;
            RLinkNamescombox.Items.Clear();
            RLinkNamescombox.Items.Add(display);
            RLinkNamescombox.SelectedIndex = 0;
            RLinkNamescombox.SelectedIndexChanged += AdjustData;

            // Cập nhật lblStatusLine
            lblStatusLine.ForeColor = Color.Green;
            lblStatusLine.Text = $"◈ Line: [{Shared.Settings.FactoryCode}] {lineId}"
                              + (string.IsNullOrEmpty(Shared.Settings.LineName)
                                 ? "" : $" — {Shared.Settings.LineName}");

            // Hiện nút toggle gán/hủy gán (chỉ khi online)
            if (IsRLinkAuthorized())
                UpdateSetLineButtonState();
            else
            {
                btnSetLine.Visible = false;
                btnUnActiveLine.Visible = false;
            }

            // Cập nhật operating mode
            InitOperatingModeControls();

            Console.WriteLine($"[UpdateLineUi] UI restored: [{Shared.Settings.FactoryCode}] {lineId}");
        }
        private static bool IsRLinkAuthorized()
        {
            return Shared.UserPermission != null && Shared.UserPermission.isOnline;
        }
        private void SetDbUiDisconnected()
        {
            InvalidateAliveCache();
            lblDbConnectionStatus.ForeColor = Color.Gray;
            lblDbConnectionStatus.Text = "○ Chưa kết nối — Nhập thông tin DB và nhấn \"Kết nối\"";
            cmbPreviewTable.Items.Clear();
            cmbPreviewTable.Enabled = false;
            btnPreviewTable.Enabled = false;
     
            btnTestDbConnection.Text = "Kết nối";
            btnTestDbConnection.BackColor = Color.FromArgb(0, 170, 80);
            btnTestDbConnection.ForeColor = Color.White;
        

            cmbDatabaseType.Enabled = true;
            txtDbPort.Enabled = true;
            txtDbServerName.Enabled = true;
            txtDbDatabaseName.Enabled = true;
            txtDbUsername.Enabled = true;
            txtDbPassword.Enabled = true;

            btnCheckConnectDB.Enabled = true;

            StopDbSizeRefresh();
        }

        private void SetDbUiConnected()
        {
            InvalidateAliveCache();
            lblDbConnectionStatus.ForeColor = Color.Green;
            lblDbConnectionStatus.Text = _tableCount > 0
                ? $"● Đã kết nối  ({_tableCount} bảng)"
                : "● Đã kết nối";
            Shared.IsDatabaseConnected = true;
            Shared.RaiseOnDatabaseStatusChangeEvent();


            btnTestDbConnection.Text = "Ngắt kết nối";
            btnTestDbConnection.BackColor = Color.FromArgb(220, 40, 40);
            btnTestDbConnection.ForeColor = Color.White;
          
           

            cmbDatabaseType.Enabled = false;
            txtDbPort.Enabled= false;
            txtDbServerName.Enabled = false;
            txtDbDatabaseName.Enabled = false;
            txtDbUsername.Enabled = false;
            txtDbPassword.Enabled = false;
            btnCheckConnectDB.Enabled = false;

            StartDbSizeRefresh();
        }

        private async void RestoreConnectedUi()
        {
            SetDbUiConnected();
            _isLoadingDbFields = true;
            cmbPreviewTable.Items.Clear();
            try
            {
                string dbType = _lastDbType;
                List<string> tableNames = null;
                await System.Threading.Tasks.Task.Run(() =>
                {
                    lock (_connectionLock)
                    {
                        if (_activeConnection == null) return;
                        var names = new List<string>();
                        string query = GetTableQuery(dbType);
                        using (IDbCommand cmd = _activeConnection.CreateCommand())
                        {
                            cmd.CommandText = query;
                            using (IDataReader reader = cmd.ExecuteReader())
                                while (reader.Read())
                                    names.Add(reader[0].ToString());
                        }
                        tableNames = names;
                    }
                });
                if (tableNames == null)
                {
                    _activeConnection = null;
                    Shared.IsDatabaseConnected = false;
                    Shared.RaiseOnDatabaseStatusChangeEvent();
                    SetDbUiDisconnected();
                    return;
                }
                foreach (var name in tableNames)
                    cmbPreviewTable.Items.Add(name);
                _tableCount = cmbPreviewTable.Items.Count;
                if (_tableCount > 0)
                {
                    string savedTable = GetSavedTableName();
                    int savedIndex = cmbPreviewTable.Items.IndexOf(savedTable);
                    cmbPreviewTable.SelectedIndex = savedIndex >= 0 ? savedIndex : 0;
                    cmbPreviewTable.Enabled = true;
                    btnPreviewTable.Enabled = true;
                    lblDbConnectionStatus.Text = $"● Đã kết nối  ({_tableCount} bảng)";
                }
            }
            catch
            {
                _activeConnection = null;
                Shared.IsDatabaseConnected = false;
                Shared.RaiseOnDatabaseStatusChangeEvent();
                SetDbUiDisconnected();
            }
            finally
            {
                _isLoadingDbFields = false;
            }
        }

        private void CmbPreviewTable_DropDown(object sender, EventArgs e)
        {
            if (_activeConnection == null || !IsConnectionAlive()) return;
            PopulateTableList();
        }

        private void CmbPreviewTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoadingDbFields) return;
            string selected = cmbPreviewTable.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;
            switch (SelectedDbType)
            {
                case "sql": Shared.Settings.THQrBankTable = selected; break;
                case "mysql": Shared.Settings.THMySqlTable = selected; break;
                default: Shared.Settings.THLocalDbTable = selected; break;
            }
         
            Shared.SaveSettings();
        }

        private string GetSavedTableName()
        {
            switch (SelectedDbType)
            {
                case "sql": return Shared.Settings.THQrBankTable ?? "";
                case "mysql": return Shared.Settings.THMySqlTable ?? "";
                default: return Shared.Settings.THLocalDbTable ?? "";
            }
        }

        private static string GetTableQuery(string dbType)
        {
            switch (dbType)
            {
                case "sql": return "SELECT name FROM sys.tables WHERE type = 'U' ORDER BY name";
                case "mysql": return "SHOW TABLES";
                default: return "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name";
            }
        }

        private async System.Threading.Tasks.Task PopulateTableListAsync()
        {
            if (_activeConnection == null) return;
            _isLoadingDbFields = true;
            cmbPreviewTable.Items.Clear();
            try
            {
                string dbType = _lastDbType;
                List<string> tableNames = null;
                await System.Threading.Tasks.Task.Run(() =>
                {
                    lock (_connectionLock)
                    {
                        if (_activeConnection == null) return;
                        var names = new List<string>();
                        using (IDbCommand cmd = _activeConnection.CreateCommand())
                        {
                            cmd.CommandText = GetTableQuery(dbType);
                            using (IDataReader reader = cmd.ExecuteReader())
                                while (reader.Read())
                                    names.Add(reader[0].ToString());
                        }
                        tableNames = names;
                    }
                });
                if (tableNames == null) return;
                foreach (var name in tableNames)
                    cmbPreviewTable.Items.Add(name);
                _tableCount = cmbPreviewTable.Items.Count;
                if (_tableCount > 0)
                {
                    string savedTable = GetSavedTableName();
                    int savedIndex = cmbPreviewTable.Items.IndexOf(savedTable);
                    cmbPreviewTable.SelectedIndex = savedIndex >= 0 ? savedIndex : 0;
                    cmbPreviewTable.Enabled = true;
                    btnPreviewTable.Enabled = true;
                }
                lblDbConnectionStatus.ForeColor = Color.Green;
                lblDbConnectionStatus.Text = _tableCount > 0
                    ? $"● Đã kết nối  ({_tableCount} bảng)"
                    : "● Đã kết nối  (không có bảng)";
            }
            catch (Exception ex)
            {
                lblDbConnectionStatus.ForeColor = Color.OrangeRed;
                lblDbConnectionStatus.Text = $"⚠ Không lấy được danh sách bảng: {ex.Message}";
            }
            finally
            {
                _isLoadingDbFields = false;
            }
        }

        private void PopulateTableList()
        {
            _ = PopulateTableListAsync();
        }

        private void BtnPreviewTable_Click(object sender, EventArgs e)
        {
            string tableName = cmbPreviewTable.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(_lastConnStr)) return;
            try
            {
                var form = new frmTableData(_lastConnStr, tableName, _lastDbType);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở dữ liệu bảng:\n{ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // Settings helpers
        // ════════════════════════════════════════════════════════════════

        private string SelectedDbType
        {
            get
            {
                switch (cmbDatabaseType.SelectedItem?.ToString())
                {
                    default: return "postgresql";
                }
            }
        }

        private void InitDeviceName()
        {
            RLinkNamescombox.Text = string.Empty;
            RLinkNames = new string[0];
            RLinkNamescombox.Items.Clear();
        }

        private void AdjustData(object sender, EventArgs args)
        {
            switch (sender)
            {
                case ComboBox cb:
                    if (cb == RLinkNamescombox)
                    {
                        if (_isRemoteLines) break;
                        string val = cb.SelectedItem?.ToString() ?? string.Empty;
                        Shared.Settings.RLinkName = val;
                        if (val.Length > 2)
                        {
                            int parsed;
                            if (int.TryParse(val.Substring(2), out parsed))
                                Shared.Settings.LineIndex = parsed;
                        }
                    }
                    else if (cb == FactoryCodeCombox)
                    {
                        var f = cb.SelectedItem as Factory;
                        if (f != null) Shared.Settings.FactoryCode = f.Code;
                    }
                    break;
                case TextBox tb:
                    if (tb == apiTextbox)
                    {
                        Shared.Settings.ApiUrl = tb.Text;
                        TriggerApiPing(); // auto ping khi sửa URL
                        // Chỉ save khi bấm btnCheckHealth
                        return;
                    }
                    else if (tb == lineName) Shared.Settings.LineName = tb.Text;
                    else if (tb == LineId) Shared.Settings.LineId = tb.Text;
                    break;

                case NumericUpDown num:
                    if (num == numIncreasedData)
                        Shared.Settings.IncreasedDataPercent = (int)numIncreasedData.Value;
                    break;

                case RadioButton rb:
                    if (rb == manufacturingRad || rb == dispatchingRad)
                    {
                        Shared.Settings.IsManufacturingMode = manufacturingRad.Checked;
                        if (!_isRemoteLines) InitDeviceName();
                    }
                    break;

                case CheckBox cbx:
                    if (cbx == maskData) Shared.Settings.MaskData = cbx.Checked;
                    if (cbx == HideFunctions) Shared.Settings.HideFunctions = cbx.Checked;
                    if (cbx == ckbCheckStart) Shared.Settings.CheckAllWhenStart = cbx.Checked;
                    break;
            }
            Shared.SaveSettings();
        }
        private void TriggerApiPing()
        {
            _apiPingDebounceTimer.Stop();
            _apiPingDebounceTimer.Start();
        }

        private async void ApiPingDebounceTimer_Tick(object sender, EventArgs e)
        {
            _apiPingDebounceTimer.Stop();
            await AutoPingAsync();
        }

        private async Task AutoPingAsync()
        {
            string rawUrl = Shared.Settings.ApiUrl?.Trim();

            string normalizedUrl;
            if (!IsValidApiUrl(rawUrl, out normalizedUrl))
            {
                lblApiStatus.ForeColor = Color.Gray;
                lblApiStatus.Text = string.IsNullOrEmpty(rawUrl)
                    ? "○ Chưa có URL máy chủ"
                    : "⚠ URL không hợp lệ";
                btnGetInfoLine.Visible = false;
                return;
            }

            lblApiStatus.ForeColor = Color.DodgerBlue;
            lblApiStatus.Text = "⟳ Đang kiểm tra kết nối...";
            btnGetInfoLine.Visible = false;

            try
            {
                Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory.Reset();
                bool ok = await Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory
                              .Instance.PingAsync();
                if (ok)
                {
                    lblApiStatus.ForeColor = Color.Green;
                    lblApiStatus.Text = "● Đã kết nối máy chủ";
                    btnGetInfoLine.Visible = true;
                    Shared.Settings.ApiPingSuccess = true;
                    Shared.SaveSettings();
                }
                else
                {
                    lblApiStatus.ForeColor = Color.OrangeRed;
                    lblApiStatus.Text = "✘ Máy chủ không phản hồi";
                }
            }
            catch (Exception ex)
            {
                lblApiStatus.ForeColor = Color.Red;
                lblApiStatus.Text = $"✘ {ex.Message}";
            }
        }
        private void groupBoxLineSettings_Enter(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void RLinkNamescombox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private static async Task DeleteLineConfigFromDbAsync()
        {
            if (_activeConnection == null || _lastDbType != "postgresql") return;
            try
            {
                await Task.Run(() =>
                {
                    lock (_connectionLock)
                    {
                        if (_activeConnection == null) return;
                        using (IDbCommand cmd = _activeConnection.CreateCommand())
                        {
                            cmd.CommandText = $"DELETE FROM {THDb.ConfigLine} WHERE {THDb.Id} = 1";
                            cmd.ExecuteNonQuery();
                        }
                    }
                });
                Console.WriteLine("[DeleteLineConfig] Đã xóa configline.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteLineConfig] Lỗi: {ex.Message}");
            }
        }
        private void UpdateSetLineButtonState()
        {
            try
            {
                if (btnSetLine == null) return;
                bool hasLine = !string.IsNullOrEmpty(Shared.Settings?.LineId);
                btnSetLine.Visible = true;
                btnUnActiveLine.Visible = false;
         
                if (hasLine)
                {
                    btnSetLine.Text = "Hủy gán thiết bị";
                    btnSetLine.BackColor = Color.FromArgb(220, 40, 40);
                    btnSetLine.ForeColor = Color.White;
                }
                else
                {
                    btnSetLine.Text = "Gán thiết bị";
                    btnSetLine.BackColor = Color.FromArgb(0, 170, 80);
                    btnSetLine.ForeColor = Color.White;
                }
            }
            catch { }
        }

        private async Task UnassignCurrentLineAsync()
        {
            string lineId = Shared.Settings.LineId;
            string lineNameMes = Shared.Settings.LineName;
            string factoryCode = Shared.Settings.FactoryCode;
            string factoryNameMes = Shared.Settings.FactoryName;

            if (string.IsNullOrEmpty(lineId)) return;

            var confirm = CustomMessageBox.Show(
                this,
                $"Bạn có chắc muốn hủy gán line '{lineId}] không?",
                "Xác nhận hủy gán",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            btnSetLine.Enabled = false;
            lblStatusLine.ForeColor = Color.DodgerBlue;
            lblStatusLine.Text = $"⟳ Đang hủy gán line '{lineId}'...";

            try
            {
                bool ok = await Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory
                              .Instance.UnassignLineAsync(lineId, factoryCode);

                if (!ok)
                {
                    lblStatusLine.ForeColor = Color.OrangeRed;
                    lblStatusLine.Text = "✘ Hủy gán thất bại";
                    return;
                }

                Shared.Settings.LineId = string.Empty;
                Shared.Settings.LineName = string.Empty;
                Shared.Settings.RLinkName = string.Empty;
                Shared.Settings.FactoryCode = string.Empty;
                Shared.Settings.FactoryName = string.Empty;
                Shared.SaveSettings();

                await DeleteLineConfigFromDbAsync();

                lblStatusLine.ForeColor = Color.DimGray;
                lblStatusLine.Text = "○ Chưa có line được gán";

                LineId.TextChanged -= AdjustData;
                lineName.TextChanged -= AdjustData;
                LineId.Text = string.Empty;
                lineName.Text = string.Empty;
                LineId.TextChanged += AdjustData;
                lineName.TextChanged += AdjustData;

                RestoreLocalRLinkNames();
                UpdateSetLineButtonState();

                lblStatusLine.ForeColor = Color.Green;
                lblStatusLine.Text = "● Đã hủy gán thành công";
            }
            catch (Exception ex)
            {
                lblStatusLine.ForeColor = Color.Red;
                lblStatusLine.Text = $"✘ Lỗi: {ex.Message}";
            }
            finally
            {
                btnSetLine.Enabled = true;
            }
        }

        // ── Handler cũ giữ lại để Designer không báo lỗi, nhưng ẩn nút ──
        private async void btnUnActiveLine_Click(object sender, EventArgs e)
        {
            await UnassignCurrentLineAsync();
        }
        // ════════════════════════════════════════════════════════════════
        // Disk Space Display (panel3)
        // ════════════════════════════════════════════════════════════════
        private Label _lblDiskValue;

        // ── Disk Space Display (panel3) ──────────────────────────────────────
        private void InitDiskSpaceDisplay()
        {
            UpdateDiskSpaceDisplay();
        }

        private void UpdateDiskSpaceDisplay()
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                Color worstColor = Color.FromArgb(0, 120, 60);

                foreach (var drive in System.IO.DriveInfo.GetDrives())
                {
                    if (!drive.IsReady) continue;
                    if (drive.DriveType != System.IO.DriveType.Fixed &&
                        drive.DriveType != System.IO.DriveType.Removable) continue;

                    double freeGb = drive.AvailableFreeSpace / 1_073_741_824.0;
                    double totalGb = drive.TotalSize / 1_073_741_824.0;
                    int usedPct = totalGb > 0 ? (int)((totalGb - freeGb) / totalGb * 100) : 0;
                  

                    Color c = freeGb < 5 ? Color.Red
                            : freeGb < 20 ? Color.OrangeRed
                                           : Color.FromArgb(0, 120, 60);

                    if (c == Color.Red || (c == Color.OrangeRed && worstColor != Color.Red))
                        worstColor = c;

                    sb.AppendLine($"[{drive.Name.TrimEnd('\\')}]  Trống: {freeGb:F1}/{totalGb:F0} GB  ({usedPct}% dùng)");
                }

                string text = sb.Length > 0 ? sb.ToString().TrimEnd() : "Không có ổ đĩa";
                Color color = worstColor;

                void DoUpdate()
                {
                    if (lblDiskStorage == null || lblDiskStorage.IsDisposed) return;
                    lblDiskStorage.ForeColor = color;
                    lblDiskStorage.Text = text;
                }

                if (InvokeRequired) Invoke(new Action(DoUpdate));
                else DoUpdate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiskSpace] {ex.Message}");
            }
        }
        private void label4_Click(object sender, EventArgs e)
        {

        }

        // ── DB Size Display ──────────────────────────────────────────────
        private async void DbSizeRefreshTimer_Tick(object sender, EventArgs e)
        {
            await UpdateDbSizeDisplayAsync();
        }

        private async System.Threading.Tasks.Task UpdateDbSizeDisplayAsync()
        {
            if (_activeConnection == null || !IsConnectionAlive() || label4.IsDisposed) return;
            try
            {
                long sizeBytes = 0;
                await System.Threading.Tasks.Task.Run(() =>
                {
                    lock (_connectionLock)
                    {
                        if (_activeConnection == null) return;
                        using (IDbCommand cmd = _activeConnection.CreateCommand())
                        {
                            cmd.CommandText = "SELECT pg_database_size(current_database())";
                            cmd.CommandTimeout = 5;
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                                sizeBytes = Convert.ToInt64(result);
                        }
                    }
                });

                string formatted = FormatSize(sizeBytes);
                if (label4.IsDisposed) return;
                label4.Text = $"{formatted}";
            }
            catch { }
        }

        private void StartDbSizeRefresh()
        {
            _dbSizeRefreshTimer.Start();
            _ = UpdateDbSizeDisplayAsync();
        }

        private void StopDbSizeRefresh()
        {
            _dbSizeRefreshTimer.Stop();
            if (!label4.IsDisposed) label4.Text = "";
        }

        private static string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}