using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Interfaces;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Apis;
using BarcodeVerificationSystem.Model.Apis.Dispatching;
using BarcodeVerificationSystem.Model.Apis.Manufacturing;
using BarcodeVerificationSystem.Model.CaoSuDongNai;
using BarcodeVerificationSystem.Model.CodeGeneration;
using BarcodeVerificationSystem.Model.Droco;
using BarcodeVerificationSystem.Model.Droco.Request;
using BarcodeVerificationSystem.Model.Droco.Response;
using BarcodeVerificationSystem.Model.Payload;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response;
using BarcodeVerificationSystem.Model.UDT;
using BarcodeVerificationSystem.Model.Woka;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Core;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Factories;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services.Droco;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.CodeGeneration;
using BarcodeVerificationSystem.Utils.ExportData;
using BarcodeVerificationSystem.Utils.ExportData.models;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.View.OtherProjects.DrocoUI;
using BarcodeVerificationSystem.View.UtilityForms;
using BarcodeVerificationSystem.View.UtilityForms.DispatchingProcess;
using CommonVariable;
using DesignUI.CuzAlert;
using Force.DeepCloner;
using Google.Protobuf.WellKnownTypes;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using OperationLog.Controller;
using OperationLog.Model;
using Org.BouncyCastle.Asn1.Ocsp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.UniversalAccessibility.Drawing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Threading;
using UILanguage;
using static BarcodeVerificationSystem.Controller.Shared;
using static BarcodeVerificationSystem.Model.SyncDataParams;
using static OfficeOpenXml.ExcelErrorValue;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using OperationCanceledException = System.OperationCanceledException;
using OperationStatus = BarcodeVerificationSystem.Model.OperationStatus;
using Timer = System.Windows.Forms.Timer;

namespace BarcodeVerificationSystem.View.DrocoUI
{
    public partial class FrmMainDroco : Form
    {
        #region VARIABLES DEFINITION
        // === Droco 4-level packaging ===
        // === Excel-based packaging mapping (Droco) ===
        private Dictionary<string, (string BoxQR, string CartonQR)> _gs1PackagingMap
            = new Dictionary<string, (string BoxQR, string CartonQR)>(StringComparer.OrdinalIgnoreCase);
        private bool _isDrocoExcelPackagingMode = false;
        private DrocoPackagingManager _packagingManager;
        private readonly FrmJobDroco _ParentForm = null;
        private JobModel _SelectedJob = new JobModel();
        private bool _IsPrinterDisconnectedNot = false;
        private bool _IsReCheck = false;
        private readonly Timer _TimerDateTime = new Timer();
        private readonly string _DateTimeFormatTicker = "yyyy/MM/dd hh:mm:ss tt";
        private int _TotalCode = 0;
        private int _TotalChecked = 0;
        private int _TotalMissed = 0;
        private int _ReceivedCode = 0;
        private int _NumberPrinted = 0;
        private int _NumberOfCheckPassed = 0;
        private int _NumberOfCheckFailed = 0;
        private int _NumberOfSentPrinter = 0;
        private int _NumberOfDuplicate = 0;
        private readonly int _TotalColumns = 1;
        private readonly int _StartIndex = 1;
        private readonly DrocoApiService apiService = new DrocoApiService();
        //public int TotalChecked { get { return _TotalChecked; } set { _TotalChecked = value; Invoke(new Action(() => { lblTotalCheckedValue.Text = string.Format("{0:N0}", _TotalChecked); })); } }
        //public int NumberOfCheckPassed { get { return _NumberOfCheckPassed; } set { _NumberOfCheckPassed = value; Invoke(new Action(() => { lblCheckResultPassedValue.Text = string.Format("{0:N0}", _NumberOfCheckPassed); })); } }
        //public int NumberOfCheckFailed { get { return _NumberOfCheckFailed; } set { _NumberOfCheckFailed = value; Invoke(new Action(() => { lblCheckResultFailedValue.Text = string.Format("{0:N0}", _NumberOfCheckFailed); })); } }
        //public int NumberPrinted { get { return _NumberPrinted; } set { _NumberPrinted = value; Invoke(new Action(() => { lblPrintedCodeValue.Text = string.Format("{0:N0}", _NumberPrinted); })); } }
        //public int ReceivedCode { get { return _ReceivedCode; } set { _ReceivedCode = value; Invoke(new Action(() => { lblReceivedValue.Text = string.Format("{0:N0}", _ReceivedCode); })); } }


        public int TotalChecked { get { return _TotalChecked; } set { _TotalChecked = value; SafeInvoke(() => { lblTotalCheckedValue.Text = string.Format("{0:N0}", _TotalChecked); }); } }
        public int NumberOfCheckPassed { get { return _NumberOfCheckPassed; } set { _NumberOfCheckPassed = value; SafeInvoke(() => { lblCheckResultPassedValue.Text = string.Format("{0:N0}", _NumberOfCheckPassed); if (txtTotalQrToBox != null) txtTotalQrToBox.Text = $"{_NumberOfCheckPassed:N0} / {_TotalCode:N0}"; }); } }
        public int NumberOfCheckFailed { get { return _NumberOfCheckFailed; } set { _NumberOfCheckFailed = value; SafeInvoke(() => { lblCheckResultFailedValue.Text = string.Format("{0:N0}", _NumberOfCheckFailed); }); } }
        public int NumberPrinted { get { return _NumberPrinted; } set { _NumberPrinted = value; SafeInvoke(() => { lblPrintedCodeValue.Text = string.Format("{0:N0}", _NumberPrinted); }); } }
        public int ReceivedCode { get { return _ReceivedCode; } set { _ReceivedCode = value; SafeInvoke(() => { lblReceivedValue.Text = string.Format("{0:N0}", _ReceivedCode); }); } }



        public static int startIndex = 0;
        public string _PixelToMMX = "";
        public string _PixelToMMY = "";
        public double PixelToMmX = 0;
        public double PixelToMmY = 0;
        private long _SendPodTimeMs;


        // === Tab Tra cứu mã (Check mode) ===
        private bool _isTabCheckMode = false;
        private TabControl _tabControlMain;
        private TabPage _tabPageMain;
        private TabPage _tabPageCheck;
        private RichTextBox _rtbCheckResult;
        private Label _lblLastScanned;

        //public long SendPodTimeMs
        //{
        //    get { return _SendPodTimeMs; }
        //    set
        //    {
        //        if (_SendPodTimeMs != value)
        //        {
        //            _SendPodTimeMs = value;
        //            Invoke(new Action(() =>
        //            {
        //                labelTimeSent.Text = string.Format("({0} ms)", _SendPodTimeMs);
        //            }));
        //        }
        //    }
        //}
        //public int NumberOfSentPrinter
        //{
        //    get
        //    {
        //        return _NumberOfSentPrinter;
        //    }
        //    set
        //    {
        //        _NumberOfSentPrinter = value;
        //        Invoke(new Action(() =>
        //        {
        //            lblSentDataValue.Text = string.Format("{0:N0}", _NumberOfSentPrinter);
        //        }));
        //    }
        //}
        public long SendPodTimeMs
        {
            get { return _SendPodTimeMs; }
            set
            {
                if (_SendPodTimeMs != value)
                {
                    _SendPodTimeMs = value;
                    SafeInvoke(() =>
                    {
                        labelTimeSent.Text = string.Format("({0} ms)", _SendPodTimeMs);
                    });
                }
            }
        }
        public int NumberOfSentPrinter
        {
            get
            {
                return _NumberOfSentPrinter;
            }
            set
            {
                _NumberOfSentPrinter = value;
                SafeInvoke(() =>
                {
                    lblSentDataValue.Text = string.Format("{0:N0}", _NumberOfSentPrinter);
                });
            }
        }
        private readonly int _MaxDatabaseLine = 500;
        private readonly List<ToolStripLabel> _LabelStatusCameraList = new List<ToolStripLabel>();
        private readonly List<ToolStripLabel> _LabelStatusPrinterList = new List<ToolStripLabel>();
        readonly static object _SyncObjCodeList = new object();
        readonly static object _SyncObjCheckedResultList = new object();
        private readonly string _DateTimeFormat = "yyMMddHHmmss";
        private string[] _DatabaseColunms = new string[0];

        private readonly string[] defaultRecord = new string[] { "100000", "data1", "Valid", "Barcode Quality", "False", "100", DateTime.Now.ToString(), "Camera", " " };
        private static readonly string _index = "Index", _resultData = "ResultData", _result = "Result"
                                       , _processingTime = "ProcessingTime",
                                       _dateTime = "DateTime";

        private static readonly string[] _ColumnNames = { _index, _resultData, _result, _processingTime, _dateTime };
        public static readonly int Index_Index = Array.IndexOf(_ColumnNames, _index), Index_ResultData = Array.IndexOf(_ColumnNames, _resultData), Index_Result = Array.IndexOf(_ColumnNames, _result),
                            Index_ProcessingTime = Array.IndexOf(_ColumnNames, _processingTime),
                            Index_DateTime = Array.IndexOf(_ColumnNames, _dateTime);

        //private readonly string[] defaultRecord = new string[] { "100000", "data1","Valid", "Barcode Quality", "False", "100", DateTime.Now.ToString(), "Camera", " " };
        //private static readonly string _index = "Index", _resultData = "ResultData", _result = "Result",
        //                               _barcodeQuality = "CodeQuality", _position = "Position", _processingTime = "ProcessingTime",
        //                               _dateTime = "DateTime", _device = "Device", _sampled = "Sampled";

        //private static readonly string[] _ColumnNames = { _index, _resultData, _result, _barcodeQuality, _position, _processingTime, _dateTime, _device, _sampled };
        //public readonly int Index_Index = Array.IndexOf(_ColumnNames, _index), Index_ResultData = Array.IndexOf(_ColumnNames, _resultData), Index_Result = Array.IndexOf(_ColumnNames, _result),
        //                    Index_BarcodeQuality = Array.IndexOf(_ColumnNames, _barcodeQuality), Index_Position = Array.IndexOf(_ColumnNames, _position), Index_ProcessingTime = Array.IndexOf(_ColumnNames, _processingTime),
        //                    Index_DateTime = Array.IndexOf(_ColumnNames, _dateTime), Index_Device = Array.IndexOf(_ColumnNames, _device), Index_Sampled = Array.IndexOf(_ColumnNames, _sampled);

        private bool _IsAfterProductionMode = false;
        private bool _IsOnProductionMode = false;
        private bool _IsVerifyAndPrintMode = false;
        private bool _IsPrintedWait = false;
        private bool _IsCheckedWait = true;
        private bool _IsPrintedResponse = false;
        private readonly List<InitDataError> _InitDataErrorList = new List<InitDataError>();
        private ComparisonResult _CheckedResult = ComparisonResult.Valid;
        private ComparisonResult _PrintedResult = ComparisonResult.Valid;
        private PrinterStatus _PrinterStatus = PrinterStatus.Null;
        private readonly object _PrintLocker = new object();
        private readonly object _ReceiveLocker = new object();
        private readonly object _CheckLocker = new object();
        private readonly object _PrintedResponseLocker = new object();
        private Thread _ThreadPrinterResponseHandler = null;
        private readonly Queue<string> _QueueBufferPrintedResponse = new Queue<string>();
        public static readonly ConcurrentQueue<DetectModel> _QueueBufferDataObtained = new ConcurrentQueue<DetectModel>();
        public static ConcurrentQueue<string> _QueuePositionDataObtained = new ConcurrentQueue<string>();
        private readonly SynchronizedQueue<DetectModel> _QueueBufferDataObtainedResult = new SynchronizedQueue<DetectModel>();
        private readonly SynchronizedQueue<string> _QueueBufferUpdateUIPrinter = new SynchronizedQueue<string>();
        private readonly SynchronizedQueue<ExportImageModel> _QueueBufferBackupImage = new SynchronizedQueue<ExportImageModel>();
        private readonly SynchronizedQueue<List<string[]>> _QueueBufferBackupPrintedCode = new SynchronizedQueue<List<string[]>>();
        private ManualResetEventSlim _printedFlushComplete = new ManualResetEventSlim(true);
        private ManualResetEventSlim _checkedFlushComplete = new ManualResetEventSlim(true);
        private readonly SynchronizedQueue<List<string[]>> _QueueBufferBackupCheckedResult = new SynchronizedQueue<List<string[]>>();
        private readonly SynchronizedQueue<object> _QueueBufferPrinterResponseData = new SynchronizedQueue<object>();
        private readonly SynchronizedQueue<string[]> _QueueBufferBackupSendLog = new SynchronizedQueue<string[]>();
        private List<string[]> _PrintedCodeObtainFromFile = new List<string[]>();
        private List<string[]> _CheckedResultCodeList = new List<string[]>();
        private readonly ConcurrentDictionary<string, CompareStatus> _CodeListPODFormat = new ConcurrentDictionary<string, CompareStatus>();
        private ConcurrentDictionary<string, int> _Emergency = new ConcurrentDictionary<string, int>();
        private List<string[]> _SentPrintedCodeObtainFromFile = new List<string[]>();
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
        private FrmSettings _FormSettings;
        private FrmViewHistoryProgram _FormViewHistoryProgram;
        private FrmPreviewDatabase _FormPreviewDatabase;
        private FrmCheckedResultDroco _FormCheckedResult;
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

                isFullHD = (Size.Width < 850 || Size.Height < 850) ? false : true;
                //lblTotalCheckedValue.Text = "1000000";
                //lblPrintedCodeValue.Text = "1000000";
                if (!IsFullHD) //Hide control for small Resolution screen
                {
                    this.tableLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                    this.tableLayoutPanel.RowStyles[0].Height = 30F; // Make first row 20% height
                    this.tableLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                    this.tableLayoutPanel.RowStyles[1].Height = 70F; // Make second row 80% height
                    this.tableLayoutPanelCheckedResult.RowCount--;
                    tableLayoutPanelCheckedResult.Controls.Remove(pnlCurrentCheck);
                    prBarCheckPassed.Dock = DockStyle.Fill;
                    //lblTemplatePrint.Visible = txtTemplatePrint.Visible = labelModeCheck.Visible = cuzTextBoxCheckMode.Visible = btnHistory.Visible = btnAccount.Visible = true;
                    lblTemplatePrint.Visible = txtTemplatePrint.Visible = labelModeCheck.Visible = cuzTextBoxCheckMode.Visible = true;
                    pnlVerificationProcess.TitleFont = new Font("Microsoft Sans Serif", 9.0f, FontStyle.Bold);

                    lblFailed.Location = lblPassed.Location = lblTotalChecked.Location = new Point(6, 2);
                    lblPrintedCodeValue.AutoSize = lblReceivedValue.AutoSize = lblSentDataValue.AutoSize = lblCheckResultFailedValue.AutoSize = lblCheckResultPassedValue.AutoSize = lblTotalCheckedValue.AutoSize = false;
                    lblPrintedCodeValue.Width = lblReceivedValue.Width = lblSentDataValue.Width = lblCheckResultFailedValue.Width = lblCheckResultPassedValue.Width = lblTotalCheckedValue.Width = 90;
                    lblCheckResultFailedValue.Font = lblCheckResultPassedValue.Font = lblTotalCheckedValue.Font = new Font("Microsoft Sans Serif", 15.0f, FontStyle.Bold);
                    lblPrintedCodeValue.TextAlign = lblReceivedValue.TextAlign = lblSentDataValue.TextAlign = lblCheckResultFailedValue.TextAlign = lblCheckResultPassedValue.TextAlign = lblTotalCheckedValue.TextAlign = ContentAlignment.MiddleLeft;
                    lblCheckResultFailedValue.Location = lblCheckResultPassedValue.Location = lblTotalCheckedValue.Location = new Point(10, 19);

                    lblPrintedCodeValue.Font = lblReceivedValue.Font = lblSentDataValue.Font = new Font("Microsoft Sans Serif", 15.0f, FontStyle.Regular);
                    lblPrintedCodeValue.Location = lblReceivedValue.Location = lblSentDataValue.Location = new Point(5, 29);
                    lblSentData.Location = lblReceived.Location = lblPrintedCode.Location = new Point(5, 10);
                }
                else //Show control for full Resolution screen
                {

                    if (this.tableLayoutPanelCheckedResult.RowCount <= 1)
                    {
                        this.tableLayoutPanelCheckedResult.RowCount++;
                        tableLayoutPanelCheckedResult.Controls.Add(pnlCurrentCheck);
                        this.tableLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                        this.tableLayoutPanel.RowStyles[0].Height = 50F;
                        this.tableLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                        this.tableLayoutPanel.RowStyles[1].Height = 50F;
                        prBarCheckPassed.Dock = DockStyle.None;
                        prBarCheckPassed.Anchor = AnchorStyles.None;
                        //lblTemplatePrint.Visible = txtTemplatePrint.Visible = labelModeCheck.Visible = cuzTextBoxCheckMode.Visible = btnHistory.Visible = btnAccount.Visible = true;
                        lblTemplatePrint.Visible = txtTemplatePrint.Visible = labelModeCheck.Visible = cuzTextBoxCheckMode.Visible = true;

                        pnlVerificationProcess.TitleFont = new Font("Microsoft Sans Serif", 12.0f, FontStyle.Bold);

                        lblFailed.Location = lblPassed.Location = lblTotalChecked.Location = new Point(13, 6);
                        lblPrintedCodeValue.AutoSize = lblReceivedValue.AutoSize = lblSentDataValue.AutoSize = lblCheckResultFailedValue.AutoSize = lblCheckResultPassedValue.AutoSize = lblTotalCheckedValue.AutoSize = true;
                        lblCheckResultFailedValue.Font = lblCheckResultPassedValue.Font = lblTotalCheckedValue.Font = new Font("Microsoft Sans Serif", 20.25f, FontStyle.Bold);
                        lblPrintedCodeValue.Location = lblReceivedValue.Location = lblCheckResultFailedValue.Location = lblCheckResultPassedValue.Location = lblTotalCheckedValue.Location = new Point(13, 28);
                        lblPrintedCodeValue.Location = lblReceivedValue.Location = lblSentDataValue.Location = new Point(27, 39);
                        lblSentData.Location = lblReceived.Location = lblPrintedCode.Location = new Point(21, 10);

                        lblPrintedCodeValue.Font = lblReceivedValue.Font = lblSentDataValue.Font = new Font("Microsoft Sans Serif", 24f, FontStyle.Regular);
                    }

                    pnlVerificationProcess.Padding = new Padding(3, 12, 3, 3);
                    tableLayoutPanel.RowCount = 2;
                    tableLayoutPanel.ColumnCount = 1;
                    var pad = pnlVerificationProcess.Margin;
                    pad.Right -= isDelProcessPnlMargin ? 11 : 0;
                    pnlVerificationProcess.Margin = pad;
                    isDelProcessPnlMargin = false;
                    tableLayoutPanel.Controls.Add(pnlVerificationProcess, 0, 1);
                    tableLayoutPanel.Controls.Add(tableLayoutPanelCheckedResult, 0, 0);

                    tableLayoutPanel1.RowCount = 2;
                    tableLayoutPanel1.ColumnCount = 1;

                    tableLayoutPanel1.RowStyles[0].Height = _SelectedJob.CompareType == CompareType.Database ?
                         tableLayoutPanel1.Width * 50 / 100 : 0;
                    tableLayoutPanel1.RowStyles[1].Height = tableLayoutPanel1.Width * 50 / 100;



                    if (_SelectedJob.CompareType != CompareType.Database)
                    {
                        tableLayoutPanel1.Controls.Remove(pnlDatabase);
                    }
                    else
                    {
                        tableLayoutPanel1.Controls.Add(pnlDatabase, 0, 0);
                    }
                    tableLayoutPanel1.Controls.Add(pnlCheckedResult, 0, 1);

                    bool isAppearResultTable = ProjectLabel.IsNutrifood && !Shared.Settings.IsManufacturingMode;
                    if (isAppearResultTable)
                    {
                        tableLayoutPanel1.RowStyles[1].Height = 0;
                        tableLayoutPanel1.Controls.Remove(pnlCheckedResult);

                        tableLayoutPanel1.RowStyles[0].Height = tableLayoutPanel1.Width * 50 / 100;
                    }
                }
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
        private bool dialogResultStopExist;

        // For Combine Camera Result
        private DetectModel _textOnlyData = null;
        private DetectModel _imageData = null;
        private CancellationTokenSource _delayCancellationTokenSource;
        private int _firstCount = 0;
        private bool _isImage;
        string PrintedVerified = "Printed-Verified";
        string UnprintedVerified = "Unprinted-Verified";
        string PrintedDuplicate = "Printed-Duplicate";
        string PrintedUnverified = "Printed-Unverified";
        string UnprintedUnverified = "Unprinted-Unverified";
        string UnprintedChecked = "Unprinted-Checked";
        string countMaster = "";
        string countSlave = "";
        private Image _nextImage;
        private bool _isUpdatePending;
        private int _numberPrev = 0;
        static string pathSendLog, pathRSFPLog;
        private ConcurrentQueue<int> _queueCountFeedback = new ConcurrentQueue<int>();
        private readonly SynchronizedQueue<string[]> _QueueBufferBackupRSFPLog = new SynchronizedQueue<string[]>();
        int countFormStopSuddenly = 0;
        private bool IsCloseButtonAction = true;
        private readonly object lockObject = new object();
        public int CountFeedback { get; set; }
        private int _countFb;
        int CountDataRev;
        AutoTriggerCameraDataman _autoTrigger;
        #endregion

        public FrmMainDroco()
        {
            InitializeComponent();
        }
        private bool isDragging = false;
        private Point dragStartPoint;
        public FrmMainDroco(FrmJobDroco parentForm)
        {
            InitializeComponent();
            FormClosing += FrmMain_FormClosing;
            FormClosed += FrmMain_FormClosed;
            _ParentForm = parentForm;
            //Shared.CurrentJob = parentForm._JobModel;
            //MessageBox.Show("Shared.CurrentJob" + Shared.CurrentJob.FileName);
            WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            Shared.OnNumberEventISCount += Shared_OnNumberEventISCountAsync;
            //StartWebSocketServer("http://localhost:3333/ws/");
            //MessageBox.Show(Shared.LoggedInUser.Role.ToString());
            RegisterExportEvent();

        }

        FilterChecked filterChecked = new FilterChecked(null, null, null, null);
        List<(bool IsChecked, string Text, string orgText)> PODControl = new List<(bool IsChecked, string Text, string orgText)>();
        CustomStatusValue customStatusValue = new CustomStatusValue();
        ExportMode mode = new ExportMode();
        string checkedResultCode = "";
        List<string> statusDb = new List<string>();
        OtherHeader otherHeader = new OtherHeader();
        List<string> checkedResult = new List<string>();
        string deviceFilterCheckRes = "";
        string sampleFilterCheckRes = "";
        CheckedExportResult checkedExportResult = new CheckedExportResult();
        CheckedExportHeader CheckedExportHeader = new CheckedExportHeader();
        CheckedHeaderList CheckedHeaderList = new CheckedHeaderList();

        private void RegisterExportEvent()
        {
            ExportSharedEvents.FilterEvent += ExportSharedEvents_FilterEvent;
            ExportSharedEvents.ExportAllProgressEvent += ExportSharedEvents_ExportAllProgressEvent;
            ExportSharedEvents.CustomStatusEvent += ExportSharedEvents_CustomStatusEvent;
            ExportSharedEvents.ExportModeEvent += ExportSharedEvents_ExportModeEvent;
            ExportSharedEvents.CheckedResultListEvent += ExportSharedEvents_CheckedResultListEvent;
            ExportSharedEvents.SendMultiStatusDbEvent += ExportSharedEventsSendMultiStatusDbEvent;
            ExportSharedEvents.CustomHeaderDbEvent += ExportSharedEvents_CustomHeaderDbEvent;
            ExportSharedEvents.CheckedResultEvent += ExportSharedEvents_CheckedResultEvent;
            ExportSharedEvents.DeviceFilterCheckResEvent += ExportSharedEvents_DeviceFilterCheckResEvent;
            ExportSharedEvents.SampleFilterEvent += ExportSharedEvents_SampleFilterEvent;
            ExportSharedEvents.CheckedResultLbEvent += ExportSharedEvents_CheckedResultLbEvent;
            ExportSharedEvents.HeaderCheckedEvent += ExportSharedEvents_HeaderCheckedEvent;
            ExportSharedEvents.CheckedHeaderListEvent += ExportSharedEvents_CheckedHeaderListEvent;
            ExportSharedEvents.ExportTemplateEvent += ExportSharedEvents_ExportTemplateEvent;
        }

        private void ExportSharedEvents_ExportTemplateEvent(object sender, ExportDataEventArgs e)
        {
            ExportCustomAll();
        }

        private void ExportSharedEvents_CheckedHeaderListEvent(object sender, ExportDataEventArgs e)
        {
            CheckedHeaderList = (CheckedHeaderList)e.Data;
        }

        private void ExportSharedEvents_HeaderCheckedEvent(object sender, ExportDataEventArgs e)
        {
            CheckedExportHeader = (CheckedExportHeader)e.Data;
        }

        private void ExportSharedEvents_CheckedResultLbEvent(object sender, ExportDataEventArgs e)
        {
            checkedExportResult = (CheckedExportResult)e.Data;
        }

        private void ExportSharedEvents_SampleFilterEvent(object sender, ExportDataEventArgs e)
        {
            sampleFilterCheckRes = (string)e.Data;
        }

        private void ExportSharedEvents_DeviceFilterCheckResEvent(object sender, ExportDataEventArgs e)
        {
            deviceFilterCheckRes = (string)e.Data;
        }

        private void ExportSharedEvents_CheckedResultEvent(object sender, ExportDataEventArgs e)
        {
            checkedResult = (List<string>)e.Data;
        }

        private void ExportSharedEvents_CustomHeaderDbEvent(object sender, ExportDataEventArgs e)
        {
            otherHeader = (OtherHeader)e.Data;
        }

        private void ExportSharedEventsSendMultiStatusDbEvent(object sender, ExportDataEventArgs e)
        {
            statusDb = (List<string>)e.Data;
        }

        private void ExportSharedEvents_CheckedResultListEvent(object sender, ExportDataEventArgs e)
        {
            checkedResultCode = (string)e.Data;
        }

        private void ExportSharedEvents_ExportModeEvent(object sender, ExportDataEventArgs e)
        {
            mode = (ExportMode)e.Data;
        }

        private void ExportSharedEvents_CustomStatusEvent(object sender, ExportDataEventArgs e)
        {
            customStatusValue = (CustomStatusValue)e.Data;
        }

        private void ExportSharedEvents_ExportAllProgressEvent(object sender, ExportDataEventArgs e)
        {
            PODControl = (List<(bool IsChecked, string Text, string orgText)>)e.Data;
        }

