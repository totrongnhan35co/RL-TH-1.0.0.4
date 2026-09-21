namespace BarcodeVerificationSystem.View
{
    partial class FrmPrintFieldForVerifyAndPrintCompareMode
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPrintFieldForVerifyAndPrintCompareMode));
            this.tblPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlPOD = new System.Windows.Forms.Panel();
            this.tblPanelPOD = new System.Windows.Forms.TableLayoutPanel();
            this.pnlPODList = new System.Windows.Forms.Panel();
            this.listBoxPODLeft = new System.Windows.Forms.ListBox();
            this.lblFieldlist = new System.Windows.Forms.Label();
            this.tblPanelButton = new System.Windows.Forms.TableLayoutPanel();
            this.btnAdd = new DesignUI.CuzUI.CuzButton();
            this.btnRemove = new DesignUI.CuzUI.CuzButton();
            this.btnClear = new DesignUI.CuzUI.CuzButton();
            this.pnlPODFormat = new System.Windows.Forms.Panel();
            this.listBoxPODRight = new System.Windows.Forms.ListBox();
            this.lblSelectedFiled = new System.Windows.Forms.Label();
            this.pnlDialogResult = new System.Windows.Forms.Panel();
            this.btnSave = new DesignUI.CuzUI.CuzButton();
            this.btnCancel = new DesignUI.CuzUI.CuzButton();
            this.pnlDisplay = new System.Windows.Forms.Panel();
            this.lblPrintFields = new System.Windows.Forms.Label();
            this.txtPrintFields = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlDrag = new System.Windows.Forms.Panel();
            this.cuzControlBox1 = new DesignUI.CuzUI.CuzControlBox();
            this.lblFormName = new System.Windows.Forms.Label();
            this.cuzDragControl1 = new DesignUI.CuzUI.CuzDragControl();
            this.cuzDropShadow1 = new DesignUI.CuzUI.CuzDropShadow();
            this.tblPanelMain.SuspendLayout();
            this.pnlPOD.SuspendLayout();
            this.tblPanelPOD.SuspendLayout();
            this.pnlPODList.SuspendLayout();
            this.tblPanelButton.SuspendLayout();
            this.pnlPODFormat.SuspendLayout();
            this.pnlDialogResult.SuspendLayout();
            this.pnlDisplay.SuspendLayout();
            this.pnlDrag.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblPanelMain
            // 
            this.tblPanelMain.BackColor = System.Drawing.Color.White;
            this.tblPanelMain.ColumnCount = 1;
            this.tblPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblPanelMain.Controls.Add(this.pnlPOD, 0, 0);
            this.tblPanelMain.Controls.Add(this.pnlDialogResult, 0, 2);
            this.tblPanelMain.Controls.Add(this.pnlDisplay, 0, 1);
            this.tblPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblPanelMain.Location = new System.Drawing.Point(20, 35);
            this.tblPanelMain.Name = "tblPanelMain";
            this.tblPanelMain.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.tblPanelMain.RowCount = 3;
            this.tblPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55.58252F));
            this.tblPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 44.41748F));
            this.tblPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 61F));
            this.tblPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblPanelMain.Size = new System.Drawing.Size(816, 480);
            this.tblPanelMain.TabIndex = 0;
            // 
            // pnlPOD
            // 
            this.pnlPOD.Controls.Add(this.tblPanelPOD);
            this.pnlPOD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPOD.Location = new System.Drawing.Point(3, 3);
            this.pnlPOD.Name = "pnlPOD";
            this.pnlPOD.Size = new System.Drawing.Size(810, 221);
            this.pnlPOD.TabIndex = 0;
            // 
            // tblPanelPOD
            // 
            this.tblPanelPOD.BackColor = System.Drawing.Color.White;
            this.tblPanelPOD.ColumnCount = 3;
            this.tblPanelPOD.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblPanelPOD.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tblPanelPOD.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblPanelPOD.Controls.Add(this.pnlPODList, 0, 0);
            this.tblPanelPOD.Controls.Add(this.tblPanelButton, 1, 0);
            this.tblPanelPOD.Controls.Add(this.pnlPODFormat, 2, 0);
            this.tblPanelPOD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblPanelPOD.Location = new System.Drawing.Point(0, 0);
            this.tblPanelPOD.Name = "tblPanelPOD";
            this.tblPanelPOD.RowCount = 1;
            this.tblPanelPOD.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblPanelPOD.Size = new System.Drawing.Size(810, 221);
            this.tblPanelPOD.TabIndex = 0;
            // 
            // pnlPODList
            // 
            this.pnlPODList.BackColor = System.Drawing.Color.White;
            this.pnlPODList.Controls.Add(this.listBoxPODLeft);
            this.pnlPODList.Controls.Add(this.lblFieldlist);
            this.pnlPODList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPODList.Location = new System.Drawing.Point(3, 3);
            this.pnlPODList.Name = "pnlPODList";
            this.pnlPODList.Size = new System.Drawing.Size(359, 215);
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
            this.listBoxPODLeft.Location = new System.Drawing.Point(10, 23);
            this.listBoxPODLeft.Name = "listBoxPODLeft";
            this.listBoxPODLeft.Size = new System.Drawing.Size(334, 164);
            this.listBoxPODLeft.TabIndex = 0;
            // 
            // lblFieldlist
            // 
            this.lblFieldlist.AutoSize = true;
            this.lblFieldlist.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFieldlist.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblFieldlist.Location = new System.Drawing.Point(6, -2);
            this.lblFieldlist.Name = "lblFieldlist";
            this.lblFieldlist.Size = new System.Drawing.Size(66, 20);
            this.lblFieldlist.TabIndex = 0;
            this.lblFieldlist.Text = "POD list";
            // 
            // tblPanelButton
            // 
            this.tblPanelButton.ColumnCount = 1;
            this.tblPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblPanelButton.Controls.Add(this.btnAdd, 0, 0);
            this.tblPanelButton.Controls.Add(this.btnRemove, 0, 1);
            this.tblPanelButton.Controls.Add(this.btnClear, 0, 2);
            this.tblPanelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblPanelButton.Location = new System.Drawing.Point(368, 20);
            this.tblPanelButton.Margin = new System.Windows.Forms.Padding(3, 20, 3, 30);
            this.tblPanelButton.Name = "tblPanelButton";
            this.tblPanelButton.RowCount = 3;
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblPanelButton.Size = new System.Drawing.Size(74, 171);
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
            this.btnAdd.Size = new System.Drawing.Size(68, 51);
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
            this.btnRemove.Location = new System.Drawing.Point(3, 60);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(68, 51);
            this.btnRemove.TabIndex = 7;
            this.btnRemove.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnRemove.UseVisualStyleBackColor = false;
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
            this.btnClear.Location = new System.Drawing.Point(3, 117);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(68, 51);
            this.btnClear.TabIndex = 8;
            this.btnClear.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnClear.UseVisualStyleBackColor = false;
            // 
            // pnlPODFormat
            // 
            this.pnlPODFormat.BackColor = System.Drawing.Color.White;
            this.pnlPODFormat.Controls.Add(this.listBoxPODRight);
            this.pnlPODFormat.Controls.Add(this.lblSelectedFiled);
            this.pnlPODFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPODFormat.Location = new System.Drawing.Point(448, 3);
            this.pnlPODFormat.Name = "pnlPODFormat";
            this.pnlPODFormat.Size = new System.Drawing.Size(359, 215);
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
            this.listBoxPODRight.Location = new System.Drawing.Point(17, 23);
            this.listBoxPODRight.Name = "listBoxPODRight";
            this.listBoxPODRight.Size = new System.Drawing.Size(320, 164);
            this.listBoxPODRight.TabIndex = 1;
            // 
            // lblSelectedFiled
            // 
            this.lblSelectedFiled.AutoSize = true;
            this.lblSelectedFiled.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectedFiled.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblSelectedFiled.Location = new System.Drawing.Point(13, -1);
            this.lblSelectedFiled.Name = "lblSelectedFiled";
            this.lblSelectedFiled.Size = new System.Drawing.Size(93, 20);
            this.lblSelectedFiled.TabIndex = 0;
            this.lblSelectedFiled.Text = "POD format";
            // 
            // pnlDialogResult
            // 
            this.pnlDialogResult.Controls.Add(this.btnSave);
            this.pnlDialogResult.Controls.Add(this.btnCancel);
            this.pnlDialogResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDialogResult.Location = new System.Drawing.Point(3, 411);
            this.pnlDialogResult.Name = "pnlDialogResult";
            this.pnlDialogResult.Size = new System.Drawing.Size(810, 56);
            this.pnlDialogResult.TabIndex = 2;
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
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(256, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(150, 45);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "OK";
            this.btnSave.TextColor = System.Drawing.Color.White;
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnCancel._BorderRadius = 10;
            this.btnCancel._BorderSize = 0;
            this.btnCancel._GradientsButton = false;
            this.btnCancel._Text = "Cancel";
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnCancel.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnCancel.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnCancel.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(412, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 45);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextColor = System.Drawing.Color.White;
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // pnlDisplay
            // 
            this.pnlDisplay.Controls.Add(this.lblPrintFields);
            this.pnlDisplay.Controls.Add(this.txtPrintFields);
            this.pnlDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDisplay.Location = new System.Drawing.Point(3, 230);
            this.pnlDisplay.Name = "pnlDisplay";
            this.pnlDisplay.Size = new System.Drawing.Size(810, 175);
            this.pnlDisplay.TabIndex = 3;
            // 
            // lblPrintFields
            // 
            this.lblPrintFields.AutoSize = true;
            this.lblPrintFields.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrintFields.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblPrintFields.Location = new System.Drawing.Point(9, 6);
            this.lblPrintFields.Name = "lblPrintFields";
            this.lblPrintFields.Size = new System.Drawing.Size(63, 20);
            this.lblPrintFields.TabIndex = 3;
            this.lblPrintFields.Text = "Sample";
            // 
            // txtPrintFields
            // 
            this.txtPrintFields.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPrintFields.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPrintFields.Location = new System.Drawing.Point(13, 30);
            this.txtPrintFields.Multiline = true;
            this.txtPrintFields.Name = "txtPrintFields";
            this.txtPrintFields.ReadOnly = true;
            this.txtPrintFields.Size = new System.Drawing.Size(772, 128);
            this.txtPrintFields.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(20, 515);
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
            this.pnlDrag.Size = new System.Drawing.Size(816, 35);
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
            this.cuzControlBox1.Location = new System.Drawing.Point(771, 0);
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
            this.lblFormName.Size = new System.Drawing.Size(93, 20);
            this.lblFormName.TabIndex = 2;
            this.lblFormName.Text = "POD format";
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
            // frmPrintFieldForVerifyAndPrintCompareMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(836, 515);
            this.Controls.Add(this.tblPanelMain);
            this.Controls.Add(this.pnlDrag);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPrintFieldForVerifyAndPrintCompareMode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "POD Format";
            this.Load += new System.EventHandler(this.FrmPODFormat_Load);
            this.tblPanelMain.ResumeLayout(false);
            this.pnlPOD.ResumeLayout(false);
            this.tblPanelPOD.ResumeLayout(false);
            this.pnlPODList.ResumeLayout(false);
            this.pnlPODList.PerformLayout();
            this.tblPanelButton.ResumeLayout(false);
            this.pnlPODFormat.ResumeLayout(false);
            this.pnlPODFormat.PerformLayout();
            this.pnlDialogResult.ResumeLayout(false);
            this.pnlDisplay.ResumeLayout(false);
            this.pnlDisplay.PerformLayout();
            this.pnlDrag.ResumeLayout(false);
            this.pnlDrag.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblPanelMain;
        private System.Windows.Forms.Panel pnlDisplay;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnlDrag;
        private DesignUI.CuzUI.CuzDragControl cuzDragControl1;
        private System.Windows.Forms.Label lblFormName;
        private System.Windows.Forms.Label lblPrintFields;
        private System.Windows.Forms.TextBox txtPrintFields;
        private System.Windows.Forms.Panel pnlPOD;
        private System.Windows.Forms.TableLayoutPanel tblPanelPOD;
        private System.Windows.Forms.TableLayoutPanel tblPanelButton;
        private System.Windows.Forms.Panel pnlPODFormat;
        private System.Windows.Forms.ListBox listBoxPODRight;
        private System.Windows.Forms.Label lblSelectedFiled;
        private DesignUI.CuzUI.CuzButton btnAdd;
        private DesignUI.CuzUI.CuzButton btnRemove;
        private DesignUI.CuzUI.CuzControlBox cuzControlBox1;
        private System.Windows.Forms.Panel pnlPODList;
        private System.Windows.Forms.ListBox listBoxPODLeft;
        private System.Windows.Forms.Label lblFieldlist;
        private DesignUI.CuzUI.CuzDropShadow cuzDropShadow1;
        private System.Windows.Forms.Panel pnlDialogResult;
        private DesignUI.CuzUI.CuzButton btnSave;
        private DesignUI.CuzUI.CuzButton btnCancel;
        private DesignUI.CuzUI.CuzButton btnClear;
    }
}