using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Data;
using OperationLog.Model;
using System.Security.Permissions;
using CommonVariable;

[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, ViewAndModify = "HKEY_CURRENT_USER")]
namespace OperationLog.Controller
{
    public class LoggingController
    {
        private static String _KeyAccess = "_rynan_loggin_access_control_management_";
        private static bool _AllowAccess = false;
        private static String _LoggingPath = "";
        private static void CreateDefaultDatabase()
        {
            //check security to access
            if (!_AllowAccess) {
                return;
            }

            String path = CommVariables.PathHistoriesApp;
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            var builder = new SQLiteConnectionStringBuilder
            {

                DataSource = path + "HistoryDB.db",
                Version = 3,                
                Password = "pass.security.Rynan@0988345294",
            };

            string connectionString = builder.ToString();

            using (SQLiteConnection databaseConnection = new SQLiteConnection(connectionString))
            {
                //set password to database
                //databaseConnection.SetPassword("pass.security.Rynan@0988345294");
                // Open the connection to the new database
                databaseConnection.Open();
                // Import the SQL file database dump into the in-memory database
                SQLiteCommand command = databaseConnection.CreateCommand();
                string importedSql = "CREATE TABLE tbl_history (id	INTEGER PRIMARY KEY AUTOINCREMENT,logtype	INTEGER,keyword	TEXT,command	TEXT,message	TEXT,date	TEXT,user	TEXT);";
                command.CommandText = importedSql;
                command.ExecuteNonQuery();
            }
        }

        public static bool LoginToAccess(String key) {
            if (key == _KeyAccess)
            {
                _AllowAccess = true;
                _LoggingPath = CommVariables.PathHistoriesApp + "\\HistoryDB.db";
            }
            else
            {
                _AllowAccess = false;
                _LoggingPath = "";
            }
            return _AllowAccess;
        }

