using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Controller.Camera.Keyence;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using static BarcodeVerificationSystem.Utils.UIControlsFuncs;
using static BarcodeVerificationSystem.Utils.ControlEvents.InitControlEvents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;
namespace BarcodeVerificationSystem.View
{
    public partial class UcCameraSettings : UserControl
    {
        private CameraModel _CameraModel;
        // ── Hikrobot OCR Mode Controls (created programmatically) ────────────
       

        private int _Index = 0;
        private bool _firstLoad = false;
        public int Index
        {
            get { return _Index; }
            set
            {
                _Index = value;
                _CameraModel = Shared.Settings.CameraList.Count > _Index ?
                    Shared.Settings.CameraList[_Index] : new CameraModel { Index = _Index };
            }
        }
        private bool _IsBinding = false;

        public UcCameraSettings()
        {
            InitializeComponent();
            Load += UcCameraSettings_Load;

        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
          
            InitEvents();
            SetLanguage();
        }
        /// <summary>
        /// Tạo các control OCR mode cho Hikrobot theo kiểu programmatic.
        /// Chỉ gọi 1 lần từ OnHandleCreated.
        /// </summary>
      
        private int convertToIndexResolution(int width, int heigth)
        {
            if (width.Equals(240) && heigth.Equals(160))
            {
                return 0;
            }
            if (width.Equals(320) && heigth.Equals(240))
            {
                return 1;
            }
            if (width.Equals(480) && heigth.Equals(360))
            {
                return 2;
            }
            return 0;
        }

        private (int width, int height) ConvertFromIndexResolution(int index)
        {
            switch (index)
            {
                case 0:
                    return (240, 160);
                case 1:
                    return (320, 240);
                case 2:
                    return (480, 360);
                default:
                    return (240, 160);
            }
        }

