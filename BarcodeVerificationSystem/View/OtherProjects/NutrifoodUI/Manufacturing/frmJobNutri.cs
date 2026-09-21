using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Controller.Camera;
using BarcodeVerificationSystem.Controller.HistorySync;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
//using Mysqlx.Crud;
using BarcodeVerificationSystem.Model.Apis.Manufacturing;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using BarcodeVerificationSystem.Model.RunningMode.Dispatching;

//using Org.BouncyCastle.Asn1.Ocsp;
//using MySqlX.XDevAPI.Common;
using BarcodeVerificationSystem.Model.UserInfo;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Core;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Factories;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services.Manufacturing;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.UI;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess;
using Cognex.DataMan.SDK;
using CommonVariable;
using DesignUI.CuzAlert;
using DesignUI.CuzMesageBox;
using GenCode.Utils;
using Newtonsoft.Json;
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
using System.Windows.Forms;
using UILanguage;
using static BarcodeVerificationSystem.Controller.Shared;
using static BarcodeVerificationSystem.Model.SyncDataParams;
using Timer = System.Windows.Forms.Timer;

namespace BarcodeVerificationSystem.View.NutrifoodUI.Manufacturing
{
    public partial class frmJobNutri : Form
    {
        #region Variables Jobs
        public static readonly DMSeries DMCamera = new DMSeries();
        public ManufacturingService ManufacturingService = new ManufacturingService();

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

        PrintingQueueProcessor _printedDataProcess;
        VerificationQueueProcessor _verificationDataProcess;
        private bool _syncingFromPopup;
        private Label _lblSyncJobName;

        private FrmSettingsNuti _FormSettings;
        public JobModel _JobModel = null;
        private FrmMainNutri _FormMainPC = null;

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

        public static bool eventTwoOccurred = false;
        public static object lockObject = new object();
        public static bool isEventTwoHandled = false;
        private readonly string _endOfLineStr = "<EOF>";

        #endregion Variables Jobs

        public frmJobNutri()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitControls();
            DMCamera.InitCameraVariables();
            SetLanguage();
            InitUI();
            InitEvents();
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
                if (selectedIndex == 0)
                {
                    dgvItems.Rows.Clear();
                }

                if (selectedIndex == 1)
                {
                    PrinterSupport(radRSeries.Checked, false);
                }
                LoadJobNameList();

                if (selectedIndex == 2)
                {
                    List<string> JobNameList = Shared.GetJobNameList();
                    DisplayHistory(JobNameList);
                }
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
                //    CustomMessageBox.Show("Công việc đã hoàn thành, không thể chỉnh sửa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    string dataPath = CurrentJob.DirectoryDatabase;

                    string sentDataPath = CommVariables.PathSentDataPrinted + CurrentJob.PrintedResponePath;
                    string url = ManufacturingApis.postPrintedDataUrl();
                    if (_printedDataProcess != null) _printedDataProcess.Stop();
                    _printedDataProcess = ReliableProcessorFactory.CreatePrintingProcessor(sentDataPath, url, dataPath);
                    _printedDataProcess.Start();

