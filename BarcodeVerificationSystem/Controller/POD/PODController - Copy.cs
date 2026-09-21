//using BarcodeVerificationSystem.Controller.ZebraPrinter;
//using BarcodeVerificationSystem.Model;
//using BarcodeVerificationSystem.View;
//using FluentFTP.Helpers;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Management;
//using System.Net.Sockets;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BarcodeVerificationSystem.Controller
//{

//    public class PODController
//    {
//        private string _ServerIP = "127.0.0.1";
//        private int _Port = 1997;
//        private int _Port2 = 1998;
//        private RoleOfStation _RoleOfPrinter = RoleOfStation.ForProduct;
//        private readonly int _TimeOutOfConnection = 5000;
//        private readonly int _SendTimeout = 1000;
//        private readonly byte _StartPackage = 0x02;
//        protected byte _EndPackage = 0x03;
//        private readonly bool _IsVersion = false;
//        private TcpClient _TcpClient;
//        private TcpClient _TcpClient2;
//        private NetworkStream _NetworkStream;
//        private StreamReader _StreamReader = null;
     
//        private StreamWriter _StreamWriter = null;
//        private Thread _ThreadReceiveData = null;
//        private Thread _ThreadReceiveData2 = null;
//        private NetworkStream _NetworkStream2;
//        private StreamReader _StreamReader2 = null;
//        private StreamWriter _StreamWriter2 = null;
//        public event EventHandler OnPODReceiveMessageEvent;
//        public event EventHandler OnPOD2ReceiveMessageEvent;
//        public event EventHandler OnPODReceiveDataEvent;
//        protected System.Collections.Concurrent.ConcurrentQueue<string> _MessageBuffer = new System.Collections.Concurrent.ConcurrentQueue<string>();//Linh.Tran_29062026
//        public string ServerIP
//        {
//            get { return _ServerIP; }
//            set { _ServerIP = value; Disconnect(); }
//        }
//        public int Port
//        {
//            get { return _Port; }
//            set { _Port = value; Disconnect(); }
//        }
//        public int Port2
//        {
//            get { return _Port2; }
//            set { _Port2 = value; Disconnect(); }
//        }

//        public RoleOfStation RoleOfPrinter
//        {
//            get { return _RoleOfPrinter; }
//            set { _RoleOfPrinter = value; }
//        }

//        public PODController(string serverIP, int port, int port2, int timeOutOfConnection, int sendTimeout)
//        {
//            _ServerIP = serverIP;
//            _Port = port;
//            _Port2 = port2;
//            _TimeOutOfConnection = timeOutOfConnection;
//            _SendTimeout = sendTimeout;
//        }
//        public PODController(string serverIP, int port, RoleOfStation roleOfPrinter, int timeOutOfConnection, int sendTimeout, bool isVersion)
//        {
//            _ServerIP = serverIP;
//            _Port = port;
//            _RoleOfPrinter = roleOfPrinter;
//            _TimeOutOfConnection = timeOutOfConnection;
//            _SendTimeout = sendTimeout;
//            _IsVersion = isVersion;
//            _IsVersion = false;
//        }

//        public bool Connect()
//        {
//            try
//            {
//                _TcpClient = new TcpClient();
//                var task = _TcpClient.ConnectAsync(_ServerIP, _Port);
//                task.Wait(_TimeOutOfConnection);
//                if (!task.IsCompleted)
//                {
//                    Disconnect();
//                    return false;
//                }
//                _TcpClient.SendTimeout = _SendTimeout;
//                _NetworkStream = _TcpClient.GetStream();
//                _StreamReader = new StreamReader(_NetworkStream);
//                _StreamWriter = new StreamWriter(_NetworkStream)
//                {
//                    AutoFlush = true
//                };

//                //Linh.Tran_29062026
//                //_MessageBuffer = new System.Collections.Concurrent.ConcurrentQueue<string>();
//                //Thread threadReceivedMessage = new Thread(ThreadExcReceivedMessage);
//                //threadReceivedMessage.IsBackground = true;
//                //threadReceivedMessage.Priority = ThreadPriority.AboveNormal;0
//                //threadReceivedMessage.Start();
//                //End Linh.Tran_29062026

