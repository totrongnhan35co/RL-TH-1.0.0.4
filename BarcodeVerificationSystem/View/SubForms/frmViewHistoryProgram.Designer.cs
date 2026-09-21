namespace BarcodeVerificationSystem.View
{
    partial class FrmViewHistoryProgram
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmViewHistoryProgram));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.datTo = new DesignUI.CuzUI.CuzDateTimePicker();
            this.datFrom = new DesignUI.CuzUI.CuzDateTimePicker();
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblTo = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlDrag = new System.Windows.Forms.Panel();
            this.cuzControlBox1 = new DesignUI.CuzUI.CuzControlBox();
            this.lblFormName = new System.Windows.Forms.Label();
            this.cuzDragControl1 = new DesignUI.CuzUI.CuzDragControl();
            this.btnExport = new DesignUI.CuzUI.CuzButton();
            this.btnRefresh = new DesignUI.CuzUI.CuzButton();
            this.btnClearLog = new DesignUI.CuzUI.CuzButton();
            this.tableLayoutControl1 = new System.Windows.Forms.TableLayoutPanel();
            this.chbStopPrint = new System.Windows.Forms.CheckBox();
            this.chbStartPrint = new System.Windows.Forms.CheckBox();
            this.chbLogout = new System.Windows.Forms.CheckBox();
            this.chbLogin = new System.Windows.Forms.CheckBox();
            this.chbInfo = new System.Windows.Forms.CheckBox();
            this.chbError = new System.Windows.Forms.CheckBox();
            this.chbWarning = new System.Windows.Forms.CheckBox();
            this.grbFilter = new System.Windows.Forms.GroupBox();
            this.dgrHistory = new DesignUI.CuzUI.CuzDataGridView();
            this.pnlDrag.SuspendLayout();
            this.tableLayoutControl1.SuspendLayout();
            this.grbFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgrHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // datTo
            // 
            this.datTo._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.datTo._BorderRadius = 10;
            this.datTo._BorderSize = 1;
            this.datTo.CalendarFont = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.datTo.CalendarTitleBackColor = System.Drawing.SystemColors.MenuHighlight;
            this.datTo.CalendarTrailingForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.datTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.datTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.datTo.Location = new System.Drawing.Point(859, 142);
            this.datTo.MinimumSize = new System.Drawing.Size(4, 35);
            this.datTo.Name = "datTo";
            this.datTo.Size = new System.Drawing.Size(150, 35);
            this.datTo.SkinColor = System.Drawing.Color.White;
            this.datTo.TabIndex = 64;
            this.datTo.TextColor = System.Drawing.SystemColors.WindowFrame;
            // 
            // datFrom
            // 
            this.datFrom._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.datFrom._BorderRadius = 10;
            this.datFrom._BorderSize = 1;
            this.datFrom.CalendarFont = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.datFrom.CalendarTitleBackColor = System.Drawing.SystemColors.MenuHighlight;
            this.datFrom.CalendarTrailingForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.datFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.datFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.datFrom.Location = new System.Drawing.Point(691, 142);
            this.datFrom.MinimumSize = new System.Drawing.Size(4, 35);
            this.datFrom.Name = "datFrom";
            this.datFrom.Size = new System.Drawing.Size(153, 35);
            this.datFrom.SkinColor = System.Drawing.Color.White;
            this.datFrom.TabIndex = 64;
            this.datFrom.TextColor = System.Drawing.SystemColors.WindowFrame;
            // 
            // lblFrom
            // 
            this.lblFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFrom.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblFrom.Location = new System.Drawing.Point(692, 117);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(46, 20);
            this.lblFrom.TabIndex = 2;
            this.lblFrom.Text = "From";
            // 
            // lblTo
            // 
            this.lblTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTo.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblTo.Location = new System.Drawing.Point(863, 117);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(27, 20);
            this.lblTo.TabIndex = 2;
            this.lblTo.Text = "To";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(20, 600);
            this.panel2.TabIndex = 62;
            // 
            // pnlDrag
            // 
            this.pnlDrag.BackColor = System.Drawing.Color.White;
            this.pnlDrag.Controls.Add(this.cuzControlBox1);
            this.pnlDrag.Controls.Add(this.lblFormName);
            this.pnlDrag.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDrag.Location = new System.Drawing.Point(20, 0);
            this.pnlDrag.Name = "pnlDrag";
            this.pnlDrag.Size = new System.Drawing.Size(1004, 35);
            this.pnlDrag.TabIndex = 63;
            // 
            // cuzControlBox1
            // 
            this.cuzControlBox1._BackColor = System.Drawing.Color.White;
            this.cuzControlBox1._CloseHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(17)))), ((int)(((byte)(35)))));
            this.cuzControlBox1._ControlBoxType = DesignUI.CuzUI.ControlBoxType.CloseBox;
            this.cuzControlBox1._IconColor = DesignUI.CuzUI.IconColor.Black;
            this.cuzControlBox1._IconSize = new System.Drawing.Size(25, 25);
            this.cuzControlBox1.BackColor = System.Drawing.Color.White;
            this.cuzControlBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.cuzControlBox1.FlatAppearance.BorderSize = 0;
            this.cuzControlBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cuzControlBox1.Image = ((System.Drawing.Image)(resources.GetObject("cuzControlBox1.Image")));
            this.cuzControlBox1.Location = new System.Drawing.Point(959, 0);
            this.cuzControlBox1.Name = "cuzControlBox1";
            this.cuzControlBox1.Size = new System.Drawing.Size(45, 35);
            this.cuzControlBox1.TabIndex = 66;
            this.cuzControlBox1.UseVisualStyleBackColor = false;
            // 
            // lblFormName
            // 
            this.lblFormName.AutoSize = true;
            this.lblFormName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormName.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblFormName.Location = new System.Drawing.Point(3, 7);
            this.lblFormName.Name = "lblFormName";
            this.lblFormName.Size = new System.Drawing.Size(121, 20);
            this.lblFormName.TabIndex = 65;
            this.lblFormName.Text = "History program";
            // 
            // cuzDragControl1
            // 
            this.cuzDragControl1.DockSides = false;
            this.cuzDragControl1.DragParent = true;
            this.cuzDragControl1.TargetControl = this.pnlDrag;
            // 
            // btnExport
            // 
            this.btnExport._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnExport._BorderRadius = 10;
            this.btnExport._BorderSize = 1;
            this.btnExport._GradientsButton = false;
            this.btnExport._Text = "Export";
            this.btnExport.BackColor = System.Drawing.Color.White;
            this.btnExport.BackgroundColor = System.Drawing.Color.White;
            this.btnExport.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnExport.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnExport.FlatAppearance.BorderSize = 0;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.btnExport.Location = new System.Drawing.Point(281, 137);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(116, 40);
            this.btnExport.TabIndex = 64;
            this.btnExport.Text = "Export";
            this.btnExport.TextColor = System.Drawing.SystemColors.WindowFrame;
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // btnRefresh
            // 
            this.btnRefresh._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnRefresh._BorderRadius = 10;
            this.btnRefresh._BorderSize = 1;
            this.btnRefresh._GradientsButton = false;
            this.btnRefresh._Text = "Refresh";
            this.btnRefresh.BackColor = System.Drawing.Color.White;
            this.btnRefresh.BackgroundColor = System.Drawing.Color.White;
            this.btnRefresh.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnRefresh.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.btnRefresh.Location = new System.Drawing.Point(159, 137);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(116, 40);
            this.btnRefresh.TabIndex = 64;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.TextColor = System.Drawing.SystemColors.WindowFrame;
            this.btnRefresh.UseVisualStyleBackColor = false;
            // 
            // btnClearLog
            // 
            this.btnClearLog._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnClearLog._BorderRadius = 10;
            this.btnClearLog._BorderSize = 1;
            this.btnClearLog._GradientsButton = false;
            this.btnClearLog._Text = "Clear log";
            this.btnClearLog.BackColor = System.Drawing.Color.White;
            this.btnClearLog.BackgroundColor = System.Drawing.Color.White;
            this.btnClearLog.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnClearLog.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnClearLog.FlatAppearance.BorderSize = 0;
            this.btnClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearLog.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.btnClearLog.Location = new System.Drawing.Point(37, 137);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(116, 40);
            this.btnClearLog.TabIndex = 64;
            this.btnClearLog.Text = "Clear log";
            this.btnClearLog.TextColor = System.Drawing.SystemColors.WindowFrame;
            this.btnClearLog.UseVisualStyleBackColor = false;
            // 
            // tableLayoutControl1
            // 
            this.tableLayoutControl1.ColumnCount = 7;
            this.tableLayoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutControl1.Controls.Add(this.chbStopPrint, 6, 0);
            this.tableLayoutControl1.Controls.Add(this.chbStartPrint, 5, 0);
            this.tableLayoutControl1.Controls.Add(this.chbLogout, 4, 0);
            this.tableLayoutControl1.Controls.Add(this.chbLogin, 3, 0);
            this.tableLayoutControl1.Controls.Add(this.chbInfo, 2, 0);
            this.tableLayoutControl1.Controls.Add(this.chbError, 0, 0);
            this.tableLayoutControl1.Controls.Add(this.chbWarning, 1, 0);
            this.tableLayoutControl1.Location = new System.Drawing.Point(6, 22);
            this.tableLayoutControl1.Name = "tableLayoutControl1";
            this.tableLayoutControl1.RowCount = 1;
            this.tableLayoutControl1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutControl1.Size = new System.Drawing.Size(880, 45);
            this.tableLayoutControl1.TabIndex = 3;
            // 
            // chbStopPrint
            // 
            this.chbStopPrint.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbStopPrint.BackColor = System.Drawing.Color.White;
            this.chbStopPrint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chbStopPrint.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.chbStopPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chbStopPrint.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.chbStopPrint.Image = ((System.Drawing.Image)(resources.GetObject("chbStopPrint.Image")));
            this.chbStopPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chbStopPrint.Location = new System.Drawing.Point(753, 3);
            this.chbStopPrint.Name = "chbStopPrint";
            this.chbStopPrint.Size = new System.Drawing.Size(124, 39);
            this.chbStopPrint.TabIndex = 0;
            this.chbStopPrint.Text = "Stop";
            this.chbStopPrint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbStopPrint.UseVisualStyleBackColor = false;
            // 
            // chbStartPrint
            // 
            this.chbStartPrint.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbStartPrint.BackColor = System.Drawing.Color.White;
            this.chbStartPrint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chbStartPrint.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.chbStartPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chbStartPrint.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.chbStartPrint.Image = ((System.Drawing.Image)(resources.GetObject("chbStartPrint.Image")));
            this.chbStartPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chbStartPrint.Location = new System.Drawing.Point(628, 3);
            this.chbStartPrint.Name = "chbStartPrint";
            this.chbStartPrint.Size = new System.Drawing.Size(119, 39);
            this.chbStartPrint.TabIndex = 0;
            this.chbStartPrint.Text = "Start";
            this.chbStartPrint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbStartPrint.UseVisualStyleBackColor = false;
            // 
            // chbLogout
            // 
            this.chbLogout.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbLogout.BackColor = System.Drawing.Color.White;
            this.chbLogout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chbLogout.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.chbLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chbLogout.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.chbLogout.Image = ((System.Drawing.Image)(resources.GetObject("chbLogout.Image")));
            this.chbLogout.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chbLogout.Location = new System.Drawing.Point(503, 3);
            this.chbLogout.Name = "chbLogout";
            this.chbLogout.Size = new System.Drawing.Size(119, 39);
            this.chbLogout.TabIndex = 0;
            this.chbLogout.Text = "Logout";
            this.chbLogout.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbLogout.UseVisualStyleBackColor = false;
            // 
            // chbLogin
            // 
            this.chbLogin.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbLogin.BackColor = System.Drawing.Color.White;
            this.chbLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chbLogin.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.chbLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chbLogin.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.chbLogin.Image = ((System.Drawing.Image)(resources.GetObject("chbLogin.Image")));
            this.chbLogin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chbLogin.Location = new System.Drawing.Point(378, 3);
            this.chbLogin.Name = "chbLogin";
            this.chbLogin.Size = new System.Drawing.Size(119, 39);
            this.chbLogin.TabIndex = 0;
            this.chbLogin.Text = "Login";
            this.chbLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbLogin.UseVisualStyleBackColor = false;
            // 
            // chbInfo
            // 
            this.chbInfo.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbInfo.BackColor = System.Drawing.Color.White;
            this.chbInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chbInfo.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.chbInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chbInfo.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.chbInfo.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_info_24px1;
            this.chbInfo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chbInfo.Location = new System.Drawing.Point(253, 3);
            this.chbInfo.Name = "chbInfo";
            this.chbInfo.Size = new System.Drawing.Size(119, 39);
            this.chbInfo.TabIndex = 0;
            this.chbInfo.Text = "Info";
            this.chbInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbInfo.UseVisualStyleBackColor = false;
            // 
            // chbError
            // 
            this.chbError.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbError.BackColor = System.Drawing.Color.White;
            this.chbError.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chbError.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.chbError.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chbError.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.chbError.Image = ((System.Drawing.Image)(resources.GetObject("chbError.Image")));
            this.chbError.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chbError.Location = new System.Drawing.Point(3, 3);
            this.chbError.Name = "chbError";
            this.chbError.Size = new System.Drawing.Size(119, 39);
            this.chbError.TabIndex = 0;
            this.chbError.Text = "Error";
            this.chbError.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbError.UseVisualStyleBackColor = false;
            // 
            // chbWarning
            // 
            this.chbWarning.Appearance = System.Windows.Forms.Appearance.Button;
            this.chbWarning.BackColor = System.Drawing.Color.White;
            this.chbWarning.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chbWarning.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.chbWarning.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chbWarning.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.chbWarning.Image = ((System.Drawing.Image)(resources.GetObject("chbWarning.Image")));
            this.chbWarning.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chbWarning.Location = new System.Drawing.Point(128, 3);
            this.chbWarning.Name = "chbWarning";
            this.chbWarning.Size = new System.Drawing.Size(119, 39);
            this.chbWarning.TabIndex = 0;
            this.chbWarning.Text = "Warning";
            this.chbWarning.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbWarning.UseVisualStyleBackColor = false;
            // 
            // grbFilter
            // 
            this.grbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grbFilter.Controls.Add(this.tableLayoutControl1);
            this.grbFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbFilter.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.grbFilter.Location = new System.Drawing.Point(35, 39);
            this.grbFilter.Name = "grbFilter";
            this.grbFilter.Size = new System.Drawing.Size(974, 77);
            this.grbFilter.TabIndex = 0;
            this.grbFilter.TabStop = false;
            this.grbFilter.Text = "Filter";
            // 
            // dgrHistory
            // 
            this.dgrHistory.AllowUserToAddRows = false;
            this.dgrHistory.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgrHistory.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgrHistory.AlterRowBackColor = System.Drawing.Color.White;
            this.dgrHistory.AlterRowForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgrHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgrHistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgrHistory.BackgroundColor = System.Drawing.Color.White;
            this.dgrHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgrHistory.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgrHistory.ColumnHeaderBackColor = System.Drawing.Color.White;
            this.dgrHistory.ColumnHeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.dgrHistory.ColumnHeaderForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgrHistory.ColumnHeaderHeight = 4;
            this.dgrHistory.ColumnHeaderPadding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.dgrHistory.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgrHistory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgrHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgrHistory.EnableHeadersVisualStyles = false;
            this.dgrHistory.GridLineColor = System.Drawing.Color.LightBlue;
            this.dgrHistory.HeaderBorder = true;
            this.dgrHistory.HeaderBorderStyle = DesignUI.CuzUI.CuzDataGridView.CustomBorderStyle.SingleHorizontal;
            this.dgrHistory.Location = new System.Drawing.Point(36, 189);
            this.dgrHistory.MultiSelect = false;
            this.dgrHistory.Name = "dgrHistory";
            this.dgrHistory.ReadOnly = true;
            this.dgrHistory.RowBackColor = System.Drawing.Color.White;
            this.dgrHistory.RowBorder = true;
            this.dgrHistory.RowBorderStyle = DesignUI.CuzUI.CuzDataGridView.CustomBorderStyle.SingleHorizontal;
            this.dgrHistory.RowFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.dgrHistory.RowForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgrHistory.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgrHistory.RowHeadersVisible = false;
            this.dgrHistory.RowHeight = 35;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgrHistory.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgrHistory.RowSelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.dgrHistory.RowSelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgrHistory.RowTemplate.Height = 35;
            this.dgrHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgrHistory.Size = new System.Drawing.Size(974, 394);
            this.dgrHistory.TabIndex = 112;
            // 
            // frmViewHistoryProgram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1024, 600);
            this.Controls.Add(this.dgrHistory);
            this.Controls.Add(this.datTo);
            this.Controls.Add(this.datFrom);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.lblFrom);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.lblTo);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.grbFilter);
            this.Controls.Add(this.pnlDrag);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1024, 600);
            this.Name = "frmViewHistoryProgram";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Program history";
            this.pnlDrag.ResumeLayout(false);
            this.pnlDrag.PerformLayout();
            this.tableLayoutControl1.ResumeLayout(false);
            this.grbFilter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgrHistory)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnlDrag;
        private DesignUI.CuzUI.CuzDragControl cuzDragControl1;
        private DesignUI.CuzUI.CuzDateTimePicker datTo;
        private DesignUI.CuzUI.CuzDateTimePicker datFrom;
        private DesignUI.CuzUI.CuzButton btnExport;
        private DesignUI.CuzUI.CuzButton btnRefresh;
        private DesignUI.CuzUI.CuzButton btnClearLog;
        private System.Windows.Forms.Label lblFormName;
        private DesignUI.CuzUI.CuzControlBox cuzControlBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutControl1;
        private System.Windows.Forms.CheckBox chbStopPrint;
        private System.Windows.Forms.CheckBox chbStartPrint;
        private System.Windows.Forms.CheckBox chbLogout;
        private System.Windows.Forms.CheckBox chbLogin;
        private System.Windows.Forms.CheckBox chbInfo;
        private System.Windows.Forms.CheckBox chbError;
        private System.Windows.Forms.CheckBox chbWarning;
        private System.Windows.Forms.GroupBox grbFilter;
        private DesignUI.CuzUI.CuzDataGridView dgrHistory;
    }
}