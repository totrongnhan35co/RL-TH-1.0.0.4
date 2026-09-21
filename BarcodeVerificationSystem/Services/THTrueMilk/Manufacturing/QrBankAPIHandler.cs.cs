using System;
using System.Net;
using System.Text;
using BarcodeVerificationSystem.Services.THMilk;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Controller;
using Newtonsoft.Json;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing
{
    /// <summary>
    /// Entry point QrBank HTTP server.
    /// Routes:
    ///   POST /api/qrbank/login
    ///   POST /api/qrbank/refresh-token
    ///   POST /api/qrbank/data
    ///   GET  /api/qrbank/ping
    ///   POST /api/qrbank/monitor
    /// </summary>
    public class QrBankAPIHandler : IDisposable
    {
        private readonly ServerService _server;
        private readonly QrBankLoginController _loginController;
        private readonly QrBankRefreshTokenController _refreshController;
        private readonly QrBankDataController _dataController;

        public event EventHandler<QrBankDataReceivedEventArgs> DataSaved;
        public event EventHandler<QrBankRequestEventArgs> RequestArrived;

        public QrBankAPIHandler(string prefix = "http://*:5002/")
        {
            _server = new ServerService(prefix);
            _loginController = new QrBankLoginController();
            _refreshController = new QrBankRefreshTokenController();
            _dataController = new QrBankDataController();

            _dataController.DataReceived += (s, e) => DataSaved?.Invoke(this, e);

            _server.RequestArrived += OnRequestArrived;
            _server.CustomRequestHandler += OnCustomRequest;

            _server.Start();
        }

        private void OnRequestArrived(object sender, RequestInfoEventArgs e)
        {
            Console.WriteLine($"[QrBankAPIHandler] {e.Timestamp:HH:mm:ss} {e.Method} {e.Path} from {e.ClientIP}");
            RequestArrived?.Invoke(this, new QrBankRequestEventArgs
            {
                Method = e.Method,
                Path = e.Path,
                ClientIP = e.ClientIP,
                Timestamp = e.Timestamp
            });
        }

        private void OnCustomRequest(HttpListenerRequest request, HttpListenerResponse response, string body)
        {
            try
            {
                if (request.HttpMethod == "OPTIONS")
                {
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    response.AddHeader("Access-Control-Allow-Methods", "GET,POST,OPTIONS");
                    response.AddHeader("Access-Control-Allow-Headers", "Content-Type,Authorization");
                    SendJson(response, "{}", 200);
                    return;
                }

                string path = request.Url.AbsolutePath.TrimEnd('/').ToLower();

                switch (path)
                {
                    case "/api/qrbank/login":
                        _loginController.HandleLogin(request, response, body);
                        break;
                    case "/api/qrbank/refresh-token":
                        _refreshController.HandleRefreshToken(request, response, body);
                        break;
                    case "/api/qrbank/data":
                        //Console.WriteLine($"[QrBankAPI] ═══ POST /api/qrbank/data ═══");
                        //Console.WriteLine($"[QrBankAPI] Body length: {body?.Length ?? 0} bytes");
                        if (!string.IsNullOrEmpty(body))
                        {
                            string preview = body.Length <= 300 ? body : body.Substring(0, 300) + "...";
                            Console.WriteLine($"[QrBankAPI] Body preview: {preview}");
                        }
                        _dataController.HandleData(request, response, body);
                        break;
                    case "/api/qrbank/ping":
                        _dataController.HandlePing(request, response);
                        break;
                    case "/api/qrbank/monitor":
                        _dataController.HandleMonitor(request, response);
                        break;
                    default:
                        SendJson(response,
                            JsonConvert.SerializeObject(new { error = "Unknown endpoint", path }),
                            404);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QrBankAPIHandler] ✗ {ex.Message}");
                SendJson(response,
                    JsonConvert.SerializeObject(new { error = "Internal server error", message = ex.Message }),
                    500);
            }
        }

        private static void SendJson(HttpListenerResponse resp, string json, int status)
        {
            byte[] data = Encoding.UTF8.GetBytes(json ?? "{}");
            resp.StatusCode = status;
            resp.ContentType = "application/json; charset=utf-8";
            resp.ContentLength64 = data.Length;
            resp.AddHeader("Access-Control-Allow-Origin", "*");
            using (var s = resp.OutputStream)
                s.Write(data, 0, data.Length);
        }

        public void Start() => _server.Start();
        public void Stop() => _server.Stop();
        public bool IsRunning => _server.IsRunning;

        public void Dispose()
        {
            _server?.Stop();
            _server?.Dispose();
        }
    }

    // ── Định nghĩa DUY NHẤT tại đây ─────────────────────────────────────
    public class QrBankDataReceivedEventArgs : EventArgs
    {
        public string QrCode { get; set; }
        public string FactoryCode { get; set; }
        public string LineId { get; set; }
        public string LineName { get; set; }
        public string Batch { get; set; }
        public string AuthenticatedUser { get; set; }
        public DateTime ReceivedAt { get; set; }
        public int TotalCount { get; set; }
        public string FirstQr { get; set; }
        public string LastQr { get; set; }
        public string JobName { get; set; }
        public string Sender { get; set; }
    }

    public class QrBankRequestEventArgs : EventArgs
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string ClientIP { get; set; }
        public DateTime Timestamp { get; set; }
    }
}