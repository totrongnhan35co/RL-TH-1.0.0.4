using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Controller.Camera.Keyence;

namespace BarcodeVerificationSystem.Controller.Camera.Keyence
{
    public class CvxCamera
    {
        private Socket client; // For commands (e.g., Trigger)
        private Socket listener; // For streaming data (StartListening)
        private byte[] bytes = new byte[1024];
        private char[] separator = { ',', '\r' };
        private NetworkStream commandStream; // For buffered command I/O
        public string ip; // Store for reconnect
        public int port;
        private int timeout;
        public CvxCamera(string IpAddr, int PortNum, int Timeout)
        {
            ip = IpAddr;
            port = PortNum;
            timeout = Timeout;
        }

        public bool Connect()
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);

                // Command socket
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.ReceiveTimeout = timeout;
                client.Connect(remoteEP);
                commandStream = new NetworkStream(client, ownsSocket: false);

                // Listener socket (use same port; for DataChannel, change to 50000 if needed)
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.ReceiveTimeout = timeout;
                listener.Connect(remoteEP);

                // Enable TCP keep-alives
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                SetKeepAlive(client, 30000, 5000, 5);
                SetKeepAlive(listener, 30000, 5000, 5);

                Console.WriteLine("Connected to {0}:{1} at {2}", ip, port, DateTime.Now);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection failed: " + ex.Message);
                client?.Dispose();
                listener?.Dispose();
                commandStream?.Dispose();
                return false;
            }
        }
        private void SetKeepAlive(Socket socket, uint keepAliveTime, uint keepAliveInterval, int keepAliveProbeCount)
        {
            try
            {
                uint size = (uint)Marshal.SizeOf(typeof(uint));
                byte[] inOptionValues = new byte[size * 3];
                byte[] outOptionValues = new byte[inOptionValues.Length];

                BitConverter.GetBytes(1u).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes(keepAliveTime).CopyTo(inOptionValues, (int)size);
                BitConverter.GetBytes(keepAliveInterval).CopyTo(inOptionValues, (int)(size * 2));

                socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, outOptionValues);
                Console.WriteLine($"Keep-alive set: Idle={keepAliveTime}ms, Interval={keepAliveInterval}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set keep-alive: {ex.Message}");
            }
        }

        private void Reconnect()
        {
            Disconnect();
            if (!Connect())
            {
                Console.WriteLine("Reconnect failed.");
            }
            else
            {
                Console.WriteLine("Reconnected successfully.");
            }
        }

        public bool IsConnected() => client?.Connected == true && listener?.Connected == true;

        public bool StartListening()//Action<string> onDataReceived
        {
            if (listener == null || !listener.Connected)
            {
                Debug.WriteLine("StartListening failed: Socket is null or not connected at {0}", DateTime.Now);
                return false;
            }

            Debug.WriteLine("Starting listening at {0}", DateTime.Now);
            try
            {
                Task.Run(() =>
                {
                    Debug.WriteLine("Listening task started at {0}", DateTime.Now);
                    int pollTimeout = 50 * 1000; // 50 ms (microseconds) - less aggressive
                    bool dataReceivedRecently = false;

                    while (true)
                    {
                        if (listener != null && !listener.Connected)
                        {
                            Debug.WriteLine("Socket disconnected at {0}, attempting to reconnect...", DateTime.Now);
                            break;
                        }

                        try
                        {
                            if (listener != null && listener.Poll(pollTimeout, SelectMode.SelectRead))
                            {
                                int bytesRec = listener.Receive(bytes);
                                if (bytesRec > 0)
                                {
                                    string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                                    Debug.WriteLine("Received data: {0} at {1}", data, DateTime.Now);

                                    char[] separator = { ',', '\r' };
                                    string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var part in parts)
                                    {
                                        string trimmedPart = part.Trim();
                                        if (!string.IsNullOrEmpty(trimmedPart))
                                        {
                                            var detectModel = new DetectModel
                                            {
                                                Text = trimmedPart,
                                            };

                                            Shared.RaiseOnCameraReadDataChangeEvent(detectModel);

                                            //Shared.CamController.RaiseOnCamReceiveMessageEvent(data);
                                            dataReceivedRecently = true;
                                            pollTimeout = 50 * 1000; // Reset to 50 ms on activity
                                            //listBoxData.Items.Add($"[{DateTime.Now:HH:mm:ss.fff}] {trimmedPart}");
                                        }
                                    }


                                }
                                else
                                {
                                    Debug.WriteLine("No data received, connection may be closed at {0}", DateTime.Now);
                                    break;
                                }
                            }
                            else
                            {
                                if (dataReceivedRecently)
                                {
                                    Task.Delay(1).Wait(); // 1 ms delay
                                }
                                else
                                {
                                    pollTimeout = 100 * 1000; // 100 ms idle
                                    Task.Delay(1).Wait();
                                }
                                dataReceivedRecently = false;
                            }
                        }
                        catch (SocketException ex)
                        {
                            Debug.WriteLine("SocketException in listening: {0} at {1}", ex.Message, DateTime.Now);
                            if (ex.SocketErrorCode == SocketError.ConnectionAborted)
                            {
                                Debug.WriteLine("Connection aborted, exiting loop at {0}", DateTime.Now);
                                break;
                            }
                            Task.Delay(1).Wait();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Exception in listening: {0} at {1}", ex.Message, DateTime.Now);
                            break;
                        }
                    }
                    Debug.WriteLine("Listening task ended at {0}", DateTime.Now);
                });
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StartListening error: {0} at {1}", ex.Message, DateTime.Now);
                return false;
            }
        }

        public void StopListening()
        {
            if (listener != null)
            {
                Debug.WriteLine("Stopping listening at {0}", DateTime.Now);
                if (listener.Connected)
                {
                    listener.Shutdown(SocketShutdown.Both);
                }
                listener.Close();
                listener.Dispose();
            }
        }

        public void Disconnect()
        {
            if (client != null)
            {
                if (client.Connected) client.Shutdown(SocketShutdown.Both);
                client.Close();
                client.Dispose();
            }
            if (listener != null)
            {
                if (listener.Connected) listener.Shutdown(SocketShutdown.Both);
                listener.Close();
                listener.Dispose();
            }
            commandStream?.Dispose();
            client = null;
            listener = null;
            commandStream = null;
        }

        public bool SendCommand(string command)
        {
            try
            {
                if (client == null || !client.Connected || commandStream == null)
                    return false;

                byte[] byteData = Encoding.ASCII.GetBytes(command + "\r");
                commandStream.Write(byteData, 0, byteData.Length);
                commandStream.Flush();
                Console.WriteLine($"[CVX] Sent: {command} at {DateTime.Now}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CVX] SendCommand error: {ex.Message}");
                return false;
            }
        }

        // Placeholder methods unchanged...
    }
    public enum CVXResponseCode
    {
        OK,
        Timeout,
        InvalidData,
        NetworkError,
        UnknownError
    }
}



