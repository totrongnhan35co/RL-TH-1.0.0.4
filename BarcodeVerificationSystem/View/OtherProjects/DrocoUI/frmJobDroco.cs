using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Controller.Camera;
using BarcodeVerificationSystem.Controller.Camera.Keyence;
using BarcodeVerificationSystem.Controller.HistorySync;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.CaoSuDongNai;
using BarcodeVerificationSystem.Model.PLC;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.CodeGeneration;
using BarcodeVerificationSystem.Utils.CodeGeneration.Helper;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.View.UtilityForms;
using Cognex.DataMan.SDK;
using CommonVariable;
using DesignUI.CuzAlert;
using DesignUI.CuzMesageBox;
using GenCode.Utils;
using OperationLog.Controller;
using OperationLog.Model;
using RestartProcessHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;
using Timer = System.Windows.Forms.Timer;

namespace BarcodeVerificationSystem.View.DrocoUI
{
    public partial class FrmJobDroco : Form
    {
        #region Variables Jobs
        public static readonly DMSeries DMCamera = new DMSeries();

        public ISMultiSyncHandler ISMultiSyncHandler;
        public ISSingleHandler ISSingleHandler;

        internal bool isShowPopupDisConOneTime = false;
        internal bool isShowPopupDupDbOneTime = false;
        internal bool isShowPopupDisPrinterOneTime = false;
        internal bool isShowPopupFalseInitDataOneTime = false;

        private readonly Timer _TimerDateTime = new Timer();
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
        private int _mappedDuplicateCount = 0;
        private FrmSettings _FormSettings;
        public JobModel _JobModel = null;
        private FrmMainDroco _FormMainPC = null;
        // Thêm HashSet riêng để track mã PASS
        private readonly HashSet<string> _mappedPassedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Thread _ThreadMonitorPrinter;
        private Thread _ThreadMonitorZebraPrinter;

        private readonly bool _IsObtainingPrintProductTemplateList = false;
        private string[] _PrintProductTemplateList = new string[] { };
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
        internal event EventHandler AutoAddSufixEvent;
        private Thread _ThreadMonitorSensorController;
        private Thread _ThreadMonitorSerialDeviceController;
        public bool[] _IsSymbol = new bool[5];

        public static bool eventTwoOccurred = false;
        public static object lockObject = new object();
        public static bool isEventTwoHandled = false;
        private readonly string _endOfLineStr = "<EOF>";
        private bool _isTabCheckMode = false;
        private ComboBox _cboJobSelect => cboJobSelect;
        private DataGridView _dgvCheckResult => dgvCheckResult;
        private RichTextBox _rtbDetail => rtbDetail;
        private Label _lblLastScanned => lblLastScanned;

        // === Tab Mapped Validation — UI (arrow properties → designer controls) ===
        private DataGridView _dgvMapped => dgvMapped;
        private RichTextBox _rtbMappedDetail => rtbMappedDetail;
        private Label _lblMappedRefDisplay => lblMappedRefDisplay;
        private Label _lblMappedPassCount => lblMappedPassCount;
        private Label _lblMappedFailCount => lblMappedFailCount;
        private Label _lblMappedTotalCount => lblMappedTotalCount;

        // === Tab Mapped Validation — State ===
        private bool _isMappedTab = false;
        private string _mappedRefCode = "";
        private QRCodeLevel _mappedRefLevel = QRCodeLevel.Unknown;
        private DrocoLookupResult _mappedRefResult = null;
        private DrocoAllValueProcess _mappedProcess = null;
        private int _mappedPassCount = 0;
        private int _mappedFailCount = 0;
        private ComboBox _cboMappedJobSelect => cboMappedJobSelect;
        private Label _lblMappedJobSelectLabel => lblMappedJobSelectLabel;

        // ── Session cache: dùng chung cho Check + Mapped tab ──
        // Load 1 lần khi vào tab, clear khi rời tab
        private readonly Dictionary<string, DrocoAllValueProcess> _sessionProcessCache =
            new Dictionary<string, DrocoAllValueProcess>(StringComparer.OrdinalIgnoreCase);
        private string _activeTabName = "";

        private Panel pnlLoadingContainer;
        private string _loadingText = "";
        private float _spinnerAngle = 0;
        private Timer _spinnerTimer;
        #endregion Variables Jobs

        public FrmJobDroco()
        {
            InitializeComponent();
            imageLoading.Visible = false;
            FirstRowHeader.Checked = true;
            FirstRowHeaderSscc.Checked = false;
            // Modern loading container with GDI+ rotating spinner and rounded corner background
            pnlLoadingContainer = new Panel
            {
                Size = new Size(340, 130),
                Visible = false,
                BackColor = Color.Transparent
            };
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, pnlLoadingContainer, new object[] { true });

            pnlLoadingContainer.Paint += PnlLoadingContainer_Paint;
            Controls.Add(pnlLoadingContainer);

            _spinnerTimer = new Timer
            {
                Interval = 20
            };
            _spinnerTimer.Tick += (s, ev) =>
            {
                _spinnerAngle = (_spinnerAngle + 12) % 360;
                pnlLoadingContainer.Invalidate();
            };
            VisibleChanged += (s, e) => { if (Visible) LoadJobListToMappedComboBox(); };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitControls();
            DMCamera.InitCameraVariables();
            InitEvents();
            SetLanguage();
            InitCheckModeTab();
            InitMappedTab();
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
                bool wasTabCheckMode = _isTabCheckMode;
                bool wasMappedTab = _isMappedTab;

                // Rời tab cũ → clear session cache để lần vào tab sau load dữ liệu mới
                if (_isTabCheckMode || _isMappedTab)
                    ClearSessionCache();

                _isTabCheckMode = (tabControl1.SelectedTab == tabReCheck);
                _isMappedTab = (tabControl1.SelectedTab == tabMapped);

