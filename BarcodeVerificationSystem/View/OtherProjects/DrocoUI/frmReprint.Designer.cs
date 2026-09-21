using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.DrocoUI
{
    partial class frmReprint
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmReprint));
            this.tabRePrint = new System.Windows.Forms.TabControl();
            this.tabRePrintQr = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.dgvListQrReprint = new System.Windows.Forms.DataGridView();
            this.tabRePrint.SuspendLayout();
            this.tabRePrintQr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvListQrReprint)).BeginInit();
            this.SuspendLayout();
            // 
            // tabRePrint
            // 
            this.tabRePrint.Controls.Add(this.tabRePrintQr);
            this.tabRePrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabRePrint.Location = new System.Drawing.Point(2, 2);
            this.tabRePrint.Name = "tabRePrint";
            this.tabRePrint.SelectedIndex = 0;
            this.tabRePrint.Size = new System.Drawing.Size(1203, 553);
            this.tabRePrint.TabIndex = 0;
            // 
            // tabRePrintQr
            // 
            this.tabRePrintQr.Controls.Add(this.label2);
            this.tabRePrintQr.Controls.Add(this.dgvListQrReprint);
            this.tabRePrintQr.Location = new System.Drawing.Point(4, 29);
            this.tabRePrintQr.Name = "tabRePrintQr";
            this.tabRePrintQr.Padding = new System.Windows.Forms.Padding(3);
            this.tabRePrintQr.Size = new System.Drawing.Size(1195, 520);
            this.tabRePrintQr.TabIndex = 1;
            this.tabRePrintQr.Text = "In lại mã Hộp/Thùng/Pallet";
            this.tabRePrintQr.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(18, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(277, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Danh sách mã Hộp/Thùng/Pallet in lại ";
            // 
            // dgvListQrReprint
            // 
            this.dgvListQrReprint.AllowUserToAddRows = false;
            this.dgvListQrReprint.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.dgvListQrReprint.GridColor = System.Drawing.Color.FromArgb(235, 237, 240); // Đường kẻ mờ
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvListQrReprint.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvListQrReprint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvListQrReprint.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvListQrReprint.BackgroundColor = System.Drawing.Color.White;
            this.dgvListQrReprint.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvListQrReprint.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgvListQrReprint.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvListQrReprint.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvListQrReprint.ColumnHeadersHeight = 45;
            this.dgvListQrReprint.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvListQrReprint.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvListQrReprint.EnableHeadersVisualStyles = false;
            this.dgvListQrReprint.Location = new System.Drawing.Point(22, 47);
            this.dgvListQrReprint.MultiSelect = false;
            this.dgvListQrReprint.Name = "dgvListQrReprint";
            this.dgvListQrReprint.ReadOnly = true;
            this.dgvListQrReprint.RowHeadersVisible = false;
            this.dgvListQrReprint.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvListQrReprint.RowTemplate.Height = 35;
            this.dgvListQrReprint.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvListQrReprint.Size = new System.Drawing.Size(1159, 453);
            this.dgvListQrReprint.TabIndex = 0;
            // 
            // frmReprint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1208, 558);
            this.Controls.Add(this.tabRePrint);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmReprint";
            this.Text = "R-LINK";
            this.tabRePrint.ResumeLayout(false);
            this.tabRePrintQr.ResumeLayout(false);
            this.tabRePrintQr.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvListQrReprint)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabRePrint;
        private System.Windows.Forms.TabPage tabRePrintQr;
        private System.Windows.Forms.DataGridView dgvListQrReprint;
        private Label label2;
    }
}