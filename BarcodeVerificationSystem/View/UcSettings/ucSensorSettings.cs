using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.ControlEvents;
using BarcodeVerificationSystem.View.UtilityForms.Nutifood;
using GenCode.Types;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;
using static BarcodeVerificationSystem.Model.SettingsModel;

namespace BarcodeVerificationSystem.View
{
    public partial class UcSensorSettings : UserControl
    {
        private bool _IsBinding = false;
        private RadioButton[] PLCVersionRadios;
        private CancellationTokenSource _cts;
        private int _CurrentSensorIndex { get; set; } = 0;
        public UcSensorSettings()
        {
            InitializeComponent();
            this.Load += UcSensorSettings_Load;
            this.Disposed += UcSensorSettings_Disposed;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitEvents();
            SetLanguage();
            UpdateStatusUI();
        }
        private void UcSensorSettings_Disposed(object sender, EventArgs e)
        {
            _cts?.Cancel();
            //_cts?.Dispose();
        }

        private void StartMonitoring(CancellationToken token)
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // Call UI update safely
                        this.Invoke(new Action(UpdateStatusUI));
                    }
                    catch (Exception) { break; }
                    await Task.Delay(2000, token); // 2 seconds
                }
            }, token);
        }

        private void InitControls()
        {
            _IsBinding = true;
            PLCVersionRadios = new[] { radioV0, radioV1, radioV2 };
            sensorOneBtn.BackColor = _CurrentSensorIndex == 0 ? Color.FromArgb(0, 170, 230) : Color.White;
            sensorTwoBtn.BackColor = _CurrentSensorIndex == 1 ? Color.FromArgb(0, 170, 230) : Color.White;
            EncoderMode.Location = new Point(493, 389);

            grBoxSensor.Text = Lang.Sensor + " " + (_CurrentSensorIndex + 1);
         
            radioResumeA.Checked = Shared.Settings.ResumeEncoder == ResumeEncoderType.ResumeA;
            radioResumeAB.Checked = Shared.Settings.ResumeEncoder == ResumeEncoderType.ResumeAB;

            InternalRad.Checked = Shared.Settings.EncoderMode == ResumeEncoderMode.Internal;
            ExternalRad.Checked = Shared.Settings.EncoderMode == ResumeEncoderMode.External;

            PLCVersionRadios[Shared.Settings.PLCVersion].Checked = true;
            UIControlsFuncs.VisibleControl(radioV1.Checked, grbResumeEncoder, grbResumeEncoder, delayOutputPanel);
            UIControlsFuncs.VisibleControl(radioV2.Checked && _CurrentSensorIndex == 1, EncoderMode);
            UIControlsFuncs.VisibleControl(ProjectLabel.IsDefault, OutputDelay);
            //UIControlsFuncs.VisibleControl(radioV2.Checked, btnProductList);

            SetInExLanguage(); 
           
            numSensorControllerPort2.Value = Shared.Settings.SensorControllerPort2;
            comboPLCPort.SelectedIndex = Shared.Settings.NumberOfPort - 1;
            Port2Panel.Visible = Shared.Settings.NumberOfPort > 1;
            if (ProjectLabel.IsTHTrueMilk)
            { 
                grbResumeEncoder.Visible = radioV0.Visible = radioV2.Visible =  false;
            }
            txtSensorControllerIP.Text = Shared.Settings.SensorControllerIP;
            EnableResumeEncoder.Checked = Shared.Settings.ResumeEncoderEnable;
            radSensorControllerEnable.Checked = Shared.Settings.SensorControllerEnable;
            radSensorControllerDisable.Checked = !Shared.Settings.SensorControllerEnable;
            numSensorControllerPort.Value = Shared.Settings.SensorControllerPort;
            numSensorControllerPulseEncoder.Value = Shared.Settings.SensorControllerPulseEncoder[_CurrentSensorIndex];
            numSensorControllerEncoderDiameter.Value = (decimal)Shared.Settings.SensorControllerEncoderDiameter[_CurrentSensorIndex];
            numSensorControllerDelayBefore.Value = Shared.Settings.SensorControllerDelayBefore[_CurrentSensorIndex];
            numSensorControllerDelayAfter.Value = Shared.Settings.SensorControllerDelayAfter[_CurrentSensorIndex];
            numGapLength1.Value = Shared.Settings.GapLength1[_CurrentSensorIndex];
            numLength2Error1.Value = Shared.Settings.Length2Error1[_CurrentSensorIndex];
            numericDelayOutputError.Value = Shared.Settings.DelayOutputError[_CurrentSensorIndex];
            numDelayOutput.Value = Shared.Settings.DelayOutputTime;
            EnableSensorController(Shared.Settings.SensorControllerEnable);
            _IsBinding = false;
        }

        private void SetInExLanguage()
        {
            bool isV2Internal = Shared.Settings.EncoderMode == ResumeEncoderMode.Internal && radioV2.Checked && _CurrentSensorIndex == 1;
            (isV2Internal ? (Action)LanguageForInternal : LanguageForExternal)();
        }

        private void LanguageForExternal()
        {
            lblSensorControllerEncoderDiameter.Text = Lang.EncoderDiameter;
            label3.Text = "mm";
            lblErrorCondition.Text = Lang.ErrorCondition;

            lblSensorControllerPulseEncoder.Text = Lang.PulseEncoder;
            lblSensorControllerDelayBefore.Text = Lang.DelaySensor;
            ppr.Text = "ppr";
            lblGAP.Text = "GAP";
            GAPmm.Text = "mm";
        }

        private void LanguageForInternal()
        {
            lblSensorControllerEncoderDiameter.Text = "Thời gian đến phân loại";
            label3.Text = "giây";
            lblErrorCondition.Text = "Điều kiện dừng";

            lblSensorControllerPulseEncoder.Text = Lang.InternalSpeed;
            lblSensorControllerDelayBefore.Text = "Bộ lọc cảm biến";
            ppr.Text = "m/min";
            lblGAP.Text = "Thời gian tín hiệu lỗi";
            GAPmm.Text = "ms";
        }

        private void UpdateStatusUI()
        {
            if (Shared.SensorController == null) return;
            iconConnectPort1.Image = Shared.SensorController.IsConnected() ? Properties.Resources.icons_green_dot : Properties.Resources.icons8_red_dot;
            iconConnectPort2.Image = Shared.SensorController.IsConnected2() ? Properties.Resources.icons_green_dot : Properties.Resources.icons8_red_dot;
        }

        private void TestPLC(object sender, EventArgs e)
        {
            if (_IsBinding) return;
            switch (sender as Button){
                case Button btn when btn == StartPLC:
                    Shared.SensorController.Send2("(C00001)");
                    break;
                case Button btn when btn == StopPLC:
                    Shared.SensorController.Send2("(C00000)");
                    break;
                case Button btn when btn == ErrorPLC:
                    Shared.SensorController.Send2("(C00005)");
                    break;
                case Button btn when btn == PassPLC:
                    Shared.SensorController.Send2("(C00003)");
                    break;
            }
        }

        private void InitEvents()
        {
            StartPLC.Click += TestPLC;
            StopPLC.Click += TestPLC;
            ErrorPLC.Click += TestPLC;
            PassPLC.Click += TestPLC;

            //numSensorControllerPort.ValueChanged += AdjustData;
            //numSensorControllerPort2.ValueChanged += AdjustData;

            InitControlEvents.RegisterNumericControls(AdjustData, Numeric_KeyUp,
                  numSensorControllerPort,
                  numSensorControllerPulseEncoder,
                  numSensorControllerEncoderDiameter,
                  numSensorControllerDelayBefore,
                  numSensorControllerDelayAfter,
                  numGapLength1,
                  numLength2Error1,
                  numSensorControllerPort2,
                  numDelayOutput
              );

            InitControlEvents.RegisterNumericKeyUpOnlyControls(Numeric_KeyUp, numericDelayOutputError);

            InitControlEvents.RegisterComboBoxControls(AdjustData, comboPLCPort);

            InitControlEvents.RegisterRadioButtonControls(
                FrmJob.RadioButton_CheckedChanged,
                AdjustData,
                radSensorControllerEnable,
                radSensorControllerDisable,
                InternalRad,
                ExternalRad
            );
            InitControlEvents.RegisterRadioButtonControls(AdjustData, 
                radioResumeA,
                radioResumeAB,
                radioV0,
                radioV1,
                radioV2);

            InitControlEvents.RegisterButtonControls(AdjustData,
                btnContenResponseClear,
                SendPort2
                , btnProductList
            );

            InitControlEvents.RegisterCheckBoxControls(AdjustData,
                EnableResumeEncoder
            );

            txtSensorControllerIP.TextChanged += AdjustData;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnRepeatTCPMessageChange += Shared_OnRepeatTCPMessageChange;
            this.Load += UcSensorSettings_Load;
        }

        private bool _handlingKeyUp;
        private void Numeric_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _handlingKeyUp = true;
                AdjustData(sender, e);
                _handlingKeyUp = false;
            }
        }

        private void UcSensorSettings_Load(object sender, EventArgs e)
        {
            InitControls();
            _cts = new CancellationTokenSource();
            StartMonitoring(_cts.Token);
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        private void Shared_OnRepeatTCPMessageChange(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Shared_OnRepeatTCPMessageChange(sender, e)));
                return;
            }

            if (sender is string)
            {
                var message = sender as string;
                richTXTContentResponse.Text = richTXTContentResponse.Text.Insert(0, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ": " + message + "\n");
                try
                {
                    //richTXTContentResponse.SelectedText = message;
                    richTXTContentResponse.ScrollToCaret();
                }
                catch
                {

                }
            }
        }

        private void SetLanguage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }
            grbSensorController.Text = Lang.SensorController;
            lblSensorControllerIP.Text = Lang.IPAddress;
            lblSensorControllerPort.Text = Lang.Port;
            radSensorControllerEnable.Text = Lang.Enable;
            radSensorControllerDisable.Text = Lang.Disable;
            lblContentResponse.Text = Lang.ResponseContent;
            btnContenResponseClear.Text = Lang.ClearResponse;

            lblSensorControllerDelayBefore.Text = Lang.DelaySensor;
            lblSensorControllerDelayAfter.Text = Lang.DisableSensor;
            lblSensorControllerPulseEncoder.Text = Lang.PulseEncoder; // "PPR"
            lblSensorControllerEncoderDiameter.Text = Lang.EncoderDiameter;
            lblErrorCondition.Text = Lang.ErrorCondition;
            lblUnit.Text = Lang.Unit;
            lblDelayOutputError.Text = Lang.DelayOutputError;
            sensorOneBtn.Text = Lang.Sensor + " " + 1;
            sensorTwoBtn.Text = Lang.Sensor + " " + 2;
            EncoderMode.Text = grbResumeEncoder.Text = Lang.EncoderSettings;
            EnableResumeEncoder.Text = Lang.ResumeEncoder;
            radioResumeA.Text = Lang.ResumeA;
            radioResumeAB.Text = Lang.ResumeAB;
            versionLabel.Text = Lang.PLCversion + ": ";
        }

        private void AdjustData(object sender, EventArgs e)
        {
            if (_IsBinding)
            {
                return;
            }

            if (_handlingKeyUp && sender is NumericUpDown)
                return;

            if (sender == radSensorControllerEnable)
            {
                if (radSensorControllerEnable.Checked == true)
                {
                    Shared.Settings.SensorControllerEnable = true;
                    EnableSensorController(Shared.Settings.SensorControllerEnable);
                }
            }
            else if(sender == btnProductList)
            {
                var productListForm = new PLCProductList();
                productListForm.ShowDialog();
            }
            else if (sender == radSensorControllerDisable)
            {
                if (radSensorControllerDisable.Checked == true)
                {
                    Shared.Settings.SensorControllerEnable = false;
                    EnableSensorController(Shared.Settings.SensorControllerEnable);
                }
            }
            else if (sender == txtSensorControllerIP)
            {
                Shared.Settings.SensorControllerIP = txtSensorControllerIP.Text;
            }
            else if (sender == numSensorControllerPort)
            {
                Shared.Settings.SensorControllerPort = (int)numSensorControllerPort.Value;
            }
            else if (sender == numSensorControllerPort2)
            {
                Shared.Settings.SensorControllerPort2 = (int)numSensorControllerPort2.Value;
            }
            else if (sender == numDelayOutput) // MinhChau 20250922
            {
                Shared.Settings.DelayOutputTime = (int)numDelayOutput.Value;
                Shared.SendSettingToSensorController();
            }
            else if (sender == numSensorControllerPulseEncoder)
            {
                Shared.Settings.Set(Shared.Settings.SensorControllerPulseEncoder, _CurrentSensorIndex, (int)numSensorControllerPulseEncoder.Value);
                Shared.SendSettingToSensorController();
            }
            else if (sender == numSensorControllerEncoderDiameter)
            {
                Shared.Settings.Set(Shared.Settings.SensorControllerEncoderDiameter, _CurrentSensorIndex, (float)numSensorControllerEncoderDiameter.Value);
                Shared.SendSettingToSensorController();
            }
            else if (sender == numSensorControllerDelayBefore)
            {
                Shared.Settings.Set(Shared.Settings.SensorControllerDelayBefore, _CurrentSensorIndex, (int)numSensorControllerDelayBefore.Value);
                Shared.SendSettingToSensorController();
            }
            else if (sender == numSensorControllerDelayAfter)
            {
                Shared.Settings.Set(Shared.Settings.SensorControllerDelayAfter, _CurrentSensorIndex, (int)numSensorControllerDelayAfter.Value);
                Shared.SendSettingToSensorController();
            }
            else if (sender == btnContenResponseClear)
            {
                richTXTContentResponse.Text = "";
                richTXTContentResponse.Clear();
            }
            else if (sender == SendPort2)
            {
                Shared.SensorController.Send2(Shared.Settings.CameraList.FirstOrDefault().CommandErrorOutput);
            }
            else if (sender == numGapLength1)
            {
                Shared.Settings.Set(Shared.Settings.GapLength1, _CurrentSensorIndex, (int)numGapLength1.Value);

                Shared.SendSettingToSensorController();
            }
            else if (sender == numLength2Error1)
            {
                Shared.Settings.Set(Shared.Settings.Length2Error1, _CurrentSensorIndex, (int)numLength2Error1.Value);
                Shared.SendSettingToSensorController();
            }
            else if (sender == numericDelayOutputError)
            {
                Shared.Settings.Set(Shared.Settings.DelayOutputError, _CurrentSensorIndex, (int)numericDelayOutputError.Value);
                Shared.SendSettingToSensorController();
            }
            else if (sender == EnableResumeEncoder)
            {
                Shared.Settings.ResumeEncoderEnable = EnableResumeEncoder.Checked;
                if (Shared.Settings.ResumeEncoderEnable)
                    Shared.SendCommandToSensorController(Shared.Settings.ResumeEncoder == SettingsModel.ResumeEncoderType.ResumeA ? Shared.ResumeA : Shared.ResumeAB);

            }
            else if (sender == radioResumeA)
            {
                Shared.Settings.ResumeEncoder = radioResumeA.Checked ? SettingsModel.ResumeEncoderType.ResumeA : SettingsModel.ResumeEncoderType.ResumeAB;
                if (Shared.Settings.ResumeEncoderEnable)
                    Shared.SendCommandToSensorController(Shared.Settings.ResumeEncoder == SettingsModel.ResumeEncoderType.ResumeA ? Shared.ResumeA : Shared.ResumeAB);

            }
            else if (sender is RadioButton rb && PLCVersionRadios.Contains(rb))
            {
                Shared.Settings.PLCVersion = Array.IndexOf(PLCVersionRadios, rb);

                //UIControlsFuncs.VisibleControl(radioV2.Checked, btnProductList);
                UIControlsFuncs.VisibleControl(radioV1.Checked, grbResumeEncoder, delayOutputPanel);
                UIControlsFuncs.VisibleControl(radioV2.Checked && _CurrentSensorIndex == 1, EncoderMode);
            }
            else if (sender == comboPLCPort)
            {
                Shared.Settings.NumberOfPort = comboPLCPort.SelectedIndex + 1;
                Port2Panel.Visible = Shared.Settings.NumberOfPort > 1;
            }
            else if (sender == ExternalRad) // sender == InternalRad || 
            {
                Shared.Settings.EncoderMode = InternalRad.Checked ? ResumeEncoderMode.Internal 
                                                                    : ResumeEncoderMode.External;
                SetInExLanguage();
                Shared.SendSettingToSensorController();
            }

            Shared.SaveSettings();
        }

        private void EnableSensorController(bool isEnable)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableSensorController(isEnable)));
                return;
            }

            txtSensorControllerIP.Enabled = isEnable;
            numSensorControllerPort.Enabled = isEnable;
            numSensorControllerPort2.Enabled = isEnable;
            SendPort2.Enabled = isEnable;
            grbResumeEncoder.Enabled = isEnable;
            numSensorControllerPulseEncoder.Enabled = isEnable;
            numericDelayOutputError.Enabled = isEnable;
            numSensorControllerEncoderDiameter.Enabled = isEnable;
            numSensorControllerDelayBefore.Enabled = isEnable;
            numSensorControllerDelayAfter.Enabled = isEnable;
            numGapLength1.Enabled = isEnable;
            numLength2Error1.Enabled = isEnable;
        }

        private void SensorTwoBtn_Click(object sender, EventArgs e)
        {
            _CurrentSensorIndex = 1;
            InitControls();
        }

        private void SensorOneBtn_Click(object sender, EventArgs e)
        {
            _CurrentSensorIndex = 0;
            InitControls();
        }

        private void richTXTContentResponse_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
