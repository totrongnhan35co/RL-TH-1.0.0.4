using BarcodeVerificationSystem.Controller;

namespace BarcodeVerificationSystem.Model
{
    public class UserDataModel
    {
        private string _FullName;

        private string _UserName;

        private string _Password;

        private int _Role;

        private string _RoleName;

        public UserDataModel(string FullName, string UserName, string Password, int Role)
        {
            _FullName = FullName;
            _UserName = UserName;
            _Password = Password;
            _Role = Role;
        }
        public UserDataModel()
        {
        }

        public string FullName { get => _FullName; set => _FullName = value; }
        public string UserName { get => _UserName; set => _UserName = value; }
        public string Password { get => _Password; set => _Password = value; }
        public int Role { get => _Role; set => _Role = value; }
        public string RoleName { get => Role == 0 || Role == 1000? _RoleName = "Administrator": _RoleName = "Operator";}
        public string UserId { get; set; }
        public string OperatorUserId { get; set; }

        public static string GeneralInsertCommand(string fullname, string username, string password, int role)
        {
            string insertCommand = string.Format("insert into tbl_account( fullname, username, password, role) values( '{0}', '{1}', '{2}', '{3}');",
                fullname, SecurityController .Encrypt(username, "rynan_encrypt_remember"), SecurityController.Encrypt(password, "rynan_encrypt_remember"), role);
            return insertCommand;
        }
        public static string GeneralEditCommand(string fullname, string username, string password, int role)
        {
            string editCommand = string.Format("update tbl_account set fullname = '{0}', password = '{1}',role = '{2}' where username = '{3}';",
                fullname, SecurityController.Encrypt(password, "rynan_encrypt_remember"), role, username);
            return editCommand;
        }
        public static string GeneralEditCommand2(string fullname, string username, int role)
        {
            string editCommand = string.Format("update tbl_account set fullname = '{0}', role = '{1}' where username = '{2}';",
                fullname, role, username);
            return editCommand;
        }
        public static string GeneralDeleteCommand(string username)
        {
            string deleteCommand = string.Format("delete from tbl_account where username = '{0}';", username);
            return deleteCommand;
        }
        public static string GeneralCheckPassCommand(string username, string password)
        {
            string checkPassCommand = string.Format("select* from tbl_account where username = '{0}' and password = '{1}';",username, password);
            return checkPassCommand;
        }
        public static string GeneralChangePassCommand(string username, string password)
        {
            string chabgePassCommand = string.Format("update tbl_account set password = '{0}' where username = '{1}';", password, username);
            return chabgePassCommand;
        }
        public static string GeneralCheckExistUsernameCommand(string username)
        {
            string getIdCommand = string.Format("select* from tbl_account where username = '{0}';", username);
            return getIdCommand;
        }
    }
}
