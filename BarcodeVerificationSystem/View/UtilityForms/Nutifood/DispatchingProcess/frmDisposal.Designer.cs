using BarcodeVerificationSystem.Model.Payload.DispatchingPayload;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View.UtilityForms.DispatchingProcess
{
    public partial class frmDisposal : Form
    {
        private System.ComponentModel.IContainer components = null;

        //private Panel listContainer;
        //private FlowLayoutPanel flowProducts;
        //private TextBox txtNotes;
        //private Button btnAddProduct;
        //private Button btnDispose;
        //private Label lblListTitle;
        //private Label lblNotes;

 

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.listContainer = new System.Windows.Forms.Panel();
            this.flowProducts = new System.Windows.Forms.FlowLayoutPanel();
            this.lblNotes = new System.Windows.Forms.Label();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.btnAddProduct = new System.Windows.Forms.Button();
            this.notesInput = new DesignUI.CuzUI.CuzTextBox();
            this.lblDatabaseType = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.NumberOfSuccess = new System.Windows.Forms.Label();
            this.NumberOfFailed = new System.Windows.Forms.Label();
            this.btnDispose = new DesignUI.CuzUI.CuzButton();
            this.listContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // listContainer
            // 
            this.listContainer.BackColor = System.Drawing.Color.WhiteSmoke;
            this.listContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listContainer.Controls.Add(this.flowProducts);
            this.listContainer.Location = new System.Drawing.Point(29, 41);
            this.listContainer.Name = "listContainer";
            this.listContainer.Size = new System.Drawing.Size(940, 585);
            this.listContainer.TabIndex = 0;
            // 
            // flowProducts
            // 
            this.flowProducts.AutoScroll = true;
            this.flowProducts.BackColor = System.Drawing.Color.White;
            this.flowProducts.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowProducts.Location = new System.Drawing.Point(10, 10);
            this.flowProducts.Name = "flowProducts";
            this.flowProducts.Padding = new System.Windows.Forms.Padding(5);
            this.flowProducts.Size = new System.Drawing.Size(920, 552);
            this.flowProducts.TabIndex = 0;
            this.flowProducts.WrapContents = false;
            // 
            // lblNotes
            // 
            this.lblNotes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNotes.Location = new System.Drawing.Point(26, 642);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new System.Drawing.Size(69, 20);
            this.lblNotes.TabIndex = 1;
            this.lblNotes.Text = "Ghi chú:";
            // 
            // lblListTitle
            // 
            this.lblListTitle.BackColor = System.Drawing.Color.White;
            this.lblListTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.Location = new System.Drawing.Point(12, 9);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(154, 20);
            this.lblListTitle.TabIndex = 1;
            this.lblListTitle.Text = "Danh sách mã hủy";
            // 
            // btnAddProduct
            // 
            this.btnAddProduct.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddProduct.Location = new System.Drawing.Point(20, 20);
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.Size = new System.Drawing.Size(150, 35);
            this.btnAddProduct.TabIndex = 0;
            this.btnAddProduct.Text = "Add Product";
            // 
            // notesInput
            // 
            this.notesInput._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.notesInput._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.notesInput.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.notesInput.BackColor = System.Drawing.Color.White;
            this.notesInput.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.notesInput.BorderFocusColor = System.Drawing.Color.Silver;
            this.notesInput.BorderRadius = 6;
            this.notesInput.BorderSize = 1;
            this.notesInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.notesInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.notesInput.Location = new System.Drawing.Point(102, 642);
            this.notesInput.Margin = new System.Windows.Forms.Padding(4);
            this.notesInput.MinimumSize = new System.Drawing.Size(0, 70);
            this.notesInput.Multiline = false;
            this.notesInput.Name = "notesInput";
            this.notesInput.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.notesInput.PasswordChar = false;
            this.notesInput.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.notesInput.PlaceholderText = "";
            this.notesInput.ReadOnly = false;
            this.notesInput.Size = new System.Drawing.Size(867, 70);
            this.notesInput.TabIndex = 130;
            this.notesInput.UnderlinedStyle = false;
            // 
            // lblDatabaseType
            // 
            this.lblDatabaseType.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblDatabaseType.Location = new System.Drawing.Point(25, 729);
            this.lblDatabaseType.Name = "lblDatabaseType";
            this.lblDatabaseType.Size = new System.Drawing.Size(208, 25);
            this.lblDatabaseType.TabIndex = 131;
            this.lblDatabaseType.Text = "Số lượng mã hủy thành công:";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label1.Location = new System.Drawing.Point(348, 729);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(188, 25);
            this.label1.TabIndex = 132;
            this.label1.Text = "Số lượng mã hủy thất bại:";
            // 
            // NumberOfSuccess
            // 
            this.NumberOfSuccess.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.NumberOfSuccess.ForeColor = System.Drawing.Color.LimeGreen;
            this.NumberOfSuccess.Location = new System.Drawing.Point(239, 729);
            this.NumberOfSuccess.Name = "NumberOfSuccess";
            this.NumberOfSuccess.Size = new System.Drawing.Size(57, 25);
            this.NumberOfSuccess.TabIndex = 133;
            this.NumberOfSuccess.Text = "0";
            // 
            // NumberOfFailed
            // 
            this.NumberOfFailed.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.NumberOfFailed.ForeColor = System.Drawing.Color.Red;
            this.NumberOfFailed.Location = new System.Drawing.Point(542, 729);
            this.NumberOfFailed.Name = "NumberOfFailed";
            this.NumberOfFailed.Size = new System.Drawing.Size(57, 25);
            this.NumberOfFailed.TabIndex = 134;
            this.NumberOfFailed.Text = "0";
            // 
            // btnDispose
            // 
            this.btnDispose._BorderColor = System.Drawing.Color.Silver;
            this.btnDispose._BorderRadius = 15;
            this.btnDispose._BorderSize = 0;
            this.btnDispose._GradientsButton = false;
            this.btnDispose._Text = "Hủy mã";
            this.btnDispose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnDispose.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnDispose.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnDispose.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnDispose.FlatAppearance.BorderSize = 0;
            this.btnDispose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDispose.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.btnDispose.ForeColor = System.Drawing.Color.White;
            this.btnDispose.Location = new System.Drawing.Point(817, 729);
            this.btnDispose.Name = "btnDispose";
            this.btnDispose.Size = new System.Drawing.Size(152, 59);
            this.btnDispose.TabIndex = 135;
            this.btnDispose.Text = "Hủy mã";
            this.btnDispose.TextColor = System.Drawing.Color.White;
            this.btnDispose.UseVisualStyleBackColor = false;
            // 
            // frmDisposal
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1000, 800);
            this.Controls.Add(this.btnDispose);
            this.Controls.Add(this.NumberOfFailed);
            this.Controls.Add(this.NumberOfSuccess);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblDatabaseType);
            this.Controls.Add(this.notesInput);
            this.Controls.Add(this.lblNotes);
            this.Controls.Add(this.listContainer);
            this.Controls.Add(this.lblListTitle);
            this.Name = "frmDisposal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hủy mã";
            this.listContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Panel listContainer;
        private FlowLayoutPanel flowProducts;
        private Button btnAddProduct;
        private List<RequestDisposal> disposedItems = new List<RequestDisposal>();
        private string notes = string.Empty;
        private Label lblNotes;
        private Label lblListTitle;
        private DesignUI.CuzUI.CuzTextBox notesInput;
        private Label lblDatabaseType;
        private Label label1;
        private Label NumberOfSuccess;
        private Label NumberOfFailed;
        private DesignUI.CuzUI.CuzButton btnDispose;
    }
}
