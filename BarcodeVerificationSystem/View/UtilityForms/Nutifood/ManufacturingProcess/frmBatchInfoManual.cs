using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using BarcodeVerificationSystem.View.CustomDialogs;

namespace BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess
{
    public partial class frmBatchInfoManual : Form
    {
        public ResponseProcessOrder.BatchInfo batchInfo;
        public int? SelectedBatchIndex { get; private set; } = null;

        public frmBatchInfoManual()
        {
            InitializeComponent();
            InitControls();
            InitEvents();
        }

        private void InitEvents()
        {
            batch.TextChanged += AdjustData;
        }

        private void AdjustData(object sender, EventArgs e)
        {
        }

        private void InitControls()
        {
            this.Text = "Chi tiết Số LOT";
            this.StartPosition = FormStartPosition.CenterScreen;
            MaufDatePicker.Format = DateTimePickerFormat.Custom;
            MaufDatePicker.CustomFormat = "yyyy/MM/dd";

            ExpiredDatePicker.Format = DateTimePickerFormat.Custom;
            ExpiredDatePicker.CustomFormat = "yyyy/MM/dd";
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; // closes the form
        }

        private void AddItem(object sender, EventArgs e)
        {
            var newItem = new ResponseProcessOrder.BatchInfo()
            {
                batch = batch.Text,
                mauf_date = MaufDatePicker.Value,
                expired_date = ExpiredDatePicker.Value,
            };
            batchInfo = newItem;
            this.DialogResult = DialogResult.OK;
            //Shared.Settings.ManufacturingListPO.process_orders[Shared.Settings.SelectedPOIndex].batch_info.Add(newItem);
            //Close();
        }
    }

}
