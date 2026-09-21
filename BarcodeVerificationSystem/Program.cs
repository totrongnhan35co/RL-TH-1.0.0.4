using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.View;
using BarcodeVerificationSystem.View.CaoSuDongNaiUI;
using BarcodeVerificationSystem.View.NutrifoodUI;
using BarcodeVerificationSystem.View.WokaUI;
using BarcodeVerificationSystem.View.DrocoUI;
using BarcodeVerificationSystem.View.OtherProjects.CenteryIndiaUI;
using BarcodeVerificationSystem.Services.WindowsUpdate;
using CommonVariable;
using EncrytionFile.Model;
using OperationLog.Controller;
using OperationLog.Model;
using Securedongle;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UILanguage;
using BarcodeVerificationSystem.View.OtherProjects.THMilkUI;
using BarcodeVerificationSystem.View.THTrueMilkUI;
namespace BarcodeVerificationSystem
{
    static class Program
    {
        #region Variables
        private static USBKey _USBKey = new USBKey();
        private static uint _HardwareIDUsing = 0;
        private static FrmWarningUSBDongleKey frmWarningKey = null;
        private static WindowsUpdateService _windowsUpdateService = null;
        private static FrmWarningWindowsUpdate _windowsUpdateWarningForm = null;
        #endregion

