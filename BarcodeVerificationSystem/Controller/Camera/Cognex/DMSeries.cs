using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Utils;
using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Discovery;
using Cognex.DataMan.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using static System.Windows.Forms.AxHost;

namespace BarcodeVerificationSystem.Controller.Camera
{
    public class DMSeries : CamerasHandler
    {
        #region Variables
        private SynchronizationContext _SyncContext = null;
        internal EthSystemDiscoverer _EthSystemDiscoverer = null;
        internal SerSystemDiscoverer _SerSystemDiscoverer = null;
        internal List<object> _CameraSystemInfoList = new List<object>();
        internal List<DataManSystem> _DataManSystemList = new List<DataManSystem>();
        private readonly object _CurrentResultInfoSyncLock = new object();
        internal event EventHandler UpdateLabelStatusEvent;
        DataManSystem _dataManSystem;
        ISystemConnector _connector;
        ResultCollector _resultCollector;

        private readonly string _endOfLineStr = "<EOF>";
        public override string IPAddress { get; set; }
        public override string Port { get; set; }
        #endregion Variables

        #region Cognex_Camera
        internal void InitCameraVariables()
        {

            _SyncContext = SynchronizationContext.Current;

            _EthSystemDiscoverer = new EthSystemDiscoverer();
            _SerSystemDiscoverer = new SerSystemDiscoverer();
              
            _EthSystemDiscoverer.SystemDiscovered += new EthSystemDiscoverer.SystemDiscoveredHandler(OnEthSystemDiscovered);
            _SerSystemDiscoverer.SystemDiscovered += new SerSystemDiscoverer.SystemDiscoveredHandler(OnSerSystemDiscovered);

            _EthSystemDiscoverer.Discover();
            _SerSystemDiscoverer.Discover();
        }
        public override void Connect(string ipadd, string port = null)
        {
            try
            {

                if (_CameraSystemInfoList.Count <= 0)
                {
                    CameraModel cameraModel = Shared.Settings.CameraList.FirstOrDefault();
                    cameraModel.IsConnected = false;
                    Shared.RaiseOnCameraStatusChangeEvent();
                }

                foreach (var systemInfo in _CameraSystemInfoList)
                {
                    //ISystemConnector iSysConnector = null;
                    if (systemInfo is EthSystemDiscoverer.SystemInfo)
                    {
                        EthSystemDiscoverer.SystemInfo ethSystemInfo = systemInfo as EthSystemDiscoverer.SystemInfo;
                        using (EthSystemConnector conn = new EthSystemConnector(ethSystemInfo.IPAddress))
                        {
                            CameraModel cameraModel = Shared.GetCameraModelBasedOnIPAddress(ethSystemInfo.IPAddress.ToString());
                            if (cameraModel != null && cameraModel.IP == ipadd)
                            {
                                conn.UserName = cameraModel.UserName;
                                conn.Password = cameraModel.Password;
                                cameraModel.Name = ethSystemInfo.Name;
                                cameraModel.SerialNumber = ethSystemInfo.SerialNumber;
                            }
                            else
                            {
                                UpdateLabelStatusEvent?.Invoke(this, EventArgs.Empty);
                                Shared.RaiseOnCameraStatusChangeEvent();
                                continue;
                            }

                            _connector = conn;
                        }
                    }
                    else if (systemInfo is SerSystemDiscoverer.SystemInfo)
                    {
                        SerSystemDiscoverer.SystemInfo ser_system_info = systemInfo as SerSystemDiscoverer.SystemInfo;
                        SerSystemConnector conn = new SerSystemConnector(ser_system_info.PortName, ser_system_info.Baudrate);
                        _connector = conn;
                    }

                    _dataManSystem = new DataManSystem(_connector);
                    _dataManSystem.DefaultTimeout = 1000;
                    _dataManSystem.SystemConnected += new SystemConnectedHandler(OnSystemConnected);
                    _dataManSystem.SystemDisconnected += new SystemDisconnectedHandler(OnSystemDisconnected);

                    _dataManSystem.SystemWentOnline += new SystemWentOnlineHandler(OnSystemWentOnline);
                    _dataManSystem.SystemWentOffline += new SystemWentOfflineHandler(OnSystemWentOffline);
                    ResultTypes resultTypes = ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;
                    _resultCollector = new ResultCollector(_dataManSystem, resultTypes);
                    _resultCollector.ComplexResultCompleted += ResultCollector_ComplexResultCompleted;
                    _dataManSystem.Connect();

                    try
                    {
                        _dataManSystem.SetResultTypes(resultTypes);
                    }
                    catch (Exception) { }

                    _DataManSystemList.Add(_dataManSystem);
                }

            }
            catch (Exception)
            {
                CleanupConnection();
            }
        }

