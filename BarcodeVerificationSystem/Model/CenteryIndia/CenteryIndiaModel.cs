using System;
using System.Xml.Serialization;

namespace BarcodeVerificationSystem.Model
{
    [Serializable]
    public class CenteryIndiaModel
    {
        private string _templateName = "";
        public string TemplateName
        {
            get { return _templateName; }
            set { _templateName = value; }
        }

        private string _splitResult = "";
        public string SplitResult
        {
            get { return _splitResult; }
            set { _splitResult = value; }
        }

        private bool _isCamera = true;
        public bool IsCamera
        {
            get { return _isCamera; }
            set { _isCamera = value; }
        }

        private bool _isScanner = false;
        public bool IsScanner
        {
            get { return _isScanner; }
            set { _isScanner = value; }
        }

        private string _databaseType = "MySQL";
        public string DatabaseType
        {
            get { return _databaseType; }
            set { _databaseType = value; }
        }

        private string _serverName = "";
        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        private string _port = "";
        public string Port
        {
            get { return _port; }
            set { _port = value; }
        }

        private string _username = "";
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password = "";
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _databaseName = "";
        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; }
        }

        private string _tableName = "";
        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        private string _lineName = "";
        public string LineName
        {
            get { return _lineName; }
            set { _lineName = value; }
        }

        private int _retentionDays = 190;
        public int RetentionDays
        {
            get { return _retentionDays; }
            set { _retentionDays = value; }
        }
    }
}

