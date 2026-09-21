
//using BarcodeVerificationSystem.Model;
//using System;
//using System.Collections.Concurrent;
//using System.Diagnostics;
//using System.Drawing;
//using System.IO;
//using System.Net;
//using System.Net.Sockets;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BarcodeVerificationSystem.Controller.Camera.Keyence
//{
//    /// <summary>
//    /// Keyence CV-X TCP client — pattern giống Cognex/Hikrobot:
//    ///
//    /// CHANNEL 1 (TEXT)  — port = `port` (mặc định nhập trong Settings):
//    ///     "&lt;QR&gt;,&lt;OCR_STATUS&gt;\r\n"
//    ///         • QR rỗng cũng OK (no-read).
//    ///         • OCR_STATUS:
//    ///              "OK" / "1" / "PASS" / "TRUE"  → CompareResult = Valid
//    ///              giá trị khác / rỗng           → CompareResult = Invalided (ép NG cứng)
//    ///         • Frame chỉ có 1 cột (không có dấu phẩy) cũng được — coi OCR pass mặc định.
//    ///
//    /// CHANNEL 2 (IMAGE) — port = `imagePort`, mặc định = port + 1, có thể đổi qua ConfigureImageChannel().
//    ///     Protocol: [length:4 byte big-endian][image bytes JPG/BMP/PNG]
//    ///         • Nếu CV-X không cấu hình kênh image → connect fail và log warning,
//    ///           camera vẫn chạy text-only. Pipeline fallback Bitmap(100,100).
//    ///         • Nếu cần đổi protocol (vd Little-Endian, có magic header) → đổi
//    ///           hằng số IMAGE_PROTOCOL_* và sửa ImageListenLoop.
//    ///
//    /// PAIRING:
//    ///     Mỗi frame text dispatch sẽ tryDequeue 1 ảnh trong _imageQueue (FIFO).
//    ///     CV-X cấu hình "ghi ảnh xong → gửi text" để FIFO khớp 1-1.
//    ///
//    /// DOWNSTREAM (đã có sẵn trong frmMainTHTrueMilk):
//    ///     case CameraType.CV_X → enqueue → Compare → UpdateUICheckedResult:
//    ///         + UpdateImageAsync → pictureBoxPreview.Image = ảnh
//    ///         + Nếu Invalid → _QueueBufferBackupImage → NewExportImageToFile
//    ///                        → THErrorImageFolder\&lt;jobName&gt;\...bmp
//    ///         + SendErrorImageAsync (base64) → R-Link Master
//    ///         + FireRLinkCameraError (CSV + DB)
//    /// </summary>
//    public class VscCamera
//    {
//        private Socket _socket;           // 1 socket duy nhất
//        private NetworkStream _stream;
//        private readonly byte[] _textBuf = new byte[4096];
//        private readonly StringBuilder _rxBuffer = new StringBuilder();
//        private CancellationTokenSource _cts;

//        public string ip;
//        public int port;
//        private int timeout;

//        public VscCamera(string ipAddr, int portNum, int timeoutMs)
//        {
//            ip = ipAddr;
//            port = portNum;
//            timeout = timeoutMs;
//        }

//        // ══════════════════════════════════════════
//        // CONNECT
//        // ══════════════════════════════════════════
//        public bool Connect()
//        {
//            try
//            {
//                // Dọn socket cũ nếu có (không cancel _cts ở đây để tránh race)
//                CleanupSocket();

//                _socket = new Socket(AddressFamily.InterNetwork,
//                                     SocketType.Stream,
//                                     ProtocolType.Tcp);
//                // Chỉ set SendTimeout, KHÔNG set ReceiveTimeout để tránh ảnh hưởng Poll loop
//                _socket.SendTimeout = timeout;
//                _socket.NoDelay = true;

//                SetKeepAlive(_socket, keepAliveTime: 10_000,
//                                      keepAliveInterval: 2_000,
//                                      probeCount: 3);

