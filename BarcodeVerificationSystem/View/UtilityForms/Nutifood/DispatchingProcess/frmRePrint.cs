using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Apis.Dispatching;
using BarcodeVerificationSystem.Model.CodeGeneration;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.View.CustomDialogs;
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
    public partial class frmRePrint : Form
    {
        private Panel listContainer;
        private FlowLayoutPanel flowProducts;
        private Button btnAddProduct;
        private Button btnSort;
        private List<QrCode> reprintItems = new List<QrCode>();

        public frmRePrint()
        {
            InitializeComponent();
            InitializeLayout();
            RegisterEvents();
        }

        private void InitializeLayout()
        {
            this.Text = "Product List";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(1000, 800);
            this.BackColor = Color.White;

            // Label-like border panel container
            listContainer = new Panel();
            listContainer.Location = new Point(20, 70);
            listContainer.Size = new Size(940, 700);
            listContainer.BorderStyle = BorderStyle.FixedSingle;
            listContainer.BackColor = Color.WhiteSmoke;

            // Title label (acts like a border label)
            Label lblListTitle = new Label();
            lblListTitle.Text = "Product List";
            lblListTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblListTitle.BackColor = Color.White;
            lblListTitle.Location = new Point(10, 50);
            lblListTitle.Size = new Size(120, 20);

            // Flow panel for product items
            flowProducts = new FlowLayoutPanel();
            flowProducts.Location = new Point(10, 10);
            flowProducts.Size = new Size(920, 680);
            flowProducts.FlowDirection = FlowDirection.TopDown;
            flowProducts.WrapContents = false;
            flowProducts.AutoScroll = true;
            flowProducts.Padding = new Padding(5);
            flowProducts.BackColor = Color.White;

            listContainer.Controls.Add(flowProducts);
            this.Controls.Add(listContainer);
            this.Controls.Add(lblListTitle);

            // Add product button (left top)
            btnAddProduct = new Button();
            btnAddProduct.Text = "Add Product";
            btnAddProduct.Size = new Size(150, 35);
            btnAddProduct.Location = new Point(20, 20);
            btnAddProduct.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            //this.Controls.Add(btnAddProduct);

            // Sort button (right top)
            btnSort = new Button();
            btnSort.Text = "Sort";
            btnSort.Size = new Size(100, 35);
            btnSort.Location = new Point(860, 20);
            btnSort.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.Controls.Add(btnSort);
        }

        private void RegisterEvents()
        {
            btnAddProduct.Click += AddSampleProduct;
            Shared.OnSerialDeviceReadDataChange += AddReprintBarcodes;
            btnSort.Click += BtnSort_Click;
        }

        private async void BtnSort_Click(object sender, EventArgs e)
        {
            var apiService = new ApiService();
             
                try
                {
                    string ReprintCodesUrl = DispatchingApis.GetSendReprintCodesUrl();
                    var request = new RequestListRePrint
                    {
                        qrCodes = reprintItems
                    };

                    bool isPosted = await apiService.PostApiDataAsync(ReprintCodesUrl, request);
                    if (isPosted)
                    {
                        CustomMessageBox.Show("In lại thành công!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception)
                {
                    CustomMessageBox.Show("In lại Thất bại công!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    reprintItems.Clear();
                    flowProducts.Controls.Clear();
                }
            }

        private void AddSampleProduct(object sender, EventArgs e)
        {
            var demoModel = new DetectModel { Text = "Demo Product " + DateTime.Now.ToLongTimeString() };
            AddReprintBarcodes(demoModel, EventArgs.Empty);
        }

        private void AddReprintBarcodes(object sender, EventArgs e)
        {
            if (Shared.OperStatus == OperationStatus.Running && Shared.OperStatus == OperationStatus.Processing)
                return;

            var model = sender as DetectModel;
            if (model == null || string.IsNullOrWhiteSpace(model.Text)) return;

            var item = new ProductItem(model.Text.Trim());
            item.OnDeleteClicked += delegate
            {
                RemoveItem(item);
            };

            AddItem(item);
            var dispatching = new Dispatching(Shared.Settings.DispatchingOrderPayload.payload.shipto_code,
                Shared.Settings.DispatchingOrderPayload.payload.shipment,
                Shared.Settings.DispatchingOrderPayload.payload.shipto_name);

            reprintItems.Add(new QrCode
            {
                qr_code = model.Text.Trim(),
                unique_code = dispatching.GetHumanReadableCode(model.Text.Trim()),            
            });

            //reprintItems.Add(new RequestRePrint
            //{
            //    qr_code = model.Text.Trim(),
            //    unique_code = Dispatching.GetHumanReadableCode(model.Text.Trim()),
            //    notes = "Reprint request",
            //    sync_date = DateTime.Now // nay can chinh voi nut sort or st
            //});
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
            if (flowProducts.InvokeRequired)
            {
                flowProducts.Invoke(new MethodInvoker(delegate
                {
                    flowProducts.Controls.Remove(item);
                    item.Dispose();
                }));
            }
            else
            {
                flowProducts.Controls.Remove(item);
                item.Dispose();
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
                btnDelete.Text = "Delete";
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
