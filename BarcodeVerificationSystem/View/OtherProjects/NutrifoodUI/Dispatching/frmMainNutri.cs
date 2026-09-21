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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using BarcodeVerificationSystem.Model.UserInfo;
using BarcodeVerificationSystem.Services.Dispatching;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;

namespace BarcodeVerificationSystem.View.NutrifoodUI
{
    public partial class FrmMainNutri : Form
    {
        #region VARIABLES DEFINITION
        private readonly frmJobNutri _ParentForm = null;
        private JobModel _SelectedJob = new JobModel();
        private bool _IsPrinterDisconnectedNot = false;
        private bool _IsReCheck = false;
        public DispatchingService DispatchingService = new DispatchingService();


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

        #region sync Data parameters
        private int _SentSyncData = 0;
        private int _SaaSSuccessCodes = 0;
        private int _SaaSFailedCodes = 0;
        private int _SAPSuccessCodes = 0;
        private int _SAPFailedCodes = 0;

        public int SentSyncData { get { return _SentSyncData; } set { _SentSyncData = value; Invoke(new Action(() => { txtCodeResult.Text = string.Format("{0:N0}", _SentSyncData); })); } }
        public int SaaSSuccess { get { return _SaaSSuccessCodes; } set { _SaaSSuccessCodes = value; Invoke(new Action(() => { sentSaaSSuccess.Text = string.Format("{0:N0}", _SaaSSuccessCodes); })); } }
        public int SaaSFailed { get { return _SaaSFailedCodes; } set { _SaaSFailedCodes = value; Invoke(new Action(() => { SaaSFailedCodes.Text = string.Format("{0:N0}", _SaaSFailedCodes); })); } }
        public int SAPSuccess { get { return _SAPSuccessCodes; } set { _SAPSuccessCodes = value; Invoke(new Action(() => { sentSAPSuccess.Text = string.Format("{0:N0}", _SAPSuccessCodes); })); } }
        public int SAPFailed { get { return _SAPFailedCodes; } set { _SAPFailedCodes = value; Invoke(new Action(() => { SAPFailedCodes.Text = string.Format("{0:N0}", _SAPFailedCodes); })); } }
        #endregion

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
        private readonly string[] defaultRecord = new string[] { "100000", "data1","Valid", "Barcode Quality", "False", "100", DateTime.Now.ToString(), "Camera", " " };
        private static readonly string _index = "Index", _resultData = "ResultData", _result = "Result",
                                       _barcodeQuality = "CodeQuality", _position = "Position", _processingTime = "ProcessingTime",
                                       _dateTime = "DateTime", _device = "Device", _sampled = "Sampled";

        private static readonly string[] _ColumnNames = { _index, _resultData, _result, _barcodeQuality, _position, _processingTime, _dateTime, _device, _sampled };
        public readonly int Index_Index = Array.IndexOf(_ColumnNames, _index), Index_ResultData = Array.IndexOf(_ColumnNames, _resultData), Index_Result = Array.IndexOf(_ColumnNames, _result),
                            Index_BarcodeQuality = Array.IndexOf(_ColumnNames, _barcodeQuality), Index_Position = Array.IndexOf(_ColumnNames, _position), Index_ProcessingTime = Array.IndexOf(_ColumnNames, _processingTime),
                            Index_DateTime = Array.IndexOf(_ColumnNames, _dateTime), Index_Device = Array.IndexOf(_ColumnNames, _device), Index_Sampled = Array.IndexOf(_ColumnNames, _sampled);


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

        private List<string[]> _SentPrintedCodeObtainFromFile = null;


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

        private FrmSettingsNuti _FormSettings;
        private FrmViewHistoryProgram _FormViewHistoryProgram;
        private FrmPreviewDatabaseNutri _FormPreviewDatabase;
        private FrmCheckedResult _FormCheckedResult;

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
                    lblTemplatePrint.Visible = txtTemplatePrint.Visible = labelModeCheck.Visible= cuzTextBoxCheckMode.Visible = btnHistory.Visible = btnAccount.Visible = false;
                    pnlVerificationProcess.TitleFont = new Font("Microsoft Sans Serif", 9.0f, FontStyle.Bold);

                    lblFailed.Location = lblPassed.Location = lblTotalChecked.Location = new Point(6, 2);
                    lblPrintedCodeValue.AutoSize = lblReceivedValue.AutoSize = lblSentDataValue.AutoSize = lblCheckResultFailedValue.AutoSize = lblCheckResultPassedValue.AutoSize = lblTotalCheckedValue.AutoSize = false;
                    lblPrintedCodeValue.Width = lblReceivedValue.Width = lblSentDataValue.Width = lblCheckResultFailedValue.Width = lblCheckResultPassedValue.Width = lblTotalCheckedValue.Width = 90;
                    lblCheckResultFailedValue.Font = lblCheckResultPassedValue.Font = lblTotalCheckedValue.Font = new Font("Microsoft Sans Serif", 15.0f, FontStyle.Bold);
                    lblPrintedCodeValue.TextAlign = lblReceivedValue.TextAlign = lblSentDataValue.TextAlign = lblCheckResultFailedValue.TextAlign = lblCheckResultPassedValue.TextAlign= lblTotalCheckedValue.TextAlign = ContentAlignment.MiddleLeft;
                    lblCheckResultFailedValue.Location = lblCheckResultPassedValue.Location = lblTotalCheckedValue.Location = new Point(10, 19);

