using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.DevModeLabel;
using BarcodeVerificationSystem.Services;
using CommonVariable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static BarcodeVerificationSystem.Labels.DevModeLabel.DevMode;
using UILanguage;
using BarcodeVerificationSystem.Model;
using OperationLog.Controller;
using OperationLog.Model;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model.UserPermission;
using BarcodeVerificationSystem.Model.Payload;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Response;
using BarcodeVerificationSystem.Model.Apis.CaoSuDongNai;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;
using BarcodeVerificationSystem.Services.CaoSuDongNai.Interface;
using BarcodeVerificationSystem.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Services.Interface;

namespace BarcodeVerificationSystem.View.CaoSuDongNaiUI
{
    public partial class frmLoginCaoSu : Form
    {
        private bool _IsBinding = false;
        private bool _IsProcessing = false;
        private string _RememberPath = "";
        private ApiService apiService = new ApiService();
        CaoSuApiService _caoSuApiService = new CaoSuApiService();
        private Thread _ThreadLogin;

        public frmLoginCaoSu()
        {
            InitializeComponent();
            this.AcceptButton = btnLogin;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitControl();
            InitEvent();
            SetLanguage();

            if (DevMode.IsDevMode)
            {
                switch (DevMode.LabelType)
                {
                    case LoginLabel.AdminOnlineMode:
                        txtUsername.Text = "admin";
                        txtPassword.Text = "123456";
                        chbRememberPassword.Checked = true;
                        break;
                    case LoginLabel.AdminOfflineMode:
                        txtUsername.Text = "Administrator";
                        txtPassword.Text = "Admin@2025";
                        chbRememberPassword.Checked = true;
                        break;
                    case LoginLabel.SupportOfflineMode:
                        txtUsername.Text = "Support";
                        txtPassword.Text = "Support@2025";
                        chbRememberPassword.Checked = true;
                        break;
                    case LoginLabel.BinhduongXH:
                        txtUsername.Text = "binhduongpp";
                        txtPassword.Text = "binhduong@pp";
                        chbRememberPassword.Checked = true;
                        break;
                    case LoginLabel.OperatorOfflineMode:
                        txtUsername.Text = "Operator";
                        txtPassword.Text = "Operator@123";
                        chbRememberPassword.Checked = true;
                        break;

                    default:
                        txtUsername.Text = "";
                        txtPassword.Text = "";
                        chbRememberPassword.Checked = false;
                        break;
                }
                Login(txtUsername.Text, txtPassword.Text, chbRememberPassword.Checked);

            }

        }

        private void InitControl()
        {
            _IsBinding = true;
            KillThreadLogin();
            lblMessage.Visible = false;
            _RememberPath = CommVariables.PathSettingsApp;
            if (!Directory.Exists(_RememberPath))
            {
                Directory.CreateDirectory(_RememberPath);
            }
            _RememberPath += "remember.dat";
            try
            {
                if (File.Exists(_RememberPath))
                {
                    string[] texts = File.ReadAllLines(_RememberPath);
                    txtUsername.Text = SecurityController.Decrypt(texts[0], "rynan_encrypt_remember");
                    txtPassword.Text = SecurityController.Decrypt(texts[1], "rynan_encrypt_remember");
                    chbRememberPassword.Checked = true;
                }
            }
            catch
            { }
            _IsBinding = false;
        }

        private void InitEvent()
        {
            btnLogin.Click += ActionChanged;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
        }

