using System.Drawing;
using System.Windows.Forms;
using System;

namespace BarcodeVerificationSystem.View.UcSettings
{
    partial class ucZebraPrinterSetting
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
            System.Windows.Forms.GroupBox groupBoxConnection;
            this.radLAN = new System.Windows.Forms.RadioButton();
            this.radUSB = new System.Windows.Forms.RadioButton();
            this.pnlUSB = new System.Windows.Forms.Panel();
            this.lblUsbPrinterName = new System.Windows.Forms.Label();
            this.cmbUsbPrinterName = new System.Windows.Forms.ComboBox();
            this.btnRefreshPrinters = new System.Windows.Forms.Button();
            this.txtPrinterIP = new IPAddressControlLib.IPAddressControl();
            this.lblPrinterPort = new System.Windows.Forms.Label();
            this.lblIPrinterIP = new System.Windows.Forms.Label();
            this.numPrinterPort = new System.Windows.Forms.NumericUpDown();
            this.groupBoxText = new System.Windows.Forms.GroupBox();
            this.chkEnableText = new System.Windows.Forms.CheckBox();
            this.cmbTextFont = new System.Windows.Forms.ComboBox();
            this.txtTextContent = new System.Windows.Forms.TextBox();
            this.txtTextY = new System.Windows.Forms.TextBox();
            this.txtTextX = new System.Windows.Forms.TextBox();
            this.txtTextFontSize = new System.Windows.Forms.TextBox();
            this.lblTextFont = new System.Windows.Forms.Label();
            this.lblTextContent = new System.Windows.Forms.Label();
            this.lblTextY = new System.Windows.Forms.Label();
            this.lblTextX = new System.Windows.Forms.Label();
            this.lblTextFontSize = new System.Windows.Forms.Label();
            this.groupBoxBarcode = new System.Windows.Forms.GroupBox();
            this.chkEnableBarcode = new System.Windows.Forms.CheckBox();
            this.cmbBarcodeType = new System.Windows.Forms.ComboBox();
            this.txtBarcodeSize = new System.Windows.Forms.TextBox();
            this.txtBarcodeY = new System.Windows.Forms.TextBox();
            this.txtBarcodeX = new System.Windows.Forms.TextBox();
            this.txtBarcodeData = new System.Windows.Forms.TextBox();
            this.lblBarcodeType = new System.Windows.Forms.Label();
            this.lblBarcodeSize = new System.Windows.Forms.Label();
            this.lblBarcodeY = new System.Windows.Forms.Label();
            this.lblBarcodeX = new System.Windows.Forms.Label();
            this.lblBarcodeData = new System.Windows.Forms.Label();
            this.txtBarcodeText = new System.Windows.Forms.TextBox();
            this.lblBarcodeText = new System.Windows.Forms.Label();
            this.groupBoxLabelSettings = new System.Windows.Forms.GroupBox();
            this.lblBarcodeModuleWidth = new System.Windows.Forms.Label();
            this.numBarcodeModuleWidth = new System.Windows.Forms.NumericUpDown();
            this.lblLabelLength = new System.Windows.Forms.Label();
            this.numLabelLength = new System.Windows.Forms.NumericUpDown();
            this.groupBoxCommands = new System.Windows.Forms.GroupBox();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.btnSendCustomZPL = new System.Windows.Forms.Button();
            this.txtCustomZPL = new System.Windows.Forms.TextBox();
            this.btnSendReady = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.groupBoxStatus = new System.Windows.Forms.GroupBox();
            this.txtStatusLog = new System.Windows.Forms.TextBox();
            groupBoxConnection = new System.Windows.Forms.GroupBox();
            groupBoxConnection.SuspendLayout();
            this.pnlUSB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPrinterPort)).BeginInit();
            this.groupBoxText.SuspendLayout();
            this.groupBoxBarcode.SuspendLayout();
            this.groupBoxLabelSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBarcodeModuleWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLabelLength)).BeginInit();
            this.groupBoxCommands.SuspendLayout();
            this.groupBoxStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxConnection
            // 
            groupBoxConnection.Controls.Add(this.radLAN);
            groupBoxConnection.Controls.Add(this.radUSB);
            groupBoxConnection.Controls.Add(this.pnlUSB);
            groupBoxConnection.Controls.Add(this.txtPrinterIP);
            groupBoxConnection.Controls.Add(this.lblPrinterPort);
            groupBoxConnection.Controls.Add(this.lblIPrinterIP);
            groupBoxConnection.Controls.Add(this.numPrinterPort);
            groupBoxConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            groupBoxConnection.Location = new System.Drawing.Point(10, 10);
            groupBoxConnection.Name = "groupBoxConnection";
            groupBoxConnection.Size = new System.Drawing.Size(620, 119);
            groupBoxConnection.TabIndex = 0;
            groupBoxConnection.TabStop = false;
            groupBoxConnection.Text = "Kết nối máy in";
            // 
            // radLAN
            // 
            this.radLAN.AutoSize = true;
            this.radLAN.Checked = true;
            this.radLAN.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.radLAN.Location = new System.Drawing.Point(8, 22);
            this.radLAN.Name = "radLAN";
            this.radLAN.Size = new System.Drawing.Size(104, 24);
            this.radLAN.TabIndex = 50;
            this.radLAN.TabStop = true;
            this.radLAN.Text = "Zebra LAN";
            this.radLAN.UseVisualStyleBackColor = true;
            // 
            // radUSB
            // 
            this.radUSB.AutoSize = true;
            this.radUSB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.radUSB.Location = new System.Drawing.Point(155, 22);
            this.radUSB.Name = "radUSB";
            this.radUSB.Size = new System.Drawing.Size(107, 24);
            this.radUSB.TabIndex = 51;
            this.radUSB.Text = "Zebra USB";
            this.radUSB.UseVisualStyleBackColor = true;
            // 
            // pnlUSB
            // 
            this.pnlUSB.Controls.Add(this.lblUsbPrinterName);
            this.pnlUSB.Controls.Add(this.cmbUsbPrinterName);
            this.pnlUSB.Controls.Add(this.btnRefreshPrinters);
            this.pnlUSB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.pnlUSB.Location = new System.Drawing.Point(0, 50);
            this.pnlUSB.Name = "pnlUSB";
            this.pnlUSB.Size = new System.Drawing.Size(618, 64);
            this.pnlUSB.TabIndex = 52;
            this.pnlUSB.Visible = false;
            // 
            // lblUsbPrinterName
            // 
            this.lblUsbPrinterName.AutoSize = true;
            this.lblUsbPrinterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblUsbPrinterName.Location = new System.Drawing.Point(8, 5);
            this.lblUsbPrinterName.Name = "lblUsbPrinterName";
            this.lblUsbPrinterName.Size = new System.Drawing.Size(127, 20);
            this.lblUsbPrinterName.TabIndex = 0;
            this.lblUsbPrinterName.Text = "Tên máy in USB:";
            // 
            // cmbUsbPrinterName
            // 
            this.cmbUsbPrinterName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUsbPrinterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.cmbUsbPrinterName.FormattingEnabled = true;
            this.cmbUsbPrinterName.Location = new System.Drawing.Point(8, 30);
            this.cmbUsbPrinterName.Name = "cmbUsbPrinterName";
            this.cmbUsbPrinterName.Size = new System.Drawing.Size(380, 26);
            this.cmbUsbPrinterName.TabIndex = 1;
            // 
            // btnRefreshPrinters
            // 
            this.btnRefreshPrinters.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnRefreshPrinters.Location = new System.Drawing.Point(400, 30);
            this.btnRefreshPrinters.Name = "btnRefreshPrinters";
            this.btnRefreshPrinters.Size = new System.Drawing.Size(60, 20);
            this.btnRefreshPrinters.TabIndex = 2;
            this.btnRefreshPrinters.Text = "Làm mới";
            this.btnRefreshPrinters.UseVisualStyleBackColor = true;
            // 
            // txtPrinterIP
            // 
            this.txtPrinterIP.AllowInternalTab = false;
            this.txtPrinterIP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPrinterIP.AutoHeight = false;
            this.txtPrinterIP.BackColor = System.Drawing.SystemColors.Window;
            this.txtPrinterIP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtPrinterIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPrinterIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPrinterIP.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtPrinterIP.Location = new System.Drawing.Point(14, 88);
            this.txtPrinterIP.MinimumSize = new System.Drawing.Size(114, 24);
            this.txtPrinterIP.Name = "txtPrinterIP";
            this.txtPrinterIP.ReadOnly = false;
            this.txtPrinterIP.Size = new System.Drawing.Size(207, 24);
            this.txtPrinterIP.TabIndex = 32;
            this.txtPrinterIP.Text = "...";
            // 
            // lblPrinterPort
            // 
            this.lblPrinterPort.AutoSize = true;
            this.lblPrinterPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrinterPort.Location = new System.Drawing.Point(269, 58);
            this.lblPrinterPort.Name = "lblPrinterPort";
            this.lblPrinterPort.Size = new System.Drawing.Size(47, 20);
            this.lblPrinterPort.TabIndex = 41;
            this.lblPrinterPort.Text = "Cổng";
            // 
            // lblIPrinterIP
            // 
            this.lblIPrinterIP.AutoSize = true;
            this.lblIPrinterIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIPrinterIP.Location = new System.Drawing.Point(8, 58);
            this.lblIPrinterIP.Name = "lblIPrinterIP";
            this.lblIPrinterIP.Size = new System.Drawing.Size(76, 20);
            this.lblIPrinterIP.TabIndex = 40;
            this.lblIPrinterIP.Text = "Địa chỉ IP";
            // 
            // numPrinterPort
            // 
            this.numPrinterPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numPrinterPort.Location = new System.Drawing.Point(273, 88);
            this.numPrinterPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPrinterPort.Name = "numPrinterPort";
            this.numPrinterPort.Size = new System.Drawing.Size(107, 26);
            this.numPrinterPort.TabIndex = 39;
            // 
            // groupBoxText
            // 
            this.groupBoxText.Controls.Add(this.chkEnableText);
            this.groupBoxText.Controls.Add(this.cmbTextFont);
            this.groupBoxText.Controls.Add(this.txtTextContent);
            this.groupBoxText.Controls.Add(this.txtTextY);
            this.groupBoxText.Controls.Add(this.txtTextX);
            this.groupBoxText.Controls.Add(this.txtTextFontSize);
            this.groupBoxText.Controls.Add(this.lblTextFont);
            this.groupBoxText.Controls.Add(this.lblTextContent);
            this.groupBoxText.Controls.Add(this.lblTextY);
            this.groupBoxText.Controls.Add(this.lblTextX);
            this.groupBoxText.Controls.Add(this.lblTextFontSize);
            this.groupBoxText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.groupBoxText.Location = new System.Drawing.Point(10, 135);
            this.groupBoxText.Name = "groupBoxText";
            this.groupBoxText.Size = new System.Drawing.Size(341, 155);
            this.groupBoxText.TabIndex = 2;
            this.groupBoxText.TabStop = false;
            this.groupBoxText.Text = "Cài đặt văn bản";
            // 
            // chkEnableText
            // 
            this.chkEnableText.AutoSize = true;
            this.chkEnableText.Checked = true;
            this.chkEnableText.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableText.Location = new System.Drawing.Point(5, 19);
            this.chkEnableText.Name = "chkEnableText";
            this.chkEnableText.Size = new System.Drawing.Size(102, 24);
            this.chkEnableText.TabIndex = 8;
            this.chkEnableText.Text = "In văn bản";
            this.chkEnableText.UseVisualStyleBackColor = true;
            // 
            // cmbTextFont
            // 
            this.cmbTextFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTextFont.FormattingEnabled = true;
            this.cmbTextFont.Items.AddRange(new object[] {
            "A - Smallest (9x12 dots)",
            "B - Small (11x20 dots)",
            "C - Medium (18x28 dots)",
            "D - Large (28x42 dots)",
            "E - Extra Large (42x60 dots)",
            "F - Very Large (60x72 dots)",
            "G - Huge (72x84 dots)",
            "H - Largest (84x120 dots)",
            "0 - Scalable (Custom size)"});
            this.cmbTextFont.Location = new System.Drawing.Point(209, 43);
            this.cmbTextFont.Name = "cmbTextFont";
            this.cmbTextFont.Size = new System.Drawing.Size(126, 28);
            this.cmbTextFont.TabIndex = 9;
            // 
            // txtTextContent
            // 
            this.txtTextContent.Location = new System.Drawing.Point(69, 117);
            this.txtTextContent.Multiline = true;
            this.txtTextContent.Name = "txtTextContent";
            this.txtTextContent.Size = new System.Drawing.Size(266, 27);
            this.txtTextContent.TabIndex = 7;
            this.txtTextContent.Text = "Test QR Code";
            // 
            // txtTextY
            // 
            this.txtTextY.Location = new System.Drawing.Point(209, 82);
            this.txtTextY.Name = "txtTextY";
            this.txtTextY.Size = new System.Drawing.Size(126, 26);
            this.txtTextY.TabIndex = 6;
            this.txtTextY.Text = "50";
            // 
            // txtTextX
            // 
            this.txtTextX.Location = new System.Drawing.Point(69, 82);
            this.txtTextX.Name = "txtTextX";
            this.txtTextX.Size = new System.Drawing.Size(87, 26);
            this.txtTextX.TabIndex = 5;
            this.txtTextX.Text = "50";
            // 
            // txtTextFontSize
            // 
            this.txtTextFontSize.Location = new System.Drawing.Point(69, 43);
            this.txtTextFontSize.Name = "txtTextFontSize";
            this.txtTextFontSize.Size = new System.Drawing.Size(87, 26);
            this.txtTextFontSize.TabIndex = 4;
            this.txtTextFontSize.Text = "40";
            // 
            // lblTextFont
            // 
            this.lblTextFont.AutoSize = true;
            this.lblTextFont.Location = new System.Drawing.Point(169, 46);
            this.lblTextFont.Name = "lblTextFont";
            this.lblTextFont.Size = new System.Drawing.Size(44, 20);
            this.lblTextFont.TabIndex = 3;
            this.lblTextFont.Text = "Kiểu:";
            // 
            // lblTextContent
            // 
            this.lblTextContent.AutoSize = true;
            this.lblTextContent.Location = new System.Drawing.Point(9, 123);
            this.lblTextContent.Name = "lblTextContent";
            this.lblTextContent.Size = new System.Drawing.Size(76, 20);
            this.lblTextContent.TabIndex = 3;
            this.lblTextContent.Text = "Nội dung:";
            // 
            // lblTextY
            // 
            this.lblTextY.AutoSize = true;
            this.lblTextY.Location = new System.Drawing.Point(169, 85);
            this.lblTextY.Name = "lblTextY";
            this.lblTextY.Size = new System.Drawing.Size(24, 20);
            this.lblTextY.TabIndex = 2;
            this.lblTextY.Text = "Y:";
            // 
            // lblTextX
            // 
            this.lblTextX.AutoSize = true;
            this.lblTextX.Location = new System.Drawing.Point(7, 85);
            this.lblTextX.Name = "lblTextX";
            this.lblTextX.Size = new System.Drawing.Size(24, 20);
            this.lblTextX.TabIndex = 1;
            this.lblTextX.Text = "X:";
            // 
            // lblTextFontSize
            // 
            this.lblTextFontSize.AutoSize = true;
            this.lblTextFontSize.Location = new System.Drawing.Point(5, 46);
            this.lblTextFontSize.Name = "lblTextFontSize";
            this.lblTextFontSize.Size = new System.Drawing.Size(63, 20);
            this.lblTextFontSize.TabIndex = 0;
            this.lblTextFontSize.Text = "Cỡ chữ:";
            // 
            // groupBoxBarcode
            // 
            this.groupBoxBarcode.Controls.Add(this.chkEnableBarcode);
            this.groupBoxBarcode.Controls.Add(this.cmbBarcodeType);
            this.groupBoxBarcode.Controls.Add(this.txtBarcodeSize);
            this.groupBoxBarcode.Controls.Add(this.txtBarcodeY);
            this.groupBoxBarcode.Controls.Add(this.txtBarcodeX);
            this.groupBoxBarcode.Controls.Add(this.txtBarcodeData);
            this.groupBoxBarcode.Controls.Add(this.lblBarcodeType);
            this.groupBoxBarcode.Controls.Add(this.lblBarcodeSize);
            this.groupBoxBarcode.Controls.Add(this.lblBarcodeY);
            this.groupBoxBarcode.Controls.Add(this.lblBarcodeX);
            this.groupBoxBarcode.Controls.Add(this.lblBarcodeData);
            this.groupBoxBarcode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.groupBoxBarcode.Location = new System.Drawing.Point(358, 135);
            this.groupBoxBarcode.Name = "groupBoxBarcode";
            this.groupBoxBarcode.Size = new System.Drawing.Size(272, 155);
            this.groupBoxBarcode.TabIndex = 3;
            this.groupBoxBarcode.TabStop = false;
            this.groupBoxBarcode.Text = "Cài đặt mã vạch";
            // 
            // chkEnableBarcode
            // 
            this.chkEnableBarcode.AutoSize = true;
            this.chkEnableBarcode.Checked = true;
            this.chkEnableBarcode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableBarcode.Location = new System.Drawing.Point(5, 19);
            this.chkEnableBarcode.Name = "chkEnableBarcode";
            this.chkEnableBarcode.Size = new System.Drawing.Size(105, 24);
            this.chkEnableBarcode.TabIndex = 12;
            this.chkEnableBarcode.Text = "In mã vạch";
            this.chkEnableBarcode.UseVisualStyleBackColor = true;
            // 
            // cmbBarcodeType
            // 
            this.cmbBarcodeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBarcodeType.FormattingEnabled = true;
            this.cmbBarcodeType.Items.AddRange(new object[] {
            "QR Code",
            "DataMatrix",
            "Code 128",
            "GS1-128",
            "Code 39",
            "EAN-13",
            "EAN-8",
            "UPC-A",
            "UPC-E",
            "SSCC"});
            this.cmbBarcodeType.Location = new System.Drawing.Point(69, 43);
            this.cmbBarcodeType.Name = "cmbBarcodeType";
            this.cmbBarcodeType.Size = new System.Drawing.Size(191, 28);
            this.cmbBarcodeType.TabIndex = 10;
            // 
            // txtBarcodeSize
            // 
            this.txtBarcodeSize.Location = new System.Drawing.Point(184, 95);
            this.txtBarcodeSize.Name = "txtBarcodeSize";
            this.txtBarcodeSize.Size = new System.Drawing.Size(76, 26);
            this.txtBarcodeSize.TabIndex = 9;
            this.txtBarcodeSize.Text = "6";
            // 
            // txtBarcodeY
            // 
            this.txtBarcodeY.Location = new System.Drawing.Point(184, 69);
            this.txtBarcodeY.Name = "txtBarcodeY";
            this.txtBarcodeY.Size = new System.Drawing.Size(76, 26);
            this.txtBarcodeY.TabIndex = 8;
            this.txtBarcodeY.Text = "150";
            // 
            // txtBarcodeX
            // 
            this.txtBarcodeX.Location = new System.Drawing.Point(69, 69);
            this.txtBarcodeX.Name = "txtBarcodeX";
            this.txtBarcodeX.Size = new System.Drawing.Size(61, 26);
            this.txtBarcodeX.TabIndex = 7;
            this.txtBarcodeX.Text = "50";
            // 
            // txtBarcodeData
            // 
            this.txtBarcodeData.Location = new System.Drawing.Point(52, 121);
            this.txtBarcodeData.Multiline = true;
            this.txtBarcodeData.Name = "txtBarcodeData";
            this.txtBarcodeData.Size = new System.Drawing.Size(208, 28);
            this.txtBarcodeData.TabIndex = 6;
            this.txtBarcodeData.Text = "https://x.ai";
            // 
            // lblBarcodeType
            // 
            this.lblBarcodeType.AutoSize = true;
            this.lblBarcodeType.Location = new System.Drawing.Point(5, 46);
            this.lblBarcodeType.Name = "lblBarcodeType";
            this.lblBarcodeType.Size = new System.Drawing.Size(43, 20);
            this.lblBarcodeType.TabIndex = 4;
            this.lblBarcodeType.Text = "Loại:";
            // 
            // lblBarcodeSize
            // 
            this.lblBarcodeSize.AutoSize = true;
            this.lblBarcodeSize.Location = new System.Drawing.Point(147, 98);
            this.lblBarcodeSize.Name = "lblBarcodeSize";
            this.lblBarcodeSize.Size = new System.Drawing.Size(44, 20);
            this.lblBarcodeSize.TabIndex = 3;
            this.lblBarcodeSize.Text = "Size:";
            // 
            // lblBarcodeY
            // 
            this.lblBarcodeY.AutoSize = true;
            this.lblBarcodeY.Location = new System.Drawing.Point(148, 72);
            this.lblBarcodeY.Name = "lblBarcodeY";
            this.lblBarcodeY.Size = new System.Drawing.Size(24, 20);
            this.lblBarcodeY.TabIndex = 2;
            this.lblBarcodeY.Text = "Y:";
            // 
            // lblBarcodeX
            // 
            this.lblBarcodeX.AutoSize = true;
            this.lblBarcodeX.Location = new System.Drawing.Point(5, 72);
            this.lblBarcodeX.Name = "lblBarcodeX";
            this.lblBarcodeX.Size = new System.Drawing.Size(24, 20);
            this.lblBarcodeX.TabIndex = 1;
            this.lblBarcodeX.Text = "X:";
            // 
            // lblBarcodeData
            // 
            this.lblBarcodeData.AutoSize = true;
            this.lblBarcodeData.Location = new System.Drawing.Point(6, 123);
            this.lblBarcodeData.Name = "lblBarcodeData";
            this.lblBarcodeData.Size = new System.Drawing.Size(62, 20);
            this.lblBarcodeData.TabIndex = 0;
            this.lblBarcodeData.Text = "Dữ liệu:";
            // 
            // txtBarcodeText
            // 
            this.txtBarcodeText.Location = new System.Drawing.Point(508, 429);
            this.txtBarcodeText.Multiline = true;
            this.txtBarcodeText.Name = "txtBarcodeText";
            this.txtBarcodeText.Size = new System.Drawing.Size(126, 27);
            this.txtBarcodeText.TabIndex = 11;
            this.txtBarcodeText.Visible = false;
            // 
            // lblBarcodeText
            // 
            this.lblBarcodeText.AutoSize = true;
            this.lblBarcodeText.Location = new System.Drawing.Point(505, 407);
            this.lblBarcodeText.Name = "lblBarcodeText";
            this.lblBarcodeText.Size = new System.Drawing.Size(60, 13);
            this.lblBarcodeText.TabIndex = 5;
            this.lblBarcodeText.Text = "Text Label:";
            this.lblBarcodeText.Visible = false;
            // 
            // groupBoxLabelSettings
            // 
            this.groupBoxLabelSettings.Controls.Add(this.lblBarcodeModuleWidth);
            this.groupBoxLabelSettings.Controls.Add(this.numBarcodeModuleWidth);
            this.groupBoxLabelSettings.Controls.Add(this.lblLabelLength);
            this.groupBoxLabelSettings.Controls.Add(this.numLabelLength);
            this.groupBoxLabelSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.groupBoxLabelSettings.Location = new System.Drawing.Point(10, 297);
            this.groupBoxLabelSettings.Name = "groupBoxLabelSettings";
            this.groupBoxLabelSettings.Size = new System.Drawing.Size(620, 60);
            this.groupBoxLabelSettings.TabIndex = 6;
            this.groupBoxLabelSettings.TabStop = false;
            this.groupBoxLabelSettings.Text = "Cài đặt nhãn";
            // 
            // lblBarcodeModuleWidth
            // 
            this.lblBarcodeModuleWidth.AutoSize = true;
            this.lblBarcodeModuleWidth.Location = new System.Drawing.Point(5, 28);
            this.lblBarcodeModuleWidth.Name = "lblBarcodeModuleWidth";
            this.lblBarcodeModuleWidth.Size = new System.Drawing.Size(147, 20);
            this.lblBarcodeModuleWidth.TabIndex = 0;
            this.lblBarcodeModuleWidth.Text = "Module Width (1-5):";
            // 
            // numBarcodeModuleWidth
            // 
            this.numBarcodeModuleWidth.Location = new System.Drawing.Point(160, 25);
            this.numBarcodeModuleWidth.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numBarcodeModuleWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBarcodeModuleWidth.Name = "numBarcodeModuleWidth";
            this.numBarcodeModuleWidth.Size = new System.Drawing.Size(80, 26);
            this.numBarcodeModuleWidth.TabIndex = 1;
            this.numBarcodeModuleWidth.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // lblLabelLength
            // 
            this.lblLabelLength.AutoSize = true;
            this.lblLabelLength.Location = new System.Drawing.Point(270, 28);
            this.lblLabelLength.Name = "lblLabelLength";
            this.lblLabelLength.Size = new System.Drawing.Size(151, 20);
            this.lblLabelLength.TabIndex = 2;
            this.lblLabelLength.Text = "Label Length (dots):";
            // 
            // numLabelLength
            // 
            this.numLabelLength.Location = new System.Drawing.Point(410, 25);
            this.numLabelLength.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numLabelLength.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numLabelLength.Name = "numLabelLength";
            this.numLabelLength.Size = new System.Drawing.Size(100, 26);
            this.numLabelLength.TabIndex = 3;
            this.numLabelLength.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            // 
            // groupBoxCommands
            // 
            this.groupBoxCommands.Controls.Add(this.lblConnectionStatus);
            this.groupBoxCommands.Controls.Add(this.btnSendCustomZPL);
            this.groupBoxCommands.Controls.Add(this.txtCustomZPL);
            this.groupBoxCommands.Controls.Add(this.btnSendReady);
            this.groupBoxCommands.Location = new System.Drawing.Point(15, 417);
            this.groupBoxCommands.Name = "groupBoxCommands";
            this.groupBoxCommands.Size = new System.Drawing.Size(455, 78);
            this.groupBoxCommands.TabIndex = 4;
            this.groupBoxCommands.TabStop = false;
            this.groupBoxCommands.Text = "Commands";
            this.groupBoxCommands.Visible = false;
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.Red;
            this.lblConnectionStatus.Location = new System.Drawing.Point(241, 16);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(124, 15);
            this.lblConnectionStatus.TabIndex = 0;
            this.lblConnectionStatus.Text = "Status: Disconnected";
            // 
            // btnSendCustomZPL
            // 
            this.btnSendCustomZPL.Location = new System.Drawing.Point(357, 43);
            this.btnSendCustomZPL.Name = "btnSendCustomZPL";
            this.btnSendCustomZPL.Size = new System.Drawing.Size(77, 26);
            this.btnSendCustomZPL.TabIndex = 5;
            this.btnSendCustomZPL.Text = "Send ZPL";
            this.btnSendCustomZPL.UseVisualStyleBackColor = true;
            this.btnSendCustomZPL.Click += new System.EventHandler(this.btnSendCustomZPL_Click);
            // 
            // txtCustomZPL
            // 
            this.txtCustomZPL.Location = new System.Drawing.Point(5, 43);
            this.txtCustomZPL.Name = "txtCustomZPL";
            this.txtCustomZPL.Size = new System.Drawing.Size(343, 20);
            this.txtCustomZPL.TabIndex = 4;
            this.txtCustomZPL.Text = "^XA^XZ";
            // 
            // btnSendReady
            // 
            this.btnSendReady.Location = new System.Drawing.Point(113, 19);
            this.btnSendReady.Name = "btnSendReady";
            this.btnSendReady.Size = new System.Drawing.Size(103, 26);
            this.btnSendReady.TabIndex = 1;
            this.btnSendReady.Text = "Send Ready";
            this.btnSendReady.UseVisualStyleBackColor = true;
            this.btnSendReady.Click += new System.EventHandler(this.btnSendReady_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnPrint.Location = new System.Drawing.Point(10, 364);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(103, 26);
            this.btnPrint.TabIndex = 0;
            this.btnPrint.Text = "In mẫu";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // groupBoxStatus
            // 
            this.groupBoxStatus.Controls.Add(this.txtStatusLog);
            this.groupBoxStatus.Location = new System.Drawing.Point(10, 502);
            this.groupBoxStatus.Name = "groupBoxStatus";
            this.groupBoxStatus.Size = new System.Drawing.Size(665, 170);
            this.groupBoxStatus.TabIndex = 5;
            this.groupBoxStatus.TabStop = false;
            this.groupBoxStatus.Text = "Status & Log";
            this.groupBoxStatus.Visible = false;
            // 
            // txtStatusLog
            // 
            this.txtStatusLog.Location = new System.Drawing.Point(5, 20);
            this.txtStatusLog.Multiline = true;
            this.txtStatusLog.Name = "txtStatusLog";
            this.txtStatusLog.ReadOnly = true;
            this.txtStatusLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatusLog.Size = new System.Drawing.Size(655, 143);
            this.txtStatusLog.TabIndex = 1;
            this.txtStatusLog.Visible = false;
            // 
            // ucZebraPrinterSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBoxLabelSettings);
            this.Controls.Add(this.groupBoxStatus);
            this.Controls.Add(this.txtBarcodeText);
            this.Controls.Add(this.groupBoxCommands);
            this.Controls.Add(this.groupBoxBarcode);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.groupBoxText);
            this.Controls.Add(this.lblBarcodeText);
            this.Controls.Add(groupBoxConnection);
            this.Name = "ucZebraPrinterSetting";
            this.Size = new System.Drawing.Size(686, 694);
            groupBoxConnection.ResumeLayout(false);
            groupBoxConnection.PerformLayout();
            this.pnlUSB.ResumeLayout(false);
            this.pnlUSB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPrinterPort)).EndInit();
            this.groupBoxText.ResumeLayout(false);
            this.groupBoxText.PerformLayout();
            this.groupBoxBarcode.ResumeLayout(false);
            this.groupBoxBarcode.PerformLayout();
            this.groupBoxLabelSettings.ResumeLayout(false);
            this.groupBoxLabelSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBarcodeModuleWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLabelLength)).EndInit();
            this.groupBoxCommands.ResumeLayout(false);
            this.groupBoxCommands.PerformLayout();
            this.groupBoxStatus.ResumeLayout(false);
            this.groupBoxStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        // ── Controls mới ──────────────────────────────────────────────
        private System.Windows.Forms.RadioButton radLAN;
        private System.Windows.Forms.RadioButton radUSB;
        private System.Windows.Forms.Panel pnlUSB;
        private System.Windows.Forms.ComboBox cmbUsbPrinterName;
        private System.Windows.Forms.Button btnRefreshPrinters;
        private System.Windows.Forms.Label lblUsbPrinterName;

        // ── Controls cũ ───────────────────────────────────────────────
        private GroupBox groupBoxText;
        private CheckBox chkEnableText;
        private ComboBox cmbTextFont;
        private TextBox txtTextContent;
        private TextBox txtTextY;
        private TextBox txtTextX;
        private TextBox txtTextFontSize;
        private Label lblTextFont;
        private Label lblTextContent;
        private Label lblTextY;
        private Label lblTextX;
        private Label lblTextFontSize;
        private GroupBox groupBoxBarcode;
        private CheckBox chkEnableBarcode;
        private TextBox txtBarcodeText;
        private ComboBox cmbBarcodeType;
        private TextBox txtBarcodeSize;
        private TextBox txtBarcodeY;
        private TextBox txtBarcodeX;
        private TextBox txtBarcodeData;
        private Label lblBarcodeText;
        private Label lblBarcodeType;
        private Label lblBarcodeSize;
        private Label lblBarcodeY;
        private Label lblBarcodeX;
        private Label lblBarcodeData;
        private GroupBox groupBoxLabelSettings;
        private System.Windows.Forms.NumericUpDown numBarcodeModuleWidth;
        private System.Windows.Forms.NumericUpDown numLabelLength;
        private Label lblBarcodeModuleWidth;
        private Label lblLabelLength;
        private GroupBox groupBoxCommands;
        private Button btnSendCustomZPL;
        private TextBox txtCustomZPL;
        private Button btnSendReady;
        private Button btnPrint;
        private GroupBox groupBoxStatus;
        private TextBox txtStatusLog;
        private Label lblConnectionStatus;
        private NumericUpDown numPrinterPort;
        private Label lblIPrinterIP;
        private Label lblPrinterPort;
        private IPAddressControlLib.IPAddressControl txtPrinterIP;
    }
}