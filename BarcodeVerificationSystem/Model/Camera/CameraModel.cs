using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BarcodeVerificationSystem.Model
{
    [Serializable]
    public class OcrToolMapping
    {
        /// <summary>Tên tool từ chunk data của Hikrobot, ví dụ: "dlocrdetect:6"</summary>
        public string ToolKey { get; set; } = "";
        /// <summary>PODName của cột DB tương ứng, ví dụ: "NSX"</summary>
        public string PODName { get; set; } = "";
    }

    public enum HikrobotCompareMode
    {
        /// <summary>Chỉ dùng barcode text để so sánh (mặc định)</summary>
        BarcodeOnly,
        /// <summary>Dùng các trường OCR để tạo key so sánh thay barcode</summary>
        OCROnly,
        /// <summary>Barcode là key chính, OCR fields kiểm tra thêm từng cột DB</summary>
        BarcodeAndOCR
    }
    [Serializable]
    public class CameraModel
    {
        private CameraBrand _CameraBrand = CameraBrand.Cognex;
        public CameraBrand CameraBrand
        {
            get => _CameraBrand;
            set => _CameraBrand = value;
        }

        private CameraType _CameraType = CameraType.DM;
        public CameraType CameraType
        {
            get => _CameraType;
            set => _CameraType = value;
        }

        private string _Port = "80";
        public string Port
        {
            get { return _Port; }
            set { _Port = value; }
        }
        //
        private int _ObjectSelectNum;
        public int ObjectSelectNum
        {
            get { return _ObjectSelectNum; }
            set { _ObjectSelectNum = value; }
        }
        public int KeyenceCurrentProgramNo { get; set; } = 0; // Default: 0001
        public bool IsSymbolMaster { get; set; } = false;
        public string ObjectNameMaster { get; set; } = "";
        private int _Index = 0;
        private RoleOfStation _RoleOfCamera = RoleOfStation.ForProduct;
        private OutputType _OutputType = OutputType.OutputCamera;
        private CameraModeRead _ReadMode = CameraModeRead.Basic;
        private string _IP = "192.168.0.2";
        private string _UserName = "";
        private string _Password = "";
        private string _NoReadOutputString = "";
        private bool _AutoReconnect = true;
        private bool _OutputEnable = true;
        private bool _IsEnable = true;
        private string _Name = "";
        private string _SerialNumber = "";
        private bool _IsConnected = false;
        private bool _IsIndexCommandEnable = false;

        private int _CountTimeReconnect = 0;

        private double _Xposition = 0;
        private double _Yposition = 0;
        private int _BarcodeWidth = 0;
        private int _BarcodeHeight = 0;
        private double _Threshold = 0;


        [XmlIgnore]
        public string Name { get => _Name; set => _Name = value; }
        [XmlIgnore]
        public string SerialNumber { get => _SerialNumber; set => _SerialNumber = value; }
        [XmlIgnore]
        public bool IsConnected { get => _IsConnected; set => _IsConnected = value; }
        [XmlIgnore]
        public int CountTimeReconnect { get => _CountTimeReconnect; set => _CountTimeReconnect = value; }
        public int Index { get => _Index; set => _Index = value; }
        public RoleOfStation RoleOfCamera { get => _RoleOfCamera; set => _RoleOfCamera = value; }
        public OutputType OutputType { get => _OutputType; set => _OutputType = value; }
        public string IP { get => _IP; set => _IP = value; }
        public string UserName { get => _UserName; set => _UserName = value; }
        public string Password { get => _Password; set => _Password = value; }
        public string NoReadOutputString { get => _NoReadOutputString; set => _NoReadOutputString = value; }
        public bool AutoReconnect { get => _AutoReconnect; set => _AutoReconnect = value; }
        public bool OutputEnable { get => _OutputEnable; set => _OutputEnable = value; }
        public bool IsEnable { get => _IsEnable; set => _IsEnable = value; }
        public bool IsIndexCommandEnable { get => _IsIndexCommandEnable; set => _IsIndexCommandEnable = value; }
        public double Xposition { get => _Xposition; set => _Xposition = value; }
        public double Yposition { get => _Yposition; set => _Yposition = value; }
        public int BarcodeWidth { get => _BarcodeWidth; set => _BarcodeWidth = value; }
        public int BarcodeHeight { get => _BarcodeHeight; set => _BarcodeHeight = value; }
        public double Threshold { get => _Threshold; set => _Threshold = value; }

        public CameraModeRead ReadMode { get => _ReadMode; set => _ReadMode = value; }
        public string ISSlaveIP { get; set; }
        public int WidthImage { get; set; } = 240;
        public int HeigthImage { get; set; } = 160;
        public int ISEventCounter { get; set; }
        public string CameraJobNameMaster { get; set; } = "JobName.jobx";
        public string CameraJobNameSlave { get; set; } = "JobName.jobx";
        public bool IsSymbolSlave { get; set; }
        public string ObjectNameSlave { get; set; } = "";
        public string CommandErrorOutput { get; set; } = "(R00001001000000100000000000000000000000000000000000000000000000000000000000000000000)";

        public HikrobotCompareMode HikrobotCompareMode { get; set; } = HikrobotCompareMode.BarcodeOnly;
        public List<OcrToolMapping> OcrToolMappings { get; set; } = new List<OcrToolMapping>();

        public string SolutionName { get; set; } = "";
    }
    public enum CameraType
    {
        UKN,
        DM,
        IS,
        ISDual,
        CV_X,
        VS_C,
        HIKROBOT
    }

    public enum CameraBrand
    {
        Cognex,
        Keyence,
        Hikrobot
    }
}
