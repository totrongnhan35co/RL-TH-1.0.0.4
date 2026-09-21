using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View.UtilityForms.ManufacturingProcess
{
    public partial class frmReCheck : Form
    {
        private System.ComponentModel.IContainer components = null;

        //private Panel listContainer;
        //private FlowLayoutPanel flowProducts;
        //private TextBox txtNotes;
        //private Button btnAddProduct;
        //private Button btnReCheck;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmReCheck));
            this.listContainer = new System.Windows.Forms.Panel();
            this.flowProducts = new System.Windows.Forms.FlowLayoutPanel();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.btnAddProduct = new System.Windows.Forms.Button();
            this.lblDatabaseType = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.NumberOfSuccess = new System.Windows.Forms.Label();
            this.NumberOfFailed = new System.Windows.Forms.Label();
            this.btnReCheck = new DesignUI.CuzUI.CuzButton();
            this.lblManualInput = new System.Windows.Forms.Label();
            this.txtManualInput = new DesignUI.CuzUI.CuzTextBox();
            this.btnAddManual = new DesignUI.CuzUI.CuzButton();
            this.lblBatchRecheckInfo = new System.Windows.Forms.Label();
            this.listContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // listContainer
            // 
            this.listContainer.BackColor = System.Drawing.Color.WhiteSmoke;
            this.listContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listContainer.Controls.Add(this.flowProducts);
            this.listContainer.Location = new System.Drawing.Point(29, 66);
            this.listContainer.Name = "listContainer";
            this.listContainer.Size = new System.Drawing.Size(940, 560);
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
            // lblListTitle
            // 
            this.lblListTitle.BackColor = System.Drawing.Color.White;
            this.lblListTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.Location = new System.Drawing.Point(12, 34);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(154, 20);
            this.lblListTitle.TabIndex = 1;
            this.lblListTitle.Text = "Danh sách mã đồng bộ";
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
            // lblDatabaseType
            // 
            this.lblDatabaseType.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblDatabaseType.Location = new System.Drawing.Point(25, 762);
            this.lblDatabaseType.Name = "lblDatabaseType";
            this.lblDatabaseType.Size = new System.Drawing.Size(208, 25);
            this.lblDatabaseType.TabIndex = 131;
            this.lblDatabaseType.Text = "Số lượng mã thành công:";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label1.Location = new System.Drawing.Point(348, 762);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(188, 25);
            this.label1.TabIndex = 132;
            this.label1.Text = "Số lượng mã thất bại:";
            // 
            // NumberOfSuccess
            // 
            this.NumberOfSuccess.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.NumberOfSuccess.ForeColor = System.Drawing.Color.LimeGreen;
            this.NumberOfSuccess.Location = new System.Drawing.Point(239, 762);
            this.NumberOfSuccess.Name = "NumberOfSuccess";
            this.NumberOfSuccess.Size = new System.Drawing.Size(57, 25);
            this.NumberOfSuccess.TabIndex = 133;
            this.NumberOfSuccess.Text = "0";
            // 
            // NumberOfFailed
            // 
            this.NumberOfFailed.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.NumberOfFailed.ForeColor = System.Drawing.Color.Red;
            this.NumberOfFailed.Location = new System.Drawing.Point(542, 762);
            this.NumberOfFailed.Name = "NumberOfFailed";
            this.NumberOfFailed.Size = new System.Drawing.Size(57, 25);
            this.NumberOfFailed.TabIndex = 134;
            this.NumberOfFailed.Text = "0";
            // 
            // btnReCheck
            // 
            this.btnReCheck._BorderColor = System.Drawing.Color.Silver;
            this.btnReCheck._BorderRadius = 15;
            this.btnReCheck._BorderSize = 0;
            this.btnReCheck._GradientsButton = false;
            this.btnReCheck._Text = "Đồng bộ mã";
            this.btnReCheck.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnReCheck.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnReCheck.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnReCheck.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnReCheck.FlatAppearance.BorderSize = 0;
            this.btnReCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReCheck.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.btnReCheck.ForeColor = System.Drawing.Color.White;
            this.btnReCheck.Location = new System.Drawing.Point(815, 744);
            this.btnReCheck.Name = "btnReCheck";
            this.btnReCheck.Size = new System.Drawing.Size(152, 59);
            this.btnReCheck.TabIndex = 135;
            this.btnReCheck.Text = "Đồng bộ mã";
            this.btnReCheck.TextColor = System.Drawing.Color.White;
            this.btnReCheck.UseVisualStyleBackColor = false;
            // 
            // lblManualInput
            // 
            this.lblManualInput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblManualInput.Location = new System.Drawing.Point(26, 669);
            this.lblManualInput.Name = "lblManualInput";
            this.lblManualInput.Size = new System.Drawing.Size(115, 20);
            this.lblManualInput.TabIndex = 136;
            this.lblManualInput.Text = "Nhập mã thủ công:";
            // 
            // txtManualInput
            // 
            this.txtManualInput._ReadOnlyBackColor = System.Drawing.Color.WhiteSmoke;
            this.txtManualInput._ReadOnlyBorderFocusColor = System.Drawing.Color.Gainsboro;
            this.txtManualInput.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtManualInput.BackColor = System.Drawing.Color.White;
            this.txtManualInput.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.txtManualInput.BorderFocusColor = System.Drawing.Color.Silver;
            this.txtManualInput.BorderRadius = 6;
            this.txtManualInput.BorderSize = 1;
            this.txtManualInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.txtManualInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtManualInput.Location = new System.Drawing.Point(148, 658);
            this.txtManualInput.Margin = new System.Windows.Forms.Padding(4);
            this.txtManualInput.MinimumSize = new System.Drawing.Size(0, 35);
            this.txtManualInput.Multiline = false;
            this.txtManualInput.Name = "txtManualInput";
            this.txtManualInput.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
            this.txtManualInput.PasswordChar = false;
            this.txtManualInput.PlaceholderColor = System.Drawing.Color.DarkGray;
            this.txtManualInput.PlaceholderText = "Nhập mã QR/Barcode...";
            this.txtManualInput.ReadOnly = false;
            this.txtManualInput.Size = new System.Drawing.Size(650, 35);
            this.txtManualInput.TabIndex = 137;
            this.txtManualInput.UnderlinedStyle = false;
            // 
            // btnAddManual
            // 
            this.btnAddManual._BorderColor = System.Drawing.Color.Silver;
            this.btnAddManual._BorderRadius = 10;
            this.btnAddManual._BorderSize = 0;
            this.btnAddManual._GradientsButton = false;
            this.btnAddManual._Text = "Thêm";
            this.btnAddManual.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnAddManual.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnAddManual.FillColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.btnAddManual.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(165)))));
            this.btnAddManual.FlatAppearance.BorderSize = 0;
            this.btnAddManual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddManual.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnAddManual.ForeColor = System.Drawing.Color.White;
            this.btnAddManual.Location = new System.Drawing.Point(813, 658);
            this.btnAddManual.Name = "btnAddManual";
            this.btnAddManual.Size = new System.Drawing.Size(152, 35);
            this.btnAddManual.TabIndex = 138;
            this.btnAddManual.Text = "Thêm";
            this.btnAddManual.TextColor = System.Drawing.Color.White;
            this.btnAddManual.UseVisualStyleBackColor = false;
            // 
            // lblBatchRecheckInfo
            // 
            this.lblBatchRecheckInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchRecheckInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblBatchRecheckInfo.Location = new System.Drawing.Point(26, 9);
            this.lblBatchRecheckInfo.Name = "lblBatchRecheckInfo";
            this.lblBatchRecheckInfo.Size = new System.Drawing.Size(940, 25);
            this.lblBatchRecheckInfo.TabIndex = 139;
            this.lblBatchRecheckInfo.Text = "Chưa có mã nào được kiểm tra lại";
            // 
            // frmReCheck
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1000, 840);
            this.Controls.Add(this.lblBatchRecheckInfo);
            this.Controls.Add(this.btnAddManual);
            this.Controls.Add(this.txtManualInput);
            this.Controls.Add(this.lblManualInput);
            this.Controls.Add(this.btnReCheck);
            this.Controls.Add(this.NumberOfFailed);
            this.Controls.Add(this.NumberOfSuccess);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblDatabaseType);
            this.Controls.Add(this.listContainer);
            this.Controls.Add(this.lblListTitle);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmReCheck";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đồng bộ mã";
            this.listContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Panel listContainer;
        private FlowLayoutPanel flowProducts;
        private Button btnAddProduct;
        private string notes = string.Empty;
        private Label lblListTitle;
        private Label lblDatabaseType;
        private Label label1;
        private Label NumberOfSuccess;
        private Label NumberOfFailed;
        private DesignUI.CuzUI.CuzButton btnReCheck;
        private Label lblManualInput;
        private DesignUI.CuzUI.CuzTextBox txtManualInput;
        private DesignUI.CuzUI.CuzButton btnAddManual;
        private Label lblBatchRecheckInfo;
    }
}