        [STAThread]
        static void Main()
        {
            try
            {
                // Enable TLS 1.2 for HTTPS connections (required for modern servers in Release mode)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                if (AnotherInstanceExists())
                {
                    MessageBox.Show(Lang.ApplicationIsAlreadyRunning, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Shared.LoadSettings();

                if (ProjectLabel.IsCenteryIndia && (string.IsNullOrEmpty(Shared.Settings.Language) || Shared.Settings.Language == "vi-VN"))
                {
                    Shared.Settings.Language = "en-US";
                }

                Lang.Culture = System.Globalization.CultureInfo.CreateSpecificCulture(Shared.Settings.Language); // Set init language
               FrmSplashScreen.ShowSplashScreen(Lang.Loading, Lang.PleaseWait); //Show splash screen

                bool isAllow = false; // true for bypass donglekey usb
                try
                {

                    var uuid = DecryptionHardwareID.GetUniqueID(); // Get PC UUID
                    var keyByte = Encoding.UTF8.GetBytes(uuid); // Convert UUID to bytes
                    var pathKey = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\R-Link"; // Create Folder in app data
                    if (!Directory.Exists(pathKey))
                    {
                        Directory.CreateDirectory(pathKey);
                    }
                    File.WriteAllBytes(pathKey + "\\UUID.txt", keyByte); // Save key to file use for bypass
                    string pathRconfig = CommVariables.PathAllowPC + "RConfig.dat"; // Read file .dat
                    if (!Directory.Exists(CommVariables.PathAllowPC))
                    {
                        Directory.CreateDirectory(CommVariables.PathAllowPC);
                    }
                    if (!Directory.Exists(CommVariables.PathImagesError))
                    {
                        Directory.CreateDirectory(CommVariables.PathImagesError);
                    }
                    DecryptionHardwareID.DecryptFile_UUID(pathRconfig); // Descript file .dat
                                                                        // Sau dòng: DecryptionHardwareID.DecryptFile_UUID(pathRconfig);

                    // ── Chống lùi đồng hồ hệ thống ──────────────────────────────
                    if (Shared.LicenseExpireDate != DateTime.MaxValue)
                    {
                        const string regPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\RLinkSvc";
                        const string regValue = "LastSyncTime";
                        const string regKey2 = "RhapsodosZyl_ft_Tieunhan1st"; // dùng chung salt

                        try
                        {
                            using (var regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(regPath))
                            {
                                string stored = regKey.GetValue(regValue) as string;
                                if (!string.IsNullOrEmpty(stored))
                                {
                                    // XOR-decode đơn giản
                                    byte[] storedBytes = Convert.FromBase64String(stored);
                                    byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(regKey2);
                                    for (int i = 0; i < storedBytes.Length; i++)
                                        storedBytes[i] ^= keyBytes[i % keyBytes.Length];
                                    string lastSeenStr = System.Text.Encoding.UTF8.GetString(storedBytes);

                                    if (DateTime.TryParse(lastSeenStr, out DateTime lastSeen))
                                    {
                                        // Đồng hồ bị lùi → chặn
                                        if (DateTime.Today < lastSeen)
                                            Shared.listPCAllow?.Clear();

                                        // Registry đã ghi nhận ngày vượt hạn → chặn
                                        if (lastSeen > Shared.LicenseExpireDate)
                                            Shared.listPCAllow?.Clear();
                                    }
                                }

                                // Lưu ngày hôm nay (XOR-encode)
                                byte[] todayBytes = System.Text.Encoding.UTF8.GetBytes(DateTime.Today.ToString("yyyy-MM-dd"));
                                byte[] keyB = System.Text.Encoding.UTF8.GetBytes(regKey2);
                                for (int i = 0; i < todayBytes.Length; i++)
                                    todayBytes[i] ^= keyB[i % keyB.Length];
                                regKey.SetValue(regValue, Convert.ToBase64String(todayBytes));
                            }
                        }
                        catch { }
                    }
                    foreach (HardwareIDModel id in Shared.listPCAllow) // Compare byte in descript data
                    {
                        var foundKeyByte = Encoding.UTF8.GetBytes(id.HardwareID);
                        for (int i = 0; i < foundKeyByte.Length; i++)
                        {
                            isAllow = true; // Allow if complete compare key
                            if (foundKeyByte[i] != keyByte[i] || foundKeyByte.Length != keyByte.Length)
                            {
                                isAllow = false;
                                break;
                            }
                        }
                    }
                    Shared.listPCAllow = null;
                }
                catch (Exception) { }

                if (!isAllow) // Check usb dongle 
                {
                    try
                    {
                        InitVariableUSBDongle(_newKey);
                        if (CheckForValidUSBDongleKey() == false)
                        {
                            InitVariableUSBDongle(_oldKey);
                            if (CheckForValidUSBDongleKey() == false)
                            {
                                ShowDialogUSBDongleKeyNotFound();
                                return;
                            }
                            //ShowDialogUSBDongleKeyNotFound();
                            //return;
                        }
                        Thread threadCheckUSBDongle = new Thread(() => CheckUSBDongleWhenRunning())
                        {
                            IsBackground = true,
                            Priority = ThreadPriority.Lowest
                        };
                        threadCheckUSBDongle.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error USB Key  {ex.Message}");
                    }
                }

                Application.EnableVisualStyles();
                LoggingController.LoginToAccess("_rynan_loggin_access_control_management_");
                
                // Check Windows Update status on startup
                //CheckWindowsUpdateStatus();
                
                string path = CommVariables.PathAccountsApp;
                if (!File.Exists(path + "AccountDB.db"))
                {
                    UserController.CreateDefaultDatabase();
                }
                DialogResult result = DialogResult.None;

                var loginform = new Form(); // Base form as fallback

                switch (true)
                {
                    case true when ProjectLabel.IsCenteryIndia:
                        loginform = new FrmLoginCentery
                        {
                            TopMost = true
                        };
                        break;

                    case true when ProjectLabel.IsNutrifood:
                        loginform = new frmLoginNutri
                        {
                            TopMost = true
                        };
                        break;

                    case true when ProjectLabel.IsCaoSuDongNai:
                        loginform = new frmLoginCaoSu
                        {
                            TopMost = true
                        };
                        break;

                    case true when ProjectLabel.IsWoka:
                        loginform = new FrmLoginWoka
                        {
                            TopMost = true
                        };
                        break;
                    case true when ProjectLabel.IsDroco:
                        loginform = new FrmLoginDroco
                        {
                            TopMost = true
                        };
                        break;
                    case true when ProjectLabel.IsTHMilk:
                        loginform = new FrmLoginTH
                        {
                            TopMost = true
                        };
                        break;
                    case true when ProjectLabel.IsTHTrueMilk:
                        loginform = new frmLoginTHTrueMilk
                        {
                            TopMost = true
                        };
                        break;
                    // Optional: default case if no condition matches (keeps the base Form)
                    case true when ProjectLabel.IsDefault:
                    default:
                        loginform = new FrmLoginNew
                        {
                            TopMost = true
                        };
                        break;
                }

                FrmSplashScreen.CloseSplash();
                result = loginform.ShowDialog();

                if (result == DialogResult.OK)
                {
                    UserController.LogedInUsername = "Administrator";
                    AppDomain currentDomain = default;
                    currentDomain = AppDomain.CurrentDomain;
                    currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
                    Application.ThreadException += GlobalThreadExceptionHandler;
                    Form form = null;

                    switch (true)
                    {
                        case true when ProjectLabel.IsCenteryIndia:
                            form = new View.OtherProjects.CenteryIndiaUI.FrmJobCentery();
                            break;
                        case true when ProjectLabel.IsNutrifood && Shared.Settings.IsManufacturingMode:
                            form = new View.NutrifoodUI.Manufacturing.frmJobNutri();
                            break;

                        case true when ProjectLabel.IsNutrifood: // IsNutrifood but not ManufacturingMode
                            form = new frmJobNutri();
                            break;

                        case true when ProjectLabel.IsCaoSuDongNai:
                            form = new frmJobCaoSu();
                            break;
                        case true when ProjectLabel.IsDroco:
                            form = new FrmJobDroco();
                            break;
                        case true when ProjectLabel.IsWoka:
                            form = new FrmJobWoka();
                            break;

                        case true when ProjectLabel.IsTHMilk:
                            form = new FrmJobTH();
                            break;

                        case true when ProjectLabel.IsTHTrueMilk && Shared.Settings.IsManufacturingMode:
                            form = new View.THTrueMilkUI.Manufacturing.frmJobTHTrueMilk();
                            break;

                        case true when ProjectLabel.IsTHTrueMilk:
                            form = new frmJobTHTrueMilk();
                            break;

                       
                        case true when ProjectLabel.IsDefault:
                        default:
                            form = new FrmJob();
                            break;
                    }
                    // Start monitoring for imminent Windows restarts
                    StartWindowsUpdateMonitoring();
                    
                    Application.Run(form);
                    
                    // Stop monitoring when application closes
                    StopWindowsUpdateMonitoring();
                }
            }
            catch (Exception) { }
        }

        #region UtilityFunction

        public static bool AnotherInstanceExists()
        {
            var currentRunningProcess = Process.GetCurrentProcess();
            var listOfProcs = Process.GetProcessesByName(currentRunningProcess.ProcessName);
            foreach (Process proc in listOfProcs)
            {
                if ((proc.MainModule.FileName == currentRunningProcess.MainModule.FileName) && (proc.Id != currentRunningProcess.Id))
                {
                    //proc.Kill();
                    return true;
                }
            }
            return false;
        }

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            var result = "";
            if (ex.InnerException != null)
            {
                string innerExection = "InnerException: " + ex.InnerException.InnerException + " && ";
                result += innerExection;
            }
            if (ex.Message != null)
            {
                string errorMessage = ex.Message.Replace("\r", "").Replace("\n", "").Replace(',', '&');
                result += "Message: " + errorMessage;
            }
            if (ex.Source != null)
            {
                string errorSource = ex.Source;
                result += " && Source: " + errorSource;
            }
            if (ex.StackTrace != null)
            {
                var stackTrace = new StackTrace(ex, true);
                foreach (StackFrame stackFrame in stackTrace.GetFrames())
                {
                    string methodName = stackFrame.GetMethod().Name;
                    int lineNumber = stackFrame.GetFileLineNumber();
                    if (methodName != "" && lineNumber != 0)
                    {
                        result += " && Method: " + methodName + " line " + lineNumber;
                    }
                }
            }
            if (ex.TargetSite != null)
            {
                string targetSite = " && TargetSite: " + ex.TargetSite.ToString() + " - " + ex.TargetSite.DeclaringType.ToString();
                result += targetSite;
            }
            result = result.Replace("'", "");
            //LoggingController.SaveHistory(
            //    string.Format("Unhandled Exception"),
            //    Lang.Error,
            //    string.Format(result),
            //    SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
            //    LoggingType.Error);
        }

        private static void GlobalThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            string result = "";
            if (ex.InnerException != null)
            {
                string innerExection = "InnerException: " + ex.InnerException.InnerException + " && ";
                result += innerExection;
            }
            if (ex.Message != null)
            {
                string errorMessage = ex.Message.Replace("\r", "").Replace("\n", "").Replace(',', '&');
                result += "Message: " + errorMessage;
            }
            if (ex.Source != null)
            {
                string errorSource = ex.Source;
                result += " && Source: " + errorSource;
            }
            if (ex.StackTrace != null)
            {
                var stackTrace = new StackTrace(ex, true);
                foreach (StackFrame stackFrame in stackTrace.GetFrames())
                {
                    string methodName = stackFrame.GetMethod().Name;
                    int lineNumber = stackFrame.GetFileLineNumber();
                    if (methodName != "" && lineNumber != 0)
                    {
                        result += " && Method: " + methodName + " line " + lineNumber;
                    }
                }
            }
            if (ex.TargetSite != null)
            {
                string targetSite = " && TargetSite: " + ex.TargetSite.ToString() + " - " + ex.TargetSite.DeclaringType.ToString();
                result += targetSite;
            }
            result = result.Replace("'", "");
            LoggingController.SaveHistory(
                string.Format("Thread Exception"),
                Lang.Error,
                string.Format(result),
                SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember"),
                LoggingType.Error);
        }