//                uint dummy = 0;
//                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
//                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
//                BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
//                BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
//                _TcpClient.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
//                _ThreadReceiveData = new Thread(ReceiveData)
//                {
//                    IsBackground = true,
//                    Priority = ThreadPriority.Normal
//                };
//                _ThreadReceiveData.Start();

//                return true;
//            }
//            catch (SocketException ex)
//            {
//                // Log or handle socket-specific exceptions
//                Debug.WriteLine($"SocketException: {ex.Message}");
//                Disconnect();
//                return false;
//            }
//            catch (NullReferenceException ex)
//            {
//                // Handle null reference exceptions specifically
//                Debug.WriteLine($"NullReferenceException: {ex.Message}");
//                Disconnect();
//                return false;
//            }
//            catch (Exception)
//            {
//                Disconnect();
//                return false;
//            }
//        }

//        public bool Connect2()
//        {
//            try
//            {
//                _TcpClient2 = new TcpClient();
//                var task = _TcpClient2.ConnectAsync(_ServerIP, _Port2);
//                task.Wait(_TimeOutOfConnection);
//                if (!task.IsCompleted)
//                {
//                    Disconnect();
//                    return false;
//                }
//                _TcpClient2.SendTimeout = _SendTimeout;
//                _NetworkStream2 = _TcpClient2.GetStream();
//                _StreamReader2 = new StreamReader(_NetworkStream2);
//                _StreamWriter2 = new StreamWriter(_NetworkStream2)
//                {
//                    AutoFlush = true
//                };

//                uint dummy = 0;
//                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
//                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
//                BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
//                BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
//                _TcpClient2.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
//                _ThreadReceiveData2 = new Thread(ReceiveData2)
//                {
//                    IsBackground = true,
//                    Priority = ThreadPriority.Normal
//                };
//                _ThreadReceiveData2.Start();

//                return true;
//            }
//            catch (SocketException ex)
//            {
//                // Log or handle socket-specific exceptions
//                Debug.WriteLine($"SocketException: {ex.Message}");
//                Disconnect();
//                return false;
//            }
//            catch (NullReferenceException ex)
//            {
//                // Handle null reference exceptions specifically
//                Debug.WriteLine($"NullReferenceException: {ex.Message}");
//                Disconnect();
//                return false;
//            }
//            catch (Exception)
//            {
//                Disconnect();
//                return false;
//            }
//        }

//        private void KillThreadReceiveData()
//        {
//            if (_ThreadReceiveData != null && _ThreadReceiveData.IsAlive)
//            {
//                try { _NetworkStream?.Close(); } catch { }
//                try { _StreamReader?.Close(); } catch { }
//                _ThreadReceiveData = null;
//            }
//        }

//        public bool Disconnect()
//        {
//            try
//            {
//                KillThreadReceiveData();

//                if (_StreamReader != null)
//                {
//                    _StreamReader.Close();
//                    _StreamReader = null;
//                }

//                if (_StreamWriter != null)
//                {
//                    _StreamWriter.Close();
//                    _StreamWriter = null;
//                }

//                if (_NetworkStream != null)
//                {
//                    _NetworkStream.Close();
//                    _NetworkStream = null;
//                }

//                Disconnect2();

//                _TcpClient?.Dispose();
//                _TcpClient = null;
//            }
//            catch (Exception)
//            { }
//            return true;
//        }

//        public bool Disconnect2()
//        {
//            try
//            {
//                if (_StreamReader2 != null)
//                {
//                    _StreamReader2.Close();
//                    _StreamReader2 = null;
//                }

//                if (_StreamWriter2 != null)
//                {
//                    _StreamWriter2.Close();
//                    _StreamWriter2 = null;
//                }

//                if (_NetworkStream2 != null)
//                {
//                    _NetworkStream2.Close();
//                    _NetworkStream2 = null;
//                }

//                _TcpClient2?.Dispose();
//                _TcpClient2 = null;
//            }
//            catch (Exception)
//            {
//                return false;
//            }
//            return true;
//        }



//        public bool IsConnected()
//        {
//            try
//            {
//                if (_TcpClient?.Client == null)
//                    return false;

//                if (!_TcpClient.Client.Connected)
//                    return false;

//                // Poll = true + Available == 0 → remote đã đóng kết nối
//                if (_TcpClient.Client.Poll(0, SelectMode.SelectRead) && _TcpClient.Client.Available == 0)
//                    return false;

