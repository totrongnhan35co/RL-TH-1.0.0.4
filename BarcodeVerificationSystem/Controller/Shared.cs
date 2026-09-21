using BarcodeVerificationSystem.Controller.Camera;
using BarcodeVerificationSystem.Controller.Camera.Hik;
using BarcodeVerificationSystem.Controller.Camera.Keyence;
using BarcodeVerificationSystem.Controller.ZebraPrinter;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using BarcodeVerificationSystem.Model.RunningMode.Dispatching;
using BarcodeVerificationSystem.Model.UserPermission;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Woka;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services.THMilk;
using BarcodeVerificationSystem.Utils.CodeGeneration;
using BarcodeVerificationSystem.View;
using BarcodeVerificationSystem.View.CustomDialogs;
using CommonVariable;
using EncrytionFile.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace BarcodeVerificationSystem.Controller
{
    public class Shared
    {

        #region Variables
        public static OperationStatus OperStatus = OperationStatus.Stopped;
        public static bool IsMainFormRunning = false;
        public static UserDataModel LoggedInUser = null;
        public static List<PODController> PODControllerList = new List<PODController>();
        public static PODController PrinterPODController = null;
        public static SettingsModel Settings = new SettingsModel();
        public static UserPermission UserPermission = new UserPermission();
        public static JobModel CurrentJob;
        public static bool IsSensorControllerConnected = false;
        public static bool PrinterDisconnectAlertShown = false;
        public static bool PendingLoginRed = false;
        public static bool IsSerialDeviceConnected = false;
        public static bool IsDatabaseConnected = false;
        public static bool IsSampled = false;
        public static DateTime LicenseExpireDate = DateTime.MaxValue;
        public static THMilkAPIHandler ServerService = new THMilkAPIHandler("http://*:5001/");
        public static bool IsClientMode = false;
        public static CaoSuAllValueProcess CaoSuAllValueProcess = null;
        public static WokaAllValueProcess WokaAllValueProcess = null;
        public static DrocoAllValueProcess DrocoAllValueProcess = null;

        public static ExcelCodeCache ExcelCodeCache { get; set; } = null;
        public static ResponseReservation Reservation;
        public static int SelectedRESMaterialIndex = 0;

        public static PODController SensorController = null;
        public static SerialDeviceController SerialDevController = null;

        public static CameraController CamController = null;
        public static DMSeries DMCamera = null;
        public static CvxCamera cvxCamera = null;
        public static VscCamera vscCamera = null;
        public static HikrobotCamera hikrobotCamera = null;
        //public static HikrobotTcpCamera hikrobotTcpCamera = null; 

        public static List<HardwareIDModel> listPCAllow = new List<HardwareIDModel>();
        public static string JobNameSelected = "";
        public static string databasePath = "";
        public static int numberOfCodesGenerate = 0;
        public static int SelectedMaterialIndex = 0;

        // shared models
        // Dispatching
        internal static ResponseListRePrint ResponseListRePrint = new ResponseListRePrint();
        internal static PrintingMode PrintMode = new PrintingMode();
        public static bool isPushDatabase = false;
        public static int FirstGeneratedCodeIndex = 0;
        public static int LastGeneratedCodeIndex = 0;
        public static int NumberPrinted = 0;
        public static int TotalCodes = 0;
        public static int NumberOfSentSaaS = 0;
        public static int NumberOfSentSAP = 0;

        public static int NumberChecked = 0;
        public static int NumberOfCheckSentSaaS = 0;
        public static int NumberOfCheckSentSAP = 0;
        public static int NumberOfCheckSentSuccess = 0;
        public enum QRType
        {
            Box,
            Carton,
            Pallet
        }

        public enum HistoryFilter{
            All,
            Finished,
            NotFinished,
        }

        public enum PlcPrinterSignal
        {
            Red,
            Yellow,
            Green,
            Reject,
            Stop
        }

        #endregion Variables

        #region Events

        public static event EventHandler OnQrCodeCartonChange;
        public static void RaiseQrCodeCartonChangeEvent()
        {
            OnQrCodeCartonChange?.Invoke(null, EventArgs.Empty);
        }

        public static event EventHandler OnSyncDataParameterChange;
        public static void RaiseOnSyncDataParameterChangeEvent(SyncDataParams sender)
        {
            OnSyncDataParameterChange?.Invoke(sender, EventArgs.Empty);
        }

        public static event EventHandler OnSyncCheckDataParameterChange;
        public static void RaiseOnSyncCheckDataParameterChangeEvent(SyncDataParams sender)
        {
            OnSyncCheckDataParameterChange?.Invoke(sender, EventArgs.Empty);
        }

        public static event EventHandler OnNextButtonEvent;
        public static void RaiseOnNextButtonEvent()
        {
            OnNextButtonEvent?.Invoke(null, EventArgs.Empty);
        }

        public static event EventHandler OnNumberEventISCount;
        public static void RaiseOnNumberEventISCountEvent(CountEventISCamera eventCounter)
        {
            OnNumberEventISCount?.Invoke(eventCounter, EventArgs.Empty);
        }

        public static event EventHandler OnLanguageChange;
        public static void RaiseLanguageChangeEvent(String languageCode)
        {
            UILanguage.Lang.Culture = System.Globalization.CultureInfo.CreateSpecificCulture(languageCode);
            OnLanguageChange?.Invoke(languageCode, EventArgs.Empty);
        }
        public static event EventHandler OnRepeatTCPMessageChange;
        public static void RaiseOnRepeatTCPMessageChange(object tcpMessage)
        {
            OnRepeatTCPMessageChange?.Invoke(tcpMessage, EventArgs.Empty);
        }

        public static event EventHandler OnSensorControllerChangeEvent;
        public static void RaiseSensorControllerChangeEvent()
        {
            OnSensorControllerChangeEvent?.Invoke(null, EventArgs.Empty);
        }

        public static event EventHandler OnSerialDeviceControllerChangeEvent;
        public static void RaiseSerialDeviceControllerChangeEvent()
        {
            OnSerialDeviceControllerChangeEvent?.Invoke(null, EventArgs.Empty);
        }

        public static event EventHandler OnPrinterStatusChange;
        public static void RaiseOnPrinterStatusChangeEvent()
        {
            OnPrinterStatusChange?.Invoke(null, EventArgs.Empty);
        }
        public static event EventHandler OnDatabaseStatusChange;
        public static void RaiseOnDatabaseStatusChangeEvent()
        {
            OnDatabaseStatusChange?.Invoke(null, EventArgs.Empty);
        }
        public static event EventHandler OnZebraPrinterStatusChange;
        public static void RaiseOnZebraPrinterStatusChangeEvent()
        {
            OnZebraPrinterStatusChange?.Invoke(null, EventArgs.Empty);
        }
        public static event EventHandler OnPrintingStateChange;
        public static void RaiseOnPrintingStateChange()
        {
            OnPrintingStateChange?.Invoke(null, EventArgs.Empty);
        }
        public static event EventHandler OnPrinterDataChange;
        public static void RaiseOnPrinterDataChangeEvent(PODDataModel data)
        {
            OnPrinterDataChange?.Invoke(data, EventArgs.Empty);
        }
        public static event EventHandler OnCameraStatusChange;
        public static void RaiseOnCameraStatusChangeEvent()
        {
            OnCameraStatusChange?.Invoke(null, EventArgs.Empty);
        }
        public static event EventHandler OnCameraReadDataChange;
        public static void RaiseOnCameraReadDataChangeEvent(DetectModel detectModel)
        {
            OnCameraReadDataChange?.Invoke(detectModel, EventArgs.Empty);
        }

        public static event EventHandler OnCameraPositionDataChange;
        public static void RaiseOnCameraPositionDataChangeEvent(DetectModel detectModel)
        {
            OnCameraPositionDataChange?.Invoke(detectModel, EventArgs.Empty);
        }

        public static event EventHandler OnSerialDeviceReadDataChange;
        public static void RaiseOnSerialDeviceReadDataChangeEvent(DetectModel detectModel)
        {
            OnSerialDeviceReadDataChange?.Invoke(detectModel, EventArgs.Empty);
        }

        public static event EventHandler OnOperationStatusChange;
        public static void RaiseOnOperationStatusChangeEvent(OperationStatus operationStatus)
        {
            OnOperationStatusChange?.Invoke(operationStatus, EventArgs.Empty);
        }
        public static event EventHandler OnCameraOutputSignalChange;
        public static void RaiseOnCameraOutputSignalChangeEvent(object sender)
        {
            OnCameraOutputSignalChange?.Invoke(sender, EventArgs.Empty);
        }
        public static event EventHandler OnCameraTriggerOnChange;
        public static void RaiseOnCameraTriggerOnChangeEvent()
        {
            OnCameraTriggerOnChange?.Invoke(null, EventArgs.Empty);
        }

        public static event EventHandler OnCameraTriggerOffChange;
        public static void RaiseOnCameraTriggerOffChangeEvent()
        {
            OnCameraTriggerOffChange?.Invoke(null, EventArgs.Empty);
        }
        public static event EventHandler OnReceiveResponsePrinter;
        public static void RaiseOnReceiveResponsePrinter(object response)
        {
            OnReceiveResponsePrinter?.Invoke(response, EventArgs.Empty);
        }
        public static event EventHandler OnVerifyAndPrindSendDataMethod;
        public static void RaiseOnVerifyAndPrindSendDataMethod()
        {
            OnVerifyAndPrindSendDataMethod?.Invoke(true, EventArgs.Empty);
        }
        public static event EventHandler OnHanlderException;
        public static void RaiseOnOnHanlderException(Exception ex)
        {
            OnHanlderException?.Invoke(ex, UnhandledExceptionEventArgs.Empty);
        }

        public static event EventHandler OnAddSuffix;
        public static void RaiseAddSuffix(object camModel)
        {
            OnAddSuffix?.Invoke(camModel, EventArgs.Empty);
        }



        public static event EventHandler OnStopButtonClick;
        public static void RaiseOnStopButtonClick()
        {
            OnStopButtonClick?.Invoke(null, EventArgs.Empty);
        }

        public static event EventHandler OnLogError;
        public static void RaiseOnLogError(object sender)
        {
            OnLogError?.Invoke(sender, EventArgs.Empty);
        }
        #endregion Events

        #region Methods

        public static void ExportCheckedResult(string sourceFilePath)
        {
            if (Path.GetFileName(sourceFilePath) != "")
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Title = "Select destination to save the copied file";
                    saveFileDialog.FileName = Path.GetFileName(sourceFilePath);
                    saveFileDialog.Filter = "All Files|*.*";
                    saveFileDialog.Filter = "CSV files (*.csv)|*.csv|PDF files (*.pdf)|*.pdf";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            if (saveFileDialog.FileName.EndsWith(".pdf"))
                            {
                                List<string> lines = new List<string>();

                                if (File.Exists(sourceFilePath))
                                {
                                    lines.AddRange(File.ReadAllLines(sourceFilePath));
                                    FrmMain.ConvertCsvToPdf(lines.ToArray(), saveFileDialog.FileName);
                                    Process.Start("explorer.exe", $"/select,\"{saveFileDialog.FileName}\"");
                                }
                                else
                                {
                                    MessageBox.Show("File not found: " + sourceFilePath);
                                }
                            }
                            else
                            {
                                File.Copy(sourceFilePath, saveFileDialog.FileName, true);
                            Process.Start("explorer.exe", $"/select,\"{saveFileDialog.FileName}\"");
                        }

                        }
                        catch (Exception ex)
                        {
                            CustomMessageBox.Show($"Result file does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

        }

        public static PrinterSettingsModel GetSettingsPrinter()
        {
            PrinterSettingsModel printerSettingsModel = new PrinterSettingsModel();
            try
            {
                string printerIPAddress = Settings.PrinterList.FirstOrDefault().IP;
                int printerPort = Settings.PrinterList.FirstOrDefault().NumPortRemote;
                string url = string.Format("http://{0}:{1}/api/request?act=get_system_setting", printerIPAddress, printerPort);
                var request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "GET";
                request.Timeout = 1000;
                request.ContentType = "application/json";
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string responseFromServer = streamReader.ReadToEnd();
                    var printerSettingsResponse = JsonConvert.DeserializeObject<PrinterSettingsResponseModel>(responseFromServer);
                    if (printerSettingsResponse != null)
                    {
                        if (printerSettingsResponse.Success)
                        {
                            printerSettingsModel = printerSettingsResponse.data;
                        }
                    }
                }
                printerSettingsModel.IsSupportHttpRequest = true;
                return printerSettingsModel;
            }
            catch (WebException)
            {
                printerSettingsModel.IsSupportHttpRequest = false;
                return printerSettingsModel;
            }
            catch (Exception)
            {
                return printerSettingsModel;
            }

        }

        public static ActivationStatus LoginLocal(string username, string password)
        {
            LoggedInUser = UserController.Login(username, password);

            // Save Username in App Lifetime
            if (LoggedInUser != null)
            {
                Properties.Settings.Default.Username = username;
                Properties.Settings.Default.Save();
            }

            if (LoggedInUser == null)
            {
                return ActivationStatus.Failed;
            }
            else
            {
                return ActivationStatus.Successful;
            }
        }

        public static string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        #endregion

        #region Functions
        public static void LoadSettings()
        {
            try
            {
                string path = CommVariables.PathSettingsApp + "Settings.xml";
                Settings = SettingsModel.LoadSetting(path);
            }
            catch
            {
                Settings = new SettingsModel();
            }
            if (Settings.CameraList.Count <= 0)
            {
                Settings.CameraList.Add(new CameraModel { Index = 0,IP = "192.168.0.2",RoleOfCamera = RoleOfStation.ForProduct });
            }
            if (Settings.PrinterList.Count <= 0)
            {
                Settings.PrinterList.Add(new PrinterModel { Index = 0,IP = "192.168.1.2",RoleOfPrinter = RoleOfStation.ForProduct });
            }
            if (Settings.SensorControllerEncoderDiameter == null)
            {
                Settings.SensorControllerDelayBefore = new List<int>() { 0, 0 };
                Settings.SensorControllerDelayAfter = new List<int>() { 0, 0 };
                Settings.SensorControllerPulseEncoder = new List<int>() { 3600, 3600 };
                Settings.SensorControllerEncoderDiameter = new List<float>() { 48.51f, 48.51f };
                Settings.GapLength1 = new List<int>() { 0, 0 };
                Settings.Length2Error1 = new List<int>() { 0, 0 };
                Settings.DelayOutputError = new List<int>() { 0, 0 };
            }
            if(Settings.ApiDomains == null)
            {
                Settings.ApiDomains = new List<string>(){};
            }
        }

        public static void SaveSettings()
        {
            try
            {
                string path = CommVariables.PathSettingsApp;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                Settings.SaveSettings(path + "Settings.xml");
            }
            catch { }
        }

        public static CameraModel GetCameraModelBasedOnIPAddress(string ipAddress)
        {
            foreach (CameraModel cameraModel in Settings.CameraList)
            {
                if (cameraModel.IP.Equals(ipAddress) && cameraModel.IsEnable)
                {
                    return cameraModel;
                }
            }
            return null;
        }

        public static bool CheckJobHasExist(string templateNameWithoutExtension)
        {
            string filePath = CommVariables.PathJobsApp + templateNameWithoutExtension + Settings.JobFileExtension;
            return File.Exists(filePath);
        }

        public static bool DeleteJob(JobModel templatePath)
        {
            string filePath = CommVariables.PathJobsApp + templatePath.FileName + Settings.JobFileExtension;
            string filePathCheckResult = CommVariables.PathCheckedResult + templatePath.CheckedResultPath;
            string filePathPrintedResponse = CommVariables.PathPrintedResponse + templatePath.PrintedResponePath;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            if (File.Exists(filePathCheckResult))
            {
                File.Delete(filePathCheckResult);
            }
            if (File.Exists(filePathPrintedResponse))
            {
                File.Delete(filePathPrintedResponse);
            }
            return true;
        }

        public static JobModel GetJob(string templateNameWithExtension)
        {
            string filePath = CommVariables.PathJobsApp + templateNameWithExtension;
            return JobModel.LoadFile(filePath);
        }

        public static List<string> GetJobNameList()
        {
            try
            {
                string folderPath = CommVariables.PathJobsApp;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var dir = new DirectoryInfo(folderPath);
                var strFileNameExtension = string.Format("*{0}",Settings.JobFileExtension);
                FileInfo[] files = dir.GetFiles(strFileNameExtension).OrderByDescending(x => x.CreationTime).ToArray();
                var result = new List<string>();
                foreach (FileInfo file in files)
                {
                    result.Add(file.Name);
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetReadStringFromResultXml(string resultXml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(resultXml);
                XmlNode fullStringNode = doc.SelectSingleNode("result/general/full_string");
                if (fullStringNode != null)
                {
                    XmlAttribute encoding = fullStringNode.Attributes["encoding"];
                    if (encoding != null && encoding.InnerText == "base64")
                    {
                        if (!string.IsNullOrEmpty(fullStringNode.InnerText))
                        {
                            byte[] code = Convert.FromBase64String(fullStringNode.InnerText);
                            return Encoding.UTF8.GetString(code,0,code.Length);
                        }
                        else
                        {
                            return "";
                        }
                    }

                    return fullStringNode.InnerText;
                }
            }
            catch (Exception)
            {
            }

            return "";
        }

        public static bool GetCameraStatus()
        {
            foreach (CameraModel cameraModel in Settings.CameraList)
            {
                if (cameraModel.IsEnable && !cameraModel.IsConnected)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool GetPrinterStatus()
        {
            foreach (PrinterModel printerModel in Settings.PrinterList)
            {
                if (printerModel.IsEnable && !printerModel.IsConnected)
                {
                    return false;
                }
            }
            return true;
        }

        //  const string commandErrorOutput = "(R00001001000000100000000000000000000000000000000000000000000000000000000)";

        public static string ResumeA = "(T11000000000000000000000000000000000000000000000000000000000000000000000000000000000)";
        public static string ResumeAB = "(T10100000000000000000000000000000000000000000000000000000000000000000000000000000000)";
        public static void SendErrorOutputToSensorController(int Index)
        {
            if (Settings.CameraList.FirstOrDefault().IsIndexCommandEnable)
            {
                string formattedCompareIndex = Index.ToString("D7"); // Formats as a 7-digit number

                switch (Settings.PLCVersion)
                {
                    case 0:
                        SensorController.Send((Settings.CameraList.FirstOrDefault()).CommandErrorOutput + formattedCompareIndex);
                        break;
                    case 1:
                        SensorController.Send((Settings.CameraList.FirstOrDefault()).CommandErrorOutput + formattedCompareIndex);
                        break;
                    case 2:
                        SensorController.Send("(C00005)" + formattedCompareIndex);
                        break;

                }

                if (SensorController.IsConnected2())
                {
                    SensorController.Send2((Settings.CameraList.FirstOrDefault()).CommandErrorOutput + formattedCompareIndex);
                }
            }
            else
            {
                switch (Settings.PLCVersion)
                {
                    case 0:
                        SensorController.Send(Settings.CameraList.FirstOrDefault().CommandErrorOutput);
                        break;
                    case 1:
                        SensorController.Send(Settings.CameraList.FirstOrDefault().CommandErrorOutput);
                        break;
                    case 2:
                        SensorController.Send2("(C00005)");
                        break;

                }

                if (Settings.PLCVersion != 2)
                {
                    SensorController.Send2(Settings.CameraList.FirstOrDefault().CommandErrorOutput);
                }
            }
            //SensorController.Send("1");

        }

        public static void SendCommandToSensorController(string command)
        {
            if (SensorController != null && IsSensorControllerConnected)
            {
                SensorController.Send(command);
            }
        }

        public static void SendCommandToSensor2Controller(string command)
        {
            if (SensorController != null && IsSensorControllerConnected)
            {
                SensorController.Send2(command);
            }
        }

        public static void SendPrinterSignalToPlc(PlcPrinterSignal signal)
        {
            if (SensorController == null || !IsSensorControllerConnected)
                return;

            string command;
            switch (signal)
            {
                case PlcPrinterSignal.Red: command = "ARED"; break;
                case PlcPrinterSignal.Yellow: command = "AYELLOW"; break;
                case PlcPrinterSignal.Green: command = "AGREEN"; break;
                case PlcPrinterSignal.Reject: command = "REJECT"; break;
                case PlcPrinterSignal.Stop: command = "AREDSTOP"; break;
                default: return;
            }
            SensorController.Send(command);
        }

        public static void SendSettingToSensorController()
        {
            try
            {
                if (SensorController != null && IsSensorControllerConnected)
                {
                    bool includeError = Settings.PLCVersion == 1;
                    string strCommand = "(" + BuildSensorSegment(0, includeError) + BuildSensorSegment(1, includeError) + ")";
                    SensorController.Send(strCommand);
                }
            }
            catch { }
        }

        private static string BuildSensorSegment(int index, bool includeDelayOutput)
        {
            string strPulseEncoder = Settings.SensorControllerPulseEncoder[index].ToString("D5");
            float encoderDiameter = Settings.SensorControllerEncoderDiameter[index] * 100.0f;
            string strEncoderDiameter = ((int)encoderDiameter).ToString("D5");
            string strSensorDisableLength = Settings.SensorControllerDelayBefore[index].ToString("D5");
            string strSensorEnableLength = Settings.SensorControllerDelayAfter[index].ToString("D5");
            string strGapLength = Settings.GapLength1[index].ToString("D5");
            string strLength2Err = Settings.Length2Error1[index].ToString("D5");

            string EncoderModeCharacter = (Settings.PLCVersion == 2
                                        && Settings.EncoderMode == SettingsModel.ResumeEncoderMode.Internal
                                        && index == 1
                                        ) ? "I" : "P";

            string segment = string.Format("{6}{0}D{1}L{2}H{3}G{4}E{5}",
                                strPulseEncoder,
                                strEncoderDiameter,
                                strSensorDisableLength,
                                strSensorEnableLength,
                                strGapLength,
                                strLength2Err,
                                EncoderModeCharacter
                            );

            //string segment = string.Format("P{0}D{1}L{2}H{3}G{4}E{5}",
            //    strPulseEncoder,
            //    strEncoderDiameter,
            //    strSensorDisableLength,
            //    strSensorEnableLength,
            //    strGapLength,
            //    strLength2Err
            //);

            if (includeDelayOutput)
            {
                string strDelayOutputError = Settings.DelayOutputError[index].ToString("D5");
                segment += "O" + strDelayOutputError;
            }

            return segment;
        }

        #endregion

        #region Zebra Printer Functions

        /// <summary>
        /// Prints text using Zebra printer settings. Can be called from anywhere in the application.
        /// </summary>
        /// <param name="text">The text to print (will be used for both text content and barcode data if enabled)</param>
        /// <returns>True if print command was sent successfully, false otherwise</returns>
        public static bool PrintZebra(string text)
        {
            try
            {
                if (Settings?.ZebraPrinter == null || Settings.ZebraPrinter.ZebraSettings == null)
                {
                    return false;
                }

                var zebraSettings = Settings.ZebraPrinter.ZebraSettings;
                var podController = Settings.ZebraPrinter.PODController;

                if (podController == null)
                {
                    return false;
                }

                List<string> zplCommands = new List<string>();
                zplCommands.Add("^XA"); // Start label

                // Add text if enabled - always use the input text parameter
                if (zebraSettings.EnableText)
                {
                    string font = GetZebraFontChar(zebraSettings.TextFont);
                    string zplText = GenerateZPLText(
                        zebraSettings.TextX,
                        zebraSettings.TextY,
                        font,
                        zebraSettings.TextFontSize,
                        text  // Always use the input text parameter
                    );
                    zplCommands.Add(zplText);
                }

                // Add barcode if enabled - always use the input text parameter
                if (zebraSettings.EnableBarcode)
                {
                    string barcodeText = zebraSettings.BarcodeText;
                    string zplBarcode = GenerateZPLBarcode(
                        zebraSettings.BarcodeX,
                        zebraSettings.BarcodeY,
                        zebraSettings.BarcodeType,
                        zebraSettings.BarcodeSize,
                        text,  // Always use the input text parameter
                        barcodeText
                    );
                    zplCommands.Add(zplBarcode);
                }

                // Check if at least one element is enabled
                if (!zebraSettings.EnableText && !zebraSettings.EnableBarcode)
                {
                    return false;
                }

                zplCommands.Add("^XZ"); // End label
                string zpl = string.Join("\n", zplCommands);

                return podController.Send(zpl);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool PrintZebraDroco(string qrData, QRType qrType)
        {
            try
            {
                if (Settings?.ZebraPrinter == null || Settings.ZebraPrinter.ZebraSettings == null)
                    return false;

                var zebraSettings = Settings.ZebraPrinter.ZebraSettings;
                string barcodeType = zebraSettings.BarcodeType;

                List<string> zplCommands = new List<string>();
                zplCommands.Add("^XA");
                zplCommands.Add($"^LL{zebraSettings.LabelLength}");

                if (zebraSettings.EnableText)
                {
                    string font = GetZebraFontChar(zebraSettings.TextFont);
                    zplCommands.Add(GenerateZPLText(
                        zebraSettings.TextX, zebraSettings.TextY,
                        font, zebraSettings.TextFontSize, qrData));
                }

                if (zebraSettings.EnableBarcode)
                {
                    if (qrType == QRType.Pallet)
                        barcodeType = "QR Code";
                    else if (qrType == QRType.Box || qrType == QRType.Carton)
                        barcodeType = "GS1-128";

                    zplCommands.Add(GenerateZPLBarcode(
                        zebraSettings.BarcodeX,
                        zebraSettings.BarcodeY,
                        barcodeType,
                        zebraSettings.BarcodeSize,
                        qrData,
                        zebraSettings.BarcodeText,
                        zebraSettings.BarcodeModuleWidth,
                        zebraSettings.TextFontSize));
                }

                if (!zebraSettings.EnableText && !zebraSettings.EnableBarcode)
                    return false;

                zplCommands.Add("^XZ");
                string zpl = string.Join("\n", zplCommands);

                // ── USB mode ──────────────────────────────────────────────
                if (zebraSettings.ConnectionType == ZebraConnectionType.USB)
                {
                    string usbName = zebraSettings.UsbPrinterName;
                    if (string.IsNullOrEmpty(usbName)) return false;
                    return RawPrinterHelper.SendStringToPrinter(usbName, zpl);
                }

                // ── LAN mode ──────────────────────────────────────────────
                var podController = Settings.ZebraPrinter.PODController;
                if (podController == null) return false;
                return podController.Send(zpl);
            }
            catch (Exception)
            {
                return false;
            }
        }
        //public static bool PrintZebraDroco(string qrData, QRType qrType)
        //{
        //    try
        //    {
        //        if (Settings?.ZebraPrinter == null || Settings.ZebraPrinter.ZebraSettings == null)
        //            return false;

        //        var zebraSettings = Shared.Settings.ZebraPrinter.ZebraSettings;
        //        string barcodeType = zebraSettings.BarcodeType;
        //        var podController = Settings.ZebraPrinter.PODController;

        //        if (podController == null)
        //            return false;

        //        List<string> zplCommands = new List<string>();
        //        zplCommands.Add("^XA");                              // ^XA trước
        //        zplCommands.Add($"^LL{zebraSettings.LabelLength}"); // ^LL sau

        //        if (zebraSettings.EnableText)
        //        {
        //            string font = GetZebraFontChar(zebraSettings.TextFont);
        //            zplCommands.Add(GenerateZPLText(
        //                zebraSettings.TextX, zebraSettings.TextY,
        //                font, zebraSettings.TextFontSize, qrData));
        //        }

        //        if (zebraSettings.EnableBarcode)
        //        {
        //            if (qrType == QRType.Pallet)
        //                barcodeType = "QR Code";
        //            else if (qrType == QRType.Box || qrType == QRType.Carton)
        //                barcodeType = "GS1-128";

        //            zplCommands.Add(GenerateZPLBarcode(
        //                zebraSettings.BarcodeX,
        //                zebraSettings.BarcodeY,
        //                barcodeType,
        //                zebraSettings.BarcodeSize,
        //                qrData,
        //                zebraSettings.BarcodeText,
        //                zebraSettings.BarcodeModuleWidth, // ← truyền đúng moduleWidth
        //                zebraSettings.TextFontSize));      // ← truyền đúng hriTextSize
        //        }

        //        if (!zebraSettings.EnableText && !zebraSettings.EnableBarcode)
        //            return false;

        //        zplCommands.Add("^XZ");
        //        return podController.Send(string.Join("\n", zplCommands));
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
        /// <summary>
        /// Prints text with custom content. Can override default text/barcode content from settings.
        /// </summary>
        /// <param name="textContent">Text content to print (if text is enabled)</param>
        /// <param name="barcodeData">Barcode data to print (if barcode is enabled)</param>
        /// <returns>True if print command was sent successfully, false otherwise</returns>
        public static bool PrintZebra(string textContent, string barcodeData)
        {
            try
            {
                if (Settings?.ZebraPrinter == null || Settings.ZebraPrinter.ZebraSettings == null)
                {
                    return false;
                }

                var zebraSettings = Settings.ZebraPrinter.ZebraSettings;
                var podController = Settings.ZebraPrinter.PODController;

                if (podController == null)
                {
                    return false;
                }

                List<string> zplCommands = new List<string>();
                zplCommands.Add("^XA"); // Start label

                // Add text if enabled
                if (zebraSettings.EnableText && !string.IsNullOrWhiteSpace(textContent))
                {
                    string font = GetZebraFontChar(zebraSettings.TextFont);
                    string zplText = GenerateZPLText(
                        zebraSettings.TextX,
                        zebraSettings.TextY,
                        font,
                        zebraSettings.TextFontSize,
                        textContent
                    );
                    zplCommands.Add(zplText);
                }

                // Add barcode if enabled
                if (zebraSettings.EnableBarcode && !string.IsNullOrWhiteSpace(barcodeData))
                {
                    string barcodeText = zebraSettings.BarcodeText;
                    string zplBarcode = GenerateZPLBarcode(
                        zebraSettings.BarcodeX,
                        zebraSettings.BarcodeY,
                        zebraSettings.BarcodeType,
                        zebraSettings.BarcodeSize,
                        barcodeData,
                        barcodeText
                    );
                    zplCommands.Add(zplBarcode);
                }

                // Check if at least one element is enabled
                if (!zebraSettings.EnableText && !zebraSettings.EnableBarcode)
                {
                    return false;
                }

                zplCommands.Add("^XZ"); // End label
                string zpl = string.Join("\n", zplCommands);

                return podController.Send(zpl);
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Kiểm tra máy in Zebra đã sẵn sàng (hỗ trợ cả USB và LAN).
        /// Thay thế toàn bộ guard PODController.IsConnected() trước khi gọi PrintZebraDroco.
        /// </summary>
        public static bool IsZebraPrinterReady()
        {
            try
            {
                var zebraSettings = Settings?.ZebraPrinter?.ZebraSettings;
                if (zebraSettings == null) return false;

                if (zebraSettings.ConnectionType == ZebraConnectionType.USB)
                {
                    string usbName = zebraSettings.UsbPrinterName;
                    if (string.IsNullOrEmpty(usbName)) return false;
                    return RawPrinterHelper.IsPrinterOnline(usbName);
                }

                // LAN mode
                return Settings.ZebraPrinter?.PODController?.IsConnected() == true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Extracts font character from font string (e.g., "A - Smallest" -> "A")
        /// </summary>
        private static string GetZebraFontChar(string fontString)
        {
            if (string.IsNullOrWhiteSpace(fontString))
                return "A";

            // Extract first character (A, B, C, D, E, F, G, H, or 0)
            return fontString[0].ToString();
        }

        /// <summary>
        /// Generates ZPL command for text
        /// </summary>
        private static string GenerateZPLText(int x, int y, string font, int fontSize, string text)
        {
            // ^FOx,y^AFont,height,width^FDtext^FS
            // Font: A, B, C, D, E, F, G, H, or 0 (scalable)
            char fontChar = font[0];
            return $"^FO{x},{y}^A{fontChar}N,{fontSize},{fontSize}^FD{text}^FS";
        }

        /// <summary>
        /// Generates ZPL command for barcode
        /// </summary>
        /// <summary>
        /// Generates ZPL command for barcode
        /// </summary>
        private static string GenerateZPLBarcode(int x, int y, string barcodeType,
            int size, string data, string textLabel = null, int moduleWidth = 1, int hriTextSize = 0)
        {
            string zpl = "";
            string byCmd = (barcodeType == "GS1-128" || barcodeType == "Code 128"
                 || barcodeType == "Code 39" || barcodeType == "EAN-13"
                 || barcodeType == "EAN-8" || barcodeType == "UPC-A"
                 || barcodeType == "UPC-E")
                 ? $"^BY{moduleWidth}"
                 : "";

            switch (barcodeType)
            {
                case "QR Code":
                    zpl = $"^FO{x},{y}^BQN,2,{size}^FDQA,{data}^FS";
                    break;

                case "GS1-128":
                case "Code 128":
                    {
                        int barcodeHeight = size * 10;
                        string fdData = barcodeType == "GS1-128" ? $">8{data}" : data;

                        if (moduleWidth == 1)
                        {
                            // Đơn giản: in HRI dưới barcode, không tính gì phức tạp
                            // - hriSize = TextFontSize (user điều chỉnh qua settings)
                            // - width = height → ký tự vuông, không bị dính, không compressed
                            // - Không dùng ^FB, không tính barcodeDots
                            int hriSize = hriTextSize > 0 ? hriTextSize : 28;
                            int hriY = y + barcodeHeight + 6;

                            string ai = data.Length >= 2 ? data.Substring(0, 2) : data;
                            string rest = data.Length > 2 ? data.Substring(2) : "";
                            string hriText = $"({ai}){rest}";

                          
                            string manualHri = $"^FO{x},{hriY}^A0N,{hriSize},{hriSize}^FD{hriText}^FS";
                            zpl = $"^FO{x},{y}{byCmd}^BCN,{barcodeHeight},N,N,N^FD{fdData}^FS{manualHri}";
                        }
                        else
                        {
                            // moduleWidth>1: built-in HRI, không bị dính
                            zpl = $"^FO{x},{y}{byCmd}^BCN,{barcodeHeight},Y,N,N^FD{fdData}^FS";
                        }
                        break;
                    }

                case "DataMatrix":
                    zpl = $"^FO{x},{y}^BXN,{size},200^FD{data}^FS";
                    break;

                case "Code 39":
                    zpl = $"^FO{x},{y}{byCmd}^B3N,{size * 10},Y,N,N^FD{data}^FS";
                    break;

                case "EAN-13":
                    zpl = $"^FO{x},{y}{byCmd}^BEN,{size * 10},Y,N,N^FD{data}^FS";
                    break;

                case "EAN-8":
                    zpl = $"^FO{x},{y}{byCmd}^B8N,{size * 10},Y,N,N^FD{data}^FS";
                    break;

                case "UPC-A":
                    zpl = $"^FO{x},{y}{byCmd}^BUN,{size * 10},Y,N,N^FD{data}^FS";
                    break;

                case "UPC-E":
                    zpl = $"^FO{x},{y}{byCmd}^B9N,{size * 10},Y,N,N^FD{data}^FS";
                    break;

                default:
                    zpl = $"^FO{x},{y}^BQN,2,{size}^FDQA,{data}^FS";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(textLabel))
            {
                int labelY = y + (size * 20) + 10;
                zpl += $"\n^FO{x},{labelY}^A0N,20,20^FD{textLabel}^FS";
            }

            return zpl;
        }
        #endregion
    }
}
