using static BarcodeVerificationSystem.Utils.UIControlsFuncs;
using BarcodeVerificationSystem.Utils.ExportData;
using BarcodeVerificationSystem.Utils.ExportData.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using UILanguage;
using System.IO;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.Utils.UI;
using CommonVariable;
using Newtonsoft.Json;

namespace BarcodeVerificationSystem.View.UtilityForms
{
    public partial class frmCusExport : Form
    {
        public List<(bool IsChecked, string Text, string orgText)> Results { get; private set; } = new List<(bool IsChecked, string Text, string orgText)>();
        private List<(CheckBox CheckBox, TextBox TextBox, bool IsChecked, string Text, string orgText)> controlPairs;
        private string[] headers;
        private List<CheckBox> checkBoxList = new List<CheckBox>();
        private List<CheckBox> checkBoxListRes = new List<CheckBox>();
        FilterChecked filterChecked = new FilterChecked("All", "All", "All", "All");
        private CheckedHeaderList checkedHeaderList = new CheckedHeaderList();
        List<string> statusLabelGl = new List<string> { "All", "Printed-Verified", "Printed-Duplicate", "Printed-Unverified", "Unprinted-Verified", "Unprinted-Unverified", "Unprinted-Checked" };
        List<string> statusLabelResGl = new List<string> { "All", "Valid", "Invalided", "Duplicated", "Null", "Missing" };
        CheckedExportResult checkedExportResult = new CheckedExportResult();
        CheckedExportHeader CheckedExportHeader = new CheckedExportHeader();

        public frmCusExport(string[] databaseColunms)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            controlPairs = new List<(CheckBox, TextBox, bool, string, string)>();

            if (databaseColunms.Length > 2)
            {
                string[] pods = databaseColunms.Skip(2).ToArray();
                headers = pods;
                // Tạo 3 cặp CheckBox và TextBox
                InitializeControls(databaseColunms.Length - 2);
            }

            comboBoxDeviceName.SelectedIndex = 0;
            comboBoxCheckedResult.SelectedIndex = 0;

            ExportPrintData.Checked = true;
            ExportPrintData.CheckedChanged += (s, e) => visControl();
            ExportCheckedResult.CheckedChanged += (s, e) => visControl();

            textBoxPV.Text = ExportAllStatus.PrintedVerified;
            textBoxPU.Text = ExportAllStatus.PrintedUnverified;
            textBoxPD.Text = ExportAllStatus.PrintedDuplicate;
            textBoxUV.Text = ExportAllStatus.UnprintedVerified;
            textBoxUU.Text = ExportAllStatus.UnprintedUnverified;
            textBoxUC.Text = ExportAllStatus.UnprintedChecked;

            comboBoxCheckRes.SelectedIndex = 0;
            comboBoxSample.SelectedIndex = 0;
            comboBoxDeviceCheckedResult.SelectedIndex = 0;

            visControl();
            CreateScrollableCheckBoxList();
            CreateScrollableCheckBoxListRes();
            SetLanguage();
        }

        private void SetLanguage()
        {
            this.Text = Lang.ExportData;
            CustomPODToExport.Text = Lang.CustomPODToExport;
            SelectExportMode.Text = Lang.SelectExportMode;
            ExportPrintData.Text = Lang.ExportPrintData;
            ExportCheckedResult.Text = Lang.ExportCheckedResult;
            FilterPrintData.Text = Lang.FilterPrintData;
            FilterCheckResults.Text = Lang.FilterCheckResults;
            CustomHeader.Text = Lang.CustomHeader;
            CustomCheckedHeader.Text = Lang.CustomCheckedHeader;
            CustomCheckedResult.Text = Lang.CustomCheckedResult;
            CustomPrintResult.Text = Lang.CustomPrintResult;
            btnExportData.Text = Lang.ExportData;
            Status.Text = Lang.Status;
            Device.Text = Lang.Device;
        }

