namespace BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing
{
    partial class frmLoadingUi
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                if (loadingSpinner != null)
                    loadingSpinner.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.loadingSpinner = new BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing.LoadingSpinner();
            this.pnlMessageBox = new BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI.Manufacturing.RoundedPanel();
            this.lblMessage = new System.Windows.Forms.Label();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlTopAccent = new System.Windows.Forms.Panel();
            this.fadeTimer = new System.Windows.Forms.Timer(this.components);
            this.pnlCard.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.pnlMessageBox.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlCard
            // 
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.Controls.Add(this.pnlBody);
            this.pnlCard.Controls.Add(this.pnlHeader);
            this.pnlCard.Controls.Add(this.pnlTopAccent);
            this.pnlCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCard.Location = new System.Drawing.Point(4, 4);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(452, 272);
            this.pnlCard.TabIndex = 0;
            // 
            // pnlBody
            // 
            this.pnlBody.BackColor = System.Drawing.Color.White;
            this.pnlBody.Controls.Add(this.loadingSpinner);
            this.pnlBody.Controls.Add(this.pnlMessageBox);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 64);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(452, 208);
            this.pnlBody.TabIndex = 2;
            // 
            // loadingSpinner
            // 
            this.loadingSpinner.BackColor = System.Drawing.Color.White;
            this.loadingSpinner.LineWidth = 8;
            this.loadingSpinner.Location = new System.Drawing.Point(183, 20);
            this.loadingSpinner.Name = "loadingSpinner";
            this.loadingSpinner.RotationStep = 9;
            this.loadingSpinner.Size = new System.Drawing.Size(86, 86);
            this.loadingSpinner.SpinnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.loadingSpinner.TabIndex = 0;
            this.loadingSpinner.TimerInterval = 28;
            this.loadingSpinner.TrackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(230)))), ((int)(((byte)(242)))));
            // 
            // pnlMessageBox
            // 
            this.pnlMessageBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(247)))), ((int)(((byte)(255)))));
            this.pnlMessageBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(205)))), ((int)(((byte)(240)))));
            this.pnlMessageBox.BorderRadius = 12;
            this.pnlMessageBox.BorderSize = 1;
            this.pnlMessageBox.Controls.Add(this.lblMessage);
            this.pnlMessageBox.Location = new System.Drawing.Point(26, 124);
            this.pnlMessageBox.Name = "pnlMessageBox";
            this.pnlMessageBox.Size = new System.Drawing.Size(400, 64);
            this.pnlMessageBox.TabIndex = 1;
            // 
            // lblMessage
            // 
            this.lblMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMessage.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.ForeColor = System.Drawing.Color.Black;
            this.lblMessage.Location = new System.Drawing.Point(0, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Padding = new System.Windows.Forms.Padding(14, 4, 14, 4);
            this.lblMessage.Size = new System.Drawing.Size(400, 64);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "Vui lòng chờ trong giây lát...";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 6);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(452, 58);
            this.pnlHeader.TabIndex = 1;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(18, 0, 18, 0);
            this.lblTitle.Size = new System.Drawing.Size(452, 58);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "ĐANG XỬ LÝ";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlTopAccent
            // 
            this.pnlTopAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(150)))), ((int)(((byte)(235)))));
            this.pnlTopAccent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopAccent.Location = new System.Drawing.Point(0, 0);
            this.pnlTopAccent.Name = "pnlTopAccent";
            this.pnlTopAccent.Size = new System.Drawing.Size(452, 6);
            this.pnlTopAccent.TabIndex = 0;
            // 
            // fadeTimer
            // 
            this.fadeTimer.Interval = 18;
            this.fadeTimer.Tick += new System.EventHandler(this.fadeTimer_Tick);
            // 
            // frmLoadingUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(67)))), ((int)(((byte)(137)))));
            this.ClientSize = new System.Drawing.Size(460, 280);
            this.ControlBox = false;
            this.Controls.Add(this.pnlCard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLoadingUi";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmLoadingUi_FormClosed);
            this.Shown += new System.EventHandler(this.frmLoadingUi_Shown);
            this.LocationChanged += new System.EventHandler(this.frmLoadingUi_LocationChanged);
            this.Resize += new System.EventHandler(this.frmLoadingUi_Resize);
            this.pnlCard.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlMessageBox.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Panel pnlBody;
        private LoadingSpinner loadingSpinner;
        private RoundedPanel pnlMessageBox;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlTopAccent;
        private System.Windows.Forms.Timer fadeTimer;
    }
}
