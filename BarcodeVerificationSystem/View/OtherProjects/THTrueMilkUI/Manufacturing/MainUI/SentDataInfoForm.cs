using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing.MainUI
{

    public sealed class SentDataInfoForm : Form
    {
        private static readonly Color BackgroundColor =
            Color.FromArgb(245, 247, 250);

        private static readonly Color HeaderColor =
            Color.FromArgb(0, 171, 230);

        private static readonly Color SentColor =
            Color.FromArgb(40, 120, 210);

        private static readonly Color ReceiveColor =
            Color.FromArgb(35, 165, 105);

        private static readonly Color PrintColor =
            Color.FromArgb(235, 142, 45);

        private static readonly Color CheckColor =
            Color.FromArgb(100, 80, 170);

        private static readonly Color RejectColor =
            Color.FromArgb(218, 70, 87);

        private static readonly Color TextColor =
            Color.FromArgb(45, 55, 65);

        // Viền ngoài giúp Form con tách biệt rõ với Form chính.
        private static readonly Color FormBorderColor =
            Color.FromArgb(52, 126, 165);

        private const int FormBorderThickness = 3;

        public SentDataInfoForm(
            long numberSent,
            long totalSent,
            long numberReceived,
            long totalReceived,
            long numberPrinted,
            long totalRsfp,
            long lastPrintedPage,
            long numberA,
            long numberB,
            long numberF,
            long totalChecked,
            long cameraRejected,
            long rlinkRejected)
        {
            InitializeForm();

            Panel header = CreateHeader();

            TableLayoutPanel content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BackgroundColor,
                Padding = new Padding(18, 16, 18, 12),
                ColumnCount = 1,
                RowCount = 6,
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize
            };

            // Dùng chiều cao cố định cho từng card để nội dung không bị ép.
            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 118F));

            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 118F));

            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 152F));

            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 185F));

            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 118F));

            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 52F));

            content.Controls.Add(
                CreateSection(
                    "SENT",
                    "Dữ liệu gửi đến máy in",
                    SentColor,
                    new[]
                    {
                    new StatItem("Đã gửi", numberSent),
                    new StatItem("Tổng gửi", totalSent)
                    }),
                0,
                0);

            content.Controls.Add(
                CreateSection(
                    "RECEIVE",
                    "Dữ liệu từ máy in",
                    ReceiveColor,
                    new[]
                    {
                    new StatItem("Đã nhận", numberReceived),
                    new StatItem("Tổng nhận", totalReceived)
                    }),
                0,
                1);

            content.Controls.Add(
                CreateSection(
                    "PRINT",
                    "Trạng thái in hiện tại",
                    PrintColor,
                    new[]
                    {
                    new StatItem("Đã in", numberPrinted),
                    new StatItem("Gói tin", totalRsfp),
                    new StatItem("Trang in cuối", lastPrintedPage)
                    }),
                0,
                2);

            string FormatPercent(long value, long total)
            {
                double pct = total > 0 ? value * 100.0 / total : 0;
                return $"{value:N0} ({pct:F2}%)";
            }

            content.Controls.Add(
                CreateSection(
                    "CHECK",
                    "Dữ liệu kiểm tra",
                    CheckColor,
                    new[]
                    {
                    // Chỉ đổi màu phần giá trị và phần trăm.
                    new StatItem(
                        "Kết quả A",
                        FormatPercent(numberA, totalChecked),
                        Color.FromArgb(35, 165, 105)),

                    new StatItem(
                        "Kết quả B",
                        FormatPercent(numberB, totalChecked),
                        Color.FromArgb(235, 142, 45)),

                    new StatItem(
                        "Kết quả F",
                        FormatPercent(numberF, totalChecked),
                        Color.FromArgb(218, 70, 87)),

                    new StatItem(
                        "Tổng kiểm tra",
                        totalChecked)
                    }),
                0,
                3);

            // Số mã bị loại: Camera phân loại lỗi (label22) và R-Link phân loại lỗi (label20) trên frmMain.
            content.Controls.Add(
                CreateSection(
                    "REJECT",
                    "Mã bị loại khi kiểm tra",
                    RejectColor,
                    new[]
                    {
                    new StatItem(
                        "Camera phân loại",
                        cameraRejected),

                    new StatItem(
                        "R-Link phân loại",
                        rlinkRejected)
                    }),
                0,
                4);

            Button closeButton = new Button
            {
                Text = "Đóng",
                Width = 110,
                Height = 36,
                Anchor = AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = HeaderColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold),
                Margin = new Padding(0, 8, 0, 0)
            };

            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (sender, e) => Close();

            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackgroundColor
            };

            buttonPanel.Controls.Add(closeButton);

            buttonPanel.Resize += (sender, e) =>
            {
                closeButton.Left =
                    buttonPanel.ClientSize.Width -
                    closeButton.Width;

                closeButton.Top =
                    Math.Max(
                        0,
                        (buttonPanel.ClientSize.Height -
                         closeButton.Height) / 2);
            };

            content.Controls.Add(
                buttonPanel,
                0,
                5);

            Controls.Add(content);
            Controls.Add(header);
        }

        private void InitializeForm()
        {
            Text = "Thông tin truyền nhận - Kết quả kiểm tra";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.None;

            // BackColor được dùng làm màu viền vì các control con
            // được Dock bên trong vùng Padding của Form.
            BackColor = FormBorderColor;
            Padding = new Padding(FormBorderThickness);

            ClientSize =
                new Size(560, 845);

            MinimumSize =
                new Size(520, 745);

            MaximumSize =
                new Size(760, 1000);

            AutoScaleMode =
                AutoScaleMode.Dpi;

            ShowInTaskbar = false;

            KeyPreview = true;

            KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
            };
        }

        private Panel CreateHeader()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = HeaderColor
            };

            Label iconLabel = new Label
            {
                AutoSize = false,
                Text = "ⓘ",
                Width = 42,
                Dock = DockStyle.Left,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI Symbol",
                    20F,
                    FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label titleLabel = new Label
            {
                AutoSize = false,
                Text = "THÔNG TIN TRUYỀN NHẬN",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    13F,
                    FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Button closeButton = new Button
            {
                Text = "×",
                Dock = DockStyle.Right,
                Width = 52,
                FlatStyle = FlatStyle.Flat,
                BackColor = HeaderColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font(
                    "Segoe UI",
                    20F,
                    FontStyle.Regular),
                TabStop = false
            };

            closeButton.FlatAppearance.BorderSize = 0;

            closeButton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(205, 65, 65);

            closeButton.Click +=
                (sender, e) => Close();

            header.Controls.Add(titleLabel);
            header.Controls.Add(closeButton);
            header.Controls.Add(iconLabel);

            EnableFormDragging(header);
            EnableFormDragging(titleLabel);
            EnableFormDragging(iconLabel);

            return header;
        }

        private Control CreateSection(
            string title,
            string subtitle,
            Color accentColor,
            StatItem[] items)
        {
            RoundedPanel section = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0),
                BackColor = Color.White,
                BorderColor = Color.FromArgb(220, 227, 234),
                BorderThickness = 1,
                CornerRadius = 8
            };

            Panel accent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = accentColor
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 7, 14, 7),
                BackColor = Color.White,
                ColumnCount = 2,
                RowCount = items.Length + 1
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    58F));

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    42F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    34F));

            Label titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Text = title,
                ForeColor = accentColor,
                Font = new Font(
                    "Segoe UI",
                    12F,
                    FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label subtitleLabel = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Text = subtitle,
                ForeColor = Color.FromArgb(120, 130, 140),
                Font = new Font(
                    "Segoe UI",
                    12F,
                    FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleRight
            };

            layout.Controls.Add(
                titleLabel,
                0,
                0);

            layout.Controls.Add(
                subtitleLabel,
                1,
                0);

            for (int index = 0;
                 index < items.Length;
                 index++)
            {
                int row = index + 1;

                layout.RowStyles.Add(
                    new RowStyle(
                        SizeType.Percent,
                        100F / items.Length));

                Label captionLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    AutoSize = false,
                    Text = items[index].Caption,
                    ForeColor = TextColor,
                    Font = new Font(
                        "Segoe UI",
                        12F,
                        FontStyle.Regular),
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

                Label valueLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    AutoSize = false,
                    Text = items[index].DisplayValue,
                    // Chỉ valueLabel dùng màu riêng.
                    // Caption vẫn giữ TextColor.
                    ForeColor =
                        items[index].ValueColor ??
                        accentColor,
                    Font = new Font(
                        "Segoe UI",
                        15F,
                        FontStyle.Bold),
                    TextAlign =
                        ContentAlignment.MiddleRight
                };

                layout.Controls.Add(
                    captionLabel,
                    0,
                    row);

                layout.Controls.Add(
                    valueLabel,
                    1,
                    row);
            }

            section.Controls.Add(layout);
            section.Controls.Add(accent);

            return section;
        }

        private void EnableFormDragging(Control control)
        {
            Point startPoint = Point.Empty;
            bool dragging = false;

            control.MouseDown += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                dragging = true;
                startPoint = e.Location;
            };

            control.MouseMove += (sender, e) =>
            {
                if (!dragging)
                {
                    return;
                }

                Point screenPoint =
                    control.PointToScreen(e.Location);

                Location = new Point(
                    screenPoint.X - startPoint.X,
                    screenPoint.Y - startPoint.Y);
            };

            control.MouseUp += (sender, e) =>
            {
                dragging = false;
            };
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            base.OnPaint(e);

            if (ClientSize.Width <= 1 ||
                ClientSize.Height <= 1)
            {
                return;
            }

            // Đường ngoài đậm.
            using (Pen outerPen = new Pen(
                FormBorderColor,
                2F))
            {
                outerPen.Alignment =
                    PenAlignment.Inset;

                e.Graphics.DrawRectangle(
                    outerPen,
                    0,
                    0,
                    ClientSize.Width - 1,
                    ClientSize.Height - 1);
            }

            // Đường sáng phía trong tạo cảm giác Form nổi hơn.
            using (Pen innerPen = new Pen(
                Color.FromArgb(190, 215, 230),
                1F))
            {
                int inset = FormBorderThickness - 1;

                e.Graphics.DrawRectangle(
                    innerPen,
                    inset,
                    inset,
                    Math.Max(
                        0,
                        ClientSize.Width -
                        inset * 2 - 1),
                    Math.Max(
                        0,
                        ClientSize.Height -
                        inset * 2 - 1));
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;

                CreateParams parameters =
                    base.CreateParams;

                parameters.ClassStyle |=
                    CS_DROPSHADOW;

                return parameters;
            }
        }

        private sealed class StatItem
        {
            public string Caption { get; }

            public string DisplayValue { get; }

            public Color? ValueColor { get; }

            public StatItem(
                string caption,
                long value,
                Color? valueColor = null)
            {
                Caption =
                    caption ?? string.Empty;

                DisplayValue =
                    Math.Max(0, value).ToString("N0");

                ValueColor =
                    valueColor;
            }

            public StatItem(
                string caption,
                string displayValue,
                Color? valueColor = null)
            {
                Caption =
                    caption ?? string.Empty;

                DisplayValue =
                    displayValue ?? string.Empty;

                ValueColor =
                    valueColor;
            }
        }

        private sealed class RoundedPanel : Panel
        {
            public Color BorderColor { get; set; } =
                Color.FromArgb(220, 227, 234);

            public int BorderThickness { get; set; } = 1;

            public int CornerRadius { get; set; } = 8;

            public RoundedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);

                if (Width <= 0 || Height <= 0)
                {
                    return;
                }

                using (GraphicsPath path =
                    CreateRoundedPath(
                        new Rectangle(
                            0,
                            0,
                            Width,
                            Height),
                        CornerRadius))
                {
                    Region oldRegion = Region;
                    Region = new Region(path);
                    oldRegion?.Dispose();
                }
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
                    CreateRoundedPath(
                        rectangle,
                        CornerRadius))
                using (Pen pen =
                    new Pen(
                        BorderColor,
                        BorderThickness))
                {
                    e.Graphics.DrawPath(
                        pen,
                        path);
                }
            }

            private static GraphicsPath CreateRoundedPath(
                Rectangle rectangle,
                int radius)
            {
                GraphicsPath path =
                    new GraphicsPath();

                if (radius <= 1)
                {
                    path.AddRectangle(rectangle);
                    path.CloseFigure();
                    return path;
                }

                int diameter = radius * 2;

                Rectangle arc = new Rectangle(
                    rectangle.X,
                    rectangle.Y,
                    diameter,
                    diameter);

                path.AddArc(arc, 180, 90);

                arc.X =
                    rectangle.Right -
                    diameter;

                path.AddArc(arc, 270, 90);

                arc.Y =
                    rectangle.Bottom -
                    diameter;

                path.AddArc(arc, 0, 90);

                arc.X = rectangle.Left;

                path.AddArc(arc, 90, 90);

                path.CloseFigure();

                return path;
            }
        }
    }
}
