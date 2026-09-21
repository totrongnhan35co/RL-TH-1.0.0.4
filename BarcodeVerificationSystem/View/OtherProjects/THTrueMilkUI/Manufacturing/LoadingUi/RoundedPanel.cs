using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing
{
    public class RoundedPanel : Panel
    {
        private int _borderRadius = 12;
        private int _borderSize = 1;

        private Color _borderColor =
            Color.LightBlue;

        public int BorderRadius
        {
            get { return _borderRadius; }
            set
            {
                _borderRadius =
                    Math.Max(2, value);

                UpdateRegion();
                Invalidate();
            }
        }

        public int BorderSize
        {
            get { return _borderSize; }
            set
            {
                _borderSize =
                    Math.Max(1, value);

                Invalidate();
            }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        public RoundedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnResize(
            EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            Rectangle rectangle =
                new Rectangle(
                    0,
                    0,
                    Width - 1,
                    Height - 1);

            using (GraphicsPath path =
                CreatePath(
                    rectangle,
                    _borderRadius))
            using (Pen pen =
                new Pen(
                    _borderColor,
                    _borderSize))
            {
                e.Graphics.DrawPath(
                    pen,
                    path);
            }
        }

        private void UpdateRegion()
        {
            if (Width <= 0 ||
                Height <= 0)
            {
                return;
            }

            using (GraphicsPath path =
                CreatePath(
                    ClientRectangle,
                    _borderRadius))
            {
                Region = new Region(path);
            }
        }

        private static GraphicsPath CreatePath(
            Rectangle rectangle,
            int radius)
        {
            GraphicsPath path =
                new GraphicsPath();

            int diameter =
                Math.Max(2, radius * 2);

            Rectangle arc =
                new Rectangle(
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
