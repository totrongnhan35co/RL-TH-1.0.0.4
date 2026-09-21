using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.UserPermission;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.View.UcSettings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View.CaoSuDongNaiUI
{
    public partial class FrmSettingsCaoSu : Form
    {
        #region Properties
        private readonly List<ToolStripLabel> _LabelStatusCameraList = new List<ToolStripLabel>();
        private readonly List<ToolStripLabel> _LabelStatusPrinterList = new List<ToolStripLabel>();
        private readonly Timer _DateTimeTicker = new Timer();
        private readonly string _DateTimeFormat = "yyyy/MM/dd hh:mm:ss tt";
        private const int CS_DropShadow = 0x00020000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ClassStyle = CS_DropShadow;
                return createParams;
            }
        }

        #endregion Properties

        public FrmSettingsCaoSu()
        {
            InitializeComponent();
            InitControls();
            SetLanguage();
            SetTabPagesContentEnabled(tabControlSettings, Shared.OperStatus == OperationStatus.Stopped); //  

        }
        private void SetTabPagesContentEnabled(TabControl tabControl, bool enable)
        {
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                foreach (Control ctrl in tabPage.Controls)
                {
                    ctrl.Enabled = enable;
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            InitEvents();
        }
        private void InitControls()
        {
            _DateTimeTicker.Start();
            // Show icon camera status
            _LabelStatusCameraList.Add(lblStatusCamera01);
            UpdateStatusLabelCamera();

            // Show icon printer status
            _LabelStatusPrinterList.Add(lblStatusPrinter01);
            UpdateStatusLabelPrinter();

            // Show icon sensor controller status
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);

            // Show icon serial device status
            UpdateUISerialDeviceControllerStatus(Shared.IsSerialDeviceConnected);
            //Initial tab settings
            tabPageSystemSettings.Controls.Clear();
            UcSystemSettings ucSystemSettings = new UcSystemSettings();
            ucSystemSettings.Dock = DockStyle.Fill;
            tabPageSystemSettings.Controls.Add(ucSystemSettings);
            //END Initial tab settings

            // Initial tab camera settings
            tabPageCameraSettings.Controls.Clear();
            for (int i = 0; i < Shared.Settings.CameraList.Count; i++)
            {
                UcCameraSettings ucCameraSettings = new UcCameraSettings
                {
                    Index = Shared.Settings.CameraList[i].Index,
                    Dock = DockStyle.Top
                };
                tabPageCameraSettings.Controls.Add(ucCameraSettings);
                ucCameraSettings.BringToFront();
            }
            // END Intial tab camera settings

            // Initial tab camera settings
            tabPagePrinterSettings.Controls.Clear();
            for (int i = 0; i < Shared.Settings.PrinterList.Count; i++)
            {
                UcPrinterSettings ucPrinterSettings = new UcPrinterSettings
                {
                    Index = Shared.Settings.PrinterList[i].Index,
                    Dock = DockStyle.Top
                };
                tabPagePrinterSettings.Controls.Add(ucPrinterSettings);
                ucPrinterSettings.BringToFront();
            }
            // END Intial tab camera settings

            //Initial tab camera settings
            tabPageSensorController.Controls.Clear();
            UcSensorSettings ucSensorSettings = new UcSensorSettings();
            ucSensorSettings.Dock = DockStyle.Fill;
            tabPageSensorController.Controls.Add(ucSensorSettings);
            //END Initial tab camera settings

            //Initial tab Serial Device settings
            tabPageSerialDevice.Controls.Clear();
            ucSerialDeviceSettings usSerialDeviceSettings = new ucSerialDeviceSettings();
            usSerialDeviceSettings.Dock = DockStyle.Fill;
            tabPageSerialDevice.Controls.Add(usSerialDeviceSettings);
            //END Initial tab Serial Device settings

            // string currentUser = SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember");
            string currentUser = Shared.LoggedInUser != null
                        ? SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember")
                     : string.Empty;


            if (currentUser == "Support")
            {
                this.tabPageProductionSetting = new System.Windows.Forms.TabPage();
                this.tabControlSettings.Controls.Add(this.tabPageProductionSetting);
                this.tabPageProductionSetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.tabPageProductionSetting.Location = new System.Drawing.Point(4, 44);
                this.tabPageProductionSetting.Name = "tabPageAPISetting";
                this.tabPageProductionSetting.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
                this.tabPageProductionSetting.Size = new System.Drawing.Size(996, 484);
                this.tabPageProductionSetting.TabIndex = 1;
                this.tabPageProductionSetting.Text = Lang.ProductionSettings;
                this.tabPageProductionSetting.UseVisualStyleBackColor = true;
                tabPageProductionSetting.Text = "Cài đặt thông tin Line";

                //Initial tab api settings
                tabPageProductionSetting.Controls.Clear();
                ucLineSettingCaoSu ucApiSetting = new ucLineSettingCaoSu();
                ucApiSetting.Dock = DockStyle.Fill;
                tabPageProductionSetting.Controls.Add(ucApiSetting);
            }
            this.tabControlSettings.Controls.Remove(this.tabPageSystemSettings);
        }
        private void InitEvents()
        {
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnCameraStatusChange += Shared_OnCameraStatusChange;
            Shared.OnPrinterStatusChange += Shared_OnPrinterStatusChange;
            Shared.OnSensorControllerChangeEvent += Shared_OnSensorControllerChangeEvent;
            Shared.OnSerialDeviceControllerChangeEvent += Shared_OnSerialDeviceControllerChangeEvent;
            _DateTimeTicker.Tick += TimerDateTime_Tick;
        }

        /// <summary>
        /// Update current time delay check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerDateTime_Tick(object sender, EventArgs e)
        {
            toolStripDateTime.Text = DateTime.Now.ToString(_DateTimeFormat);
        }

        /// <summary>
        /// Invoke language change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        /// <summary>
        /// Set user interface language
        /// </summary>
        private void SetLanguage()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetLanguage()));
                return;
            }
            tabPageCameraSettings.Text = Lang.CameraSettings;
            tabPageSystemSettings.Text = Lang.SystemSettings;
            tabPagePrinterSettings.Text = Lang.PrinterSettings;
            tabPageSensorController.Text = Lang.PLCSettings;
            tabPageSerialDevice.Text = Lang.ScannerSettings;
            lblFormName.Text = Lang.Settings;
            toolStripVersion.Text = Lang.Version + ": " + Properties.Settings.Default.SoftwareVersion;
            lblSensorControllerStatus.Text = Lang.PLCLabel;
            lblStatusCamera01.Text = Lang.CameraTMP;
            lblStatusPrinter01.Text = Lang.Printer;
        }

        /// <summary>
        /// Invoke printer status change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shared_OnPrinterStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelPrinter();
        }

        /// <summary>
        /// Invoke camera status change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shared_OnCameraStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelCamera();
        }

        /// <summary>
        /// Invoke sensor status change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shared_OnSensorControllerChangeEvent(object sender, EventArgs e)
        {
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);
        }

        /// <summary>
        ///  Update Camera connection status icon for connect (green), disconnect (red)
        /// </summary>
        /// 

        /// <summary>
        /// Invoke SerialDevice status change
        /// </summary>
        private void Shared_OnSerialDeviceControllerChangeEvent(object sender, EventArgs e)
        {
            UpdateUISerialDeviceControllerStatus(Shared.IsSerialDeviceConnected);
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
                    }
                }
            }
        }

        private void ShowLabelIcon(ToolStripLabel label, String text, Image icon)
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

        /// <summary>
        /// Update Printer connection status icon for connect (green), disconnect (red)
        /// </summary>
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
                    //string printerName = string.Format("{0} {1}",Lang.Printer,i + 1);
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

        /// <summary>
        /// Update Sensor connection status icon for connect (green), disconnect (red)
        /// </summary>
        /// <param name="isConnect"></param>
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
    }
}
