using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.UserPermission;
using BarcodeVerificationSystem.View.CaoSuDongNaiUI;
using BarcodeVerificationSystem.View.UcSettings;
using BarcodeVerificationSystem.View.WokaUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmSettings : Form
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

        public FrmSettings()
        {
            InitializeComponent();
            InitControls();
            SetLanguage();

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
            UpdateStatusLabelZebraPrinter();

            // Show icon sensor controller status
            UpdateUISensorControllerStatus(Shared.IsSensorControllerConnected);

            // Show icon serial device status
            UpdateUISerialDeviceControllerStatus(Shared.IsSerialDeviceConnected);

            // Show icon database status
            UpdateStatusLabelDatabase();
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

            //Initial tab Serial Device settings
            tabPageZebraPrinter.Controls.Clear();
            ucZebraPrinterSetting usZebraPrinterSettings = new ucZebraPrinterSetting();
            usZebraPrinterSettings.Dock = DockStyle.Fill;
            tabPageZebraPrinter.Controls.Add(usZebraPrinterSettings);
            //END Initial tab Serial Device settings

            string currentUser = SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember");


            if (ProjectLabel.IsNutrifood && (Shared.UserPermission.ProductionSettings || Shared.UserPermission.ViewSetting || currentUser == "Support"))
            {
                this.tabPageProductionSetting = new System.Windows.Forms.TabPage();
                this.tabControlSettings.Controls.Add(this.tabPageProductionSetting);
                this.tabPageProductionSetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.tabPageProductionSetting.Location = new System.Drawing.Point(4, 44);
                this.tabPageProductionSetting.Name = "tabPageAPISetting";
                this.tabPageProductionSetting.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
                this.tabPageProductionSetting.Size = new System.Drawing.Size(996, 484);
                this.tabPageProductionSetting.TabIndex = 1;
                this.tabPageProductionSetting.Text = "Cài đặt môi trường";
                this.tabPageProductionSetting.UseVisualStyleBackColor = true;

                //Initial tab api settings
                tabPageProductionSetting.Controls.Clear();
                ucProductionNutiSetting ucApiSetting = new ucProductionNutiSetting();
                ucApiSetting.Dock = DockStyle.Fill;
                tabPageProductionSetting.Controls.Add(ucApiSetting);
            }

            if (ProjectLabel.IsNutrifood)
            {
                this.tabControlSettings.Controls.Remove(this.tabPageSystemSettings);
                this.tabControlSettings.Controls.Remove(this.tabPageCameraSettings);
                this.tabControlSettings.Controls.Remove(this.tabPageSensorController);

                //this.tabControlSettings.Controls.Add(this.tabPagePrinterSettings);
                //this.tabControlSettings.Controls.Add(this.tabPageSerialDevice);
            }

            if (ProjectLabel.IsWoka)
            {
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
                    this.tabPageProductionSetting.Text = "Cấu hình Line";
                    this.tabPageProductionSetting.UseVisualStyleBackColor = true;

                    //Initial tab api settings
                    tabPageProductionSetting.Controls.Clear();
                    ucLineSettingWoka ucApiSetting = new ucLineSettingWoka();
                    ucApiSetting.Dock = DockStyle.Fill;
                    tabPageProductionSetting.Controls.Add(ucApiSetting);
                }

                lblStatusSerialDevice.Visible = false;
                this.tabControlSettings.Controls.Remove(this.tabPageSystemSettings);
                //this.tabControlSettings.Controls.Remove(this.tabPageSerialDevice);
            }
            if (ProjectLabel.IsDefault)
            {

                this.tabControlSettings.Controls.Remove(this.tabPageZebraPrinter);

            }
            this.tabControlSettings.Controls.Remove(this.tabPaggQRDroco);
            if (ProjectLabel.IsDroco)
            {
                this.tabControlSettings.Controls.Remove(this.tabPageSystemSettings);

                // Nạp UserControl vào tab đã có sẵn trong Designer
                this.tabPaggQRDroco.Controls.Clear();
                var ucQR = new BarcodeVerificationSystem.View.OtherProjects.DrocoUI.ucDrocoQRFieldMapping
                {
                    Dock = DockStyle.Fill
                };
                this.tabPaggQRDroco.Controls.Add(ucQR);
            }
            //if (!ProjectLabel.IsDroco)
            //{
            //    this.tabControlSettings.Controls.Remove(this.tabPaggQRDroco);
            //}

            if (ProjectLabel.IsCenteryIndia)
            {
                this.tabPageProductionSetting = new System.Windows.Forms.TabPage();
                this.tabControlSettings.Controls.Add(this.tabPageProductionSetting);
                this.tabPageProductionSetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.tabPageProductionSetting.Location = new System.Drawing.Point(4, 44);
                this.tabPageProductionSetting.Name = "tabPageAPISetting";
                this.tabPageProductionSetting.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
                this.tabPageProductionSetting.Size = new System.Drawing.Size(996, 484);
                this.tabPageProductionSetting.TabIndex = 1;
                this.tabPageProductionSetting.Text = "Line Settings";
                this.tabPageProductionSetting.UseVisualStyleBackColor = true;

                //Initial tab api settings
                tabPageProductionSetting.Controls.Clear();
                ucLineSettingCentery ucApiSetting = new ucLineSettingCentery();
                ucApiSetting.Dock = DockStyle.Fill;
                tabPageProductionSetting.Controls.Add(ucApiSetting);

                lblStatusPrinterZebra.Visible = false;
               // this.tabControlSettings.Controls.Remove(this.tabPageSystemSettings);
                this.tabControlSettings.Controls.Remove(this.tabPageZebraPrinter);
            }
            if (ProjectLabel.IsTHMilk)
            {
                if (Shared.UserPermission.ProductionSettings || Shared.UserPermission.ViewSetting || currentUser != "Operator")
                {
                    this.tabPageProductionSetting = new System.Windows.Forms.TabPage();
                    this.tabControlSettings.Controls.Add(this.tabPageProductionSetting);
                    this.tabPageProductionSetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    this.tabPageProductionSetting.Location = new System.Drawing.Point(4, 44);
                    this.tabPageProductionSetting.Name = "tabPageProductionSetting";
                    this.tabPageProductionSetting.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
                    this.tabPageProductionSetting.Size = new System.Drawing.Size(996, 484);
                    this.tabPageProductionSetting.TabIndex = 1;
                    this.tabPageProductionSetting.Text = "Cài đặt môi trường";
                    this.tabPageProductionSetting.UseVisualStyleBackColor = true;
                    lblStatusDatabase.Visible = true;
                    tabPageProductionSetting.Controls.Clear();
                    var ucTHSetting = new BarcodeVerificationSystem.View.OtherProjects.THMilkUI.ucProductionTHSetting();
                    ucTHSetting.Dock = DockStyle.Fill;
                    tabPageProductionSetting.Controls.Add(ucTHSetting);
                }

                this.tabControlSettings.Controls.Remove(this.tabPageSystemSettings);
            }


        }
        private void InitEvents()
        {
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnCameraStatusChange += Shared_OnCameraStatusChange;
            Shared.OnPrinterStatusChange += Shared_OnPrinterStatusChange;
            Shared.OnZebraPrinterStatusChange += Shared_OnZebraPrinterStatusChange;
            Shared.OnSensorControllerChangeEvent += Shared_OnSensorControllerChangeEvent;
            Shared.OnSerialDeviceControllerChangeEvent += Shared_OnSerialDeviceControllerChangeEvent;
            Shared.OnDatabaseStatusChange += Shared_OnDatabaseStatusChange;
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

        private void Shared_OnZebraPrinterStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelZebraPrinter();
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
        /// Invoke database status change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shared_OnDatabaseStatusChange(object sender, EventArgs e)
        {
            UpdateStatusLabelDatabase();
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
        /// Update Zebra Printer connection status icon for connect (green), disconnect (red)
        /// </summary>
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

        /// <summary>
        /// Update Database connection status icon for connect (green), disconnect (red)
        /// </summary>
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
    }
}
