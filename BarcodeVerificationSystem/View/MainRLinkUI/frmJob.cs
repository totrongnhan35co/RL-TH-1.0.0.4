using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Controller.Camera;
using BarcodeVerificationSystem.Controller.Camera.Hik;
using BarcodeVerificationSystem.Controller.Camera.Keyence;
using BarcodeVerificationSystem.Controller.HistorySync;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.PLC;
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

namespace BarcodeVerificationSystem.View
{
    public partial class FrmJob : Form
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

        private FrmSettings _FormSettings;
        public JobModel _JobModel = null;
        private FrmMain _FormMainPC = null;

        private Thread _ThreadMonitorPrinter;
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
        public static HikrobotCamera hikrobotCamera = null;
        public static bool eventTwoOccurred = false;
        public static object lockObject = new object();
        public static bool isEventTwoHandled = false;
        private readonly string _endOfLineStr = "<EOF>";

        #endregion Variables Jobs

        public FrmJob()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitControls();
            DMCamera.InitCameraVariables();
            InitEvents();
            SetLanguage();
        }

        #region UI_Control_Event
        private void ActionResult(object sender, EventArgs e)
        {
            if (_IsBinding)
            {
                return;
            }
            if (sender == tabControl1)
            {
                Shared.JobNameSelected = "";
                txtFileName.Text = "";
                if (tabControl1.SelectedIndex == 1)
                {
                    PrinterSupport(radRSeries.Checked, false);
                }
                LoadJobNameList();
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
                    _JobModel.TemplatePrint = GetSelectedPrintProductTemplate();
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
            else if (sender == btnGennerate)
            {
                AutoGenerateFileName();
            }
            else if (sender == btnImportDatabase)
            {
                var _FormDatabase = new frmDatabase(this);
                _FormDatabase.ShowDialog();

                var t = _JobModel.DispatchingOrderPayload;

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

                        _FormMainPC?.Dispose();

                        if (_FormMainPC == null || _FormMainPC.IsDisposed)
                        {
                            _FormMainPC = new FrmMain(this);
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
            }
            else if (sender == btnSave)
            {
                if (_JobModel != null)
                {
                    _JobModel.TemplatePrint = GetSelectedPrintProductTemplate();
                    _JobModel.NumberTotalsCode = _NumberTotalsCode;
                    _JobModel.JobStatus = JobStatus.NewlyCreated;
                }

                SaveJob();
            }
            else if (sender == btnAbout)
            {
                var about = new FrmAbout();
                about.ShowDialog();
            }
            else if(sender == btnHelp)
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
        private void TxtSearchTemplate_TextChanged(object sender, EventArgs e)
        {
            string keyWord = txtSearchTemplate.Text.ToLower();
            if (_PrintProductTemplateList.Count() > 0)
            {
                UpdateUIListBoxPrintProductTemplateList(_PrintProductTemplateList, keyWord);
            }
        }
        private void TimerDateTime_Tick(object sender, EventArgs e)
        {
            toolStripDateTime.Text = DateTime.Now.ToString(_DateTimeFormat);
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
            if (_JobModel != null && radDatabase.Checked)
            {
                _JobModel.DirectoryDatabase = txtDirectoryDatabse.Text;
            }
        }
        private void TxtStaticText_TextChanged(object sender, EventArgs e)
        {
            if (_JobModel != null && radStaticText.Checked)
            {
                _JobModel.StaticText = txtStaticText.Text;
            }
        }
        private void TxtFileName_TextChanged(object sender, EventArgs e)
        {
            if (_JobModel != null)
            {
                _JobModel.FileName = txtFileName.Text;
            }
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
            EnableUIPrinting();
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
            tblJobType.Enabled = false;
            UpdateUIListBoxPrintProductTemplateList(_PrintProductTemplateList);
            pnlStandaloneColor.BackColor = _Standalone;
            pnlRLinkSeriesColor.BackColor = _RLinkColor;
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
                    }
                    break;
                case CameraType.IS:
                    ISSingleHandler?.ManualTriggerAction();
                    break;
                case CameraType.ISDual:
                    ISMultiSyncHandler?.ManualTriggerAction();
                    break;
                case CameraType.HIKROBOT:
                    Shared.hikrobotCamera?.SoftwareTrigger();
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
                    if(cameraModel.OutputType == OutputType.OutputCamera)
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
              
                case CameraType.HIKROBOT:
                    if (cameraModel.OutputType == OutputType.CommandError)
                    {
                        Shared.SendErrorOutputToSensorController(currentIndex);
                    }
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
            lblStatusSerialDevice.Text = Lang.ScannerLabel;
            lblSensorControllerStatus.Text = Lang.PLCLabel;
            // thinh them Lang text
            txtJobType.Text = _JobModel.JobType.ToFriendlyString();
            txtJobStatus.Text = _JobModel.JobStatus.ToFriendlyString();

            switch(_JobModel.CompareType)
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

            lblToolStripVersion.Text = Lang.Version + ": " + Properties.Settings.Default.SoftwareVersion;
            btnDelete.Text = Lang.Delete;
            btnHelp.Text = Lang.Help;
            btnRestart.Text = Lang.Restart;

            tabPage1.Text = Lang.SelectJob;
            tabPage2.Text = Lang.CreateANewJob;
        }

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
            cboSupportForCamera.Enabled = false;
            cboSupportForCamera.DataSource = CameraSupportNameList;
            cboSupportForCamera.SelectedIndex = Shared.Settings.CameraList.FirstOrDefault().CameraType == CameraType.DM ? 0 : 1;
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

            MonitorCameraConnection(); // Kết nối camera qua TCP/IP
            MonitorCameraConnection_CognexSupport(); // Kết nối camera Cognex Dataman và IS series

            MonitorPrinterConnection();
            MonitorSensorControllerConnection();
            MonitorSerialDeviceControllerConnection();
            MonitorListenerServer();
        }

        private void InitEvents()
        {
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
            txtPODFormat.TextChanged += TxtPODFormat_TextChanged; ;

            txtSearch.TextChanged += TxtSearch_TextChanged; ;
            txtSearchTemplate.TextChanged += TxtSearchTemplate_TextChanged;

            btnPODFormat.Click += ActionResult;

            btnSettings.Click += ActionResult;
            listBoxJobList.SelectedIndexChanged += ActionResult;
            listBoxPrintProductTemplate.SelectedIndexChanged += ActionResult;
            btnRefesh.Click += ActionResult;
            btnImportDatabase.Click += ActionResult;
            Shared.OnLanguageChange += Shared_OnLanguageChange;

            Load += FrmJob_Load;
            tabControl1.SelectedIndexChanged += ActionResult;
            tabPage2.Click += ActionResult;

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
            tabControl1.Click += TabPage3_Click;
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
            job.DispatchingOrderPayload = _JobModel.DispatchingOrderPayload;

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
                job.TemplatePrint = GetSelectedPrintProductTemplate();
                job.NumberTotalsCode = job.IsFirstRowHeader ? _NumberTotalsCode : _NumberTotalsCode + 1;
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
                    job.NumberTotalsCode = job.IsFirstRowHeader ? _NumberTotalsCode : _NumberTotalsCode + 1;
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
                if (_JobModel != null)   // Check current Job has null
                {
                    string JobName = _JobModel.FileName;  // Check Job name is empty
                    if (JobName == "")
                    {
                        CuzMessageBox.Show(Lang.PleaseInputJobName, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var JobNameList = Shared.GetJobNameList();

                    bool jobExists = JobNameList.Any(name =>
                        Path.GetFileNameWithoutExtension(name).Equals(JobName, StringComparison.OrdinalIgnoreCase)
                    );

                    if (jobExists)
                    {
                        CuzMessageBox.Show(Lang.JobNameAlreadyExists, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                            string podFormat = _JobModel.PODFormat.ToString();   // Check POD format

                            if (_JobModel.CompareType == CompareType.Database && podFormat == "" || txtPODFormat.Text == "")
                            {
                                CuzMessageBox.Show(Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            if (_JobModel != null && _JobModel.CompareType == CompareType.Database && !CheckExistTemplatePrint(_JobModel.TemplatePrint) && _JobModel.PrinterSeries)
                            {
                                CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
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
                        Shared.DeleteJob(_JobModel);  // Perform delete Job file
                    }

                    _JobModel.SaveFile();
                    // END Save Job
                    // Reload Job name list
                    Shared.JobNameSelected = JobName + Shared.Settings.JobFileExtension;
                    _PODFormat.Clear();
                    listBoxPrintProductTemplate.ClearSelected();
                }

                DialogResult dialogResult = CuzMessageBox.Show(Lang.SuccessfulNewJobCreationStartTheProcess, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    if (Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings && _JobModel.CompareType == CompareType.Database && _JobModel.PrinterSeries)
                    {
                        PrinterSettingsModel printerSettingsModel = Shared.GetSettingsPrinter();

                        if (printerSettingsModel.PodDataType != 1)
                        {
                            radOther.Checked = true;
                            txtFileName.Text = "";
                            UpdateUIClearJobInformation();
                            CuzMessageBox.Show(Lang.DataTypeMustBeRAWData, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    Hide();
                    _FormMainPC?.Dispose();
                    if (_FormMainPC == null || _FormMainPC.IsDisposed)
                    {
                        _FormMainPC = new FrmMain(this);

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
                CuzMessageBox.Show(Lang.NewJobCreationFailed, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
        public void Exit()
        {
            DialogResult dialogResult = CuzMessageBox.Show(Lang.DoYouWantExitApplication, Lang.Info, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    // Cleanup Hikrobot camera SDK trước khi thoát
                    if (Shared.hikrobotCamera != null)
                    {
                        try
                        {
                            Shared.hikrobotCamera.StopSdkGrab();
                            Shared.hikrobotCamera.Disconnect();
                        }
                        catch { }
                        Shared.hikrobotCamera = null;
                    }

                    // FIX: Finalize SDK để giải phóng GigE heartbeat
                    // Không gọi → lần mở app sau camera vẫn giữ connection cũ → EnumDevices thất bại
                    HikrobotCamera.FinalizeSDK();

                    LoggingController.SaveHistory(
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
        //public void Exit()
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
                                        cameraModel.IsConnected = Shared.CamController?.IsConnected() ?? false;
                                        Shared.RaiseOnCameraStatusChangeEvent();
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
                                    case CameraType.HIKROBOT:
                                        if (Shared.hikrobotCamera == null || Shared.hikrobotCamera.CurrentIp != cameraModel.IP)
                                        {
                                            try
                                            {
                                                Shared.hikrobotCamera?.StopSdkGrab();
                                                Shared.hikrobotCamera?.Disconnect();
                                            }
                                            catch { }
                                            Shared.hikrobotCamera = null;
                                        }
                                        bool opened = false;
                                        if (Shared.hikrobotCamera == null)
                                        {
                                            // Trường hợp 1: Chưa có instance → connect mới
                                            Shared.hikrobotCamera = new HikrobotCamera();
                                            opened = Shared.hikrobotCamera.OpenCameraByIp(cameraModel.IP);
                                            if (opened)
                                            {
                                                Shared.hikrobotCamera.ObjectNameFilter = cameraModel.ObjectNameMaster ?? "";
                                                Shared.hikrobotCamera.SetTriggerMode(true);
                                                opened = Shared.hikrobotCamera.StartSdkGrab();
                                                if (!opened)
                                                {
                                                    // StartGrabbing fail → cleanup để retry lần sau
                                                    try { Shared.hikrobotCamera?.Disconnect(); } catch { }
                                                    Shared.hikrobotCamera = null;
                                                }
                                            }
                                            else
                                            {
                                                try { Shared.hikrobotCamera?.Disconnect(); } catch { }
                                                Shared.hikrobotCamera = null;
                                            }
                                        }
                                        else if (!Shared.hikrobotCamera.IsSdkOpen)
                                        {
                                            // Trường hợp 2: SDK đóng đột ngột → reconnect hoàn toàn
                                            try { Shared.hikrobotCamera?.StopSdkGrab(); Shared.hikrobotCamera?.Disconnect(); } catch { }
                                            Shared.hikrobotCamera = null;
                                            opened = false;
                                        }
                                        else if (!Shared.hikrobotCamera.IsCameraGrabbing)
                                        {
                                            // Trường hợp 3: SDK còn mở nhưng grab dừng (form đóng/mở)
                                            // → CHỈ restart grab, KHÔNG disconnect để tránh mất kết nối
                                            Shared.hikrobotCamera.SetTriggerMode(true);
                                            opened = Shared.hikrobotCamera.StartSdkGrab();
                                        }
                                        else
                                        {
                                            // Trường hợp 4: Bình thường - đang kết nối và grab
                                            opened = true;
                                        }
                                        cameraModel.IsConnected = opened;
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
                             case CameraType.HIKROBOT:
                            Shared.RaiseOnCameraReadDataChangeEvent(detectModel);
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
                                            ISMultiSyncHandler = new ISMultiSyncHandler(cameraModel.IP,"80",cameraModel.ISSlaveIP, "80");
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
                        if(!Shared.Settings.EnSerialDevice)
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

    }

}
