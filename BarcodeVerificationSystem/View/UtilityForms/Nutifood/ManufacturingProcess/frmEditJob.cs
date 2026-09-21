using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response;
using CommonVariable;
using System;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess
{
    public partial class frmEditJob : Form
    {
        private readonly JobModel _jobModel;

        public frmEditJob(JobModel jobModel)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            _jobModel = jobModel ?? throw new ArgumentNullException(nameof(jobModel));

            InitControls();
            InitEvents();
        }

        private void InitEvents()
        {
            btnSave.Click += OnSave_Click;
            btnClose.Click += OnClose_Click;
        }

        private void InitControls()
        {
            if (_jobModel.IsProcessOrderMode) InitPOControls();

            if (_jobModel.IsReservationMode) InitRESControls();

        }

        private void InitRESControls()
        {
            var payload = _jobModel.ReservationItem;
            ProcessOrder.Text = "Mã Material_doc:";
            ProcessOrderInput.Text = _jobModel.Reservation.material_doc ?? string.Empty;
            MaterialNumberInput.Text = payload?.material_number ?? string.Empty;
            LOTNumberInput.Text = payload?.batch ?? string.Empty;
        }

        private void InitPOControls()
        {
            var payload = _jobModel.ProcessOrderItem;
            ProcessOrder.Text = "Mã PO:";
            ProcessOrderInput.Text = payload?.process_order ?? string.Empty;
            MaterialNumberInput.Text = payload?.material_number ?? string.Empty;
            LOTNumberInput.Text = payload?.batch_info[_jobModel.SelectedBatchIndex].batch ?? string.Empty;
        }

        private void OnSave_Click(object sender, EventArgs e)
        {
            if (_jobModel.IsProcessOrderMode) SavePOJob();

            if (_jobModel.IsReservationMode) SaveRESJob();

            _jobModel.SaveFile();
            Close();
        }

        private void SaveRESJob()
        {
            var reservationItem = _jobModel.ReservationItem;
            if (reservationItem == null) return;
            _jobModel.Reservation.material_doc = ProcessOrderInput.Text.Trim();
            reservationItem.material_number = MaterialNumberInput.Text.Trim();
            reservationItem.batch = LOTNumberInput.Text.Trim();
        }

        private void SavePOJob()
        {
            var payload = _jobModel.ProcessOrderItem;
            if (payload == null) return;

            payload.process_order = ProcessOrderInput.Text.Trim();
            payload.material_number = MaterialNumberInput.Text.Trim();
            if (_jobModel.SelectedBatchIndex >= 0 &&
                _jobModel.SelectedBatchIndex < payload.batch_info.Count)
            {
                payload.batch_info[_jobModel.SelectedBatchIndex].batch = LOTNumberInput.Text.Trim();
            }
        }

        private void OnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
