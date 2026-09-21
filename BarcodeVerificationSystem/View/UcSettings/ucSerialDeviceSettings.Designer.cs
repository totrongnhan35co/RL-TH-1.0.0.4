namespace BarcodeVerificationSystem.View
{
    partial class ucSerialDeviceSettings
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
            this.bitsPerSecond = new System.Windows.Forms.Label();
            this.comboBoxBitPerSeconds = new System.Windows.Forms.ComboBox();
            this.dataBits = new System.Windows.Forms.Label();
            this.comboBoxDataBits = new System.Windows.Forms.ComboBox();
            this.parity = new System.Windows.Forms.Label();
            this.comboBoxParity = new System.Windows.Forms.ComboBox();
            this.stopBits = new System.Windows.Forms.Label();
            this.comboBoxStopBits = new System.Windows.Forms.ComboBox();
            this.comName = new System.Windows.Forms.Label();
            this.comboBoxComName = new System.Windows.Forms.ComboBox();
            this.groupBoxSerial = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.radioEnScanner = new System.Windows.Forms.RadioButton();
            this.radioDisScanner = new System.Windows.Forms.RadioButton();
            this.groupBoxSerial.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bitsPerSecond
            // 
            this.bitsPerSecond.AutoSize = true;
            this.bitsPerSecond.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bitsPerSecond.Location = new System.Drawing.Point(50, 126);
            this.bitsPerSecond.Name = "bitsPerSecond";
            this.bitsPerSecond.Size = new System.Drawing.Size(127, 20);
            this.bitsPerSecond.TabIndex = 39;
            this.bitsPerSecond.Text = "Bits per second: ";
            // 
            // comboBoxBitPerSeconds
            // 
            this.comboBoxBitPerSeconds.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxBitPerSeconds.FormattingEnabled = true;
            this.comboBoxBitPerSeconds.Items.AddRange(new object[] {
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.comboBoxBitPerSeconds.Location = new System.Drawing.Point(183, 123);
            this.comboBoxBitPerSeconds.Name = "comboBoxBitPerSeconds";
            this.comboBoxBitPerSeconds.Size = new System.Drawing.Size(276, 28);
            this.comboBoxBitPerSeconds.TabIndex = 40;
            // 
            // dataBits
            // 
            this.dataBits.AutoSize = true;
            this.dataBits.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataBits.Location = new System.Drawing.Point(50, 164);
            this.dataBits.Name = "dataBits";
            this.dataBits.Size = new System.Drawing.Size(77, 20);
            this.dataBits.TabIndex = 41;
            this.dataBits.Text = "Data bits:";
            // 
            // comboBoxDataBits
            // 
            this.comboBoxDataBits.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxDataBits.FormattingEnabled = true;
            this.comboBoxDataBits.Items.AddRange(new object[] {
            "7",
            "8"});
            this.comboBoxDataBits.Location = new System.Drawing.Point(183, 161);
            this.comboBoxDataBits.Name = "comboBoxDataBits";
            this.comboBoxDataBits.Size = new System.Drawing.Size(276, 28);
            this.comboBoxDataBits.TabIndex = 42;
            // 
            // parity
            // 
            this.parity.AutoSize = true;
            this.parity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.parity.Location = new System.Drawing.Point(50, 202);
            this.parity.Name = "parity";
            this.parity.Size = new System.Drawing.Size(56, 20);
            this.parity.TabIndex = 43;
            this.parity.Text = "Parity: ";
            // 
            // comboBoxParity
            // 
            this.comboBoxParity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxParity.FormattingEnabled = true;
            this.comboBoxParity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even"});
            this.comboBoxParity.Location = new System.Drawing.Point(183, 199);
            this.comboBoxParity.Name = "comboBoxParity";
            this.comboBoxParity.Size = new System.Drawing.Size(276, 28);
            this.comboBoxParity.TabIndex = 44;
            // 
            // stopBits
            // 
            this.stopBits.AutoSize = true;
            this.stopBits.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stopBits.Location = new System.Drawing.Point(50, 240);
            this.stopBits.Name = "stopBits";
            this.stopBits.Size = new System.Drawing.Size(80, 20);
            this.stopBits.TabIndex = 45;
            this.stopBits.Text = "Stop bits: ";
            // 
            // comboBoxStopBits
            // 
            this.comboBoxStopBits.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxStopBits.FormattingEnabled = true;
            this.comboBoxStopBits.Items.AddRange(new object[] {
            "1",
            "2"});
            this.comboBoxStopBits.Location = new System.Drawing.Point(183, 237);
            this.comboBoxStopBits.Name = "comboBoxStopBits";
            this.comboBoxStopBits.Size = new System.Drawing.Size(276, 28);
            this.comboBoxStopBits.TabIndex = 46;
            // 
            // comName
            // 
            this.comName.AutoSize = true;
            this.comName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comName.Location = new System.Drawing.Point(50, 88);
            this.comName.Name = "comName";
            this.comName.Size = new System.Drawing.Size(95, 20);
            this.comName.TabIndex = 47;
            this.comName.Text = "COM Name:";
            // 
            // comboBoxComName
            // 
            this.comboBoxComName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxComName.FormattingEnabled = true;
            this.comboBoxComName.Items.AddRange(new object[] {
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7"});
            this.comboBoxComName.Location = new System.Drawing.Point(183, 85);
            this.comboBoxComName.Name = "comboBoxComName";
            this.comboBoxComName.Size = new System.Drawing.Size(276, 28);
            this.comboBoxComName.TabIndex = 48;
            // 
            // groupBoxSerial
            // 
            this.groupBoxSerial.Controls.Add(this.tableLayoutPanel1);
            this.groupBoxSerial.Controls.Add(this.comboBoxComName);
            this.groupBoxSerial.Controls.Add(this.bitsPerSecond);
            this.groupBoxSerial.Controls.Add(this.comName);
            this.groupBoxSerial.Controls.Add(this.comboBoxBitPerSeconds);
            this.groupBoxSerial.Controls.Add(this.comboBoxStopBits);
            this.groupBoxSerial.Controls.Add(this.dataBits);
            this.groupBoxSerial.Controls.Add(this.stopBits);
            this.groupBoxSerial.Controls.Add(this.comboBoxDataBits);
            this.groupBoxSerial.Controls.Add(this.comboBoxParity);
            this.groupBoxSerial.Controls.Add(this.parity);
            this.groupBoxSerial.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxSerial.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBoxSerial.Location = new System.Drawing.Point(0, 3);
            this.groupBoxSerial.Name = "groupBoxSerial";
            this.groupBoxSerial.Size = new System.Drawing.Size(987, 492);
            this.groupBoxSerial.TabIndex = 49;
            this.groupBoxSerial.TabStop = false;
            this.groupBoxSerial.Text = "Scanner Settings";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.radioEnScanner, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.radioDisScanner, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(183, 35);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(276, 26);
            this.tableLayoutPanel1.TabIndex = 50;
            // 
            // radioEnScanner
            // 
            this.radioEnScanner.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioEnScanner.BackColor = System.Drawing.Color.White;
            this.radioEnScanner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioEnScanner.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radioEnScanner.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioEnScanner.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioEnScanner.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioEnScanner.Location = new System.Drawing.Point(0, 0);
            this.radioEnScanner.Margin = new System.Windows.Forms.Padding(0);
            this.radioEnScanner.Name = "radioEnScanner";
            this.radioEnScanner.Size = new System.Drawing.Size(138, 26);
            this.radioEnScanner.TabIndex = 5;
            this.radioEnScanner.TabStop = true;
            this.radioEnScanner.Text = "Enable";
            this.radioEnScanner.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioEnScanner.UseVisualStyleBackColor = false;
            // 
            // radioDisScanner
            // 
            this.radioDisScanner.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioDisScanner.BackColor = System.Drawing.Color.White;
            this.radioDisScanner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioDisScanner.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.radioDisScanner.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioDisScanner.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioDisScanner.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioDisScanner.Location = new System.Drawing.Point(138, 0);
            this.radioDisScanner.Margin = new System.Windows.Forms.Padding(0);
            this.radioDisScanner.Name = "radioDisScanner";
            this.radioDisScanner.Size = new System.Drawing.Size(138, 26);
            this.radioDisScanner.TabIndex = 6;
            this.radioDisScanner.TabStop = true;
            this.radioDisScanner.Text = "Disable";
            this.radioDisScanner.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioDisScanner.UseVisualStyleBackColor = false;
            // 
            // ucSerialDeviceSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxSerial);
            this.Name = "ucSerialDeviceSettings";
            this.Size = new System.Drawing.Size(990, 514);
            this.groupBoxSerial.ResumeLayout(false);
            this.groupBoxSerial.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label bitsPerSecond;
        private System.Windows.Forms.ComboBox comboBoxBitPerSeconds;
        private System.Windows.Forms.Label dataBits;
        private System.Windows.Forms.ComboBox comboBoxDataBits;
        private System.Windows.Forms.Label parity;
        private System.Windows.Forms.ComboBox comboBoxParity;
        private System.Windows.Forms.Label stopBits;
        private System.Windows.Forms.ComboBox comboBoxStopBits;
        private System.Windows.Forms.Label comName;
        private System.Windows.Forms.ComboBox comboBoxComName;
        private System.Windows.Forms.GroupBox groupBoxSerial;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RadioButton radioEnScanner;
        private System.Windows.Forms.RadioButton radioDisScanner;
    }
}