        #region USB Dongle key
        // Old key : 0xAB2A, 0x9718, 0xFF56, 0x2A25 
        // New key :  0x890F, 0x1A74, 0x9844, 0xC60A
        private static readonly ushort[] _newKey = { 0x890F, 0x1A74, 0x9844, 0xC60A };
        //private static readonly ushort[] _newKey1 = { 0xB4F4 , 0x50D0 , 0x352D , 0x412B };
        private static readonly ushort[] _oldKey = { 0xAB2A, 0x9718, 0xFF56, 0x2A25 };
       

        private static void InitVariableUSBDongle(ushort[] key)
        {
            _USBKey = new USBKey
            {
                USBPassword = key,
                InputValue = new ushort[] { 0x06, 0x02, 0x08, 0x15 }
            };
            _USBKey.ExpectedResult = CalculateValueWithFormulaDefined(_USBKey.InputValue[0], _USBKey.InputValue[1], _USBKey.InputValue[2], _USBKey.InputValue[3]);
        }

        private static ushort[] CalculateValueWithFormulaDefined(ushort ValueA, ushort ValueB, ushort ValueC, ushort ValueD)
        {
            ValueB = (ushort)(ValueB & ValueD);
            ValueA = (ushort)(ValueA + ValueB);
            ValueC = (ushort)(ValueC - ValueA);
            ValueD = (ushort)(ValueD | ValueC);
            return new ushort[] { ValueA, ValueB, ValueC, ValueD };
        }

