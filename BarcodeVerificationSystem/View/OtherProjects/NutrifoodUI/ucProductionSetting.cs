using BarcodeVerificationSystem.Controller;
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

namespace BarcodeVerificationSystem.View.UcSettings
{
    public partial class ucProductionNutiSetting : UserControl
    {
        private string[] RLinkNames;
        public class Factory
        {
            public string Code { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return $"{Code} ({Name})";
            }
        }

        private List<Factory> factories = new List<Factory>
        {
            new Factory { Code = "1210", Name = "Bình Dương" },
            new Factory { Code = "1240", Name = "Hưng Yên" },
            new Factory { Code = "1260", Name = "Gia Lai" },
            new Factory { Code = "1212", Name = "Đà Nẵng" }
        };

        public ucProductionNutiSetting()
        {
            InitializeComponent();
            InitControls();
            InitEvents();
            InitLanguage();
        }

        private void InitLanguage()
        {
            lineIdLabel.Text = Lang.LineID;
            lineNameLabel.Text = Lang.LineName;
            factoryCodeLabel.Text = Lang.FactoryCode + ":";
            manufacturingRad.Text = Lang.Manufacturing;
            dispatchingRad.Text = Lang.Dispatching;
            productionMode.Text = Lang.ProductionMode;
            dataDisplay.Text = Lang.DisplayData;
            maskData.Text = Lang.MaskData;
            dataIncrease.Text = Lang.IncreasedData;
            groupBoxProductionSettings.Text = "Cài đặt Line"; // Lang.ProductionSettings
            labelApi.Text = "URL máy chủ:"; // Lang.URLPath;
        }

        private void InitControls()
        {
            apiTextbox.Text = Shared.Settings.ApiUrl;
            numIncreasedData.Value = Shared.Settings.IncreasedDataPercent;
            manufacturingRad.Checked = Shared.Settings.IsManufacturingMode;
            dispatchingRad.Checked = !Shared.Settings.IsManufacturingMode;
            maskData.Checked = Shared.Settings.MaskData;
            HideFunctions.Checked = Shared.Settings.HideFunctions;
            lineName.Text = Shared.Settings.LineName;
            LineId.Text = Shared.Settings.LineId;

            FactoryCodeCombox.Items.Clear();
            FactoryCodeCombox.Items.AddRange(factories.ToArray());
            var selected = factories.FirstOrDefault(f => f.Code == Shared.Settings.FactoryCode);
            if (selected != null)
            {
                FactoryCodeCombox.SelectedItem = selected;
            }
            onlineProductionSettings.Enabled = !Shared.UserPermission.isOnline;

            InitDeviceName();
            RLinkNamescombox.SelectedItem = Shared.Settings.RLinkName;
        }

        private void InitEvents()
        {
            apiTextbox.TextChanged += AdjustData;
            RLinkNamescombox.SelectedIndexChanged += AdjustData;
            FactoryCodeCombox.SelectedIndexChanged += AdjustData;
            numIncreasedData.ValueChanged += AdjustData;
            manufacturingRad.CheckedChanged += AdjustData;
            dispatchingRad.CheckedChanged += AdjustData;
            maskData.CheckedChanged += AdjustData;
            HideFunctions.CheckedChanged += AdjustData;
            lineName.TextChanged += AdjustData;
            LineId.TextChanged += AdjustData;
        }

        private void InitDeviceName()
        {
            bool t = Shared.Settings.IsManufacturingMode;
            RLinkNamescombox.Text = string.Empty;
            RLinkNames = Enumerable
                        .Range(1, 30)
                        .Select(i => Shared.Settings.IsManufacturingMode ? $"SX{i:D3}" : $"XH{i:D3}")
                        .ToArray();
            RLinkNamescombox.Items.Clear();
            RLinkNamescombox.Items.AddRange(RLinkNames);
        }

        private void AdjustData(object sender, EventArgs args)
        {
            switch (sender)
            {
                case ComboBox cb:
                    if (cb == RLinkNamescombox)
                    {
                        Shared.Settings.RLinkName = cb.SelectedItem?.ToString() ?? string.Empty;
                        Shared.Settings.LineIndex = int.Parse(Shared.Settings.RLinkName.Substring(2));
                    }
                    else if (cb == FactoryCodeCombox)
                    {
                        var selectedFactory = cb.SelectedItem as Factory;
                        if (selectedFactory != null)
                        {
                            Shared.Settings.FactoryCode = selectedFactory.Code;
                        }
                    }
                    break;

                case TextBox tb:
                    if (tb == apiTextbox)
                        Shared.Settings.ApiUrl = tb.Text;
                    else if (tb == lineName)
                        Shared.Settings.LineName = tb.Text;
                    else if (tb == LineId)
                        Shared.Settings.LineId = tb.Text;
                    break;

                case NumericUpDown num:
                    if (num == numIncreasedData)
                        Shared.Settings.IncreasedDataPercent = (int)numIncreasedData.Value;
                    break;
                case RadioButton rb:
                    if (rb == manufacturingRad || rb == dispatchingRad)
                    {
                        Shared.Settings.IsManufacturingMode = manufacturingRad.Checked;
                        InitDeviceName();
                    }
                    break;
                case CheckBox cbx:
                    if (cbx == maskData)
                        Shared.Settings.MaskData = cbx.Checked;
                    if (cbx == HideFunctions)
                        Shared.Settings.HideFunctions = cbx.Checked;
                    break;
            }
            Shared.SaveSettings();

        }

    }
}





