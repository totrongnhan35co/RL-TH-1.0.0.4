using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Controller
{
 
    public class TouchScroll
    {
        private Point mouseDownPoint;
        private readonly Panel parentPanel;
       
        public TouchScroll(Panel panel)
        {
            parentPanel = panel;
            AssignEvents(panel);
        }

        private void AssignEvents(Control control)
        {
            control.MouseDown += MouseDown;
            control.MouseMove += MouseMove;
            foreach (Control child in control.Controls)
                AssignEvents(child);
        }

        private void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                mouseDownPoint = Cursor.Position;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            Point pointDifference = new Point(Cursor.Position.X - mouseDownPoint.X, Cursor.Position.Y - mouseDownPoint.Y);

            if ((mouseDownPoint.X == Cursor.Position.X) && (mouseDownPoint.Y == Cursor.Position.Y))
                return;

            Point currAutoS = parentPanel.AutoScrollPosition;
            parentPanel.AutoScrollPosition = new Point(Math.Abs(currAutoS.X) - pointDifference.X, Math.Abs(currAutoS.Y) - pointDifference.Y);
            mouseDownPoint = Cursor.Position;
        }
    }
}