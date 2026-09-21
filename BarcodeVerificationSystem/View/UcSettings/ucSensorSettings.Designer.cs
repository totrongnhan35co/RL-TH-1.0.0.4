namespace BarcodeVerificationSystem.View
{
    partial class UcSensorSettings
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
            this.components = new System.ComponentModel.Container();
            this.lblSensorControllerIP = new System.Windows.Forms.Label();
            this.lblSensorControllerPort = new System.Windows.Forms.Label();
            this.lblSensorControllerPulseEncoder = new System.Windows.Forms.Label();
            this.lblSensorControllerEncoderDiameter = new System.Windows.Forms.Label();
            this.txtSensorControllerIP = new IPAddressControlLib.IPAddressControl();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.radSensorControllerEnable = new System.Windows.Forms.RadioButton();
            this.radSensorControllerDisable = new System.Windows.Forms.RadioButton();
            this.numSensorControllerPort = new System.Windows.Forms.NumericUpDown();
            this.numSensorControllerPulseEncoder = new System.Windows.Forms.NumericUpDown();
            this.numSensorControllerEncoderDiameter = new System.Windows.Forms.NumericUpDown();
            this.lblSensorControllerDelayBefore = new System.Windows.Forms.Label();
            this.lblSensorControllerDelayAfter = new System.Windows.Forms.Label();
            this.numSensorControllerDelayBefore = new System.Windows.Forms.NumericUpDown();
            this.numSensorControllerDelayAfter = new System.Windows.Forms.NumericUpDown();
            this.ppr = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblContentResponse = new System.Windows.Forms.Label();
            this.richTXTContentResponse = new System.Windows.Forms.RichTextBox();
            this.btnContenResponseClear = new System.Windows.Forms.Button();
            this.grbSensorController = new System.Windows.Forms.GroupBox();
            this.btnProductList = new DesignUI.CuzUI.CuzButton();
            this.iconConnectPort1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.PassPLC = new System.Windows.Forms.Button();
            this.ErrorPLC = new System.Windows.Forms.Button();
            this.StopPLC = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.StartPLC = new System.Windows.Forms.Button();
            this.EncoderMode = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ExternalRad = new System.Windows.Forms.RadioButton();
            this.InternalRad = new System.Windows.Forms.RadioButton();
            this.radioV2 = new System.Windows.Forms.RadioButton();
            this.delayOutputPanel = new System.Windows.Forms.Panel();
            this.lblDelayOutputError = new System.Windows.Forms.Label();
            this.Delaymm = new System.Windows.Forms.Label();
            this.numericDelayOutputError = new System.Windows.Forms.NumericUpDown();
            this.radioV1 = new System.Windows.Forms.RadioButton();
            this.versionLabel = new System.Windows.Forms.Label();
            this.radioV0 = new System.Windows.Forms.RadioButton();
            this.comboPLCPort = new System.Windows.Forms.ComboBox();
            this.Port2Panel = new System.Windows.Forms.Panel();
            this.iconConnectPort2 = new System.Windows.Forms.PictureBox();
            this.numSensorControllerPort2 = new System.Windows.Forms.NumericUpDown();
            this.SendPort2 = new System.Windows.Forms.Button();
            this.grbResumeEncoder = new System.Windows.Forms.GroupBox();
            this.EnableResumeEncoder = new System.Windows.Forms.CheckBox();
            this.radioResumeA = new System.Windows.Forms.RadioButton();
            this.radioResumeAB = new System.Windows.Forms.RadioButton();
            this.sensorTwoBtn = new System.Windows.Forms.Button();
            this.sensorOneBtn = new System.Windows.Forms.Button();
            this.grBoxSensor = new System.Windows.Forms.GroupBox();
            this.OutputDelay = new System.Windows.Forms.Panel();
            this.labelDelayOut = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.numDelayOutput = new System.Windows.Forms.NumericUpDown();
            this.lblErrorCondition = new System.Windows.Forms.Label();
            this.lblUnit = new System.Windows.Forms.Label();
            this.lblGAP = new System.Windows.Forms.Label();
            this.numGapLength1 = new System.Windows.Forms.NumericUpDown();
            this.GAPmm = new System.Windows.Forms.Label();
            this.numLength2Error1 = new System.Windows.Forms.NumericUpDown();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.imageList3 = new System.Windows.Forms.ImageList(this.components);
            this.iconMenuItem1 = new FontAwesome.Sharp.IconMenuItem();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerPulseEncoder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerEncoderDiameter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerDelayBefore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerDelayAfter)).BeginInit();
            this.grbSensorController.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconConnectPort1)).BeginInit();
            this.panel1.SuspendLayout();
            this.EncoderMode.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.delayOutputPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericDelayOutputError)).BeginInit();
            this.Port2Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconConnectPort2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerPort2)).BeginInit();
            this.grbResumeEncoder.SuspendLayout();
            this.grBoxSensor.SuspendLayout();
            this.OutputDelay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDelayOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGapLength1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLength2Error1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSensorControllerIP
            // 
            this.lblSensorControllerIP.AutoSize = true;
            this.lblSensorControllerIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSensorControllerIP.Location = new System.Drawing.Point(21, 141);
            this.lblSensorControllerIP.Name = "lblSensorControllerIP";
            this.lblSensorControllerIP.Size = new System.Drawing.Size(85, 20);
            this.lblSensorControllerIP.TabIndex = 21;
            this.lblSensorControllerIP.Text = "IP address";
            // 
            // lblSensorControllerPort
            // 
            this.lblSensorControllerPort.AutoSize = true;
            this.lblSensorControllerPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSensorControllerPort.Location = new System.Drawing.Point(258, 141);
            this.lblSensorControllerPort.Name = "lblSensorControllerPort";
            this.lblSensorControllerPort.Size = new System.Drawing.Size(38, 20);
            this.lblSensorControllerPort.TabIndex = 25;
            this.lblSensorControllerPort.Text = "Port";
            // 
            // lblSensorControllerPulseEncoder
            // 
            this.lblSensorControllerPulseEncoder.AutoSize = true;
            this.lblSensorControllerPulseEncoder.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSensorControllerPulseEncoder.Location = new System.Drawing.Point(6, 81);
            this.lblSensorControllerPulseEncoder.Name = "lblSensorControllerPulseEncoder";
            this.lblSensorControllerPulseEncoder.Size = new System.Drawing.Size(110, 20);
            this.lblSensorControllerPulseEncoder.TabIndex = 27;
            this.lblSensorControllerPulseEncoder.Text = "Pulse encoder";
            // 
            // lblSensorControllerEncoderDiameter
            // 
            this.lblSensorControllerEncoderDiameter.AutoSize = true;
            this.lblSensorControllerEncoderDiameter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSensorControllerEncoderDiameter.Location = new System.Drawing.Point(252, 81);
            this.lblSensorControllerEncoderDiameter.Name = "lblSensorControllerEncoderDiameter";
            this.lblSensorControllerEncoderDiameter.Size = new System.Drawing.Size(135, 20);
            this.lblSensorControllerEncoderDiameter.TabIndex = 30;
            this.lblSensorControllerEncoderDiameter.Text = "Encoder diameter";
            // 
            // txtSensorControllerIP
            // 
            this.txtSensorControllerIP.AllowInternalTab = false;
            this.txtSensorControllerIP.AutoHeight = true;
            this.txtSensorControllerIP.BackColor = System.Drawing.SystemColors.Window;
            this.txtSensorControllerIP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtSensorControllerIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtSensorControllerIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSensorControllerIP.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtSensorControllerIP.Location = new System.Drawing.Point(24, 167);
            this.txtSensorControllerIP.MinimumSize = new System.Drawing.Size(126, 26);
            this.txtSensorControllerIP.Name = "txtSensorControllerIP";
            this.txtSensorControllerIP.ReadOnly = false;
            this.txtSensorControllerIP.Size = new System.Drawing.Size(213, 26);
            this.txtSensorControllerIP.TabIndex = 32;
            this.txtSensorControllerIP.Text = "...";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Controls.Add(this.radSensorControllerEnable, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.radSensorControllerDisable, 1, 0);
            this.tableLayoutPanel5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableLayoutPanel5.Location = new System.Drawing.Point(23, 77);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(439, 38);
            this.tableLayoutPanel5.TabIndex = 33;
            // 
            // radSensorControllerEnable
            // 
            this.radSensorControllerEnable.Appearance = System.Windows.Forms.Appearance.Button;
            this.radSensorControllerEnable.BackColor = System.Drawing.Color.White;
            this.radSensorControllerEnable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radSensorControllerEnable.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radSensorControllerEnable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radSensorControllerEnable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radSensorControllerEnable.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radSensorControllerEnable.Location = new System.Drawing.Point(0, 0);
            this.radSensorControllerEnable.Margin = new System.Windows.Forms.Padding(0);
            this.radSensorControllerEnable.Name = "radSensorControllerEnable";
            this.radSensorControllerEnable.Size = new System.Drawing.Size(219, 38);
            this.radSensorControllerEnable.TabIndex = 3;
            this.radSensorControllerEnable.TabStop = true;
            this.radSensorControllerEnable.Text = "Enable";
            this.radSensorControllerEnable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radSensorControllerEnable.UseVisualStyleBackColor = false;
            // 
            // radSensorControllerDisable
            // 
            this.radSensorControllerDisable.Appearance = System.Windows.Forms.Appearance.Button;
            this.radSensorControllerDisable.BackColor = System.Drawing.Color.White;
            this.radSensorControllerDisable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radSensorControllerDisable.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radSensorControllerDisable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radSensorControllerDisable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radSensorControllerDisable.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radSensorControllerDisable.Location = new System.Drawing.Point(219, 0);
            this.radSensorControllerDisable.Margin = new System.Windows.Forms.Padding(0);
            this.radSensorControllerDisable.Name = "radSensorControllerDisable";
            this.radSensorControllerDisable.Size = new System.Drawing.Size(220, 38);
            this.radSensorControllerDisable.TabIndex = 4;
            this.radSensorControllerDisable.TabStop = true;
            this.radSensorControllerDisable.Text = "Disable";
            this.radSensorControllerDisable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radSensorControllerDisable.UseVisualStyleBackColor = false;
            // 
            // numSensorControllerPort
            // 
            this.numSensorControllerPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numSensorControllerPort.Location = new System.Drawing.Point(262, 167);
            this.numSensorControllerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numSensorControllerPort.Name = "numSensorControllerPort";
            this.numSensorControllerPort.Size = new System.Drawing.Size(200, 26);
            this.numSensorControllerPort.TabIndex = 34;
            // 
            // numSensorControllerPulseEncoder
            // 
            this.numSensorControllerPulseEncoder.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numSensorControllerPulseEncoder.Location = new System.Drawing.Point(10, 108);
            this.numSensorControllerPulseEncoder.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numSensorControllerPulseEncoder.Name = "numSensorControllerPulseEncoder";
            this.numSensorControllerPulseEncoder.Size = new System.Drawing.Size(186, 26);
            this.numSensorControllerPulseEncoder.TabIndex = 35;
            // 
            // numSensorControllerEncoderDiameter
            // 
            this.numSensorControllerEncoderDiameter.DecimalPlaces = 2;
            this.numSensorControllerEncoderDiameter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numSensorControllerEncoderDiameter.Location = new System.Drawing.Point(256, 109);
            this.numSensorControllerEncoderDiameter.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numSensorControllerEncoderDiameter.Name = "numSensorControllerEncoderDiameter";
            this.numSensorControllerEncoderDiameter.Size = new System.Drawing.Size(186, 26);
            this.numSensorControllerEncoderDiameter.TabIndex = 36;
            // 
            // lblSensorControllerDelayBefore
            // 
            this.lblSensorControllerDelayBefore.AutoSize = true;
            this.lblSensorControllerDelayBefore.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSensorControllerDelayBefore.Location = new System.Drawing.Point(6, 19);
            this.lblSensorControllerDelayBefore.Name = "lblSensorControllerDelayBefore";
            this.lblSensorControllerDelayBefore.Size = new System.Drawing.Size(101, 20);
            this.lblSensorControllerDelayBefore.TabIndex = 37;
            this.lblSensorControllerDelayBefore.Text = "Delay sensor";
            // 
            // lblSensorControllerDelayAfter
            // 
            this.lblSensorControllerDelayAfter.AutoSize = true;
            this.lblSensorControllerDelayAfter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSensorControllerDelayAfter.Location = new System.Drawing.Point(252, 19);
            this.lblSensorControllerDelayAfter.Name = "lblSensorControllerDelayAfter";
            this.lblSensorControllerDelayAfter.Size = new System.Drawing.Size(114, 20);
            this.lblSensorControllerDelayAfter.TabIndex = 38;
            this.lblSensorControllerDelayAfter.Text = "Disable sensor";
            // 
            // numSensorControllerDelayBefore
            // 
            this.numSensorControllerDelayBefore.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numSensorControllerDelayBefore.Location = new System.Drawing.Point(10, 46);
            this.numSensorControllerDelayBefore.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numSensorControllerDelayBefore.Name = "numSensorControllerDelayBefore";
            this.numSensorControllerDelayBefore.Size = new System.Drawing.Size(186, 26);
            this.numSensorControllerDelayBefore.TabIndex = 39;
            // 
            // numSensorControllerDelayAfter
            // 
            this.numSensorControllerDelayAfter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numSensorControllerDelayAfter.Location = new System.Drawing.Point(256, 46);
            this.numSensorControllerDelayAfter.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numSensorControllerDelayAfter.Name = "numSensorControllerDelayAfter";
            this.numSensorControllerDelayAfter.Size = new System.Drawing.Size(186, 26);
            this.numSensorControllerDelayAfter.TabIndex = 40;
            // 
            // ppr
            // 
            this.ppr.AutoSize = true;
            this.ppr.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ppr.Location = new System.Drawing.Point(202, 111);
            this.ppr.Name = "ppr";
            this.ppr.Size = new System.Drawing.Size(32, 20);
            this.ppr.TabIndex = 41;
            this.ppr.Text = "ppr";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(202, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 20);
            this.label2.TabIndex = 42;
            this.label2.Text = "mm";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(448, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 20);
            this.label3.TabIndex = 43;
            this.label3.Text = "mm";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(448, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 20);
            this.label4.TabIndex = 44;
            this.label4.Text = "mm";
            // 
            // lblContentResponse
            // 
            this.lblContentResponse.AutoSize = true;
            this.lblContentResponse.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblContentResponse.Location = new System.Drawing.Point(21, 272);
            this.lblContentResponse.Name = "lblContentResponse";
            this.lblContentResponse.Size = new System.Drawing.Size(136, 20);
            this.lblContentResponse.TabIndex = 48;
            this.lblContentResponse.Text = "Content response";
            // 
            // richTXTContentResponse
            // 
            this.richTXTContentResponse.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTXTContentResponse.Location = new System.Drawing.Point(24, 295);
            this.richTXTContentResponse.Name = "richTXTContentResponse";
            this.richTXTContentResponse.ReadOnly = true;
            this.richTXTContentResponse.Size = new System.Drawing.Size(438, 143);
            this.richTXTContentResponse.TabIndex = 49;
            this.richTXTContentResponse.Text = "";
            this.richTXTContentResponse.TextChanged += new System.EventHandler(this.richTXTContentResponse_TextChanged);
            // 
            // btnContenResponseClear
            // 
            this.btnContenResponseClear.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnContenResponseClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnContenResponseClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnContenResponseClear.ForeColor = System.Drawing.SystemColors.WindowText;
            this.btnContenResponseClear.Location = new System.Drawing.Point(25, 233);
            this.btnContenResponseClear.Name = "btnContenResponseClear";
            this.btnContenResponseClear.Size = new System.Drawing.Size(186, 34);
            this.btnContenResponseClear.TabIndex = 50;
            this.btnContenResponseClear.Text = "Clear";
            this.btnContenResponseClear.UseVisualStyleBackColor = true;
            // 
            // grbSensorController
            // 
            this.grbSensorController.BackColor = System.Drawing.Color.White;
            this.grbSensorController.Controls.Add(this.btnProductList);
            this.grbSensorController.Controls.Add(this.iconConnectPort1);
            this.grbSensorController.Controls.Add(this.panel1);
            this.grbSensorController.Controls.Add(this.EncoderMode);
            this.grbSensorController.Controls.Add(this.radioV2);
            this.grbSensorController.Controls.Add(this.delayOutputPanel);
            this.grbSensorController.Controls.Add(this.radioV1);
            this.grbSensorController.Controls.Add(this.versionLabel);
            this.grbSensorController.Controls.Add(this.radioV0);
            this.grbSensorController.Controls.Add(this.comboPLCPort);
            this.grbSensorController.Controls.Add(this.Port2Panel);
            this.grbSensorController.Controls.Add(this.grbResumeEncoder);
            this.grbSensorController.Controls.Add(this.sensorTwoBtn);
            this.grbSensorController.Controls.Add(this.sensorOneBtn);
            this.grbSensorController.Controls.Add(this.grBoxSensor);
            this.grbSensorController.Controls.Add(this.btnContenResponseClear);
            this.grbSensorController.Controls.Add(this.richTXTContentResponse);
            this.grbSensorController.Controls.Add(this.lblContentResponse);
            this.grbSensorController.Controls.Add(this.numSensorControllerPort);
            this.grbSensorController.Controls.Add(this.tableLayoutPanel5);
            this.grbSensorController.Controls.Add(this.txtSensorControllerIP);
            this.grbSensorController.Controls.Add(this.lblSensorControllerPort);
            this.grbSensorController.Controls.Add(this.lblSensorControllerIP);
            this.grbSensorController.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbSensorController.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.grbSensorController.Location = new System.Drawing.Point(0, 0);
            this.grbSensorController.Name = "grbSensorController";
            this.grbSensorController.Size = new System.Drawing.Size(990, 514);
            this.grbSensorController.TabIndex = 21;
            this.grbSensorController.TabStop = false;
            this.grbSensorController.Text = "PLC";
            // 
            // btnProductList
            // 
            this.btnProductList._BorderColor = System.Drawing.Color.Silver;
            this.btnProductList._BorderRadius = 15;
            this.btnProductList._BorderSize = 0;
            this.btnProductList._GradientsButton = false;
            this.btnProductList._Text = "Loại thùng trên dây truyền";
            this.btnProductList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnProductList.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnProductList.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnProductList.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnProductList.FlatAppearance.BorderSize = 0;
            this.btnProductList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProductList.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.btnProductList.ForeColor = System.Drawing.Color.White;
            this.btnProductList.Location = new System.Drawing.Point(23, 225);
            this.btnProductList.Name = "btnProductList";
            this.btnProductList.Size = new System.Drawing.Size(214, 45);
            this.btnProductList.TabIndex = 156;
            this.btnProductList.Text = "Loại thùng trên dây truyền";
            this.btnProductList.TextColor = System.Drawing.Color.White;
            this.btnProductList.UseVisualStyleBackColor = false;
            this.btnProductList.Visible = false;
            // 
            // iconConnectPort1
            // 
            this.iconConnectPort1.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_red_dot;
            this.iconConnectPort1.InitialImage = global::BarcodeVerificationSystem.Properties.Resources.icons8_red_dot;
            this.iconConnectPort1.Location = new System.Drawing.Point(463, 167);
            this.iconConnectPort1.Name = "iconConnectPort1";
            this.iconConnectPort1.Size = new System.Drawing.Size(24, 28);
            this.iconConnectPort1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconConnectPort1.TabIndex = 152;
            this.iconConnectPort1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.PassPLC);
            this.panel1.Controls.Add(this.ErrorPLC);
            this.panel1.Controls.Add(this.StopPLC);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.StartPLC);
            this.panel1.Location = new System.Drawing.Point(503, 389);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(454, 109);
            this.panel1.TabIndex = 138;
            this.panel1.Visible = false;
            // 
            // PassPLC
            // 
            this.PassPLC.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.PassPLC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PassPLC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.PassPLC.ForeColor = System.Drawing.SystemColors.WindowText;
            this.PassPLC.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PassPLC.Location = new System.Drawing.Point(281, 60);
            this.PassPLC.Name = "PassPLC";
            this.PassPLC.Size = new System.Drawing.Size(153, 34);
            this.PassPLC.TabIndex = 120;
            this.PassPLC.Text = "Pass";
            this.PassPLC.UseVisualStyleBackColor = true;
            // 
            // ErrorPLC
            // 
            this.ErrorPLC.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.ErrorPLC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ErrorPLC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.ErrorPLC.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ErrorPLC.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ErrorPLC.Location = new System.Drawing.Point(95, 60);
            this.ErrorPLC.Name = "ErrorPLC";
            this.ErrorPLC.Size = new System.Drawing.Size(153, 34);
            this.ErrorPLC.TabIndex = 119;
            this.ErrorPLC.Text = "Error";
            this.ErrorPLC.UseVisualStyleBackColor = true;
            // 
            // StopPLC
            // 
            this.StopPLC.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.StopPLC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopPLC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.StopPLC.ForeColor = System.Drawing.SystemColors.WindowText;
            this.StopPLC.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StopPLC.Location = new System.Drawing.Point(281, 12);
            this.StopPLC.Name = "StopPLC";
            this.StopPLC.Size = new System.Drawing.Size(153, 34);
            this.StopPLC.TabIndex = 118;
            this.StopPLC.Text = "Stop";
            this.StopPLC.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label6.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(15, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 20);
            this.label6.TabIndex = 117;
            this.label6.Text = "Test PLC";
            // 
            // StartPLC
            // 
            this.StartPLC.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.StartPLC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StartPLC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.StartPLC.ForeColor = System.Drawing.SystemColors.WindowText;
            this.StartPLC.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StartPLC.Location = new System.Drawing.Point(95, 12);
            this.StartPLC.Name = "StartPLC";
            this.StartPLC.Size = new System.Drawing.Size(153, 34);
            this.StartPLC.TabIndex = 80;
            this.StartPLC.Text = "Start";
            this.StartPLC.UseVisualStyleBackColor = true;
            // 
            // EncoderMode
            // 
            this.EncoderMode.Controls.Add(this.tableLayoutPanel1);
            this.EncoderMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EncoderMode.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.EncoderMode.Location = new System.Drawing.Point(6, 389);
            this.EncoderMode.Name = "EncoderMode";
            this.EncoderMode.Size = new System.Drawing.Size(494, 85);
            this.EncoderMode.TabIndex = 79;
            this.EncoderMode.TabStop = false;
            this.EncoderMode.Text = "Encoder Settings";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.ExternalRad, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.InternalRad, 1, 0);
            this.tableLayoutPanel1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(32, 25);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(439, 38);
            this.tableLayoutPanel1.TabIndex = 34;
            // 
            // ExternalRad
            // 
            this.ExternalRad.Appearance = System.Windows.Forms.Appearance.Button;
            this.ExternalRad.BackColor = System.Drawing.Color.White;
            this.ExternalRad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExternalRad.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.ExternalRad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ExternalRad.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExternalRad.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ExternalRad.Location = new System.Drawing.Point(0, 0);
            this.ExternalRad.Margin = new System.Windows.Forms.Padding(0);
            this.ExternalRad.Name = "ExternalRad";
            this.ExternalRad.Size = new System.Drawing.Size(219, 38);
            this.ExternalRad.TabIndex = 3;
            this.ExternalRad.TabStop = true;
            this.ExternalRad.Text = "External";
            this.ExternalRad.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ExternalRad.UseVisualStyleBackColor = false;
            // 
            // InternalRad
            // 
            this.InternalRad.Appearance = System.Windows.Forms.Appearance.Button;
            this.InternalRad.BackColor = System.Drawing.Color.White;
            this.InternalRad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InternalRad.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.InternalRad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InternalRad.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InternalRad.ForeColor = System.Drawing.SystemColors.WindowText;
            this.InternalRad.Location = new System.Drawing.Point(219, 0);
            this.InternalRad.Margin = new System.Windows.Forms.Padding(0);
            this.InternalRad.Name = "InternalRad";
            this.InternalRad.Size = new System.Drawing.Size(220, 38);
            this.InternalRad.TabIndex = 4;
            this.InternalRad.TabStop = true;
            this.InternalRad.Text = "Internal";
            this.InternalRad.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.InternalRad.UseVisualStyleBackColor = false;
            // 
            // radioV2
            // 
            this.radioV2.AutoSize = true;
            this.radioV2.Location = new System.Drawing.Point(372, 40);
            this.radioV2.Name = "radioV2";
            this.radioV2.Size = new System.Drawing.Size(90, 24);
            this.radioV2.TabIndex = 85;
            this.radioV2.TabStop = true;
            this.radioV2.Text = "v1.0.0.2";
            this.radioV2.UseVisualStyleBackColor = true;
            // 
            // delayOutputPanel
            // 
            this.delayOutputPanel.Controls.Add(this.lblDelayOutputError);
            this.delayOutputPanel.Controls.Add(this.Delaymm);
            this.delayOutputPanel.Controls.Add(this.numericDelayOutputError);
            this.delayOutputPanel.Location = new System.Drawing.Point(502, 315);
            this.delayOutputPanel.Name = "delayOutputPanel";
            this.delayOutputPanel.Size = new System.Drawing.Size(232, 62);
            this.delayOutputPanel.TabIndex = 84;
            // 
            // lblDelayOutputError
            // 
            this.lblDelayOutputError.AutoSize = true;
            this.lblDelayOutputError.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDelayOutputError.Location = new System.Drawing.Point(-3, 0);
            this.lblDelayOutputError.Name = "lblDelayOutputError";
            this.lblDelayOutputError.Size = new System.Drawing.Size(131, 20);
            this.lblDelayOutputError.TabIndex = 71;
            this.lblDelayOutputError.Text = "Delay ouput error";
            // 
            // Delaymm
            // 
            this.Delaymm.AutoSize = true;
            this.Delaymm.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Delaymm.Location = new System.Drawing.Point(193, 28);
            this.Delaymm.Name = "Delaymm";
            this.Delaymm.Size = new System.Drawing.Size(35, 20);
            this.Delaymm.TabIndex = 73;
            this.Delaymm.Text = "mm";
            // 
            // numericDelayOutputError
            // 
            this.numericDelayOutputError.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericDelayOutputError.Location = new System.Drawing.Point(0, 28);
            this.numericDelayOutputError.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numericDelayOutputError.Name = "numericDelayOutputError";
            this.numericDelayOutputError.Size = new System.Drawing.Size(186, 26);
            this.numericDelayOutputError.TabIndex = 72;
            // 
            // radioV1
            // 
            this.radioV1.AutoSize = true;
            this.radioV1.Location = new System.Drawing.Point(262, 40);
            this.radioV1.Name = "radioV1";
            this.radioV1.Size = new System.Drawing.Size(90, 24);
            this.radioV1.TabIndex = 79;
            this.radioV1.TabStop = true;
            this.radioV1.Text = "v1.0.0.1";
            this.radioV1.UseVisualStyleBackColor = true;
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionLabel.Location = new System.Drawing.Point(21, 42);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(93, 20);
            this.versionLabel.TabIndex = 83;
            this.versionLabel.Text = "PLC version";
            // 
            // radioV0
            // 
            this.radioV0.AutoSize = true;
            this.radioV0.Location = new System.Drawing.Point(152, 40);
            this.radioV0.Name = "radioV0";
            this.radioV0.Size = new System.Drawing.Size(90, 24);
            this.radioV0.TabIndex = 78;
            this.radioV0.TabStop = true;
            this.radioV0.Text = "v1.0.0.0";
            this.radioV0.UseVisualStyleBackColor = true;
            // 
            // comboPLCPort
            // 
            this.comboPLCPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboPLCPort.FormattingEnabled = true;
            this.comboPLCPort.Items.AddRange(new object[] {
            "1",
            "2"});
            this.comboPLCPort.Location = new System.Drawing.Point(421, 133);
            this.comboPLCPort.Name = "comboPLCPort";
            this.comboPLCPort.Size = new System.Drawing.Size(41, 28);
            this.comboPLCPort.TabIndex = 82;
            // 
            // Port2Panel
            // 
            this.Port2Panel.Controls.Add(this.iconConnectPort2);
            this.Port2Panel.Controls.Add(this.numSensorControllerPort2);
            this.Port2Panel.Controls.Add(this.SendPort2);
            this.Port2Panel.Location = new System.Drawing.Point(243, 199);
            this.Port2Panel.Name = "Port2Panel";
            this.Port2Panel.Size = new System.Drawing.Size(249, 79);
            this.Port2Panel.TabIndex = 81;
            // 
            // iconConnectPort2
            // 
            this.iconConnectPort2.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_red_dot;
            this.iconConnectPort2.InitialImage = global::BarcodeVerificationSystem.Properties.Resources.icons8_red_dot;
            this.iconConnectPort2.Location = new System.Drawing.Point(220, 6);
            this.iconConnectPort2.Name = "iconConnectPort2";
            this.iconConnectPort2.Size = new System.Drawing.Size(24, 28);
            this.iconConnectPort2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconConnectPort2.TabIndex = 151;
            this.iconConnectPort2.TabStop = false;
            // 
            // numSensorControllerPort2
            // 
            this.numSensorControllerPort2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numSensorControllerPort2.Location = new System.Drawing.Point(19, 6);
            this.numSensorControllerPort2.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numSensorControllerPort2.Name = "numSensorControllerPort2";
            this.numSensorControllerPort2.Size = new System.Drawing.Size(200, 26);
            this.numSensorControllerPort2.TabIndex = 79;
            // 
            // SendPort2
            // 
            this.SendPort2.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.SendPort2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SendPort2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SendPort2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.SendPort2.Location = new System.Drawing.Point(66, 39);
            this.SendPort2.Name = "SendPort2";
            this.SendPort2.Size = new System.Drawing.Size(153, 34);
            this.SendPort2.TabIndex = 80;
            this.SendPort2.Text = "Gửi Port 2";
            this.SendPort2.UseVisualStyleBackColor = true;
            // 
            // grbResumeEncoder
            // 
            this.grbResumeEncoder.Controls.Add(this.EnableResumeEncoder);
            this.grbResumeEncoder.Controls.Add(this.radioResumeA);
            this.grbResumeEncoder.Controls.Add(this.radioResumeAB);
            this.grbResumeEncoder.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbResumeEncoder.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.grbResumeEncoder.Location = new System.Drawing.Point(493, 389);
            this.grbResumeEncoder.Name = "grbResumeEncoder";
            this.grbResumeEncoder.Size = new System.Drawing.Size(494, 85);
            this.grbResumeEncoder.TabIndex = 78;
            this.grbResumeEncoder.TabStop = false;
            this.grbResumeEncoder.Text = "Encoder Settings";
            // 
            // EnableResumeEncoder
            // 
            this.EnableResumeEncoder.AutoSize = true;
            this.EnableResumeEncoder.Location = new System.Drawing.Point(9, 25);
            this.EnableResumeEncoder.Name = "EnableResumeEncoder";
            this.EnableResumeEncoder.Size = new System.Drawing.Size(166, 24);
            this.EnableResumeEncoder.TabIndex = 77;
            this.EnableResumeEncoder.Text = "Resume Encoder";
            this.EnableResumeEncoder.UseVisualStyleBackColor = true;
            // 
            // radioResumeA
            // 
            this.radioResumeA.AutoSize = true;
            this.radioResumeA.Location = new System.Drawing.Point(255, 24);
            this.radioResumeA.Name = "radioResumeA";
            this.radioResumeA.Size = new System.Drawing.Size(110, 24);
            this.radioResumeA.TabIndex = 75;
            this.radioResumeA.TabStop = true;
            this.radioResumeA.Text = "Resume A";
            this.radioResumeA.UseVisualStyleBackColor = true;
            // 
            // radioResumeAB
            // 
            this.radioResumeAB.AutoSize = true;
            this.radioResumeAB.Location = new System.Drawing.Point(255, 54);
            this.radioResumeAB.Name = "radioResumeAB";
            this.radioResumeAB.Size = new System.Drawing.Size(122, 24);
            this.radioResumeAB.TabIndex = 76;
            this.radioResumeAB.TabStop = true;
            this.radioResumeAB.Text = "Resume AB";
            this.radioResumeAB.UseVisualStyleBackColor = true;
            // 
            // sensorTwoBtn
            // 
            this.sensorTwoBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.sensorTwoBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sensorTwoBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sensorTwoBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.sensorTwoBtn.Location = new System.Drawing.Point(720, 52);
            this.sensorTwoBtn.Name = "sensorTwoBtn";
            this.sensorTwoBtn.Size = new System.Drawing.Size(191, 38);
            this.sensorTwoBtn.TabIndex = 66;
            this.sensorTwoBtn.Text = "Sensor 2";
            this.sensorTwoBtn.UseVisualStyleBackColor = true;
            this.sensorTwoBtn.Click += new System.EventHandler(this.SensorTwoBtn_Click);
            // 
            // sensorOneBtn
            // 
            this.sensorOneBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.sensorOneBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sensorOneBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sensorOneBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.sensorOneBtn.Location = new System.Drawing.Point(493, 52);
            this.sensorOneBtn.Name = "sensorOneBtn";
            this.sensorOneBtn.Size = new System.Drawing.Size(196, 38);
            this.sensorOneBtn.TabIndex = 65;
            this.sensorOneBtn.Text = "Sensor 1";
            this.sensorOneBtn.UseVisualStyleBackColor = true;
            this.sensorOneBtn.Click += new System.EventHandler(this.SensorOneBtn_Click);
            // 
            // grBoxSensor
            // 
            this.grBoxSensor.Controls.Add(this.OutputDelay);
            this.grBoxSensor.Controls.Add(this.lblErrorCondition);
            this.grBoxSensor.Controls.Add(this.lblUnit);
            this.grBoxSensor.Controls.Add(this.lblGAP);
            this.grBoxSensor.Controls.Add(this.numGapLength1);
            this.grBoxSensor.Controls.Add(this.GAPmm);
            this.grBoxSensor.Controls.Add(this.numLength2Error1);
            this.grBoxSensor.Controls.Add(this.lblSensorControllerDelayAfter);
            this.grBoxSensor.Controls.Add(this.lblSensorControllerPulseEncoder);
            this.grBoxSensor.Controls.Add(this.lblSensorControllerEncoderDiameter);
            this.grBoxSensor.Controls.Add(this.numSensorControllerPulseEncoder);
            this.grBoxSensor.Controls.Add(this.numSensorControllerEncoderDiameter);
            this.grBoxSensor.Controls.Add(this.label4);
            this.grBoxSensor.Controls.Add(this.lblSensorControllerDelayBefore);
            this.grBoxSensor.Controls.Add(this.label3);
            this.grBoxSensor.Controls.Add(this.numSensorControllerDelayBefore);
            this.grBoxSensor.Controls.Add(this.label2);
            this.grBoxSensor.Controls.Add(this.numSensorControllerDelayAfter);
            this.grBoxSensor.Controls.Add(this.ppr);
            this.grBoxSensor.Location = new System.Drawing.Point(493, 100);
            this.grBoxSensor.Name = "grBoxSensor";
            this.grBoxSensor.Size = new System.Drawing.Size(494, 283);
            this.grBoxSensor.TabIndex = 63;
            this.grBoxSensor.TabStop = false;
            this.grBoxSensor.Text = "Sensor 1";
            // 
            // OutputDelay
            // 
            this.OutputDelay.Controls.Add(this.labelDelayOut);
            this.OutputDelay.Controls.Add(this.label7);
            this.OutputDelay.Controls.Add(this.numDelayOutput);
            this.OutputDelay.Location = new System.Drawing.Point(247, 209);
            this.OutputDelay.Name = "OutputDelay";
            this.OutputDelay.Size = new System.Drawing.Size(236, 68);
            this.OutputDelay.TabIndex = 121;
            // 
            // labelDelayOut
            // 
            this.labelDelayOut.AutoSize = true;
            this.labelDelayOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDelayOut.Location = new System.Drawing.Point(3, 8);
            this.labelDelayOut.Name = "labelDelayOut";
            this.labelDelayOut.Size = new System.Drawing.Size(133, 20);
            this.labelDelayOut.TabIndex = 54;
            this.labelDelayOut.Text = "Output delay time";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(199, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(30, 20);
            this.label7.TabIndex = 56;
            this.label7.Text = "ms";
            // 
            // numDelayOutput
            // 
            this.numDelayOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numDelayOutput.Location = new System.Drawing.Point(7, 35);
            this.numDelayOutput.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numDelayOutput.Name = "numDelayOutput";
            this.numDelayOutput.Size = new System.Drawing.Size(186, 26);
            this.numDelayOutput.TabIndex = 55;
            // 
            // lblErrorCondition
            // 
            this.lblErrorCondition.AutoSize = true;
            this.lblErrorCondition.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblErrorCondition.Location = new System.Drawing.Point(251, 146);
            this.lblErrorCondition.Name = "lblErrorCondition";
            this.lblErrorCondition.Size = new System.Drawing.Size(115, 20);
            this.lblErrorCondition.TabIndex = 46;
            this.lblErrorCondition.Text = "Error Condition";
            // 
            // lblUnit
            // 
            this.lblUnit.AutoSize = true;
            this.lblUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnit.Location = new System.Drawing.Point(447, 175);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(38, 20);
            this.lblUnit.TabIndex = 50;
            this.lblUnit.Text = "Unit";
            // 
            // lblGAP
            // 
            this.lblGAP.AutoSize = true;
            this.lblGAP.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGAP.Location = new System.Drawing.Point(5, 146);
            this.lblGAP.Name = "lblGAP";
            this.lblGAP.Size = new System.Drawing.Size(43, 20);
            this.lblGAP.TabIndex = 45;
            this.lblGAP.Text = "GAP";
            // 
            // numGapLength1
            // 
            this.numGapLength1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numGapLength1.Location = new System.Drawing.Point(9, 173);
            this.numGapLength1.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numGapLength1.Name = "numGapLength1";
            this.numGapLength1.Size = new System.Drawing.Size(186, 26);
            this.numGapLength1.TabIndex = 47;
            // 
            // GAPmm
            // 
            this.GAPmm.AutoSize = true;
            this.GAPmm.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GAPmm.Location = new System.Drawing.Point(201, 175);
            this.GAPmm.Name = "GAPmm";
            this.GAPmm.Size = new System.Drawing.Size(35, 20);
            this.GAPmm.TabIndex = 49;
            this.GAPmm.Text = "mm";
            // 
            // numLength2Error1
            // 
            this.numLength2Error1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numLength2Error1.Location = new System.Drawing.Point(255, 173);
            this.numLength2Error1.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numLength2Error1.Name = "numLength2Error1";
            this.numLength2Error1.Size = new System.Drawing.Size(186, 26);
            this.numLength2Error1.TabIndex = 48;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imageList2
            // 
            this.imageList2.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList2.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imageList3
            // 
            this.imageList3.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList3.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList3.TransparentColor = System.Drawing.Color.Transparent;
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
            // UcSensorSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grbSensorController);
            this.Name = "UcSensorSettings";
            this.Size = new System.Drawing.Size(990, 514);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerPulseEncoder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerEncoderDiameter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerDelayBefore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerDelayAfter)).EndInit();
            this.grbSensorController.ResumeLayout(false);
            this.grbSensorController.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconConnectPort1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.EncoderMode.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.delayOutputPanel.ResumeLayout(false);
            this.delayOutputPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericDelayOutputError)).EndInit();
            this.Port2Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.iconConnectPort2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSensorControllerPort2)).EndInit();
            this.grbResumeEncoder.ResumeLayout(false);
            this.grbResumeEncoder.PerformLayout();
            this.grBoxSensor.ResumeLayout(false);
            this.grBoxSensor.PerformLayout();
            this.OutputDelay.ResumeLayout(false);
            this.OutputDelay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDelayOutput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGapLength1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLength2Error1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblSensorControllerIP;
        private System.Windows.Forms.Label lblSensorControllerPort;
        private System.Windows.Forms.Label lblSensorControllerPulseEncoder;
        private System.Windows.Forms.Label lblSensorControllerEncoderDiameter;
        private IPAddressControlLib.IPAddressControl txtSensorControllerIP;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.RadioButton radSensorControllerEnable;
        private System.Windows.Forms.RadioButton radSensorControllerDisable;
        private System.Windows.Forms.NumericUpDown numSensorControllerPort;
        private System.Windows.Forms.NumericUpDown numSensorControllerPulseEncoder;
        private System.Windows.Forms.NumericUpDown numSensorControllerEncoderDiameter;
        private System.Windows.Forms.Label lblSensorControllerDelayBefore;
        private System.Windows.Forms.Label lblSensorControllerDelayAfter;
        private System.Windows.Forms.NumericUpDown numSensorControllerDelayBefore;
        private System.Windows.Forms.NumericUpDown numSensorControllerDelayAfter;
        private System.Windows.Forms.Label ppr;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblContentResponse;
        private System.Windows.Forms.RichTextBox richTXTContentResponse;
        private System.Windows.Forms.Button btnContenResponseClear;
        private System.Windows.Forms.GroupBox grbSensorController;
        private System.Windows.Forms.GroupBox grBoxSensor;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label lblErrorCondition;
        private System.Windows.Forms.Label lblUnit;
        private System.Windows.Forms.Label lblGAP;
        private System.Windows.Forms.NumericUpDown numGapLength1;
        private System.Windows.Forms.Label GAPmm;
        private System.Windows.Forms.NumericUpDown numLength2Error1;
        private System.Windows.Forms.Button sensorTwoBtn;
        private System.Windows.Forms.Button sensorOneBtn;
        private System.Windows.Forms.Label lblDelayOutputError;
        private System.Windows.Forms.NumericUpDown numericDelayOutputError;
        private System.Windows.Forms.GroupBox grbResumeEncoder;
        private System.Windows.Forms.RadioButton radioResumeA;
        private System.Windows.Forms.RadioButton radioResumeAB;
        private System.Windows.Forms.CheckBox EnableResumeEncoder;
        private System.Windows.Forms.Label Delaymm;
        private System.Windows.Forms.NumericUpDown numSensorControllerPort2;
        private System.Windows.Forms.Button SendPort2;
        private System.Windows.Forms.Panel Port2Panel;
        private System.Windows.Forms.ComboBox comboPLCPort;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.RadioButton radioV1;
        private System.Windows.Forms.RadioButton radioV0;
        private System.Windows.Forms.Panel delayOutputPanel;
        private System.Windows.Forms.RadioButton radioV2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RadioButton ExternalRad;
        private System.Windows.Forms.RadioButton InternalRad;
        private System.Windows.Forms.GroupBox EncoderMode;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button PassPLC;
        private System.Windows.Forms.Button ErrorPLC;
        private System.Windows.Forms.Button StopPLC;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button StartPLC;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.ImageList imageList3;
        private FontAwesome.Sharp.IconMenuItem iconMenuItem1;
        private System.Windows.Forms.PictureBox iconConnectPort2;
        private System.Windows.Forms.PictureBox iconConnectPort1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labelDelayOut;
        private System.Windows.Forms.NumericUpDown numDelayOutput;
        private System.Windows.Forms.Panel OutputDelay;
        private DesignUI.CuzUI.CuzButton btnProductList;
    }
}
