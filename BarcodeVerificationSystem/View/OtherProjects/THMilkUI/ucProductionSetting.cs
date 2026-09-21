using BarcodeVerificationSystem.Controller;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UILanguage;

namespace BarcodeVerificationSystem.View.OtherProjects.THMilkUI
{
    public partial class ucProductionTHSetting : UserControl
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

        public ucProductionTHSetting()
        {
            InitializeComponent();
            InitControls();
            InitEvents();
            InitDbControls();
            InitDbEvents();
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

        #region Database Local Settings

        private void InitDbControls()
        {
            cmbDatabaseType.Items.Clear();
            cmbDatabaseType.Items.Add("MySQL");
            cmbDatabaseType.Items.Add("SQL");

            var dbModel = Shared.Settings.CenterIndiaModel;
            int idx = cmbDatabaseType.FindStringExact(dbModel.DatabaseType ?? "MySQL");
            cmbDatabaseType.SelectedIndex = idx >= 0 ? idx : 0;

            txtDbServerName.Text = dbModel.ServerName ?? "";
            txtDbPort.Text = dbModel.Port ?? "";
            txtDbDatabaseName.Text = dbModel.DatabaseName ?? "";
            txtDbTableName.Text = dbModel.TableName ?? "";
            txtDbUsername.Text = dbModel.Username ?? "";
            txtDbPassword.Text = dbModel.Password ?? "";

            UpdateDbPortVisibility();
        }

        private void InitDbEvents()
        {
            cmbDatabaseType.SelectedIndexChanged += AdjustDbData;
            cmbDatabaseType.SelectedIndexChanged += (s, e) => UpdateDbPortVisibility();
            txtDbServerName.TextChanged += AdjustDbData;
            txtDbPort.TextChanged += AdjustDbData;
            txtDbDatabaseName.TextChanged += AdjustDbData;
            txtDbTableName.TextChanged += AdjustDbData;
            txtDbUsername.TextChanged += AdjustDbData;
            txtDbPassword.TextChanged += AdjustDbData;
        }

        private void UpdateDbPortVisibility()
        {
            bool isMySQL = string.Compare(
                cmbDatabaseType.SelectedItem?.ToString(), "SQL",
                StringComparison.OrdinalIgnoreCase) != 0;
            lblDbPort.Visible = isMySQL;
            txtDbPort.Visible = isMySQL;
        }

        private void AdjustDbData(object sender, EventArgs e)
        {
            var dbModel = Shared.Settings.CenterIndiaModel;
            dbModel.DatabaseType = cmbDatabaseType.SelectedItem?.ToString() ?? "MySQL";
            dbModel.ServerName = txtDbServerName.Text.Trim();
            dbModel.Port = txtDbPort.Text.Trim();
            dbModel.DatabaseName = txtDbDatabaseName.Text.Trim();
            dbModel.TableName = txtDbTableName.Text.Trim();
            dbModel.Username = txtDbUsername.Text.Trim();
            dbModel.Password = txtDbPassword.Text;
            Shared.SaveSettings();
        }

        private async void btnTestDbConnection_Click(object sender, EventArgs e)
        {
            btnTestDbConnection.Enabled = false;
            lblDbConnectionStatus.Text = "Đang kết nối...";
            lblDbConnectionStatus.ForeColor = Color.Orange;

            bool isConnected = false;
            string errorMessage = null;
            string connectionString = GetDatabaseConnectionString();

            await Task.Run(() =>
            {
                try
                {
                    using (IDbConnection conn = GetDatabaseConnection(connectionString))
                    {
                        conn.Open();
                        isConnected = conn.State == ConnectionState.Open;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            });

            btnTestDbConnection.Enabled = true;
            if (isConnected)
            {
                lblDbConnectionStatus.Text = "✔ Kết nối thành công!";
                lblDbConnectionStatus.ForeColor = Color.Green;
            }
            else
            {
                lblDbConnectionStatus.Text = "✘ " + (errorMessage ?? "Kết nối thất bại!");
                lblDbConnectionStatus.ForeColor = Color.Red;
            }
        }

        private string GetDatabaseConnectionString()
        {
            var dbModel = Shared.Settings.CenterIndiaModel;
            string databaseType = dbModel.DatabaseType ?? "MySQL";
            switch (databaseType.ToLower())
            {
                case "sql":
                    return $"Server={dbModel.ServerName};Database={dbModel.DatabaseName};User Id={dbModel.Username};Password={dbModel.Password};";
                case "mysql":
                default:
                    return $"Server={dbModel.ServerName};Port={dbModel.Port};Database={dbModel.DatabaseName};Uid={dbModel.Username};Pwd={dbModel.Password};";
            }
        }

        private IDbConnection GetDatabaseConnection(string connectionString)
        {
            string databaseType = Shared.Settings.CenterIndiaModel.DatabaseType ?? "MySQL";
            switch (databaseType.ToLower())
            {
                case "sql":
                    return new SqlConnection(connectionString);
                case "mysql":
                default:
                    return new MySqlConnection(connectionString);
            }
        }

        #endregion
    }
}