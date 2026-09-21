namespace BarcodeVerificationSystem.View
{
    partial class FrmRemotePrinter
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
            if (disposing)
            {
                try { webView21?.Dispose(); } catch { }
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.components = new System.ComponentModel.Container();
            this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.btnMenu = new DesignUI.CuzUI.CuzButton();
            this.cuzDropdownMenu = new DesignUI.CuzUI.CuzDropdownMenu(this.components);
            this.reloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cuzDragControl1 = new DesignUI.CuzUI.CuzDragControl();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            this.cuzDropdownMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // webView21
            // 
            this.webView21.AllowExternalDrop = true;
            this.webView21.CreationProperties = null;
            this.webView21.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView21.Location = new System.Drawing.Point(0, 0);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(1024, 600);
            this.webView21.TabIndex = 1;
            this.webView21.ZoomFactor = 1D;
            // 
            // btnMenu
            // 
            this.btnMenu._BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(171)))), ((int)(((byte)(230)))));
            this.btnMenu._BorderRadius = 10;
            this.btnMenu._BorderSize = 0;
            this.btnMenu._GradientsButton = false;
            this.btnMenu._Text = "";
            this.btnMenu.BackColor = System.Drawing.SystemColors.Control;
            this.btnMenu.BackgroundColor = System.Drawing.SystemColors.Control;
            this.btnMenu.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnMenu.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnMenu.FlatAppearance.BorderSize = 0;
            this.btnMenu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMenu.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnMenu.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnMenu.Image = global::BarcodeVerificationSystem.Properties.Resources.icon_settings_321;
            this.btnMenu.Location = new System.Drawing.Point(942, 21);
            this.btnMenu.Margin = new System.Windows.Forms.Padding(0);
            this.btnMenu.Name = "btnMenu";
            this.btnMenu.Size = new System.Drawing.Size(60, 60);
            this.btnMenu.TabIndex = 2;
            this.btnMenu.TextColor = System.Drawing.SystemColors.ControlText;
            this.btnMenu.UseVisualStyleBackColor = false;
            // 
            // cuzDropdownMenu
            // 
            this.cuzDropdownMenu.IsMainMenu = false;
            this.cuzDropdownMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reloadToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.cuzDropdownMenu.MenuItemHeight = 25;
            this.cuzDropdownMenu.MenuItemTextColor = System.Drawing.Color.Empty;
            this.cuzDropdownMenu.Name = "cuzDropdownMenu1";
            this.cuzDropdownMenu.PrimaryColor = System.Drawing.Color.Empty;
            this.cuzDropdownMenu.Size = new System.Drawing.Size(128, 52);
            // 
            // reloadToolStripMenuItem
            // 
            this.reloadToolStripMenuItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            this.reloadToolStripMenuItem.Size = new System.Drawing.Size(127, 24);
            this.reloadToolStripMenuItem.Text = "Reload";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitToolStripMenuItem.ForeColor = System.Drawing.Color.Red;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(127, 24);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // cuzDragControl1
            // 
            this.cuzDragControl1.DockSides = false;
            this.cuzDragControl1.DragParent = false;
            this.cuzDragControl1.TargetControl = this.btnMenu;
            // 
            // frmRemotePrinter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1024, 600);
            this.Controls.Add(this.btnMenu);
            this.Controls.Add(this.webView21);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "frmRemotePrinter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmRemotePrinter";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).EndInit();
            this.cuzDropdownMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private DesignUI.CuzUI.CuzButton btnMenu;
        private DesignUI.CuzUI.CuzDropdownMenu cuzDropdownMenu;
        private System.Windows.Forms.ToolStripMenuItem reloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private DesignUI.CuzUI.CuzDragControl cuzDragControl1;
    }
}