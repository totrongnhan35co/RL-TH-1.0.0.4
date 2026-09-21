using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Interfaces;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.UDT;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.View.CustomDialogs;
using CommonVariable;
using DesignUI.CuzAlert;
using MySqlX.XDevAPI;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using OperationLog.Controller;
using OperationLog.Model;
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
using System.Net.WebSockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using OperationCanceledException = System.OperationCanceledException;
using OperationStatus = BarcodeVerificationSystem.Model.OperationStatus;
using Timer = System.Windows.Forms.Timer;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Core;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Factories;
using BarcodeVerificationSystem.Model.CodeGeneration;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model.Apis;
using BarcodeVerificationSystem.View.UtilityForms;
using BarcodeVerificationSystem.Model.Apis.Dispatching;
using BarcodeVerificationSystem.Model.Apis.Manufacturing;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request;
using BarcodeVerificationSystem.Model.Payload;
using System.Net.Sockets;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Policy;
using BarcodeVerificationSystem.View.UtilityForms.DispatchingProcess;
using static BarcodeVerificationSystem.Model.SyncDataParams;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using BarcodeVerificationSystem.Utils.ExportData.models;
using BarcodeVerificationSystem.Utils.ExportData;
using Force.DeepCloner;
using BarcodeVerificationSystem.Services.Woka;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Woka;
using BarcodeVerificationSystem.Model.Woka.Request;
using BarcodeVerificationSystem.Model.Woka.Response;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Model.Woka;
using Org.BouncyCastle.Asn1.Ocsp;
using BarcodeVerificationSystem.Model.UserInfo;

