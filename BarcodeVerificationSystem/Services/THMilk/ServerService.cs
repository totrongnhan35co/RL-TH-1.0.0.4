using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THMilk
{
    public class ServerService : IDisposable
    {
        private HttpListener _listener;
        private Thread _serverThread;
        private volatile bool _isRunning;
        private readonly string _prefix;

        // ────────────────────────────────────────────────
        // Events that other classes can subscribe to
        // ────────────────────────────────────────────────

        /// <summary>
        /// Fired when any client sends data (POST body, or GET with query, etc.)
        /// </summary>
        public event EventHandler<ClientDataEventArgs> DataReceived;

        /// <summary>
        /// Fired for every incoming request (even OPTIONS, HEAD, invalid paths…)
        /// Useful for logging or debugging
        /// </summary>
        public event EventHandler<RequestInfoEventArgs> RequestArrived;

        // Optional: still allow custom full control over response if needed
        public event Action<HttpListenerRequest, HttpListenerResponse, string> CustomRequestHandler;

        public ServerService(string prefix = "http://*:5001/")
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentNullException(nameof(prefix));

            if (!prefix.EndsWith("/")) prefix += "/";
            _prefix = prefix;

        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            _serverThread = new Thread(ServerLoop) { IsBackground = true, Name = "HttpServer-5001" };
            _serverThread.Start();
        }

        private void ServerLoop()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(_prefix);
                _listener.Start();

                while (_isRunning)
                {
                    HttpListenerContext ctx;
                    try
                    {
                        ctx = _listener.GetContext();
                    }
                    catch (HttpListenerException ex) when (ex.ErrorCode == 995 || ex.ErrorCode == 2)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }

                    if (!_isRunning) break;

                    try
                    {
                        ProcessRequest(ctx);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Request failed: {ex.Message}");
                        try { SendError(ctx.Response, 500, "Server error"); }
                        catch { }
                    }
                    finally
                    {
                        try { ctx.Response?.Close(); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server crashed: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            // ── Notify everyone that something arrived ───────────────────────
            RequestArrived?.Invoke(this, new RequestInfoEventArgs
            {
                ClientIP = req.RemoteEndPoint?.ToString() ?? "unknown",
                Method = req.HttpMethod,
                Path = req.Url.AbsolutePath,
                Query = req.Url.Query,
                HasBody = req.HasEntityBody,
                ContentType = req.ContentType,
                Timestamp = DateTime.Now
            });

            // ── Try to read body if present ─────────────────────────────────
            string body = null;
            if (req.HasEntityBody)
            {
                try
                {
                    var encoding = req.ContentEncoding ?? Encoding.UTF8;
                    using (var reader = new StreamReader(req.InputStream, encoding))
                    {
                        body = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Body read error: {ex.Message}");
                }
            }

            // ── Raise the main DataReceived event ───────────────────────────
            if (DataReceived != null &&
                (body != null || !string.IsNullOrEmpty(req.Url.Query) || req.HttpMethod != "OPTIONS"))
            {
                DataReceived?.Invoke(this, new ClientDataEventArgs
                {
                    ClientIP = req.RemoteEndPoint?.Address?.ToString() ?? "unknown",
                    Method = req.HttpMethod,
                    Path = req.Url.AbsolutePath,
                    QueryString = req.QueryString,
                    Body = body,
                    ContentType = req.ContentType ?? "none",
                   // Headers = req.Headers,
                    ReceivedAt = DateTime.Now
                });
            }

            // ── Let custom handler take over if someone wants full control ──
            // Pass the already-read body to avoid reading the stream twice
            if (CustomRequestHandler != null)
            {
                CustomRequestHandler.Invoke(req, resp, body);
                return;
            }

            // ── Default minimal response if no custom handler ───────────────
            string responseJson = "{\"status\":\"received\"}";
            int status = 200;

            if (req.HttpMethod == "GET" && req.Url.AbsolutePath.TrimEnd('/') == "/ping")
            {
                responseJson = "{\"status\":\"ok\",\"message\":\"pong\"}";
            }
            else if (req.HttpMethod == "OPTIONS")
            {
                status = 200;
                responseJson = "{\"allow\":\"GET,POST,OPTIONS\"}";
                resp.AddHeader("Allow", "GET, POST, OPTIONS");
            }
            else if (req.HttpMethod != "POST" && req.HttpMethod != "GET")
            {
                status = 405;
                responseJson = "{\"error\":\"Method not allowed\"}";
            }

            SendJson(resp, responseJson, status);
        }

        // Helpers
        private static void SendJson(HttpListenerResponse resp, string json, int status = 200)
        {
            byte[] data = Encoding.UTF8.GetBytes(json ?? "{}");
            resp.StatusCode = status;
            resp.ContentType = "application/json; charset=utf-8";
            resp.ContentLength64 = data.Length;
            using (var s = resp.OutputStream)
                s.Write(data, 0, data.Length);
        }

        private static void SendError(HttpListenerResponse resp, int code, string msg)
        {
            SendJson(resp, "{\"error\":\"" + msg.Replace("\"", "\\\"") + "\"}", code);
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            try { _listener?.Stop(); _listener?.Close(); } catch { }
            Thread.Sleep(150);
        }

        public void Dispose()
        {
            Stop();
            //_listener?.Dispose();
            _listener = null;
        }

        public bool IsRunning => _isRunning;
        public string Prefix => _prefix;
    }

    // ──────────────────────────────────────────────────────────────
    // Event Argument Classes
    // ──────────────────────────────────────────────────────────────

    public class RequestInfoEventArgs : EventArgs
    {
        public string ClientIP { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
        public bool HasBody { get; set; }
        public string ContentType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ClientDataEventArgs : EventArgs
    {
        public string ClientIP { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public NameValueCollection QueryString { get; set; }
        public string Body { get; set; }               // ← most important for POST data
        public string ContentType { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
