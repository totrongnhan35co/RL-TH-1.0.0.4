using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{

    public partial class UcPrinterSettings : UserControl
    {
        private PrinterModel _PrinterModel;
        private int _Index = 0;
        private char[] symbols = new char[]
        {
            '\t',     // Tab (\t)
            '\x1f',   // Unit Separator (\x1f)
            ' ',      // Space ( )
            '#',      // Hash (#)
            '%',      // Mod (%)
            '&',      // And (&)
            '*',      // Star (*)
            ',',      // Comma (,)
            '.',      // Dot (.)
            ':',      // Colon (:)
            ';',      // Semicolon (;)
            '?',      // Question (?)
            '@'       // AT (@)
        };
        public int Index
        {
            get { return _Index; }
            set
            {
                _Index = value;
                _PrinterModel = Shared.Settings.PrinterList.Count > _Index ? Shared.Settings.PrinterList[_Index] : new PrinterModel { Index = _Index };
            }
        }
        private bool _IsBinding = false;

        public UcPrinterSettings()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitEvents();
            SetLanguage();
            initSplitCharacter();
        }

        private void initSplitCharacter()
        {
            splitCombo.Items.Add("Tab (\\t)");
            splitCombo.Items.Add("Unit Separator (\\x1f)");
            splitCombo.Items.Add("Space ( )");
            splitCombo.Items.Add("Hash (#)");
            splitCombo.Items.Add("Mod (%)");
            splitCombo.Items.Add("And (&)");
            splitCombo.Items.Add("Star (*)");
            splitCombo.Items.Add("Comma (,)");
            splitCombo.Items.Add("Dot (.)");
            splitCombo.Items.Add("Colon (:)");
            splitCombo.Items.Add("Semicolon (;)");
            splitCombo.Items.Add("Question (?)");
            splitCombo.Items.Add("AT (@)");

            // Set the selected index based on Shared.Settings.SplitCharacter
            char splitCharacter = Shared.Settings.SplitCharacter;
            int selectedIndex = Array.IndexOf(symbols, splitCharacter);

            if (selectedIndex >= 0)
            {
                splitCombo.SelectedIndex = selectedIndex;
            }
            else
            {
                // If the character is not found, set to default index (e.g., 0 for Tab)
                splitCombo.SelectedIndex = 1;
            }

        }



        private void InitControls()
        {
            _IsBinding = true;
            txtPrinterIP.Text = _PrinterModel.IP;
            numPrinterPort.Value = _PrinterModel.Port;

            tableLayoutPanelNumBuffer1St.Visible =
            tableLayoutPanelTimeDelay1stSend.Visible = 
                labelMissedStop.Visible = 
                tableLayoutPanel1.Visible = 
                Properties.Settings.Default.Username == "demo";
            // Tiem delay 1st send
            numericUpDownTimeDelay1st.Value = _PrinterModel.TimeDelaySendFirstBuffer;

            // Number buffer 1st send   
            numericUpDownNumBuff1St.Value = _PrinterModel.NumberBuffer1StSend;

            // Check all settings
            radSupported.Checked = _PrinterModel.CheckAllPrinterSettings;
            radUnsuported.Checked = !_PrinterModel.CheckAllPrinterSettings;

            // Mised stop
            radioEnButtonMissedStop.Checked = _PrinterModel.EnableButtonMissedStop;
            radioDisButtonMissedStop.Checked = !_PrinterModel.EnableButtonMissedStop;

            // Send turbo speed 
            radioButtonEnTurboSpeed.Checked = _PrinterModel.EnableSendTurboSpeed;
            radioButtonDisSendIndep.Checked = !_PrinterModel.EnableSendTurboSpeed;

            numPortRemote.Value = _PrinterModel.NumPortRemote;

            if (ProjectLabel.IsNutrifood)
            {
                UIControlsFuncs.HideControls(btnSetupPrinter, lblPrinterOperSys, tableLayoutPanel2, groupBox1);
            }
            if (ProjectLabel.IsTHTrueMilk) TemplateControl.Visible = false;


            TemplateControl.Visible = ProjectLabel.IsCaoSuDongNai || ProjectLabel.IsWoka || ProjectLabel.IsDroco; // || ProjectLabel.IsCenteryIndia
            TemplateTextBox.Text = Shared.Settings.PrintTemplate;

            _IsBinding = false;
        }

        private void InitEvents()
        {
            txtPrinterIP.TextChanged += AdjustData;
            TemplateTextBox.TextChanged += AdjustData;
            numPrinterPort.ValueChanged += AdjustData;
            numPortRemote.ValueChanged += AdjustData;
            //radNewPODProtocol.CheckedChanged += AdjustData;
            //radOldPODProtocol.CheckedChanged += AdjustData;
            radUnsuported.CheckedChanged += AdjustData;
            radSupported.CheckedChanged += AdjustData;

            // en/dis send independent
            radioButtonDisSendIndep.CheckedChanged += AdjustData;
            radioButtonEnTurboSpeed.CheckedChanged += AdjustData;

            // Missed stop
            radioEnButtonMissedStop.CheckedChanged += AdjustData;
            radioDisButtonMissedStop.CheckedChanged += AdjustData;

            // Time delay 1st send  
            numericUpDownTimeDelay1st.ValueChanged += AdjustData;

            // Number buffer 1st send   
            numericUpDownNumBuff1St.ValueChanged += AdjustData;

            btnSetupPrinter.Click += AdjustData;
            //radNewPODProtocol.CheckedChanged += FrmJob.RadioButton_CheckedChanged;
            //radOldPODProtocol.CheckedChanged += FrmJob.RadioButton_CheckedChanged;
            radUnsuported.CheckedChanged += FrmJob.RadioButton_CheckedChanged;
            radSupported.CheckedChanged += FrmJob.RadioButton_CheckedChanged;
            radioButtonEnTurboSpeed.CheckedChanged += FrmJob.RadioButton_CheckedChanged;
            radioButtonDisSendIndep.CheckedChanged += FrmJob.RadioButton_CheckedChanged;
            radioEnButtonMissedStop.CheckedChanged += FrmJob.RadioButton_CheckedChanged;
            radioDisButtonMissedStop.CheckedChanged += FrmJob.RadioButton_CheckedChanged;

            splitCombo.DrawMode = DrawMode.OwnerDrawVariable;
            splitCombo.Height = 200;
            splitCombo.DropDownHeight = 200;
            splitCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            splitCombo.DrawItem += ComboBoxCustom.MyComboBox_DrawItem;
            splitCombo.MeasureItem += ComboBoxCustom.Cbo_MeasureItem;

            Shared.OnLanguageChange += Shared_OnLanguageChange;
            Shared.OnReceiveResponsePrinter += Shared_OnReceiveResponsePrinter;
            Load += UcPrinterSettings_Load;
        }


      
      

        private void RadioButtonEnSendIndep_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton.Enabled)
                {
                    radioButton.BackColor = radioButton.Checked ? Color.FromArgb(0, 170, 230) : Color.White;
                }
            }
        }

        private void UcPrinterSettings_Load(object sender, EventArgs e)
        {
            InitControls();
        }

        private void Shared_OnReceiveResponsePrinter(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Shared_OnReceiveResponsePrinter(sender, e)));
                return;
            }
            if (sender is string)
            {
                var message = sender as string;
            }
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        private void SetLanguage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }
            lblIPrinterIP.Text = Lang.IPAddress;
            lblPrinterPort.Text = Lang.Port;
            labelSendInde.Text = Lang.TurboSend;
          
            groupBox1.Text = Lang.PODSendMode;
            grbPrinter.Text = Lang.Printer;
            lblPortRemote.Text = Lang.PrinterRemotePort;
            btnSetupPrinter.Text = Lang.AdvancedPrinterSettings;
          
            lblPrinterOperSys.Text = Lang.CheckAllPrinterSettings;
            radSupported.Text = Lang.Enable;
            radUnsuported.Text = Lang.Disable;

            radioButtonEnTurboSpeed.Text =  Lang.Enable;
            radioButtonDisSendIndep.Text = Lang.Disable;
            groupBoxPOD.Text = Lang.PODProtocol;
        }

        private void AdjustData(object sender, EventArgs e)
        {
            if (_IsBinding)
            {
                return;
            }
            if (sender == txtPrinterIP)
            {
                PrinterModel checkExist = Shared.Settings.PrinterList.Find(x => x.IP == _PrinterModel.IP);
                if (checkExist != null)
                {
                    checkExist.IP = txtPrinterIP.Text;
                    PODController checkExistPOD = Shared.PODControllerList.Find(x => x.ServerIP == _PrinterModel.IP);
                    checkExistPOD?.Disconnect();
                }
                _PrinterModel.IP = txtPrinterIP.Text;
            }
            else if (sender == TemplateTextBox)
            {
                Shared.Settings.PrintTemplate = TemplateTextBox.Text;
            }
            else if (sender == numPrinterPort)
            {
                PrinterModel checkExist = Shared.Settings.PrinterList.Find(x => x.IP == _PrinterModel.IP);
                if (checkExist != null)
                {
                    checkExist.Port = (int)numPrinterPort.Value;
                    var checkExistPOD = Shared.PODControllerList.Find(x => x.ServerIP == _PrinterModel.IP);
                    checkExistPOD?.Disconnect();
                }
                _PrinterModel.Port = (int)numPrinterPort.Value;
            }
            else if (sender == numericUpDownNumBuff1St)
            {
                _PrinterModel.NumberBuffer1StSend = (int)numericUpDownNumBuff1St.Value;
            }
            else if (sender == numericUpDownTimeDelay1st)
            {
                _PrinterModel.TimeDelaySendFirstBuffer = (int)numericUpDownTimeDelay1st.Value;
            }
            else if (sender == btnSetupPrinter)
            {
                var remotePrinter = new FrmRemotePrinter
                {
                    IPAddress = txtPrinterIP.Text,
                    Port = (int)numPortRemote.Value
                };
                remotePrinter.ShowDialog();
            }
            else if (sender == numPortRemote)
            {
                _PrinterModel.NumPortRemote = (int)numPortRemote.Value;
            }
            else if (sender == radSupported)
            {
                if (radSupported.Checked)
                {
                    _PrinterModel.CheckAllPrinterSettings = true;
                }
            }
            else if (sender == radUnsuported)
            {
                if (radUnsuported.Checked)
                {
                    _PrinterModel.CheckAllPrinterSettings = false;
                }
            }
            else if (sender == radioEnButtonMissedStop)
            {
                _PrinterModel.EnableButtonMissedStop = radioEnButtonMissedStop.Checked;
            }
            else if (sender == radioDisButtonMissedStop)
            {
                _PrinterModel.EnableButtonMissedStop = !radioDisButtonMissedStop.Checked;
            }

            else if (sender == radioButtonEnTurboSpeed)
            {
                _PrinterModel.EnableSendTurboSpeed = radioButtonEnTurboSpeed.Checked;
            }
            else if (sender == radioButtonDisSendIndep)
            {
                _PrinterModel.EnableSendTurboSpeed = !radioButtonDisSendIndep.Checked;
            }
          
            Shared.SaveSettings();
        }

        private void splitCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = splitCombo.SelectedIndex;

            // Check if the selected index is valid
            if (selectedIndex >= 0 && selectedIndex < symbols.Length)
            {
                char symbol = symbols[selectedIndex];
                Shared.Settings.SplitCharacter = symbol;
                Shared.SaveSettings();
            }

        }

    }
}
