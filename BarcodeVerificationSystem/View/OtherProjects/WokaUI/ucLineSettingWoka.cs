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

namespace BarcodeVerificationSystem.View.WokaUI
{
    public partial class ucLineSettingWoka : UserControl
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
            new Factory { Code = "A", Name = "Xuân Lộc" },
            new Factory { Code = "B", Name = "An Đông" },
        };

        public ucLineSettingWoka()
        {
            InitializeComponent();
            InitControls();
            InitEvents();
            InitLanguage();
        }

        private void InitLanguage()
        {
            groupBoxProductionSettings.Text = "Cài đặt Line"; // Lang.ProductionSettings
            labelApi.Text = "URL máy chủ:"; // Lang.URLPath;
        }

        private void InitControls()
        {
            apiTextbox.Text = Shared.Settings.ApiUrl;
            WokaToken.Text = Shared.Settings.WokaToken;
            InitDeviceName();
            RLinkNamescombox.SelectedItem = Shared.Settings.RLinkName;
        }

        private void InitEvents()
        {
            apiTextbox.TextChanged += AdjustData;
            WokaToken.TextChanged += AdjustData;
            RLinkNamescombox.SelectedIndexChanged += AdjustData;
        }

        private void InitDeviceName()
        {
            RLinkNamescombox.Text = string.Empty;
            RLinkNames = Enumerable
                        .Range(1, 10)
                        .Select(i => $"Line{i:D3}")
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
                        //Shared.Settings.LineIndex = int.Parse(Shared.Settings.RLinkName.Substring(2));
                        Shared.Settings.LineIndex = int.Parse(Shared.Settings.RLinkName.Substring(Shared.Settings.RLinkName.Length - 2));
                    }
                    break;

                case TextBox tb:
                    if (tb == apiTextbox)
                        Shared.Settings.ApiUrl = tb.Text;
                    else if (tb == WokaToken)
                        Shared.Settings.WokaToken = tb.Text;
                    break;
            }
            Shared.SaveSettings();

        }

    }
}
