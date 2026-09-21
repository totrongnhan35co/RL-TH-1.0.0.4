using BarcodeVerificationSystem.Controller;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmAbout : Form
    
    {
        public FrmAbout()
        {
            InitializeComponent();
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitControls();
            SetLanguage();
            Shared.OnLanguageChange += Shared_OnLanguageChange;
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
            lblFormName.Text = Lang.About;
            grbGeneralInfo.Text = Lang.GeneralInfo;
            grbReleaseNote.Text = Lang.ReleaseNote;
        }
        private void InitControls()
        {
            try
            {
                
                if (File.Exists(Application.StartupPath + "\\Labels\\RynanSingapore\\about.txt")) //load info file
                {
                    using (FileStream fs = File.Open(Application.StartupPath + "\\Labels\\RynanSingapore\\about.txt", FileMode.Open,FileAccess.Read))
                    {
                        using (var rd = new StreamReader(fs))
                        {
                            string about = rd.ReadToEnd();
                            about = about.Replace("Soft_Version",Properties.Settings.Default.SoftwareVersion);
                            webBrowser1.DocumentText = about;
                        }
                    }
                }
                string text = File.ReadAllText(Application.StartupPath + "\\Labels\\Readme.txt", Encoding.UTF8); // load readme file
                rchReleaseNote.Text = text;

            }
            catch (Exception)
            {
            }
        }

    }
}