//                return true;
//            }
//            catch (SocketException)
//            {
//                return false;
//            }
//            catch (ObjectDisposedException)
//            {
//                return false;
//            }
//        }
//        public bool IsConnected2()
//        {
//            try
//            {
//                if (_TcpClient2?.Client == null)
//                    return false;

//                if (!_TcpClient2.Client.Connected)
//                    return false;

//                // Poll = true + Available == 0 → remote đã đóng kết nối
//                if (_TcpClient2.Client.Poll(0, SelectMode.SelectRead) && _TcpClient2.Client.Available == 0)
//                    return false;

//                return true;
//            }
//            catch (SocketException)
//            {
//                return false;
//            }
//            catch (ObjectDisposedException)
//            {
//                return false;
//            }
//        }
//        //private void ReceiveData2()
//        //{
//        //    while (true)
//        //    {
//        //        try
//        //        {
//        //            var currentLine = new StringBuilder();
//        //            int code;
//        //            char charRead;
//        //            while ((code = _StreamReader2.Read()) >= 0)
//        //            {
//        //                charRead = (char)code;
//        //                if (charRead == '\r' || charRead == '\n' || charRead == _EndPackage)
//        //                {
//        //                    break;
//        //                }
//        //                if (charRead != _StartPackage && charRead != _EndPackage)
//        //                {
//        //                    currentLine.Append(charRead);
//        //                }
//        //            }
//        //            // Receive PLC data
//        //            string dataRead = currentLine.ToString();
//        //            RaiseOnPOD2ReceiveMessageEvent(dataRead);
//        //            //RaiseOnPODReceiveDataEventEvent(new PODDataModel { IP = _ServerIP, Port = _Port, RoleOfPrinter = _RoleOfPrinter, Text = dataRead });
//        //            if (dataRead != null && dataRead == "")
//        //            {
//        //                OnDisconnect();
//        //            }
//        //        }
//        //        catch (Exception) { }
//        //        //  Thread.Sleep(1);
//        //    }

//        //}

//        private void ReceiveData2()
//        {
//            while (true)
//            {
//                try
//                {
//                    var currentLine = new StringBuilder();
//                    int code;
//                    char charRead;
//                    while ((code = _StreamReader2.Read()) >= 0)
//                    {
//                        charRead = (char)code;
//                        if (charRead == '\r' || charRead == '\n' || charRead == _EndPackage)
//                            break;
//                        if (charRead != _StartPackage && charRead != _EndPackage)
//                            currentLine.Append(charRead);
//                    }

//                    string dataRead = currentLine.ToString();

//                    if (code < 0 || dataRead == "")
//                    {
//                        Debug.WriteLine("[PODController] ReceiveData2: connection closed");
//                        OnDisconnect();
//                        break; // ← THOÁT
//                    }

//                    RaiseOnPOD2ReceiveMessageEvent(dataRead);
//                }
//                catch (IOException ioEx)
//                {
//                    Debug.WriteLine($"[PODController] IOException in ReceiveData2: {ioEx.Message}");
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    Debug.WriteLine($"[PODController] Exception in ReceiveData2: {ex.Message}");
//                    PrinterLogger.Log("", _ServerIP, "ERR", "RECV2_CRASH",
//                        $"type={ex.GetType().Name} msg={ex.Message} stack={ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}",
//                        "CRITICAL", "fault=rlink: Thread receive ReceiveData2() chết");
//                    break;
//                }
//            }
//        }
//        private void ReceiveData()
//        {
//            if (_IsVersion)
//            {
//                //Support software R20 1.0.6.G
//                Byte[] bytes;
//                int counter;
//                while (true)
//                {
//                    try
//                    {
//                        if (_TcpClient != null && _TcpClient.ReceiveBufferSize > 0)
//                        {
//                            bytes = new byte[_TcpClient.ReceiveBufferSize];
//                            _NetworkStream.Read(bytes, 0, bytes.Length);
//                            counter = 0;
//                            for (int index = 0; index < bytes.Length; index++)
//                            {
//                                if (bytes[index] > 0x00)
//                                {
//                                    counter++;
//                                }
//                                else
//                                {
//                                    break;
//                                }
//                            }
//                            string dataRead = Encoding.ASCII.GetString(bytes, 0, counter);
//                            string rawLog = dataRead.Replace("\x02", "<STX>").Replace("\x03", "<ETX>");
//                            Debug.WriteLine($"[POD←RAW] len={counter} | {dataRead?.Substring(0, Math.Min(120, dataRead?.Length ?? 0))}");
//                            RaiseOnPODReceiveMessageEvent(dataRead);
//                            RaiseOnPODReceiveDataEventEvent(new PODDataModel
//                            {
//                                IP = _ServerIP,
//                                Port = _Port,
//                                RoleOfPrinter = _RoleOfPrinter,
//                                Text = dataRead,
//                                RawText = rawLog
//                            });

