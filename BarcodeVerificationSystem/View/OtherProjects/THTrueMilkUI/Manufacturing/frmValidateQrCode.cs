using BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing
{
    public partial class frmValidateQrCode : Form
    {
        // Khai báo thêm 2 Label để quản lý dấu hai chấm ":"
        private Label lblColon1;
        private Label lblColon2;

        public bool SkipConfirmStep { get; set; } = false;
        public bool AutoConfirmCheckboxes { get; set; } = false;

        public frmValidateQrCode()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            MinimumSize = new Size(1024, 648);

            pnlProductCard.Anchor = AnchorStyles.None;
            pnlQrCard.Anchor = AnchorStyles.None;
            pnlLinkCard.Anchor = AnchorStyles.None;
            pnlCheckConfirm.Anchor = AnchorStyles.None;

            InitCustomUI();
            InitState();
            ApplyResponsiveLayout();
        }

        private void InitCustomUI()
        {
            pnlProductCard.BorderStyle = BorderStyle.None;
            pnlQrCard.BorderStyle = BorderStyle.None;
            pnlLinkCard.BorderStyle = BorderStyle.None;

            pnlProductCard.Paint += DrawLightCardBorder;
            pnlQrCard.Paint += DrawLightCardBorder;
            pnlLinkCard.Paint += DrawLightCardBorder;

            pnlContent.Controls.Add(chkProductImage);
            pnlContent.Controls.Add(chkQrContent);

            Font titleFont18 = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblProductTitle.Font = titleFont18;
            lblQrTitle.Font = titleFont18;
            lblLinkTitle.Font = titleFont18;
            label1.Font = titleFont18;
            lblInfoTitle.Font = titleFont18;

            Color headerBackColor = Color.FromArgb(235, 243, 252);
            Color headerForeColor = Color.FromArgb(0, 85, 160);

            lblProductTitle.BackColor = headerBackColor;
            lblProductTitle.ForeColor = headerForeColor;
            lblQrTitle.BackColor = headerBackColor;
            lblQrTitle.ForeColor = headerForeColor;
            lblLinkTitle.BackColor = headerBackColor;
            lblLinkTitle.ForeColor = headerForeColor;

            label1.BackColor = Color.White;
            lblInfoTitle.BackColor = Color.White;
            lblGtin.BackColor = Color.White;
            lblProductId.BackColor = Color.White;

            lblGtin.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblProductId.Font = new Font("Segoe UI", 18F, FontStyle.Bold);

            btnCopyLink.FlatStyle = FlatStyle.Flat;
            btnCopyLink.FlatAppearance.BorderColor = Color.FromArgb(0, 102, 204);
            btnCopyLink.FlatAppearance.BorderSize = 1;
            btnCopyLink.BackColor = Color.White;
            btnCopyLink.ForeColor = Color.FromArgb(0, 102, 204);

            txtLinkSample.BackColor = Color.FromArgb(244, 245, 247);
            txtLinkSample.BorderStyle = BorderStyle.FixedSingle;

            chkProductImage.ForeColor = Color.FromArgb(45, 55, 65);
            chkQrContent.ForeColor = Color.FromArgb(45, 55, 65);
            pnlCheckConfirm.BackColor = Color.FromArgb(255, 250, 240);

            lblProductName.TextAlign = ContentAlignment.MiddleCenter;
            label1.TextAlign = ContentAlignment.MiddleCenter;
            lblInfoTitle.TextAlign = ContentAlignment.MiddleCenter;

            // TẠO VÀ XỬ LÝ DẤU HAI CHẤM ":" TÁCH BIỆT
            lblColon1 = new Label { Text = ":", AutoSize = true, BackColor = Color.White, Font = lblGtin.Font };
            lblColon2 = new Label { Text = ":", AutoSize = true, BackColor = Color.White, Font = lblProductId.Font };
            pnlLinkCard.Controls.Add(lblColon1);
            pnlLinkCard.Controls.Add(lblColon2);

            // Xóa dấu ":" dính liền trong Label gốc (nếu có)
            lblGtin.Text = lblGtin.Text.Replace(":", "").Trim();
            lblProductId.Text = lblProductId.Text.Replace(":", "").Trim();

            rtbProductInfo.ScrollBars = RichTextBoxScrollBars.None;
            rtbProducId.ScrollBars = RichTextBoxScrollBars.None;
            rtbProductInfo.ReadOnly = true;
            rtbProducId.ReadOnly = true;
        }

        private void DrawLightCardBorder(object sender, PaintEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl != null)
            {
                using (Pen p = new Pen(Color.FromArgb(215, 220, 225), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, ctrl.Width - 1, ctrl.Height - 1);
                }
            }
        }

        public string LinkSample
        {
            get { return txtLinkSample.Text; }
            set { txtLinkSample.Text = value ?? string.Empty; }
        }

        public string ProductTitle
        {
            get { return lblProductName.Text; }
            set { lblProductName.Text = string.IsNullOrWhiteSpace(value) ? "Sản phẩm mẫu" : value; }
        }

        public string JobCode
        {
            get { return lblJobCode.Text; }
            set { lblJobCode.Text = string.IsNullOrWhiteSpace(value) ? "Mã sản phẩm: --" : "Mã sản phẩm: " + value; }
        }

        public Image ProductImage
        {
            get { return picProductImage.Image; }
            set
            {
                picProductImage.Image = value;
                lblProductPlaceholder.Visible = value == null;
                if (value == null) lblProductPlaceholder.BringToFront();
            }
        }

        public Image QrImage
        {
            get { return picQrSample.Image; }
            set
            {
                picQrSample.Image = value;
                lblQrPlaceholder.Visible = value == null;
                if (value == null) lblQrPlaceholder.BringToFront();
            }
        }

        public string NsxHsd
        {
            get { return lblNsxHsd.Text; }
            set { lblNsxHsd.Text = value ?? string.Empty; }
        }

        public void SetValidateData(Image productImage, Image qrImage, string linkSample, string productName = "", string jobCode = "", string nsxHsd = "", string gtin = "")
        {
            ProductImage = productImage;
            QrImage = qrImage;
            LinkSample = linkSample;
            ProductTitle = productName;
            JobCode = jobCode;
            NsxHsd = nsxHsd;
            SetProductInfoDisplay(gtin, jobCode);

            if (productImage == null)
            {
                lblProductPlaceholder.Text = "Không tải được ảnh sản phẩm";
                lblProductPlaceholder.ForeColor = Color.Red;
                lblProductPlaceholder.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            }
        }

        public void SetValidateDataFromLocal(string productLocalPath, Image qrImage, string linkSample, string productName = "", string jobCode = "", string nsxHsd = "", string gtin = "")
        {
            Image productImage = ProductImageHelper.GetProductImage(productLocalPath, null);

            ProductImage = productImage;
            QrImage = qrImage;
            LinkSample = linkSample;
            ProductTitle = productName;
            JobCode = jobCode;
            NsxHsd = nsxHsd;
            SetProductInfoDisplay(gtin, jobCode);

            if (productImage == null)
            {
                lblProductPlaceholder.Text = "Không tải được ảnh sản phẩm";
                lblProductPlaceholder.ForeColor = Color.Red;
                lblProductPlaceholder.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            }
        }

        public void SetStartMode()
        {
            SkipConfirmStep = true;
            AutoConfirmCheckboxes = true;
            lblTitle.Text = "XÁC THỰC NỘI DUNG QR TRƯỚC KHI BẮT ĐẦU VẬN HÀNH";
            lblSubTitle.Text = "Kiểm tra hình ảnh sản phẩm, QR mẫu và link mẫu trước khi bắt đầu vận hành";
            txtLinkSample.TabStop = false;
            InitState();
        }

        private void SetProductInfoDisplay(string gtin, string productCode)
        {
            rtbProductInfo.Clear();
            if (!string.IsNullOrEmpty(gtin))
            {
                rtbProductInfo.Text = gtin;
                rtbProductInfo.SelectAll();
                rtbProductInfo.SelectionFont = new Font("Segoe UI", 18F, FontStyle.Bold);
                rtbProductInfo.SelectionColor = Color.Black;

                int last5Len = Math.Min(5, gtin.Length);
                rtbProductInfo.Select(gtin.Length - last5Len, last5Len);
                rtbProductInfo.SelectionColor = Color.Red;
                rtbProductInfo.Select(0, 0);
            }

            rtbProducId.Clear();
            if (!string.IsNullOrEmpty(productCode))
            {
                rtbProducId.Text = productCode;
                rtbProducId.SelectAll();
                rtbProducId.SelectionFont = new Font("Segoe UI", 18F, FontStyle.Bold);
                rtbProducId.SelectionColor = Color.Black;
                rtbProducId.Select(0, 0);
            }
        }

        private void InitState()
        {
            pnlConfirmOverlay.Visible = false;

            if (AutoConfirmCheckboxes)
            {
                chkProductImage.Checked = true;
                chkQrContent.Checked = true;
                chkProductImage.Visible = false;
                chkQrContent.Visible = false;
            }
            else
            {
                chkProductImage.Checked = false;
                chkQrContent.Checked = false;
            }

            btnContinue.Enabled = AutoConfirmCheckboxes || (chkProductImage.Checked && chkQrContent.Checked);
            lblProductPlaceholder.Visible = picProductImage.Image == null;
            lblQrPlaceholder.Visible = picQrSample.Image == null;
            if (lblProductPlaceholder.Visible) lblProductPlaceholder.BringToFront();
            if (lblQrPlaceholder.Visible) lblQrPlaceholder.BringToFront();
            UpdateContinueState();
            CenterConfirmCard();
        }

        private void CheckConfirmChanged(object sender, EventArgs e)
        {
            UpdateContinueState();
        }

        private void UpdateContinueState()
        {
            bool isReady = chkProductImage.Checked && chkQrContent.Checked;

            btnContinue.Enabled = isReady;
            btnContinue.BackColor = isReady ? Color.FromArgb(0, 150, 70) : Color.FromArgb(180, 180, 180);
            btnContinue.ForeColor = Color.White;

            if (isReady)
            {
                lblStatus.Text = "ĐÃ XÁC NHẬN ĐỦ ĐIỀU KIỆN CÓ THỂ TIẾP TỤC.";
                lblStatus.ForeColor = Color.FromArgb(0, 130, 60);
            }
            else
            {
                lblStatus.Text = "VUI LÒNG TICK ĐỦ 2 NỘI DUNG XÁC NHẬN TRƯỚC KHI TIẾP TỤC.";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (!chkProductImage.Checked || !chkQrContent.Checked)
            {
                MessageBox.Show(
                    "Vui lòng xác nhận hình ảnh sản phẩm và QR Code / Link mẫu trước khi tiếp tục.",
                    "Thiếu thông tin xác nhận",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (SkipConfirmStep)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            pnlConfirmOverlay.Visible = true;
            pnlConfirmOverlay.BringToFront();
            CenterConfirmCard();
            pnlFooter.Visible = false;
        }

        private void btnConfirmCancel_Click(object sender, EventArgs e)
        {
            pnlConfirmOverlay.Visible = false;
            pnlFooter.Visible = true;
        }

        private async void btnConfirmOk_Click(object sender, EventArgs e)
        {
            pnlConfirmOverlay.Visible = true;
            pnlConfirmOverlay.BringToFront();
            CenterConfirmCard();
            await Task.Delay(200);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnCopyLink_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtLinkSample.Text))
                {
                    Clipboard.SetText(txtLinkSample.Text.Trim());
                }
            }
            catch { }
        }

        private void frmValidateQrCode_Resize(object sender, EventArgs e)
        {
            ApplyResponsiveLayout();
            CenterConfirmCard();

            pnlProductCard.Invalidate();
            pnlQrCard.Invalidate();
            pnlLinkCard.Invalidate();
        }

        private void ApplyResponsiveLayout()
        {
            if (pnlContent == null || pnlContent.ClientSize.Width <= 0 || pnlContent.ClientSize.Height <= 0)
                return;

            int contentWidth = pnlContent.ClientSize.Width;
            int contentHeight = pnlContent.ClientSize.Height;

            int marginX = 25;
            int gapX = 20;

            int confirmHeight = 75;
            int confirmBottomMargin = 15;
            int confirmTop = contentHeight - confirmHeight - confirmBottomMargin;

            pnlCheckConfirm.SetBounds(marginX, confirmTop, contentWidth - marginX * 2, confirmHeight);

            int checkHeight = 45;
            int cardBottomMargin = 15;
            int cardTop = 20;

            int cardHeight = confirmTop - cardTop - cardBottomMargin - checkHeight - 10;
            int availableWidth = contentWidth - (marginX * 2) - (gapX * 2);
            int cardWidth = Math.Max(250, availableWidth / 3);

            int col1Left = marginX;
            int col2Left = col1Left + cardWidth + gapX;
            int col3Left = col2Left + cardWidth + gapX;

            pnlProductCard.SetBounds(col1Left, cardTop, cardWidth, cardHeight);
            pnlQrCard.SetBounds(col2Left, cardTop, cardWidth, cardHeight);
            pnlLinkCard.SetBounds(col3Left, cardTop, cardWidth, cardHeight);

            int checkboxTop = pnlProductCard.Bottom + 10;
            chkProductImage.SetBounds(col1Left + 20, checkboxTop, cardWidth, checkHeight);
            chkQrContent.SetBounds(col2Left + 20, checkboxTop, cardWidth * 2, checkHeight);

            ApplyProductCardLayout(cardWidth, cardHeight);
            ApplyQrCardLayout(cardWidth, cardHeight);
            ApplyLinkCardLayout(cardWidth, cardHeight);

            lblJobCode.Visible = false;

            if (pnlConfirmOverlay.Visible)
            {
                pnlConfirmOverlay.BringToFront();
            }
        }

        private void ApplyProductCardLayout(int cardWidth, int cardHeight)
        {
            int titleHeight = 50;
            lblProductTitle.SetBounds(0, 0, cardWidth, titleHeight);

            int nameLabelHeight = 60;
            lblProductName.SetBounds(10, cardHeight - nameLabelHeight - 10, cardWidth - 20, nameLabelHeight);

            int imageTop = titleHeight + 15;
            int imageHeight = lblProductName.Top - imageTop - 10;

            picProductImage.SetBounds(10, imageTop, cardWidth - 20, imageHeight);

            lblProductPlaceholder.SetBounds(
                picProductImage.Left,
                picProductImage.Top + Math.Max(0, (picProductImage.Height - 24) / 2),
                picProductImage.Width, 24);
        }

        private void ApplyQrCardLayout(int cardWidth, int cardHeight)
        {
            int titleHeight = 50;
            lblQrTitle.SetBounds(0, 0, cardWidth, titleHeight);

            int qrAreaHeight = cardHeight - titleHeight;
            int qrSize = Math.Min(cardWidth - 60, qrAreaHeight - 40);
            qrSize = Math.Max(150, qrSize);

            int qrLeft = Math.Max(10, (cardWidth - qrSize) / 2);
            int qrTop = titleHeight + Math.Max(10, (qrAreaHeight - qrSize) / 2);

            picQrSample.SetBounds(qrLeft, qrTop, qrSize, qrSize);

            lblQrPlaceholder.SetBounds(
                picQrSample.Left,
                picQrSample.Top + Math.Max(0, (picQrSample.Height - 24) / 2),
                picQrSample.Width, 24);
        }

        private void ApplyLinkCardLayout(int cardWidth, int cardHeight)
        {
            int titleHeight = 50;
            lblLinkTitle.SetBounds(0, 0, cardWidth, titleHeight);

            int availableHeight = cardHeight - titleHeight;
            int halfHeight = availableHeight / 2;
            int part1Top = titleHeight;

            int separatorY = part1Top + halfHeight;
            lblSeparator.Visible = true;
            lblSeparator.SetBounds(20, separatorY, cardWidth - 40, 1);
            lblSeparator.BackColor = Color.FromArgb(230, 230, 230);

            // NỬA TRÊN
            int labelHeight = 30;
            int sidePadding = 25;
            label1.SetBounds(10, part1Top + 10, cardWidth - 20, labelHeight);

            int btnHeight = 42;
            int btnTop = separatorY - btnHeight - 15;
            int contentWidth = cardWidth - (sidePadding * 2);
            btnCopyLink.SetBounds(sidePadding, btnTop, contentWidth, btnHeight);

            int txtTop = label1.Bottom + 5;

           // int txtHeight = btnTop - txtTop - 12;
            int txtHeight = btnTop - 30 - 15 -10;
            txtLinkSample.SetBounds(sidePadding, txtTop, contentWidth, txtHeight);

            // NỬA DƯỚI
            lblInfoTitle.SetBounds(10, separatorY + 15, cardWidth - 20, labelHeight);

            int infoStartY = lblInfoTitle.Bottom + 45;
            int rowSpacing = 70;

            // Đưa Icon và Label ra các lớp hiển thị trên cùng
            lblColon1.BringToFront();
            lblColon2.BringToFront();
            rtbProductInfo.BringToFront();
            rtbProducId.BringToFront();

            // TÍNH TOÁN TỌA ĐỘ VÀ KÍCH THƯỚC ICON CHUẨN XÁC
            int iconSize = 32; // Khóa cứng icon ở dạng vuông 32x32px
            pictureBox3.Size = new Size(iconSize, iconSize);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.Size = new Size(iconSize, iconSize);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            int iconX = sidePadding;
            int titleX = iconX + iconSize + 15; // Giãn ra xa Icon 15px

            // Tránh đè chữ bằng cách tính động độ rộng nhãn "Mã sản phẩm"
            int maxTitleWidth = Math.Max(lblGtin.Width, lblProductId.Width);
            int colonX = titleX + maxTitleWidth + 10;
            int valueX = colonX + 25;

            // --- DÒNG 1: GTIN ---
            int row1Y = infoStartY;
            // Công thức CĂN GIỮA: Y của Icon = Y của chữ + (Chiều cao chữ - Chiều cao Icon) / 2
            int icon1Y = row1Y + (lblGtin.Height - iconSize) / 2;
            pictureBox3.Location = new Point(iconX, icon1Y);
            lblGtin.Location = new Point(titleX, row1Y);
            lblColon1.Location = new Point(colonX, row1Y);
            rtbProductInfo.SetBounds(valueX, row1Y + 2, cardWidth - valueX - 5, 40);

            // --- DÒNG 2: MÃ SẢN PHẨM ---
            int row2Y = infoStartY + rowSpacing;
            int icon2Y = row2Y + (lblProductId.Height - iconSize) / 2; // Căn giữa
            pictureBox2.Location = new Point(iconX, icon2Y);
            lblProductId.Location = new Point(titleX, row2Y);
            lblColon2.Location = new Point(colonX, row2Y);
            rtbProducId.SetBounds(valueX, row2Y + 2, cardWidth - valueX - 5, 40);
        }

        private void CenterConfirmCard()
        {
            if (pnlConfirmOverlay == null || pnlConfirmCard == null) return;
            pnlConfirmCard.Left = (pnlConfirmOverlay.Width - pnlConfirmCard.Width) / 2;
            pnlConfirmCard.Top = (pnlConfirmOverlay.Height - pnlConfirmCard.Height) / 2;
        }

        private void label1_Click(object sender, EventArgs e) { }

        private void lblNote_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// CheckBox tùy chỉnh: Vẽ ô vuông màu xanh có bo góc nhạt giống thiết kế
    /// </summary>
    public class LargeCheckBox : CheckBox
    {
        private int checkBoxSize = 28;

        public int CheckBoxSize
        {
            get { return checkBoxSize; }
            set { checkBoxSize = Math.Max(16, value); Invalidate(); }
        }

        public LargeCheckBox()
        {
            AutoSize = false;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int boxTop = Math.Max(0, (Height - CheckBoxSize) / 2);
            Rectangle boxRectangle = new Rectangle(1, boxTop, CheckBoxSize - 2, CheckBoxSize - 2);

            Color borderColor = Enabled ? Color.FromArgb(105, 115, 125) : Color.FromArgb(180, 180, 180);
            Color fillColor = Checked ? Color.FromArgb(0, 150, 70) : Color.White;

            using (SolidBrush fillBrush = new SolidBrush(fillColor))
            using (Pen borderPen = new Pen(Checked ? fillColor : borderColor, 1.5F))
            {
                e.Graphics.FillRectangle(fillBrush, boxRectangle);
                e.Graphics.DrawRectangle(borderPen, boxRectangle);
            }

            if (Checked)
            {
                PointF[] checkPoints =
                {
                    new PointF(boxRectangle.Left + boxRectangle.Width * 0.22F, boxRectangle.Top + boxRectangle.Height * 0.50F),
                    new PointF(boxRectangle.Left + boxRectangle.Width * 0.42F, boxRectangle.Top + boxRectangle.Height * 0.72F),
                    new PointF(boxRectangle.Left + boxRectangle.Width * 0.78F, boxRectangle.Top + boxRectangle.Height * 0.28F)
                };

                using (Pen checkPen = new Pen(Color.White, Math.Max(2.5F, CheckBoxSize / 8F)))
                {
                    checkPen.StartCap = LineCap.Round;
                    checkPen.EndCap = LineCap.Round;
                    checkPen.LineJoin = LineJoin.Round;
                    e.Graphics.DrawLines(checkPen, checkPoints);
                }
            }

            Rectangle textRectangle = new Rectangle(CheckBoxSize + 9, 0, Math.Max(0, Width - CheckBoxSize - 9), Height);

            TextRenderer.DrawText(
                e.Graphics, Text, Font, textRectangle,
                Enabled ? ForeColor : SystemColors.GrayText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

            if (Focused && ShowFocusCues)
            {
                Rectangle focusRectangle = textRectangle;
                focusRectangle.Inflate(-1, -4);
                ControlPaint.DrawFocusRectangle(e.Graphics, focusRectangle);
            }
        }
    }
}