        private void InitVisibiltyForCamType(CameraType cameraType)
        {
            bool prevBinding = _IsBinding;
            _IsBinding = true;
            try
            {
                CameraBrandPanel.Visible = ProjectLabel.IsDefault || ProjectLabel.IsWoka || ProjectLabel.IsTHTrueMilk || ProjectLabel.IsCenteryIndia;
                HikrobotRad.Visible = ProjectLabel.IsDefault;
                if (ProjectLabel.IsCenteryIndia)
                {
                    KeyenceRad.Visible = false;
                    HikrobotRad.Visible = false;
                }
                InitCameraType();

                // Đồng bộ trạng thái RadioButtons với Model để đảm bảo UI chính xác
                bool isCognex = _CameraModel?.CameraBrand == Model.CameraBrand.Cognex;
                bool isKeyence = _CameraModel?.CameraBrand == Model.CameraBrand.Keyence;
                bool isHikrobot = _CameraModel?.CameraBrand == Model.CameraBrand.Hikrobot;
                if (ProjectLabel.IsCenteryIndia)
                {
                    _CameraModel.CameraBrand = Model.CameraBrand.Cognex;
                    CognexRad.Checked = true;
                }
                CognexRad.Checked = isCognex;
                KeyenceRad.Checked = isKeyence;
                HikrobotRad.Checked = isHikrobot;
                CameraBrandPanel.Visible = false;
                switch (cameraType)
                {
                    case CameraType.DM:
                        comboBoxCamType.SelectedIndex = 0;
                        OutputTypePanel.Enabled = true;
                        OutputTypePanel.Location = new System.Drawing.Point(336, 207);
                        CustomCommandPanel.Location = new System.Drawing.Point(326, 247);
                        break;
                    case CameraType.IS:
                        comboBoxCamType.SelectedIndex = 1;
                        CommandErrorBox.Checked = true;
                        OutputTypePanel.Enabled = false;
                        OutputTypePanel.Location = new System.Drawing.Point(30, 394);
                        CustomCommandPanel.Location = new System.Drawing.Point(335, 397);
                        break;
                    case CameraType.ISDual:
                        comboBoxCamType.SelectedIndex = 2;
                        CommandErrorBox.Checked = true;
                        OutputTypePanel.Enabled = false;
                        OutputTypePanel.Location = new System.Drawing.Point(30, 394);
                        CustomCommandPanel.Location = new System.Drawing.Point(335, 397);
                        break;
                    case CameraType.CV_X:
                        comboBoxCamType.SelectedIndex = 0;
                        CommandErrorBox.Checked = true;
                        OutputTypePanel.Enabled = false;
                        OutputTypePanel.Location = new System.Drawing.Point(30, 394);
                        CustomCommandPanel.Location = new System.Drawing.Point(5, 142);
                        break;
                    case CameraType.VS_C:
                        comboBoxCamType.SelectedIndex = 1;
                        CommandErrorBox.Checked = true;
                        OutputTypePanel.Enabled = false;
                        OutputTypePanel.Visible = false;
                        OutputTypePanel.Location = new System.Drawing.Point(30, 394);
                        CustomCommandPanel.Location = new System.Drawing.Point(5, 142);
                        break;
                    case CameraType.HIKROBOT:
                        comboBoxCamType.SelectedIndex = 0;
                        OutputTypePanel.Enabled = true;
                        OutputTypePanel.Location = new System.Drawing.Point(677,145);
                     
                        CustomCommandPanel.Location = new System.Drawing.Point(5, 142);

                        break;
                    default:
                        comboBoxCamType.SelectedIndex = 0;
                        OutputTypePanel.Enabled = true;
                        OutputTypePanel.Location = new System.Drawing.Point(336, 207);
                        CustomCommandPanel.Location = new System.Drawing.Point(326, 247);
                        break;
                }

                // Default port 8500 for Keyence
                if (cameraType == CameraType.CV_X || cameraType == CameraType.VS_C)
                {
                    if (string.IsNullOrEmpty(_CameraModel.Port) || _CameraModel.Port == "0")
                    {
                        _CameraModel.Port = "8500";
                    }
                    OutputTypePanel.Visible = false;
                }

                numCamPort.Value = int.TryParse(_CameraModel.Port, out int port) ? port : 0;
                EnablePosition.Checked = Shared.Settings.EnablePosition;
                itemsPerHour.Checked = Shared.Settings.IsItemsPerHour;
                radioBarcodePosition.Checked = Shared.Settings.Position == SettingsModel.PositionType.BarcodePosition;
                radioLogoPosition.Checked = Shared.Settings.Position == SettingsModel.PositionType.LogoPosition;

                labelMasterJobName.Text = cameraType.Equals(CameraType.IS) ? Lang.SingleJobFileName : Lang.MasterJobName;
                labelObjectNameMaster.Text = cameraType.Equals(CameraType.IS) ? Lang.ObjectNameSingle : Lang.ObjectNameMaster;

                VisibleControl(cameraType.Equals(CameraType.ISDual),
                    textBoxObjectNameSlave,
                    labelObjectNameSlave,
                    tableLayoutPanelObjectSym,
                    textBoxSlaveJobName,
                    labelSlaveJobName,
                    lblSlaveIp,
                    txtSlaveIPAddress);

                VisibleControl(cameraType.Equals(CameraType.IS), PositionPanel);
                CustomCommandPanel.Parent = this;
                CustomCommandPanel.BringToFront();
                OutputTypePanel.Parent = this;
                OutputTypePanel.BringToFront();

                comboBoxImageResolution.SelectedIndex = convertToIndexResolution(_CameraModel.WidthImage, _CameraModel.HeigthImage);
                comboBox_ModeReadCamera.SelectedIndex = (cameraType.Equals(CameraType.DM) && (Shared.Settings.CameraList.FirstOrDefault().ReadMode == CameraModeRead.Basic)) ? 0 : 1;
                txtIPAddress.Text = _CameraModel.IP;
                txtSlaveIPAddress.Text = _CameraModel.ISSlaveIP;
                textBoxMasterJobName.Text = _CameraModel.CameraJobNameMaster;
                textBoxSlaveJobName.Text = _CameraModel.CameraJobNameSlave;

                // Sử dụng biến cục bộ thay vì kiểm tra trực tiếp trên Control để tránh sai lệch trạng thái
            //   VisibleControl(isCognex, CognexComponentsPanel);
                VisibleControl(isKeyence, CameraPortPanel);
                VisibleControl(isCognex || isHikrobot, OutputTypePanel);

                //VisibleControl(!cameraType.Equals(CameraType.DM) && isCognex,
                //    labelImageResolution, comboBoxImageResolution, groupBoxOCR);
                //VisibleControl(!cameraType.Equals(CameraType.DM) && isCognex,
                //  labelImageResolution, comboBoxImageResolution);
                VisibleControl(!cameraType.Equals(CameraType.DM) && !KeyenceRad.Checked,
             labelImageResolution, comboBoxImageResolution, groupBoxOCR);
                VisibleControl(isHikrobot, groupBoxHikrobotOCR);
                
                textBoxHikObjectName.Text = _CameraModel.ObjectNameMaster;
                textBoxHikSolutionName.Text = _CameraModel.SolutionName;

                if (comboBoxHikCompareMode != null)
                {
                    int modeIdx = (int)_CameraModel.HikrobotCompareMode;
                    comboBoxHikCompareMode.SelectedIndex =
                        modeIdx < comboBoxHikCompareMode.Items.Count ? modeIdx : 0;

                    dgvOcrMappings.Rows.Clear();
                    if (_CameraModel.OcrToolMappings != null)
                    {
                        foreach (var m in _CameraModel.OcrToolMappings)
                            dgvOcrMappings.Rows.Add(m.ToolKey, m.PODName);
                    }
                }

                if (cameraType == CameraType.CV_X || cameraType == CameraType.VS_C)
                {
                    txtJobName.Text = _CameraModel.KeyenceCurrentProgramNo.ToString("D4");
                    txtPathFTPCameraKeyence.Text = Shared.Settings.THFtpImagePath;
                }
            }
            finally
            {
                _IsBinding = prevBinding;
            }
        }

        private void InitControls()
        {
            _IsBinding = true;
            InitVisibiltyForCamType(_CameraModel.CameraType);
            textBoxCommandError.Text = _CameraModel.CommandErrorOutput;
            textBoxObjectNameMaster.Text = _CameraModel.ObjectNameMaster.Replace(".ReadText", "").Replace(".Result00.String", "");
            textBoxObjectNameSlave.Text = _CameraModel.ObjectNameSlave.Replace(".ReadText", "").Replace(".Result00.String", "");
            numCamPort.Value = int.TryParse(_CameraModel.Port, out int portVal) ? portVal : 0;
            UpdateCameraInfo();
            UpdateKeyenceProgramButtons();
            _IsBinding = false;

            // Fix: dùng string.IsNullOrEmpty(Name) thay vì !IsConnected
            // vì TCP đã set IsConnected=true → check Name để biết SDK đã connect chưa
            if (_CameraModel.CameraType == CameraType.DM
                && Shared.Settings.CameraList.FirstOrDefault()?.ReadMode == CameraModeRead.Basic
                && string.IsNullOrEmpty(_CameraModel.Name))
            {
                Shared.DMCamera?.Disconnect();
                Shared.DMCamera?.Connect(_CameraModel.IP);
            }

            bool[] listBoolCheckBox = UtilityFunctions.IntToBools(_CameraModel.ObjectSelectNum, 5);
            InitControlsAndEvents_IS();
            if (ProjectLabel.IsTHTrueMilk)
            {
                 CognexRad.Visible = false;
                 KeyenceRad.Checked = true;
            }
               
            if(ProjectLabel.IsTHTrueMilk)
            {
                txtPathFTPCameraKeyence.Visible = true;
                label2.Visible = true;
            }    

        }

