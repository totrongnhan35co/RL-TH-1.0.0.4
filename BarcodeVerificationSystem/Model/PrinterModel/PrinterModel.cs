using BarcodeVerificationSystem.Controller;
using System;
using System.Xml.Serialization;

namespace BarcodeVerificationSystem.Model
{
    // ── Kiểu kết nối máy in Zebra 
    public enum ZebraConnectionType { LAN, USB }
    [Serializable]
    public class PrinterModel
    {
        private int _Index = 0;
        private string _IP = "127.0.0.1";
        private int _Port = 2030;
        private RoleOfStation _RoleOfPrinter = RoleOfStation.ForProduct;
        private bool _IsEnable = true;
        private bool _IsVersion = false;
        private bool _IsConnected = false;
        private int _CountTimeReconnect = 0;
        private int _NumPortRemote = 80;
        private bool _CheckPrinterSettingsIsEnable = false;
        private PODController _PODController = null;
        [XmlIgnore]
        public bool IsConnected { get => _IsConnected; set => _IsConnected = value; }
        [XmlIgnore]
        public int CountTimeReconnect { get => _CountTimeReconnect; set => _CountTimeReconnect = value; }
        [XmlIgnore]
        public PODController PODController { get => _PODController; set => _PODController = value; }
        public int Index { get => _Index; set => _Index = value; }
        public string IP { get => _IP; set => _IP = value; }
        public int Port { get => _Port; set => _Port = value; }
        public RoleOfStation RoleOfPrinter { get => _RoleOfPrinter; set => _RoleOfPrinter = value; }
        public bool IsEnable { get => _IsEnable; set => _IsEnable = value; }
        public bool IsVersion { get => _IsVersion; set => _IsVersion = value; }
        public int NumPortRemote { get => _NumPortRemote; set => _NumPortRemote = value; }
        public bool CheckAllPrinterSettings { get => _CheckPrinterSettingsIsEnable; set => _CheckPrinterSettingsIsEnable = value; }
        public bool EnableSendTurboSpeed { get; set; } = false;
        public int SendTimeIndependent { get; set; } = 50;
        public int Speed { get; set; } = 0;
        public int Gap { get; set; } = 0;
        public bool EnableButtonMissedStop { get; set; }
        public int TimeDelaySendFirstBuffer { get; set; } = 25; // ms
        public int NumberBuffer1StSend { get; set; } = 200; //POD

        // Zebra Printer Settings (separated for easier maintenance)
        private ZebraSettings _ZebraSettings = new ZebraSettings();
        public ZebraSettings ZebraSettings { get => _ZebraSettings; set => _ZebraSettings = value; }
    }

    [Serializable]
    public class ZebraSettings
    {
        // Text Settings
        private bool _EnableText = true;
        private string _TextFont = "A - Smallest (9x12 dots)";
        private string _TextContent = "Test QR Code";
        private int _TextX = 50;
        private int _TextY = 50;
        private int _TextFontSize = 40;

        // Barcode Settings
        private bool _EnableBarcode = true;
        private string _BarcodeType = "QR Code";
        private string _BarcodeData = "https://google.com";
        private int _BarcodeX = 50;
        private int _BarcodeY = 150;
        private int _BarcodeSize = 6;
        private string _BarcodeText = "";
        // ── THÊM MỚI ──────────────────────────────────────────────
        // Module width (^BY): 1=hẹp nhất, 2=mặc định, 3=rộng hơn
        private int _BarcodeModuleWidth = 1;
        public int BarcodeModuleWidth
        {
            get => _BarcodeModuleWidth;
            set => _BarcodeModuleWidth = value;
        }

        // Label length (^LL) tính bằng dots (203dpi: 1mm ≈ 8 dots)
        // 30mm = 240 dots, 50mm = 400 dots
        private int _LabelLength = 400;
        public int LabelLength
        {
            get => _LabelLength;
            set => _LabelLength = value;
        }
        // ── Connection Type ────────────────────────────────────────────
        private ZebraConnectionType _ConnectionType = ZebraConnectionType.LAN;
        private string _UsbPrinterName = "";

        public ZebraConnectionType ConnectionType { get => _ConnectionType; set => _ConnectionType = value; }
        public string UsbPrinterName { get => _UsbPrinterName; set => _UsbPrinterName = value; }
        // ─


        public bool EnableText { get => _EnableText; set => _EnableText = value; }
        public string TextFont { get => _TextFont; set => _TextFont = value; }
        public string TextContent { get => _TextContent; set => _TextContent = value; }
        public int TextX { get => _TextX; set => _TextX = value; }
        public int TextY { get => _TextY; set => _TextY = value; }
        public int TextFontSize { get => _TextFontSize; set => _TextFontSize = value; }

        public bool EnableBarcode { get => _EnableBarcode; set => _EnableBarcode = value; }
        public string BarcodeType { get => _BarcodeType; set => _BarcodeType = value; }
        public string BarcodeData { get => _BarcodeData; set => _BarcodeData = value; }
        public int BarcodeX { get => _BarcodeX; set => _BarcodeX = value; }
        public int BarcodeY { get => _BarcodeY; set => _BarcodeY = value; }
        public int BarcodeSize { get => _BarcodeSize; set => _BarcodeSize = value; }
        public string BarcodeText { get => _BarcodeText; set => _BarcodeText = value; }
    }
}