        private static bool CheckForValidUSBDongleKey()
        {
            byte[] buffer = new byte[1024];
            uint hardwareID = 0;
            ushort handle = 0;
            uint lp1 = 0;
            uint lp2 = 0;
            ulong ret = 1;
            SecuredongleControl SD = new SecuredongleControl();
            ret = SD.SecureDongle((ushort)SDCmd.SD_FIND, ref handle, ref lp1, ref lp2,
                ref _USBKey.USBPassword[0], ref _USBKey.USBPassword[1], ref _USBKey.USBPassword[2], ref _USBKey.USBPassword[3], buffer);
            if (ret != 0)
            {
#if DEBUG
                Console.WriteLine("TrangNoi No SecureDongle found");
#endif
                return false;
            }
            hardwareID = lp1;
            if (_HardwareIDUsing == 0)
            {
                _HardwareIDUsing = hardwareID;
            }

            if (_HardwareIDUsing != hardwareID)
            {
#if DEBUG
                Console.WriteLine("_HardwareIDUsing changed");
#endif
                return false;
            }

            ret = SD.SecureDongle((ushort)SDCmd.SD_OPEN, ref handle, ref hardwareID, ref lp2,
                ref _USBKey.USBPassword[0], ref _USBKey.USBPassword[1], ref _USBKey.USBPassword[2], ref _USBKey.USBPassword[3], buffer);
            if (ret != 0)
            {
                return false;
            }
            lp1 = 0;
            lp2 = 0;
            ushort[] inputValue = new ushort[] { _USBKey.InputValue[0], _USBKey.InputValue[1], _USBKey.InputValue[2], _USBKey.InputValue[3] };
            ret = SD.SecureDongle((ushort)SDCmd.SD_CALCULATE1, ref handle, ref lp1, ref lp2,
                ref inputValue[0], ref inputValue[1], ref inputValue[2], ref inputValue[3], buffer);
            if (ret != 0)
            {
#if DEBUG
                Console.WriteLine("SD_CALCULATE1 fail");
#endif
                return false;
            }
            for (int i = 0; i < inputValue.Length; i++)
            {
                if (inputValue[i] != _USBKey.ExpectedResult[i])
                {
                    return false;
                }
            }
            ushort p1 = 500;  //Offset of UDZ (UDZ memory position)
            //[500] Rynan 0x54
            //[501] 0x01 Basler camera, 0x02 Cognex camera
            //[502] support level
            //[503] 
            ushort p2 = 4;
            ushort p3 = 0;
            ushort p4 = 0;
            ret = SD.SecureDongle((ushort)SDCmd.SD_READ, ref handle, ref hardwareID, ref lp2, ref p1, ref p2, ref p3, ref p4, buffer);
            if (ret != 0)
            {
                return false;
            }

            //Check OEM code
            if (buffer[0] != 0x54) //0x54 is label Rynan
            {
                return false;
            }

            // Check Cognex
            if (buffer[1] != 0x02)
            {
                return false;
            }

            //Close SecureDongle
            SD.SecureDongle((ushort)SDCmd.SD_CLOSE,
                ref handle,
                ref lp1,
                ref lp2,
                ref _USBKey.USBPassword[0],
                ref _USBKey.USBPassword[1],
                ref _USBKey.USBPassword[2],
                ref _USBKey.USBPassword[3],
                buffer);
            return true;
        }

