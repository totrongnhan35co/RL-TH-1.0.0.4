using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using CommonVariable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace BarcodeVerificationSystem.Controller
{
    public class UserController
    {
        private static readonly bool _AllowAccess = false;
        public static string LogedInUsername = "";
        public static void CreateDefaultDatabase()
        {
            if (!_AllowAccess) {}
            string path = CommVariables.PathAccountsApp;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = path + "AccountDB.db",
                Version = 3,
                Password = "pass.security.Rynan@0988345294",
            };

            string connectionString = builder.ToString();

            using (var databaseConnection = new SQLiteConnection(connectionString))
            {
                databaseConnection.Open();
                SQLiteCommand command = databaseConnection.CreateCommand();

                if (ProjectLabel.IsDefault)
                {
                    string addSuperAdminSql = UserDataModel.GeneralInsertCommand("Demonstration", "demo", "123456", 0);
                    string addAdminSql = UserDataModel.GeneralInsertCommand("Administrator", "admin", "123456", 0);
                    string addOperatorSql = UserDataModel.GeneralInsertCommand("Operator", "operator", "123456", 1);
                    string importedSql = "CREATE TABLE tbl_account (id	INTEGER PRIMARY KEY AUTOINCREMENT, fullname	TEXT, username	TEXT,password	TEXT,role	INTEGER);";
                    command.CommandText = importedSql + addAdminSql + addOperatorSql + addSuperAdminSql;
                }
                else{
                    string addAdminSql = UserDataModel.GeneralInsertCommand("Administrator", "Admin", "Admin@1234", 0);
                    string addOperatorSql = UserDataModel.GeneralInsertCommand("Operator", "Operator", "Operator@123", 1);
                    string adSupportSql = UserDataModel.GeneralInsertCommand("Administrator", "Support", "Support@2025", 0);
                    string importedSql = "CREATE TABLE tbl_account (id	INTEGER PRIMARY KEY AUTOINCREMENT, fullname	TEXT, username	TEXT,password	TEXT,role	INTEGER);";
                    command.CommandText = importedSql + addAdminSql + addOperatorSql + adSupportSql;
                }

                //string addCreatorSql = UserDataModel.GeneralInsertCommand("Creator", "creator", "123456", 3);

                command.ExecuteNonQuery();
            }
        }
        public static UserDataModel Login(string username, string password)
        {
            string path = CommVariables.PathAccountsApp;
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = path + "AccountDB.db",
                Version = 3,
                Password = "pass.security.Rynan@0988345294",
            };

            string connectionString = builder.ToString();

            using (var databaseConnection = new SQLiteConnection(connectionString))
            {
                try
                {
                    databaseConnection.Open();
                     SQLiteCommand command = databaseConnection.CreateCommand();
                    var accountDataTable = new DataTable();
                    string importedSql = $"select * from tbl_account";
                    command.CommandText = importedSql;
                    SQLiteDataReader reader = command.ExecuteReader();
                    accountDataTable.Load(reader);
                    string user = "";
                    string pass = "";
                    int role = 1;
                    foreach (DataRow row in accountDataTable.Rows)
                    {
                        user = SecurityController.Decrypt(row["username"].ToString(), "rynan_encrypt_remember");
                        pass = SecurityController.Decrypt(row["password"].ToString(), "rynan_encrypt_remember");
                        role = int.Parse(row["role"].ToString());
                        string rawUid = row.Table.Columns.Contains("user_id") && row["user_id"] != DBNull.Value
                            ? row["user_id"].ToString() : "";
                        // Nếu chưa có user_id hợp lệ (rỗng hoặc trùng username), dùng ACC001 cho offline
                        string uid = string.IsNullOrWhiteSpace(rawUid) || rawUid.Equals(user, StringComparison.OrdinalIgnoreCase)
                            ? "ACC001"
                            : rawUid;
                        if (user == username && pass == password && role != 1000)
                        {
                            var loginUser = new UserDataModel
                            {
                                UserName = row["username"].ToString(),
                                FullName = row["fullname"].ToString(),
                                Role = role,
                                Password = row["password"].ToString(),
                                UserId = uid,
                                OperatorUserId = uid
                            };
                            return loginUser;
                        }
                        else if (user == username && role == 1000)
                        {
                            pass += DateTime.Now.ToString("dd/yy");
                            if (pass == password)
                            {
                                var loginUser = new UserDataModel
                                {
                                    UserName = row["username"].ToString(),
                                    FullName = row["fullname"].ToString(),
                                    Role = role,
                                    Password = row["password"].ToString(),
                                    UserId = uid,
                                    OperatorUserId = uid
                                };
                                return loginUser;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                catch
                {

                }
            }
            return null;
        }
        /// <summary>
        /// Load account
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<UserDataModel> LoadAccount(string key)
        {
            var listUser = new List<UserDataModel>();
            string path = CommVariables.PathAccountsApp;
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = path + "AccountDB.db",
                Version = 3,
                Password = "pass.security.Rynan@0988345294",
            };

            string connectionString = builder.ToString();

            using (var databaseConnection = new SQLiteConnection(connectionString))
            {

                try
                {
                    databaseConnection.Open();
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    var accountDataTable = new DataTable();
                    string importedSql = $"select * from tbl_account";
                    command.CommandText = importedSql;
                    SQLiteDataReader reader = command.ExecuteReader();
                    accountDataTable.Load(reader);
                    foreach (DataRow row in accountDataTable.Rows)
                    {
                        string userName = SecurityController.Decrypt(row["username"].ToString(), "rynan_encrypt_remember");
                        int passLenght = SecurityController.Decrypt(row["password"].ToString(), "rynan_encrypt_remember").Count();
                        int role = int.Parse(row["role"].ToString());
                        if (userName.ToLower().Contains(key))
                        {
                            if (role != 1000)
                            {
                                var User = new UserDataModel
                                {
                                    UserName = userName,
                                    Password = new string('*', passLenght),
                                    FullName = row["fullname"].ToString(),
                                    Role = int.Parse(row["role"].ToString())
                                };
                                listUser.Add(User);
                            }
                            else if (Shared.LoggedInUser != null && role == 1000 && Shared.LoggedInUser.Role == 1000)
                            {
                                var User = new UserDataModel
                                {
                                    UserName = userName,
                                    Password = new string('*', passLenght),
                                    FullName = row["fullname"].ToString(),
                                    Role = int.Parse(row["role"].ToString())
                                };
                                listUser.Add(User);
                            }
                        }
                    }
                }
                catch
                {

                }
            }
            return listUser;
        }
        /// <summary>
        /// Add new account
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool AddAccount(string fullname, string username, string password, int role)
        {
            try
            {
                string path = CommVariables.PathAccountsApp;
                var builder = new SQLiteConnectionStringBuilder
                {

                    DataSource = path + "AccountDB.db",
                    Version = 3,
                    Password = "pass.security.Rynan@0988345294",
                };

                string connectionString = builder.ToString();

                using (var databaseConnection = new SQLiteConnection(connectionString))
                {
                    databaseConnection.Open();
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    string importedSql = UserDataModel.GeneralInsertCommand(fullname, username, password, role);
                    command.CommandText = importedSql;
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Edit account
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        /// <param name="isChangePassword"></param>
        /// <returns></returns>
        public static bool EditAccount(string fullname, string username, string password, int role, bool isChangePassword = true)
        {
            try
            {
                string path = CommVariables.PathAccountsApp;
                var builder = new SQLiteConnectionStringBuilder
                {
                    DataSource = path + "AccountDB.db",
                    Version = 3,
                    Password = "pass.security.Rynan@0988345294",
                };

                string connectionString = builder.ToString();

                using (var databaseConnection = new SQLiteConnection(connectionString))
                {
                    databaseConnection.Open();
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    string importedSql = UserDataModel.GeneralEditCommand(fullname, username, password, role);
                    command.CommandText = importedSql;
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Delete account
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool DeleteAccount(string username)
        {
            try
            {
                string path = CommVariables.PathAccountsApp;
                var builder = new SQLiteConnectionStringBuilder
                {
                    DataSource = path + "AccountDB.db",
                    Version = 3,
                    Password = "pass.security.Rynan@0988345294",
                };

                string connectionString = builder.ToString();

                using (var databaseConnection = new SQLiteConnection(connectionString))
                {
                    databaseConnection.Open();
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    string importedSql = UserDataModel.GeneralDeleteCommand(username);
                    command.CommandText = importedSql;
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Check exits account
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static UserDataModel CheckExistUserName(string username)
        {
            string path = CommVariables.PathAccountsApp;
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = path + "AccountDB.db",
                Version = 3,
                Password = "pass.security.Rynan@0988345294",
            };

            string connectionString = builder.ToString();

            using (var databaseConnection = new SQLiteConnection(connectionString))
            {

                try
                {
                    databaseConnection.Open();
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    DataTable accountDataTable = new DataTable();
                    string importedSql = $"select * from tbl_account";
                    command.CommandText = importedSql;
                    SQLiteDataReader reader = command.ExecuteReader();
                    accountDataTable.Load(reader);
                    string user = "";
                    string pass = "";
                    foreach (DataRow row in accountDataTable.Rows)
                    {
                        user = SecurityController.Decrypt(row["username"].ToString(), "rynan_encrypt_remember");
                        pass = SecurityController.Decrypt(row["password"].ToString(), "rynan_encrypt_remember");
                        if (user == username)
                        {
                            var loginUser = new UserDataModel
                            {
                                UserName = row["username"].ToString(),
                                FullName = row["fullname"].ToString(),
                                Role = int.Parse(row["role"].ToString()),
                                Password = row["password"].ToString()
                            };
                            return loginUser;
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        /// <summary>
        /// Check password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool CheckCorrectPassword(string password)
        {
            if(Shared.LoggedInUser != null)
            {
                string currentPass = SecurityController.Decrypt(Shared.LoggedInUser.Password, "rynan_encrypt_remember");
                if (currentPass == password)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public static bool ResetPassword(string username, string newPassword) => true;
        /// <summary>
        /// Change password account
        /// </summary>
        /// <param name="username"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public static bool ChangePassword(string username, string newPassword)
        {
            try
            {
                string path = CommVariables.PathAccountsApp;
                var builder = new SQLiteConnectionStringBuilder
                {

                    DataSource = path + "AccountDB.db",
                    Version = 3,
                    Password = "pass.security.Rynan@0988345294",
                };

                string connectionString = builder.ToString();

                using (var databaseConnection = new SQLiteConnection(connectionString))
                {
                    databaseConnection.Open();
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    string importedSql = UserDataModel.GeneralChangePassCommand(username, newPassword);
                    command.CommandText = importedSql;
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Check and prepare data
        /// </summary>
        /// <param name="securityCode"></param>
        /// <returns></returns>
        public static int CheckAndPrepareData(string securityCode) => 0;

        public static void UpdateUserId(string username, string userId)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId)) return;
            string path = CommVariables.PathAccountsApp;
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = path + "AccountDB.db",
                Version = 3,
                Password = "pass.security.Rynan@0988345294",
            };
            using (var conn = new SQLiteConnection(builder.ToString()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('tbl_account') WHERE name='user_id'";
                    if ((long)cmd.ExecuteScalar() == 0)
                    {
                        cmd.CommandText = "ALTER TABLE tbl_account ADD COLUMN user_id TEXT";
                        cmd.ExecuteNonQuery();
                    }
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE tbl_account SET user_id=@id WHERE username=@user";
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.Parameters.AddWithValue("@user", SecurityController.Encrypt(username, "rynan_encrypt_remember"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static string GetCachedUserId(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return "";
            string path = CommVariables.PathAccountsApp;
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = path + "AccountDB.db",
                Version = 3,
                Password = "pass.security.Rynan@0988345294",
            };
            try
            {
                using (var conn = new SQLiteConnection(builder.ToString()))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT user_id FROM tbl_account WHERE username=@user";
                        cmd.Parameters.AddWithValue("@user", SecurityController.Encrypt(username, "rynan_encrypt_remember"));
                        var val = cmd.ExecuteScalar();
                        if (val != null && val != DBNull.Value)
                            return val.ToString() ?? "";
                    }
                }
            }
            catch { }
            return "";
        }

    }
}