                    lblPrintedCodeValue.Font = lblReceivedValue.Font = lblSentDataValue.Font =  new Font("Microsoft Sans Serif", 15.0f, FontStyle.Regular);
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
        private readonly Stopwatch _BigSTW = new Stopwatch(), _sendDataSTW = new Stopwatch();
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
        string countMaster = "";
        string countSlave = "";
        private Image _nextImage;
        private bool _isUpdatePending;
        private int _numberPrev = 0;
        static string pathSendLog;
        static string pathRSFPLog;
        private ConcurrentQueue<int> _queueCountFeedback = new ConcurrentQueue<int>();
        private readonly SynchronizedQueue<string[]> _QueueBufferBackupRSFPLog = new SynchronizedQueue<string[]>();
        int countFormStopSuddenly = 0;
        private bool IsCloseButtonAction = true;
        private readonly object lockObject = new object();
        public int CountFeedback { get; set; }
        private int _countFb;
        int CountDataRev;
        #endregion

        public FrmMainNutri()
        {
            InitializeComponent();
        }
        private bool isDragging = false;
        private Point dragStartPoint;
        public FrmMainNutri(frmJobNutri parentForm)
        {
            InitializeComponent();
            FormClosing += FrmMain_FormClosing;
            FormClosed += FrmMain_FormClosed;
            _ParentForm = parentForm;
            WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            Shared.OnNumberEventISCount += Shared_OnNumberEventISCountAsync;
            //StartWebSocketServer("http://localhost:3333/ws/");
            //MessageBox.Show(Shared.LoggedInUser.Role.ToString());

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

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {

            _ParentForm.isShowPopupDisConOneTime = false;
            ReleaseResource();
            _ParentForm?.ShowForm();
        }

        private async void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsCloseButtonAction == true)
            {
                DialogResult dialogResult = CustomMessageBox.Show(Lang.DoYouWantExitApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        ApiService apiService = new ApiService();
                        await MonitorSenderService.sendParametersToServerAsync(apiService, false);
                    }catch (Exception){}
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

        //protected override void WndProc(ref Message m)
        //{
        //    try
        //    {
        //        const int WM_SYSCOMMAND = 0x0112;
        //        const int SC_RESTORE = 0xF120;
        //        const int SC_MOVE = 0xF010;

        //        if (m.Msg == WM_SYSCOMMAND && m.WParam.ToInt32() == SC_RESTORE) // Restore down window
        //        {
        //            // Prevent restore down action
        //            // return;
        //        }
        //        if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt32() & 0xFFF0) == SC_MOVE) // Move by header window
        //        {
        //            // Prevent window dragging
        //            return;
        //        }

        //        base.WndProc(ref m);
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //}

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

            // Show icon printer status

            _LabelStatusPrinterList.Add(lblStatusPrinter01);
            UpdateStatusLabelPrinter();


            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected); // Show icon sensor controller status

            // Show icon serial device status
            UpdateUISerialDeviceControllerStatus(Shared.IsSerialDeviceConnected);


            UpdateJobInfomationInterface(); // Get Job Infor

      
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

            btnExportAll.Visible = btnExportResult.Visible = btnExportData.Visible = Shared.UserPermission.Exports;
            btnAccount.Enabled = Shared.UserPermission.Accounts;
            pnlControllButton.Enabled = Shared.UserPermission.Controls;

            if (Shared.Settings.ExportOneForAllEnable)
            {
                btnExportData.Visible = false;
                btnExportResult.Visible = false;
            }