        // 🔑 This is the global override for Enter/Space
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //if (keyData == Keys.Enter || keyData == Keys.Space)
                if (keyData == Keys.Space)
                {
                return true; // block both keys globally
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        private void ActionChanged(object sender, EventArgs e)
        {
            if (_IsBinding)
            {
                return;
            }
            if (sender == btnLogin)
            {
                if (txtUsername.Text == "" || txtPassword.Text == "")
                {
                    UpdateMessageLabel(true, false, Lang.UsernameOrPasswordCannotBeLeftBlank);
                }
                else
                {
                    Login(txtUsername.Text, txtPassword.Text, chbRememberPassword.Checked);
                }
            }
        }

        private async void Login(string username, string password, bool isRemenber)
        {
            if (_IsProcessing)
            {
                return;
            }
            var service = new PermissionService();

            KillThreadLogin();

            _ThreadLogin = new Thread(async () =>
            {
                _IsProcessing = true;
                UpdateMessageLabel(false, true, "");

                if (username.Trim() == "" || password == "")
                {
                    UpdateMessageLabel(true, false, Lang.UsernameOrPasswordCannotBeLeftBlank);
                }
                else
                {
                    if (isRemenber)
                    {
                        try
                        {
                            if (File.Exists(_RememberPath))
                            {
                                File.Delete(_RememberPath);
                            }
                            string ecryptUsername = SecurityController.Encrypt(txtUsername.Text, "rynan_encrypt_remember");
                            string ecryptPassword = SecurityController.Encrypt(txtPassword.Text, "rynan_encrypt_remember");
                            string[] content = { ecryptUsername, ecryptPassword };
                            File.WriteAllLines(_RememberPath, content);
                        }
                        catch
                        { }
                    }
                    else
                    {
                        if (File.Exists(_RememberPath))
                        {
                            File.Delete(_RememberPath);
                        }
                    }

                    ActivationStatus activationStatus = Shared.LoginLocal(username, password);
                    if (activationStatus == ActivationStatus.Successful)
                    {
                        Shared.UserPermission = username.ToLower() == "admin" || username.ToLower() == "administrator" || username.ToLower() == "support" || username == "demo" ? UserPermission.AdminPermission : UserPermission.OperatorPermission;

                        LoggingController.SaveHistory("Login success",
                            "Login",
                            "Login success: " + username,
                            username,
                            LoggingType.LogedIn);

                        Invoke(new Action(() =>
                        {
                            DialogResult = DialogResult.OK;
                        }));
                    }
                    else
                    {
                        bool isOnlineAccountOK = false;
                        try
                        {
                            RequestLogin loginPayload = new RequestLogin
                            {
                                user_name = username,
                                password = password
                            };

                            ResponseLogin LoginPayload = await _caoSuApiService.PostLoginAsync(loginPayload);
                            if (LoginPayload.success)
                            {
                                Shared.UserPermission = UserPermission.OperatorPermission;
                                Shared.UserPermission.isOnline = isOnlineAccountOK = true;
                                Shared.Settings.AccessToken = LoginPayload.data.accessToken;
                                Shared.Settings.RefreshToken = LoginPayload.data.refreshToken;
                                Shared.Settings.DomainQRCode = LoginPayload.data.domainQRCode;
                                int expireIn = LoginPayload.data.expiresIn;
                                TokenRefresher.Instance.StartAutoRefresh(expireIn);
                                //CaoSuApiService caoSuApiService = new CaoSuApiService();
                                //var response = await caoSuApiService.PostRefreshTokenAsync();
                            }
                            Shared.SaveSettings();

                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show("Failed to load permissions: " + ex.Message);
                            //return;
                        }

                        if (isOnlineAccountOK)
                        {
                            //activationStatus = Shared.LoginLocal("admin", "123456");
                            activationStatus = Shared.LoginLocal("Administrator", "Admin@2025");

                            LoggingController.SaveHistory("Login success",
                           "Login",
                           "Login success: " + username,
                           username,
                           LoggingType.LogedIn);

                            Invoke(new Action(() =>
                            {
                                DialogResult = DialogResult.OK;
                            }));
                        }
                        else
                        {
                            UpdateMessageLabel(true, false, Lang.UsernameOrPasswordIsIncorrect);
                            LoggingController.SaveHistory("Login error",
                                "Login",
                                "Password incorrect!",
                                username,
                                LoggingType.LogedIn);
                        }

                    }
                }

                _IsProcessing = false;
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadLogin.Start();
        }

        private void KillThreadLogin()
        {
            if (_ThreadLogin != null && _ThreadLogin.IsAlive)
            {
                _ThreadLogin.Abort();
                _ThreadLogin = null;
            }
            _IsProcessing = false;
        }

        private void UpdateMessageLabel(bool isVisible, bool isNormalState, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateMessageLabel(isVisible, isNormalState, message)));
                return;
            }
            picLoading.Visible = !isVisible;
            lblMessage.Visible = isVisible;
            if (isNormalState)
            {
                lblMessage.ForeColor = Color.Black;
            }
            else
            {
                lblMessage.ForeColor = Color.Red;
            }
            lblMessage.Text = message;

            txtUsername.Enabled = isVisible;
            txtPassword.Enabled = isVisible;
            chbRememberPassword.Enabled = isVisible;
            btnLogin.Enabled = isVisible;
        }

        private void SetLanguage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }
            lblLogIn.Text = Lang.Login.ToUpper();
            lblUsername.Text = Lang.Username;
            lblPassword.Text = Lang.Password;
            chbRememberPassword.Text = Lang.RememberPassword;
            lblMessage.Text = Lang.UsernameOrPasswordIsIncorrect;
            btnLogin.Text = Lang.Login.ToUpper();
            labelSoftwareName.Text = Lang.BarcodeVerificationSystem;
        }
    }

}
