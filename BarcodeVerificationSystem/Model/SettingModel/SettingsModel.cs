using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Response;
using BarcodeVerificationSystem.Model.CodeGeneration;
using BarcodeVerificationSystem.Model.Droco;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace BarcodeVerificationSystem.Model
{
    public class SettingsModel
    {
        #region Properties

        public ResponseOrder DispatchingOrderPayload = new ResponseOrder
        {
            payload = new ResponseOrder.Payload
            {
                items = new List<ResponseOrder.Item>()
            }
        };

        public ResponseProcessOrder ManufacturingListPO;
        public int SelectedPOIndex = 0;
        public int SelectedBatchIndex = 0;

        public ResponseAllProducts CaoSuProductList;
        public ProductItem CaoSuProduct;
        public double ProductWeight;
        // ── THTrueMilk R-Link Master Settings ─────────────────────────

        public List<ProductItem> CachedProducts { get; set; } = new List<ProductItem>();
        public List<string> CachedPrinterTemplates { get; set; } = new List<string>();
        public List<string> CachedCameraPrograms { get; set; } = new List<string>();
//        public List<string> CachedCameraPrograms { get; set; } = new List<string>
//{
//    "0001_8935217401130",
//    "0002_8935217401758",
//    "0003_8935217401765",

//};


        private int _thDeltaMinutes = 300; // 300 giây = 5 phút
        public int THDeltaMinutes
        {
            get => _thDeltaMinutes;
            set => _thDeltaMinutes = Math.Max(1, Math.Min(900, value)); // 1-900 giây
        }

        private int _thNMinutes = 30;
        public int THNMinutes
        {
            get => _thNMinutes;
            set => _thNMinutes = Math.Max(1, value);
        }

        public int THRetentionDays { get; set; } = 180;
        private int _thBufferCount = 50;
        public int THBufferCount
        {
            get => _thBufferCount;
            set => _thBufferCount = Math.Max(0, value);
        }

        public int THMaxConsecutiveError { get; set; } = 5;

        public double THReserveFactor { get; set; } = 1.5;

        private int _thMonitorInterval = 10;
        public int THMonitorInterval
        {
            get => _thMonitorInterval;
            set => _thMonitorInterval = Math.Max(1, value);
        }

        private int _thLogInterval = 10;
        public int THLogInterval
        {
            get => _thLogInterval;
            set => _thLogInterval = Math.Max(1, value);
        }

        private int _thQrThreshold = 100;
        public int THQrThreshold
        {
            get => _thQrThreshold;
            set => _thQrThreshold = Math.Max(0, value);
        }

        public string THQrBaseUrl { get; set; } = "";
        public int THQrNumberOfUrl { get; set; } = 0;

        private int _THTotalCodes = 0;
        public int THTotalCodes
        {
            get => _THTotalCodes;
            set => _THTotalCodes = Math.Max(0, value);
        }
        private int _thJobMonitorInterval = 10;
        public int THJobMonitorInterval
        {
            get => _thJobMonitorInterval;
            set => _thJobMonitorInterval = Math.Max(1, value);
        }

      


        private string _thCameraModelForTraining = "";
        public string THCameraModelForTraining
        {
            get => _thCameraModelForTraining;
            set => _thCameraModelForTraining = value ?? string.Empty;
        }
        private string _thFtpImagePath = @"D:\hinhanh\VS\Camera\Images";
        public string THFtpImagePath
        {
            get => _thFtpImagePath;
            set => _thFtpImagePath = value ?? string.Empty;
        }
        private string _thErrorImageFolder = @"D:\HinhAnhLoiDuAnTHTrueMilk\ImagesError";
        public string THErrorImageFolder
        {
            get => _thErrorImageFolder;
            set => _thErrorImageFolder = value ?? string.Empty;
        }
        /// <summary>Job đang chạy dở chưa Complete — tự mở lại sau login.</summary>
        public string LastActiveJobName { get; set; } = "";
        // ── Danh mục sản phẩm THTrueMilk (session data — không lưu XML) ──
        [XmlIgnore]
        public List<BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models.ProductItem> THProductList { get; set; }
            = new List<BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models.ProductItem>();
      
        public int AddQuantity = 0;
        private string _printTemplate = "";
        public string PrintTemplate { get => _printTemplate; set => _printTemplate = value; }

        private string _RLinkName = "";
        public string RLinkName
        {
            get { return _RLinkName; }
            set { _RLinkName = value; }
        }

        public int TotalLines = 14;
        public int LineIndex = 0;
        // Trong SettingsModel.cs — thêm field:
        private bool _apiPingSuccess = false;
        public bool ApiPingSuccess { get => _apiPingSuccess; set => _apiPingSuccess = value; }

        private CompareType _CompareType = CompareType.CanRead;
        public CompareType CompareType { get => _CompareType; set => _CompareType = value; }

        private string _JobFileExtension = ".rvis";

        private List<CameraModel> _CameraList = new List<CameraModel>();
        public List<CameraModel> CameraList { get => _CameraList; set => _CameraList = value; }
        private PrinterModel _ZebraPrinter = new PrinterModel();
        public PrinterModel ZebraPrinter { get => _ZebraPrinter; set => _ZebraPrinter = value; }

        private List<PrinterModel> _PrinterList = new List<PrinterModel>();
        public List<PrinterModel> PrinterList { get => _PrinterList; set => _PrinterList = value; }
        private bool _IsPrinting = true;
        public bool IsPrinting { get => _IsPrinting; set => _IsPrinting = value; }

        private char _splitCharacter = ';';
        public char SplitCharacter { get => _splitCharacter; set => _splitCharacter = value; }

        private bool _SensorControllerEnable = true;
        private string _SensorControllerIP = "192.168.1.100";
        private int _SensorControllerPort = 2001;
        private int _SensorControllerPort2 = 2002;
        private int _NumberOfPort = 1;
        public int NumberOfPort { get => _NumberOfPort; set => _NumberOfPort = value; }

        private bool _allowDupAndNonStop = false;
        public bool AllowDupAndNonStop { get => _allowDupAndNonStop; set => _allowDupAndNonStop = value; }

        private bool _isAllowJobDeletion = true;
        public bool IsAllowJobDeletion { get => _isAllowJobDeletion; set => _isAllowJobDeletion = value; }
        

        private bool _maskData = false;
        public bool MaskData { get => _maskData && ProjectLabel.IsNutrifood; set => _maskData = value; }
        private bool _hideFunctions = false;
        public bool HideFunctions { get => _hideFunctions && ProjectLabel.IsNutrifood; set => _hideFunctions = value; }
        public bool SensorControllerEnable { get => _SensorControllerEnable; set => _SensorControllerEnable = value; }
        public int SensorControllerPort2 { get => _SensorControllerPort2; set => _SensorControllerPort2 = value; }
        public string SensorControllerIP { get => _SensorControllerIP; set => _SensorControllerIP = value; }
        public int SensorControllerPort { get => _SensorControllerPort; set => _SensorControllerPort = value; }

        private int _PLCVersion = 0;
        public int PLCVersion { get => _PLCVersion; set => _PLCVersion = value; }

        private ResumeEncoderType _ResumeEncoderType = ResumeEncoderType.ResumeA;
        private bool _ResumeEncoderEnable = false;
        public bool ResumeEncoderEnable { get => _ResumeEncoderEnable; set => _ResumeEncoderEnable = value; }
        public enum ResumeEncoderType
        {
            ResumeA,
            ResumeAB,
        }
        public ResumeEncoderType ResumeEncoder { get => _ResumeEncoderType; set => _ResumeEncoderType = value; }


        private ResumeEncoderMode _ResumeEncoderMode = ResumeEncoderMode.External;
        public enum ResumeEncoderMode
        {
            Internal,
            External,
        }
        public ResumeEncoderMode EncoderMode { get => _ResumeEncoderMode; set => _ResumeEncoderMode = value; }

        private bool _EnablePosition = false;
        public bool EnablePosition { get => _EnablePosition && Shared.Settings.CameraList.FirstOrDefault().CameraType != CameraType.DM; set => _EnablePosition = value; }

        private bool _isItemsPerHour = false;
        public bool IsItemsPerHour { get => _isItemsPerHour; set => _isItemsPerHour = value; }
        public enum PositionType
        {
            LogoPosition,
            BarcodePosition,
        }
        private PositionType _PositionType = PositionType.BarcodePosition;
        public PositionType Position { get => _PositionType; set => _PositionType = value; }


        public List<int> SensorControllerDelayBefore;
        public List<int> SensorControllerDelayAfter;
        public List<int> SensorControllerPulseEncoder;
        public List<float> SensorControllerEncoderDiameter;
        public List<int> GapLength1;
        public List<int> Length2Error1;
        public List<int> DelayOutputError;

        public T Get<T>(List<T> list, int index)
        {
            while (list.Count <= index) list.Add(default);
            return list[index];
        }

        public void Set<T>(List<T> list, int index, T value)
        {
            while (list.Count <= index) list.Add(default);
            list[index] = value;
        }

        private int _gapLength2 = 0;
        public int GapLength2
        {
            get { return _gapLength2; }
            set { _gapLength2 = value; }
        }


        #region Production Mode

        private int _increasedDataPercent = 10;
        public int IncreasedDataPercent
        {
            get { return _increasedDataPercent; }
            set { _increasedDataPercent = value; }
        }

        private bool _isManufacturingMode = true;
        public bool IsManufacturingMode
        {
            get { return _isManufacturingMode; } 
            set { _isManufacturingMode = value; }
        }


        private List<string> _apiDomains;
        public List<string> ApiDomains 
        { 
            get { return _apiDomains; }
            set { _apiDomains = value; }
        }

        private string _apiUrl = "https://www.google.com/";
        private string _apiDomain = "https://www.google.com/";

        private string _accessToken = "";
        public string AccessToken
        {
            get { return _accessToken; }
            set { _accessToken = value; }
        }

        private string _refreshToken = "";
        public string RefreshToken
        {
            get { return _refreshToken; }
            set { _refreshToken = value; }
        }

        private string _wokaToken = "";
        public string WokaToken
        {
            get { return _wokaToken; }
            set { _wokaToken = value; }
        }
        //

        private string _drocoToken = "";
        public string DrocoToken
        {
            get { return _drocoToken; }
            set { _drocoToken = value; }
        }

        private string _domainQRCode = "";
        public string DomainQRCode
        {
            get { return _domainQRCode; }
            set { _domainQRCode = value; }
        }

        public string ApiUrl
        {
            get { return _apiUrl; }
            set { _apiUrl = value; }
        }

        public string ApiDomain
        {
            get { return _apiDomain; }
            set { _apiDomain = value; }
        }

        private string _RLinkId = "";

        public string LineId
        {
            get { return _RLinkId; }
            set { _RLinkId = value; }
        }

        private string _OrderId = "";
        public string OrderId
        {
            get { return _OrderId; }
            set { _OrderId = value; }
        }

        private string _wmsNumber = "";
        public string WmsNumber
        {
            get { return _wmsNumber; }
            set { _wmsNumber = value; }
        }

        private string _lineName = "";
        public string LineName
        {
            get { return _lineName; }
            set { _lineName = value; }
        }

        private string _factoryCode = "";
        public string FactoryCode
        {
            get { return _factoryCode; }
            set { _factoryCode = value; }
        }
        private string _factoryName = "";  
        public string FactoryName
        {
            get { return _factoryName; }
            set { _factoryName = value; }
        }

        private string _LOTFormatDate = "yyyy/MM/dd";
        public string LOTFormatDate { get => _LOTFormatDate; set => _LOTFormatDate = value; }

        #endregion

        #region CenterIndia

        private CenteryIndiaModel _centerIndiaModel = new CenteryIndiaModel();
        public CenteryIndiaModel CenterIndiaModel
        {
            get { return _centerIndiaModel; }
            set { _centerIndiaModel = value ?? new CenteryIndiaModel(); }
        }

        #endregion
     

        #region Droco QR Field Mapping

        private DrocoQRFieldMapping _drocoQRFieldMapping = new DrocoQRFieldMapping();
        public DrocoQRFieldMapping DrocoQRFieldMapping
        {
            get { return _drocoQRFieldMapping ?? (_drocoQRFieldMapping = new DrocoQRFieldMapping()); }
            set { _drocoQRFieldMapping = value; }
        }

        #endregion

        #region THTrueMilk Database

        // QR Code Bank (SQL Server)
        private string _thQrBankServer = "";
        public string THQrBankServer { get => _thQrBankServer; set => _thQrBankServer = value; }

        private string _thQrBankPort = "1433";
        public string THQrBankPort { get => _thQrBankPort; set => _thQrBankPort = value; }

        private string _thQrBankDatabase = "";
        public string THQrBankDatabase { get => _thQrBankDatabase; set => _thQrBankDatabase = value; }

        private string _thQrBankUsername = "";
        public string THQrBankUsername { get => _thQrBankUsername; set => _thQrBankUsername = value; }

        private string _thQrBankPassword = "";
        public string THQrBankPassword { get => _thQrBankPassword; set => _thQrBankPassword = value; }

        private string _thQrBankTable = "";
        public string THQrBankTable { get => _thQrBankTable; set => _thQrBankTable = value; }

        // Local Database (PostgreSQL)
        private string _thLocalDbServer = "127.0.0.1";
        public string THLocalDbServer { get => _thLocalDbServer; set => _thLocalDbServer = value; }

        private string _thLocalDbPort = "5432";
        public string THLocalDbPort { get => _thLocalDbPort; set => _thLocalDbPort = value; }

        private string _thLocalDbDatabase = "";
        public string THLocalDbDatabase { get => _thLocalDbDatabase; set => _thLocalDbDatabase = value; }

        private string _thLocalDbUsername = "postgres";
        public string THLocalDbUsername { get => _thLocalDbUsername; set => _thLocalDbUsername = value; }

        private string _thLocalDbPassword = "";
        public string THLocalDbPassword { get => _thLocalDbPassword; set => _thLocalDbPassword = value; }

        private string _thLocalDbTable = "";
        public string THLocalDbTable { get => _thLocalDbTable; set => _thLocalDbTable = value; }
        // Thêm vào region THTrueMilk Database
        public string THMySqlServer { get; set; } = "localhost";
        public string THMySqlPort { get; set; } = "3306";
        public string THMySqlDatabase { get; set; } = "";
        public string THMySqlTable { get; set; } = "";
        public string THMySqlUsername { get; set; } = "root";
        public string THMySqlPassword { get; set; } = "";
        private string _thSelectedDbType = "SQL Server";
        public string THSelectedDbType { get => _thSelectedDbType; set => _thSelectedDbType = value; }

        // ── Chế độ vận hành THTrueMilk ──────────────────────────────────
        private BarcodeVerificationSystem.Model.THTrueMilk.THTrueMilkOperatingMode _thOperatingMode
            = BarcodeVerificationSystem.Model.THTrueMilk.THTrueMilkOperatingMode.BatchOneQrCode;
        public BarcodeVerificationSystem.Model.THTrueMilk.THTrueMilkOperatingMode THOperatingMode
        {
            get => _thOperatingMode;
            set => _thOperatingMode = value;
        }

        #endregion

        #region Serial Device
        public bool EnSerialDevice { get; set; }
        private string _serialDivComName = "COM3";
        private int _serialDivbitPerSecond = 9600;
        private int _serialDivDataBits = 8;
        private Parity _serialDivParity = Parity.None;
        private StopBits _serialDivStopBits = StopBits.One;

        public string SerialDivComName { get => _serialDivComName; set => _serialDivComName = value; }
        public int SerialDivBitPerSecond { get => _serialDivbitPerSecond; set => _serialDivbitPerSecond = value; }
        public int SerialDivDataBits { get => _serialDivDataBits; set => _serialDivDataBits = value; }
        public Parity SerialDivParity { get => _serialDivParity; set => _serialDivParity = value; }
        public StopBits SerialDivStopBits { get => _serialDivStopBits; set => _serialDivStopBits = value; }
        #endregion Serial Device

        private int _SensorControllerPulseEncoder2 = 3600;

        private int _length2Error2 = 0;
        public int Length2Error2
        {
            get { return _length2Error2; }
            set { _length2Error2 = value; }
        }

        public int SensorControllerPulseEncoder2
        {
            get { return _SensorControllerPulseEncoder2; }
            set { _SensorControllerPulseEncoder2 = value; }
        }

        private float _SensorControllerEncoderDiameter2 = 48.51f;

        public float SensorControllerEncoderDiameter2
        {
            get { return _SensorControllerEncoderDiameter2; }
            set { _SensorControllerEncoderDiameter2 = value; }
        }

        private int _SensorControllerDelayBefore2 = 0;

        public int SensorControllerDelayBefore2
        {
            get { return _SensorControllerDelayBefore2; }
            set { _SensorControllerDelayBefore2 = value; }
        }

        private int _SensorControllerDelayAfter2 = 0;

        public int SensorControllerDelayAfter2
        {
            get { return _SensorControllerDelayAfter2; }
            set { _SensorControllerDelayAfter2 = value; }
        }

        private int _delayOutputTime = 0;
        public int DelayOutputTime
        {
            get => _delayOutputTime;
            set
            {
                _delayOutputTime = value;
            }
        }

        private string _ExportCheckedResultPath = @"C:\Users\Public\Exports\CheckedResult";
        private string _DataCheckedFileName = "20191220_164200_DataChecked.txt";
        private bool _ExportImageEnable = false;
        private string _ExportImagePath = @"C:\Users\Public\Exports\Images";
        private string _FailedDataSentToPrinter = @"Failure";
        private List<PODModel> _PrintFieldForVerifyAndPrint = new List<PODModel>();
        public string ExportCheckedResultPath { get => _ExportCheckedResultPath; set => _ExportCheckedResultPath = value; }
        public string DataCheckedFileName { get => _DataCheckedFileName; set => _DataCheckedFileName = value; }
        public bool ExportImageEnable { get => _ExportImageEnable; set => _ExportImageEnable = value; }
        public string ExportImagePath { get => _ExportImagePath; set => _ExportImagePath = value; }
        public string FailedDataSentToPrinter { get => _FailedDataSentToPrinter; set => _FailedDataSentToPrinter = value; }
        public List<PODModel> PrintFieldForVerifyAndPrint { get => _PrintFieldForVerifyAndPrint; set => _PrintFieldForVerifyAndPrint = value; }

        private string _Language = "vi-VN"; // "vi-VN" "en-US"
        public string Language { get => _Language; set => _Language = value; }

        private string _DateTimeFormatOfResult = "yyyy/MM/dd HH:mm:ss";
        public string DateTimeFormatOfResult { get => _DateTimeFormatOfResult; set => _DateTimeFormatOfResult = value; }

        private bool _OutputEnable = true;
        public bool OutputEnable { get => _OutputEnable; set => _OutputEnable = value; }

        private bool _ExportOneForAllEnable = false;
        public bool ExportOneForAllEnable { get => _ExportOneForAllEnable; set => _ExportOneForAllEnable = value; }
        private bool _DuplicatedDBEnable = false;
        public bool DuplicatedDBEnable { get => _DuplicatedDBEnable; set => _DuplicatedDBEnable = value; }

        private bool _MarkCheckedAsPrintedEnable = true;
        public bool MarkCheckedAsPrintedEnable { get => _MarkCheckedAsPrintedEnable; set => _MarkCheckedAsPrintedEnable = value; }

        private bool _TotalCheckEnable = ProjectLabel.IsDefault; // true
        public bool TotalCheckEnable { get => _TotalCheckEnable; set => _TotalCheckEnable = value; }

        private bool _CheckAllWhenStart = true;
        public bool CheckAllWhenStart { get => _CheckAllWhenStart; set => _CheckAllWhenStart = value; }

        private bool _VerifyAndPrintBasicSentMethod = false; //true
        public bool VerifyAndPrintBasicSentMethod { get => _VerifyAndPrintBasicSentMethod; set => _VerifyAndPrintBasicSentMethod = value; }

        private string _ExportNamePrefixFormat = "yyyyMMdd_HHmmss";
        public string ExportNamePrefixFormat { get => _ExportNamePrefixFormat; set => _ExportNamePrefixFormat = value; }


        private string _JobDateTimeFormat = "yyyyMMdd_HHmmss";
        public string JobDateTimeFormat { get => _JobDateTimeFormat; set => _JobDateTimeFormat = value; }
        private string _JobFileNameDefault = "Template";
        public string JobFileNameDefault { get => _JobFileNameDefault; set => _JobFileNameDefault = value; }
        public string JobFileExtension { get => _JobFileExtension; set => _JobFileExtension = value; }

        #endregion Properties

        #region Methods
        public virtual void SaveSettings(string fileName)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                var serializer = new XmlSerializer(this.GetType());
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, this);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        public static SettingsModel LoadSetting(string fileName)
        {
            SettingsModel info = null;
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (var read = new StringReader(xmlString))
                {
                    Type outType = typeof(SettingsModel);
                    var serializer = new XmlSerializer(outType);
                    using (var reader = new XmlTextReader(read))
                    {
                        info = (SettingsModel)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception)
            {
                return new SettingsModel();
            }

            return info;
        }

        #endregion Methods
    }
}
