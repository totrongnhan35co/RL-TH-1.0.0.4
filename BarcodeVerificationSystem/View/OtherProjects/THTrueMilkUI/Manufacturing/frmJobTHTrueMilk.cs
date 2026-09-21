using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Controller.Camera;
using BarcodeVerificationSystem.Controller.Camera.Keyence;
using BarcodeVerificationSystem.Controller.HistorySync;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
//using Mysqlx.Crud;
using BarcodeVerificationSystem.Model.Apis.THTrueMilk.Manufacturing;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using BarcodeVerificationSystem.Model.RunningMode.Dispatching;
using BarcodeVerificationSystem.Model.THTrueMilk;
//using Org.BouncyCastle.Asn1.Ocsp;
//using MySqlX.XDevAPI.Common;
using BarcodeVerificationSystem.Model.UserInfo;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Core;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Factories;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services.THTrueMilk;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Controller;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.UI;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing;
using BarcodeVerificationSystem.View.UcSettings;
using BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess;
using Cognex.DataMan.SDK;
using CommonVariable;
using DesignUI.CuzAlert;
using DesignUI.CuzMesageBox;
using DesignUI.CuzUI;
using GenCode.Utils;
using Newtonsoft.Json;
using NPOI.HSSF.Record.CF;
using NPOI.SS.Formula.Functions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OperationLog.Controller;
using OperationLog.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Interop;
using UILanguage;
using ZstdSharp.Unsafe;
using static BarcodeVerificationSystem.Controller.Shared;
using static BarcodeVerificationSystem.Model.THTrueMilk.SyncDataParams;
using static BarcodeVerificationSystem.Services.THTrueMilk.THDb;
using SyncDataParams = BarcodeVerificationSystem.Model.THTrueMilk.SyncDataParams;
using Timer = System.Windows.Forms.Timer;


namespace BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing
{
    public partial class frmJobTHTrueMilk : Form
    {
        #region Variables Jobs
        private const int SYNC_LOG_TOP_OFFSET = 52;
        // TODO: Thêm control InputPO vào TabCreatePOOffline trong Designer
        private DesignUI.CuzUI.CuzTextBox InputPO = new DesignUI.CuzUI.CuzTextBox();
        public static readonly DMSeries DMCamera = new DMSeries();
        public ManufacturingService ManufacturingService = new ManufacturingService();
        private QrBankAPIHandler _qrBankAPIHandler;
        private static Thread _ThreadMonitorDatabase;
        private static volatile bool _isReconnecting;
        public ISMultiSyncHandler ISMultiSyncHandler;
        public ISSingleHandler ISSingleHandler;
        // Chế độ 1: 1 Batch in 1 QR Code
        internal event EventHandler AutoAddSufixEvent;
        // ── RLinkLogService dùng để lưu settings/products/configline vào SQLite ──
        private readonly BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService _rlinkLogService
            = new BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService();
        // Chế độ 2: N phút in 1 QR Code
       
        private DateTime _lastMode2DebugTime = DateTime.MinValue;
#if DEBUG
        private DateTime? _debugNsxHsdTime;
#endif
        private int _mode2RemainingSeconds = 0;
        private DateTime _mode2NextRefreshAt = DateTime.MinValue;
        // ── QR consumption tracking (Mode 2) ─────────────────────────
        private int _qrConsumedCount = 0;          // Số QR đã thay đổi từ đầu job
        public int QrConsumedCount => _qrConsumedCount;
        private DateTime _lastQrLowAlertTime = DateTime.MinValue;
        private const int QR_LOW_ALERT_COOLDOWN_MINUTES = 10; // Tránh spam alert
        private bool _qrLowAlertShown = false;
        private int _initialTotalQr = -1;

        public event EventHandler<(string qrCode, DateTime batchDate)> Mode1QrCodeChanged;
        internal TaskCompletionSource<bool> _rebuildTcs;
        private string _currentBatchId = string.Empty;
        private string _currentBatchQrCode = string.Empty;
        private int _currentQrId = -1;
        private readonly object _batchQrLock = new object();
        private DateTime _currentBatchDate = DateTime.MinValue;
        private readonly Timer _TimerMidnightReset = new Timer();
        internal bool isShowPopupDisConOneTime = false;
        internal bool isShowPopupDupDbOneTime = false;
        internal bool isShowPopupDisPrinterOneTime = false;
        private DateTime _printerDisconnectedSince = DateTime.MinValue;
        private int _plcDisconnectDebounce = 0;
        private DateTime _plcDisconnectedSince = DateTime.MinValue;
        private bool _plcDisconnectAlertShown = false;
        private DateTime _keyenceDisconnectedSince = DateTime.MinValue;
        private bool _keyenceDisconnectAlertShown = false;
        private DateTime _cognexDisconnectedSince = DateTime.MinValue;
        private bool _cognexDisconnectAlertShown = false;
        private bool _printerWasEverConnected = false;
        private bool _keyenceWasEverConnected = false;
        private bool _cognexWasEverConnected = false;
        private bool _plcWasEverConnected = false;
        internal bool isShowPopupFalseInitDataOneTime = false;
        private bool _noQrWarningShown = false;
        private bool _needsQrChange = false;
        private Label _lblMode1BatchInfo;
        private Label label43;
        private Label label44;

        private readonly Timer _TimerDateTime = new Timer();
        private DateTime _lastDateForFilter = DateTime.MinValue;
        private readonly string _DateTimeFormat = "yyyy/MM/dd hh:mm:ss tt";
        public double _NumberTotalsCode = 0;
        private bool _IsBinding = false;
        private bool _IsProcessing = false;
        private string _NameOfJobOld = "";
        private int countSkipFirstAlert = 0;
        private List<PODModel> _PODFormat = new List<PODModel>();
        private readonly List<PODModel> _PODList = new List<PODModel>();
        private List<string> _JobNameList = null;

        private readonly List<ToolStripLabel> _LabelStatusCameraList = new List<ToolStripLabel>();
        private readonly List<ToolStripLabel> _LabelStatusPrinterList = new List<ToolStripLabel>();

        PrintingQueueProcessor _printedDataProcess;
        VerificationQueueProcessor _verificationDataProcess;
        private bool _syncingFromPopup;
        private Label _lblSyncJobName;
        private System.Windows.Forms.Timer _connectCheckTimer;
        private System.Windows.Forms.Timer _qrMarkRetryTimer;

        private FrmSettingsTHTrueMilk _FormSettings;
        public JobModel _JobModel = null;
        private frmMainTHTrueMilk _FormMainPC = null;

        private Thread _ThreadMonitorPrinter;   
        private readonly bool _IsObtainingPrintProductTemplateList = false;
        private TaskCompletionSource<bool> _rslResponseTcs;
        private string[] _PrintProductTemplateList = new string[] { };
        private string _matchedCameraProgram = "";
        private string _matchedPrinterTemplate = "";
        private Thread _ThreadMonitorCamera;

        private readonly string SupportForPrinter = "Support for printer: RYNAN R10, RYNAN R20, RYNAN R40, RYNAN R60, RYNAN B1040.";
        private readonly string Standalone = "In this mode the software does not communicate and control the printer, the software only verifies the barcode through the camera.";
        private readonly List<string> CameraSupportNameList = new List<string>
        {
            "Camera Cognex DM series",
            "Camera Cognex IS2800 series"
        };
        private static readonly Color _Standalone = Color.DarkBlue;
        private static readonly Color _RLinkColor = Color.FromArgb(0, 171, 230);

        private Thread _ThreadMonitorSensorController;
        private Thread _ThreadMonitorSerialDeviceController;
        public bool[] _IsSymbol = new bool[5];

        public static bool eventTwoOccurred = false;
        public static object lockObject = new object();
        public static bool isEventTwoHandled = false;
        private readonly string _endOfLineStr = "<EOF>";
#if DEBUG
        private TextBox _txtDebugResetTime;
        private System.Windows.Forms.Panel _pnlDebug;
        private System.Windows.Forms.TextBox _txtCameraPrograms;
        private System.Windows.Forms.Button _btnLoadPrograms;
        private System.Windows.Forms.Button _btnClearPrograms;
#endif
        #endregion Variables Jobs

        public frmJobTHTrueMilk()
        {
            InitializeComponent();
            Shared.DMCamera = DMCamera;
            WindowState = FormWindowState.Maximized;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Shared.Settings.PLCVersion = 1;  // THTrueMilk luôn dùng PLC Version 1 (radioV1)
            Shared.Settings.SensorControllerPort2 = 0;  // THTrueMilk chỉ dùng 1 port PLC
            InitControls();
            DMCamera.InitCameraVariables();
            SetLanguage();
            InitUI();
            InitEvents();
            SetupResponsiveLayout();
            StartQrBankServer();
            InitMidnightResetTimer();
            InitCollapsePanel();
            try { _ = LoadCurrentBatchQrFromDbAsync(); }
            catch (Exception ex) { ProjectLogger.WriteError("[OnHandleCreated] LoadCurrentBatchQrFromDb: " + ex.Message); }
            UpdateStatusLabelDatabase();
            ApplyPermissions();
#if DEBUG
            InitializeDebugControls();
#endif

            if (!string.IsNullOrEmpty(Shared.Settings.LineId))
                _ = AutoLoadProductsAsync();
            // Thêm vào cuối OnHandleCreated — khởi động retry + cleanup (tạm dừng auto, chỉ sync tay)
            var retryService = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
                .RLinkLogRetrySenderService.Instance;
            retryService.Start();
            retryService.Pause();

            BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
                .RLinkAutoCleanupService.Instance.Start();

            // ── Timer kiểm tra kết nối R-Link Master (icon do OnConnectionChanged của heartbeat quản lý) ──
            _connectCheckTimer = new System.Windows.Forms.Timer { Interval = 20_000 };
            _connectCheckTimer.Tick += (_, __) => { };
            _connectCheckTimer.Start();

            // ── QR mark-used retry timer (mỗi 30 giây) ──────────────────
            _qrMarkRetryTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
            _qrMarkRetryTimer.Tick += async (_, __) =>
            {
                try
                {
                    var svc = RLinkMasterServiceFactory.Instance;
                    if (svc == null) return;
                    var pending = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.GetUnsentMarkedQrPg(10);
                    foreach (var (qrCode, batch, pid, prod, exp, jobName, printedAt) in pending)
                    {
                        if (string.IsNullOrWhiteSpace(qrCode)) continue;
                        try
                        {
                            ProjectLogger.WriteInfo($"[QR-MARK] TIMER gửi: {qrCode} job={jobName} batch={batch} product_id={pid}");
                            bool ok = await svc.MarkQrUsedAsync(
                                new System.Collections.Generic.List<string> { qrCode },
                                jobName ?? "",
                                Shared.Settings?.LineId ?? "",
                                Shared.Settings?.LineName ?? "",
                                Shared.Settings?.FactoryCode ?? "",
                                batch, pid, prod, exp, printedAt);
                            if (ok)
                                BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.MarkQrAsSentToMaster(qrCode);
                            else
                                ProjectLogger.WriteWarning($"[QR-MARK] TIMER API fail: {qrCode}");
                        }
                        catch (Exception ex) { ProjectLogger.WriteError($"[QR-MARK] TIMER exception: {qrCode} - {ex.Message}"); }
                    }
                }
                catch { }
            };
            // _qrMarkRetryTimer.Start(); // Disabled: BackupPrintedResponseAsync trong frmMain đã xử lý mark-used

            // ── Monitor heartbeat — singleton ─────────────────────────────
            var monitor = RLinkMonitorService.Create(BuildJobMonitorSnapshot, "frmJob");
            int monitorInterval = Shared.Settings.THMonitorInterval * 60;
            Console.WriteLine($"[frmJob] SẮP gọi monitor.Start(interval={monitorInterval})");
            monitor.Start(monitorInterval, startDelaySeconds: 10);
            Console.WriteLine($"[frmJob] ĐÃ gọi monitor.Start xong");
            _ = Task.Run(() => monitor.SendImmediate());

            // ── Cập nhật icon server theo kết nối heartbeat ──
            monitor.OnConnectionChanged = (connected) =>
            {
                try
                {
                    if (lblConnectServer != null)
                        BeginInvoke(new Action(() =>
                            lblConnectServer.Image = connected
                                ? Properties.Resources.icons8_cloud_database_25
                                : Properties.Resources.icons8_cloud_database_25__1_));
                }
                catch { }
            };

            // ── Khi frmJob activated lại → đổi snapshot factory + re-set callback về frmJob ──
            this.Activated += (_, __) =>
            {
                if (!Shared.IsMainFormRunning)
                {
                    var mon = RLinkMonitorService.Instance;
                    if (mon != null)
                    {
                        // Re-set callback để icon frmJob được cập nhật
                        mon.OnConnectionChanged = (connected) =>
                        {
                            try
                            {
                                if (lblConnectServer != null)
                                    BeginInvoke(new Action(() =>
                                        lblConnectServer.Image = connected
                                            ? Properties.Resources.icons8_cloud_database_25
                                            : Properties.Resources.icons8_cloud_database_25__1_));
                            }
                            catch { }
                        };
                        // Re-set snapshot factory
                        mon.SetSnapshotFactory(BuildJobMonitorSnapshot);
                    }
                }
            };
        }

        private CancellationTokenSource _syncCts;
        private System.Threading.Tasks.TaskCompletionSource<bool> _syncConfirmTcs;
  

        private MonitorPayload BuildJobMonitorSnapshot()
        {
            // Khi frmMain đang mở → frmMain gửi monitor đầy đủ, frmJob bỏ qua
            if (Shared.IsMainFormRunning) return null;

            try
            {
                var settings = Shared.Settings;
                bool printerOk = settings?.PrinterList?.Count > 0
                    && settings.PrinterList[0].IsConnected;
                bool cameraOk = settings?.CameraList?.Count > 0
                    && settings.CameraList[0].IsConnected;
                bool plcOk = Shared.IsSensorControllerConnected;

                string status = (Shared.OperStatus == OperationStatus.Running ||
                                 Shared.OperStatus == OperationStatus.Processing)
                                ? "running" : "stop";

                return new MonitorPayload
                {
                    RLinkName = settings?.RLinkName ?? "",
                    LineId = settings?.LineId ?? "",
                    FactoryCode = settings?.FactoryCode ?? "",
                    IpAddress = GetLocalIPAddress(),

                    Status = status,
                    IsPrinterConnected = printerOk,
                    IsCameraConnected = cameraOk,
                    IsPlcConnected = plcOk,
                    IsDatabaseConnected = Shared.IsDatabaseConnected,
                    IpAddressPrinter = settings?.PrinterList?.Count > 0 ? settings.PrinterList[0].IP : "",
                    IpAddressCamera = settings?.CameraList?.Count > 0 ? settings.CameraList[0].IP : "",
                    IpAddressPlc = settings?.SensorControllerIP ?? "",

                    CurrentJobName = "",
                    CurrentBatch = "",
                    OperatorUser = Shared.LoggedInUser?.UserId ?? Shared.LoggedInUser?.OperatorUserId ?? "ACC001",

                    TotalQrAllocated = GetTotalQrBankCount(),
                    TotalQrUsed = GetTotalQrUsedFromPg(),
                    TotalQrFailed = 0,
                    TotalProduced = 0,

                    ImageErrorFolder = GetErrorImageRootFolder(),
                    TotalImageErrorJob = 0,
                    TotalImageError = CountFilesInFolder(GetErrorImageRootFolder()),

                    Stats = new MonitorStats
                    {
                    },

                    Timestamp = "00:00:00"
                };
            }
            catch { return null; }
        }

        private static string GetErrorImageRootFolder()
        {
            try
            {
                string subFolder = !string.IsNullOrWhiteSpace(Shared.Settings?.THErrorImageFolder)
                    ? Shared.Settings.THErrorImageFolder
                    : "Default";
                return Path.Combine(CommVariables.PathImagesError, subFolder);
            }
            catch { return CommVariables.PathImagesError; }
        }

        private static int CountFilesInFolder(string folder)
        {
            try
            {
                if (Directory.Exists(folder))
                    return Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Length;
            }
            catch { }
            return 0;
        }

        private static string GetLocalIPAddress()
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
            return "";
        }

        private static int _cachedQrBankCountJob = -1;
        private static DateTime _lastQrBankCountTimeJob = DateTime.MinValue;
        private static int GetTotalQrBankCount()
        {
            try
            {
                if (_cachedQrBankCountJob >= 0 && (DateTime.Now - _lastQrBankCountTimeJob).TotalSeconds < 60)
                    return _cachedQrBankCountJob;

                string connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                    "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);
                if (string.IsNullOrWhiteSpace(connStr))
                {
                    using (var conn = new System.Data.SQLite.SQLiteConnection(
                        RLinkLogService.ConnStr))
                    {
                        conn.Open();
                        using (var cmd = new System.Data.SQLite.SQLiteCommand($"SELECT COUNT(*) FROM {THDb.Code}", conn))
                        {
                            _cachedQrBankCountJob = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                else
                {
                    string table = THDb.Code;
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand($"SELECT COUNT(*) FROM \"{table}\"", conn))
                        {
                            _cachedQrBankCountJob = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                _lastQrBankCountTimeJob = DateTime.Now;
                return _cachedQrBankCountJob;
            }
            catch { return _cachedQrBankCountJob >= 0 ? _cachedQrBankCountJob : 0; }
        }

        private static int GetTotalQrUsedFromPg()
        {
            try
            {
                string connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                    "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);
                if (string.IsNullOrWhiteSpace(connStr)) return 0;

                string table = THDb.Code;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(
                        $"SELECT COUNT(*) FROM \"{table}\" WHERE {IsUsed} = TRUE", conn))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch { return 0; }
        }


        private void StartQrBankServer()
        {
            try
            {
                _qrBankAPIHandler = new QrBankAPIHandler("http://*:5002/");
                _qrBankAPIHandler.DataSaved += QrBankHandler_DataSaved;
                _qrBankAPIHandler.RequestArrived += QrBankHandler_RequestArrived;
                ProjectLogger.WriteInfo("[QrBankAPIHandler] Started on http://*:5002/");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[QrBankAPIHandler] Failed to start: " + ex.Message, ex);
            }
        }
        /// <summary>
        /// Xử lý QR codes nhận từ CompleteJob response.
        /// SQLite + PG saving đã được xử lý bởi SaveAllocatedQrCodesAsync trong frmMainTHTrueMilk.
        /// Ở đây chỉ ghi log CSV kiểm toán.
        /// </summary>
        public void ProcessAllocatedQrCodesFromComplete(
            List<string> qrCodes, string lineId, string batch)
        {
            if (qrCodes == null || qrCodes.Count == 0) return;

            // Ghi CSV log (audit trail) — các bảng code/rlink_allocated_qr
            // được lưu bởi SaveAllocatedQrCodesAsync gọi ngay sau method này
            AppendAllocatedQrToCsv(qrCodes, lineId, batch);

            ProjectLogger.WriteInfo(
                $"[ProcessAllocatedQrCodesFromComplete] ✔ {qrCodes.Count} QR | CSV logged" +
                $" | line='{lineId}' batch='{batch}'");
        }

        /// <summary>
        /// Ghi QR codes được cấp phát vào file CSV kiểm toán.
        /// Đường dẫn: %ProgramData%\R-Link\AllocatedQr\allocated_qr_{lineId}_{date}.csv
        /// </summary>
        private void AppendAllocatedQrToCsv(List<string> qrCodes, string lineId, string batch)
        {
            try
            {
                string dir = Path.Combine(
                    CommVariables.PathProgramDataApp,
                    "AllocatedQr");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string safeLineId = string.IsNullOrWhiteSpace(lineId) ? "unknown" : lineId;
                string fileName = $"allocated_qr_{safeLineId}_{DateTime.Today:yyyyMMdd}.csv";
                string filePath = Path.Combine(dir, fileName);

                var sb = new StringBuilder();
                // Header nếu file chưa tồn tại
                if (!File.Exists(filePath))
                    sb.AppendLine($"{QrCode},{ReceivedAt},{LineId},{Batch}");

                string receivedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                foreach (var qr in qrCodes)
                {
                    if (!string.IsNullOrWhiteSpace(qr))
                        sb.AppendLine($"{qr},{receivedAt},{lineId},{batch}");
                }

                File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                ProjectLogger.WriteInfo(
                    $"[AllocatedQrCsv] ✔ Ghi {qrCodes.Count} QR → '{filePath}'");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[AppendAllocatedQrToCsv] Lỗi: " + ex.Message, ex);
            }
        }
        private void QrBankHandler_DataSaved(object sender, QrBankDataReceivedEventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action(() => QrBankHandler_DataSaved(sender, e))); return; }

            ProjectLogger.WriteInfo(
                $"[QrBank] Nhận {e.TotalCount} QR | Batch: {e.Batch} | Line: {e.LineId} | First: {e.FirstQr} | Last: {e.LastQr}");

            // ── QrBank chỉ push vào PostgreSQL — KHÔNG cập nhật mode ──
            // Mode 1 & 2: timer tự đọc PG theo FIFO (HandleMode1DeltaReset / HandleMode2MidnightReset)
            // Mode 3: vẫn dùng ApplyMode3 nếu có
        }
        /// <summary>
        /// Áp dụng phân quyền theo <see cref="Shared.UserPermission"/> vào các control vận hành.
        /// Gọi sau khi đăng nhập online hoặc offline thành công.
        /// </summary>
        public void ApplyPermissions()
        {
            var perm = Shared.UserPermission;
            if (perm == null) return;

            if (InvokeRequired) { Invoke(new Action(ApplyPermissions)); return; }

            bool canJob = perm.CreateJob;

            // ── Tạo / Start Job ──────────────────────────────────────────
            btnSave.Enabled = canJob;   // Save job (tab SelectJob)
            saveJobTH.Enabled = canJob;   // Tạo job Mode 1/2/3 (tabGetPO)
            btnNext.Enabled = canJob;   // Bắt đầu chạy job → frmMainTHTrueMilk

            // ── Xóa Job ──────────────────────────────────────────────────
            btnDelete.Enabled = perm.DeleteJob && Shared.Settings.IsAllowJobDeletion;

            // ── Settings ─────────────────────────────────────────────────
            btnSettings.Enabled = perm.Settings || perm.ProductionSettings || perm.ViewSetting;
        }

        private void SetupResponsiveLayout()
        {
            // ── Form config ─────────────────────────────────────────────
            this.MinimumSize = new System.Drawing.Size(1153, 786);
            this.AutoScroll = true;
            cbcSelectProduct.Width = 800;
          //  panel2.Width = 800;
            // ── Main containers ─────────────────────────────────────────
            if (pnlMain != null) pnlMain.Dock = DockStyle.Fill;
            if (tabControl1 != null) tabControl1.Dock = DockStyle.Fill;

            // ── DataGridViews ───────────────────────────────────────────
            ConfigureDataGridView(dgvItems);
            ConfigureDataGridView(dgvHistoryJob);
            ConfigureDataGridView(dgvSyncLog);
            ConfigureDataGridView(dgvQrIsUsed);
            ConfigureDataGridView(dgvHistoryQr);
            ConfigureDataGridView(materialTable);

            // ── Tab Get PO ──────────────────────────────────────────────
            SetAnchor(txtSearchProduct, AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            SetAnchor(btnSearchProduct, AnchorStyles.Top | AnchorStyles.Right);
            SetAnchor(cbcSelectedModel, AnchorStyles.Top | AnchorStyles.Left);
            if (txtBatchNumber != null) txtBatchNumber.AutoSize = false;
            SetAnchor(txtBatchNumber, AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            
            if (txtTotalProduct != null) txtTotalProduct.AutoSize = false;
            SetAnchor(txtTotalProduct, AnchorStyles.Top | AnchorStyles.Right);
            SetAnchor(label45, AnchorStyles.Top | AnchorStyles.Right);
            SetAnchor(label39, AnchorStyles.Top | AnchorStyles.Left);
            SetAnchor(label28, AnchorStyles.Top | AnchorStyles.Left);
            SetAnchor(label27, AnchorStyles.Top | AnchorStyles.Left);
            SetAnchor(label25, AnchorStyles.Top | AnchorStyles.Left);
            SetAnchor(btnRefreshListProgramFroCameraKeyence, AnchorStyles.Top | AnchorStyles.Left);
            SetAnchor(button2, AnchorStyles.Top | AnchorStyles.Right);
          

            //// ── Dock Fill cho các flowLayoutPanel trong tableLayoutPanel1 ──
            //if (flowLayoutPanel1 != null) flowLayoutPanel1.Dock = DockStyle.Fill;
            //if (flowLayoutPanel2 != null) flowLayoutPanel2.Dock = DockStyle.Fill;
            //if (flowLayoutPanel3 != null) flowLayoutPanel3.Dock = DockStyle.Fill;
            //if (flowLayoutPanel4 != null) flowLayoutPanel4.Dock = DockStyle.Fill;
            //if (flowLayoutPanel5 != null) flowLayoutPanel5.Dock = DockStyle.Fill;
            //if (flowLayoutPanel6 != null) flowLayoutPanel6.Dock = DockStyle.Fill;
            //if (flowLayoutPanel7 != null) flowLayoutPanel7.Dock = DockStyle.Fill;
            //if (flowLayoutPanel8 != null) flowLayoutPanel8.Dock = DockStyle.Fill;
            //SetAnchor(txtMaxConsecutiveDefects, AnchorStyles.Top | AnchorStyles.Right);
            //SetAnchor(txtTimeLogSave, AnchorStyles.Top | AnchorStyles.Right);

            tabGetPO.Resize += (s, e) =>
            {
                try
                {
                    int tabW = tabGetPO.ClientSize.Width;
                    int tabH = tabGetPO.ClientSize.Height;
                    int padding = 8;
                    int panelGap = 6;
                    int buttonHeight = btnGetInfo?.Height ?? 76;
                    int bottomMargin = 10;

                    // ══ Buttons LUÔN ở dưới cùng (anchor bottom) ══
                    if (btnGetInfo != null)
                        btnGetInfo.Location = new Point(padding, tabH - buttonHeight - bottomMargin);
                    if (saveJobTH != null)
                        saveJobTH.Location = new Point(tabW - saveJobTH.Width - padding, tabH - buttonHeight - bottomMargin);

                    // ══ cuzPanel5 + cuzPanel1: vị trí phụ thuộc cuzPanel5.Visible ══
                    if (cuzPanel5 != null && cuzPanel5.Visible)
                    {
                        // cuzPanel5 đang hiện → đặt vị trí cuzPanel5 trước
                        cuzPanel5.Location = new Point(padding, padding);
                        cuzPanel5.Width = Math.Max(100, tabW - 2 * padding);
                        cuzPanel5.Height = _expandedHeight;

                        if (cuzPanel1 != null)
                        {
                            cuzPanel1.Location = new Point(padding, cuzPanel5.Bottom + panelGap);
                            cuzPanel1.Width = Math.Max(100, tabW - 2 * padding);
                            cuzPanel1.Height = 135;
                        }
                    }
                    else
                    {
                        // cuzPanel5 ẩn → cuzPanel1 lên đầu
                        if (cuzPanel1 != null)
                        {
                            cuzPanel1.Location = new Point(padding, padding);
                            cuzPanel1.Width = Math.Max(100, tabW - 2 * padding);
                            cuzPanel1.Height = 135;
                        }
                    }

                    // ══ cuzPanel2: LUÔN nằm giữa cuzPanel1 và buttons ══
                    if (cuzPanel2 != null && cuzPanel1 != null && btnGetInfo != null)
                    {
                        cuzPanel2.Location = new Point(padding, cuzPanel1.Bottom + panelGap);
                        cuzPanel2.Width = Math.Max(100, tabW - 2 * padding);
                        // Chiều cao dừng trước buttons — KHÔNG BAO GIỜ đè
                        int maxHeight = btnGetInfo.Top - cuzPanel2.Top - panelGap;
                        cuzPanel2.Height = Math.Max(50, maxHeight);
                    }

                    // tableLayoutPanel1 bên trong cuzPanel5
                    if (tableLayoutPanel1 != null && cuzPanel5 != null && cuzPanel5.Visible)
                    {
                        tableLayoutPanel1.Width = Math.Max(50, cuzPanel5.ClientSize.Width - 14);
                        tableLayoutPanel1.Height = Math.Max(50, cuzPanel5.ClientSize.Height - tableLayoutPanel1.Top - 5);
                    }

                    // tableLayoutPanel3 bên trong cuzPanel1
                    if (tableLayoutPanel3 != null && cuzPanel1 != null)
                    {
                        tableLayoutPanel3.Width = Math.Max(50, cuzPanel1.ClientSize.Width - 14);
                        tableLayoutPanel3.Height = Math.Max(50, cuzPanel1.ClientSize.Height - tableLayoutPanel3.Top - 5);
                    }
                }
                catch { }
            };

            // ── Tab Select Job ──────────────────────────────────────────
            SetAnchor(pnlJobInfomation, AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            SetAnchor(cuzPanel3, AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
            SetAnchor(cuzButtonPurge, AnchorStyles.Top | AnchorStyles.Right);

            // ── Tab Log Sync ────────────────────────────────────────────
            SetAnchor(btnRefreshSyncLog, AnchorStyles.Top | AnchorStyles.Left);
            SetAnchor(btnSyncSelected, AnchorStyles.Top | AnchorStyles.Left);
            SetAnchor(btnStopSyncLog, AnchorStyles.Top | AnchorStyles.Left);
            SetAnchor(syncDataLog, AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);

            // ── Tab QR Is Used ──────────────────────────────────────────
            SetAnchor(txtSearchQr, AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
           // SetAnchor(btnSyncMarkQr, AnchorStyles.Bottom | AnchorStyles.Right);
            SetAnchor(pnlLoadSyncMarkQr, AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);

            // ── Tab History Receive QR ──────────────────────────────────
            SetAnchor(txtSearchHistoryReceiveQr, AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            SetAnchor(btnRefreshHistoryReceiveQr, AnchorStyles.Top | AnchorStyles.Right);

            // ── Tab Sync Data ───────────────────────────────────────────
            SetAnchor(txtSearch, AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            SetAnchor(btnRefreshSearchQr, AnchorStyles.Top | AnchorStyles.Right);

            // ── Tab Create PO Offline ───────────────────────────────────
            SetAnchor(savePOOffline, AnchorStyles.Bottom | AnchorStyles.Right);

            // ── Tab Create RES Offline ──────────────────────────────────
            SetAnchor(CreateRESOffline, AnchorStyles.Bottom | AnchorStyles.Right);
            SetAnchor(cuzPanel4, AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
            SetAnchor(txtSearchTemplate, AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            SetAnchor(listBoxPrintProductTemplate, AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
        }

        private void SetAnchor(Control control, AnchorStyles anchor)
        {
            if (control != null)
                control.Anchor = anchor;
        }

        private void ConfigureDataGridView(DataGridView dgv)
        {
            if (dgv != null)
            {
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            }
        }

        private void ResizeFlowLayoutTextBox(FlowLayoutPanel panel, Control textBox)
        {
            if (panel != null && textBox != null)
            {
                textBox.Width = Math.Max(50, panel.ClientSize.Width - textBox.Left - 5);
            }
        }

       
        /// <summary>
        /// Kiểm tra số QR còn lại trong kho. Nếu < THQrThreshold → CuzAlert + log server.
        /// Có cooldown để tránh spam alert mỗi lần đổi QR.
        /// </summary>
        private async Task CheckQrStockAndAlertAsync(bool bypassCooldown = false)
        {
            try
            {
                // Chỉ áp dụng cooldown khi gọi từ timer, không áp dụng khi QR vừa đổi
                if (!bypassCooldown &&
                    (DateTime.Now - _lastQrLowAlertTime).TotalMinutes < QR_LOW_ALERT_COOLDOWN_MINUTES)
                    return;

                _lastQrLowAlertTime = DateTime.Now;

                // Lấy total từ lần cấp phát đầu, chỉ 1 lần
                if (_initialTotalQr < 0)
                    _initialTotalQr = await GetInitialAllocationTotalAsync();

                int available = await CountAvailableQrInDbAsync(_JobModel?.THJobProductGtin ?? "");
                int threshold = Shared.Settings.THQrThreshold > 0 ? Shared.Settings.THQrThreshold : 30;

                int remainPercent = _initialTotalQr > 0
                    ? (int)(available * 100.0 / _initialTotalQr)
                    : 100;

                if (remainPercent < threshold)
                {
                    if (_qrLowAlertShown) return;

                    _qrLowAlertShown = true;
                    _lastQrLowAlertTime = DateTime.Now;

                    ProjectLogger.WriteWarning($"[QrStock] Kho QR còn {remainPercent}% ({available} mã) — ngưỡng {threshold}%");
                    NotifyDeviceException(DeviceExceptionType.QrStockLow, $"QR stock low: {remainPercent}% ({available}/{_initialTotalQr})");
                    _FormMainPC?.ShowQrLowAlert(available, threshold, remainPercent, _initialTotalQr);
                }
                else
                {
                    _lastQrLowAlertTime = DateTime.MinValue;
                    _qrLowAlertShown = false;
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[CheckQrStockAndAlert] Lỗi: " + ex.Message, ex);
            }
        }
        private void QrBankHandler_RequestArrived(object sender, QrBankRequestEventArgs e)
        {
            ProjectLogger.WriteDebug($"[QrBank] {e.Timestamp:HH:mm:ss} {e.Method} {e.Path} ← {e.ClientIP}");
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _TimerMidnightReset.Stop();    
            _TimerMidnightReset.Dispose(); 
            _qrBankAPIHandler?.Stop();
            _qrBankAPIHandler?.Dispose();

            // Clear cached camera/printer data khi exit
            Shared.Settings.CachedCameraPrograms = new List<string>();
            Shared.Settings.CachedPrinterTemplates = new List<string>();

            BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
                .RLinkLogRetrySenderService.Instance.Stop();
            BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
                .RLinkAutoCleanupService.Instance.Stop();
        }
        private bool _isClosingFromExit = false;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Nếu đang thoát từ Exit() → cho phép đóng
            if (_isClosingFromExit)
            {
                base.OnFormClosing(e);
                return;
            }

            // Nếu user bấm X → gọi Exit() thay vì đóng ngay
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Exit();
                return;
            }

            base.OnFormClosing(e);
        }
        public string CurrentBatchQrCode
        {
            get { lock (_batchQrLock) return _currentBatchQrCode; }
        }
        /// <summary>
        /// Khôi phục QR Code từ PostgreSQL khi app khởi động lại (Mode 1 / Mode 2).
        /// Mode 1: lấy QR is_used=TRUE mới nhất của line → restore QR đang chạy.
        /// Mode 2: lấy QR is_used=FALSE mới nhất (không filter line_id vì QR chưa dùng chưa có line_id) → tiếp tục.
        /// </summary>
        private async System.Threading.Tasks.Task LoadCurrentBatchQrFromDbAsync()
        {
            var mode = _JobModel?.THJobOperatingMode ?? Shared.Settings.THOperatingMode;
            if (mode != THTrueMilkOperatingMode.BatchOneQrCode &&
                mode != THTrueMilkOperatingMode.BatchOneQrCodeNoChange &&
                mode != THTrueMilkOperatingMode.AutoRefreshByTime)
                return;

            await System.Threading.Tasks.Task.Run(() =>
            {
            try
            {
                string table = THDb.Code;

                string connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                    "postgresql",
                    Shared.Settings.THLocalDbServer,
                    Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername,
                    Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);

                string lineId = Shared.Settings.LineId ?? string.Empty;
                string jobGtin = _JobModel?.THJobProductGtin ?? "";
                string modeLabel = (mode == THTrueMilkOperatingMode.BatchOneQrCode || mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange) ? "Mode1" : "Mode2";

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    RLinkLogService.EnsurePgQrBankTable(conn);

                    string sql;
                    Npgsql.NpgsqlCommand cmd;

                    if (mode == THTrueMilkOperatingMode.BatchOneQrCode ||
                        mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
                    {
                        // ── Mode 1/4: lấy QR is_used=TRUE mới nhất theo line_id + gtin ──
                            //Nhan.To_260920: Them manufactured_date vao query de load _currentBatchDate dung ngay QR
                        //Nguyen nhan: used_at la thoi diem danh dau (truoc midnight), manufactured_date la ngay QR that su (sau midnight)
                        //sql = $@"
                        //    SELECT {Id}, {QrCode}, COALESCE({Batch},''), {UsedAt}
                        //    FROM ""{table}""
                        //    WHERE {IsUsed} = TRUE
                        //      AND {LineId} = @line_id
                        //      AND {THDb.ProductGtin} = @gtin
                        //    ORDER BY {UsedAt} DESC
                        //    LIMIT 1";
                        sql = $@"
                            SELECT {Id}, {QrCode}, COALESCE({Batch},''), {UsedAt}, manufactured_date
                            FROM ""{table}""
                            WHERE {IsUsed} = TRUE
                              AND {LineId} = @line_id
                              AND {THDb.ProductGtin} = @gtin
                            ORDER BY {UsedAt} DESC
                            LIMIT 1";
                        cmd = new Npgsql.NpgsqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue(LineId, lineId);
                        cmd.Parameters.AddWithValue("gtin", jobGtin);
                    }
                    else
                    {
                        // ── Mode 2: lấy QR is_used=FALSE gần nhất (FIFO) theo gtin ─────
                        // QR chưa dùng chưa có line_id → KHÔNG filter line_id
                        // Fallback: nếu không có is_used=FALSE thì lấy is_used=TRUE mới nhất của hôm nay
                        sql = $@"
                            SELECT {Id}, {QrCode}, COALESCE({Batch},''), COALESCE({LineId},'')
                            FROM ""{table}""
                            WHERE {IsUsed} = FALSE
                              AND {THDb.ProductGtin} = @gtin
                            ORDER BY {Id} ASC
                            LIMIT 1";
                        cmd = new Npgsql.NpgsqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("gtin", jobGtin);
                    }

                    using (cmd)
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int qrId = reader.GetInt32(0);
                            string qrCode = reader.GetString(1);
                            string batch = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            DateTime usedAt;
                            if (mode == THTrueMilkOperatingMode.BatchOneQrCode ||
                                mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
                                usedAt = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3);
                            else
                                usedAt = DateTime.Now;

                            lock (_batchQrLock)
                            {
                                _currentBatchQrCode = qrCode;
                                _currentBatchId = batch;
                                //Nhan.To_260920: Load _currentBatchDate tu manufactured_date thay vi used_at
                                //Nguyen nhan: used_at = 23:59 ngay 19/09 (truoc midnight), manufactured_date = 20/09 (ngay QR that su)
                                //Dung used_at -> _currentBatchDate = 19/09 -> CheckQrChangeOnStartAsync tu can QR moi -> rebuild lang phi
                                //Dung manufactured_date -> _currentBatchDate = 20/09 -> CheckQrChangeOnStartAsync biet QR da dung -> khong rebuild
                                //_currentBatchDate = usedAt;
                                DateTime batchDate;
                                string mfgStr = reader.IsDBNull(4) ? "" : reader.GetString(4);
                                if (!string.IsNullOrEmpty(mfgStr) &&
                                    DateTime.TryParseExact(mfgStr, "dd MM yy",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None,
                                        out DateTime mfgDate))
                                {
                                    batchDate = mfgDate;
                                }
                                else
                                {
                                    batchDate = usedAt;
                                }
                                _currentBatchDate = batchDate;
                                _currentQrId = qrId;
                                if (_JobModel != null)
                                {
                                    _JobModel.CurrentBatchDate = _currentBatchDate;
                                    _JobModel.CurrentBatchQrCode = _currentBatchQrCode;
                                    _JobModel.CurrentQrId = _currentQrId;
                                }
                            }

                            if (mode == THTrueMilkOperatingMode.AutoRefreshByTime)
                                ResetMode2Countdown();

                            ProjectLogger.WriteInfo(
                                $"[{modeLabel}] Khôi phục QR từ DB: batch='{batch}' → QR='{qrCode}'");
                        }
                        else
                        {
                            // ── Mode 2 fallback: không có QR chưa dùng → tìm QR đang dùng hôm nay ──
                            if (mode == THTrueMilkOperatingMode.AutoRefreshByTime && !string.IsNullOrWhiteSpace(lineId))
                            {
                                reader.Close();
                                string fallbackSql = $@"
                                    SELECT {QrCode}, COALESCE({Batch},'')
                                    FROM ""{table}""
                                    WHERE {IsUsed} = TRUE
                                      AND {LineId}  = @line_id
                                      AND {UsedAt} >= CURRENT_DATE
                                      AND {THDb.ProductGtin} = @gtin
                                    ORDER BY {UsedAt} DESC
                                    LIMIT 1";
                                using (var cmd2 = new Npgsql.NpgsqlCommand(fallbackSql, conn))
                                {
                                    cmd2.Parameters.AddWithValue(LineId, lineId);
                                    cmd2.Parameters.AddWithValue("gtin", jobGtin);
                                    using (var r2 = cmd2.ExecuteReader())
                                    {
                                        if (r2.Read())
                                        {
                                            string qr2 = r2.GetString(0);
                                            string batch2 = r2.IsDBNull(1) ? string.Empty : r2.GetString(1);
                                            lock (_batchQrLock)
                                            {
                                                _currentBatchQrCode = qr2;
                                                _currentBatchId = batch2;
                                                _currentBatchDate = DateTime.Now;
                        if (_JobModel != null)
                        {
                            _JobModel.CurrentBatchDate = _currentBatchDate;
                            _JobModel.CurrentBatchQrCode = _currentBatchQrCode;
                            _JobModel.CurrentQrId = _currentQrId;
                        }
                                                _lastDeltaResetDate = DateTime.Now;
                                            }
                                            ResetMode2Countdown();
                                            ProjectLogger.WriteInfo(
                                                $"[Mode2] Fallback restore QR is_used=TRUE: batch='{batch2}' → QR='{qr2}'");
                                        }
                                        else
                                        {
                                            ProjectLogger.WriteWarning(
                                                $"[{modeLabel}] Không có QR phù hợp trong DB — chờ timer lấy QR.");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ProjectLogger.WriteWarning(
                                    $"[{modeLabel}] Không có QR phù hợp trong DB — chờ timer lấy QR.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[LoadCurrentBatchQrFromDb] lỗi: " + ex.Message, ex);
            }
            });

            UpdateMode1UI();
        }
        /// <summary>
        /// Ghi thêm <paramref name="repeatCount"/> dòng QR mới vào file CSV của job hiện tại.
        /// Gọi sau khi timer đổi QR thành công.
        /// </summary>
        private void AppendQrToCsv(string qrCode, int repeatCount)
        {
            try
            {
                string csvPath = _JobModel?.DirectoryDatabase;
                if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
                {
                    ProjectLogger.WriteWarning($"[AppendQrToCsv] CSV không tồn tại: '{csvPath}'");
                    return;
                }

                var lines = new System.Text.StringBuilder();
                for (int i = 0; i < repeatCount; i++)
                    lines.AppendLine(qrCode);

                File.AppendAllText(csvPath, lines.ToString(), System.Text.Encoding.UTF8);
                ProjectLogger.WriteInfo(
                    $"[AppendQrToCsv] Thêm {repeatCount} dòng QR '{qrCode}' → '{csvPath}'");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[AppendQrToCsv] Lỗi: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Rebuild CSV khi đổi QR code theo timer.
        /// Logic: giữ lại N dòng đã consumed (printed + buffer), thay phần còn lại bằng QR mới.
        /// consumed = NumberOfPrintedCodes + bufferCount
        /// </summary>
        //private void RebuildCsvOnQrSwitch(string newQrCode, string newBatch,
        //                                  string newLineId, string newFactoryCode)
        //{
        //    try
        //    {
        //        string csvPath = _JobModel?.DirectoryDatabase;
        //        if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
        //        {
        //            ProjectLogger.WriteWarning($"[RebuildCsv] CSV không tồn tại: '{csvPath}'");
        //            return;
        //        }

        //        int totalQr = (int)(_JobModel?.NumberTotalsCode > 0
        //            ? _JobModel.NumberTotalsCode : 0);
        //        if (totalQr <= 0)
        //        {
        //            ProjectLogger.WriteWarning("[RebuildCsv] NumberTotalsCode = 0 → bỏ qua.");
        //            return;
        //        }

        //        // ── Số mã đã consumed: printed + buffer đang nạp vào máy in ──
        //        int printed = _FormMainPC?.GetPrintedCount() ?? 0;
        //        int buffer = _JobModel?.THJobBufferCount > 0
        //            ? _JobModel.THJobBufferCount
        //            : (Shared.Settings.THBufferCount > 0 ? Shared.Settings.THBufferCount : 1);

        //        int consumed = Math.Min(printed + buffer, totalQr);
        //        int remaining = totalQr - consumed;

        //        ProjectLogger.WriteInfo(
        //            $"[RebuildCsv] total={totalQr} | printed={printed} | buffer={buffer}" +
        //            $" | consumed={consumed} | remaining={remaining} | newQR='{newQrCode}'");

        //        if (remaining <= 0)
        //        {
        //            ProjectLogger.WriteWarning("[RebuildCsv] Không còn dòng nào để thay → bỏ qua.");
        //            return;
        //        }

        //        // ── Đọc file hiện tại, lấy header + N dòng đầu (consumed) ────
        //        var allLines = File.ReadAllLines(csvPath, Encoding.UTF8);
        //        bool hasHeader = allLines.Length > 0 &&
        //                         allLines[0].StartsWith(QrCode, StringComparison.OrdinalIgnoreCase);

        //        int headerCount = hasHeader ? 1 : 0;
        //        var keepLines = new List<string>();

        //        // Giữ header
        //        if (hasHeader)
        //            keepLines.Add(allLines[0]);

        //        // Giữ consumed dòng data đã in/buffer
        //        for (int i = headerCount; i < headerCount + consumed && i < allLines.Length; i++)
        //            keepLines.Add(allLines[i]);

        //        // Thêm remaining dòng QR mới
        //        string newLine = $"{newQrCode},{newBatch},{newLineId},{newFactoryCode}";
        //        for (int i = 0; i < remaining; i++)
        //            keepLines.Add(newLine);

        //        // Ghi đè file
        //        File.WriteAllLines(csvPath, keepLines, Encoding.UTF8);

        //        ProjectLogger.WriteInfo(
        //            $"[RebuildCsv] ✔ Ghi lại CSV: {keepLines.Count - headerCount} dòng" +
        //            $" ({consumed} × cũ + {remaining} × '{newQrCode}')");
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError("[RebuildCsv] Lỗi: " + ex.Message, ex);
        //    }
        //}
        private void RebuildCsvOnQrSwitch(string newQrCode, string newBatch,
                                  string newLineId, string newFactoryCode)
        {
            try
            {
                string csvPath = _JobModel?.DirectoryDatabase;
                if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
                {
                    ProjectLogger.WriteWarning($"[RebuildCsv] CSV không tồn tại: '{csvPath}'");
                    return;
                }

                int totalQr = (int)(_JobModel?.NumberTotalsCode > 0 ? _JobModel.NumberTotalsCode : 0);
                if (totalQr <= 0) return;

                int printed = _FormMainPC?.GetPrintedCount() ?? 0;
                int buffer = _JobModel?.THJobBufferCount > 0
                    ? _JobModel.THJobBufferCount
                    : (Shared.Settings.THBufferCount > 0 ? Shared.Settings.THBufferCount : 1);

                int consumed = Math.Min(printed + buffer, totalQr);
                int remaining = totalQr - consumed;

                if (remaining <= 0)
                {
                    ProjectLogger.WriteWarning("[RebuildCsv] Không còn dòng nào để thay → bỏ qua.");
                    return;
                }

                // ── Đọc file, KHÔNG có header ─────────────────────────────
                var allLines = File.ReadAllLines(csvPath, Encoding.UTF8);

                var keepLines = new List<string>();

                // Giữ consumed dòng đầu (đã in/buffer)
                for (int i = 0; i < consumed && i < allLines.Length; i++)
                    keepLines.Add(allLines[i]);

                // Thêm remaining dòng QR mới (chỉ qr_code)
                for (int i = 0; i < remaining; i++)
                    keepLines.Add(newQrCode);

                File.WriteAllLines(csvPath, keepLines, Encoding.UTF8);

                ProjectLogger.WriteInfo(
                    $"[RebuildCsv] ✔ {keepLines.Count} dòng ({consumed} × cũ + {remaining} × '{newQrCode}')");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RebuildCsv] Lỗi: " + ex.Message, ex);
            }
        }
        private void InitMidnightResetTimer()
        {
            _TimerMidnightReset.Interval = 1_000; // 1 giây
            _TimerMidnightReset.Tick += TimerMidnightReset_Tick;
            _TimerMidnightReset.Start();
        }
        private void LblMasterOffline_Click(object sender, EventArgs e)
        {
            lblMasterOffline.Visible = false;
        }

        private void TimerMidnightReset_Tick(object sender, EventArgs e)
        {
            var mode = _JobModel?.THJobOperatingMode ?? Shared.Settings.THOperatingMode;

            if (mode != THTrueMilkOperatingMode.BatchOneQrCode &&
                mode != THTrueMilkOperatingMode.BatchOneQrCodeNoChange &&
                mode != THTrueMilkOperatingMode.AutoRefreshByTime)
            {
                _mode1GuardLogCount++;
                if (_mode1GuardLogCount % 72 == 0)
                    ProjectLogger.WriteDebug($"[Mode1] Timer tick #{_mode1GuardLogCount} — mode={mode} ≠ Mode1/2/4 → bỏ qua");
                return;
            }
            _mode1GuardLogCount = 0;

            if (mode == THTrueMilkOperatingMode.BatchOneQrCode)
                HandleMode1DeltaReset();
            else if (mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
            {
                // Mode 4: giữ QR đầu tiên, không delta reset
                UpdateMode1UI();
            }
            else // AutoRefreshByTime — đếm ngược thời gian chạy thực tế
            {
                bool isRunning = Shared.OperStatus == OperationStatus.Running
                              || Shared.OperStatus == OperationStatus.Processing;

                lock (_batchQrLock)
                {
                    if (string.IsNullOrEmpty(_currentBatchQrCode))
                    {
                        // Cold start: fetch ngay
                        HandleMode2MidnightReset();
                        return;
                    }
                }

                if (!isRunning) return;

                if (_mode2RemainingSeconds > 0)
                {
                    _mode2RemainingSeconds--;
                    if (_mode2RemainingSeconds <= 0)
                        HandleMode2MidnightReset();
                }
            }

            UpdateMode1UI();

            // Kiểm tra kết nối R-Link Master — chỉ hiện trên frmMain
        }

        private DateTime _lastDeltaResetDate = DateTime.MinValue;
        private bool _isMode1Resetting = false;
        private int _mode1GuardLogCount = 0;
        //        private async void HandleMode1DeltaReset()
        //        {
        //            if (_isMode1Resetting) return;
        //            // ── Chỉ đổi QR khi job đang chạy ──────────────────────────────────────
        //            if (Shared.OperStatus != OperationStatus.Running &&
        //                Shared.OperStatus != OperationStatus.Processing)
        //            {
        //                _mode1GuardLogCount++;
        //                if (_mode1GuardLogCount % 72 == 0)
        //                    ProjectLogger.WriteDebug("[Mode1] Timer fired nhưng job chưa start → bỏ qua delta reset.");
        //                return;
        //            }
        //            _isMode1Resetting = true;
        //            try
        //            {
        //                string oldQr = string.Empty;
        //                bool shouldFetch = false;
        //                bool isColdStart = false; // restart vào ngày mới, chưa có QR
        //                bool isNewDay = false;

        //                lock (_batchQrLock)
        //                {
        //                    // Khôi phục _currentBatchDate từ job file nếu restart
        //                    if (_currentBatchDate == DateTime.MinValue && _JobModel?.CurrentBatchDate > DateTime.MinValue)
        //                        _currentBatchDate = _JobModel.CurrentBatchDate;

        //                    // Khôi phục _currentBatchQrCode từ job file nếu restart
        //                    if (string.IsNullOrEmpty(_currentBatchQrCode) && !string.IsNullOrEmpty(_JobModel?.CurrentBatchQrCode))
        //                    {
        //                        _currentBatchQrCode = _JobModel.CurrentBatchQrCode;
        //                        ProjectLogger.WriteInfo($"[Mode1] Khôi phục QR từ job file: '{_currentBatchQrCode}' | batchDate={_currentBatchDate:yyyy-MM-dd}");
        //                    }

        //                    // Khôi phục _currentQrId từ job file nếu restart
        //                    if (_currentQrId < 0 && _JobModel?.CurrentQrId > 0)
        //                    {
        //                        _currentQrId = _JobModel.CurrentQrId;
        //                        ProjectLogger.WriteInfo($"[Mode1] Khôi phục QR id từ job file: {_currentQrId}");
        //                    }

        //                    int delta = (_JobModel?.THJobDeltaMinutes > 0)
        //                        ? _JobModel.THJobDeltaMinutes
        //                        : (Shared.Settings.THDeltaMinutes > 0 ? Shared.Settings.THDeltaMinutes : 0);

        //                    // ── Cold start: chưa có QR (restart sang ngày mới) → fetch ngay ──
        //                    if (string.IsNullOrEmpty(_currentBatchQrCode))
        //                    {
        //                        isColdStart = true;
        //                        shouldFetch = true;
        //                        _noQrWarningShown = false;
        //                        ProjectLogger.WriteInfo("[Mode1] Cold start — chưa có QR, fetch ngay từ DB.");
        //                    }
        //                    else
        //                    {
        //                        // ── Delta reset bình thường ──────────────────────────────────
        //#if DEBUG
        //                        // Bỏ qua delta check nhưng vẫn giữ guard 1-ngày-1-lần để tránh đổi QR liên tục
        //                        if (_lastDeltaResetDate.Date == DateTime.Today)
        //                        {
        //                            ProjectLogger.WriteDebug($"[Mode1] DEBUG — đã reset hôm nay ({_lastDeltaResetDate:HH:mm:ss}), bỏ qua");
        //                            return;
        //                        }
        //                        DateTime resetAt;
        //                        string timeStr = _txtDebugResetTime?.Text?.Trim() ?? "";
        //                        if (TimeSpan.TryParseExact(timeStr, "hh\\:mm",
        //                            System.Globalization.CultureInfo.InvariantCulture, out TimeSpan debugTs))
        //                        {
        //                            resetAt = DateTime.Today.Add(debugTs).AddSeconds(-delta);
        //                            // resetAt = DateTime.Today.Add(debugTs);
        //                        }
        //                        else
        //                            resetAt = DateTime.Now.AddSeconds(35);
        //                        if (DateTime.Now < resetAt) return;
        //#else
        //                        if (delta <= 0)
        //                        {
        //                            ProjectLogger.WriteWarning("[Mode1] Bỏ qua — delta=0 (THDeltaMinutes chưa được cấu hình?)");
        //                            return;
        //                        }

        //                        // ── TH2: Day-boundary — QR từ ngày cũ → đổi ngay ──
        //                        isNewDay = _currentBatchDate != DateTime.MinValue
        //                                && _currentBatchDate.Date < DateTime.Today;

        //                        if (isNewDay && _JobModel?.CurrentBatchDate.Date >= DateTime.Today)
        //                        {
        //                            ProjectLogger.WriteInfo(
        //                                $"[Mode1] Day-boundary skipped — QR đã được delta reset cho hôm nay (batchDate={_JobModel?.CurrentBatchDate:yyyy-MM-dd}).");
        //                            isNewDay = false;
        //                        }

        //                        DateTime resetAt = DateTime.Today.AddDays(1).AddSeconds(-delta);

        //                        if (!isNewDay)
        //                        {
        //                            if (_lastDeltaResetDate.Date == DateTime.Today)
        //                            {
        //                                ProjectLogger.WriteWarning($"[Mode1] Bỏ qua — đã reset hôm nay ({_lastDeltaResetDate:HH:mm:ss}), guard chặn retry");
        //                                return;
        //                            }
        //                            if (DateTime.Now < resetAt)
        //                            {
        //                                _mode1GuardLogCount++;
        //                                if (_mode1GuardLogCount % 72 == 0)
        //                                    ProjectLogger.WriteDebug($"[Mode1] Chưa tới giờ — resetAt={resetAt:HH:mm}, delta={delta} (lần #{_mode1GuardLogCount})");
        //                                return;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            ProjectLogger.WriteInfo(
        //                                $"[Mode1] Day-boundary — QR cũ từ {_currentBatchDate:yyyy-MM-dd}, đổi sang code mới.");
        //                        }
        //#endif

        //                        oldQr = _currentBatchQrCode;
        //                        shouldFetch = true;

        //                        ProjectLogger.WriteInfo(
        //                            $"[Mode1] {(isNewDay ? "Day-boundary" : "Delta")} reset lúc {DateTime.Now:HH:mm:ss} | QR cũ='{oldQr}'");
        //                    }
        //                }

        //                if (!shouldFetch) return;

        //                if (isColdStart || isNewDay)
        //                    _currentQrId = -1;

        //                var (nextId, nextQr) = await GetNextQrFromDatabaseAsync();

        //                if (nextId >= 0 && !string.IsNullOrEmpty(nextQr) && nextQr != oldQr)
        //                {
        //                    _noQrWarningShown = false;
        //                    lock (_batchQrLock)
        //                    {
        //                        _currentBatchQrCode = nextQr;
        //                        _currentQrId = nextId;
        //                        _currentBatchId = string.Empty;
        //                        // Delta reset: QR luôn dành cho ngày mai
        //                        // Các TH khác (cold start, day-boundary) → QR dành cho hôm nay
        //                        bool isDeltaReset = !isColdStart && !isNewDay;
        //                        _currentBatchDate = isDeltaReset ? DateTime.Today.AddDays(1) : DateTime.Now;
        //                        if (_JobModel != null)
        //                        {
        //                            _JobModel.CurrentBatchDate = _currentBatchDate;
        //                            _JobModel.CurrentBatchQrCode = _currentBatchQrCode;
        //                        }
        //                        // Guard đã được set ở else block cho delta reset (không set cho cold start / day-boundary)
        //                    }

        //                    var ids = new List<int> { nextId };
        //                    var qrCodes = new List<string> { nextQr };
        //                    await MarkQrAsUsedAsync(ids, qrCodes);

        //                    // Chỉ set guard khi reset thực sự thành công
        //                    _lastDeltaResetDate = DateTime.Now;

        //                    ProjectLogger.WriteInfo(
        //                        $"[Mode1] {(isColdStart ? "Cold start" : "Delta reset")} → QR mới: '{oldQr}' → '{nextQr}' | id={nextId}");
        //                    Mode1QrCodeChanged?.Invoke(this, (nextQr, _currentBatchDate));
        //                    _ = CheckQrStockAndAlertAsync(bypassCooldown: true);
        //                }
        //                else
        //                {
        //                    ProjectLogger.WriteWarning(
        //                        $"[Mode1] Không tìm thấy QR hợp lệ — {(isColdStart ? "chờ QrBank đẩy dữ liệu" : $"tiếp tục dùng QR cũ '{oldQr}'")}");

        //                    // ── Hiển thị thông báo + dừng in khi hết buffer ──
        //                    if (!isColdStart && !_noQrWarningShown)
        //                    {
        //                        _noQrWarningShown = true;
        //                        Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
        //                        _FormMainPC?.BeginInvoke(new Action(() =>
        //                        {
        //                            CustomMessageBox.ShowCenterScreen(
        //                                "Không tìm thấy QR hợp lệ cho job.\nDừng in để chờ QrBank đẩy dữ liệu mới.",
        //                                "QR không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //                        }));
        //                    }
        //                }

        //                UpdateMode1UI();
        //            }
        //            finally
        //            {
        //                _isMode1Resetting = false;
        //            }
        //        }

        private async void HandleMode1DeltaReset()
        {
            if (_isMode1Resetting) return;
            // ── Chỉ đổi QR khi job đang chạy ──────────────────────────────────────
            if (Shared.OperStatus != OperationStatus.Running &&
                Shared.OperStatus != OperationStatus.Processing)
            {
                _mode1GuardLogCount++;
                if (_mode1GuardLogCount % 72 == 0)
                    ProjectLogger.WriteDebug("[Mode1] Timer fired nhưng job chưa start → bỏ qua delta reset.");
                return;
            }
            _isMode1Resetting = true;
            try
            {
                string oldQr = string.Empty;
                bool shouldFetch = false;
                bool isColdStart = false; // restart vào ngày mới, chưa có QR
                bool isNewDay = false;

                lock (_batchQrLock)
                {
                    // Khôi phục _currentBatchDate từ job file nếu restart
                    if (_currentBatchDate == DateTime.MinValue && _JobModel?.CurrentBatchDate > DateTime.MinValue)
                        _currentBatchDate = _JobModel.CurrentBatchDate;

                    // Khôi phục _currentBatchQrCode từ job file nếu restart
                    if (string.IsNullOrEmpty(_currentBatchQrCode) && !string.IsNullOrEmpty(_JobModel?.CurrentBatchQrCode))
                    {
                        _currentBatchQrCode = _JobModel.CurrentBatchQrCode;
                        ProjectLogger.WriteInfo($"[Mode1] Khôi phục QR từ job file: '{_currentBatchQrCode}' | batchDate={_currentBatchDate:yyyy-MM-dd}");
                    }

                    // Khôi phục _currentQrId từ job file nếu restart
                    if (_currentQrId < 0 && _JobModel?.CurrentQrId > 0)
                    {
                        _currentQrId = _JobModel.CurrentQrId;
                        ProjectLogger.WriteInfo($"[Mode1] Khôi phục QR id từ job file: {_currentQrId}");
                    }

                    int delta = (_JobModel?.THJobDeltaMinutes > 0)
                        ? _JobModel.THJobDeltaMinutes
                        : (Shared.Settings.THDeltaMinutes > 0 ? Shared.Settings.THDeltaMinutes : 0);

                    // ── Cold start: chưa có QR (restart sang ngày mới) → fetch ngay ──
                    if (string.IsNullOrEmpty(_currentBatchQrCode))
                    {
                        isColdStart = true;
                        shouldFetch = true;
                        _noQrWarningShown = false;
                        ProjectLogger.WriteInfo("[Mode1] Cold start — chưa có QR, fetch ngay từ DB.");
                    }
                    else
                    {
                        // ── Delta reset bình thường ──────────────────────────────────
#if DEBUG
                        // Bỏ qua delta check nhưng vẫn giữ guard 1-ngày-1-lần để tránh đổi QR liên tục
                        if (_lastDeltaResetDate.Date == DateTime.Today)
                        {
                            ProjectLogger.WriteDebug($"[Mode1] DEBUG — đã reset hôm nay ({_lastDeltaResetDate:HH:mm:ss}), bỏ qua");
                            return;
                        }
                        DateTime resetAt;
                        string timeStr = _txtDebugResetTime?.Text?.Trim() ?? "";
                        if (TimeSpan.TryParseExact(timeStr, "hh\\:mm",
                            System.Globalization.CultureInfo.InvariantCulture, out TimeSpan debugTs))
                        {
                            resetAt = DateTime.Today.Add(debugTs).AddSeconds(-delta);
                            // resetAt = DateTime.Today.Add(debugTs);
                        }
                        else
                            resetAt = DateTime.Now.AddSeconds(35);
                        if (DateTime.Now < resetAt) return;
#else
                        if (delta <= 0)
                        {
                            ProjectLogger.WriteWarning("[Mode1] Bỏ qua — delta=0 (THDeltaMinutes chưa được cấu hình?)");
                            return;
                        }

                        // ── TH2: Day-boundary — QR từ ngày cũ → đổi ngay ──
                        isNewDay = _currentBatchDate != DateTime.MinValue
                                && _currentBatchDate.Date < DateTime.Today;

                        if (isNewDay && _JobModel?.CurrentBatchDate.Date >= DateTime.Today)
                        {
                            ProjectLogger.WriteInfo(
                                $"[Mode1] Day-boundary skipped — QR đã được delta reset cho hôm nay (batchDate={_JobModel?.CurrentBatchDate:yyyy-MM-dd}).");
                            isNewDay = false;
                        }

                        DateTime resetAt = DateTime.Today.AddDays(1).AddSeconds(-delta);

                        if (!isNewDay)
                        {
                            if (_lastDeltaResetDate.Date == DateTime.Today)
                            {
                                ProjectLogger.WriteWarning($"[Mode1] Bỏ qua — đã reset hôm nay ({_lastDeltaResetDate:HH:mm:ss}), guard chặn retry");
                                return;
                            }
                            if (DateTime.Now < resetAt)
                            {
                                _mode1GuardLogCount++;
                                if (_mode1GuardLogCount % 72 == 0)
                                    ProjectLogger.WriteDebug($"[Mode1] Chưa tới giờ — resetAt={resetAt:HH:mm}, delta={delta} (lần #{_mode1GuardLogCount})");
                                return;
                            }
                        }
                        else
                        {
                            ProjectLogger.WriteInfo(
                                $"[Mode1] Day-boundary — QR cũ từ {_currentBatchDate:yyyy-MM-dd}, đổi sang code mới.");
                        }
#endif

                        oldQr = _currentBatchQrCode;
                        shouldFetch = true;

                        ProjectLogger.WriteInfo(
                            $"[Mode1] {(isNewDay ? "Day-boundary" : "Delta")} reset lúc {DateTime.Now:HH:mm:ss} | QR cũ='{oldQr}'");
                    }
                }

                if (!shouldFetch) return;

                if (isColdStart || isNewDay)
                    _currentQrId = -1;

                // Tính ngày cần filter added_date: cold start/day-boundary = hôm nay, delta reset = ngày mai
                bool isDeltaResetFetch = !isColdStart && !isNewDay;
                DateTime fetchAddedDate = isDeltaResetFetch ? DateTime.Today.AddDays(1) : DateTime.Now.Date;

                var (nextId, nextQr) = await GetNextQrFromDatabaseAsync();

                if (nextId >= 0 && !string.IsNullOrEmpty(nextQr) && nextQr != oldQr)
                {
                    _noQrWarningShown = false;
                    _needsQrChange = false;
                    lock (_batchQrLock)
                    {
                        _currentBatchQrCode = nextQr;
                        _currentQrId = nextId;
                        _currentBatchId = string.Empty;
                        // Delta reset: QR luôn dành cho ngày mai
                        // Các TH khác (cold start, day-boundary) → QR dành cho hôm nay
                        bool isDeltaReset = !isColdStart && !isNewDay;
                        _currentBatchDate = isDeltaReset ? DateTime.Today.AddDays(1) : DateTime.Now;
                        if (_JobModel != null)
                        {
                            _JobModel.CurrentBatchDate = _currentBatchDate;
                            _JobModel.CurrentBatchQrCode = _currentBatchQrCode;
                        }
                        // Guard đã được set ở else block cho delta reset (không set cho cold start / day-boundary)
                    }

                    var ids = new List<int> { nextId };
                    var qrCodes = new List<string> { nextQr };
                    await MarkQrAsUsedAsync(ids, qrCodes);

                    // Chỉ set guard khi reset thực sự thành công
                    _lastDeltaResetDate = DateTime.Now;

                    ProjectLogger.WriteInfo(
                        $"[Mode1] {(isColdStart ? "Cold start" : "Delta reset")} → QR mới: '{oldQr}' → '{nextQr}' | id={nextId}");
                    Mode1QrCodeChanged?.Invoke(this, (nextQr, _currentBatchDate));
                    _ = CheckQrStockAndAlertAsync(bypassCooldown: true);
                }
                else
                {
                    ProjectLogger.WriteWarning(
                        $"[Mode1] Không tìm thấy QR hợp lệ — {(isColdStart ? "chờ QrBank đẩy dữ liệu" : $"tiếp tục dùng QR cũ '{oldQr}'")}");

                    // ── Hiển thị thông báo + dừng in khi hết buffer ──
                    if (!_noQrWarningShown)
                    {
                        _noQrWarningShown = true;
                        _needsQrChange = true;
                        Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                        _FormMainPC?.BeginInvoke(new Action(() =>
                        {
                            _FormMainPC?.RequestAutoStop("QR không hợp lệ");
                            CustomMessageBox.ShowCenterScreen(
                                "Không tìm thấy QR hợp lệ cho job.\nDừng in để chờ QrBank đẩy dữ liệu mới.",
                                "QR không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                    }

                }

                UpdateMode1UI();
            }
            finally
            {
                _isMode1Resetting = false;
            }
        }

        /// <summary>
        /// Lấy QR tiếp theo từ DB theo FIFO.
        /// Trả về (id, qrCode) — id dùng để mark is_used sau khi áp dụng thành công.
        /// </summary>
        private async Task<(int id, string qrCode)> GetNextQrFromDatabaseAsync()
        {
            return await Task.Run(() =>
            {
                string lineId = Shared.Settings.LineId ?? string.Empty;

                // ── Lấy GTIN từ job để filter QR hợp lệ ──
                string jobGtin = _JobModel?.THJobProductGtin ?? "";
                string baseUrl = Shared.Settings.THQrBaseUrl ?? "";
                int numberOfUrl = Shared.Settings.THQrNumberOfUrl;

                try
                {
                    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                    if (string.IsNullOrWhiteSpace(connStr))
                        connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                            "postgresql",
                            Shared.Settings.THLocalDbServer,
                            Shared.Settings.THLocalDbPort,
                            Shared.Settings.THLocalDbUsername,
                            Shared.Settings.THLocalDbPassword,
                            Shared.Settings.THLocalDbDatabase);

                    if (string.IsNullOrWhiteSpace(connStr))
                    {
                        ProjectLogger.WriteWarning("[Mode1/2] GetNextQrFromDatabaseAsync: không có connStr → SQLite fallback.");
                        var sqliteQr = RLinkLogService.FetchQrCodesFromSQLite(lineId, 20, jobGtin, baseUrl, numberOfUrl);
                        if (sqliteQr != null && sqliteQr.Count > 0)
                        {
                            ProjectLogger.WriteInfo($"[Mode1/2] GetNextQr → SQLite fallback (no PG conn): id={sqliteQr[0].id}");
                            return (sqliteQr[0].id, sqliteQr[0].qrCode);
                        }
                        return (-1, string.Empty);
                    }

                    string table = THDb.Code;

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        RLinkLogService.EnsurePgQrBankTable(conn);

                        // Query với GTIN filter + baseurl + numberOfUrl validation
                        // Bỏ qua QR đang dùng (nếu có) để không lấy lại QR cũ
                        string baseSql = $"SELECT {Id}, {QrCode} FROM \"{table}\" WHERE {IsUsed} = FALSE";
                        if (!string.IsNullOrWhiteSpace(jobGtin))
                            baseSql += $" AND {THDb.ProductGtin} = @gtin";
                        if (!string.IsNullOrWhiteSpace(lineId))
                            baseSql += $" AND {LineId} = @line_id";
                        //if (_currentQrId > 0)
                        //    baseSql += $" AND {Id} > {_currentQrId}";
                        baseSql += $" ORDER BY {Id} ASC";

                        using (var cmd = new Npgsql.NpgsqlCommand(baseSql, conn))
                        {
                            if (!string.IsNullOrWhiteSpace(jobGtin))
                                cmd.Parameters.AddWithValue("gtin", jobGtin);
                            if (!string.IsNullOrWhiteSpace(lineId))
                                cmd.Parameters.AddWithValue(LineId, lineId);

                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    int id = r.GetInt32(0);
                                    string qr = r.IsDBNull(1) ? "" : r.GetString(1);
                                    if (string.IsNullOrEmpty(qr)) continue;

                                    // Validate baseurl + numberOfUrl
                                    bool urlOk = string.IsNullOrEmpty(baseUrl)
                                        || qr.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase);
                                    bool lenOk = numberOfUrl <= 0 || qr.Length == numberOfUrl;

                                    if (urlOk && lenOk)
                                    {
                                        ProjectLogger.WriteInfo($"[Mode1/2] GetNextQr → id={id}, qr='{qr}' (GTIN={jobGtin})");
                                        return (id, qr);
                                    }
                                }
                            }
                        }

                        ProjectLogger.WriteWarning($"[Mode1/2] GetNextQr: không tìm thấy QR hợp lệ (GTIN={jobGtin}, baseUrl={baseUrl}, len={numberOfUrl})");
                        // SQLite fallback đã bỏ — PG đã query đúng GTIN + lineId + validate baseUrl/len
                        return (-1, string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[Mode1/2] GetNextQrFromDatabaseAsync lỗi: " + ex.Message, ex);
                    var sqliteQr = RLinkLogService.FetchQrCodesFromSQLite(lineId, 20, jobGtin, baseUrl, numberOfUrl);
                    if (sqliteQr != null && sqliteQr.Count > 0)
                    {
                        ProjectLogger.WriteInfo($"[Mode1/2] GetNextQr → SQLite fallback (PG error): id={sqliteQr[0].id}");
                        return (sqliteQr[0].id, sqliteQr[0].qrCode);
                    }
                    return (-1, string.Empty);
                }
            });
        }

        /// <summary>
        /// Kiểm tra trước khi Start: job có cần QR mới không?
        /// Trả về true = OK (có QR mới hoặc không cần đổi), false = chặn Start.
        /// </summary>
        public async Task<bool> CheckQrChangeOnStartAsync()
        {
            var mode = _JobModel?.THJobOperatingMode ?? Shared.Settings.THOperatingMode;

            if (mode != THTrueMilkOperatingMode.BatchOneQrCode &&
                mode != THTrueMilkOperatingMode.AutoRefreshByTime)
                return true;

            bool needNewQr = false;
            string reason = "";

            // Case 1: Đã bị dừng do không đổi được QR tự động
            if (_needsQrChange)
            {
                needNewQr = true;
                reason = "Job đã bị dừng do không có QR mới";
            }
            // Case 2: QR từ ngày cũ (day-boundary) — stop ngày 1, start ngày 2+
            else if (!string.IsNullOrEmpty(_currentBatchQrCode) &&
                     _currentBatchDate != DateTime.MinValue &&
                     _currentBatchDate.Date < DateTime.Today)
            {
                needNewQr = true;
                reason = $"QR đang dùng từ {_currentBatchDate:dd/MM/yyyy}, cần QR cho ngày {DateTime.Today:dd/MM/yyyy}";
            }

            if (!needNewQr) return true;

            // Thử lấy QR mới từ DB — filter theo ngày hôm nay
            var (nextId, nextQr) = await GetNextQrFromDatabaseAsync();
            string oldQr = _currentBatchQrCode;

            if (nextId >= 0 && !string.IsNullOrEmpty(nextQr) && nextQr != oldQr)
            {
                _needsQrChange = false;
                _noQrWarningShown = false;
                lock (_batchQrLock)
                {
                    _currentBatchQrCode = nextQr;
                    _currentQrId = nextId;
                    //_currentBatchDate = DateTime.Today.AddDays(1);
                    _currentBatchDate = DateTime.Today;
                    if (_JobModel != null)
                    {
                        _JobModel.CurrentBatchDate = _currentBatchDate;
                        _JobModel.CurrentBatchQrCode = _currentBatchQrCode;
                    }
                }

                var ids = new List<int> { nextId };
                var qrCodes = new List<string> { nextQr };
                await MarkQrAsUsedAsync(ids, qrCodes);

                //Nhan.To_260920: Bo set _lastDeltaResetDate o CheckQrOnStart
                //Nguyen nhan: CheckQrOnStart (rebuild khi start job cu ngay moi) va HandleMode1DeltaReset (auto doi luc 23:59)
                //deu dung chung _lastDeltaResetDate -> CheckQrOnStart set guard -> delta reset luc 23:59 bi block -> khong fetch QR cho ngay moi
                //_lastDeltaResetDate = DateTime.Now;

                ProjectLogger.WriteInfo(
                    $"[CheckQrOnStart] {reason} → QR mới: '{oldQr}' → '{nextQr}' | id={nextId}");
                _rebuildTcs = new TaskCompletionSource<bool>();
                Mode1QrCodeChanged?.Invoke(this, (nextQr, _currentBatchDate));
                await _rebuildTcs.Task;
                _ = CheckQrStockAndAlertAsync(bypassCooldown: true);
                return true;
            }

            // Không tìm thấy QR mới → chặn Start
            ProjectLogger.WriteWarning(
                $"[CheckQrOnStart] {reason} nhưng không tìm thấy QR mới → chặn start");
            _FormMainPC?.BeginInvoke(new Action(() =>
            {
                CustomMessageBox.ShowCenterScreen(
                    $"Job cần QR mới nhưng chưa có QR.\n{reason}\nVui lòng chờ QrBank đẩy dữ liệu.",
                    "QR chưa sẵn sàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }));
            return false;
        }

        private async Task<string> GetSampleQrFromDatabaseAsync(string gtin = "")
        {
            return await Task.Run(() =>
            {
                try
                {
                    string lineId = Shared.Settings.LineId ?? string.Empty;
                    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                    if (string.IsNullOrWhiteSpace(connStr))
                        connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                            "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                            Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword, Shared.Settings.THLocalDbDatabase);
                    if (string.IsNullOrWhiteSpace(connStr)) return string.Empty;

                    string table = THDb.Code;
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        RLinkLogService.EnsurePgQrBankTable(conn);
                        string sql = $"SELECT {THDb.QrCode} FROM \"{table}\" WHERE {THDb.IsUsed} = FALSE";
                        if (!string.IsNullOrWhiteSpace(lineId))
                            sql += $" AND {THDb.LineId} = @line_id";
                        if (!string.IsNullOrWhiteSpace(gtin))
                            sql += $" AND {ProductGtin} = @gtin";
                        sql += $" ORDER BY {THDb.Id} ASC LIMIT 1";
                        using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                        {
                            if (!string.IsNullOrWhiteSpace(lineId))
                                cmd.Parameters.AddWithValue("line_id", lineId);
                            if (!string.IsNullOrWhiteSpace(gtin))
                                cmd.Parameters.AddWithValue("gtin", gtin);
                            var result = cmd.ExecuteScalar();
                            return result?.ToString() ?? string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[ValidateQrCode] Lỗi lấy QR sample: " + ex.Message);
                    return string.Empty;
                }
            });
        }

        private async Task<string> GetSampleQrByGtinAsync(string gtin)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string lineId = Shared.Settings.LineId ?? string.Empty;
                    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                    if (string.IsNullOrWhiteSpace(connStr))
                        connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                            "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                            Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                            Shared.Settings.THLocalDbDatabase);
                    if (string.IsNullOrWhiteSpace(connStr)) return "";

                    string table = THDb.Code;
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        string sql = $"SELECT {THDb.QrCode} FROM \"{table}\" WHERE {THDb.IsUsed} = FALSE AND {THDb.ProductGtin} = @gtin";
                        if (!string.IsNullOrWhiteSpace(lineId))
                            sql += $" AND {THDb.LineId} = @line_id";
                        sql += $" ORDER BY {THDb.Id} ASC LIMIT 1";

                        using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("gtin", gtin);
                            if (!string.IsNullOrWhiteSpace(lineId))
                                cmd.Parameters.AddWithValue("line_id", lineId);
                            var result = cmd.ExecuteScalar();
                            return result?.ToString() ?? "";
                        }
                    }
                }
                catch { return ""; }
            });
        }

        private static System.Drawing.Image GenerateQrImageUsingZXing(string content, int size = 250)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            try
            {
                var writer = new ZXing.BarcodeWriter
                {
                    Format = ZXing.BarcodeFormat.QR_CODE,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = size,
                        Width = size,
                        Margin = 2,
                        PureBarcode = true
                    }
                };
                using (var bitmap = writer.Write(content))
                {
                    return new System.Drawing.Bitmap(bitmap);
                }
            }
            catch { return null; }
        }

        private bool _isMode2Resetting = false;

        private async void HandleMode2MidnightReset()
        {
            if (_isMode2Resetting) return;
            _isMode2Resetting = true;
            try
            {
                string oldQr = string.Empty;
                bool shouldFetch = false;
                bool isColdStart = false;

                lock (_batchQrLock)
                {
                    // Khôi phục countdown từ job file nếu restart
                    if (_mode2RemainingSeconds <= 0 && _JobModel?.Mode2NextRefreshAt > DateTime.Now)
                    {
                        _mode2RemainingSeconds = (int)(_JobModel.Mode2NextRefreshAt - DateTime.Now).TotalSeconds;
                        _mode2NextRefreshAt = _JobModel.Mode2NextRefreshAt;
                        ProjectLogger.WriteInfo($"[Mode2] Restore countdown — còn {_mode2RemainingSeconds}s, hết hạn lúc {_mode2NextRefreshAt:HH:mm:ss}");
                    }

                    // ── Cold start: chưa có QR → fetch ngay ──────────────────
                    if (string.IsNullOrEmpty(_currentBatchQrCode))
                    {
                        isColdStart = true;
                        shouldFetch = true;
                        _noQrWarningShown = false;
                        ProjectLogger.WriteInfo("[Mode2] Cold start — chưa có QR, fetch ngay từ DB.");
                    }
                    else
                    {
#if DEBUG
                        // Debug: dùng textbox time thay vì timer N phút
                        if (_lastMode2DebugTime != DateTime.MinValue
                            && (DateTime.Now - _lastMode2DebugTime).TotalSeconds < 30)
                        {
                            ProjectLogger.WriteDebug("[Mode2] DEBUG — vừa mới reset, bỏ qua để tránh đổi QR liên tục");
                            return;
                        }

                        string timeStr = _txtDebugResetTime?.Text?.Trim() ?? "";
                        if (TimeSpan.TryParseExact(timeStr, "hh\\:mm",
                            System.Globalization.CultureInfo.InvariantCulture, out TimeSpan debugTs))
                        {
                            if (DateTime.Now < DateTime.Today.Add(debugTs)) return;
                        }
                        // Nếu textbox rỗng hoặc invalid → trigger ngay
                        oldQr = _currentBatchQrCode;
                        shouldFetch = true;
                        ProjectLogger.WriteInfo($"[Mode2] DEBUG reset, QR cũ '{oldQr}'");
#else
                        // Đếm ngược đã hết → fetch QR tiếp theo
                        oldQr = _currentBatchQrCode;
                        shouldFetch = true;
                        ProjectLogger.WriteInfo($"[Mode2] Hết thời gian, QR cũ '{oldQr}'");
#endif
                    }
                }

                if (!shouldFetch) return;

                var (nextId, nextQr) = await GetNextQrFromDatabaseAsync();

                if (nextId >= 0 && !string.IsNullOrEmpty(nextQr) && nextQr != oldQr)
                {
                    _noQrWarningShown = false;
                    lock (_batchQrLock)
                    {
                        _currentBatchQrCode = nextQr;
                        _currentQrId = nextId;
                        _currentBatchDate = DateTime.Now;
                        if (_JobModel != null)
                        {
                            _JobModel.CurrentBatchDate = _currentBatchDate;
                            _JobModel.CurrentBatchQrCode = _currentBatchQrCode;
                        }
                        if (!isColdStart) _qrConsumedCount++;
                    }

                    await MarkQrAsUsedAsync(new List<int> { nextId }, new List<string> { nextQr });

                    ProjectLogger.WriteInfo(
                        $"[Mode2] {(isColdStart ? "Cold start" : "Reset")} → '{oldQr}' → '{nextQr}' | id={nextId}");
                    Mode1QrCodeChanged?.Invoke(this, (nextQr, _currentBatchDate));
                    _ = CheckQrStockAndAlertAsync(bypassCooldown: true);

                    ResetMode2Countdown();
                }
                else
                {
                    ProjectLogger.WriteWarning(
                        $"[Mode2] Không tìm thấy QR hợp lệ — {(isColdStart ? "chờ QrBank đẩy dữ liệu" : $"giữ QR cũ '{oldQr}'")}");

                    // ── Hiển thị thông báo + dừng in khi hết buffer ──
                    if (!isColdStart && !_noQrWarningShown)
                    {
                        _noQrWarningShown = true;
                        Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                        _FormMainPC?.BeginInvoke(new Action(() =>
                        {
                            CustomMessageBox.ShowCenterScreen(
                                "Không tìm thấy QR hợp lệ cho job.\nDừng in để chờ QrBank đẩy dữ liệu mới.",
                                "QR không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                    }
                }

                UpdateMode1UI();
            }
            finally
            {
                _isMode2Resetting = false;
            }
        }

        private void ResetMode2Countdown()
        {
            int n = (_JobModel?.THJobNMinutes > 0)
                ? _JobModel.THJobNMinutes
                : (Shared.Settings.THNMinutes > 0 ? Shared.Settings.THNMinutes : 30);
            _mode2RemainingSeconds = n * 60;
            _mode2NextRefreshAt = DateTime.Now.AddSeconds(n * 60);
            if (_JobModel != null) _JobModel.Mode2NextRefreshAt = _mode2NextRefreshAt;
            ProjectLogger.WriteInfo($"[Mode2] Countdown reset — {n} phút, hết hạn lúc {_mode2NextRefreshAt:HH:mm:ss}");
        }

        #region UI_Control_Event
        private async void ActionResult(object sender, EventArgs e)
        {
            if (_IsBinding)
            {
                return;
            }
            if (sender == tabControl1)
            {
                int selectedIndex = tabControl1.SelectedIndex;
                Shared.JobNameSelected = "";
                txtFileName.Text = "";
                materialTable.Rows.Clear();
                ClearValues.ClearTextBoxes(reservation, companyCode, createdDate);

                // ── Luôn hiển thị product list khi switch tab ─────────────────────
                LoadProductListToGrid();
                // ── Auto reload khi chuyển sang tab Log đồng bộ ──────────
                if (tabControl1.SelectedTab == tabLogSync)
                    LoadSyncLogToGrid();
                if (selectedIndex == 1)
                    PrinterSupport(radRSeries.Checked, false);
                LoadJobNameList();
                if (selectedIndex == 2)
                    DisplayHistory(Shared.GetJobNameList());

                // ── Refresh config khi switch tab ──────────
                _ = AutoLoadSettingsAsync();

                _ = LoadQrConfigAsync();


            }
            else if (sender == ErrorsLogger)
            {
                //ProjectLogger.WriteError("Error occurred in btnError_Click");
                ProjectLogger.OpenErrorFile();
            }
            else if (sender == cbbHisFilterType)
            {
                switch (cbbHisFilterType.SelectedIndex)
                {
                    case 0: // All
                        DisplayHistory(Shared.GetJobNameList(), HistoryFilter.All);
                        break;
                    case 1: // Newly Created
                        DisplayHistory(Shared.GetJobNameList(), HistoryFilter.Finished);
                        break;
                    case 2: // Finished
                        DisplayHistory(Shared.GetJobNameList(), HistoryFilter.NotFinished);
                        break;
                    default:
                        DisplayHistory(Shared.GetJobNameList());
                        break;
                }
            }
            else if (sender == editJobBtn)
            {
                int lineIndex = dgvHistoryJob.SelectedRows[0].Index;
                string JobName = dgvHistoryJob.Rows[lineIndex].Cells[1].Value.ToString();
                JobModel CurrentJob = Shared.GetJob(JobName);
                //if (CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes)
                //{
                //    CustomMessageBox.Show("Công vi?c đ? hoàn thành, không th? ch?nh s?a!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                //}
                var formEdit = new frmEditJob(CurrentJob);
                formEdit.ShowDialog();
                List<string> JobNameList = GetJobNameList();
                DisplayHistory(JobNameList);
            }
            else if (sender == SyncDataBtn)
            {
                try
                {
                    int lineIndex = dgvHistoryJob.SelectedRows[0].Index;
                    string JobName = dgvHistoryJob.Rows[lineIndex].Cells[1].Value.ToString();

                    CurrentJob = GetJob(JobName);

                    string checkedDataPath = CommVariables.PathCheckedResult + CurrentJob.CheckedResultPath;
                    string sentCheckedDataPath = CommVariables.PathSentDataChecked + CurrentJob.CheckedResultPath;

                    NumberChecked = FileFuncs.ReadCodeData(checkedDataPath).Count() - 1;
                    NumberOfCheckSentSuccess = FileFuncs.ReadCodeData(sentCheckedDataPath).Count(item => item.Length > 8 && item[8].Equals("Sent", StringComparison.OrdinalIgnoreCase));


                    if ((CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes || CurrentJob.NumberOfPrintedCodes == 0) && NumberChecked == NumberOfCheckSentSuccess)
                    {
                        UIControlsFuncs.EnableControls(tabControl1, dgvHistoryJob, pnlMenu);
                        UIControlsFuncs.HideControls(syncDataPanel);
                        return;
                    }

                    var listQrcodes = FileFuncs.ReadStringListFromCsv(CurrentJob.DirectoryDatabase);

                    PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder); //Problem here , have to seperate PO and Reservation

                    bool isSent = CurrentJob.isPushedDatabase = isPushDatabase = await SendGeneratedCodes(listQrcodes, CurrentJob);
                    CurrentJob.SaveFile();
                    if (!isSent)
                    {
                        return;
                    }
                    UIControlsFuncs.ShowControls(syncDataPanel);
                    UIControlsFuncs.DisableAllTabsSelection(tabControl1);
                    UIControlsFuncs.DisableControls(dgvHistoryJob, pnlMenu, SyncDataBtn, cbbHisFilterType);

                    //string dataPath = CurrentJob.DirectoryDatabase;

                    //string sentDataPath = CommVariables.PathSentDataPrinted + CurrentJob.PrintedResponePath;
                    //string url = ManufacturingApis.postPrintedDataUrl();
                    //if (_printedDataProcess != null) _printedDataProcess.Stop();
                    //_printedDataProcess = THTrueMilkProcessorFactory.CreatePrintingProcessor(sentDataPath, url, dataPath);
                    //_printedDataProcess.Start();

                    //string urlChecked = ManufacturingApis.postCheckedDataUrl();
                    //if (_verificationDataProcess != null) _verificationDataProcess.Stop();
                    //_verificationDataProcess = THTrueMilkProcessorFactory.CreateVerificationProcessor(sentCheckedDataPath, urlChecked, dataPath);
                    //_verificationDataProcess.Start();

                }
                catch (Exception ex)
                {
                }


            }
            else if (sender == StopSyncData)
            {
                OnSentPrintedCodesCompleted();
            }
            else if (sender == btnGetInfo)
            {
            
                ClearProductInfo();
                btnGetInfo.Enabled = false;
                btnRefreshListProgramFroCameraKeyence.Enabled = false;
                string origText = btnGetInfo._Text;
                btnGetInfo._Text = "Đang tải...";
                picLoading.Visible = true;
                try
                {
                    var results = new List<(string name, bool ok, string error)>();

                    await Task.WhenAll(
                        RunSafe("Tài khoản", AutoLoadAccountsAsync(), results),
                        RunSafe("Cấu hình", AutoLoadSettingsAsync(), results),
                        RunSafe("Sản phẩm", AutoLoadProductsAsync(forceRefreshImages: true), results),
                        RunSafe("Cấu hình QR", LoadQrConfigAsync(), results),
                        RunSafe("Camera Programs", RefreshCameraProgramsAsync(), results),
                        RunSafe("Printer Templates", ObtainPrintProductTemplateListAsync(), results)
                    );

                    txtBatchNumber.Text = "";
                    txtTotalQR.Text = "";

                    var okList = results.Where(r => r.ok).Select(r =>
                        r.name == "Camera Programs" ? $"{r.name} ({cbcSelectedModel.Items.Count} programs)"
                        : r.name == "Printer Templates" ? $"{r.name} ({listBoxPrintProductTemplate.Items.Count} templates)"
                        : r.name).ToList();
                    var failList = results.Where(r => !r.ok).Select(r => $"{r.name} ({r.error})").ToList();

                    string msg = "";
                    if (okList.Count > 0) msg += $"  Thành công:\n    - {string.Join("\n    - ", okList)}\n";
                    if (failList.Count > 0) msg += $"  Thất bại:\n    - {string.Join("\n    - ", failList)}\n";

                    var icon = failList.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
                    CustomMessageBox.Show(msg.TrimEnd(), "Kết quả cập nhật", MessageBoxButtons.OK, icon);
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[btnGetInfo] Lỗi lấy thông tin từ R-Link Master: " + ex.Message, ex);
                    CustomMessageBox.Show($"Lấy thông tin từ R-Link Master thất bại!\n{ex.Message}",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    picLoading.Visible = false;
                    btnGetInfo.Enabled = true;
                    btnRefreshListProgramFroCameraKeyence.Enabled = true;
                    btnGetInfo._Text = origText;
                }
            }
            else if (sender == GetMaterialDoc)
            {
                try
                {
                    if (material_doc.Text == "")
                    {
                        CustomMessageBox.Show("Vui lòng nhập Material Doc!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    materialTable.Rows.Clear();
                    ClearValues.ClearTextBoxes(reservation, companyCode, createdDate);

                    ResponseReservation payload = await ManufacturingService.GetReservationAsync(Settings.FactoryCode, material_doc.Text, Settings.RLinkName);
                    Reservation = payload;
                    Settings.LOTFormatDate = payload.batch_date_format;
                    companyCode.Text = payload.sales_org;
                    reservation.Text = payload.reservation;
                    createdDate.Text = payload.create_date.ToString();

                    foreach (var data in payload.items)
                    {
                        materialTable.Rows.Add(data.material_number, data.material_name, data.batch,
                            data.qty, data.qty_per_carton, data.qty / data.qty_per_carton, data.printed_count);
                    }
                }
                catch (Exception ex)
                {

                }

            }
            else if (sender == saveJobTH)
            {
                try
                {
                
                    Shared.PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder);
                    // ── Kiểm tra template ────────────────────────────────────────
                    if (!CheckExistTemplatePrint(_matchedPrinterTemplate))
                    {
                        CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Khi tạo job mới, dùng global settings (đã get từ RLink Master)
                    // không dùng _JobModel.THJobOperatingMode (có thể là mode cũ)
                    var mode = Shared.Settings.THOperatingMode;

                    if (mode == THTrueMilkOperatingMode.BatchOneQrCode ||
                        mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange ||
                        mode == THTrueMilkOperatingMode.AutoRefreshByTime ||
                        mode == THTrueMilkOperatingMode.ProductOneQrCode)
                    {
                        // Mode 1, 4, 2, 3: tạo CSV từ kho QrBank
                        await SaveJobForMode1Or2Async(mode);
                    }
                }
                catch (Exception ex)
                {
                    DisplayJobLoading(false);
                    CuzMessageBox.Show("Không thể tạo job!", Lang.Error,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ProjectLogger.WriteError("[saveJobTH] " + ex.Message);
                }
            }
            else if (sender == savePOOffline)
            {
                try
                {
                    PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder);

                    int numberOfCodes;

                    if (!EmptyValueValidator.CheckRequiredFields(InputPO, MaterialNumber, InputCodeNumber)) return;

                    if (!int.TryParse(InputCodeNumber.Text, out numberOfCodes))
                    {
                        CustomMessageBox.Show("Vui lòng nhập số lượng hợp lệ!", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!CheckExistTemplatePrint(_matchedPrinterTemplate))
                    {
                        CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DisplayJobLoading(true);
                    _JobModel.IsProcessOrderMode = true;
                    GenerateCodesOffline(_JobModel);
                    if (Shared.databasePath != "")
                    {
                        txtDirectoryDatabse.Text = _JobModel.DirectoryDatabase = Shared.databasePath;
                    }

                    _PODFormat.Clear();
                    txtPODFormat.Text = "";
                    Shared.databasePath = "";
                    //Shared.PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.PrintingModeOffline);
                    DisplayJobLoading(false);

                    SaveJob();
                }
                catch (Exception ex)
                {
                    DisplayJobLoading(false);
                    ProjectLogger.WriteError("Error occurred in get saveJobTHTrueMilkOffline Function" + ex.Message);

                }
            }
            else if (sender == saveJobReservation)
            {
                try
                {
                    PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.Reservation);
                    if (!CheckExistTemplatePrint(_matchedPrinterTemplate))
                    {
                        CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    await GenerateReservationCodes();
                    if (Shared.databasePath != "")
                    {
                        txtDirectoryDatabse.Text = _JobModel.DirectoryDatabase = Shared.databasePath;
                    }
                    else
                    {
                        return;
                    }

                    _PODFormat.Clear();
                    txtPODFormat.Text = "";
                    Shared.databasePath = "";
                    DisplayJobLoading(false);
                    SaveJob();
                    dgvItems.Rows.Clear();
                }
                catch (Exception ex)
                {
                    DisplayJobLoading(false); ;
                    CustomMessageBox.Show($"Không thể tạo phiếu soạn hàng!", Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ProjectLogger.WriteError("Error occurred in get saveJob Function" + ex.Message);

                }
            }
            else if (sender == CreateRESOffline)
            {
                try
                {
                    Shared.PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.Reservation);

                    int numberOfCodes;

                    if (!EmptyValueValidator.CheckRequiredFields(RES_Material_doc, RES_MaterialNumber, RES_LotNumber, RES_NumberCode)) return;

                    if (!int.TryParse(RES_NumberCode.Text, out numberOfCodes))
                    {
                        CustomMessageBox.Show("Vui lòng nhập số lượng hợp lệ!", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DisplayJobLoading(true);
                    _JobModel.IsReservationMode = true;
                    GenerateCodesOffline(_JobModel);
                    if (Shared.databasePath != "")
                    {
                        txtDirectoryDatabse.Text = _JobModel.DirectoryDatabase = Shared.databasePath;
                    }

                    _PODFormat.Clear();
                    txtPODFormat.Text = "";
                    Shared.databasePath = "";
                    DisplayJobLoading(false);

                    SaveJob();
                }
                catch (Exception ex)
                {
                    DisplayJobLoading(false);
                    ProjectLogger.WriteError("Error occurred in get saveJobTHTrueMilkOffline Function" + ex.Message);

                }
            }
            else if (sender == radAfterProduction)
            {
                if (_JobModel != null)
                {
                    if (radAfterProduction.Checked && _JobModel.PrinterSeries)
                    {
                        _JobModel.JobType = JobType.AfterProduction;
                    }
                }
            }
            else if (sender == radOnProduction)
            {
                if (_JobModel != null)
                {
                    if (radOnProduction.Checked && _JobModel.PrinterSeries)
                    {
                        _JobModel.JobType = JobType.OnProduction;
                    }
                }
            }
            else if (sender == radVerifyAndPrint)
            {
                if (_JobModel != null)
                {
                    if (radVerifyAndPrint.Checked && _JobModel.PrinterSeries)
                    {
                        _JobModel.JobType = JobType.VerifyAndPrint;
                    }
                }
            }
            else if (sender == radRSeries)
            {
                if (_JobModel != null)
                {
                    if (radRSeries.Checked)
                    {
                        _JobModel.PrinterSeries = true;
                        PrinterSupport(true);
                    }
                }
            }
            else if (sender == radOther)
            {
                if (_JobModel != null)
                {
                    if (radOther.Checked)
                    {
                        _JobModel.PrinterSeries = false;
                        PrinterSupport(false);
                    }
                }
            }
            else if (sender == FirstRowHeader)
            {
                if (_JobModel != null)
                {
                    _JobModel.IsFirstRowHeader = FirstRowHeader.Checked;
                }
            }
            else if (sender == radCanRead)
            {
                if (_JobModel != null)
                {
                    if (radCanRead.Checked)
                    {
                        _JobModel.CompareType = CompareType.CanRead;
                    }
                    EnableForCompareType(CompareType.CanRead);
                }
            }
            else if (sender == radStaticText)
            {
                if (_JobModel != null)
                {
                    if (radStaticText.Checked)
                    {
                        _JobModel.CompareType = CompareType.StaticText;
                    }
                    EnableForCompareType(CompareType.StaticText);
                }

            }
            else if (sender == radDatabase)
            {
                if (_JobModel != null)
                {
                    if (radDatabase.Checked)
                    {
                        _JobModel.CompareType = CompareType.Database;
                    }
                    EnableForCompareType(CompareType.Database);
                    if (radAfterProduction.Checked)
                    {
                        _JobModel.JobType = JobType.AfterProduction;
                    }
                    else if (radOnProduction.Checked)
                    {
                        _JobModel.JobType = JobType.OnProduction;
                    }
                    else
                    {
                        _JobModel.JobType = JobType.VerifyAndPrint;
                    }
                }
            }
            else if (sender == btnPODFormat)
            {
                if (txtDirectoryDatabse.Text == "" || txtDirectoryDatabse.Text == null)
                {
                    CuzMessageBox.Show(Lang.PleaseSelectTheDatabaseFileFirst, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using (var frmPODFormat = new FrmPODFormat())  // Create and show dialog POD format form base on default POD format list
                {
                    FrmPODFormat._DirectoryDatabase = txtDirectoryDatabse.Text;
                    FrmPODFormat.IsFirstRowHeader = FirstRowHeader.Checked;

                    txtPODFormat.Text = "";
                    frmPODFormat.ShowDialog();
                    if (frmPODFormat.DialogResult == DialogResult.OK)
                    {
                        _PODFormat = FrmPODFormat._PODFormat; // Get POD format from POD format form
                        if (_PODFormat.Count > 0)
                        {
                            foreach (PODModel item in _PODFormat)
                            {
                                txtPODFormat.Text += item.ToStringSample();
                            }
                        }
                        _NumberTotalsCode = frmPODFormat._NumberTotalsCode;

                    }
                }
            }
            else if (sender == listBoxPrintProductTemplate)
            {
                if (_JobModel != null && radDatabase.Checked)
                {
                    _JobModel.TemplatePrint = _matchedPrinterTemplate;
                }
            }
            else if (sender == listBoxJobList)
            {
                OpenJob();
            }
            else if (sender == btnSettings)
            {
                if (_FormSettings == null || _FormSettings.IsDisposed)
                {
                    _FormSettings = new FrmSettingsTHTrueMilk();
                    _FormSettings.Show();
                }
                else
                {
                    if (_FormSettings.WindowState == FormWindowState.Minimized)
                    {
                        _FormSettings.WindowState = FormWindowState.Normal;
                    }

                    _FormSettings.Focus();
                    _FormSettings.BringToFront();
                }
            }
            else if (sender == btnRefesh)
            {
                dtpFromDate.Value = DateTime.Now.AddDays(-7);
                dtpToDate.Value = DateTime.Now;
                RefreshJobList();
            }
            else if (sender == btnGennerate)
            {
                AutoGenerateFileName();
            }
            else if (sender == btnImportDatabase)
            {

                if (Shared.databasePath != "")
                {
                    txtDirectoryDatabse.Text = _JobModel.DirectoryDatabase = Shared.databasePath;
                }

                _PODFormat.Clear();
                txtPODFormat.Text = "";
                Shared.databasePath = "";
            }
            else if (sender == btnNext)
            {
               //// Mode 1: không cần chọn job — tự động tạo job từ batch QR hiện tại
               // if (Shared.Settings.THOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode)
               // {
               //     StartMode1Job();
               //     return;
               // }
     

                Shared.RaiseOnNextButtonEvent();
                try
                {
                    if (Shared.JobNameSelected == "")
                    {
                        JobModel jobModel = Shared.GetJob(txtFileName.Text + Shared.Settings.JobFileExtension);
                        if (jobModel == null && txtFileName.Text != "")
                        {
                            CuzMessageBox.Show(Lang.PleaseSaveTheWorkYouJustEntered, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        CuzMessageBox.Show(Lang.PleaseChooseAJobOrCreateANewOne, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (_JobModel != null && _JobModel.CompareType == CompareType.Database && !CheckExistTemplatePrint(_JobModel.TemplatePrint) && _JobModel.PrinterSeries)
                        {
                            CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        if (Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings && _JobModel.CompareType == CompareType.Database && _JobModel.PrinterSeries)
                        {
                            PrinterSettingsModel printerSettingsModel = Shared.GetSettingsPrinter();
                            if (printerSettingsModel.IsSupportHttpRequest)
                            {
                                if (printerSettingsModel.PodDataType != 1)
                                {
                                    CuzMessageBox.Show(Lang.DataTypeMustBeRAWData, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                            }
                            else
                            {
                                CuzMessageBox.Show(Lang.PrinterNotSupportHttpRequest, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }

                        Hide();
                        // Force close form cũ + cleanup hoàn toàn (kể cả đang Running/Processing)
                        if (_FormMainPC != null && !_FormMainPC.IsDisposed)
                            _FormMainPC.ForceClose();
                        _FormMainPC?.Dispose();
                        // singleton keeps running — frmMain will SetSnapshotFactory
                        // Luôn tạo form mới
                        _FormMainPC = new frmMainTHTrueMilk(this);
#if DEBUG
                        _FormMainPC.DebugNsxHsdTime = _debugNsxHsdTime;
#endif
                        _FormMainPC.Show();
                        _FormMainPC.UpdateJobInfoDisplay(_JobModel);
                    }
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("Error occurred in get btnNext Function" + ex.Message);
                }
            }
            else if (sender == btnSave)
            {
                if (_JobModel != null)
                {
                    _JobModel.TemplatePrint = _matchedPrinterTemplate;
                    _JobModel.NumberTotalsCode = _NumberTotalsCode;
                    _JobModel.JobStatus = JobStatus.NewlyCreated;
                }

                // Đổi camera TRƯỚC, nếu fail → không cho tạo job
                bool camOk = await TryChangeKeyenceProgramAsync();
                if (!camOk) return;

                SaveJob();
            }
            else if (sender == btnAbout)
            {
                var about = new FrmAbout();
                about.ShowDialog();
            }
            else if (sender == btnHelp)
            {
                //string pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "R_LINK_USER_MANUEL.pdf");
                //Process.Start(new ProcessStartInfo
                //{
                //    FileName = pdfPath,
                //    UseShellExecute = true // Important for opening in default PDF viewer
                //});
                CustomMessageBox.Show(Lang.FunctionIsComingSoon, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (sender == btnRestart)
            {
                // Get the current process
                RestartApplication();
            }
            else if (sender == btnDelete)
            {
                DeleteJob();
            }
            else if (sender == btnRefeshTemplate)
            {
                _PrintProductTemplateList = new string[] { };
                ObtainPrintProductTemplateList();
                UpdateUIListBoxPrintProductTemplateList(_PrintProductTemplateList);
            }
        }
        private async Task<bool> TryChangeKeyenceProgramAsync()
        {
            var cam = Shared.Settings.CameraList.FirstOrDefault();
            if (cam == null) return true;

            bool isKeyence = cam.CameraBrand == CameraBrand.Keyence
                             || cam.CameraType == CameraType.VS_C
                             || cam.CameraType == CameraType.CV_X;
            if (!isKeyence) return true; // Không phải Keyence → bỏ qua

            if (Shared.vscCamera == null || !Shared.vscCamera.IsConnected())
            {
                CuzMessageBox.Show("Camera Keyence chưa kết nối.", "Warning");
                return false;
            }

            // Dùng _matchedCameraProgram từ lblProgramCamera
            string camModel = _matchedCameraProgram ?? "";
            int progNo = -1;
            if (camModel.Length >= 4 && int.TryParse(camModel.Substring(0, 4), out int parsed) && parsed >= 0)
            {
                progNo = parsed;
            }
            else
            {
                progNo = cam.KeyenceCurrentProgramNo;
            }
            if (progNo < 0) return true; // Không có program → bỏ qua

            DisplayJobLoading(true);
            try
            {
                var changeTask = Shared.vscCamera.ChangeProgramAsync(progNo);
                var timeoutTask = Task.Delay(15000);
                var completed = await Task.WhenAny(changeTask, timeoutTask);

                if (completed == timeoutTask)
                {
                    DisplayJobLoading(false);
                    CuzMessageBox.Show("Camera không phản hồi sau 15 giây. Vui lòng kiểm tra kết nối camera và thử lại.", "Camera Timeout");
                    return false;
                }

                var (success, message) = await changeTask;
                if (success)
                {
                    cam.KeyenceCurrentProgramNo = progNo;
                    Shared.SaveSettings();
                    return true;
                }
                else
                {
                    DisplayJobLoading(false);
                    CuzMessageBox.Show(message, "Lỗi camera");
                    return false;
                }
            }
            catch (Exception ex)
            {
                DisplayJobLoading(false);
                CuzMessageBox.Show($"Lỗi đổi program camera: {ex.Message}", "Error");
                return false;
            }
            finally
            {
                DisplayJobLoading(false);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // Tab History Receive QR — Lịch sử nhận QR
        // ════════════════════════════════════════════════════════════════

        private void InitDgvHistoryReceiveQr()
        {
            dgvHistoryQr.Columns.Clear();
            dgvHistoryQr.AutoGenerateColumns = false;
            dgvHistoryQr.ScrollBars = ScrollBars.Vertical;
            dgvHistoryQr.ReadOnly = true;
            dgvHistoryQr.AllowUserToAddRows = false;
            dgvHistoryQr.AllowUserToDeleteRows = false;
            dgvHistoryQr.MultiSelect = false;
            dgvHistoryQr.RowTemplate.Height = 35;

            dgvHistoryQr.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHJobName", HeaderText = "Công việc cấp phát", DataPropertyName = JobName, Width = 150 });
            dgvHistoryQr.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHSender", HeaderText = "Tài khoản", DataPropertyName = Sender, Width = 120 });
            dgvHistoryQr.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHReceiveTime", HeaderText = "Thời gian nhận", DataPropertyName = ReceivedAt, Width = 180 });
            dgvHistoryQr.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHTHDb.ColTotalCodes", HeaderText = "Số lượng", DataPropertyName = THDb.ColTotalCodes, Width = 90 });

            dgvHistoryQr.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHFirstQr", HeaderText = "QR đầu tiên", DataPropertyName = FirstQr, Width = 240 });
            dgvHistoryQr.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHLastQr", HeaderText = "QR cuối", DataPropertyName = LastQr, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvHistoryQr.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHView", HeaderText = "Chi tiết", Width = 90, SortMode = DataGridViewColumnSortMode.NotSortable });

            dgvHistoryQr.CellFormatting += (s, e) =>
            {
                if (dgvHistoryQr.Columns[e.ColumnIndex].Name == "colHJobName" && e.Value is string hj)
                {
                    e.Value = hj.Replace(' ', '_');
                    e.FormattingApplied = true;
                }
            };

            // ── label43 + label44: tổng QR / còn lại ──
            if (label43 == null)
            {
                label43 = new Label
                {
                    Name = "label43",
                    Text = "Tổng QR: ...",
                    AutoSize = true,
                    Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(64, 64, 64),
                    Location = new Point(12, 12)
                };
                tabHistoryReciveQr.Controls.Add(label43);
            }
            if (label44 == null)
            {
                label44 = new Label
                {
                    Name = "label44",
                    Text = "Còn lại: ...",
                    AutoSize = true,
                    Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 140, 70),
                    Location = new Point(250, 12)
                };
                tabHistoryReciveQr.Controls.Add(label44);
            }
        }

        private async Task LoadHistoryReceiveQrAsync(string filter = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var rows = await Task.Run(() => FetchHistoryReceiveQrRows(filter, fromDate, toDate));
                ProjectLogger.WriteInfo($"[TabHistoryReceiveQr] Fetched {rows.Count} rows");
                if (InvokeRequired)
                    Invoke(new Action(() => BindHistoryReceiveQrRows(rows)));
                else
                    BindHistoryReceiveQrRows(rows);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[TabHistoryReceiveQr] LoadHistoryReceiveQrAsync lỗi: " + ex.Message, ex);
            }
        }

        private List<Dictionary<string, object>> FetchHistoryReceiveQrRows(string filter, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new List<Dictionary<string, object>>();
            string lineId = Shared.Settings.LineId ?? "";

            ProjectLogger.WriteInfo($"[FetchHistoryReceiveQr] lineId='{lineId}' filter='{filter}'");

            string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
            if (string.IsNullOrWhiteSpace(connStr))
                connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                    "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);

            if (!string.IsNullOrWhiteSpace(connStr))
            {
                try
                {
                    string lineFilter = string.IsNullOrWhiteSpace(lineId)
                        ? ""
                        : $"AND {LineId} = @lineId";
                    string whereFilter = string.IsNullOrWhiteSpace(filter)
                        ? ""
                        : $"AND (COALESCE({Batch},'') ILIKE @kw OR COALESCE({FirstQr},'') ILIKE @kw OR COALESCE({LastQr},'') ILIKE @kw)";

                    if (fromDate.HasValue)
                        whereFilter += $" AND {ReceivedAt} >= @fromDate";
                    if (toDate.HasValue)
                        whereFilter += $" AND {ReceivedAt} <= @toDate";

                    string sql = $@"SELECT {ReceivedAt}, {ColTotalCodes},
                                        COALESCE({Batch},'') AS {Batch},
                                        COALESCE({FirstQr},'') AS {FirstQr},
                                        COALESCE({LastQr},'') AS {LastQr},
                                        COALESCE({JobName},'') AS {JobName},
                                        COALESCE({Sender},'') AS {Sender}
                                   FROM {ReceiveHistory}
                                   WHERE 1=1 {lineFilter} {whereFilter}
                                   ORDER BY {ReceivedAt} DESC
                                   LIMIT 500";

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("lineId", lineId);
                            if (!string.IsNullOrWhiteSpace(filter))
                                cmd.Parameters.AddWithValue("kw", "%" + filter.Trim() + "%");
                            if (fromDate.HasValue)
                                cmd.Parameters.AddWithValue("fromDate", fromDate.Value.Date);
                            if (toDate.HasValue)
                                cmd.Parameters.AddWithValue("toDate", toDate.Value.Date.AddDays(1));

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    result.Add(new Dictionary<string, object>
                                    {
                                        [ReceivedAt] = reader.GetDateTime(0).ToString("yyyy-MM-dd HH:mm:ss"),
                                        [THDb.ColTotalCodes] = reader.GetInt32(1),
                                        [Batch] = reader.GetString(2),
                                        [FirstQr] = reader.GetString(3),
                                        [LastQr] = reader.GetString(4),
                                        [JobName] = reader.GetString(5),
                                        [Sender] = reader.GetString(6)
                                    });
                                }
                            }
                        }
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteWarning("[FetchHistoryReceiveQrRows] PG error, fallback SQLite: " + ex.Message);
                }
            }

            // ── SQLite fallback ──
            try
            {
                string lineFilterSqlite = string.IsNullOrWhiteSpace(lineId)
                    ? ""
                    : $"AND {LineId} = @lineId";

                string sql = $@"SELECT {ReceivedAt}, {ColTotalCodes},
                                    COALESCE({Batch},'') AS {Batch},
                                    COALESCE({FirstQr},'') AS {FirstQr},
                                    COALESCE({LastQr},'') AS {LastQr},
                                    COALESCE({JobName},'') AS {JobName},
                                    COALESCE({Sender},'') AS {Sender}
                               FROM {ReceiveHistory}
                               WHERE 1=1 {lineFilterSqlite}
                               ORDER BY {ReceivedAt} DESC
                               LIMIT 500";

                using (var conn = new System.Data.SQLite.SQLiteConnection(RLinkLogService.ConnStr))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(sql, conn))
                    {
                        if (!string.IsNullOrWhiteSpace(lineId))
                            cmd.Parameters.AddWithValue("@lineId", lineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new Dictionary<string, object>
                                {
                                    [ReceivedAt] = reader.GetString(0),
                                    [THDb.ColTotalCodes] = reader.GetInt32(1),
                                    [Batch] = reader.GetString(2),
                                    [FirstQr] = reader.GetString(3),
                                    [LastQr] = reader.GetString(4),
                                    [JobName] = reader.GetString(5),
                                    [Sender] = reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[FetchHistoryReceiveQrRows] SQLite error: " + ex.Message, ex);
            }

            return result;
        }

        private void BindHistoryReceiveQrRows(List<Dictionary<string, object>> rows)
        {
            var dt = new DataTable();
            dt.Columns.Add(ReceivedAt, typeof(string));
            dt.Columns.Add(THDb.ColTotalCodes, typeof(int));
            dt.Columns.Add(Batch, typeof(string));
            dt.Columns.Add(FirstQr, typeof(string));
            dt.Columns.Add(LastQr, typeof(string));
            dt.Columns.Add(JobName, typeof(string));
            dt.Columns.Add(Sender, typeof(string));
            dt.Columns.Add("_view", typeof(string));

            if (rows != null)
                foreach (var row in rows)
                    dt.Rows.Add(row[ReceivedAt], row[THDb.ColTotalCodes], row[Batch], row[FirstQr], row[LastQr], row[JobName], row[Sender], "Xem");

            dgvHistoryQr.DataSource = dt;

            foreach (DataGridViewRow r in dgvHistoryQr.Rows)
                r.Cells["colHView"].Value = "Xem";

            // ── Cập nhật label43 (tổng QR đã cấp) + label44 (còn lại chưa dùng) ──
            _ = UpdateQrStatsLabelsAsync();
        }

        private async Task UpdateQrStatsLabelsAsync()
        {
            try
            {
                string lineId = Shared.Settings.LineId ?? "";
                string table = THDb.Code;
                string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                if (string.IsNullOrWhiteSpace(connStr))
                    connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                        "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                        Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                        Shared.Settings.THLocalDbDatabase);

                long total = 0, unused = 0;
                bool pgOk = false;

                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    try
                    {
                        using (var conn = new Npgsql.NpgsqlConnection(connStr))
                        {
                            conn.Open();
                            using (var cmd = new Npgsql.NpgsqlCommand(
                                $"SELECT COUNT(*) FROM \"{table}\"", conn))
                            {
                                total = (long)cmd.ExecuteScalar();
                            }
                            string lineFilter = string.IsNullOrWhiteSpace(lineId) ? "" : $"AND {LineId} = @line";
                            using (var cmd = new Npgsql.NpgsqlCommand(
                                $"SELECT COUNT(*) FROM \"{table}\" WHERE {IsUsed} = FALSE {lineFilter}", conn))
                            {
                                if (!string.IsNullOrWhiteSpace(lineId))
                                    cmd.Parameters.AddWithValue("line", lineId);
                                unused = (long)cmd.ExecuteScalar();
                            }
                        }
                        pgOk = true;
                    }
                    catch { }
                }

                if (!pgOk)
                {
                    try
                    {
                        using (var conn = new System.Data.SQLite.SQLiteConnection(RLinkLogService.ConnStr))
                        {
                            conn.Open();
                            using (var cmd = new System.Data.SQLite.SQLiteCommand(
                                $"SELECT COUNT(*) FROM {table}", conn))
                            {
                                total = (long)cmd.ExecuteScalar();
                            }
                            string lineFilter = string.IsNullOrWhiteSpace(lineId) ? "" : $"AND {LineId} = @line";
                            using (var cmd = new System.Data.SQLite.SQLiteCommand(
                                $"SELECT COUNT(*) FROM {table} WHERE {IsUsed} = 0 {lineFilter}", conn))
                            {
                                if (!string.IsNullOrWhiteSpace(lineId))
                                    cmd.Parameters.AddWithValue("@line", lineId);
                                unused = (long)cmd.ExecuteScalar();
                            }
                        }
                    }
                    catch { }
                }

                if (InvokeRequired)
                    Invoke(new Action(() => SetStatsLabels(total, unused)));
                else
                    SetStatsLabels(total, unused);
            }
            catch { }
        }

        private void SetStatsLabels(long total, long unused)
        {
            if (label43 != null) label43.Text = $"Tổng QR: {total:n0}";
            if (label44 != null) label44.Text = $"Còn lại: {unused:n0}";
        }

        private async void dgvHistoryReceiveQr_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            try
            {
                var dt = dgvHistoryQr.DataSource as DataTable;
                if (dt == null) return;
                DataRow dr = dt.Rows[e.RowIndex];

                string receivedAt = dr[ReceivedAt]?.ToString() ?? "";
                string jobName = dr[JobName]?.ToString() ?? "";
#if DEBUG
                Console.WriteLine($"[DEBUG] CellDoubleClick RowIndex={e.RowIndex} jobName='{jobName}'");
#endif

                if (string.IsNullOrWhiteSpace(jobName)) return;

                var detailRows = await Task.Run(() => FetchReciveDetailRows(jobName));
#if DEBUG
                Console.WriteLine($"[DEBUG] Detail rows: {detailRows.Count}");
#endif
                ShowReciveDcetailPopup(detailRows, receivedAt);
            }
            catch (Exception ex)
            {
            }
        }

        private async void dgvHistoryReceiveQr_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvHistoryQr.Columns[e.ColumnIndex].Name != "colHView") return;

            try
            {
                var dt = dgvHistoryQr.DataSource as DataTable;
                if (dt == null) return;
                DataRow dr = dt.Rows[e.RowIndex];

                string receivedAt = dr[ReceivedAt]?.ToString() ?? "";
                string jobName = dr[JobName]?.ToString() ?? "";
#if DEBUG
                Console.WriteLine($"[DEBUG] CellClick RowIndex={e.RowIndex} jobName='{jobName}'");
#endif

                if (string.IsNullOrWhiteSpace(jobName)) return;

                var detailRows = await Task.Run(() => FetchReciveDetailRows(jobName));
                ShowReciveDcetailPopup(detailRows, receivedAt);
            }
            catch (Exception ex)
            {
            }
        }

        private List<string[]> FetchReciveDetailRows(string jobName)
        {
            var result = new List<string[]>();
            string lineId = Shared.Settings.LineId ?? "";
            string table = THDb.Code;

            string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
            if (string.IsNullOrWhiteSpace(connStr))
                connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                    "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);

            if (string.IsNullOrWhiteSpace(connStr) || string.IsNullOrWhiteSpace(jobName))
                return result;

            if (!string.IsNullOrWhiteSpace(connStr))
            {
                try
                {
                    string lineFilter = string.IsNullOrWhiteSpace(lineId) ? "" : $"AND {LineId} = @lineId";

                    string sql = $@"SELECT {Id}::text, {QrCode},
                                        CASE WHEN {IsUsed} THEN 'Đã dùng' ELSE 'Chưa dùng' END,
                                        CASE WHEN {IsPrinted} THEN 'Đã in' ELSE 'Chưa in' END,
                                        COALESCE({Batch},''), COALESCE({LineId},''),
                                        COALESCE({ReceivedAt}::text, '')
                                   FROM ""{table}""
                                   WHERE {JobNameReceiveQr} = @jobName {lineFilter}
                                   ORDER BY {Id} ASC
                                   LIMIT 10000";

#if DEBUG
                    Console.WriteLine($"[FetchReciveDetailRows] PG: jobName='{jobName}'");
#endif

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("jobName", jobName);
                            if (!string.IsNullOrWhiteSpace(lineId))
                                cmd.Parameters.AddWithValue("lineId", lineId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    result.Add(new[] { reader.GetString(0), reader.GetString(1), reader.GetString(2),
                                        reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6) });
                            }
                        }
                    }
#if DEBUG
                    Console.WriteLine($"[FetchReciveDetailRows] PG result: {result.Count} rows");
#endif
                    return result;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"[FetchReciveDetailRows] PG error: {ex.Message}");
#endif
                    ProjectLogger.WriteWarning("[FetchReciveDetailRows] PG error: " + ex.Message);
                }
            }

            // SQLite fallback
            try
            {
                string lineFilterSqlite = string.IsNullOrWhiteSpace(lineId) ? "" : $"AND {LineId} = @lineId";

                string sql = $@"SELECT {Id}, {QrCode},
                                    CASE WHEN {IsUsed} = 1 THEN 'Đã dùng' ELSE 'Chưa dùng' END,
                                    CASE WHEN {IsPrinted} = 1 THEN 'Đã in' ELSE 'Chưa in' END,
                                    COALESCE({Batch},''), COALESCE({LineId},''), COALESCE({ReceivedAt},'')
                               FROM {THDb.Code}
                               WHERE {JobNameReceiveQr} = @jobName {lineFilterSqlite}
                               ORDER BY {Id} ASC LIMIT 10000";

#if DEBUG
                Console.WriteLine($"[FetchReciveDetailRows] SQLite: jobName='{jobName}'");
#endif

                using (var conn = new System.Data.SQLite.SQLiteConnection(RLinkLogService.ConnStr))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@jobName", jobName);
                        if (!string.IsNullOrWhiteSpace(lineId))
                            cmd.Parameters.AddWithValue("@lineId", lineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                result.Add(new[] { reader.GetValue(0).ToString(), reader.GetString(1), reader.GetString(2),
                                    reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6) });
                        }
                    }
                }
#if DEBUG
                Console.WriteLine($"[FetchReciveDetailRows] SQLite result: {result.Count} rows");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"[FetchReciveDetailRows] SQLite error: {ex.Message}");
#endif
                ProjectLogger.WriteError("[FetchReciveDetailRows] SQLite error: " + ex.Message, ex);
            }

            return result;
        }

        private void ShowReciveDcetailPopup(List<string[]> rows, string receivedAt)
        {
            if (rows == null || rows.Count == 0) return;
            using (var frm = new View.OtherProjects.THTrueMilkUI.Manufacturing.frmPreviewQrDetail(rows, receivedAt))
                frm.ShowDialog(this);
        }

        // ════════════════════════════════════════════════════════════════
        // Tab QR Is Used — Hiển thị bảng code
        // ════════════════════════════════════════════════════════════════

        private void InitDgvQrIsUsed()
        {
            dgvQrIsUsed.Columns.Clear();
            dgvQrIsUsed.AutoGenerateColumns = false;
            dgvQrIsUsed.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvQrIsUsed.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvQrIsUsed.ScrollBars = ScrollBars.Both;
            dgvQrIsUsed.ReadOnly = true;
            dgvQrIsUsed.AllowUserToAddRows = false;
            dgvQrIsUsed.AllowUserToDeleteRows = false;
            dgvQrIsUsed.MultiSelect = false;

            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQrCode", HeaderText = "QR Code", DataPropertyName = QrCode, MinimumWidth = 120 });
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIsUsed", HeaderText = "Đã dùng", DataPropertyName = IsUsed, MinimumWidth = 60 });
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIsPrinted", HeaderText = "Đã in", DataPropertyName = IsPrinted, MinimumWidth = 60 });
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBatch", HeaderText = "Batch", DataPropertyName = Batch, MinimumWidth = 80 });
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLineId", HeaderText = "Line", DataPropertyName = LineId, MinimumWidth = 60 });
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFactoryCode", HeaderText = "Factory", DataPropertyName = FactoryCode, MinimumWidth = 60 });
            dgvQrIsUsed.Columns["colFactoryCode"].Visible = false;
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colJobName", HeaderText = "Job Name", DataPropertyName = JobName, MinimumWidth = 100 });
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIsSent", HeaderText = "Đã đồng bộ", DataPropertyName = IsSentToMaster, MinimumWidth = 70 });
            dgvQrIsUsed.RowTemplate.Height = 35;
            dgvQrIsUsed.CellFormatting += DgvQrIsUsed_CellFormatting;
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMarkedSentAt", HeaderText = "Thời gian đồng bộ", DataPropertyName = MarkedSentAt, MinimumWidth = 120 });
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUsedAt", HeaderText = "Thời gian in", DataPropertyName = UsedAt, MinimumWidth = 120 });
            dgvQrIsUsed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNote", HeaderText = "Ghi chú", DataPropertyName = "Note", MinimumWidth = 100 });
        }

        private void DgvQrIsUsed_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string colName = dgvQrIsUsed.Columns[e.ColumnIndex].Name;

            if (colName == "colJobName")
            {
                if (e.Value is string jobVal)
                {
                    e.Value = jobVal.Replace(' ', '_');
                    e.FormattingApplied = true;
                }
                return;
            }

            if (colName != "colIsUsed" && colName != "colIsPrinted" && colName != "colIsSent") return;
            if (e.Value == null || e.Value == DBNull.Value) return;

            if (ConvertToBool(e.Value))
            {
                e.Value = "✓";
                e.CellStyle.ForeColor = Color.Green;
                e.CellStyle.Font = new Font(dgvQrIsUsed.Font.FontFamily, 14, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.FormattingApplied = true;
            }
            else
            {
                e.Value = "";
                e.FormattingApplied = true;
            }
        }

        private async Task LoadQrIsUsedAsync(string filter = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var rows = await Task.Run(() => FetchQrIsUsedRows(filter, fromDate, toDate));
                if (InvokeRequired)
                    Invoke(new Action(() => BindQrIsUsedRows(rows)));
                else
                    BindQrIsUsedRows(rows);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[TabQrIsUsed] LoadQrIsUsedAsync lỗi: " + ex.Message, ex);
            }
        }

        private List<Dictionary<string, object>> FetchQrIsUsedRows(string filter, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new List<Dictionary<string, object>>();
            string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
            if (string.IsNullOrWhiteSpace(connStr))
                connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                    "postgresql",
                    Shared.Settings.THLocalDbServer,
                    Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername,
                    Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);

            // ── Thử PostgreSQL trước ─────────────────────────────────
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                try
                {
                    string table = THDb.Code;
                    string currentLineId = Shared.Settings.LineId ?? "";
                    string lineFilter = string.IsNullOrWhiteSpace(currentLineId)
                        ? ""
                        : $"AND {LineId} = @lineId";

                    string whereFilter = string.IsNullOrWhiteSpace(filter)
                        ? lineFilter
                        : $"{lineFilter} AND ({QrCode} ILIKE @kw OR COALESCE({JobName},'') ILIKE @kw OR COALESCE({Batch},'') ILIKE @kw OR COALESCE({LineId},'') ILIKE @kw)";

                    if (fromDate.HasValue)
                        whereFilter += $" AND {ReceivedAt} >= @fromDate";
                    if (toDate.HasValue)
                        whereFilter += $" AND {ReceivedAt} <= @toDate";

                    string sql = $@"SELECT {Id}, {QrCode}, {IsUsed}, {IsPrinted}, {IsSentToMaster},
                                       COALESCE(to_char({MarkedSentAt},'YYYY-MM-DD HH24:MI:SS'),'') AS {MarkedSentAt},
                                       COALESCE({Batch},'') AS {Batch},
                                       COALESCE({LineId},'') AS {LineId},
                                       COALESCE({FactoryCode},'') AS {FactoryCode},
                                       COALESCE({JobName},'') AS {JobName},
                                       {UsedAt}
                                  FROM ""{table}""
                                  WHERE 1=1 {whereFilter}
                                  ORDER BY {Id} DESC
                                  LIMIT 1000";

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                        {
                            if (!string.IsNullOrWhiteSpace(filter))
                                cmd.Parameters.AddWithValue("kw", "%" + filter.Trim() + "%");
                            if (!string.IsNullOrWhiteSpace(currentLineId))
                                cmd.Parameters.AddWithValue("lineId", currentLineId);
                            if (fromDate.HasValue)
                                cmd.Parameters.AddWithValue("fromDate", fromDate.Value.Date);
                            if (toDate.HasValue)
                                cmd.Parameters.AddWithValue("toDate", toDate.Value.Date.AddDays(1));

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    result.Add(new Dictionary<string, object>
                                    {
                                        [Id] = reader.GetInt32(0),
                                        [QrCode] = reader.GetString(1),
                                        [IsUsed] = reader.GetBoolean(2),
                                        [IsPrinted] = !reader.IsDBNull(3) && reader.GetBoolean(3),
                                        [IsSentToMaster] = !reader.IsDBNull(4) && reader.GetBoolean(4),
                                        [MarkedSentAt] = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                        [Batch] = reader.GetString(6),
                                        [LineId] = reader.GetString(7),
                                        [FactoryCode] = reader.GetString(8),
                                        [JobName] = reader.GetString(9),
                                        [UsedAt] = reader.IsDBNull(10) ? (object)"" : reader.GetDateTime(10).ToString("yyyy-MM-dd HH:mm:ss")
                                    });
                                }
                            }
                        }
                    }
                    ProjectLogger.WriteInfo($"[TabQrIsUsed] PG: {result.Count} bản ghi (filter='{filter}')");
                    return result;
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteWarning("[TabQrIsUsed] PG lỗi → fallback SQLite: " + ex.Message);
                }
            }

            // ── Fallback SQLite ──────────────────────────────────────
            try
            {
                string kw = filter?.Trim() ?? "";
                string currentLineId = Shared.Settings.LineId ?? "";
                string sqliteLineFilter = string.IsNullOrWhiteSpace(currentLineId)
                    ? ""
                    : $"AND {LineId} = @lineId";
                using (var conn = new System.Data.SQLite.SQLiteConnection(
                    BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ConnStr))
                {
                    conn.Open();
                    string whereKw = string.IsNullOrWhiteSpace(kw)
                        ? ""
                        : $"AND ({QrCode} LIKE @kw OR COALESCE({JobName},'') LIKE @kw OR COALESCE({Batch},'') LIKE @kw OR COALESCE({LineId},'') LIKE @kw)";
                    string sqlLite = $@"SELECT {Id}, {QrCode}, {IsUsed}, {IsPrinted}, {IsSentToMaster},
                                           COALESCE({MarkedSentAt},'') AS {MarkedSentAt},
                                           COALESCE({Batch},''), COALESCE({LineId},''),
                                           COALESCE({FactoryCode},''), COALESCE({JobName},''),
                                           {UsedAt}
                                        FROM {THDb.Code}
                                        WHERE 1=1 {sqliteLineFilter} {whereKw}
                                        ORDER BY {Id} DESC LIMIT 1000";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sqlLite;
                        if (!string.IsNullOrWhiteSpace(kw))
                            cmd.Parameters.AddWithValue("@kw", "%" + kw + "%");
                        if (!string.IsNullOrWhiteSpace(currentLineId))
                            cmd.Parameters.AddWithValue("@lineId", currentLineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new Dictionary<string, object>
                                {
                                    [Id] = reader.GetInt32(0),
                                    [QrCode] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    [IsUsed] = reader.GetInt32(2) == 1,
                                    [IsPrinted] = reader.GetInt32(3) == 1,
                                    [IsSentToMaster] = reader.GetInt32(4) == 1,
                                    [MarkedSentAt] = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    [Batch] = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                    [LineId] = reader.IsDBNull(7) ? "" : reader.GetString(7),
                                    [FactoryCode] = reader.IsDBNull(8) ? "" : reader.GetString(8),
                                    [JobName] = reader.IsDBNull(9) ? "" : reader.GetString(9),
                                    [UsedAt] = reader.IsDBNull(10) ? (object)"" : reader.GetString(10)
                                });
                            }
                        }
                    }
                }
                ProjectLogger.WriteInfo($"[TabQrIsUsed] SQLite: {result.Count} bản ghi (filter='{filter}')");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[TabQrIsUsed] SQLite fallback lỗi: " + ex.Message, ex);
            }
            return result;
        }
        //private List<Dictionary<string, object>> FetchQrIsUsedRows(string filter)
        //{
        //    var result = new List<Dictionary<string, object>>();
        //    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
        //    if (string.IsNullOrWhiteSpace(connStr))
        //        connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
        //            "postgresql",
        //            Shared.Settings.THLocalDbServer,
        //            Shared.Settings.THLocalDbPort,
        //            Shared.Settings.THLocalDbUsername,
        //            Shared.Settings.THLocalDbPassword,
        //            Shared.Settings.THLocalDbDatabase);

        //    // ── Thử PostgreSQL trước ─────────────────────────────────
        //    if (!string.IsNullOrWhiteSpace(connStr))
        //    {
        //        try
        //        {
        //            string table = Shared.Settings.THLocalDbTable;
        //            if (string.IsNullOrWhiteSpace(table)) table = "code";

        //            string whereFilter = string.IsNullOrWhiteSpace(filter)
        //                ? ""
        //                : "AND (qr_code ILIKE @kw OR COALESCE(job_name,'') ILIKE @kw OR COALESCE(batch,'') ILIKE @kw OR COALESCE(line_id,'') ILIKE @kw)";

        //            string sql = $@"SELECT id, qr_code, is_used,
        //                               COALESCE(batch,'') AS batch,
        //                               COALESCE(line_id,'') AS line_id,
        //                               COALESCE(factory_code,'') AS factory_code,
        //                               COALESCE(job_name,'') AS job_name,
        //                               used_at
        //                          FROM ""{table}""
        //                          WHERE 1=1 {whereFilter}
        //                          ORDER BY id DESC
        //                          LIMIT 1000";

        //            using (var conn = new Npgsql.NpgsqlConnection(connStr))
        //            {
        //                conn.Open();
        //                using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
        //                {
        //                    if (!string.IsNullOrWhiteSpace(filter))
        //                        cmd.Parameters.AddWithValue("kw", "%" + filter.Trim() + "%");

        //                    using (var reader = cmd.ExecuteReader())
        //                    {
        //                        while (reader.Read())
        //                        {
        //                            var row = new Dictionary<string, object>
        //                            {
        //                                ["id"] = reader.GetInt32(0),
        //                                [QrCode] = reader.GetString(1),
        //                                [IsUsed] = reader.GetBoolean(2),
        //                                [Batch] = reader.GetString(3),
        //                                [LineId] = reader.GetString(4),
        //                                [FactoryCode] = reader.GetString(5),
        //                                [JobName] = reader.GetString(6),
        //                                [UsedAt] = reader.IsDBNull(7) ? (object)"" : reader.GetDateTime(7).ToString("yyyy-MM-dd HH:mm:ss")
        //                            };
        //                            result.Add(row);
        //                        }
        //                    }
        //                }
        //            }
        //            ProjectLogger.WriteInfo($"[TabQrIsUsed] PG: {result.Count} bản ghi (filter='{filter}')");
        //            return result;
        //        }
        //        catch (Exception ex)
        //        {
        //            ProjectLogger.WriteWarning("[TabQrIsUsed] PG lỗi → fallback SQLite: " + ex.Message);
        //        }
        //    }

        //    // ── Fallback SQLite ──────────────────────────────────────
        //    try
        //    {
        //        var sqliteRows = RLinkLogService.FetchQrCodesFromSQLite(Shared.Settings.LineId ?? "", isUsed: null, keyword: filter, limit: 1000);
        //        foreach (var r in sqliteRows)
        //        {
        //            result.Add(new Dictionary<string, object>
        //            {
        //                ["id"] = r.Id,
        //                [QrCode] = r.QrCode ?? "",
        //                [IsUsed] = r.IsUsed,
        //                [Batch] = r.Batch ?? "",
        //                [LineId] = r.LineId ?? "",
        //                [FactoryCode] = r.FactoryCode ?? "",
        //                [JobName] = r.JobName ?? "",
        //                [UsedAt] = r.UsedAt.HasValue ? r.UsedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""
        //            });
        //        }
        //        ProjectLogger.WriteInfo($"[TabQrIsUsed] SQLite: {result.Count} bản ghi (filter='{filter}')");
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError("[TabQrIsUsed] SQLite fallback lỗi: " + ex.Message, ex);
        //    }
        //    return result;
        //}

        private void BindQrIsUsedRows(List<Dictionary<string, object>> rows)
        {
            dgvQrIsUsed.DataSource = null;
            var dt = new System.Data.DataTable();
            //dt.Columns.Add("id", typeof(int));
            dt.Columns.Add(QrCode, typeof(string));
            dt.Columns.Add(IsUsed, typeof(bool));
            dt.Columns.Add(IsPrinted, typeof(bool));
            dt.Columns.Add(Batch, typeof(string));
            dt.Columns.Add(LineId, typeof(string));
            dt.Columns.Add(FactoryCode, typeof(string));
            dt.Columns.Add(JobName, typeof(string));
            dt.Columns.Add(IsSentToMaster, typeof(bool));
            dt.Columns.Add("Note", typeof(string));
            dt.Columns.Add(MarkedSentAt, typeof(string));
            dt.Columns.Add(UsedAt, typeof(string));

            foreach (var r in rows)
            {
                bool isPrinted = ConvertToBool(r[IsPrinted]);
                bool isSent = ConvertToBool(r[IsSentToMaster]);
                string jobName = r[JobName]?.ToString() ?? "";

                string note;
                if (isSent)
                    note = "Đã đồng bộ";
                else if (isPrinted && !string.IsNullOrWhiteSpace(jobName))
                    note = "Cần đồng bộ";
                else
                    note = "Chưa đủ điều kiện";

                dt.Rows.Add(r[QrCode], r[IsUsed], r[IsPrinted], r[Batch], r[LineId], r[FactoryCode], r[JobName], r[IsSentToMaster], note, r[MarkedSentAt], r[UsedAt]);
            }

            dgvQrIsUsed.DataSource = dt;
            dgvQrIsUsed.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private static bool ConvertToBool(object value)
        {
            if (value is bool b) return b;
            if (value is int i) return i != 0;
            if (value is long l) return l != 0;
            return false;
        }

        private void TabControl1_QrIsUsed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabQrIsUsed)
            {
                dtpFromDateQr.Value = DateTime.Now.AddDays(-7);
                dtpToDateQr.Value = DateTime.Now;
                _ = LoadQrIsUsedAsync(txtSearchQr.Text, dtpFromDateQr.Value, dtpToDateQr.Value);
                _ = UpdateQrStatsLabelsAsync();
            }
        }

        private void TabControl1_HistoryReceiveQr_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabHistoryReciveQr)
            {
                dtpFromDateQrRecive.Value = DateTime.Now.AddDays(-7);
                dtpToDateQrRecive.Value = DateTime.Now;
                _ = LoadHistoryReceiveQrAsync(txtSearchHistoryReceiveQr.Text, dtpFromDateQrRecive.Value, dtpToDateQrRecive.Value);
            }
        }

        private void TxtSearchQr_TextChanged(object sender, EventArgs e)
        {
            // Chỉ tìm kiếm khi nhấn nút, không live search
        }

        private void BtnRefreshSearchQr_Click(object sender, EventArgs e)
        {
            _ = LoadQrIsUsedAsync(txtSearchQr.Text, dtpFromDateQr.Value, dtpToDateQr.Value);
        }


        /// <summary>
        /// Mode 1 & 2: Validate → kiểm tra kho QR → render CSV từ qrbank_data → SaveJob.
        /// Không gọi SendGeneratedCodes (bỏ check trùng API).
        /// </summary>

        private static DateTime _lastQrConfigFetch = DateTime.MinValue;
        private static QrConfig _cachedQrConfig = null;

        private async Task SaveJobForMode1Or2Async(THTrueMilkOperatingMode mode)
        {
            // ── 1. Validate input ────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(Shared.Settings.LineId))
            {
                DisplayJobLoading(false);
                CuzMessageBox.Show("Chưa gán Line! Vui lòng vào Settings → Gán Line trước khi tạo job.",
                    "Chưa gán Line", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string rawTon = txtTotalQR.Text.Trim().Replace(",", ".");
            if (string.IsNullOrWhiteSpace(rawTon) || !double.TryParse(rawTon,
                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture,
                out double ton) || ton <= 0)
            {
                DisplayJobLoading(false);
                CuzMessageBox.Show("Vui lòng nhập số tấn hợp lệ (> 0)!",
                    "Chưa nhập số tấn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTotalQR.Focus();
                return;
            }
            if (ton > 9999)
            {
                DisplayJobLoading(false);
                CuzMessageBox.Show("Số tấn không được vượt quá 9.999!",
                    "Số tấn vượt giới hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTotalQR.Focus();
                return;
            }

            var selectedProduct = GetSelectedProduct();
            if (selectedProduct == null)
            {
                DisplayJobLoading(false);
                CuzMessageBox.Show("Vui lòng chọn sản phẩm cần tạo job!", "Chưa chọn sản phẩm",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 1b. Đọc Volume (dung tích) từ sản phẩm đã chọn ──────────────────────
            if (!selectedProduct.Volume.HasValue || selectedProduct.Volume <= 0)
            {
                DisplayJobLoading(false);
                CuzMessageBox.Show("Sản phẩm chưa có dung tích (Volume)! Vui lòng chọn sản phẩm khác.",
                    "Thiếu dung tích", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int volume = selectedProduct.Volume.Value;

            // ── 1c. Tính số hộp từ công thức: Số hộp = (Số tấn × 1.000.000) ÷ Size(ml) ──
            int totalQr = (int)(ton * 1_000_000 / volume);
            if (totalQr <= 0)
            {
                DisplayJobLoading(false);
                CuzMessageBox.Show("Số hộp tính ra = 0. Kiểm tra lại số tấn hoặc dung tích sản phẩm!",
                    "Số hộp không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTotalQR.Focus();
                return;
            }
            if (totalQr >= 4_000_000)
            {
                DisplayJobLoading(false);
                CuzMessageBox.Show($"Số hộp ({totalQr:N0}) vượt quá giới hạn 3.999.999!",
                    "Số hộp vượt giới hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTotalQR.Focus();
                return;
            }

            // Hạn sử dụng lấy từ sản phẩm đã chọn
            //string batchNo = selectedRow.Cells["Exp"].Value?.ToString() ?? "";
           
            string batchNo = "";

            // ── bufferPrint = số mã gửi trước cho máy in (printer prefetch buffer) ──

            _TimerMidnightReset.Stop();
            try
            {
                // ── 2. Thông tin sản phẩm ─────────────────────────────────────────
                string productId = selectedProduct.ProductId ?? "";
                string productName = selectedProduct.ProductName ?? "";

                if (string.IsNullOrWhiteSpace(productId))
                {
                    CuzMessageBox.Show("Không lấy được mã sản phẩm từ danh sách!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ── Lưu Exp (số tháng) từ sản phẩm ──
                int expiryMonths = 6;
                if (selectedProduct.Exp.HasValue && selectedProduct.Exp > 0)
                    expiryMonths = selectedProduct.Exp.Value;
                _JobModel.THJobExpiryMonths = expiryMonths;

                int bufferPrint = Shared.Settings.THBufferCount > 0 ? Shared.Settings.THBufferCount : 1;
                int nMinutes = Shared.Settings.THNMinutes > 0 ? Shared.Settings.THNMinutes : 30;
                int delta = Shared.Settings.THDeltaMinutes;
                int intervalMin = mode == THTrueMilkOperatingMode.AutoRefreshByTime
                    ? nMinutes
                    : Math.Max(nMinutes - delta, 1);

                // ── 3. Tính số QR cần thiết ──────────────────────────────────────
                const int SPEED_PER_HOUR = 24000; // sản phẩm/giờ (tốc độ máy)

                int qrForCsv = 1;
                string modeLabel;
                int qrMinRequired;

                if (mode == THTrueMilkOperatingMode.BatchOneQrCode ||
                    mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
                {
                    double runHours = (double)totalQr / SPEED_PER_HOUR;
                    double days = runHours / 24;
                    qrMinRequired = Math.Max(1, (int)Math.Ceiling(days));

                    string modeSuffix = mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                        ? " (không đổi QR)" : "";
                    modeLabel = $"Mode 1{modeSuffix} (Batch/QR) – {totalQr:N0} hộp\n"
                             // + $"Tốc độ        : {SPEED_PER_HOUR:N0} SP/h\n"
                              + $"Thời gian     : {runHours:F1} giờ ({days:F1} ngày)\n"
                              + $"Cần QR        : {qrMinRequired} mã\n"
                              + $"Delta reset   : {(mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange ? "Không đổi QR" : $"00:00 trước {delta} giây")}";
                }
                else if (mode == THTrueMilkOperatingMode.AutoRefreshByTime)
                {
                    double runHours = (double)totalQr / SPEED_PER_HOUR;
                    int intervals = Math.Max(1, (int)Math.Ceiling(runHours * 60 / nMinutes));
                    qrMinRequired = intervals;

                    modeLabel = $"Mode 2 (AutoRefresh) – {totalQr:N0} hộp\n"
                             // + $"Tốc độ        : {SPEED_PER_HOUR:N0} SP/h\n"
                              + $"Thời gian     : {runHours:F1} giờ\n"
                              + $"Đổi QR mỗi    : {nMinutes} phút\n"
                              + $"Cần QR        : {qrMinRequired} mã\n"
                              + $"Buffer máy in : {bufferPrint} mã";
                }
                else // ProductOneQrCode
                {
                    qrMinRequired = totalQr;
                    qrForCsv = totalQr;

                    modeLabel = $"Mode 3 (Product/QR) – {totalQr:N0} hộp\n"
                             // + $"Tốc độ        : {SPEED_PER_HOUR:N0} SP/h\n"
                              + $"Thời gian     : {(double)totalQr / SPEED_PER_HOUR:F1} giờ\n"
                              + $"Cần QR        : {qrMinRequired:N0} mã (mỗi SP 1 mã)\n"
                              + $"Buffer máy in : {bufferPrint} mã";
                }

                // ── Áp dụng hệ số dự phòng ────────────────────────────────────────
                int qrRaw = qrMinRequired;
                double reserveFactor = Shared.Settings.THReserveFactor > 0
                    ? Shared.Settings.THReserveFactor
                    : 1.5;
                qrMinRequired = (int)Math.Ceiling(qrMinRequired * reserveFactor);
                modeLabel += $"\nHệ số dự phòng : {reserveFactor}× ({qrRaw:N0} → {qrMinRequired:N0})";

                // ── 4. Lấy QR config từ R-Link Master (cached, chỉ gọi API mỗi 1 giờ) ──
                string qrBaseUrl = Shared.Settings.THQrBaseUrl ?? "";
                int qrNumberOfUrl = Shared.Settings.THQrNumberOfUrl;

                if (_cachedQrConfig == null || (DateTime.Now - _lastQrConfigFetch).TotalHours >= 1)
                {
                    try
                    {
                        var service = RLinkMasterServiceFactory.Instance;
                        var qrConfig = await service.GetQrConfigAsync();
                        if (qrConfig != null)
                        {
                            _cachedQrConfig = qrConfig;
                            _lastQrConfigFetch = DateTime.Now;
                            qrBaseUrl = qrConfig.BaseUrl ?? "";
                            qrNumberOfUrl = qrConfig.NumberOfUrl;
                            Shared.Settings.THQrBaseUrl = qrBaseUrl;
                            Shared.Settings.THQrNumberOfUrl = qrNumberOfUrl;
                            Shared.SaveSettings();
                        }
                    }
                    catch
                    {
                        // Offline: dùng cache cũ hoặc settings đã lưu
                        if (_cachedQrConfig != null)
                        {
                            qrBaseUrl = _cachedQrConfig.BaseUrl ?? "";
                            qrNumberOfUrl = _cachedQrConfig.NumberOfUrl;
                        }
                    }
                }
                else
                {
                    qrBaseUrl = _cachedQrConfig.BaseUrl ?? "";
                    qrNumberOfUrl = _cachedQrConfig.NumberOfUrl;
                }

                // ── Validate BaseUrl + NumberOfUrl từ QR mẫu trong DB ──
                string gtin = selectedProduct.ProductGtin ?? "";
                if (!string.IsNullOrWhiteSpace(gtin) && !string.IsNullOrWhiteSpace(qrBaseUrl))
                {
                    string dbSampleQr = await GetSampleQrByGtinAsync(gtin);
                    if (!string.IsNullOrWhiteSpace(dbSampleQr))
                    {
                        if (!dbSampleQr.StartsWith(qrBaseUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            CuzMessageBox.Show(
                                $"QR không khớp base_url!\n" +
                                $"QR DB: {dbSampleQr}\n" +
                                $"Base URL mong đợi: {qrBaseUrl}\n\n" +
                                "Vui lòng làm mới dữ liệu trước khi tạo job.",
                                "Lỗi BaseUrl", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (qrNumberOfUrl > 0 && dbSampleQr.Length != qrNumberOfUrl)
                        {
                            CuzMessageBox.Show(
                                $"QR không khớp số ký tự!\n" +
                                $"QR DB: {dbSampleQr}\n" +
                                $"Số ký tự QR: {dbSampleQr.Length}\n" +
                                $"Số ký tự mong đợi: {qrNumberOfUrl}\n\n" +
                                "Vui lòng kiểm tra lại cấu hình QR.",
                                "Lỗi số ký tự", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                // ── 5. Kiểm tra kho QR theo GTIN + validate ──────────────────────
            

                // ── Validate GTIN với Camera Programs & Printer Templates ──
                if (!string.IsNullOrWhiteSpace(gtin))
                {
                    var camPrograms = Shared.Settings.CachedCameraPrograms ?? new List<string>();
                    var printerTpls = Shared.Settings.CachedPrinterTemplates ?? new List<string>();
                    bool matchCamera = camPrograms.Any(x => x.Split('_').Last() == gtin);
                    bool matchPrinter = printerTpls.Contains(gtin);

                    if (!matchCamera || !matchPrinter)
                    {
                        string detail = !matchCamera && !matchPrinter
                            ? "không khớp với cả danh sách chương trình camera và template printer"
                            : !matchCamera ? "không khớp với danh sách chương trình camera" : "không khớp với danh sách template printer";
                        CuzMessageBox.Show(
                            $"Sản phẩm GTIN '{gtin}' {detail}.\n\nVui lòng kiểm tra lại.",
                            "GTIN không khớp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                int availableQr = await CountValidQrByGtinAsync(gtin, qrBaseUrl, qrNumberOfUrl);

                if (availableQr < qrMinRequired)
                {
                    CuzMessageBox.Show(
                        $"{modeLabel}\n\n"
                      + $"GTIN          : {gtin}\n"
                      + $"QR hợp lệ     : {availableQr:N0}\n"
                      + $"Cần tối thiểu : {qrMinRequired:N0} QR\n\n"
                      + "Vui lòng đẩy thêm QR từ R-Link Master!",
                        "Không đủ QR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ── 6. Xác nhận ──────────────────────────────────────────────────
                // Ẩn loading trong lúc user đọc và xác nhận dialog
                DisplayJobLoading(false);

                string qrInfo = $"Hiện có {availableQr:N0} / cần {qrMinRequired:N0}";
                string confirm = $"{modeLabel}\n\n"
                               + $"Sản phẩm  : [{productId}] {productName}\n"
                               + $"GTIN       : {gtin}\n"
                               + $"Số lượng  : {totalQr:N0} hộp\n"
                               + $"QR hợp lệ : {qrInfo}";
                if (!frmNotificationJob.ShowConfirmWithGtinHighlight(confirm, gtin, new[] { qrInfo }))
                {
                    // User từ chối → kết thúc, loading đã tắt ở trên
                    return;
                }

                // ── Validate QR Code ──
                string sampleQr = await GetSampleQrFromDatabaseAsync(gtin);

                // Tính NSX/HSD — chỉ hiển thị với Mode 4
                string nsxHsd = "";
                if (mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
                {
                    try
                    {
                        DateTime nsxDate = DateTime.Now;
                        string nsx = nsxDate.ToString("dd MM yy");
                        int hsdMonths = selectedProduct.Exp ?? 6;
                        string hsd = nsxDate.AddDays(hsdMonths - 1).ToString("dd MM yy");
                        nsxHsd = $"NSX: {nsx}\nHSD: {hsd}";
                    }
                    catch { }
                }

                // Tải ảnh sản phẩm từ API
                System.Drawing.Image productImage = null;
                string productLocalPath = null;
                try
                {
                    var prod = GetSelectedProduct();
                    if (prod != null && !string.IsNullOrWhiteSpace(prod.Image))
                    {
                        productLocalPath = prod.Image;
                        // Nếu chưa có file local → tải từ API
                        if (!File.Exists(productLocalPath))
                        {
                            var loadingImg = frmLoadingUi.ShowLoading(this, "TẢI ẢNH", "Đang tải ảnh sản phẩm...");
                            try
                            {
                                productImage = ProductImageHelper.GetProductImage(prod.Image, prod.ProductId);
                            }
                            finally
                            {
                                frmLoadingUi.CloseLoading(ref loadingImg);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteWarning($"[SaveJob] Không tải được ảnh sản phẩm: {ex.Message}");
                }

                using (var qrImage = GenerateQrImageUsingZXing(sampleQr))
                {
                    using (var frmValidate = new frmValidateQrCode())
                    {
                        // Ưu tiên load từ file local (nhanh hơn)
                        if (!string.IsNullOrEmpty(productLocalPath) && File.Exists(productLocalPath))
                        {
                            frmValidate.SetValidateDataFromLocal(
                                productLocalPath, qrImage, sampleQr, productName, productId, nsxHsd, gtin);
                        }
                        else
                        {
                            frmValidate.SetValidateData(
                                productImage, qrImage, sampleQr, productName, productId, nsxHsd, gtin);
                        }
                        if (frmValidate.ShowDialog() != DialogResult.OK)
                        {
                            // User từ chối xác nhận QR → kết thúc
                            return;
                        }
                    }
                }

                // User đã xác nhận xong → hiện loading đúng message
                DisplayJobLoading(true, "Đang tạo job...");

                // ── 6. Build CSV — chỉ 1 QR lặp totalQr lần ─────────────────────
                //
                //  Mode 1: [QR1 × totalQr dòng]    — 1 QR dùng cả ngày
                //  Mode 2: [QR1 × totalQr dòng]    — timer sẽ rebuild theo thời gian
                //
                //  Ví dụ Mode 2 (totalQr=1000, bufferPrint=50, intervalMin=10p):
                //    T=0p:   CSV = [QR1 × 1000]
                //    T=10p:  printed≈100, buffer=50
                //            consumed = min(100+50, 1000) = 150
                //            CSV = [QR1 × 150] + [QR2 × 850]
                //    T=20p:  printed≈200, buffer=50
                //            consumed = min(200+50, 1000) = 250 (tính trên file gốc)
                //            CSV = [dòng cũ × 250] + [QR3 × 750]
                //
                var (csvLines, qrIds) = await BuildCsvFromQrBankDbAsync(
                    mode, totalQr, qrForCsv, batchNo, gtin);

                if (csvLines == null || csvLines.Count == 0)
                {
                    CuzMessageBox.Show(
                        "Không thể tạo database từ kho QR!\nKiểm tra kết nối PostgreSQL.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DisplayJobLoading(false);
                    return;
                }

                // ── 7. Ghi file CSV ───────────────────────────────────────────────
                string dbDir = CommVariables.PathDatabaseApp;
                if (!Directory.Exists(dbDir)) Directory.CreateDirectory(dbDir);

                string prefix = (mode == THTrueMilkOperatingMode.BatchOneQrCode || mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange) ? "Mode1"
                    : mode == THTrueMilkOperatingMode.ProductOneQrCode ? "Mode3"
                    : "Mode2";
                string csvPath = Path.Combine(dbDir,
                    $"{prefix}_{batchNo}_{productId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                File.WriteAllLines(csvPath, csvLines, Encoding.UTF8);

                ProjectLogger.WriteInfo(
                    $"[SaveJob] {prefix} CSV: {csvLines.Count} dòng "
                  + $"| QR: '{csvLines[0]}' | path='{csvPath}'");

                // ── 8. Cập nhật UI / JobModel ─────────────────────────────────────
                string jobFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{prefix}_{productId}_{Settings.RLinkName}";

                _suppressTextChangedSync = true;
                txtFileName.Text = _JobModel.FileName = jobFileName;
                txtDirectoryDatabse.Text = _JobModel.DirectoryDatabase = csvPath;
                _suppressTextChangedSync = false;

                _PODFormat.Clear();
                txtPODFormat.Text = "";
                Shared.databasePath = "";
                isPushDatabase = false;
                _NumberTotalsCode = totalQr;
                Shared.PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder);
                DisplayJobLoading(false);

                // ── 9. SaveJob ────────────────────────────────────────────────────
                bool saveOk = false;
                string savedCsvPath = csvPath; // giữ để rollback nếu cần
                try
                {
                    string savedFileName = _JobModel.FileName;
                    string savedDirectoryDb = _JobModel.DirectoryDatabase;
                    string savedTemplate = _JobModel.TemplatePrint;

                    bool camOk = await TryChangeKeyenceProgramAsync();
                    if (!camOk)
                    {
                        DisplayJobLoading(false);
                        return;
                    }
                    SaveJob();
                    saveOk = true;

                    // Snapshot cấu hình TH vào JobModel
                    _JobModel.THJobOperatingMode = Shared.Settings.THOperatingMode;
                    _JobModel.THJobDeltaMinutes = Shared.Settings.THDeltaMinutes;
                    _JobModel.THJobBufferCount = bufferPrint;
                    _JobModel.THJobNMinutes = nMinutes;
                    _JobModel.THJobLogInterval = Shared.Settings.THLogInterval;
                    _JobModel.THJobErrorImageFolder = Shared.Settings.THErrorImageFolder ?? "";
                    _JobModel.THJobCameraModelForTraining = _matchedCameraProgram;
                    _JobModel.THJobMonitorInterval = Shared.Settings.THMonitorInterval;
                    _JobModel.THJobQrThreshold = Shared.Settings.THQrThreshold;
                    _JobModel.THJobExpiryMonths = expiryMonths;
                    _JobModel.THMaxConsecutiveError = Shared.Settings.THMaxConsecutiveError;
                    _JobModel.THJobProductId = productId;
                    _JobModel.THJobProductGtin = selectedProduct.ProductGtin ?? "";
                    _JobModel.THJobProductName = productName;
                    _JobModel.THJobImageUrl = selectedProduct.Image ?? "";
                    _JobModel.THJobBatchNo = batchNo;
                    _JobModel.THJobAllowNsxHsdChange = true;
                    Console.WriteLine($"[DEBUG SAVEJOB] THJobBatchNo = '{batchNo}'");
                    _JobModel.IsProcessOrderMode = false;
                    _JobModel.ProcessOrderItem = null;
                    _JobModel.CompareType = CompareType.Database;
                    _JobModel.SelectedBatchIndex = 0;
                    _JobModel.NumberTotalsCode = totalQr;

                    // Xóa file job rỗng nếu có
                    try
                    {
                        string emptyJobPath = Path.Combine(
                            CommVariables.PathJobsApp, Shared.Settings.JobFileExtension);
                        if (File.Exists(emptyJobPath)) File.Delete(emptyJobPath);
                    }
                    catch { }

                    _suppressTextChangedSync = true;
                    txtBatchNumber.Text = "";
                    txtTotalQR.Text = "";
                    _suppressTextChangedSync = false;

                    _ = Task.Run(() => _JobModel.SaveFile());
                    _FormMainPC?.UpdateJobInfoDisplay(_JobModel);

                    ProjectLogger.WriteInfo(
                        $"[SaveJob] ✔ Job '{savedFileName}' lưu thành công"
                      + $" | mode={mode} | buffer={bufferPrint} | interval={intervalMin}p");

                    // ── Hiển thị thông báo xác nhận tạo job thành công ──
                    string jobInfo = $"Tên job     : {savedFileName}\n"
                                   + $"Sản phẩm   : {productName}\n"
                                   + $"Số tấn      : {ton:N2}\n"
                                   + $"Số hộp      : {totalQr:N0}\n"
                                   + $"Chế độ      : {modeLabel}\n"
                                   + $"Ngày tạo   : {DateTime.Now:dd/MM/yyyy HH:mm}";
                  //  CuzMessageBox.Show(jobInfo, "Tạo job thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[SaveJobForMode1Or2] Lỗi lưu job: " + ex.Message, ex);
                    CuzMessageBox.Show($"Không thể lưu job!\n{ex.Message}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // ── 10. Mark QR là is_used=TRUE chỉ khi SaveJob thành công ───────
                if (saveOk && qrIds != null && qrIds.Count > 0)
                {
                    // Lấy danh sách QR code duy nhất từ CSV (chỉ 1 QR cho cả Mode 1 lẫn Mode 2)
                    var qrCodesUsed = csvLines
                        .Select(line => line.Split(',')[0].Trim())
                        .Where(q => !string.IsNullOrWhiteSpace(q))
                        .Distinct()
                        .ToList();

                    _ = Task.Run(async () => await MarkQrAsUsedAsync(qrIds, qrCodesUsed));

                    // Cập nhật _currentBatchQrCode để timer biết QR hiện tại
                    lock (_batchQrLock)
                    {
                        _currentBatchQrCode = qrCodesUsed[0];
                        _currentBatchId = batchNo;
                        _currentBatchDate = DateTime.Now;
                        if (_JobModel != null)
                        {
                            _JobModel.CurrentBatchDate = _currentBatchDate;
                            _JobModel.CurrentBatchQrCode = _currentBatchQrCode;
                        }
                    }
                    if (mode == THTrueMilkOperatingMode.AutoRefreshByTime)
                        ResetMode2Countdown();

                    ProjectLogger.WriteInfo(
                        $"[SaveJob] Mark {qrIds.Count} QR is_used=TRUE"
                      + $" | codes=[{string.Join(", ", qrCodesUsed)}]"
                      + $" | _currentBatchQrCode='{_currentBatchQrCode}'");
                }
                else if (!saveOk)
                {
                    // SaveJob thất bại → xóa file CSV tránh rác
                    try { if (File.Exists(savedCsvPath)) File.Delete(savedCsvPath); }
                    catch { }
                }

                UpdateMode1UI();
                Settings.ManufacturingListPO = null;
                _ = Task.Run(() => LoadProductListToGrid());
            }
            finally
            {
                DisplayJobLoading(false);
                _TimerMidnightReset.Start();
            }
        }

        /// <summary>
        /// Đếm số QR code hiện có trong qrbank_data hôm nay.
        /// Pattern giống frmTableData: luôn mở fresh NpgsqlConnection từ _lastConnStr.
        /// Có diagnostic logging để debug khi count = 0.
        /// </summary>
        public async Task<int> CountAvailableQrInDbAsync(string gtin = "")
        {
            return await Task.Run(() =>
            {
                try
                {
                    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                    string dbType = ucProductionTHTrueMilkSetting.ActiveDbType;

                    if (string.IsNullOrWhiteSpace(connStr))
                    {
                        connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                            "postgresql",
                            Shared.Settings.THLocalDbServer,
                            Shared.Settings.THLocalDbPort,
                            Shared.Settings.THLocalDbUsername,
                            Shared.Settings.THLocalDbPassword,
                            Shared.Settings.THLocalDbDatabase);
                    }

                    if (string.IsNullOrWhiteSpace(connStr))
                    {
                        ProjectLogger.WriteWarning("[CountAvailableQr] Không có PG connStr → fallback SQLite.");
                        return BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService
                            .CountAvailableQrInSQLite(Shared.Settings.LineId ?? "", gtin);
                    }

                    string table =  THDb.Code;
                    string lineId = Shared.Settings.LineId ?? string.Empty;
                    bool hasGtin = !string.IsNullOrWhiteSpace(gtin);

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        RLinkLogService.EnsurePgQrBankTable(conn);
                        int finalCount;

                        string baseWhere = $"{IsUsed} = FALSE";
                        if (hasGtin)
                            baseWhere += $" AND {ProductGtin} = @gtin";

                        if (!string.IsNullOrWhiteSpace(lineId))
                        {
                            using (var cmd = new Npgsql.NpgsqlCommand(
                                $"SELECT COUNT(*) FROM \"{table}\" WHERE {baseWhere} AND {LineId} = @line_id", conn))
                            {
                                cmd.Parameters.AddWithValue(LineId, lineId);
                                if (hasGtin)
                                    cmd.Parameters.AddWithValue("gtin", gtin);
                                finalCount = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                            if (finalCount == 0)
                            {
                                using (var cmd2 = new Npgsql.NpgsqlCommand(
                                    $"SELECT COUNT(*) FROM \"{table}\" WHERE {baseWhere}", conn))
                                {
                                    if (hasGtin)
                                        cmd2.Parameters.AddWithValue("gtin", gtin);
                                    finalCount = Convert.ToInt32(cmd2.ExecuteScalar());
                                }
                            }
                        }
                        else
                        {
                            using (var cmd = new Npgsql.NpgsqlCommand(
                                $"SELECT COUNT(*) FROM \"{table}\" WHERE {baseWhere}", conn))
                            {
                                if (hasGtin)
                                    cmd.Parameters.AddWithValue("gtin", gtin);
                                finalCount = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }
                        ProjectLogger.WriteInfo($"[CountAvailableQr] Kho QR hiện có: {finalCount} (line_id='{lineId}', gtin='{gtin}')");
                        return finalCount;
                    }
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteWarning("[CountAvailableQr] PG lỗi → fallback SQLite: " + ex.Message);
                    return BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService
                        .CountAvailableQrInSQLite(Shared.Settings.LineId ?? "", gtin);
                }
            });
        }

        public async Task<int> CountValidQrByGtinAsync(string gtin, string baseUrl, int numberOfUrl)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(gtin)) return 0;

                    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                    string dbType = ucProductionTHTrueMilkSetting.ActiveDbType;

                    if (string.IsNullOrWhiteSpace(connStr))
                        connStr = frmDatabase.GetConnectionString("postgresql",
                            Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                            Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                            Shared.Settings.THLocalDbDatabase);

                    if (string.IsNullOrWhiteSpace(connStr))
                        return 0;

                    string table = THDb.Code;
                    string lineId = Shared.Settings.LineId ?? string.Empty;
                    int validCount = 0;

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        RLinkLogService.EnsurePgQrBankTable(conn);

                        // Tối ưu: dùng COUNT(*) với điều kiện trong SQL, không đọc toàn bộ rows
                        string sql = $"SELECT COUNT(*) FROM \"{table}\" " +
                                     $"WHERE {THDb.IsUsed} = FALSE AND {THDb.ProductGtin} = @gtin";

                        if (!string.IsNullOrWhiteSpace(lineId))
                            sql += $" AND {THDb.LineId} = @line_id";
                        if (!string.IsNullOrWhiteSpace(baseUrl))
                            sql += $" AND {THDb.QrCode} ILIKE @base_url || '%'";
                        if (numberOfUrl > 0)
                            sql += $" AND LENGTH({THDb.QrCode}) = @number_of_url";

                        using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("gtin", gtin);
                            if (!string.IsNullOrWhiteSpace(lineId))
                                cmd.Parameters.AddWithValue("line_id", lineId);
                            if (!string.IsNullOrWhiteSpace(baseUrl))
                                cmd.Parameters.AddWithValue("base_url", baseUrl);
                            if (numberOfUrl > 0)
                                cmd.Parameters.AddWithValue("number_of_url", numberOfUrl);

                            validCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }

                    ProjectLogger.WriteInfo($"[CountValidQr] GTIN={gtin}, QR hợp lệ: {validCount} (baseUrl={baseUrl}, numberOfUrl={numberOfUrl})");
                    return validCount;
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[CountValidQr] Lỗi: " + ex.Message, ex);
                    return 0;
                }
            });
        }

        /// <summary>Lấy tổng số QR từ lần cấp phát đầu tiên (tb_IssuanceQRLogs) — dùng làm ngưỡng %.</summary>
        private async Task<int> GetInitialAllocationTotalAsync()
        {
            return await Task.Run(() =>
            {
                string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                if (string.IsNullOrWhiteSpace(connStr))
                    connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                        "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                        Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                        Shared.Settings.THLocalDbDatabase);
                if (string.IsNullOrWhiteSpace(connStr)) return -1;

                string lineId = Shared.Settings.LineId ?? string.Empty;
                if (string.IsNullOrWhiteSpace(lineId)) return -1;

                try
                {
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand(
                            $"SELECT {ColTotalCodes} FROM {ReceiveHistory} WHERE {LineId} = @lid ORDER BY {Id} ASC LIMIT 1", conn))
                        {
                            cmd.Parameters.AddWithValue("lid", lineId);
                            object result = cmd.ExecuteScalar();
                            if (result != null && int.TryParse(result.ToString(), out int total))
                                return total;
                        }
                    }
                }
                catch { }

                // Fallback: đếm tất cả QR có line_id trong tb_QRInventory
                try
                {
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand(
                            $"SELECT COUNT(*) FROM \"{THDb.Code}\" WHERE {LineId} = @lid", conn))
                        {
                            cmd.Parameters.AddWithValue("lid", lineId);
                            return Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                catch { return -1; }
            });
        }

        /// <summary>
        /// Đánh dấu QR đã dùng trong cả SQLite (theo qr_code) và PostgreSQL (theo id).
        /// Đồng thời cập nhật metadata thực tế: job_name, line_id, factory_code, batch, used_at.
        /// Sau khi mark local xong → notify R-Link Master (fire-and-forget).
        /// </summary>
        private async Task MarkQrAsUsedAsync(List<int> pgIds, List<string> qrCodes = null)
        {
            string jobName = _JobModel?.FileName ?? "";
            string lineId = Shared.Settings.LineId ?? "";
            string lineName = Shared.Settings.LineName ?? "";
            string factoryCode = Shared.Settings.FactoryCode ?? "";
            string productId = _JobModel?.THJobProductId ?? "";
            string manufacturedDate = "";
            string expiryDate = "";
            try
            {
                DateTime nsxDate = _currentBatchDate != DateTime.MinValue ? _currentBatchDate : DateTime.Now;
                manufacturedDate = nsxDate.ToString("dd MM yy");
                int expiryMonths = _JobModel?.THJobExpiryMonths ?? 6;
                expiryDate = nsxDate.AddDays(expiryMonths - 1).ToString("dd MM yy");
            }
            catch { }

            string batch = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(manufacturedDate, lineId);

            await Task.Run(() =>
            {
                // ── Mark SQLite — kèm metadata thực tế của line ──────────
                if (qrCodes != null && qrCodes.Count > 0)
                {
                    RLinkLogService.MarkQrAsUsedInSQLiteByCodeWithInfo(
                        qrCodes, jobName, lineId, lineName, factoryCode, batch, manufacturedDate, expiryDate, productId);
                    ProjectLogger.WriteInfo(
                        $"[MarkQrAsUsed] SQLite: {qrCodes.Count} QR | job='{jobName}'" +
                        $" line='{lineId}' lineName='{lineName}' factory='{factoryCode}' batch='{batch}' nsx='{manufacturedDate}' hsd='{expiryDate}' product_id='{productId}'");
                }
                else
                {
                    ProjectLogger.WriteWarning("[MarkQrAsUsed] Không có qrCodes → bỏ qua SQLite mark.");
                }

                // ── Mark PostgreSQL — cập nhật is_used + metadata theo qr_code ──
                if (qrCodes != null && qrCodes.Count > 0)
                {
                    RLinkLogService.MarkQrAsUsedInPgByCode(
                        qrCodes, jobName, lineId, lineName, factoryCode, batch, manufacturedDate, expiryDate, productId);
                }
                else if (pgIds != null && pgIds.Count > 0)
                {
                    // Fallback: mark theo id (dữ liệu cũ chưa có qrCodes)
                    try
                    {
                        string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                        if (string.IsNullOrWhiteSpace(connStr))
                            connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                                "postgresql",
                                Shared.Settings.THLocalDbServer,
                                Shared.Settings.THLocalDbPort,
                                Shared.Settings.THLocalDbUsername,
                                Shared.Settings.THLocalDbPassword,
                                Shared.Settings.THLocalDbDatabase);

                        if (string.IsNullOrWhiteSpace(connStr))
                        {
                            ProjectLogger.WriteWarning("[MarkQrAsUsed] Không có PG connStr → bỏ qua PG mark.");
                            return;
                        }

                        string table = THDb.Code;

                        string idList = string.Join(",", pgIds);
                        using (var conn = new Npgsql.NpgsqlConnection(connStr))
                        {
                            conn.Open();
                            using (var cmd = new Npgsql.NpgsqlCommand(
                                $@"UPDATE ""{table}""
                                   SET {IsUsed}      = TRUE,
                                       {UsedAt}      = NOW(),
                                       {JobName}     = @job_name,
                                       {LineId}      = @line_id,
                                       {LineName}    = @line_name,
                                       {FactoryCode} = @factory_code,
                                       {Batch}        = @batch
                                   WHERE {Id} IN ({idList})",
                                conn))
                            {
                                cmd.Parameters.AddWithValue(JobName, jobName);
                                cmd.Parameters.AddWithValue(LineId, lineId);
cmd.Parameters.AddWithValue(THDb.LineName, lineName);
                                cmd.Parameters.AddWithValue(FactoryCode, factoryCode);
                                cmd.Parameters.AddWithValue(Batch, batch);
                                int updated = cmd.ExecuteNonQuery();
                                ProjectLogger.WriteInfo(
                                    $"[MarkQrAsUsed] PG (by id): {updated}/{pgIds.Count} row(s) marked" +
                                    $" | job='{jobName}' batch='{batch}'");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ProjectLogger.WriteError("[MarkQrAsUsed] PG lỗi (SQLite đã mark): " + ex.Message, ex);
                    }
                }
                else
                {
                    ProjectLogger.WriteWarning("[MarkQrAsUsed] Không có qrCodes lẫn pgIds → bỏ qua PG mark.");
                }
            });

        }
        ///// <summary>
        ///// Đánh dấu QR đã dùng trong cả SQLite (theo qr_code) và PostgreSQL (theo id).
        ///// Đồng thời cập nhật metadata thực tế: job_name, line_id, factory_code, batch, used_at.
        ///// </summary>
        //private async Task MarkQrAsUsedAsync(List<int> pgIds, List<string> qrCodes = null)
        //{
        //    string jobName = _JobModel?.FileName ?? "";
        //    string lineId = Shared.Settings.LineId ?? "";
        //    string lineName = Shared.Settings.LineName ?? "";
        //    string factoryCode = Shared.Settings.FactoryCode ?? "";
        //    string batch = _JobModel?.THJobBatchNo ?? "";

        //    await Task.Run(() =>
        //    {
        //        // ── Mark SQLite — kèm metadata thực tế của line ──────────
        //        if (qrCodes != null && qrCodes.Count > 0)
        //        {
        //            RLinkLogService.MarkQrAsUsedInSQLiteByCodeWithInfo(
        //                qrCodes, jobName, lineId, lineName, factoryCode, batch);
        //            ProjectLogger.WriteInfo(
        //                $"[MarkQrAsUsed] SQLite: {qrCodes.Count} QR | job='{jobName}'" +
        //                $" line='{lineId}' lineName='{lineName}' factory='{factoryCode}' batch='{batch}'");
        //        }
        //        else
        //        {
        //            ProjectLogger.WriteWarning("[MarkQrAsUsed] Không có qrCodes → bỏ qua SQLite mark.");
        //        }

        //        // ── Mark PostgreSQL — cập nhật is_used + metadata theo qr_code ──
        //        if (qrCodes != null && qrCodes.Count > 0)
        //        {
        //            RLinkLogService.MarkQrAsUsedInPgByCode(
        //                qrCodes, jobName, lineId, lineName, factoryCode, batch);
        //        }
        //        else if (pgIds != null && pgIds.Count > 0)
        //        {
        //            // Fallback: mark theo id (dữ liệu cũ chưa có qrCodes)
        //            try
        //            {
        //                string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
        //                if (string.IsNullOrWhiteSpace(connStr))
        //                    connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
        //                        "postgresql",
        //                        Shared.Settings.THLocalDbServer,
        //                        Shared.Settings.THLocalDbPort,
        //                        Shared.Settings.THLocalDbUsername,
        //                        Shared.Settings.THLocalDbPassword,
        //                        Shared.Settings.THLocalDbDatabase);

        //                if (string.IsNullOrWhiteSpace(connStr))
        //                {
        //                    ProjectLogger.WriteWarning("[MarkQrAsUsed] Không có PG connStr → bỏ qua PG mark.");
        //                    return;
        //                }

        //                string table = Shared.Settings.THLocalDbTable;
        //                if (string.IsNullOrWhiteSpace(table)) table = "code";

        //                string idList = string.Join(",", pgIds);
        //                using (var conn = new Npgsql.NpgsqlConnection(connStr))
        //                {
        //                    conn.Open();
        //                    using (var cmd = new Npgsql.NpgsqlCommand(
        //                        $@"UPDATE ""{table}""
        //                           SET is_used      = TRUE,
        //                               used_at      = NOW(),
        //                               job_name     = @job_name,
        //                               line_id      = @line_id,
        //                               line_name    = @line_name,
        //                               factory_code = @factory_code,
        //                               batch        = @batch
        //                           WHERE id IN ({idList})",
        //                        conn))
        //                    {
        //                        cmd.Parameters.AddWithValue(JobName, jobName);
        //                        cmd.Parameters.AddWithValue(LineId, lineId);
        //                        cmd.Parameters.AddWithValue("line_name", lineName);
        //                        cmd.Parameters.AddWithValue(FactoryCode, factoryCode);
        //                        cmd.Parameters.AddWithValue(Batch, batch);
        //                        int updated = cmd.ExecuteNonQuery();
        //                        ProjectLogger.WriteInfo(
        //                            $"[MarkQrAsUsed] PG (by id): {updated}/{pgIds.Count} row(s) marked" +
        //                            $" | job='{jobName}' batch='{batch}'");
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                ProjectLogger.WriteError("[MarkQrAsUsed] PG lỗi (SQLite đã mark): " + ex.Message, ex);
        //            }
        //        }
        //        else
        //        {
        //            ProjectLogger.WriteWarning("[MarkQrAsUsed] Không có qrCodes lẫn pgIds → bỏ qua PG mark.");
        //        }
        //    });
        //}


        /// <summary>
        /// Build CSV từ qrbank_data theo FIFO — KHÔNG mark is_used ở đây.
        /// Trả về (csvLines, qrIds) để caller mark sau khi SaveJob thành công.
        /// </summary>
        private async Task<(List<string> csvLines, List<int> qrIds)> BuildCsvFromQrBankDbAsync(
     THTrueMilkOperatingMode mode, int quantity, int qrNeeded, string batch, string gtin = "")
        {
            return await Task.Run(() =>
            {
                try
                {
                    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                    if (string.IsNullOrWhiteSpace(connStr))
                        connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                            "postgresql",
                            Shared.Settings.THLocalDbServer,
                            Shared.Settings.THLocalDbPort,
                            Shared.Settings.THLocalDbUsername,
                            Shared.Settings.THLocalDbPassword,
                            Shared.Settings.THLocalDbDatabase);

                    if (string.IsNullOrWhiteSpace(connStr))
                    {
                        ProjectLogger.WriteWarning("[BuildCsvFromQrBankDb] Không có PG connStr → fallback SQLite.");
                        return BuildCsvFromSQLiteFallback(mode, quantity, qrNeeded, gtin);
                    }


                    string table = THDb.Code;
                    string lineId = Shared.Settings.LineId ?? string.Empty;

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();

                        Func<string, List<(int id, string qrCode, string batch, string lineId, string factoryCode)>> fetchQrList = (extraWhere) =>
                        {
                            var list = new List<(int id, string qrCode, string batch, string lineId, string factoryCode)>();
                            int limit = (mode == THTrueMilkOperatingMode.BatchOneQrCode || mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange) ? 1 : qrNeeded;
                            string sql = $"SELECT {Id}, {QrCode}, COALESCE({Batch},''), COALESCE({LineId},''), COALESCE({FactoryCode},'') " +
                                         $"FROM \"{table}\" " +
                                         $"WHERE {IsUsed} = FALSE {extraWhere} " +
                                         $"ORDER BY {Id} ASC LIMIT @lim";
                            using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                            {
                                if (extraWhere.Contains("@line_id"))
                                    cmd.Parameters.AddWithValue(LineId, lineId);
                                if (extraWhere.Contains("@gtin"))
                                    cmd.Parameters.AddWithValue("gtin", gtin);
                                cmd.Parameters.AddWithValue("lim", limit);
                                using (var reader = cmd.ExecuteReader())
                                    while (reader.Read())
                                        list.Add((reader.GetInt32(0), reader.GetString(1),
                                                  reader.GetString(2), reader.GetString(3), reader.GetString(4)));
                            }
                            return list;
                        };

                        List<(int id, string qrCode, string batch, string lineId, string factoryCode)> qrList = null;
                        bool hasGtin = !string.IsNullOrWhiteSpace(gtin);
                        bool hasLineId = !string.IsNullOrWhiteSpace(lineId);

                        if (hasGtin && hasLineId)
                        {
                            qrList = fetchQrList($"AND {LineId} = @line_id AND {ProductGtin} = @gtin");
                            if (qrList.Count == 0)
                                qrList = fetchQrList($"AND {ProductGtin} = @gtin");
                            if (qrList.Count == 0)
                                qrList = fetchQrList($"AND {LineId} = @line_id");
                            if (qrList.Count == 0)
                                qrList = fetchQrList("");
                        }
                        else if (hasGtin)
                        {
                            qrList = fetchQrList($"AND {ProductGtin} = @gtin");
                            if (qrList.Count == 0)
                                qrList = fetchQrList("");
                        }
                        else if (hasLineId)
                        {
                            qrList = fetchQrList($"AND {LineId} = @line_id");
                            if (qrList.Count == 0)
                                qrList = fetchQrList("");
                        }
                        else
                        {
                            qrList = fetchQrList("");
                        }

                        if (qrList == null || qrList.Count == 0)
                        {
                            ProjectLogger.WriteWarning("[BuildCsvFromQrBankDb] PG trả về 0 bản ghi → fallback SQLite.");
                            return BuildCsvFromSQLiteFallback(mode, quantity, qrNeeded, gtin);
                        }

                        return BuildCsvLines(mode, quantity, qrNeeded, qrList);
                    }
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[BuildCsvFromQrBankDb] PG lỗi → fallback SQLite: " + ex.Message, ex);
                    return BuildCsvFromSQLiteFallback(mode, quantity, qrNeeded, gtin);
                }
            });
        }

        private (List<string> csvLines, List<int> qrIds) BuildCsvFromSQLiteFallback(
     THTrueMilkOperatingMode mode, int quantity, int qrNeeded, string gtin = "")
        {
            ProjectLogger.WriteWarning("[BuildCsvFromQrBankDb] Dùng SQLite offline cache.");
            int limit = (mode == THTrueMilkOperatingMode.BatchOneQrCode || mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange) ? 1 : qrNeeded;

            var qrListSimple = RLinkLogService.FetchQrCodesFromSQLite(
                Shared.Settings.LineId ?? "", limit, gtin,
                Shared.Settings.THQrBaseUrl ?? "", Shared.Settings.THQrNumberOfUrl);

            if (qrListSimple == null || qrListSimple.Count == 0)
                return (new List<string>(), new List<int>());

            // ── Map (int id, string qrCode) → (int id, string qrCode, string batch, string lineId, string factoryCode) ──
            var qrListFull = qrListSimple
                .Select(x => (x.id, x.qrCode, batch: string.Empty,
                               lineId: Shared.Settings.LineId ?? string.Empty,
                               factoryCode: string.Empty))
                .ToList();

            return BuildCsvLines(mode, quantity, qrNeeded, qrListFull);
        }
        // Build file CSV gồm header + qr_code,batch,line_id,factory_code từ list QR lấy được.
        //  private static (List<string> csvLines, List<int> qrIds) BuildCsvLines(
        //THTrueMilkOperatingMode mode, int quantity, int qrNeeded,
        //List<(int id, string qrCode, string batch, string lineId, string factoryCode)> qrList)
        //  {
        //      var result = new List<string>();
        //      var fetchedIds = qrList.Select(x => x.id).ToList();

        //      // Header — dễ đọc khi mở bằng Excel/Notepad
        //      result.Add("qr_code,batch,line_id,factory_code");

        //      if (mode == THTrueMilkOperatingMode.BatchOneQrCode)
        //      {
        //          var item = qrList[0];
        //          for (int i = 0; i < quantity; i++)
        //              result.Add($"{item.qrCode},{item.batch},{item.lineId},{item.factoryCode}");
        //          ProjectLogger.WriteInfo($"[BuildCsvLines] Mode1 → QR: {item.qrCode} × {quantity}");
        //      }
        //      else
        //      {
        //          int repeatPerQr = (int)Math.Ceiling((double)quantity / qrList.Count);
        //          foreach (var item in qrList)
        //          {
        //              for (int i = 0; i < repeatPerQr && result.Count - 1 < quantity; i++)
        //                  result.Add($"{item.qrCode},{item.batch},{item.lineId},{item.factoryCode}");
        //              if (result.Count - 1 >= quantity) break;
        //          }
        //          var last = qrList[qrList.Count - 1];
        //          while (result.Count - 1 < quantity)
        //              result.Add($"{last.qrCode},{last.batch},{last.lineId},{last.factoryCode}");
        //      }
        //      return (result, fetchedIds);
        //  }
        private static (List<string> csvLines, List<int> qrIds) BuildCsvLines(
    THTrueMilkOperatingMode mode, int quantity, int qrNeeded,
    List<(int id, string qrCode, string batch, string lineId, string factoryCode)> qrList)
        {
            var result = new List<string>();
            var fetchedIds = qrList.Select(x => x.id).ToList();

            if (mode == THTrueMilkOperatingMode.BatchOneQrCode ||
                mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
            {
                var item = qrList[0];
                for (int i = 0; i < quantity; i++)
                    result.Add(item.qrCode);
                ProjectLogger.WriteInfo($"[BuildCsvLines] Mode1/4 → QR: {item.qrCode} × {quantity}");
            }
            else if (mode == THTrueMilkOperatingMode.ProductOneQrCode)
            {
                foreach (var item in qrList)
                {
                    if (result.Count >= quantity) break;
                    result.Add(item.qrCode);
                }
                ProjectLogger.WriteInfo($"[BuildCsvLines] Mode3 → {result.Count} QR unique");
            }
            else
            {
                int repeatPerQr = (int)Math.Ceiling((double)quantity / qrList.Count);
                foreach (var item in qrList)
                {
                    for (int i = 0; i < repeatPerQr && result.Count < quantity; i++)
                        result.Add(item.qrCode);
                    if (result.Count >= quantity) break;
                }
                var last = qrList[qrList.Count - 1];
                while (result.Count < quantity)
                    result.Add(last.qrCode);
            }
            return (result, fetchedIds);
        }
        private void SetComboBoxCellIndex(DataGridView dgv, int rowIndex, string columnName, int selectedIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgv.Rows.Count)
                return;

            var cell = dgv.Rows[rowIndex].Cells[columnName] as DataGridViewComboBoxCell;
            if (cell == null || cell.Items.Count == 0)
                return;

            if (selectedIndex >= 0 && selectedIndex < cell.Items.Count)
            {
                cell.Value = cell.Items[selectedIndex];
            }
        }

        public void CSVDataBaseClick()
        {
            txtDirectoryDatabse.Text = _JobModel.DirectoryDatabase = OpenDirectoryFileDatabase();
            _PODFormat.Clear();
            txtPODFormat.Text = "";
        }

        private void RestartApplication()
        {
            try
            {
                //CuzMessageBox.Show()
                DialogResult dialogResult = CuzMessageBox.Show(Lang.DoYouWantToRestartTheApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    string applicationPath = Process.GetCurrentProcess().MainModule?.FileName;
                    string local = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                    string helperPath = Path.Combine(local, "RestartProcessHelper.exe");

                    if (!File.Exists(helperPath))
                    {
                        return;
                    }

                    Process.Start(new ProcessStartInfo  // Start the helper process to restart the application
                    {
                        FileName = helperPath,
                        Arguments = $"{Process.GetCurrentProcess().Id} \"{applicationPath}\"",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false
                    });
                    Thread.Sleep(500); // avoid app exit so fast will terminate process above
                    Application.Exit();
                }

            }
            catch (Exception)
            {
                // Optionally log the exception or notify the user
            }
        }

        public static void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.RadioButton radioButton)
            {
                if (radioButton.Enabled)
                {
                    radioButton.BackColor = radioButton.Checked ? Color.FromArgb(0, 170, 230) : Color.White;
                }
            }
        }
        private void CboSupportForCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cbbSupportCam = (System.Windows.Controls.ComboBox)sender;
            switch (cbbSupportCam.SelectedIndex)
            {
                case 0: // DM Series

                    break;
                case 1: // IS2800 Series

                    break;
                default:
                    break;
            }
        }
        private void BtnViewLog_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = CommVariables.PathProgramDataApp;
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|Job files (*.rvis)|*.rvis|Database files (*.db)|*.db|csv files (*.csv)|*.csv|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 5;
                    openFileDialog.Multiselect = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFile = openFileDialog.FileName;
                        Process.Start("notepad.exe", selectedFile);
                    }
                }
            }
            catch (Exception) { }

        }
        private void ListBoxJobList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1 || (sender as System.Windows.Forms.ListBox).Items.Count == 0) return;
            try
            {
                JobModel job = Shared.GetJob((sender as System.Windows.Forms.ListBox).Items[e.Index].ToString());
                Rectangle headItemRect = new Rectangle(0, e.Bounds.Y + 4, 8, e.Bounds.Height - 10);
                using (Brush brush = new SolidBrush(_Standalone))
                    if (!job.PrinterSeries)
                        e.Graphics.FillRectangle(brush, headItemRect);
            }
            catch
            {
            }
        }
        private async void BtnRefreshProgramsFromCameraKeyence_Click(object sender, EventArgs e)
        {
            btnRefreshListProgramFroCameraKeyence.Enabled = false;
            try
            {
                await RefreshCameraProgramsAsync();
            }
            catch (Exception ex)
            {
                CuzMessageBox.Show($"Lỗi tải program: {ex.Message}", "Error");
            }
            finally
            {
                btnRefreshListProgramFroCameraKeyence.Enabled = true;
            }
        }

        //private async Task RefreshCameraProgramsAsync()
        //{
        //    var programs = await Shared.vscCamera.GetProgramListAsync();
        //    cbcSelectedModel.Items.Clear();
        //    foreach (var p in programs)
        //        cbcSelectedModel.Items.Add(p);
        //    if (cbcSelectedModel.Items.Count > 0)
        //        cbcSelectedModel.SelectedIndex = 0;
        //}
        private async Task RefreshCameraProgramsAsync()
        {
            // Guard: Check camera connection
            if (Shared.vscCamera == null || !Shared.vscCamera.IsConnected())
            {
                Shared.Settings.CachedCameraPrograms = new List<string>();
                cbcSelectedModel.Items.Clear();
                return;
            }

            var programs = await Shared.vscCamera.GetProgramListAsync();

            // Lưu vào CachedCameraPrograms
            Shared.Settings.CachedCameraPrograms = programs
                .Select(p => $"{p.ProgramNo:D4}_{p.Name}").ToList();
            Shared.SaveSettings();

            cbcSelectedModel.Items.Clear();
            foreach (var p in programs)
                cbcSelectedModel.Items.Add(p);
            if (cbcSelectedModel.Items.Count > 0)
                cbcSelectedModel.SelectedIndex = 0;
        }
        private async Task RunSafe(string name, Task task, List<(string name, bool ok, string error)> results)
        {
            try
            {
                await task;
                results.Add((name, true, ""));
            }
            catch (Exception ex)
            {
                results.Add((name, false, ex.Message));
            }
        }
        private void JobType_EnabledChanged(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.RadioButton radioButton)
            {
                if (!radioButton.Enabled)
                {
                    radioButton.BackColor = Color.WhiteSmoke;
                }
                else
                {
                    if (radioButton.Checked)
                    {
                        radioButton.BackColor = Color.FromArgb(0, 171, 230);
                    }
                    else
                    {
                        radioButton.BackColor = Color.White;
                    }
                }
            }
        }
        private void TimerDateTime_Tick(object sender, EventArgs e)
        {
            toolStripDateTime.Text = DateTime.Now.ToString(_DateTimeFormat);

            try
            {
                string driveLetter = CommVariables.PathProgramDataApp.Substring(0, 1);
                var drive = new System.IO.DriveInfo(driveLetter);
                if (drive.IsReady)
                {
                    double freePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100.0;
                    toolStripStatusDiskStorage.Text = string.Format("{0}: {1:F1}%", driveLetter, freePercent);
                    toolStripStatusDiskStorage.ForeColor = freePercent < 10.0 ? Color.Red
                                                         : freePercent < 20.0 ? Color.Orange
                                                         : Color.Black;
                }
            }
            catch { }

            // ── Tự cập nhật filter ngày khi qua ngày mới ──
            if (DateTime.Today != _lastDateForFilter)
            {
                _lastDateForFilter = DateTime.Today;
                dtpFromDate.Value = DateTime.Now.AddDays(-7);
                dtpToDate.Value = DateTime.Now;
                dtpFromDateQr.Value = DateTime.Now.AddDays(-7);
                dtpToDateQr.Value = DateTime.Now;
                dtpFromDateQrRecive.Value = DateTime.Now.AddDays(-7);
                dtpToDateQrRecive.Value = DateTime.Now;
                if (tabControl1.SelectedTab == tabQrIsUsed)
                    _ = LoadQrIsUsedAsync(txtSearchQr.Text, dtpFromDateQr.Value, dtpToDateQr.Value);
                if (tabControl1.SelectedTab == tabHistoryReciveQr)
                    _ = LoadHistoryReceiveQrAsync(txtSearchHistoryReceiveQr.Text, dtpFromDateQrRecive.Value, dtpToDateQrRecive.Value);
            }
        }
        private void TxtPODFormat_TextChanged(object sender, EventArgs e)
        {
            if (_JobModel != null && radDatabase.Checked)
            {
                _JobModel.PODFormat = _PODFormat;
            }
        }
        private void TxtDirectoryDatabse_TextChanged(object sender, EventArgs e)
        {
            if (_suppressTextChangedSync) return;
            if (_JobModel != null && !string.IsNullOrEmpty(txtDirectoryDatabse.Text))
                _JobModel.DirectoryDatabase = txtDirectoryDatabse.Text;
        }
        private void TxtStaticText_TextChanged(object sender, EventArgs e)
        {
            if (_JobModel != null && radStaticText.Checked)
            {
                _JobModel.StaticText = txtStaticText.Text;
            }
        }
        private bool _suppressTextChangedSync;

        private void TxtFileName_TextChanged(object sender, EventArgs e)
        {
            if (_suppressTextChangedSync) return;
            if (_JobModel != null && !string.IsNullOrEmpty(txtFileName.Text))  
                _JobModel.FileName = txtFileName.Text;
        }

       
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyWord = txtSearch.Text.ToLower();
            if (_JobNameList != null)
            {
                listBoxJobList.Items.Clear();
                foreach (string templateName in _JobNameList)
                {
                    if (templateName.ToLower().Contains(keyWord))
                    {
                        JobModel jobModel = Shared.GetJob(templateName);
                        if (jobModel != null && jobModel.JobStatus != JobStatus.Deleted)
                            listBoxJobList.Items.Add(templateName);
                    }
                }
            }
        }
        private void BtnClose_Click(object sender, EventArgs e)
        {
            Exit();
        }
        private void FrmJob_Load(object sender, EventArgs e)
        {
            LoadJobNameList();
            radRSeries.Checked = _JobModel.PrinterSeries;
            radOther.Checked = !_JobModel.PrinterSeries;
            if (_JobModel.JobType == JobType.AfterProduction)
                radAfterProduction.Checked = true;
            else if (_JobModel.JobType == JobType.OnProduction)
                radOnProduction.Checked = true;
            else
                radVerifyAndPrint.Checked = true;
            EnableUIPrinting();
            _LabelStatusCameraList.Add(lblStatusCamera01);
            UpdateStatusLabelCamera();
            _LabelStatusPrinterList.Add(lblStatusPrinter01);
            UpdateStatusLabelPrinter();
            UpdateStatusLabelDatabase();
            EnableUIPrinting();
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
            tblJobType.Enabled = false;
            UpdateUIListBoxPrintProductTemplateList(_PrintProductTemplateList);
            pnlStandaloneColor.BackColor = _Standalone;
            pnlRLinkSeriesColor.BackColor = _RLinkColor;

            // Load cài đặt từ R-Link Master vào các textbox trên tabGetPO
            LoadRLinkSettingsToUI();
        }
        public async void RefreshAfterLogin()
        {
            ApplyPermissions();
            LoadRLinkSettingsToUI();
            UpdateQrConfigLabels();
            if (!string.IsNullOrEmpty(Shared.Settings.LineId))
            {
                await Task.WhenAll(
                    AutoLoadAccountsAsync(),
                    AutoLoadSettingsAsync(),
                    AutoLoadProductsAsync(forceRefreshImages: true),
                    LoadQrConfigAsync()
                );

                // Preload tất cả ảnh từ disk vào RAM cache → offline luôn có hình
                ProductImageHelper.PreloadAllImages();
            }

        }

        private bool TryAutoReopenLastActiveJob()
        {
            try
            {
                // ── Không mở lại job nếu user không có quyền tạo/chạy job ──
                if (Shared.UserPermission == null || !Shared.UserPermission.CreateJob)
                {
                    ProjectLogger.WriteInfo("[TryAutoReopenLastActiveJob] Bỏ qua — user không có quyền createJob.");
                    return false;
                }

                string lastJob = Shared.Settings.LastActiveJobName;
                if (string.IsNullOrWhiteSpace(lastJob)) return false;

                JobModel job = Shared.GetJob(lastJob);
                if (job == null) return false;

                if (job.CompleteJobStatus == CompleteJobStatus.Completed) return false;
                if (job.JobStatus == JobStatus.Deleted) return false;

                _JobModel = job;
                Shared.JobNameSelected = lastJob;

                Hide();
                if (_FormMainPC != null && !_FormMainPC.IsDisposed)
                    _FormMainPC.ForceClose();
                _FormMainPC?.Dispose();
                // singleton keeps running — frmMain will SetSnapshotFactory
                _FormMainPC = new frmMainTHTrueMilk(this);
#if DEBUG
                _FormMainPC.DebugNsxHsdTime = _debugNsxHsdTime;
#endif
                _FormMainPC.Show();
                _FormMainPC.UpdateJobInfoDisplay(_JobModel);

                // ── Tự động đổi program camera khi mở lại job cũ ──
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000); // chờ form load xong
                    await _FormMainPC.ChangeCameraProgramFromJobAsync(job);
                });

                ProjectLogger.WriteInfo($"[TryAutoReopenLastActiveJob] Tự động mở lại job: {job.FileName}");
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[TryAutoReopenLastActiveJob] Lỗi: " + ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Tự động gọi API lấy danh sách sản phẩm, lưu vào Shared.Settings.THProductList
        /// và hiển thị lên dgvItems.
        /// Chỉ thực hiện khi đã login và LineId != empty.
        /// </summary>
        private async Task AutoLoadProductsAsync(bool forceRefreshImages = false)
        {
            try
            {
                List<BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models.ProductItem> products = null;

                // Thử lấy từ R-Link API trước
                try
                {
                    var service = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory.Instance;
                    products = await service.GetProductsAsync();

                    if (products != null && products.Count > 0)
                        // DB writes chạy background — không block UI
                        _ = Task.Run(async () =>
                        {
                            try { await BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.LocalAccountStore.SyncProductsAsync(products); }
                            catch (Exception ex) { Debug.WriteLine($"[AutoLoadProducts] DB sync error: {ex.Message}"); }
                        });
                }
                catch { }

                // Fallback: load từ PostgreSQL
                if (products == null || products.Count == 0)
                    products = await BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.LocalAccountStore.LoadProductsFromDbAsync();

                // Gán cache path cho ảnh local nếu có + preload vào RAM
                if (products != null)
                {
                    ProductImageHelper.PreloadImagesToCache(products);
                }

                if (products == null || products.Count == 0)
                {
                    ProjectLogger.WriteWarning("[AutoLoadProducts] Không có sản phẩm nào từ API lẫn DB.");
                    return;
                }

                // Lưu lại productId đang chọn trước khi reload
                string currentProductId = GetSelectedProduct()?.ProductId;

                Shared.Settings.THProductList = products;
                ProjectLogger.WriteInfo($"[AutoLoadProducts] Đã load {products.Count} sản phẩm.");

                // Hiển thị danh sách SP NGAY LẬP TỨC (dùng ảnh cache có sẵn)
                LoadProductListToGrid();
                LoadProductToComboBox();

                // Restore selected product sau khi reload list
                if (!string.IsNullOrEmpty(currentProductId))
                {
                    int newIndex = products.FindIndex(p => p.ProductId == currentProductId);
                    if (newIndex >= 0 && newIndex < cbcSelectProduct.Items.Count)
                        cbcSelectProduct.SelectedIndex = newIndex;
                }

                // Download ảnh mới trong nền (KHÔNG block UI)
                try
                {
                    var _dbgLog = Path.Combine(CommVariables.PathProductImages, "_debug.log");
                    File.AppendAllText(_dbgLog, $"[{DateTime.Now:HH:mm:ss}] START forceRefresh={forceRefreshImages}, products={products?.Count}, lineId='{Shared.Settings.LineId}'\n");
                    foreach (var p in products)
                        File.AppendAllText(_dbgLog, $"[{DateTime.Now:HH:mm:ss}] PRODUCT '{p.ProductId}': Image='{p.Image}'\n");
                }
                catch { }
                _ = Task.Run(async () =>
                {
                    try
                    {
                        int count = await ProductImageHelper.DownloadProductImagesAsync(
                            products,
                            forceRefresh: forceRefreshImages);
                        Debug.WriteLine($"[AutoLoadProducts] Background downloaded {count} images.");

                        // Sau khi download xong → cập nhật UI cho sản phẩm đang chọn
                        this.BeginInvoke(new Action(() =>
                        {
                            var selected = GetSelectedProduct();
                            if (selected != null)
                                LoadProductImage(selected.Image, selected.ProductId);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[AutoLoadProducts] Background download error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[AutoLoadProducts] Lỗi: " + ex.Message, ex);
            }
        }
        /// <summary>
        /// Load thông tin thiết đặt từ R-Link Master (đã lưu trong Shared.Settings) lên các textbox trên tabGetPO.
        /// Gọi sau khi đăng nhập thành công hoặc khi form load.
        /// </summary>
        private void LoadRLinkSettingsToUI()
        {
            if (InvokeRequired) { Invoke(new Action(LoadRLinkSettingsToUI)); return; }
            try
            {
                var s = Shared.Settings;

                txtModeOperator.Text = s.THOperatingMode.ToDisplayString();
                lblModeOperator.Text = "Chế độ in: " + (int)s.THOperatingMode;

                bool isMode1 = s.THOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode;
                bool isMode4 = s.THOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange;
                bool isMode2 = s.THOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;

                if (isMode4)
                {
                    // Mode 4: ẩn txtDetalForMode1, hiển thị txtCoefficient
                    label1.Text = "Hệ số dự phòng";
                    txtDetalForMode1.Visible = false;
                    txtCoefficient.Visible = true;
                    txtCoefficient.Text = s.THReserveFactor.ToString();
                    label1.Visible = true;
                }
                else
                {
                    // Mode 1/2/3: hiển thị txtDetalForMode1 như cũ
                    label1.Text = isMode2 ? "Chu kì đổi QR Code (Phút)" : "Làm mới QR trước 12h đêm";
                    txtDetalForMode1.Text = isMode2
                        ? s.THNMinutes + " phút"
                        : s.THDeltaMinutes + " giây";
                    txtDetalForMode1.Visible = true;
                    txtCoefficient.Visible = false;
                    label1.Visible = true;
                }
                // Ẩn hẳn dòng cũ của txtTimeForMode2 + label16
                txtTimeForMode2.Visible = false;
                txtBufferPrint.Text = s.THBufferCount + " mã";
                txtTimeMornitor.Text = s.THMonitorInterval + " phút";
                txtTimeLogSave.Text = s.THLogInterval + " phút";
                txtQrThreshold.Text = s.THQrThreshold + " %";
                txtPathErrorImage.Text = s.THErrorImageFolder?.ToString() ?? "";
                txtMaxConsecutiveDefects.Text = s.THMaxConsecutiveError.ToString();
            
                LoadProductListToGrid();
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[LoadRLinkSettingsToUI] Lỗi: " + ex.Message, ex);
            }
        }









        // ============================================================
        // COLLAPSE PANEL - WinForms C#
        // Gắn vào nút "—" góc trên phải của panel cài đặt
        // ============================================================

        // Khai báo biến
        private bool _isCollapsed = false;
        private int _expandedHeight;      // Lưu chiều cao gốc
        private System.Windows.Forms.Timer _animTimer;         // Timer animate
        private int _cuzPanel1OriginalTop;
        private int _cuzPanel2OriginalTop;
        private int _cuzPanel2OriginalHeight;
        private int _dgvItemsOriginalHeight;

        // Gọi hàm này trong Form_Load
        private void InitCollapsePanel()
        {
            // Lưu chiều cao gốc của panel
            _expandedHeight = cuzPanel5.Height;

            // Lưu vị trí & kích thước gốc (null-safe)
            if (cuzPanel1 != null) _cuzPanel1OriginalTop = cuzPanel1.Top;
            if (cuzPanel2 != null)
            {
                _cuzPanel2OriginalTop = cuzPanel2.Top;
                _cuzPanel2OriginalHeight = cuzPanel2.Height;
            }
            if (dgvItems != null) _dgvItemsOriginalHeight = dgvItems.Height;

            // ── Ẩn cuzPanel5 mặc định khi mở form ──
            _isCollapsed = true;
            if (cuzPanel5 != null)
            {
                cuzPanel5.Visible = false;

                int btnH = btnGetInfo?.Height ?? 76;
                if (cuzPanel1 != null)
                    cuzPanel1.Top = 8;

                if (cuzPanel2 != null && cuzPanel1 != null)
                {
                    cuzPanel2.Top = cuzPanel1.Bottom + 6;
                    int btnTop = tabGetPO.ClientSize.Height - btnH - 10;
                    cuzPanel2.Height = Math.Max(50, btnTop - cuzPanel2.Top - 6);
                }
                if (dgvItems != null && cuzPanel2 != null)
                    dgvItems.Height = cuzPanel2.Height - 65;
            }

            // Khởi tạo timer animate
            _animTimer = new System.Windows.Forms.Timer();
            _animTimer.Interval = 8; // ms
            _animTimer.Tick += AnimTimer_Tick;

            // Gán sự kiện click nút —/+
            button2.Click += BtnCollapse_Click;

            // ── colSetting: toggle hiển thị/ẩn cuzPanel5 ──
            if (colSetting != null)
                colSetting.Click += ColSetting_Click;
        }

        /// <summary>
        /// Xử lý click colSetting (PictureBox) để toggle hiển thị/ẩn cuzPanel5.
        /// </summary>
        private void ColSetting_Click(object sender, EventArgs e)
        {
            if (cuzPanel5 == null) return;

            int padding = 8;
            int panelGap = 6;
            int buttonHeight = btnGetInfo?.Height ?? 76;
            int bottomMargin = 10;
            int tabH = tabGetPO.ClientSize.Height;
            int tabW = tabGetPO.ClientSize.Width;
            int fullWidth = Math.Max(100, tabW - 2 * padding);

            // Tính vị trí buttons trước — LUÔN neo bottom
            int btnTop = tabH - buttonHeight - bottomMargin;

            // Đặt buttons TRƯỚC để vì btnTop là ground truth
            if (btnGetInfo != null)
                btnGetInfo.Location = new Point(padding, btnTop);
            if (saveJobTH != null)
                saveJobTH.Location = new Point(tabW - saveJobTH.Width - padding, btnTop);

            if (cuzPanel5.Visible)
            {
                // ── Ẩn cuzPanel5 ──
                _isCollapsed = true;
                cuzPanel5.Visible = false;

                // cuzPanel1 lên đầu
                if (cuzPanel1 != null)
                {
                    cuzPanel1.Location = new Point(padding, padding);
                    cuzPanel1.Width = fullWidth;
                }

                // cuzPanel2 chiếm phần còn lại
                if (cuzPanel2 != null && cuzPanel1 != null)
                {
                    cuzPanel2.Location = new Point(padding, cuzPanel1.Bottom + panelGap);
                    cuzPanel2.Width = fullWidth;
                    int maxHeight = btnTop - cuzPanel2.Top - panelGap;
                    cuzPanel2.Height = Math.Max(50, maxHeight);
                }
                if (dgvItems != null && cuzPanel2 != null)
                    dgvItems.Height = cuzPanel2.Height - 65;
            }
            else
            {
                // ── Hiện cuzPanel5 ──
                _isCollapsed = false;
                cuzPanel5.Visible = true;
                cuzPanel5.Location = new Point(padding, padding);
                cuzPanel5.Width = fullWidth;
                cuzPanel5.Height = _expandedHeight;
                cuzPanel5.Controls.Cast<Control>().ToList()
                    .ForEach(c => c.Visible = true);

                button2.Visible = false;
                button2.Text = "—";

                // cuzPanel1 dưới cuzPanel5
                if (cuzPanel1 != null)
                {
                    cuzPanel1.Location = new Point(padding, cuzPanel5.Bottom + panelGap);
                    cuzPanel1.Width = fullWidth;
                }

                // cuzPanel2 chiếm phần còn lại — tính từ btnTop, KHÔNG dùng _original
                if (cuzPanel2 != null && cuzPanel1 != null)
                {
                    cuzPanel2.Location = new Point(padding, cuzPanel1.Bottom + panelGap);
                    cuzPanel2.Width = fullWidth;
                    int maxHeight = btnTop - cuzPanel2.Top - panelGap;
                    cuzPanel2.Height = Math.Max(50, maxHeight);
                }
            }

            tabGetPO.PerformLayout();
            tabGetPO.Invalidate();
        }

        // Xử lý click nút —/+
        private void BtnCollapse_Click(object sender, EventArgs e)
        {
            
                _animTimer.Start();
            
        }

        // Animate thu/mở panel
        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            int step = 15;
            int collapsedHeight = 30;

            if (_isCollapsed)
            {
                // ── Expand ──
                cuzPanel5.Height = Math.Min(_expandedHeight, cuzPanel5.Height + step);

                if (cuzPanel5.Height >= _expandedHeight)
                {
                    cuzPanel5.Height = _expandedHeight;
                    cuzPanel5.Controls.Cast<Control>().ToList()
                        .ForEach(c => c.Visible = true);
                    button2.Text = "—";
                    _isCollapsed = false;
                    _animTimer.Stop();

                    // Phục hồi vị trí & kích thước các panel bên dưới
                    if (cuzPanel1 != null) cuzPanel1.Top = _cuzPanel1OriginalTop;
                    if (cuzPanel2 != null)
                    {
                        cuzPanel2.Top = _cuzPanel2OriginalTop;
                        cuzPanel2.Height = _cuzPanel2OriginalHeight;
                    }
                    if (dgvItems != null) dgvItems.Height = _dgvItemsOriginalHeight;
                }
            }
            else
            {
                // ── Collapse ──
                // Ẩn control bên trong (trừ nút collapse, icon và title)
                cuzPanel5.Controls.Cast<Control>().ToList()
                    .ForEach(c => { if (c != button2 && c != pictureBox2 && c != label38) c.Visible = false; });

                cuzPanel5.Height = Math.Max(collapsedHeight, cuzPanel5.Height - step);

                if (cuzPanel5.Height <= collapsedHeight)
                {
                    cuzPanel5.Height = collapsedHeight+ 7;
                    button2.Text = "+";
                    _isCollapsed = true;
                    _animTimer.Stop();

                    // Kéo các panel bên dưới lên + mở rộng dgvItems
                    int targetTop = cuzPanel5.Bottom + 5;
                    if (cuzPanel1 != null) cuzPanel1.Top = targetTop;
                    if (cuzPanel2 != null && cuzPanel1 != null)
                    {
                        cuzPanel2.Top = cuzPanel1.Bottom + 5;
                        int btnH = btnGetInfo?.Height ?? 76;
                        int btnTop = tabGetPO.ClientSize.Height - btnH - 10;
                        cuzPanel2.Height = Math.Max(50, btnTop - cuzPanel2.Top - 5);
                    }
                    if (dgvItems != null && cuzPanel2 != null) dgvItems.Height = cuzPanel2.Height - 65;
                }
            }
        }

        // ============================================================
        // HƯỚNG DẪN SETUP TRONG DESIGNER:
        // ============================================================
        //
        // 1. Panel cài đặt đặt tên: panelSetting
        //    - Chứa toàn bộ các TextBox, Label cài đặt bên trong
        //
        // 2. Nút —/+ đặt tên: btnCollapse
        //    - Đặt góc trên phải của panelSetting
        //    - Text = "—"
        //    - Size = (25, 25)
        //    - FlatStyle = Flat
        //    - Anchor = Top, Right
        //
        // 3. Gọi InitCollapsePanel() trong Form_Load:
        //    private void Form1_Load(object sender, EventArgs e)
        //    {
        //        InitCollapsePanel();
        //    }
        //
        // ============================================================
        // NẾU MUỐN GIỮ TIÊU ĐỀ KHI THU GỌN:
        // ============================================================
        //
        // Thêm 1 Label tiêu đề trong panelSetting:
        //    lblSettingTitle.Text = "Cài đặt"
        //    lblSettingTitle.Dock = DockStyle.Top
        //    lblSettingTitle.Height = 30
        //
        // Rồi chỉ ẩn các control KHÁC, không ẩn lblSettingTitle:
        //    panelSetting.Controls.Cast<Control>()
        //        .Where(c => c != lblSettingTitle && c != btnCollapse)
        //        .ToList()
        //        .ForEach(c => c.Visible = false);
        // ============================================================







        /// <summary>
        /// Hiển thị danh sách sản phẩm (từ R-Link Master) lên dgvItems trên tabGetPO.
        /// Gọi sau khi đăng nhập thành công hoặc khi form load.
        /// </summary>
        private void LoadProductListToGrid(string filter = null)
        {
            if (InvokeRequired) { Invoke(new Action(() => LoadProductListToGrid(filter))); return; }
            try
            {
                var products = Shared.Settings.THProductList;
                if (products == null || products.Count == 0)
                {
                    ProjectLogger.WriteWarning("[LoadProductListToGrid] THProductList rỗng.");
                    return;
                }

                // Lọc theo từ khóa nếu có
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string kw = filter.Trim().ToLower();
                    products = products.Where(p =>
                        (p.ProductName?.ToLower().Contains(kw) == true) ||
                        (p.ProductName?.ToLower().Contains(kw) == true)
                    ).ToList();
                }

                dgvItems.Columns.Clear();
                dgvItems.Columns.Add(ProductId, "Mã sản phẩm");
                dgvItems.Columns.Add(ProductName, "Tên sản phẩm");
                dgvItems.Columns.Add("Volume", "Dung tích");
                dgvItems.Columns.Add("Exp", "Hạn sử dụng");
                dgvItems.ScrollBars = ScrollBars.Both;
                dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvItems.MultiSelect = false;
                dgvItems.ReadOnly = true;
                dgvItems.AllowUserToAddRows = false;
                dgvItems.RowTemplate.Height = 40;

                dgvItems.Rows.Clear();
                foreach (var p in products)
                    dgvItems.Rows.Add(p.ProductId, p.ProductName, p.Volume?.ToString() ?? "", p.Exp?.ToString() ?? "");

                foreach (DataGridViewColumn col in dgvItems.Columns)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                dgvItems.Columns[ProductName].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvItems.Columns[ProductName].MinimumWidth = 100;

                ProjectLogger.WriteInfo($"[LoadProductListToGrid] Loaded {products.Count} sản phẩm.");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[LoadProductListToGrid] Lỗi: " + ex.Message, ex);
            }
        }

        private void LoadProductToComboBox()
        {
            if (InvokeRequired) { Invoke(new Action(LoadProductToComboBox)); return; }
            // Tắt event để tránh trigger SelectedIndexChanged khi rebuild
            cbcSelectProduct.SelectedIndexChanged -= CbcSelectProduct_SelectedIndexChanged;
            try
            {
                var products = Shared.Settings.THProductList;
                cbcSelectProduct.Items.Clear();
                ClearProductInfo();
                if (products == null || products.Count == 0) return;

                foreach (var p in products)
                    cbcSelectProduct.Items.Add(
                        string.IsNullOrEmpty(p.ProductGtin)
                            ? p.ProductName ?? ""
                            : $"{p.ProductName} ({p.ProductGtin})");
               // cbcSelectProduct.Items.Add(p.ProductName ?? "");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[LoadProductToComboBox] Lỗi: " + ex.Message, ex);
            }
            finally
            {
                cbcSelectProduct.SelectedIndexChanged += CbcSelectProduct_SelectedIndexChanged;
            }
        }

        private void ClearProductInfo()
        {
            cbcSelectProduct.SelectedIndex = -1;
            cbcSelectProduct.Text = "Chọn sản phẩm";
            lblProductId.Text = "";
            lblProductName.Text = "";
            rtbGtinProduct.Clear();
            lblProductVolume.Text = "";
            lblProductExp.Text = "";
            picProductImage.Image = null;
            lblProgramCamera.Text = "(-)";
            lblTemplatePrinter.Text = "(-)";
            _matchedCameraProgram = "";
            _matchedPrinterTemplate = "";
        }

        private void SetGtinProductText(string gtin)
        {
            rtbGtinProduct.Clear();
            rtbGtinProduct.AppendText(gtin);

            if (!string.IsNullOrEmpty(gtin))
            {
                var boldFont = new Font(rtbGtinProduct.Font, FontStyle.Bold);
                rtbGtinProduct.Select(0, gtin.Length);
                rtbGtinProduct.SelectionFont = boldFont;

                int last5Len = Math.Min(5, gtin.Length);
                string last5 = gtin.Substring(gtin.Length - last5Len);
                int idx5 = gtin.LastIndexOf(last5, StringComparison.Ordinal);
                if (idx5 >= 0)
                {
                    rtbGtinProduct.Select(idx5, last5Len);
                    rtbGtinProduct.SelectionFont = boldFont;
                    rtbGtinProduct.SelectionColor = Color.Red;
                }
                rtbGtinProduct.Select(0, 0);
            }
        }

        private BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models.ProductItem GetSelectedProduct()
        {
            var products = Shared.Settings.THProductList;
            if (products == null || cbcSelectProduct.SelectedIndex < 0 || cbcSelectProduct.SelectedIndex >= products.Count)
                return null;
            return products[cbcSelectProduct.SelectedIndex];
        }

        private async Task LoadQrConfigAsync()
        {
            try
            {
                var service = RLinkMasterServiceFactory.Instance;
                var qrConfig = await service.GetQrConfigAsync();
                if (qrConfig != null)
                {
                    Shared.Settings.THQrBaseUrl = qrConfig.BaseUrl ?? "";
                    Shared.Settings.THQrNumberOfUrl = qrConfig.NumberOfUrl;
                    Shared.SaveSettings();
                    ProjectLogger.WriteInfo($"[QrConfig] Updated: baseUrl={qrConfig.BaseUrl}, numberOfUrl={qrConfig.NumberOfUrl}");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteWarning($"[QrConfig] Không lấy được từ API: {ex.Message}");
            }

            UpdateQrConfigLabels();
        }

        private void UpdateQrConfigLabels()
        {
            if (InvokeRequired) { Invoke(new Action(UpdateQrConfigLabels)); return; }
            try
            {
                lblBaseUrl.Text = !string.IsNullOrWhiteSpace(Shared.Settings.THQrBaseUrl)
                    ? Shared.Settings.THQrBaseUrl : "-";
                lblCharTotal.Text = Shared.Settings.THQrNumberOfUrl > 0
                    ? Shared.Settings.THQrNumberOfUrl.ToString() : "-";
            }
            catch { }
        }

        private async void CbcSelectProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbcSelectProduct.SelectedIndex < 0)
                {
                    ClearProductInfo();
                    return;
                }

                var products = Shared.Settings.THProductList;
                if (products == null || cbcSelectProduct.SelectedIndex >= products.Count) return;

                var p = products[cbcSelectProduct.SelectedIndex];
                if (p == null) return;

                lblProductId.Text = p.ProductId ?? "";
                lblProductName.Text = p.ProductName ?? "";
                SetGtinProductText(p.ProductGtin ?? "");
                lblProductVolume.Text = p.Volume?.ToString() + " ml" ?? "";
                // lblProductExp.Text = (p.Exp > 0 ? (p.Exp.Value - 1).ToString() : p.Exp?.ToString()) + " Ngày" ?? "";
                lblProductExp.Text = p.Exp?.ToString() + " Ngày" ?? "";

                // ── Match product_gtin với Camera Programs & Printer Templates ──
                string productGtin = p.ProductGtin ?? "";
                string cameraMatch = "(-)";
                string printerMatch = "(-)";

                if (!string.IsNullOrWhiteSpace(productGtin))
                {
                    var camPrograms = Shared.Settings.CachedCameraPrograms ?? new List<string>();
                    string matchedCam = camPrograms.FirstOrDefault(x => x.Split('_').Last() == productGtin);
                    if (matchedCam != null) cameraMatch = matchedCam;

                    var printerTpls = Shared.Settings.CachedPrinterTemplates ?? new List<string>();
                    if (printerTpls.Contains(productGtin)) printerMatch = productGtin;
                }

                lblProgramCamera.Text = cameraMatch;
                lblTemplatePrinter.Text = printerMatch;
                _matchedCameraProgram = cameraMatch;
                _matchedPrinterTemplate = printerMatch;

                // ── Tự đổi camera program nếu tìm thấy ──
                if (cameraMatch != "(-)" && Shared.vscCamera != null && Shared.vscCamera.IsConnected())
                {
                    string camModel = cameraMatch;
                    if (camModel.Length >= 4 && int.TryParse(camModel.Substring(0, 4), out int progNo) && progNo >= 0)
                    {
                        try
                        {
                            var (success, message) = await Shared.vscCamera.ChangeProgramAsync(progNo);
                            if (success)
                                ProjectLogger.WriteInfo($"[Camera] {message} khi chọn sản phẩm '{p.ProductName}'");
                            else
                                ProjectLogger.WriteWarning($"[Camera] {message} khi chọn sản phẩm '{p.ProductName}'");
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteWarning($"[Camera] Lỗi đổi program khi chọn sản phẩm: {ex.Message}");
                        }
                    }
                }

                for (int i = 0; i < dgvItems.Rows.Count; i++)
                {
                    if (dgvItems.Rows[i].Cells[ProductId].Value?.ToString() == p.ProductId)
                    {
                        dgvItems.ClearSelection();
                        dgvItems.Rows[i].Selected = true;
                        dgvItems.FirstDisplayedScrollingRowIndex = i;
                        break;
                    }
                }

                LoadProductImage(p.Image, p.ProductId);
            }
            catch { }
        }

        private CancellationTokenSource _imageLoadCts;
        private long _imageLoadSequence = 0;

        private void LoadProductImage(string imagePath, string productId = "")
        {
            long seq = Interlocked.Increment(ref _imageLoadSequence);
            _imageLoadCts?.Cancel();
            _imageLoadCts = new CancellationTokenSource();
            var token = _imageLoadCts.Token;

            // Tier 0: RAM cache — instant, KHÔNG clear hình, không flash
            if (!string.IsNullOrWhiteSpace(productId) &&
                ProductImageHelper.ImageCache.TryGetValue(productId, out var cached) && cached != null)
            {
                picProductImage.Image = cached;
                lblProductPlaceholder.Visible = false;
                return;
            }

            // Clear hình cũ ngay lập tức + dispose để tránh memory leak
            picProductImage.Image?.Dispose();
            picProductImage.Image = null;
            lblProductPlaceholder.Visible = true;

            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            // Tier 1-2: Disk — async + debounce 80ms
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                // Debounce: chờ 80ms, nếu user đổi sản phẩm khác → cancel
                try { await System.Threading.Tasks.Task.Delay(80, token); }
                catch { return; }
                if (seq != Interlocked.Read(ref _imageLoadSequence)) return;

                var img = ProductImageHelper.GetProductImage(imagePath, productId);
                if (token.IsCancellationRequested || seq != Interlocked.Read(ref _imageLoadSequence))
                {
                    img?.Dispose();
                    return;
                }

                this.BeginInvoke(new Action(() =>
                {
                    var selected = GetSelectedProduct();
                    if (selected?.ProductId == productId)
                    {
                        picProductImage.Image?.Dispose();
                        picProductImage.Image = img;
                        lblProductPlaceholder.Visible = img == null;
                    }
                    else
                    {
                        img?.Dispose();
                    }
                }));
            });
        }

        private void BtnSearchProduct_Click(object sender, EventArgs e)
        {
            LoadProductListToGrid(txtSearchProduct.Text);
        }

        private async void BtnSyncMarkQr_Click(object sender, EventArgs e)
        {
            if (dgvQrIsUsed.SelectedRows.Count == 0)
            {
                CuzMessageBox.Show("Vui lòng chọn ít nhất 1 dòng QR cần đồng bộ.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnSyncMarkQr.Enabled = false;
            int synced = 0;
            int skipped = 0;
            int failed = 0;
            try
            {
                foreach (DataGridViewRow row in dgvQrIsUsed.SelectedRows)
                {
                    string qrCode = row.Cells["colQrCode"].Value?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(qrCode)) continue;

                    //bool isSent = row.Cells["colIsSent"].Value is bool s && s;
                    //if (isSent) { skipped++; continue; }

                    //if (RLinkLogService.IsQrSentToMaster(qrCode)) { skipped++; continue; }
                    object usedVal = row.Cells["colIsUsed"].Value;
                    bool isUsed = usedVal is bool u ? u : (usedVal is int ui ? ui == 1 : false);
                    if (!isUsed) { skipped++; continue; }

                    object printedVal = row.Cells["colIsPrinted"].Value;
                    bool isPrinted = printedVal is bool p ? p : (printedVal is int pi ? pi == 1 : false);
                    if (!isPrinted) { skipped++; continue; }

                    object sentVal = row.Cells["colIsSent"].Value;
                    bool isSent = sentVal is bool s ? s
                        : (sentVal is int si ? si == 1
                        : (sentVal is string ss && (ss == "1" || ss.Equals("true", StringComparison.OrdinalIgnoreCase))));
                    if (isSent) { skipped++; continue; }
                    var svc = RLinkMasterServiceFactory.Instance;
                    if (svc == null) break;

                    try
                    {
                        var (batch, pid, prod, exp, jobName, printedAt) = RLinkLogService.GetQrCodeData(qrCode);
                        bool ok = await Task.Run(() => svc.MarkQrUsedAsync(
                            new System.Collections.Generic.List<string> { qrCode },
                            jobName,
                            Shared.Settings?.LineId ?? "",
                            Shared.Settings?.LineName ?? "",
                            Shared.Settings?.FactoryCode ?? "",
                            batch, pid, prod, exp, printedAt));
                        if (ok)
                        {
                            RLinkLogService.MarkQrAsSentToMaster(qrCode);
                            synced++;
                        }
                        else
                        {
                            failed++;
                        }
                    }
                    catch { failed++; }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[BtnSyncMarkQr] " + ex.Message);
            }
            finally
            {
                btnSyncMarkQr.Enabled = true;
            }

            if (synced > 0 || skipped > 0 || failed > 0)
            {
                string msg = synced > 0
                    ? $"Đã đồng bộ {synced} mã QR thành công."
                    : "Không có mã QR nào được đồng bộ.";
                if (skipped > 0)
                    msg += $"\n({skipped} mã đã gửi trước đó — bỏ qua)";
                if (failed > 0)
                    msg += $"\n({failed} mã gửi thất bại — vui lòng thử lại)";
                CuzMessageBox.Show(msg, "Kết quả",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadQrIsUsedAsync(txtSearchQr.Text, dtpFromDateQr.Value, dtpToDateQr.Value);
            }
        }

        // ── Thêm method này vào region Utility_Function ──────────────────────────
        /// <summary>
        /// Thiết lập cột HeaderText + layout cho dgvSyncLog, wire button events.
        /// Gọi 1 lần trong InitControls().
        /// </summary>
        private void SetupSyncLogTab()
        {
            // ── Cột HeaderText cho dgvSyncLog ─────────────────────────────
            dataGridViewTextBoxColumn1.HeaderText = "Tên Job";
            dataGridViewTextBoxColumn1.Name = "colJob";
            dataGridViewTextBoxColumn1.FillWeight = 28;

            dataGridViewTextBoxColumn2.HeaderText = "Batch";
            dataGridViewTextBoxColumn2.Name = "colBatch";
            dataGridViewTextBoxColumn2.FillWeight = 14;

            dataGridViewTextBoxColumn3.HeaderText = "Log In chờ";
            dataGridViewTextBoxColumn3.Name = "colLogIn";
            dataGridViewTextBoxColumn3.FillWeight = 12;
            dataGridViewTextBoxColumn3.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewTextBoxColumn4.HeaderText = "Camera chờ";
            dataGridViewTextBoxColumn4.Name = "colCamera";
            dataGridViewTextBoxColumn4.FillWeight = 12;
            dataGridViewTextBoxColumn4.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewTextBoxColumn5.HeaderText = "Error chờ";
            dataGridViewTextBoxColumn5.Name = "colError";
            dataGridViewTextBoxColumn5.FillWeight = 12;
            dataGridViewTextBoxColumn5.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewTextBoxColumn6.HeaderText = "Tổng chờ";
            dataGridViewTextBoxColumn6.Name = "colTotal";
            dataGridViewTextBoxColumn6.FillWeight = 10;
            dataGridViewTextBoxColumn6.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewTextBoxColumn7.HeaderText = "Trạng thái";
            dataGridViewTextBoxColumn7.Name = "colStatus";
            dataGridViewTextBoxColumn7.FillWeight = 12;
            dataGridViewTextBoxColumn7.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewTextBoxColumn8.Visible = false;

            // ── Layout dgvSyncLog ─────────────────────────────────────────
            dgvSyncLog.Dock = DockStyle.None;
            dgvSyncLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvSyncLog.Location = new System.Drawing.Point(0, SYNC_LOG_TOP_OFFSET);
            dgvSyncLog.Size = new System.Drawing.Size(
                tabLogSync.ClientSize.Width,
                tabLogSync.ClientSize.Height - SYNC_LOG_TOP_OFFSET);
            dgvSyncLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSyncLog.MultiSelect = true;
            dgvSyncLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSyncLog.RowTemplate.Height = 35;
            dgvSyncLog.RowHeadersVisible = false;

            // ── Style giống dgvQrIsUsed ──────────────────────────────
            dgvSyncLog.EnableHeadersVisualStyles = false;
            dgvSyncLog.BackgroundColor = System.Drawing.Color.FromArgb(245, 245, 245);
            dgvSyncLog.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dgvSyncLog.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvSyncLog.GridColor = System.Drawing.Color.FromArgb(224, 224, 224);

            dgvSyncLog.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = System.Drawing.Color.FromArgb(0, 171, 230),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F),
                ForeColor = System.Drawing.Color.White,
                Padding = new Padding(0, 5, 0, 5),
                SelectionBackColor = System.Drawing.Color.FromArgb(0, 171, 230),
                SelectionForeColor = System.Drawing.SystemColors.HighlightText,
                WrapMode = DataGridViewTriState.True
            };

            dgvSyncLog.DefaultCellStyle = new DataGridViewCellStyle
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

            dgvSyncLog.RowHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = System.Drawing.SystemColors.Control,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F),
                ForeColor = System.Drawing.SystemColors.WindowText,
                SelectionBackColor = System.Drawing.Color.FromArgb(8, 109, 70),
                SelectionForeColor = System.Drawing.SystemColors.HighlightText,
                WrapMode = DataGridViewTriState.True
            };

            // ── Thiết kế lại syncDataLog overlay panel ────────────────────
            syncDataLog.Visible = false;
            syncDataLog.BackColor = System.Drawing.Color.White;
            syncDataLog.Padding = new Padding(0);
            syncDataLog.MinimumSize = new System.Drawing.Size(400, 200);

            // Cấu hình progressLogin thành Marquee animated
            progressSyncData.Style = ProgressBarStyle.Marquee;
            progressSyncData.MarqueeAnimationSpeed = 25;
            progressSyncData.Height = 6;
            progressSyncData.Visible = false;
            progressSyncData.Dock = DockStyle.None;
            progressSyncData.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            progressSyncData.BackColor = System.Drawing.Color.FromArgb(230, 240, 250);

            // label33: tiêu đề — lớn, đậm
            label33.Text = "🔄 Đồng bộ dữ liệu";
            label33.Font = new System.Drawing.Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold);
            label33.ForeColor = System.Drawing.Color.FromArgb(30, 90, 160);
            label33.TextAlign = ContentAlignment.MiddleCenter;
            label33.Dock = DockStyle.None;

            // label34: tên job — nhỏ, xám
            label34.Text = "";
            label34.Font = new System.Drawing.Font("Segoe UI", 9.5f);
            label34.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            label34.TextAlign = ContentAlignment.MiddleCenter;
            label34.Dock = DockStyle.None;

            // label32: trạng thái realtime — rõ, không bị tràn
            label32.Text = "";
            label32.Font = new System.Drawing.Font("Segoe UI", 10.5f);
            label32.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            label32.TextAlign = ContentAlignment.MiddleCenter;
            label32.Dock = DockStyle.None;

            // Sắp xếp layout các control trong syncDataLog
            syncDataLog.SizeChanged += (s, e) => LayoutSyncDataLogPanel();
            LayoutSyncDataLogPanel();

            // ── Wire events ───────────────────────────────────────────────
            btnStopSyncLog.Visible = false;
            btnStopSyncLog.Click += BtnStopSyncLog_Click;
            btnSyncSelected.Click += BtnSyncSelected_Click;
            btnRefreshSyncLog.Click += (s, e) => LoadSyncLogToGrid();

            tabLogSync.Resize += (s, e) =>
            {
                dgvSyncLog.Size = new System.Drawing.Size(
                    tabLogSync.ClientSize.Width,
                    tabLogSync.ClientSize.Height - SYNC_LOG_TOP_OFFSET);
            };

            dgvSyncLog.CellFormatting += (s, e) =>
            {
                if (dgvSyncLog.Columns[e.ColumnIndex].Name == "colJob" && e.Value is string sj)
                {
                    e.Value = sj.Replace(' ', '_');
                    e.FormattingApplied = true;
                }
            };
        }

        /// <summary>
        /// Căn giữa và sắp xếp các control trong overlay syncDataLog.
        /// </summary>
        private void LayoutSyncDataLogPanel()
        {
            if (InvokeRequired) { Invoke(new Action(LayoutSyncDataLogPanel)); return; }

            int w = syncDataLog.Width;

            // Tiêu đề
            label33.Width = w - 60;
            label33.Height = 36;
            label33.Location = new System.Drawing.Point(30, 20);
            label33.AutoSize = true;
            label33.TextAlign = ContentAlignment.MiddleCenter;

            // Tên job
            label34.Width = w - 60;
            label34.Height = 24;
            label34.Location = new System.Drawing.Point(30, 60);
            label34.AutoSize = true;
            label34.TextAlign = ContentAlignment.MiddleCenter;

            // Progress bar
            progressSyncData.Width = w - 80;
            progressSyncData.Height = 6;
            progressSyncData.Location = new System.Drawing.Point(40, 95);

            // Status text
            label32.Width = w - 60;
            label32.Height = 40;
            label32.Location = new System.Drawing.Point(30, 115);
            label32.AutoSize = true;
            label32.TextAlign = ContentAlignment.MiddleCenter;
            label32.MaximumSize = new System.Drawing.Size(w - 60, 80);

            // Nút xác nhận / hủy — căn giữa dưới đáy
            int btnY = label32.Location.Y + label32.Height + 8;
            btnStopSyncLog.Width = 130;
            btnStopSyncLog.Height = 36;
            btnStopSyncLog.Location = new System.Drawing.Point((w - btnStopSyncLog.Width) / 2, btnY);
            btnStopSyncLog.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
        }

        private void HideSyncDataLogPanel()
        {
            if (InvokeRequired) { BeginInvoke(new Action(HideSyncDataLogPanel)); return; }

            syncDataLog.Visible = false;
            progressSyncData.Visible = false;

            // Reset label về trạng thái ban đầu cho lần sync tiếp
            label33.Text = "🔄  ĐỒNG BỘ DỮ LIỆU";
            label33.ForeColor = System.Drawing.Color.FromArgb(30, 90, 160);
            label34.Text = "";
            label32.Text = "";
            label32.ForeColor = System.Drawing.Color.FromArgb(30, 90, 160);

            btnSyncSelected.Enabled = true;
        }

        /// <summary>
        /// Tải danh sách log chưa gửi theo job từ SQLite, hiển thị lên dgvSyncLog.
        /// </summary>
        private void LoadSyncLogToGrid()
        {
            Task.Run(() =>
            {
                try
                {
                    var jobs = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
                        .RLinkLogService.GetUnsentCountsByJobPg(Shared.Settings.LineId ?? "");

                    BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            dgvSyncLog.Rows.Clear();
                            foreach (var j in jobs)
                            {
                                int total = j.pending_in + j.pending_camera + j.pending_error;
                                string status = total == 0 ? "✔ Đã đồng bộ"
                                             : "⏳ Chưa đồng bộ (" + total + ")";

                                int rowIdx = dgvSyncLog.Rows.Add(
                                    j.job_name,
                                    j.batch,
                                    j.pending_in,
                                    j.pending_camera,
                                    j.pending_error,
                                    total,
                                    status);

                                var row = dgvSyncLog.Rows[rowIdx];
                                if (dgvSyncLog.Columns["colStatus"] != null)
                                {
                                    row.Cells["colStatus"].Style.ForeColor = total == 0
                                        ? System.Drawing.Color.FromArgb(0, 140, 70)
                                        : System.Drawing.Color.FromArgb(192, 0, 0);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteError("[SyncLogGrid] Lỗi update UI: " + ex.Message, ex);
                        }
                    }));
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[SyncLogGrid] Lỗi load: " + ex.Message, ex);
                }
            });
        }

        private void BtnSyncSelected_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSyncLog.SelectedRows.Count == 0)
                {
                    CuzMessageBox.Show("Vui lòng chọn job cần đồng bộ!", "Chưa chọn",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var svcRef = RLinkMasterServiceFactory.Instance;
                if (svcRef == null)
                {
                    CuzMessageBox.Show("Chưa kết nối R-Link Master!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ── Gom danh sách job được chọn ──────────────────────────
                var selectedJobs = new List<(string jobName, string batch, int inCnt, int camCnt, int errCnt)>();
                foreach (DataGridViewRow row in dgvSyncLog.SelectedRows)
                {
                    string jn = row.Cells["colJob"].Value?.ToString() ?? "";
                    string bt = row.Cells["colBatch"].Value?.ToString() ?? "";
                    int.TryParse(row.Cells["colLogIn"].Value?.ToString(), out int iCnt);
                    int.TryParse(row.Cells["colCamera"].Value?.ToString(), out int cCnt);
                    int.TryParse(row.Cells["colError"].Value?.ToString(), out int eCnt);
                    if (iCnt + cCnt + eCnt > 0)
                        selectedJobs.Add((jn, bt, iCnt, cCnt, eCnt));
                }

                if (selectedJobs.Count == 0)
                {
                    CuzMessageBox.Show("Các job đã chọn không có dữ liệu chờ đồng bộ!", "Hết dữ liệu",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ── Dùng Form thông báo mới ──────────────────────────────
                btnSyncSelected.Enabled = false;
                var syncForm = new SyncNotificationForm();
                syncForm.StartPosition = FormStartPosition.Manual;
                syncForm.Location = new Point(
                    this.Left + (this.Width - syncForm.Width) / 2,
                    this.Top + (this.Height - syncForm.Height) / 2);
                syncForm.Show(this);
                var showTime = DateTime.Now;

                var cancelToken = syncForm.Cts.Token;
                var jobsToSync = selectedJobs;
                _ = Task.Run(async () =>
                {
                    int sentIn = 0, sentCam = 0, sentErr = 0;
                    int failIn = 0, failCam = 0, failErr = 0;
                    int stopIn = 0, stopCam = 0, stopErr = 0;
                    var semaphore = new System.Threading.SemaphoreSlim(20);

                    async System.Threading.Tasks.Task<(int sent, int fail, int stop)> SyncTypeAsync<T>(
                        int pending,
                        Func<int, List<(long id, T payload)>> fetcher,
                        Func<T, System.Threading.Tasks.Task<bool>> sendAsync,
                        Action<long> marker,
                        string label)
                    {
                        if (pending <= 0) return (0, 0, 0);
                        int s = 0, f = 0, st = 0;
                        syncForm.SetStatus($"{label}: đang gửi {pending} bản ghi…");
                        var rows = fetcher(pending);
                        var tasks = rows.Select(async row =>
                        {
                            await semaphore.WaitAsync(cancelToken);
                            try
                            {
                                bool ok = await sendAsync(row.payload);
                                if (ok) { marker(row.id); Interlocked.Increment(ref s); }
                                else Interlocked.Increment(ref f);
                            }
                            catch (System.OperationCanceledException) { Interlocked.Increment(ref st); }
                            catch { Interlocked.Increment(ref f); }
                            finally { semaphore.Release(); }
                        });
                        await Task.WhenAll(tasks);
                        return (s, f, st);
                    }

                    foreach (var job in jobsToSync)
                    {
                        string jn = job.jobName;
                        string bt = job.batch;
                        var jobTasks = new List<System.Threading.Tasks.Task<(int sent, int fail, int stop)>>();
                        jobTasks.Add(SyncTypeAsync(
                            job.inCnt,
                            limit => RLinkLogService.GetUnsentLogInPg(limit, jn, bt),
                            p => svcRef.SendLogStatusAsync(p),
                            id => {
                                RLinkLogService.MarkTableRowSentPgById(THDb.LogIn, id);
                                RLinkLogService.MarkTableRowSent(THDb.LogIn, id);
                            },
                            "Log In"));
                        jobTasks.Add(SyncTypeAsync(
                            job.camCnt,
                            limit => RLinkLogService.GetUnsentLogCameraPg(limit, jn, bt),
                            p => svcRef.SendLogCameraAsync(p),
                            id => {
                                RLinkLogService.MarkTableRowSentPgById(THDb.LogCamera, id);
                                RLinkLogService.MarkTableRowSent(THDb.LogCamera, id);
                            },
                            "Camera"));
                        jobTasks.Add(SyncTypeAsync(
                            job.errCnt,
                            limit => RLinkLogService.GetUnsentLogCameraErrorPg(limit, jn, bt),
                            p => svcRef.SendLogCameraErrorAsync(p),
                            id => {
                                RLinkLogService.MarkTableRowSentPgById(THDb.LogCameraError, id);
                                RLinkLogService.MarkTableRowSent(THDb.LogCameraError, id);
                            },
                            "Error"));

                        var results = await Task.WhenAll(jobTasks);
                        sentIn += results[0].sent; failIn += results[0].fail; stopIn += results[0].stop;
                        sentCam += results[1].sent; failCam += results[1].fail; stopCam += results[1].stop;
                        sentErr += results[2].sent; failErr += results[2].fail; stopErr += results[2].stop;
                    }

                    ProjectLogger.WriteInfo(
                        $"[BtnSyncSelected] ✔ OK:{sentIn}/{sentCam}/{sentErr} FAIL:{failIn}/{failCam}/{failErr}");

                    // ── Đảm bảo form hiển thị ít nhất 3 giây ──
                    double elapsed = (DateTime.Now - showTime).TotalSeconds;
                    if (elapsed < 3)
                        await System.Threading.Tasks.Task.Delay((int)((3 - elapsed) * 1000));

                    // ── Cập nhật form kết quả ──
                    BeginInvoke(new Action(() =>
                    {
                        syncForm.SetComplete(sentIn, sentCam, sentErr, failIn + failCam + failErr, stopIn + stopCam + stopErr);
                        _ = syncForm.WaitForConfirmAsync().ContinueWith(_ =>
                        {
                            BeginInvoke(new Action(() =>
                            {
                                syncForm.Close();
                                syncForm.Dispose();
                                LoadSyncLogToGrid();
                                btnSyncSelected.Enabled = true;
                            }));
                        }, TaskContinuationOptions.ExecuteSynchronously);
                    }));
                });
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[BtnSyncSelected_Click] Lỗi: " + ex.Message, ex);
            }
        }

        private void BtnStopSyncLog_Click(object sender, EventArgs e)
        {
            // Nếu sync xong → bấm "Xác nhận" → đóng overlay
            if (_syncConfirmTcs != null)
            {
                var tcs = _syncConfirmTcs;
                _syncConfirmTcs = null;
                tcs.TrySetResult(true);
                return;
            }

            // Nếu đang đồng bộ bằng tay → hủy (ưu tiên)
            if (_syncCts != null && !_syncCts.IsCancellationRequested)
            {
                _syncCts.Cancel();
                btnStopSyncLog._Text = "⏳ Đang dừng...";
                btnStopSyncLog.Enabled = false;
                return;
            }

            // Ngược lại → toggle auto-retry
            var svc = RLinkLogRetrySenderService.Instance;
            if (!svc.IsPaused)
            {
                svc.Pause();
                btnStopSyncLog._Text = "▶ Bật lại tự động";
                btnStopSyncLog.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
                btnStopSyncLog.BackgroundColor = System.Drawing.Color.FromArgb(40, 167, 69);
            }
            else
            {
                svc.Resume();
                btnStopSyncLog._Text = "■ Dừng tự động";
                btnStopSyncLog.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
                btnStopSyncLog.BackgroundColor = System.Drawing.Color.FromArgb(220, 53, 69);
            }
        }

       
        /// <summary>
        /// Tự động lấy cấu hình từ R-Link Master API, lưu vào Shared.Settings,
        /// rồi refresh toàn bộ textbox trên tabGetPO.
        /// Fallback: đọc từ PostgreSQL nếu API offline.
        /// </summary>
        private async Task AutoLoadAccountsAsync()
        {
            try
            {
                var service = RLinkMasterServiceFactory.Instance;
                var accounts = await service.GetAccountsAsync();
                if (accounts != null && accounts.Count > 0)
                {
                    // DB writes chạy background — không block UI
                    _ = Task.Run(async () =>
                    {
                        try { await LocalAccountStore.SyncAccountsFromRLinkAsync(accounts); }
                        catch (Exception ex) { Debug.WriteLine($"[AutoLoadAccounts] DB sync error: {ex.Message}"); }
                    });
                    Console.WriteLine($"[AutoLoadAccounts] Synced {accounts.Count} tài khoản.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[AutoLoadAccounts] Skip: " + ex.Message);
            }
        }

        private async Task AutoLoadSettingsAsync()
        {
            try
            {
                RLinkSettings rlinkSettings = null;
                string lineId = Shared.Settings.LineId ?? string.Empty;

                // ── Ưu tiên 1: Load từ local DB (retry nếu đang sync) ──
                for (int retry = 0; retry < 2; retry++)
                {
                    rlinkSettings = await BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.LocalAccountStore.LoadSettingsFromDbAsync(lineId);
                    if (rlinkSettings != null) break;
                    if (retry < 1) await Task.Delay(500); // chờ background sync từ login
                }

                // ── Ưu tiên 2: Fallback API nếu DB không có ──────────
                if (rlinkSettings == null)
                {
                    try
                    {
                        var service = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory.Instance;
                        rlinkSettings = await service.GetSettingsAsync(lineId);
                        if (rlinkSettings != null && rlinkSettings.OperatingMode > 0)
                            _ = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.LocalAccountStore.SaveSettingsAsync(lineId, rlinkSettings);
                        else
                            rlinkSettings = null; // API trả về default → coi như không có
                    }
                    catch { }
                }
                else
                {
                    // ── Background refresh từ API (không block UI) ────
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var service = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkMasterServiceFactory.Instance;
                            var fresh = await service.GetSettingsAsync(lineId);
                            if (fresh != null && fresh.OperatingMode > 0) // Chỉ apply nếu API trả về data hợp lệ
                            {
                                _ = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.LocalAccountStore.SaveSettingsAsync(lineId, fresh);
                                Shared.Settings.THOperatingMode = (THTrueMilkOperatingMode)fresh.OperatingMode;
                                Shared.Settings.THDeltaMinutes = fresh.DeltaMinutes;
                                Shared.Settings.THNMinutes = fresh.NMinutes;
                                Shared.Settings.THBufferCount = fresh.BufferCount;
                                Shared.Settings.THMonitorInterval = fresh.MonitoringIntervalMinutes;
                                Shared.Settings.THLogInterval = fresh.LogIntervalMinutes;
                                Shared.Settings.THQrThreshold = fresh.QrThreshold;
                                Shared.Settings.THErrorImageFolder = fresh.ErrorImageFolder ?? string.Empty;
                                Shared.Settings.THMaxConsecutiveError = fresh.MaxConsecutiveDefects;
                                Shared.Settings.THReserveFactor = fresh.ReserveFactor;
                                SaveSettings();
                                BeginInvoke(new Action(() => LoadRLinkSettingsToUI()));
                                ProjectLogger.WriteInfo($"[AutoLoadSettings] Background refresh OK → Mode={fresh.OperatingMode}");
                            }
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteWarning("[AutoLoadSettings] Background refresh fail: " + ex.Message);
                        }
                    });
                }

                if (rlinkSettings == null)
                {
                    ProjectLogger.WriteWarning("[AutoLoadSettings] Không lấy được settings từ DB lẫn API.");
                    return;
                }

                // ── Áp dụng vào Shared.Settings ──────────────────────────
                Shared.Settings.THOperatingMode = (THTrueMilkOperatingMode)rlinkSettings.OperatingMode;
                Shared.Settings.THDeltaMinutes = rlinkSettings.DeltaMinutes;
                Shared.Settings.THNMinutes = rlinkSettings.NMinutes;
                Shared.Settings.THBufferCount = rlinkSettings.BufferCount;
                Shared.Settings.THMonitorInterval = rlinkSettings.MonitoringIntervalMinutes;
                Shared.Settings.THLogInterval = rlinkSettings.LogIntervalMinutes;
                Shared.Settings.THQrThreshold = rlinkSettings.QrThreshold;
                Shared.Settings.THErrorImageFolder = rlinkSettings.ErrorImageFolder ?? string.Empty;
                Shared.Settings.THMaxConsecutiveError = rlinkSettings.MaxConsecutiveDefects;
                Shared.Settings.THReserveFactor = rlinkSettings.ReserveFactor;
                SaveSettings();
                ProjectLogger.WriteInfo($"[AutoLoadSettings] Mode={rlinkSettings.OperatingMode}, Delta={rlinkSettings.DeltaMinutes}p, Buffer={rlinkSettings.BufferCount}, Threshold={rlinkSettings.QrThreshold}");

                LoadRLinkSettingsToUI();
                UpdateQrConfigLabels();
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[AutoLoadSettings] Lỗi: " + ex.Message, ex);
            }
        }

        #endregion UI_Control_Event

        #region Orther_Events
        private void Shared_OnPrintingStateChange(object sender, EventArgs e)
        {
            EnableUIPrinting();
        }
        private void Shared_OnPrinterStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelPrinter();
            ObtainPrintProductTemplateList();
        }
        private void Shared_OnSensorControllerChangeEvent(object sender, EventArgs e)
        {
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
        }
        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }
        private void Shared_OnPrinterDataChange(object sender, EventArgs e)
        {
            if (sender is PODDataModel)
            {
                var podDataModel = sender as PODDataModel;

                try
                {
                    // Split and remove empty entries
                    string[] pODcommand = podDataModel.Text
                            .Split(new[] { Shared.Settings.SplitCharacter }, StringSplitOptions.RemoveEmptyEntries);

                    var PODResponseModel = new PODResponseModel
                    {
                        Command = pODcommand.FirstOrDefault()
                    };

                    if (PODResponseModel != null && PODResponseModel.Command == "RSLI")
                    {
                        pODcommand = pODcommand.Skip(1).ToArray();
                        PODResponseModel.Template = pODcommand;

                        if (podDataModel.RoleOfPrinter == RoleOfStation.ForProduct)
                        {
                            _PrintProductTemplateList = PODResponseModel.Template;  // List print template
                            Shared.Settings.CachedPrinterTemplates = PODResponseModel.Template.ToList();
                            UpdateUIListBoxPrintProductTemplateList(_PrintProductTemplateList);
                        }
                        _rslResponseTcs?.TrySetResult(true);
                    }
                }
                catch (Exception)
                {
                    // Ideally log or handle the exception, even if silently
                }
            }
        }
        private void Shared_OnCameraStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelCamera();
            LoadKeyenceProgramsToComboBox();
        }

        private async void LoadKeyenceProgramsToComboBox()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LoadKeyenceProgramsToComboBox()));
                return;
            }
            
            var cam = Shared.Settings.CameraList.FirstOrDefault();
            bool isKeyence = cam?.CameraBrand == CameraBrand.Keyence
                             || cam?.CameraType == CameraType.VS_C
                             || cam?.CameraType == CameraType.CV_X;
            if (!isKeyence) return;
            bool connected = Shared.vscCamera != null && Shared.vscCamera.IsConnected();
            if (!connected)
            {
                Shared.Settings.CachedCameraPrograms = new List<string>();
                cbcSelectedModel.Items.Clear();
                return;
            }
            try
            {
                var programs = await Shared.vscCamera.GetProgramListAsync();
                Shared.Settings.CachedCameraPrograms = programs.Select(p => $"{p.ProgramNo:D4}_{p.Name}").ToList();
                cbcSelectedModel.Items.Clear();
                foreach (var p in programs)
                    cbcSelectedModel.Items.Add(p);  // VscProgramInfo.ToString() → "0001 ProgramName"
                if (cbcSelectedModel.Items.Count > 0)
                    cbcSelectedModel.SelectedIndex = 0;
                Shared.SaveSettings();  // Lưu programs ra disk
            }
            catch (Exception ex)
            {
                Shared.Settings.CachedCameraPrograms = new List<string>();
                cbcSelectedModel.Items.Clear();
                ProjectLogger.WriteError($"Load Keyence programs failed: {ex.Message}");
            }
        }
        public void Shared_OnCameraTriggerOnChange(object sender, EventArgs e)
        {
            switch (Shared.Settings.CameraList.FirstOrDefault().CameraType)
            {
                case CameraType.DM:
                    foreach (DataManSystem dataManSystem in DMCamera._DataManSystemList)
                    {
                        try
                        {
                            dataManSystem.SendCommand("TRIGGER ON");
                        }
                        catch (Exception) { }
                    }
                    break;
                case CameraType.IS:
                    ISSingleHandler?.ManualTriggerAction();
                    break;
                case CameraType.ISDual:
                    ISMultiSyncHandler?.ManualTriggerAction();
                    break;

                default:
                    break;
            }

        }
        private void Shared_OnCameraTriggerOffChange(object sender, EventArgs e)
        {
            foreach (DataManSystem dataManSystem in DMCamera._DataManSystemList)
            {
                try
                {
                    dataManSystem.SendCommand("TRIGGER OFF");
                }
                catch (Exception) { }
            }
        }
        private void Shared_OnCameraOutputSignalChange(object sender, EventArgs e)
        {
            var cameraModel = Shared.Settings.CameraList.FirstOrDefault();
            int currentIndex = 0;
            if (sender is int index)
            {
                currentIndex = index;
            }

            switch (cameraModel.CameraType)
            {
                case CameraType.DM:
                    if (cameraModel.OutputType == OutputType.OutputCamera)
                    {
                        foreach (DataManSystem dataManSystem in DMCamera._DataManSystemList)
                        {
                            try
                            {
                                DmccResponse response = dataManSystem.SendCommand("OUTPUT.USER1");
                            }
                            catch (Exception) { }
                        }
                    }
                    else
                    {
                        Shared.SendErrorOutputToSensorController(currentIndex);
                    }

                    break;
                case CameraType.IS:
                case CameraType.ISDual:
                    Shared.SendErrorOutputToSensorController(currentIndex);
                    break;
                case CameraType.CV_X:
                    Shared.SendErrorOutputToSensorController(currentIndex);
                    break;

                default:
                    break;
            }
        }
        private void PODController_OnPODReceiveDataEvent(object sender, EventArgs e)
        {
            if (sender is PODDataModel)
            {
                Shared.RaiseOnPrinterDataChangeEvent(sender as PODDataModel);
            }
        }
        private void SensorController_OnPODReceiveMessageEvent(object sender, EventArgs e)
        {
            Shared.RaiseOnRepeatTCPMessageChange(sender);
        }

        #endregion Orther_Events

        #region Utility_Function
        private void SetLanguage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }

            btnSettings.Text = Lang.Settings;
            btnExit.Text = Lang.Exit;
            btnAbout.Text = Lang.About;

            pnlJobInfomation.Text = Lang.JobDetails;
            lblJobName.Text = Lang.FileName;
            lblCompareType.Text = Lang.CompareType;
            FirstRowHeader.Text = Lang.FirstRowHeader;
            lblStaticText1.Text = Lang.StaticText;
            lblPODFormat.Text = Lang.PODFormat;
            lblTemplatePrint.Text = Lang.TemplateName;
            btnNext.Text = Lang.Next;
            lblPrinterSeries.Text = Lang.PrinterSeries;
            lblTemplate.Text = Lang.TemplateName;
            lblJobTypeInput.Text = Lang.JobType;
            radAfterProduction.Text = Lang.AfterProduction;
            radOnProduction.Text = Lang.OnProduction;
            radVerifyAndPrint.Text = Lang.VerifyAndPrint;

            lblJobType.Text = Lang.JobType;
            lblJobStatus.Text = Lang.JobStatus;
            btnSave.Text = Lang.Save;
            lblSupportForCamera.Text = Lang.SupportForCamera;
            lblCompare.Text = Lang.CompareType;
            lblStaticText.Text = Lang.StaticText;
            radCanRead.Text = Lang.CanRead;
            radStaticText.Text = Lang.StaticText;

            radDatabase.Text = Lang.Database;
            lblImportDatabase.Text = Lang.ImportDatabase;
            lblPODFromat.Text = Lang.PODFormat;
            lblFileName.Text = Lang.JobList;

            lblStatusCamera01.Text = Lang.CameraTMP;
            lblStatusPrinter01.Text = Lang.Printer;
            //lblStatusSerialDevice.Text = Lang.ScannerLabel;
            lblSensorControllerStatus.Text = Lang.PLCLabel;
            // thinh them Lang text
            txtJobType.Text = _JobModel.JobType.ToFriendlyString();
            txtJobStatus.Text = _JobModel.JobStatus.ToFriendlyString();

            switch (_JobModel.CompareType)
            {
                case CompareType.CanRead:
                    lblCompareTypeInfo.Text = Lang.CanRead;
                    break;
                case CompareType.StaticText:
                    lblCompareTypeInfo.Text = Lang.StaticText;
                    break;
                default:
                    lblCompareTypeInfo.Text = Lang.Database;
                    break;
            }

           // lblToolStripVersion.Text = Lang.Version + ": " + Properties.Settings.Default.SoftwareVersion;
            lblToolStripVersion.Text = "v" + Properties.Settings.Default.SoftwareVersion;
            btnDelete.Text = Lang.Delete;
            btnHelp.Text = Lang.Help;
            btnRestart.Text = Lang.Restart;

            tabGetPO.Text = Lang.CreateANewJob;
            //tabSelectJob.Text = Lang.SelectJob;
            tabSelectJob.Text = Lang.ViewOldJob;
            tabPage3.Text = "In Reservation";
            tabSyncData.Text = "Lịch sử đồng bộ"; // Lang.HistorySync
            TabCreatePOOffline.Text = "In Loyalty"; // Lang.Settings
            tabCreateRESOffline.Text = "In Reservation";
        }

        private void InitUI()
        {
            try
            {
                RESMaufDatePicker.Format = RESExpiredDatePicker.Format = MaufDatePicker.Format = ExpiredDatePicker.Format = DateTimePickerFormat.Custom;
                RESMaufDatePicker.CustomFormat = RESExpiredDatePicker.CustomFormat = ExpiredDatePicker.CustomFormat = MaufDatePicker.CustomFormat = Settings.LOTFormatDate = "yyyy/MM/dd";

                cbbHisFilterType.SelectedIndex = 0;
                dtpFromDate.Value = DateTime.Now.AddDays(-7);
                dtpToDate.Value = DateTime.Now;
                HistoryUtils.CustomDataGridView(dgv: dgvHistoryJob);
                SetupDataGridView();


                var mode = _JobModel?.THJobOperatingMode ?? Shared.Settings.THOperatingMode;
                if (mode == THTrueMilkOperatingMode.BatchOneQrCode ||
                    mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange ||
                    mode == THTrueMilkOperatingMode.AutoRefreshByTime)
                {
                    InitMode1UI();   
                }
                else
                {
                    if (!UserPermission.isOnline)
                    {
                       // tabControl1.Controls.Remove(tabGetPO);
                        tabControl1.Controls.Remove(tabPage3);
            tabControl1.Controls.Remove(tabSyncData);

            // Batch tự động theo ngày gửi máy in
            txtBatchNumber.Visible = false;
                    }
                    else
                    {
                        tabControl1.Controls.Remove(TabCreatePOOffline);
                        tabControl1.Controls.Remove(tabCreateRESOffline);
                    }
                }
                dgvItems.Rows.Clear();
            }
            catch (Exception)
            {
            }
        }
        // ─── Thêm method mới InitMode1UI() ───
        /// <summary>
        /// Ẩn các tab không cần thiết cho Mode 1 (BatchOneQrCode).
        /// Mode 1 không cần tạo PO/Reservation — job được tạo tự động từ QR batch.
        /// </summary>
        private void InitMode1UI()
        {
            //tabControl1.Controls.Remove(tabGetPO);
            tabControl1.Controls.Remove(tabPage3);
            tabControl1.Controls.Remove(tabSyncData);
            tabControl1.Controls.Remove(TabCreatePOOffline);
            tabControl1.Controls.Remove(tabCreateRESOffline);

            // Ẩn btnNext — Mode1 dùng nút riêng
            //btnNext.Visible = false;

           

            // Label batch info
            //_lblMode1BatchInfo = new Label
            //{
            //    AutoSize = false,
            //    Dock = DockStyle.Top,
            //    Height = 48,
            //    TextAlign = ContentAlignment.MiddleLeft,
            //    Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold),
            //    ForeColor = Color.FromArgb(0, 120, 215),
            //    Text = "Batch QR: (chưa có — chờ QrBank đẩy dữ liệu)",
            //    Padding = new Padding(6, 0, 0, 0)
            //};

            //// Nút Tạo Job
            //var btnCreateMode1 = new Button
            //{
            //    Text = "▶  Tạo Job & Bắt đầu",
            //    Height = 44,
            //    Dock = DockStyle.Top,
            //    BackColor = Color.FromArgb(0, 171, 230),
            //    ForeColor = Color.White,
            //    FlatStyle = FlatStyle.Flat,
            //    Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold)
            //};
            //btnCreateMode1.FlatAppearance.BorderSize = 0;
            //btnCreateMode1.Click += (s, e) => StartMode1Job();

            //// Panel gộp 2 control
            //var pnlMode1 = new Panel { Dock = DockStyle.Top, Height = 100 };
            //pnlMode1.Controls.Add(btnCreateMode1);
            //pnlMode1.Controls.Add(_lblMode1BatchInfo);

            //tabSelectJob.Controls.Add(pnlMode1);
            //tabSelectJob.Controls.SetChildIndex(pnlMode1, 0);
        }

        /// <summary>
        /// Cập nhật label hiển thị batch/QR hiện tại cho Mode 1.
        /// </summary>
        private void UpdateMode1UI()
        {
            var mode = _JobModel?.THJobOperatingMode ?? Shared.Settings.THOperatingMode;
            if (mode != THTrueMilkOperatingMode.BatchOneQrCode &&
                mode != THTrueMilkOperatingMode.BatchOneQrCodeNoChange &&
                mode != THTrueMilkOperatingMode.AutoRefreshByTime)
                return;
            if (_lblMode1BatchInfo == null) return;

            void DoUpdate()
            {
                lock (_batchQrLock)
                {
                    if (string.IsNullOrEmpty(_currentBatchQrCode))
                    {
                        _lblMode1BatchInfo.ForeColor = Color.OrangeRed;
                        _lblMode1BatchInfo.Text = "Batch QR: (chưa có — chờ QrBank đẩy dữ liệu)";
                    }
                    else
                    {
                        _lblMode1BatchInfo.ForeColor = Color.FromArgb(0, 120, 215);
                        _lblMode1BatchInfo.Text = $"Batch: {_currentBatchId}  |  QR: {_currentBatchQrCode}  |  Ngày: {_currentBatchDate:yyyy-MM-dd}";
                    }
                }
            }

            if (InvokeRequired)
                Invoke(new Action(DoUpdate));
            else
                DoUpdate();
        }

        private void InitControls()
        {

#if DEBUG

            DebugVirtual();
#endif

            tabControl1.Controls.Remove(tabCreateRESOffline);
            tabControl1.Controls.Remove(tabPage3);

            _LabelStatusCameraList.Add(lblStatusCamera01);
            UpdateStatusLabelCamera();
            _LabelStatusPrinterList.Add(lblStatusPrinter01);
            UpdateStatusLabelPrinter();
            _NameOfJobOld = "";
            CreateJob();
            cuzButtonPurge.Visible = Properties.Settings.Default.Username == "demo";
            cboSupportForCamera.Enabled = false;
            if (cboSupportForCamera.Items.Count == 0)
            {
                cboSupportForCamera.Items.Add("DM Series");
                cboSupportForCamera.Items.Add("IS Series");
            }
            var camType = Shared.Settings.CameraList?.FirstOrDefault()?.CameraType;
            cboSupportForCamera.SelectedIndex = camType == CameraType.DM ? 0 : 1;
            // Initialize DomainInput ComboBox with ApiDomains
            InitDomainInputComboBox();
            
            _TimerDateTime.Start();
            _NameOfJobOld = "";
            Shared.JobNameSelected = "";
            var podText = new PODModel(0, "", PODModel.TypePOD.TEXT, "");
            _PODList.Add(podText);

            //btnSettings.Enabled = Shared.UserPermission.Settings;
            //btnDelete.Enabled = Shared.UserPermission.DeleteJob;
            //tabGetPO.Enabled = Shared.UserPermission.CreateJob;

            for (int index = 1; index <= 20; index++)
            {
                var podVCD = new PODModel(index, "", PODModel.TypePOD.FIELD, "");
                _PODList.Add(podVCD);
            }

            if (ProjectLabel.IsTHTrueMilk)
            {
                FirstRowHeader.Visible = _JobModel.IsFirstRowHeader = FirstRowHeader.Checked = false;
                UIControlsFuncs.HideControls( FirstRowHeader, btnHelp, btnAbout); // lblStatusCamera01
                UserNameDisplay.Text = "Người dùng: " + CurrentUser.UserName ?? "";
                LineName.Text = "Tên Line: " + Shared.Settings.RLinkName ?? "";
                if (CurrentUser.UserName == "Support")
                {
                    ErrorsLogger.Visible = true;
                }
            }
            resentDatabase.Visible = Shared.Settings.HideFunctions;

            _lblSyncJobName = new Label
            {
                AutoSize = false,
                Height = 24,
                Dock = DockStyle.Top,
                Text = "",
                Font = new Font("Microsoft Sans Serif", 9.75f)
            };
            syncDataPanel.Controls.Add(_lblSyncJobName);
            syncDataPanel.Controls.SetChildIndex(_lblSyncJobName, 0);
            Shared.OnCameraStatusChange += Shared_OnCameraStatusChange;
            MonitorCameraConnection();
            MonitorCameraConnection_CognexSupport();
            MonitorPrinterConnection();
            MonitorSensorControllerConnection();
            MonitorSerialDeviceControllerConnection();
            MonitorListenerServer();
            MonitorDatabaseConnection();

            // Tạo UC instance ẩn để trigger TriggerDebounce → ConnectWithCurrentFieldsAsync
            // (giống FrmSettingsTHTrueMilk — đây là cơ chế duy nhất connect DB đúng cách)
            var ucDbInit = new ucProductionTHTrueMilkSetting();
            ucDbInit.Visible = false;
            ucDbInit.Size = new System.Drawing.Size(1, 1);
            Controls.Add(ucDbInit);

            _ = Task.Run(() => ucProductionTHTrueMilkSetting.AutoConnectFromSettingsAsync()); // ← tự kết nối DB khi load (chạy ngầm)
            // Chạy refresh trên background thread để không block UI
            _ = Task.Run(async () =>
            {
                await Task.Delay(200); // chờ UI render xong
                RefreshAfterLogin();
            });
            SetupSyncLogTab();      // — thiết lập tab Log chờ đồng bộ
            LoadSyncLogToGrid();    //  — load dữ liệu lần đầu
            InitDgvQrIsUsed();
            InitDgvHistoryReceiveQr();

            tabControl1.Controls.Remove(tabSyncData);
        }

        private void frmJobTHTrueMilk_Shown(object sender, EventArgs e)
        {
            Shown -= frmJobTHTrueMilk_Shown;

            // Hiển thị loading khi mở lại job dở dang
            picLoading.Visible = true;
            try
            {
                if (TryAutoReopenLastActiveJob())
                    return;
                ShowIncompleteJobsPopup();
            }
            finally
            {
                picLoading.Visible = false;
            }
        }

        /// <summary>
        /// Shows a popup form with all jobs that are not completed (similar to DisplayHistory with HistoryFilter.NotFinished).
        /// Called once after the form is shown. Does nothing if there are no incomplete jobs.
        /// </summary>
        private void ShowIncompleteJobsPopup()
        {
            if (!HasIncompleteJobs() || !UserPermission.isOnline)
                return;

            using (var frm = new FrmIncompleteJobsNutri(this))
            {
                frm.ShowDialog(this);
            }
        }

        private void UpdateStatusLabelDatabase()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatusLabelDatabase()));
                return;
            }

            if (Shared.IsDatabaseConnected)
            {
                ShowLabelIcon(lblStatusDatabase, "DB", Properties.Resources.database__connected);
            }
            else
            {
                ShowLabelIcon(lblStatusDatabase, "DB", Properties.Resources.database__disconnect);
            }
        }

       

        private static bool HasIncompleteJobs()
        {
            var jobNameList = Shared.GetJobNameList();
            if (jobNameList == null || jobNameList.Count == 0)
                return false;
            var rows = SyncDataList.ReturnSyncDataList(jobNameList);
            return rows.Any(r => !r.Hoanthanh);
        }

        private void DisplayHistory(List<string> JobNameList, HistoryFilter filter = HistoryFilter.All)
        {
            var rows = SyncDataList.ReturnSyncDataList(JobNameList);
            dgvHistoryJob.Columns["MaPhieuSoanHang"].HeaderText = "M? phi?u";
            dgvHistoryJob.Rows.Clear();
            int i = 0;
            DateTime fromDate = dtpFromDate.Value.Date;
            DateTime toDate = dtpToDate.Value.Date.AddDays(1);
            foreach (var row in rows)
            {
                if (filter == HistoryFilter.Finished && !row.Hoanthanh)
                    continue;
                if (filter == HistoryFilter.NotFinished && row.Hoanthanh)
                    continue;
                if (row.LastRunTime != DateTime.MinValue && (row.LastRunTime < fromDate || row.LastRunTime >= toDate))
                    continue;
                i++;
                int rowIndex = dgvHistoryJob.Rows.Add();
                dgvHistoryJob.Rows[rowIndex].Cells["STT"].Value = i;
                dgvHistoryJob.Rows[rowIndex].Cells["MaCongViec"].Value = row.MaCongViec;
                dgvHistoryJob.Rows[rowIndex].Cells["MaPhieuSoanHang"].Value = row.MaPhieuSoanHang;
                dgvHistoryJob.Rows[rowIndex].Cells["MaSanPham"].Value = row.MaSanPham;
                dgvHistoryJob.Rows[rowIndex].Cells["SoLuongCanXuat"].Value = row.SoLuongCanXuat;
                dgvHistoryJob.Rows[rowIndex].Cells["SoLuongDongBoSaaS"].Value = row.SoLuongDongBoSaaS;
                dgvHistoryJob.Rows[rowIndex].Cells["SoLuongDongBoSAP"].Value = row.SoLuongDongBoSAP;
                dgvHistoryJob.Rows[rowIndex].Cells["HoanThanh"].Value = row.Hoanthanh ? "Đ? Hoàn Thành" : "Chưa Hoàn Thành";

            }

            Console.WriteLine("So luong: " + _JobNameList.Count);
        }

        private void DtpDate_ValueChanged(object sender, EventArgs e)
        {
            if (_JobNameList != null)
            {
                DisplayHistory(_JobNameList);
                RefreshJobList();
            }
        }

        private void DtpDateQr_ValueChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabQrIsUsed)
            {
                _ = LoadQrIsUsedAsync(txtSearchQr.Text, dtpFromDateQr.Value, dtpToDateQr.Value);
            }
        }

        private void DtpDateQrRecive_ValueChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabHistoryReciveQr)
            {
                _ = LoadHistoryReceiveQrAsync(txtSearchHistoryReceiveQr.Text, dtpFromDateQrRecive.Value, dtpToDateQrRecive.Value);
            }
        }

        private void RefreshJobList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RefreshJobList()));
                return;
            }
            listBoxJobList.Items.Clear();
            if (_JobNameList == null) return;

            string currentLineId = Shared.Settings.LineId ?? "";
            DateTime fromDate = dtpFromDate.Value.Date;
            DateTime toDate = dtpToDate.Value.Date;
            string dateFormat = (Shared.Settings.JobDateTimeFormat ?? "yyyyMMdd").Split('_')[0];

            foreach (string JobName in _JobNameList)
            {
                if (string.IsNullOrEmpty(currentLineId) || JobName.Contains(currentLineId))
                {
                    JobModel jobModel = Shared.GetJob(JobName);
                    if (jobModel != null && jobModel.JobStatus != JobStatus.Deleted)
                    {
                        string dateStr = JobName.Split('_')[0];
                        if (DateTime.TryParseExact(dateStr, dateFormat,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out DateTime jobDate))
                        {
                            if (jobDate.Date >= fromDate && jobDate.Date <= toDate)
                                listBoxJobList.Items.Add(JobName);
                        }
                        else
                            listBoxJobList.Items.Add(JobName);
                    }
                }
            }
        }
        private void SetupDataGridView()
        {
            // ── dgvItems: KHÔNG setup ở đây nữa — sẽ do LoadProductListToGrid() quản lý ──

            #region Material Table
            materialTable.Columns.Clear();
            materialTable.Columns.Add("material_number", "Mã sản phẩm");
            materialTable.Columns.Add("material_name", "Tên sản phẩm");
            materialTable.Columns.Add("qty_per_carton", "Tên LOT");
            materialTable.Columns.Add("qty", "Số lượng");
            materialTable.Columns.Add("total_qty_ctn", "Quy cách thùng");
            materialTable.Columns.Add("needed_number", "Số lượng cần in");
            materialTable.Columns.Add("printed_count", "Số lượng đã in");

            materialTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            materialTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            materialTable.MultiSelect = false;
            materialTable.ReadOnly = true;
            materialTable.AllowUserToAddRows = false;
            materialTable.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            materialTable.RowTemplate.Height = 45;
            foreach (DataGridViewColumn column in materialTable.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            materialTable.AutoResizeColumnHeadersHeight();
            #endregion
        }
        /// <summary>
        /// Setup dgvItems với cột PO — chỉ gọi khi cần hiển thị danh sách PO (btnGetInfo).
        /// </summary>
        private void SetupDgvItemsForPO()
        {
            dgvItems.Columns.Clear();
            dgvItems.Columns.Add("process_order", "Process order");
            dgvItems.Columns.Add("plant", "Mã nhà máy");
            dgvItems.Columns.Add("material_number", "Mã sản phẩm");
            dgvItems.Columns.Add("material_name", "Tên sản phẩm");
            dgvItems.Columns.Add("material_group", "Số LOT");
            dgvItems.Columns.Add("qty", "Số lượng");
            dgvItems.Columns.Add("qty_per_carton", "Quy cách thùng");
            dgvItems.Columns.Add("needed_number", "Số lượng cần in");
            dgvItems.Columns.Add("printed_count", "Số lượng đã in");

            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.MultiSelect = false;
            dgvItems.ReadOnly = false;
            dgvItems.AllowUserToAddRows = false;
            dgvItems.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvItems.RowTemplate.Height = 45;
            dgvItems.Columns["material_group"].Width = 120;
            foreach (DataGridViewColumn col in dgvItems.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvItems.AutoResizeColumnHeadersHeight();
        }
        private void GenerateCodesOffline(JobModel _JobModel)
        {
            try
            {
                if (_JobModel.IsProcessOrderMode)
                    GenerateCodesOfflinePO();

                if (_JobModel.IsReservationMode)
                    GenerateCodesOfflineRES();
            }
            catch (Exception ex)
            {
            }
        }

        private void GenerateCodesOfflineRES()
        {
            string material_doc = RES_Material_doc.Text;
            string materialNumber = RES_MaterialNumber.Text;
            string lotNumber = RES_LotNumber.Text; // Not used in this context, but kept for consistency
            int numberOfCodes = int.Parse(RES_NumberCode.Text);

            if (_matchedPrinterTemplate == "" || _matchedPrinterTemplate == "(-)")
            {
                CustomMessageBox.Show("Vui lòng chọn sản phẩm có template máy in hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string AskQuestion = Lang.AreYouSureGenerateDispatchingCodes +
                                  $"\nMã Material_doc: {material_doc}" +
                                  $"\nMã sản phẩm: {materialNumber}" +
                                  $"\nSố lượng mã cần tạo: {numberOfCodes}" +
                                  $"\nPhần trăm số dư: {Settings.AddQuantity}%";
            if (!CustomMessageBox.IsResultShow(AskQuestion)) return;

            bool isManufacturingMode = Settings.IsManufacturingMode;
            List<string> list;

            list = Base30AutoCodeGenerator.GenerateLineCodesForLoyalty(quantity: numberOfCodes);

            string tableName = isManufacturingMode ? "Manufacturing" : "DispatchingCodes";
            string fileName = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string documentsPath = CommVariables.PathDatabaseApp;

            if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);

            string filePath = Path.Combine(documentsPath, fileName);
            FileFuncs.WriteStringListToCsv(list, filePath); // Ensure this method is accessible
            databasePath = filePath;
            numberOfCodesGenerate = list.Count;

            txtFileName.Text = _JobModel.FileName
    = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_RES" + "_" + material_doc + "_" + materialNumber + "_" +
             Settings.RLinkName + "_" + Settings.LineIndex;
            _JobModel.TemplatePrint = _matchedPrinterTemplate;
        }


        private void GenerateCodesOfflinePO()
        {
            string materialNumber = MaterialNumber.Text;
            string process_order = InputPO.Text;
            string lotNumber = LOTNumber.Text; // Not used in this context, but kept for consistency
            int numberOfCodes = int.Parse(InputCodeNumber.Text);

            if (_matchedPrinterTemplate == "" || _matchedPrinterTemplate == "(-)")
            {
                CustomMessageBox.Show("Vui lòng chọn sản phẩm có template máy in hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string AskQuestion = Lang.AreYouSureGenerateDispatchingCodes +
                                          $"\nMã Material_doc: {material_doc}" +
                                          $"\nMã sản phẩm: {materialNumber}" +
                                          $"\nSố lượng mã cần tạo: {numberOfCodes}" +
                                          $"\nPhần trăm số dư: {Settings.AddQuantity}%";
            if (!CustomMessageBox.IsResultShow(AskQuestion)) return;

            bool isManufacturingMode = Settings.IsManufacturingMode;
            List<string> list;

            list = Base30AutoCodeGenerator.GenerateLineCodesForLoyalty(quantity: numberOfCodes);


            string tableName = isManufacturingMode ? "Manufacturing" : "DispatchingCodes";
            string fileName = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string documentsPath = CommVariables.PathDatabaseApp;

            if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);

            string filePath = Path.Combine(documentsPath, fileName);
            FileFuncs.WriteStringListToCsv(list, filePath); // Ensure this method is accessible
            databasePath = filePath;
            numberOfCodesGenerate = list.Count;

            txtFileName.Text = _JobModel.FileName
            = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_PO" + "_" + process_order + "_" + materialNumber + "_" +
     Settings.RLinkName + "_" + Settings.LineIndex;
            _JobModel.TemplatePrint = _matchedPrinterTemplate;
        }


        private async Task GenerateReservationCodes()
        {
            try
            {
                if (materialTable.SelectedRows.Count == 0)
                {
                    CustomMessageBox.Show("Cần chọn sản phẩm!", "Chọn sản phảm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int lineIndex = SelectedRESMaterialIndex = materialTable.SelectedRows[0].Index;
                string material_doc = Reservation.material_doc;

                var SelectedItem = Reservation.items[lineIndex];
                string materialNumber = SelectedItem.material_number;
                string materialName = SelectedItem.material_name;

                int numberOfCodes = (SelectedItem.qty / SelectedItem.qty_per_carton);

                int? surplusPercentage = Shared.Settings.AddQuantity;
                int quantity = (int)((numberOfCodes * surplusPercentage) / 100) + numberOfCodes;

                if (SelectedItem.printed_count >= quantity)
                {
                    CustomMessageBox.Show("Số lượng đã in vượt ngưỡng cho phép!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string AskQuestion = Lang.AreYouSureGenerateDispatchingCodes +
                         $"\nMaterial_doc: {material_doc}" +
                         $"\nSố lượng mã cần tạo: {quantity} (+{surplusPercentage}%)" +
                         $"\nMã sản phẩm: {materialNumber}" +
                         $"\nTên sản phẩm: {materialName}";
                if (!CustomMessageBox.IsResultShow(AskQuestion)) return;
                DisplayJobLoading(true);

                txtFileName.Text = _JobModel.FileName
     = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_RES" + "_" + material_doc + "_" + materialNumber + "_" +
      Settings.RLinkName + "_" + Settings.LineIndex;
                _JobModel.TemplatePrint = _matchedPrinterTemplate;
                Settings.ApiDomain = SelectedItem.link_web;
                if (!Settings.ApiDomains.Contains(SelectedItem.link_web))
                {
                    Settings.ApiDomains.Add(SelectedItem.link_web);
                    SaveSettings();
                }

                List<string> list = Base30AutoCodeGenerator.GenerateLineCodesForLoyalty(quantity: quantity);

                _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
                _JobModel.Reservation = Reservation;
                _JobModel.ReservationItem = SelectedItem;
                _JobModel.SelectedRESItemIndex = SelectedRESMaterialIndex;
                _JobModel.SelectedBatchIndex = 0;
                _JobModel.IsReservationMode = true;
                _JobModel.isPushedDatabase = isPushDatabase = false;

                bool isSent = isPushDatabase = await SendGeneratedCodes(list, _JobModel);

                if (!isSent) return;

                string tableName = material_doc + "_" + materialNumber; // Example table name, adjust as needed
                string fileName = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string documentsPath = CommVariables.PathDatabaseApp;

                if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);

                string filePath = Path.Combine(documentsPath, fileName);
                FileFuncs.WriteStringListToCsv(list, filePath); // Ensure this method is accessible
                databasePath = filePath;
                numberOfCodesGenerate = list.Count;
            }
            catch (Exception ex)
            {
                DisplayJobLoading(false);
                CustomMessageBox.Show("Không thể tạo mới!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProjectLogger.WriteError("Error occurred in get GenerateCodes (pushDatabase)" + ex.Message);

            }
        }

        private async Task GeneratePOCodes()
        {
            try
            {
                if (dgvItems.SelectedRows.Count == 0)
                {
                    CustomMessageBox.Show("Cần chọn PO!", "Chọn PO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int lineIndex = Settings.SelectedPOIndex = dgvItems.SelectedRows[0].Index;
                var PO = _JobModel.ProcessOrderItem = Settings.ManufacturingListPO.process_orders[lineIndex];

                string materialNumber = PO.material_number;
                string materialName = PO.material_name;
                string process_order = PO.process_order;

                int numberOfCodes = (PO.qty / PO.qty_per_carton);

                int? surplusPercentage = Shared.Settings.AddQuantity;
                int quantity = (int)((numberOfCodes * surplusPercentage) / 100) + numberOfCodes;

                if (PO.printed_count >= quantity)
                {
                    CustomMessageBox.Show("Số lượng đã in vượt ngưỡng cho phép!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (PO.batch_info.Count == 0 || PO?.batch_info[Settings.SelectedBatchIndex]?.batch == "")
                {
                    CustomMessageBox.Show("Số LOT không được rỗng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if ((Settings.FactoryCode == "1210" || Settings.FactoryCode == "1240" || Settings.FactoryCode == "1212") && PO.status != "Processing")
                {
                    CustomMessageBox.Show($"Trạng thái phiếu là {PO.status}!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if ((Settings.FactoryCode == "1260") && PO.status != "Released")
                {
                    CustomMessageBox.Show($"Trạng thái phiếu là {PO.status}!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string AskQuestion = Lang.AreYouSureGenerateDispatchingCodes +
                         $"\nProcess Order: {process_order}" +
                         $"\nSố lượng mã cần tạo: {quantity} (+{surplusPercentage}%)" +
                         $"\nPhần trăm số dư: {Settings.AddQuantity}%" +
                         $"\nMã sản phẩm: {materialNumber}" +
                         $"\nTên sản phẩm: {materialName}";
                if (!CustomMessageBox.IsResultShow(AskQuestion)) return;
                DisplayJobLoading(true);

                txtFileName.Text = _JobModel.FileName
     = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_PO" + "_" + process_order + "_" + materialNumber + "_" +
      Settings.RLinkName + "_" + Settings.LineIndex;
                _JobModel.TemplatePrint = _matchedPrinterTemplate;
                Settings.ApiDomain = PO.link_web;
                if (!Settings.ApiDomains.Contains(PO.link_web))
                {
                    Settings.ApiDomains.Add(PO.link_web);
                    SaveSettings();
                }

                List<string> list = Base30AutoCodeGenerator.GenerateLineCodesForLoyalty(quantity: quantity);

                _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
                _JobModel.IsProcessOrderMode = true;
                _JobModel.isPushedDatabase = isPushDatabase = false;

                bool isSent = isPushDatabase = await SendGeneratedCodes(list, _JobModel);

                if (!isSent) return;

                string tableName = process_order + "_" + materialNumber; // Example table name, adjust as needed
                string fileName = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string documentsPath = CommVariables.PathDatabaseApp;

                if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);

                string filePath = Path.Combine(documentsPath, fileName);
                FileFuncs.WriteStringListToCsv(list, filePath); // Ensure this method is accessible
                databasePath = filePath;
                numberOfCodesGenerate = list.Count;
            }
            catch (Exception ex)
            {
                DisplayJobLoading(false);
                CustomMessageBox.Show("Không thể tạo mã!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProjectLogger.WriteError("Error occurred in get GenerateCodes (GeneratePOCodes)" + ex.Message);

            }
        }

        public async Task<bool> SendGeneratedCodes(List<string> list, JobModel jobModel, int counts = 0)
        {
            if (jobModel.isPushedDatabase) return true;

            List<RequestGeneratedCodes.Qrcode> qrCodes = new List<RequestGeneratedCodes.Qrcode>();

            for (int i = 0; i < list.Count; i++)
            {
                if (counts > 0) counts++;
                string[] fields = list[i].Split(',');
                var code = new RequestGeneratedCodes.Qrcode
                {
                    index_qr_code = counts > 0 ? counts - 1 : i + 1,
                    unique_code = fields[1],
                    qr_code = fields[0],
                    create_date = DateTime.Now
                };
                qrCodes.Add(code);
            }
            var request = new RequestGeneratedCodes();

            if (jobModel.IsProcessOrderMode)
            {
                var PO = jobModel.ProcessOrderItem;
                request = new RequestGeneratedCodes
                {
                    job_name = jobModel.FileName,
                    process_order = PO.process_order, //  = "B2103565"
                    material_number = PO.material_number,
                    batch = PO.batch_info[Settings.SelectedBatchIndex].batch, // Can sua
                    mauf_date = PO.batch_info[Settings.SelectedBatchIndex].mauf_date,
                    expired_date = PO.batch_info[Settings.SelectedBatchIndex].expired_date,
                    print_type = "process_order",
                    qrcodes = qrCodes,
                    first_index = jobModel.FirstGeneratedCodeIndex,
                    last_index = jobModel.LastGeneratedCodeIndex
                };

                if (string.IsNullOrEmpty(PO.batch_info[Settings.SelectedBatchIndex].batch))
                {
                    CustomMessageBox.Show("Thiếu thông tin của Batch/LOT!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            if (jobModel.IsReservationMode)
            {
                var Material = jobModel.ReservationItem;
                request = new RequestGeneratedCodes
                {
                    job_name = jobModel.FileName,
                    batch = Material.batch, // Can sua
                    mauf_date = Material.mauf_date,
                    expired_date = Material.expired_date,
                    material_doc = jobModel.Reservation.material_doc,
                    material_number = Material.material_number,
                    print_type = "reservation",
                    qrcodes = qrCodes,
                    first_index = jobModel.FirstGeneratedCodeIndex,
                    last_index = jobModel.LastGeneratedCodeIndex
                };
                if (string.IsNullOrEmpty(Material.batch))
                {
                    CustomMessageBox.Show("Thiếu thông tin của Batch/LOT!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            //new Form
            //{
            //    Text = "JSON Viewer",
            //    Width = 800,
            //    Height = 600,
            //    Controls = { new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both,
            // Text = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented) } }
            //}.ShowDialog();

            var result = await ManufacturingService.PostGeneratedCodesAsync(request);
            if (result is null)
            {
                CustomMessageBox.Show("Không thể gửi dữ liệu ban đầu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!result.is_success)
            {
                CustomMessageBox.Show(result.message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisplayJobLoading(false);
                return false;
            }
            else
            {
                DisplayJobLoading(false);
                return true;
            }

            //return await SendAsync(request, url);
        }
        private async Task<bool> SendAsync(RequestGeneratedCodes data, string url)
        {
            try
            {
                var client = new HttpClient();

                var json = JsonConvert.SerializeObject(data);
                var bytes = Encoding.UTF8.GetBytes(json);

                using (var ms = new MemoryStream())
                {
                    using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                    {
                        gzip.Write(bytes, 0, bytes.Length);
                    }

                    var compressedBytes = ms.ToArray();
                    var content = new ByteArrayContent(compressedBytes);
                    content.Headers.ContentEncoding.Add("gzip");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await client.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseText);

                    var sb = new StringBuilder();
                    var result = JsonConvert.DeserializeObject<ResponseGeneratedCodes>(responseContent);

                    if (!result.is_success)
                    {
                        CustomMessageBox.Show(result.message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        DisplayJobLoading(false);
                        return false;
                    }
                    else
                    {
                        DisplayJobLoading(false);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayJobLoading(false);
                CustomMessageBox.Show("Không thể gửi dữ liệu ban đầu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProjectLogger.WriteError($"Error occurred in {url}: " + ex.Message);
            }
            return false;

        }
        internal enum DeviceExceptionType
        {
            PrinterDisconnected,
            PrinterLowInk,
            PrinterNoCartridge,
            CameraDisconnected,
            PlcDisconnected,
            QrStockLow
        }

        /// <summary>
        /// Khi thiết bị có ngoại lệ:<br/>
        /// 1. Xuất tín hiệu lỗi ra PLC (nếu đang kết nối).<br/>
        /// 2. Ghi pending_log → gửi lên R-Link Master (retry tự động nếu offline).
        /// </summary>
        internal void NotifyDeviceException(DeviceExceptionType exType, string detail)
        {
            // 1. Tín hiệu PLC
            try
            {
                if (Shared.IsSensorControllerConnected)
                    Shared.SendErrorOutputToSensorController(0);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteWarning($"[DeviceException] Gửi tín hiệu PLC lỗi: {ex.Message}");
            }

           
        }
        private frmLoadingUi _loadingJob = null;

        private void DisplayJobLoading(bool isLoading, string message = "Đang lưu job...")
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => DisplayJobLoading(isLoading, message)));
                return;
            }

            if (isLoading)
            {
                // Đóng loading cũ nếu còn
                if (_loadingJob != null && !_loadingJob.IsDisposed)
                {
                    _loadingJob.Close();
                    _loadingJob.Dispose();
                    _loadingJob = null;
                }

                // Disable tất cả controls phía sau
                foreach (Control c in Controls)
                {
                    if (c is Form) continue;
                    c.Enabled = false;
                }

                _loadingJob = new frmLoadingUi("TẠO JOB", message);
                _loadingJob.Show(this);
                _loadingJob.BringToFront();
                _loadingJob.Refresh();
            }
            else
            {
                if (_loadingJob != null && !_loadingJob.IsDisposed)
                {
                    _loadingJob.Close();
                    _loadingJob.Dispose();
                }
                _loadingJob = null;

                // Enable lại tất cả controls
                foreach (Control c in Controls)
                {
                    c.Enabled = true;
                }
            }
        }

        //var responseText = await response.Content.ReadAsStringAsync();
        //Console.WriteLine(responseText);

        //var sb = new StringBuilder();
        //sb.AppendLine("=== HTTP RESPONSE ===");
        //sb.AppendLine($"Status Code : {(int)response.StatusCode} {response.ReasonPhrase}");
        //sb.AppendLine("Headers:");
        //foreach (var header in response.Headers)
        //{
        //    sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        //}
        //foreach (var header in response.Content.Headers)
        //{
        //    sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        //}
        //sb.AppendLine("Body:");
        //sb.AppendLine(responseText);

        // Show as MessageBox
        //MessageBox.Show(sb.ToString(), "HTTP Response", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private byte[] Compress(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress, leaveOpen: false)) // ensure proper close
                {
                    gzip.Write(bytes, 0, bytes.Length);
                }
                return output.ToArray(); // This will now include the full compressed stream
            }
        }

        private void InitEvents()
        {
            dgvItems.CellClick += dataGridView1_CellClick;
            dgvItems.SelectionChanged += (s, e) => UpdateCalculatedBoxCount();
            btnSearchProduct.Click += BtnSearchProduct_Click;
            cbcSelectProduct.SelectedIndexChanged += CbcSelectProduct_SelectedIndexChanged;
            btnSyncMarkQr.Click += BtnSyncMarkQr_Click;
            cbcSelectProduct.MouseClick += (s, e) =>
            {
                cbcSelectProduct.Focus();
                cbcSelectProduct.DroppedDown = true;
            };

            // ── cuzTextBox3 hiển thị số hộp tính được ──────────────────────────
            txtTotalProduct.ReadOnly = true;
            txtTotalProduct.Text = "0";
          //  txtTotalProduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold);
            txtTotalProduct.ForeColor = System.Drawing.Color.DimGray;
            txtTotalProduct.BackColor = System.Drawing.Color.WhiteSmoke;

            // ── Đường kẻ phân tách giữa "Hạn sử dụng" và "Model camera" ──
            var lblSep = new Label
            {
                BackColor = Color.Silver,
                Height = 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel5.Controls.Add(lblSep);

            Action positionSep = () =>
            {
                if (label57 != null) // label57 = "Hạn sử dụng:"
                    lblSep.Location = new Point(12, label57.Bottom + 12);
                lblSep.Width = panel5.ClientSize.Width - 24;
            };
            panel5.Resize += (s, e) => positionSep();
            positionSep();

            // ── Giãn spacing theo chiều cao panel5 ──
            var panel5OriginalY = new Dictionary<Control, int>();
            foreach (Control c in panel5.Controls)
            {
                if (c is Label && c != lblProductTitle && c != lblSep)
                    panel5OriginalY[c] = c.Location.Y;
            }
            if (rtbGtinProduct != null)
                panel5OriginalY[rtbGtinProduct] = rtbGtinProduct.Location.Y;


            // ── Layout panel5: center content + rowSpacing=45 ──
            Action layoutPanel5Content = () =>
            {
                if (panel5 == null) return;

                int titleBottom = lblProductTitle.Bottom;  // = 37
                int rowSpacing = 60;
                int separatorHeight = 2;

                var rows = new[] { label53, label54, label55, label56, label57, label65, label63 };
                var values = new Control[] { lblProductId, lblProductName, rtbGtinProduct,
                                  lblProductVolume, lblProductExp, lblProgramCamera, lblTemplatePrinter };

                int totalContentH = rows.Length * rowSpacing + 12 + separatorHeight;
                int availH = panel5.ClientSize.Height - titleBottom;
                int centerOffset = Math.Max(5, (availH - totalContentH) / 2);

                int currentY = titleBottom + centerOffset;

                for (int i = 0; i < rows.Length; i++)
                {
                    if (rows[i] != null) rows[i].Location = new Point(rows[i].Location.X, currentY);
                    if (values[i] != null) values[i].Location = new Point(values[i].Location.X, currentY + 2);

                    if (i == 4) // Sau "Hạn sử dụng" → thêm gap separator
                        currentY += rowSpacing + 12;
                    else
                        currentY += rowSpacing;
                }

                // Separator: full width, positioned dưới label57
                if (lblSep != null && label57 != null)
                {
                    lblSep.Location = new Point(12, label57.Bottom + 12);
                    lblSep.Width = panel5.ClientSize.Width - 24;  // full width, giữ nguyên
                }
            };
            panel5.Resize += (s, e) => layoutPanel5Content();
            layoutPanel5Content();


            // Nút "✕" clear trong txtSearchProduct
            var btnClear = new Label
            {
                Text = "✕",
                ForeColor = System.Drawing.Color.Gray,
                BackColor = System.Drawing.Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10f),
                Cursor = Cursors.Hand,
                AutoSize = false,
                Size = new System.Drawing.Size(22, txtSearchProduct.Height - 8),
                Visible = false
            };
            btnClear.Location = new System.Drawing.Point(
                txtSearchProduct.Right - btnClear.Width - 3,
                txtSearchProduct.Top + 5 );
            btnClear.Click += (s, ev) =>
            {
                txtSearchProduct.Text = "";
                LoadProductListToGrid();
            };
            txtSearchProduct.TextChanged += (s, ev) =>
            {
                btnClear.Visible = !string.IsNullOrEmpty(txtSearchProduct.Text);
            };
            txtSearchProduct.Parent.Controls.Add(btnClear);
            btnClear.BringToFront();


            OnSerialDeviceReadDataChange += Shared_OnSerialDeviceReadDataChange;
            OnSyncDataParameterChange += Shared_OnSyncDataParameterChange;
            OnSyncCheckDataParameterChange += Shared_OnSyncCheckDataParameterChange;
            Shared.OnCameraStatusChange += Shared_OnCameraStatusChange;
            Shared.OnDatabaseStatusChange += Shared_OnDatabaseStatusChange;
            btnRefreshListProgramFroCameraKeyence.Click += BtnRefreshProgramsFromCameraKeyence_Click;
            GetMaterialDoc.Click += ActionResult;
            btnGetInfo.Click += ActionResult;
            saveJobTH.Click += ActionResult;
            saveJobReservation.Click += ActionResult;
            SyncDataBtn.Click += ActionResult;
            savePOOffline.Click += ActionResult;
            CreateRESOffline.Click += ActionResult;
            StopSyncData.Click += ActionResult;
            cbbHisFilterType.SelectedIndexChanged += ActionResult;
            dtpFromDate.ValueChanged += DtpDate_ValueChanged;
            dtpToDate.ValueChanged += DtpDate_ValueChanged;
            dtpFromDateQr.ValueChanged += DtpDateQr_ValueChanged;
            dtpToDateQr.ValueChanged += DtpDateQr_ValueChanged;
            dtpFromDateQrRecive.ValueChanged += DtpDateQrRecive_ValueChanged;
            dtpToDateQrRecive.ValueChanged += DtpDateQrRecive_ValueChanged;
            editJobBtn.Click += ActionResult;
            ErrorsLogger.Click += ActionResult;

            _TimerDateTime.Tick += TimerDateTime_Tick;
            btnGennerate.Click += ActionResult;
            radCanRead.CheckedChanged += ActionResult;
            radCanRead.EnabledChanged += JobType_EnabledChanged;
            radStaticText.CheckedChanged += ActionResult;
            radStaticText.EnabledChanged += JobType_EnabledChanged;
            radDatabase.CheckedChanged += ActionResult;
            radDatabase.EnabledChanged += JobType_EnabledChanged;

            radRSeries.CheckedChanged += ActionResult;
            radOther.CheckedChanged += ActionResult;
            FirstRowHeader.CheckedChanged += ActionResult;
            radRSeries.CheckedChanged += RadioButton_CheckedChanged;
            radOther.CheckedChanged += RadioButton_CheckedChanged;

            radAfterProduction.CheckedChanged += RadioButton_CheckedChanged;
            radAfterProduction.CheckedChanged += ActionResult;
            radAfterProduction.EnabledChanged += JobType_EnabledChanged; ;
            radOnProduction.CheckedChanged += RadioButton_CheckedChanged;
            radOnProduction.CheckedChanged += ActionResult;
            radOnProduction.EnabledChanged += JobType_EnabledChanged;
            radVerifyAndPrint.CheckedChanged += RadioButton_CheckedChanged;
            radVerifyAndPrint.CheckedChanged += ActionResult;
            radVerifyAndPrint.EnabledChanged += JobType_EnabledChanged;
            txtStaticText.TextChanged += TxtStaticText_TextChanged; ;
            txtDirectoryDatabse.TextChanged += TxtDirectoryDatabse_TextChanged; ;
            txtFileName.TextChanged += TxtFileName_TextChanged; ;
            txtPODFormat.TextChanged += TxtPODFormat_TextChanged;
            DomainInput.SelectedIndexChanged += DomainInput_SelectedIndexChanged;
            DomainInput.TextChanged += DomainInput_TextChanged;
            DomainRevInput.TextChanged += DomainRevInput_TextChanged;

            txtSearch.TextChanged += TxtSearch_TextChanged; ;
           // txtSearchTemplate.TextChanged += TxtSearchTemplate_TextChanged;

            btnPODFormat.Click += ActionResult;

            btnSettings.Click += ActionResult;
            listBoxJobList.SelectedIndexChanged += ActionResult;
            listBoxPrintProductTemplate.SelectedIndexChanged += ActionResult;
            btnRefesh.Click += ActionResult;
            btnImportDatabase.Click += ActionResult;
            Shared.OnLanguageChange += Shared_OnLanguageChange;

            Load += FrmJob_Load;
            Shown += frmJobTHTrueMilk_Shown;
            tabControl1.SelectedIndexChanged += ActionResult;
            tabGetPO.Click += ActionResult;

            btnExit.Click += BtnClose_Click;
            btnNext.Click += ActionResult;
            btnSave.Click += ActionResult;
            btnAbout.Click += ActionResult;
            btnHelp.Click += ActionResult;
            btnRestart.Click += ActionResult;
            btnDelete.Click += ActionResult;
            btnRefeshTemplate.Click += ActionResult;
            radCanRead.CheckedChanged += RadioButton_CheckedChanged;
            radDatabase.CheckedChanged += RadioButton_CheckedChanged;
            radStaticText.CheckedChanged += RadioButton_CheckedChanged;
            BtnViewLog.Click += BtnViewLog_Click;

            cboSupportForCamera.DrawMode = DrawMode.OwnerDrawVariable;
            cboSupportForCamera.Height = 40;
            cboSupportForCamera.DropDownHeight = 150;
            cboSupportForCamera.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSupportForCamera.DrawItem += ComboBoxCustom.MyComboBox_DrawItem;
            cboSupportForCamera.MeasureItem += ComboBoxCustom.Cbo_MeasureItem;
            cboSupportForCamera.SelectedIndexChanged += CboSupportForCamera_SelectedIndexChanged;
            listBoxJobList.DrawItem += ListBoxJobList_DrawItem;

            Shared.OnPrintingStateChange += Shared_OnPrintingStateChange;
            Shared.OnPrinterStatusChange += Shared_OnPrinterStatusChange;
            Shared.OnPrinterDataChange += Shared_OnPrinterDataChange;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnSensorControllerChangeEvent += Shared_OnSensorControllerChangeEvent;

            //Camera Event
            Shared.OnCameraStatusChange += Shared_OnCameraStatusChange;
            Shared.OnCameraTriggerOnChange += Shared_OnCameraTriggerOnChange;
            Shared.OnCameraTriggerOffChange += Shared_OnCameraTriggerOffChange;
            Shared.OnCameraOutputSignalChange += Shared_OnCameraOutputSignalChange;
            AutoAddSufixEvent += FrmJob_AutoAddSufixEvent;
            DMCamera.UpdateLabelStatusEvent += UpdateLabelStatusEvent;
            // ISCamera.UpdateLabelStatusEvent += UpdateLabelStatusEvent;

            cuzButtonPurge.Click += CuzButtonPurge_Click;

            txtTotalQR.KeyPress += TxtTotalQR_KeyPress;
            txtTotalQR.Leave += TxtTotalQR_Leave;
            txtTotalQR.TextChanged += TxtTotalQR_TextChanged;
            tabControl1.SelectedIndexChanged += TabControl1_QrIsUsed_SelectedIndexChanged;
            tabControl1.SelectedIndexChanged += TabControl1_HistoryReceiveQr_SelectedIndexChanged;
            txtSearchQr.TextChanged += TxtSearchQr_TextChanged;
            btnRefreshSearchQr.Click += BtnRefreshSearchQr_Click;

            // Helper: đặt nút "✕" sát cạnh phải + căn giữa dọc theo textbox (responsive)
            void PositionClearButton(Control btn, Control box) =>
                btn.Location = new System.Drawing.Point(
                    box.Right - btn.Width - 3,
                    box.Top + (box.Height - btn.Height) / 2);

            // Nút "✕" clear trong txtSearchQr
            var btnClearSearchQr = new Label
            {
                Text = "✕",
                ForeColor = System.Drawing.Color.Gray,
                BackColor = System.Drawing.Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10f),
                Cursor = Cursors.Hand,
                AutoSize = false,
                Size = new System.Drawing.Size(22, txtSearchQr.Height - 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = false
            };
            PositionClearButton(btnClearSearchQr, txtSearchQr);
            btnClearSearchQr.Click += (s, ev) =>
            {
                txtSearchQr.Text = "";
                _ = LoadQrIsUsedAsync("", dtpFromDateQr.Value, dtpToDateQr.Value);
            };
            txtSearchQr.TextChanged += (s, ev) =>
            {
                btnClearSearchQr.Visible = !string.IsNullOrEmpty(txtSearchQr.Text);
            };
            txtSearchQr.Parent.Controls.Add(btnClearSearchQr);
            btnClearSearchQr.BringToFront();
            PositionClearButton(btnClearSearchQr, txtSearchQr);
            txtSearchQr.SizeChanged += (s, ev) => PositionClearButton(btnClearSearchQr, txtSearchQr);

            btnRefreshHistoryReceiveQr.Click += (s, e) => _ = LoadHistoryReceiveQrAsync(txtSearchHistoryReceiveQr.Text, dtpFromDateQrRecive.Value, dtpToDateQrRecive.Value);

            btnRefreshHistoryQr.Click += (s, e) => _ = LoadHistoryReceiveQrAsync(txtSearchHistoryReceiveQr.Text, dtpFromDateQrRecive.Value, dtpToDateQrRecive.Value);

            var btnClearHistoryReceiveQr = new Label
            {
                Text = "✕",
                ForeColor = System.Drawing.Color.Gray,
                BackColor = System.Drawing.Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10f),
                Cursor = Cursors.Hand,
                AutoSize = false,
                Size = new System.Drawing.Size(22, txtSearchHistoryReceiveQr.Height - 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = !string.IsNullOrEmpty(txtSearchHistoryReceiveQr.Text)
            };
            PositionClearButton(btnClearHistoryReceiveQr, txtSearchHistoryReceiveQr);
            btnClearHistoryReceiveQr.Click += (s, ev) =>
            {
                txtSearchHistoryReceiveQr.Text = "";
                _ = LoadHistoryReceiveQrAsync("", dtpFromDateQrRecive.Value, dtpToDateQrRecive.Value);
            };
            txtSearchHistoryReceiveQr.TextChanged += (s, ev) =>
            {
                btnClearHistoryReceiveQr.Visible = !string.IsNullOrEmpty(txtSearchHistoryReceiveQr.Text);
            };
            txtSearchHistoryReceiveQr.Parent.Controls.Add(btnClearHistoryReceiveQr);
            btnClearHistoryReceiveQr.BringToFront();
            PositionClearButton(btnClearHistoryReceiveQr, txtSearchHistoryReceiveQr);
            txtSearchHistoryReceiveQr.SizeChanged += (s, ev) => PositionClearButton(btnClearHistoryReceiveQr, txtSearchHistoryReceiveQr);
            dgvHistoryQr.CellDoubleClick += dgvHistoryReceiveQr_CellDoubleClick;
            dgvHistoryQr.CellClick += dgvHistoryReceiveQr_CellClick;

            // ── lblProductPlaceholder + picProductImage: đặt dưới header "Hình ảnh" ──
            if (pnlProductCard != null && label37 != null)
            {
                Action fitImageArea = () =>
                {
                    int headerBottom = label37.Bottom;
                    int areaW = pnlProductCard.ClientSize.Width;
                    int areaH = pnlProductCard.ClientSize.Height - headerBottom;

                    // picProductImage: nằm dưới header, kéo dài full
                    if (picProductImage != null)
                    {
                        picProductImage.Location = new Point(0, headerBottom);
                        picProductImage.Size = new Size(areaW, areaH);
                        picProductImage.SizeMode = PictureBoxSizeMode.Zoom;
                        picProductImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    }

                    // lblProductPlaceholder: cùng vị trí với picProductImage
                    if (lblProductPlaceholder != null)
                    {
                        lblProductPlaceholder.Left = 0;
                        lblProductPlaceholder.Top = headerBottom;
                        lblProductPlaceholder.Width = areaW;
                        lblProductPlaceholder.Height = areaH;
                        lblProductPlaceholder.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        lblProductPlaceholder.BackColor = Color.Transparent;
                        lblProductPlaceholder.ForeColor = Color.Gray;
                        lblProductPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
                    }
                };

                pnlProductCard.Resize += (s, e) => fitImageArea();
                fitImageArea();
            }
        }

        private void DomainRevInput_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.ApiDomain = DomainRevInput.Text;
        }

        private void DomainInput_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.ApiDomain = DomainInput.Text;
        }

        private void DomainInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DomainInput.SelectedItem != null)
            {
                Shared.Settings.ApiDomain = DomainInput.SelectedItem.ToString();
            }
        }
        private bool _isFormattingTotalQR = false;

        private void TxtTotalQR_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Cho phép số, dấu chấm/phẩy (decimal), và control keys
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)
                && e.KeyChar != '.' && e.KeyChar != ',')
                e.Handled = true;
        }

        private void TxtTotalQR_TextChanged(object sender, EventArgs e)
        {
            if (_isFormattingTotalQR) return;
            _isFormattingTotalQR = true;
            try
            {
                string raw = txtTotalQR.Text.Trim();
                if (string.IsNullOrWhiteSpace(raw)) return;
                // Chỉ giữ lại ký tự số, chấm, phẩy
                string cleaned = new string(raw.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                if (cleaned != raw)
                    txtTotalQR.Text = cleaned;
            }
            finally
            {
                _isFormattingTotalQR = false;
            }
            UpdateCalculatedBoxCount();
        }
        private void TxtTotalQR_Leave(object sender, EventArgs e)
        {
            string raw = txtTotalQR.Text.Trim().Replace(",", ".");
            if (double.TryParse(raw, out double val) && val > 9999)
            {
                CuzMessageBox.Show("Số tấn không được vượt quá 9.999!",
                    "Số tấn vượt giới hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTotalQR.Text = "9999";
                txtTotalQR.Focus();
            }
        }
        private void UpdateCalculatedBoxCount()
        {
            try
            {
                string rawTon = txtTotalQR.Text.Trim().Replace(",", ".");
                if (!double.TryParse(rawTon, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double ton) || ton <= 0)
                {
                    txtTotalProduct.Text = "0";
                    return;
                }

                var selProduct = GetSelectedProduct();
                if (selProduct == null || !selProduct.Volume.HasValue || selProduct.Volume <= 0)
                {
                    txtTotalProduct.Text = "0";
                    return;
                }

                int totalBoxes = (int)(ton * 1_000_000 / selProduct.Volume.Value);
                txtTotalProduct.Text = totalBoxes.ToString("N0");
            }
            catch
            {
                txtTotalProduct.Text = "-";
            }
        }
        private void InitDomainInputComboBox()
        {
            DomainInput.Items.Clear();
            if (Settings.ApiDomains != null && Settings.ApiDomains.Count > 0)
            {
                foreach (var domain in Settings.ApiDomains)
                {
                    DomainInput.Items.Add(domain);
                }
                // Select the current ApiDomain if it exists in the list
                if (!string.IsNullOrEmpty(Settings.ApiDomain) && Settings.ApiDomains.Contains(Settings.ApiDomain))
                {
                    DomainInput.SelectedItem = Settings.ApiDomain;
                }
                else if (DomainInput.Items.Count > 0)
                {
                    DomainInput.SelectedIndex = 0;
                }
            }
        }
        private void Shared_OnDatabaseStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelDatabase();
        }
        private void Shared_OnSyncCheckDataParameterChange(object sender, EventArgs e)
        {
            try
            {
                if (sender is SyncDataParams ParamsName)
                {
                    switch (ParamsName.DataType)
                    {
                        case SyncDataType.SAPSuccess:
                            NumberOfCheckSentSAP++;
                            break;
                        case SyncDataType.SentSuccess:
                            NumberOfCheckSentSuccess++;
                            break;
                        default:
                            break;
                    }
                }

                if (InvokeRequired)
                {
                    Invoke(new Action(() => syncCheckedProgress.Value = NumberOfCheckSentSuccess * 100 / NumberChecked));
                }
                else
                {
                    syncCheckedProgress.Value = NumberOfCheckSentSuccess * 100 / NumberChecked;
                }

                // Call this where needed:
                if (CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes && (NumberOfCheckSentSuccess == NumberChecked))
                {
                    OnSentPrintedCodesCompleted();
                }

            }
            catch (Exception ex)
            {
            }

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

                // Chỉ xử lý batch info khi đang ở chế độ PO (ManufacturingListPO != null)
                if (Settings?.ManufacturingListPO?.process_orders == null) return;

                int lineIndex = Settings.SelectedPOIndex = dgvItems.SelectedRows[0].Index;
                if (lineIndex >= Settings.ManufacturingListPO.process_orders.Count) return;

                var BatchInfos = Settings.ManufacturingListPO.process_orders[lineIndex]?.batch_info;
                SelectBatchInfo(BatchInfos, lineIndex);
            }
            catch (Exception ex)
            {
            }
        }

        private void SelectBatchInfo(List<ResponseProcessOrder.BatchInfo> batchInfos, int lineIndex)
        {
            using (var frmBatch = new frmBatchInfo(batchInfos, Settings.SelectedBatchIndex))
            {
                if (frmBatch.ShowDialog() == DialogResult.OK)
                {
                    int SelectedLineIndex = dgvItems.Rows[0].Index;
                    int selectedIndex = frmBatch.SelectedBatchIndex.Value;
                    Settings.SelectedBatchIndex = selectedIndex;
                    //if (Settings.FactoryCode == "1260")
                    //{
                    //    InitPOListCombo(Settings.ManufacturingListPO);
                    //}
                    SetComboBoxCellIndex(dgvItems, lineIndex, "material_group", selectedIndex);
                    dgvItems.ClearSelection();
                    dgvItems.Rows[lineIndex].Selected = true;
                    dgvItems.CurrentCell = dgvItems.Rows[lineIndex].Cells[0];

                }
            }
        }

        private void Shared_OnSerialDeviceReadDataChange(object sender, EventArgs e)
        {
            if ((Shared.OperStatus == OperationStatus.Running && Shared.OperStatus == OperationStatus.Processing)) return;
            try
            {
                if (sender is DetectModel detectModel)
                {
                }
            }
            catch (Exception)
            {
            }
        }

        private void Shared_OnSyncDataParameterChange(object sender, EventArgs e)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => syncPrintedProgress.Value = CurrentJob.NumberOfSAPSentCodes * 100 / CurrentJob.NumberOfPrintedCodes));
                }
                else
                {
                    syncPrintedProgress.Value = CurrentJob.NumberOfSAPSentCodes * 100 / CurrentJob.NumberOfPrintedCodes;
                }
                // Call this where needed:
                //if (Shared.CurrentJob.NumberOfPrintedCodes == Shared.CurrentJob.NumberOfSAPSentCodes)
                //{
                //    OnSentPrintedCodesCompleted();
                //}
                if (CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes && (NumberOfCheckSentSuccess == NumberChecked))
                {
                    OnSentPrintedCodesCompleted();
                }
            }
            catch (Exception ex)
            {
            }

        }

        private void OnSentPrintedCodesCompleted()
        {
            try
            {
                _printedDataProcess?.Stop();
                _verificationDataProcess?.Stop();
                if (_syncingFromPopup)
                    return;
                Task.Run(() =>
                {
                    // Heavy work off the UI thread
                    List<string> jobNameList = Shared.GetJobNameList();

                    this.BeginInvoke(new Action(() =>
                    {
                        UIControlsFuncs.HideControls(syncDataPanel);
                        UIControlsFuncs.EnableAllTabsSelection(tabControl1);
                        UIControlsFuncs.EnableControls(dgvHistoryJob, pnlMenu, SyncDataBtn, cbbHisFilterType);
                        DisplayHistory(jobNameList);
                    }));
                });

            }
            catch (Exception ex)
            {
            }

        }

        /// <summary>
        /// Stops the current sync (printing + verification processors). Used by popup "D?ng đ?ng b?".
        /// </summary>
        public void StopCurrentSync()
        {
            try
            {
                _printedDataProcess?.Stop();
                _verificationDataProcess?.Stop();
            }
            catch { }
        }

        /// <summary>
        /// Shows or hides syncDataPanel when syncing from popup, and updates the current job name label.
        /// </summary>
        public void ShowSyncPanelForPopupSync(bool show, string jobName = null)
        {
            void DoUpdate()
            {
                bool tabExists = tabControl1.TabPages.Contains(tabSyncData);
                if (show && tabExists)
                {
                    int idx = tabControl1.TabPages.IndexOf(tabSyncData);
                    if (idx >= 0) tabControl1.SelectedIndex = idx;
                    UIControlsFuncs.ShowControls(syncDataPanel);
                    if (_lblSyncJobName != null)
                        _lblSyncJobName.Text = string.IsNullOrEmpty(jobName) ? "" : "Đang đ?ng b?: " + jobName;
                }
                else if (!show && tabExists)
                {
                    UIControlsFuncs.HideControls(syncDataPanel);
                    if (_lblSyncJobName != null)
                        _lblSyncJobName.Text = "";
                }
            }
            if (InvokeRequired)
                Invoke(new Action(DoUpdate));
            else
                DoUpdate();
        }

        /// <summary>
        /// Syncs a single job (SendGeneratedCodes + printing/verification processors) and waits until complete or cancelled.
        /// Used by FrmIncompleteJobsNutri to sync all incomplete jobs. Returns false if cancelled or failed.
        /// </summary>
        public async Task<bool> SyncSingleJobAsync(JobModel job, CancellationToken ct)
        {
            if (job == null) return false;
            _syncingFromPopup = true;
            try
            {
                CurrentJob = job;
                string checkedDataPath = CommVariables.PathCheckedResult + job.CheckedResultPath;
                string sentCheckedDataPath = CommVariables.PathSentDataChecked + job.CheckedResultPath;

                var checkedCount = FileFuncs.ReadCodeData(checkedDataPath).Count;
                NumberChecked = checkedCount > 0 ? checkedCount - 1 : 0;
                NumberOfCheckSentSuccess = FileFuncs.ReadCodeData(sentCheckedDataPath).Count(item => item.Length > 8 && item[8].Equals("Sent", StringComparison.OrdinalIgnoreCase));

                if ((job.NumberOfPrintedCodes == job.NumberOfSAPSentCodes || job.NumberOfPrintedCodes == 0) && NumberChecked == NumberOfCheckSentSuccess)
                    return true;

                var listQrcodes = FileFuncs.ReadStringListFromCsv(job.DirectoryDatabase);
                PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder);

                bool isSent = job.isPushedDatabase = isPushDatabase = await SendGeneratedCodes(listQrcodes, job);
                job.SaveFile();
                if (!isSent) return false;

                //if (_printedDataProcess != null) _printedDataProcess.Stop();
                //if (_verificationDataProcess != null) _verificationDataProcess.Stop();

                //string dataPath = job.DirectoryDatabase;
                //string sentDataPath = CommVariables.PathSentDataPrinted + job.PrintedResponePath;
                //string url = ManufacturingApis.postPrintedDataUrl();
                //_printedDataProcess = THTrueMilkProcessorFactory.CreatePrintingProcessor(sentDataPath, url, dataPath);
                //_printedDataProcess.Start();

                //string urlChecked = ManufacturingApis.postCheckedDataUrl();
                //_verificationDataProcess = THTrueMilkProcessorFactory.CreateVerificationProcessor(sentCheckedDataPath, urlChecked, dataPath);
                //_verificationDataProcess.Start();

                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    if (CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes && (NumberChecked == 0 || NumberOfCheckSentSuccess == NumberChecked))
                        break;
                    await Task.Delay(300, ct);
                }

                //_printedDataProcess?.Stop();
                //_verificationDataProcess?.Stop();
                return true;
            }
            catch (Exception)
            {
                //_printedDataProcess?.Stop();
                //_verificationDataProcess?.Stop();
                return false;
            }
            finally
            {
                _syncingFromPopup = false;
            }
        }

        private void CuzButtonPurge_Click(object sender, EventArgs e)
        {
            try
            {
                if (_IsProcessing || listBoxJobList.SelectedItem == null || _JobModel.FileName == null)
                {
                    CuzMessageBox.Show("Please select valid Job to Purge !", "Purge Job", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var resPurgeDialog = CuzMessageBox.Show("Do you want to Purge this Job !", "Purge Job", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (resPurgeDialog == DialogResult.OK)
                {
                    PurgeJob(_JobModel);
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateLabelStatusEvent(object sender, EventArgs e)
        {
            // UpdateStatusLabelCamera(); //spare
        }

        private void FrmJob_AutoAddSufixEvent(object sender, EventArgs e)
        {
            //  ISCamera.AutoAddSuffixes(ISCamera._CameraModel);
            Shared.RaiseAddSuffix(Shared.Settings.CameraList.FirstOrDefault());
        }

        private void DebugVirtual()
        {
            BtnViewLog.Visible = true;

#if DEBUG
            var pnlDebug = new Panel
            {
                Height = 36,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(255, 255, 200),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblDebug = new Label
            {
                Text = "🛠 Debug reset time (HH:mm):",
                AutoSize = true,
                Location = new Point(6, 9),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            _txtDebugResetTime = new TextBox
            {
                Text = "",
                Width = 60,
                Location = new Point(210, 7),
                Font = new Font("Segoe UI", 9f)
            };

            var btnApply = new Button
            {
                Text = "▶ Apply",
                Width = 64,
                Height = 24,
                Location = new Point(278, 5),
                Font = new Font("Segoe UI", 8f),
                BackColor = Color.FromArgb(0, 171, 230),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += (s, e) =>
            {
                // TimeSpan dùng "hh\\:mm" (không có HH)
                if (TimeSpan.TryParseExact(_txtDebugResetTime.Text.Trim(), "hh\\:mm",
                    System.Globalization.CultureInfo.InvariantCulture, out TimeSpan ts))
                {
                    _lastDeltaResetDate = DateTime.MinValue;
                    _lastMode2DebugTime = DateTime.MinValue;
                    _FormMainPC?.ResetDebugNsxHsdTriggered();
#if DEBUG
                    _debugNsxHsdTime = DateTime.Today.Add(ts);
                    _FormMainPC.DebugNsxHsdTime = _debugNsxHsdTime;
#endif
                    lock (_batchQrLock)
                    {
                        if (string.IsNullOrEmpty(_currentBatchQrCode))
                            _currentBatchQrCode = "(debug-placeholder)";
                    }
                    ProjectLogger.WriteInfo(
                        $"[DEBUG] Reset time set → {DateTime.Today.Add(ts):HH:mm:ss} | all guards cleared");

                    btnApply.BackColor = Color.SeaGreen;
                    // ── Trigger ngay ──
                    TimerMidnightReset_Tick(this, EventArgs.Empty);
                    var t = new System.Windows.Forms.Timer { Interval = 1500 };
                    t.Tick += (ss, ee) =>
                    {
                        btnApply.BackColor = Color.FromArgb(0, 171, 230);
                        t.Stop(); t.Dispose();
                    };
                    t.Start();
                }
                else
                {
                    // Hiện lỗi parse
                    btnApply.BackColor = Color.Red;
                    btnApply.Text = "⚠ HH:mm";
                    var t = new System.Windows.Forms.Timer { Interval = 1500 };
                    t.Tick += (ss, ee) =>
                    {
                        btnApply.BackColor = Color.FromArgb(0, 171, 230);
                        btnApply.Text = "▶ Apply";
                        t.Stop(); t.Dispose();
                    };
                    t.Start();
                }
            };

            pnlDebug.Controls.Add(lblDebug);
            pnlDebug.Controls.Add(_txtDebugResetTime);
            pnlDebug.Controls.Add(btnApply);
            Controls.Add(pnlDebug);
            pnlDebug.BringToFront();
#endif
        }

        private async void MonitorListenerServer()
        {
            try
            {
                await StartListenerServer();
            }
            catch (Exception exx)
            {
                System.Windows.MessageBox.Show("ERROR: " + exx);
            }
        }

        private async Task StartListenerServer()
        {
            var url = new StringBuilder("http://");
            url.Append(Shared.GetLocalIPAddress());
            url.Append("/");
            string[] prefixes = new string[] { url.ToString() };

            var server = new CameraListenerServer(prefixes);
            await server.StartAsync();
        }

        private void PrinterSupport(bool printerSub, bool isAlert = true)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => PrinterSupport(printerSub)));
                return;
            }
            if (countSkipFirstAlert == 0)
            {
                countSkipFirstAlert++;
                return;
            }
            string content = printerSub ? SupportForPrinter : Standalone;

            if (isAlert) CuzAlert.Show(content, Alert.enmType.Info, new Size(500, 90), new Point(Location.X, Location.Y), Size);
            if (printerSub)
            {
                radDatabase.Checked = true;
                radCanRead.Enabled = false;
                radStaticText.Enabled = false;
                radDatabase.Enabled = true;
                tblJobType.Enabled = true;
                DatabaseChecked(true, true);
            }
            else
            {
                radCanRead.Enabled = true;
                radCanRead.Checked = true;
                if (_JobModel.CompareType == CompareType.Database) _JobModel.JobType = JobType.StandAlone;
                radStaticText.Enabled = true;
                radDatabase.Enabled = true;
                tblJobType.Enabled = false;
                DatabaseChecked(false, true);
            }

        }

        private void DatabaseChecked(bool isChecked, bool isTemplate)
        {
            if (isChecked)
            {
                txtDirectoryDatabse.Enabled = true;
                txtPODFormat.Enabled = true;

                if (isTemplate)
                {
                    txtSearchTemplate.Enabled = true;
                    btnRefeshTemplate.Enabled = true;
                    listBoxPrintProductTemplate.Enabled = true;
                    listBoxPrintProductTemplate.ClearSelected();
                    txtSearchTemplate.BackColor = Color.White;
                }

                btnImportDatabase.Enabled = true;
                btnPODFormat.Enabled = true;

                txtDirectoryDatabse.BackColor = Color.White;
                txtPODFormat.BackColor = Color.White;

                txtStaticText.Text = "";
                txtDirectoryDatabse.Text = "";
                txtPODFormat.Text = "";
            }
            else
            {
                txtDirectoryDatabse.Enabled = false;
                txtPODFormat.Enabled = false;

                if (isTemplate)
                {
                    txtSearchTemplate.Enabled = false;
                    btnRefeshTemplate.Enabled = false;
                    listBoxPrintProductTemplate.Enabled = false;
                    listBoxPrintProductTemplate.ClearSelected();
                    txtSearchTemplate.BackColor = Color.WhiteSmoke;
                }

                btnImportDatabase.Enabled = false;
                btnPODFormat.Enabled = false;

                txtDirectoryDatabse.BackColor = Color.WhiteSmoke;
                txtPODFormat.BackColor = Color.WhiteSmoke;

                txtStaticText.Text = "";
                txtDirectoryDatabse.Text = "";
                txtPODFormat.Text = "";
            }
        }

        public void ShowForm()
        {
            Show();
            BeginInvoke(new Action(() =>
            {
                UpdateUIClearJobInformation();
                ClearProductInfo();
                tabControl1.SelectedIndex = 0;
                _IsProcessing = false;
                txtSearchProduct.Text = "";
                _ = Task.Run(() => RefreshJobList());
                LoadProductListToGrid();
                Invalidate(true);
                Refresh();
            }));
        }

        private bool CheckExistTemplatePrint(string tmp)
        {
            if (_PrintProductTemplateList.Count() <= 0)
            {
                return false;
            }
            foreach (var item in _PrintProductTemplateList)
            {
                if (item == tmp)
                {
                    return true;
                }
            }
            return false;
        }

        private string GetSelectedPrintProductTemplate()
        {
            string printTemplate = "";
            object selectedItem = null;
            Invoke(new Action(() =>
            {
                selectedItem = listBoxPrintProductTemplate.SelectedItem;
            }));

            if (selectedItem != null && selectedItem is ItemCustomModel)
            {
                var itemCustomModel = selectedItem as ItemCustomModel;
                if (_PrintProductTemplateList != null && itemCustomModel.Value >= 0 && itemCustomModel.Value < _PrintProductTemplateList.Count())
                {
                    printTemplate = _PrintProductTemplateList[itemCustomModel.Value];
                }
            }
            return printTemplate;
        }

        private void AutoGenerateFileName()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AutoGenerateFileName()));
                return;
            }
            string defaultName = string.Format("{0}_{1}", DateTime.Now.ToString(Shared.Settings.JobDateTimeFormat), Shared.Settings.JobFileNameDefault);
            txtFileName.Text = defaultName;
        }

        private void UpdateUIClearJobInformation()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUIClearJobInformation()));
                return;
            }
            _JobModel = new JobModel
            {
                CompareType = CompareType.CanRead,
                StaticText = "",
                DirectoryDatabase = "",
                PODFormat = _PODFormat,
                FileName = "",
                UserCreate = Shared.LoggedInUser.FullName,
                AutoLoad = true,
                THJobOperatingMode = Shared.Settings.THOperatingMode
            };
            listBoxJobList.Enabled = true;
            UpdateUIClearTextBoxInfo(_JobModel);
        }

        private string OpenDirectoryFileDatabase()
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                string filePath = "";
                openFileDialog1.Filter = "Database files (*.csv, *.txt)|*.csv;*.txt";
                openFileDialog1.FilterIndex = 0;
                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog1.FileName;
                }
                return filePath;
            }
        }

        private void CreateJob()
        {
            _NameOfJobOld = "";
            _JobModel = new JobModel
            {
                CompareType = CompareType.CanRead,
                StaticText = "",
                DirectoryDatabase = "",
                PODFormat = _PODFormat,
                FileName = "",
                UserCreate = Shared.LoggedInUser.FullName,
                AutoLoad = true,
                THJobOperatingMode = Shared.Settings.THOperatingMode
            };
            _JobModel.PrinterSeries = _JobModel.PrinterSeries;
            _JobModel.TemplatePrint = "";
            _JobModel.JobStatus = JobStatus.NewlyCreated;
        }

        private void OpenJob()
        {
            if (_IsProcessing || listBoxJobList.SelectedItem == null)  // Check existing processing
            {
                return;
            }
            _IsProcessing = true;
            _NameOfJobOld = listBoxJobList.SelectedItem.ToString(); // Get Job name with extension
            Shared.JobNameSelected = _NameOfJobOld;  // Open Job file
            _JobModel = Shared.GetJob(_NameOfJobOld);
            UpdateUIJobInformation(_JobModel);
            _IsProcessing = false;
        }

        private void DeleteJob()
        {
            try
            {
                if (_NameOfJobOld != "")
                {
                    JobModel jobModel = Shared.GetJob(_NameOfJobOld);

                    bool permission = !(Shared.LoggedInUser.Role == 1);
                    if (!permission)
                    {
                        bool isNewCreate = jobModel.JobStatus == JobStatus.NewlyCreated;
                        if (!isNewCreate)
                        {
                            string warningMsg = Lang.YouDoNotHavePermission;
                            CuzMessageBox.Show(warningMsg, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string message = Lang.AreYouSureYouWantToDeleteFile + "\r\n" + _NameOfJobOld;
                    DialogResult result = CuzMessageBox.Show(message, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        jobModel.JobStatus = JobStatus.Deleted; // Reload Job name list
                        jobModel.SaveFile();
                    }

                    LoadJobNameList();
                }
            }
            catch
            {
                LoadJobNameList();
            }
        }

        private JobModel InitJobModel()
        {
            var job = new JobModel();
            bool isRSeries = radRSeries.Checked;
            job.PrinterSeries = isRSeries;
            job.FileName = txtFileName.Text;

            if (Shared.PrintMode.IsPrintingMode && UserPermission.isOnline)
            {
                job.DispatchingOrderPayload = _JobModel.DispatchingOrderPayload;
                var selProd = GetSelectedProduct();
                job.SelectedMaterialIndex = selProd != null ? cbcSelectProduct.SelectedIndex : -1;
            }


            if (isRSeries)
            {
                job.CompareType = CompareType.Database;

                if (radAfterProduction.Checked)
                {
                    job.JobType = JobType.AfterProduction;
                }
                else if (radOnProduction.Checked)
                {
                    job.JobType = JobType.OnProduction;
                }
                else if (radVerifyAndPrint.Checked)
                {
                    job.JobType = JobType.VerifyAndPrint;
                }

                job.DirectoryDatabase = txtDirectoryDatabse.Text;
                job.IsFirstRowHeader = FirstRowHeader.Checked;
                job.PODFormat = _PODFormat;
                job.StaticText = "";
                job.TemplatePrint = _matchedPrinterTemplate;
                job.NumberTotalsCode = job.IsFirstRowHeader ? _NumberTotalsCode : _NumberTotalsCode;
                job.JobStatus = JobStatus.NewlyCreated;
            }
            else
            {
                job.JobType = JobType.StandAlone;
                job.IsFirstRowHeader = FirstRowHeader.Checked;
                job.TemplatePrint = "";
                job.StaticText = "";
                job.PODFormat = new List<PODModel>();
                job.DirectoryDatabase = "";
                if (radCanRead.Checked)
                {
                    job.CompareType = CompareType.CanRead;
                }
                else if (radStaticText.Checked)
                {
                    job.CompareType = CompareType.StaticText;
                    job.StaticText = txtStaticText.Text;
                }
                else if (radDatabase.Checked)
                {
                    job.CompareType = CompareType.Database;
                    job.DirectoryDatabase = txtDirectoryDatabse.Text;
                    job.PODFormat = _PODFormat;
                    job.NumberTotalsCode = job.IsFirstRowHeader ? _NumberTotalsCode : _NumberTotalsCode;
                }
            }

            return job;
        }

        private void PurgeFile(string path)
        {
            try
            {
                string[] lines = File.ReadAllLines(path);

                if (lines.Length > 0)
                {
                    // Get the first line
                    string firstLine = lines[0];
                    File.WriteAllText(path, firstLine + Environment.NewLine);
                }
            }
            catch (Exception)
            {
            }

        }

        private void PurgeJob(JobModel jobModel)
        {
            try
            {
                var pathDatabase = jobModel.DirectoryDatabase;
                var pathBackupPrintedResponse = CommVariables.PathPrintedResponse + jobModel.PrintedResponePath;
                var pathCheckedResult = CommVariables.PathCheckedResult + jobModel.CheckedResultPath;
                PurgeFile(pathBackupPrintedResponse);
                PurgeFile(pathCheckedResult);
                CuzMessageBox.Show("Purge Job successfully !", "Purge Job", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
            }
        }

        private void SaveJob()
        {
            try
            {
                _JobModel = InitJobModel();
                _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
                _JobModel.TemplatePrint = _matchedPrinterTemplate;
                _JobModel.SelectedBatchIndex = Settings.SelectedBatchIndex;
                _JobModel.isPushedDatabase = isPushDatabase;
                // ── Snapshot cấu hình THTrueMilk tại thời điểm tạo job ──────
                _JobModel.THJobOperatingMode = Shared.Settings.THOperatingMode;
                _JobModel.THJobDeltaMinutes = Shared.Settings.THDeltaMinutes;
                _JobModel.THJobBufferCount = Shared.Settings.THBufferCount;
                _JobModel.THJobNMinutes = Shared.Settings.THNMinutes;          
                _JobModel.THJobLogInterval = Shared.Settings.THLogInterval;
                _JobModel.THJobErrorImageFolder = Shared.Settings.THErrorImageFolder;
                _JobModel.THJobMonitorInterval = Shared.Settings.THMonitorInterval;
                _JobModel.THJobCameraModelForTraining = _matchedCameraProgram;
                _JobModel.THMaxConsecutiveError = Shared.Settings.THMaxConsecutiveError;

                // ── Lưu QR hiện tại để restart cùng ngày giữ lại ──
                _JobModel.CurrentBatchQrCode = _currentBatchQrCode;
                _JobModel.CurrentBatchDate = _currentBatchDate;
                _JobModel.CurrentQrId = _currentQrId;

                // ── Lưu số tấn và volume ──
                if (double.TryParse(txtTotalQR.Text.Trim().Replace(",", "."), 
                    System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out double tons))
                    _JobModel.EstimatedTons = tons;
                var selProdForVol = GetSelectedProduct();
                if (selProdForVol != null && selProdForVol.Volume.HasValue)
                    _JobModel.Volume = selProdForVol.Volume.Value;

                if (UserPermission.isOnline)
                {
                    if (PrintMode.IsProcessOrderMode)
                    {
                        _JobModel.ProcessOrderItem = Settings?.ManufacturingListPO?.process_orders[Settings.SelectedPOIndex];
                        _JobModel.IsProcessOrderMode = true;
                    }
                    else if (PrintMode.IsReservationMode)
                    {
                        _JobModel.ReservationItem = Reservation?.items[SelectedRESMaterialIndex];
                        _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                        _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
                        _JobModel.Reservation = Reservation;
                        _JobModel.SelectedRESItemIndex = SelectedRESMaterialIndex;
                        _JobModel.SelectedBatchIndex = 0;
                        _JobModel.IsReservationMode = true;
                        _JobModel.IsReservationMode = true;
                    }
                }
                else
                {
                    if (PrintMode.IsProcessOrderMode)
                    {
                        _JobModel.IsProcessOrderMode = true;
                        _JobModel.SelectedBatchIndex = 0;

                        _JobModel.ProcessOrderItem = new ResponseProcessOrder.Data()
                        {
                            process_order = InputPO.Text,
                            material_number = MaterialNumber.Text,
                            batch_info = new List<ResponseProcessOrder.BatchInfo>
                        {
                            new ResponseProcessOrder.BatchInfo
                            {
                                batch = LOTNumber.Text,
                                mauf_date = MaufDatePicker.Value,
                                expired_date = ExpiredDatePicker.Value,
                            }
                        }
                        };
                    }
                    else
                    {

                        _JobModel.IsReservationMode = true;
                        _JobModel.Reservation = new ResponseReservation()
                        {
                            material_doc = RES_Material_doc.Text,
                            items = new List<ReservationItem>
                            {
                                new ReservationItem
                                {
                                    material_number = RES_MaterialNumber.Text,
                                    batch = RES_LotNumber.Text,
                                    mauf_date = RESMaufDatePicker.Value,
                                    expired_date = RESExpiredDatePicker.Value,
                                }
                            }
                        };
                        _JobModel.ReservationItem = _JobModel.Reservation.items[0];
                        _JobModel.SelectedBatchIndex = 0;
                        _JobModel.SelectedRESItemIndex = 0;

                    }
                }


                if (_JobModel != null)   // Check current Job has null
                {
                    string JobName = _JobModel.FileName;  // Check Job name is empty
                    if (JobName == "")
                    {
                        //CuzMessageBox.Show(Lang.PleaseInputJobName, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (_JobModel.PrinterSeries)
                    {
                        if (_JobModel.CompareType == CompareType.Database)
                        {

                            string databasePath = _JobModel.DirectoryDatabase;  // Check Database
                            if (_JobModel.CompareType == CompareType.Database && databasePath == "")
                            {
                                CuzMessageBox.Show(Lang.PleaseSelectDatabasePath, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            //_JobModel.PODFormat = "<field1>";
                            txtPODFormat.Text = "<field1>";
                            _JobModel.PODFormat = new List<PODModel>
                            {
                                new PODModel(1, txtPODFormat.Text, PODModel.TypePOD.FIELD, txtPODFormat.Text)
                            };  // Check POD format

                            string podFormat = _JobModel.PODFormat.ToString();   // Check POD format

                            if (_JobModel.CompareType == CompareType.Database && podFormat == "" || txtPODFormat.Text == "")
                            {
                                CuzMessageBox.Show(Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            _JobModel.TemplatePrint = _matchedPrinterTemplate;

                            if (_JobModel != null && _JobModel.CompareType == CompareType.Database && !CheckExistTemplatePrint(_JobModel.TemplatePrint) && _JobModel.PrinterSeries)
                            {
                                //CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                //return;
                            }
                        }
                    }
                    else
                    {
                        if (_JobModel.CompareType == CompareType.Database)
                        {

                            string databasePath = _JobModel.DirectoryDatabase;  // Check Database
                            if (_JobModel.CompareType == CompareType.Database && databasePath == "")
                            {
                                CuzMessageBox.Show(Lang.PleaseSelectDatabasePath, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            string podFormat = _JobModel.PODFormat.ToString();  // Check POD format

                            if (_JobModel.CompareType == CompareType.Database && podFormat == "" || txtPODFormat.Text == "")
                            {
                                CuzMessageBox.Show(Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                        else
                        {
                            if (_JobModel.CompareType == CompareType.StaticText)
                            {
                                if (_JobModel.StaticText == "")
                                {
                                    CuzMessageBox.Show(Lang.PleaseEnterTheStaticText, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                            }
                            else
                            {
                                _JobModel.StaticText = "";
                            }
                        }
                    }


                    if (Shared.CheckJobHasExist(JobName))  // Check Job name has exist and confirm replace
                    {
                        JobModel tmpJob = Shared.GetJob(JobName + Shared.Settings.JobFileExtension);
                        if (tmpJob != null)
                        {
                            if (tmpJob.JobStatus == JobStatus.Deleted)
                            {
                                string oldJobPath = CommVariables.PathJobsApp + JobName + Shared.Settings.JobFileExtension;
                                string newJobPath = CommVariables.PathJobsApp + JobName + "_Old_" +
                                    DateTime.Now.ToString("yyMMddHHmmss") + Shared.Settings.JobFileExtension;
                                try
                                {
                                    File.Move(oldJobPath, newJobPath);
                                }
                                catch
                                {

                                }
                            }
                            else
                            {
                                string message = Lang.DoYouWantToReplaceExistingTemplate + "\r\n" + JobName + Shared.Settings.JobFileExtension;
                                DialogResult result = CuzMessageBox.Show(message, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (result == DialogResult.Yes)
                                {
                                    // Continue execute code below
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }

                    if (JobName != "")
                    {
                        var jobToDelete = _JobModel;
                        _ = Task.Run(() => Shared.DeleteJob(jobToDelete));
                    }

                    _ = Task.Run(() => _JobModel.SaveFile());

                    Shared.JobNameSelected = JobName + Shared.Settings.JobFileExtension;
                    _PODFormat.Clear();
                    listBoxPrintProductTemplate.ClearSelected();
                }

                string jobInfo = "";
                var jm = _JobModel;
                if (jm != null)
                {
                    string modeLabel = jm.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                        ? "Mode 1 - BatchOneQrCode"
                        : jm.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                        ? "Mode 4 - BatchOneQrCodeNoChange"
                        : jm.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime
                        ? "Mode 2 - AutoRefreshByTime" : "Mode 3 - ProductOneQrCode";
                    string deltaInfo = jm.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                        ? $"\nDelta reset (phút): {jm.THJobDeltaMinutes}"
                        : jm.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                        ? $"\nKhông đổi QR"
                        : "";
                    string intervalInfo = jm.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime
                        ? $"\nLàm mới sau (phút): {jm.THJobNMinutes}" : "";
                    jobInfo =
                        $"\n========================================" +
                        $"\nSản phẩm: {jm.THJobProductName} ({jm.THJobProductId}) | Batch: {jm.THJobBatchNo}" +
                        $"\nSố lượng: {jm.NumberTotalsCode:N0}" +
                        $"\n----------------------------------------" +
                        $"\n{modeLabel}" +
                        deltaInfo +
                        intervalInfo +
                        $"\nĐệm gửi trước in (Buffer): {jm.THJobBufferCount}" +
                        $"\nGhi log mỗi (phút): {jm.THJobLogInterval}" +
                        $"\nMonitor mỗi (giây): {jm.THJobMonitorInterval}" +
                        $"\nLỗi liên tục tối đa: {jm.THMaxConsecutiveError}" +
                        $"\nNgưỡng QR: {jm.THJobQrThreshold}" +
                        $"\n========================================";
                }
                //DialogResult dialogResult = CuzMessageBox.Show(
                //    Lang.SuccessfulNewJobCreationStartTheProcess + jobInfo,
                //    Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                DialogResult dialogResult = DialogResult.Yes;
                if (dialogResult == DialogResult.Yes)
                {
                    DisplayJobLoading(true);

                    if (Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings && _JobModel.CompareType == CompareType.Database && _JobModel.PrinterSeries)
                    {
                        PrinterSettingsModel printerSettingsModel = Shared.GetSettingsPrinter();

                        if (printerSettingsModel.PodDataType != 1)
                        {
                            DisplayJobLoading(false);
                            radOther.Checked = true;
                            txtFileName.Text = "";
                            UpdateUIClearJobInformation();
                            CuzMessageBox.Show(Lang.DataTypeMustBeRAWData, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    DisplayJobLoading(false);
                    Hide();
                    _FormMainPC?.Dispose();
                    // singleton keeps running — frmMain will SetSnapshotFactory
                    if (_FormMainPC == null || _FormMainPC.IsDisposed)
                    {
                        _FormMainPC = new frmMainTHTrueMilk(this);  // needed changed
#if DEBUG
                        _FormMainPC.DebugNsxHsdTime = _debugNsxHsdTime;
#endif

                        _FormMainPC.Show();
                    }
                    else
                    {
                        if (_FormMainPC.WindowState == FormWindowState.Minimized)
                        {
                            _FormMainPC.WindowState = FormWindowState.Normal;
                        }

                        _FormMainPC.Focus();
                        _FormMainPC.BringToFront();
                    }
                    _FormMainPC.UpdateJobInfoDisplay(_JobModel);
                    PrinterSupport(_JobModel.PrinterSeries, false);
                    txtFileName.Text = "";

                }
                else
                {
                    UpdateUIClearJobInformation();
                }
                return;
            }
            catch (Exception ex)
            {
                //CuzMessageBox.Show(Lang.NewJobCreationFailed, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                return;
            }
        }

        private void UpdateUIClearTextBoxInfo(JobModel jobModel)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUIClearTextBoxInfo(jobModel)));
                return;
            }


            Shared.JobNameSelected = "";
            lblJobNameInfo.Text = "";
            lblCompareTypeInfo.Text = "";
            lblStaticTextInfo.Text = "";
            lblPODFormatInfo.Text = "";
            lblTemplatePrintInfo.Text = "";
            txtJobType.Text = "";
            txtFileName.Text = jobModel.FileName;
            txtJobStatus.Text = "";

            txtStaticText.Text = jobModel.StaticText;

            txtDirectoryDatabse.Text = jobModel.DirectoryDatabase;

            txtPODFormat.Text = "";

            lblStaticTextInfo.BackColor = Color.White;
            lblPODFormatInfo.BackColor = Color.White;
            lblTemplatePrintInfo.BackColor = Color.White;
            txtJobType.BackColor = Color.White;

        }

        private void LoadJobNameList()
        {

            if (_IsProcessing)
            {
                return;
            }
            try
            {
                Invoke(new Action(() =>
                {
                    picLoading.Visible = false;
                }));
                Thread threadLoadJobNameList = new Thread(() =>
                {
                    try
                    {
                        _IsProcessing = true;

                        _NameOfJobOld = "";
                        UpdateUIClearJobInformation();
                        UpdateUILoadJobNameList(false);


                        Invoke(new Action(() =>
                        {
                            listBoxJobList.Items.Clear();
                        }));


                        _JobNameList = null;
                        _JobNameList = Shared.GetJobNameList();
                        //DisplayHistory();


                        Invoke(new Action(() =>
                        {
                            RefreshJobList();
                        }));

                        _IsProcessing = false;
                        Invoke(new Action(() =>
                        {
                            picLoading.Visible = true;
                        }));

                        UpdateUILoadJobNameList(true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("error in thread: " + ex.Message);
                    }

                });
                threadLoadJobNameList.IsBackground = true;
                threadLoadJobNameList.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("error: " + ex.Message);
            }

        }

        private void UpdateUILoadJobNameList(bool isEnable)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUILoadJobNameList(isEnable)));
                return;
            }

            picLoading.Visible = !isEnable;
            listBoxJobList.Enabled = isEnable;
        }

        private void UpdateUIJobInformation(JobModel jobModel)
        {

            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUIJobInformation(jobModel)));
                return;
            }

            if (jobModel != null)
            {
                _IsBinding = true;
                string pODFormat = "";

                if (jobModel.CompareType == CompareType.CanRead)
                {
                    lblCompareTypeInfo.Text = Lang.CanRead;
                    lblStaticText1.Text = Lang.StaticText;
                    lblStaticTextInfo.Text = jobModel.StaticText;
                    txtJobType.Text = jobModel.JobType.ToFriendlyString();
                }
                else if (jobModel.CompareType == CompareType.StaticText)
                {
                    lblCompareTypeInfo.Text = Lang.StaticText;
                    lblStaticText1.Text = Lang.StaticText;
                    lblStaticTextInfo.Text = jobModel.StaticText;
                    txtJobType.Text = jobModel.JobType.ToFriendlyString();
                }
                else
                {
                    lblCompareTypeInfo.Text = Lang.Database;
                    txtJobType.Text = jobModel.JobType.ToFriendlyString();
                    lblStaticText1.Text = Lang.Totals;
                    lblStaticTextInfo.Text = jobModel.NumberTotalsCode.ToString();
                }

                if (jobModel.JobType == JobType.StandAlone)
                {
                    if (jobModel.CompareType == CompareType.CanRead)
                        lblStaticTextInfo.BackColor = Color.WhiteSmoke;
                    else
                        lblStaticTextInfo.BackColor = Color.White;

                    if (jobModel.CompareType != CompareType.Database)
                        lblPODFormatInfo.BackColor = Color.WhiteSmoke;
                    else
                        lblPODFormatInfo.BackColor = Color.White;

                    lblTemplatePrintInfo.BackColor = Color.WhiteSmoke;
                    txtJobType.BackColor = Color.WhiteSmoke;
                }
                else
                {
                    lblStaticTextInfo.BackColor = Color.White;
                    lblPODFormatInfo.BackColor = Color.White;
                    lblTemplatePrintInfo.BackColor = Color.White;
                    txtJobType.BackColor = Color.White;
                }

                foreach (PODModel item in jobModel.PODFormat)
                {
                    if (item.Type == PODModel.TypePOD.FIELD)
                        pODFormat += item.ToString();
                    else if (item.Type == PODModel.TypePOD.TEXT)
                        pODFormat += item.ToStringSample();
                }
                txtJobStatus.Text = jobModel.JobStatus.ToFriendlyString();
                lblPODFormatInfo.Text = pODFormat;
                lblJobNameInfo.Text = jobModel.FileName;
                lblTemplatePrintInfo.Text = jobModel.TemplatePrint;
                _IsBinding = false;
            }
            else
            {
                UpdateUIClearJobInformation();
            }
        }

        private void EnableForCompareType(CompareType compareType)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableForCompareType(compareType)));
                return;
            }
            bool isTemplate = radRSeries.Checked;
            if (compareType == CompareType.CanRead)
            {
                txtStaticText.ReadOnly = true;
                txtStaticText.Text = "";
                DatabaseChecked(false, isTemplate);
            }
            else if (compareType == CompareType.StaticText)
            {
                txtStaticText.ReadOnly = false;
                txtStaticText.Text = "";
                DatabaseChecked(false, isTemplate);
            }
            else if (compareType == CompareType.Database)
            {
                txtStaticText.ReadOnly = true;
                txtStaticText.Text = "";
                DatabaseChecked(true, isTemplate);
            }
        }
        public async void Exit()
        {
            DialogResult dialogResult = CuzMessageBox.Show(Lang.DoYouWantExitApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    string displayName = "";
                    try
                    {
                        displayName = SecurityController.Decrypt(Shared.LoggedInUser?.UserName ?? "", "rynan_encrypt_remember");
                    }
                    catch { displayName = Shared.LoggedInUser?.UserName ?? ""; }

                    LoggingController.SaveHistory(
                        Lang.Exit,
                        Lang.LogOut,
                        Lang.LogoutSuccessfully,
                        displayName,
                        LoggingType.LogedOut);
                }
                catch { }

                try
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            ApiService apiService = new ApiService();
                            await BarcodeVerificationSystem.Services.THTrueMilk.MonitorSenderService.sendParametersToServerAsync(apiService, false);
                        }
                        catch { }
                    });
                }
                catch { }
                _isClosingFromExit = true;
                Close();
            }
        }
        //public async void Exit()
        //{
        //    DialogResult dialogResult = CuzMessageBox.Show(Lang.DoYouWantExitApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //    if (dialogResult == DialogResult.Yes)
        //    {
        //        try
        //        {
        //            LoggingController.SaveHistory(  //Save history
        //                Lang.Exit,
        //                Lang.LogOut,
        //                Lang.LogoutSuccessfully,
        //                SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
        //                LoggingType.LogedOut);

        //            try
        //            {
        //                ApiService apiService = new ApiService();
        //                await BarcodeVerificationSystem.Services.THTrueMilk.MonitorSenderService.sendParametersToServerAsync(apiService, false);
        //            }
        //            catch (Exception) { }

        //            Close();
        //        }
        //        catch (Exception)
        //        {

        //        }
        //    }
        //}

        internal void Invoke_AutoAddSufixEvent()
        {
            AutoAddSufixEvent.Invoke(this, EventArgs.Empty);
        }

        #endregion Utility_Function

        #region UpdateUI Printer 
        private void UpdateStatusLabelPrinter()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatusLabelPrinter()));
                return;
            }

            for (int i = 0; i < Shared.Settings.PrinterList.Count; i++)
            {
                if (i < _LabelStatusPrinterList.Count)
                {
                    PrinterModel printerModel = Shared.Settings.PrinterList[i];
                    ToolStripLabel labelStatusPrinter = _LabelStatusPrinterList[i];

                    if (printerModel.IsConnected)
                    {
                        ShowLabelIcon(labelStatusPrinter, Lang.Printer, Properties.Resources.icons8_printer_30px_connected);
                    }
                    else
                    {
                        ShowLabelIcon(labelStatusPrinter, Lang.Printer, Properties.Resources.icons8_printer_30px_disconnected);
                    }
                }
            }
        }

        private void UpdateUISensorControllerStatus(bool isConnect)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUISensorControllerStatus(isConnect)));
                return;
            }
            if (isConnect)
            {
                ShowLabelIcon(lblSensorControllerStatus, Lang.SensorController, Properties.Resources.icons8_sensor_30px_connected);
            }
            else
            {
                ShowLabelIcon(lblSensorControllerStatus, Lang.SensorController, Properties.Resources.icons8_sensor_30px_disconnected);
            }
        }

        private void UpdateUISerialDeviceControllerStatus(bool isConnect)
        {
            // thinh dang sua
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUISerialDeviceControllerStatus(isConnect)));
                return;
            }
            if (isConnect)
            {
                ShowLabelIcon(lblStatusSerialDevice, Lang.SerialDevice, Properties.Resources.icons8_scanner_connected);
            }
            else
            {
                ShowLabelIcon(lblStatusSerialDevice, Lang.SerialDevice, Properties.Resources.icons8_scanner_disconnected);
            }
        }


        private void ShowLabelIcon(ToolStripLabel label, string text, System.Drawing.Image icon)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowLabelIcon(label, text, icon)));
                return;
            }

            if (label.Tag == icon)
            {
                return;
            }

            label.Tag = icon;
            label.ImageAlign = ContentAlignment.MiddleLeft;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.Text = text;
            label.Image = icon;
        }

        #endregion UpdateUI Printer

        #region Monitor Printer
        //private void MonitorPrinterConnection()
        //{
        //    _ThreadMonitorPrinter = new Thread(() =>
        //    {
        //        while (true)
        //        {
        //            try
        //            {
        //                for (int i = 0; i < Shared.Settings.PrinterList.Count; i++)
        //                {
        //                    PrinterModel printerModel = Shared.Settings.PrinterList[i];
        //                    if (printerModel.IsEnable)
        //                    {
        //                        PODController podController = printerModel.PODController; // Get controller has exist if not exist then add new controller
        //                        if (podController == null)
        //                        {
        //                            podController = new PODController(printerModel.IP, printerModel.Port, printerModel.RoleOfPrinter, 1000, 1000, printerModel.IsVersion);
        //                            podController.Connect();
        //                            podController.OnPODReceiveDataEvent -= PODController_OnPODReceiveDataEvent;
        //                            podController.OnPODReceiveDataEvent += PODController_OnPODReceiveDataEvent;
        //                            printerModel.PODController = podController;
        //                        }
        //                        else
        //                        {
        //                            if (podController.Port != printerModel.Port)
        //                            {
        //                                podController.Port = printerModel.Port;
        //                            }
        //                            else if (podController.ServerIP != printerModel.IP)
        //                            {
        //                                podController.ServerIP = printerModel.IP;
        //                            }
        //                        }
        //                        bool isConnected = podController.IsConnected();
        //                        if (isConnected == false)
        //                        {
        //                            podController.Disconnect();
        //                            podController.Connect();
        //                        }
        //                        if (isConnected != printerModel.IsConnected)
        //                        {
        //                            printerModel.IsConnected = podController.IsConnected();
        //                            ProjectLogger.WriteInfo($"[Printer] Trạng thái thay đổi → [{printerModel.IP}] {(printerModel.IsConnected ? "Connected" : "Disconnected")}");
        //                            UpdateStatusLabelPrinter();
        //                            Shared.RaiseOnPrinterStatusChangeEvent();
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception)
        //            {

        //            }

        //            Thread.Sleep(2000);
        //        }
        //    })
        //    {
        //        IsBackground = true,
        //        Priority = ThreadPriority.Normal
        //    };
        //    _ThreadMonitorPrinter.Start();
        //}
        // ── THAY THẾ MonitorPrinterConnection — thêm NotifyDeviceException ───
        private void MonitorPrinterConnection()
        {
            _ThreadMonitorPrinter = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        for (int i = 0; i < Shared.Settings.PrinterList.Count; i++)
                        {
                            PrinterModel printerModel = Shared.Settings.PrinterList[i];
                            if (printerModel.IsEnable)
                            {
                                PODController podController = printerModel.PODController;
                                if (podController == null)
                                {
                                    podController = new PODController(printerModel.IP, printerModel.Port, printerModel.RoleOfPrinter, 1000, 1000, printerModel.IsVersion);
                                    podController.Connect();
                                    podController.OnPODReceiveDataEvent -= PODController_OnPODReceiveDataEvent;
                                    podController.OnPODReceiveDataEvent += PODController_OnPODReceiveDataEvent;
                                    printerModel.PODController = podController;
                                }
                                else
                                {
                                    if (podController.Port != printerModel.Port)
                                        podController.Port = printerModel.Port;
                                    else if (podController.ServerIP != printerModel.IP)
                                        podController.ServerIP = printerModel.IP;
                                }

                                bool isConnected = podController.IsConnected();
                                if (!isConnected)
                                {
                                    podController.Disconnect();
                                    podController.Connect();
                                }
                                if (isConnected != printerModel.IsConnected)
                                {
                                    printerModel.IsConnected = podController.IsConnected();
                                    ProjectLogger.WriteInfo($"[Printer] Trạng thái thay đổi → [{printerModel.IP}] {(printerModel.IsConnected ? "Connected" : "Disconnected")}");
                                    UpdateStatusLabelPrinter();
                                    Shared.RaiseOnPrinterStatusChangeEvent();

                                    // Theo dõi thời gian mất kết nối
                                    if (!printerModel.IsConnected)
                                    {
                                        if (_printerWasEverConnected && _printerDisconnectedSince == DateTime.MinValue)
                                            _printerDisconnectedSince = DateTime.Now;
                                    }
                                    else
                                    {
                                        _printerWasEverConnected = true;
                                        // Reconnect thành công → KHÔNG gửi AGREEN, giữ trạng thái hiện tại
                                        _printerDisconnectedSince = DateTime.MinValue;
                                        Shared.PrinterDisconnectAlertShown = false;
                                    }
                                }
                            }
                        }

                        // Sau >10s mất kết nối liên tục → thông báo + PLC + stop (nếu đang chạy)
                        if (_printerDisconnectedSince != DateTime.MinValue
                            && (DateTime.Now - _printerDisconnectedSince).TotalSeconds >= 10
                            && !Shared.PrinterDisconnectAlertShown)
                        {
                                        Shared.PrinterDisconnectAlertShown = true;
                                        bool isRunning = Shared.OperStatus == OperationStatus.Running
                                                      || Shared.OperStatus == OperationStatus.Processing;
                                        ProjectLogger.WriteInfo($"[DISCONNECT] [PRINTER] >10s lost → Signal=RED Running={isRunning}");
                                        BeginInvoke(new Action(() =>
                                        {
                                            CustomMessageBox.ShowCenterScreen("Máy in mất kết nối hơn 10 giây, vui lòng kiểm tra lại kết nối!",
                                                "Mất kết nối máy in", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }));
                                        if (isRunning && Shared.SensorController != null && Shared.IsSensorControllerConnected)
                            {
                                Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                                if (_FormMainPC != null && !_FormMainPC.IsDisposed && _FormMainPC.IsHandleCreated)
                                    _FormMainPC.BeginInvoke(new Action(() => _FormMainPC?.RequestAutoStop("Máy in mất kết nối >10s")));
                            }
                        }
                    }
                    catch (Exception) { }

                    Thread.Sleep(1000);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitorPrinter.Start();
        }
        private void UpdateUIListBoxPrintProductTemplateList(string[] printTemplateNames, string keyWord = "")
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUIListBoxPrintProductTemplateList(printTemplateNames, keyWord)));
                return;
            }

            if (printTemplateNames == null)
            {
                listBoxPrintProductTemplate.Items.Clear();
            }
            else
            {
                listBoxPrintProductTemplate.Items.Clear();
                keyWord = keyWord.ToLower();
                int itemIndex = 0;
                foreach (string printTemplateName in printTemplateNames)
                {
                    if (printTemplateName.ToLower().Contains(keyWord))
                    {
                        var obj = new ItemCustomModel(printTemplateName, itemIndex);
                        listBoxPrintProductTemplate.Items.Add(obj);
                    }
                    itemIndex++;
                }

            }
        }
        private void ObtainPrintProductTemplateList()
        {
            if (_IsObtainingPrintProductTemplateList)
            {
                return;
            }
            Invoke(new Action(() =>
            {
                listBoxPrintProductTemplate.Items.Clear();
            }));
            _PrintProductTemplateList = new string[] { };
            Task.Run(() =>
            {
                PODController podController = Shared.Settings.PrinterList.Where(p => p.RoleOfPrinter == RoleOfStation.ForProduct).FirstOrDefault().PODController;

                if (podController != null)
                {
                    podController.Send("RQLI"); // send command request template list
                    Task.Delay(5);
                    UpdateUIListBoxPrintProductTemplateList(_PrintProductTemplateList);
                }
            });
        }

        private async Task ObtainPrintProductTemplateListAsync()
        {
            if (_IsObtainingPrintProductTemplateList) return;

            _rslResponseTcs = new TaskCompletionSource<bool>();
            ObtainPrintProductTemplateList();

            var completed = await Task.WhenAny(
                _rslResponseTcs.Task,
                Task.Delay(5000)
            );

            if (completed != _rslResponseTcs.Task)
                throw new TimeoutException("Máy in không phản hồi RQLI");
        }
        private void EnableUIPrinting(bool isActive = true, bool isObtain = true)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableUIPrinting(isActive, isObtain)));
                return;
            }
            bool isEnable = Shared.Settings.IsPrinting & isActive;
            listBoxPrintProductTemplate.Enabled = isEnable;
            if (isEnable && isObtain)
            {
                ObtainPrintProductTemplateList();
            }
        }
        #endregion Monitor Printer

        #region Camera Connection
        private void MonitorCameraConnection()
        {
            _ThreadMonitorCamera = new Thread(() =>
            {
                int counter = 0;
                int vscDisconnectCount = 0;
                while (true)
                {
                    try
                    {
                        foreach (CameraModel cameraModel in Shared.Settings.CameraList)
                        {
                            if (cameraModel.IsEnable)
                            {
                                switch (cameraModel.CameraType)
                                {
                                    case CameraType.CV_X:
                                    case CameraType.VS_C:
                                        // ── Bước 1: Tạo camera nếu chưa có ──
                                        if (Shared.vscCamera == null || counter >= 3)
                                        {
                                            if (Shared.vscCamera != null)
                                            {
                                                Shared.vscCamera.StopListening();
                                                Shared.vscCamera.Disconnect();
                                                Shared.vscCamera = null;
                                            }
                                            Shared.vscCamera = new VscCamera(
                                                cameraModel.IP, int.Parse(cameraModel.Port), 5000);
                                            Shared.vscCamera.FtpImagePath = Shared.Settings.THFtpImagePath;

                                            // QUAN TRỌNG: Set protocol mode là Framed cho Keyence
                                            Shared.vscCamera.SetProtocolMode(ProtocolMode.Framed);
                                            bool ok = Shared.vscCamera.Connect();
                                            ProjectLogger.WriteInfo(
                                                $"[Keyence] Connect({cameraModel.IP}:{cameraModel.Port}) → {ok}");
                                            if (ok)
                                            {
                                                Shared.vscCamera.StartListening();
                                                vscDisconnectCount = 0;
                                                counter = 0;
                                                // Reconnect thành công → KHÔNG gửi AGREEN, giữ trạng thái hiện tại
                                                _keyenceDisconnectedSince = DateTime.MinValue;
                                            }
                                            else
                                            {
                                                Shared.vscCamera.Disconnect();
                                                Shared.vscCamera = null;
                                                counter++;
                                            }
                                            SetCameraConnected(cameraModel, Shared.vscCamera != null);

                                            // ── Keyence mất kết nối >10s → thông báo + PLC + stop (nếu đang chạy) ──
                                            if (_keyenceDisconnectedSince != DateTime.MinValue
                                                && (DateTime.Now - _keyenceDisconnectedSince).TotalSeconds >= 10
                                                && !_keyenceDisconnectAlertShown)
                                            {
                                                _keyenceDisconnectAlertShown = true;
                                                bool isRunning = Shared.OperStatus == OperationStatus.Running
                                                              || Shared.OperStatus == OperationStatus.Processing;
                                                ProjectLogger.WriteInfo($"[DISCONNECT] [CAM_KEYENCE] >10s lost → Signal=RED Running={isRunning}");
                                                BeginInvoke(new Action(() =>
                                                {
                                                    CustomMessageBox.ShowCenterScreen("Camera mất kết nối hơn 10 giây, vui lòng kiểm tra lại kết nối!",
                                                        "Mất kết nối Camera", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }));
                                                if (isRunning && Shared.SensorController != null && Shared.IsSensorControllerConnected)
                                                {
                                                     Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                                                     if (_FormMainPC != null && !_FormMainPC.IsDisposed && _FormMainPC.IsHandleCreated)
                                                         _FormMainPC.BeginInvoke(new Action(() => _FormMainPC?.RequestAutoStop("Camera mất kết nối >10s")));
                                                }
                                            }
                                            break;
                                        }
                                        // ── Bước 2: Kiểm tra IP/Port thay đổi ──
                                        if (Shared.vscCamera.ip != cameraModel.IP ||
                                            Shared.vscCamera.port != int.Parse(cameraModel.Port))
                                        {
                                            ProjectLogger.WriteInfo("[Keyence] IP/Port thay đổi → reconnect");
                                            Shared.vscCamera.Disconnect();
                                            Shared.vscCamera = null;
                                            SetCameraConnected(cameraModel, false);
                                            break;
                                        }
                                        // ── Bước 3: Health-check với debounce ──
                                        if (Shared.vscCamera.IsConnected())
                                        {
                                            _keyenceWasEverConnected = true;
                                            vscDisconnectCount = 0;
                                            counter = 0;
                                            SetCameraConnected(cameraModel, true);
                                        }
                                        else
                                        {
                                            vscDisconnectCount++;
                                            ProjectLogger.WriteInfo(
                                                $"[Keyence] IsConnected=false tick {vscDisconnectCount}/3");
                                            if (_keyenceWasEverConnected && _keyenceDisconnectedSince == DateTime.MinValue)
                                                _keyenceDisconnectedSince = DateTime.Now;
                                            if (vscDisconnectCount >= 3)
                                            {
                                                ProjectLogger.WriteInfo("[Keyence] Xác nhận mất kết nối → reconnect");
                                                Shared.vscCamera.Disconnect();
                                                Shared.vscCamera = null;
                                                vscDisconnectCount = 0;
                                                counter++;
                                                SetCameraConnected(cameraModel, false);
                                            }
                                        }

                                        // ── Keyence mất kết nối >10s → thông báo + PLC + stop (nếu đang chạy) ──
                                        if (_keyenceDisconnectedSince != DateTime.MinValue
                                            && (DateTime.Now - _keyenceDisconnectedSince).TotalSeconds >= 10
                                            && !_keyenceDisconnectAlertShown)
                                        {
                                            _keyenceDisconnectAlertShown = true;
                                            bool isRunning = Shared.OperStatus == OperationStatus.Running
                                                          || Shared.OperStatus == OperationStatus.Processing;
                                            ProjectLogger.WriteInfo($"[DISCONNECT] [CAM_KEYENCE] >10s lost → Signal=RED Running={isRunning}");
                                            BeginInvoke(new Action(() =>
                                            {
                                                CustomMessageBox.ShowCenterScreen("Camera mất kết nối hơn 10 giây, vui lòng kiểm tra lại kết nối!",
                                                    "Mất kết nối Camera", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            }));
                                            if (isRunning && Shared.SensorController != null && Shared.IsSensorControllerConnected)
                                            {
                                                Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                                                _FormMainPC?.BeginInvoke(new Action(() => _FormMainPC?.RequestAutoStop("Camera mất kết nối >10s")));
                                            }
                                        }
                                        break;

                                    case CameraType.DM: // DM Series Camera
                                        cameraModel.Port = "23"; // Default is 23
                                        if (Shared.CamController == null || counter >= 3)
                                        {
                                            if (Shared.CamController != null)
                                            {
                                                // Disconnect and remove the previous event handler
                                                Shared.CamController.Disconnect();
                                                CameraController.OnCamReceiveMessageEvent -= CamController_OnCamReceiveMessageEvent;
                                                Shared.CamController = null;
                                            }

                                            // Create a new camera controller
                                            Shared.CamController = new CameraController(cameraModel.IP, int.Parse(cameraModel.Port), 1000, 1000);
                                            Shared.CamController.Connect();
                                            CameraController.OnCamReceiveMessageEvent -= CamController_OnCamReceiveMessageEvent;
                                            CameraController.OnCamReceiveMessageEvent += CamController_OnCamReceiveMessageEvent;
                                            counter = 0;
                                        }
                                        else
                                        {
                                            bool checkIP = Shared.CamController.ServerIP == cameraModel.IP;
                                            if (checkIP)
                                            {
                                                bool checkPort = Shared.CamController.Port == int.Parse(cameraModel.Port);
                                                if (!checkPort)
                                                {
                                                    Shared.CamController.Disconnect();
                                                    Shared.CamController = null;
                                                }
                                            }
                                            else
                                            {
                                                Shared.CamController.Disconnect();
                                                CameraController.OnCamReceiveMessageEvent -= CamController_OnCamReceiveMessageEvent;
                                                Shared.CamController = null;
                                            }
                                        }

                                        bool cognexConnected = Shared.CamController.IsConnected();

                                        if (!cognexConnected)
                                        {
                                            Shared.CamController.Disconnect();
                                            Shared.CamController.Connect();
                                            counter++;
                                        }
                                        else
                                        {
                                            _cognexWasEverConnected = true;
                                            counter = 0;
                                            if (_cognexDisconnectAlertShown)
                                            {
                                                Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Green);
                                                _cognexDisconnectAlertShown = false;
                                            }
                                            _cognexDisconnectedSince = DateTime.MinValue;
                                        }

                                        if (!cognexConnected && _cognexWasEverConnected && _cognexDisconnectedSince == DateTime.MinValue)
                                            _cognexDisconnectedSince = DateTime.Now;

                                        if (_cognexDisconnectedSince != DateTime.MinValue
                                            && (DateTime.Now - _cognexDisconnectedSince).TotalSeconds >= 10
                                            && !_cognexDisconnectAlertShown)
                                        {
                                            _cognexDisconnectAlertShown = true;
                                            bool isRunning = Shared.OperStatus == OperationStatus.Running
                                                          || Shared.OperStatus == OperationStatus.Processing;
                                            ProjectLogger.WriteInfo($"[DISCONNECT] [CAM_DM] >10s lost → Signal=RED Running={isRunning}");
                                            BeginInvoke(new Action(() =>
                                            {
                                                CustomMessageBox.ShowCenterScreen("Camera mất kết nối hơn 10 giây, vui lòng kiểm tra lại kết nối!",
                                                    "Mất kết nối Camera", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            }));
                                            if (isRunning && Shared.SensorController != null && Shared.IsSensorControllerConnected)
                                            {
                                                 Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                                                 if (_FormMainPC != null && !_FormMainPC.IsDisposed && _FormMainPC.IsHandleCreated)
                                                     _FormMainPC.BeginInvoke(new Action(() => _FormMainPC?.RequestAutoStop("Camera mất kết nối >10s")));
                                             }
                                         }
                                         break;

                                     case CameraType.IS:
                                    case CameraType.UKN:
                                        cameraModel.Port = "3000"; // Default is 23
                                        if (Shared.CamController == null || counter >= 3)
                                        {
                                            if (Shared.CamController != null)
                                            {
                                                // Disconnect and remove the previous event handler
                                                Shared.CamController.Disconnect();
                                                CameraController.OnCamReceiveMessageEvent -= CamController_OnCamReceiveMessageEvent;
                                                Shared.CamController = null;
                                            }

                                            // Create a new camera controller
                                            Shared.CamController = new CameraController(cameraModel.IP, int.Parse(cameraModel.Port), 1000, 1000);
                                            Shared.CamController.Connect();
                                            CameraController.OnCamReceiveMessageEvent -= CamController_OnCamReceiveMessageEvent;
                                            CameraController.OnCamReceiveMessageEvent += CamController_OnCamReceiveMessageEvent;
                                            counter = 0;
                                        }
                                        else
                                        {
                                            bool checkIP = Shared.CamController.ServerIP == cameraModel.IP;
                                            if (checkIP)
                                            {
                                                bool checkPort = Shared.CamController.Port == int.Parse(cameraModel.Port);
                                                if (!checkPort)
                                                {
                                                    Shared.CamController.Disconnect();
                                                    Shared.CamController = null;
                                                }
                                            }
                                            else
                                            {
                                                Shared.CamController.Disconnect();
                                                CameraController.OnCamReceiveMessageEvent -= CamController_OnCamReceiveMessageEvent;
                                                Shared.CamController = null;
                                            }
                                        }

                                        bool cognexIsConnected = Shared.CamController.IsConnected();

                                        if (!cognexIsConnected)
                                        {
                                            Shared.CamController.Disconnect();
                                            Shared.CamController.Connect();
                                            counter++;
                                        }
                                        else
                                        {
                                            _cognexWasEverConnected = true;
                                            counter = 0;
                                            if (_cognexDisconnectAlertShown)
                                            {
                                                Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Green);
                                                _cognexDisconnectAlertShown = false;
                                            }
                                            _cognexDisconnectedSince = DateTime.MinValue;
                                        }

                                        if (!cognexIsConnected && _cognexWasEverConnected && _cognexDisconnectedSince == DateTime.MinValue)
                                            _cognexDisconnectedSince = DateTime.Now;

                                        if (_cognexDisconnectedSince != DateTime.MinValue
                                            && (DateTime.Now - _cognexDisconnectedSince).TotalSeconds >= 10
                                            && !_cognexDisconnectAlertShown)
                                        {
                                            _cognexDisconnectAlertShown = true;
                                            bool isRunning = Shared.OperStatus == OperationStatus.Running
                                                          || Shared.OperStatus == OperationStatus.Processing;
                                            ProjectLogger.WriteInfo($"[DISCONNECT] [CAM_IS] >10s lost → Signal=RED Running={isRunning}");
                                            BeginInvoke(new Action(() =>
                                            {
                                                CustomMessageBox.ShowCenterScreen("Camera mất kết nối hơn 10 giây, vui lòng kiểm tra lại kết nối!",
                                                    "Mất kết nối Camera", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            }));
                                            if (isRunning && Shared.SensorController != null && Shared.IsSensorControllerConnected)
                                            {
                                                 Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                                                 if (_FormMainPC != null && !_FormMainPC.IsDisposed && _FormMainPC.IsHandleCreated)
                                                     _FormMainPC.BeginInvoke(new Action(() => _FormMainPC?.RequestAutoStop("Camera mất kết nối >10s")));
                                             }
                                         }
                                         break;
                                }
                            }
                            else
                            {
                                cameraModel.CountTimeReconnect = 0;
                            }
                        }
                    }
                    catch (Exception) { }
                    Thread.Sleep(2000);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitorCamera.Start();

            #region Old
            #endregion
        }
        /// <summary>
        /// Cập nhật cameraModel.IsConnected và raise event
        /// CHỈ KHI state thay đổi — tránh icon chớp liên tục.
        /// </summary>
        private void SetCameraConnected(CameraModel cameraModel, bool connected)
        {
            if (cameraModel.IsConnected == connected) return;

            cameraModel.IsConnected = connected;
            ProjectLogger.WriteInfo(
                $"[VS_C] Status → {(connected ? "✅ Connected" : "❌ Disconnected")}");
            Shared.RaiseOnCameraStatusChangeEvent();
        }
        private void CamController_OnCamReceiveMessageEvent(object sender, EventArgs e)
        {
            try
            {
                string getStringVar = ((string)sender).Replace("\r\n", "");
                if (getStringVar.Equals(_endOfLineStr))
                {
                    getStringVar = "";
                }
                else
                {
                    getStringVar = getStringVar.Replace(_endOfLineStr, "");
                }
                var bitmap = new Bitmap(100, 100);
                var detectModel = new DetectModel
                {
                    Text = getStringVar,
                };
                if (Shared.Settings.CameraList.FirstOrDefault().ReadMode == CameraModeRead.MultiRead) // MultiRead Mode
                {
                    switch (Shared.Settings.CameraList.FirstOrDefault().CameraType)
                    {
                        case CameraType.DM:
                            Shared.RaiseOnCameraReadDataChangeEvent(detectModel);
                            break;
                        case CameraType.IS:
                        case CameraType.ISDual:
                            Shared.RaiseOnCameraPositionDataChangeEvent(detectModel);
                            break;
                    }

                }
            }
            catch (Exception)
            {
            }
        }

        private async void DisposeMultiSyncHandler()
        {
            if (ISMultiSyncHandler != null)
            {
                await ISMultiSyncHandler.DisconnectAsync();
                ISMultiSyncHandler.Dispose();
                ISMultiSyncHandler = null;
            }
        }
        private async void DisposeSingleHandler()
        {
            if (ISSingleHandler != null)
            {
                await ISSingleHandler.DisconnectAsync();
                ISSingleHandler.Dispose();
                ISSingleHandler = null;
            }
        }
        private void MonitorCameraConnection_CognexSupport()
        {
            _ThreadMonitorCamera = new Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        foreach (var cameraModel in Shared.Settings.CameraList)
                        {
                            if (cameraModel.IsEnable && !cameraModel.IsConnected)
                            {
                                // ── VS_C được quản lý hoàn toàn bởi MonitorCameraConnection ──
                                // Không gọi DMCamera/IS handlers cho VS_C để tránh
                                // DMSeries.Connect() reset IsConnected của camera đầu tiên trong list.
                                if (cameraModel.CameraType == CameraType.VS_C)
                                {
                                    Thread.Sleep(200);
                                    continue;
                                }

                                bool wasConnected = cameraModel.IsConnected;

                                DisposeSingleHandler();
                                DisposeMultiSyncHandler();
                                DMCamera?.Disconnect();

                                switch (cameraModel.CameraType)
                                {
                                    case CameraType.DM:
                                        if (Shared.Settings.CameraList.FirstOrDefault().ReadMode == CameraModeRead.Basic)
                                            DMCamera?.Connect(cameraModel.IP);
                                        else
                                            DMCamera?.MultiReadConnect(cameraModel.IP);

                                        cameraModel.CountTimeReconnect++;
                                        if (cameraModel.CountTimeReconnect >= 3)
                                        {
                                            cameraModel.CountTimeReconnect = 0;
                                            DMCamera?._EthSystemDiscoverer?.Discover();
                                        }
                                        break;

                                    case CameraType.IS:
                                        if (ISSingleHandler == null)
                                        {
                                            ISSingleHandler = new ISSingleHandler(cameraModel.IP, "80");
                                            await ISSingleHandler.FirtConnectionAsync();
                                            await Task.Delay(1000);
                                        }
                                        if (!cameraModel.IsConnected)
                                        {
                                            cameraModel.CountTimeReconnect++;
                                            if (cameraModel.CountTimeReconnect >= 2)
                                                cameraModel.CountTimeReconnect = 0;
                                        }
                                        break;

                                    case CameraType.ISDual:
                                        if (ISMultiSyncHandler == null)
                                        {
                                            ISMultiSyncHandler = new ISMultiSyncHandler(cameraModel.IP, "80", cameraModel.ISSlaveIP, "80");
                                            await ISMultiSyncHandler.FirtConnectionAsync();
                                            await Task.Delay(2000);
                                        }
                                        if (!cameraModel.IsConnected)
                                        {
                                            cameraModel.CountTimeReconnect++;
                                            if (cameraModel.CountTimeReconnect >= 2)
                                                cameraModel.CountTimeReconnect = 0;
                                        }
                                        break;
                                }

                                // Feature 2: camera mất kết nối → tín hiệu PLC + log server
                                // Chỉ notify 1 lần khi lần đầu phát hiện mất kết nối (CountTimeReconnect == 1)
                                if (cameraModel.CountTimeReconnect == 1)
                                    NotifyDeviceException(DeviceExceptionType.CameraDisconnected,
                                        $"Camera [{cameraModel.CameraType}:{cameraModel.IP}] mất kết nối");
                            }
                            else
                            {
                                cameraModel.CountTimeReconnect = 0;
                            }
                        }
                        Thread.Sleep(2000);
                    }
                    catch (Exception) { }
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitorCamera.Start();
        }
        //private void MonitorCameraConnection_CognexSupport()
        //{
        //    _ThreadMonitorCamera = new Thread(async () =>
        //    {
        //        while (true)
        //        {
        //            try
        //            {
        //                foreach (var cameraModel in Shared.Settings.CameraList)
        //                {
        //                    if (cameraModel.IsEnable && !cameraModel.IsConnected)
        //                    {
        //                        // Disconnect all camera
        //                        DisposeSingleHandler();
        //                        DisposeMultiSyncHandler();
        //                        DMCamera?.Disconnect();

        //                        // Connect by camera type   
        //                        switch (cameraModel.CameraType)
        //                        {
        //                            case CameraType.DM: //DM Series Camera
        //                                if (Shared.Settings.CameraList.FirstOrDefault().ReadMode == CameraModeRead.Basic)
        //                                {
        //                                    DMCamera?.Connect(cameraModel.IP);
        //                                }
        //                                else
        //                                {
        //                                    DMCamera?.MultiReadConnect(cameraModel.IP);
        //                                }
        //                                cameraModel.CountTimeReconnect++;
        //                                if (cameraModel.CountTimeReconnect >= 3)
        //                                {
        //                                    cameraModel.CountTimeReconnect = 0;
        //                                    DMCamera?._EthSystemDiscoverer?.Discover();
        //                                }
        //                                break;

        //                            case CameraType.IS: //IS Sigle Read Camera (3800)
        //                                if (ISSingleHandler == null)
        //                                {
        //                                    ISSingleHandler = new ISSingleHandler(cameraModel.IP, "80");
        //                                    await ISSingleHandler.FirtConnectionAsync();
        //                                    await Task.Delay(1000);
        //                                }
        //                                // Reconnect 2 times
        //                                if (!cameraModel.IsConnected)
        //                                {
        //                                    cameraModel.CountTimeReconnect++;
        //                                    if (cameraModel.CountTimeReconnect >= 2)
        //                                    {
        //                                        cameraModel.CountTimeReconnect = 0;
        //                                    }
        //                                }
        //                                break;

        //                            case CameraType.ISDual:
        //                                if (ISMultiSyncHandler == null)
        //                                {
        //                                    ISMultiSyncHandler = new ISMultiSyncHandler(cameraModel.IP, "80", cameraModel.ISSlaveIP, "80");
        //                                    await ISMultiSyncHandler.FirtConnectionAsync();
        //                                    await Task.Delay(2000);
        //                                }
        //                                // Reconnect 2 times
        //                                if (!cameraModel.IsConnected)
        //                                {
        //                                    cameraModel.CountTimeReconnect++;
        //                                    if (cameraModel.CountTimeReconnect >= 2)
        //                                    {
        //                                        cameraModel.CountTimeReconnect = 0;
        //                                    }
        //                                }
        //                                break;

        //                            case CameraType.UKN:

        //                                break;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        cameraModel.CountTimeReconnect = 0;
        //                    }
        //                }
        //                Thread.Sleep(2000);
        //            }
        //            catch (Exception)
        //            {
        //            }
        //        }
        //    })
        //    {
        //        IsBackground = true,
        //        Priority = ThreadPriority.Normal
        //    };
        //    _ThreadMonitorCamera.Start();
        //}
        private void UpdateStatusLabelCamera()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatusLabelCamera()));
                return;
            }
            for (int i = 0; i < Shared.Settings.CameraList.Count; i++)
            {
                if (i < _LabelStatusCameraList.Count)
                {
                    CameraModel cameraModel = Shared.Settings.CameraList[i];
                    ToolStripLabel labelStatusCamera = _LabelStatusCameraList[i];
                    if (cameraModel.IsConnected)
                    {
                        ShowLabelIcon(labelStatusCamera, Lang.CameraTMP, Properties.Resources.icons8_camera_30px_connected);
                    }
                    else
                    {
                        ShowLabelIcon(labelStatusCamera, Lang.CameraTMP, Properties.Resources.icons8_camera_30px_disconnected);
                    }
                }
            }
        }
        private void ShowLabelIcon(System.Windows.Forms.Label label, string text, System.Drawing.Image icon)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowLabelIcon(label, text, icon)));
                return;
            }

            if (label.Tag == icon)
            {
                return;
            }

            label.Tag = icon;
            label.ImageAlign = ContentAlignment.MiddleLeft;
            label.TextAlign = ContentAlignment.MiddleRight;
            int gap = 0;
            label.Text = text;
            label.Image = icon;
            label.AutoSize = true;
            int autoWidth = label.Width;
            label.AutoSize = false;
            label.Width = autoWidth + gap + label.Image.Width;
        }
        #endregion Camera Connection

        #region Monitor_Sensor_Controller
        //private void MonitorSensorControllerConnection()
        //{
        //    _ThreadMonitorSensorController = new Thread(() =>
        //    {
        //        int counter = 0;
        //        while (true)
        //        {
        //            try
        //            {
        //                if (Shared.Settings.SensorControllerEnable)
        //                {
        //                    if (Shared.SensorController == null || counter >= 3)
        //                    {
        //                        Shared.SensorController = null;
        //                        Shared.SensorController = new PODController(Shared.Settings.SensorControllerIP, Shared.Settings.SensorControllerPort, Shared.Settings.SensorControllerPort2, 1000, 1000);
        //                        Shared.SensorController.Connect();
        //                        Shared.SensorController.Connect2();
        //                        Shared.SensorController.OnPODReceiveMessageEvent -= SensorController_OnPODReceiveMessageEvent;
        //                        Shared.SensorController.OnPODReceiveMessageEvent += SensorController_OnPODReceiveMessageEvent;
        //                        counter = 0;
        //                    }
        //                    else
        //                    {
        //                        bool checkIP = Shared.SensorController.ServerIP == Shared.Settings.SensorControllerIP;
        //                        if (checkIP)
        //                        {
        //                            bool checkPort = Shared.SensorController.Port == Shared.Settings.SensorControllerPort;
        //                            if (!checkPort)
        //                            {
        //                                Shared.SensorController.Disconnect();
        //                                Shared.SensorController = null;
        //                            }
        //                            bool checkPort2 = Shared.SensorController.Port2 == Shared.Settings.SensorControllerPort2;
        //                            if (!checkPort2)
        //                            {
        //                                Shared.SensorController.Disconnect();
        //                                Shared.SensorController = null;
        //                            }
        //                            if (!SensorController.IsConnected2() && checkPort2)
        //                            {
        //                                SensorController.Connect2();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            Shared.SensorController.Disconnect();
        //                            Shared.SensorController = null;
        //                        }
        //                    }
        //                    if (Shared.SensorController.IsConnected() == false)
        //                    {
        //                        Shared.SensorController.Disconnect();
        //                        Shared.SensorController.Connect();
        //                        counter++;
        //                    }
        //                    else
        //                    {
        //                        counter = 0;
        //                    }

        //                    if (Shared.IsSensorControllerConnected != Shared.SensorController.IsConnected())
        //                    {
        //                        Shared.IsSensorControllerConnected = Shared.SensorController.IsConnected();
        //                        ProjectLogger.WriteInfo($"[SensorController] Trạng thái thay đổi → {(Shared.IsSensorControllerConnected ? "Connected" : "Disconnected")} [{Shared.Settings.SensorControllerIP}]");
        //                        UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
        //                        Shared.RaiseSensorControllerChangeEvent();

        //                        if (Shared.IsSensorControllerConnected)
        //                            Shared.SendSettingToSensorController();
        //                    }
        //                }
        //            }
        //            catch (Exception) { }
        //            Thread.Sleep(2000);
        //        }
        //    })
        //    {
        //        IsBackground = true,
        //        Priority = ThreadPriority.Normal
        //    };
        //    _ThreadMonitorSensorController.Start();
        //}
        private void MonitorSensorControllerConnection()
        {
            _ThreadMonitorSensorController = new Thread(() =>
            {
                int counter = 0;
                while (true)
                {
                    try
                    {
                        if (Shared.Settings.SensorControllerEnable)
                        {
                            if (Shared.SensorController == null || counter >= 3)
                            {
                                Shared.SensorController = null;
                                Shared.SensorController = new PODController(Shared.Settings.SensorControllerIP, Shared.Settings.SensorControllerPort, Shared.Settings.SensorControllerPort2, 1000, 1000);
                                Shared.SensorController.Connect();
                                if (Shared.Settings.SensorControllerPort2 > 0)
                                    Shared.SensorController.Connect2();
                                Shared.SensorController.OnPODReceiveMessageEvent -= SensorController_OnPODReceiveMessageEvent;
                                Shared.SensorController.OnPODReceiveMessageEvent += SensorController_OnPODReceiveMessageEvent;
                                counter = 0;
                            }
                            else
                            {
                                bool checkIP = Shared.SensorController.ServerIP == Shared.Settings.SensorControllerIP;
                                if (checkIP)
                                {
                                    bool checkPort = Shared.SensorController.Port == Shared.Settings.SensorControllerPort;
                                    if (!checkPort) { Shared.SensorController.Disconnect(); Shared.SensorController = null; }
                                    if (Shared.Settings.SensorControllerPort2 > 0)
                                    {
                                        bool checkPort2 = Shared.SensorController.Port2 == Shared.Settings.SensorControllerPort2;
                                        if (!checkPort2) { Shared.SensorController.Disconnect(); Shared.SensorController = null; }
                                        if (Shared.SensorController != null && !SensorController.IsConnected2() && checkPort2)
                                            SensorController.Connect2();
                                    }
                                }
                                else
                                {
                                    Shared.SensorController.Disconnect();
                                    Shared.SensorController = null;
                                }
                            }

                            // Gọi IsConnected() 1 lần duy nhất mỗi vòng lặp
                            bool isCurrentlyConnected = Shared.SensorController.IsConnected();

                            // Debounce: chỉ kết luận mất kết nối sau 3 lần liên tiếp
                            if (!isCurrentlyConnected)
                            {
                                _plcDisconnectDebounce++;
                            }
                            else
                            {
                                _plcDisconnectDebounce = 0;
                                counter = 0;
                            }

                            // Thực sự mất kết nối (qua debounce) → reconnect
                            if (_plcDisconnectDebounce >= 3)
                            {
                                Shared.SensorController.Disconnect();
                                Shared.SensorController.Connect();
                                counter++;
                                _plcDisconnectDebounce = 0;
                                isCurrentlyConnected = Shared.SensorController.IsConnected();
                            }

                            // Cập nhật trạng thái UI khi thay đổi (chỉ khi kết nối thật sự thay đổi, bỏ qua glitch)
                            if (Shared.IsSensorControllerConnected != isCurrentlyConnected)
                            {
                                // Đang đếm debounce (1-2) → chưa xác nhận, bỏ qua
                                if (!isCurrentlyConnected && _plcDisconnectDebounce > 0 && _plcDisconnectDebounce < 3)
                                {
                                    // skip — chưa đủ 3 lần để kết luận mất kết nối
                                }
                                else
                                {
                                    bool wasConnected = Shared.IsSensorControllerConnected;
                                    Shared.IsSensorControllerConnected = isCurrentlyConnected;
                                    ProjectLogger.WriteInfo($"[SensorController] Trạng thái thay đổi → {(Shared.IsSensorControllerConnected ? "Connected" : "Disconnected")} [{Shared.Settings.SensorControllerIP}]");
                                    UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
                                    Shared.RaiseSensorControllerChangeEvent();

                                    if (Shared.IsSensorControllerConnected)
                                    {
                                        _plcWasEverConnected = true;
                                        Shared.SendSettingToSensorController();
                                        // Bỏ gửi ARED khi đăng nhập lại — giữ trạng thái đèn hiện tại
                                        Shared.PendingLoginRed = false;
                                    }

                                    // Feature 2: PLC mất kết nối → log server (không gửi tín hiệu PLC vì PLC đã offline)
                                    if (wasConnected && !Shared.IsSensorControllerConnected)
                                        NotifyDeviceException(DeviceExceptionType.PlcDisconnected,
                                            $"PLC/SensorController [{Shared.Settings.SensorControllerIP}] mất kết nối");
                                }
                            }

                            // Mất kết nối PLC >10s liên tục → hiển thị thông báo giữa màn hình
                            if (!isCurrentlyConnected && _plcWasEverConnected)
                            {
                                if (_plcDisconnectedSince == DateTime.MinValue)
                                    _plcDisconnectedSince = DateTime.Now;

                                if ((DateTime.Now - _plcDisconnectedSince).TotalSeconds >= 10
                                    && !_plcDisconnectAlertShown)
                                {
                                    _plcDisconnectAlertShown = true;
                                    bool isRunning = Shared.OperStatus == OperationStatus.Running
                                                  || Shared.OperStatus == OperationStatus.Processing;
                                    ProjectLogger.WriteInfo($"[DISCONNECT] [PLC] >10s lost → Running={isRunning}");
                                    BeginInvoke(new Action(() =>
                                    {
                                        CustomMessageBox.ShowCenterScreen("PLC mất kết nối hơn 10 giây, vui lòng kiểm tra lại kết nối!",
                                            "Mất kết nối PLC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }));
                                     if (isRunning)
                                     {
                                         if (_FormMainPC != null && !_FormMainPC.IsDisposed && _FormMainPC.IsHandleCreated)
                                             _FormMainPC.BeginInvoke(new Action(() => _FormMainPC?.RequestAutoStop("PLC mất kết nối >10s")));
                                     }
                                }
                            }
                            else
                            {
                                _plcDisconnectedSince = DateTime.MinValue;
                                _plcDisconnectAlertShown = false;
                            }
                        }
                    }
                    catch (Exception) { }
                    Thread.Sleep(2000);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitorSensorController.Start();
        }
        #endregion Monitor_Sensor_Controller

        #region Monitor_SerialDevice_Controller

        private void ConnectSerialDeviceController()
        {
            Shared.SerialDevController = null;
            Shared.SerialDevController = new SerialDeviceController(Shared.Settings.SerialDivComName, Shared.Settings.SerialDivBitPerSecond, Shared.Settings.SerialDivDataBits,
                                                                                    Shared.Settings.SerialDivParity, Shared.Settings.SerialDivStopBits);
            bool isConnect = Shared.SerialDevController.ConnectSerialDevice();
            UpdateUISerialDeviceControllerStatus(isConnect);
            Shared.IsSerialDeviceConnected = isConnect;
        }
        private void MonitorSerialDeviceControllerConnection()
        {
            _ThreadMonitorSerialDeviceController = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (Shared.Settings.EnSerialDevice && (Shared.SerialDevController == null || !Shared.SerialDevController.IsSerialDevConnected()))
                        {
                            ConnectSerialDeviceController();
                        }
                        if (!Shared.Settings.EnSerialDevice)
                        {
                            Shared.SerialDevController?.DisconnectSerialDevice();
                        }

                        if (Shared.SerialDevController != null)
                        {
                            UpdateUISerialDeviceControllerStatus(Shared.SerialDevController.IsSerialDevConnected());
                            Shared.IsSerialDeviceConnected = Shared.SerialDevController.IsSerialDevConnected();
                        }
                        else
                        {
                            UpdateUISerialDeviceControllerStatus(false);
                            Shared.IsSerialDeviceConnected = false;
                        }

                        Shared.RaiseSerialDeviceControllerChangeEvent();
                    }
                    catch (Exception) { }
                    Thread.Sleep(2000);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitorSerialDeviceController.Start();
        }

        #endregion Monitor_SerialDevice_Controller
        private void MonitorDatabaseConnection()
        {
            if (_ThreadMonitorDatabase != null && _ThreadMonitorDatabase.IsAlive)
                return;

            _ThreadMonitorDatabase = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        bool isAlive = ucProductionTHTrueMilkSetting.IsConnectionAlive();

                        if (!isAlive)
                        {
                            if (Shared.IsDatabaseConnected)
                            {
                                Shared.IsDatabaseConnected = false;
                                ProjectLogger.WriteWarning("[DB] Mất kết nối database — đang thử kết nối lại...");
                                Shared.RaiseOnDatabaseStatusChangeEvent();
                                UpdateStatusLabelDatabase();
                            }

                            if (!_isReconnecting)
                            {
                                _isReconnecting = true;
                                Task.Run(async () =>
                                {
                                    try
                                    {
                                        await ucProductionTHTrueMilkSetting.AutoConnectFromSettingsAsync();
                                    }
                                    finally
                                    {
                                        _isReconnecting = false;
                                    }
                                });
                            }
                        }
                        else if (!Shared.IsDatabaseConnected)
                        {
                            Shared.IsDatabaseConnected = true;
                            ProjectLogger.WriteInfo("[DB] Kết nối database đã được phục hồi.");
                            Shared.RaiseOnDatabaseStatusChangeEvent();
                            UpdateStatusLabelDatabase();
                        }
                    }
                    catch (Exception ex)
                    {
                        ProjectLogger.WriteError("[DB] MonitorDatabaseConnection lỗi: " + ex.Message, ex);
                    }

                    Thread.Sleep(5000);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitorDatabase.Start();
        }
        private void dgvItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabCreateRevOffline_Click(object sender, EventArgs e)
        {

        }

        private async void ResendPushDatabase(object sender, EventArgs e)
        {
            int lineIndex = dgvHistoryJob.SelectedRows[0].Index;
            string JobName = dgvHistoryJob.Rows[lineIndex].Cells[1].Value.ToString();

            CurrentJob = GetJob(JobName);

            var listQrcodes = FileFuncs.ReadStringListFromCsv(CurrentJob.DirectoryDatabase);

            PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder); //Problem here , have to seperate PO and Reservation
            CurrentJob.isPushedDatabase = false;
            bool isSent = CurrentJob.isPushedDatabase = isPushDatabase = await SendGeneratedCodes(listQrcodes, CurrentJob);
            if (isSent)
            {
                CuzMessageBox.Show("G?i d? li?u ban đ?u thành công!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                CuzMessageBox.Show("G?i d? li?u ban đ?u th?y b?i!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void lblSensorControllerStatus_Click(object sender, EventArgs e)
        {

        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {

        }

        private void btnGetInfo_Click(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void picSaveJobLoading_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label36_Click(object sender, EventArgs e)
        {

        }

        private void statusStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void tabLogSync_Click(object sender, EventArgs e) { }
        private void btnSyncSelected_Click_1(object sender, EventArgs e) { }

        private void lblModeOperator_Click(object sender, EventArgs e)
        {

        }

        private void label31_Click(object sender, EventArgs e)
        {

        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void dtpToDateQrRecive_ValueChanged(object sender, EventArgs e)
        {

        }

#if DEBUG
        private void InitializeDebugControls()
        {
            _pnlDebug = new System.Windows.Forms.Panel
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(750, 35),
                BackColor = System.Drawing.Color.LightYellow,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                Visible = true
            };

            int x = 5;
            var lbl = new System.Windows.Forms.Label
            {
                Text = "Camera Programs (DEBUG):",
                Width = 150,
                Height = 20,
                Location = new System.Drawing.Point(x, 8)
            };
            x += 155;

            _txtCameraPrograms = new System.Windows.Forms.TextBox
            {
                Text = "0001_8935217401130,0002_8935217401758,0003_8935217401765",
                Width = 400,
                Height = 25,
                Location = new System.Drawing.Point(x, 5)
            };
            x += 405;

            _btnLoadPrograms = new System.Windows.Forms.Button
            {
                Text = "Load",
                Width = 80,
                Height = 25,
                Location = new System.Drawing.Point(x, 5),
                BackColor = System.Drawing.Color.LightGreen
            };
            _btnLoadPrograms.Click += (s, e) =>
            {
                string text = _txtCameraPrograms.Text?.Trim();
                var programs = string.IsNullOrEmpty(text)
                    ? new List<string>()
                    : text.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(item => item.Trim())
                          .Where(item => !string.IsNullOrEmpty(item))
                          .ToList();
                Shared.Settings.CachedCameraPrograms = programs;
                Debug.WriteLine($"[JobDebug] Loaded {programs.Count} camera programs: {string.Join(", ", programs)}");

                if (cbcSelectedModel != null)
                {
                    cbcSelectedModel.Items.Clear();
                    foreach (var p in programs)
                        cbcSelectedModel.Items.Add(p);
                    if (programs.Count > 0)
                        cbcSelectedModel.SelectedIndex = 0;
                }
            };
            x += 85;

            _btnClearPrograms = new System.Windows.Forms.Button
            {
                Text = "Clear",
                Width = 60,
                Height = 25,
                Location = new System.Drawing.Point(x, 5),
                BackColor = System.Drawing.Color.LightCoral
            };
            _btnClearPrograms.Click += (s, e) =>
            {
                Shared.Settings.CachedCameraPrograms = new List<string>();
                if (cbcSelectedModel != null) cbcSelectedModel.Items.Clear();
                Debug.WriteLine("[JobDebug] Cleared camera programs cache");
            };

            _pnlDebug.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lbl, _txtCameraPrograms, _btnLoadPrograms, _btnClearPrograms
            });

            this.Controls.Add(_pnlDebug);
            _pnlDebug.BringToFront();
        }
#endif
    }

    internal class SyncNotificationForm : Form
    {
        private readonly Label _lblTitle;
        private readonly Label _lblStatus;
        private readonly ProgressBar _progressBar;
        private readonly Button _btnAction;
        private readonly TaskCompletionSource<bool> _confirmTcs = new TaskCompletionSource<bool>();
        public readonly CancellationTokenSource Cts = new CancellationTokenSource();

        public SyncNotificationForm(string title = "")
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new System.Drawing.Size(420, 200);
            BackColor = System.Drawing.Color.White;
            ShowInTaskbar = false;
            ShowIcon = false;
            ControlBox = false;
            Padding = new Padding(0);
            Margin = new Padding(0);

            _lblTitle = new Label
            {
                Text = string.IsNullOrEmpty(title) ? "🔄 Đang đồng bộ..." : title,
                Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(30, 90, 160),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 380, Height = 36,
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            _progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 25,
                Width = 340, Height = 6,
                Location = new System.Drawing.Point(40, 72),
                ForeColor = System.Drawing.Color.FromArgb(0, 120, 215),
                BackColor = System.Drawing.Color.FromArgb(230, 240, 250)
            };

            _lblStatus = new Label
            {
                Text = "Đang chuẩn bị…",
                Font = new System.Drawing.Font("Segoe UI", 10.5f),
                ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 380, Height = 40,
                Location = new System.Drawing.Point(20, 95),
                AutoSize = true
            };

            _btnAction = new Button
            {
                Text = "Hủy",
                Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White,
                BackColor = System.Drawing.Color.FromArgb(220, 53, 69),
                FlatStyle = FlatStyle.Flat,
                Width = 130, Height = 36,
                Location = new System.Drawing.Point(145, 145),
                Cursor = Cursors.Hand
            };
            _btnAction.FlatAppearance.BorderSize = 0;
            _btnAction.Click += (_, __) =>
            {
                if (_btnAction.Text.Contains("Xác nhận"))
                {
                    _confirmTcs.TrySetResult(true);
                }
                else
                {
                    Cts?.Cancel();
                    _btnAction.Enabled = false;
                    _btnAction.Text = "Đang dừng...";
                }
            };

            Controls.Add(_lblTitle);
            Controls.Add(_progressBar);
            Controls.Add(_lblStatus);
            Controls.Add(_btnAction);
        }

        public void SetStatus(string text)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => SetStatus(text))); return; }
            _lblStatus.Text = text;
        }

        public void SetComplete(int sentIn, int sentCam, int sentErr, int totalFail, int totalStop)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => SetComplete(sentIn, sentCam, sentErr, totalFail, totalStop))); return; }
            _progressBar.Visible = false;

            bool wasCancelled = Cts.IsCancellationRequested;
            bool hasError = totalFail > 0 && sentIn + sentCam + sentErr == 0;

            if (wasCancelled)
            {
                _lblTitle.Text = "ĐÃ DỪNG";
                _lblTitle.ForeColor = System.Drawing.Color.FromArgb(255, 165, 0);
            }
            else if (hasError)
            {
                _lblTitle.Text = "LỖI HỆ THỐNG";
                _lblTitle.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
            }
            else if (totalFail > 0)
            {
                _lblTitle.Text = "ĐỒNG BỘ 1 PHẦN";
                _lblTitle.ForeColor = System.Drawing.Color.FromArgb(200, 140, 0);
            }
            else
            {
                _lblTitle.Text = "THÀNH CÔNG";
                _lblTitle.ForeColor = System.Drawing.Color.FromArgb(0, 140, 70);
            }

            string detail = $"Log máy in: {sentIn}  |  Log camera: {sentCam}  |  Log lỗi camera: {sentErr}";
            if (totalFail > 0) detail += $"  |  {totalFail} thất bại";
            if (totalStop > 0) detail += $"  |  {totalStop} bỏ qua";
            _lblStatus.Text = detail;

            _btnAction.Text = "Xác nhận";
            _btnAction.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            _btnAction.Enabled = true;
        }

        public Task<bool> WaitForConfirmAsync() => _confirmTcs.Task;

        protected override bool ShowWithoutActivation => true;
    }
}

