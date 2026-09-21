using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.CodeGeneration;
using BarcodeVerificationSystem.Services.Manufacturing;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.View.CustomDialogs;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request;
using System.Collections.Generic;

namespace BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess
{
    public partial class frmDisposal : Form
    {
        private List<Qrcode> disposedItems = new List<Qrcode>();
        TextBox txtNotes = new TextBox();
        ManufacturingService manufaturingService = new ManufacturingService();

        public frmDisposal()
        {
            InitializeComponent();
            InitializeLayout();
            RegisterEvents();
            LoadBatchDisposalDisplay();
        }

        private void InitializeLayout()
        {

        }

        private void LoadBatchDisposalDisplay()
        {
            UpdateBatchDisposalDisplay();
        }

        private void UpdateBatchDisposalDisplay()
        {
            if (lblBatchDisposalInfo == null) return;

            try
            {
                if (Shared.CurrentJob != null && Shared.CurrentJob.IsProcessOrderMode && 
                    Shared.CurrentJob.ProcessOrderItem != null && 
                    Shared.CurrentJob.BatchDisposalCounts != null && 
                    Shared.CurrentJob.BatchDisposalCounts.Count > 0)
                {
                    var batchInfoList = new List<string>();
                    foreach (var batchDisposal in Shared.CurrentJob.BatchDisposalCounts)
                    {
                        batchInfoList.Add($"Batch: {batchDisposal.Batch} - Đã hủy: {batchDisposal.DisposalCount} mã");
                    }
                    lblBatchDisposalInfo.Text = string.Join(" | ", batchInfoList);
                }
                else
                {
                    lblBatchDisposalInfo.Text = "Chưa có mã nào được hủy";
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"Error updating batch disposal display: {ex.Message}");
                lblBatchDisposalInfo.Text = "Chưa có mã nào được hủy";
            }
        }
        private void NotesInput_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null)
            {
                notes = txt.Text.Trim();
                //CustomMessageBox.Show("Note updated: " + notes, "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Optional: store the note or trigger other logic here
            }
        }

        private void RegisterEvents()
        {
            btnAddProduct.Click += AddSampleProduct;
            Shared.OnSerialDeviceReadDataChange += AddBarcodes;
            btnDispose.Click += BtnDispose_Click;
            notesInput.TextChanged += NotesInput_TextChanged;
            btnAddManual.Click += AddManualBarcode;
            txtManualInput.KeyDown += TxtManualInput_KeyDown;
        }

        private async void BtnDispose_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;

                disposedItems.ForEach(item => { item.notes = notes == "" ? "Không có ghi chú!" : notes; });
                var request = new RequestDestroyCodes
                {
                    qrcodes = disposedItems,
                    notes = notes == "" ? "Không có ghi chú!" : notes,
                };



                //var ResponseDisposal = await apiService.PostApiDataAsync<ResponseDisposal>(DisposedCodesUrl, request);
                var ResponseDisposal = await manufaturingService.PostDestroyCodesAsync(request);

                if (ResponseDisposal.is_success)
                {
                    NumberOfSuccess.Text = ResponseDisposal.destroyed_qty.ToString();
                    NumberOfFailed.Text = (disposedItems.Count - ResponseDisposal.destroyed_qty).ToString();

                    // Store disposal count and batch info to CurrentJob model
                    if (Shared.CurrentJob != null && Shared.CurrentJob.IsProcessOrderMode && 
                        Shared.CurrentJob.ProcessOrderItem != null && 
                        Shared.Settings.SelectedBatchIndex >= 0 &&
                        Shared.Settings.SelectedBatchIndex < Shared.CurrentJob.ProcessOrderItem.batch_info.Count)
                    {
                        string currentBatch = Shared.CurrentJob.ProcessOrderItem.batch_info[Shared.Settings.SelectedBatchIndex].batch;
                        
                        // Initialize list if null
                        if (Shared.CurrentJob.BatchDisposalCounts == null)
                        {
                            Shared.CurrentJob.BatchDisposalCounts = new List<BatchDisposalModel>();
                        }

                        // Find existing batch disposal entry or create new one
                        var existingBatchDisposal = Shared.CurrentJob.BatchDisposalCounts.FirstOrDefault(b => b.Batch == currentBatch);
                        if (existingBatchDisposal != null)
                        {
                            // Update existing entry
                            existingBatchDisposal.DisposalCount += ResponseDisposal.destroyed_qty;
                        }
                        else
                        {
                            // Add new entry
                            Shared.CurrentJob.BatchDisposalCounts.Add(new BatchDisposalModel(currentBatch, ResponseDisposal.destroyed_qty));
                        }

                        // Save the job file
                        Shared.CurrentJob.SaveFile();

                        // Update UI to display batch disposal information
                        UpdateBatchDisposalDisplay();
                    }

                    CustomMessageBox.Show("Hủy mã thành công!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    ProjectLogger.WriteError($"Error occurred in PostDestroyDataAsync" + ResponseDisposal.message);
                    CustomMessageBox.Show(ResponseDisposal.message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                notes = notesInput.Text = string.Empty; // Clear notes after disposal
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"Error occurred in PostDestroyDataAsync " + ex.Message);
                CustomMessageBox.Show("Không thể hủy mã!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                disposedItems.Clear();
                flowProducts.Controls.Clear();
                notes = string.Empty; // Clear notes after disposal
                txtNotes.Clear();
                txtManualInput.Text = "";
            }
        }

        private void AddSampleProduct(object sender, EventArgs e)
        {
            var demoModel = new DetectModel { Text = "Demo Product " + DateTime.Now.ToLongTimeString() };
            AddBarcodes(demoModel, EventArgs.Empty);
        }

        private void AddBarcodes(object sender, EventArgs e)
        {
            if (Shared.OperStatus == OperationStatus.Running && Shared.OperStatus == OperationStatus.Processing)
                return;

            try
            {
                var model = sender as DetectModel;
                if (model == null || string.IsNullOrWhiteSpace(model.Text)) return;

                string qrCode = model.Text.Trim();

                // 🔴 Check if disposedItems already contains this qrCode
                if (disposedItems.Any(d => d.qr_code.Equals(qrCode, StringComparison.OrdinalIgnoreCase)))
                    return;

                var item = new ProductItem(qrCode);
                item.OnDeleteClicked += delegate
                {
                    RemoveItem(item);
                };

                AddItem(item);

                disposedItems.Add(new Qrcode
                {
                    qr_code = qrCode,
                    scan_date = DateTime.Now,
                });
            }
            catch (Exception)
            {
                CustomMessageBox.Show("Hủy mã không thành công!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddManualBarcode(object sender, EventArgs e)
        {
            try
            {
                string inputText = txtManualInput.Text.Trim();
                if (string.IsNullOrWhiteSpace(inputText))
                {
                    CustomMessageBox.Show("Vui lòng nhập mã!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Prepend Manufacturing.Url to the text
                string qrCode = Shared.Settings.ApiDomain + inputText;

                // Check if disposedItems already contains this qrCode
                if (disposedItems.Any(d => d.qr_code.Equals(qrCode, StringComparison.OrdinalIgnoreCase)))
                {
                    CustomMessageBox.Show("Mã này đã tồn tại trong danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtManualInput.Text = "";
                    txtManualInput.Focus();
                    return;
                }

                var item = new ProductItem(qrCode);
                item.OnDeleteClicked += delegate
                {
                    RemoveItem(item);
                };

                AddItem(item);

                disposedItems.Add(new Qrcode
                {
                    qr_code = qrCode,
                    scan_date = DateTime.Now,
                });

                // Clear input after adding
                txtManualInput.Text = "";
                txtManualInput.Focus();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Không thể thêm mã! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtManualInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddManualBarcode(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void AddItem(Control item)
        {
            if (flowProducts.InvokeRequired)
            {
                flowProducts.Invoke(new MethodInvoker(delegate
                {
                    flowProducts.Controls.Add(item);
                }));
            }
            else
            {
                flowProducts.Controls.Add(item);
            }
        }

        private void RemoveItem(Control item)
        {
            try
            {
                if (flowProducts.InvokeRequired)
                {
                    flowProducts.Invoke(new MethodInvoker(delegate
                    {
                        disposedItems.RemoveAll(x => x.qr_code == ((ProductItem)item).ProductName);
                        flowProducts.Controls.Remove(item);
                        item.Dispose();
                    }));
                }
                else
                {
                    disposedItems.RemoveAll(x => x.qr_code == ((ProductItem)item).ProductName);
                    flowProducts.Controls.Remove(item);
                    item.Dispose();
                }
            }
            catch (Exception)
            {
            }
          
        }

        // Nested user control
        public class ProductItem : UserControl
        {
            public event EventHandler OnDeleteClicked;

            private Label lblName;
            private Button btnDelete;

            public string ProductName
            {
                get { return lblName.Text; }
                set { lblName.Text = value; }
            }

            public ProductItem(string productName)
            {
                this.Size = new Size(890, 50);
                this.Margin = new Padding(5);
                this.BackColor = Color.White;
                this.BorderStyle = BorderStyle.FixedSingle;

                lblName = new Label();
                lblName.Text = productName;
                lblName.Font = new Font("Segoe UI", 9);
                lblName.Location = new Point(10, 10);
                lblName.Size = new Size(700, 30);
                lblName.TextAlign = ContentAlignment.MiddleLeft;

                btnDelete = new Button();
                btnDelete.Text = "Xóa";
                btnDelete.Size = new Size(80, 30);
                btnDelete.Location = new Point(790, 10);
                btnDelete.Click += delegate
                {
                    if (OnDeleteClicked != null)
                        OnDeleteClicked(this, EventArgs.Empty);
                };

                this.Controls.Add(lblName);
                this.Controls.Add(btnDelete);
            }
        }
    }

}