            if (ProjectLabel.IsNutrifood)
            {
                UIControlsFuncs.HideControls(lblSensorControllerStatus, lblStatusCamera01, btnHistory);
                LineName.Text = "Tên Line: " + Shared.Settings.RLinkName ?? "";
                pnlCurrentCheck.Text = "Thông tin mã in"; // Printing Process
                lblCodeResult.Text = "Number of Sync Code";            
                BarcodeQualityLabel.Text = "Confirm Completion";
                txtBarcodeQuality.Visible = false;
                GetSample.Text = "Disposal Process";
                pnlVerificationProcess.Text = "Vận hành khác";
                UserNameDisplay.Text = "Người dùng: " + CurrentUser.UserName ?? "";


                lblCodeResult.Text = "Mã đã gửi đồng bộ"; //Lang.SentSyncData;
                SentSaaS.Text = "Mã đồng bộ SaaS thành công"; // Lang.SentSyncDataSaaS;
                SentSAP.Text = "Mã đồng bộ HUB thành công"; //Lang.SentSAPData;
                ConfirmLabel.Text = Lang.ConfirmCompletion + " " + Lang.Job;
                SyncDataLabel.Text = Lang.SyncData;
                DisposeLabel.Text = Lang.DisposeBarcodes;
                RePrintLabel.Text = Lang.RePrintBarcodes;

                confirmCompletion.Text = Lang.ConfirmCompletion;
                syncDataBtn.Text = Lang.Sync;
                disposeBtn.Text = Lang.Dispose;
                RePrintBtn.Text = Lang.RePrint;

                if (_SelectedJob.IsRePrintMode)
                {
                    materialNumber.Text = _SelectedJob.ProcessOrder.material_number;
                    materialName.Text = _SelectedJob.ProcessOrder.material_name;
                    wmsNumber.Text = _SelectedJob.ProcessOrder?.process_order;

                }
                else
                {
                    var payloadJob = _SelectedJob?.DispatchingOrderPayload?.payload;
                    wmsNumber.Text = payloadJob?.wms_number;
                    materialNumber.Text = payloadJob?.items[_SelectedJob.SelectedMaterialIndex]?.material_number;
                    materialName.Text = payloadJob?.items[_SelectedJob.SelectedMaterialIndex]?.material_name;
                }
                DispatchingActionsPanel.Visible = Shared.UserPermission.isOnline;
                StopPrintedDataProcess(_printedDataProcess);
            }


            IsFullHD = true;

            //Visible control in Debug mode
#if DEBUG
            DebugVirtual();
#endif
            UpdateCheckTotalAndPrintedDatabase();         
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
            syncDataBtn.Click += ActionChanged;
            RePrintBtn.Click += ActionChanged;
            disposeBtn.Click += ActionChanged;
            confirmCompletion.Click += ActionChanged;

            cuzButtonGetSample.Click += GetSampleRaise;


            mnManage.Click += ActionChanged;
            mnChangePassword.Click += ActionChanged;
            mnLogOut.Click += ActionChanged;

            Shared.OnSyncDataParameterChange += Shared_OnSyncDataParameterChange;

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
        private readonly object _syncLock = new object();

        private void Shared_OnSyncDataParameterChange(object sender, EventArgs e)
        {
            try
            {
                if(_SentPrintedCodeObtainFromFile.Count == 0)
                {
                    string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
                    _SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);
                }
                if (sender is SyncDataParams ParamsName)
                {
                    switch (ParamsName.DataType)
                    {
                        case SyncDataType.SAPSuccess:
                            _SentPrintedCodeObtainFromFile[ParamsName.CodeIndex - 1][PrintingValues.SAPStatus] = "success";

                            SAPSuccess = Shared.NumberOfSentSAP = _SelectedJob.NumberOfSAPSentCodes =
                            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 &&
                                                             item[5].Equals("success", StringComparison.OrdinalIgnoreCase));
                            break;
                        case SyncDataType.SAPFailed:
                            SAPFailed++;
                            break;
                        case SyncDataType.SaaSSuccess:
                            _SentPrintedCodeObtainFromFile[ParamsName.CodeIndex - 1][PrintingValues.SaaSStatus] = "success";
                            SaaSSuccess = Shared.NumberOfSentSaaS = _SelectedJob.NumberOfSaaSSentCodes =
                            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 &&
                                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));
                            break;
                        case SyncDataType.SaaSFailed:
                            SaaSFailed++;
                            break;
                    }

