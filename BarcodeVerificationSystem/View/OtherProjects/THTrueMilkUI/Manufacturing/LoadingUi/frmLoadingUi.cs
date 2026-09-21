using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing
{
    public partial class frmLoadingUi : Form
    {
        private Form _ownerForm;
        private bool _allowClose;

        private int _borderRadius = 2;
        //private Color _primaryColor = Color.FromArgb(0, 102, 204);
        private Color _primaryColor = Color.FromArgb(0, 160, 204);

        public frmLoadingUi()
            : this("ĐANG XỬ LÝ", "Vui lòng chờ trong giây lát...")
        {
        }

        public frmLoadingUi(
            string title,
            string message,
            bool showTitle = true,
            bool showMessage = true,
            bool showSpinner = true)
        {
            InitializeComponent();

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            TitleText = title;
            MessageText = message;

            ShowTitle = showTitle;
            ShowMessage = showMessage;
            ShowSpinner = showSpinner;

            ApplyPrimaryColor(_primaryColor);
            LayoutLoadingControls();
            ApplyRoundedRegion();
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;

                CreateParams parameters = base.CreateParams;
                parameters.ClassStyle |= CS_DROPSHADOW;

                return parameters;
            }
        }

        #region Public properties

        public string TitleText
        {
            get { return lblTitle.Text; }
            set
            {
                lblTitle.Text = string.IsNullOrWhiteSpace(value)
                    ? "ĐANG XỬ LÝ"
                    : value;
            }
        }

        public string MessageText
        {
            get { return lblMessage.Text; }
            set { lblMessage.Text = value ?? string.Empty; }
        }

        public bool ShowTitle
        {
            get { return pnlHeader.Visible; }
            set
            {
                pnlHeader.Visible = value;
                LayoutLoadingControls();
            }
        }

        public bool ShowMessage
        {
            get { return pnlMessageBox.Visible; }
            set
            {
                pnlMessageBox.Visible = value;
                LayoutLoadingControls();
            }
        }

        public bool ShowSpinner
        {
            get { return loadingSpinner.Visible; }
            set
            {
                loadingSpinner.Visible = value;

                if (value && Visible)
                    loadingSpinner.Start();
                else
                    loadingSpinner.Stop();

                LayoutLoadingControls();
            }
        }

        public Color PrimaryColor
        {
            get { return _primaryColor; }
            set
            {
                _primaryColor = value;
                ApplyPrimaryColor(value);
            }
        }

        public Color SpinnerColor
        {
            get { return loadingSpinner.SpinnerColor; }
            set
            {
                loadingSpinner.SpinnerColor = value;
                loadingSpinner.Invalidate();
            }
        }

        public Color SpinnerTrackColor
        {
            get { return loadingSpinner.TrackColor; }
            set
            {
                loadingSpinner.TrackColor = value;
                loadingSpinner.Invalidate();
            }
        }

        public int SpinnerSize
        {
            get { return loadingSpinner.Width; }
            set
            {
                int size = Math.Max(36, value);
                loadingSpinner.Size = new Size(size, size);
                LayoutLoadingControls();
            }
        }

        public int SpinnerLineWidth
        {
            get { return loadingSpinner.LineWidth; }
            set
            {
                loadingSpinner.LineWidth = Math.Max(2, value);
                loadingSpinner.Invalidate();
            }
        }

        public int SpinnerSpeed
        {
            get { return loadingSpinner.TimerInterval; }
            set
            {
                loadingSpinner.TimerInterval =
                    Math.Max(10, value);
            }
        }

        #endregion

        #region Public methods

        public void UpdateTitle(string title)
        {
            RunOnUi(() => TitleText = title);
        }

        public void UpdateMessage(string message)
        {
            RunOnUi(() => MessageText = message);
        }

        public void SetTitleVisible(bool visible)
        {
            RunOnUi(() => ShowTitle = visible);
        }

        public void SetMessageVisible(bool visible)
        {
            RunOnUi(() => ShowMessage = visible);
        }

        public void SetSpinnerVisible(bool visible)
        {
            RunOnUi(() => ShowSpinner = visible);
        }

        public void SetPrimaryColor(Color color)
        {
            RunOnUi(() => PrimaryColor = color);
        }

        public void SetSpinnerStyle(
            Color color,
            int size = 86,
            int lineWidth = 8,
            int speed = 28)
        {
            RunOnUi(() =>
            {
                SpinnerColor = color;
                SpinnerSize = size;
                SpinnerLineWidth = lineWidth;
                SpinnerSpeed = speed;
            });
        }

        public void AttachOwner(Form owner)
        {
            DetachOwner();

            _ownerForm = owner;

            if (_ownerForm == null ||
                _ownerForm.IsDisposed)
            {
                return;
            }

            _ownerForm.LocationChanged += OwnerBoundsChanged;
            _ownerForm.SizeChanged += OwnerBoundsChanged;
            _ownerForm.VisibleChanged += OwnerBoundsChanged;
            _ownerForm.FormClosed += OwnerFormClosed;

            CenterOverOwner();
        }

        public static frmLoadingUi ShowLoading(
            Form owner,
            string title = "ĐANG XỬ LÝ",
            string message = "Vui lòng chờ trong giây lát...",
            bool showTitle = true,
            bool showMessage = true,
            bool showSpinner = true)
        {
            if (owner != null && owner.InvokeRequired)
            {
                return (frmLoadingUi)owner.Invoke(
                    new Func<frmLoadingUi>(() =>
                        ShowLoading(
                            owner,
                            title,
                            message,
                            showTitle,
                            showMessage,
                            showSpinner)));
            }

            frmLoadingUi loading = new frmLoadingUi(
                title,
                message,
                showTitle,
                showMessage,
                showSpinner);

            loading.AttachOwner(owner);

            if (owner != null && !owner.IsDisposed)
                loading.Show(owner);
            else
                loading.Show();

            loading.CenterOverOwner();
            loading.TopMost = true;
            loading.BringToFront();
            loading.Refresh();

            return loading;
        }

        public static void CloseLoading(
            ref frmLoadingUi loading)
        {
            if (loading == null)
                return;

            frmLoadingUi current = loading;
            loading = null;

            current.RequestClose();
        }

        public static async Task RunAsync(
            Form owner,
            Func<Task> action,
            string title = "ĐANG XỬ LÝ",
            string message = "Vui lòng chờ trong giây lát...",
            bool showTitle = true,
            bool showMessage = true,
            bool showSpinner = true)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            frmLoadingUi loading = null;

            try
            {
                loading = ShowLoading(
                    owner,
                    title,
                    message,
                    showTitle,
                    showMessage,
                    showSpinner);

                await Task.Yield();
                await action();
            }
            finally
            {
                CloseLoading(ref loading);
            }
        }

        #endregion

        private void ApplyPrimaryColor(Color color)
        {
            BackColor = Darken(color, 35);
            pnlHeader.BackColor = color;
            pnlTopAccent.BackColor = Lighten(color, 30);

            pnlMessageBox.BorderColor =
                Lighten(color, 125);

            pnlMessageBox.BackColor =
                Lighten(color, 235);

            lblMessage.ForeColor =
                Darken(color, 55);

            loadingSpinner.SpinnerColor = color;
            loadingSpinner.Invalidate();

            Invalidate();
        }

        private void RunOnUi(Action action)
        {
            if (action == null || IsDisposed)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(action);
                return;
            }

            action();
        }

        private void RequestClose()
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RequestClose));
                return;
            }

            _allowClose = true;
            fadeTimer.Stop();
            Close();
            Dispose();
        }

        private void frmLoadingUi_Shown(
            object sender,
            EventArgs e)
        {
            Opacity = 0.15;

            if (ShowSpinner)
                loadingSpinner.Start();

            CenterOverOwner();

            TopMost = true;
            BringToFront();

            fadeTimer.Start();
        }

        private void fadeTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (Opacity >= 1D)
            {
                Opacity = 1D;
                fadeTimer.Stop();
                return;
            }

            Opacity = Math.Min(
                1D,
                Opacity + 0.10D);
        }

        private void frmLoadingUi_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            fadeTimer.Stop();
            loadingSpinner.Stop();
            DetachOwner();
        }

        private void frmLoadingUi_Resize(
            object sender,
            EventArgs e)
        {
            LayoutLoadingControls();
            ApplyRoundedRegion();
        }

        private void frmLoadingUi_LocationChanged(
            object sender,
            EventArgs e)
        {
            KeepInsideScreen();
        }

        private void OwnerBoundsChanged(
            object sender,
            EventArgs e)
        {
            CenterOverOwner();
        }

        private void OwnerFormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            RequestClose();
        }

        private void DetachOwner()
        {
            if (_ownerForm == null)
                return;

            _ownerForm.LocationChanged -= OwnerBoundsChanged;
            _ownerForm.SizeChanged -= OwnerBoundsChanged;
            _ownerForm.VisibleChanged -= OwnerBoundsChanged;
            _ownerForm.FormClosed -= OwnerFormClosed;

            _ownerForm = null;
        }

        private void CenterOverOwner()
        {
            if (IsDisposed)
                return;

            Rectangle targetBounds;

            if (_ownerForm != null &&
                !_ownerForm.IsDisposed &&
                _ownerForm.Visible)
            {
                targetBounds = _ownerForm.Bounds;
            }
            else
            {
                targetBounds =
                    Screen.FromPoint(
                        Cursor.Position).WorkingArea;
            }

            Location = new Point(
                targetBounds.Left +
                    (targetBounds.Width - Width) / 2,
                targetBounds.Top +
                    (targetBounds.Height - Height) / 2);

            KeepInsideScreen();
        }

        private void KeepInsideScreen()
        {
            if (IsDisposed ||
                Width <= 0 ||
                Height <= 0)
            {
                return;
            }

            Rectangle area =
                Screen.FromRectangle(Bounds).WorkingArea;

            int x = Math.Max(
                area.Left,
                Math.Min(
                    Left,
                    area.Right - Width));

            int y = Math.Max(
                area.Top,
                Math.Min(
                    Top,
                    area.Bottom - Height));

            if (x != Left || y != Top)
                Location = new Point(x, y);
        }

        protected override void OnFormClosing(
            FormClosingEventArgs e)
        {
            if (!_allowClose &&
                e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                return;
            }

            base.OnFormClosing(e);
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            Rectangle rectangle = new Rectangle(
                1,
                1,
                Width - 3,
                Height - 3);

            using (GraphicsPath path =
                CreateRoundedPath(
                    rectangle,
                    _borderRadius))
            using (Pen outerPen =
                new Pen(
                    Darken(_primaryColor, 45),
                    3))
            using (Pen innerPen =
                new Pen(
                    Color.FromArgb(230, 240, 250),
                    1))
            {
                e.Graphics.DrawPath(
                    outerPen,
                    path);

                rectangle.Inflate(-3, -3);

                using (GraphicsPath innerPath =
                    CreateRoundedPath(
                        rectangle,
                        Math.Max(
                            4,
                            _borderRadius - 3)))
                {
                    e.Graphics.DrawPath(
                        innerPen,
                        innerPath);
                }
            }
        }

        private void LayoutLoadingControls()
        {
            if (loadingSpinner == null ||
                pnlBody == null ||
                pnlMessageBox == null)
            {
                return;
            }

            const int sideMargin = 26;
            const int messageHeight = 64;
            const int bottomMargin = 20;

            int bodyWidth =
                pnlBody.ClientSize.Width;

            int bodyHeight =
                pnlBody.ClientSize.Height;

            int messageTop;

            if (pnlMessageBox.Visible)
            {
                pnlMessageBox.SetBounds(
                    sideMargin,
                    bodyHeight -
                        messageHeight -
                        bottomMargin,
                    Math.Max(
                        120,
                        bodyWidth -
                            sideMargin * 2),
                    messageHeight);

                messageTop =
                    pnlMessageBox.Top;
            }
            else
            {
                messageTop = bodyHeight;
            }

            if (loadingSpinner.Visible)
            {
                int spinnerAreaBottom =
                    pnlMessageBox.Visible
                        ? messageTop - 10
                        : bodyHeight;

                loadingSpinner.Left =
                    (bodyWidth -
                     loadingSpinner.Width) / 2;

                loadingSpinner.Top =
                    Math.Max(
                        14,
                        (spinnerAreaBottom -
                         loadingSpinner.Height) / 2);
            }

            if (!loadingSpinner.Visible &&
                pnlMessageBox.Visible)
            {
                pnlMessageBox.Top =
                    Math.Max(
                        16,
                        (bodyHeight -
                         pnlMessageBox.Height) / 2);
            }
        }

        private void ApplyRoundedRegion()
        {
            if (Width <= 0 || Height <= 0)
                return;

            using (GraphicsPath path =
                CreateRoundedPath(
                    new Rectangle(
                        0,
                        0,
                        Width,
                        Height),
                    _borderRadius))
            {
                Region = new Region(path);
            }

            Invalidate();
        }

        private static Color Lighten(
            Color color,
            int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount));
        }

        private static Color Darken(
            Color color,
            int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, color.R - amount),
                Math.Max(0, color.G - amount),
                Math.Max(0, color.B - amount));
        }

        private static GraphicsPath CreateRoundedPath(
            Rectangle rectangle,
            int radius)
        {
            GraphicsPath path =
                new GraphicsPath();

            int diameter =
                Math.Max(2, radius * 2);

            Rectangle arc = new Rectangle(
                rectangle.X,
                rectangle.Y,
                diameter,
                diameter);

            path.AddArc(arc, 180, 90);

            arc.X =
                rectangle.Right - diameter;

            path.AddArc(arc, 270, 90);

            arc.Y =
                rectangle.Bottom - diameter;

            path.AddArc(arc, 0, 90);

            arc.X = rectangle.Left;

            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
