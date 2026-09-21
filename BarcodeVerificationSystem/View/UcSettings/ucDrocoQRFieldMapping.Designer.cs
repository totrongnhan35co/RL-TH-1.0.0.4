namespace BarcodeVerificationSystem.View.OtherProjects.DrocoUI
{
    partial class ucDrocoQRFieldMapping
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.chkUseExcelMode = new System.Windows.Forms.CheckBox();
            this.lblModeStatus = new System.Windows.Forms.Label();
            this.grpFile = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.grpMapping = new System.Windows.Forms.GroupBox();
            this.lblProduct = new System.Windows.Forms.Label();
            this.cboProduct = new System.Windows.Forms.ComboBox();
            this.lblBox = new System.Windows.Forms.Label();
            this.cboBox = new System.Windows.Forms.ComboBox();
            this.lblCarton = new System.Windows.Forms.Label();
            this.cboCarton = new System.Windows.Forms.ComboBox();
            this.lblPallet = new System.Windows.Forms.Label();
            this.cboPallet = new System.Windows.Forms.ComboBox();
            this.lblNote = new System.Windows.Forms.Label();
            this.grpPreview = new System.Windows.Forms.GroupBox();
            this.lblPreview = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.grpFile.SuspendLayout();
            this.grpMapping.SuspendLayout();
            this.grpPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkUseExcelMode
            // 
            this.chkUseExcelMode.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.chkUseExcelMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.chkUseExcelMode.Location = new System.Drawing.Point(10, 10);
            this.chkUseExcelMode.Name = "chkUseExcelMode";
            this.chkUseExcelMode.Size = new System.Drawing.Size(650, 30);
            this.chkUseExcelMode.TabIndex = 0;
            this.chkUseExcelMode.Text = " Bật chế độ sinh QR từ file ";
            // 
            // lblModeStatus
            // 
            this.lblModeStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblModeStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblModeStatus.Location = new System.Drawing.Point(670, 15);
            this.lblModeStatus.Name = "lblModeStatus";
            this.lblModeStatus.Size = new System.Drawing.Size(300, 22);
            this.lblModeStatus.TabIndex = 1;
            this.lblModeStatus.Text = "Dang dung sinh tu dong";
            // 
            // grpFile
            // 
            this.grpFile.Controls.Add(this.btnBrowse);
            this.grpFile.Controls.Add(this.lblFilePath);
            this.grpFile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpFile.Location = new System.Drawing.Point(10, 50);
            this.grpFile.Name = "grpFile";
            this.grpFile.Size = new System.Drawing.Size(970, 75);
            this.grpFile.TabIndex = 2;
            this.grpFile.TabStop = false;
            this.grpFile.Text = "1. Chọn file Excel mẫu ";
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Location = new System.Drawing.Point(12, 28);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(130, 32);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "Chọn file";
            this.btnBrowse.UseVisualStyleBackColor = false;
            // 
            // lblFilePath
            // 
            this.lblFilePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFilePath.ForeColor = System.Drawing.Color.Gray;
            this.lblFilePath.Location = new System.Drawing.Point(155, 35);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(800, 20);
            this.lblFilePath.TabIndex = 1;
            this.lblFilePath.Text = "(Chua chon file)";
            // 
            // grpMapping
            // 
            this.grpMapping.Controls.Add(this.lblProduct);
            this.grpMapping.Controls.Add(this.cboProduct);
            this.grpMapping.Controls.Add(this.lblBox);
            this.grpMapping.Controls.Add(this.cboBox);
            this.grpMapping.Controls.Add(this.lblCarton);
            this.grpMapping.Controls.Add(this.cboCarton);
            this.grpMapping.Controls.Add(this.lblPallet);
            this.grpMapping.Controls.Add(this.cboPallet);
            this.grpMapping.Controls.Add(this.lblNote);
            this.grpMapping.Enabled = false;
            this.grpMapping.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpMapping.Location = new System.Drawing.Point(10, 135);
            this.grpMapping.Name = "grpMapping";
            this.grpMapping.Size = new System.Drawing.Size(970, 220);
            this.grpMapping.TabIndex = 3;
            this.grpMapping.TabStop = false;
            this.grpMapping.Text = "2. Gán cột Excel -> cap dong goi (tu dong goi y theo ten cot)";
            // 
            // lblProduct
            // 
            this.lblProduct.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblProduct.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.lblProduct.Location = new System.Drawing.Point(10, 28);
            this.lblProduct.Name = "lblProduct";
            this.lblProduct.Size = new System.Drawing.Size(200, 26);
            this.lblProduct.TabIndex = 0;
            this.lblProduct.Text = "Mã Sản Phẩm (GS1):";
            this.lblProduct.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboProduct
            // 
            this.cboProduct.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProduct.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboProduct.Location = new System.Drawing.Point(218, 28);
            this.cboProduct.Name = "cboProduct";
            this.cboProduct.Size = new System.Drawing.Size(400, 25);
            this.cboProduct.TabIndex = 1;
            // 
            // lblBox
            // 
            this.lblBox.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.lblBox.Location = new System.Drawing.Point(10, 68);
            this.lblBox.Name = "lblBox";
            this.lblBox.Size = new System.Drawing.Size(200, 26);
            this.lblBox.TabIndex = 2;
            this.lblBox.Text = "Mã Hộp (Box):";
            this.lblBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboBox
            // 
            this.cboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboBox.Location = new System.Drawing.Point(218, 68);
            this.cboBox.Name = "cboBox";
            this.cboBox.Size = new System.Drawing.Size(400, 25);
            this.cboBox.TabIndex = 3;
            // 
            // lblCarton
            // 
            this.lblCarton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCarton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.lblCarton.Location = new System.Drawing.Point(10, 108);
            this.lblCarton.Name = "lblCarton";
            this.lblCarton.Size = new System.Drawing.Size(200, 26);
            this.lblCarton.TabIndex = 4;
            this.lblCarton.Text = "Mã Thùng (Carton):";
            this.lblCarton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboCarton
            // 
            this.cboCarton.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCarton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboCarton.Location = new System.Drawing.Point(218, 108);
            this.cboCarton.Name = "cboCarton";
            this.cboCarton.Size = new System.Drawing.Size(400, 25);
            this.cboCarton.TabIndex = 5;
            // 
            // lblPallet
            // 
            this.lblPallet.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPallet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.lblPallet.Location = new System.Drawing.Point(10, 148);
            this.lblPallet.Name = "lblPallet";
            this.lblPallet.Size = new System.Drawing.Size(200, 26);
            this.lblPallet.TabIndex = 6;
            this.lblPallet.Text = "Mã Pallet";
            this.lblPallet.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboPallet
            // 
            this.cboPallet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPallet.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboPallet.Location = new System.Drawing.Point(218, 148);
            this.cboPallet.Name = "cboPallet";
            this.cboPallet.Size = new System.Drawing.Size(400, 25);
            this.cboPallet.TabIndex = 7;
            // 
            // lblNote
            // 
            this.lblNote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblNote.ForeColor = System.Drawing.Color.Gray;
            this.lblNote.Location = new System.Drawing.Point(10, 190);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(940, 20);
            this.lblNote.TabIndex = 8;
            this.lblNote.Text = "He thong tu dong goi y dua theo ten cot. Ban co the thay doi neu can.";
            // 
            // grpPreview
            // 
            this.grpPreview.Controls.Add(this.lblPreview);
            this.grpPreview.Enabled = false;
            this.grpPreview.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpPreview.Location = new System.Drawing.Point(10, 365);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Size = new System.Drawing.Size(970, 65);
            this.grpPreview.TabIndex = 4;
            this.grpPreview.TabStop = false;
            this.grpPreview.Text = "3. Kết quả ánh xạ";
            // 
            // lblPreview
            // 
            this.lblPreview.Font = new System.Drawing.Font("Courier New", 10F);
            this.lblPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(0)))));
            this.lblPreview.Location = new System.Drawing.Point(10, 25);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(945, 28);
            this.lblPreview.TabIndex = 0;
            this.lblPreview.Text = "(Chua co du lieu)";
            // 
            // btnSave
            // 
       
          
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(10, 442);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(180, 40);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Lưu cấu hình";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // ucDrocoQRFieldMapping
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.chkUseExcelMode);
            this.Controls.Add(this.lblModeStatus);
            this.Controls.Add(this.grpFile);
            this.Controls.Add(this.grpMapping);
            this.Controls.Add(this.grpPreview);
            this.Controls.Add(this.btnSave);
            this.Name = "ucDrocoQRFieldMapping";
            this.Size = new System.Drawing.Size(990, 494);
            this.grpFile.ResumeLayout(false);
            this.grpMapping.ResumeLayout(false);
            this.grpPreview.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.CheckBox chkUseExcelMode;
        private System.Windows.Forms.Label lblModeStatus;
        private System.Windows.Forms.GroupBox grpFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.GroupBox grpMapping;
        private System.Windows.Forms.Label lblProduct;
        private System.Windows.Forms.ComboBox cboProduct;
        private System.Windows.Forms.Label lblBox;
        private System.Windows.Forms.ComboBox cboBox;
        private System.Windows.Forms.Label lblCarton;
        private System.Windows.Forms.ComboBox cboCarton;
        private System.Windows.Forms.Label lblPallet;
        private System.Windows.Forms.ComboBox cboPallet;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.Button btnSave;
    }
}