        private void CreateScrollableCheckBoxList()
        {
            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Location = new System.Drawing.Point(186, 47),
                Size = new System.Drawing.Size(220, 100),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            List<string> statusLabel = new List<string>
            {
                "All",
                "Printed-Verified",
                "Printed-Duplicate",
                "Printed-Unverified",
                "Unprinted-Verified",
                "Unprinted-Unverified",
                "Unprinted-Checked"
            };

            for (int i = 0; i < statusLabel.Count; i++)
            {
                CheckBox checkBox = new CheckBox
                {
                    Text = statusLabel[i],
                    AutoSize = true,
                    Margin = new Padding(3),
                    Checked = true
                };

                checkBoxList.Add(checkBox);
                flowLayoutPanel.Controls.Add(checkBox);

                checkBox.CheckedChanged += (sender, e) =>
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb == null) return;

                    if (cb.Text == "All")
                    {
                        for (int j = 1; j < checkBoxList.Count; j++)
                            checkBoxList[j].Checked = cb.Checked;
                    }
                };
            }

            FilterPrintData.Controls.Add(flowLayoutPanel);

            Button checkButton = new Button
            {
                Text = "Check Selected",
                Location = new System.Drawing.Point(20, 180),
                Size = new System.Drawing.Size(100, 30)
            };

            checkButton.Click += (sender, e) =>
            {
                var selectedItems = checkBoxList.Where(cb => cb.Checked).Select(cb => cb.Text);
                MessageBox.Show(!selectedItems.Any() ? "No items selected" : $"Selected items:\n{string.Join("\n", selectedItems)}");
            };

            SelectExportMode.Controls.Add(checkButton);
        }

        private void CreateScrollableCheckBoxListRes()
        {
            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Location = new System.Drawing.Point(15, 20),
                Size = new System.Drawing.Size(260, 100),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            List<string> statusLabelRes = new List<string> { "All", "Valid", "Invalided", "Duplicated", "Null", "Missed" };

            for (int i = 0; i < statusLabelRes.Count; i++)
            {
                CheckBox checkBox = new CheckBox
                {
                    Text = statusLabelRes[i],
                    AutoSize = true,
                    Margin = new Padding(3),
                    Checked = (i == 0)
                };

                checkBoxListRes.Add(checkBox);
                flowLayoutPanel.Controls.Add(checkBox);

                checkBox.CheckedChanged += (sender, e) =>
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb == null) return;

                    if (cb.Text == "All")
                    {
                        for (int j = 1; j < checkBoxListRes.Count; j++)
                            checkBoxListRes[j].Checked = cb.Checked;
                    }
                };
            }

            FilterCheckResults.Controls.Add(flowLayoutPanel);

            for (int j = 1; j < checkBoxListRes.Count; j++)
                checkBoxListRes[j].Checked = true;
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                MessageBox.Show($"{checkBox.Text} is {(checkBox.Checked ? "checked" : "unchecked")}");
            }
        }

        private void CheckAllCheckBoxes()
        {
            foreach (var checkBox in checkBoxList)
            {
                if (checkBox.Checked)
                {
                    Console.WriteLine($"{checkBox.Text} is checked");
                }
            }
        }

        private void changeFilter()
        {

            filterChecked.filterStatus = comboBoxCheckRes.SelectedItem.ToString();
            if (filterChecked.filterStatus == "All")
            {
                filterChecked.filterStatus = null;
            }

            filterChecked.filterDevice = comboBoxDeviceName.SelectedItem.ToString();
            if (filterChecked.filterDevice == "All")
            {
                filterChecked.filterDevice = null;
            }

        }

        private void EnableControlPairs(bool sts)
        {
            for (int i = 0; i < controlPairs.Count; i++)
            {
                controlPairs[i].CheckBox.Enabled = sts;
                controlPairs[i].TextBox.Enabled = sts;
            }
            CustomPrintResult.Enabled = sts;
        }

        void visControl()
        {
            if (ExportPrintData.Checked)
            {
                CustomHeader.Enabled = true;
                EnableControlPairs(true);
                FilterPrintData.Enabled = true;
                DisableControls(FilterCheckResults, CustomCheckedResult, CustomCheckedHeader);
            }
            else
            {
                CustomHeader.Enabled = false;
                EnableControlPairs(false);
                FilterPrintData.Enabled = false;
                EnableControls(FilterCheckResults, CustomCheckedResult, CustomCheckedHeader);
            }
        }

        private void InitializeControls(int n)
        {
            for (int i = 0; i < n; i++)
            {
                CheckBox chk = new CheckBox
                {
                    Location = new System.Drawing.Point(20, 45 + i * 30),
                    Text = $"POD {i + 1}",
                    Name = $"chk{i + 1}",
                    Width = 100,
                    Checked = true,
                };

                TextBox txt = new TextBox
                {
                    Location = new System.Drawing.Point(120, 45 + i * 30),
                    Name = $"txt{i + 1}",
                    Width = 200,// Đảm bảo TextBox có kích thước hiển thị rõ
                    Text = headers[i]
                };

                chk.CheckedChanged += (s, e) => UpdatePairValue(chk, txt);
                txt.TextChanged += (s, e) => UpdatePairValue(chk, txt);

                this.Controls.Add(chk);
                this.Controls.Add(txt);

                controlPairs.Add((chk, txt, chk.Checked, txt.Text, headers[i]));
            }
        }

        private void UpdatePairValue(CheckBox chk, TextBox txt)
        {
            for (int i = 0; i < controlPairs.Count; i++)
            {
                if (controlPairs[i].CheckBox == chk && controlPairs[i].TextBox == txt)
                {
                    controlPairs[i] = (chk, txt, chk.Checked, txt.Text, headers[i]);
                    break;
                }
            }
        }

        private void btnShowValues_Click(object sender, EventArgs e)
        {
            foreach (var pair in controlPairs)
            {
                MessageBox.Show($"CheckBox: {pair.CheckBox.Text}, Checked: {pair.IsChecked}, TextBox: {pair.Text}");
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Results.Clear();
            foreach (var pair in controlPairs)
            {
                Results.Add((pair.CheckBox.Checked, pair.TextBox.Text, pair.orgText));
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void CustomHeader_Enter(object sender, EventArgs e)
        {

        }

        private void btnExportData_Click(object sender, EventArgs e)
        {
            filterChecked.filterDeviceDb = comboBoxDeviceName.SelectedItem.ToString();
            if (filterChecked.filterDeviceDb == "All")
            {
                filterChecked.filterDeviceDb = null;
            }

            filterChecked.filterStatusDb = comboBoxCheckedResult.SelectedItem.ToString();
            if (filterChecked.filterStatusDb == "All")
            {
                filterChecked.filterStatusDb = null;
            }

            #region Export status by db
            /// Check checkbox of status by db
            List<string> listToRemove = new List<string>();
            for (int i = 0; i < checkBoxList.Count; i++)
            {
                if (!checkBoxList[i].Checked)
                {
                    listToRemove.Add(statusLabelGl[i]);
                }
            }
            foreach (var item in listToRemove)
            {
                statusLabelGl.Remove(item);
            }
            ExportSharedEvents.RaiseSendMultiStatusDbEvent(statusLabelGl);
            #endregion Export status by db

            #region Custom header
            var lb = new OtherHeader
            {
                statusHeader = textBoxStatusHeader.Text,
                deviceHeader = textBoxDeviceHeader.Text,
                verifydateHeader = textBoxVerifyDateHeader.Text,
                positionHeader = textBoxPositionHeader.Text,
                numCheckHeader = textBoxNumCheckHeader.Text
            };
            ExportSharedEvents.RaiseCustomHeaderDbEvent(lb);

            #endregion Custom header

            #region Checkbox result
            List<string> listToRemoveRes = new List<string>();
            for (int i = 0; i < checkBoxListRes.Count; i++)
            {
                if (!checkBoxListRes[i].Checked)
                {
                    listToRemoveRes.Add(statusLabelResGl[i]);
                }
            }
            foreach (var item in listToRemoveRes)
            {
                statusLabelResGl.Remove(item);
            }
            ExportSharedEvents.RaiseCheckedResultEvent(statusLabelResGl);
            #endregion Checkbox result

            ExportSharedEvents.RaiseDeviceFilterCheckResEvent(comboBoxDeviceCheckedResult.SelectedItem.ToString());

            ExportSharedEvents.RaiseSampleFilterEvent(comboBoxSample.SelectedItem.ToString());

            checkedExportResult.Valid = textBoxValid.Text;
            checkedExportResult.Invalided = textBoxInvalided.Text;
            checkedExportResult.Duplicated = textBoxDuplicated.Text;
            checkedExportResult.Null = textBoxNull.Text;
            checkedExportResult.Missed = textBoxMissed.Text;
            ExportSharedEvents.RaiseCheckedResultLbEvent(checkedExportResult);

            CheckedExportHeader.DataHeader = textBoxDataHeader.Text;
            CheckedExportHeader.DateVerifyHeader = textBoxCheckedDateHeader.Text;
            CheckedExportHeader.DeviceNameHeader = textBoxPosChecked.Text;
            CheckedExportHeader.IndexHeader = textBoxDeviceCheckHeader.Text;
            CheckedExportHeader.IsPositionHeader = textBoxResultCheckHeader.Text;
            CheckedExportHeader.ResultHeader = textBoxResultCheckHeader.Text;
            CheckedExportHeader.SampledHeader = textBoxSampledHeader.Text;
            ExportSharedEvents.RaiseHeaderCheckedEvent(CheckedExportHeader);


            checkedHeaderList.isStatusChecked = CheckStatus.Checked;
            checkedHeaderList.isDeviceChecked = CheckDevice.Checked;
            checkedHeaderList.isVerifyDateChecked = CheckVerifyDate.Checked;
            checkedHeaderList.isPositionChecked = CheckPosition.Checked;
            checkedHeaderList.isCheckingCodeChecked = CheckCheckingCode.Checked;
            ExportSharedEvents.RaiseCheckedHeaderListEvent(checkedHeaderList);


            changeFilter();
            Results.Clear();
            foreach (var pair in controlPairs)
            {
                Results.Add((pair.CheckBox.Checked, pair.TextBox.Text, pair.orgText));
            }
            ExportSharedEvents.RaiseExportAllProgress(Results);
            ExportSharedEvents.RaiseFilterEvent(filterChecked);
            CustomStatusValue cusSts = new CustomStatusValue();

            cusSts.PV = textBoxPV.Text;
            cusSts.PD = textBoxPD.Text;
            cusSts.PU = textBoxPU.Text;
            cusSts.UV = textBoxUV.Text;
            cusSts.UU = textBoxUU.Text;
            cusSts.UC = textBoxUC.Text;

            ExportSharedEvents.RaiseCustomStatusEvent(cusSts);
            if (ExportPrintData.Checked)
            {
                ExportSharedEvents.RaiseExportModeEvent(ExportMode.ExportAll);
            }
            else
            {
                ExportSharedEvents.RaiseExportModeEvent(ExportMode.ExportResult);
            }

            var isSave = CustomMessageBox.Show("Bạn có muốn lưu thông tin xuất mã ?", "Lưu thông tin!", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (isSave == DialogResult.OK)
            {
                string templateName = InputBox.Show("Nhập tên mẫu", "Vui lòng nhập tên mẫu xuất:");
                if (!string.IsNullOrEmpty(templateName))
                {
                    var settings = new ExportSettings
                    {
                        FilterStatus = filterChecked.filterStatus,
                        FilterDevice = filterChecked.filterDevice,
                        FilterStatusDb = filterChecked.filterStatusDb,
                        FilterDeviceDb = filterChecked.filterDeviceDb,
                        StatusLabelGl = new List<string>(statusLabelGl),
                        StatusLabelResGl = new List<string>(statusLabelResGl),
                        DeviceCheckedResult = comboBoxDeviceCheckedResult.SelectedItem?.ToString(),
                        SampleFilter = comboBoxSample.SelectedItem?.ToString(),
                        CustomHeader = lb,
                        CheckedExportHeader = CheckedExportHeader,
                        CheckedHeaderList = checkedHeaderList,
                        CheckedExportResult = checkedExportResult,
                        Results = new List<(bool, string, string)>(Results),
                        CustomStatus = cusSts,
                        ExportMode = ExportPrintData.Checked ? ExportMode.ExportAll : ExportMode.ExportResult,
                        TemplateName = templateName
                    };

                    string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    string sentDataPath = Path.Combine(CommVariables.PathExportTemplates, $"{templateName}.rvis");

                    if (!Directory.Exists(CommVariables.PathExportTemplates))
                    {
                        Directory.CreateDirectory(CommVariables.PathExportTemplates);
                    }

                    try
                    {
                        File.WriteAllText(sentDataPath, json);
                        //MessageBox.Show($"Đã lưu mẫu xuất vào: {sentDataPath}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi lưu mẫu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Bạn chưa nhập tên mẫu.", "Lưu thông tin!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
