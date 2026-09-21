using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.View;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Controller
{

    public class POD_Custom
    {
        private string _ServerIP = "127.0.0.1";
        private int _Port = 1997;
        private int _Port2 = 1998;
        private RoleOfStation _RoleOfPrinter = RoleOfStation.ForProduct;
        private readonly int _TimeOutOfConnection = 5000;
        private readonly int _SendTimeout = 1000;
        private readonly byte _StartPackage = 0x02;
        protected byte _EndPackage = 0x03;
        private readonly bool _IsVersion = false;
        private TcpClient _TcpClient;
        private TcpClient _TcpClient2;
        private NetworkStream _NetworkStream;
        private StreamReader _StreamReader = null;
        private StreamWriter _StreamWriter = null;
        private Thread _ThreadReceiveData = null;
        private Thread _ThreadReceiveData2 = null;
        private NetworkStream _NetworkStream2;
        private StreamReader _StreamReader2 = null;
        private StreamWriter _StreamWriter2 = null;
        public event EventHandler OnPODReceiveMessageEvent;
        public event EventHandler OnPOD2ReceiveMessageEvent;
        public event EventHandler OnPODReceiveDataEvent;
        public string ServerIP
        {
            get { return _ServerIP; }
            set { _ServerIP = value; Disconnect(); }
        }
        public int Port
        {
            get { return _Port; }
            set { _Port = value; Disconnect(); }
        }
        public int Port2
        {
            get { return _Port2; }
            set { _Port2 = value; Disconnect(); }
        }

        public RoleOfStation RoleOfPrinter
        {
            get { return _RoleOfPrinter; }
            set { _RoleOfPrinter = value; }
        }

        public POD_Custom(string serverIP, int port, int port2, int timeOutOfConnection, int sendTimeout)
        {
            _ServerIP = serverIP;
            _Port = port;
            _Port2 = port2;
            _TimeOutOfConnection = timeOutOfConnection;
            _SendTimeout = sendTimeout;
        }
        public POD_Custom(string serverIP, int port, RoleOfStation roleOfPrinter, int timeOutOfConnection, int sendTimeout, bool isVersion)
        {
            _ServerIP = serverIP;
            _Port = port;
            _RoleOfPrinter = roleOfPrinter;
            _TimeOutOfConnection = timeOutOfConnection;
            _SendTimeout = sendTimeout;
            _IsVersion = isVersion;
            _IsVersion = false;
        }

        public bool Connect()
        {
            try
            {
                _TcpClient = new TcpClient();
                var task = _TcpClient.ConnectAsync(_ServerIP, _Port);
                task.Wait(_TimeOutOfConnection);
                if (!task.IsCompleted)
                {
                    Disconnect();
                    return false;
                }
                _TcpClient.SendTimeout = _SendTimeout;
                _NetworkStream = _TcpClient.GetStream();
                _StreamReader = new StreamReader(_NetworkStream);
                _StreamWriter = new StreamWriter(_NetworkStream)
                {
                    AutoFlush = true
                };

                uint dummy = 0;
                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
                BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
                _TcpClient.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
                _ThreadReceiveData = new Thread(ReceiveData)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                };
                _ThreadReceiveData.Start();

                return true;
            }
            catch (SocketException ex)
            {
                // Log or handle socket-specific exceptions
                Debug.WriteLine($"SocketException: {ex.Message}");
                Disconnect();
                return false;
            }
            catch (NullReferenceException ex)
            {
                // Handle null reference exceptions specifically
                Debug.WriteLine($"NullReferenceException: {ex.Message}");
                Disconnect();
                return false;
            }
            catch (Exception)
            {
                Disconnect();
                return false;
            }
        }

        public bool Connect2()
        {
            try
            {
                _TcpClient2 = new TcpClient();
                var task = _TcpClient2.ConnectAsync(_ServerIP, _Port2);
                task.Wait(_TimeOutOfConnection);
                if (!task.IsCompleted)
                {
                    Disconnect();
                    return false;
                }
                _TcpClient2.SendTimeout = _SendTimeout;
                _NetworkStream2 = _TcpClient2.GetStream();
                _StreamReader2 = new StreamReader(_NetworkStream2);
                _StreamWriter2 = new StreamWriter(_NetworkStream2)
                {
                    AutoFlush = true
                };

                uint dummy = 0;
                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
                BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
                _TcpClient2.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
                _ThreadReceiveData2 = new Thread(ReceiveData2)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                };
                _ThreadReceiveData2.Start();

                return true;
            }
            catch (SocketException ex)
            {
                // Log or handle socket-specific exceptions
                Debug.WriteLine($"SocketException: {ex.Message}");
                Disconnect();
                return false;
            }
            catch (NullReferenceException ex)
            {
                // Handle null reference exceptions specifically
                Debug.WriteLine($"NullReferenceException: {ex.Message}");
                Disconnect();
                return false;
            }
            catch (Exception)
            {
                Disconnect();
                return false;
            }
        }

        private void KillThreadReceiveData()
        {
            if (_ThreadReceiveData != null && _ThreadReceiveData.IsAlive)
            {
                _ThreadReceiveData.Abort();
                _ThreadReceiveData = null;
            }
        }

        public bool Disconnect()
        {
            try
            {
                KillThreadReceiveData();

                if (_StreamReader != null)
                {
                    _StreamReader.Close();
                    _StreamReader = null;
                }

                if (_StreamWriter != null)
                {
                    _StreamWriter.Close();
                    _StreamWriter = null;
                }

                if (_NetworkStream != null)
                {
                    _NetworkStream.Close();
                    _NetworkStream = null;
                }

                Disconnect2();

                _TcpClient?.Dispose();
                _TcpClient = null;
            }
            catch (Exception)
            { }
            return true;
        }

        public bool Disconnect2()
        {
            try
            {
                if (_StreamReader2 != null)
                {
                    _StreamReader2.Close();
                    _StreamReader2 = null;
                }

                if (_StreamWriter2 != null)
                {
                    _StreamWriter2.Close();
                    _StreamWriter2 = null;
                }

                if (_NetworkStream2 != null)
                {
                    _NetworkStream2.Close();
                    _NetworkStream2 = null;
                }

                _TcpClient2?.Dispose();
                _TcpClient2 = null;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }



        public bool IsConnected()
        {
            try
            {
                if (_TcpClient?.Client == null)
                    return false;

                // Check if the socket is connected and readable without blocking
                bool isConnected = _TcpClient.Client.Connected &&
                                  !_TcpClient.Client.Poll(0, SelectMode.SelectRead) &&
                                  _TcpClient.Client.Available == 0;

                return isConnected;
            }
            catch (SocketException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
        //public bool IsConnected()
        //{
        //    try
        //    {
        //        return _TcpClient?.Client?.Connected ?? false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        public bool IsConnected2()
        {
            try
            {
                if (_TcpClient2?.Client == null)
                    return false;

                // Check if the socket is connected and readable without blocking
                bool isConnected = _TcpClient2.Client.Connected &&
                                  !_TcpClient2.Client.Poll(0, SelectMode.SelectRead) &&
                                  _TcpClient2.Client.Available == 0;

                return isConnected;
            }
            catch (SocketException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
        private void ReceiveData2()
        {
            while (true)
            {
                try
                {
                    var currentLine = new StringBuilder();
                    int code;
                    char charRead;
                    while ((code = _StreamReader2.Read()) >= 0)
                    {
                        charRead = (char)code;
                        if (charRead == '\r' || charRead == '\n' || charRead == _EndPackage)
                        {
                            break;
                        }
                        if (charRead != _StartPackage && charRead != _EndPackage)
                        {
                            currentLine.Append(charRead);
                        }
                    }
                    // Receive PLC data
                    string dataRead = currentLine.ToString();
                    RaiseOnPOD2ReceiveMessageEvent(dataRead);
                    //RaiseOnPODReceiveDataEventEvent(new PODDataModel { IP = _ServerIP, Port = _Port, RoleOfPrinter = _RoleOfPrinter, Text = dataRead });
                    if (dataRead != null && dataRead == "")
                    {
                        OnDisconnect();
                    }
                }
                catch (Exception) { }
                //  Thread.Sleep(1);
            }

        }

        private void ReceiveData()
        {
            if (_IsVersion)
            {
                //Support software R20 1.0.6.G
                Byte[] bytes;
                int counter;
                while (true)
                {
                    try
                    {
                        if (_TcpClient != null && _TcpClient.ReceiveBufferSize > 0)
                        {
                            bytes = new byte[_TcpClient.ReceiveBufferSize];
                            _NetworkStream.Read(bytes, 0, bytes.Length);
                            counter = 0;
                            for (int index = 0; index < bytes.Length; index++)
                            {
                                if (bytes[index] > 0x00)
                                {
                                    counter++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            string dataRead = Encoding.ASCII.GetString(bytes, 0, counter);
                            RaiseOnPODReceiveMessageEvent(dataRead);
                            RaiseOnPODReceiveDataEventEvent(new PODDataModel
                            {
                                IP = _ServerIP,
                                Port = _Port,
                                RoleOfPrinter = _RoleOfPrinter,
                                Text = dataRead
                            });

                            if (counter == 0)
                            {
                                OnDisconnect();
                            }
                        }
                        else
                        {
                            Thread.Sleep(5);
                        }
                    }
                    catch (Exception) { }
                    //   Thread.Sleep(1);
                }
            }
            else
            {
                while (true)
                {
                    try
                    {
                        var currentLine = new StringBuilder();
                        int code;
                        char charRead;
                        while ((code = _StreamReader.Read()) >= 0)
                        {
                            charRead = (char)code;
                            if (charRead == '\r' || charRead == '\n' || charRead == _EndPackage)
                            {
                                break;
                            }
                            if (charRead != _StartPackage && charRead != _EndPackage)
                            {
                                currentLine.Append(charRead);
                            }
                        }
                        // Receive PLC data
                        string dataRead = currentLine.ToString();
                        RaiseOnPODReceiveMessageEvent(dataRead);
                        if (dataRead == "PLC001")
                        {
                            int startCommandIndex = FrmMain.startIndex;
                            string formattedIndex = startCommandIndex.ToString("D7"); // Formats as a 7-digit number

                            if (Shared.Settings.CameraList.FirstOrDefault().IsIndexCommandEnable)
                            {
                                Shared.SensorController.Send("0" + formattedIndex);
                            }

                            //Shared.SendErrorOutputToSensorController(32);
                        }
                        RaiseOnPODReceiveDataEventEvent(new PODDataModel { IP = _ServerIP, Port = _Port, RoleOfPrinter = _RoleOfPrinter, Text = dataRead });
                        if (dataRead != null && dataRead == "")
                        {
                            OnDisconnect();
                        }
                    }
                    catch (Exception) { }
                    //  Thread.Sleep(1);
                }
            }
        }

        public bool Send(string message, int sendTimeout = 0)
        {
            try
            {
                _StreamWriter?.Write((char)_StartPackage + message + (char)_EndPackage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Send2(string message, int sendTimeout = 0)
        {
            try
            {
                _StreamWriter2?.Write((char)_StartPackage + message + (char)_EndPackage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void OnDisconnect()
        {
            await Task.Run(() =>
            {
                Disconnect();
            });
        }

        public void RaiseOnPODReceiveMessageEvent(object data)
        {
            OnPODReceiveMessageEvent?.Invoke(data, EventArgs.Empty);
        }

        public void RaiseOnPOD2ReceiveMessageEvent(object data)
        {
            OnPOD2ReceiveMessageEvent?.Invoke(data, EventArgs.Empty);
        }

        public void RaiseOnPODReceiveDataEventEvent(PODDataModel data)
        {
            OnPODReceiveDataEvent?.Invoke(data, EventArgs.Empty);
        }
    }
}