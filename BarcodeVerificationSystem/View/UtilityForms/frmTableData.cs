using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using System;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using UILanguage;
using Newtonsoft.Json.Linq;

namespace BarcodeVerificationSystem.View
{
    public partial class frmTableData : Form
    {
        private string _connectionString;
        private string _tableName;
        private string _databaseType;
        private JArray _jArray;
        private DataTable _allDataTable; // Store all data for pagination
        private int _currentPage = 1;
        private const int _pageSize = 30;
        private int _totalPages = 1;

        public frmTableData(string connectionString, string tableName, string databaseType)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = Lang.previewdatabase;
            _connectionString = connectionString;
            _tableName = tableName;
            _databaseType = databaseType;
            InitPagination();
            LoadTableData();
        }

        public frmTableData(string tableName, string databaseType, JArray array)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            _jArray = array;
            InitPagination();
            LoadTableApi();
        }

        public frmTableData(string tableName, SQLiteConnection currentConnection)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            InitPagination();
            LoadTableDataSqLite(tableName, currentConnection);
        }

        private void LoadTableDataSqLite(string tableName, SQLiteConnection currentConnection)
        {
            try
            {
                if (currentConnection == null || currentConnection.State != ConnectionState.Open)
                {
                    MessageBox.Show("No open SQLite connection available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Load all data first
                string query = $"SELECT * FROM \"{tableName}\"";

                using (SQLiteCommand command = new SQLiteCommand(query, currentConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        _allDataTable = new DataTable();
                        _allDataTable.Load(reader);
                    }
                }

                _totalPages = (int)Math.Ceiling((double)_allDataTable.Rows.Count / _pageSize);
                _currentPage = 1;
                UpdatePaginationUI();
                LoadCurrentPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data from table '{tableName}': {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTableApi()
        {
            _allDataTable = new DataTable();

            // Dynamically create columns
            if (_jArray != null && _jArray.Count > 0)
            {
                foreach (JProperty prop in _jArray[0])
                {
                    _allDataTable.Columns.Add(prop.Name);
                }

                // Add rows
                foreach (JObject row in _jArray)
                {
                    DataRow dataRow = _allDataTable.NewRow();
                    foreach (JProperty prop in row.Properties())
                    {
                        dataRow[prop.Name] = prop.Value.ToString();
                    }
                    _allDataTable.Rows.Add(dataRow);
                }
            }

            _totalPages = (int)Math.Ceiling((double)_allDataTable.Rows.Count / _pageSize);
            _currentPage = 1;
            UpdatePaginationUI();
            LoadCurrentPage();
        }


        private void LoadTableData()
        {
            try
            {
                using (IDbConnection connection = GetDatabaseConnection(_databaseType, _connectionString))
                {
                    connection.Open();

                    // PostgreSQL yêu cầu tên bảng trong dấu ngoặc kép
                    string query;
                    switch (_databaseType.ToLower())
                    {
                        case "postgresql":
                            query = $"SELECT * FROM \"{_tableName}\"";
                            break;
                        case "mysql":
                            query = $"SELECT * FROM `{_tableName}`";
                            break;
                        default:
                            query = $"SELECT * FROM [{_tableName}]";
                            break;
                    }

                    using (IDbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (IDataReader reader = command.ExecuteReader())
                        {
                            _allDataTable = new DataTable();
                            _allDataTable.Load(reader);
                        }
                    }
                }

                _totalPages = (int)Math.Ceiling((double)_allDataTable.Rows.Count / _pageSize);
                _currentPage = 1;
                UpdatePaginationUI();
                LoadCurrentPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load table data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCurrentPage()
        {
            if (_allDataTable == null) return;

            DataTable pageData = _allDataTable.Clone();
            int startIndex = (_currentPage - 1) * _pageSize;
            int endIndex = Math.Min(startIndex + _pageSize, _allDataTable.Rows.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                pageData.ImportRow(_allDataTable.Rows[i]);
            }

            dataGridView.DataSource = pageData;
        }

        private void InitPagination()
        {
            if (btnPrevious != null && btnNext != null && lblPageInfo != null)
            {
                btnPrevious.Click += BtnPrevious_Click;
                btnNext.Click += BtnNext_Click;
            }
        }

        private void UpdatePaginationUI()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdatePaginationUI()));
                return;
            }

            if (btnPrevious != null)
            {
                btnPrevious.Enabled = _currentPage > 1;
            }
            if (btnNext != null)
            {
                btnNext.Enabled = _currentPage < _totalPages && _totalPages > 0;
            }
            if (lblPageInfo != null)
            {
                int totalRows = _allDataTable?.Rows.Count ?? 0;
                if (totalRows > 0)
                {
                    int startRow = (_currentPage - 1) * _pageSize + 1;
                    int endRow = Math.Min(_currentPage * _pageSize, totalRows);
                    lblPageInfo.Text = $"Page {_currentPage} of {_totalPages} (Rows {startRow}-{endRow} of {totalRows})";
                }
                else
                {
                    lblPageInfo.Text = "No data available";
                }
            }
        }

        private void BtnPrevious_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadCurrentPage();
                UpdatePaginationUI();
            }
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                LoadCurrentPage();
                UpdatePaginationUI();
            }
        }

        private IDbConnection GetDatabaseConnection(string databaseType, string connectionString)
        {
            switch (databaseType.ToLower())
            {
                case "sql":
                    return new SqlConnection(connectionString);
                case "mysql":
                    return new MySqlConnection(connectionString);
                case "postgresql":
                    return new Npgsql.NpgsqlConnection(connectionString);
                default:
                    throw new ArgumentException($"Unsupported database type: {databaseType}");
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
 
    }
}