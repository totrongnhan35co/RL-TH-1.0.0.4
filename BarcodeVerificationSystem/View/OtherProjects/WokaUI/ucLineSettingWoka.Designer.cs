namespace BarcodeVerificationSystem.View.WokaUI
{
    partial class ucLineSettingWoka
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
            this.RLinkNamescombox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.WokaToken = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
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
            this.groupBoxProductionSettings.Controls.Add(this.label2);
            this.groupBoxProductionSettings.Controls.Add(this.WokaToken);
            this.groupBoxProductionSettings.Controls.Add(this.RLinkNamescombox);
            this.groupBoxProductionSettings.Controls.Add(this.label1);
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
            // WokaToken
            // 
            this.WokaToken.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.WokaToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WokaToken.Location = new System.Drawing.Point(233, 219);
            this.WokaToken.MinimumSize = new System.Drawing.Size(361, 30);
            this.WokaToken.Name = "WokaToken";
            this.WokaToken.Size = new System.Drawing.Size(439, 30);
            this.WokaToken.TabIndex = 79;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(66, 229);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 20);
            this.label2.TabIndex = 80;
            this.label2.Text = "Token:";
            // 
            // ucLineSettingWoka
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxProductionSettings);
            this.Name = "ucLineSettingWoka";
            this.Size = new System.Drawing.Size(990, 500);
            this.groupBoxProductionSettings.ResumeLayout(false);
            this.groupBoxProductionSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox apiTextbox;
        private System.Windows.Forms.Label labelApi;
        private System.Windows.Forms.GroupBox groupBoxProductionSettings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox RLinkNamescombox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox WokaToken;
    }
}
