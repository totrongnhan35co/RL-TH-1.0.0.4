using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmPrintFieldForVerifyAndPrintCompareMode : Form
    {
        public List<PODModel> _PODFormat = new List<PODModel>();
        public List<PODModel> _PODList = new List<PODModel>();
        public FrmPrintFieldForVerifyAndPrintCompareMode()
        {
            InitializeComponent();
        }

        private void FrmPODFormat_Load(object sender, EventArgs e)
        {
            InitControl();
            InitEvent();
            SetLanguage();
        }

        private void SetLanguage()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetLanguage()));
                return;
            }
            lblFormName.Text = Lang.SelectPrintField;
            lblFieldlist.Text = Lang.ListOfField;
            lblSelectedFiled.Text = Lang.SelectedField;
            lblPrintFields.Text = Lang.PrintField;
            btnSave.Text = Lang.OK;
            btnCancel.Text = Lang.Cancel;
        }

        private void InitControl()
        {
            if (_PODFormat.Count > 0)
            {
                for (int index = 0; index < _PODFormat.Count; index++)
                {
                    PODModel pod = _PODFormat[index];
                    listBoxPODLeft.Items.Add(pod);
                }
            }
            else
            {
                for (int index = 1; index <= 20; index++)
                {
                    var podVCD = new PODModel(index, "", PODModel.TypePOD.FIELD, "");
                    _PODFormat.Add(podVCD);
                    listBoxPODLeft.Items.Add(podVCD);
                }
            }

            txtPrintFields.Enabled = false;


            listBoxPODLeft.DrawMode = DrawMode.OwnerDrawVariable;
            listBoxPODLeft.DrawItem -= DrawListBox.ListBoxJobNameList_DrawItem;
            listBoxPODLeft.DrawItem += DrawListBox.ListBoxJobNameList_DrawItem;
            listBoxPODLeft.ItemHeight = 38;
            listBoxPODLeft.BorderStyle = BorderStyle.None;


            listBoxPODRight.DrawMode = DrawMode.OwnerDrawVariable;
            listBoxPODRight.DrawItem -= DrawListBox.ListBoxJobNameList_DrawItem;
            listBoxPODRight.DrawItem += DrawListBox.ListBoxJobNameList_DrawItem;
            listBoxPODRight.ItemHeight = 38;
            listBoxPODRight.BorderStyle = BorderStyle.None;
        }

        private void InitEvent()
        {
            btnAdd.Click += ActionChanged;
            btnRemove.Click += ActionChanged;
            btnClear.Click += ActionChanged;
            btnSave.Click += ActionChanged;
            btnCancel.Click += ActionChanged;
            listBoxPODLeft.DoubleClick += ActionChanged;
            listBoxPODRight.DoubleClick += ActionChanged;
            listBoxPODRight.SelectedIndexChanged += ListBoxPODRight_SelectedIndexChanged;
            txtPrintFields.TextChanged += ActionChanged;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
        }

        private void ListBoxPODRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtPrintFields.Clear();
            if (listBoxPODRight.SelectedItem == null)
            {
                txtPrintFields.Enabled = false;
                return;
            }

            int index = listBoxPODRight.Items.IndexOf(listBoxPODRight.SelectedItem);
            var podTMP = (PODModel)listBoxPODRight.Items[index];
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        private void FrmPODFormat_FormClosing(object sender, FormClosingEventArgs e)
        {
            _PODFormat.Clear();
            foreach (object item in listBoxPODRight.Items)
            {
                var podTmp = (PODModel)item;
                _PODFormat.Add(podTmp);
            }
            DialogResult = DialogResult.OK;
        }

        private void ActionChanged(object sender, EventArgs e)
        {
            if (sender == listBoxPODLeft)
            {
                if (listBoxPODLeft.SelectedItem == null)
                {
                    return;
                }
                AddingField();
                Sample();
            }
            else if (sender == listBoxPODRight)
            {
                if (listBoxPODRight.SelectedItem == null)
                {
                    return;
                }
                listBoxPODRight.Items.Remove(listBoxPODRight.SelectedItem);
                Sample();
            }
            else if (sender == btnAdd)
            {
                if (listBoxPODLeft.SelectedItem == null)
                {
                    return;
                }
               
                AddingField();
                Sample();
            }
            else if (sender == btnRemove)
            {
                if (listBoxPODRight.SelectedItem == null)
                {
                    return;
                }
                listBoxPODRight.Items.Remove(listBoxPODRight.SelectedItem);
                Sample();
            }
            else if (sender == btnClear)
            {
                listBoxPODRight.Items.Clear();
                txtPrintFields.Text = "";
                Sample();
            }
            else if (sender == btnSave)
            {
                _PODFormat.Clear();
                foreach (object item in listBoxPODRight.Items)
                {
                    var podTmp = (PODModel)item;
                    if (podTmp.Type == PODModel.TypePOD.TEXT)
                    {
                        podTmp.Value = txtPrintFields.Text;
                    }
                    _PODFormat.Add(podTmp);
                }
                DialogResult = DialogResult.OK;
            }
            else if (sender == btnCancel)
            {
                _PODFormat.Clear();
                DialogResult = DialogResult.OK;
            }
            else if (sender == listBoxPODRight)
            {

            }
            else if (sender == txtPrintFields)
            {

            }
        }

        private void AddingField()
        {
            try
            {
                var podTmp = ((PODModel)(listBoxPODLeft.SelectedItem));
                bool checkExist = false;
                foreach (object item in listBoxPODRight.Items)
                {
                    var podItem = (PODModel)(item);
                    if (podItem.Index == podTmp.Index)
                    {
                        checkExist = true;
                    }
                }

                if (!checkExist)
                {
                    if (listBoxPODRight.Items.Count == 0)
                    {
                        listBoxPODRight.Items.Add(podTmp.Clone());
                    }
                    else
                    {
                        for (int i = 0; i < listBoxPODRight.Items.Count; i++)
                        {
                            object item = listBoxPODRight.Items[i];
                            var podItem = (PODModel)(item);

                            if (podItem.Index == podTmp.Index + 1)
                            {
                                listBoxPODRight.Items.Insert(i, podTmp.Clone());
                                return;
                            }
                        }

                        object lastItem = listBoxPODRight.Items[listBoxPODRight.Items.Count - 1];
                        var lastPodItem = (PODModel)(lastItem);
                        if (lastPodItem.Index > podTmp.Index)
                        {
                            listBoxPODRight.Items.Insert(0, podTmp.Clone());
                        }
                        else if(lastPodItem.Index < podTmp.Index)
                        {
                            listBoxPODRight.Items.Add(podTmp.Clone());
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void Sample()
        {
            txtPrintFields.Text = "";
            foreach (object item in listBoxPODRight.Items)
            {
                var podTmp = (PODModel)item;
                txtPrintFields.Text += podTmp.ToString();
            }
        }
    }
}
