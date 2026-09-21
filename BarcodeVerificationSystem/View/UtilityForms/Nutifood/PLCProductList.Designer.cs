namespace BarcodeVerificationSystem.View.UtilityForms.Nutifood
{
    partial class PLCProductList
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PLCProductList));
            this.flowLayoutPanelSlots = new System.Windows.Forms.FlowLayoutPanel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.groupBoxSlots = new System.Windows.Forms.GroupBox();
            this.CameraIcon = new System.Windows.Forms.PictureBox();
            this.SensorIcon = new System.Windows.Forms.PictureBox();
            this.DirectionIcon = new System.Windows.Forms.PictureBox();
            this.groupBoxLog = new System.Windows.Forms.GroupBox();
            this.panelButtonContainer = new System.Windows.Forms.Panel();
            this.chkAutoReload = new System.Windows.Forms.CheckBox();
            this.numAutoReloadInterval = new System.Windows.Forms.NumericUpDown();
            this.btnSendSlot = new DesignUI.CuzUI.CuzButton();
            this.btnReloadUI = new DesignUI.CuzUI.CuzButton();
            this.iconMenuItem1 = new FontAwesome.Sharp.IconMenuItem();
            this.iconToolStripButton1 = new FontAwesome.Sharp.IconToolStripButton();
            this.iconSplitButton1 = new FontAwesome.Sharp.IconSplitButton();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.iconToolStripButton2 = new FontAwesome.Sharp.IconToolStripButton();
            this.groupBoxSlots.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CameraIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SensorIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DirectionIcon)).BeginInit();
            this.groupBoxLog.SuspendLayout();
            this.panelButtonContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoReloadInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanelSlots
            // 
            this.flowLayoutPanelSlots.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelSlots.Location = new System.Drawing.Point(7, 83);
            this.flowLayoutPanelSlots.Name = "flowLayoutPanelSlots";
            this.flowLayoutPanelSlots.Padding = new System.Windows.Forms.Padding(15);
            this.flowLayoutPanelSlots.Size = new System.Drawing.Size(1328, 75);
            this.flowLayoutPanelSlots.TabIndex = 0;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(3, 16);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(1340, 61);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // groupBoxSlots
            // 
            this.groupBoxSlots.Controls.Add(this.CameraIcon);
            this.groupBoxSlots.Controls.Add(this.SensorIcon);
            this.groupBoxSlots.Controls.Add(this.DirectionIcon);
            this.groupBoxSlots.Controls.Add(this.flowLayoutPanelSlots);
            this.groupBoxSlots.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxSlots.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSlots.Name = "groupBoxSlots";
            this.groupBoxSlots.Size = new System.Drawing.Size(1346, 300);
            this.groupBoxSlots.TabIndex = 2;
            this.groupBoxSlots.TabStop = false;
            this.groupBoxSlots.Text = "Vị trí sản phẩm trên PLC";
            // 
            // CameraIcon
            // 
            this.CameraIcon.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.CameraIcon.Image = global::BarcodeVerificationSystem.Properties.Resources.Cognex_Small;
            this.CameraIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CameraIcon.InitialImage = global::BarcodeVerificationSystem.Properties.Resources.icons8_arrow_right;
            this.CameraIcon.Location = new System.Drawing.Point(22, 23);
            this.CameraIcon.Name = "CameraIcon";
            this.CameraIcon.Size = new System.Drawing.Size(55, 54);
            this.CameraIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.CameraIcon.TabIndex = 116;
            this.CameraIcon.TabStop = false;
            // 
            // SensorIcon
            // 
            this.SensorIcon.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.SensorIcon.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.SensorIcon.Image = global::BarcodeVerificationSystem.Properties.Resources.Sensor_Small_1;
            this.SensorIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SensorIcon.InitialImage = global::BarcodeVerificationSystem.Properties.Resources.icons8_arrow_right;
            this.SensorIcon.Location = new System.Drawing.Point(1286, 23);
            this.SensorIcon.Name = "SensorIcon";
            this.SensorIcon.Size = new System.Drawing.Size(48, 54);
            this.SensorIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.SensorIcon.TabIndex = 115;
            this.SensorIcon.TabStop = false;
            // 
            // DirectionIcon
            // 
            this.DirectionIcon.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.DirectionIcon.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_arrow_right;
            this.DirectionIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DirectionIcon.InitialImage = global::BarcodeVerificationSystem.Properties.Resources.icons8_arrow_right;
            this.DirectionIcon.Location = new System.Drawing.Point(655, 23);
            this.DirectionIcon.Name = "DirectionIcon";
            this.DirectionIcon.Size = new System.Drawing.Size(44, 47);
            this.DirectionIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.DirectionIcon.TabIndex = 114;
            this.DirectionIcon.TabStop = false;
            this.DirectionIcon.Click += new System.EventHandler(this.SyncLoading_Click);
            // 
            // groupBoxLog
            // 
            this.groupBoxLog.Controls.Add(this.richTextBox1);
            this.groupBoxLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxLog.Location = new System.Drawing.Point(0, 370);
            this.groupBoxLog.Name = "groupBoxLog";
            this.groupBoxLog.Size = new System.Drawing.Size(1346, 80);
            this.groupBoxLog.TabIndex = 3;
            this.groupBoxLog.TabStop = false;
            this.groupBoxLog.Text = "Log PLC phản hồi";
            // 
            // panelButtonContainer
            // 
            this.panelButtonContainer.Controls.Add(this.chkAutoReload);
            this.panelButtonContainer.Controls.Add(this.numAutoReloadInterval);
            this.panelButtonContainer.Controls.Add(this.btnSendSlot);
            this.panelButtonContainer.Controls.Add(this.btnReloadUI);
            this.panelButtonContainer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtonContainer.Location = new System.Drawing.Point(0, 300);
            this.panelButtonContainer.Name = "panelButtonContainer";
            this.panelButtonContainer.Size = new System.Drawing.Size(1346, 70);
            this.panelButtonContainer.TabIndex = 4;
            // 
            // chkAutoReload
            // 
            this.chkAutoReload.AutoSize = true;
            this.chkAutoReload.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkAutoReload.Location = new System.Drawing.Point(251, 25);
            this.chkAutoReload.Name = "chkAutoReload";
            this.chkAutoReload.Size = new System.Drawing.Size(116, 23);
            this.chkAutoReload.TabIndex = 140;
            this.chkAutoReload.Text = "Tự động (giây)";
            this.chkAutoReload.UseVisualStyleBackColor = true;
            this.chkAutoReload.CheckedChanged += new System.EventHandler(this.chkAutoReload_CheckedChanged);
            // 
            // numAutoReloadInterval
            // 
            this.numAutoReloadInterval.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.numAutoReloadInterval.Location = new System.Drawing.Point(165, 25);
            this.numAutoReloadInterval.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numAutoReloadInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numAutoReloadInterval.Name = "numAutoReloadInterval";
            this.numAutoReloadInterval.Size = new System.Drawing.Size(80, 25);
            this.numAutoReloadInterval.TabIndex = 139;
            this.numAutoReloadInterval.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numAutoReloadInterval.ValueChanged += new System.EventHandler(this.numAutoReloadInterval_ValueChanged);
            // 
            // btnSendSlot
            // 
            this.btnSendSlot._BorderColor = System.Drawing.Color.Silver;
            this.btnSendSlot._BorderRadius = 15;
            this.btnSendSlot._BorderSize = 0;
            this.btnSendSlot._GradientsButton = false;
            this.btnSendSlot._Text = "Lấy thùng ra";
            this.btnSendSlot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnSendSlot.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnSendSlot.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnSendSlot.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnSendSlot.FlatAppearance.BorderSize = 0;
            this.btnSendSlot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendSlot.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.btnSendSlot.ForeColor = System.Drawing.Color.White;
            this.btnSendSlot.Location = new System.Drawing.Point(1201, 9);
            this.btnSendSlot.Name = "btnSendSlot";
            this.btnSendSlot.Size = new System.Drawing.Size(133, 53);
            this.btnSendSlot.TabIndex = 137;
            this.btnSendSlot.Text = "Lấy thùng ra";
            this.btnSendSlot.TextColor = System.Drawing.Color.White;
            this.btnSendSlot.UseVisualStyleBackColor = false;
            this.btnSendSlot.Click += new System.EventHandler(this.btnSendSlot_Click);
            // 
            // btnReloadUI
            // 
            this.btnReloadUI._BorderColor = System.Drawing.Color.Silver;
            this.btnReloadUI._BorderRadius = 15;
            this.btnReloadUI._BorderSize = 0;
            this.btnReloadUI._GradientsButton = false;
            this.btnReloadUI._Text = "Cập nhật vị trí";
            this.btnReloadUI.BackColor = System.Drawing.Color.LightSeaGreen;
            this.btnReloadUI.BackgroundColor = System.Drawing.Color.LightSeaGreen;
            this.btnReloadUI.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnReloadUI.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnReloadUI.FlatAppearance.BorderSize = 0;
            this.btnReloadUI.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReloadUI.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.btnReloadUI.ForeColor = System.Drawing.Color.White;
            this.btnReloadUI.Location = new System.Drawing.Point(12, 9);
            this.btnReloadUI.Name = "btnReloadUI";
            this.btnReloadUI.Size = new System.Drawing.Size(133, 53);
            this.btnReloadUI.TabIndex = 136;
            this.btnReloadUI.Text = "Cập nhật vị trí";
            this.btnReloadUI.TextColor = System.Drawing.Color.White;
            this.btnReloadUI.UseVisualStyleBackColor = false;
            this.btnReloadUI.Click += new System.EventHandler(this.btnReloadUI_Click);
            // 
            // iconMenuItem1
            // 
            this.iconMenuItem1.IconChar = FontAwesome.Sharp.IconChar.None;
            this.iconMenuItem1.IconColor = System.Drawing.Color.Black;
            this.iconMenuItem1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconMenuItem1.Name = "iconMenuItem1";
            this.iconMenuItem1.Size = new System.Drawing.Size(32, 19);
            this.iconMenuItem1.Text = "iconMenuItem1";
            // 
            // iconToolStripButton1
            // 
            this.iconToolStripButton1.IconChar = FontAwesome.Sharp.IconChar.None;
            this.iconToolStripButton1.IconColor = System.Drawing.Color.Black;
            this.iconToolStripButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconToolStripButton1.Name = "iconToolStripButton1";
            this.iconToolStripButton1.Size = new System.Drawing.Size(23, 23);
            this.iconToolStripButton1.Text = "iconToolStripButton1";
            // 
            // iconSplitButton1
            // 
            this.iconSplitButton1.Flip = FontAwesome.Sharp.FlipOrientation.Normal;
            this.iconSplitButton1.IconChar = FontAwesome.Sharp.IconChar.None;
            this.iconSplitButton1.IconColor = System.Drawing.Color.Black;
            this.iconSplitButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconSplitButton1.IconSize = 48;
            this.iconSplitButton1.Name = "iconSplitButton1";
            this.iconSplitButton1.Rotation = 0D;
            this.iconSplitButton1.Size = new System.Drawing.Size(23, 23);
            this.iconSplitButton1.Text = "iconSplitButton1";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // iconToolStripButton2
            // 
            this.iconToolStripButton2.IconChar = FontAwesome.Sharp.IconChar.None;
            this.iconToolStripButton2.IconColor = System.Drawing.Color.Black;
            this.iconToolStripButton2.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconToolStripButton2.Name = "iconToolStripButton2";
            this.iconToolStripButton2.Size = new System.Drawing.Size(23, 23);
            this.iconToolStripButton2.Text = "iconToolStripButton2";
            // 
            // PLCProductList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1346, 450);
            this.Controls.Add(this.panelButtonContainer);
            this.Controls.Add(this.groupBoxLog);
            this.Controls.Add(this.groupBoxSlots);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(816, 489);
            this.Name = "PLCProductList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Danh sách sản phẩm";
            this.groupBoxSlots.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CameraIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SensorIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DirectionIcon)).EndInit();
            this.groupBoxLog.ResumeLayout(false);
            this.panelButtonContainer.ResumeLayout(false);
            this.panelButtonContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoReloadInterval)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSlots;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.GroupBox groupBoxSlots;
        private System.Windows.Forms.GroupBox groupBoxLog;
        private System.Windows.Forms.Panel panelButtonContainer;   // ← NEW
        private FontAwesome.Sharp.IconMenuItem iconMenuItem1;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton1;
        private FontAwesome.Sharp.IconSplitButton iconSplitButton1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton2;
        private System.Windows.Forms.PictureBox DirectionIcon;
        private System.Windows.Forms.PictureBox CameraIcon;
        private System.Windows.Forms.PictureBox SensorIcon;
        private DesignUI.CuzUI.CuzButton btnSendSlot;
        private DesignUI.CuzUI.CuzButton btnReloadUI;
        private System.Windows.Forms.NumericUpDown numAutoReloadInterval;
        private System.Windows.Forms.CheckBox chkAutoReload;
    }
}