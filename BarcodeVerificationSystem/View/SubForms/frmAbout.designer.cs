namespace BarcodeVerificationSystem.View
{
    partial class FrmAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAbout));
            this.panel1 = new System.Windows.Forms.Panel();
            this.grbGeneralInfo = new System.Windows.Forms.GroupBox();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.grbReleaseNote = new System.Windows.Forms.GroupBox();
            this.rchReleaseNote = new System.Windows.Forms.RichTextBox();
            this.pnlDrag = new System.Windows.Forms.Panel();
            this.cuzControlBox1 = new DesignUI.CuzUI.CuzControlBox();
            this.lblFormName = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cuzDropShadow1 = new DesignUI.CuzUI.CuzDropShadow();
            this.panel1.SuspendLayout();
            this.grbGeneralInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.grbReleaseNote.SuspendLayout();
            this.pnlDrag.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.grbGeneralInfo);
            this.panel1.Controls.Add(this.grbReleaseNote);
            this.panel1.Controls.Add(this.pnlDrag);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(648, 556);
            this.panel1.TabIndex = 0;
            // 
            // grbGeneralInfo
            // 
            this.grbGeneralInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grbGeneralInfo.Controls.Add(this.picLogo);
            this.grbGeneralInfo.Controls.Add(this.webBrowser1);
            this.grbGeneralInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbGeneralInfo.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.grbGeneralInfo.Location = new System.Drawing.Point(38, 43);
            this.grbGeneralInfo.Name = "grbGeneralInfo";
            this.grbGeneralInfo.Size = new System.Drawing.Size(592, 207);
            this.grbGeneralInfo.TabIndex = 57;
            this.grbGeneralInfo.TabStop = false;
            this.grbGeneralInfo.Text = "General info";
            // 
            // picLogo
            // 
            this.picLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picLogo.Image = global::BarcodeVerificationSystem.Properties.Resources.logo1;
            this.picLogo.InitialImage = null;
            this.picLogo.Location = new System.Drawing.Point(442, 14);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(147, 52);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 6;
            this.picLogo.TabStop = false;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(4, 19);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScrollBarsEnabled = false;
            this.webBrowser1.Size = new System.Drawing.Size(427, 172);
            this.webBrowser1.TabIndex = 5;
            // 
            // grbReleaseNote
            // 
            this.grbReleaseNote.Controls.Add(this.rchReleaseNote);
            this.grbReleaseNote.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbReleaseNote.Location = new System.Drawing.Point(38, 268);
            this.grbReleaseNote.Name = "grbReleaseNote";
            this.grbReleaseNote.Size = new System.Drawing.Size(589, 270);
            this.grbReleaseNote.TabIndex = 58;
            this.grbReleaseNote.TabStop = false;
            this.grbReleaseNote.Text = "Release note";
            // 
            // rchReleaseNote
            // 
            this.rchReleaseNote.BackColor = System.Drawing.Color.White;
            this.rchReleaseNote.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rchReleaseNote.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rchReleaseNote.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.rchReleaseNote.Location = new System.Drawing.Point(8, 18);
            this.rchReleaseNote.Name = "rchReleaseNote";
            this.rchReleaseNote.ReadOnly = true;
            this.rchReleaseNote.Size = new System.Drawing.Size(575, 245);
            this.rchReleaseNote.TabIndex = 0;
            this.rchReleaseNote.Text = "";
            // 
            // pnlDrag
            // 
            this.pnlDrag.BackColor = System.Drawing.Color.White;
            this.pnlDrag.Controls.Add(this.cuzControlBox1);
            this.pnlDrag.Controls.Add(this.lblFormName);
            this.pnlDrag.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDrag.Location = new System.Drawing.Point(20, 0);
            this.pnlDrag.Name = "pnlDrag";
            this.pnlDrag.Size = new System.Drawing.Size(628, 35);
            this.pnlDrag.TabIndex = 60;
            // 
            // cuzControlBox1
            // 
            this.cuzControlBox1._BackColor = System.Drawing.Color.White;
            this.cuzControlBox1._CloseHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(17)))), ((int)(((byte)(35)))));
            this.cuzControlBox1._ControlBoxType = DesignUI.CuzUI.ControlBoxType.CloseBox;
            this.cuzControlBox1._IconColor = DesignUI.CuzUI.IconColor.Black;
            this.cuzControlBox1._IconSize = new System.Drawing.Size(25, 25);
            this.cuzControlBox1.BackColor = System.Drawing.Color.White;
            this.cuzControlBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.cuzControlBox1.FlatAppearance.BorderSize = 0;
            this.cuzControlBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cuzControlBox1.Image = ((System.Drawing.Image)(resources.GetObject("cuzControlBox1.Image")));
            this.cuzControlBox1.Location = new System.Drawing.Point(583, 0);
            this.cuzControlBox1.Name = "cuzControlBox1";
            this.cuzControlBox1.Size = new System.Drawing.Size(45, 35);
            this.cuzControlBox1.TabIndex = 2;
            this.cuzControlBox1.UseVisualStyleBackColor = false;
            // 
            // lblFormName
            // 
            this.lblFormName.AutoSize = true;
            this.lblFormName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormName.Location = new System.Drawing.Point(3, 6);
            this.lblFormName.Name = "lblFormName";
            this.lblFormName.Size = new System.Drawing.Size(52, 20);
            this.lblFormName.TabIndex = 1;
            this.lblFormName.Text = "About";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(20, 556);
            this.panel2.TabIndex = 59;
            // 
            // cuzDropShadow1
            // 
            this.cuzDropShadow1.TargetControl = this;
            // 
            // frmAbout
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(648, 556);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 280);
            this.Name = "frmAbout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About";
            this.panel1.ResumeLayout(false);
            this.grbGeneralInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.grbReleaseNote.ResumeLayout(false);
            this.pnlDrag.ResumeLayout(false);
            this.pnlDrag.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox grbGeneralInfo;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.GroupBox grbReleaseNote;
        private System.Windows.Forms.RichTextBox rchReleaseNote;
        private System.Windows.Forms.Panel pnlDrag;
        private System.Windows.Forms.Label lblFormName;
        private System.Windows.Forms.Panel panel2;
        private DesignUI.CuzUI.CuzDropShadow cuzDropShadow1;
        private DesignUI.CuzUI.CuzControlBox cuzControlBox1;
    }
}