        private void InitCameraType()
        {
            bool isCognex = _CameraModel?.CameraBrand == Model.CameraBrand.Cognex;
            bool isHikrobot = _CameraModel?.CameraBrand == Model.CameraBrand.Hikrobot;

            comboBoxCamType.Items.Clear();

            if (isCognex)
            {
                CognexComponentsPanel.BringToFront();
                comboBoxCamType.Items.AddRange(new object[] { "DM Series", "IS Series", "IS Series Dual" });
            }
            else if (isHikrobot)
            {
                comboBoxCamType.Items.Add("SC Series");
            }
            else // Keyence
            {
                comboBoxCamType.Items.Add("CV-X Series");
                comboBoxCamType.Items.Add("VS-C Series");
            }
            comboBoxCamType.Text = "";
        }


        private void InitControlsAndEvents_IS()

        {
            radioButtonSymMaster.Checked = _CameraModel.IsSymbolMaster;
            radioButtonTextMaster.Checked = !_CameraModel.IsSymbolMaster;

            radioButtonSymSlave.Checked = _CameraModel.IsSymbolSlave;
            radioButtonTextSlave.Checked = !_CameraModel.IsSymbolSlave;

            radioButtonSymMaster.CheckedChanged += RadioButtonMaster_CheckedChanged;
            radioButtonTextMaster.CheckedChanged += RadioButtonMaster_CheckedChanged;

            radioButtonSymSlave.CheckedChanged += RadioButtonSlave_CheckedChanged;
            radioButtonTextSlave.CheckedChanged += RadioButtonSlave_CheckedChanged;
        }

        private void RadioButtonSlave_CheckedChanged(object sender, EventArgs e)
        {
            _CameraModel.IsSymbolSlave = radioButtonSymSlave.Checked;
            _CameraModel.ObjectNameSlave = textBoxObjectNameSlave.Text;
            Shared.SaveSettings();
        }

