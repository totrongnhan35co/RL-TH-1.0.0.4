using BarcodeVerificationSystem.Controller;
using OperationLog.Controller;
using OperationLog.Model;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmWarningUSBDongleKey : Form
    {
        private readonly Timer _TimerCloseApp = new Timer();
        public FrmWarningUSBDongleKey()
        {
            InitializeComponent();
            InitControl();
            InitEvents();
            SetLanguage();
            UpdateIcon();
        }

        #region Init

        private void InitControl()
        {
            _TimerCloseApp.Interval = 60 * 1000;
            _TimerCloseApp.Start(); 
            LoggingController.SaveHistory("USB key",
                    "USB key",
                    "USB key invalid or unplugged!",
                    UserController.LogedInUsername,
                    LoggingType.Error);
        }

        #endregion Init

        #region Event
        private void InitEvents()
        {
            btnClose.Click += BtnClose_Click;
            _TimerCloseApp.Tick += TimerCloseApp_Tick;
            FormClosing += FrmWarningUSBDongleKey_FormClosing;
            Shared.OnLanguageChange += ShareFunction_OnLanguageChange;
        }

        private void TimerCloseApp_Tick(object sender, EventArgs e)
        {
            CloseApplication();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            CloseApplication();
        }

        private void FrmWarningUSBDongleKey_FormClosing(object sender, FormClosingEventArgs e)
        {
            RemoveEventHandler();
            CloseApplication();
        }

        private void RemoveEventHandler()
        {
            Shared.OnLanguageChange -= ShareFunction_OnLanguageChange;
        }

        private void ShareFunction_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        #endregion Event

        #region Close application

        private void CloseApplication()
        {
            Environment.Exit(0); 
        }

        #endregion Close application

        #region Override
        public void SetLanguage()
       
        {
          
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }

            Text = Lang.Warning;
            btnClose.Text = Lang.Exit;
            lblCheckUSBKey.Text = Lang.PleaseCheckTheUSBKey;

        }

        public void UpdateIcon()
        {
            string path = Application.StartupPath + "\\Label\\icon.ico"; // logores.ico
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