//                            if (counter == 0)
//                            {
//                                OnDisconnect();
//                            }
//                        }
//                        else
//                        {
//                            Thread.Sleep(5);
//                        }
//                    }
//                    catch (Exception) { }
//                    //   Thread.Sleep(1);
//                }
//            }
//            //else
//            //{
//            //    while (true)
//            //    {
//            //        try
//            //        {
//            //            var currentLine = new StringBuilder();
//            //            int code;
//            //            char charRead;
//            //            while ((code = _StreamReader.Read()) >= 0)
//            //            {
//            //                charRead = (char)code;
//            //                if (charRead == '\r' || charRead == '\n' || charRead == _EndPackage)
//            //                {
//            //                    break;
//            //                }
//            //                if (charRead != _StartPackage && charRead != _EndPackage)
//            //                {
//            //                    currentLine.Append(charRead);
//            //                }
//            //            }
//            //            // Receive PLC data
//            //            string dataRead = currentLine.ToString();
//            //            RaiseOnPODReceiveMessageEvent(dataRead);
//            //            if (dataRead == "PLC001")
//            //            {
//            //                int startCommandIndex = FrmMain.startIndex;
//            //                string formattedIndex = startCommandIndex.ToString("D7"); // Formats as a 7-digit number

//            //                if (Shared.Settings.CameraList.FirstOrDefault().IsIndexCommandEnable)
//            //                {
//            //                    Shared.SensorController.Send("0" + formattedIndex);
//            //                }

//            //                //Shared.SendErrorOutputToSensorController(32);
//            //            }
//            //            RaiseOnPODReceiveDataEventEvent(new PODDataModel { IP = _ServerIP, Port = _Port, RoleOfPrinter = _RoleOfPrinter, Text = dataRead });
//            //            if (dataRead != null && dataRead == "")
//            //            {
//            //                OnDisconnect();
//            //            }
//            //        }
//            //        catch (Exception) { }
//            //        //  Thread.Sleep(1);
//            //    } 
//            //}

//            else
//            {
//                DateTime _lastAliveLog = DateTime.MinValue;
//                while (true)
//                {
//                    try
//                    {
//                        var rawLine = new StringBuilder();
//                        var currentLine = new StringBuilder();
//                        int code;
//                        char charRead;
//                        while ((code = _StreamReader.Read()) >= 0)
//                        {
//                            charRead = (char)code;
//                            rawLine.Append(charRead);
//                            if (charRead == '\r' || charRead == '\n' || charRead == _EndPackage)
//                                break;
//                            if (charRead != _StartPackage && charRead != _EndPackage)
//                                currentLine.Append(charRead);
//                        }

//                        string dataRead = currentLine.ToString();
//                        string rawLog = rawLine.ToString().Replace("\x02", "<STX>").Replace("\x03", "<ETX>");
//                        Debug.WriteLine($"[POD←RAW] len={dataRead?.Length} | {dataRead?.Substring(0, Math.Min(120, dataRead?.Length ?? 0))}");
//                        PrinterLogger.Log("", _ServerIP, "RECV", "DATA",
//                            $"cmd={dataRead?.Split(';')[0]} len={dataRead?.Length} raw={rawLog?.Substring(0, Math.Min(120, rawLog?.Length ?? 0))}",
//                            "OK", "Dữ liệu nhận từ máy in");
//                        if ((DateTime.Now - _lastAliveLog).TotalSeconds >= 60)
//                        {
//                            PrinterLogger.Log("", _ServerIP, "STATE", "THREAD_ALIVE",
//                                $"uptime since last recv", "OK", "Thread receive còn sống");
//                            _lastAliveLog = DateTime.Now;
//                        }
//                        // ── Nếu Read() trả về -1 → connection đóng ──
//                        if (code < 0 || dataRead == "")
//                        {
//                            Debug.WriteLine($"[PODController] Connection closed by remote (code={code})");
//                            PrinterLogger.Log("", _ServerIP, "STATE", "DISCONNECT",
//                            "code=-1 dataRead=empty", "DISCONNECT", "fault=network: Mất kết nối TCP - máy in đóng kết nối");
//                            OnDisconnect();
//                            break; // ← THOÁT outer loop, không loop vô hạn
//                        }

