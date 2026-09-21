namespace BarcodeVerificationSystem.View
{
    partial class FrmManageAccount
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmManageAccount));
            this.lbUsername = new System.Windows.Forms.Label();
            this.lbRole = new System.Windows.Forms.Label();
            this.lbRetypePasword = new System.Windows.Forms.Label();
            this.lbPassword = new System.Windows.Forms.Label();
            this.lbFullname = new System.Windows.Forms.Label();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.btnSearchJobs = new System.Windows.Forms.Button();
            this.dgvAccount = new System.Windows.Forms.DataGridView();
            this.btnAccountRefresh = new DesignUI.CuzUI.CuzButton();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.txtFind = new DesignUI.CuzUI.CuzTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnAdd = new DesignUI.CuzUI.CuzButton();
            this.btnDelete = new DesignUI.CuzUI.CuzButton();
            this.btnEdit = new DesignUI.CuzUI.CuzButton();
            this.btnSave = new DesignUI.CuzUI.CuzButton();
            this.txtRetypePassword = new DesignUI.CuzUI.CuzTextBox();
            this.txtPassword = new DesignUI.CuzUI.CuzTextBox();
            this.txtUserName = new DesignUI.CuzUI.CuzTextBox();
            this.txtFullName = new DesignUI.CuzUI.CuzTextBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblInfoTitle = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cbxRole = new System.Windows.Forms.ComboBox();
            this.tblPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlDrag = new System.Windows.Forms.Panel();
            this.cuzControlBox1 = new DesignUI.CuzUI.CuzControlBox();
            this.lblFormName = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.cuzDropShadow1 = new DesignUI.CuzUI.CuzDropShadow();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAccount)).BeginInit();
            this.pnlRight.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tblPanelMain.SuspendLayout();
            this.pnlDrag.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbUsername
            // 
            this.lbUsername.AutoSize = true;
            this.lbUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbUsername.Location = new System.Drawing.Point(21, 138);
            this.lbUsername.Name = "lbUsername";
            this.lbUsername.Size = new System.Drawing.Size(83, 20);
            this.lbUsername.TabIndex = 40;
            this.lbUsername.Text = "Username";
            // 
            // lbRole
            // 
            this.lbRole.AutoSize = true;
            this.lbRole.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRole.Location = new System.Drawing.Point(21, 347);
            this.lbRole.Name = "lbRole";
            this.lbRole.Size = new System.Drawing.Size(42, 20);
            this.lbRole.TabIndex = 44;
            this.lbRole.Text = "Role";
            // 
            // lbRetypePasword
            // 
            this.lbRetypePasword.AutoSize = true;
            this.lbRetypePasword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRetypePasword.Location = new System.Drawing.Point(21, 277);
            this.lbRetypePasword.Name = "lbRetypePasword";
            this.lbRetypePasword.Size = new System.Drawing.Size(139, 20);
            this.lbRetypePasword.TabIndex = 42;
            this.lbRetypePasword.Text = "Re-tupe password";
            // 
            // lbPassword
            // 
            this.lbPassword.AutoSize = true;
            this.lbPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPassword.Location = new System.Drawing.Point(21, 208);
            this.lbPassword.Name = "lbPassword";
            this.lbPassword.Size = new System.Drawing.Size(78, 20);
            this.lbPassword.TabIndex = 41;
            this.lbPassword.Text = "Password";
            // 
            // lbFullname
            // 
            this.lbFullname.AutoSize = true;
            this.lbFullname.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbFullname.Location = new System.Drawing.Point(21, 67);
            this.lbFullname.Name = "lbFullname";
            this.lbFullname.Size = new System.Drawing.Size(74, 20);
            this.lbFullname.TabIndex = 39;
            this.lbFullname.Text = "Fullname";
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnlLeft.Controls.Add(this.btnSearchJobs);
            this.pnlLeft.Controls.Add(this.dgvAccount);
            this.pnlLeft.Controls.Add(this.btnAccountRefresh);
            this.pnlLeft.Controls.Add(this.lblListTitle);
            this.pnlLeft.Controls.Add(this.txtFind);
            this.pnlLeft.Controls.Add(this.panel1);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.Location = new System.Drawing.Point(3, 3);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(562, 559);
            this.pnlLeft.TabIndex = 0;
            // 
            // btnSearchJobs
            // 
            this.btnSearchJobs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearchJobs.FlatAppearance.BorderSize = 0;
            this.btnSearchJobs.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnSearchJobs.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnSearchJobs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchJobs.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSearchJobs.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_search_181;
            this.btnSearchJobs.Location = new System.Drawing.Point(427, 69);
            this.btnSearchJobs.Name = "btnSearchJobs";
            this.btnSearchJobs.Size = new System.Drawing.Size(30, 30);
            this.btnSearchJobs.TabIndex = 96;
            this.btnSearchJobs.UseVisualStyleBackColor = true;
            // 
            // dgvAccount
            // 
            this.dgvAccount.BackgroundColor = System.Drawing.Color.White;
            this.dgvAccount.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAccount.Location = new System.Drawing.Point(20, 119);
            this.dgvAccount.MultiSelect = false;
            this.dgvAccount.Name = "dgvAccount";
            this.dgvAccount.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAccount.Size = new System.Drawing.Size(519, 416);
            this.dgvAccount.TabIndex = 95;
            // 
            // btnAccountRefresh
            // 
            this.btnAccountRefresh._BorderColor = System.Drawing.Color.Silver;
            this.btnAccountRefresh._BorderRadius = 10;
            this.btnAccountRefresh._BorderSize = 1;
            this.btnAccountRefresh._GradientsButton = false;
            this.btnAccountRefresh._Text = "";
            this.btnAccountRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAccountRefresh.BackColor = System.Drawing.Color.White;
            this.btnAccountRefresh.BackgroundColor = System.Drawing.Color.White;
            this.btnAccountRefresh.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnAccountRefresh.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnAccountRefresh.FlatAppearance.BorderSize = 0;
            this.btnAccountRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAccountRefresh.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnAccountRefresh.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAccountRefresh.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_refresh_161;
            this.btnAccountRefresh.Location = new System.Drawing.Point(473, 63);
            this.btnAccountRefresh.Name = "btnAccountRefresh";
            this.btnAccountRefresh.Size = new System.Drawing.Size(66, 40);
            this.btnAccountRefresh.TabIndex = 94;
            this.btnAccountRefresh.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnAccountRefresh.UseVisualStyleBackColor = false;
            // 
            // lblListTitle
            // 
            this.lblListTitle.AutoSize = true;
            this.lblListTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblListTitle.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblListTitle.Location = new System.Drawing.Point(16, 12);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(103, 20);
            this.lblListTitle.TabIndex = 90;
            this.lblListTitle.Text = "Account list";
            // 
            // txtFind
            // 
            this.txtFind._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtFind._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtFind.BackColor = System.Drawing.Color.White;
            this.txtFind.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtFind.BorderFocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.txtFind.BorderRadius = 6;
            this.txtFind.BorderSize = 1;
            this.txtFind.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFind.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtFind.Location = new System.Drawing.Point(20, 65);
            this.txtFind.Margin = new System.Windows.Forms.Padding(4);
            this.txtFind.Multiline = false;
            this.txtFind.Name = "txtFind";
            this.txtFind.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.txtFind.PasswordChar = false;
            this.txtFind.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtFind.PlaceholderText = "";
            this.txtFind.ReadOnly = false;
            this.txtFind.Size = new System.Drawing.Size(446, 35);
            this.txtFind.TabIndex = 93;
            this.txtFind.UnderlinedStyle = false;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(20, 50);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(519, 1);
            this.panel1.TabIndex = 88;
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnlRight.Controls.Add(this.tableLayoutPanel1);
            this.pnlRight.Controls.Add(this.txtRetypePassword);
            this.pnlRight.Controls.Add(this.txtPassword);
            this.pnlRight.Controls.Add(this.txtUserName);
            this.pnlRight.Controls.Add(this.txtFullName);
            this.pnlRight.Controls.Add(this.lblMessage);
            this.pnlRight.Controls.Add(this.lblInfoTitle);
            this.pnlRight.Controls.Add(this.panel2);
            this.pnlRight.Controls.Add(this.lbUsername);
            this.pnlRight.Controls.Add(this.cbxRole);
            this.pnlRight.Controls.Add(this.lbRole);
            this.pnlRight.Controls.Add(this.lbFullname);
            this.pnlRight.Controls.Add(this.lbPassword);
            this.pnlRight.Controls.Add(this.lbRetypePasword);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(571, 3);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(0, 0, 0, 27);
            this.pnlRight.Size = new System.Drawing.Size(430, 559);
            this.pnlRight.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.btnAdd, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnDelete, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnEdit, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSave, 2, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(71, 441);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(339, 54);
            this.tableLayoutPanel1.TabIndex = 96;
            // 
            // btnAdd
            // 
            this.btnAdd._BorderColor = System.Drawing.Color.Silver;
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
            this.btnAdd.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_add_181;
            this.btnAdd.Location = new System.Drawing.Point(3, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(78, 48);
            this.btnAdd.TabIndex = 95;
            this.btnAdd.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnAdd.UseVisualStyleBackColor = false;
            // 
            // btnDelete
            // 
            this.btnDelete._BorderColor = System.Drawing.Color.Silver;
            this.btnDelete._BorderRadius = 10;
            this.btnDelete._BorderSize = 1;
            this.btnDelete._GradientsButton = false;
            this.btnDelete._Text = "";
            this.btnDelete.BackColor = System.Drawing.Color.White;
            this.btnDelete.BackgroundColor = System.Drawing.Color.White;
            this.btnDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDelete.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnDelete.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnDelete.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDelete.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_delete_181;
            this.btnDelete.Location = new System.Drawing.Point(255, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(81, 48);
            this.btnDelete.TabIndex = 95;
            this.btnDelete.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnEdit
            // 
            this.btnEdit._BorderColor = System.Drawing.Color.Silver;
            this.btnEdit._BorderRadius = 10;
            this.btnEdit._BorderSize = 1;
            this.btnEdit._GradientsButton = false;
            this.btnEdit._Text = "";
            this.btnEdit.BackColor = System.Drawing.Color.White;
            this.btnEdit.BackgroundColor = System.Drawing.Color.White;
            this.btnEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnEdit.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnEdit.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnEdit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnEdit.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_edit_181;
            this.btnEdit.Location = new System.Drawing.Point(87, 3);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(78, 48);
            this.btnEdit.TabIndex = 95;
            this.btnEdit.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnEdit.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            this.btnSave._BorderColor = System.Drawing.Color.Silver;
            this.btnSave._BorderRadius = 10;
            this.btnSave._BorderSize = 1;
            this.btnSave._GradientsButton = false;
            this.btnSave._Text = "";
            this.btnSave.BackColor = System.Drawing.Color.White;
            this.btnSave.BackgroundColor = System.Drawing.Color.White;
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSave.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnSave.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnSave.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnSave.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_save_181;
            this.btnSave.Location = new System.Drawing.Point(171, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(78, 48);
            this.btnSave.TabIndex = 94;
            this.btnSave.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // txtRetypePassword
            // 
            this.txtRetypePassword._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtRetypePassword._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtRetypePassword.BackColor = System.Drawing.Color.White;
            this.txtRetypePassword.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtRetypePassword.BorderFocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.txtRetypePassword.BorderRadius = 6;
            this.txtRetypePassword.BorderSize = 1;
            this.txtRetypePassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRetypePassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtRetypePassword.Location = new System.Drawing.Point(24, 303);
            this.txtRetypePassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtRetypePassword.Multiline = false;
            this.txtRetypePassword.Name = "txtRetypePassword";
            this.txtRetypePassword.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.txtRetypePassword.PasswordChar = true;
            this.txtRetypePassword.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtRetypePassword.PlaceholderText = "";
            this.txtRetypePassword.ReadOnly = false;
            this.txtRetypePassword.Size = new System.Drawing.Size(386, 35);
            this.txtRetypePassword.TabIndex = 93;
            this.txtRetypePassword.UnderlinedStyle = false;
            // 
            // txtPassword
            // 
            this.txtPassword._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtPassword._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtPassword.BackColor = System.Drawing.Color.White;
            this.txtPassword.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtPassword.BorderFocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.txtPassword.BorderRadius = 6;
            this.txtPassword.BorderSize = 1;
            this.txtPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtPassword.Location = new System.Drawing.Point(24, 234);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtPassword.Multiline = false;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.txtPassword.PasswordChar = true;
            this.txtPassword.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtPassword.PlaceholderText = "";
            this.txtPassword.ReadOnly = false;
            this.txtPassword.Size = new System.Drawing.Size(386, 35);
            this.txtPassword.TabIndex = 93;
            this.txtPassword.UnderlinedStyle = false;
            // 
            // txtUserName
            // 
            this.txtUserName._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtUserName._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtUserName.BackColor = System.Drawing.Color.White;
            this.txtUserName.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtUserName.BorderFocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.txtUserName.BorderRadius = 6;
            this.txtUserName.BorderSize = 1;
            this.txtUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtUserName.Location = new System.Drawing.Point(24, 164);
            this.txtUserName.Margin = new System.Windows.Forms.Padding(4);
            this.txtUserName.Multiline = false;
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.txtUserName.PasswordChar = false;
            this.txtUserName.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtUserName.PlaceholderText = "";
            this.txtUserName.ReadOnly = false;
            this.txtUserName.Size = new System.Drawing.Size(386, 35);
            this.txtUserName.TabIndex = 93;
            this.txtUserName.UnderlinedStyle = false;
            // 
            // txtFullName
            // 
            this.txtFullName._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtFullName._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtFullName.BackColor = System.Drawing.Color.White;
            this.txtFullName.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtFullName.BorderFocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.txtFullName.BorderRadius = 6;
            this.txtFullName.BorderSize = 1;
            this.txtFullName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFullName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtFullName.Location = new System.Drawing.Point(24, 93);
            this.txtFullName.Margin = new System.Windows.Forms.Padding(4);
            this.txtFullName.Multiline = false;
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.txtFullName.PasswordChar = false;
            this.txtFullName.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtFullName.PlaceholderText = "";
            this.txtFullName.ReadOnly = false;
            this.txtFullName.Size = new System.Drawing.Size(386, 35);
            this.txtFullName.TabIndex = 93;
            this.txtFullName.UnderlinedStyle = false;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.ForeColor = System.Drawing.Color.Red;
            this.lblMessage.Location = new System.Drawing.Point(21, 421);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(13, 20);
            this.lblMessage.TabIndex = 92;
            this.lblMessage.Text = " ";
            // 
            // lblInfoTitle
            // 
            this.lblInfoTitle.AutoSize = true;
            this.lblInfoTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfoTitle.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblInfoTitle.Location = new System.Drawing.Point(21, 14);
            this.lblInfoTitle.Name = "lblInfoTitle";
            this.lblInfoTitle.Size = new System.Drawing.Size(101, 20);
            this.lblInfoTitle.TabIndex = 91;
            this.lblInfoTitle.Text = "Information";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Location = new System.Drawing.Point(24, 50);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(386, 1);
            this.panel2.TabIndex = 89;
            // 
            // cbxRole
            // 
            this.cbxRole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxRole.BackColor = System.Drawing.Color.WhiteSmoke;
            this.cbxRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRole.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbxRole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.cbxRole.FormattingEnabled = true;
            this.cbxRole.Location = new System.Drawing.Point(25, 373);
            this.cbxRole.Name = "cbxRole";
            this.cbxRole.Size = new System.Drawing.Size(385, 28);
            this.cbxRole.TabIndex = 43;
            // 
            // tblPanelMain
            // 
            this.tblPanelMain.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.tblPanelMain.ColumnCount = 2;
            this.tblPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56.625F));
            this.tblPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43.375F));
            this.tblPanelMain.Controls.Add(this.pnlRight, 1, 0);
            this.tblPanelMain.Controls.Add(this.pnlLeft, 0, 0);
            this.tblPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblPanelMain.Location = new System.Drawing.Point(20, 35);
            this.tblPanelMain.Name = "tblPanelMain";
            this.tblPanelMain.RowCount = 1;
            this.tblPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblPanelMain.Size = new System.Drawing.Size(1004, 565);
            this.tblPanelMain.TabIndex = 1;
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
            this.pnlDrag.TabIndex = 59;
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
            this.cuzControlBox1.TabIndex = 2;
            this.cuzControlBox1.UseVisualStyleBackColor = false;
            // 
            // lblFormName
            // 
            this.lblFormName.AutoSize = true;
            this.lblFormName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormName.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblFormName.Location = new System.Drawing.Point(3, 6);
            this.lblFormName.Name = "lblFormName";
            this.lblFormName.Size = new System.Drawing.Size(128, 20);
            this.lblFormName.TabIndex = 1;
            this.lblFormName.Text = "Manage account";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(20, 600);
            this.panel3.TabIndex = 58;
            // 
            // cuzDropShadow1
            // 
            this.cuzDropShadow1.TargetControl = this;
            // 
            // frmManageAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 600);
            this.Controls.Add(this.tblPanelMain);
            this.Controls.Add(this.pnlDrag);
            this.Controls.Add(this.panel3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1024, 600);
            this.Name = "frmManageAccount";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage";
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAccount)).EndInit();
            this.pnlRight.ResumeLayout(false);
            this.pnlRight.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tblPanelMain.ResumeLayout(false);
            this.pnlDrag.ResumeLayout(false);
            this.pnlDrag.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.TableLayoutPanel tblPanelMain;
        private System.Windows.Forms.Label lbUsername;
        private System.Windows.Forms.Label lbRole;
        private System.Windows.Forms.Label lbRetypePasword;
        private System.Windows.Forms.Label lbPassword;
        private System.Windows.Forms.Label lbFullname;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblListTitle;
        private System.Windows.Forms.Label lblInfoTitle;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Panel pnlDrag;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblFormName;
        private DesignUI.CuzUI.CuzTextBox txtRetypePassword;
        private DesignUI.CuzUI.CuzTextBox txtPassword;
        private DesignUI.CuzUI.CuzTextBox txtUserName;
        private DesignUI.CuzUI.CuzTextBox txtFullName;
        private System.Windows.Forms.ComboBox cbxRole;
        private DesignUI.CuzUI.CuzTextBox txtFind;
        private DesignUI.CuzUI.CuzButton btnAccountRefresh;
        private DesignUI.CuzUI.CuzButton btnSave;
        private DesignUI.CuzUI.CuzButton btnDelete;
        private DesignUI.CuzUI.CuzButton btnAdd;
        private DesignUI.CuzUI.CuzButton btnEdit;
        private DesignUI.CuzUI.CuzDropShadow cuzDropShadow1;
        private System.Windows.Forms.DataGridView dgvAccount;
        private DesignUI.CuzUI.CuzControlBox cuzControlBox1;
        private System.Windows.Forms.Button btnSearchJobs;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}