namespace BarcodeVerificationSystem.View.NutrifoodUI
{
    partial class FrmPreviewDatabaseNutri
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPreviewDatabaseNutri));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlDrag = new System.Windows.Forms.Panel();
            this.cuzControlBox1 = new DesignUI.CuzUI.CuzControlBox();
            this.lblFormName = new System.Windows.Forms.Label();
            this.grbDatabase = new System.Windows.Forms.Panel();
            this.dgvDatabase = new DesignUI.CuzUI.CuzDataGridView();
            this.grb1 = new DesignUI.CuzUI.CuzPanel();
            this.lblTotalNumberOfCodesInDatabase = new System.Windows.Forms.Label();
            this.lblTotalDatabase = new System.Windows.Forms.Label();
            this.picLoadDatabase = new System.Windows.Forms.PictureBox();
            this.btnSearch = new DesignUI.CuzUI.CuzButton();
            this.btnRefeshDatabase = new DesignUI.CuzUI.CuzButton();
            this.txtSearchDatabase = new DesignUI.CuzUI.CuzTextBox();
            this.pnlPaging = new System.Windows.Forms.Panel();
            this.btnFirst = new System.Windows.Forms.Button();
            this.lblPagePerTotals = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            this.Number1 = new System.Windows.Forms.Button();
            this.Number2 = new System.Windows.Forms.Button();
            this.Number3 = new System.Windows.Forms.Button();
            this.Number4 = new System.Windows.Forms.Button();
            this.Number5 = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnLast = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblGo = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.lblGoToPage = new System.Windows.Forms.Label();
            this.cuzDragControl1 = new DesignUI.CuzUI.CuzDragControl();
            this.pnlDrag.SuspendLayout();
            this.grbDatabase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabase)).BeginInit();
            this.grb1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLoadDatabase)).BeginInit();
            this.pnlPaging.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(109)))), ((int)(((byte)(70)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(20, 600);
            this.panel2.TabIndex = 64;
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
            this.pnlDrag.TabIndex = 65;
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
            this.cuzControlBox1.TabIndex = 4;
            this.cuzControlBox1.UseVisualStyleBackColor = false;
            // 
            // lblFormName
            // 
            this.lblFormName.AutoSize = true;
            this.lblFormName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormName.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblFormName.Location = new System.Drawing.Point(3, 8);
            this.lblFormName.Name = "lblFormName";
            this.lblFormName.Size = new System.Drawing.Size(191, 20);
            this.lblFormName.TabIndex = 3;
            this.lblFormName.Text = "DATABASE PREVIEW";
            // 
            // grbDatabase
            // 
            this.grbDatabase.Controls.Add(this.dgvDatabase);
            this.grbDatabase.Controls.Add(this.grb1);
            this.grbDatabase.Controls.Add(this.btnSearch);
            this.grbDatabase.Controls.Add(this.btnRefeshDatabase);
            this.grbDatabase.Controls.Add(this.txtSearchDatabase);
            this.grbDatabase.Controls.Add(this.pnlPaging);
            this.grbDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbDatabase.Location = new System.Drawing.Point(20, 35);
            this.grbDatabase.Name = "grbDatabase";
            this.grbDatabase.Size = new System.Drawing.Size(1004, 565);
            this.grbDatabase.TabIndex = 67;
            // 
            // dgvDatabase
            // 
            this.dgvDatabase.AllowUserToAddRows = false;
            this.dgvDatabase.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvDatabase.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDatabase.AlterRowBackColor = System.Drawing.Color.White;
            this.dgvDatabase.AlterRowForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDatabase.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDatabase.BackgroundColor = System.Drawing.Color.White;
            this.dgvDatabase.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDatabase.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgvDatabase.ColumnHeaderBackColor = System.Drawing.Color.White;
            this.dgvDatabase.ColumnHeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.dgvDatabase.ColumnHeaderForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvDatabase.ColumnHeaderHeight = 4;
            this.dgvDatabase.ColumnHeaderPadding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.dgvDatabase.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDatabase.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDatabase.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDatabase.EnableHeadersVisualStyles = false;
            this.dgvDatabase.GridLineColor = System.Drawing.Color.LightBlue;
            this.dgvDatabase.HeaderBorder = true;
            this.dgvDatabase.HeaderBorderStyle = DesignUI.CuzUI.CuzDataGridView.CustomBorderStyle.SingleHorizontal;
            this.dgvDatabase.Location = new System.Drawing.Point(18, 115);
            this.dgvDatabase.MultiSelect = false;
            this.dgvDatabase.Name = "dgvDatabase";
            this.dgvDatabase.ReadOnly = true;
            this.dgvDatabase.RowBackColor = System.Drawing.Color.White;
            this.dgvDatabase.RowBorder = true;
            this.dgvDatabase.RowBorderStyle = DesignUI.CuzUI.CuzDataGridView.CustomBorderStyle.SingleHorizontal;
            this.dgvDatabase.RowFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.dgvDatabase.RowForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvDatabase.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvDatabase.RowHeadersVisible = false;
            this.dgvDatabase.RowHeight = 35;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvDatabase.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvDatabase.RowSelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.dgvDatabase.RowSelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvDatabase.RowTemplate.Height = 35;
            this.dgvDatabase.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDatabase.Size = new System.Drawing.Size(970, 401);
            this.dgvDatabase.TabIndex = 120;
            // 
            // grb1
            // 
            this.grb1._BorderColor = System.Drawing.Color.Silver;
            this.grb1._BorderRadius = 10;
            this.grb1._BorderSize = 1;
            this.grb1._Corner = 0F;
            this.grb1._FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.grb1._FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.grb1._GradientPanel = false;
            this.grb1.Controls.Add(this.lblTotalNumberOfCodesInDatabase);
            this.grb1.Controls.Add(this.lblTotalDatabase);
            this.grb1.Controls.Add(this.picLoadDatabase);
            this.grb1.Location = new System.Drawing.Point(18, 11);
            this.grb1.Name = "grb1";
            this.grb1.Size = new System.Drawing.Size(412, 90);
            this.grb1.TabIndex = 119;
            // 
            // lblTotalNumberOfCodesInDatabase
            // 
            this.lblTotalNumberOfCodesInDatabase.AutoSize = true;
            this.lblTotalNumberOfCodesInDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalNumberOfCodesInDatabase.Location = new System.Drawing.Point(13, 15);
            this.lblTotalNumberOfCodesInDatabase.Name = "lblTotalNumberOfCodesInDatabase";
            this.lblTotalNumberOfCodesInDatabase.Size = new System.Drawing.Size(281, 20);
            this.lblTotalNumberOfCodesInDatabase.TabIndex = 118;
            this.lblTotalNumberOfCodesInDatabase.Text = "Total number of codes in the database";
            // 
            // lblTotalDatabase
            // 
            this.lblTotalDatabase.AutoSize = true;
            this.lblTotalDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalDatabase.Location = new System.Drawing.Point(25, 43);
            this.lblTotalDatabase.Name = "lblTotalDatabase";
            this.lblTotalDatabase.Size = new System.Drawing.Size(65, 37);
            this.lblTotalDatabase.TabIndex = 10;
            this.lblTotalDatabase.Text = "0/0";
            // 
            // picLoadDatabase
            // 
            this.picLoadDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picLoadDatabase.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_loading_2681;
            this.picLoadDatabase.Location = new System.Drawing.Point(361, 54);
            this.picLoadDatabase.Name = "picLoadDatabase";
            this.picLoadDatabase.Size = new System.Drawing.Size(37, 26);
            this.picLoadDatabase.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLoadDatabase.TabIndex = 15;
            this.picLoadDatabase.TabStop = false;
            this.picLoadDatabase.Visible = false;
            // 
            // btnSearch
            // 
            this.btnSearch._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnSearch._BorderRadius = 10;
            this.btnSearch._BorderSize = 1;
            this.btnSearch._GradientsButton = false;
            this.btnSearch._Text = "Search";
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnSearch.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnSearch.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnSearch.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnSearch.Image")));
            this.btnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearch.Location = new System.Drawing.Point(839, 12);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnSearch.Size = new System.Drawing.Size(149, 48);
            this.btnSearch.TabIndex = 96;
            this.btnSearch.Text = "Search";
            this.btnSearch.TextColor = System.Drawing.Color.White;
            this.btnSearch.UseVisualStyleBackColor = false;
            // 
            // btnRefeshDatabase
            // 
            this.btnRefeshDatabase._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnRefeshDatabase._BorderRadius = 10;
            this.btnRefeshDatabase._BorderSize = 1;
            this.btnRefeshDatabase._GradientsButton = false;
            this.btnRefeshDatabase._Text = "Refresh";
            this.btnRefeshDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefeshDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnRefeshDatabase.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnRefeshDatabase.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnRefeshDatabase.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnRefeshDatabase.FlatAppearance.BorderSize = 0;
            this.btnRefeshDatabase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefeshDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefeshDatabase.ForeColor = System.Drawing.Color.White;
            this.btnRefeshDatabase.Image = ((System.Drawing.Image)(resources.GetObject("btnRefeshDatabase.Image")));
            this.btnRefeshDatabase.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRefeshDatabase.Location = new System.Drawing.Point(682, 12);
            this.btnRefeshDatabase.Name = "btnRefeshDatabase";
            this.btnRefeshDatabase.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnRefeshDatabase.Size = new System.Drawing.Size(149, 48);
            this.btnRefeshDatabase.TabIndex = 95;
            this.btnRefeshDatabase.Text = "Refresh";
            this.btnRefeshDatabase.TextColor = System.Drawing.Color.White;
            this.btnRefeshDatabase.UseVisualStyleBackColor = false;
            // 
            // txtSearchDatabase
            // 
            this.txtSearchDatabase._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtSearchDatabase._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtSearchDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearchDatabase.BackColor = System.Drawing.SystemColors.Window;
            this.txtSearchDatabase.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtSearchDatabase.BorderFocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.txtSearchDatabase.BorderRadius = 6;
            this.txtSearchDatabase.BorderSize = 1;
            this.txtSearchDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearchDatabase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtSearchDatabase.Location = new System.Drawing.Point(448, 66);
            this.txtSearchDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.txtSearchDatabase.Multiline = false;
            this.txtSearchDatabase.Name = "txtSearchDatabase";
            this.txtSearchDatabase.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.txtSearchDatabase.PasswordChar = false;
            this.txtSearchDatabase.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtSearchDatabase.PlaceholderText = "";
            this.txtSearchDatabase.ReadOnly = false;
            this.txtSearchDatabase.Size = new System.Drawing.Size(540, 35);
            this.txtSearchDatabase.TabIndex = 19;
            this.txtSearchDatabase.UnderlinedStyle = false;
            // 
            // pnlPaging
            // 
            this.pnlPaging.Controls.Add(this.btnFirst);
            this.pnlPaging.Controls.Add(this.lblPagePerTotals);
            this.pnlPaging.Controls.Add(this.btnBack);
            this.pnlPaging.Controls.Add(this.Number1);
            this.pnlPaging.Controls.Add(this.Number2);
            this.pnlPaging.Controls.Add(this.Number3);
            this.pnlPaging.Controls.Add(this.Number4);
            this.pnlPaging.Controls.Add(this.Number5);
            this.pnlPaging.Controls.Add(this.btnNext);
            this.pnlPaging.Controls.Add(this.btnLast);
            this.pnlPaging.Controls.Add(this.panel3);
            this.pnlPaging.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlPaging.Location = new System.Drawing.Point(0, 525);
            this.pnlPaging.Name = "pnlPaging";
            this.pnlPaging.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
            this.pnlPaging.Size = new System.Drawing.Size(1004, 40);
            this.pnlPaging.TabIndex = 117;
            // 
            // btnFirst
            // 
            this.btnFirst.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnFirst.FlatAppearance.BorderSize = 0;
            this.btnFirst.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFirst.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFirst.Image = ((System.Drawing.Image)(resources.GetObject("btnFirst.Image")));
            this.btnFirst.Location = new System.Drawing.Point(323, 0);
            this.btnFirst.Name = "btnFirst";
            this.btnFirst.Size = new System.Drawing.Size(50, 40);
            this.btnFirst.TabIndex = 21;
            this.btnFirst.UseVisualStyleBackColor = true;
            // 
            // lblPagePerTotals
            // 
            this.lblPagePerTotals.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblPagePerTotals.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPagePerTotals.Location = new System.Drawing.Point(15, 0);
            this.lblPagePerTotals.Name = "lblPagePerTotals";
            this.lblPagePerTotals.Size = new System.Drawing.Size(280, 40);
            this.lblPagePerTotals.TabIndex = 18;
            this.lblPagePerTotals.Text = "Page number of totals page.";
            this.lblPagePerTotals.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnBack
            // 
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBack.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_sort_left_24px2;
            this.btnBack.Location = new System.Drawing.Point(373, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(50, 40);
            this.btnBack.TabIndex = 10;
            this.btnBack.UseVisualStyleBackColor = true;
            // 
            // Number1
            // 
            this.Number1.Dock = System.Windows.Forms.DockStyle.Right;
            this.Number1.FlatAppearance.BorderSize = 0;
            this.Number1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Number1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Number1.Location = new System.Drawing.Point(423, 0);
            this.Number1.Name = "Number1";
            this.Number1.Size = new System.Drawing.Size(50, 40);
            this.Number1.TabIndex = 11;
            this.Number1.Text = "1";
            this.Number1.UseVisualStyleBackColor = true;
            // 
            // Number2
            // 
            this.Number2.Dock = System.Windows.Forms.DockStyle.Right;
            this.Number2.FlatAppearance.BorderSize = 0;
            this.Number2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Number2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Number2.Location = new System.Drawing.Point(473, 0);
            this.Number2.Name = "Number2";
            this.Number2.Size = new System.Drawing.Size(50, 40);
            this.Number2.TabIndex = 12;
            this.Number2.Text = "2";
            this.Number2.UseVisualStyleBackColor = true;
            // 
            // Number3
            // 
            this.Number3.Dock = System.Windows.Forms.DockStyle.Right;
            this.Number3.FlatAppearance.BorderSize = 0;
            this.Number3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Number3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Number3.Location = new System.Drawing.Point(523, 0);
            this.Number3.Name = "Number3";
            this.Number3.Size = new System.Drawing.Size(50, 40);
            this.Number3.TabIndex = 13;
            this.Number3.Text = "3";
            this.Number3.UseVisualStyleBackColor = true;
            // 
            // Number4
            // 
            this.Number4.Dock = System.Windows.Forms.DockStyle.Right;
            this.Number4.FlatAppearance.BorderSize = 0;
            this.Number4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Number4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Number4.Location = new System.Drawing.Point(573, 0);
            this.Number4.Name = "Number4";
            this.Number4.Size = new System.Drawing.Size(50, 40);
            this.Number4.TabIndex = 14;
            this.Number4.Text = "4";
            this.Number4.UseVisualStyleBackColor = true;
            // 
            // Number5
            // 
            this.Number5.Dock = System.Windows.Forms.DockStyle.Right;
            this.Number5.FlatAppearance.BorderSize = 0;
            this.Number5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Number5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Number5.Location = new System.Drawing.Point(623, 0);
            this.Number5.Name = "Number5";
            this.Number5.Size = new System.Drawing.Size(50, 40);
            this.Number5.TabIndex = 15;
            this.Number5.Text = "5";
            this.Number5.UseVisualStyleBackColor = true;
            // 
            // btnNext
            // 
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNext.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_sort_right_24px1;
            this.btnNext.Location = new System.Drawing.Point(673, 0);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(50, 40);
            this.btnNext.TabIndex = 16;
            this.btnNext.UseVisualStyleBackColor = true;
            // 
            // btnLast
            // 
            this.btnLast.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnLast.FlatAppearance.BorderSize = 0;
            this.btnLast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLast.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLast.Image = ((System.Drawing.Image)(resources.GetObject("btnLast.Image")));
            this.btnLast.Location = new System.Drawing.Point(723, 0);
            this.btnLast.Name = "btnLast";
            this.btnLast.Size = new System.Drawing.Size(50, 40);
            this.btnLast.TabIndex = 17;
            this.btnLast.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lblGo);
            this.panel3.Controls.Add(this.comboBox1);
            this.panel3.Controls.Add(this.lblGoToPage);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(773, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(216, 40);
            this.panel3.TabIndex = 20;
            // 
            // lblGo
            // 
            this.lblGo.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGo.Location = new System.Drawing.Point(167, 0);
            this.lblGo.Name = "lblGo";
            this.lblGo.Size = new System.Drawing.Size(49, 40);
            this.lblGo.TabIndex = 21;
            this.lblGo.Text = "Đến";
            this.lblGo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboBox1
            // 
            this.comboBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(102, 7);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(59, 28);
            this.comboBox1.TabIndex = 19;
            // 
            // lblGoToPage
            // 
            this.lblGoToPage.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblGoToPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGoToPage.Location = new System.Drawing.Point(0, 0);
            this.lblGoToPage.Name = "lblGoToPage";
            this.lblGoToPage.Size = new System.Drawing.Size(96, 40);
            this.lblGoToPage.TabIndex = 20;
            this.lblGoToPage.Text = "Đi đến trang";
            this.lblGoToPage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cuzDragControl1
            // 
            this.cuzDragControl1.DockSides = false;
            this.cuzDragControl1.DragParent = true;
            this.cuzDragControl1.TargetControl = this.pnlDrag;
            // 
            // FrmPreviewDatabaseNutri
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1024, 600);
            this.Controls.Add(this.grbDatabase);
            this.Controls.Add(this.pnlDrag);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmPreviewDatabaseNutri";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmPreviewDatabase";
            this.pnlDrag.ResumeLayout(false);
            this.pnlDrag.PerformLayout();
            this.grbDatabase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabase)).EndInit();
            this.grb1.ResumeLayout(false);
            this.grb1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLoadDatabase)).EndInit();
            this.pnlPaging.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnlDrag;
        private System.Windows.Forms.Panel grbDatabase;
        private System.Windows.Forms.PictureBox picLoadDatabase;
        private System.Windows.Forms.Label lblTotalDatabase;
        private DesignUI.CuzUI.CuzDragControl cuzDragControl1;
        private DesignUI.CuzUI.CuzTextBox txtSearchDatabase;
        private DesignUI.CuzUI.CuzButton btnRefeshDatabase;
        private System.Windows.Forms.Label lblFormName;
        private DesignUI.CuzUI.CuzControlBox cuzControlBox1;
        private DesignUI.CuzUI.CuzButton btnSearch;
        private System.Windows.Forms.Panel pnlPaging;
        private System.Windows.Forms.Label lblTotalNumberOfCodesInDatabase;
        private DesignUI.CuzUI.CuzPanel grb1;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button Number1;
        private System.Windows.Forms.Button Number2;
        private System.Windows.Forms.Button Number3;
        private System.Windows.Forms.Button Number4;
        private System.Windows.Forms.Button Number5;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnLast;
        private System.Windows.Forms.Label lblPagePerTotals;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblGo;
        private System.Windows.Forms.Label lblGoToPage;
        private System.Windows.Forms.Button btnFirst;
        private DesignUI.CuzUI.CuzDataGridView dgvDatabase;
    }
}