//                        RaiseOnPODReceiveMessageEvent(dataRead);
//                        if (dataRead == "PLC001")
//                        {
//                            int startCommandIndex = FrmMain.startIndex;
//                            string formattedIndex = startCommandIndex.ToString("D7");
//                            if (Shared.Settings.CameraList.FirstOrDefault().IsIndexCommandEnable)
//                                Shared.SensorController.Send("0" + formattedIndex);
//                        }
//                        RaiseOnPODReceiveDataEventEvent(new PODDataModel
//                        {
//                            IP = _ServerIP,
//                            Port = _Port,
//                            RoleOfPrinter = _RoleOfPrinter,
//                            Text = dataRead,
//                            RawText = rawLog
//                        });
//                    }
//                    catch (IOException ioEx)
//                    {
//                        // TCP bị reset / mất kết nối đột ngột
//                        Debug.WriteLine($"[PODController] IOException in ReceiveData: {ioEx.Message}");
//                        PrinterLogger.Log("", _ServerIP, "STATE", "DISCONNECT",
//                            "IOException - " + ioEx.Message, "DISCONNECT", "fault=network: TCP bị reset đột ngột (IOException trong receive thread)");
//                        OnDisconnect();
//                        break;
//                    }
//                    catch (Exception ex)
//                    {
//                        Debug.WriteLine($"[PODController] Exception in ReceiveData: {ex.Message}");
//                        PrinterLogger.Log("", _ServerIP, "ERR", "RECV_CRASH",
//                            $"type={ex.GetType().Name} msg={ex.Message} stack={ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}",
//                            "CRITICAL", "fault=rlink: Thread receive ReceiveData() chết");
//                        break;
//                    }
//                }
//            }
//        }


//        private void ReceiveData_New_29062026()
//        {
//            if (_IsVersion)
//            {
//                //Support software R20 1.0.6.G
//                Byte[] bytes;
//                int counter;
//                while (true)
//                {
//                    try
//                    {
//                        if (_TcpClient != null && _TcpClient.ReceiveBufferSize > 0)
//                        {
//                            bytes = new byte[_TcpClient.ReceiveBufferSize];
//                            _NetworkStream.Read(bytes, 0, bytes.Length);
//                            counter = 0;
//                            for (int index = 0; index < bytes.Length; index++)
//                            {
//                                if (bytes[index] > 0x00)
//                                {
//                                    counter++;
//                                }
//                                else
//                                {
//                                    break;
//                                }
//                            }
//                            string dataRead = Encoding.ASCII.GetString(bytes, 0, counter);
//                            Debug.WriteLine($"[POD←RAW] len={counter} | {dataRead?.Substring(0, Math.Min(120, dataRead?.Length ?? 0))}");
//                            RaiseOnPODReceiveMessageEvent(dataRead);
//                            RaiseOnPODReceiveDataEventEvent(new PODDataModel
//                            {
//                                IP = _ServerIP,
//                                Port = _Port,
//                                RoleOfPrinter = _RoleOfPrinter,
//                                Text = dataRead
//                            });

//                            if (counter == 0)
//                            {
//                                OnDisconnect();
//                            }
//                        }
//                        else
//                        {
//                            Thread.Sleep(5);
//                        }
//                    }
//                    catch (Exception) { }
//                    //   Thread.Sleep(1);
//                }
//            }
//            //else
//            //{
//            //    while (true)
//            //    {
//            //        try
//            //        {
//            //            var currentLine = new StringBuilder();
//            //            int code;
//            //            char charRead;
//            //            while ((code = _StreamReader.Read()) >= 0)
//            //            {
//            //                charRead = (char)code;
//            //                if (charRead == '\r' || charRead == '\n' || charRead == _EndPackage)
//            //                {
//            //                    break;
//            //                }
//            //                if (charRead != _StartPackage && charRead != _EndPackage)
//            //                {
//            //                    currentLine.Append(charRead);
//            //                }
//            //            }
//            //            // Receive PLC data
//            //            string dataRead = currentLine.ToString();
//            //            RaiseOnPODReceiveMessageEvent(dataRead);
//            //            if (dataRead == "PLC001")
//            //            {
//            //                int startCommandIndex = FrmMain.startIndex;
//            //                string formattedIndex = startCommandIndex.ToString("D7"); // Formats as a 7-digit number