        private void ExportSharedEvents_FilterEvent(object sender, ExportDataEventArgs e)
        {
            filterChecked = (FilterChecked)e.Data;
        }

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

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {

            _ParentForm.isShowPopupDisConOneTime = false;
            ReleaseResource();
            _ParentForm?.ShowForm();
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 1. Đang chạy → chặn đóng
            if (Shared.OperStatus == OperationStatus.Running || Shared.OperStatus == OperationStatus.Processing)
            {
                CustomMessageBox.Show("Vui lòng dừng công việc trước khi thoát!", Lang.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            // 2. Không phải hành động từ nút Close (btnJob / btnExit đã set IsCloseButtonAction = false)
            if (!IsCloseButtonAction)
            {
                IsCloseButtonAction = true; // reset cho lần sau
                return;
            }

            // 3. Trạng thái Stopped → không cần hỏi, cho đóng luôn
            if (Shared.OperStatus == OperationStatus.Stopped)
            {
                return;
            }

            // 4. Chưa từng chạy (chưa có trạng thái nào) → hỏi xác nhận
            DialogResult dialogResult = CustomMessageBox.Show(Lang.DoYouWantExitApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                _ParentForm.Close();
            }
            else
            {
                e.Cancel = true;
            }
        }

        #region Inits first
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitControls();
            InitEvents();
          
        }

        //private void UpdatePackagingLabels()
        //{
        //    if (InvokeRequired)
        //    {
        //        Invoke(new Action(() => UpdatePackagingLabels()));
        //        return;
        //    }

        //    JobModel job = Shared.CurrentJob ?? _SelectedJob;
        //    if (job == null) return;
        //    _SelectedJob = job; // giữ đồng bộ

        //    int totalCodes = (int)job.NumberTotalsCode;
        //    int codesPerBox = job.NumberOfCodesInBox > 0 ? job.NumberOfCodesInBox : 1;
        //    int boxesPerCarton = job.NumberOfBoxesInCarton > 0 ? job.NumberOfBoxesInCarton : 1;
        //    int cartonsPerPallet = job.NumberOfCartonsInPallet > 0 ? job.NumberOfCartonsInPallet : 1;

        //    int totalBoxesBySpec = (int)Math.Ceiling((double)totalCodes / codesPerBox);
        //    int totalCartonsBySpec = (int)Math.Ceiling((double)totalBoxesBySpec / boxesPerCarton);
        //    int totalPalletsBySpec = (int)Math.Ceiling((double)totalCartonsBySpec / cartonsPerPallet);

        //    // ── Hộp → Thùng ───────────────────────────────────────────────────
        //    int printedBoxes = job.BoxList?.Count(b => b.sentToPrinter) ?? 0;
        //    int scannedBoxes = job.BoxList?.Count(b => b.IsSent) ?? 0;
        //    txtBoxToCarton.Text = $"{printedBoxes} / {totalBoxesBySpec}  ({scannedBoxes} hộp đã quét)";

        //    // ── Thùng → Pallet ─────────────────────────────────────────────────
        //    int scannedCartons = job.DrocoCartonList?.Count ?? 0;
        //    int cartonsInPallets = 0;
        //    int confirmedCartons = 0;
        //    int confirmedPallets = 0;
        //    if (job.DrocoPalletList != null)
        //    {
        //        cartonsInPallets = job.DrocoPalletList
        //            .Where(p => p.CartonCodes != null)
        //            .Sum(p => p.CartonCodes.Count);
        //        confirmedCartons = job.DrocoPalletList
        //            .Where(p => p.IsSent && p.CartonCodes != null)
        //            .Sum(p => p.CartonCodes.Count);
        //        confirmedPallets = job.DrocoPalletList.Count(p => p.IsSent);
        //    }
        //    txtCartonToPallet.Text = $"{scannedCartons} / {totalCartonsBySpec}  ({cartonsInPallets} thùng đã quét)";

        //    // ── Pallet ────────────────────────────────────────────────────────
        //    int printedPallets = job.DrocoPalletList?.Count(p => p.sentToPrinter) ?? 0;
        //    txtPalletConfirm.Text = $"{printedPallets} / {totalPalletsBySpec}  ({confirmedPallets} pallet đã quét)";
        //}
        private void UpdatePackagingLabels()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePackagingLabels()));
                return;
            }

            JobModel job = Shared.CurrentJob ?? _SelectedJob;
            if (job == null) return;
            _SelectedJob = job;

            int totalCodes = (int)job.NumberTotalsCode;
            int codesPerBox = job.NumberOfCodesInBox > 0 ? job.NumberOfCodesInBox : 1;
            int boxesPerCarton = job.NumberOfBoxesInCarton > 0 ? job.NumberOfBoxesInCarton : 1;
            int cartonsPerPallet = job.NumberOfCartonsInPallet > 0 ? job.NumberOfCartonsInPallet : 1;

            int totalBoxesBySpec = (int)Math.Ceiling((double)totalCodes / codesPerBox);
            int totalCartonsBySpec = (int)Math.Ceiling((double)totalBoxesBySpec / boxesPerCarton);
            int totalPalletsBySpec = (int)Math.Ceiling((double)totalCartonsBySpec / cartonsPerPallet);

            // ── Hộp → Thùng: trái = đã IN, ngoặc = đã QUÉT ──────────────────
            int printedBoxes = job.BoxList?.Count(b => b.sentToPrinter) ?? 0;
            int scannedBoxes = job.BoxList?.Count(b => b.IsSent) ?? 0;
            txtBoxToCarton.Text = $"{printedBoxes} / {totalBoxesBySpec}  ({scannedBoxes} hộp đã quét)";

            // ── Thùng → Pallet: trái = đã IN, ngoặc = đã QUÉT ───────────────
            int printedCartons = job.DrocoCartonList?.Count(c => c.sentToPrinter) ?? 0;
            int scannedCartons = job.DrocoCartonList?.Count(c => c.IsSent) ?? 0;  // ← FIX
            txtCartonToPallet.Text = $"{printedCartons} / {totalCartonsBySpec}  ({scannedCartons} thùng đã quét)";

            // ── Pallet: đã IN, ngoặc = đã QUÉT ──────────────────────────────
            int printedPallets = job.DrocoPalletList?.Count(p => p.sentToPrinter) ?? 0;
            int confirmedPallets = job.DrocoPalletList?.Count(p => p.IsSent) ?? 0;
            txtPalletConfirm.Text = $"{printedPallets} / {totalPalletsBySpec}  ({confirmedPallets} pallet đã quét)";

            // ── Mã đã vào hộp ───────────────────────────────────────────────
            if (lblContQr != null)
            {
                int pending = _packagingManager?.PendingCodesForBoxCount ?? 0;
                lblContQr.Text = $"Mã đã vào hộp: {pending}/{codesPerBox}";
            }
        }

        private string GetStatusText(string qr, bool printed, bool scanned)
        {
            if (printed && scanned) return $"{qr} (Đã quét)";
            if (printed) return $"{qr} (Đã in)";
            if (scanned) return $"{qr} (Đã quét)";
            return qr;
        }


        private void InitControls()
        {
            dgvDatabase.Visible = false;
            dgvCheckedResult.Visible = false;
            picDatabaseLoading.Visible = true;
            picCheckedResultLoading.Visible = true;

            txtStatusResult.ReadOnly = true;
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
            _LabelStatusPrinterList.Add(lblStatusPrinter01);
            UpdateStatusLabelPrinter();
            UpdateStatusLabelZebraPrinter();
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected); // Show icon sensor controller status
            UpdateUISerialDeviceControllerStatus(Shared.IsSerialDeviceConnected);
            UpdateJobInfomationInterface(); // Get Job Infor — sets Shared.CurrentJob BEFORE packaging init
            UpdatedCodeInCartonNumber();
            UpdatedSyncCodeInCartonNumber();
            if (Shared.LoggedInUser != null && Shared.LoggedInUser.Role == 1)
            {
                btnAccount.Visible = false;

            }
            ProcessUserAccess();
            //this.btnRePrintCode.Location = new System.Drawing.Point(13, 810);
         
            //droco — CHỈ KHỞI TẠO 1 LẦN DUY NHẤT
            _packagingManager = new DrocoPackagingManager();
            // === Khôi phục trạng thái đóng gói từ job đã lưu ===

            // Lúc này Shared.CurrentJob đã được set đúng bởi UpdateJobInfomationInterface()
            _packagingManager.OnStateRestored += (s, args) =>
            {
                Invoke(new Action(() =>
                {
                    // Đồng bộ dữ liệu mới nhất
                    _SelectedJob = Shared.CurrentJob;

                    // 1. Load lại danh sách Hộp (Box)
                    cboQRBox.Items.Clear();
                    if (_SelectedJob.BoxList != null)
                    {
                        cboQRBox.Items.AddRange(_SelectedJob.BoxList
                            .Select(p => GetStatusText(p.QrCode, p.sentToPrinter, p.IsSent))
                            .Reverse().ToArray());
                        if (cboQRBox.Items.Count > 0) cboQRBox.SelectedIndex = 0;
                    }


                    // 2. Load lại danh sách Thùng (Carton)
                    cboQRCarton.Items.Clear();
                    if (_SelectedJob.DrocoCartonList != null)
                    {
                        cboQRCarton.Items.AddRange(_SelectedJob.DrocoCartonList
                            .Select(c => GetStatusText(c.QrCode, c.sentToPrinter, c.IsSent))
                            .Reverse().ToArray());
                        if (cboQRCarton.Items.Count > 0) cboQRCarton.SelectedIndex = 0;
                    }

                    // 3. Load lại danh sách Pallet
                    cboQRPallet.Items.Clear();
                    if (_SelectedJob.DrocoPalletList != null)
                    {
                        cboQRPallet.Items.AddRange(_SelectedJob.DrocoPalletList
                            .Select(p => GetStatusText(p.QrCode, p.sentToPrinter, p.IsSent))
                            .Reverse().ToArray());
                        if (cboQRPallet.Items.Count > 0) cboQRPallet.SelectedIndex = 0;
                    }

                    // 4. Kiểm tra và hiển thị cảnh báo trạng thái chờ quét
                    if (_packagingManager.CurrentScanState != DrocoPackagingManager.ScanWaitState.None)
                    {
                        string waitMsg = "";
                        switch (_packagingManager.CurrentScanState)
                        {
                            case DrocoPackagingManager.ScanWaitState.WaitingForBoxScan:
                                waitMsg = $"Hộp {_packagingManager.PendingBoxQrCode} chưa quét xác nhận!";
                                break;
                            case DrocoPackagingManager.ScanWaitState.WaitingForCartonScan:
                                waitMsg = $"Thùng {_packagingManager.PendingCartonQrCode} chưa quét xác nhận!";
                                break;
                            case DrocoPackagingManager.ScanWaitState.WaitingForPalletScan:
                                waitMsg = $"Pallet {_packagingManager.PendingPalletQrCode} chưa quét xác nhận!";
                                break;
                        }

                        if (!string.IsNullOrEmpty(waitMsg))
                        {
                            DesignUI.CuzAlert.CuzAlert.Show(waitMsg, DesignUI.CuzAlert.Alert.enmType.Warning,
                                new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                        }
                    }

                    UpdatePackagingLabels();
                }));
            };

            // Hàm Helper để thống nhất cách hiển thị trạng thái (Nên để ở cấp class để dùng chung)


     
            _packagingManager.RestoreFromJob(_SelectedJob);




            // === QR Hộp được sinh ra khi đủ N mã sản phẩm ===
            _packagingManager.OnBoxReady += (s, args) =>
            {
                Invoke(new Action(() =>
                {
                    // Đồng bộ _SelectedJob với Shared.CurrentJob
                    _SelectedJob = Shared.CurrentJob;

                    var box = _SelectedJob.BoxList?.FirstOrDefault(b => b.QrCode == args.QrCode);
                    if (box != null && !box.sentToPrinter)
                    {
                        box.sentToPrinter = true;
                        //   if (Shared.Settings.ZebraPrinter.PODController.IsConnected())
                        if (Shared.IsZebraPrinterReady())
                        {
                            Shared.PrintZebraDroco(box.QrCode, QRType.Box);
                        }
                        Shared.CurrentJob.SaveFile();
                        Debug.WriteLine($"QR: {box.QrCode}, Length: {box.QrCode.Length}");
                    }

                    // Update ComboBox to show "Đã in"
                    cboQRBox.Items.Clear();
                    if (_SelectedJob.BoxList != null && _SelectedJob.BoxList.Count > 0)
                    {
                        cboQRBox.Items.AddRange(_SelectedJob.BoxList
                            .Select(p => GetStatusText(p.QrCode, p.sentToPrinter, p.IsSent))
                            .Reverse()
                            .ToArray());
                        if (cboQRBox.Items.Count > 0) cboQRBox.SelectedIndex = 0;
                    }

                    DesignUI.CuzAlert.CuzAlert.Show(args.Message, DesignUI.CuzAlert.Alert.enmType.Info,
                        new Size(500, 120), new Point(Location.X, Location.Y), this.Size);

                    // ── Cập nhật nhãn đếm ngay sau khi in ──
                    UpdatePackagingLabels();
                }));
            };

            // === Scanner xác nhận QR Hộp → đếm hộp cho thùng ===
            _packagingManager.OnBoxCompleted += (s, args) =>
            {
                Invoke(new Action(() =>
                {
                    // Đồng bộ _SelectedJob với Shared.CurrentJob (DrocoPackagingManager lưu vào Shared.CurrentJob)
                    _SelectedJob = Shared.CurrentJob;

                    DesignUI.CuzAlert.CuzAlert.Show(args.Message, DesignUI.CuzAlert.Alert.enmType.Success,
                        new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                    UpdatedCodeInCartonNumber();

                    cboQRBox.Items.Clear();
                    if (_SelectedJob.BoxList != null && _SelectedJob.BoxList.Count > 0)
                    {
                        cboQRBox.Items.AddRange(_SelectedJob.BoxList
                            .Select(p =>
                            {
                                if (p.sentToPrinter && p.IsSent)
                                    return $"{p.QrCode} ( Đã quét)";
                                else if (p.sentToPrinter)
                                    return $"{p.QrCode} (Đã in)";
                                else if (p.IsSent)
                                    return $"{p.QrCode} (Đã quét)";
                                else
                                    return p.QrCode;
                            })
                            .Reverse()
                            .ToArray());
                        if (cboQRBox.Items.Count > 0) cboQRBox.SelectedIndex = 0;
                    }

                    UpdatePackagingLabels();
                }));
            };

            // === QR Thùng được sinh ra khi đủ N hộp ===
            _packagingManager.OnCartonReady += (s, args) =>
            {
                Invoke(new Action(() =>
                {
                    // Đồng bộ _SelectedJob với Shared.CurrentJob
                    _SelectedJob = Shared.CurrentJob;

                    var carton = _SelectedJob.DrocoCartonList?.FirstOrDefault(c => c.QrCode == args.QrCode);
                    if (carton != null && !carton.sentToPrinter)
                    {
                        carton.sentToPrinter = true;
                        if (Shared.IsZebraPrinterReady())
                        //  if (Shared.Settings.ZebraPrinter.PODController.IsConnected())
                        {
                            Shared.PrintZebraDroco(carton.QrCode, QRType.Carton);
                        }
                        Shared.CurrentJob.SaveFile();
                    }

                    // Update ComboBox to show "Đã in"
                    cboQRCarton.Items.Clear();
                    if (_SelectedJob.DrocoCartonList != null && _SelectedJob.DrocoCartonList.Count > 0)
                    {
                        cboQRCarton.Items.AddRange(_SelectedJob.DrocoCartonList
                            .Select(c => GetStatusText(c.QrCode, c.sentToPrinter, c.IsSent))
                            .Reverse()
                            .ToArray());
                        if (cboQRCarton.Items.Count > 0) cboQRCarton.SelectedIndex = 0;
                    }

                    DesignUI.CuzAlert.CuzAlert.Show(args.Message, DesignUI.CuzAlert.Alert.enmType.Info,
                         new Size(500, 120), new Point(Location.X, Location.Y), this.Size);

                    // ── Cập nhật nhãn đếm ngay sau khi in thùng ──
                    UpdatePackagingLabels();
                }));
            };

            // === Scanner xác nhận QR Thùng → đếm thùng cho pallet ===
            _packagingManager.OnCartonCompleted += (s, args) =>
            {
                Invoke(new Action(() =>
                {
                    _SelectedJob = Shared.CurrentJob;
                    DesignUI.CuzAlert.CuzAlert.Show(args.Message, DesignUI.CuzAlert.Alert.enmType.Success,
                        new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                    UpdatedCodeInCartonNumber();

                    // Sửa lại đoạn này:
                    cboQRCarton.Items.Clear();
                    if (_SelectedJob.DrocoCartonList != null && _SelectedJob.DrocoCartonList.Count > 0)
                    {
                        cboQRCarton.Items.AddRange(_SelectedJob.DrocoCartonList
                            .Select(c => GetStatusText(c.QrCode, c.sentToPrinter, c.IsSent))
                            .Reverse()
                            .ToArray());
                        if (cboQRCarton.Items.Count > 0) cboQRCarton.SelectedIndex = 0;
                    }

                    UpdatePackagingLabels();
                }));
            };

            // === QR Pallet được sinh ra khi đủ N thùng ===
            _packagingManager.OnPalletReady += (s, args) =>
            {
                Invoke(new Action(() =>
                {
                    // FIX: Đồng bộ _SelectedJob trước khi tìm pallet
                    _SelectedJob = Shared.CurrentJob;

                    var pallet = _SelectedJob.DrocoPalletList?.FirstOrDefault(p => p.QrCode == args.QrCode);
                    if (pallet != null)
                    {
                        // Chỉ in nếu chưa in
                        if (!pallet.sentToPrinter)
                        {
                            pallet.sentToPrinter = true;
                            if (Shared.IsZebraPrinterReady())
                            //   if (Shared.Settings.ZebraPrinter.PODController.IsConnected())
                            {
                                Shared.PrintZebraDroco(pallet.QrCode, QRType.Pallet);
                            }
                            Shared.CurrentJob.SaveFile();
                        }
                    }

                    // Cập nhật ComboBox
                    cboQRPallet.Items.Clear();
                    if (_SelectedJob.DrocoPalletList != null && _SelectedJob.DrocoPalletList.Count > 0)
                    {
                        cboQRPallet.Items.AddRange(_SelectedJob.DrocoPalletList
                            .Select(p => GetStatusText(p.QrCode, p.sentToPrinter, p.IsSent))
                            .Reverse()
                            .ToArray());
                        if (cboQRPallet.Items.Count > 0) cboQRPallet.SelectedIndex = 0;
                    }

                    DesignUI.CuzAlert.CuzAlert.Show(args.Message, DesignUI.CuzAlert.Alert.enmType.Info,
                         new Size(500, 120), new Point(Location.X, Location.Y), this.Size);

                    // FIX: _SelectedJob đã đồng bộ → UpdatePackagingLabels đọc đúng sentToPrinter
                    UpdatePackagingLabels();
                }));
            };

            // === Scanner xác nhận QR Pallet ===
            _packagingManager.OnPalletCompleted += (s, args) =>
            {
                Invoke(new Action(() =>
                {
                    _SelectedJob = Shared.CurrentJob;
                    DesignUI.CuzAlert.CuzAlert.Show(args.Message, DesignUI.CuzAlert.Alert.enmType.Success,
                        new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                    UpdatedCodeInCartonNumber();

                    // Sửa lại đoạn này:
                    cboQRPallet.Items.Clear();
                    if (_SelectedJob.DrocoPalletList != null && _SelectedJob.DrocoPalletList.Count > 0)
                    {
                        cboQRPallet.Items.AddRange(_SelectedJob.DrocoPalletList
                            .Select(p => GetStatusText(p.QrCode, p.sentToPrinter, p.IsSent))
                            .Reverse()
                            .ToArray());
                        if (cboQRPallet.Items.Count > 0) cboQRPallet.SelectedIndex = 0;
                    }

                    UpdatePackagingLabels();
                }));
            };



            // === Quét sai mã → cảnh báo người dùng ===
            _packagingManager.OnScanMismatch += (s, args) =>
            {
                Invoke(new Action(() =>
                {

                    DesignUI.CuzAlert.CuzAlert.Show(args.Message, DesignUI.CuzAlert.Alert.enmType.Error,
                        new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                }));
            };
            //Visible control in Debug mode
            UpdateCheckTotalAndPrintedDatabase();
            // Check and update Complete Job status UI
            UpdateCompleteJobStatusUI();
            // === Khởi tạo TabControl tra cứu mã ===
            //InitCheckModeTab();

        
        }
        //private void InitCheckModeTab()
        //{
        //    if (_tabControlMain != null) return; // tránh khởi tạo lại

        //    // --- Tab 1: nội dung hiện tại (Tổng quan) ---
        //    //_tabPageMain = new TabPage("  Tổng quan  ");
        //    //_tabPageMain.BackColor = Color.FromArgb(245, 250, 255);
        //    //panel2.Controls.Remove(tableLayoutPanel1);
        //    //tableLayoutPanel1.Dock = DockStyle.Fill;
        //    //_tabPageMain.Controls.Add(tableLayoutPanel1);

        //    // --- Tab 2: Tra cứu mã ---
        //    _tabPageCheck = new TabPage("  🔍 Tra cứu mã  ");
        //    _tabPageCheck.BackColor = Color.White;
        //    _tabPageCheck.Padding = new Padding(8);

        //    var lblInstruction = new Label
        //    {
        //        Text = "📡  Scanner đang hoạt động — Quét bất kỳ mã QR nào để tra cứu",
        //        Dock = DockStyle.Top,
        //        Height = 36,
        //        Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Italic),
        //        ForeColor = Color.FromArgb(0, 102, 204),
        //        BackColor = Color.FromArgb(224, 240, 255),
        //        TextAlign = ContentAlignment.MiddleCenter,
        //        Padding = new Padding(0, 6, 0, 6)
        //    };

        //    _lblLastScanned = new Label
        //    {
        //        Text = "Mã vừa quét:  (chưa có)",
        //        Dock = DockStyle.Top,
        //        Height = 32,
        //        Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold),
        //        ForeColor = Color.DimGray,
        //        TextAlign = ContentAlignment.MiddleLeft,
        //        Padding = new Padding(8, 0, 0, 0)
        //    };

        //    var btnClear = new Button
        //    {
        //        Text = "Xóa kết quả",
        //        Dock = DockStyle.Top,
        //        Height = 32,
        //        Font = new Font("Microsoft Sans Serif", 10F),
        //        BackColor = Color.FromArgb(240, 240, 240),
        //        FlatStyle = FlatStyle.Flat,
        //        Cursor = Cursors.Hand
        //    };
        //    btnClear.FlatAppearance.BorderColor = Color.Silver;
        //    btnClear.Click += (s, e) =>
        //    {
        //        if (_rtbCheckResult != null)
        //        {
        //            _rtbCheckResult.Clear();
        //            _rtbCheckResult.Text = "Kết quả tra cứu sẽ hiển thị ở đây...";
        //        }
        //        if (_lblLastScanned != null)
        //            _lblLastScanned.Text = "Mã vừa quét:  (chưa có)";
        //    };

        //    _rtbCheckResult = new RichTextBox
        //    {
        //        Dock = DockStyle.Fill,
        //        Font = new Font("Consolas", 11F),
        //        ReadOnly = true,
        //        BackColor = Color.White,
        //        //BorderStyle = BorderStyle.None,
        //        ScrollBars = RichTextBoxScrollBars.Vertical,
        //        Text = "Kết quả tra cứu sẽ hiển thị ở đây..."
        //    };

        //    // Thêm theo thứ tự ngược (DockStyle.Top chồng từ dưới lên)
        //    _tabPageCheck.Controls.Add(_rtbCheckResult);
        //    _tabPageCheck.Controls.Add(btnClear);
        //    _tabPageCheck.Controls.Add(_lblLastScanned);
        //    _tabPageCheck.Controls.Add(lblInstruction);

        //    // --- TabControl ---
        //    //_tabControlMain = new TabControl();
        //    //_tabControlMain.Dock = DockStyle.Fill;
        //    //_tabControlMain.Font = new Font("Microsoft Sans Serif", 11F);
        //    //_tabControlMain.SelectedIndexChanged += TabControlMain_SelectedIndexChanged;
        //    //_tabControlMain.TabPages.Add(_tabPageMain);
        //    //_tabControlMain.TabPages.Add(_tabPageCheck);

        //    //panel2.Controls.Add(_tabControlMain);
        //}

        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            _isTabCheckMode = (_tabControlMain != null && _tabControlMain.SelectedTab == _tabPageCheck);
            ProjectLogger.WriteInfo(_isTabCheckMode
                ? "[TabCheck] Check mode BẬT — scanner sẵn sàng tra cứu."
                : "[TabCheck] Check mode TẮT — trở về chế độ đóng gói.");
        }
        private void ChangeCheckMode(Checkmode checkMode)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ChangeCheckMode(checkMode)));
                return;
            }
            try
            {
                btnTrigger.Enabled = checkMode == Checkmode.Camera;
                switch (checkMode)
                {
                    case Checkmode.getSampleWithScanner:
                        cuzTextBoxCheckMode.Text = Lang.GetSampleMode;
                        break;
                    case Checkmode.recheckWithScanner:
                        cuzTextBoxCheckMode.Text = Lang.RecheckMode;
                        break;
                    default:
                        cuzTextBoxCheckMode.Text = Lang.Camera;
                        break;
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Event Action
        private void InitEvents()
        {
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
            btnAccount.Click += ActionChanged;
            btnHistory.Click += ActionChanged;
            btnSettings.Click += ActionChanged;
            pnlPrintedCode.Click += ActionChanged;
            pnlCheckFailed.Click += ActionChanged;
            pnlCheckPassed.Click += ActionChanged;
            pnlTotalChecked.Click += ActionChanged;
            btnExportData.Click += ActionChanged;
            btnExportAll.Click += ActionChanged;
            cuzButtonGetSample.Click += GetSampleRaise;
            btnCustomExport.Click += ActionChanged;
            btnPrintCarton.Click += ActionChanged;

            mnManage.Click += ActionChanged;
            mnChangePassword.Click += ActionChanged;
            mnLogOut.Click += ActionChanged;

            Shared.OnSyncDataParameterChange += Shared_OnSyncDataParameterChange;

            Shared.OnQrCodeCartonChange += Shared_OnQrCodeCartonChange;
            Shared.OnCameraStatusChange += Shared_OnCameraStatusChange;
            Shared.OnCameraReadDataChange += Shared_OnCameraReadDataChange;
            Shared.OnCameraPositionDataChange += Shared_OnCameraPositionDataChange;
            Shared.OnSerialDeviceReadDataChange += Shared_OnSerialDeviceReadDataChange;
            Shared.OnPrinterDataChange += Shared_OnPrinterDataChange;
            Shared.OnPrintingStateChange += Shared_OnPrintingStateChange;
            Shared.OnPrinterStatusChange += Shared_OnPrinterStatusChange;
            Shared.OnZebraPrinterStatusChange += Shared_OnZebraPrinterStatusChange;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnSensorControllerChangeEvent += Shared_OnSensorControllerChangeEvent;
            Shared.OnSerialDeviceControllerChangeEvent += Shared_OnSerialDeviceControllerChangeEvent;
            Shared.OnVerifyAndPrindSendDataMethod += Shared_OnVerifyAndPrindSendDataMethod;
            OnReceiveVerifyDataEvent += SendVerifiedDataToPrinter;
            Shared.OnLogError += Shared_OnLogError;

            Resize += (obj, e) =>
            {
                panel1.Width = Size.Width / 6;
                if (Size.Width < 850 || Size.Height < 850)
                {
                    if (IsFullHD)
                    {
                        IsFullHD = false;
                    }
                }
                else if (Size.Width >= 850 || Size.Height >= 850)
                {
                    if (!IsFullHD)
                    {
                        IsFullHD = true;
                    }
                }
                prBarCheckPassed.Height = prBarCheckPassed.Width = (int)(tableLayoutPanelCheckedResult.RowStyles[0].Height * tableLayoutPanelCheckedResult.Height / 100) * 955 / 1000;
            };

            _QueueBufferPrinterResponseData.Clear();
            ReceiveResponseFromPrinterHandlerAsync();

            // Shown: cập nhật labels khi form đã hiển thị (IsHandleCreated = true)
            this.Shown += (s, e) => LoadProgressFromAllValues();
        }

        private void Shared_OnSyncDataParameterChange(object sender, EventArgs e)
        {
            if (sender is SyncDataParams ParamsName)
            {
                switch (ParamsName.DataType)
                {
                    case SyncDataType.CodeInCarton:
                        UpdatedCodeInCartonNumber();
                        break;
                    case SyncDataType.SyncCodeInCarton:
                        UpdatedSyncCodeInCartonNumber();
                        break;
                    default:
                        break;
                }
            }

        }

        private void UpdatedSyncCodeInCartonNumber()
        {
            if (txtTemplatePrint.InvokeRequired)
            {
                txtTemplatePrint.Invoke(new Action(UpdatedSyncCodeInCartonNumber));
                return;
            }

            txtTemplatePrint.Text = $"{GetSyncCodeInCarton()}";

        }

        private void UpdatedCodeInCartonNumber()
        {
            if (txtPODFormat.InvokeRequired)
            {
                txtPODFormat.Invoke(new Action(UpdatedCodeInCartonNumber));
                return;
            }

            txtPODFormat.Text = $"{GetCodeInCarton()}";


        }

        private int GetCodeInCarton()
        {
            try
            {
                char us = '\x1F';
                var codes = SyncDataList.ReadPrintedCodeData(CommVariables.PathSentDataPallet + _SelectedJob.CheckedResultPath, us);
                int successCount = codes.Count(c => c.Length > 4 && string.Equals(c[4], "success", StringComparison.OrdinalIgnoreCase));
                return successCount;
            }
            catch (Exception) { }
            return 0;
        }

        private int GetSyncCodeInCarton()
        {
            try
            {
                char us = '\x1F';
                var codes = SyncDataList.ReadPrintedCodeData(CommVariables.PathSentDataCargo + _SelectedJob.CheckedResultPath, us);
                int successCount = codes.Count(c => c.Length > 4 && string.Equals(c[4], "success", StringComparison.OrdinalIgnoreCase));
                return successCount;
            }
            catch (Exception) { }
            return 0;
        }

        private void Shared_OnQrCodeCartonChange(object sender, EventArgs e)
        {
            var CurrentCarton = Shared.CurrentJob.CartonList.LastOrDefault(x => x.IsSent == false);
            if (CurrentCarton != null)
            {
                if (Shared.IsZebraPrinterReady())
                // if (Shared.Settings.ZebraPrinter.PODController.IsConnected())
                {
                    //Shared.PrintZebra(CurrentCarton.QrCode);
                    Shared.PrintZebraDroco(CurrentCarton.QrCode, QRType.Carton);
                    CurrentCarton.sentToPrinter = true;
                    Shared.CurrentJob.SaveFile();
                }

                UIControlsFuncs.UI(cboQRBox, () =>
                {
                    cboQRBox.Items.Clear();
                    cboQRBox.Items.AddRange(_SelectedJob.CartonList.Select(p => p.sentToPrinter ? $"{p.QrCode} (Đã in)" : p.QrCode).Reverse().ToArray());
                    cboQRBox.SelectedIndex = 0;
                });
            }

        }

        private void GetSampleRaise(object sender, EventArgs e)
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
        }

        private void BtnTrigger_MouseDown(object sender, MouseEventArgs e)
        {
            Shared.RaiseOnCameraTriggerOnChangeEvent();
        }

        private void BtnTrigger_MouseUp(object sender, MouseEventArgs e)
        {
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

        //private void SendVerifiedDataToPrinter(object sender, EventArgs e) // Verify and print 
        //{
        //    string command = "DATA;";
        //    string[] arr = sender as string[];
        //    if (Shared.Settings.VerifyAndPrintBasicSentMethod)
        //        command += arr[1] == null ? Shared.Settings.FailedDataSentToPrinter : arr[1];
        //    else
        //    {
        //        arr = arr.Skip(2).ToArray();
        //        if (Shared.Settings.PrintFieldForVerifyAndPrint.Count() == 0)
        //        {
        //            command += string.Join(Shared.Settings.SplitCharacter.ToString(), arr
        //                .Select(x => x == null ? Shared.Settings.FailedDataSentToPrinter : x));
        //        }
        //        else
        //        {
        //            command += string.Join(Shared.Settings.SplitCharacter.ToString(), Shared.Settings.PrintFieldForVerifyAndPrint
        //                .Where(x => x.Index < arr.Length + 1)
        //                .Select(x => arr[x.Index - 1] == null ? Shared.Settings.FailedDataSentToPrinter : arr[x.Index - 1])
        //                );
        //        }
        //    }

        //    if (podController != null)
        //    {
        //        podController.Send(command);
        //        NumberOfSentPrinter++;
        //    }
        //    else
        //    {
        //        podController = Shared.Settings.PrinterList.Where(p => p.RoleOfPrinter == RoleOfStation.ForProduct).FirstOrDefault().PODController;
        //    }
        //}
    
     private void SendVerifiedDataToPrinter(object sender, EventArgs e)
        {
            string command = "DATA;";
            string[] arr = sender as string[];
            //if (Shared.Settings.VerifyAndPrintBasicSentMethod)
            //    command += arr[1] == null ? Shared.Settings.FailedDataSentToPrinter : arr[1];
            if (Shared.Settings.VerifyAndPrintBasicSentMethod)
                command += ReplaceAllGS1Separators(arr[1] == null ? Shared.Settings.FailedDataSentToPrinter : arr[1]);
            
            else
            {
                arr = arr.Skip(2).ToArray();
                if (Shared.Settings.PrintFieldForVerifyAndPrint.Count() == 0)
                {
                    command += string.Join(Shared.Settings.SplitCharacter.ToString(), arr
                        .Select(x => ReplaceAllGS1Separators(x == null ? Shared.Settings.FailedDataSentToPrinter : x)));
                }
                else
                {
                    command += string.Join(Shared.Settings.SplitCharacter.ToString(), Shared.Settings.PrintFieldForVerifyAndPrint
                        .Where(x => x.Index < arr.Length + 1)
                        .Select(x => ReplaceAllGS1Separators(arr[x.Index - 1] == null ? Shared.Settings.FailedDataSentToPrinter : arr[x.Index - 1]))
                        );
                }
            }

            if (podController != null)
            {
                podController.Send(command);
                NumberOfSentPrinter++;
            }
            else
            {
                podController = Shared.Settings.PrinterList.Where(p => p.RoleOfPrinter == RoleOfStation.ForProduct).FirstOrDefault().PODController;
            }
        }

        // Hàm chuyển đổi tất cả các dạng ký tự đặc biệt về ký tự thực \x1D
        private string ReplaceAllGS1Separators(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return input
                .Replace("\\F", "\x1D")
                .Replace("<0x1D>", "\x1D")
                .Replace("_x001d_", "\x1D")
                .Replace("%1d", "\x1D");
        }

        /// <summary>
        /// Đọc giá trị chuỗi từ ô NPOI, xử lý đúng từng CellType.
        /// Trả về "" cho Blank / Error / Numeric = 0 (ô trống placeholder).
        /// </summary>
        private static string GetNpoiCellString(ICell cell)
        {
            if (cell == null) return "";

            // Với formula cell, dùng cached result type
            CellType effectiveType = cell.CellType == CellType.Formula
                ? cell.CachedFormulaResultType
                : cell.CellType;

            switch (effectiveType)
            {
                case CellType.Blank:
                case CellType.Error:
                    return "";

                case CellType.String:
                    return cell.StringCellValue?.Trim() ?? "";

                case CellType.Numeric:
                    double v = cell.NumericCellValue;
                    // 0 = ô trống placeholder trong Excel Droco → bỏ qua
                    if (v == 0) return "";
                    // Dùng "G" để tránh scientific notation (1E+18 → chuỗi đầy đủ)
                    return v.ToString("G").Trim();

                case CellType.Boolean:
                    return cell.BooleanCellValue ? "TRUE" : "FALSE";

                default:
                    return cell.ToString()?.Trim() ?? "";
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
                    _CheckedResultCodeSet.Add(NormalizeDataForComparison(array[1]));
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

                    // Normalize data for consistent comparison
                    data = NormalizeDataForComparison(data);

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

                //if (e.ColumnIndex != _DatabaseImageIndex)
                //    //e.Value = _PrintedCodeObtainFromFile[correspondingIndex][e.ColumnIndex];
                //    e.Value = e.ColumnIndex != 0 ? MaskData.MaskString(_PrintedCodeObtainFromFile[correspondingIndex][e.ColumnIndex])
                //                                 : _PrintedCodeObtainFromFile[correspondingIndex][e.ColumnIndex];
                if (e.ColumnIndex != _DatabaseImageIndex)
                {
                    // Chỉ chuyển đổi khi hiển thị lên lưới
                    string value = _PrintedCodeObtainFromFile[correspondingIndex][e.ColumnIndex];
                    e.Value = e.ColumnIndex != 0 ? MaskData.MaskString(ReplaceGSCharacters(value)) : ReplaceGSCharacters(value);
                }

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
                            e.Value = Properties.Resources.icons8_in_progress_20px_4; // Dùng icon "đang xử lý" thay vì "done"
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

                //if (e.ColumnIndex != _CheckedResulImageIndex)
                //{
                //    //string value = _CheckedResultCodeList[correspondingIndex][e.ColumnIndex];
                //    string value = e.ColumnIndex == Index_ResultData ? MaskData.MaskString(_CheckedResultCodeList[correspondingIndex][e.ColumnIndex]) : _CheckedResultCodeList[correspondingIndex][e.ColumnIndex];

                //    e.Value = value == "" ? Lang.CannotDetect : value;
                //}
                //if (e.ColumnIndex != _CheckedResulImageIndex)
                //{
                //    string value = e.ColumnIndex == Index_ResultData
                //        ? MaskData.MaskString(ReplaceGSCharacters(_CheckedResultCodeList[correspondingIndex][e.ColumnIndex]))
                //        : ReplaceGSCharacters(_CheckedResultCodeList[correspondingIndex][e.ColumnIndex]);

                //    e.Value = string.IsNullOrEmpty(value) ? Lang.CannotDetect : value;
                //}
      
                if (e.ColumnIndex != _CheckedResulImageIndex)
                {
                    string value = e.ColumnIndex == Index_ResultData
                        ? MaskData.MaskString(ReplaceGSCharacters(_CheckedResultCodeList[correspondingIndex][e.ColumnIndex]))
                        : ReplaceGSCharacters(_CheckedResultCodeList[correspondingIndex][e.ColumnIndex]);

                    e.Value = string.IsNullOrEmpty(value) ? Lang.CannotDetect : value;
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
            if (Shared.GetCameraStatus() == false)
            {
                return CheckCondition.NotConnectCamera;
            }
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

        /// <summary>
        /// Nếu đang bật chế độ Excel QR → mở dialog chọn file và nạp vào cache.
        /// Nếu tắt (tự động) → không làm gì.
        /// </summary>
        /// <summary>
        /// Tự động nạp file SSCC từ job (không hiện dialog).
        /// Cột 0 = QR Hộp, cột 1 = QR Thùng, Pallet tự sinh.
        /// </summary>
        private void InitExcelCacheIfNeeded()
        {
            // Tắt luồng cũ dùng _gs1PackagingMap (Honest mark_UNIT)
            _isDrocoExcelPackagingMode = false;
            _gs1PackagingMap.Clear();

            string ssccPath = _SelectedJob?.DirectoryDatabaseCodeSSCC;

            if (!string.IsNullOrEmpty(ssccPath) && File.Exists(ssccPath))
            {
                try
                {
                    Shared.ExcelCodeCache = new ExcelCodeCache();
                    Shared.ExcelCodeCache.LoadNoHeader(ssccPath); 
                    // SSCC không có dòng tiêu đề
                    // ── Khôi phục vị trí đọc khi resume job cũ ─────
                    // Mỗi lần StartProcess gọi LoadNoHeader → _currentRowIndex về 0.
                    // Phải skip qua số mã đã cấp trước đó để tránh tái sử dụng code cũ
                    // (gây mã hộp = mã thùng hoặc trùng giữa các phiên).
                    var currentJob = Shared.CurrentJob;
                    if (currentJob != null)
                    {
                        int usedBoxCodes = currentJob.BoxList?.Count ?? 0;
                        int usedCartonCodes = currentJob.DrocoCartonList?.Count ?? 0;
                        int skipCount = usedBoxCodes + usedCartonCodes;
                        if (skipCount > 0)
                        {
                            Shared.ExcelCodeCache.Resume(skipCount);
                            ProjectLogger.WriteInfo($"[ExcelCache] Resume: skipped {skipCount} SSCC codes (boxes={usedBoxCodes}, cartons={usedCartonCodes})");
                        }
                    }

                    var mapping = Shared.Settings.DrocoQRFieldMapping ?? new DrocoQRFieldMapping();
                    mapping.UseExcelMode = true;
                    mapping.BoxColumnIndex = 1;     // cột A (1-based) → index 0 = QR Hộp
                    mapping.CartonColumnIndex = 1;  // cột A (1-based) → index 0 = QR Thùng — đọc tuần tự
                    mapping.PalletColumnIndex = -1; // Pallet tự sinh
                    Shared.Settings.DrocoQRFieldMapping = mapping;
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(
                        "Lỗi đọc file SSCC:\n" + ex.Message,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Shared.ExcelCodeCache = null;
                    var mapping = Shared.Settings.DrocoQRFieldMapping ?? new DrocoQRFieldMapping();
                    mapping.UseExcelMode = false;
                    Shared.Settings.DrocoQRFieldMapping = mapping;
                }
            }
            else
            {
                // Không có file SSCC → tắt ExcelMode, sinh mã tự động
                Shared.ExcelCodeCache = null;
                var mapping = Shared.Settings.DrocoQRFieldMapping ?? new DrocoQRFieldMapping();
                mapping.UseExcelMode = false;
                Shared.Settings.DrocoQRFieldMapping = mapping;
            }
        }
        private void SafeInvoke(Action action)
        {
            try
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    Invoke(action);
                }
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }
        private void StartProcess(bool interactOnUI = true)
        {
            // Kiểm tra kết nối máy in Zebra
            if (!Shared.IsZebraPrinterReady())
            {
                CustomMessageBox.Show("Máy in Zebra chưa kết nối. Vui lòng kiểm tra lại kết nối máy in!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //return;
            }
            if (Shared.OperStatus == OperationStatus.Running || Shared.OperStatus == OperationStatus.Processing)  // Avoid start more 1 time
            {
                return;
            }

            if (_SelectedJob.CompleteJobStatus == CompleteJobStatus.Completed)
            {
                CustomMessageBox.Show("Công việc đã xác nhận hoàn thành!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            InitExcelCacheIfNeeded();

            countFormStopSuddenly = 0;
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
            //CheckCondition checkCondition = CheckAllTheConditions();  // Check all condition
            //if (checkCondition != CheckCondition.Success)
            //{
            //    if (interactOnUI)
            //    {
            //        if (checkCondition == CheckCondition.NoJobsSelected)
            //        {
            //            CustomMessageBox.Show(Lang.PleaseSeletedJobForTheSystem, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        if (checkCondition == CheckCondition.NotLoadDatabase)
            //        {
            //            CustomMessageBox.Show(Lang.PleaseCheckTheDatabaseConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else if (checkCondition == CheckCondition.NotLoadTemplate)
            //        {
            //            CustomMessageBox.Show(Lang.PleaseCheckTheTemplate, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else if (checkCondition == CheckCondition.NotConnectCamera)
            //        {
            //            CustomMessageBox.Show(Lang.PleaseCheckTheCameraConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else if (checkCondition == CheckCondition.MissingParameter)
            //        {
            //            CustomMessageBox.Show(Lang.SomeParametersAreMissing, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else if (checkCondition == CheckCondition.NotConnectPrinter)
            //        {
            //            CustomMessageBox.Show(Lang.PleaseCheckThePrinterConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else if (checkCondition == CheckCondition.LeastOneAction)
            //        {
            //            CustomMessageBox.Show(Lang.ThereMustBeAtLeastOneActionSelected, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else if (checkCondition == CheckCondition.MissingParameterActivation)
            //        {
            //            CustomMessageBox.Show(Lang.SomeActivationParametersAreMissing, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else if (checkCondition == CheckCondition.MissingParameterPrinting)
            //        {
            //            CustomMessageBox.Show(Lang.SomePrintParametersAreMissing, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else if (checkCondition == CheckCondition.OCRCameraIsOffline)
            //        {
            //            CustomMessageBox.Show(Lang.OCRCameraIsOffline, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //    }
            //    return;
            //}
            //btnRePrintCode.Visible = false;
            //btnPrintCheckCode.Visible = false;
            //btnPrintCheckCode.Enabled = false;
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
            _QueueBufferPrinterResponseData.Clear();
            //_QueuePositionDataObtained = new ConcurrentQueue<string>();
            _queueCountFeedback = new ConcurrentQueue<int>();
            CountFeedback = 0;
            _numberPrev = 0; // use for detect missed print page

            if (Shared.Settings.IsPrinting && _SelectedJob.CompareType == CompareType.Database && _SelectedJob.PrinterSeries && !_IsReCheck)
            {
                Shared.OperStatus = OperationStatus.Processing;
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
                    string startPrintCommand = string.Format("STAR" + Shared.Settings.SplitCharacter + "{0}" + Shared.Settings.SplitCharacter + 1 + Shared.Settings.SplitCharacter + 1 + Shared.Settings.SplitCharacter + "true", templateName);

                    podController.Send(startPrintCommand);
                }
            }
            else
            {
                Shared.OperStatus = OperationStatus.Running;
            }

            bool isNonePrinted = _SelectedJob.CompareType == CompareType.CanRead || _SelectedJob.CompareType == CompareType.StaticText;
            _SelectedJob.JobStatus = JobStatus.Unfinished;
            _SelectedJob.SaveFile();


            // Fix: đảm bảo job context đúng cho DrocoPackagingManager (dùng Shared.CurrentJob)
            Shared.CurrentJob = _SelectedJob;
            // Fix: luôn tạo mới DrocoAllValueProcess cho job hiện tại, tránh ghi nhầm file job cũ
            Shared.DrocoAllValueProcess = new DrocoAllValueProcess(_SelectedJob);
            // Fix: reset trạng thái packaging manager về đúng job mới
            _packagingManager?.RestoreFromJob(_SelectedJob);


            _OperationCancelTokenSource = new CancellationTokenSource();
            _UICheckedResultCancelTokenSource = new CancellationTokenSource();
            _UIPrintedResponseCancelTokenSource = new CancellationTokenSource();
            _BackupImageCancelTokenSource = new CancellationTokenSource();
            _BackupResponseCancelTokenSource = new CancellationTokenSource();
            _BackupResultCancelTokenSource = new CancellationTokenSource();
            _BackupSendLogCancelTokenSource = new CancellationTokenSource();
            _BackupRSFPLogCancelTokenSource = new CancellationTokenSource();

            var operationToken = _OperationCancelTokenSource.Token;
            var uiCheckedResultToken = _UICheckedResultCancelTokenSource.Token;
            var uiPrintedResponseToken = _UIPrintedResponseCancelTokenSource.Token;
            var backupImageToken = _BackupImageCancelTokenSource.Token;
            var backupResultToken = _BackupResultCancelTokenSource.Token;
            var backupResponseToken = _BackupResponseCancelTokenSource.Token;
            var backupSendLogToken = _BackupSendLogCancelTokenSource.Token;
            var backupRSFPLogToken = _BackupRSFPLogCancelTokenSource.Token;

            BackupSendLogAsync(backupSendLogToken);

            CompareAsync(operationToken);

            if (Shared.Settings.ExportImageEnable)
                ExportImageToFileAsync(backupImageToken);

            ExportCheckedResultToFileAsync(backupResultToken);
            UpdateUICheckedResultAsync(uiCheckedResultToken);

            if (_SelectedJob.CompareType == CompareType.Database)
            {
                ExportPrintedResponseToFileAsync(backupResponseToken);
                UpdateUIPrintedResponseAsync(uiPrintedResponseToken);
                BackupResultFinishPrintCommandAsync(backupRSFPLogToken);
            }

            Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
            EnableUIComponent(Shared.OperStatus);

            //CheckCodeOutOfThreshold(backupResponseToken);
            //SendParametersToServerAsync(uiPrintedResponseToken);
            //AutoTriggerCamera(uiPrintedResponseToken);
            //  _ParentForm.ISCamera.StartGetData(); // Start get data from OCR camera
        }

        private async void AutoTriggerCamera(CancellationToken token)
        {
            await Task.Run(async () => { await Shared_OnCameraTriggerOnChange(token); });

        }

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

            if (_SelectedJob.CompareType == CompareType.StaticText)
            {
                Invoke(new Action(() => staticText = txtStaticText.Text));
            }

            bool isAutoComplete = _SelectedJob.CompareType == CompareType.Database;
            bool isReprint = _SelectedJob.CompareType == CompareType.Database &&
                            _SelectedJob.JobType == JobType.AfterProduction &&
                            _TotalMissed > 0 &&
                            Shared.Settings.TotalCheckEnable;
            bool isDBStandalone = _SelectedJob.CompareType == CompareType.Database &&
                                 _SelectedJob.JobType == JobType.StandAlone;
            int reprintStopCond = TotalChecked + _TotalMissed - _NumberOfDuplicate;
            int stopCond = _TotalCode - _NumberOfDuplicate;
            startIndex = TotalChecked;
            int startCommandIndex = TotalChecked;
            string formattedIndex = startCommandIndex.ToString("D7");

            if (Shared.Settings.CameraList.FirstOrDefault()?.IsIndexCommandEnable == true)
            {
                Shared.SensorController.Send("0" + formattedIndex);
            }

            bool isPosition = Shared.Settings.EnablePosition &&
                             Shared.Settings.CameraList.FirstOrDefault()?.CameraType == CameraType.IS;
            bool isBarcodePosition = Shared.Settings.Position == SettingsModel.PositionType.BarcodePosition;

            //isPosition = true;
            //isBarcodePosition = false;

            // Initialize the appropriate position handler
            IPositionHandler positionHandler = CreatePositionHandler(isPosition, isBarcodePosition);


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

                        //if (completeCondition && !_IsReCheck && !Shared.Settings.AllowDupAndNonStop)
                        //{
                        //    Invoke(new Action(() => StopProcessAsync(false, Lang.CompleteTheBarcodeVerificationProcess, true)));
                        //    isComplete = true;
                        //    continue;
                        //}
                    }

                    // Process queue data using the position handler
                    if (!positionHandler.TryDequeueData(out DetectModel detectModel, out string positionData))
                    {
                        continue;
                    }

                    //if (Shared.Settings.IsItemsPerHour)
                    //{
                    //    currentReceivedTime = GetCurrentTimeInMilliseconds() - lastReceivedTime;
                    //    double rate = (1000.0 / currentReceivedTime) * 3600;
                    //    itemsPerHour.Text = rate.ToString("F0") + " " + Lang.ItemsPerHour;
                    //    lastReceivedTime = GetCurrentTimeInMilliseconds();
                    //}
                    if (Shared.Settings.IsItemsPerHour)
                    {
                        currentReceivedTime = GetCurrentTimeInMilliseconds() - lastReceivedTime;
                        double rate = (1000.0 / currentReceivedTime) * 3600;
                        string text = rate.ToString("F0") + " " + Lang.ItemsPerHour;
                        lastReceivedTime = GetCurrentTimeInMilliseconds();
                    }


                    // Process position data
                    positionHandler.ProcessPositionData(detectModel, positionData);
                    bool isPositionCorrect = detectModel.isBarcodeWithinThreshold.Contains("True");

                    var measureTime = Stopwatch.StartNew();
                    int compareIndex = _StartIndex + TotalChecked;

                    if (detectModel != null)
                    {
                        Console.WriteLine("Du lieu camera co duoc: " + ++CountDataRev);

                        if (_SelectedJob.CompareType == CompareType.CanRead)
                        {
                            detectModel.CompareResult = CanreadCompare(detectModel.Text);
                        }
                        else if (_SelectedJob.CompareType == CompareType.StaticText)
                        {
                            detectModel.CompareResult = StaticTextCompare(detectModel.Text, staticText);
                        }
                        else if (_SelectedJob.CompareType == CompareType.Database)
                        {
                            currentCheckedIndex = -1;
                            bool isNeedToCheckPrintedResponse = true;
                            if (_IsOnProductionMode)
                            {
                                lock (_PrintedResponseLocker)
                                {
                                    isNeedToCheckPrintedResponse = _IsPrintedResponse;
                                    _IsPrintedResponse = false;
                                }
                            }

                            if ((!isNeedToCheckPrintedResponse || _CodeListPODFormat == null) && !_IsReCheck)
                            {
                                detectModel.CompareResult = ComparisonResult.Invalided;
                            }
                            else
                            {
                                int dbIndex = -1;
                                detectModel.CompareResult = isPosition && !isPositionCorrect
                                    ? ComparisonResult.Invalided
                                    : DatabaseCompare(detectModel.Text, ref dbIndex);

                                if (detectModel.CompareResult == ComparisonResult.Valid)
                                {
                                    compareIndex = dbIndex + 1;
                                    currentCheckedIndex = dbIndex;
                                }

                                if (_IsOnProductionMode)
                                {
                                    lock (_CheckLocker)
                                    {
                                        _CheckedResult = detectModel.CompareResult;
                                        _IsCheckedWait = false;
                                        Monitor.PulseAll(_CheckLocker);
                                    }
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

                        //bool isOutputAllowed = !(isPosition && isBarcodePosition && (detectModel.Text == "" || !isBarcodeWithinThreshold));
                        string t = positionHandler.Name;
                        bool isOutputAllowed = positionHandler.ShouldAllowOutput(detectModel, isPositionCorrect);

                        if (Shared.Settings.OutputEnable && isOutputAllowed) //  && Shared.GetCameraStatus()
                        {
                            Shared.RaiseOnCameraOutputSignalChangeEvent(compareIndex);
                        }

                        measureTime.Stop();
                        TotalChecked++;
                        if (detectModel.CompareResult == ComparisonResult.Valid)
                        {
                            NumberOfCheckPassed++;
                            int boxSize = _SelectedJob?.NumberOfCodesInBox ?? 0;
                            if (boxSize > 0 && NumberOfCheckPassed % boxSize == 0)
                            {
                                int expectedBoxes = NumberOfCheckPassed / boxSize;
                                int actualBoxes = _packagingManager?.TotalBoxes ?? 0;
                                ProjectLogger.WriteInfo($"[COUNTER] NumberOfCheckPassed={NumberOfCheckPassed}, ExpectedBoxes={expectedBoxes}, ActualBoxes={actualBoxes}, Diff={expectedBoxes - actualBoxes}");
                            }
                        }
                        else
                        {
                            NumberOfCheckFailed++;
                        }

                        detectModel.Index = compareIndex;
                        detectModel.CompareTime = measureTime.ElapsedMilliseconds;
                        detectModel.ProcessingDateTime = DateTime.Now.ToString(Shared.Settings.DateTimeFormatOfResult);

                        if (isDBStandalone && detectModel.CompareResult == ComparisonResult.Valid)
                        {
                            _QueueBufferUpdateUIPrinter.Enqueue(detectModel.Text);
                        }

                        startIndex = TotalChecked;
                        //List<string[]> test = new List<string[]>();
                        //string[] cloneValue = (string[])_PrintedCodeObtainFromFile[currentCheckedIndex].Clone();
                        //cloneValue[1] = "Checked"; // Now this won't affect the list
                        //test.Add(cloneValue);
                        //string sentDataPath = CommVariables.PathSentDataChecked + _SelectedJob.CheckedResultPath;
                        //string url = Shared.Settings.ApiUrl + "/" + Shared.Settings.RLinkId + "/checkedData";
                        //var clone = test.Select(arr => arr.ToArray()).ToList();
                        //SendDataToServer(apiService, clone, sentDataPath, url);
                        //test.Clear();

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
            if (_CodeListPODFormat == null)
            {
                return ComparisonResult.Invalided;
            }
            else
            {
                var checkNull = txt == "";
                if (!checkNull) // Check null
                {
                    if (_CodeListPODFormat.TryGetValue(txt, out CompareStatus compareStatus))
                    {
                        if (compareStatus.Index == -1)
                        {
                            compareStatus.Index = _PrintedCodeObtainFromFile.FindIndex(x => NormalizeDataForComparison(GetCompareDataByPODFormat(x, _SelectedJob.PODFormat)) == txt);
                        }

                        if (!compareStatus.Status) // Check duplicate
                        {
                            _CodeListPODFormat[txt].Status = true;
                            currentValidIndex = compareStatus.Index;
                            return ComparisonResult.Valid;
                        }
                        else
                        {
                            if (Shared.Settings.AllowDupAndNonStop)
                            {
                                _CodeListPODFormat[txt].Status = true;
                                currentValidIndex = compareStatus.Index;
                                return ComparisonResult.Valid;
                            }
                            return ComparisonResult.Duplicated;
                        }
                    }
                    else
                    {
                        return ComparisonResult.Invalided;
                    }
                }
                else
                {
                    return ComparisonResult.Null;
                }
            }
        }

        private async void UpdateUIPrintedResponseAsync(CancellationToken token)
        {
            await Task.Run(() => { UpdateUIPrintedResponse(token); });
        }

        private void UpdateUIPrintedResponse(CancellationToken token)
        {
            Debug.WriteLine("UI 1 thread working on thread " + Environment.CurrentManagedThreadId);
            // Init CSV path & AllValues
            lock (_PrintLocker)
            {
                if (_SelectedJob.PrintedResponePath == "")
                {
                    string fn = DateTime.Now.ToString(_DateTimeFormat) + "_Printed_" + _SelectedJob.FileName;
                    string p = CommVariables.PathPrintedResponse + fn + ".csv";
                    if (!Directory.Exists(CommVariables.PathPrintedResponse)) Directory.CreateDirectory(CommVariables.PathPrintedResponse);
                    if (!File.Exists(p)) { using (var sw = new StreamWriter(p, true, new UTF8Encoding(true))) sw.WriteLine(String.Join(",", _DatabaseColunms)); }
                    _SelectedJob.PrintedResponePath = fn + ".csv"; _SelectedJob.SaveFile();
                }
            }
            string csvPath = CommVariables.PathPrintedResponse + _SelectedJob.PrintedResponePath;
            if (Shared.DrocoAllValueProcess == null) Shared.DrocoAllValueProcess = new DrocoAllValueProcess(_SelectedJob);

            List<string[]> strPrintedResponseList = new List<string[]>();
            var isAutoComplete = _SelectedJob.CompareType == CompareType.Database;
            int numOfResponse = NumberPrinted;
            int currentIndex = 0;
            var currentPage = 0;
            try
            {
                while (true)
                {
                    // Only stop if handled all data
                    if (token.IsCancellationRequested)
                        if (_QueueBufferUpdateUIPrinter.Count() == 0)
                            token.ThrowIfCancellationRequested(); // Stop thread

                    // Waiting until have data
                    string podCommand = _QueueBufferUpdateUIPrinter.Dequeue();
                    if (podCommand != null)
                    {
                        // Normalize podCommand for consistent comparison
                        podCommand = NormalizeDataForComparison(podCommand);

                        if (_IsOnProductionMode) // Check if need to wait check result
                        {
                            ComparisonResult checkedResult = ComparisonResult.Null;
                            lock (_CheckLocker)
                            {
                                while (_IsCheckedWait) Monitor.Wait(_CheckLocker); // Waiting until detect data was verify
                                checkedResult = _CheckedResult;
                                _IsCheckedWait = true;
                            }

                            lock (_PrintLocker) // Notify that code is printed
                            {
                                _IsPrintedWait = false;
                                _PrintedResult = checkedResult;
                                Monitor.PulseAll(_PrintLocker);
                            }

                            if (checkedResult != ComparisonResult.Valid) continue;
                        }

                        // Update printed status
                        if (_CodeListPODFormat.TryGetValue(podCommand, out CompareStatus compareStatus))
                        {
                            // Printed response backup data
                           // Printed response backup data
            if (_PrintedCodeObtainFromFile[compareStatus.Index][1] == "Waiting" ||
                _PrintedCodeObtainFromFile[compareStatus.Index][1] == "Reprint")
            {
                NumberPrinted++;
                lock (_SyncObjCodeList)
                {
                    (_PrintedCodeObtainFromFile[compareStatus.Index])[1] = "Printed";
                    strPrintedResponseList.Add(_PrintedCodeObtainFromFile[compareStatus.Index]);
                }
            }
                            else if (Shared.Settings.DuplicatedDBEnable)
                            {
                                for (int i = 0; i < _PrintedCodeObtainFromFile.Count; i++)
                                {
                                    var row = _PrintedCodeObtainFromFile[i];
                                    if (row.Length <= 1 || row[1] == "Printed") continue;

                                    var compareRow = row.Where((_, idx) => idx != 1).ToArray();
                                    if (NormalizeDataForComparison(GetCompareDataByPODFormat(compareRow, _SelectedJob.PODFormat)) == podCommand)
                                    {
                                        NumberPrinted++;
                                        compareStatus.Index = i;
                                        lock (_SyncObjCodeList)
                                        {
                                            row[1] = "Printed";
                                            strPrintedResponseList.Add(row);
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
                                Invoke(new Action(() => { dgvDatabase.Invalidate(); }));
                            }
                            else
                            {
                                int row = currentIndex;
                                Invoke(new Action(() =>
                                {
                                    dgvDatabase.Invalidate();
                                    if (row < dgvDatabase.RowCount)
                                        dgvDatabase.Rows[row].Cells[0].Selected = true;
                                }));
                            }

                            // Enqueue to backup thread (CSV + AllValues batch)
                            if (strPrintedResponseList.Count > 0)
                            {
                                _QueueBufferBackupPrintedCode.Enqueue(new List<string[]>(strPrintedResponseList));
                            }
                            strPrintedResponseList.Clear();
                        }
                    }

                }
                Thread.Sleep(1);
            }
            catch (System.OperationCanceledException)
            {
                Console.WriteLine("Thread update printed status was stoppped!");
                _QueueBufferBackupPrintedCode.Enqueue(null);
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

                    //Image image = detectModel.Image == null ? new Bitmap(100, 100) : detectModel.Image;
                    var image = detectModel.Image ?? new Bitmap(100, 100); // MinhChau Modify 11122023

                    if (Shared.Settings.ExportImageEnable && image != null)
                    {
                        if (detectModel.CompareResult != ComparisonResult.Valid)
                        {
                            _QueueBufferBackupImage.Enqueue(new ExportImageModel(new Bitmap(image), detectModel.Index));
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
                    strResult[Index_DateTime] = detectModel.ProcessingDateTime;
                    //strResult[Index_Device] = detectModel.Device;
                    //strResult[Index_Sampled] = detectModel.Sampled;


                    strResultCheckList.Add(strResult);

                    lock (_SyncObjCheckedResultList)
                    {
                        _CheckedResultCodeList.Add(strResult);
                    }
                    //Reprint when all code was checked but number of code pass not enough, only for after production mode and database compare type
                    //if (TotalChecked == NumberPrinted && NumberOfCheckPassed != NumberPrinted)
                    //{
                    //    ReprintUnCheckedCodes();
                    //}
                    // Update checked results
                    Invoke(new Action(() =>
                    {
                        lineCounter = _CheckedResultCodeList.Count() - 1;
                        if (lineCounter < 500)
                        {
                            dgvCheckedResult.Invalidate();
                            dgvCheckedResult.Rows[lineCounter].Cells[0].Selected = true;
                        }
                        else
                        {
                            dgvCheckedResult.Invalidate();
                            dgvCheckedResult.Rows[499].Cells[0].Selected = true;
                        }
                    }));

         
                    // === PACKAGING: Chỉ đếm vào hộp khi camera check Valid ===
                    if (_packagingManager != null && _IsAfterProductionMode
                        && detectModel.CompareResult == ComparisonResult.Valid
                        && !string.IsNullOrEmpty(detectModel.Text))
                    {
                        try
                        {
                            string normalizedCode = detectModel.Text;

                            // Cập nhật trạng thái "valid" trong AllValueProcess (đã được chuyển sang luồng backup)

                            // ── Chế độ Excel-map: tra cứu BoxQR/CartonQR theo GS1 ──
                          if (!_isDrocoExcelPackagingMode)
                            {
                                // Chế độ thường: đếm đủ N mã → sinh QR Hộp tự động
                                _packagingManager.AddValidCode(normalizedCode);
                                UpdatePackagingLabels(); // cập nhật lblContQr
                            }
                            // else: Excel mode nhưng GS1 không có trong map → bỏ qua
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteError("[Packaging] Error in AddValidCode after camera check: " + ex.Message);
                        }
                    }

                    // Cập nhật trạng thái lưới khi camera check pass
                    if (detectModel.CompareResult == ComparisonResult.Valid && detectModel.Index > 0
                        && (detectModel.Index - 1) < _PrintedCodeObtainFromFile.Count)
                    {
                        // TÍNH NĂNG "CỨU" MÃ:
                        // Đánh dấu mã đã check đúng là mã đã in nếu như mã đó chưa đánh dấu (bỏ qua delay 500ms)
                        int captureIndex = detectModel.Index - 1;
                        bool rescued = false;
                        lock (_SyncObjCodeList)
                        {
                            if (_PrintedCodeObtainFromFile[captureIndex].Length > 1 && 
                                (_PrintedCodeObtainFromFile[captureIndex][1] == "Waiting" || _PrintedCodeObtainFromFile[captureIndex][1] == "Reprint"))
                            {
                                _PrintedCodeObtainFromFile[captureIndex][1] = "Printed";
                                NumberPrinted++;
                                rescued = true;
                                _QueueBufferBackupPrintedCode.Enqueue(new List<string[]> { _PrintedCodeObtainFromFile[captureIndex] });
                            }
                        }
                        if (rescued)
                        {
                            Invoke(new Action(() => 
                            { 
                                UpdateCheckTotalAndCheckFailedLabel(); // Cập nhật số đếm trên giao diện
                                UpdateCheckTotalAndPrintedDatabase();  // Cập nhật số lượng đã in trên giao diện
                                dgvDatabase.Invalidate(); 
                            }));
                        }

                        // CHÚ Ý: Bỏ qua việc "chọn dòng" và "đổi trang" ở đây vì luồng máy in đã phụ trách việc này.
                        // Nếu cả camera và máy in cùng chọn dòng trên dgvDatabase, màn hình sẽ bị giật qua lại giữa 2 dòng.
                        Invoke(new Action(() =>
                        {
                            dgvDatabase.Invalidate(); // Chỉ vẽ lại để hiện màu xanh (pass), không chọn (select)
                        }));
                    }

                    //Update value to user interface
                    Invoke(new Action(() =>
                    {
                        txtCodeResult.Text = detectModel.Text;
                        txtProcessingTimeResult.Text = (detectModel.CompareTime) + " ms";
                        txtBarcodeQuality.Text = detectModel.CodeQuality;
                        txtStatusResult.Text = detectModel.CompareResult.ToFriendlyString();
                        txtStatusResult.ForeColor = detectModel.CompareResult == ComparisonResult.Valid ? Color.FromArgb(0, 199, 82) : Color.Red;
                    }));
                    //Update value to user interface
                    //Invoke(new Action(() =>
                    //{
                    //    txtCodeResult.Text = detectModel.Text;
                    //    txtProcessingTimeResult.Text = (detectModel.CompareTime) + " ms";
                    //    txtBarcodeQuality.Text = detectModel.CodeQuality;
                    //    txtStatusResult.Text = detectModel.CompareResult.ToFriendlyString();
                    //    txtStatusResult.ForeColor = detectModel.CompareResult == ComparisonResult.Valid ? Color.FromArgb(0, 199, 82) : Color.Red;
                    //}));

                    //END Add result need save to queue
                    _QueueBufferBackupCheckedResult.Enqueue(new List<string[]>(strResultCheckList));
                    //Clear list
                    strResult.DefaultIfEmpty();
                    strResultCheckList.Clear();

                    ProgressBarCheckedUpdate();

                    //END Update value to user interface
                    //Time delay avoid frezee user interface
                    Thread.Sleep(5);
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
                ProjectLogger.WriteError("[UpdateUICheckedResult] Thread crashed: " + ex.Message, ex);
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

        private async void ExportPrintedResponseToFileAsync(CancellationToken token)
        {
            await Task.Run(() => { NewExportPrintedResponseToFile(token); });
        }

        private PrintingQueueProcessor _printedDataProcess;
        private void NewExportPrintedResponseToFile(CancellationToken token)
        {
            try
            {
                _printedFlushComplete = new ManualResetEventSlim(false);

                if (_SelectedJob.PrintedResponePath == "")
                // Đợi cho đến khi PrintedResponePath được khởi tạo bởi UpdateUIPrintedResponse (nếu nó đang chạy)
                // hoặc tự khởi tạo nếu chưa có. Dùng lock để tránh double header.
                lock (_PrintLocker)
                {
                    if (_SelectedJob.PrintedResponePath == "")
                    {
                        string fileName = DateTime.Now.ToString(_DateTimeFormat) + "_Printed_" + _SelectedJob.FileName;
                        string path = CommVariables.PathPrintedResponse + fileName + ".csv";

                        if (!Directory.Exists(CommVariables.PathPrintedResponse))
                            Directory.CreateDirectory(CommVariables.PathPrintedResponse);

                        if (!File.Exists(path))
                        {
                            using (var sw = new StreamWriter(path, true, new UTF8Encoding(true)))
                                sw.WriteLine(String.Join(",", _DatabaseColunms));
                        }

                        _SelectedJob.PrintedResponePath = fileName + ".csv";
                        _SelectedJob.SaveFile();
                    }
                }

                string filePath = CommVariables.PathPrintedResponse + _SelectedJob.PrintedResponePath;

                if (Shared.DrocoAllValueProcess == null)
                    Shared.DrocoAllValueProcess = new DrocoAllValueProcess(_SelectedJob);

                var allValsBatch = new List<(string qrcode, string printedDate)>();
                DateTime lastFlush = DateTime.Now;

                while (true)
                {
                    if (token.IsCancellationRequested && _QueueBufferBackupPrintedCode.Count() == 0)
                    {
                        break;
                    }

                    List<string[]> valueArr = _QueueBufferBackupPrintedCode.Dequeue();
                    if (valueArr == null) continue;
                    if (valueArr.Count() == 0) continue;

                    // CSV mỗi lần deque (nhanh, append)
                    SaveResultToFile(valueArr, filePath);

                    // Gom AllValues để batch
                    foreach (var row in valueArr)
                    {
                        string gs1 = (row.Length > 2 && (row[2] ?? "").Contains("\\F"))
                            ? row[2].Replace("\\F", "\x1D") : (row.Length > 2 ? row[2] : "");
                        allValsBatch.Add((gs1, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    }
                    if (allValsBatch.Count >= 200 || (DateTime.Now - lastFlush).TotalMilliseconds >= 500
                        || (token.IsCancellationRequested && _QueueBufferBackupPrintedCode.Count() == 0))
                    {
                        if (allValsBatch.Count > 0 && Shared.DrocoAllValueProcess != null)
                            Shared.DrocoAllValueProcess.UpdatePrintBatch(allValsBatch);
                        allValsBatch.Clear();
                        lastFlush = DateTime.Now;
                    }
                }

                // Final flush AllValues
                if (allValsBatch.Count > 0 && Shared.DrocoAllValueProcess != null)
                    Shared.DrocoAllValueProcess.UpdatePrintBatch(allValsBatch);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Backup printed response cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Backup printed response error: " + ex.Message);
                KillAllProccessThread();
                StopProcessAsync(false, Lang.HandleError, false, true);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
            finally
            {
                _printedFlushComplete.Set();
                Console.WriteLine("Backup thread flush completed.");
            }
        }

        private async void BackupSendLogAsync(CancellationToken token)
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
                    KillAllProccessThread();
                    StopProcessAsync(false, Lang.HandleError, false, true);
                    Shared.RaiseOnLogError(ex);
                    EnableUIComponent(OperationStatus.Stopped);
                }
            });
        }

        private async void BackupResultFinishPrintCommandAsync(CancellationToken token)
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

        private async void ExportCheckedResultToFileAsync(CancellationToken token)
        {
            await Task.Run(() => { NewExportCheckedResultToFile(token); });
        }

        private VerificationQueueProcessor _checkedDataProcess;
        private void NewExportCheckedResultToFile(CancellationToken token)
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
                _checkedFlushComplete = new ManualResetEventSlim(false);

                string path = CommVariables.PathCheckedResult + _SelectedJob.CheckedResultPath;

                if (!Directory.Exists(CommVariables.PathSentDataChecked))
                {
                    Directory.CreateDirectory(CommVariables.PathSentDataChecked);
                }
                string sentDataPath = CommVariables.PathSentDataChecked + _SelectedJob.CheckedResultPath;
                string url = ManufacturingApis.postCheckedDataUrl();

                try
                {
                    if (Shared.DrocoAllValueProcess == null)
                        Shared.DrocoAllValueProcess = new DrocoAllValueProcess(_SelectedJob);
                }
                catch (Exception) { }

                var allCheckValsBatch = new List<(string qrcode, string checkedDate)>();
                DateTime lastFlushCheck = DateTime.Now;

                while (true)
                {
                    if (token.IsCancellationRequested && _QueueBufferBackupCheckedResult.Count() == 0)
                    {
                        break;
                    }

                    List<string[]> valueArr = _QueueBufferBackupCheckedResult.Dequeue();

                    if (valueArr == null) continue;
                    if (valueArr.Count() == 0) continue;
                    
                    try
                    {
                        if (Shared.DrocoAllValueProcess == null)
                            Shared.DrocoAllValueProcess = new DrocoAllValueProcess(_SelectedJob);
                    }
                    catch (Exception) { }

                    SaveResultToFile(valueArr, path);

                    // Gom AllValues để batch update check
                    foreach (var row in valueArr)
                    {
                        if (row.Length > Index_ResultData && row[Index_Result] == ComparisonResult.Valid.ToString())
                        {
                            string code = row[Index_ResultData];
                            string codeForUpdate = code.Contains("\\F") ? code.Replace("\\F", "\x1D") : code;
                            string checkNow = (row.Length > Index_DateTime && !string.IsNullOrEmpty(row[Index_DateTime])) 
                                ? row[Index_DateTime] : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            allCheckValsBatch.Add((codeForUpdate, checkNow));
                        }
                    }

                    if (allCheckValsBatch.Count >= 200 || (DateTime.Now - lastFlushCheck).TotalMilliseconds >= 500
                        || (token.IsCancellationRequested && _QueueBufferBackupCheckedResult.Count() == 0))
                    {
                        if (allCheckValsBatch.Count > 0 && Shared.DrocoAllValueProcess != null)
                            Shared.DrocoAllValueProcess.UpdateCheckBatch(allCheckValsBatch);
                        allCheckValsBatch.Clear();
                        lastFlushCheck = DateTime.Now;
                    }

                    valueArr.Clear();
                }

                // Final flush
                if (allCheckValsBatch.Count > 0 && Shared.DrocoAllValueProcess != null)
                    Shared.DrocoAllValueProcess.UpdateCheckBatch(allCheckValsBatch);
                
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
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
            finally
            {
                _checkedFlushComplete.Set();
                Console.WriteLine("Backup checked result thread flush completed.");
            }
        }

        private VerificationQueueProcessor _cargoDataStorageProcess;
        private void CreateDrocoCargoStorageProcesssor(JobModel jobModel)
        {
            try
            {
                if (!Directory.Exists(CommVariables.PathSentDataPallet))
                {
                    Directory.CreateDirectory(CommVariables.PathSentDataPallet);
                }
                string sentPalletDataPath = CommVariables.PathSentDataPallet + jobModel.CheckedResultPath;
                string pathPallet = CommVariables.PathSentDataPallet + jobModel.CheckedResultPath;
                if (_cargoDataStorageProcess != null) _cargoDataStorageProcess.Stop();
                _cargoDataStorageProcess = ReliableProcessorFactory.CreateDrocoCargoStorageProcessor(sentPalletDataPath, "", pathPallet);
                _cargoDataStorageProcess.Start();
            }
            catch (Exception)
            {
            }
        }

        private VerificationQueueProcessor _cargoDataSenderProcess;

        private void CreateDrocoCargoSenderProcesssor(JobModel jobModel)
        {
            try
            {
                if (!Directory.Exists(CommVariables.PathSentDataCargo))
                {
                    Directory.CreateDirectory(CommVariables.PathSentDataCargo);
                }
                string sentPalletDataPath = CommVariables.PathSentDataCargo + jobModel.CheckedResultPath;
                string pathPallet = CommVariables.PathSentDataCargo + jobModel.CheckedResultPath;
                if (_cargoDataSenderProcess != null) _cargoDataSenderProcess.Stop();
                _cargoDataSenderProcess = ReliableProcessorFactory.CreateDrocoCargoSenderProcessor(sentPalletDataPath, "", pathPallet);
                _cargoDataSenderProcess.Start();
            }
            catch (Exception)
            {
            }
        }


        private async void ExportImageToFileAsync(CancellationToken token)
        {
            await Task.Run(() => { NewExportImageToFile(token); });
        }

        private void NewExportImageToFile(CancellationToken token)
        {
            if (!Directory.Exists(Shared.Settings.ExportImagePath + "\\" + _SelectedJob.FileName))
            {
                Directory.CreateDirectory(Shared.Settings.ExportImagePath + "\\" + _SelectedJob.FileName);
            }
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        if (_QueueBufferBackupImage.Count() == 0)
                        {
                            token.ThrowIfCancellationRequested();
                        }
                    }

                    ExportImageModel exportImageModel = null;
                    exportImageModel = _QueueBufferBackupImage.Dequeue();

                    if (exportImageModel != null)
                    {
                        string fileName = string.Format("\\{0}_Job_{1}_Image_{2:D7}.bmp", _ExportNamePrefix, _SelectedJob.FileName, exportImageModel.Index);
                        if (Shared.Settings.ExportImagePath == null) Shared.Settings.ExportImagePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                        string path = Shared.Settings.ExportImagePath + "\\" + _SelectedJob.FileName + fileName;
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

        private async void StopProcessAsync(bool interactOnUI = true, string messages = "", bool isClosed = false, bool isManualClose = false)
        {
            bool stopped = await Task.Run(() => StopProcess(interactOnUI, messages, isClosed, isManualClose));
            if (!stopped) return;

            Shared.RaiseOnStopButtonClick();

            // Show dialog thông báo đang lưu (UI đẹp hơn)
            var waitForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(340, 130),
                BackColor = Color.Magenta,
                TransparencyKey = Color.Magenta,
                ShowInTaskbar = false
            };

            float spinnerAngle = 0;
            string loadingText = "Đang đồng bộ dữ liệu...\nVui lòng chờ trong giây lát";

            // Enable double buffering to prevent flickering during GDI+ custom painting
            typeof(Form).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, waitForm, new object[] { true });

            waitForm.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw rounded background card (dark premium grey)
                Rectangle rect = new Rectangle(0, 0, waitForm.Width - 1, waitForm.Height - 1);
                using (var path = GetRoundedRect(rect, 12))
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(245, 24, 24, 27)))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    using (Pen pen = new Pen(Color.FromArgb(120, 0, 171, 230), 1.5f))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                // Draw rotating spinner (premium R-Link blue)
                int size = 36;
                int spinnerX = (waitForm.Width - size) / 2;
                int spinnerY = 22;

                using (Pen backgroundPen = new Pen(Color.FromArgb(40, 255, 255, 255), 3.5f))
                {
                    e.Graphics.DrawEllipse(backgroundPen, spinnerX, spinnerY, size, size);
                }

                using (Pen foregroundPen = new Pen(Color.FromArgb(0, 171, 230), 3.5f))
                {
                    foregroundPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    foregroundPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    e.Graphics.DrawArc(foregroundPen, spinnerX, spinnerY, size, size, spinnerAngle, 100);
                }

                // Draw text centered below the spinner
                using (Font font = new Font("Segoe UI", 11.5f, FontStyle.Bold))
                {
                    using (Brush brush = new SolidBrush(Color.White))
                    {
                        StringFormat sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near
                        };
                        RectangleF textRect = new RectangleF(10, 75, waitForm.Width - 20, 50);
                        e.Graphics.DrawString(loadingText, font, brush, textRect, sf);
                    }
                }
            };

            var animTimer = new System.Windows.Forms.Timer { Interval = 20 };
            animTimer.Tick += (s, e) =>
            {
                spinnerAngle = (spinnerAngle + 12) % 360;
                waitForm.Invalidate();
            };
            animTimer.Start();

            // Vô hiệu hóa form cha để người dùng không click gì trong lúc lưu
            bool wasEnabled = this.Enabled;
            try { this.Enabled = false; waitForm.Show(this); waitForm.Refresh(); Application.DoEvents(); } catch { }

            // Chờ queue checked result và printed code xử lý xong
            int waitMs = 0;
            const int maxWaitMs = 5000; // Chờ tối đa 5s
            const int stepMs = 50; // Check nhanh hơn
            await Task.Run(() =>
            {
                while (waitMs < maxWaitMs)
                {
                    if (_QueueBufferDataObtainedResult.Count() == 0 &&
                        _QueueBufferBackupPrintedCode.Count() == 0 &&
                        _QueueBufferUpdateUIPrinter.Count() == 0 &&
                        _QueueBufferBackupCheckedResult.Count() == 0) // Quan trọng: chờ AllValue queue
                        break;
                    Thread.Sleep(stepMs);
                    waitMs += stepMs;
                }
            });
            
            animTimer.Stop();

            // Chờ backup thread flush xong (tối đa 5s)
            if (!_printedFlushComplete.Wait(5000))
                Console.WriteLine("WARNING: Backup printed thread did not flush within 5s!");
            if (!_checkedFlushComplete.Wait(5000))
                Console.WriteLine("WARNING: Backup checked thread did not flush within 5s!");

            try { waitForm.Invoke(new Action(() => waitForm.Close())); } catch { }
            try { this.Enabled = wasEnabled; } catch { }
            if (wasEnabled) try { this.Activate(); } catch { }

            // Thêm buffer nhỏ để UI thread cập nhật xong
            await Task.Delay(100);
            UpdateCheckTotalAndCheckFailedLabel();
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseAllFigures();
            return path;
        }

        // ─── Loading overlay cho export ───────────────────────────────────
        private Form _loadingForm = null;
        private Timer _loadingTimer = null;
        private Timer _loadingTimeout = null;
        private float _loadingAngle = 0;

        private void ShowLoading(string text)
        {
            if (_loadingForm != null) return;

            _loadingForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(340, 130),
                BackColor = Color.Magenta,
                TransparencyKey = Color.Magenta,
                ShowInTaskbar = false,
                Owner = this,
                ControlBox = false
            };

            typeof(Form).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, _loadingForm, new object[] { true });

            string loadingText = text;
            _loadingForm.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, _loadingForm.Width - 1, _loadingForm.Height - 1);
                using (var path = GetRoundedRect(rect, 12))
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(245, 24, 24, 27)))
                        e.Graphics.FillPath(brush, path);
                    using (Pen pen = new Pen(Color.FromArgb(120, 0, 171, 230), 1.5f))
                        e.Graphics.DrawPath(pen, path);
                }

                int size = 36;
                int spinnerX = (_loadingForm.Width - size) / 2;
                int spinnerY = 22;
                using (Pen backgroundPen = new Pen(Color.FromArgb(40, 255, 255, 255), 3.5f))
                    e.Graphics.DrawEllipse(backgroundPen, spinnerX, spinnerY, size, size);
                using (Pen foregroundPen = new Pen(Color.FromArgb(0, 171, 230), 3.5f))
                {
                    foregroundPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    foregroundPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    e.Graphics.DrawArc(foregroundPen, spinnerX, spinnerY, size, size, _loadingAngle, 100);
                }

                using (Font font = new Font("Segoe UI", 11.5f, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                    RectangleF textRect = new RectangleF(10, 75, _loadingForm.Width - 20, 50);
                    e.Graphics.DrawString(loadingText, font, brush, textRect, sf);
                }
            };

            _loadingTimer = new Timer { Interval = 20 };
            _loadingTimer.Tick += (s, e) =>
            {
                _loadingAngle = (_loadingAngle + 12) % 360;
                _loadingForm?.Invalidate();
            };
            _loadingTimer.Start();

            // Timeout an toàn: tự động ẩn sau 60s nếu HideLoading không được gọi
            _loadingTimeout = new Timer { Interval = 60000 };
            _loadingTimeout.Tick += (s, e) =>
            {
                var t = s as Timer;
                if (t != null) { t.Stop(); t.Dispose(); }
                HideLoading();
            };
            _loadingTimeout.Start();

            // Vô hiệu hóa form cha + căn giữa loading form
            this.Enabled = false;
            try
            {
                _loadingForm.StartPosition = FormStartPosition.Manual;
                _loadingForm.Location = new Point(
                    this.Location.X + (this.Width - _loadingForm.Width) / 2,
                    this.Location.Y + (this.Height - _loadingForm.Height) / 2);
                _loadingForm.Show(this);
                _loadingForm.Refresh();
                Application.DoEvents();
            }
            catch { }
        }

        private void HideLoading()
        {
            if (_loadingTimer != null)
            {
                _loadingTimer.Stop();
                _loadingTimer.Dispose();
                _loadingTimer = null;
            }
            if (_loadingTimeout != null)
            {
                _loadingTimeout.Stop();
                _loadingTimeout.Dispose();
                _loadingTimeout = null;
            }
            if (_loadingForm != null)
            {
                try { _loadingForm.Close(); } catch { }
                _loadingForm.Dispose();
                _loadingForm = null;
            }
            this.Enabled = true;
        }
        // ─────────────────────────────────────────────────────────────────

        private bool StopProcess(bool interactOnUI = true, string messages = "", bool isClosed = false, bool isManualClose = false)
        {
            //if (interactOnUI)
            //{
            //    DialogResult dialogResult = DialogResult.None;
            //    if (dialogResultStopExist) { return false; }
            //    dialogResultStopExist = true;

            //    this.Invoke((MethodInvoker)delegate
            //    {
            //        dialogResult = CustomMessageBox.Show(Lang.DoYouWantToStopTheSystem, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            //    });
            //    if (dialogResult != DialogResult.Yes)
            //    {
            //        dialogResultStopExist = false;
            //        return false;
            //    }
            //    else
            //    {
            //        Shared.OperStatus = OperationStatus.Stopped;
            //    }

            //    dialogResultStopExist = false;
            //    messages = Lang.UserStoppedTheSystem;

            //}
            if (interactOnUI)
            {
                DialogResult dialogResult = DialogResult.None;
                if (dialogResultStopExist) { return false; }
                dialogResultStopExist = true;

                try
                {
                    if (!IsDisposed && IsHandleCreated)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            dialogResult = CustomMessageBox.Show(Lang.DoYouWantToStopTheSystem, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        });
                    }
                }
                catch (ObjectDisposedException) { dialogResultStopExist = false; return false; }
                catch (InvalidOperationException) { dialogResultStopExist = false; return false; }

                if (dialogResult != DialogResult.Yes)
                {
                    dialogResultStopExist = false;
                    return false;
                }
                else
                {
                    Shared.OperStatus = OperationStatus.Stopped;
                }

                dialogResultStopExist = false;
                messages = Lang.UserStoppedTheSystem;
            }
            ChangeCheckMode(Checkmode.Camera);
            _VirtualCTS?.Cancel();
            KillTThreadSendPODDataToPrinter();
            _QueueBufferPrinterResponseData.Clear();



            if (Shared.Settings.IsPrinting && _SelectedJob.PrinterSeries)
            {
                PODController podController = Shared.Settings.PrinterList.Where(p => p.RoleOfPrinter == RoleOfStation.ForProduct).FirstOrDefault().PODController;
                if (podController != null)
                {
                    podController.Send("STOP");
                    lock (_StopLocker)
                    {
                        _IsStopOK = false;
                        int countTimeout = 0;
                        while (!_IsStopOK && countTimeout < 1)
                        {
                            Monitor.Wait(_StopLocker, 3000); //Wait until there is a stop notify from the printer
                            countTimeout++;
                        }
                    }
                }
            }
            _TotalMissed = 0;
            Shared.IsSampled = false;
            _IsReCheck = false;
            _UIPrintedResponseCancelTokenSource?.Cancel();
            _OperationCancelTokenSource?.Cancel();
            while (_QueueBufferDataObtained.TryDequeue(out _)) { }
            while (_QueuePositionDataObtained.TryDequeue(out _)) { }
            _QueueBufferUpdateUIPrinter.Enqueue(null);
            if (ProjectLabel.IsNutrifood)
            {
                _printedDataProcess.Stop();
                _checkedDataProcess.Stop();
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

                    int totalBoxes = _SelectedJob.BoxList?.Count ?? 0;
                    int totalCartons = _SelectedJob.DrocoCartonList?.Count ?? 0;
                    int totalPallets = _SelectedJob.DrocoPalletList?.Count ?? 0;
                    int totalGSCodes = _SelectedJob.BoxList?.Sum(b => b.ProductCodes?.Count ?? 0) ?? 0;
                    ProjectLogger.WriteInfo($"[JOB COMPLETE] TotalGS1={totalGSCodes} Boxes={totalBoxes} Cartons={totalCartons} Pallets={totalPallets} " +
                        $"ExpectedGS1={_TotalCode} Dup={_NumberOfDuplicate} CheckPassed={NumberOfCheckPassed}");
                }
            }

            if (!interactOnUI)
            {
                countFormStopSuddenly++;
                if (countFormStopSuddenly <= 1)
                {
                    DialogResult dialogResult = CustomMessageBox.Show(messages, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                NumberOfSentPrinter = 0;
                ReceivedCode = 0;
            }

            try
            {
                _cargoDataStorageProcess.Stop();
                _cargoDataSenderProcess.Stop();
            }
            catch (Exception)
            {
            }

            // Dừng tất cả các luồng xử lý và backup
            KillAllProccessThread();
            return true;
        }

        private void KillThread(ref Thread thread)
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
                thread = null;
            }
        }

        private void KillAllProccessThread()
        {
            _VirtualCTS?.Cancel();
            _UICheckedResultCancelTokenSource?.Cancel();
            _UIPrintedResponseCancelTokenSource?.Cancel();
            _BackupImageCancelTokenSource?.Cancel();
            _BackupResultCancelTokenSource?.Cancel();
            _BackupResponseCancelTokenSource?.Cancel();
            _BackupSendLogCancelTokenSource?.Cancel();
            _BackupRSFPLogCancelTokenSource?.Cancel();

            _QueueBufferDataObtainedResult.Enqueue(null);
            _QueueBufferUpdateUIPrinter.Enqueue(null);
            _QueueBufferBackupImage.Enqueue(null);
            _QueueBufferBackupPrintedCode.Enqueue(null);
            _QueueBufferBackupCheckedResult.Enqueue(null);
            _QueueBufferBackupSendLog.Enqueue(null);
            _QueueBufferBackupRSFPLog.Enqueue(null);
        }

        #endregion Operation

        #region Jobs
        private void UpdateJobInfomationInterface()
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

                TotalChecked = 0;
                NumberOfCheckPassed = 0;
                NumberOfCheckFailed = 0;
                ProgressBarInitialize();
                ProgressBarCheckedUpdate();
                UpdateJobInfo(_SelectedJob);
                EnableUIComponentWhenLoadData(false);
                btnStop.Enabled = false;
                btnTrigger.Enabled = false;
                pnlMenu.Enabled = false;

                _CheckedResultCodeList.Clear();
                _PrintedCodeObtainFromFile.Clear();
                _CodeListPODFormat.Clear();
                dgvDatabase.RowCount = 0;
                dgvCheckedResult.RowCount = 0;

                InitDataAsync(_SelectedJob);
                LoadProgressFromAllValues(); // Hiển thị số mã/hộp ngay khi mở job
            }
        }

        /// <summary>
        /// Đọc AllValues → cập nhật txtTotalQrToBox và lblContQr ngay khi mở job, không cần Start.
        /// Được gọi từ UpdateJobInfomationInterface() và từ Shown event.
        /// </summary>
        private void LoadProgressFromAllValues()
        {
            try
            {
                var job = _SelectedJob ?? Shared.CurrentJob;
                if (job == null) return;
                if (job.NumberOfCodesInBox <= 0) return;

                var process = new DrocoAllValueProcess(job);
                var payload = process.GetAllValuePayload();

                int totalCodes = (int)job.NumberTotalsCode;
                int passed = payload.qr_list.Count(q =>
                    string.Equals(q.status, "valid", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(q.status, "mapped", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(q.status, "printed", StringComparison.OrdinalIgnoreCase));
                int pending = payload.qr_list.Count(q =>
                    string.Equals(q.status, "valid", StringComparison.OrdinalIgnoreCase));
                int boxSize = job.NumberOfCodesInBox;

                SafeInvoke(() =>
                {
                    if (txtTotalQrToBox != null)
                        txtTotalQrToBox.Text = $"{passed:N0} / {totalCodes:N0}";
                    if (lblContQr != null)
                        lblContQr.Text = $"Mã đã vào hộp: {pending}/{boxSize}";
                });
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"LoadProgressFromAllValues: {ex.Message}");
            }
        }

        private void ReprintUnCheckedCodes()
        {
            // 1. Đọc lại file checked result (nếu cần, có thể dùng _CheckedResultCodeList nếu đã load đúng)
            string checkedResultPath = CommVariables.PathCheckedResult + _SelectedJob.CheckedResultPath;
            var checkedResultList = new List<string[]>();
            if (File.Exists(checkedResultPath))
            {
                var rexCsvSplitter = checkedResultPath.EndsWith(".csv")
                    ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))")
                    : new Regex(@"[\t]");
                using (var reader = new StreamReader(checkedResultPath, Encoding.UTF8, true))
                {
                    bool isFirstLine = true;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (isFirstLine) { isFirstLine = false; continue; } // bỏ header
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var arr = rexCsvSplitter.Split(line).Select(x => Csv.Unescape(x)).ToArray();
                        checkedResultList.Add(arr);
                    }
                }
            }
            else
            {
                // Nếu không có file, dùng danh sách đã load
                checkedResultList = _CheckedResultCodeList;
            }

            // 2. Tạo HashSet các mã đã kiểm tra thành công (Valid)
            var checkedCodes = new HashSet<string>(
                checkedResultList
                    .Where(x => x.Length > Index_Result && x[Index_Result] == "Valid")
                    .Select(x => NormalizeDataForComparison(GetCompareDataByPODFormat(x, _SelectedJob.PODFormat)))
            );

            // 3. Lọc ra các mã đã in nhưng chưa được kiểm tra thành công
            var codesToReprint = _PrintedCodeObtainFromFile
                .Where(record =>
                    record.Length > 1 &&
                    record[1] == "Printed" &&
                    !checkedCodes.Contains(
                        NormalizeDataForComparison(
                            GetCompareDataByPODFormat(record.Where((item, idx) => idx != 1).ToArray(), _SelectedJob.PODFormat)
                        )
                    )
                )
                .ToList();

            // 4. Ghi log chi tiết
            string logDirectory = @"C:\ProgramData\R-Link\Reprint";
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
            string logPath = Path.Combine(logDirectory, "ReprintLogGS1.txt");
            using (var logWriter = new StreamWriter(logPath, true, Encoding.UTF8))
            {
                logWriter.WriteLine($"=== {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                logWriter.WriteLine($"CheckedResultPath: {checkedResultPath}");
                logWriter.WriteLine($"Số mã đã check đúng (Valid): {checkedCodes.Count}");
                logWriter.WriteLine($"Số mã đã in: {_PrintedCodeObtainFromFile.Count(x => x.Length > 1 && x[1] == "Printed")}");
                logWriter.WriteLine($"Số mã sẽ gửi lại: {codesToReprint.Count}");

                foreach (var record in codesToReprint)
                {
                    var code = NormalizeDataForComparison(
                        GetCompareDataByPODFormat(record.Where((item, idx) => idx != 1).ToArray(), _SelectedJob.PODFormat)
                    );
                    logWriter.WriteLine($"[REPRINT] Index: {record[0]}, Code: {code}");
                    if (checkedCodes.Contains(code))
                    {
                        logWriter.WriteLine($"[WARNING] Mã này đã có trong checkedCodes nhưng vẫn bị gửi lại!");
                    }
                }
                logWriter.WriteLine("========================================");
            }

            // 5. Gửi lại mã chưa được kiểm tra
            int reprintedCount = 0;
            foreach (var codeModel in codesToReprint)
            {
                //string data = string.Join(Shared.Settings.SplitCharacter.ToString(), codeModel.Skip(2).ToArray());
                string data = string.Join(
    Shared.Settings.SplitCharacter.ToString(),
    codeModel.Skip(2).Select(x => ReplaceAllGS1Separators(x ?? "")));
                string command = $"DATA;{data}";
                if (podController != null)
                {
                    podController.Send(command);
                    NumberOfSentPrinter++;
                    reprintedCount++;
                }
            }

            if (reprintedCount > 0)
            {
                CustomMessageBox.Show($"Đã gửi lại {reprintedCount} mã chưa được kiểm tra!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                CustomMessageBox.Show("Không có mã nào cần gửi lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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

        private async void InitDataAsync(JobModel jobModel)
        {
            _BigSTW.Start();
            Stopwatch stw = Stopwatch.StartNew();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " Start init data on thread " + Environment.CurrentManagedThreadId);

            if (jobModel.CompareType == CompareType.Database)
            {
                Task<List<string[]>> databaseTsk = InitDatabaseAndPrintedStatusAsync(jobModel); //Load database and update printed status
                Task<List<string[]>> checkedResultTsk = InitCheckedResultDataAsync(jobModel); //Load checked result
                await Task.WhenAll(databaseTsk, checkedResultTsk); // Waiting until database and checked result completed load

                string checkInitDataMessage = "";
                checkInitDataMessage = CheckInitDataErrorAndGenerateMessage();
                if (checkInitDataMessage != "")
                {

                    foreach (string value in checkInitDataMessage.Split('\n'))
                    {
                        if (value != "")
                        {
                            CuzAlert.Show(value, Alert.enmType.Error, new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                        }
                    }
                }
                else
                {

                    _PrintedCodeObtainFromFile = databaseTsk.Result;
                    _CheckedResultCodeList = checkedResultTsk.Result;
                    // thinh dang lam ne
                    if (_PrintedCodeObtainFromFile.Count() > 1)  // Inititalize database information
                    {
                        _DatabaseColunms = _PrintedCodeObtainFromFile[0];
                        _PrintedCodeObtainFromFile.RemoveAt(0);

                        if (_SelectedJob.CompareType == CompareType.Database) // Initialize compare data
                        {
                            await InitCompareDataAsync(_PrintedCodeObtainFromFile, _CheckedResultCodeList); // Waiting until initialize compare data completed
                        }

                        _TotalCode = _PrintedCodeObtainFromFile.Count();
                        NumberPrinted = _PrintedCodeObtainFromFile.Where(x => x[1] == "Printed").Count();
                        int firstWaiting = _PrintedCodeObtainFromFile.IndexOf(_PrintedCodeObtainFromFile.Find(x => x[1] == "Waiting"));  // Identify datas need to display by first waiting code
                        _CurrentPage = CalculateCurrentPage(_TotalCode, _MaxDatabaseLine, firstWaiting);
                        InitDataGridView(dgvDatabase, _DatabaseColunms, 1, true); //// Implement virtual mode for DataGridView display database
                        var lastCode = _PrintedCodeObtainFromFile[_PrintedCodeObtainFromFile.Count() - 1];// Adjust width of columns
                        AutoResizeColumnWith(dgvDatabase, lastCode, _DatabaseColunms.Length - 1);
                        dgvDatabase.RowCount = _TotalCode > _MaxDatabaseLine ? _MaxDatabaseLine : _TotalCode; // Define number of DataGridView row
                        dgvDatabase.Invalidate(); // Update both of DataGridView
                        if (firstWaiting >= 0) { int r = firstWaiting % _MaxDatabaseLine; if (r < dgvDatabase.RowCount) dgvDatabase.FirstDisplayedScrollingRowIndex = Math.Max(0, r - 3); }

                        if (_NumberOfDuplicate > 0)
                        {

                            txtStaticText.Text = $"{_TotalCode} ({_NumberOfDuplicate} Duplicate)";
                            CuzAlert.Show(Lang.DuplicateDataMessage.Replace("NN", "" + _NumberOfDuplicate) + Lang.PODFormat, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), Size, false);
                        }

                    }
                }
            }
            else
            {
                _CheckedResultCodeList = await InitCheckedResultDataAsync(jobModel);  // Load checked result
            }


            TotalChecked = _CheckedResultCodeList.Count();
            NumberOfCheckPassed = _CheckedResultCodeList.Where(x => x[2] == "Valid").Count();
            NumberOfCheckFailed = TotalChecked - NumberOfCheckPassed;

            InitDataGridView(dgvCheckedResult, _ColumnNames, 2);  // Implement virtual mode for DataGridView display checked results
            await Task.Delay(50);
            AutoResizeColumnWith(dgvCheckedResult, defaultRecord, 2);  // Adjust width of columns
           // DataGridViewCustom.AdjustColumnWidthsToFitContent(dgvCheckedResult);
            DataGridViewCustom.AdjustColumnWidthsToFitContent(dgvCheckedResult);
            // ── Set ResultData column cố định 400px ──
            if (dgvCheckedResult.Columns.Count > 1)
            {
                dgvCheckedResult.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvCheckedResult.Columns[1].Width = 350;
               
            }

            DataGridViewCustom.AdjustColumnWidthsToFitContent(dgvDatabase);
            // Update progress bar
            ProgressBarInitialize();
            ProgressBarCheckedUpdate();
            prBarCheckPassed.Invalidate();
            dgvCheckedResult.Invalidate();
            // Enable control after completed initialize data
            pnlMenu.Enabled = true;
            EnableUIComponentWhenLoadData(true);
            stw.Stop();
            Debug.WriteLine("Init completed, it took " + stw.ElapsedMilliseconds);
        }



        private async Task<List<string[]>> InitDatabaseAndPrintedStatusAsync(JobModel jobModel)
        {
            var pathDatabase = jobModel.DirectoryDatabase;
            var pathBackupPrintedResponse = CommVariables.PathPrintedResponse + jobModel.PrintedResponePath;

            // Initialize barcode data
            var tmp = await Task.Run(() => { return InitDatabase(pathDatabase, jobModel.IsFirstRowHeader); });
            if (jobModel.PrintedResponePath != "" && File.Exists(jobModel.DirectoryDatabase) && tmp.Count() > 1)
                await Task.Run(() => { InitPrintedStatus(pathBackupPrintedResponse, tmp); });
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

        private async Task InitCompareDataAsync(List<string[]> datas, List<string[]> result)
        {
            // Initialize compare data async implement
            await Task.Run(() => { InitCompareData(datas, result); });
        }

        private List<string[]> InitDatabase(string path, bool isFirstRowHeader)
        {
            Stopwatch stw = Stopwatch.StartNew();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitDatabase work on thread " + Environment.CurrentManagedThreadId);
            List<string[]> result = new List<string[]>();

            if (!File.Exists(path))
            {
                _InitDataErrorList.Add(InitDataError.DatabaseDoNotExist);
                DialogResult dialogResult = CustomMessageBox.Show("'" + path + "' " + Lang.CanNotFindDatabase, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return result;
            }

            try
            {
                string extension = System.IO.Path.GetExtension(path).ToLower();

                // Check if it's an Excel file (.xlsx or .xls)
                if (extension == ".xlsx" || extension == ".xls")
                {
                    return InitDatabaseFromExcel(path, isFirstRowHeader);
                }
                else
                {
                    return InitDatabaseFromCsv(path, isFirstRowHeader);
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
            return result;
        }

        /// <summary>
        /// Replaces GS (Group Separator) and other special characters with \F for GS1 data
        /// </summary>
        private string ReplaceGSCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Replace GS (Group Separator) character (ASCII 29, 0x1D) with \F
            return input.Replace("\u001D", "\\F");
        }

        /// <summary>
        /// Normalizes data by replacing <0x1D> string representation with \F for consistent comparison
        /// </summary>
        private string NormalizeDataForComparison(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return input
                .Replace("\u001D", "\\F")
                .Replace("<0x1D>", "\\F")
                .Replace("_x001d_", "\\F")
                .Replace("%1d", "\\F");
        }
        //private string ReplaceGSCharacters(string input)
        //{
        //    // kept for backward compatibility — delegate to unified normalizer
        //    return NormalizeForComparison(input);
        //}

        //private string NormalizeDataForComparison(string input)
        //{
        //    // kept for backward compatibility — delegate to unified normalizer
        //    return NormalizeForComparison(input);
        //}
        //private string NormalizeForComparison(string input)
        //{
        //    if (string.IsNullOrEmpty(input))
        //        return input;

        //    // Remove common invisible characters first
        //    string s = input.Replace("\r", "").Replace("\n", "").Trim();
        //    s = s.Replace("\uFEFF", ""); // BOM
        //    s = Regex.Replace(s, @"\u200B|\u200C|\u200D", ""); // zero-width chars

        //    // 1) Replace actual Group Separator char (ASCII 29) with canonical \F
        //    s = s.Replace("\u001D", "\\F");

        //    // 2) Common textual forms
        //    s = Regex.Replace(s, @"<\s*0x1d\s*>", "\\F", RegexOptions.IgnoreCase | RegexOptions.Compiled); // <0x1D>
        //    s = Regex.Replace(s, @"_x001d_", "\\F", RegexOptions.IgnoreCase | RegexOptions.Compiled);     // _x001d_

        //    // 3) Escaped hex/unicode forms (literal backslashes in the text)
        //    s = Regex.Replace(s, @"\\+x1d", "\\F", RegexOptions.IgnoreCase | RegexOptions.Compiled);     // \x1D or \\x1D
        //    s = Regex.Replace(s, @"\\+u001d", "\\F", RegexOptions.IgnoreCase | RegexOptions.Compiled);   // \u001D or \\u001D

        //    // 4) Percent-encoding
        //    s = Regex.Replace(s, @"%1d", "\\F", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        //    // 5) Normalize any \f or multiple backslashes before F to single canonical "\F"
        //    s = Regex.Replace(s, @"\\+f", "\\F", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        //    // 6) Collapse accidental double-backslash sequences like "\\F" -> "\F"
        //    s = s.Replace("\\\\F", "\\F");

        //    return s;
        //}
        /// <summary>
        /// Initializes database from Excel file (.xlsx or .xls)
        /// Uses ExcelDataReader - no external dependencies required
        /// </summary>
        /// <summary>
        /// Initializes database from Excel file (.xlsx or .xls).
        /// Droco mode: tự động phát hiện cột "Honest mark_UNIT" trong 10 hàng đầu
        /// (header Droco ở row 3 — index 2) và CHỈ load cột đó vào DataGridView.
        /// Chế độ thường: giữ nguyên toàn bộ cột.
        /// </summary>
        //private List<string[]> InitDatabaseFromExcel(string path, bool isFirstRowHeader)
        //{
        //    List<string[]> result = new List<string[]>();

        //    try
        //    {
        //        List<string[]> allRows = FileFuncs.ReadExcelData(path, null);

        //        if (allRows == null || allRows.Count == 0)
        //        {
        //            _InitDataErrorList.Add(InitDataError.DatabaseUnknownError);
        //            return result;
        //        }

        //        string[] headerRow = FileFuncs.GetFirstRowFromExcel(path, true);
        //        if (headerRow == null || headerRow.Length == 0)
        //        {
        //            _InitDataErrorList.Add(InitDataError.DatabaseUnknownError);
        //            return result;
        //        }

        //        // ── Tìm hàng header Droco (tối đa 10 hàng đầu) ───────────────────
        //        // Dùng EXACT MATCH cho "Honest mark_UNIT" để tránh match ô mô tả
        //        // ("Honest mark_UNIT - 러시아 법인 전달 예정" ≠ "Honest mark_UNIT")
        //        int gs1ColIndex = -1;
        //        int boxColIndex = -1;
        //        int cartonColIndex = -1;
        //        int headerRowIndex = -1;

        //        int scanLimit = Math.Min(allRows.Count, 10);
        //        for (int ri = 0; ri < scanLimit; ri++)
        //        {
        //            string[] scanRow = allRows[ri];
        //            int tempGs1 = -1, tempBox = -1, tempCarton = -1;

        //            for (int ci = 0; ci < scanRow.Length; ci++)
        //            {
        //                string cellVal = (scanRow[ci] ?? "").Trim();
        //                if (string.IsNullOrEmpty(cellVal)) continue;

        //                // EXACT match "Honest mark_UNIT" — loại trừ ô mô tả nhiều dòng
        //                if (string.Equals(cellVal, "Honest mark_UNIT", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    if (tempGs1 < 0) tempGs1 = ci;
        //                }

        //                // Contains match nhưng không chứa "MASTER" → cột SSCC_MID
        //                if (cellVal.IndexOf("SSCC_MID", StringComparison.OrdinalIgnoreCase) >= 0
        //                    && cellVal.IndexOf("SSCC_MASTER", StringComparison.OrdinalIgnoreCase) < 0)
        //                {
        //                    tempBox = ci;
        //                }

        //                // Contains match "SSCC_MASTER" → cột Carton
        //                if (cellVal.IndexOf("SSCC_MASTER", StringComparison.OrdinalIgnoreCase) >= 0)
        //                {
        //                    tempCarton = ci;
        //                }
        //            }

        //            // Hàng header hợp lệ: GS1 và BOX phải ở CÁC CỘT KHÁC NHAU
        //            if (tempGs1 >= 0 && tempBox >= 0 && tempGs1 != tempBox)
        //            {
        //                gs1ColIndex = tempGs1;
        //                boxColIndex = tempBox;
        //                cartonColIndex = tempCarton;
        //                headerRowIndex = ri;
        //                break;
        //            }
        //        }

        //        // ════════════════════════════════════════════════════════════════
        //        // CHẾ ĐỘ DROCO — chỉ hiển thị cột "Honest mark_UNIT"
        //        // ════════════════════════════════════════════════════════════════
        //        if (gs1ColIndex >= 0 && headerRowIndex >= 0)
        //        {
        //            string colHeader = allRows[headerRowIndex][gs1ColIndex].Trim();
        //            result.Add(new string[] { "Index", "Status", colHeader + " - Field1" });

        //            int lineCounter = 0;
        //            for (int ri = headerRowIndex + 1; ri < allRows.Count; ri++)
        //            {
        //                string[] row = allRows[ri];
        //                string gs1Value = gs1ColIndex < row.Length
        //                    ? ReplaceGSCharacters((row[gs1ColIndex] ?? "").Trim())
        //                    : "";
        //                if (string.IsNullOrEmpty(gs1Value)) continue;

        //                lineCounter++;
        //                result.Add(new string[] { lineCounter.ToString(), "Waiting", gs1Value });
        //            }

        //            return result;
        //        }

        //        // ════════════════════════════════════════════════════════════════
        //        // CHẾ ĐỘ THƯỜNG — giữ nguyên tất cả các cột
        //        // ════════════════════════════════════════════════════════════════
        //        int columnCount = 0;
        //        int lineCounterStd = -1;

        //        if (isFirstRowHeader)
        //        {
        //            var tmp = new string[headerRow.Length + 2];
        //            tmp[0] = "Index";
        //            tmp[1] = "Status";
        //            for (int i = 2; i < tmp.Length; i++)
        //                tmp[i] = headerRow[i - 2] + $" - Field{i - 1}";
        //            columnCount = tmp.Length;
        //            result.Add(tmp);
        //        }
        //        else
        //        {
        //            var tmp = new string[headerRow.Length + 2];
        //            tmp[0] = "Index";
        //            tmp[1] = "Status";
        //            for (int i = 2; i < tmp.Length; i++)
        //                tmp[i] = $"Field{i - 1}";
        //            columnCount = tmp.Length;
        //            result.Add(tmp);
        //        }

        //        foreach (var row in allRows)
        //        {
        //            lineCounterStd++;
        //            string[] line = row;

        //            if (isFirstRowHeader)
        //            {
        //                var tmp1 = new string[columnCount];
        //                tmp1[0] = "" + lineCounterStd;
        //                tmp1[1] = "Waiting";
        //                for (int i = 2; i < tmp1.Length; i++)
        //                    tmp1[i] = (i - 2 < line.Length) ? ReplaceGSCharacters(line[i - 2]) : "";
        //                result.Add(tmp1);
        //            }
        //            else
        //            {
        //                var tmp1 = new string[columnCount];
        //                tmp1[0] = "" + (lineCounterStd + 1);
        //                tmp1[1] = "Waiting";
        //                for (int i = 2; i < tmp1.Length; i++)
        //                    tmp1[i] = (i - 2 < line.Length) ? line[i - 2] : "";
        //                result.Add(tmp1);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _InitDataErrorList.Add(InitDataError.DatabaseUnknownError);
        //        Debug.WriteLine($"Excel loading error: {ex.Message}");
        //    }

        //    return result;
        //}

        //private List<string[]> InitDatabaseFromCsv(string path, bool isFirstRowHeader)
        //{
        //    List<string[]> result = new List<string[]>();

        //    try
        //    {
        //        using (var reader = new StreamReader(path, Encoding.UTF8, true))
        //        {
        //            var rexCsvSplitter = path.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");
        //            int lineCounter = -1;
        //            int columnCount = 0;
        //            while (!reader.EndOfStream)
        //            {
        //                string[] line = rexCsvSplitter.Split(reader.ReadLine()).Select(x => Csv.Unescape(x)).ToArray();
        //                lineCounter++;
        //                if (lineCounter == 0)
        //                {
        //                    if (isFirstRowHeader)
        //                    {
        //                        var tmp = new string[line.Length + 2];
        //                        tmp[0] = "Index";
        //                        tmp[1] = "Status";
        //                        for (int i = 2; i < tmp.Length; i++)
        //                        {
        //                            tmp[i] = line[i - 2] + $" - Field{i - 1}";
        //                        }
        //                        columnCount = tmp.Length;
        //                        result.Add(tmp);
        //                    }
        //                    else
        //                    {
        //                        var tmp = new string[line.Length + 2];
        //                        tmp[0] = "Index";
        //                        tmp[1] = "Status";
        //                        for (int i = 2; i < tmp.Length; i++)
        //                        {
        //                            tmp[i] = $"Field{i - 1}";
        //                        }
        //                        columnCount = tmp.Length;
        //                        result.Add(tmp);
        //                    }

        //                }
        //                else
        //                {
        //                    if (isFirstRowHeader)
        //                    {
        //                        var tmp1 = new string[columnCount];
        //                        tmp1[0] = "" + lineCounter;
        //                        tmp1[1] = "Waiting";
        //                        for (int i = 2; i < tmp1.Length; i++)
        //                        {
        //                            if (i - 2 < line.Length)
        //                            {
        //                               // tmp1[i] = ReplaceGSCharacters(line[i - 2]);
        //                                tmp1[i] = line[i - 2];
        //                            }
        //                            else
        //                            {
        //                                tmp1[i] = "";
        //                            }
        //                        }
        //                        result.Add(tmp1);
        //                    }
        //                }

        //                if (!isFirstRowHeader)
        //                {
        //                    var tmp1 = new string[columnCount];
        //                    tmp1[0] = "" + (lineCounter + 1);
        //                    tmp1[1] = "Waiting";
        //                    for (int i = 2; i < tmp1.Length; i++)
        //                    {
        //                        if (i - 2 < line.Length)
        //                        {
        //                            tmp1[i] = line[i - 2];
        //                        }
        //                        else
        //                        {
        //                            tmp1[i] = "";
        //                        }
        //                    }
        //                    result.Add(tmp1);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        // Error handling is done in calling method
        //    }

        //    return result;
        //}

        private List<string[]> InitDatabaseFromExcel(string path, bool isFirstRowHeader)
        {
            List<string[]> result = new List<string[]>();

            try
            {
                // Đọc toàn bộ dữ liệu từ Excel vào List các mảng string
                List<string[]> allRows = FileFuncs.ReadExcelData(path, null);

                if (allRows == null || allRows.Count == 0)
                {
                    _InitDataErrorList.Add(InitDataError.DatabaseUnknownError);
                    return result;
                }

                // --- BƯỚC 1: TÌM HEADER (DROCO MODE) ---
                int gs1ColIndex = -1;
                int boxColIndex = -1;
                int cartonColIndex = -1;
                int headerRowIndex = -1;

                int scanLimit = Math.Min(allRows.Count, 10);
                for (int ri = 0; ri < scanLimit; ri++)
                {
                    string[] scanRow = allRows[ri];
                    int tempGs1 = -1, tempBox = -1, tempCarton = -1;

                    for (int ci = 0; ci < scanRow.Length; ci++)
                    {
                        string cellVal = (scanRow[ci] ?? "").Trim();
                        if (string.IsNullOrEmpty(cellVal)) continue;

                        if (string.Equals(cellVal, "Honest mark_UNIT", StringComparison.OrdinalIgnoreCase))
                            tempGs1 = ci;

                        if (cellVal.IndexOf("SSCC_MID", StringComparison.OrdinalIgnoreCase) >= 0
                            && cellVal.IndexOf("SSCC_MASTER", StringComparison.OrdinalIgnoreCase) < 0)
                            tempBox = ci;

                        if (cellVal.IndexOf("SSCC_MASTER", StringComparison.OrdinalIgnoreCase) >= 0)
                            tempCarton = ci;
                    }

                    if (tempGs1 >= 0 && tempBox >= 0 && tempGs1 != tempBox)
                    {
                        gs1ColIndex = tempGs1;
                        boxColIndex = tempBox;
                        cartonColIndex = tempCarton;
                        headerRowIndex = ri;
                        break;
                    }
                }

                // --- BƯỚC 2: XỬ LÝ DỮ LIỆU ---

                // THÀNH PHẦN A: CHẾ ĐỘ DROCO (Nếu tìm thấy Honest mark_UNIT)
                if (gs1ColIndex >= 0 && headerRowIndex >= 0)
                {
                    string colHeader = allRows[headerRowIndex][gs1ColIndex].Trim();
                    result.Add(new string[] { "Index", "Status", colHeader + " - Field1" });

                    for (int ri = headerRowIndex + 1; ri < allRows.Count; ri++)
                    {
                        string[] row = allRows[ri];
                        if (gs1ColIndex >= row.Length) continue;

                        // LẤY DỮ LIỆU GỐC: Không Trim quá đà, giữ nguyên ký tự GS1
                        string gs1Value = (row[gs1ColIndex] ?? "");

                        // QUAN TRỌNG: Nếu bạn thấy dữ liệu bị mất sau dấu phẩy, có thể Reader đã split nó sang cột bên cạnh (gs1ColIndex + 1)
                        // Bạn có thể kiểm tra: if (row.Length > gs1ColIndex + 1) gs1Value += row[gs1ColIndex + 1];

                        if (string.IsNullOrWhiteSpace(gs1Value)) continue;

                        result.Add(new string[] {
                    result.Count.ToString(),
                    "Waiting",
                    ReplaceGSCharacters(gs1Value)
                });
                    }
                    return result;
                }

                // THÀNH PHẦN B: CHẾ ĐỘ THƯỜNG (Giữ nguyên số cột)
                int startRow = isFirstRowHeader ? 1 : 0;
                string[] rawHeader = allRows[0];
                int columnCount = rawHeader.Length + 2;

                // Tạo Header cho Result
                var headerArr = new string[columnCount];
                headerArr[0] = "Index";
                headerArr[1] = "Status";
                for (int i = 2; i < columnCount; i++)
                    headerArr[i] = isFirstRowHeader ? $"{rawHeader[i - 2]} - Field{i - 1}" : $"Field{i - 1}";
                result.Add(headerArr);

                // Duyệt data
                for (int ri = startRow; ri < allRows.Count; ri++)
                {
                    string[] row = allRows[ri];
                    var dataRow = new string[columnCount];
                    dataRow[0] = result.Count.ToString();
                    dataRow[1] = "Waiting";

                    for (int i = 2; i < columnCount; i++)
                    {
                        if (i - 2 < row.Length)
                            dataRow[i] = ReplaceGSCharacters(row[i - 2]);
                        else
                            dataRow[i] = "";
                    }
                    result.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
                _InitDataErrorList.Add(InitDataError.DatabaseUnknownError);
                System.Diagnostics.Debug.WriteLine($"Excel loading error: {ex.Message}");
            }

            return result;
        }


        private List<string[]> InitDatabaseFromCsv(string path, bool isFirstRowHeader)
        {
            List<string[]> result = new List<string[]>();
            try
            {
                // Quan trọng: Phải dùng Encoding.UTF8 để nhận diện ký tự đặc biệt như \x1D
                using (var reader = new StreamReader(path, Encoding.UTF8))
                {
                    int lineCounter = 0;
                    while (!reader.EndOfStream)
                    {
                        string rawLine = reader.ReadLine();
                        if (string.IsNullOrEmpty(rawLine)) continue;

                        lineCounter++;

                        // Lần đầu tiên chạy: Tạo Header
                        if (result.Count == 0)
                        {
                            // Tạo 3 cột mặc định: Index, Status, và Nội dung Data
                            result.Add(new string[] { "Index", "Status", "Data Content - Field1" });

                            // Nếu dòng đầu là header thật sự của file, ta bỏ qua không add vào data
                            if (isFirstRowHeader) continue;
                        }

                        // Đổ dữ liệu: Coi toàn bộ rawLine là 1 giá trị duy nhất
                        // Không Split, không Regex để bảo toàn ký tự ! " ; : và \x1D
                        var dataRow = new string[3];
                        dataRow[0] = result.Count.ToString(); // Index
                        dataRow[1] = "Waiting";               // Status
                        dataRow[2] = rawLine;                 // Toàn bộ nội dung dòng

                        result.Add(dataRow);
                    }
                }
            }
            catch (Exception ex)
            {
                // Debug.WriteLine(ex.Message);
            }
            return result;
        }
        private void InitPrintedStatus(string path, List<string[]> list)
        {
            Stopwatch stw = Stopwatch.StartNew();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitPrintedStatus work on thread " + Environment.CurrentManagedThreadId);
            if (!File.Exists(path))
            {
                _InitDataErrorList.Add(InitDataError.CheckedResultDoNotExist);
                DialogResult dialogResult = CustomMessageBox.Show(Lang.CanNotFindPrintedResponse, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string extension = System.IO.Path.GetExtension(path).ToLower();

                // Check if it's an Excel file (.xlsx or .xls)
                if (extension == ".xlsx" || extension == ".xls")
                {
                    InitPrintedStatusFromExcel(path, list);
                }
                else
                {
                    InitPrintedStatusFromCsv(path, list);
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
        }

        private void InitPrintedStatusFromExcel(string path, List<string[]> list)
        {
            try
            {
                // Read Excel file, skip first row (header)
                List<string[]> allRows = FileFuncs.ReadExcelData(path, null);

                if (allRows != null && allRows.Count > 0)
                {
                    foreach (var row in allRows)
                    {
                        if (row != null && row.Length >= 2)
                        {
                            // using only index value to update printed status
                            string index = row[0];
                            string status = row[1];
                            if (int.TryParse(index, out int indexNumber) && indexNumber >= 0 && indexNumber < list.Count)
                            {
                                list[indexNumber][1] = "Printed";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show message box - ExcelDataReader handles errors gracefully
                Debug.WriteLine($"Error in InitPrintedStatusFromExcel: {ex.Message}");
            }
        }

        private void InitPrintedStatusFromCsv(string path, List<string[]> list)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path, Encoding.UTF8, true))
                {
                    int i = -1;
                    var rexCsvSplitter = path.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");

                    while (!reader.EndOfStream)
                    {
                        i++;
                        if (i == 0)
                        {
                            reader.ReadLine(); // Skip header row
                        }
                        else
                        {
                            // using only index value to update printed status
                            var dataLine = reader.ReadLine(); // Data: 1,Printed,....
                            if (string.IsNullOrWhiteSpace(dataLine))
                                continue;

                            var fields = rexCsvSplitter.Split(dataLine).Select(x => Csv.Unescape(x)).ToArray();
                            if (fields.Length >= 2)
                            {
                                string index = fields[0];
                                string status = fields[1];
                                if (int.TryParse(index, out int indexNumber) && indexNumber >= 0 && indexNumber < list.Count)
                                {
                                    list[indexNumber][1] = "Printed";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Error handling is done in calling method
            }
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
                string extension = System.IO.Path.GetExtension(path).ToLower();

                // Check if it's an Excel file (.xlsx or .xls)
                if (extension == ".xlsx" || extension == ".xls")
                {
                    // Read Excel file, skip first row (header)
                    List<string[]> allRows = FileFuncs.ReadExcelData(path, null);

                    if (allRows != null && allRows.Count > 0)
                    {
                        foreach (var row in allRows)
                        {
                            string[] line = row;
                            // ignore empty line
                            if (line.Length == 1 && line[0] == "") continue;
                            if (line.Length < _ColumnNames.Length)
                            {
                                var checkedResult = GetTheRightString(line);
                                result.Add(checkedResult);
                            }
                            else
                            {
                                result.Add(line);
                            }
                        }
                    }
                }
                else
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
                                if (line.Length < _ColumnNames.Length) // nay them -1 o day so sanh dc
                                {
                                    var checkedResult = GetTheRightString(line);
                                    result.Add(checkedResult);
                                }
                                else
                                {
                                    result.Add(line);
                                }
                            }
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

        private void InitCompareData(List<string[]> datas, List<string[]> result)
        {
            Stopwatch stw = Stopwatch.StartNew();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " InitCompareData work on thread " + Environment.CurrentManagedThreadId);

            // Use a HashSet instead of a List
            HashSet<string> _CheckedResultCodeSet = new HashSet<string>();

            // Populate the HashSet with the second element of each array
            var validCond = ComparisonResult.Valid.ToString();
            var columnCount = _ColumnNames.Count();// Thinh Note: Increase/Decrease the column count when change the _ColumnNames 
            try
            {
                foreach (var array in result)
                {
                    if (columnCount == array.Length && array[2] == validCond)
                    {
                        _CheckedResultCodeSet.Add(NormalizeDataForComparison(array[1]));
                    }
                }
                if (datas.Count > 0)
                {
                    int codeLenght = datas[0].Count() - 1;
                    for (int index = 0; index < datas.Count; index++)
                    {

                        string[] row = datas[index].Where((item, idx) => idx != 1).ToArray();
                        string data = GetCompareDataByPODFormat(row, _SelectedJob.PODFormat);
                        // Normalize data for consistent comparison (replace <0x1D> with \F)
                        data = NormalizeDataForComparison(data);

                        if (_CheckedResultCodeSet.Contains(data))
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

                            // Data use to update printed status for Verify and print - Compare mode
                            if (_IsVerifyAndPrintMode)
                            {
                                string tmp = "";
                                row = row.Skip(1).ToArray();
                                for (int i = 1; i <= row.Length; i++)
                                {
                                    var tmpPOD = Shared.Settings.PrintFieldForVerifyAndPrint.Find(x => x.Index == i);
                                    if (tmpPOD != null)
                                    {
                                        tmp += row[tmpPOD.Index - 1];
                                    }
                                }
                                var tryAdd2 = _Emergency.TryAdd(tmp, index);
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
        private void ActionChanged(object sender, EventArgs e)
        {
            if (sender == btnJob)
            {
                IsCloseButtonAction = false;
                Close();
            }
            else if (sender == btnStart)
            {
                _isRlinkStop = false;
                _ParentForm.Invoke_AutoAddSufixEvent();
                Thread.Sleep(300);
                StartProcess();
            }
            else if (sender == btnStop)
            {
                _isRlinkStop = true;
                StopProcessAsync(true, "", false, true);
            }
            else if (sender == btnDatabase || sender == pnlPrintedCode)
            {
                var isDatabaseDeny = _SelectedJob.CompareType == CompareType.Database && _TotalCode == 0;
                if (isDatabaseDeny)
                {
                    CustomMessageBox.Show(Lang.DatabaseDoesNotExist, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (_FormPreviewDatabase == null || _FormPreviewDatabase.IsDisposed)
                {
                    _FormPreviewDatabase = new FrmPreviewDatabase
                    {
                        _DatabaseColunms = new List<string>(_DatabaseColunms),
                        _ObtainCodeList = _PrintedCodeObtainFromFile.ToList(),
                        _TotalColumns = _TotalColumns,
                        _Totals = _TotalCode,
                        _NumberPrinted = NumberPrinted
                    };
                    _FormPreviewDatabase.Show();
                }
                else
                {
                    if (_FormPreviewDatabase.WindowState == FormWindowState.Minimized)
                    {
                        _FormPreviewDatabase.WindowState = FormWindowState.Normal;
                    }
                    _FormPreviewDatabase.Focus();
                    _FormPreviewDatabase.BringToFront();
                }
            }
            else if (sender == pnlCheckFailed || sender == pnlCheckPassed || sender == pnlTotalChecked)
            {
                if (_FormCheckedResult == null || _FormCheckedResult.IsDisposed)
                {
                    _FormCheckedResult = new FrmCheckedResultDroco
                    {
                        _IsAfterProduction = _IsAfterProductionMode,
                        _IsRSeries = _SelectedJob.PrinterSeries,
                        _ColumnNames = _ColumnNames.ToList(),
                        _CheckedResult = _CheckedResultCodeList.ToList(),
                        _CheckedData = _CodeListPODFormat,
                        _CodeData = _PrintedCodeObtainFromFile.ToList(),
                        _frmParent = this,
                        _TotalColumns = _ColumnNames.Count(),
                        _TotalCode = _TotalCode,
                        _NumberOfPrinted = NumberPrinted,
                        _TotalChecked = TotalChecked,
                        _NumberOfCheckedPassed = NumberOfCheckPassed,
                        _NumberOfCheckedFailed = (TotalChecked - NumberOfCheckPassed),
                        _JobName = _SelectedJob.FileName,
                        _PODFormat = _SelectedJob.PODFormat,
                        //_frmParent = this
                    };
                    // fillValue = 0: Load all
                    // fillValue = 1: Load passed result
                    // fillValue > 1: Load failed
                    if (sender == pnlCheckFailed)
                    {
                        _FormCheckedResult._FillValue = "Failed";
                    }
                    else if (sender == pnlCheckPassed)
                    {
                        _FormCheckedResult._FillValue = ComparisonResult.Valid.ToString();
                    }
                    else if (sender == pnlTotalChecked)
                    {
                        _FormCheckedResult._FillValue = "All";
                    }
                    _FormCheckedResult.Show();
                }
                else
                {
                    if (_FormCheckedResult != null)
                    {
                        _FormCheckedResult._CheckedResult = _CheckedResultCodeList.ToList();
                        _FormCheckedResult._CheckedData = _CodeListPODFormat;
                        _FormCheckedResult._CodeData = _PrintedCodeObtainFromFile.ToList();
                        _FormCheckedResult._NumberOfPrinted = NumberPrinted;
                        _FormCheckedResult._TotalChecked = TotalChecked;
                        _FormCheckedResult._NumberOfCheckedPassed = NumberOfCheckPassed;
                        _FormCheckedResult._NumberOfCheckedFailed = (TotalChecked - NumberOfCheckPassed);
                        if (sender == pnlCheckFailed)
                        {
                            _FormCheckedResult._FillValue = "Failed";
                        }
                        else if (sender == pnlCheckPassed)
                        {
                            _FormCheckedResult._FillValue = ComparisonResult.Valid.ToString();
                        }
                        else if (sender == pnlTotalChecked)
                        {
                            _FormCheckedResult._FillValue = "All";
                        }
                        _FormCheckedResult.Reload();
                        _FormCheckedResult.BringToFront();
                        _FormCheckedResult.Focus();
                        _FormCheckedResult.TopMost = true;
                    }
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
                    _FormSettings = new FrmSettings();
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
                // ExportDataAsync();
            }
            else if (sender == btnCustomExport)
            {
                ExportSharedEvents.RaiseHeaderGet(_DatabaseColunms);
                List<HeaderModel> headers = new List<HeaderModel>();
                List<(bool IsChecked, string Text, string orgText)> Results = new List<(bool IsChecked, string Text, string orgText)>();
                using (var child = new frmCusExport(_DatabaseColunms))
                {
                    if (child.ShowDialog() == DialogResult.OK)
                    {
                        CustomMessageBox.Show("Vui lòng dừng công việc trước khi xuất báo cáo!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        return;
                    }
                }
                foreach (var item in PODControl)
                {
                    headers.Add(new HeaderModel { IsChecked = item.IsChecked, OrgName = item.orgText, NewName = item.Text });
                }

                var exportConfigList = new List<ExportColumnConfig>();
                var firstItem = new ExportColumnConfig { OriginalName = "Index", DisplayName = "Index", IsExport = true, Index = 0 };

                exportConfigList.Add(firstItem);
                for (int i = 0; i < headers.Count; i++)
                {
                    var item = new ExportColumnConfig { OriginalName = headers[i].OrgName, DisplayName = headers[i].NewName, IsExport = headers[i].IsChecked, Index = i + 1 };
                    exportConfigList.Add(item);
                }

                CustomExportAllDataAsync(exportConfigList, filterChecked, mode);
            }
            else if (sender == btnExportAll)
            {
                ExportAllDataAsync();
            }
            else if (sender == btnExportResult)
            {
                string filePathCheckResult = CommVariables.PathCheckedResult + _SelectedJob.CheckedResultPath;
                ExportCheckedResultAsync(filePathCheckResult);
            }
            else if (sender == btnPrintCarton)
            {
                var item = cboQRBox.SelectedItem;
                if (item != null)
                {
                    string qrCode = item.ToString();
                    // Remove "(Đã in)" suffix if present to get the actual QR code
                    if (qrCode.EndsWith(" (Đã in)"))
                    {
                        qrCode = qrCode.Replace(" (Đã in)", "");
                    }
                    if (!Shared.Settings.ZebraPrinter.PODController.IsConnected())
                    {
                        CustomMessageBox.Show("Máy in không kết nối được. Vui lòng kiểm tra lại kết nối máy in Zebra.", Lang.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //   Shared.PrintZebra(qrCode);
                    Shared.CurrentJob.CartonList.Find(x => x.QrCode == qrCode).sentToPrinter = true;
                    Shared.CurrentJob.SaveFile();

                    // Find and mark the carton as sentToPrinter
                    var carton = _SelectedJob.CartonList.FirstOrDefault(c => c.QrCode == qrCode);
                    if (carton != null)
                    {
                        carton.sentToPrinter = true;

                        // Update the combo box display
                        UIControlsFuncs.UI(cboQRBox, () =>
                        {
                            int selectedIndex = cboQRBox.SelectedIndex;
                            cboQRBox.Items.Clear();
                            cboQRBox.Items.AddRange(_SelectedJob.CartonList.Select(p => p.sentToPrinter ? $"{p.QrCode} (Đã in)" : p.QrCode).Reverse().ToArray());
                            // Try to maintain selection
                            if (selectedIndex >= 0 && selectedIndex < cboQRBox.Items.Count)
                            {
                                cboQRBox.SelectedIndex = selectedIndex;
                            }
                        });
                    }
                }
            }
        }

        public void ExportCustomAll()
        {
            List<HeaderModel> headers = new List<HeaderModel>();

            foreach (var item in PODControl)
            {
                headers.Add(new HeaderModel { IsChecked = item.IsChecked, OrgName = item.orgText, NewName = item.Text });
            }

            var exportConfigList = new List<ExportColumnConfig>();
            var firstItem = new ExportColumnConfig { OriginalName = "Index", DisplayName = "Index", IsExport = true, Index = 0 };

            exportConfigList.Add(firstItem);
            for (int i = 0; i < headers.Count; i++)
            {
                var item = new ExportColumnConfig { OriginalName = headers[i].OrgName, DisplayName = headers[i].NewName, IsExport = headers[i].IsChecked, Index = i + 1 };
                exportConfigList.Add(item);
            }

            CustomExportAllDataAsync(exportConfigList, filterChecked, mode);
        }

        public async void CustomExportAllDataAsync(List<ExportColumnConfig> cf, FilterChecked fchk, ExportMode exportMode)
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
                await Task.Run(() => { CustomExportData(sfd.FileName, cf, fchk, exportMode); });
                EnableUIComponentWhenLoadData(true);
            }
        }

        public void CustomExportData(string fileName,
        List<ExportColumnConfig> exportConfigList,
        FilterChecked filterChecked,
        ExportMode exportMode = ExportMode.ExportAll,
        string checkedFilter = null)
        {
            try
            {
                List<string> linesToWrite = new List<string>();

                // Check by key - Value => key is data compare and value is duplicate count
                var duplicateCountDict = _CheckedResultCodeList
                .GroupBy(x => x[Index_ResultData])
                .ToDictionary(g => g.Key, g => g.Count());

                switch (exportMode)
                {
                    // Export tong hop
                    case ExportMode.ExportAll:
                        filterChecked.filterStatus = null;
                        filterChecked.filterDevice = null;
                        break;
                    //Export chi dua theo db
                    case ExportMode.ExportDatabase:

                        break;
                    // Export theo ket qua
                    case ExportMode.ExportResult:

                        break;
                    default:
                        break;
                }

                // Lọc ra những code đã kiểm tra, nếu trùng chỉ lấy lần đầu
                // Data + Date Time Check + Position + Device
                var checkedResultDict = _CheckedResultCodeList
                 .Where(item =>
                     (filterChecked.filterStatus == null || (!string.IsNullOrEmpty(item[2]) && item[2] == filterChecked.filterStatus)) && // Lọc theo Status (index 1)
                     (filterChecked.filterDevice == null || (!string.IsNullOrEmpty(item[7]) && item[7] == filterChecked.filterDevice))) // Lọc theo Device (index 6)
                 .GroupBy(x => x[Index_ResultData]) // Nhóm theo Index_ResultData (index 0)
                 .ToDictionary(
                     g => g.Key
                 //g => (DateTime: g.First()[Index_DateTime], Position: g.First()[Index_Position], Device: g.First()[Index_Device], Result: g.First()[Index_Result])
                 );


                #region Check Result logic

                List<string> deviceListRes = new List<string>();
                List<string> sampleListRes = new List<string>();

                if (deviceFilterCheckRes == "All")
                {
                    deviceListRes.Clear();
                    deviceListRes.Add("Barcode Scanner");
                    deviceListRes.Add("Camera");
                }
                else if (deviceFilterCheckRes == "Camera")
                {
                    deviceListRes.Clear();
                    deviceListRes.Add("Camera");
                }
                else if (deviceFilterCheckRes == "Barcode Scanner")
                {
                    deviceListRes.Clear();
                    deviceListRes.Add("Barcode Scanner");
                }

                if (sampleFilterCheckRes == "All")
                {
                    sampleListRes.Clear();
                    sampleListRes.Add("True");
                    sampleListRes.Add("False");
                }
                else if (sampleFilterCheckRes == "SAMPLE")
                {
                    sampleListRes.Clear();
                    sampleListRes.Add("True");
                }
                else if (sampleFilterCheckRes == "NOT SAMPLE")
                {
                    sampleListRes.Clear();
                    sampleListRes.Add("False");
                }

                var copy_CheckedResultCodeList = _CheckedResultCodeList.ShallowClone();

                foreach (string[] item in copy_CheckedResultCodeList)
                {
                    if (item[8] != null && item[8] == " ")
                    {
                        item[8] = "False";
                    }
                }

                var checkedResultDictForCheck = copy_CheckedResultCodeList
                        .Where(item =>
                            ((!string.IsNullOrEmpty(item[2]) && checkedResult.Any(x => x == item[2]))) && // Lọc theo Status (index 1)
                            ((!string.IsNullOrEmpty(item[7]) && deviceListRes.Any(x => x == item[7]))) && // Lọc theo Device (index 6)
                             (sampleListRes.Any(x => x == item[8])) // thiết bị


                        ) // Lọc theo Device (index 6)
                    .GroupBy(x => x[Index_ResultData]) // Nhóm theo Index_ResultData (index 0)
                    .ToDictionary(
                        g => g.Key
                    //g => (DateTime: g.First()[Index_DateTime], Position: g.First()[Index_Position], Device: g.First()[Index_Device], Result: g.First()[Index_Result], Sampled: g.First()[Index_Sampled])
                    );

                #endregion Check Result Logic

                //if (exportMode == ExportMode.ExportAll)
                //{

                //    var test2 = _CheckedResultCodeList
                //             .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] != "Valid" && arr[Index_Result] != "Duplicated")
                //             .GroupBy(x => x[Index_ResultData])
                //             .ToDictionary(g => g.Key, g => g.First()[Index_DateTime]);

                //    //Console.WriteLine($"Element 16: Index_ResultData={_CheckedResultCodeList[15][Index_ResultData]}, Index_Device={_CheckedResultCodeList[15][Index_Device]}, Index_Result={_CheckedResultCodeList[15][Index_Result]}");

                //    // Danh sách code lỗi chỉ tính camera
                //    var failedList = _CheckedResultCodeList
                //                                .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] != "Valid")
                //                                .Select(arr => new KeyValuePair<string, string>(arr[Index_ResultData], arr[Index_DateTime]))
                //                                .ToList();

                //    // Xoa file truoc khi tao moi
                //    if (File.Exists(fileName))
                //        File.Delete(fileName);

                //    // ==== Build Header theo Config ====
                //    List<ExportColumnConfig> selectedColumns = exportConfigList.Where(c => c.IsExport).ToList(); // Tiêu đề custom chỉ lấy các cột được hiện
                //    string header = string.Join(",", selectedColumns.Select(c => Csv.Escape(c.DisplayName))); // Nối tiêu đề với nhau

                //    // thinh_header
                //    header += (CheckedHeaderList.isStatusChecked ? "," + otherHeader.statusHeader : "") +
                //               (CheckedHeaderList.isCheckingCodeChecked ? "," + otherHeader.numCheckHeader : "") +
                //              (CheckedHeaderList.isPositionChecked ? "," + otherHeader.positionHeader : "") +
                //              (CheckedHeaderList.isVerifyDateChecked ? "," + otherHeader.verifydateHeader : "")
                //               + (CheckedHeaderList.isDeviceChecked ? "," + otherHeader.deviceHeader : "");
                //    linesToWrite.Add(header); // Ghi tiêu đề cột - ghi file


                //    // ==== Build Record & Export All base on Database====
                //    for (int i = 0; i < _TotalCode; i++)
                //    {
                //        var record = _PrintedCodeObtainFromFile[i];

                //        // Thực hiện đưa cột trạng thái xuống dưới cùng
                //        if (record.Length > 1)
                //        {
                //            var newRecord = new string[record.Length];
                //            Array.Copy(record, 0, newRecord, 0, 1);
                //            Array.Copy(record, 2, newRecord, 1, record.Length - 2);
                //            newRecord[newRecord.Length - 1] = record[1];
                //            record = newRecord;
                //        }

                //        var compareString = GetCompareDataByPODFormat(record, _SelectedJob.PODFormat); // Nội dung chính POD cần check

                //        // Thêm các dữ liệu vào tương ứng các cột custom
                //        var values = new List<string>();
                //        foreach (var col in selectedColumns)
                //        {
                //            string val = (col.Index < record.Length) ? record[col.Index] : "";
                //            values.Add(Csv.Escape(val));
                //        }

                //        var status = record[record.Length - 1]; // Lấy ra trạng thái của database (Printed hoặc waiting)

                //        bool isChecked = checkedResultDict.TryGetValue(compareString, out var checkedValues); // Kiểm tra dữ liệu này đã check chưa

                //        int duplicateCount = duplicateCountDict.ContainsKey(compareString) ? duplicateCountDict[compareString] : 1; // Nếu check rồi thì kiểm tra có trùng không

                //        if (isChecked) // Nếu đã được check
                //        {
                //            if (status.Equals("Printed")) // Nêu đã được in
                //            {
                //                if (duplicateCount > 1) // Nếu có trùng
                //                {
                //                    if (CheckedHeaderList.isStatusChecked) values.Add(PrintedDuplicate); //PrintedDuplicate
                //                    if (CheckedHeaderList.isCheckingCodeChecked) values.Add(duplicateCount.ToString());
                //                }
                //                else // Nếu không trùng thì Đã in và đã Check kèm 
                //                {
                //                    if (CheckedHeaderList.isStatusChecked) values.Add(PrintedVerified); //PrintedVerified
                //                    if (CheckedHeaderList.isCheckingCodeChecked) values.Add("1");
                //                }
                //            }
                //            else // Nếu chưa được in mà được check thì có vấn đề
                //            {
                //                var index = failedList.FindIndex(kvp => kvp.Key == compareString);
                //                if (index != -1)
                //                    failedList.RemoveAt(index);

                //                if (CheckedHeaderList.isStatusChecked) values.Add(UnprintedChecked); //UnprintedChecked
                //                if (CheckedHeaderList.isCheckingCodeChecked) values.Add("1");
                //            }

                //            checkedResultDict.Remove(compareString);
                //        }
                //        else // Nếu chưa được check
                //        {
                //            if (status.Equals("Printed")) // Đã in
                //            {
                //                if (duplicateCount > 1) // Kiểm tra có trùng
                //                {
                //                    if (CheckedHeaderList.isStatusChecked) values.Add(PrintedDuplicate); // đánh dấu trùng PrintedDuplicate
                //                    if (CheckedHeaderList.isCheckingCodeChecked) values.Add(duplicateCount.ToString()); // đếm số trùng
                //                }
                //                else
                //                {
                //                    if (CheckedHeaderList.isStatusChecked) values.Add(PrintedUnverified); // PrintedUnverified ngược lại không có duplicate mà đã in thì là Printed và chưa kiểm tra
                //                }
                //            }
                //            else // Nếu chưa in và cũng chưa kiểm ra 
                //            {
                //                if (CheckedHeaderList.isStatusChecked) values.Add(UnprintedUnverified); //UnprintedUnverified
                //            }
                //            if (CheckedHeaderList.isCheckingCodeChecked) values.Add("");// Number Checking Code : Số lượng check cũng không có
                //            if (CheckedHeaderList.isPositionChecked) values.Add(""); // Position : Kiểm tra postion sẽ không có
                //            if (CheckedHeaderList.isVerifyDateChecked) values.Add(""); // VerifyDate : ngày xác minh check sẽ rỗng
                //        }


                //        // Chỉ thêm các cột bổ sung nếu có giá trị, tránh thêm rỗng
                //        if (isChecked)
                //        {
                //            if (CheckedHeaderList.isPositionChecked) values.Add(Csv.Escape(checkedValues.Position).Trim('"')); // Is Position check
                //            if (CheckedHeaderList.isVerifyDateChecked) values.Add(Csv.Escape(checkedValues.DateTime)); // Datetime
                //            if (CheckedHeaderList.isDeviceChecked) values.Add(Csv.Escape(checkedValues.Device)); // Device
                //        }
                //        else
                //        {
                //            // Không thêm các cột rỗng, để chúng tự động bị bỏ qua
                //        }

                //        // Loại bỏ các phần tử rỗng trước khi nối
                //        // Khi lọc theo trạnh thái có kết hợp device thì sẽ kết hợp điều kiện nếu chọn all thì nhận hết
                //        int indexSts = (exportConfigList.Where(x => x.IsExport == true).Count());
                //        int indexDevice = values.Count - 1;

                //        bool isWantedSts = values
                //            .Any(item => /* string.IsNullOrEmpty(filterChecked.filterStatusDb) ||*/ !string.IsNullOrEmpty(item) &&
                //            values.IndexOf(item) == indexSts &&
                //            statusDb.Any(s => s.Equals(item))); // Lọc trạng thái

                //        bool isWantedDevice = values
                //          .Any(item => string.IsNullOrEmpty(filterChecked.filterDeviceDb) ||
                //          !string.IsNullOrEmpty(item) && values.IndexOf(item) == indexDevice && item == filterChecked.filterDeviceDb); // Lọc thiết bị

                //        if (!isWantedSts || !isWantedDevice) continue; // Nếu các trường không nằm trong mục lọc thì bỏ qua

                //        // Đổi tên các trạng thái thành trạng thái người dùng mong muốn
                //        if (values.Count > indexSts && values[indexSts] == PrintedVerified)
                //        {
                //            values[indexSts] = customStatusValue.PV;
                //        }
                //        if (values.Count > indexSts && values[indexSts] == PrintedUnverified)
                //        {
                //            values[indexSts] = customStatusValue.PU;
                //        }
                //        if (values.Count > indexSts && values[indexSts] == PrintedDuplicate)
                //        {
                //            values[indexSts] = customStatusValue.PD;
                //        }
                //        if (values.Count > indexSts && values[indexSts] == UnprintedUnverified)
                //        {
                //            values[indexSts] = customStatusValue.UU;
                //        }
                //        if (values.Count > indexSts && values[indexSts] == UnprintedChecked)
                //        {
                //            values[indexSts] = customStatusValue.UC;
                //        }

                //        values = values
                //            .Where(v => !string.IsNullOrEmpty(v))
                //            .ToList();

                //        linesToWrite.Add(string.Join(",", values));
                //    }
                //}

                //if (exportMode == ExportMode.ExportResult)
                //{
                //    //
                //    // Xoa file truoc khi tao moi
                //    if (File.Exists(fileName))
                //        File.Delete(fileName);
                //    string headers = "";

                //    headers += "Index" + "," +
                //                CheckedExportHeader.DataHeader + "," +
                //                CheckedExportHeader.DateVerifyHeader + "," +
                //                CheckedExportHeader.IsPositionHeader + "," +
                //                CheckedExportHeader.DeviceNameHeader + "," +
                //                CheckedExportHeader.ResultHeader + "," +
                //                CheckedExportHeader.SampledHeader;


                //    linesToWrite.Add(headers); // Ghi tiêu đề cột - ghi file
                //                               // Prepare data for CSV
                //                               // Prepare data for CSV
                //    int indexAll = 1;




                //    foreach (var kvp in checkedResultDictForCheck)
                //    {
                //        var index = indexAll; // Use the dictionary key as the index
                //        var (DateTime, Position, Device, Result, Sampled) = kvp.Value; // Deconstruct the value tuple
                //        var data = kvp.Key; // Assuming the key is the data field
                //        var date = DateTime;
                //        var isposition = Position.ToString();
                //        var device = Device.ToString();

                //        var sampled = Sampled.ToString();

                //        if (Result == "Valid")
                //        {
                //            Result = checkedExportResult.Valid;
                //        }
                //        if (Result == "Invalided")
                //        {
                //            Result = checkedExportResult.Invalided;
                //        }
                //        if (Result == "Duplicated")
                //        {
                //            Result = checkedExportResult.Duplicated;
                //        }
                //        if (Result == "Null")
                //        {
                //            Result = checkedExportResult.Null;
                //        }
                //        if (Result == "Missed")
                //        {
                //            Result = checkedExportResult.Missed;
                //        }
                //        var result = Result.ToString();

                //        linesToWrite.Add($"{index},{data},{date},{isposition},{device},{result},{sampled}");

                //        indexAll++;
                //    }

                //}
                //// ==== Write file ====
                //if (fileName.EndsWith(".pdf"))
                //{
                //    ConvertCsvToPdf(linesToWrite.ToArray(), fileName);
                //}
                //else
                //{
                //    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                //    {
                //        foreach (var line in linesToWrite)
                //        {
                //            writer.WriteLine(line);
                //        }
                //    }
                //}

                MoveToTheFile(fileName);
                checkedResultDict.Clear();
            }
            catch (Exception ex)
            {
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        private void Shared_OnCameraReadDataChange(object sender, EventArgs e)
        {
            if ((Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing) || _IsReCheck) return;
            try
            {
                if (sender is DetectModel)
                {
                    var detectModel = sender as DetectModel;

                    // Normalize camera data for consistent comparison (replace <0x1D> with \F)
                    if (!string.IsNullOrEmpty(detectModel.Text))
                    {
                        detectModel.Text = NormalizeDataForComparison(detectModel.Text);
                    }

                    switch (Shared.Settings.CameraList.FirstOrDefault().CameraType)
                    {
                        case CameraType.UKN:
                            break;
                        case CameraType.DM:
                        case CameraType.IS:
                        case CameraType.ISDual:
                        case CameraType.CV_X:
                            if (Shared.Settings.Position == SettingsModel.PositionType.BarcodePosition && Shared.Settings.EnablePosition)
                            {
                                break;
                            }
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
            if ((Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)) return;

            try
            {
                if (sender is DetectModel detectModel)
                {
                    if (!string.IsNullOrEmpty(detectModel.Text))
                    {
                        detectModel.Text = NormalizeDataForComparison(detectModel.Text);
                    }

                    // Nếu frmReprint đang mở thì nhường scanner cho frmReprint
                    if (_isReprintFormOpen) return;

                    // ReCheck / GetSample mode: ưu tiên compare queue, không đóng gói
                    if (_IsReCheck)
                    {
                        _QueueBufferDataObtained.Enqueue(detectModel);
                        return;
                    }

                    // Chế độ đóng gói bình thường
                    if (_packagingManager != null)
                    {
                        _packagingManager.ProcessAnyScan(detectModel.Text);
                        return;
                    }
                }
            }
            catch (Exception) { }
        }
        //private void Shared_OnSerialDeviceReadDataChange(object sender, EventArgs e)
        //{
        //    if ((Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)) return;

        //    try
        //    {
        //        if (sender is DetectModel)
        //        {
        //            var detectModel = sender as DetectModel;

        //            if (!string.IsNullOrEmpty(detectModel.Text))
        //            {
        //                detectModel.Text = NormalizeDataForComparison(detectModel.Text);
        //            }

        //            // === Xử lý quét Scanner cho đóng gói 4 cấp ===
        //            if (_packagingManager != null && _packagingManager.CurrentScanState != DrocoPackagingManager.ScanWaitState.None)
        //            {
        //                string scannedCode = detectModel.Text;
        //                switch (_packagingManager.CurrentScanState)
        //                {
        //                    case DrocoPackagingManager.ScanWaitState.WaitingForBoxScan:
        //                        _packagingManager.ProcessBoxScan(scannedCode);
        //                        return;
        //                    case DrocoPackagingManager.ScanWaitState.WaitingForCartonScan:
        //                        _packagingManager.ProcessCartonScan(scannedCode);
        //                        return;
        //                    case DrocoPackagingManager.ScanWaitState.WaitingForPalletScan:
        //                        _packagingManager.ProcessPalletScan(scannedCode);
        //                        return;
        //                }
        //            }

        //            // === Xử lý ReCheck bình thường ===
        //            if (!_IsReCheck) return;
        //            _QueueBufferDataObtained.Enqueue(detectModel);
        //        }
        //    }
        //    catch (Exception) { }
        //}

        //private void Shared_OnSerialDeviceReadDataChange(object sender, EventArgs e)
        //{
        //    if ((Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing) || !_IsReCheck) return;
        //    try
        //    {
        //        if (sender is DetectModel)
        //        {
        //            var detectModel = sender as DetectModel;

        //            // Normalize camera data for consistent comparison (replace <0x1D> with \F)
        //            if (!string.IsNullOrEmpty(detectModel.Text))
        //            {
        //                detectModel.Text = NormalizeDataForComparison(detectModel.Text);
        //            }

        //            _QueueBufferDataObtained.Enqueue(detectModel);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

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
            // ChangePictureCamera();
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
                                    if (PODResponseModel.Status != null && PODResponseModel.Status == "RYES")
                                    {
                                        if (ReceivedCode < 100)
                                        {
                                            lock (_ReceiveLocker)
                                            {
                                                Monitor.Pulse(_ReceiveLocker); // Notify that printer was received data
                                            }
                                        }

                                        if (Shared.OperStatus != OperationStatus.Stopped) ReceivedCode++;

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
                                    break;
                                case "RSFP":
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

                                        // Keep stable this for another task
                                        lock (_PrintLocker)
                                        {
                                            _IsPrintedWait = false;
                                            Monitor.Pulse(_PrintLocker);
                                        }

                                    }

                                    //Receive data: RSFP;1/101;DATA; check.pvcfc.com.vn/?id=L927GCCR72;L927GCCR72;0;0;1
                                    pODcommand = pODcommand.Skip(3).ToArray();
                                    string printedResult = "";
                                    _QueueBufferBackupRSFPLog.Enqueue(ArrayAddOneElement(pODcommand, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))); // Add to queue log
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

                                    _QueueBufferUpdateUIPrinter.Enqueue(printedResult);

                                    break;

                                case "STAR":
                                    PODResponseModel.Command = pODcommand[0];
                                    PODResponseModel.Status = pODcommand[1];
                                    if (PODResponseModel.Status != null && (PODResponseModel.Status == "OK" || PODResponseModel.Status == "READY"))
                                    {
                                        //if (podDataModel.RoleOfPrinter == RoleOfStation.ForProduct && !_IsVerifyAndPrintMode)
                                        //{
                                        //    SendDataToPrinterAsync(); // Send POD data to printer when printer ready receive data
                                        //}
                                        if (podDataModel.RoleOfPrinter == RoleOfStation.ForProduct && !_IsVerifyAndPrintMode)
                                        {
                                            // FIX: Chỉ gọi khi hệ thống ĐANG chạy
                                            // Tránh máy in phản hồi trễ sau Stop → gửi data lại
                                            if (Shared.OperStatus == OperationStatus.Running ||
                                                Shared.OperStatus == OperationStatus.Processing)
                                            {
                                                SendDataToPrinterAsync();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        PODResponseModel.Error = pODcommand[2];
                                        var message = "Unknown";
                                        switch (PODResponseModel.Error)
                                        {
                                            case "000": message = "Unknown"; break;
                                            case "001": message = "Open templates failed (dose not exist, others templates being opening,...)"; break;
                                            case "002": message = "Start pages, End pages is invalid"; break;
                                            case "003": message = "No printhead is selected"; break;
                                            case "004": message = "Speed limit"; break;
                                            case "005": message = "Printhead disconnected"; break;
                                            case "006": message = "Unknown printhead"; break;
                                            case "007": message = "No cartridges"; break;
                                            case "008": message = "Invalid cartridges"; break;
                                            case "009": message = "Out of ink"; break;
                                            case "010": message = "Cartridges is locked"; break;
                                            case "011": message = "Invalid version"; break;
                                            case "012": message = "Incorrect printhead"; break;
                                            case "013": message = "Start print processing"; break;
                                            case "014": message = "Invalid loop values"; break;
                                            case "015": message = "Ink low"; break;


                                            default:
                                                break;
                                        }

                                        Invoke(new Action(() =>
                                        {
                                            StopProcessAsync(false, Lang.SomePrintParametersAreMissing + ": " + message, false, true);
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
                                    break;
                                case "MON":
                                    PODResponseModel.Status = pODcommand[3];
                                    if (PODResponseModel.Status == "Stop" &&
                                        Shared.OperStatus == OperationStatus.Running &&
                                        _SelectedJob.CompareType == CompareType.Database &&
                                        _SelectedJob.JobType != JobType.StandAlone &&
                                        !_IsReCheck &&
                                        !_isRlinkStop)
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            StopProcessAsync(false, "Printer stops suddenly!", false, true);
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
                                        case "Connected": _PrinterStatus = PrinterStatus.Connected; ; break;
                                        case "Disconnected": _PrinterStatus = PrinterStatus.Disconnected; break;
                                        case "Error": _PrinterStatus = PrinterStatus.Error; ; break;
                                        case "Disable": _PrinterStatus = PrinterStatus.Disable; break;
                                        case "": _PrinterStatus = PrinterStatus.Null; break;
                                        default:
                                            break;
                                    }
                                    break;
                                default:
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
            _QueueBufferPrinterResponseData.Enqueue(sender);
        }
        private volatile bool _isSendingDataToPrinter = false;

        private volatile bool _isReprintFormOpen = false;
        //private async void SendDataToPrinterAsync()
        //{
        //    _SendDataToPrinterTokenCTS = new CancellationTokenSource();
        //    var token = _SendDataToPrinterTokenCTS.Token;
        //    await Task.Run(() => { SendPODDataProductToPrinter(token); });
        //}
        private async void SendDataToPrinterAsync()
        {
            // FIX: Tránh 2 task gửi data chạy song song khi start/stop nhanh
            if (_isSendingDataToPrinter) return;
            _isSendingDataToPrinter = true;

            _SendDataToPrinterTokenCTS = new CancellationTokenSource();
            var token = _SendDataToPrinterTokenCTS.Token;
            try
            {
                await Task.Run(() => { SendPODDataProductToPrinter(token); });
            }
            finally
            {
                _isSendingDataToPrinter = false;
            }
        }
        private void SendPODDataProductToPrinter(CancellationToken token)
        {
            Thread.Sleep(500);
            int counter = 0;
            List<string[]> codeList = null;
            List<string> tmpListLog = new List<string>();

            lock (_SyncObjCodeList)
            {
                codeList = new List<string[]>(_PrintedCodeObtainFromFile);
            }
            lock (_PrintLocker)
            {
                _IsPrintedWait = false;
                _PrintedResult = ComparisonResult.Valid;
            }

            // Xóa queue feedback cũ còn sót từ lần chạy trước
            while (_queueCountFeedback.TryDequeue(out _)) { }

            try
            {
                var spinWait = new SpinWait();
                int startIndex = codeList.FindIndex(x => x[1] != "Printed");
                if (startIndex == -1) return;

                _IsPrintedWait = true;

                for (int codeIndex = startIndex; codeIndex < codeList.Count(); codeIndex++)
                {
                    token.ThrowIfCancellationRequested();
                    string[] codeModel = codeList[codeIndex];
                    int statusIndex = 1;

                    if (codeModel[statusIndex] != "Printed" && (codeModel[statusIndex] != "Duplicate" || Shared.Settings.DuplicatedDBEnable))
                    {
                        token.ThrowIfCancellationRequested();

                        string data = string.Join(
                                                  Shared.Settings.SplitCharacter.ToString(),
                                                  codeModel.Skip(2).Select(x => ReplaceAllGS1Separators(x ?? "")));

                        {
                            string[] rowWithoutStatus = codeModel.Where((item, idx) => idx != 1).ToArray();
                            string checkKey = NormalizeDataForComparison(GetCompareDataByPODFormat(rowWithoutStatus, _SelectedJob.PODFormat));
                            if (_CodeListPODFormat.TryGetValue(checkKey, out CompareStatus cs) && cs.Status)
                            {
                                codeModel[statusIndex] = "Printed";
                                NumberPrinted++;
                                continue;
                            }
                        }

                        string command = $"DATA;{data}";

                        if (podController != null)
                        {
                            token.ThrowIfCancellationRequested();
                            if (Shared.OperStatus == OperationStatus.Stopped)
                                throw new OperationCanceledException();

                            podController.Send(command);
                            NumberOfSentPrinter++;
                            tmpListLog = command.Split(Shared.Settings.SplitCharacter).Skip(1).ToList();
                            tmpListLog.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                            _QueueBufferBackupSendLog.Enqueue(tmpListLog.ToArray());
                            tmpListLog.Clear();
                        }

                        counter++;

                        if (Shared.OperStatus == OperationStatus.Processing)
                        {
                            if (counter >= Shared.Settings.PrinterList[0].NumberBuffer1StSend && _IsAfterProductionMode)
                            {
                                Shared.OperStatus = OperationStatus.Running;
                                Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
                                EnableUIComponent(Shared.OperStatus);
                            }
                            else if (counter >= 1 && _IsOnProductionMode)
                            {
                                Shared.OperStatus = OperationStatus.Running;
                                Shared.RaiseOnOperationStatusChangeEvent(Shared.OperStatus);
                                EnableUIComponent(Shared.OperStatus);
                            }
                        }

                        //if (_IsOnProductionMode)
                        //{
                        //    lock (_PrintLocker)
                        //    {
                        //        _IsPrintedWait = true;
                        //        while (_IsPrintedWait) Monitor.Wait(_PrintLocker);
                        //        if (_PrintedResult != ComparisonResult.Valid && _PrintedResult != ComparisonResult.Duplicated)
                        //        {
                        //            codeIndex--;
                        //        }
                        //    }
                        //}
                        if (_IsOnProductionMode)
                        {
                            lock (_PrintLocker)
                            {
                                _IsPrintedWait = true;
                                // FIX: Thêm timeout 10s để tránh block vô hạn khi printer không phản hồi
                                int waitTimeout = 10000;
                                int waited = 0;
                                while (_IsPrintedWait && waited < waitTimeout)
                                {
                                    Monitor.Wait(_PrintLocker, 1000);
                                    waited += 1000;
                                    token.ThrowIfCancellationRequested();
                                }
                                if (_PrintedResult != ComparisonResult.Valid && _PrintedResult != ComparisonResult.Duplicated)
                                {
                                    codeIndex--;
                                }
                            }
                        }
                        else if (_IsAfterProductionMode)
                        {
                            if (counter < Shared.Settings.PrinterList[0].NumberBuffer1StSend)
                            {
                                Thread.Sleep(Shared.Settings.PrinterList[0].TimeDelaySendFirstBuffer);
                            }
                            else
                            {
                                int timeoutMs   = 10000;
                                int elapsedMs   = 0;
                                bool gotFeedback = false;

                                while (!gotFeedback)
                                {
                                    token.ThrowIfCancellationRequested();

                                    if (_queueCountFeedback.TryDequeue(out int _))
                                    {
                                        gotFeedback = true;
                                        break;
                                    }

                                    if (!Shared.Settings.PrinterList[0].EnableSendTurboSpeed)
                                    {
                                        Thread.Sleep(10);
                                        elapsedMs += 10;
                                    }
                                    else
                                    {
                                        spinWait.SpinOnce();
                                        elapsedMs += 1;
                                    }

                                    if (elapsedMs >= timeoutMs)
                                    {
                                        break;
                                    }
                                }
                            }

                            // FIX: Bỏ chặn vòng lặp gửi dữ liệu bởi CurrentScanState
                            // PackagingManager xử lý scan độc lập qua Shared_OnSerialDeviceReadDataChange
                            // KHÔNG chặn thread gửi dữ liệu tại đây
                            // (Đây là root cause khi Start lại: CurrentScanState != None → chặn mãi)
                        }
                    }
                }

                if (Shared.OperStatus == OperationStatus.Processing)
                {
                    Shared.OperStatus = OperationStatus.Running;
                    EnableUIComponent(Shared.OperStatus);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Thread send data to printer was stopped!");
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
                Console.WriteLine("Thread send data to printer was error!");
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

        //private void KillTThreadSendPODDataToPrinter()
        //{
        //    ReleaseLocker();
        //    _SendDataToPrinterTokenCTS?.Cancel();
        //}
        //private void KillTThreadSendPODDataToPrinter()
        //{
        //    ReleaseLocker();
        //    try
        //    {
        //        _SendDataToPrinterTokenCTS?.Cancel();
        //        _SendDataToPrinterTokenCTS?.Dispose();
        //        _SendDataToPrinterTokenCTS = null;
        //    }
        //    catch (ObjectDisposedException) { }
        //}
        private void KillTThreadSendPODDataToPrinter()
        {
            _isSendingDataToPrinter = false; // Reset flag trước
            ReleaseLocker();
            try
            {
                _SendDataToPrinterTokenCTS?.Cancel();
                _SendDataToPrinterTokenCTS?.Dispose();
                _SendDataToPrinterTokenCTS = null;
            }
            catch (ObjectDisposedException) { }
        }
        public async void ReprintAsync()
        {
            if (Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)
            {
                //await Task.Run(() => { Reprint(); });
                Reprint();
            }
        }

        public void ReCheck()
        {
            if (Shared.SerialDevController.IsSerialDevConnected())
            {
                if (Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)
                {
                    if (_FormCheckedResult != null)
                    {
                        Invoke(new Action(() => { _FormCheckedResult.Close(); }));
                    }

                    var dialogResult = CustomMessageBox.Show(Lang.ReCheckConfirm, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Yes)
                    {
                        ChangeCheckMode(Checkmode.recheckWithScanner);
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

        private void Reprint()
        {
            try
            {
                if (!_IsAfterProductionMode) return;

                lock (_SyncObjCodeList)
                {
                    if (_PrintedCodeObtainFromFile.Count() > 0 && _CodeListPODFormat.Count() > 0)
                    {
                        _TotalMissed = 0;
                        int codeDataLenght = 1;
                        foreach (var item in _CodeListPODFormat)
                        {
                            if (!item.Value.Status)
                            {
                                if (_PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] == "Printed")
                                    _PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] = "Reprint";
                            }
                            else
                            {
                                if (_PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] != "Printed")
                                    _PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] = "Printed";
                            }
                        }
                        _TotalMissed = _TotalCode - NumberOfCheckPassed;
                    }
                }

                if (_FormCheckedResult != null)
                    Invoke(new Action(() => { _FormCheckedResult.Close(); }));

                if (NumberOfCheckPassed < _TotalCode)
                {
                    // Còn mã chưa pass → chạy lại camera → AddValidCode → OnBoxReady → QR Hộp
                    // Khi Stop → FlushRemaining() sẽ sinh QR Thùng/Pallet lẻ (fix StopProcessAsync)
                    DialogResult dialogResult = CustomMessageBox.Show(Lang.ReprintConfirm, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                        StartProcess();
                }
                else
                {
                    // Tất cả mã đã pass
                    // [COMMENT] FlushRemaining không cần dùng: mã lẻ không ép vào hộp/thùng/pallet
                    // Chỉ xuất theo quy cách chẵn. Mã lẻ tồn tại đến session sau.
                    // try { if (_packagingManager != null && !_isDrocoExcelPackagingMode) _packagingManager.FlushRemaining(); }
                    // catch (Exception ex) { ProjectLogger.WriteError($"[Reprint] FlushRemaining lỗi: {ex.Message}"); }

                    // 2. Đồng bộ lại _SelectedJob
                    _SelectedJob = Shared.CurrentJob ?? _SelectedJob;

                    // 3. In lại QR Hộp
                    if (_SelectedJob?.BoxList != null)
                    {
                        foreach (var box in _SelectedJob.BoxList)
                        {
                            if (string.IsNullOrEmpty(box.QrCode)) continue;
                            try { Shared.PrintZebraDroco(box.QrCode, QRType.Box); }
                            catch (Exception ex) { ProjectLogger.WriteError($"[Reprint] Box {box.QrCode}: {ex.Message}"); }
                        }
                    }

                    // 4. In lại QR Thùng
                    if (_SelectedJob?.DrocoCartonList != null)
                    {
                        foreach (var carton in _SelectedJob.DrocoCartonList)
                        {
                            if (string.IsNullOrEmpty(carton.QrCode)) continue;
                            try { Shared.PrintZebraDroco(carton.QrCode, QRType.Carton); }
                            catch (Exception ex) { ProjectLogger.WriteError($"[Reprint] Carton {carton.QrCode}: {ex.Message}"); }
                        }
                    }

                    // 5. In lại QR Pallet
                    if (_SelectedJob?.DrocoPalletList != null)
                    {
                        foreach (var pallet in _SelectedJob.DrocoPalletList)
                        {
                            if (string.IsNullOrEmpty(pallet.QrCode)) continue;
                            try { Shared.PrintZebraDroco(pallet.QrCode, QRType.Pallet); }
                            catch (Exception ex) { ProjectLogger.WriteError($"[Reprint] Pallet {pallet.QrCode}: {ex.Message}"); }
                        }
                    }
                }
            }
            catch
            {
                CustomMessageBox.Show(Lang.ReprintError, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }
        //private void Reprint()
        //{
        //    try
        //    {
        //        if (!_IsAfterProductionMode)
        //            return;
        //        lock (_SyncObjCodeList)
        //        {
        //            if (_PrintedCodeObtainFromFile.Count() > 0 && _CodeListPODFormat.Count() > 0)
        //            {
        //                _TotalMissed = 0;
        //                int codeDataLenght = 1;

        //                foreach (var item in _CodeListPODFormat)
        //                {
        //                    if (!item.Value.Status)
        //                    {
        //                        if (_PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] == "Printed")
        //                        {
        //                            _PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] = "Reprint";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (_PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] != "Printed")
        //                        {
        //                            _PrintedCodeObtainFromFile[item.Value.Index][codeDataLenght] = "Printed";
        //                        }
        //                    }
        //                }
        //                _TotalMissed = _TotalCode - NumberOfCheckPassed;
        //            }
        //        }

        //        if (_FormCheckedResult != null)
        //        {
        //            Invoke(new Action(() => { _FormCheckedResult.Close(); }));
        //        }

        //        if (NumberOfCheckPassed < _TotalCode)
        //        {
        //            // Còn mã chưa pass → chạy lại luồng check bình thường → AddValidCode → in QR hộp
        //            DialogResult dialogResult = CustomMessageBox.Show(Lang.ReprintConfirm, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //            if (dialogResult == DialogResult.Yes)
        //                StartProcess();
        //            else
        //                return;
        //        }
        //        else
        //        {
        //            // Tất cả mã đã pass → in lại QR Hộp trực tiếp từ BoxList đã lưu
        //            if (_SelectedJob?.BoxList != null && _SelectedJob.BoxList.Count > 0)
        //            {
        //                foreach (var box in _SelectedJob.BoxList)
        //                {
        //                    if (!string.IsNullOrEmpty(box.QrCode))
        //                    {
        //                        try
        //                        {
        //                            Shared.PrintZebraDroco(box.QrCode, Shared.QRType.Box);
        //                            ProjectLogger.WriteInfo($"[Reprint] In lại QR Hộp: {box.QrCode}");
        //                        }
        //                        catch (Exception exBox)
        //                        {
        //                            ProjectLogger.WriteError($"[Reprint] Lỗi in QR Hộp {box.QrCode}: {exBox.Message}");
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        CustomMessageBox.Show(Lang.ReprintError, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Question);
        //        return;
        //    }
        //}

        public async void ExportSelectedType(int typeNumber)
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
                await Task.Run(() =>
                {
                    switch (typeNumber)
                    {
                        case 0:
                            ExportAllPassedData(sfd.FileName);
                            break;
                        case 1:
                            ExportPassedDataByCamAndScanner(sfd.FileName);
                            break;
                        case 2:
                            ExportPassedDataByScanner(sfd.FileName);
                            break;
                        case 3:
                            ExportUnverifiedData(sfd.FileName);
                            break;
                        case 4:
                            ExportSampledData(sfd.FileName);
                            break;
                        case 5:
                            ExportFailedData(sfd.FileName);
                            break;
                        case 6:
                            ExportWaitingData(sfd.FileName);
                            break;
                        case 7:
                            ExportWaitingData_2(sfd.FileName);
                            break;

                        default:
                            break;
                    }
                });
                EnableUIComponentWhenLoadData(true);
            }

        }

        private void ExportAllPassedData(string fileName)
        {
            var checkedResultDict = _CheckedResultCodeList
                  .Where(arr => arr[Index_Result] == "Valid")
                  .GroupBy(x => x[Index_ResultData])
                  .ToDictionary(
                                  g => g.Key,
                                  g => string.Join(", ", g.First().Skip(3).Prepend(g.First().ElementAtOrDefault(Index_ResultData)).Where(value => !string.IsNullOrEmpty(value)))

                               );
            ExportPassedData(fileName, checkedResultDict);
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
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
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
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
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
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
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
                CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }
        public bool ExportJobReportToCsv(string filePath)
        {
            try
            {
                LogToFile($"[ExportJobReport] Bắt đầu xuất báo cáo: {filePath}");

                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // 1. Tổng số lượng QR code
                    writer.WriteLine("=== BÁO CÁO JOB ===");
                    writer.WriteLine($"Tổng số QR Hộp: {_SelectedJob.BoxList?.Count ?? 0}");
                    writer.WriteLine($"Tổng số QR Thùng: {_SelectedJob.DrocoCartonList?.Count ?? 0}");
                    writer.WriteLine($"Tổng số QR Pallet: {_SelectedJob.DrocoPalletList?.Count ?? 0}");
                    writer.WriteLine();

                    // 2. Báo cáo QR Hộp
                    writer.WriteLine("=== Báo cáo QR Code Hộp ===");
                    writer.WriteLine("STT,QR Code Hộp,Đã in,Đã quét");
                    int idx = 1;
                    foreach (var box in _SelectedJob.BoxList ?? new List<BoxModel>())
                    {
                        writer.WriteLine($"{idx},{box.QrCode},{(box.sentToPrinter ? "X" : "")},{(box.IsSent ? "X" : "")}");
                        idx++;
                    }
                    writer.WriteLine();

                    // 3. Báo cáo QR Thùng
                    writer.WriteLine("=== Báo cáo QR Code Thùng ===");
                    writer.WriteLine("STT,QR Code Thùng,Đã in,Đã quét");
                    idx = 1;
                    foreach (var carton in _SelectedJob.DrocoCartonList ?? new List<DrocoCartonModel>())
                    {
                        writer.WriteLine($"{idx},{carton.QrCode},{(carton.sentToPrinter ? "X" : "")},{(carton.IsSent ? "X" : "")}");
                        idx++;
                    }
                    writer.WriteLine();

                    // 4. Báo cáo QR Pallet
                    writer.WriteLine("=== Báo cáo QR Code Pallet ===");
                    writer.WriteLine("STT,QR Code Pallet,Đã in,Đã quét");
                    idx = 1;
                    foreach (var pallet in _SelectedJob.DrocoPalletList ?? new List<DrocoPalletModel>())
                    {
                        writer.WriteLine($"{idx},{pallet.QrCode},{(pallet.sentToPrinter ? "X" : "")},{(pallet.IsSent ? "X" : "")}");
                        idx++;
                    }
                }

                // Kiểm tra file đã được tạo thành công
                bool exists = File.Exists(filePath);
                LogToFile($"[ExportJobReport] Xuất báo cáo {(exists ? "thành công" : "thất bại")}: {filePath}");
                return exists;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                Shared.RaiseOnLogError(ex);
                return false;
            }
        }

        private bool ExportJobReportToExcel(string filePath, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var wsReport = package.Workbook.Worksheets.Add("DRC");
                    wsReport.Cells.Style.Font.Name = "Arial";
                    wsReport.Cells.Style.Font.Size = 10;

                    // ── 1. TIÊU ĐỀ ────────────────────────────────────────────────
                    wsReport.Row(1).Height = 22;
                    var titleCell = wsReport.Cells[1, 1];
                    titleCell.Value = "List of Applied Markings";
                    wsReport.Cells[1, 1, 1, 9].Merge = true;
                    titleCell.Style.Font.Bold = true;
                    titleCell.Style.Font.Size = 14;
                    titleCell.Style.Font.Name = "Arial";
                    titleCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    titleCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    // ── 2. HEADER (dòng 2) ────────────────────────────────────────
                    wsReport.Row(2).Height = 32;
                            string[] headers = {
                        "S/O",
                        "Batch number",
                        "SKU",
                        "Honest mark_UNIT",
                        "Honest mark_UNIT\n(short version)",
                        "SSCC_MID\n(중박스)",
                        "SSCC_MASTER\n(대박스)",
                        "QR_PALLET\n(팔레트)"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var hCell = wsReport.Cells[2, i + 2];
                        hCell.Value = headers[i];
                        hCell.Style.Font.Name = "Arial";
                        hCell.Style.Font.Bold = true;
                        hCell.Style.Font.Size = 9;
                        hCell.Style.WrapText = true;
                        hCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        hCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        hCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        hCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        hCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    }

                    wsReport.Cells[2, 2, 2, 9].AutoFilter = true;

                    // ── 3. THU THẬP DỮ LIỆU ──────────────────────────────────────
                    var dataList = new List<(
                        string SO, string Batch, string SKU,
                        string FullGS1, string ShortGS1,
                        string BoxQR, string CartonQR, string PalletQR)>();

                    if (_SelectedJob.BoxList != null)
                    {
                        foreach (var box in _SelectedJob.BoxList)
                        {
                            string qrCarton = _SelectedJob.DrocoCartonList?
                                .FirstOrDefault(c => c.BoxCodes != null && c.BoxCodes.Contains(box.QrCode))?.QrCode ?? "";
                            string qrPallet = !string.IsNullOrEmpty(qrCarton)
                                ? (_SelectedJob.DrocoPalletList?
                                    .FirstOrDefault(p => p.CartonCodes != null && p.CartonCodes.Contains(qrCarton))?.QrCode ?? "")
                                : "";

                            foreach (var rawCode in (box.ProductCodes ?? new List<string>()))
                            {
                                if (string.IsNullOrWhiteSpace(rawCode)) continue;

                                //string fullGs1 = ReplaceGSCharacters(rawCode);

                                string fullGs1 = rawCode.Replace("\\F", "\x1D");     // ✅
                                string shortTemp = fullGs1.Replace("\x1D", " ").Trim(); 
                                string shortGs1 = shortTemp.Length > 24
                                    ? shortTemp.Substring(0, 24)
                                    : shortTemp;

                                dataList.Add((
                                    SO: _SelectedJob.SalesOrder ?? "",
                                    Batch: _SelectedJob.BatchNumber ?? "",
                                    SKU: _SelectedJob.ProductModel ?? "",
                                    FullGS1: fullGs1,
                                    ShortGS1: shortGs1,
                                    BoxQR: box.QrCode ?? "",
                                    CartonQR: qrCarton,
                                    PalletQR: qrPallet
                                ));
                            }
                        }
                    }

                    // ── 4. SẮP XẾP ────────────────────────────────────────────────
                    var sortedData = dataList
                        .OrderByDescending(x => !string.IsNullOrEmpty(x.FullGS1))
                        .ThenByDescending(x => !string.IsNullOrEmpty(x.PalletQR))
                        .ThenByDescending(x => !string.IsNullOrEmpty(x.CartonQR))
                        .ThenByDescending(x => !string.IsNullOrEmpty(x.BoxQR))
                        .ThenBy(x => x.PalletQR)
                        .ThenBy(x => x.CartonQR)
                        .ThenBy(x => x.BoxQR)
                        .ThenBy(x => x.FullGS1)
                        .ToList();

                    // ── 5. GHI DỮ LIỆU (dòng 3 trở đi) ─────────────────────────
                    int currentRow = 3;
                    foreach (var item in sortedData)
                    {
                        wsReport.Cells[currentRow, 2].Value = item.SO;
                        wsReport.Cells[currentRow, 3].Value = item.Batch;
                        wsReport.Cells[currentRow, 4].Value = item.SKU;
                        wsReport.Cells[currentRow, 5].Value = item.FullGS1;
                        wsReport.Cells[currentRow, 6].Value = item.ShortGS1;
                        wsReport.Cells[currentRow, 7].Value = item.BoxQR;
                        wsReport.Cells[currentRow, 8].Value = item.CartonQR;
                        wsReport.Cells[currentRow, 9].Value = item.PalletQR;

                        var rowRange = wsReport.Cells[currentRow, 2, currentRow, 9];
                        rowRange.Style.Font.Name = "Arial";
                        rowRange.Style.Font.Size = 8;
                        rowRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        rowRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        rowRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        rowRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        currentRow++;
                    }

                    // ── 6. AUTO-FIT & SAVE ────────────────────────────────────────
                    foreach (var ws in package.Workbook.Worksheets)
                    {
                        if (ws.Dimension != null)
                            ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    }

                    package.SaveAs(new FileInfo(filePath));
                }
                return File.Exists(filePath);
               
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
        //private bool ExportJobReportToExcel(string filePath, out string errorMessage)
        //{
        //    errorMessage = "";
        //    try
        //    {
        //        using (var package = new OfficeOpenXml.ExcelPackage())
        //        {
        //            var wsReport = package.Workbook.Worksheets.Add("DRC");
        //            wsReport.Cells.Style.Font.Name = "Arial";
        //            wsReport.Cells.Style.Font.Size = 10;

        //            // ── 1. TIÊU ĐỀ ────────────────────────────────────────────────
        //            wsReport.Row(1).Height = 22;
        //            var titleCell = wsReport.Cells[1, 1];
        //            titleCell.Value = "List of Applied Markings";
        //            wsReport.Cells[1, 1, 1, 9].Merge = true;
        //            titleCell.Style.Font.Bold = true;
        //            titleCell.Style.Font.Size = 14;
        //            titleCell.Style.Font.Name = "Arial";
        //            titleCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //            titleCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

        //            // ── 2. CHÚ THÍCH ──────────────────────────────────────────────
        //            wsReport.Row(2).Height = 80;
        //            var noteCell = wsReport.Cells[2, 2];
        //            noteCell.Value =
        //                "Honest mark_UNIT - 러시아 법인 전달 예정\n" +
        //                "Honest mark_UNIT (short version) - 러시아 법인 전달 예정\n" +
        //                "SSCC_MID(중박스) - 러시아 법인 전달 예정\n" +
        //                "SSCC_MASTER(대박스) - 러시아 법인 전달 예정\n" +
        //                "QR_PALLET (팔레트) - 러시아 법인 전달 예정";
        //            noteCell.Style.WrapText = true;
        //            noteCell.Style.Font.Name = "맑은 고딕";
        //            noteCell.Style.Font.Size = 8;
        //            noteCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        //            wsReport.Cells[2, 2, 2, 9].Merge = true;

        //            // ── 3. HEADER (dòng 3) ────────────────────────────────────────
        //            wsReport.Row(3).Height = 32;
        //            string[] headers = {
        //                "S/O",                               // B (2)
        //                "Batch number",                      // C (3)
        //                "SKU",                               // D (4)
        //                "Honest mark_UNIT",                  // E (5)
        //                "Honest mark_UNIT\n(short version)", // F (6)
        //                "SSCC_MID\n(중박스)",                  // G (7) → QR Hộp
        //                "SSCC_MASTER\n(대박스)",               // H (8) → QR Thùng
        //                "QR_PALLET\n(팔레트)"                // I (9) → QR Pallet
        //            };

        //            for (int i = 0; i < headers.Length; i++)
        //            {
        //                var hCell = wsReport.Cells[3, i + 2];
        //                hCell.Value = headers[i];
        //                hCell.Style.Font.Name = "Arial";
        //                hCell.Style.Font.Bold = true;
        //                hCell.Style.Font.Size = 9;
        //                hCell.Style.WrapText = true;
        //                hCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //                hCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        //                hCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
        //                hCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //                hCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        //            }

        //            // AutoFilter trên dòng header
        //            wsReport.Cells[3, 2, 3, 9].AutoFilter = true;

        //            // ── 4. THU THẬP DỮ LIỆU ──────────────────────────────────────
        //            // Dùng ValueTuple thay dynamic — tương thích C# 7.3 + LINQ an toàn
        //            var dataList = new List<(
        //                string SO, string Batch, string SKU,
        //                string FullGS1, string ShortGS1,
        //                string BoxQR, string CartonQR, string PalletQR)>();

        //            if (_SelectedJob.BoxList != null)
        //            {
        //                foreach (var box in _SelectedJob.BoxList)
        //                {
        //                    // Tra cứu 1 lần / hộp (ngoài vòng lặp GS1)
        //                    string qrCarton = _SelectedJob.DrocoCartonList?
        //                        .FirstOrDefault(c => c.BoxCodes != null && c.BoxCodes.Contains(box.QrCode))?.QrCode ?? "";
        //                    string qrPallet = !string.IsNullOrEmpty(qrCarton)
        //                        ? (_SelectedJob.DrocoPalletList?
        //                            .FirstOrDefault(p => p.CartonCodes != null && p.CartonCodes.Contains(qrCarton))?.QrCode ?? "")
        //                        : "";

        //                    foreach (var rawCode in (box.ProductCodes ?? new List<string>()))
        //                    {
        //                        if (string.IsNullOrWhiteSpace(rawCode)) continue;

        //                        string fullGs1 = ReplaceGSCharacters(rawCode);
        //                        string shortTemp = fullGs1.Replace("\\F", " ").Trim();
        //                        string shortGs1 = shortTemp.Length > 24
        //                            ? shortTemp.Substring(0, 24)
        //                            : shortTemp;

        //                        dataList.Add((
        //                            SO: _SelectedJob.SalesOrder ?? "",
        //                            Batch: _SelectedJob.BatchNumber ?? "",
        //                            SKU: _SelectedJob.ProductModel ?? "",
        //                            FullGS1: fullGs1,
        //                            ShortGS1: shortGs1,
        //                            BoxQR: box.QrCode ?? "",
        //                            CartonQR: qrCarton,
        //                            PalletQR: qrPallet
        //                        ));
        //                    }
        //                }
        //            }

        //            // ── 5. SẮP XẾP ────────────────────────────────────────────────
        //            // Bước 1: Ưu tiên hàng có đầy đủ 4 cấp lên đầu (hoàn chỉnh nhất → ít nhất)
        //            // Bước 2: Trong cùng mức hoàn chỉnh → gom nhóm theo Pallet → Thùng → Hộp → GS1
        //            var sortedData = dataList
        //                .OrderByDescending(x => !string.IsNullOrEmpty(x.FullGS1))       // Hàng có GS1 lên đầu
        //                .ThenByDescending(x => !string.IsNullOrEmpty(x.PalletQR))       // Hàng có Pallet lên trên
        //                .ThenByDescending(x => !string.IsNullOrEmpty(x.CartonQR))       // Hàng có Carton lên trên
        //                .ThenByDescending(x => !string.IsNullOrEmpty(x.BoxQR))          // Hàng có Box lên trên
        //                .ThenBy(x => x.PalletQR)                                        // Gom nhóm cùng Pallet
        //                .ThenBy(x => x.CartonQR)                                        // Gom nhóm cùng Thùng
        //                .ThenBy(x => x.BoxQR)                                           // Gom nhóm cùng Hộp
        //                .ThenBy(x => x.FullGS1)                                         // Sắp tăng dần theo mã GS1
        //                .ToList();

        //            // ── 6. GHI DỮ LIỆU (dòng 4 trở đi) ─────────────────────────
        //            int currentRow = 4;
        //            foreach (var item in sortedData)
        //            {
        //                wsReport.Cells[currentRow, 2].Value = item.SO;
        //                wsReport.Cells[currentRow, 3].Value = item.Batch;
        //                wsReport.Cells[currentRow, 4].Value = item.SKU;
        //                wsReport.Cells[currentRow, 5].Value = item.FullGS1;
        //                wsReport.Cells[currentRow, 6].Value = item.ShortGS1;
        //                wsReport.Cells[currentRow, 7].Value = item.BoxQR;
        //                wsReport.Cells[currentRow, 8].Value = item.CartonQR;
        //                wsReport.Cells[currentRow, 9].Value = item.PalletQR;

        //                var rowRange = wsReport.Cells[currentRow, 2, currentRow, 9];
        //                rowRange.Style.Font.Name = "Arial";
        //                rowRange.Style.Font.Size = 8;
        //                rowRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        //                rowRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        //                rowRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        //                rowRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

        //                currentRow++;
        //            }

        //            // ── 7. AUTO-FIT & SAVE ────────────────────────────────────────
        //            foreach (var ws in package.Workbook.Worksheets)
        //            {
        //                if (ws.Dimension != null)
        //                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
        //            }

        //            package.SaveAs(new FileInfo(filePath));
        //        }
        //        return File.Exists(filePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMessage = ex.Message;
        //        return false;
        //    }
        //}
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

        private async void ExportCheckedResultAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                CustomMessageBox.Show("Không tìm thấy file kết quả kiểm tra!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ShowLoading("Đang xuất dữ liệu...\nVui lòng chờ trong giây lát");
            await Task.Run(() => Shared.ExportCheckedResult(filePath));
            HideLoading();
        }

        public void ExportAllData(string fileName)
        {
            //try
            //{

            //    List<string> linesToWrite = new List<string>();
            //    // if duplicate, which position would we get, if valid then miss-alignment, what would we get
            //    // thinh_04_04_2025
            //    var duplicateCountDict = _CheckedResultCodeList
            //        .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] == "Duplicated")
            //        .GroupBy(x => x[Index_ResultData])
            //        .ToDictionary(g => g.Key, g => g.Count());

            //    //Dictionary with Result data is key and datetime is value
            //    var checkedResultDict = _CheckedResultCodeList
            //        .Where(arr => arr[Index_Device] != "Barcode Scanner")
            //        .GroupBy(x => x[Index_ResultData])
            //        .ToDictionary(
            //            g => g.Key,
            //            g => (DateTime: g.First()[Index_DateTime], Position: g.First()[Index_Position])
            //        );

            //    var test2 = _CheckedResultCodeList
            //        .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] != "Valid" && arr[Index_Result] != "Duplicated").GroupBy(x => x[Index_ResultData]).ToDictionary(g => g.Key, g => g.First()[Index_DateTime]);


            //    var test = _CheckedResultCodeList
            //                                .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] != "Valid" && arr[Index_Result] != "Duplicated")
            //                                .Select(arr => new KeyValuePair<string, string>(arr[Index_ResultData], arr[Index_DateTime]))
            //                                .ToList();

            //    if (File.Exists(fileName))
            //        File.Delete(fileName);

            //    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
            //    {
            //        // Write the header, edit column element
            //        var statusCol = _DatabaseColunms[1];
            //        var newColumn = new string[_DatabaseColunms.Length];
            //        int currentIndex = 0;
            //        for (int i = 0; i < _DatabaseColunms.Length; i++)
            //        {
            //            if (i != 1)
            //            {
            //                newColumn[currentIndex++] = _DatabaseColunms[i];
            //            }
            //        }
            //        newColumn[newColumn.Length - 1] = statusCol;
            //        string header = string.Join(",", newColumn.Select(Csv.Escape)) + ",Number Checking Code" + ",Position" + ",VerifyDate";
            //        //writer.WriteLine(header);
            //        linesToWrite.Add(header);

            //        for (int i = 0; i < _TotalCode; i++)
            //        {
            //            var record = _PrintedCodeObtainFromFile[i];

            //            // Rearrange the record if it has more than 1 element
            //            if (record.Length > 1)
            //            {
            //                var newRecord = new string[record.Length];
            //                Array.Copy(record, 0, newRecord, 0, 1);
            //                Array.Copy(record, 2, newRecord, 1, record.Length - 2);
            //                newRecord[newRecord.Length - 1] = record[1];
            //                record = newRecord;
            //            }

            //            var compareString = GetCompareDataByPODFormat(record, _SelectedJob.PODFormat);
            //            var writeValue = string.Join(",", record.Take(record.Length - 1).Select(Csv.Escape)) + ",";
            //            var status = record[record.Length - 1];
            //            bool isChecked = checkedResultDict.TryGetValue(compareString, out var checkedValues);

            //            if (isChecked)
            //            {
            //                if (status.Equals("Printed")) // Is Printed 
            //                {
            //                    if (duplicateCountDict.TryGetValue(compareString, out int duplicateCount) && duplicateCount >= 1)
            //                    {
            //                        //writeValue += "Duplicated";
            //                        writeValue += "Valid";
            //                        writeValue += "," + (duplicateCount + 1);
            //                    }
            //                    else
            //                    {
            //                        writeValue += "Valid";
            //                        writeValue += "," + "1";
            //                    }
            //                }
            //                else // Is Not Printed
            //                {
            //                    var index = test.FindIndex(kvp => kvp.Key == compareString);
            //                    if (index != -1)
            //                    {
            //                        test.RemoveAt(index);
            //                    }


            //                    // Dang lam cho In-Sight
            //                    writeValue += "Miss Alignment";
            //                    writeValue += "," + "1";
            //                }


            //                writeValue += "," + Csv.Escape(checkedValues.Position).Trim('"');
            //                writeValue += "," + Csv.Escape(checkedValues.DateTime);
            //                checkedResultDict.Remove(compareString);

            //            }
            //            else
            //            {
            //                if (status.Equals("Printed"))
            //                {
            //                    writeValue += PrintedUnverified;
            //                }
            //                else
            //                {
            //                    writeValue += "Unverified";

            //                }

            //            }
            //            linesToWrite.Add(writeValue);
            //        }
            //        //foreach (var item in test)
            //        //{
            //        //    string valu = "";
            //        //    int t = _PrintedCodeObtainFromFile[0].Length - 1;
            //        //    for (int i = 0; i < t-1; i++)
            //        //    {
            //        //        valu += ",";
            //        //    }

            //        //    string key = item.Key;
            //        //    string value = item.Value;
            //        //    if(key == "")
            //        //    {
            //        //        key = "Null";
            //        //    }
            //        //    valu += key;
            //        //    valu += ",Invalid";
            //        //    valu += ",1,";
            //        //    valu += value;
            //        //    linesToWrite.Add(valu);
            //        //}
            //    }

            //    //linesToWrite = linesToWrite
            //    //            .Select((line, index) => new { Array = line.Split(','), OriginalIndex = index }) // Split and preserve original index
            //    //            .OrderBy(x =>
            //    //            {
            //    //                DateTime dt;
            //    //                return DateTime.TryParseExact(x.Array[x.Array.Length - 1], "yyyy/MM/dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dt)
            //    //                    ? dt.Ticks // Use ticks for DateTime lines
            //    //                    : long.MaxValue; // Push non-DateTime lines to the end
            //    //            })
            //    //            .ThenBy(x => x.OriginalIndex) // Stable sort for non-DateTime lines
            //    //            .Select(x => string.Join(",", x.Array)) // Join back into a string
            //    //            .ToList();



            //    if (fileName.EndsWith(".pdf"))
            //    {
            //        ConvertCsvToPdf(linesToWrite.ToArray(), fileName);
            //    }
            //    else
            //    {
            //        using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
            //        {
            //            foreach (var line in linesToWrite)
            //            {
            //                writer.WriteLine(line); // Write the sorted comma-separated string
            //            }
            //        }
            //    }

            //    MoveToTheFile(fileName);
            //    checkedResultDict.Clear();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(Lang.DetectError);
            //    Shared.RaiseOnLogError(ex);
            //    EnableUIComponent(OperationStatus.Stopped);
            //}
        }

        public void ExportData(string fileName)
        {
            //try
            //{
            //    var duplicateCountDict = _CheckedResultCodeList
            //        .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] == "Duplicated")
            //        .GroupBy(x => x[Index_ResultData])
            //        .ToDictionary(g => g.Key, g => g.Count());

            //    var checkedResultDict = _CheckedResultCodeList
            //        .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] == "Valid")
            //        .GroupBy(x => x[Index_ResultData])
            //        .ToDictionary(g => g.Key, g => g.First()[Index_DateTime]);

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
            //    string header = string.Join(",", newColumn.Select(Csv.Escape)) + ",VerifyDate";
            //    lines.Add(header);
            //    for (int i = 0; i < _TotalCode; i++)
            //    {
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
            //            if (status.Equals("Printed")) 
            //            {
            //                if (duplicateCountDict.TryGetValue(compareString, out int duplicateCount) && duplicateCount >= 1)
            //                {
            //                    writeValue += PrintedDuplicate;
            //                }
            //                else
            //                {
            //                    writeValue += PrintedVerified;
            //                }
            //            }
            //            else 
            //            {
            //                writeValue += UnprintedVerified;
            //            }

            //            writeValue += "," + Csv.Escape(dateVerify);
            //            checkedResultDict.Remove(compareString);
            //        }
            //        else
            //        {
            //            if (status.Equals("Printed"))
            //            {
            //                writeValue += PrintedUnverified;
            //            }
            //            else
            //            {
            //                writeValue += UnprintedUnverified;
            //            }

            //        }
            //        lines.Add(writeValue);
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

            //    //SetFileReadOnly(fileName);
            //    MoveToTheFile(fileName);
            //    checkedResultDict.Clear();
            //}
            //catch (Exception ex)
            //{
            //    CuzAlert.Show(Lang.DetectError, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
            //    Shared.RaiseOnLogError(ex);
            //    EnableUIComponent(OperationStatus.Stopped);
            //}
        }

        public static void ConvertCsvToPdf(string[] lines, string pdfFilePath)
        {
            try
            {
                //if(lines.Length < 2)
                //{
                //    CustomMessageBox.Show($"No data to export", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                //}
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

            //this is BitMap solution, this is for image in PDF
            //using (Bitmap bitmap = new Bitmap(width, height))
            //{
            //    using (Graphics graphics = Graphics.FromImage(bitmap))
            //    {
            //        graphics.Clear(Color.White);
            //        int yPos = 4;
            //        foreach (var line in lines)
            //        {
            //            graphics.DrawString(line, font, brush, new PointF(4, yPos));
            //            yPos += lineHeight;
            //        }

            //        // Save bitmap to temp BMP file
            //        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".bmp");
            //        bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            //        // Load the saved BMP using XImage
            //        using (XImage xImage = XImage.FromFile(tempPath))
            //        {
            //            PdfPage page = pdf.AddPage();
            //            XGraphics gfx = XGraphics.FromPdfPage(page);

            //            gfx.DrawImage(xImage, 0, 0, page.Width, page.Height);
            //        }

            //        // Clean up the temp file
            //        File.Delete(tempPath);
            //    }
            //}

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

        private void Shared_OnZebraPrinterStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelZebraPrinter();
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        private void Shared_OnSensorControllerChangeEvent(object sender, EventArgs e)
        {
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
        }

        private void Shared_OnSerialDeviceControllerChangeEvent(object sender, EventArgs e)
        {
            UpdateUISerialDeviceControllerStatus(Shared.IsSerialDeviceConnected);
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

            btnCustomExport.Text = Lang.Custom;
            btnJob.Text = Lang.Operation;
            btnDatabase.Text = Lang.DatabaseFrmMain;
            btnAccount.Text = Lang.Account;
            btnHistory.Text = Lang.ProgramHistoryFrmMain;
            btnSettings.Text = Lang.Settings;
            btnExit.Text = Lang.Exit;
            btnExportData.Text = Lang.ExportDataExcel;
            btnExportResult.Text = Lang.ExportCheckedLog;
            btnExportAll.Text = Lang.ExportAll;

            pnlJobInformation.Text = Lang.JobDetails;
            lblJobName.Text = Lang.FileName;


            lblTotalGS1.Text = "Tổng mã GS1";
            lblCodeToBox.Text = "Mã → Hộp";
            lblBoxToCarton.Text = "Hộp → Thùng";
            lblCartonToPallet.Text = "Thùng → Pallet";  // ← phải là lblCartonToPallet

            lblQRBox.Text = "QR Hộp";
            lblQRCarton.Text = "QR Thùng";
            lblQRPallet.Text = "QR Pallet";
            lblReceived.Text = Lang.Received;
            lblSentData.Text = Lang.SentData;
            lblPrintedCode.Text = Lang.PrintedCode;

            pnlVerificationProcess.Text = Lang.VerifyProgress;
            lblTotalChecked.Text = Lang.TotalChecked;
            lblPassed.Text = Lang.CheckedPassed;
            lblFailed.Text = Lang.CheckedFailed;

            pnlCurrentCheck.Text = Lang.CheckedResult;
            lblCodeResult.Text = Lang.Code;
            lblProcessingTime.Text = Lang.ProcessingTime;
            lblStatusResult.Text = Lang.StatusCode;
            BarcodeQualityLabel.Text = Lang.BarcodeQuality + ":";

            lblStatusCamera01.Text = Lang.CameraTMP;
            lblStatusPrinter01.Text = Lang.Printer;
            lblStatusSerialDevice.Text = Lang.ScannerLabel;
            lblSensorControllerStatus.Text = Lang.PLCLabel;
            toolStripOperationStatus.Text = Lang.Stopped;

            labelModeCheck.Text = Lang.CheckMode;

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
        private async void btnCompleteJob_Click(object sender, EventArgs e)
        {
            try
            {
                if (Shared.OperStatus != OperationStatus.Stopped)
                {
                    CustomMessageBox.Show("Vui lòng dừng quá trình trước khi hoàn thành công việc!", Lang.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (Shared.DrocoAllValueProcess == null)
                    Shared.DrocoAllValueProcess = new DrocoAllValueProcess(_SelectedJob);

                var listCancelQr = Shared.DrocoAllValueProcess.GetUnmappedQRCodes();
                var CancelList = new RequestCancelList { listCancelQr = listCancelQr };

                var resultMessage = CustomMessageBox.IsResultShow("Bạn có chắc chắn muốn hoàn thành công việc không?\n" +
                    $"Số lượng mã đã vào thùng: {GetCodeInCarton()}\n" +
                    $"Số lượng mã sẽ hủy: {listCancelQr.Count}\n" +
                    $"Cảnh báo: Sau khi hoàn thành không thể tiếp tục chạy!");
                if (!resultMessage) return;

                bool isSuccess = false;
                string errorMessage = "";
                string fullUrl = Shared.Settings.ApiUrl + "/packaging-sessions/cancel-list";

                LogToFile($"=== Starting POST request ===");
                LogToFile($"URL: {fullUrl}");
                LogToFile($"AccessToken present: {!string.IsNullOrEmpty(Shared.Settings.DrocoToken)}");
                LogToFile($"CancelList count: {CancelList.listCancelQr?.Count ?? 0}");

                try
                {
                    using (var handler = new HttpClientHandler())
                    {
                        // Allow redirects and use default credentials if needed
                        handler.AllowAutoRedirect = true;
                        handler.UseCookies = false;

                        using (HttpClient _client = new HttpClient(handler))
                        {
                            _client.Timeout = TimeSpan.FromSeconds(30);
                            LogToFile($"HttpClient created with timeout: 30 seconds");

                            if (!string.IsNullOrEmpty(Shared.Settings.DrocoToken))
                            {
                                _client.DefaultRequestHeaders.Add("app_info", Shared.Settings.DrocoToken);
                            }

                            using (var sendRequest = new HttpRequestMessage(HttpMethod.Post, fullUrl))
                            {

                                var jsonPayload = JsonConvert.SerializeObject(CancelList);
                                LogToFile($"JSON payload length: {jsonPayload.Length} bytes");
                                sendRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                                LogToFile($"Content-Type: application/json");

                                LogToFile($"Sending request...");
                                using (var responsee = await _client.SendAsync(sendRequest))
                                {
                                    LogToFile($"Request sent. StatusCode: {(int)responsee.StatusCode} ({responsee.StatusCode})");

                                    // IMPORTANT: Check HTTP status code first (should be 201 Created)
                                    int statusCode = (int)responsee.StatusCode;
                                    bool httpSuccess = statusCode >= 200 && statusCode < 300;

                                    if (httpSuccess)
                                    {
                                        LogToFile($"HTTP Status OK - Status: {statusCode} ({responsee.StatusCode})");
                                    }
                                    else
                                    {
                                        LogToFile($"HTTP Status FAILED - Status: {statusCode} ({responsee.StatusCode})");
                                    }

                                    // Note: isSuccess will be set after parsing JSON response

                                    // Read response content using ReadAsStreamAsync
                                    string responseContent = "";
                                    if (responsee.Content != null)
                                    {
                                        LogToFile($"Attempting to read response content using ReadAsStreamAsync...");
                                        LogToFile($"Content type: {responsee.Content.Headers?.ContentType?.MediaType ?? "unknown"}");
                                        LogToFile($"Content length header: {responsee.Content.Headers?.ContentLength?.ToString() ?? "unknown"}");

                                        try
                                        {
                                            using (var stream = await responsee.Content.ReadAsStreamAsync())
                                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                                            {
                                                responseContent = await reader.ReadToEndAsync();
                                                LogToFile($"ReadAsStreamAsync completed. Length: {responseContent?.Length ?? 0} bytes");

                                                if (!string.IsNullOrEmpty(responseContent))
                                                {
                                                    LogToFile($"Response content: {responseContent}");
                                                }
                                                else
                                                {
                                                    LogToFile($"Response content is empty (normal for 201 Created responses)");
                                                }
                                            }
                                        }
                                        catch (Exception readEx)
                                        {
                                            LogToFile($"Could not read response content ({readEx.GetType().Name}): {readEx.Message}");
                                            LogToFile($"StackTrace: {readEx.StackTrace}");
                                            if (readEx.InnerException != null)
                                            {
                                                LogToFile($"InnerException: {readEx.InnerException.GetType().Name} - {readEx.InnerException.Message}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LogToFile($"Response has no content (this is normal for 201 Created responses)");
                                    }

                                    // Parse JSON response and check the code field
                                    if (!string.IsNullOrEmpty(responseContent))
                                    {
                                        try
                                        {
                                            var responseCancelList = JsonConvert.DeserializeObject<ResponseCancelList>(responseContent);
                                            if (responseCancelList != null)
                                            {
                                                LogToFile($"Parsed JSON response - code: {responseCancelList.code}, message: {responseCancelList.message}");

                                                // Success only if HTTP status is OK AND JSON response code is 200
                                                if (responseCancelList.code == 200)
                                                {
                                                    isSuccess = true;
                                                    LogToFile($"Request SUCCESS - HTTP: {statusCode}, JSON code: {responseCancelList.code}");
                                                }
                                                else
                                                {
                                                    isSuccess = false;
                                                    errorMessage = $"Server returned code: {responseCancelList.code}, message: {responseCancelList.message}";
                                                    LogToFile($"Request FAILED - HTTP: {statusCode}, JSON code: {responseCancelList.code}, message: {responseCancelList.message}");
                                                }
                                            }
                                            else
                                            {
                                                LogToFile($"Failed to parse JSON response - responseCancelList is null");
                                                // Fall back to HTTP status check
                                                isSuccess = httpSuccess;
                                                if (!isSuccess)
                                                {
                                                    errorMessage = $"Server returned: {statusCode} ({responsee.StatusCode}) - Could not parse response";
                                                }
                                            }
                                        }
                                        catch (Exception jsonEx)
                                        {
                                            LogToFile($"Failed to parse JSON response: {jsonEx.Message}");
                                            LogToFile($"Response content: {responseContent}");
                                            // Fall back to HTTP status check
                                            isSuccess = httpSuccess;
                                            if (!isSuccess)
                                            {
                                                errorMessage = $"Server returned: {statusCode} ({responsee.StatusCode}) - Could not parse JSON: {jsonEx.Message}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // No response content, use HTTP status as indicator
                                        LogToFile($"No response content received. Using HTTP status as indicator.");
                                        isSuccess = httpSuccess;
                                        if (!isSuccess)
                                        {
                                            errorMessage = $"Server returned: {statusCode} ({responsee.StatusCode}) - No response content";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    // Only set error if we haven't already determined success
                    if (!isSuccess)
                    {
                        errorMessage = $"HTTP Request Exception: {httpEx.Message}";
                    }
                    LogToFile($"HttpRequestException: {httpEx.Message}");
                    LogToFile($"HttpRequestException StackTrace: {httpEx.StackTrace}");
                    if (httpEx.InnerException != null)
                    {
                        LogToFile($"HttpRequestException InnerException: {httpEx.InnerException.Message}");
                    }
                }
                catch (TaskCanceledException timeoutEx)
                {
                    // Only set error if we haven't already determined success
                    if (!isSuccess)
                    {
                        errorMessage = $"Request timeout: {timeoutEx.Message}";
                    }
                    LogToFile($"TaskCanceledException (timeout): {timeoutEx.Message}");
                    LogToFile($"TaskCanceledException StackTrace: {timeoutEx.StackTrace}");
                    // Don't re-throw if we already got a successful response
                    if (!isSuccess)
                    {
                        throw; // Re-throw to be caught by outer catch
                    }
                }
                catch (Exception requestEx)
                {
                    // Only set error if we haven't already determined success
                    if (!isSuccess)
                    {
                        errorMessage = $"Request exception: {requestEx.Message}";
                    }
                    LogToFile($"Exception during request: {requestEx.GetType().Name}: {requestEx.Message}");
                    LogToFile($"Exception StackTrace: {requestEx.StackTrace}");
                    if (requestEx.InnerException != null)
                    {
                        LogToFile($"InnerException: {requestEx.InnerException.GetType().Name}: {requestEx.InnerException.Message}");
                    }
                }

                // Update UI on the UI thread (we're already on UI thread since we removed ConfigureAwait(false))
                if (isSuccess)
                {
                    try
                    {
                        LogToFile($"Processing successful response...");
                        // Set job status to Completed after successful request
                        _SelectedJob.CompleteJobStatus = CompleteJobStatus.Completed;
                        _SelectedJob.SaveFile();
                        LogToFile($"Job status saved successfully");

                        // Update UI to reflect completed status
                        UpdateCompleteJobStatusUI();
                        LogToFile($"UI updated successfully");

                        CustomMessageBox.Show("Hoàn thành công việc thành công!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LogToFile($"Success message shown");
                    }
                    catch (Exception successEx)
                    {
                        // Even if UI update fails, the request was successful
                        LogToFile($"Error during success handling (but request succeeded): {successEx.Message}");
                        LogToFile($"StackTrace: {successEx.StackTrace}");
                        CustomMessageBox.Show("Hoàn thành công việc thành công!\n(Có lỗi khi cập nhật giao diện nhưng yêu cầu đã được gửi thành công)", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    LogToFile($"Showing error message: {errorMessage}");
                    CustomMessageBox.Show($"Hoàn thành công việc thất bại!\n{errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (TaskCanceledException ex)
            {
                LogToFile($"Unexpected error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                CustomMessageBox.Show("Hoàn thành công việc thất bại!\nRequest timeout - Server không phản hồi.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogToFile($"Unexpected error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                CustomMessageBox.Show($"Hoàn thành công việc thất bại!\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static readonly object _logFileLock = new object();

        private void LogToFile(string message)
        {
            try
            {
                string baseDirectory = @"C:\ProgramData\R-Link";
                string logDirectory = Path.Combine(baseDirectory, "LogsExport");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string fileName = $"Log_{DateTime.Now:yyyy_MM_dd}.txt";
                string filePath = Path.Combine(logDirectory, fileName);

                string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";

                lock (_logFileLock)
                {
                    string oldContent = File.Exists(filePath) ? File.ReadAllText(filePath) : "";
                    File.WriteAllText(filePath, logEntry + oldContent);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Logging failed: " + ex.Message);
            }
        }

        private void UpdateCompleteJobStatusUI()
        {
            if (_SelectedJob == null) return;

            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateCompleteJobStatusUI()));
                return;
            }

            if (_SelectedJob.CompleteJobStatus == CompleteJobStatus.Completed)
            {

                // Disable the Complete Job button
                btnCompleteJob.Enabled = false;
                completedStatus.Visible = true;
                // Show completed status in txtStatusResult
                completedStatus.Text = "Đã hoàn thành";
            }
            else if (_SelectedJob.CompleteJobStatus == CompleteJobStatus.Running)
            {
                // Enable button for running status
                btnCompleteJob.Enabled = true;
                // Optionally show running status
                if (string.IsNullOrEmpty(txtStatusResult.Text) || txtStatusResult.Text == "Completed")
                {
                    txtStatusResult.Text = "";
                }
            }
            else
            {
                // Enable the button for Created status
                btnCompleteJob.Enabled = true;
            }
        }

        private void pictureBoxPreview_Click(object sender, EventArgs e)
        {

        }
        private void ReleaseResource()
        {
            try
            {
                _VirtualCTS?.Cancel();
                _SendDataToPrinterTokenCTS?.Cancel();
                _PrinterRespontCST?.Cancel();
                _QueueBufferPrinterResponseData.Enqueue(null);

                _VirtualCTS?.Dispose();
                _SendDataToPrinterTokenCTS?.Dispose();
                _PrinterRespontCST?.Dispose();

                _OperationCancelTokenSource?.Cancel();
                _UICheckedResultCancelTokenSource?.Cancel();
                _UIPrintedResponseCancelTokenSource?.Cancel();
                _BackupImageCancelTokenSource?.Cancel();
                _BackupResponseCancelTokenSource?.Cancel();
                _BackupResultCancelTokenSource?.Cancel();
                _BackupSendLogCancelTokenSource?.Cancel();
                _BackupRSFPLogCancelTokenSource?.Cancel();

                _OperationCancelTokenSource?.Dispose();
                _UICheckedResultCancelTokenSource?.Dispose();
                _UIPrintedResponseCancelTokenSource?.Dispose();
                _BackupImageCancelTokenSource?.Dispose();
                _BackupResponseCancelTokenSource?.Dispose();
                _BackupResultCancelTokenSource?.Dispose();
                _BackupSendLogCancelTokenSource?.Dispose();
                _BackupRSFPLogCancelTokenSource?.Dispose();

                _CheckedResultCodeList.Clear();
                _CodeListPODFormat.Clear();
                _PrintedCodeObtainFromFile.Clear();

                //_QueueBufferDataObtained.Clear();
                _QueueBufferDataObtainedResult.Clear();
                _QueueBufferUpdateUIPrinter.Clear();

                _QueueBufferBackupImage.Clear();
                _QueueBufferBackupPrintedCode.Clear();
                _QueueBufferBackupCheckedResult.Clear();
                _QueueBufferBackupSendLog.Clear();
                _QueueBufferBackupRSFPLog.Clear();

                Shared.OnQrCodeCartonChange -= Shared_OnQrCodeCartonChange;
                Shared.OnCameraPositionDataChange -= Shared_OnCameraPositionDataChange;
                Shared.OnCameraStatusChange -= Shared_OnCameraStatusChange;
                Shared.OnSerialDeviceReadDataChange -= Shared_OnSerialDeviceReadDataChange;
                Shared.OnCameraReadDataChange -= Shared_OnCameraReadDataChange;
                Shared.OnPrinterDataChange -= Shared_OnPrinterDataChange;
                Shared.OnPrintingStateChange -= Shared_OnPrintingStateChange;
                Shared.OnPrinterStatusChange -= Shared_OnPrinterStatusChange;
                Shared.OnZebraPrinterStatusChange -= Shared_OnZebraPrinterStatusChange;
                Shared.OnLanguageChange -= Shared_OnLanguageChange;
                Shared.OnSensorControllerChangeEvent -= Shared_OnSensorControllerChangeEvent;
                Shared.OnSerialDeviceControllerChangeEvent -= Shared_OnSerialDeviceControllerChangeEvent;
                Shared.OnVerifyAndPrindSendDataMethod -= Shared_OnVerifyAndPrindSendDataMethod;
                Shared.OnLogError -= Shared_OnLogError;
                this.OnReceiveVerifyDataEvent -= SendVerifiedDataToPrinter;
                KillThread(ref _ThreadPrinterResponseHandler);

                _FormCheckedResult?.Close();
                _FormCheckedResult?.Dispose();
                _FormPreviewDatabase?.Close();
                _FormPreviewDatabase?.Dispose();
            }
            catch (Exception e)
            {
                // FIX: Không re-throw exception trong quá trình cleanup
                // throw (e) gây crash khi form đang closing
                Console.WriteLine($"ReleaseResource error: {e.Message}");
            }
        }
        //private void ReleaseResource()
        //{
        //    try
        //    {
        //        _VirtualCTS?.Cancel();
        //        _SendDataToPrinterTokenCTS?.Cancel();
        //        _PrinterRespontCST?.Cancel();
        //        _QueueBufferPrinterResponseData.Enqueue(null);

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

        //        //_QueueBufferDataObtained.Clear();
        //        _QueueBufferDataObtainedResult.Clear();
        //        _QueueBufferUpdateUIPrinter.Clear();

        //        _QueueBufferBackupImage.Clear();
        //        _QueueBufferBackupPrintedCode.Clear();
        //        _QueueBufferBackupCheckedResult.Clear();
        //        _QueueBufferBackupSendLog.Clear();
        //        _QueueBufferBackupRSFPLog.Clear();

        //        Shared.OnQrCodeCartonChange -= Shared_OnQrCodeCartonChange;
        //        Shared.OnCameraPositionDataChange -= Shared_OnCameraPositionDataChange;
        //        Shared.OnCameraStatusChange -= Shared_OnCameraStatusChange;
        //        Shared.OnSerialDeviceReadDataChange -= Shared_OnSerialDeviceReadDataChange;
        //        Shared.OnCameraReadDataChange -= Shared_OnCameraReadDataChange;
        //        Shared.OnPrinterDataChange -= Shared_OnPrinterDataChange;
        //        Shared.OnPrintingStateChange -= Shared_OnPrintingStateChange;
        //        Shared.OnPrinterStatusChange -= Shared_OnPrinterStatusChange;
        //        Shared.OnZebraPrinterStatusChange -= Shared_OnZebraPrinterStatusChange;
        //        Shared.OnLanguageChange -= Shared_OnLanguageChange;
        //        Shared.OnSensorControllerChangeEvent -= Shared_OnSensorControllerChangeEvent;
        //        Shared.OnSerialDeviceControllerChangeEvent -= Shared_OnSerialDeviceControllerChangeEvent;
        //        Shared.OnVerifyAndPrindSendDataMethod -= Shared_OnVerifyAndPrindSendDataMethod;
        //        Shared.OnLogError -= Shared_OnLogError;
        //        this.OnReceiveVerifyDataEvent -= SendVerifiedDataToPrinter;
        //        KillThread(ref _ThreadPrinterResponseHandler);

        //        _FormCheckedResult?.Close();
        //        _FormCheckedResult?.Dispose();
        //        _FormPreviewDatabase?.Close();
        //        _FormPreviewDatabase?.Dispose();
        //    }
        //    catch (Exception e)
        //    {
        //        throw (e);
        //    }
        //}
        public static void AutoResizeColumnWith(DataGridView dgv, string[] value, int imgIndex = 0)
        {
            try
            {
                var firstRowWith = value;
                int totalColumnsWidth = TextRenderer.MeasureText(firstRowWith[0], dgv.Font).Width;
                int[] thickestRowIndex = { 0, TextRenderer.MeasureText(firstRowWith[0], dgv.Font).Width };
                for (int i = 1; i < firstRowWith.Length; i++)
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
        #region UpdateUI
        private void UpdateStopUI()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStopUI()));
                return;
            }

            EnableUIComponent(Shared.OperStatus);
            NumberOfSentPrinter = 0;
            ReceivedCode = 0;
        }

        private void UpdateJobInfo(JobModel jobModel)
        {
            Shared.CurrentJob = jobModel;

            txtJobName.Text = jobModel.FileName;

            // Ẩn control cũ
            lblStaticText.Visible = false;
            txtStaticText.Visible = false;
            lblPODFormat.Visible = false;
            txtPODFormat.Visible = false;
            lblTemplatePrint.Visible = false;
            txtTemplatePrint.Visible = false;
            lblJobType.Visible = false;
            txtJobType.Visible = false;
            lblCompareType.Visible = false;
            txtCompareType.Visible = false;
            label14.Visible = false;
            txtEXPDate.Visible = false;
            lblEXPDate.Visible = false;

            // Thông tin job
            txtTotalGS1.Text = jobModel.NumberTotalsCode.ToString("N0");
            txtLotNumber.Text = jobModel.BatchNumber;
            txtSalesOrder.Text = jobModel.SalesOrder;
            txtProductModel.Text = jobModel.ProductModel;

            // ── Tổng theo quy cách (giống GetPackingSummaryMessage) ───────────────
            int totalCodes = (int)jobModel.NumberTotalsCode;
            int codesPerBox = jobModel.NumberOfCodesInBox > 0 ? jobModel.NumberOfCodesInBox : 1;
            int boxesPerCarton = jobModel.NumberOfBoxesInCarton > 0 ? jobModel.NumberOfBoxesInCarton : 1;
            int cartonsPerPallet = jobModel.NumberOfCartonsInPallet > 0 ? jobModel.NumberOfCartonsInPallet : 1;

            int totalBoxesBySpec = (int)Math.Ceiling((double)totalCodes / codesPerBox);
            int totalCartonsBySpec = (int)Math.Ceiling((double)totalBoxesBySpec / boxesPerCarton);
            int totalPalletsBySpec = (int)Math.Ceiling((double)totalCartonsBySpec / cartonsPerPallet);

            // ── Mã → Hộp ────────────────────────────────────────────────────────
            int codesInBoxes = 0;
            int confirmedBoxes2 = 0;
            if (jobModel.BoxList != null)
            {
                var sentBoxes = jobModel.BoxList.Where(b => b.IsSent && b.ProductCodes != null).ToList();
                codesInBoxes = sentBoxes.Sum(b => b.ProductCodes.Count);
                confirmedBoxes2 = sentBoxes.Count;
            }
            txtCodeToBox.Text = $"{codesInBoxes} / {totalCodes}  ({confirmedBoxes2} đã quét)";

            // ── Hộp → Thùng: trái = đã IN, ngoặc = đã QUÉT ──────────────────
            int printedBoxes2 = jobModel.BoxList?.Count(b => b.sentToPrinter) ?? 0;
            int scannedBoxes2 = jobModel.BoxList?.Count(b => b.IsSent) ?? 0;
            txtBoxToCarton.Text = $"{printedBoxes2} / {totalBoxesBySpec}  ({scannedBoxes2} hộp đã quét)";

            // ── Thùng → Pallet: thùng đã vào pallet / tổng thùng  (pallet đã quét) ──
            int cartonsInPallets = 0;
            int confirmedPallets = 0;
            if (jobModel.DrocoPalletList != null)
            {
                cartonsInPallets = jobModel.DrocoPalletList.Where(p => p.CartonCodes != null).Sum(p => p.CartonCodes.Count);
                confirmedPallets = jobModel.DrocoPalletList.Count(p => p.IsSent);
            }
            txtCartonToPallet.Text = $"{cartonsInPallets} / {totalCartonsBySpec}  ({confirmedPallets} đã quét)";

            // ── Pallet: đã in / tổng  (đã quét) ──────────────────────────────────
                       // ── Pallet: đã in / tổng  (đã quét xác nhận) ─────────────────────────
            int printedPallets  = jobModel.DrocoPalletList?.Count(p => p.sentToPrinter) ?? 0;
            txtPalletConfirm.Text = $"{printedPallets} / {totalPalletsBySpec}  ({confirmedPallets} đã quét)";
        }


        private void UpdateCheckTotalAndCheckFailedLabel()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateCheckTotalAndCheckFailedLabel()));
                return;
            }

            lblCheckResultPassedValue.Text = string.Format("{0:N0}", NumberOfCheckPassed);
            lblCheckResultFailedValue.Text = string.Format("{0:N0}", (TotalChecked - NumberOfCheckPassed));
            lblTotalCheckedValue.Text = string.Format("{0:N0}", TotalChecked);
            ProgressBarCheckedUpdate();
            btnPrintCheckCode.Visible = false;
             btnPrintCheckCode.Enabled = false;
            //// Chỉ đánh giá hiện nút khi hệ thống đã STOP và có dữ liệu
            //if (Shared.OperStatus != OperationStatus.Stopped || _TotalCode <= 0)
            //{
            //    btnPrintCheckCode.Visible = false;
            //    btnPrintCheckCode.Enabled = false;
            //    return;
            //}


            //// Đếm trực tiếp từ _PrintedCodeObtainFromFile để đảm bảo đồng nhất
            //int actualPrinted = 0;
            //lock (_SyncObjCodeList)
            //{
            //    actualPrinted = _PrintedCodeObtainFromFile.Count(x => x.Length > 1 && x[1] == "Printed");
            //}

            //// Điều kiện 1: Máy in đã in xong TẤT CẢ dòng database
            //bool hasPrintedAll = actualPrinted >= _TotalCode;

            //// Điều kiện 2: Camera đã check qua hết TẤT CẢ dòng database
            //bool hasCheckedAll = TotalChecked >= _TotalCode;

            //// Điều kiện 3: Vẫn còn mã chưa pass (check Valid chưa đủ)
            //bool hasUnpassed = NumberOfCheckPassed < _TotalCode;

            //// Hiện và enable nút khi đủ CẢ BA điều kiện
            //bool canReprint = hasPrintedAll && hasCheckedAll && hasUnpassed;

            //btnPrintCheckCode.Visible = canReprint;
            //btnPrintCheckCode.Enabled = canReprint;
        }

        //private void UpdateCheckTotalAndPrintedDatabase()
        //{
        //    if (InvokeRequired)
        //    {
        //        Invoke(new Action(() => UpdateCheckTotalAndPrintedDatabase()));
        //        return;
        //    }

        //    lblReceivedValue.Text = string.Format("{0:N0}", ReceivedCode);//{0:N3} 0.000 decimal
        //    lblPrintedCodeValue.Text = string.Format("{0:N0}", NumberPrinted);//{0:N3} 0.000 decimal
        //    lblSentDataValue.Text = string.Format("{0:N0}", NumberOfSentPrinter);
        //    labelTimeSent.Text = string.Format("({0} ms)", SendPodTimeMs);
        //}
        private void UpdateCheckTotalAndPrintedDatabase()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateCheckTotalAndPrintedDatabase()));
                return;
            }

            // === 1. Số code đã gửi đến máy in ===
            lblSentDataValue.Text = string.Format("{0:N0}", NumberOfSentPrinter);

            // === 2. Số code máy in đã nhận (response) ===
            lblReceivedValue.Text = string.Format("{0:N0}", ReceivedCode);

            // === 3. Số code máy in đã in (đếm từ file PrintedResponse) ===
            int printedCount = 0;
            if (_PrintedCodeObtainFromFile != null && _PrintedCodeObtainFromFile.Count > 0)
            {
                lock (_SyncObjCodeList)
                {
                    printedCount = _PrintedCodeObtainFromFile.Count(x => x.Length > 1 && x[1] == "Printed");
                }
            }
            lblPrintedCodeValue.Text = string.Format("{0:N0}", printedCount);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void btnExportData_Click(object sender, EventArgs e)
        {
            if (Shared.OperStatus != OperationStatus.Stopped)
            {
                CustomMessageBox.Show("Vui lòng dừng công việc trước khi xuất báo cáo!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string timestamp = DateTime.Now.ToString("ddMMyyyy_HHmmss");
            var sfd = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"LOT_{_SelectedJob.BatchNumber}_BaoCao_{_SelectedJob.FileName}_{timestamp}.xlsx"

            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                LogToFile($"[btnExportData_Click] Người dùng bắt đầu xuất báo cáo: {sfd.FileName}");
                ShowLoading("Đang xuất báo cáo...\nVui lòng chờ trong giây lát");

                string errorMessage = null;
                string fileName = sfd.FileName;
                bool isSuccess = await Task.Run(() => ExportJobReportToExcel(fileName, out errorMessage));

                HideLoading();

                if (isSuccess)
                {
                    LogToFile($"[btnExportData_Click] Xuất báo cáo thành công: {fileName}");
                    CustomMessageBox.Show("Xuất báo cáo thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MoveToTheFile(fileName);
                }
                else
                {
                    LogToFile($"[btnExportData_Click] Xuất báo cáo thất bại: {fileName} - {errorMessage}");
                    CustomMessageBox.Show($"Xuất báo cáo thất bại!\n{errorMessage}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnRePrintCode_Click(object sender, EventArgs e)
        {
            if (Shared.OperStatus == OperationStatus.Running || Shared.OperStatus == OperationStatus.Processing)
            {
                CustomMessageBox.Show("Vui lòng dừng công việc trước khi thực hiện in lại mã!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_SelectedJob == null)
            {
                DesignUI.CuzAlert.CuzAlert.Show("Lỗi: Không tìm thấy phiên làm việc (Job). Vui lòng chọn Job trước khi in lại!",
                    DesignUI.CuzAlert.Alert.enmType.Error, new Size(500, 120),
                    new Point(Location.X, Location.Y), this.Size, true);
                return;
            }

            ProjectLogger.WriteInfo($"Truy cập giao diện in lại cho Job: {_SelectedJob.FileName}");

            _isReprintFormOpen = true;
            try
            {
                using (var frm = new frmReprint(_SelectedJob, _PrintedCodeObtainFromFile))
                {
                    frm.ShowDialog();
                }
            }
            finally
            {
                _isReprintFormOpen = false;
            }
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        private string GetPackingSummaryMessage(JobModel job)
        {
            int totalCodes = (int)job.NumberTotalsCode;
            int codesPerBox = job.NumberOfCodesInBox;
            int boxesPerCarton = job.NumberOfBoxesInCarton;
            int cartonsPerPallet = job.NumberOfCartonsInPallet;

            int totalBoxes = codesPerBox > 0 ? (int)Math.Ceiling((double)totalCodes / codesPerBox) : 0;
            int remainderCodes = codesPerBox > 0 ? totalCodes % codesPerBox : 0;

            int totalCartons = boxesPerCarton > 0 ? (int)Math.Ceiling((double)totalBoxes / boxesPerCarton) : 0;
            int remainderBoxes = boxesPerCarton > 0 ? totalBoxes % boxesPerCarton : 0;

            int totalPallets = cartonsPerPallet > 0 ? (int)Math.Ceiling((double)totalCartons / cartonsPerPallet) : 0;
            int remainderCartons = cartonsPerPallet > 0 ? totalCartons % cartonsPerPallet : 0;

            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"▸ Tổng số mã GS1: {totalCodes:N0}");
            sb.AppendLine();
            sb.AppendLine("▸ Quy cách đóng gói:");
            sb.AppendLine($"   • Số Mã / Hộp: {codesPerBox}");
            sb.AppendLine($"   • Số Hộp / Thùng: {boxesPerCarton}");
            sb.AppendLine($"   • Số Thùng / Pallet: {cartonsPerPallet}");
            sb.AppendLine();
            sb.AppendLine("▸ Dự kiến:");
            sb.AppendLine($"   • Tổng Hộp: {totalBoxes}" + (remainderCodes > 0 ? $" (hộp cuối lẻ {remainderCodes} mã)" : ""));
            sb.AppendLine($"   • Tổng Thùng: {totalCartons}" + (remainderBoxes > 0 ? $" (thùng cuối lẻ {remainderBoxes} hộp)" : ""));
            sb.AppendLine($"   • Tổng Pallet: {totalPallets}" + (remainderCartons > 0 ? $" (pallet cuối lẻ {remainderCartons} thùng)" : ""));
            sb.AppendLine();
            //sb.AppendLine("▸ Thông tin Batch:");
            //sb.AppendLine($"   • LOT: {job.BatchNumber}");
            //sb.AppendLine($"   • NSX: {job.ManufactureDate?.ToString("dd/MM/yyyy")}");
            //sb.AppendLine($"   • NHH: {job.ExpiryDate?.ToString("dd/MM/yyyy")}");
            sb.AppendLine();
            return sb.ToString();
        }
        private void btnConfigPacking_Click(object sender, EventArgs e)
        {
            if (_SelectedJob == null)
            {
                CustomMessageBox.Show("Không tìm thấy thông tin công việc.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string info = GetPackingSummaryMessage(_SelectedJob);
            CustomMessageBox.Show(info, "Thông tin quy cách đóng gói", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lblStatusResult_Click(object sender, EventArgs e)
        {

        }

        private void CartonListCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnJob_Click(object sender, EventArgs e)
        {

        }

        private void btnHistory_Click(object sender, EventArgs e)
        {

        }

        private void btnPrintCheckCode_Click(object sender, EventArgs e)
        {
            // ── 1. Kiểm tra trạng thái hệ thống ─────────────────────────────────
            if (Shared.OperStatus != OperationStatus.Stopped)
            {
                CustomMessageBox.Show(
                    "Vui lòng dừng công việc trước khi thực hiện in lại!",
                    Lang.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_PrintedCodeObtainFromFile.Count == 0 || _TotalCode == 0)
            {
                CustomMessageBox.Show(
                    "Không có dữ liệu database để kiểm tra.",
                    Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ── 2. Kiểm tra kết nối máy in ───────────────────────────────────────
            var productPrinter = Shared.Settings.PrinterList
                .FirstOrDefault(p => p.RoleOfPrinter == RoleOfStation.ForProduct);
            bool isPrinterConnected = productPrinter?.PODController?.IsConnected() == true;

            if (!isPrinterConnected)
            {
                CustomMessageBox.Show(
                    "Máy in sản phẩm chưa kết nối! Vui lòng kiểm tra lại.",
                    Lang.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 3. Xây dựng tập hợp mã đã kiểm tra đạt (Valid) ──────────────────
            // Dùng HashSet để lookup O(1)
            var validCodeSet = new HashSet<string>();
            lock (_SyncObjCheckedResultList)
            {
                foreach (var row in _CheckedResultCodeList)
                {
                    if (row.Length > Index_Result &&
                        row[Index_Result] == ComparisonResult.Valid.ToString())
                    {
                        validCodeSet.Add(NormalizeDataForComparison(row[Index_ResultData]));
                    }
                }
            }

            // ── 4. Tìm mã đã in nhưng chưa check đạt ────────────────────────────
            // Mã "Printed" trong DB mà compare string không có trong validCodeSet
            var codesToReprint = new List<string[]>();
            lock (_SyncObjCodeList)
            {
                foreach (var record in _PrintedCodeObtainFromFile)
                {
                    if (record.Length < 2 || record[1] != "Printed") continue;

                    // Loại cột Status (index 1) trước khi lấy compare string
                    var rowWithoutStatus = record.Where((_, idx) => idx != 1).ToArray();
                    string compareCode = NormalizeDataForComparison(
                        GetCompareDataByPODFormat(rowWithoutStatus, _SelectedJob.PODFormat));

                    if (!validCodeSet.Contains(compareCode))
                    {
                        codesToReprint.Add(record);
                    }
                }
            }

            validCodeSet.Clear();

            // ── 5. Kiểm tra có mã cần in lại không ──────────────────────────────
            if (codesToReprint.Count == 0)
            {
                CustomMessageBox.Show(
                    $"Tất cả {NumberOfCheckPassed} mã đã được kiểm tra đạt!\nKhông có mã nào cần in lại.",
                    Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ── 6. Hiển thị dialog xác nhận với thống kê ────────────────────────
            int printedCount;
            lock (_SyncObjCodeList)
            {
                printedCount = _PrintedCodeObtainFromFile.Count(x => x.Length > 1 && x[1] == "Printed");
            }

            // Lấy tối đa 5 mã đầu tiên để hiển thị preview
            string previewCodes = string.Join("\n",
                codesToReprint.Take(5).Select(r =>
                {
                    var rowWithoutStatus = r.Where((_, idx) => idx != 1).ToArray();
                    return "  • " + GetCompareDataByPODFormat(rowWithoutStatus, _SelectedJob.PODFormat);
                }));
            if (codesToReprint.Count > 5)
                previewCodes += $"\n  ... và {codesToReprint.Count - 5} mã khác";

            DialogResult confirm = CustomMessageBox.Show(
                $"Phát hiện mã đã in nhưng chưa kiểm tra đạt:\n\n" +
                $"  Tổng mã trong DB:      {_TotalCode}\n" +
                $"  Đã in:                 {printedCount}\n" +
                $"  Đã check đạt (Valid):  {NumberOfCheckPassed}\n" +
                $"  Cần in lại:            {codesToReprint.Count}\n\n" +
                $"Mã sẽ in lại:\n{previewCodes}\n\n" +
                $"Bạn có muốn gửi lệnh in lại không?",
                "Xác nhận in lại mã",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // ── 7. Cập nhật trạng thái + gửi lệnh in lại ────────────────────────
            int sentCount = 0;
            var reprintLogLines = new List<string>();
            reprintLogLines.Add($"=== In lại lúc {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Job: {_SelectedJob.FileName} ===");
            reprintLogLines.Add($"Tổng mã in lại: {codesToReprint.Count}");

            var currentPodController = productPrinter.PODController;

            foreach (var record in codesToReprint)
            {
                // a. Đổi trạng thái sang "Reprint" (hiển thị icon khác trên bảng)
                lock (_SyncObjCodeList)
                {
                    record[1] = "Reprint";
                }

                // b. Reset CompareStatus về chưa check để có thể check lại
                var rowWithoutStatus = record.Where((_, idx) => idx != 1).ToArray();
                string compareCode = NormalizeDataForComparison(
                    GetCompareDataByPODFormat(rowWithoutStatus, _SelectedJob.PODFormat));

                if (_CodeListPODFormat.TryGetValue(compareCode, out CompareStatus status))
                {
                    status.Status = false; // Cho phép check lại
                }

                // c. Gửi lệnh DATA; đến máy in
                string data = string.Join(
     Shared.Settings.SplitCharacter.ToString(),
     record.Skip(2).Select(x => ReplaceAllGS1Separators(x ?? Shared.Settings.FailedDataSentToPrinter)));
                string command = "DATA;" + data;

                currentPodController.Send(command);
                NumberOfSentPrinter++;
                sentCount++;

                reprintLogLines.Add($"[{sentCount:D4}] Index={record[0]} | Code={compareCode}");
            }

            reprintLogLines.Add("=== Kết thúc in lại ===");

            // ── 8. Ghi log ───────────────────────────────────────────────────────
            try
            {
                string logDir = Path.Combine(@"C:\ProgramData\R-Link", "Reprint");
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                string logPath = Path.Combine(logDir,
                    $"ReprintGS1_{DateTime.Now:yyyyMMdd_HHmmss}_{_SelectedJob.FileName}.txt");
                File.WriteAllLines(logPath, reprintLogLines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ghi log reprint thất bại: " + ex.Message);
            }

            // ── 9. Cập nhật UI ───────────────────────────────────────────────────
            dgvDatabase.Invalidate();
            UpdateCheckTotalAndPrintedDatabase();

            // Ẩn nút sau khi đã gửi in lại (tránh nhấn lại liên tục)
            btnPrintCheckCode.Visible = false;
            btnPrintCheckCode.Enabled = false;

            CuzAlert.Show(
                $"Đã gửi in lại {sentCount} mã!\nVui lòng chạy lại quá trình để kiểm tra.",
                Alert.enmType.Success,
                new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
        }

        private void ImgInfo_Click(object sender, EventArgs e)
        {
            if (_SelectedJob == null)
            {
                CustomMessageBox.Show("Không tìm thấy thông tin công việc.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string info = GetPackingSummaryMessage(_SelectedJob);
            CustomMessageBox.Show(info, "Thông tin quy cách đóng gói", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExportAll_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlDatabase_Paint(object sender, PaintEventArgs e)
        {

        }

        private void picDatabaseLoading_Click(object sender, EventArgs e)
        {

        }

        private void pnlControllButton_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlJobInformation_Enter(object sender, EventArgs e)
        {

        }

        private void btnExportDataRePrint_Click(object sender, EventArgs e)
        {
            if (Shared.OperStatus != OperationStatus.Stopped)
            {
                CustomMessageBox.Show("Vui lòng dừng công việc trước khi xuất dữ liệu!",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_DatabaseColunms == null || _DatabaseColunms.Length < 2 || _TotalCode == 0)
            {
                CustomMessageBox.Show("Chưa có dữ liệu. Vui lòng tải Job trước!",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── Kiểm tra file SSCC có tồn tại không ──────────────────────────────
            string ssccSource = _SelectedJob?.DirectoryDatabaseCodeSSCC;
            bool hasSsccFile = !string.IsNullOrEmpty(ssccSource) && File.Exists(ssccSource);

            // ── Dialog chọn loại file xuất ────────────────────────────────────────
            bool exportReprint = false;
            bool exportSscc = false;

            using (var dlg = new Form())
            {
                dlg.Text = "Thông báo";
                dlg.Width = 440;
                dlg.Icon= SystemIcons.Information;
                dlg.Height = 218;
                dlg.FormBorderStyle = FormBorderStyle.FixedSingle;
                dlg.BackColor = Color.White;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                // ── Dải tiêu đề màu xanh ──────────────────────────────────────────
                var pnlTitle = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 40,
                   BackColor = Color.FromArgb(0, 171, 230)
                };
                var lblTitle = new Label
                {
                    Text = "Chọn loại file xuất",
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    Font = new Font("UTM Avo", 11F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlTitle.Controls.Add(lblTitle);

                // ── Icon hỏi (giống FormMessageBox) ──────────────────────────────
                //var picIcon = new PictureBox
                //{
                //    Image = SystemIcons.Question.ToBitmap(),
                //    SizeMode = PictureBoxSizeMode.Zoom,
                //    Left = 14,
                //    Top = 54,
                //    Width = 34,
                //    Height = 34,
                //    BackColor = Color.Transparent
                //};

                // ── Checkbox nội dung (dịch sang phải để nhường chỗ icon) ─────────
                var chkReprint = new CheckBox
                {
                    Text = "Danh sách mã Honest mark chưa sử dụng",
                    Left = 58,
                    Top = 58,
                    Width = 362,
                    Checked = true,
                    Font = new Font("UTM Avo", 10F),
                    ForeColor = Color.FromArgb(30, 30, 30),
                    Cursor = Cursors.Hand
                };

                var chkSscc = new CheckBox
                {
                    Text = hasSsccFile
                        ? "Danh sách mã SSCC chưa sử dụng"
                        : "SSCC chưa sử dụng  [Không tìm thấy file SSCC nguồn]",
                    Left = 58,
                    Top = 90,
                    Width = 362,
                    Checked = hasSsccFile,
                    Enabled = hasSsccFile,
                    Font = new Font("UTM Avo", 10F),
                    ForeColor = hasSsccFile ? Color.FromArgb(30, 30, 30) : Color.Silver,
                    Cursor = hasSsccFile ? Cursors.Hand : Cursors.Default
                };

                // ── Footer ────────────────────────────────────────────────────────
                var pnlFooter = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = Color.FromArgb(245, 245, 245)
                };

                // Nút OK — xanh lá (giống Yes trong FormMessageBox)
                var btnOK = new Button
                {
                    Text = "Tiếp tục",
                    DialogResult = DialogResult.OK,
                    Width = 100,
                    Height = 32,
                    Left = 224,
                    Top = 9,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 167, 69),
                    ForeColor = Color.White,
                    Font = new Font("UTM Avo", 10F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOK.FlatAppearance.BorderSize = 0;
                btnOK.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 140, 55);

                // Nút Cancel — đỏ (giống No trong FormMessageBox)
                var btnCancel = new Button
                {
                    Text = "Hủy",
                    DialogResult = DialogResult.Cancel,
                    Width = 80,
                    Height = 32,
                    Left = 334,
                    Top = 9,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 53, 69),
                    ForeColor = Color.White,
                    Font = new Font("UTM Avo", 10F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(190, 40, 55);

                pnlFooter.Controls.AddRange(new Control[] { btnOK, btnCancel });
                dlg.Controls.AddRange(new Control[] { pnlTitle, chkReprint, chkSscc, pnlFooter });
                dlg.AcceptButton = btnOK;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                exportReprint = chkReprint.Checked;
                exportSscc = chkSscc.Checked && hasSsccFile;
            }



            if (!exportReprint && !exportSscc)
            {
                CustomMessageBox.Show("Vui lòng chọn ít nhất một loại file!",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string timestamp = DateTime.Now.ToString("ddMMyyyy_HHmmss");
            string reprintPath = null;
            string ssccPath = null;

            // ── SaveFileDialog cho file Reprint ───────────────────────────────────
            if (exportReprint)
            {
                var sfd1 = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"HonestMark_ChuaSuDung_{_SelectedJob.FileName}_{timestamp}"
                };
                if (sfd1.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(sfd1.FileName))
                    return;
                reprintPath = sfd1.FileName;
            }

            // ── SaveFileDialog cho file SSCC ──────────────────────────────────────
            if (exportSscc)
            {
                var sfd2 = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"SSCC_ChuaSuDung_{_SelectedJob.FileName}_{timestamp}"
                };
                if (sfd2.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(sfd2.FileName))
                {
                    // Đã chọn xong file Reprint → hỏi có muốn xuất tiếp không
                    if (reprintPath != null)
                    {
                        var res = CustomMessageBox.Show(
                            "Bạn đã hủy chọn đường dẫn file SSCC.\n" +
                            "Bạn có muốn tiếp tục xuất file Honest Mark không?",
                            "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (res != DialogResult.Yes) return;
                        exportSscc = false; // bỏ qua SSCC, vẫn xuất Reprint
                    }
                    else
                    {
                        return; // không có gì để xuất
                    }
                }
                else
                {
                    ssccPath = sfd2.FileName;
                }
            }

            try
            {
                EnableUIComponentWhenLoadData(false);
                var exportedFiles = new List<string>();
                string lastExportedPath = reprintPath ?? ssccPath;

                if (exportReprint)
                {
                    ExportRePrintToExcel(reprintPath);
                    exportedFiles.Add($"✓ Danh sách mã Honest mark chưa sử dụng:\n   {reprintPath}");
                    lastExportedPath = reprintPath;
                }

                if (exportSscc)
                {
                    ExportUnusedSsccToExcel(ssccPath);
                    exportedFiles.Add($"✓ Danh sách mã SSCC chưa sử dụng:\n   {ssccPath}");
                    lastExportedPath = ssccPath;
                }

                string msg = "Xuất dữ liệu thành công!\n\n" +
                             string.Join("\n\n", exportedFiles);
                CustomMessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                MoveToTheFile(lastExportedPath);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Xuất dữ liệu thất bại!\n{ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Shared.RaiseOnLogError(ex);
            }
            finally
            {
                EnableUIComponentWhenLoadData(true);
            }
        }

        /// <summary>
        /// Đọc file SSCC nguồn, lọc ra các mã chưa được gán cho Hộp/Thùng,
        /// xuất ra file Excel 1 cột (không header) tại <paramref name="filePath"/>.
        /// </summary>
        private void ExportUnusedSsccToExcel(string filePath)
        {
            string ssccSource = _SelectedJob?.DirectoryDatabaseCodeSSCC;
            if (string.IsNullOrEmpty(ssccSource) || !File.Exists(ssccSource))
                return;

            // ── 1. Tập hợp mã đã sử dụng (Hộp + Thùng) ─────────────────────────
            var usedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_SelectedJob.BoxList != null)
                foreach (var b in _SelectedJob.BoxList)
                    if (!string.IsNullOrEmpty(b.QrCode)) usedCodes.Add(b.QrCode.Trim());
            if (_SelectedJob.DrocoCartonList != null)
                foreach (var c in _SelectedJob.DrocoCartonList)
                    if (!string.IsNullOrEmpty(c.QrCode)) usedCodes.Add(c.QrCode.Trim());

            // ── 2. Đọc toàn bộ SSCC từ file nguồn ───────────────────────────────
            var cache = new ExcelCodeCache();
            cache.LoadNoHeader(ssccSource);
            int totalAll = cache.TotalRows;

            var unusedCodes = new List<string>();
            string code;
            while ((code = cache.GetNextCode(0)) != null)
            {
                if (string.IsNullOrWhiteSpace(code)) continue;
                string trimmed = code.Trim();
                if (!usedCodes.Contains(trimmed))
                    unusedCodes.Add(trimmed);
            }

            // ── 3. Xuất ra Excel (không header, dữ liệu từ dòng 1) ──────────────
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("SSCC_ChuaSuDung");
                ws.Cells.Style.Font.Name = "Arial";
                ws.Cells.Style.Font.Size = 10;

                for (int i = 0; i < unusedCodes.Count; i++)
                    ws.Cells[i + 1, 1].Value = unusedCodes[i];

                ws.Column(1).Width = 80;
                package.SaveAs(new FileInfo(filePath));
            }

            LogToFile($"[ExportUnusedSscc] Xuất {unusedCodes.Count}/{totalAll} SSCC chưa sử dụng: {filePath}");
        }


        /// <summary>
        /// Sheet 1: 1 cột, không header — chỉ các mã CHƯA hoàn thành (bỏ qua Đã in + Đã check đúng).
        /// Sheet 2: Tổng hợp đầy đủ — đánh dấu "Đã In" và "Đã Check Đúng" cho từng mã.
        /// </summary>
        //private void ExportRePrintToExcel(string filePath)
        //{
        //    // Build tập mã đã check Valid
        //    var validCodeSet = new HashSet<string>();
        //    lock (_SyncObjCheckedResultList)
        //    {
        //        foreach (var row in _CheckedResultCodeList)
        //        {
        //            if (row.Length > Index_Result &&
        //                row[Index_Result] == ComparisonResult.Valid.ToString())
        //                validCodeSet.Add(NormalizeDataForComparison(row[Index_ResultData]));
        //        }
        //    }

        //    using (var package = new ExcelPackage())
        //    {
        //        // ════════════════════════════════════════════════════════════
        //        // SHEET 1 — Danh sách mã cần in lại (1 cột, không header)
        //        // Bỏ qua: Đã in VÀ Đã check đúng
        //        // ════════════════════════════════════════════════════════════
        //        var ws1 = package.Workbook.Worksheets.Add("DanhSachMa");
        //        ws1.Cells.Style.Font.Name = "Arial";
        //        ws1.Cells.Style.Font.Size = 10;

        //        int rowIdx1 = 1;
        //        lock (_SyncObjCodeList)
        //        {
        //            foreach (var record in _PrintedCodeObtainFromFile)
        //            {
        //                if (record.Length < 2) continue;

        //                string status = record[1];
        //                var rowWithoutStatus = record.Where((_, idx) => idx != 1).ToArray();
        //                string compareCode = NormalizeDataForComparison(
        //                    GetCompareDataByPODFormat(rowWithoutStatus, _SelectedJob.PODFormat));

        //                //// Bỏ qua nếu đã in VÀ đã check đúng
        //                //if (status == "Printed" && validCodeSet.Contains(compareCode))
        //                // Bỏ qua nếu  đã check đúng
        //                if (validCodeSet.Contains(compareCode))
        //                    continue;

        //                //ws1.Cells[rowIdx1, 1].Value = ReplaceGSCharacters(compareCode);
        //                ws1.Cells[rowIdx1, 1].Value = compareCode.Replace("\\F", "\x1D");
        //                rowIdx1++;
        //            }
        //        }
        //        if (rowIdx1 > 1) // có dữ liệu
        //            ws1.Column(1).Width = 60;

        //        // ════════════════════════════════════════════════════════════
        //        // SHEET 2 — Tổng hợp đầy đủ có header + đánh dấu
        //        // ════════════════════════════════════════════════════════════
        //        var ws2 = package.Workbook.Worksheets.Add("TongHop");
        //        ws2.Cells.Style.Font.Name = "Arial";
        //        ws2.Cells.Style.Font.Size = 10;

        //        // Thống kê để ghi vào title
        //        int totalPrinted = 0;
        //        lock (_SyncObjCodeList)
        //        {
        //            totalPrinted = _PrintedCodeObtainFromFile
        //                .Count(x => x.Length > 1 && x[1] == "Printed");
        //        }
        //        int totalValid = validCodeSet.Count;

        //        // ── Build header columns (Status chuyển xuống cuối) ──────────
        //        var headerCols = new List<string>();
        //        for (int i = 0; i < _DatabaseColunms.Length; i++)
        //        {
        //            if (i != 1) headerCols.Add(_DatabaseColunms[i]);
        //        }
        //        headerCols.Add(_DatabaseColunms[1]); // Status
        //        headerCols.Add("Đã In");
        //        headerCols.Add("Đã Check Đúng");
        //        int totalCols = headerCols.Count;

        //        // ── Dòng 1: Tiêu đề tổng hợp ─────────────────────────────────
        //        ws2.Cells[1, 1, 1, totalCols].Merge = true;
        //        var titleCell = ws2.Cells[1, 1];
        //        titleCell.Value = $"TỔNG HỢP  |  Tổng mã: {_TotalCode}  |  Đã in: {totalPrinted}  |  Đã check đúng: {totalValid}  |  Chưa hoàn thành: {_TotalCode - totalValid}";
        //        titleCell.Style.Font.Bold = true;
        //        titleCell.Style.Font.Size = 12;
        //        titleCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //        titleCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        //        titleCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //        titleCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(173, 216, 230));
        //        ws2.Row(1).Height = 24;

        //        // ── Dòng 2: Header cột ────────────────────────────────────────
        //        for (int col = 0; col < headerCols.Count; col++)
        //        {
        //            var cell = ws2.Cells[2, col + 1];
        //            cell.Value = headerCols[col];
        //            cell.Style.Font.Bold = true;
        //            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(173, 216, 230));
        //            cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
        //            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //            cell.Style.WrapText = true;
        //        }
        //        ws2.Cells[2, 1, 2, totalCols].AutoFilter = true;

        //        // Cột "Đã In" và "Đã Check Đúng" — đổi màu header riêng
        //        int daInColIdx = headerCols.Count - 1; // 0-based
        //        int daCheckColIdx = headerCols.Count;     // 0-based, cuối cùng
        //        ws2.Cells[2, daInColIdx].Style.Fill.BackgroundColor
        //            .SetColor(Color.FromArgb(255, 235, 156)); // Vàng
        //        ws2.Cells[2, daCheckColIdx].Style.Fill.BackgroundColor
        //            .SetColor(Color.FromArgb(198, 239, 206)); // Xanh lá

        //        // ── Dòng 3 trở đi: Dữ liệu ───────────────────────────────────
        //        int dataRow = 3;
        //        lock (_SyncObjCodeList)
        //        {
        //            foreach (var record in _PrintedCodeObtainFromFile)
        //            {
        //                if (record.Length < 2) continue;

        //                string status = record[1];

        //                // Rearrange: Status xuống cuối
        //                var newRecord = new string[record.Length];
        //                Array.Copy(record, 0, newRecord, 0, 1);
        //                Array.Copy(record, 2, newRecord, 1, record.Length - 2);
        //                newRecord[newRecord.Length - 1] = status;

        //                var rowWithoutStatus = record.Where((_, idx) => idx != 1).ToArray();
        //                string compareCode = NormalizeDataForComparison(
        //                    GetCompareDataByPODFormat(rowWithoutStatus, _SelectedJob.PODFormat));

        //                bool isPrinted = status == "Printed";
        //                bool isValidChecked = validCodeSet.Contains(compareCode);

        //                // Ghi dữ liệu các cột DB
        //                for (int col = 0; col < newRecord.Length; col++)
        //                    //ws2.Cells[dataRow, col + 1].Value =
        //                    //    ReplaceGSCharacters(newRecord[col] ?? "");
        //                    ws2.Cells[dataRow, col + 1].Value =
        //                             (newRecord[col] ?? "").Replace("\\F", "\x1D");

        //                // Cột "Đã In"
        //                var cellDaIn = ws2.Cells[dataRow, daInColIdx];
        //                cellDaIn.Value = isPrinted ? "✓" : "";
        //                cellDaIn.Style.HorizontalAlignment =
        //                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //                if (isPrinted)
        //                {
        //                    cellDaIn.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //                    cellDaIn.Style.Fill.BackgroundColor
        //                        .SetColor(Color.FromArgb(255, 235, 156)); // Vàng
        //                }

        //                // Cột "Đã Check Đúng"
        //                var cellDaCheck = ws2.Cells[dataRow, daCheckColIdx];
        //                cellDaCheck.Value = isValidChecked ? "✓" : "";
        //                cellDaCheck.Style.HorizontalAlignment =
        //                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //                if (isValidChecked)
        //                {
        //                    cellDaCheck.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //                    cellDaCheck.Style.Fill.BackgroundColor
        //                        .SetColor(Color.FromArgb(198, 239, 206)); // Xanh lá
        //                }

        //                // Màu cả dòng theo trạng thái
        //                var rowRange = ws2.Cells[dataRow, 1, dataRow, newRecord.Length];
        //                rowRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //                if (isPrinted && isValidChecked)
        //                    rowRange.Style.Fill.BackgroundColor
        //                        .SetColor(Color.FromArgb(198, 239, 206)); // Xanh lá — Hoàn thành
        //                else if (isPrinted)
        //                    rowRange.Style.Fill.BackgroundColor
        //                        .SetColor(Color.FromArgb(255, 199, 206)); // Đỏ nhạt — Đã in, chưa check
        //                else
        //                    rowRange.Style.Fill.BackgroundColor
        //                        .SetColor(Color.FromArgb(255, 235, 156)); // Vàng — Chưa in

        //                dataRow++;
        //            }
        //        }


        //        // ── Chiều cao dòng tiêu đề và header ─────────────────────────
        //        ws2.Row(1).Height = 32;  // Title
        //        ws2.Row(2).Height = 16;  // Header

        //        // ── AutoFit có giới hạn min/max ───────────────────────────────
        //        if (ws2.Dimension != null)
        //        {
        //            // min 12 char, max 60 char
        //            ws2.Cells[ws2.Dimension.Address].AutoFitColumns(12, 60);

        //            // Cột "Đã In" — hẹp cố định
        //            ws2.Column(daInColIdx).Width = 10;
        //            // Cột "Đã Check Đúng" — hẹp cố định
        //            ws2.Column(daCheckColIdx).Width = 16;
        //        }

        //        // Sheet 1 — cột dữ liệu GS1 rộng cố định
        //        if (rowIdx1 > 1)
        //            ws1.Column(1).Width = 80;
        //        validCodeSet.Clear();
        //        package.SaveAs(new FileInfo(filePath));
        //    }
        //}

        /// <summary>
        /// Sheet 1: 1 cột, không header — các mã CHƯA được mapped vào hộp.
        /// Sheet 2: Tổng hợp đầy đủ — giữ cột "Đã Check Đúng" + thêm cột "Đã Mapped".
        /// </summary>
        private void ExportRePrintToExcel(string filePath)
        {
            // ── Build validCodeSet (camera check Valid) ───────────────────────────
            var validCodeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            lock (_SyncObjCheckedResultList)
            {
                foreach (var row in _CheckedResultCodeList)
                {
                    if (row.Length > Index_Result &&
                        row[Index_Result] == ComparisonResult.Valid.ToString())
                        validCodeSet.Add(NormalizeDataForComparison(row[Index_ResultData]));
                }
            }

            // ── Build mappedCodeSet (đã vào hộp qua AddValidCode) ────────────────
            var mappedCodeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_SelectedJob.BoxList != null)
            {
                foreach (var box in _SelectedJob.BoxList)
                {
                    if (box.ProductCodes == null) continue;
                    foreach (var code in box.ProductCodes)
                        if (!string.IsNullOrEmpty(code))
                            mappedCodeSet.Add(NormalizeDataForComparison(code));
                }
            }

            using (var package = new ExcelPackage())
            {
                // ════════════════════════════════════════════════════════════
                // SHEET 1 — Danh sách mã chưa mapped (1 cột, không header)
                // ════════════════════════════════════════════════════════════
                var ws1 = package.Workbook.Worksheets.Add("DanhSachMa");
                ws1.Cells.Style.Font.Name = "Arial";
                ws1.Cells.Style.Font.Size = 10;

                int rowIdx1 = 1;
                lock (_SyncObjCodeList)
                {
                    foreach (var record in _PrintedCodeObtainFromFile)
                    {
                        if (record.Length < 2) continue;

                        var rowWithoutStatus = record.Where((_, idx) => idx != 1).ToArray();
                        string compareCode = NormalizeDataForComparison(
                            GetCompareDataByPODFormat(rowWithoutStatus, _SelectedJob.PODFormat));

                        // Sheet 1: chỉ xuất mã chưa mapped
                        if (mappedCodeSet.Contains(compareCode))
                            continue;

                        ws1.Cells[rowIdx1, 1].Value = compareCode.Replace("\\F", "\x1D");
                        rowIdx1++;
                    }
                }
                if (rowIdx1 > 1)
                    ws1.Column(1).Width = 80;

                // ════════════════════════════════════════════════════════════
                // SHEET 2 — Tổng hợp: giữ "Đã Check Đúng" + thêm "Đã Mapped"
                // ════════════════════════════════════════════════════════════
                var ws2 = package.Workbook.Worksheets.Add("TongHop");
                ws2.Cells.Style.Font.Name = "Arial";
                ws2.Cells.Style.Font.Size = 10;

                // Thống kê
                int totalPrinted = 0;
                lock (_SyncObjCodeList)
                {
                    totalPrinted = _PrintedCodeObtainFromFile
                        .Count(x => x.Length > 1 && x[1] == "Printed");
                }
                int totalValid = validCodeSet.Count;
                int totalMapped = mappedCodeSet.Count;
                int totalUnmapped = _TotalCode - totalMapped;

                // ── Header columns: Status xuống cuối, thêm 3 cột bổ sung ────────
                var headerCols = new List<string>();
                for (int i = 0; i < _DatabaseColunms.Length; i++)
                {
                    if (i != 1) headerCols.Add(_DatabaseColunms[i]);
                }
                headerCols.Add(_DatabaseColunms[1]); // Status
                headerCols.Add("Đã In");
                headerCols.Add("Đã Check Đúng");
                headerCols.Add("Đã Mapped");
                int totalCols = headerCols.Count;

                // ── Dòng 1: Tiêu đề ──────────────────────────────────────────────
                ws2.Cells[1, 1, 1, totalCols].Merge = true;
                var titleCell = ws2.Cells[1, 1];
                titleCell.Value =
                    $"TỔNG HỢP  |  Tổng mã: {_TotalCode}  |  Đã in: {totalPrinted}" +
                    $"  |  Đã check đúng: {totalValid}" +
                    $"  |  Đã mapped vào hộp: {totalMapped}" +
                    $"  |  Chưa mapped: {totalUnmapped}";
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Font.Size = 12;
                titleCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                titleCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                titleCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                titleCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(173, 216, 230));
                ws2.Row(1).Height = 24;

                // ── Dòng 2: Header cột ────────────────────────────────────────────
                for (int col = 0; col < headerCols.Count; col++)
                {
                    var cell = ws2.Cells[2, col + 1];
                    cell.Value = headerCols[col];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(173, 216, 230));
                    cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    cell.Style.WrapText = true;
                }
                ws2.Cells[2, 1, 2, totalCols].AutoFilter = true;

                // Chỉ số 1-based cho 3 cột cuối
                int daInColIdx = headerCols.Count - 2; // "Đã In"
                int daCheckColIdx = headerCols.Count - 1; // "Đã Check Đúng"
                int daMappedColIdx = headerCols.Count;     // "Đã Mapped"

                ws2.Cells[2, daInColIdx].Style.Fill.BackgroundColor
                    .SetColor(Color.FromArgb(255, 235, 156)); // Vàng
                ws2.Cells[2, daCheckColIdx].Style.Fill.BackgroundColor
                    .SetColor(Color.FromArgb(189, 215, 238)); // Xanh dương nhạt
                ws2.Cells[2, daMappedColIdx].Style.Fill.BackgroundColor
                    .SetColor(Color.FromArgb(198, 239, 206)); // Xanh lá

                // ── Dòng 3+: Dữ liệu ─────────────────────────────────────────────
                int dataRow = 3;
                lock (_SyncObjCodeList)
                {
                    foreach (var record in _PrintedCodeObtainFromFile)
                    {
                        if (record.Length < 2) continue;

                        string status = record[1];

                        // Rearrange: Status xuống cuối
                        var newRecord = new string[record.Length];
                        Array.Copy(record, 0, newRecord, 0, 1);
                        Array.Copy(record, 2, newRecord, 1, record.Length - 2);
                        newRecord[newRecord.Length - 1] = status;

                        var rowWithoutStatus = record.Where((_, idx) => idx != 1).ToArray();
                        string compareCode = NormalizeDataForComparison(
                            GetCompareDataByPODFormat(rowWithoutStatus, _SelectedJob.PODFormat));

                        bool isPrinted = status == "Printed";
                        bool isValidChecked = validCodeSet.Contains(compareCode);
                        bool isMapped = mappedCodeSet.Contains(compareCode);

                        // Ghi dữ liệu các cột DB
                        for (int col = 0; col < newRecord.Length; col++)
                            ws2.Cells[dataRow, col + 1].Value =
                                (newRecord[col] ?? "").Replace("\\F", "\x1D");

                        // Cột "Đã In"
                        var cellDaIn = ws2.Cells[dataRow, daInColIdx];
                        cellDaIn.Value = isPrinted ? "✓" : "";
                        cellDaIn.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        if (isPrinted)
                        {
                            cellDaIn.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cellDaIn.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 235, 156));
                        }

                        // Cột "Đã Check Đúng"
                        var cellDaCheck = ws2.Cells[dataRow, daCheckColIdx];
                        cellDaCheck.Value = isValidChecked ? "✓" : "";
                        cellDaCheck.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        if (isValidChecked)
                        {
                            cellDaCheck.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cellDaCheck.Style.Fill.BackgroundColor
                                .SetColor(Color.FromArgb(189, 215, 238)); // Xanh dương nhạt
                        }

                        // Cột "Đã Mapped"
                        var cellDaMapped = ws2.Cells[dataRow, daMappedColIdx];
                        cellDaMapped.Value = isMapped ? "✓" : "";
                        cellDaMapped.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        if (isMapped)
                        {
                            cellDaMapped.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cellDaMapped.Style.Fill.BackgroundColor
                                .SetColor(Color.FromArgb(198, 239, 206)); // Xanh lá
                        }

                        // Màu cả dòng theo độ ưu tiên
                        var rowRange = ws2.Cells[dataRow, 1, dataRow, newRecord.Length];
                        rowRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        if (isMapped)
                            rowRange.Style.Fill.BackgroundColor
                                .SetColor(Color.FromArgb(198, 239, 206)); // Xanh lá — Đã mapped
                        else if (isPrinted && isValidChecked)
                            rowRange.Style.Fill.BackgroundColor
                                .SetColor(Color.FromArgb(189, 215, 238)); // Xanh dương — Đã in + check đúng, chưa mapped
                        else if (isPrinted)
                            rowRange.Style.Fill.BackgroundColor
                                .SetColor(Color.FromArgb(255, 199, 206)); // Đỏ nhạt — Đã in, chưa check, chưa mapped
                        else
                            rowRange.Style.Fill.BackgroundColor
                                .SetColor(Color.FromArgb(255, 235, 156)); // Vàng — Chưa in, chưa mapped

                        dataRow++;
                    }
                }

                ws2.Row(1).Height = 32;
                ws2.Row(2).Height = 16;

                if (ws2.Dimension != null)
                {
                    ws2.Cells[ws2.Dimension.Address].AutoFitColumns(12, 60);
                    ws2.Column(daInColIdx).Width = 10;
                    ws2.Column(daCheckColIdx).Width = 16;
                    ws2.Column(daMappedColIdx).Width = 16;
                }

                validCodeSet.Clear();
                mappedCodeSet.Clear();
                package.SaveAs(new FileInfo(filePath));
            }
        }
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
                    //string cameraName = string.Format("{0} {1}",Lang.Camera,i + 1);
                    if (cameraModel.IsConnected)
                    {
                        ShowLabelIcon(labelStatusCamera, Lang.CameraTMP, Properties.Resources.icons8_camera_30px_connected);
                    }
                    else
                    {
                        ShowLabelIcon(labelStatusCamera, Lang.CameraTMP, Properties.Resources.icons8_camera_30px_disconnected);
                        if (!cameraModel.IsConnected && !_ParentForm.isShowPopupDisConOneTime)
                        {
                            if (ProjectLabel.IsNutrifood && !Shared.Settings.IsManufacturingMode)
                            {
                                return;
                            }
                            _ParentForm.isShowPopupDisConOneTime = true;
                            CuzAlert.Show(Lang.CameraDisconnected,
                                Alert.enmType.Warning,
                                new Size(500, 120),
                                new Point(Location.X,
                                Location.Y),
                                Size,
                                true);

                        }
                        else if (cameraModel.IsConnected)
                        {
                            _ParentForm.isShowPopupDisConOneTime = false;
                        }
                    }
                }
            }
        }

        private void ShowLabelIcon(ToolStripLabel label, string text, Image icon)
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
                        if (!printerModel.IsConnected && _IsPrinterDisconnectedNot)
                        {
                            CuzAlert.Show(Lang.PrinterDisconnected, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size);
                        }

                    }
                }
            }
        }

        private void UpdateStatusLabelZebraPrinter()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatusLabelZebraPrinter()));
                return;
            }

            PrinterModel zebraPrinterModel = Shared.Settings.ZebraPrinter;
            if (zebraPrinterModel != null)
            {
                if (zebraPrinterModel.IsConnected)
                {
                    ShowLabelIcon(lblStatusPrinterZebra, "Zebra", Properties.Resources.zebra_connected_png);
                }
                else
                {
                    ShowLabelIcon(lblStatusPrinterZebra, "Zebra", Properties.Resources.zebra_disconnected);
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

        private void EnableUIComponent(OperationStatus operationStatus, bool isNonStart = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableUIComponent(operationStatus)));
                return;
            }

            bool isEnable = operationStatus == OperationStatus.Stopped;

            btnStart.Enabled = isEnable;
            btnStop.Enabled = !isEnable;
            btnTrigger.Enabled = !isEnable;
            btnJob.Enabled = isEnable;
            btnAccount.Enabled = isEnable;
            btnHistory.Enabled = isEnable;
            btnSettings.Enabled = isEnable;
            btnCustomExport.Enabled = isEnable;
            btnExportResult.Enabled = isEnable;
            btnExportAll.Enabled = isEnable;
            btnExit.Enabled = isEnable;
            btnExportData.Enabled = isEnable;
            btnExportDataRePrint.Enabled = isEnable;

            //// ── Nút in lại: chỉ hiện khi đã Stop ────────────────────────────────
            //btnRePrintCode.Visible = isEnable;
            //       btnRePrintCode.Visible = false;
            //if (isEnable)
            //{
            //    //this.btnRePrintCode.Location = new System.Drawing.Point(13, 810);
            //}

            //// ── Nút in lại mã chưa check: UpdateCheckTotalAndCheckFailedLabel tự quyết ──
            //// Chỉ reset về false khi bắt đầu chạy (không phải Stopped)
            //if (!isEnable)
            //{
            //    btnPrintCheckCode.Visible = false;
            //    btnPrintCheckCode.Enabled = false;
            //}

            if (isEnable)
            {
                ProcessUserAccess();
            }

            if (!isNonStart)
            {
                toolStripOperationStatus.Text = operationStatus.ToFriendlyString();
                toolStripOperationStatus.ForeColor = operationStatus.GetForegroundColor();
                Console.WriteLine(toolStripOperationStatus.Text);
            }
        }

        void EnableUIComponentWhenLoadData(bool isEnable)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableUIComponentWhenLoadData(isEnable)));
                return;
            }

            btnStart.Enabled = isEnable;
            btnStop.Enabled = !isEnable;
            btnTrigger.Enabled = !isEnable;
            btnJob.Enabled = isEnable;
            btnAccount.Enabled = isEnable && Shared.UserPermission.Accounts;
            btnHistory.Enabled = isEnable;
            btnSettings.Enabled = isEnable && Shared.UserPermission.Settings;
            //  btnExportData.Enabled = isEnable;
            btnCustomExport.Enabled = isEnable;
            btnExportResult.Enabled = isEnable;

            btnExit.Enabled = isEnable;

            btnDatabase.Enabled = isEnable;
            pnlPrintedCode.Enabled = isEnable;
            pnlTotalChecked.Enabled = isEnable;
            pnlCheckPassed.Enabled = isEnable;
            pnlCheckFailed.Enabled = isEnable;

            dgvDatabase.Visible = isEnable;
            dgvCheckedResult.Visible = isEnable;
            dgvDatabase.Enabled = isEnable;
            dgvCheckedResult.Enabled = isEnable;
            picDatabaseLoading.Visible = !isEnable;
            picCheckedResultLoading.Visible = !isEnable;
            if (!isEnable) Application.DoEvents();
        }

        private void SafeInvoke(Control control, Action action)
        {
            if (control == null) return;
            if (control.InvokeRequired)
                control.BeginInvoke(action);
            else
                action();
        }

        private void ProcessUserAccess()
        {
            if (Shared.LoggedInUser == null) return;

            bool isAdmin = Shared.LoggedInUser.Role == 0 || Shared.LoggedInUser.Role == 1000;

           
            btnHistory.Visible = isAdmin;
        }

        #endregion Update UI 

        #endregion
    }
}