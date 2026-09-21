namespace BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess
{
    partial class frmBatchInfoManual
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBatchInfoManual));
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.saveJobNuti = new DesignUI.CuzUI.CuzButton();
            this.Close = new DesignUI.CuzUI.CuzButton();
            this.batch = new DesignUI.CuzUI.CuzTextBox();
            this.MaufDatePicker = new System.Windows.Forms.DateTimePicker();
            this.ExpiredDatePicker = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoEllipsis = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label2.Location = new System.Drawing.Point(31, 186);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(126, 20);
            this.label2.TabIndex = 163;
            this.label2.Text = "Ngày hết hạn";
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label1.Location = new System.Drawing.Point(31, 122);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 20);
            this.label1.TabIndex = 162;
            this.label1.Text = "Ngày sản xuất";
            // 
            // label9
            // 
            this.label9.AutoEllipsis = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label9.Location = new System.Drawing.Point(31, 57);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(102, 20);
            this.label9.TabIndex = 158;
            this.label9.Text = "Số LOT";
            // 
            // saveJobNuti
            // 
            this.saveJobNuti._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.saveJobNuti._BorderRadius = 20;
            this.saveJobNuti._BorderSize = 1;
            this.saveJobNuti._GradientsButton = false;
            this.saveJobNuti._Text = "Thêm";
            this.saveJobNuti.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.saveJobNuti.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.saveJobNuti.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.saveJobNuti.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.saveJobNuti.FlatAppearance.BorderSize = 0;
            this.saveJobNuti.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveJobNuti.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.saveJobNuti.ForeColor = System.Drawing.Color.White;
            this.saveJobNuti.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.saveJobNuti.Location = new System.Drawing.Point(295, 223);
            this.saveJobNuti.Name = "saveJobNuti";
            this.saveJobNuti.Size = new System.Drawing.Size(72, 37);
            this.saveJobNuti.TabIndex = 164;
            this.saveJobNuti.Text = "Thêm";
            this.saveJobNuti.TextColor = System.Drawing.Color.White;
            this.saveJobNuti.UseVisualStyleBackColor = false;
            this.saveJobNuti.Click += new System.EventHandler(this.AddItem);
            // 
            // Close
            // 
            this.Close._BorderColor = System.Drawing.Color.Coral;
            this.Close._BorderRadius = 20;
            this.Close._BorderSize = 1;
            this.Close._GradientsButton = false;
            this.Close._Text = "Hủy";
            this.Close.BackColor = System.Drawing.Color.Coral;
            this.Close.BackgroundColor = System.Drawing.Color.Coral;
            this.Close.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.Close.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.Close.FlatAppearance.BorderSize = 0;
            this.Close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Close.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.Close.ForeColor = System.Drawing.Color.White;
            this.Close.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Close.Location = new System.Drawing.Point(380, 223);
            this.Close.Name = "Close";
            this.Close.Size = new System.Drawing.Size(72, 37);
            this.Close.TabIndex = 165;
            this.Close.Text = "Hủy";
            this.Close.TextColor = System.Drawing.Color.White;
            this.Close.UseVisualStyleBackColor = false;
            this.Close.Click += new System.EventHandler(this.Close_Click);
            // 
            // batch
            // 
            this.batch._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.batch._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.batch.BackColor = System.Drawing.Color.White;
            this.batch.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.batch.BorderFocusColor = System.Drawing.Color.Silver;
            this.batch.BorderRadius = 6;
            this.batch.BorderSize = 1;
            this.batch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.batch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.batch.Location = new System.Drawing.Point(173, 42);
            this.batch.Margin = new System.Windows.Forms.Padding(4);
            this.batch.Multiline = false;
            this.batch.Name = "batch";
            this.batch.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.batch.PasswordChar = false;
            this.batch.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.batch.PlaceholderText = "";
            this.batch.ReadOnly = false;
            this.batch.Size = new System.Drawing.Size(279, 35);
            this.batch.TabIndex = 166;
            this.batch.UnderlinedStyle = false;
            // 
            // MaufDatePicker
            // 
            this.MaufDatePicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaufDatePicker.Location = new System.Drawing.Point(173, 117);
            this.MaufDatePicker.Name = "MaufDatePicker";
            this.MaufDatePicker.Size = new System.Drawing.Size(275, 26);
            this.MaufDatePicker.TabIndex = 168;
            this.MaufDatePicker.Value = new System.DateTime(2025, 9, 19, 10, 31, 33, 0);
            // 
            // ExpiredDatePicker
            // 
            this.ExpiredDatePicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExpiredDatePicker.Location = new System.Drawing.Point(173, 181);
            this.ExpiredDatePicker.Name = "ExpiredDatePicker";
            this.ExpiredDatePicker.Size = new System.Drawing.Size(275, 26);
            this.ExpiredDatePicker.TabIndex = 169;
            // 
            // frmBatchInfoManual
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 272);
            this.Controls.Add(this.ExpiredDatePicker);
            this.Controls.Add(this.MaufDatePicker);
            this.Controls.Add(this.batch);
            this.Controls.Add(this.Close);
            this.Controls.Add(this.saveJobNuti);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label9);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmBatchInfoManual";
            this.Text = "frmBatchInfo";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label9;
        private DesignUI.CuzUI.CuzButton saveJobNuti;
        private DesignUI.CuzUI.CuzButton Close;
        private DesignUI.CuzUI.CuzTextBox batch;
        private System.Windows.Forms.DateTimePicker MaufDatePicker;
        private System.Windows.Forms.DateTimePicker ExpiredDatePicker;
    }
}