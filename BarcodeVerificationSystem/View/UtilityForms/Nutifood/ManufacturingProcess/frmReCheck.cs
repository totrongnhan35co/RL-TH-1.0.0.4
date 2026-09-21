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
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using System.Collections.Generic;

namespace BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess
{
    public partial class frmReCheck : Form
    {
        private List<Model.Payload.ManufacturingPayload.Request.Qrcode> recheckedItems = new List<Model.Payload.ManufacturingPayload.Request.Qrcode>();
        TextBox txtNotes = new TextBox();
        ManufacturingService manufaturingService = new ManufacturingService();

        public frmReCheck()
        {
            InitializeComponent();
            InitializeLayout();
            RegisterEvents();
            LoadBatchRecheckDisplay();
        }

        private void InitializeLayout()
        {

        }

        private void LoadBatchRecheckDisplay()
        {
            UpdateBatchRecheckDisplay();
        }

        private void UpdateBatchRecheckDisplay()
        {
            if (lblBatchRecheckInfo == null) return;

            try
            {
                if (Shared.CurrentJob != null && 
                    Shared.CurrentJob.BatchRecheckCounts != null && 
                    Shared.CurrentJob.BatchRecheckCounts.Count > 0)
                {
                    var batchInfoList = new List<string>();
                    foreach (var batchRecheck in Shared.CurrentJob.BatchRecheckCounts)
                    {
                        batchInfoList.Add($"Đã kiểm tra lại: {batchRecheck.RecheckCount} mã"); // Batch: {batchRecheck.Batch} - 
                    }
                    lblBatchRecheckInfo.Text = string.Join(" | ", batchInfoList);
                }
                else
                {
                    lblBatchRecheckInfo.Text = "Chưa có mã nào được kiểm tra lại";
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"Error updating batch recheck display: {ex.Message}");
                lblBatchRecheckInfo.Text = "Chưa có mã nào được kiểm tra lại";
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
            btnReCheck.Click += BtnReCheck_Click;
            //notesInput.TextChanged += NotesInput_TextChanged;
            btnAddManual.Click += AddManualBarcode;
            txtManualInput.KeyDown += TxtManualInput_KeyDown;
        }

        private async void BtnReCheck_Click(object sender, EventArgs e)
        {
            int successCount = 0;
            int failedCount = 0;
            string errorMessages = "";

            try
            {
                if (recheckedItems == null || recheckedItems.Count == 0)
                {
                    CustomMessageBox.Show("Không có mã nào để kiểm tra lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if CurrentJob is available
                if (Shared.CurrentJob == null)
                {
                    CustomMessageBox.Show("Không tìm thấy thông tin job!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Process each item individually
                for (int i = 0; i < recheckedItems.Count; i++)
                {
                    var item = recheckedItems[i];
                    try
                    {
                        RequestChecked request = null;

                        // Build request based on job mode
                        if (Shared.CurrentJob.IsProcessOrderMode)
                        {
                            var payload = Shared.CurrentJob.ProcessOrderItem;
                            if (payload == null)
                            {
                                failedCount++;
                                errorMessages += $"Item {i + 1}: Không tìm thấy thông tin Process Order\n";
                                continue;
                            }

                            // Get batch index safely
                            int batchIndex = Shared.CurrentJob.SelectedBatchIndex >= 0 && 
                                           Shared.CurrentJob.SelectedBatchIndex < (payload.batch_info?.Count ?? 0)
                                ? Shared.CurrentJob.SelectedBatchIndex 
                                : 0;

                            var batchInfo = payload.batch_info != null && payload.batch_info.Count > 0 
                                ? payload.batch_info[batchIndex] 
                                : null;

                            request = new RequestChecked
                            {
                                index_qr_code = i + 1,
                                qr_code = item.qr_code,
                                process_order = payload.process_order,
                                material_number = payload.material_number,
                                check_date = item.scan_date.ToString("yyyy-MM-dd HH:mm:ss"),
                                status = "valid", // Assuming recheck means valid status
                                print_type = "process_order",
                                batch = batchInfo?.batch ?? "",
                                mauf_date = batchInfo?.mauf_date ?? DateTime.Now,
                                expired_date = batchInfo?.expired_date ?? DateTime.Now,
                            };
                        }
                        else if (Shared.CurrentJob.IsReservationMode)
                        {
                            var payload = Shared.CurrentJob.ReservationItem;
                            if (payload == null)
                            {
                                failedCount++;
                                errorMessages += $"Item {i + 1}: Không tìm thấy thông tin Reservation\n";
                                continue;
                            }

                            request = new RequestChecked
                            {
                                index_qr_code = i + 1,
                                qr_code = item.qr_code,
                                material_number = payload.material_number,
                                check_date = item.scan_date.ToString("yyyy-MM-dd HH:mm:ss"),
                                status = "valid", // Assuming recheck means valid status
                                material_doc = Shared.CurrentJob.Reservation?.material_doc ?? "",
                                print_type = "reservation",
                                batch = payload.batch,
                                mauf_date = payload.mauf_date,
                                expired_date = payload.expired_date,
                            };
                        }
                        else
                        {
                            failedCount++;
                            errorMessages += $"Item {i + 1}: Job không phải Process Order hoặc Reservation mode\n";
                            continue;
                        }

                        // Post verified data and wait for response
                        var response = await manufaturingService.PostVerifiedDataAsync(request);

                        if (response.is_success)
                        {
                            successCount++;
                        }
                        else
                        {
                            failedCount++;
                            errorMessages += $"Item {i + 1} ({item.qr_code}): {response.message ?? "Lỗi không xác định"}\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        errorMessages += $"Item {i + 1} ({item.qr_code}): {ex.Message}\n";
                        ProjectLogger.WriteError($"Error occurred in PostVerifiedDataAsync for item {i + 1}: " + ex.Message);
                    }
                }

                // Update UI with counts
                NumberOfSuccess.Text = successCount.ToString();
                NumberOfFailed.Text = failedCount.ToString();

                // Store recheck count and batch info to CurrentJob model
                if (successCount > 0 && Shared.CurrentJob != null)
                {
                    string currentBatch = null;

                    if (Shared.CurrentJob.IsProcessOrderMode && 
                        Shared.CurrentJob.ProcessOrderItem != null && 
                        Shared.Settings.SelectedBatchIndex >= 0 &&
                        Shared.Settings.SelectedBatchIndex < Shared.CurrentJob.ProcessOrderItem.batch_info.Count)
                    {
                        currentBatch = Shared.CurrentJob.ProcessOrderItem.batch_info[Shared.Settings.SelectedBatchIndex].batch;
                    }
                    else if (Shared.CurrentJob.IsReservationMode && 
                             Shared.CurrentJob.ReservationItem != null)
                    {
                        currentBatch = Shared.CurrentJob.ReservationItem.batch;
                    }

                    if (!string.IsNullOrEmpty(currentBatch))
                    {
                        // Initialize list if null
                        if (Shared.CurrentJob.BatchRecheckCounts == null)
                        {
                            Shared.CurrentJob.BatchRecheckCounts = new List<BatchRecheckModel>();
                        }

                        // Find existing batch recheck entry or create new one
                        var existingBatchRecheck = Shared.CurrentJob.BatchRecheckCounts.FirstOrDefault(b => b.Batch == currentBatch);
                        if (existingBatchRecheck != null)
                        {
                            // Update existing entry
                            existingBatchRecheck.RecheckCount += successCount;
                        }
                        else
                        {
                            // Add new entry
                            Shared.CurrentJob.BatchRecheckCounts.Add(new BatchRecheckModel(currentBatch, successCount));
                        }

                        // Save the job file
                        Shared.CurrentJob.SaveFile();

                        // Update UI to display batch recheck information
                        UpdateBatchRecheckDisplay();
                    }
                }

                // Show result message
                string message = $"Đã kiểm tra lại {recheckedItems.Count} mã.\n";
                message += $"Thành công: {successCount}\n";
                message += $"Thất bại: {failedCount}";

                if (failedCount > 0 && !string.IsNullOrEmpty(errorMessages))
                {
                    message += $"\n\nChi tiết lỗi:\n{errorMessages}";
                }

                MessageBoxIcon icon = failedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
                CustomMessageBox.Show(message, "Kết quả kiểm tra lại", MessageBoxButtons.OK, icon);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"Error occurred in BtnReCheck_Click: " + ex.Message);
                CustomMessageBox.Show("Không thể kiểm tra lại mã! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                recheckedItems.Clear();
                flowProducts.Controls.Clear();
                notes = string.Empty;
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

                // 🔴 Check if recheckedItems already contains this qrCode
                if (recheckedItems.Any(d => d.qr_code.Equals(qrCode, StringComparison.OrdinalIgnoreCase)))
                    return;

                var item = new ProductItem(qrCode);
                item.OnDeleteClicked += delegate
                {
                    RemoveItem(item);
                };

                AddItem(item);

                recheckedItems.Add(new Model.Payload.ManufacturingPayload.Request.Qrcode
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
                string qrCode = Manufacturing.Url + inputText;

                // Check if recheckedItems already contains this qrCode
                if (recheckedItems.Any(d => d.qr_code.Equals(qrCode, StringComparison.OrdinalIgnoreCase)))
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

                recheckedItems.Add(new Model.Payload.ManufacturingPayload.Request.Qrcode
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
                        recheckedItems.RemoveAll(x => x.qr_code == ((ProductItem)item).ProductName);
                        flowProducts.Controls.Remove(item);
                        item.Dispose();
                    }));
                }
                else
                {
                    recheckedItems.RemoveAll(x => x.qr_code == ((ProductItem)item).ProductName);
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
