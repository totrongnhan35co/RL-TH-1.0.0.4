using System;
using System.Data;
using System.Data.SqlClient; // For SQL Server
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.View.CustomDialogs;
using BarcodeVerificationSystem.View.OtherProjects.CenteryIndiaUI;
using BarcodeVerificationSystem.View.UtilityForms;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Tls;
using PdfSharp.Charting;
using UILanguage;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Npgsql;
//using Oracle.ManagedDataAccess.Client;

namespace BarcodeVerificationSystem.View
{
    public partial class frmDatabase : Form
    {
        private Form _frmJob;
        private SQLiteConnection currentConnection;
        private JArray _jArray;
        private void CallCSVClick()
        {
            if (_frmJob is FrmJob job)
            {
                job.CSVDataBaseClick();
            }
            else if (_frmJob is FrmJobCentery nutri)
            {
                nutri.CSVDataBaseClick(); // assuming it has the method
            }
            // add more else-if for other forms that have the method
        }
        public frmDatabase(Form frmJob)
        {
            _frmJob = frmJob;
            InitializeComponent();
            InitEvents();
            SetLanguage();
            this.StartPosition = FormStartPosition.CenterScreen;

            //cmbDatabaseType.Text = "SQL";
            //txtServerName.Text = "192.168.10.155\\sqlexpress";
            //txtPort.Text = "1433";
            //txtUsername.Text = "testing";
            //txtPassword.Text = "268479#Kzx";
            //txtDatabaseName.Text = "dm";

            //cmbDatabaseType.Text = "MySQL";
            //txtServerName.Text = "127.0.0.1";
            //txtPort.Text = "3306";
            //txtUsername.Text = "root";
            //txtPassword.Text = "Thinh@2024";
            //txtDatabaseName.Text = "store";
        }

