namespace BarcodeVerificationSystem.View.UcSettings
{
    partial class ucProductionNutiSetting
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
            this.apiTextbox = new System.Windows.Forms.TextBox();
            this.labelApi = new System.Windows.Forms.Label();
            this.comboBoxRLinkId = new System.Windows.Forms.ComboBox();
            this.lineIdLabel = new System.Windows.Forms.Label();
            this.groupBoxProductionSettings = new System.Windows.Forms.GroupBox();
            this.dispatchingRad = new System.Windows.Forms.RadioButton();
            this.manufacturingRad = new System.Windows.Forms.RadioButton();
            this.productionMode = new System.Windows.Forms.Label();
            this.FactoryCodeCombox = new System.Windows.Forms.ComboBox();
            this.RLinkNamescombox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.onlineProductionSettings = new System.Windows.Forms.Panel();
            this.LineId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dataIncrease = new System.Windows.Forms.Label();
            this.numIncreasedData = new System.Windows.Forms.NumericUpDown();
            this.lineNameLabel = new System.Windows.Forms.Label();
            this.maskData = new System.Windows.Forms.CheckBox();
            this.lineName = new System.Windows.Forms.TextBox();
            this.dataDisplay = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.factoryCodeLabel = new System.Windows.Forms.Label();
            this.HideFunctions = new System.Windows.Forms.CheckBox();
            this.groupBoxProductionSettings.SuspendLayout();
            this.onlineProductionSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIncreasedData)).BeginInit();
            this.SuspendLayout();
            // 
            // apiTextbox
            // 
            this.apiTextbox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.apiTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.apiTextbox.Location = new System.Drawing.Point(233, 65);
            this.apiTextbox.MinimumSize = new System.Drawing.Size(361, 30);
            this.apiTextbox.Name = "apiTextbox";
            this.apiTextbox.Size = new System.Drawing.Size(439, 26);
            this.apiTextbox.TabIndex = 0;
            // 
            // labelApi
            // 
            this.labelApi.AutoSize = true;
            this.labelApi.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelApi.Location = new System.Drawing.Point(66, 68);
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
            // groupBoxProductionSettings
            // 
            this.groupBoxProductionSettings.Controls.Add(this.HideFunctions);
            this.groupBoxProductionSettings.Controls.Add(this.dispatchingRad);
            this.groupBoxProductionSettings.Controls.Add(this.manufacturingRad);
            this.groupBoxProductionSettings.Controls.Add(this.productionMode);
            this.groupBoxProductionSettings.Controls.Add(this.FactoryCodeCombox);
            this.groupBoxProductionSettings.Controls.Add(this.RLinkNamescombox);
            this.groupBoxProductionSettings.Controls.Add(this.label1);
            this.groupBoxProductionSettings.Controls.Add(this.onlineProductionSettings);
            this.groupBoxProductionSettings.Controls.Add(this.progressBar1);
            this.groupBoxProductionSettings.Controls.Add(this.factoryCodeLabel);
            this.groupBoxProductionSettings.Controls.Add(this.labelApi);
            this.groupBoxProductionSettings.Controls.Add(this.apiTextbox);
            this.groupBoxProductionSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxProductionSettings.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBoxProductionSettings.Location = new System.Drawing.Point(3, 3);
            this.groupBoxProductionSettings.Name = "groupBoxProductionSettings";
            this.groupBoxProductionSettings.Size = new System.Drawing.Size(984, 494);
            this.groupBoxProductionSettings.TabIndex = 50;
            this.groupBoxProductionSettings.TabStop = false;
            this.groupBoxProductionSettings.Text = "Cài đặt Line";
            // 
            // dispatchingRad
            // 
            this.dispatchingRad.AutoSize = true;
            this.dispatchingRad.Location = new System.Drawing.Point(433, 138);
            this.dispatchingRad.Name = "dispatchingRad";
            this.dispatchingRad.Size = new System.Drawing.Size(122, 24);
            this.dispatchingRad.TabIndex = 45;
            this.dispatchingRad.TabStop = true;
            this.dispatchingRad.Text = "Dispatching";
            this.dispatchingRad.UseVisualStyleBackColor = true;
            // 
            // manufacturingRad
            // 
            this.manufacturingRad.AutoSize = true;
            this.manufacturingRad.Location = new System.Drawing.Point(233, 136);
            this.manufacturingRad.Name = "manufacturingRad";
            this.manufacturingRad.Size = new System.Drawing.Size(142, 24);
            this.manufacturingRad.TabIndex = 44;
            this.manufacturingRad.TabStop = true;
            this.manufacturingRad.Text = "Manufacturing";
            this.manufacturingRad.UseVisualStyleBackColor = true;
            // 
            // productionMode
            // 
            this.productionMode.AutoSize = true;
            this.productionMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.productionMode.Location = new System.Drawing.Point(66, 140);
            this.productionMode.Name = "productionMode";
            this.productionMode.Size = new System.Drawing.Size(133, 20);
            this.productionMode.TabIndex = 43;
            this.productionMode.Text = "Production Mode:";
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
            this.FactoryCodeCombox.Location = new System.Drawing.Point(233, 280);
            this.FactoryCodeCombox.Name = "FactoryCodeCombox";
            this.FactoryCodeCombox.Size = new System.Drawing.Size(160, 28);
            this.FactoryCodeCombox.TabIndex = 81;
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
            this.RLinkNamescombox.Location = new System.Drawing.Point(233, 204);
            this.RLinkNamescombox.Name = "RLinkNamescombox";
            this.RLinkNamescombox.Size = new System.Drawing.Size(160, 28);
            this.RLinkNamescombox.TabIndex = 78;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(66, 207);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 20);
            this.label1.TabIndex = 78;
            this.label1.Text = "Tên thiết bị:";
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
            this.onlineProductionSettings.Location = new System.Drawing.Point(688, 413);
            this.onlineProductionSettings.Name = "onlineProductionSettings";
            this.onlineProductionSettings.Size = new System.Drawing.Size(277, 75);
            this.onlineProductionSettings.TabIndex = 51;
            this.onlineProductionSettings.Visible = false;
            // 
            // LineId
            // 
            this.LineId.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LineId.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LineId.Location = new System.Drawing.Point(96, 12);
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
            this.lineName.Location = new System.Drawing.Point(73, 152);
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
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(433, 207);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(10, 284);
            this.progressBar1.TabIndex = 76;
            this.progressBar1.Visible = false;
            // 
            // factoryCodeLabel
            // 
            this.factoryCodeLabel.AutoSize = true;
            this.factoryCodeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.factoryCodeLabel.Location = new System.Drawing.Point(66, 283);
            this.factoryCodeLabel.Name = "factoryCodeLabel";
            this.factoryCodeLabel.Size = new System.Drawing.Size(99, 20);
            this.factoryCodeLabel.TabIndex = 72;
            this.factoryCodeLabel.Text = "Mã nhà máy:";
            // 
            // HideFunctions
            // 
            this.HideFunctions.AutoSize = true;
            this.HideFunctions.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideFunctions.Location = new System.Drawing.Point(67, 363);
            this.HideFunctions.Name = "HideFunctions";
            this.HideFunctions.Size = new System.Drawing.Size(132, 24);
            this.HideFunctions.TabIndex = 82;
            this.HideFunctions.Text = "Tính năng ẩn";
            this.HideFunctions.UseVisualStyleBackColor = true;
            // 
            // ucProductionSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxProductionSettings);
            this.Name = "ucProductionSetting";
            this.Size = new System.Drawing.Size(990, 500);
            this.groupBoxProductionSettings.ResumeLayout(false);
            this.groupBoxProductionSettings.PerformLayout();
            this.onlineProductionSettings.ResumeLayout(false);
            this.onlineProductionSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIncreasedData)).EndInit();
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
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox LineId;
        private System.Windows.Forms.Panel onlineProductionSettings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox FactoryCodeCombox;
        private System.Windows.Forms.ComboBox RLinkNamescombox;
        private System.Windows.Forms.CheckBox HideFunctions;
    }
}
