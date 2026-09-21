using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload;
using CommonVariable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View.UtilityForms.DispatchingProcess
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
            // Text change updates the job model in memory
            EditMaterialNumber.TextChanged += OnTextChanged_UpdateModel;
            EditWmsNumber.TextChanged += OnTextChanged_UpdateModel;
            EditWavekey.TextChanged += OnTextChanged_UpdateModel;

            // Buttons handle actions
            btnSave.Click += OnSave_Click;
            btnClose.Click += OnClose_Click;
        }

        private void InitControls()
        {
            var payload = _jobModel.DispatchingOrderPayload.payload;

            EditWmsNumber.Text = payload?.wms_number ?? string.Empty;
            EditWavekey.Text = payload?.wave_key ?? string.Empty;

            if (_jobModel.SelectedMaterialIndex >= 0 &&
                _jobModel.SelectedMaterialIndex < payload?.items?.Count)
            {
                EditMaterialNumber.Text = payload.items[_jobModel.SelectedMaterialIndex].material_number ?? string.Empty;
            }
            else
            {
                EditMaterialNumber.Text = string.Empty;
            }
        }

        private void OnTextChanged_UpdateModel(object sender, EventArgs e)
        {
            var payload = _jobModel.DispatchingOrderPayload.payload;
            if (payload == null) return;

            payload.wms_number = EditWmsNumber.Text.Trim();
            payload.wave_key = EditWavekey.Text.Trim();

            if (_jobModel.SelectedMaterialIndex >= 0 &&
                _jobModel.SelectedMaterialIndex < payload.items.Count)
            {
                payload.items[_jobModel.SelectedMaterialIndex].material_number = EditMaterialNumber.Text.Trim();
            }
        }

        private void OnSave_Click(object sender, EventArgs e)
        {
            _jobModel.SaveFile();
            Close();
        }

        private void OnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
