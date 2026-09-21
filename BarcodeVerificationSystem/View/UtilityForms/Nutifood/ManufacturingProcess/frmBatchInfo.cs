using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using BarcodeVerificationSystem.Utils;

namespace BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess
{
    public partial class frmBatchInfo : Form
    {
        List<ResponseProcessOrder.BatchInfo> batchInfo;
        public int? SelectedBatchIndex { get; private set; } = 0;

        public frmBatchInfo(List<ResponseProcessOrder.BatchInfo> BatchInfo, int selectedIndex)
        {
            InitializeComponent();
            batchInfo = BatchInfo;
            SelectedBatchIndex = selectedIndex;
            UIControlsFuncs.VisibleControl(Shared.Settings.FactoryCode == "1260", CreateNewLOT);
            LOTNumberCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            InitControls();
            InitEvents();
        }

        private void InitEvents()
        {
            LOTNumberCombo.SelectedIndexChanged += AdjustData;
        }

        private void AdjustData(object sender, EventArgs e)
        {
            if(sender == LOTNumberCombo)
            {
                ChangeBatchInfo(LOTNumberCombo.SelectedIndex);
            }
        }

        private void InitControls()
        {
            this.Text = "Chi tiết Số LOT";
            this.StartPosition = FormStartPosition.CenterScreen;
            if (batchInfo == null || batchInfo.Count == 0) return;

            InitItemsCombo();
        }
        private void InitItemsCombo()
        {
            LOTNumberCombo.Items.Clear();
            LOTNumberCombo.Items.AddRange(batchInfo.Select(x => x.batch).ToArray() ?? Array.Empty<object>());
            LOTNumberCombo.SelectedIndex = (LOTNumberCombo.Items.Count > SelectedBatchIndex ? SelectedBatchIndex : 0) ?? 0;
            ChangeBatchInfo(LOTNumberCombo.SelectedIndex);
        }

        private void ChangeBatchInfo(int selectedIndex)
        {
            MaufDate.Text = batchInfo[selectedIndex].mauf_date.ToString("yyyy/MM/dd"); // Shared.Settings.LOTFormatDate
            ExpireDate.Text = batchInfo[selectedIndex].expired_date.ToString("yyyy/MM/dd");
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; // closes the form
        }

        private void Choose(object sender, EventArgs e)
        {
            SelectedBatchIndex = LOTNumberCombo.SelectedIndex;
            this.DialogResult = DialogResult.OK; // closes the for
        }

        private void AddNew(object sender, EventArgs e)
        {

            using (var frmBatch = new frmBatchInfoManual())
            {
                if (frmBatch.ShowDialog() == DialogResult.OK)
                {
                    Shared.Settings.ManufacturingListPO.process_orders[Shared.Settings.SelectedPOIndex].batch_info.Add(frmBatch.batchInfo);
                }
            }

            batchInfo = Shared.Settings?.ManufacturingListPO?.process_orders[Shared.Settings.SelectedPOIndex]?.batch_info;
            InitItemsCombo();
        }
    }

}
