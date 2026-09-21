using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing
{
    public partial class frmNotificationJob : Form
    {
        public frmNotificationJob()
        {
            InitializeComponent();
            ConfigureControlState();
            WindowState = FormWindowState.Maximized;
        }

        public frmNotificationJob(string confirmText) : this()
        {
            SetContent(confirmText);
        }

        public frmNotificationJob(string title, string confirmText) : this(confirmText)
        {
            SetTitle(title);
        }

        private void ConfigureControlState()
        {
            btnConfirm.Click += BtnConfirm_Click;
            btnCancel.Click += BtnCancel_Click;

            this.AcceptButton = btnConfirm;
            this.CancelButton = btnCancel;
        }

        public void SetTitle(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                lblTitle.Text = title;
            }
        }

        public void SetContent(string confirmText)
        {
            rtbContent.Text = confirmText ?? string.Empty;
        }

        public void SetContentWithGtinHighlight(string fullText, string gtinValue, string[] boldValues)
        {
            rtbContent.Clear();
            rtbContent.AppendText(fullText ?? string.Empty);

            if (string.IsNullOrEmpty(fullText)) return;

            var boldFont = new Font(rtbContent.Font, FontStyle.Bold);

            // (A) In đậm các giá trị bổ sung (vd: QR info)
            if (boldValues != null)
            {
                foreach (var val in boldValues)
                {
                    if (string.IsNullOrEmpty(val)) continue;
                    int idx = fullText.IndexOf(val, StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        rtbContent.Select(idx, val.Length);
                        rtbContent.SelectionFont = boldFont;
                    }
                }
            }

            // (B) In đậm toàn bộ GTIN
            if (!string.IsNullOrEmpty(gtinValue))
            {
                int idxGtin = fullText.IndexOf(gtinValue, StringComparison.Ordinal);
                if (idxGtin >= 0)
                {
                    rtbContent.Select(idxGtin, gtinValue.Length);
                    rtbContent.SelectionFont = boldFont;
                }

                // (C) 5 số cuối GTIN → đỏ + đậm
                int last5Length = Math.Min(5, gtinValue.Length);
                string last5 = gtinValue.Substring(gtinValue.Length - last5Length);
                int idx5 = fullText.LastIndexOf(last5, StringComparison.Ordinal);
                if (idx5 >= 0)
                {
                    rtbContent.Select(idx5, last5Length);
                    rtbContent.SelectionFont = boldFont;
                    rtbContent.SelectionColor = Color.Red;
                }
            }

            rtbContent.Select(0, 0);
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        public static bool ShowConfirm(string confirmText)
        {
            using (var frm = new frmNotificationJob(confirmText))
            {
                return frm.ShowDialog() == DialogResult.Yes;
            }
        }

        public static bool ShowConfirm(string title, string confirmText)
        {
            using (var frm = new frmNotificationJob(title, confirmText))
            {
                return frm.ShowDialog() == DialogResult.Yes;
            }
        }

        public static bool ShowConfirmWithGtinHighlight(string fullText, string gtinValue, string[] boldValues = null)
        {
            using (var frm = new frmNotificationJob())
            {
                frm.SetContentWithGtinHighlight(fullText, gtinValue, boldValues);
                return frm.ShowDialog() == DialogResult.Yes;
            }
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }
    }
}