        private void SetLanguage()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetLanguage()));
                return;
            }
            lblDatabaseType.Text = Lang.DatabaseType + ":"; 
            lblServerName.Text = Lang.ServerName + ":";
            lblPort.Text = Lang.Port + ":";
            lblUsername.Text = Lang.Username + ":";
            lblPassword.Text = Lang.Password + ":";
            lblDatabaseName.Text = Lang.DatabaseName + ":";
            lblTableName.Text = Lang.TableName + ":";

            this.Text = Lang.LoadDatabase;
            LoadDatabaseLabel.Text = Lang.LoadDatabase;
            btnConnect.Text = Lang.Connect;
            btnPreview.Text = Lang.Preview;
            btnSave.Text = Lang.Save;
            btnClose.Text = Lang.Close;
        }

        private void InitEvents()
        {
            cmbDatabaseType.SelectedIndexChanged += cmbDatabaseType_SelectedIndexChanged;
        }

        private void cmbDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            EmptyFields();
            string databaseType = cmbDatabaseType.Text;
            switch (databaseType.ToLower())
            {
                case "csv":
                    //_frmJob.CSVDataBaseClick();
                    CallCSVClick();
                    this.Close();
                    break;
                case "sqlite":
                    LoadSQLiteFile(); break;
                //case "production data":
                //    GetProductionDatabase();
                //    break;
                case "sql":
                case "mysql":
                default:
                    break;
            }
        }

        private void EmptyFields()
        {
            txtServerName.Text = txtPort.Text= txtUsername.Text = txtPassword.Text = txtDatabaseName.Text = txtDatabaseName.Text = "";
            cmbTableName.Items.Clear();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            string databaseType = cmbDatabaseType.Text;
            string serverName = txtServerName.Text;
            string port = txtPort.Text;
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string databaseName = txtDatabaseName.Text;

            if (string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(username))
            {
                CustomMessageBox.Show(Lang.FillLoadDBGaps, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string connectionString = GetConnectionString(databaseType, serverName, port, username, password, databaseName);

                using (IDbConnection connection = GetDatabaseConnection(databaseType, connectionString))
                {
                    var connectTask = Task.Run(() => connection.Open());
                    if (await Task.WhenAny(connectTask, Task.Delay(3000)) == connectTask) // 3 second timeout
                    {
                        await connectTask;
                        CustomMessageBox.Show(Lang.SuccessDBConnection,
                                       Lang.Info,
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Information);

                        PopulateTableNames(connection, databaseType);
                    }
                    else
                    {
                        throw new TimeoutException("Connection attempt timed out after 3 seconds");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Lang.FailedDBConnection, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error); //  Error: {ex.Message}
            }

        }

        public static string GetConnectionString(string databaseType, string serverName, string port, string username, string password, string databaseName)
        {
            switch (databaseType.ToLower())
            {
                case "sql":
                    return $"Server={serverName};Database={databaseName};User Id={username};Password={password};";
                case "mysql":
                    return $"Server={serverName};Port={port};Database={databaseName};Uid={username};Pwd={password};";
                case "postgresql":
                    return $"Host={serverName};Port={port};Database={databaseName};Username={username};Password={password};";
                case "oracle":
                    return $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={serverName})(PORT={port}))(CONNECT_DATA=(SERVICE_NAME={databaseName})));User Id={username};Password={password};";
                default:
                    return "";
            }
        }

  
        public static IDbConnection GetDatabaseConnection(string databaseType, string connectionString)
        {
            try
            {
                switch (databaseType.ToLower())
                {
                    case "sql":
                        return new SqlConnection(connectionString);
                    case "mysql":
                        return new MySqlConnection(connectionString);
                    case "postgresql":
                        return new NpgsqlConnection(connectionString);
                    //case "oracle":
                    //    return new OracleConnection(connectionString);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private async void GetProductionDatabase()
        {
            try
            {
                //this.Close();
                //Form dispatchInfo = Shared.Settings.IsManufacturingMode
                //    ? (Form)new frmGetManufacturingInfo()
                //    : new frmGetDispatchingInfo();

                //dispatchInfo.ShowDialog();

                //string apiUrl = Shared.Settings.ApiUrl + "/" + Shared.Settings.RLinkId + "/data";
                //var apiService = new ApiService();  
                //_jArray = await apiService.GetApiDataAsync(apiUrl);
                //txtDatabaseName.Text = "API Data"; 
                //CustomMessageBox.Show(Lang.SuccessDBConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void PopulateTableNames(IDbConnection connection, string databaseType)
        {
            // Clear existing items in the ComboBox
            cmbTableName.Items.Clear();

            try
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    string query;
                    switch (databaseType.ToLower())
                    {
                        case "sql":
                            query = "SELECT name FROM sys.tables WHERE type = 'U' ORDER BY name";
                            break;
                        case "mysql":
                            query = "SHOW TABLES";
                            break;
                        case "postgresql":
                            query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name";
                            break;
                        case "oracle":
                            query = "SELECT table_name FROM user_tables ORDER BY table_name";
                            break;
                        default:
                            return;
                    }

                    command.CommandText = query;
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tableName = reader[0].ToString();
                            cmbTableName.Items.Add(tableName);
                        }
                    }
                }

                if (cmbTableName.Items.Count > 0)
                {
                    cmbTableName.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Lang.FailedRetrieveTable, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReview_Click(object sender, EventArgs e)
        {
            string databaseType = cmbDatabaseType.Text;
            string serverName = txtServerName.Text;
            string port = txtPort.Text;
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string databaseName = txtDatabaseName.Text;
            string tableName = cmbTableName?.SelectedItem?.ToString();

            if (databaseType.ToLower() == "get api")
            {
                frmTableData tableDataForm1 = new frmTableData("Api Data", databaseType, _jArray);
                tableDataForm1.ShowDialog();
                return;
            }

            if (cmbTableName.SelectedItem == null)
            {
                CustomMessageBox.Show(Lang.PleaseSelectTable, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }




            string connectionString = GetConnectionString(databaseType, serverName, port, username, password, databaseName);
            
            if(databaseType.ToLower() == "sqlite")
            {
                frmTableData tableDataForm1 = new frmTableData(tableName, currentConnection);
                tableDataForm1.ShowDialog();
                return;
            }
            
            frmTableData tableDataForm = new frmTableData(connectionString, tableName, databaseType);
            tableDataForm.ShowDialog();
        }

        private void btnSaveDB_Click(object sender, EventArgs e)
        {
            string databaseType = cmbDatabaseType.Text;
            string serverName = txtServerName.Text;
            string port = txtPort.Text;
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string databaseName = txtDatabaseName.Text;
            string tableName = cmbTableName?.SelectedItem?.ToString();

            switch (databaseType.ToLower())
            {
                case "sql":
                    ExportDatabaseByConnection(databaseType, serverName, port, username, password, databaseName, tableName);
                    this.Close();
                    break;
                case "mysql":
                    ExportDatabaseByConnection(databaseType, serverName, port, username, password, databaseName, tableName);
                    this.Close();
                    break;
                case "sqlite":
                    ExportSQLiteTable(cmbTableName.SelectedItem.ToString());
                    this.Close();
                    break;
                case "production data":
                    if (_jArray != null && _jArray.Count > 0)
                    {
                        ExportApiDataToCsv();
                    }
                    else
                    {
                        CustomMessageBox.Show("Lang.NoDataFromAPI", Lang.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    this.Close();
                    break;
                default:
                    break;

            }

        }

        private void LoadSQLiteFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "SQLite Database (*.db)|*.db|All Files (*.*)|*.*";
                openFileDialog.Title = "Select SQLite Database File";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Default to Documents folder

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string sqliteFilePath = openFileDialog.FileName;
                    txtDatabaseName.Text = Path.GetFileName(sqliteFilePath);

                    try
                    {
                        string connectionString = $"Data Source={sqliteFilePath};Version=3;";
                        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                        {
                            connection.Open();
                            currentConnection?.Close();
                            currentConnection = new SQLiteConnection($"Data Source={sqliteFilePath};Version=3;");
                            currentConnection.Open();

                            CustomMessageBox.Show(Lang.SuccessDBConnection, Lang.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            PopulateSQLiteTableNames(connection);
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show(Lang.FailedLoadSQLite, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void PopulateSQLiteTableNames(SQLiteConnection connection)
        {
            cmbTableName.Items.Clear();

            try
            {
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tableName = reader["name"].ToString();
                            cmbTableName.Items.Add(tableName);
                        }
                    }
                }

                if (cmbTableName.Items.Count > 0)
                {
                    cmbTableName.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Lang.FailedRetrieveTable, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportSQLiteTable(string tableName)
        {
            try
            {
                if (currentConnection == null || currentConnection.State != ConnectionState.Open)
                {
                    CustomMessageBox.Show(Lang.FailedLoadSQLite, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = $"SELECT * FROM \"{tableName}\"";

                using (SQLiteCommand command = new SQLiteCommand(query, currentConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dataTable.Columns.Add(reader.GetName(i));
                        }

                        while (reader.Read())
                        {
                            DataRow row = dataTable.NewRow();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[i] = reader[i]?.ToString() ?? "";
                            }
                            dataTable.Rows.Add(row);
                        }

                        ExportToCsv(dataTable, tableName);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{Lang.FailedRetrieveTable} : '{tableName}': {ex.Message}", Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportApiDataToCsv()
        {
            DataTable table = new DataTable();

            // Dynamically create columns
            foreach (JProperty prop in _jArray[0])
            {
                table.Columns.Add(prop.Name);
            }

            // Add rows
            foreach (JObject row in _jArray)
            {
                DataRow dataRow = table.NewRow();
                foreach (JProperty prop in row.Properties())
                {
                    dataRow[prop.Name] = prop.Value.ToString();
                }
                table.Rows.Add(dataRow);
            }
            ExportToCsv(table, "tableName");
        }


        public static DataTable ExportDatabaseByConnection(string databaseType, string serverName, string port, string  username, string password, string databaseName, string tableName) 
        {
            string connectionString = GetConnectionString(databaseType, serverName, port, username, password, databaseName);
            try
            {
                using (IDbConnection connection = GetDatabaseConnection(databaseType, connectionString))
                {
                    connection.Open();
                    
                    // Build query with WHERE clause to filter rows where qr_print is 'N' or not 'Y'
                    string tableQuote = databaseType.ToLower() == "sql" ? $"[{tableName}]" : $"`{tableName}`";
                    string qrPrintQuote = databaseType.ToLower() == "sql" ? "[qr_print]" : "`qr_print`";
                    string query = $"SELECT * FROM {tableQuote} WHERE {qrPrintQuote} != 'Y' OR {qrPrintQuote} IS NULL";
                    
                    using (IDbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (IDataReader reader = command.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                dataTable.Columns.Add(reader.GetName(i));
                            }

                            while (reader.Read())
                            {
                                DataRow row = dataTable.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[i] = reader[i]?.ToString() ?? "";
                                }
                                dataTable.Rows.Add(row);
                            }

                            ExportToCsv(dataTable, tableName);
                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{Lang.FailedRetrieveTable}: {ex.Message}", Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        /// <summary>
        /// Gets the index of the column with the specified name in the DataTable
        /// </summary>
        /// <param name="dataTable">The DataTable to search</param>
        /// <param name="columnName">The name of the column to find</param>
        /// <returns>The index of the column if found, otherwise -1</returns>
        public static int GetColumnIndexByName(DataTable dataTable, string columnName)
        {
            if (dataTable == null || string.IsNullOrEmpty(columnName))
                return -1;

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (string.Equals(dataTable.Columns[i].ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the index of the column matching Shared.Settings.CenterIndiaModel.TemplateName
        /// </summary>
        /// <param name="dataTable">The DataTable to search</param>
        /// <returns>The index of the column if found, otherwise -1</returns>
        public static int GetTemplateColumnIndex(DataTable dataTable)
        {
            string templateName = Shared.Settings.CenterIndiaModel?.TemplateName;
            if (string.IsNullOrEmpty(templateName))
                return -1;

            return GetColumnIndexByName(dataTable, templateName);
        }

        private static void ExportToCsv(DataTable dataTable, string tableName)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                CustomMessageBox.Show(Lang.CanNotFindDatabase, Lang.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fileName = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            //string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "R-Link");
            string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "R-Link", "Database");

            if (!Directory.Exists(documentsPath))
            {
                Directory.CreateDirectory(documentsPath);
            }

            string filePath = Path.Combine(documentsPath, fileName);
            Console.WriteLine(documentsPath);
            try
            {

                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        sw.Write(dataTable.Columns[i].ColumnName);
                        if (i < dataTable.Columns.Count - 1)
                            sw.Write(",");
                    }
                    sw.WriteLine();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            sw.Write(EscapeCsvValue(row[i]?.ToString()));
                            if (i < dataTable.Columns.Count - 1)
                                sw.Write(",");
                        }
                        sw.WriteLine();
                    }
                }
                Shared.databasePath = filePath;

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Lang.CanNotFindDatabase, Lang.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void ExportSQLiteToCSV(string dbPath, string tableName, string csvPath)
        {
            string connectionString = $"Data Source={dbPath};Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM {tableName}";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                using (StreamWriter writer = new StreamWriter(csvPath, false, Encoding.UTF8))
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        writer.Write(reader.GetName(i));
                        if (i < reader.FieldCount - 1)
                            writer.Write(",");
                    }

                    writer.WriteLine();
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            writer.Write(reader[i].ToString());
                            if (i < reader.FieldCount - 1)
                                writer.Write(",");
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click_Click(object sender, EventArgs e)
        {
            btnSaveDB_Click(sender, e);
        }
    }
}