//                // Dùng ConnectTimeout riêng thay vì dựa vào ReceiveTimeout
//                var result = _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), null, null);
//                bool connected = result.AsyncWaitHandle.WaitOne(timeout, true);
//                if (!connected || !_socket.Connected)
//                {
//                    Console.WriteLine($"[VSC] ❌ Connect timeout sau {timeout}ms @ {ip}:{port}");
//                    CleanupSocket();
//                    return false;
//                }
//                _socket.EndConnect(result);

//                _stream = new NetworkStream(_socket, ownsSocket: false);
//                _rxBuffer.Clear();
//                Console.WriteLine($"[VSC] ✅ Connected @ {ip}:{port}");
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[VSC] ❌ Connect failed: {ex.Message}");
//                CleanupSocket();
//                return false;
//            }
//        }

//        // ══════════════════════════════════════════
//        // START LISTENING
//        // ══════════════════════════════════════════
//        public bool StartListening()
//        {
//            if (_socket == null || !_socket.Connected)
//            {
//                Console.WriteLine("[VSC] StartListening fail: chưa connect");
//                return false;
//            }

//            _cts = new CancellationTokenSource();
//            Task.Run(() => TextListenLoop(_cts.Token), _cts.Token);
//            return true;
//        }

//        // ══════════════════════════════════════════
//        // TEXT LOOP — 1 socket, không duplicate
//        // ══════════════════════════════════════════
//        private void TextListenLoop(CancellationToken token)
//        {
//            Console.WriteLine($"[VSC] Listen loop started @ {DateTime.Now}");
//            const int POLL_US = 50_000; // 50ms

//            while (!token.IsCancellationRequested)
//            {
//                if (_socket == null || !_socket.Connected)
//                {
//                    Console.WriteLine("[VSC] Socket mất kết nối, thoát loop");
//                    break;
//                }

//                try
//                {
//                    // Poll để không block vĩnh viễn
//                    if (!_socket.Poll(POLL_US, SelectMode.SelectRead))
//                    {
//                        Thread.Sleep(1);
//                        continue;
//                    }

//                    int n = _socket.Receive(_textBuf);
//                    if (n <= 0)
//                    {
//                        Console.WriteLine("[VSC] Server đóng kết nối (n=0)");
//                        break;
//                    }

//                    string chunk = Encoding.ASCII.GetString(_textBuf, 0, n);
//                    Debug.WriteLine($"[VSC] RX raw: {chunk.Replace("\r", "\\r").Replace("\n", "\\n")}");

//                    _rxBuffer.Append(chunk);
//                    ProcessBufferedFrames();
//                }
//                catch (SocketException ex)
//                    when (ex.SocketErrorCode == SocketError.TimedOut)
//                {
//                    // Timeout receive bình thường, tiếp tục
//                    continue;
//                }
//                catch (SocketException ex)
//                {
//                    Console.WriteLine($"[VSC] SocketException: {ex.SocketErrorCode} - {ex.Message}");
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"[VSC] Exception: {ex.Message}");
//                    break;
//                }
//            }

//            Console.WriteLine($"[VSC] Listen loop ended @ {DateTime.Now}");

//            // Auto-reconnect nếu không phải do Cancel chủ động
//            if (!token.IsCancellationRequested)
//            {
//                Console.WriteLine("[VSC] Mất kết nối ngoài ý muốn → reconnect sau 2s...");
//                Thread.Sleep(2000);
//                Reconnect();
//            }
//        }

//        private void ProcessBufferedFrames()
//        {
//            string buf = _rxBuffer.ToString();
//            int start = 0;

//            for (int i = 0; i < buf.Length; i++)
//            {
//                if (buf[i] != '\r' && buf[i] != '\n') continue;
//                if (i > start)
//                {
//                    string frame = buf.Substring(start, i - start).Trim();
//                    if (!string.IsNullOrEmpty(frame))
//                        DispatchFrame(frame);
//                }
//                start = i + 1;
//            }

//            _rxBuffer.Clear();
//            if (start < buf.Length)
//                _rxBuffer.Append(buf, start, buf.Length - start);
//        }

//        private void DispatchFrame(string frame)
//        {
//            string qr;
//            bool ocrPass = true;

