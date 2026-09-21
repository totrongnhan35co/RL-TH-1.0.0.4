using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.UtilityForms.THTrueMilk
{
    public partial class frmPrinterMonitorCustom : Form
    {
        public string IPAddress = "";
        public int Port = 1001;
        public frmPrinterMonitorCustom()
        {
            InitializeComponent();
        }
        public frmPrinterMonitorCustom(string ip, int port = 1001)
        {
            InitializeComponent();
            IPAddress = ip;
            Port = port;
            this.Size = new Size(1200,900);
            this.FormClosing += (s, e) => { try { webView21?.Dispose(); } catch { } };
        }
       

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            string url = $"{IPAddress}:{Port}";

            InitializeBrowser(url);

            reloadToolStripMenuItem.Click += (sender, eventArgs) =>
            {
                webView21.Reload();
            };

            exitToolStripMenuItem.Click += (sender, eventArgs) =>
            {
                try { webView21?.Dispose(); } catch { }
                Close();
            };

            btnMenu.MouseDown += (sender, eventArgs) => {
                cuzDropdownMenu.PrimaryColor = Color.FromArgb(0, 171, 230);
                cuzDropdownMenu.MenuItemHeight = 40;
                cuzDropdownMenu.Font = new Font("Microsoft Sans Serif", 12);
                cuzDropdownMenu.ForeColor = Color.Black;
                cuzDropdownMenu.Show(btnMenu, btnMenu.Width + 5, -17);
            };

            btnMenu.LostFocus += (s, ev) =>
            {
                cuzDropdownMenu.Close();
            };
        }

        private async void InitializeBrowser(string url = null)
        {
            try
            {

             string userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BarcodeVerificationSystems\\PrinterMonitor";
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                if (IsDisposed || !IsHandleCreated) return;
                await webView21.EnsureCoreWebView2Async(env);
                if (IsDisposed || !IsHandleCreated) return;
                webView21.Source = new UriBuilder(url).Uri;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tại Form Custom: " + ex.Message);
            }
          
        }
    }
}