        public static void AddHistory(LoggingModel model) {
            //check security
            if (!_AllowAccess)
            {
                return;
            }
            try
            {
                //check database
                if (!File.Exists(_LoggingPath))
                {
                    CreateDefaultDatabase();
                }

                //insert data to database command
                String insertCommand = model.GeneralInsertCommand();
                var builder = new SQLiteConnectionStringBuilder
                {

                    DataSource = _LoggingPath,
                    Version = 3,
                    Password = "pass.security.Rynan@0988345294",
                };

                string connectionString = builder.ToString();

                using (SQLiteConnection databaseConnection = new SQLiteConnection(connectionString))
                {
                    // Open the connection to the new database
                    databaseConnection.Open();

                    // Import the SQL file database dump into the in-memory database
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    command.CommandText = insertCommand;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void SaveHistory(String command, String keyword, String message, String username, LoggingType logtype)
        {
            AddHistory(
                new LoggingModel
                {
                    Command = command,
                    Date = DateTime.Now,
                    KeyWord = keyword,
                    LogType = logtype,
                    Message = message,
                    User = username
                }
                );
        }

        public static List<LoggingModel> ListHistory(LoggingType type = LoggingType.None)
        {
            //check security
            if (!_AllowAccess)
            {
                return null;
            }
            List<LoggingModel> result = new List<LoggingModel>();
            try
            {
                //query
                String queryCommand = String.Format("select * from tbl_history where logtype={0}", (int)type);
                var builder = new SQLiteConnectionStringBuilder
                {

                    DataSource = _LoggingPath,
                    Version = 3,
                    Password = "pass.security.Rynan@0988345294",
                };

                string connectionString = builder.ToString();

                using (SQLiteConnection databaseConnection = new SQLiteConnection(connectionString))
                {
                    // Open the connection to the new database
                    databaseConnection.Open();

                    // Import the SQL file database dump into the in-memory database
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    command.CommandText = queryCommand;
                    SQLiteDataAdapter odbcDataAdapter_Object = new SQLiteDataAdapter(command);
                    DataTable tableData = new DataTable();
                    odbcDataAdapter_Object.Fill(tableData);
                    foreach (DataRow dr in tableData.Rows)
                    {
                        LoggingModel model = new LoggingModel
                        {
                            User = dr["User"].ToString(),
                            Message = dr["Message"].ToString(),
                            LogType = (LoggingType)int.Parse(dr["LogType"].ToString()),
                            KeyWord = dr["KeyWord"].ToString(),
                            Id = int.Parse(dr["Id"].ToString()),
                            //Date
                            Command = dr["Command"].ToString()
                        };
                        result.Add(model);
                    }
                }
            }
            catch { }
            return result;
        }

        public static List<LoggingModel> ListHistory(DateTime dateFrom, DateTime dateTo, List<LoggingType> searchTypes)
        {
            //check security
            if (!_AllowAccess)
            {
                return null;
            }
            List<LoggingModel> result = new List<LoggingModel>();
            try
            {
                #region Loggtype
                List<int> logtypes = new List<int>();
                if (searchTypes == null || searchTypes.Count == 0)
                {
                    logtypes.Add(0);
                }
                else
                {
                    foreach (LoggingType t in searchTypes)
                    {
                        logtypes.Add((int)t);
                    }
                }
                #endregion Loggtype

                String types =  "(" + String.Join(",", logtypes) + ")";
                //String strFrom = dateFrom.ToString("yyyy/MM/dd") + " 00:00:00";
                //String strTo = dateTo.ToString("yyyy/MM/dd") + " 23:59:59";
                DateTime dayFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day);
                DateTime dayTo = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, 23, 59, 59);
                long strFrom = dayFrom.Ticks;
                long strTo = dayTo.Ticks;
                //query
                String queryCommand = String.Format("select * from tbl_history where logtype in{0} and date >= '{1}' and date <= '{2}'", types, strFrom, strTo);
                String queryTest = String.Format("select * from tbl_history");
                var builder = new SQLiteConnectionStringBuilder
                {

                    DataSource = _LoggingPath,
                    Version = 3,
                    Password = "pass.security.Rynan@0988345294",
                };

                string connectionString = builder.ToString();

                using (SQLiteConnection databaseConnection = new SQLiteConnection(connectionString))
                {
                    // Open the connection to the new database
                    databaseConnection.Open();

                    // Import the SQL file database dump into the in-memory database
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    command.CommandText = queryCommand;
                   // command.CommandText = queryTest;
                    SQLiteDataAdapter odbcDataAdapter_Object = new SQLiteDataAdapter(command);
                    DataTable tableData = new DataTable();
                    odbcDataAdapter_Object.Fill(tableData);
                    foreach (DataRow dr in tableData.Rows)
                    {
                        LoggingModel model = new LoggingModel(dr);
                        result.Add(model);
                    }
                }
            }
            catch { }
            return result;
        }

        public static bool ClearHistory(DateTime dateFrom, DateTime dateTo, List<LoggingType> searchTypes)
        {
            //check security
            if (!_AllowAccess)
            {
                return false;
            }
            List<LoggingModel> result = new List<LoggingModel>();
            try
            {
                #region Loggtype
                List<int> logtypes = new List<int>();
                if (searchTypes == null || searchTypes.Count == 0)
                {
                    logtypes.Add(0);
                }
                else
                {
                    foreach (LoggingType t in searchTypes)
                    {
                        logtypes.Add((int)t);
                    }
                }
                #endregion Loggtype
                String types = "(" + String.Join(",", logtypes) + ")";
                //String strFrom = dateFrom.ToString("yyyy/MM/dd") + " 00:00:00";
                //String strTo = dateTo.ToString("yyyy/MM/dd") + " 23:59:59";
                DateTime dayFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day);
                DateTime dayTo = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, 23, 59, 59);
                long strFrom = dayFrom.Ticks;
                long strTo = dayTo.Ticks;
                //query
                String queryCommand = String.Format("delete from tbl_history where logtype in{0} and date >= '{1}' and date <= '{2}'", types, strFrom, strTo);//logtypes <=> types
                //String queryTest = String.Format("delete from tbl_history");
                var builder = new SQLiteConnectionStringBuilder
                {

                    DataSource = _LoggingPath,
                    Version = 3,
                    Password = "pass.security.Rynan@0988345294",
                };

                string connectionString = builder.ToString();

                using (SQLiteConnection databaseConnection = new SQLiteConnection(connectionString))
                {
                    // Open the connection to the new database
                    databaseConnection.Open();
                    // Import the SQL file database dump into the in-memory database
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    command.CommandText = queryCommand;
                    //command.CommandText = queryTest;
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex){ }
            return false;
        }
    }
}
