using System.Drawing;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Controller
{
    public class ComboBoxCustom
    {
        public static void Cbo_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 35;
        }

        public static void MyComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            var box = (ComboBox)sender;
            if (box is null)
                return;
            e.DrawBackground();
            if (e.Index >= 0)
            {
                Graphics g = e.Graphics;
                using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                using (Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                                      ? new SolidBrush(SystemColors.Highlight)
                                      : new SolidBrush(e.BackColor))
                {
                    using (Brush textBrush = new SolidBrush(e.ForeColor))
                    {
                        g.FillRectangle(brush, e.Bounds);
                        g.DrawString(box.Items[e.Index].ToString(),
                                     e.Font,
                                     textBrush,
                                     e.Bounds,
                                     sf);
                    }
                }
            }
            e.DrawFocusRectangle();
        }
    }
}
