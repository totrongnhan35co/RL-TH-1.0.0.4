using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.UserInfo;
using System;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View.WokaUI
{
    public partial class ucLineSettingCentery : UserControl
    {

        public ucLineSettingCentery()
        {
            InitializeComponent();
            InitControls();
            InitEvents();
        }

        private void InitControls()
        {
            TemplateNameTxt.Text = Shared.Settings.CenterIndiaModel.TemplateName;
            splitResultTxt.Text = Shared.Settings.CenterIndiaModel.SplitResult;
            radioCamera.Checked = Shared.Settings.CenterIndiaModel.IsCamera;
            radioScanner.Checked = Shared.Settings.CenterIndiaModel.IsScanner;

            // Initialize database settings - always default to MySQL
            Shared.Settings.CenterIndiaModel.DatabaseType = "MySQL";
            txtServerName.Text = Shared.Settings.CenterIndiaModel.ServerName;
            txtPort.Text = Shared.Settings.CenterIndiaModel.Port;
            txtUsername.Text = Shared.Settings.CenterIndiaModel.Username;
            txtPassword.Text = Shared.Settings.CenterIndiaModel.Password;
            txtDatabaseName.Text = Shared.Settings.CenterIndiaModel.DatabaseName;

            txtLineName.Text = Shared.Settings.CenterIndiaModel.LineName;

            string currentUser = SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember");
            if (currentUser == "Admin") pnlSettingDayForDataDeletion.Visible = true;

            txtRetentionDays.Text = Shared.Settings.CenterIndiaModel.RetentionDays.ToString();
        }

        private void InitEvents()
        {
            TemplateNameTxt.TextChanged += TemplateNameTxt_TextChanged;
            splitResultTxt.TextChanged += splitResultTxt_TextChanged;
            radioCamera.CheckedChanged += radioCamera_CheckedChanged;
            radioScanner.CheckedChanged += radioScanner_CheckedChanged;

            // Database settings events
            txtServerName.TextChanged += txtServerName_TextChanged;
            txtPort.TextChanged += txtPort_TextChanged;
            txtUsername.TextChanged += txtUsername_TextChanged;
            txtPassword.TextChanged += txtPassword_TextChanged;
            txtDatabaseName.TextChanged += txtDatabaseName_TextChanged;

            txtLineName.KeyPress += txtLineName_KeyPress;
            txtLineName.Leave += txtLineName_Leave;
            txtLineName.TextChanged += txtLineName_TextChanged;

            txtRetentionDays.KeyPress += txtRetentionDays_KeyPress;
            txtRetentionDays.Leave += txtRetentionDays_Leave;
            txtRetentionDays.TextChanged += txtRetentionDays_TextChanged;
        }

        private void TemplateNameTxt_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.CenterIndiaModel.TemplateName = TemplateNameTxt.Text;
            Shared.SaveSettings();
        }

        private void splitResultTxt_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.CenterIndiaModel.SplitResult = splitResultTxt.Text;
            Shared.SaveSettings();
        }

        private void radioCamera_CheckedChanged(object sender, EventArgs e)
        {
            if (radioCamera.Checked)
            {
                Shared.Settings.CenterIndiaModel.IsCamera = true;
                Shared.Settings.CenterIndiaModel.IsScanner = false;
                Shared.SaveSettings();
            }
        }

        private void radioScanner_CheckedChanged(object sender, EventArgs e)
        {
            if (radioScanner.Checked)
            {
                Shared.Settings.CenterIndiaModel.IsCamera = false;
                Shared.Settings.CenterIndiaModel.IsScanner = true;
                Shared.SaveSettings();
            }
        }

        private void txtServerName_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.CenterIndiaModel.ServerName = txtServerName.Text;
            Shared.SaveSettings();
        }

        private void txtPort_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.CenterIndiaModel.Port = txtPort.Text;
            Shared.SaveSettings();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.CenterIndiaModel.Username = txtUsername.Text;
            Shared.SaveSettings();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.CenterIndiaModel.Password = txtPassword.Text;
            Shared.SaveSettings();
        }

        private void txtDatabaseName_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.CenterIndiaModel.DatabaseName = txtDatabaseName.Text;
            Shared.SaveSettings();
        }

        private void txtLineName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void txtLineName_Leave(object sender, EventArgs e)
        {
            if (int.TryParse(txtLineName.Text, out int val))
            {
                if (val < 1) txtLineName.Text = "1";
                else if (val > 99) txtLineName.Text = "99";
            }
            else
            {
                txtLineName.Text = "";
            }
        }

        private void txtLineName_TextChanged(object sender, EventArgs e)
        {
            Shared.Settings.CenterIndiaModel.LineName = txtLineName.Text;
            Shared.SaveSettings();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void txtRetentionDays_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void txtRetentionDays_Leave(object sender, EventArgs e)
        {
            if (int.TryParse(txtRetentionDays.Text, out int val))
            {
                if (val < 0) txtRetentionDays.Text = "0";
                else if (val > 365) txtRetentionDays.Text = "365";
            }
            else
            {
                txtRetentionDays.Text = "190";
            }
        }

        private void txtRetentionDays_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtRetentionDays.Text, out int val))
            {
                Shared.Settings.CenterIndiaModel.RetentionDays = val;
                Shared.SaveSettings();
            }
        }
    }
}
