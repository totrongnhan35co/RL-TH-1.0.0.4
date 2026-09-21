namespace BarcodeVerificationSystem.View
{
    partial class FrmWarningWindowsUpdate
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
            this.btnContinue = new System.Windows.Forms.Button();
            this.btnOpenWindowsUpdate = new System.Windows.Forms.Button();
            this.lblUpdateStatus = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnContinue
            // 
            this.btnContinue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnContinue.Location = new System.Drawing.Point(287, 150);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(136, 32);
            this.btnContinue.TabIndex = 7;
            this.btnContinue.Text = "Continue";
            this.btnContinue.UseVisualStyleBackColor = true;
            // 
            // btnOpenWindowsUpdate
            // 
            this.btnOpenWindowsUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenWindowsUpdate.Location = new System.Drawing.Point(145, 150);
            this.btnOpenWindowsUpdate.Name = "btnOpenWindowsUpdate";
            this.btnOpenWindowsUpdate.Size = new System.Drawing.Size(136, 32);
            this.btnOpenWindowsUpdate.TabIndex = 6;
            this.btnOpenWindowsUpdate.Text = "Open Windows Update";
            this.btnOpenWindowsUpdate.UseVisualStyleBackColor = true;
            // 
            // lblUpdateStatus
            // 
            this.lblUpdateStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdateStatus.Location = new System.Drawing.Point(83, 12);
            this.lblUpdateStatus.Name = "lblUpdateStatus";
            this.lblUpdateStatus.Size = new System.Drawing.Size(340, 130);
            this.lblUpdateStatus.TabIndex = 5;
            this.lblUpdateStatus.Text = "Windows Update Status";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::BarcodeVerificationSystem.Properties.Resources.Warning128;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(66, 66);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // frmWarningWindowsUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 194);
            this.Controls.Add(this.btnContinue);
            this.Controls.Add(this.btnOpenWindowsUpdate);
            this.Controls.Add(this.lblUpdateStatus);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmWarningWindowsUpdate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Windows Update Warning";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.Button btnOpenWindowsUpdate;
        private System.Windows.Forms.Label lblUpdateStatus;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

