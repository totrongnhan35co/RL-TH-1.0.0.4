using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Utils;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace BarcodeVerificationSystem.Controller
{
    public class SerialDeviceController
    {
        private static SerialPort serialPort;
        private static string _comName = "COM7";
        private static int _bitPerSecond = 9600;
        private static Parity _parity = Parity.None;
        private static int _dataBits = 8;
        private static StopBits _stopBits = StopBits.One;

        public SerialDeviceController(string ComName, int BitPerSecond, int DataBits, Parity Parity, StopBits stopBits)
        {
            _comName = ComName;
            _bitPerSecond = BitPerSecond;
            _dataBits = DataBits;
            _parity = Parity;
            _stopBits = stopBits;
        }

        public bool ConnectSerialDevice()
        {
            serialPort = new SerialPort(_comName, _bitPerSecond, _parity, _dataBits, _stopBits);
            try
            {
                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                }
                else
                {
                    serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);    
                }
                return true;
            }
            catch (Exception ex)
            {
                serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                Logger.LogError("SerialDeviceController,ConnectSerialDevice: " + ex.Message);
                Debug.WriteLine("An error occurred: " + ex.Message);
            }
            return false;
        }

        public void DisconnectSerialDevice()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                    serialPort.Close(); 
                    serialPort.Dispose(); 
                    Shared.SerialDevController = null;
                }
                catch (Exception ex)
                {
                    Logger.LogError("SerialDeviceController, DisconnectSerialDevice: " + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("Port is already closed or not initialized.");
            }
        }

        public bool IsSerialDevConnected()
        {
            try
            {
                if (serialPort == null) return false;
                if (!serialPort.IsOpen)
                {
                    try
                    {
                        serialPort.Open();
                        serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("SerialDeviceController,IsSerialDevConnected: " + ex.Message);
                        Debug.WriteLine("Reconnection failed: " + ex.Message);
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
          
        }

        private static StringBuilder _buffer = new StringBuilder();
        private static DateTime _lastDataTime = DateTime.Now;
        private static System.Timers.Timer _timer;

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var sp = (SerialPort)sender;
                var data = sp.ReadExisting();
                _buffer.Append(data);
                _lastDataTime = DateTime.Now;

                if (_timer == null)
                {
                    _timer = new System.Timers.Timer(50);
                    _timer.Elapsed += (s, ev) =>
                    {
                        if (_buffer.Length > 0 && (DateTime.Now - _lastDataTime).TotalMilliseconds > 100)
                        {
                            var text = _buffer.ToString().Trim();
                            _buffer.Clear();

                            var image = Properties.Resources.icon_NoImage;
                            string isSampled = Shared.IsSampled ? "True" : " ";
                            var detectModel = new DetectModel
                            {
                                Text = text,
                                Device = "Barcode Scanner",
                                Sampled = isSampled,
                                Image = image
                            };
                            Shared.RaiseOnSerialDeviceReadDataChangeEvent(detectModel);
                        }
                    };
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("SerialDeviceController,DataReceivedHandler: " + ex.Message);
            }
        }

        //private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        //{
        //    try
        //    {
        //        // thinh nowww
        //        var sp = (SerialPort)sender;
        //        var data = sp.ReadExisting();
        //        var text = data.Trim();
        //        var image = Properties.Resources.icon_NoImage;
        //        string isSampled = Shared.IsSampled ? "True" : " ";
        //        var detectModel = new DetectModel
        //        {
        //            Text = text,
        //            Device = "Barcode Scanner",
        //            Sampled = isSampled,
        //            Image = image
        //        };
        //        Shared.RaiseOnSerialDeviceReadDataChangeEvent(detectModel); // Send data to UI
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError("SerialDeviceController,DataReceivedHandler: " + ex.Message);
        //    }
        //}

    }
}