        public void MultiReadConnect(string ipadd, string port = null)
        {
            try
            {
                if (_CameraSystemInfoList.Count <= 0)
                {
                    CameraModel cameraModel = Shared.Settings.CameraList.FirstOrDefault();
                    cameraModel.IsConnected = false;
                    Shared.RaiseOnCameraStatusChangeEvent();
                }

                foreach (var systemInfo in _CameraSystemInfoList)
                {
                    //ISystemConnector iSysConnector = null;
                    if (systemInfo is EthSystemDiscoverer.SystemInfo)
                    {
                        EthSystemDiscoverer.SystemInfo ethSystemInfo = systemInfo as EthSystemDiscoverer.SystemInfo;
                        using (EthSystemConnector conn = new EthSystemConnector(ethSystemInfo.IPAddress))
                        {
                            CameraModel cameraModel = Shared.GetCameraModelBasedOnIPAddress(ethSystemInfo.IPAddress.ToString());
                            if (cameraModel != null && cameraModel.IP == ipadd)
                            {
                                conn.UserName = cameraModel.UserName;
                                conn.Password = cameraModel.Password;
                                cameraModel.Name = ethSystemInfo.Name;
                                cameraModel.SerialNumber = ethSystemInfo.SerialNumber;
                            }
                            else
                            {
                                UpdateLabelStatusEvent?.Invoke(this, EventArgs.Empty);
                                Shared.RaiseOnCameraStatusChangeEvent();
                                continue;
                            }

                            _connector = conn;
                        }
                    }
                    else if (systemInfo is SerSystemDiscoverer.SystemInfo)
                    {
                        SerSystemDiscoverer.SystemInfo ser_system_info = systemInfo as SerSystemDiscoverer.SystemInfo;
                        SerSystemConnector conn = new SerSystemConnector(ser_system_info.PortName, ser_system_info.Baudrate);
                        _connector = conn;
                    }

                    _dataManSystem = new DataManSystem(_connector);
                    _dataManSystem.SystemConnected += new SystemConnectedHandler(OnSystemConnected);
                    _dataManSystem.SystemDisconnected += new SystemDisconnectedHandler(OnSystemDisconnected);
                    _dataManSystem.SystemWentOnline += new SystemWentOnlineHandler(OnSystemWentOnline);
                    _dataManSystem.SystemWentOffline += new SystemWentOfflineHandler(OnSystemWentOffline);
                    _dataManSystem.Connect();
                    _DataManSystemList.Add(_dataManSystem);
                }

            }
            catch (Exception)
            {
                CleanupConnection();
            }
        }
        public void Disconnect()
        {
            try
            {
                _DataManSystemList.Clear();
                if (_dataManSystem == null || _resultCollector == null) return;
                _dataManSystem?.Disconnect();
                _dataManSystem = null;
                CleanupConnection();
                _resultCollector?.ClearCachedResults();
                _resultCollector = null;
            }
            catch { }
        }
        private void OnEthSystemDiscovered(EthSystemDiscoverer.SystemInfo systemInfo)
        {
            _SyncContext.Post(
                new SendOrPostCallback(
                    delegate
                    {
                        Console.WriteLine(string.Format("IP camera: {0}", systemInfo.IPAddress.ToString()));
                        CameraModel cameraModel = Shared.GetCameraModelBasedOnIPAddress(systemInfo.IPAddress.ToString());
                        bool hasExist = CheckCameraInfoHasExist(systemInfo, _CameraSystemInfoList);
                        if (cameraModel != null && hasExist == false)
                        {
                            _CameraSystemInfoList.Add(systemInfo);
                        }
                    }),
                    null);
        }
        private void OnSerSystemDiscovered(SerSystemDiscoverer.SystemInfo systemInfo)
        {
            _SyncContext.Post(
                new SendOrPostCallback(
                    delegate
                    {

                    }),
                    null);
        }
        private bool CheckCameraInfoHasExist(EthSystemDiscoverer.SystemInfo cameraInfoNeedCheck, List<object> cameraSystemInfoList)
        {
            foreach (var systemInfo in cameraSystemInfoList)
            {
                if (systemInfo is EthSystemDiscoverer.SystemInfo)
                {
                    EthSystemDiscoverer.SystemInfo ethSystemInfo = systemInfo as EthSystemDiscoverer.SystemInfo;
                    if (ethSystemInfo.SerialNumber == cameraInfoNeedCheck.SerialNumber)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void CleanupConnection()
        {
            try
            {
                _connector = null;
                _dataManSystem = null;
                foreach (DataManSystem dataManSystem in _DataManSystemList)
                {
                    dataManSystem.Disconnect();
                    dataManSystem.SystemConnected -= OnSystemConnected;
                    dataManSystem.SystemDisconnected -= OnSystemDisconnected;
                }
                _DataManSystemList?.Clear();
                _CameraSystemInfoList?.Clear();
            }
            catch (Exception)
            {
            }
        }
        private void OnSystemConnected(object sender, EventArgs args)
        {
            _SyncContext.Post(
                delegate
                {
                    Console.WriteLine("OnSystemConnected");
                    if (sender is DataManSystem)
                    {
                        DataManSystem dataManSystem = sender as DataManSystem;
                        EthSystemConnector ethSystemConnector = dataManSystem.Connector as EthSystemConnector;
                        CameraModel cameraModel = Shared.GetCameraModelBasedOnIPAddress(ethSystemConnector.Address.ToString());
                        if (cameraModel != null)
                        {
                            cameraModel.IsConnected = true;
                        }
                    }
                    UpdateLabelStatusEvent?.Invoke(this, EventArgs.Empty);
                    Shared.RaiseOnCameraStatusChangeEvent();
                },
                null);
        }
        private void OnSystemDisconnected(object sender, EventArgs args)
        {
            _SyncContext.Post(
                delegate
                {
                    if (sender is DataManSystem)
                    {
                        DataManSystem dataManSystem = sender as DataManSystem;
                        EthSystemConnector ethSystemConnector = dataManSystem.Connector as EthSystemConnector;
                        CameraModel cameraModel = Shared.GetCameraModelBasedOnIPAddress(ethSystemConnector.Address.ToString());
                        if (cameraModel != null)
                        {
                            cameraModel.IsConnected = false;
                            cameraModel.Name = "";
                            cameraModel.SerialNumber = "";
                        }
                    }
                    UpdateLabelStatusEvent?.Invoke(this, EventArgs.Empty);
                    Shared.RaiseOnCameraStatusChangeEvent();
                },
                null);
        }
        private void OnSystemWentOnline(object sender, EventArgs args)
        {
            _SyncContext.Post(
                delegate
                {
                },
                null);
        }
        private void OnSystemWentOffline(object sender, EventArgs args)
        {
            _SyncContext.Post(
                delegate
                {
                },
                null);
        }
        private void ResultCollector_ComplexResultCompleted(object sender, ComplexResult e)
        {
            _SyncContext.Post(
                delegate
                {
                    ProcessComplexResultCompleted(sender, e);
                },
                null);
        }

        private void ProcessComplexResultCompleted(object sender, ComplexResult complexResult)
        {
            CameraModel cameraModel = null;
            if (sender is ResultCollector)
            {
                var resultCollector = sender as ResultCollector;
                var field = resultCollector.GetType().GetField("_dmSystem", BindingFlags.NonPublic | BindingFlags.Instance);
                var dataManSystem = (DataManSystem)field.GetValue(resultCollector);
                if (dataManSystem != null)
                {
                    var ethSystemConnector = dataManSystem.Connector as EthSystemConnector;
                    cameraModel = Shared.GetCameraModelBasedOnIPAddress(ethSystemConnector.Address.ToString());
                }
            }
            Image imageResult = null;
            string strResult = "";
            string codeQuality = " ";
            var imageGraphics = new List<string>();
            lock (_CurrentResultInfoSyncLock)
            {
                foreach (var simpleResult in complexResult.SimpleResults)
                {
                    try
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(simpleResult.GetDataAsString());

                        XmlNode gradeNode = doc.SelectSingleNode("//symbol_grade/grade");

                        if (gradeNode != null)
                        {
                            codeQuality = gradeNode.InnerText;
                        }
                    }
                    catch (Exception)
                    {
                    }


                    string getStringVar = strResult.Replace("\r\n", "");

                    switch (simpleResult.Id.Type)
                    {
                        case ResultTypes.Image:
                            imageResult = ImageArrivedEventArgs.GetImageFromImageBytes(simpleResult.Data);
                            break;

                        case ResultTypes.ImageGraphics:
                            imageGraphics.Add(simpleResult.GetDataAsString());
                            break;

                        case ResultTypes.ReadString:
                            strResult = Encoding.UTF8.GetString(simpleResult.Data);
                            break;

                        case ResultTypes.ReadXml:
                            strResult = Shared.GetReadStringFromResultXml(simpleResult.GetDataAsString());
                            break;

                        default:
                            break;
                    }
                }
            }
            try
            {
                var bitmap = new Bitmap(100, 100);
                if (imageResult != null)
                {
                    bitmap = ((Bitmap)imageResult).Clone(new Rectangle(0, 0, imageResult.Width, imageResult.Height), PixelFormat.Format24bppRgb);
                }
                else
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.Clear(Color.White);
                    }
                }

                if (imageGraphics.Count > 0)
                {
                    using (var graphicsImage = Graphics.FromImage(bitmap))
                    {
                        foreach (string graphics in imageGraphics)
                        {
                            var resultGraphics = GraphicsResultParser.Parse(graphics, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                            ResultGraphicsRenderer.PaintResults(graphicsImage, resultGraphics);
                        }
                    }
                }

                string getStringVar = strResult.Replace("\r\n", "");
                if (getStringVar.Equals(_endOfLineStr))
                {
                    getStringVar = "";
                }
                else
                {
                    getStringVar = getStringVar.Replace(_endOfLineStr, "");
                }
                var detectModel = new DetectModel
                {
                    Image = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat),
                    Text = getStringVar,
                    CodeQuality = codeQuality,
                };

                if (cameraModel != null)
                {
                    detectModel.RoleOfCamera = cameraModel.RoleOfCamera;
                }
                if (Shared.Settings.CameraList.FirstOrDefault().ReadMode == CameraModeRead.Basic)
                {
                    Shared.RaiseOnCameraReadDataChangeEvent(detectModel);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("ProcessComplexResultCompleted" + ex.Message);
            }
        }
        #endregion Cognex_Camera
    }
}
