namespace BarcodeVerificationSystem.View.OtherProjects.THMilkUI
{
    partial class ucProductionTHSetting
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.apiTextbox = new System.Windows.Forms.TextBox();
            this.labelApi = new System.Windows.Forms.Label();
            this.comboBoxRLinkId = new System.Windows.Forms.ComboBox();
            this.lineIdLabel = new System.Windows.Forms.Label();
            this.groupBoxProductionSettings = new System.Windows.Forms.GroupBox();
            this.RLinkNamescombox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.onlineProductionSettings = new System.Windows.Forms.Panel();
            this.HideFunctions = new System.Windows.Forms.CheckBox();
            this.LineId = new System.Windows.Forms.TextBox();
            this.FactoryCodeCombox = new System.Windows.Forms.ComboBox();
            this.productionMode = new System.Windows.Forms.Label();
            this.manufacturingRad = new System.Windows.Forms.RadioButton();
            this.dispatchingRad = new System.Windows.Forms.RadioButton();
            this.factoryCodeLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dataIncrease = new System.Windows.Forms.Label();
            this.numIncreasedData = new System.Windows.Forms.NumericUpDown();
            this.lineNameLabel = new System.Windows.Forms.Label();
            this.maskData = new System.Windows.Forms.CheckBox();
            this.lineName = new System.Windows.Forms.TextBox();
            this.dataDisplay = new System.Windows.Forms.Label();
            this.groupBoxDatabaseSettings = new System.Windows.Forms.GroupBox();
            this.lblDbType = new System.Windows.Forms.Label();
            this.cmbDatabaseType = new System.Windows.Forms.ComboBox();
            this.lblDbServer = new System.Windows.Forms.Label();
            this.txtDbServerName = new System.Windows.Forms.TextBox();
            this.lblDbPort = new System.Windows.Forms.Label();
            this.txtDbPort = new System.Windows.Forms.TextBox();
            this.lblDbDatabase = new System.Windows.Forms.Label();
            this.txtDbDatabaseName = new System.Windows.Forms.TextBox();
            this.lblDbTable = new System.Windows.Forms.Label();
            this.txtDbTableName = new System.Windows.Forms.TextBox();
            this.lblDbUser = new System.Windows.Forms.Label();
            this.txtDbUsername = new System.Windows.Forms.TextBox();
            this.lblDbPassword = new System.Windows.Forms.Label();
            this.txtDbPassword = new System.Windows.Forms.TextBox();
            this.btnTestDbConnection = new System.Windows.Forms.Button();
            this.lblDbConnectionStatus = new System.Windows.Forms.Label();
            this.groupBoxProductionSettings.SuspendLayout();
            this.onlineProductionSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIncreasedData)).BeginInit();
            this.groupBoxDatabaseSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // apiTextbox
            // 
            this.apiTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.apiTextbox.Location = new System.Drawing.Point(160, 50);
            this.apiTextbox.Name = "apiTextbox";
            this.apiTextbox.Size = new System.Drawing.Size(260, 26);
            this.apiTextbox.TabIndex = 0;
            // 
            // labelApi
            // 
            this.labelApi.AutoSize = true;
            this.labelApi.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelApi.Location = new System.Drawing.Point(15, 50);
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
            this.comboBoxRLinkId.Location = new System.Drawing.Point(336, 158);
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
            // groupBoxProductionSettings
            // 
            this.groupBoxProductionSettings.Controls.Add(this.RLinkNamescombox);
            this.groupBoxProductionSettings.Controls.Add(this.label1);
            this.groupBoxProductionSettings.Controls.Add(this.labelApi);
            this.groupBoxProductionSettings.Controls.Add(this.apiTextbox);
            this.groupBoxProductionSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxProductionSettings.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBoxProductionSettings.Location = new System.Drawing.Point(3, 3);
            this.groupBoxProductionSettings.Name = "groupBoxProductionSettings";
            this.groupBoxProductionSettings.Size = new System.Drawing.Size(440, 476);
            this.groupBoxProductionSettings.TabIndex = 50;
            this.groupBoxProductionSettings.TabStop = false;
            this.groupBoxProductionSettings.Text = "Cài đặt Line";
            // 
            // RLinkNamescombox
            // 
            this.RLinkNamescombox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RLinkNamescombox.FormattingEnabled = true;
            this.RLinkNamescombox.Location = new System.Drawing.Point(160, 100);
            this.RLinkNamescombox.Name = "RLinkNamescombox";
            this.RLinkNamescombox.Size = new System.Drawing.Size(110, 28);
            this.RLinkNamescombox.TabIndex = 78;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(15, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 20);
            this.label1.TabIndex = 78;
            this.label1.Text = "Tên thiết bị:";
            // 
            // onlineProductionSettings
            // 
            this.onlineProductionSettings.Controls.Add(this.HideFunctions);
            this.onlineProductionSettings.Controls.Add(this.LineId);
            this.onlineProductionSettings.Controls.Add(this.FactoryCodeCombox);
            this.onlineProductionSettings.Controls.Add(this.productionMode);
            this.onlineProductionSettings.Controls.Add(this.manufacturingRad);
            this.onlineProductionSettings.Controls.Add(this.dispatchingRad);
            this.onlineProductionSettings.Controls.Add(this.factoryCodeLabel);
            this.onlineProductionSettings.Controls.Add(this.label2);
            this.onlineProductionSettings.Controls.Add(this.dataIncrease);
            this.onlineProductionSettings.Controls.Add(this.numIncreasedData);
            this.onlineProductionSettings.Controls.Add(this.lineNameLabel);
            this.onlineProductionSettings.Controls.Add(this.maskData);
            this.onlineProductionSettings.Controls.Add(this.lineName);
            this.onlineProductionSettings.Controls.Add(this.dataDisplay);
            this.onlineProductionSettings.Controls.Add(this.comboBoxRLinkId);
            this.onlineProductionSettings.Controls.Add(this.lineIdLabel);
            this.onlineProductionSettings.Location = new System.Drawing.Point(966, 464);
            this.onlineProductionSettings.Name = "onlineProductionSettings";
            this.onlineProductionSettings.Size = new System.Drawing.Size(83, 42);
            this.onlineProductionSettings.TabIndex = 51;
            this.onlineProductionSettings.Visible = false;
            // 
            // HideFunctions
            // 
            this.HideFunctions.AutoSize = true;
            this.HideFunctions.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideFunctions.Location = new System.Drawing.Point(223, 14);
            this.HideFunctions.Name = "HideFunctions";
            this.HideFunctions.Size = new System.Drawing.Size(132, 24);
            this.HideFunctions.TabIndex = 82;
            this.HideFunctions.Text = "Tính năng ẩn";
            this.HideFunctions.UseVisualStyleBackColor = true;
            // 
            // LineId
            // 
            this.LineId.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LineId.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LineId.Location = new System.Drawing.Point(9, 36);
            this.LineId.Name = "LineId";
            this.LineId.Size = new System.Drawing.Size(103, 26);
            this.LineId.TabIndex = 77;
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
            this.FactoryCodeCombox.Location = new System.Drawing.Point(345, 61);
            this.FactoryCodeCombox.Name = "FactoryCodeCombox";
            this.FactoryCodeCombox.Size = new System.Drawing.Size(160, 28);
            this.FactoryCodeCombox.TabIndex = 81;
            // 
            // productionMode
            // 
            this.productionMode.AutoSize = true;
            this.productionMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.productionMode.Location = new System.Drawing.Point(30, 18);
            this.productionMode.Name = "productionMode";
            this.productionMode.Size = new System.Drawing.Size(133, 20);
            this.productionMode.TabIndex = 43;
            this.productionMode.Text = "Production Mode:";
            // 
            // manufacturingRad
            // 
            this.manufacturingRad.AutoSize = true;
            this.manufacturingRad.Location = new System.Drawing.Point(159, 51);
            this.manufacturingRad.Name = "manufacturingRad";
            this.manufacturingRad.Size = new System.Drawing.Size(93, 17);
            this.manufacturingRad.TabIndex = 44;
            this.manufacturingRad.TabStop = true;
            this.manufacturingRad.Text = "Manufacturing";
            this.manufacturingRad.UseVisualStyleBackColor = true;
            // 
            // dispatchingRad
            // 
            this.dispatchingRad.AutoSize = true;
            this.dispatchingRad.Location = new System.Drawing.Point(26, 51);
            this.dispatchingRad.Name = "dispatchingRad";
            this.dispatchingRad.Size = new System.Drawing.Size(81, 17);
            this.dispatchingRad.TabIndex = 45;
            this.dispatchingRad.TabStop = true;
            this.dispatchingRad.Text = "Dispatching";
            this.dispatchingRad.UseVisualStyleBackColor = true;
            // 
            // factoryCodeLabel
            // 
            this.factoryCodeLabel.AutoSize = true;
            this.factoryCodeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.factoryCodeLabel.Location = new System.Drawing.Point(389, 18);
            this.factoryCodeLabel.Name = "factoryCodeLabel";
            this.factoryCodeLabel.Size = new System.Drawing.Size(99, 20);
            this.factoryCodeLabel.TabIndex = 72;
            this.factoryCodeLabel.Text = "Mã nhà máy:";
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
            this.lineName.Location = new System.Drawing.Point(-14, 169);
            this.lineName.Name = "lineName";
            this.lineName.Size = new System.Drawing.Size(200, 26);
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
            // groupBoxDatabaseSettings
            // 
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbType);
            this.groupBoxDatabaseSettings.Controls.Add(this.cmbDatabaseType);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbServer);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbServerName);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbPort);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbPort);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbDatabase);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbDatabaseName);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbTable);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbTableName);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbUser);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbUsername);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbPassword);
            this.groupBoxDatabaseSettings.Controls.Add(this.txtDbPassword);
            this.groupBoxDatabaseSettings.Controls.Add(this.btnTestDbConnection);
            this.groupBoxDatabaseSettings.Controls.Add(this.lblDbConnectionStatus);
            this.groupBoxDatabaseSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.groupBoxDatabaseSettings.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBoxDatabaseSettings.Location = new System.Drawing.Point(450, 3);
            this.groupBoxDatabaseSettings.Name = "groupBoxDatabaseSettings";
            this.groupBoxDatabaseSettings.Size = new System.Drawing.Size(531, 476);
            this.groupBoxDatabaseSettings.TabIndex = 52;
            this.groupBoxDatabaseSettings.TabStop = false;
            this.groupBoxDatabaseSettings.Text = "Cài đặt và kết nối Database Local";
            // 
            // lblDbType
            // 
            this.lblDbType.AutoSize = true;
            this.lblDbType.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbType.Location = new System.Drawing.Point(15, 50);
            this.lblDbType.Name = "lblDbType";
            this.lblDbType.Size = new System.Drawing.Size(117, 20);
            this.lblDbType.TabIndex = 0;
            this.lblDbType.Text = "Loại Database:";
            // 
            // cmbDatabaseType
            // 
            this.cmbDatabaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDatabaseType.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.cmbDatabaseType.FormattingEnabled = true;
            this.cmbDatabaseType.Location = new System.Drawing.Point(170, 50);
            this.cmbDatabaseType.Name = "cmbDatabaseType";
            this.cmbDatabaseType.Size = new System.Drawing.Size(110, 28);
            this.cmbDatabaseType.TabIndex = 1;
            // 
            // lblDbServer
            // 
            this.lblDbServer.AutoSize = true;
            this.lblDbServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbServer.Location = new System.Drawing.Point(15, 100);
            this.lblDbServer.Name = "lblDbServer";
            this.lblDbServer.Size = new System.Drawing.Size(86, 20);
            this.lblDbServer.TabIndex = 2;
            this.lblDbServer.Text = "Server / IP:";
            // 
            // txtDbServerName
            // 
            this.txtDbServerName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbServerName.Location = new System.Drawing.Point(170, 100);
            this.txtDbServerName.Name = "txtDbServerName";
            this.txtDbServerName.Size = new System.Drawing.Size(340, 26);
            this.txtDbServerName.TabIndex = 3;
            // 
            // lblDbPort
            // 
            this.lblDbPort.AutoSize = true;
            this.lblDbPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbPort.Location = new System.Drawing.Point(15, 150);
            this.lblDbPort.Name = "lblDbPort";
            this.lblDbPort.Size = new System.Drawing.Size(42, 20);
            this.lblDbPort.TabIndex = 4;
            this.lblDbPort.Text = "Port:";
            // 
            // txtDbPort
            // 
            this.txtDbPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbPort.Location = new System.Drawing.Point(170, 150);
            this.txtDbPort.Name = "txtDbPort";
            this.txtDbPort.Size = new System.Drawing.Size(110, 26);
            this.txtDbPort.TabIndex = 5;
            // 
            // lblDbDatabase
            // 
            this.lblDbDatabase.AutoSize = true;
            this.lblDbDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbDatabase.Location = new System.Drawing.Point(15, 200);
            this.lblDbDatabase.Name = "lblDbDatabase";
            this.lblDbDatabase.Size = new System.Drawing.Size(114, 20);
            this.lblDbDatabase.TabIndex = 6;
            this.lblDbDatabase.Text = "Tên Database:";
            // 
            // txtDbDatabaseName
            // 
            this.txtDbDatabaseName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbDatabaseName.Location = new System.Drawing.Point(170, 200);
            this.txtDbDatabaseName.Name = "txtDbDatabaseName";
            this.txtDbDatabaseName.Size = new System.Drawing.Size(340, 26);
            this.txtDbDatabaseName.TabIndex = 7;
            // 
            // lblDbTable
            // 
            this.lblDbTable.AutoSize = true;
            this.lblDbTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbTable.Location = new System.Drawing.Point(15, 250);
            this.lblDbTable.Name = "lblDbTable";
            this.lblDbTable.Size = new System.Drawing.Size(80, 20);
            this.lblDbTable.TabIndex = 8;
            this.lblDbTable.Text = "Tên bảng:";
            // 
            // txtDbTableName
            // 
            this.txtDbTableName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbTableName.Location = new System.Drawing.Point(170, 250);
            this.txtDbTableName.Name = "txtDbTableName";
            this.txtDbTableName.Size = new System.Drawing.Size(340, 26);
            this.txtDbTableName.TabIndex = 9;
            // 
            // lblDbUser
            // 
            this.lblDbUser.AutoSize = true;
            this.lblDbUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbUser.Location = new System.Drawing.Point(15, 300);
            this.lblDbUser.Name = "lblDbUser";
            this.lblDbUser.Size = new System.Drawing.Size(82, 20);
            this.lblDbUser.TabIndex = 10;
            this.lblDbUser.Text = "Tài khoản:";
            // 
            // txtDbUsername
            // 
            this.txtDbUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbUsername.Location = new System.Drawing.Point(170, 300);
            this.txtDbUsername.Name = "txtDbUsername";
            this.txtDbUsername.Size = new System.Drawing.Size(340, 26);
            this.txtDbUsername.TabIndex = 11;
            // 
            // lblDbPassword
            // 
            this.lblDbPassword.AutoSize = true;
            this.lblDbPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDbPassword.Location = new System.Drawing.Point(15, 350);
            this.lblDbPassword.Name = "lblDbPassword";
            this.lblDbPassword.Size = new System.Drawing.Size(79, 20);
            this.lblDbPassword.TabIndex = 12;
            this.lblDbPassword.Text = "Mật khẩu:";
            // 
            // txtDbPassword
            // 
            this.txtDbPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.txtDbPassword.Location = new System.Drawing.Point(170, 350);
            this.txtDbPassword.Name = "txtDbPassword";
            this.txtDbPassword.PasswordChar = '*';
            this.txtDbPassword.Size = new System.Drawing.Size(340, 26);
            this.txtDbPassword.TabIndex = 13;
            // 
            // btnTestDbConnection
            // 
            this.btnTestDbConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestDbConnection.Location = new System.Drawing.Point(15, 420);
            this.btnTestDbConnection.Name = "btnTestDbConnection";
            this.btnTestDbConnection.Size = new System.Drawing.Size(170, 32);
            this.btnTestDbConnection.TabIndex = 14;
            this.btnTestDbConnection.Text = "Kiểm tra kết nối";
            this.btnTestDbConnection.UseVisualStyleBackColor = true;
            this.btnTestDbConnection.Click += new System.EventHandler(this.btnTestDbConnection_Click);
            // 
            // lblDbConnectionStatus
            // 
            this.lblDbConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDbConnectionStatus.Location = new System.Drawing.Point(205, 428);
            this.lblDbConnectionStatus.Name = "lblDbConnectionStatus";
            this.lblDbConnectionStatus.Size = new System.Drawing.Size(320, 20);
            this.lblDbConnectionStatus.TabIndex = 15;
            // 
            // ucProductionTHSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxProductionSettings);
            this.Controls.Add(this.groupBoxDatabaseSettings);
            this.Controls.Add(this.onlineProductionSettings);
            this.Name = "ucProductionTHSetting";
            this.Size = new System.Drawing.Size(984, 494);
            this.groupBoxProductionSettings.ResumeLayout(false);
            this.groupBoxProductionSettings.PerformLayout();
            this.onlineProductionSettings.ResumeLayout(false);
            this.onlineProductionSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIncreasedData)).EndInit();
            this.groupBoxDatabaseSettings.ResumeLayout(false);
            this.groupBoxDatabaseSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox apiTextbox;
        private System.Windows.Forms.Label labelApi;
        private System.Windows.Forms.ComboBox comboBoxRLinkId;
        private System.Windows.Forms.Label lineIdLabel;
        private System.Windows.Forms.GroupBox groupBoxProductionSettings;
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
        private System.Windows.Forms.Label factoryCodeLabel;
        private System.Windows.Forms.TextBox LineId;
        private System.Windows.Forms.Panel onlineProductionSettings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox FactoryCodeCombox;
        private System.Windows.Forms.ComboBox RLinkNamescombox;
        private System.Windows.Forms.CheckBox HideFunctions;
        private System.Windows.Forms.GroupBox groupBoxDatabaseSettings;
        private System.Windows.Forms.Label lblDbType;
        private System.Windows.Forms.ComboBox cmbDatabaseType;
        private System.Windows.Forms.Label lblDbServer;
        private System.Windows.Forms.TextBox txtDbServerName;
        private System.Windows.Forms.Label lblDbPort;
        private System.Windows.Forms.TextBox txtDbPort;
        private System.Windows.Forms.Label lblDbDatabase;
        private System.Windows.Forms.TextBox txtDbDatabaseName;
        private System.Windows.Forms.Label lblDbTable;
        private System.Windows.Forms.TextBox txtDbTableName;
        private System.Windows.Forms.Label lblDbUser;
        private System.Windows.Forms.TextBox txtDbUsername;
        private System.Windows.Forms.Label lblDbPassword;
        private System.Windows.Forms.TextBox txtDbPassword;
        private System.Windows.Forms.Button btnTestDbConnection;
        private System.Windows.Forms.Label lblDbConnectionStatus;
    }
}