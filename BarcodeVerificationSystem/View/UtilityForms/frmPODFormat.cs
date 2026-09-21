using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View
{
    public partial class FrmPODFormat : Form
    {
        public static List<PODModel> _PODFormat = new List<PODModel>();
        public static List<PODModel> _PODList = new List<PODModel>();
        public static string _DirectoryDatabase = "";
        public static bool IsFirstRowHeader = true;
        public static bool _IsComparision = true;
        private Thread _ThreadReadDatabaseFromFile;
        private Thread _ThreadUpdateDataGridView;
        private Thread _ThreadCreatePreviewVerifyData;

        private List<string> _DatabaseColunms = new List<string>();
        private readonly List<string> _VerifyData = new List<string>();
        private readonly List<string[]> _CodeListFromFile = new List<string[]>();
        public double _NumberTotalsCode = 0;

        public FrmPODFormat()
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
            if (_IsComparision)
            {
                lblPODlist.Text = Lang.PODList;
                lblPODformat.Text = Lang.PODFormat;
                lblSample.Text = Lang.Sample;
                lblText.Text = Lang.Text;
                btnSave.Text = Lang.OK;
                lblFormName.Text = Lang.PODFormat;
                btnPreviewComparison.Text = Lang.PreviewComparisonData;
                btnPreviewDatabase.Text = Lang.previewdatabase;
                lblPreview.Text = Lang.previewdatabase;
            }
        }

        private void InitControl()
        {
            ReadDatabaseFromFile();

            txtText.Enabled = false;
            txtSample.Enabled = false;

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

            UpdateFiledsByNumberColumn();


            if (ProjectLabel.IsNutrifood)
            {
                numberOfCodes.Visible = true;
                numberOfCodes.Text = Shared.numberOfCodesGenerate > 0 ? Lang.NumberOfGeneratedCodes + ": " + Shared.numberOfCodesGenerate : Lang.NoCodesGeneratedYet;
            }
        }

        private void UpdateFiledsByNumberColumn()
        {
            if (_DirectoryDatabase != null || _DirectoryDatabase != "")
            {
                try
                {
                    string[] lineContent;
                    string extension = System.IO.Path.GetExtension(_DirectoryDatabase).ToLower();

                    // Check if it's an Excel file
                    if (extension == ".xlsx" || extension == ".xls")
                    {
                        // Read from Excel file
                        lineContent = FileFuncs.GetFirstRowFromExcel(_DirectoryDatabase, IsFirstRowHeader);
                    }
                    else
                    {
                        // Read from CSV or text file
                        Regex splitter = _DirectoryDatabase.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");
                        string tmp = File.ReadLines(_DirectoryDatabase).FirstOrDefault();
                        if (tmp == null)
                            return;
                        lineContent = splitter.Split(tmp).Select(x => Csv.Unescape(x)).ToArray();
                    }

                    if (!IsFirstRowHeader)
                    {
                        for (int i = 0; i < lineContent.Length; i++)
                        {
                            lineContent[i] = $"field{i + 1}";
                        }
                    }

                    var modelText = new PODModel
                    {
                        Type = PODModel.TypePOD.TEXT
                    };
                    listBoxPODLeft.Items.Add(modelText);
                    for (int index = 0; index < lineContent.Count(); index++)
                    {
                        var pod = new PODModel
                        {
                            Index = index + 1,
                            Type = PODModel.TypePOD.FIELD,
                            PODName = lineContent[index] 
                        };
                        listBoxPODLeft.Items.Add(pod);
                    }
                }
                catch
                {

                }
            }
        }

        private void InitEvent()
        {
            btnAdd.Click += ActionChanged;
            btnRemove.Click += ActionChanged;
            btnUp.Click += ActionChanged;
            btnDown.Click += ActionChanged;
            btnClear.Click += ActionChanged;
            btnSave.Click += ActionChanged;
            listBoxPODLeft.DoubleClick += ActionChanged;
            listBoxPODRight.DoubleClick += ActionChanged;
            listBoxPODRight.SelectedIndexChanged += ListBoxPODRight_SelectedIndexChanged;
            txtText.TextChanged += ActionChanged;
            Shared.OnLanguageChange += Shared_OnLanguageChange;
            FormClosing += FrmPODFormat_FormClosing;
            btnPreviewComparison.Click += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    lblPreview.Text = Lang.PreviewComparisonData;
                }));
                string dataColumn = "";
                _PODFormat.Clear();
                foreach (var item in listBoxPODRight.Items)
                {
                    _PODFormat.Add((PODModel)item);
                }
                dataColumn = string.Join("", _PODFormat.Select(x => x.ToStringSample()));
                string[] columns = new string[] { "Index", dataColumn };
                if (_PODFormat.Count() > 0)
                {
                    AdjustDataGridViewColumn(dgvSampleData, columns);
                    string[] newColumn = new string[] { "Index", "IndexPlus" };
                    DataGridViewCustom.AutoResizeColumnWith(dgvSampleData, newColumn);
                }
            };

            btnPreviewDatabase.Click += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    lblPreview.Text = Lang.previewdatabase;
                }));

                AdjustDataGridViewColumn(dgvSampleData, _DatabaseColunms.ToArray());
                if (_CodeListFromFile.Count == 0)
                    DataGridViewCustom.AutoResizeColumnWith(dgvSampleData, _DatabaseColunms.ToArray());
                else
                    DataGridViewCustom.AutoResizeColumnWith(dgvSampleData, _CodeListFromFile[0]);
            };
        }

        private void ListBoxPODRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtText.Clear();
            if (listBoxPODRight.SelectedItem == null)
            {
                txtText.Enabled = false;
                return;
            }

            int index = listBoxPODRight.Items.IndexOf(listBoxPODRight.SelectedItem);
            var podTMP = (PODModel)listBoxPODRight.Items[index];
            if (podTMP.Type == PODModel.TypePOD.TEXT)
            {
                txtText.Enabled = true;
                txtText.Text = podTMP.Value;
            }
            else
            {
                txtText.Enabled = false;
            }
        }

        private void Shared_OnLanguageChange(object sender, EventArgs e)
        {
            SetLanguage();
        }

        private void FrmPODFormat_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                KillThreadCreatePreviewVerifyData();
                KillThreadReadDatabaseFromFile();
                KillThreadUpdateDataGridView();

                _PODFormat.Clear();
                _DatabaseColunms.Clear();
                _CodeListFromFile.Clear();
                dgvSampleData.Rows.Clear();

                if (_ThreadReadDatabaseFromFile != null && _ThreadReadDatabaseFromFile.IsAlive)
                {
                    _ThreadReadDatabaseFromFile.Abort();
                    _ThreadReadDatabaseFromFile = null;
                }

                foreach (var item in listBoxPODRight.Items)
                {
                    var podTmp = (PODModel)item;
                    _PODFormat.Add(podTmp);
                }
                DialogResult = DialogResult.OK;
            }
            catch
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void ActionChanged(object sender, EventArgs e)
        {
            if (sender == listBoxPODLeft)
            {
                if (listBoxPODLeft.SelectedItem == null)
                {
                    return;
                }

                var podTmp = ((PODModel)(listBoxPODLeft.SelectedItem));
                listBoxPODRight.Items.Add(podTmp.Clone());
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
                var podTmp = ((PODModel)(listBoxPODLeft.SelectedItem));
                listBoxPODRight.Items.Add(podTmp.Clone());
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
            else if (sender == btnUp)
            {
                if (listBoxPODRight.SelectedItem == null)
                {
                    return;
                }
                int index = listBoxPODRight.Items.IndexOf(listBoxPODRight.SelectedItem);
                var podTMP = (PODModel)listBoxPODRight.Items[index];
                if (index > 0)
                {
                    listBoxPODRight.Items.RemoveAt(index);
                    listBoxPODRight.Items.Insert(index - 1, podTMP);
                    listBoxPODRight.SetSelected(index - 1, true);
                }
                Sample();
            }
            else if (sender == btnDown)
            {
                if (listBoxPODRight.SelectedItem == null)
                {
                    return;
                }
                int index = listBoxPODRight.Items.IndexOf(listBoxPODRight.SelectedItem);
                var podTMP = (PODModel)listBoxPODRight.Items[index];
                if (index < listBoxPODRight.Items.Count - 1)
                {
                    listBoxPODRight.Items.RemoveAt(index);
                    listBoxPODRight.Items.Insert(index + 1, podTMP);
                    listBoxPODRight.SetSelected(index + 1, true);
                }
                Sample();
            }
            else if (sender == btnClear)
            {
                listBoxPODRight.Items.Clear();
                txtText.Text = "";
                txtSample.Text = "";
            }
            else if (sender == btnSave)
            {
                _PODFormat.Clear();
                foreach (var item in listBoxPODRight.Items)
                {
                    var podTmp = (PODModel)item;
                    if (podTmp.Type == PODModel.TypePOD.TEXT)
                    {
                        podTmp.Value = txtText.Text;
                    }
                    _PODFormat.Add(podTmp);
                }
                DialogResult = DialogResult.OK;
            }
            else if (sender == listBoxPODRight)
            {

            }
            else if (sender == txtText)
            {
                if (txtText.Text != "")
                {
                    int index = listBoxPODRight.Items.IndexOf(listBoxPODRight.SelectedItem);
                    var podTMP = (PODModel)listBoxPODRight.Items[index];
                    podTMP.Value = txtText.Text;
                    Sample();
                }
            }
        }

        private void Sample()
        {
            txtSample.Text = "";
            foreach (var item in listBoxPODRight.Items)
            {
                var podTmp = (PODModel)item;
                txtSample.Text += podTmp.ToStringSample();
            }
        }

        private void PreviewVerifyData()
        {
            _PODFormat.Clear();
            foreach (object listboxRightItems in listBoxPODRight.Items)
            {
                var podTmp = (PODModel)listboxRightItems;
                _PODFormat.Add(podTmp);
            }
            _VerifyData.Clear();
            KillThreadCreatePreviewVerifyData();
            _ThreadCreatePreviewVerifyData = new Thread(() =>
            {
                if (_CodeListFromFile != null)
                {
                    if (_PODFormat.Count > 0)
                    {
                        foreach (string[] item in _CodeListFromFile)
                        {
                            string data = "";
                            foreach (var model in _PODFormat)
                            {
                                if (model.Type == PODModel.TypePOD.FIELD)
                                {
                                    data += item[model.Index - 1];
                                }
                                else if (model.Type == PODModel.TypePOD.TEXT)
                                {
                                    data += model.Value;
                                }
                            }
                            _VerifyData.Add(data);
                        }
                    }
                }
            });
            _ThreadCreatePreviewVerifyData.IsBackground = true;
            _ThreadCreatePreviewVerifyData.Start();
        }

        private void ReadDatabaseFromFile()
        {
            _PODFormat.Clear();
            _CodeListFromFile.Clear();
            foreach (var listboxRightItems in listBoxPODRight.Items)
            {
                var podTmp = (PODModel)listboxRightItems;
                _PODFormat.Add(podTmp);
            }
            KillThreadReadDatabaseFromFile();
            _ThreadReadDatabaseFromFile = new Thread(() =>
            {
                if (_DirectoryDatabase == "")
                {
                    return;
                }
                try
                {
                    if (File.Exists(_DirectoryDatabase))
                    {
                        string extension = System.IO.Path.GetExtension(_DirectoryDatabase).ToLower();
                        
                        // Check if it's an Excel file
                        if (extension == ".xlsx" || extension == ".xls")
                        {
                            ReadExcelDatabase();
                        }
                        else
                        {
                            ReadCsvDatabase();
                        }
                    }
                }
                catch (Exception)
                {

                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            _ThreadReadDatabaseFromFile.Start();
        }

        private void ReadExcelDatabase()
        {
            try
            {
                // Get header row to set up columns
                string[] headerRow = FileFuncs.GetFirstRowFromExcel(_DirectoryDatabase, true);
                if (headerRow == null || headerRow.Length == 0)
                    return;

                var line = headerRow.ToList();
                var temp = headerRow.ToList();

                if (!IsFirstRowHeader)
                {
                    for (int i = 0; i < line.Count; i++)
                    {
                        line[i] = $"field{i + 1}";
                    }
                }

                _DatabaseColunms = line.ToList();
                _DatabaseColunms.Insert(0, "Index");
                Invoke(new Action(() =>
                {
                    InitDataGridView(dgvSampleData, _DatabaseColunms.ToArray());
                }));
                int columnCount = _DatabaseColunms.Count();

                // Read data rows (this will skip header if IsFirstRowHeader is true)
                List<string[]> allRows = FileFuncs.ReadExcelData(_DirectoryDatabase, null);
                
                int lineCounter = 0;

                // If first row is not header, add it as first data row
                if (!IsFirstRowHeader)
                {
                    string[] code = new string[columnCount];
                    code[0] = "" + lineCounter;
                    for (int i = 1; i < code.Length; i++)
                    {
                        if (i - 1 < temp.Count)
                        {
                            code[i] = MaskData.MaskString(temp[i - 1]);
                        }
                        else
                        {
                            code[i] = "";
                        }
                    }
                    _CodeListFromFile.Add(code);
                }

                // Process remaining rows
                if (allRows != null && allRows.Count > 0)
                {
                    foreach (var row in allRows)
                    {
                        lineCounter++;

                        if (lineCounter <= 500)
                        {
                            List<string> rowList = row.ToList();
                            string[] code = new string[columnCount];
                            code[0] = "" + lineCounter;
                            for (int i = 1; i < code.Length; i++)
                            {
                                if (i - 1 < rowList.Count)
                                {
                                    code[i] = MaskData.MaskString(rowList[i - 1]);
                                }
                                else
                                {
                                    code[i] = "";
                                }
                            }

                            _CodeListFromFile.Add(code);
                        }
                    }
                }

                Invoke(new Action(() =>
                {
                    try
                    {
                        if (_CodeListFromFile.Count() > 0)
                        {
                            DataGridViewCustom.AutoResizeColumnWith(dgvSampleData, _CodeListFromFile[0]);
                            _NumberTotalsCode = lineCounter;
                        }
                    }
                    catch
                    {

                    }
                }));
            }
            catch (Exception)
            {

            }
        }

        private void ReadCsvDatabase()
        {
            try
            {
                using (var reader = new StreamReader(_DirectoryDatabase, Encoding.UTF8, true, 65536))
                {
                    var splitter = _DirectoryDatabase.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");
                    bool isFirstline = false;
                    int lineCounter = 0;
                    int columnCount = 0;
                    while (!reader.EndOfStream)
                    {
                        if (!isFirstline)
                        {
                            var line = splitter.Split(reader.ReadLine()).Select(x => Csv.Unescape(x)).ToList();
                            if (line == null)
                            {
                                break;
                            }

                            var temp = line.ToList();
                            if (!IsFirstRowHeader)
                            {
                                for (int i = 0; i < line.Count; i++)
                                {
                                    line[i] = $"field{i + 1}";
                                }
                            }

                            _DatabaseColunms = line.ToList();
                            _DatabaseColunms.Insert(0, "Index");
                            Invoke(new Action(() =>
                            {
                                InitDataGridView(dgvSampleData, _DatabaseColunms.ToArray());
                            }));
                            columnCount = _DatabaseColunms.Count();
                            isFirstline = true;

                            // them
                            if (!IsFirstRowHeader)
                            {
                                string[] code = new string[columnCount];
                                code[0] = "" + lineCounter;
                                for (int i = 1; i < code.Length; i++)
                                {
                                    if (i - 1 < temp.Count)
                                    {
                                        code[i] = MaskData.MaskString(temp[i - 1]);
                                    }
                                    else
                                    {
                                        code[i] = "";
                                    }
                                }
                                _CodeListFromFile.Add(code);
                            }

                        }
                        else
                        {
                            lineCounter++;

                            if (lineCounter <= 500)
                            {
                                List<string> line = splitter.Split(reader.ReadLine()).Select(x => Csv.Unescape(x)).ToList();
                                if (line == null)
                                {
                                    break;
                                }

                                string[] code = new string[columnCount];
                                code[0] = "" + lineCounter;
                                for (int i = 1; i < code.Length; i++)
                                {
                                    if (i - 1 < line.Count)
                                    {
                                        code[i] = MaskData.MaskString(line[i - 1]);
                                    }
                                    else
                                    {
                                        code[i] = "";
                                    }
                                }

                                _CodeListFromFile.Add(code);
                            }
                            else
                            {
                                reader.ReadLine();
                            }
                        }
                    }

                    Invoke(new Action(() =>
                    {
                        try
                        {
                            if (_CodeListFromFile.Count() > 0)
                            {
                                DataGridViewCustom.AutoResizeColumnWith(dgvSampleData, _CodeListFromFile[0]);
                                _NumberTotalsCode = lineCounter;
                            }
                        }
                        catch
                        {

                        }
                    }));
                }
            }
            catch (Exception)
            {

            }
        }

        private void KillThreadUpdateDataGridView()
        {
            if (_ThreadUpdateDataGridView != null && _ThreadUpdateDataGridView.IsAlive)
            {
                _ThreadUpdateDataGridView.Abort();
                _ThreadUpdateDataGridView = null;
            }
        }

        private void KillThreadReadDatabaseFromFile()
        {
            if (_ThreadReadDatabaseFromFile != null && _ThreadReadDatabaseFromFile.IsAlive)
            {
                _ThreadReadDatabaseFromFile.Abort();
                _ThreadReadDatabaseFromFile = null;
            }
        }

        private void KillThreadCreatePreviewVerifyData()
        {
            if (_ThreadCreatePreviewVerifyData != null && _ThreadCreatePreviewVerifyData.IsAlive)
            {
                _ThreadCreatePreviewVerifyData.Abort();
                _ThreadCreatePreviewVerifyData = null;
            }
        }

        public void InitDataGridView(DataGridView dgv, string[] columns)
        {
            dgv.CellValueNeeded += (obj, e) =>
            {
                if (e.RowIndex == -1) return;
                try
                {
                    string cell = "";
                    if (_CodeListFromFile.Count <= 0) return;
                    if (dgv.Columns.Count < _CodeListFromFile[0].Length && e.ColumnIndex != 0)
                    {
                        string[] row = _CodeListFromFile[e.RowIndex];
                        foreach (PODModel item in _PODFormat)
                        {
                            if (item.Type == PODModel.TypePOD.DATETIME)
                            {
                                cell += DateTime.Now;
                            }
                            else if (item.Type == PODModel.TypePOD.FIELD)
                            {
                                cell += row[item.Index];
                            }
                            else
                            {
                                cell += item.Value;
                            }
                        }
                    }
                    else
                    {
                        cell = _CodeListFromFile[e.RowIndex][e.ColumnIndex];
                    }
                    if (cell == "") return;
                    e.Value = cell;
                }
                catch
                {

                }
            };

            dgv.VirtualMode = true;
            dgv.RowCount = 500;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgv.Columns.Clear();
            try
            {
                int tableWidth = dgv.Width;
                float percentWidth = (float)1 / columns.Length;
                int tableCodeProductListWidth = dgv.Width - 39;
                for (int index = 0; index < columns.Length; index++)
                {
                    var col = new DataGridViewTextBoxColumn
                    {
                        HeaderText = (index != 0) ? columns[index] + " - Field" + (index) : columns[index],
                        //HeaderText = " Field" + (index),

                        Name = columns[index].Trim(),
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    if (index == 0)
                    {
                        col.Width = (int)(0.75 * tableCodeProductListWidth);
                    }
                    Size textSize = TextRenderer.MeasureText(col.HeaderText, dgv.Font);
                    col.Width = textSize.Width + 40;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv.Columns.Add(col);
                }
            }
            catch
            {

            }

            DataGridViewCustom.AutoResizeColumnWith(dgv, columns);
            dgv.RowCount = 500;
        }

        public void AdjustDataGridViewColumn(DataGridView dgv, string[] columns)
        {
            try
            {
                dgv.RowCount = 0;
                dgv.Columns.Clear();
                int tableWidth = dgv.Width;
                float percentWidth = (float)1 / columns.Length;
                int tableCodeProductListWidth = dgv.Width - 39;
                for (int index = 0; index < columns.Length; index++)
                {
                    var col = new DataGridViewTextBoxColumn();
                    if (index > 0)
                    {
                        col.HeaderText = (index != 0 && columns.Count() - 1 != index) ? columns[index] + " - Field" + (index) : columns[index];
                        col.Name = columns[index].Trim();
                    }
                    else
                    {
                        col.HeaderText = columns[index];
                        col.Name = columns[index].Trim();
                    }
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    if (index == 0)
                    {
                        col.Width = (int)(0.75 * tableCodeProductListWidth);
                    }

                    Size textSize = TextRenderer.MeasureText(col.HeaderText, dgv.Font);
                    col.Width = textSize.Width + 40;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgv.Columns.Add(col);
                }
                DataGridViewCustom.AutoResizeColumnWith(dgv, columns);
                dgv.RowCount = _CodeListFromFile.Count();
            }
            catch
            {

            }
        }
    }
}
