namespace BarcodeVerificationSystem.View.UcSettings
{
    partial class ucProductionTHTrueMilkSetting
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.labelApi = new System.Windows.Forms.Label();
            this.comboBoxRLinkId = new System.Windows.Forms.ComboBox();
            this.lineIdLabel = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.manufacturingRad = new System.Windows.Forms.RadioButton();
            this.productionMode = new System.Windows.Forms.Label();
            this.dispatchingRad = new System.Windows.Forms.RadioButton();
            this.groupBoxDatabaseSettings = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblDbType = new System.Windows.Forms.Label();
            this.cmbPreviewTable = new System.Windows.Forms.ComboBox();
            this.cmbDatabaseType = new System.Windows.Forms.ComboBox();
            this.lblDbServer = new System.Windows.Forms.Label();
            this.txtDbServerName = new System.Windows.Forms.TextBox();
            this.lblDbPort = new System.Windows.Forms.Label();
            this.txtDbPort = new System.Windows.Forms.TextBox();
            this.lblDbDatabase = new System.Windows.Forms.Label();
            this.txtDbDatabaseName = new System.Windows.Forms.TextBox();
            this.btnPreviewTable = new System.Windows.Forms.Button();
            this.lblDbUser = new System.Windows.Forms.Label();
            this.lblPreviewTable = new System.Windows.Forms.Label();
            this.txtDbUsername = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lblDbStatus = new System.Windows.Forms.Label();
            this.lblDbPassword = new System.Windows.Forms.Label();
            this.txtDbPassword = new System.Windows.Forms.TextBox();
            this.btnTestDbConnection = new System.Windows.Forms.Button();
            this.btnCheckConnectDB = new System.Windows.Forms.Button();
            this.lblDbConnectionStatus = new System.Windows.Forms.Label();
            this.onlineProductionSettings = new System.Windows.Forms.Panel();
            this.LineId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dataIncrease = new System.Windows.Forms.Label();
            this.numIncreasedData = new System.Windows.Forms.NumericUpDown();
            this.lineNameLabel = new System.Windows.Forms.Label();
            this.maskData = new System.Windows.Forms.CheckBox();
            this.lineName = new System.Windows.Forms.TextBox();
            this.dataDisplay = new System.Windows.Forms.Label();
            this.groupBoxLineSettings = new System.Windows.Forms.GroupBox();
            this.lblpAddressLocal = new System.Windows.Forms.Label();
            this.btnGetInfoLine = new DesignUI.CuzUI.CuzButton();
            this.lblStatusLine = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblDiskStorage = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label111 = new System.Windows.Forms.Label();
            this.lbl = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblApiStatus = new System.Windows.Forms.Label();
            this.btnCheckHealth = new System.Windows.Forms.Button();
            this.apiTextbox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblOperatingMode = new System.Windows.Forms.Label();
            this.radModeProductQr = new System.Windows.Forms.RadioButton();
            this.radModeAutoRefresh = new System.Windows.Forms.RadioButton();
            this.radModeBatchQr = new System.Windows.Forms.RadioButton();
            this.HideFunctions = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.RLinkNamescombox = new System.Windows.Forms.ComboBox();
            this.btnSetLine = new System.Windows.Forms.Button();
            this.factoryCodeLabel = new System.Windows.Forms.Label();
            this.FactoryCodeCombox = new System.Windows.Forms.ComboBox();
            this.btnUnActiveLine = new System.Windows.Forms.Button();
            this.ckbCheckStart = new System.Windows.Forms.CheckBox();
            this.panel2.SuspendLayout();
            this.groupBoxDatabaseSettings.SuspendLayout();
            this.onlineProductionSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIncreasedData)).BeginInit();
            this.groupBoxLineSettings.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelApi
            // 
            this.labelApi.AutoSize = true;
            this.labelApi.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelApi.Location = new System.Drawing.Point(11, 32);
            this.labelApi.Name = "labelApi";
            this.labelApi.Size = new System.Drawing.Size(57, 20);
            this.labelApi.TabIndex = 39;
            this.labelApi.Text = "Api url:";
            // 
            // comboBoxRLinkId
            // 
            this.comboBoxRLinkId.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxRLinkId.FormattingEnabled = true;
            this.comboBoxRLinkId.Items.AddRange(new object[] {
            "R1",
            "R2",
            "R3",
            "R4"});
            this.comboBoxRLinkId.Location = new System.Drawing.Point(333, 47);
            this.comboBoxRLinkId.Name = "comboBoxRLinkId";
            this.comboBoxRLinkId.Size = new System.Drawing.Size(103, 28);
            this.comboBoxRLinkId.TabIndex = 40;
            this.comboBoxRLinkId.Visible = false;
            // 
            // lineIdLabel
            // 
            this.lineIdLabel.AutoSize = true;
            this.lineIdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lineIdLabel.Location = new System.Drawing.Point(22, 232);
            this.lineIdLabel.Name = "lineIdLabel";
            this.lineIdLabel.Size = new System.Drawing.Size(61, 20);
            this.lineIdLabel.TabIndex = 41;
            this.lineIdLabel.Text = "Line Id:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.manufacturingRad);
            this.panel2.Controls.Add(this.productionMode);
            this.panel2.Controls.Add(this.dispatchingRad);
            this.panel2.Location = new System.Drawing.Point(680, 571);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(103, 45);
            this.panel2.TabIndex = 85;
            this.panel2.Visible = false;
            // 
            // manufacturingRad
            // 
            this.manufacturingRad.AutoSize = true;
            this.manufacturingRad.Location = new System.Drawing.Point(144, 19);
            this.manufacturingRad.Name = "manufacturingRad";
            this.manufacturingRad.Size = new System.Drawing.Size(93, 17);
            this.manufacturingRad.TabIndex = 44;
            this.manufacturingRad.TabStop = true;
            this.manufacturingRad.Text = "Manufacturing";
            this.manufacturingRad.UseVisualStyleBackColor = true;
            // 
            // productionMode
            // 
            this.productionMode.AutoSize = true;
            this.productionMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.productionMode.Location = new System.Drawing.Point(5, 21);
            this.productionMode.Name = "productionMode";
            this.productionMode.Size = new System.Drawing.Size(133, 20);
            this.productionMode.TabIndex = 43;
            this.productionMode.Text = "Production Mode:";
            // 
            // dispatchingRad
            // 
            this.dispatchingRad.AutoSize = true;
            this.dispatchingRad.Location = new System.Drawing.Point(307, 19);
            this.dispatchingRad.Name = "dispatchingRad";
            this.dispatchingRad.Size = new System.Drawing.Size(81, 17);
            this.dispatchingRad.TabIndex = 45;
            this.dispatchingRad.TabStop = true;
            this.dispatchingRad.Text = "Dispatching";
            this.dispatchingRad.UseVisualStyleBackColor = true;
            // 
            // groupBoxDatabaseSettings
            // 
            this.groupBoxDatabaseSettings.AutoSize = true;
            this.groupBoxDatabaseSettings.Controls.Add(this.label4);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbType);
            this.groupBoxDatabaseSettings.Controls.Add(this.cmbPreviewTable);
            this.groupBoxDatabaseSettings.Controls.Add(this.cmbDatabaseType);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbServer);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbServerName);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbPort);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbPort);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbDatabase);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbDatabaseName);
            this.groupBoxDatabaseSettings.Controls.Add(this.btnPreviewTable);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbUser);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblPreviewTable);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbUsername);
            this.groupBoxDatabaseSettings.Controls.Add(this.label5);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbStatus);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbPassword);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbPassword);
            this.groupBoxDatabaseSettings.Controls.Add(this.btnTestDbConnection);
            this.groupBoxDatabaseSettings.Controls.Add(this.btnCheckConnectDB);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbConnectionStatus);
            this.groupBoxDatabaseSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.groupBoxDatabaseSettings.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBoxDatabaseSettings.Location = new System.Drawing.Point(458, 4);
            this.groupBoxDatabaseSettings.Name = "groupBoxDatabaseSettings";
            this.groupBoxDatabaseSettings.Size = new System.Drawing.Size(518, 495);
            this.groupBoxDatabaseSettings.TabIndex = 84;
            this.groupBoxDatabaseSettings.TabStop = false;
            this.groupBoxDatabaseSettings.Text = "Cơ sở dữ liệu";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(141, 363);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 20);
            this.label4.TabIndex = 20;
            this.label4.Text = "0 MB";
            // 
            // lblDbType
            // 
            this.lblDbType.AutoSize = true;
            this.lblDbType.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbType.Location = new System.Drawing.Point(6, 31);
            this.lblDbType.Name = "lblDbType";
            this.lblDbType.Size = new System.Drawing.Size(90, 20);
            this.lblDbType.TabIndex = 0;
            this.lblDbType.Text = "Loại CSDL:";
            // 
            // cmbPreviewTable
            // 
            this.cmbPreviewTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPreviewTable.Enabled = false;
            this.cmbPreviewTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.cmbPreviewTable.FormattingEnabled = true;
            this.cmbPreviewTable.Location = new System.Drawing.Point(143, 252);
            this.cmbPreviewTable.Name = "cmbPreviewTable";
            this.cmbPreviewTable.Size = new System.Drawing.Size(233, 28);
            this.cmbPreviewTable.TabIndex = 18;
            // 
            // cmbDatabaseType
            // 
            this.cmbDatabaseType.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.cmbDatabaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDatabaseType.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.cmbDatabaseType.FormattingEnabled = true;
            this.cmbDatabaseType.Location = new System.Drawing.Point(144, 24);
            this.cmbDatabaseType.Name = "cmbDatabaseType";
            this.cmbDatabaseType.Size = new System.Drawing.Size(110, 28);
            this.cmbDatabaseType.TabIndex = 1;
            // 
            // lblDbServer
            // 
            this.lblDbServer.AutoSize = true;
            this.lblDbServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbServer.Location = new System.Drawing.Point(6, 75);
            this.lblDbServer.Name = "lblDbServer";
            this.lblDbServer.Size = new System.Drawing.Size(72, 20);
            this.lblDbServer.TabIndex = 2;
            this.lblDbServer.Text = "Máy chủ:";
            // 
            // txtDbServerName
            // 
            this.txtDbServerName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbServerName.Location = new System.Drawing.Point(143, 70);
            this.txtDbServerName.Name = "txtDbServerName";
            this.txtDbServerName.Size = new System.Drawing.Size(348, 26);
            this.txtDbServerName.TabIndex = 3;
            // 
            // lblDbPort
            // 
            this.lblDbPort.AutoSize = true;
            this.lblDbPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbPort.Location = new System.Drawing.Point(328, 31);
            this.lblDbPort.Name = "lblDbPort";
            this.lblDbPort.Size = new System.Drawing.Size(51, 20);
            this.lblDbPort.TabIndex = 4;
            this.lblDbPort.Text = "Cổng:";
            // 
            // txtDbPort
            // 
            this.txtDbPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbPort.Location = new System.Drawing.Point(382, 25);
            this.txtDbPort.Name = "txtDbPort";
            this.txtDbPort.Size = new System.Drawing.Size(110, 26);
            this.txtDbPort.TabIndex = 5;
            // 
            // lblDbDatabase
            // 
            this.lblDbDatabase.AutoSize = true;
            this.lblDbDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbDatabase.Location = new System.Drawing.Point(6, 119);
            this.lblDbDatabase.Name = "lblDbDatabase";
            this.lblDbDatabase.Size = new System.Drawing.Size(114, 20);
            this.lblDbDatabase.TabIndex = 6;
            this.lblDbDatabase.Text = "Tên Database:";
            // 
            // txtDbDatabaseName
            // 
            this.txtDbDatabaseName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbDatabaseName.Location = new System.Drawing.Point(143, 114);
            this.txtDbDatabaseName.Name = "txtDbDatabaseName";
            this.txtDbDatabaseName.Size = new System.Drawing.Size(348, 26);
            this.txtDbDatabaseName.TabIndex = 7;
            // 
            // btnPreviewTable
            // 
            this.btnPreviewTable.Enabled = false;
            this.btnPreviewTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPreviewTable.Location = new System.Drawing.Point(382, 250);
            this.btnPreviewTable.Name = "btnPreviewTable";
            this.btnPreviewTable.Size = new System.Drawing.Size(109, 30);
            this.btnPreviewTable.TabIndex = 19;
            this.btnPreviewTable.Text = "Xem dữ liệu";
            this.btnPreviewTable.UseVisualStyleBackColor = true;
            // 
            // lblDbUser
            // 
            this.lblDbUser.AutoSize = true;
            this.lblDbUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbUser.Location = new System.Drawing.Point(5, 166);
            this.lblDbUser.Name = "lblDbUser";
            this.lblDbUser.Size = new System.Drawing.Size(82, 20);
            this.lblDbUser.TabIndex = 10;
            this.lblDbUser.Text = "Tài khoản:";
            // 
            // lblPreviewTable
            // 
            this.lblPreviewTable.AutoSize = true;
            this.lblPreviewTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblPreviewTable.Location = new System.Drawing.Point(5, 260);
            this.lblPreviewTable.Name = "lblPreviewTable";
            this.lblPreviewTable.Size = new System.Drawing.Size(86, 20);
            this.lblPreviewTable.TabIndex = 17;
            this.lblPreviewTable.Text = "Xem bảng:";
            // 
            // txtDbUsername
            // 
            this.txtDbUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbUsername.Location = new System.Drawing.Point(143, 158);
            this.txtDbUsername.Name = "txtDbUsername";
            this.txtDbUsername.Size = new System.Drawing.Size(348, 26);
            this.txtDbUsername.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.label5.Location = new System.Drawing.Point(5, 363);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 20);
            this.label5.TabIndex = 12;
            this.label5.Text = "Dung lượng:";
            // 
            // lblDbStatus
            // 
            this.lblDbStatus.AutoSize = true;
            this.lblDbStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbStatus.Location = new System.Drawing.Point(5, 312);
            this.lblDbStatus.Name = "lblDbStatus";
            this.lblDbStatus.Size = new System.Drawing.Size(84, 20);
            this.lblDbStatus.TabIndex = 12;
            this.lblDbStatus.Text = "Trạng thái:";
            // 
            // lblDbPassword
            // 
            this.lblDbPassword.AutoSize = true;
            this.lblDbPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbPassword.Location = new System.Drawing.Point(5, 213);
            this.lblDbPassword.Name = "lblDbPassword";
            this.lblDbPassword.Size = new System.Drawing.Size(79, 20);
            this.lblDbPassword.TabIndex = 12;
            this.lblDbPassword.Text = "Mật khẩu:";
            // 
            // txtDbPassword
            // 
            this.txtDbPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbPassword.Location = new System.Drawing.Point(143, 202);
            this.txtDbPassword.Name = "txtDbPassword";
            this.txtDbPassword.PasswordChar = '*';
            this.txtDbPassword.Size = new System.Drawing.Size(348, 26);
            this.txtDbPassword.TabIndex = 13;
            // 
            // btnTestDbConnection
            // 
            this.btnTestDbConnection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(80)))));
            this.btnTestDbConnection.FlatAppearance.BorderSize = 0;
            this.btnTestDbConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestDbConnection.ForeColor = System.Drawing.Color.White;
            this.btnTestDbConnection.Location = new System.Drawing.Point(332, 393);
            this.btnTestDbConnection.Name = "btnTestDbConnection";
            this.btnTestDbConnection.Size = new System.Drawing.Size(160, 32);
            this.btnTestDbConnection.TabIndex = 14;
            this.btnTestDbConnection.Text = "Ngắt kết nối";
            this.btnTestDbConnection.UseVisualStyleBackColor = false;
            // 
            // btnCheckConnectDB
            // 
            this.btnCheckConnectDB.BackColor = System.Drawing.SystemColors.Control;
            this.btnCheckConnectDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnCheckConnectDB.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.btnCheckConnectDB.Location = new System.Drawing.Point(11, 393);
            this.btnCheckConnectDB.Name = "btnCheckConnectDB";
            this.btnCheckConnectDB.Size = new System.Drawing.Size(160, 32);
            this.btnCheckConnectDB.TabIndex = 17;
            this.btnCheckConnectDB.Text = "Kiểm tra CSDL";
            this.btnCheckConnectDB.UseVisualStyleBackColor = false;
            // 
            // lblDbConnectionStatus
            // 
            this.lblDbConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbConnectionStatus.Location = new System.Drawing.Point(141, 304);
            this.lblDbConnectionStatus.Name = "lblDbConnectionStatus";
            this.lblDbConnectionStatus.Size = new System.Drawing.Size(348, 47);
            this.lblDbConnectionStatus.TabIndex = 15;
            // 
            // onlineProductionSettings
            // 
            this.onlineProductionSettings.Controls.Add(this.LineId);
            this.onlineProductionSettings.Controls.Add(this.label2);
            this.onlineProductionSettings.Controls.Add(this.dataIncrease);
            this.onlineProductionSettings.Controls.Add(this.numIncreasedData);
            this.onlineProductionSettings.Controls.Add(this.lineNameLabel);
            this.onlineProductionSettings.Controls.Add(this.maskData);
            this.onlineProductionSettings.Controls.Add(this.lineName);
            this.onlineProductionSettings.Controls.Add(this.dataDisplay);
            this.onlineProductionSettings.Controls.Add(this.comboBoxRLinkId);
            this.onlineProductionSettings.Controls.Add(this.lineIdLabel);
            this.onlineProductionSettings.Location = new System.Drawing.Point(440, 575);
            this.onlineProductionSettings.Name = "onlineProductionSettings";
            this.onlineProductionSettings.Size = new System.Drawing.Size(213, 35);
            this.onlineProductionSettings.TabIndex = 51;
            this.onlineProductionSettings.Visible = false;
            // 
            // LineId
            // 
            this.LineId.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LineId.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LineId.Location = new System.Drawing.Point(64, -8);
            this.LineId.Name = "LineId";
            this.LineId.Size = new System.Drawing.Size(103, 26);
            this.LineId.TabIndex = 77;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(307, 166);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 20);
            this.label2.TabIndex = 46;
            this.label2.Text = "%";
            // 
            // dataIncrease
            // 
            this.dataIncrease.AutoSize = true;
            this.dataIncrease.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataIncrease.Location = new System.Drawing.Point(22, 168);
            this.dataIncrease.Name = "dataIncrease";
            this.dataIncrease.Size = new System.Drawing.Size(141, 20);
            this.dataIncrease.TabIndex = 46;
            this.dataIncrease.Text = "Data increased by:";
            // 
            // numIncreasedData
            // 
            this.numIncreasedData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numIncreasedData.Location = new System.Drawing.Point(198, 163);
            this.numIncreasedData.Name = "numIncreasedData";
            this.numIncreasedData.Size = new System.Drawing.Size(103, 26);
            this.numIncreasedData.TabIndex = 48;
            // 
            // lineNameLabel
            // 
            this.lineNameLabel.AutoSize = true;
            this.lineNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lineNameLabel.Location = new System.Drawing.Point(22, 296);
            this.lineNameLabel.Name = "lineNameLabel";
            this.lineNameLabel.Size = new System.Drawing.Size(89, 20);
            this.lineNameLabel.TabIndex = 74;
            this.lineNameLabel.Text = "Line Name:";
            // 
            // maskData
            // 
            this.maskData.AutoSize = true;
            this.maskData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.maskData.Location = new System.Drawing.Point(198, 97);
            this.maskData.Name = "maskData";
            this.maskData.Size = new System.Drawing.Size(198, 24);
            this.maskData.TabIndex = 70;
            this.maskData.Text = "Partially Mask Values";
            this.maskData.UseVisualStyleBackColor = true;
            // 
            // lineName
            // 
            this.lineName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lineName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lineName.Location = new System.Drawing.Point(41, 132);
            this.lineName.Name = "lineName";
            this.lineName.Size = new System.Drawing.Size(103, 26);
            this.lineName.TabIndex = 73;
            // 
            // dataDisplay
            // 
            this.dataDisplay.AutoSize = true;
            this.dataDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataDisplay.Location = new System.Drawing.Point(22, 101);
            this.dataDisplay.Name = "dataDisplay";
            this.dataDisplay.Size = new System.Drawing.Size(100, 20);
            this.dataDisplay.TabIndex = 71;
            this.dataDisplay.Text = "Data display:";
            this.dataDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxLineSettings
            // 
            this.groupBoxLineSettings.Controls.Add(this.lblpAddressLocal);
            this.groupBoxLineSettings.Controls.Add(this.btnGetInfoLine);
            this.groupBoxLineSettings.Controls.Add(this.lblStatusLine);
            this.groupBoxLineSettings.Controls.Add(this.groupBox1);
            this.groupBoxLineSettings.Controls.Add(this.label111);
            this.groupBoxLineSettings.Controls.Add(this.lbl);
            this.groupBoxLineSettings.Controls.Add(this.label3);
            this.groupBoxLineSettings.Controls.Add(this.lblApiStatus);
            this.groupBoxLineSettings.Controls.Add(this.btnCheckHealth);
            this.groupBoxLineSettings.Controls.Add(this.apiTextbox);
            this.groupBoxLineSettings.Controls.Add(this.panel1);
            this.groupBoxLineSettings.Controls.Add(this.HideFunctions);
            this.groupBoxLineSettings.Controls.Add(this.labelApi);
            this.groupBoxLineSettings.Controls.Add(this.label1);
            this.groupBoxLineSettings.Controls.Add(this.RLinkNamescombox);
            this.groupBoxLineSettings.Controls.Add(this.btnSetLine);
            this.groupBoxLineSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.groupBoxLineSettings.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBoxLineSettings.Location = new System.Drawing.Point(3, 3);
            this.groupBoxLineSettings.Name = "groupBoxLineSettings";
            this.groupBoxLineSettings.Size = new System.Drawing.Size(448, 496);
            this.groupBoxLineSettings.TabIndex = 83;
            this.groupBoxLineSettings.TabStop = false;
            this.groupBoxLineSettings.Text = "Cài đặt thông tin Line";
            this.groupBoxLineSettings.Enter += new System.EventHandler(this.groupBoxLineSettings_Enter);
            // 
            // lblpAddressLocal
            // 
            this.lblpAddressLocal.AutoSize = true;
            this.lblpAddressLocal.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblpAddressLocal.Location = new System.Drawing.Point(104, 263);
            this.lblpAddressLocal.Name = "lblpAddressLocal";
            this.lblpAddressLocal.Size = new System.Drawing.Size(35, 20);
            this.lblpAddressLocal.TabIndex = 145;
            this.lblpAddressLocal.Text = "N/A";
            // 
            // btnGetInfoLine
            // 
            this.btnGetInfoLine._BorderColor = System.Drawing.Color.Silver;
            this.btnGetInfoLine._BorderRadius = 10;
            this.btnGetInfoLine._BorderSize = 1;
            this.btnGetInfoLine._GradientsButton = false;
            this.btnGetInfoLine._Text = "";
            this.btnGetInfoLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetInfoLine.BackColor = System.Drawing.Color.White;
            this.btnGetInfoLine.BackgroundColor = System.Drawing.Color.White;
            this.btnGetInfoLine.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnGetInfoLine.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnGetInfoLine.FlatAppearance.BorderSize = 0;
            this.btnGetInfoLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGetInfoLine.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnGetInfoLine.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnGetInfoLine.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_refresh_161;
            this.btnGetInfoLine.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnGetInfoLine.Location = new System.Drawing.Point(357, 116);
            this.btnGetInfoLine.Name = "btnGetInfoLine";
            this.btnGetInfoLine.Size = new System.Drawing.Size(76, 31);
            this.btnGetInfoLine.TabIndex = 144;
            this.btnGetInfoLine.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnGetInfoLine.UseVisualStyleBackColor = false;
            // 
            // lblStatusLine
            // 
            this.lblStatusLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblStatusLine.Location = new System.Drawing.Point(104, 206);
            this.lblStatusLine.Name = "lblStatusLine";
            this.lblStatusLine.Size = new System.Drawing.Size(320, 46);
            this.lblStatusLine.TabIndex = 101;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblDiskStorage);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.groupBox1.Location = new System.Drawing.Point(15, 313);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(415, 153);
            this.groupBox1.TabIndex = 100;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Dung lượng ổ cứng";
            // 
            // lblDiskStorage
            // 
            this.lblDiskStorage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDiskStorage.Location = new System.Drawing.Point(98, 33);
            this.lblDiskStorage.Name = "lblDiskStorage";
            this.lblDiskStorage.Size = new System.Drawing.Size(311, 102);
            this.lblDiskStorage.TabIndex = 2;
            this.lblDiskStorage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblDiskStorage.Click += new System.EventHandler(this.label5_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_disk_usage_48;
            this.pictureBox1.Location = new System.Drawing.Point(23, 53);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(57, 46);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label111
            // 
            this.label111.AutoSize = true;
            this.label111.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.label111.Location = new System.Drawing.Point(11, 263);
            this.label111.Name = "label111";
            this.label111.Size = new System.Drawing.Size(62, 20);
            this.label111.TabIndex = 98;
            this.label111.Text = "IP Line:";
            // 
            // lbl
            // 
            this.lbl.AutoSize = true;
            this.lbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lbl.Location = new System.Drawing.Point(11, 206);
            this.lbl.Name = "lbl";
            this.lbl.Size = new System.Drawing.Size(84, 20);
            this.lbl.TabIndex = 98;
            this.lbl.Text = "Trạng thái:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.label3.Location = new System.Drawing.Point(11, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 20);
            this.label3.TabIndex = 98;
            this.label3.Text = "Trạng thái:";
            // 
            // lblApiStatus
            // 
            this.lblApiStatus.AutoSize = true;
            this.lblApiStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblApiStatus.Location = new System.Drawing.Point(112, 73);
            this.lblApiStatus.Name = "lblApiStatus";
            this.lblApiStatus.Size = new System.Drawing.Size(119, 20);
            this.lblApiStatus.TabIndex = 97;
            this.lblApiStatus.Text = "Chưa có kết nối";
            // 
            // btnCheckHealth
            // 
            this.btnCheckHealth.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.btnCheckHealth.Location = new System.Drawing.Point(357, 25);
            this.btnCheckHealth.Name = "btnCheckHealth";
            this.btnCheckHealth.Size = new System.Drawing.Size(78, 30);
            this.btnCheckHealth.TabIndex = 96;
            this.btnCheckHealth.Text = "Kiểm tra";
            this.btnCheckHealth.UseVisualStyleBackColor = true;
            this.btnCheckHealth.Click += new System.EventHandler(this.btnCheckHealth_Click);
            // 
            // apiTextbox
            // 
            this.apiTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.apiTextbox.Location = new System.Drawing.Point(116, 26);
            this.apiTextbox.Name = "apiTextbox";
            this.apiTextbox.Size = new System.Drawing.Size(235, 26);
            this.apiTextbox.TabIndex = 95;
            this.apiTextbox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblOperatingMode);
            this.panel1.Controls.Add(this.radModeProductQr);
            this.panel1.Controls.Add(this.radModeAutoRefresh);
            this.panel1.Controls.Add(this.radModeBatchQr);
            this.panel1.Location = new System.Drawing.Point(15, 524);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(232, 65);
            this.panel1.TabIndex = 94;
            this.panel1.Visible = false;
            // 
            // lblOperatingMode
            // 
            this.lblOperatingMode.AutoSize = true;
            this.lblOperatingMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOperatingMode.Location = new System.Drawing.Point(3, 10);
            this.lblOperatingMode.Name = "lblOperatingMode";
            this.lblOperatingMode.Size = new System.Drawing.Size(133, 20);
            this.lblOperatingMode.TabIndex = 90;
            this.lblOperatingMode.Text = "Chế độ vận hành:";
            // 
            // radModeProductQr
            // 
            this.radModeProductQr.AutoSize = true;
            this.radModeProductQr.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radModeProductQr.Location = new System.Drawing.Point(7, 83);
            this.radModeProductQr.Name = "radModeProductQr";
            this.radModeProductQr.Size = new System.Drawing.Size(197, 22);
            this.radModeProductQr.TabIndex = 93;
            this.radModeProductQr.TabStop = true;
            this.radModeProductQr.Text = "1 Sản phẩm = 1 QR Code";
            this.radModeProductQr.UseVisualStyleBackColor = true;
            // 
            // radModeAutoRefresh
            // 
            this.radModeAutoRefresh.AutoSize = true;
            this.radModeAutoRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radModeAutoRefresh.Location = new System.Drawing.Point(7, 61);
            this.radModeAutoRefresh.Name = "radModeAutoRefresh";
            this.radModeAutoRefresh.Size = new System.Drawing.Size(229, 22);
            this.radModeAutoRefresh.TabIndex = 92;
            this.radModeAutoRefresh.TabStop = true;
            this.radModeAutoRefresh.Text = "Tự động làm mới theo thời gian";
            this.radModeAutoRefresh.UseVisualStyleBackColor = true;
            // 
            // radModeBatchQr
            // 
            this.radModeBatchQr.AutoSize = true;
            this.radModeBatchQr.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radModeBatchQr.Location = new System.Drawing.Point(7, 33);
            this.radModeBatchQr.Name = "radModeBatchQr";
            this.radModeBatchQr.Size = new System.Drawing.Size(168, 22);
            this.radModeBatchQr.TabIndex = 91;
            this.radModeBatchQr.TabStop = true;
            this.radModeBatchQr.Text = "1 Batch = 1 QR Code";
            this.radModeBatchQr.UseVisualStyleBackColor = true;
            // 
            // HideFunctions
            // 
            this.HideFunctions.AutoSize = true;
            this.HideFunctions.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideFunctions.Location = new System.Drawing.Point(272, 536);
            this.HideFunctions.Name = "HideFunctions";
            this.HideFunctions.Size = new System.Drawing.Size(132, 24);
            this.HideFunctions.TabIndex = 82;
            this.HideFunctions.Text = "Tính năng ẩn";
            this.HideFunctions.UseVisualStyleBackColor = true;
            this.HideFunctions.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 123);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 20);
            this.label1.TabIndex = 78;
            this.label1.Text = "Tên thiết bị:";
            // 
            // RLinkNamescombox
            // 
            this.RLinkNamescombox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RLinkNamescombox.FormattingEnabled = true;
            this.RLinkNamescombox.Items.AddRange(new object[] {
            "R1",
            "R2",
            "R3",
            "R4"});
            this.RLinkNamescombox.Location = new System.Drawing.Point(116, 117);
            this.RLinkNamescombox.Name = "RLinkNamescombox";
            this.RLinkNamescombox.Size = new System.Drawing.Size(235, 28);
            this.RLinkNamescombox.TabIndex = 78;
            this.RLinkNamescombox.SelectedIndexChanged += new System.EventHandler(this.RLinkNamescombox_SelectedIndexChanged);
            // 
            // btnSetLine
            // 
            this.btnSetLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(80)))));
            this.btnSetLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetLine.ForeColor = System.Drawing.Color.White;
            this.btnSetLine.Location = new System.Drawing.Point(272, 162);
            this.btnSetLine.Name = "btnSetLine";
            this.btnSetLine.Size = new System.Drawing.Size(160, 32);
            this.btnSetLine.TabIndex = 14;
            this.btnSetLine.Text = "Set Line";
            this.btnSetLine.UseVisualStyleBackColor = false;
            this.btnSetLine.Visible = false;
            // 
            // factoryCodeLabel
            // 
            this.factoryCodeLabel.AutoSize = true;
            this.factoryCodeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.factoryCodeLabel.Location = new System.Drawing.Point(16, 534);
            this.factoryCodeLabel.Name = "factoryCodeLabel";
            this.factoryCodeLabel.Size = new System.Drawing.Size(99, 20);
            this.factoryCodeLabel.TabIndex = 72;
            this.factoryCodeLabel.Text = "Mã nhà máy:";
            this.factoryCodeLabel.Visible = false;
            // 
            // FactoryCodeCombox
            // 
            this.FactoryCodeCombox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FactoryCodeCombox.FormattingEnabled = true;
            this.FactoryCodeCombox.Items.AddRange(new object[] {
            "R1",
            "R2",
            "R3",
            "R4"});
            this.FactoryCodeCombox.Location = new System.Drawing.Point(121, 526);
            this.FactoryCodeCombox.Name = "FactoryCodeCombox";
            this.FactoryCodeCombox.Size = new System.Drawing.Size(317, 28);
            this.FactoryCodeCombox.TabIndex = 81;
            this.FactoryCodeCombox.Visible = false;
            // 
            // btnUnActiveLine
            // 
            this.btnUnActiveLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUnActiveLine.Location = new System.Drawing.Point(301, 226);
            this.btnUnActiveLine.Name = "btnUnActiveLine";
            this.btnUnActiveLine.Size = new System.Drawing.Size(129, 32);
            this.btnUnActiveLine.TabIndex = 14;
            this.btnUnActiveLine.Text = "Hủy";
            this.btnUnActiveLine.UseVisualStyleBackColor = true;
            this.btnUnActiveLine.Visible = false;
            this.btnUnActiveLine.Click += new System.EventHandler(this.btnUnActiveLine_Click);
            // 
            // ckbCheckStart
            // 
            this.ckbCheckStart.AutoSize = true;
            this.ckbCheckStart.Location = new System.Drawing.Point(18, 505);
            this.ckbCheckStart.Name = "ckbCheckStart";
            this.ckbCheckStart.Size = new System.Drawing.Size(134, 17);
            this.ckbCheckStart.TabIndex = 86;
            this.ckbCheckStart.Text = "Kiểm tra tất cả khi start";
            this.ckbCheckStart.UseVisualStyleBackColor = true;
            this.ckbCheckStart.Visible = false;
            // 
            // ucProductionTHTrueMilkSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ckbCheckStart);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.groupBoxDatabaseSettings);
            this.Controls.Add(this.onlineProductionSettings);
            this.Controls.Add(this.groupBoxLineSettings);
            this.Controls.Add(this.FactoryCodeCombox);
            this.Controls.Add(this.factoryCodeLabel);
            this.Name = "ucProductionTHTrueMilkSetting";
            this.Size = new System.Drawing.Size(990, 644);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBoxDatabaseSettings.ResumeLayout(false);
            this.groupBoxDatabaseSettings.PerformLayout();
            this.onlineProductionSettings.ResumeLayout(false);
            this.onlineProductionSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIncreasedData)).EndInit();
            this.groupBoxLineSettings.ResumeLayout(false);
            this.groupBoxLineSettings.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelApi;
        private System.Windows.Forms.ComboBox comboBoxRLinkId;
        private System.Windows.Forms.Label lineIdLabel;
        private System.Windows.Forms.RadioButton dispatchingRad;
        private System.Windows.Forms.RadioButton manufacturingRad;
        private System.Windows.Forms.Label productionMode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label dataIncrease;
        private System.Windows.Forms.NumericUpDown numIncreasedData;
        private System.Windows.Forms.Label dataDisplay;
        private System.Windows.Forms.CheckBox maskData;
        private System.Windows.Forms.Label lineNameLabel;
        private System.Windows.Forms.TextBox lineName;
        private System.Windows.Forms.TextBox LineId;
        private System.Windows.Forms.Panel onlineProductionSettings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox RLinkNamescombox;
        private System.Windows.Forms.CheckBox HideFunctions;
        private System.Windows.Forms.GroupBox groupBoxLineSettings;
        private System.Windows.Forms.GroupBox groupBoxDatabaseSettings;
        private System.Windows.Forms.Label lblDbType;
        private System.Windows.Forms.ComboBox cmbDatabaseType;
        private System.Windows.Forms.Label lblDbServer;
        private System.Windows.Forms.TextBox txtDbServerName;
        private System.Windows.Forms.Label lblDbPort;
        private System.Windows.Forms.TextBox txtDbPort;
        private System.Windows.Forms.Label lblDbDatabase;
        private System.Windows.Forms.TextBox txtDbDatabaseName;
        private System.Windows.Forms.Label lblDbUser;
        private System.Windows.Forms.TextBox txtDbUsername;
        private System.Windows.Forms.Label lblDbPassword;
        private System.Windows.Forms.TextBox txtDbPassword;
        private System.Windows.Forms.Button btnTestDbConnection;
        private System.Windows.Forms.Label lblDbConnectionStatus;
        private System.Windows.Forms.Label lblDbStatus;
        private System.Windows.Forms.Label lblPreviewTable;
        private System.Windows.Forms.ComboBox cmbPreviewTable;
        private System.Windows.Forms.Button btnPreviewTable;
        // ── Chế độ vận hành ──────────────────────────────────────────────
        private System.Windows.Forms.Label lblOperatingMode;
        private System.Windows.Forms.RadioButton radModeBatchQr;
        private System.Windows.Forms.RadioButton radModeAutoRefresh;
        private System.Windows.Forms.RadioButton radModeProductQr;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox apiTextbox;
        private System.Windows.Forms.Button btnCheckHealth;
        private System.Windows.Forms.Label lblApiStatus;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSetLine;
        private System.Windows.Forms.Button btnUnActiveLine;
        private System.Windows.Forms.Button btnCheckConnectDB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbl;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblDiskStorage;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblStatusLine;
        private DesignUI.CuzUI.CuzButton btnGetInfoLine;
        private System.Windows.Forms.Label lblpAddressLocal;
        private System.Windows.Forms.Label label111;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label factoryCodeLabel;
        private System.Windows.Forms.ComboBox FactoryCodeCombox;
        private System.Windows.Forms.CheckBox ckbCheckStart;
    }
}