//            //                if (Shared.Settings.CameraList.FirstOrDefault().IsIndexCommandEnable)
//            //                {
//            //                    Shared.SensorController.Send("0" + formattedIndex);
//            //                }

//            //                //Shared.SendErrorOutputToSensorController(32);
//            //            }
//            //            RaiseOnPODReceiveDataEventEvent(new PODDataModel { IP = _ServerIP, Port = _Port, RoleOfPrinter = _RoleOfPrinter, Text = dataRead });
//            //            if (dataRead != null && dataRead == "")
//            //            {
//            //                OnDisconnect();
//            //            }
//            //        }
//            //        catch (Exception) { }
//            //        //  Thread.Sleep(1);
//            //    } 
//            //}

//            else
//            {
//                while (true)
//                {
//                    try
//                    {
//                        //Read data package
//                        var currentLine = new StringBuilder();
//                        int code;
//                        char charRead;
//                        while ((code = _StreamReader.Read()) >= 0)
//                        {
//                            charRead = (char)code;
//                            if (charRead == '\r' || charRead == '\n' || charRead == _EndPackage)
//                                break;
//                            if (charRead != _StartPackage && charRead != _EndPackage)
//                                currentLine.Append(charRead);
//                        }
//                        //
//                        //Add to Buffer

//                        string dataRead = currentLine.ToString();
//                        _MessageBuffer.Enqueue(dataRead);
//                        //
//                        Debug.WriteLine($"[POD←RAW] len={dataRead?.Length} | {dataRead?.Substring(0, Math.Min(120, dataRead?.Length ?? 0))}");
//                        // ── Nếu Read() trả về -1 → connection đóng ──
//                        if (code < 0 || dataRead == "")
//                        {
//                            Debug.WriteLine($"[PODController] Connection closed by remote (code={code})");
//                            PrinterLogger.Log("", _ServerIP, "STATE", "DISCONNECT",
//                            "code=-1 dataRead=empty", "DISCONNECT", "fault=network: Mất kết nối TCP - máy in đóng kết nối");
//                            OnDisconnect();
//                            break; // ← THOÁT outer loop, không loop vô hạn
//                        }
//                        //
//                    }
//                    catch (IOException ioEx)
//                    {
//                        // TCP bị reset / mất kết nối đột ngột
//                        Debug.WriteLine($"[PODController] IOException in ReceiveData: {ioEx.Message}");
//                        PrinterLogger.Log("", _ServerIP, "STATE", "DISCONNECT",
//                            "IOException - " + ioEx.Message, "DISCONNECT", "fault=network: TCP bị reset đột ngột (IOException trong receive thread)");
//                        OnDisconnect();
//                        break;
//                    }
//                    catch (Exception ex)
//                    {
//                        Debug.WriteLine($"[PODController] Exception in ReceiveData: {ex.Message}");
//                        PrinterLogger.Log("", _ServerIP, "ERR", "RECV_NEW_CRASH",
//                            $"type={ex.GetType().Name} msg={ex.Message} stack={ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}",
//                            "CRITICAL", "fault=rlink: Thread receive ReceiveData_New_29062026() chết");
//                        break;
//                    }
//                }
//            }
//        }

//        //Linh.Tran_29062026
//        public bool isPODPacket(string data, byte _start, byte _end)
//        {
//            Char startData = Convert.ToChar(data[0]);
//            Char endData = Convert.ToChar(data[data.Length - 1]);
//            if (startData.Equals((Char)_start) && endData.Equals((Char)_end))//right format of data or command
//            {
//                return true;
//            }
//            return false;
//        }

//        private void ThreadExcReceivedMessage()

//        {
//            string oldData = "";
//            Debug.WriteLine("oldData: " + oldData);
//            while (true)

