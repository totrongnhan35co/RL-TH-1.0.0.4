namespace BarcodeVerificationSystem.View.THTrueMilkUI.Manufacturing
{
    partial class FrmIncompleteJobsNutri
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmIncompleteJobsNutri));
            this._dgvIncompleteJobs = new System.Windows.Forms.DataGridView();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.closeBtn = new DesignUI.CuzUI.CuzButton();
            this.StopSyncData = new DesignUI.CuzUI.CuzButton();
            this.SyncDataBtn = new DesignUI.CuzUI.CuzButton();
            this.editJobBtn = new DesignUI.CuzUI.CuzButton();
            this.picDatabaseLoading = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this._dgvIncompleteJobs)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDatabaseLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // _dgvIncompleteJobs
            // 
            this._dgvIncompleteJobs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dgvIncompleteJobs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dgvIncompleteJobs.Location = new System.Drawing.Point(0, 36);
            this._dgvIncompleteJobs.Name = "_dgvIncompleteJobs";
            this._dgvIncompleteJobs.Size = new System.Drawing.Size(1202, 346);
            this._dgvIncompleteJobs.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblStatus.Location = new System.Drawing.Point(11, 10);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(968, 20);
            this.lblStatus.TabIndex = 3;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.lblTitle);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlTop.Size = new System.Drawing.Size(1202, 36);
            this.pnlTop.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblTitle.Location = new System.Drawing.Point(8, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(231, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Các công việc chưa hoàn thành";
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.closeBtn);
            this.pnlBottom.Controls.Add(this.StopSyncData);
            this.pnlBottom.Controls.Add(this.SyncDataBtn);
            this.pnlBottom.Controls.Add(this.editJobBtn);
            this.pnlBottom.Controls.Add(this.lblStatus);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 382);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlBottom.Size = new System.Drawing.Size(1202, 80);
            this.pnlBottom.TabIndex = 2;
            // 
            // closeBtn
            // 
            this.closeBtn._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.closeBtn._BorderRadius = 0;
            this.closeBtn._BorderSize = 1;
            this.closeBtn._GradientsButton = false;
            this.closeBtn._Text = "Đóng";
            this.closeBtn.BackColor = System.Drawing.Color.Red;
            this.closeBtn.BackgroundColor = System.Drawing.Color.Red;
            this.closeBtn.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.closeBtn.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.closeBtn.FlatAppearance.BorderSize = 0;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.closeBtn.ForeColor = System.Drawing.Color.White;
            this.closeBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.closeBtn.Location = new System.Drawing.Point(1092, 31);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(87, 37);
            this.closeBtn.TabIndex = 137;
            this.closeBtn.Text = "Đóng";
            this.closeBtn.TextColor = System.Drawing.Color.White;
            this.closeBtn.UseVisualStyleBackColor = false;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // StopSyncData
            // 
            this.StopSyncData._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.StopSyncData._BorderRadius = 0;
            this.StopSyncData._BorderSize = 1;
            this.StopSyncData._GradientsButton = false;
            this.StopSyncData._Text = "Dừng đồng bộ";
            this.StopSyncData.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.StopSyncData.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.StopSyncData.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.StopSyncData.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.StopSyncData.FlatAppearance.BorderSize = 0;
            this.StopSyncData.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopSyncData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.StopSyncData.ForeColor = System.Drawing.Color.White;
            this.StopSyncData.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StopSyncData.Location = new System.Drawing.Point(916, 31);
            this.StopSyncData.Name = "StopSyncData";
            this.StopSyncData.Size = new System.Drawing.Size(156, 37);
            this.StopSyncData.TabIndex = 136;
            this.StopSyncData.Text = "Dừng đồng bộ";
            this.StopSyncData.TextColor = System.Drawing.Color.White;
            this.StopSyncData.UseVisualStyleBackColor = false;
            this.StopSyncData.Click += new System.EventHandler(this.StopSyncData_Click);
            // 
            // SyncDataBtn
            // 
            this.SyncDataBtn._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.SyncDataBtn._BorderRadius = 0;
            this.SyncDataBtn._BorderSize = 1;
            this.SyncDataBtn._GradientsButton = false;
            this.SyncDataBtn._Text = "Đồng Bộ";
            this.SyncDataBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(109)))), ((int)(((byte)(70)))));
            this.SyncDataBtn.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(109)))), ((int)(((byte)(70)))));
            this.SyncDataBtn.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.SyncDataBtn.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.SyncDataBtn.FlatAppearance.BorderSize = 0;
            this.SyncDataBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SyncDataBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.SyncDataBtn.ForeColor = System.Drawing.Color.White;
            this.SyncDataBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SyncDataBtn.Location = new System.Drawing.Point(760, 31);
            this.SyncDataBtn.Name = "SyncDataBtn";
            this.SyncDataBtn.Size = new System.Drawing.Size(127, 37);
            this.SyncDataBtn.TabIndex = 133;
            this.SyncDataBtn.Text = "Đồng Bộ";
            this.SyncDataBtn.TextColor = System.Drawing.Color.White;
            this.SyncDataBtn.UseVisualStyleBackColor = false;
            this.SyncDataBtn.Click += new System.EventHandler(this.SyncDataBtn_Click);
            // 
            // editJobBtn
            // 
            this.editJobBtn._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.editJobBtn._BorderRadius = 0;
            this.editJobBtn._BorderSize = 1;
            this.editJobBtn._GradientsButton = false;
            this.editJobBtn._Text = "Chỉnh sửa";
            this.editJobBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.editJobBtn.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.editJobBtn.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.editJobBtn.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.editJobBtn.FlatAppearance.BorderSize = 0;
            this.editJobBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.editJobBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.editJobBtn.ForeColor = System.Drawing.Color.White;
            this.editJobBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.editJobBtn.Location = new System.Drawing.Point(24, 31);
            this.editJobBtn.Name = "editJobBtn";
            this.editJobBtn.Size = new System.Drawing.Size(127, 37);
            this.editJobBtn.TabIndex = 138;
            this.editJobBtn.Text = "Chỉnh sửa";
            this.editJobBtn.TextColor = System.Drawing.Color.White;
            this.editJobBtn.UseVisualStyleBackColor = false;
            this.editJobBtn.Click += new System.EventHandler(this.editJobBtn_Click);
            // 
            // picDatabaseLoading
            // 
            this.picDatabaseLoading.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.picDatabaseLoading.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_loading_2681;
            this.picDatabaseLoading.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.picDatabaseLoading.Location = new System.Drawing.Point(551, 154);
            this.picDatabaseLoading.Name = "picDatabaseLoading";
            this.picDatabaseLoading.Size = new System.Drawing.Size(92, 62);
            this.picDatabaseLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picDatabaseLoading.TabIndex = 113;
            this.picDatabaseLoading.TabStop = false;
            this.picDatabaseLoading.Visible = false;
            // 
            // FrmIncompleteJobsNutri
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1202, 462);
            this.Controls.Add(this.picDatabaseLoading);
            this.Controls.Add(this._dgvIncompleteJobs);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.4F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 350);
            this.Name = "FrmIncompleteJobsNutri";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Công việc chưa hoàn thành";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmIncompleteJobsNutri_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this._dgvIncompleteJobs)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picDatabaseLoading)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.PictureBox picDatabaseLoading;
        private DesignUI.CuzUI.CuzButton SyncDataBtn;
        private DesignUI.CuzUI.CuzButton StopSyncData;
        private DesignUI.CuzUI.CuzButton closeBtn;
        private DesignUI.CuzUI.CuzButton editJobBtn;
    }
}