        private void Shared_OnCameraStatusChange(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Shared_OnCameraStatusChange(sender, e)));
                return;
            }
            UpdateCameraInfo();
            UpdateKeyenceProgramButtons();
        }

        private void UpdateKeyenceProgramButtons()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateKeyenceProgramButtons()));
                return;
            }
            var camType = _CameraModel.CameraType;
            bool isKeyence = camType == CameraType.VS_C || camType == CameraType.CV_X;
            bool isConnected = Shared.vscCamera != null && Shared.vscCamera.IsConnected();
            btnLoadPrograms.Enabled = isKeyence && isConnected;
            btnChangeJob.Enabled = isKeyence && isConnected;

            if (isKeyence && isConnected)
            {
                Shared.vscCamera.CurrentProgramChanged -= VscCamera_CurrentProgramChanged;
                Shared.vscCamera.CurrentProgramChanged += VscCamera_CurrentProgramChanged;

                Task.Run(() =>
                {
                    try
                    {
                        int progNo = Shared.vscCamera.QueryCurrentProgramNo();
                        if (IsDisposed) return;
                        BeginInvoke(new Action(() =>
                        {
                            if (!IsDisposed)
                                txtProgramSelected.Text = progNo.ToString("D4");
                        }));
                    }
                    catch { }
                });
            }
        }

        private void VscCamera_CurrentProgramChanged(int programNo)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<int>(VscCamera_CurrentProgramChanged), programNo);
                return;
            }
            txtProgramSelected.Text = programNo.ToString("D4");
        }

        private void UpdateCameraInfo()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateCameraInfo()));
                return;
            }

            txtModel.Text = _CameraModel.Name;
            txtSerialNumber.Text = _CameraModel.SerialNumber;
        }
        /// <summary>
        /// Ngắt kết nối Hikrobot và xóa thông tin khi chuyển sang loại camera khác
        /// </summary>
        private void DisconnectHikrobotCamera()
        {
            _CameraModel.IsConnected = false;
            _CameraModel.Name = "";
            _CameraModel.SerialNumber = "";

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

            UpdateCameraInfo();
            Shared.RaiseOnCameraStatusChangeEvent();
        }

        /// <summary>
        /// Ngắt kết nối Cognex và xóa thông tin khi chuyển sang loại camera khác
        /// </summary>
        private void DisconnectCognexCamera()
        {
            _CameraModel.IsConnected = false;
            _CameraModel.Name = "";
            _CameraModel.SerialNumber = "";

            if (Shared.DMCamera != null)
            {
                try
                {
                    Shared.DMCamera.Disconnect();
                }
                catch { }
                // Không gán null để giữ instance DMCamera cho lần kết nối lại
            }

            UpdateCameraInfo();
            Shared.RaiseOnCameraStatusChangeEvent();
        }

        /// <summary>
        /// Ngắt kết nối Keyence và xóa thông tin khi chuyển sang loại camera khác
        /// </summary>
        private void DisconnectKeyenceCamera()
        {
            _CameraModel.IsConnected = false;
            _CameraModel.Name = "";
            _CameraModel.SerialNumber = "";

            if (Shared.cvxCamera != null)
            {
                try
                {
                    Shared.cvxCamera.StopListening();
                    Shared.cvxCamera.Disconnect();
                }
                catch { }
                Shared.cvxCamera = null;
            }

            if (Shared.vscCamera != null)
            {
                try
                {
                    Shared.vscCamera.StopListening();
                    Shared.vscCamera.Disconnect();
                }
                catch { }
                Shared.vscCamera = null;
            }

            UpdateCameraInfo();
            Shared.RaiseOnCameraStatusChangeEvent();
        }
        private void InitEvents()
        {
            RegisterRadioButtonControls(AdjustData, CognexRad, KeyenceRad, HikrobotRad);
            txtIPAddress.TextChanged += AdjustData;
            txtSlaveIPAddress.TextChanged += AdjustData;
            txtPathFTPCameraKeyence.TextChanged += AdjustData;
            EnablePosition.CheckedChanged += AdjustData;
            radioBarcodePosition.CheckedChanged += AdjustData;
            radioLogoPosition.CheckedChanged += AdjustData;
            numCamPort.ValueChanged += AdjustData;
            itemsPerHour.CheckedChanged += AdjustData;
            textBoxHikObjectName.TextChanged += AdjustData;   // chỉ 1 lần
            textBoxHikSolutionName.TextChanged += AdjustData;

            RegisterTextBoxControls(AdjustData,
                textBoxMasterJobName,
                textBoxSlaveJobName,
                textBoxObjectNameMaster,
                textBoxObjectNameSlave,
                textBoxCommandError);

            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnCameraStatusChange += Shared_OnCameraStatusChange;

            //Load += UcCameraSettings_Load;
            RegisterComboBoxControls(AdjustData, comboBoxCamType, comboBoxImageResolution, comboBox_ModeReadCamera);

            comboBoxHikCompareMode.SelectedIndexChanged += ComboBoxHikCompareMode_SelectedIndexChanged;
            dgvOcrMappings.CellEndEdit += DgvOcrMappings_CellEndEdit;
            btnAddOcrMapping.Click += BtnAddOcrMapping_Click;
            btnRemoveOcrMapping.Click += BtnRemoveOcrMapping_Click;

            btnLoadPrograms.Click += BtnLoadPrograms_Click;
            btnChangeJob.Click += BtnChangeJob_Click;
            lstPrograms.SelectedIndexChanged += LstPrograms_SelectedIndexChanged;

            IndexCheckBox.Checked = _CameraModel.IsIndexCommandEnable;
            OutputCameraBox.Checked = _CameraModel.OutputType == OutputType.OutputCamera;
            CommandErrorBox.Checked = _CameraModel.OutputType == OutputType.CommandError;
        }

        private void RadioButtonMaster_CheckedChanged(object sender, EventArgs e)
        {
            _CameraModel.IsSymbolMaster = radioButtonSymMaster.Checked;
            _CameraModel.ObjectNameMaster = textBoxObjectNameMaster.Text;
            Shared.SaveSettings();
        }

        private void CheckBoxSelectObjectChange(object sender, EventArgs e)
        {

            Shared.SaveSettings();
        }

        private void UcCameraSettings_Load(object sender, EventArgs e)
        {
            _firstLoad = true;
            InitControls();
            _firstLoad = false;
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        //private void AdjustData(object sender, EventArgs e)
        //{
        //    if (_IsBinding)
        //    {
        //        return;
        //    }
        //    if (sender == txtIPAddress)
        //    {
        //        if (!_firstLoad) // prevent disconect by _firstLoad
        //        {
        //            _CameraModel.IsConnected = false;
        //        }
        //        _CameraModel.IP = txtIPAddress.Text;
        //    }
        //    if (sender == txtSlaveIPAddress)
        //    {
        //        if (!_firstLoad)
        //        {
        //            _CameraModel.IsConnected = false;
        //        }
        //        _CameraModel.ISSlaveIP = txtSlaveIPAddress.Text;
        //    }
        //    else if (sender == textBoxCommandError)
        //    {
        //        if (textBoxCommandError.Text == "")
        //        {
        //            textBoxCommandError.Text = "1";
        //        }
        //        _CameraModel.CommandErrorOutput = textBoxCommandError.Text;
        //    }
        //    else if (sender == textBoxMasterJobName)
        //    {
        //        if (!_firstLoad) // prevent disconect by _firstLoad
        //        {
        //            _CameraModel.IsConnected = false;
        //        }
        //        _CameraModel.CameraJobNameMaster = textBoxMasterJobName.Text;
        //    }
        //    else if (sender == textBoxSlaveJobName)
        //    {
        //        if (!_firstLoad) // prevent disconect by _firstLoad
        //        {
        //            _CameraModel.IsConnected = false;
        //        }
        //        _CameraModel.CameraJobNameSlave = textBoxSlaveJobName.Text;
        //    }
        //    else if (sender == comboBoxImageResolution)
        //    {
        //        _CameraModel.WidthImage = ConvertFromIndexResolution(comboBoxImageResolution.SelectedIndex).width;
        //        _CameraModel.HeigthImage = ConvertFromIndexResolution(comboBoxImageResolution.SelectedIndex).height;
        //    }
        //    else if (sender == comboBoxCamType)
        //    {
        //        if(Model.CameraBrand.Cognex == _CameraModel.CameraBrand)
        //        {
        //            switch (comboBoxCamType.SelectedIndex)
        //            {
        //                case 0:
        //                    if (!(_CameraModel.CameraType == CameraType.DM))
        //                    {
        //                        _CameraModel.CameraType = CameraType.DM;
        //                        _CameraModel.IsConnected = false;
        //                        InitVisibiltyForCamType(_CameraModel.CameraType);

        //                    }
        //                    break;
        //                case 1:
        //                    if (!(_CameraModel.CameraType == CameraType.IS))
        //                    {
        //                        _CameraModel.CameraType = CameraType.IS;
        //                        _CameraModel.IsConnected = false;
        //                        InitVisibiltyForCamType(_CameraModel.CameraType);

        //                    }
        //                    break;
        //                case 2:
        //                    if (!(_CameraModel.CameraType == CameraType.ISDual))
        //                    {
        //                        _CameraModel.CameraType = CameraType.ISDual;
        //                        _CameraModel.IsConnected = false;
        //                        InitVisibiltyForCamType(_CameraModel.CameraType);

        //                    }

        //                    break;
        //                default:
        //                    break;
        //            }

        //        }
        //        else if (_CameraModel.CameraBrand == Model.CameraBrand.Hikrobot)
        //        {
        //            switch (comboBoxCamType.SelectedIndex)
        //            {
        //                case 0:
        //                    if (!(_CameraModel.CameraType == CameraType.HIKROBOT))
        //                    {
        //                        _CameraModel.CameraType = CameraType.HIKROBOT;
        //                        _CameraModel.IsConnected = false;
        //                        InitVisibiltyForCamType(_CameraModel.CameraType);
        //                    }
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        else // Keyence
        //        {
        //            switch (comboBoxCamType.SelectedIndex)
        //            {
        //                case 0:
        //                    if (!(_CameraModel.CameraType == CameraType.CV_X))
        //                    {
        //                        _CameraModel.CameraType = CameraType.CV_X;
        //                        _CameraModel.IsConnected = false;
        //                        InitVisibiltyForCamType(_CameraModel.CameraType);
        //                    }
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //    }
        //    else if (sender == textBoxObjectNameMaster)
        //    {
        //        _CameraModel.ObjectNameMaster = textBoxObjectNameMaster.Text;
        //    }
        //    else if (sender == textBoxObjectNameSlave)
        //    {
        //        _CameraModel.ObjectNameSlave = textBoxObjectNameSlave.Text;
        //    }
        //    else if (sender == comboBox_ModeReadCamera)
        //    {
        //        if (comboBox_ModeReadCamera.SelectedIndex == 0)
        //        {
        //            Shared.Settings.CameraList.FirstOrDefault().ReadMode = CameraModeRead.Basic;
        //            Shared.DMCamera?.Disconnect();
        //            Shared.DMCamera?.Connect(Shared.Settings.CameraList.FirstOrDefault().IP);

        //        }
        //        else
        //        {
        //            Shared.Settings.CameraList.FirstOrDefault().ReadMode = CameraModeRead.MultiRead;
        //            Shared.DMCamera?.Disconnect();
        //        }
        //    }
        //    else if (sender == EnablePosition)
        //    {
        //        Shared.Settings.EnablePosition = EnablePosition.Checked;
        //    }
        //    else if (sender == itemsPerHour)
        //    {
        //        Shared.Settings.IsItemsPerHour = itemsPerHour.Checked;
        //    }
        //    else if (sender == radioBarcodePosition || sender == radioLogoPosition)
        //    {
        //        Shared.Settings.Position = radioBarcodePosition.Checked ? SettingsModel.PositionType.BarcodePosition : SettingsModel.PositionType.LogoPosition;
        //    }
        //    else if (sender == CognexRad || sender == KeyenceRad || sender == HikrobotRad)
        //    {
        //        bool isCognex = CognexRad.Checked;
        //        bool isHikrobot = HikrobotRad.Checked;

        //        // Nếu đang là Hikrobot mà chuyển sang loại khác → disconnect và xóa thông tin
        //        bool wasHikrobot = _CameraModel.CameraBrand == Model.CameraBrand.Hikrobot;

        //        if (isCognex)
        //        {
        //            if (wasHikrobot) DisconnectHikrobotCamera();

        //            _CameraModel.CameraBrand = Model.CameraBrand.Cognex;
        //            if (_CameraModel.CameraType != CameraType.DM &&
        //                _CameraModel.CameraType != CameraType.IS &&
        //                _CameraModel.CameraType != CameraType.ISDual)
        //                _CameraModel.CameraType = CameraType.DM;
        //        }
        //        else if (isHikrobot)
        //        {
        //            _CameraModel.CameraBrand = Model.CameraBrand.Hikrobot;
        //            _CameraModel.CameraType = CameraType.HIKROBOT;
        //        }
        //        else // Keyence
        //        {
        //            if (wasHikrobot) DisconnectHikrobotCamera();

        //            _CameraModel.CameraBrand = Model.CameraBrand.Keyence;
        //            _CameraModel.CameraType = CameraType.CV_X;
        //        }

        //        InitVisibiltyForCamType(_CameraModel.CameraType);
        //    }
        //    else if(sender == numCamPort)
        //    {
        //        _CameraModel.Port = numCamPort.Value.ToString();
        //        //numCamPort
        //    }

        //    Shared.SaveSettings();
        //}
        private void AdjustData(object sender, EventArgs e)
        {
            if (_IsBinding)
            {
                return;
            }
            if (sender == txtIPAddress)
            {
                if (!_firstLoad)
                {
                    _CameraModel.IsConnected = false;
                    _CameraModel.Name = "";
                    _CameraModel.SerialNumber = "";
                    UpdateCameraInfo();
                    Shared.RaiseOnCameraStatusChangeEvent();
                }
                _CameraModel.IP = txtIPAddress.Text;
            }
            if (sender == txtSlaveIPAddress)
            {
                if (!_firstLoad)
                {
                    _CameraModel.IsConnected = false;
                    _CameraModel.Name = "";
                    _CameraModel.SerialNumber = "";
                    UpdateCameraInfo();
                }
                _CameraModel.ISSlaveIP = txtSlaveIPAddress.Text;
            }
            else if (sender == textBoxCommandError)
            {
                if (textBoxCommandError.Text == "")
                {
                    textBoxCommandError.Text = "1";
                }
                _CameraModel.CommandErrorOutput = textBoxCommandError.Text;
            }
            else if (sender == textBoxMasterJobName)
            {
                if (!_firstLoad)
                {
                    _CameraModel.IsConnected = false;
                }
                _CameraModel.CameraJobNameMaster = textBoxMasterJobName.Text;
            }
            else if (sender == textBoxSlaveJobName)
            {
                if (!_firstLoad)
                {
                    _CameraModel.IsConnected = false;
                }
                _CameraModel.CameraJobNameSlave = textBoxSlaveJobName.Text;
            }
            else if (sender == textBoxHikObjectName)  // chỉ 1 block, bỏ block bị lặp
            {
                _CameraModel.ObjectNameMaster = textBoxHikObjectName.Text;
            }
            else if (sender == textBoxHikSolutionName)
            {
                _CameraModel.SolutionName = textBoxHikSolutionName.Text;
            }

            else if (sender == comboBoxImageResolution)
            {
                _CameraModel.WidthImage = ConvertFromIndexResolution(comboBoxImageResolution.SelectedIndex).width;
                _CameraModel.HeigthImage = ConvertFromIndexResolution(comboBoxImageResolution.SelectedIndex).height;
            }
            else if (sender == comboBoxCamType)
            {
                if (Model.CameraBrand.Cognex == _CameraModel.CameraBrand)
                {
                    switch (comboBoxCamType.SelectedIndex)
                    {
                        case 0:
                            if (!(_CameraModel.CameraType == CameraType.DM))
                            {
                                _CameraModel.CameraType = CameraType.DM;
                                _CameraModel.IsConnected = false;
                                _CameraModel.Name = "";
                                _CameraModel.SerialNumber = "";
                                UpdateCameraInfo();
                                InitVisibiltyForCamType(_CameraModel.CameraType);
                            }
                            break;
                        case 1:
                            if (!(_CameraModel.CameraType == CameraType.IS))
                            {
                                _CameraModel.CameraType = CameraType.IS;
                                _CameraModel.IsConnected = false;
                                _CameraModel.Name = "";
                                _CameraModel.SerialNumber = "";
                                UpdateCameraInfo();
                                InitVisibiltyForCamType(_CameraModel.CameraType);
                            }
                            break;
                        case 2:
                            if (!(_CameraModel.CameraType == CameraType.ISDual))
                            {
                                _CameraModel.CameraType = CameraType.ISDual;
                                _CameraModel.IsConnected = false;
                                _CameraModel.Name = "";
                                _CameraModel.SerialNumber = "";
                                UpdateCameraInfo();
                                InitVisibiltyForCamType(_CameraModel.CameraType);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else if (_CameraModel.CameraBrand == Model.CameraBrand.Hikrobot)
                {
                    switch (comboBoxCamType.SelectedIndex)
                    {
                        case 0:
                            if (!(_CameraModel.CameraType == CameraType.HIKROBOT))
                            {
                                _CameraModel.CameraType = CameraType.HIKROBOT;
                                _CameraModel.IsConnected = false;
                                _CameraModel.Name = "";
                                _CameraModel.SerialNumber = "";
                                UpdateCameraInfo();
                                InitVisibiltyForCamType(_CameraModel.CameraType);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else // Keyence
                {
                    switch (comboBoxCamType.SelectedIndex)
                    {
                        case 0:
                            if (!(_CameraModel.CameraType == CameraType.CV_X))
                            {
                                _CameraModel.CameraType = CameraType.CV_X;
                                _CameraModel.IsConnected = false;
                                _CameraModel.Name = "";
                                _CameraModel.SerialNumber = "";
                                UpdateCameraInfo();
                                InitVisibiltyForCamType(_CameraModel.CameraType);
                            }
                            break;
                        case 1:
                            if (!(_CameraModel.CameraType == CameraType.VS_C))
                            {
                                _CameraModel.CameraType = CameraType.VS_C;
                                _CameraModel.IsConnected = false;
                                _CameraModel.Name = "";
                                _CameraModel.SerialNumber = "";
                                UpdateCameraInfo();
                                InitVisibiltyForCamType(_CameraModel.CameraType);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (sender == textBoxObjectNameMaster)
            {
                _CameraModel.ObjectNameMaster = textBoxObjectNameMaster.Text;
            }
            else if (sender == textBoxObjectNameSlave)
            {
                _CameraModel.ObjectNameSlave = textBoxObjectNameSlave.Text;
            }
            else if (sender == comboBox_ModeReadCamera)
            {
                if (comboBox_ModeReadCamera.SelectedIndex == 0)
                {
                    Shared.Settings.CameraList.FirstOrDefault().ReadMode = CameraModeRead.Basic;
                    Shared.DMCamera?.Disconnect();
                    Shared.DMCamera?.Connect(Shared.Settings.CameraList.FirstOrDefault().IP);
                }
                else
                {
                    Shared.Settings.CameraList.FirstOrDefault().ReadMode = CameraModeRead.MultiRead;
                    Shared.DMCamera?.Disconnect();
                }
            }
            else if (sender == EnablePosition)
            {
                Shared.Settings.EnablePosition = EnablePosition.Checked;
            }
            else if (sender == itemsPerHour)
            {
                Shared.Settings.IsItemsPerHour = itemsPerHour.Checked;
            }
            else if (sender == radioBarcodePosition || sender == radioLogoPosition)
            {
                Shared.Settings.Position = radioBarcodePosition.Checked
                    ? SettingsModel.PositionType.BarcodePosition
                    : SettingsModel.PositionType.LogoPosition;
            }
            else if (sender == CognexRad || sender == KeyenceRad || sender == HikrobotRad)
            {
                bool isCognex = CognexRad.Checked;
                bool isHikrobot = HikrobotRad.Checked;
                bool wasHikrobot = _CameraModel.CameraBrand == Model.CameraBrand.Hikrobot;

                if (isCognex)
                {
                    if (wasHikrobot) DisconnectHikrobotCamera();
                    else if (_CameraModel.CameraBrand == Model.CameraBrand.Keyence) DisconnectKeyenceCamera();

                    _CameraModel.CameraBrand = Model.CameraBrand.Cognex;
                    if (_CameraModel.CameraType != CameraType.DM &&
                        _CameraModel.CameraType != CameraType.IS &&
                        _CameraModel.CameraType != CameraType.ISDual)
                        _CameraModel.CameraType = CameraType.DM;
                    _CameraModel.Name = "";
                    _CameraModel.SerialNumber = "";
                    UpdateCameraInfo();

                    InitVisibiltyForCamType(_CameraModel.CameraType);
                }
                else if (isHikrobot)
                {
                    if (wasHikrobot) { }
                    else if (_CameraModel.CameraBrand == Model.CameraBrand.Keyence) DisconnectKeyenceCamera();
                    else
                    {
                        _CameraModel.IsConnected = false;
                        _CameraModel.Name = "";
                        _CameraModel.SerialNumber = "";
                        UpdateCameraInfo();
                    }
                    _CameraModel.CameraBrand = Model.CameraBrand.Hikrobot;
                    _CameraModel.CameraType = CameraType.HIKROBOT;
                }
                else // Keyence
                {
                    if (wasHikrobot) DisconnectHikrobotCamera();
                    else if (_CameraModel.CameraBrand == Model.CameraBrand.Cognex) DisconnectCognexCamera();
                    else
                    {
                        _CameraModel.IsConnected = false;
                        _CameraModel.Name = "";
                        _CameraModel.SerialNumber = "";
                        UpdateCameraInfo();
                    }
                    _CameraModel.CameraBrand = Model.CameraBrand.Keyence;
                    _CameraModel.CameraType = CameraType.CV_X;

                    // Set default port for Keyence
                    _CameraModel.Port = "8500";
                    numCamPort.Value = 8500;

                    InitVisibiltyForCamType(_CameraModel.CameraType);
                }
            }
            else if (sender == numCamPort)
            {
                _CameraModel.Port = numCamPort.Value.ToString();
            }
            else if (sender == txtPathFTPCameraKeyence)
            {
                Shared.Settings.THFtpImagePath = txtPathFTPCameraKeyence.Text;
            }

            Shared.SaveSettings();
        }
        private void SetLanguage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }

            lblModel.Text = Lang .Model;
            labelCamType.Text = Lang.Type;
            lblIPAddress.Text = Lang.IPAddress;
            lblSerialNumber.Text = Lang.SerialNumber;
            lblSlaveIp.Text = Lang.SlaveIPAddress;
            grbCamera.Text = Lang.CameraTMP;
            groupBoxOCR.Text = Lang.InsightVisionObjectSettings;
            labelObjectNameMaster.Text = Lang.ObjectNameMaster;
            labelObjectNameSlave.Text = Lang.ObjectNameSlave;
            labelImageResolution.Text = Lang.ImageResolution;
            radioButtonSymSlave.Text = radioButtonSymMaster.Text = Lang.IDRead;
            radioButtonTextSlave.Text = radioButtonTextMaster.Text = Lang.CharacterRead;
            lblModeRead.Text = Lang.DatamanReadMode;
            labelMasterJobName.Text = Lang.MasterJobName;
            labelSlaveJobName.Text = Lang.SlaveJobName;
            CameraBrand.Text = Lang.Camera;
            labelHikObjectName.Text = Lang.ObjectNameMaster;
            groupBoxHikrobotOCR.Text = "Hikrobot OCR Settings";
        }

        private void CustomButton_Click(object sender, EventArgs e)
        {
            textBoxCommandError.Enabled = true;
        }

        private void CustomCommandCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            textBoxCommandError.Enabled = CustomCommandCheckBox.Checked;
        }

        private void IndexCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // thinh dep trai dang lam
            _CameraModel.IsIndexCommandEnable = IndexCheckBox.Checked;
            Shared.SaveSettings();
        }
    
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            _CameraModel.OutputType = OutputType.OutputCamera;
            Shared.SaveSettings();
        }

        private void CommandErrorBox_CheckedChanged(object sender, EventArgs e)
        {
            _CameraModel.OutputType = OutputType.CommandError;
            Shared.SaveSettings();
        }
        private void ComboBoxHikCompareMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_IsBinding) return;
            var mode = (HikrobotCompareMode)comboBoxHikCompareMode.SelectedIndex;
            _CameraModel.HikrobotCompareMode = mode;

            // Auto-fill ObjectNameMaster khi chuyển mode
            switch (mode)
            {
                case HikrobotCompareMode.BarcodeOnly:
                case HikrobotCompareMode.BarcodeAndOCR:
                    // Nếu đang là OCR tool name → reset về barcode default
                    if (string.IsNullOrWhiteSpace(textBoxHikObjectName.Text)
                        || textBoxHikObjectName.Text == "obj_char_info_1")
                    {
                        textBoxHikObjectName.Text = "code_string";
                    }
                    break;
                case HikrobotCompareMode.OCROnly:
                    // Nếu đang là barcode tool name → reset về OCR default
                    if (string.IsNullOrWhiteSpace(textBoxHikObjectName.Text)
                        || textBoxHikObjectName.Text == "code_string")
                    {
                        textBoxHikObjectName.Text = "obj_char_info_1";
                    }
                    break;
            }

            Shared.SaveSettings();
        }

        private void DgvOcrMappings_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            SyncOcrMappingsToModel();
        }

        private void BtnAddOcrMapping_Click(object sender, EventArgs e)
        {
            int newRowIdx = dgvOcrMappings.Rows.Add("", "");
            dgvOcrMappings.CurrentCell = dgvOcrMappings.Rows[newRowIdx].Cells[0];
            dgvOcrMappings.BeginEdit(true);
        }

        private void BtnRemoveOcrMapping_Click(object sender, EventArgs e)
        {
            if (dgvOcrMappings.CurrentRow == null) return;
            dgvOcrMappings.Rows.Remove(dgvOcrMappings.CurrentRow);
            SyncOcrMappingsToModel();
        }

        private void SyncOcrMappingsToModel()
        {
            _CameraModel.OcrToolMappings = new List<OcrToolMapping>();
            foreach (DataGridViewRow row in dgvOcrMappings.Rows)
            {
                string toolKey = row.Cells["ToolKey"].Value?.ToString() ?? "";
                string podName = row.Cells["PODName"].Value?.ToString() ?? "";
                if (!string.IsNullOrEmpty(toolKey) || !string.IsNullOrEmpty(podName))
                {
                    _CameraModel.OcrToolMappings.Add(new OcrToolMapping
                    {
                        ToolKey = toolKey,
                        PODName = podName
                    });
                }
            }
            Shared.SaveSettings();
        }

        #region Keyence Program Management
        private async void BtnLoadPrograms_Click(object sender, EventArgs e)
        {
            var camType = _CameraModel.CameraType;
            if (camType != CameraType.VS_C && camType != CameraType.CV_X)
            {
                MessageBox.Show("Chỉ hỗ trợ camera Keyence CV-X / VS-C.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            btnLoadPrograms.Enabled = false;
            try
            {
                if (Shared.vscCamera == null || !Shared.vscCamera.IsConnected())
                {
                    MessageBox.Show("Camera chưa kết nối.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // FTP đã được connect trong Connect() → không cần EnableFtp() nữa
                var programs = await Shared.vscCamera.GetProgramListAsync();

                lstPrograms.Items.Clear();
                foreach (var p in programs)
                    lstPrograms.Items.Add(p);
                if (lstPrograms.Items.Count > 0)
                    lstPrograms.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải program: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLoadPrograms.Enabled = true;
            }
        }
        private async void BtnChangeJob_Click(object sender, EventArgs e)
        {
            var camType = _CameraModel.CameraType;
            if (camType != CameraType.VS_C && camType != CameraType.CV_X)
            {
                MessageBox.Show("Chỉ hỗ trợ camera Keyence CV-X / VS-C.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string jobNoText = txtJobName.Text.Trim();
            if (string.IsNullOrWhiteSpace(jobNoText) || !int.TryParse(jobNoText, out int jobNo) || jobNo < 0 || jobNo > 9999)
            {
                MessageBox.Show("Job number phải là số từ 0 đến 9999.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            btnChangeJob.Enabled = false;

            try
            {
                if (Shared.vscCamera == null || !Shared.vscCamera.IsConnected())
                {
                    MessageBox.Show("Camera chưa kết nối.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var (success, message) = await Shared.vscCamera.ChangeProgramAsync(jobNo);

                if (!success)
                {
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // LƯU PROGRAM NUMBER VÀO MODEL
                _CameraModel.KeyenceCurrentProgramNo = jobNo;
                Shared.SaveSettings(); // Persist to Settings.xml

                // ── Query PR để xác nhận + hiển thị ──
                try
                {
                    int confirmedNo = Shared.vscCamera.QueryCurrentProgramNo();
                    if (confirmedNo >= 0)
                        txtProgramSelected.Text = confirmedNo.ToString("D4");
                }
                catch { }

                MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chuyển program: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnChangeJob.Enabled = true;
            }
        }

        private void LstPrograms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPrograms.SelectedItem is VscProgramInfo program)
            {
                txtJobName.Text = program.ProgramNo.ToString("D4");
                // Cập nhật model nhưng KHÔNG save ngay - chỉ save khi nhấn "Change Job" thành công
                _CameraModel.KeyenceCurrentProgramNo = program.ProgramNo;
            }
        }
        #endregion Keyence Program Management

        private void lblSlaveIp_Click(object sender, EventArgs e)
        {

        }
    }
}
