using BarcodeVerificationSystem.Controller;
using System;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmChangePassword : Form
    {
        public FrmChangePassword()
        {
            InitializeComponent();
            InitControl();
            InitEvents();
            SetLanguage();
        }

        private void SetLanguage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }
            lblCurrentPassword.Text = Lang.CurrentPassword;
            lblNewPassword.Text = Lang.NewPassword;
            lblRetypePassword.Text = Lang.RetypePassword;
            btnOK.Text = Lang.OK;
            btnCancel.Text = Lang.Cancel;
            lblFormName.Text = Lang.ChangePassword;
        }

        public void InitControl()
        {
            lblUsername.Text = UserController.LogedInUsername;
        }

        public void InitEvents()
        {
            btnCancel.Click += ButtonClicked;
            btnOK.Click += ButtonClicked;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
        }

        private void Shared_OnLanguageChange(object sender,EventArgs e)
        {
            SetLanguage();
        }

        private void ButtonClicked(object sender, EventArgs e)
        {
            if (sender == btnCancel)
            {
                Close();
            }
            else if (sender == btnOK)
            {
                ProcessChangePassword();
            }
        }


        private void ProcessChangePassword()
        {
            string oldPassword = txtCurrentPassword.Text;
            _ = SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember");
            if (UserController.CheckCorrectPassword(oldPassword) == false)
            {
                lblMessage.Text = Lang.InvalidPassword;
                lblMessage.Visible = true;
                return;
            }

            //check password length
            string newPass = txtNewPassword.Text;
            if (newPass.Length < 6)
            {
                lblMessage.Text = Lang.PasswordAtLeast6Character;
                lblMessage.Visible = true;
                return;
            }

            //check re-type password
            string retypePass = txtRetypePassword.Text;
            if (retypePass != newPass)
            {
                lblMessage.Text = Lang.PasswordNotMatch;
                lblMessage.Visible = true;
                return;
            }

            //do change password
            bool result = UserController.ChangePassword(Shared.LoggedInUser.UserName, SecurityController.Encrypt(newPass, "rynan_encrypt_remember"));
            if (result)
            {
                Close();
            }
            else
            {
                lblMessage.Visible = true;
                return;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Enter:
                    ProcessChangePassword();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
