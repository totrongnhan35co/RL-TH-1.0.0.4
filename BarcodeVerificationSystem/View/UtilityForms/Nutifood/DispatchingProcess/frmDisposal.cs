using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.Apis.Dispatching;
using BarcodeVerificationSystem.Model.CodeGeneration;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response;
using Newtonsoft.Json;
using BarcodeVerificationSystem.Utils;
using System.Security.Policy;
using BarcodeVerificationSystem.Services.Dispatching;

namespace BarcodeVerificationSystem.View.UtilityForms.DispatchingProcess
{
    public partial class frmDisposal : Form
    {
        //private Panel listContainer;
        //private FlowLayoutPanel flowProducts;
        //private Button btnAddProduct;
        //private Button btnDispose;
        //private List<RequestDisposal> disposedItems = new List<RequestDisposal>();
        //private string notes = string.Empty;
        //TextBox txtNotes = new TextBox();
        TextBox txtNotes = new TextBox();
        DispatchingService dispatchingService = new DispatchingService();

        public frmDisposal()
        {
            InitializeComponent();
            InitializeLayout();
            RegisterEvents();
        }

        private void InitializeLayout()
        {

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
        }

        private async void BtnDispose_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                disposedItems.ForEach(item => { item.destroy_date = now; item.notes = notes; });
                var request = new RequestListDisposal
                {
                    qrcodes = disposedItems
                };
                string json = JsonConvert.SerializeObject(request);
                Console.WriteLine(json);

                //var ResponseDisposal = await apiService.PostApiDataAsync<ResponseDisposal>(DisposedCodesUrl, request);
                var ResponseDisposal = await dispatchingService.PostDestroyDataAsync(request);

                if (ResponseDisposal.is_success)
                {
                    NumberOfSuccess.Text = ResponseDisposal.destroyed_qty.ToString();
                    NumberOfFailed.Text = (disposedItems.Count - ResponseDisposal.destroyed_qty).ToString();

                    CustomMessageBox.Show("Hủy mã thành công!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    ProjectLogger.WriteError($"Error occurred in PostDestroyDataAsync" + ResponseDisposal.message + " Payload:" + json.ToString());
                    CustomMessageBox.Show(ResponseDisposal.message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

                var dispatching = new Dispatching(
                    Shared.Settings.DispatchingOrderPayload.payload.shipto_code,
                    Shared.Settings.DispatchingOrderPayload.payload.shipment,
                    Shared.Settings.DispatchingOrderPayload.payload.shipto_name);

                disposedItems.Add(new RequestDisposal
                {
                    qr_code = qrCode,
                    unique_code = dispatching.GetHumanReadableCode(qrCode),
                    scan_date = DateTime.Now,
                });
            }
            catch (Exception)
            {
                CustomMessageBox.Show("Hủy mã không thành công!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
