using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using UILanguage;

namespace BarcodeVerificationSystem.Model
{
    public class InternalData
    {
    }

    #region Enum

    #region USB Dongle key
    public enum SDCmd : ushort
    {
        SD_FIND = 1,		//Find SecureDongle
        SD_FIND_NEXT = 2,		//Find next
        SD_OPEN = 3,			//Open SecureDongle
        SD_CLOSE = 4,			//Close SecureDongle
        SD_READ,			//Read SecureDongle
        SD_WRITE,			//Write SecureDongle
        SD_RANDOM,			//Generate random
        SD_SEED,			//Generate seed
        SD_DECREASE = 17,
        SD_WRITE_USERID = 9,	//Read UID
        SD_READ_USERID = 10,	//Read UID
        SD_SET_MODULE = 11,   //Set Module
        SD_CHECK_MODULE = 12,	//Check Module
        SD_CALCULATE1 = 14,	//Calculate1
        SD_CALCULATE2,		//Calculate1
        SD_CALCULATE3,		//Calculate1
        SD_SET_COUNTER_EX = 160,         //Set Counter, Type change from WORD to DWORD
        SD_GET_COUNTER_EX = 161,          //Read counter, Type change from WORD to DWORD
        SD_SET_TIMER_EX = 162,         //Set Timer Unit Clock, Type change from WORD to DWORD
        SD_GET_TIMER_EX = 163,        //Get Timer Unit Code, , Type change from WORD to DWORD
        SD_ADJUST_TIMER_EX = 164,
    }

    public enum SDErrCode : uint
    {
        ERR_SUCCESS = 0,							//No error
        ERR_NO_PARALLEL_PORT = 0x80300001,		//(0x80300001)No parallel port
        ERR_NO_DRIVER,							//(0x80300002)No drive
        ERR_NO_DONGLE,							//(0x80300003)No SecureDongle
        ERR_INVALID_pWORD,					//(0x80300004)Invalid pword
        ERR_INVALID_pWORD_OR_ID,				//(0x80300005)Invalid pword or ID
        ERR_SETID,								//(0x80300006)Set id error
        ERR_INVALID_ADDR_OR_SIZE,				//(0x80300007)Invalid address or size
        ERR_UNKNOWN_COMMAND,					//(0x80300008)Unkown command
        ERR_NOTBELEVEL3,						//(0x80300009)Inner error
        ERR_READ,								//(0x8030000A)Read error
        ERR_WRITE,								//(0x8030000B)Write error
        ERR_RANDOM,								//(0x8030000C)Generate random error
        ERR_SEED,								//(0x8030000D)Generate seed error
        ERR_CALCULATE,							//(0x8030000E)Calculate error
        ERR_NO_OPEN,							//(0x8030000F)The SecureDongle is not opened
        ERR_OPEN_OVERFLOW,						//(0x80300010)Open SecureDongle too more(>16)
        ERR_NOMORE = 17,								//(0x80300011)No more SecureDongle
        ERR_NEED_FIND,							//(0x80300012)Need Find before FindNext
        ERR_DECREASE,							//(0x80300013)Dcrease error
        ERR_AR_BADCOMMAND,						//(0x80300014)Band command
        ERR_AR_UNKNOWN_OPCODE,					//(0x80300015)Unkown op code
        ERR_AR_WRONGBEGIN,						//(0x80300016)There could not be constant in first instruction in arithmetic 
        ERR_AR_WRONG_END,						//(0x80300017)There could not be constant in last instruction in arithmetic 
        ERR_AR_VALUEOVERFLOW,					//(0x80300018)The constant in arithmetic overflow
        ERR_UNKNOWN = 0x8030ffff,					//(0x8030FFFF)Unkown error
        ERR_RECEIVE_NULL = 0x80300100,			//(0x80300100)Receive null
        ERR_PRNPORT_BUSY = 0x80300101				//(0x80300101)Parallel port busy
    }
    #endregion USB Dongle key

    public enum CheckPrinterSettings
    {
        Success = 0,
        NotRawData = 1,
        PODNotEnabled = 2,
        ResponsePODDataNotEnable = 3,
        ResponsePODCommandNotEnable = 4,
        MonitorNotEnable = 5,
        PODMode = 6
    }

    public enum PrinterStatus
    {
        //Stop, Processing, Ready, Printing, Connected, Disconnected, Error, Disable
        Stop,
        Processing,
        Ready,
        Printing,
        Connected,
        Disconnected,
        Error,
        Disable,
        Start,
        Null
    }

    public enum JobType
    {
        AfterProduction,
        OnProduction,
        VerifyAndPrint,
        StandAlone
    }

    public enum JobStatus
    {
        NewlyCreated,
        Unfinished,
        Accomplished,
        Deleted
    }

    public enum CompleteJobStatus
    {
        Created,
        Running,
        Completed
    }

    public enum OperationStatus
    {
        Running = 1,
        Processing = 3,
        Stopped = 2
    }

    public enum ObjectType
    {
        Barcode,
        OCR
    }

    public enum ComparisonType
    {
        CanRead,
        StaticText,
        RegularExpression,
        DataExist,
        DataOrder
    }

    public enum ComparisonResult
    {
        Valid,
        Invalided,
        Duplicated,
        Null,
        Missed
    }

    public enum ConnectionProtocol
    {
        COM,
        TCP,
        UDP
    }

    public enum SplitCharacter
    {
        Tab = '\t',
        Space = ' ',
        Dot = '.',
        Comma = ',',
        SemiColon = ';',
        Star = '*',
        Colon = ':',
        Hash = '#',
        AT = '@',
        Mod = '%',
        And = '&',
        Question = '?',
        US = 0x1F,
    }

