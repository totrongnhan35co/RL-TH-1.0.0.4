//// File: BarcodeVerificationSystem\Controller\Camera\Keyence\VscCamera.cs
using BarcodeVerificationSystem.Model;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Controller.Camera.Keyence
{
    public enum ProtocolMode
    {
        Line,
        Framed
    }

    public class VscProgramInfo
    {
        public int ProgramNo { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string ModifyInfo { get; set; }

        public override string ToString()
            => ProgramNo.ToString("D4") + (string.IsNullOrWhiteSpace(Name) ? string.Empty : " " + Name);
    }

    public class VscCamera
    {
        private Socket _socket;           // 1 socket duy nhất
        private NetworkStream _stream;
        private readonly byte[] _textBuf = new byte[4096];
        private readonly StringBuilder _rxBuffer = new StringBuilder();
        private CancellationTokenSource _cts;

        // Framed helpers
        private readonly byte[] _recvBuffer = new byte[8192];
        private AsyncFtpClient _ftpClient;
        private int _ftpPort = 21;
        private readonly object _sync = new object();
        private readonly List<VscProgramInfo> _programCache = new List<VscProgramInfo>();
        private int _currentProgramNo = 0;
        private TaskCompletionSource<string> _commandResponseTcs;
        private int _frameDispatchCount = 0;
        public string ip;
        public int port;
        private int timeout;
        private ProtocolMode _protocol = ProtocolMode.Line;


        public string FtpImagePath { get; set; }

        public event Action<string> OnCameraFrameReceived;
        public event Action<int> CurrentProgramChanged;

        public VscCamera(string ipAddr, int portNum, int timeoutMs)
        {
            ip = ipAddr;
            port = portNum;
            timeout = timeoutMs;
        }

        public void SetProtocolMode(ProtocolMode mode) => _protocol = mode;

        // ══════════════════════════════════════════
        // CONNECT
        // ══════════════════════════════════════════
        public bool Connect()
        {
            try
            {
                CleanupSocket();
                _socket = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Stream,
                                     ProtocolType.Tcp);
                _socket.SendTimeout = timeout;
                _socket.NoDelay = true;
                SetKeepAlive(_socket, keepAliveTime: 10_000,
                                      keepAliveInterval: 2_000,
                                      probeCount: 3);
                var result = _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), null, null);
                bool connected = result.AsyncWaitHandle.WaitOne(timeout, true);
                if (!connected || !_socket.Connected)
                {
                    Console.WriteLine($"[VSC] ❌ Connect timeout sau {timeout}ms @ {ip}:{port}");
                    CleanupSocket();
                    return false;
                }
                _socket.EndConnect(result);
                _stream = new NetworkStream(_socket, ownsSocket: false);
                _rxBuffer.Clear();

                // KẾT NỐI FTP NGAY LẬP TỨC (giống reference project)
                Console.WriteLine($"[VSC FTP] Connecting to {ip}:21...");
                _ftpClient = new AsyncFtpClient(ip, 21);
                _ftpClient.Config.ConnectTimeout = timeout;
                _ftpClient.Config.ReadTimeout = timeout;
                _ftpClient.Config.DataConnectionConnectTimeout = timeout;
                _ftpClient.Config.DataConnectionReadTimeout = timeout;
                _ftpClient.Config.SelfConnectMode = FtpSelfConnectMode.Always;
                // KHÔNG có DataConnectionType = AutoPassive

                _ftpClient.Connect();
                Console.WriteLine($"[VSC FTP] ✅ FTP connected @ {ip}:21");
                _currentProgramNo = Shared.Settings?.CameraList?.FirstOrDefault()?.KeyenceCurrentProgramNo ?? 0;
                Console.WriteLine($"[VSC] Current program from settings: {_currentProgramNo:D4}");
                Console.WriteLine($"[VSC] ✅ Connected @ {ip}:{port}");
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VSC] ❌ Connect failed: {ex.Message}");
                CleanupSocket();
                try { _ftpClient?.Dispose(); } catch { }
                _ftpClient = null;
                return false;
            }
        }
        // Optional: enable FTP (to read program list)
        //public bool EnableFtp(int ftpPort = 21)
        //{
        //    if (_socket == null || !_socket.Connected)
        //    {
        //        Console.WriteLine("[VSC] EnableFtp fail: socket chưa connect");
        //        return false;
        //    }
        //    try
        //    {
        //        _ftpPort = ftpPort;
        //        Console.WriteLine($"[VSC FTP] Connecting to {ip}:{_ftpPort}...");

        //        _ftpClient = new AsyncFtpClient(ip, _ftpPort);
        //        _ftpClient.Config.ConnectTimeout = timeout;
        //        _ftpClient.Config.ReadTimeout = timeout;
        //        _ftpClient.Config.DataConnectionConnectTimeout = timeout;
        //        _ftpClient.Config.DataConnectionReadTimeout = timeout;
        //        _ftpClient.Config.SelfConnectMode = FtpSelfConnectMode.Always;
        //        _ftpClient.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;

        //        _ftpClient.Connect();
        //        Console.WriteLine($"[VSC FTP] FTP connected successfully @ {ip}:{_ftpPort}");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[VSC FTP] FTP enable failed: {ex.GetType().Name} - {ex.Message}");
        //        _ftpClient = null;
        //        return false;
        //    }
        //}
        public async Task<IReadOnlyList<VscProgramInfo>> GetProgramListAsync(CancellationToken cancellationToken = default)
        {
            if (_ftpClient == null)
                throw new InvalidOperationException("FTP client chưa được khởi tạo. Hãy kết nối trước.");

            // Kiểm tra FTP connection — reconnect nếu cần
            if (!_ftpClient.IsConnected)
            {
                Console.WriteLine($"[VSC FTP] FTP disconnected, attempting reconnect...");
                try
                {
                    _ftpClient?.Dispose();
                    _ftpClient = new AsyncFtpClient(ip, 21);
                    _ftpClient.Config.ConnectTimeout = timeout;
                    _ftpClient.Config.ReadTimeout = timeout;
                    _ftpClient.Config.DataConnectionConnectTimeout = timeout;
                    _ftpClient.Config.DataConnectionReadTimeout = timeout;
                    _ftpClient.Config.SelfConnectMode = FtpSelfConnectMode.Always;
                    _ftpClient.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;
                    await _ftpClient.Connect(cancellationToken);
                    Console.WriteLine($"[VSC FTP] ✅ FTP reconnected @ {ip}:21");
                }
                catch (Exception reconnectEx)
                {
                    throw new InvalidOperationException($"FTP không kết nối được. Chi tiết: {reconnectEx.Message}");
                }
            }

            try
            {
                var result = new List<VscProgramInfo>();
                var list = await _ftpClient.GetListing("VS/Camera/Programs", cancellationToken).ConfigureAwait(false);
                foreach (var item in list)
                {
                    var program = ParseProgram(item);
                    if (program != null)
                        result.Add(program);
                }
                lock (_sync)
                {
                    _programCache.Clear();
                    _programCache.AddRange(result);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Lỗi khi đọc danh sách program từ thiết bị.", ex);
            }
        }

        private Bitmap DownloadErrorImage(string imageNumber)
        {
            try
            {
                string basePath = FtpImagePath;
                if (string.IsNullOrEmpty(basePath))
                    basePath = Shared.Settings?.THFtpImagePath
                        ?? @"D:\hinhanh\VS\Camera\Images";
                if (!Directory.Exists(basePath)) return null;
                string pattern = $"*{imageNumber}*.png";
                string altPattern = $"*{imageNumber}*.bmp";
                string[] extensions = { ".png", ".bmp", ".jpg" };
                // Thử ngay lập tức
                var result = FindImageFile(basePath, imageNumber, extensions);
                if (result != null)
                {
                    Console.WriteLine($"[VSC] Loaded: {result}");
                    using (var fs = new FileStream(result, FileMode.Open, FileAccess.Read, FileShare.Read))
                        return new Bitmap(fs);
                }
                // Chưa có → chờ 500ms thử lại 1 lần
                Thread.Sleep(500);
                result = FindImageFile(basePath, imageNumber, extensions);
                if (result != null)
                {
                    Console.WriteLine($"[VSC] Loaded (retry): {result}");
                    using (var fs = new FileStream(result, FileMode.Open, FileAccess.Read, FileShare.Read))
                        return new Bitmap(fs);
                }
                Console.WriteLine($"[VSC] No image '{imageNumber}' found");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VSC] Load error image failed: {ex.Message}");
                return null;
            }
        }

        private static string FindImageFile(string basePath, string imageNumber, string[] extensions)
        {
            string pattern = $"*{imageNumber}*.*";
            return Directory.EnumerateFiles(basePath, pattern, SearchOption.AllDirectories)
                .FirstOrDefault(f =>
                {
                    string ext = Path.GetExtension(f);
                    return ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
                        || ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
                        || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase);
                });
        }
        public async Task<(bool success, string message)> ChangeProgramAsync(int programNo, CancellationToken cancellationToken = default)
        {
            if (_protocol == ProtocolMode.Framed)
            {
                // ── Gửi RUN phòng trường hợp camera đang ở SETUP ──
                // Camera ở SETUP → chuyển sang RUN, camera ở RUN → echo "RUN"
                // Cả hai trường hợp đều cần discard để không leak vào PL TCS
                SendRawCommandFramed("RUN");

                // Chờ RUN response (hoặc timeout 2s nếu camera đã ở RUN → không phản hồi)
                _commandResponseTcs = new TaskCompletionSource<string>();
                var runCheck = await Task.WhenAny(
                    _commandResponseTcs.Task,
                    Task.Delay(2000, cancellationToken)
                ).ConfigureAwait(false);
                // Discard RUN/ER response — không dùng, chỉ để tránh race condition

                // TCS mới cho PL — đảm bảo sạch, không bị污染 bởi RUN response
                _commandResponseTcs = new TaskCompletionSource<string>();

                // Gửi PL,1,{programNo}
                string plCmd = $"PL,1,{programNo:D4}";
                var plPayload = new byte[plCmd.Length + 2];
                plPayload[0] = 0x02;
                Encoding.ASCII.GetBytes(plCmd, 0, plCmd.Length, plPayload, 1);
                plPayload[plPayload.Length - 1] = 0x03;

                int sent = 0;
                while (sent < plPayload.Length)
                {
                    int s = _socket.Send(plPayload, sent, plPayload.Length - sent, SocketFlags.None);
                    if (s <= 0)
                        throw new SocketException((int)SocketError.ConnectionReset);
                    sent += s;
                }

                Console.WriteLine($"[VSC] Sent PL,1,{programNo:D4}");

                // Chờ response PL từ camera (timeout 10s)
                var plCompleted = await Task.WhenAny(
                    _commandResponseTcs.Task,
                    Task.Delay(10000, cancellationToken)
                ).ConfigureAwait(false);

                if (plCompleted != _commandResponseTcs.Task)
                {
                    return (false, "Camera không phản hồi lệnh PL sau 10 giây");
                }

                string plResponse = _commandResponseTcs.Task.Result;
                string plNormalized = plResponse?.Replace("\x02", "").Replace("\x03", "").Trim();

                // Response "ER,PL,xxx" = thất bại
                if (plNormalized?.StartsWith("ER", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return (false, $"Camera lỗi: {plNormalized}");
                }

                // Response "PL" = camera chấp nhận lệnh
                if (!string.Equals(plNormalized, "PL", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, $"Camera phản hồi không xác định: {plNormalized}");
                }

                // Đảm bảo camera ở chế độ RUN (sau khi PL → camera chắc chắn đã ở RUN)
                // Vẫn gửi RUN + discard để tránh race condition với PR TCS
                SendRawCommandFramed("RUN");
                _commandResponseTcs = new TaskCompletionSource<string>();
                await Task.WhenAny(
                    _commandResponseTcs.Task,
                    Task.Delay(2000, cancellationToken)
                ).ConfigureAwait(false);
                // Discard RUN response

                // Gửi PR để xác minh program đã đổi thành công
                _commandResponseTcs = new TaskCompletionSource<string>();
                string prCmd = "PR";
                var prPayload = new byte[prCmd.Length + 2];
                prPayload[0] = 0x02;
                Encoding.ASCII.GetBytes(prCmd, 0, prCmd.Length, prPayload, 1);
                prPayload[prPayload.Length - 1] = 0x03;

                sent = 0;
                while (sent < prPayload.Length)
                {
                    int s = _socket.Send(prPayload, sent, prPayload.Length - sent, SocketFlags.None);
                    if (s <= 0)
                        throw new SocketException((int)SocketError.ConnectionReset);
                    sent += s;
                }

                Console.WriteLine($"[VSC] Sent PR to verify");

                // Chờ response PR (timeout 10s)
                var prCompleted = await Task.WhenAny(
                    _commandResponseTcs.Task,
                    Task.Delay(10000, cancellationToken)
                ).ConfigureAwait(false);

                string prNormalized;
                if (prCompleted != _commandResponseTcs.Task)
                {
                    // PL đã thành công, chỉ PR timeout → chấp nhận
                    _currentProgramNo = programNo;
                    return (true, $"Đã đổi sang program {programNo:D4} (PL ok, PR timeout)");
                }

                string prResponse = _commandResponseTcs.Task.Result;
                prNormalized = prResponse?.Replace("\x02", "").Replace("\x03", "").Trim();

                // Parse PR,1,{confirmedNo}
                if (prNormalized != null && prNormalized.StartsWith("PR,1,", StringComparison.OrdinalIgnoreCase))
                {
                    string noStr = prNormalized.Substring(5);
                    if (int.TryParse(noStr, out int confirmedNo))
                    {
                        _currentProgramNo = confirmedNo;
                        if (confirmedNo == programNo)
                        {
                            return (true, $"Đã đổi sang program {programNo:D4} thành công (xác minh PR: {confirmedNo:D4})");
                        }
                        else
                        {
                            return (false, $"Camera trả về program {confirmedNo:D4}, yêu cầu {programNo:D4}");
                        }
                    }
                }

                // PR không parse được → chấp nhận PL thành công
                _currentProgramNo = programNo;
                return (true, $"Đã đổi sang program {programNo:D4} thành công (PL ok, PR: {prNormalized})");
            }
            else
            {
                SetRunMode();
                Thread.Sleep(100);
                if (!SendCommand($"PN{programNo:D4}"))
                    return (false, "Gửi lệnh thất bại");
                _currentProgramNo = programNo;
                return (true, $"Đã đổi sang program {programNo:D4} (non-framed mode)");
            }
        }
        // ══════════════════════════════════════════
        // START LISTENING
        // ══════════════════════════════════════════
        public bool StartListening()
        {
            if (_socket == null || !_socket.Connected) return false;

            _cts = new CancellationTokenSource();

            if (_protocol == ProtocolMode.Framed)
            {
                Task.Run(() => FramedListenLoop(_cts.Token), _cts.Token);
            }
            else
            {
                Task.Run(() => TextListenLoop(_cts.Token), _cts.Token);
            }
            return true;
        }
        // ══════════════════════════════════════════
        // TEXT LOOP — 1 socket, không duplicate
        // ══════════════════════════════════════════
        private void TextListenLoop(CancellationToken token)
        {
            Console.WriteLine($"[VSC] Listen loop started @ {DateTime.Now}");
            const int POLL_US = 50_000; // 50ms

            while (!token.IsCancellationRequested)
            {
                if (_socket == null || !_socket.Connected)
                {
                    Console.WriteLine("[VSC] Socket mất kết nối, thoát loop");
                    break;
                }

                try
                {
                    // Poll để không block vĩnh viễn
                    if (!_socket.Poll(POLL_US, SelectMode.SelectRead))
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    int n = _socket.Receive(_textBuf);
                    if (n <= 0)
                    {
                        Console.WriteLine("[VSC] Server đóng kết nối (n=0)");
                        break;
                    }

                    string chunk = Encoding.ASCII.GetString(_textBuf, 0, n);
                    Debug.WriteLine($"[VSC] RX raw: {chunk.Replace("\r", "\\r").Replace("\n", "\\n")}");

                    _rxBuffer.Append(chunk);
                    ProcessBufferedFrames();
                }
                catch (SocketException ex)
                    when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    // Timeout receive bình thường, tiếp tục
                    continue;
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"[VSC] SocketException: {ex.SocketErrorCode} - {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VSC] Exception: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine($"[VSC] Listen loop ended @ {DateTime.Now}");

            // Auto-reconnect nếu không phải do Cancel chủ động
            if (!token.IsCancellationRequested)
            {
                Console.WriteLine("[VSC] Mất kết nối ngoài ý muốn → reconnect sau 2s...");
                Thread.Sleep(2000);
                Reconnect();
            }
        }
        private void FramedListenLoop(CancellationToken token)
        {
            const byte STX = 0x02;
            const byte ETX = 0x03;
            var buffer = new List<byte>();

            while (!token.IsCancellationRequested)
            {
                if (_socket == null || !_socket.Connected) break;

                try
                {
                    if (!_socket.Poll(50_000, SelectMode.SelectRead))
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    int n = _socket.Receive(_textBuf);
                    if (n <= 0) break;

                    buffer.AddRange(_textBuf.Take(n));

                    // Tìm STX...ETX frames
                    while (true)
                    {
                        int stxIdx = buffer.IndexOf(STX);
                        if (stxIdx < 0) { buffer.Clear(); break; }

                        int etxIdx = buffer.IndexOf(ETX, stxIdx + 1);
                        if (etxIdx < 0) break;  // Chưa có ETX, chờ thêm data

                        int len = etxIdx - stxIdx - 1;
                        if (len > 0)
                        {
                            var frameBytes = buffer.GetRange(stxIdx + 1, len).ToArray();
                            string frame = Encoding.ASCII.GetString(frameBytes).Trim('\r', '\n');
                            if (!string.IsNullOrEmpty(frame))
                            {
                                Console.WriteLine($"[VSC] Received frame: '{frame}'");
                                DispatchFrame(frame);
                            }
                        }

                        buffer.RemoveRange(0, etxIdx + 1);
                    }
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VSC] FramedListenLoop error: {ex.Message}");
                    break;
                }
            }
        }
        private void ProcessBufferedFrames()
        {
            string buf = _rxBuffer.ToString();
            int start = 0;

            for (int i = 0; i < buf.Length; i++)
            {
                if (buf[i] != '\r' && buf[i] != '\n') continue;
                if (i > start)
                {
                    string frame = buf.Substring(start, i - start).Trim();
                    if (!string.IsNullOrEmpty(frame))
                        DispatchFrame(frame);
                }
                start = i + 1;
            }

            _rxBuffer.Clear();
            if (start < buf.Length)
                _rxBuffer.Append(buf, start, buf.Length - start);
        }

        private void DispatchFrame(string frame)
        {
            if (string.Equals(frame.Trim(), "TRG", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[VSC] Ignoring echo frame: TRG");
                return;
            }

            // Bắt command response (PL, ER, RUNPL, RUNER, PR,...)
            string upperFrame = frame.Trim().ToUpper();
            if (upperFrame.StartsWith("RUN") || upperFrame.StartsWith("ER")
                || upperFrame == "PL" || upperFrame.StartsWith("PR,"))
            {
                Console.WriteLine($"[VSC] Command response: '{frame}'");
                _commandResponseTcs?.TrySetResult(frame.Trim());
                return;
            }

            Console.WriteLine($"[VSC] DispatchFrame #{++_frameDispatchCount}: '{frame}'");

            string cleanedFrame = frame.StartsWith("TRG", StringComparison.OrdinalIgnoreCase)
                ? frame.Substring(3)
                : frame;
            string[] parts = cleanedFrame.Split(',');

            // Format: QR,QR_Result,NSX,Time,HSD,Batch,OCR_Result,Image_ID
            string qr = parts.Length > 0 ? parts[0].Trim() : "";
            string qrResult = parts.Length > 1 ? parts[1].Trim() : "";
            string nsx = parts.Length > 2 ? parts[2].Trim() : "";
            string time = parts.Length > 3 ? parts[3].Trim() : "";
            string hsd = parts.Length > 4 ? parts[4].Trim() : "";
            string batch = parts.Length > 5 ? parts[5].Trim() : "";
            string ocrResult = parts.Length > 6 ? parts[6].Trim() : "";
            string imageId = parts.Length > 7 ? parts[7].Trim() : "";

            bool qrPass = IsResultOk(qrResult);
            bool ocrPass = IsResultOk(ocrResult);

            var detect = new DetectModel
            {
                Text = qr,
                Image = null,
                CompareResult = (qrPass && ocrPass)
                    ? ComparisonResult.Valid
                    : ComparisonResult.Invalided,
            };
            detect.ExtraFields["QR_RESULT"] = qrPass ? "OK" : "NG";
            detect.ExtraFields["OCR_RESULT"] = ocrPass ? "OK" : "NG";
            detect.ExtraFields["OCR_STATUS"] = ocrPass ? "OK" : "NG";
            detect.ExtraFields["NSX"] = StripPrefix(nsx, "NSX:");
            detect.ExtraFields["TIME"] = time;
            detect.ExtraFields["HSD"] = StripPrefix(hsd, "HSD:");
            detect.ExtraFields["BATCH"] = batch;

            detect.ExtraFields["RAW_FRAME"] = cleanedFrame;
            detect.ExtraFields["RECEIVE_TIME"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            // Không download ngay, lưu imageId vào ExtraFields để Compare thread xử lý
            if (!string.IsNullOrEmpty(imageId))
                detect.ExtraFields["IMAGE_ID"] = imageId;

            Console.WriteLine($"[VSC] Frame: QR='{qr}' QR_Result={qrPass} OCR_Result={ocrPass} ImageID='{imageId}'");
            OnCameraFrameReceived?.Invoke(cleanedFrame);
            Shared.RaiseOnCameraReadDataChangeEvent(detect);
        }

        private static bool IsResultOk(string val)
        {
            return val.Equals("OK", StringComparison.OrdinalIgnoreCase)
                || val == "1"
                || val.Equals("PASS", StringComparison.OrdinalIgnoreCase)
                || val.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
        }

        private static string StripPrefix(string value, string prefix)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return value.Substring(prefix.Length).Trim();
            return value.Trim();
        }

        // ══════════════════════════════════════════
        // SEND COMMAND (trigger, đổi program, v.v.) — line mode
        // ══════════════════════════════════════════
        public bool SendCommand(string command)
        {
            try
            {
                if (_socket == null || !_socket.Connected)
                    throw new InvalidOperationException("Chưa kết nối");

                byte[] data = Encoding.ASCII.GetBytes(command + "\r\n");
                _socket.Send(data);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VSC] SendCommand fail: {ex.Message}");
                return false;
            }
        }

        public bool Trigger() => SendCommand("T1");
        public bool SetRunMode() => SendCommand("RUN");
        public bool SetSetupMode() => SendCommand("SET");
        public bool ChangeProgram(int no) => SendCommand($"PN{no:D4}");

        // ══════════════════════════════════════════
        // FRAMED (STX/ETX) helpers (similar to KeyenceVS)
        // ══════════════════════════════════════════
        public bool SendRawCommandFramed(string command)
        {
            if (_socket == null || !_socket.Connected)
            {
                Console.WriteLine("[VSC] SendRawCommandFramed fail: socket not connected");
                return false;
            }

            var payload = new byte[command.Length + 2];
            payload[0] = 0x02;
            Encoding.ASCII.GetBytes(command, 0, command.Length, payload, 1);
            payload[payload.Length - 1] = 0x03;

            int sent = 0;
            while (sent < payload.Length)
            {
                int s = _socket.Send(payload, sent, payload.Length - sent, SocketFlags.None);
                if (s <= 0)
                    throw new SocketException((int)SocketError.ConnectionReset);
                sent += s;
            }

            return true;
        }

        public string SendAndReceiveFramed(string command, int waitMs = 3000)
        {
            if (!SendRawCommandFramed(command))
                return null;

            var frames = ReadAllFrames(waitMs);
            if (frames == null || frames.Count == 0)
                return null;

            foreach (var frame in frames)
            {
                var text = (frame ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(text))
                    return text;
            }

            return null;
        }

        public int QueryCurrentProgramNo()
        {
            string response = SendAndReceiveFramed("PR", 3000);
            if (string.IsNullOrWhiteSpace(response))
                return -1;

            int idx = response.IndexOf("PR", StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return -1;

            string[] parts = response.Substring(idx).Split(',');
            for (int i = parts.Length - 1; i >= 1; i--)
            {
                string token = (parts[i] ?? string.Empty).Trim();
                if (int.TryParse(token, out int no))
                {
                    _currentProgramNo = no;
                    CurrentProgramChanged?.Invoke(no);
                    return no;
                }
            }

            return -1;
        }

        public VSMeasurementResult TriggerFramed()
        {
            var result = new VSMeasurementResult { Code = VSResponseCode.OK };

            if (_socket == null || !_socket.Connected)
            {
                result.Code = VSResponseCode.ConnectionError;
                Console.WriteLine("[VSC] TriggerFramed: socket not connected");
                return result;
            }

            try
            {
                SendRawCommandFramed("TRG");
            }
            catch (Exception ex)
            {
                result.Code = VSResponseCode.ConnectionError;
                Console.WriteLine("[VSC] TriggerFramed send error: " + ex.Message);
                return result;
            }

            var frames = ReadAllFrames(Math.Max(timeout, 5000));
            if (frames == null || frames.Count == 0)
            {
                result.Code = VSResponseCode.Timeout;
                return result;
            }

            string dataFrame = null;
            foreach (var frame in frames)
            {
                var trimmed = (frame ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(trimmed) && !string.Equals(trimmed, "TRG", StringComparison.OrdinalIgnoreCase))
                {
                    dataFrame = trimmed;
                    break;
                }
            }

            if (dataFrame == null)
            {
                var moreFrames = ReadAllFrames(Math.Max(timeout, 5000));
                foreach (var frame in moreFrames)
                {
                    var trimmed = (frame ?? string.Empty).Trim();
                    if (!string.IsNullOrEmpty(trimmed) && !string.Equals(trimmed, "TRG", StringComparison.OrdinalIgnoreCase))
                    {
                        dataFrame = trimmed;
                        break;
                    }
                }
            }

            if (dataFrame == null)
            {
                result.Code = VSResponseCode.Timeout;
                return result;
            }

            result.RawResponse = dataFrame;

            string cleaned = dataFrame.StartsWith("TRG", StringComparison.OrdinalIgnoreCase)
                ? dataFrame.Substring(3).TrimStart(',').Trim()
                : dataFrame.Trim();

            var tokens = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var data = new List<string>();
            bool firstToken = true;

            foreach (var token in tokens)
            {
                var t = token.Trim();
                if (string.IsNullOrEmpty(t))
                    continue;

                if (firstToken)
                {
                    firstToken = false;
                    if (t == "0" || t == "1")
                    {
                        result.Judgment = t == "1";
                        continue;
                    }
                }

                data.Add(t);
            }

            result.StringData = data.ToArray();
            result.Code = VSResponseCode.OK;
            return result;
        }

        public VSImageResult ReceiveImageFramed(int waitMs = 5000, string savePath = null)
        {
            var result = new VSImageResult { Code = VSResponseCode.OK };
            var raw = ReceiveFramedResponse(waitMs);
            result.RawResponse = raw;

            if (string.IsNullOrEmpty(raw))
            {
                result.Code = VSResponseCode.Timeout;
                return result;
            }

            var payload = raw.Trim();
            if (payload.StartsWith("TRG", StringComparison.OrdinalIgnoreCase))
                payload = payload.Substring(3).Trim();

            int comma = payload.IndexOf(',');
            if (comma <= 0 || comma >= payload.Length - 1)
            {
                result.Code = VSResponseCode.ParseError;
                return result;
            }

            string header = payload.Substring(0, comma).Trim();
            string data = payload.Substring(comma + 1).Trim();

            try
            {
                if (header.IndexOf("JPEG", StringComparison.OrdinalIgnoreCase) >= 0 || header.IndexOf("JPG", StringComparison.OrdinalIgnoreCase) >= 0)
                    result.ContentType = "image/jpeg";
                else if (header.IndexOf("PNG", StringComparison.OrdinalIgnoreCase) >= 0)
                    result.ContentType = "image/png";
                else
                    result.ContentType = "application/octet-stream";

                result.ImageBytes = Convert.FromBase64String(data);
                using (var ms = new MemoryStream(result.ImageBytes))
                using (var temp = Image.FromStream(ms))
                {
                    result.Image = new Bitmap(temp);
                }

                if (!string.IsNullOrWhiteSpace(savePath))
                {
                    File.WriteAllBytes(savePath, result.ImageBytes);
                    result.SavePath = savePath;
                }
            }
            catch (Exception ex)
            {
                result.Code = VSResponseCode.ParseError;
                Console.WriteLine("[VSC] ReceiveImageFramed decode error: " + ex.Message);
            }

            return result;
        }

        // Framed read helpers (STX/ETX)
        private List<string> ReadAllFrames(int waitMs)
        {
            const byte STX = 0x02;
            const byte ETX = 0x03;

            var result = new List<string>();
            var stream = new MemoryStream();
            var sw = Stopwatch.StartNew();
            int originalTimeout = 0;
            try { originalTimeout = _socket.ReceiveTimeout; } catch { }

            try
            {
                try { _socket.ReceiveTimeout = 200; } catch { }

                while (sw.ElapsedMilliseconds < waitMs)
                {
                    try
                    {
                        if (_socket.Available == 0)
                        {
                            if (HasNonEchoFrame(result))
                                break;

                            Thread.Sleep(10);
                            continue;
                        }

                        int toRead = Math.Min(_recvBuffer.Length, Math.Max(_socket.Available, 1));
                        int read = _socket.Receive(_recvBuffer, 0, toRead, SocketFlags.None);
                        if (read <= 0)
                            break;

                        stream.Write(_recvBuffer, 0, read);
                        ExtractFrames(stream.ToArray(), result, STX, ETX);

                        if (HasNonEchoFrame(result))
                            break;
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        continue;
                    }
                }

                return result;
            }
            finally
            {
                try { _socket.ReceiveTimeout = originalTimeout; } catch { }
            }
        }

        private string ReceiveFramedResponse(int waitMs)
        {
            if (_socket == null || !_socket.Connected)
                return null;

            const byte STX = 0x02;
            const byte ETX = 0x03;
            var stream = new MemoryStream();
            var sw = Stopwatch.StartNew();
            int originalTimeout = 0;
            try { originalTimeout = _socket.ReceiveTimeout; } catch { }

            try
            {
                try { _socket.ReceiveTimeout = 200; } catch { }

                while (sw.ElapsedMilliseconds < waitMs)
                {
                    try
                    {
                        if (_socket.Available == 0)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        int toRead = Math.Min(_recvBuffer.Length, Math.Max(_socket.Available, 1));
                        int read = _socket.Receive(_recvBuffer, 0, toRead, SocketFlags.None);
                        if (read <= 0)
                            break;

                        stream.Write(_recvBuffer, 0, read);
                        var full = stream.ToArray();

                        int etxIndex = Array.IndexOf(full, ETX);
                        if (etxIndex < 0)
                            continue;

                        int stxIndex = Array.IndexOf(full, STX);
                        int start = stxIndex >= 0 ? stxIndex + 1 : 0;
                        int count = Math.Max(0, etxIndex - start);
                        var content = Encoding.ASCII.GetString(full, start, count).Trim('\r', '\n');

                        return content;
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        continue;
                    }
                }

                if (stream.Length <= 0)
                    return null;

                var raw = stream.ToArray();
                int stxIndex2 = Array.IndexOf(raw, STX);
                int etxIndex2 = Array.IndexOf(raw, ETX);
                int start2 = stxIndex2 >= 0 ? stxIndex2 + 1 : 0;
                int count2 = etxIndex2 > start2 ? etxIndex2 - start2 : raw.Length - start2;
                var partial = Encoding.ASCII.GetString(raw, start2, Math.Max(0, count2)).Trim('\r', '\n');
                return partial;
            }
            finally
            {
                try { _socket.ReceiveTimeout = originalTimeout; } catch { }
            }
        }

        private static void ExtractFrames(byte[] raw, List<string> result, byte stx, byte etx)
        {
            int pos = 0;
            while (pos < raw.Length)
            {
                int stxPos = Array.IndexOf(raw, stx, pos);
                if (stxPos < 0)
                    break;

                int etxPos = Array.IndexOf(raw, etx, stxPos + 1);
                if (etxPos < 0)
                    break;

                int contentLen = etxPos - stxPos - 1;
                var content = contentLen > 0
                    ? Encoding.ASCII.GetString(raw, stxPos + 1, contentLen).Trim('\r', '\n')
                    : string.Empty;

                result.Add(content);
                pos = etxPos + 1;
            }
        }

        private static bool HasNonEchoFrame(IEnumerable<string> frames)
        {
            foreach (var frame in frames)
            {
                if (!string.Equals((frame ?? string.Empty).Trim(), "TRG", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        // ══════════════════════════════════════════
        // RECONNECT / DISCONNECT
        // ══════════════════════════════════════════
        private void Reconnect()
        {
            CleanupSocket();
            if (Connect()) StartListening();
            else Console.WriteLine("[VSC] ❌ Reconnect thất bại");
        }

        public void Disconnect()
        {
            try { _cts?.Cancel(); } catch { }
            Thread.Sleep(100);
            CleanupSocket();
            try
            {
                if (_ftpClient != null)
                {
                    _ftpClient.Disconnect();
                    _ftpClient.Dispose();
                }
            }
            catch { }
            _ftpClient = null;
        }

        private void CleanupSocket()
        {
            try
            {
                _stream?.Dispose();
                if (_socket?.Connected == true)
                    _socket.Shutdown(SocketShutdown.Both);
                _socket?.Dispose();
            }
            catch { }
            _stream = null;
            _socket = null;
            _rxBuffer.Clear();
        }

        public bool IsConnected() => _socket?.Connected == true;

        // ══════════════════════════════════════════
        // KEEP-ALIVE
        // ══════════════════════════════════════════
        private static void SetKeepAlive(Socket socket,
            uint keepAliveTime, uint keepAliveInterval, int probeCount)
        {
            try
            {
                uint size = (uint)Marshal.SizeOf(typeof(uint));
                byte[] inOpt = new byte[size * 3];
                BitConverter.GetBytes(1u).CopyTo(inOpt, 0);
                BitConverter.GetBytes(keepAliveTime).CopyTo(inOpt, (int)size);
                BitConverter.GetBytes(keepAliveInterval).CopyTo(inOpt, (int)(size * 2));
                socket.IOControl(IOControlCode.KeepAliveValues, inOpt, new byte[inOpt.Length]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VSC] KeepAlive fail: {ex.Message}");
            }
        }

        // Test helper
        public void SimulateFrame(string qr, bool ocrPass)
            => DispatchFrame($"{qr},{(ocrPass ? "OK" : "NG")}");
        public void StopListening()
        {
            try { _cts?.Cancel(); } catch { }
            Console.WriteLine("[VSC] StopListening called");
        }

        // Program parse reused from KeyenceVS
        private static VscProgramInfo ParseProgram(FluentFTP.FtpListItem item)
        {
            if (item == null || item.Type != FluentFTP.FtpObjectType.Directory)
                return null;

            var name = item.Name;
            if (string.IsNullOrWhiteSpace(name) || name.Length < 5)
                return null;

            string sProgramNo = name.Substring(0, 4);
            string sProgramName = name.Substring(5);

            if (!int.TryParse(sProgramNo, out int programNo))
                return null;

            return new VscProgramInfo
            {
                ProgramNo = programNo,
                Name = sProgramName,
                Location = "Local",
                ModifyInfo = "Modified : " + item.Modified.ToString("yy/MM/dd - HH:mm:ss")
            };
        }
    }

    // Minimal result types reused for framed helpers
    public enum VSResponseCode
    {
        OK = 0,
        Timeout = 99,
        ConnectionError = 100,
        ParseError = 101
    }

    public class VSMeasurementResult
    {
        public VSResponseCode Code { get; set; }
        public bool Judgment { get; set; }
        public string[] StringData { get; set; } = Array.Empty<string>();
        public string RawResponse { get; set; }
    }

    public class VSImageResult
    {
        public VSResponseCode Code { get; set; }
        public string RawResponse { get; set; }
        public byte[] ImageBytes { get; set; }
        public Image Image { get; set; }
        public string ContentType { get; set; }
        public string SavePath { get; set; }
        public bool HasImage => ImageBytes != null && ImageBytes.Length > 0;
    }
}



// File: BarcodeVerificationSystem\Controller\Camera\Keyence\VscCamera.cs


//using BarcodeVerificationSystem.Model;
//using FluentFTP;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BarcodeVerificationSystem.Controller.Camera.Keyence
//{
//    public enum ProtocolMode
//    {
//        Line,
//        Framed
//    }

//    public class VscProgramInfo
//    {
//        public int ProgramNo { get; set; }
//        public string Name { get; set; }
//        public string Location { get; set; }
//        public string ModifyInfo { get; set; }

//        public override string ToString()
//            => ProgramNo.ToString("D4") + (string.IsNullOrWhiteSpace(Name) ? string.Empty : " " + Name);
//    }

//    public class VscCamera
//    {
//        private Socket _socket;           // 1 socket duy nhất
//        private NetworkStream _stream;
//        private readonly byte[] _textBuf = new byte[4096];
//        private readonly StringBuilder _rxBuffer = new StringBuilder();
//        private CancellationTokenSource _cts;

//        // Framed helpers
//        private readonly byte[] _recvBuffer = new byte[8192];
//        private AsyncFtpClient _ftpClient;
//        private int _ftpPort = 21;
//        private readonly object _sync = new object();
//        private readonly List<VscProgramInfo> _programCache = new List<VscProgramInfo>();
//        private int _currentProgramNo = 0;

//        // Program command helpers
//        private string _commandFlag = string.Empty;
//        private TaskCompletionSource<VscProgramInfo> _currentProgramTcs;
//        private volatile bool _isListening = false;

//        public string ip;
//        public int port;
//        private int timeout;
//        private ProtocolMode _protocol = ProtocolMode.Line;


//        public string FtpImagePath { get; set; }

//        public event Action<string> OnCameraFrameReceived;
//        public event Action<VscProgramInfo> CurrentProgramChanged;

//        public VscCamera(string ipAddr, int portNum, int timeoutMs)
//        {
//            ip = ipAddr;
//            port = portNum;
//            timeout = timeoutMs;
//        }

//        public void SetProtocolMode(ProtocolMode mode) => _protocol = mode;

//        // ══════════════════════════════════════════
//        // CONNECT
//        // ══════════════════════════════════════════
//        public bool Connect()
//        {
//            try
//            {
//                CleanupSocket();
//                _socket = new Socket(AddressFamily.InterNetwork,
//                                     SocketType.Stream,
//                                     ProtocolType.Tcp);
//                _socket.SendTimeout = timeout;
//                _socket.NoDelay = true;
//                SetKeepAlive(_socket, keepAliveTime: 10_000,
//                                      keepAliveInterval: 2_000,
//                                      probeCount: 3);
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

//                // KẾT NỐI FTP NGAY LẬP TỨC (giống reference project)
//                Console.WriteLine($"[VSC FTP] Connecting to {ip}:21...");
//                _ftpClient = new AsyncFtpClient(ip, 21);
//                _ftpClient.Config.ConnectTimeout = timeout;
//                _ftpClient.Config.ReadTimeout = timeout;
//                _ftpClient.Config.DataConnectionConnectTimeout = timeout;
//                _ftpClient.Config.DataConnectionReadTimeout = timeout;
//                _ftpClient.Config.SelfConnectMode = FtpSelfConnectMode.Always;
//                // KHÔNG có DataConnectionType = AutoPassive

//                _ftpClient.Connect();
//                Console.WriteLine($"[VSC FTP] ✅ FTP connected @ {ip}:21");

//                // Đọc program hiện tại trực tiếp từ camera bằng lệnh PR.
//                // Nếu camera không phản hồi PR thì mới fallback về setting cũ.
//                try
//                {
//                    var currentProgram = QueryCurrentProgramDirect(timeout);
//                    if (currentProgram != null)
//                    {
//                        _currentProgramNo = currentProgram.ProgramNo;
//                        Console.WriteLine($"[VSC] Current program from camera PR: {_currentProgramNo:D4}");
//                    }
//                    else
//                    {
//                        _currentProgramNo = Shared.Settings?.CameraList?.FirstOrDefault()?.KeyenceCurrentProgramNo ?? 0;
//                        Console.WriteLine($"[VSC] Current program fallback from settings: {_currentProgramNo:D4}");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _currentProgramNo = Shared.Settings?.CameraList?.FirstOrDefault()?.KeyenceCurrentProgramNo ?? 0;
//                    Console.WriteLine($"[VSC] Query current program by PR failed: {ex.Message}. Fallback settings: {_currentProgramNo:D4}");
//                }

//                Console.WriteLine($"[VSC] ✅ Connected @ {ip}:{port}");
//                return true;

//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[VSC] ❌ Connect failed: {ex.Message}");
//                CleanupSocket();
//                try { _ftpClient?.Dispose(); } catch { }
//                _ftpClient = null;
//                return false;
//            }
//        }
//        // Optional: enable FTP (to read program list)
//        //public bool EnableFtp(int ftpPort = 21)
//        //{
//        //    if (_socket == null || !_socket.Connected)
//        //    {
//        //        Console.WriteLine("[VSC] EnableFtp fail: socket chưa connect");
//        //        return false;
//        //    }
//        //    try
//        //    {
//        //        _ftpPort = ftpPort;
//        //        Console.WriteLine($"[VSC FTP] Connecting to {ip}:{_ftpPort}...");

//        //        _ftpClient = new AsyncFtpClient(ip, _ftpPort);
//        //        _ftpClient.Config.ConnectTimeout = timeout;
//        //        _ftpClient.Config.ReadTimeout = timeout;
//        //        _ftpClient.Config.DataConnectionConnectTimeout = timeout;
//        //        _ftpClient.Config.DataConnectionReadTimeout = timeout;
//        //        _ftpClient.Config.SelfConnectMode = FtpSelfConnectMode.Always;
//        //        _ftpClient.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;

//        //        _ftpClient.Connect();
//        //        Console.WriteLine($"[VSC FTP] FTP connected successfully @ {ip}:{_ftpPort}");
//        //        return true;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"[VSC FTP] FTP enable failed: {ex.GetType().Name} - {ex.Message}");
//        //        _ftpClient = null;
//        //        return false;
//        //    }
//        //}
//        public async Task<IReadOnlyList<VscProgramInfo>> GetProgramListAsync(CancellationToken cancellationToken = default)
//        {
//            if (_ftpClient == null)
//                throw new InvalidOperationException("FTP client chưa được khởi tạo. Hãy kết nối trước.");
//            try
//            {
//                var result = new List<VscProgramInfo>();
//                var list = await _ftpClient.GetListing("VS/Camera/Programs", cancellationToken).ConfigureAwait(false);
//                foreach (var item in list)
//                {
//                    var program = ParseProgram(item);
//                    if (program != null)
//                        result.Add(program);
//                }
//                lock (_sync)
//                {
//                    _programCache.Clear();
//                    _programCache.AddRange(result);
//                }
//                return result;
//            }
//            catch (Exception ex)
//            {
//                throw new InvalidOperationException("Lỗi khi đọc danh sách program từ thiết bị.", ex);
//            }
//        }

//        private Bitmap DownloadErrorImage(string imageNumber)
//        {
//            try
//            {
//                string basePath = FtpImagePath;
//                if (string.IsNullOrEmpty(basePath))
//                    basePath = Shared.Settings?.THFtpImagePath
//                        ?? @"D:\HinhAnhLoiDuAnTHTrueMilk\VS\Camera\Images";
//                if (!Directory.Exists(basePath)) return null;
//                string pattern = $"*{imageNumber}*.png";
//                string altPattern = $"*{imageNumber}*.bmp";
//                string[] extensions = { ".png", ".bmp", ".jpg" };
//                // Thử ngay lập tức
//                var result = FindImageFile(basePath, imageNumber, extensions);
//                if (result != null)
//                {
//                    Console.WriteLine($"[VSC] Loaded: {result}");
//                    using (var fs = new FileStream(result, FileMode.Open, FileAccess.Read, FileShare.Read))
//                        return new Bitmap(fs);
//                }
//                // Chưa có → chờ 500ms thử lại 1 lần
//                Thread.Sleep(500);
//                result = FindImageFile(basePath, imageNumber, extensions);
//                if (result != null)
//                {
//                    Console.WriteLine($"[VSC] Loaded (retry): {result}");
//                    using (var fs = new FileStream(result, FileMode.Open, FileAccess.Read, FileShare.Read))
//                        return new Bitmap(fs);
//                }
//                Console.WriteLine($"[VSC] No image '{imageNumber}' found");
//                return null;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[VSC] Load error image failed: {ex.Message}");
//                return null;
//            }
//        }

//        private static string FindImageFile(string basePath, string imageNumber, string[] extensions)
//        {
//            string pattern = $"*{imageNumber}*.*";
//            return Directory.EnumerateFiles(basePath, pattern, SearchOption.AllDirectories)
//                .FirstOrDefault(f =>
//                {
//                    string ext = Path.GetExtension(f);
//                    return ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
//                        || ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
//                        || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase);
//                });
//        }
//        public async Task<VscProgramInfo> ChangeProgramAsync(int programNo, CancellationToken cancellationToken = default)
//        {
//            if (_socket == null || !_socket.Connected)
//                throw new InvalidOperationException("Chưa kết nối camera VS.");

//            // VS-C dùng PL,1,{programNo} để đổi program.
//            // Sau khi đổi xong, luôn hỏi lại PR để xác nhận program thật trên camera.
//            if (_protocol == ProtocolMode.Framed)
//            {
//                SendRawCommandFramed("RM");
//                await Task.Delay(200, cancellationToken).ConfigureAwait(false);

//                var response = await Task.Run(() =>
//                    SendAndReceiveFramed($"PL,1,{programNo:D4}", timeout),
//                    cancellationToken).ConfigureAwait(false);

//                if (!string.IsNullOrEmpty(response) && response.StartsWith("ER", StringComparison.OrdinalIgnoreCase))
//                    throw new InvalidOperationException("Thiết bị trả lỗi khi đổi program: " + response);
//            }
//            else
//            {
//                SetRunMode();
//                await Task.Delay(100, cancellationToken).ConfigureAwait(false);

//                if (!SendCommand($"PL,1,{programNo:D4}"))
//                    throw new InvalidOperationException("Send PL command failed");

//                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
//            }

//            var currentProgram = await GetCurrentProgramAsync(cancellationToken).ConfigureAwait(false);
//            if (currentProgram == null)
//            {
//                _currentProgramNo = programNo;
//                currentProgram = new VscProgramInfo { ProgramNo = programNo };
//            }
//            else
//            {
//                _currentProgramNo = currentProgram.ProgramNo;
//            }

//            if (_currentProgramNo != programNo)
//            {
//                Console.WriteLine($"[VSC] ⚠ Change program requested {programNo:D4} but camera PR returned {_currentProgramNo:D4}");
//            }

//            CurrentProgramChanged?.Invoke(currentProgram);
//            return currentProgram;
//        }

//        public async Task<VscProgramInfo> GetCurrentProgramAsync(CancellationToken cancellationToken = default)
//        {
//            if (_socket == null || !_socket.Connected)
//                throw new InvalidOperationException("Chưa kết nối camera VS.");

//            // Nếu đang có listen loop thì không đọc trực tiếp socket ở đây,
//            // vì sẽ tranh socket với TextListenLoop/FramedListenLoop.
//            // Thay vào đó gửi PR và để DispatchFrame xử lý response.
//            if (_isListening)
//            {
//                var tcs = new TaskCompletionSource<VscProgramInfo>(TaskCreationOptions.RunContinuationsAsynchronously);

//                lock (_sync)
//                {
//                    _currentProgramTcs?.TrySetCanceled();
//                    _currentProgramTcs = tcs;
//                    _commandFlag = "PR";
//                }

//                CancellationTokenRegistration reg = default;
//                if (cancellationToken.CanBeCanceled)
//                    reg = cancellationToken.Register(() => tcs.TrySetCanceled());

//                try
//                {
//                    bool sendOk = _protocol == ProtocolMode.Framed
//                        ? SendRawCommandFramed("PR")
//                        : SendCommand("PR");

//                    if (!sendOk)
//                        throw new InvalidOperationException("Send PR command failed");

//                    var timeoutTask = Task.Delay(Math.Max(timeout, 3000), cancellationToken);
//                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);
//                    if (completedTask != tcs.Task)
//                    {
//                        lock (_sync)
//                        {
//                            if (_currentProgramTcs == tcs)
//                            {
//                                _currentProgramTcs = null;
//                                _commandFlag = string.Empty;
//                            }
//                        }

//                        throw new TimeoutException("Timeout khi chờ phản hồi PR từ camera VS.");
//                    }

//                    return await tcs.Task.ConfigureAwait(false);
//                }
//                finally
//                {
//                    reg.Dispose();
//                }
//            }

//            return await Task.Run(() => QueryCurrentProgramDirect(timeout), cancellationToken).ConfigureAwait(false);
//        }

//        // ══════════════════════════════════════════
//        // START LISTENING
//        // ══════════════════════════════════════════
//        public bool StartListening()
//        {
//            if (_socket == null || !_socket.Connected) return false;

//            _cts = new CancellationTokenSource();
//            _isListening = true;

//            if (_protocol == ProtocolMode.Framed)
//            {
//                Task.Run(() => FramedListenLoop(_cts.Token), _cts.Token);
//            }
//            else
//            {
//                Task.Run(() => TextListenLoop(_cts.Token), _cts.Token);
//            }
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
//            _isListening = false;

//            // Auto-reconnect nếu không phải do Cancel chủ động
//            if (!token.IsCancellationRequested)
//            {
//                Console.WriteLine("[VSC] Mất kết nối ngoài ý muốn → reconnect sau 2s...");
//                Thread.Sleep(2000);
//                Reconnect();
//            }
//        }
//        private void FramedListenLoop(CancellationToken token)
//        {
//            const byte STX = 0x02;
//            const byte ETX = 0x03;
//            var buffer = new List<byte>();

//            while (!token.IsCancellationRequested)
//            {
//                if (_socket == null || !_socket.Connected) break;

//                try
//                {
//                    if (!_socket.Poll(50_000, SelectMode.SelectRead))
//                    {
//                        Thread.Sleep(1);
//                        continue;
//                    }

//                    int n = _socket.Receive(_textBuf);
//                    if (n <= 0) break;

//                    buffer.AddRange(_textBuf.Take(n));

//                    // Tìm STX...ETX frames
//                    while (true)
//                    {
//                        int stxIdx = buffer.IndexOf(STX);
//                        if (stxIdx < 0) { buffer.Clear(); break; }

//                        int etxIdx = buffer.IndexOf(ETX, stxIdx + 1);
//                        if (etxIdx < 0) break;  // Chưa có ETX, chờ thêm data

//                        int len = etxIdx - stxIdx - 1;
//                        if (len > 0)
//                        {
//                            var frameBytes = buffer.GetRange(stxIdx + 1, len).ToArray();
//                            string frame = Encoding.ASCII.GetString(frameBytes).Trim('\r', '\n');
//                            if (!string.IsNullOrEmpty(frame))
//                            {
//                                Console.WriteLine($"[VSC] Received frame: '{frame}'");
//                                DispatchFrame(frame);
//                            }
//                        }

//                        buffer.RemoveRange(0, etxIdx + 1);
//                    }
//                }
//                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
//                {
//                    continue;
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"[VSC] FramedListenLoop error: {ex.Message}");
//                    break;
//                }
//            }

//            _isListening = false;
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
//            if (TryHandleProgramCommandFrame(frame))
//                return;

//            if (string.Equals(frame.Trim(), "TRG", StringComparison.OrdinalIgnoreCase))
//            {
//                Console.WriteLine("[VSC] Ignoring echo frame: TRG");
//                return;
//            }
//            string cleanedFrame = frame.StartsWith("TRG", StringComparison.OrdinalIgnoreCase)
//                ? frame.Substring(3)
//                : frame;
//            string[] parts = cleanedFrame.Split(',');

//            // Format: QR,QR_Result,NSX,Time,HSD,Batch,OCR_Result,Image_ID
//            string qr = parts.Length > 0 ? parts[0].Trim() : "";
//            string qrResult = parts.Length > 1 ? parts[1].Trim() : "";
//            string nsx = parts.Length > 2 ? parts[2].Trim() : "";
//            string time = parts.Length > 3 ? parts[3].Trim() : "";
//            string hsd = parts.Length > 4 ? parts[4].Trim() : "";
//            string batch = parts.Length > 5 ? parts[5].Trim() : "";
//            string ocrResult = parts.Length > 6 ? parts[6].Trim() : "";
//            string imageId = parts.Length > 7 ? parts[7].Trim() : "";

//            bool qrPass = IsResultOk(qrResult);
//            bool ocrPass = IsResultOk(ocrResult);

//            var detect = new DetectModel
//            {
//                Text = qr,
//                Image = null,
//                CompareResult = (qrPass && ocrPass)
//                    ? ComparisonResult.Valid
//                    : ComparisonResult.Invalided,
//            };
//            detect.ExtraFields["QR_RESULT"] = qrPass ? "OK" : "NG";
//            detect.ExtraFields["OCR_RESULT"] = ocrPass ? "OK" : "NG";
//            detect.ExtraFields["OCR_STATUS"] = ocrPass ? "OK" : "NG";
//            detect.ExtraFields["NSX"] = StripPrefix(nsx, "NSX:");
//            detect.ExtraFields["TIME"] = time;
//            detect.ExtraFields["HSD"] = StripPrefix(hsd, "HSD:");
//            detect.ExtraFields["BATCH"] = batch;

//            detect.ExtraFields["RAW_FRAME"] = cleanedFrame;
//            detect.ExtraFields["RECEIVE_TIME"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

//            // Không download ngay, lưu imageId vào ExtraFields để Compare thread xử lý
//            if (!string.IsNullOrEmpty(imageId))
//                detect.ExtraFields["IMAGE_ID"] = imageId;

//            Console.WriteLine($"[VSC] Frame: QR='{qr}' QR_Result={qrPass} OCR_Result={ocrPass} ImageID='{imageId}'");
//            OnCameraFrameReceived?.Invoke(cleanedFrame);
//            Shared.RaiseOnCameraReadDataChangeEvent(detect);
//        }

//        private static bool IsResultOk(string val)
//        {
//            return val.Equals("OK", StringComparison.OrdinalIgnoreCase)
//                || val == "1"
//                || val.Equals("PASS", StringComparison.OrdinalIgnoreCase)
//                || val.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
//        }

//        private static string StripPrefix(string value, string prefix)
//        {
//            if (string.IsNullOrEmpty(value)) return value;
//            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
//                return value.Substring(prefix.Length).Trim();
//            return value.Trim();
//        }

//        private bool TryHandleProgramCommandFrame(string frame)
//        {
//            string processingData = (frame ?? string.Empty).Trim();
//            if (string.IsNullOrEmpty(processingData))
//                return false;

//            string flag;
//            TaskCompletionSource<VscProgramInfo> tcs;
//            lock (_sync)
//            {
//                flag = _commandFlag;
//                tcs = _currentProgramTcs;
//            }

//            // Không để các phản hồi command bị parse nhầm thành frame QR/OCR.
//            bool looksLikeProgramReply =
//                processingData.StartsWith("PR", StringComparison.OrdinalIgnoreCase) ||
//                processingData.StartsWith("PL", StringComparison.OrdinalIgnoreCase) ||
//                processingData.StartsWith("ER", StringComparison.OrdinalIgnoreCase);

//            if (string.IsNullOrEmpty(flag))
//            {
//                if (processingData.StartsWith("PR", StringComparison.OrdinalIgnoreCase))
//                {
//                    var program = UpdateCurrentProgramFromReply(processingData);
//                    if (program != null)
//                    {
//                        _currentProgramNo = program.ProgramNo;
//                        CurrentProgramChanged?.Invoke(program);
//                    }
//                    return true;
//                }

//                return looksLikeProgramReply;
//            }

//            if (!processingData.StartsWith(flag, StringComparison.OrdinalIgnoreCase) &&
//                !processingData.StartsWith("ER", StringComparison.OrdinalIgnoreCase))
//            {
//                return false;
//            }

//            try
//            {
//                if (processingData.StartsWith("ER", StringComparison.OrdinalIgnoreCase))
//                {
//                    tcs?.TrySetException(new InvalidOperationException("Lỗi VS: " + processingData));
//                    return true;
//                }

//                if (string.Equals(flag, "PR", StringComparison.OrdinalIgnoreCase))
//                {
//                    var program = UpdateCurrentProgramFromReply(processingData);
//                    if (program != null)
//                    {
//                        _currentProgramNo = program.ProgramNo;
//                        tcs?.TrySetResult(program);
//                        CurrentProgramChanged?.Invoke(program);
//                    }
//                    else
//                    {
//                        tcs?.TrySetResult(null);
//                    }
//                    return true;
//                }

//                return true;
//            }
//            finally
//            {
//                lock (_sync)
//                {
//                    if (_currentProgramTcs == tcs)
//                    {
//                        _currentProgramTcs = null;
//                        _commandFlag = string.Empty;
//                    }
//                }
//            }
//        }

//        private VscProgramInfo QueryCurrentProgramDirect(int waitMs)
//        {
//            string response;

//            if (_protocol == ProtocolMode.Framed)
//                response = SendAndReceiveFramed("PR", waitMs);
//            else
//                response = SendAndReceiveLine("PR", waitMs, "PR");

//            if (string.IsNullOrWhiteSpace(response))
//                return null;

//            if (response.StartsWith("ER", StringComparison.OrdinalIgnoreCase))
//                throw new InvalidOperationException("Lỗi VS khi hỏi PR: " + response);

//            var program = UpdateCurrentProgramFromReply(response);
//            if (program != null)
//            {
//                _currentProgramNo = program.ProgramNo;
//                CurrentProgramChanged?.Invoke(program);
//            }

//            return program;
//        }

//        private VscProgramInfo UpdateCurrentProgramFromReply(string replyString)
//        {
//            if (string.IsNullOrWhiteSpace(replyString))
//                return null;

//            int idx = replyString.IndexOf("PR", StringComparison.OrdinalIgnoreCase);
//            if (idx < 0)
//                return null;

//            string payload = replyString.Substring(idx).Trim();
//            string[] parts = payload.Split(',');

//            int programNo = -1;
//            for (int i = parts.Length - 1; i >= 1; i--)
//            {
//                string token = (parts[i] ?? string.Empty).Trim();
//                if (int.TryParse(token, out programNo))
//                    break;
//            }

//            if (programNo < 0)
//                throw new InvalidOperationException("Không parse được ProgramNo từ phản hồi PR: " + replyString);

//            VscProgramInfo found = null;
//            lock (_sync)
//            {
//                found = _programCache.FirstOrDefault(p => p.ProgramNo == programNo);
//            }

//            if (found != null)
//            {
//                return new VscProgramInfo
//                {
//                    ProgramNo = found.ProgramNo,
//                    Name = found.Name,
//                    Location = found.Location,
//                    ModifyInfo = found.ModifyInfo
//                };
//            }

//            return new VscProgramInfo { ProgramNo = programNo, Location = "Local" };
//        }

//        private string SendAndReceiveLine(string command, int waitMs, string expectedPrefix = null)
//        {
//            if (_socket == null || !_socket.Connected)
//                return null;

//            if (!SendCommand(command))
//                return null;

//            var buffer = new StringBuilder();
//            var sw = Stopwatch.StartNew();
//            int originalTimeout = 0;
//            try { originalTimeout = _socket.ReceiveTimeout; } catch { }

//            try
//            {
//                try { _socket.ReceiveTimeout = 200; } catch { }

//                while (sw.ElapsedMilliseconds < waitMs)
//                {
//                    try
//                    {
//                        if (_socket.Available == 0)
//                        {
//                            Thread.Sleep(10);
//                            continue;
//                        }

//                        int toRead = Math.Min(_textBuf.Length, Math.Max(_socket.Available, 1));
//                        int read = _socket.Receive(_textBuf, 0, toRead, SocketFlags.None);
//                        if (read <= 0)
//                            break;

//                        buffer.Append(Encoding.ASCII.GetString(_textBuf, 0, read));

//                        string line;
//                        while (TryTakeBufferedLine(buffer, out line))
//                        {
//                            if (string.IsNullOrWhiteSpace(line))
//                                continue;

//                            string trimmed = line.Trim();
//                            if (string.IsNullOrEmpty(expectedPrefix) ||
//                                trimmed.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase) ||
//                                trimmed.StartsWith("ER", StringComparison.OrdinalIgnoreCase))
//                            {
//                                return trimmed;
//                            }
//                        }
//                    }
//                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
//                    {
//                        continue;
//                    }
//                }

//                return null;
//            }
//            finally
//            {
//                try { _socket.ReceiveTimeout = originalTimeout; } catch { }
//            }
//        }

//        private static bool TryTakeBufferedLine(StringBuilder buffer, out string line)
//        {
//            line = null;
//            if (buffer == null || buffer.Length == 0)
//                return false;

//            int endIdx = -1;
//            for (int i = 0; i < buffer.Length; i++)
//            {
//                if (buffer[i] == '\r' || buffer[i] == '\n')
//                {
//                    endIdx = i;
//                    break;
//                }
//            }

//            if (endIdx < 0)
//                return false;

//            line = buffer.ToString(0, endIdx).Trim();

//            int removeCount = endIdx + 1;
//            if (endIdx + 1 < buffer.Length && buffer[endIdx] == '\r' && buffer[endIdx + 1] == '\n')
//                removeCount++;

//            buffer.Remove(0, removeCount);
//            return true;
//        }

//        // ══════════════════════════════════════════
//        // SEND COMMAND (trigger, đổi program, v.v.) — line mode
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
//        public bool ChangeProgram(int no) => SendCommand($"PL,1,{no:D4}");

//        // ══════════════════════════════════════════
//        // FRAMED (STX/ETX) helpers (similar to KeyenceVS)
//        // ══════════════════════════════════════════
//        public bool SendRawCommandFramed(string command)
//        {
//            if (_socket == null || !_socket.Connected)
//            {
//                Console.WriteLine("[VSC] SendRawCommandFramed fail: socket not connected");
//                return false;
//            }

//            var payload = new byte[command.Length + 2];
//            payload[0] = 0x02;
//            Encoding.ASCII.GetBytes(command, 0, command.Length, payload, 1);
//            payload[payload.Length - 1] = 0x03;

//            int sent = 0;
//            while (sent < payload.Length)
//            {
//                int s = _socket.Send(payload, sent, payload.Length - sent, SocketFlags.None);
//                if (s <= 0)
//                    throw new SocketException((int)SocketError.ConnectionReset);
//                sent += s;
//            }

//            return true;
//        }

//        public string SendAndReceiveFramed(string command, int waitMs = 3000)
//        {
//            if (!SendRawCommandFramed(command))
//                return null;

//            var frames = ReadAllFrames(waitMs);
//            if (frames == null || frames.Count == 0)
//                return null;

//            foreach (var frame in frames)
//            {
//                var text = (frame ?? string.Empty).Trim();
//                if (!string.IsNullOrEmpty(text))
//                    return text;
//            }

//            return null;
//        }

//        public VSMeasurementResult TriggerFramed()
//        {
//            var result = new VSMeasurementResult { Code = VSResponseCode.OK };

//            if (_socket == null || !_socket.Connected)
//            {
//                result.Code = VSResponseCode.ConnectionError;
//                Console.WriteLine("[VSC] TriggerFramed: socket not connected");
//                return result;
//            }

//            try
//            {
//                SendRawCommandFramed("TRG");
//            }
//            catch (Exception ex)
//            {
//                result.Code = VSResponseCode.ConnectionError;
//                Console.WriteLine("[VSC] TriggerFramed send error: " + ex.Message);
//                return result;
//            }

//            var frames = ReadAllFrames(Math.Max(timeout, 5000));
//            if (frames == null || frames.Count == 0)
//            {
//                result.Code = VSResponseCode.Timeout;
//                return result;
//            }

//            string dataFrame = null;
//            foreach (var frame in frames)
//            {
//                var trimmed = (frame ?? string.Empty).Trim();
//                if (!string.IsNullOrEmpty(trimmed) && !string.Equals(trimmed, "TRG", StringComparison.OrdinalIgnoreCase))
//                {
//                    dataFrame = trimmed;
//                    break;
//                }
//            }

//            if (dataFrame == null)
//            {
//                var moreFrames = ReadAllFrames(Math.Max(timeout, 5000));
//                foreach (var frame in moreFrames)
//                {
//                    var trimmed = (frame ?? string.Empty).Trim();
//                    if (!string.IsNullOrEmpty(trimmed) && !string.Equals(trimmed, "TRG", StringComparison.OrdinalIgnoreCase))
//                    {
//                        dataFrame = trimmed;
//                        break;
//                    }
//                }
//            }

//            if (dataFrame == null)
//            {
//                result.Code = VSResponseCode.Timeout;
//                return result;
//            }

//            result.RawResponse = dataFrame;

//            string cleaned = dataFrame.StartsWith("TRG", StringComparison.OrdinalIgnoreCase)
//                ? dataFrame.Substring(3).TrimStart(',').Trim()
//                : dataFrame.Trim();

//            var tokens = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//            var data = new List<string>();
//            bool firstToken = true;

//            foreach (var token in tokens)
//            {
//                var t = token.Trim();
//                if (string.IsNullOrEmpty(t))
//                    continue;

//                if (firstToken)
//                {
//                    firstToken = false;
//                    if (t == "0" || t == "1")
//                    {
//                        result.Judgment = t == "1";
//                        continue;
//                    }
//                }

//                data.Add(t);
//            }

//            result.StringData = data.ToArray();
//            result.Code = VSResponseCode.OK;
//            return result;
//        }

//        public VSImageResult ReceiveImageFramed(int waitMs = 5000, string savePath = null)
//        {
//            var result = new VSImageResult { Code = VSResponseCode.OK };
//            var raw = ReceiveFramedResponse(waitMs);
//            result.RawResponse = raw;

//            if (string.IsNullOrEmpty(raw))
//            {
//                result.Code = VSResponseCode.Timeout;
//                return result;
//            }

//            var payload = raw.Trim();
//            if (payload.StartsWith("TRG", StringComparison.OrdinalIgnoreCase))
//                payload = payload.Substring(3).Trim();

//            int comma = payload.IndexOf(',');
//            if (comma <= 0 || comma >= payload.Length - 1)
//            {
//                result.Code = VSResponseCode.ParseError;
//                return result;
//            }

//            string header = payload.Substring(0, comma).Trim();
//            string data = payload.Substring(comma + 1).Trim();

//            try
//            {
//                if (header.IndexOf("JPEG", StringComparison.OrdinalIgnoreCase) >= 0 || header.IndexOf("JPG", StringComparison.OrdinalIgnoreCase) >= 0)
//                    result.ContentType = "image/jpeg";
//                else if (header.IndexOf("PNG", StringComparison.OrdinalIgnoreCase) >= 0)
//                    result.ContentType = "image/png";
//                else
//                    result.ContentType = "application/octet-stream";

//                result.ImageBytes = Convert.FromBase64String(data);
//                using (var ms = new MemoryStream(result.ImageBytes))
//                using (var temp = Image.FromStream(ms))
//                {
//                    result.Image = new Bitmap(temp);
//                }

//                if (!string.IsNullOrWhiteSpace(savePath))
//                {
//                    File.WriteAllBytes(savePath, result.ImageBytes);
//                    result.SavePath = savePath;
//                }
//            }
//            catch (Exception ex)
//            {
//                result.Code = VSResponseCode.ParseError;
//                Console.WriteLine("[VSC] ReceiveImageFramed decode error: " + ex.Message);
//            }

//            return result;
//        }

//        // Framed read helpers (STX/ETX)
//        private List<string> ReadAllFrames(int waitMs)
//        {
//            const byte STX = 0x02;
//            const byte ETX = 0x03;

//            var result = new List<string>();
//            var stream = new MemoryStream();
//            var sw = Stopwatch.StartNew();
//            int originalTimeout = 0;
//            try { originalTimeout = _socket.ReceiveTimeout; } catch { }

//            try
//            {
//                try { _socket.ReceiveTimeout = 200; } catch { }

//                while (sw.ElapsedMilliseconds < waitMs)
//                {
//                    try
//                    {
//                        if (_socket.Available == 0)
//                        {
//                            if (HasNonEchoFrame(result))
//                                break;

//                            Thread.Sleep(10);
//                            continue;
//                        }

//                        int toRead = Math.Min(_recvBuffer.Length, Math.Max(_socket.Available, 1));
//                        int read = _socket.Receive(_recvBuffer, 0, toRead, SocketFlags.None);
//                        if (read <= 0)
//                            break;

//                        stream.Write(_recvBuffer, 0, read);
//                        ExtractFrames(stream.ToArray(), result, STX, ETX);

//                        if (HasNonEchoFrame(result))
//                            break;
//                    }
//                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
//                    {
//                        continue;
//                    }
//                }

//                return result;
//            }
//            finally
//            {
//                try { _socket.ReceiveTimeout = originalTimeout; } catch { }
//            }
//        }

//        private string ReceiveFramedResponse(int waitMs)
//        {
//            if (_socket == null || !_socket.Connected)
//                return null;

//            const byte STX = 0x02;
//            const byte ETX = 0x03;
//            var stream = new MemoryStream();
//            var sw = Stopwatch.StartNew();
//            int originalTimeout = 0;
//            try { originalTimeout = _socket.ReceiveTimeout; } catch { }

//            try
//            {
//                try { _socket.ReceiveTimeout = 200; } catch { }

//                while (sw.ElapsedMilliseconds < waitMs)
//                {
//                    try
//                    {
//                        if (_socket.Available == 0)
//                        {
//                            Thread.Sleep(10);
//                            continue;
//                        }

//                        int toRead = Math.Min(_recvBuffer.Length, Math.Max(_socket.Available, 1));
//                        int read = _socket.Receive(_recvBuffer, 0, toRead, SocketFlags.None);
//                        if (read <= 0)
//                            break;

//                        stream.Write(_recvBuffer, 0, read);
//                        var full = stream.ToArray();

//                        int etxIndex = Array.IndexOf(full, ETX);
//                        if (etxIndex < 0)
//                            continue;

//                        int stxIndex = Array.IndexOf(full, STX);
//                        int start = stxIndex >= 0 ? stxIndex + 1 : 0;
//                        int count = Math.Max(0, etxIndex - start);
//                        var content = Encoding.ASCII.GetString(full, start, count).Trim('\r', '\n');

//                        return content;
//                    }
//                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
//                    {
//                        continue;
//                    }
//                }

//                if (stream.Length <= 0)
//                    return null;

//                var raw = stream.ToArray();
//                int stxIndex2 = Array.IndexOf(raw, STX);
//                int etxIndex2 = Array.IndexOf(raw, ETX);
//                int start2 = stxIndex2 >= 0 ? stxIndex2 + 1 : 0;
//                int count2 = etxIndex2 > start2 ? etxIndex2 - start2 : raw.Length - start2;
//                var partial = Encoding.ASCII.GetString(raw, start2, Math.Max(0, count2)).Trim('\r', '\n');
//                return partial;
//            }
//            finally
//            {
//                try { _socket.ReceiveTimeout = originalTimeout; } catch { }
//            }
//        }

//        private static void ExtractFrames(byte[] raw, List<string> result, byte stx, byte etx)
//        {
//            int pos = 0;
//            while (pos < raw.Length)
//            {
//                int stxPos = Array.IndexOf(raw, stx, pos);
//                if (stxPos < 0)
//                    break;

//                int etxPos = Array.IndexOf(raw, etx, stxPos + 1);
//                if (etxPos < 0)
//                    break;

//                int contentLen = etxPos - stxPos - 1;
//                var content = contentLen > 0
//                    ? Encoding.ASCII.GetString(raw, stxPos + 1, contentLen).Trim('\r', '\n')
//                    : string.Empty;

//                result.Add(content);
//                pos = etxPos + 1;
//            }
//        }

//        private static bool HasNonEchoFrame(IEnumerable<string> frames)
//        {
//            foreach (var frame in frames)
//            {
//                if (!string.Equals((frame ?? string.Empty).Trim(), "TRG", StringComparison.OrdinalIgnoreCase))
//                    return true;
//            }

//            return false;
//        }

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
//            Thread.Sleep(100);
//            CleanupSocket();
//            try
//            {
//                if (_ftpClient != null)
//                {
//                    _ftpClient.Disconnect();
//                    _ftpClient.Dispose();
//                }
//            }
//            catch { }
//            _ftpClient = null;

//            lock (_sync)
//            {
//                _commandFlag = string.Empty;
//                _currentProgramTcs?.TrySetCanceled();
//                _currentProgramTcs = null;
//            }
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
//            _isListening = false;
//            Console.WriteLine("[VSC] StopListening called");
//        }

//        // Program parse reused from KeyenceVS
//        private static VscProgramInfo ParseProgram(FluentFTP.FtpListItem item)
//        {
//            if (item == null || item.Type != FluentFTP.FtpObjectType.Directory)
//                return null;

//            var name = item.Name;
//            if (string.IsNullOrWhiteSpace(name) || name.Length < 5)
//                return null;

//            string sProgramNo = name.Substring(0, 4);
//            string sProgramName = name.Substring(5);

//            if (!int.TryParse(sProgramNo, out int programNo))
//                return null;

//            return new VscProgramInfo
//            {
//                ProgramNo = programNo,
//                Name = sProgramName,
//                Location = "Local",
//                ModifyInfo = "Modified : " + item.Modified.ToString("yy/MM/dd - HH:mm:ss")
//            };
//        }
//    }

//    // Minimal result types reused for framed helpers
//    public enum VSResponseCode
//    {
//        OK = 0,
//        Timeout = 99,
//        ConnectionError = 100,
//        ParseError = 101
//    }

//    public class VSMeasurementResult
//    {
//        public VSResponseCode Code { get; set; }
//        public bool Judgment { get; set; }
//        public string[] StringData { get; set; } = Array.Empty<string>();
//        public string RawResponse { get; set; }
//    }

//    public class VSImageResult
//    {
//        public VSResponseCode Code { get; set; }
//        public string RawResponse { get; set; }
//        public byte[] ImageBytes { get; set; }
//        public Image Image { get; set; }
//        public string ContentType { get; set; }
//        public string SavePath { get; set; }
//        public bool HasImage => ImageBytes != null && ImageBytes.Length > 0;
//    }
//}