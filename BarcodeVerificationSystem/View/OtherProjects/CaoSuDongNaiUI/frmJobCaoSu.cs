using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Controller.Camera;
using BarcodeVerificationSystem.Controller.HistorySync;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Apis.CaoSuDongNai;
using BarcodeVerificationSystem.Model.Apis.Manufacturing;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Response;
using BarcodeVerificationSystem.Model.Droco;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using BarcodeVerificationSystem.Model.RunningMode.Dispatching;
using BarcodeVerificationSystem.Model.UserInfo;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Core;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Factories;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services.CaoSuDongNai;
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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq;
using OperationLog.Controller;
using OperationLog.Model;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml.Serialization;
using UILanguage;
using static BarcodeVerificationSystem.Controller.Shared;
using static BarcodeVerificationSystem.Model.SyncDataParams;
using Timer = System.Windows.Forms.Timer;

namespace BarcodeVerificationSystem.View.CaoSuDongNaiUI
{
    public partial class frmJobCaoSu : Form
    {
        #region Variables Jobs
        public static readonly DMSeries DMCamera = new DMSeries();
        public ManufacturingService ManufacturingService = new ManufacturingService();
        CaoSuApiService _caoSuApiService = new CaoSuApiService();

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
        VerificationQueueProcessor _palletDataProcess;

        private FrmSettingsCaoSu _FormSettings;
        public JobModel _JobModel = null;
        private FrmMainCaoSu _FormMainPC = null;

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

        private ApiService apiService = new ApiService();

        #endregion Variables Jobs

        public frmJobCaoSu()
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

                if (selectedIndex == 1)
                {
                    PrinterSupport(radRSeries.Checked, false);
                }
                LoadJobNameList();

