using BarcodeVerificationSystem.Controller;
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
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.View.OtherProjects.THMilkUI;
using BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess;
using BarcodeVerificationSystem.View.UtilityForms.THTrueMilk;
using CommonVariable;
using DesignUI.CuzAlert;
using GenCode.Utils;
using Newtonsoft.Json;
using OperationLog.Controller;
using OperationLog.Model;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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

            //lblTotalQrcode.Text = $"SP dự kiến: {(_SelectedJob?.NumberTotalsCode ?? 0):N0}";
            //lblTotalTon.Text = $"Số tấn dự kiến: {(_SelectedJob?.EstimatedTons ?? 0):N2}";
        }

        private void UpdateJobInfo(JobModel jobModel)
        {
            Shared.CurrentJob = jobModel;

            txtJobName.Text = jobModel.FileName;
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

            double actualTons = 0;
            double expectedCodes = _SelectedJob?.NumberTotalsCode ?? 0;
            double expectedTons = _SelectedJob?.EstimatedTons ?? 0;
            if (expectedCodes > 0 && expectedTons > 0)
                actualTons = NumberPrinted * expectedTons / expectedCodes;
            lblTotalQrcode.Text = $"{_SelectedJob?.TotalRlinkPrinted ?? 0:N0}";
            lblTotalTon.Text = $"Tấn: {expectedTons:N2} / {actualTons:N2}";
        }

        private void btnDestroyCarton_Click(object sender, EventArgs e)
        {
            var productListForm = new PLCProductList();
            productListForm.ShowDialog();
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
                        if (_ParentForm != null)
                            _ParentForm.isShowPopupDisConOneTime = false; // Reset để lần sau mất kết nối vẫn thông báo
                    }
                    else
                    {
                        ShowLabelIcon(labelStatusCamera, Lang.CameraTMP, Properties.Resources.icons8_camera_30px_disconnected);

                        // Tạm comment alert camera gốc — đã có thông báo mới sau 10s
                        // bool isRunning = Shared.OperStatus == OperationStatus.Running
                        //                   || Shared.OperStatus == OperationStatus.Processing;
                        // if (!cameraModel.IsConnected && !_ParentForm.isShowPopupDisConOneTime && isRunning)
                        // {
                        //     if (ProjectLabel.IsNutrifood && !Shared.Settings.IsManufacturingMode)
                        //         return;
                        //     _ParentForm.isShowPopupDisConOneTime = true;
                        //     CuzAlert.Show(Lang.CameraDisconnected,
                        //         Alert.enmType.Warning,
                        //         new Size(500, 120),
                        //         new Point(Location.X, Location.Y),
                        //         Size,
                        //         true);
                        // }
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
                        // Tạm comment alert máy in gốc — đã có thông báo mới sau 10s
                        // if (!printerModel.IsConnected && _IsPrinterDisconnectedNot)
                        // {
                        //     LogDisconnect("TIMER_POLLING");
                        //     CuzAlert.Show(Lang.PrinterDisconnected, Alert.enmType.Warning, new Size(500, 120), new Point(Location.X, Location.Y), this.Size, true);
                        // }

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
            if (_SelectedJob != null && _SelectedJob.CompleteJobStatus == CompleteJobStatus.Completed)
            {

                btnStart.Enabled = false;

            }
            else if (IsDefaultSupportAccount())
            {
                btnStart.Visible = false;
            }
            else btnStart.Enabled = isEnable;

            btnStop.Enabled = !isEnable;

            btnTrigger.Enabled = !isEnable;
            btnJob.Enabled = isEnable;
            btnAccount.Enabled = isEnable;
            btnHistory.Enabled = isEnable;
            //btnSettings.Enabled = isEnable;
            btnExportData.Enabled = isEnable;
            btnExportResult.Enabled = isEnable;
            btnExportAll.Enabled = isEnable;
            btnExit.Enabled = isEnable;
            btnAddBarcode.Enabled = isEnable && Shared.UserPermission.IncreaseProductionQuantity;
            bool isCompleted = _SelectedJob?.CompleteJobStatus == CompleteJobStatus.Completed;
            txtAddTotalTon.ReadOnly = !isEnable || isCompleted;
            btnAddTons.Enabled = isEnable && !isCompleted;
            // END menu script
            if (isEnable)
            {
                ProcessUserAccess();
            }

            if (!isNonStart)
            {
                toolStripOperationStatus.Text = operationStatus.ToFriendlyString();
                toolStripOperationStatus.ForeColor = operationStatus.GetForegroundColor();

                lblStatusOperator.Text = operationStatus.ToFriendlyString();
                lblStatusOperator.ForeColor = operationStatus.GetForegroundColor();
              
            }
        }

        void EnableUIComponentWhenLoadData(bool isEnable)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableUIComponentWhenLoadData(isEnable)));
                return;
            }

            // ── Khi re-enable: restore trạng thái panel cha ──────────────
            if (isEnable)
                pnlControllButton.Visible = Shared.UserPermission.Controls;
            if (_SelectedJob != null && _SelectedJob.CompleteJobStatus == CompleteJobStatus.Completed)
            {

                btnStart.Enabled = false;

            }
            else if (IsDefaultSupportAccount())
            {
                btnStart.Visible = false;
            }
            else
            {
                btnStart.Enabled = isEnable;
            }

            btnStop.Enabled = !isEnable;
            btnTrigger.Enabled = !isEnable;
            btnJob.Enabled = isEnable;
            btnAccount.Enabled = isEnable && Shared.UserPermission.Accounts;
            btnHistory.Enabled = isEnable;
            btnSettings.Enabled = Shared.UserPermission.Settings || Shared.UserPermission.ViewSetting;
            btnExportData.Enabled = isEnable;
            btnExit.Enabled = isEnable;
            bool isCompleted2 = _SelectedJob?.CompleteJobStatus == CompleteJobStatus.Completed;
            txtAddTotalTon.ReadOnly = !isEnable || isCompleted2;
            btnAddTons.Enabled = isEnable && !isCompleted2;
            btnDatabase.Enabled = isEnable;
            pnlPrintedCode.Enabled = isEnable;
            pnlTotalChecked.Enabled = isEnable;
            pnlCheckPassed.Enabled = isEnable;
            pnlCheckFailed.Enabled = isEnable;
            dgvDatabase.Enabled = isEnable;
            dgvCheckedResult.Enabled = isEnable;
            picDatabaseLoading.Visible = !isEnable;
            picCheckedResultLoading.Visible = !isEnable;
            // ── btnCompleteJob: chỉ enable khi spinner tắt + job chưa hoàn thành ──
            if (_SelectedJob?.CompleteJobStatus == CompleteJobStatus.Completed)
                btnCompleteJob.Enabled = false;
            else
                btnCompleteJob.Enabled = isEnable && !picCheckedResultLoading.Visible && !picDatabaseLoading.Visible;
        }

        private void ProcessUserAccess()
        {
            if (Shared.LoggedInUser == null) { }
            else if (Shared.LoggedInUser.Role == 0) { }
            else if (Shared.LoggedInUser.Role == 1) { }
        }

        #endregion Update UI 
    }
}
