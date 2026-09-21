using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//using System.Web.Script.Serialization;
using BarcodeVerificationSystem.Services.THMilk.Controller;
using BarcodeVerificationSystem.Services.THMilk.Model.ReceiveModel;
using BarcodeVerificationSystem.Services.THMilk.Model.ResponseModel;
using Newtonsoft.Json;

namespace BarcodeVerificationSystem.Services.THMilk
{
    public class THMilkAPIHandler
    {
        private ServerService _serverService;
        private LoginController _loginController;
        private RefreshTokenController _refreshTokenController;
        private CodeController _codeController;

        // Events to notify application about received data
        public event EventHandler<QRCodeReceivedEventArgs> QRCodeReceived;
        public event EventHandler<APIRequestEventArgs> APIRequestReceived;

        public THMilkAPIHandler(string prefix = "http://*:5001/")
        {
            _serverService = new ServerService(prefix);
            _loginController = new LoginController();
            _refreshTokenController = new RefreshTokenController();
            _codeController = new CodeController();

            // Subscribe to code controller events
            _codeController.QRCodeReceived += (sender, e) =>
            {
                QRCodeReceived?.Invoke(this, e);
            };

            // Subscribe to server events - use CustomRequestHandler to send responses
            _serverService.CustomRequestHandler += OnCustomRequest;
            _serverService.RequestArrived += OnRequestArrived;

            _serverService.Start();
        }

        private void OnRequestArrived(object sender, RequestInfoEventArgs e)
        {
            Console.WriteLine($"[{e.Timestamp:HH:mm:ss}] {e.Method} {e.Path} from {e.ClientIP}");
            
            APIRequestReceived?.Invoke(this, new APIRequestEventArgs
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
                Console.WriteLine($"══════════════════════════════════════════");
                Console.WriteLine($"Received Request: {request.HttpMethod} {request.Url.AbsolutePath}");
                Console.WriteLine($"Content-Type: {request.ContentType}");
                Console.WriteLine($"Request Body (from ServerService): {body ?? "(null)"}");
                Console.WriteLine($"Request Body Length: {body?.Length ?? 0}");
                Console.WriteLine($"══════════════════════════════════════════");
                
                // Handle CORS preflight
                if (request.HttpMethod == "OPTIONS")
                {
                    SendJson(response, "{\"allow\":\"GET,POST,OPTIONS\"}", 200);
                    return;
                }

                // Route to appropriate controller
                string path = request.Url.AbsolutePath?.TrimEnd('/').ToLower();
                
                switch (path)
                {
                    case "/api/login":
                        _loginController.HandleLogin(request, response, body);
                        break;

                    case "/api/refresh-token":
                        _refreshTokenController.HandleRefreshToken(request, response, body);
                        break;

                    case "/api/code":
                        _codeController.HandleCodeData(request, response, body);
                        break;

                    default:
                        Console.WriteLine($"Unknown endpoint: {request.Url.AbsolutePath}");
                        SendJson(response, JsonConvert.SerializeObject(new { error = "Unknown endpoint" }), 404);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (body != null)
                {
                    Console.WriteLine($"Failed Body was: {body}");
                }
                SendJson(response, JsonConvert.SerializeObject(new { error = "Internal server error", message = ex.Message }), 500);
            }
        }

        // Simple handlers are removed - controllers now handle responses directly
        
        private static void SendJson(HttpListenerResponse response, string json, int status = 200)
        {
            byte[] data = Encoding.UTF8.GetBytes(json ?? "{}");
            response.StatusCode = status;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = data.Length;
            
            // Add CORS headers
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");
            
            using (var s = response.OutputStream)
            {
                s.Write(data, 0, data.Length);
            }
        }

        public void Start()
        {
            _serverService.Start();
            Console.WriteLine($"TH Milk API Handler started on {_serverService.Prefix}");
        }

        public void Stop()
        {
            _serverService.Stop();
            Console.WriteLine("TH Milk API Handler stopped");
        }

        public bool IsRunning => _serverService.IsRunning;
    }

    public class APIRequestEventArgs : EventArgs
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string ClientIP { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