//            {
//                try
//                {
//                    if (_MessageBuffer.Count > 0)
//                    {
//                        string newData = "";
//                        _MessageBuffer.TryDequeue(out newData);
//                        string excData = oldData + newData;
//                        if (excData.Contains((Char)_StartPackage))
//                        {
//                            oldData = "";
//                            //
//                            if (excData.Length >= 0)//must have 0x02 and 0x03 to check, not wrong packet
//                            {
//                                string[] dataList = excData.Split((Char)_StartPackage);
//                                foreach (string strData in dataList)
//                                {
//                                    if (strData.Length > 0)
//                                    {
//                                        string subData = (Char)_StartPackage + strData;
//                                        if (isPODPacket(subData, _StartPackage, _EndPackage))//right format of data or command
//                                        {
//                                            string realData = subData.Substring(1, subData.Length - 2);
//                                            RaiseOnPODReceiveMessageEvent(realData);
//                                            Debug.WriteLine("RaiseOnPODReceiveMessageEvent: " + realData);
//                                            //
//                                            if (realData == "PLC001")
//                                            {
//                                                int startCommandIndex = FrmMain.startIndex;
//                                                string formattedIndex = startCommandIndex.ToString("D7");
//                                                if (Shared.Settings.CameraList.FirstOrDefault().IsIndexCommandEnable)
//                                                    Shared.SensorController.Send("0" + formattedIndex);
//                                            }
//                                            RaiseOnPODReceiveDataEventEvent(new PODDataModel
//                                            {
//                                                IP = _ServerIP,
//                                                Port = _Port,
//                                                RoleOfPrinter = _RoleOfPrinter,
//                                                Text = realData
//                                            });
//                                        }
//                                        else
//                                        {
//                                            oldData += subData;
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                        else
//                        {
//                            oldData += newData;
//                        }
//                        Debug.WriteLine("oldData: " + oldData);
//                    }
//                }
//                catch (IOException ioEx)
//                {
//                    // TCP bị reset / mất kết nối đột ngột
//                    Debug.WriteLine($"[PODController] IOException in ReceiveData: {ioEx.Message}");
//                    PrinterLogger.Log("", _ServerIP, "STATE", "DISCONNECT",
//                        "IOException - " + ioEx.Message, "DISCONNECT", "fault=network: TCP bị reset đột ngột (IOException trong receive thread)");
//                    OnDisconnect();
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    Debug.WriteLine($"[PODController] Exception in ReceiveData: {ex.Message}");
//                    PrinterLogger.Log("", _ServerIP, "ERR", "BUF_CRASH",
//                        $"type={ex.GetType().Name} msg={ex.Message} stack={ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}",
//                        "CRITICAL", "fault=rlink: Thread buffer ThreadExcReceivedMessage() chết");
//                    break;
//                }
//                //
//                Thread.Sleep(1);
//            }
//        }


//        public bool Send(string message, int sendTimeout = 0)
//        {
//            try
//            {
//                _StreamWriter?.Write((char)_StartPackage + message + (char)_EndPackage);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                string msgBrief = message == null ? "(null)" : message.Length > 80 ? message.Substring(0, 80) + "..." : message;
//                PrinterLogger.Log("", _ServerIP, "ERR", "SEND_FAIL",
//                    $"message={msgBrief} ex={ex.Message}",
//                    "SEND_ERR", "fault=rlink: Gửi dữ liệu thất bại qua TCP");
//                return false;
//            }
//        }

//        public bool Send2(string message, int sendTimeout = 0)
//        {
//            try
//            {
//                _StreamWriter2?.Write((char)_StartPackage + message + (char)_EndPackage);
//                return true;
//            }
//            catch (Exception)
//            {
//                return false;
//            }
//        }

//        private async void OnDisconnect()
//        {
//            await Task.Run(() =>
//            {
//                Disconnect();
//            });
//        }

//        public void RaiseOnPODReceiveMessageEvent(object data)
//        {
//            OnPODReceiveMessageEvent?.Invoke(data, EventArgs.Empty);
//        }

//        public void RaiseOnPOD2ReceiveMessageEvent(object data)
//        {
//            OnPOD2ReceiveMessageEvent?.Invoke(data, EventArgs.Empty);
//        }

//        public void RaiseOnPODReceiveDataEventEvent(PODDataModel data)
//        {
//            OnPODReceiveDataEvent?.Invoke(data, EventArgs.Empty);
//        }
//    }
//}
