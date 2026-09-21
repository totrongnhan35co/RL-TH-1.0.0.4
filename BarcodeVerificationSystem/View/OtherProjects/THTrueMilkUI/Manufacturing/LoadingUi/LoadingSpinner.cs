using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing
{
    public class LoadingSpinner : Control
    {
        private readonly Timer _timer;
        private int _angle;
        private int _timerInterval = 28;

        public Color SpinnerColor { get; set; } =
            Color.FromArgb(0, 102, 204);

        public Color TrackColor { get; set; } =
            Color.FromArgb(218, 230, 242);

        public int LineWidth { get; set; } = 8;
        public int RotationStep { get; set; } = 9;

        public int TimerInterval
        {
            get { return _timerInterval; }
            set
            {
                _timerInterval = Math.Max(10, value);
                _timer.Interval = _timerInterval;
            }
        }

        public LoadingSpinner()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;

            _timer = new Timer
            {
                Interval = _timerInterval
            };

            _timer.Tick += (sender, e) =>
            {
                _angle =
                    (_angle + RotationStep) % 360;

                Invalidate();
            };
        }

        public void Start()
        {
            if (Visible)
                _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        protected override void OnVisibleChanged(
            EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!Visible)
                Stop();
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            int padding = LineWidth + 4;

            Rectangle rectangle =
                new Rectangle(
                    padding,
                    padding,
                    Math.Max(
                        1,
                        Width - padding * 2),
                    Math.Max(
                        1,
                        Height - padding * 2));

            using (Pen trackPen =
                new Pen(
                    TrackColor,
                    LineWidth))
            {
                trackPen.StartCap = LineCap.Round;
                trackPen.EndCap = LineCap.Round;

                e.Graphics.DrawArc(
                    trackPen,
                    rectangle,
                    0,
                    360);
            }

            using (Pen activePen =
                new Pen(
                    SpinnerColor,
                    LineWidth))
            {
                activePen.StartCap = LineCap.Round;
                activePen.EndCap = LineCap.Round;

                e.Graphics.DrawArc(
                    activePen,
                    rectangle,
                    _angle,
                    105);
            }
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
                _timer.Dispose();

            base.Dispose(disposing);
        }
    }
}