                if (selectedIndex == 1)
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
                DisplayHistoryByFilter();
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
            else if (sender == btnAddWeight || sender == btnAddWeight1)
            {
                try
                {
                    string input = WeightInput.Text.Trim();

                    if (!double.TryParse(input, out double weight) || weight <= 0)
                    {
                        CustomMessageBox.Show("Vui lòng nhập trọng lượng là số dương!",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dgvItems.SelectedRows == null || dgvItems.SelectedRows.Count == 0)
                    {
                        CustomMessageBox.Show("Vui lòng chọn một sản phẩm!",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int index = dgvItems.SelectedRows[0].Index;

                    // Thêm vào data source model
                    Settings.CaoSuProductList.data.data[index].listweight.Add(new ListWeight
                    {
                        weight = weight
                    });

                    // Cập nhật DataSource của ComboBox cell — giữ nguyên giá trị đang chọn
                    var comboCell = dgvItems.Rows[index].Cells["listweight"] as DataGridViewComboBoxCell;
                    if (comboCell != null)
                    {
                        // Lưu giá trị đang chọn trước khi rebuild
                        string currentValue = comboCell.Value?.ToString();

                        // Rebuild danh sách weight từ model
                        var updatedWeights = Settings.CaoSuProductList.data.data[index]
                            .listweight.Select(w => w.weight.ToString()).ToList();

                        comboCell.DataSource = updatedWeights;

                        // Khôi phục giá trị đang chọn (không đổi sang weight mới)
                        if (!string.IsNullOrEmpty(currentValue) && updatedWeights.Contains(currentValue))
                        {
                            comboCell.Value = currentValue;
                        }
                    }

                    WeightInput.Text = "";
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("Error in btnAddWeight: " + ex.Message);
                }
            }
            else if (sender == SyncDataBtn)
            {
                try
                {
                    if (dgvHistoryJob.SelectedRows.Count == 0)
                    {
                        CustomMessageBox.Show("Vui lòng chọn một công việc để đồng bộ!",
                            Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int filterIndex = cbbHisFilterType.SelectedIndex < 0 ? 0 : cbbHisFilterType.SelectedIndex;

                    // ── Xác định row được chọn là JobCheck hay JobPrint ───────
                    int selectedRowIndex = dgvHistoryJob.SelectedRows[0].Index;
                    string selectedJobName = dgvHistoryJob.Rows[selectedRowIndex]
                        .Cells["MaCongViec"]?.Value?.ToString() ?? "";

                    if (string.IsNullOrWhiteSpace(selectedJobName))
                    {
                        CustomMessageBox.Show("Không đọc được tên công việc!",
                            Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string fileExt = Shared.Settings.JobFileExtension ?? ".rvis";

                    // Kiểm tra file nằm ở đâu để xác định loại job
                    string checkPath = Path.Combine(
                        CommVariables.PathJobsCheckApp.TrimEnd('\\'),
                        selectedJobName.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase)
                            ? selectedJobName : selectedJobName + fileExt);

                    string printPath = Path.Combine(
                        CommVariables.PathJobsApp.TrimEnd('\\'),
                        selectedJobName.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase)
                            ? selectedJobName : selectedJobName + fileExt);

                    bool isJobCheck = File.Exists(checkPath);
                    bool isJobPrint = File.Exists(printPath) && !isJobCheck;

                    // ── JobCheck → SyncJobCheckOfflineBatchAsync ──────────────
                    if (isJobCheck)
                    {
                        await SyncJobCheckOfflineBatchAsync();
                        return;
                    }

                    // ── JobPrint → API push database + update printed ─────────
                    if (isJobPrint)
                    {
                        StartUISyncData();
                        try
                        {
                            int successCount = 0;
                            int failCount = 0;

                            var job = JobModel.LoadFile(printPath);
                            if (job == null)
                            {
                                CustomMessageBox.Show("Không thể đọc file JobPrint!",
                                    Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            var allProc = new CaoSuAllValueProcess(job);
                            var allPayload = allProc.GetAllValuePayload();
                            if (allPayload?.qr_list == null || allPayload.qr_list.Count == 0)
                            {
                                CustomMessageBox.Show("Không có dữ liệu QR để đồng bộ!",
                                    Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // API 1: push database
                            bool api1Success = false;
                            try
                            {
                                var pushRequest = new RequestPushDatabase
                                {
                                    production_batch_code = job.LOTNumber ?? "",
                                    product_code = !string.IsNullOrEmpty(job.PrintJobProductCode)
                                                                ? job.PrintJobProductCode
                                                                : (job.CaoSuProduct?.product_code
                                                                   ?? job.LastCheckedProductCode ?? ""),
                                    weight = job.productWeight,
                                    printed_number = allPayload.qr_list.Count,
                                    execute_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    qr_code = allPayload.qr_list.Select(qr => new DatabaseQrCode
                                    {
                                        qrcode_value = qr.qrcode_value,
                                        index_in_lot = qr.index_in_lot,
                                        execute_date = !string.IsNullOrWhiteSpace(qr.created_time)
                                                        ? qr.created_time
                                                        : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    }).ToList()
                                };
                                var pushResp = await _caoSuApiService.PostPushDatabaseAsync(pushRequest);
                                if (pushResp?.success == true) api1Success = true;
                                else
                                {
                                    failCount++;
                                    ProjectLogger.WriteError($"[SyncJobPrint] API1 failed: {pushResp?.message}");
                                }
                            }
                            catch (Exception ex)
                            {
                                failCount++;
                                ProjectLogger.WriteError($"[SyncJobPrint] API1 exception: {ex.Message}");
                            }

                            // API 2: update status to printed
                            if (api1Success)
                            {
                                try
                                {
                                    var printedCodes = allProc.GetPrintedQRCodes();
                                    if (printedCodes?.Count > 0)
                                    {
                                        var printRequest = new RequestUpdateCodePrint
                                        {
                                            qr_list = printedCodes.Select(s =>
                                            {
                                                var parts = s.Split(',');
                                                return new QrCodePrint
                                                {
                                                    qrcode_value = parts.Length > 0 ? parts[0] : "",
                                                    execute_date = parts.Length > 1 ? parts[1]
                                                                   : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                                };
                                            }).ToList()
                                        };
                                        var printResp = await _caoSuApiService.PostCodePrintAsync(printRequest);
                                        if (printResp?.success == true)
                                        {
                                            try
                                            {
                                                string sentDir = CommVariables.PathSentDataPrinted;
                                                if (!Directory.Exists(sentDir))
                                                    Directory.CreateDirectory(sentDir);
                                                string sentFilePath = Path.Combine(sentDir,
                                                    job.PrintedResponePath ?? (job.FileName + ".csv"));
                                                int qi = 1;
                                                var sentLines = printRequest.qr_list.Select(qr =>
                                                    $"{qi++},{qr.qrcode_value},,{qr.execute_date},success,,,,");
                                                File.AppendAllLines(sentFilePath, sentLines, Encoding.UTF8);
                                            }
                                            catch (Exception ex)
                                            {
                                                ProjectLogger.WriteError($"[SyncJobPrint] Write SentData: {ex.Message}");
                                            }
                                        }
                                        else
                                        {
                                            ProjectLogger.WriteError($"[SyncJobPrint] API2 failed: {printResp?.message}");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ProjectLogger.WriteError($"[SyncJobPrint] API2 exception: {ex.Message}");
                                }
                                try { job.SaveFile(); } catch { }
                                successCount++;
                            }

                            // ── Refresh giữ nguyên filter đang chọn ──────────
                            DisplayHistoryByFilter();
                            CustomMessageBox.Show(
                                $"Đồng bộ JobPrint hoàn tất.\nThành công: {successCount} | Thất bại: {failCount}",
                                Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteError("Sync JobPrint error: " + ex.Message);
                            CustomMessageBox.Show("Lỗi khi đồng bộ JobPrint.\n" + ex.Message,
                                Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally { StopUISyncData(); }
                        return;
                    }

                    // ── Không tìm thấy file → báo lỗi ────────────────────────
                    CustomMessageBox.Show(
                        $"Không tìm thấy file job:\n{selectedJobName}\n\nVui lòng kiểm tra lại!",
                        Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show("Lỗi đồng bộ!\n" + ex.Message,
                        Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ProjectLogger.WriteError("SyncDataBtn error: " + ex.Message);
                    StopUISyncData();
                }
            }

            //else if (sender == SyncDataBtn)
            //{
            //    try
            //    {
            //        if (dgvHistoryJob.SelectedRows.Count == 0)
            //        {
            //            CustomMessageBox.Show("Vui lòng chọn một công việc để đồng bộ!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //            return;
            //        }

            //        if (cbbHisFilterType.SelectedItem != null
            //            && cbbHisFilterType.SelectedItem.ToString().IndexOf("PrintJob", StringComparison.OrdinalIgnoreCase) >= 0)
            //        {
            //            StartUISyncData();
            //            try
            //            {
            //                int successCount = 0;
            //                int failCount = 0;

            //                for (int r = 0; r < dgvHistoryJob.Rows.Count; r++)
            //                {
            //                    try
            //                    {
            //                        var cell = dgvHistoryJob.Rows[r].Cells["MaCongViec"];
            //                        if (cell == null) continue;
            //                        string jobName = cell.Value?.ToString();
            //                        if (string.IsNullOrWhiteSpace(jobName)) continue;

            //                        string fileExt1 = Shared.Settings.JobFileExtension ?? ".rvis";
            //                        string jobPath = Path.Combine(
            //                            CommVariables.PathJobsApp.TrimEnd('\\'),
            //                            jobName.EndsWith(fileExt1, StringComparison.OrdinalIgnoreCase)
            //                                ? jobName : jobName + fileExt1);

            //                        if (!File.Exists(jobPath)) continue;

            //                        var job = JobModel.LoadFile(jobPath);
            //                        if (job == null) continue;

            //                        var allProc = new CaoSuAllValueProcess(job);
            //                        var allPayload = allProc.GetAllValuePayload();
            //                        if (allPayload.qr_list == null || allPayload.qr_list.Count == 0) continue;

            //                        // ── API 1: /check-sync/production-batch ──────────────
            //                        // production_batch_code, product_code, weight đều có thể rỗng/0
            //                        // server chấp nhận — chỉ qr_code là bắt buộc
            //                        bool api1Success = false;
            //                        try
            //                        {
            //                            var pushRequest = new RequestPushDatabase
            //                            {
            //                                production_batch_code = job.LOTNumber ?? "",
            //                                product_code = !string.IsNullOrEmpty(job.PrintJobProductCode)
            //                                                            ? job.PrintJobProductCode
            //                                                            : (job.CaoSuProduct?.product_code
            //                                                                ?? job.LastCheckedProductCode
            //                                                                ?? ""),
            //                                weight = job.productWeight,
            //                                printed_number = allPayload.qr_list.Count,
            //                                execute_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //                                qr_code = allPayload.qr_list.Select(qr => new DatabaseQrCode
            //                                {
            //                                    qrcode_value = qr.qrcode_value,
            //                                    index_in_lot = qr.index_in_lot,
            //                                    execute_date = !string.IsNullOrWhiteSpace(qr.created_time)
            //                                                    ? qr.created_time
            //                                                    : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            //                                }).ToList()
            //                            };

            //                            var pushResp = await _caoSuApiService.PostPushDatabaseAsync(pushRequest);
            //                            if (pushResp != null && pushResp.success)
            //                            {
            //                                api1Success = true;
            //                            }
            //                            else
            //                            {
            //                                failCount++;
            //                                ProjectLogger.WriteError($"[SyncJobPrint] API1 failed for {jobName}: {pushResp?.message}");
            //                                continue;
            //                            }
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            failCount++;
            //                            ProjectLogger.WriteError($"[SyncJobPrint] API1 exception for {jobName}: {ex.Message}");
            //                            continue;
            //                        }

            //                        // ── API 2: /check-sync/update-status-to-printed ──────
            //                        // Chỉ gọi khi API 1 thành công
            //                        if (api1Success)
            //                        {
            //                            try
            //                            {
            //                                var printedCodes = allProc.GetPrintedQRCodes();
            //                                if (printedCodes != null && printedCodes.Count > 0)
            //                                {
            //                                    var printRequest = new RequestUpdateCodePrint
            //                                    {
            //                                        qr_list = printedCodes.Select(s =>
            //                                        {
            //                                            var parts = s.Split(',');
            //                                            return new QrCodePrint
            //                                            {
            //                                                qrcode_value = parts.Length > 0 ? parts[0] : "",
            //                                                execute_date = parts.Length > 1 ? parts[1]
            //                                                    : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            //                                            };
            //                                        }).ToList()
            //                                    };

            //                                    var printResp = await _caoSuApiService.PostCodePrintAsync(printRequest);
            //                                    if (printResp != null && printResp.success)
            //                                    {
            //                                        // Ghi kết quả vào PathSentDataPrinted
            //                                        // Format: Index,QrCode,UniqueCode,PrintedDate,SaaSStatus,SAPStatus,SaaSError,SAPError,SentStatus
            //                                        try
            //                                        {
            //                                            string sentDir = CommVariables.PathSentDataPrinted;
            //                                            if (!Directory.Exists(sentDir))
            //                                                Directory.CreateDirectory(sentDir);

            //                                            string sentFilePath = Path.Combine(sentDir, job.PrintedResponePath ?? (job.FileName + ".csv"));
            //                                            var sentLines = new List<string>();
            //                                            int idx = 1;
            //                                            foreach (var qr in printRequest.qr_list)
            //                                            {
            //                                                // Index,QrCode,UniqueCode,PrintedDate,SaaSStatus,SAPStatus,SaaSError,SAPError,SentStatus
            //                                                sentLines.Add($"{idx},{qr.qrcode_value},,{qr.execute_date},success,,,,");
            //                                                idx++;
            //                                            }
            //                                            File.AppendAllLines(sentFilePath, sentLines, Encoding.UTF8);
            //                                        }
            //                                        catch (Exception ex)
            //                                        {
            //                                            ProjectLogger.WriteError($"[SyncJobPrint] Write SentDataPrinted error: {ex.Message}");
            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        ProjectLogger.WriteError($"[SyncJobPrint] API2 failed for {jobName}: {printResp?.message}");
            //                                    }
            //                                }
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                ProjectLogger.WriteError($"[SyncJobPrint] API2 exception for {jobName}: {ex.Message}");
            //                            }

            //                            try
            //                            {
            //                                job.NumberOfSaaSSentCodes = allPayload.qr_list
            //                                    .Count(q => !string.IsNullOrWhiteSpace(q.printed_at));
            //                                job.SaveFile();
            //                            }
            //                            catch { }

            //                            successCount++;
            //                        }

            //                        // Refresh row UI
            //                        try
            //                        {
            //                            int quantity = job.PrintJobWeightRange == ">30" ? 144 : 240;
            //                            dgvHistoryJob.Rows[r].Cells["SoLuongCanXuat"].Value = job.NumberOfSaaSSentCodes;
            //                            dgvHistoryJob.Rows[r].Cells["SoLuongDongBoSaaS"].Value = job.NumberOfCheckSaaSSentCodes;
            //                            dgvHistoryJob.Rows[r].Cells["SoLuongDongBoSAP"].Value = $"{job.NumberOfCodesInPallet} / {quantity}";
            //                            dgvHistoryJob.Rows[r].Cells["HoanThanh"].Value =
            //                                job.NumberOfCodesInPallet == quantity ? "Đã Hoàn Thành" : "Chưa Hoàn Thành";
            //                        }
            //                        catch { }

            //                        await Task.Delay(150);
            //                    }
            //                    catch (Exception exRow)
            //                    {
            //                        ProjectLogger.WriteError($"[SyncJobPrint] Row error: {exRow.Message}");
            //                    }
            //                }

            //                LoadJobNameList();
            //                DisplayHistory_PrintJobOffline();
            //                CustomMessageBox.Show(
            //                    $"Đồng bộ JobPrint Offline hoàn tất.\nThành công: {successCount} | Thất bại: {failCount}",
            //                    Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //            }
            //            catch (Exception ex)
            //            {
            //                ProjectLogger.WriteError("Batch sync JobPrint Offline error: " + ex.Message);
            //                CustomMessageBox.Show("Lỗi khi đồng bộ JobPrint Offline.\n" + ex.Message,
            //                    Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            }
            //            finally
            //            {
            //                StopUISyncData();
            //            }
            //            return;
            //        }
            //    }
            //    catch (Exception ex)  // ← catch bị thiếu ở đây
            //    {
            //        CustomMessageBox.Show("Lỗi đồng bộ!\n" + ex.Message, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        ProjectLogger.WriteError("SyncDataBtn error: " + ex.Message);
            //        StopUISyncData();
            //    }
            //}

            else if (sender == StopSyncData)
            {
                bool isOnline = CurrentJob != null && CurrentJob.IsJobOnline;
                (isOnline ? (Action)OnSentPrintedCodesCompleted : StopUISyncData)();
            }
            else if (sender == btnUpdateProducts)
            {
                try
                {
                    dgvItems.Rows.Clear();
                    GetAllProducts();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Logger.LogError("Error occurred in get PO List btnGetInfo" + ex.Message);
                }


            }
            else if (sender == saveJobCaoSu)
            {
                try
                {
                    Shared.PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder);
                    if (!CheckExistTemplatePrint(Settings.PrintTemplate))
                    {
                        CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    //if(InputLotNumber.Text.Trim().Length != 8)
                    //{
                    //    CuzMessageBox.Show("Số LOT cần phải có 8 ký tự! Vui lòng nhập lại!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    return;
                    //}

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
                    InputLotNumber.Text = "";
                }
                catch (Exception ex)
                {
                    DisplayJobLoading(false); ;
                    CustomMessageBox.Show($"Không thể tạo phiếu soạn hàng!", Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ProjectLogger.WriteError("Error occurred in get saveJob Function" + ex.Message);

                }

            }
            else if (sender == saveJobCaoSuPrint)
            {
                try
                {
                    Shared.PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder);
                    if (!CheckExistTemplatePrint(Settings.PrintTemplate))
                    {
                        CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    //if (dgvItems.SelectedRows == null || dgvItems.SelectedRows.Count == 0)
                    //{
                    //    CustomMessageBox.Show("Vui lòng chọn một sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}

                    // Chỉ cần chọn điều kiện để xác định số lượng mã
                    string weightText = cbcWeight.Text.Trim();
                    if (weightText != "<=30" && weightText != ">30")
                    {
                        CustomMessageBox.Show("Vui lòng chọn loại trọng lượng (<=30 hoặc >30)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // KHÔNG gán CaoSuProduct, KHÔNG gán productWeight
                    await GeneratePrintCodesSendWeightOnly();

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
                    InputLotNumber.Text = "";
                    cbcWeight.Text = "";
                }
                catch (Exception ex)
                {
                    DisplayJobLoading(false);
                    CustomMessageBox.Show($"Không thể tạo phiếu soạn hàng!", Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ProjectLogger.WriteError("Error occurred in saveJobCaoSuPrint: " + ex.Message);
                }
            }
            else if (sender == saveJobCaoSuCheck)
            {
                try
                {
                    Shared.PrintMode.SetPrintingMode(PrintingMode.PrintingModeLabel.ProcessOrder);

                    if (InputLotNumber.Text.Trim().Length != 8)
                    {
                        CuzMessageBox.Show("Số LOT cần phải có 8 ký tự! Vui lòng nhập lại!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        InputLotNumber.Focus();
                        return;
                    }
                    if (dgvListJobPrint.SelectedItems.Count == 0)
                    {
                        CustomMessageBox.Show("Vui lòng chọn một JobPrint trong danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (dgvItems.SelectedRows == null || dgvItems.SelectedRows.Count == 0)
                    {
                        CustomMessageBox.Show("Vui lòng chọn một Sản phẩm trong danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Force commit ComboBox cell trước khi đọc
                    if (dgvItems.IsCurrentCellDirty)
                        dgvItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    dgvItems.EndEdit();

                    // Đọc weight từ cột listweight của hàng đang chọn
                
                    int rowIndex = dgvItems.SelectedRows[0].Index;
                    var weightCellValue = dgvItems.Rows[rowIndex].Cells["listweight"].Value;

                    if (weightCellValue == null
                        || !double.TryParse(weightCellValue.ToString(), out double selectedWeight)
                        || selectedWeight <= 0)
                    {
                        CustomMessageBox.Show("Vui lòng chọn trọng lượng từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string selectedJobPrint = dgvListJobPrint.SelectedItems[0].ToString();

                    // Kiểm tra bành lớn
                    // Kiểm tra bành lớn: nếu trọng lượng <= 30 mà JobPrint là bành lớn (> 144 mã)
                    // ── Ràng buộc: trọng lượng phải khớp loại bành của JobPrint ─────
                    // weight > 30kg → bành nhỏ (144 mã)
                    // weight <= 30kg → bành lớn (240 mã)
                    JobModel jobPrintModel = Shared.GetJob(selectedJobPrint);
                    if (jobPrintModel != null)
                    {
                        // Xác định loại bành của JobPrint theo thứ tự ưu tiên:
                        // 1. NumberTotalsCode (số mã kế hoạch) — chính xác nhất
                        // 2. PrintJobWeightRange (">30" / "<=30")
                        // 3. productWeight (trọng lượng lưu trong job)
                        int jobTotalCodes = 0;
                        if (jobPrintModel.NumberTotalsCode > 0)
                            jobTotalCodes = (int)jobPrintModel.NumberTotalsCode;
                        else if (!string.IsNullOrWhiteSpace(jobPrintModel.PrintJobWeightRange))
                            jobTotalCodes = jobPrintModel.PrintJobWeightRange
                                .Equals(">30", StringComparison.OrdinalIgnoreCase) ? 144 : 240;
                        else if (jobPrintModel.productWeight > 0)
                            jobTotalCodes = jobPrintModel.productWeight > 30 ? 144 : 240;
                        if (jobTotalCodes > 0)
                        {
                            // Normalize ±1 phòng trường hợp IsFirstRowHeader offset
                            // 144 hoặc 145 → bành lớn (weight > 30)
                            // 240 hoặc 241 → bành nhỏ (weight <= 30)
                            bool jobIsLarge = jobTotalCodes == 144 || jobTotalCodes == 145;
                            bool jobIsSmall = jobTotalCodes == 240 || jobTotalCodes == 241;

                            bool needLarge = selectedWeight > 30;

                            if ((needLarge && !jobIsLarge) || (!needLarge && !jobIsSmall))
                            {
                                string weightLabel = selectedWeight > 30 ? "> 30" : "<= 30";
                                string expectedType = selectedWeight > 30 ? "lớn (144 mã)" : "nhỏ (240 mã)";
                                string actualType = jobIsLarge ? "lớn (144 mã)"
                                                    : jobIsSmall ? "nhỏ (240 mã)"
                                                    : $"không xác định ({jobTotalCodes} mã)";
                                CustomMessageBox.Show(
                                    $"Trọng lượng {selectedWeight}kg ({weightLabel}kg) yêu cầu chọn JobPrint bành {expectedType}.\n" +
                                    $"JobPrint đang chọn là bành {actualType}.\n\n" +
                                    $"Vui lòng chọn đúng loại JobPrint!",
                                    "Không khớp loại bành", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    string lotNumber = InputLotNumber.Text.Trim();
                    string productCode = "";
                    string productName = "";

                    var selectedRow = dgvItems.SelectedRows[0];
                    if (selectedRow.Cells["product_code"].Value != null)
                        productCode = selectedRow.Cells["product_code"].Value.ToString().Trim();
                    if (selectedRow.Cells["product_name"].Value != null)
                        productName = selectedRow.Cells["product_name"].Value.ToString().Trim();
             

                    // ── Hiển thị xác nhận trước khi tạo JobCheck ─────────────
                    string confirmMsg =
                        $"Bạn có chắc muốn tạo JobCheck không?\n\n" +
                        $"JobPrint  : {selectedJobPrint}\n" +
                        $"LOT       : {lotNumber}\n" +
                        $"Sản phẩm  : {productCode} - {productName}\n" +
                        $"Trọng lượng: {selectedWeight} kg";

                    DialogResult confirm = CustomMessageBox.Show(
                        confirmMsg,
                        "Xác nhận tạo JobCheck",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirm != DialogResult.Yes)
                        return;
                    _PODFormat.Clear();
                    txtPODFormat.Text = "";
                    Shared.databasePath = "";
                    DisplayJobLoading(false);

                    bool created = CreateJobCheck(lotNumber, productCode, productName, selectedWeight);
                    if (created)
                        InputLotNumber.Text = "";
                    LoadJobNameList();
                }
                catch (Exception ex)
                {
                    DisplayJobLoading(false);
                    CustomMessageBox.Show("Không thể tạo phiếu soạn hàng!", Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ProjectLogger.WriteError("Error occurred in saveJobCaoSuCheck: " + ex.Message);
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
                    _FormSettings = new FrmSettingsCaoSu();
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
                        if (!_JobModel.IsJobOnline && UserPermission.isOnline)
                        {
                            CuzMessageBox.Show("Vui lòng vào tài khoản Offline để tiếp tục!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        if (_JobModel.IsJobOnline && !UserPermission.isOnline)
                        {
                            CuzMessageBox.Show("Vui lòng vào tài khoản Onlline để tiếp tục!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
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
                            _FormMainPC = new FrmMainCaoSu(this); //  // needed changed
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

            else if (sender == btnNextCheck)
            {
                try
                {
                    




                    string fileName = dgvListJobCheck.SelectedItems[0].ToString();
                    string fileExt = Shared.Settings.JobFileExtension;
                    if (string.IsNullOrWhiteSpace(fileExt)) fileExt = ".rvis";

                    string jobCheckFilePath = Path.Combine(
                        CommVariables.PathJobsCheckApp.TrimEnd('\\'),
                        fileName.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase)
                            ? fileName
                            : fileName + fileExt);


                    JobModel jobCheck = JobModel.LoadFile(jobCheckFilePath);
                    if (jobCheck == null)
                    {
                        MessageBox.Show("Không thể đọc dữ liệu JobCheck!", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Kiểm tra trạng thái online/offline của jobCheck
                    if (!jobCheck.IsJobOnline && UserPermission.isOnline)
                    {
                        CuzMessageBox.Show("Vui lòng vào tài khoản Offline để tiếp tục!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (jobCheck.IsJobOnline && !UserPermission.isOnline)
                    {
                        CuzMessageBox.Show("Vui lòng vào tài khoản Online để tiếp tục!", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (!File.Exists(jobCheckFilePath))
                    {
                        MessageBox.Show("Không tìm thấy file JobCheck!\nPath: " + jobCheckFilePath, "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

               
                   

                    string lotNumber = jobCheck.LOTNumber ?? jobCheck.LastCheckedLot ?? "";
                    string productCode = jobCheck.LastCheckedProductCode ?? "";

                    var frmCheck = new FrmMainCaoSuCheck(
                        this,
                        jobCheck,
                        lotNumber,
                        productCode,
                        jobCheckFilePath
                    );
                    frmCheck.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi mở JobCheck:\n" + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ProjectLogger.WriteError("Error in btnNextCheck: " + ex.Message + "\n" + ex.StackTrace);
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

        static bool isSyncingData = false;
        private void StartUISyncData()
        {
            isSyncingData = true;
            UIControlsFuncs.ShowControls(picDataLoading);
            UIControlsFuncs.DisableAllTabsSelection(tabControl1);
            UIControlsFuncs.DisableControls(dgvHistoryJob, pnlMenu, SyncDataBtn, cbbHisFilterType);
        }

        //private void StopUISyncData()
        //{
        //    isSyncingData = false;
        //    DisplayHistory(GetJobNameList());
        //    UIControlsFuncs.HideControls(picDataLoading);
        //    UIControlsFuncs.EnableAllTabsSelection(tabControl1);
        //    UIControlsFuncs.EnableControls(dgvHistoryJob, pnlMenu, SyncDataBtn, cbbHisFilterType);
        //}
        private void StopUISyncData()
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired) { BeginInvoke(new Action(StopUISyncData)); return; }

            try
            {
                isSyncingData = false;
                UIControlsFuncs.HideControls(picDataLoading);
                UIControlsFuncs.EnableAllTabsSelection(tabControl1);
                UIControlsFuncs.EnableControls(dgvHistoryJob, pnlMenu, SyncDataBtn, cbbHisFilterType);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("StopUISyncData UI error: " + ex.Message);
            }

            try
            {
                DisplayHistoryByFilter();
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("StopUISyncData DisplayHistory error: " + ex.Message);
            }
        }

        //private void InitPOListCombo(ResponseAllProducts payload, string TypedWeight = "", int newRowIndex = 0)
        //{
        //    dgvItems.Rows.Clear();
        //    if (payload == null || payload.data == null || payload.data.data == null) return;

        //    foreach (var data in payload.data.data)
        //    {
        //        int rowIndex = dgvItems.Rows.Add(data.product_code, data.product_name, data.size);
        //        // Nếu có cột listweight, bạn có thể xử lý thêm ở đây
        //    }

        //}
        private void InitPOListCombo(ResponseAllProducts payload, string TypedWeight = "", int newRowIndex = 0)
        {
            // 1. Kiểm tra an toàn: payload hoặc payload.data bị null
            if (payload == null || payload.data == null || payload.data.data == null)
            {
                ProjectLogger.WriteError("InitPOListCombo: Payload hoặc danh sách dữ liệu bị null.");
                return;
            }

            // 2. Kiểm tra DataGridView có tồn tại không
            if (dgvItems == null) return;

            try
            {
                dgvItems.Rows.Clear();
                int i = 0;

                foreach (var data in payload.data.data)
                {
                    // Kiểm tra từng item trong vòng lặp
                    if (data == null) continue;

                    // Thêm dòng mới
                    int rowIndex = dgvItems.Rows.Add(data.product_code, data.product_name, data.size, "");

                    // 3. Lấy cell ComboBoxColumn
                    // Lưu ý: Đảm bảo tên cột "listweight" khớp chính xác với tên bạn đặt trong Designer
                    var comboCell = dgvItems.Rows[rowIndex].Cells["listweight"] as DataGridViewComboBoxCell;

                    if (comboCell != null)
                    {
                        comboCell.Items.Clear();

                        // Kiểm tra danh sách cân nặng (listweight) bên trong sản phẩm
                        if (data.listweight != null && data.listweight.Any())
                        {
                            var weights = data.listweight
                                .Where(w => w != null)
                                .Select(w => w.weight.ToString())
                                .ToList();

                            if (weights.Count > 0)
                            {
                                comboCell.Items.AddRange(weights.ToArray());

                                // Xác định giá trị mặc định được chọn
                                string selectedWeight = (TypedWeight != "" && newRowIndex == i)
                                    ? TypedWeight
                                    : weights.FirstOrDefault();

                                if (!string.IsNullOrEmpty(selectedWeight) && comboCell.Items.Contains(selectedWeight))
                                {
                                    comboCell.Value = selectedWeight;
                                }
                            }
                        }
                    }

                    i++;
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Lỗi thực thi InitPOListCombo: " + ex.Message);
            }
        }
        private void OnSentPrintedCodesCompleted()
        {
            try
            {
                if (_printedDataProcess != null)
                    _printedDataProcess.Stop();

                if (_verificationDataProcess != null)
                    _verificationDataProcess.Stop();

                if (_palletDataProcess != null)
                    _palletDataProcess.Stop();

                // Đảm bảo cập nhật UI trên UI thread
                Action updateUI = () =>
                {
                    UIControlsFuncs.HideControls(picDataLoading);
                    UIControlsFuncs.EnableAllTabsSelection(tabControl1);
                    UIControlsFuncs.EnableControls(dgvHistoryJob, pnlMenu, SyncDataBtn, cbbHisFilterType);
                    isSyncingData = false;
                    DisplayHistoryByFilter(); // ← giữ nguyên filter + selection
                };

                if (InvokeRequired)
                    BeginInvoke(updateUI);
                else
                    updateUI();
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("OnSentPrintedCodesCompleted error: " + ex.Message);
            }
        }

        private void Shared_OnSyncDataParameterChange(object sender, EventArgs e)
        {
            try
            {
                if (CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSaaSSentCodes && (CurrentJob.NumberOfCheckSaaSSentCodes == NumberChecked))
                {
                    OnSentPrintedCodesCompleted();
                }

            }
            catch (Exception ex)
            {
            }

        }
     

        // Pull data from server (/check-sync/production-batch) and update local AllValues file + .rvis
        private async Task SyncJobPrintFromServerAsync(string jobFileName)
        {
            if (string.IsNullOrWhiteSpace(jobFileName)) return;
            await _syncSemaphore.WaitAsync();
            try
            {
                string fileExt = Shared.Settings.JobFileExtension ?? ".rvis";
                string jobPath = Path.Combine(CommVariables.PathJobsApp.TrimEnd('\\'),
                                 jobFileName.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase) ? jobFileName : jobFileName + fileExt);
                if (!File.Exists(jobPath)) return;

                var job = JobModel.LoadFile(jobPath);
                if (job == null) return;

                var allProc = new CaoSuAllValueProcess(job);
                var payload = allProc.GetAllValuePayload();

                ResponseSyncOffline response = null;
                try
                {
                    response = await _caoSuApiService.PostSyncOfflineAsync(payload);
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError($"SyncJobPrintFromServerAsync API error for {jobFileName}: {ex.Message}");
                    return;
                }

                if (response == null || response.result == null) return;

                try
                {
                    var j = JObject.FromObject(response);
                    var qrList = j["result"]?["qr_list"] as JArray;
                    if (qrList != null)
                    {
                        foreach (var item in qrList)
                        {
                            string code = (string)item["qrcode_value"] ?? "";
                            string printedAt = (string)item["printed_at"] ?? "";
                            string checkedAt = (string)item["qr_checked_at"] ?? "";
                            string palletQr = (string)item["qr_pallet"] ?? "";
                            string mappedAt = (string)item["mapped_at"] ?? "";

                            if (!string.IsNullOrWhiteSpace(printedAt))
                                allProc.UpdatePrint(code, printedAt);

                            if (!string.IsNullOrWhiteSpace(checkedAt))
                                allProc.UpdateCheck(code, checkedAt);

                            if (!string.IsNullOrWhiteSpace(palletQr))
                            {
                                var mapped = string.IsNullOrWhiteSpace(mappedAt)
                                    ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    : mappedAt;
                                allProc.UpdatePallet(code, palletQr, mapped);
                            }
                        }
                    }

                    // update aggregates if provided
                    job.NumberOfSaaSSentCodes = response.result.printed_count;
                    job.NumberOfCheckSaaSSentCodes = response.result.checked_count;
                    job.NumberOfCodesInPallet = response.result.qr_with_pallet_count;
                    try { job.SaveFile(); } catch { }
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError($"Parse sync result error for {jobFileName}: {ex.Message}");
                }
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        /// <summary>
        /// Hiển thị danh sách JobCheck offline lên dgvHistoryJob
        /// </summary>
        private void DisplayHistory_PrintJobOffline()
        {
            DisplayHistoryByFilter();
        }

        private void DisplayHistory_JobCheckOffline()
        {
            if (InvokeRequired) { Invoke(new Action(DisplayHistory_JobCheckOffline)); return; }
            DisplayHistoryByFilter();
        }

        /// <summary>
        /// Hiển thị danh sách job lên dgvHistoryJob theo filter từ cbbHisFilterType
        /// Index: 0=Tất cả | 1=Đã hoàn thành | 2=Chưa hoàn thành | 3=JobPrint | 4=JobCheck
        /// </summary>
        /// <summary>
        /// Hiển thị danh sách job lên dgvHistoryJob theo filter từ cbbHisFilterType
        /// Index: 0=Tất cả | 1=Đã hoàn thành | 2=Chưa hoàn thành | 3=JobPrint | 4=JobCheck
        /// </summary>
        private void DisplayHistoryByFilter()
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired) { Invoke(new Action(DisplayHistoryByFilter)); return; }

            // ── Guard: cbbHisFilterType phải sẵn sàng ────────────────────────
            if (cbbHisFilterType == null || cbbHisFilterType.Items.Count == 0) return;

            string selectedJobName = null;
            try
            {
                if (dgvHistoryJob.SelectedRows.Count > 0)
                    selectedJobName = dgvHistoryJob.SelectedRows[0]
                        .Cells["MaCongViec"]?.Value?.ToString();
            }
            catch { }

            int filterIndex = cbbHisFilterType.SelectedIndex < 0 ? 0 : cbbHisFilterType.SelectedIndex;
            string fileExt = Shared.Settings.JobFileExtension ?? ".rvis";
            string checkDir = CommVariables.PathJobsCheckApp;
            string printDir = CommVariables.PathJobsApp;

            var checkFileNames = Directory.Exists(checkDir)
                ? Directory.GetFiles(checkDir, "*" + fileExt).Select(Path.GetFileName).ToList()
                : new List<string>();

            var printFileNames = Directory.Exists(printDir)
                ? Directory.GetFiles(printDir, "*" + fileExt)
                            .Select(Path.GetFileName)
                            .Where(fn => !Path.GetFileNameWithoutExtension(fn)
                                              .StartsWith("CHECK_", StringComparison.OrdinalIgnoreCase))
                            .ToList()
                : new List<string>();

            var allRows = new List<SyncDataList>();
            try
            {
                if (filterIndex != 4)
                    allRows.AddRange(SyncDataList.ReturnSyncDataList(printFileNames, printDir));
                if (filterIndex != 3)
                    allRows.AddRange(SyncDataList.ReturnSyncDataList(checkFileNames, checkDir));
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("DisplayHistoryByFilter load error: " + ex.Message);
                return;
            }

            if (filterIndex == 1)
                allRows = allRows.Where(r => r.Hoanthanh).ToList();
            else if (filterIndex == 2)
                allRows = allRows.Where(r => !r.Hoanthanh).ToList();

            try
            {
                dgvHistoryJob.SuspendLayout();
                dgvHistoryJob.Rows.Clear();

                for (int i = 0; i < allRows.Count; i++)
                {
                    var row = allRows[i];
                    int idx = dgvHistoryJob.Rows.Add();
                    dgvHistoryJob.Rows[idx].Cells["STT"].Value = i + 1;
                    dgvHistoryJob.Rows[idx].Cells["MaCongViec"].Value = row.MaCongViec;
                    dgvHistoryJob.Rows[idx].Cells["MaPhieuSoanHang"].Value = row.MaPhieuSoanHang;
                    dgvHistoryJob.Rows[idx].Cells["MaSanPham"].Value = row.MaSanPham;
                    dgvHistoryJob.Rows[idx].Cells["SoLuongCanXuat"].Value = row.SoLuongCanXuat;
                    dgvHistoryJob.Rows[idx].Cells["SoLuongDongBoSaaS"].Value = row.SoLuongDongBoSaaS;
                    dgvHistoryJob.Rows[idx].Cells["SoLuongDongBoSAP"].Value = row.SoLuongDongBoSAP;
                    dgvHistoryJob.Rows[idx].Cells["HoanThanh"].Value =
                        row.Hoanthanh ? "Đã Hoàn Thành" : "Chưa Hoàn Thành";
                }
            }
            finally
            {
                dgvHistoryJob.ResumeLayout();
            }

            // ── Restore selection ─────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(selectedJobName))
            {
                foreach (DataGridViewRow row in dgvHistoryJob.Rows)
                {
                    if (string.Equals(row.Cells["MaCongViec"]?.Value?.ToString(),
                            selectedJobName, StringComparison.OrdinalIgnoreCase))
                    {
                        row.Selected = true;
                        dgvHistoryJob.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Đồng bộ tất cả JobCheck offline đang hiển thị qua API /check-sync/offline
        /// </summary>

        /// <summary>
        /// Đồng bộ JobCheck offline được chọn trong dgvHistoryJob
        /// Bắt buộc phải chọn ít nhất 1 row, không hỗ trợ đồng bộ tất cả
        /// </summary>
        private async Task SyncJobCheckOfflineBatchAsync()
        {
            if (!Shared.UserPermission.isOnline)
            {
                CustomMessageBox.Show("Không thể đồng bộ khi đang ở chế độ Offline!",
                    Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvHistoryJob.SelectedRows.Count == 0)
            {
                CustomMessageBox.Show("Vui lòng chọn JobCheck cần đồng bộ!",
                    Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var targetRows = dgvHistoryJob.SelectedRows.Cast<DataGridViewRow>().Select(r => r.Index).ToList();
            string fileExt = Shared.Settings.JobFileExtension ?? ".rvis";

            // ── Kiểm tra row hợp lệ (Giữ nguyên logic cũ) ─────────────────
            var invalidRows = new List<string>();
            foreach (int r in targetRows)
            {
                var cellVal = dgvHistoryJob.Rows[r].Cells["MaCongViec"]?.Value?.ToString();
                if (string.IsNullOrWhiteSpace(cellVal)) continue;
                string checkPath = Path.Combine(CommVariables.PathJobsCheckApp.TrimEnd('\\'),
                    cellVal.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase) ? cellVal : cellVal + fileExt);
                if (!File.Exists(checkPath)) invalidRows.Add(cellVal);
            }

            if (invalidRows.Count > 0)
            {
                CustomMessageBox.Show("Job được chọn không phải JobCheck:\n" + string.Join("\n", invalidRows),
                    Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StartUISyncData();
            try
            {
                int successCount = 0;
                int failCount = 0;
                var apiService = new CaoSuApiService();

                foreach (int r in targetRows)
                {
                    try
                    {
                        var cell = dgvHistoryJob.Rows[r].Cells["MaCongViec"];
                        if (cell?.Value == null) continue;
                        string jobName = cell.Value.ToString();

                        // ── 1. Load JobCheck & JobPrint ──────────────────────────────
                        string jobCheckPath = Path.Combine(CommVariables.PathJobsCheckApp.TrimEnd('\\'),
                            jobName.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase) ? jobName : jobName + fileExt);
                        var jobCheck = JobModel.LoadFile(jobCheckPath);
                        if (jobCheck == null) continue;

                        JobModel jobPrint = null;
                        if (!string.IsNullOrWhiteSpace(jobCheck.LastCheckedPrintJob))
                        {
                            string printPath = Path.Combine(CommVariables.PathJobsApp.TrimEnd('\\'),
                                jobCheck.LastCheckedPrintJob.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase)
                                ? jobCheck.LastCheckedPrintJob : jobCheck.LastCheckedPrintJob + fileExt);
                            if (File.Exists(printPath)) jobPrint = JobModel.LoadFile(printPath);
                        }

                        // ── 2. Khởi tạo các Maps dữ liệu ─────────────────────────────
                        var printedAtMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        var checkedAtMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        var mappedAtMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        var palletMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        var createdAtMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        var statusFromCheckMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        // A. Đọc PrintedAt từ file AllValues của Job IN (Nguồn chính xác nhất cho thời gian in)
                        string allValuePrintFile = (!string.IsNullOrWhiteSpace(jobCheck.LastCheckedPrintJob)
                            ? Path.GetFileNameWithoutExtension(jobCheck.LastCheckedPrintJob) : jobCheck.FileName) + "_AllValues.csv";
                        string pathAVPrint = Path.Combine(CommVariables.PathAllValues, allValuePrintFile);
                        if (File.Exists(pathAVPrint))
                        {
                            var lines = File.ReadAllLines(pathAVPrint);
                            for (int i = 1; i < lines.Length; i++)
                            {
                                var p = lines[i].Split(',');
                                if (p.Length > CaoSuAllValueValues.PrintedTimeIndex && !string.IsNullOrWhiteSpace(p[1]))
                                    printedAtMap[p[1].Trim()] = p[CaoSuAllValueValues.PrintedTimeIndex].Trim();
                            }
                        }

                        // B. Đọc bổ sung từ file AllValues của Job CHECK (File bạn cung cấp - chứa thông tin Check/Map/Pallet)
                        string allValueCheckFile = Path.GetFileNameWithoutExtension(jobName) + "_AllValues.csv";
                        string pathAVCheck = Path.Combine(CommVariables.PathAllValues, allValueCheckFile);
                        if (File.Exists(pathAVCheck))
                        {
                            var lines = File.ReadAllLines(pathAVCheck);
                            for (int i = 1; i < lines.Length; i++)
                            {
                                var p = lines[i].Split(',');
                                if (p.Length >= 11)
                                {
                                    string qr = p[1].Trim();
                                    if (string.IsNullOrEmpty(qr)) continue;

                                    statusFromCheckMap[qr] = p[2].Trim();   // Cột Status
                                    createdAtMap[qr] = p[6].Trim();        // Cột Created Time
                                                                           // Nếu file In không có thời gian, lấy ở file Check (nếu có)
                                    if (!printedAtMap.ContainsKey(qr) || string.IsNullOrEmpty(printedAtMap[qr]))
                                        printedAtMap[qr] = p[7].Trim();

                                    checkedAtMap[qr] = p[8].Trim();        // Cột Checked Time
                                    mappedAtMap[qr] = p[9].Trim();         // Cột Mapped Time
                                    palletMap[qr] = p[10].Trim();          // Cột QRCode Pallet
                                }
                            }
                        }

                        // ── 3. Đọc database gốc để lấy danh sách QR chuẩn ─────────────
                        string dbPath = jobCheck.DirectoryDatabase ?? jobPrint?.DirectoryDatabase ?? "";
                        if (!File.Exists(dbPath)) { failCount++; continue; }
                        var databaseQRList = new List<string>();
                        var dbLines = File.ReadAllLines(dbPath);
                        bool skipFirst = jobCheck.IsFirstRowHeader || (jobPrint?.IsFirstRowHeader ?? false);
                        for (int li = skipFirst ? 1 : 0; li < dbLines.Length; li++)
                        {
                            var p = dbLines[li].Split(',');
                            if (p.Length > 0 && !string.IsNullOrWhiteSpace(p[0])) databaseQRList.Add(p[0].Trim());
                        }

                        // ── 4. Build Payload Sync ────────────────────────────────────
                        var qrList = new List<QrList>();
                        for (int qi = 0; qi < databaseQRList.Count; qi++)
                        {
                            string qr = databaseQRList[qi]; // Đây chính là mã qrcode_value

                            printedAtMap.TryGetValue(qr, out string pAt);
                            checkedAtMap.TryGetValue(qr, out string cAt);
                            mappedAtMap.TryGetValue(qr, out string mAt);
                            palletMap.TryGetValue(qr, out string pl);
                            createdAtMap.TryGetValue(qr, out string crAt);
                            statusFromCheckMap.TryGetValue(qr, out string stCheck);

                            // Xác định status theo độ ưu tiên: valid (đã check) > printed > created
                            string finalStatus = "created";
                            if (!string.IsNullOrEmpty(cAt)) finalStatus = "valid";
                            else if (!string.IsNullOrEmpty(pAt)) finalStatus = "printed";
                            else if (!string.IsNullOrEmpty(stCheck)) finalStatus = stCheck.ToLower() == "mapped" ? "valid" : stCheck.ToLower();

                            qrList.Add(new QrList
                            {
                                index_in_lot = qi + 1,
                                qrcode_value = qr,
                                status = finalStatus,
                                product_code = jobCheck.LastCheckedProductCode ?? jobPrint?.CaoSuProduct?.product_code ?? "",
                                production_batch_code = jobCheck.LOTNumber ?? "",
                                weight = (int)(jobCheck.productWeight > 0 ? jobCheck.productWeight : (jobPrint?.productWeight ?? 0)),
                                created_time = !string.IsNullOrEmpty(crAt) ? crAt : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                printed_at = pAt ?? "",
                                qr_checked_at = cAt ?? "",
                                mapped_at = mAt ?? "",
                                qr_pallet = pl ?? ""
                            });
                        }

                        if (qrList.Count == 0) { failCount++; continue; }

                        // ── 5. Gọi API ────────────────────────────────────────────────
                        var response = await apiService.PostSyncOfflineAsync(new RequestSyncOffline { qr_list = qrList });
                        if (response != null && response.success)
                        {
                            // Cập nhật số lượng vào JobCheck
                            jobCheck.NumberOfSaaSSentCodes = response.result.printed_count;
                            jobCheck.NumberOfCheckSaaSSentCodes = response.result.checked_count;
                            jobCheck.NumberOfCodesInPallet = response.result.qr_with_pallet_count;

                            try { jobCheck.SaveFile(); } catch { }

                            successCount++;
                            try { dgvHistoryJob.Rows[r].Cells["HoanThanh"].Value = "Đã Đồng Bộ"; } catch { }
                        }
                        else
                        {
                            failCount++;
                            //ProjectLogger.WriteError($"[SyncJobCheck] API failed for {jobName}: {response?.message}");
                        }

                        await Task.Delay(150);
                    }
                    catch (Exception ex) { failCount++; ProjectLogger.WriteError($"Row error: {ex.Message}"); }
                }

                DisplayHistoryByFilter();
                CustomMessageBox.Show($"Đồng bộ hoàn tất.\nThành công: {successCount} | Thất bại: {failCount}",
                    Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { CustomMessageBox.Show("Lỗi: " + ex.Message, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { StopUISyncData(); }
        }

        // Push local printed statuses to server (/check-sync/update-status-to-printed)
        private async Task PushLocalPrintedStatusToServerAsync(string jobFileName)
        {
            if (string.IsNullOrWhiteSpace(jobFileName)) return;
            await _syncSemaphore.WaitAsync();
            try
            {
                string fileExt = Shared.Settings.JobFileExtension ?? ".rvis";
                string jobPath = Path.Combine(CommVariables.PathJobsApp.TrimEnd('\\'),
                                 jobFileName.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase) ? jobFileName : jobFileName + fileExt);
                if (!File.Exists(jobPath)) return;

                var job = JobModel.LoadFile(jobPath);
                if (job == null) return;

                var allProc = new CaoSuAllValueProcess(job);
                var printedList = allProc.GetPrintedQRCodes(); // "qrcode,printedDate"

                if (printedList == null || printedList.Count == 0) return;

                var request = new RequestUpdateCodePrint
                {
                    qr_list = printedList.Select(s =>
                    {
                        var parts = s.Split(',');
                        return new QrCodePrint
                        {
                            qrcode_value = parts.Length > 0 ? parts[0] : "",
                            execute_date = parts.Length > 1 ? parts[1] : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                    }).ToList()
                };

                try
                {
                    var resp = await _caoSuApiService.PostCodePrintAsync(request);
                    if (resp != null && resp.success)
                    {
                        // nếu cần đánh dấu local đã gửi: có thể cập nhật một trường trong AllValues/Storage
                        try { job.SaveFile(); } catch { }
                    }
                    else
                    {
                        ProjectLogger.WriteError($"PushLocalPrintedStatus failed for {jobFileName}: {resp?.message}");
                    }
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError($"PushLocalPrintedStatusToServerAsync error for {jobFileName}: {ex.Message}");
                }
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        // Replace/extend SyncDataBtn handling: nếu filter là "PrintJob Offline" thì duyệt tất cả row đang hiển thị và sync tuần tự
        // (chèn vào ActionResult khi xử lý sender == SyncDataBtn)
        private async Task Sync_PrintJobOffline_BatchAsync()
        {
            // Kiểm tra filter hiện tại: có chứa chữ "PrintJob" hoặc SelectedIndex tương ứng
            bool isPrintJobOfflineFilter = false;
            try
            {
                if (cbbHisFilterType.SelectedItem != null)
                {
                    var txt = cbbHisFilterType.SelectedItem.ToString();
                    if (txt.IndexOf("PrintJob", StringComparison.OrdinalIgnoreCase) >= 0)
                        isPrintJobOfflineFilter = true;
                }
                // optional: nếu bạn biết index cố định cho mục này, kiểm tra SelectedIndex == 3
                if (!isPrintJobOfflineFilter && cbbHisFilterType.SelectedIndex == 3)
                    isPrintJobOfflineFilter = true;
            }
            catch { isPrintJobOfflineFilter = false; }

            if (!isPrintJobOfflineFilter) return;

            StartUISyncData();
            try
            {
                for (int r = 0; r < dgvHistoryJob.Rows.Count; r++)
                {
                    try
                    {
                        var cell = dgvHistoryJob.Rows[r].Cells["MaCongViec"];
                        if (cell == null) continue;
                        var jobName = cell.Value?.ToString();
                        if (string.IsNullOrWhiteSpace(jobName)) continue;

                        // Pull from server and update AllValues/.rvis
                        await SyncJobPrintFromServerAsync(jobName);

                        // Push local printed statuses to server
                        await PushLocalPrintedStatusToServerAsync(jobName);

                        // Update row UI from saved job
                        try
                        {
                            string fileExt = Shared.Settings.JobFileExtension ?? ".rvis";
                            string jobPath = Path.Combine(CommVariables.PathJobsApp.TrimEnd('\\'),
                                                 jobName.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase) ? jobName : jobName + fileExt);

                            if (!File.Exists(jobPath))
                            {
                                string checkPath = Path.Combine(CommVariables.PathJobsCheckApp.TrimEnd('\\'),
                                                   jobName.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase) ? jobName : jobName + fileExt);
                                if (File.Exists(checkPath)) jobPath = checkPath;
                            }

                            if (File.Exists(jobPath))
                            {
                                var j = JobModel.LoadFile(jobPath);
                                if (j != null)
                                {
                                    int quantity = j.productWeight > 30 ? 144 : 240;
                                    dgvHistoryJob.Rows[r].Cells["SoLuongCanXuat"].Value = j.NumberOfSaaSSentCodes;
                                    dgvHistoryJob.Rows[r].Cells["SoLuongDongBoSaaS"].Value = j.NumberOfCheckSaaSSentCodes;
                                    dgvHistoryJob.Rows[r].Cells["SoLuongDongBoSAP"].Value = $"{j.NumberOfCodesInPallet} / {quantity}";
                                    dgvHistoryJob.Rows[r].Cells["HoanThanh"].Value = (j.NumberOfCodesInPallet == quantity) ? "Đã Hoàn Thành" : "Chưa Hoàn Thành";
                                }
                            }
                        }
                        catch { /* non-fatal */ }

                        await Task.Delay(150); // tránh quá tải API
                    }
                    catch (Exception exRow)
                    {
                        ProjectLogger.WriteError("Error syncing job row: " + exRow.Message);
                    }
                }

                // Reload lists sau khi batch sync hoàn tất
                LoadJobNameList();
                DisplayHistory_PrintJobOffline();
                CustomMessageBox.Show("Đồng bộ PrintJob Offline hoàn tất.", Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Batch sync PrintJob Offline error: " + ex.Message);
                CustomMessageBox.Show("Lỗi khi đồng bộ danh sách PrintJob Offline.\n" + ex.Message, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                StopUISyncData();
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
        private void LoadJobCheckList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LoadJobCheckList()));
                return;
            }

            dgvListJobCheck.Items.Clear();

            string dir = CommVariables.PathJobsCheckApp;
            if (!Directory.Exists(dir)) return;

            string[] jobCheckFiles = Directory.GetFiles(
                dir,
                "*" + Shared.Settings.JobFileExtension
            );

            foreach (var file in jobCheckFiles)
            {
                try
                {
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(JobModel));
                    using (var sr = new StreamReader(file))
                    {
                        var job = (JobModel)xs.Deserialize(sr);
                        if (job != null
                            && job.JobType == JobType.StandAlone
                            && job.CompareType == CompareType.Database
                            && job.JobStatus != JobStatus.Deleted)
                        {
                            // Chỉ add FileName thuần vào Items
                            // DrawItem sẽ load lại JobModel và hiển thị đầy đủ thông tin
                            dgvListJobCheck.Items.Add(job.FileName);
                        }
                    }
                }
                catch { }
            }
        }

        private void dgvListJobCheck_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || dgvListJobCheck.Items.Count == 0) return;
            try
            {
                string fileName = dgvListJobCheck.Items[e.Index].ToString();
                JobModel jobModel = null;

                string filePath = Path.Combine(
                    CommVariables.PathJobsCheckApp,
                    fileName + Shared.Settings.JobFileExtension);

                if (File.Exists(filePath))
                {
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(JobModel));
                    using (var sr = new StreamReader(filePath))
                    {
                        jobModel = (JobModel)xs.Deserialize(sr);
                    }
                }

                string lotDisplay = jobModel?.LOTNumber ?? "N/A";
                // Ưu tiên hiển thị tên SP, fallback về mã SP nếu không có tên
                string productDisplay = !string.IsNullOrWhiteSpace(jobModel?.LastCheckedProductName)
                    ? string.Format("{0} ({1})", jobModel.LastCheckedProductName, jobModel.LastCheckedProductCode)
                    : jobModel?.LastCheckedProductCode ?? "N/A";
                string userDisplay = jobModel?.UserCreate ?? "";
                string statusDisplay = jobModel?.JobStatus.ToFriendlyString() ?? "";

                string displayText = jobModel != null
                    ? string.Format(" {0}  |  LOT: {1}  ",
                        //? string.Format(" {0}  |  LOT: {1}  |  SP: {2}  |  {3}  |  {4}",
                        fileName, lotDisplay, productDisplay, userDisplay, statusDisplay)
                    : string.Format(" {0}", fileName);

                e.DrawBackground();

                Rectangle headItemRect = new Rectangle(e.Bounds.X, e.Bounds.Y + 4, 8, e.Bounds.Height - 10);
                using (Brush brush = new SolidBrush(_RLinkColor))
                {
                    e.Graphics.FillRectangle(brush, headItemRect);
                }

                int textOffset = headItemRect.Right + 4;
                Rectangle textRect = new Rectangle(textOffset, e.Bounds.Y, e.Bounds.Width - textOffset, e.Bounds.Height);
                Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? Color.White
                    : Color.Black;

                using (Brush brush = new SolidBrush(textColor))
                {
                    e.Graphics.DrawString(displayText, e.Font, brush, textRect);
                }

                e.DrawFocusRectangle();
            }
            catch { }
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
        //private void dgvListJobPrint_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    if (e.Index == -1 || (sender as System.Windows.Forms.ListBox).Items.Count == 0) return;
        //    try
        //    {
        //        JobModel job = Shared.GetJob((sender as System.Windows.Forms.ListBox).Items[e.Index].ToString());
        //        Rectangle headItemRect = new Rectangle(0, e.Bounds.Y + 4, 8, e.Bounds.Height - 10);
        //        using (Brush brush = new SolidBrush(_Standalone))
        //            if (!job.PrinterSeries)
        //                e.Graphics.FillRectangle(brush, headItemRect);
        //    }
        //    catch
        //    {
        //    }
        //}
        private void dgvListJobPrint_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || dgvListJobPrint.Items.Count == 0) return;
            try
            {
                // Lấy JobModel từ JobName
                string jobName = dgvListJobPrint.Items[e.Index].ToString();
                JobModel jobModel = Shared.GetJob(jobName);
                if (jobModel == null) return;

                // Xác định loại bành
        
                string productType = jobModel.NumberOfPrintedCodes > 144 ? "nhỏ" : "lớn";

                // Chuỗi hiển thị
                string displayText = $" {jobModel.FileName} | " +
                                     $"{jobModel.productWeight}KG| Bành (Loại {productType}) | " +
                                     $"{jobModel.NumberOfPrintedCodes} / {jobModel.NumberOfNeededSentCodes}";

                // Vẽ nền mặc định
                e.DrawBackground();

                // Vẽ hình chữ nhật màu xanh ở đầu dòng (luôn luôn vẽ)
                Rectangle headItemRect = new Rectangle(e.Bounds.X, e.Bounds.Y + 4, 8, e.Bounds.Height - 10);
                using (Brush brush = new SolidBrush(_RLinkColor))
                {
                    e.Graphics.FillRectangle(brush, headItemRect);
                }

                // Vẽ text, lùi vào sau hình chữ nhật
                int textOffset = headItemRect.Right + 4;
                Rectangle textRect = new Rectangle(textOffset, e.Bounds.Y, e.Bounds.Width - textOffset, e.Bounds.Height);
                Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? Color.White : Color.Black;
                using (Brush brush = new SolidBrush(textColor))
                {
                    e.Graphics.DrawString(displayText, e.Font, brush, textRect);
                }
                e.DrawFocusRectangle();
            }
            catch
            {
                // Không làm gì để tránh crash UI
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

        private void TxtSearchJobCheck_TextChanged(object sender, EventArgs e)
        {
            string keyWord = txtSearchJobCheck.Text.ToLower();

            dgvListJobCheck.Items.Clear();

            string dir = CommVariables.PathJobsCheckApp;
            if (!Directory.Exists(dir)) return;

            string[] jobCheckFiles = Directory.GetFiles(dir, "*" + Shared.Settings.JobFileExtension);

            foreach (var file in jobCheckFiles)
            {
                try
                {
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(JobModel));
                    using (var sr = new StreamReader(file))
                    {
                        var job = (JobModel)xs.Deserialize(sr);
                        if (job != null
                            && job.JobType == JobType.StandAlone
                            && job.CompareType == CompareType.Database
                            && job.JobStatus != JobStatus.Deleted
                            && job.FileName.ToLower().Contains(keyWord))
                        {
                            dgvListJobCheck.Items.Add(job.FileName);
                        }
                    }
                }
                catch { }
            }
        }

        private void BtnRefeshJobCheck_Click(object sender, EventArgs e)
        {
            txtSearchJobCheck.Text = "";
            LoadJobCheckList();
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

            tabGetPO.Text = "In Sản Phẩm"; // Lang.CreateANewJob
            tabSelectJob.Text = "Danh Sách Lệnh Sản Xuất"; //Lang.SelectJob
            //tabSyncData.Text = "Lịch Sử Đồng Bộ"; // Lang.HistorySync
            TabCreatePOOffline.Text = "In Sản Phẩm"; // Lang.Settings
        }
        private void InitUI()
        {
            try
            {
                // ── Init items cho cbbHisFilterType ──────────────────────────
                _IsBinding = true;
                cbbHisFilterType.Items.Clear();
                cbbHisFilterType.Items.Add("Tất cả");          // 0
                cbbHisFilterType.Items.Add("Đã hoàn thành");   // 1
                cbbHisFilterType.Items.Add("Chưa hoàn thành"); // 2
                cbbHisFilterType.Items.Add("JobPrint");         // 3
                cbbHisFilterType.Items.Add("JobCheck");         // 4
                cbbHisFilterType.SelectedIndex = 0;
                _IsBinding = false;

                HistoryUtils.CustomDataGridView(dgv: dgvHistoryJob);
                SetupDataGridView();

               

                tabControl1.Controls.Remove(TabCreatePOOffline);
                dgvItems.Rows.Clear();
                GetAllProducts();

                // ── Load tất cả job ngay khi khởi động ───────────────────────
                DisplayHistoryByFilter();
            }
            catch (Exception) { }
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
            _TimerDateTime.Start();
            _NameOfJobOld = "";
            Shared.JobNameSelected = "";
            var podText = new PODModel(0, "", PODModel.TypePOD.TEXT, "");
            _PODList.Add(podText);

            TabPage tempTab = tabGetPO;
            TabPage tabSync = tabSyncData;
            TabPage TabCreatePO = TabCreatePOOffline;
            TabPage tabSelect = tabSelectJob;

            tabControl1.TabPages.Remove(tabGetPO);
            if (!UserPermission.isOnline)
            {
                tabControl1.Controls.Remove(tabSyncData);
            }
            tabControl1.TabPages.Remove(TabCreatePOOffline);
            tabControl1.TabPages.Remove(tabSelectJob);

            btnSettings.Enabled = Shared.UserPermission.Settings;
            btnDelete.Enabled = Shared.UserPermission.DeleteJob;
            tabGetPO.Enabled = Shared.UserPermission.CreateJob;

            for (int index = 1; index <= 20; index++)
            {
                var podVCD = new PODModel(index, "", PODModel.TypePOD.FIELD, "");
                _PODList.Add(podVCD);
            }

            FirstRowHeader.Visible = _JobModel.IsFirstRowHeader = FirstRowHeader.Checked = false;
            UIControlsFuncs.HideControls(lblSensorControllerStatus, FirstRowHeader, btnHelp, btnAbout);
            LineName.Text = "Tên Line: " + Shared.Settings.RLinkName ?? "";
            UserNameDisplay.Text = "Trạng thái: " + (UserPermission.isOnline ? "Online" : "Offline");

            if (CurrentUser.UserName == "Support")
            {
                ErrorsLogger.Visible = true;
            }

            // ── Init filter history ───────────────────────────────────────────
            cbbHisFilterType.Items.Clear();
            cbbHisFilterType.Items.Add("Tất cả");          // 0
            cbbHisFilterType.Items.Add("Đã hoàn thành");   // 1
            cbbHisFilterType.Items.Add("Chưa hoàn thành"); // 2
            cbbHisFilterType.Items.Add("JobPrint");         // 3
            cbbHisFilterType.Items.Add("JobCheck");         // 4
            cbbHisFilterType.SelectedIndex = 0;
            cbbHisFilterType.SelectedIndexChanged += (s, e) => DisplayHistoryByFilter();

            MonitorCameraConnection();
            MonitorCameraConnection_CognexSupport();
            MonitorPrinterConnection();
            MonitorSensorControllerConnection();
            MonitorSerialDeviceControllerConnection();
            MonitorListenerServer();
        }

        private async void GetAllProducts()
        {
            ResponseAllProducts products;
            if (UserPermission.isOnline)
            {
                products = Settings.CaoSuProductList = await _caoSuApiService.GetAllProductsAsync();
            }
            else
            {
                products = Settings.CaoSuProductList;
            }
            InitPOListCombo(products);
            SaveSettings();
        }
        // BarcodeVerificationSystem\View\OtherProjects\CaoSuDongNaiUI\frmJobCaoSu.cs

        private void DisplayHistory(List<string> JobNameList, HistoryFilter filter = HistoryFilter.All)
        {
            // Chỉ load JobCheck từ PathJobsCheckApp
            string checkDir = CommVariables.PathJobsCheckApp;
            string fileExt = Shared.Settings.JobFileExtension;
            var checkJobNames = new List<string>();

            if (Directory.Exists(checkDir))
            {
                checkJobNames = Directory.GetFiles(checkDir, "*" + fileExt)
                    .Select(f => Path.GetFileName(f))
                    .ToList();
            }

            var rows = SyncDataList.ReturnSyncDataList(checkJobNames);
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
        }
        //private void DisplayHistory(List<string> JobNameList, HistoryFilter filter = HistoryFilter.All)
        //{
        //    var rows = SyncDataList.ReturnSyncDataList(JobNameList);
        //    dgvHistoryJob.Rows.Clear();
        //    int i = 0;
        //    foreach (var row in rows)
        //    {
        //        if (filter == HistoryFilter.Finished && !row.Hoanthanh)
        //            continue;
        //        if (filter == HistoryFilter.NotFinished && row.Hoanthanh)
        //            continue;
        //        i++;
        //        int rowIndex = dgvHistoryJob.Rows.Add();
        //        dgvHistoryJob.Rows[rowIndex].Cells["STT"].Value = i;
        //        dgvHistoryJob.Rows[rowIndex].Cells["MaCongViec"].Value = row.MaCongViec;
        //        dgvHistoryJob.Rows[rowIndex].Cells["MaPhieuSoanHang"].Value = row.MaPhieuSoanHang;
        //        dgvHistoryJob.Rows[rowIndex].Cells["MaSanPham"].Value = row.MaSanPham;
        //        dgvHistoryJob.Rows[rowIndex].Cells["SoLuongCanXuat"].Value = row.SoLuongCanXuat;
        //        dgvHistoryJob.Rows[rowIndex].Cells["SoLuongDongBoSaaS"].Value = row.SoLuongDongBoSaaS;
        //        dgvHistoryJob.Rows[rowIndex].Cells["SoLuongDongBoSAP"].Value = row.SoLuongDongBoSAP;
        //        dgvHistoryJob.Rows[rowIndex].Cells["HoanThanh"].Value = row.Hoanthanh ? "Đã Hoàn Thành" : "Chưa Hoàn Thành";

        //    }

        //    Console.WriteLine("So luong: " + _JobNameList.Count);
        //}
        private void SetupDataGridView()
        {
            #region Process Order
            dgvItems.Columns.Clear();
            dgvItems.Columns.Add("product_code", "Mã sản phẩm");
            dgvItems.Columns.Add("product_name", "Tên sản phẩm");
            dgvItems.Columns.Add("size", "Kích thước");

            var weightColumn = new DataGridViewComboBoxColumn
            {
                Name = "listweight",
                HeaderText = "Trọng lượng",
                Width = 120,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                FlatStyle = FlatStyle.Flat
            };
            dgvItems.Columns.Add(weightColumn);

            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.MultiSelect = false;
            dgvItems.ReadOnly = false;
            dgvItems.AllowUserToAddRows = false;
            dgvItems.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvItems.RowTemplate.Height = 45;
            dgvItems.Columns["listweight"].Width = 120;

            foreach (DataGridViewColumn column in dgvItems.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;

            dgvItems.AutoResizeColumnHeadersHeight();
            #endregion

            // ── dgvHistoryJob: chỉ cho chọn 1 row, không cho chọn nhiều ──────
            dgvHistoryJob.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistoryJob.MultiSelect = false;
            dgvHistoryJob.ReadOnly = true;
            dgvHistoryJob.AllowUserToAddRows = false;
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

                List<string> list = Base30AutoCodeGenerator.GenerateLineCodesForLoyalty(quantity: quantity);

                _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
                _JobModel.Reservation = Reservation;
                _JobModel.ReservationItem = SelectedItem;
                _JobModel.SelectedRESItemIndex = SelectedRESMaterialIndex;
                _JobModel.SelectedBatchIndex = 0;
                _JobModel.IsReservationMode = true;

                var firstColumnList = FileFuncs.GetFirstColumn(list);
                bool isSent = isPushDatabase = await SendGeneratedCodes(firstColumnList, _JobModel);

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
                var PO = _JobModel.CaoSuProduct = Settings.CaoSuProduct = Settings.CaoSuProductList.data.data[lineIndex];
                string productCode = PO.product_code;
                string productName = PO.product_name;
                string size = PO.size;
                double weight = _JobModel.productWeight = Settings.ProductWeight = double.Parse(dgvItems.SelectedRows[0].Cells["listweight"].Value.ToString());
                int quantity = weight > 30 ? 144 : 240;

                string AskQuestion = Lang.AreYouSureGenerateDispatchingCodes +
                         $"\nMã sản phẩm: {productCode}" +
                         $"\nTên sản phẩm: {productName}" +
                         $"\nKích thước: {size}" +
                         $"\nSố lượng mã cần tạo: {quantity}";

                if (!CustomMessageBox.IsResultShow(AskQuestion)) return;
                DisplayJobLoading(true);

                txtFileName.Text = _JobModel.FileName = jobName.Text
                    = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + productName + "_" + InputLotNumber.Text.Trim() + "_" +
                     Settings.RLinkName + "_" + Settings.LineIndex; // + "_" + size 
                templatePrint.Text = _JobModel.TemplatePrint = Settings.PrintTemplate; //templatePrint.Text

                List<string> list = Base30AutoCodeGenerator.GenerateCodesForCaoSuDongNai(quantity: quantity);

                _JobModel.isPushedDatabase = isPushDatabase = false;
                _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
                _JobModel.LOTNumber = InputLotNumber.Text.Trim();

                if (UserPermission.isOnline)
                {
                    var firstColumnList = FileFuncs.GetFirstColumn(list);
                    bool isSent = isPushDatabase = await SendGeneratedCodes(firstColumnList, _JobModel);
                    if (!isSent) return;
                }

                string tableName = productName; // Example table name, adjust as needed
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
        /// <summary>
        /// Đếm số JobPrint đã tạo trong ngày hôm nay để lấy số thứ tự tiếp theo.
        /// Đếm cả PathJobsApp (JobPrint) lẫn PathJobsCheckApp (để tránh trùng).
        /// </summary>
        private int GetNextJobPrintIndex()
        {
            try
            {
                string today = DateTime.Now.ToString("yyyyMMdd");
                string fileExt = Shared.Settings.JobFileExtension ?? ".rvis";
                string dir = CommVariables.PathJobsApp;

                if (!Directory.Exists(dir)) return 1;

                int count = Directory.GetFiles(dir, "*" + fileExt)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Count(name =>
                        name != null &&
                        !name.StartsWith("CHECK_", StringComparison.OrdinalIgnoreCase) &&
                        name.StartsWith(today, StringComparison.OrdinalIgnoreCase));

                return count + 1;
            }
            catch
            {
                return 1;
            }
        }
        private string _currentPrintJobWeightRange = "";
        private async Task GeneratePrintCodesSendWeightOnly()
        {
            try
            {
                string weightText = cbcWeight.Text.Trim();
                int quantity = 0;

                if (weightText == "<=30")
                    quantity = 240;
                else if (weightText == ">30")
                    quantity = 144;
                else
                {
                    CustomMessageBox.Show("Vui lòng chọn loại trọng lượng (<=30 hoặc >30)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string AskQuestion = Lang.AreYouSureGenerateDispatchingCodes +
                    $"\nĐiều kiện: {weightText}" +
                    $"\nSố lượng mã cần tạo: {quantity}";

                if (!CustomMessageBox.IsResultShow(AskQuestion)) return;
                DisplayJobLoading(true);
                int nextIndex = GetNextJobPrintIndex();
                string onlineFlag = UserPermission.isOnline ? "" : "_Offline";
                txtFileName.Text = _JobModel.FileName = jobName.Text
                    = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" +
                      Settings.RLinkName + "_" + nextIndex + onlineFlag;
                templatePrint.Text = _JobModel.TemplatePrint = Settings.PrintTemplate;

                // ── Set NumberTotalsCode để SaveJob lưu đúng vào .rvis ────────
                _currentPrintJobWeightRange = weightText; // ← lưu lại để SaveJob dùng
                _NumberTotalsCode = quantity;
                _JobModel.NumberTotalsCode = quantity;
                _JobModel.PrintJobWeightRange = weightText; // ">30" hoặc "<=30"

                List<string> list = Base30AutoCodeGenerator.GenerateCodesForCaoSuDongNai(quantity: quantity);

                _JobModel.isPushedDatabase = isPushDatabase = false;
                _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;

                string tableName = "WeightOnly";
                string fileName = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string documentsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "R-Link", "Database");
                if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);
                string filePath = Path.Combine(documentsPath, fileName);

                if (UserPermission.isOnline)
                {
                    bool isSent = await SendWeightOnlyAndSaveDatabase(0, filePath, list);
                    if (!isSent) return;
                }
                else
                {
                    FileFuncs.WriteStringListToCsv(list, filePath);
                }

                databasePath = filePath;
                numberOfCodesGenerate = list.Count;
            }
            catch (Exception ex)
            {
                DisplayJobLoading(false);
                CustomMessageBox.Show("Không thể lưu database!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProjectLogger.WriteError("Error occurred in GeneratePrintCodesSendWeightOnly: " + ex.Message);
            }
        }


        /// <summary>
        /// Gửi trọng lượng duy nhất lên server (không gửi danh sách mã), vẫn lưu file database như cũ.
        /// </summary>
        /// <param name="weight">Trọng lượng cần gửi</param>
        /// <param name="databaseFilePath">Đường dẫn file database cần lưu</param>
        /// <param name="dataToSave">Danh sách dữ liệu cần lưu vào file</param>
        /// <returns>True nếu gửi thành công, ngược lại trả về false</returns>
        public async Task<bool> SendWeightOnlyAndSaveDatabase(double weight, string databaseFilePath, List<string> dataToSave)
        {
            try
            {
                // Gửi trọng lượng lên server với body giữ nguyên các trường, trường nào trống thì gửi ""
                var request = new RequestPushDatabase()
                {
                    production_batch_code = "",
                    product_code = "",
                    weight = weight,
                    printed_number = dataToSave?.Count ?? 0,
                    execute_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    qr_code = dataToSave != null
                    ? dataToSave.Select((x, index) => new DatabaseQrCode
                    {
                        // Tách chuỗi bằng dấu phẩy và chỉ lấy phần tử đầu tiên (URL)
                        qrcode_value = (!string.IsNullOrEmpty(x) && x.Contains(","))
                                       ? x.Split(',')[0]
                                       : (x ?? ""),

                        index_in_lot = index + 1,
                        execute_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }).ToList()
    : new List<DatabaseQrCode>()
                };

                ResponsePushDatabase result = await _caoSuApiService.PostPushDatabaseAsync(request);
                if (result is null)
                {
                    CustomMessageBox.Show("Không thể gửi trọng lượng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!result.success)
                {
                    CustomMessageBox.Show(result.message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DisplayJobLoading(false);
                    return false;
                }

                // Lưu file database như cũ
                if (!string.IsNullOrEmpty(databaseFilePath) && dataToSave != null)
                {
                    FileFuncs.WriteStringListToCsv(dataToSave, databaseFilePath);
                }

                DisplayJobLoading(false);
                return true;
            }
            catch (Exception ex)
            {
                DisplayJobLoading(false);
                CustomMessageBox.Show("Không thể gửi trọng lượng hoặc lưu file database!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProjectLogger.WriteError("Error occurred in SendWeightOnlyAndSaveDatabase: " + ex.Message);
                return false;
            }
        }
        public async Task<bool> SendGeneratedCodes(List<string> list, JobModel jobModel)
        {
            if (jobModel.isPushedDatabase) return true;

            var request = new RequestPushDatabase();
            request = new RequestPushDatabase()
            {
                production_batch_code = jobModel.LOTNumber,
                product_code = jobModel.CaoSuProduct?.product_code ?? "",
                weight = jobModel.productWeight,
                printed_number = list.Count,
                execute_date = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                qr_code = list
                .Select((x, index) => new DatabaseQrCode
                {
                    qrcode_value = x,
                    index_in_lot = index + 1,
                    execute_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList()
            };

            ResponsePushDatabase result = await _caoSuApiService.PostPushDatabaseAsync(request);
            if (result is null)
            {
                CustomMessageBox.Show("Không thể gửi dữ liệu ban đầu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!result.success)
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

            //return true; // await SendAsync(request, url)
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
        private void dgvItems_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception is ArgumentException)
            {
                e.ThrowException = false;
            }
        }


        private void InitEvents()
        {
            OnSerialDeviceReadDataChange += Shared_OnSerialDeviceReadDataChange;
            OnSyncDataParameterChange += Shared_OnSyncDataParameterChange;
            OnSyncCheckDataParameterChange += Shared_OnSyncCheckDataParameterChange;
            dgvItems.DataError += dgvItems_DataError;
            dgvItems.CurrentCellDirtyStateChanged += dgvItems_CurrentCellDirtyStateChanged;
            dgvItems.CellValueChanged += dgvItems_CellValueChanged;
            tabControl1.Selecting += TabControl1_Selecting;
            GetMaterialDoc.Click += ActionResult;
            btnUpdateProducts.Click += ActionResult;
            saveJobCaoSu.Click += ActionResult;
            saveJobCaoSuCheck.Click += ActionResult;
            saveJobCaoSuPrint.Click += ActionResult;
            saveJobReservation.Click += ActionResult;
            SyncDataBtn.Click += ActionResult;
            savePOOffline.Click += ActionResult;
            CreateRESOffline.Click += ActionResult;
            StopSyncData.Click += ActionResult;
            cbbHisFilterType.SelectedIndexChanged += ActionResult;
            editJobBtn.Click += ActionResult;
            ErrorsLogger.Click += ActionResult;
            btnAddWeight.Click += ActionResult;

            txtSearchJobCheck.TextChanged += TxtSearchJobCheck_TextChanged;
            btnRefeshJobCheck.Click += BtnRefeshJobCheck_Click;
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
            radAfterProduction.EnabledChanged += JobType_EnabledChanged;
            radOnProduction.CheckedChanged += RadioButton_CheckedChanged;
            radOnProduction.CheckedChanged += ActionResult;
            radOnProduction.EnabledChanged += JobType_EnabledChanged;
            radVerifyAndPrint.CheckedChanged += RadioButton_CheckedChanged;
            radVerifyAndPrint.CheckedChanged += ActionResult;
            radVerifyAndPrint.EnabledChanged += JobType_EnabledChanged;
            txtStaticText.TextChanged += TxtStaticText_TextChanged;
            txtDirectoryDatabse.TextChanged += TxtDirectoryDatabse_TextChanged;
            txtFileName.TextChanged += TxtFileName_TextChanged;
            txtPODFormat.TextChanged += TxtPODFormat_TextChanged;
            // Thêm vào cuối InitEvents()
            dgvItems.CellPainting += DgvItems_CellPainting;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearchTemplate.TextChanged += TxtSearchTemplate_TextChanged;

            btnNextCheck.Click += ActionResult;
            dgvListJobCheck.DrawItem += dgvListJobCheck_DrawItem;

            btnPODFormat.Click += ActionResult;

            btnSettings.Click += ActionResult;
            listBoxJobList.SelectedIndexChanged += ActionResult;
            dgvListJobPrint.SelectedIndexChanged += ActionResult;
            dgvListJobCheck.DrawItem += dgvListJobCheck_DrawItem;
            listBoxPrintProductTemplate.SelectedIndexChanged += ActionResult;
            btnRefesh.Click += ActionResult;
            btnImportDatabase.Click += ActionResult;
            Shared.OnLanguageChange += Shared_OnLanguageChange;

            Load += FrmJob_Load;
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
            dgvListJobPrint.DrawItem += dgvListJobPrint_DrawItem;

            Shared.OnPrintingStateChange += Shared_OnPrintingStateChange;
            Shared.OnPrinterStatusChange += Shared_OnPrinterStatusChange;
            Shared.OnPrinterDataChange += Shared_OnPrinterDataChange;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnSensorControllerChangeEvent += Shared_OnSensorControllerChangeEvent;

            Shared.OnCameraStatusChange += Shared_OnCameraStatusChange;
            Shared.OnCameraTriggerOnChange += Shared_OnCameraTriggerOnChange;
            Shared.OnCameraTriggerOffChange += Shared_OnCameraTriggerOffChange;
            Shared.OnCameraOutputSignalChange += Shared_OnCameraOutputSignalChange;
            AutoAddSufixEvent += FrmJob_AutoAddSufixEvent;
            DMCamera.UpdateLabelStatusEvent += UpdateLabelStatusEvent;

            cuzButtonPurge.Click += CuzButtonPurge_Click;
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
      

        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // Refresh dữ liệu khi chuyển sang tabSyncData  
            if (e.TabPage == tabSyncData)
            {
                try
                {
                    DisplayHistoryByFilter();
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("TabControl1_Selecting error: " + ex.Message);
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

                if (CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSaaSSentCodes && (CurrentJob.NumberOfCheckSaaSSentCodes == NumberChecked))
                {
                    OnSentPrintedCodesCompleted();
                }

            }
            catch (Exception ex)
            {
            }

        }
        private void DgvItems_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvItems.Columns[e.ColumnIndex].Name != "listweight") return;

            // 1. Vẽ nền trắng mặc định của cell trước (tạo lớp viền trắng bao quanh)
            e.PaintBackground(e.ClipBounds, true);

            // 2. Thiết lập thông số màu sắc chính xác theo hình
            Color boxBgColor = Color.FromArgb(225, 225, 225); // Màu xám nhạt của khối
            Color borderColor = Color.FromArgb(160, 160, 160); // Màu viền xám
            Color arrowColor = Color.FromArgb(100, 100, 100);  // Màu mũi tên nhạt hơn một chút

            // 3. Tính toán hình chữ nhật cho khối nội dung (thụt vào 3-4 pixel mỗi cạnh)
            int paddingX = 4;
            int paddingY = 4;
            Rectangle contentRect = new Rectangle(
                e.CellBounds.X + paddingX,
                e.CellBounds.Y + paddingY,
                e.CellBounds.Width - (paddingX * 2),
                e.CellBounds.Height - (paddingY * 2)
            );

            // 4. Vẽ khối màu xám
            using (var bgBrush = new SolidBrush(boxBgColor))
            {
                e.Graphics.FillRectangle(bgBrush, contentRect);
            }

            // 5. Vẽ viền cho khối màu xám
            using (var borderPen = new Pen(borderColor))
            {
                e.Graphics.DrawRectangle(borderPen, contentRect.X, contentRect.Y, contentRect.Width - 1, contentRect.Height - 1);
            }

            // 6. Vẽ khu vực nút mũi tên
            int arrowBtnWidth = 22;
            Rectangle arrowRect = new Rectangle(
                contentRect.Right - arrowBtnWidth,
                contentRect.Y,
                arrowBtnWidth,
                contentRect.Height
            );

            // Đường kẻ dọc ngăn cách bên trong khối xám
            using (var sepPen = new Pen(borderColor))
            {
                e.Graphics.DrawLine(sepPen, arrowRect.Left, arrowRect.Top, arrowRect.Left, arrowRect.Bottom - 1);
            }

            // 7. Vẽ mũi tên chữ V mảnh (Căn giữa nút)
            int centerX = arrowRect.Left + (arrowRect.Width / 2);
            int centerY = arrowRect.Top + (arrowRect.Height / 2);

            using (var arrowPen = new Pen(arrowColor, 1.2f)) // Nét mảnh 1.2
            {
                Point[] vPoints = {
            new Point(centerX - 4, centerY - 2),
            new Point(centerX, centerY + 2),
            new Point(centerX + 4, centerY - 2)
        };
                e.Graphics.DrawLines(arrowPen, vPoints);
            }

            // 8. Vẽ Text (Trọng lượng)
            string displayValue = e.Value?.ToString() ?? "";
            Rectangle textRect = new Rectangle(contentRect.X + 5, contentRect.Y, contentRect.Width - arrowBtnWidth - 5, contentRect.Height);

            TextRenderer.DrawText(e.Graphics, displayValue, e.CellStyle.Font ?? dgvItems.Font,
                textRect, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            // Đánh dấu đã xử lý
            e.Handled = true;
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

        //private async void MonitorListenerServer()
        //{
        //    try
        //    {
        //        await StartListenerServer();
        //    }
        //    catch (Exception exx)
        //    {
        //        System.Windows.MessageBox.Show("ERROR: " + exx);
        //    }
        //}
        private async void MonitorListenerServer()
        {
            // Thử lần lượt các port: 80, 8080, 8081, 8082
            // Cổng 80 thường bị IIS/Skype/Windows chiếm → fallback sang 8080+
            int[] ports = new int[] { 80, 8080, 8081, 8082, 8083 };

            foreach (int port in ports)
            {
                try
                {
                    await StartListenerServer(port);
                    return; // thành công → thoát
                }
                catch (HttpListenerException hex) when (hex.ErrorCode == 5 || hex.ErrorCode == 32 || hex.ErrorCode == 183)
                {
                    // ErrorCode 5   = Access Denied (cần quyền admin cho port 80)
                    // ErrorCode 32  = Port đang được dùng
                    // ErrorCode 183 = Prefix đã được đăng ký
                    Console.WriteLine($"[ListenerServer] Port {port} bi chiem, thu port tiep theo...");
                    continue;
                }
                catch (Exception ex)
                {
                    // Lỗi khác → log, không popup
                    Console.WriteLine($"[ListenerServer] Loi khong mong doi: {ex.Message}");
                    return;
                }
            }

            Console.WriteLine("[ListenerServer] Khong the mo bat ky port nao. Camera HTTP listener bi tat.");
        }

        private async Task StartListenerServer(int port = 80)
        {
            string ip = Shared.GetLocalIPAddress();

            // Port 80 dùng http://ip/ (không có port)
            // Port khác dùng http://ip:port/
            string prefix = port == 80
                ? $"http://{ip}/"
                : $"http://{ip}:{port}/";

            string[] prefixes = new string[] { prefix };
            Console.WriteLine($"[ListenerServer] Dang mo HTTP listener tren: {prefix}");

            var server = new CameraListenerServer(prefixes);
            await server.StartAsync();
        }
        private void dgvItems_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Commit ngay khi user chọn giá trị trong ComboBox cell
            // để .Value được cập nhật trước khi đọc
            if (dgvItems.IsCurrentCellDirty)
            {
                dgvItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Khi user chọn weight trong ComboBox cell → lưu ngay vào Settings.ProductWeight
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvItems.Columns[e.ColumnIndex].Name != "listweight") return;

            try
            {
                var cellValue = dgvItems.Rows[e.RowIndex].Cells["listweight"].Value;
                if (cellValue != null
                    && double.TryParse(cellValue.ToString(), out double w)
                    && w > 0)
                {
                    Settings.ProductWeight = w;
                    //ProjectLogger.WriteInfo($"ProductWeight updated from dgvItems: {w}");
                }
            }
            catch (Exception ex)
            {
                //ProjectLogger.WriteError("dgvItems_CellValueChanged error: " + ex.Message);
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
            try
            {
                LoadJobNameList();
            }
            catch (Exception) { }
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
                UserCreate = Shared.LoggedInUser?.FullName ?? "Unknown Admin",
                AutoLoad = true
            };
            listBoxJobList.Enabled = true;
            dgvListJobPrint.Enabled = true;
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
                UserCreate = Shared.LoggedInUser?.FullName,
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
                                                                    // Get Job name with extension
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

                // ── Fix CaoSu: InitJobModel cộng +1 vào NumberTotalsCode do IsFirstRowHeader=false
                // → override lại giá trị chính xác (144 hoặc 240), đồng thời bảo toàn PrintJobWeightRange
                if (!string.IsNullOrWhiteSpace(_currentPrintJobWeightRange))
                {
                    _JobModel.PrintJobWeightRange = _currentPrintJobWeightRange;
                    _JobModel.NumberTotalsCode = _NumberTotalsCode; // không +1
                    _currentPrintJobWeightRange = "";                // reset sau dùng
                }

                _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
                _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
                _JobModel.TemplatePrint = Settings.PrintTemplate;
                _JobModel.isPushedDatabase = isPushDatabase;
                _JobModel.IsFirstRowHeader = false;
                _JobModel.IsJobOnline = UserPermission.isOnline;


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

                            if ((_JobModel.CompareType == CompareType.Database && podFormat == "") || txtPODFormat.Text == "")
                            {
                                CuzMessageBox.Show(Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            _JobModel.TemplatePrint = templatePrint.Text;
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

                            if ((_JobModel.CompareType == CompareType.Database && podFormat == "") || txtPODFormat.Text == "")
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

                    // --- SYNCHRONIZE COUNTERS FROM EXISTING FILES BEFORE SAVING (.rvis) ---
                    try
                    {
                        // Printed response file -> NumberOfPrintedCodes
                        if (!string.IsNullOrWhiteSpace(_JobModel.PrintedResponePath))
                        {
                            string printedRespFull = Path.Combine(CommVariables.PathPrintedResponse, _JobModel.PrintedResponePath);
                            var printedLines = FileFuncs.ReadCodeData(printedRespFull);
                            _JobModel.NumberOfPrintedCodes = (printedLines?.Count ?? 0) > 0 ? Math.Max(0, printedLines.Count - 1) : 0;
                        }

                        // Sent printed file -> NumberOfSaaSSentCodes, NumberOfSAPSentCodes
                        if (!string.IsNullOrWhiteSpace(_JobModel.PrintedResponePath))
                        {
                            string sentPrintedFull = Path.Combine(CommVariables.PathSentDataPrinted, _JobModel.PrintedResponePath);
                            var sentPrintedLines = FileFuncs.ReadCodeData(sentPrintedFull);
                            if (sentPrintedLines != null && sentPrintedLines.Count > 0)
                            {
                                _JobModel.NumberOfSaaSSentCodes = sentPrintedLines.Count(item =>
                                    item.Length > PrintingValues.SentStatus &&
                                    item[PrintingValues.SentStatus].Equals("Sent", StringComparison.OrdinalIgnoreCase));

                                _JobModel.NumberOfSAPSentCodes = sentPrintedLines.Count(item =>
                                    item.Length > PrintingValues.SAPStatus &&
                                    item[PrintingValues.SAPStatus].Equals("success", StringComparison.OrdinalIgnoreCase));
                            }
                        }

                        // Checked result file -> NumberOfCheckSaaSSentCodes (fallback to checked count if sent file missing)
                        if (!string.IsNullOrWhiteSpace(_JobModel.CheckedResultPath))
                        {
                            string sentCheckedFull = Path.Combine(CommVariables.PathSentDataChecked, _JobModel.CheckedResultPath);
                            var sentCheckedLines = FileFuncs.ReadCodeData(sentCheckedFull);
                            if (sentCheckedLines != null && sentCheckedLines.Count > 0)
                            {
                                _JobModel.NumberOfCheckSaaSSentCodes = sentCheckedLines.Count(item =>
                                    item.Length > PrintingValues.SentStatus &&
                                    item[PrintingValues.SentStatus].Equals("Sent", StringComparison.OrdinalIgnoreCase));

                                _JobModel.NumberOfCheckSAPSentCodes = sentCheckedLines.Count(item =>
                                    item.Length > PrintingValues.SAPStatus &&
                                    item[PrintingValues.SAPStatus].Equals("success", StringComparison.OrdinalIgnoreCase));
                            }
                            else
                            {
                                // fallback: total checked lines in checked result file (minus header)
                                string checkedFull = Path.Combine(CommVariables.PathCheckedResult, _JobModel.CheckedResultPath);
                                var checkedLines = FileFuncs.ReadCodeData(checkedFull);
                                _JobModel.NumberOfCheckSaaSSentCodes = (checkedLines?.Count ?? 0) > 0 ? Math.Max(0, checkedLines.Count - 1) : 0;
                            }

                            // Pallet sent data -> NumberOfCodesInPallet (count of success entries)
                            string sentPalletFull = Path.Combine(CommVariables.PathSentDataPallet, _JobModel.CheckedResultPath);
                            var sentPalletLines = FileFuncs.ReadCodeData(sentPalletFull);
                            if (sentPalletLines != null && sentPalletLines.Count > 0)
                            {
                                _JobModel.NumberOfCodesInPallet = sentPalletLines.Count(item =>
                                    item.Length > PrintingValues.SaaSStatus &&
                                    item[PrintingValues.SaaSStatus].Equals("success", StringComparison.OrdinalIgnoreCase));
                            }
                        }
                    }
                    catch
                    {
                        // Ignore read errors so save still continues
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

                    // Save to .rvis (PathJobsApp)
                    _JobModel.SaveFile();

                    Shared.JobNameSelected = JobName + Shared.Settings.JobFileExtension;
                    _PODFormat.Clear();
                    listBoxPrintProductTemplate.ClearSelected();
                }

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
                        _FormMainPC = new FrmMainCaoSu(this);  // needed changed

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
            catch (Exception)
            {
                //CuzMessageBox.Show(Lang.NewJobCreationFailed, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                return;
            }
        }
        //private void SaveJob()
        //{
        //    try
        //    {
        //        _JobModel = InitJobModel();
        //        _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
        //        _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
        //        _JobModel.TemplatePrint = Settings.PrintTemplate;
        //        _JobModel.isPushedDatabase = isPushDatabase;
        //        _JobModel.IsFirstRowHeader = false;
        //        _JobModel.IsJobOnline = UserPermission.isOnline;

        //        _JobModel.CaoSuProduct = Settings.CaoSuProduct;
        //        _JobModel.productWeight = Settings.ProductWeight;
        //        _JobModel.LOTNumber = InputLotNumber.Text.Trim();

        //        if (_JobModel != null)   // Check current Job has null
        //        {
        //            string JobName = _JobModel.FileName;  // Check Job name is empty
        //            if (JobName == "")
        //            {
        //                //CuzMessageBox.Show(Lang.PleaseInputJobName, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                return;
        //            }

        //            if (_JobModel.PrinterSeries)
        //            {
        //                if (_JobModel.CompareType == CompareType.Database)
        //                {

        //                    string databasePath = _JobModel.DirectoryDatabase;  // Check Database
        //                    if (_JobModel.CompareType == CompareType.Database && databasePath == "")
        //                    {
        //                        CuzMessageBox.Show(Lang.PleaseSelectDatabasePath, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        return;
        //                    }

        //                    //_JobModel.PODFormat = "<field1>";
        //                    txtPODFormat.Text = "<field1>";
        //                    _JobModel.PODFormat = new List<PODModel>
        //                    {
        //                        new PODModel(1, txtPODFormat.Text, PODModel.TypePOD.FIELD, txtPODFormat.Text)
        //                    };  // Check POD format

        //                    string podFormat = _JobModel.PODFormat.ToString();   // Check POD format

        //                    if (_JobModel.CompareType == CompareType.Database && podFormat == "" || txtPODFormat.Text == "")
        //                    {
        //                        CuzMessageBox.Show(Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        return;
        //                    }

        //                    _JobModel.TemplatePrint = templatePrint.Text;

        //                    if (_JobModel != null && _JobModel.CompareType == CompareType.Database && !CheckExistTemplatePrint(_JobModel.TemplatePrint) && _JobModel.PrinterSeries)
        //                    {
        //                        //CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        //return;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (_JobModel.CompareType == CompareType.Database)
        //                {

        //                    string databasePath = _JobModel.DirectoryDatabase;  // Check Database
        //                    if (_JobModel.CompareType == CompareType.Database && databasePath == "")
        //                    {
        //                        CuzMessageBox.Show(Lang.PleaseSelectDatabasePath, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        return;
        //                    }

        //                    string podFormat = _JobModel.PODFormat.ToString();  // Check POD format

        //                    if (_JobModel.CompareType == CompareType.Database && podFormat == "" || txtPODFormat.Text == "")
        //                    {
        //                        CuzMessageBox.Show(Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        return;
        //                    }
        //                }
        //                else
        //                {
        //                    if (_JobModel.CompareType == CompareType.StaticText)
        //                    {
        //                        if (_JobModel.StaticText == "")
        //                        {
        //                            CuzMessageBox.Show(Lang.PleaseEnterTheStaticText, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                            return;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        _JobModel.StaticText = "";
        //                    }
        //                }
        //            }


        //            if (Shared.CheckJobHasExist(JobName))  // Check Job name has exist and confirm replace
        //            {
        //                JobModel tmpJob = Shared.GetJob(JobName + Shared.Settings.JobFileExtension);
        //                if (tmpJob != null)
        //                {
        //                    if (tmpJob.JobStatus == JobStatus.Deleted)
        //                    {
        //                        string oldJobPath = CommVariables.PathJobsApp + JobName + Shared.Settings.JobFileExtension;
        //                        string newJobPath = CommVariables.PathJobsApp + JobName + "_Old_" +
        //                            DateTime.Now.ToString("yyMMddHHmmss") + Shared.Settings.JobFileExtension;
        //                        try
        //                        {
        //                            File.Move(oldJobPath, newJobPath);
        //                        }
        //                        catch
        //                        {

        //                        }
        //                    }
        //                    else
        //                    {
        //                        string message = Lang.DoYouWantToReplaceExistingTemplate + "\r\n" + JobName + Shared.Settings.JobFileExtension;
        //                        DialogResult result = CuzMessageBox.Show(message, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //                        if (result == DialogResult.Yes)
        //                        {
        //                            // Continue execute code below
        //                        }
        //                        else
        //                        {
        //                            return;
        //                        }
        //                    }
        //                }
        //            }

        //            if (JobName != "")
        //            {
        //                Shared.DeleteJob(_JobModel);  // Perform delete Job file
        //            }

        //            _JobModel.SaveFile();

        //            Shared.JobNameSelected = JobName + Shared.Settings.JobFileExtension;
        //            _PODFormat.Clear();
        //            listBoxPrintProductTemplate.ClearSelected();
        //        }

        //        //DialogResult dialogResult = CuzMessageBox.Show(Lang.SuccessfulNewJobCreationStartTheProcess, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //        DialogResult dialogResult = DialogResult.Yes;
        //        if (dialogResult == DialogResult.Yes)
        //        {
        //            if (Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings && _JobModel.CompareType == CompareType.Database && _JobModel.PrinterSeries)
        //            {
        //                PrinterSettingsModel printerSettingsModel = Shared.GetSettingsPrinter();

        //                if (printerSettingsModel.PodDataType != 1)
        //                {
        //                    radOther.Checked = true;
        //                    txtFileName.Text = "";
        //                    UpdateUIClearJobInformation();
        //                    CuzMessageBox.Show(Lang.DataTypeMustBeRAWData, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                    return;
        //                }
        //            }

        //            Hide();
        //            _FormMainPC?.Dispose();
        //            if (_FormMainPC == null || _FormMainPC.IsDisposed)
        //            {
        //                _FormMainPC = new FrmMainCaoSu(this);  // needed changed

        //                _FormMainPC.Show();
        //            }
        //            else
        //            {
        //                if (_FormMainPC.WindowState == FormWindowState.Minimized)
        //                {
        //                    _FormMainPC.WindowState = FormWindowState.Normal;
        //                }

        //                _FormMainPC.Focus();
        //                _FormMainPC.BringToFront();
        //            }

        //            PrinterSupport(_JobModel.PrinterSeries, false);
        //            txtFileName.Text = "";

        //        }
        //        else
        //        {
        //            UpdateUIClearJobInformation();
        //        }
        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        //CuzMessageBox.Show(Lang.NewJobCreationFailed, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //        return;
        //    }
        //}
        //private void SaveJobCheck()
        //{
        //    try
        //    {
        //        _JobModel = InitJobModel();
        //        _JobModel.FirstGeneratedCodeIndex = FirstGeneratedCodeIndex;
        //        _JobModel.LastGeneratedCodeIndex = LastGeneratedCodeIndex;
        //        _JobModel.TemplatePrint = Settings.PrintTemplate;
        //        _JobModel.isPushedDatabase = isPushDatabase;
        //        _JobModel.IsFirstRowHeader = false;
        //        _JobModel.IsJobOnline = UserPermission.isOnline;

        //        _JobModel.CaoSuProduct = Settings.CaoSuProduct;
        //        _JobModel.productWeight = Settings.ProductWeight;
        //        _JobModel.LOTNumber = InputLotNumber.Text.Trim();

        //        if (_JobModel != null)   // Check current Job has null
        //        {
        //            string JobName = _JobModel.FileName;  // Check Job name is empty
        //            if (JobName == "")
        //            {
        //                //CuzMessageBox.Show(Lang.PleaseInputJobName, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                return;
        //            }

        //            if (_JobModel.PrinterSeries)
        //            {
        //                if (_JobModel.CompareType == CompareType.Database)
        //                {

        //                    string databasePath = _JobModel.DirectoryDatabase;  // Check Database
        //                    if (_JobModel.CompareType == CompareType.Database && databasePath == "")
        //                    {
        //                        CuzMessageBox.Show(Lang.PleaseSelectDatabasePath, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        return;
        //                    }

        //                    //_JobModel.PODFormat = "<field1>";
        //                    txtPODFormat.Text = "<field1>";
        //                    _JobModel.PODFormat = new List<PODModel>
        //                    {
        //                        new PODModel(1, txtPODFormat.Text, PODModel.TypePOD.FIELD, txtPODFormat.Text)
        //                    };  // Check POD format

        //                    string podFormat = _JobModel.PODFormat.ToString();   // Check POD format

        //                    if (_JobModel.CompareType == CompareType.Database && podFormat == "" || txtPODFormat.Text == "")
        //                    {
        //                        CuzMessageBox.Show(Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        return;
        //                    }

        //                    _JobModel.TemplatePrint = templatePrint.Text;

        //                    if (_JobModel != null && _JobModel.CompareType == CompareType.Database && !CheckExistTemplatePrint(_JobModel.TemplatePrint) && _JobModel.PrinterSeries)
        //                    {
        //                        //CuzMessageBox.Show(Lang.CheckExistTemplatePrinter, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        //return;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (_JobModel.CompareType == CompareType.Database)
        //                {

        //                    string databasePath = _JobModel.DirectoryDatabase;  // Check Database
        //                    if (_JobModel.CompareType == CompareType.Database && databasePath == "")
        //                    {
        //                        CuzMessageBox.Show(Lang.PleaseSelectDatabasePath, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        return;
        //                    }

        //                    string podFormat = _JobModel.PODFormat.ToString();  // Check POD format

        //                    if (_JobModel.CompareType == CompareType.Database && podFormat == "" || txtPODFormat.Text == "")
        //                    {
        //                        CuzMessageBox.Show(Lang.PleaseSelectPODFormat, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                        return;
        //                    }
        //                }
        //                else
        //                {
        //                    if (_JobModel.CompareType == CompareType.StaticText)
        //                    {
        //                        if (_JobModel.StaticText == "")
        //                        {
        //                            CuzMessageBox.Show(Lang.PleaseEnterTheStaticText, Lang.Confirm, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                            return;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        _JobModel.StaticText = "";
        //                    }
        //                }
        //            }


        //            if (Shared.CheckJobHasExist(JobName))  // Check Job name has exist and confirm replace
        //            {
        //                JobModel tmpJob = Shared.GetJob(JobName + Shared.Settings.JobFileExtension);
        //                if (tmpJob != null)
        //                {
        //                    if (tmpJob.JobStatus == JobStatus.Deleted)
        //                    {
        //                        string oldJobPath = CommVariables.PathJobsApp + JobName + Shared.Settings.JobFileExtension;
        //                        string newJobPath = CommVariables.PathJobsApp + JobName + "_Old_" +
        //                            DateTime.Now.ToString("yyMMddHHmmss") + Shared.Settings.JobFileExtension;
        //                        try
        //                        {
        //                            File.Move(oldJobPath, newJobPath);
        //                        }
        //                        catch
        //                        {

        //                        }
        //                    }
        //                    else
        //                    {
        //                        string message = Lang.DoYouWantToReplaceExistingTemplate + "\r\n" + JobName + Shared.Settings.JobFileExtension;
        //                        DialogResult result = CuzMessageBox.Show(message, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //                        if (result == DialogResult.Yes)
        //                        {
        //                            // Continue execute code below
        //                        }
        //                        else
        //                        {
        //                            return;
        //                        }
        //                    }
        //                }
        //            }

        //            if (JobName != "")
        //            {
        //                Shared.DeleteJob(_JobModel);  // Perform delete Job file
        //            }

        //            _JobModel.SaveFile();

        //            Shared.JobNameSelected = JobName + Shared.Settings.JobFileExtension;
        //            _PODFormat.Clear();
        //            listBoxPrintProductTemplate.ClearSelected();
        //        }

        //        //DialogResult dialogResult = CuzMessageBox.Show(Lang.SuccessfulNewJobCreationStartTheProcess, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //        DialogResult dialogResult = DialogResult.Yes;
        //        if (dialogResult == DialogResult.Yes)
        //        {
        //            if (Shared.Settings.PrinterList.FirstOrDefault().CheckAllPrinterSettings && _JobModel.CompareType == CompareType.Database && _JobModel.PrinterSeries)
        //            {
        //                PrinterSettingsModel printerSettingsModel = Shared.GetSettingsPrinter();

        //                if (printerSettingsModel.PodDataType != 1)
        //                {
        //                    radOther.Checked = true;
        //                    txtFileName.Text = "";
        //                    UpdateUIClearJobInformation();
        //                    CuzMessageBox.Show(Lang.DataTypeMustBeRAWData, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                    return;
        //                }
        //            }

        //            Hide();
        //            _FormMainPC?.Dispose();
        //            if (_FormMainPC == null || _FormMainPC.IsDisposed)
        //            {
        //                _FormMainPC = new FrmMainCaoSu(this);  // needed changed

        //                _FormMainPC.Show();
        //            }
        //            else
        //            {
        //                if (_FormMainPC.WindowState == FormWindowState.Minimized)
        //                {
        //                    _FormMainPC.WindowState = FormWindowState.Normal;
        //                }

        //                _FormMainPC.Focus();
        //                _FormMainPC.BringToFront();
        //            }

        //            PrinterSupport(_JobModel.PrinterSeries, false);
        //            txtFileName.Text = "";

        //        }
        //        else
        //        {
        //            UpdateUIClearJobInformation();
        //        }
        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        //CuzMessageBox.Show(Lang.NewJobCreationFailed, Lang.Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //        return;
        //    }
        //}

        private bool CreateJobCheck(string lotNumber, string productCode, string productName = "", double selectedWeight = 0)
        {
            try
            {
                if (dgvListJobPrint.SelectedItems.Count == 0)
                {
                    CustomMessageBox.Show("Vui lòng chọn một JobPrint trong danh sách!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                string jobPrintRaw = dgvListJobPrint.SelectedItems[0].ToString();
                string jobPrintName = Path.GetFileNameWithoutExtension(jobPrintRaw);
                string fileExt = Shared.Settings.JobFileExtension;
                if (string.IsNullOrWhiteSpace(fileExt))
                    fileExt = ".rvis";

                string jobPrintFilePath = Path.Combine(CommVariables.PathJobsApp, jobPrintName + fileExt);

                if (!File.Exists(jobPrintFilePath))
                {
                    CustomMessageBox.Show("Không tìm thấy file JobPrint:\n" + jobPrintFilePath, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                JobModel jobPrint = JobModel.LoadFile(jobPrintFilePath);
                if (jobPrint == null)
                {
                    CustomMessageBox.Show("Không thể đọc dữ liệu JobPrint!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(jobPrint.PrintedResponePath))
                {
                    DialogResult confirm = CustomMessageBox.Show(
                        "JobPrint chưa có dữ liệu đã in.\n" +
                        "JobCheck sẽ được tạo nhưng chưa có dữ liệu để check.\n\n" +
                        "Bạn có muốn tiếp tục không?",
                        "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm != DialogResult.Yes)
                        return false;
                }

                string safeProductCode = string.IsNullOrWhiteSpace(productCode)
                    ? "NoProduct"
                    : productCode.Replace("/", "-").Replace("\\", "-")
                                 .Replace(":", "-").Replace("*", "-")
                                 .Replace("?", "-").Replace("\"", "-")
                                 .Replace("<", "-").Replace(">", "-")
                                 .Replace("|", "-").Trim();

                string safeJobPrintName = jobPrintName.Length > 20
                    ? jobPrintName.Substring(0, 25)
                    : jobPrintName;

                string offlineFlag = UserPermission.isOnline ? "" : "_Offline";
                string jobCheckFileName = string.Format("CHECK_{0}_{1}_{2}_{3}{4}",
                    safeJobPrintName,
                    lotNumber,
                    safeProductCode,
                    DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                    offlineFlag);

                // Ưu tiên weight từ dgvItems listweight cell;
                // fallback về jobPrint.productWeight nếu chưa chọn
                double weightToSave = selectedWeight > 0 ? selectedWeight : jobPrint.productWeight;

                var jobCheck = new JobModel
                {
                    FileName = jobCheckFileName,
                    DirectoryDatabase = jobPrint.DirectoryDatabase,
                    PrintedResponePath = jobPrint.PrintedResponePath ?? "",
                    PODFormat = jobPrint.PODFormat,
                    CaoSuProduct = jobPrint.CaoSuProduct,
                    productWeight = weightToSave,
                    PrintJobWeightRange = jobPrint.PrintJobWeightRange, // ← copy từ JobPrint
                    LOTNumber = lotNumber,
                    LastCheckedLot = lotNumber,
                    LastCheckedProductCode = productCode,
                    LastCheckedProductName = productName,
                    LastCheckedPrintJob = jobPrint.FileName,
                    NumberTotalsCode = jobPrint.NumberTotalsCode,
                    IsFirstRowHeader = jobPrint.IsFirstRowHeader,
                    FirstGeneratedCodeIndex = jobPrint.FirstGeneratedCodeIndex,
                    LastGeneratedCodeIndex = jobPrint.LastGeneratedCodeIndex,
                    PrinterSeries = false,
                    CompareType = CompareType.Database,
                    JobType = JobType.StandAlone,
                    IsJobOnline = UserPermission.isOnline,
                    isPushedDatabase = false,
                    JobStatus = JobStatus.NewlyCreated,
                    TemplatePrint = "",
                    UserCreate = Shared.LoggedInUser?.FullName ?? "Unknown",
                };

                string checkDir = CommVariables.PathJobsCheckApp;
                if (!Directory.Exists(checkDir))
                    Directory.CreateDirectory(checkDir);

                string savedPath = Path.Combine(checkDir.TrimEnd('\\'), jobCheckFileName + fileExt);

                var xs = new System.Xml.Serialization.XmlSerializer(typeof(JobModel));
                using (var sw = new System.IO.StreamWriter(savedPath, false, System.Text.Encoding.UTF8))
                {
                    xs.Serialize(sw, jobCheck);
                }

                if (!File.Exists(savedPath))
                {
                    CustomMessageBox.Show("Lưu file JobCheck thất bại!\nPath: " + savedPath,
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var frmCheck = new FrmMainCaoSuCheck(
                    this,
                    jobCheck,
                    lotNumber,
                    productCode,
                    jobPrint.DirectoryDatabase
                );
                frmCheck.Show();
                this.Hide();

                LoadJobCheckList();
                return true;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Lỗi khi tạo JobCheck:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ProjectLogger.WriteError("Error in CreateJobCheck: " + ex.Message + "\n" + ex.StackTrace);
                return false;
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




        private readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(3); // giới hạn concurrency


        // Sửa 2: LoadJobNameList() - bảo vệ Invoke khi form chưa sẵn sàng
        private void LoadJobNameList()
        {
            if (_IsProcessing)
            {
                return;
            }
            try
            {
                // Kiểm tra form còn hợp lệ trước khi Invoke
                if (!IsHandleCreated || IsDisposed) return;

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

                        // Kiểm tra form còn hợp lệ trước mỗi Invoke trong thread
                        if (!IsHandleCreated || IsDisposed) { _IsProcessing = false; return; }

                        Invoke(new Action(() =>
                        {
                            listBoxJobList.Items.Clear();
                            dgvListJobPrint.Items.Clear();
                            dgvListJobCheck.Items.Clear();
                            LoadJobCheckList();
                        }));

                        _JobNameList = null;
                        _JobNameList = Shared.GetJobNameList();

                        // Build set of referenced print jobs by existing JobCheck files (normalized without extension)
                        var referencedPrintJobs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        try
                        {
                            string checkDir = CommVariables.PathJobsCheckApp;
                            string fileExt = Shared.Settings.JobFileExtension;
                            if (Directory.Exists(checkDir))
                            {
                                var checkFiles = Directory.GetFiles(checkDir, "*" + fileExt);
                                foreach (var f in checkFiles)
                                {
                                    try
                                    {
                                        var xs = new System.Xml.Serialization.XmlSerializer(typeof(JobModel));
                                        using (var sr = new StreamReader(f))
                                        {
                                            var checkJob = (JobModel)xs.Deserialize(sr);
                                            if (checkJob != null && !string.IsNullOrWhiteSpace(checkJob.LastCheckedPrintJob))
                                            {
                                                referencedPrintJobs.Add(Path.GetFileNameWithoutExtension(checkJob.LastCheckedPrintJob).Trim());
                                            }
                                        }
                                    }
                                    catch { /* ignore bad check files */ }
                                }
                            }
                        }
                        catch { /* ignore scanning errors */ }

                        // Kiểm tra form còn hợp lệ trước khi update UI
                        if (!IsHandleCreated || IsDisposed) { _IsProcessing = false; return; }

                        Invoke(new Action(() =>
                        {
                            if (_JobNameList != null)
                            {
                                foreach (string JobName in _JobNameList)
                                {
                                    JobModel jobModel = Shared.GetJob(JobName);
                                    if (jobModel == null || jobModel.JobStatus == JobStatus.Deleted)
                                        continue;

                                    string nameOnly = Path.GetFileNameWithoutExtension(JobName);
                                    if (nameOnly.StartsWith("CHECK_", StringComparison.OrdinalIgnoreCase))
                                        continue;

                                    string normalizedJobName = nameOnly.Trim();

                                    listBoxJobList.Items.Add(JobName);

                                    // ── Chỉ hiển thị JobPrint chưa được dùng để tạo JobCheck ──
                                    if (!referencedPrintJobs.Contains(normalizedJobName))
                                    {
                                        // Nếu tài khoản offline thì chỉ hiển thị job offline
                                        if (!UserPermission.isOnline && !jobModel.IsJobOnline)
                                            dgvListJobPrint.Items.Add(JobName);

                                        // Nếu tài khoản online thì hiển thị job online
                                        else if (UserPermission.isOnline && jobModel.IsJobOnline)
                                            dgvListJobPrint.Items.Add(JobName);
                                    }
                                }
                            }
                        }));
                        _IsProcessing = false;
                        Thread.Sleep(5);

                        if (!IsHandleCreated || IsDisposed) return;

                        Invoke(new Action(() =>
                        {
                            picLoading.Visible = true;
                        }));

                        UpdateUILoadJobNameList(true);
                    }
                    catch (Exception ex)
                    {
                        _IsProcessing = false;
                        System.Diagnostics.Debug.WriteLine("error in thread: " + ex.Message);
                    }
                });
                threadLoadJobNameList.IsBackground = true;
                threadLoadJobNameList.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error: " + ex.Message);
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
                    //    LoggingController.SaveHistory(  //Save history
                    //        Lang.Exit,
                    //        Lang.LogOut,
                    //        Lang.LogoutSuccessfully,
                    //        SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                    //        LoggingType.LogedOut);

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

        private void tabGetPO_Click(object sender, EventArgs e)
        {

        }

        private void listBoxJobList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnNext_Click(object sender, EventArgs e)
        {

        }

        private void saveJobCaoSuPrint_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void dgvListJobPrint_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnRefreshHistory_Click(object sender, EventArgs e)
        {
            cbbHisFilterType.SelectedIndex = 0;
            DisplayHistoryByFilter();
        }
    }

}
