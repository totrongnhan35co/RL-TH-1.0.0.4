using BarcodeVerificationSystem.Labels.ProjectLabel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Controller.HistorySync
{
    public class HistoryUtils
    {
        public static void CustomDataGridView(DataGridView dgv)
        {
            dgv.Columns["STT"].Width = 50;
            dgv.Columns["MaCongViec"].Width = 300;
            dgv.Columns["MaPhieuSoanHang"].Width = 180;
            dgv.Columns["MaSanPham"].Width = 100;
            dgv.Columns["SoLuongCanXuat"].Width = 120;
            dgv.Columns["SoLuongDongBoSaaS"].Width = 150;
            dgv.Columns["SoLuongDongBoSAP"].Width = 150;
            dgv.Columns["HoanThanh"].Width = 150;
            dgv.RowTemplate.Height = 45;

            if (ProjectLabel.IsCaoSuDongNai)
            {
                dgv.Columns["MaPhieuSoanHang"].HeaderText = "Số LOT";
                dgv.Columns["SoLuongCanXuat"].HeaderText = "Số lượng in đã đồng bộ";
                dgv.Columns["SoLuongDongBoSaaS"].HeaderText = "Số lượng kiểm tra đã đồng bộ";
                dgv.Columns["SoLuongDongBoSAP"].HeaderText = "Số lượng mã đồng bộ trên pallet";

            }

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }


            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#D3D3D3");
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ClearSelection();
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor;


            // Column behavior
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;

            // Appearance
            dgv.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#086D46"); //Color.SteelBlue;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;

            // Header style
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Row style
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.RowHeadersVisible = false; // optional

            // Scrollbars
            dgv.ScrollBars = ScrollBars.Both;
        }

        public static void StyleButton(Button btn)
        {
            // Flat style
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            // Background & text color
            btn.BackColor = ColorTranslator.FromHtml("#007ACC");  // Blue
            btn.ForeColor = Color.White;

            // Font
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Padding and margin
            btn.Padding = new Padding(10, 5, 10, 5);
            btn.Margin = new Padding(5);

            // Optional: Rounded corners (simulate with region)
            btn.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 10, 10)
            );

            // Hover effect
            btn.MouseEnter += (s, e) => btn.BackColor = ColorTranslator.FromHtml("#005A9E");
            btn.MouseLeave += (s, e) => btn.BackColor = ColorTranslator.FromHtml("#007ACC");
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
    int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
    int nWidthEllipse, int nHeightEllipse);


    }
}
