using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.DevModeLabel;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Apis.THTrueMilk;
using BarcodeVerificationSystem.Model.Payload;
using BarcodeVerificationSystem.Model.THTrueMilk;
using BarcodeVerificationSystem.Model.UserPermission;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster;
using BarcodeVerificationSystem.View.CustomDialogs;
using CommonVariable;
using OperationLog.Controller;
using OperationLog.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;
using THTrueMilkMonitor = BarcodeVerificationSystem.Services.THTrueMilk.MonitorSenderService;
using BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing;
using static BarcodeVerificationSystem.Labels.DevModeLabel.DevMode;

namespace BarcodeVerificationSystem.View.THTrueMilkUI
{
    public partial class frmLoginTHTrueMilk : Form
    {
        private bool _IsBinding = false;
        private bool _IsProcessing = false;
        private string _RememberPath = "";

        private const int CS_DropShadow = 0x00020000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ClassStyle |= CS_DropShadow;
                return createParams;
            }
        }

        public frmLoginTHTrueMilk()
        {
            InitializeComponent();
            this.AcceptButton = btnLogin;
            chbRememberPassword.Checked = false;
            panelDrag.Paint += PanelDrag_Paint;
            this.Paint += FrmLogin_Paint;
           // WindowState = FormWindowState.Maximized;
        }

        private void FrmLogin_Paint(object sender, PaintEventArgs e)
        {
            using (Pen borderPen = new Pen(Color.DarkSlateBlue, 2))
            {
                Rectangle rect = new Rectangle(3, 3, this.ClientSize.Width - 2, this.ClientSize.Height - 2);
                e.Graphics.DrawRectangle(borderPen, rect);
            }
        }

        private void PanelDrag_Paint(object sender, PaintEventArgs e)
        {
            using (Pen borderPen = new Pen(Color.FromArgb(80, 80, 80), 1))
            {
                Rectangle rect = new Rectangle(0, 0, panelDrag.ClientSize.Width - 1, panelDrag.ClientSize.Height - 1);
                e.Graphics.DrawRectangle(borderPen, rect);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitControl();
            InitEvent();
            SetLanguage();
           // this.ActiveControl = txtUsername;

            THTrueMilkMonitor.SendParametersToServer();

            if (DevMode.IsDevMode)
            {
                switch (DevMode.LabelType)
                {
                    case LoginLabel.AdminOnlineMode:
                        txtUsername.Text = "admin";
                        txtPassword.Text = "admin@123";
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

        // ─────────────────────────────────────────────────────────────────────
        #region Init
        // ─────────────────────────────────────────────────────────────────────

        private void InitControl()
        {
            _IsBinding = true;
            lblMessage.Visible = false;
            checkBox1.Visible = false;
            _RememberPath = CommVariables.PathSettingsApp;
            if (!Directory.Exists(_RememberPath))
                Directory.CreateDirectory(_RememberPath);
            _RememberPath += "remember.dat";
            _IsBinding = false;
        }

        private void InitEvent()
        {
            btnLogin.Click += ActionChanged;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            FormClosing += FrmMain_FormClosing;
            Load += FrmLoginTHTrueMilk_Load;
            Shown += (s, e) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    // Đảm bảo form là foreground window trước
                    this.Activate();
                    this.BringToFront();

                    // Focus trực tiếp inner textBox1
                    var field = txtUsername.GetType().GetField("textBox1",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        var inner = field.GetValue(txtUsername) as TextBox;
                        if (inner != null)
                        {
                            inner.Focus();
                            inner.SelectAll();
                        }
                    }
                    else
                    {
                        txtUsername.Focus();
                    }
                }));
            };
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Form Events
        // ─────────────────────────────────────────────────────────────────────

        private void FrmLoginTHTrueMilk_Load(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(_RememberPath))
                {
                    string[] texts = File.ReadAllLines(_RememberPath);
                    if (texts.Length >= 2)
                    {
                        txtUsername.Text = SecurityController.Decrypt(texts[0], "rynan_encrypt_remember");
                        txtPassword.Text = SecurityController.Decrypt(texts[1], "rynan_encrypt_remember");
                        chbRememberPassword.Checked = true;
                    }
                }
            }
            catch { }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (ActiveControl == txtUsername || ActiveControl == txtPassword)
                {
                    if (txtUsername.Text != "" && txtPassword.Text != "")
                        Login(txtUsername.Text, txtPassword.Text, chbRememberPassword.Checked);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ApiService apiService = new ApiService();
                await THTrueMilkMonitor.sendParametersToServerAsync(apiService, false);
            }
            catch { }
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e) => SetLanguage();

        private void ActionChanged(object sender, EventArgs e)
        {
            if (_IsBinding) return;
            if (sender == btnLogin)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                    UpdateMessageLabel(true, false, Lang.UsernameOrPasswordCannotBeLeftBlank);
                else
                    Login(txtUsername.Text, txtPassword.Text, chbRememberPassword.Checked);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Login
        // ─────────────────────────────────────────────────────────────────────

        private async void Login(string username, string password, bool isRemember)
        {
            if (_IsProcessing) return;
            _IsProcessing = true;
            UpdateMessageLabel(false, true, "");

            try
            {
                SaveRemember(username, password, isRemember);

                // ══════════════════════════════════════════════════════════════
                // BƯỚC 1: Ưu tiên đăng nhập online R-Link
                // ══════════════════════════════════════════════════════════════
                Console.WriteLine("[Login] Thử R-Link online: " + username);
                bool onlineOk = await TryLoginOnlineSyncAsync(username, password);

                if (onlineOk)
                {
                    return;
                }

                // ══════════════════════════════════════════════════════════════
                // BƯỚC 2: Online fail → fallback offline SQLite
                // ══════════════════════════════════════════════════════════════
                Console.WriteLine("[Login] R-Link fail → thử offline SQLite: " + username);
                ActivationStatus localStatus = await Task.Run(() => Shared.LoginLocal(username, password));

                if (localStatus == ActivationStatus.Successful)
                {
                    ApplyLocalPermission(username);
                    await LogAsync("Login (offline SQLite): " + username, username, true);
                    Console.WriteLine("[Login] Offline SQLite OK: " + username);
                    RLinkMasterServiceFactory.Instance.StoreCredentials(username, password);
                    CompleteLogin();
                    Console.WriteLine("[Login] CompleteLogin() offline, DialogResult=OK");
                }
                else
                {
                    UpdateMessageLabel(true, false, Lang.UsernameOrPasswordIsIncorrect);
                    await LogAsync("Login fail (online + offline): " + username, username, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Login] Unhandled: " + ex.Message);
                UpdateMessageLabel(true, false, "Lỗi: " + ex.Message);
            }
            finally
            {
                _IsProcessing = false;
            }
        }

        private void SaveRemember(string username, string password, bool isRemember)
        {
            try
            {
                if (isRemember)
                {
                    if (File.Exists(_RememberPath)) File.Delete(_RememberPath);
                    File.WriteAllLines(_RememberPath, new[]
                    {
                        SecurityController.Encrypt(username, "rynan_encrypt_remember"),
                        SecurityController.Encrypt(password, "rynan_encrypt_remember")
                    });
                }
                else
                {
                    if (File.Exists(_RememberPath)) File.Delete(_RememberPath);
                }
            }
            catch { }
        }

        // ── Thay thế ApplyLocalPermission() cũ ─────────────────────────────────────
        private void ApplyLocalPermission(string username)
        {
            // Ưu tiên permissions đã cache từ lần login R-Link trước
            var cached = RLinkLogService.LoadPermissionsFromSQLite(username);
            if (cached != null && cached.Count > 0)
            {
                Shared.UserPermission = new UserPermission { isOnline = false, Permissions = cached };
                Console.WriteLine($"[Login] Loaded cached permissions cho '{username}' từ SQLite.");
            }
            else
            {
                // Fallback: hardcoded theo username (lần đầu chưa có cache)
                string userLower = username.ToLower();
                bool isAdmin = userLower == "administrator" || userLower == "support"
                            || userLower == "admin" || userLower == "demo";
                Shared.UserPermission = isAdmin
                    ? UserPermission.AdminPermission
                    : UserPermission.OperatorPermission;
                Console.WriteLine($"[Login] Dùng fallback permission (chưa có cache) cho '{username}'.");
            }
            UserController.LogedInUsername = username;
        }

        /// <summary>Đăng nhập online R-Link. Trả về true nếu thành công.</summary>
        private async Task<bool> TryLoginOnlineSyncAsync(string username, string password)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                RLinkMasterServiceFactory.Instance.StoreCredentials(username, password);
                var rlinkService = RLinkMasterServiceFactory.Instance;
                var loginResult = await rlinkService.LoginAsync(username, password);
                Console.WriteLine($"[LOGIN] LoginAsync: {sw.ElapsedMilliseconds}ms");

                if (loginResult != null && loginResult.IsSuccess)
                {
                    int role = int.TryParse(loginResult.Role, out int r) ? r : 1;

                    string onlineUserId = !string.IsNullOrEmpty(loginResult.UserId)
                        && !loginResult.UserId.Equals(username, StringComparison.OrdinalIgnoreCase)
                        ? loginResult.UserId
                        : UserController.GetCachedUserId(username);

                    if (string.IsNullOrWhiteSpace(onlineUserId))
                    {
                        onlineUserId = "ACC001";
                        //ProjectLogger.WriteWarning($"[Login] userId không hợp lệ cho '{username}', dùng fallback 'ACC001'");
                    }

                    Shared.LoggedInUser = new UserDataModel
                    {
                        UserName = SecurityController.Encrypt(loginResult.Username, "rynan_encrypt_remember"),
                        FullName = loginResult.FullName,
                        Role = role,
                        UserId = onlineUserId,
                        OperatorUserId = onlineUserId
                    };
                    Shared.UserPermission = new UserPermission
                    {
                        isOnline = true,
                        Permissions = loginResult.Permissions
                    };
                    UserController.LogedInUsername = username;
                    Properties.Settings.Default.Username = username;
                    Properties.Settings.Default.Save();

                    await Task.Run(() => CacheAccountToSQLite(loginResult.FullName, username, password, role));
                    Console.WriteLine($"[LOGIN] CacheAccountToSQLite: {sw.ElapsedMilliseconds}ms");

                    UserController.UpdateUserId(username, onlineUserId);

                    RLinkLogService.SavePermissionsToSQLite(username, loginResult.Permissions);
                    Console.WriteLine($"[LOGIN] SavePermissionsToSQLite: {sw.ElapsedMilliseconds}ms");

                    Console.WriteLine("[Login] Bắt đầu SyncSettingsAndProductsAsync...");
                    await SyncSettingsAndProductsAsync(rlinkService);
                    Console.WriteLine($"[LOGIN] SyncSettingsAndProductsAsync: {sw.ElapsedMilliseconds}ms");
                    Console.WriteLine("[Login] SyncSettingsAndProductsAsync hoàn tất.");

                    await LogAsync("Login (R-Link online): " + username, username, true);
                    Console.WriteLine($"[LOGIN] LogAsync: {sw.ElapsedMilliseconds}ms");
                    Console.WriteLine("[Login] R-Link online OK: " + username);
                    _ = Task.Run(() => RLinkMonitorService.Instance?.SendImmediate());
                    CompleteLogin();
                    Console.WriteLine($"[LOGIN] TOTAL: {sw.ElapsedMilliseconds}ms");
                    Console.WriteLine("[Login] CompleteLogin() done (R-Link)");
                    return true;
                }
                else
                {
                    Console.WriteLine("[Login] R-Link login fail: " + loginResult?.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Login] R-Link không kết nối: " + ex.Message);
            }
            Console.WriteLine($"[LOGIN] TOTAL FAIL: {sw.ElapsedMilliseconds}ms");
            return false;
        }

        private void CacheAccountToSQLite(string fullName, string username, string password, int role)
        {
            try
            {
                UserController.DeleteAccount(username);
                UserController.AddAccount(fullName, username, password, role);
                Console.WriteLine("[Login] Cached account '" + username + "' → SQLite.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Login] Cache SQLite bỏ qua: " + ex.Message);
            }
        }

        private async Task SyncSettingsAndProductsAsync(IRLinkMasterService rlinkService)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string lineId = Shared.Settings.LineId;
            string lineName = Shared.Settings.LineName ?? "";
            string factoryCode = Shared.Settings.FactoryCode ?? "";
            string factoryName = Shared.Settings.FactoryName ?? "";
            string localIp = Shared.GetLocalIPAddress();

            // ── 1. Gọi 3 API SONG SONG ──────────────────────────────────────
            var accounts = new List<AccountInfo>();
            RLinkSettings settings = null;
            var products = new List<ProductItem>();
            QrConfig qrConfig = null;

            var t1 = System.Threading.Tasks.Task.Run(async () =>
            {
                try { var a = await rlinkService.GetAccountsAsync(); if (a != null) accounts.AddRange(a); }
                catch { Console.WriteLine("[Login] GetAccounts bỏ qua"); }
            });
            var t2 = System.Threading.Tasks.Task.Run(async () =>
            {
                try { settings = await rlinkService.GetSettingsAsync(lineId); }
                catch { Console.WriteLine("[Login] GetSettings bỏ qua"); }
            });
            var t3 = System.Threading.Tasks.Task.Run(async () =>
            {
                try { var p = await rlinkService.GetProductsAsync(); if (p != null) products.AddRange(p); }
                catch { Console.WriteLine("[Login] GetProducts bỏ qua"); }
            });
            var t4 = System.Threading.Tasks.Task.Run(async () =>
            {
                try { qrConfig = await rlinkService.GetQrConfigAsync(); }
                catch { Console.WriteLine("[Login] GetQrConfig bỏ qua"); }
            });

            await System.Threading.Tasks.Task.WhenAll(t1, t2, t3, t4);
            Console.WriteLine($"[SYNC] 4 APIs: {sw.ElapsedMilliseconds}ms");

            // ── 2. Apply RAM (không await, không DB) ─────────────────────────
            if (settings != null)
            {
                Shared.Settings.THOperatingMode = (THTrueMilkOperatingMode)settings.OperatingMode;
                Shared.Settings.THDeltaMinutes = settings.DeltaMinutes;
                Shared.Settings.THNMinutes = settings.NMinutes;
                Shared.Settings.THBufferCount = settings.BufferCount;
                Shared.Settings.THMonitorInterval = settings.MonitoringIntervalMinutes;
                Shared.Settings.THLogInterval = settings.LogIntervalMinutes;
                Shared.Settings.THQrThreshold = settings.QrThreshold;
                Shared.Settings.THErrorImageFolder = settings.ErrorImageFolder;
                Shared.Settings.THMaxConsecutiveError = settings.MaxConsecutiveDefects;
                Shared.Settings.THReserveFactor = settings.ReserveFactor;
                Shared.SaveSettings();
                Console.WriteLine("[Login] Settings synced từ R-Link → RAM + XML.");
            }

            // ── 2b. Apply QR config ──
            if (qrConfig != null)
            {
                Shared.Settings.THQrBaseUrl = qrConfig.BaseUrl ?? "";
                Shared.Settings.THQrNumberOfUrl = qrConfig.NumberOfUrl;
                Shared.SaveSettings();
                Console.WriteLine($"[Login] QR config synced: baseUrl={qrConfig.BaseUrl}, numberOfUrl={qrConfig.NumberOfUrl}");
            }

            // ── 3. Local DB writes SONG SONG ────────────────────────────────
            var swDb = System.Diagnostics.Stopwatch.StartNew();
            var dbTasks = new List<System.Threading.Tasks.Task>();

            if (accounts != null && accounts.Count > 0)
            {
                dbTasks.Add(System.Threading.Tasks.Task.Run(async () =>
                {
                    Debug.WriteLine($"[DB] SyncAccounts START ({accounts.Count} accounts)");
                    var swA = System.Diagnostics.Stopwatch.StartNew();
                    await LocalAccountStore.SyncAccountsFromRLinkAsync(accounts);
                    Debug.WriteLine($"[DB] SyncAccounts DONE: {swA.ElapsedMilliseconds}ms");
                }));
            }

            if (settings != null)
            {
                dbTasks.Add(System.Threading.Tasks.Task.Run(async () =>
                {
                    Debug.WriteLine("[DB] SaveSettings START");
                    var swS = System.Diagnostics.Stopwatch.StartNew();
                    await LocalAccountStore.SaveSettingsAsync(lineId, settings);
                    Debug.WriteLine($"[DB] SaveSettings DONE: {swS.ElapsedMilliseconds}ms");
                }));
                dbTasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    Debug.WriteLine("[DB] SaveConfigLine START");
                    var swC = System.Diagnostics.Stopwatch.StartNew();
                    RLinkLogService.SaveConfigLine(lineId, lineName, factoryCode, factoryName, localIp,
                        (int)Shared.Settings.THOperatingMode, Shared.Settings.THBufferCount);
                    Debug.WriteLine($"[DB] SaveConfigLine DONE: {swC.ElapsedMilliseconds}ms");
                }));
                dbTasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    Debug.WriteLine("[DB] SaveConfig START");
                    var swCfg = System.Diagnostics.Stopwatch.StartNew();
                    RLinkLocalLogService.SaveConfig(settings, lineId);
                    Debug.WriteLine($"[DB] SaveConfig DONE: {swCfg.ElapsedMilliseconds}ms");
                }));
            }

            if (products != null && products.Count > 0)
            {
                Shared.Settings.THProductList = products;

                // Preload URL gốc trước để SyncProductsAsync có thể lưu đúng URL vào DB
                ProductImageHelper.PreloadImagesToCache(products);

                // Tải ảnh sản phẩm trong nền (không block login)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        int count = await ProductImageHelper.DownloadProductImagesAsync(products, forceRefresh: true);
                        Debug.WriteLine($"[Login] Downloaded {count} product images in background.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Login] Download product images failed: {ex.Message}");
                    }
                });

                dbTasks.Add(System.Threading.Tasks.Task.Run(async () =>
                {
                    Debug.WriteLine($"[DB] SyncProducts START ({products.Count} products)");
                    var swP = System.Diagnostics.Stopwatch.StartNew();
                    await LocalAccountStore.SyncProductsAsync(products);
                    Debug.WriteLine($"[DB] SyncProducts DONE: {swP.ElapsedMilliseconds}ms");
                }));
                dbTasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    Debug.WriteLine("[DB] SaveProducts START");
                    var swPr = System.Diagnostics.Stopwatch.StartNew();
                    RLinkLocalLogService.SaveProducts(products);
                    Debug.WriteLine($"[DB] SaveProducts DONE: {swPr.ElapsedMilliseconds}ms");
                }));
            }

            // DB writes chạy background — không block UI
            _ = Task.Run(async () =>
            {
                try { await Task.WhenAll(dbTasks); }
                catch (Exception ex) { Debug.WriteLine($"[DB] sync error: {ex.Message}"); }
                Debug.WriteLine($"[DB] ALL DB writes TOTAL: {swDb.ElapsedMilliseconds}ms");
            });

            // ── Persist settings ────────────────────────────────────────────
            try { Shared.SaveSettings(); } catch (Exception ex) { Console.WriteLine("[Login] SaveSettings: " + ex.Message); }
            Console.WriteLine($"[SYNC] TOTAL: {sw.ElapsedMilliseconds}ms");
        }

        private void CompleteLogin()
        {
            this.Tag = "NeedRefreshProducts";
            Shared.PendingLoginRed = true;
            DialogResult = DialogResult.OK;
        }

        private Task LogAsync(string detail, string username, bool isSuccess)
        {
            return Task.Run(() =>
            {
                try
                {
                    LoggingController.SaveHistory(
                        isSuccess ? "Login success" : "Login error",
                        "Login", detail, username, LoggingType.LogedIn);
                }
                catch { }
            });
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region UI Helpers
        // ─────────────────────────────────────────────────────────────────────

        private void UpdateMessageLabel(bool isVisible, bool isNormalState, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateMessageLabel(isVisible, isNormalState, message)));
                return;
            }
            picLoading.Visible = !isVisible;
            lblMessage.Visible = isVisible;
            lblMessage.ForeColor = isNormalState ? Color.Black : Color.Red;
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
            labelSoftwareName.Text = Lang.BarcodeVerificationSystemQr;
        }

        #endregion
    }
}