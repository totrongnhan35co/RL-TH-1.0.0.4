using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing
{
    public partial class RsalAlertForm : Form
    {
        private readonly HashSet<string> _shownCodes = new HashSet<string>();
        public bool IsWarning { get; private set; }

        public RsalAlertForm(bool isWarning)
        {
            IsWarning = isWarning;
            InitializeComponent();

            if (isWarning)
            {
                pnlTitle.BackColor = Color.FromArgb(255, 193, 7);
                picIcon.Image = Properties.Resources.icons8_warning_24px1;
                lblTitle.Text = "Cảnh báo máy in";
            }
            else
            {
                pnlTitle.BackColor = Color.FromArgb(220, 53, 69);
                picIcon.Image = Properties.Resources.icons8_X_result;
                lblTitle.Text = "Lỗi máy in";
            }
        }

        public void AppendMessage(string headIdx, string errorCode, string detail, string instruction)
        {
            if (_shownCodes.Contains(errorCode))
                return;
            _shownCodes.Add(errorCode);

            int blockWidth = pnlMessages.ClientSize.Width - 20;

            var block = new Panel
            {
                Width = blockWidth,
                Height = 100,
                Margin = new Padding(2, 4, 2, 2),
                BackColor = Color.FromArgb(248, 248, 248)
            };

            Color titleColor = IsWarning ? Color.FromArgb(180, 120, 10) : Color.FromArgb(190, 30, 30);
            block.Controls.Add(new Label
            {
                Text = $"POD {headIdx}: {detail}",
                Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
                ForeColor = titleColor,
                Location = new Point(6, 4),
                AutoSize = true,
                MaximumSize = new Size(blockWidth - 12, 0)
            });
            block.Controls.Add(new Label
            {
                Text = $"Mã: RSAL-{errorCode}",
                Font = new Font("Microsoft Sans Serif", 11F),
                ForeColor = Color.Gray,
                Location = new Point(6, 32),
                AutoSize = true,
                MaximumSize = new Size(blockWidth - 12, 0)
            });
            block.Controls.Add(new Label
            {
                Text = instruction,
                Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Italic),
                ForeColor = Color.DimGray,
                Location = new Point(6, 58),
                AutoSize = true,
                MaximumSize = new Size(blockWidth - 12, 0)
            });

            pnlMessages.Controls.Add(block);
            pnlMessages.ScrollControlIntoView(block);
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
