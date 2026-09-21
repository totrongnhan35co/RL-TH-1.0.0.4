using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;

namespace OperationLog.Model
{
    public class LoggingModel
    {
        #region Properties

        //private String _DateFormat = "yyyy/MM/dd HH:mm:ss";
        private CultureInfo _DateProvider = CultureInfo.InvariantCulture;

        private int _Id;

        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        private LoggingType _LogType = LoggingType.None;

        public LoggingType LogType
        {
            get { return _LogType; }
            set { _LogType = value; }
        }

        private String _KeyWord;

        public String KeyWord
        {
            get { return _KeyWord; }
            set { _KeyWord = value; }
        }

        private String _Command;

        public String Command
        {
            get { return _Command; }
            set { _Command = value; }
        }
        private String _Message;

        public String Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
        private DateTime _Date;

        public DateTime Date
        {
            get { return _Date; }
            set { _Date = value; }
        }

        private String _User;

        public String User
        {
            get { return _User; }
            set { _User = value; }
        }

        #endregion Properties

        #region Constructor
        public LoggingModel() { }

        public LoggingModel(DataRow dr)
        {
            try
            {
                User = dr["User"].ToString();
                Message = dr["Message"].ToString();
                LogType = (LoggingType)int.Parse(dr["LogType"].ToString());
                KeyWord = dr["KeyWord"].ToString();
                Id = int.Parse(dr["Id"].ToString());
                //_Date = DateTime.ParseExact(dr["Date"].ToString(), _DateFormat, _DateProvider);
                //_Date = DateTime.Parse(dr["Date"].ToString());
                _Date = new DateTime(Convert.ToInt64(dr["Date"]));
                Command = dr["Command"].ToString();
            }
            catch (Exception Exception){
                Console.WriteLine(Exception.Message);
               // throw Exception;
            }
        }

        #endregion Constructor

        #region Methods

        public String GeneralInsertCommand()
        {
            //String insertCommand = String.Format("insert into tbl_history( logtype, keyword, command, message, date, user) values( {1}, '{2}', '{3}', '{4}', '{5}', '{6}')",
            //    this.Id, (int)this.LogType, this.KeyWord, this.Command, this.Message, this.Date.ToString(_DateFormat), this.User);
            String insertCommand = String.Format("insert into tbl_history( logtype, keyword, command, message, date, user) values( {1}, '{2}', '{3}', '{4}', '{5}', '{6}')",
                this.Id, (int)this.LogType, this.KeyWord, this.Command, this.Message, this.Date.Ticks, this.User);
            return insertCommand;
        }

        public String ExportStringData() {
            //String insertCommand = String.Format("{1}, {2}, {3}, {4}, {5}, {6}",
            //    this.Id, this.LogType.ToString(), this.KeyWord, this.Command, this.Message, this.Date.ToString(_DateFormat), this.User);
            String insertCommand = String.Format("{1}, {2}, {3}, {4}, {5}, {6}",
                this.Id, this.LogType.ToString(), this.KeyWord, this.Command, this.Message, this.Date.ToString(), this.User);
            return insertCommand;
        }

        public static String ExportHeader() {
            return "Type, Keyword, Command, Message, Date, User";
        }

        #endregion Methods
    }

    /// <summary>
    /// @Author: TrangDong
    /// Stored log type of history
    /// </summary>
    public enum LoggingType {
        None = 0,
        Warning = 1,
        Error = 2,
        Info = 3,
        LogedIn = 4,
        LogedOut = 5,
        Started = 6,
        Stopped = 7,
        Tracking = 8,
    }

}
