namespace BarcodeVerificationSystem.View
{
    partial class frmDatabase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

            this.headerPanel = new System.Windows.Forms.Panel();
            this.LoadDatabaseLabel = new System.Windows.Forms.Label();
            this.cmbDatabaseType = new System.Windows.Forms.ComboBox();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtDatabaseName = new System.Windows.Forms.TextBox();
            this.cmbTableName = new System.Windows.Forms.ComboBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblDatabaseType = new System.Windows.Forms.Label();
            this.lblServerName = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblDatabaseName = new System.Windows.Forms.Label();
            this.lblTableName = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.headerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.Teal;
            this.headerPanel.Controls.Add(this.LoadDatabaseLabel);
            this.headerPanel.Location = new System.Drawing.Point(10, 10);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(466, 50);
            this.headerPanel.TabIndex = 0;
            // 
            // LoadDatabaseLabel
            // 
            this.LoadDatabaseLabel.AutoSize = true;
            this.LoadDatabaseLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.LoadDatabaseLabel.ForeColor = System.Drawing.Color.White;
            this.LoadDatabaseLabel.Location = new System.Drawing.Point(10, 12);
            this.LoadDatabaseLabel.Name = "LoadDatabaseLabel";
            this.LoadDatabaseLabel.Size = new System.Drawing.Size(142, 25);
            this.LoadDatabaseLabel.TabIndex = 0;
            this.LoadDatabaseLabel.Text = "Load Database";
            // 
            // cmbDatabaseType
            // 
            this.cmbDatabaseType.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.cmbDatabaseType.Items.AddRange(new object[] {
            "CSV",
            "SQL",
            "MySQL",
            "SQLite"});
            this.cmbDatabaseType.Location = new System.Drawing.Point(20, 91);
            this.cmbDatabaseType.Name = "cmbDatabaseType";
            this.cmbDatabaseType.Size = new System.Drawing.Size(450, 28);
            this.cmbDatabaseType.TabIndex = 1;
            // 
            // txtServerName
            // 
            this.txtServerName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtServerName.Location = new System.Drawing.Point(20, 140);
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.Size = new System.Drawing.Size(450, 27);
            this.txtServerName.TabIndex = 2;
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtPort.Location = new System.Drawing.Point(20, 192);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(450, 27);
            this.txtPort.TabIndex = 3;
            // 
            // txtUsername
            // 
            this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtUsername.Location = new System.Drawing.Point(20, 245);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(450, 27);
            this.txtUsername.TabIndex = 4;
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtPassword.Location = new System.Drawing.Point(20, 298);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(450, 27);
            this.txtPassword.TabIndex = 5;
            // 
            // txtDatabaseName
            // 
            this.txtDatabaseName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtDatabaseName.Location = new System.Drawing.Point(20, 350);
            this.txtDatabaseName.Name = "txtDatabaseName";
            this.txtDatabaseName.Size = new System.Drawing.Size(450, 27);
            this.txtDatabaseName.TabIndex = 6;
            // 
            // cmbTableName
            // 
            this.cmbTableName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.cmbTableName.Location = new System.Drawing.Point(20, 400);
            this.cmbTableName.Name = "cmbTableName";
            this.cmbTableName.Size = new System.Drawing.Size(450, 28);
            this.cmbTableName.TabIndex = 7;
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.Green;
            this.btnConnect.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnConnect.ForeColor = System.Drawing.Color.White;
            this.btnConnect.Location = new System.Drawing.Point(20, 450);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(100, 35);
            this.btnConnect.TabIndex = 8;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.BackColor = System.Drawing.Color.LightGreen;
            this.btnPreview.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnPreview.Location = new System.Drawing.Point(135, 450);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(100, 35);
            this.btnPreview.TabIndex = 9;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = false;
            this.btnPreview.Click += new System.EventHandler(this.btnReview_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.LightCoral;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(370, 450);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 35);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblDatabaseType
            // 
            this.lblDatabaseType.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblDatabaseType.Location = new System.Drawing.Point(20, 70);
            this.lblDatabaseType.Name = "lblDatabaseType";
            this.lblDatabaseType.Size = new System.Drawing.Size(200, 25);
            this.lblDatabaseType.TabIndex = 11;
            this.lblDatabaseType.Text = "Database Type:";
            // 
            // lblServerName
            // 
            this.lblServerName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblServerName.Location = new System.Drawing.Point(20, 118);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Size = new System.Drawing.Size(200, 25);
            this.lblServerName.TabIndex = 12;
            this.lblServerName.Text = "Server Name:";
            // 
            // lblPort
            // 
            this.lblPort.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblPort.Location = new System.Drawing.Point(20, 170);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(200, 25);
            this.lblPort.TabIndex = 13;
            this.lblPort.Text = "Port:";
            // 
            // lblUsername
            // 
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblUsername.Location = new System.Drawing.Point(20, 222);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(200, 25);
            this.lblUsername.TabIndex = 14;
            this.lblUsername.Text = "Username:";
            // 
            // lblPassword
            // 
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblPassword.Location = new System.Drawing.Point(20, 275);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(200, 25);
            this.lblPassword.TabIndex = 15;
            this.lblPassword.Text = "Password:";
            // 
            // lblDatabaseName
            // 
            this.lblDatabaseName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblDatabaseName.Location = new System.Drawing.Point(20, 327);
            this.lblDatabaseName.Name = "lblDatabaseName";
            this.lblDatabaseName.Size = new System.Drawing.Size(200, 25);
            this.lblDatabaseName.TabIndex = 16;
            this.lblDatabaseName.Text = "Database Name:";
            // 
            // lblTableName
            // 
            this.lblTableName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblTableName.Location = new System.Drawing.Point(20, 380);
            this.lblTableName.Name = "lblTableName";
            this.lblTableName.Size = new System.Drawing.Size(200, 24);
            this.lblTableName.TabIndex = 17;
            this.lblTableName.Text = "Table Name:";
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.LightGreen;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnSave.Location = new System.Drawing.Point(253, 450);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 35);
            this.btnSave.TabIndex = 18;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click_Click);
            // 
            // frmDatabase
            // 
            this.ClientSize = new System.Drawing.Size(488, 500);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.cmbDatabaseType);
            this.Controls.Add(this.txtServerName);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtDatabaseName);
            this.Controls.Add(this.cmbTableName);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblDatabaseType);
            this.Controls.Add(this.lblServerName);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblDatabaseName);
            this.Controls.Add(this.lblTableName);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmDatabase";
            this.Text = "Load Database";
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label LoadDatabaseLabel;
        private System.Windows.Forms.ComboBox cmbDatabaseType;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtDatabaseName;
        private System.Windows.Forms.ComboBox cmbDataType;
        private System.Windows.Forms.ComboBox cmbTableName; // Changed from txtTableName to cmbTableName
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblDatabaseType;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblDatabaseName;
        private System.Windows.Forms.Label lblDataType;
        private System.Windows.Forms.Label lblTableName;
        private System.Windows.Forms.Button btnSave;
    }
}