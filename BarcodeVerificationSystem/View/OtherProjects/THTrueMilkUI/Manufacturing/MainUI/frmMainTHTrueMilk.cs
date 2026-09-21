using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Controller.Camera.Keyence;
using BarcodeVerificationSystem.Controller.ZebraPrinter;
using BarcodeVerificationSystem.Interfaces;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Apis.THTrueMilk.Manufacturing;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using BarcodeVerificationSystem.Model.THTrueMilk;
using BarcodeVerificationSystem.Model.UDT;
using BarcodeVerificationSystem.Model.UserInfo;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Core;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Factories;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Woka;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services.THTrueMilk;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models; 
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.Logging;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.View.OtherProjects.THMilkUI;
using BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing;
using BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing;
using BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing.MainUI;
using BarcodeVerificationSystem.View.UcSettings;
using BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess;
using BarcodeVerificationSystem.View.UtilityForms.THTrueMilk;
using CommonVariable;
using DesignUI.CuzAlert;
using DesignUI.CuzMesageBox;
using GenCode.Utils;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Mysqlx.Crud;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OperationLog.Controller;
using OperationLog.Model;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;
using static BarcodeVerificationSystem.Model.SyncDataParams;
using static BarcodeVerificationSystem.Services.THTrueMilk.THDb;
using static BarcodeVerificationSystem.Utils.UIControlsFuncs;
using static BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing.frmJobTHTrueMilk;
using OperationCanceledException = System.OperationCanceledException;
using OperationStatus = BarcodeVerificationSystem.Model.OperationStatus;
using SyncDataParams = BarcodeVerificationSystem.Model.THTrueMilk.SyncDataParams;
using Timer = System.Windows.Forms.Timer;

namespace BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing
{
    public partial class frmMainTHTrueMilk : Form
    {
        #region VARIABLES DEFINITION
        private readonly frmJobTHTrueMilk _ParentForm = null;
        private JobModel _SelectedJob = new JobModel();
        private bool _IsPrinterDisconnectedNot = false;
        private bool _IsReCheck = false;
        public ManufacturingService ManufacturingService = new ManufacturingService();
        private int _printedCsvSaveCounter = 0;

        private readonly Timer _TimerDateTime = new Timer();
        private readonly string _DateTimeFormatTicker = "yyyy/MM/dd hh:mm:ss tt";

        private int _TotalCode = 0;
        private int _TotalChecked = 0;
        private int _TotalMissed = 0;
        private int _ReceivedCode = 0;
        private int _NumberPrinted = 0;
        private int _NumberOfCheckPassed = 0;
        private int _NumberOfCheckFailed = 0;
        private int _DateCheckPassed = 0;
        private int _DateCheckFailed = 0;
        private int _NumberOfSentPrinter = 0;
        private int _printerBuffer = 0; // buffer từ RSFP: received - printed
        private int _printedFileCount = 0; // số dòng trong PrintedResponse CSV
        private int _lastRsfpReceived = 0; // lưu received từ RSFP cuối cùng để RSMPOD dùng
        private DateTime _lastReconnectTime = DateTime.MinValue; // thời điểm reconnect TCP gần nhất
        private int _NumberOfDuplicate = 0;
        private int _errorCountA = 0;
        private int _errorCountB = 0;
        private int _errorCountF = 0;
        private int _phanLoaiCount = 0;
        private DateTime _lastRsfpTimestamp = DateTime.MinValue;
        private int _rsfpMissingSentCount = 0; // số mã đã gửi bù khi RSFP mất (reset khi RSFP quay lại)
        private DateTime _lastDataSendTimestamp = DateTime.MinValue;
        private int _totalDataSent = 0;
        private int _totalRsfpReceived = 0;
        /// <summary>
        /// Last printed page từ Form Printer (WPL/PRT command response).
        /// Dùng để tính A = NumberPrinted - _lastPrintedPageFormPrinter (số mã đã in trong phiên hiện tại).
        /// pending = _NumberOfSentPrinter - A = buffer thực tế.
        /// Tạm mặc định 0, sẽ được set từ command printer sau.
        /// </summary>
        private int _lastPrintedPageFormPrinter = 0;
        private int _sessionStartCount = 0;
        private int _receivedFinishPrintFormMON = 0;
        private int _sessionFirstRsfpPrinted = 0; // 
        private DateTime _lastMonTimestamp = DateTime.MinValue; // thời điểm MON cuối cùng
        private int _sessionFirstMonPrinted = -1; // baseline MON khi RSFP chưa về
        private volatile bool _isDataLoaded = false;
        private bool _printedBaseSet = false;
        private int _consecutiveTimeouts = 0;
        private bool _isInTimeout = false;
        private frmLoadingUi _loadingStop = null;
        private int _prevConsecutiveTimeouts = 0;
        private DateTime _connectedSince = DateTime.MinValue;
        private DateTime _lastPerfLogTime = DateTime.MinValue;
        private int _lastBufferWarnPct = 0;
        private frmLoadingUi _loadingRebuild;
        private bool _isStartRebuild = false;
        private string _deferredAlertMessage;
        private string _deferredQrLowAlert;
        private readonly int _TotalColumns = 1;
        private readonly int _StartIndex = 1;
        private volatile int _dynamicStopCond = 0;
        private System.Windows.Forms.Timer _dbStatusSyncTimer;

        #region sync Data parameters
        private int _SaaSSuccessCodes = 0;
        private int _SAPSuccessCodes = 0;

        private int _CheckSaaSSuccessCodes = 0;
        private int _CheckSAPSuccessCodes = 0;

        public int SaaSSuccess { get { return _SaaSSuccessCodes; } set { _SaaSSuccessCodes = value; Invoke(new Action(() => { sentSaaSSuccess.Text = string.Format("{0:N0}", _SaaSSuccessCodes); })); } }
        public int SAPSuccess { get { return _SAPSuccessCodes; } set { _SAPSuccessCodes = value; Invoke(new Action(() => { sentSAPSuccess.Text = string.Format("{0:N0}", _SAPSuccessCodes); })); } }

        public int CheckSaaSSuccess { get { return _CheckSaaSSuccessCodes; } set { _CheckSaaSSuccessCodes = value; Invoke(new Action(() => { sentCheckSaaSSuccess.Text = string.Format("{0:N0}", _CheckSaaSSuccessCodes); })); } }
        public int CheckSAPSuccess { get { return _CheckSAPSuccessCodes; } set { _CheckSAPSuccessCodes = value; Invoke(new Action(() => { sentCheckSAPSuccess.Text = string.Format("{0:N0}", _CheckSAPSuccessCodes); })); } }


        private int _SAPFailedCodes = 0;
        private int _SentSyncData = 0;
        private int _SaaSFailedCodes = 0;
        public int SentSyncData { get { return _SentSyncData; } set { _SentSyncData = value; Invoke(new Action(() => { txtCodeResult.Text = string.Format("{0:N0}", _SentSyncData); })); } }
        //public int SaaSFailed { get { return _SaaSFailedCodes; } set { _SaaSFailedCodes = value; Invoke(new Action(() => { SaaSFailedCodes.Text = string.Format("{0:N0}", _SaaSFailedCodes); })); } }
        //public int SAPFailed { get { return _SAPFailedCodes; } set { _SAPFailedCodes = value; Invoke(new Action(() => { SAPFailedCodes.Text = string.Format("{0:N0}", _SAPFailedCodes); })); } }

        #endregion

        #region Runtime tracking
        public TimeSpan AccumulatedRunTime = TimeSpan.Zero;
        public Stopwatch RunStopwatch = new Stopwatch();
        private System.Windows.Forms.Timer _runTimeTimer;

        private static string FormatRunTime(TimeSpan ts)
        {
            int totalHours = (int)ts.TotalHours;
            return $"{totalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        // Batch UI update — Timer-based
        private System.Windows.Forms.Timer _uiBatchTimer;
        private volatile bool _uiUpdatePending = false;
        private int _uiInvalidateCounter = 0;
        private int _progressBarCounter = 0;
        private int _lastSelectedRow = -1;
        // Change detection fields — chỉ update label khi giá trị thay đổi
        private int _sessionPrintedCount = 0;
        private int _prevPrintedCount = -1;
        private int _prevTotalPrintedCount = -1;
        private int _prevTotalChecked = -1;
        private int _prevCheckPassed = -1;
        private int _prevCheckFailed = -1;
        private int _prevReceived = -1;
        private int _prevErrorA = -1;
        private int _prevErrorB = -1;
        private int _prevErrorF = -1;
        private int _prevPhanLoai = -1;
        private int _prevRemaining = -1;
        private double _prevRatioA = -1;
        private double _prevRatioB = -1;
        private double _prevRatioF = -1;
        // Persistent StreamWriter cho printed file
        // _printedWriter removed — using local StreamWriter via using block
#if DEBUG
        // ── Camera Simulation Controls ──
        private System.Windows.Forms.Panel _pnlDebug;
        private System.Windows.Forms.Button _btnCameraSim;
        private System.Windows.Forms.NumericUpDown _numSimSpeed;
        private System.Windows.Forms.Button _btnVirtualPrinter;
        private System.Windows.Forms.Button _btnSimStartStop;
        private System.Windows.Forms.Label _lblSimStatus;
        private System.Windows.Forms.TextBox _txtSimInput;
        private System.Windows.Forms.Button _btnSimSend;
        private System.Windows.Forms.TextBox _txtCameraPrograms;
        private System.Windows.Forms.Button _btnLoadPrograms;
        private System.Windows.Forms.Button _btnClearPrograms;
        private System.Threading.CancellationTokenSource _simCTS;
        private int _simIndex = 0;
#endif
        #endregion

        public int TotalRlinkPrinted
        {
            get
            {
                var vl = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                return (vl != null)
                    ? vl.GetPrintedCount()
                    : (_PrintedCodeObtainFromFile as System.Collections.Generic.List<string[]>)?.Count(r => r.Length > 1 && r[1] == "Printed") ?? 0;
            }
        }

        public int TotalChecked { get { return _TotalChecked; } set { _TotalChecked = value; _uiUpdatePending = true; } }
        public int NumberOfCheckPassed { get { return _NumberOfCheckPassed; } set { _NumberOfCheckPassed = value; _uiUpdatePending = true; } }
        public int NumberOfCheckFailed { get { return _NumberOfCheckFailed; } set { _NumberOfCheckFailed = value; _uiUpdatePending = true; } }
        private DateTime _lastPrintedUiUpdate = DateTime.MinValue;
        public int NumberPrinted { get { return _NumberPrinted; } set {
                int captured = value;
                // Nếu printer reset (count < sessionStart), reset baseline
                if (_sessionStartCount > 0 && captured < _sessionStartCount)
                    _sessionStartCount = captured;
                _NumberPrinted = captured;
                _uiUpdatePending = true;
                // Throttle UI update: max 1 lần / 200ms để tránh BeginInvoke tích lũy
                if ((DateTime.Now - _lastPrintedUiUpdate).TotalMilliseconds >= 200)
                {
                    _lastPrintedUiUpdate = DateTime.Now;
                    BeginInvoke(new Action(() => {
                        //int sessionCount = captured - _sessionStartCount;
                        //if (sessionCount < 0) sessionCount = 0;
                        //lblPrintedCodeValue.Text = string.Format("{0:N0}", sessionCount);

                        //int sessionCount = TotalRlinkPrinted - _sessionStartCount;oper
                        //if (sessionCount < 0) sessionCount = 0;
                        //lblPrintedCodeValue.Text = string.Format("{0:N0}", sessionCount);

                        double expectedCodes = _SelectedJob?.NumberTotalsCode ?? 0;
                    double expectedTons = _SelectedJob?.EstimatedTons ?? 0;
                    double actualTons = 0;
                    if (expectedCodes > 0 && expectedTons > 0)
                    {
                        actualTons = (_SelectedJob?.TotalRlinkPrinted ?? 0) * expectedTons / expectedCodes;
                    }
                    lblTotalQrcode.Text = $"SP thực tế: {_SelectedJob?.TotalRlinkPrinted ?? 0:N0}";
                    lblTotalTon.Text = $"Số tấn thực tế: {actualTons:N2}";
                })); } } }
        public int ErrorCountA { get { return _errorCountA; } set { _errorCountA = value; _uiUpdatePending = true; } }
        public int ErrorCountB { get { return _errorCountB; } set { _errorCountB = value; _uiUpdatePending = true; } }
        public int ErrorCountF { get { return _errorCountF; } set { _errorCountF = value; _uiUpdatePending = true; } }
        public int PhanLoaiCount { get { return _phanLoaiCount; } set { _phanLoaiCount = value; _uiUpdatePending = true; } }
        private void UpdatePhanLoaiRemaining()
        {
            int remaining = _errorCountF - _phanLoaiCount;
            if (remaining < 0) remaining = 0;
            Invoke(new Action(() => { if (label22 != null) label22.Text = string.Format("Camera phân loại lỗi: {0:N0}", remaining); }));
        }
        public int ReceivedCode { get { return _ReceivedCode; } set { _ReceivedCode = value; _uiUpdatePending = true; } }

        public static int startIndex = 0;
        public string _PixelToMMX = "";
        public string _PixelToMMY = "";
        public double PixelToMmX = 0;
        public double PixelToMmY = 0;

        private long _SendPodTimeMs;
        public long SendPodTimeMs
        {
            get { return _SendPodTimeMs; }
            set
            {
                if (_SendPodTimeMs != value)
                {
                    _SendPodTimeMs = value;
                    Invoke(new Action(() =>
                    {
                        labelTimeSent.Text = string.Format("({0} ms)", _SendPodTimeMs);
                    }));
                }
            }
        }

        public int NumberOfSentPrinter
        {
            get { return _NumberOfSentPrinter; }
            set
            {
                Interlocked.Exchange(ref _NumberOfSentPrinter, value);
                SafeUpdateSentLabel();
            }
        }

        private void SafeUpdateSentLabel()
        {
            var val = _NumberOfSentPrinter;
            if (InvokeRequired)
                BeginInvoke(new Action(() => { lblSentDataValue.Text = string.Format("{0:N0}", val); }));
            else
                lblSentDataValue.Text = string.Format("{0:N0}", val);
        }

        private void IncrementSentPrinter()
        {
            int val = Interlocked.Increment(ref _NumberOfSentPrinter);
            if (_SelectedJob != null) _SelectedJob.TotalSent++;
            if (InvokeRequired)
                BeginInvoke(new Action(() => { lblSentDataValue.Text = string.Format("{0:N0}", val); }));
            else
                lblSentDataValue.Text = string.Format("{0:N0}", val);
        }

        private bool IsRsfpStale()
        {
            return _sessionFirstRsfpPrinted < 0
                || _lastRsfpTimestamp == DateTime.MinValue
                || (DateTime.Now - _lastRsfpTimestamp).TotalSeconds > 5;
        }

        private readonly int _MaxDatabaseLine = 500;
        private readonly List<ToolStripLabel> _LabelStatusCameraList = new List<ToolStripLabel>();
        private readonly List<ToolStripLabel> _LabelStatusPrinterList = new List<ToolStripLabel>();

        readonly static object _SyncObjCodeList = new object();
        readonly static object _SyncObjCheckedResultList = new object();
        private readonly string _DateTimeFormat = "yyMMddHHmmss";
        private string[] _DatabaseColunms = new string[0];
        private readonly string[] defaultRecord = new string[] { "100000", "Valid", "data1", "01/01/2026", "01/07/2026", "01 01 26", "100", DateTime.Now.ToString(), "raw_camera_frame", "A" };
        private static readonly string _index = "STT", _resultData = "QR", _result = "Trạng thái"
                                       , _processingTime = "Thời gian xử lý",
                                       _receiveTime = "Thời gian nhận";
        private static readonly string _nsx = "NSX", _hsd = "HSD", _batchCol = "Batch",
                                       _frameInfo = "Camera details", _errorType = "Chất lượng";

        private static readonly string[] _ColumnNames = { _index, _result, _resultData, _nsx, _hsd,
                                                           _batchCol, _processingTime, _receiveTime, _frameInfo, _errorType };
        public static readonly int Index_Index = Array.IndexOf(_ColumnNames, _index), Index_ResultData = Array.IndexOf(_ColumnNames, _resultData), Index_Result = Array.IndexOf(_ColumnNames, _result),
                            Index_ProcessingTime = Array.IndexOf(_ColumnNames, _processingTime),
                            Index_DateTime = Array.IndexOf(_ColumnNames, _receiveTime),
                            Index_NSX = Array.IndexOf(_ColumnNames, _nsx),
                            Index_HSD = Array.IndexOf(_ColumnNames, _hsd),
                            Index_BatchCol = Array.IndexOf(_ColumnNames, _batchCol),
                            Index_FrameInfo = Array.IndexOf(_ColumnNames, _frameInfo),
                            Index_ErrorType = Array.IndexOf(_ColumnNames, _errorType);


        private bool _IsAfterProductionMode = false;
        private bool _vscCameraFrameSubscribed = false;
        private int _consecutiveErrorCount = 0;
        private bool _consecutiveErrorAlertShown;
        private string _lastCameraNsxWithTime = "";
        private string _lastCameraFrameInfo = "";
        private string _lastCameraHsd = "";
        private DateTime _lastCameraPacketReceived = DateTime.MinValue;
        private static string ErrorImageBasePath => CommonVariable.CommVariables.PathImagesError;
        private bool _IsOnProductionMode = false;
        private bool _IsVerifyAndPrintMode = false;
        private bool _IsPrintedWait = false;
        private bool _IsCheckedWait = true;
        private bool _IsPrintedResponse = false;
        private int _printerAckedCount = -1; // từ RSMPOD, dùng để tính pending khi RSFP thiếu
        private string _lastRsalCode = ""; // chống popup RSAL trùng nội dung
        private DateTime _lastRsalPopupTime = DateTime.MinValue;
        private RsalAlertForm _rsalErrorForm = null;
        private RsalAlertForm _rsalWarningForm = null;
        private readonly List<InitDataError> _InitDataErrorList = new List<InitDataError>();
        private ComparisonResult _CheckedResult = ComparisonResult.Valid;
        private ComparisonResult _PrintedResult = ComparisonResult.Valid;
        private PrinterStatus _PrinterStatus = PrinterStatus.Null;
        private readonly object _PrintLocker = new object();
        private readonly object _ReceiveLocker = new object();
        private readonly object _CheckLocker = new object();
        private readonly object _PrintedResponseLocker = new object();
        private readonly AutoResetEvent _printerFeedbackEvent = new AutoResetEvent(false);

        private Thread _ThreadPrinterResponseHandler = null;
        private Thread _ThreadMonitorSensorController = null;

        private readonly Queue<string> _QueueBufferPrintedResponse = new Queue<string>();
        public static readonly ConcurrentQueue<DetectModel> _QueueBufferDataObtained = new ConcurrentQueue<DetectModel>();
        private int _handlerCallCount = 0;

        public static ConcurrentQueue<string> _QueuePositionDataObtained = new ConcurrentQueue<string>();
        private readonly Model.THTrueMilk.SynchronizedQueue<DetectModel> _QueueBufferDataObtainedResult = new Model.THTrueMilk.SynchronizedQueue<DetectModel>();
        private readonly Model.THTrueMilk.SynchronizedQueue<Model.THTrueMilk.PrinterResponseData> _QueueBufferUpdateUIPrinter = new Model.THTrueMilk.SynchronizedQueue<Model.THTrueMilk.PrinterResponseData>();
        private readonly Model.THTrueMilk.SynchronizedQueue<ExportImageModel> _QueueBufferBackupImage = new Model.THTrueMilk.SynchronizedQueue<ExportImageModel>();
        private readonly ConcurrentQueue<(string imageId, int index, string jobName)> _pendingErrorImages = new ConcurrentQueue<(string, int, string)>();
        private readonly Model.THTrueMilk.SynchronizedQueue<List<string[]>> _QueueBufferBackupCheckedResult = new Model.THTrueMilk.SynchronizedQueue<List<string[]>>();
        private readonly Model.THTrueMilk.SynchronizedQueue<List<string[]>> _QueueBufferBackupPrintedCode = new Model.THTrueMilk.SynchronizedQueue<List<string[]>>();
        private readonly Model.THTrueMilk.SynchronizedQueue<object> _QueueBufferPrinterResponseData = new Model.THTrueMilk.SynchronizedQueue<object>();
        private readonly Model.THTrueMilk.SynchronizedQueue<string[]> _QueueBufferBackupSendLog = new Model.THTrueMilk.SynchronizedQueue<string[]>();
        private Thread _printedQrWorker;
        private volatile bool _isPrintedQrWorkerRunning;
        private IList<string[]> _PrintedCodeObtainFromFile = new List<string[]>();
        private List<string[]> _CheckedResultCodeList = new List<string[]>();
        private readonly ConcurrentDictionary<string, CompareStatus> _CodeListPODFormat = new ConcurrentDictionary<string, CompareStatus>();
        private ConcurrentDictionary<string, int> _Emergency = new ConcurrentDictionary<string, int>();

        private List<string[]> _SentPrintedCodeObtainFromFile = null;
        private List<string[]> _SentCheckedCodeObtainFromFile = null;

        private CancellationTokenSource _OperationCancelTokenSource;
        private CancellationTokenSource _UICheckedResultCancelTokenSource;
        private CancellationTokenSource _UIPrintedResponseCancelTokenSource;
        private CancellationTokenSource _BackupResultCancelTokenSource;
        private CancellationTokenSource _BackupResponseCancelTokenSource;
        private CancellationTokenSource _BackupImageCancelTokenSource;
        private CancellationTokenSource _SendDataToPrinterTokenCTS;
        private CancellationTokenSource _PrinterRespontCST;
        private CancellationTokenSource _VirtualCTS;
        private CancellationTokenSource _BackupSendLogCancelTokenSource;
        private CancellationTokenSource _BackupRSFPLogCancelTokenSource;
        private int _updatePrintedBatchCounter = 0;
        private bool _isStopping;
        private Task _backupResponseTask;
        private volatile int _isSendLoopRunning = 0; // guard chống concurrent send loop (0=false, 1=true)
        private Task _backupSendLogTask;
        private Task _backupRSFPLogTask;

        private bool _benchmarkMode = false;
        private CancellationTokenSource _benchmarkCTS;
        private bool _forceClose;
        private long _benchmarkSeq;
        private readonly AutoResetEvent _benchmarkPrintCycle = new AutoResetEvent(false);

        private FrmSettingsTHTrueMilk _FormSettings;
        private FrmViewHistoryProgram _FormViewHistoryProgram;
        private FrmPreviewDatabaseTHTrueMilk _FormPreviewDatabase;
        private FrmCheckedResultTHTrueMilk _FormCheckedResult;

        private PrinterSettingsModel _PrinterSettingsModel;
        private string _ExportNamePrefix = "";

        //ReleaseCapture extern
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        //SendMessage extern
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam); bool isFullHD = false;
        bool isDelProcessPnlMargin = false;
        public bool IsFullHD
        {
            get => isFullHD;
            set
            {
                isFullHD = value;
                var t = Size.Width;

                //isFullHD = (Size.Width < 850 || Size.Height < 850) ? false : true;
                ////lblTotalCheckedValue.Text = "1000000";
                ////lblPrintedCodeValue.Text = "1000000";
                //if (!IsFullHD) //Hide control for small Resolution screen
                //{
                //    this.tableLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                //    this.tableLayoutPanel.RowStyles[0].Height = 30F; // Make first row 20% height
                //    this.tableLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                //    this.tableLayoutPanel.RowStyles[1].Height = 70F; // Make second row 80% height
                //                                                     //this.tableLayoutPanelCheckedResult.RowCount--;
                //                                                     // tableLayoutPanelCheckedResult.Controls.Remove(pnlCurrentCheck);
                //                                                     //  prBarCheckPassed.Dock = DockStyle.Fill;
                //    btnHistory.Visible = btnAccount.Visible = false;
                //    pnlVerificationProcess.TitleFont = new Font("Microsoft Sans Serif", 9.0f, FontStyle.Bold);

                //    lblFailed.Location = lblPassed.Location = lblTotalChecked.Location = new Point(6, 2);
                //    lblPrintedCodeValue.AutoSize = lblReceivedValue.AutoSize = lblSentDataValue.AutoSize = lblCheckResultFailedValue.AutoSize = lblCheckResultPassedValue.AutoSize = lblTotalCheckedValue.AutoSize = false;
                //    lblPrintedCodeValue.Width = lblReceivedValue.Width = lblSentDataValue.Width = lblCheckResultFailedValue.Width = lblCheckResultPassedValue.Width = lblTotalCheckedValue.Width = 80;
                //    lblCheckResultFailedValue.Font = lblCheckResultPassedValue.Font = lblTotalCheckedValue.Font = new Font("Microsoft Sans Serif", 15.0f, FontStyle.Bold);
                //    lblPrintedCodeValue.TextAlign = lblReceivedValue.TextAlign = lblSentDataValue.TextAlign = lblCheckResultFailedValue.TextAlign = lblCheckResultPassedValue.TextAlign = lblTotalCheckedValue.TextAlign = ContentAlignment.MiddleLeft;
                //    lblCheckResultFailedValue.Location = lblCheckResultPassedValue.Location = lblTotalCheckedValue.Location = new Point(10, 19);

                //    lblPrintedCodeValue.Font = lblReceivedValue.Font = lblSentDataValue.Font = new Font("Microsoft Sans Serif", 15.0f, FontStyle.Regular);
                //    //  lblPrintedCodeValue.Location = lblReceivedValue.Location = lblSentDataValue.Location = new Point(5, 29);
                //    lblSentData.Location = lblReceived.Location = new Point(5, 10); // lblPrintedCode.Location = new Point(5, 10);
                //}
                //else //Show control for full Resolution screen
                //{

                //    //if (this.tableLayoutPanelCheckedResult.RowCount <= 1)
                //    //{
                //    //    //this.tableLayoutPanelCheckedResult.RowCount++;
                //    //    //tableLayoutPanelCheckedResult.Controls.Add(pnlCurrentCheck);
                //    //    this.tableLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                //    //    this.tableLayoutPanel.RowStyles[0].Height = 50F;
                //    //    this.tableLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                //    //    this.tableLayoutPanel.RowStyles[1].Height = 50F;
                //    //    //prBarCheckPassed.Dock = DockStyle.None;
                //    //    //prBarCheckPassed.Anchor = AnchorStyles.None;
                //    //    // btnHistory.Visible = btnAccount.Visible = true; // Đã chuyển sang HideControls ở InitControls
                //    //    pnlVerificationProcess.TitleFont = new Font("Microsoft Sans Serif", 12.0f, FontStyle.Bold);

                //    //    lblFailed.Location = lblPassed.Location = lblTotalChecked.Location = new Point(13, 6);
                //    //    lblPrintedCodeValue.AutoSize = lblReceivedValue.AutoSize = lblSentDataValue.AutoSize = lblCheckResultFailedValue.AutoSize = lblCheckResultPassedValue.AutoSize = lblTotalCheckedValue.AutoSize = true;
                //    //    lblCheckResultFailedValue.Font = lblCheckResultPassedValue.Font = lblTotalCheckedValue.Font = new Font("Microsoft Sans Serif", 20.25f, FontStyle.Bold);
                //    //  lblReceivedValue.Location = lblCheckResultFailedValue.Location = lblCheckResultPassedValue.Location = lblTotalCheckedValue.Location = new Point(13, 28);
                //    //    // lblPrintedCodeValue.Location = lblReceivedValue.Location = lblCheckResultFailedValue.Location = lblCheckResultPassedValue.Location = lblTotalCheckedValue.Location = new Point(13, 28);
                //    //    // lblPrintedCodeValue.Location = lblReceivedValue.Location = lblSentDataValue.Location = new Point(27, 39);
                //    // lblReceivedValue.Location = lblSentDataValue.Location = new Point(27, 39);
                //    //    lblSentData.Location = lblReceived.Location = lblPrintedCode.Location = new Point(21, 10);

                //    //    lblPrintedCodeValue.Font = lblReceivedValue.Font = lblSentDataValue.Font = new Font("Microsoft Sans Serif", 24f, FontStyle.Regular);
                //    //}

                //    pnlVerificationProcess.Padding = new Padding(3, 12, 3, 3);
                //    tableLayoutPanel.RowCount = 2;
                //    tableLayoutPanel.ColumnCount = 1;
                //    var pad = pnlVerificationProcess.Margin;
                //    pad.Right -= isDelProcessPnlMargin ? 11 : 0;
                //    pnlVerificationProcess.Margin = pad;
                //    isDelProcessPnlMargin = false;
                //    tableLayoutPanel.Controls.Add(pnlVerificationProcess, 0, 1);
                //    //tableLayoutPanel.Controls.Add(tableLayoutPanelCheckedResult, 0, 0);

                //    tableLayoutPanel1.RowCount = 2;
                //    tableLayoutPanel1.ColumnCount = 1;

                //    tableLayoutPanel1.RowStyles[0].Height = _SelectedJob.CompareType == CompareType.Database ?
                //         tableLayoutPanel1.Width * 50 / 100 : 0;
                //    tableLayoutPanel1.RowStyles[1].Height = tableLayoutPanel1.Width * 50 / 100;



                //    if (_SelectedJob.CompareType != CompareType.Database)
                //    {
                //        tableLayoutPanel1.Controls.Remove(pnlDatabase);
                //    }
                //    else
                //    {
                //        tableLayoutPanel1.Controls.Add(pnlDatabase, 0, 0);
                //    }
                //    tableLayoutPanel1.Controls.Add(pnlCheckedResult, 0, 1);

                //    bool isAppearResultTable = ProjectLabel.IsTHTrueMilk && !Shared.Settings.IsManufacturingMode;
                //    if (isAppearResultTable)
                //    {
                //        tableLayoutPanel1.RowStyles[1].Height = 0;
                //        tableLayoutPanel1.Controls.Remove(pnlCheckedResult);

                //        tableLayoutPanel1.RowStyles[0].Height = tableLayoutPanel1.Width * 50 / 100;
                //    }
                //}
            }
        }

        PODController podController = Shared.Settings.PrinterList.Where(p => p.RoleOfPrinter == RoleOfStation.ForProduct).FirstOrDefault().PODController;
        public event EventHandler OnReceiveVerifyDataEvent;
        private int _CurrentPage = 0;
        private int _DatabaseImageIndex = -1;
        private int _CheckedResulImageIndex = -1;
        private readonly object _StopLocker = new object();
        private bool _IsStopOK = false;
        private readonly Stopwatch _BigSTW = new Stopwatch();
        private readonly Stopwatch _sendDataSTW = new Stopwatch();
        private string _PrintedResponseValue = "";
        private string _qrLogCurrent = "";
        private int _qrLogStartCount = 0;
        private readonly List<QrDetailItem> _qrLogSegments = new List<QrDetailItem>();
        private volatile int _dialogResultStopExist;

        // For Combine Camera Result
        private DetectModel _textOnlyData = null;
        private DetectModel _imageData = null;
        private CancellationTokenSource _delayCancellationTokenSource;
        private DateTime _lastDateForNsxHsd = DateTime.MinValue;
#if DEBUG
        public void ResetLastDateForNsxHsd()
        {
            _lastDateForNsxHsd = DateTime.MinValue;
            Console.WriteLine("[DEBUG] _lastDateForNsxHsd reset → MinValue");
        }
        public DateTime? DebugNsxHsdTime { get; set; }
        private bool _debugNsxHsdTriggered = false;
        public void ResetDebugNsxHsdTriggered()
        {
            _debugNsxHsdTriggered = false;
        }
#endif
        private int _firstCount = 0;
        private bool _isImage;
        string PrintedVerified = "Printed-Verified";
        string UnprintedVerified = "Unprinted-Verified";
        string PrintedDuplicate = "Printed-Duplicate";
        string PrintedUnverified = "Printed-Unverified";
        string UnprintedUnverified = "Unprinted-Unverified";
        string countMaster = "";
        string countSlave = "";
        private Image _nextImage;
        private bool _isUpdatePending;
        private int _numberPrev = 0;
        static string pathSendLog;
        static string pathRSFPLog;
        private ConcurrentQueue<int> _queueCountFeedback = new ConcurrentQueue<int>();
        private readonly Model.THTrueMilk.SynchronizedQueue<string[]> _QueueBufferBackupRSFPLog = new Model.THTrueMilk.SynchronizedQueue<string[]>();
        int countFormStopSuddenly = 0;
        private bool IsCloseButtonAction = true;
        private readonly object lockObject = new object();
        public int CountFeedback { get; set; }
        private int _countFb;
        int CountDataRev;
        AutoTriggerCameraDataman _autoTrigger;
        private RLinkLogService _rlinkLogService;
        private System.Threading.Timer _TimerRunLog;

        #endregion

        public frmMainTHTrueMilk()
        {
            InitializeComponent();
        }
        private bool isDragging = false;
        private Point dragStartPoint;
        public frmMainTHTrueMilk(frmJobTHTrueMilk parentForm)
        {
            InitializeComponent();
         
            roundPanel1.Height = 239;
            CreateSummaryTable(5);
            FormClosing += FrmMain_FormClosing;
            FormClosed += FrmMain_FormClosed;
            _ParentForm = parentForm;
            // Đăng ký sự kiện khi QR code thay đổi trong lúc job đang chạy
            if (_ParentForm != null)
            {
                _ParentForm.Mode1QrCodeChanged += ParentForm_OnQrCodeChanged;
                ProjectLogger.WriteInfo("[Mode1] Subscribed Handler A (constructor)");
            }
            //Shared.CurrentJob = parentForm._JobModel;
            //MessageBox.Show("Shared.CurrentJob" + Shared.CurrentJob.FileName);
            WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            Shared.OnNumberEventISCount += Shared_OnNumberEventISCountAsync;
            //StartWebSocketServer("http://localhost:3333/ws/");
            //MessageBox.Show(Shared.LoggedInUser.Role.ToString());

            CreateScoreLabels();
#if DEBUG
            InitializeDebugControls();
#endif
            //pnlCurrentCheck.MaximumSize = new System.Drawing.Size(600, 231);
            //pnlVerificationProcess.MinimumSize = new System.Drawing.Size(231,697);
        }

        private void CreateScoreLabels()
        {
        }

        private void UpdateConsecutiveErrorDisplay()
        {
            int count = _consecutiveErrorCount;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (lblCountError == null) return;
                    int max = 5;
                    if (txtMaxConsecutiveDefects != null)
                    {
                        string txt = txtMaxConsecutiveDefects.Text;
                        int sp = txt.IndexOf(' ');
                        if (sp > 0) int.TryParse(txt.Substring(0, sp), out max);
                    }
                    lblCountError.Text = $"Số lỗi liên tục: {count} / {max}";
                }));
            }
            catch { }
        }

        private void UiBatchTimer_Tick(object sender, EventArgs e)
        {
            if (!_uiUpdatePending) return;
            _uiUpdatePending = false;

            // Freeze layout — không repaint trong lúc update
            _tblSummary.SuspendLayout();
            try
            {
                // Stats labels — chỉ update nếu thay đổi
                if (_TotalChecked != _prevTotalChecked)
                {
                    lblTotalCheckedValue.Text = _TotalChecked.ToString("N0");
                    _prevTotalChecked = _TotalChecked;
                }
                if (_NumberOfCheckPassed != _prevCheckPassed)
                {
                    lblCheckResultPassedValue.Text = _NumberOfCheckPassed.ToString("N0");
                    _prevCheckPassed = _NumberOfCheckPassed;
                }
                int sessionPrinted = _sessionPrintedCount;
                if (sessionPrinted != _prevPrintedCount)
                {
                    lblPrintedCodeValue.Text = sessionPrinted.ToString("N0");
                    _prevPrintedCount = sessionPrinted;
                }
                if (TotalRlinkPrinted != _prevTotalPrintedCount)
                {
                    lblLastPrintedPage.Text = TotalRlinkPrinted.ToString("N0");
                    _prevTotalPrintedCount = TotalRlinkPrinted;
                }
                if (_NumberOfCheckFailed != _prevCheckFailed)
                {
                    lblCheckResultFailedValue.Text = _NumberOfCheckFailed.ToString("N0");
                    _prevCheckFailed = _NumberOfCheckFailed;
                }
                if (_ReceivedCode != _prevReceived)
                {
                    lblReceivedValue.Text = _ReceivedCode.ToString("N0");
                    _prevReceived = _ReceivedCode;
                }

                // Error counts — chỉ update nếu thay đổi
                if (_errorCountA != _prevErrorA && lblTotalScoreA != null)
                {
                    lblTotalScoreA.Text = _errorCountA.ToString("N0");
                    _prevErrorA = _errorCountA;
                }
                if (_errorCountB != _prevErrorB && lblTotalScoreB != null)
                {
                    lblTotalScoreB.Text = _errorCountB.ToString("N0");
                    _prevErrorB = _errorCountB;
                }
                if (_errorCountF != _prevErrorF && lblTotalScoreF != null)
                {
                    lblTotalScoreF.Text = _errorCountF.ToString("N0");
                    _prevErrorF = _errorCountF;
                }
                if (_phanLoaiCount != _prevPhanLoai && label20 != null)
                {
                    label20.Text = $"R-Link phân loại lỗi: {_phanLoaiCount:N0}";
                    _prevPhanLoai = _phanLoaiCount;
                }

                // PhanLoai remaining — chỉ update nếu thay đổi
                int remaining = _errorCountF - _phanLoaiCount;
                if (remaining < 0) remaining = 0;
                if (remaining != _prevRemaining && label22 != null)
                {
                    label22.Text = $"Camera phân loại lỗi: {remaining:N0}";
                    _prevRemaining = remaining;
                }

                // Error ratios — chỉ update nếu thay đổi
                double total = _TotalChecked > 0 ? _TotalChecked : 1;
                double ratioA = _errorCountA * 100.0 / total;
                double ratioB = _errorCountB * 100.0 / total;
                double ratioF = _errorCountF * 100.0 / total;
                if (Math.Abs(ratioA - _prevRatioA) > 0.01 && lblRaitoA != null)
                {
                    lblRaitoA.Text = $"{ratioA:F2} %";
                    _prevRatioA = ratioA;
                }
                if (Math.Abs(ratioB - _prevRatioB) > 0.01 && lblRaitoB != null)
                {
                    lblRaitoB.Text = $"{ratioB:F2} %";
                    _prevRatioB = ratioB;
                }
                if (Math.Abs(ratioF - _prevRatioF) > 0.01 && lblRaitoF != null)
                {
                    lblRaitoF.Text = $"{ratioF:F2} %";
                    _prevRatioF = ratioF;
                }

                // Summary table
                UpdateSummaryTable();

                // DataGridView — invalidate
                dgvCheckedResult.Invalidate();

                // Progress bar
                int progress = 0;
                if (_SelectedJob != null)
                {
                    if (_SelectedJob.CompareType == CompareType.Database)
                        progress = _TotalCode > 0 ? _NumberOfCheckPassed * 100 / (_TotalCode - _NumberOfDuplicate) : 0;
                    else
                        progress = _TotalChecked > 0 ? _NumberOfCheckPassed * 100 / _TotalChecked : 0;
                }
                if (progress < 100)
                {
                    prBarCheckPassed.Text = $"{progress:N0}%";
                    prBarCheckPassed.Value = progress;
                }
                else
                {
                    prBarCheckPassed.Value = 100;
                    prBarCheckPassed.Text = "100%";
                }
                prBarCheckPassed.Invalidate();
            }
            finally
            {
                _tblSummary.ResumeLayout(true);
            }
        }

        private void UpdateErrorRatios()
        {
            //double total = _TotalCode > 0 ? _TotalCode : 1;
            double total = TotalChecked > 0 ? TotalChecked : 1;
            try
            {
                Invoke(new Action(() =>
                {
                    if (lblRaitoA != null) lblRaitoA.Text = $"{_errorCountA * 100.0 / total:F2} %";
                    if (lblRaitoB != null) lblRaitoB.Text = $"{_errorCountB * 100.0 / total:F2} %";
                    if (lblRaitoF != null) lblRaitoF.Text = $"{_errorCountF * 100.0 / total:F2} %";
                }));
            }
            catch { }
        }

        public void ShowQrLowAlert(int available, int thresholdPercent, int remainPercent, int total)
        {
            Action show = () =>
            {
                if (_isStartRebuild)
                {
                    _deferredQrLowAlert =
                        $"Kho QR còn {remainPercent}% ({available} mã) — ngưỡng {thresholdPercent}%.\nVui lòng cấp thêm QR!";
                    return;
                }
                CuzAlert.Show(
                    $"Kho QR còn {remainPercent}% ({available} mã) — ngưỡng {thresholdPercent}%.\nVui lòng cấp thêm QR!",
                    Alert.enmType.Warning,
                    new Size(480, 100),
                    Location, Size,
                    true);
            };

            if (InvokeRequired)
                BeginInvoke(show);
            else
                show();
        }

        /// <summary>Batch hiện tại (từ camera NSX hoặc THJobBatchNo).</summary>
        public string CurrentBatch
        {
            get
            {
                if (!string.IsNullOrEmpty(_lastCameraNsxWithTime))
                {
                    string dateOnly = _lastCameraNsxWithTime.Split(' ')[0];
                    return NormalizeDateToDdMmYy(dateOnly);
                }
                return _SelectedJob?.THJobBatchNo ?? "";
            }
        }

        private void UpdateBatchFromCamera()
        {
            if (string.IsNullOrEmpty(_lastCameraNsxWithTime) || _SelectedJob == null) return;
            string dateOnly = _lastCameraNsxWithTime.Split(' ')[0];
            string normalized = NormalizeDateToDdMmYy(dateOnly);
            if (string.IsNullOrWhiteSpace(normalized)) return;
            if (_SelectedJob.THJobBatchNo != normalized)
                _SelectedJob.THJobBatchNo = normalized;
            if (txtBatchNumber.Text != normalized)
                txtBatchNumber.Text = normalized;
        }

        private async void ParentForm_OnQrCodeChanged(object sender, (string qrCode, DateTime batchDate) e)
        {
            ProjectLogger.WriteInfo($"[QrChanged] Handler A fires — QR: {e.qrCode}, OperStatus={Shared.OperStatus}");

            if (Shared.OperStatus != OperationStatus.Running &&
                Shared.OperStatus != OperationStatus.Processing)
                return;

            RecordQrSwitchForLog(e.qrCode);
        }

        private void RecordQrSwitchForLog(string newQrCode)
        {
            if (string.IsNullOrEmpty(_qrLogCurrent))
            {
                _qrLogCurrent = newQrCode;
                _qrLogStartCount = _NumberPrinted;
                return;
            }

            if (_qrLogCurrent == newQrCode) return;

            int totalOld = _NumberPrinted - _qrLogStartCount;
            if (totalOld > 0)
                _qrLogSegments.Add(new QrDetailItem { qr = _qrLogCurrent, count = totalOld });
            _qrLogCurrent = newQrCode;
            _qrLogStartCount = _NumberPrinted;
        }
        #region SendData to WebSocket Server
        //HttpListener listener;
        //ConcurrentBag<WebSocket> clients = new ConcurrentBag<WebSocket>();
        //CancellationTokenSource cts = new CancellationTokenSource();
        //private async void StartWebSocketServer(string prefix)
        //{
        //    listener = new HttpListener();
        //    listener.Prefixes.Add(prefix);
        //    listener.Start();

        //    _ = Task.Run(async () =>
        //    {
        //        while (!cts.Token.IsCancellationRequested)
        //        {
        //            var context = await listener.GetContextAsync();
        //            if (context.Request.IsWebSocketRequest)
        //            {
        //                var wsContext = await context.AcceptWebSocketAsync(null);
        //                var socket = wsContext.WebSocket;
        //                clients.Add(socket);
        //                _ = HandleClient(socket);
        //            }
        //        }
        //    });

        //    _ = Task.Run(async () =>
        //    {
        //        while (!cts.Token.IsCancellationRequested)
        //        {
        //            var img = FormCapture.Capture(this);
        //            foreach (var ws in clients)
        //            {
        //                if (ws.State == WebSocketState.Open)
        //                {
        //                    try
        //                    {
        //                        await ws.SendAsync(new ArraySegment<byte>(img), WebSocketMessageType.Binary, true, cts.Token);
        //                    }
        //                    catch { }
        //                }
        //            }
        //            await Task.Delay(100); // 10 FPS
        //        }
        //    });
        //}
        //private async Task HandleClient(WebSocket ws)
        //{
        //    var buffer = new byte[1024];
        //    while (ws.State == WebSocketState.Open)
        //    {
        //        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
        //    }
        //}
        #endregion


        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 2;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCLBUTTONDOWN && (int)m.WParam == HTCAPTION)
            {
                Console.WriteLine("Title bar clicked"); // Debug
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal; // Restore to allow dragging
                }
            }
            else if (m.Msg == 0x0200) // WM_MOUSEMOVE
            {
                Point mousePos = Control.MousePosition;
                if (this.WindowState == FormWindowState.Normal)
                {
                    // Check for top-half snapping
                    if (mousePos.Y <= 0) // Top edge of the screen
                    {
                        Rectangle screenBounds = Screen.PrimaryScreen.WorkingArea;
                        this.Location = new Point(0, 0);
                        this.Size = new Size(screenBounds.Width, screenBounds.Height / 2);
                        return; // Stop default dragging after snapping
                    }
                }
            }

            base.WndProc(ref m); // Pass the message to the default handler for normal dragging
        }
        private void Shared_OnNumberEventISCountAsync(object sender, EventArgs e)
        {

            var countEventISCamera = (CountEventISCamera)sender;
            // await Task.Delay(100); // Introduce a 100ms delay

            if (countEventISCamera.Index.Equals(0))
            {
                countMaster = countEventISCamera.Count.ToString();
            }
            else if (countEventISCamera.Index.Equals(1))
            {
                countSlave = countEventISCamera.Count.ToString();
            }

        }
        private void InitRLinkLogService()
        {
            try
            {
                _rlinkLogService = new RLinkLogService();
                Console.WriteLine($"[InitRLinkLogService] ✔ _rlinkLogService khởi tạo OK");

                // Singleton — update snapshot factory sang frmMain
                var monitor = RLinkMonitorService.Create(BuildMonitorSnapshot, "frmMain");
                monitor.OnConnectionChanged = (connected) =>
                {
                    try
                    {
                        if (IsHandleCreated)
                            BeginInvoke(new Action(() =>
                            {
                                lblConnectServer.Image = connected
                                    ? Properties.Resources.icons8_cloud_database_25
                                    : Properties.Resources.icons8_cloud_database_25__1_;
                            }));
                    }
                    catch { }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InitRLinkLogService] ✘ LỖI: {ex.GetType().Name} – {ex.Message}");
            }
        }
        /// <summary>Tạo snapshot trạng thái hiện tại để gửi lên R-Link Master.</summary>
        private MonitorPayload BuildMonitorSnapshot()
        {
            try
            {
                var settings = Shared.Settings;
                bool printerOk = settings?.PrinterList?.Count > 0
                    && settings.PrinterList[0].IsConnected;
                bool cameraOk = settings?.CameraList?.Count > 0
                    && settings.CameraList[0].IsConnected;
                bool plcOk = Shared.IsSensorControllerConnected;

                string lineId = Shared.Settings?.LineId ?? "";
                string qrCode = GetCurrentQrCode();
                var qrData = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.GetQrCodeData(qrCode);
                string batch = !string.IsNullOrWhiteSpace(qrData.batch)
                    ? qrData.batch
                    : BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(
                        NormalizeDateToDdMmYy(_lastCameraNsxWithTime ?? ""), lineId);
                string jobName = _SelectedJob?.FileName ?? "";

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

                    CurrentJobName = jobName,
                    CurrentBatch = batch,
                    OperatorUser = Shared.LoggedInUser?.OperatorUserId ?? Shared.LoggedInUser?.UserId ?? "ACC001",
                    TotalQrAllocated = GetTotalQrBankCount(),
                    TotalQrUsed = GetTotalQrUsedFromPg(),
                    TotalQrFailed = _NumberOfCheckFailed,
                    TotalProduced = _NumberOfCheckPassed,

                    ImageErrorFolder = GetErrorImageFolder(),
                    TotalImageErrorJob = CountFilesInFolder(GetErrorJobImageFolder()),
                    TotalImageError = CountFilesInFolder(GetErrorImageFolder()),
                    LastErrorImageBase64 = "",

                    ProductA = _errorCountA,
                    ProductB = _errorCountB,
                    ProductF = _errorCountF,

                    Stats = new MonitorStats
                    {
                        TotalCode = _TotalCode,
                        TotalChecked = _TotalChecked,
                        CheckPassed = _NumberOfCheckPassed,
                        CheckFailed = _NumberOfCheckFailed,
                        NumberPrinted = TotalRlinkPrinted
                    },

                    Stats2 = new MonitorStats2
                    {
                        TotalChecked = _TotalChecked,
                        QrCheckPassed = _NumberOfCheckPassed,
                        QrCheckFailed = _NumberOfCheckFailed,
                        DateCheckPassed = _DateCheckPassed,
                        DateCheckFailed = _DateCheckFailed
                    },

                    Timestamp = FormatRunTime(AccumulatedRunTime + (RunStopwatch.IsRunning ? RunStopwatch.Elapsed : TimeSpan.Zero))
                };
            }
            catch
            {
                return null;
            }
        }
        private string GetErrorImageFolder()
        {
            try
            {
                string subFolder = !string.IsNullOrWhiteSpace(Shared.Settings?.THErrorImageFolder)
                    ? Shared.Settings.THErrorImageFolder
                    : "Default";
                return Path.Combine(ErrorImageBasePath, subFolder);
            }
            catch { return ErrorImageBasePath; }
        }

        private string GetErrorJobImageFolder()
        {
            try
            {
                string subFolder = !string.IsNullOrWhiteSpace(Shared.Settings?.THErrorImageFolder)
                    ? Shared.Settings.THErrorImageFolder
                    : "Default";
                string job = _SelectedJob?.FileName ?? "";
                if (!string.IsNullOrEmpty(job))
                    return Path.Combine(ErrorImageBasePath, subFolder, job);
                return Path.Combine(ErrorImageBasePath, subFolder);
            }
            catch { return ErrorImageBasePath; }
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
                        continue; // không có gateway = mạng ảo (VirtualBox, Hyper-V...)
                    foreach (var ip in props.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            return ip.Address.ToString();
                    }
                }
                // fallback: DNS approach
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
        private static int _cachedQrBankCount = -1;
        private static DateTime _lastQrBankCountTime = DateTime.MinValue;
        private static int GetTotalQrBankCount()
        {
            try
            {
                if (_cachedQrBankCount >= 0 && (DateTime.Now - _lastQrBankCountTime).TotalSeconds < 60)
                    return _cachedQrBankCount;

                string connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                    "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);
                if (string.IsNullOrWhiteSpace(connStr))
                {
                    using (var conn = new System.Data.SQLite.SQLiteConnection(
                        Services.THTrueMilk.RLinkMaster.RLinkLogService.ConnStr))
                    {
                        conn.Open();
                        using (var cmd = new System.Data.SQLite.SQLiteCommand($"SELECT COUNT(*) FROM {Code}", conn))
                        {
                            _cachedQrBankCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                else
                {
                    string table = Code;
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand($"SELECT COUNT(*) FROM \"{table}\"", conn))
                        {
                            _cachedQrBankCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                _lastQrBankCountTime = DateTime.Now;
                return _cachedQrBankCount;
            }
            catch { return _cachedQrBankCount >= 0 ? _cachedQrBankCount : 0; }
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

                string table = Code;

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
        private string GetLatestErrorImageBase64()
        {
            try
            {
                string jobName = _SelectedJob?.FileName ?? "";
                if (string.IsNullOrWhiteSpace(jobName)) return null;

                string connStr = BarcodeVerificationSystem.View.frmDatabase.GetConnectionString(
                    "postgresql", Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                    Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                    Shared.Settings.THLocalDbDatabase);
                if (string.IsNullOrWhiteSpace(connStr)) return null;

                using (var conn = new Npgsql.NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(
                                                $"SELECT {ImagePath} FROM {LogCameraError} WHERE {JobName}=@jn AND {ImagePath} IS NOT NULL AND {ImagePath}!='' ORDER BY {Id} DESC LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue(JobName, jobName);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            return result.ToString();
                    }
                }
            }
            catch { }
            return null;
        }
        private void StartRunLogTimer()
        {
            StopRunLogTimer();
            int intervalMs = Math.Max(1, Shared.Settings?.THLogInterval ?? 5) * 60 * 1000;
            _TimerRunLog = new System.Threading.Timer(_ => FireRLinkLog("running"), null, intervalMs, intervalMs);
        }

        private void StopRunLogTimer()
        {
            _TimerRunLog?.Dispose();
            _TimerRunLog = null;
        }

 
        private static readonly string _RLinkLogsDir =
            Path.Combine(CommVariables.PathProgramDataApp, "RLinkLogs");
        private static readonly object _rlinkLogFileLock = new object();
        private static readonly object _rebuildLock = new object();
        private void FireRLinkLog(string status)
        {
            try
            {
                if (status == "running" &&
                    Shared.OperStatus != OperationStatus.Running &&
                    Shared.OperStatus != OperationStatus.Processing)
                    return;

                string jobName = _SelectedJob?.FileName ?? "";
                bool hasCamera = (_NumberOfCheckPassed + _NumberOfCheckFailed) > 0;
                bool hasPrinted = _NumberPrinted > 0;
                string lineId = Shared.Settings?.LineId ?? "";
                string qrCode = GetCurrentQrCode();
                var qrData = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.GetQrCodeData(qrCode);
                string batch = !string.IsNullOrWhiteSpace(qrData.batch)
                    ? qrData.batch
                    : BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(
                        NormalizeDateToDdMmYy(_lastCameraNsxWithTime?.Split(' ')[0] ?? ""), lineId);
                string rlinkName = Shared.Settings?.RLinkName ?? "";
                string productId = _SelectedJob?.THJobProductId ?? "";
                string productName = _SelectedJob?.THJobProductName ?? "";
                int qty = TotalRlinkPrinted;
                int cameraOk = _NumberOfCheckPassed;
                int cameraFail = _NumberOfCheckFailed;
                int totalCheck = cameraOk + cameraFail;
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var qrDetail = BuildQrDetailForLog();
                string qrDetailStr = qrDetail != null
                    ? string.Join(";", qrDetail.Select(x => $"{x.qr}({x.count})"))
                    : "";
                int qrUsed = _ParentForm?.QrConsumedCount > 0 ? _ParentForm.QrConsumedCount : _NumberPrinted;

                // ── Thông tin tài khoản đang vận hành & trạng thái R-Link ──
                string operatorUser = !string.IsNullOrEmpty(Shared.LoggedInUser?.OperatorUserId)
                    ? Shared.LoggedInUser.OperatorUserId
                    : (!string.IsNullOrEmpty(Shared.LoggedInUser?.UserId)
                        ? Shared.LoggedInUser.UserId
                        : "ACC001");

                // Safeguard: nếu operator_user trùng username → dùng "ACC001" (tránh FK fail trên server)
                if (operatorUser.Equals(UserController.LogedInUsername ?? "",
                    StringComparison.OrdinalIgnoreCase))
                {
                    operatorUser = "ACC001";
                }

                string rlinkStatus = Shared.OperStatus.ToString(); // Running / Stopped / Processing / ...

                // ── 1. Ghi file CSV vào RLinkLogs\<jobName>\ ──
                try
                {
                    // Subfolder theo job để phân biệt log của từng job
                    string safeJobName = string.IsNullOrWhiteSpace(jobName) ? "_NoJob" : jobName;
                    foreach (var ch in Path.GetInvalidFileNameChars())
                        safeJobName = safeJobName.Replace(ch, '_');

                    string jobLogDir = Path.Combine(_RLinkLogsDir, safeJobName);
                    if (!Directory.Exists(jobLogDir))
                        Directory.CreateDirectory(jobLogDir);

                    string dateTag = DateTime.Now.ToString("yyyyMMdd");

                    lock (_rlinkLogFileLock)
                    {
                        string logInPath = Path.Combine(jobLogDir, $"{LogIn}_{dateTag}.csv");
                        string logCamPath = Path.Combine(jobLogDir, $"{LogCamera}_{dateTag}.csv");

                        bool logInExists = File.Exists(logInPath);
                        bool logCamExists = File.Exists(logCamPath);

                        using (var sw = new StreamWriter(logInPath, append: true, new UTF8Encoding(true)))
                        {
                            if (!logInExists)
                                sw.WriteLine($"{Timestamp},{Status},{RlinkStatus},{OperatorUser},{Qty},{QrCode},{QrDetail},{LineId},{RlinkName},{JobName},{Batch},{ProductId},{ProductName}");

                            sw.WriteLine(string.Join(",", new[]
                            {
                                now ?? "", status ?? "", rlinkStatus ?? "", operatorUser ?? "", qty.ToString(), qrCode ?? "", qrDetailStr ?? "",
                                lineId ?? "", rlinkName ?? "", jobName ?? "", batch ?? "", productId ?? "", productName ?? ""
                            }.Select(s => Csv.Escape(s ?? ""))));
                        }

                        using (var sw = new StreamWriter(logCamPath, append: true, new UTF8Encoding(true)))
                        {
                            if (!logCamExists)
                                sw.WriteLine($"{Timestamp},{RlinkStatus},{OperatorUser},camera_ok,camera_fail,{TotalCheck},{QrCode},{QrDetail},{LineId},{RlinkName},{JobName},{Batch},{ProductId},{ProductName}");

                            sw.WriteLine(string.Join(",", new[]
                            {
                                now ?? "", rlinkStatus ?? "", operatorUser ?? "",
                                cameraOk.ToString(), cameraFail.ToString(), totalCheck.ToString(), qrCode ?? "", qrDetailStr ?? "",
                                lineId ?? "", rlinkName ?? "", jobName ?? "", batch ?? "", productId ?? "", productName ?? ""
                            }.Select(s => Csv.Escape(s ?? ""))));
                        }

                        Console.WriteLine($"[FireRLinkLog] ✔ CSV → {logInPath}");
                        Console.WriteLine($"[FireRLinkLog] ✔ CSV → {logCamPath}");
                    }
                }
                catch (Exception fileEx)
                {
                    Console.WriteLine($"[FireRLinkLog] ✘ Ghi file lỗi: {fileEx.GetType().Name} - {fileEx.Message} | dir={_RLinkLogsDir}");
                }
                // ── 2. Ghi DB (SQLite + PostgreSQL) — fire-and-forget ────────────
                if (_rlinkLogService == null) return;

                Task.Run(async () =>
                {
                    try
                    {
                        // NSX, HSD, lastPrintTime from last database rows
                        string nsx = "", hsd = "", lastPrintTime = "", cameraNsx = "";
                        if (hasCamera) cameraNsx = _lastCameraNsxWithTime;
                        if (hasPrinted && _PrintedCodeObtainFromFile?.Count > 0)
                        {
                            var lastRow = _PrintedCodeObtainFromFile[_PrintedCodeObtainFromFile.Count - 1];
                            int len = lastRow.Length;
                            if (len >= 4) nsx = lastRow[len - 4] ?? "";
                            if (len >= 3) hsd = lastRow[len - 3] ?? "";
                            for (int i = _PrintedCodeObtainFromFile.Count - 1; i >= 0; i--)
                            {
                                var row = _PrintedCodeObtainFromFile[i];
                                string pt = row.Length >= 1 ? row[row.Length - 1] : "";
                                if (!string.IsNullOrEmpty(pt)) { lastPrintTime = pt; break; }
                            }
                        }

                        await _rlinkLogService.SaveLogInAsync(
                            status, rlinkStatus, operatorUser, qty,
                            lineId, rlinkName, jobName, batch, productId, productName, qrCode, qrDetail, qrUsed,
                            nsx, hsd, lastPrintTime, cameraNsx);

                        // camera_date: dùng _lastCameraNsxWithTime nếu có camera data
                        string camDateOnly = _lastCameraNsxWithTime;
                        if (!string.IsNullOrEmpty(camDateOnly))
                        {
                            var parts = camDateOnly.Split(' ');
                            if (parts.Length >= 3) camDateOnly = $"{parts[0]} {parts[1]} {parts[2]}";
                        }
                        else
                        {
                            var vl = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                            if (vl != null)
                                camDateOnly = vl.Nsx;
                            else if (_PrintedCodeObtainFromFile.Count > 0)
                            {
                                var lastRow = _PrintedCodeObtainFromFile[_PrintedCodeObtainFromFile.Count - 1];
                                if (lastRow.Length > 4)
                                    camDateOnly = lastRow[lastRow.Length - 2];
                            }
                        }

                        string camExpiry = _lastCameraHsd ?? "";
                        if (string.IsNullOrEmpty(camExpiry))
                        {
                            var vl = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                            if (vl != null)
                                camExpiry = vl.Hsd;
                            else if (_PrintedCodeObtainFromFile.Count > 0)
                            {
                                var lastRow = _PrintedCodeObtainFromFile[_PrintedCodeObtainFromFile.Count - 1];
                                if (lastRow.Length > 4)
                                    camExpiry = lastRow[lastRow.Length - 3];
                            }
                        }

                        string camLastPacket = _lastCameraPacketReceived != DateTime.MinValue
                            ? _lastCameraPacketReceived.ToString("dd MM yy HH:mm:ss") : "";

                        string camLastProdManu = _lastCameraNsxWithTime ?? "";

                        string camFrameInfo = _lastCameraFrameInfo ?? "";

                        await _rlinkLogService.SaveLogCameraAsync(
                            status, rlinkStatus, operatorUser, cameraOk, cameraFail,
                            lineId, rlinkName, jobName, batch, productId, productName, qrCode, qrDetail,
                            frameInfo: camFrameInfo,
                            cameraManufacturedDate: camDateOnly,
                            cameraExpiryDate: camExpiry,
                            cameraLastPacketReceivedAt: camLastPacket,
                            cameraLastProductManufacturedDate: camLastProdManu);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[FireRLinkLog] ✘ DB error: {ex.Message}");
                    }
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FireRLinkLog({status})] outer error: {ex.Message}");
            }
        }
        /// <summary>QR hiện tại — ưu tiên từ VirtualList, fallback từ frmJob.</summary>
        private string GetCurrentQrCode()
        {
            if (_PrintedCodeObtainFromFile is PrintedCodeVirtualList vl)
                return vl.QrCode ?? "";
            return _ParentForm?.CurrentBatchQrCode ?? "";
        }

        private List<QrDetailItem> BuildQrDetailForLog()
        {
            if (string.IsNullOrEmpty(_qrLogCurrent))
                _qrLogCurrent = _ParentForm?.CurrentBatchQrCode ?? "";

            int currentCount = _NumberPrinted - _qrLogStartCount;
            if (currentCount > 0 && !string.IsNullOrEmpty(_qrLogCurrent))
                _qrLogSegments.Add(new QrDetailItem { qr = _qrLogCurrent, count = currentCount });

            var result = new List<QrDetailItem>(_qrLogSegments);

            _qrLogSegments.Clear();
            _qrLogCurrent = _ParentForm?.CurrentBatchQrCode ?? "";
            _qrLogStartCount = _NumberPrinted;
            return result;
        }

        /// <summary>Đọc file CSV kết quả check, trả về thống kê theo từng QR code.</summary>
        private Dictionary<string, QrStatsDetail> BuildQrStatsFromCsv(string csvPath)
        {
            var qrStatsMap = new Dictionary<string, QrStatsDetail>();
            if (!File.Exists(csvPath)) return qrStatsMap;

            var rexCsvSplitter = new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");
            try
            {
                using (var reader = new StreamReader(csvPath, Encoding.UTF8, true))
                {
                    bool isFirstLine = true;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (isFirstLine) { isFirstLine = false; continue; }

                        string[] row = rexCsvSplitter.Split(line).Select(x => Csv.Unescape(x)).ToArray();
                        if (row.Length <= Index_ErrorType) continue;

                        string qr = row[Index_ResultData] ?? "";
                        string result = row[Index_Result] ?? "";
                        string errorType = row[Index_ErrorType] ?? "";

                        if (string.IsNullOrEmpty(qr)) continue;

                        if (!qrStatsMap.ContainsKey(qr))
                            qrStatsMap[qr] = new QrStatsDetail();

                        var stats = qrStatsMap[qr];
                        stats.TotalCheck++;
                        if (result == "Valid") stats.TotalQRCheckPassed++;
                        else stats.TotalQRCheckFailed++;
                        if (errorType == "A") stats.TotalA++;
                        else if (errorType == "B") stats.TotalB++;
                        else if (errorType == "F") stats.TotalF++;
                        stats.TotalDateCheckPassed = stats.TotalA + stats.TotalB;
                        stats.TotalDateCheckFailed = stats.TotalF;
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[BuildQrStatsFromCsv] " + ex.Message, ex);
            }
            return qrStatsMap;
        }

        /// <summary>Tạo danh sách QrStatsItem để gửi lên server khi hoàn thành job.</summary>
        private List<QrStatsItem> BuildQrStatsArray()
        {
            var result = new List<QrStatsItem>();

            string jobName = _SelectedJob?.FileName ?? "";
            var qrList = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.GetQrCodesUsedByJobPg(jobName);
            if (qrList.Count == 0)
                qrList = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.GetQrCodesUsedByJob(jobName);
            if (qrList.Count == 0) return result;

            // ── Ưu tiên đọc từ file CSV (đầy đủ data, không bị trim) ──
            string csvPath = CommVariables.PathCheckedResult + _SelectedJob?.CheckedResultPath;
            var qrStatsMap = BuildQrStatsFromCsv(csvPath);

            if (qrStatsMap.Count > 0)
            {
                foreach (var qrCode in qrList)
                {
                    if (string.IsNullOrEmpty(qrCode)) continue;
                    qrStatsMap.TryGetValue(qrCode, out var stats);
                    result.Add(new QrStatsItem { QrCode = qrCode, Stats = stats ?? new QrStatsDetail() });
                }
                return result;
            }

            // ── Fallback: dùng _CheckedResultCodeList (in-memory, bị trim max 50K) ──
            var qrRowsMap = qrList.ToDictionary(q => q, _ => new List<string[]>());
            string lastQr = null;
            foreach (var row in _CheckedResultCodeList)
            {
                string qr = row.Length > Index_ResultData ? (row[Index_ResultData] ?? "") : "";
                if (!string.IsNullOrEmpty(qr) && qrRowsMap.ContainsKey(qr))
                    lastQr = qr;
                if (!string.IsNullOrEmpty(lastQr) && qrRowsMap.ContainsKey(lastQr))
                    qrRowsMap[lastQr].Add(row);
            }
            foreach (var qrCode in qrList)
            {
                if (string.IsNullOrEmpty(qrCode)) continue;
                var qrRows = qrRowsMap[qrCode];
                int total = qrRows.Count;
                int passed = qrRows.Count(r => r.Length > Index_Result && r[Index_Result] == "Valid");
                int a = qrRows.Count(r => r.Length > Index_ErrorType && r[Index_ErrorType] == "A");
                int b = qrRows.Count(r => r.Length > Index_ErrorType && r[Index_ErrorType] == "B");
                int f = qrRows.Count(r => r.Length > Index_ErrorType && r[Index_ErrorType] == "F");
                result.Add(new QrStatsItem
                {
                    QrCode = qrCode,
                    Stats = new QrStatsDetail
                    {
                        TotalCheck = total,
                        TotalQRCheckPassed = passed,
                        TotalQRCheckFailed = total - passed,
                        TotalDateCheckPassed = a + b,
                        TotalDateCheckFailed = f,
                        TotalA = a, TotalB = b, TotalF = f
                    }
                });
            }
            if (result.Count > 0) return result;

            // ── Fallback cuối cùng: dùng counter toàn cục ──
            string currentQr = GetCurrentQrCode();
            if (!string.IsNullOrEmpty(currentQr))
                result.Add(new QrStatsItem { QrCode = currentQr, Stats = new QrStatsDetail
                {
                    TotalCheck = _TotalChecked,
                    TotalQRCheckPassed = _NumberOfCheckPassed,
                    TotalQRCheckFailed = _NumberOfCheckFailed,
                    TotalDateCheckPassed = _DateCheckPassed,
                    TotalDateCheckFailed = _DateCheckFailed,
                    TotalA = _errorCountA,
                    TotalB = _errorCountB,
                    TotalF = _errorCountF
                } });

            return result;
        }

        /// <summary>Tính trước đường dẫn ảnh lỗi (cùng quy tắc với NewExportImageToFile).</summary>
        private string BuildErrorImagePath(int index)
        {
            try
            {
                string subFolder = !string.IsNullOrWhiteSpace(_SelectedJob?.THJobErrorImageFolder)
                    ? _SelectedJob.THJobErrorImageFolder
                    : (!string.IsNullOrWhiteSpace(Shared.Settings?.THErrorImageFolder)
                        ? Shared.Settings.THErrorImageFolder
                        : "Default");

                string basePath = ErrorImageBasePath;
                string root = Path.Combine(basePath, subFolder);

                string jobName = _SelectedJob?.FileName ?? "Unknown";
                string jobFolder = Path.Combine(root, jobName);
                string fileName = string.Format("{0}_Job_{1}_Image_{2:D7}.bmp",
                                       _ExportNamePrefix, jobName, index);
                return Path.Combine(jobFolder, fileName);
            }
            catch { return ""; }
        }

        /// <summary>Đường dẫn ảnh lỗi đơn giản với imageId từ camera.</summary>
        private string BuildErrorImagePath(string imageId, string jobName = null)
        {
            try
            {
                string subFolder = !string.IsNullOrWhiteSpace(_SelectedJob?.THJobErrorImageFolder)
                    ? _SelectedJob.THJobErrorImageFolder
                    : (!string.IsNullOrWhiteSpace(Shared.Settings?.THErrorImageFolder)
                        ? Shared.Settings.THErrorImageFolder
                        : "Default");

                string basePath = ErrorImageBasePath;
                string root = Path.Combine(basePath, subFolder);
                string useJobName = jobName ?? _SelectedJob?.FileName ?? "Unknown";
                string jobFolder = Path.Combine(root, useJobName);
                return Path.Combine(jobFolder, $"image_{imageId}.jpg");
            }
            catch { return ""; }
        }

        // ── Ghi log từng lỗi camera vào rlink_log_camera_error_YYYYMMDD.csv + DB ──
        private void FireRLinkCameraError(string qrCode, string resultType, int index = 0,
            string errorNsx = "", string errorHsd = "", string errorFrameInfo = "")
        {
            try
            {
                if (!Directory.Exists(_RLinkLogsDir))
                    Directory.CreateDirectory(_RLinkLogsDir);

                string lineId = Shared.Settings?.LineId ?? "";
                string rlinkName = Shared.Settings?.RLinkName ?? "";
                string jobName = _SelectedJob?.FileName ?? "";
                var qrData = RLinkLogService.GetQrCodeData(qrCode);
                string batch = !string.IsNullOrWhiteSpace(qrData.batch)
                    ? qrData.batch
                    : BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(
                        !string.IsNullOrEmpty(errorNsx)
                            ? NormalizeDateToDdMmYy(errorNsx)
                            : NormalizeDateToDdMmYy(_lastCameraNsxWithTime?.Split(' ')[0] ?? ""),
                        lineId);
                string productId = _SelectedJob?.THJobProductId ?? "";
                string productName = _SelectedJob?.THJobProductName ?? "";
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string dateTag = DateTime.Now.ToString("yyyyMMdd");
                // Extract imageId from camera frame info for new path format
                string imgId = "";
                if (!string.IsNullOrEmpty(errorFrameInfo))
                {
                    string[] parts = errorFrameInfo.Split(',');
                    imgId = parts.Length > 7 ? parts[7].Trim() : "";
                }
                string imagePath = !string.IsNullOrEmpty(imgId)
                    ? BuildErrorImagePath(imgId)
                    : BuildErrorImagePath(index);

                string operatorUser = !string.IsNullOrEmpty(Shared.LoggedInUser?.OperatorUserId)
                    ? Shared.LoggedInUser.OperatorUserId
                    : (!string.IsNullOrEmpty(Shared.LoggedInUser?.UserId)
                        ? Shared.LoggedInUser.UserId
                        : "ACC001");

                if (operatorUser.Equals(UserController.LogedInUsername ?? "",
                    StringComparison.OrdinalIgnoreCase))
                {
                    operatorUser = "ACC001";
                }

                string rlinkStatus = Shared.OperStatus.ToString();

                // Subfolder theo job
                string safeJobName = string.IsNullOrWhiteSpace(jobName) ? "_NoJob" : jobName;
                foreach (var ch in Path.GetInvalidFileNameChars())
                    safeJobName = safeJobName.Replace(ch, '_');

                string jobLogDir = Path.Combine(_RLinkLogsDir, safeJobName);
                if (!Directory.Exists(jobLogDir))
                    Directory.CreateDirectory(jobLogDir);

                lock (_rlinkLogFileLock)
                {
                    string path = Path.Combine(jobLogDir, $"{LogCameraError}_{dateTag}.csv");
                    bool exists = File.Exists(path);
                    using (var sw = new StreamWriter(path, append: true, new UTF8Encoding(true)))
                    {
                        if (!exists)
                            sw.WriteLine($"{Timestamp},{RlinkStatus},{OperatorUser},{QrCode},result,{ImagePath},{LineId},{RlinkName},{JobName},{Batch},{ProductId},{ProductName}");

                        sw.WriteLine(string.Join(",", new[]
                        {
                            now ?? "", rlinkStatus ?? "", operatorUser ?? "", qrCode ?? "", resultType ?? "", imagePath ?? "",
                            lineId ?? "", rlinkName ?? "", jobName ?? "", batch ?? "", productId ?? "", productName ?? ""
                        }.Select(s => Csv.Escape(s ?? ""))));
                    }
                    Console.WriteLine($"[FireRLinkCameraError] ✔ CSV → {path}");
                }

                if (_rlinkLogService != null)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _rlinkLogService.SaveLogCameraErrorAsync(
                                rlinkStatus, operatorUser, qrCode, resultType,
                                lineId, rlinkName, jobName, batch, productId, productName,
                                imagePath, errorNsx, errorHsd, errorFrameInfo);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[FireRLinkCameraError] ✘ DB error: {ex.Message}");
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FireRLinkCameraError] ✘ {ex.Message}");
            }
        }

        private bool _isExitingApp;

        //private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    Shared.IsMainFormRunning = false;
        //    _ = Task.Run(() => ReleaseResource());  // Cleanup nền, không block UI
        //    if (!_isExitingApp && _ParentForm != null && !_ParentForm.IsDisposed)
        //    {
        //        _ParentForm.isShowPopupDisConOneTime = false;
        //        _ParentForm.ShowForm();
        //    }
        //}
        //private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    Shared.IsMainFormRunning = false;
        //    ReleaseResource();
        //    if (!_isExitingApp && _ParentForm != null && !_ParentForm.IsDisposed)
        //    {
        //        _ParentForm.isShowPopupDisConOneTime = false;
        //        _ParentForm.ShowForm();
        //    }
        //}

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Shared.IsMainFormRunning = false;
            ReleaseResource();
            if (!_isExitingApp && _ParentForm != null && !_ParentForm.IsDisposed)
            {
                _ParentForm.isShowPopupDisConOneTime = false;
                _ParentForm.ShowForm();
            }
        }

        //private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    Shared.IsMainFormRunning = false;

        //    // ── CRITICAL: Cleanup đồng bộ TRƯỚC KHI show parent form ──
        //    // Đảm bảo old form không còn nhận event nào từ printer
        //    _PrinterRespontCST?.Cancel();
        //    Shared.OnPrinterDataChange -= Shared_OnPrinterDataChange;
        //    Shared.OnSyncDataParameterChange -= Shared_OnSyncDataParameterChange;
        //    Shared.OnSyncCheckDataParameterChange -= Shared_OnSyncCheckDataParameterChange;
        //    //Shared.OnDatabaseStatusChange -= Shared.OnDatabaseStatusChange;
        //    try { _printedDataProcess?.Stop(); } catch { }
        //    try { _checkedDataProcess?.Stop(); } catch { }

        //    // ── Non-critical: Cleanup nền, không block UI ──
        //    _ = Task.Run(() => ReleaseResource());

        //    if (!_isExitingApp && _ParentForm != null && !_ParentForm.IsDisposed)
        //    {
        //        _ParentForm.isShowPopupDisConOneTime = false;
        //        _ParentForm.ShowForm();
        //    }
        //}
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ── ForceClose: bypass tất cả kiểm tra ──
            if (_forceClose) return;

            // ── Không cho đóng form khi đang chạy hoặc đang load ──
            if (Shared.OperStatus == OperationStatus.Running || Shared.OperStatus == OperationStatus.Processing
                || picCheckedResultLoading.Visible)
            {
                CustomMessageBox.Show("Hệ thống đang chạy hoặc đang tải dữ liệu. Vui lòng dừng trước khi thoát", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
                return;
            }

            if (IsCloseButtonAction == true)
            {
                DialogResult dialogResult = CustomMessageBox.Show(Lang.DoYouWantExitApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    _isExitingApp = true;
                    if (Shared.OperStatus == OperationStatus.Running || Shared.OperStatus == OperationStatus.Processing)
                        StopProcess(false, "", false, true);
                    KillAllProccessThread();
                    // Đóng form cha (frmJob) — là main form của Application.Run → thoát app
                    _ParentForm?.Close();
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
            IsCloseButtonAction = true;
        }

        #region Inits first
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Shared.IsMainFormRunning = true;
            Shared.Settings.PLCVersion = 1;  // THTrueMilk PLC Version 1
            Shared.Settings.NumberOfPort = 1;  // chỉ 1 port
            Shared.Settings.SensorControllerPort2 = 0;  // THTrueMilk chỉ dùng 1 port PLC
            InitControls();
            InitEvents();
            MonitorSensorControllerConnection();
        }
        private async void btnCompleteJob_Click(object sender, EventArgs e)
        {
            bool isZeroPrinted = TotalRlinkPrinted == 0;
            string confirmMsg = isZeroPrinted
                ? "Chưa in sản phẩm nào!\n\nQR đã được đánh dấu là đã dùng.\nBạn có muốn hoàn thành job và GIẢI PHÓNG QR về trạng thái chưa dùng?"
                : "Bạn có chắc chắn muốn hoàn thành công việc không?\n" +
                  $"Số sản phẩm đã in và kiểm đúng: {NumberOfCheckPassed:N0}\n" +
                  $"Tổng đã kiểm: {_TotalChecked:N0}\n" +
                  "Cảnh báo: Sau khi hoàn thành không thể tiếp tục chạy!";

            bool confirmed = CustomMessageBox.IsResultShow(confirmMsg);
            if (!confirmed) return;

            btnCompleteJob.Enabled = false;
            try
            {
                var service = RLinkMasterServiceFactory.Instance;

                // ── Nếu chưa in sản phẩm nào → chỉ giải phóng QR local, không gọi API ──
                if (isZeroPrinted)
                {
                    string jobName = _SelectedJob?.FileName ?? "";
                    try
                    {
                        var qrList = await Task.Run(() =>
                        {
                            var list = Services.THTrueMilk.RLinkMaster.RLinkLogService
                                .GetQrUsedButNotPrintedByJobPg(jobName);
                            if (list.Count == 0)
                                list = Services.THTrueMilk.RLinkMaster.RLinkLogService
                                    .GetQrUsedButNotPrintedByJob(jobName);
                            if (list.Count > 0)
                            {
                                Services.THTrueMilk.RLinkMaster.RLinkLogService
                                    .MarkQrAsUnusedInSQLiteByCode(list);
                                Services.THTrueMilk.RLinkMaster.RLinkLogService
                                    .MarkQrAsUnusedInPgByCode(list);
                            }
                            return list;
                        });

                        Console.WriteLine($"[CompleteJob] ✔ Đã giải phóng {qrList.Count} QR về trạng thái chưa dùng (không gọi API).");
                    }
                    catch (Exception unmarkEx)
                    {
                        Console.WriteLine($"[CompleteJob] ✘ Lỗi giải phóng QR: {unmarkEx.Message}");
                    }

                    // ── Đánh dấu job hoàn thành local ──
                    if (_SelectedJob != null)
                    {
                        _SelectedJob.CompleteJobStatus = CompleteJobStatus.Completed;
                        _SelectedJob.SaveFile();
                    }
                    StopRunLogTimer();
                    FireRLinkLog("completed");

                    CustomMessageBox.Show(
                        "Hoàn thành job thành công!\n" +
                        "Chưa in sản phẩm nào — QR đã được trả về trạng thái chưa dùng.\n" +
                        "Không gửi đồng bộ lên server.",
                        Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    EnableUIComponentWhenLoadData(true);
                    btnCompleteJob.Enabled = false;
                    btnCompleteJob.Text = "Đã xác nhận hoàn thành";
                    btnCompleteJob.BackColor = System.Drawing.Color.FromArgb(0, 199, 82);
                    btnCompleteJob.BackgroundColor = System.Drawing.Color.FromArgb(0, 199, 82);
                    Shared.Settings.LastActiveJobName = "";
                    Shared.SaveSettings();
                    return;
                }

                // ── Đếm QR đã dùng (is_used=TRUE) và còn lại (is_used=FALSE) trong PG ──
                int qrUsedInPg = await CountQrUsedInPgAsync();
                var (qrRemainingPg, pgError) = await CountQrAvailableInPgAsync();

                // Fallback nếu PG không trả về kết quả hợp lệ
                if (qrUsedInPg < 0)
                    qrUsedInPg = _ParentForm?.QrConsumedCount > 0 ? _ParentForm.QrConsumedCount : _NumberPrinted;
                Console.WriteLine($"[CompleteJob] QrUsed={qrUsedInPg} | QrRemaining={qrRemainingPg} | Produced={NumberOfCheckPassed}");

                if (qrRemainingPg < 0)
                {
                    string detail = string.IsNullOrWhiteSpace(pgError) ? "Không nhận được phản hồi từ PostgreSQL." : pgError;
                    CustomMessageBox.Show(
                        $"Không thể đọc số QR còn lại từ cơ sở dữ liệu.\n\nChi tiết lỗi:\n{detail}\n\nVui lòng kiểm tra cấu hình PostgreSQL trong Settings và thử lại.",
                        Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    btnCompleteJob.Enabled = true;
                    return;
                }

                // ── Gửi request — server dùng QrRemainingInDb để so ngưỡng ──────────
                var request = new CompleteJobRequest
                {
                    JobName = _SelectedJob?.FileName ?? string.Empty,
                    RLinkName = Shared.Settings?.RLinkName ?? string.Empty,
                    LineId = Shared.Settings?.LineId ?? string.Empty,
                    QrUsedCount = qrUsedInPg,
                    QrThreshold = _SelectedJob?.THJobQrThreshold ?? 0,
                    QrRemainingInDb = qrRemainingPg,
                    ProducedCount = _TotalCode,
                    OperatorUser = Shared.LoggedInUser?.OperatorUserId ?? Shared.LoggedInUser?.UserId ?? "ACC001",
                    ProductId = _SelectedJob?.THJobProductId ?? "",
                    ProductName = _SelectedJob?.THJobProductName ?? "",
                    TotalPrint = TotalRlinkPrinted,
                    StatusGood = _NumberOfCheckPassed,
                    StatusFail = _NumberOfCheckFailed,
                    TotalCheck = _TotalChecked,
                    Product_A = _errorCountA,
                    Product_B = _errorCountB,
                    Product_F = _errorCountF,
                    Timestamp = DateTime.Now,
                    StatsArr = BuildQrStatsArray()
                };

                var result = await service.CompleteJobAsync(request);

                if (result == null || !result.IsSuccess)
                {
                    string errMsg = result?.Message ?? "Không nhận được phản hồi từ R-Link Master.";
                    CustomMessageBox.Show($"Hoàn thành job thất bại!\n{errMsg}", Lang.Error,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Console.WriteLine($"[CompleteJob] ✘ {errMsg}");
                    btnCompleteJob.Enabled = true;
                    return;
                }

                // ── Ghi log hoàn thành job vào DB ────────────────────────────────
                _rlinkLogService.InsertCompletedJobLog(
                    jobName: request.JobName,
                    userId: Shared.LoggedInUser?.UserId,
                    lineId: request.LineId,
                    productId: request.ProductId,
                    qrUsed: request.QrUsedCount,
                    totalPrint: request.TotalPrint,
                    statusGood: request.StatusGood,
                    statusFailed: request.StatusFail,
                    totalCheck: request.TotalCheck);

                // ── Đánh dấu job hoàn thành ──────────────────────────────────────────
                if (_SelectedJob != null)
                {
                    _SelectedJob.CompleteJobStatus = CompleteJobStatus.Completed;
                    _SelectedJob.SaveFile();
                }
                StopRunLogTimer();
                FireRLinkLog("completed");

                // ── Giải phóng QR đã cấp nhưng chưa in về trạng thái ban đầu ──
                try
                {
                    string jobNameForRelease = _SelectedJob?.FileName ?? "";
                    var unreleasedQr = await Task.Run(() =>
                    {
                        var list = Services.THTrueMilk.RLinkMaster.RLinkLogService
                            .GetQrUsedButNotPrintedByJobPg(jobNameForRelease);
                        if (list.Count == 0)
                            list = Services.THTrueMilk.RLinkMaster.RLinkLogService
                                .GetQrUsedButNotPrintedByJob(jobNameForRelease);
                        if (list.Count > 0)
                        {
                            Services.THTrueMilk.RLinkMaster.RLinkLogService
                                .MarkQrAsUnusedInSQLiteByCode(list);
                            Services.THTrueMilk.RLinkMaster.RLinkLogService
                                .MarkQrAsUnusedInPgByCode(list);
                        }
                        return list;
                    });
                    if (unreleasedQr.Count > 0)
                        Console.WriteLine($"[CompleteJob] ✔ Đã giải phóng {unreleasedQr.Count} QR chưa in về trạng thái ban đầu.");
                }
                catch (Exception releaseEx)
                {
                    Console.WriteLine($"[CompleteJob] ✘ Lỗi giải phóng QR chưa in: {releaseEx.Message}");
                }

                // ── Lưu QR mới nếu server có cấp phát ────────────────────────────────
                bool hasNewQr = result.AllocatedQrCodes?.Count > 0;
                if (hasNewQr)
                {
                    bool savedOk = false;
                    try
                    {
                        _ParentForm?.ProcessAllocatedQrCodesFromComplete(
                            result.AllocatedQrCodes,
                            Shared.Settings?.LineId ?? "",
                            _SelectedJob?.THJobBatchNo ?? "");

                        if (_rlinkLogService != null)
                        {
                            await Task.Run(() => _rlinkLogService.SaveAllocatedQrCodesAsync(
                                result.AllocatedQrCodes,
                                _SelectedJob?.FileName ?? "",
                                Shared.Settings?.LineId ?? "",
                                _SelectedJob?.THJobBatchNo ?? ""));
                        }

                        _SelectedJob?.SaveFile();
                        savedOk = true;
                        Console.WriteLine($"[CompleteJob] ✔ Lưu {result.AllocatedQrCodes.Count} QR mới.");
                        if (hasNewQr && savedOk)
                        {
                            string firstQr = result.AllocatedQrCodes.FirstOrDefault() ?? "";
                            string lastQr = result.AllocatedQrCodes.LastOrDefault() ?? "";
                            int count = result.AllocatedQrCodes.Count;
                            string batch = string.IsNullOrEmpty(_lastCameraNsxWithTime) ? ""
                                : NormalizeDateToDdMmYy(_lastCameraNsxWithTime.Split(' ')[0]);
                            string lineId = Shared.Settings?.LineId ?? "";
                            string lineName = Shared.Settings?.RLinkName ?? "";
                            string factoryCode = Shared.Settings?.FactoryCode ?? "";

                            Task.Run(() => RLinkLogService.SaveReceiveHistoryToPostgres(
                                DateTime.Now, count, batch, lineId, lineName, factoryCode, firstQr, lastQr, "", ""));

                            Task.Run(() => RLinkLogService.SaveReceiveHistoryToSQLite(
                                DateTime.Now, count, batch, lineId, lineName, factoryCode, firstQr, lastQr, "", ""));
                        }
                    }
                    catch (Exception saveEx)
                    {
                        Console.WriteLine($"[CompleteJob] ✘ Lỗi lưu QR: {saveEx.Message}");
                    }

                    CustomMessageBox.Show(
                        savedOk
                            ? $"Hoàn thành job thành công!\n{result.Message}\n" +
                              $"✔ Đã nhận và lưu {result.AllocatedQrCodes.Count} QR mới cho ca tiếp theo."
                            : $"Hoàn thành job thành công!\n{result.Message}\n" +
                              $"⚠ Nhận {result.AllocatedQrCodes.Count} QR mới nhưng lưu thất bại — kiểm tra log.",
                        Lang.Info, MessageBoxButtons.OK,
                        savedOk ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                }
                else
                {
                    CustomMessageBox.Show(
                        $"Hoàn thành job thành công!\n{result.Message}",
                        Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                EnableUIComponentWhenLoadData(true);
                btnCompleteJob.Enabled = false;
                btnCompleteJob.Text = "Đã xác nhận hoàn thành";
                btnCompleteJob.BackColor = System.Drawing.Color.FromArgb(0, 199, 82);
                btnCompleteJob.BackgroundColor = System.Drawing.Color.FromArgb(0, 199, 82);
                Shared.Settings.LastActiveJobName = "";
                Shared.SaveSettings();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Lỗi khi gọi API hoàn thành job:\n{ex.Message}", Lang.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"[CompleteJob] Exception: {ex.Message}");
                btnCompleteJob.Enabled = true;
            }
        }
        /// <summary>Đếm số QR is_used=TRUE trong PG (đã dùng) — lọc theo line_id + batch hiện tại.</summary>
        private async Task<int> CountQrUsedInPgAsync()
        {
            try
            {
                var s = Shared.Settings;
                if (string.IsNullOrWhiteSpace(s?.THLocalDbServer) ||
                    string.IsNullOrWhiteSpace(s?.THLocalDbDatabase))
                    return -1;

                string connStr = View.frmDatabase.GetConnectionString(
                    "postgresql", s.THLocalDbServer, s.THLocalDbPort,
                    s.THLocalDbUsername, s.THLocalDbPassword, s.THLocalDbDatabase);
                string table = Code;
                string jobName = _SelectedJob?.FileName ?? "";

                return await Task.Run(() =>
                {
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();

                        // Đếm QR theo job_name_receive_qr_rlinkmaster (tổng QR cấp cho job)
                        string sql = $@"SELECT COUNT(*) FROM ""{table}"" WHERE {JobName} = @jn";

                        using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("jn", jobName);
                            return Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CountQrUsedInPg] ✘ {ex.Message}");
                return -1;
            }
        }
        /// <summary>Đếm tổng số QR is_used=FALSE trong PG (còn dùng được — toàn bảng).</summary>
        private async Task<(int Count, string Error)> CountQrAvailableInPgAsync()
        {
            try
            {
                if (_ParentForm == null)
                    return (-1, "Không có tham chiếu đến frmJobTHTrueMilk.");

                int count = await _ParentForm.CountAvailableQrInDbAsync();
                return (count, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CountQrAvailableInPg] ✘ {ex.Message}");
                return (-1, ex.Message);
            }
        }
        private bool IsDefaultSupportAccount()
        {
            try { return CurrentUser.UserName == "Support"; }
            catch { return false; }
        }
        private void InitControls()
        {
            ChangeCheckMode(Checkmode.Camera);
            //  ChangePictureCamera();
            TransparencyKey = Color.DarkKhaki;
            SetLanguage();
            _TimerDateTime.Start();

            // Menu item for Account Management
            if (Shared.LoggedInUser.Role != 0)
            {
                mnManage.Visible = false;
                if (Shared.LoggedInUser.Role == 1000)
                {
                    mnManage.Visible = true;
                }
            }

            // Show icon camera status
            _LabelStatusCameraList.Add(lblStatusCamera01);
            UpdateStatusLabelCamera();

            // Show icon printer status

            _LabelStatusPrinterList.Add(lblStatusPrinter01);
            UpdateStatusLabelPrinter();


            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected); // Show icon sensor controller status

            // Show icon serial device status
            UpdateUISerialDeviceControllerStatus(Shared.IsSerialDeviceConnected);


            _ = UpdateJobInfomationInterface(); // Get Job Infor


            if (_SelectedJob.CompareType == CompareType.Database) // For Database Compare
            {
                tableLayoutPanelPrintedState.Visible = true;
                btnDatabase.Visible = true;
             //   btnExportData.Visible = true;
              //  btnExportAll.Visible = true;
                if (_SelectedJob.PrinterSeries)
                {
                    _IsPrinterDisconnectedNot = true;
                }
            }
            else // For No - Database Compare
            {
                _IsPrinterDisconnectedNot = false;
                tableLayoutPanelPrintedState.Visible = false;
                btnDatabase.Visible = false;
                btnExportData.Visible = false;
                btnExportAll.Visible = false;
            }

           // btnExportAll.Visible = btnExportResult.Visible = btnExportData.Visible = Shared.UserPermission.Exports;
            btnAccount.Enabled = Shared.UserPermission.Accounts;
            pnlControllButton.Visible = Shared.UserPermission.Controls;
           

            btnAddBarcode.Enabled = Shared.UserPermission.IncreaseProductionQuantity;

            btnPrinterMonitor.Visible = true;

            if (Shared.Settings.ExportOneForAllEnable)
            {
                btnExportData.Visible = false;
                btnExportResult.Visible = false;
            }

            HideControls(btnHistory, btnAccount); // lblStatusCamera01 // lblSensorControllerStatus
            LineName.Text = "Tên Line: " + Shared.Settings.RLinkName ?? "";
            lineId.Text = "Mã Line: " + Shared.Settings.LineId ?? "";
            lineId2.Text =  Shared.Settings.LineId ?? "";
            //pnlCurrentCheck.Text = "Thông tin máy in"; // Printing Process
            lblProcessTitle.Text = "  Thống kê sản xuất";
            UserNameDisplay.Text = "Người dùng: " + CurrentUser.UserName ?? "";

            if (IsDefaultSupportAccount())
                btnStart.Enabled = false;

            SentSaaS.Text = "Mã đồng bộ SaaS thành công"; // Lang.SentSyncDataSaaS;
            SentSAP.Text = "Mã đồng bộ HUB thành công"; //Lang.SentSAPData;
            //ConfirmLabel.Text = Lang.ConfirmCompletion + " " + Lang.Job;
            SyncDataLabel.Text = Lang.SyncData;
            DisposeLabel.Text = Lang.DisposeBarcodes;

            //syncDataManual.Text = Lang.ConfirmCompletion;
            syncDataBtn.Text = Lang.Sync;
            disposeBtn.Text = Lang.Dispose;
           numberOfCode.Text = _SelectedJob.NumberTotalsCode.ToString("N0")+" hộp";
     


            //if (_SelectedJob.IsProcessOrderMode)
            //{
            //    var payloadJob = _SelectedJob?.ProcessOrderItem;
            //    MaterialNumberTextBox.Text = payloadJob?.material_number;
            //    MaterialName.Text = payloadJob?.material_name;
            //    int selectedIndex = _SelectedJob.SelectedBatchIndex;
            //    InitLOTItems();
            //    ChangeBatchInfo(selectedIndex);
            //    Shared.Settings.SelectedBatchIndex = LOTNumberCombo.SelectedIndex = selectedIndex;
            //}

            //if (_SelectedJob.IsReservationMode)
            //{
            //    var payloadJob = _SelectedJob?.ReservationItem;
            //    int selectedIndex = _SelectedJob.SelectedBatchIndex;
            //    InitLOTItems();
            //    ChangeBatchInfo(0);
            //    Shared.Settings.SelectedBatchIndex = LOTNumberCombo.SelectedIndex = selectedIndex;
            //    MaterialNumberTextBox.Text = payloadJob?.material_number;
            //    MaterialName.Text = payloadJob?.material_name;
            //}

     

          
            //VisibleControl(_SelectedJob.IsProcessOrderMode && Shared.UserPermission.isOnline, btnRefesh); //  
            DispatchingActionsPanel.Visible = Shared.UserPermission.isOnline;
            StopPrintedDataProcess(_printedDataProcess);
            if (_checkedDataProcess != null)
            {
                _checkedDataProcess.Stop();
            }

            IsFullHD = true;

            //Visible control in Debug mode
#if DEBUG
            DebugVirtual();
#endif
            UpdateCheckTotalAndPrintedDatabase();


            // ── Enforce trạng thái Completed / Running ──────────────────
            if (_SelectedJob != null && _SelectedJob.CompleteJobStatus == CompleteJobStatus.Completed)
            {
                btnCompleteJob.Enabled = false;
                btnCompleteJob.Text = "Đã xác nhận hoàn thành";
                btnCompleteJob.BackColor = System.Drawing.Color.FromArgb(0, 199, 82);
                btnCompleteJob.BackgroundColor = System.Drawing.Color.FromArgb(0, 199, 82);
                btnStart.Enabled = false;
                btnAddBarcode.Enabled = false;
                txtAddTotalTon.ReadOnly = true;
                btnAddTons.Enabled = false;
                CuzAlert.Show(
                    "Job này đã hoàn thành – không thể tiếp tục chạy.",
                    Alert.enmType.Info,
                    new Size(500, 120),
                    new Point(Location.X, Location.Y),
                    Size,
                    false);
            }
            else if (Shared.OperStatus == OperationStatus.Running ||
                     Shared.OperStatus == OperationStatus.Processing)
            {
                btnCompleteJob.Enabled = false;
            }
           
            else
            {
               // btnCompleteJob.Enabled = true;
            }
            InitRLinkLogService();

            // Runtime timer
            _runTimeTimer = new System.Windows.Forms.Timer();
            _runTimeTimer.Interval = 1000;
            _runTimeTimer.Tick += (s, args) =>
            {
                var total = AccumulatedRunTime + RunStopwatch.Elapsed;
                lblRunningTime.Text = FormatRunTime(total);
                if (_SelectedJob != null)
                    _SelectedJob.TotalRunTimeTicks = total.Ticks;

                //// ── Hiển thị thời gian bắt đầu / kết thúc ──
                //if (_SelectedJob?.FirstRunTime > DateTime.MinValue)
                //{
                //    string start = _SelectedJob.FirstRunTime.ToString("dd/MM HH:mm");
                //    string end = RunStopwatch.IsRunning
                //        ? "\u0110ang ch\u1ea1y..."
                //        : (_SelectedJob.LastRunTime > DateTime.MinValue
                //            ? _SelectedJob.LastRunTime.ToString("dd/MM HH:mm")
                //            : "--");
                //    lblTimeProcess.Text = "B\u1eaft \u0111\u1ea7u: " + start + " | K\u1ebft th\u00fac: " + end;
                //}
                //else
                //{
                //    lblTimeProcess.Text = "B\u1eaft \u0111\u1ea7u: -- | K\u1ebft th\u00fac: --";
                //}
            };
            AccumulatedRunTime = TimeSpan.FromTicks(_SelectedJob?.TotalRunTimeTicks ?? 0);
            lblRunningTime.Text = FormatRunTime(AccumulatedRunTime);
            if (Shared.OperStatus == OperationStatus.Running ||
                Shared.OperStatus == OperationStatus.Processing)
            {
                RunStopwatch.Start();
                _runTimeTimer.Start();
            }
        }


       

        

        // public Checkmode CheckMode { get; set; }
        private void ChangeCheckMode(Checkmode checkMode)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ChangeCheckMode(checkMode)));
                return;
            }
            try
            {

            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Event Action
        private void InitEvents()
        {
            // Timer-based batch UI update — ~30fps
            _uiBatchTimer = new System.Windows.Forms.Timer { Interval = 33 };
            _uiBatchTimer.Tick += UiBatchTimer_Tick;
            _uiBatchTimer.Start();

            _TimerDateTime.Tick += TimerDateTime_Tick;
            btnStart.Click += ActionChanged;
            btnStop.Click += ActionChanged;
            btnTrigger.MouseUp += BtnTrigger_MouseUp;
            btnTrigger.MouseDown += BtnTrigger_MouseDown;
            pnlSentData.MouseDown += (obj, e) =>
            {
                ReleaseCapture();
                Message m = Message.Create(this.Handle, 0x00A1, (IntPtr)0x0002, IntPtr.Zero);
                base.WndProc(ref m);
            };
            pnlJobInformation.MouseDown += (obj, e) =>
            {
                if (e.Y <= pnlJobInformation.TitleHeight)
                {
                    ReleaseCapture();
                    Message m = Message.Create(this.Handle, 0x00A1, (IntPtr)0x0002, IntPtr.Zero);
                    base.WndProc(ref m);
                }
            };

         
            pnlMenu.DoubleClick += PnlMenu_DoubleClick;
            btnJob.Click += ActionChanged;
            btnExit.Click += ActionChanged;
            btnExportResult.Click += ActionChanged;
            btnDatabase.Click += ActionChanged;
            btnViewDatabase.Click += ActionChanged;
            btnAccount.Click += ActionChanged;
            btnHistory.Click += ActionChanged;
            btnSettings.Click += ActionChanged;
            pnlPrintedCode.Click += ActionChanged;
            pnlCheckFailed.Click += ActionChanged;
            prBarCheckPassed.Click += ActionChanged;
            pnlCheckPassed.Click += ActionChanged;
            pnlTotalChecked.Click += ActionChanged;
            btnViewResult.Click += ActionChanged;
            lblCheckedResult.Click += ActionChanged;
     
            //if (_summaryTotalLabel != null)
            //    _summaryTotalLabel.Click += ActionChanged;
            btnExportData.Click += ActionChanged;
            btnExportAll.Click += ActionChanged;
            syncDataBtn.Click += ActionChanged;
            disposeBtn.Click += ActionChanged;
            syncDataManual.Click += ActionChanged;
            lblSentData.Click += (s, e) => ShowSentDataInfo();
           
            btnAddBarcode.Click += ActionChanged;
            txtAddTotalTon.TextChanged += txtAddTotalTon_TextChanged;


            mnManage.Click += ActionChanged;
            mnChangePassword.Click += ActionChanged;
            mnLogOut.Click += ActionChanged;

            this.Activated += FrmMainTHTrueMilk_Activated;

            Shared.OnSyncDataParameterChange += Shared_OnSyncDataParameterChange;
            Shared.OnSyncCheckDataParameterChange += Shared_OnSyncCheckDataParameterChange;



            Shared.OnCameraStatusChange += Shared_OnCameraStatusChange;
            Shared.OnCameraReadDataChange += Shared_OnCameraReadDataChange;
            Shared.OnCameraPositionDataChange += Shared_OnCameraPositionDataChange;
            Shared.OnSerialDeviceReadDataChange += Shared_OnSerialDeviceReadDataChange;
            Shared.OnPrinterDataChange += Shared_OnPrinterDataChange;
            Shared.OnPrintingStateChange += Shared_OnPrintingStateChange;
            Shared.OnPrinterStatusChange += Shared_OnPrinterStatusChange;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnSensorControllerChangeEvent += Shared_OnSensorControllerChangeEvent;
            Shared.OnSerialDeviceControllerChangeEvent += Shared_OnSerialDeviceControllerChangeEvent;


            Shared.OnVerifyAndPrindSendDataMethod += Shared_OnVerifyAndPrindSendDataMethod;
            OnReceiveVerifyDataEvent += SendVerifiedDataToPrinter;
            Shared.OnLogError += Shared_OnLogError;
            Shared.OnDatabaseStatusChange += Shared_OnDatabaseStatusChange;
            UpdateStatusLabelDatabase();

            _dbStatusSyncTimer = new System.Windows.Forms.Timer();
            _dbStatusSyncTimer.Interval = 3000;
            _dbStatusSyncTimer.Tick += (s, e) => UpdateStatusLabelDatabase();
            _dbStatusSyncTimer.Start();
            //Resize += (obj, e) =>
            //{
            //    panel1.Width = Size.Width / 6;
            //    if (Size.Width < 850 || Size.Height < 850)
            //    {
            //        if (IsFullHD)
            //        {
            //            IsFullHD = false;
            //        }
            //    }
            //    else if (Size.Width >= 850 || Size.Height >= 850)
            //    {
            //        if (!IsFullHD)
            //        {
            //            IsFullHD = true;
            //        }
            //    }
            //    // prBarCheckPassed giữ nguyên fixed size 120x120
            //};

            _QueueBufferPrinterResponseData.Clear();
            try { ReceiveResponseFromPrinterHandlerAsync(); }
            catch (Exception ex) { ProjectLogger.WriteError("[InitEvents] Exception before Mode1 sub: " + ex.Message, ex); }

            // Mode 1: lắng nghe sự kiện QR đổi qua 12h đêm
            if (_ParentForm != null)
            {
                _ParentForm.Mode1QrCodeChanged -= ParentForm_OnMode1QrCodeChanged;
                _ParentForm.Mode1QrCodeChanged += ParentForm_OnMode1QrCodeChanged;
                ProjectLogger.WriteInfo("[Mode1] Subscribed Handler B (InitEvents)");
            }

            btnPrinterMonitor.Click += (s, e) =>
            {
                var printer = Shared.Settings?.PrinterList?.FirstOrDefault();
                if (printer != null && !string.IsNullOrEmpty(printer.IP))
                {
                    new UtilityForms.THTrueMilk.frmPrinterMonitorCustom(printer.IP, printer.NumPortRemote).ShowDialog();
                }
            };
        }

        private void FrmMainTHTrueMilk_Activated(object sender, EventArgs e)
        {
            LineName.Text = "Tên Line: " + (Shared.Settings?.RLinkName ?? "");

            // Đảm bảo Mode1 event luôn subscribed khi form được activated lại
            if (_ParentForm != null && !_ParentForm.IsDisposed)
            {
                _ParentForm.Mode1QrCodeChanged -= ParentForm_OnMode1QrCodeChanged;
                _ParentForm.Mode1QrCodeChanged += ParentForm_OnMode1QrCodeChanged;
            }
        }

        private void Shared_OnSyncCheckDataParameterChange(object sender, EventArgs e)
        {
            try
            {
                if (sender is SyncDataParams ParamsName)
                {
                    switch (ParamsName.DataType)
                    {
                        case SyncDataParams.SyncDataType.SAPSuccess:
                            _SelectedJob.NumberOfCheckSAPSentCodes = ++CheckSAPSuccess;
                            break;
                        case SyncDataParams.SyncDataType.SaaSSuccess:
                            Shared.NumberOfCheckSentSaaS = ++CheckSaaSSuccess;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
                // optional logging
            }

        }
        /// <summary>
        /// Hiển thị thông tin job vừa tạo lên các textbox trên frmMain.
        /// </summary>
        public void UpdateJobInfoDisplay(JobModel job)
        {
            _SelectedJob = job;
            // ← KHÔNG ghi đè Shared.Settings.THOperatingMode ở đây!
            // Shared.Settings.THOperatingMode chỉ nên được set từ Settings UI,
            // không phải từ việc mở job — nếu không sẽ làm sai mode khi tạo job mới.
            if (InvokeRequired) { Invoke(new Action(() => UpdateJobInfoDisplay(job))); return; }
            if (job == null) return;
            try
            {
                _SelectedJob = job;

                var s = Shared.Settings;

                txtModeOperator.Text = job.THJobOperatingMode.ToDisplayString();
                label12.Text = "Chế độ in: " + (int)job.THJobOperatingMode;

                bool isMode1 = job.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode;
                bool isMode4 = job.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange;
                bool isMode2 = job.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;

                if (isMode4)
                {
                    // Mode 4: ẩn txtDetalForMode1, hiển thị txtCoefficient
                    txtTimeChangeCode.Visible = false;
                    txtDetalForMode1.Visible = false;
                    txtCoefficient.Location = txtTimeChangeCode.Location;
                    txtCoefficient.Text = (Shared.Settings?.THReserveFactor ?? 1.5).ToString();
                    txtCoefficient.Visible = true;
                    label15.Text = "Hệ số dự phòng";
                    label15.Visible = true;
                }
                else if (isMode1)
                {
                    txtTimeChangeCode.Visible = false;
                    txtCoefficient.Visible = false;
                    txtDetalForMode1.Location = txtTimeChangeCode.Location;
                    txtDetalForMode1.Text = job.THJobDeltaMinutes + " giây";
                    txtDetalForMode1.Visible = true;
                    label15.Text = "Làm mới QR trước 12h đêm";
                    label15.Visible = true;
                }
                else
                {
                    txtTimeChangeCode.Visible = true;
                    txtTimeChangeCode.Text = (isMode2 ? job.THJobNMinutes : 0) + " phút";
                    txtDetalForMode1.Visible = false;
                    txtCoefficient.Visible = false;
                    label15.Text = "Chu kì đổi QR Code (Phút)";
                    label15.Visible = true;
                }
               

                txtBufferPrint.Text = job.THJobBufferCount + " mã";
                txtTimeLogSave.Text = job.THJobLogInterval + " phút";
                txtPathErrorImage.Text = job.THJobErrorImageFolder ?? "";
                txtModelTrainCamera.Text = !string.IsNullOrEmpty(job.THJobCameraModelForTraining)
                    ? job.THJobCameraModelForTraining
                    : (Shared.Settings.THCameraModelForTraining ?? "");
                txtMaxConsecutiveDefects.Text = job.THMaxConsecutiveError + " lỗi";
                UpdateConsecutiveErrorDisplay();
                txtMonitor.Text = job.THJobMonitorInterval + " phút";
                SetProductGtinText(job.THJobProductGtin ?? "");
                txtQrThreshold.Text = job.THJobQrThreshold + " ";

                if (job.NumberTotalsCode > 1)
                    numberOfCode.Text = job.NumberTotalsCode.ToString("N0") + " hộp";

                lblTotalQrcode.Text = $"SP thực tế: {_SelectedJob?.TotalRlinkPrinted ?? 0:N0}";
                lblTotalTon.Text = $"0";

                MaterialNumberTextBox.Text = !string.IsNullOrEmpty(job.THJobProductId)
                    ? job.THJobProductId
                    : job.ProcessOrderItem?.material_number ?? "";

                MaterialName.Text = !string.IsNullOrEmpty(job.THJobProductName)
                    ? job.THJobProductName
                    : job.ProcessOrderItem?.material_name ?? "";

                txtBatchNumber.Text = !string.IsNullOrEmpty(job.THJobBatchNo)
                    ? job.THJobBatchNo
                    : job.ProcessOrderItem?.batch_info?.FirstOrDefault()?.batch ?? "";
                txtTotalTon.Text = $"{job.EstimatedTons:N2} tấn";

                txtAddTotalTon.Text = "0";
                if (lblAdditionalCodes != null) lblAdditionalCodes.Text = "";

                if (!string.IsNullOrEmpty(job.THJobImageUrl))
                {
                    var img = ProductImageHelper.GetProductImage(job.THJobImageUrl, job.THJobProductId);
                    if (img != null)
                    {
                        var old = pictureBox4.Image;
                        pictureBox4.Image = new Bitmap(img);  // UI giữ bản copy riêng, không bị ImageCache dispose
                        old?.Dispose();
                    }
                }

                lblLastPrintedPage.Text = string.Format("{0:N0}", job.TotalRlinkPrinted);
                Console.WriteLine($"[DEBUG UPDATEUI] TXT = '{txtBatchNumber.Text}' | job.THJobBatchNo = '{job.THJobBatchNo}' | ProcessOrderItem = {job.ProcessOrderItem != null}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateJobInfoDisplay] Lỗi: {ex.Message}");
            }
        }
        private void SetProductGtinText(string gtin)
        {
            rtbProductGtin.Clear();
            rtbProductGtin.AppendText(gtin);

            if (!string.IsNullOrEmpty(gtin))
            {
                var boldFont = new Font(rtbProductGtin.Font, FontStyle.Bold);
                rtbProductGtin.Select(0, gtin.Length);
                rtbProductGtin.SelectionFont = boldFont;

                int last5Len = Math.Min(5, gtin.Length);
                string last5 = gtin.Substring(gtin.Length - last5Len);
                int idx5 = gtin.LastIndexOf(last5, StringComparison.Ordinal);
                if (idx5 >= 0)
                {
                    rtbProductGtin.Select(idx5, last5Len);
                    rtbProductGtin.SelectionFont = boldFont;
                    rtbProductGtin.SelectionColor = Color.Red;
                }
                rtbProductGtin.Select(0, 0);
            }
        }

        private void PnlGtinWrapper_Paint(object sender, PaintEventArgs e)
        {
            var pnl = sender as Panel;
            if (pnl == null) return;

            int radius = 8;
            var rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = GetRoundRectangle(rect, radius))
            {
                using (var brush = new SolidBrush(pnl.BackColor))
                    e.Graphics.FillPath(brush, path);

                using (var pen = new Pen(Color.FromArgb(224, 224, 224)))
                    e.Graphics.DrawPath(pen, path);
            }
        }

        private static GraphicsPath GetRoundRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int d = radius * 2;
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void Shared_OnSyncDataParameterChange(object sender, EventArgs e)
        {
            try
            {
                if (_SentPrintedCodeObtainFromFile.Count == 0)
                {
                    string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
                    _SentPrintedCodeObtainFromFile = FileFuncs.ReadCodeData(sentDataPath);
                }
                if (sender is SyncDataParams ParamsName)
                {
                    switch (ParamsName.DataType)
                    {
                        case SyncDataParams.SyncDataType.SAPSuccess:
                            _SentPrintedCodeObtainFromFile[ParamsName.CodeIndex - 1][PrintingValues.SAPStatus] = "success";

                            SAPSuccess = Shared.NumberOfSentSAP = _SelectedJob.NumberOfSAPSentCodes =
                            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 &&
                                                             item[5].Equals("success", StringComparison.OrdinalIgnoreCase));
                            break;
                        case SyncDataParams.SyncDataType.SaaSSuccess:
                            _SentPrintedCodeObtainFromFile[ParamsName.CodeIndex - 1][PrintingValues.SaaSStatus] = "success";
                            SaaSSuccess = Shared.NumberOfSentSaaS = _SelectedJob.NumberOfSaaSSentCodes =
                            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 &&
                                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));
                            break;
                        case SyncDataParams.SyncDataType.SAPFailed:
                        case SyncDataParams.SyncDataType.SaaSFailed:
                            break;
                    }
                }
            }
            catch (Exception)
            {
                // optional logging
            }

        }
        private void Shared_OnDatabaseStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelDatabase();
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
                ShowLabelIcon(lblStatusDatabase, "Database", Properties.Resources.database__connected);
            }
            else
            {
                ShowLabelIcon(lblStatusDatabase, "Database", Properties.Resources.database__disconnect);
            }
        }
        private void GetSampleRaise(object sender, EventArgs e)
        {
            if (ProjectLabel.IsTHTrueMilk)
            {
                CustomMessageBox.Show("Disposal Process", "Disposal Process", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                return;
            }
            GetSampleWithScanner();

            // Test
            //IsBarcodeWithinThreshold(49, 40, 233, 245, 49, 40, 9, 9, 269);
            //IsBarcodeWithinThreshold1(49, 40, 233, 245, 9, 9, 269);
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

        private void PnlMenu_DoubleClick(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                return;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        private void TimerDateTime_Tick(object sender, EventArgs e)
        {
            toolStripDateTime.Text = DateTime.Now.ToString(_DateTimeFormatTicker);
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

            // ── Qua ngày mới: cập nhật NSX/HSD cho mã Waiting (không đổi QR) ──
#if DEBUG
            bool debugTrigger = DebugNsxHsdTime.HasValue
                && !_debugNsxHsdTriggered
                && DateTime.Now >= DebugNsxHsdTime.Value;
#else
            bool debugTrigger = false;
#endif
            if (DateTime.Now.Date != _lastDateForNsxHsd || debugTrigger)
            {
                // Mode 4 (BatchOneQrCodeNoChange): giữ nguyên NSX/HSD, không cập nhật
                if (_SelectedJob?.THJobOperatingMode == Model.THTrueMilk.THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                    && _SelectedJob?.THJobAllowNsxHsdChange != true)
                {
                    _lastDateForNsxHsd = DateTime.Now.Date;
                    Console.WriteLine($"[Midnight] Mode 4 — bỏ qua cập nhật NSX/HSD");
                }
                //Nhan.To_070926_Xoa_UpdateNsxHsd_khoi_TimerDateTime_Tick_vi_rebuild_da_cap_nhat_dong_bo_QR_NSX_HSD
                // Lý do: UpdateNsxHsd() chỉ đổi NSX/HSD mà KHÔNG đổi QR → không đồng bộ.
                // RebuildMode1DatabaseForNewDayAsync() đã gọi UpdateQrCodeAndNsxHsd() trên background thread
                // → đảm bảo QR + NSX + HSD luôn đổi CÙNG LÚC, đồng bộ 100%.
                // Bỏ call này: vừa fix UI freeze (1.1M rows chạy trên UI thread),
                // vừa đảm bảo dữ liệu luôn đồng bộ.
                // THJobBatchNo vẫn được set từ UpdateBatchFromCamera() + Camera NSX detection.

                //else if (_PrintedCodeObtainFromFile is PrintedCodeVirtualList virtualList)
                //{
                //    DateTime nsxBase = (_SelectedJob?.CurrentBatchDate > DateTime.MinValue)
                //        ? _SelectedJob.CurrentBatchDate
                //        : DateTime.Now;
                //    string today = nsxBase.ToString("dd MM yy");
                //    string hsdDate = nsxBase.AddDays((_SelectedJob?.THJobExpiryMonths ?? 6) - 1).ToString("dd MM yy");
                //    virtualList.UpdateNsxHsd(today, hsdDate);
                //    _lastDateForNsxHsd = DateTime.Now.Date;
                //    Console.WriteLine($"[Midnight] NSX/HSD updated → {today} / {hsdDate} | Waiting={virtualList.GetWaitingCount()}");
                //    if (_SelectedJob != null)
                //    {
                //        string batchToUse = "";
                //        if (!string.IsNullOrEmpty(_lastCameraNsxWithTime))
                //        {
                //            string dateOnly = _lastCameraNsxWithTime.Split(' ')[0];
                //            batchToUse = NormalizeDateToDdMmYy(dateOnly);
                //        }
                //        if (string.IsNullOrWhiteSpace(batchToUse))
                //            batchToUse = today;
                //        _SelectedJob.THJobBatchNo = batchToUse;
                //        Console.WriteLine($"[Midnight] THJobBatchNo updated → {batchToUse}");
                //    }
                //#if DEBUG
                //    if (debugTrigger) _debugNsxHsdTriggered = true;
                //#endif
                //}
            }
        }

        #region Auto Trigger
        private void Button_AutoTrigger(object sender, EventArgs e)
        {
            try
            {
                _autoTrigger.StartTimer();
                _autoTrigger.TriggerEvent += _autoTrigger_TriggerEvent;
            }
            catch (Exception)
            {
            }
        }
        private void Button_StopAutoTrigger(object sender, EventArgs e)
        {
            try
            {
                _autoTrigger.StopTimer();
                _autoTrigger.TriggerEvent -= _autoTrigger_TriggerEvent;
                _autoTrigger = null;
            }
            catch (Exception)
            {
            }
        }

        private void _autoTrigger_TriggerEvent(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Shared.RaiseOnCameraTriggerOnChangeEvent();
                Shared.RaiseOnCameraTriggerOffChangeEvent();
            });
        }
        #endregion Auto Trigger

      

        /// <summary>
        /// Query log từ PostgreSQL trước, nếu không có kết quả thì fallback sang SQLite.
        /// </summary>
        private List<string[]> QueryLogFromBothDbs(string connStr, string table, string filter, string jobName,
            string pgSql, string sqliteSql)
        {
            var result = new List<string[]>();

            if (!string.IsNullOrWhiteSpace(connStr))
            {
                try
                {
                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        Console.WriteLine($"[{table}] PG connected OK");
                        using (var cmd = new Npgsql.NpgsqlCommand(pgSql, conn))
                        {
                            if (!string.IsNullOrWhiteSpace(jobName))
                                cmd.Parameters.AddWithValue("job", jobName);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var cols = new string[reader.FieldCount];
                                    for (int i = 0; i < reader.FieldCount; i++)
                                        cols[i] = reader.IsDBNull(i) ? "" : reader.GetString(i);
                                    result.Add(cols);
                                }
                            }
                        }
                    }
                    Console.WriteLine($"[{table}] PG rows: {result.Count}");
                    if (result.Count > 0) return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{table}] PG error: {ex.GetType().Name}: {ex.Message}");
                    DbLogger.Error($"[{table}] PG error: {ex.Message}", ex);
                }
            }
            else
            {
                Console.WriteLine($"[{table}] PG connStr null/empty → skip PG");
            }

            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(
                    Services.THTrueMilk.RLinkMaster.RLinkLogService.ConnStr))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(sqliteSql, conn))
                    {
                        if (!string.IsNullOrWhiteSpace(jobName))
                            cmd.Parameters.AddWithValue("@job", jobName);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var cols = new string[reader.FieldCount];
                                for (int i = 0; i < reader.FieldCount; i++)
                                    cols[i] = reader.IsDBNull(i) ? "" : reader.GetString(i);
                                result.Add(cols);
                            }
                        }
                    }
                }
                Console.WriteLine($"[{table}] SQLite rows: {result.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{table}] SQLite error: {ex.GetType().Name}: {ex.Message}");
                DbLogger.Error($"[{table}] SQLite error: {ex.Message}", ex);
            }

            return result;
        }

        private List<string[]> FetchRlinkLogInRows()
        {
            var result = new List<string[]>();
            string jobName = _SelectedJob?.FileName ?? "";
            string productNameCol = THDb.ProductName;
            string connStr = Services.THTrueMilk.RLinkMaster.RLinkLogService.GetPgConnStr();

            // ── Diagnostic: thêm log để biết PG có được dùng không ──
            if (string.IsNullOrWhiteSpace(connStr))
                Console.WriteLine("[FetchRlinkLogInRows] ⚠ GetPgConnStr() = null → chỉ query SQLite");
            else
                Console.WriteLine($"[FetchRlinkLogInRows] ✔ PG connStr OK, jobName='{jobName}'");

            string filter = string.IsNullOrWhiteSpace(jobName) ? "" : $"WHERE {JobName} = @job";

            result = QueryLogFromBothDbs(connStr, LogIn, filter, jobName,
                pgSql: $@"SELECT {Id}::text, COALESCE({JobName},''), COALESCE({Batch},''), COALESCE({Status},''), 
                          COALESCE({OperatorUser},''), COALESCE({ProductId},''), COALESCE({Qty}::text,'0'),
                          COALESCE({QrCode},''), COALESCE({QrDetail},''), COALESCE({Timestamp}::text,''), COALESCE({RlinkStatus},''), COALESCE({IsSent}::text,'0') AS {IsSent},
                          COALESCE({LineId},''), COALESCE({productNameCol},'')
                   FROM {LogIn} {filter} ORDER BY {Id} DESC LIMIT 4900",
                sqliteSql: $@"SELECT CAST({Id} AS TEXT), COALESCE({JobName},''), COALESCE({Batch},''), COALESCE({Status},''),
                              COALESCE({OperatorUser},''), COALESCE({ProductId},''), COALESCE(CAST({Qty} AS TEXT),'0'),
                              COALESCE({QrCode},''), COALESCE({QrDetail},''), COALESCE({Timestamp},''), COALESCE({RlinkStatus},''), COALESCE(CAST({IsSent} AS TEXT),''),
                              COALESCE({LineId},''), COALESCE({productNameCol},'')
                        FROM {LogIn} {filter} ORDER BY {Id} DESC LIMIT 4900");

            Console.WriteLine($"[FetchRlinkLogInRows] rows with filter: {result.Count}");

            return result;
        }

        private async System.Threading.Tasks.Task<List<string[]>> FetchRlinkLogInRowsAsync()
        {
            return await System.Threading.Tasks.Task.Run(() => FetchRlinkLogInRows());
        }

        private async void BtnPreviewLogCamera_Click(object sender, EventArgs e)
        {
            var rows = await FetchRlinkLogCameraRowsAsync();
            if (rows.Count == 0)
            {
                CustomMessageBox.Show("Chưa có dữ liệu log camera",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var frm = new frmPreviewLog(rows, isCameraLog: true, refreshCallback: FetchRlinkLogCameraRowsAsync))
            {
                frm.Text = "Lịch sử Log Camera";
                frm.ShowDialog(this);
            }
        }

        private List<string[]> FetchRlinkLogCameraRows()
        {
            string jobName = _SelectedJob?.FileName ?? "";
            string productNameCol = THDb.ProductName;
            string connStr = Services.THTrueMilk.RLinkMaster.RLinkLogService.GetPgConnStr();
            string filter = string.IsNullOrWhiteSpace(jobName) ? "" : $"WHERE {JobName} = @job";

            var result = QueryLogFromBothDbs(connStr, LogCamera, filter, jobName,
                pgSql: $@"SELECT {Id}::text, COALESCE({JobName},''), COALESCE({Batch},''), 
                                 COALESCE({OperatorUser},''), COALESCE({ProductId},''),
                                 COALESCE({StatusGood}::text,'0'), COALESCE({StatusFail}::text,'0'),
                                  COALESCE({TotalCheck}::text,'0'), COALESCE({QrCode},''), COALESCE({QrDetail},''), COALESCE({Timestamp}::text,''),
                                  COALESCE({RlinkStatus},''), COALESCE({IsSent}::text,'0') AS {IsSent},
                                  COALESCE({LineId},''), COALESCE({productNameCol},''),
                                  COALESCE({Status},'')
                          FROM {LogCamera} {filter} ORDER BY {Id} DESC LIMIT 4900",
                sqliteSql: $@"SELECT CAST({Id} AS TEXT), COALESCE({JobName},''), COALESCE({Batch},''),
                                     COALESCE({OperatorUser},''), COALESCE({ProductId},''),
                                     COALESCE(CAST({StatusGood} AS TEXT),'0'), COALESCE(CAST({StatusFail} AS TEXT),'0'),
                                     COALESCE(CAST({TotalCheck} AS TEXT),'0'), COALESCE({QrCode},''), COALESCE({QrDetail},''), COALESCE({Timestamp},''),
                                      COALESCE({RlinkStatus},''), COALESCE(CAST({IsSent} AS TEXT),''),
                                      COALESCE({LineId},''), COALESCE({productNameCol},''),
                                      COALESCE({Status},'')
                              FROM {LogCamera} {filter} ORDER BY {Id} DESC LIMIT 4900");

            return result;
        }

        private async System.Threading.Tasks.Task<List<string[]>> FetchRlinkLogCameraRowsAsync()
        {
            return await System.Threading.Tasks.Task.Run(() => FetchRlinkLogCameraRows());
        }

        private void BtnTrigger_MouseDown(object sender, MouseEventArgs e)
        {
            var camType = Shared.Settings.CameraList.FirstOrDefault()?.CameraType;
            if (camType == CameraType.CV_X || camType == CameraType.VS_C)
            {
                if (Shared.vscCamera != null && Shared.vscCamera.IsConnected())
                {
                    try
                    {
                        // Fire-and-forget: chỉ gửi lệnh TRG, không chờ phản hồi
                        // Phản hồi sẽ được nhận qua FramedListenLoop → DispatchFrame
                        Shared.vscCamera.SendRawCommandFramed("TRG");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Keyence Trigger] Error: {ex.Message}");
                    }
                }
                return;
            }
            Shared.RaiseOnCameraTriggerOnChangeEvent();
        }
        private void BtnTrigger_MouseUp(object sender, MouseEventArgs e)
        {
            var camType = Shared.Settings.CameraList.FirstOrDefault()?.CameraType;
            if (camType == CameraType.CV_X || camType == CameraType.VS_C)
            {
                return;
            }
            Shared.RaiseOnCameraTriggerOffChangeEvent();
        }

        private async void Shared_OnVerifyAndPrindSendDataMethod(object sender, EventArgs e)
        {
            if (Shared.Settings.VerifyAndPrintBasicSentMethod) return;
            EnableUIComponentWhenLoadData(false);
            _Emergency.Clear();
            _Emergency = await InitVNPUpdatePrintedStatusConditionBuffer();
            EnableUIComponentWhenLoadData(true);
        }

        private void SendVerifiedDataToPrinter(object sender, EventArgs e) // Verify and print 
        {
            string command = "DATA;";
            string[] arr = sender as string[];
            if (Shared.Settings.VerifyAndPrintBasicSentMethod)
                command += arr[1] == null ? Shared.Settings.FailedDataSentToPrinter : arr[1];
            else
            {
                arr = arr.Skip(2).ToArray();
                if (Shared.Settings.PrintFieldForVerifyAndPrint.Count() == 0)
                {
                    command += string.Join(Shared.Settings.SplitCharacter.ToString(), arr
                        .Select(x => x == null ? Shared.Settings.FailedDataSentToPrinter : x));
                }
                else
                {
                    command += string.Join(Shared.Settings.SplitCharacter.ToString(), Shared.Settings.PrintFieldForVerifyAndPrint
                        .Where(x => x.Index < arr.Length + 1)
                        .Select(x => arr[x.Index - 1] == null ? Shared.Settings.FailedDataSentToPrinter : arr[x.Index - 1])
                        );
                }
            }

            if (podController != null)
            {
                podController.Send(command);
                IncrementSentPrinter();
            }
            else
            {
                podController = Shared.Settings.PrinterList.Where(p => p.RoleOfPrinter == RoleOfStation.ForProduct).FirstOrDefault().PODController;
            }
        }
        #endregion End Event Action

        #region 
        private async Task<ConcurrentDictionary<string, int>> InitVNPUpdatePrintedStatusConditionBuffer()
        {
            var result = new ConcurrentDictionary<string, int>();
            var _CheckedResultCodeSet = new HashSet<string>();
            string validCond = ComparisonResult.Valid.ToString();
            int columnCount = _ColumnNames.Length;
            foreach (string[] array in _CheckedResultCodeList)
            {
                if (columnCount == array.Length && array[2] == validCond)
                {
                    _CheckedResultCodeSet.Add(array[1]);
                }
            }

            if (_PrintedCodeObtainFromFile.Count > 0)
            {
                int codeLenght = _PrintedCodeObtainFromFile[0].Count() - 1;
                for (int index = 0; index < _PrintedCodeObtainFromFile.Count; index++)
                {
                    string[] row = _PrintedCodeObtainFromFile[index].ToArray();
                    string data = "";
                    foreach (PODModel item in _SelectedJob.PODFormat)
                    {
                        if (item.Type == PODModel.TypePOD.DATETIME)
                        {
                            data += DateTime.Now;
                        }
                        else if (item.Type == PODModel.TypePOD.FIELD)
                        {
                            data += row[item.Index];
                        }
                        else
                        {
                            data += item.Value;
                        }
                    }

                    if (!_CheckedResultCodeSet.Contains(data))
                    {
                        if (_IsVerifyAndPrintMode)
                        {
                            string tmp = "";
                            row = row.Skip(2).ToArray(); // Exclude index and status column
                            for (int i = 1; i <= row.Length; i++)
                            {
                                PODModel tmpPOD = Shared.Settings.PrintFieldForVerifyAndPrint.Find(x => x.Index == i);
                                if (tmpPOD != null)
                                {
                                    tmp += row[tmpPOD.Index - 1];
                                }
                            }
                            result.TryAdd(tmp, index);
                        }
                    }
                }
            }
            await Task.Delay(10);
            _CheckedResultCodeSet.Clear();
            return result;
        }
        public void RaiseOnReceiveVerifyDataEvent(object sender)
        {
            OnReceiveVerifyDataEvent?.Invoke(sender, EventArgs.Empty);
        }
        #endregion

        #region DataGridView
        public void InitDataGridView(DataGridView dgv, string[] columns, int imgIndex = -1, bool isPOD = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => InitDataGridView(dgv, columns, imgIndex, isPOD)));
                return;
            }

            if (columns.Length == 0) return;
            dgv.Columns.Clear();
            dgv.ScrollBars = ScrollBars.Both;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            int tableWidth = dgv.Width;
            float percentWidth = (float)1 / columns.Length;
            int tableCodeProductListWidth = dgv.Width - 39;
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
                    Size textSize = TextRenderer.MeasureText(col.HeaderText, dgv.Font);
                    col.Width = textSize.Width + 40;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv.Columns.Add(col);
                }
                else
                {
                    var col = new DataGridViewTextBoxColumn
                    {
                        HeaderText = columns[index],
                        Name = columns[index].Trim(),
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    Size textSize = TextRenderer.MeasureText(col.HeaderText, dgv.Font);
                    col.Width = textSize.Width + 25;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv.Columns.Add(col);
                }
            }

            if (isPOD)
            {
                _DatabaseImageIndex = imgIndex;
                dgv.CellValueNeeded += Database_CellValueNeeded;
            }
            else
            {
                _CheckedResulImageIndex = imgIndex;
                dgv.CellValueNeeded += CheckedResult_CellValueNeeded;
            }
            dgv.VirtualMode = true;
            dgv.RowCount = _MaxDatabaseLine;
        }

        /// <summary>
        /// For R&D Debug department
        /// </summary>
        /// <param name="correspondingIndex"></param>
        /// <returns></returns>
        private bool StopProcessWhileMissingData(int correspondingIndex)
        {
            if (Properties.Settings.Default.Username.Equals("demo") && Shared.OperStatus == OperationStatus.Running)
            {
                if (_numberPrev == 0)
                {
                    _numberPrev = correspondingIndex;
                }

                if (_numberPrev % _MaxDatabaseLine == 499) // Avoid missing indexes when switching pages
                {
                    _numberPrev++;
                }

                int sequenceNumber = correspondingIndex - _numberPrev; // The order of printing before and after must be 1
                _numberPrev = correspondingIndex;
                if (sequenceNumber > 1)
                {
                    CuzAlert.Show($"Printed data is missing", Alert.enmType.Error);
                    StopProcessAsync(false, "Printer stopped due to missing printed data.", false, true);
                    return false;
                }
                else
                {
                    return false;
                }

            }
            return false;
        }

        private void Database_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;
                if (e.RowIndex > _PrintedCodeObtainFromFile.Count - 1) return;
                int correspondingIndex = e.RowIndex + _MaxDatabaseLine * _CurrentPage;
                if (correspondingIndex > _PrintedCodeObtainFromFile.Count - 1) return;


                if (e.ColumnIndex != _DatabaseImageIndex)
                    //e.Value = _PrintedCodeObtainFromFile[correspondingIndex][e.ColumnIndex];
                    e.Value = e.ColumnIndex != 0 ? MaskData.MaskString(_PrintedCodeObtainFromFile[correspondingIndex][e.ColumnIndex])
                                                 : _PrintedCodeObtainFromFile[correspondingIndex][e.ColumnIndex];
                else
                {
                    var status = _PrintedCodeObtainFromFile[correspondingIndex][e.ColumnIndex];
                    switch (status)
                    {
                        case "Printed":
                            e.Value = Properties.Resources.icons8_done_24px_result;
                            // for R&D debug mode
                            if (Shared.Settings.PrinterList.FirstOrDefault().EnableButtonMissedStop)
                            {
                                if (StopProcessWhileMissingData(correspondingIndex))
                                {
                                    while (Shared.OperStatus == OperationStatus.Running)
                                    {
                                        Thread.Sleep(100);
                                    }
                                    return;
                                }
                            }
                            break;
                        case "Waiting":
                            e.Value = Properties.Resources.icons8_in_progress_20px_4;
                            break;
                        case "Sent":
                            e.Value = Properties.Resources.icons8_in_progress_20px_4;
                            break;
                        case "Reprint":
                            e.Value = Properties.Resources.icons8_done_24px_result;
                            break;
                        case "Duplicate":
                            (sender as DataGridView).Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Red;
                            e.Value = Properties.Resources.icon_check_241;
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        private void CheckedResult_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;
                if (e.RowIndex > _CheckedResultCodeList.Count - 1) return;
                int correspondingIndex = _CheckedResultCodeList.Count < 500 ? e.RowIndex : _CheckedResultCodeList.Count - (_MaxDatabaseLine - e.RowIndex);
                if (correspondingIndex >= _CheckedResultCodeList.Count) return;

                if (e.ColumnIndex != _CheckedResulImageIndex)
                {
                    //string value = _CheckedResultCodeList[correspondingIndex][e.ColumnIndex];
                    string value = e.ColumnIndex == Index_ResultData ? MaskData.MaskString(_CheckedResultCodeList[correspondingIndex][e.ColumnIndex]) : _CheckedResultCodeList[correspondingIndex][e.ColumnIndex];

                    e.Value = value == "" ? Lang.CannotDetect : value;
                }
                else
                {
                    switch (_CheckedResultCodeList[correspondingIndex][e.ColumnIndex])
                    {
                        case "Valid":
                            e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_done_24px_result;
                            break;
                        case "Duplicated":
                            e.Value = BarcodeVerificationSystem.Properties.Resources.icon_Duplicated_Barcode;
                            break;
                        case "Missed":
                            e.Value = BarcodeVerificationSystem.Properties.Resources.icon_Missed_Barcode;
                            break;
                        case "Null":
                            e.Value = BarcodeVerificationSystem.Properties.Resources.icon_CantDetect_Barcode;
                            break;
                        case "Invalided":
                            e.Value = BarcodeVerificationSystem.Properties.Resources.icons8_multiply_20px;
                            break;
                    }
                }
            }
            catch
            {

            }
        }
        #endregion

        #region Operation

        #region Testing
        private void DebugVirtual()
        {
            // ── Nút test Printer Disconnected/Error ─────────────────────────
            var btnTestPrinterDisconnect = new Button
            {
                Text = "Test Printer Disconnect",
                Visible = true,
                Width = 170,
                Height = 30,
                Location = new Point(330, 10),
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            Controls.Add(btnTestPrinterDisconnect);
            btnTestPrinterDisconnect.BringToFront();

            btnTestPrinterDisconnect.Click += (s, ev) =>
            {
                Console.WriteLine("[DEBUG] Simulate Printer Disconnect");

                // ── 1. Gửi PLC dừng băng tải ─────────────────────────────
                try
                {
                    if (Shared.Settings.PLCVersion == 2)
                        Shared.SensorController.Send("(C00000)");
                    else if (Shared.IsSensorControllerConnected && Shared.SensorController != null)
                        Shared.SensorController.Send("STOP");
                    Console.WriteLine("[DEBUG] PLC stop sent ✔");
                }
                catch (Exception ex) { Console.WriteLine("[DEBUG] PLC stop error: " + ex.Message); }

                // ── 2. Thông báo trên UI ──────────────────────────────────
                CuzAlert.Show(
                    "Máy in mất kết nối!\nĐã gửi lệnh dừng băng tải PLC.",
                    Alert.enmType.Error,
                    new Size(500, 120),
                    new Point(Location.X, Location.Y),
                    Size, false);

                // ── 3. Notify RLink Master (giống MON Disconnected) ───────
                _ParentForm?.NotifyDeviceException(
                    DeviceExceptionType.PrinterDisconnected,
                    "[DEBUG] Simulate printer disconnected");
            };

            // ── Nút test Camera Disconnected ────────────────────────────────
            var btnTestCameraDisconnect = new Button
            {
                Text = "Test Camera Disconnect",
                Visible = true,
                Width = 170,
                Height = 30,
                Location = new Point(510, 10),
                BackColor = Color.DarkOrange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            Controls.Add(btnTestCameraDisconnect);
            btnTestCameraDisconnect.BringToFront();

            btnTestCameraDisconnect.Click += (s, ev) =>
            {
                Console.WriteLine("[DEBUG] Simulate Camera Disconnect");

                // ── 1. Gửi PLC dừng băng tải ─────────────────────────────
                try
                {
                    if (Shared.Settings.PLCVersion == 2)
                        Shared.SensorController.Send("(C00000)");
                    else if (Shared.IsSensorControllerConnected && Shared.SensorController != null)
                        Shared.SensorController.Send("STOP");
                    Console.WriteLine("[DEBUG] PLC stop sent ✔");
                }
                catch (Exception ex) { Console.WriteLine("[DEBUG] PLC stop error: " + ex.Message); }

                // ── 2. Thông báo trên UI ──────────────────────────────────
                CuzAlert.Show(
                    "Camera mất kết nối!\nĐã gửi lệnh dừng băng tải PLC.",
                    Alert.enmType.Warning,
                    new Size(500, 120),
                    new Point(Location.X, Location.Y),
                    Size, false);

                // ── 3. Cập nhật icon status camera ────────────────────────
                // Fake ngắt kết nối camera để UpdateStatusLabelCamera hiện icon đỏ
                if (Shared.Settings.CameraList.Count > 0)
                {
                    bool prevState = Shared.Settings.CameraList[0].IsConnected;
                    Shared.Settings.CameraList[0].IsConnected = false;
                    UpdateStatusLabelCamera();
                    // Restore sau 3 giây
                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        Shared.Settings.CameraList[0].IsConnected = prevState;
                        SafeInvoke(this, () => UpdateStatusLabelCamera());
                    });
                }
            };
            var btnFakeError = new Button
            {
                Text = "Fake Error Image",
                Visible = true,
                Width = 140,
                Height = 30,
                Location = new Point(10, 10),
                BackColor = Color.OrangeRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            Controls.Add(btnFakeError);
            btnFakeError.BringToFront();

            btnFakeError.Click += (s, ev) => EnqueueFakeErrorImage();

            // ── Nút test midnight QR change ──────────────────────────────────
            var btnTestMode1Midnight = new Button
            {
                Text = "Test Mode1 Midnight",
                Visible = true,
                Width = 160,
                Height = 30,
                Location = new Point(160, 10),   // cạnh nút Fake Error Image
                BackColor = Color.DarkViolet,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            Controls.Add(btnTestMode1Midnight);
            btnTestMode1Midnight.BringToFront();

            btnTestMode1Midnight.Click += (s, ev) =>
            {
                string fakeNewQr = "TEST_QR_" + DateTime.Now.ToString("HHmmss");

                // In trạng thái TRƯỚC rebuild
                Console.WriteLine("=== TRƯỚC REBUILD ===");
                Console.WriteLine($"  _NumberOfSentPrinter = {_NumberOfSentPrinter}");
                Console.WriteLine($"  NumberPrinted        = {NumberPrinted}");
                Console.WriteLine($"  Buffer còn           = {Math.Max(0, _NumberOfSentPrinter - NumberPrinted)}");
                lock (_SyncObjCodeList)
                {
                    int w = _PrintedCodeObtainFromFile.Count(r => r[1] == "Waiting");
                    int p = _PrintedCodeObtainFromFile.Count(r => r[1] == "Printed");
                    Console.WriteLine($"  Waiting rows = {w} | Printed rows = {p}");
                    string oldQr = _PrintedCodeObtainFromFile.FirstOrDefault(r => r[1] == "Waiting")?[2] ?? "(none)";
                    Console.WriteLine($"  QR cũ = '{oldQr}'");
                }

                Console.WriteLine($"\n=== GỌI REBUILD → QR mới: {fakeNewQr} ===");
                _ = RebuildMode1DatabaseForNewDayAsync(fakeNewQr);

                // In trạng thái SAU rebuild (delay nhỏ)
                Task.Delay(500).ContinueWith(_ =>
                {
                    Console.WriteLine("\n=== SAU REBUILD ===");
                    lock (_SyncObjCodeList)
                    {
                        int w = _PrintedCodeObtainFromFile.Count(r => r[1] == "Waiting");
                        int p = _PrintedCodeObtainFromFile.Count(r => r[1] == "Printed");
                        Console.WriteLine($"  Waiting rows = {w} | Printed rows = {p}");
                        var keys = string.Join(", ", _CodeListPODFormat.Keys.Take(5));
                        Console.WriteLine($"  _CodeListPODFormat keys = [{keys}]");
                    }
                });
            };

            btnVirtualStart.Visible = true;
            btnVirtualStop.Visible = true;
            btnValid.Visible = true;
            btnInvalid.Visible = true;
            btnDuplicate.Visible = true;
            btnNull.Visible = true;

            btnVirtualStart.Click += (sender, eventArgs) =>
            {
                StartAllThreadForTesting();
            };

            btnVirtualStop.Click += (sender, eventArgs) =>
            {
                StopAllThreadForTesting();
            };

            btnValid.Click += async (sender, eventArgs) =>
            {
                await Task.Run(() => { AddValidInput(); });
            };

            btnInvalid.Click += async (sender, eventArgs) =>
            {
                await Task.Run(() => { AddInvalidInput(0); });
            };

            btnDuplicate.Click += async (sender, eventArgs) =>
            {
                await Task.Run(() => { AddInvalidInput(1); });
            };

            btnNull.Click += async (sender, eventArgs) =>
            {
                await Task.Run(() => { AddInvalidInput(2); });
            };

            // ── RSAL Alarm Simulator ──────────────────────────────────────
            var lblRsal = new Label
            {
                Text = "RSAL Test:",
                Visible = true,
                AutoSize = true,
                Location = new Point(700, 13),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            Controls.Add(lblRsal);
            lblRsal.BringToFront();

            var cboRsalCode = new ComboBox
            {
                Visible = true,
                DropDownStyle = ComboBoxStyle.DropDown,
                Width = 60,
                Height = 26,
                Location = new Point(770, 10),
                Text = "001"
            };
            cboRsalCode.Items.AddRange(new object[] { "001", "002", "003", "004", "005", "006", "007", "008", "009", "010", "011" });
            Controls.Add(cboRsalCode);
            cboRsalCode.BringToFront();

            var numRsalHead = new NumericUpDown
            {
                Visible = true,
                Width = 45,
                Height = 26,
                Location = new Point(835, 10),
                Minimum = 1,
                Maximum = 4,
                Value = 1
            };
            Controls.Add(numRsalHead);
            numRsalHead.BringToFront();

            var btnRsalOnce = new Button
            {
                Text = "RSAL Once",
                Visible = true,
                Width = 90,
                Height = 30,
                Location = new Point(885, 10),
                BackColor = Color.Teal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            Controls.Add(btnRsalOnce);
            btnRsalOnce.BringToFront();

            btnRsalOnce.Click += (s, ev) =>
            {
                string code = cboRsalCode.Text.Trim();
                int head = (int)numRsalHead.Value;
                _QueueBufferPrinterResponseData.Enqueue(new PODDataModel
                {
                    Text = $"RSAL;{code};{head}",
                    RoleOfPrinter = RoleOfStation.ForProduct
                });
                Console.WriteLine($"[DEBUG] RSAL Once: {code} head={head}");
            };

            var btnRsalSpam = new Button
            {
                Text = "RSAL Spam x5",
                Visible = true,
                Width = 100,
                Height = 30,
                Location = new Point(980, 10),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            Controls.Add(btnRsalSpam);
            btnRsalSpam.BringToFront();

            btnRsalSpam.Click += (s, ev) =>
            {
                string code = cboRsalCode.Text.Trim();
                int head = (int)numRsalHead.Value;
                Console.WriteLine($"[DEBUG] RSAL Spam: {code} head={head} x5");
                for (int i = 0; i < 5; i++)
                {
                    _QueueBufferPrinterResponseData.Enqueue(new PODDataModel
                    {
                        Text = $"RSAL;{code};{head}",
                        RoleOfPrinter = RoleOfStation.ForProduct
                    });
                }
            };
        }
        public void AddValidInput()
        {
            DetectModel dtm = new DetectModel();
            dtm.Text = "OKDC_SSTYOGHS115220";
            string[] data = new string[0];

            if (!_IsVerifyAndPrintMode)
            {
                if (_CodeListPODFormat.TryGetValue(_PrintedResponseValue, out CompareStatus compareStatus))
                {
                    if (!compareStatus.Status)
                    {
                        data = _PrintedCodeObtainFromFile[compareStatus.Index];
                    }
                }

                _PrintedResponseValue = "";
            }
            else
            {
                for (int i = 0; i < _TotalCode; i++)
                {
                    string tmp = GetCompareDataByPODFormat(_PrintedCodeObtainFromFile[i], _SelectedJob.PODFormat);
                    if (_CodeListPODFormat.TryGetValue(tmp, out CompareStatus compareStatus))
                    {
                        if (!compareStatus.Status)
                        {
                            data = _PrintedCodeObtainFromFile[i];
                            break;
                        }
                    }
                }
            }

            if (_SelectedJob.CompareType == CompareType.Database)
            {
                dtm.Text = GetCompareDataByPODFormat(data, _SelectedJob.PODFormat);
            }

            PODDataModel pod2 = new PODDataModel();
            pod2.Text = "RSFP;1/101;DATA";
            if (data != null)
            {
                for (int i = 1; i < data.Count() - 1; i++)
                {
                    pod2.Text += ";" + data[i];
                }
            }

            //    Shared.RaiseOnPrinterDataChangeEvent(pod2);

            if (_SelectedJob.CompareType != CompareType.Database)
            {
                if (_SelectedJob.CompareType == CompareType.StaticText)
                {
                    dtm.Text = _SelectedJob.StaticText;
                }
            }

            //  Shared.RaiseOnCameraReadDataChangeEvent(dtm);  
        }
        public void AddInvalidInput(int num = 0)
        {
            DetectModel dtm = new DetectModel();
            if (num == 0)
            {
                dtm.Text = "Trigger";

                string[] data = _PrintedCodeObtainFromFile.Find(x => x[0] == "Waiting");
                PODDataModel pod2 = new PODDataModel
                {
                    Text = "RSFP;1/101;DATA"
                };

                if (data != null)
                {
                    for (int i = 1; i < data.Count() - 1; i++)
                    {
                        pod2.Text += ";" + data[i];
                    }
                }

                if (_SelectedJob.CompareType != CompareType.Database)
                {
                    if (_SelectedJob.CompareType == CompareType.CanRead)
                    {
                        dtm.Text = "";
                    }
                }
            }
            else if (num == 1)
            {
                dtm.Text = _CheckedResultCodeList.Find(x => x.Length > 1 && x[1] == "Valid")[1];
            }
            else
            {
                dtm.Text = "";
            }
            //  Shared.RaiseOnCameraReadDataChangeEvent(dtm);
        }
        private void StartAllThreadForTesting()
        {
            Shared.OperStatus = OperationStatus.Processing;
            Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
            EnableUIComponent(Shared.OperStatus);

            //Save history
            string fileName = DateTime.Now.ToString(_DateTimeFormat) + "_" + _SelectedJob.FileName + ".txt";
            LoggingController.SaveHistory(
                String.Format("{0}: {1}; {2}: {3}", Lang.StartIndex, _PrintedCodeObtainFromFile.FindIndex(x => x.Last() == "Waiting"), Lang.EndIndex, _TotalCode),
                "Start testing",
                String.Format("{0}: {1}", Lang.ResultFile, fileName),
                SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                LoggingType.Started);

            // Init new token to able cancel all operatons
            _OperationCancelTokenSource = new CancellationTokenSource();
            _UICheckedResultCancelTokenSource = new CancellationTokenSource();
            _UIPrintedResponseCancelTokenSource = new CancellationTokenSource();
            _BackupImageCancelTokenSource = new CancellationTokenSource();
            _BackupResponseCancelTokenSource = new CancellationTokenSource();
            _BackupResultCancelTokenSource = new CancellationTokenSource();

            // Reset virtual list index để tránh tìm sai khi restart
            var virtualListReset = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
            virtualListReset?.ResetNextWaitingIndex();

            var operationToken = _OperationCancelTokenSource.Token;
            var uiCheckedResultToken = _UICheckedResultCancelTokenSource.Token;
            var uiPrintedResponseToken = _UIPrintedResponseCancelTokenSource.Token;
            var backupImageToken = _BackupImageCancelTokenSource.Token;
            var backupResultToken = _BackupResultCancelTokenSource.Token;
            var backupResponseToken = _BackupResponseCancelTokenSource.Token;

            if (!Shared.GetCameraStatus())
            {
                VirtualTestAsync();
            }

            CompareAsync(operationToken);

            bool shouldStartImageThread = Shared.Settings.ExportImageEnable
      || !string.IsNullOrWhiteSpace(Shared.Settings.THErrorImageFolder);
            if (shouldStartImageThread)
                ExportImageToFileAsync(backupImageToken);

            ExportCheckedResultToFileAsync(backupResultToken);
            UpdateUICheckedResultAsync(uiCheckedResultToken);

            if (_SelectedJob.CompareType == CompareType.Database)
            {
                // Khởi tạo PrintedDataProcess (SaaS/SAP sender)
                //string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
                //string printUrl = ManufacturingApis.postPrintedDataUrl();
                //if (!Directory.Exists(CommVariables.PathSentDataPrinted))
                //    Directory.CreateDirectory(CommVariables.PathSentDataPrinted);
                //try
                //{
                //    string dataPath = _SelectedJob.DirectoryDatabase;
                //    StopPrintedDataProcess(_printedDataProcess);
                //    _printedDataProcess = THTrueMilkProcessorFactory.CreatePrintingProcessor(sentDataPath, printUrl, dataPath);
                //    StartPrintedDataProcess(_printedDataProcess);
                //}
                //catch (Exception) { }

                UpdateUIPrintedResponseAsync(uiPrintedResponseToken);
                // ── Clear queue cũ để tránh zombie/null marker ──
                int cleared = 0;
                while (_QueueBufferBackupPrintedCode.Count() > 0) { _QueueBufferBackupPrintedCode.Dequeue(); cleared++; }
                if (cleared > 0) ProjectLogger.WriteDebug($"[StartProcess] Cleared {cleared} items from printed queue");
                _backupResponseTask = BackupPrintedResponseAsync(backupResponseToken);
                StartPrintedQrWorker();
            }
            Thread.Sleep(200);
            Console.WriteLine("Run all thread!");
        }
        private void StopAllThreadForTesting()
        {
            // Save history
            var fileName = "";
            if (_SelectedJob.CheckedResultPath != "")
            {
                fileName = _SelectedJob.CheckedResultPath;
            }

            LoggingController.SaveHistory(
                String.Format("{0}: {1}", Lang.TotalChecked, TotalChecked),
                "Stop testing",
                String.Format("{0}: {1}", Lang.ResultFile, fileName),
                UserController.LogedInUsername,
                LoggingType.Stopped);

            // END Stop print


            // Stop print
            if (Shared.Settings.IsPrinting)
            {
                PODController podController = Shared.Settings.PrinterList.Where(p => p.RoleOfPrinter == RoleOfStation.ForProduct).FirstOrDefault().PODController;
                if (podController != null)
                {
                    podController.Send("CLPB");
                    Thread.Sleep(5);
                    // Send command to stop printer
                    podController.Send("STOP");
                }
            }

            if (_VirtualCTS != null)
                _VirtualCTS.Cancel();
            if (_SendDataToPrinterTokenCTS != null)
                _SendDataToPrinterTokenCTS.Cancel();

            _TotalMissed = 0;

            Thread.Sleep(50);
            // Stop thread
            _UIPrintedResponseCancelTokenSource?.Cancel();
            _OperationCancelTokenSource?.Cancel();
            Thread.Sleep(50);
            while (_QueueBufferDataObtained.TryDequeue(out _)) { }
            while (_QueuePositionDataObtained.TryDequeue(out _)) { }
            _QueueBufferUpdateUIPrinter.Enqueue(null);
            _QueueBufferDataObtainedResult.Enqueue(null);
            _QueueBufferBackupCheckedResult.Enqueue(null);
            _QueueBufferBackupImage.Enqueue(null);
            _QueueBufferBackupSendLog.Enqueue(null);
            _QueueBufferBackupRSFPLog.Enqueue(null);
            _BackupResponseCancelTokenSource?.Cancel();
            _QueueBufferBackupPrintedCode.Enqueue(null);

            // ── Đợi backup tasks hoàn tất ──
            //try { if (_backupResponseTask != null) _backupResponseTask.Wait(5000); } catch { }
            //try { if (_backupSendLogTask != null) _backupSendLogTask.Wait(5000); } catch { }
            //try { if (_backupRSFPLogTask != null) _backupRSFPLogTask.Wait(5000); } catch { }
      
         

         
            try { if (_backupResponseTask != null) _backupResponseTask.Wait(10000); } catch { }
            try { if (_backupSendLogTask != null) _backupSendLogTask.Wait(10000); } catch { }
            try { if (_backupRSFPLogTask != null) _backupRSFPLogTask.Wait(10000); } catch { }
            // ── Drain printed queue race window ──
            if (!string.IsNullOrWhiteSpace(_SelectedJob?.PrintedResponePath))
            {
                string printedPath = CommVariables.PathPrintedResponse + _SelectedJob.PrintedResponePath;
                while (_QueueBufferBackupPrintedCode.Count() > 0)
                {
                    var batch = _QueueBufferBackupPrintedCode.Dequeue();
                    if (batch == null || batch.Count == 0) continue;
                    try
                    {
                        using (var sw = new System.IO.StreamWriter(printedPath, true, new System.Text.UTF8Encoding(true)))
                        {
                            foreach (var row in batch)
                                sw.WriteLine(string.Join(",", row.Select(x => Csv.Escape(x))));
                        }
                    }
                    catch { }
                }
            }

            // Drain các queue còn lại
            int drainWait = 0;
            while ((_QueueBufferUpdateUIPrinter.Count() > 0 || _QueueBufferDataObtainedResult.Count() > 0 || _QueueBufferBackupCheckedResult.Count() > 0) && drainWait < 50)
            {
                Thread.Sleep(10);
                drainWait++;
            }

            if (ProjectLabel.IsTHTrueMilk)
            {
                try
                {
                    _checkedDataProcess?.Stop();
                    StopPrintedDataProcess(_printedDataProcess);
                }
                catch (Exception)
                {
                }

            }
            Shared.OperStatus = OperationStatus.Stopped;
            Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);

            //END  Kill all thread
            Console.WriteLine("Stop all thread!");
        }
        public async void VirtualTestAsync()
        {
            _VirtualCTS = new CancellationTokenSource();
            var token = _VirtualCTS.Token;

            await Task.Run(() => { VirtualTest(token); });
        }
        private void VirtualTest(CancellationToken token)
        {
            var codes = new List<string[]>();
            lock (_SyncObjCodeList)
            {
                codes = _PrintedCodeObtainFromFile.Where(x => x.Last() == "Waiting").ToList();
            }
            if (_SelectedJob.JobType == JobType.VerifyAndPrint)
            {
                // _IsDetectWait = false;
            }

            try
            {
                if (_SelectedJob.CompareType == CompareType.Database)
                {
                    if (Shared.Settings.IsPrinting && _SelectedJob.CompareType == CompareType.Database && _SelectedJob.PrinterSeries)
                    {
                        foreach (PODController podController in Shared.Settings.PrinterList.Select(x => x.PODController))
                        {

                        }
                    }
                    else
                    {
                        Shared.OperStatus = OperationStatus.Running;
                    }

                    if (_IsVerifyAndPrintMode)
                    {
                        Thread.Sleep(3000);
                    }

                    for (int i = 0; i < codes.Count(); i++)
                    {
                        // fake
                        token.ThrowIfCancellationRequested();
                        string[] codeModel = codes[i];
                        if (codeModel.Last() == "Printed") continue;

                        PODDataModel podDataModel = new PODDataModel();
                        podDataModel.Text = "RSFP;1/101;DATA;";
                        podDataModel.Text += string.Join(Shared.Settings.SplitCharacter.ToString(), codeModel.Take(codeModel.Length - 1).Skip(1));

                        DetectModel detectModel = new DetectModel();
                        Bitmap bmp = new Bitmap(100, 100);
                        detectModel = new DetectModel();
                        detectModel.Text = _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.ProductOneQrCode
                            ? codeModel[2]
                            : GetCompareDataByPODFormat(codeModel, _SelectedJob.PODFormat);
                        detectModel.RoleOfCamera = RoleOfStation.ForProduct;

                        if (_SelectedJob.PrinterSeries)
                            _QueueBufferPrinterResponseData.Enqueue(podDataModel);
                        _QueueBufferDataObtained.Enqueue(detectModel);
                        Thread.Sleep(25);
                    }
                }
                else if (_SelectedJob.CompareType == CompareType.CanRead)
                {
                    while (Shared.OperStatus != OperationStatus.Stopped)
                    {
                        DetectModel detectModel = new DetectModel();
                        detectModel.RoleOfCamera = RoleOfStation.ForProduct;
                        detectModel.Image = new Bitmap(100, 100);
                        detectModel.Text = "Hello Worlds";

                        _QueueBufferDataObtained.Enqueue(detectModel);
                        Thread.Sleep(40);
                    }
                }
                else if (_SelectedJob.CompareType == CompareType.StaticText)
                {
                    while (Shared.OperStatus != OperationStatus.Stopped)
                    {
                        DetectModel detectModel = new DetectModel();
                        detectModel.RoleOfCamera = RoleOfStation.ForProduct;
                        detectModel.Image = new Bitmap(100, 100);
                        detectModel.Text = _SelectedJob.StaticText;
                        _QueueBufferDataObtained.Enqueue(detectModel);
                        Thread.Sleep(40);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Thread virual data was stopped!");
            }
            catch
            {
                Console.WriteLine("Thread virual data was stopped!");
                _VirtualCTS?.Cancel();
                _benchmarkCTS?.Cancel();
            }
        }
        #endregion End Testing

        #region Benchmark
        private void BenchmarkDataGenerator(CancellationToken token)
        {
            var rng = new Random();
            bool hasPrinter = _SelectedJob?.PrinterSeries == true && Shared.Settings.IsPrinting;
            _benchmarkPrintCycle.Set();
            while (!token.IsCancellationRequested)
            {
                if (hasPrinter)
                {
                    try { _benchmarkPrintCycle.WaitOne(System.Threading.Timeout.Infinite); }
                    catch { break; }
                }

                var seq = Interlocked.Increment(ref _benchmarkSeq);
                var text = $"BENCH_{DateTime.Now:HHmmssfff}_{seq:D8}";

                _QueueBufferDataObtained.Enqueue(new DetectModel
                {
                    RoleOfCamera = RoleOfStation.ForProduct,
                    Text = text,
                    Image = null,
                    CompareResult = rng.Next(10) == 0 ? ComparisonResult.Invalided : ComparisonResult.Valid
                });

                if (hasPrinter)
                    _QueueBufferPrinterResponseData.Enqueue(new PODDataModel
                    {
                        Text = "RSFP;1/101;DATA;" + text,
                        RoleOfPrinter = RoleOfStation.ForProduct
                    });

                if (rng.Next(10) == 0)
                    Task.Run(() => FireRLinkCameraError(text, "Invalided", (int)seq));
            }
        }
        #endregion

        private string CheckInitDataErrorAndGenerateMessage()
        {
            if (_InitDataErrorList.Count() > 0)
            {
                string tmp = "";
                foreach (var value in _InitDataErrorList)
                {
                    if (value == InitDataError.DatabaseUnknownError)
                        tmp += Lang.DetectError.Replace("NN", Lang.Database.ToLower()) + "\n";
                    else if (value == InitDataError.CheckedResultUnknownError)
                        tmp += Lang.DetectError.Replace("NN", Lang.CheckedResult.ToLower()) + "\n";
                    else if (value == InitDataError.PrintedStatusUnknownError)
                        tmp += Lang.DetectError.Replace("NN", Lang.PrintedResponse.ToLower()) + "\n";
                    else if (value == InitDataError.CannotAccessDatabase)
                        tmp += Lang.UnableToAccess.Replace("NN", Lang.Database.ToLower()) + "\n";
                    else if (value == InitDataError.CannotAccessCheckedResult)
                        tmp += Lang.UnableToAccess.Replace("NN", Lang.CheckedResult.ToLower()) + "\n";
                    else if (value == InitDataError.CannotAccessPrintedResponse)
                        tmp += Lang.UnableToAccess.Replace("NN", Lang.PrintedResponse.ToLower()) + "\n";
                    else if (value == InitDataError.DatabaseDoNotExist)
                        tmp += Lang.CanNotFindDatabase + "\n";
                    else if (value == InitDataError.CheckedResultDoNotExist)
                        tmp += Lang.CanNotFindCheckedResult + "\n";
                    else if (value == InitDataError.PrintedResponseDoNotExist)
                        tmp += Lang.CanNotFindPrintedResponse + "\n";
                    else
                        tmp += Lang.Unknown + "\n";
                }

                return tmp;
            }

            return "";
        }

        private CheckCondition CheckAllTheConditions()
        {
            // Check camera connection - Uncomment when release - Update later
#if !DEBUG
            //if (Shared.GetCameraStatus() == false)
            //{
            //    return CheckCondition.NotConnectCamera;
            //}
#endif
            //Check IS Dual read connection
            if (Shared.Settings.CameraList.FirstOrDefault().CameraType == CameraType.ISDual && _ParentForm.ISMultiSyncHandler != null)
            {
                if ((_ParentForm.ISMultiSyncHandler._isCam1._inSight.Connected && !_ParentForm.ISMultiSyncHandler._isCam1._inSight.Online) ||
              (_ParentForm.ISMultiSyncHandler._isCam2._inSight.Connected && !_ParentForm.ISMultiSyncHandler._isCam2._inSight.Online))
                {
                    return CheckCondition.OCRCameraIsOffline;
                }
            }


            // Check Camera IS Online
            if (Shared.Settings.CameraList.FirstOrDefault().CameraType == CameraType.IS)
            {
                if (_ParentForm.ISSingleHandler._isCam1._inSight.Connected &&
               !_ParentForm.ISSingleHandler._isCam1._inSight.Online) // Detect camera offline on IS3800
                {
                    return CheckCondition.OCRCameraIsOffline;
                }
            }


            //END Check camera connection

            // Check Camera Keyence (VS-C/CV-X)
            var camType = Shared.Settings.CameraList?.FirstOrDefault()?.CameraType;
            if (camType == CameraType.VS_C || camType == CameraType.CV_X)
            {
                if (Shared.vscCamera == null || !Shared.vscCamera.IsConnected())
                    return CheckCondition.NotConnectKeyence;
            }

            // Check PostgreSQL
            if (!string.IsNullOrWhiteSpace(Shared.Settings.THLocalDbDatabase) && !Shared.IsDatabaseConnected)
            {
                return CheckCondition.NotConnectDatabase;
            }

            // Check PLC
            if (Shared.Settings.SensorControllerEnable && !Shared.IsSensorControllerConnected)
            {
                return CheckCondition.NotConnectPlc;
            }

            if (_SelectedJob.CompareType == CompareType.Database && _SelectedJob.PrinterSeries)
            {
                // Check printer connection
                if (Shared.GetPrinterStatus() == false && Shared.Settings.IsPrinting)
                {
                    return CheckCondition.NotConnectPrinter;
                }
                // END Check printer connection

                // Check print template 
                if (_SelectedJob.CompareType == CompareType.Database && (_SelectedJob == null || _SelectedJob.TemplatePrint == ""))
                {
                    return CheckCondition.MissingParameterActivation;
                }
                // END Check print template
            }

            // Check list code for print and check
            if (_CodeListPODFormat == null || _CodeListPODFormat == null)
            {
                return CheckCondition.MissingParameterActivation;
            }
            // END Check list code for print and check

            return CheckCondition.Success;
        }

        private CheckPrinterSettings CheckAllSettingsPrinter()
        {
            if (Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings)
            {
                _PrinterSettingsModel = Shared.GetSettingsPrinter();
                if (!_PrinterSettingsModel.EnablePOD)
                {
                    return CheckPrinterSettings.PODNotEnabled;
                }

                if (!_PrinterSettingsModel.ResponsePODCommand)
                {
                    return CheckPrinterSettings.ResponsePODCommandNotEnable;
                }

                if (!_PrinterSettingsModel.ResponsePODData)
                {
                    return CheckPrinterSettings.ResponsePODDataNotEnable;
                }
                // 0: json, 1: Raw data, 2: Customise
                if (_PrinterSettingsModel.PodDataType != 1)
                {
                    return CheckPrinterSettings.NotRawData;
                }
                // 0:print all, 1:print last, 2 print last and repeat
                var podMode = _SelectedJob.JobType == JobType.AfterProduction ? 0 : 1;
                if (_PrinterSettingsModel.PodMode != podMode)
                {
                    return CheckPrinterSettings.PODMode;
                }

                if (!_PrinterSettingsModel.EnableMonitor)
                {
                    return CheckPrinterSettings.MonitorNotEnable;
                }
            }

            return CheckPrinterSettings.Success;
        }

        private bool _isStarting;
        private readonly object _startLock = new object();

        private async Task<(bool ok, string error)> ValidateQrBeforeStartAsync()
        {
            try
            {
                var job = _SelectedJob;
                if (job == null) return (false, "Chưa chọn job");

                string jobGtin = job.THJobProductGtin ?? "";
                var mode = job.THJobOperatingMode;
                double totalQr = job.NumberTotalsCode;

                // ── Tính số QR cần ──
                const int SPEED_PER_HOUR = 24000;
                int qrNeeded = 0;

                if (job.NumberOfPrintedCodes > 0)
                {
                    // Job cũ: tính QR còn lại
                    double remainingQr = totalQr - job.NumberOfPrintedCodes;
                    if (remainingQr <= 0) return (false, "Job đã in đủ số lượng");

                    if (mode == THTrueMilkOperatingMode.BatchOneQrCode || mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
                        qrNeeded = Math.Max(1, (int)Math.Ceiling(remainingQr / SPEED_PER_HOUR / 24));
                    else if (mode == THTrueMilkOperatingMode.AutoRefreshByTime)
                    {
                        double runHours = remainingQr / SPEED_PER_HOUR;
                        int nMinutes = Shared.Settings.THNMinutes > 0 ? Shared.Settings.THNMinutes : 30;
                        qrNeeded = Math.Max(1, (int)Math.Ceiling(runHours * 60 / nMinutes));
                    }
                    else
                        qrNeeded = (int)remainingQr;
                }
                else
                {
                    // Job mới: tính QR tổng
                    if (mode == THTrueMilkOperatingMode.BatchOneQrCode || mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
                        qrNeeded = Math.Max(1, (int)Math.Ceiling(totalQr / SPEED_PER_HOUR / 24));
                    else if (mode == THTrueMilkOperatingMode.AutoRefreshByTime)
                    {
                        double runHours = totalQr / SPEED_PER_HOUR;
                        int nMinutes = Shared.Settings.THNMinutes > 0 ? Shared.Settings.THNMinutes : 30;
                        qrNeeded = Math.Max(1, (int)Math.Ceiling(runHours * 60 / nMinutes));
                    }
                    else
                        qrNeeded = (int)totalQr;
                }

                // Áp dụng hệ số dự phòng
                double reserveFactor = Shared.Settings.THReserveFactor > 0 ? Shared.Settings.THReserveFactor : 1.5;
                qrNeeded = (int)Math.Ceiling(qrNeeded * reserveFactor);

                // ── Check DB có đủ QR theo GTIN không ──
                string baseUrl = Shared.Settings.THQrBaseUrl ?? "";
                int numberOfUrl = Shared.Settings.THQrNumberOfUrl;

                //NhanTo_260907_1003 - Trừ QR job đang giữ để không tính trùng khi check đủ QR
                int qrHeldByJob = await CountQrHeldByJobAsync(
                    job.FileName, jobGtin, baseUrl, numberOfUrl);
                qrNeeded = Math.Max(0, qrNeeded - qrHeldByJob);

                int availableQr = await CountValidQrByGtinAsync(jobGtin, baseUrl, numberOfUrl);
                if (availableQr < qrNeeded)
                {
                    return (false,
                        $"Không đủ QR cho job!\n" +
                        $"GTIN         : {jobGtin}\n" +
                        $"QR cần (ban đầu): {qrNeeded + qrHeldByJob}\n" +
                        $"QR đang giữ  : {qrHeldByJob}\n" +
                        $"QR cần thêm  : {qrNeeded}\n" +
                        $"QR hiện có   : {availableQr}\n" +
                        $"Thiếu        : {qrNeeded - availableQr} mã");
                }

                // ── Mode 1/2: Validate baseUrl + numberOfUrl trên 1 QR mẫu ──
                if (mode == THTrueMilkOperatingMode.BatchOneQrCode ||
                    mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange ||
                    mode == THTrueMilkOperatingMode.AutoRefreshByTime)
                {
                    string sampleQr = await GetSampleQrByGtinAsync(jobGtin);
                    if (string.IsNullOrEmpty(sampleQr))
                    {
                        return (false, "Không tìm thấy QR mẫu trong database");
                    }

                    if (!string.IsNullOrEmpty(baseUrl) && !sampleQr.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        return (false,
                            $"QR không khớp base_url!\n" +
                            $"QR: {sampleQr}\n" +
                            $"Base URL mong đợi: {baseUrl}");
                    }

                    if (numberOfUrl > 0 && sampleQr.Length != numberOfUrl)
                    {
                        return (false,
                            $"QR không khớp số lượng ký tự!\n" +
                            $"QR: {sampleQr}\n" +
                            $"Số ký tự: {sampleQr.Length} (cần: {numberOfUrl})");
                    }
                }

                // ── Validate QR trong file CSV database ──
                string csvPath = _SelectedJob?.DirectoryDatabase;
                if (!string.IsNullOrWhiteSpace(csvPath) && File.Exists(csvPath))
                {
                    try
                    {
                        var uniqueQrs = File.ReadLines(csvPath)
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .Select(line => line.Split(',')[0].Trim())
                            .Where(q => !string.IsNullOrWhiteSpace(q))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        foreach (var qr in uniqueQrs)
                        {
                            if (!string.IsNullOrEmpty(baseUrl) && !qr.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                return (false,
                                    $"QR trong file CSV không khớp base_url!\n" +
                                    $"QR: {qr}\n" +
                                    $"Base URL mong đợi: {baseUrl}");
                            }

                            if (numberOfUrl > 0 && qr.Length != numberOfUrl)
                            {
                                return (false,
                                    $"QR trong file CSV không khớp số ký tự!\n" +
                                    $"QR: {qr}\n" +
                                    $"Số ký tự: {qr.Length} (cần: {numberOfUrl})");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ProjectLogger.WriteWarning($"[ValidateQr] Không đọc được file CSV: {ex.Message}");
                    }
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[ValidateQr] Lỗi: " + ex.Message, ex);
                return (false, $"Lỗi kiểm tra QR: {ex.Message}");
            }
        }

        //NhanTo_260907_1003 - Thêm method đếm QR đang giữ bởi job để tránh tính trùng khi start
        private async Task<int> CountQrHeldByJobAsync(
            string jobName, string gtin, string baseUrl, int numberOfUrl)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(gtin) || string.IsNullOrWhiteSpace(jobName))
                        return 0;

                    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                    if (string.IsNullOrWhiteSpace(connStr))
                        connStr = frmDatabase.GetConnectionString("postgresql",
                            Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                            Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                            Shared.Settings.THLocalDbDatabase);

                    if (string.IsNullOrWhiteSpace(connStr)) return 0;

                    string table = THDb.Code;
                    string lineId = Shared.Settings.LineId ?? "";
                    int heldCount = 0;

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        RLinkLogService.EnsurePgQrBankTable(conn);

                        string sql = $"SELECT {THDb.QrCode} FROM \"{table}\" " +
                            $"WHERE {THDb.IsUsed} = TRUE " +
                            $"AND {THDb.JobName} = @job_name " +
                            $"AND {THDb.ProductGtin} = @gtin";
                        if (!string.IsNullOrWhiteSpace(lineId))
                            sql += $" AND {THDb.LineId} = @line_id";

                        using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("job_name", jobName);
                            cmd.Parameters.AddWithValue("gtin", gtin);
                            if (!string.IsNullOrWhiteSpace(lineId))
                                cmd.Parameters.AddWithValue("line_id", lineId);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string qr = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                    if (string.IsNullOrEmpty(qr)) continue;

                                    bool urlOk = string.IsNullOrEmpty(baseUrl)
                                        || qr.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase);
                                    bool lenOk = numberOfUrl <= 0 || qr.Length == numberOfUrl;

                                    if (urlOk && lenOk) heldCount++;
                                }
                            }
                        }
                    }

                    ProjectLogger.WriteInfo(
                        $"[CountHeldQr] job='{jobName}' GTIN={gtin}, QR đang giữ: {heldCount}");
                    return heldCount;
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[CountHeldQr] Lỗi: " + ex.Message, ex);
                    return 0;
                }
            });
        }

        //NhanTo_260907_1003 - Ưu tiên lấy QR job đang giữ, fallback QR chưa dùng
        private async Task<string> GetSampleQrByGtinAsync(string gtin)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                    if (string.IsNullOrWhiteSpace(connStr))
                        connStr = frmDatabase.GetConnectionString("postgresql",
                            Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                            Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                            Shared.Settings.THLocalDbDatabase);

                    if (string.IsNullOrWhiteSpace(connStr)) return "";

                    string table = THDb.Code;
                    string lineId = Shared.Settings.LineId ?? "";
                    string jobName = _SelectedJob?.FileName ?? "";

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();

                        // ── Ưu tiên: lấy QR job đang giữ (QR sẽ in) ──
                        if (!string.IsNullOrWhiteSpace(jobName))
                        {
                            string sqlHeld = $"SELECT {THDb.QrCode} FROM \"{table}\" " +
                                $"WHERE {THDb.IsUsed} = TRUE " +
                                $"AND {THDb.JobName} = @job_name " +
                                $"AND {THDb.ProductGtin} = @gtin";
                            if (!string.IsNullOrWhiteSpace(lineId))
                                sqlHeld += $" AND {THDb.LineId} = @line_id";
                            sqlHeld += $" ORDER BY {THDb.Id} DESC LIMIT 1";

                            using (var cmd = new Npgsql.NpgsqlCommand(sqlHeld, conn))
                            {
                                cmd.Parameters.AddWithValue("job_name", jobName);
                                cmd.Parameters.AddWithValue("gtin", gtin);
                                if (!string.IsNullOrWhiteSpace(lineId))
                                    cmd.Parameters.AddWithValue("line_id", lineId);

                                var heldResult = cmd.ExecuteScalar();
                                string heldQr = heldResult?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(heldQr))
                                {
                                    ProjectLogger.WriteInfo(
                                        $"[GetSampleQr] Lấy QR từ job đang giữ: '{heldQr}' (job='{jobName}')");
                                    return heldQr;
                                }
                            }
                        }

                        // ── Fallback: pool còn QR chưa dùng ──
                        string sql = $"SELECT {THDb.QrCode} FROM \"{table}\" " +
                            $"WHERE {THDb.IsUsed} = FALSE AND {THDb.ProductGtin} = @gtin";
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

        private async Task<int> CountValidQrByGtinAsync(string gtin, string baseUrl, int numberOfUrl)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(gtin)) return 0;

                    string connStr = ucProductionTHTrueMilkSetting.ActiveConnectionString;
                    if (string.IsNullOrWhiteSpace(connStr))
                        connStr = frmDatabase.GetConnectionString("postgresql",
                            Shared.Settings.THLocalDbServer, Shared.Settings.THLocalDbPort,
                            Shared.Settings.THLocalDbUsername, Shared.Settings.THLocalDbPassword,
                            Shared.Settings.THLocalDbDatabase);

                    if (string.IsNullOrWhiteSpace(connStr)) return 0;

                    string table = THDb.Code;
                    string lineId = Shared.Settings.LineId ?? "";
                    int validCount = 0;

                    using (var conn = new Npgsql.NpgsqlConnection(connStr))
                    {
                        conn.Open();
                        RLinkLogService.EnsurePgQrBankTable(conn);

                        string sql = $"SELECT {THDb.QrCode} FROM \"{table}\" WHERE {THDb.IsUsed} = FALSE AND {THDb.ProductGtin} = @gtin";
                        if (!string.IsNullOrWhiteSpace(lineId))
                            sql += $" AND {THDb.LineId} = @line_id";

                        using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("gtin", gtin);
                            if (!string.IsNullOrWhiteSpace(lineId))
                                cmd.Parameters.AddWithValue("line_id", lineId);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string qr = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                    if (string.IsNullOrEmpty(qr)) continue;

                                    bool urlOk = string.IsNullOrEmpty(baseUrl) || qr.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase);
                                    bool lenOk = numberOfUrl <= 0 || qr.Length == numberOfUrl;

                                    if (urlOk && lenOk) validCount++;
                                }
                            }
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

        private void StartProcess(bool interactOnUI = true)
        {
            lock (_startLock)
            {
                if (_isStarting || Shared.OperStatus == OperationStatus.Running || Shared.OperStatus == OperationStatus.Processing)
                    return;
                _isStarting = true;
            }
            try
            {
            countFormStopSuddenly = 0;
            _consecutiveErrorCount = 0;
            UpdateConsecutiveErrorDisplay();
            string checkInitDataMessage = "";
            checkInitDataMessage = CheckInitDataErrorAndGenerateMessage();
            if (checkInitDataMessage != "")
            {
                DialogResult dialogResult = CustomMessageBox.Show(checkInitDataMessage, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool isDatabaseDeny = _SelectedJob.CompareType == CompareType.Database && _TotalCode == 0;
            if (isDatabaseDeny)
            {
                CustomMessageBox.Show(Lang.DatabaseDoesNotExist, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            CheckCondition checkCondition = CheckAllTheConditions();  // Check all condition
            if (checkCondition != CheckCondition.Success)
            {
                if (interactOnUI)
                {
                    if (checkCondition == CheckCondition.NoJobsSelected)
                    {
                        CustomMessageBox.Show(Lang.PleaseSeletedJobForTheSystem, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    if (checkCondition == CheckCondition.NotLoadDatabase)
                    {
                        CustomMessageBox.Show(Lang.PleaseCheckTheDatabaseConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.NotLoadTemplate)
                    {
                        CustomMessageBox.Show(Lang.PleaseCheckTheTemplate, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.NotConnectCamera)
                    {
                        CustomMessageBox.Show(Lang.PleaseCheckTheCameraConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.MissingParameter)
                    {
                        CustomMessageBox.Show(Lang.SomeParametersAreMissing, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.NotConnectPrinter)
                    {
                        CustomMessageBox.Show(Lang.PleaseCheckThePrinterConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.LeastOneAction)
                    {
                        CustomMessageBox.Show(Lang.ThereMustBeAtLeastOneActionSelected, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.MissingParameterActivation)
                    {
                        CustomMessageBox.Show(Lang.SomeActivationParametersAreMissing, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.MissingParameterPrinting)
                    {
                        CustomMessageBox.Show(Lang.SomePrintParametersAreMissing, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.OCRCameraIsOffline)
                    {
                        CustomMessageBox.Show(Lang.OCRCameraIsOffline, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.NotConnectKeyence)
                    {
                        CustomMessageBox.Show("Camera Keyence chưa kết nối. Vui lòng kiểm tra kết nối camera.", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.NotConnectDatabase)
                    {
                        CustomMessageBox.Show("PostgreSQL chưa kết nối. Vui lòng kiểm tra Settings → Database.", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (checkCondition == CheckCondition.NotConnectPlc)
                    {
                        CustomMessageBox.Show("PLC (SensorController) chưa kết nối. Vui lòng kiểm tra kết nối PLC.", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                return;
            }

            bool isNeedToCheckPrinter = _SelectedJob.PrinterSeries && _SelectedJob.CompareType == CompareType.Database;
            CheckPrinterSettings checkPrinterSettings = CheckAllSettingsPrinter();

            if (checkPrinterSettings != CheckPrinterSettings.Success && isNeedToCheckPrinter) // If occur error setting printer
            {
                if (interactOnUI)
                {
                    switch (checkPrinterSettings)
                    {
                        case CheckPrinterSettings.NotRawData:
                            CustomMessageBox.Show(Lang.DataTypeMustBeRAWData, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case CheckPrinterSettings.PODNotEnabled:
                            CustomMessageBox.Show(Lang.PODFeatureIsNotEnabled, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case CheckPrinterSettings.ResponsePODDataNotEnable:
                            CustomMessageBox.Show(Lang.ResponsePODDataFeatureIsNotEnable, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case CheckPrinterSettings.ResponsePODCommandNotEnable:
                            CustomMessageBox.Show(Lang.ResponsePODCommandFeatureIsNotEnable, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case CheckPrinterSettings.MonitorNotEnable:
                            CustomMessageBox.Show(Lang.MonitorFeatureIsNotEnabled, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case CheckPrinterSettings.PODMode:
                            string mes = _SelectedJob.JobType == JobType.AfterProduction ? Lang.PODModeMustBePrintAll : Lang.PODModeMustBePrintLast;
                            CustomMessageBox.Show(mes, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            break;
                    }
                }
                return;
            }

            _ExportNamePrefix = DateTime.Now.ToString(Shared.Settings.ExportNamePrefixFormat);
            string fileName = DateTime.Now.ToString(_DateTimeFormat) + "_" + _SelectedJob.FileName + ".txt";
            int startIndex = _PrintedCodeObtainFromFile.FindIndex(x => x[0] == "Waiting") + 1;
            LoggingController.SaveHistory(
                string.Format("{0}: {1}; {2}: {3} - Job: {4}", Lang.StartIndex, startIndex, Lang.EndIndex, _TotalCode, _SelectedJob.FileName),
                Lang.Start,
                string.Format("{0}: {1}", Lang.ResultFile, fileName),
                SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                LoggingType.Started);

            // Cleaning First
            //_QueuePositionDataObtained = new ConcurrentQueue<string>();
            _queueCountFeedback = new ConcurrentQueue<int>();
            CountFeedback = 0;

            // Reset virtual list index để tránh tìm sai khi restart
            var virtualListReset = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
            virtualListReset?.ResetNextWaitingIndex();
            _numberPrev = 0; // use for detect missed print page

            if (Shared.Settings.IsPrinting && _SelectedJob.CompareType == CompareType.Database && _SelectedJob.PrinterSeries && !_IsReCheck)
            {
                foreach (PODController podController in Shared.Settings.PrinterList.Select(x => x.PODController))
                {
                    podController.Send("STOP");
                    string templateName = "";
                    if (podController.RoleOfPrinter == RoleOfStation.ForProduct)
                    {
                        templateName = _SelectedJob.TemplatePrint;
                    }
                    Thread.Sleep(300);
                    if (_PrinterStatus != PrinterStatus.Stop && Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings && isNeedToCheckPrinter)
                    {
                        CustomMessageBox.Show(Lang.ThePrinterIsInAnAbnormalStatePleaseCheckAgain + $"({_PrinterStatus.ToString().ToUpper()})", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // ── CLPB: Xóa buffer printer trước khi start mới ──
                    podController.Send("CLPB");
                    Thread.Sleep(500);

                    Shared.OperStatus = OperationStatus.Processing;
                    string startPrintCommand = string.Format("STAR" + Shared.Settings.SplitCharacter + "{0}" + Shared.Settings.SplitCharacter + 1 + Shared.Settings.SplitCharacter + 1 + Shared.Settings.SplitCharacter + "true", templateName);

                    podController.Send(startPrintCommand);
                }

                // Drain TCP buffer và app queue SAU KHI gửi STOP/STAR
                // để loại bỏ RYES phản hồi từ lệnh STOP/STAR cũ
                Thread.Sleep(200);
                foreach (PODController podController in Shared.Settings.PrinterList.Select(x => x.PODController))
                {
                    podController.ClearMessageBuffer();
                }
                _QueueBufferPrinterResponseData.Clear();
            }
            else
            {
            Shared.OperStatus = OperationStatus.Running;
                if (_SelectedJob != null && _SelectedJob.FirstRunTime == DateTime.MinValue)
                    _SelectedJob.FirstRunTime = DateTime.Now;
            }
            
            bool isNonePrinted = _SelectedJob.CompareType == CompareType.CanRead || _SelectedJob.CompareType == CompareType.StaticText;
            _SelectedJob.JobStatus = JobStatus.Unfinished;
            _SelectedJob.SaveFile();

           

            _OperationCancelTokenSource = new CancellationTokenSource();
            _UICheckedResultCancelTokenSource = new CancellationTokenSource();
            _UIPrintedResponseCancelTokenSource = new CancellationTokenSource();
            _BackupImageCancelTokenSource = new CancellationTokenSource();
            _BackupResponseCancelTokenSource = new CancellationTokenSource();
            _BackupResultCancelTokenSource = new CancellationTokenSource();
            _BackupSendLogCancelTokenSource = new CancellationTokenSource();
            _BackupRSFPLogCancelTokenSource = new CancellationTokenSource();

            // Khởi động lại handler phản hồi từ máy in (bị cancel khi stop)
            ReceiveResponseFromPrinterHandlerAsync();

            var operationToken = _OperationCancelTokenSource.Token;
            var uiCheckedResultToken = _UICheckedResultCancelTokenSource.Token;
            var uiPrintedResponseToken = _UIPrintedResponseCancelTokenSource.Token;
            var backupImageToken = _BackupImageCancelTokenSource.Token;
            var backupResultToken = _BackupResultCancelTokenSource.Token;
            var backupResponseToken = _BackupResponseCancelTokenSource.Token;
            var backupSendLogToken = _BackupSendLogCancelTokenSource.Token;
            var backupRSFPLogToken = _BackupRSFPLogCancelTokenSource.Token;

            _backupSendLogTask = BackupSendLogAsync(backupSendLogToken);

            CompareAsync(operationToken);

            bool shouldStartImageThread = Shared.Settings.ExportImageEnable
      || !string.IsNullOrWhiteSpace(Shared.Settings.THErrorImageFolder);
            if (shouldStartImageThread)
                ExportImageToFileAsync(backupImageToken);

            ExportCheckedResultToFileAsync(backupResultToken);
            UpdateUICheckedResultAsync(uiCheckedResultToken);

            if (_SelectedJob.CompareType == CompareType.Database)
            {
                // Khởi tạo PrintedDataProcess (SaaS/SAP sender)
                //string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
                //string printUrl = ManufacturingApis.postPrintedDataUrl();
                //if (!Directory.Exists(CommVariables.PathSentDataPrinted))
                //    Directory.CreateDirectory(CommVariables.PathSentDataPrinted);
                //try
                //{
                //    string dataPath = _SelectedJob.DirectoryDatabase;
                //    StopPrintedDataProcess(_printedDataProcess);
                //    _printedDataProcess = THTrueMilkProcessorFactory.CreatePrintingProcessor(sentDataPath, printUrl, dataPath);
                //    StartPrintedDataProcess(_printedDataProcess);
                //}
                //catch (Exception) { }

                UpdateUIPrintedResponseAsync(uiPrintedResponseToken);
                // ── Cleanup zombie consumer trước khi tạo consumer mới ──
                try { if (_backupResponseTask != null) _backupResponseTask.Wait(10000); } catch { }
                // ── Clear queue cũ để tránh zombie/null marker ──
                int cleared = 0;
                while (_QueueBufferBackupPrintedCode.Count() > 0) { _QueueBufferBackupPrintedCode.Dequeue(); cleared++; }
                if (cleared > 0) ProjectLogger.WriteDebug($"[ResumeProcess] Cleared {cleared} items from printed queue");
                // ── Tạo consumer mới (token CHƯA bị cancel) ──
                _backupResponseTask = BackupPrintedResponseAsync(backupResponseToken);
                _backupRSFPLogTask = BackupResultFinishPrintCommandAsync(backupRSFPLogToken);
                StartPrintedQrWorker();
            }

            Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
            EnableUIComponent(Shared.OperStatus);
            btnCompleteJob.Enabled = false;
            // ── Lưu job đang chạy vào settings ──────────────────────────
            Shared.Settings.LastActiveJobName = Shared.JobNameSelected;
            Shared.SaveSettings();

            // ── RLink log: start ─────────────────────────────────────
            FireRLinkLog("start");
            StartRunLogTimer();
            CheckCodeOutOfThreshold(backupResponseToken);
     
            // ── Benchmark mode ──────────────────────────────────────
            if (_benchmarkMode)
            {
                _benchmarkCTS = CancellationTokenSource.CreateLinkedTokenSource(operationToken);
                var bmToken = _benchmarkCTS.Token;
                Task.Run(() => BenchmarkDataGenerator(bmToken), bmToken);
            }

            //SendParametersToServerAsync(uiPrintedResponseToken);
            //AutoTriggerCamera(uiPrintedResponseToken);
            //  _ParentForm.ISCamera.StartGetData(); // Start get data from OCR camera
            }
            finally
            {
                _isStarting = false;
                if (Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)
                    btnCompleteJob.Enabled = true;

            }
        }

        private async void CheckCodeOutOfThreshold(CancellationToken token)
        {
            await Task.Run(async () => { await CheckPrintedCodeThreshold(token); });

        }

        private async Task CheckPrintedCodeThreshold(CancellationToken token)
        {

            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested(); // Stop thread

                    var payload = Shared.CurrentJob?.DispatchingOrderPayload.payload;
                    //var CheckCodeAmount = new RequestCheckCodeAmount()
                    //{
                    //    job_name = Shared.CurrentJob?.FileName,
                    //    wave_key = payload?.wave_key,
                    //    wms_number = payload?.wms_number,
                    //    material_number = payload?.items[Shared.CurrentJob.SelectedMaterialIndex].material_number,
                    //    username = CurrentUser.UserCode
                    //};
                    //var currentPrintedCodeInfo = await apiService.PostApiDataAsync<ResponseCurrentPrintedCodeInfo>(url, CheckCodeAmount); // GetPrintedAmountDataAsync
                    //var currentPrintedCodeInfo = await ManufacturingService.GetPrintedAmountDataAsync(CheckCodeAmount);

                    //if (!currentPrintedCodeInfo.is_success)
                    //{
                    //    ProjectLogger.WriteError($"Error occurred in GetPrintedAmountDataAsync" + currentPrintedCodeInfo.message + " Payload:" + CheckCodeAmount.ToString());
                    //}
                    //if (currentPrintedCodeInfo.is_exceed)
                    //{
                    //    CustomMessageBox.Show("B?n có mu?n dùng chương tr?nh ? \n S? lư?ng m? đ? in vư?t ngư?ng" +
                    //        "\n " + $"S? lư?ng m? đ? in : {currentPrintedCodeInfo.amount}", "M? vư?t ngư?ng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //}
                }
                catch (Exception ex)
                {
                    //ProjectLogger.WriteError($"Error occurred in GetPrintedAmountDataAsync" + ex.Message);
                }
                await Task.Delay(2000);
            }
        }


        private async void AutoTriggerCamera(CancellationToken token)
        {
            await Task.Run(async () => { await Shared_OnCameraTriggerOnChange(token); });

        }
        int count = 0;
        public async Task Shared_OnCameraTriggerOnChange(CancellationToken token)
        {
            while (true)
            {
                // auto trigger cam
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested(); // Stop thread
                //count++;
                //DetectModel detectModel = new DetectModel()
                //{
                //    Text = "Number_" + count.ToString(),
                //};
                ////_QueuePositionDataObtained.Enqueue($"#123#123#true#");
                //_QueuePositionDataObtained.Enqueue($"#1");

                //_QueueBufferDataObtained.Enqueue(detectModel);
                _ParentForm.Shared_OnCameraTriggerOnChange(null, null);
                await Task.Delay(100);
            }

        }

        public bool IsBarcodeWithinThreshold(
          double barcodeX, double barcodeY, double barcodeWidthPx, double barcodeHeightPx,
          double fixedX, double fixedY, int actualBarcodeWidthMm, int actualBarcodeHeightMm,
          double angleDegrees)
        {
            // Convert angle to radians
            double angleRadians = angleDegrees * Math.PI / 180.0;
            double cosAngle = Math.Cos(angleRadians);
            double sinAngle = Math.Sin(angleRadians);

            // Compute rotated bounding box dimensions
            double effectiveWidthPx = Math.Abs(barcodeWidthPx * cosAngle) + Math.Abs(barcodeHeightPx * sinAngle);
            double effectiveHeightPx = Math.Abs(barcodeWidthPx * sinAngle) + Math.Abs(barcodeHeightPx * cosAngle);

            double effectivebarcodeX = Math.Abs(barcodeX * cosAngle) + Math.Abs(barcodeY * sinAngle);
            double effectivebarcodeY = Math.Abs(barcodeX * sinAngle) + Math.Abs(barcodeY * cosAngle);

            double effectivefixedX = Math.Abs(fixedX * cosAngle) + Math.Abs(fixedY * sinAngle);
            double effectivefixedY = Math.Abs(fixedX * sinAngle) + Math.Abs(fixedY * cosAngle);

            // Recalculate pixel-to-mm conversion ratios based on rotated dimensions
            double pxToMmX = actualBarcodeWidthMm / effectiveWidthPx;  // mm per pixel (x-axis)
            double pxToMmY = actualBarcodeHeightMm / effectiveHeightPx; // mm per pixel (y-axis)

            PixelToMmX = pxToMmX;
            PixelToMmY = pxToMmY;

            double barcodeXMm = effectivebarcodeX * pxToMmX;
            double barcodeYMm = effectivebarcodeY * pxToMmY;

            double fixedXMm = effectivefixedX * pxToMmX;
            double fixedYMm = effectivefixedY * pxToMmY;

            double diffX = Math.Abs(barcodeXMm - fixedXMm);
            double diffY = Math.Abs(barcodeYMm - fixedYMm);

            double threshold = Shared.Settings.CameraList.FirstOrDefault()?.Threshold ?? 0.5;
            return (diffX <= threshold && diffY <= threshold);
        }

        private async void CompareAsync(CancellationToken token)
        {
            await Task.Run(() => Compare(token));
        }

        private static long lastReceivedTime = 0;
        private static long currentReceivedTime = 0;

        static long GetCurrentTimeInMilliseconds()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return now.ToUnixTimeMilliseconds();
        }

        private void Compare(CancellationToken token)
        {
            Debug.WriteLine("Compare thread working on thread " + Environment.CurrentManagedThreadId);
            int currentCheckedIndex = -1;
            string staticText = "";


            bool isAutoComplete = _SelectedJob.CompareType == CompareType.Database;
            bool isReprint = _SelectedJob.CompareType == CompareType.Database &&
                            _SelectedJob.JobType == JobType.AfterProduction &&
                            _TotalMissed > 0 &&
                            Shared.Settings.TotalCheckEnable;
            bool isDBStandalone = _SelectedJob.CompareType == CompareType.Database &&
                                 _SelectedJob.JobType == JobType.StandAlone;
            int reprintStopCond = TotalChecked + _TotalMissed - _NumberOfDuplicate;
            int stopCond = _dynamicStopCond;
            startIndex = TotalChecked;
            int startCommandIndex = TotalChecked;
            string formattedIndex = startCommandIndex.ToString("D7");

            if (Shared.Settings.CameraList.FirstOrDefault()?.IsIndexCommandEnable == true)
            {
                Shared.SensorController.Send("0" + formattedIndex);
            }

            if (Shared.Settings.PLCVersion == 2)
            {
                Shared.SensorController.Send("(C00001)");
            }

            bool isPosition = Shared.Settings.EnablePosition &&
                             Shared.Settings.CameraList.FirstOrDefault()?.CameraType == CameraType.IS;
            bool isBarcodePosition = Shared.Settings.Position == SettingsModel.PositionType.BarcodePosition;

            //isPosition = true;
            //isBarcodePosition = false;

            // Initialize the appropriate position handler
            IPositionHandler positionHandler = CreatePositionHandler(isPosition, isBarcodePosition);

            if (itemsPerHour != null && !itemsPerHour.IsDisposed)
            {
                if (itemsPerHour.InvokeRequired)
                {
                    itemsPerHour.Invoke(new MethodInvoker(delegate
                    {
                        itemsPerHour.Visible = Shared.Settings.IsItemsPerHour;
                    }));
                }
                else
                {
                    itemsPerHour.Visible = Shared.Settings.IsItemsPerHour;
                }
            }

            bool isOneMore = false;
            bool isComplete = false;

            try
            {
                while (true)
                {
                    if (positionHandler.ShouldThrowCancellation(token))
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    if (isComplete)
                    {
                        continue;
                    }

                    if (isAutoComplete)
                    {
                        bool completeCondition = false;
                        int stopNumber = Shared.Settings.TotalCheckEnable ? TotalChecked : NumberOfCheckPassed;

                        if (_SelectedJob.JobType == JobType.VerifyAndPrint)
                        {
                            if (!Shared.Settings.TotalCheckEnable)
                            {
                                if (isOneMore)
                                {
                                    stopNumber++;
                                }
                                if (NumberOfCheckPassed >= _TotalCode - _NumberOfDuplicate)
                                {
                                    isOneMore = true;
                                }
                            }
                            stopNumber--;
                        }

                        completeCondition = isReprint
                            ? Shared.OperStatus != OperationStatus.Stopped && stopNumber >= reprintStopCond
                            : Shared.OperStatus != OperationStatus.Stopped && stopNumber >= stopCond;

                        if (completeCondition && !_IsReCheck && !Shared.Settings.AllowDupAndNonStop)
                        {
                             BeginInvoke(new Action(() => StopProcessAsync(false, Lang.CompleteTheBarcodeVerificationProcess, true)));
                            isComplete = true;
                            continue;
                        }
                    }

                    // Process queue data using the position handler
                    if (!positionHandler.TryDequeueData(out DetectModel detectModel, out string positionData))
                    {
                        continue;
                    }

                    Console.WriteLine($"[CMP] Dequeued: qr={detectModel?.Text}");

                    if (Shared.Settings.IsItemsPerHour)
                    {
                        currentReceivedTime = GetCurrentTimeInMilliseconds() - lastReceivedTime;
                        double rate = (1000.0 / currentReceivedTime) * 3600;
                        string text = rate.ToString("F0") + " " + Lang.ItemsPerHour;
                        if (itemsPerHour.InvokeRequired)
                            itemsPerHour.Invoke(new Action(() => { itemsPerHour.Text = text; itemsPerHour.Visible = true; }));
                        else { itemsPerHour.Text = text; itemsPerHour.Visible = true; }
                        lastReceivedTime = GetCurrentTimeInMilliseconds();
                    }

                    // Process position data
                    positionHandler.ProcessPositionData(detectModel, positionData);
                    bool isPositionCorrect = detectModel.isBarcodeWithinThreshold.Contains("True");

                    var measureTime = Stopwatch.StartNew();
                    int compareIndex = _StartIndex + TotalChecked;

                    if (detectModel != null)
                    {
                        // Console.WriteLine("Du lieu camera co duoc: " + ++CountDataRev);

                        // Check camera read/OCR result — if either failed, skip comparison
                        string qrRes = "", ocrRes = "";
                        detectModel.ExtraFields?.TryGetValue("QR_RESULT", out qrRes);
                        detectModel.ExtraFields?.TryGetValue("OCR_RESULT", out ocrRes);
                        bool qrPass = (qrRes == "OK");
                        bool ocrPass = (ocrRes == "OK");
                        bool cameraPass = qrPass && ocrPass;

                        _uiUpdatePending = true;

                        // [MỚI] Nếu date check passed > (đã in + buffer) → Failed mặc định, skip so sánh QR
                        if (ocrPass && _DateCheckPassed > (TotalRlinkPrinted + _printerBuffer))
                        {
                            detectModel.CompareResult = ComparisonResult.Invalided;
                        }
                        else if (!cameraPass)
                        {
                            detectModel.CompareResult = ComparisonResult.Invalided;
                        }
                        else if (_SelectedJob.CompareType == CompareType.CanRead)
                        {
                            detectModel.CompareResult = CanreadCompare(detectModel.Text);
                        }
                        else if (_SelectedJob.CompareType == CompareType.StaticText)
                        {
                            detectModel.CompareResult = StaticTextCompare(detectModel.Text, staticText);
                        }
                        else if (_SelectedJob.CompareType == CompareType.Database)
                        {
                            bool isRepeatedQrMode =
                                   _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                                || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                                || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime
                                || Shared.Settings.THOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                                || Shared.Settings.THOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                                || Shared.Settings.THOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;

                            // DEBUG: in ra để chắc chắn
                            Debug.WriteLine($"[Compare] THJobMode={_SelectedJob.THJobOperatingMode}, SharedMode={Shared.Settings.THOperatingMode}, isRepeated={isRepeatedQrMode}, _IsOnProd={_IsOnProductionMode}, _IsPrintedRes={_IsPrintedResponse}");

                            bool isNeedToCheckPrintedResponse = true;
                            // Mode 1/2: QR lặp lại → KHÔNG đồng bộ RSFP-camera
                            if (_IsOnProductionMode && !isRepeatedQrMode)
                            {
                                lock (_PrintedResponseLocker)
                                {
                                    isNeedToCheckPrintedResponse = _IsPrintedResponse;
                                    _IsPrintedResponse = false;
                                }
                            }

                            // Mode 1/2: bỏ qua early-return path luôn
                            bool earlyInvalid = (!isNeedToCheckPrintedResponse || _CodeListPODFormat == null) && !_IsReCheck;
                            if (earlyInvalid && !isRepeatedQrMode)
                            {
                                detectModel.CompareResult = ComparisonResult.Invalided;
                            }
                            else
                            {
                                detectModel.CompareResult = isPosition && !isPositionCorrect
                                    ? ComparisonResult.Invalided
                                    : DatabaseCompare(detectModel.Text, ref currentCheckedIndex);
                            }

                            // LUÔN pulse _CheckLocker — producer không bị block
                            if (_IsOnProductionMode)
                            {
                                lock (_CheckLocker)
                                {
                                    _CheckedResult = detectModel.CompareResult;
                                    _IsCheckedWait = false;
                                    Monitor.PulseAll(_CheckLocker);
                                }
                            }

                            if (_IsVerifyAndPrintMode)
                            {
                                bool verifyAndPrintCondition = Shared.GetPrinterStatus();
                                if (verifyAndPrintCondition)
                                {
                                    string[] arr = ProcessVerifyAndPrint(detectModel, currentCheckedIndex);
                                    RaiseOnReceiveVerifyDataEvent(arr);
                                    currentCheckedIndex = -1;
                                }
                            }
                        }

                        if (isPosition && !isPositionCorrect)
                        {
                            detectModel.CompareResult = ComparisonResult.Invalided;
                        }

                        // Đếm DateCheckPassed/Failed — dựa vào OCR + QR + CompareResult
                        if (ocrPass)
                        {
                            if (qrPass && detectModel.CompareResult != ComparisonResult.Valid)
                            {
                                // QR=true, OCR=true, so sánh SAI → DateCheckFailed
                                Interlocked.Increment(ref _DateCheckFailed);
                            }
                            else
                            {
                                // OCR=true, (QR=false hoặc so sánh OK) → DateCheckPassed
                                Interlocked.Increment(ref _DateCheckPassed);
                            }
                        }
                        else
                        {
                            // OCR=false → DateCheckFailed
                            Interlocked.Increment(ref _DateCheckFailed);
                        }

                        //bool isOutputAllowed = !(isPosition && isBarcodePosition && (detectModel.Text == "" || !isBarcodeWithinThreshold));
                        string t = positionHandler.Name;
                        bool isOutputAllowed = positionHandler.ShouldAllowOutput(detectModel, isPositionCorrect);

                        if (Shared.Settings.OutputEnable && isOutputAllowed && Shared.GetCameraStatus())
                        {
                            Shared.RaiseOnCameraOutputSignalChangeEvent(compareIndex);
                        }

                        measureTime.Stop();
                        TotalChecked++;
                        if (detectModel.CompareResult == ComparisonResult.Valid)
                        {
                            NumberOfCheckPassed++;

                            if (Shared.Settings.PLCVersion == 2)
                            {
                                Shared.SensorController.Send("(C00003)");
                            }
                        }
                        else
                        {
                            NumberOfCheckFailed++;
                        }

                        detectModel.Index = compareIndex;
                        detectModel.CompareTime = measureTime.Elapsed.TotalMilliseconds;
                        detectModel.ProcessingDateTime = DateTime.Now.ToString(Shared.Settings.DateTimeFormatOfResult);

                        if (isDBStandalone && detectModel.CompareResult == ComparisonResult.Valid)
                        {
                            _QueueBufferUpdateUIPrinter.Enqueue(new Model.THTrueMilk.PrinterResponseData { Data = detectModel.Text });
                        }

                        startIndex = TotalChecked;
                        _QueueBufferDataObtainedResult.Enqueue(detectModel);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _UICheckedResultCancelTokenSource?.Cancel();
                _QueueBufferDataObtainedResult.Enqueue(null);
                Thread.Sleep(200);
                UpdateStopUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Thread compare was error!");
                Thread.Sleep(200);
                StopProcessAsync(false, Lang.HandleError, false, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        private IPositionHandler CreatePositionHandler(bool isPosition, bool isBarcodePosition)
        {
            if (!isPosition)
            {
                return new NoPositionHandler(_QueueBufferDataObtained);
            }
            else if (isBarcodePosition)
            {
                return new BarcodePositionHandler(_QueuePositionDataObtained);
            }
            else
            {
                return new LogoPositionHandler(_QueueBufferDataObtained, _QueuePositionDataObtained);
            }
        }

        private string[] ProcessVerifyAndPrint(DetectModel detectModel, int currentCheckedIndex)
        {
            string[] arr;
            if (detectModel.CompareResult == ComparisonResult.Valid)
            {
                if (!Shared.Settings.VerifyAndPrintBasicSentMethod)
                {
                    lock (_SyncObjCodeList)
                    {
                        arr = _PrintedCodeObtainFromFile[currentCheckedIndex];
                    }
                }
                else
                {
                    arr = new string[3];
                    arr[1] = detectModel.Text;
                }
            }
            else
            {
                arr = !Shared.Settings.VerifyAndPrintBasicSentMethod
                    ? new string[_PrintedCodeObtainFromFile[0].Length]
                    : new string[3];
            }
            return arr;
        }
        private ComparisonResult CanreadCompare(string txt)
        {
            // Verify data canread job
            if (txt == Shared.Settings.CameraList[0].NoReadOutputString || txt == "")
            {
                return ComparisonResult.Invalided;
            }
            else
            {
                return ComparisonResult.Valid;
            }
        }

        private ComparisonResult StaticTextCompare(string txt, string staticText)
        {
            // Verify data static text
            if (staticText != "" && txt == staticText)
            {
                return ComparisonResult.Valid;
            }
            else
            {
                return ComparisonResult.Invalided;
            }
        }

        private ComparisonResult DatabaseCompare(string txt, ref int currentValidIndex)
        {
            var checkNull = txt == "";
            if (checkNull) return ComparisonResult.Null;

            // ── Mode 1 (BatchOneQrCode) & Mode 2 (AutoRefreshByTime):
            //    QR lặp lại — KHÔNG dùng PODFormat dict (chỉ có 1 QR duy nhất).
            //    So sánh trực tiếp O(1) thay vì O(n) trên toàn bộ database.
            bool isRepeatedQrMode =
                   _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;

            if (isRepeatedQrMode)
            {
                // ── Mode 1/2: dùng dict _CodeListPODFormat — QR nào có trong dict → Valid ──
                if (_CodeListPODFormat != null && _CodeListPODFormat.TryGetValue(txt, out CompareStatus cs))
                {
                    currentValidIndex = cs.Index;
                    Debug.WriteLine($"[DBCompare] Mode1/2 ✔ QR='{txt}' | idx={currentValidIndex} | dict");
                    return ComparisonResult.Valid;
                }

                // ── Fallback nếu dict chưa có (cold start, VirtualList chưa init dict) ──
                var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                if (virtualList != null)
                {
                    for (int i = 0; i < virtualList.Count; i++)
                    {
                        if (txt == virtualList.GetQrCode(i))
                        {
                            currentValidIndex = i;
                            Debug.WriteLine($"[DBCompare] Mode1/2 ✔ QR='{txt}' | idx={currentValidIndex} | fallback scan");
                            return ComparisonResult.Valid;
                        }
                    }
                }

                Debug.WriteLine($"[DBCompare] Mode1/2 ✘ QR '{txt}' không có trong dict → Invalided");
                return ComparisonResult.Invalided;
            }

            // ── Mode khác: logic gốc (dict + duplicate check) ──
            if (_CodeListPODFormat == null) return ComparisonResult.Invalided;
            if (!_CodeListPODFormat.TryGetValue(txt, out CompareStatus compareStatus))
                return ComparisonResult.Invalided;

            if (compareStatus.Index == -1)
            {
                var list = _PrintedCodeObtainFromFile as List<string[]>;
                if (list != null)
                    compareStatus.Index = list.FindIndex(x =>
                        GetCompareDataByPODFormat(x, _SelectedJob.PODFormat) == txt);
            }

            if (!compareStatus.Status)
            {
                _CodeListPODFormat[txt].Status = true;
                currentValidIndex = compareStatus.Index;
                return ComparisonResult.Valid;
            }

            if (Shared.Settings.AllowDupAndNonStop)
            {
                _CodeListPODFormat[txt].Status = true;
                currentValidIndex = compareStatus.Index;
                return ComparisonResult.Valid;
            }
            return ComparisonResult.Duplicated;
        }

        private async void UpdateUIPrintedResponseAsync(CancellationToken token)
        {
            await Task.Run(() => { UpdateUIPrintedResponse(token); });
        }

        private void UpdateUIPrintedResponse(CancellationToken token)
        {
            Debug.WriteLine("UI 1 thread working on thread " + Environment.CurrentManagedThreadId);
            List<string[]> strPrintedResponseList = new List<string[]>();
            var isAutoComplete = _SelectedJob.CompareType == CompareType.Database; // Check if need to auto stop procces when compare type is verify and print
            int numOfResponse = NumberPrinted;
            int currentIndex = 0;
            var currentPage = 0;
            int batchCounter = 0;
            try
            {
                while (true)
                {
                    // Only stop if handled all data
                    if (token.IsCancellationRequested)
                        if (_QueueBufferUpdateUIPrinter.Count() == 0)
                            token.ThrowIfCancellationRequested(); // Stop thread

                    // Waiting until have data
                    var responseData = _QueueBufferUpdateUIPrinter.Dequeue();
                    if (responseData == null || responseData.Data == null) continue;

                    string podCommand = responseData.Data;
                    {
                        if (_IsOnProductionMode) // Check if need to wait check result
                        {
                            ComparisonResult checkedResult = ComparisonResult.Null;
                            lock (_CheckLocker)
                            {
                                while (_IsCheckedWait) Monitor.Wait(_CheckLocker); // Waiting until detect data was verify
                                checkedResult = _CheckedResult;
                                _IsCheckedWait = true;
                                Monitor.PulseAll(_CheckLocker);  // Pulse sau khi đọc để Compare không bị block
                            }

                            lock (_PrintLocker) // Notify that code is printed
                            {
                                _IsPrintedWait = false;
                                _PrintedResult = checkedResult;
                                Monitor.PulseAll(_PrintLocker);
                            }

                            if (checkedResult != ComparisonResult.Valid)
                            {
                               // ProjectLogger.WriteDebug($"[Producer] Camera check skip: result={checkedResult}");
                                continue;
                            }
                        }
                        // Update printed status
                        bool isMode1Or2 = _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                                         || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                                         || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;

                        if (isMode1Or2)
                        {
                            // ── Mode 1/2: FIFO + so sánh dữ liệu (không dùng RSFP index) ──
                            int nextIdx = -1;
                            var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;

                            if (virtualList != null)
                            {
                                nextIdx = virtualList.FindNextWaiting();
                                if (nextIdx >= 0)
                                {
                                    string expected = string.Join("", virtualList[nextIdx].Skip(2).Take(3));
                                    if (!string.IsNullOrEmpty(podCommand) && expected == podCommand)
                                    {
                                        virtualList.SetPrinted(nextIdx, DateTime.Now.ToString("dd MM yy HH:mm:ss"));
                                        Interlocked.Increment(ref _sessionPrintedCount);
                                        string[] row = virtualList[nextIdx];
                                        row[1] = "Printed";
                                        strPrintedResponseList.Add(row);

                                        string qrVal1 = row.Length > 2 ? row[2] : "";
                                        string prod1 = row.Length > 3 ? row[3] : "";
                                        string exp1 = row.Length > 4 ? row[4] : "";
                                        string pid1 = _SelectedJob?.THJobProductId ?? "";
                                        string batch1 = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(prod1, Shared.Settings?.LineId ?? "");
                                        if (!string.IsNullOrEmpty(qrVal1) && !RLinkLogService.IsQrSentToMaster(qrVal1))
                                        {
                                            var item = (qrVal1, pid1, prod1, exp1, batch1);
                                            RLinkLogService.MarkQrAsPrintedBatch(new List<(string, string, string, string, string)> { item });
                                            RLinkLogService.MarkQrAsPrintedBatchPg(new List<(string, string, string, string, string)> { item });
                                        }
                                    }
                                    else
                                    {
                                        ProjectLogger.WriteDebug($"[Producer] Mode1/2 Match fail: expected='{expected}' podCommand='{podCommand}'");
                                        continue;
                                    }
                                }
                                else
                                {
                                   // ProjectLogger.WriteDebug("[Producer] Mode1/2 FindNextWaiting = -1, all rows printed");
                                    continue;
                                }
                            }
                            else
                            {
                                var list = _PrintedCodeObtainFromFile as List<string[]>;
                                if (list != null)
                                {
                                    lock (_SyncObjCodeList)
                                    {
                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            if (list[i][1] == "Waiting")
                                            {
                                                nextIdx = i;
                                                break;
                                            }
                                        }
                                        if (nextIdx >= 0)
                                        {
                                            string[] row = list[nextIdx];
                                            string expected = string.Join("", row.Skip(2).Take(3));
                                            if (!string.IsNullOrEmpty(podCommand) && expected == podCommand)
                                            {
                                                list[nextIdx][1] = "Printed";
                                                list[nextIdx][list[nextIdx].Length - 1] = DateTime.Now.ToString("dd MM yy HH:mm:ss");
                                                strPrintedResponseList.Add(list[nextIdx]);

                                                string qrVal2 = list[nextIdx].Length > 2 ? list[nextIdx][2] : "";
                                                string prod2 = list[nextIdx].Length > 3 ? list[nextIdx][3] : "";
                                                string exp2 = list[nextIdx].Length > 4 ? list[nextIdx][4] : "";
                                                string pid2 = _SelectedJob?.THJobProductId ?? "";
                                                string batch2 = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(prod2, Shared.Settings?.LineId ?? "");
                                                if (!string.IsNullOrEmpty(qrVal2) && !RLinkLogService.IsQrSentToMaster(qrVal2))
                                                {
                                                    var item = (qrVal2, pid2, prod2, exp2, batch2);
                                                    RLinkLogService.MarkQrAsPrintedBatch(new List<(string, string, string, string, string)> { item });
                                                    RLinkLogService.MarkQrAsPrintedBatchPg(new List<(string, string, string, string, string)> { item });
                                                }
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }

                            // Update current code position
                            currentIndex = nextIdx % _MaxDatabaseLine;
                            currentPage = nextIdx / _MaxDatabaseLine;

                            if (currentPage != _CurrentPage)
                            {
                                _CurrentPage = currentPage;
                            }
                            {
                                int rowIdx = currentIndex;
                                BeginInvoke(new Action(() =>
                                {
                                    if (rowIdx >= 0 && rowIdx < dgvDatabase.Rows.Count)
                                    {
                                        dgvDatabase.InvalidateRow(rowIdx);
                                        dgvDatabase.ClearSelection();
                                        dgvDatabase.Rows[rowIdx].Cells[0].Selected = true;
                                        int visibleRows = dgvDatabase.DisplayedRowCount(true);
                                        dgvDatabase.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIdx - visibleRows + 2);
                                    }
                                }));
                            }
                            batchCounter++;

                            // Ghi CSV qua background queue
                            if (strPrintedResponseList.Count > 0)
                            {
                                _QueueBufferBackupPrintedCode.Enqueue(new List<string[]>(strPrintedResponseList));
                                //WritePrintedDebugLog($"Producer Enqueued {strPrintedResponseList.Count} items, queue={_QueueBufferBackupPrintedCode.Count()}, NumberPrinted={NumberPrinted}");
                            }
                            strPrintedResponseList.Clear();

                            // ── Chỉ tăng NumberPrinted nội bộ khi KHÔNG phải Mode 1/2/4
                            // Mode 1/2/4: dùng giá trị tuyệt đối từ RSFP (TCP thread đã set)
                            bool isMode124Now = _SelectedJob?.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                                || _SelectedJob?.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                                || _SelectedJob?.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;
                            if (!isMode124Now)
                            {
                                NumberPrinted = Interlocked.Increment(ref _NumberPrinted);
                                Shared.NumberPrinted = NumberPrinted;
                            }

                            var vlUpdate = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                            long rlinkPrinted = (vlUpdate != null)
                                ? vlUpdate.GetPrintedCount()
                                : (_PrintedCodeObtainFromFile as System.Collections.Generic.List<string[]>)?.Count(r => r[1] == "Printed") ?? 0;
                            if (_SelectedJob != null) _SelectedJob.TotalRlinkPrinted = rlinkPrinted;
                            //BeginInvoke(new Action(() => { lblLastPrintedPage.Text = string.Format("{0:N0}", rlinkPrinted); }));
                        }
                        else if (_CodeListPODFormat.TryGetValue(podCommand, out CompareStatus compareStatus))
                        {
                            // ── Mode 3 / logic gốc ──
                            if (_PrintedCodeObtainFromFile[compareStatus.Index][1] == "Waiting")
                            {
                                    lock (_SyncObjCodeList)
                                    {
                                        (_PrintedCodeObtainFromFile[compareStatus.Index])[1] = "Printed";
                                        var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                                        if (virtualList != null)
                                            virtualList.SetPrintTime(compareStatus.Index, DateTime.Now.ToString("dd MM yy HH:mm:ss"));
                                        var printedRow = _PrintedCodeObtainFromFile[compareStatus.Index];
                                        printedRow[1] = "Printed";
                                        strPrintedResponseList.Add(printedRow);

                                        string qrVal3 = printedRow.Length > 2 ? printedRow[2] : "";
                                        string prod3 = printedRow.Length > 3 ? printedRow[3] : "";
                                        string exp3 = printedRow.Length > 4 ? printedRow[4] : "";
                                        string pid3 = _SelectedJob?.THJobProductId ?? "";
                                        string batch3 = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(prod3, Shared.Settings?.LineId ?? "");
                                        if (!string.IsNullOrEmpty(qrVal3) && !RLinkLogService.IsQrSentToMaster(qrVal3))
                                        {
                                            var item = (qrVal3, pid3, prod3, exp3, batch3);
                                            RLinkLogService.MarkQrAsPrintedBatch(new List<(string, string, string, string, string)> { item });
                                            RLinkLogService.MarkQrAsPrintedBatchPg(new List<(string, string, string, string, string)> { item });
                                        }
                                    }
                            }

                            else if (Shared.Settings.DuplicatedDBEnable)
                            {
                                for (int i = 0; i < _PrintedCodeObtainFromFile.Count; i++)
                                {
                                    var row = _PrintedCodeObtainFromFile[i];
                                    if (row.Length <= 1 || row[1] == "Printed") continue;
                                    var compareRow = row.Where((_, idx) => idx != 1).ToArray();
                                    if (GetCompareDataByPODFormat(compareRow, _SelectedJob.PODFormat) == podCommand)
                                    {
                                        compareStatus.Index = i;
                                        lock (_SyncObjCodeList)
                                        {
                                            row[1] = "Printed";
                            var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                                            if (virtualList != null)
                                                virtualList.SetPrintTime(i, DateTime.Now.ToString("dd MM yy HH:mm:ss"));
                                            strPrintedResponseList.Add(row);

                                            string qrVal4 = row.Length > 2 ? row[2] : "";
                                            string prod4 = row.Length > 3 ? row[3] : "";
                                            string exp4 = row.Length > 4 ? row[4] : "";
                                            string pid4 = _SelectedJob?.THJobProductId ?? "";
                                            string batch4 = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(prod4, Shared.Settings?.LineId ?? "");
                                            if (!string.IsNullOrEmpty(qrVal4) && !RLinkLogService.IsQrSentToMaster(qrVal4))
                                            {
                                                var item = (qrVal4, pid4, prod4, exp4, batch4);
                                                RLinkLogService.MarkQrAsPrintedBatch(new List<(string, string, string, string, string)> { item });
                                                RLinkLogService.MarkQrAsPrintedBatchPg(new List<(string, string, string, string, string)> { item });
                                            }
                                        }
                                    }
                                    break;
                                }
                            }

                            // Update current code position
                            currentIndex = compareStatus.Index % _MaxDatabaseLine;
                            currentPage = compareStatus.Index / _MaxDatabaseLine;

                            if (currentPage != _CurrentPage)
                            {
                                _CurrentPage = currentPage;
                            }
                            // Luôn focus rowIdx SAU KHI update page
                            {
                                int rowIdx = currentIndex;
                                BeginInvoke(new Action(() =>
                                {
                                    if (rowIdx >= 0 && rowIdx < dgvDatabase.Rows.Count)
                                    {
                                        dgvDatabase.InvalidateRow(rowIdx);
                                        dgvDatabase.ClearSelection();
                                        dgvDatabase.Rows[rowIdx].Cells[0].Selected = true;
                                        int visibleRows = dgvDatabase.DisplayedRowCount(true);
                                        dgvDatabase.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIdx - visibleRows + 2);
                                    }
                                }));
                            }
                            batchCounter++;

                            // Ghi CSV qua background queue
                            if (strPrintedResponseList.Count > 0)
                            {
                                _QueueBufferBackupPrintedCode.Enqueue(new List<string[]>(strPrintedResponseList));
                                //ProjectLogger.WriteDebug($"[PrintedProducer] Enqueued {strPrintedResponseList.Count} items (total={NumberPrinted}, queue={_QueueBufferBackupPrintedCode.Count()})");
                            }
                            strPrintedResponseList.Clear();

                            // Đồng bộ số đã in từ dữ liệu thực tế
                            // Mode 1/2/4: dùng giá trị tuyệt đối từ RSFP, không tự tăng
                            bool isMode124Now2 = _SelectedJob?.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                                || _SelectedJob?.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                                || _SelectedJob?.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;
                            if (!isMode124Now2)
                            {
                                NumberPrinted = Interlocked.Increment(ref _NumberPrinted);
                                Shared.NumberPrinted = NumberPrinted;
                            }
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch (System.OperationCanceledException)
            {
                //Console.WriteLine("Thread update printed status was stoppped!");
                _BackupResponseCancelTokenSource?.Cancel();
            }
            catch (Exception ex)
            {
                // Catch Error - Add by ThongThach 05/12/2023
                Console.WriteLine("Thread update printed status was error!");
                KillAllProccessThread();
                StopProcessAsync(false, Lang.HandleError, false, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }
        /// <summary>Debug: enqueue ảnh lỗi giả để test pipeline lưu ảnh.</summary>
        public void EnqueueFakeErrorImage()
        {
            var bmp = new Bitmap(320, 100);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.DrawString("FAKE ERROR IMAGE\n" + DateTime.Now.ToString("HH:mm:ss"),
                    new Font("Arial", 14, FontStyle.Bold),
                    Brushes.Red, new PointF(10, 10));
            }

            var fakeDetect = new DetectModel
            {
                Text = "FAKE_BARCODE_ERROR",
                Image = bmp,
                CompareResult = ComparisonResult.Invalided,
                Index = TotalChecked + 1,
                CompareTime = 0,
                ProcessingDateTime = DateTime.Now.ToString(Shared.Settings.DateTimeFormatOfResult)
            };

            _QueueBufferDataObtainedResult.Enqueue(fakeDetect);
            Console.WriteLine("[FakeError] Enqueued → kiểm tra THErrorImageFolder");
        }
        private async void UpdateUICheckedResultAsync(CancellationToken token)
        {
            await Task.Run(() => { UpdateUICheckedResult(token); });
        }

        #region Image Show Handler

        private async void UpdateImageAsync(Image image)
        {
            _nextImage = image;
            if (!_isUpdatePending)
            {
                _isUpdatePending = true;
                await Task.Delay(100); // Delay for 100 milisecond or any suitable value
                UpdateImage();
                _isUpdatePending = false;
            }
        }

        private void UpdateImage()
        {
            if (_nextImage != null)
            {
                Invoke(new Action(() =>
                {
                    Image oldImage = pictureBoxPreview.Image;
                    pictureBoxPreview.Image = _nextImage;
                    oldImage?.Dispose();
                }));
                _nextImage = null;
            }
        }

        #endregion Image Show Handler
        private string BuildDisplayText(DetectModel detectModel)
        {
            var hikCam = Shared.Settings.CameraList
                .FirstOrDefault(c => c.CameraType == CameraType.HIKROBOT);
            if (hikCam?.HikrobotCompareMode == HikrobotCompareMode.BarcodeAndOCR
                && detectModel.ExtraFields?.Count > 0)
            {
                string ocr = string.Join(";", detectModel.ExtraFields.Values
                    .Where(v => !string.IsNullOrEmpty(v)));
                if (!string.IsNullOrEmpty(ocr))
                    return detectModel.Text + ";" + ocr;
            }
            return detectModel.Text;
        }
        private void UpdateUICheckedResult(CancellationToken token)
        {
            Debug.WriteLine("UI 2 thread working on thread " + Environment.CurrentManagedThreadId);
            List<string[]> strResultCheckList = new List<string[]>();
            string[] strResult = new string[0];
            int lineCounter = TotalChecked;
            try
            {
                while (true)
                {
                    // Only stop if handled all data
                    if (token.IsCancellationRequested)
                        if (_QueueBufferDataObtainedResult.Count() == 0)
                            token.ThrowIfCancellationRequested();

                    // Waiting until have data
                    DetectModel detectModel = _QueueBufferDataObtainedResult.Dequeue();
                    if (detectModel == null) continue;

                    // Image image = detectModel.Image == null ? new Bitmap(100, 100) : detectModel.Image;
                    bool hasRealImage = detectModel.Image != null;
                    var image = detectModel.Image ?? new Bitmap(100, 100); // MinhChau Modify 11122023

                    bool shouldSaveErrorImage = Shared.Settings.ExportImageEnable
                        || !string.IsNullOrWhiteSpace(Shared.Settings.THErrorImageFolder);

                    if (detectModel.CompareResult != ComparisonResult.Valid)
                    {
                        if (detectModel.ExtraFields?.TryGetValue("IMAGE_ID", out string imgId) == true && !string.IsNullOrEmpty(imgId))
                        {
                            // Copy ảnh bất đồng bộ với delay nhẹ 1ms
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(1); // delay nhẹ để không block processing loop
                                TryDownloadSingleErrorImage(imgId, detectModel.Index, maxRetries: 3, retryDelayMs: 300);
                                // Nếu fail → KHÔNG lưu ảnh đen, bỏ qua
                            });
                        }
                    }
                    UpdateImageAsync(image);
                    //strResult = new string[] { detectModel.Index + "", detectModel.Text,
                    //        detectModel.CompareResult.ToString(),detectModel.CodeQuality, detectModel.isBarcodeWithinThreshold, (detectModel.CompareTime) + " ms", detectModel.ProcessingDateTime , detectModel.Device, detectModel.Sampled};
                    // thinh clean strResult, more flexible
                    strResult = new string[_ColumnNames.Length];

                    strResult[Index_Index] = detectModel.Index.ToString();
                    strResult[Index_ResultData] = detectModel.Text;
                    strResult[Index_Result] = detectModel.CompareResult.ToString();
                    //strResult[Index_BarcodeQuality] = detectModel.CodeQuality;
                    //strResult[Index_Position] = detectModel.isBarcodeWithinThreshold;
                    strResult[Index_ProcessingTime] = detectModel.CompareTime + " ms";

                    // Camera extra fields
                    string nsxVal = "", hsdVal = "", batchVal = "", receiveTime = "", rawFrame = "", timeVal = "";
                    if (detectModel.ExtraFields != null)
                    {
                        detectModel.ExtraFields.TryGetValue("NSX", out nsxVal);
                        detectModel.ExtraFields.TryGetValue("TIME", out timeVal);
                        detectModel.ExtraFields.TryGetValue("HSD", out hsdVal);
                        detectModel.ExtraFields.TryGetValue("BATCH", out batchVal);
                        detectModel.ExtraFields.TryGetValue("RECEIVE_TIME", out receiveTime);
                        detectModel.ExtraFields.TryGetValue("RAW_FRAME", out rawFrame);
                    }

                    strResult[Index_DateTime] = string.IsNullOrEmpty(receiveTime) ? detectModel.ProcessingDateTime : receiveTime;
                    // NSX: camera detect → fallback virtual list
                    var camVirtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                    if (string.IsNullOrEmpty(nsxVal) && camVirtualList != null)
                        nsxVal = camVirtualList.Nsx;
                    else if (string.IsNullOrEmpty(nsxVal) && _PrintedCodeObtainFromFile.Count > 0)
                    {
                        var lastRow = _PrintedCodeObtainFromFile[_PrintedCodeObtainFromFile.Count - 1];
                        if (lastRow.Length > 4)
                            nsxVal = lastRow[lastRow.Length - 2];
                    }
                    strResult[Index_NSX] = NormalizeDateToDdMmYy(nsxVal);
                    // HSD: camera detect → fallback database
                    if (string.IsNullOrEmpty(hsdVal))
                    {
                        if (camVirtualList != null)
                            hsdVal = camVirtualList.Hsd;
                        else if (_PrintedCodeObtainFromFile.Count > 0)
                        {
                            var lastRow = _PrintedCodeObtainFromFile[_PrintedCodeObtainFromFile.Count - 1];
                            if (lastRow.Length > 4)
                                hsdVal = lastRow[lastRow.Length - 3];
                        }
                    }
                    strResult[Index_HSD] = NormalizeDateToDdMmYy(hsdVal);
                    // Batch = NSX(ddMMyy) + LineId
                    batchVal = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(
                        NormalizeDateToDdMmYy(nsxVal), Shared.Settings?.LineId ?? "");
                    strResult[Index_BatchCol] = batchVal;
                    strResult[Index_FrameInfo] = rawFrame;

                    // Save camera NSX+Time for tb_PrintingLogs
                    if (!string.IsNullOrEmpty(nsxVal) && !string.IsNullOrEmpty(timeVal))
                        _lastCameraNsxWithTime = $"{nsxVal} {timeVal}";
                    else if (!string.IsNullOrEmpty(nsxVal))
                        _lastCameraNsxWithTime = nsxVal;

                    if (!string.IsNullOrEmpty(_lastCameraNsxWithTime))
                        UpdateBatchFromCamera();

                    // Error classification: A=Pass, B=OCR error, F=Camera/QR mismatch
                    string qrRes = "", ocrRes = "";
                    detectModel.ExtraFields?.TryGetValue("QR_RESULT", out qrRes);
                    detectModel.ExtraFields?.TryGetValue("OCR_RESULT", out ocrRes);
                    bool qrPass = (qrRes == "OK");
                    bool ocrPass = (ocrRes == "OK");
                    string errorType;
                    if (qrPass && ocrPass)
                        errorType = detectModel.CompareResult == ComparisonResult.Valid ? "A" : "F";
                    else if (!qrPass && ocrPass)
                        errorType = "B";
                    else
                        errorType = "F";
                    detectModel.ExtraFields["ERROR_TYPE"] = errorType;
                    strResult[Index_ErrorType] = errorType;

                    // ── Count A/B/F ────────────────────────────────────
                    if (errorType == "A") ErrorCountA = Interlocked.Increment(ref _errorCountA);
                    else if (errorType == "B") ErrorCountB = Interlocked.Increment(ref _errorCountB);
                    else if (errorType == "F") ErrorCountF = Interlocked.Increment(ref _errorCountF);
                    // ────────────────────────────────────────────────────

                    // ── Fire camera error log if applicable ──
                    if (errorType != "A")
                    {
                        string errNsx = nsxVal, errHsd = hsdVal, errFrameInfo = rawFrame;
                        int errIdx = detectModel.Index;
                        string errQr2 = detectModel.Text ?? "";
                        Task.Run(() => FireRLinkCameraError(errQr2, errorType, errIdx, errNsx, errHsd, errFrameInfo));
                    }

                    // ── PLC signal for individual F errors (Phân loại) ──
                    if (qrPass && ocrPass && errorType == "F")
                    {
                        Shared.SensorController?.Send("REJECT");
                        PhanLoaiCount = Interlocked.Increment(ref _phanLoaiCount);
                    }


                    // ── Consecutive error counting (Part 3) ──
                    if (errorType == "A")
                    {
                        Interlocked.Exchange(ref _consecutiveErrorCount, 0);
                        _consecutiveErrorAlertShown = false;
                        UpdateConsecutiveErrorDisplay();
                    }
                    else
                    {
                        int count = Interlocked.Increment(ref _consecutiveErrorCount);
                        UpdateConsecutiveErrorDisplay();
                        int threshold = _SelectedJob?.THMaxConsecutiveError ?? 5;
                        if (count == threshold)
                        {
                            ProjectLogger.WriteInfo($"[PLC] Lỗi liên tục đạt {count}/{threshold} → Gửi ARED + Stop");
                            Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                            BeginInvoke(new Action(() =>
                            {
                                CustomMessageBox.ShowCenterScreen(
                                    $"Hệ thống phát hiện {count} lỗi liên tục! Đã gửi lệnh dừng máy.",
                                    "Lỗi liên tục", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));
                            RequestAutoStop($"Lỗi liên tục {count}/{threshold}", false);
                            Interlocked.Exchange(ref _consecutiveErrorCount, 0);
                            UpdateConsecutiveErrorDisplay();
                        }
                    }
                    //strResult[Index_Device] = detectModel.Device;
                    //strResult[Index_Sampled] = detectModel.Sampled;


                    strResultCheckList.Add(strResult);

                    //lock (_SyncObjCheckedResultList)
                    //{
                    //    _CheckedResultCodeList.Add(strResult);
                    //}
                    lock (_SyncObjCheckedResultList)
                    {
                        _CheckedResultCodeList.Add(strResult);
                        // Trim khi vượt ngưỡng — giữ 50000 dòng gần nhất
                        const int MaxCheckedResultRows = 50_000;
                        if (_CheckedResultCodeList.Count > MaxCheckedResultRows)
                            _CheckedResultCodeList.RemoveRange(0, _CheckedResultCodeList.Count - MaxCheckedResultRows);
                    }

                    // Update all UI in 1 BeginInvoke (non-blocking, real-time per packet)
                    int totalCount = _CheckedResultCodeList.Count();
                    _progressBarCounter++;
                    bool doProgress = _progressBarCounter % 10 == 0;
                    int targetRow = totalCount < 500 ? totalCount - 1 : 499;
                    bool rowChanged = targetRow != _lastSelectedRow;
                    _lastSelectedRow = targetRow;
                    BeginInvoke(new Action(() =>
                    {
                        int hScroll = dgvCheckedResult.HorizontalScrollingOffset;
                        lineCounter = totalCount - 1;

                        // Invalidate every packet for real-time data
                        dgvCheckedResult.Invalidate();

                        // Only update selection when row actually changes (prevents flicker)
                        if (rowChanged)
                            dgvCheckedResult.Rows[targetRow].Cells[0].Selected = true;
                        dgvCheckedResult.HorizontalScrollingOffset = hScroll;

                        UpdateSummaryTable();
                        if (doProgress)
                            ProgressBarCheckedUpdate();

                        txtCodeResult.Text = BuildDisplayText(detectModel);
                        txtProcessingTimeResult.Text = $"{detectModel.CompareTime:F2} ms";
                        txtStatusResult.Text = detectModel.CompareResult.ToFriendlyString();
                        txtStatusResult.ForeColor = detectModel.CompareResult == ComparisonResult.Valid ? Color.FromArgb(0, 199, 82) : Color.Red;
                    }));

                    //END Add result need save to queue
                    _QueueBufferBackupCheckedResult.Enqueue(new List<string[]>(strResultCheckList));
                    //Clear list
                    strResult.DefaultIfEmpty();
                    strResultCheckList.Clear();
                    //END Update value to user interface
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Thread update checked result was stopped!");
                Thread.Sleep(20);
                _BackupImageCancelTokenSource?.Cancel();
                _BackupResultCancelTokenSource?.Cancel();
                _QueueBufferBackupImage.Enqueue(null);
                _QueueBufferBackupCheckedResult.Enqueue(null);
            }
            catch (Exception ex)
            {
                // Catch Error - Add by ThongThach 05/12/2023
                Console.WriteLine("Thread update checked result was error!");
                KillAllProccessThread();
                StopProcessAsync(false, Lang.HandleError, false, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        #region ExportFile

        private void SaveResultToFile(List<string[]> list, string path)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(path, true, new UTF8Encoding(true)))
                {
                    //Add row result
                    foreach (string[] strArr in list)
                    {
                        streamWriter.WriteLine(String.Join(",", strArr.Select(x => Csv.Escape(x))));
                    }
                }
            }
            catch (Exception ex)
            {
                Shared.RaiseOnLogError(ex);
            }
        }

        private void SaveSendLogToFile(string[] list, string path)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(path, true, new UTF8Encoding(true)))
                {
                    streamWriter.WriteLine(String.Join(",", list.Select(x => Csv.Escape(x))));//Add row result
                }
            }
            catch (Exception ex)
            {
                Shared.RaiseOnLogError(ex);
            }
        }

        private PrintingQueueProcessor _printedDataProcess;


        
        private static string NormalizeDateStatic(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            if (DateTime.TryParseExact(raw, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt.ToString("dd MM yy");
            if (DateTime.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("dd MM yy");
            var sep = raw.Contains("/") ? "/" : raw.Contains("-") ? "-" : null;
            if (sep != null)
            {
                var r = raw.Replace(sep, " ");
                var p = r.Split(' ');
                if (p.Length >= 3 && p[2].Length == 4)
                    r = $"{p[0]} {p[1]} {p[2].Substring(2, 2)}";
                return r;
            }
            return raw;
        }



        private void StartPrintedDataProcess(PrintingQueueProcessor PrintedProcess)
        {
            try
            {
                if (PrintedProcess == null)
                    return;

                PrintedProcess.Start();

                if (Shared.UserPermission.isOnline)
                {
                    SafeInvoke(SyncLoading, () => SyncLoading.Visible = true);
                    SafeInvoke(SyncDataText, () =>
                    {
                        SyncDataText.Text = "Đang đ?ng b?";
                        SyncDataText.ForeColor = Color.Green;
                    });
                }

            }
            catch (Exception)
            {
                // optional: log the exception
            }
        }
        private void SafeInvoke(Control control, Action action)
        {
            if (control == null) return;

            if (control.InvokeRequired)
                control.BeginInvoke(action);
            else
                action();
        }

        private void StopPrintedDataProcess(PrintingQueueProcessor PrintedProcess)
        {
            try
            {
                if (PrintedProcess == null)
                    return;

                PrintedProcess.Stop();

                if (Shared.UserPermission.isOnline)
                {
                    SafeInvoke(SyncLoading, () => SyncLoading.Visible = false);
                    SafeInvoke(SyncDataText, () =>
                    {
                        SyncDataText.Text = "D?ng đ?ng b?";
                        SyncDataText.ForeColor = Color.Red;
                    });
                }

            }
            catch (Exception)
            {
                // optional: log the exception
            }
        }

        private async void SendDataToServer(ApiService apiService, List<string[]> value, string path, string url)
        {
            try
            {
                var payload = new
                {
                    data = value
                };

                bool isSent = await apiService.PostApiDataAsync(url, payload);
                //MessageBox.Show($"isPosted: {isSent}");

                string t = $"isPosted: {isSent}";

                if (!File.Exists(path))
                {
                    // Create empty file with BOM if it doesn't exist
                    using (var fileStream = File.Create(path))
                    {
                        byte[] bom = new UTF8Encoding(true).GetPreamble();
                        fileStream.Write(bom, 0, bom.Length);
                    }
                }

                using (var streamWriter = new StreamWriter(path, true, new UTF8Encoding(true)))
                {
                    foreach (string[] strArr in value)
                    {
                        string escapedRow = string.Join(",", strArr.Select(Csv.Escape));
                        escapedRow += isSent ? ",Sent" : ",Not Sent"; // Append status

                        streamWriter.WriteLine(escapedRow);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while sending data:\n" + ex.Message);
            }
        }

        private async Task BackupSendLogAsync(CancellationToken token)
        {

            await Task.Run(() =>
            {
                pathSendLog = CommVariables.PathPrintedResponse + DateTime.Now.ToString(_DateTimeFormat) + "_SendLog_" + _SelectedJob.FileName + ".csv";
                try
                {
                    while (true)
                    {
                        // Only stop if handled all data
                        if (token.IsCancellationRequested)
                        {
                            if (_QueueBufferBackupSendLog.Count() == 0)
                                token.ThrowIfCancellationRequested();
                        }

                        string[] valueArr = _QueueBufferBackupSendLog.Dequeue();
                        if (valueArr == null) continue;
                        if (valueArr.Count() >= 0)
                        {
                            // This text is added only once to the file.
                            if (!File.Exists(pathSendLog))
                            {
                                // Create a file to write to.
                                using (StreamWriter streamWriter = new StreamWriter(pathSendLog, false, new UTF8Encoding(true)))
                                {
                                    //Add header
                                    streamWriter.WriteLine(String.Join(",", _DatabaseColunms.Take(_DatabaseColunms.Length - 1).Skip(1)));
                                }
                            }

                            SaveSendLogToFile(valueArr, pathSendLog);
                        }
                        Thread.Sleep(5);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Thread backup printed response was stopped!");
                }
                catch (Exception ex)
                {
                    // Catch Error - Add by ThongThach 05/12/2023
                    Console.WriteLine("Thread backup printed response was error!");
                    if (!_isStopping)
                    {
                        KillAllProccessThread();
                        StopProcessAsync(false, Lang.HandleError, false, true);
                        Shared.RaiseOnLogError(ex);
                        EnableUIComponent(OperationStatus.Stopped);
                    }
                }
            });
        }

        private async Task BackupResultFinishPrintCommandAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                pathRSFPLog = CommVariables.PathPrintedResponse + DateTime.Now.ToString(_DateTimeFormat) + "_RSFPLog_" + _SelectedJob.FileName + ".csv";
                try
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            if (_QueueBufferBackupRSFPLog.Count() <= 0)  // Only stop Task if dequeue all 
                            {
                                token.ThrowIfCancellationRequested();
                            }
                        }

                        string[] valueArr = _QueueBufferBackupRSFPLog.Dequeue();
                        if (valueArr == null) continue;
                        if (valueArr.Count() > 0)
                        {
                            if (!File.Exists(pathRSFPLog))
                            {
                                // Create a file to write to.
                                using (StreamWriter streamWriter = new StreamWriter(pathRSFPLog, false, new UTF8Encoding(true)))
                                {
                                    streamWriter.WriteLine(String.Join(",", _DatabaseColunms.Take(_DatabaseColunms.Length - 1).Skip(1))); //Add header
                                }
                            }
                            SaveSendLogToFile(valueArr, pathRSFPLog);
                        }
                        Thread.Sleep(5);
                    }
                }
                catch { }
            });

        }

        private readonly HashSet<string> _sendingQrSet = new HashSet<string>();

        private void StartPrintedQrWorker()
        {
            _isPrintedQrWorkerRunning = true;
            _printedQrWorker = new Thread(PrintedQrWorkerLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _printedQrWorker.Start();
        }

        private void PrintedQrWorkerLoop()
        {
            while (_isPrintedQrWorkerRunning)
            {
                try
                {
                    var pending = RLinkLogService.GetUnsentMarkedQrPg(300);
                    if (pending.Count > 0)
                    {
                        string lineId = Shared.Settings?.LineId ?? "";
                        string lineName = Shared.Settings?.LineName ?? "";
                        string factoryCode = Shared.Settings?.FactoryCode ?? "";

                        foreach (var (qrCode, batch, pid, prod, exp, jobNameFromDb, printedAt) in pending)
                        {
                            if (string.IsNullOrEmpty(qrCode)) continue;
                            if (RLinkLogService.IsQrSentToMaster(qrCode)) continue;
                            lock (_sendingQrSet)
                            {
                                if (!_sendingQrSet.Add(qrCode)) continue;
                            }

                            string capturedQr = qrCode;
                            string capturedJobName = jobNameFromDb ?? "";
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    var svc = RLinkMasterServiceFactory.Instance;
                                    if (svc == null) return;
                                    bool ok = await svc.MarkQrUsedAsync(
                                        new List<string> { capturedQr },
                                        capturedJobName, lineId, lineName, factoryCode,
                                        batch ?? "", pid ?? "", prod ?? "", exp ?? "", printedAt);
                                    if (ok)
                                        RLinkLogService.MarkQrAsSentToMaster(capturedQr);
                                }
                                catch { }
                                finally
                                {
                                    lock (_sendingQrSet) _sendingQrSet.Remove(capturedQr);
                                }
                            });
                        }

                        Debug.WriteLine($"[PrintedQrWorker] Sent {pending.Count} QR to mark-used API");
                    }

                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError($"[PrintedQrWorker] Unexpected: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }
            Debug.WriteLine("[PrintedQrWorker] Stopped.");
        }

        private void FlushPrintedQrWorker()
        {
            if (!_isPrintedQrWorkerRunning) return;
            _isPrintedQrWorkerRunning = false;
            try { _printedQrWorker?.Join(5000); } catch { }
        }

        private void WritePrintedDebugLog(string message)
        {
            try
            {
                string logDir = CommVariables.PathProgramDataApp + "Logs\\";
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                string logPath = logDir + $"PrintedDebug_{DateTime.Now:yyyyMMdd}.log";
                File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch { }
        }

        private async Task BackupPrintedResponseAsync(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                try
                {
                    // ── Đảm bảo file CSV tồn tại ──
                    if (_SelectedJob.PrintedResponePath == "")
                    {
                        string fileName = DateTime.Now.ToString(_DateTimeFormat) + "_Printed_" + _SelectedJob.FileName;
                        string csvPath = CommVariables.PathPrintedResponse + fileName + ".csv";

                        if (!Directory.Exists(CommVariables.PathPrintedResponse))
                            Directory.CreateDirectory(CommVariables.PathPrintedResponse);

                        if (!File.Exists(csvPath))
                        {
                            using (var sw = new StreamWriter(csvPath, true, new UTF8Encoding(true)))
                            {
                                var cols = _DatabaseColunms?.ToList() ?? new List<string>();
                                if (!cols.Contains("Thời gian gửi")) cols.Add("Thời gian gửi");
                                if (!cols.Contains("Thời gian in")) cols.Add("Thời gian in");
                                sw.WriteLine(string.Join(",", cols));
                            }
                        }

                        _SelectedJob.PrintedResponePath = fileName + ".csv";
                        int saveRetry = 0;
                        while (saveRetry < 3)
                        {
                            try { _SelectedJob.SaveFile(); break; }
                            catch (Exception ex)
                            {
                                saveRetry++;
                                ProjectLogger.WriteError($"[BackupPrintedResponse] SaveFile retry {saveRetry}: {ex.Message}");
                                if (saveRetry >= 3) break;
                                Thread.Sleep(100);
                            }
                        }
                    }

                    string path = CommVariables.PathPrintedResponse + _SelectedJob.PrintedResponePath;
                    int saveCounter = 0;

                    while (true)
                    {
                        // Cancel check → save + throw (giống code cũ)
                        if (token.IsCancellationRequested && _QueueBufferBackupPrintedCode.Count() == 0)
                        {
                            _SelectedJob.NumberOfPrintedCodes = NumberPrinted;
                            try { _SelectedJob.SaveFile(); } catch { }
                            token.ThrowIfCancellationRequested();
                        }

                        List<string[]> batch = _QueueBufferBackupPrintedCode.Dequeue();
                        if (batch == null)
                        {
                            Thread.Sleep(10);  // Đợi producer enqueue
                            continue;
                        }

                        // Ghi file — mở/ghi/đóng mỗi batch (giống checkresult)
                        using (var sw = new StreamWriter(path, true, new UTF8Encoding(true)))
                        {
                            foreach (var row in batch)
                            {
                                sw.WriteLine(string.Join(",", row.Select(x => Csv.Escape(x))));

                                // SaaS/SAP sender khong hoat dong (Start() da comment),
                                // AppendPrintingEntry rewrite toan bo file gay block consumer
                                // if (_printedDataProcess != null && row.Length >= 4)
                                // {
                                //     _printedDataProcess.Enqueue(int.Parse(row[0]), row[2], row[3]);
                                // }
                            }
                        }
                        // using block exit → file closed → OS refresh metadata → Explorer thấy tăng

                        // Save job file mỗi 50 batch
                        if (++saveCounter % 50 == 0)
                        {
                            _SelectedJob.NumberOfPrintedCodes = NumberPrinted;
                            try { _SelectedJob.SaveFile(); } catch { }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[BackupPrintedResponse] Thread stopped.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[BackupPrintedResponse] Error: " + ex.Message);
                    if (!_isStopping)
                    {
                        KillAllProccessThread();
                        StopProcessAsync(false, Lang.HandleError, false, true);
                        Shared.RaiseOnLogError(ex);
                        EnableUIComponent(OperationStatus.Stopped);
                    }
                }
            });
        }

        private async void ExportCheckedResultToFileAsync(CancellationToken token)
        {
            await Task.Run(() => { NewExportCheckedResultToFile(token); });
        }

        private VerificationQueueProcessor _checkedDataProcess;
        private async void NewExportCheckedResultToFile(CancellationToken token)
        {
            if (_SelectedJob.CheckedResultPath == "")
            {
                string fileName = DateTime.Now.ToString(_DateTimeFormat) + "_" + _SelectedJob.FileName;
                string path = CommVariables.PathCheckedResult + fileName + ".csv";

                // Determine whether the directory exists.
                if (!Directory.Exists(CommVariables.PathCheckedResult))
                {
                    // Try to create the directory.
                    Directory.CreateDirectory(CommVariables.PathCheckedResult);
                }

                // This text is added only once to the file.
                if (!File.Exists(path))
                {
                    // Create a file to write to.
                    using (StreamWriter streamWriter = new StreamWriter(path, true, new UTF8Encoding(true)))
                    {
                        //Add header
                        streamWriter.WriteLine(String.Join(",", _ColumnNames));
                    }
                }

                _SelectedJob.CheckedResultPath = fileName + ".csv";
                _SelectedJob.SaveFile();
            }

            try
            {
                string path = CommVariables.PathCheckedResult + _SelectedJob.CheckedResultPath;

                //if (!Directory.Exists(CommVariables.PathSentDataChecked))
                //{
                //    Directory.CreateDirectory(CommVariables.PathSentDataChecked);
                //}
                //string sentDataPath = CommVariables.PathSentDataChecked + _SelectedJob.CheckedResultPath;
                //string url = ManufacturingApis.postCheckedDataUrl();


                //try
                //{
                //    if (_checkedDataProcess != null)
                //    {
                //    _checkedDataProcess?.Stop();
                //        //await Task.Delay(1000);
                //    }
                //    _checkedDataProcess = THTrueMilkProcessorFactory.CreateVerificationProcessor(sentDataPath, url, path);
                //    _checkedDataProcess.Start();
                //    //await Task.Delay(200);

                //}
                //catch (Exception)
                //{
                //}

                while (true)
                {
                    // Only stop if handled all data
                    if (token.IsCancellationRequested)
                        if (_QueueBufferBackupCheckedResult.Count() == 0)
                            token.ThrowIfCancellationRequested();

                    List<string[]> valueArr = _QueueBufferBackupCheckedResult.Dequeue();

                    if (valueArr == null) continue;
                    var clone = valueArr.Select(arr => arr.ToArray()).ToList();
                    if (valueArr.Count() > 0)
                    {
                        // use the Dispatching or the Manufacturing to extract the code, write in the model a function to do that.
                        SaveResultToFile(valueArr, path);
                        try
                        {
                            // SaaS/SAP check data sender khong hoat dong (Start() da comment)
                            // if (_checkedDataProcess == null) await Task.Delay(400);
                            // _checkedDataProcess.Enqueue(int.Parse(clone[0][0]), clone[0]);

                            if (clone[0][Index_Result] == "Valid")
                            {
                                UpdatePrintedResponse(clone[0][Index_ResultData]);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    valueArr.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Thread backup checked result was stopped!");
            }
            catch (Exception ex)
            {
                // Catch Error - Add by ThongThach 05/12/2023
                Console.WriteLine("Thread backup checked result was error!");
                KillAllProccessThread();
                StopProcessAsync(false, Lang.HandleError, false, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        //private void UpdatePrintedResponse(string data)
        //{
        //    string podCommand = data;
        //    List<string[]> strPrintedResponseList = new List<string[]>();
        //    var isAutoComplete = _SelectedJob.CompareType == CompareType.Database; // Check if need to auto stop procces when compare type is verify and print
        //    int numOfResponse = NumberPrinted;
        //    int currentIndex = 0;
        //    var currentPage = 0;
        //    if (podCommand != null)
        //    {
        //        // Update printed status
        //        if (_CodeListPODFormat.TryGetValue(podCommand, out CompareStatus compareStatus))
        //        {
        //            // Printed response backup data
        //            if (_PrintedCodeObtainFromFile[compareStatus.Index][1] == "Waiting")
        //            {
        //                NumberPrinted++;
        //                Shared.NumberPrinted = NumberPrinted;
        //                lock (_SyncObjCodeList)
        //                {
        //                    (_PrintedCodeObtainFromFile[compareStatus.Index])[1] = "Printed";
        //                    strPrintedResponseList.Add(_PrintedCodeObtainFromFile[compareStatus.Index]);
        //                }
        //            }
        //            else
        //            {
        //                return;
        //            }


        //            // Update current code position
        //            currentIndex = compareStatus.Index % _MaxDatabaseLine;
        //            currentPage = compareStatus.Index / _MaxDatabaseLine;

        //            if (currentPage != _CurrentPage)
        //            {
        //                _CurrentPage = currentPage;
        //                Invoke(new Action(() => { dgvDatabase.Invalidate(); }));
        //            }
        //            else
        //            {
        //                Invoke(new Action(() =>
        //                {
        //                    dgvDatabase.Invalidate();
        //                    dgvDatabase.Rows[currentIndex].Cells[0].Selected = true;
        //                }));
        //            }

        //            // Send backup data to backup thread
        //            _QueueBufferBackupPrintedCode.Enqueue(new List<string[]>(strPrintedResponseList)); // Enqueue backup data
        //                                                                                               // Clear list
        //            strPrintedResponseList.Clear();
        //        }
        //    }


        //}
        //private void UpdatePrintedResponse(string data)
        //{
        //    string podCommand = data;
        //    List<string[]> strPrintedResponseList = new List<string[]>();
        //    int currentIndex = 0;
        //    var currentPage = 0;

        //    if (podCommand == null) return;

        //    if (_CodeListPODFormat.TryGetValue(podCommand, out CompareStatus compareStatus))
        //    {
        //        bool isRepeatedQrMode = _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
        //              || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;

        //        int targetIndex = compareStatus.Index;

        //        if (isRepeatedQrMode)
        //        {
        //            // Mode 1/2: tìm row Waiting tiếp theo (không dùng compareStatus.Index cố định)
        //            targetIndex = _PrintedCodeObtainFromFile.FindIndex(x => x[1] == "Waiting");
        //            if (targetIndex < 0) return; // Hết buffer
        //        }
        //        else
        //        {
        //            if (_PrintedCodeObtainFromFile[targetIndex][1] != "Waiting") return;
        //        }

        //        NumberPrinted++;
        //        Shared.NumberPrinted = NumberPrinted;
        //        lock (_SyncObjCodeList)
        //        {
        //            _PrintedCodeObtainFromFile[targetIndex][1] = "Printed";
        //            strPrintedResponseList.Add(_PrintedCodeObtainFromFile[targetIndex]);
        //        }

        //        // Cập nhật index để UI scroll đúng vị trí
        //        currentIndex = targetIndex % _MaxDatabaseLine;
        //        currentPage = targetIndex / _MaxDatabaseLine;

        //        if (currentPage != _CurrentPage)
        //        {
        //            _CurrentPage = currentPage;
        //            Invoke(new Action(() => { dgvDatabase.Invalidate(); }));
        //        }
        //        else
        //        {
        //            Invoke(new Action(() =>
        //            {
        //                dgvDatabase.Invalidate();
        //                dgvDatabase.Rows[currentIndex].Cells[0].Selected = true;
        //            }));
        //        }

        //        _QueueBufferBackupPrintedCode.Enqueue(new List<string[]>(strPrintedResponseList));
        //        strPrintedResponseList.Clear();
        //    }
        //}


        private void UpdatePrintedResponse(string data)
        {
            string podCommand = data;
            if (podCommand == null) return;

            bool isRepeatedQrMode = _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;

            // ── Mode 1/2: RSFP đã xử lý marking trong UpdateUIPrintedResponse rồi,
            //     UpdatePrintedResponse từ camera check sẽ gây double-count.
            //     Chặn cho cả OnProduction lẫn AfterProduction khi dùng PrinterSeries.
            if (isRepeatedQrMode && _SelectedJob.PrinterSeries)
                return;

            List<string[]> strPrintedResponseList = new List<string[]>();
            int currentIndex = 0;
            int currentPage = 0;

            if (!_CodeListPODFormat.TryGetValue(podCommand, out CompareStatus compareStatus)) return;

            int targetIndex;
            lock (_SyncObjCodeList) // ← FindIndex + mark TRONG 1 lock duy nhất
            {
                if (isRepeatedQrMode)
                {
                    // Mode 1/2 không dùng printer RSFP: tìm dòng Waiting tiếp theo
                    targetIndex = _PrintedCodeObtainFromFile.FindIndex(x => x[1] == "Waiting");
                    if (targetIndex < 0) return;
                }
                else
                {
                    targetIndex = compareStatus.Index;
                    if (_PrintedCodeObtainFromFile[targetIndex][1] != "Waiting") return;
                }

                if (_PrintedCodeObtainFromFile is PrintedCodeVirtualList vl)
                    vl.SetPrinted(targetIndex, DateTime.Now.ToString("dd MM yy HH:mm:ss"));
                else
                    _PrintedCodeObtainFromFile[targetIndex][1] = "Printed";
                Interlocked.Increment(ref _sessionPrintedCount);
                var printedRow = _PrintedCodeObtainFromFile[targetIndex];
                printedRow[1] = "Printed";
                strPrintedResponseList.Add(printedRow);

                string qrVal5 = printedRow.Length > 2 ? printedRow[2] : "";
                string prod5 = printedRow.Length > 3 ? printedRow[3] : "";
                string exp5 = printedRow.Length > 4 ? printedRow[4] : "";
                string pid5 = _SelectedJob?.THJobProductId ?? "";
                string batch5 = BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ComputeBatchNsxLine(prod5, Shared.Settings?.LineId ?? "");
                if (!string.IsNullOrEmpty(qrVal5) && !RLinkLogService.IsQrSentToMaster(qrVal5))
                {
                    var item = (qrVal5, pid5, prod5, exp5, batch5);
                    RLinkLogService.MarkQrAsPrintedBatch(new List<(string, string, string, string, string)> { item });
                    RLinkLogService.MarkQrAsPrintedBatchPg(new List<(string, string, string, string, string)> { item });
                }
            }

            NumberPrinted = Interlocked.Increment(ref _NumberPrinted);
            Shared.NumberPrinted = NumberPrinted;

            currentIndex = targetIndex % _MaxDatabaseLine;
            currentPage = targetIndex / _MaxDatabaseLine;

                            if (currentPage != _CurrentPage)
                            {
                                _CurrentPage = currentPage;
                            }
                            {
                                int rowIdx = currentIndex;
                                BeginInvoke(new Action(() =>
                                {
                                    if (rowIdx >= 0 && rowIdx < dgvDatabase.Rows.Count)
                                    {
                                        dgvDatabase.InvalidateRow(rowIdx);
                                        dgvDatabase.ClearSelection();
                                        dgvDatabase.Rows[rowIdx].Cells[0].Selected = true;
                                        int visibleRows = dgvDatabase.DisplayedRowCount(true);
                                        dgvDatabase.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIdx - visibleRows + 2);
                                    }
                                }));
            }
            _updatePrintedBatchCounter++;

            _QueueBufferBackupPrintedCode.Enqueue(strPrintedResponseList);
          //  ProjectLogger.WriteDebug($"[PrintedProducer] Enqueued {strPrintedResponseList.Count} items (total={NumberPrinted}, queue={_QueueBufferBackupPrintedCode.Count()})");
        }

        private async void ExportImageToFileAsync(CancellationToken token)
        {
            await Task.Run(() => { NewExportImageToFile(token); });
        }

        private void NewExportImageToFile(CancellationToken token)
        {
            string subFolder = !string.IsNullOrWhiteSpace(_SelectedJob?.THJobErrorImageFolder)
                ? _SelectedJob.THJobErrorImageFolder
                : (!string.IsNullOrWhiteSpace(Shared.Settings?.THErrorImageFolder)
                    ? Shared.Settings.THErrorImageFolder
                    : "Default");

            string basePath = ErrorImageBasePath;
            string root = Path.Combine(basePath, subFolder);
            string jobFolder = Path.Combine(root, _SelectedJob.FileName);
            try
            {
                if (!Directory.Exists(jobFolder))
                    Directory.CreateDirectory(jobFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NewExportImageToFile] ✘ Không thể tạo thư mục '{jobFolder}': {ex.Message}");
                jobFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                    "RLink_ErrorImages",
                    _SelectedJob.FileName);
                Directory.CreateDirectory(jobFolder);
            }

            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        if (_QueueBufferBackupImage.Count() == 0)
                            token.ThrowIfCancellationRequested();
                    }

                    ExportImageModel exportImageModel = _QueueBufferBackupImage.Dequeue();
                    if (exportImageModel != null)
                    {
                        // Bỏ ký tự '\' đầu để Path.Combine không nhầm là absolute
                        string fileName = string.Format("{0}_Job_{1}_Image_{2:D7}.bmp",
                            _ExportNamePrefix, _SelectedJob.FileName, exportImageModel.Index);
                        string path = Path.Combine(jobFolder, fileName);
                        using (exportImageModel.Image)
                        {
                            UtilityFunctions.SaveBitmap(exportImageModel.Image, path);
                        }
                    }
                    Thread.Sleep(5);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Thread backup image was stoppped!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Thread backup image was error!");
                KillAllProccessThread();
                StopProcessAsync(false, Lang.HandleError, false, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        #endregion End ExportFile
        /// <summary>
        /// //////
        /// SynchronizedQueue<ExportImageModel> _QueueBufferBackupImage
        /// ConcurrentQueue _pendingErrorImages
        /// shouldSaveErrorImage && CompareResult != Valid
        /// _QueueBufferBackupImage.Enqueue(new ExportImageModel(...))
        /// TryDownloadSingleErrorImage(...)
        /// ExportImageToFileAsync() → NewExportImageToFile()
        /// DownloadPendingImages() (khi Stop)
        /// BuildErrorImagePath(imageId, jobName)
        /// EnsureErrorImageExists(imageId, jobName) — lazy copy khi xem ảnh
       ///  Camera detect QR sai (CompareResult != Valid)
        //  Path A: Lưu bitmap từ camera frame
        //   │  _QueueBufferBackupImage.Enqueue(bitmap)
        //   │       → Consumer: NewExportImageToFile()
        //   │       → Lưu file:
        //   │         (thực tế là JPEG dù tên .bmp)
        //   │
        //    Path B: Copy ảnh từ camera FTP
        //       │
        //       └── TryDownloadSingleErrorImage() 
        //           → Thread.Sleep(200ms)
        //           → Tìm file trong camera FTP: { FtpImagePath}/{ date}/{ id}.*
        //           → Copy sang ImagesError folder
        //           → Nếu fail → enqueue _pendingErrorImages
        /// </summary>
        /// <param name="imageId"></param>
        /// <param name="index"></param>
        /// <param name="maxRetries"></param>
        /// <param name="retryDelayMs"></param>
        /// <returns></returns>

        private bool TryDownloadSingleErrorImage(string imageId, int index, int maxRetries, int retryDelayMs)
        {
            var vscCam = Shared.vscCamera;
            string basePath = vscCam?.FtpImagePath;
            if (string.IsNullOrEmpty(basePath))
                basePath = Shared.Settings?.THFtpImagePath
                    ?? @"D:\hinhanh\VS\Camera\Images";

            string[] exts = { ".png", ".bmp", ".jpg" };
            string dateFolder = imageId.Length >= 8 ? imageId.Substring(0, 8) : "";
            string datePath = !string.IsNullOrEmpty(dateFolder)
                ? Path.Combine(basePath, dateFolder)
                : null;

            // Đợi camera ghi file xong (500ms lần đầu)
            Thread.Sleep(200);

            string srcPath = null;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (datePath != null && Directory.Exists(datePath))
                    {
                        srcPath = Directory.EnumerateFiles(datePath, $"*{imageId}*.*")
                            .FirstOrDefault(f => exts.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)));
                    }
                    if (srcPath == null)
                    {
                        srcPath = Directory.EnumerateFiles(basePath, $"*{imageId}*.*", SearchOption.AllDirectories)
                            .FirstOrDefault(f => exts.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)));
                    }
                }
                catch { }

                // Verify file size > 10KB (ảnh thật ~160KB, placeholder ~1-2KB)
                if (srcPath != null)
                {
                    try
                    {
                        var fileInfo = new FileInfo(srcPath);
                        if (fileInfo.Length > 10240)
                        {
                            break; // File hợp lệ
                        }
                        // File quá nhỏ → chưa ghi xong, retry
                        srcPath = null;
                    }
                    catch { }
                }

                if (i < maxRetries - 1)
                    Thread.Sleep(retryDelayMs * (i + 1)); // Tăng dần: 500ms, 1000ms, 1500ms
            }

            if (srcPath == null)
            {
                Console.WriteLine($"[ErrorImage] ✘ Không tìm thấy ảnh {imageId} sau {maxRetries} lần thử");
                return false;
            }

            string destPath = BuildErrorImagePath(imageId);
            if (string.IsNullOrEmpty(destPath)) return false;
            try
            {
                string dir = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.Copy(srcPath, destPath, true);
                Console.WriteLine($"[ErrorImage] ✔ Đã lấy ảnh lỗi: {srcPath} → {destPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ErrorImage] ✘ Copy ảnh lỗi thất bại: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lazy copy: kiểm tra ảnh tồn tại chưa, nếu chưa thì copy từ camera FTP.
        /// Trả về đường dẫn ảnh nếu tìm thấy, null nếu không.
        /// </summary>
        private string EnsureErrorImageExists(string imageId, string jobName = null)
        {
            try
            {
                string destPath = BuildErrorImagePath(imageId, jobName);
                if (File.Exists(destPath))
                    return destPath; // Ảnh đã có

                // Tìm trong camera FTP folder
                string srcPath = FindImageInCameraFolder(imageId);
                if (srcPath != null)
                {
                    string destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);
                    File.Copy(srcPath, destPath, true);
                    Console.WriteLine($"[ErrorImage] ✔ Lazy copy: {srcPath} → {destPath}");
                    return destPath;
                }

                // Không tìm thấy → không hiển thị
                Console.WriteLine($"[ErrorImage] ✘ Không tìm thấy ảnh {imageId} trong camera FTP");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ErrorImage] ✘ Lazy copy lỗi: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tìm file ảnh trong camera FTP folder.
        /// </summary>
        private string FindImageInCameraFolder(string imageId)
        {
            var vscCam = Shared.vscCamera;
            string basePath = vscCam?.FtpImagePath;
            if (string.IsNullOrEmpty(basePath))
                basePath = Shared.Settings?.THFtpImagePath
                    ?? @"D:\hinhanh\VS\Camera\Images";

            string[] exts = { ".png", ".bmp", ".jpg" };
            string dateFolder = imageId.Length >= 8 ? imageId.Substring(0, 8) : "";
            string datePath = !string.IsNullOrEmpty(dateFolder)
                ? Path.Combine(basePath, dateFolder)
                : null;

            // Tìm trong thư mục date trước
            if (datePath != null && Directory.Exists(datePath))
            {
                var found = Directory.EnumerateFiles(datePath, $"*{imageId}*.*")
                    .FirstOrDefault(f => exts.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)));
                if (found != null) return found;
            }

            // Tìm trong thư mục gốc (recursive)
            if (Directory.Exists(basePath))
            {
                var found = Directory.EnumerateFiles(basePath, $"*{imageId}*.*", SearchOption.AllDirectories)
                    .FirstOrDefault(f => exts.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)));
                if (found != null) return found;
            }

            return null;
        }

        private void DownloadPendingImages()
        {
            string currentJob = _SelectedJob?.FileName ?? "";
            Stopwatch swDl = Stopwatch.StartNew();
            while (_pendingErrorImages.TryDequeue(out var item))
            {
                if (swDl.Elapsed.TotalSeconds > 5) break;

                string imageJobName = item.jobName;
                string destPath = BuildErrorImagePath(item.imageId, imageJobName);
                if (string.IsNullOrEmpty(destPath)) continue;

                // Nếu đã có ảnh thì bỏ qua
                if (File.Exists(destPath)) continue;

                // Tìm trong camera FTP và copy
                string srcPath = FindImageInCameraFolder(item.imageId);
                if (srcPath != null)
                {
                    try
                    {
                        string dir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.Copy(srcPath, destPath, true);
                        Console.WriteLine($"[ErrorImage] ✔ Retry copy: {srcPath} → {destPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ErrorImage] ✘ Retry copy lỗi: {ex.Message}");
                    }
                }
                // Nếu không tìm thấy → KHÔNG lưu ảnh đen, bỏ qua
            }
        }

        private async Task<bool> StopProcessAsync(bool interactOnUI = true, string messages = "", bool isClosed = false, bool isManualClose = false, bool showWarning = true)
        {
            try
            {
                bool stopped = await Task.Run(() => StopProcess(interactOnUI, messages, isClosed, isManualClose, showWarning));
                if (stopped)
                    Shared.RaiseOnStopButtonClick();
                return stopped;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public void RequestAutoStop(string reason, bool showWarning = true)
        {
            if (_isStopping) return;
            StopProcessAsync(false, reason, false, false, showWarning);
        }

        private bool StopProcess(bool interactOnUI = true, string messages = "", bool isClosed = false, bool isManualClose = false, bool showWarning = true)
        {
            StopRunLogTimer();
            if (interactOnUI)
            {
                DialogResult dialogResult = DialogResult.None;
                // ── Thread-safe guard: chỉ 1 luồng được hiện dialog ──
                if (Interlocked.CompareExchange(ref _dialogResultStopExist, 1, 0) == 1)
                    return false;

                if (IsDisposed || !IsHandleCreated)
                {
                    _dialogResultStopExist = 0;
                    return false;
                }

                try
                {
                    Invoke(new Action(() =>
                    {
                        try
                        {
                            dialogResult = CustomMessageBox.Show(Lang.DoYouWantToStopTheSystem, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        }
                        catch { /* form đóng giữa chừng */ }
                    }));
                }
                catch
                {
                    _dialogResultStopExist = 0;
                    return false;
                }
                if (dialogResult != DialogResult.Yes)
                {
                    _dialogResultStopExist = 0;
                    return false;
                }
                else
                {
                    Shared.OperStatus = OperationStatus.Stopped;
                }
                _dialogResultStopExist = 0;
                messages = Lang.UserStoppedTheSystem;

            }
            //else
            //{
            //    Shared.OperStatus = OperationStatus.Stopped;
            //}
            else
            {
                Shared.OperStatus = OperationStatus.Stopped;
                if (!string.IsNullOrEmpty(messages) && showWarning)
                {
                    BeginInvoke(new Action(() =>
                    {
                        CustomMessageBox.Show(messages, Lang.Warning,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                }
            }
            // ── Flag chống cascade từ backup thread catch ──
            _isStopping = true;
            try { BeginInvoke(new Action(() => { try { btnCompleteJob.Enabled = false; } catch { } })); } catch { }

            // ── Show loading overlay ──
            BeginInvoke(new Action(() => ShowLoadingOverlay()));
            UpdateLoadingText("Đang dừng máy in...");

            try
            {
                ChangeCheckMode(Checkmode.Camera);
                _VirtualCTS?.Cancel();
                _benchmarkCTS?.Cancel();
                _UICheckedResultCancelTokenSource?.Cancel();
                _OperationCancelTokenSource?.Cancel();
                _BackupSendLogCancelTokenSource?.Cancel();
                _BackupResultCancelTokenSource?.Cancel();
                _BackupImageCancelTokenSource?.Cancel();
                _BackupRSFPLogCancelTokenSource?.Cancel();

                // ── Bước 1: Dừng send loop trước, tránh gửi thêm data vào buffer máy in ──
                KillTThreadSendPODDataToPrinter();
                _printerFeedbackEvent.Set(); // unblock send loop đang chờ RSFP (di chuyển lên đầu)

                // ── Bước 2: STOP máy in ──
                if (Shared.Settings.IsPrinting && _SelectedJob.PrinterSeries)
                {
                    PODController podController = Shared.Settings.PrinterList.Where(p => p.RoleOfPrinter == RoleOfStation.ForProduct).FirstOrDefault().PODController;
                    string stopIp = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                    if (podController != null)
                    {
                        PrinterLogger.Log(_SelectedJob?.FileName ?? "", stopIp, "STOP", "BEGIN",
                            $"sent={_NumberOfSentPrinter} printed={NumberPrinted} pending={_NumberOfSentPrinter - NumberPrinted} operStatus={Shared.OperStatus}",
                            "INFO", "Bắt đầu quy trình dừng máy in");

                        ProjectLogger.WriteInfo($"[CLPB] Sending CLPB before stop. sent={_NumberOfSentPrinter} printed={NumberPrinted} operStatus={Shared.OperStatus}");
                        podController.Send("CLPB");
                        PrinterLogger.Log(_SelectedJob?.FileName ?? "", stopIp, "STOP", "CLPB", "Xoá bộ đệm máy in", "INFO", "Đã gửi lệnh xoá bộ đệm");

                        UpdateLoadingText("Đang đợi máy in phản hồi...");
                        podController.Send("STOP");
                        lock (_StopLocker)
                        {
                            _IsStopOK = false;
                            int countTimeout = 0;
                            Stopwatch stopStopwatch = Stopwatch.StartNew();
                            while (!_IsStopOK && countTimeout < 4)
                            {
                                Monitor.Wait(_StopLocker, 2500);
                                countTimeout++;
                            }
                            stopStopwatch.Stop();
                            PrinterLogger.Log(_SelectedJob?.FileName ?? "", stopIp, "STOP", "RESULT",
                                $"ok={_IsStopOK} waitMs={stopStopwatch.ElapsedMilliseconds} operStatus={Shared.OperStatus}",
                                _IsStopOK ? "OK" : "TIMEOUT",
                                _IsStopOK ? "Máy in xác nhận đã dừng" : "Máy in không phản hồi STOP - mất kết nối");
                        }
                    }
                }

                if (Shared.Settings.PLCVersion == 2)
                {
                    Shared.SensorController.Send("(C00000)");
                }

                // ── Bước 3: Đợi _QueueBufferPrinterResponseData drain (Thread C vẫn chạy) ──
                UpdateLoadingText("Đang đợi xử lý RSFP còn lại...");
                int rsfpDrainMs = 0;
                while (_QueueBufferPrinterResponseData.Count() > 0 && rsfpDrainMs < 5000)
                {
                    Thread.Sleep(50);
                    rsfpDrainMs += 50;
                }
                Thread.Sleep(1000); // chờ RSFP cuối từ socket thread

                // ── Bước 4: Hủy Thread C SAU KHI RSFP queue drain ──
                _PrinterRespontCST?.Cancel();

                // ── Bước 5: Đợi _QueueBufferUpdateUIPrinter drain (Thread B vẫn chạy) ──
              //  UpdateLoadingText("Đang đợi xử lý printed response còn lại...");
                UpdateLoadingText("Đang đợi xử lý dữ liệu phản hồi máy in...");
                int producerDrainMs = 0;
                while (_QueueBufferUpdateUIPrinter.Count() > 0 && producerDrainMs < 5000)
                {
                    Thread.Sleep(50);
                    producerDrainMs += 50;
                }
                Thread.Sleep(1000); // chờ Thread B enqueue nốt items cuối

                // ── Bước 6: Hủy Thread B SAU KHI UI queue drain ──
                _UIPrintedResponseCancelTokenSource?.Cancel();

                // ── Bước 7: Cancel Thread A + enqueue null + đợi consumer ──
                _BackupResponseCancelTokenSource?.Cancel();
                _QueueBufferBackupPrintedCode.Enqueue(null);
                try { if (_backupResponseTask != null) _backupResponseTask.Wait(10000); } catch { }

                // ── Session Summary ──
                ProjectLogger.WriteInfo($"[SUMMARY] job={_SelectedJob?.FileName} sent={_NumberOfSentPrinter} rx={ReceivedCode} printed={NumberPrinted} delta={ReceivedCode - _NumberOfSentPrinter}");

                // ── Cleanup ──
                _QueueBufferPrinterResponseData.Clear();

                // ── Cancel backup token SAU KHI drain xong ──
              //  _BackupResponseCancelTokenSource?.Cancel();

                _TotalMissed = 0;
                // ── Đợi send loop cũ thoát hoàn toàn trước khi reset guard ──
                int waitSendExit = 0;
                while (Interlocked.CompareExchange(ref _isSendLoopRunning, 0, 0) == 1 && waitSendExit < 2000)
                {
                    Thread.Sleep(100);
                    waitSendExit += 100;
                }
                if (waitSendExit >= 2000)
                    ProjectLogger.WriteWarning($"[STOP] Timeout {waitSendExit}ms waiting send loop exit, force reset _isSendLoopRunning");
                Interlocked.Exchange(ref _isSendLoopRunning, 0); // reset guard
                Shared.IsSampled = false;
                while (_QueueBufferDataObtained.TryDequeue(out _)) { }
                while (_QueuePositionDataObtained.TryDequeue(out _)) { }
                _QueueBufferUpdateUIPrinter.Enqueue(null);
                _QueueBufferDataObtainedResult.Enqueue(null);
                _QueueBufferBackupCheckedResult.Enqueue(null);
                // ── Flush printed QR queue ──
                FlushPrintedQrWorker();
                // Drain các queue còn lại
                int drainWait = 0;
                while ((_QueueBufferDataObtainedResult.Count() > 0 || _QueueBufferBackupCheckedResult.Count() > 0) && drainWait < 100)
                {
                    drainWait++;
                    System.Threading.Thread.Sleep(10);
                }
                // Lưu số cuối sau khi drain xong
                _SelectedJob.NumberOfPrintedCodes = NumberPrinted;
                _SelectedJob.PhanLoaiCount = _phanLoaiCount;
                _SelectedJob.LastRunTime = DateTime.Now;
                _SelectedJob.DateCheckPassed = _DateCheckPassed;
                _SelectedJob.DateCheckFailed = _DateCheckFailed;
                try { _SelectedJob.SaveFile(); } catch { }

                if (ProjectLabel.IsTHTrueMilk)
                {
                    try
                    {
                        _checkedDataProcess?.Stop();
                        StopPrintedDataProcess(_printedDataProcess);
                    }
                    catch (Exception)
                    {
                    }

                }
                Shared.OperStatus = OperationStatus.Stopped;
                Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);

                //_ParentForm.ISCamera.StopGetData(); // Stop get data from IS OCR


                string fileName = "";
                if (_SelectedJob.CheckedResultPath != "")
                {
                    fileName = _SelectedJob.CheckedResultPath;
                }
                LoggingController.SaveHistory(
                    string.Format("{0}: {1}", Lang.TotalChecked, TotalChecked),
                    messages,
                    string.Format("{0}: {1}", Lang.ResultFile, fileName),
                    SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                    LoggingType.Stopped);
                if (_SelectedJob.CompareType == CompareType.Database)
                {
                    var completeNum = 0;
                    completeNum = Shared.Settings.TotalCheckEnable ? TotalChecked : NumberOfCheckPassed;
                    if (completeNum >= _TotalCode - _NumberOfDuplicate)
                    {
                        _SelectedJob.JobStatus = JobStatus.Accomplished;
                        _SelectedJob.SaveFile();
                    }
                }

                //if (!interactOnUI)
                //{
                //    countFormStopSuddenly++;
                //    if (countFormStopSuddenly <= 1)
                //    {
                //        DialogResult dialogResult = CustomMessageBox.Show(messages, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    }
                //    NumberOfSentPrinter = 0;
                //    ReceivedCode = 0;
                //}
                // ── Lưu file database CSV ──
                UpdateLoadingText("Đang lưu file database...");
                SaveCurrentDataToCsv();

                // ── RLink log: stop (TRƯỚC KHI reset counter) ──
                StopRunLogTimer();
                //if (_isRlinkStop) // chỉ log "stop" khi user bấm Stop, không log khi Complete
                //    FireRLinkLog("stop");
                //else
                //    FireRLinkLog("error"); // dừng do lỗi / máy in tự dừng / crash
                FireRLinkLog("stop");
                NumberPrinted = 0;
                _sessionPrintedCount = 0;
                _NumberOfSentPrinter = 0;
                _printedBaseSet = false;
                Shared.PrinterDisconnectAlertShown = false;
                Interlocked.Exchange(ref _printedFileCount, 0);
                Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Stop);

                // ── Ẩn overlay ngay sau khi lưu CSV, không chờ download ảnh ──
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        try { HideLoadingOverlay(); } catch { }
                    }));
                }
                catch { }

                // ── Retry copy ảnh lỗi từ camera FTP ──
                if (_pendingErrorImages.Count > 0)
                {
                    BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            UpdateLoadingText("Đang xử lý ảnh lỗi...");
                        }
                        catch { }
                    }));
                    DownloadPendingImages();
                }

                // ── Tắt loading overlay ──
                return true;
            }
            finally
            {
                // ── Dừng timer vận hành (auto-stop hoặc manual stop) ──
                if (RunStopwatch.IsRunning)
                {
                    AccumulatedRunTime += RunStopwatch.Elapsed;
                    RunStopwatch.Reset();
                }
                _runTimeTimer.Stop();
                _SelectedJob.LastRunTime = DateTime.Now;

                // ── Cập nhật lblTimeProcessStop khi dừng (manual + auto-stop) ──
                DateTime stopTime = _SelectedJob.LastRunTime;
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (lblTimeProcessStop == null) return;
                        lblTimeProcessStop.Text = "Kết thúc: " + stopTime.ToString("dd/MM/yyyy HH:mm");
                        lblTimeProcessStop.Visible = true;
                    }));
                }
                catch { }

                // ── Ẩn overlay an toàn, không phụ thuộc btnCompleteJob ──
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (_loadingStop != null)
                                _loadingStop.UpdateMessage("Hoàn tất");
                            System.Threading.Thread.Sleep(200);
                            HideLoadingOverlay();
                        }
                        catch { }
                    }));
                }
                catch { }
                _isStopping = false;
                try { BeginInvoke(new Action(() => { try { if (_isDataLoaded && _SelectedJob?.CompleteJobStatus != CompleteJobStatus.Completed) btnCompleteJob.Enabled = true; } catch { } })); } catch { }
            }
        }
        private void ShowLoadingOverlay()
        {
            if (_loadingStop != null) return;

            // Disable tất cả controls phía sau
            foreach (Control c in Controls)
            {
                if (c is Form) continue;
                c.Enabled = false;
            }

            _loadingStop = frmLoadingUi.ShowLoading(this,
                "DỪNG HỆ THỐNG",
                "Đang xử lý dừng hệ thống...");
        }

        private void UpdateLoadingText(string text)
        {
            if (_loadingStop == null) return;
            _loadingStop.UpdateMessage(text);
        }

        private void HideLoadingOverlay()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HideLoadingOverlay));
                return;
            }

            frmLoadingUi.CloseLoading(ref _loadingStop);

            // Enable lại tất cả controls
            foreach (Control c in Controls)
            {
                c.Enabled = true;
            }
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

        private void LoadABFScores()
        {
            // Count A/B/F from _CheckedResultCodeList (loaded from CheckedResult CSV)
            if (_CheckedResultCodeList == null || _CheckedResultCodeList.Count == 0) return;
            if (Index_ErrorType < 0 || Index_ErrorType >= (_CheckedResultCodeList[0]?.Length ?? 0)) return;
            try
            {
                int a = 0, b = 0, f = 0;
                foreach (var row in _CheckedResultCodeList)
                {
                    if (row == null || row.Length <= Index_ErrorType) continue;
                    string et = row[Index_ErrorType];
                    if (et == "A") a++;
                    else if (et == "B") b++;
                    else if (et == "F") f++;
                }
                _errorCountA = a; _errorCountB = b; _errorCountF = f;
                Invoke(new Action(() =>
                {
                    lblTotalScoreA.Text = a.ToString("N0");
                    lblTotalScoreB.Text = b.ToString("N0");
                    lblTotalScoreF.Text = f.ToString("N0");
                    UpdateConsecutiveErrorDisplay();
                    UpdateErrorRatios();
                }));
            }
            catch { }
        }

        // ── KillThread: deprecated — thread.Abort() gây crash ngẫu nhiên. ──
        //private void KillThread(ref Thread thread)
        //{
        //    if (thread != null && thread.IsAlive)
        //    {
        //        thread.Abort();
        //        thread = null;
        //    }
        //}

        private void KillAllProccessThread()
        {
            _isStopping = true;
            _benchmarkCTS?.Cancel();
            _VirtualCTS?.Cancel();
            _UICheckedResultCancelTokenSource?.Cancel();
            _UIPrintedResponseCancelTokenSource?.Cancel();
            _BackupImageCancelTokenSource?.Cancel();
            _BackupResultCancelTokenSource?.Cancel();
            _BackupResponseCancelTokenSource?.Cancel();
            _BackupSendLogCancelTokenSource?.Cancel();
            _BackupRSFPLogCancelTokenSource?.Cancel();

            // ── CRITICAL: Cancel send loop token + unblock tất cả các wait ──
            _SendDataToPrinterTokenCTS?.Cancel();
            _printerFeedbackEvent.Set(); // unblock send loop đang chờ RSFP (WaitOne)
            ReleaseLocker(); // unblock send loop đang chờ Monitor.Wait (OnProduction mode)
            // Đợi send loop thoát (tối đa 2s)
            int waitSend = 0;
            while (Interlocked.CompareExchange(ref _isSendLoopRunning, 0, 0) == 1 && waitSend < 2000)
            {
                Thread.Sleep(100);
                waitSend += 100;
            }
            if (waitSend >= 2000)
                ProjectLogger.WriteWarning($"[KILL] Timeout {waitSend}ms waiting send loop exit");

            // ── CRITICAL: Stop receive handler + unsubscribe static events NGAY ──
            // Phải làm TRƯỚC KHI tạo form mới, tránh race condition 2 form cùng nhận STAR OK
            _PrinterRespontCST?.Cancel();
            Shared.OnPrinterDataChange -= Shared_OnPrinterDataChange;
            Shared.OnSyncDataParameterChange -= Shared_OnSyncDataParameterChange;
            Shared.OnSyncCheckDataParameterChange -= Shared_OnSyncCheckDataParameterChange;
            _dbStatusSyncTimer?.Stop();
            _dbStatusSyncTimer?.Dispose();
            _dbStatusSyncTimer = null;
            Shared.OnDatabaseStatusChange -= Shared_OnDatabaseStatusChange;

            // Đợi queue xử lý xong trước khi enqueue null để tránh race
            Thread.Sleep(50);
            while (_QueueBufferDataObtained.TryDequeue(out _)) { }
            while (_QueuePositionDataObtained.TryDequeue(out _)) { }

            //_QueueBufferDataObtainedResult.Enqueue(null);
            //_QueueBufferUpdateUIPrinter.Enqueue(null);
            //_QueueBufferBackupImage.Enqueue(null);
            //_QueueBufferBackupCheckedResult.Enqueue(null);
            //_QueueBufferBackupSendLog.Enqueue(null);
            //_QueueBufferBackupRSFPLog.Enqueue(null);


            _QueueBufferDataObtainedResult.Enqueue(null);
            _QueueBufferUpdateUIPrinter.Enqueue(null);
            _QueueBufferBackupImage.Enqueue(null);
            _QueueBufferBackupCheckedResult.Enqueue(null);
            _QueueBufferBackupSendLog.Enqueue(null);
            _QueueBufferBackupRSFPLog.Enqueue(null);
            _QueueBufferBackupPrintedCode.Enqueue(null);


            Thread.Sleep(100); // cho các thread nhận null và break loop

            // ── Đợi backup threads flush xong ──
            try { if (_backupResponseTask != null) _backupResponseTask.Wait(5000); } catch { }
            try { if (_backupSendLogTask != null) _backupSendLogTask.Wait(5000); } catch { }
            try { if (_backupRSFPLogTask != null) _backupRSFPLogTask.Wait(5000); } catch { }

            // ── Drain nốt items còn sót trong printed queue ──
            if (!string.IsNullOrWhiteSpace(_SelectedJob?.PrintedResponePath))
            {
                string printedPath = System.IO.Path.Combine(CommVariables.PathPrintedResponse, _SelectedJob.PrintedResponePath);
                while (_QueueBufferBackupPrintedCode.Count() > 0)
                {
                    var batch = _QueueBufferBackupPrintedCode.Dequeue();
                    if (batch == null || batch.Count == 0) continue;
                    try
                    {
                        using (var sw = new System.IO.StreamWriter(printedPath, true, new System.Text.UTF8Encoding(true)))
                        {
                            foreach (var row in batch)
                                sw.WriteLine(string.Join(",", row.Select(x => Csv.Escape(x))));
                        }
                    }
                    catch { }
                }
            }
        }

        #endregion Operation

        #region Jobs
    
        /// <summary>
        /// Gọi khi frmJobTHTrueMilk phát hiện QR mới sang ngày mới (Mode 1)
        /// hoặc đến giờ đổi QR tự động (Mode 2).
        /// Rebuild CSV + _CodeListPODFormat mà không dừng hệ thống.
        /// </summary>
        private void ParentForm_OnMode1QrCodeChanged(object sender, (string qrCode, DateTime batchDate) e)
        {
            if (string.IsNullOrWhiteSpace(e.qrCode)) return;

            var mode = _SelectedJob?.THJobOperatingMode;
            if (mode != THTrueMilkOperatingMode.BatchOneQrCode &&
                mode != THTrueMilkOperatingMode.BatchOneQrCodeNoChange &&
                mode != THTrueMilkOperatingMode.AutoRefreshByTime)
                return;

            Console.WriteLine($"[Mode1/2] Nhận QR mới: {e.qrCode} — bắt đầu rebuild database...");
            _ = RebuildMode1DatabaseForNewDayAsync(e.qrCode, e.batchDate)
                .ContinueWith(t =>
                {
                    _ParentForm?._rebuildTcs?.TrySetResult(t.Status == TaskStatus.RanToCompletion);
                });
        }

        /// <summary>Số mã đã in thực tế — dùng cho RebuildCsvOnQrSwitch.</summary>
        public int GetPrintedCount()
        {
            return Shared.CurrentJob?.NumberOfPrintedCodes ?? 0;
        }
        private async Task RebuildMode1DatabaseForNewDayAsync(string newQrCode)
        {
            await RebuildMode1DatabaseForNewDayAsync(newQrCode, DateTime.Now);
        }

        private async Task RebuildMode1DatabaseForNewDayAsync(string newQrCode, DateTime batchDate)
        {
            await Task.Run(() =>
            {
                lock (_rebuildLock)
                {
                try
                {
                    var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                    string oldQrCode = virtualList != null ? virtualList.QrCode : "";
                    int bufferCount;
                    int waitingCount = 0;

                    int configBuffer = _SelectedJob?.THJobBufferCount > 0
                        ? _SelectedJob.THJobBufferCount
                        : (Shared.Settings.THBufferCount > 0 ? Shared.Settings.THBufferCount : 0);
                    bufferCount = Math.Max(_printerBuffer, configBuffer);

                    if (!_isStartRebuild && !string.IsNullOrEmpty(oldQrCode) && oldQrCode == newQrCode)
                    {
                        Console.WriteLine("[Mode1 Midnight] Không cần rebuild.");
                        return;
                    }

                    string today = batchDate.ToString("dd MM yy");
                    string hsdDate = batchDate.AddDays((_SelectedJob?.THJobExpiryMonths ?? 6) - 1).ToString("dd MM yy");
                    string pid = _SelectedJob?.THJobProductId ?? "";

                    if (virtualList != null)
                    {
                            //Nhan.to update 05/09/2026
                            //Nguyên nhân: Giải thuật update qr vs nsx/ hsd
                            //=> nếu thời gian gửi code<thời gian xử lý 2 hàm updateqr vs updatennsxhsx
                            //=> gửi qr nhưng lệch ngày
                            //số hộp có thể lỗi: 1 hộp


                            //virtualList.UpdateQrCode(newQrCode, _isStartRebuild);
                            //virtualList.UpdateNsxHsd(today, hsdDate, _isStartRebuild);

                            //Nhan.To_070926_Sua_updateAllWaiting_de_buffer_rows_giu_QR_NSX_HSD_cu_khong_match_fail
                            // Cold start (_isStartRebuild=true): update tất cả Waiting rows
                            // Midnight reset (_isStartRebuild=false): chỉ update hàng chưa gửi → buffer giữ QR/NSX/HSD cũ → printer in xong → match OK
                            virtualList.UpdateQrCodeAndNsxHsd(newQrCode, today, hsdDate, _isStartRebuild);



                     }
                        else
                    {
                        // ── Fallback: List<string[]> (iterate nếu cần) ──
                        lock (_SyncObjCodeList)
                        {
                            int bufferSkipped = 0;
                            int firstNewDayIdx = -1;
                            var list = _PrintedCodeObtainFromFile as List<string[]>;
                            if (list == null) return;

                            SafeInvoke(this, () => _loadingRebuild?.UpdateMessage($"Đang rebuild {list.Count:N0} mã..."));
                            for (int i = 0; i < list.Count; i++)
                            {
                                var row = list[i];
                                if (row.Length > 2 && row[1] == "Waiting")
                                {
                                    // Start job: không giữ buffer (printer đã CLPB) → update tất cả Waiting
                                    // Midnight reset: giữ bufferCount dòng đầu (printer còn buffer)
                                    if (!_isStartRebuild && bufferSkipped < bufferCount)
                                    {
                                        bufferSkipped++;
                                    }
                                    else
                                    {
                                        row[2] = newQrCode;
                                        if (row.Length > 3)
                                        {
                                            row[row.Length - 2] = today;
                                            row[row.Length - 1] = hsdDate;
                                        }
                                        waitingCount++;
                                        if (firstNewDayIdx < 0) firstNewDayIdx = i;
                                    }
                                }
                            }
                            // Cập nhật dict so sánh (nếu còn dùng cho Mode khác)
                            if (bufferSkipped == 0)
                            {
                                _CodeListPODFormat.TryRemove(oldQrCode, out _);
                            }
                            else
                            {
                                int firstBufferIdx = list.FindIndex(r => r.Length > 2 && r[1] == "Waiting" && r[2] == oldQrCode);
                                if (firstBufferIdx >= 0)
                                    _CodeListPODFormat[oldQrCode] = new CompareStatus(firstBufferIdx, false);
                            }
                            if (firstNewDayIdx >= 0)
                                _CodeListPODFormat.TryAdd(newQrCode, new CompareStatus(firstNewDayIdx, false));
                            Console.WriteLine($"[Mode1 Midnight] List update {waitingCount} dòng | Buffer giữ: {bufferSkipped}");
                        }
                    }

                    // ── Ghi đè lên CSV ─────────────────
                    SafeInvoke(this, () => _loadingRebuild?.UpdateMessage($"Đang lưu file CSV ({waitingCount:N0} mã)..."));
                    SaveCurrentDataToCsv();
                    _SelectedJob.SaveFile();

                    SafeInvoke(this, () =>
                    {
                        _loadingRebuild?.UpdateMessage("Hoàn tất rebuild!");
                        dgvDatabase.Invalidate();
                        if (_isStartRebuild)
                        {
                            _deferredAlertMessage =
                                $"Mode 1: QR mới → {newQrCode}\nCập nhật {waitingCount} mã | Buffer: {bufferCount} mã";
                        }
                        else
                        {
                            CuzAlert.Show(
                                $"Mode 1: QR mới → {newQrCode}\nCập nhật {waitingCount} mã | Buffer: {bufferCount} mã",
                                Alert.enmType.Warning,
                                new Size(500, 120),
                                new Point(Location.X, Location.Y),
                                Size, false);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RebuildMode1Database] Lỗi: {ex.Message}");
                    ProjectLogger.WriteError("[RebuildMode1Database] " + ex.Message);
                }
                } // end lock (_rebuildLock)
            });
        }

        private void SaveCurrentDataToCsv()
        {
            try
            {
                string path = _SelectedJob?.DirectoryDatabase;
                if (string.IsNullOrEmpty(path)) return;

                // ── DEBUG LOG ──
                var caller = new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod()?.Name ?? "?";
                var firstRow = _PrintedCodeObtainFromFile?.Count > 0 ? _PrintedCodeObtainFromFile[0] : null;
                string qr = firstRow?.Length > 2 ? firstRow[2] : "?";
                string nsx = firstRow?.Length > 3 ? firstRow[3] : "?";
                string hsd = firstRow?.Length > 4 ? firstRow[4] : "?";
                string logDir = CommVariables.PathProgramDataApp + "Logs\\";
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                File.AppendAllText(logDir + "SaveCsvDebug.log",
                    $"[{DateTime.Now:HH:mm:ss.fff}] [SaveCsv] caller={caller} QR={qr} NSX={nsx} HSD={hsd} rows={_PrintedCodeObtainFromFile?.Count}{Environment.NewLine}");
                // ── END DEBUG LOG ──

                var delimiter = path.EndsWith(".csv") ? "," : "\t";

                //lock (_SyncObjCodeList)
                //{
                //    using (var sw = new StreamWriter(path, false, Encoding.UTF8))
                //    {
                //        // Header line
                //        if (_SelectedJob.IsFirstRowHeader && _DatabaseColunms != null && _DatabaseColunms.Length > 2)
                //        {
                //            var headerFields = _DatabaseColunms.Skip(2).Take(3).Select(x => Csv.Escape(x ?? ""));
                //            sw.WriteLine(string.Join(delimiter, headerFields));
                //        }

                //        // Data lines — ghi tất cả data columns (bỏ STT, Status), không ghi timestamps
                //        foreach (var row in _PrintedCodeObtainFromFile)
                //        {
                //            if (row.Length <= 2) continue;
                //            // row có thể là 7 cột (STT, Status, QR..., NSX, HSD) — chỉ ghi data, không ghi timestamp
                //            int dataCols = row.Length - 2; // bỏ STT và Status
                //            // Nếu là PrintedCodeVirtualList, row.Length = 7 (display), nhưng data chỉ có 5 cột
                //            // Lấy từ cột 2 đến cột cuối - 2 (bỏ timestamp T.Gửi, T.In nếu có)
                //            int endDataCol = row.Length;
                //            if (row.Length >= 7 && _PrintedCodeObtainFromFile is PrintedCodeVirtualList)
                //                endDataCol = row.Length - 2; // bỏ 2 cột timestamp cuối (T.Gửi, T.In)
                //            var dataFields = row.Skip(2).Take(endDataCol - 2).Select(x => Csv.Escape(x ?? ""));
                //            sw.WriteLine(string.Join(delimiter, dataFields));
                //        }
                //    }
                //}


                //Nhan.To_070926_VirtualList_ghi_truc_tiep_khong_snapshot_giam_100MB_memory_cho_1M_rows
                // ── VirtualList: ghi trực tiếp (không snapshot, không lock, không GC giật) ──
                bool writeHeader = _SelectedJob.IsFirstRowHeader
                    && _DatabaseColunms != null
                    && _DatabaseColunms.Length > 2;
                bool isVirtualList = _PrintedCodeObtainFromFile is PrintedCodeVirtualList;

                if (isVirtualList)
                {
                    var vl = (PrintedCodeVirtualList)_PrintedCodeObtainFromFile;
                    string[] csvCols = _DatabaseColunms?.Skip(2).Take(3).ToArray();
                    vl.WriteCsvDirect(path, delimiter, csvCols, writeHeader);
                }
                else
                {
                    // ── Fallback List<string[]>: lock brief snapshot ──
                    List<string[]> snapshot;
                    lock (_SyncObjCodeList)
                    {
                        snapshot = new List<string[]>(_PrintedCodeObtainFromFile.Count);
                        for (int i = 0; i < _PrintedCodeObtainFromFile.Count; i++)
                        {
                            var row = _PrintedCodeObtainFromFile[i];
                            var copy = new string[row.Length];
                            Array.Copy(row, copy, row.Length);
                            snapshot.Add(copy);
                        }
                    }

                    using (var sw = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        if (writeHeader)
                        {
                            var headerFields = _DatabaseColunms.Skip(2).Take(3).Select(x => Csv.Escape(x ?? ""));
                            sw.WriteLine(string.Join(delimiter, headerFields));
                        }
                        foreach (var row in snapshot)
                        {
                            if (row.Length <= 2) continue;
                            var dataFields = row.Skip(2).Take(row.Length - 2).Select(x => Csv.Escape(x ?? ""));
                            sw.WriteLine(string.Join(delimiter, dataFields));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveCurrentDataToCsv] Lỗi: {ex.Message}");
                ProjectLogger.WriteError("[SaveCurrentDataToCsv] " + ex.Message);
            }
        }

        private void UpdatePrintedRowInCsv(int rowIndex)
        {
            try
            {
                string path = _SelectedJob?.DirectoryDatabase;
                if (string.IsNullOrEmpty(path)) return;
                if (!File.Exists(path)) return;

                var delimiter = path.EndsWith(".csv") ? "," : "\t";

                lock (_SyncObjCodeList)
                {
                    if (rowIndex < 0 || rowIndex >= _PrintedCodeObtainFromFile.Count) return;
                    var row = _PrintedCodeObtainFromFile[rowIndex];
                    if (row.Length <= 2) return;

                    var allLines = File.ReadAllLines(path, Encoding.UTF8).ToList();
                    bool hasHeader = _SelectedJob.IsFirstRowHeader && _DatabaseColunms != null && _DatabaseColunms.Length > 2;

                    int dataLineIndex = rowIndex + (hasHeader ? 1 : 0);
                    if (dataLineIndex < 0 || dataLineIndex >= allLines.Count) return;

                    int dataCols = row.Length - 2;
                    if (row.Length >= 7 && _PrintedCodeObtainFromFile is PrintedCodeVirtualList)
                        dataCols = row.Length - 4; // bỏ 2 cột timestamp cuối
                    var dataFields = row.Skip(2).Take(dataCols).Select(x => Csv.Escape(x ?? ""));
                    allLines[dataLineIndex] = string.Join(delimiter, dataFields);

                    File.WriteAllLines(path, allLines, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdatePrintedRowInCsv] Lỗi: {ex.Message}");
                ProjectLogger.WriteError("[UpdatePrintedRowInCsv] " + ex.Message);
            }
        }

        private async Task UpdateJobInfomationInterface()
        {
            if (Shared.JobNameSelected == null || Shared.JobNameSelected == "")
            {
                return;
            }

            _SelectedJob = Shared.GetJob(Shared.JobNameSelected);
            lblStatusPrinter01.Visible = _SelectedJob.PrinterSeries;

            if (_SelectedJob != null)
            {
                _IsAfterProductionMode = _SelectedJob.JobType == JobType.AfterProduction ? true : false;
                _IsOnProductionMode = _SelectedJob.JobType == JobType.OnProduction ? true : false;
                _IsVerifyAndPrintMode = _SelectedJob.JobType == JobType.VerifyAndPrint ? true : false;

                _TotalCode = 0;
                NumberOfSentPrinter = 0;
                ReceivedCode = 0;
                NumberPrinted = 0;
                _sessionPrintedCount = 0;
                _printedBaseSet = false;
                Interlocked.Exchange(ref _printedFileCount, 0);
                Shared.PrinterDisconnectAlertShown = false;
                _printerBuffer = 0;
                _lastRsfpReceived = 0;
                _lastPrintedPageFormPrinter = 0;

                // ── Debug log: check job status khi load ──
                if (_PrintedCodeObtainFromFile != null && _PrintedCodeObtainFromFile.Count > 0)
                {
                    int printedCount = _PrintedCodeObtainFromFile.Count(r => r.Length > 1 && r[1] == "Printed");
                    int waitingCount = _PrintedCodeObtainFromFile.Count(r => r.Length > 1 && r[1] == "Waiting");
                    ProjectLogger.WriteInfo($"[JOB_LOAD] total={_PrintedCodeObtainFromFile.Count} printed={printedCount} waiting={waitingCount} fileName={_SelectedJob?.FileName}");
                }
                if (!string.IsNullOrEmpty(_SelectedJob?.PrintedResponePath))
                {
                    string printedPath = CommVariables.PathPrintedResponse + _SelectedJob.PrintedResponePath;
                    bool exists = System.IO.File.Exists(printedPath);
                    ProjectLogger.WriteInfo($"[JOB_REOPEN] PrintedResponse exists={exists} path={_SelectedJob.PrintedResponePath}");
                    if (exists && _PrintedCodeObtainFromFile != null)
                    {
                        var printedQrSet = new HashSet<string>();
                        try
                        {
                            foreach (var line in System.IO.File.ReadLines(printedPath).Skip(1))
                            {
                                var cols = line.Split(',');
                                if (cols.Length > 2) printedQrSet.Add(cols[2].Trim('"'));
                            }
                        }
                        catch { }
                        int marked = 0;
                        for (int i = 0; i < _PrintedCodeObtainFromFile.Count; i++)
                        {
                            var row = _PrintedCodeObtainFromFile[i];
                            if (row.Length > 2 && printedQrSet.Contains(row[2]) && row[1] == "Waiting")
                            {
                                row[1] = "Printed";
                                marked++;
                            }
                        }
                        if (marked > 0)
                            ProjectLogger.WriteInfo($"[JOB_REOPEN] Marked {marked} codes as Printed from previous session");
                    }
                }

                // Reset runtime về giá trị lưu trong job
                if (RunStopwatch.IsRunning) RunStopwatch.Stop();
                _runTimeTimer?.Stop();
                AccumulatedRunTime = TimeSpan.FromTicks(_SelectedJob.TotalRunTimeTicks);
                lblRunningTime.Text = FormatRunTime(AccumulatedRunTime);

                if (_SelectedJob.FirstRunTime > DateTime.MinValue)
                {
                    lblTimeProcessStart.Text = $"Bắt đầu: " + _SelectedJob.FirstRunTime.ToString("dd/MM/yyyy HH:mm");
                    lblTimeProcessStart.Visible = true;
                }
                else
                    lblTimeProcessStart.Text = $"Bắt đầu: -";

                if (_SelectedJob.LastRunTime > DateTime.MinValue)
                {
                    lblTimeProcessStop.Text = $"Kết thúc: " + _SelectedJob.LastRunTime.ToString("dd/MM/yyyy HH:mm");
                    lblTimeProcessStop.Visible = true;
                }
                else
                    lblTimeProcessStop.Text = $"Kết thúc: -";

                TotalChecked = 0;
                NumberOfCheckPassed = 0;
                NumberOfCheckFailed = 0;
                _uiInvalidateCounter = 0;
                _progressBarCounter = 0;
                _lastSelectedRow = -1;

                ProgressBarInitialize();
                ProgressBarCheckedUpdate();
                UpdateJobInfo(_SelectedJob);
                UpdateJobInfoDisplay(_SelectedJob);
                EnableUIComponentWhenLoadData(false);
                btnStop.Enabled = false;
                btnTrigger.Enabled = false;
                pnlMenu.Enabled = false;
             
                _CheckedResultCodeList.Clear();
                _PrintedCodeObtainFromFile.Clear();
                _CodeListPODFormat.Clear();
                _SentPrintedCodeObtainFromFile?.Clear();
                _SentCheckedCodeObtainFromFile?.Clear();
                _errorCountA = _errorCountB = _errorCountF = _phanLoaiCount = 0;
                ErrorCountA = ErrorCountB = ErrorCountF = PhanLoaiCount = 0;
                _lastCameraNsxWithTime = "";
                _lastDateForNsxHsd = DateTime.Now.Date;
                while (_pendingErrorImages.TryDequeue(out _)) { }

                InitDataAsync(_SelectedJob);

                // ── Đổi program camera Keyence cho khớp với job ──
                var (camOk, camMessage) = await ChangeCameraProgramFromJobAsync(_SelectedJob);
                if (!camOk && _SelectedJob.PrinterSeries && Shared.Settings.CheckAllWhenStart)
                {
                    CuzMessageBox.Show(camMessage, "Cảnh báo camera", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
        }

        public async Task<(bool success, string message)> ChangeCameraProgramFromJobAsync(JobModel job)
        {
            if (job == null) return (false, "Chưa chọn job");
            string camModel = job.THJobCameraModelForTraining ?? "";
            if (camModel.Length < 4) return (false, "Thiếu thông tin camera program trong job");

            if (!int.TryParse(camModel.Substring(0, 4), out int progNo) || progNo < 0)
                return (false, "Program number không hợp lệ");

            if (Shared.vscCamera == null || !Shared.vscCamera.IsConnected())
                return (false, "Camera Keyence chưa kết nối");

            var (success, message) = await Shared.vscCamera.ChangeProgramAsync(progNo);
            if (success)
                ProjectLogger.WriteInfo($"[Camera] {message} cho job '{job.FileName}'");
            else
                ProjectLogger.WriteWarning($"[Camera] {message} cho job '{job.FileName}'");

            return (success, message);
        }

        private int CalculateCurrentPage(int totalCode, int maxDatabaseLine, int firstWaiting)
        {
            if (totalCode <= 0 || maxDatabaseLine <= 0)
            {
                return 0;
            }

            if (firstWaiting > 0)
            {
                return (firstWaiting - 1) / maxDatabaseLine;
            }
            else if (firstWaiting == 0)
            {
                return 0;
            }
            else
            {
                return (totalCode - 1) / maxDatabaseLine;
            }
        }
        //public static List<string[]> ReadPrintedCodeData(string fullFilePath, char delimiter = ',')
        //{
        //    var result = new List<string[]>();

        //    if (string.IsNullOrWhiteSpace(fullFilePath) || !File.Exists(fullFilePath))
        //        return result;

        //    try
        //    {
        //        foreach (var line in File.ReadLines(fullFilePath))
        //        {
        //            if (!string.IsNullOrWhiteSpace(line))
        //            {
        //                var values = line.Split(delimiter);
        //                result.Add(values);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error reading file at {fullFilePath}: {ex.Message}");
        //    }

        //    return result;
        //}
        private async void InitDataAsync(JobModel jobModel)
{
    _isDataLoaded = false;
    _BigSTW.Start();
    Stopwatch stw = Stopwatch.StartNew();
    Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " Start init data on thread " + Environment.CurrentManagedThreadId);

    try
    {
        if (jobModel.CompareType == CompareType.Database)
        {
            #region Sent Printed Data
            string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
            string sentCheckDataPath = CommVariables.PathSentDataChecked + _SelectedJob.CheckedResultPath;
            await Task.Run(() =>
            {
                _SentPrintedCodeObtainFromFile = FileFuncs.ReadCodeData(sentDataPath);
                _SelectedJob.NumberOfSaaSSentCodes = SaaSSuccess = _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 && item[4].Equals("success", StringComparison.OrdinalIgnoreCase));
                _SelectedJob.NumberOfSAPSentCodes = SAPSuccess = _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 && item[5].Equals("success", StringComparison.OrdinalIgnoreCase));

                _SentCheckedCodeObtainFromFile = FileFuncs.ReadCodeData(sentCheckDataPath);
                _SelectedJob.NumberOfCheckSaaSSentCodes = CheckSaaSSuccess = _SentCheckedCodeObtainFromFile.Count(item => item.Length > 4 && item[4].Equals("success", StringComparison.OrdinalIgnoreCase));
                _SelectedJob.NumberOfCheckSAPSentCodes = CheckSAPSuccess = _SentCheckedCodeObtainFromFile.Count(item => item.Length > 5 && item[5].Equals("success", StringComparison.OrdinalIgnoreCase));
            });
            #endregion

            Task<IList<string[]>> databaseTsk = InitDatabaseAndPrintedStatusAsync(jobModel);
            Task<List<string[]>> checkedResultTsk = InitCheckedResultDataAsync(jobModel);
            await Task.WhenAll(databaseTsk, checkedResultTsk);

            string checkInitDataMessage = CheckInitDataErrorAndGenerateMessage();
            if (checkInitDataMessage != "")
            {
                foreach (string value in checkInitDataMessage.Split('\n'))
                {
                    if (value != "")
                        CuzAlert.Show(value, Alert.enmType.Error, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                }
            }
            else
            {
                _PrintedCodeObtainFromFile = databaseTsk.Result;
                _CheckedResultCodeList = checkedResultTsk.Result;
                if (_PrintedCodeObtainFromFile.Count() > 0)
                {
                    var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                    if (virtualList != null)
                    {
                        // Mode 1/2: PrintedCodeVirtualList không có header row
                        // _DatabaseColunms có 7 cột: STT, Trạng thái, QR, NSX, HSD, T.Gửi, T.In
                        // dgvDatabase chỉ hiển thị 5 cột (ẩn T.Gửi, T.In)
                        _DatabaseColunms = new string[virtualList.DisplayFieldCount];
                        _DatabaseColunms[0] = "STT";
                        _DatabaseColunms[1] = "Trạng thái";
                        for (int i = 2; i < virtualList.FieldCount - 2; i++) _DatabaseColunms[i] = "QR";
                        _DatabaseColunms[virtualList.FieldCount - 2] = "NSX";
                        _DatabaseColunms[virtualList.FieldCount - 1] = "HSD";
                        _DatabaseColunms[virtualList.FieldCount] = "Thời gian gửi";
                        _DatabaseColunms[virtualList.FieldCount + 1] = "Thời gian in";

                        // Giải phóng ngay vùng nhớ tạm (List<string[]> 3 triệu dòng) sau khi chuyển sang VirtualList
                        GC.Collect(2, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();
                    }
                    else
                    {
                        _DatabaseColunms = _PrintedCodeObtainFromFile[0];
                        (_PrintedCodeObtainFromFile as List<string[]>)?.RemoveAt(0);
                    }

                    if (_SelectedJob.CompareType == CompareType.Database)
                        await InitCompareDataAsync(_PrintedCodeObtainFromFile, _CheckedResultCodeList);

                    _TotalCode = _PrintedCodeObtainFromFile.Count();
                    Shared.TotalCodes = _SelectedJob.NumberOfNeededSentCodes = _TotalCode;
                    _dynamicStopCond = _TotalCode - _NumberOfDuplicate;
                    UpdateErrorRatios();

                    // Tính toán thống kê trên background thread
                    int printedCount = 0, firstWaitingIdx = 0;
                    int checkPassed = 0, totalChecked = _CheckedResultCodeList.Count;
                    await Task.Run(() =>
                    {
                        if (virtualList != null)
                        {
                            printedCount = virtualList.GetPrintedCount();
                            firstWaitingIdx = virtualList.FindNextWaiting();
                            if (firstWaitingIdx < 0) firstWaitingIdx = 0;
                        }
                        else
                        {
                            var list = _PrintedCodeObtainFromFile as List<string[]>;
                            if (list != null)
                            {
                                printedCount = list.Count(x => x[1] == "Printed");
                                int found = list.FindIndex(x => x[1] == "Waiting");
                                firstWaitingIdx = found >= 0 ? found : 0;
                            }
                        }
                        checkPassed = _CheckedResultCodeList.Count(x => x.Length > 1 && x[1] == "Valid");
                    });

                    // ── Fallback: đếm dòng từ file PrintedResponse trực tiếp ──
                    int fileLineCount = 0;
                    try
                    {
                        string backupPath = CommVariables.PathPrintedResponse + _SelectedJob.PrintedResponePath;
                        if (File.Exists(backupPath))
                        {
                            using (var r = new StreamReader(backupPath, Encoding.UTF8, true))
                            {
                                r.ReadLine(); // skip header
                                while (r.ReadLine() != null) fileLineCount++;
                            }
                        }
                    }
                    catch { }
                    int prevSaved = _SelectedJob.NumberOfPrintedCodes;
                    int count = Math.Max(printedCount, Math.Max(prevSaved, fileLineCount));
                    _sessionStartCount = count;
                    _SelectedJob.NumberOfPrintedCodes = NumberPrinted = count;
                    string ipInit = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                    PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipInit, "PERF", "INIT_COUNT",
                        $"printedCount={printedCount} saved={prevSaved} fileLines={fileLineCount} result={NumberPrinted} checked={totalChecked}",
                        "INFO", "Khởi tạo số lượng đã in từ file backup và job");
                    TotalChecked = totalChecked;
                    NumberOfCheckPassed = checkPassed;
                    NumberOfCheckFailed = totalChecked - checkPassed;
                    _DateCheckPassed = _SelectedJob?.DateCheckPassed ?? 0;
                    _DateCheckFailed = _SelectedJob?.DateCheckFailed ?? 0;
                    UpdateSummaryTable();

                    _CurrentPage = CalculateCurrentPage(_TotalCode, _MaxDatabaseLine, firstWaitingIdx);

                    // ── UI updates ──
                    numberOfCode.Text = (_SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                        || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                        || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime)
                        ? _SelectedJob.NumberTotalsCode.ToString("N0") +" hộp"
                        : _TotalCode.ToString("N0")+"hộp";

                    InitDataGridView(dgvDatabase, _DatabaseColunms, 1, true);
                    // Mode 1/2: ẩn 2 cột timestamp cuối (T.Gửi, T.In) trên dgvDatabase
                    // nhưng vẫn giữ trong _DatabaseColunms để FrmPreviewDatabaseTHTrueMilk hiển thị đầy đủ
                    if (virtualList != null && dgvDatabase.Columns.Count >= 2)
                    {
                        dgvDatabase.Columns[dgvDatabase.Columns.Count - 2].Visible = false;
                        dgvDatabase.Columns[dgvDatabase.Columns.Count - 1].Visible = false;
                    }
                    var lastCode = _PrintedCodeObtainFromFile[_PrintedCodeObtainFromFile.Count() - 1];
                    AutoResizeColumnWith(dgvDatabase, lastCode, _DatabaseColunms.Length - 1);
                    dgvDatabase.RowCount = _TotalCode > _MaxDatabaseLine ? _MaxDatabaseLine : _TotalCode;
                    dgvDatabase.Invalidate();

                    if (_NumberOfDuplicate > 0)
                        CuzAlert.Show(Lang.DuplicateDataMessage.Replace("NN", "" + _NumberOfDuplicate) + Lang.PODFormat, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), Size, false);
                }
            }
        }
        else
        {
            _CheckedResultCodeList = await InitCheckedResultDataAsync(jobModel);
            TotalChecked = _CheckedResultCodeList.Count();
            NumberOfCheckPassed = _CheckedResultCodeList.Count(x => x.Length > 1 && x[1] == "Valid");
            NumberOfCheckFailed = TotalChecked - NumberOfCheckPassed;
            _DateCheckPassed = _SelectedJob?.DateCheckPassed ?? 0;
            _DateCheckFailed = _SelectedJob?.DateCheckFailed ?? 0;
            UpdateSummaryTable();
        }

        // Load saved A/B/F scores
        LoadABFScores();

        // Load saved PhanLoaiCount
        if (_SelectedJob != null)
            PhanLoaiCount = _SelectedJob.PhanLoaiCount;

        UpdatePhanLoaiRemaining();

        InitDataGridView(dgvCheckedResult, _ColumnNames, 1);
        if (dgvCheckedResult.Columns.Count > Index_FrameInfo && Index_FrameInfo >= 0)
            dgvCheckedResult.Columns[Index_FrameInfo].Visible = false;
        foreach (DataGridViewColumn col in dgvCheckedResult.Columns)
        {
            col.MinimumWidth = 50;
            switch (col.Index)
            {
                case 0: col.Width = 55; break;
                case 1: col.Width = 110; break;
                case 2: col.Width = 500; break;
                case 3: col.Width = 120; break;
                case 4: col.Width = 120; break;
                case 5: col.Width = 120; break;
                case 6: col.Width = 130; break;
                case 7: col.Width = 190; break;
                case 9: col.Width = 80; break;
                default: col.Width = 100; break;
            }
        }
        // ── Defer AdjustColumnWidthsToFitContent để UI không bị đứng ──
        BeginInvoke(new Action(() =>
        {
            DataGridViewCustom.AdjustColumnWidthsToFitContent(dgvDatabase);
            // ── Đảm bảo cột STT đủ rộng cho 7 chữ số (1,234,567) ──
            if (dgvDatabase.Columns.Count > 0)
                dgvDatabase.Columns[0].MinimumWidth = dgvDatabase.Columns[0].Width = 90;
            ProgressBarInitialize();
            ProgressBarCheckedUpdate();
            prBarCheckPassed.Invalidate();
            dgvCheckedResult.Invalidate();
            // ── Đảm bảo cột STT + Chất lượng đủ rộng ──
            if (dgvCheckedResult.Columns.Count > 0)
                dgvCheckedResult.Columns[0].MinimumWidth = dgvCheckedResult.Columns[0].Width = 90;
            if (Index_ErrorType >= 0 && Index_ErrorType < dgvCheckedResult.Columns.Count)
                dgvCheckedResult.Columns[Index_ErrorType].MinimumWidth = 120;
        }));

        stw.Stop();
        Debug.WriteLine("Init completed, it took " + stw.ElapsedMilliseconds);
        _isDataLoaded = true;
    }
    catch (Exception ex)
    {
        Debug.WriteLine("InitDataAsync exception: " + ex.Message);
        Shared.RaiseOnLogError(ex);
    }
            finally
            {
                // ── BeginInvoke: không blocking, không deadlock ──────────────
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(new Action(() =>
                    {
                        pnlMenu.Enabled = true;
                        EnableUIComponentWhenLoadData(true);
                    }));
                }
            }
        }
        //private async void InitDataAsync(JobModel jobModel)
        //{
        //    _BigSTW.Start();
        //    Stopwatch stw = Stopwatch.StartNew();
        //    Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " Start init data on thread " + Environment.CurrentManagedThreadId);

        //    if (jobModel.CompareType == CompareType.Database)
        //    {
        //        #region Sent Printed Data
        //        string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
        //        _SentPrintedCodeObtainFromFile = FileFuncs.ReadCodeData(sentDataPath);
        //        _SelectedJob.NumberOfSaaSSentCodes = SaaSSuccess = _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 && item[4].Equals("success", StringComparison.OrdinalIgnoreCase));
        //        _SelectedJob.NumberOfSAPSentCodes = SAPSuccess = _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 && item[5].Equals("success", StringComparison.OrdinalIgnoreCase));

        //        string sentCheckDataPath = CommVariables.PathSentDataChecked + _SelectedJob.CheckedResultPath;
        //        _SentCheckedCodeObtainFromFile = FileFuncs.ReadCodeData(sentCheckDataPath);
        //        _SelectedJob.NumberOfCheckSaaSSentCodes = CheckSaaSSuccess = _SentCheckedCodeObtainFromFile.Count(item => item.Length > 4 && item[4].Equals("success", StringComparison.OrdinalIgnoreCase));
        //        _SelectedJob.NumberOfCheckSAPSentCodes = CheckSAPSuccess = _SentCheckedCodeObtainFromFile.Count(item => item.Length > 5 && item[5].Equals("success", StringComparison.OrdinalIgnoreCase));

        //        #endregion

        //        Task<List<string[]>> databaseTsk = InitDatabaseAndPrintedStatusAsync(jobModel); //Load database and update printed status
        //        Task<List<string[]>> checkedResultTsk = InitCheckedResultDataAsync(jobModel); //Load checked result
        //        await Task.WhenAll(databaseTsk, checkedResultTsk); // Waiting until database and checked result completed load

        //        string checkInitDataMessage = "";
        //        checkInitDataMessage = CheckInitDataErrorAndGenerateMessage();
        //        if (checkInitDataMessage != "")
        //        {

        //            foreach (string value in checkInitDataMessage.Split('\n'))
        //            {
        //                if (value != "")
        //                {
        //                    CuzAlert.Show(value, Alert.enmType.Error, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
        //                }
        //            }
        //        }
        //        else
        //        {

        //            _PrintedCodeObtainFromFile = databaseTsk.Result;
        //            _CheckedResultCodeList = checkedResultTsk.Result;
        //            if (_PrintedCodeObtainFromFile.Count() > 1)  // Inititalize database information
        //            {
        //                _DatabaseColunms = _PrintedCodeObtainFromFile[0];
        //                _PrintedCodeObtainFromFile.RemoveAt(0);

        //                if (_SelectedJob.CompareType == CompareType.Database) // Initialize compare data
        //                {
        //                    await InitCompareDataAsync(_PrintedCodeObtainFromFile, _CheckedResultCodeList); // Waiting until initialize compare data completed
        //                }

        //                _TotalCode = _PrintedCodeObtainFromFile.Count();
        //                Shared.TotalCodes = _SelectedJob.NumberOfNeededSentCodes = _TotalCode;

        //                numberOfCode.Text = _TotalCode.ToString();

        //                _SelectedJob.NumberOfPrintedCodes = NumberPrinted = _PrintedCodeObtainFromFile.Where(x => x[1] == "Printed").Count();
        //                int firstWaiting = _PrintedCodeObtainFromFile.IndexOf(_PrintedCodeObtainFromFile.Find(x => x[1] == "Waiting"));  // Identify datas need to display by first waiting code
        //                _CurrentPage = CalculateCurrentPage(_TotalCode, _MaxDatabaseLine, firstWaiting);
        //                InitDataGridView(dgvDatabase, _DatabaseColunms, 1, true); //// Implement virtual mode for DataGridView display database
        //                var lastCode = _PrintedCodeObtainFromFile[_PrintedCodeObtainFromFile.Count() - 1];// Adjust width of columns
        //                AutoResizeColumnWith(dgvDatabase, lastCode, _DatabaseColunms.Length - 1);
        //                dgvDatabase.RowCount = _TotalCode > _MaxDatabaseLine ? _MaxDatabaseLine : _TotalCode; // Define number of DataGridView row
        //                dgvDatabase.Invalidate(); // Update both of DataGridView

        //                if (_NumberOfDuplicate > 0)
        //                {

        //                    CuzAlert.Show(Lang.DuplicateDataMessage.Replace("NN", "" + _NumberOfDuplicate) + Lang.PODFormat, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), Size, false);
        //                }

        //            }
        //        }
        //    }
        //    else
        //    {
        //        _CheckedResultCodeList = await InitCheckedResultDataAsync(jobModel);  // Load checked result
        //    }


        //    TotalChecked = _CheckedResultCodeList.Count();
        //    NumberOfCheckPassed = _CheckedResultCodeList.Where(x => x[2] == "Valid").Count();
        //    NumberOfCheckFailed = TotalChecked - NumberOfCheckPassed;

        //    InitDataGridView(dgvCheckedResult, _ColumnNames, 2);  // Implement virtual mode for DataGridView display checked results
        //    await Task.Delay(50);
        //    AutoResizeColumnWith(dgvCheckedResult, defaultRecord, 2);  // Adjust width of columns

        //    DataGridViewCustom.AdjustColumnWidthsToFitContent(dgvCheckedResult);
        //    DataGridViewCustom.AdjustColumnWidthsToFitContent(dgvDatabase);


        //    // Update progress bar
        //    ProgressBarInitialize();
        //    ProgressBarCheckedUpdate();
        //    prBarCheckPassed.Invalidate();
        //    dgvCheckedResult.Invalidate();
        //    // Enable control after completed initialize data
        //    pnlMenu.Enabled = true;
        //    EnableUIComponentWhenLoadData(true);
        //    stw.Stop();
        //    Debug.WriteLine("Init completed, it took " + stw.ElapsedMilliseconds);
        //}

        private async Task<IList<string[]>> InitDatabaseAndPrintedStatusAsync(JobModel jobModel)
        {
            var pathDatabase = jobModel.DirectoryDatabase;
            var pathBackupPrintedResponse = CommVariables.PathPrintedResponse + jobModel.PrintedResponePath;

            // Initialize barcode data
            var (tmp, csvHasNsxHsd) = await Task.Run(() => InitDatabase(pathDatabase, jobModel.IsFirstRowHeader));

            // Append NSX/HSD to CSV if not already present
            if (!csvHasNsxHsd && tmp.Count > 1)
                await Task.Run(() => AppendNsxHsdToCsv(pathDatabase, tmp, jobModel.IsFirstRowHeader));

            // ── InitPrintedStatus trước — xác định dòng nào thực sự đã in ──
            var timestamps = new Dictionary<int, (string sendTime, string printTime)>();
            if (jobModel.PrintedResponePath != "" && File.Exists(jobModel.DirectoryDatabase) && tmp.Count() > 1)
                timestamps = await Task.Run(() => { return InitPrintedStatus(pathBackupPrintedResponse, tmp); });

            // ── RỒI mới update NSX/HSD — chỉ dòng còn Waiting mới được cập nhật ──
            // Mode 1/2: dùng CurrentBatchDate (ngày của QR hiện tại) thay vì DateTime.Now
            // để giữ NSX/HSD đồng bộ với QR khi mở lại job.
            DateTime nsxBase = (jobModel.CurrentBatchDate > DateTime.MinValue)
                ? jobModel.CurrentBatchDate
                : DateTime.Now;
            string todayNow = nsxBase.ToString("dd MM yy");
            string hsdNow = nsxBase.AddDays((_SelectedJob?.THJobExpiryMonths ?? 6) - 1).ToString("dd MM yy");
            await Task.Run(() =>
            {
                for (int i = 1; i < tmp.Count; i++)
                {
                    var row = tmp[i];
                    if (row.Length > 2 && row[1] == "Waiting")
                    {
                        row[row.Length - 2] = todayNow;
                        row[row.Length - 1] = hsdNow;
                    }
                }
            });

            // ── Mode 1/2: chuyển sang PrintedCodeVirtualList để tiết kiệm RAM ──
            bool isMode1Or2 = jobModel.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                           || jobModel.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                           || jobModel.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;
            if (isMode1Or2 && tmp.Count > 1)
            {
                string qrCode = tmp[tmp.Count - 1].Length > 2 ? tmp[tmp.Count - 1][2] : "";
                //Nhan.To_260920: Lay NSX/HSD tu dong cuoi CSV (dong moi nhat, chua in) thay vi dong dau (da in, NSX cu)
                //Nguyen nhan: Khi restart app, constructor lay NSX/HSD tu dong dau CSV -> gia tri cu -> safety net ghidep NSX/HSD len tat ca dong chua in
                //string nsx = tmp[1].Length > 3 ? tmp[1][tmp[1].Length - 2] : "";
                //string hsd = tmp[1].Length > 4 ? tmp[1][tmp[1].Length - 1] : "";
                string nsx = tmp[tmp.Count - 1].Length > 3 ? tmp[tmp.Count - 1][tmp[tmp.Count - 1].Length - 2] : "";
                string hsd = tmp[tmp.Count - 1].Length > 4 ? tmp[tmp.Count - 1][tmp[tmp.Count - 1].Length - 1] : "";
                int fieldCount = tmp[0].Length;  // số cột data (5)
                var virtualList = new PrintedCodeVirtualList(tmp.Count - 1, qrCode, nsx, hsd, fieldCount, 7);
                // copy status đã in từ InitPrintedStatus
                await Task.Run(() =>
                {
                    for (int i = 1; i < tmp.Count; i++)
                    {
                        if (tmp[i].Length > 1)
                        {
                            virtualList.SetStatus(i - 1, tmp[i][1]);
                            // copy QR per-row từ tmp vào VirtualList
                            if (tmp[i].Length > 2)
                                virtualList.SetQrCode(i - 1, tmp[i][2]);
                            // copy NSX/HSD per-row từ tmp vào VirtualList
                            if (tmp[i].Length > 3)
                            {
                                virtualList.SetNsx(i - 1, tmp[i][tmp[i].Length - 2]);
                                virtualList.SetHsd(i - 1, tmp[i][tmp[i].Length - 1]);
                            }
                        }
                    }
                });
                // Populate timestamps từ PrintedResponse
                foreach (var kvp in timestamps)
                {
                    if (!string.IsNullOrEmpty(kvp.Value.sendTime))
                        virtualList.SetSendTime(kvp.Key, kvp.Value.sendTime);
                    if (!string.IsNullOrEmpty(kvp.Value.printTime))
                        virtualList.SetPrintTime(kvp.Key, kvp.Value.printTime);
                }
                return virtualList;
            }
            return tmp;
        }

        private async Task<List<string[]>> InitCheckedResultDataAsync(JobModel jobModel)
        {
            // Loading checked result async implement
            var path = CommVariables.PathCheckedResult + jobModel.CheckedResultPath;
            if (jobModel.CheckedResultPath != "")
            {
                var tsk = Task.Run(() => { return InitCheckedResultData(path); });
                return await tsk;
            }
            return new List<string[]>();
        }

        private async Task InitCompareDataAsync(IList<string[]> datas, List<string[]> result)
        {
            // Initialize compare data async implement
            await Task.Run(() => { InitCompareData(datas, result); });
        }

        private (List<string[]> data, bool hasNsxHsd) InitDatabase(string path, bool isFirstRowHeader)
        {
            Stopwatch stw = Stopwatch.StartNew();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitDatabase work on thread " + Environment.CurrentManagedThreadId);
            List<string[]> result = new List<string[]>();
            bool csvHasNsxHsd = false;

            if (!File.Exists(path))
            {
                _InitDataErrorList.Add(InitDataError.DatabaseDoNotExist);
                DialogResult dialogResult = CustomMessageBox.Show("'" + path + "' " + Lang.CanNotFindDatabase, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (result, false);
            }

            try
            {
                using (var reader = new StreamReader(path, Encoding.UTF8, true))
                {
                    var rexCsvSplitter = path.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");
                    int lineCounter = -1;
                    int columnCount = 0;
                    bool hasTimestamps = false;
                    bool hasNsxHsd = false;

                    var isDate = new Func<string, bool>(v =>
                        !string.IsNullOrEmpty(v) &&
                        (DateTime.TryParseExact(v, "dd MM yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                         || DateTime.TryParseExact(v, "dd MM yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)));

                    while (!reader.EndOfStream)
                    {
                        string[] line = rexCsvSplitter.Split(reader.ReadLine()).Select(x => Csv.Unescape(x)).ToArray();
                        lineCounter++;
                        if (lineCounter == 0)
                        {
                            // Detect timestamps
                            bool isTimestampHeader = line.Length >= 2
                                && string.Equals(line[line.Length - 2], "Thời gian gửi", StringComparison.OrdinalIgnoreCase)
                                && string.Equals(line[line.Length - 1], "Thời gian in", StringComparison.OrdinalIgnoreCase);

                            bool isTimestampData = !isTimestampHeader && line.Length >= 4
                                && isDate(line[line.Length - 4])
                                && isDate(line[line.Length - 3])
                                && isDate(line[line.Length - 2])
                                && isDate(line[line.Length - 1]);

                            // Detect NSX/HSD
                            bool isNsxHsdHeader = line.Any(x => string.Equals(x, "NSX", StringComparison.OrdinalIgnoreCase))
                                && line.Any(x => string.Equals(x, "HSD", StringComparison.OrdinalIgnoreCase));

                            bool isNsxHsdData = !isTimestampHeader && !isTimestampData && line.Length >= 2
                                && isDate(line[line.Length - 2])
                                && isDate(line[line.Length - 1]);

                            hasTimestamps = isTimestampHeader || isTimestampData;
                            hasNsxHsd = isNsxHsdHeader || isNsxHsdData;
                            csvHasNsxHsd = hasNsxHsd;

                            // dataCols = số cột data từ file (không tính timestamp, nhưng tính NSX/HSD)
                            int dataCols = line.Length;
                            if (hasTimestamps) dataCols -= 2;
                            if (!hasNsxHsd) dataCols += 2; // thêm NSX/HSD mặc định nếu chưa có

                            columnCount = dataCols + 2; // +2 cho STT và Status

                            // Tạo header row
                            var tmp = new string[columnCount];
                            tmp[0] = "STT";
                            tmp[1] = "Trạng thái";
                            if (isFirstRowHeader)
                            {
                                int fileDataCols = line.Length;
                                if (hasTimestamps) fileDataCols -= 2;
                                for (int i = 2; i < 2 + fileDataCols && i < columnCount - 2; i++)
                                    tmp[i] = line[i - 2];
                                for (int i = 2 + fileDataCols; i < columnCount - 2; i++)
                                    tmp[i] = "QR";
                            }
                            else
                            {
                                for (int i = 2; i < columnCount - 2; i++)
                                    tmp[i] = "QR";
                            }
                            tmp[columnCount - 2] = "NSX";
                            tmp[columnCount - 1] = "HSD";
                            result.Add(tmp);
                        }

                        // Nếu không có header row, dòng đầu tiên cũng là data row
                        if (!isFirstRowHeader || lineCounter > 0)
                        {
                            // Data row
                            var tmp = new string[columnCount];
                            tmp[0] = isFirstRowHeader ? lineCounter.ToString() : (lineCounter + 1).ToString();
                            tmp[1] = "Waiting";

                            int fileDataCols = line.Length;
                            if (hasTimestamps) fileDataCols -= 2;

                            for (int i = 2; i < columnCount - 2; i++)
                            {
                                int fileIdx = i - 2;
                                if (fileIdx < fileDataCols && fileIdx < line.Length)
                                    tmp[i] = line[fileIdx];
                                else
                                    tmp[i] = "";
                            }

                            if (hasNsxHsd)
                            {
                                int nsxIdx = hasTimestamps ? line.Length - 4 : line.Length - 2;
                                int hsdIdx = hasTimestamps ? line.Length - 3 : line.Length - 1;
                                if (nsxIdx >= 0 && nsxIdx < line.Length)
                                    tmp[columnCount - 2] = line[nsxIdx];
                                if (hsdIdx >= 0 && hsdIdx < line.Length)
                                    tmp[columnCount - 1] = line[hsdIdx];
                            }
                            else
                            {
                                tmp[columnCount - 2] = DateTime.Now.ToString("dd MM yy");
                                tmp[columnCount - 1] = DateTime.Now.AddDays((_SelectedJob?.THJobExpiryMonths ?? 6) - 1).ToString("dd MM yy");
                            }
                            result.Add(tmp);
                        }
                    }
                }
            }
            catch (IOException)
            {
                _InitDataErrorList.Add(InitDataError.CannotAccessDatabase);
            }
            catch (Exception)
            {
                _InitDataErrorList.Add(InitDataError.DatabaseUnknownError);
            }

            stw.Stop();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitDatabase completed, it took " + stw.ElapsedMilliseconds);
            return (result, csvHasNsxHsd);
        }

        private void AppendNsxHsdToCsv(string path, List<string[]> result, bool isFirstRowHeader)
        {
            try
            {
                if (!File.Exists(path)) return;

                // ── DEBUG LOG ──
                string logDir2 = CommVariables.PathProgramDataApp + "Logs\\";
                if (!Directory.Exists(logDir2)) Directory.CreateDirectory(logDir2);
                File.AppendAllText(logDir2 + "SaveCsvDebug.log",
                    $"[{DateTime.Now:HH:mm:ss.fff}] [AppendNsxHsd] path={path} result.Count={result.Count} isFirstRowHeader={isFirstRowHeader}{Environment.NewLine}");
                // ── END DEBUG LOG ──

                // ── Guard: skip nếu CSV đã có data NSX/HSD ──
                var checkDelimiter = path.EndsWith(".csv") ? "," : "\t";
                var firstDataLine = File.ReadLines(path).Skip(isFirstRowHeader ? 1 : 0)
                    .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
                if (!string.IsNullOrEmpty(firstDataLine))
                {
                    var checkParts = firstDataLine.Split(checkDelimiter[0]);
                    if (checkParts.Length >= 3)
                    {
                        string lastCol = checkParts[checkParts.Length - 1].Trim().Trim('"');
                        string secondLast = checkParts[checkParts.Length - 2].Trim().Trim('"');
                        bool lastIsDate = DateTime.TryParseExact(lastCol, "dd MM yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                            || DateTime.TryParseExact(lastCol, "dd MM yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
                        bool secondLastIsDate = DateTime.TryParseExact(secondLast, "dd MM yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                            || DateTime.TryParseExact(secondLast, "dd MM yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

                        if (lastIsDate && secondLastIsDate)
                        {
                            Console.WriteLine($"[AppendNsxHsdToCsv] CSV đã có NSX/HSD → skip ghi đè");
                            return;
                        }
                    }
                }

                var lines = File.ReadAllLines(path, Encoding.UTF8).ToList();
                var delimiter = path.EndsWith(".csv") ? "," : "\t";

                for (int i = 0; i < lines.Count; i++)
                {
                    // resultIndex = i khi file có header (result[0] = header, result[1..] = data)
                    // resultIndex = i+1 khi file không có header (result[0] = synthetic header, result[1..] = data)
                    int resultIndex = isFirstRowHeader ? i : i + 1;

                    if (isFirstRowHeader && i == 0)
                    {
                        lines[i] = lines[i] + delimiter + Csv.Escape("NSX") + delimiter + Csv.Escape("HSD");
                    }
                    else if (resultIndex < result.Count)
                    {
                        int rowLen = result[resultIndex].Length;
                        string nsx = Csv.Escape(result[resultIndex][rowLen - 2]);
                        string hsd = Csv.Escape(result[resultIndex][rowLen - 1]);
                        lines[i] = lines[i] + delimiter + nsx + delimiter + hsd;
                    }
                    else
                    {
                        string nsx = Csv.Escape(DateTime.Now.ToString("dd MM yy"));
                        string hsd = Csv.Escape(DateTime.Now.AddDays((_SelectedJob?.THJobExpiryMonths ?? 6) - 1).ToString("dd MM yy"));
                        lines[i] = lines[i] + delimiter + nsx + delimiter + hsd;
                    }
                }

                File.WriteAllLines(path, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[AppendNsxHsdToCsv] " + ex.Message);
            }
        }

        private Dictionary<int, (string sendTime, string printTime)> InitPrintedStatus(string path, List<string[]> list)
        {
            var timestamps = new Dictionary<int, (string sendTime, string printTime)>();
            Stopwatch stw = Stopwatch.StartNew();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitPrintedStatus work on thread " + Environment.CurrentManagedThreadId);
            if (!File.Exists(path))
            {
                _InitDataErrorList.Add(InitDataError.CheckedResultDoNotExist);
                DialogResult dialogResult = CustomMessageBox.Show(Lang.CanNotFindPrintedResponse, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return timestamps;
            }

            try
            {
                using (StreamReader reader = new StreamReader(path, Encoding.UTF8, true))
                {
                    int i = -1;
                    int lineCount = 0;
                    var rexCsvSplitter = path.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");

                    while (!reader.EndOfStream)
                    {
                        i++;
                        if (i == 0)
                        {
                            reader.ReadLine();
                        }
                        else
                        {
                            // using only index value to update printed status

                            var dataLine = reader.ReadLine(); // Data: index,Printed,qr_code,...,timestamp
                            var fields = dataLine.Split(',');
                            string index = fields[0];
                            string status = fields[1];
                            if (int.TryParse(index, out int indexNumber) && indexNumber < list.Count)
                            {
                                list[indexNumber][1] = "Printed";
                                // Lưu timestamps để populate vào VirtualList sau
                                string sendTime = "";
                                string printTime = "";
                                if (fields.Length > 2)
                                    printTime = Csv.Unescape(fields[fields.Length - 1]);
                                if (fields.Length > 3)
                                    sendTime = Csv.Unescape(fields[fields.Length - 2]);
                                if (!string.IsNullOrEmpty(sendTime) || !string.IsNullOrEmpty(printTime))
                                    timestamps[indexNumber] = (sendTime, printTime);
                                lineCount++;
                            }

                            //var indexString = Csv.Unescape(rexCsvSplitter.Split(dataLine)[0]);
                            //  var lineStatus = Csv.Unescape(rexCsvSplitter.Split(reader.ReadLine())[1]);
                            //if(int.TryParse(indexString, out int indexNumber))
                            //    list[indexNumber][1] = "Printed";
                        }
                    }
                    ProjectLogger.WriteInfo($"[InitPrintedStatus] Đọc {lineCount} dòng đã in từ file {Path.GetFileName(path)} (tổng: {list.Count - 1} dòng database)");
                }
            }
            catch (IOException)
            {
                _InitDataErrorList.Add(InitDataError.CannotAccessPrintedResponse);
            }
            catch (Exception)
            {
                _InitDataErrorList.Add(InitDataError.PrintedStatusUnknownError);
            }

            stw.Stop();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitPrintedStaus complete, it took " + stw.ElapsedMilliseconds);
            return timestamps;
        }

        private List<string[]> InitCheckedResultData(string path)
        {
            Stopwatch stw = Stopwatch.StartNew();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitCheckedResult work on thread " + Environment.CurrentManagedThreadId);
            List<string[]> result = new List<string[]>();

            if (!File.Exists(path))
            {
                _InitDataErrorList.Add(InitDataError.CheckedResultDoNotExist);
                DialogResult dialogResult = CustomMessageBox.Show(Lang.CanNotFindCheckedResult, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return result;
            }

            try
            {
                var rexCsvSplitter = path.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");
                using (var reader = new StreamReader(path, Encoding.UTF8, true))
                {
                    bool isFirstline = false;
                    while (!reader.EndOfStream)
                    {
                        if (!isFirstline)
                        {
                            reader.ReadLine();
                            isFirstline = true;
                        }
                        else
                        {
                            // handle checked result row before adding
                            string[] line = rexCsvSplitter.Split(reader.ReadLine()).Select(x => Csv.Unescape(x)).ToArray();
                            // ignore empty line 16/11/2023 by Thong Thach
                            if (line.Length == 1 && line[0] == "") continue;
                            string[] checkedResult;
                            if (line.Length < _ColumnNames.Length)
                                checkedResult = GetTheRightString(line);
                            else
                                checkedResult = line;

                            // Normalize NSX và HSD về dd mm yy
                            if (checkedResult.Length > Index_NSX)
                                checkedResult[Index_NSX] = NormalizeDateToDdMmYy(checkedResult[Index_NSX]);
                            if (checkedResult.Length > Index_HSD)
                                checkedResult[Index_HSD] = NormalizeDateToDdMmYy(checkedResult[Index_HSD]);
                            if (checkedResult.Length > Index_BatchCol)
                                checkedResult[Index_BatchCol] = NormalizeDateToDdMmYy(checkedResult[Index_BatchCol]);

                            result.Add(checkedResult);
                        }
                    }
                }
            }
            catch (IOException)
            {
                _InitDataErrorList.Add(InitDataError.CannotAccessCheckedResult);
            }
            catch (Exception)
            {
                _InitDataErrorList.Add(InitDataError.CheckedResultUnknownError);
            }
            stw.Stop();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitCheckedResult completed, it took " + stw.ElapsedMilliseconds);
            return result;
        }
        private void InitCompareData(IList<string[]> datas, List<string[]> result)
        {
            Stopwatch stw = Stopwatch.StartNew();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitCompareData work on thread " + Environment.CurrentManagedThreadId);

            HashSet<string> _CheckedResultCodeSet = new HashSet<string>();
            var validCond = ComparisonResult.Valid.ToString();
            var columnCount = _ColumnNames.Count();

            // Mode 1 (BatchOneQrCode): 1 QR lặp lại N lần
            // Mode 2 (AutoRefreshByTime): nhiều QR, mỗi QR lặp lại N lần
            // → Cả hai đều KHÔNG được đánh dấu Duplicate cho các dòng lặp
            bool isRepeatedQrMode = _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                       || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                       || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;

            try
            {
                // ── Mode 1/2: populate _CodeListPODFormat để DatabaseCompare dùng dict ──
                if (isRepeatedQrMode)
                {
                    _CheckedResultCodeSet.Clear();
                    _CodeListPODFormat.Clear();
                    // Thêm tất cả QR duy nhất từ VirtualList vào dict
                    if (datas is PrintedCodeVirtualList virtualList)
                    {
                        var uniqueQrs = new HashSet<string>();
                        for (int i = 0; i < virtualList.Count; i++)
                        {
                            string qr = virtualList.GetQrCode(i);
                            if (!string.IsNullOrEmpty(qr) && uniqueQrs.Add(qr))
                                _CodeListPODFormat.TryAdd(qr, new CompareStatus(i, false));
                        }
                    }
                    stw.Stop();
                    Debug.WriteLine(_BigSTW.ElapsedMilliseconds + $" InitCompareData (Mode 1/2 skipped) completed, dict has {_CodeListPODFormat.Count} QRs, it took " + stw.ElapsedMilliseconds);
                    return;
                }

                foreach (var array in result)
                {
                    if (columnCount == array.Length && array[2] == validCond)
                    {
                        _CheckedResultCodeSet.Add(array[1]);
                    }
                }

                if (datas.Count > 0)
                {
                    int codeLenght = datas[0].Count() - 1;
                    for (int index = 0; index < datas.Count; index++)
                    {
                        var source = datas[index];
                        string data;
                        if (_SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.ProductOneQrCode)
                        {
                            data = source[2]; // QR code directly (skip id[0], status[1])
                        }
                        else
                        {
                            var compareSb = new StringBuilder();
                            foreach (var item in _SelectedJob.PODFormat)
                            {
                                if (item.Type == PODModel.TypePOD.FIELD)
                                {
                                    int origIdx = item.Index >= 1 ? item.Index + 1 : item.Index;
                                    compareSb.Append(source[origIdx]);
                                }
                                else if (item.Type == PODModel.TypePOD.TEXT)
                                {
                                    compareSb.Append(item.Value);
                                }
                            }
                            data = compareSb.ToString();
                        }

                        bool alreadyChecked = _CheckedResultCodeSet.Contains(data);

                        if (alreadyChecked)
                        {
                            bool tryAdd = _CodeListPODFormat.TryAdd(data, new CompareStatus(index, true));
                            if (!tryAdd && !Shared.Settings.DuplicatedDBEnable)
                            {
                                _PrintedCodeObtainFromFile[index][1] = "Duplicate";
                                _NumberOfDuplicate++;
                            }
                        }
                        else
                        {
                            bool tryAdd = _CodeListPODFormat.TryAdd(data, new CompareStatus(index, false));
                            if (!tryAdd && !Shared.Settings.DuplicatedDBEnable)
                            {
                                _PrintedCodeObtainFromFile[index][1] = "Duplicate";
                                _NumberOfDuplicate++;
                            }

                            if (_IsVerifyAndPrintMode)
                            {
                                string tmp = "";
                                for (int i = 1; i <= source.Length - 2; i++)
                                {
                                    var tmpPOD = Shared.Settings.PrintFieldForVerifyAndPrint.Find(x => x.Index == i);
                                    if (tmpPOD != null)
                                    {
                                        tmp += source[tmpPOD.Index + 1];
                                    }
                                }
                                _Emergency.TryAdd(tmp, index);
                            }
                        }
                    }
                }

                _CheckedResultCodeSet.Clear();
            }
            catch
            {
                _InitDataErrorList.Add(InitDataError.Unknown);
            }

            stw.Stop();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitCompareData completed, it took " + stw.ElapsedMilliseconds);
        }
        //private void InitCompareData(List<string[]> datas, List<string[]> result)
        //{
        //    Stopwatch stw = Stopwatch.StartNew();
        //    Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitCompareData work on thread " + Environment.CurrentManagedThreadId);

        //    // Use a HashSet instead of a List
        //    HashSet<string> _CheckedResultCodeSet = new HashSet<string>();

        //    // Populate the HashSet with the second element of each array
        //    var validCond = ComparisonResult.Valid.ToString();
        //    var columnCount = _ColumnNames.Count();// Thinh Note: Increase/Decrease the column count when change the _ColumnNames 
        //    try
        //    {
        //        foreach (var array in result)
        //        {
        //            if (columnCount == array.Length && array[2] == validCond)
        //            {
        //                _CheckedResultCodeSet.Add(array[1]);
        //            }
        //        }
        //        if (datas.Count > 0)
        //        {
        //            int codeLenght = datas[0].Count() - 1;
        //            for (int index = 0; index < datas.Count; index++)
        //            {

        //                string[] row = datas[index].Where((item, idx) => idx != 1).ToArray();
        //                string data = GetCompareDataByPODFormat(row, _SelectedJob.PODFormat);

        //                if (_CheckedResultCodeSet.Contains(data))
        //                {
        //                    bool tryAdd = _CodeListPODFormat.TryAdd(data, new CompareStatus(index, true));
        //                    if (!tryAdd && !Shared.Settings.DuplicatedDBEnable)
        //                    {
        //                        _PrintedCodeObtainFromFile[index][1] = "Duplicate";
        //                        _NumberOfDuplicate++;
        //                    }
        //                }
        //                else
        //                {
        //                    bool tryAdd = _CodeListPODFormat.TryAdd(data, new CompareStatus(index, false));
        //                    if (!tryAdd && !Shared.Settings.DuplicatedDBEnable)
        //                    {
        //                        _PrintedCodeObtainFromFile[index][1] = "Duplicate";
        //                        _NumberOfDuplicate++;
        //                    }

        //                    // Data use to update printed status for Verify and print - Compare mode
        //                    if (_IsVerifyAndPrintMode)
        //                    {
        //                        string tmp = "";
        //                        row = row.Skip(1).ToArray();
        //                        for (int i = 1; i <= row.Length; i++)
        //                        {
        //                            var tmpPOD = Shared.Settings.PrintFieldForVerifyAndPrint.Find(x => x.Index == i);
        //                            if (tmpPOD != null)
        //                            {
        //                                tmp += row[tmpPOD.Index - 1];
        //                            }
        //                        }
        //                        var tryAdd2 = _Emergency.TryAdd(tmp, index);
        //                    }
        //                }
        //            }
        //        }

        //        _CheckedResultCodeSet.Clear();
        //    }
        //    catch
        //    {
        //        _InitDataErrorList.Add(InitDataError.Unknown);
        //    }
        //    stw.Stop();
        //    Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitCompareData completed, it took " + stw.ElapsedMilliseconds);
        //}

        private string[] GetTheRightString(string[] line)
        {
            var code = new string[_ColumnNames.Length];
            for (int i = 0; i < code.Length; i++)
            {
                if (i < line.Length)
                    code[i] = line[i];
                else
                    code[i] = "";
            }
            return code;
        }

        public string GetCompareDataByPODFormat(string[] values, List<PODModel> podFormat, int addingIndex = 0)
        {
            if (values.Length == 0) return "";
            var compareString = "";
            foreach (var item in podFormat)
            {
                if (item.Type == PODModel.TypePOD.FIELD)
                {
                    compareString += values[item.Index + addingIndex];
                }
                else if (item.Type == PODModel.TypePOD.TEXT)
                {
                    compareString += item.Value;
                }
            }
            return compareString;
        }

        /// <summary>Chuẩn hóa date string về định dạng dd mm yy.</summary>
        private static string NormalizeDateToDdMmYy(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            string[] formats = {
                "dd/MM/yyyy", "dd/MM/yy",
                "yyyy-MM-dd", "dd-MM-yyyy", "dd-MM-yy",
                "dd MM yy", "dd MM yyyy",
                "dd.MM.yyyy", "dd.MM.yy",
                "yyyy/MM/dd",
                "ddMMyy", "ddMMyyyy",
                "dd/MM/yyyy HH:mm:ss", "dd/MM/yy HH:mm:ss",
                "dd MM yy HH:mm:ss", "dd MM yyyy HH:mm:ss",
                "dd-MM-yyyy HH:mm:ss", "dd-MM-yy HH:mm:ss"
            };
            foreach (var fmt in formats)
            {
                if (DateTime.TryParseExact(raw, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                    return dt.ToString("dd MM yy");
            }
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt2))
                return dt2.ToString("dd MM yy");
            // Nếu không parse được, log và trả về raw
            Console.WriteLine($"[NormalizeDate] Không parse được: '{raw}'");
            return raw;
        }

        #endregion Jobs

        #region Events Called
        private bool _isRlinkStop;
        private void Shared_OnLogError(object sender, EventArgs e)
        {
            try
            {
                Exception ex = default(Exception);
                ex = (Exception)sender;
                var result = "";
                if (ex.InnerException != null)
                {
                    string innerExection = "InnerException: " + ex.InnerException.InnerException + " && ";
                    result += innerExection;
                }
                if (ex.Message != null)
                {
                    string errorMessage = ex.Message.Replace("\r", "").Replace("\n", "").Replace(',', '&');
                    result += "Message: " + errorMessage;
                }
                if (ex.Source != null)
                {
                    string errorSource = ex.Source;
                    result += " && Source: " + errorSource;
                }
                if (ex.StackTrace != null)
                {
                    StackTrace stackTrace = new StackTrace(ex, true);

                    foreach (StackFrame stackFrame in stackTrace.GetFrames())
                    {
                        string methodName = stackFrame.GetMethod().Name;
                        int lineNumber = stackFrame.GetFileLineNumber();
                        if (methodName != "" && lineNumber != 0)
                        {
                            result += " && Method: " + methodName + " line " + lineNumber;
                        }
                    }
                }
                if (ex.TargetSite != null)
                {
                    string targetSite = " && TargetSite: " + ex.TargetSite.ToString() + " - " + ex.TargetSite.DeclaringType.ToString();
                    result += targetSite;
                }
                result = result.Replace("'", "");
                LoggingController.SaveHistory(
                    String.Format("Error catch"),
                    Lang.Error,
                    String.Format(result),
                    SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                    LoggingType.Error);
            }
            catch
            {

            }
        }
        //private void ShowSentDataInfo()
        //{
        //    long totalSent = _SelectedJob?.TotalSent ?? 0;
        //    long totalReceived = _SelectedJob?.TotalReceived ?? 0;

        //    string msg =
        //        "──────── SENT ────────\n" +
        //          "\n" +
        //        $"  {"Đã gửi",-5}:     {_NumberOfSentPrinter:N0}\n" +
        //        $"  {"Tổng gửi",-5}:   {totalSent:N0}\n" +
        //        "\n" +
        //        "──────── RECEIVE ────────\n" +
        //          "\n" +
        //        $"  {"Đã nhận",-5}:    {_ReceivedCode:N0}\n" +
        //        $"  {"Tổng nhận",-5}:  {totalReceived:N0}\n" +
        //        "\n" +
        //        "──────── PRINT ────────\n" +
        //          "\n" +
        //        $"  {"Đã in",-5}:     {NumberPrinted:N0}\n" +
        //        $"  {"RSFP",-5}:      {(_SelectedJob?.TotalRsfpReceived ?? 0):N0}\n" +
        //        "\n" +
        //        $"  {"Last printed page",-5} {_lastPrintedPageFormPrinter:N0}";

        //    CuzMessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //}

        private void ShowSentDataInfo()
        {
            using (SentDataInfoForm form = new SentDataInfoForm(
                numberSent: _NumberOfSentPrinter,
                totalSent: _SelectedJob?.TotalSent ?? 0,
                numberReceived: _ReceivedCode,
                totalReceived: _SelectedJob?.TotalReceived ?? 0,
                numberPrinted: NumberPrinted,
                totalRsfp: _SelectedJob?.TotalRsfpReceived ?? 0,
                lastPrintedPage: _lastPrintedPageFormPrinter,
                numberA: ErrorCountA,
                numberB: ErrorCountB,
                numberF: ErrorCountF,
                totalChecked: TotalChecked,
                cameraRejected: Math.Max(0, ErrorCountF - PhanLoaiCount),
                rlinkRejected: PhanLoaiCount))
            {
                form.ShowDialog(this);
            }
        }



        private async void ActionChanged(object sender, EventArgs e)
        {
            if (sender == btnJob)
            {
                IsCloseButtonAction = false;
                Close();
            }
            else if (sender == btnStart)
            {
                if (IsDefaultSupportAccount())
                {
                    CustomMessageBox.Show(
                        "Tài khoản Support không được phép start job.",
                        "Không được phép",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                // ── Kiểm tra job đã in hết mã chưa ──
                bool allPrinted = _TotalCode > 0 && NumberPrinted >= _TotalCode;

                int waitingCount = 0;
                var vlCheck = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                if (vlCheck != null)
                    waitingCount = vlCheck.GetWaitingCount();
                else
                    waitingCount = _PrintedCodeObtainFromFile?
                        .Count(x => x.Length > 1 && x[1] == "Waiting") ?? 0;
                bool noWaiting = _TotalCode > 0 && waitingCount == 0 && !_IsReCheck;

                if (allPrinted || noWaiting)
                {
                    CustomMessageBox.Show(
                        $"Job này đã in đủ {_TotalCode:N0} mã.\nKhông thể start lại.",
                        "Job đã hoàn thành", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _isRlinkStop = false;
                _ParentForm.Invoke_AutoAddSufixEvent();
                Thread.Sleep(300);
                btnCompleteJob.Enabled = false;

                // Disable tất cả controls phía sau
                foreach (Control c in Controls)
                {
                    if (c is Form) continue;
                    c.Enabled = false;
                }

                // ── Hiển thị loading ──
                var loading = frmLoadingUi.ShowLoading(this,
                    "CHUẨN BỊ",
                    "Vui lòng đợi chuẩn bị chương trình camera...");

                // ── Chuyển program camera ──
                var swCam = System.Diagnostics.Stopwatch.StartNew();
                var (camOk, camMessage) = await ChangeCameraProgramFromJobAsync(_SelectedJob);
                swCam.Stop();

                // Đảm bảo loading hiển thị tối thiểu 1.5s rồi ẩn
                int elapsed = (int)swCam.ElapsedMilliseconds;
                if (elapsed < 1500)
                    await Task.Delay(1500 - elapsed);
                frmLoadingUi.CloseLoading(ref loading);

                if (!camOk && Shared.Settings.CheckAllWhenStart)
                {
                    foreach (Control c in Controls) c.Enabled = true;
                    CustomMessageBox.Show(camMessage, "Cảnh báo camera",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnCompleteJob.Enabled = true;
                    return;
                }

                // ── Validate GTIN + QR trước khi start ──
                var loadingQr = frmLoadingUi.ShowLoading(this,
                    "KIỂM TRA QR",
                    "Đang kiểm tra mã QR hợp lệ...");

                var swQr = System.Diagnostics.Stopwatch.StartNew();
                var (qrOk, qrError) = await ValidateQrBeforeStartAsync();
                swQr.Stop();

                // Đảm bảo loading hiển thị tối thiểu 2s
                if (swQr.ElapsedMilliseconds < 2000)
                    await Task.Delay(2000 - (int)swQr.ElapsedMilliseconds);

                frmLoadingUi.CloseLoading(ref loadingQr);

                // Enable lại tất cả controls
                foreach (Control c in Controls) c.Enabled = true;

                if (!qrOk)
                {
                    CustomMessageBox.Show(qrError, "Kiểm tra QR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnCompleteJob.Enabled = true;
                    return;
                }

                // ── Kiểm tra job cần đổi QR trước khi start ──
                _isStartRebuild = true;
                _loadingRebuild = frmLoadingUi.ShowLoading(this, "Rebuild", "Đang kiểm tra QR...");
                var swRebuild = System.Diagnostics.Stopwatch.StartNew();
                var qrChangeOk = await _ParentForm.CheckQrChangeOnStartAsync();
                swRebuild.Stop();

                // ── Safety net: set trực tiếp QR/NSX/HSD cho TẤT CẢ dòng chưa in ──
                // Dùng SetQrCode/SetNsx/SetHsd trực tiếp (bỏ qua status check)
                {
                    var vl = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                    bool isVL = vl != null;
                    string globalQr = vl?.QrCode ?? "";
                    string globalNsx = vl?.Nsx ?? "";
                    string globalHsd = vl?.Hsd ?? "";
                    int totalRows = _PrintedCodeObtainFromFile?.Count ?? 0;
                    int updatedCount = 0;
                    int skippedPrinted = 0;
                    int skippedNull = 0;

                    ProjectLogger.WriteInfo($"[START_SAFETY] isVL={isVL} globalQR='{globalQr}' globalNsx='{globalNsx}' globalHsd='{globalHsd}' totalRows={totalRows}");

                    if (isVL && !string.IsNullOrEmpty(globalQr))
                    {
                        for (int i = 0; i < totalRows; i++)
                        {
                            var row = _PrintedCodeObtainFromFile[i];
                            if (row == null || row.Length <= 1) continue;
                            string status = row[1];
                            if (status == "Printed") { skippedPrinted++; continue; }

                            string oldQr = row.Length > 2 ? row[2] : "";
                            string oldNsx = row.Length > 3 ? row[row.Length - 2] : "";
                            string oldHsd = row.Length > 4 ? row[row.Length - 1] : "";

                            vl.SetQrCode(i, globalQr);
                            vl.SetNsx(i, globalNsx);
                            vl.SetHsd(i, globalHsd);
                            updatedCount++;

                            if (i < 3)
                            {
                                ProjectLogger.WriteInfo($"[START_SAFETY] row[{i}] status='{status}' oldQR='{oldQr}' → newQR='{globalQr}' oldNsx='{oldNsx}' → newNsx='{globalNsx}' oldHsd='{oldHsd}' → newHsd='{globalHsd}'");
                            }
                        }
                    }
                    else
                    {
                        skippedNull++;
                    }

                    ProjectLogger.WriteInfo($"[START_SAFETY] DONE updated={updatedCount} skippedPrinted={skippedPrinted} skippedNull={skippedNull}");
                }

                if (swRebuild.ElapsedMilliseconds < 1500)
                    await Task.Delay(1500 - (int)swRebuild.ElapsedMilliseconds);
                _isStartRebuild = false;
                frmLoadingUi.CloseLoading(ref _loadingRebuild);
                if (!qrChangeOk)
                {
                    foreach (Control c in Controls) c.Enabled = true;
                    btnCompleteJob.Enabled = true;
                    return;
                }

                // ── [MỚI] Hiển thị frmValidateQrCode ──
                System.Drawing.Image productImage = null;
                string productLocalPath = null;
                try
                {
                    var product = Shared.Settings.THProductList?.FirstOrDefault(
                        p => p.ProductId == _SelectedJob?.THJobProductId);
                    if (product != null && !string.IsNullOrWhiteSpace(product.Image))
                    {
                        productLocalPath = product.Image;
                        // Nếu chưa có file local → tải từ API
                        if (!System.IO.File.Exists(productLocalPath))
                        {
                            var loadingImg = frmLoadingUi.ShowLoading(this, "TẢI ẢNH", "Đang tải ảnh sản phẩm...");
                            try
                            {
                                productImage = ProductImageHelper.GetProductImage(product.Image, product.ProductId);
                            }
                            finally
                            {
                                frmLoadingUi.CloseLoading(ref loadingImg);
                            }
                        }
                    }
                }
                catch { }
                string sampleQr = "";
                if (_PrintedCodeObtainFromFile != null && _PrintedCodeObtainFromFile.Count > 0)
                {
                    var firstWaiting = _PrintedCodeObtainFromFile.FirstOrDefault(
                        x => x.Length > 2 && x[1] == "Waiting");
                    if (firstWaiting != null)
                        sampleQr = firstWaiting[2] ?? "";
                }

                System.Drawing.Image qrImage = null;
                if (!string.IsNullOrEmpty(sampleQr))
                    qrImage = GenerateQrImageUsingZXing(sampleQr);

                // Tính NSX/HSD — chỉ hiển thị với Mode 4
                //string nsxHsd = "";
                //if (_SelectedJob?.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
                //{
                //    try
                //    {
                //        // NSX = ngày tạo job (FirstRunTime hoặc ngày hiện tại)
                //        DateTime nsxDate = _SelectedJob.FirstRunTime != DateTime.MinValue
                //            ? _SelectedJob.FirstRunTime
                //            : DateTime.Now;
                //        string nsx = nsxDate.ToString("dd MM yy");
                //        // HSD = NSX + (tháng hết hạn - 1)
                //        int expiryMonths = _SelectedJob?.THJobExpiryMonths ?? 6;
                //        string hsd = nsxDate.AddMonths(expiryMonths - 1).ToString("dd MM yy");
                //        nsxHsd = $"NSX: {nsx}\nHSD: {hsd}";
                //    }
                //    catch { }
                //}
                // Lấy NSX/HSD từ file CSV database (VirtualList)
                string nsxHsd = "";
                if (_SelectedJob?.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange)
                {
                    try
                    {
                        var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                        if (virtualList != null)
                        {
                            string nsx = virtualList.Nsx ?? "";
                            string hsd = virtualList.Hsd ?? "";
                            if (!string.IsNullOrEmpty(nsx) && !string.IsNullOrEmpty(hsd))
                            {
                                nsxHsd = $"NSX: {nsx}\nHSD: {hsd}";
                            }
                        }
                    }
                    catch { }
                }
                using (var frmValidate = new frmValidateQrCode())
                {
                    frmValidate.SetStartMode();
                    // Ưu tiên load từ file local (nhanh hơn)
                    if (!string.IsNullOrEmpty(productLocalPath) && System.IO.File.Exists(productLocalPath))
                    {
                        frmValidate.SetValidateDataFromLocal(
                            productLocalPath,
                            qrImage,
                            sampleQr,
                            _SelectedJob?.THJobProductName ?? "",
                            _SelectedJob?.THJobProductId ?? "",
                            nsxHsd,
                            _SelectedJob?.THJobProductGtin ?? "");
                    }
                    else
                    {
                        frmValidate.SetValidateData(
                            productImage,
                            qrImage,
                            sampleQr,
                            _SelectedJob?.THJobProductName ?? "",
                            _SelectedJob?.THJobProductId ?? "",
                            nsxHsd,
                            _SelectedJob?.THJobProductGtin ?? "");
                    }

                    if (frmValidate.ShowDialog() != DialogResult.OK)
                    {
                      btnCompleteJob.Enabled = true;
                    return;
                    }
                       
                }

                if (_deferredAlertMessage != null)
                {
                    string msg = _deferredAlertMessage;
                    _deferredAlertMessage = null;
                    CuzAlert.Show(msg, Alert.enmType.Warning,
                        new Size(500, 120),
                        new Point(Location.X, Location.Y),
                        Size, false);
                }

                if (_deferredQrLowAlert != null)
                {
                    string msg = _deferredQrLowAlert;
                    _deferredQrLowAlert = null;
                    CuzAlert.Show(msg, Alert.enmType.Warning,
                        new Size(480, 100),
                        Location, Size, true);
                }

                if (_SelectedJob.FirstRunTime == DateTime.MinValue)
                {
                    _SelectedJob.FirstRunTime = DateTime.Now;
                    lblTimeProcessStart.Text = $"Bắt đầu: " + _SelectedJob.FirstRunTime.ToString("dd/MM/yyyy HH:mm");
                    lblTimeProcessStart.Visible = true;
                }
                RunStopwatch.Start();
                _runTimeTimer.Start();
                StartProcess();
            }
            else if (sender == btnStop)
            {
                _isRlinkStop = true;
                bool stopped = await StopProcessAsync(true, "", false, true);
                if (stopped)
                {
                    if (RunStopwatch.IsRunning)
                    {
                        AccumulatedRunTime += RunStopwatch.Elapsed;
                        RunStopwatch.Reset();
                    }
                    _SelectedJob.LastRunTime = DateTime.Now;
                    lblTimeProcessStop.Text = $"Kết thúc: " + _SelectedJob.LastRunTime.ToString("dd/MM/yyyy HH:mm");
                    lblTimeProcessStop.Visible = true;
                    _runTimeTimer.Stop();
                    _SelectedJob.TotalRunTimeTicks = AccumulatedRunTime.Ticks;
                    try { _SelectedJob.SaveFile(); } catch { }
                    btnCompleteJob.Enabled = true;
                }
            }
            else if (sender == btnAddBarcode)
            {
                try
                {
                    int NumberOfNewCode = (int)numPrinterPort.Value;
                    if (NumberOfNewCode <= 0) return;
                    var result = CustomMessageBox.Show(
                        $"Bạn có chắc chăn muốn tạo thêm {NumberOfNewCode} mã mới không ?",
                        Lang.Confirm,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;
                    List<string> list = Base30AutoCodeGenerator.GenerateLineCodesForLoyalty(quantity: NumberOfNewCode);

                    int counts = File.ReadAllLines(_SelectedJob.DirectoryDatabase).ToList().Count + 1;
                    File.AppendAllLines(_SelectedJob.DirectoryDatabase, list);
                    string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;



                    //var firstColumnList = FileFuncs.GetFirstColumn(list);

                    if (Shared.UserPermission.isOnline)
                    {
                        _SelectedJob.isPushedDatabase = false;

                        bool isSent = await _ParentForm.SendGeneratedCodes(list, _SelectedJob, counts);
                        if (!isSent)
                        {
                            CustomMessageBox.Show("Không thể gửi dữ liệu tạo!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    //if (Shared.CaoSuAllValueProcess == null)
                    //    Shared.CaoSuAllValueProcess = new CaoSuAllValueProcess(_SelectedJob);
                    //Shared.CaoSuAllValueProcess.AddNewQrCode(firstColumnList);

          
                    if (File.Exists(sentDataPath))
                    {
                        var newLines = list.Select((line, i) =>
                        {
                            var parts = line.Split(',');
                            return $"{counts + i},{parts[0]},{parts[1]},,,,,,";
                        }).ToList();
                        File.AppendAllLines(sentDataPath, newLines);
                    }
                    numPrinterPort.Value = 0;
                    InitControls();

                }
                catch (Exception)
                {
                }


            }
            else if (sender == btnAddTons)
            {
                await HandleAddTonsAsync();
            }
            else if (sender == btnDatabase || sender == pnlPrintedCode || sender == btnViewDatabase)
            {
                var isDatabaseDeny = _SelectedJob.CompareType == CompareType.Database && _TotalCode == 0;
                if (isDatabaseDeny)
                {
                    CustomMessageBox.Show(Lang.DatabaseDoesNotExist, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (_FormPreviewDatabase == null || _FormPreviewDatabase.IsDisposed)
                {
                    try
                    {
                        var updatedColumns = _DatabaseColunms
                             .Select(c => c == "Index" ? "STT" : c == "Status" ? "Trạng thái" : c.Replace(" - Field", " - Cột"))
                            .ToArray();

                        _FormPreviewDatabase = new FrmPreviewDatabaseTHTrueMilk
                        {
                            _DatabaseColunms = new List<string>(updatedColumns),
                            _ObtainCodeList = _PrintedCodeObtainFromFile,
                            _TotalColumns = _TotalColumns,
                            _Totals = _TotalCode,
                            _NumberPrinted = NumberPrinted,
                            _UseImageColumns = false
                        };
                        _FormPreviewDatabase.Show();
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("Lỗi mở Database: " + ex.Message, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (_FormPreviewDatabase.WindowState == FormWindowState.Minimized)
                        _FormPreviewDatabase.WindowState = FormWindowState.Normal;
                    _FormPreviewDatabase.Focus();
                    _FormPreviewDatabase.BringToFront();
                }
            }
           // else if ( sender == btnViewResult)
              else if (sender == pnlCheckFailed || sender == pnlCheckPassed || sender == pnlTotalChecked || sender == btnViewResult || sender == lblCheckedResult || sender == _summaryTotalLabel || sender == prBarCheckPassed)
                    {
                if (_FormCheckedResult == null || _FormCheckedResult.IsDisposed)
                {
                    _FormCheckedResult = new FrmCheckedResultTHTrueMilk
                    {
                        _frmParent = this,
                        _IsAfterProduction = _IsAfterProductionMode,
                        _IsRSeries = _SelectedJob.PrinterSeries,
                        _ColumnNames = _ColumnNames,
                        _CheckedResult = _CheckedResultCodeList,
                        _CheckedData = _CodeListPODFormat,
                        _CodeData = _PrintedCodeObtainFromFile,
                        _TotalColumns = _ColumnNames.Count(),
                        _TotalCode = _TotalCode,
                        _JobName = _SelectedJob.FileName,
                        _PODFormat = _SelectedJob.PODFormat,
                    };
                    if (sender == pnlCheckFailed)
                        _FormCheckedResult._FillValue = "Sai";
                    else if (sender == pnlCheckPassed)
                        _FormCheckedResult._FillValue = "Đúng";
                    //else if (sender == pnlTotalChecked)
                    else if (sender == _summaryTotalLabel || sender == prBarCheckPassed)
                        _FormCheckedResult._FillValue = "Tất cả";
                  
                        _FormCheckedResult.Show();
                }
                else
                {
                    // Re-assign references in case parent loaded new job data
                    _FormCheckedResult._frmParent = this;
                    _FormCheckedResult._ColumnNames = _ColumnNames;
                    _FormCheckedResult._CheckedResult = _CheckedResultCodeList;
                    _FormCheckedResult._CheckedData = _CodeListPODFormat;
                    _FormCheckedResult._CodeData = _PrintedCodeObtainFromFile;
                    _FormCheckedResult._TotalCode = _TotalCode;
                    _FormCheckedResult._JobName = _SelectedJob.FileName;
                    _FormCheckedResult._PODFormat = _SelectedJob.PODFormat;

                    if (sender == pnlCheckFailed)
                        _FormCheckedResult._FillValue = "Sai";
                    else if (sender == pnlCheckPassed)
                        _FormCheckedResult._FillValue = "Đúng";
                    else if (sender == pnlTotalChecked)
                        _FormCheckedResult._FillValue = "Tất cả";
                    _FormCheckedResult.Reload();
                    _FormCheckedResult.Show();
                    _FormCheckedResult.BringToFront();
                    _FormCheckedResult.Focus();
                    _FormCheckedResult.TopMost = true;
                }
            }
            else if (sender == btnHistory)
            {
                if (_FormViewHistoryProgram == null || _FormViewHistoryProgram.IsDisposed)
                {
                    _FormViewHistoryProgram = new FrmViewHistoryProgram("_rynan_loggin_access_control_management_");
                    _FormViewHistoryProgram.Show();
                }
                else
                {
                    if (_FormViewHistoryProgram.WindowState == FormWindowState.Minimized)
                    {
                        _FormViewHistoryProgram.WindowState = FormWindowState.Normal;
                    }
                    _FormViewHistoryProgram.Focus();
                    _FormViewHistoryProgram.BringToFront();
                }
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
            else if (sender == btnAccount)
            {
                cuzDropdownManageAccount.PrimaryColor = Color.FromArgb(0, 171, 230);
                cuzDropdownManageAccount.MenuItemHeight = 40;
                cuzDropdownManageAccount.Font = new Font("Microsoft Sans Serif", 12);
                cuzDropdownManageAccount.ForeColor = Color.Black;
                cuzDropdownManageAccount.Show(btnAccount, btnAccount.Width, 0);
            }
            else if (sender == mnManage)
            {
                FrmManageAccount form = new FrmManageAccount();
                _ = form.ShowDialog();
            }
            else if (sender == mnChangePassword)
            {
                FrmChangePassword frmChangePassword = new FrmChangePassword();
                frmChangePassword.ShowDialog();
            }
            else if (sender == mnLogOut)
            {
                _ParentForm.Exit();
            }
            else if (sender == btnExit)
            {
                _ParentForm.Exit();
            }
            else if (sender == btnExportData)
            {
                ExportDataAsync();
            }
            else if (sender == btnExportAll)
            {
                ExportAllDataAsync();
            }
            else if (sender == btnExportResult)
            {
                string filePathCheckResult = CommVariables.PathCheckedResult + _SelectedJob.CheckedResultPath;
                Shared.ExportCheckedResult(filePathCheckResult);
            }
            else if (sender == syncDataManual)
            {
                Form confirmCompletion = new frmReCheck();
                confirmCompletion.ShowDialog();
                //tableLayoutControl.Enabled = false;
            }
            else if (sender == syncDataBtn)
            {
                if (Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)
                {
                    // TODO: Lấy QR được chọn — hiện tại dùng QR từ dòng đang select trên dgvDatabase
                    string selectedQr = null;
                    if (dgvDatabase.SelectedCells.Count > 0)
                    {
                        int rowIdx = dgvDatabase.SelectedCells[0].RowIndex;
                        if (rowIdx >= 0 && dgvDatabase.Rows[rowIdx].Cells.Count > 2)
                            selectedQr = dgvDatabase.Rows[rowIdx].Cells[2].Value?.ToString();
                    }
                    if (string.IsNullOrWhiteSpace(selectedQr))
                    {
                        CustomMessageBox.Show("Vui lòng chọn 1 QR trong bảng dữ liệu.", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Đọc batch, product_id, prod, exp từ bảng Code
                    if (RLinkLogService.IsQrSentToMaster(selectedQr))
                    {
                        CustomMessageBox.Show("QR này đã được gửi rồi.", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    var (batch, pid, prod, exp, jobName, printedAt) = RLinkLogService.GetQrCodeData(selectedQr);

                    // Gọi API mark-used
                    var svc = RLinkMasterServiceFactory.Instance;
                    ProjectLogger.WriteInfo($"[QR-MARK] MANUAL gửi: {selectedQr} job={jobName} batch={batch}");
                    bool ok = await Task.Run(() => svc.MarkQrUsedAsync(
                        new System.Collections.Generic.List<string> { selectedQr },
                        jobName,
                        Shared.Settings?.LineId ?? "",
                        Shared.Settings?.LineName ?? "",
                        Shared.Settings?.FactoryCode ?? "",
                        batch, pid, prod, exp, printedAt));

                    if (ok)
                    {
                        RLinkLogService.MarkQrAsSentToMaster(selectedQr);
                        CustomMessageBox.Show("Đồng bộ QR thành công.", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        CustomMessageBox.Show("Đồng bộ QR thất bại - vui lòng thử lại sau.", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Hệ thống đang vận hành, vui lòng dừng trước khi đồng bộ.", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            else if (sender == disposeBtn)
            {
                var frmDisposal = new frmDisposal();
                frmDisposal.ShowDialog();
            }
           
        }

        

        private void Shared_OnCameraReadDataChange(object sender, EventArgs e)
        {
            Console.WriteLine($"[CAM] OnCameraReadDataChange #{++_handlerCallCount}, OperStatus={Shared.OperStatus}, IsReCheck={_IsReCheck}");

            // Lazy-subscribe to VscCamera frame log event
            if (!_vscCameraFrameSubscribed && Shared.vscCamera != null)
            {
                Shared.vscCamera.OnCameraFrameReceived += VscCamera_OnCameraFrameReceived;
                _vscCameraFrameSubscribed = true;
            }

            if ((Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing) || _IsReCheck) return;
            try
            {
                if (sender is DetectModel)
                {
                    var detectModel = sender as DetectModel;

                    switch (Shared.Settings.CameraList.FirstOrDefault().CameraType)
                    {
                        case CameraType.UKN:
                            break;
                        case CameraType.DM:
                        case CameraType.IS:
                        case CameraType.ISDual:
                        case CameraType.CV_X:
                        case CameraType.VS_C:
                            if (Shared.Settings.Position == SettingsModel.PositionType.BarcodePosition && Shared.Settings.EnablePosition)
                            {
                                break;
                            }
                            Console.WriteLine($"[CAM] Enqueue #{_handlerCallCount}: qr={detectModel.Text}, result={detectModel.CompareResult}");
                            _QueueBufferDataObtained.Enqueue(detectModel);
                            break;
                        default:
                            break;
                    }
                    detectModel = null;
                }
            }
            catch (Exception)
            {
            }
        }

        private void VscCamera_OnCameraFrameReceived(string rawFrame)
        {
            try
            {
                if (_rlinkLogService == null || _SelectedJob == null) return;

                // Parse raw frame: QR,QR_Result,NSX:date,Time,HSD:date,Batch,OCR_Result,Image_ID
                string[] parts = rawFrame.Split(',');
                string nsxRaw = parts.Length > 2 ? parts[2].Trim() : "";
                string timeRaw = parts.Length > 3 ? parts[3].Trim() : "";
                string hsdRaw = parts.Length > 4 ? parts[4].Trim() : "";

                string nsx = nsxRaw.StartsWith("NSX:", StringComparison.OrdinalIgnoreCase)
                    ? nsxRaw.Substring(4).Trim() : nsxRaw;
                string hsd = hsdRaw.StartsWith("HSD:", StringComparison.OrdinalIgnoreCase)
                    ? hsdRaw.Substring(4).Trim() : hsdRaw;
                string cameraNsx = string.IsNullOrEmpty(timeRaw) ? nsx : $"{nsx} {timeRaw}";

                // Store last frame data for periodic camera log
                _lastCameraFrameInfo = rawFrame;
                _lastCameraHsd = hsd;
                _lastCameraNsxWithTime = cameraNsx;
                UpdateBatchFromCamera();
                _lastCameraPacketReceived = DateTime.Now;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VscCamera_OnCameraFrameReceived] Error: {ex.Message}");
            }
        }

        private void Shared_OnCameraPositionDataChange(object sender, EventArgs e)
        {
            if ((Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing) || _IsReCheck) return;
            if (Shared.Settings.CameraList.FirstOrDefault().CameraType == CameraType.IS)
            {
                if (_ParentForm.ISSingleHandler._isCam1._inSight.Connected &&
               !_ParentForm.ISSingleHandler._isCam1._inSight.Online) // Detect camera offline on IS3800
                {
                    return;
                }
            }

            try
            {
                if (sender is DetectModel)
                {
                    var detectModel = sender as DetectModel;
                    _QueuePositionDataObtained.Enqueue(detectModel.Text);
                }
            }
            catch (Exception)
            {
            }
        }

        private void Shared_OnSerialDeviceReadDataChange(object sender, EventArgs e)
        {
            if ((Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing) || !_IsReCheck) return;
            try
            {
                if (sender is DetectModel)
                {
                    var detectModel = sender as DetectModel;
                    _QueueBufferDataObtained.Enqueue(detectModel);
                }
            }
            catch (Exception)
            {
            }
        }

        private void ChangePictureCamera()
        {
            // Cognex.InSight.Web.Controls.CvsDisplay cvsDsp = _ParentForm.ISCamera._CvsDisplay;
            if (InvokeRequired)
            {
                Invoke(new Action(() => ChangePictureCamera()));
                return;
            }
            lock (lockObject)
            {
                try
                {
                    if (Shared.Settings.CameraList[0].CameraType == CameraType.DM) // Show Image IS Series
                    {
                        pictureBoxPreview.Visible = true;
                    }

                    if (Shared.Settings.CameraList[0].CameraType == CameraType.IS) // Show Image IS Series
                    {
                        pictureBoxPreview.Visible = true;
                    }
                    if (Shared.Settings.CameraList[0].CameraType == CameraType.CV_X) // Show Image CV_X
                    {
                        pictureBoxPreview.Visible = true;
                    }
                    if (Shared.Settings.CameraList[0].CameraType == CameraType.VS_C)
                    {
                        pictureBoxPreview.Visible = true;
                    }

                }
                catch (Exception ex)
                {
#if DEBUG
                MessageBox.Show(ex.Message);
#endif
                }
            }

        }

        private void Shared_OnCameraStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelCamera();
             ChangePictureCamera();
        }

        private void Shared_OnPrintingStateChange(object sender, EventArgs e)
        {

        }

        private async void ReceiveResponseFromPrinterHandlerAsync()
        {
            _PrinterRespontCST = new CancellationTokenSource();
            var token = _PrinterRespontCST.Token;
            try
            {
                await Task.Run(() => { ReceiveResponseFromPrinterHandler(token); });
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Thread handle printed response was stopped!");
            }
            catch (Exception ex)
            {
                // Catch Error - Add by ThongThach 05/12/2023
                Console.WriteLine("Thread handle printed response was error!");
                KillAllProccessThread();
                StopProcessAsync(false, Lang.HandleError, false, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        private void ReceiveResponseFromPrinterHandler(CancellationToken token)
        {
            Debug.WriteLine("Task handle printer response work on thread " + Environment.CurrentManagedThreadId);
            while (true)
            {
                token.ThrowIfCancellationRequested();
                var sender = _QueueBufferPrinterResponseData.Dequeue();
                if (sender == null) continue;
                //if (sender is PODDataModel pod)
                //    ProjectLogger.WriteInfo($"[POD←RECV] raw='{pod.Text?.Substring(0, Math.Min(120, pod.Text?.Length ?? 0))}'");
                try
                {
                    if (sender is PODDataModel)
                    {
                        var podDataModel = sender as PODDataModel;

                        string[] pODcommand = podDataModel.Text.Split(Shared.Settings.SplitCharacter);
                        var PODResponseModel = new PODResponseModel
                        {
                            Command = pODcommand[0]
                        };

                        if (PODResponseModel != null)
                        {
                            switch (PODResponseModel.Command)
                            {

                                case "DATA":
                                    PODResponseModel.Status = pODcommand[1];
                                    //ProjectLogger.WriteInfo($"[POD←DATA] status='{PODResponseModel.Status}' | ReceivedCode={ReceivedCode}");
                                    if (PODResponseModel.Status != null && PODResponseModel.Status == "RYES")
                                    {
                                        if (ReceivedCode < 100)
                                        {
                                            lock (_ReceiveLocker)
                                            {
                                                Monitor.Pulse(_ReceiveLocker); // Notify that printer was received data
                                            }
                                        }

                                        if (Shared.OperStatus != OperationStatus.Stopped)
                                        {
                                            ReceivedCode++;
                                            //if (ReceivedCode > _NumberOfSentPrinter + 10)
                                            //{
                                            //    ProjectLogger.WriteWarning($"[RYES_DUPLICATE] ReceivedCode={ReceivedCode} > SentPrinter={_NumberOfSentPrinter} delta={ReceivedCode - _NumberOfSentPrinter}");
                                            //}
                                            //ProjectLogger.WriteDebug($"[RX] #{ReceivedCode} delta={ReceivedCode - _NumberOfSentPrinter}");
                                            //ProjectLogger.WriteDebug($"[RYES] ReceivedCode={ReceivedCode} SentPrinter={_NumberOfSentPrinter} delta={ReceivedCode - _NumberOfSentPrinter} operStatus={Shared.OperStatus}");
                                        }
                                        if (Shared.OperStatus != OperationStatus.Stopped && _SelectedJob != null) _SelectedJob.TotalReceived++;

                                        if (Shared.OperStatus == OperationStatus.Processing)
                                        {
                                            if (ReceivedCode >= 1 && _IsVerifyAndPrintMode)
                                            {
                                                Shared.OperStatus = OperationStatus.Running;
                                                Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
                                                EnableUIComponent(Shared.OperStatus);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        {string ipUnk = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                        // string rawData = podDataModel.Text?.Length > 80 ? podDataModel.Text.Substring(0, 80) + "..." : podDataModel.Text;
                                            string rawData = podDataModel.Text;
                                            //PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipUnk, "RECV", $"DATA {PODResponseModel.Status}",
                                            //rawData, "UNEXPECTED", "fault=printer: Máy in phản hồi DATA không mong đợi - có thể báo lỗi hoặc từ chối nhận dữ liệu");
                                            
                                            }
                                    }
                                    break;
                                case "RSFP":
                                    DateTime prevRsfpTs = _lastRsfpTimestamp;
                                    _lastRsfpTimestamp = DateTime.Now;
                                    _rsfpMissingSentCount = 0; // RSFP quay lại → reset bù mã
                                    _sessionFirstMonPrinted = -1; // RSFP quay lại → dùng RSFP, bỏ MON fallback
                                    _totalRsfpReceived++;
                                    if (_SelectedJob != null) _SelectedJob.TotalRsfpReceived++;
                                    _consecutiveTimeouts = 0;
                                    if (_isInTimeout)
                                    {
                                        string ipRsfpR = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                        double gap = prevRsfpTs == DateTime.MinValue ? 0 : (DateTime.Now - prevRsfpTs).TotalSeconds;
                                        PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipRsfpR, "RECV", "RSFP_RECOVER",
                                            $"timeouts={_prevConsecutiveTimeouts} gap={gap:F0}s sent={_NumberOfSentPrinter} printed={NumberPrinted} pending={_NumberOfSentPrinter - NumberPrinted}",
                                            "OK", "Máy in đã hồi phục sau khoảng thời gian không phản hồi");
                                        _isInTimeout = false;
                                    }
                                    if (_IsOnProductionMode)
                                    {
                                        lock (_PrintedResponseLocker)
                                        {
                                            _IsPrintedResponse = true; // Notify that have a printed response
                                        }
                                    }

                                    if (_IsAfterProductionMode)
                                    {
                                        // Use queue for send POD data
                                        CountFeedback++;
                                        Interlocked.Exchange(ref _countFb, CountFeedback);

                                        //  EnqueueFeedback(_countFb);
                                        _queueCountFeedback.Enqueue(_countFb);

                                        // Signal send loop to refill buffer immediately
                                        //_printerFeedbackEvent.Set();

                                        // Keep stable this for another task
                                        lock (_PrintLocker)
                                        {
                                            _IsPrintedWait = false;
                                            Monitor.Pulse(_PrintLocker);
                                        }

                                    }

                                    // ── Mode 1/2: parse index từ RSFP (ví dụ "59131/59330") ──
                                    int rsfpRowIndex = -1;
                                    bool isMode1Or2Rsfp = _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCode
                                                       || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                                                       || _SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.AutoRefreshByTime;
                                    if (isMode1Or2Rsfp && pODcommand.Length > 1)
                                    {
                                        string idxField = pODcommand[1]; // "59131/59330"
                                        int slashIdx = idxField.IndexOf('/');
                                        if (slashIdx > 0 && int.TryParse(idxField.Substring(0, slashIdx), out int printed))
                                        {
                                            rsfpRowIndex = printed - 1; // 0-based
                                            int.TryParse(idxField.Substring(slashIdx + 1), out int received);
                                            if (received > 0) _lastRsfpReceived = received;
                                            int newBuffer = Math.Max(0, received - printed);
                                            if (newBuffer != _printerBuffer)
                                            {
                                                // ProjectLogger.WriteDebug($"[BUFFER] old={_printerBuffer} new={newBuffer} received={received} printed={printed}");
                                            }
                                            _printerBuffer = newBuffer;
                                            if (_sessionFirstRsfpPrinted < 0)
                                                _sessionFirstRsfpPrinted = printed; // snapshot giá trị RSFP đầu tiên
                                            int oldPrinted = NumberPrinted;
                                            if (_sessionStartCount == 0 || printed >= _sessionStartCount)
                                                NumberPrinted = printed;
                                            else if (printed < _sessionStartCount)
                                            {
                                                _sessionStartCount = printed;
                                                NumberPrinted = printed;
                                            }
                                            // Signal send loop SAU khi NumberPrinted đã cập nhật
                                            if (NumberPrinted != oldPrinted && _IsAfterProductionMode)
                                                _printerFeedbackEvent.Set();
                                        }
                                    }

                                    //Receive data: RSFP;1/101;DATA; check.pvcfc.com.vn/?id=L927GCCR72;L927GCCR72;0;0;1
                                    pODcommand = pODcommand.Skip(3).ToArray();
                                    string printedResult = "";
                                    _QueueBufferBackupRSFPLog.Enqueue(ArrayAddOneElement(pODcommand, DateTime.Now.ToString("dd MM yy HH:mm:ss"))); // Add to queue log
                                    if (_SelectedJob.JobType == JobType.VerifyAndPrint)
                                    {
                                        if (Shared.Settings.VerifyAndPrintBasicSentMethod) // Verify and print basic mode
                                        {
                                            printedResult = pODcommand[0];
                                        }
                                        else // Verify and print compare mode
                                        {
                                            if (Shared.Settings.PrintFieldForVerifyAndPrint.Count() > 0) // No print field selected
                                            {
                                                if (_Emergency.TryGetValue(string.Join("", pODcommand), out int codeIndex))
                                                {
                                                    lock (_SyncObjCodeList)
                                                    {
                                                        string[] row = _PrintedCodeObtainFromFile[codeIndex].Where((item, idx) => idx != 1).ToArray();
                                                        printedResult = GetCompareDataByPODFormat(row, _SelectedJob.PODFormat);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                foreach (var item in _SelectedJob.PODFormat)
                                                {
                                                    if (item.Type == PODModel.TypePOD.FIELD)
                                                    {
                                                        int indexItem = item.Index - 1;
                                                        if (indexItem < pODcommand.Length)
                                                            printedResult += pODcommand[item.Index - 1];
                                                    }
                                                    else if (item.Type == PODModel.TypePOD.TEXT)
                                                    {
                                                        printedResult += item.Value;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.ProductOneQrCode
                                            && pODcommand.Length > 0)
                                        {
                                            printedResult = pODcommand[0];
                                        }
                                        else
                                        {
                                            foreach (var item in _SelectedJob.PODFormat)
                                            {
                                                /// Loi
                                                if (item.Type == PODModel.TypePOD.FIELD)
                                                {
                                                    int indexItem = item.Index - 1;
                                                    if (indexItem < pODcommand.Length)
                                                        printedResult += pODcommand[item.Index - 1];
                                                }
                                                else if (item.Type == PODModel.TypePOD.TEXT)
                                                {
                                                    printedResult += item.Value;
                                                }
                                            }
                                        }
                                    }
                                    // ── Mode 1/2: gộp raw data fields thay vì PODFormat ──
                                    string podCommand = isMode1Or2Rsfp
                                        ? string.Join("", pODcommand.Take(3))
                                        : printedResult;
                                    _QueueBufferUpdateUIPrinter.Enqueue(new Model.THTrueMilk.PrinterResponseData { RowIndex = rsfpRowIndex, Data = podCommand });
                                   // ProjectLogger.WriteInfo($"[RSFP] rowIndex={rsfpRowIndex} podCommand='{podCommand}' rawFields=[{string.Join("|", pODcommand)}]");
                                    if (_benchmarkMode)
                                        _benchmarkPrintCycle?.Set();
                                    break;
                                //case "STAR":
                                //    PODResponseModel.Command = pODcommand[0];
                                //    PODResponseModel.Status = pODcommand[1];
                                //    if (PODResponseModel.Status != null && (PODResponseModel.Status == "OK" || PODResponseModel.Status == "READY"))
                                //    {
                                //        if (podDataModel.RoleOfPrinter == RoleOfStation.ForProduct && !_IsVerifyAndPrintMode)
                                //        {
                                //            SendDataToPrinterAsync(); // Send POD data to printer when printer ready receive data
                                //        }
                                //    }
                                //    else
                                //    {
                                //        PODResponseModel.Error = pODcommand[2];
                                //        var message = "Unknown";
                                //        switch (PODResponseModel.Error)
                                //        {
                                //            case "001": message = "Open templates failed (dose not exist, others templates being opening,...)"; break;
                                //            case "002": message = "Start pages, End pages is invalid"; break;
                                //            case "003": message = "No printhead is selected"; break;
                                //            case "004": message = "Speed limit"; break;
                                //            case "005": message = "Printhead disconnected"; break;
                                //            case "006": message = "Unknown printhead"; break;
                                //            case "007": message = "No cartridges"; break;
                                //            case "008": message = "Invalid cartridges"; break;
                                //            case "009": message = "Out of ink"; break;
                                //            case "010": message = "Cartridges is locked"; break;
                                //            case "011": message = "Invalid version"; break;
                                //            case "012": message = "Incorrect printhead"; break;
                                //            default:
                                //                break;
                                //        }

                                //        Invoke(new Action(() =>
                                //        {
                                //            StopProcessAsync(false, Lang.SomePrintParametersAreMissing + ": " + message, false, true);
                                //        }));

                                //    }
                                //    break;
                                case "STAR":
                                    PODResponseModel.Status = pODcommand[1];
                                    {string ipStar = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                        string errorCode = pODcommand.Length > 2 ? pODcommand[2] : "?";
                                        string starDetail = PODResponseModel.Status == "OK" || PODResponseModel.Status == "READY"
                                            ? $"operStatus={Shared.OperStatus}"
                                            : $"error={errorCode} operStatus={Shared.OperStatus}";
                                        string starNote = PODResponseModel.Status == "OK" || PODResponseModel.Status == "READY"
                                            ? "Máy in sẵn sàng nhận lệnh"
                                            : $"fault=printer: Máy in báo lỗi khi nhận lệnh STAR - mã lỗi {errorCode}";
                                        PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipStar, "RECV", $"STAR {PODResponseModel.Status}", starDetail,
                                        PODResponseModel.Status == "OK" || PODResponseModel.Status == "READY" ? "OK" : $"ERR_{pODcommand.ElementAtOrDefault(2)}",
                                        starNote);}
                                    if (PODResponseModel.Status == "OK" || PODResponseModel.Status == "READY")
                                    {
                                        if (podDataModel.RoleOfPrinter == RoleOfStation.ForProduct && !_IsVerifyAndPrintMode)
                                        {
                                            ProjectLogger.WriteInfo($"[STAR_TRIGGERS_SEND] STAR OK received. sent={_NumberOfSentPrinter} printed={NumberPrinted} buffer={_printerBuffer} operStatus={Shared.OperStatus}");
                                            SendDataToPrinterAsync();
                                        }
                                    }
                                    else
                                    {
                                        PODResponseModel.Error = pODcommand[2];
                                        var message = "Unknown";
                                        DeviceExceptionType? exType = null;

                                        switch (PODResponseModel.Error)
                                        {
                                            case "001": message = "Open templates failed"; break;
                                            case "002": message = "Start/End pages invalid"; break;
                                            case "003": message = "No printhead selected"; break;
                                            case "004": message = "Speed limit"; break;
                                             case "005":
                                                 LogDisconnect("STAR_005");
                                                 message = "Printhead disconnected";
                                                 exType = DeviceExceptionType.PrinterDisconnected; break;
                                            case "006": message = "Unknown printhead"; break;
                                            case "007":
                                                message = "No cartridges";
                                                exType = DeviceExceptionType.PrinterNoCartridge; break;
                                            case "008":
                                                message = "Invalid cartridges";
                                                exType = DeviceExceptionType.PrinterNoCartridge; break;
                                            case "009":
                                                message = "Out of ink";
                                                exType = DeviceExceptionType.PrinterLowInk; break;
                                            case "010": message = "Cartridges locked"; break;
                                            case "011": message = "Invalid version"; break;
                                            case "012": message = "Incorrect printhead"; break;
                                            case "013": message = "Start print processing"; break;
                                            case "014": message = "Invalid loop values"; break;
                                            case "015":
                                                message = "Ink low";
                                                exType = DeviceExceptionType.PrinterLowInk; break;
                                            case "000": message = "Unknown"; break;
                                            case "016": message = "Not response"; break;
                                            case "017": message = "Incorrect start key"; break;
                                            case "018": message = "Conflict cartridge Funai";
                                                exType = DeviceExceptionType.PrinterNoCartridge; break;
                                        }

                                        if (exType.HasValue)
                                            _ParentForm?.NotifyDeviceException(exType.Value,
                                                $"STAR error {PODResponseModel.Error}: {message}");

                                        Invoke(new Action(() =>
                                        {
                                            StopProcessAsync(false,
                                                Lang.SomePrintParametersAreMissing + ": " + message, false, true);
                                        }));
                                    }
                                    break;
                                case "STOP":
                                    _PrinterStatus = PrinterStatus.Stop;
                                    Shared.OperStatus = OperationStatus.Processing;
                                    PODResponseModel.Status = pODcommand[1];
                                    if (PODResponseModel.Status != null && PODResponseModel.Status == "OK")
                                    {
                                        _PrinterStatus = PrinterStatus.Null;
                                        Shared.OperStatus = OperationStatus.Stopped;
                                        lock (_StopLocker)
                                        {
                                            _IsStopOK = true;
                                            Monitor.PulseAll(_StopLocker); //Notifications have stopped
                                        }
                                    }
                                    else
                                    {
                                        Shared.OperStatus = OperationStatus.Stopped;
                                    }
                                    break;
                                //case "MON":
                                //    PODResponseModel.Status = pODcommand[3];
                                //    if (PODResponseModel.Status == "Stop" &&
                                //        Shared.OperStatus == OperationStatus.Running &&
                                //        _SelectedJob.CompareType == CompareType.Database &&
                                //        _SelectedJob.JobType != JobType.StandAlone &&
                                //        !_IsReCheck &&
                                //        !_isRlinkStop)
                                //    {
                                //        Invoke(new Action(() =>
                                //        {
                                //            StopProcessAsync(false, "Printer stops suddenly!", false, true);
                                //        }));
                                //    }
                                //    switch (PODResponseModel.Status)
                                //    {
                                //        case "Stop": _PrinterStatus = PrinterStatus.Stop; break;
                                //        case "Processing": _PrinterStatus = PrinterStatus.Processing; break;
                                //        case "Ready":
                                //        case "Start":
                                //            _PrinterStatus = PrinterStatus.Ready;
                                //            _PrinterStatus = PrinterStatus.Start; break;
                                //        case "Printing": _PrinterStatus = PrinterStatus.Printing; break;
                                //        case "Connected": _PrinterStatus = PrinterStatus.Connected; ; break;
                                //        case "Disconnected": _PrinterStatus = PrinterStatus.Disconnected; break;
                                //        case "Error": _PrinterStatus = PrinterStatus.Error; ; break;
                                //        case "Disable": _PrinterStatus = PrinterStatus.Disable; break;
                                //        case "": _PrinterStatus = PrinterStatus.Null; break;
                                //        default:
                                //            break;
                                //    }
                                //    break;
                                case "MON":
                                    _receivedFinishPrintFormMON = int.Parse(pODcommand[4]);
                                    _lastMonTimestamp = DateTime.Now;

                                    // ── MON fallback: signal send loop khi RSFP stale ──
                                    if (_IsAfterProductionMode && IsRsfpStale())
                                    {
                                        if (_sessionFirstMonPrinted < 0)
                                            _sessionFirstMonPrinted = _receivedFinishPrintFormMON;
                                        _printerFeedbackEvent.Set();
                                    }

                                    PODResponseModel.Status = pODcommand[3]; // PrinterStatus
                                    {string ipMon = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                    PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipMon, "RECV", $"MON {PODResponseModel.Status}", $"operStatus={Shared.OperStatus} printerStatus={_PrinterStatus}", "OK", "Trạng thái máy in thay đổi");}

                                    if (PODResponseModel.Status == "Stop" &&
                                        Shared.OperStatus == OperationStatus.Running &&
                                        _SelectedJob.CompareType == CompareType.Database &&
                                        _SelectedJob.JobType != JobType.StandAlone &&
                                        !_IsReCheck && !_isRlinkStop)
                                    {
                                        Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                                        Invoke(new Action(() =>
                                        {
                                            StopProcessAsync(false, "Máy in dừng đột ngột!", false, true);
                                        }));
                                    }

                                    switch (PODResponseModel.Status)
                                    {
                                        case "Stop": _PrinterStatus = PrinterStatus.Stop; break;
                                        case "Processing": _PrinterStatus = PrinterStatus.Processing; break;
                                        case "Ready":
                                        case "Start":
                                            _PrinterStatus = PrinterStatus.Ready;
                                            _PrinterStatus = PrinterStatus.Start; break;
                                        case "Printing": _PrinterStatus = PrinterStatus.Printing; break;
                                        case "Connected": _PrinterStatus = PrinterStatus.Connected; break;
                                        case "Disconnected":
                                            LogDisconnect("MON_DISCONNECTED");
                                            _PrinterStatus = PrinterStatus.Disconnected;
                                            _ParentForm?.NotifyDeviceException(
                                                DeviceExceptionType.PrinterDisconnected,
                                                "MON: Máy in mất kết nối (Disconnected)");
                                            // ── Dừng và thông báo như STAR ──────────────────────────
                                            Invoke(new Action(() =>
                                                StopProcessAsync(false, "Máy in mất kết nối trong khi đang in!", false, true)));
                                            break;

                                        case "Error":
                                            _PrinterStatus = PrinterStatus.Error;
                                            // ── Feature 2: kiểm tra PrintheadStatus (pODcommand[5]) ──
                                            // PrintheadStatus phân cách bằng 0x1D (group separator)
                                            if (pODcommand.Length > 5 && !string.IsNullOrEmpty(pODcommand[5]))
                                            {
                                                string[] headStatuses = pODcommand[5].Split('\x1D');
                                                bool hasNoCartridge = headStatuses.Any(s =>
                                                    s == "No" || s == "Invalid");
                                                bool hasInkLow = headStatuses.Any(s =>
                                                    s == "InkLow" || s == "OutOfInk");

                                                if (hasNoCartridge)
                                                    _ParentForm?.NotifyDeviceException(
                                                        DeviceExceptionType.PrinterNoCartridge,
                                                        "MON Error: No/Invalid cartridge — " + pODcommand[5]);
                                                else if (hasInkLow)
                                                    _ParentForm?.NotifyDeviceException(
                                                        DeviceExceptionType.PrinterLowInk,
                                                        "MON Error: InkLow/OutOfInk — " + pODcommand[5]);
                                                else
                                                {
                                                    LogDisconnect("MON_ERROR");
                                                    _ParentForm?.NotifyDeviceException(
                                                        DeviceExceptionType.PrinterDisconnected,
                                                        "MON Error: " + pODcommand[5]);
                                                }
                                            }
                                            else
                                            {
                                                LogDisconnect("MON_ERROR_NODETAIL");
                                                _ParentForm?.NotifyDeviceException(
                                                    DeviceExceptionType.PrinterDisconnected,
                                                    "MON: Printer Error (no detail)");
                                            }
                                            break;

                                        case "Disable": _PrinterStatus = PrinterStatus.Disable; break;
                                        case "": _PrinterStatus = PrinterStatus.Null; break;
                                        default: break;
                                    }
                                    break;
                                case "RYES":
                                    // Printer xác nhận đã nhận dữ liệu (lặp từ DATA handler)
                                    break;
                                case "RSLI":
                                    // Printer gửi thông tin template/label — không cần xử lý
                                    break;
                                case "RSAL":
                                    // Printer alarm khi đang in: RSAL;{code};{headIndex}
                                    {
                                        string alarmErr = pODcommand.Length > 1 ? pODcommand[1] : "?";
                                        string alarmIdx = pODcommand.Length > 2 ? pODcommand[2] : "?";

                                        string plcSignal = "";
                                        DeviceExceptionType? alarmEx = null;
                                        switch (alarmErr)
                                        {
                                            case "001": plcSignal = "001_OVER_SPEED_ERROR"; break;
                                            case "002": plcSignal = "002_RECEIVE_DATA_TIMEOUT"; break;
                                            case "003": plcSignal = "003_DELAY_DATA_ERROR"; break;
                                            case "004": plcSignal = "004_NO_CARTRIDGE"; alarmEx = DeviceExceptionType.PrinterNoCartridge; break;
                                            case "005": plcSignal = "005_INVALID_CARTRIDGE"; alarmEx = DeviceExceptionType.PrinterNoCartridge; break;
                                            case "006": plcSignal = "006_LOCK_CARTRIDGE"; break;
                                             case "007": plcSignal = "007_INK_OUT"; alarmEx = DeviceExceptionType.PrinterLowInk; break;
                                             case "008": plcSignal = "008_INK_LOW"; break;
                                            case "009": plcSignal = "009_EMPTY_DATA"; break;
                                            case "010": plcSignal = "010_POD_EMPTY_BUFFER"; break;
                                            case "011": plcSignal = "011_PRINTER_PROCESSING_WAITING_DATA"; break;
                                            default: plcSignal = $"UNKNOWN_{alarmErr}"; break;
                                        }

                                        // Phân loại Warning/Error
                                        bool isWarning = alarmErr == "003" || alarmErr == "008" || alarmErr == "009" || alarmErr == "010" || alarmErr == "011";

                                        // Gửi tín hiệu PLC theo mã lỗi
                                        switch (alarmErr)
                                        {
                                            case "008":
                                            case "011":
                                                Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Yellow);
                                                break;
                                            default:
                                                Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                                                RequestAutoStop($"RSAL lỗi {alarmErr}: {plcSignal}");
                                                break;
                                        }

                                        // Log
                                        string jobRsal = _SelectedJob?.FileName ?? "";
                                        string ipRsal = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                        PrinterLogger.Log(jobRsal, ipRsal, "RECV", $"RSAL {alarmErr}",
                                            $"error={alarmErr} head={alarmIdx} plc={plcSignal}",
                                            "ALARM", $"Máy in báo alarm tại đầu in {alarmIdx}: {plcSignal}");

                                        // Thông báo parent (đã thay bằng SendPrinterSignalToPlc ở trên)
                                        // if (!isWarning && alarmEx.HasValue)
                                        //     _ParentForm?.NotifyDeviceException(alarmEx.Value, $"RSAL {alarmErr}: {plcSignal} at head {alarmIdx}");

                                        // Popup UI (tất cả các mã, có dedup)
                                        if (_lastRsalCode != alarmErr || (DateTime.Now - _lastRsalPopupTime).TotalSeconds > 5)
                                        {
                                            _lastRsalCode = alarmErr;
                                            _lastRsalPopupTime = DateTime.Now;

                                            var rsalMessages = new Dictionary<string, string>
                                            {
                                                {"001", "Tốc độ in vượt quá giới hạn"},
                                                {"002", "Quá thời gian nhận dữ liệu"},
                                                {"003", "Lỗi trễ dữ liệu"},
                                                {"004", "Không có cartridge"},
                                                {"005", "Cartridge không hợp lệ"},
                                                {"006", "Cartridge bị khóa"},
                                                {"007", "Hết mực"},
                                                {"008", "Mực sắp hết"},
                                                {"009", "Dữ liệu in trống"},
                                                {"010", "Buffer máy in trống"},
                                                {"011", "Máy in đang chờ dữ liệu"},
                                            };
                                            var rsalInstructions = new Dictionary<string, string>
                                            {
                                                {"001", "Kiểm tra tốc độ băng tải/máy in"},
                                                {"002", "Kiểm tra kết nối dữ liệu đến máy in"},
                                                {"003", "Kiểm tra đường truyền dữ liệu"},
                                                {"004", "Lắp cartridge mới"},
                                                {"005", "Thay cartridge đúng chủng loại"},
                                                {"006", "Mở khóa cartridge"},
                                                {"007", "Thay cartridge/mực in mới"},
                                                {"008", "Chuẩn bị thay mực"},
                                                {"009", "Kiểm tra dữ liệu đầu vào"},
                                                {"010", "Đợi máy in nhận dữ liệu mới"},
                                                {"011", "Máy in đang xử lý, vui lòng đợi"},
                                            };
                                            string friendlyMsg = rsalMessages.TryGetValue(alarmErr, out string msg)
                                                ? msg
                                                : $"Mã lỗi [{alarmErr}]";
                                            string instruction = rsalInstructions.TryGetValue(alarmErr, out string instr)
                                                ? instr
                                                : "Liên hệ kỹ thuật";

                                            BeginInvoke(new Action(() =>
                                            {
                                                if (isWarning)
                                                {
                                                    if (_rsalWarningForm == null || _rsalWarningForm.IsDisposed)
                                                    {
                                                        _rsalWarningForm = new RsalAlertForm(true);
                                                        _rsalWarningForm.FormClosed += (o, args) => _rsalWarningForm = null;
                                                        _rsalWarningForm.AppendMessage(alarmIdx, alarmErr, friendlyMsg, instruction);
                                                        PositionForm(_rsalWarningForm, true);
                                                        _rsalWarningForm.Show();
                                                    }
                                                    else
                                                    {
                                                        _rsalWarningForm.AppendMessage(alarmIdx, alarmErr, friendlyMsg, instruction);
                                                    }
                                                }
                                                else
                                                {
                                                    if (_rsalErrorForm == null || _rsalErrorForm.IsDisposed)
                                                    {
                                                        _rsalErrorForm = new RsalAlertForm(false);
                                                        _rsalErrorForm.FormClosed += (o, args) => _rsalErrorForm = null;
                                                        _rsalErrorForm.AppendMessage(alarmIdx, alarmErr, friendlyMsg, instruction);
                                                        PositionForm(_rsalErrorForm, false);
                                                        _rsalErrorForm.Show();
                                                    }
                                                    else
                                                    {
                                                        _rsalErrorForm.AppendMessage(alarmIdx, alarmErr, friendlyMsg, instruction);
                                                    }
                                                }
                                            }));
                                        }
                                    }
                                    break;
                                case "RSMPOD":
                                    // Printer feedback: value = số mã đã in thực tế
                                    if (pODcommand.Length > 1 && int.TryParse(pODcommand[1], out int rsmpodVal) && rsmpodVal > _printerAckedCount)
                                    {
                                        _printerAckedCount = rsmpodVal;
                                        if (_lastRsfpReceived > 0)
                                            _printerBuffer = Math.Max(0, _lastRsfpReceived - rsmpodVal);
                                        string ipRsmpod = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                        PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipRsmpod, "RECV", "RSMPOD",
                                            $"value={rsmpodVal} myPrinted={NumberPrinted} sent={_NumberOfSentPrinter} buffer={_printerBuffer}",
                                            "SYNC", "Printer gửi RSMPOD - đồng bộ buffer");
                                        _printerFeedbackEvent.Set();
                                    }
                                    break;
                                default:
                                    {string ipDef = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                    string rawDef = podDataModel.Text?.Length > 80 ? podDataModel.Text.Substring(0, 80) + "..." : podDataModel.Text;
                                    PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipDef, "RECV", $"UNKNOWN_{PODResponseModel.Command}",
                                        rawDef, "UNEXPECTED", "Máy in gửi lệnh không xác định");}
                                    break;
                            }
                        }
                    }
                }
                catch
                {
                    LoggingController.SaveHistory(
                        string.Format("Thread Exception"),
                        Lang.Error,
                        string.Format("Printed response handler"),
                        SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                        LoggingType.Error);
                }
            }
        }

        private static string[] ArrayAddOneElement(string[] originalArray, string newElement)
        {
            try
            {
                string[] newArray = new string[originalArray.Length + 1];
                Array.Copy(originalArray, newArray, originalArray.Length);
                newArray[newArray.Length - 1] = newElement;
                return newArray;
            }
            catch (Exception)
            {
                return originalArray;
            }

        }

        private void Shared_OnPrinterDataChange(object sender, EventArgs e)
        {
            if (sender is PODDataModel pod)
               // PrinterLogger.LogRaw(pod.IP, pod.RawText, pod.RawText?.Length ?? 0, _SelectedJob?.FileName ?? "");
            _QueueBufferPrinterResponseData.Enqueue(sender);
        }

        private async void SendDataToPrinterAsync()
        {
            // ── CRITICAL: Không start send loop nếu form đang force close/dispose ──
            if (_forceClose || IsDisposed || !IsHandleCreated)
            {
                ProjectLogger.WriteInfo($"[SEND_SKIP] Form is closing/disposed, skip send loop");
                return;
            }

            // Đợi old loop finish hoàn toàn trước khi start mới
            int waitMs = 0;
            while (Interlocked.CompareExchange(ref _isSendLoopRunning, 0, 0) == 1 && waitMs < 5000)
            {
                Thread.Sleep(100);
                waitMs += 100;
            }
            if (waitMs >= 5000)
                ProjectLogger.WriteWarning($"[SEND_START] Timeout waiting for old send loop to finish");

            // Thread-safe guard: chỉ 1 thread được start
            if (Interlocked.CompareExchange(ref _isSendLoopRunning, 1, 0) == 1)
            {
                ProjectLogger.WriteInfo($"[SEND_GUARD] Skip: another thread already started send loop");
                return;
            }

            try
            {
                // Chỉ tạo CTS mới nếu chưa có hoặc đã cancelled
                if (_SendDataToPrinterTokenCTS == null || _SendDataToPrinterTokenCTS.IsCancellationRequested)
                {
                    _SendDataToPrinterTokenCTS = new CancellationTokenSource();
                }
                var token = _SendDataToPrinterTokenCTS.Token;
                await Task.Run(() => { SendPODDataProductToPrinter(token); });
            }
            finally
            {
                Interlocked.Exchange(ref _isSendLoopRunning, 0);
            }
        }

        //private void SendPODDataProductToPrinter(CancellationToken token)
        //{
        //    Thread.Sleep(500);  // Wait printer ready
        //    int counter = 0;
        //    List<string[]> codeList = null;
        //    List<string> tmpListLog = new List<string>();
        //    lock (_SyncObjCodeList)
        //    {
        //        codeList = new List<string[]>(_PrintedCodeObtainFromFile); //Clone list
        //    }
        //    lock (_PrintLocker)
        //    {
        //        _IsPrintedWait = false;
        //        _PrintedResult = ComparisonResult.Valid;
        //    }
        //    try
        //    {
        //        var spinWait = new SpinWait();
        //        int startIndex = codeList.FindIndex(x => x[1] != "Printed"); // Get the first not-printed code in database 
        //        if (startIndex == -1) return;
        //        _IsPrintedWait = true; // Wait for 100 code first
        //        for (int codeIndex = startIndex; codeIndex < codeList.Count(); codeIndex++)
        //        {
        //            //   stopw = Stopwatch.StartNew();
        //            token.ThrowIfCancellationRequested();
        //            string[] codeModel = codeList[codeIndex]; // Last index of valid code
        //            int statusIndex = 1;
        //            if (codeModel[statusIndex] != "Printed" && (codeModel[statusIndex] != "Duplicate" || Shared.Settings.DuplicatedDBEnable)) // Check if current code is printed or duplicate 
        //            {
        //                //  string data = "";
        //                token.ThrowIfCancellationRequested();
        //                string data = string.Join(Shared.Settings.SplitCharacter.ToString(), codeModel.Skip(2).ToArray());// Init send data
        //                //string command = string.Format("DATA;{0}", data); // Init send command
        //                string command = $"DATA;{data}";

        //                if (podController != null)
        //                {
        //                    podController.Send(command);
        //                    NumberOfSentPrinter++;
        //                    tmpListLog = command.Split(Shared.Settings.SplitCharacter).Skip(1).ToList();
        //                    tmpListLog.Add(DateTime.Now.ToString("dd MM yy HH:mm:ss"));
        //                    _QueueBufferBackupSendLog.Enqueue(tmpListLog.ToArray());
        //                    tmpListLog.Clear();
        //                }
        //                counter++;
        //                if (Shared.OperStatus == OperationStatus.Processing) // Change operation status
        //                {
        //                    if (counter >= Shared.Settings.PrinterList[0].NumberBuffer1StSend && _IsAfterProductionMode)  // Check allow system runing, not waiting util send data complete
        //                    {
        //                        Shared.OperStatus = OperationStatus.Running; // Update user interface the system is ready
        //                        Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
        //                        EnableUIComponent(Shared.OperStatus);
        //                    }
        //                    else if (counter >= 1 && _IsOnProductionMode)
        //                    {
        //                        Shared.OperStatus = OperationStatus.Running;
        //                        Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
        //                        EnableUIComponent(Shared.OperStatus);
        //                    }
        //                }

        //                if (_IsOnProductionMode)
        //                {
        //                    lock (_PrintLocker)
        //                    {
        //                        _IsPrintedWait = true;
        //                        while (_IsPrintedWait) Monitor.Wait(_PrintLocker); // Waiting until code is print
        //                        if (_PrintedResult != ComparisonResult.Valid && _PrintedResult != ComparisonResult.Duplicated) // Check checked result to know if need to re sent code 
        //                        {
        //                            codeIndex--;
        //                        }
        //                    }
        //                }
        //                else if (_IsAfterProductionMode)
        //                {
        //                    if (counter < Shared.Settings.PrinterList[0].NumberBuffer1StSend)
        //                    {
        //                        Thread.Sleep(Shared.Settings.PrinterList[0].TimeDelaySendFirstBuffer);
        //                    }
        //                    else
        //                    {
        //                        while (true)
        //                        {
        //                            if (_queueCountFeedback.TryDequeue(out int res))
        //                            {
        //                                break;
        //                            }
        //                            if (!Shared.Settings.PrinterList[0].EnableSendTurboSpeed) // Use mode wait data mode
        //                            {
        //                                spinWait.SpinOnce();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if (Shared.OperStatus == OperationStatus.Processing)
        //        {
        //            Shared.OperStatus = OperationStatus.Running;  // Update user interface the system is ready
        //            EnableUIComponent(Shared.OperStatus);
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        Console.WriteLine("Thread send data to printer was stopped!");

        //        _BackupSendLogCancelTokenSource?.Cancel();
        //        _BackupSendLogCancelTokenSource?.Dispose();
        //        _BackupSendLogCancelTokenSource = null;
        //        _QueueBufferBackupSendLog.Enqueue(null);
        //        _QueueBufferBackupSendLog.Clear();

        //        _BackupRSFPLogCancelTokenSource?.Cancel();
        //        _QueueBufferBackupRSFPLog.Enqueue(null);
        //        _QueueBufferBackupRSFPLog.Clear();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Thread send data to printer was error!");  // Catch Error - Add by ThongThach 05/12/2023
        //        StopProcessAsync(false, Lang.HandleError, false, true);
        //        Shared.RaiseOnLogError(ex);
        //        EnableUIComponent(OperationStatus.Stopped);
        //    }
        //}
        private void SendPODDataProductToPrinter(CancellationToken token)
        {
            Thread.Sleep(50);  // Wait printer ready
            _printerFeedbackEvent.Reset(); // Đảm bảo event ở trạng thái unsignaled
            while (_queueCountFeedback.TryDequeue(out _)) { }
            _printerAckedCount = -1; // reset RSMPOD counter
            _lastReconnectTime = DateTime.MinValue;
            _printerBuffer = 0;
            _lastRsfpReceived = 0;
            _rsfpMissingSentCount = 0; // reset khi start job mới
            _sessionFirstRsfpPrinted = 0;
            _sessionFirstMonPrinted = -1; // reset MON fallback
            _lastMonTimestamp = DateTime.MinValue;
            _lastPrintedPageFormPrinter = NumberPrinted; // snapshot số đã in đầu phiên — dùng cho buffer formula

            // Ưu tiên BufferCount: Job > Settings > Printer fallback (tối thiểu 200)
            int bufferCount = _SelectedJob?.THJobBufferCount > 0
                ? _SelectedJob.THJobBufferCount
                : (Shared.Settings?.THBufferCount > 0
                    ? Shared.Settings.THBufferCount
                    : Math.Max(Shared.Settings.PrinterList[0].NumberBuffer1StSend, 200));

            int counter = 0;
            int confirmedAtStart = NumberPrinted; // snapshot số đã in trước phiên này (để check không in quá)
            int baselineSent = _NumberOfSentPrinter;     // snapshot gửi đầu phiên
            int baselinePrinted = NumberPrinted;          // snapshot in đầu phiên
            List<string> tmpListLog = new List<string>();
            int startIndex = -1;
            int totalCount = 0;
            List<string[]> codeList = null;
            lock (_SyncObjCodeList)
            {
                codeList = new List<string[]>(_PrintedCodeObtainFromFile);
                totalCount = codeList.Count;
                for (int i = 0; i < totalCount; i++)
                {
                    var row = _PrintedCodeObtainFromFile[i];
                    if (row.Length > 1 && row[1] != "Printed")
                    {
                        startIndex = i;
                        break;
                    }
                }
            }
            if (startIndex == -1) return;
            ProjectLogger.WriteInfo($"[SEND_START] startIndex={startIndex} totalCount={totalCount} bufferCount={bufferCount} NumberTotalsCode={_SelectedJob?.NumberTotalsCode} sent={_NumberOfSentPrinter} printed={NumberPrinted} jobFileName={_SelectedJob?.FileName}");
            PrinterLogger.Log(_SelectedJob?.FileName ?? "", Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "", "STATE", "SEND_LOOP_START",
                $"sent={_NumberOfSentPrinter} printed={NumberPrinted} baselineSent={baselineSent} baselinePrinted={baselinePrinted} bufferCount={bufferCount} totalCount={totalCount}",
                "OK", "Bắt đầu vòng lặp gửi dữ liệu xuống máy in");

            lock (_PrintLocker)
            {
                _IsPrintedWait = false;
                _PrintedResult = ComparisonResult.Valid;
            }
            try
            {
                var spinWait = new SpinWait();
                _IsPrintedWait = true; // Wait for 100 code first
                if (_connectedSince == DateTime.MinValue)
                    _connectedSince = DateTime.Now;
                string jobStart = _SelectedJob?.FileName ?? "";
                string ipStart = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                PrinterLogger.Log(jobStart, ipStart, "STATE", "START",
                    $"totalCodes={totalCount} bufferCount={bufferCount} startIndex={startIndex} sent={_NumberOfSentPrinter} printed={NumberPrinted}",
                    "OK", "Bắt đầu gửi dữ liệu xuống máy in");

                // ── Set batch từ NSX dòng đầu tiên (trước khi camera trigger API) ──
                if (string.IsNullOrEmpty(_SelectedJob?.THJobBatchNo) && codeList.Count > 0)
                {
                    var firstRow = _PrintedCodeObtainFromFile[0];
                    if (firstRow.Length > 4)
                    {
                        string nsxVal = firstRow[firstRow.Length - 4]?.Trim() ?? "";
                        if (!string.IsNullOrEmpty(nsxVal))
                            _SelectedJob.THJobBatchNo = nsxVal;
                    }
                }


                for (int codeIndex = startIndex; codeIndex < totalCount; codeIndex++)
                {
                    // Kiểm tra tổng đã in (cũ + mới) không vượt quá số lượng yêu cầu
                    if (_SelectedJob.NumberTotalsCode > 0 && confirmedAtStart + counter >= _SelectedJob.NumberTotalsCode)
                        break;

                    //   stopw = Stopwatch.StartNew();
                    token.ThrowIfCancellationRequested();

                    //string[] codeModel;
                    //lock (_SyncObjCodeList)
                    //{
                    //    if (codeIndex >= _PrintedCodeObtainFromFile.Count) break;
                    //    codeModel = _PrintedCodeObtainFromFile[codeIndex];
                    //


                    //Nhan.To added 05092026
                    // Dòng 11121-11128 — thêm 1 dòng null check


                    string[] codeModel;
                    lock (_SyncObjCodeList)
                    {
                        if (codeIndex >= _PrintedCodeObtainFromFile.Count) break;
                        codeModel = _PrintedCodeObtainFromFile[codeIndex];
                    }
                    if (codeModel == null || codeModel.Length < 5) continue;  //Nhan.To added 05092026 - null check
                    int statusIndex = 1;
                    if (codeModel[statusIndex] != "Printed" && (codeModel[statusIndex] != "Duplicate" || Shared.Settings.DuplicatedDBEnable)) // Check if current code is printed or duplicate 
                    {
                        //  string data = "";
                        token.ThrowIfCancellationRequested();
                        string data = string.Join(Shared.Settings.SplitCharacter.ToString(), codeModel.Skip(2).Take(codeModel.Length - 4).ToArray());// Init send data
                        string command = $"DATA;{data}";

                        if (podController != null)
                        {
                            // Kiểm tra kết nối TCP — tự động reconnect nếu mất kết nối
                            DateTime tcpDisconnectStart = DateTime.Now;
                            while (!podController.IsConnected())
                            {
                                string ipConn = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                string connUpTime = _connectedSince == DateTime.MinValue
                                    ? "unknown" : $"lúc {_connectedSince:HH:mm:ss} ({(DateTime.Now - _connectedSince).TotalMinutes:F0}ph trước)";
                                string stateDetail = $"counter={counter} sent={_NumberOfSentPrinter} printed={NumberPrinted} printerStatus={_PrinterStatus} mấtLần={_consecutiveTimeouts} kết nốiTừ={connUpTime} fault=network";
                                PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipConn, "STATE", "DISCONNECT",
                                    stateDetail,
                                    "WARN", "fault=network: Mất kết nối TCP với máy in, đang thử kết nối lại...");
                                token.ThrowIfCancellationRequested();

                                // Sau 10s retry không được → PLC + thông báo
                                if ((DateTime.Now - tcpDisconnectStart).TotalSeconds >= 10
                                    && !Shared.PrinterDisconnectAlertShown
                                    && Shared.SensorController != null
                                    && Shared.IsSensorControllerConnected)
                                {
                                    Shared.PrinterDisconnectAlertShown = true;
                                    string ipPrinterLog = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                    PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipPrinterLog, "CRITICAL", "DISCONNECT_10S",
                                        $"sent={_NumberOfSentPrinter} printed={NumberPrinted} timeouts={_consecutiveTimeouts}",
                                        "ALERT", "Máy in mất kết nối >10s → RED + stop");
                                    Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Red);
                                    RequestAutoStop("Máy in mất kết nối >10s khi đang in");
                                    BeginInvoke(new Action(() =>
                                    {
                                        CustomMessageBox.ShowCenterScreen("Máy in mất kết nối hơn 10 giây, vui lòng kiểm tra lại kết nối!",
                                            "Mất kết nối máy in", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }));
                                }

                                bool retry = podController.Connect();
                                if (retry)
                                {
                                    _connectedSince = DateTime.Now;
                                    _consecutiveTimeouts = 0;
                                    _isInTimeout = false;
                                    _lastReconnectTime = DateTime.Now;
                                    if (Shared.PrinterDisconnectAlertShown)
                                    {
                                        Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Green);
                                    }
                                    PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipConn, "STATE", "RECONNECT_OK",
                                        $"sent={_NumberOfSentPrinter} printed={NumberPrinted} baselineSent={baselineSent} baselinePrinted={baselinePrinted}" +
                                        $" pending={_NumberOfSentPrinter - NumberPrinted} sessionPending={_NumberOfSentPrinter - baselineSent - (NumberPrinted - baselinePrinted)}" +
                                        $" connectedSince={_connectedSince:HH:mm:ss}",
                                        "OK", "Đã kết nối lại thành công với máy in");
                                    break;
                                }
                                PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipConn, "STATE", "DISCONNECT",
                                    stateDetail,
                                    "WARN", "fault=network: Kết nối lại máy in thất bại, sẽ thử lại sau 1s");
                                Thread.Sleep(1000);
                            }

                            // Batch = NSX của dòng hiện tại (từ gói tin đã gửi)
                            string nsxVal = codeModel[codeModel.Length - 4]?.Trim() ?? "";
                            if (!string.IsNullOrEmpty(nsxVal))
                            {
                                string[] batchFormats = { "dd MM yy", "dd/MM/yy", "dd/MM/yyyy", "dd-MM-yy", "dd-MM-yyyy", "ddMMyy", "ddMMyyyy" };
                                bool parsed = false;
                                foreach (var fmt in batchFormats)
                                {
                                    if (DateTime.TryParseExact(nsxVal, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime nsxDt))
                                    {
                                        _SelectedJob.THJobBatchNo = nsxDt.ToString("dd MM yy");
                                        parsed = true;
                                        break;
                                    }
                                }
                                if (!parsed)
                                {
                                    // Nếu không parse được, giữ nguyên giá trị gốc
                                    _SelectedJob.THJobBatchNo = nsxVal;
                                }
                            }
                            // Đảm bảo buffer không vượt quá bufferCount trước khi gửi
                            if (_IsAfterProductionMode && counter >= bufferCount)
                            {
                                int sessionPrintedPreSend;
                                if (!IsRsfpStale() && _sessionFirstRsfpPrinted >= 0)
                                    sessionPrintedPreSend = NumberPrinted - _sessionFirstRsfpPrinted;
                                else if (_sessionFirstMonPrinted >= 0)
                                    sessionPrintedPreSend = _receivedFinishPrintFormMON - _sessionFirstMonPrinted;
                                else
                                    sessionPrintedPreSend = 0;
                                int pendingPreSend = (_NumberOfSentPrinter - baselineSent) - sessionPrintedPreSend;
                                while (pendingPreSend >= bufferCount)
                                {
                                    token.ThrowIfCancellationRequested();
                                    _printerFeedbackEvent.WaitOne(2000);
                                    if (!IsRsfpStale() && _sessionFirstRsfpPrinted >= 0)
                                        sessionPrintedPreSend = NumberPrinted - _sessionFirstRsfpPrinted;
                                    else if (_sessionFirstMonPrinted >= 0)
                                        sessionPrintedPreSend = _receivedFinishPrintFormMON - _sessionFirstMonPrinted;
                                    else
                                        sessionPrintedPreSend = 0;
                                    pendingPreSend = (_NumberOfSentPrinter - baselineSent) - sessionPrintedPreSend;
                                }
                            }
                            // Track send timing for RSFP latency measurement
                            _lastDataSendTimestamp = DateTime.Now;

                            //NhanTo_260907_1003 - Lock rebuild để tránh race condition
                            // Send loop gửi data + set sendTime phải atomic
                            // Nếu không lock, rebuild có thể update QR trong khoảng giữa Send() và SetSendTime()
                            lock (_rebuildLock)
                            {
                                Thread.Sleep(1); // delay 1ms tránh TCP printer ngập
                                bool sendOk = podController.Send(command);
                                if (sendOk)
                                {
                                    IncrementSentPrinter();
                                    counter++;
                                    _totalDataSent = _NumberOfSentPrinter;
                                    string qrPreview = command?.Length > 5 ? command.Substring(5, Math.Min(10, command.Length - 5)) : "(empty)";
                                    //ProjectLogger.WriteDebug($"[TX] #{_NumberOfSentPrinter} idx={codeIndex} qr={qrPreview}");
                                    //ProjectLogger.WriteDebug($"[SEND] sent={_NumberOfSentPrinter} counter={counter} codeIndex={codeIndex} buffer={_printerBuffer}");
                                    //PrinterLogger.Log(_SelectedJob?.FileName ?? "", Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "", "SEND", "SENT_INC",
                                    //    $"sent={_NumberOfSentPrinter} printed={NumberPrinted} codeIndex={codeIndex} cmd={command?.Substring(0, Math.Min(60, command?.Length ?? 0))}",
                                    //    "OK", "Đã gửi dữ liệu xuống máy in");
                                }
                                else
                                {
                                    string ipFail = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                    //PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipFail, "ERR", "SEND_FAIL",
                                    //    $"codeIndex={codeIndex} sent={_NumberOfSentPrinter} printed={NumberPrinted} fault=rlink",
                                    //    "SEND_ERR", "fault=rlink: Gửi dữ liệu thất bại, StreamWriter có thể null hoặc TCP đã đóng");
                                    codeIndex--;
                                    continue;
                                }
                                // Update send time + đánh dấu Sent cho VirtualList (Mode 1/2)
                                var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                                if (virtualList != null)
                                {
                                    virtualList.SetSendTime(codeIndex, DateTime.Now.ToString("dd MM yy HH:mm:ss"));
                                    // virtualList.SetSent(codeIndex, DateTime.Now.ToString("dd MM yy HH:mm:ss"));
                                }
                            }
                            tmpListLog = command.Split(Shared.Settings.SplitCharacter).Skip(1).ToList();
                            tmpListLog.Add(DateTime.Now.ToString("dd MM yy HH:mm:ss"));
                            _QueueBufferBackupSendLog.Enqueue(tmpListLog.ToArray());
                            tmpListLog.Clear();
                        }
                        if (Shared.OperStatus == OperationStatus.Processing) // Change operation status
                        {
                            if (counter >= bufferCount && _IsAfterProductionMode)  // Check allow system runing, not waiting util send data complete
                            {
                                Shared.OperStatus = OperationStatus.Running; // Update user interface the system is ready
                                Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Green);
                                Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
                                EnableUIComponent(Shared.OperStatus);
                            }
                            else if (counter >= 1 && _IsOnProductionMode)
                            {
                                Shared.OperStatus = OperationStatus.Running;
                                Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Green);
                                Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
                                EnableUIComponent(Shared.OperStatus);
                            }
                        }

                        if (_IsOnProductionMode)
                        {
                            lock (_PrintLocker)
                            {
                                _IsPrintedWait = true;
                                while (_IsPrintedWait) Monitor.Wait(_PrintLocker); // Waiting until code is print
                                if (_PrintedResult != ComparisonResult.Valid && _PrintedResult != ComparisonResult.Duplicated) // Check checked result to know if need to re sent code 
                                {
                                    codeIndex--;
                                }
                            }
                        }
                        //else if (_IsAfterProductionMode)
                        //{
                        //    // Giai đoạn 1: nạp đủ bufferCount mã đầu — gửi liên tục không throttle
                        //    if (counter < bufferCount)
                        //{
                        //    Thread.Sleep(Shared.Settings.PrinterList[0].TimeDelaySendFirstBuffer);
                        //}
                        //else
                        //{
                        //    // Giai đoạn 2: duy trì buffer >= bufferCount
                        //    // Chỉ dùng số từ máy in (RSFP), không dùng counter nội bộ R-Link
                        //    int pending;
                        //    bool rsfpFresh = _lastRsfpTimestamp != DateTime.MinValue
                        //        && (DateTime.Now - _lastRsfpTimestamp).TotalSeconds <= 5;

                        //    if (rsfpFresh)
                        //    {
                        //        // RSFP còn đến đều → dùng buffer chính xác từ máy in
                        //        pending = _printerBuffer;
                        //    }
                        //    else
                        //    {
                        //        // RSFP mất > 5 giây → chỉ bù 1 mã mỗi 5 giây
                        //        if (_rsfpMissingSentCount == 0)
                        //        {
                        //            _rsfpMissingSentCount++;
                        //            pending = 0;  // giả định buffer trống → gửi 1 mã
                        //        }
                        //        else
                        //        {
                        //            // Đã gửi bù rồi, chờ 5 giây rồi kiểm tra lại
                        //            Thread.Sleep(5000);
                        //            continue;
                        //        }
                        //    }

                        //    // Buffer thấp → gửi thêm ngay, không chờ
                        //    if (pending < bufferCount)
                        //        continue;

                        //    // Buffer đủ + RSFP fresh → chờ RSFP bình thường
                        //    if (rsfpFresh)
                        //    {
                        //        _printerFeedbackEvent.WaitOne(2000);
                        //        continue;
                        //    }

                        //    // Buffer đủ + RSFP mất → đã xử lý ở trên
                        //    continue;
                        //    // Đảm bảo queue cũng được dequeue (để đồng bộ với logic cũ)
                        //    _queueCountFeedback.TryDequeue(out int _);

                        //    // PERF SUMMARY: log mỗi 60 giây
                        //    if (_lastPerfLogTime == DateTime.MinValue || (DateTime.Now - _lastPerfLogTime).TotalSeconds >= 60)
                        //    {
                        //        _lastPerfLogTime = DateTime.Now;
                        //        int s = _NumberOfSentPrinter;
                        //        int p = NumberPrinted;
                        //        int pend = s - p;
                        //        string ipS = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                        //        PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipS, "PERF", "SUMMARY",
                        //            $"sent={s} printed={p} pending={pend}/{bufferCount} timeouts={_consecutiveTimeouts} printerStatus={_PrinterStatus} tcpOK={podController?.IsConnected() ?? false}",
                        //            "INFO", "Trạng thái tổng quát định kỳ");

                        //        // BUFFER_WARN theo ngưỡng: chỉ log khi vượt ngưỡng 80%, 90%, 100%
                        //        int pendingPct = bufferCount > 0 ? pend * 100 / bufferCount : 0;
                        //        int threshold = pendingPct >= 100 ? 100 : pendingPct >= 90 ? 90 : pendingPct >= 80 ? 80 : 0;
                        //        if (threshold > 0 && threshold > _lastBufferWarnPct)
                        //        {
                        //            _lastBufferWarnPct = threshold;
                        //            PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipS, "WARN", "BUFFER_HIGH",
                        //                $"pending={pend}/{bufferCount} ({pendingPct}%) threshold={threshold}% printerStatus={_PrinterStatus}",
                        //                threshold >= 100 ? "CRITICAL" : "WARN",
                        //                threshold >= 100
                        //                    ? "Bộ đệm đầy - máy in không theo kịp, nguy cơ treo firmware"
                        //                    : "Bộ đệm gần đầy - máy in đang chậm dần");
                        //        }
                        //        // Reset buffer warn khi pending giảm xuống dưới 80%
                        //        if (pendingPct < 80 && _lastBufferWarnPct > 0)
                        //            _lastBufferWarnPct = 0;
                        //    }
                        //}


                        else if (_IsAfterProductionMode)
                        {
                            // Giai đoạn 1: nạp bufferCount mã đầu
                            if (counter < bufferCount)
                            {
                                Thread.Sleep(Shared.Settings.PrinterList[0].TimeDelaySendFirstBuffer);
                            }
                            else
                            {
                                // Giai đoạn 2: chỉ gửi khi buffer còn chỗ (RSFP hoặc MON fallback)
                                int sessionPrinted;
                                if (!IsRsfpStale() && _sessionFirstRsfpPrinted >= 0)
                                    sessionPrinted = NumberPrinted - _sessionFirstRsfpPrinted;
                                else if (_sessionFirstMonPrinted >= 0)
                                    sessionPrinted = _receivedFinishPrintFormMON - _sessionFirstMonPrinted;
                                else
                                    sessionPrinted = 0;
                                int sessionPending = (_NumberOfSentPrinter - baselineSent) - sessionPrinted;

                                if (sessionPending >= bufferCount)
                                {
                                    // Buffer đầy → chờ printer in xong (RSFP/MON sẽ Set event)
                                    _printerFeedbackEvent.WaitOne(2000);
                                }
                                // else: sessionPending < bufferCount → cho phép iteration tiếp theo gửi
                            }

                            // Luôn dequeue queue (đồng bộ)
                            _queueCountFeedback.TryDequeue(out int _);

                            // PERF SUMMARY: log mỗi 60 giây
                            if (_lastPerfLogTime == DateTime.MinValue || (DateTime.Now - _lastPerfLogTime).TotalSeconds >= 60)
                            {
                                _lastPerfLogTime = DateTime.Now;
                                int s = _NumberOfSentPrinter - baselineSent;
                                int p;
                                if (!IsRsfpStale() && _sessionFirstRsfpPrinted >= 0)
                                    p = NumberPrinted - _sessionFirstRsfpPrinted;
                                else if (_sessionFirstMonPrinted >= 0)
                                    p = _receivedFinishPrintFormMON - _sessionFirstMonPrinted;
                                else
                                    p = 0;
                                int sessionPend = s - p;
                                string ipS = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                                PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipS, "PERF", "SUMMARY",
                                    $"sessionSent={s} sessionPrinted={p} sessionPending={sessionPend}/{bufferCount} totalSent={_NumberOfSentPrinter} totalPrinted={NumberPrinted} firstRsfpPrinted={_sessionFirstRsfpPrinted} rsfpStale={IsRsfpStale()} monPrinted={_receivedFinishPrintFormMON} firstMon={_sessionFirstMonPrinted} timeouts={_consecutiveTimeouts} printerStatus={_PrinterStatus} tcpOK={podController?.IsConnected() ?? false}",
                                    "INFO", "Trạng thái tổng quát định kỳ");

                                int pendingPct = bufferCount > 0 ? sessionPend * 100 / bufferCount : 0;
                                int threshold = pendingPct >= 100 ? 100 : pendingPct >= 90 ? 90 : pendingPct >= 80 ? 80 : 0;
                                if (threshold > 0 && threshold > _lastBufferWarnPct)
                                {
                                    _lastBufferWarnPct = threshold;
                                    PrinterLogger.Log(_SelectedJob?.FileName ?? "", ipS, "WARN", "BUFFER_HIGH",
                                        $"sessionPending={sessionPend}/{bufferCount} ({pendingPct}%) threshold={threshold}% printerStatus={_PrinterStatus}",
                                        threshold >= 100 ? "CRITICAL" : "WARN",
                                        threshold >= 100
                                            ? "Bộ đệm đầy - máy in không theo kịp, nguy cơ treo firmware"
                                            : "Bộ đệm gần đầy - máy in đang chậm dần");
                                }
                                if (pendingPct < 80 && _lastBufferWarnPct > 0)
                                    _lastBufferWarnPct = 0;
                            }
                        }



                        //else if (_IsAfterProductionMode)
                        //{
                        //    if (counter < bufferCount)
                        //    {
                        //        Thread.Sleep(Shared.Settings.PrinterList[0].TimeDelaySendFirstBuffer);
                        //    }
                        //    else
                        //    {
                        //        while (true)
                        //        {
                        //            if (_queueCountFeedback.TryDequeue(out int res))
                        //            {
                        //                break;
                        //            }
                        //            if (!Shared.Settings.PrinterList[0].EnableSendTurboSpeed) // Use mode wait data mode
                        //            {
                        //                spinWait.SpinOnce();
                        //            }
                        //        }
                        //    }
                        //}

                    }
                }
                if (Shared.OperStatus == OperationStatus.Processing)
                {
                    Shared.OperStatus = OperationStatus.Running;
                    Shared.SendPrinterSignalToPlc(Shared.PlcPrinterSignal.Green);
                    EnableUIComponent(Shared.OperStatus);
                }
                string jobStop = _SelectedJob?.FileName ?? "";
                string ipStop = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                PrinterLogger.Log(jobStop, ipStop, "STATE", "STOP",
                    $"sent={_NumberOfSentPrinter} printed={NumberPrinted} pending={_NumberOfSentPrinter - NumberPrinted} lýDo=hoànTất",
                    "OK", "Kết thúc gửi dữ liệu - hoàn tất");
            }
            catch (OperationCanceledException)
            {
                string jobCancel = _SelectedJob?.FileName ?? "";
                string ipCancel = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                PrinterLogger.Log(jobCancel, ipCancel, "STATE", "STOP",
                    $"sent={_NumberOfSentPrinter} printed={NumberPrinted} pending={_NumberOfSentPrinter - NumberPrinted} lýDo=hủy",
                    "CANCEL", "Kết thúc gửi dữ liệu - bị hủy bởi người dùng");

                _BackupSendLogCancelTokenSource?.Cancel();
                _BackupSendLogCancelTokenSource?.Dispose();
                _BackupSendLogCancelTokenSource = null;
                _QueueBufferBackupSendLog.Enqueue(null);
                _QueueBufferBackupSendLog.Clear();

                _BackupRSFPLogCancelTokenSource?.Cancel();
                _QueueBufferBackupRSFPLog.Enqueue(null);
                _QueueBufferBackupRSFPLog.Clear();
            }
            catch (Exception ex)
            {
                string jobEx = _SelectedJob?.FileName ?? "";
                string ipEx = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                PrinterLogger.Log(jobEx, ipEx, "STATE", "STOP",
                    $"sent={_NumberOfSentPrinter} printed={NumberPrinted} pending={_NumberOfSentPrinter - NumberPrinted} lýDo=lỗi ex={ex.Message}",
                    "ERR", "fault=rlink: Lỗi không mong đợi trong luồng gửi dữ liệu, đang dừng hệ thống");

                StopProcessAsync(false, Lang.HandleError, false, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }
        private void ReleaseLocker()
        {
            lock (_PrintLocker)
            {
                _IsPrintedWait = false;
                Monitor.Pulse(_PrintLocker);
            }
            lock (_ReceiveLocker)
            {
                Monitor.Pulse(_ReceiveLocker);
            }
            lock (_CheckLocker)
            {
                _IsCheckedWait = true;
                Monitor.Pulse(_CheckLocker);
            }
            lock (_ReceiveLocker)
            {
                Monitor.Pulse(_ReceiveLocker);
            }
            lock (_PrintLocker)
                _IsPrintedWait = false;
            lock (_CheckLocker)
                _IsCheckedWait = true;
        }

        private void KillTThreadSendPODDataToPrinter()
        {
            ReleaseLocker();
            _SendDataToPrinterTokenCTS?.Cancel();
        }

        public async void ReprintAsync()
        {
            if (Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)
            {
                await Task.Run(() => { Reprint(); });
            }
        }

        public void GetSampleWithScanner()
        {
            //Shared.SerialDevController.IsSerialDevConnected() &&
            try
            {
                if (Shared.SerialDevController != null && Shared.SerialDevController.IsSerialDevConnected())
                {
                    if (Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)
                    {
                        var dialogResult = CustomMessageBox.Show(Lang.GetSampleConfirm, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dialogResult == DialogResult.Yes)
                        {
                            // thinh dang lam
                            ChangeCheckMode(Checkmode.getSampleWithScanner);
                            Shared.IsSampled = true;
                            _IsReCheck = true;
                            StartProcess();
                        }
                    }
                    else
                    {
                        CustomMessageBox.Show(Lang.SystemIsRunningPleaseStop, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    CustomMessageBox.Show(Lang.PleaseCheckSerialDeviceConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Lang.PleaseCheckSerialDeviceConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        /// <summary>
        /// Recheck with barcode scanner
        /// </summary>
        public void ReCheck()
        {
            if (Shared.SerialDevController.IsSerialDevConnected())
            {
                if (Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)
                {
                    if (_FormCheckedResult != null)
                    {
                        Invoke(new Action(() => { _FormCheckedResult.ForceClose(); }));
                        _FormCheckedResult = null;
                    }

                    var dialogResult = CustomMessageBox.Show(Lang.ReCheckConfirm, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Yes)
                    {
                        ChangeCheckMode(Checkmode.recheckWithScanner);
                        StartProcess();
                    }

                }
                else
                {
                    CustomMessageBox.Show(Lang.SystemIsRunningPleaseStop, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                CustomMessageBox.Show(Lang.PleaseCheckSerialDeviceConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Reprint()
        {
            try
            {
                if (!_IsAfterProductionMode)
                    return;
                lock (_SyncObjCodeList)
                {
                    if (_PrintedCodeObtainFromFile.Count() > 0 && _CodeListPODFormat.Count() > 0)
                    {
                        _TotalMissed = 0;
                        //int codeDataLenght = _PrintedCodeObtainFromFile[0].Length - 1;
                        int codeDataLenght = 1;

                        foreach (var item in _CodeListPODFormat)
                        {
                            if (!item.Value.Status)
                            {
                                if (_PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] == "Printed")
                                {
                                    _PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] = "Reprint";
                                }
                            }
                            else
                            {
                                if (_PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] != "Printed")
                                {
                                    _PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] = "Printed";
                                }
                            }
                        }
                        //_TotalMissed = _TotalCode - NumberOfCheckPassed;
                        _TotalMissed = _TotalCode - NumberOfCheckPassed;
                    }
                }

                if (_FormCheckedResult != null)
                {
                    Invoke(new Action(() => { _FormCheckedResult.ForceClose(); }));
                    _FormCheckedResult = null;
                }

                if (NumberOfCheckPassed < _TotalCode)
                {
                    DialogResult dialogResult = CustomMessageBox.Show(Lang.ReprintConfirm, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                        StartProcess();
                    else
                        return;
                }
            }
            catch
            {
                CustomMessageBox.Show(Lang.ReprintError, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Question);
                return;
            }
        }

        public async void ExportSelectedType(int typeNumber)
        {
            if (Shared.OperStatus == OperationStatus.Running)
            {
                CuzAlert.Show("Vui lòng dừng job trước khi xuất dữ liệu.", Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                return;
            }
            string checkInitDataMessage = "";
            checkInitDataMessage = CheckInitDataErrorAndGenerateMessage();
            if (checkInitDataMessage != "")
            {
                DialogResult dialogResult = CustomMessageBox.Show(checkInitDataMessage, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var sfd = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|PDF files (*.pdf)|*.pdf",
                FileName = _SelectedJob.FileName
            };
            DialogResult dialogRes = sfd.ShowDialog();
            if (dialogRes.Equals(DialogResult.Cancel) || sfd.FileName == "")
            {
                return;
            }
            else
            {
                EnableUIComponentWhenLoadData(false);
                try
                {
                    await Task.Run(() =>
                    {
                        switch (typeNumber)
                        {
                            case 0:
                                ExportAllData(sfd.FileName);
                                break;
                            case 1:
                                ExportAllPassedData(sfd.FileName);
                                break;
                            case 2:
                                ExportFailedData(sfd.FileName);
                                break;
                            default:
                                break;
                        }
                    });
                }
                catch (Exception ex)
                {
                    CuzAlert.Show(Lang.DetectError + ": " + ex.Message, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                    Shared.RaiseOnLogError(ex);
                }
                finally
                {
                    EnableUIComponentWhenLoadData(true);
                }
            }

        }

        private void ExportAllPassedData(string fileName)
        {
            try
            {
                var validData = _CheckedResultCodeList
                    .Where(arr => arr[Index_Result] == "Valid");

                if (File.Exists(fileName))
                    File.Delete(fileName);

                List<string> lines = new List<string>();
                string header = string.Join(",", _ColumnNames.Select(Csv.Escape));
                lines.Add(header);

                foreach (var record in validData)
                {
                    string line = string.Join(",", record.Select(Csv.Escape));
                    lines.Add(line);
                }

                if (fileName.EndsWith(".pdf"))
                {
                    ConvertCsvToPdf(lines.ToArray(), fileName);
                }
                else
                {
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

                MoveToTheFile(fileName);
            }
            catch (Exception ex)
            {
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        private void ExportPassedDataByCamAndScanner(string fileName)
        {
            //var checkedResultDict = _CheckedResultCodeList
            //      .Where(arr => arr[Index_Result] == "Valid" && arr[Index_Sampled] != "True")
            //      .GroupBy(x => x[Index_ResultData])
            //      .ToDictionary(
            //                      g => g.Key,
            //                      g => string.Join(", ", g.First().Skip(3).Prepend(g.First().ElementAtOrDefault(Index_ResultData)).Where(value => !string.IsNullOrEmpty(value)))

            //                   );
            //ExportPassedData(fileName, checkedResultDict);
        }

        private void ExportPassedDataByScanner(string fileName)
        {
            //var checkedResultDict = _CheckedResultCodeList
            //     .Where(arr => arr[Index_Result] == "Valid" && arr[Index_Device] != "Camera" && arr[Index_Sampled] != "True")
            //     .GroupBy(x => x[Index_ResultData])
            //     .ToDictionary(
            //                      g => g.Key,
            //                      g => string.Join(", ", g.First().Skip(3).Prepend(g.First().ElementAtOrDefault(Index_ResultData)).Where(value => !string.IsNullOrEmpty(value)))

            //                  );
            //ExportPassedData(fileName, checkedResultDict);
        }

        private void ExportSampledData(string fileName)
        {
            //var checkedResultDict = _CheckedResultCodeList
            //     .Where(arr => arr[Index_Sampled] == "True")
            //     .GroupBy(x => x[Index_ResultData])
            //     .ToDictionary(
            //                      g => g.Key,
            //                      g => string.Join(", ", g.First().Skip(3).Prepend(g.First().ElementAtOrDefault(Index_ResultData)).Where(value => !string.IsNullOrEmpty(value)))

            //                  );
            //ExportPassedData(fileName, checkedResultDict);
        }

        private void ExportFailedData(string fileName)
        {

            try
            {
                // Filter records where the third column is not "Valid"
                var invalidData = _CheckedResultCodeList
                    .Where(arr => arr[Index_Result] != "Valid");

                if (File.Exists(fileName))
                    File.Delete(fileName);

                List<string> lines = new List<string>();

                string header = string.Join(",", _ColumnNames.Select(Csv.Escape));
                lines.Add(header);

                // Write invalid records
                foreach (var record in invalidData)
                {
                    string line = string.Join(",", record.Select(Csv.Escape));
                    lines.Add(line);
                }

                if (fileName.EndsWith(".pdf"))
                {
                    ConvertCsvToPdf(lines.ToArray(), fileName);
                }
                else
                {
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line); // Write the sorted comma-separated string
                        }
                    }
                }

                MoveToTheFile(fileName);
            }
            catch (Exception ex)
            {
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }

        }

        private void ExportAllData(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);

                List<string> lines = new List<string>();
                string header = string.Join(",", _ColumnNames.Select(Csv.Escape));
                lines.Add(header);

                foreach (var record in _CheckedResultCodeList)
                {
                    string line = string.Join(",", record.Select(Csv.Escape));
                    lines.Add(line);
                }

                if (fileName.EndsWith(".pdf"))
                {
                    ConvertCsvToPdf(lines.ToArray(), fileName);
                }
                else
                {
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

                MoveToTheFile(fileName);
            }
            catch (Exception ex)
            {
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        private void ExportUnverifiedData(string fileName)
        {
            try
            {
                var duplicateCountDict = _CheckedResultCodeList
                    .Where(arr => arr[Index_Result] == "Duplicated")
                    .GroupBy(x => x[Index_ResultData])
                    .ToDictionary(g => g.Key, g => g.Count());

                //Dictionary with Result data is key and datetime is value  
                var checkedResultDict = _CheckedResultCodeList
                    .Where(arr => arr[Index_Result] == "Valid")
                    .GroupBy(x => x[Index_ResultData])
                    .ToDictionary(g => g.Key, g => g.First()[Index_DateTime]);

                if (File.Exists(fileName))
                    File.Delete(fileName);

                List<string> lines = new List<string>();

                // Write the header, edit column element
                var statusCol = _DatabaseColunms[1];
                var newColumn = new string[_DatabaseColunms.Length];
                int currentIndex = 0;
                for (int i = 0; i < _DatabaseColunms.Length; i++)
                {
                    if (i != 1)
                    {
                        newColumn[currentIndex++] = _DatabaseColunms[i];
                    }
                }
                newColumn[newColumn.Length - 1] = statusCol;
                string header = string.Join(",", newColumn.Select(Csv.Escape));
                lines.Add(header);

                for (int i = 0; i < _TotalCode; i++)
                {
                    var record = _PrintedCodeObtainFromFile[i];

                    if (record.Length > 1)
                    {
                        var newRecord = new string[record.Length];
                        Array.Copy(record, 0, newRecord, 0, 1);
                        Array.Copy(record, 2, newRecord, 1, record.Length - 2);
                        newRecord[newRecord.Length - 1] = record[1];
                        record = newRecord;
                    }

                    var compareString = GetCompareDataByPODFormat(record, _SelectedJob.PODFormat);
                    var writeValue = string.Join(",", record.Take(record.Length - 1).Select(Csv.Escape)) + ",";
                    var status = record[record.Length - 1];
                    bool isChecked = checkedResultDict.TryGetValue(compareString, out string dateVerify);

                    if (!isChecked && status.Equals("Printed"))
                    {
                        writeValue += PrintedUnverified;
                        lines.Add(writeValue);
                    }
                }

                if (fileName.EndsWith(".pdf"))
                {
                    ConvertCsvToPdf(lines.ToArray(), fileName);
                }
                else
                {
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line); // Write the sorted comma-separated string
                        }
                    }
                }

                MoveToTheFile(fileName);
                checkedResultDict.Clear();
            }
            catch (Exception ex)
            {
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        private void ExportWaitingData(string fileName)
        {

            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);

                List<string> lines = new List<string>();

                var statusCol = _DatabaseColunms[1];
                var newColumn = new string[_DatabaseColunms.Length];
                int currentIndex = 0;
                for (int i = 0; i < _DatabaseColunms.Length; i++)
                {
                    if (i != 1)
                    {
                        newColumn[currentIndex++] = _DatabaseColunms[i];
                    }
                }
                newColumn[newColumn.Length - 1] = statusCol;
                string header = string.Join(",", newColumn.Select(Csv.Escape));
                lines.Add(header);

                for (int i = 0; i < _TotalCode; i++)
                {
                    var record = _PrintedCodeObtainFromFile[i];

                    if (record.Length > 1)
                    {
                        var newRecord = new string[record.Length];
                        Array.Copy(record, 0, newRecord, 0, 1);
                        Array.Copy(record, 2, newRecord, 1, record.Length - 2);
                        newRecord[newRecord.Length - 1] = record[1];
                        record = newRecord;
                    }

                    var compareString = GetCompareDataByPODFormat(record, _SelectedJob.PODFormat);
                    var writeValue = string.Join(",", record.Take(record.Length - 1).Select(Csv.Escape)) + ",";
                    var status = record[record.Length - 1];


                    if (status.Equals("Printed"))
                    {
                        writeValue += PrintedUnverified;
                    }
                    else
                    {
                        writeValue += UnprintedUnverified;
                        lines.Add(writeValue);
                    }

                }

                if (fileName.EndsWith(".pdf"))
                {
                    ConvertCsvToPdf(lines.ToArray(), fileName);
                }
                else
                {
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line); // Write the sorted comma-separated string
                        }
                    }
                }

                MoveToTheFile(fileName);
            }
            catch (Exception ex)
            {
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        private void ExportWaitingData_2(string fileName)
        {
            //try
            //{
            //    var checkedResultDict = _CheckedResultCodeList
            //      .Where(arr => arr[Index_Result] == "Valid" && arr[Index_Sampled] != "True")
            //      .GroupBy(x => x[Index_ResultData])
            //      .ToDictionary(
            //                       g => g.Key,
            //                       g => string.Join(", ", g.First().Skip(3).Prepend(g.First().ElementAtOrDefault(Index_ResultData)).Where(value => !string.IsNullOrEmpty(value)))
            //    );
            //    var duplicateCountDict = _CheckedResultCodeList
            //     .Where(arr => arr[Index_Result] == "Duplicated")
            //     .GroupBy(x => x[Index_ResultData])
            //     .ToDictionary(g => g.Key, g => g.Count());

            //    if (File.Exists(fileName))
            //        File.Delete(fileName);

            //    List<string> lines = new List<string>();
            //    var statusCol = _DatabaseColunms[1];
            //    var newColumn = new string[_DatabaseColunms.Length];
            //    int currentIndex = 0;
            //    for (int i = 0; i < _DatabaseColunms.Length; i++)
            //    {
            //        if (i != 1)
            //        {
            //            newColumn[currentIndex++] = _DatabaseColunms[i];
            //        }
            //    }
            //    newColumn[newColumn.Length - 1] = statusCol;
            //    string header = string.Join(",", newColumn.Select(Csv.Escape));
            //    lines.Add(header);

            //    for (int i = 0; i < _TotalCode; i++)
            //    {
            //        bool isAllowedWriteLine = false;
            //        var record = _PrintedCodeObtainFromFile[i];

            //        if (record.Length > 1)
            //        {
            //            var newRecord = new string[record.Length];
            //            Array.Copy(record, 0, newRecord, 0, 1);
            //            Array.Copy(record, 2, newRecord, 1, record.Length - 2);
            //            newRecord[newRecord.Length - 1] = record[1];
            //            record = newRecord;
            //        }

            //        var compareString = GetCompareDataByPODFormat(record, _SelectedJob.PODFormat);
            //        var writeValue = string.Join(",", record.Take(record.Length - 1).Select(Csv.Escape)) + ",";
            //        var status = record[record.Length - 1];
            //        bool isChecked = checkedResultDict.TryGetValue(compareString, out string dateVerify);

            //        if (isChecked)
            //        {


            //        }
            //        else
            //        {
            //            if (status.Equals("Printed")) // Is Printed 
            //            {
            //                if (duplicateCountDict.TryGetValue(compareString, out int duplicateCount) && duplicateCount >= 1)
            //                {
            //                    // writeValue += "Printed-Duplicate";
            //                    writeValue += PrintedVerified;
            //                }
            //                else
            //                {
            //                    writeValue += PrintedUnverified;
            //                }
            //            }
            //            else
            //            {
            //                writeValue += UnprintedUnverified;
            //            }

            //            //writeValue += "," + Csv.Escape(dateVerify).Trim('"');
            //            //checkedResultDict.Remove(compareString);

            //            isAllowedWriteLine = true;
            //        }
            //        if (isAllowedWriteLine)
            //        {
            //            lines.Add(writeValue);// Write the value if the record is checked
            //        }

            //    }
            //    if (fileName.EndsWith(".pdf"))
            //    {
            //        ConvertCsvToPdf(lines.ToArray(), fileName);
            //    }
            //    else
            //    {
            //        using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
            //        {
            //            foreach (var line in lines)
            //            {
            //                writer.WriteLine(line); // Write the sorted comma-separated string
            //            }
            //        }
            //    }

            //    MoveToTheFile(fileName);
            //    checkedResultDict.Clear();
            //}
            //catch (Exception)
            //{

            //}

        }

        private void ExportPassedData(string fileName, Dictionary<string, string> checkedResultDict)
        {
            try
            {
                var duplicateCountDict = _CheckedResultCodeList
                    .Where(arr => arr[Index_Result] == "Duplicated")
                    .GroupBy(x => x[Index_ResultData])
                    .ToDictionary(g => g.Key, g => g.Count());

                if (File.Exists(fileName))
                    File.Delete(fileName);
                List<string> lines = new List<string>();

                var statusCol = _DatabaseColunms[1];
                var newColumn = new string[_DatabaseColunms.Length];
                int currentIndex = 0;
                for (int i = 0; i < _DatabaseColunms.Length; i++)
                {
                    if (i != 1)
                    {
                        newColumn[currentIndex++] = _DatabaseColunms[i];
                    }
                }
                newColumn[newColumn.Length - 1] = statusCol;
                string header = string.Join(",", newColumn.Select(Csv.Escape)) + ",ScanData" + ",Barcode Quality" + ",Position" + ",Processing Time" + ",VerifyDate" + ",Device" + ",Sampled";
                lines.Add(header);

                for (int i = 0; i < _TotalCode; i++)
                {
                    bool isAllowedWriteLine = true;
                    var record = _PrintedCodeObtainFromFile[i];

                    if (record.Length > 1)
                    {
                        var newRecord = new string[record.Length];
                        Array.Copy(record, 0, newRecord, 0, 1);
                        Array.Copy(record, 2, newRecord, 1, record.Length - 2);
                        newRecord[newRecord.Length - 1] = record[1];
                        record = newRecord;
                    }

                    var compareString = GetCompareDataByPODFormat(record, _SelectedJob.PODFormat);
                    var writeValue = string.Join(",", record.Take(record.Length - 1).Select(Csv.Escape)) + ",";
                    var status = record[record.Length - 1];
                    bool isChecked = checkedResultDict.TryGetValue(compareString, out string dateVerify);

                    if (isChecked)
                    {
                        if (status.Equals("Printed")) // Is Printed 
                        {
                            if (duplicateCountDict.TryGetValue(compareString, out int duplicateCount) && duplicateCount >= 1)
                            {
                                // writeValue += "Printed-Duplicate";
                                writeValue += PrintedVerified;
                            }
                            else
                            {
                                writeValue += PrintedVerified;
                            }
                        }
                        else
                        {
                            writeValue += UnprintedVerified;
                        }

                        writeValue += "," + Csv.Escape(dateVerify).Trim('"');
                        checkedResultDict.Remove(compareString);

                    }
                    else
                    {
                        isAllowedWriteLine = false;
                    }
                    if (isAllowedWriteLine)
                    {
                        lines.Add(writeValue);
                    }
                }

                if (fileName.EndsWith(".pdf"))
                {
                    ConvertCsvToPdf(lines.ToArray(), fileName);
                }
                else
                {
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line); // Write the sorted comma-separated string
                        }
                    }
                }


                MoveToTheFile(fileName);
                checkedResultDict.Clear();
            }
            catch (Exception ex)
            {
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        public async void ExportDataAsync()
        {
            if (Shared.OperStatus != OperationStatus.Stopped)
            {
                return;
            }
            string checkInitDataMessage = "";
            checkInitDataMessage = CheckInitDataErrorAndGenerateMessage();
            if (checkInitDataMessage != "")
            {
                DialogResult dialogResult = CustomMessageBox.Show(checkInitDataMessage, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var sfd = new SaveFileDialog
            {
                //Filter = "CSV|*.csv",
                Filter = "CSV files (*.csv)|*.csv|PDF files (*.pdf)|*.pdf",
                FileName = _SelectedJob.FileName
            };
            DialogResult dialogRes = sfd.ShowDialog();
            if (dialogRes.Equals(DialogResult.Cancel) || sfd.FileName == "")
            {
                return;
            }
            else
            {
                EnableUIComponentWhenLoadData(false);
                await Task.Run(() => { ExportData(sfd.FileName); });
                EnableUIComponentWhenLoadData(true);
            }
        }

        public async void ExportAllDataAsync()
        {
            if (Shared.OperStatus != OperationStatus.Stopped)
            {
                return;
            }
            string checkInitDataMessage = "";
            checkInitDataMessage = CheckInitDataErrorAndGenerateMessage();
            if (checkInitDataMessage != "")
            {
                DialogResult dialogResult = CustomMessageBox.Show(checkInitDataMessage, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var sfd = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|PDF files (*.pdf)|*.pdf",
                FileName = _SelectedJob.FileName
            };
            DialogResult dialogRes = sfd.ShowDialog();
            if (dialogRes.Equals(DialogResult.Cancel) || sfd.FileName == "")
            {
                return;
            }
            else
            {
                EnableUIComponentWhenLoadData(false);
                await Task.Run(() => { ExportAllData(sfd.FileName); });
                EnableUIComponentWhenLoadData(true);
            }
        }



        public void ExportData(string fileName)
        {
            try
            {
                var duplicateCountDict = _CheckedResultCodeList
                    .Where(arr => arr[Index_Result] == "Duplicated") // arr[Index_Device] != "Barcode Scanner" &&
                    .GroupBy(x => x[Index_ResultData])
                    .ToDictionary(g => g.Key, g => g.Count());

                var checkedResultDict = _CheckedResultCodeList
                    .Where(arr => arr[Index_Result] == "Valid") //  arr[Index_Device] != "Barcode Scanner" &&
                    .GroupBy(x => x[Index_ResultData])
                    .ToDictionary(g => g.Key, g => g.First()[Index_DateTime]);

                if (File.Exists(fileName))
                    File.Delete(fileName);
                List<string> lines = new List<string>();

                var statusCol = _DatabaseColunms[1];
                var newColumn = new string[_DatabaseColunms.Length];
                int currentIndex = 0;
                for (int i = 0; i < _DatabaseColunms.Length; i++)
                {
                    if (i != 1)
                    {
                        newColumn[currentIndex++] = _DatabaseColunms[i];
                    }
                }
                newColumn[newColumn.Length - 1] = statusCol;
                string header = string.Join(",", newColumn.Select(Csv.Escape)) + ",VerifyDate";
                lines.Add(header);
                for (int i = 0; i < _TotalCode; i++)
                {
                    var record = _PrintedCodeObtainFromFile[i];
                    if (record.Length > 1)
                    {
                        var newRecord = new string[record.Length];
                        Array.Copy(record, 0, newRecord, 0, 1);
                        Array.Copy(record, 2, newRecord, 1, record.Length - 2);
                        newRecord[newRecord.Length - 1] = record[1];
                        record = newRecord;
                    }

                    var compareString = GetCompareDataByPODFormat(record, _SelectedJob.PODFormat);
                    var writeValue = string.Join(",", record.Take(record.Length - 1).Select(Csv.Escape)) + ",";
                    var status = record[record.Length - 1];
                    bool isChecked = checkedResultDict.TryGetValue(compareString, out string dateVerify);

                    if (isChecked)
                    {
                        if (status.Equals("Printed"))
                        {
                            if (duplicateCountDict.TryGetValue(compareString, out int duplicateCount) && duplicateCount >= 1)
                            {
                                writeValue += PrintedDuplicate;
                            }
                            else
                            {
                                writeValue += PrintedVerified;
                            }
                        }
                        else
                        {
                            writeValue += UnprintedVerified;
                        }

                        writeValue += "," + Csv.Escape(dateVerify);
                        checkedResultDict.Remove(compareString);
                    }
                    else
                    {
                        if (status.Equals("Printed"))
                        {
                            writeValue += PrintedUnverified;
                        }
                        else
                        {
                            writeValue += UnprintedUnverified;
                        }

                    }
                    lines.Add(writeValue);
                }

                if (fileName.EndsWith(".pdf"))
                {
                    ConvertCsvToPdf(lines.ToArray(), fileName);
                }
                else
                {
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line); // Write the sorted comma-separated string
                        }
                    }
                }

                //SetFileReadOnly(fileName);
                MoveToTheFile(fileName);
                checkedResultDict.Clear();
            }
            catch (Exception ex)
            {
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        public static void ConvertCsvToPdf(string[] lines, string pdfFilePath)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                Font font = new Font("Arial", 10);
                Brush brush = Brushes.Black;
                int width = GetDynamicWidth(lines, font);
                int maxHeight = width; // Based on original logic
                int lineHeight = 15;
                int linesPerImage = maxHeight / lineHeight;

                PdfDocument pdf = new PdfDocument();
                List<string> batchLines = new List<string>();

                foreach (var line in lines)
                {
                    batchLines.Add(line);

                    if (batchLines.Count == linesPerImage)
                    {
                        int requiredHeight = (int)(batchLines.Count * lineHeight * 1.01);
                        AddBatchToPdf(batchLines, pdf, width, requiredHeight, lineHeight, font, brush);
                        batchLines.Clear();
                    }
                }

                if (batchLines.Count > 0)
                {
                    int requiredHeight = (int)(batchLines.Count * lineHeight * 1.01);
                    AddBatchToPdf(batchLines, pdf, width, requiredHeight, lineHeight, font, brush);
                }

                pdf.Save(pdfFilePath);
                stopwatch.Stop();
                //MessageBox.Show("Time taken to calculate width: " + stopwatch.ElapsedMilliseconds + " ms");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Export Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        static void AddBatchToPdf(List<string> lines, PdfDocument pdf, int width, int height, int lineHeight, Font font, Brush brush)
        {
            int linesPerPage = (int)((height) / lineHeight); // leaving small top/bottom margin
            int totalPages = (int)Math.Ceiling((double)lines.Count / linesPerPage);

            XFont xFont = new XFont(font.Name, font.Size);
            XBrush xBrush = XBrushes.Black;

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                PdfPage page = pdf.AddPage();
                page.Width = width;
                page.Height = height;

                XGraphics gfx = XGraphics.FromPdfPage(page);
                double y = 4; // top margin

                int startLine = pageIndex * linesPerPage;
                int endLine = Math.Min(startLine + linesPerPage, lines.Count);

                for (int i = startLine; i < endLine; i++)
                {
                    gfx.DrawString(lines[i], xFont, xBrush, new XPoint(4, y), XStringFormats.TopLeft);
                    y += lineHeight;
                }
            }
        }

        private static int GetDynamicWidth(string[] lines, Font font)
        {
            int maxWidth = 0;
            using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                foreach (string line in lines)
                {
                    int lineWidth = (int)graphics.MeasureString(line, font).Width;
                    if (lineWidth > maxWidth)
                    {
                        maxWidth = lineWidth;
                    }
                }
            }
            return maxWidth + 20; // Add some padding
        }

        static void SetFileReadOnly(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
            }
            else
            {
                Console.WriteLine("File does not exist.");
            }
        }

        private void MoveToTheFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                // Get the directory path of the file
                string directoryPath = Path.GetDirectoryName(fileName);

                // Use Process.Start to open the folder and select the file
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer",
                    Arguments = $"/select,\"{fileName}\"",
                    UseShellExecute = true
                });
            }
            else
            {
                Console.WriteLine("File does not exist.");
            }
        }

        private void Shared_OnPrinterStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelPrinter();
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        private void btnValid_Click(object sender, EventArgs e)
        {

        }

        private void btnInvalid_Click(object sender, EventArgs e)
        {

        }

        private void btnDuplicate_Click(object sender, EventArgs e)
        {

        }

        private void btnNull_Click(object sender, EventArgs e)
        {

        }

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
                            if (Shared.SensorController == null || counter >= 2)
                            {
                                Shared.SensorController = null;
                                Shared.SensorController = new PODController(
                                    Shared.Settings.SensorControllerIP,
                                    Shared.Settings.SensorControllerPort,
                                    0,
                                    500, 500);
                                Shared.SensorController.Connect();
                                counter = 0;
                            }
                            else
                            {
                                bool checkIP = Shared.SensorController.ServerIP == Shared.Settings.SensorControllerIP;
                                if (checkIP)
                                {
                                    bool checkPort = Shared.SensorController.Port == Shared.Settings.SensorControllerPort;
                                    if (!checkPort)
                                    {
                                        Shared.SensorController.Disconnect();
                                        Shared.SensorController = null;
                                    }
                                }
                                else
                                {
                                    Shared.SensorController.Disconnect();
                                    Shared.SensorController = null;
                                }
                            }

                            if (Shared.SensorController.IsConnected() == false)
                            {
                                Shared.SensorController.Disconnect();
                                Shared.SensorController.Connect();
                                counter++;
                            }
                            else
                            {
                                counter = 0;
                            }

                            if (Shared.IsSensorControllerConnected != Shared.SensorController.IsConnected())
                            {
                                Shared.IsSensorControllerConnected = Shared.SensorController.IsConnected();
                                UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
                                Shared.RaiseSensorControllerChangeEvent();
                                if (Shared.IsSensorControllerConnected)
                                {
                                    Shared.SendSettingToSensorController();
                                }
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

        private void Shared_OnSensorControllerChangeEvent(object sender, EventArgs e)
        {
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
        }

        private void Shared_OnSerialDeviceControllerChangeEvent(object sender, EventArgs e)
        {
            UpdateUISerialDeviceControllerStatus(Shared.IsSerialDeviceConnected);
        }

        private void PositionForm(RsalAlertForm form, bool isWarning)
        {
            var screen = Screen.PrimaryScreen.WorkingArea;
            int w = 480, h = 400, gap = 10;
            bool errorOpen = _rsalErrorForm != null && !_rsalErrorForm.IsDisposed;
            bool warnOpen = _rsalWarningForm != null && !_rsalWarningForm.IsDisposed;
            bool otherOpen = isWarning ? errorOpen : warnOpen;

            if (otherOpen)
            {
                int totalW = w * 2 + gap;
                int left = (screen.Width - totalW) / 2;
                int top = (screen.Height - h) / 2;

                if (errorOpen)
                    _rsalErrorForm.Location = new System.Drawing.Point(left, top);
                if (warnOpen)
                    _rsalWarningForm.Location = new System.Drawing.Point(left + w + gap, top);

                form.StartPosition = FormStartPosition.Manual;
                form.Location = isWarning
                    ? new System.Drawing.Point(left + w + gap, top)
                    : new System.Drawing.Point(left, top);
            }
            else
            {
                form.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void LogDisconnect(string source)
        {
            try
            {
                string dir = Path.Combine(
                    CommVariables.PathProgramDataApp, "Logs", "Disconnect");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, $"disconnect_{DateTime.Now:yyyyMMdd}.log");

                string ip = Shared.Settings?.PrinterList?.Count > 0 ? Shared.Settings.PrinterList[0].IP : "";
                bool tcpOk = Shared.Settings?.PrinterList?.FirstOrDefault()?.PODController?.IsConnected() ?? false;
                double rsfpGap = _lastRsfpTimestamp != DateTime.MinValue
                    ? (DateTime.Now - _lastRsfpTimestamp).TotalSeconds : -1;

                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{source}]" +
                    $" tcpConnected={tcpOk} printerStatus={_PrinterStatus}" +
                    $" rsfpGap={rsfpGap:F1}s timeouts={_consecutiveTimeouts}" +
                    $" totalRsfp={_totalRsfpReceived} ip={ip}";

                File.AppendAllText(path, line + Environment.NewLine);
            }
            catch { }
        }

        private void lblTotalTon_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        #endregion Events Called

        #region Procedure for UI
        private void SetLanguage()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetLanguage()));
                return;
            }

            //btnJob.Text = "Trang chủ";//Lang.Operation;
            btnDatabase.Text = Lang.DatabaseFrmMain;
            btnAccount.Text = Lang.Account;
            btnHistory.Text = Lang.ProgramHistoryFrmMain;
            btnSettings.Text = Lang.Settings;
            btnExit.Text = Lang.Exit;
            btnExportData.Text = Lang.ExportLog;
            btnExportResult.Text = Lang.ExportCheckedLog;
            btnExportAll.Text = Lang.ExportAll;

            pnlJobInformation.Text = Lang.JobDetails;
            lblJobName.Text = Lang.FileName;

            lblReceived.Text = "Dữ liệu đã nhận";//Lang.Received;
            lblSentData.Text = Lang.SentData;
            lblPrintedCode.Text = Lang.PrintedCode;

            lblProcessTitle.Text = "  " + Lang.VerifyProgress;
            lblTotalChecked.Text = "Tổng sản phẩm thực tế ";//Lang.TotalChecked;
            lblPassed.Text = "Sản phẩm đạt"; //aa Lang.CheckedPassed;
            lblFailed.Text = "Sản phẩm không đạt";//Lang.CheckedFailed;

            pnlCurrentCheck.Text = Lang.CheckedResult;


            lblStatusCamera01.Text = Lang.CameraTMP;
            lblStatusPrinter01.Text = Lang.Printer;
            lblStatusSerialDevice.Text = Lang.ScannerLabel;
            lblSensorControllerStatus.Text = Lang.PLCLabel;
            toolStripOperationStatus.Text = Lang.Stopped;

            mnChangePassword.Text = Lang.ChangePassword;
            mnManage.Text = Lang.Manage;
            mnLogOut.Text = Lang.LogOut;

            lblDatabase.Text = Lang.Database;
            lblCheckedResult.Text = Lang.CheckedResult1;

            toolStripVersion.Text = Lang.Version + ": " + Properties.Settings.Default.SoftwareVersion;
        }

        private void dgvCheckedResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {


        }

        private async void btnPreviewLogPrint_Click(object sender, EventArgs e)
        {
            var rows = await FetchRlinkLogInRowsAsync();
            if (rows.Count == 0)
            {
                CustomMessageBox.Show("Chưa có dữ liệu log máy in",
                     "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var frm = new frmPreviewLog(rows, refreshCallback: FetchRlinkLogInRowsAsync))
                frm.ShowDialog(this);
        }

        private async void openLogPrint (object sender, EventArgs e)
        {
            var rows = await FetchRlinkLogInRowsAsync();
            if (rows.Count == 0)
            {
                CustomMessageBox.Show("Chưa có dữ liệu log máy in",
                     "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var frm = new frmPreviewLog(rows, refreshCallback: FetchRlinkLogInRowsAsync))
                frm.ShowDialog(this);
        }
        private void lblReceivedValue_Click(object sender, EventArgs e)
        {

        }

        //private async void btnPreviewLogPrint_Click(object sender, EventArgs e)
        //{
        //    // ── Diagnostic: kiểm tra trạng thái service và DB path ──
        //    Console.WriteLine($"[btnPreviewLog] _rlinkLogService = {(_rlinkLogService != null ? "OK" : "NULL")}");
        //    Console.WriteLine($"[btnPreviewLog] SQLite ConnStr = {BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ConnStr}");
        //    Console.WriteLine($"[btnPreviewLog] SQLite file exists = {System.IO.File.Exists(BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.RLinkLogService.ConnStr?.Replace("Data Source=", "").Trim())}");

        //    var rows = await FetchRlinkLogInRowsAsync();
        //    Console.WriteLine($"[btnPreviewLog] rows.Count = {rows.Count}");

        //    if (rows.Count == 0)
        //    {
        //        CustomMessageBox.Show("Chưa có dữ liệu log máy in",
        //             "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        return;
        //    }
        //    using (var frm = new frmPreviewLog(rows, refreshCallback: FetchRlinkLogInRowsAsync))
        //        frm.ShowDialog(this);
        //}




        private async void btnPreviewLogCameraError_Click(object sender, EventArgs e)
        {
            var rows = await FetchRlinkLogCameraErrorRowsAsync();
            if (rows.Count == 0)
            {
                CustomMessageBox.Show("Chưa có dữ liệu lỗi của camera",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var frm = new frmPreviewLog(rows, isCameraError: true, refreshCallback: FetchRlinkLogCameraErrorRowsAsync))
            {
                frm.Text = "Lịch sử Log Camera Error";
                frm.ShowDialog(this);
            }
        }

        private List<string[]> FetchRlinkLogCameraErrorRows()
        {
            string jobName = _SelectedJob?.FileName ?? "";
            string productNameCol = THDb.ProductName;
            string connStr = Services.THTrueMilk.RLinkMaster.RLinkLogService.GetPgConnStr();
            string filter = string.IsNullOrWhiteSpace(jobName) ? "" : $"WHERE {JobName} = @job";

            var result = QueryLogFromBothDbs(connStr, LogCameraError, filter, jobName,
                pgSql: $@"SELECT {Id}::text, COALESCE({JobName},''), COALESCE({Batch},''), 
                                 COALESCE({OperatorUser},''), COALESCE({QrCode},''),
                                 COALESCE({ErrorManufacturedDate},''), COALESCE({ErrorExpiryDate},''),
                                 COALESCE({ErrorType},''), COALESCE({Timestamp}::text,''),
                                 COALESCE({ErrorFrameInfo},''), COALESCE({IsSent}::text,'0') AS {IsSent},
                                 COALESCE({ImagePath},''), COALESCE({ProductId},''), COALESCE({productNameCol},'')
                          FROM {LogCameraError} {filter} ORDER BY {Id} DESC LIMIT 4900",
                sqliteSql: $@"SELECT CAST({Id} AS TEXT), COALESCE({JobName},''), COALESCE({Batch},''),
                                     COALESCE({OperatorUser},''), COALESCE({QrCode},''),
                                     COALESCE({ErrorManufacturedDate},''), COALESCE({ErrorExpiryDate},''),
                                     COALESCE({ErrorType},''), COALESCE({Timestamp},''),
                                     COALESCE({ErrorFrameInfo},''), COALESCE(CAST({IsSent} AS TEXT),''),
                                     COALESCE({ImagePath},''), COALESCE({ProductId},''), COALESCE({productNameCol},'')
                              FROM {LogCameraError} {filter} ORDER BY {Id} DESC LIMIT 4900");

            return result;
        }

        private async System.Threading.Tasks.Task<List<string[]>> FetchRlinkLogCameraErrorRowsAsync()
        {
            return await System.Threading.Tasks.Task.Run(() => FetchRlinkLogCameraErrorRows());
        }

        private void OperatorConfig_Click(object sender, EventArgs e)
        {
            using (var frm = new frmConfigLine())
            {
                frm.Text = "Cấu hình Line sản xuất";
                frm.ShowDialog(this);
            }
        }

        private void pictureBoxPreview_Click(object sender, EventArgs e)
        {

        }

        private void ConfirmLabel_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void pnlDatabase_Paint(object sender, PaintEventArgs e)
        {

        }

        private void LblMasterOffline_Click(object sender, EventArgs e)
        {
            lblMasterOffline.Visible = false;
        }

        /// <summary>Force đóng form — bypass FormClosing cancellation, cleanup hoàn toàn.</summary>
        public void ForceClose()
        {
            _forceClose = true;
            KillAllProccessThread();
            Close();
        }

        //private void ReleaseResource()
        //{
        //    try
        //    {
        //        if (_SelectedJob != null)
        //        {
        //            if (_phanLoaiCount > 0)
        //                _SelectedJob.PhanLoaiCount = _phanLoaiCount;
        //            if (_phanLoaiCount > 0 || _NumberOfSentPrinter > 0 || _ReceivedCode > 0)
        //                _SelectedJob.SaveFile();
        //        }

        //        // ── Stop SaaS/SAP data processes trước khi cancel tokens ──
        //        try { _printedDataProcess?.Stop(); } catch { }
        //        try { _checkedDataProcess?.Stop(); } catch { }

        //        _VirtualCTS?.Cancel();
        //        _benchmarkCTS?.Cancel();
        //        _SendDataToPrinterTokenCTS?.Cancel();
        //        _PrinterRespontCST?.Cancel();
        //        _QueueBufferPrinterResponseData.Enqueue(null);

        //        _benchmarkPrintCycle?.Set();
        //        _benchmarkCTS?.Dispose();
        //        _benchmarkPrintCycle?.Dispose();
        //        _VirtualCTS?.Dispose();
        //        _SendDataToPrinterTokenCTS?.Dispose();
        //        _PrinterRespontCST?.Dispose();

        //        _OperationCancelTokenSource?.Cancel();
        //        _UICheckedResultCancelTokenSource?.Cancel();
        //        _UIPrintedResponseCancelTokenSource?.Cancel();
        //        _BackupImageCancelTokenSource?.Cancel();
        //        _BackupResponseCancelTokenSource?.Cancel();
        //        _BackupResultCancelTokenSource?.Cancel();
        //        _BackupSendLogCancelTokenSource?.Cancel();
        //        _BackupRSFPLogCancelTokenSource?.Cancel();

        //        // ── Gửi null để consumer thoát ──
        //        _QueueBufferBackupPrintedCode.Enqueue(null);

        //        // ── Đợi backup threads hoàn tất trước khi dispose ──
        //        try { if (_backupResponseTask != null) _backupResponseTask.Wait(5000); } catch { }
        //        try { if (_backupSendLogTask != null) _backupSendLogTask.Wait(5000); } catch { }
        //        try { if (_backupRSFPLogTask != null) _backupRSFPLogTask.Wait(5000); } catch { }

        //        // ── Drain nốt items còn sót trong printed queue ──
        //        if (!string.IsNullOrWhiteSpace(_SelectedJob?.PrintedResponePath))
        //        {
        //            string printedPath = System.IO.Path.Combine(CommVariables.PathPrintedResponse, _SelectedJob.PrintedResponePath);
        //            while (_QueueBufferBackupPrintedCode.Count() > 0)
        //            {
        //                var batch = _QueueBufferBackupPrintedCode.Dequeue();
        //                if (batch == null || batch.Count == 0) continue;
        //                try
        //                {
        //                    using (var sw = new System.IO.StreamWriter(printedPath, true, new System.Text.UTF8Encoding(true)))
        //                    {
        //                        foreach (var row in batch)
        //                            sw.WriteLine(string.Join(",", row.Select(x => Csv.Escape(x))));
        //                    }
        //                }
        //                catch { }
        //            }
        //        }

        //        _OperationCancelTokenSource?.Dispose();
        //        _UICheckedResultCancelTokenSource?.Dispose();
        //        _UIPrintedResponseCancelTokenSource?.Dispose();
        //        _BackupImageCancelTokenSource?.Dispose();
        //        _BackupResponseCancelTokenSource?.Dispose();
        //        _BackupResultCancelTokenSource?.Dispose();
        //        _BackupSendLogCancelTokenSource?.Dispose();
        //        _BackupRSFPLogCancelTokenSource?.Dispose();

        //        _CheckedResultCodeList.Clear();
        //        _CodeListPODFormat.Clear();
        //        _PrintedCodeObtainFromFile.Clear();

        //        while (_QueueBufferDataObtained.TryDequeue(out _)) { }
        //        _QueueBufferDataObtainedResult.Clear();
        //        _QueueBufferUpdateUIPrinter.Clear();

        //        _QueueBufferBackupImage.Clear();
        //        _QueueBufferBackupCheckedResult.Clear();
        //        _QueueBufferBackupSendLog.Clear();
        //        _QueueBufferBackupRSFPLog.Clear();

        //        _errorCountA = 0; _errorCountB = 0; _errorCountF = 0; _phanLoaiCount = 0;
        //        ErrorCountA = 0; ErrorCountB = 0; ErrorCountF = 0; PhanLoaiCount = 0;

        //        Shared.OnCameraPositionDataChange -= Shared_OnCameraPositionDataChange;
        //        Shared.OnCameraStatusChange -= Shared_OnCameraStatusChange;
        //        Shared.OnSerialDeviceReadDataChange -= Shared_OnSerialDeviceReadDataChange;
        //        Shared.OnCameraReadDataChange -= Shared_OnCameraReadDataChange;
        //        Shared.OnPrinterDataChange -= Shared_OnPrinterDataChange;
        //        Shared.OnPrintingStateChange -= Shared_OnPrintingStateChange;
        //        Shared.OnPrinterStatusChange -= Shared_OnPrinterStatusChange;
        //        Shared.OnLanguageChange -= Shared_OnLanguageChange;
        //        Shared.OnSensorControllerChangeEvent -= Shared_OnSensorControllerChangeEvent;
        //        Shared.OnSerialDeviceControllerChangeEvent -= Shared_OnSerialDeviceControllerChangeEvent;
        //        Shared.OnVerifyAndPrindSendDataMethod -= Shared_OnVerifyAndPrindSendDataMethod;
        //        Shared.OnLogError -= Shared_OnLogError;
        //        Shared.OnSyncDataParameterChange -= Shared_OnSyncDataParameterChange;
        //        Shared.OnSyncCheckDataParameterChange -= Shared_OnSyncCheckDataParameterChange;
        //        Shared.OnDatabaseStatusChange -= Shared_OnDatabaseStatusChange;
        //        this.OnReceiveVerifyDataEvent -= SendVerifiedDataToPrinter;
        //        if (_ParentForm != null)
        //            _ParentForm.Mode1QrCodeChanged -= ParentForm_OnMode1QrCodeChanged;
        //        // KillThread(ref _ThreadPrinterResponseHandler); // deprecated — thread.Abort() gây crash
        //        // Chỉ set null, thread sẽ tự kết thúc khi nhận CancellationToken
        //        _ThreadPrinterResponseHandler = null;

        //        if (_FormCheckedResult != null)
        //        {
        //            _FormCheckedResult.ForceClose();
        //            _FormCheckedResult = null;
        //        }
        //        _FormPreviewDatabase?.Close();
        //        _FormPreviewDatabase?.Dispose();

        //        // Singleton keeps running — frmJob.Activated sẽ set snapshot factory về frmJob
        //    }
        //    catch (Exception e)
        //    {
        //        throw (e);
        //    }
        //}


        private void ReleaseResource()
        {
            try
            {
                // ── 1. Save job (~0ms) ──
                if (_SelectedJob != null)
                {
                    if (_phanLoaiCount > 0)
                        _SelectedJob.PhanLoaiCount = _phanLoaiCount;
                    if (_phanLoaiCount > 0 || _NumberOfSentPrinter > 0 || _ReceivedCode > 0)
                        _SelectedJob.SaveFile();
                }

                // ── 2. Stop processes (~10ms) ──
                // Không dispose _printedWriter ở đây — để consumer tự drain + dispose
                try { _printedDataProcess?.Stop(); } catch { }
                try { _checkedDataProcess?.Stop(); } catch { }

                // ── 3. Cancel tokens (~0ms) - CRITICAL ──
                _VirtualCTS?.Cancel();
                _benchmarkCTS?.Cancel();
                _SendDataToPrinterTokenCTS?.Cancel();
                _PrinterRespontCST?.Cancel();
                _QueueBufferPrinterResponseData.Enqueue(null);

                _benchmarkPrintCycle?.Set();
                _OperationCancelTokenSource?.Cancel();
                _UICheckedResultCancelTokenSource?.Cancel();
                _UIPrintedResponseCancelTokenSource?.Cancel();
                _BackupImageCancelTokenSource?.Cancel();
                _BackupResponseCancelTokenSource?.Cancel();
                _BackupResultCancelTokenSource?.Cancel();
                _BackupSendLogCancelTokenSource?.Cancel();
                _BackupRSFPLogCancelTokenSource?.Cancel();

                // ── 4. Null-terminate queues (~0ms) - CRITICAL ──
                _QueueBufferBackupPrintedCode.Enqueue(null);

                // ── 5. Đợi consumer exit — tối đa 10 giây ──
                try { _backupResponseTask?.Wait(10000); } catch { }
                try { _backupSendLogTask?.Wait(10000); } catch { }
                try { _backupRSFPLogTask?.Wait(10000); } catch { }

                // ── 6. Dispose CTS tokens (~0ms) ──
                _benchmarkCTS?.Dispose();
                _benchmarkPrintCycle?.Dispose();
                _VirtualCTS?.Dispose();
                _SendDataToPrinterTokenCTS?.Dispose();
                _PrinterRespontCST?.Dispose();
                _OperationCancelTokenSource?.Dispose();
                _UICheckedResultCancelTokenSource?.Dispose();
                _UIPrintedResponseCancelTokenSource?.Dispose();
                _BackupImageCancelTokenSource?.Dispose();
                _BackupResponseCancelTokenSource?.Dispose();
                _BackupResultCancelTokenSource?.Dispose();
                _BackupSendLogCancelTokenSource?.Dispose();
                _BackupRSFPLogCancelTokenSource?.Dispose();

                // ── 8. Clear lists & queues (~0ms) ──
                _CheckedResultCodeList.Clear();
                _CodeListPODFormat.Clear();
                _PrintedCodeObtainFromFile.Clear();
                while (_QueueBufferDataObtained.TryDequeue(out _)) { }
                _QueueBufferDataObtainedResult.Clear();
                _QueueBufferUpdateUIPrinter.Clear();
                _QueueBufferBackupImage.Clear();
                _QueueBufferBackupCheckedResult.Clear();
                _QueueBufferBackupSendLog.Clear();
                _QueueBufferBackupRSFPLog.Clear();
                _errorCountA = 0; _errorCountB = 0; _errorCountF = 0; _phanLoaiCount = 0;
                ErrorCountA = 0; ErrorCountB = 0; ErrorCountF = 0; PhanLoaiCount = 0;

                // ── 9. Unsubscribe events (~0ms) - CRITICAL ──
                Shared.OnCameraPositionDataChange -= Shared_OnCameraPositionDataChange;
                Shared.OnCameraStatusChange -= Shared_OnCameraStatusChange;
                Shared.OnSerialDeviceReadDataChange -= Shared_OnSerialDeviceReadDataChange;
                Shared.OnCameraReadDataChange -= Shared_OnCameraReadDataChange;
                Shared.OnPrinterDataChange -= Shared_OnPrinterDataChange;
                Shared.OnPrintingStateChange -= Shared_OnPrintingStateChange;
                Shared.OnPrinterStatusChange -= Shared_OnPrinterStatusChange;
                Shared.OnLanguageChange -= Shared_OnLanguageChange;
                Shared.OnSensorControllerChangeEvent -= Shared_OnSensorControllerChangeEvent;
                Shared.OnSerialDeviceControllerChangeEvent -= Shared_OnSerialDeviceControllerChangeEvent;
                Shared.OnVerifyAndPrindSendDataMethod -= Shared_OnVerifyAndPrindSendDataMethod;
                Shared.OnLogError -= Shared_OnLogError;
                Shared.OnSyncDataParameterChange -= Shared_OnSyncDataParameterChange;
                Shared.OnSyncCheckDataParameterChange -= Shared_OnSyncCheckDataParameterChange;
                Shared.OnDatabaseStatusChange -= Shared_OnDatabaseStatusChange;
                this.OnReceiveVerifyDataEvent -= SendVerifiedDataToPrinter;
                if (_ParentForm != null)
                {
                    _ParentForm.Mode1QrCodeChanged -= ParentForm_OnQrCodeChanged;
                    _ParentForm.Mode1QrCodeChanged -= ParentForm_OnMode1QrCodeChanged;
                }
                _ThreadPrinterResponseHandler = null;

                // ── 10. Close child forms (~50ms, sync) ──
                if (_FormCheckedResult != null)
                {
                    _FormCheckedResult.Close();
                    _FormCheckedResult = null;
                }
                _FormPreviewDatabase?.Close();
                _FormPreviewDatabase?.Dispose();
            }
            catch (Exception e)
            {
                throw (e);
            }
        }


        public static void AutoResizeColumnWith(DataGridView dgv, string[] value, int imgIndex = 0)
        {
            try
            {
                var firstRowWith = value;
                int totalColumnsWidth = TextRenderer.MeasureText(firstRowWith[0], dgv.Font).Width;
                int[] thickestRowIndex = { 0, TextRenderer.MeasureText(firstRowWith[0], dgv.Font).Width };
                int maxCol = Math.Min(firstRowWith.Length, dgv.Columns.Count);
                for (int i = 1; i < maxCol; i++)
                {
                    if (i == imgIndex)
                    {
                        totalColumnsWidth += dgv.Columns[i].Width;
                        continue;
                    }
                    Size colTextSize = TextRenderer.MeasureText(dgv.Columns[i].HeaderText, dgv.Font);
                    Size rowTextSize = TextRenderer.MeasureText(firstRowWith[i], dgv.Font);
                    if (rowTextSize.Width > thickestRowIndex[1])
                    {
                        thickestRowIndex[0] = i;
                        thickestRowIndex[1] = rowTextSize.Width;
                    }

                    if (colTextSize.Width < rowTextSize.Width)
                    {
                        dgv.Columns[i].Width = rowTextSize.Width + 40;
                    }
                    else if (colTextSize.Width > rowTextSize.Width)
                    {
                        dgv.Columns[i].Width = colTextSize.Width + 40;
                    }
                    totalColumnsWidth += dgv.Columns[i].Width;
                }
                if (totalColumnsWidth < dgv.Width)
                {
                    dgv.Columns[thickestRowIndex[0]].Width += dgv.Width - totalColumnsWidth - 35;
                }
            }
            catch
            {

            }
        }

        #region Progress bar
        private void ProgressBarInitialize()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ProgressBarInitialize()));
                return;
            }
            if (_SelectedJob != null)
            {
                if (_SelectedJob.CompareType == CompareType.Database)
                {
                    prBarCheckPassed.Maximum = 100;
                    prBarCheckPassed.Minimum = 0;
                    prBarCheckPassed.Update();
                }
                else
                {
                    prBarCheckPassed.Maximum = 100;
                    prBarCheckPassed.Minimum = 0;
                    prBarCheckPassed.Update();
                }
            }
        }

        private void ProgressBarCheckedUpdate()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ProgressBarCheckedUpdate()));
                return;
            }

            try
            {
                var progress = 0;
                if (_SelectedJob.CompareType == CompareType.Database)
                {
                    //Check passed
                    progress = _TotalCode > 0 ? NumberOfCheckPassed * 100 / (_TotalCode - _NumberOfDuplicate) : 0;
                }
                else
                {
                    //Check passed
                    progress = TotalChecked > 0 ? NumberOfCheckPassed * 100 / TotalChecked : 0;
                }

                if (progress < 100)
                {
                    prBarCheckPassed.Text = string.Format("{0:N0}%", progress);//{0:N3} 0.000 decimal
                    prBarCheckPassed.Value = progress;
                }
                else
                {
                    prBarCheckPassed.Value = 100;
                    prBarCheckPassed.Text = string.Format("100%");//{0:N3} 0.000 decimal
                }

                prBarCheckPassed.Invalidate();
            }
            catch (Exception)
            {

            }
        }

        #endregion Progress bar



        #endregion

        //    #region UI
        //    private TableLayoutPanel _tblSummary;

        //    private void CreateSummaryTable(int rowIndex)
        //    {
        //        if (_tblSummary != null)
        //            return;

        //        Label AddCell(
        //            string text,
        //            int column,
        //            int row,
        //            bool merge = false,
        //            float fontSize = 11F,
        //            FontStyle style = FontStyle.Regular)
        //        {
        //            var lbl = new Label
        //            {
        //                Text = text,
        //                Dock = DockStyle.Fill,
        //                Margin = Padding.Empty,
        //                Padding = new Padding(2),
        //                BackColor = Color.White,
        //                TextAlign = ContentAlignment.MiddleCenter,
        //                ForeColor = SystemColors.WindowText,
        //                Font = new Font(
        //                    "Microsoft Sans Serif",
        //                    fontSize,
        //                    style)
        //            };

        //            _tblSummary.Controls.Add(lbl, column, row);

        //            if (merge)
        //                _tblSummary.SetColumnSpan(lbl, 3);

        //            return lbl;
        //        }

        //        _tblSummary = new TableLayoutPanel
        //        {
        //            Dock = DockStyle.Fill,
        //            Margin = Padding.Empty,

        //            BackColor = Color.White,
        //            ColumnCount = 3,
        //            RowCount = 8,
        //            CellBorderStyle =
        //                TableLayoutPanelCellBorderStyle.Single,
        //            Padding = new Padding(0, 0, 0, 10)
        //        };
        //       // Wrap _tblSummary trong RoundPanel để bo góc
        //       //var wrapper = new DesignUI.CuzUI.RoundPanel
        //       //{
        //       //    Dock = DockStyle.Fill,
        //       //    Radious = 15,
        //       //    BackColor = Color.White,
        //       //    TitleHeight = 0

        //       //};
        //       // wrapper.Controls.Add(_tblSummary);
        //       // wrapper.Padding = new Padding(0, 0, 0, 5);
        //        // Add wrapper thay vì _tblSummary
        //        tableLayoutPanelProcess.Controls.Add(_tblSummary, 0, rowIndex);
        //        tableLayoutPanelProcess.SetColumnSpan(_tblSummary, tableLayoutPanelProcess.ColumnCount);
        //        _tblSummary.Margin = new Padding(0, 0, 0, 10);
        //        _tblSummary.Padding = new Padding(0, 0, 0, 10);
        //        _tblSummary.ColumnStyles.Add(
        //            new ColumnStyle(SizeType.Percent, 34F));

        //        _tblSummary.ColumnStyles.Add(
        //            new ColumnStyle(SizeType.Percent, 33F));

        //        _tblSummary.ColumnStyles.Add(
        //            new ColumnStyle(SizeType.Percent, 33F));

        //        float[] heights =
        //        {
        //    48, 50, 42, 45, 45, 42, 45, 45
        //};

        //        foreach (float height in heights)
        //        {
        //            _tblSummary.RowStyles.Add(
        //                new RowStyle(
        //                    SizeType.Absolute,
        //                    height));
        //        }

        //        AddCell(
        //            "TỔNG KIỂM TRA",
        //            0,
        //            0,
        //            true,
        //            16F,
        //            FontStyle.Bold);

        //        AddCell(
        //            TotalChecked.ToString("N0"),
        //            0,
        //            1,
        //            true,
        //            21F);

        //        AddCell(
        //            "Check QR code",
        //            0,
        //            2,
        //            true,
        //            16F,
        //            FontStyle.Bold);

        //        int qrPass = NumberOfCheckPassed;
        //        int qrFail = NumberOfCheckFailed;
        //        double qrPassPct = TotalChecked > 0 ? (double)qrPass / TotalChecked * 100 : 0;
        //        double qrFailPct = TotalChecked > 0 ? (double)qrFail / TotalChecked * 100 : 0;

        //        AddCell("Đạt", 0, 3, false, 15F);
        //        AddCell(qrPass.ToString("N0"), 1, 3, false, 15F);
        //        AddCell($"{qrPassPct:F2}%", 2, 3, false, 15F);

        //        AddCell("Không đạt", 0, 4, false, 15F);
        //        AddCell(qrFail.ToString("N0"), 1, 4, false, 15F);
        //        AddCell($"{qrFailPct:F2}%", 2, 4, false, 15F);

        //        AddCell(
        //            "Check Date",
        //            0,
        //            5,
        //            true,
        //            16F,
        //            FontStyle.Bold);

        //        int datePass = _DateCheckPassed;
        //        int dateFail = _DateCheckFailed;
        //        int dateTotal = datePass + dateFail;
        //        double datePassPct = dateTotal > 0 ? (double)datePass / dateTotal * 100 : 0;
        //        double dateFailPct = dateTotal > 0 ? (double)dateFail / dateTotal * 100 : 0;

        //        AddCell("Đạt", 0, 6, false, 15F);
        //        AddCell(datePass.ToString("N0"), 1, 6, false, 15F);
        //        AddCell($"{datePassPct:F2}%", 2, 6, false, 15F);

        //        AddCell("Không đạt", 0, 7, false, 15F);
        //        AddCell(dateFail.ToString("N0"), 1, 7, false, 15F);
        //        AddCell($"{dateFailPct:F2}%", 2, 7, false, 15F);

        //    }

        //    private void UpdateSummaryTable()
        //    {
        //        if (_tblSummary == null) return;

        //        // Row 1: Tổng kiểm tra
        //        var totalLabel = _tblSummary.GetControlFromPosition(0, 1) as Label;
        //        if (totalLabel != null) totalLabel.Text = TotalChecked.ToString("N0");

        //        // Row 3: Check QR code - Đạt
        //        int qrPass = NumberOfCheckPassed;
        //        int qrFail = NumberOfCheckFailed;
        //        double qrPassPct = TotalChecked > 0 ? (double)qrPass / TotalChecked * 100 : 0;
        //        double qrFailPct = TotalChecked > 0 ? (double)qrFail / TotalChecked * 100 : 0;

        //        var qrPassLabel = _tblSummary.GetControlFromPosition(1, 3) as Label;
        //        if (qrPassLabel != null) qrPassLabel.Text = qrPass.ToString("N0");
        //        var qrPassPctLabel = _tblSummary.GetControlFromPosition(2, 3) as Label;
        //        if (qrPassPctLabel != null) qrPassPctLabel.Text = $"{qrPassPct:F2}%";

        //        // Row 4: Check QR code - Không đạt
        //        var qrFailLabel = _tblSummary.GetControlFromPosition(1, 4) as Label;
        //        if (qrFailLabel != null) qrFailLabel.Text = qrFail.ToString("N0");
        //        var qrFailPctLabel = _tblSummary.GetControlFromPosition(2, 4) as Label;
        //        if (qrFailPctLabel != null) qrFailPctLabel.Text = $"{qrFailPct:F2}%";

        //        // Row 6: Check Date - Đạt
        //        int datePass = _DateCheckPassed;
        //        int dateFail = _DateCheckFailed;
        //        int dateTotal = datePass + dateFail;
        //        double datePassPct = dateTotal > 0 ? (double)datePass / dateTotal * 100 : 0;
        //        double dateFailPct = dateTotal > 0 ? (double)dateFail / dateTotal * 100 : 0;

        //        var datePassLabel = _tblSummary.GetControlFromPosition(1, 6) as Label;
        //        if (datePassLabel != null) datePassLabel.Text = datePass.ToString("N0");
        //        var datePassPctLabel = _tblSummary.GetControlFromPosition(2, 6) as Label;
        //        if (datePassPctLabel != null) datePassPctLabel.Text = $"{datePassPct:F2}%";

        //        // Row 7: Check Date - Không đạt
        //        var dateFailLabel = _tblSummary.GetControlFromPosition(1, 7) as Label;
        //        if (dateFailLabel != null) dateFailLabel.Text = dateFail.ToString("N0");
        //        var dateFailPctLabel = _tblSummary.GetControlFromPosition(2, 7) as Label;
        //        if (dateFailPctLabel != null) dateFailPctLabel.Text = $"{dateFailPct:F2}%";
        //    }
        //    #endregion
        //#region UI - Summary table anti flicker

        //private const int WM_SETREDRAW = 0x000B;

        //private BufferedTableLayoutPanel _tblSummary;

        //private Label _summaryTotalLabel;
        //private Label _summaryQrPassLabel;
        //private Label _summaryQrPassRateLabel;
        //private Label _summaryQrFailLabel;
        //private Label _summaryQrFailRateLabel;
        //private Label _summaryDatePassLabel;
        //private Label _summaryDatePassRateLabel;
        //private Label _summaryDateFailLabel;
        //private Label _summaryDateFailRateLabel;

        //private void CreateSummaryTable(int rowIndex)
        //{
        //    if (_tblSummary != null &&
        //        !_tblSummary.IsDisposed)
        //    {
        //        return;
        //    }

        //    Label AddCell(
        //        string text,
        //        int column,
        //        int row,
        //        bool merge = false,
        //        float fontSize = 11F,
        //        FontStyle style = FontStyle.Regular)
        //    {
        //        Label label = new Label
        //        {
        //            AutoSize = false,
        //            Text = text,
        //            Dock = DockStyle.Fill,
        //            Margin = Padding.Empty,
        //            Padding = new Padding(2),
        //            BackColor = Color.White,
        //            TextAlign =
        //                ContentAlignment.MiddleCenter,
        //            ForeColor =
        //                SystemColors.WindowText,
        //            UseCompatibleTextRendering = false,
        //            Font = new Font(
        //                "Microsoft Sans Serif",
        //                fontSize,
        //                style)
        //        };

        //        _tblSummary.Controls.Add(
        //            label,
        //            column,
        //            row);

        //        if (merge)
        //        {
        //            _tblSummary.SetColumnSpan(
        //                label,
        //                3);
        //        }

        //        return label;
        //    }

        //    _tblSummary =
        //        new BufferedTableLayoutPanel
        //        {
        //            Name = "tblSummary",
        //            Dock = DockStyle.Fill,
        //            Margin = Padding.Empty,
        //            Padding =
        //                new Padding(0, 0, 0, 10),
        //            BackColor = Color.White,
        //            ColumnCount = 3,
        //            RowCount = 8,
        //            GrowStyle =
        //                TableLayoutPanelGrowStyle.FixedSize,
        //            CellBorderStyle =
        //                TableLayoutPanelCellBorderStyle.Single
        //        };

        //    _tblSummary.SuspendLayout();

        //    _tblSummary.ColumnStyles.Add(
        //        new ColumnStyle(
        //            SizeType.Percent,
        //            34F));

        //    _tblSummary.ColumnStyles.Add(
        //        new ColumnStyle(
        //            SizeType.Percent,
        //            33F));

        //    _tblSummary.ColumnStyles.Add(
        //        new ColumnStyle(
        //            SizeType.Percent,
        //            33F));

        //    float[] heights =
        //    {
        //        48F,
        //        50F,
        //        42F,
        //        45F,
        //        45F,
        //        42F,
        //        45F,
        //        45F
        //    };

        //    foreach (float height in heights)
        //    {
        //        _tblSummary.RowStyles.Add(
        //            new RowStyle(
        //                SizeType.Absolute,
        //                height));
        //    }

        //    AddCell(
        //        "TỔNG KIỂM TRA",
        //        0,
        //        0,
        //        true,
        //        16F,
        //        FontStyle.Bold);

        //    _summaryTotalLabel = AddCell(
        //        TotalChecked.ToString("N0"),
        //        0,
        //        1,
        //        true,
        //        21F);

        //    AddCell(
        //        "Check QR code",
        //        0,
        //        2,
        //        true,
        //        16F,
        //        FontStyle.Bold);

        //    AddCell(
        //        "Đạt",
        //        0,
        //        3,
        //        false,
        //        15F);

        //    _summaryQrPassLabel = AddCell(
        //        NumberOfCheckPassed.ToString("N0"),
        //        1,
        //        3,
        //        false,
        //        15F);

        //    _summaryQrPassRateLabel = AddCell(
        //        FormatSummaryPercent(
        //            NumberOfCheckPassed,
        //            TotalChecked),
        //        2,
        //        3,
        //        false,
        //        15F);

        //    AddCell(
        //        "Không đạt",
        //        0,
        //        4,
        //        false,
        //        15F);

        //    _summaryQrFailLabel = AddCell(
        //        NumberOfCheckFailed.ToString("N0"),
        //        1,
        //        4,
        //        false,
        //        15F);

        //    _summaryQrFailRateLabel = AddCell(
        //        FormatSummaryPercent(
        //            NumberOfCheckFailed,
        //            TotalChecked),
        //        2,
        //        4,
        //        false,
        //        15F);

        //    AddCell(
        //        "Check Date",
        //        0,
        //        5,
        //        true,
        //        16F,
        //        FontStyle.Bold);

        //    int dateTotal =
        //        _DateCheckPassed +
        //        _DateCheckFailed;

        //    AddCell(
        //        "Đạt",
        //        0,
        //        6,
        //        false,
        //        15F);

        //    _summaryDatePassLabel = AddCell(
        //        _DateCheckPassed.ToString("N0"),
        //        1,
        //        6,
        //        false,
        //        15F);

        //    _summaryDatePassRateLabel = AddCell(
        //        FormatSummaryPercent(
        //            _DateCheckPassed,
        //            dateTotal),
        //        2,
        //        6,
        //        false,
        //        15F);

        //    AddCell(
        //        "Không đạt",
        //        0,
        //        7,
        //        false,
        //        15F);

        //    _summaryDateFailLabel = AddCell(
        //        _DateCheckFailed.ToString("N0"),
        //        1,
        //        7,
        //        false,
        //        15F);

        //    _summaryDateFailRateLabel = AddCell(
        //        FormatSummaryPercent(
        //            _DateCheckFailed,
        //            dateTotal),
        //        2,
        //        7,
        //        false,
        //        15F);

        //    _tblSummary.ResumeLayout(false);

        //    int targetRow = Math.Max(
        //        0,
        //        Math.Min(
        //            rowIndex,
        //            tableLayoutPanelProcess.RowCount - 1));

        //    tableLayoutPanelProcess.SuspendLayout();

        //    tableLayoutPanelProcess.Controls.Add(
        //        _tblSummary,
        //        0,
        //        targetRow);

        //    tableLayoutPanelProcess.SetColumnSpan(
        //        _tblSummary,
        //        tableLayoutPanelProcess.ColumnCount);

        //    tableLayoutPanelProcess.SetRowSpan(
        //        _tblSummary,
        //        1);

        //    tableLayoutPanelProcess.ResumeLayout(true);
        //}

        //private void UpdateSummaryTable()
        //{
        //    if (_tblSummary == null ||
        //        _tblSummary.IsDisposed)
        //    {
        //        return;
        //    }

        //    int total =
        //        Math.Max(0, TotalChecked);

        //    int qrPass =
        //        Math.Max(0, NumberOfCheckPassed);

        //    int qrFail =
        //        Math.Max(0, NumberOfCheckFailed);

        //    int datePass =
        //        Math.Max(0, _DateCheckPassed);

        //    int dateFail =
        //        Math.Max(0, _DateCheckFailed);

        //    int dateTotal =
        //        datePass + dateFail;

        //    string totalText =
        //        total.ToString("N0");

        //    string qrPassText =
        //        qrPass.ToString("N0");

        //    string qrPassRateText =
        //        FormatSummaryPercent(
        //            qrPass,
        //            total);

        //    string qrFailText =
        //        qrFail.ToString("N0");

        //    string qrFailRateText =
        //        FormatSummaryPercent(
        //            qrFail,
        //            total);

        //    string datePassText =
        //        datePass.ToString("N0");

        //    string datePassRateText =
        //        FormatSummaryPercent(
        //            datePass,
        //            dateTotal);

        //    string dateFailText =
        //        dateFail.ToString("N0");

        //    string dateFailRateText =
        //        FormatSummaryPercent(
        //            dateFail,
        //            dateTotal);

        //    // Không redraw nếu toàn bộ giá trị đều giữ nguyên.
        //    bool hasChange =
        //        IsLabelTextDifferent(
        //            _summaryTotalLabel,
        //            totalText) ||
        //        IsLabelTextDifferent(
        //            _summaryQrPassLabel,
        //            qrPassText) ||
        //        IsLabelTextDifferent(
        //            _summaryQrPassRateLabel,
        //            qrPassRateText) ||
        //        IsLabelTextDifferent(
        //            _summaryQrFailLabel,
        //            qrFailText) ||
        //        IsLabelTextDifferent(
        //            _summaryQrFailRateLabel,
        //            qrFailRateText) ||
        //        IsLabelTextDifferent(
        //            _summaryDatePassLabel,
        //            datePassText) ||
        //        IsLabelTextDifferent(
        //            _summaryDatePassRateLabel,
        //            datePassRateText) ||
        //        IsLabelTextDifferent(
        //            _summaryDateFailLabel,
        //            dateFailText) ||
        //        IsLabelTextDifferent(
        //            _summaryDateFailRateLabel,
        //            dateFailRateText);

        //    if (!hasChange)
        //        return;

        //    bool redrawDisabled = false;

        //    try
        //    {
        //        _tblSummary.SuspendLayout();

        //        if (_tblSummary.IsHandleCreated)
        //        {
        //            SendMessage(
        //                _tblSummary.Handle,
        //                WM_SETREDRAW,
        //                0,
        //                0);

        //            redrawDisabled = true;
        //        }

        //        SetLabelTextIfChanged(
        //            _summaryTotalLabel,
        //            totalText);

        //        SetLabelTextIfChanged(
        //            _summaryQrPassLabel,
        //            qrPassText);

        //        SetLabelTextIfChanged(
        //            _summaryQrPassRateLabel,
        //            qrPassRateText);

        //        SetLabelTextIfChanged(
        //            _summaryQrFailLabel,
        //            qrFailText);

        //        SetLabelTextIfChanged(
        //            _summaryQrFailRateLabel,
        //            qrFailRateText);

        //        SetLabelTextIfChanged(
        //            _summaryDatePassLabel,
        //            datePassText);

        //        SetLabelTextIfChanged(
        //            _summaryDatePassRateLabel,
        //            datePassRateText);

        //        SetLabelTextIfChanged(
        //            _summaryDateFailLabel,
        //            dateFailText);

        //        SetLabelTextIfChanged(
        //            _summaryDateFailRateLabel,
        //            dateFailRateText);
        //    }
        //    finally
        //    {
        //        // false: không ép TableLayoutPanel layout lại toàn bộ.
        //        _tblSummary.ResumeLayout(false);

        //        if (redrawDisabled &&
        //            _tblSummary.IsHandleCreated)
        //        {
        //            SendMessage(
        //                _tblSummary.Handle,
        //                WM_SETREDRAW,
        //                1,
        //                0);
        //        }

        //        // Chỉ yêu cầu một lần Paint sau khi cập nhật xong.
        //        _tblSummary.Invalidate(true);
        //    }
        //}

        //private static bool IsLabelTextDifferent(
        //    Label label,
        //    string value)
        //{
        //    return label != null &&
        //        !label.IsDisposed &&
        //        !string.Equals(
        //            label.Text,
        //            value ?? string.Empty,
        //            StringComparison.Ordinal);
        //}

        //private static void SetLabelTextIfChanged(
        //    Label label,
        //    string value)
        //{
        //    if (label == null ||
        //        label.IsDisposed)
        //    {
        //        return;
        //    }

        //    value = value ?? string.Empty;

        //    if (!string.Equals(
        //        label.Text,
        //        value,
        //        StringComparison.Ordinal))
        //    {
        //        label.Text = value;
        //    }
        //}

        //private static string FormatSummaryPercent(
        //    int value,
        //    int total)
        //{
        //    if (total <= 0)
        //        return "0.00%";

        //    double percentage =
        //        value * 100.0 / total;

        //    return percentage.ToString(
        //        "F2",
        //        CultureInfo.CurrentCulture) + "%";
        //}

        //private sealed class BufferedTableLayoutPanel
        //    : TableLayoutPanel
        //{
        //    public BufferedTableLayoutPanel()
        //    {
        //        DoubleBuffered = true;
        //        ResizeRedraw = false;

        //        SetStyle(
        //            ControlStyles.AllPaintingInWmPaint |
        //            ControlStyles.UserPaint |
        //            ControlStyles.OptimizedDoubleBuffer,
        //            true);

        //        UpdateStyles();
        //    }
        //}

        //#endregion


        //#region UI - Summary table modern style

        //private const int SummaryUpdateIntervalMs = 100;

        //private BufferedTableLayoutPanel _tblSummary;

        //private Label _summaryTotalLabel;
        //private Label _summaryQrPassLabel;
        //private Label _summaryQrPassRateLabel;
        //private Label _summaryQrFailLabel;
        //private Label _summaryQrFailRateLabel;
        //private Label _summaryDatePassLabel;
        //private Label _summaryDatePassRateLabel;
        //private Label _summaryDateFailLabel;
        //private Label _summaryDateFailRateLabel;

        //private System.Windows.Forms.Timer _summaryUpdateTimer;

        //private int _summaryUpdatePending;
        //private int _summaryUiDispatchPending;

        //private void CreateSummaryTable(int rowIndex)
        //{
        //    if (_tblSummary != null &&
        //        !_tblSummary.IsDisposed)
        //    {
        //        return;
        //    }

        //    EnableDoubleBuffering(
        //        pnlVerificationProcess);

        //    EnableDoubleBuffering(
        //        tableLayoutPanelProcess);

        //    _tblSummary =
        //        new BufferedTableLayoutPanel
        //        {
        //            Name = "tblSummary",
        //            Dock = DockStyle.Fill,
        //            Margin = new Padding(10, 8, 10, 10),
        //            Padding = new Padding(2),
        //            BackColor = Color.White,
        //            BorderColor =
        //                Color.FromArgb(221, 228, 231),
        //            BorderThickness = 1,
        //            CornerRadius = 9,
        //            ColumnCount = 3,
        //            RowCount = 8,
        //            GrowStyle =
        //                TableLayoutPanelGrowStyle.FixedSize,
        //            CellBorderStyle =
        //                TableLayoutPanelCellBorderStyle.Single
        //        };

        //    _tblSummary.ColumnStyles.Add(
        //        new ColumnStyle(
        //            SizeType.Percent,
        //            40F));

        //    _tblSummary.ColumnStyles.Add(
        //        new ColumnStyle(
        //            SizeType.Percent,
        //            30F));

        //    _tblSummary.ColumnStyles.Add(
        //        new ColumnStyle(
        //            SizeType.Percent,
        //            30F));

        //    float[] heights =
        //    {
        //        44F,
        //        58F,
        //        38F,
        //        42F,
        //        42F,
        //        38F,
        //        42F,
        //        42F
        //    };

        //    foreach (float height in heights)
        //    {
        //        _tblSummary.RowStyles.Add(
        //            new RowStyle(
        //                SizeType.Absolute,
        //                height));
        //    }

        //    _tblSummary.SuspendLayout();

        //    AddSummaryCell(
        //        "TỔNG KIỂM TRA",
        //        0,
        //        0,
        //        merge: true,
        //        role: "title");

        //    _summaryTotalLabel = AddSummaryCell(
        //        TotalChecked.ToString("N0"),
        //        0,
        //        1,
        //        merge: true,
        //        role: "bigvalue");

        //    AddSummaryCell(
        //        "▦ CHECK QR CODE",
        //        0,
        //        2,
        //        merge: true,
        //        role: "section");

        //    AddSummaryCell(
        //        "● Đạt",
        //        0,
        //        3,
        //        role: "passcaption");

        //    _summaryQrPassLabel = AddSummaryCell(
        //        NumberOfCheckPassed.ToString("N0"),
        //        1,
        //        3,
        //        role: "passvalue");

        //    _summaryQrPassRateLabel = AddSummaryCell(
        //        FormatSummaryPercent(
        //            NumberOfCheckPassed,
        //            TotalChecked),
        //        2,
        //        3,
        //        role: "rate");

        //    AddSummaryCell(
        //        "● Không đạt",
        //        0,
        //        4,
        //        role: "failcaption");

        //    _summaryQrFailLabel = AddSummaryCell(
        //        NumberOfCheckFailed.ToString("N0"),
        //        1,
        //        4,
        //        role: "failvalue");

        //    _summaryQrFailRateLabel = AddSummaryCell(
        //        FormatSummaryPercent(
        //            NumberOfCheckFailed,
        //            TotalChecked),
        //        2,
        //        4,
        //        role: "rate");

        //    AddSummaryCell(
        //        "▦ CHECK DATE",
        //        0,
        //        5,
        //        merge: true,
        //        role: "section");

        //    int dateTotal =
        //        _DateCheckPassed +
        //        _DateCheckFailed;

        //    AddSummaryCell(
        //        "● Đạt",
        //        0,
        //        6,
        //        role: "passcaption");

        //    _summaryDatePassLabel = AddSummaryCell(
        //        _DateCheckPassed.ToString("N0"),
        //        1,
        //        6,
        //        role: "passvalue");

        //    _summaryDatePassRateLabel = AddSummaryCell(
        //        FormatSummaryPercent(
        //            _DateCheckPassed,
        //            dateTotal),
        //        2,
        //        6,
        //        role: "rate");

        //    AddSummaryCell(
        //        "● Không đạt",
        //        0,
        //        7,
        //        role: "failcaption");

        //    _summaryDateFailLabel = AddSummaryCell(
        //        _DateCheckFailed.ToString("N0"),
        //        1,
        //        7,
        //        role: "failvalue");

        //    _summaryDateFailRateLabel = AddSummaryCell(
        //        FormatSummaryPercent(
        //            _DateCheckFailed,
        //            dateTotal),
        //        2,
        //        7,
        //        role: "rate");

        //    _tblSummary.ResumeLayout(false);

        //    int targetRow = Math.Max(
        //        0,
        //        Math.Min(
        //            rowIndex,
        //            tableLayoutPanelProcess.RowCount - 1));

        //    tableLayoutPanelProcess.SuspendLayout();

        //    tableLayoutPanelProcess.Controls.Add(
        //        _tblSummary,
        //        0,
        //        targetRow);

        //    tableLayoutPanelProcess.SetColumnSpan(
        //        _tblSummary,
        //        tableLayoutPanelProcess.ColumnCount);

        //    tableLayoutPanelProcess.SetRowSpan(
        //        _tblSummary,
        //        1);

        //    tableLayoutPanelProcess.ResumeLayout(true);

        //    EnsureSummaryUpdateTimer();
        //}

        //private Label AddSummaryCell(
        //    string textValue,
        //    int column,
        //    int row,
        //    bool merge = false,
        //    string role = "normal")
        //{
        //    Label label = new BufferedLabel
        //    {
        //        AutoSize = false,
        //        Text = textValue,
        //        Dock = DockStyle.Fill,
        //        Margin = Padding.Empty,
        //        Padding = new Padding(10, 2, 10, 2),
        //        UseCompatibleTextRendering = false,
        //        TextAlign = ContentAlignment.MiddleCenter,
        //        BackColor = Color.White,
        //        ForeColor = Color.FromArgb(44, 62, 80),
        //        Font = new Font(
        //            "Segoe UI",
        //            10F,
        //            FontStyle.Regular,
        //            GraphicsUnit.Point)
        //    };

        //    switch (role)
        //    {
        //        case "title":
        //            label.BackColor = Color.FromArgb(241, 246, 250);
        //            label.ForeColor = Color.FromArgb(42, 126, 171);
        //            label.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        //            break;

        //        case "bigvalue":
        //            label.BackColor = Color.White;
        //            label.ForeColor = Color.FromArgb(33, 33, 33);
        //            label.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        //            break;

        //        case "section":
        //            label.BackColor = Color.FromArgb(244, 248, 252);
        //            label.ForeColor = Color.FromArgb(67, 118, 153);
        //            label.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        //            label.TextAlign = ContentAlignment.MiddleLeft;
        //            break;

        //        case "passcaption":
        //            label.BackColor = Color.White;
        //            label.ForeColor = Color.FromArgb(54, 68, 79);
        //            label.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        //            label.TextAlign = ContentAlignment.MiddleLeft;
        //            break;

        //        case "failcaption":
        //            label.BackColor = Color.White;
        //            label.ForeColor = Color.FromArgb(54, 68, 79);
        //            label.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        //            label.TextAlign = ContentAlignment.MiddleLeft;
        //            break;

        //        case "passvalue":
        //            label.BackColor = Color.White;
        //            label.ForeColor = Color.FromArgb(63, 170, 110);
        //            label.Font = new Font("Segoe UI", 14F, FontStyle.Regular);
        //            break;

        //        case "failvalue":
        //            label.BackColor = Color.White;
        //            label.ForeColor = Color.FromArgb(219, 88, 88);
        //            label.Font = new Font("Segoe UI", 14F, FontStyle.Regular);
        //            break;

        //        case "rate":
        //            label.BackColor = Color.White;
        //            label.ForeColor = Color.FromArgb(49, 49, 49);
        //            label.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        //            break;
        //    }

        //    _tblSummary.Controls.Add(
        //        label,
        //        column,
        //        row);

        //    if (merge)
        //    {
        //        _tblSummary.SetColumnSpan(
        //            label,
        //            3);
        //    }

        //    return label;
        //}

        ///// <summary>
        ///// Có thể được gọi nhiều lần từ timer hoặc BeginInvoke.
        ///// Hàm này chỉ đánh dấu bảng cần cập nhật; dữ liệu được gom lại
        ///// và vẽ tối đa 10 lần/giây.
        ///// </summary>
        //private void UpdateSummaryTable()
        //{
        //    if (_tblSummary == null ||
        //        _tblSummary.IsDisposed ||
        //        IsDisposed)
        //    {
        //        return;
        //    }

        //    Interlocked.Exchange(
        //        ref _summaryUpdatePending,
        //        1);

        //    if (!IsHandleCreated)
        //        return;

        //    if (InvokeRequired)
        //    {
        //        if (Interlocked.Exchange(
        //            ref _summaryUiDispatchPending,
        //            1) != 0)
        //        {
        //            return;
        //        }

        //        try
        //        {
        //            BeginInvoke(new Action(() =>
        //            {
        //                Interlocked.Exchange(
        //                    ref _summaryUiDispatchPending,
        //                    0);

        //                StartSummaryUpdateTimer();
        //            }));
        //        }
        //        catch
        //        {
        //            Interlocked.Exchange(
        //                ref _summaryUiDispatchPending,
        //                0);
        //        }

        //        return;
        //    }

        //    StartSummaryUpdateTimer();
        //}

        //private void EnsureSummaryUpdateTimer()
        //{
        //    if (_summaryUpdateTimer != null)
        //        return;

        //    _summaryUpdateTimer =
        //        new System.Windows.Forms.Timer
        //        {
        //            Interval =
        //                SummaryUpdateIntervalMs
        //        };

        //    _summaryUpdateTimer.Tick +=
        //        SummaryUpdateTimer_Tick;

        //    components?.Add(
        //        _summaryUpdateTimer);
        //}

        //private void StartSummaryUpdateTimer()
        //{
        //    if (_tblSummary == null ||
        //        _tblSummary.IsDisposed ||
        //        IsDisposed)
        //    {
        //        return;
        //    }

        //    EnsureSummaryUpdateTimer();

        //    if (!_summaryUpdateTimer.Enabled)
        //    {
        //        _summaryUpdateTimer.Start();
        //    }
        //}

        //private void SummaryUpdateTimer_Tick(
        //    object sender,
        //    EventArgs e)
        //{
        //    if (_tblSummary == null ||
        //        _tblSummary.IsDisposed ||
        //        IsDisposed)
        //    {
        //        _summaryUpdateTimer.Stop();
        //        return;
        //    }

        //    if (Interlocked.Exchange(
        //        ref _summaryUpdatePending,
        //        0) == 0)
        //    {
        //        _summaryUpdateTimer.Stop();
        //        return;
        //    }

        //    ApplySummaryTableValues();
        //}

        //private void ApplySummaryTableValues()
        //{
        //    if (_tblSummary == null ||
        //        _tblSummary.IsDisposed)
        //    {
        //        return;
        //    }

        //    int total =
        //        Math.Max(0, TotalChecked);

        //    int qrPass =
        //        Math.Max(
        //            0,
        //            NumberOfCheckPassed);

        //    int qrFail =
        //        Math.Max(
        //            0,
        //            NumberOfCheckFailed);

        //    int datePass =
        //        Math.Max(
        //            0,
        //            _DateCheckPassed);

        //    int dateFail =
        //        Math.Max(
        //            0,
        //            _DateCheckFailed);

        //    int dateTotal =
        //        datePass + dateFail;

        //    // AutoSize của các Label đều là false,
        //    // nên thay Text không làm thay đổi layout bảng.
        //    // Chỉ Label có dữ liệu mới tự repaint.
        //    SetLabelTextIfChanged(
        //        _summaryTotalLabel,
        //        total.ToString("N0"));

        //    SetLabelTextIfChanged(
        //        _summaryQrPassLabel,
        //        qrPass.ToString("N0"));

        //    SetLabelTextIfChanged(
        //        _summaryQrPassRateLabel,
        //        FormatSummaryPercent(
        //            qrPass,
        //            total));

        //    SetLabelTextIfChanged(
        //        _summaryQrFailLabel,
        //        qrFail.ToString("N0"));

        //    SetLabelTextIfChanged(
        //        _summaryQrFailRateLabel,
        //        FormatSummaryPercent(
        //            qrFail,
        //            total));

        //    SetLabelTextIfChanged(
        //        _summaryDatePassLabel,
        //        datePass.ToString("N0"));

        //    SetLabelTextIfChanged(
        //        _summaryDatePassRateLabel,
        //        FormatSummaryPercent(
        //            datePass,
        //            dateTotal));

        //    SetLabelTextIfChanged(
        //        _summaryDateFailLabel,
        //        dateFail.ToString("N0"));

        //    SetLabelTextIfChanged(
        //        _summaryDateFailRateLabel,
        //        FormatSummaryPercent(
        //            dateFail,
        //            dateTotal));
        //}

        //private static bool IsLabelTextDifferent(
        //    Label label,
        //    string value)
        //{
        //    return label != null &&
        //        !label.IsDisposed &&
        //        !string.Equals(
        //            label.Text,
        //            value ?? string.Empty,
        //            StringComparison.Ordinal);
        //}

        //private static void SetLabelTextIfChanged(
        //    Label label,
        //    string value)
        //{
        //    if (label == null ||
        //        label.IsDisposed)
        //    {
        //        return;
        //    }

        //    value = value ?? string.Empty;

        //    if (string.Equals(
        //        label.Text,
        //        value,
        //        StringComparison.Ordinal))
        //    {
        //        return;
        //    }

        //    label.Text = value;

        //    // Chỉ yêu cầu vẽ lại ô vừa đổi.
        //    // Không Invalidate(true) toàn bộ TableLayoutPanel.
        //    label.Invalidate();
        //}

        //private static string FormatSummaryPercent(
        //    int value,
        //    int total)
        //{
        //    if (total <= 0)
        //        return "0.00%";

        //    double percent =
        //        value * 100.0 / total;

        //    return percent.ToString(
        //        "F2",
        //        CultureInfo.CurrentCulture) + "%";
        //}

        //private static void EnableDoubleBuffering(
        //    Control control)
        //{
        //    if (control == null)
        //        return;

        //    try
        //    {
        //        System.Reflection.PropertyInfo property =
        //            typeof(Control).GetProperty(
        //                "DoubleBuffered",
        //                System.Reflection.BindingFlags.Instance |
        //                System.Reflection.BindingFlags.NonPublic);

        //        property?.SetValue(
        //            control,
        //            true,
        //            null);
        //    }
        //    catch
        //    {
        //    }
        //}

        //private sealed class BufferedTableLayoutPanel
        //    : TableLayoutPanel
        //{
        //    private Color _borderColor =
        //        Color.FromArgb(0, 171, 230);
        //        //Color.FromArgb(0, 174, 239);

        //    private int _borderThickness = 0;
        //    private int _cornerRadius = 0;

        //    public Color BorderColor
        //    {
        //        get => _borderColor;
        //        set
        //        {
        //            _borderColor = value;
        //            Invalidate();
        //        }
        //    }

        //    public int BorderThickness
        //    {
        //        get => _borderThickness;
        //        set
        //        {
        //            _borderThickness =
        //                Math.Max(0, value);

        //            Invalidate();
        //        }
        //    }

        //    public int CornerRadius
        //    {
        //        get => _cornerRadius;
        //        set
        //        {
        //            _cornerRadius =
        //                Math.Max(0, value);

        //            UpdateRoundedRegion();
        //            Invalidate();
        //        }
        //    }

        //    public BufferedTableLayoutPanel()
        //    {
        //        DoubleBuffered = true;
        //        ResizeRedraw = true;

        //        SetStyle(
        //            ControlStyles.AllPaintingInWmPaint |
        //            ControlStyles.OptimizedDoubleBuffer |
        //            ControlStyles.ResizeRedraw,
        //            true);

        //        UpdateStyles();
        //    }

        //    protected override void OnHandleCreated(
        //        EventArgs e)
        //    {
        //        base.OnHandleCreated(e);
        //        UpdateRoundedRegion();
        //    }

        //    protected override void OnResize(
        //        EventArgs e)
        //    {
        //        base.OnResize(e);
        //        UpdateRoundedRegion();
        //    }

        //    protected override void OnPaint(
        //        PaintEventArgs e)
        //    {
        //        base.OnPaint(e);

        //        if (ClientSize.Width <= 1 ||
        //            ClientSize.Height <= 1)
        //        {
        //            return;
        //        }

        //        e.Graphics.SmoothingMode =
        //            System.Drawing.Drawing2D
        //                .SmoothingMode.AntiAlias;

        //        Rectangle borderRectangle =
        //            ClientRectangle;

        //        int inset =
        //            Math.Max(
        //                1,
        //                BorderThickness);

        //        borderRectangle.X +=
        //            inset / 2;

        //        borderRectangle.Y +=
        //            inset / 2;

        //        borderRectangle.Width -=
        //            inset;

        //        borderRectangle.Height -=
        //            inset;

        //        using (
        //            System.Drawing.Drawing2D.GraphicsPath path =
        //                CreateRoundedPath(
        //                    borderRectangle,
        //                    CornerRadius))
        //        using (
        //            Pen borderPen =
        //                new Pen(
        //                    BorderColor,
        //                    BorderThickness))
        //        {
        //            borderPen.Alignment =
        //                System.Drawing.Drawing2D
        //                    .PenAlignment.Inset;

        //            e.Graphics.DrawPath(
        //                borderPen,
        //                path);
        //        }
        //    }

        //    private void UpdateRoundedRegion()
        //    {
        //        if (Width <= 0 ||
        //            Height <= 0 ||
        //            !IsHandleCreated)
        //        {
        //            return;
        //        }

        //        Rectangle rectangle =
        //            new Rectangle(
        //                0,
        //                0,
        //                Width,
        //                Height);

        //        using (
        //            System.Drawing.Drawing2D.GraphicsPath path =
        //                CreateRoundedPath(
        //                    rectangle,
        //                    CornerRadius))
        //        {
        //            Region previousRegion =
        //                Region;

        //            Region =
        //                new Region(path);

        //            previousRegion?.Dispose();
        //        }
        //    }

        //    private static
        //        System.Drawing.Drawing2D.GraphicsPath
        //        CreateRoundedPath(
        //            Rectangle rectangle,
        //            int radius)
        //    {
        //        System.Drawing.Drawing2D.GraphicsPath path =
        //            new System.Drawing.Drawing2D.GraphicsPath();

        //        if (radius <= 1)
        //        {
        //            path.AddRectangle(
        //                rectangle);

        //            path.CloseFigure();
        //            return path;
        //        }

        //        int diameter =
        //            Math.Min(
        //                radius * 2,
        //                Math.Min(
        //                    rectangle.Width,
        //                    rectangle.Height));

        //        Rectangle arc =
        //            new Rectangle(
        //                rectangle.X,
        //                rectangle.Y,
        //                diameter,
        //                diameter);

        //        path.AddArc(
        //            arc,
        //            180,
        //            90);

        //        arc.X =
        //            rectangle.Right -
        //            diameter;

        //        path.AddArc(
        //            arc,
        //            270,
        //            90);

        //        arc.Y =
        //            rectangle.Bottom -
        //            diameter;

        //        path.AddArc(
        //            arc,
        //            0,
        //            90);

        //        arc.X =
        //            rectangle.Left;

        //        path.AddArc(
        //            arc,
        //            90,
        //            90);

        //        path.CloseFigure();
        //        return path;
        //    }
        //}

        //private sealed class BufferedLabel
        //    : Label
        //{
        //    public BufferedLabel()
        //    {
        //        AutoSize = false;
        //        DoubleBuffered = true;

        //        SetStyle(
        //            ControlStyles.AllPaintingInWmPaint |
        //            ControlStyles.OptimizedDoubleBuffer |
        //            ControlStyles.UserPaint,
        //            true);

        //        UpdateStyles();
        //    }
        //}

        //#endregion


        #region UI - Summary table style giống mẫu

        private const int SummaryUpdateIntervalMs = 100;

        private static readonly Color SummaryBorderColor =
            Color.FromArgb(218, 229, 236);

        private static readonly Color SummaryTitleBackColor =
            Color.FromArgb(247, 250, 252);

        private static readonly Color SummarySectionBackColor =
            Color.FromArgb(240, 247, 251);

        private static readonly Color SummaryBlueColor =
            Color.FromArgb(0, 171, 230);

        private static readonly Color SummaryTextColor =
            Color.FromArgb(52, 62, 70);

        private static readonly Color SummaryPassColor =
            Color.FromArgb(49, 177, 105);

        private static readonly Color SummaryFailColor =
            Color.FromArgb(218, 70, 87);

        private RoundedBorderPanel _summaryCard;
        private BufferedTableLayoutPanel _tblSummary;

        private Label _summaryTotalLabel;
        private Label _summaryQrPassLabel;
        private Label _summaryQrPassRateLabel;
        private Label _summaryQrFailLabel;
        private Label _summaryQrFailRateLabel;
        private Label _summaryDatePassLabel;
        private Label _summaryDatePassRateLabel;
        private Label _summaryDateFailLabel;
        private Label _summaryDateFailRateLabel;

        private System.Windows.Forms.Timer _summaryUpdateTimer;

        private int _summaryUpdatePending;
        private int _summaryUiDispatchPending;


        private void label50_Click(object sender, EventArgs e)
        {
            btnPreviewLogPrint_Click(sender, e);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            btnPreviewLogPrint_Click(sender, e);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            BtnPreviewLogCamera_Click(sender, e);
        }

        private void label51_Click(object sender, EventArgs e)
        {
            BtnPreviewLogCamera_Click(sender, e);
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            btnPreviewLogCameraError_Click(sender, e);
        }

        private void label52_Click(object sender, EventArgs e)
        {
            btnPreviewLogCameraError_Click(sender, e);
        }

        private void cuzPanel14_Paint(object sender, PaintEventArgs e)
        {
        }

        private void cuzPanel14_Click(object sender, EventArgs e)
        {
            BtnPreviewLogCamera_Click(sender, e);
        }

        private void cuzPanel16_Click(object sender, EventArgs e)
        {
            btnPreviewLogCameraError_Click(sender, e);
        }

        private void cuzPanel15_Paint(object sender, PaintEventArgs e)
        {

        }


        /// <summary>
        /// Tạo bảng tổng kiểm tra theo layout mẫu:
        /// - 8 hàng
        /// - 3 cột
        /// - chiều cao chia theo Percent để không bị kéo giãn hàng cuối
        /// - QR và DATE có cấu trúc giống nhau
        /// </summary>
        private void CreateSummaryTable(int rowIndex)
        {
            if (_summaryCard != null &&
                !_summaryCard.IsDisposed)
            {
                return;
            }

            EnableDoubleBuffering(cuzPanel23);
            EnableDoubleBuffering(pnlVerificationProcess);
            EnableDoubleBuffering(tableLayoutPanelProcess);

            _summaryCard = new RoundedBorderPanel
            {
                Name = "summaryCard",
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
                Padding = new Padding(1),
                BackColor = SummaryBorderColor,
                BorderColor = SummaryBorderColor,
                BorderThickness = 1,
                CornerRadius = 8
            };

            _tblSummary = new BufferedTableLayoutPanel
            {
                Name = "tblSummary",
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = SummaryBorderColor,
                CornerRadius = 7,

                ColumnCount = 3,
                RowCount = 8,
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // =========================
            // COLUMN
            // =========================
            _tblSummary.ColumnStyles.Clear();

            // Caption | Value | Rate
            _tblSummary.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 35F));

            _tblSummary.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 40F));

            _tblSummary.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 25F));

            // =========================
            // ROW
            // =========================
            _tblSummary.RowStyles.Clear();

            float[] rowPercent =
            {
        12F, // TỔNG KIỂM TRA
        20F, // Tổng số
        10F, // CHECK QR CODE
        11F, // QR đạt
        11F, // QR không đạt
        10F, // CHECK DATE
        13F, // Date đạt
        13F  // Date không đạt
    };

            foreach (float percent in rowPercent)
            {
                _tblSummary.RowStyles.Add(
                    new RowStyle(
                        SizeType.Percent,
                        percent));
            }

            _tblSummary.SuspendLayout();

            // =========================
            // TOTAL
            // =========================
            AddSummaryCell(
                "TỔNG KIỂM TRA",
                0,
                0,
                merge: true,
                role: "title");

            _summaryTotalLabel = AddSummaryCell(
                FormatSummaryNumber(TotalChecked),
                0,
                1,
                merge: true,
                role: "bigvalue");

            // =========================
            // CHECK QR CODE
            // =========================
            AddSummaryCell(
                "CHECK QR CODE",
                0,
                2,
                merge: true,
                role: "sectionQr");

            AddSummaryCell(
                "Đạt",
                0,
                3,
                role: "passcaption");

            _summaryQrPassLabel = AddSummaryCell(
                FormatSummaryNumber(NumberOfCheckPassed),
                1,
                3,
                role: "passvalue");

            _summaryQrPassRateLabel = AddSummaryCell(
                FormatSummaryPercent(
                    NumberOfCheckPassed,
                    TotalChecked),
                2,
                3,
                role: "rate");

            AddSummaryCell(
                "Không đạt",
                0,
                4,
                role: "failcaption");

            _summaryQrFailLabel = AddSummaryCell(
                FormatSummaryNumber(NumberOfCheckFailed),
                1,
                4,
                role: "failvalue");

            _summaryQrFailRateLabel = AddSummaryCell(
                FormatSummaryPercent(
                    NumberOfCheckFailed,
                    TotalChecked),
                2,
                4,
                role: "rate");

            // =========================
            // CHECK DATE
            // =========================
            AddSummaryCell(
                "CHECK DATE",
                0,
                5,
                merge: true,
                role: "sectionDate");

            int dateTotal =
                Math.Max(0, _DateCheckPassed) +
                Math.Max(0, _DateCheckFailed);

            AddSummaryCell(
                "Đạt",
                0,
                6,
                role: "passcaption");

            _summaryDatePassLabel = AddSummaryCell(
                FormatSummaryNumber(_DateCheckPassed),
                1,
                6,
                role: "passvalue");

            _summaryDatePassRateLabel = AddSummaryCell(
                FormatSummaryPercent(
                    _DateCheckPassed,
                    dateTotal),
                2,
                6,
                role: "rate");

            AddSummaryCell(
                "Không đạt",
                0,
                7,
                role: "failcaption");

            _summaryDateFailLabel = AddSummaryCell(
                FormatSummaryNumber(_DateCheckFailed),
                1,
                7,
                role: "failvalue");

            _summaryDateFailRateLabel = AddSummaryCell(
                FormatSummaryPercent(
                    _DateCheckFailed,
                    dateTotal),
                2,
                7,
                role: "rate");

            _tblSummary.ResumeLayout(false);

            _summaryCard.Controls.Add(_tblSummary);

            cuzPanel23.SuspendLayout();
            cuzPanel23.Controls.Clear();
            cuzPanel23.Controls.Add(_summaryCard);
            cuzPanel23.ResumeLayout(true);

            EnsureSummaryUpdateTimer();
        }


        private Label AddSummaryCell(
            string textValue,
            int column,
            int row,
            bool merge = false,
            string role = "normal")
        {
            Label label;

            if (role == "passcaption" ||
                role == "failcaption")
            {
                label = new SummaryStatusLabel
                {
                    IsPassed = role == "passcaption"
                };
            }
            else if (role == "sectionQr" ||
                     role == "sectionDate")
            {
                label = new SummarySectionLabel
                {
                    IconType = role == "sectionQr"
                        ? SummarySectionIcon.QrCode
                        : SummarySectionIcon.Calendar
                };
            }
            else
            {
                label = new BufferedLabel();
            }

            label.AutoSize = false;
            label.Text = textValue ?? string.Empty;
            label.Dock = DockStyle.Fill;
            label.UseCompatibleTextRendering = false;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.BackColor = Color.White;
            label.ForeColor = SummaryTextColor;

            label.Font = new Font(
                "Segoe UI",
                10F,
                FontStyle.Regular,
                GraphicsUnit.Point);

            int rightGap =
                merge || column == 2
                    ? 0
                    : 1;

            int bottomGap =
                row == 7
                    ? 0
                    : 1;

            label.Margin = new Padding(
                0,
                0,
                rightGap,
                bottomGap);

            label.Padding =
                new Padding(12, 0, 12, 0);

            switch (role)
            {
                case "title":
                    label.BackColor =
                        SummaryTitleBackColor;

                    label.ForeColor =
                        SummaryBlueColor;

                    label.Font = new Font(
                        "Segoe UI",
                        24F,
                        FontStyle.Bold,
                        GraphicsUnit.Point);

                    label.TextAlign =
                        ContentAlignment.MiddleCenter;
                    break;

                case "bigvalue":
                    label.BackColor =
                        Color.White;

                    label.ForeColor =
                        Color.Black;

                    label.Font = new Font(
                        "Segoe UI",
                        54F,
                         FontStyle.Bold,
                        GraphicsUnit.Point);

                    label.TextAlign =
                        ContentAlignment.MiddleCenter;
                    break;

                case "sectionQr":
                case "sectionDate":
                    label.BackColor =
                        SummarySectionBackColor;

                    label.ForeColor =
                        SummaryBlueColor;

                    label.Font = new Font(
                        "Segoe UI",
                        18F,
                        FontStyle.Bold,
                        GraphicsUnit.Point);

                    label.TextAlign =
                        ContentAlignment.MiddleLeft;

                    label.Padding =
                        Padding.Empty;
                    break;

                case "passcaption":
                case "failcaption":
                    label.BackColor =
                        Color.White;

                    label.ForeColor =
                        SummaryTextColor;

                    label.Font = new Font(
                        "Segoe UI",
                        24F,
                        FontStyle.Regular,
                        GraphicsUnit.Point);

                    label.TextAlign =
                        ContentAlignment.MiddleLeft;

                    label.Padding =
                        Padding.Empty;
                    break;

                case "passvalue":
                    label.BackColor =
                        Color.White;

                    label.ForeColor =
                        SummaryPassColor;

                    label.Font = new Font(
                        "Segoe UI",
                        24F,
                        FontStyle.Bold,
                        GraphicsUnit.Point);

                    label.TextAlign =
                        ContentAlignment.MiddleRight;

                    label.Padding =
                        new Padding(0, 0, 20, 0);
                    break;

                case "failvalue":
                    label.BackColor =
                        Color.White;

                    label.ForeColor =
                        SummaryFailColor;

                    label.Font = new Font(
                        "Segoe UI",
                        24F,
                        FontStyle.Bold,
                        GraphicsUnit.Point);

                    label.TextAlign =
                        ContentAlignment.MiddleRight;

                    label.Padding =
                        new Padding(0, 0, 20, 0);
                    break;

                case "rate":
                    label.BackColor =
                        Color.White;

                    label.ForeColor =
                        Color.FromArgb(
                            30,
                            30,
                            30);

                    label.Font = new Font(
                        "Segoe UI",
                        24F,
                        FontStyle.Bold,
                        GraphicsUnit.Point);

                    label.TextAlign =
                        ContentAlignment.MiddleRight;

                    label.Padding =
                        new Padding(0, 0, 18, 0);
                    break;
            }

            _tblSummary.Controls.Add(
                label,
                column,
                row);

            if (merge)
            {
                _tblSummary.SetColumnSpan(
                    label,
                    3);
            }

            return label;
        }

        private async void btnAddTons_Click(object sender, EventArgs e)
        {
            await HandleAddTonsAsync();
        }

        private void UpdateSummaryTable()
        {
            if (_tblSummary == null ||
                _tblSummary.IsDisposed ||
                IsDisposed)
            {
                return;
            }

            Interlocked.Exchange(
                ref _summaryUpdatePending,
                1);

            if (!IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                if (Interlocked.Exchange(
                    ref _summaryUiDispatchPending,
                    1) != 0)
                {
                    return;
                }

                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        Interlocked.Exchange(
                            ref _summaryUiDispatchPending,
                            0);

                        StartSummaryUpdateTimer();
                    }));
                }
                catch
                {
                    Interlocked.Exchange(
                        ref _summaryUiDispatchPending,
                        0);
                }

                return;
            }

            StartSummaryUpdateTimer();
        }


        private void EnsureSummaryUpdateTimer()
        {
            if (_summaryUpdateTimer != null)
            {
                return;
            }

            _summaryUpdateTimer =
                new System.Windows.Forms.Timer
                {
                    Interval = SummaryUpdateIntervalMs
                };

            _summaryUpdateTimer.Tick +=
                SummaryUpdateTimer_Tick;

            components?.Add(_summaryUpdateTimer);
        }


        private void StartSummaryUpdateTimer()
        {
            if (_tblSummary == null ||
                _tblSummary.IsDisposed ||
                IsDisposed)
            {
                return;
            }

            EnsureSummaryUpdateTimer();

            if (!_summaryUpdateTimer.Enabled)
            {
                _summaryUpdateTimer.Start();
            }
        }


        private void SummaryUpdateTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (_tblSummary == null ||
                _tblSummary.IsDisposed ||
                IsDisposed)
            {
                _summaryUpdateTimer?.Stop();
                return;
            }

            if (Interlocked.Exchange(
                ref _summaryUpdatePending,
                0) == 0)
            {
                _summaryUpdateTimer.Stop();
                return;
            }

            ApplySummaryTableValues();
        }


        private void ApplySummaryTableValues()
        {
            if (_tblSummary == null ||
                _tblSummary.IsDisposed)
            {
                return;
            }

            int total =
                Math.Max(0, TotalChecked);

            int qrPass =
                Math.Max(0, NumberOfCheckPassed);

            int qrFail =
                Math.Max(0, NumberOfCheckFailed);

            int datePass =
                Math.Max(0, _DateCheckPassed);

            int dateFail =
                Math.Max(0, _DateCheckFailed);

            int dateTotal =
                datePass + dateFail;

            SetLabelTextIfChanged(
                _summaryTotalLabel,
                FormatSummaryNumber(total));

            SetLabelTextIfChanged(
                _summaryQrPassLabel,
                FormatSummaryNumber(qrPass));

            SetLabelTextIfChanged(
                _summaryQrPassRateLabel,
                FormatSummaryPercent(
                    qrPass,
                    total));

            SetLabelTextIfChanged(
                _summaryQrFailLabel,
                FormatSummaryNumber(qrFail));

            SetLabelTextIfChanged(
                _summaryQrFailRateLabel,
                FormatSummaryPercent(
                    qrFail,
                    total));

            SetLabelTextIfChanged(
                _summaryDatePassLabel,
                FormatSummaryNumber(datePass));

            SetLabelTextIfChanged(
                _summaryDatePassRateLabel,
                FormatSummaryPercent(
                    datePass,
                    dateTotal));

            SetLabelTextIfChanged(
                _summaryDateFailLabel,
                FormatSummaryNumber(dateFail));

            SetLabelTextIfChanged(
                _summaryDateFailRateLabel,
                FormatSummaryPercent(
                    dateFail,
                    dateTotal));
        }


        private static void SetLabelTextIfChanged(
            Label label,
            string value)
        {
            if (label == null ||
                label.IsDisposed)
            {
                return;
            }

            value = value ?? string.Empty;

            if (string.Equals(
                label.Text,
                value,
                StringComparison.Ordinal))
            {
                return;
            }

            label.Text = value;
            label.Invalidate();
        }


        private static string FormatSummaryNumber(
            int value)
        {
            return Math.Max(0, value).ToString(
                "#,##0",
                CultureInfo.InvariantCulture);
        }


        private static string FormatSummaryPercent(
            int value,
            int total)
        {
            if (total <= 0)
            {
                return "0,00%";
            }

            double percent =
                Math.Max(0, value) * 100.0 / total;

            return percent.ToString(
                "F2",
                CultureInfo.GetCultureInfo("vi-VN")) + "%";
        }


        private static void EnableDoubleBuffering(
            Control control)
        {
            if (control == null)
            {
                return;
            }

            try
            {
                System.Reflection.PropertyInfo property =
                    typeof(Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic);

                property?.SetValue(
                    control,
                    true,
                    null);
            }
            catch
            {
            }
        }


        private enum SummarySectionIcon
        {
            QrCode,
            Calendar
        }


        private sealed class SummarySectionLabel : Label
        {
            public SummarySectionIcon IconType { get; set; }

            public SummarySectionLabel()
            {
                AutoSize = false;
                DoubleBuffered = true;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw,
                    true);

                UpdateStyles();
            }

            protected override void OnPaint(
                PaintEventArgs e)
            {
                e.Graphics.Clear(BackColor);

                e.Graphics.SmoothingMode =
                    SmoothingMode.AntiAlias;

                const int iconX = 12;

                int iconSize = Math.Min(
                    18,
                    Math.Max(
                        14,
                        Height - 16));

                int iconY =
                    (Height - iconSize) / 2;

                Rectangle iconRect =
                    new Rectangle(
                        iconX,
                        iconY,
                        iconSize,
                        iconSize);

                using (Pen pen = new Pen(
                    ForeColor,
                    1.4F))
                {
                    if (IconType ==
                        SummarySectionIcon.QrCode)
                    {
                        DrawQrIcon(
                            e.Graphics,
                            pen,
                            iconRect);
                    }
                    else
                    {
                        DrawCalendarIcon(
                            e.Graphics,
                            pen,
                            iconRect);
                    }
                }

                Rectangle textRectangle =
                    new Rectangle(
                        iconRect.Right + 10,
                        0,
                        Math.Max(
                            0,
                            Width -
                            iconRect.Right -
                            12),
                        Height);

                TextRenderer.DrawText(
                    e.Graphics,
                    Text ?? string.Empty,
                    Font,
                    textRectangle,
                    ForeColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.EndEllipsis);
            }


            private static void DrawQrIcon(
                Graphics graphics,
                Pen pen,
                Rectangle rectangle)
            {
                int unit =
                    Math.Max(
                        2,
                        rectangle.Width / 5);

                DrawQrFinder(
                    graphics,
                    pen,
                    new Rectangle(
                        rectangle.Left,
                        rectangle.Top,
                        unit * 2,
                        unit * 2));

                DrawQrFinder(
                    graphics,
                    pen,
                    new Rectangle(
                        rectangle.Right - unit * 2,
                        rectangle.Top,
                        unit * 2,
                        unit * 2));

                DrawQrFinder(
                    graphics,
                    pen,
                    new Rectangle(
                        rectangle.Left,
                        rectangle.Bottom - unit * 2,
                        unit * 2,
                        unit * 2));

                using (Brush brush =
                    new SolidBrush(pen.Color))
                {
                    graphics.FillRectangle(
                        brush,
                        rectangle.Left + unit * 3,
                        rectangle.Top + unit * 3,
                        unit,
                        unit);

                    graphics.FillRectangle(
                        brush,
                        rectangle.Left + unit * 2,
                        rectangle.Top + unit * 4,
                        unit,
                        unit);

                    graphics.FillRectangle(
                        brush,
                        rectangle.Left + unit * 4,
                        rectangle.Top + unit * 2,
                        unit,
                        unit);
                }
            }


            private static void DrawQrFinder(
                Graphics graphics,
                Pen pen,
                Rectangle rectangle)
            {
                graphics.DrawRectangle(
                    pen,
                    rectangle);

                int inset =
                    Math.Max(
                        2,
                        rectangle.Width / 3);

                Rectangle inner =
                    Rectangle.Inflate(
                        rectangle,
                        -inset,
                        -inset);

                using (Brush brush =
                    new SolidBrush(pen.Color))
                {
                    graphics.FillRectangle(
                        brush,
                        inner);
                }
            }


            private static void DrawCalendarIcon(
                Graphics graphics,
                Pen pen,
                Rectangle rectangle)
            {
                Rectangle body =
                    new Rectangle(
                        rectangle.Left,
                        rectangle.Top + 2,
                        rectangle.Width - 1,
                        rectangle.Height - 3);

                graphics.DrawRectangle(
                    pen,
                    body);

                graphics.DrawLine(
                    pen,
                    body.Left,
                    body.Top + 4,
                    body.Right,
                    body.Top + 4);

                graphics.DrawLine(
                    pen,
                    body.Left + 4,
                    body.Top - 2,
                    body.Left + 4,
                    body.Top + 3);

                graphics.DrawLine(
                    pen,
                    body.Right - 4,
                    body.Top - 2,
                    body.Right - 4,
                    body.Top + 3);

                using (Brush brush =
                    new SolidBrush(pen.Color))
                {
                    int cell =
                        Math.Max(
                            2,
                            body.Width / 7);

                    for (int row = 0; row < 2; row++)
                    {
                        for (int column = 0;
                             column < 3;
                             column++)
                        {
                            graphics.FillRectangle(
                                brush,
                                body.Left +
                                    3 +
                                    column *
                                    (cell + 2),
                                body.Top +
                                    7 +
                                    row *
                                    (cell + 2),
                                cell,
                                cell);
                        }
                    }
                }
            }
        }


        private sealed class SummaryStatusLabel : Label
        {
            public bool IsPassed { get; set; }

            public SummaryStatusLabel()
            {
                AutoSize = false;
                DoubleBuffered = true;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw,
                    true);

                UpdateStyles();
            }

            protected override void OnPaint(
                PaintEventArgs e)
            {
                e.Graphics.Clear(BackColor);

                e.Graphics.SmoothingMode =
                    SmoothingMode.AntiAlias;

                Color iconColor =
                    IsPassed
                        ? SummaryPassColor
                        : SummaryFailColor;

                int iconSize = Math.Min(
                    18,
                    Math.Max(
                        14,
                        Height - 16));

                int iconX = 12;

                int iconY =
                    (Height - iconSize) / 2;

                Rectangle iconRectangle =
                    new Rectangle(
                        iconX,
                        iconY,
                        iconSize,
                        iconSize);

                using (Brush iconBrush =
                    new SolidBrush(iconColor))
                {
                    e.Graphics.FillEllipse(
                        iconBrush,
                        iconRectangle);
                }

                using (Pen symbolPen =
                    new Pen(
                        Color.White,
                        1.9F))
                {
                    symbolPen.StartCap =
                        LineCap.Round;

                    symbolPen.EndCap =
                        LineCap.Round;

                    if (IsPassed)
                    {
                        PointF p1 =
                            new PointF(
                                iconRectangle.Left +
                                iconRectangle.Width * 0.25F,
                                iconRectangle.Top +
                                iconRectangle.Height * 0.53F);

                        PointF p2 =
                            new PointF(
                                iconRectangle.Left +
                                iconRectangle.Width * 0.43F,
                                iconRectangle.Top +
                                iconRectangle.Height * 0.70F);

                        PointF p3 =
                            new PointF(
                                iconRectangle.Left +
                                iconRectangle.Width * 0.76F,
                                iconRectangle.Top +
                                iconRectangle.Height * 0.32F);

                        e.Graphics.DrawLines(
                            symbolPen,
                            new[]
                            {
                        p1,
                        p2,
                        p3
                            });
                    }
                    else
                    {
                        float inset =
                            iconRectangle.Width *
                            0.31F;

                        e.Graphics.DrawLine(
                            symbolPen,
                            iconRectangle.Left + inset,
                            iconRectangle.Top + inset,
                            iconRectangle.Right - inset,
                            iconRectangle.Bottom - inset);

                        e.Graphics.DrawLine(
                            symbolPen,
                            iconRectangle.Right - inset,
                            iconRectangle.Top + inset,
                            iconRectangle.Left + inset,
                            iconRectangle.Bottom - inset);
                    }
                }

                Rectangle textRectangle =
                    new Rectangle(
                        iconRectangle.Right + 10,
                        0,
                        Math.Max(
                            0,
                            Width -
                            iconRectangle.Right -
                            12),
                        Height);

                TextRenderer.DrawText(
                    e.Graphics,
                    Text ?? string.Empty,
                    Font,
                    textRectangle,
                    ForeColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.EndEllipsis);
            }
        }


        private sealed class RoundedBorderPanel : Panel
        {
            private Color _borderColor =
                SummaryBorderColor;

            private int _borderThickness = 1;
            private int _cornerRadius = 8;

            public Color BorderColor
            {
                get => _borderColor;

                set
                {
                    _borderColor = value;
                    BackColor = value;
                    Invalidate();
                }
            }

            public int BorderThickness
            {
                get => _borderThickness;

                set
                {
                    _borderThickness =
                        Math.Max(
                            1,
                            value);

                    Padding =
                        new Padding(
                            _borderThickness);

                    Invalidate();
                }
            }

            public int CornerRadius
            {
                get => _cornerRadius;

                set
                {
                    _cornerRadius =
                        Math.Max(
                            0,
                            value);

                    UpdateRoundedRegion();
                    Invalidate();
                }
            }


            public RoundedBorderPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                BackColor = SummaryBorderColor;
                Padding = new Padding(1);

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);

                UpdateStyles();
            }


            protected override void OnHandleCreated(
                EventArgs e)
            {
                base.OnHandleCreated(e);
                UpdateRoundedRegion();
            }


            protected override void OnResize(
                EventArgs eventArgs)
            {
                base.OnResize(eventArgs);

                UpdateRoundedRegion();
                Invalidate();
            }


            protected override void OnPaintBackground(
                PaintEventArgs e)
            {
                if (ClientSize.Width <= 0 ||
                    ClientSize.Height <= 0)
                {
                    return;
                }

                e.Graphics.SmoothingMode =
                    SmoothingMode.AntiAlias;

                e.Graphics.PixelOffsetMode =
                    PixelOffsetMode.HighQuality;

                RectangleF rectangle =
                    new RectangleF(
                        0F,
                        0F,
                        ClientSize.Width - 1F,
                        ClientSize.Height - 1F);

                using (GraphicsPath path =
                    CreateRoundedPath(
                        rectangle,
                        CornerRadius))
                using (Brush brush =
                    new SolidBrush(
                        BorderColor))
                {
                    e.Graphics.FillPath(
                        brush,
                        path);
                }
            }


            protected override void OnPaint(
                PaintEventArgs e)
            {
                base.OnPaint(e);

                if (ClientSize.Width <= 2 ||
                    ClientSize.Height <= 2)
                {
                    return;
                }

                e.Graphics.SmoothingMode =
                    SmoothingMode.AntiAlias;

                e.Graphics.PixelOffsetMode =
                    PixelOffsetMode.HighQuality;

                float halfPen =
                    BorderThickness / 2F;

                RectangleF rectangle =
                    new RectangleF(
                        halfPen,
                        halfPen,
                        ClientSize.Width -
                            BorderThickness -
                            1F,
                        ClientSize.Height -
                            BorderThickness -
                            1F);

                using (GraphicsPath path =
                    CreateRoundedPath(
                        rectangle,
                        CornerRadius))
                using (Pen pen =
                    new Pen(
                        BorderColor,
                        BorderThickness))
                {
                    pen.Alignment =
                        PenAlignment.Center;

                    e.Graphics.DrawPath(
                        pen,
                        path);
                }
            }


            private void UpdateRoundedRegion()
            {
                if (!IsHandleCreated || Width < 2 || Height < 2)
                {
                    return;
                }

                RectangleF rectangle =
                    new RectangleF(
                        0F,
                        0F,
                        Width - 1F,
                        Height - 1F);

                using (GraphicsPath path =
                    CreateRoundedPath(
                        rectangle,
                        CornerRadius))
                {
                    Region oldRegion =
                        Region;

                    Region =
                        new Region(path);

                    oldRegion?.Dispose();
                }
            }


            internal static GraphicsPath CreateRoundedPath(
                RectangleF rectangle,
                float radius)
            {
                GraphicsPath path =
                    new GraphicsPath();

                if (radius <= 1F)
                {
                    path.AddRectangle(
                        rectangle);

                    path.CloseFigure();
                    return path;
                }

                float diameter =
                    Math.Min(
                        radius * 2F,
                        Math.Min(
                            rectangle.Width,
                            rectangle.Height));

                RectangleF arc =
                    new RectangleF(
                        rectangle.X,
                        rectangle.Y,
                        diameter,
                        diameter);

                path.AddArc(
                    arc,
                    180F,
                    90F);

                arc.X =
                    rectangle.Right -
                    diameter;

                path.AddArc(
                    arc,
                    270F,
                    90F);

                arc.Y =
                    rectangle.Bottom -
                    diameter;

                path.AddArc(
                    arc,
                    0F,
                    90F);

                arc.X =
                    rectangle.Left;

                path.AddArc(
                    arc,
                    90F,
                    90F);

                path.CloseFigure();
                return path;
            }
        }


        private sealed class BufferedTableLayoutPanel : TableLayoutPanel
        {
            private int _cornerRadius = 7;

            public int CornerRadius
            {
                get => _cornerRadius;

                set
                {
                    _cornerRadius =
                        Math.Max(
                            0,
                            value);

                    UpdateRoundedRegion();
                    Invalidate();
                }
            }


            public BufferedTableLayoutPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);

                UpdateStyles();
            }


            protected override void OnHandleCreated(
                EventArgs e)
            {
                base.OnHandleCreated(e);
                UpdateRoundedRegion();
            }


            protected override void OnResize(
                EventArgs e)
            {
                base.OnResize(e);
                UpdateRoundedRegion();
            }


            private void UpdateRoundedRegion()
            {
                if (!IsHandleCreated ||
                    Width <= 0 ||
                    Height <= 0)
                {
                    return;
                }

                RectangleF rectangle =
                    new RectangleF(
                        0F,
                        0F,
                        Width - 1F,
                        Height - 1F);

                using (GraphicsPath path =
                    RoundedBorderPanel.CreateRoundedPath(
                        rectangle,
                        CornerRadius))
                {
                    Region oldRegion =
                        Region;

                    Region =
                        new Region(path);

                    oldRegion?.Dispose();
                }
            }
        }


        private sealed class BufferedLabel : Label
        {
            public BufferedLabel()
            {
                AutoSize = false;
                DoubleBuffered = true;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw,
                    true);

                UpdateStyles();
            }
        }

        #endregion


#if DEBUG
        #region DEBUG: Camera Simulation Controls

        private void InitializeDebugControls()
        {
            _pnlDebug = new System.Windows.Forms.Panel
            {
                Location = new System.Drawing.Point(10, 110),
                Size = new System.Drawing.Size(750, 105),
                BackColor = System.Drawing.Color.LightYellow,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                Visible = true
            };

            int x = 5;
            _btnCameraSim = new System.Windows.Forms.Button
            {
                Text = "Camera Sim: OFF",
                Width = 110,
                Height = 25,
                Location = new System.Drawing.Point(x, 5),
                BackColor = System.Drawing.Color.LightGray
            };
            _btnCameraSim.Click += (s, e) =>
            {
                bool on = _btnCameraSim.Tag?.ToString() == "ON";
                if (!on)
                {
                    _btnCameraSim.Text = "Camera Sim: ON";
                    _btnCameraSim.BackColor = System.Drawing.Color.LightGreen;
                    _btnCameraSim.Tag = "ON";
                    _numSimSpeed.Visible = true;
                    _btnVirtualPrinter.Visible = true;
                    _btnSimStartStop.Visible = true;
                    _txtSimInput.Visible = true;
                    _btnSimSend.Visible = true;
                    _txtCameraPrograms.Visible = true;
                    _btnLoadPrograms.Visible = true;
                    _btnClearPrograms.Visible = true;
                }
                else
                {
                    StopCameraSimulation();
                    _btnCameraSim.Text = "Camera Sim: OFF";
                    _btnCameraSim.BackColor = System.Drawing.Color.LightGray;
                    _btnCameraSim.Tag = "OFF";
                    _numSimSpeed.Visible = false;
                    _btnVirtualPrinter.Visible = false;
                    _btnSimStartStop.Visible = false;
                    _txtSimInput.Visible = false;
                    _btnSimSend.Visible = false;
                    _txtCameraPrograms.Visible = false;
                    _btnLoadPrograms.Visible = false;
                    _btnClearPrograms.Visible = false;
                }
                UpdateSimStatusLabel();
            };
            x += 115;

            _numSimSpeed = new System.Windows.Forms.NumericUpDown
            {
                Minimum = 1,
                Maximum = 50,
                Value = 10,
                Width = 60,
                Height = 25,
                Location = new System.Drawing.Point(x, 5),
                Visible = false
            };
            _numSimSpeed.ValueChanged += (s, e) => UpdateSimStatusLabel();
            x += 65;

            _btnVirtualPrinter = new System.Windows.Forms.Button
            {
                Text = "Máy in thật",
                Width = 100,
                Height = 25,
                Location = new System.Drawing.Point(x, 5),
                BackColor = System.Drawing.Color.White,
                Visible = false
            };
            _btnVirtualPrinter.Click += (s, e) =>
            {
                bool on = _btnVirtualPrinter.Tag?.ToString() == "ON";
                if (!on)
                {
                    _btnVirtualPrinter.Text = "Virtual Printer";
                    _btnVirtualPrinter.BackColor = System.Drawing.Color.LightBlue;
                    _btnVirtualPrinter.Tag = "ON";
                }
                else
                {
                    _btnVirtualPrinter.Text = "Máy in thật";
                    _btnVirtualPrinter.BackColor = System.Drawing.Color.White;
                    _btnVirtualPrinter.Tag = "OFF";
                }
                UpdateSimStatusLabel();
            };
            x += 105;

            _btnSimStartStop = new System.Windows.Forms.Button
            {
                Text = "Start Sim",
                Width = 80,
                Height = 25,
                Location = new System.Drawing.Point(x, 5),
                BackColor = System.Drawing.Color.LightGreen,
                Visible = false
            };
            _btnSimStartStop.Click += (s, e) =>
            {
                if (_simCTS == null)
                {
                    _simCTS = new System.Threading.CancellationTokenSource();
                    _simIndex = 0;
                    System.Threading.Tasks.Task.Run(() => RunCameraSimulation(_simCTS.Token));
                    _btnSimStartStop.Text = "Stop Sim";
                    _btnSimStartStop.BackColor = System.Drawing.Color.Red;
                }
                else
                {
                    StopCameraSimulation();
                    _btnSimStartStop.Text = "Start Sim";
                    _btnSimStartStop.BackColor = System.Drawing.Color.LightGreen;
                }
                UpdateSimStatusLabel();
            };
            x += 85;

            _lblSimStatus = new System.Windows.Forms.Label
            {
                Text = "Sim: OFF",
                Width = 250,
                Height = 20,
                Location = new System.Drawing.Point(x, 8)
            };

            // ── Row 2: Raw frame input ──
            _txtSimInput = new System.Windows.Forms.TextBox
            {
                Text = "https://beta.ndatrace.vn/01/8935217401758/21/testMNDHe4,true,NSX:19/07/2026,11:48,HSD:19/03/2027,A16,true,20260718_115800_1",
                Width = 600,
                Height = 25,
                Location = new System.Drawing.Point(5, 40),
                Visible = false
            };

            _btnSimSend = new System.Windows.Forms.Button
            {
                Text = "Send",
                Width = 60,
                Height = 25,
                Location = new System.Drawing.Point(610, 40),
                BackColor = System.Drawing.Color.LightBlue,
                Visible = false
            };
            _btnSimSend.Click += (s, e) =>
            {
                string raw = _txtSimInput.Text?.Trim();
                if (!string.IsNullOrEmpty(raw))
                {
                    var detectModel = ParseRawFrameToDetectModel(raw);
                    if (detectModel != null)
                    {
                        _QueueBufferDataObtained.Enqueue(detectModel);
                        Debug.WriteLine($"[SimSend] Enqueued: qr='{detectModel.Text}'");
                    }
                }
            };

            // ── Row 3: Camera Programs (DEBUG) ──
            _txtCameraPrograms = new System.Windows.Forms.TextBox
            {
                Text = "0001_8935217401130,0002_8935217401758,0003_8935217401765",
                Width = 500,
                Height = 25,
                Location = new System.Drawing.Point(5, 75),
                Visible = false
            };

            _btnLoadPrograms = new System.Windows.Forms.Button
            {
                Text = "Load Programs",
                Width = 100,
                Height = 25,
                Location = new System.Drawing.Point(510, 75),
                BackColor = System.Drawing.Color.LightGreen,
                Visible = false
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
                Debug.WriteLine($"[Debug] Loaded {programs.Count} camera programs: {string.Join(", ", programs)}");
            };

            _btnClearPrograms = new System.Windows.Forms.Button
            {
                Text = "Clear",
                Width = 60,
                Height = 25,
                Location = new System.Drawing.Point(615, 75),
                BackColor = System.Drawing.Color.LightCoral,
                Visible = false
            };
            _btnClearPrograms.Click += (s, e) =>
            {
                Shared.Settings.CachedCameraPrograms = new List<string>();
                Debug.WriteLine("[Debug] Cleared camera programs cache");
            };

            _pnlDebug.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                _btnCameraSim, _numSimSpeed, _btnVirtualPrinter, _btnSimStartStop, _lblSimStatus,
                _txtSimInput, _btnSimSend,
                _txtCameraPrograms, _btnLoadPrograms, _btnClearPrograms
            });

            this.Controls.Add(_pnlDebug);
            _pnlDebug.BringToFront();
        }

        private void UpdateSimStatusLabel()
        {
            bool on = _btnCameraSim.Tag?.ToString() == "ON";
            if (!on)
            {
                _lblSimStatus.Text = "Sim: OFF";
                return;
            }
            int speed = (int)_numSimSpeed.Value;
            bool virtualPrinter = _btnVirtualPrinter.Tag?.ToString() == "ON";
            bool running = _simCTS != null;
            _lblSimStatus.Text = $"Sim: {(running ? "RUNNING" : "IDLE")} | {speed} mã/s | {(virtualPrinter ? "Virtual" : "Real Printer")}";
        }

        private void StopCameraSimulation()
        {
            _simCTS?.Cancel();
            _simCTS = null;
        }

        private void RunCameraSimulation(System.Threading.CancellationToken token)
        {
            int delayMs = 1000 / (int)_numSimSpeed.Value;
            bool virtualPrinter = _btnVirtualPrinter.Tag?.ToString() == "ON";

            while (!token.IsCancellationRequested)
            {
                string rawFrame = null;
                BeginInvoke(new System.Action(() => { rawFrame = _txtSimInput.Text?.Trim(); }));
                System.Threading.Thread.Sleep(10); // wait for BeginInvoke

                DetectModel detectModel = null;

                if (!string.IsNullOrEmpty(rawFrame))
                {
                    // Use raw frame input (user-provided camera string)
                    detectModel = ParseRawFrameToDetectModel(rawFrame);
                }
                else
                {
                    // Fallback: read from database
                    string[] row;
                    lock (_SyncObjCodeList)
                    {
                        if (_simIndex >= _PrintedCodeObtainFromFile.Count)
                        {
                            _simIndex = 0;
                        }
                        row = _PrintedCodeObtainFromFile[_simIndex];
                        _simIndex++;
                    }

                    if (row.Length > 1 && row[1] != "Waiting")
                    {
                        continue;
                    }

                    string qr;
                    if (_SelectedJob.THJobOperatingMode == THTrueMilkOperatingMode.ProductOneQrCode)
                    {
                        qr = row.Length > 2 ? row[2] : "";
                    }
                    else
                    {
                        var compareSb = new System.Text.StringBuilder();
                        foreach (var item in _SelectedJob.PODFormat)
                        {
                            if (item.Type == PODModel.TypePOD.FIELD)
                            {
                                int origIdx = item.Index >= 1 ? item.Index + 1 : item.Index;
                                if (origIdx < row.Length) compareSb.Append(row[origIdx]);
                            }
                            else if (item.Type == PODModel.TypePOD.TEXT)
                            {
                                compareSb.Append(item.Value);
                            }
                        }
                        qr = compareSb.ToString();
                    }

                    Debug.WriteLine($"[Sim] idx={_simIndex} qr='{qr}' status='{(row.Length > 1 ? row[1] : "?")}'");

                    var extra = new Dictionary<string, string>
                    {
                        { "QR_RESULT", "OK" },
                        { "OCR_RESULT", "OK" },
                        { "NSX", row.Length >= 2 ? row[row.Length - 2] : "" },
                        { "HSD", row.Length >= 1 ? row[row.Length - 1] : "" },
                        { "RAW_FRAME", qr }
                    };
                    if (row.Length >= 3 && !string.IsNullOrEmpty(row[row.Length - 3]))
                        extra["TIME"] = row[row.Length - 3];

                    detectModel = new DetectModel
                    {
                        Text = qr,
                        RoleOfCamera = RoleOfStation.ForProduct,
                        Image = new System.Drawing.Bitmap(100, 100),
                        CompareResult = ComparisonResult.Valid,
                        ExtraFields = extra
                    };
                }

                if (detectModel != null)
                {
                    _QueueBufferDataObtained.Enqueue(detectModel);
                    if (virtualPrinter)
                    {
                        _QueueBufferPrinterResponseData.Enqueue(new PODDataModel
                        {
                            Text = $"RSFP;{_simIndex}/{_simIndex + 200};DATA;{detectModel.Text}",
                            RoleOfPrinter = RoleOfStation.ForProduct
                        });
                    }
                }

                if (delayMs > 0)
                    System.Threading.Thread.Sleep(delayMs);
            }

            BeginInvoke(new System.Action(() =>
            {
                _btnSimStartStop.Text = "Start Sim";
                _btnSimStartStop.BackColor = System.Drawing.Color.LightGreen;
                UpdateSimStatusLabel();
            }));
        }

        private DetectModel ParseRawFrameToDetectModel(string rawFrame)
        {
            try
            {
                string[] parts = rawFrame.Split(',');
                string qr = parts[0];
                string qrResult = parts.Length > 1 ? parts[1].Trim() : "true";
                string nsxRaw = parts.Length > 2 ? parts[2].Trim() : "";
                string timeRaw = parts.Length > 3 ? parts[3].Trim() : "";
                string hsdRaw = parts.Length > 4 ? parts[4].Trim() : "";
                string batch = parts.Length > 5 ? parts[5].Trim() : "";
                string ocrResult = parts.Length > 6 ? parts[6].Trim() : "true";
                string imageId = parts.Length > 7 ? parts[7].Trim() : "";

                string nsx = nsxRaw.StartsWith("NSX:", StringComparison.OrdinalIgnoreCase)
                    ? nsxRaw.Substring(4).Trim() : nsxRaw;
                string hsd = hsdRaw.StartsWith("HSD:", StringComparison.OrdinalIgnoreCase)
                    ? hsdRaw.Substring(4).Trim() : hsdRaw;

                var extra = new Dictionary<string, string>
                {
                    { "QR_RESULT", qrResult == "true" || qrResult == "OK" ? "OK" : "NG" },
                    { "OCR_RESULT", ocrResult == "true" || ocrResult == "OK" ? "OK" : "NG" },
                    { "NSX", nsx },
                    { "TIME", timeRaw },
                    { "HSD", hsd },
                    { "BATCH", batch },
                    { "IMAGE_ID", imageId },
                    { "RAW_FRAME", rawFrame }
                };

                return new DetectModel
                {
                    Text = qr,
                    RoleOfCamera = RoleOfStation.ForProduct,
                    Image = new System.Drawing.Bitmap(100, 100),
                    CompareResult = ComparisonResult.Valid,
                    ExtraFields = extra
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ParseRawFrame] Error: {ex.Message}");
                return null;
            }
        }

        #endregion
#endif

        private void txtAddTotalTon_TextChanged(object sender, EventArgs e)
        {
            if (_SelectedJob == null || _SelectedJob.Volume <= 0)
            {
                lblAdditionalCodes.Text = "≈ 0 hộp";
                return;
            }

            string raw = txtAddTotalTon.Text.Trim().Replace(",", ".");
            if (string.IsNullOrWhiteSpace(raw) || !double.TryParse(raw,
                NumberStyles.Any, CultureInfo.InvariantCulture,
                out double tons) || tons <= 0)
            {
                lblAdditionalCodes.Text = "≈ 0 hộp";
                return;
            }

            int additionalCodes = (int)(tons * 1_000_000 / _SelectedJob.Volume);
            lblAdditionalCodes.Text = $"≈ {additionalCodes:N0} hộp";
        }

        private async Task HandleAddTonsAsync()
        {
            if (_SelectedJob == null || Shared.OperStatus != OperationStatus.Stopped)
            {
                CuzAlert.Show("Chỉ thêm được khi hệ thống đã dừng", Alert.enmType.Warning, new Size(500, 100), new Point(Location.X, Location.Y), this.Size, false);
                return;
            }

            var mode = _SelectedJob.THJobOperatingMode;
            bool isMode1Or2 = mode == THTrueMilkOperatingMode.BatchOneQrCode
                           || mode == THTrueMilkOperatingMode.BatchOneQrCodeNoChange
                           || mode == THTrueMilkOperatingMode.AutoRefreshByTime;
            if (!isMode1Or2)
            {
                CuzAlert.Show("Chỉ áp dụng cho Mode 1/2/4", Alert.enmType.Warning, new Size(500, 100), new Point(Location.X, Location.Y), this.Size, false);
                return;
            }

            string raw = txtAddTotalTon.Text.Trim().Replace(",", ".");
            if (string.IsNullOrWhiteSpace(raw) || !double.TryParse(raw,
                NumberStyles.Any, CultureInfo.InvariantCulture,
                out double additionalTons) || additionalTons <= 0)
            {
                CuzAlert.Show("Vui lòng nhập số tấn > 0", Alert.enmType.Warning, new Size(500, 100), new Point(Location.X, Location.Y), this.Size, false);
                return;
            }

            if (_SelectedJob.Volume <= 0)
            {
                CuzAlert.Show("Sản phẩm chưa có dung tích", Alert.enmType.Warning, new Size(500, 100), new Point(Location.X, Location.Y), this.Size, false);
                return;
            }

            int additionalCodes = (int)(additionalTons * 1_000_000 / _SelectedJob.Volume);
            if (additionalCodes <= 0)
            {
                CuzAlert.Show("Số hộp tính ra = 0", Alert.enmType.Warning, new Size(500, 100), new Point(Location.X, Location.Y), this.Size, false);
                return;
            }

            double newTotalTons = _SelectedJob.EstimatedTons + additionalTons;
            double newTotalCodes = _SelectedJob.NumberTotalsCode + additionalCodes;
            if (newTotalCodes >= 4_000_000)
            {
                CuzAlert.Show($"Tổng số hộp ({newTotalCodes:N0}) vượt 3,999,999!", Alert.enmType.Warning, new Size(500, 100), new Point(Location.X, Location.Y), this.Size, false);
                return;
            }

            var confirmResult = CustomMessageBox.Show(
                $"Cộng thêm {additionalTons:N2} tấn ≈ {additionalCodes:N0} hộp?\n"
              + $"Tổng: {newTotalTons:N2} tấn / {newTotalCodes:N0} hộp",
                "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult != DialogResult.Yes) return;

            try
            {
                string csvPath = _SelectedJob.DirectoryDatabase;
                if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
                {
                    CuzAlert.Show("File database không tồn tại", Alert.enmType.Error, new Size(500, 100), new Point(Location.X, Location.Y), this.Size, false);
                    return;
                }

                string qrCode = "";
                string nsx = DateTime.Now.ToString("dd MM yy");
                string hsd = DateTime.Now.AddDays((_SelectedJob.THJobExpiryMonths > 0 ? _SelectedJob.THJobExpiryMonths : 6) - 1).ToString("dd MM yy");

                var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
                if (virtualList != null)
                {
                    qrCode = virtualList.QrCode;
                    if (virtualList.Count > 0)
                    {
                        var firstRow = virtualList[0];
                        if (firstRow.Length > 4)
                        {
                            nsx = firstRow[firstRow.Length - 2];
                            hsd = firstRow[firstRow.Length - 1];
                        }
                    }
                }
                else
                {
                    var list = _PrintedCodeObtainFromFile as List<string[]>;
                    if (list != null && list.Count > 0)
                    {
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            if (list[i].Length > 2 && list[i][1] == "Waiting")
                            {
                                qrCode = list[i][2];
                                if (list[i].Length > 4)
                                {
                                    nsx = list[i][list[i].Length - 2];
                                    hsd = list[i][list[i].Length - 1];
                                }
                                break;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(qrCode))
                {
                    CuzAlert.Show("Không lấy được QR code hiện tại", Alert.enmType.Error, new Size(500, 100), new Point(Location.X, Location.Y), this.Size, false);
                    return;
                }

                var delimiter = csvPath.EndsWith(".csv") ? "," : "\t";
                var newLines = new List<string>();
                for (int i = 0; i < additionalCodes; i++)
                    newLines.Add($"{qrCode}{delimiter}{nsx}{delimiter}{hsd}");

                await Task.Run(() => File.AppendAllLines(csvPath, newLines, Encoding.UTF8));

                ProjectLogger.WriteInfo(
                    $"[AddTons] +{additionalTons:N2}t / {additionalCodes:N0} codes | "
                  + $"QR={qrCode} | total={newTotalTons:N2}t / {newTotalCodes:N0}codes");

                _SelectedJob.NumberTotalsCode = newTotalCodes;
                _SelectedJob.EstimatedTons = newTotalTons;
                _SelectedJob.SaveFile();

                InitDataAsync(_SelectedJob);

                numberOfCode.Text = _SelectedJob.NumberTotalsCode.ToString("N0") + " hộp";
                txtTotalTon.Text = $"{_SelectedJob.EstimatedTons:N2} tấn";
                txtAddTotalTon.Text = "";
                lblAdditionalCodes.Text = "";
                CuzAlert.Show(
                    $"Đã cộng thêm {additionalTons:N2} tấn ≈ {additionalCodes:N0} hộp",
                    Alert.enmType.Info, new Size(500, 100),
                    new Point(Location.X, Location.Y), this.Size, false);
            }
            catch (Exception ex)
            {
                CuzAlert.Show($"Lỗi: {ex.Message}", Alert.enmType.Error, new Size(500, 100), new Point(Location.X, Location.Y), this.Size, false);
                ProjectLogger.WriteError("[AddTons] " + ex.Message, ex);
            }
        }
    }
}