namespace BarcodeVerificationSystem.View
{
    partial class FrmPODFormat
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPODFormat));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlDrag = new System.Windows.Forms.Panel();
            this.cuzControlBox1 = new DesignUI.CuzUI.CuzControlBox();
            this.lblFormName = new System.Windows.Forms.Label();
            this.cuzDragControl1 = new DesignUI.CuzUI.CuzDragControl();
            this.cuzDropShadow1 = new DesignUI.CuzUI.CuzDropShadow();
            this.pnlDisplay = new System.Windows.Forms.Panel();
            this.dgvSampleData = new DesignUI.CuzUI.CuzDataGridView();
            this.txtSample = new System.Windows.Forms.TextBox();
            this.lblSample = new System.Windows.Forms.Label();
            this.lblPreview = new System.Windows.Forms.Label();
            this.lblText = new System.Windows.Forms.Label();
            this.txtText = new System.Windows.Forms.TextBox();
            this.pnlPOD = new System.Windows.Forms.Panel();
            this.tblPanelPOD = new System.Windows.Forms.TableLayoutPanel();
            this.pnlPODList = new System.Windows.Forms.Panel();
            this.listBoxPODLeft = new System.Windows.Forms.ListBox();
            this.lblPODlist = new System.Windows.Forms.Label();
            this.tblPanelButton = new System.Windows.Forms.TableLayoutPanel();
            this.btnAdd = new DesignUI.CuzUI.CuzButton();
            this.btnRemove = new DesignUI.CuzUI.CuzButton();
            this.btnUp = new DesignUI.CuzUI.CuzButton();
            this.btnDown = new DesignUI.CuzUI.CuzButton();
            this.btnClear = new DesignUI.CuzUI.CuzButton();
            this.pnlPODFormat = new System.Windows.Forms.Panel();
            this.listBoxPODRight = new System.Windows.Forms.ListBox();
            this.lblPODformat = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnPreviewDatabase = new DesignUI.CuzUI.CuzButton();
            this.btnPreviewComparison = new DesignUI.CuzUI.CuzButton();
            this.btnSave = new DesignUI.CuzUI.CuzButton();
            this.tblPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.numberOfCodes = new System.Windows.Forms.Label();
            this.pnlDrag.SuspendLayout();
            this.pnlDisplay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSampleData)).BeginInit();
            this.pnlPOD.SuspendLayout();
            this.tblPanelPOD.SuspendLayout();
            this.pnlPODList.SuspendLayout();
            this.tblPanelButton.SuspendLayout();
            this.pnlPODFormat.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tblPanelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(20, 600);
            this.panel2.TabIndex = 60;
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
            this.pnlDrag.TabIndex = 61;
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
            this.cuzControlBox1.TabIndex = 3;
            this.cuzControlBox1.UseVisualStyleBackColor = false;
            // 
            // lblFormName
            // 
            this.lblFormName.AutoSize = true;
            this.lblFormName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormName.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblFormName.Location = new System.Drawing.Point(3, 6);
            this.lblFormName.Name = "lblFormName";
            this.lblFormName.Size = new System.Drawing.Size(158, 20);
            this.lblFormName.TabIndex = 2;
            this.lblFormName.Text = "Select a field to verify";
            // 
            // cuzDragControl1
            // 
            this.cuzDragControl1.DockSides = false;
            this.cuzDragControl1.DragParent = true;
            this.cuzDragControl1.TargetControl = this.pnlDrag;
            // 
            // cuzDropShadow1
            // 
            this.cuzDropShadow1.TargetControl = this;
            // 
            // pnlDisplay
            // 
            this.pnlDisplay.Controls.Add(this.numberOfCodes);
            this.pnlDisplay.Controls.Add(this.dgvSampleData);
            this.pnlDisplay.Controls.Add(this.txtSample);
            this.pnlDisplay.Controls.Add(this.lblSample);
            this.pnlDisplay.Controls.Add(this.lblPreview);
            this.pnlDisplay.Controls.Add(this.lblText);
            this.pnlDisplay.Controls.Add(this.txtText);
            this.pnlDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDisplay.Location = new System.Drawing.Point(3, 251);
            this.pnlDisplay.Name = "pnlDisplay";
            this.pnlDisplay.Size = new System.Drawing.Size(998, 311);
            this.pnlDisplay.TabIndex = 3;
            // 
            // dgvSampleData
            // 
            this.dgvSampleData.AllowUserToAddRows = false;
            this.dgvSampleData.AllowUserToResizeRows = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvSampleData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvSampleData.AlterRowBackColor = System.Drawing.Color.White;
            this.dgvSampleData.AlterRowForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvSampleData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSampleData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSampleData.BackgroundColor = System.Drawing.Color.White;
            this.dgvSampleData.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvSampleData.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgvSampleData.ColumnHeaderBackColor = System.Drawing.Color.White;
            this.dgvSampleData.ColumnHeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.dgvSampleData.ColumnHeaderForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvSampleData.ColumnHeaderHeight = 4;
            this.dgvSampleData.ColumnHeaderPadding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.dgvSampleData.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle5.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSampleData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvSampleData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSampleData.EnableHeadersVisualStyles = false;
            this.dgvSampleData.GridLineColor = System.Drawing.Color.LightBlue;
            this.dgvSampleData.HeaderBorder = true;
            this.dgvSampleData.HeaderBorderStyle = DesignUI.CuzUI.CuzDataGridView.CustomBorderStyle.SingleHorizontal;
            this.dgvSampleData.Location = new System.Drawing.Point(13, 117);
            this.dgvSampleData.MultiSelect = false;
            this.dgvSampleData.Name = "dgvSampleData";
            this.dgvSampleData.ReadOnly = true;
            this.dgvSampleData.RowBackColor = System.Drawing.Color.White;
            this.dgvSampleData.RowBorder = true;
            this.dgvSampleData.RowBorderStyle = DesignUI.CuzUI.CuzDataGridView.CustomBorderStyle.SingleHorizontal;
            this.dgvSampleData.RowFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.dgvSampleData.RowForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvSampleData.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvSampleData.RowHeadersVisible = false;
            this.dgvSampleData.RowHeight = 35;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvSampleData.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvSampleData.RowSelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(232)))), ((int)(((byte)(255)))));
            this.dgvSampleData.RowSelectionForeColor = System.Drawing.SystemColors.WindowFrame;
            this.dgvSampleData.RowTemplate.Height = 35;
            this.dgvSampleData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSampleData.Size = new System.Drawing.Size(976, 191);
            this.dgvSampleData.TabIndex = 112;
            // 
            // txtSample
            // 
            this.txtSample.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSample.BackColor = System.Drawing.Color.White;
            this.txtSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSample.Location = new System.Drawing.Point(511, 30);
            this.txtSample.Multiline = true;
            this.txtSample.Name = "txtSample";
            this.txtSample.ReadOnly = true;
            this.txtSample.Size = new System.Drawing.Size(478, 48);
            this.txtSample.TabIndex = 5;
            // 
            // lblSample
            // 
            this.lblSample.AutoSize = true;
            this.lblSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSample.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblSample.Location = new System.Drawing.Point(513, 5);
            this.lblSample.Name = "lblSample";
            this.lblSample.Size = new System.Drawing.Size(69, 20);
            this.lblSample.TabIndex = 2;
            this.lblSample.Text = "Sample";
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPreview.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblPreview.Location = new System.Drawing.Point(9, 87);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(70, 20);
            this.lblPreview.TabIndex = 3;
            this.lblPreview.Text = "Preview";
            // 
            // lblText
            // 
            this.lblText.AutoSize = true;
            this.lblText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblText.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblText.Location = new System.Drawing.Point(15, 5);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(43, 20);
            this.lblText.TabIndex = 3;
            this.lblText.Text = "Text";
            // 
            // txtText
            // 
            this.txtText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtText.Location = new System.Drawing.Point(13, 30);
            this.txtText.Multiline = true;
            this.txtText.Name = "txtText";
            this.txtText.Size = new System.Drawing.Size(478, 48);
            this.txtText.TabIndex = 4;
            // 
            // pnlPOD
            // 
            this.pnlPOD.Controls.Add(this.tblPanelPOD);
            this.pnlPOD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPOD.Location = new System.Drawing.Point(3, 3);
            this.pnlPOD.Name = "pnlPOD";
            this.pnlPOD.Size = new System.Drawing.Size(998, 242);
            this.pnlPOD.TabIndex = 0;
            // 
            // tblPanelPOD
            // 
            this.tblPanelPOD.BackColor = System.Drawing.Color.White;
            this.tblPanelPOD.ColumnCount = 4;
            this.tblPanelPOD.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblPanelPOD.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tblPanelPOD.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblPanelPOD.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tblPanelPOD.Controls.Add(this.pnlPODList, 0, 0);
            this.tblPanelPOD.Controls.Add(this.tblPanelButton, 1, 0);
            this.tblPanelPOD.Controls.Add(this.pnlPODFormat, 2, 0);
            this.tblPanelPOD.Controls.Add(this.panel1, 3, 0);
            this.tblPanelPOD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblPanelPOD.Location = new System.Drawing.Point(0, 0);
            this.tblPanelPOD.Name = "tblPanelPOD";
            this.tblPanelPOD.RowCount = 1;
            this.tblPanelPOD.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblPanelPOD.Size = new System.Drawing.Size(998, 242);
            this.tblPanelPOD.TabIndex = 0;
            // 
            // pnlPODList
            // 
            this.pnlPODList.BackColor = System.Drawing.Color.White;
            this.pnlPODList.Controls.Add(this.listBoxPODLeft);
            this.pnlPODList.Controls.Add(this.lblPODlist);
            this.pnlPODList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPODList.Location = new System.Drawing.Point(3, 3);
            this.pnlPODList.Name = "pnlPODList";
            this.pnlPODList.Size = new System.Drawing.Size(353, 236);
            this.pnlPODList.TabIndex = 7;
            // 
            // listBoxPODLeft
            // 
            this.listBoxPODLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxPODLeft.BackColor = System.Drawing.Color.WhiteSmoke;
            this.listBoxPODLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxPODLeft.FormattingEnabled = true;
            this.listBoxPODLeft.ItemHeight = 20;
            this.listBoxPODLeft.Location = new System.Drawing.Point(10, 38);
            this.listBoxPODLeft.Name = "listBoxPODLeft";
            this.listBoxPODLeft.Size = new System.Drawing.Size(328, 184);
            this.listBoxPODLeft.TabIndex = 0;
            // 
            // lblPODlist
            // 
            this.lblPODlist.AutoSize = true;
            this.lblPODlist.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPODlist.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblPODlist.Location = new System.Drawing.Point(6, 10);
            this.lblPODlist.Name = "lblPODlist";
            this.lblPODlist.Size = new System.Drawing.Size(76, 20);
            this.lblPODlist.TabIndex = 0;
            this.lblPODlist.Text = "Field list";
            // 
            // tblPanelButton
            // 
            this.tblPanelButton.ColumnCount = 1;
            this.tblPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblPanelButton.Controls.Add(this.btnAdd, 0, 0);
            this.tblPanelButton.Controls.Add(this.btnRemove, 0, 1);
            this.tblPanelButton.Controls.Add(this.btnUp, 0, 2);
            this.tblPanelButton.Controls.Add(this.btnDown, 0, 3);
            this.tblPanelButton.Controls.Add(this.btnClear, 0, 4);
            this.tblPanelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblPanelButton.Location = new System.Drawing.Point(362, 3);
            this.tblPanelButton.Name = "tblPanelButton";
            this.tblPanelButton.RowCount = 5;
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblPanelButton.Size = new System.Drawing.Size(74, 236);
            this.tblPanelButton.TabIndex = 5;
            // 
            // btnAdd
            // 
            this.btnAdd._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnAdd._BorderRadius = 10;
            this.btnAdd._BorderSize = 1;
            this.btnAdd._GradientsButton = false;
            this.btnAdd._Text = "";
            this.btnAdd.BackColor = System.Drawing.Color.White;
            this.btnAdd.BackgroundColor = System.Drawing.Color.White;
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAdd.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnAdd.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnAdd.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAdd.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_sort_right_24px_1;
            this.btnAdd.Location = new System.Drawing.Point(3, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(68, 41);
            this.btnAdd.TabIndex = 7;
            this.btnAdd.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnAdd.UseVisualStyleBackColor = false;
            // 
            // btnRemove
            // 
            this.btnRemove._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnRemove._BorderRadius = 10;
            this.btnRemove._BorderSize = 1;
            this.btnRemove._GradientsButton = false;
            this.btnRemove._Text = "";
            this.btnRemove.BackColor = System.Drawing.Color.White;
            this.btnRemove.BackgroundColor = System.Drawing.Color.White;
            this.btnRemove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRemove.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnRemove.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnRemove.FlatAppearance.BorderSize = 0;
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnRemove.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRemove.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_sort_left_24px_1;
            this.btnRemove.Location = new System.Drawing.Point(3, 50);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(68, 41);
            this.btnRemove.TabIndex = 7;
            this.btnRemove.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnRemove.UseVisualStyleBackColor = false;
            // 
            // btnUp
            // 
            this.btnUp._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnUp._BorderRadius = 10;
            this.btnUp._BorderSize = 1;
            this.btnUp._GradientsButton = false;
            this.btnUp._Text = "";
            this.btnUp.BackColor = System.Drawing.Color.White;
            this.btnUp.BackgroundColor = System.Drawing.Color.White;
            this.btnUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnUp.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnUp.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnUp.FlatAppearance.BorderSize = 0;
            this.btnUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUp.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnUp.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnUp.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_Sort_Up_24px_1;
            this.btnUp.Location = new System.Drawing.Point(3, 97);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(68, 41);
            this.btnUp.TabIndex = 7;
            this.btnUp.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnUp.UseVisualStyleBackColor = false;
            // 
            // btnDown
            // 
            this.btnDown._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnDown._BorderRadius = 10;
            this.btnDown._BorderSize = 1;
            this.btnDown._GradientsButton = false;
            this.btnDown._Text = "";
            this.btnDown.BackColor = System.Drawing.Color.White;
            this.btnDown.BackgroundColor = System.Drawing.Color.White;
            this.btnDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDown.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnDown.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnDown.FlatAppearance.BorderSize = 0;
            this.btnDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDown.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnDown.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDown.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_Sort_Down_24px_1;
            this.btnDown.Location = new System.Drawing.Point(3, 144);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(68, 41);
            this.btnDown.TabIndex = 7;
            this.btnDown.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnDown.UseVisualStyleBackColor = false;
            // 
            // btnClear
            // 
            this.btnClear._BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnClear._BorderRadius = 10;
            this.btnClear._BorderSize = 1;
            this.btnClear._GradientsButton = false;
            this.btnClear._Text = "";
            this.btnClear.BackColor = System.Drawing.Color.White;
            this.btnClear.BackgroundColor = System.Drawing.Color.White;
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnClear.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnClear.FlatAppearance.BorderSize = 0;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnClear.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnClear.Image = global::BarcodeVerificationSystem.Properties.Resources.icons8_delete_file_24px;
            this.btnClear.Location = new System.Drawing.Point(3, 191);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(68, 42);
            this.btnClear.TabIndex = 7;
            this.btnClear.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnClear.UseVisualStyleBackColor = false;
            // 
            // pnlPODFormat
            // 
            this.pnlPODFormat.BackColor = System.Drawing.Color.White;
            this.pnlPODFormat.Controls.Add(this.listBoxPODRight);
            this.pnlPODFormat.Controls.Add(this.lblPODformat);
            this.pnlPODFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPODFormat.Location = new System.Drawing.Point(442, 3);
            this.pnlPODFormat.Name = "pnlPODFormat";
            this.pnlPODFormat.Size = new System.Drawing.Size(353, 236);
            this.pnlPODFormat.TabIndex = 6;
            // 
            // listBoxPODRight
            // 
            this.listBoxPODRight.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxPODRight.BackColor = System.Drawing.Color.WhiteSmoke;
            this.listBoxPODRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxPODRight.FormattingEnabled = true;
            this.listBoxPODRight.ItemHeight = 20;
            this.listBoxPODRight.Location = new System.Drawing.Point(11, 38);
            this.listBoxPODRight.Name = "listBoxPODRight";
            this.listBoxPODRight.Size = new System.Drawing.Size(331, 184);
            this.listBoxPODRight.TabIndex = 1;
            // 
            // lblPODformat
            // 
            this.lblPODformat.AutoSize = true;
            this.lblPODformat.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPODformat.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblPODformat.Location = new System.Drawing.Point(12, 10);
            this.lblPODformat.Name = "lblPODformat";
            this.lblPODformat.Size = new System.Drawing.Size(209, 20);
            this.lblPODformat.TabIndex = 0;
            this.lblPODformat.Text = "Comparison fileds format";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnPreviewDatabase);
            this.panel1.Controls.Add(this.btnPreviewComparison);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(801, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(194, 236);
            this.panel1.TabIndex = 8;
            // 
            // btnPreviewDatabase
            // 
            this.btnPreviewDatabase._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnPreviewDatabase._BorderRadius = 10;
            this.btnPreviewDatabase._BorderSize = 0;
            this.btnPreviewDatabase._GradientsButton = false;
            this.btnPreviewDatabase._Text = "Preview database";
            this.btnPreviewDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnPreviewDatabase.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnPreviewDatabase.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnPreviewDatabase.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnPreviewDatabase.FlatAppearance.BorderSize = 0;
            this.btnPreviewDatabase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPreviewDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPreviewDatabase.ForeColor = System.Drawing.Color.White;
            this.btnPreviewDatabase.Location = new System.Drawing.Point(1, 162);
            this.btnPreviewDatabase.Name = "btnPreviewDatabase";
            this.btnPreviewDatabase.Size = new System.Drawing.Size(194, 60);
            this.btnPreviewDatabase.TabIndex = 6;
            this.btnPreviewDatabase.Text = "Preview database";
            this.btnPreviewDatabase.TextColor = System.Drawing.Color.White;
            this.btnPreviewDatabase.UseVisualStyleBackColor = false;
            // 
            // btnPreviewComparison
            // 
            this.btnPreviewComparison._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnPreviewComparison._BorderRadius = 10;
            this.btnPreviewComparison._BorderSize = 0;
            this.btnPreviewComparison._GradientsButton = false;
            this.btnPreviewComparison._Text = "Preview comparison data";
            this.btnPreviewComparison.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnPreviewComparison.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(151)))), ((int)(((byte)(149)))));
            this.btnPreviewComparison.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnPreviewComparison.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnPreviewComparison.FlatAppearance.BorderSize = 0;
            this.btnPreviewComparison.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPreviewComparison.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPreviewComparison.ForeColor = System.Drawing.Color.White;
            this.btnPreviewComparison.Location = new System.Drawing.Point(3, 100);
            this.btnPreviewComparison.Name = "btnPreviewComparison";
            this.btnPreviewComparison.Size = new System.Drawing.Size(191, 60);
            this.btnPreviewComparison.TabIndex = 5;
            this.btnPreviewComparison.Text = "Preview comparison data";
            this.btnPreviewComparison.TextColor = System.Drawing.Color.White;
            this.btnPreviewComparison.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            this.btnSave._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnSave._BorderRadius = 10;
            this.btnSave._BorderSize = 0;
            this.btnSave._GradientsButton = false;
            this.btnSave._Text = "OK";
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnSave.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnSave.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnSave.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(1, 37);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(194, 60);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "OK";
            this.btnSave.TextColor = System.Drawing.Color.White;
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // tblPanelMain
            // 
            this.tblPanelMain.BackColor = System.Drawing.Color.White;
            this.tblPanelMain.ColumnCount = 1;
            this.tblPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblPanelMain.Controls.Add(this.pnlPOD, 0, 0);
            this.tblPanelMain.Controls.Add(this.pnlDisplay, 0, 1);
            this.tblPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblPanelMain.Location = new System.Drawing.Point(20, 35);
            this.tblPanelMain.Name = "tblPanelMain";
            this.tblPanelMain.RowCount = 2;
            this.tblPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 44.01623F));
            this.tblPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55.98377F));
            this.tblPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblPanelMain.Size = new System.Drawing.Size(1004, 565);
            this.tblPanelMain.TabIndex = 0;
            // 
            // numberOfCodes
            // 
            this.numberOfCodes.AutoSize = true;
            this.numberOfCodes.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.numberOfCodes.ForeColor = System.Drawing.Color.DarkOrange;
            this.numberOfCodes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.numberOfCodes.Location = new System.Drawing.Point(770, 87);
            this.numberOfCodes.Name = "numberOfCodes";
            this.numberOfCodes.Size = new System.Drawing.Size(136, 20);
            this.numberOfCodes.TabIndex = 134;
            this.numberOfCodes.Text = "Number Of Codes";
            this.numberOfCodes.Visible = false;
            // 
            // FrmPODFormat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(1024, 600);
            this.Controls.Add(this.tblPanelMain);
            this.Controls.Add(this.pnlDrag);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmPODFormat";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "POD Format";
            this.Load += new System.EventHandler(this.FrmPODFormat_Load);
            this.pnlDrag.ResumeLayout(false);
            this.pnlDrag.PerformLayout();
            this.pnlDisplay.ResumeLayout(false);
            this.pnlDisplay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSampleData)).EndInit();
            this.pnlPOD.ResumeLayout(false);
            this.tblPanelPOD.ResumeLayout(false);
            this.pnlPODList.ResumeLayout(false);
            this.pnlPODList.PerformLayout();
            this.tblPanelButton.ResumeLayout(false);
            this.pnlPODFormat.ResumeLayout(false);
            this.pnlPODFormat.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.tblPanelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnlDrag;
        private DesignUI.CuzUI.CuzDragControl cuzDragControl1;
        private System.Windows.Forms.Label lblFormName;
        private DesignUI.CuzUI.CuzControlBox cuzControlBox1;
        private DesignUI.CuzUI.CuzDropShadow cuzDropShadow1;
        private System.Windows.Forms.TableLayoutPanel tblPanelMain;
        private System.Windows.Forms.Panel pnlPOD;
        private System.Windows.Forms.TableLayoutPanel tblPanelPOD;
        private System.Windows.Forms.Panel pnlPODList;
        private System.Windows.Forms.ListBox listBoxPODLeft;
        private System.Windows.Forms.Label lblPODlist;
        private System.Windows.Forms.TableLayoutPanel tblPanelButton;
        private DesignUI.CuzUI.CuzButton btnAdd;
        private DesignUI.CuzUI.CuzButton btnRemove;
        private DesignUI.CuzUI.CuzButton btnUp;
        private DesignUI.CuzUI.CuzButton btnDown;
        private DesignUI.CuzUI.CuzButton btnClear;
        private System.Windows.Forms.Panel pnlPODFormat;
        private System.Windows.Forms.ListBox listBoxPODRight;
        private System.Windows.Forms.Label lblPODformat;
        private System.Windows.Forms.Panel pnlDisplay;
        private System.Windows.Forms.TextBox txtSample;
        private System.Windows.Forms.Label lblSample;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.Panel panel1;
        private DesignUI.CuzUI.CuzButton btnPreviewComparison;
        private DesignUI.CuzUI.CuzButton btnSave;
        private DesignUI.CuzUI.CuzButton btnPreviewDatabase;
        private DesignUI.CuzUI.CuzDataGridView dgvSampleData;
        private System.Windows.Forms.Label numberOfCodes;
    }
}