namespace BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess
{
    partial class frmBatchInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBatchInfo));
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.LOTNumberCombo = new System.Windows.Forms.ComboBox();
            this.saveJobNuti = new DesignUI.CuzUI.CuzButton();
            this.Close = new DesignUI.CuzUI.CuzButton();
            this.CreateNewLOT = new DesignUI.CuzUI.CuzButton();
            this.ExpireDate = new DesignUI.CuzUI.CuzTextBox();
            this.MaufDate = new DesignUI.CuzUI.CuzTextBox();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoEllipsis = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label2.Location = new System.Drawing.Point(44, 186);
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
            this.label1.Location = new System.Drawing.Point(44, 122);
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
            this.label9.Location = new System.Drawing.Point(53, 57);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(102, 20);
            this.label9.TabIndex = 158;
            this.label9.Text = "Số LOT";
            // 
            // LOTNumberCombo
            // 
            this.LOTNumberCombo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LOTNumberCombo.FormattingEnabled = true;
            this.LOTNumberCombo.Location = new System.Drawing.Point(177, 49);
            this.LOTNumberCombo.Name = "LOTNumberCombo";
            this.LOTNumberCombo.Size = new System.Drawing.Size(275, 28);
            this.LOTNumberCombo.TabIndex = 157;
            // 
            // saveJobNuti
            // 
            this.saveJobNuti._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(199)))), ((int)(((byte)(82)))));
            this.saveJobNuti._BorderRadius = 20;
            this.saveJobNuti._BorderSize = 1;
            this.saveJobNuti._GradientsButton = false;
            this.saveJobNuti._Text = "Chọn";
            this.saveJobNuti.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(109)))), ((int)(((byte)(70)))));
            this.saveJobNuti.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(109)))), ((int)(((byte)(70)))));
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
            this.saveJobNuti.Text = "Chọn";
            this.saveJobNuti.TextColor = System.Drawing.Color.White;
            this.saveJobNuti.UseVisualStyleBackColor = false;
            this.saveJobNuti.Click += new System.EventHandler(this.Choose);
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
            // CreateNewLOT
            // 
            this.CreateNewLOT._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.CreateNewLOT._BorderRadius = 20;
            this.CreateNewLOT._BorderSize = 1;
            this.CreateNewLOT._GradientsButton = false;
            this.CreateNewLOT._Text = "Thêm mới";
            this.CreateNewLOT.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.CreateNewLOT.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.CreateNewLOT.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.CreateNewLOT.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.CreateNewLOT.FlatAppearance.BorderSize = 0;
            this.CreateNewLOT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CreateNewLOT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.CreateNewLOT.ForeColor = System.Drawing.Color.White;
            this.CreateNewLOT.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CreateNewLOT.Location = new System.Drawing.Point(177, 223);
            this.CreateNewLOT.Name = "CreateNewLOT";
            this.CreateNewLOT.Size = new System.Drawing.Size(99, 37);
            this.CreateNewLOT.TabIndex = 166;
            this.CreateNewLOT.Text = "Thêm mới";
            this.CreateNewLOT.TextColor = System.Drawing.Color.White;
            this.CreateNewLOT.UseVisualStyleBackColor = false;
            this.CreateNewLOT.Click += new System.EventHandler(this.AddNew);
            // 
            // ExpireDate
            // 
            this.ExpireDate._ReadOnlyBackColor = System.Drawing.Color.White;
            this.ExpireDate._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.ExpireDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExpireDate.BackColor = System.Drawing.Color.White;
            this.ExpireDate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ExpireDate.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.ExpireDate.BorderRadius = 8;
            this.ExpireDate.BorderSize = 1;
            this.ExpireDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExpireDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ExpireDate.Location = new System.Drawing.Point(177, 171);
            this.ExpireDate.Margin = new System.Windows.Forms.Padding(4);
            this.ExpireDate.Multiline = false;
            this.ExpireDate.Name = "ExpireDate";
            this.ExpireDate.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.ExpireDate.PasswordChar = false;
            this.ExpireDate.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.ExpireDate.PlaceholderText = "";
            this.ExpireDate.ReadOnly = true;
            this.ExpireDate.Size = new System.Drawing.Size(275, 35);
            this.ExpireDate.TabIndex = 161;
            this.ExpireDate.TabStop = false;
            this.ExpireDate.UnderlinedStyle = false;
            // 
            // MaufDate
            // 
            this.MaufDate._ReadOnlyBackColor = System.Drawing.Color.White;
            this.MaufDate._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.MaufDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MaufDate.BackColor = System.Drawing.Color.White;
            this.MaufDate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.MaufDate.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.MaufDate.BorderRadius = 8;
            this.MaufDate.BorderSize = 1;
            this.MaufDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaufDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.MaufDate.Location = new System.Drawing.Point(177, 107);
            this.MaufDate.Margin = new System.Windows.Forms.Padding(4);
            this.MaufDate.Multiline = false;
            this.MaufDate.Name = "MaufDate";
            this.MaufDate.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.MaufDate.PasswordChar = false;
            this.MaufDate.PlaceholderColor = System.Drawing.Color.Thistle;
            this.MaufDate.PlaceholderText = "";
            this.MaufDate.ReadOnly = true;
            this.MaufDate.Size = new System.Drawing.Size(275, 35);
            this.MaufDate.TabIndex = 160;
            this.MaufDate.TabStop = false;
            this.MaufDate.UnderlinedStyle = false;
            // 
            // frmBatchInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 272);
            this.Controls.Add(this.CreateNewLOT);
            this.Controls.Add(this.Close);
            this.Controls.Add(this.saveJobNuti);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ExpireDate);
            this.Controls.Add(this.MaufDate);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.LOTNumberCombo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmBatchInfo";
            this.Text = "frmBatchInfo";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox LOTNumberCombo;
        private DesignUI.CuzUI.CuzButton saveJobNuti;
        private DesignUI.CuzUI.CuzButton Close;
        private DesignUI.CuzUI.CuzButton CreateNewLOT;
        private DesignUI.CuzUI.CuzTextBox ExpireDate;
        private DesignUI.CuzUI.CuzTextBox MaufDate;
    }
}