                if (_isTabCheckMode)
                {
                    LoadJobListToComboBox();
                }
                else if (_isMappedTab)
                {
                    LoadJobListToMappedComboBox();
                }
                else
                {
                    Shared.JobNameSelected = "";
                    LoadJobNameList();
                }
            }
            else if (sender == FirstRowHeader)
            {
                if (_JobModel != null)
                {
                    _JobModel.IsFirstRowHeader = FirstRowHeader.Checked;
                }
            }
            else if (sender == FirstRowHeaderSscc)
            {
                if (_JobModel != null)
                {
                    _JobModel.IsFirstRowHeaderSscc = FirstRowHeaderSscc.Checked;
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
                    _FormSettings = new FrmSettings();
                    _FormSettings.FormClosed += (s, args) =>
                        btnDelete.Visible = Shared.Settings.IsAllowJobDeletion;
                    _FormSettings.Show();
                }
                else
                {
                    if (_FormSettings.WindowState == FormWindowState.Minimized)
                        _FormSettings.WindowState = FormWindowState.Normal;

                    _FormSettings.Focus();
                    _FormSettings.BringToFront();
                }

            }
            else if (sender == btnRefesh)
            {
                LoadJobNameList();
            }
            else if (sender == btnImportDatabase)
            {
                SetImportSaveButtons(false);
                ShowLoading("Đang import dữ liệu Honest mark...");

                string selectedPath = OpenDirectoryFileDatabase();
                if (string.IsNullOrEmpty(selectedPath)) { HideLoading(); SetImportSaveButtons(true); return; }

                bool valid = await Task.Run(() => IsValidDatabaseFile(selectedPath));
                if (!valid)
                {
                    CustomMessageBox.Show(this, "File không đúng định dạng hoặc rỗng. Vui lòng chọn lại file!", "Lỗi Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtDirectoryDatabse.Text = ""; _JobModel.DirectoryDatabase = ""; _PODFormat.Clear();
                    HideLoading(); SetImportSaveButtons(true); return;
                }

                txtDirectoryDatabse.Text = _JobModel.DirectoryDatabase = selectedPath;
                if (Shared.databasePath != "") txtDirectoryDatabse.Text = _JobModel.DirectoryDatabase = Shared.databasePath;
                _PODFormat.Clear(); Shared.databasePath = "";
                HideLoading(); SetImportSaveButtons(true);
            }
            else if (sender == btnImportDatabaseSSCC)
            {
                SetImportSaveButtons(false);
                ShowLoading("Đang import dữ liệu SSCC...");

                string path = OpenDirectoryFileDatabase();
                if (string.IsNullOrEmpty(path)) { HideLoading(); SetImportSaveButtons(true); return; }

                bool valid = await Task.Run(() => IsValidDatabaseFile(path));
                if (!valid)
                {
                    CustomMessageBox.Show(this, "File SSCC không đúng định dạng hoặc rỗng. Vui lòng chọn lại!", "Lỗi Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtDirectoryDatabseCodeSSCC.Text = ""; _JobModel.DirectoryDatabaseCodeSSCC = ""; _PODFormat.Clear();
                    HideLoading(); SetImportSaveButtons(true); return;
                }

                txtDirectoryDatabseCodeSSCC.Text = _JobModel.DirectoryDatabaseCodeSSCC = path;
                HideLoading(); SetImportSaveButtons(true);
            }
            else if (sender == btnNext)
            {
                ShowLoading("Đang mở giao diện chính...");
                Shared.RaiseOnNextButtonEvent();
                try
                {
                    if (Shared.JobNameSelected == "")
                    {
                        //JobModel jobModel = Shared.GetJob(txtFileName.Text + Shared.Settings.JobFileExtension);
                        //if (jobModel == null && txtFileName.Text != "")
                        //{
                        //    CuzMessageBox.Show(this, Lang.PleaseSaveTheWorkYouJustEntered, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    return;
                        //}
                        CuzMessageBox.Show(this, Lang.PleaseChooseAJobOrCreateANewOne, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (_JobModel != null && _JobModel.CompareType == CompareType.Database && !CheckExistTemplatePrint(_JobModel.TemplatePrint) && _JobModel.PrinterSeries)
                        {
                            CuzMessageBox.Show(this, Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        if (Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings && _JobModel.CompareType == CompareType.Database && _JobModel.PrinterSeries)
                        {
                            PrinterSettingsModel printerSettingsModel = Shared.GetSettingsPrinter();
                            if (printerSettingsModel.IsSupportHttpRequest)
                            {
                                if (printerSettingsModel.PodDataType != 1)
                                {
                                    CuzMessageBox.Show(this, Lang.DataTypeMustBeRAWData, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                            }
                            else
                            {
                                CuzMessageBox.Show(this, Lang.PrinterNotSupportHttpRequest, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                        }

                        Hide();

                        _FormMainPC?.Dispose();

                        if (_FormMainPC == null || _FormMainPC.IsDisposed)
                        {
                            _FormMainPC = new FrmMainDroco(this);
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

                    }
                }
                catch (Exception) { }
                finally
                {
                    HideLoading();
                }
            }
            else if (sender == btnSave)
            {
                SetImportSaveButtons(false);
                ShowLoading("Đang tạo công việc...");

                if (_JobModel != null)
                {
                    _JobModel.TemplatePrint = GetSelectedPrintProductTemplate();
                    _JobModel.NumberTotalsCode = _NumberTotalsCode;
                    _JobModel.JobStatus = JobStatus.NewlyCreated;
                }

                await SaveJobAsync();
                HideLoading(); SetImportSaveButtons(true);
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
                CustomMessageBox.Show(this, Lang.FunctionIsComingSoon, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        }

        private void SetImportSaveButtons(bool enabled)
        {
            btnImportDatabase.Enabled = enabled;
            btnImportDatabaseSSCC.Enabled = enabled;
            btnSave.Enabled = enabled;
            groupBox1.Enabled = enabled;
            tabControl1.Enabled = enabled;
            UseWaitCursor = !enabled;
        }

        private void ShowLoading(string text)
        {
            _loadingText = text;
            pnlLoadingContainer.Left = (ClientSize.Width - pnlLoadingContainer.Width) / 2;
            pnlLoadingContainer.Top = (ClientSize.Height - pnlLoadingContainer.Height) / 2;
            pnlLoadingContainer.Visible = true;
            pnlLoadingContainer.BringToFront();
            _spinnerTimer.Start();
            pnlLoadingContainer.Refresh();
        }

        private void HideLoading()
        {
            _spinnerTimer.Stop();
            pnlLoadingContainer.Visible = false;
        }

        private void PnlLoadingContainer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw rounded background card (dark premium grey)
            Rectangle rect = new Rectangle(0, 0, pnlLoadingContainer.Width - 1, pnlLoadingContainer.Height - 1);
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
            int spinnerX = (pnlLoadingContainer.Width - size) / 2;
            int spinnerY = 22;

            using (Pen backgroundPen = new Pen(Color.FromArgb(40, 255, 255, 255), 3.5f))
            {
                e.Graphics.DrawEllipse(backgroundPen, spinnerX, spinnerY, size, size);
            }

            using (Pen foregroundPen = new Pen(Color.FromArgb(0, 171, 230), 3.5f))
            {
                foregroundPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                foregroundPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                e.Graphics.DrawArc(foregroundPen, spinnerX, spinnerY, size, size, _spinnerAngle, 100);
            }

            // Draw text centered below the spinner
            string text = _loadingText ?? "Vui lòng chờ...";
            using (Font font = new Font("Segoe UI", 11.5f, FontStyle.Bold))
            {
                using (Brush brush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near
                    };
                    RectangleF textRect = new RectangleF(10, 75, pnlLoadingContainer.Width - 20, 50);
                    e.Graphics.DrawString(text, font, brush, textRect, sf);
                }
            }
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

        private bool IsValidDatabaseFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".csv" || extension == ".txt")
            {
                var lines = File.ReadAllLines(filePath);
                return lines.Length > 0;
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                try
                {
                    var rows = FileFuncs.ReadExcelData(filePath);
                    return rows != null && rows.Count > 0;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
        public void CSVDataBaseClick()
        {
            string selectedPath = OpenDirectoryFileDatabase();
            if (!IsValidDatabaseFile(selectedPath))
            {
                CustomMessageBox.Show("File không đúng định dạng hoặc rỗng. Vui lòng chọn lại file!", "Lỗi Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDirectoryDatabse.Text = "";
                _JobModel.DirectoryDatabase = "";
                _PODFormat.Clear();
                return;
            }
            txtDirectoryDatabse.Text = _JobModel.DirectoryDatabase = selectedPath;
            _PODFormat.Clear();
        }

        private void RestartApplication()
        {
            try
            {
                //CuzMessageBox.Show(this, )
                DialogResult dialogResult = CuzMessageBox.Show(this, Lang.DoYouWantToRestartTheApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
            if (sender is RadioButton radioButton)
            {
                if (radioButton.Enabled)
                {
                    radioButton.BackColor = radioButton.Checked ? Color.FromArgb(0, 170, 230) : Color.White;
                }
            }
        }
        private void CboSupportForCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cbbSupportCam = (ComboBox)sender;
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
            if (e.Index == -1 || (sender as ListBox).Items.Count == 0) return;
            try
            {
                JobModel job = Shared.GetJob((sender as ListBox).Items[e.Index].ToString());
                Rectangle headItemRect = new Rectangle(0, e.Bounds.Y + 4, 8, e.Bounds.Height - 10);
                using (Brush brush = new SolidBrush(_Standalone))
                    if (!job.PrinterSeries)
                        e.Graphics.FillRectangle(brush, headItemRect);
            }
            catch
            {
            }
        }
        private void JobType_EnabledChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton)
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
            EnableUIPrinting();
            _LabelStatusCameraList.Add(lblStatusCamera01);
            UpdateStatusLabelCamera();
            _LabelStatusPrinterList.Add(lblStatusPrinter01);
            UpdateStatusLabelPrinter();
            UpdateStatusLabelZebraPrinter();
            EnableUIPrinting();
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
            UpdateUIListBoxPrintProductTemplateList(_PrintProductTemplateList);
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
        private void Shared_OnZebraPrinterStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelZebraPrinter();
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
                            UpdateUIListBoxPrintProductTemplateList(_PrintProductTemplateList);
                        }
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
                HideLoading();
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

        private static PlcEventHandler _eventHandler = new PlcEventHandler();
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
                        if (_eventHandler == null)
                            _eventHandler = new PlcEventHandler();
                        _eventHandler.EnqueueEvent(currentIndex, Shared.Settings.DelayOutputTime);
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
            //FirstRowHeader.Text = Lang.FirstRowHeader;
    
            lblStaticText1.Text = "Tổng mã GS1";
            lblPODFormat.Text = "Mã → Hộp";
            lblTemplatePrint.Text = "Hộp → Thùng";
            btnNext.Text = Lang.Next;

            lblJobType.Text = "Thùng → Pallet";
            lblJobStatus.Text = Lang.JobStatus;
            btnSave.Text = Lang.Save;
            lblImportDatabase.Text = Lang.ImportDatabase;
            lblStatusCamera01.Text = Lang.CameraTMP;
            lblStatusPrinter01.Text = Lang.Printer;
            lblStatusSerialDevice.Text = Lang.ScannerLabel;
            lblSensorControllerStatus.Text = Lang.PLCLabel;

            txtJobType.Text = "";
            txtJobStatus.Text = _JobModel.JobStatus.ToFriendlyString();
            lblCompareTypeInfo.Text = Lang.Database;

            lblToolStripVersion.Text = Lang.Version + ": " + Properties.Settings.Default.SoftwareVersion;
            btnDelete.Text = Lang.Delete;
            btnHelp.Text = Lang.Help;
            btnRestart.Text = Lang.Restart;

            tabPage1.Text = Lang.SelectJob;
            tabPage2.Text = Lang.CreateANewJob;
            tabReCheck.Text = Lang.CheckModeDroco;
            tabMapped.Text = Lang.tabMapped;
        }


        private void DebugVirtual()
        {
            // Debug method for testing
        }
        // ========== THÊM METHOD MỚI ==========

        #region Tab Check Mode



        //nhanne
        //private void InitCheckModeTab()
        //{
        //    if (InvokeRequired) { Invoke(new Action(InitCheckModeTab)); return; }

        //    dgvCheckResult.EnableHeadersVisualStyles = false;
        //    DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
        //    headerStyle.BackColor = Color.FromArgb(0, 171, 230);
        //    headerStyle.ForeColor = Color.White;
        //    headerStyle.Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold);
        //    headerStyle.SelectionBackColor = Color.FromArgb(0, 171, 230); 
        //    headerStyle.SelectionForeColor = Color.White;                
        //    dgvCheckResult.ColumnHeadersDefaultCellStyle = headerStyle;
        //    dgvCheckResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        //    dgvCheckResult.ColumnHeadersHeight = 35;

        //    rtbDetail.Text = "(Quét mã để xem chi tiết)";
        //    lblLastScanned.Text = "Mã vừa quét:  —";

        //    // Wire events
        //    dgvCheckResult.SelectionChanged += DgvCheckResult_SelectionChanged;
        //    cboJobSelect.SelectedIndexChanged += (s, e) =>
        //    {
        //        Shared.DrocoAllValueProcess = null;
        //        TryInitDrocoAllValueProcess();
        //    };
        //}
        private void InitCheckModeTab()
        {
            if (InvokeRequired) { Invoke(new Action(InitCheckModeTab)); return; }

            dgvCheckResult.EnableHeadersVisualStyles = false;
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.BackColor = Color.FromArgb(0, 171, 230);
            headerStyle.ForeColor = Color.White;
            headerStyle.Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold);
            headerStyle.SelectionBackColor = Color.FromArgb(0, 171, 230);
            headerStyle.SelectionForeColor = Color.White;
            dgvCheckResult.ColumnHeadersDefaultCellStyle = headerStyle;
            dgvCheckResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvCheckResult.ColumnHeadersHeight = 35;

            rtbDetail.Text = "(Quét mã để xem chi tiết)";
            lblLastScanned.Text = "Mã vừa quét:  —";

            // FIX Bug 2: XÓA subscription ở đây — InitEvents() đã đăng ký rồi
            // dgvCheckResult.SelectionChanged += DgvCheckResult_SelectionChanged;  // ← ĐÃ XÓA
            // FIX Bug 3: XÓA lambda ở đây — comboBox1_SelectedIndexChanged trong InitEvents() đã xử lý
            // cboJobSelect.SelectedIndexChanged += ...                              // ← ĐÃ XÓA
        }
        //nhanne
        //private void BtnClearGrid_Click(object sender, EventArgs e)
        //{
        //    _cboJobSelect.SelectedIndex = 0;
        //    if (_dgvCheckResult.Rows.Count == 0) return;
        //    _dgvCheckResult.Rows.Clear();
        //    _lblLastScanned.Text = $"Mã vừa quét:";
        
      
        //    if (_rtbDetail != null)
        //    {
        //        _rtbDetail.ForeColor = Color.Gray;
        //        _rtbDetail.Text = "(Quét mã để xem chi tiết)";
        //    }
        //}

        private void BtnClearGrid_Click(object sender, EventArgs e)
        {
            // FIX Bug 1: KHÔNG set SelectedIndex = 0 vì kích hoạt SelectedIndexChanged
            // Chỉ reset UI, không reset combo
            if (_dgvCheckResult.Rows.Count == 0) return;
            _dgvCheckResult.Rows.Clear();
            _lblLastScanned.Text = $"Mã vừa quét:";

            if (_rtbDetail != null)
            {
                _rtbDetail.ForeColor = Color.Gray;
                _rtbDetail.Text = "(Quét mã để xem chi tiết)";
            }
        }


        private void DgvCheckResult_SelectionChanged(object sender, EventArgs e)
        {
            if (_dgvCheckResult.SelectedRows.Count == 0) return;
            var row = _dgvCheckResult.SelectedRows[0];

            string time = row.Cells[0].Value?.ToString() ?? "";
            string code = row.Cells[1].Value?.ToString() ?? "";
            string type = row.Cells[2].Value?.ToString() ?? "";
            string col3 = row.Cells[3].Value?.ToString() ?? "";  // BoxQR / "N hộp" / ""
            string col4 = row.Cells[4].Value?.ToString() ?? "";  // CartonQR / "N thùng"
            string col5 = row.Cells[5].Value?.ToString() ?? "";  // PalletQR / CartonCode / PalletCode
            string status = row.Cells[6].Value?.ToString() ?? "";
            string job = row.Cells[7].Value?.ToString() ?? "";

            if (_rtbDetail == null) return;

            if (type == "Không tìm thấy")
            {
                _rtbDetail.ForeColor = Color.Red;
                _rtbDetail.Text =
                    $"❌  Không tìm thấy\r\n" +
                    $"{new string('─', 40)}\r\n" +
                    $"Mã quét  : {code}\r\n" +
                    $"Job      : {job}\r\n" +
                    $"Thời gian: {time}\r\n\r\n" +
                    $"Mã này không tồn tại trong cơ sở dữ liệu.";
                return;
            }

            _rtbDetail.ForeColor = Color.Black;
            var sb = new StringBuilder();
            sb.AppendLine($"Thời gian : {time}");
            sb.AppendLine($"Mã quét   : {code}");
            sb.AppendLine($"Loại mã   : {type}");
            sb.AppendLine(new string('─', 40));

            switch (type)
            {
                case "Mã GS1":
                    // col3=BoxQR, col4=CartonQR, col5=PalletQR
                    if (!string.IsNullOrEmpty(col3)) sb.AppendLine($"Mã Hộp    : {col3}");
                    if (!string.IsNullOrEmpty(col4)) sb.AppendLine($"Mã Thùng  : {col4}");
                    if (!string.IsNullOrEmpty(col5)) sb.AppendLine($"Mã Pallet : {col5}");
                    break;

                case "Mã Hộp":
                    // col3=BoxCode (trùng với code), col4=CartonQR, col5=PalletQR
                    if (!string.IsNullOrEmpty(col4)) sb.AppendLine($"Mã Thùng  : {col4}");
                    if (!string.IsNullOrEmpty(col5)) sb.AppendLine($"Mã Pallet : {col5}");
                    break;

                case "Mã Thùng":
                    // col3="N hộp" (số lượng), col4=CartonCode (trùng code), col5=PalletQR
                    if (!string.IsNullOrEmpty(col3)) sb.AppendLine($"Số Hộp    : {col3}");
                    if (!string.IsNullOrEmpty(col5)) sb.AppendLine($"Mã Pallet : {col5}");
                    sb.AppendLine();
                    sb.AppendLine("(Quét lại mã thùng để xem danh sách hộp chi tiết)");
                    break;

                case "Mã Pallet":
                    // col3="", col4="N thùng" (số lượng), col5=PalletCode (trùng code)
                    if (!string.IsNullOrEmpty(col4)) sb.AppendLine($"Số Thùng  : {col4}");
                    sb.AppendLine();
                    sb.AppendLine("(Quét lại mã pallet để xem danh sách thùng chi tiết)");
                    break;

                default:
                    if (!string.IsNullOrEmpty(col3)) sb.AppendLine($"Cột 3     : {col3}");
                    if (!string.IsNullOrEmpty(col4)) sb.AppendLine($"Cột 4     : {col4}");
                    if (!string.IsNullOrEmpty(col5)) sb.AppendLine($"Cột 5     : {col5}");
                    break;
            }

            if (!string.IsNullOrEmpty(status)) sb.AppendLine($"Trạng thái: {status}");
            if (!string.IsNullOrEmpty(job)) sb.AppendLine($"Job       : {job}");

            _rtbDetail.Text = sb.ToString();
        }
        //nhanne
        //private void LoadJobListToComboBox()
        //{
        //    if (_cboJobSelect == null) return;
        //    if (InvokeRequired) { Invoke(new Action(LoadJobListToComboBox)); return; }

        //    _cboJobSelect.Items.Clear();
        //    _cboJobSelect.Items.Add("--- Tất cả Jobs ---");

        //    var jobNames = Shared.GetJobNameList();
        //    if (jobNames != null)
        //    {
        //        foreach (var name in jobNames)
        //        {
        //            var job = Shared.GetJob(name);
        //            if (job != null && job.JobStatus != JobStatus.Deleted)
        //                _cboJobSelect.Items.Add(name);
        //        }
        //    }

        //    int idx = string.IsNullOrEmpty(Shared.JobNameSelected)
        //        ? 0
        //        : _cboJobSelect.Items.IndexOf(Shared.JobNameSelected);
        //    _cboJobSelect.SelectedIndex = idx < 0 ? 0 : idx;
        //}

        private bool ExecuteSearchLogic(string scannedCode, bool isAllJobs, string selectedJobName = "")
        {
            if (isAllJobs)
            {
                var jobNames = Shared.GetJobNameList();
                if (jobNames == null) return false;

                // ── FIX: dùng anyFound thay vì return true để tìm QUA TẤT CẢ job ──
                bool anyFound = false;
                foreach (var name in jobNames)
                {
                    try
                    {
                        var job = Shared.GetJob(name);
                        if (job == null || job.JobStatus == JobStatus.Deleted) continue;
                        if (!AllValuesFileExists(job)) continue;

                        // Dùng cached process — chỉ tạo mới nếu chưa có
                        var process = GetSessionProcess(job);
                        if (TryAddLookupRow(scannedCode, process, job.FileName))
                            anyFound = true; // ← KHÔNG return ngay, tiếp tục tìm job tiếp theo
                    }
                    catch (Exception ex)
                    {
                        ProjectLogger.WriteError($"ExecuteSearchLogic job '{name}': {ex.Message}");
                    }
                }
                return anyFound;
            }
            else
            {
                if (string.IsNullOrEmpty(selectedJobName)) return false;
                var jobToUse = Shared.GetJob(selectedJobName);
                if (jobToUse == null) return false;
                if (!AllValuesFileExists(jobToUse)) return false;

                var process = GetSessionProcess(jobToUse);
                return TryAddLookupRow(scannedCode, process, selectedJobName);
            }
        }

        /// <summary>
        /// Lấy process từ session cache — tạo mới + build cache nếu chưa có.
        /// Chỉ rebuild cache lần đầu khi vào tab, không rebuild lại mỗi lần quét.
        /// </summary>
        private DrocoAllValueProcess GetSessionProcess(JobModel job)
        {
            if (job == null || string.IsNullOrEmpty(job.FileName)) return null;
            string key = job.FileName + "_" + (job.BatchNumber ?? "");
            if (_sessionProcessCache.TryGetValue(key, out var process))
                return process;
            process = new DrocoAllValueProcess(job);
            process.RebuildCache();
            _sessionProcessCache[key] = process;
            return process;
        }

        /// <summary>
        /// Xoá toàn bộ session cache — gọi khi rời tab để lần vào tab sau load dữ liệu mới.
        /// </summary>
        private void ClearSessionCache()
        {
            _sessionProcessCache.Clear();
            _mappedProcess = null;
        }

        private void LoadJobListToComboBox()
        {
            if (_cboJobSelect == null) return;
            if (InvokeRequired) { Invoke(new Action(LoadJobListToComboBox)); return; }

            _cboJobSelect.Items.Clear();
            _cboJobSelect.Items.Add("--- Tất cả Jobs ---");

            var jobNames = Shared.GetJobNameList();
            int addedCount = 0;
            if (jobNames != null)
            {
                foreach (var name in jobNames)
                {
                    var job = Shared.GetJob(name);
                    if (job != null && job.JobStatus != JobStatus.Deleted)
                    {
                        _cboJobSelect.Items.Add(name);
                        addedCount++;
                    }
                }
            }

            // Client mode: cảnh báo nếu server chưa có dữ liệu AllValues
            if (Shared.IsClientMode && _lblLastScanned != null)
            {
                bool hasData = jobNames != null && jobNames.Any(n =>
                {
                    var j = Shared.GetJob(n);
                    return j != null && j.JobStatus != JobStatus.Deleted && AllValuesFileExists(j);
                });

                _lblLastScanned.Text = hasData
                    ? "Chọn Job và quét mã để tra cứu"
                    : "⚠️  Server chưa có dữ liệu — hãy chờ Server khởi chạy Job";
                _lblLastScanned.ForeColor = hasData ? Color.Black : Color.OrangeRed;
            }

            int idx = string.IsNullOrEmpty(Shared.JobNameSelected)
                ? (_cboJobSelect.Items.Count > 1 ? 1 : 0)
                : _cboJobSelect.Items.IndexOf(Shared.JobNameSelected);
            _cboJobSelect.SelectedIndex = idx < 0 ? 0 : idx;
        }

        private void TryInitDrocoAllValueProcess()
        {
            try
            {
                bool isAllJobs = _cboJobSelect == null || _cboJobSelect.SelectedIndex <= 0;
                if (isAllJobs) { Shared.DrocoAllValueProcess = null; return; }
                if (Shared.DrocoAllValueProcess != null) return;

                string selectedJobName = _cboJobSelect.SelectedItem?.ToString();
                var jobToUse = Shared.GetJob(selectedJobName);
                if (jobToUse == null || string.IsNullOrEmpty(jobToUse.FileName)) return;

                // readOnly=true → không tạo file mới nếu chưa tồn tại
                Shared.DrocoAllValueProcess = new DrocoAllValueProcess(jobToUse);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("TryInitDrocoAllValueProcess error: " + ex.Message);
            }
        }



        // frmJobDroco.cs
        private async void ShowLookupResult(string scannedCode)
        {
            if (InvokeRequired) { Invoke(new Action(() => ShowLookupResult(scannedCode))); return; }
            if (string.IsNullOrWhiteSpace(scannedCode)) return;

            bool isAllJobs = _cboJobSelect == null || _cboJobSelect.SelectedIndex <= 0;
            string selectedJobName = _cboJobSelect?.SelectedItem?.ToString() ?? "";
            string jobDisplayName = isAllJobs ? "Tất cả Jobs" : selectedJobName;

            // ── Show loading NGAY, không cần Task.Delay(100) ──
            if (_lblLastScanned != null)
                _lblLastScanned.Text = $"Đang tìm: {scannedCode}";
            picLoading.Visible = true;

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                try
                {
                    bool found = await Task.Run(() =>
                        ExecuteSearchLogic(scannedCode, isAllJobs, selectedJobName),
                    cts.Token);

                    if (!found)
                    {
                        if (Shared.IsClientMode)
                            AddNotFoundRow(scannedCode, "⚠️ Server chưa có dữ liệu cho mã này");
                        else
                            AddNotFoundRow(scannedCode, jobDisplayName);
                    }
                }
                catch (System.OperationCanceledException)
                {
                    ProjectLogger.WriteError($"Tìm kiếm mã {scannedCode} bị Timeout (10s)");
                    AddNotFoundRow(scannedCode, "Hệ thống bận (Timeout)");
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("Error: " + ex.Message);
                }
                finally
                {
                    picLoading.Visible = false; // ← luôn ẩn khi xong
                    if (_lblLastScanned != null)
                        _lblLastScanned.Text = $"Mã vừa quét: {scannedCode}";
                }
            }
        }

        /// <summary>
        /// Kiểm tra AllValues file đã được tạo cho job chưa (job đã chạy hay chưa).
        /// Tránh side-effect khi new DrocoAllValueProcess() tự tạo file.
        /// </summary>
        private static bool AllValuesFileExists(JobModel job)
        {
            if (job == null || string.IsNullOrEmpty(job.FileName)) return false;
            string fileName = job.FileName + "_" + (job.BatchNumber ?? "") + "_AllValues.csv";
            return File.Exists(CommVariables.PathAllValues + fileName);
        }

        private void TrySetMappedReference(string scannedCode, string timeStr)
        {
            // Bắt buộc phải chọn Job trước
            if (_cboMappedJobSelect == null || _cboMappedJobSelect.SelectedIndex <= 0)
            {
                AddMappedRow(timeStr, scannedCode, "⚠️ Chưa chọn Job",
                    "Vui lòng chọn Job từ danh sách phía trên trước khi quét.",
                    Color.FromArgb(255, 248, 220), Color.DarkOrange);
                return;
            }

            string selectedJobName = _cboMappedJobSelect.SelectedItem?.ToString();
            var job = Shared.GetJob(selectedJobName);

            if (job == null || !AllValuesFileExists(job))
            {
                AddMappedRow(timeStr, scannedCode, "⚠️ Job chưa sẵn sàng",
                    $"Job '{selectedJobName}' chưa có dữ liệu AllValues.\nHãy chạy Job trước rồi thử lại.",
                    Color.FromArgb(255, 248, 220), Color.DarkOrange);
                return;
            }

            // Dùng cached process thay vì tạo mới
            var proc = GetSessionProcess(job);

            var boxRes = proc.LookupBoxCode(scannedCode);
            if (boxRes.IsFound) { SetMappedRef(scannedCode, QRCodeLevel.Box, boxRes, proc, selectedJobName, timeStr); return; }

            var cartonRes = proc.LookupCartonCode(scannedCode);
            if (cartonRes.IsFound) { SetMappedRef(scannedCode, QRCodeLevel.Carton, cartonRes, proc, selectedJobName, timeStr); return; }

            var palletRes = proc.LookupPalletCode(scannedCode);
            if (palletRes.IsFound) { SetMappedRef(scannedCode, QRCodeLevel.Pallet, palletRes, proc, selectedJobName, timeStr); return; }

            // Không tìm thấy trong job được chọn
            AddMappedRow(timeStr, scannedCode, "⚠️ Không xác định",
                $"Mã này không phải Hộp / Thùng / Pallet trong Job: {selectedJobName}.",
                Color.FromArgb(255, 248, 220), Color.DarkOrange);
        }
        private bool ExecuteSearchLogic(string scannedCode)
        {
            bool isAllJobs = _cboJobSelect == null || _cboJobSelect.SelectedIndex <= 0;

            if (isAllJobs)
            {
                var jobNames = Shared.GetJobNameList();
                if (jobNames == null) return false;

                foreach (var name in jobNames)
                {
                    var job = Shared.GetJob(name);
                    if (job == null || job.JobStatus == JobStatus.Deleted) continue;
                    if (!AllValuesFileExists(job)) continue;

                    var process = GetSessionProcess(job);
                    if (TryAddLookupRow(scannedCode, process, job.FileName)) return true;
                }
            }
            else
            {
                string selectedJobName = _cboJobSelect.SelectedItem?.ToString();
                var jobToUse = Shared.GetJob(selectedJobName);
                if (jobToUse == null) return false;
                if (!AllValuesFileExists(jobToUse)) return false;

                var process = GetSessionProcess(jobToUse);
                return TryAddLookupRow(scannedCode, process, selectedJobName);
            }

            return false;
        }

        //private bool ExecuteSearchLogic(string scannedCode)
        //{
        //    bool isAllJobs = _cboJobSelect == null || _cboJobSelect.SelectedIndex <= 0;

        //    if (isAllJobs)
        //    {
        //        var jobNames = Shared.GetJobNameList();
        //        if (jobNames == null) return false;

        //        foreach (var name in jobNames)
        //        {
        //            var job = Shared.GetJob(name);
        //            if (job == null || job.JobStatus == JobStatus.Deleted) continue;
        //            if (!AllValuesFileExists(job)) continue;   // ← FIX

        //            var process = new DrocoAllValueProcess(job);
        //            if (TryAddLookupRow(scannedCode, process, job.FileName)) return true;
        //        }
        //    }
        //    else
        //    {
        //        if (Shared.DrocoAllValueProcess == null) TryInitDrocoAllValueProcess();
        //        return TryAddLookupRow(scannedCode, Shared.DrocoAllValueProcess, _cboJobSelect.SelectedItem.ToString());
        //    }
        //    return false;
        //}
        /// <summary>
        /// Tra cứu và thêm 1 dòng vào DataGridView. Trả về true nếu tìm thấy.
        /// </summary>
        /// <summary>
        /// Tra cứu và thêm dòng vào DataGridView. Trả về true nếu tìm thấy.
        /// GS1   → 1 dòng: Hộp + Thùng + Pallet
        /// Hộp   → 1 dòng: Thùng + Pallet
        /// Thùng → N dòng: mỗi Hộp thuộc Thùng + Pallet chứa Thùng
        /// Pallet→ N dòng: mỗi Thùng trên Pallet
        /// </summary>
        private bool TryAddLookupRow(string scannedCode, DrocoAllValueProcess process, string jobName)
        {
            string timeStr = DateTime.Now.ToString("HH:mm:ss");

            // === GS1: hiển thị Hộp, Thùng, Pallet ===
            var gsResult = process.LookupProductCode(scannedCode);
            if (gsResult.IsFound)
            {
                AddRowToGrid(timeStr, scannedCode, "Mã GS1",
                    gsResult.BoxQR, gsResult.CartonQR, gsResult.PalletQR,
                    gsResult.Status, jobName, Color.FromArgb(236, 253, 255));
                UpdateDetailPanel(scannedCode, gsResult);
                return true;
            }

            // === Box: hiển thị Thùng, Pallet ===
            var boxResult = process.LookupBoxCode(scannedCode);
            if (boxResult.IsFound)
            {
                string statusBox = boxResult.ProductCodes?.Count > 0 ? $"{boxResult.ProductCodes.Count} SP" : "";
                AddRowToGrid(timeStr, scannedCode, "Mã Hộp",
                    scannedCode, boxResult.CartonQR, boxResult.PalletQR,
                    statusBox, jobName, Color.FromArgb(236, 253, 255));
                UpdateDetailPanel(scannedCode, boxResult);
                return true;
            }

            // === Carton: 1 hàng summary trong grid, chi tiết hộp hiển thị ở rtbDetail ===
            var cartonResult = process.LookupCartonCode(scannedCode);
            if (cartonResult.IsFound)
            {
                int boxCount = cartonResult.BoxCodes?.Count ?? 0;
                string statusCarton = !string.IsNullOrEmpty(cartonResult.Status)
                    ? cartonResult.Status
                    : (!string.IsNullOrEmpty(cartonResult.PalletQR) ? "Đã ghép pallet" : "Chưa ghép pallet");

                AddRowToGrid(timeStr, scannedCode, "Mã Thùng",
                    $"{boxCount} hộp", scannedCode, cartonResult.PalletQR,
                    statusCarton, jobName, Color.FromArgb(236, 253, 255));

                UpdateDetailPanel(scannedCode, cartonResult); // ← hiển thị danh sách hộp ở rtbDetail
                return true;
            }

            // === Pallet: 1 hàng tóm tắt, detail hiển thị danh sách thùng ===
            var palletResult = process.LookupPalletCode(scannedCode);
            if (palletResult.IsFound)
            {
                int cartonCount = palletResult.CartonCodes?.Count ?? 0;
                string statusPallet = !string.IsNullOrEmpty(palletResult.Status)
                    ? palletResult.Status
                    : (cartonCount > 0 ? $"{cartonCount} thùng" : "Chưa có thùng");
                AddRowToGrid(timeStr, scannedCode, "Mã Pallet",
                    "", $"{cartonCount} thùng", scannedCode,
                    statusPallet, jobName, Color.FromArgb(236, 253, 255));
                UpdateDetailPanel(scannedCode, palletResult);
                return true;
            }

            return false;
        }
        private void UpdateDetailPanel(string scannedCode, DrocoLookupResult result)
        {
            if (_rtbDetail == null) return;
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdateDetailPanel(scannedCode, result))); return; }

            _rtbDetail.ForeColor = Color.Black; // reset màu (tránh còn đỏ từ not-found trước đó)

            var sb = new StringBuilder();
            sb.AppendLine($"Thời gian : {DateTime.Now:HH:mm:ss}");
            sb.AppendLine($"Mã quét   : {scannedCode}");
            sb.AppendLine($"Loại mã   : {result.LevelLabel}");
            sb.AppendLine(new string('─', 60));

            switch (result.Level)
            {
                case QRCodeLevel.GS1:
                    sb.AppendLine($"Mã Hộp    : {(string.IsNullOrEmpty(result.BoxQR) ? "(chưa ghép hộp)" : result.BoxQR)}");
                    sb.AppendLine($"Mã Thùng  : {(string.IsNullOrEmpty(result.CartonQR) ? "(chưa ghép thùng)" : result.CartonQR)}");
                    sb.AppendLine($"Mã Pallet : {(string.IsNullOrEmpty(result.PalletQR) ? "(chưa ghép pallet)" : result.PalletQR)}");
                    sb.AppendLine($"Trạng thái: {result.Status}");
                    break;

                case QRCodeLevel.Box:
                    sb.AppendLine($"Mã Thùng  : {(string.IsNullOrEmpty(result.CartonQR) ? "(chưa ghép thùng)" : result.CartonQR)}");
                    sb.AppendLine($"Mã Pallet : {(string.IsNullOrEmpty(result.PalletQR) ? "(chưa ghép pallet)" : result.PalletQR)}");
                    if (result.ProductCodes?.Count > 0)
                        sb.AppendLine($"Số SP     : {result.ProductCodes.Count}");
                    break;

                case QRCodeLevel.Carton:
                    sb.AppendLine($"Mã Pallet : {(string.IsNullOrEmpty(result.PalletQR) ? "(chưa ghép pallet)" : result.PalletQR)}");
                    sb.AppendLine();
                    var boxes = result.BoxCodes ?? new List<string>();
                    sb.AppendLine($"Danh sách Hộp trong Thùng ({boxes.Count} hộp):");
                    for (int i = 0; i < boxes.Count; i++)
                        sb.AppendLine($"  {(i + 1).ToString().PadLeft(3)}. {boxes[i]}");
                    if (boxes.Count == 0)
                        sb.AppendLine("  (Chưa có hộp nào được ghép vào thùng này)");
                    break;

                case QRCodeLevel.Pallet:
                    var cartons = result.CartonCodes ?? new List<string>();
                    sb.AppendLine($"Danh sách Thùng trên Pallet ({cartons.Count} thùng):");
                    for (int i = 0; i < cartons.Count; i++)
                        sb.AppendLine($"  {(i + 1).ToString().PadLeft(3)}. {cartons[i]}");
                    if (cartons.Count == 0)
                        sb.AppendLine("  (Chưa có thùng nào trên pallet này)");
                    break;
            }

            _rtbDetail.Text = sb.ToString();
        }
        private void AddRowToGrid(string time, string code, string type,
    string box, string carton, string pallet, string status, string job, Color rowColor)
        {
            if (_dgvCheckResult == null) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddRowToGrid(time, code, type, box, carton, pallet, status, job, rowColor)));
                return;
            }

            try
            {
                _dgvCheckResult.SuspendLayout();
                _dgvCheckResult.SelectionChanged -= DgvCheckResult_SelectionChanged;

                var row = new DataGridViewRow();
                row.CreateCells(_dgvCheckResult, time, code, type, box, carton, pallet, status, job);
                row.DefaultCellStyle.BackColor = rowColor;
                _dgvCheckResult.Rows.Insert(0, row);
                _dgvCheckResult.FirstDisplayedScrollingRowIndex = 0;

                _dgvCheckResult.ClearSelection();
                _dgvCheckResult.Rows[0].Selected = true;
            }
            finally
            {
                _dgvCheckResult.SelectionChanged += DgvCheckResult_SelectionChanged;
                _dgvCheckResult.ResumeLayout();
            }

            ProjectLogger.WriteInfo($"[TabCheck] {code} → {type} | Job: {job}");
        }

        private void AddNotFoundRow(string code, string jobName)
        {
            if (_dgvCheckResult == null) return;
            if (InvokeRequired) { BeginInvoke(new Action(() => AddNotFoundRow(code, jobName))); return; }

            try
            {
                _dgvCheckResult.SuspendLayout();
                _dgvCheckResult.SelectionChanged -= DgvCheckResult_SelectionChanged;

                var row = new DataGridViewRow();
                row.CreateCells(_dgvCheckResult,
                    DateTime.Now.ToString("HH:mm:ss"), code, "Không tìm thấy",
                    "", "", "", "❌", jobName);
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                row.DefaultCellStyle.ForeColor = Color.Red;
                _dgvCheckResult.Rows.Insert(0, row);
                _dgvCheckResult.FirstDisplayedScrollingRowIndex = 0;

                _dgvCheckResult.ClearSelection();
                _dgvCheckResult.Rows[0].Selected = true;
            }
            finally
            {
                _dgvCheckResult.SelectionChanged += DgvCheckResult_SelectionChanged;
                _dgvCheckResult.ResumeLayout();
            }

            if (_rtbDetail != null)
            {
                _rtbDetail.ForeColor = Color.Red;
                _rtbDetail.Text =
                    $"❌  Không tìm thấy\r\n" +
                    $"{new string('─', 40)}\r\n" +
                    $"Mã quét  : {code}\r\n" +
                    $"Job      : {jobName}\r\n" +
                    $"Thời gian: {DateTime.Now:HH:mm:ss}\r\n\r\n" +
                    $"Mã này không tồn tại trong cơ sở dữ liệu.";
            }
        }

        //private void Shared_OnSerialDeviceReadDataChange_CheckMode(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (!(sender is DetectModel detectModel)) return;
        //        string scannedCode = (detectModel.Text ?? "").Trim();
        //        if (_isTabCheckMode)
        //            ShowLookupResult(scannedCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError("Error in Shared_OnSerialDeviceReadDataChange_CheckMode: " + ex.Message);
        //    }
        //}
        private void Shared_OnSerialDeviceReadDataChange_CheckMode(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is DetectModel detectModel)) return;
                string scannedCode = (detectModel.Text ?? "").Trim();

                // ── Normalize GS1 AI parentheses: (00)046071... → 00046071... ──
                var match = System.Text.RegularExpressions.Regex.Match(
                    scannedCode, @"^\((\d{2,4})\)(.+)$");
                if (match.Success)
                    scannedCode = match.Groups[1].Value + match.Groups[2].Value;
                // ─────────────────────────────────────────────────────────────────

                if (_isTabCheckMode)
                    ShowLookupResult(scannedCode);
                else if (_isMappedTab)
                    ShowMappedValidationResult(scannedCode);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in Shared_OnSerialDeviceReadDataChange_CheckMode: " + ex.Message);
            }
        }

        #endregion Tab Check Mode
        #region Tab Mapped Validation

        private void InitMappedTab()
        {
            if (InvokeRequired) { Invoke(new Action(InitMappedTab)); return; }

            splitMapped.Panel1MinSize = 300;
            splitMapped.Panel2MinSize = 180;

            // Header style
            dgvMapped.EnableHeadersVisualStyles = false;
            var hdrStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 171, 230),
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold),
                SelectionBackColor = Color.FromArgb(0, 171, 230),
                SelectionForeColor = Color.White
            };
            dgvMapped.ColumnHeadersDefaultCellStyle = hdrStyle;

            var cellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Microsoft Sans Serif", 9f),
                ForeColor = Color.FromArgb(64, 64, 64),
                SelectionBackColor = Color.FromArgb(255, 253, 224),
                SelectionForeColor = Color.FromArgb(64, 64, 64)
            };
            dgvMapped.RowsDefaultCellStyle = cellStyle;

            // Wire events
            _cboMappedJobSelect.SelectedIndexChanged += (s, e) => ResetMappedValidation();
            dgvMapped.SelectionChanged += DgvMapped_SelectionChanged;
            btnMappedReset.Click += (s, e) => ResetMappedValidation();

            LoadJobListToMappedComboBox();
        }


        private void LoadJobListToMappedComboBox()
        {
            if (_cboMappedJobSelect == null) return;
            if (InvokeRequired) { Invoke(new Action(LoadJobListToMappedComboBox)); return; }

            string previousSelection = _cboMappedJobSelect.SelectedItem?.ToString();

            _cboMappedJobSelect.Items.Clear();
            _cboMappedJobSelect.Items.Add("--- Chọn Job ---");

            var jobNames = Shared.GetJobNameList();
            if (jobNames != null)
            {
                foreach (var name in jobNames)
                {
                    var job = Shared.GetJob(name);
                    if (job == null || job.JobStatus == JobStatus.Deleted) continue;
                    if (!AllValuesFileExists(job)) continue; // Chỉ hiện job đã chạy
                    _cboMappedJobSelect.Items.Add(name);
                }
            }

            // Giữ lại lựa chọn cũ nếu vẫn còn tồn tại
            int restoreIdx = string.IsNullOrEmpty(previousSelection)
                ? -1
                : _cboMappedJobSelect.Items.IndexOf(previousSelection);

            if (restoreIdx > 0)
                _cboMappedJobSelect.SelectedIndex = restoreIdx;
            else if (_cboMappedJobSelect.Items.Count > 1)
                _cboMappedJobSelect.SelectedIndex = 1; // Tự chọn job đầu tiên có sẵn
            else
                _cboMappedJobSelect.SelectedIndex = 0;
        }
        private void DgvMapped_SelectionChanged(object sender, EventArgs e)
        {
            if (_dgvMapped == null || _dgvMapped.SelectedRows.Count == 0) return;
            var row = _dgvMapped.SelectedRows[0];
            string time = row.Cells["colMTime"].Value?.ToString() ?? "";
            string code = row.Cells["colMCode"].Value?.ToString() ?? "";
            string result = row.Cells["colMResult"].Value?.ToString() ?? "";
            string detail = row.Cells["colMDetail"].Value?.ToString() ?? "";

            if (_rtbMappedDetail == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($"Thời gian : {time}");
            sb.AppendLine($"Mã quét   : {code}");
            sb.AppendLine($"Kết quả   : {result}");
            sb.AppendLine(new string('─', 40));
            sb.AppendLine(detail);

            bool isPass = result.Contains("PASS");
            bool isRef = result.Contains("📌");
            _rtbMappedDetail.ForeColor = isRef ? Color.FromArgb(0, 100, 160)
                                        : isPass ? Color.DarkGreen
                                        : result.Contains("⚠️") ? Color.DarkOrange
                                        : Color.Red;
            _rtbMappedDetail.Text = sb.ToString();
        }

        private void ResetMappedValidation()
        {
            if (InvokeRequired) { Invoke(new Action(ResetMappedValidation)); return; }

            _mappedRefCode = "";
            _mappedRefLevel = QRCodeLevel.Unknown;
            _mappedRefResult = null;
            _mappedProcess = null;
            _mappedPassCount = 0;
            _mappedFailCount = 0;
            _mappedDuplicateCount = 0;
          
            _mappedPassedCodes.Clear();
            if (_dgvMapped != null) _dgvMapped.Rows.Clear();
            if (_rtbMappedDetail != null)
            {
                _rtbMappedDetail.ForeColor = Color.Gray;
                _rtbMappedDetail.Text = "(Quét mã để xem kết quả kiểm tra)";
            }
            if (_lblMappedRefDisplay != null)
            {
                _lblMappedRefDisplay.Text = "(Quét mã Hộp / Thùng / Pallet để bắt đầu)";
                _lblMappedRefDisplay.ForeColor = Color.Gray;
            }
            UpdateMappedStats();
        }

        private void UpdateMappedStats()
        {
            if (InvokeRequired) { BeginInvoke(new Action(UpdateMappedStats)); return; }

            // Tổng số mã con của tham chiếu hiện tại
            int totalChild = 0;
            if (_mappedRefResult != null)
            {
                switch (_mappedRefLevel)
                {
                    case QRCodeLevel.Box: totalChild = _mappedRefResult.ProductCodes?.Count ?? 0; break;
                    case QRCodeLevel.Carton: totalChild = _mappedRefResult.BoxCodes?.Count ?? 0; break;
                    case QRCodeLevel.Pallet: totalChild = _mappedRefResult.CartonCodes?.Count ?? 0; break;
                }
            }

            if (_lblMappedPassCount != null)
                _lblMappedPassCount.Text = totalChild > 0
                    ? $"✅  PASS: {_mappedPassCount} / {totalChild}"
                    : $"✅  PASS: {_mappedPassCount}";

            if (_lblMappedFailCount != null)
                _lblMappedFailCount.Text = $"❌  FAIL: {_mappedFailCount}";

            if (_lblMappedTotalCount != null)
                _lblMappedTotalCount.Text =
                    $"Tổng: {_mappedPassCount + _mappedFailCount + _mappedDuplicateCount}" +
                    (_mappedDuplicateCount > 0 ? $"  ({_mappedDuplicateCount} trùng)" : "");
        }
        /// <summary>
        /// Entry point chính: điều hướng scan → đặt tham chiếu hoặc validate.
        /// </summary>
        //private void ShowMappedValidationResult(string scannedCode)
        //{
        //    if (InvokeRequired) { Invoke(new Action(() => ShowMappedValidationResult(scannedCode))); return; }
        //    if (string.IsNullOrWhiteSpace(scannedCode)) return;

        //    string timeStr = DateTime.Now.ToString("HH:mm:ss");

        //    if (_mappedRefResult == null)
        //    {
        //        TrySetMappedReference(scannedCode, timeStr);
        //        return;
        //    }

        //    // Tìm cấp độ trong job hiện tại trước, nếu Unknown → tìm toàn bộ jobs
        //    QRCodeLevel scannedLevel = TryIdentifyLevelInProcess(scannedCode, _mappedProcess);
        //    if (scannedLevel == QRCodeLevel.Unknown)
        //        scannedLevel = TryIdentifyLevelInAllJobs(scannedCode);  // ← FIX Bug 3

        //    // Cùng cấp với tham chiếu hiện tại → cập nhật tham chiếu mới
        //    if (scannedLevel == _mappedRefLevel && scannedCode != _mappedRefCode)
        //    {
        //        TrySetMappedReference(scannedCode, timeStr);
        //        return;
        //    }

        //    ValidateMappedCode(scannedCode, timeStr);
        //}
        private void ShowMappedValidationResult(string scannedCode)
        {
            if (InvokeRequired) { Invoke(new Action(() => ShowMappedValidationResult(scannedCode))); return; }
            if (string.IsNullOrWhiteSpace(scannedCode)) return;

            string timeStr = DateTime.Now.ToString("HH:mm:ss");

            // Chưa có tham chiếu → scan đầu tiên luôn là đặt tham chiếu
            if (_mappedRefResult == null)
            {
                TrySetMappedReference(scannedCode, timeStr);
                return;
            }

            // Đã có tham chiếu → mọi mã quét tiếp theo đều kiểm tra (PASS/FAIL)
            // Muốn đổi tham chiếu mới → phải bấm Reset trước
            ValidateMappedCode(scannedCode, timeStr);
        }
        /// <summary>
        /// Tìm cấp độ mã trong tất cả jobs đã chạy (có AllValues file).
        /// </summary>
        private QRCodeLevel TryIdentifyLevelInAllJobs(string code)
        {
            var jobNames = Shared.GetJobNameList();
            if (jobNames == null) return QRCodeLevel.Unknown;

            foreach (var name in jobNames)
            {
                var job = Shared.GetJob(name);
                if (job == null || job.JobStatus == JobStatus.Deleted || !AllValuesFileExists(job)) continue;
                var proc = GetSessionProcess(job);
                var level = TryIdentifyLevelInProcess(code, proc);
                if (level != QRCodeLevel.Unknown) return level;
            }
            return QRCodeLevel.Unknown;
        }


        private void SetMappedRef(string code, QRCodeLevel level, DrocoLookupResult result,
            DrocoAllValueProcess proc, string jobName, string timeStr)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetMappedRef(code, level, result, proc, jobName, timeStr))); return; }

            _mappedRefCode = code;
            _mappedRefLevel = level;
            _mappedRefResult = result;
            _mappedProcess = proc;

            string levelLabel = result.LevelLabel ?? level.ToString();
            int childCount;
            string childType;
            switch (level)
            {
                case QRCodeLevel.Box:
                    childCount = result.ProductCodes?.Count ?? 0;
                    childType = "mã GS1";
                    break;
                case QRCodeLevel.Carton:
                    childCount = result.BoxCodes?.Count ?? 0;
                    childType = "mã Hộp";
                    break;
                default: // Pallet
                    childCount = result.CartonCodes?.Count ?? 0;
                    childType = "mã Thùng";
                    break;
            }

            if (_lblMappedRefDisplay != null)
            {
                _lblMappedRefDisplay.Text = $"[{levelLabel}]  {code}   ({childCount} {childType})   Job: {jobName}";
                _lblMappedRefDisplay.ForeColor = Color.FromArgb(0, 120, 180);
            }

            string detail = $"Đặt tham chiếu: [{levelLabel}] {code}\r\nJob: {jobName}\r\nSố con: {childCount} {childType}\r\nBây giờ quét mã con để kiểm tra.";
            AddMappedRow(timeStr, code, $"📌 {levelLabel}", detail,
                Color.FromArgb(225, 242, 255), Color.FromArgb(0, 100, 160));
        }

        /// <summary>
        /// Kiểm tra mã quét có thuộc container tham chiếu không → PASS / FAIL.
        /// </summary>T
        private QRCodeLevel GetExpectedChildLevel(QRCodeLevel refLevel)
        {
            switch (refLevel)
            {
                case QRCodeLevel.Box: return QRCodeLevel.GS1;
                case QRCodeLevel.Carton: return QRCodeLevel.Box;
                case QRCodeLevel.Pallet: return QRCodeLevel.Carton;
                default: return QRCodeLevel.Unknown;
            }
        }

        private void ValidateMappedCode(string scannedCode, string timeStr)
        {
            if (_mappedRefResult == null) return;
            // ── Kiểm tra duplicate TRƯỚC mọi thứ ────────────────────────────────
            if (_mappedPassedCodes.Contains(scannedCode))          
            {
                _mappedDuplicateCount++;
                AddMappedRow(timeStr, scannedCode, "⚠️ Trùng lặp",
                    $"Mã này đã PASS trước đó.\r\nKhông tính vào kết quả.",
                    Color.FromArgb(255, 248, 220), Color.DarkOrange);
                UpdateMappedStats();
                return;
            }
           
            // ── FIX Bug 4: phát hiện sai cấp độ ──────────────────────────────────
            QRCodeLevel scannedLevel = TryIdentifyLevelInProcess(scannedCode, _mappedProcess);
            if (scannedLevel == QRCodeLevel.Unknown)
                scannedLevel = TryIdentifyLevelInAllJobs(scannedCode);

            QRCodeLevel expectedChild = GetExpectedChildLevel(_mappedRefLevel);
            if (scannedLevel != QRCodeLevel.Unknown && scannedLevel != expectedChild)
            {
                _mappedFailCount++;
                string mismatchDetail =
                    $"⚠️ Sai cấp độ!\r\n" +
                    $"Tham chiếu  : [{_mappedRefResult.LevelLabel}]  {_mappedRefCode}\r\n" +
                    $"Mã quét     : [{scannedLevel}]\r\n" +
                    $"Cần quét    : [{expectedChild}] để kiểm tra.";
                AddMappedRow(timeStr, scannedCode, "⚠️ Sai cấp", mismatchDetail,
                    Color.FromArgb(255, 248, 220), Color.DarkOrange);
                UpdateMappedStats();
                return;
            }
            // ─────────────────────────────────────────────────────────────────────

            bool isPass = false;
            string detail;

            switch (_mappedRefLevel)
            {
                case QRCodeLevel.Box:
                    {
                        var list = _mappedRefResult.ProductCodes ?? new List<string>();
                        isPass = list.Any(c => string.Equals(c.Trim(), scannedCode.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (isPass)
                        {
                            detail = $"✔ Mã GS1 thuộc Hộp: {_mappedRefCode}";
                        }
                        else
                        {
                            string parent = FindActualParentBox(scannedCode);
                            detail = string.IsNullOrEmpty(parent)
                                ? $"Mã GS1 không thuộc Hộp: {_mappedRefCode}"
                                : $"Mã GS1 không thuộc Hộp: {_mappedRefCode}\r\n→ Thuộc Hộp: {parent}";
                        }
                        break;
                    }
                case QRCodeLevel.Carton:
                    {
                        var list = _mappedRefResult.BoxCodes ?? new List<string>();
                        isPass = list.Any(c => string.Equals(c.Trim(), scannedCode.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (isPass)
                        {
                            detail = $"✔ Mã Hộp thuộc Thùng: {_mappedRefCode}";
                        }
                        else
                        {
                            string parent = FindActualParentCarton(scannedCode);
                            detail = string.IsNullOrEmpty(parent)
                                ? $"Mã Hộp không thuộc Thùng: {_mappedRefCode}"
                                : $"Mã Hộp không thuộc Thùng: {_mappedRefCode}\r\n→ Thuộc Thùng: {parent}";
                        }
                        break;
                    }
                case QRCodeLevel.Pallet:
                    {
                        var list = _mappedRefResult.CartonCodes ?? new List<string>();
                        isPass = list.Any(c => string.Equals(c.Trim(), scannedCode.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (isPass)
                        {
                            detail = $"✔ Mã Thùng thuộc Pallet: {_mappedRefCode}";
                        }
                        else
                        {
                            string parent = FindActualParentPallet(scannedCode);
                            detail = string.IsNullOrEmpty(parent)
                                ? $"Mã Thùng không thuộc Pallet: {_mappedRefCode}"
                                : $"Mã Thùng không thuộc Pallet: {_mappedRefCode}\r\n→ Thuộc Pallet: {parent}";
                        }
                        break;
                    }
                default:
                    detail = "Không có mã tham chiếu.";
                    break;
            }

            if (isPass)
            {
                _mappedPassedCodes.Add(scannedCode);
                _mappedPassCount++;
                AddMappedRow(timeStr, scannedCode, "✅ PASS", detail,
                    Color.FromArgb(236, 255, 236), Color.DarkGreen);
            }
            else
            {
                _mappedFailCount++;
                AddMappedRow(timeStr, scannedCode, "❌ FAIL", detail,
                    Color.FromArgb(255, 235, 235), Color.Red);
            }

            UpdateMappedStats();
        }

        // ── Helper: nhận dạng cấp của mã trong process đang dùng ──
        private QRCodeLevel TryIdentifyLevelInProcess(string code, DrocoAllValueProcess process)
        {
            if (process == null) return QRCodeLevel.Unknown;
            try
            {
                if (process.LookupBoxCode(code).IsFound) return QRCodeLevel.Box;
                if (process.LookupCartonCode(code).IsFound) return QRCodeLevel.Carton;
                if (process.LookupPalletCode(code).IsFound) return QRCodeLevel.Pallet;
            }
            catch { }
            return QRCodeLevel.Unknown;
        }

        // ── Helper: tìm Hộp thực sự chứa mã GS1 (để hiện "→ thuộc Hộp X") ──
        private string FindActualParentBox(string gs1Code)
        {
            try
            {
                if (_mappedProcess == null) return "";
                var res = _mappedProcess.LookupProductCode(gs1Code);
                return res.IsFound ? (res.BoxQR ?? "") : "";
            }
            catch { return ""; }
        }

        // ── Helper: tìm Thùng thực sự chứa mã Hộp ──
        private string FindActualParentCarton(string boxCode)
        {
            try
            {
                if (_mappedProcess == null) return "";
                var res = _mappedProcess.LookupBoxCode(boxCode);
                return res.IsFound ? (res.CartonQR ?? "") : "";
            }
            catch { return ""; }
        }

        // ── Helper: tìm Pallet thực sự chứa mã Thùng ──
        private string FindActualParentPallet(string cartonCode)
        {
            try
            {
                if (_mappedProcess == null) return "";
                var res = _mappedProcess.LookupCartonCode(cartonCode);
                return res.IsFound ? (res.PalletQR ?? "") : "";
            }
            catch { return ""; }
        }

        private void AddMappedRow(string time, string code, string result, string detail,
            Color backColor, Color foreColor)
        {
            if (_dgvMapped == null) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddMappedRow(time, code, result, detail, backColor, foreColor)));
                return;
            }

            try
            {
                _dgvMapped.SuspendLayout();
                _dgvMapped.SelectionChanged -= DgvMapped_SelectionChanged;

                var row = new DataGridViewRow();
                row.CreateCells(_dgvMapped, time, code, result, detail);
                row.DefaultCellStyle.BackColor = backColor;
                row.DefaultCellStyle.ForeColor = foreColor;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 253, 224);
                row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(64, 64, 64);

                _dgvMapped.Rows.Insert(0, row);
                _dgvMapped.FirstDisplayedScrollingRowIndex = 0;

                _dgvMapped.ClearSelection();
                _dgvMapped.Rows[0].Selected = true;
            }
            finally
            {
                _dgvMapped.SelectionChanged += DgvMapped_SelectionChanged;
                _dgvMapped.ResumeLayout();
            }

            // Cập nhật detail panel ngay lập tức
            if (_rtbMappedDetail != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Thời gian : {time}");
                sb.AppendLine($"Mã quét   : {code}");
                sb.AppendLine($"Kết quả   : {result}");
                sb.AppendLine(new string('─', 40));
                sb.AppendLine(detail);
                _rtbMappedDetail.ForeColor = foreColor;
                _rtbMappedDetail.Text = sb.ToString();
            }

            ProjectLogger.WriteInfo($"[TabMapped] {code} → {result}");
        }

        #endregion Tab Mapped Validation

        private void InitControls()
        {
#if DEBUG
            DebugVirtual();
#endif


            btnDelete.Visible = Shared.Settings.IsAllowJobDeletion;
            _LabelStatusCameraList.Add(lblStatusCamera01);
            UpdateStatusLabelCamera();
            _LabelStatusPrinterList.Add(lblStatusPrinter01);
            UpdateStatusLabelPrinter();
            _NameOfJobOld = "";
            CreateJob();
            cuzButtonPurge.Visible = Properties.Settings.Default.Username == "demo";
            _TimerDateTime.Start();
            _NameOfJobOld = "";
            Shared.JobNameSelected = "";
            var podText = new PODModel(0, "", PODModel.TypePOD.TEXT, "");
            _PODList.Add(podText);

            btnSettings.Enabled = Shared.UserPermission.Settings;
            btnDelete.Enabled = Shared.UserPermission.DeleteJob;
            tabPage2.Enabled = Shared.UserPermission.CreateJob;

            for (int index = 1; index <= 20; index++)
            {
                var podVCD = new PODModel(index, "", PODModel.TypePOD.FIELD, "");
                _PODList.Add(podVCD);
            }

            MonitorCameraConnection();
            MonitorCameraConnection_CognexSupport();
            MonitorPrinterConnection();
            MonitorZebraPrinterConnection();
            MonitorSensorControllerConnection();
            MonitorSerialDeviceControllerConnection();
            MonitorListenerServer();
            // === Căn chỉnh vị trí 3 hàng: Mã/Hộp, Hộp/Thùng, Thùng/Pallet ===
            int labelX = labelProductInBox.Location.X;
            int inputX = codesInBoxNum.Location.X;
            int rowHeight = 36;
            int baseY = codesInBoxNum.Location.Y;

            // === Ràng buộc NumericUpDown: chỉ nhập số nguyên dương, không bỏ trống ===
            // Phải set Value TRƯỚC Minimum để tránh lỗi Value < Minimum
            codesInBoxNum.Maximum = 999999;
            codesInBoxNum.DecimalPlaces = 0;
            codesInBoxNum.Value = 0;
            codesInBoxNum.Minimum = 0;

            boxesInCartonNum.Maximum = 999999;
            boxesInCartonNum.DecimalPlaces = 0;
            boxesInCartonNum.Value = 0;
            boxesInCartonNum.Minimum = 0;

            cartonsInPalletNum.Maximum = 999999;
            cartonsInPalletNum.DecimalPlaces = 0;
            cartonsInPalletNum.Value = 0;
            cartonsInPalletNum.Minimum = 0;

            // Chặn nhập ký tự không phải số
            codesInBoxNum.KeyPress += NumericUpDown_KeyPress;
            boxesInCartonNum.KeyPress += NumericUpDown_KeyPress;
            cartonsInPalletNum.KeyPress += NumericUpDown_KeyPress;

            // Khi rời ô nhập mà trống → tự động đặt lại = 1
            codesInBoxNum.Validating += NumericUpDown_Validating;
            boxesInCartonNum.Validating += NumericUpDown_Validating;
            cartonsInPalletNum.Validating += NumericUpDown_Validating;


            // Hàng 1: "Số mã / hộp" — label1 + codesInBoxNum (đã có sẵn, giữ nguyên)

            // Hàng 2: "Số hộp / thùng"
            //labelBoxesInCarton.Location = new Point(labelX, baseY + rowHeight + 6);
            // boxesInCartonNum.Location = new Point(inputX, baseY + rowHeight);
            //boxesInCartonNum.Size = codesInBoxNum.Size;

            // Hàng 3: "Số thùng / pallet"
            //labelCartonsInPallet.Location = new Point(labelX, baseY + rowHeight * 2 + 6);
            //cartonsInPalletNum.Location = new Point(inputX, baseY + rowHeight * 2);
            //cartonsInPalletNum.Size = codesInBoxNum.Size;

            // Dời nút "Tạo công việc" xuống dưới để không bị đè
            btnSave.Location = new Point(btnSave.Location.X, baseY + rowHeight * 3 + 250);
        }

        /// <summary>
        /// Chỉ cho phép nhập số 0-9 và phím điều khiển (Backspace, Delete...)
        /// </summary>
        private void NumericUpDown_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Khi rời ô nhập: nếu giá trị trống hoặc < 1 → tự động đặt lại = 1
        /// </summary>
        private void NumericUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var numericUpDown = sender as NumericUpDown;
            if (numericUpDown != null)
            {
                if (string.IsNullOrWhiteSpace(numericUpDown.Text) || numericUpDown.Value < 1)
                {
                    numericUpDown.Value = 1;
                }
            }
        }

        private void InitEvents()
        {
            _TimerDateTime.Tick += TimerDateTime_Tick;
            FirstRowHeader.CheckedChanged += ActionResult;
            FirstRowHeaderSscc.CheckedChanged += ActionResult;

            txtSearch.TextChanged += TxtSearch_TextChanged; ;
            btnSettings.Click += ActionResult;
            listBoxJobList.SelectedIndexChanged += ActionResult;
            btnRefesh.Click += ActionResult;
            btnImportDatabase.Click += ActionResult;
            btnImportDatabaseSSCC.Click += ActionResult;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnSerialDeviceReadDataChange += Shared_OnSerialDeviceReadDataChange_CheckMode;
            Load += FrmJob_Load;
            tabControl1.SelectedIndexChanged += ActionResult;
            tabPage2.Click += ActionResult;
            boxesInCartonNum.ValueChanged += BoxesInCartonNum_ValueChanged;
            boxesInCartonNum.KeyUp += BoxesInCartonNum_ValueChanged;
            cartonsInPalletNum.ValueChanged += CartonsInPalletNum_ValueChanged;
            cartonsInPalletNum.KeyUp += CartonsInPalletNum_ValueChanged;
            btnExit.Click += BtnClose_Click;
            btnNext.Click += ActionResult;
            btnSave.Click += ActionResult;
            btnAbout.Click += ActionResult;
            btnHelp.Click += ActionResult;
            btnRestart.Click += ActionResult;
            btnDelete.Click += ActionResult;
            _dgvCheckResult.SelectionChanged += DgvCheckResult_SelectionChanged;
            _cboJobSelect.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            listBoxJobList.DrawItem += ListBoxJobList_DrawItem;

            Shared.OnPrintingStateChange += Shared_OnPrintingStateChange;
            Shared.OnPrinterStatusChange += Shared_OnPrinterStatusChange;
            Shared.OnZebraPrinterStatusChange += Shared_OnZebraPrinterStatusChange;
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
            codesInBoxNum.ValueChanged += CodesInBoxNum_ValueChanged;
            codesInBoxNum.KeyUp += CodesInBoxNum_ValueChanged;
            // ISCamera.UpdateLabelStatusEvent += UpdateLabelStatusEvent;

            cuzButtonPurge.Click += CuzButtonPurge_Click;
            tabControl1.Click += TabPage3_Click;
        }
        private void BoxesInCartonNum_ValueChanged(object sender, EventArgs e)
        {
            _JobModel.NumberOfBoxesInCarton = (int)boxesInCartonNum.Value;
        }

        private void CartonsInPalletNum_ValueChanged(object sender, EventArgs e)
        {
            _JobModel.NumberOfCartonsInPallet = (int)cartonsInPalletNum.Value;
        }
        private void CodesInBoxNum_ValueChanged(object sender, EventArgs e)
        {
            _JobModel.NumberOfCodesInBox = (int)codesInBoxNum.Value;
            _JobModel.NumberOfCodesInPallet = (int)codesInBoxNum.Value; // backward compat
        }

        private void TabPage3_Click(object sender, EventArgs e)
        {

        }

        private void CuzButtonPurge_Click(object sender, EventArgs e)
        {
            try
            {
                if (_IsProcessing || listBoxJobList.SelectedItem == null || _JobModel.FileName == null)
                {
                    CuzMessageBox.Show(this, "Please select valid Job to Purge !", "Purge Job", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var resPurgeDialog = CuzMessageBox.Show(this, "Do you want to Purge this Job !", "Purge Job", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
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
        private async void MonitorListenerServer()
        {
            try
            {
                await StartListenerServer();
            }
            catch (Exception exx)
            {
                CustomMessageBox.Show(this, "ERROR: " + exx, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                DatabaseChecked(true, true);
            }
            else
            {
                if (_JobModel.CompareType == CompareType.Database) _JobModel.JobType = JobType.StandAlone;
                DatabaseChecked(false, true);
            }

        }

        private void DatabaseChecked(bool isChecked, bool isTemplate)
        {
            if (isChecked)
            {
                txtDirectoryDatabse.Enabled = true;

                btnImportDatabase.Enabled = true;

                txtDirectoryDatabse.BackColor = Color.White;

                txtDirectoryDatabse.Text = "";
            }
            else
            {
                txtDirectoryDatabse.Enabled = false;
                btnImportDatabase.Enabled = false;
                txtDirectoryDatabse.BackColor = Color.WhiteSmoke;
                txtDirectoryDatabse.Text = "";
            }
        }

        public void ShowForm()
        {
            Show();
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
        }
        private void ResetAllUIControls()
        {
            // Xóa dữ liệu các control liên quan đến job
            listBoxJobList.Items.Clear();
            txtDirectoryDatabse.Text = "";
            txtDirectoryDatabseCodeSSCC.Text = "";
            codesInBoxNum.Value = 0;
            boxesInCartonNum.Value = 0;
            cartonsInPalletNum.Value = 0;
            txtBatchNumber.Text = "";
            txtSalesOrder.Text = "";
            txtModel.Text = "";
            //txtJobType.Text = "";
            //txtJobStatus.Text = "";
            //lblJobNameInfo.Text = "";
            //lblCompareTypeInfo.Text = "";
            //lblStaticTextInfo.Text = "0";
            //lblPODFormatInfo.Text = "0 / 0  (0 hộp)";
            //lblTemplatePrintInfo.Text = "0 / 0  (0 thùng)";
            //txtJobType.Text = "0 / 0  (0 pallet)";

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
                AutoLoad = true
            };
            listBoxJobList.Enabled = true;
            UpdateUIClearTextBoxInfo(_JobModel);
        }

        //private string OpenDirectoryFileDatabase()
        //{
        //    using (var openFileDialog1 = new OpenFileDialog())
        //    {
        //        string filePath = "";
        //        openFileDialog1.Filter = "Database files (*.csv, *.txt)|*.csv;*.txt";
        //        openFileDialog1.FilterIndex = 0;
        //        openFileDialog1.RestoreDirectory = true;

        //        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        //        {
        //            filePath = openFileDialog1.FileName;
        //        }
        //        return filePath;
        //    }
        //}

        private string OpenDirectoryFileDatabase()
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = "Select a database file (CSV, TXT, or Excel)";
                openFileDialog1.Filter =
                    "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|" +
                    "CSV Files (*.csv)|*.csv|" +
                    "Text Files (*.txt)|*.txt|" +
                    "All Supported Files (*.xlsx;*.xls;*.csv;*.txt)|*.xlsx;*.xls;*.csv;*.txt|" +
                    "All Files (*.*)|*.*";

                openFileDialog1.FilterIndex = 4; // Starts with "All Supported Files" as default (recommended)
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.Multiselect = false;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog1.FileName;
                }

                return string.Empty; // or return null if preferred
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
                AutoLoad = true
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
                            CuzMessageBox.Show(this, warningMsg, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string message = Lang.AreYouSureYouWantToDeleteFile + "\r\n" + _NameOfJobOld;
                    DialogResult result = CuzMessageBox.Show(this, message, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
            job.CompareType = CompareType.Database;
            job.DirectoryDatabase = txtDirectoryDatabse.Text;
            job.DirectoryDatabaseCodeSSCC = txtDirectoryDatabseCodeSSCC.Text.Trim();
            job.PrinterSeries = true;
            job.IsFirstRowHeader = FirstRowHeader.Checked;
            job.IsFirstRowHeaderSscc = FirstRowHeaderSscc.Checked;
            job.PODFormat = _PODFormat;
            job.StaticText = "";
            job.NumberOfCodesInBox = (int)codesInBoxNum.Value;
            job.NumberOfCodesInPallet = (int)codesInBoxNum.Value; // backward compat
            job.NumberOfBoxesInCarton = (int)boxesInCartonNum.Value;
            job.NumberOfCartonsInPallet = (int)cartonsInPalletNum.Value;
            job.TemplatePrint = Shared.Settings.PrintTemplate;
            job.JobType = JobType.AfterProduction;
            job.JobStatus = JobStatus.NewlyCreated;

            // === Batch Info ===
            job.BatchNumber = txtBatchNumber.Text.Trim();
            job.SalesOrder = txtSalesOrder.Text.Trim();
            job.ProductModel = txtModel.Text.Trim();

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
                CuzMessageBox.Show(this, "Purge Job successfully !", "Purge Job", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
            }
        }

        private async Task SaveJobAsync()
        {
            try
            {
                _JobModel = InitJobModel();
                if (_JobModel != null)
                {
                    _JobModel.FileName = DateTime.Now.ToString("yyMMdd_HHmmss") + "_LINE_" + Shared.Settings.LineIndex.ToString("D2") + "_" + RandomCharHelper.GetThreeRandomCharacters();
                    string JobName = _JobModel.FileName;
                    if (JobName == "")
                    {
                        CuzMessageBox.Show(this, Lang.PleaseInputJobName, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (_JobModel.NumberOfCodesInBox <= 0)
                    {
                        CuzMessageBox.Show(this, "Vui lòng nhập số lượng mã / hộp hợp lệ!", Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (_JobModel.NumberOfBoxesInCarton <= 0)
                    {
                        CuzMessageBox.Show(this, "Vui lòng nhập số lượng hộp / thùng hợp lệ!", Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (_JobModel.NumberOfCartonsInPallet <= 0)
                    {
                        CuzMessageBox.Show(this, "Vui lòng nhập số lượng thùng / pallet hợp lệ!", Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // === Validate Batch (chỉ kiểm tra 1 lần duy nhất) ===
                    if (string.IsNullOrWhiteSpace(_JobModel.BatchNumber))
                    {
                        CuzMessageBox.Show(this, "Vui lòng nhập mã lô hàng!", Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(_JobModel.SalesOrder))
                    {
                        CuzMessageBox.Show(this, "Vui lòng nhập SO!", Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(_JobModel.ProductModel))
                    {
                        CuzMessageBox.Show(this, "Vui lòng nhập mã sản phẩm!", Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }


                    var JobNameList = Shared.GetJobNameList();
                    bool jobExists = JobNameList.Any(name =>
                        Path.GetFileNameWithoutExtension(name).Equals(JobName, StringComparison.OrdinalIgnoreCase)
                    );
                    if (jobExists)
                    {
                        CuzMessageBox.Show(this, Lang.JobNameAlreadyExists, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (_JobModel.PrinterSeries)
                    {
                        if (_JobModel.CompareType == CompareType.Database)
                        {
                            string databasePath = _JobModel.DirectoryDatabase;
                            if (_JobModel.CompareType == CompareType.Database && databasePath == "")
                            {
                                CuzMessageBox.Show(this, Lang.PleaseSelectDatabasePath, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            if (string.IsNullOrWhiteSpace(_JobModel.DirectoryDatabaseCodeSSCC))
                            {
                                CuzMessageBox.Show(this, "Vui lòng chọn file SSCC (dữ liệu QR hộp/thùng)!", Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            _JobModel.PODFormat = new List<PODModel>
                            {
                                new PODModel(1, "<field1>", PODModel.TypePOD.FIELD, "<field1>")
                            };

                            string podFormat = _JobModel.PODFormat.ToString();
                            if (_JobModel.CompareType == CompareType.Database && podFormat == "")
                            {
                                CuzMessageBox.Show(this, Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                            _JobModel.TemplatePrint = Shared.Settings.PrintTemplate;

                            if (_JobModel != null && _JobModel.CompareType == CompareType.Database && !CheckExistTemplatePrint(_JobModel.TemplatePrint) && _JobModel.PrinterSeries)
                            {
                                CuzMessageBox.Show(this, Lang.CheckExistTemplatePrinter, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (_JobModel.CompareType == CompareType.Database)
                        {
                            string databasePath = _JobModel.DirectoryDatabase;
                            if (_JobModel.CompareType == CompareType.Database && databasePath == "")
                            {
                                CuzMessageBox.Show(this, Lang.PleaseSelectDatabasePath, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            string podFormat = _JobModel.PODFormat.ToString();
                            if (_JobModel.CompareType == CompareType.Database && podFormat == "")
                            {
                                CuzMessageBox.Show(this, Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                        else
                        {
                            if (_JobModel.CompareType == CompareType.StaticText)
                            {
                                if (_JobModel.StaticText == "")
                                {
                                    CuzMessageBox.Show(this, Lang.PleaseEnterTheStaticText, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                            }
                            else
                            {
                                _JobModel.StaticText = "";
                            }
                        }
                    }

                    if (Shared.CheckJobHasExist(JobName))
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
                                catch { }
                            }
                            else
                            {
                                string message = Lang.DoYouWantToReplaceExistingTemplate + "\r\n" + JobName + Shared.Settings.JobFileExtension;
                                DialogResult result = CuzMessageBox.Show(this, message, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (result != DialogResult.Yes)
                                {
                                    return;
                                }
                            }
                        }
                    }

                    if (JobName != "")
                    {
                        Shared.DeleteJob(_JobModel);
                    }

                    Shared.JobNameSelected = JobName + Shared.Settings.JobFileExtension;


                    _PODFormat.Clear();
                }

                // Capture local UI variables to avoid cross-thread access during background execution
                string txtDbText = txtDirectoryDatabse.Text;
                string txtSsccText = _JobModel.DirectoryDatabaseCodeSSCC;
                bool isFirstRowHeaderChecked = FirstRowHeader.Checked;
                bool isFirstRowHeaderSsccChecked = _JobModel.IsFirstRowHeaderSscc;

                double numberTotalsCode = 0;
                int totalCodes = 0;
                int codesPerBox = _JobModel.NumberOfCodesInBox;
                int boxesPerCarton = _JobModel.NumberOfBoxesInCarton;
                int cartonsPerPallet = _JobModel.NumberOfCartonsInPallet;

                int totalBoxes = 0;
                int remainderCodes = 0;
                int totalCartons = 0;
                int remainderBoxes = 0;
                int totalPallets = 0;
                int remainderCartons = 0;

                int ssccRequired = 0;
                int ssccAvailable = 0;

                List<string> gs1Duplicates = new List<string>();
                List<string> ssccDuplicates = new List<string>();

                ShowLoading("Đang đọc và kiểm tra tệp dữ liệu...");

                await Task.Run(() =>
                {
                    // === Đọc tổng số mã ===
                    if (!string.IsNullOrEmpty(txtDbText) && File.Exists(txtDbText))
                    {
                        var rows = FileFuncs.ReadExcelData(txtDbText);
                        numberTotalsCode = rows != null ? rows.Count : 0;
                        if (isFirstRowHeaderChecked && numberTotalsCode > 0)
                            numberTotalsCode--;
                    }

                    totalCodes = (int)numberTotalsCode;
                    totalBoxes = codesPerBox > 0 ? (int)Math.Ceiling((double)totalCodes / codesPerBox) : 0;
                    remainderCodes = codesPerBox > 0 ? totalCodes % codesPerBox : 0;

                    totalCartons = boxesPerCarton > 0 ? (int)Math.Ceiling((double)totalBoxes / boxesPerCarton) : 0;
                    remainderBoxes = boxesPerCarton > 0 ? totalBoxes % boxesPerCarton : 0;

                    totalPallets = cartonsPerPallet > 0 ? (int)Math.Ceiling((double)totalCartons / cartonsPerPallet) : 0;
                    remainderCartons = cartonsPerPallet > 0 ? totalCartons % cartonsPerPallet : 0;

                    // Box QR + Carton QR đều lấy từ file SSCC tuần tự → cần: totalBoxes + totalCartons
                    ssccRequired = totalBoxes + totalCartons;

                    if (!string.IsNullOrEmpty(txtSsccText) && File.Exists(txtSsccText))
                    {
                        ssccAvailable = ExcelCodeCache.CountRows(txtSsccText, hasHeader: isFirstRowHeaderSsccChecked);
                    }

                    // Check GS1 duplicates
                    if (!string.IsNullOrEmpty(txtDbText) && File.Exists(txtDbText))
                    {
                        var gs1Values = ExcelCodeCache.GetColumnValues(txtDbText, hasHeader: false, columnIndex: 0);
                        gs1Duplicates = gs1Values
                            .GroupBy(v => v)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();
                    }

                    // Check SSCC duplicates
                    if (!string.IsNullOrEmpty(txtSsccText) && File.Exists(txtSsccText))
                    {
                        var ssccValues = ExcelCodeCache.GetColumnValues(txtSsccText, hasHeader: isFirstRowHeaderSsccChecked, columnIndex: 0);
                        ssccDuplicates = ssccValues
                            .GroupBy(v => v)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();
                    }
                });

                _JobModel.NumberTotalsCode = numberTotalsCode;

                if (ssccAvailable < ssccRequired)
                {
                    HideLoading();
                    CuzMessageBox.Show(this,
                        "Số mã SSCC không đủ để tạo job!\n\n" +
                        $"▸ Cần:    {ssccRequired} mã  ({totalBoxes} QR Hộp  +  {totalCartons} QR Thùng)\n" +
                        $"▸ Có:     {ssccAvailable} mã\n" +
                        $"▸ Thiếu:  {ssccRequired - ssccAvailable} mã\n\n" +
                        "Vui lòng kiểm tra lại file SSCC hoặc điều chỉnh quy cách đóng gói.",
                        "Không đủ mã SSCC",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (gs1Duplicates.Count > 0)
                {
                    HideLoading();
                    var examples = string.Join("\n   ", gs1Duplicates.Take(10));
                    CuzMessageBox.Show(this,
                        $"File GS1 có {gs1Duplicates.Count} mã bị trùng lặp!\n\n" +
                        $"▸ Ví dụ:\n   {examples}" +
                        (gs1Duplicates.Count > 10 ? $"\n   ... và {gs1Duplicates.Count - 10} mã khác" : "") +
                        "\n\nVui lòng kiểm tra lại file GS1 database.",
                        "Mã GS1 trùng lặp",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (ssccDuplicates.Count > 0)
                {
                    HideLoading();
                    var examples = string.Join("\n   ", ssccDuplicates.Take(10));
                    CuzMessageBox.Show(this,
                        $"File SSCC có {ssccDuplicates.Count} mã bị trùng lặp!\n\n" +
                        $"▸ Ví dụ:\n   {examples}" +
                        (ssccDuplicates.Count > 10 ? $"\n   ... và {ssccDuplicates.Count - 10} mã khác" : "") +
                        "\n\nVui lòng kiểm tra lại file SSCC.",
                        "Mã SSCC trùng lặp",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Show confirmation summary dialog
                HideLoading();

                var sb = new StringBuilder();
                sb.AppendLine("═══ THÔNG TIN JOB ═══");
                sb.AppendLine();
                sb.AppendLine($"▸ Tổng số mã sản phẩm: {totalCodes:N0}");
                sb.AppendLine();
                sb.AppendLine($"▸ Tổng số mã SSCC: {ssccAvailable:N0}");
                sb.AppendLine();
                sb.AppendLine("▸ Quy cách đóng gói:");
                sb.AppendLine($"   • Số mã / Hộp: {codesPerBox}");
                sb.AppendLine($"   • Số Hộp / Thùng: {boxesPerCarton}");
                sb.AppendLine($"   • Số Thùng / Pallet: {cartonsPerPallet}");
                sb.AppendLine();
                sb.AppendLine("▸ Dự kiến:");
                sb.AppendLine($"   • Tổng Hộp: {totalBoxes}" + (remainderCodes > 0 ? $" (hộp cuối lẻ {remainderCodes} mã)" : ""));
                sb.AppendLine($"   • Tổng Thùng: {totalCartons}" + (remainderBoxes > 0 ? $" (thùng cuối lẻ {remainderBoxes} hộp)" : ""));
                sb.AppendLine($"   • Tổng Pallet: {totalPallets}" + (remainderCartons > 0 ? $" (pallet cuối lẻ {remainderCartons} thùng)" : ""));
                sb.AppendLine();
                sb.AppendLine("▸ Thông tin Batch:");
                sb.AppendLine($"   • Batch: {_JobModel.BatchNumber}");
                sb.AppendLine($"   • SO: {_JobModel.SalesOrder}");
                sb.AppendLine($"   • SKU: {_JobModel.ProductModel}");
                sb.AppendLine();
                sb.AppendLine("Bạn có muốn bắt đầu quá trình in không?");

                DialogResult dialogResult = CuzMessageBox.Show(this, sb.ToString(), Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    ShowLoading("Đang chuẩn hóa dữ liệu và tạo công việc...");

                    string destDb = "";
                    string destSscc = "";
                    bool hasDestDb = false;
                    bool hasDestSscc = false;

                    await Task.Run(() =>
                    {
                        // === Đọc xong → chuẩn hoá sang CSV 1 cột, lưu vào thư mục Database ===
                        string databaseDir = Path.Combine(CommVariables.PathDatabaseApp);
                        if (!Directory.Exists(databaseDir))
                            Directory.CreateDirectory(databaseDir);

                        // Tạo prefix tên file: JobName_LOT_ddMMyyyy
                        string safeJob = _JobModel.FileName;
                        string safeLot = string.IsNullOrWhiteSpace(_JobModel.BatchNumber)
                            ? "LOT"
                            : _JobModel.BatchNumber.Replace(" ", "_");
                        string datePart = DateTime.Now.ToString("ddMMyyyy");
                        string filePrefix = $"{safeJob}_{safeLot}_{datePart}";

                        // File GS1: {prefix}_gs1.csv
                        if (!string.IsNullOrEmpty(txtDbText) && File.Exists(txtDbText))
                        {
                            destDb = Path.Combine(databaseDir, filePrefix + "_GS1.csv");
                            ExcelCodeCache.ExportColumnAsCsv(txtDbText, destDb, hasHeader: false, columnIndex: 0);
                            hasDestDb = true;
                        }

                        // File SSCC: {prefix}_sscc.csv
                        if (!string.IsNullOrEmpty(txtSsccText) && File.Exists(txtSsccText))
                        {
                            destSscc = Path.Combine(databaseDir, filePrefix + "_SSCC.csv");
                            ExcelCodeCache.ExportColumnAsCsv(txtSsccText, destSscc, hasHeader: isFirstRowHeaderSsccChecked, columnIndex: 0);
                            hasDestSscc = true;
                        }
                    });

                    if (hasDestDb)
                    {
                        _JobModel.DirectoryDatabase = destDb;
                        txtDirectoryDatabse.Text = destDb;
                    }
                    if (hasDestSscc)
                    {
                        _JobModel.DirectoryDatabaseCodeSSCC = destSscc;
                        txtDirectoryDatabseCodeSSCC.Text = destSscc;
                    }

                    _JobModel.SaveFile();
                    ResetAllUIControls();
                    // === Ghi log tạo Job ===
                    string logMessage = $"Job: {_JobModel.FileName} | " +
                                        $"Tổng mã: {totalCodes} | " +
                                        $"LOT: {_JobModel.BatchNumber} | " +
                                        $"NSX: {_JobModel.SalesOrder} | " +
                                        $"NHH: {_JobModel.ProductModel} | " +
                                        $"Mã/Hộp: {codesPerBox} | " +
                                        $"Hộp/Thùng: {boxesPerCarton} | " +
                                        $"Thùng/Pallet: {cartonsPerPallet} | " +
                                        $"Dự kiến: {totalBoxes} Hộp, {totalCartons} Thùng, {totalPallets} Pallet";

                    LoggingController.SaveHistory(
                        "CreateJob",
                        _JobModel.FileName,
                        logMessage,
                        SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                        LoggingType.Info);

                    if (Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings && _JobModel.CompareType == CompareType.Database && _JobModel.PrinterSeries)
                    {
                        PrinterSettingsModel printerSettingsModel = Shared.GetSettingsPrinter();

                        if (printerSettingsModel.PodDataType != 1)
                        {
                            UpdateUIClearJobInformation();
                            CuzMessageBox.Show(this, Lang.DataTypeMustBeRAWData, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    Hide();
                    _FormMainPC?.Dispose();
                    if (_FormMainPC == null || _FormMainPC.IsDisposed)
                    {
                        _FormMainPC = new FrmMainDroco(this);
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

                    PrinterSupport(_JobModel.PrinterSeries, false);
                }
                else
                {
                    UpdateUIClearJobInformation();
                }

                return;
            }
            catch (Exception ex)
            {
                CuzMessageBox.Show(this, Lang.NewJobCreationFailed + "\n" +
                    ex.Message, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            lblStaticTextInfo.Text = "0";
            lblPODFormatInfo.Text = "0 / 0  (0 hộp)";
            lblTemplatePrintInfo.Text = "0 / 0  (0 thùng)";
            txtJobType.Text = "0 / 0  (0 pallet)";
            txtJobStatus.Text = "";
            txtDirectoryDatabse.Text = jobModel.DirectoryDatabase;
            txtDirectoryDatabseCodeSSCC.Text = jobModel.DirectoryDatabaseCodeSSCC ?? "";

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
            Invoke(new Action(() =>
            {
                picLoading.Visible = false;
            }));
            Thread threadLoadJobNameList = new Thread(() =>
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

                Invoke(new Action(() =>
                {
                    if (_JobNameList != null)
                    {
                        foreach (string JobName in _JobNameList)
                        {
                            JobModel jobModel = Shared.GetJob(JobName);
                            if (jobModel != null && jobModel.JobStatus != JobStatus.Deleted)
                                listBoxJobList.Items.Add(JobName);
                        }
                    }
                }));

                _IsProcessing = false;
                Thread.Sleep(5);
                Invoke(new Action(() =>
                {
                    picLoading.Visible = true;
                }));

                UpdateUILoadJobNameList(true);
            });
            threadLoadJobNameList.IsBackground = true;
            threadLoadJobNameList.Start();
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

                // === Thông tin cơ bản ===
                lblJobNameInfo.Text = jobModel.FileName;
                lblCompareTypeInfo.Text = Lang.Database;
                txtJobType.Text = jobModel.JobType.ToFriendlyString();
                txtJobStatus.Text = jobModel.JobStatus.ToFriendlyString();

                // === Đường dẫn file database ===
                txtDirectoryDatabse.Text = jobModel.DirectoryDatabase;
                txtDirectoryDatabseCodeSSCC.Text = jobModel.DirectoryDatabaseCodeSSCC ?? "";

                // === Tổng mã GS1 ===
                lblStaticText1.Text = "Tổng mã GS1";
                lblStaticTextInfo.Text = jobModel.NumberTotalsCode.ToString("N0");

                // === Số mã đã vào Hộp ===
                int codesInBoxes = 0;
                if (jobModel.BoxList != null)
                {
                    codesInBoxes = jobModel.BoxList
                        .Where(b => b.ProductCodes != null)
                        .Sum(b => b.ProductCodes.Count);
                }
                lblPODFormat.Text = "Mã → Hộp";
                lblPODFormatInfo.Text = $"{codesInBoxes} / {jobModel.NumberTotalsCode:N0}  ({(jobModel.BoxList?.Count ?? 0)} hộp)";

                // === Số hộp đã vào Thùng ===
                int boxesInCartons = 0;
                if (jobModel.DrocoCartonList != null)
                {
                    boxesInCartons = jobModel.DrocoCartonList
                        .Where(c => c.BoxCodes != null)
                        .Sum(c => c.BoxCodes.Count);
                }
                lblTemplatePrint.Text = "Hộp → Thùng";
                lblTemplatePrintInfo.Text = $"{boxesInCartons} / {jobModel.BoxList?.Count ?? 0}  ({(jobModel.DrocoCartonList?.Count ?? 0)} thùng)";

                // === Số thùng đã vào Pallet ===
                int cartonsInPallets = 0;
                if (jobModel.DrocoPalletList != null)
                {
                    cartonsInPallets = jobModel.DrocoPalletList
                        .Where(p => p.CartonCodes != null)
                        .Sum(p => p.CartonCodes.Count);
                }
                lblJobType.Text = "Thùng → Pallet";
                txtJobType.Text = $"{cartonsInPallets} / {jobModel.DrocoCartonList?.Count ?? 0}  ({(jobModel.DrocoPalletList?.Count ?? 0)} pallet)";

                // === Trạng thái Job ===
                lblJobStatus.Text = Lang.JobStatus;
                txtJobStatus.Text = jobModel.JobStatus.ToFriendlyString();

                // === Style ===
                lblStaticTextInfo.BackColor = System.Drawing.Color.White;
                lblPODFormatInfo.BackColor = System.Drawing.Color.White;
                lblTemplatePrintInfo.BackColor = System.Drawing.Color.White;
                txtJobType.BackColor = System.Drawing.Color.White;

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

        }

        public void Exit()
        {
            DialogResult dialogResult = CuzMessageBox.Show(this, Lang.DoYouWantExitApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    LoggingController.SaveHistory(  //Save history
                        Lang.Exit,
                        Lang.LogOut,
                        Lang.LogoutSuccessfully,
                        SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                        LoggingType.LogedOut);

                    Close();
                }
                catch (Exception)
                {

                }
            }
        }

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
                    ShowLabelIcon(lblStatusPrinterZebra, "Zebra", Properties.Resources.zebra_connected_png); //Lang.Printer
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

        #endregion UpdateUI Printer

        #region Monitor Printer
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
                                PODController podController = printerModel.PODController; // Get controller has exist if not exist then add new controller
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
                                    {
                                        podController.Port = printerModel.Port;
                                    }
                                    else if (podController.ServerIP != printerModel.IP)
                                    {
                                        podController.ServerIP = printerModel.IP;
                                    }
                                }
                                bool isConnected = podController.IsConnected();
                                if (isConnected == false)
                                {
                                    podController.Disconnect();
                                    podController.Connect();
                                }
                                if (isConnected != printerModel.IsConnected)
                                {
                                    printerModel.IsConnected = podController.IsConnected();
                                    UpdateStatusLabelPrinter();
                                    Shared.RaiseOnPrinterStatusChangeEvent();
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                    Thread.Sleep(2000);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitorPrinter.Start();
        }

        private void MonitorZebraPrinterConnection()
        {
            _ThreadMonitorZebraPrinter = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        PrinterModel printerModel = Shared.Settings.ZebraPrinter;
                        if (printerModel.IsEnable)
                        {
                            // ── USB mode: chỉ poll trạng thái, không TCP ──────
                            if (printerModel.ZebraSettings?.ConnectionType == ZebraConnectionType.USB)
                            {
                                bool usbOnline = Shared.IsZebraPrinterReady();
                                if (usbOnline != printerModel.IsConnected)
                                {
                                    printerModel.IsConnected = usbOnline;
                                    UpdateStatusLabelZebraPrinter();
                                    Shared.RaiseOnZebraPrinterStatusChangeEvent();
                                }
                                Thread.Sleep(2000);
                                continue;   // bỏ qua toàn bộ TCP logic bên dưới
                            }

                            // ── LAN mode: giữ nguyên logic TCP ───────────────
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
                            if (isConnected == false)
                            {
                                podController.Disconnect();
                                podController.Connect();
                            }

                            if (isConnected != printerModel.IsConnected)
                            {
                                printerModel.IsConnected = podController.IsConnected();
                                UpdateStatusLabelZebraPrinter();
                                Shared.RaiseOnZebraPrinterStatusChangeEvent();
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
            _ThreadMonitorZebraPrinter.Start();
        }


        private void UpdateUIListBoxPrintProductTemplateList(string[] printTemplateNames, string keyWord = "")
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUIListBoxPrintProductTemplateList(printTemplateNames, keyWord)));
                return;
            }

        }
        private void ObtainPrintProductTemplateList()
        {
            if (_IsObtainingPrintProductTemplateList)
            {
                return;
            }
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
        private void EnableUIPrinting(bool isActive = true, bool isObtain = true)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableUIPrinting(isActive, isObtain)));
                return;
            }
            bool isEnable = Shared.Settings.IsPrinting & isActive;
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
                                    case CameraType.CV_X: // DM Series Camera
                                        //cameraModel.Port = "8500"; // Default is 23

                                        if (Shared.cvxCamera == null || counter >= 3)
                                        {
                                            Shared.cvxCamera = new CvxCamera(cameraModel.IP, int.Parse(cameraModel.Port), 3000);
                                            Shared.cvxCamera.Connect();
                                            Shared.cvxCamera.StartListening();
                                        }
                                        else
                                        {
                                            bool checkIP = Shared.cvxCamera.ip == cameraModel.IP;
                                            if (checkIP)
                                            {
                                                bool checkPort = Shared.cvxCamera.port == int.Parse(cameraModel.Port);
                                                if (!checkPort)
                                                {
                                                    Shared.cvxCamera.Disconnect();
                                                    Shared.cvxCamera = null;
                                                }
                                            }
                                            else
                                            {
                                                Shared.cvxCamera.Disconnect();
                                                Shared.cvxCamera = null;
                                            }
                                        }
                                        cameraModel.IsConnected = Shared.cvxCamera.IsConnected();
                                        Shared.RaiseOnCameraStatusChangeEvent();
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

                                        if (Shared.CamController.IsConnected() == false)
                                        {
                                            Shared.CamController.Disconnect();
                                            Shared.CamController.Connect();
                                            counter++;
                                        }
                                        else
                                        {
                                            counter = 0;
                                        }
                                        //cameraModel.IsConnected = Shared.CamController?.IsConnected() ?? false;
                                        //Shared.RaiseOnCameraStatusChangeEvent();
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

                                        if (Shared.CamController.IsConnected() == false)
                                        {
                                            Shared.CamController.Disconnect();
                                            Shared.CamController.Connect();
                                            counter++;
                                        }
                                        else
                                        {
                                            counter = 0;
                                        }
                                        cameraModel.IsConnected = Shared.CamController?.IsConnected() ?? false;
                                        Shared.RaiseOnCameraStatusChangeEvent();
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
                                // Disconnect all camera
                                DisposeSingleHandler();
                                DisposeMultiSyncHandler();
                                DMCamera?.Disconnect();

                                // Connect by camera type   
                                switch (cameraModel.CameraType)
                                {
                                    case CameraType.DM: //DM Series Camera
                                        if (Shared.Settings.CameraList.FirstOrDefault().ReadMode == CameraModeRead.Basic)
                                        {
                                            DMCamera?.Connect(cameraModel.IP);
                                        }
                                        else
                                        {
                                            DMCamera?.MultiReadConnect(cameraModel.IP);
                                        }
                                        cameraModel.CountTimeReconnect++;
                                        if (cameraModel.CountTimeReconnect >= 3)
                                        {
                                            cameraModel.CountTimeReconnect = 0;
                                            DMCamera?._EthSystemDiscoverer?.Discover();
                                        }
                                        break;

                                    case CameraType.IS: //IS Sigle Read Camera (3800)
                                        if (ISSingleHandler == null)
                                        {
                                            ISSingleHandler = new ISSingleHandler(cameraModel.IP, "80");
                                            await ISSingleHandler.FirtConnectionAsync();
                                            await Task.Delay(1000);
                                        }
                                        // Reconnect 2 times
                                        if (!cameraModel.IsConnected)
                                        {
                                            cameraModel.CountTimeReconnect++;
                                            if (cameraModel.CountTimeReconnect >= 2)
                                            {
                                                cameraModel.CountTimeReconnect = 0;
                                            }
                                        }
                                        break;

                                    case CameraType.ISDual:
                                        if (ISMultiSyncHandler == null)
                                        {
                                            ISMultiSyncHandler = new ISMultiSyncHandler(cameraModel.IP, "80", cameraModel.ISSlaveIP, "80");
                                            await ISMultiSyncHandler.FirtConnectionAsync();
                                            await Task.Delay(2000);
                                        }
                                        // Reconnect 2 times
                                        if (!cameraModel.IsConnected)
                                        {
                                            cameraModel.CountTimeReconnect++;
                                            if (cameraModel.CountTimeReconnect >= 2)
                                            {
                                                cameraModel.CountTimeReconnect = 0;
                                            }
                                        }
                                        break;

                                    case CameraType.UKN:

                                        break;
                                }
                            }
                            else
                            {
                                cameraModel.CountTimeReconnect = 0;
                            }
                        }
                        Thread.Sleep(2000);
                    }
                    catch (Exception)
                    {
                    }
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitorCamera.Start();
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
        private void ShowLabelIcon(Label label, string text, Image icon)
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
                        if (Shared.Settings.EnSerialDevice && Shared.SerialDevController == null || !Shared.SerialDevController.IsSerialDevConnected())
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

        private void label1_Click(object sender, EventArgs e)
        {

        }



        private void numberOfCodes_Click(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {

        }

        private void dgvCheckResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void splitContainerCheck_Panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //nhanne
           // Shared.DrocoAllValueProcess = null;
            //TryInitDrocoAllValueProcess();
        }

        private void lblSelectJob_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void dgvMapped_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
       

}
