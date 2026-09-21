using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.OtherProjects.THTrueMilkUI
{
    public partial class ucConfigInfo : UserControl
    {
        private DesignUI.CuzUI.CuzTextBox txtMaxConsecutiveDefects;
        private Label lblMaxConsecutiveDefects;

        public ucConfigInfo()
        {
            InitializeComponent();
            InitExtraControls();
        }

        private void InitExtraControls()
        {

        }

        public void LoadData(
            string lineId, string lineName, string status,
            int bufferPrint, int timeLogSave, int timeMonitor, int qrThreshold,
            string modeOperator, string detalMode1, string timeMode2, string errorImagePath,
            int operatingMode = 0, int maxConsecutiveDefects = 0, double reserveFactor = 1.5)
        {
            lblLineId.Text = lineId ?? "";
            lblLineName.Text = lineName ?? "";

            bool isLineActive = !string.IsNullOrEmpty(lineId);
            lblStatusLine.Text = isLineActive ? "  Hoạt động  " : "  Không hoạt động  ";
            lblStatusLine.ForeColor = Color.White;
            lblStatusLine.BackColor = isLineActive ? Color.Green : Color.Red;

            bool isMode1 = operatingMode == 1;
            bool isMode2 = operatingMode == 2;
            bool isMode4 = operatingMode == 4;

            lblModeOperator.Text = operatingMode > 0 ? "Chế độ in: " + operatingMode : "Chế độ in";

            if (isMode4)
            {
                label6.Text = "Hệ số dự phòng";
                txtDetalForMode1.Visible = false;
                txtCoefficient.Location = txtDetalForMode1.Location;
                txtTimeForMode2.Visible = false;
                txtCoefficient.Visible = true;
                txtCoefficient.Text = reserveFactor.ToString();
            }
            else if (isMode2)
            {
                label6.Text = "Chu kì đổi QR Code (Phút)";
                txtTimeForMode2.Location = txtDetalForMode1.Location;
                txtTimeForMode2.Visible = true;
                txtTimeForMode2.Text = timeMode2 ?? "";
                txtDetalForMode1.Visible = false;
                txtCoefficient.Visible = false;
            }
            else
            {
                label6.Text = "Làm mới QR trước 12h đêm";
                txtTimeForMode2.Visible = false;
                txtDetalForMode1.Text = detalMode1 ?? "";
                txtDetalForMode1.Visible = true;
                txtCoefficient.Visible = false;
            }

            txtBufferPrint.Text = bufferPrint.ToString();
            txtTimeLogSave.Text = timeLogSave + " phút";
            txtTimeMornitor.Text = timeMonitor + " phút";
            txtQrThreshold.Text = qrThreshold + "%";
            txtModeOperator.Text = modeOperator ?? "";
            txtPathErrorImage.Text = errorImagePath ?? "";
            txtMaxConsecutiveDefects.Text = maxConsecutiveDefects + " lỗi";
        }
    }
}
