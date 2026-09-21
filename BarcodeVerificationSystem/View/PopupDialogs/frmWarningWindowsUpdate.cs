using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.WindowsUpdate;
using OperationLog.Controller;
using OperationLog.Model;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmWarningWindowsUpdate : Form
    {
        private readonly WindowsUpdateStatus _updateStatus;
        private readonly Timer _TimerAutoClose;
        private bool _userAcknowledged = false;

        public FrmWarningWindowsUpdate(WindowsUpdateStatus updateStatus)
        {
            _updateStatus = updateStatus;
            _TimerAutoClose = new Timer { Interval = 300000 }; // 5 minutes
            InitializeComponent();
            InitControl();
            InitEvents();
            SetLanguage();
            UpdateIcon();
            UpdateStatusMessage();
        }

        #region Init

        private void InitControl()
        {
            _TimerAutoClose.Start();
            LoggingController.SaveHistory("Windows Update",
                    "Windows Update Warning",
                    $"Windows Update Status: {_updateStatus.StatusMessage}",
                    UserController.LogedInUsername ?? "System",
                    LoggingType.Warning);
        }

        private void UpdateStatusMessage()
        {
            string message = "⚠️ CRITICAL: Windows Update Restart Required\n\n";
            
            // This dialog only shows when restart is required and cannot be postponed
            if (_updateStatus.RequiresRestart)
            {
                message += "The system requires a restart to complete Windows Updates.\n";
                message += $"   {_updateStatus.PendingUpdateCount} update(s) are pending installation.\n\n";
                message += "⚠️ WARNING: The system may restart automatically at any time,\n";
                message += "which could interrupt your work and cause data loss.\n\n";
                message += "The software attempted to postpone the restart automatically,\n";
                message += "but was unable to do so (may require administrator privileges).\n\n";
                message += "RECOMMENDATION:\n";
                message += "1. Save all your work immediately\n";
                message += "2. Restart the system manually when convenient\n";
                message += "3. Or contact your system administrator to postpone the restart";
            }
            else
            {
                // Fallback message (shouldn't normally reach here)
                message += _updateStatus.StatusMessage;
            }

            lblUpdateStatus.Text = message;
        }

        #endregion Init

        #region Event
        private void InitEvents()
        {
            btnContinue.Click += BtnContinue_Click;
            btnOpenWindowsUpdate.Click += BtnOpenWindowsUpdate_Click;
            _TimerAutoClose.Tick += TimerAutoClose_Tick;
            FormClosing += FrmWarningWindowsUpdate_FormClosing;
            Shared.OnLanguageChange += ShareFunction_OnLanguageChange;
        }

        private void TimerAutoClose_Tick(object sender, EventArgs e)
        {
            if (!_userAcknowledged)
            {
                _userAcknowledged = true;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnContinue_Click(object sender, EventArgs e)
        {
            _userAcknowledged = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnOpenWindowsUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Open Windows Update settings
                Process.Start("ms-settings:windowsupdate");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open Windows Update settings: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmWarningWindowsUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            RemoveEventHandler();
        }

        private void RemoveEventHandler()
        {
            _TimerAutoClose.Stop();
            _TimerAutoClose.Dispose();
            Shared.OnLanguageChange -= ShareFunction_OnLanguageChange;
        }

        private void ShareFunction_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        #endregion Event

        #region Override
        public void SetLanguage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }

            Text = Lang.Warning ?? "Windows Update Warning";
            btnContinue.Text = "Continue";
            btnOpenWindowsUpdate.Text = "Open Windows Update";
        }

        public void UpdateIcon()
        {
            string path = Application.StartupPath + "\\Label\\icon.ico";
            if (File.Exists(path))
            {
                Icon = Icon.ExtractAssociatedIcon(path);
            }
            else
            {
                ShowIcon = false;
            }
        }
        #endregion Override
    }
}

