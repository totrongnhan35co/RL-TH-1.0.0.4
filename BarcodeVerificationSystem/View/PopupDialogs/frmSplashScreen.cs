using BarcodeVerificationSystem.Labels.ProjectLabel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmSplashScreen : Form
    {
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
        public FrmSplashScreen()
        {
            InitializeComponent();
            InitControl();
        }

        private void InitControl()
        {
            if(ProjectLabel.IsDefault)
            {
                lblComment.BackColor = System.Drawing.Color.FromArgb(0, 170, 230);
            }
            else if (ProjectLabel.IsNutrifood || ProjectLabel.IsCaoSuDongNai)
            {
                lblComment.BackColor = System.Drawing.Color.FromArgb(8, 109, 70);
            }
            else
            {
                lblComment.BackColor = System.Drawing.Color.FromArgb(0, 170, 230);
            }

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (Pen borderPen = new Pen(Color.FromArgb(80, 80, 80), 1))
            {
                Rectangle rect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
                e.Graphics.DrawRectangle(borderPen, rect);
            }
        }

        private delegate void CloseDelegate();
        private static FrmSplashScreen _splashForm;
        private static Thread _splashThread;
        private static bool IsWaitOne = false;

        public static void ShowSplashScreen()
        {
            IsWaitOne = true;
            if (_splashThread == null)
            {
                _splashThread = new Thread(new ThreadStart(DoShowSplash))
                {
                    IsBackground = true
                };
                _splashThread.Start();
            }
        }

        public static void ShowSplashScreen(string message, string comment)
        {
            IsWaitOne = true;
            if (_splashThread == null)
            {
                _splashThread = new Thread(new ThreadStart(() => DoShowSplash(message, comment)))
                {
                    IsBackground = true
                };
                _splashThread.Start();
            }
        }

        private static void DoShowSplash()
        {
            if (_splashForm == null)
            {
                _splashForm = new FrmSplashScreen
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    TopMost = true
                };
            }

            IsWaitOne = false;
            Application.Run(_splashForm);
        }

        private static void DoShowSplash(string message, string comment)
        {
            if (_splashForm == null)
            {
                _splashForm = new FrmSplashScreen();
                _splashForm.lblLoading.Text = message;
                _splashForm.lblComment.Text = comment;
                _splashForm.StartPosition = FormStartPosition.CenterScreen;
                _splashForm.TopMost = true;
            }
            IsWaitOne = false;      
            Application.Run(_splashForm);
        }
        
        public static void CloseSplash()
        {
            int i = 0;
            while (IsWaitOne && i < 20)
            {
                Thread.Sleep(100);
                i++;
            }

            if (_splashForm == null)
            {
                return;
            }

            if (_splashForm.InvokeRequired)
            {
                _splashForm.Invoke(new MethodInvoker(CloseSplash));
            }
            else
            {
                _splashThread = null;
                _splashForm = null;
                Application.ExitThread();
            }
        }
    }
}
