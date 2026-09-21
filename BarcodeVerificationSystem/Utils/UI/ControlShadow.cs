using System.Drawing;
using System.Drawing.Drawing2D;

namespace BarcodeVerificationSystem.Controller
{
    public class ControlShadow
    {
        public static void ApplyShadow(System.Windows.Forms.Control ctrl, Color pColor, int blur)
        {
            ctrl.BackColor = (ctrl.Parent).BackColor;
            ctrl.Paint += (sender, eventArgs) =>
            {
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(blur, 0), Color.Transparent, pColor);
                eventArgs.Graphics.FillRectangle(brush, 0, blur, blur, ctrl.Height - blur * 2);
                brush.RotateTransform(90);
                eventArgs.Graphics.FillRectangle(brush, blur, 0, ctrl.Width - blur * 2, blur);
                brush.ResetTransform();
                brush.TranslateTransform(ctrl.Width % blur, ctrl.Height % blur);
                brush.RotateTransform(180);
                eventArgs.Graphics.FillRectangle(brush, ctrl.Width - blur, blur, blur, ctrl.Height - blur * 2);
                brush.RotateTransform(90);
                eventArgs.Graphics.FillRectangle(brush, blur, ctrl.Height - blur, ctrl.Width - blur * 2, blur);
                var gp = new GraphicsPath();
                gp.AddEllipse(0, 0, blur  *2, blur  *2);
                var pgb = new PathGradientBrush(gp)
                {
                    CenterColor = pColor,
                    SurroundColors = new[] { Color.Transparent },
                    CenterPoint = new Point(blur, blur)
                };

                int w = ctrl.Width;
                int h = ctrl.Height;

                eventArgs.Graphics.FillPie(pgb, 0, 0, blur  *2, blur  *2, 180, 90);
                var matrix = new Matrix();
                matrix.Translate(w - blur * 2, 0);
                pgb.Transform = matrix;

                eventArgs.Graphics.FillPie(pgb, w - blur  *2, 0, blur  *2, blur * 2, 270, 90);
                matrix.Translate(0, h - blur * 2);
                pgb.Transform = matrix;
                eventArgs.Graphics.FillPie(pgb, w - blur  *2, h - blur  *2, blur  *2, blur  *2, 0, 90);
                matrix.Reset();
                matrix.Translate(0, h - blur * 2);
                pgb.Transform = matrix;
                eventArgs.Graphics.FillPie(pgb, 0, h - blur  *2, blur  *2, blur * 2, 90, 90);
            };
            ctrl.Resize += (sender, eventArgs) =>
            {
                ctrl.Invalidate();
            };
        }
    }
}