namespace BarcodeVerificationSystem.View.OtherProjects.CenteryIndiaUI
{
    public partial class FrmMainCentery : Form
    {
        #region VARIABLES DEFINITION
        private readonly FrmJobCentery _ParentForm = null;
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
        private readonly WokaApiService apiService = new WokaApiService();
        public int TotalChecked { get { return _TotalChecked; } set { _TotalChecked = value; Invoke(new Action(() => { lblTotalCheckedValue.Text = string.Format("{0:N0}", _TotalChecked); })); } }
        public int NumberOfCheckPassed { get { return _NumberOfCheckPassed; } set { _NumberOfCheckPassed = value; Invoke(new Action(() => { lblCheckResultPassedValue.Text = string.Format("{0:N0}", _NumberOfCheckPassed); })); } }
        public int NumberOfCheckFailed { get { return _NumberOfCheckFailed; } set { _NumberOfCheckFailed = value; Invoke(new Action(() => { lblCheckResultFailedValue.Text = string.Format("{0:N0}", _NumberOfCheckFailed); })); } }
        public int NumberPrinted { get { return _NumberPrinted; } set { _NumberPrinted = value; Invoke(new Action(() => { lblPrintedCodeValue.Text = string.Format("{0:N0}", _NumberPrinted); })); } }
        public int ReceivedCode { get { return _ReceivedCode; } set { _ReceivedCode = value; Invoke(new Action(() => { lblReceivedValue.Text = string.Format("{0:N0}", _ReceivedCode); })); } }

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
            get
            {
                return _NumberOfSentPrinter;
            }
            set
            {
                _NumberOfSentPrinter = value;
                Invoke(new Action(() =>
                {
                    lblSentDataValue.Text = string.Format("{0:N0}", _NumberOfSentPrinter);
                }));
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
        private FrmCheckedResultCentery _FormCheckedResult;
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
                    lblTemplatePrint.Visible = txtTemplatePrint.Visible = labelModeCheck.Visible = cuzTextBoxCheckMode.Visible = btnHistory.Visible = btnAccount.Visible = false;
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
                        lblTemplatePrint.Visible = txtTemplatePrint.Visible = labelModeCheck.Visible = cuzTextBoxCheckMode.Visible = btnHistory.Visible = btnAccount.Visible = true;
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

        public FrmMainCentery()
        {
            InitializeComponent();
        }
        private bool isDragging = false;
        private Point dragStartPoint;
        public FrmMainCentery(FrmJobCentery parentForm)
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
            if (IsCloseButtonAction == true)
            {
                DialogResult dialogResult = CustomMessageBox.Show(Lang.DoYouWantExitApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    _ParentForm.Close();
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
            InitControls();
            InitEvents();
        }
        private void InitControls()
        {
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
            UpdateStatusLabelDatabase(); // Show icon database status
            UpdateJobInfomationInterface(); // Get Job Infor
            UpdatedCodeInCartonNumber();
            UpdatedSyncCodeInCartonNumber();


            if (_SelectedJob.CompareType == CompareType.Database) // For Database Compare
            {
                tableLayoutPanelPrintedState.Visible = true;
                btnDatabase.Visible = true;
                btnExportData.Visible = true;
                btnExportAll.Visible = true;
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
            UIControlsFuncs.VisibleControl(Shared.UserPermission.Exports && Shared.Settings.ExportOneForAllEnable, btnExportAll, btnExportResult, btnExportData);
            btnAccount.Enabled = Shared.UserPermission.Accounts;
            pnlControllButton.Enabled = Shared.UserPermission.Controls;
            //UIControlsFuncs.VisibleControl(!Shared.Settings.ExportOneForAllEnable, btnExportData, btnExportResult, btnCustomExport);
            IsFullHD = true;

            CartonListCombo.Items.Clear();
            CartonListCombo.Items.AddRange(_SelectedJob.CartonList.Select(p => p.QrCode).Reverse().ToArray());
            if(CartonListCombo.Items.Count > 0) CartonListCombo.SelectedIndex = 0;

            Shared.CaoSuAllValueProcess = null;

            //Visible control in Debug mode
            UpdateCheckTotalAndPrintedDatabase();

                            // Check and update Complete Job status UI
                UpdateCompleteJobStatusUI();
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
            Shared.OnDatabaseStatusChange += Shared_OnDatabaseStatusChange;
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
            catch (Exception){}
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
                if (Shared.Settings.ZebraPrinter.PODController.IsConnected())
                {
                    Shared.PrintZebra(CurrentCarton.QrCode);
                    CurrentCarton.sentToPrinter = true;
                    Shared.CurrentJob.SaveFile();
                }

                UIControlsFuncs.UI(CartonListCombo, () => {
                    CartonListCombo.Items.Clear();
                    CartonListCombo.Items.AddRange(_SelectedJob.CartonList.Select(p => p.sentToPrinter ? $"{p.QrCode} (Đã in)" : p.QrCode).Reverse().ToArray());
                    CartonListCombo.SelectedIndex = 0;
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
                NumberOfSentPrinter++;
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

        private async void StartProcess(bool interactOnUI = true)
        {
            if (Shared.OperStatus == OperationStatus.Running || Shared.OperStatus == OperationStatus.Processing)  // Avoid start more 1 time
            {
                return;
            }

            if(_SelectedJob.CompleteJobStatus == CompleteJobStatus.Completed)
            {
                CustomMessageBox.Show("Job is completed", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Shared.Settings.PrintTemplate = "";

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

            //StartPrinting();
            Shared.OperStatus = OperationStatus.Running;


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

        private async void StartPrinting()
        {

            bool isNeedToCheckPrinter = _SelectedJob.PrinterSeries && _SelectedJob.CompareType == CompareType.Database;
            CheckPrinterSettings checkPrinterSettings = CheckAllSettingsPrinter();

            if (checkPrinterSettings != CheckPrinterSettings.Success && isNeedToCheckPrinter) // If occur error setting printer
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
                return;
            }

            if (Shared.Settings.IsPrinting && _SelectedJob.CompareType == CompareType.Database && _SelectedJob.PrinterSeries && !_IsReCheck)
            {
                Shared.OperStatus = OperationStatus.Processing;
                foreach (PODController podController in Shared.Settings.PrinterList.Select(x => x.PODController))
                {
                    podController.Send("STOP");
                    string templateName = "";
                    if (podController.RoleOfPrinter == RoleOfStation.ForProduct)
                    {
                        templateName = _SelectedJob.TemplatePrint= Shared.Settings.PrintTemplate;
                    }
                    await Task.Delay(4000);
                    //Thread.Sleep(1000);
                    if (_PrinterStatus != PrinterStatus.Stop && Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings)
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
            Shared.OperStatus = OperationStatus.Running;
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

        private async void Compare(CancellationToken token)
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
                                detectModel.CompareResult = isPosition && !isPositionCorrect
                                    ? ComparisonResult.Invalided
                                    : DatabaseCompare(detectModel.Text, ref currentCheckedIndex);

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
                                    string[] arr = await ProcessVerifyAndPrint(detectModel, currentCheckedIndex);
                                    if (detectModel.CompareResult == ComparisonResult.Valid)
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

        private async Task<string[]> ProcessVerifyAndPrint(DetectModel detectModel, int currentCheckedIndex)
        {
            string[] arr;
            int TemplateColumnIndex = _SelectedJob.TemplateIndex + 2; // 18
            string tempTemplate = Shared.Settings.PrintTemplate;



            if (detectModel.CompareResult == ComparisonResult.Valid)
            {
                if (!Shared.Settings.VerifyAndPrintBasicSentMethod)
                {
                    lock (_SyncObjCodeList)
                    {
                        arr = _PrintedCodeObtainFromFile[currentCheckedIndex];
                        if(arr[TemplateColumnIndex] != Shared.Settings.PrintTemplate)
                        {
                            Shared.Settings.PrintTemplate = arr[TemplateColumnIndex];
                            StartPrinting();
                        }
                    }

                    //if (_PrinterStatus != PrinterStatus.Printing && arr[TemplateColumnIndex] == tempTemplate)
                    //{
                    //    StartPrinting();
                    //    await Task.Delay(7000); // 9000
                    //}

                    if (arr[TemplateColumnIndex] != tempTemplate)
                    {
                        await Task.Delay(7000); // 9000
                        tempTemplate = arr[TemplateColumnIndex];
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
                // Separate the text by ; character if it contains, and only use the first one to compare
                //string tempText = txt;
                //if (!string.IsNullOrEmpty(txt) && txt.Contains(Shared.Settings.CenterIndiaModel.SplitResult))
                //{
                //    string[] fields = txt.Split(Shared.Settings.CenterIndiaModel.SplitResult);
                //    if (fields.Length > 0)
                //    {
                //        tempText = fields[0];
                //    }
                //}
                string tempText = txt;
                if (!string.IsNullOrEmpty(txt) && txt.Contains(Shared.Settings.CenterIndiaModel.SplitResult))
                {
                    string[] fields = txt.Split(new string[] { Shared.Settings.CenterIndiaModel.SplitResult }, StringSplitOptions.None);
                    if (fields.Length > 0)
                    {
                        tempText = fields[0];
                    }
                }

                var checkNull = tempText == "";
                if (!checkNull) // Check null
                {
                    if (_CodeListPODFormat.TryGetValue(tempText, out CompareStatus compareStatus))
                    {
                        if (compareStatus.Index == -1)
                        {
                            compareStatus.Index = _PrintedCodeObtainFromFile.FindIndex(x => NormalizeDataForComparison(GetCompareDataByPODFormat(x, _SelectedJob.PODFormat)) == tempText);
                        }

                        if (!compareStatus.Status) // Check duplicate
                        {
                            _CodeListPODFormat[tempText].Status = true;
                            currentValidIndex = compareStatus.Index;
                            return ComparisonResult.Valid;
                        }
                        else
                        {
                            if (Shared.Settings.AllowDupAndNonStop)
                            {
                                _CodeListPODFormat[tempText].Status = true;
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
            List<string[]> strPrintedResponseList = new List<string[]>();
            var isAutoComplete = _SelectedJob.CompareType == CompareType.Database; // Check if need to auto stop procces when compare type is verify and print
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
                        
                        NumberPrinted++;
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
                            if (_PrintedCodeObtainFromFile[compareStatus.Index][1] == "Waiting")
                            {
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
                                Invoke(new Action(() =>
                                {
                                    dgvDatabase.Invalidate();
                                    dgvDatabase.Rows[currentIndex].Cells[0].Selected = true;
                                }));
                            }

                            // Send backup data to backup thread
                            _QueueBufferBackupPrintedCode.Enqueue(new List<string[]>(strPrintedResponseList)); // Enqueue backup data
                                                                                                               // Clear list
                            strPrintedResponseList.Clear();
                        }
                    }

                    //Time delay avoid frezee user interface
                    Thread.Sleep(5);
                }
            }
            catch (System.OperationCanceledException)
            {
                Console.WriteLine("Thread update printed status was stoppped!");
                _BackupResponseCancelTokenSource?.Cancel();
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

                    //END Add row

                    //Update value to user interface
                    Invoke(new Action(() =>
                    {
                        txtCodeResult.Text = detectModel.Text;
                        txtProcessingTimeResult.Text = (detectModel.CompareTime) + " ms";
                        txtBarcodeQuality.Text = detectModel.CodeQuality;
                        txtStatusResult.Text = detectModel.CompareResult.ToFriendlyString();
                        txtStatusResult.ForeColor = detectModel.CompareResult == ComparisonResult.Valid ? Color.FromArgb(0, 199, 82) : Color.Red;
                    }));

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

        private async void ExportPrintedResponseToFileAsync(CancellationToken token)
        {
            await Task.Run(() => { NewExportPrintedResponseToFile(token); });
        }

        private PrintingQueueProcessor _printedDataProcess;
        private void NewExportPrintedResponseToFile(CancellationToken token)
        {
            if (_SelectedJob.PrintedResponePath == "")
            {
                string fileName = DateTime.Now.ToString(_DateTimeFormat) + "_Printed_" + _SelectedJob.FileName;
                string path = CommVariables.PathPrintedResponse + fileName + ".csv";

                // Determine whether the directory exists.
                if (!Directory.Exists(CommVariables.PathPrintedResponse))
                {
                    // Try to create the directory.
                    Directory.CreateDirectory(CommVariables.PathPrintedResponse);
                }

                // This text is added only once to the file.
                if (!File.Exists(path))
                {
                    // Create a file to write to.
                    using (StreamWriter streamWriter = new StreamWriter(path, true, new UTF8Encoding(true)))
                    {
                        //Add header
                        streamWriter.WriteLine(String.Join(",", _DatabaseColunms));
                    }
                }
                if (Shared.CaoSuAllValueProcess == null)
                    Shared.CaoSuAllValueProcess = new CaoSuAllValueProcess(_SelectedJob);

                _SelectedJob.PrintedResponePath = fileName + ".csv";
                _SelectedJob.SaveFile();
            }

            try
            {
                string path = CommVariables.PathPrintedResponse + _SelectedJob.PrintedResponePath;

                while (true)
                {
                    // Only stop if handled all data
                    if (token.IsCancellationRequested)
                        if (_QueueBufferBackupPrintedCode.Count() == 0)
                            token.ThrowIfCancellationRequested();

                    List<string[]> valueArr = _QueueBufferBackupPrintedCode.Dequeue();
                    if (valueArr == null) continue;
                    var clone = valueArr.Select(arr => arr.ToArray()).ToList();
                    if (valueArr.Count() > 0)
                    {
                        SaveResultToFile(valueArr, path);
                        try
                        {
                            Shared.CaoSuAllValueProcess.UpdatePrint(clone[0][2], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        catch (Exception)
                        {
                        }
                    }
                    valueArr.Clear();
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
                string path = CommVariables.PathCheckedResult + _SelectedJob.CheckedResultPath;

                if (!Directory.Exists(CommVariables.PathSentDataChecked))
                {
                    Directory.CreateDirectory(CommVariables.PathSentDataChecked);
                }
                string sentDataPath = CommVariables.PathSentDataChecked + _SelectedJob.CheckedResultPath;

                while (true)
                {
                    // Only stop if handled all data
                    if (token.IsCancellationRequested)
                        if (_QueueBufferBackupCheckedResult.Count() == 0)
                            token.ThrowIfCancellationRequested();

                    List<string[]> valueArr = _QueueBufferBackupCheckedResult.Dequeue();
                    //var apiService = new ApiService();

                    if (valueArr == null) continue;
                    var clone = valueArr.Select(arr => arr.ToArray()).ToList(); 
                    if (valueArr.Count() > 0)
                    {
                        SaveResultToFile(valueArr, path);
                    }
                    valueArr.Clear();
                    Thread.Sleep(5);
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
            await Task.Run(() => StopProcess(interactOnUI, messages, isClosed, isManualClose));
            Shared.RaiseOnStopButtonClick();
        }

        private void StopProcess(bool interactOnUI = true, string messages = "", bool isClosed = false, bool isManualClose = false)
        {
            if (interactOnUI)
            {
                DialogResult dialogResult = DialogResult.None;
                if (dialogResultStopExist) { return; }
                dialogResultStopExist = true;

                this.Invoke((MethodInvoker)delegate
                {
                    dialogResult = CustomMessageBox.Show(Lang.DoYouWantToStopTheSystem, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                });
                if (dialogResult != DialogResult.Yes)
                {
                    dialogResultStopExist = false;
                    return;
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
                            Monitor.Wait(_StopLocker, 1000); //Wait until there is a stop notify from the printer (old is 3000)
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

                InitDataAsync(_SelectedJob);
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
                            CuzAlert.Show(value, Alert.enmType.Error, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
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
            DataGridViewCustom.AdjustColumnWidthsToFitContent(dgvCheckedResult);
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

            // Replace <0x1D> string representation with \F
            return input.Replace("<0x1D>", "\\F");
        }

        /// <summary>
        /// Initializes database from Excel file (.xlsx or .xls)
        /// Uses ExcelDataReader - no external dependencies required
        /// </summary>
        private List<string[]> InitDatabaseFromExcel(string path, bool isFirstRowHeader)
        {
            List<string[]> result = new List<string[]>();

            try
            {
                List<string[]> allRows = FileFuncs.ReadExcelData(path, null);

                if (allRows == null || allRows.Count == 0)
                {
                    // Empty file or read failed
                    _InitDataErrorList.Add(InitDataError.DatabaseUnknownError);
                    return result;
                }

                // Get header row
                string[] headerRow = FileFuncs.GetFirstRowFromExcel(path, true);
                if (headerRow == null || headerRow.Length == 0)
                {
                    _InitDataErrorList.Add(InitDataError.DatabaseUnknownError);
                    return result;
                }

                int columnCount = 0;
                int lineCounter = -1;

                // Process header row
                if (isFirstRowHeader)
                {
                    var tmp = new string[headerRow.Length + 2];
                    tmp[0] = "Index";
                    tmp[1] = "Status";
                    for (int i = 2; i < tmp.Length; i++)
                    {
                        tmp[i] = ReplaceGSCharacters(headerRow[i - 2]) + $" - Field{i - 1}";
                    }
                    columnCount = tmp.Length;
                    result.Add(tmp);
                }
                else
                {
                    var tmp = new string[headerRow.Length + 2];
                    tmp[0] = "Index";
                    tmp[1] = "Status";
                    for (int i = 2; i < tmp.Length; i++)
                    {
                        tmp[i] = $"Field{i - 1}";
                    }
                    columnCount = tmp.Length;
                    result.Add(tmp);
                }

                // Process data rows
                foreach (var row in allRows)
                {
                    lineCounter++;
                    string[] line = row;

                    if (isFirstRowHeader)
                    {
                        var tmp1 = new string[columnCount];
                        tmp1[0] = "" + lineCounter;
                        tmp1[1] = "Waiting";
                        for (int i = 2; i < tmp1.Length; i++)
                        {
                            if (i - 2 < line.Length)
                            {
                                tmp1[i] = ReplaceGSCharacters(line[i - 2]);
                            }
                            else
                            {
                                tmp1[i] = "";
                            }
                        }
                        result.Add(tmp1);
                    }
                    else
                    {
                        var tmp1 = new string[columnCount];
                        tmp1[0] = "" + (lineCounter + 1);
                        tmp1[1] = "Waiting";
                        for (int i = 2; i < tmp1.Length; i++)
                        {
                            if (i - 2 < line.Length)
                            {
                                tmp1[i] = ReplaceGSCharacters(line[i - 2]);
                            }
                            else
                            {
                                tmp1[i] = "";
                            }
                        }
                        result.Add(tmp1);
                    }
                }
            }
            catch (Exception ex)
            {
                // General error handling - ExcelDataReader handles errors gracefully
                _InitDataErrorList.Add(InitDataError.DatabaseUnknownError);
                Debug.WriteLine($"Excel loading error: {ex.Message}");
                // Don't show error message to user - FileFuncs already handles errors gracefully
            }

            return result;
        }

        private List<string[]> InitDatabaseFromCsv(string path, bool isFirstRowHeader)
        {
            List<string[]> result = new List<string[]>();

            try
            {
                using (var reader = new StreamReader(path, Encoding.UTF8, true))
                {
                    var rexCsvSplitter = path.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");
                    int lineCounter = -1;
                    int columnCount = 0;
                    while (!reader.EndOfStream)
                    {
                        string[] line = rexCsvSplitter.Split(reader.ReadLine()).Select(x => Csv.Unescape(x)).ToArray();
                        lineCounter++;
                        if (lineCounter == 0)
                        {
                            if (isFirstRowHeader)
                            {
                                var tmp = new string[line.Length + 2];
                                tmp[0] = "Index";
                                tmp[1] = "Status";
                                for (int i = 2; i < tmp.Length; i++)
                                {
                                    tmp[i] = ReplaceGSCharacters(line[i - 2]) + $" - Field{i - 1}";
                                }
                                columnCount = tmp.Length;
                                result.Add(tmp);
                            }
                            else
                            {
                                var tmp = new string[line.Length + 2];
                                tmp[0] = "Index";
                                tmp[1] = "Status";
                                for (int i = 2; i < tmp.Length; i++)
                                {
                                    tmp[i] = $"Field{i - 1}";
                                }
                                columnCount = tmp.Length;
                                result.Add(tmp);
                            }

                        }
                        else
                        {
                            if (isFirstRowHeader)
                            {
                                var tmp1 = new string[columnCount];
                                tmp1[0] = "" + lineCounter;
                                tmp1[1] = "Waiting";
                                for (int i = 2; i < tmp1.Length; i++)
                                {
                                    if (i - 2 < line.Length)
                                    {
                                        tmp1[i] = ReplaceGSCharacters(line[i - 2]);
                                    }
                                    else
                                    {
                                        tmp1[i] = "";
                                    }
                                }
                                result.Add(tmp1);
                            }
                        }

                        if (!isFirstRowHeader)
                        {
                            var tmp1 = new string[columnCount];
                            tmp1[0] = "" + (lineCounter + 1);
                            tmp1[1] = "Waiting";
                            for (int i = 2; i < tmp1.Length; i++)
                            {
                                if (i - 2 < line.Length)
                                {
                                    tmp1[i] = ReplaceGSCharacters(line[i - 2]);
                                }
                                else
                                {
                                    tmp1[i] = "";
                                }
                            }
                            result.Add(tmp1);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Error handling is done in calling method
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
                    _FormCheckedResult = new FrmCheckedResultCentery
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
                if (CurrentUser.UserName.ToLower() == "operator")
                {
                    CustomMessageBox.Show("Current User Does Not Have Permission For This Action!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

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
                ExportDataAsync();
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
                Shared.ExportCheckedResult(filePathCheckResult);
            }
            else if (sender == btnPrintCarton)
            {
                var item = CartonListCombo.SelectedItem;
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

                    Shared.PrintZebra(qrCode);
                    Shared.CurrentJob.CartonList.Find(x => x.QrCode == qrCode).sentToPrinter = true;
                    Shared.CurrentJob.SaveFile();

                    // Find and mark the carton as sentToPrinter
                    var carton = _SelectedJob.CartonList.FirstOrDefault(c => c.QrCode == qrCode);
                    if (carton != null)
                    {
                        carton.sentToPrinter = true;
                        
                        // Update the combo box display
                        UIControlsFuncs.UI(CartonListCombo, () => {
                            int selectedIndex = CartonListCombo.SelectedIndex;
                            CartonListCombo.Items.Clear();
                            CartonListCombo.Items.AddRange(_SelectedJob.CartonList.Select(p => p.sentToPrinter ? $"{p.QrCode} (Đã in)" : p.QrCode).Reverse().ToArray());
                            // Try to maintain selection
                            if (selectedIndex >= 0 && selectedIndex < CartonListCombo.Items.Count)
                            {
                                CartonListCombo.SelectedIndex = selectedIndex;
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
            if ((Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing) || Shared.Settings.CenterIndiaModel.IsScanner ) return;
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
            if ((Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing) || Shared.Settings.CenterIndiaModel.IsCamera) return; // || !_IsReCheck
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
                    //CustomMessageBox.Show(detectModel.Text, "test", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                        if (podDataModel.RoleOfPrinter == RoleOfStation.ForProduct && !_IsVerifyAndPrintMode)
                                        {
                                            SendDataToPrinterAsync(); // Send POD data to printer when printer ready receive data
                                        }
                                    }
                                    else
                                    {
                                        PODResponseModel.Error = pODcommand[2];
                                        var message = "Unknown";
                                        switch (PODResponseModel.Error)
                                        {
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
                                        //Invoke(new Action(() =>
                                        //{
                                        //    StopProcessAsync(false, "Printer stops suddenly!", false, true);
                                        //}));
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

        private async void SendDataToPrinterAsync()
        {
            _SendDataToPrinterTokenCTS = new CancellationTokenSource();
            var token = _SendDataToPrinterTokenCTS.Token;
            await Task.Run(() => { SendPODDataProductToPrinter(token); });
        }

        private void SendPODDataProductToPrinter(CancellationToken token)
        {
            Thread.Sleep(500);  // Wait printer ready
            int counter = 0;
            List<string[]> codeList = null;
            List<string> tmpListLog = new List<string>();
            lock (_SyncObjCodeList)
            {
                codeList = new List<string[]>(_PrintedCodeObtainFromFile); //Clone list
            }
            lock (_PrintLocker)
            {
                _IsPrintedWait = false;
                _PrintedResult = ComparisonResult.Valid;
            }
            try
            {
                var spinWait = new SpinWait();
                int startIndex = codeList.FindIndex(x => x[1] != "Printed"); // Get the first not-printed code in database 
                if (startIndex == -1) return;
                _IsPrintedWait = true; // Wait for 100 code first
                for (int codeIndex = startIndex; codeIndex < codeList.Count(); codeIndex++)
                {
                    //   stopw = Stopwatch.StartNew();
                    token.ThrowIfCancellationRequested();
                    string[] codeModel = codeList[codeIndex]; // Last index of valid code
                    int statusIndex = 1;
                    if (codeModel[statusIndex] != "Printed" && (codeModel[statusIndex] != "Duplicate" || Shared.Settings.DuplicatedDBEnable)) // Check if current code is printed or duplicate 
                    {
                        //  string data = "";
                        token.ThrowIfCancellationRequested();
                        string data = string.Join(Shared.Settings.SplitCharacter.ToString(), codeModel.Skip(2).ToArray());// Init send data
                        //string command = string.Format("DATA;{0}", data); // Init send command
                        string command = $"DATA;{data}";

                        if (podController != null)
                        {
                            podController.Send(command);
                            NumberOfSentPrinter++;
                            tmpListLog = command.Split(Shared.Settings.SplitCharacter).Skip(1).ToList();
                            tmpListLog.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                            _QueueBufferBackupSendLog.Enqueue(tmpListLog.ToArray());
                            tmpListLog.Clear();
                        }
                        counter++;
                        if (Shared.OperStatus == OperationStatus.Processing) // Change operation status
                        {
                            if (counter >= Shared.Settings.PrinterList[0].NumberBuffer1StSend && _IsAfterProductionMode)  // Check allow system runing, not waiting util send data complete
                            {
                                Shared.OperStatus = OperationStatus.Running; // Update user interface the system is ready
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
                        else if (_IsAfterProductionMode)
                        {
                            if (counter < Shared.Settings.PrinterList[0].NumberBuffer1StSend)
                            {
                                Thread.Sleep(Shared.Settings.PrinterList[0].TimeDelaySendFirstBuffer);
                            }
                            else
                            {
                                while (true)
                                {
                                    if (_queueCountFeedback.TryDequeue(out int res))
                                    {
                                        break;
                                    }
                                    if (!Shared.Settings.PrinterList[0].EnableSendTurboSpeed) // Use mode wait data mode
                                    {
                                        spinWait.SpinOnce();
                                    }
                                }
                            }
                        }
                    }
                }

                if (Shared.OperStatus == OperationStatus.Processing)
                {
                    Shared.OperStatus = OperationStatus.Running;  // Update user interface the system is ready
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
                Console.WriteLine("Thread send data to printer was error!");  // Catch Error - Add by ThongThach 05/12/2023
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
                    Invoke(new Action(() => { _FormCheckedResult.Close(); }));
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
                await Task.Run(() => {
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

        private void Shared_OnDatabaseStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelDatabase();
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
            lblJobType.Text = Lang.JobType;
            btnJob.Text = Lang.Operation;
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
            lblCompareType.Text = Lang.CompareType;
            lblPODFormat.Text = "Số lượng mã đã vào thùng"; //Lang.PODFormat
            lblTemplatePrint.Text = "Số lượng mã đã đồng bộ"; // Lang.TemplateName

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
            txtJobType.Text = _SelectedJob.JobType.ToFriendlyString();
            lblStaticText.Text = "Tổng mã cần in"; // Lang.Totals
            switch (_SelectedJob.CompareType)
            {
                case CompareType.CanRead:
                    txtCompareType.Text = Lang.CanRead;
                    break;
                case CompareType.StaticText:
                    txtCompareType.Text = Lang.StaticText;
                    break;
                default:
                    txtCompareType.Text = Lang.Database;
                    break;
            }

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
        private void btnCompleteJob_Click(object sender, EventArgs e)
        {
            try
            {
                if(Shared.OperStatus != OperationStatus.Stopped)
                {
                    CustomMessageBox.Show("Please stop the process before completing!", Lang.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var resultMessage = CustomMessageBox.IsResultShow("Are you sure you want to complete this job?\n" +
                                                                    $"Note: You cannot continue processing once it is completed.");
                if (!resultMessage) return;

                if (Shared.CaoSuAllValueProcess == null)
                    Shared.CaoSuAllValueProcess = new CaoSuAllValueProcess(_SelectedJob);

                var printedList = Shared.CaoSuAllValueProcess.GetPrintedQRCodes();
                string tableName = _SelectedJob.TableName;
                LogToFile(" printedList.Count: " + printedList.Count + " tableName "+ tableName);
                // Update database with printed codes
                if (!string.IsNullOrEmpty(tableName) && printedList != null && printedList.Count > 0)
                {
                    DatabaseUpdateResult updateResult = UpdateDatabasePrintedCodes(tableName, printedList);
                    
                    string message = "";
                    MessageBoxIcon icon = MessageBoxIcon.Information;
                    
                    if (updateResult.Success)
                    {
                        message = $"Database updated successfully!\n\n";
                        message += $"Total codes: {updateResult.TotalCount}\n";
                        message += $"Updated: {updateResult.UpdatedCount}\n";
                        
                        if (updateResult.FailedCodes.Count > 0)
                        {
                            message += $"Failed: {updateResult.FailedCodes.Count}\n\n";
                            message += "Codes that could not be updated:\n";
                            int displayCount = Math.Min(updateResult.FailedCodes.Count, 10); // Show max 10 codes
                            for (int i = 0; i < displayCount; i++)
                            {
                                message += $"- {updateResult.FailedCodes[i]}\n";
                            }
                            if (updateResult.FailedCodes.Count > 10)
                            {
                                message += $"... and {updateResult.FailedCodes.Count - 10} more.";
                            }
                            icon = MessageBoxIcon.Warning;
                        }
                    }
                    else
                    {
                        message = "Failed to update database. Please check the logs for details.";
                        if (updateResult.FailedCodes.Count > 0)
                        {
                            message += $"\n\nTotal codes: {updateResult.TotalCount}\n";
                            message += $"Updated: {updateResult.UpdatedCount}\n";
                            message += $"Failed: {updateResult.FailedCodes.Count}\n\n";
                            message += "Codes that could not be updated:\n";
                            int displayCount = Math.Min(updateResult.FailedCodes.Count, 10);
                            for (int i = 0; i < displayCount; i++)
                            {
                                message += $"- {updateResult.FailedCodes[i]}\n";
                            }
                            if (updateResult.FailedCodes.Count > 10)
                            {
                                message += $"... and {updateResult.FailedCodes.Count - 10} more.";
                            }
                        }
                        icon = MessageBoxIcon.Error;
                    }
                    
                    CustomMessageBox.Show(message, updateResult.Success ? "Success" : "Error", MessageBoxButtons.OK, icon);
                }

                _SelectedJob.CompleteJobStatus = CompleteJobStatus.Completed;
                _SelectedJob.SaveFile();
                UpdateCompleteJobStatusUI();
                LogToFile($"Job Completed");

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
        private void LogToFile(string message)
        {
            try
            {
                string directory = @"C:\ProgramData\R-Link";
                string filePath = Path.Combine(directory, "Data.txt");

                // Create directory if it doesn't exist
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create file if it doesn't exist (append will create it automatically)
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";

                File.AppendAllText(filePath, logEntry);
            }
            catch
            {
                // Silent fail - don't crash the app if logging fails
                // Optionally, you could show a message or log to event viewer here
            }
        }

        private class DatabaseUpdateResult
        {
            public bool Success { get; set; }
            public int UpdatedCount { get; set; }
            public int TotalCount { get; set; }
            public List<string> FailedCodes { get; set; } = new List<string>();
        }

        private DatabaseUpdateResult UpdateDatabasePrintedCodes(string tableName, List<string> printedCodes)
        {
            var result = new DatabaseUpdateResult();
            try
            {
                var dbModel = Shared.Settings.CenterIndiaModel;

                // Check if database settings are configured
                if (string.IsNullOrEmpty(dbModel.ServerName) ||
                    string.IsNullOrEmpty(dbModel.DatabaseName) ||
                    string.IsNullOrEmpty(dbModel.Username) ||
                    string.IsNullOrEmpty(tableName))
                {
                    LogToFile($"UpdateDatabasePrintedCodes: Database settings or table name not configured");
                    result.Success = false;
                    return result;
                }

                string connectionString = GetDatabaseConnectionString();
                string databaseType = dbModel.DatabaseType ?? "MySQL";

                using (IDbConnection connection = GetDatabaseConnection(connectionString))
                {
                    connection.Open();

                    // Get the first column name from the table
                    string firstColumnName = GetFirstColumnName(connection, tableName, databaseType);
                    if (string.IsNullOrEmpty(firstColumnName))
                    {
                        LogToFile($"UpdateDatabasePrintedCodes: Could not get first column name from table {tableName}");
                        result.Success = false;
                        return result;
                    }

                    // Update each matching code
                    foreach (string codeWithDate in printedCodes)
                    {
                        if (string.IsNullOrEmpty(codeWithDate))
                            continue;

                        result.TotalCount++;

                        // Parse the code and date (format: "qrcode,printedDate")
                        string[] parts = codeWithDate.Split(new[] { ',' }, 2);
                        if (parts.Length < 2)
                        {
                            LogToFile($"UpdateDatabasePrintedCodes: Invalid format for code entry: {codeWithDate}");
                            result.FailedCodes.Add(codeWithDate);
                            continue;
                        }

                        string code = parts[0];
                        string printedDate = parts[1];

                        if (string.IsNullOrEmpty(code))
                        {
                            result.FailedCodes.Add(codeWithDate);
                            continue;
                        }

                        string lineNameStr = Shared.Settings.CenterIndiaModel.LineName;
                        int mcno = int.TryParse(lineNameStr, out int parsed) && parsed >= 1 && parsed <= 999 ? parsed : 0;

                        string updateQuery;
                        if (databaseType.ToLower() == "sql")
                        {
                            updateQuery = $"UPDATE [{tableName}] SET [qr_print] = 'Y', [qr_print_datetime] = @printedDate, [qr_print_mcno] = @mcno WHERE [{firstColumnName}] = @code";
                        }
                        else // MySQL
                        {
                            updateQuery = $"UPDATE `{tableName}` SET `qr_print` = 'Y', `qr_print_datetime` = @printedDate, `qr_print_mcno` = @mcno WHERE `{firstColumnName}` = @code";
                        }

                        bool codeUpdated = false;
                        using (IDbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = updateQuery;
                            
                            // Add parameter for the code
                            IDbDataParameter codeParam = command.CreateParameter();
                            codeParam.ParameterName = "@code";
                            codeParam.Value = code;
                            command.Parameters.Add(codeParam);

                            // Add parameter for the printed date
                            IDbDataParameter dateParam = command.CreateParameter();
                            dateParam.ParameterName = "@printedDate";
                            
                            // Try to parse the date, if it fails, use the string as-is or set to NULL
                            if (DateTime.TryParse(printedDate, out DateTime parsedDate))
                            {
                                dateParam.Value = parsedDate;
                            }
                            else
                            {
                                dateParam.Value = printedDate; // Use as string if parsing fails
                            }
                            command.Parameters.Add(dateParam);

                            IDbDataParameter mcnoParam = command.CreateParameter();
                            mcnoParam.ParameterName = "@mcno";
                            mcnoParam.Value = mcno;
                            command.Parameters.Add(mcnoParam);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                codeUpdated = true;
                                result.UpdatedCount++;
                            }
                        }

                        // If code was not updated, add it to failed codes list
                        if (!codeUpdated)
                        {
                            result.FailedCodes.Add(code);
                        }
                    }

                    LogToFile($"UpdateDatabasePrintedCodes: Updated {result.UpdatedCount} out of {result.TotalCount} codes in table {tableName}");
                    
                    // Consider it successful if at least some codes were updated
                    result.Success = result.UpdatedCount > 0;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogToFile($"UpdateDatabasePrintedCodes Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                result.Success = false;
                return result;
            }
        }

        private string GetFirstColumnName(IDbConnection connection, string tableName, string databaseType)
        {
            try
            {
                string query;
                if (databaseType.ToLower() == "sql")
                {
                    query = $"SELECT TOP 1 COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' ORDER BY ORDINAL_POSITION";
                }
                else // MySQL
                {
                    query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}' ORDER BY ORDINAL_POSITION LIMIT 1";
                }

                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    object result = command.ExecuteScalar();
                    return result?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                LogToFile($"GetFirstColumnName Error: {ex.Message}");
                return string.Empty;
            }
        }

        private string GetDatabaseConnectionString()
        {
            var dbModel = Shared.Settings.CenterIndiaModel;
            string databaseType = dbModel.DatabaseType ?? "MySQL";
            string serverName = dbModel.ServerName;
            string port = dbModel.Port;
            string username = dbModel.Username;
            string password = dbModel.Password;
            string databaseName = dbModel.DatabaseName;

            switch (databaseType.ToLower())
            {
                case "sql":
                    return $"Server={serverName};Database={databaseName};User Id={username};Password={password};";
                case "mysql":
                    return $"Server={serverName};Port={port};Database={databaseName};Uid={username};Pwd={password};";
                default:
                    return $"Server={serverName};Port={port};Database={databaseName};Uid={username};Pwd={password};";
            }
        }

        private IDbConnection GetDatabaseConnection(string connectionString)
        {
            var dbModel = Shared.Settings.CenterIndiaModel;
            string databaseType = dbModel.DatabaseType ?? "MySQL";

            switch (databaseType.ToLower())
            {
                case "sql":
                    return new SqlConnection(connectionString);
                case "mysql":
                    return new MySqlConnection(connectionString);
                default:
                    return new MySqlConnection(connectionString);
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
                completedStatus.Text = "Job's Completed";
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
            txtTemplatePrint.Text = jobModel.TemplatePrint;
            txtPODFormat.Text = "";

            foreach (PODModel item in jobModel.PODFormat)
            {
                txtPODFormat.Text += item.ToStringSample();
            }
            if (jobModel.CompareType == CompareType.CanRead)
            {
                lblStaticText.Text = Lang.StaticText;
                txtStaticText.Text = jobModel.StaticText;
                txtCompareType.Text = Lang.CanRead;
                txtJobType.Text = "";
                txtPODFormat.BackColor = Color.WhiteSmoke;
                txtStaticText.BackColor = Color.WhiteSmoke;
            }
            else if (jobModel.CompareType == CompareType.StaticText)
            {
                lblStaticText.Text = Lang.StaticText;
                txtStaticText.Text = jobModel.StaticText;
                txtCompareType.Text = Lang.StaticText;
                txtJobType.Text = "";
                txtPODFormat.BackColor = Color.WhiteSmoke;
                txtStaticText.BackColor = Color.White;
            }
            else
            {
                lblStaticText.Text = Lang.Totals;
                txtCompareType.Text = Lang.Database;
                txtStaticText.Text = jobModel.NumberTotalsCode.ToString();
                txtJobType.Text = jobModel.JobType.ToFriendlyString();
                txtPODFormat.BackColor = Color.White;
                txtStaticText.BackColor = Color.White;
            }

            if (_SelectedJob.JobType == JobType.StandAlone)
            {
                txtTemplatePrint.BackColor = Color.WhiteSmoke;
                txtJobType.BackColor = Color.WhiteSmoke;
            }
            else
            {
                txtStaticText.BackColor = Color.White;
                txtJobType.BackColor = Color.White;
            }
        }

        private void UpdateCheckTotalAndCheckFailedLabel()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateCheckTotalAndCheckFailedLabel()));
                return;
            }

            lblCheckResultPassedValue.Text = string.Format("{0:N0}", NumberOfCheckPassed);//{0:N3} 0.000 decimal
            lblCheckResultFailedValue.Text = string.Format("{0:N0}", (TotalChecked - NumberOfCheckPassed));
            lblTotalCheckedValue.Text = string.Format("{0:N0}", TotalChecked);
            ProgressBarCheckedUpdate();
        }

        private void UpdateCheckTotalAndPrintedDatabase()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateCheckTotalAndPrintedDatabase()));
                return;
            }

            lblReceivedValue.Text = string.Format("{0:N0}", ReceivedCode);//{0:N3} 0.000 decimal
            lblPrintedCodeValue.Text = string.Format("{0:N0}", NumberPrinted);//{0:N3} 0.000 decimal
            lblSentDataValue.Text = string.Format("{0:N0}", NumberOfSentPrinter);
            labelTimeSent.Text = string.Format("({0} ms)", SendPodTimeMs);
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
                            CuzAlert.Show(Lang.PrinterDisconnected, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
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

        private void EnableUIComponent(OperationStatus operationStatus, bool isNonStart = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableUIComponent(operationStatus)));
                return;
            }
            bool isEnable = false;
            if (operationStatus != OperationStatus.Stopped)
            {
                isEnable = false;
            }
            else
            {
                isEnable = true;
            }

            btnStart.Enabled = isEnable;
            btnStop.Enabled = !isEnable;
            btnTrigger.Enabled = !isEnable;
            btnJob.Enabled = isEnable;
            btnAccount.Enabled = isEnable;
            btnHistory.Enabled = isEnable;
            btnSettings.Enabled = isEnable;
            btnExportData.Enabled = isEnable;
            btnCustomExport.Enabled = isEnable;
            btnExportResult.Enabled = isEnable;
            btnExportAll.Enabled = isEnable;
            btnExit.Enabled = isEnable;
            // END menu script
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
            btnExportData.Enabled = isEnable;
            btnCustomExport.Enabled = isEnable;
            btnExportResult.Enabled = isEnable;

            btnExit.Enabled = isEnable;

            btnDatabase.Enabled = isEnable;
            pnlPrintedCode.Enabled = isEnable;
            pnlTotalChecked.Enabled = isEnable;
            pnlCheckPassed.Enabled = isEnable;
            pnlCheckFailed.Enabled = isEnable;

            dgvDatabase.Enabled = isEnable;
            dgvCheckedResult.Enabled = isEnable;
            picDatabaseLoading.Visible = !isEnable;
            picCheckedResultLoading.Visible = !isEnable;
        }

        private void ProcessUserAccess()
        {
            if (Shared.LoggedInUser == null) { }
            else if (Shared.LoggedInUser.Role == 0) { }
            else if (Shared.LoggedInUser.Role == 1) { }
        }

        #endregion Update UI 

        #endregion
    }
}