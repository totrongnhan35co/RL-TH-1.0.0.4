using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class ucSerialDeviceSettings : UserControl
    {
        private bool _firstLoad = false;

        public ucSerialDeviceSettings()
        {
            InitializeComponent();
            Load += UcSerialDeviceSettings_Load;
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitEvents();
            SetLanguage();
        }
        private void UcSerialDeviceSettings_Load(object sender, EventArgs e)
        {
            _firstLoad = true;
            // load cac setting da co hoac mac dinh
            InitSerialDevComboBoxValues();
            _firstLoad = false;

        }

        private void InitSerialDevComboBoxValues()
        {
            radioEnScanner.Checked = Shared.Settings.EnSerialDevice;
            radioDisScanner.Checked = !radioEnScanner.Checked;
            InitSerialDivComName();
            InitSerialDivBitPerSecond();
            InitSerialDivDataBits();
            InitSerialDivParity();
            InitSerialDivStopBits();
        }

        private void InitSerialDivComName()
        {
            switch (Shared.Settings.SerialDivComName)
            {
                case "COM3":
                    comboBoxComName.SelectedIndex = 0;
                    break;
                case "COM4":
                    comboBoxComName.SelectedIndex = 1;
                    break;
                case "COM5":
                    comboBoxComName.SelectedIndex = 2;
                    break;
                case "COM6":
                    comboBoxComName.SelectedIndex = 3;
                    break;
                case "COM7":
                    comboBoxComName.SelectedIndex = 4;
                    break;
                default:
                    break;
            }
        }

        private void InitSerialDivBitPerSecond()
        {
            switch (Shared.Settings.SerialDivBitPerSecond)
            {
                case 9600:
                    comboBoxBitPerSeconds.SelectedIndex = 0;
                    break;
                case 19200:
                    comboBoxBitPerSeconds.SelectedIndex = 1;
                    break;
                case 38400:
                    comboBoxBitPerSeconds.SelectedIndex = 2;
                    break;
                case 57600:
                    comboBoxBitPerSeconds.SelectedIndex = 3;
                    break;
                case 115200:
                    comboBoxBitPerSeconds.SelectedIndex = 4;
                    break;
                default:
                    break;
            }
        }

        private void InitSerialDivDataBits()
        {
            switch (Shared.Settings.SerialDivDataBits)
            {
                case 7:
                    comboBoxDataBits.SelectedIndex = 0;
                    break;
                case 8:
                    comboBoxDataBits.SelectedIndex = 1;
                    break;
                default:
                    break;
            }
        }

        private void InitSerialDivParity()
        {
            switch (Shared.Settings.SerialDivParity)
            {
                case Parity.None:
                    comboBoxParity.SelectedIndex = 0;
                    break;
                case Parity.Odd:
                    comboBoxParity.SelectedIndex = 1;
                    break;
                case Parity.Even:
                    comboBoxParity.SelectedIndex = 2;
                    break;
                default:
                    break;
            }
        }

        private void InitSerialDivStopBits()
        {
            switch (Shared.Settings.SerialDivStopBits)
            {
                case StopBits.One:
                    comboBoxStopBits.SelectedIndex = 0;
                    break;
                case StopBits.Two:
                    comboBoxStopBits.SelectedIndex = 1;
                    break;
                default:
                    break;
            }
        }

        private void InitEvents()
        {
            radioEnScanner.CheckedChanged += (s, e) =>
            {
                Shared.Settings.EnSerialDevice = radioEnScanner.Checked;
                Shared.SaveSettings();
            };
            //radioDisScanner.CheckedChanged += (s, e) =>
            //{
            //    Shared.Settings.EnSerialDevice = radioEnScanner.Checked;
            //    Shared.SaveSettings();
            //};
            radioEnScanner.CheckedChanged += FrmJob.RadioButton_CheckedChanged;
            radioDisScanner.CheckedChanged += FrmJob.RadioButton_CheckedChanged;
           
            comboBoxComName.SelectedIndexChanged += AdjustData;
            comboBoxBitPerSeconds.SelectedIndexChanged += AdjustData;
            comboBoxDataBits.SelectedIndexChanged += AdjustData;
            comboBoxParity.SelectedIndexChanged += AdjustData;
            comboBoxStopBits.SelectedIndexChanged += AdjustData;

            Shared.OnLanguageChange += Shared_OnLanguageChange;
        }

        private void AdjustData(object sender, EventArgs e)
        {

            if(sender == comboBoxComName)
            {
                switch (comboBoxComName.SelectedIndex)
                {
                    case 0:
                        if (!(Shared.Settings.SerialDivComName == "COM3"))
                        {
                            Shared.Settings.SerialDivComName = "COM3";
                        }
                        break;
                    case 1:
                        if (!(Shared.Settings.SerialDivComName == "COM4"))
                        {
                            Shared.Settings.SerialDivComName = "COM4";
                        }
                        break;
                    case 2:
                        if (!(Shared.Settings.SerialDivComName == "COM5"))
                        {
                            Shared.Settings.SerialDivComName = "COM5";
                        }
                        break;
                    case 3:
                        if (!(Shared.Settings.SerialDivComName == "COM6"))
                        {
                            Shared.Settings.SerialDivComName = "COM6";
                        }
                        break;
                    case 4:
                        if (!(Shared.Settings.SerialDivComName == "COM7"))
                        {
                            Shared.Settings.SerialDivComName = "COM7";
                        }
                        break;
                    default:
                        break;
                }

                InitSerialDivComName();
            }
            if (sender == comboBoxBitPerSeconds)
            {
                switch (comboBoxBitPerSeconds.SelectedIndex)
                {
                    case 0:
                        if (!(Shared.Settings.SerialDivBitPerSecond == 9600))
                        {
                            Shared.Settings.SerialDivBitPerSecond = 9600;
                            //Shared.SerialDevController.
                        }
                        break;
                    case 1:
                        if (!(Shared.Settings.SerialDivBitPerSecond == 19200))
                        {
                            Shared.Settings.SerialDivBitPerSecond = 19200;
                        }
                        break;
                    case 2:
                        if (!(Shared.Settings.SerialDivBitPerSecond == 38400))
                        {
                            Shared.Settings.SerialDivBitPerSecond = 38400;
                        }
                        break;
                    case 3:
                        if (!(Shared.Settings.SerialDivBitPerSecond == 57600))
                        {
                            Shared.Settings.SerialDivBitPerSecond = 57600;
                        }
                        break;
                    case 4:
                        if (!(Shared.Settings.SerialDivBitPerSecond == 115200))
                        {
                            Shared.Settings.SerialDivBitPerSecond = 115200;
                        }
                        break;
                    default:
                        break;
                }

                InitSerialDivBitPerSecond();
            }

            if (sender == comboBoxDataBits)
            {
                switch (comboBoxDataBits.SelectedIndex)
                {
                    case 0:
                        if (!(Shared.Settings.SerialDivDataBits == 7))
                        {
                            Shared.Settings.SerialDivDataBits = 7;
                        }
                        break;
                    case 1:
                        if (!(Shared.Settings.SerialDivDataBits == 8))
                        {
                            Shared.Settings.SerialDivDataBits = 8;
                        }
                        break;
                    default:
                        break;
                }

                InitSerialDivDataBits();
            }

            if (sender == comboBoxParity)
            {
                switch (comboBoxParity.SelectedIndex)
                {
                    case 0:  
                        if (!(Shared.Settings.SerialDivParity == Parity.None))
                        {
                            Shared.Settings.SerialDivParity = Parity.None;
                        } 
                        break;
                    case 1:
                        if (!(Shared.Settings.SerialDivParity == Parity.Odd))
                        {
                            Shared.Settings.SerialDivParity = Parity.Odd;
                        }
                        break;
                    case 2:
                        if (!(Shared.Settings.SerialDivParity == Parity.Even))
                        {
                            Shared.Settings.SerialDivParity = Parity.Even;
                        }
                        break;
                    default:
                        break;
                }

                InitSerialDivParity();
            }

            if (sender == comboBoxStopBits)
            {
                switch (comboBoxStopBits.SelectedIndex)
                {
                    case 0:
                        if (!(Shared.Settings.SerialDivStopBits == StopBits.One))
                        {
                            Shared.Settings.SerialDivStopBits = StopBits.One;
                        }
                        break;
                    case 1:
                        if (!(Shared.Settings.SerialDivStopBits == StopBits.Two))
                        {
                            Shared.Settings.SerialDivStopBits = StopBits.Two;
                        }
                        break;
                    default:
                        break;
                }

                InitSerialDivStopBits();
            }
            //
            if (!_firstLoad) // prevent disconect by _firstLoad
            {
                Shared.SerialDevController?.DisconnectSerialDevice();
            }

            Shared.SaveSettings();
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        private void SetLanguage()
        {
            // cap nhat ngon ngu
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }
            comName.Text = Lang.ComName;
            bitsPerSecond.Text = Lang.BitsPerSecond;
            dataBits.Text = Lang.DataBits;
            parity.Text = Lang.Parity;
            stopBits.Text = Lang.StopBits;
            groupBoxSerial.Text = Lang.SerialDevice;

            radioEnScanner.Text = Lang.Enable;
            radioDisScanner.Text = Lang.Disable;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }

}
