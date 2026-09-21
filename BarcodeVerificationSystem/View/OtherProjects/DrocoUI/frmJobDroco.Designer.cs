using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.DrocoUI
{
    partial class FrmJobDroco
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
        private bool ValidateJobInputs()
        {
            if (string.IsNullOrWhiteSpace(txtBatchNumber.Text))
            {
                MessageBox.Show("Vui lòng nhập Batch Number.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBatchNumber.Focus();
                return false;
            }
            if (codesInBoxNum.Value <= 0)
            {
                MessageBox.Show("Số mã/hộp phải là số nguyên dương.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                codesInBoxNum.Focus();
                return false;
            }
            if (boxesInCartonNum.Value <= 0)
            {
                MessageBox.Show("Số hộp/thùng phải là số nguyên dương.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                boxesInCartonNum.Focus();
                return false;
            }
            if (cartonsInPalletNum.Value <= 0)
            {
                MessageBox.Show("Số thùng/pallet phải là số nguyên dương.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cartonsInPalletNum.Focus();
                return false;
            }
            return true;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmJobDroco));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.DragControl = new DesignUI.CuzUI.CuzDragControl();
            this.pnlMenu = new DesignUI.CuzUI.CuzPanel();
            this.btnHelp = new FontAwesome.Sharp.IconButton();
            this.btnAbout = new FontAwesome.Sharp.IconButton();
            this.btnSettings = new FontAwesome.Sharp.IconButton();
            this.btnRestart = new FontAwesome.Sharp.IconButton();
            this.btnExit = new FontAwesome.Sharp.IconButton();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.lblStatusCamera01 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusPrinter01 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusPrinterZebra = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusSerialDevice = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblSensorControllerStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblToolStripVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripDateTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblLoadingSSCC = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblLoadingHonest = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtModel = new System.Windows.Forms.TextBox();
            this.txtSalesOrder = new System.Windows.Forms.TextBox();
            this.txtBatchNumber = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.FirstRowHeaderSscc = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.FirstRowHeader = new System.Windows.Forms.CheckBox();
            this.lblImportDatabase = new System.Windows.Forms.Label();
            this.txtDirectoryDatabseCodeSSCC = new DesignUI.CuzUI.CuzTextBox();
            this.txtDirectoryDatabse = new DesignUI.CuzUI.CuzTextBox();
            this.btnImportDatabaseSSCC = new DesignUI.CuzUI.CuzButton();
            this.btnImportDatabase = new DesignUI.CuzUI.CuzButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cartonsInPalletNum = new System.Windows.Forms.NumericUpDown();
            this.boxesInCartonNum = new System.Windows.Forms.NumericUpDown();
            this.labelProductInBox = new System.Windows.Forms.Label();
            this.codesInBoxNum = new System.Windows.Forms.NumericUpDown();
            this.labelBoxesInCarton = new System.Windows.Forms.Label();
            this.labelCartonsInPallet = new System.Windows.Forms.Label();
            this.imageLoading = new System.Windows.Forms.PictureBox();
            this.numberOfCodes = new System.Windows.Forms.Label();
            this.btnSave = new DesignUI.CuzUI.CuzButton();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cuzPanel3 = new DesignUI.CuzUI.CuzPanel();
            this.picLoading = new System.Windows.Forms.PictureBox();
            this.listBoxJobList = new DesignUI.CuzUI.CuzListBox();
            this.pnlJobInfomation = new DesignUI.CuzUI.RoundPanel();
            this.cuzButtonPurge = new DesignUI.CuzUI.CuzButton();
            this.txtJobStatus = new DesignUI.CuzUI.CuzTextBox();
            this.lblJobStatus = new System.Windows.Forms.Label();
            this.txtJobType = new DesignUI.CuzUI.CuzTextBox();
            this.btnDelete = new DesignUI.CuzUI.CuzButton();
            this.lblJobType = new System.Windows.Forms.Label();
            this.lblTemplatePrintInfo = new DesignUI.CuzUI.CuzTextBox();
            this.lblStandalone = new System.Windows.Forms.Label();
            this.lblPODFormatInfo = new DesignUI.CuzUI.CuzTextBox();
            this.lblRLinkSeries = new System.Windows.Forms.Label();
            this.lblStaticTextInfo = new DesignUI.CuzUI.CuzTextBox();
            this.lblTemplatePrint = new System.Windows.Forms.Label();
            this.lblPODFormat = new System.Windows.Forms.Label();
            this.lblStaticText1 = new System.Windows.Forms.Label();
            this.lblCompareTypeInfo = new DesignUI.CuzUI.CuzTextBox();
            this.lblCompareType = new System.Windows.Forms.Label();
            this.lblJobNameInfo = new DesignUI.CuzUI.CuzTextBox();
            this.lblJobName = new System.Windows.Forms.Label();
            this.btnNext = new DesignUI.CuzUI.CuzButton();
            this.txtSearch = new DesignUI.CuzUI.CuzTextBox();
            this.btnSearchJob = new System.Windows.Forms.Button();
            this.btnRefesh = new DesignUI.CuzUI.CuzButton();
            this.tabReCheck = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnClearGrid = new DesignUI.CuzUI.CuzButton();
            this.rtbDetail = new System.Windows.Forms.RichTextBox();
            this.dgvCheckResult = new System.Windows.Forms.DataGridView();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBox = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCarton = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPallet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJob = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cboJobSelect = new System.Windows.Forms.ComboBox();
            this.lblDetailHeader = new System.Windows.Forms.Label();
            this.lblLastScanned = new System.Windows.Forms.Label();
            this.lblSelectJob = new System.Windows.Forms.Label();
            this.tabMapped = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.rtbMappedDetail = new System.Windows.Forms.RichTextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dgvMapped = new System.Windows.Forms.DataGridView();
            this.colMTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMDetail = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlMappedStats = new System.Windows.Forms.Panel();
            this.lblMappedRefLabel = new System.Windows.Forms.Label();
            this.lblMappedRefDisplay = new System.Windows.Forms.Label();
            this.pnlMappedTop = new System.Windows.Forms.Panel();
            this.btnMappedReset = new DesignUI.CuzUI.CuzButton();
            this.pnlJobSelectRow = new System.Windows.Forms.Panel();
            this.cboMappedJobSelect = new System.Windows.Forms.ComboBox();
            this.lblMappedJobSelectLabel = new System.Windows.Forms.Label();
            this.lblMappedPassCount = new System.Windows.Forms.Label();
            this.lblMappedFailCount = new System.Windows.Forms.Label();
            this.lblMappedTotalCount = new System.Windows.Forms.Label();
            this.splitMapped = new System.Windows.Forms.SplitContainer();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mySqlCommand1 = new MySql.Data.MySqlClient.MySqlCommand();
            this.pnlMenu.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lblLoadingSSCC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblLoadingHonest)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cartonsInPalletNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.boxesInCartonNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.codesInBoxNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageLoading)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.cuzPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).BeginInit();
            this.pnlJobInfomation.SuspendLayout();
            this.tabReCheck.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCheckResult)).BeginInit();
            this.tabMapped.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMapped)).BeginInit();
            this.pnlMappedStats.SuspendLayout();
            this.pnlMappedTop.SuspendLayout();
            this.pnlJobSelectRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMapped)).BeginInit();
            this.splitMapped.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // DragControl
            // 
            this.DragControl.DockSides = false;
            this.DragControl.DragParent = true;
            this.DragControl.TargetControl = this.pnlMenu;
            // 
            // pnlMenu
            // 
            this.pnlMenu._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(230)))));
            this.pnlMenu._BorderRadius = 0;
            this.pnlMenu._BorderSize = 1;
            this.pnlMenu._Corner = 90F;
            this.pnlMenu._FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(230)))));
            this.pnlMenu._FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(230)))));
            this.pnlMenu._GradientPanel = true;
            this.pnlMenu.Controls.Add(this.btnHelp);
            this.pnlMenu.Controls.Add(this.btnAbout);
            this.pnlMenu.Controls.Add(this.btnSettings);
            this.pnlMenu.Controls.Add(this.btnRestart);
            this.pnlMenu.Controls.Add(this.btnExit);
            resources.ApplyResources(this.pnlMenu, "pnlMenu");
            this.pnlMenu.Name = "pnlMenu";
            // 
            // btnHelp
            // 
            this.btnHelp.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnHelp, "btnHelp");
            this.btnHelp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnHelp.FlatAppearance.BorderSize = 0;
            this.btnHelp.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnHelp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnHelp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnHelp.ForeColor = System.Drawing.Color.White;
            this.btnHelp.IconChar = FontAwesome.Sharp.IconChar.Question;
            this.btnHelp.IconColor = System.Drawing.Color.White;
            this.btnHelp.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnHelp.IconSize = 30;
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.UseVisualStyleBackColor = false;
            // 
            // btnAbout
            // 
            this.btnAbout.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnAbout, "btnAbout");
            this.btnAbout.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnAbout.FlatAppearance.BorderSize = 0;
            this.btnAbout.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnAbout.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnAbout.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnAbout.ForeColor = System.Drawing.Color.White;
            this.btnAbout.IconChar = FontAwesome.Sharp.IconChar.Info;
            this.btnAbout.IconColor = System.Drawing.Color.White;
            this.btnAbout.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnAbout.IconSize = 30;
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.UseVisualStyleBackColor = false;
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnSettings, "btnSettings");
            this.btnSettings.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.IconChar = FontAwesome.Sharp.IconChar.Cog;
            this.btnSettings.IconColor = System.Drawing.Color.White;
            this.btnSettings.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnSettings.IconSize = 30;
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.UseVisualStyleBackColor = false;
            // 
            // btnRestart
            // 
            this.btnRestart.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnRestart, "btnRestart");
            this.btnRestart.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnRestart.FlatAppearance.BorderSize = 0;
            this.btnRestart.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnRestart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnRestart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnRestart.ForeColor = System.Drawing.Color.White;
            this.btnRestart.IconChar = FontAwesome.Sharp.IconChar.RotateForward;
            this.btnRestart.IconColor = System.Drawing.Color.White;
            this.btnRestart.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnRestart.IconSize = 30;
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.UseVisualStyleBackColor = false;
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnExit, "btnExit");
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnExit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.IconChar = FontAwesome.Sharp.IconChar.RightFromBracket;
            this.btnExit.IconColor = System.Drawing.Color.White;
            this.btnExit.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnExit.IconSize = 30;
            this.btnExit.Name = "btnExit";
            this.btnExit.UseVisualStyleBackColor = false;
            // 
            // statusStrip2
            // 
            resources.ApplyResources(this.statusStrip2, "statusStrip2");
            this.statusStrip2.BackColor = System.Drawing.Color.White;
            this.statusStrip2.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusCamera01,
            this.lblStatusPrinter01,
            this.lblStatusPrinterZebra,
            this.lblStatusSerialDevice,
            this.lblSensorControllerStatus,
            this.lblToolStripVersion,
            this.toolStripDateTime});
            this.statusStrip2.Name = "statusStrip2";
            // 
            // lblStatusCamera01
            // 
            resources.ApplyResources(this.lblStatusCamera01, "lblStatusCamera01");
            this.lblStatusCamera01.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblStatusCamera01.Margin = new System.Windows.Forms.Padding(15, 3, 0, 5);
            this.lblStatusCamera01.Name = "lblStatusCamera01";
            this.lblStatusCamera01.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            // 
            // lblStatusPrinter01
            // 
            resources.ApplyResources(this.lblStatusPrinter01, "lblStatusPrinter01");
            this.lblStatusPrinter01.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblStatusPrinter01.Margin = new System.Windows.Forms.Padding(10, 3, 0, 5);
            this.lblStatusPrinter01.Name = "lblStatusPrinter01";
            this.lblStatusPrinter01.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            // 
            // lblStatusPrinterZebra
            // 
            resources.ApplyResources(this.lblStatusPrinterZebra, "lblStatusPrinterZebra");
            this.lblStatusPrinterZebra.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblStatusPrinterZebra.Margin = new System.Windows.Forms.Padding(10, 3, 0, 5);
            this.lblStatusPrinterZebra.Name = "lblStatusPrinterZebra";
            this.lblStatusPrinterZebra.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            // 
            // lblStatusSerialDevice
            // 
            resources.ApplyResources(this.lblStatusSerialDevice, "lblStatusSerialDevice");
            this.lblStatusSerialDevice.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblStatusSerialDevice.Margin = new System.Windows.Forms.Padding(10, 3, 0, 5);
            this.lblStatusSerialDevice.Name = "lblStatusSerialDevice";
            this.lblStatusSerialDevice.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            // 
            // lblSensorControllerStatus
            // 
            resources.ApplyResources(this.lblSensorControllerStatus, "lblSensorControllerStatus");
            this.lblSensorControllerStatus.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblSensorControllerStatus.Margin = new System.Windows.Forms.Padding(10, 3, 0, 5);
            this.lblSensorControllerStatus.Name = "lblSensorControllerStatus";
            this.lblSensorControllerStatus.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.lblSensorControllerStatus.Spring = true;
            // 
            // lblToolStripVersion
            // 
            this.lblToolStripVersion.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            resources.ApplyResources(this.lblToolStripVersion, "lblToolStripVersion");
            this.lblToolStripVersion.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblToolStripVersion.Margin = new System.Windows.Forms.Padding(3, 3, 0, 2);
            this.lblToolStripVersion.Name = "lblToolStripVersion";
            // 
            // toolStripDateTime
            // 
            this.toolStripDateTime.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            resources.ApplyResources(this.toolStripDateTime, "toolStripDateTime");
            this.toolStripDateTime.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.toolStripDateTime.Name = "toolStripDateTime";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabReCheck);
            this.tabControl1.Controls.Add(this.tabMapped);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.White;
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.imageLoading);
            this.tabPage2.Controls.Add(this.numberOfCodes);
            this.tabPage2.Controls.Add(this.btnSave);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.lblLoadingSSCC);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.lblLoadingHonest);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.txtModel);
            this.groupBox3.Controls.Add(this.txtSalesOrder);
            this.groupBox3.Controls.Add(this.txtBatchNumber);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label3.Name = "label3";
            // 
            // lblLoadingSSCC
            // 
            resources.ApplyResources(this.lblLoadingSSCC, "lblLoadingSSCC");
            this.lblLoadingSSCC.Name = "lblLoadingSSCC";
            this.lblLoadingSSCC.TabStop = false;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label2.Name = "label2";
            // 
            // lblLoadingHonest
            // 
            resources.ApplyResources(this.lblLoadingHonest, "lblLoadingHonest");
            this.lblLoadingHonest.Name = "lblLoadingHonest";
            this.lblLoadingHonest.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label1.Name = "label1";
            this.label1.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // txtModel
            // 
            this.txtModel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtModel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtModel.ForeColor = System.Drawing.SystemColors.WindowFrame;
            resources.ApplyResources(this.txtModel, "txtModel");
            this.txtModel.Name = "txtModel";
            // 
            // txtSalesOrder
            // 
            this.txtSalesOrder.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtSalesOrder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSalesOrder.ForeColor = System.Drawing.SystemColors.WindowFrame;
            resources.ApplyResources(this.txtSalesOrder, "txtSalesOrder");
            this.txtSalesOrder.Name = "txtSalesOrder";
            // 
            // txtBatchNumber
            // 
            this.txtBatchNumber.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtBatchNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBatchNumber.ForeColor = System.Drawing.SystemColors.WindowFrame;
            resources.ApplyResources(this.txtBatchNumber, "txtBatchNumber");
            this.txtBatchNumber.Name = "txtBatchNumber";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.FirstRowHeaderSscc);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.FirstRowHeader);
            this.groupBox2.Controls.Add(this.lblImportDatabase);
            this.groupBox2.Controls.Add(this.txtDirectoryDatabseCodeSSCC);
            this.groupBox2.Controls.Add(this.txtDirectoryDatabse);
            this.groupBox2.Controls.Add(this.btnImportDatabaseSSCC);
            this.groupBox2.Controls.Add(this.btnImportDatabase);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // FirstRowHeaderSscc
            // 
            resources.ApplyResources(this.FirstRowHeaderSscc, "FirstRowHeaderSscc");
            this.FirstRowHeaderSscc.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.FirstRowHeaderSscc.Name = "FirstRowHeaderSscc";
            this.FirstRowHeaderSscc.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label5.Name = "label5";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label4.Name = "label4";
            // 
            // FirstRowHeader
            // 
            resources.ApplyResources(this.FirstRowHeader, "FirstRowHeader");
            this.FirstRowHeader.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.FirstRowHeader.Name = "FirstRowHeader";
            this.FirstRowHeader.UseVisualStyleBackColor = true;
            // 
            // lblImportDatabase
            // 
            resources.ApplyResources(this.lblImportDatabase, "lblImportDatabase");
            this.lblImportDatabase.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblImportDatabase.Name = "lblImportDatabase";
            // 
            // txtDirectoryDatabseCodeSSCC
            // 
            this.txtDirectoryDatabseCodeSSCC._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtDirectoryDatabseCodeSSCC._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtDirectoryDatabseCodeSSCC.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtDirectoryDatabseCodeSSCC.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtDirectoryDatabseCodeSSCC.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtDirectoryDatabseCodeSSCC.BorderRadius = 6;
            this.txtDirectoryDatabseCodeSSCC.BorderSize = 1;
            resources.ApplyResources(this.txtDirectoryDatabseCodeSSCC, "txtDirectoryDatabseCodeSSCC");
            this.txtDirectoryDatabseCodeSSCC.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtDirectoryDatabseCodeSSCC.Multiline = false;
            this.txtDirectoryDatabseCodeSSCC.Name = "txtDirectoryDatabseCodeSSCC";
            this.txtDirectoryDatabseCodeSSCC.PasswordChar = false;
            this.txtDirectoryDatabseCodeSSCC.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtDirectoryDatabseCodeSSCC.PlaceholderText = "";
            this.txtDirectoryDatabseCodeSSCC.ReadOnly = true;
            this.txtDirectoryDatabseCodeSSCC.UnderlinedStyle = false;
            // 
            // txtDirectoryDatabse
            // 
            this.txtDirectoryDatabse._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtDirectoryDatabse._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtDirectoryDatabse.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtDirectoryDatabse.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtDirectoryDatabse.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtDirectoryDatabse.BorderRadius = 6;
            this.txtDirectoryDatabse.BorderSize = 1;
            resources.ApplyResources(this.txtDirectoryDatabse, "txtDirectoryDatabse");
            this.txtDirectoryDatabse.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtDirectoryDatabse.Multiline = false;
            this.txtDirectoryDatabse.Name = "txtDirectoryDatabse";
            this.txtDirectoryDatabse.PasswordChar = false;
            this.txtDirectoryDatabse.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtDirectoryDatabse.PlaceholderText = "";
            this.txtDirectoryDatabse.ReadOnly = true;
            this.txtDirectoryDatabse.UnderlinedStyle = false;
            // 
            // btnImportDatabaseSSCC
            // 
            this.btnImportDatabaseSSCC._BorderColor = System.Drawing.Color.Silver;
            this.btnImportDatabaseSSCC._BorderRadius = 10;
            this.btnImportDatabaseSSCC._BorderSize = 1;
            this.btnImportDatabaseSSCC._GradientsButton = false;
            this.btnImportDatabaseSSCC._Text = "";
            this.btnImportDatabaseSSCC.BackColor = System.Drawing.Color.White;
            this.btnImportDatabaseSSCC.BackgroundColor = System.Drawing.Color.White;
            this.btnImportDatabaseSSCC.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnImportDatabaseSSCC.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnImportDatabaseSSCC.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnImportDatabaseSSCC, "btnImportDatabaseSSCC");
            this.btnImportDatabaseSSCC.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnImportDatabaseSSCC.Name = "btnImportDatabaseSSCC";
            this.btnImportDatabaseSSCC.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnImportDatabaseSSCC.UseVisualStyleBackColor = false;
            // 
            // btnImportDatabase
            // 
            this.btnImportDatabase._BorderColor = System.Drawing.Color.Silver;
            this.btnImportDatabase._BorderRadius = 10;
            this.btnImportDatabase._BorderSize = 1;
            this.btnImportDatabase._GradientsButton = false;
            this.btnImportDatabase._Text = "";
            this.btnImportDatabase.BackColor = System.Drawing.Color.White;
            this.btnImportDatabase.BackgroundColor = System.Drawing.Color.White;
            this.btnImportDatabase.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnImportDatabase.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnImportDatabase.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnImportDatabase, "btnImportDatabase");
            this.btnImportDatabase.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnImportDatabase.Name = "btnImportDatabase";
            this.btnImportDatabase.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnImportDatabase.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cartonsInPalletNum);
            this.groupBox1.Controls.Add(this.boxesInCartonNum);
            this.groupBox1.Controls.Add(this.labelProductInBox);
            this.groupBox1.Controls.Add(this.codesInBoxNum);
            this.groupBox1.Controls.Add(this.labelBoxesInCarton);
            this.groupBox1.Controls.Add(this.labelCartonsInPallet);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // cartonsInPalletNum
            // 
            resources.ApplyResources(this.cartonsInPalletNum, "cartonsInPalletNum");
            this.cartonsInPalletNum.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.cartonsInPalletNum.Name = "cartonsInPalletNum";
            // 
            // boxesInCartonNum
            // 
            resources.ApplyResources(this.boxesInCartonNum, "boxesInCartonNum");
            this.boxesInCartonNum.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.boxesInCartonNum.Name = "boxesInCartonNum";
            // 
            // labelProductInBox
            // 
            resources.ApplyResources(this.labelProductInBox, "labelProductInBox");
            this.labelProductInBox.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.labelProductInBox.Name = "labelProductInBox";
            this.labelProductInBox.Click += new System.EventHandler(this.label1_Click);
            // 
            // codesInBoxNum
            // 
            resources.ApplyResources(this.codesInBoxNum, "codesInBoxNum");
            this.codesInBoxNum.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.codesInBoxNum.Name = "codesInBoxNum";
            // 
            // labelBoxesInCarton
            // 
            resources.ApplyResources(this.labelBoxesInCarton, "labelBoxesInCarton");
            this.labelBoxesInCarton.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.labelBoxesInCarton.Name = "labelBoxesInCarton";
            this.labelBoxesInCarton.Click += new System.EventHandler(this.label1_Click);
            // 
            // labelCartonsInPallet
            // 
            resources.ApplyResources(this.labelCartonsInPallet, "labelCartonsInPallet");
            this.labelCartonsInPallet.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.labelCartonsInPallet.Name = "labelCartonsInPallet";
            this.labelCartonsInPallet.Click += new System.EventHandler(this.label1_Click);
            // 
            // imageLoading
            // 
            resources.ApplyResources(this.imageLoading, "imageLoading");
            this.imageLoading.Name = "imageLoading";
            this.imageLoading.TabStop = false;
            // 
            // numberOfCodes
            // 
            resources.ApplyResources(this.numberOfCodes, "numberOfCodes");
            this.numberOfCodes.ForeColor = System.Drawing.Color.DarkOrange;
            this.numberOfCodes.Name = "numberOfCodes";
            this.numberOfCodes.Click += new System.EventHandler(this.numberOfCodes_Click);
            // 
            // btnSave
            // 
            this.btnSave._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnSave._BorderRadius = 10;
            this.btnSave._BorderSize = 1;
            this.btnSave._GradientsButton = false;
            this.btnSave._Text = "Tạo công việc";
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnSave.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnSave.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnSave.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Name = "btnSave";
            this.btnSave.TextColor = System.Drawing.Color.White;
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.White;
            this.tabPage1.Controls.Add(this.cuzPanel3);
            this.tabPage1.Controls.Add(this.pnlJobInfomation);
            this.tabPage1.Controls.Add(this.btnNext);
            this.tabPage1.Controls.Add(this.txtSearch);
            this.tabPage1.Controls.Add(this.btnSearchJob);
            this.tabPage1.Controls.Add(this.btnRefesh);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            // 
            // cuzPanel3
            // 
            this.cuzPanel3._BorderColor = System.Drawing.Color.Silver;
            this.cuzPanel3._BorderRadius = 10;
            this.cuzPanel3._BorderSize = 1;
            this.cuzPanel3._Corner = 0F;
            this.cuzPanel3._FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.cuzPanel3._FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.cuzPanel3._GradientPanel = false;
            this.cuzPanel3.Controls.Add(this.picLoading);
            this.cuzPanel3.Controls.Add(this.listBoxJobList);
            resources.ApplyResources(this.cuzPanel3, "cuzPanel3");
            this.cuzPanel3.Name = "cuzPanel3";
            // 
            // picLoading
            // 
            resources.ApplyResources(this.picLoading, "picLoading");
            this.picLoading.Name = "picLoading";
            this.picLoading.TabStop = false;
            // 
            // listBoxJobList
            // 
            this.listBoxJobList._BackEnableSelectedColor = System.Drawing.Color.LightGray;
            this.listBoxJobList._BackSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.listBoxJobList._BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxJobList._ItemHeight = 35;
            this.listBoxJobList._LeftBaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(230)))));
            this.listBoxJobList._LeftEnableSelectedColor = System.Drawing.Color.DarkGray;
            this.listBoxJobList._LeftSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(230)))));
            this.listBoxJobList._TextColor = System.Drawing.SystemColors.WindowFrame;
            this.listBoxJobList._TextSelectedColor = System.Drawing.SystemColors.WindowText;
            this.listBoxJobList._Underline = false;
            this.listBoxJobList._UnderlineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(230)))));
            resources.ApplyResources(this.listBoxJobList, "listBoxJobList");
            this.listBoxJobList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBoxJobList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listBoxJobList.FormattingEnabled = true;
            this.listBoxJobList.Name = "listBoxJobList";
            // 
            // pnlJobInfomation
            // 
            this.pnlJobInfomation.BackColor = System.Drawing.Color.Transparent;
            this.pnlJobInfomation.Controls.Add(this.cuzButtonPurge);
            this.pnlJobInfomation.Controls.Add(this.txtJobStatus);
            this.pnlJobInfomation.Controls.Add(this.lblJobStatus);
            this.pnlJobInfomation.Controls.Add(this.txtJobType);
            this.pnlJobInfomation.Controls.Add(this.btnDelete);
            this.pnlJobInfomation.Controls.Add(this.lblJobType);
            this.pnlJobInfomation.Controls.Add(this.lblTemplatePrintInfo);
            this.pnlJobInfomation.Controls.Add(this.lblStandalone);
            this.pnlJobInfomation.Controls.Add(this.lblPODFormatInfo);
            this.pnlJobInfomation.Controls.Add(this.lblRLinkSeries);
            this.pnlJobInfomation.Controls.Add(this.lblStaticTextInfo);
            this.pnlJobInfomation.Controls.Add(this.lblTemplatePrint);
            this.pnlJobInfomation.Controls.Add(this.lblPODFormat);
            this.pnlJobInfomation.Controls.Add(this.lblStaticText1);
            this.pnlJobInfomation.Controls.Add(this.lblCompareTypeInfo);
            this.pnlJobInfomation.Controls.Add(this.lblCompareType);
            this.pnlJobInfomation.Controls.Add(this.lblJobNameInfo);
            this.pnlJobInfomation.Controls.Add(this.lblJobName);
            resources.ApplyResources(this.pnlJobInfomation, "pnlJobInfomation");
            this.pnlJobInfomation.IsTitleHatchStyle = false;
            this.pnlJobInfomation.Name = "pnlJobInfomation";
            this.pnlJobInfomation.Radious = 15;
            this.pnlJobInfomation.TabStop = false;
            this.pnlJobInfomation.TitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(230)))));
            this.pnlJobInfomation.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlJobInfomation.TitleForeColor = System.Drawing.Color.White;
            this.pnlJobInfomation.TitleHatchStyle = System.Drawing.Drawing2D.HatchStyle.Percent60;
            this.pnlJobInfomation.TitleHeight = 35;
            // 
            // cuzButtonPurge
            // 
            this.cuzButtonPurge._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.cuzButtonPurge._BorderRadius = 20;
            this.cuzButtonPurge._BorderSize = 1;
            this.cuzButtonPurge._GradientsButton = false;
            this.cuzButtonPurge._Text = "Purge";
            this.cuzButtonPurge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.cuzButtonPurge.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.cuzButtonPurge.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.cuzButtonPurge.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.cuzButtonPurge.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.cuzButtonPurge, "cuzButtonPurge");
            this.cuzButtonPurge.ForeColor = System.Drawing.Color.White;
            this.cuzButtonPurge.Name = "cuzButtonPurge";
            this.cuzButtonPurge.TextColor = System.Drawing.Color.White;
            this.cuzButtonPurge.UseVisualStyleBackColor = false;
            // 
            // txtJobStatus
            // 
            this.txtJobStatus._ReadOnlyBackColor = System.Drawing.Color.White;
            this.txtJobStatus._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtJobStatus.BackColor = System.Drawing.Color.White;
            this.txtJobStatus.BorderColor = System.Drawing.Color.Silver;
            this.txtJobStatus.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtJobStatus.BorderRadius = 8;
            this.txtJobStatus.BorderSize = 1;
            resources.ApplyResources(this.txtJobStatus, "txtJobStatus");
            this.txtJobStatus.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.txtJobStatus.Multiline = false;
            this.txtJobStatus.Name = "txtJobStatus";
            this.txtJobStatus.PasswordChar = false;
            this.txtJobStatus.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtJobStatus.PlaceholderText = "";
            this.txtJobStatus.ReadOnly = true;
            this.txtJobStatus.UnderlinedStyle = false;
            // 
            // lblJobStatus
            // 
            resources.ApplyResources(this.lblJobStatus, "lblJobStatus");
            this.lblJobStatus.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblJobStatus.Name = "lblJobStatus";
            // 
            // txtJobType
            // 
            this.txtJobType._ReadOnlyBackColor = System.Drawing.Color.White;
            this.txtJobType._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtJobType.BackColor = System.Drawing.Color.White;
            this.txtJobType.BorderColor = System.Drawing.Color.Silver;
            this.txtJobType.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtJobType.BorderRadius = 8;
            this.txtJobType.BorderSize = 1;
            resources.ApplyResources(this.txtJobType, "txtJobType");
            this.txtJobType.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.txtJobType.Multiline = false;
            this.txtJobType.Name = "txtJobType";
            this.txtJobType.PasswordChar = false;
            this.txtJobType.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtJobType.PlaceholderText = "";
            this.txtJobType.ReadOnly = true;
            this.txtJobType.UnderlinedStyle = false;
            // 
            // btnDelete
            // 
            this.btnDelete._BorderColor = System.Drawing.Color.Red;
            this.btnDelete._BorderRadius = 20;
            this.btnDelete._BorderSize = 1;
            this.btnDelete._GradientsButton = false;
            this.btnDelete._Text = "Delete";
            this.btnDelete.BackColor = System.Drawing.Color.Red;
            this.btnDelete.BackgroundColor = System.Drawing.Color.Red;
            this.btnDelete.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnDelete.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnDelete.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnDelete, "btnDelete");
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.TextColor = System.Drawing.Color.White;
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // lblJobType
            // 
            resources.ApplyResources(this.lblJobType, "lblJobType");
            this.lblJobType.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblJobType.Name = "lblJobType";
            // 
            // lblTemplatePrintInfo
            // 
            this.lblTemplatePrintInfo._ReadOnlyBackColor = System.Drawing.Color.White;
            this.lblTemplatePrintInfo._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblTemplatePrintInfo.BackColor = System.Drawing.Color.White;
            this.lblTemplatePrintInfo.BorderColor = System.Drawing.Color.Silver;
            this.lblTemplatePrintInfo.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblTemplatePrintInfo.BorderRadius = 8;
            this.lblTemplatePrintInfo.BorderSize = 1;
            resources.ApplyResources(this.lblTemplatePrintInfo, "lblTemplatePrintInfo");
            this.lblTemplatePrintInfo.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblTemplatePrintInfo.Multiline = false;
            this.lblTemplatePrintInfo.Name = "lblTemplatePrintInfo";
            this.lblTemplatePrintInfo.PasswordChar = false;
            this.lblTemplatePrintInfo.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.lblTemplatePrintInfo.PlaceholderText = "";
            this.lblTemplatePrintInfo.ReadOnly = true;
            this.lblTemplatePrintInfo.UnderlinedStyle = false;
            // 
            // lblStandalone
            // 
            resources.ApplyResources(this.lblStandalone, "lblStandalone");
            this.lblStandalone.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblStandalone.Name = "lblStandalone";
            // 
            // lblPODFormatInfo
            // 
            this.lblPODFormatInfo._ReadOnlyBackColor = System.Drawing.Color.White;
            this.lblPODFormatInfo._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblPODFormatInfo.BackColor = System.Drawing.Color.White;
            this.lblPODFormatInfo.BorderColor = System.Drawing.Color.Silver;
            this.lblPODFormatInfo.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblPODFormatInfo.BorderRadius = 8;
            this.lblPODFormatInfo.BorderSize = 1;
            resources.ApplyResources(this.lblPODFormatInfo, "lblPODFormatInfo");
            this.lblPODFormatInfo.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblPODFormatInfo.Multiline = false;
            this.lblPODFormatInfo.Name = "lblPODFormatInfo";
            this.lblPODFormatInfo.PasswordChar = false;
            this.lblPODFormatInfo.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.lblPODFormatInfo.PlaceholderText = "";
            this.lblPODFormatInfo.ReadOnly = true;
            this.lblPODFormatInfo.UnderlinedStyle = false;
            // 
            // lblRLinkSeries
            // 
            resources.ApplyResources(this.lblRLinkSeries, "lblRLinkSeries");
            this.lblRLinkSeries.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblRLinkSeries.Name = "lblRLinkSeries";
            // 
            // lblStaticTextInfo
            // 
            this.lblStaticTextInfo._ReadOnlyBackColor = System.Drawing.Color.White;
            this.lblStaticTextInfo._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblStaticTextInfo.BackColor = System.Drawing.Color.White;
            this.lblStaticTextInfo.BorderColor = System.Drawing.Color.Silver;
            this.lblStaticTextInfo.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblStaticTextInfo.BorderRadius = 8;
            this.lblStaticTextInfo.BorderSize = 1;
            resources.ApplyResources(this.lblStaticTextInfo, "lblStaticTextInfo");
            this.lblStaticTextInfo.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblStaticTextInfo.Multiline = false;
            this.lblStaticTextInfo.Name = "lblStaticTextInfo";
            this.lblStaticTextInfo.PasswordChar = false;
            this.lblStaticTextInfo.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.lblStaticTextInfo.PlaceholderText = "";
            this.lblStaticTextInfo.ReadOnly = true;
            this.lblStaticTextInfo.UnderlinedStyle = false;
            // 
            // lblTemplatePrint
            // 
            resources.ApplyResources(this.lblTemplatePrint, "lblTemplatePrint");
            this.lblTemplatePrint.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblTemplatePrint.Name = "lblTemplatePrint";
            // 
            // lblPODFormat
            // 
            resources.ApplyResources(this.lblPODFormat, "lblPODFormat");
            this.lblPODFormat.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblPODFormat.Name = "lblPODFormat";
            // 
            // lblStaticText1
            // 
            resources.ApplyResources(this.lblStaticText1, "lblStaticText1");
            this.lblStaticText1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblStaticText1.Name = "lblStaticText1";
            // 
            // lblCompareTypeInfo
            // 
            this.lblCompareTypeInfo._ReadOnlyBackColor = System.Drawing.Color.White;
            this.lblCompareTypeInfo._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblCompareTypeInfo.BackColor = System.Drawing.Color.White;
            this.lblCompareTypeInfo.BorderColor = System.Drawing.Color.Silver;
            this.lblCompareTypeInfo.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblCompareTypeInfo.BorderRadius = 8;
            this.lblCompareTypeInfo.BorderSize = 1;
            resources.ApplyResources(this.lblCompareTypeInfo, "lblCompareTypeInfo");
            this.lblCompareTypeInfo.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblCompareTypeInfo.Multiline = false;
            this.lblCompareTypeInfo.Name = "lblCompareTypeInfo";
            this.lblCompareTypeInfo.PasswordChar = false;
            this.lblCompareTypeInfo.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.lblCompareTypeInfo.PlaceholderText = "";
            this.lblCompareTypeInfo.ReadOnly = true;
            this.lblCompareTypeInfo.UnderlinedStyle = false;
            // 
            // lblCompareType
            // 
            resources.ApplyResources(this.lblCompareType, "lblCompareType");
            this.lblCompareType.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblCompareType.Name = "lblCompareType";
            // 
            // lblJobNameInfo
            // 
            this.lblJobNameInfo._ReadOnlyBackColor = System.Drawing.Color.White;
            this.lblJobNameInfo._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblJobNameInfo.BackColor = System.Drawing.Color.White;
            this.lblJobNameInfo.BorderColor = System.Drawing.Color.Silver;
            this.lblJobNameInfo.BorderFocusColor = System.Drawing.Color.Gainsboro;
            this.lblJobNameInfo.BorderRadius = 8;
            this.lblJobNameInfo.BorderSize = 1;
            resources.ApplyResources(this.lblJobNameInfo, "lblJobNameInfo");
            this.lblJobNameInfo.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblJobNameInfo.Multiline = false;
            this.lblJobNameInfo.Name = "lblJobNameInfo";
            this.lblJobNameInfo.PasswordChar = false;
            this.lblJobNameInfo.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.lblJobNameInfo.PlaceholderText = "";
            this.lblJobNameInfo.ReadOnly = true;
            this.lblJobNameInfo.UnderlinedStyle = false;
            // 
            // lblJobName
            // 
            resources.ApplyResources(this.lblJobName, "lblJobName");
            this.lblJobName.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblJobName.Name = "lblJobName";
            // 
            // btnNext
            // 
            this.btnNext._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(199)))), ((int)(((byte)(82)))));
            this.btnNext._BorderRadius = 20;
            this.btnNext._BorderSize = 1;
            this.btnNext._GradientsButton = false;
            this.btnNext._Text = "Next";
            this.btnNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(199)))), ((int)(((byte)(82)))));
            this.btnNext.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(199)))), ((int)(((byte)(82)))));
            this.btnNext.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnNext.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnNext.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnNext, "btnNext");
            this.btnNext.ForeColor = System.Drawing.Color.White;
            this.btnNext.Name = "btnNext";
            this.btnNext.TextColor = System.Drawing.Color.White;
            this.btnNext.UseVisualStyleBackColor = false;
            // 
            // txtSearch
            // 
            this.txtSearch._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtSearch._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtSearch.BackColor = System.Drawing.SystemColors.Window;
            this.txtSearch.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtSearch.BorderFocusColor = System.Drawing.Color.Silver;
            this.txtSearch.BorderRadius = 6;
            this.txtSearch.BorderSize = 1;
            resources.ApplyResources(this.txtSearch, "txtSearch");
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtSearch.Multiline = false;
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PasswordChar = false;
            this.txtSearch.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtSearch.PlaceholderText = "";
            this.txtSearch.ReadOnly = false;
            this.txtSearch.UnderlinedStyle = false;
            // 
            // btnSearchJob
            // 
            resources.ApplyResources(this.btnSearchJob, "btnSearchJob");
            this.btnSearchJob.FlatAppearance.BorderSize = 0;
            this.btnSearchJob.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnSearchJob.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnSearchJob.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSearchJob.Name = "btnSearchJob";
            this.btnSearchJob.UseVisualStyleBackColor = true;
            // 
            // btnRefesh
            // 
            this.btnRefesh._BorderColor = System.Drawing.Color.Silver;
            this.btnRefesh._BorderRadius = 10;
            this.btnRefesh._BorderSize = 1;
            this.btnRefesh._GradientsButton = false;
            this.btnRefesh._Text = "";
            resources.ApplyResources(this.btnRefesh, "btnRefesh");
            this.btnRefesh.BackColor = System.Drawing.Color.White;
            this.btnRefesh.BackgroundColor = System.Drawing.Color.White;
            this.btnRefesh.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnRefesh.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnRefesh.FlatAppearance.BorderSize = 0;
            this.btnRefesh.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRefesh.Name = "btnRefesh";
            this.btnRefesh.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnRefesh.UseVisualStyleBackColor = false;
            // 
            // tabReCheck
            // 
            this.tabReCheck.Controls.Add(this.panel1);
            resources.ApplyResources(this.tabReCheck, "tabReCheck");
            this.tabReCheck.Name = "tabReCheck";
            this.tabReCheck.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.btnClearGrid);
            this.panel1.Controls.Add(this.rtbDetail);
            this.panel1.Controls.Add(this.dgvCheckResult);
            this.panel1.Controls.Add(this.cboJobSelect);
            this.panel1.Controls.Add(this.lblDetailHeader);
            this.panel1.Controls.Add(this.lblLastScanned);
            this.panel1.Controls.Add(this.lblSelectJob);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // btnClearGrid
            // 
            this.btnClearGrid._BorderColor = System.Drawing.Color.Silver;
            this.btnClearGrid._BorderRadius = 10;
            this.btnClearGrid._BorderSize = 1;
            this.btnClearGrid._GradientsButton = false;
            this.btnClearGrid._Text = "";
            resources.ApplyResources(this.btnClearGrid, "btnClearGrid");
            this.btnClearGrid.BackColor = System.Drawing.Color.White;
            this.btnClearGrid.BackgroundColor = System.Drawing.Color.White;
            this.btnClearGrid.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnClearGrid.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnClearGrid.FlatAppearance.BorderSize = 0;
            this.btnClearGrid.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnClearGrid.Name = "btnClearGrid";
            this.btnClearGrid.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnClearGrid.UseVisualStyleBackColor = false;
            this.btnClearGrid.Click += new System.EventHandler(this.BtnClearGrid_Click);
            // 
            // rtbDetail
            // 
            this.rtbDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.rtbDetail, "rtbDetail");
            this.rtbDetail.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.rtbDetail.Name = "rtbDetail";
            this.rtbDetail.ReadOnly = true;
            // 
            // dgvCheckResult
            // 
            this.dgvCheckResult.AllowUserToAddRows = false;
            this.dgvCheckResult.AllowUserToDeleteRows = false;
            this.dgvCheckResult.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(253)))), ((int)(((byte)(224)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvCheckResult.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvCheckResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCheckResult.BackgroundColor = System.Drawing.SystemColors.MenuBar;
            this.dgvCheckResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvCheckResult.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            resources.ApplyResources(this.dgvCheckResult, "dgvCheckResult");
            this.dgvCheckResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvCheckResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTime,
            this.colCode,
            this.colType,
            this.colBox,
            this.colCarton,
            this.colPallet,
            this.colStatus,
            this.colJob});
            this.dgvCheckResult.GridColor = System.Drawing.SystemColors.ButtonFace;
            this.dgvCheckResult.Name = "dgvCheckResult";
            this.dgvCheckResult.ReadOnly = true;
            this.dgvCheckResult.RowHeadersVisible = false;
            this.dgvCheckResult.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvCheckResult.RowTemplate.Height = 35;
            this.dgvCheckResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // colTime
            // 
            this.colTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            resources.ApplyResources(this.colTime, "colTime");
            this.colTime.Name = "colTime";
            this.colTime.ReadOnly = true;
            // 
            // colCode
            // 
            resources.ApplyResources(this.colCode, "colCode");
            this.colCode.Name = "colCode";
            this.colCode.ReadOnly = true;
            // 
            // colType
            // 
            this.colType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            resources.ApplyResources(this.colType, "colType");
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            // 
            // colBox
            // 
            resources.ApplyResources(this.colBox, "colBox");
            this.colBox.Name = "colBox";
            this.colBox.ReadOnly = true;
            // 
            // colCarton
            // 
            resources.ApplyResources(this.colCarton, "colCarton");
            this.colCarton.Name = "colCarton";
            this.colCarton.ReadOnly = true;
            // 
            // colPallet
            // 
            resources.ApplyResources(this.colPallet, "colPallet");
            this.colPallet.Name = "colPallet";
            this.colPallet.ReadOnly = true;
            // 
            // colStatus
            // 
            resources.ApplyResources(this.colStatus, "colStatus");
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            // 
            // colJob
            // 
            resources.ApplyResources(this.colJob, "colJob");
            this.colJob.Name = "colJob";
            this.colJob.ReadOnly = true;
            // 
            // cboJobSelect
            // 
            this.cboJobSelect.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.cboJobSelect.FormattingEnabled = true;
            resources.ApplyResources(this.cboJobSelect, "cboJobSelect");
            this.cboJobSelect.Name = "cboJobSelect";
            this.cboJobSelect.TabStop = false;
            this.cboJobSelect.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // lblDetailHeader
            // 
            resources.ApplyResources(this.lblDetailHeader, "lblDetailHeader");
            this.lblDetailHeader.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblDetailHeader.Name = "lblDetailHeader";
            // 
            // lblLastScanned
            // 
            resources.ApplyResources(this.lblLastScanned, "lblLastScanned");
            this.lblLastScanned.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblLastScanned.Name = "lblLastScanned";
            // 
            // lblSelectJob
            // 
            resources.ApplyResources(this.lblSelectJob, "lblSelectJob");
            this.lblSelectJob.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblSelectJob.Name = "lblSelectJob";
            // 
            // tabMapped
            // 
            this.tabMapped.BackColor = System.Drawing.Color.White;
            this.tabMapped.Controls.Add(this.panel3);
            this.tabMapped.Controls.Add(this.panel2);
            this.tabMapped.Controls.Add(this.pnlMappedStats);
            this.tabMapped.Controls.Add(this.pnlMappedTop);
            resources.ApplyResources(this.tabMapped, "tabMapped");
            this.tabMapped.Name = "tabMapped";
            this.tabMapped.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.rtbMappedDetail);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // rtbMappedDetail
            // 
            this.rtbMappedDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.rtbMappedDetail, "rtbMappedDetail");
            this.rtbMappedDetail.ForeColor = System.Drawing.Color.Gray;
            this.rtbMappedDetail.Name = "rtbMappedDetail";
            this.rtbMappedDetail.ReadOnly = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dgvMapped);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // dgvMapped
            // 
            this.dgvMapped.AllowUserToAddRows = false;
            this.dgvMapped.AllowUserToDeleteRows = false;
            this.dgvMapped.AllowUserToResizeRows = false;
            this.dgvMapped.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMapped.BackgroundColor = System.Drawing.Color.White;
            this.dgvMapped.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.dgvMapped, "dgvMapped");
            this.dgvMapped.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvMapped.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMTime,
            this.colMCode,
            this.colMResult,
            this.colMDetail});
            this.dgvMapped.EnableHeadersVisualStyles = false;
            this.dgvMapped.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.dgvMapped.Name = "dgvMapped";
            this.dgvMapped.ReadOnly = true;
            this.dgvMapped.RowHeadersVisible = false;
            this.dgvMapped.RowTemplate.Height = 35;
            this.dgvMapped.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMapped.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMapped_CellContentClick);
            // 
            // colMTime
            // 
            this.colMTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            resources.ApplyResources(this.colMTime, "colMTime");
            this.colMTime.Name = "colMTime";
            this.colMTime.ReadOnly = true;
            // 
            // colMCode
            // 
            this.colMCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.colMCode, "colMCode");
            this.colMCode.Name = "colMCode";
            this.colMCode.ReadOnly = true;
            // 
            // colMResult
            // 
            this.colMResult.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            resources.ApplyResources(this.colMResult, "colMResult");
            this.colMResult.Name = "colMResult";
            this.colMResult.ReadOnly = true;
            // 
            // colMDetail
            // 
            this.colMDetail.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.colMDetail, "colMDetail");
            this.colMDetail.Name = "colMDetail";
            this.colMDetail.ReadOnly = true;
            // 
            // pnlMappedStats
            // 
            this.pnlMappedStats.BackColor = System.Drawing.Color.Transparent;
            this.pnlMappedStats.Controls.Add(this.lblMappedRefLabel);
            this.pnlMappedStats.Controls.Add(this.lblMappedRefDisplay);
            resources.ApplyResources(this.pnlMappedStats, "pnlMappedStats");
            this.pnlMappedStats.Name = "pnlMappedStats";
            // 
            // lblMappedRefLabel
            // 
            resources.ApplyResources(this.lblMappedRefLabel, "lblMappedRefLabel");
            this.lblMappedRefLabel.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblMappedRefLabel.Name = "lblMappedRefLabel";
            // 
            // lblMappedRefDisplay
            // 
            resources.ApplyResources(this.lblMappedRefDisplay, "lblMappedRefDisplay");
            this.lblMappedRefDisplay.ForeColor = System.Drawing.Color.Gray;
            this.lblMappedRefDisplay.Name = "lblMappedRefDisplay";
            // 
            // pnlMappedTop
            // 
            this.pnlMappedTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.pnlMappedTop.Controls.Add(this.btnMappedReset);
            this.pnlMappedTop.Controls.Add(this.pnlJobSelectRow);
            resources.ApplyResources(this.pnlMappedTop, "pnlMappedTop");
            this.pnlMappedTop.Name = "pnlMappedTop";
            // 
            // btnMappedReset
            // 
            this.btnMappedReset._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnMappedReset._BorderRadius = 20;
            this.btnMappedReset._BorderSize = 1;
            this.btnMappedReset._GradientsButton = false;
            this.btnMappedReset._Text = "Reset";
            this.btnMappedReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnMappedReset.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnMappedReset.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnMappedReset.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnMappedReset.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnMappedReset, "btnMappedReset");
            this.btnMappedReset.ForeColor = System.Drawing.Color.White;
            this.btnMappedReset.Name = "btnMappedReset";
            this.btnMappedReset.TextColor = System.Drawing.Color.White;
            this.btnMappedReset.UseVisualStyleBackColor = false;
            // 
            // pnlJobSelectRow
            // 
            this.pnlJobSelectRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.pnlJobSelectRow.Controls.Add(this.cboMappedJobSelect);
            this.pnlJobSelectRow.Controls.Add(this.lblMappedJobSelectLabel);
            this.pnlJobSelectRow.Controls.Add(this.lblMappedPassCount);
            this.pnlJobSelectRow.Controls.Add(this.lblMappedFailCount);
            this.pnlJobSelectRow.Controls.Add(this.lblMappedTotalCount);
            resources.ApplyResources(this.pnlJobSelectRow, "pnlJobSelectRow");
            this.pnlJobSelectRow.Name = "pnlJobSelectRow";
            // 
            // cboMappedJobSelect
            // 
            this.cboMappedJobSelect.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.cboMappedJobSelect.FormattingEnabled = true;
            resources.ApplyResources(this.cboMappedJobSelect, "cboMappedJobSelect");
            this.cboMappedJobSelect.Name = "cboMappedJobSelect";
            this.cboMappedJobSelect.TabStop = false;
            // 
            // lblMappedJobSelectLabel
            // 
            resources.ApplyResources(this.lblMappedJobSelectLabel, "lblMappedJobSelectLabel");
            this.lblMappedJobSelectLabel.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblMappedJobSelectLabel.Name = "lblMappedJobSelectLabel";
            // 
            // lblMappedPassCount
            // 
            resources.ApplyResources(this.lblMappedPassCount, "lblMappedPassCount");
            this.lblMappedPassCount.ForeColor = System.Drawing.Color.Green;
            this.lblMappedPassCount.Name = "lblMappedPassCount";
            // 
            // lblMappedFailCount
            // 
            resources.ApplyResources(this.lblMappedFailCount, "lblMappedFailCount");
            this.lblMappedFailCount.ForeColor = System.Drawing.Color.Red;
            this.lblMappedFailCount.Name = "lblMappedFailCount";
            // 
            // lblMappedTotalCount
            // 
            resources.ApplyResources(this.lblMappedTotalCount, "lblMappedTotalCount");
            this.lblMappedTotalCount.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblMappedTotalCount.Name = "lblMappedTotalCount";
            // 
            // splitMapped
            // 
            resources.ApplyResources(this.splitMapped, "splitMapped");
            this.splitMapped.Name = "splitMapped";
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.tabControl1);
            this.pnlMain.Controls.Add(this.statusStrip2);
            this.pnlMain.Controls.Add(this.pnlMenu);
            resources.ApplyResources(this.pnlMain, "pnlMain");
            this.pnlMain.Name = "pnlMain";
            // 
            // dataGridViewTextBoxColumn1
            // 
            resources.ApplyResources(this.dataGridViewTextBoxColumn1, "dataGridViewTextBoxColumn1");
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            resources.ApplyResources(this.dataGridViewTextBoxColumn2, "dataGridViewTextBoxColumn2");
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            resources.ApplyResources(this.dataGridViewTextBoxColumn3, "dataGridViewTextBoxColumn3");
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            resources.ApplyResources(this.dataGridViewTextBoxColumn4, "dataGridViewTextBoxColumn4");
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // dataGridViewTextBoxColumn5
            // 
            resources.ApplyResources(this.dataGridViewTextBoxColumn5, "dataGridViewTextBoxColumn5");
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // dataGridViewTextBoxColumn6
            // 
            resources.ApplyResources(this.dataGridViewTextBoxColumn6, "dataGridViewTextBoxColumn6");
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            // 
            // dataGridViewTextBoxColumn7
            // 
            resources.ApplyResources(this.dataGridViewTextBoxColumn7, "dataGridViewTextBoxColumn7");
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            // 
            // mySqlCommand1
            // 
            this.mySqlCommand1.CacheAge = 0;
            this.mySqlCommand1.Connection = null;
            this.mySqlCommand1.EnableCaching = false;
            this.mySqlCommand1.Transaction = null;
            // 
            // FrmJobDroco
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmJobDroco";
            this.pnlMenu.ResumeLayout(false);
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lblLoadingSSCC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblLoadingHonest)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cartonsInPalletNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.boxesInCartonNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.codesInBoxNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageLoading)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.cuzPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).EndInit();
            this.pnlJobInfomation.ResumeLayout(false);
            this.pnlJobInfomation.PerformLayout();
            this.tabReCheck.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCheckResult)).EndInit();
            this.tabMapped.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMapped)).EndInit();
            this.pnlMappedStats.ResumeLayout(false);
            this.pnlMappedStats.PerformLayout();
            this.pnlMappedTop.ResumeLayout(false);
            this.pnlJobSelectRow.ResumeLayout(false);
            this.pnlJobSelectRow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMapped)).EndInit();
            this.splitMapped.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DesignUI.CuzUI.CuzDragControl DragControl;
        private DesignUI.CuzUI.CuzPanel pnlMenu;
        private FontAwesome.Sharp.IconButton btnAbout;
        private FontAwesome.Sharp.IconButton btnSettings;
        private FontAwesome.Sharp.IconButton btnExit;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusCamera01;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusSerialDevice;
        private System.Windows.Forms.ToolStripStatusLabel lblSensorControllerStatus;
        internal System.Windows.Forms.ToolStripStatusLabel lblToolStripVersion;
        private System.Windows.Forms.ToolStripStatusLabel toolStripDateTime;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private DesignUI.CuzUI.CuzPanel cuzPanel3;
        private System.Windows.Forms.PictureBox picLoading;
        private DesignUI.CuzUI.CuzButton btnDelete;
        private DesignUI.CuzUI.CuzButton btnNext;
        private DesignUI.CuzUI.CuzTextBox txtSearch;
        private System.Windows.Forms.Button btnSearchJob;
        private System.Windows.Forms.Label lblRLinkSeries;
        private DesignUI.CuzUI.CuzButton btnRefesh;
        private System.Windows.Forms.Label lblStandalone;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label lblImportDatabase;
        private DesignUI.CuzUI.CuzButton btnSave;
        private DesignUI.CuzUI.CuzButton btnImportDatabase;
        private DesignUI.CuzUI.CuzTextBox txtDirectoryDatabse;
        private System.Windows.Forms.Panel pnlMain;
        private DesignUI.CuzUI.RoundPanel pnlJobInfomation;
        private DesignUI.CuzUI.CuzTextBox txtJobStatus;
        private System.Windows.Forms.Label lblJobStatus;
        private DesignUI.CuzUI.CuzTextBox txtJobType;
        private System.Windows.Forms.Label lblJobType;
        private DesignUI.CuzUI.CuzTextBox lblTemplatePrintInfo;
        private DesignUI.CuzUI.CuzTextBox lblPODFormatInfo;
        private DesignUI.CuzUI.CuzTextBox lblStaticTextInfo;
        private System.Windows.Forms.Label lblTemplatePrint;
        private System.Windows.Forms.Label lblPODFormat;
        private System.Windows.Forms.Label lblStaticText1;
        private DesignUI.CuzUI.CuzTextBox lblCompareTypeInfo;
        private System.Windows.Forms.Label lblCompareType;
        private DesignUI.CuzUI.CuzTextBox lblJobNameInfo;
        private System.Windows.Forms.Label lblJobName;
        private DesignUI.CuzUI.CuzListBox listBoxJobList;
        private DesignUI.CuzUI.CuzButton cuzButtonPurge;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusPrinter01;
        private FontAwesome.Sharp.IconButton btnHelp;
        private FontAwesome.Sharp.IconButton btnRestart;
        private System.Windows.Forms.CheckBox FirstRowHeader;
        private System.Windows.Forms.CheckBox FirstRowHeaderSscc;
        private System.Windows.Forms.Label numberOfCodes;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.Label labelProductInBox;
        private System.Windows.Forms.NumericUpDown codesInBoxNum;
        private System.Windows.Forms.PictureBox imageLoading;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusPrinterZebra;
        private System.Windows.Forms.NumericUpDown cartonsInPalletNum;
        private System.Windows.Forms.NumericUpDown boxesInCartonNum;
        private System.Windows.Forms.Label labelCartonsInPallet;
        private System.Windows.Forms.Label labelBoxesInCarton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBatchNumber;
        private System.Windows.Forms.TextBox txtModel;
        private System.Windows.Forms.TextBox txtSalesOrder;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private DesignUI.CuzUI.CuzTextBox txtDirectoryDatabseCodeSSCC;
        private DesignUI.CuzUI.CuzButton btnImportDatabaseSSCC;
        private System.Windows.Forms.TabPage tabReCheck;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cboJobSelect;
        private System.Windows.Forms.Label lblSelectJob;
        private System.Windows.Forms.DataGridView dgvCheckResult;
        private System.Windows.Forms.RichTextBox rtbDetail;
        private System.Windows.Forms.Label lblDetailHeader;
        private System.Windows.Forms.Label lblLastScanned;
        private DesignUI.CuzUI.CuzButton btnClearGrid;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCarton;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPallet;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJob;
        private System.Windows.Forms.TabPage tabMapped;
        private System.Windows.Forms.Panel pnlMappedTop;
        private System.Windows.Forms.Label lblMappedRefDisplay;
        private System.Windows.Forms.Panel pnlMappedStats;
        private System.Windows.Forms.Label lblMappedPassCount;
        private System.Windows.Forms.Label lblMappedFailCount;
        private System.Windows.Forms.Label lblMappedTotalCount;
        private System.Windows.Forms.SplitContainer splitMapped;
        private System.Windows.Forms.DataGridView dgvMapped;
        private System.Windows.Forms.RichTextBox rtbMappedDetail;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private DesignUI.CuzUI.CuzButton btnMappedReset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMDetail;
        private System.Windows.Forms.Panel pnlJobSelectRow;
        private System.Windows.Forms.Label lblMappedJobSelectLabel;
        private MySql.Data.MySqlClient.MySqlCommand mySqlCommand1;
        private Label lblMappedRefLabel;
        private ComboBox cboMappedJobSelect;
        private PictureBox lblLoadingSSCC;
        private PictureBox lblLoadingHonest;
    }

}