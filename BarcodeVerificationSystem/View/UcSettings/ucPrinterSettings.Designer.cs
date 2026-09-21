namespace BarcodeVerificationSystem.View
{
    partial class UcPrinterSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grbPrinter = new System.Windows.Forms.GroupBox();
            this.groupBoxPOD = new System.Windows.Forms.GroupBox();
            this.numPrinterPort = new System.Windows.Forms.NumericUpDown();
            this.lblIPrinterIP = new System.Windows.Forms.Label();
            this.lblPrinterOperSys = new System.Windows.Forms.Label();
            this.lblPrinterPort = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.radSupported = new System.Windows.Forms.RadioButton();
            this.radUnsuported = new System.Windows.Forms.RadioButton();
            this.txtPrinterIP = new IPAddressControlLib.IPAddressControl();
            this.btnSetupPrinter = new System.Windows.Forms.Button();
            this.numPortRemote = new System.Windows.Forms.NumericUpDown();
            this.lblPortRemote = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TemplateControl = new System.Windows.Forms.Panel();
            this.TemplateTextBox = new System.Windows.Forms.TextBox();
            this.labelCommandError = new System.Windows.Forms.Label();
            this.splitCombo = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tableLayoutPanelNumBuffer1St = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownNumBuff1St = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanelTimeDelay1stSend = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownTimeDelay1st = new System.Windows.Forms.NumericUpDown();
            this.labelMissedStop = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.radioEnButtonMissedStop = new System.Windows.Forms.RadioButton();
            this.radioDisButtonMissedStop = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.radioButtonEnTurboSpeed = new System.Windows.Forms.RadioButton();
            this.radioButtonDisSendIndep = new System.Windows.Forms.RadioButton();
            this.labelSendInde = new System.Windows.Forms.Label();
            this.grbPrinter.SuspendLayout();
            this.groupBoxPOD.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPrinterPort)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPortRemote)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.TemplateControl.SuspendLayout();
            this.tableLayoutPanelNumBuffer1St.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumBuff1St)).BeginInit();
            this.tableLayoutPanelTimeDelay1stSend.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeDelay1st)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // grbPrinter
            // 
            this.grbPrinter.BackColor = System.Drawing.Color.White;
            this.grbPrinter.Controls.Add(this.groupBoxPOD);
            this.grbPrinter.Controls.Add(this.groupBox1);
            this.grbPrinter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbPrinter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbPrinter.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.grbPrinter.Location = new System.Drawing.Point(0, 0);
            this.grbPrinter.Name = "grbPrinter";
            this.grbPrinter.Size = new System.Drawing.Size(996, 592);
            this.grbPrinter.TabIndex = 20;
            this.grbPrinter.TabStop = false;
            this.grbPrinter.Text = "Printer";
            // 
            // groupBoxPOD
            // 
            this.groupBoxPOD.Controls.Add(this.numPrinterPort);
            this.groupBoxPOD.Controls.Add(this.lblIPrinterIP);
            this.groupBoxPOD.Controls.Add(this.lblPrinterOperSys);
            this.groupBoxPOD.Controls.Add(this.lblPrinterPort);
            this.groupBoxPOD.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxPOD.Controls.Add(this.txtPrinterIP);
            this.groupBoxPOD.Controls.Add(this.btnSetupPrinter);
            this.groupBoxPOD.Controls.Add(this.numPortRemote);
            this.groupBoxPOD.Controls.Add(this.lblPortRemote);
            this.groupBoxPOD.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxPOD.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBoxPOD.Location = new System.Drawing.Point(6, 25);
            this.groupBoxPOD.Name = "groupBoxPOD";
            this.groupBoxPOD.Size = new System.Drawing.Size(488, 540);
            this.groupBoxPOD.TabIndex = 55;
            this.groupBoxPOD.TabStop = false;
            this.groupBoxPOD.Text = "POD Protocol";
            // 
            // numPrinterPort
            // 
            this.numPrinterPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numPrinterPort.Location = new System.Drawing.Point(303, 56);
            this.numPrinterPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPrinterPort.Name = "numPrinterPort";
            this.numPrinterPort.Size = new System.Drawing.Size(139, 26);
            this.numPrinterPort.TabIndex = 37;
            // 
            // lblIPrinterIP
            // 
            this.lblIPrinterIP.AutoSize = true;
            this.lblIPrinterIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIPrinterIP.Location = new System.Drawing.Point(11, 28);
            this.lblIPrinterIP.Name = "lblIPrinterIP";
            this.lblIPrinterIP.Size = new System.Drawing.Size(87, 20);
            this.lblIPrinterIP.TabIndex = 21;
            this.lblIPrinterIP.Text = "IP Address";
            // 
            // lblPrinterOperSys
            // 
            this.lblPrinterOperSys.AutoSize = true;
            this.lblPrinterOperSys.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrinterOperSys.Location = new System.Drawing.Point(11, 326);
            this.lblPrinterOperSys.Name = "lblPrinterOperSys";
            this.lblPrinterOperSys.Size = new System.Drawing.Size(182, 20);
            this.lblPrinterOperSys.TabIndex = 45;
            this.lblPrinterOperSys.Text = "Check all printer settings";
            // 
            // lblPrinterPort
            // 
            this.lblPrinterPort.AutoSize = true;
            this.lblPrinterPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrinterPort.Location = new System.Drawing.Point(299, 28);
            this.lblPrinterPort.Name = "lblPrinterPort";
            this.lblPrinterPort.Size = new System.Drawing.Size(38, 20);
            this.lblPrinterPort.TabIndex = 25;
            this.lblPrinterPort.Text = "Port";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.radSupported, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.radUnsuported, 1, 0);
            this.tableLayoutPanel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableLayoutPanel2.Location = new System.Drawing.Point(15, 356);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(440, 38);
            this.tableLayoutPanel2.TabIndex = 44;
            // 
            // radSupported
            // 
            this.radSupported.Appearance = System.Windows.Forms.Appearance.Button;
            this.radSupported.BackColor = System.Drawing.Color.White;
            this.radSupported.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radSupported.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radSupported.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radSupported.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radSupported.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radSupported.Location = new System.Drawing.Point(0, 0);
            this.radSupported.Margin = new System.Windows.Forms.Padding(0);
            this.radSupported.Name = "radSupported";
            this.radSupported.Size = new System.Drawing.Size(220, 38);
            this.radSupported.TabIndex = 3;
            this.radSupported.TabStop = true;
            this.radSupported.Text = "Enable";
            this.radSupported.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radSupported.UseVisualStyleBackColor = false;
            // 
            // radUnsuported
            // 
            this.radUnsuported.Appearance = System.Windows.Forms.Appearance.Button;
            this.radUnsuported.BackColor = System.Drawing.Color.White;
            this.radUnsuported.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radUnsuported.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radUnsuported.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radUnsuported.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radUnsuported.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radUnsuported.Location = new System.Drawing.Point(220, 0);
            this.radUnsuported.Margin = new System.Windows.Forms.Padding(0);
            this.radUnsuported.Name = "radUnsuported";
            this.radUnsuported.Size = new System.Drawing.Size(220, 38);
            this.radUnsuported.TabIndex = 4;
            this.radUnsuported.TabStop = true;
            this.radUnsuported.Text = "Disable";
            this.radUnsuported.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radUnsuported.UseVisualStyleBackColor = false;
            // 
            // txtPrinterIP
            // 
            this.txtPrinterIP.AllowInternalTab = false;
            this.txtPrinterIP.AutoHeight = true;
            this.txtPrinterIP.BackColor = System.Drawing.SystemColors.Window;
            this.txtPrinterIP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtPrinterIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPrinterIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPrinterIP.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtPrinterIP.Location = new System.Drawing.Point(14, 55);
            this.txtPrinterIP.MinimumSize = new System.Drawing.Size(126, 26);
            this.txtPrinterIP.Name = "txtPrinterIP";
            this.txtPrinterIP.ReadOnly = false;
            this.txtPrinterIP.Size = new System.Drawing.Size(265, 26);
            this.txtPrinterIP.TabIndex = 32;
            this.txtPrinterIP.Text = "...";
            // 
            // btnSetupPrinter
            // 
            this.btnSetupPrinter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnSetupPrinter.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnSetupPrinter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetupPrinter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetupPrinter.ForeColor = System.Drawing.Color.White;
            this.btnSetupPrinter.Location = new System.Drawing.Point(15, 225);
            this.btnSetupPrinter.Name = "btnSetupPrinter";
            this.btnSetupPrinter.Size = new System.Drawing.Size(428, 57);
            this.btnSetupPrinter.TabIndex = 41;
            this.btnSetupPrinter.Text = "Remote Printer Settings";
            this.btnSetupPrinter.UseVisualStyleBackColor = false;
            // 
            // numPortRemote
            // 
            this.numPortRemote.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numPortRemote.Location = new System.Drawing.Point(15, 143);
            this.numPortRemote.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPortRemote.Name = "numPortRemote";
            this.numPortRemote.Size = new System.Drawing.Size(428, 26);
            this.numPortRemote.TabIndex = 37;
            this.numPortRemote.Value = new decimal(new int[] {
            8080,
            0,
            0,
            0});
            // 
            // lblPortRemote
            // 
            this.lblPortRemote.AutoSize = true;
            this.lblPortRemote.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPortRemote.Location = new System.Drawing.Point(11, 114);
            this.lblPortRemote.Name = "lblPortRemote";
            this.lblPortRemote.Size = new System.Drawing.Size(99, 20);
            this.lblPortRemote.TabIndex = 25;
            this.lblPortRemote.Text = "Remote Port";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TemplateControl);
            this.groupBox1.Controls.Add(this.splitCombo);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.tableLayoutPanelNumBuffer1St);
            this.groupBox1.Controls.Add(this.tableLayoutPanelTimeDelay1stSend);
            this.groupBox1.Controls.Add(this.labelMissedStop);
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Controls.Add(this.tableLayoutPanel4);
            this.groupBox1.Controls.Add(this.labelSendInde);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBox1.Location = new System.Drawing.Point(500, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(478, 540);
            this.groupBox1.TabIndex = 50;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "POD Send Mode";
            // 
            // TemplateControl
            // 
            this.TemplateControl.Controls.Add(this.TemplateTextBox);
            this.TemplateControl.Controls.Add(this.labelCommandError);
            this.TemplateControl.Location = new System.Drawing.Point(6, 197);
            this.TemplateControl.Name = "TemplateControl";
            this.TemplateControl.Size = new System.Drawing.Size(466, 53);
            this.TemplateControl.TabIndex = 72;
            this.TemplateControl.Visible = false;
            // 
            // TemplateTextBox
            // 
            this.TemplateTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TemplateTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TemplateTextBox.Location = new System.Drawing.Point(76, 12);
            this.TemplateTextBox.MinimumSize = new System.Drawing.Size(361, 30);
            this.TemplateTextBox.Name = "TemplateTextBox";
            this.TemplateTextBox.Size = new System.Drawing.Size(377, 26);
            this.TemplateTextBox.TabIndex = 68;
            // 
            // labelCommandError
            // 
            this.labelCommandError.AutoSize = true;
            this.labelCommandError.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCommandError.Location = new System.Drawing.Point(11, 19);
            this.labelCommandError.Name = "labelCommandError";
            this.labelCommandError.Size = new System.Drawing.Size(59, 20);
            this.labelCommandError.TabIndex = 67;
            this.labelCommandError.Text = "Bản tin";
            // 
            // splitCombo
            // 
            this.splitCombo.BackColor = System.Drawing.SystemColors.Window;
            this.splitCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.splitCombo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.splitCombo.FormattingEnabled = true;
            this.splitCombo.ItemHeight = 20;
            this.splitCombo.Location = new System.Drawing.Point(17, 146);
            this.splitCombo.Name = "splitCombo";
            this.splitCombo.Size = new System.Drawing.Size(220, 28);
            this.splitCombo.TabIndex = 56;
            this.splitCombo.SelectedIndexChanged += new System.EventHandler(this.splitCombo_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(17, 114);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 20);
            this.label5.TabIndex = 55;
            this.label5.Text = "Split character";
            // 
            // tableLayoutPanelNumBuffer1St
            // 
            this.tableLayoutPanelNumBuffer1St.ColumnCount = 3;
            this.tableLayoutPanelNumBuffer1St.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelNumBuffer1St.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 153F));
            this.tableLayoutPanelNumBuffer1St.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 99F));
            this.tableLayoutPanelNumBuffer1St.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanelNumBuffer1St.Controls.Add(this.label4, 2, 0);
            this.tableLayoutPanelNumBuffer1St.Controls.Add(this.numericUpDownNumBuff1St, 1, 0);
            this.tableLayoutPanelNumBuffer1St.Location = new System.Drawing.Point(14, 488);
            this.tableLayoutPanelNumBuffer1St.Name = "tableLayoutPanelNumBuffer1St";
            this.tableLayoutPanelNumBuffer1St.RowCount = 1;
            this.tableLayoutPanelNumBuffer1St.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelNumBuffer1St.Size = new System.Drawing.Size(440, 39);
            this.tableLayoutPanelNumBuffer1St.TabIndex = 54;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 20);
            this.label3.TabIndex = 50;
            this.label3.Text = "Number Bufer 1St";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(344, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 20);
            this.label4.TabIndex = 52;
            this.label4.Text = "Buffer";
            // 
            // numericUpDownNumBuff1St
            // 
            this.numericUpDownNumBuff1St.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericUpDownNumBuff1St.Location = new System.Drawing.Point(191, 3);
            this.numericUpDownNumBuff1St.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDownNumBuff1St.Name = "numericUpDownNumBuff1St";
            this.numericUpDownNumBuff1St.Size = new System.Drawing.Size(89, 26);
            this.numericUpDownNumBuff1St.TabIndex = 51;
            // 
            // tableLayoutPanelTimeDelay1stSend
            // 
            this.tableLayoutPanelTimeDelay1stSend.ColumnCount = 3;
            this.tableLayoutPanelTimeDelay1stSend.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelTimeDelay1stSend.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 153F));
            this.tableLayoutPanelTimeDelay1stSend.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 99F));
            this.tableLayoutPanelTimeDelay1stSend.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanelTimeDelay1stSend.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanelTimeDelay1stSend.Controls.Add(this.numericUpDownTimeDelay1st, 1, 0);
            this.tableLayoutPanelTimeDelay1stSend.Location = new System.Drawing.Point(14, 443);
            this.tableLayoutPanelTimeDelay1stSend.Name = "tableLayoutPanelTimeDelay1stSend";
            this.tableLayoutPanelTimeDelay1stSend.RowCount = 1;
            this.tableLayoutPanelTimeDelay1stSend.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelTimeDelay1stSend.Size = new System.Drawing.Size(440, 39);
            this.tableLayoutPanelTimeDelay1stSend.TabIndex = 53;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 20);
            this.label1.TabIndex = 50;
            this.label1.Text = "Time delay 1st Send";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(344, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 20);
            this.label2.TabIndex = 52;
            this.label2.Text = "ms";
            // 
            // numericUpDownTimeDelay1st
            // 
            this.numericUpDownTimeDelay1st.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericUpDownTimeDelay1st.Location = new System.Drawing.Point(191, 3);
            this.numericUpDownTimeDelay1st.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDownTimeDelay1st.Name = "numericUpDownTimeDelay1st";
            this.numericUpDownTimeDelay1st.Size = new System.Drawing.Size(89, 26);
            this.numericUpDownTimeDelay1st.TabIndex = 51;
            // 
            // labelMissedStop
            // 
            this.labelMissedStop.AutoSize = true;
            this.labelMissedStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMissedStop.Location = new System.Drawing.Point(17, 302);
            this.labelMissedStop.Name = "labelMissedStop";
            this.labelMissedStop.Size = new System.Drawing.Size(192, 20);
            this.labelMissedStop.TabIndex = 49;
            this.labelMissedStop.Text = "Missed Printed Page Stop";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.radioEnButtonMissedStop, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.radioDisButtonMissedStop, 1, 0);
            this.tableLayoutPanel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(17, 333);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(440, 38);
            this.tableLayoutPanel1.TabIndex = 48;
            // 
            // radioEnButtonMissedStop
            // 
            this.radioEnButtonMissedStop.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioEnButtonMissedStop.BackColor = System.Drawing.Color.White;
            this.radioEnButtonMissedStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioEnButtonMissedStop.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radioEnButtonMissedStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioEnButtonMissedStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioEnButtonMissedStop.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioEnButtonMissedStop.Location = new System.Drawing.Point(0, 0);
            this.radioEnButtonMissedStop.Margin = new System.Windows.Forms.Padding(0);
            this.radioEnButtonMissedStop.Name = "radioEnButtonMissedStop";
            this.radioEnButtonMissedStop.Size = new System.Drawing.Size(220, 38);
            this.radioEnButtonMissedStop.TabIndex = 3;
            this.radioEnButtonMissedStop.TabStop = true;
            this.radioEnButtonMissedStop.Text = "Enable";
            this.radioEnButtonMissedStop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioEnButtonMissedStop.UseVisualStyleBackColor = false;
            // 
            // radioDisButtonMissedStop
            // 
            this.radioDisButtonMissedStop.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioDisButtonMissedStop.BackColor = System.Drawing.Color.White;
            this.radioDisButtonMissedStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioDisButtonMissedStop.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radioDisButtonMissedStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioDisButtonMissedStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioDisButtonMissedStop.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioDisButtonMissedStop.Location = new System.Drawing.Point(220, 0);
            this.radioDisButtonMissedStop.Margin = new System.Windows.Forms.Padding(0);
            this.radioDisButtonMissedStop.Name = "radioDisButtonMissedStop";
            this.radioDisButtonMissedStop.Size = new System.Drawing.Size(220, 38);
            this.radioDisButtonMissedStop.TabIndex = 4;
            this.radioDisButtonMissedStop.TabStop = true;
            this.radioDisButtonMissedStop.Text = "Disable";
            this.radioDisButtonMissedStop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioDisButtonMissedStop.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Controls.Add(this.radioButtonEnTurboSpeed, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.radioButtonDisSendIndep, 1, 0);
            this.tableLayoutPanel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableLayoutPanel4.Location = new System.Drawing.Point(17, 58);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(440, 38);
            this.tableLayoutPanel4.TabIndex = 47;
            // 
            // radioButtonEnTurboSpeed
            // 
            this.radioButtonEnTurboSpeed.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioButtonEnTurboSpeed.BackColor = System.Drawing.Color.White;
            this.radioButtonEnTurboSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButtonEnTurboSpeed.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radioButtonEnTurboSpeed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioButtonEnTurboSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonEnTurboSpeed.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonEnTurboSpeed.Location = new System.Drawing.Point(0, 0);
            this.radioButtonEnTurboSpeed.Margin = new System.Windows.Forms.Padding(0);
            this.radioButtonEnTurboSpeed.Name = "radioButtonEnTurboSpeed";
            this.radioButtonEnTurboSpeed.Size = new System.Drawing.Size(220, 38);
            this.radioButtonEnTurboSpeed.TabIndex = 3;
            this.radioButtonEnTurboSpeed.TabStop = true;
            this.radioButtonEnTurboSpeed.Text = "Enable";
            this.radioButtonEnTurboSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioButtonEnTurboSpeed.UseVisualStyleBackColor = false;
            // 
            // radioButtonDisSendIndep
            // 
            this.radioButtonDisSendIndep.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioButtonDisSendIndep.BackColor = System.Drawing.Color.White;
            this.radioButtonDisSendIndep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButtonDisSendIndep.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radioButtonDisSendIndep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioButtonDisSendIndep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonDisSendIndep.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonDisSendIndep.Location = new System.Drawing.Point(220, 0);
            this.radioButtonDisSendIndep.Margin = new System.Windows.Forms.Padding(0);
            this.radioButtonDisSendIndep.Name = "radioButtonDisSendIndep";
            this.radioButtonDisSendIndep.Size = new System.Drawing.Size(220, 38);
            this.radioButtonDisSendIndep.TabIndex = 4;
            this.radioButtonDisSendIndep.TabStop = true;
            this.radioButtonDisSendIndep.Text = "Disable";
            this.radioButtonDisSendIndep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioButtonDisSendIndep.UseVisualStyleBackColor = false;
            // 
            // labelSendInde
            // 
            this.labelSendInde.AutoSize = true;
            this.labelSendInde.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSendInde.Location = new System.Drawing.Point(17, 31);
            this.labelSendInde.Name = "labelSendInde";
            this.labelSendInde.Size = new System.Drawing.Size(146, 20);
            this.labelSendInde.TabIndex = 46;
            this.labelSendInde.Text = "Sent independently";
            // 
            // UcPrinterSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grbPrinter);
            this.Name = "UcPrinterSettings";
            this.Size = new System.Drawing.Size(996, 592);
            this.grbPrinter.ResumeLayout(false);
            this.groupBoxPOD.ResumeLayout(false);
            this.groupBoxPOD.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPrinterPort)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numPortRemote)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.TemplateControl.ResumeLayout(false);
            this.TemplateControl.PerformLayout();
            this.tableLayoutPanelNumBuffer1St.ResumeLayout(false);
            this.tableLayoutPanelNumBuffer1St.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumBuff1St)).EndInit();
            this.tableLayoutPanelTimeDelay1stSend.ResumeLayout(false);
            this.tableLayoutPanelTimeDelay1stSend.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeDelay1st)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grbPrinter;
        private System.Windows.Forms.NumericUpDown numPrinterPort;
        private IPAddressControlLib.IPAddressControl txtPrinterIP;
        private System.Windows.Forms.Label lblPrinterPort;
        private System.Windows.Forms.Label lblIPrinterIP;
        private System.Windows.Forms.Button btnSetupPrinter;
        private System.Windows.Forms.NumericUpDown numPortRemote;
        private System.Windows.Forms.Label lblPortRemote;
        private System.Windows.Forms.Label lblPrinterOperSys;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton radSupported;
        private System.Windows.Forms.RadioButton radUnsuported;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.RadioButton radioButtonEnTurboSpeed;
        private System.Windows.Forms.RadioButton radioButtonDisSendIndep;
        private System.Windows.Forms.Label labelSendInde;
        private System.Windows.Forms.GroupBox groupBoxPOD;
        private System.Windows.Forms.Label labelMissedStop;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RadioButton radioEnButtonMissedStop;
        private System.Windows.Forms.RadioButton radioDisButtonMissedStop;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTimeDelay1stSend;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownTimeDelay1st;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelNumBuffer1St;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownNumBuff1St;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox splitCombo;
        private System.Windows.Forms.Panel TemplateControl;
        private System.Windows.Forms.Label labelCommandError;
        private System.Windows.Forms.TextBox TemplateTextBox;
    }
}