        private static void CheckUSBDongleWhenRunning()
        {
            while (true)
            {
#if DEBUG
                Console.WriteLine("CheckUSBDongleWhenRunning");
#endif
                Thread.Sleep(6000);
                InitVariableUSBDongle(_newKey);
                if (CheckForValidUSBDongleKey() == false)
                {
                    InitVariableUSBDongle(_oldKey);
                    if (CheckForValidUSBDongleKey() == false)
                    {
                        ShowDialogUSBDongleKeyNotFound();
                        break;
                    }
                }
            }
        }

        private static void ShowDialogUSBDongleKeyNotFound()
        {
            if (frmWarningKey == null || frmWarningKey.IsDisposed)
            {
                frmWarningKey = new FrmWarningUSBDongleKey();
                frmWarningKey.Focus();
                frmWarningKey.BringToFront();
                frmWarningKey.TopMost = true;
                frmWarningKey.ShowDialog();
            }
            else
            {
                frmWarningKey.BringToFront();
            }
        }

        #endregion USB Dongle key

        #region Windows Update Check

        /// <summary>
        /// Checks Windows Update status and handles it appropriately (startup check only)
        /// </summary>
        private static void CheckWindowsUpdateStatus()
        {
            try
            {
                var updateService = new WindowsUpdateService();
                var status = updateService.CheckUpdateStatus();

                // Only try automatic handling at startup, don't show warnings
                // Warnings will be shown by the monitoring service when restart is imminent
                if (status.RequiresRestart || status.HasPendingUpdates || !status.UpdateServiceRunning)
                {
                    // Try to handle automatically but don't show warning
                    updateService.TryAutomaticHandling(status);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't block application startup
                Debug.WriteLine($"Error checking Windows Update: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts monitoring for imminent Windows restarts
        /// </summary>
        private static void StartWindowsUpdateMonitoring()
        {
            try
            {
                _windowsUpdateService = new WindowsUpdateService();
                _windowsUpdateService.ImminentRestartDetected += WindowsUpdateService_ImminentRestartDetected;
                _windowsUpdateService.StartMonitoring();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting Windows Update monitoring: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops monitoring for imminent Windows restarts
        /// </summary>
        private static void StopWindowsUpdateMonitoring()
        {
            try
            {
                if (_windowsUpdateService != null)
                {
                    _windowsUpdateService.ImminentRestartDetected -= WindowsUpdateService_ImminentRestartDetected;
                    _windowsUpdateService.StopMonitoring();
                    _windowsUpdateService = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping Windows Update monitoring: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles imminent restart detection event
        /// </summary>
        private static void WindowsUpdateService_ImminentRestartDetected(object sender, WindowsUpdateStatus status)
        {
            // Show warning on UI thread
            if (Application.OpenForms.Count > 0)
            {
                Application.OpenForms[0].Invoke(new Action(() =>
                {
                    ShowWindowsUpdateWarning(status);
                }));
            }
            else
            {
                // If no forms are open, show on main thread
                ShowWindowsUpdateWarning(status);
            }
        }

        /// <summary>
        /// Shows Windows Update warning dialog to user (only when restart is imminent)
        /// </summary>
        private static void ShowWindowsUpdateWarning(WindowsUpdateStatus status)
        {
            try
            {
                // Only show if form is not already displayed
                if (_windowsUpdateWarningForm == null || _windowsUpdateWarningForm.IsDisposed)
                {
                    _windowsUpdateWarningForm = new FrmWarningWindowsUpdate(status);
                    _windowsUpdateWarningForm.FormClosed += (s, e) => { _windowsUpdateWarningForm = null; };
                    _windowsUpdateWarningForm.ShowDialog();
                }
                else
                {
                    // Bring existing form to front
                    _windowsUpdateWarningForm.BringToFront();
                    _windowsUpdateWarningForm.Activate();
                }
            }
            catch (Exception ex)
            {
                // Fallback to simple message box if custom form fails
                MessageBox.Show(
                    $"⚠️ CRITICAL: Windows is about to force a restart!\n\n{status.StatusMessage}\n\nSave your work immediately!",
                    "Windows Restart Imminent",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        #endregion Windows Update Check
        #endregion
    }
}