//public CVXResponseCode Trigger(string command, out string[] resultData) // Param for "TA" or "SE8"
//{
//    resultData = null;
//    string res = sendMessage(command + "\r");
//    if (string.IsNullOrEmpty(res))
//    {
//        return CVXResponseCode.Timeout;
//    }

//    string[] resSplit = res.Split(separator, StringSplitOptions.RemoveEmptyEntries);
//    if (resSplit.Length > 0 && (string.Compare(resSplit[0], command) == 0 || string.Compare(resSplit[0], "OK") == 0)) // Handle "OK" for SE8
//    {
//        resultData = resSplit;
//        return CVXResponseCode.OK;
//    }

//    return resSplit.Length > 2 ? (CVXResponseCode)Convert.ToInt32(resSplit[2]) : CVXResponseCode.InvalidData;
//}

//private string sendMessage(string req, int maxRetries = 3)
//{
//    for (int retry = 0; retry < maxRetries; retry++)
//    {
//        try
//        {
//            byte[] byteData = Encoding.UTF8.GetBytes(req);
//            int bytesSent = 0;
//            do
//            {
//                int toSend = Math.Min(1024, byteData.Length - bytesSent);
//                commandStream.Write(byteData, bytesSent, toSend);
//                bytesSent += toSend;
//            } while (bytesSent < byteData.Length);
//            commandStream.Flush();

//            // Read response with partial handling
//            byte[] recvBuffer = new byte[1024];
//            int totalBytesRead = 0;
//            int bytesRec;
//            do
//            {
//                bytesRec = commandStream.Read(recvBuffer, totalBytesRead, recvBuffer.Length - totalBytesRead);
//                if (bytesRec == 0) break; // End of stream
//                totalBytesRead += bytesRec;
//            } while (totalBytesRead < recvBuffer.Length && bytesRec > 0);

//            if (totalBytesRead > 0)
//            {
//                string result = Encoding.ASCII.GetString(recvBuffer, 0, totalBytesRead).TrimEnd(separator); // Trim terminators
//                Console.WriteLine("Sent: {0}, Received: {1} at {2}", req, result, DateTime.Now);
//                return result;
//            }
//            return string.Empty;
//        }
//        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionAborted)
//        {
//            Console.WriteLine($"sendMessage abort (retry {retry + 1}/{maxRetries}): {ex.Message}. Check AV/firewall/port.");
//            if (retry == maxRetries - 1)
//            {
//                Reconnect(); // Auto-reconnect on final failure
//                return string.Empty;
//            }
//            Task.Delay(1000 * (retry + 1)).Wait(); // Backoff
//        }
//        catch (SocketException ex)
//        {
//            Console.WriteLine($"sendMessage SocketException: {ex.Message}, ErrorCode: {ex.SocketErrorCode}");
//            return string.Empty;
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("sendMessage error: " + ex.Message);
//            return string.Empty;
//        }
//    }
//    return string.Empty;
//}
