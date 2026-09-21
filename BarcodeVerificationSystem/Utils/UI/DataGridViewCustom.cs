using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Controller
{
    public class DataGridViewCustom
    {
        public static void InitDataGridView(DataGridView dgv, string[] columns, int imgIndex = 0, bool isPOD = false)
        {
            dgv.VirtualMode = false;
            dgv.AllowUserToAddRows = false;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.BorderStyle = BorderStyle.None;
            Color cor = dgv.Parent != null ? dgv.Parent.BackColor : Color.Wheat;
            dgv.BackgroundColor = cor;
            dgv.Rows.Clear();
            dgv.Columns.Clear();
            dgv.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular);
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            int tableWidth = dgv.Width;
            float percentWidth = (float)1 / columns.Length;
            int tableCodeProductListWidth = dgv.Width - 39;
            for (int index = 0; index < columns.Length; index++)
            {
                if (index == imgIndex)
                {
                    var col = new DataGridViewImageColumn
                    {
                        HeaderText = columns[index],
                        Name = columns[index].Trim(),
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    if (index == 0)
                    {
                        col.Width = (int)(0.75 * tableCodeProductListWidth);
                    }
                    Size textSize = TextRenderer.MeasureText(col.HeaderText, dgv.Font);
                    col.Width = textSize.Width + 40;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv.Columns.Add(col);
                }
                else
                {
                    var col = new DataGridViewTextBoxColumn
                    {
                        HeaderText = (index != 0 && columns.Count() - 1 != index && isPOD) ? columns[index] + " - Field" + (index) : columns[index],
                        Name = columns[index].Trim(),
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    if (index == 0)
                    {
                        col.Width = (int)(0.75 * tableCodeProductListWidth);
                    }
                    Size textSize = TextRenderer.MeasureText(col.HeaderText, dgv.Font);
                    col.Width = textSize.Width + 40;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv.Columns.Add(col);
                }
            }

            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.DefaultCellStyle.ForeColor = SystemColors.WindowFrame;
            dgv.DefaultCellStyle.BackColor = cor;
            dgv.ColumnHeadersHeight = 35;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = cor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.WindowFrame;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular);
            dgv.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 232, 255);
            dgv.RowsDefaultCellStyle.SelectionForeColor = SystemColors.WindowFrame;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);
            dgv.AllowUserToResizeRows = false;
            dgv.RowPostPaint += (obj, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.Gainsboro),
                    0, 0, dgv.Width, 0);
                if (e.RowIndex != -1)
                {
                    Rectangle rowRectangle = dgv.GetRowDisplayRectangle(e.RowIndex, true);
                    e.Graphics.DrawLine(new Pen(Color.Gainsboro),
                        rowRectangle.X, rowRectangle.Y, rowRectangle.Width, rowRectangle.Y);
                }
            };
        }

        public static void AdjustColumnWidthsToFitContent(DataGridView dgv)
        {
            // Adjust the DataGridView
            if (dgv.Columns.Count > 0)
            {
                // Step 1: Autosize all columns to fit content
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                // Step 2: Calculate total column width after autosizing
                int totalColumnWidth = 0;
                foreach (DataGridViewColumn column in dgv.Columns)
                {
                    totalColumnWidth += column.Width;
                }

                // Step 3: Set MinimumWidth and FillWeight based on autosized width, and set mode to Fill for each
                foreach (DataGridViewColumn column in dgv.Columns)
                {
                    column.MinimumWidth = column.Width;
                    column.FillWeight = (float)column.Width;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                // Step 4: Get available client width
                int clientWidth = dgv.ClientSize.Width;

                // Step 5: Decide mode based on total width
                if (totalColumnWidth >= clientWidth)
                {
                    // Keep content-fit widths and allow scrolling
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                }
                else
                {
                    // Fill proportionally using FillWeights
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                // Step 6: Always enable both horizontal and vertical scrolling
                dgv.ScrollBars = ScrollBars.Both;
            }
        }
        //public static void AdjustColumnWidthsToFitContent(DataGridView dgv)
        //{
        //    // Adjust dgvCheckedResult
        //    if (dgv.Columns.Count > 0)
        //    {
        //        // Step 1: Autosize all columns to fit content
        //        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        //        dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

        //        // Step 2: Calculate total column width after autosizing
        //        int totalColumnWidth = 0;
        //        foreach (DataGridViewColumn column in dgv.Columns)
        //        {
        //            totalColumnWidth += column.Width;
        //        }

        //        // Step 3: Set MinimumWidth and FillWeight based on autosized width, and set mode to Fill for each
        //        foreach (DataGridViewColumn column in dgv.Columns)
        //        {
        //            column.MinimumWidth = column.Width;
        //            column.FillWeight = (float)column.Width;
        //            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        //        }

        //        // Step 4: Get available client width
        //        int clientWidth = dgv.ClientSize.Width;

        //        // Step 5: Decide mode based on total width
        //        if (totalColumnWidth >= clientWidth)
        //        {
        //            // Keep content-fit widths and allow scrolling
        //            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        //            dgv.ScrollBars = ScrollBars.Horizontal;
        //        }
        //        else
        //        {
        //            // Fill proportionally using FillWeights
        //            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        //            dgv.ScrollBars = ScrollBars.None;
        //        }
        //    }
        //}

        public static void AutoResizeColumnWith(DataGridView dgv, string[] value)
        {
            try
            {
                string[] firstRowWith = value;
                int totalColumnsWidth = 0;
                int[] thickestRowIndex = { 0, TextRenderer.MeasureText(firstRowWith[0], dgv.Font).Width };
                for (int i = 0; i < firstRowWith.Length; i++)
                {
                    Size colTextSize = TextRenderer.MeasureText(dgv.Columns[i].HeaderText, dgv.Font);
                    Size rowTextSize = TextRenderer.MeasureText(firstRowWith[i], dgv.Font);
                    if (rowTextSize.Width > thickestRowIndex[1])
                    {
                        thickestRowIndex[0] = i;
                        thickestRowIndex[1] = rowTextSize.Width;
                    }

                    if (colTextSize.Width < rowTextSize.Width)
                    {
                        dgv.Columns[i].Width = rowTextSize.Width + 40;
                    }
                    else if (colTextSize.Width > rowTextSize.Width)
                    {
                        dgv.Columns[i].Width = colTextSize.Width + 40;
                    }
                    totalColumnsWidth += dgv.Columns[i].Width;
                }
                if (totalColumnsWidth < dgv.Width)
                {
                    dgv.Columns[thickestRowIndex[0]].Width += dgv.Width - totalColumnsWidth - 30;
                }
            }
            catch
            {

            }
        }

    }
}