                    //string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
                    //_SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);

                }
            }
            catch (Exception)
            {
                // optional logging
            }
        }

        private void GetSampleRaise(object sender, EventArgs e)
        {
            if (ProjectLabel.IsNutrifood)
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
                dtm.Text = _CheckedResultCodeList.Find(x => x[2] == "Valid")[1];
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

            if (Shared.Settings.ExportImageEnable)
                ExportImageToFileAsync(backupImageToken);

            ExportCheckedResultToFileAsync(backupResultToken);
            UpdateUICheckedResultAsync(uiCheckedResultToken);

            if (_SelectedJob.CompareType == CompareType.Database)
            {
                ExportPrintedResponseToFileAsync(backupResponseToken);
                UpdateUIPrintedResponseAsync(uiPrintedResponseToken);
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
                        detectModel.Text = GetCompareDataByPODFormat(codeModel, _SelectedJob.PODFormat);
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
            }
        }
        #endregion End Testing

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

        private void StartProcess(bool interactOnUI = true)
        {
            if (Shared.OperStatus == OperationStatus.Running || Shared.OperStatus == OperationStatus.Processing)  // Avoid start more 1 time
            {
                return;
            }
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

            CheckCodeOutOfThreshold(backupResponseToken);
            //SendParametersToServerAsync(uiPrintedResponseToken);
            //AutoTriggerCamera(uiPrintedResponseToken);
            //  _ParentForm.ISCamera.StartGetData(); // Start get data from OCR camera
        }

        private async void CheckCodeOutOfThreshold(CancellationToken token)
        {
            await Task.Run(async () => { await CheckPrintedCodeThreshold(token); });

        }

        private async Task CheckPrintedCodeThreshold(CancellationToken token)
        {

            while (true)
            {

                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested(); // Stop thread

                try
                {
                    var payload = Shared.CurrentJob?.DispatchingOrderPayload.payload;
                    var CheckCodeAmount = new RequestCheckCodeAmount()
                    {
                        job_name = Shared.CurrentJob?.FileName,
                        wave_key = payload?.wave_key,
                        wms_number = payload?.wms_number,
                        material_number = payload?.items[Shared.CurrentJob.SelectedMaterialIndex].material_number,
                        username = CurrentUser.UserCode
                    };
                    //var currentPrintedCodeInfo = await apiService.PostApiDataAsync<ResponseCurrentPrintedCodeInfo>(url, CheckCodeAmount); // GetPrintedAmountDataAsync
                    var currentPrintedCodeInfo = await DispatchingService.GetPrintedAmountDataAsync(CheckCodeAmount);

                    if (!currentPrintedCodeInfo.is_success)
                    {
                        ProjectLogger.WriteError($"Error occurred in GetPrintedAmountDataAsync" + currentPrintedCodeInfo.message + " Payload:" + CheckCodeAmount.ToString());
                    }
                    if (currentPrintedCodeInfo.is_exceed)
                    {

                        if (Application.OpenForms.Count > 0)
                        {
                            var mainForm = Application.OpenForms[0];
                            mainForm.Invoke(new Action(() =>
                            {
                                CustomMessageBox.Show(
                                    "Bạn có muốn dùng chương trình ? \n Số lượng mã đã in vượt ngưỡng" +
                                    "\n " + $"Số lượng mã đã in : {currentPrintedCodeInfo.amount}",
                                    "Mã vượt ngưỡng",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information
                                );
                            }));
                        }
                        else
                        {
                            CustomMessageBox.Show("Bạn có muốn dùng chương trình ? \n Số lượng mã đã in vượt ngưỡng" +
                                "\n " + $"Số lượng mã đã in : {currentPrintedCodeInfo.amount}", "Mã vượt ngưỡng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError($"Error occurred in GetPrintedAmountDataAsync" + ex.Message);
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
                            Invoke(new Action(() => StopProcessAsync(false, Lang.CompleteTheBarcodeVerificationProcess, true)));
                            isComplete = true;
                            continue;
                        }
                    }

                    // Process queue data using the position handler
                    if (!positionHandler.TryDequeueData(out DetectModel detectModel, out string positionData))
                    {
                        continue;
                    }

                    if (Shared.Settings.IsItemsPerHour)
                    {
                        currentReceivedTime = GetCurrentTimeInMilliseconds() - lastReceivedTime;
                        double rate = (1000.0 / currentReceivedTime) * 3600;
                        itemsPerHour.Text = rate.ToString("F0") + " " + Lang.ItemsPerHour;
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

                        if (Shared.Settings.OutputEnable && isOutputAllowed && Shared.GetCameraStatus())
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
                            compareStatus.Index = _PrintedCodeObtainFromFile.FindIndex(x => GetCompareDataByPODFormat(x, _SelectedJob.PODFormat) == txt);
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
                        NumberPrinted++;
                        Shared.NumberPrinted = NumberPrinted;
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
                            else if(Shared.Settings.DuplicatedDBEnable)
                            {
                                for(int i = 0; i <_PrintedCodeObtainFromFile.Count; i++)
                                {
                                    var row = _PrintedCodeObtainFromFile[i];
                                    if (row.Length <= 1 || row[1] == "Printed") continue;

                                    var compareRow = row.Where((_, idx) => idx !=1).ToArray();
                                    if(GetCompareDataByPODFormat(compareRow, _SelectedJob.PODFormat) == podCommand)
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
                    strResult[Index_BarcodeQuality] = detectModel.CodeQuality;
                    strResult[Index_Position] = detectModel.isBarcodeWithinThreshold;
                    strResult[Index_ProcessingTime] = detectModel.CompareTime + " ms";
                    strResult[Index_DateTime] = detectModel.ProcessingDateTime;
                    strResult[Index_Device] = detectModel.Device;
                    strResult[Index_Sampled] = detectModel.Sampled;


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

                _SelectedJob.PrintedResponePath = fileName + ".csv";
                _SelectedJob.SaveFile();
            }

            try
            {
                string path = CommVariables.PathPrintedResponse + _SelectedJob.PrintedResponePath;
                string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
                string url = Shared.Settings.IsManufacturingMode ? ManufacturingApis.postPrintedDataUrl()
                                                                 : DispatchingApis.GetPrintedDataUrl();

                if (Shared.PrintMode.IsReprintMode)
                {
                    url = DispatchingApis.GetSendReprintCodesUrl();
                }

                if (!Directory.Exists(CommVariables.PathSentDataPrinted))
                {
                    Directory.CreateDirectory(CommVariables.PathSentDataPrinted);
                }
                try
                {
                    string dataPath = _SelectedJob.DirectoryDatabase;
                    StopPrintedDataProcess(_printedDataProcess);
                    _printedDataProcess = ReliableProcessorFactory.CreatePrintingProcessor(sentDataPath, url, dataPath);
                    StartPrintedDataProcess(_printedDataProcess);
                }
                catch (Exception)
                {
                }

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
                            _SelectedJob.NumberOfPrintedCodes++;
                            SentSyncData++;
                            _SelectedJob.SaveFile();
                            _printedDataProcess.Enqueue(int.Parse(clone[0][0]), clone[0][2], clone[0][3]);
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
                        SyncDataText.Text = "Đang đồng bộ";
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
                        SyncDataText.Text = "Dừng đồng bộ";
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
                string url = ManufacturingApis.postCheckedDataUrl();


                try
                {
                    if (ProjectLabel.IsNutrifood)
                    {
                        _checkedDataProcess = ReliableProcessorFactory.CreateVerificationProcessor(sentDataPath, url, path);
                        _checkedDataProcess.Start();
                    }
                }
                catch (Exception)
                {
                }
     


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
                        // use the Dispatching or the Manufacturing to extract the code, write in the model a function to do that.
                        SaveResultToFile(valueArr, path);
                        try
                        {
                           _checkedDataProcess.Enqueue(int.Parse(clone[0][0]), clone[0]);
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
                            Monitor.Wait(_StopLocker, 3000); //Wait until there is a stop notify from the printer
                            countTimeout++;
                        }
                    }
                }
            }
            _TotalMissed = 0;
            Shared.IsSampled = false;
            _UIPrintedResponseCancelTokenSource?.Cancel();
            _OperationCancelTokenSource?.Cancel();
            while (_QueueBufferDataObtained.TryDequeue(out _)) { }
            while (_QueuePositionDataObtained.TryDequeue(out _)) { }
            _QueueBufferUpdateUIPrinter.Enqueue(null);
            if (ProjectLabel.IsNutrifood)
            {
                try
                {
                    _checkedDataProcess.Stop();
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
        public static List<string[]> ReadPrintedCodeData(string fullFilePath, char delimiter = ',')
        {
            var result = new List<string[]>();

            if (string.IsNullOrWhiteSpace(fullFilePath) || !File.Exists(fullFilePath))
                return result;

            try
            {
                using (var fs = new FileStream(
                    fullFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite)) // 👈 allow other readers and writers
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var values = line.Split(delimiter);
                            result.Add(values);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file at {fullFilePath}: {ex.Message}");
            }

            return result;
        }


        private async void InitDataAsync(JobModel jobModel)
        {
            _BigSTW.Start();
            Stopwatch stw = Stopwatch.StartNew();
            Debug.WriteLine(_BigSTW.ElapsedMilliseconds + " Start init data on thread " + Environment.CurrentManagedThreadId);

            if (jobModel.CompareType == CompareType.Database)
            {
                #region Sent Printed Data
                string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
                _SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);
                _SelectedJob.NumberOfSaaSSentCodes = SaaSSuccess = _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 && item[4].Equals("success", StringComparison.OrdinalIgnoreCase));
                SaaSFailed = _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 && item[4].Equals("failed", StringComparison.OrdinalIgnoreCase));

                _SelectedJob.NumberOfSAPSentCodes =  SAPSuccess = _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 && item[5].Equals("success", StringComparison.OrdinalIgnoreCase));
                SAPFailed = _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 && item[5].Equals("failed", StringComparison.OrdinalIgnoreCase));

                int BothSuccessCount = _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 && item[4].Equals("success", StringComparison.OrdinalIgnoreCase) 
                                        && item[5].Equals("success", StringComparison.OrdinalIgnoreCase));
                #endregion

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
                    if (_PrintedCodeObtainFromFile.Count() > 1)  // Inititalize database information
                    {
                        _DatabaseColunms = _PrintedCodeObtainFromFile[0];
                        _PrintedCodeObtainFromFile.RemoveAt(0);

                        if (_SelectedJob.CompareType == CompareType.Database) // Initialize compare data
                        {
                            await InitCompareDataAsync(_PrintedCodeObtainFromFile, _CheckedResultCodeList); // Waiting until initialize compare data completed
                        }

                        _TotalCode = _PrintedCodeObtainFromFile.Count();
                        Shared.TotalCodes = _SelectedJob.NumberOfNeededSentCodes = _TotalCode;

                        numberOfCode.Text = _TotalCode.ToString();

                        _SelectedJob.NumberOfPrintedCodes = NumberPrinted = _PrintedCodeObtainFromFile.Where(x => x[1] == "Printed").Count();
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
                                    tmp[i] = line[i - 2] + $" - Field{i - 1}";
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
                                        tmp1[i] = line[i - 2];
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
                                    tmp1[i] = line[i - 2];
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
                using (StreamReader reader = new StreamReader(path, Encoding.UTF8, true))
                {
                    int i = -1;
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

                            var dataLine = reader.ReadLine(); // Data: 1,Printed,....
                            var fields = dataLine.Split(',');
                            string index = fields[0];
                            string status = fields[1];
                            if (int.TryParse(index, out int indexNumber))
                                list[indexNumber][1] = "Printed";

                            //var indexString = Csv.Unescape(rexCsvSplitter.Split(dataLine)[0]);
                            //  var lineStatus = Csv.Unescape(rexCsvSplitter.Split(reader.ReadLine())[1]);
                            //if(int.TryParse(indexString, out int indexNumber))
                            //    list[indexNumber][1] = "Printed";
                        }
                    }
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
                        _CheckedResultCodeSet.Add(array[1]);
                    }
                }
                if (datas.Count > 0)
                {
                    int codeLenght = datas[0].Count() - 1;
                    for (int index = 0; index < datas.Count; index++)
                    {

                        string[] row = datas[index].Where((item, idx) => idx != 1).ToArray();
                        string data = GetCompareDataByPODFormat(row, _SelectedJob.PODFormat);

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

                    string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
                    var SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);

                    List<string[]> DataWithSyncList = _PrintedCodeObtainFromFile
                                    .Select((arr, i) =>
                                    {
                                        var newArray = new string[arr.Length + 2];

                                        newArray[0] = arr[0];
                                        newArray[1] = arr[1];

                                        newArray[2] = SentPrintedCodeObtainFromFile[i][PrintingValues.SaaSStatus];
                                        newArray[3] = SentPrintedCodeObtainFromFile[i][PrintingValues.SAPStatus];

                                        Array.Copy(arr, 2, newArray, 4, arr.Length - 2);

                                        return newArray;
                                    })
                                    .ToList();

                    var updatedColumns = _DatabaseColunms
                                            .Take(2)
                                            .Concat(new[] { "Trạng thái SaaS", "Trạng thái HUB" })
                                            .Concat(_DatabaseColunms.Skip(2))
                                            .ToArray();

                    _FormPreviewDatabase = new FrmPreviewDatabaseNutri
                    {
                        _DatabaseColunms = new List<string>(updatedColumns),
                        _ObtainCodeList = DataWithSyncList,
                        _TotalColumns = _TotalColumns + 2,
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
                    _FormCheckedResult = new FrmCheckedResult
                    {
                        _IsAfterProduction = _IsAfterProductionMode,
                        _IsRSeries = _SelectedJob.PrinterSeries,
                        _ColumnNames = _ColumnNames.ToList(),
                        _CheckedResult = _CheckedResultCodeList.ToList(),
                        _CheckedData = _CodeListPODFormat,
                        _CodeData = _PrintedCodeObtainFromFile.ToList(),
                        _TotalColumns = _ColumnNames.Count(),
                        _TotalCode = _TotalCode,
                        _NumberOfPrinted = NumberPrinted,
                        _TotalChecked = TotalChecked,
                        _NumberOfCheckedPassed = NumberOfCheckPassed,
                        _NumberOfCheckedFailed = (TotalChecked - NumberOfCheckPassed),
                        _JobName = _SelectedJob.FileName,
                        _PODFormat = _SelectedJob.PODFormat,
                        //_frmParent = this // needed changes
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
                    _FormSettings = new FrmSettingsNuti();
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
            else if(sender == btnExportAll)
            {
                ExportAllDataAsync();
            }
            else if (sender == btnExportResult)
            {
                string filePathCheckResult = CommVariables.PathCheckedResult + _SelectedJob.CheckedResultPath;
                Shared.ExportCheckedResult(filePathCheckResult);
            }
            else if(sender == syncDataBtn)
            {
                if (Shared.OperStatus != OperationStatus.Running && Shared.OperStatus != OperationStatus.Processing)
                {
                    string path = CommVariables.PathPrintedResponse + _SelectedJob.PrintedResponePath;
                    string sentDataPath = CommVariables.PathSentDataPrinted + _SelectedJob.PrintedResponePath;
                    string url = Shared.Settings.IsManufacturingMode ? ManufacturingApis.postPrintedDataUrl()
                                                                     : DispatchingApis.GetPrintedDataUrl();
                    string dataPath = _SelectedJob.DirectoryDatabase;

                    StopPrintedDataProcess(_printedDataProcess);
                    _printedDataProcess = ReliableProcessorFactory.CreatePrintingProcessor(sentDataPath, url, dataPath);
                    StartPrintedDataProcess(_printedDataProcess);
                }
                else
                {
                    // Lang.SystemIsRunningPleaseStop
                    CustomMessageBox.Show("Hệ thống đang vận hành và đang thực hiện đồng bộ!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            else if(sender == RePrintBtn)
            {
                var RePrintForm = new frmRePrint();
                RePrintForm.ShowDialog();
            }
            else if(sender == disposeBtn)
            {
                var frmDisposal = new frmDisposal();
                frmDisposal.ShowDialog();
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

                    switch (Shared.Settings.CameraList.FirstOrDefault().CameraType)
                    {
                        case CameraType.UKN:
                            break;
                        case CameraType.DM:
                        case CameraType.IS:
                        case CameraType.ISDual:
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
                                            /// Loi o day
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
                    if (codeModel[statusIndex] != "Printed" && (codeModel[statusIndex] != "Duplicate" || Shared.Settings.DuplicatedDBEnable) ) // Check if current code is printed or duplicate 
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
                        Invoke(new Action(() => { _FormCheckedResult.Close(); }));
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
            if(Shared.OperStatus != OperationStatus.Stopped)
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
            var checkedResultDict = _CheckedResultCodeList
                  .Where(arr => arr[Index_Result] == "Valid" && arr[Index_Sampled] != "True")
                  .GroupBy(x => x[Index_ResultData])
                  .ToDictionary(
                                  g => g.Key,
                                  g => string.Join(", ", g.First().Skip(3).Prepend(g.First().ElementAtOrDefault(Index_ResultData)).Where(value => !string.IsNullOrEmpty(value)))

                               );
            ExportPassedData(fileName, checkedResultDict);
        }

        private void ExportPassedDataByScanner(string fileName)
        {
            var checkedResultDict = _CheckedResultCodeList
                 .Where(arr => arr[Index_Result] == "Valid" && arr[Index_Device] != "Camera" && arr[Index_Sampled] != "True")
                 .GroupBy(x => x[Index_ResultData])
                 .ToDictionary(
                                  g => g.Key,
                                  g => string.Join(", ", g.First().Skip(3).Prepend(g.First().ElementAtOrDefault(Index_ResultData)).Where(value => !string.IsNullOrEmpty(value)))

                              );
            ExportPassedData(fileName, checkedResultDict);
        }

        private void ExportSampledData(string fileName)
        {
            var checkedResultDict = _CheckedResultCodeList
                 .Where(arr => arr[Index_Sampled] == "True")
                 .GroupBy(x => x[Index_ResultData])
                 .ToDictionary(
                                  g => g.Key,
                                  g => string.Join(", ", g.First().Skip(3).Prepend(g.First().ElementAtOrDefault(Index_ResultData)).Where(value => !string.IsNullOrEmpty(value)))

                              );
            ExportPassedData(fileName, checkedResultDict);
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
            try
            {
                var checkedResultDict = _CheckedResultCodeList
                  .Where(arr => arr[Index_Result] == "Valid" && arr[Index_Sampled] != "True")
                  .GroupBy(x => x[Index_ResultData])
                  .ToDictionary(
                                   g => g.Key,
                                   g => string.Join(", ", g.First().Skip(3).Prepend(g.First().ElementAtOrDefault(Index_ResultData)).Where(value => !string.IsNullOrEmpty(value)))
                );
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
                string header = string.Join(",", newColumn.Select(Csv.Escape));
                lines.Add(header);

                for (int i = 0; i < _TotalCode; i++)
                {
                    bool isAllowedWriteLine = false;
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


                    }
                    else
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
                                writeValue += PrintedUnverified;
                            }
                        }
                        else
                        {
                            writeValue += UnprintedUnverified;
                        }

                        //writeValue += "," + Csv.Escape(dateVerify).Trim('"');
                        //checkedResultDict.Remove(compareString);

                        isAllowedWriteLine = true;
                    }
                    if (isAllowedWriteLine)
                    {
                        lines.Add(writeValue);// Write the value if the record is checked
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
            catch (Exception)
            {

            }
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
            try
            {

                List<string> linesToWrite = new List<string>();
                // if duplicate, which position would we get, if valid then miss-alignment, what would we get
                // thinh_04_04_2025
                var duplicateCountDict = _CheckedResultCodeList
                    .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] == "Duplicated")
                    .GroupBy(x => x[Index_ResultData])
                    .ToDictionary(g => g.Key, g => g.Count());

                //Dictionary with Result data is key and datetime is value
                var checkedResultDict = _CheckedResultCodeList
                    .Where(arr => arr[Index_Device] != "Barcode Scanner")
                    .GroupBy(x => x[Index_ResultData])
                    .ToDictionary(
                        g => g.Key,
                        g => (DateTime: g.First()[Index_DateTime], Position: g.First()[Index_Position])
                    );

                var test2 = _CheckedResultCodeList
                    .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] != "Valid" && arr[Index_Result] != "Duplicated").GroupBy(x => x[Index_ResultData]).ToDictionary(g => g.Key, g => g.First()[Index_DateTime]);


                var test = _CheckedResultCodeList
                                            .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] != "Valid" && arr[Index_Result] != "Duplicated")
                                            .Select(arr => new KeyValuePair<string, string>(arr[Index_ResultData], arr[Index_DateTime]))
                                            .ToList();


                if (File.Exists(fileName))
                    File.Delete(fileName);

                using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
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
                    string header = string.Join(",", newColumn.Select(Csv.Escape)) + ",Number Checking Code" + ",Position" + ",VerifyDate";
                    //writer.WriteLine(header);
                    linesToWrite.Add(header);

                    for (int i = 0; i < _TotalCode; i++)
                    {
                        var record = _PrintedCodeObtainFromFile[i];

                        // Rearrange the record if it has more than 1 element
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
                        bool isChecked = checkedResultDict.TryGetValue(compareString, out var checkedValues);

                        if (isChecked)
                        {
                            if (status.Equals("Printed")) // Is Printed 
                            {
                                if (duplicateCountDict.TryGetValue(compareString, out int duplicateCount) && duplicateCount >= 1)
                                {
                                    //writeValue += "Duplicated";
                                    writeValue += "Valid";
                                    writeValue += "," + (duplicateCount + 1);
                                }
                                else
                                {
                                    writeValue += "Valid";
                                    writeValue += "," + "1";
                                }
                            }
                            else // Is Not Printed
                            {
                                var index = test.FindIndex(kvp => kvp.Key == compareString);
                                if (index != -1)
                                {
                                    test.RemoveAt(index);
                                }


                                // Dang lam cho In-Sight
                                writeValue += "Miss Alignment";
                                writeValue += "," + "1";
                            }


                            writeValue += "," + Csv.Escape(checkedValues.Position).Trim('"');
                            writeValue += "," + Csv.Escape(checkedValues.DateTime);
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
                                writeValue += "Unverified";

                            }

                        }
                        linesToWrite.Add(writeValue);
                    }
                    //foreach (var item in test)
                    //{
                    //    string valu = "";
                    //    int t = _PrintedCodeObtainFromFile[0].Length - 1;
                    //    for (int i = 0; i < t-1; i++)
                    //    {
                    //        valu += ",";
                    //    }

                    //    string key = item.Key;
                    //    string value = item.Value;
                    //    if(key == "")
                    //    {
                    //        key = "Null";
                    //    }
                    //    valu += key;
                    //    valu += ",Invalid";
                    //    valu += ",1,";
                    //    valu += value;
                    //    linesToWrite.Add(valu);
                    //}
                }

                //linesToWrite = linesToWrite
                //            .Select((line, index) => new { Array = line.Split(','), OriginalIndex = index }) // Split and preserve original index
                //            .OrderBy(x =>
                //            {
                //                DateTime dt;
                //                return DateTime.TryParseExact(x.Array[x.Array.Length - 1], "yyyy/MM/dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dt)
                //                    ? dt.Ticks // Use ticks for DateTime lines
                //                    : long.MaxValue; // Push non-DateTime lines to the end
                //            })
                //            .ThenBy(x => x.OriginalIndex) // Stable sort for non-DateTime lines
                //            .Select(x => string.Join(",", x.Array)) // Join back into a string
                //            .ToList();

              

                if (fileName.EndsWith(".pdf"))
                {
                    ConvertCsvToPdf(linesToWrite.ToArray(), fileName);
                }
                else
                {
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        foreach (var line in linesToWrite)
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
                MessageBox.Show(Lang.DetectError);
                Shared.RaiseOnLogError(ex);
                EnableUIComponent(OperationStatus.Stopped);
            }
        }

        public void ExportData(string fileName)
        {
            try
            {
                var duplicateCountDict = _CheckedResultCodeList
                    .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] == "Duplicated")
                    .GroupBy(x => x[Index_ResultData])
                    .ToDictionary(g => g.Key, g => g.Count());

                var checkedResultDict = _CheckedResultCodeList
                    .Where(arr => arr[Index_Device] != "Barcode Scanner" && arr[Index_Result] == "Valid")
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
            lblPODFormat.Text = Lang.PODFormat;
            lblTemplatePrint.Text = Lang.TemplateName;

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
            lblStaticText.Text = Lang.Totals;
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
                            //CuzAlert.Show(Lang.CameraDisconnected,
                            //    Alert.enmType.Warning,
                            //    new Size(500, 120),
                            //    new Point(Location.X,
                            //    Location.Y),
                            //    Size,
                            //    true);

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