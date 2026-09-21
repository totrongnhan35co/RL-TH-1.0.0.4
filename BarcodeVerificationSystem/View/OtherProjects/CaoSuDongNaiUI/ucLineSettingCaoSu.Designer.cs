namespace BarcodeVerificationSystem.View.CaoSuDongNaiUI
{
    partial class ucLineSettingCaoSu
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
            this.groupBoxProductionSettings = new System.Windows.Forms.GroupBox();
            this.FactoryCodeCombox = new System.Windows.Forms.ComboBox();
            this.RLinkNamescombox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.factoryCodeLabel = new System.Windows.Forms.Label();
            this.groupBoxProductionSettings.SuspendLayout();
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
            // groupBoxProductionSettings
            // 
            this.groupBoxProductionSettings.Controls.Add(this.FactoryCodeCombox);
            this.groupBoxProductionSettings.Controls.Add(this.RLinkNamescombox);
            this.groupBoxProductionSettings.Controls.Add(this.label1);
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
            // FactoryCodeCombox
            // 
            this.FactoryCodeCombox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FactoryCodeCombox.FormattingEnabled = true;
            this.FactoryCodeCombox.Items.AddRange(new object[] {
            "R1",
            "R2",
            "R3",
            "R4"});
            this.FactoryCodeCombox.Location = new System.Drawing.Point(233, 217);
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
            this.RLinkNamescombox.Location = new System.Drawing.Point(233, 141);
            this.RLinkNamescombox.Name = "RLinkNamescombox";
            this.RLinkNamescombox.Size = new System.Drawing.Size(160, 28);
            this.RLinkNamescombox.TabIndex = 78;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(66, 144);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 20);
            this.label1.TabIndex = 78;
            this.label1.Text = "Tên Line:";
            // 
            // factoryCodeLabel
            // 
            this.factoryCodeLabel.AutoSize = true;
            this.factoryCodeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.factoryCodeLabel.Location = new System.Drawing.Point(66, 220);
            this.factoryCodeLabel.Name = "factoryCodeLabel";
            this.factoryCodeLabel.Size = new System.Drawing.Size(99, 20);
            this.factoryCodeLabel.TabIndex = 72;
            this.factoryCodeLabel.Text = "Mã nhà máy:";
            // 
            // ucLineSettingCaoSu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxProductionSettings);
            this.Name = "ucLineSettingCaoSu";
            this.Size = new System.Drawing.Size(990, 500);
            this.groupBoxProductionSettings.ResumeLayout(false);
            this.groupBoxProductionSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox apiTextbox;
        private System.Windows.Forms.Label labelApi;
        private System.Windows.Forms.GroupBox groupBoxProductionSettings;
        private System.Windows.Forms.Label factoryCodeLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox FactoryCodeCombox;
        private System.Windows.Forms.ComboBox RLinkNamescombox;
    }
}
