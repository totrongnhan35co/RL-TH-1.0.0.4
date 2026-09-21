using System.Drawing;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Controller
{
    public class DrawListBox
    {
        public static void ListBoxJobNameList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }
            e.DrawBackground();

            int a = e.Bounds.Height * 16 / 100;
            int b = e.Bounds.Height * 8 / 100 + 1;
            Brush myBrush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ?
                  Brushes.White : new SolidBrush(SystemColors.WindowFrame);

            Rectangle fullItemRect = new Rectangle(10, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            Rectangle headItemRect = new Rectangle(0, e.Bounds.Y + 5, 8, e.Bounds.Height - 10);

            using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
            {
                if ((sender as ListBox).Enabled)
                {
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(210, 232, 255)), e.Bounds);
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 170, 230)), headItemRect);
                        e.Graphics.DrawString((sender as ListBox).Items[e.Index].ToString(),
                            e.Font, Brushes.Black, fullItemRect, sf);
                        e.DrawFocusRectangle();
                        return;
                    }
                    else
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 170, 230)), headItemRect);
                        e.Graphics.DrawString((sender as ListBox).Items[e.Index].ToString(),
                            e.Font, myBrush, fullItemRect, sf);
                    }
                }
                else
                {
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), e.Bounds);
                        e.Graphics.FillRectangle(new SolidBrush(Color.DarkGray), headItemRect);
                        e.Graphics.DrawString((sender as ListBox).Items[e.Index].ToString(),
                            e.Font, Brushes.Black, fullItemRect, sf);
                        e.DrawFocusRectangle();
                    }
                    else
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), headItemRect);
                        e.Graphics.DrawString((sender as ListBox).Items[e.Index].ToString(), e.Font, myBrush, fullItemRect, sf);
                        if (e.Index < (sender as ListBox).Items.Count - 1)
                        {
                            e.Graphics.DrawRectangle(Pens.Gainsboro, fullItemRect);
                        }
                    }
                }
            }
        }

        private static readonly Color _Standalone = Color.DarkBlue;
        private static readonly Color _RLinkColor = Color.FromArgb(0, 171, 230);

        public static void ListBoxColor_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }
            try
            {
                e.DrawBackground();
                Rectangle fullItemRect = new Rectangle(10, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                Rectangle headItemRect = new Rectangle(0, e.Bounds.Y + 5, 8, e.Bounds.Height - 10);
                Model.JobModel job = Shared.GetJob((sender as ListBox).Items[e.Index].ToString());
                using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                using (var headItemBrush = new SolidBrush(FindColor(job)))
                using (Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : new SolidBrush(SystemColors.WindowFrame))
                {
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(210, 232, 255)), e.Bounds);
                        e.Graphics.FillRectangle(headItemBrush, headItemRect);
                        e.Graphics.DrawString((sender as ListBox).Items[e.Index].ToString(),
                            e.Font, Brushes.Black, fullItemRect, sf);
                        e.DrawFocusRectangle();
                        return;
                    }
                    else
                    {
                        e.Graphics.FillRectangle(headItemBrush, headItemRect);
                        e.Graphics.DrawString((sender as ListBox).Items[e.Index].ToString(),
                            e.Font, brush, fullItemRect, sf);
                    }
                }
                job = null;
            }
            catch
            {

            }
        }

        private static Color FindColor(Model.JobModel job)
        {
            Color cl = job.JobType == Model.JobType.StandAlone ? _Standalone : _RLinkColor;
            return cl;
        }
    }
}