    public enum DBConnectionType
    {
        CSVFile,
        XLSFile,
        XLSXFile,
        AccessFile,
        TXTFile,
        SQL,
        MySQL,
        Oracle,
        XML,
        SQLite
    }

    public enum DatabaseShow
    {
        Table,
        Views,
        StoreProcedures
    }

    public enum ReadTextMode
    {
        Normal = 0,
        Schema = 1,
        //Split = 2
    }

    public enum HitCursorType
    {
        None,
        Body,
        TopLeft,
        Top,
        TopRight,
        Left,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }


    public enum CheckCondition
    {
        Success,
        NotLoadDatabase,
        NotConnectCamera,
        NotLoadTemplate,
        MissingParameter,
        NotConnectPrinter,
        NotConnectServer,
        LeastOneAction,
        MissingParameterActivation,
        MissingParameterPrinting,
        MissingParameterWarehouseInput,
        MissingParameterWarehouseOutput,
        CreatingWarehouseInputReceipt,
        NoJobsSelected,
        OCRCameraIsOffline,
        NotConnectKeyence,
        NotConnectDatabase,
        NotConnectPlc
    }
    public enum RoleOfStation
    {
        ForProduct,
        ForBox
    }

    public enum OutputType
    {
        OutputCamera,
        CommandError
    }

    public enum CompareType
    {
        CanRead,
        StaticText,
        Database
    }
    public enum ActivationStatus
    {
        NotConnectedToServer,
        Failed,
        Successful
    }
    public enum InitDataError
    {
        DatabaseUnknownError,
        PrintedStatusUnknownError,
        CheckedResultUnknownError,
        CannotAccessDatabase,
        CannotAccessCheckedResult,
        CannotAccessPrintedResponse,
        DatabaseDoNotExist,
        CheckedResultDoNotExist,
        PrintedResponseDoNotExist,
        Unknown
    }
    public enum CameraModeRead
    {
        Basic,
        MultiRead
    }

    #endregion Enum

    #region Enum extensions
    public static class JobStatusExtensions
    {
        public static string ToFriendlyString(this JobStatus jobStatus)
        {
            string strConvert = "";
            switch (jobStatus)
            {
                case JobStatus.NewlyCreated:
                    strConvert = Lang.NewLyCreated;
                    break;
                case JobStatus.Unfinished:
                    strConvert = Lang.Unfinished;
                    break;
                case JobStatus.Accomplished:
                    strConvert = Lang.Completed;
                    break;
                case JobStatus.Deleted:
                    strConvert = Lang.Deleted;
                    break;
            }
            return strConvert;
        }
    }

    public static class JobTypeExtensions
    {
        public static string ToFriendlyString(this JobType jobType)
        {
            string strResult = "";
            switch (jobType)
            {
                case JobType.AfterProduction:
                    strResult = Lang.AfterProduction;
                    break;
                case JobType.OnProduction:
                    strResult = Lang.OnProduction;
                    break;
                case JobType.VerifyAndPrint:
                    strResult = Lang.VerifyAndPrint;
                    break;
                case JobType.StandAlone:
                    strResult = Lang.Standalone;
                    break;
            }
            return strResult;
        }
    }

    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T enumerationValue) where T : struct
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumerationValue.ToString();
        }
    }


    public static class ComparisonResultExtensions
    {
        public static string ToFriendlyString(this ComparisonResult comparisonResult)
        {
            string strResult = "";
            switch (comparisonResult)
            {
                case ComparisonResult.Valid:
                    strResult = ComparisonResult.Valid.ToString().ToUpper(); // Don't convert Uppercase here because it's time consuming
                    break;

                case ComparisonResult.Invalided:
                    strResult = ComparisonResult.Invalided.ToString().ToUpper();
                    break;

                case ComparisonResult.Null:
                    strResult = ComparisonResult.Null.ToString().ToUpper();
                    break;

                case ComparisonResult.Missed:
                    strResult = ComparisonResult.Missed.ToString().ToUpper();
                    break;

                case ComparisonResult.Duplicated:
                    strResult = ComparisonResult.Duplicated.ToString().ToUpper();
                    break;

                default:
                    strResult = ComparisonResult.Invalided.ToString().ToUpper();
                    break;
            }
            return strResult;
        }

        public static Color GetBackgroundColor(this ComparisonResult comparisonResult)
        {
            _ = Color.Black;
            Color color;
            switch (comparisonResult)
            {
                case ComparisonResult.Valid:
                    color = Color.Green;
                    break;

                case ComparisonResult.Invalided:
                    color = Color.Red;
                    break;

                default:
                    color = Color.Gray;
                    break;
            }
            return color;
        }
    }

    public static class OperationStatusExtensions
    {
        public static string ToFriendlyString(this OperationStatus operationStatus)
        {
            string strResult;
            switch (operationStatus)
            {
                case OperationStatus.Running:
                    strResult = Lang.Running;
                    break;

                case OperationStatus.Processing:
                    strResult = Lang.Processing;
                    break;

                default:
                    strResult = Lang.Stopped;
                    break;
            }
            return strResult;
        }

        public static Color GetForegroundColor(this OperationStatus operationStatus)
        {
            _ = Color.Black;
            Color color;
            switch (operationStatus)
            {
                case OperationStatus.Running:
                    color = Color.Green;
                    break;

                case OperationStatus.Processing:
                    color = Color.Orange;
                    break;

                default:
                    color = Color.Red;
                    break;
            }
            return color;
        }

    }
    #endregion Enum extensions
}