//            int comma = frame.IndexOf(',');
//            if (comma >= 0)
//            {
//                qr = frame.Substring(0, comma).Trim();
//                string ocrStatus = frame.Substring(comma + 1).Trim();
//                ocrPass = ocrStatus.Equals("OK", StringComparison.OrdinalIgnoreCase)
//                       || ocrStatus == "1"
//                       || ocrStatus.Equals("PASS", StringComparison.OrdinalIgnoreCase)
//                       || ocrStatus.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
//            }
//            else
//            {
//                qr = frame.Trim();
//            }

//            var detect = new DetectModel
//            {
//                Text = qr,
//                Image = null, // Xử lý image sau khi text ổn định
//                CompareResult = ocrPass
//                    ? ComparisonResult.Valid
//                    : ComparisonResult.Invalided,
//            };
//            detect.ExtraFields["OCR_STATUS"] = ocrPass ? "OK" : "NG";

//            Console.WriteLine($"[VSC] Frame: QR='{qr}' OCR={(ocrPass ? "OK" : "NG")}");
//            Shared.RaiseOnCameraReadDataChangeEvent(detect);
//        }

//        // ══════════════════════════════════════════
//        // SEND COMMAND (trigger, đổi program, v.v.)
//        // ══════════════════════════════════════════
//        public bool SendCommand(string command)
//        {
//            try
//            {
//                if (_socket == null || !_socket.Connected)
//                    throw new InvalidOperationException("Chưa kết nối");

//                byte[] data = Encoding.ASCII.GetBytes(command + "\r\n");
//                _socket.Send(data);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[VSC] SendCommand fail: {ex.Message}");
//                return false;
//            }
//        }

//        public bool Trigger() => SendCommand("T1");
//        public bool SetRunMode() => SendCommand("RM");
//        public bool SetSetupMode() => SendCommand("SM");
//        public bool ChangeProgram(int no) => SendCommand($"PN{no:D4}");

//        // ══════════════════════════════════════════
//        // RECONNECT / DISCONNECT
//        // ══════════════════════════════════════════
//        private void Reconnect()
//        {
//            CleanupSocket();
//            if (Connect()) StartListening();
//            else Console.WriteLine("[VSC] ❌ Reconnect thất bại");
//        }

//        public void Disconnect()
//        {
//            try { _cts?.Cancel(); } catch { }
//            Thread.Sleep(100); // Cho loop thoát
//            CleanupSocket();
//        }

//        private void CleanupSocket()
//        {
//            try
//            {
//                _stream?.Dispose();
//                if (_socket?.Connected == true)
//                    _socket.Shutdown(SocketShutdown.Both);
//                _socket?.Dispose();
//            }
//            catch { }
//            _stream = null;
//            _socket = null;
//            _rxBuffer.Clear();
//        }

//        public bool IsConnected() => _socket?.Connected == true;

//        // ══════════════════════════════════════════
//        // KEEP-ALIVE
//        // ══════════════════════════════════════════
//        private static void SetKeepAlive(Socket socket,
//            uint keepAliveTime, uint keepAliveInterval, int probeCount)
//        {
//            try
//            {
//                uint size = (uint)Marshal.SizeOf(typeof(uint));
//                byte[] inOpt = new byte[size * 3];
//                BitConverter.GetBytes(1u).CopyTo(inOpt, 0);
//                BitConverter.GetBytes(keepAliveTime).CopyTo(inOpt, (int)size);
//                BitConverter.GetBytes(keepAliveInterval).CopyTo(inOpt, (int)(size * 2));
//                socket.IOControl(IOControlCode.KeepAliveValues, inOpt, new byte[inOpt.Length]);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[VSC] KeepAlive fail: {ex.Message}");
//            }
//        }

//        // Test helper
//        public void SimulateFrame(string qr, bool ocrPass)
//            => DispatchFrame($"{qr},{(ocrPass ? "OK" : "NG")}");
//        public void StopListening()
//        {
//            try { _cts?.Cancel(); } catch { }
//            Console.WriteLine("[VSC] StopListening called");
//        }
//    }

//    public enum VSCResponseCode
//    {
//        OK,
//        Timeout,
//        InvalidData,
//        NetworkError,
//        UnknownError
//    }
//}