                    string urlChecked = ManufacturingApis.postCheckedDataUrl();
                    if (_verificationDataProcess != null) _verificationDataProcess.Stop();
                    _verificationDataProcess = ReliableProcessorFactory.CreateVerificationProcessor(sentCheckedDataPath, urlChecked, dataPath);
                    _verificationDataProcess.Start();

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
                try
                {
                    dgvItems.Rows.Clear();

                    ResponseProcessOrder payload = await ManufacturingService.GetProcessOrderAsync(Settings.LineId, Settings.FactoryCode, Settings.RLinkName);
                    Settings.ManufacturingListPO = payload;
                    Settings.LOTFormatDate = payload.process_orders[0].batch_date_format;
                    Settings.PrintTemplate = payload.process_orders[0].print_template_name;
                    Settings.ApiDomain = payload.process_orders[0].link_web;
                    if (!Settings.ApiDomains.Contains(payload.process_orders[0].link_web))
                    {
                        Settings.ApiDomains.Add(payload.process_orders[0].link_web);
                        SaveSettings();
                    }
                    InitPOListCombo(payload);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Logger.LogError("Error occurred in get PO List btnGetInfo" + ex.Message);
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
            else if (sender == saveJobNuti)
            {
                try
                {
                    Shared.PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder);
                    if (!CheckExistTemplatePrint(Settings.PrintTemplate))
                    {
                        CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    await GeneratePOCodes();
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

                    if (!CheckExistTemplatePrint(Settings.PrintTemplate))
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
                    ProjectLogger.WriteError("Error occurred in get saveJobNutriOffline Function" + ex.Message);

                }
            }
            else if (sender == saveJobReservation)
            {
                try
                {
                    PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.Reservation);
                    if (!CheckExistTemplatePrint(Settings.PrintTemplate))
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
                    ProjectLogger.WriteError("Error occurred in get saveJobNutriOffline Function" + ex.Message);

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
                            _FormMainPC = new FrmMainNutri(this); //  // needed changed
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
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("Error occurred in get btnNext Function" + ex.Message);
                }
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

        private void InitPOListCombo(ResponseProcessOrder payload)
        {

            dgvItems.Rows.Clear();
            foreach (var data in payload.process_orders)
            {
                dgvItems.Rows.Add(data.process_order, data.plant, data.material_number, data.material_name, ""
                    , data.qty, data.qty_per_carton, data.qty / data.qty_per_carton, data.printed_count);

                var cell1 = new DataGridViewComboBoxCell();
                cell1.Items.AddRange(data.batch_info.Select(b => b.batch).ToArray());
                cell1.Value = data.batch_info.FirstOrDefault()?.batch;

                // hide arrow until editing
                cell1.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                dgvItems.Rows[dgvItems.Rows.Count - 1].Cells["material_group"] = cell1;
            }


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

            lblToolStripVersion.Text = Lang.Version + ": " + Properties.Settings.Default.SoftwareVersion;
            btnDelete.Text = Lang.Delete;
            btnHelp.Text = Lang.Help;
            btnRestart.Text = Lang.Restart;

            tabGetPO.Text = "In Loyalty"; // Lang.CreateANewJob
            tabSelectJob.Text = "Danh Sách Lệnh Sản Xuất"; //Lang.SelectJob
            tabPage3.Text = "In Reservation";
            tabSyncData.Text = "Lịch Sử Đồng Bộ"; // Lang.HistorySync
            TabCreatePOOffline.Text = "In Loyalty"; // Lang.Settings
            tabCreateRESOffline.Text = "In Reservation";
        }

        private void InitUI()
        {
            try
            {
                RESMaufDatePicker.Format = RESExpiredDatePicker.Format = MaufDatePicker.Format = ExpiredDatePicker.Format = DateTimePickerFormat.Custom;
                RESMaufDatePicker.CustomFormat = RESExpiredDatePicker.CustomFormat = ExpiredDatePicker.CustomFormat = MaufDatePicker.CustomFormat = Settings.LOTFormatDate = "yyyy/MM/dd";

                //UIControlsFuncs.VisibleControl(Settings.FactoryCode == "1260", DateTimePanel);
                cbbHisFilterType.SelectedIndex = 0;
                HistoryUtils.CustomDataGridView(dgv: dgvHistoryJob);

                SetupDataGridView();
                if (!UserPermission.isOnline)
                {
                    tabControl1.Controls.Remove(tabGetPO);
                    tabControl1.Controls.Remove(tabPage3);
                    tabControl1.Controls.Remove(tabSyncData);
                }
                else
                {
                    tabControl1.Controls.Remove(TabCreatePOOffline);
                    tabControl1.Controls.Remove(tabCreateRESOffline);
                }
                dgvItems.Rows.Clear();
            }
            catch (Exception)
            {
            }
        }

        private void InitControls()
        {

#if DEBUG
            DebugVirtual();
#endif
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
            
            // Initialize DomainInput ComboBox with ApiDomains
            InitDomainInputComboBox();
            
            _TimerDateTime.Start();
            _NameOfJobOld = "";
            Shared.JobNameSelected = "";
            var podText = new PODModel(0, "", PODModel.TypePOD.TEXT, "");
            _PODList.Add(podText);

            btnSettings.Enabled = Shared.UserPermission.Settings;
            btnDelete.Enabled = Shared.UserPermission.DeleteJob;
            tabGetPO.Enabled = Shared.UserPermission.CreateJob;

            for (int index = 1; index <= 20; index++)
            {
                var podVCD = new PODModel(index, "", PODModel.TypePOD.FIELD, "");
                _PODList.Add(podVCD);
            }

            if (ProjectLabel.IsNutrifood)
            {
                FirstRowHeader.Visible = _JobModel.IsFirstRowHeader = FirstRowHeader.Checked = false;
                UIControlsFuncs.HideControls(lblSensorControllerStatus, FirstRowHeader, btnHelp, btnAbout); // lblStatusCamera01
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

            MonitorCameraConnection();
            MonitorCameraConnection_CognexSupport();
            MonitorPrinterConnection();
            MonitorSensorControllerConnection();
            MonitorSerialDeviceControllerConnection();
            MonitorListenerServer();
        }

        private void FrmJobNutri_Shown(object sender, EventArgs e)
        {
            Shown -= FrmJobNutri_Shown;
            ShowIncompleteJobsPopup();
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
            dgvHistoryJob.Columns["MaPhieuSoanHang"].HeaderText = "Mã phiếu";
            dgvHistoryJob.Rows.Clear();
            int i = 0;
            foreach (var row in rows)
            {
                if (filter == HistoryFilter.Finished && !row.Hoanthanh)
                    continue;
                if (filter == HistoryFilter.NotFinished && row.Hoanthanh)
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
                dgvHistoryJob.Rows[rowIndex].Cells["HoanThanh"].Value = row.Hoanthanh ? "Đã Hoàn Thành" : "Chưa Hoàn Thành";

            }

            Console.WriteLine("So luong: " + _JobNameList.Count);
        }
        private void SetupDataGridView()
        {

            #region Process Order
            dgvItems.Columns.Clear();
            dgvItems.Columns.Add("process_order", "Process order");
            dgvItems.Columns.Add("plant", "Mã nhà máy");
            dgvItems.Columns.Add("material_number", "Mã sản phẩm");
            dgvItems.Columns.Add("material_name", "Tên sản phẩm");
            dgvItems.Columns.Add("material_group", "Số LOT"); // Số lượng trên thùng
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
            foreach (DataGridViewColumn column in dgvItems.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dgvItems.AutoResizeColumnHeadersHeight();

            #endregion

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
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            materialTable.AutoResizeColumnHeadersHeight();

            #endregion


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

            if (Settings.PrintTemplate == "")
            {
                CustomMessageBox.Show("Vui lòng đăng nhập tài khoản Online trước để lấy thông tin thiết bị!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "R-Link", "Database");

            if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);

            string filePath = Path.Combine(documentsPath, fileName);
            FileFuncs.WriteStringListToCsv(list, filePath); // Ensure this method is accessible
            databasePath = filePath;
            numberOfCodesGenerate = list.Count;

            txtFileName.Text = _JobModel.FileName = jobName.Text
           = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_RES" + "_" + material_doc + "_" + materialNumber + "_" +
            Settings.RLinkName + "_" + Settings.LineIndex;
            templatePrint.Text = _JobModel.TemplatePrint = Settings.PrintTemplate; //templatePrint.Text
        }

        private void GenerateCodesOfflinePO()
        {
            string materialNumber = MaterialNumber.Text;
            string process_order = InputPO.Text;
            string lotNumber = LOTNumber.Text; // Not used in this context, but kept for consistency
            int numberOfCodes = int.Parse(InputCodeNumber.Text);

            if (Settings.PrintTemplate == "")
            {
                CustomMessageBox.Show("Vui lòng đăng nhập tài khoản Online trước để lấy thông tin thiết bị!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string AskQuestion = Lang.AreYouSureGenerateDispatchingCodes +
                                  $"\nMã phiếu PO: {process_order}" +
                                  $"\nMã sản phẩm: {materialNumber}" +
                                  $"\nSố lượng mã cần tạo: {numberOfCodes}" +
                                  $"\nPhần trăm số dư: {Settings.AddQuantity}%";
            if (!CustomMessageBox.IsResultShow(AskQuestion)) return;

            bool isManufacturingMode = Settings.IsManufacturingMode;
            List<string> list;

            list = Base30AutoCodeGenerator.GenerateLineCodesForLoyalty(quantity: numberOfCodes);


            string tableName = isManufacturingMode ? "Manufacturing" : "DispatchingCodes";
            string fileName = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "R-Link", "Database");

            if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);

            string filePath = Path.Combine(documentsPath, fileName);
            FileFuncs.WriteStringListToCsv(list, filePath); // Ensure this method is accessible
            databasePath = filePath;
            numberOfCodesGenerate = list.Count;

            txtFileName.Text = _JobModel.FileName = jobName.Text
           = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_PO" + "_" + process_order + "_" + materialNumber + "_" +
            Settings.RLinkName + "_" + Settings.LineIndex;
            templatePrint.Text = _JobModel.TemplatePrint = Settings.PrintTemplate; //templatePrint.Text
        }

        private async Task GenerateReservationCodes()
        {
            try
            {
                if (materialTable.SelectedRows.Count == 0)
                {
                    CustomMessageBox.Show("Cần chọn sản phẩm!", "Chọn sản phẩm", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                txtFileName.Text = _JobModel.FileName = jobName.Text
                    = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_RES" + "_" + material_doc + "_" + materialNumber + "_" +
                     Settings.RLinkName + "_" + Settings.LineIndex;
                templatePrint.Text = _JobModel.TemplatePrint = Settings.PrintTemplate; //templatePrint.Text
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
                string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "R-Link", "Database");

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

                txtFileName.Text = _JobModel.FileName = jobName.Text
                    = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_PO" + "_" + process_order + "_" + materialNumber + "_" +
                     Settings.RLinkName + "_" + Settings.LineIndex;
                templatePrint.Text = _JobModel.TemplatePrint = Settings.PrintTemplate; //templatePrint.Text
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
                string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "R-Link", "Database");

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
                CustomMessageBox.Show("Không thể gửi dữ liệu ban đầu!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProjectLogger.WriteError($"Error occurred in {url}: " + ex.Message);
            }
            return false;

        }

        private void DisplayJobLoading(bool isLoading)
        {
            tabControl1.Enabled = !isLoading;
            picSaveJobLoading.Visible = isLoading;
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


            OnSerialDeviceReadDataChange += Shared_OnSerialDeviceReadDataChange;
            OnSyncDataParameterChange += Shared_OnSyncDataParameterChange;
            OnSyncCheckDataParameterChange += Shared_OnSyncCheckDataParameterChange;


            GetMaterialDoc.Click += ActionResult;
            btnGetInfo.Click += ActionResult;
            saveJobNuti.Click += ActionResult;
            saveJobReservation.Click += ActionResult;
            SyncDataBtn.Click += ActionResult;
            savePOOffline.Click += ActionResult;
            CreateRESOffline.Click += ActionResult;
            StopSyncData.Click += ActionResult;
            cbbHisFilterType.SelectedIndexChanged += ActionResult;
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
            txtSearchTemplate.TextChanged += TxtSearchTemplate_TextChanged;

            btnPODFormat.Click += ActionResult;

            btnSettings.Click += ActionResult;
            listBoxJobList.SelectedIndexChanged += ActionResult;
            listBoxPrintProductTemplate.SelectedIndexChanged += ActionResult;
            btnRefesh.Click += ActionResult;
            btnImportDatabase.Click += ActionResult;
            Shared.OnLanguageChange += Shared_OnLanguageChange;

            Load += FrmJob_Load;
            Shown += FrmJobNutri_Shown;
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

                int lineIndex = Settings.SelectedPOIndex = dgvItems.SelectedRows[0].Index;
                var BatchInfos = Settings?.ManufacturingListPO?.process_orders[lineIndex]?.batch_info;
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
                    if (Settings.FactoryCode == "1260")
                    {
                        InitPOListCombo(Settings.ManufacturingListPO);
                    }
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
        /// Stops the current sync (printing + verification processors). Used by popup "Dừng đồng bộ".
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
                        _lblSyncJobName.Text = string.IsNullOrEmpty(jobName) ? "" : "Đang đồng bộ: " + jobName;
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

                if (_printedDataProcess != null) _printedDataProcess.Stop();
                if (_verificationDataProcess != null) _verificationDataProcess.Stop();

                string dataPath = job.DirectoryDatabase;
                string sentDataPath = CommVariables.PathSentDataPrinted + job.PrintedResponePath;
                string url = ManufacturingApis.postPrintedDataUrl();
                _printedDataProcess = ReliableProcessorFactory.CreatePrintingProcessor(sentDataPath, url, dataPath);
                _printedDataProcess.Start();

                string urlChecked = ManufacturingApis.postCheckedDataUrl();
                _verificationDataProcess = ReliableProcessorFactory.CreateVerificationProcessor(sentCheckedDataPath, urlChecked, dataPath);
                _verificationDataProcess.Start();

                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    if (CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes && (NumberChecked == 0 || NumberOfCheckSentSuccess == NumberChecked))
                        break;
                    await Task.Delay(300, ct);
                }

                _printedDataProcess?.Stop();
                _verificationDataProcess?.Stop();
                return true;
            }
            catch (Exception)
            {
                _printedDataProcess?.Stop();
                _verificationDataProcess?.Stop();
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

            if (Shared.PrintMode.IsPrintingMode && UserPermission.isOnline)
            {
                job.DispatchingOrderPayload = _JobModel.DispatchingOrderPayload;
                int lineIndex = dgvItems.SelectedRows[0].Index;
                job.SelectedMaterialIndex = lineIndex;
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
                _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
                _JobModel.TemplatePrint = Settings.PrintTemplate;
                _JobModel.SelectedBatchIndex = Settings.SelectedBatchIndex;
                _JobModel.isPushedDatabase = isPushDatabase;

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

                            _JobModel.TemplatePrint = templatePrint.Text;

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
                        Shared.DeleteJob(_JobModel);  // Perform delete Job file
                    }

                    _JobModel.SaveFile();

                    Shared.JobNameSelected = JobName + Shared.Settings.JobFileExtension;
                    _PODFormat.Clear();
                    listBoxPrintProductTemplate.ClearSelected();
                }

                //DialogResult dialogResult = CuzMessageBox.Show(Lang.SuccessfulNewJobCreationStartTheProcess, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                DialogResult dialogResult = DialogResult.Yes;
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
                        _FormMainPC = new FrmMainNutri(this);  // needed changed

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
                    LoggingController.SaveHistory(  //Save history
                        Lang.Exit,
                        Lang.LogOut,
                        Lang.LogoutSuccessfully,
                        SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                        LoggingType.LogedOut);

                    try
                    {
                        ApiService apiService = new ApiService();
                        await MonitorSenderService.sendParametersToServerAsync(apiService, false);
                    }
                    catch (Exception) { }

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
                                    bool checkPort2 = Shared.SensorController.Port2 == Shared.Settings.SensorControllerPort2;
                                    if (!checkPort2)
                                    {
                                        Shared.SensorController.Disconnect();
                                        Shared.SensorController = null;
                                    }
                                    if (!SensorController.IsConnected2() && checkPort2)
                                    {
                                        SensorController.Connect2();
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
                CuzMessageBox.Show("Gửi dữ liệu ban đầu thành công!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                CuzMessageBox.Show("Gửi dữ liệu ban đầu thấy bại!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
