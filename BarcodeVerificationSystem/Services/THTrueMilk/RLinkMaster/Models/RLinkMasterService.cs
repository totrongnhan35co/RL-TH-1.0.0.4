using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using BarcodeVerificationSystem.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
{
    /// <summary>
    /// Gọi HTTP API của R-Link Master (hoặc Simulator).
    /// Khi token hết hạn (401): tự động refresh → nếu refresh cũng fail thì re-login lại.
    /// </summary>
    public class RLinkMasterService : IRLinkMasterService
    {
        private readonly string _baseUrl;
        private string _accessToken;
        private string _refreshToken;
        private DateTime? _tokenExpiry;

        private string _storedUsername;
        private string _storedPassword;

        private const int TokenBufferSeconds = 600;
        private const int RequestTimeoutSeconds = 10; // timeout riêng cho mỗi request

        // ── Fix: chống gọi lồng EnsureTokenValidAsync ──────────────
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
        private bool _isRefreshing = false;
        // ────────────────────────────────────────────────────────────

        private bool _isMasterConnected = true;
        /// <summary>Trạng thái kết nối đến R-Link Master (cập nhật sau mỗi lần gửi monitor).</summary>
        public bool IsMasterConnected => _isMasterConnected;

        internal string AccessToken => _accessToken;
        internal string RefreshTokenValue => _refreshToken;
        internal string StoredUsername => _storedUsername;
        internal string StoredPassword => _storedPassword;

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_accessToken);

        internal void SetTokens(string accessToken, string refreshToken)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _tokenExpiry = ExtractExpFromToken(accessToken);
        }

        private static DateTime? ExtractExpFromToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            var parts = token.Split('.');
            if (parts.Length < 2) return null;
            var payload = parts[1];
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            payload = payload.Replace('-', '+').Replace('_', '/');
            try
            {
                var bytes = Convert.FromBase64String(payload);
                var json = Encoding.UTF8.GetString(bytes);
                var obj = JsonConvert.DeserializeAnonymousType(json, new { exp = 0L });
                if (obj != null && obj.exp > 0)
                    return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(obj.exp);
            }
            catch { }
            return null;
        }

        public void StoreCredentials(string username, string password)
        {
            _storedUsername = username;
            _storedPassword = password;
        }

        public RLinkMasterService(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<bool> PostSyncReportAsync(SyncReportPayload report)
        {
            try { await PostRawAsync(ApiEndpoints.LogSyncReport, JsonConvert.SerializeObject(report), auth: false); return true; }
            catch { return false; }
        }

        public async Task<bool> MarkQrUsedAsync(
            List<string> qrCodes,
            string jobName,
            string lineId,
            string lineName,
            string factoryCode,
            string batch,
            string productId,
            string prod = "",
            string exp = "",
            string printedAt = "")
        {
            try
            {
                var payload = new
                {
                    qr_codes = qrCodes,
                    job_name = jobName ?? string.Empty,
                    line_id = lineId ?? string.Empty,
                    line_name = lineName ?? string.Empty,
                    factory_code = factoryCode ?? string.Empty,
                    batch = batch ?? string.Empty,
                    product_id = productId ?? string.Empty,
                    rlink_name = Shared.Settings?.RLinkName ?? string.Empty,
                    prod = prod ?? string.Empty,
                    exp = exp ?? string.Empty,
                    timestamp = string.IsNullOrWhiteSpace(printedAt) ? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff") : printedAt
                };
                await PostRawAsync(ApiEndpoints.LogMarkUsed, JsonConvert.SerializeObject(payload), auth: true);
                ProjectLogger.WriteInfo(
                    $"[RLinkMaster] MarkQrUsed OK: {qrCodes.Count} QR" +
                    $" | job='{jobName}' line='{lineId}' batch='{batch}'");
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteWarning($"[RLinkMaster] MarkQrUsedAsync lỗi (non-critical): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkQrUnusedAsync(List<string> qrCodes, string jobName)
        {
            try
            {
                var payload = new
                {
                    qr_codes = qrCodes ?? new List<string>(),
                    job_name = jobName ?? string.Empty,
                    released_at = DateTime.Now
                };
                await PostRawAsync(ApiEndpoints.LogMarkUnused, JsonConvert.SerializeObject(payload), auth: true);
                ProjectLogger.WriteInfo($"[RLinkMaster] MarkQrUnused OK: {qrCodes.Count} QR | job='{jobName}'");
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteWarning($"[RLinkMaster] MarkQrUnusedAsync lỗi (non-critical): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckResyncRequestAsync(string lineId)
        {
            try
            {
                string json = await GetRawAsync($"{ApiEndpoints.LogResyncRequest}/{Uri.EscapeDataString(lineId)}");

                if (string.IsNullOrWhiteSpace(json) || json.TrimStart().StartsWith("<"))
                {
                    ProjectLogger.WriteWarning($"[RLinkMaster] CheckResync: Server trả về HTML thay vì JSON — endpoint chưa được hỗ trợ trên Simulator.");
                    return false;
                }

                var d = JsonConvert.DeserializeObject<dynamic>(json);
                return d?.resync == true;
            }
            catch { return false; }
        }

        public async Task<bool> PingAsync()
        {
            string url = $"{_baseUrl}{ApiEndpoints.Health}";
            ProjectLogger.WriteInfo($"[RLinkMaster] PingAsync → {url}");

            var uri = new Uri(url);
            bool isHttps = string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase);

            try
            {
                using (var tcp = new System.Net.Sockets.TcpClient())
                {
                    var connectTask = tcp.ConnectAsync(uri.Host, uri.Port);
                    if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                    {
                        ProjectLogger.WriteError($"[RLinkMaster] PingAsync → TCP connect TIMEOUT (5s) ({uri.Host}:{uri.Port})");
                        return false;
                    }

                    try { await connectTask; }
                    catch (Exception ex)
                    {
                        ProjectLogger.WriteError($"[RLinkMaster] PingAsync → TCP connect FAILED ({uri.Host}:{uri.Port}): {ex.Message}");
                        return false;
                    }

                    if (!tcp.Connected)
                    {
                        ProjectLogger.WriteError($"[RLinkMaster] PingAsync → TCP connect FAILED ({uri.Host}:{uri.Port})");
                        return false;
                    }

                    ProjectLogger.WriteInfo($"[RLinkMaster] PingAsync → TCP connect OK ({uri.Host}:{uri.Port})");

                    var netStream = tcp.GetStream();
                    System.IO.Stream stream = netStream;

                    System.Net.Security.SslStream sslStream = null;
                    if (isHttps)
                    {
                        sslStream = new System.Net.Security.SslStream(netStream, false,
                            (sender, cert, chain, sslPolicyErrors) => true);

                        var sslTask = sslStream.AuthenticateAsClientAsync(
                            uri.Host,
                            null,
                            System.Security.Authentication.SslProtocols.Tls12 |
                            System.Security.Authentication.SslProtocols.Tls13,
                            false
                        );

                        if (await Task.WhenAny(sslTask, Task.Delay(15000)) != sslTask)
                        {
                            ProjectLogger.WriteError($"[RLinkMaster] PingAsync → SSL handshake TIMEOUT (15s)");
                            return false;
                        }

                        try { await sslTask; }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteError($"[RLinkMaster] PingAsync → SSL handshake ERROR: {ex.Message}");
                            return false;
                        }

                        ProjectLogger.WriteInfo($"[RLinkMaster] PingAsync → SSL handshake OK");
                        stream = sslStream;
                    }

                    try
                    {
                        string requestStr = $"GET {uri.PathAndQuery} HTTP/1.1\r\n" +
                                            $"Host: {uri.Host}\r\n" +
                                            $"User-Agent: RLink-HealthCheck\r\n" +
                                            $"Accept: */*\r\n" +
                                            $"Connection: close\r\n\r\n";

                        byte[] requestBytes = Encoding.ASCII.GetBytes(requestStr);
                        stream.WriteTimeout = 15000;

                        var writeTask = stream.WriteAsync(requestBytes, 0, requestBytes.Length);
                        if (await Task.WhenAny(writeTask, Task.Delay(15000)) != writeTask)
                        {
                            ProjectLogger.WriteError($"[RLinkMaster] PingAsync → HTTP request send TIMEOUT (15s)");
                            return false;
                        }

                        try { await writeTask; }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteError($"[RLinkMaster] PingAsync → HTTP request send ERROR: {ex.Message}");
                            return false;
                        }

                        ProjectLogger.WriteInfo($"[RLinkMaster] PingAsync → HTTP request sent");
                    }
                    catch (Exception ex)
                    {
                        ProjectLogger.WriteError($"[RLinkMaster] PingAsync → HTTP request ERROR: {ex.Message}");
                        return false;
                    }

                    try
                    {
                        stream.ReadTimeout = 15000;
                        using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                        {
                            var readTask = reader.ReadToEndAsync();
                            if (await Task.WhenAny(readTask, Task.Delay(15000)) != readTask)
                            {
                                ProjectLogger.WriteError($"[RLinkMaster] PingAsync → HTTP response TIMEOUT (15s)");
                                return false;
                            }

                            string responseBody = await readTask;
                            string statusLine = responseBody?.Split('\n')?.FirstOrDefault()?.Trim() ?? "";

                            bool ok = false;
                            string[] parts = statusLine.Split(' ');
                            if (parts.Length >= 2 && int.TryParse(parts[1], out int statusCode))
                                ok = statusCode >= 200 && statusCode < 300;

                            if (!ok && !string.IsNullOrEmpty(responseBody))
                            {
                                ok = responseBody.Contains("\"status\":\"ok\"") ||
                                     responseBody.Contains("\"status\": \"ok\"");
                            }

                            ProjectLogger.WriteInfo($"[RLinkMaster] PingAsync → HTTP {(ok ? "OK" : "FAIL")} (status: {statusLine})");
                            return ok;
                        }
                    }
                    catch (Exception ex)
                    {
                        ProjectLogger.WriteError($"[RLinkMaster] PingAsync → HTTP response read ERROR: {ex.Message}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[RLinkMaster] PingAsync lỗi ngoài dự kiến: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> SendHeartbeatAsync(string lineId, string factoryCode)
        {
            try
            {
                string path = $"{ApiEndpoints.Heartbeat}" +
                    $"?lineId={Uri.EscapeDataString(lineId ?? "")}" +
                    $"&factoryCode={Uri.EscapeDataString(factoryCode ?? "")}";
                await GetRawAsync(path);
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[RLinkMaster] Heartbeat lỗi: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            try
            {
                ProjectLogger.WriteInfo($"[RLinkMaster] Đang đăng nhập: user='{username}' → {_baseUrl}");
                string body = JsonConvert.SerializeObject(new { username, password });
                string json = await PostRawAsync(ApiEndpoints.AuthLogin, body, auth: false);
                var result = JsonConvert.DeserializeObject<LoginResult>(json);

                if (result != null && result.IsSuccess)
                {
                    _accessToken = result.AccessToken;
                    _refreshToken = result.RefreshToken;
                    _tokenExpiry = result.TokenExpiresIn > 0
                        ? DateTime.UtcNow.AddSeconds(result.TokenExpiresIn)
                        : (DateTime?)null;
                    _storedUsername = username;
                    _storedPassword = password;
                    SyncTokensToSharedSettings();
                    ProjectLogger.WriteInfo($"[RLinkMaster] Login OK: user='{username}' token hết hạn sau {result.TokenExpiresIn}s");
                }
                else
                {
                    ProjectLogger.WriteWarning($"[RLinkMaster] Login FAIL: user='{username}' → {result?.Message}");
                }

                return result ?? new LoginResult { IsSuccess = false, Message = "Phản hồi không hợp lệ." };
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[RLinkMaster] LoginAsync exception: user='{username}' → {ex.Message}", ex);
                return new LoginResult { IsSuccess = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        public async Task<LoginResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                ProjectLogger.WriteInfo("[RLinkMaster] Đang refresh token...");
                string body = JsonConvert.SerializeObject(new { refresh_token = refreshToken });
                string json = await PostRawAsync(ApiEndpoints.AuthRefreshToken, body, auth: false);
                var result = JsonConvert.DeserializeObject<LoginResult>(json);

                // ── Fix: refresh endpoint trả về snake_case, login trả về camelCase ──
                if (result == null || !result.IsSuccess)
                {
                    var snake = JsonConvert.DeserializeAnonymousType(json, new
                    {
                        is_success = false,
                        message = "",
                        userId = "",
                        token = "",
                        refresh_token = "",
                        token_expires_in = 0
                    });
                    if (snake != null && snake.is_success)
                    {
                        result = new LoginResult
                        {
                            IsSuccess = true,
                            Message = snake.message,
                            UserId = snake.userId,
                            AccessToken = snake.token,
                            RefreshToken = snake.refresh_token,
                            TokenExpiresIn = snake.token_expires_in
                        };
                    }
                }
                // ──────────────────────────────────────────────────────────────────────

                if (result != null && result.IsSuccess)
                {
                    _accessToken = result.AccessToken;
                    _refreshToken = result.RefreshToken;
                    _tokenExpiry = result.TokenExpiresIn > 0
                        ? DateTime.UtcNow.AddSeconds(result.TokenExpiresIn)
                        : (DateTime?)null;
                    SyncTokensToSharedSettings();
                    ProjectLogger.WriteInfo("[RLinkMaster] Refresh token OK.");
                }
                else
                {
                    ProjectLogger.WriteWarning($"[RLinkMaster] Refresh token FAIL: {result?.Message}");
                }

                return result ?? new LoginResult { IsSuccess = false };
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkMaster] RefreshTokenAsync exception: " + ex.Message, ex);
                return new LoginResult { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<List<AccountInfo>> GetAccountsAsync()
        {
            try
            {
                string json = await GetRawAsync(ApiEndpoints.AuthAccounts);

                var wrapper = JsonConvert.DeserializeObject<AccountListResponse>(json);
                if (wrapper?.data != null)
                {
                    ProjectLogger.WriteInfo($"[RLinkMaster] GetAccounts OK: {wrapper.data.Count} tài khoản.");
                    return wrapper.data;
                }

                var list = JsonConvert.DeserializeObject<List<AccountInfo>>(json) ?? new List<AccountInfo>();
                ProjectLogger.WriteInfo($"[RLinkMaster] GetAccounts OK (raw): {list.Count} tài khoản.");
                return list;
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex.InnerException is WebException)
                    ProjectLogger.WriteDebug($"[RLinkMaster] GetAccountsAsync chưa đăng nhập: {ex.Message}");
                else
                    ProjectLogger.WriteError("[RLinkMaster] GetAccountsAsync lỗi: " + ex.Message, ex);
                return new List<AccountInfo>();
            }
        }

        public async Task<List<LineInfo>> GetLinesAsync(string factoryCode = null)
        {
            try
            {
                string path = ApiEndpoints.LinesList;
                if (!string.IsNullOrWhiteSpace(factoryCode))
                    path += $"?factory_code={Uri.EscapeDataString(factoryCode)}";

                string json = await GetRawAsync(path);

                var wrapper = JsonConvert.DeserializeObject<LineListResponse>(json);
                if (wrapper?.data != null)
                {
                    ProjectLogger.WriteInfo($"[RLinkMaster] GetLines OK: {wrapper.data.Count} line (factory='{factoryCode}').");
                    return wrapper.data;
                }

                var list = JsonConvert.DeserializeObject<List<LineInfo>>(json) ?? new List<LineInfo>();
                ProjectLogger.WriteInfo($"[RLinkMaster] GetLines OK (raw): {list.Count} line (factory='{factoryCode}').");
                return list;
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex.InnerException is WebException)
                    ProjectLogger.WriteDebug($"[RLinkMaster] GetLinesAsync chưa đăng nhập: {ex.Message}");
                else
                    ProjectLogger.WriteError("[RLinkMaster] GetLinesAsync lỗi: " + ex.Message, ex);
                return new List<LineInfo>();
            }
        }

        public async Task<bool> SetLineStatusAsync(string lineId, string machineIp = "", string factoryCode = "")
        {
            try
            {
                await PostRawAsync(ApiEndpoints.LinesStatus,
                    JsonConvert.SerializeObject(new { line_id = lineId, factory_code = factoryCode ?? "", line_ip = machineIp ?? "" }));
                ProjectLogger.WriteInfo($"[RLinkMaster] SetLineStatus OK: line='{lineId}' ip='{machineIp}'");
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[RLinkMaster] SetLineStatusAsync lỗi: line='{lineId}' → {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> UnassignLineAsync(string lineId, string factoryCode = "")
        {
            try
            {
                await PostRawAsync(ApiEndpoints.LinesDeactivate,
                    JsonConvert.SerializeObject(new { line_id = lineId, factory_code = factoryCode ?? "" }));
                ProjectLogger.WriteInfo($"[RLinkMaster] UnassignLine OK: line='{lineId}'");
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[RLinkMaster] UnassignLineAsync lỗi: line='{lineId}' → {ex.Message}", ex);
                return false;
            }
        }

        public async Task<RLinkSettings> GetSettingsAsync(string lineId)
        {
            try
            {
                string json = await GetRawAsync($"{ApiEndpoints.Settings}/{Uri.EscapeDataString(lineId ?? "")}");

                var wrapper = JsonConvert.DeserializeObject<SettingsResponse>(json);
                if (wrapper?.data != null)
                {
                    ProjectLogger.WriteInfo($"[RLinkMaster] GetSettings OK: line='{lineId}' mode={wrapper.data.OperatingMode} buffer={wrapper.data.BufferCount}");
                    return wrapper.data;
                }

                var s = JsonConvert.DeserializeObject<RLinkSettings>(json) ?? new RLinkSettings();
                ProjectLogger.WriteInfo($"[RLinkMaster] GetSettings OK (raw): line='{lineId}' mode={s.OperatingMode} buffer={s.BufferCount}");
                return s;
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex.InnerException is WebException)
                    ProjectLogger.WriteDebug($"[RLinkMaster] GetSettingsAsync chưa đăng nhập: {ex.Message}");
                else
                    ProjectLogger.WriteError($"[RLinkMaster] GetSettingsAsync lỗi: line='{lineId}' → {ex.Message}", ex);
                return new RLinkSettings();
            }
        }

        public async Task<List<ProductItem>> GetProductsAsync()
        {
            try
            {
                string json = await GetRawAsync(ApiEndpoints.Products);

                var wrapper = JsonConvert.DeserializeObject<ProductListResponse>(json);
                if (wrapper?.data != null)
                {
                    ProjectLogger.WriteInfo($"[RLinkMaster] GetProducts OK: {wrapper.data.Count} sản phẩm.");
                    return wrapper.data;
                }

                var list = JsonConvert.DeserializeObject<List<ProductItem>>(json) ?? new List<ProductItem>();
                ProjectLogger.WriteInfo($"[RLinkMaster] GetProducts OK (raw): {list.Count} sản phẩm.");
                return list;
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex.InnerException is WebException)
                    ProjectLogger.WriteDebug($"[RLinkMaster] GetProductsAsync chưa đăng nhập: {ex.Message}");
                else
                    ProjectLogger.WriteError("[RLinkMaster] GetProductsAsync lỗi: " + ex.Message, ex);
                return new List<ProductItem>();
            }
        }

        public async Task<QrConfig> GetQrConfigAsync()
        {
            try
            {
                string json = await GetRawAsync(ApiEndpoints.QrConfig);
                var wrapper = JsonConvert.DeserializeObject<QrConfigResponse>(json);
                if (wrapper?.data != null)
                {
                    ProjectLogger.WriteInfo($"[RLinkMaster] GetQrConfig OK: baseUrl={wrapper.data.BaseUrl}, numberOfUrl={wrapper.data.NumberOfUrl}");
                    return wrapper.data;
                }
                return null;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkMaster] GetQrConfigAsync lỗi: " + ex.Message, ex);
                return null;
            }
        }

        public async Task<bool> SendMonitorAsync(MonitorPayload payload)
        {
            try
            {
                await PostRawAsync(ApiEndpoints.Monitor, JsonConvert.SerializeObject(payload), allowRetry: false);
                _isMasterConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                _isMasterConnected = false;
                ProjectLogger.WriteWarning("[RLinkMaster] SendMonitorAsync lỗi: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> SendLogCameraAsync(LogCameraPayload payload)
        {
            try { await PostRawAsync(ApiEndpoints.LogCamera, JsonConvert.SerializeObject(payload)); return true; }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkMaster] SendLogCameraAsync lỗi: " + ex.Message, ex);
                return false;
            }
        }

        public async Task<bool> SendLogCameraErrorAsync(LogCameraErrorPayload payload)
        {
            try { await PostRawAsync(ApiEndpoints.LogCameraError, JsonConvert.SerializeObject(payload)); return true; }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkMaster] SendLogCameraErrorAsync lỗi: " + ex.Message, ex);
                return false;
            }
        }

        public async Task<bool> SendErrorImageAsync(ErrorImagePayload payload)
        {
            try { await PostRawAsync(ApiEndpoints.LogErrorImage, JsonConvert.SerializeObject(payload)); return true; }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkMaster] SendErrorImageAsync lỗi: " + ex.Message, ex);
                return false;
            }
        }

        public async Task<bool> SendLogStatusAsync(LogStatusPayload payload)
        {
            try { await PostRawAsync(ApiEndpoints.LogStatus, JsonConvert.SerializeObject(payload)); return true; }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkMaster] SendLogStatusAsync lỗi: " + ex.Message, ex);
                return false;
            }
        }

        public async Task<CompleteJobResult> CompleteJobAsync(CompleteJobRequest request)
        {
            try
            {
                ProjectLogger.WriteInfo($"[RLinkMaster] CompleteJob: job='{request?.JobName}'");
                string body = JsonConvert.SerializeObject(request);
                string json = await PostRawAsync(ApiEndpoints.LogComplete, body, auth: true);
                var result = JsonConvert.DeserializeObject<CompleteJobResult>(json)
                    ?? new CompleteJobResult { IsSuccess = false, Message = "Phản hồi không hợp lệ." };

                if (result.IsSuccess)
                    ProjectLogger.WriteInfo($"[RLinkMaster] CompleteJob OK: job='{request?.JobName}'");
                else
                    ProjectLogger.WriteWarning($"[RLinkMaster] CompleteJob FAIL: job='{request?.JobName}' → {result.Message}");

                return result;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[RLinkMaster] CompleteJobAsync lỗi: job='{request?.JobName}' → {ex.Message}", ex);
                return new CompleteJobResult { IsSuccess = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // ── HTTP helpers ─────────────────────────────────────────────

        private static string Truncate(string s, int max = 10000)
        {
            if (string.IsNullOrEmpty(s)) return "(empty)";
            return s.Length <= max ? s : s.Substring(0, max) + $"...[+{s.Length - max} chars]";
        }

        private async Task<(HttpStatusCode StatusCode, string Body)> SendWebRequestAsync(
            string url, string method, string jsonBody, bool auth)
        {
            var uri = new Uri(url);
            int port = uri.Port > 0 ? uri.Port : (uri.Scheme == "https" ? 443 : 80);
            bool isHttps = uri.Scheme == "https";

            if (auth && string.IsNullOrEmpty(_accessToken))
            {
                ProjectLogger.WriteWarning($"[DIAG] Token rỗng, trả synthetic 401");
                return (HttpStatusCode.Unauthorized, "");
            }

            ProjectLogger.WriteInfo($"[DIAG] TCP {method} {url} | Auth={auth} | Bearer={auth}");

            using (var tcp = new System.Net.Sockets.TcpClient())
            {
                var connTask = tcp.ConnectAsync(uri.Host, port);
                if (await Task.WhenAny(connTask, Task.Delay(RequestTimeoutSeconds * 1000)) != connTask)
                    throw new OperationCanceledException("TCP connect timeout");
                await connTask;

                System.IO.Stream stream = tcp.GetStream();

                if (isHttps)
                {
                    var ssl = new System.Net.Security.SslStream(stream, false,
                        (s, cert, chain, err) => true);
                    var sslTask = ssl.AuthenticateAsClientAsync(uri.Host, null,
                        System.Security.Authentication.SslProtocols.Tls12 |
                        System.Security.Authentication.SslProtocols.Tls13 |
                        System.Security.Authentication.SslProtocols.Tls11 |
                        System.Security.Authentication.SslProtocols.Tls,
                        false);
                    if (await Task.WhenAny(sslTask, Task.Delay(RequestTimeoutSeconds * 1000)) != sslTask)
                        throw new OperationCanceledException("SSL handshake timeout");
                    await sslTask;
                    stream = ssl;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"{method} {uri.PathAndQuery} HTTP/1.1");
                sb.AppendLine($"Host: {uri.Host}");
                sb.AppendLine("User-Agent: RLink-Client");
                sb.AppendLine("Accept: application/json");
                sb.AppendLine("Connection: close");

                if (auth)
                    sb.AppendLine($"Authorization: Bearer {_accessToken}");

                if (jsonBody != null)
                {
                    byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);
                    sb.AppendLine("Content-Type: application/json; charset=utf-8");
                    sb.AppendLine($"Content-Length: {bodyBytes.Length}");
                    sb.AppendLine();
                    byte[] headerBytes = Encoding.ASCII.GetBytes(sb.ToString());
                    await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
                    await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
                }
                else
                {
                    sb.AppendLine();
                    byte[] headerBytes = Encoding.ASCII.GetBytes(sb.ToString());
                    await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
                }

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var readTask = reader.ReadToEndAsync();
                    if (await Task.WhenAny(readTask, Task.Delay(RequestTimeoutSeconds * 1000)) != readTask)
                        throw new OperationCanceledException("HTTP response timeout");
                    string raw = await readTask;

                    int hEnd = raw.IndexOf("\r\n\r\n");
                    if (hEnd < 0) throw new Exception("Invalid HTTP response");

                    string headerPart = raw.Substring(0, hEnd);
                    string bodyPart = raw.Substring(hEnd + 4);
                    string[] headerLines = headerPart.Split(new[] { "\r\n" }, StringSplitOptions.None);

                    string[] sp = headerLines[0].Split(' ');
                    int sc = 500;
                    if (sp.Length >= 2) int.TryParse(sp[1], out sc);

                    foreach (string hl in headerLines.Skip(1))
                    {
                        int ci = hl.IndexOf(':');
                        if (ci > 0 && hl.Substring(0, ci).Trim()
                            .Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
                            && hl.IndexOf("chunked", StringComparison.OrdinalIgnoreCase) >= ci)
                        {
                            bodyPart = DecodeChunkedBody(bodyPart);
                            break;
                        }
                    }

                    ProjectLogger.WriteDebug($"[RAW] {(HttpStatusCode)sc} | {Truncate(bodyPart)}");
                    return ((HttpStatusCode)sc, bodyPart);
                }
            }
        }

        private static string DecodeChunkedBody(string body)
        {
            var result = new StringBuilder();
            int pos = 0;
            while (pos < body.Length)
            {
                int nl = body.IndexOf("\r\n", pos);
                if (nl < 0) break;
                string sizeHex = body.Substring(pos, nl - pos).Trim();
                if (!int.TryParse(sizeHex, System.Globalization.NumberStyles.HexNumber, null, out int chunkSize) || chunkSize == 0)
                    break;
                pos = nl + 2;
                if (pos + chunkSize > body.Length) break;
                result.Append(body.Substring(pos, chunkSize));
                pos += chunkSize + 2;
            }
            return result.ToString();
        }

        private async Task<string> GetRawAsync(string path)
        {
            await EnsureTokenValidAsync();
            string url = _baseUrl + path;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            ProjectLogger.WriteDebug($"[API→] GET {url}");

            HttpStatusCode statusCode;
            string responseBody;

            try
            {
                (statusCode, responseBody) = await SendWebRequestAsync(url, "GET", null, auth: true).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                ProjectLogger.WriteError($"[RLinkMaster] GET {path} → TIMEOUT sau {RequestTimeoutSeconds}s");
                throw new TaskCanceledException($"GET {path} timeout sau {RequestTimeoutSeconds}s");
            }

            sw.Stop();
            ProjectLogger.WriteDebug($"[API←] {(int)statusCode} | GET {path} | {sw.ElapsedMilliseconds}ms | {Truncate(responseBody)}");

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                ProjectLogger.WriteWarning($"[RLinkMaster] 401 Unauthorized: GET {path} → đang restore auth...");
                if (await TryRestoreAuthAsync())
                {
                (statusCode, responseBody) = await SendWebRequestAsync(url, "GET", null, auth: true).ConfigureAwait(false);
                    ProjectLogger.WriteDebug($"[API←] Retry {(int)statusCode} | GET {path} | {Truncate(responseBody)}");
                }
            }

            if (!((int)statusCode >= 200 && (int)statusCode < 300))
                ProjectLogger.WriteWarning($"[RLinkMaster] GET {path} → HTTP {(int)statusCode}");

            if (!((int)statusCode >= 200 && (int)statusCode < 300))
                throw new WebException($"GET {path} returned {(int)statusCode}");

            return responseBody;
        }

        private async Task<string> PostRawAsync(string path, string jsonBody, bool auth = true, bool allowRetry = true)
        {
            if (auth) await EnsureTokenValidAsync();
            string url = _baseUrl + path;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            ProjectLogger.WriteDebug($"[API→] POST {url} | Body: {Truncate(jsonBody)}");

            HttpStatusCode statusCode;
            string responseBody;

            try
            {
                (statusCode, responseBody) = await SendWebRequestAsync(url, "POST", jsonBody, auth).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                ProjectLogger.WriteError($"[RLinkMaster] POST {path} → TIMEOUT sau {RequestTimeoutSeconds}s");
                throw new TaskCanceledException($"POST {path} timeout sau {RequestTimeoutSeconds}s");
            }

            sw.Stop();
            ProjectLogger.WriteDebug($"[API←] {(int)statusCode} | POST {path} | {sw.ElapsedMilliseconds}ms | {Truncate(responseBody)}");

            if (allowRetry && auth && statusCode == HttpStatusCode.Unauthorized)
            {
                ProjectLogger.WriteWarning($"[RLinkMaster] 401 Unauthorized: POST {path} → đang restore auth...");
                if (await TryRestoreAuthAsync())
                {
                    (statusCode, responseBody) = await SendWebRequestAsync(url, "POST", jsonBody, auth).ConfigureAwait(false);
                    ProjectLogger.WriteDebug($"[API←] Retry {(int)statusCode} | POST {path} | {Truncate(responseBody)}");
                }
            }

            if (!((int)statusCode >= 200 && (int)statusCode < 300))
                ProjectLogger.WriteError(
                    $"[API] POST {url} → {(int)statusCode}" +
                    $"\n  REQUEST : {Truncate(jsonBody)}" +
                    $"\n  RESPONSE: {Truncate(responseBody)}");

            if (!((int)statusCode >= 200 && (int)statusCode < 300))
                throw new WebException($"POST {path} returned {(int)statusCode}");

            return responseBody;
        }

        /// <summary>
        /// Kiểm tra token sắp hết hạn — nếu còn dưới TokenBufferSeconds thì refresh trước.
        /// Fix: dùng SemaphoreSlim + flag _isRefreshing để tránh gọi lồng khi nhiều request cùng lúc.
        /// </summary>
        private async Task EnsureTokenValidAsync()
        {
            if (!_tokenExpiry.HasValue) return;

            double remainingSeconds = (_tokenExpiry.Value - DateTime.UtcNow).TotalSeconds;
            if (remainingSeconds > TokenBufferSeconds) return;

            // ── Fix: nếu đang refresh rồi thì bỏ qua, không gọi lồng ──
            if (_isRefreshing) return;

            // ── Fix: dùng lock để chỉ 1 thread refresh tại 1 thời điểm ──
            await _refreshLock.WaitAsync();
            try
            {
                // Double-check sau khi lấy được lock
                remainingSeconds = (_tokenExpiry.Value - DateTime.UtcNow).TotalSeconds;
                if (remainingSeconds > TokenBufferSeconds) return;

                _isRefreshing = true;
                ProjectLogger.WriteInfo($"[RLinkMaster] Token sắp hết hạn (còn {remainingSeconds:F0}s) → refresh trước...");

                if (!string.IsNullOrEmpty(_refreshToken))
                {
                    var rr = await RefreshTokenAsync(_refreshToken);
                    if (rr.IsSuccess)
                    {
                        ProjectLogger.WriteInfo("[RLinkMaster] Proactive refresh OK.");
                        return;
                    }
                    ProjectLogger.WriteWarning($"[RLinkMaster] Proactive refresh thất bại → {rr.Message}");
                }

                if (!string.IsNullOrEmpty(_storedUsername) && !string.IsNullOrEmpty(_storedPassword))
                {
                    var lr = await LoginAsync(_storedUsername, _storedPassword);
                    if (lr.IsSuccess)
                    {
                        ProjectLogger.WriteInfo("[RLinkMaster] Proactive re-login OK.");
                        return;
                    }
                    ProjectLogger.WriteError($"[RLinkMaster] Proactive re-login thất bại → {lr.Message}", null);
                }
            }
            finally
            {
                _isRefreshing = false;
                _refreshLock.Release();
            }
        }

        /// <summary>
        /// Khôi phục auth: thử refresh token trước; nếu không được thì re-login lại.
        /// </summary>
        private async Task<bool> TryRestoreAuthAsync()
        {
            if (!string.IsNullOrEmpty(_refreshToken))
            {
                var rr = await RefreshTokenAsync(_refreshToken);
                if (rr.IsSuccess)
                {
                    ProjectLogger.WriteInfo("[RLinkMaster] TryRestoreAuth: Token đã được refresh tự động.");
                    return true;
                }
                ProjectLogger.WriteWarning($"[RLinkMaster] TryRestoreAuth: Refresh thất bại → {rr.Message}");
            }

            if (!string.IsNullOrEmpty(_storedUsername) && !string.IsNullOrEmpty(_storedPassword))
            {
                var lr = await LoginAsync(_storedUsername, _storedPassword);
                if (lr.IsSuccess)
                {
                    ProjectLogger.WriteInfo("[RLinkMaster] TryRestoreAuth: Re-login tự động thành công.");
                    return true;
                }
                ProjectLogger.WriteError($"[RLinkMaster] TryRestoreAuth: Re-login thất bại → {lr.Message}", null);
            }

            return false;
        }

        private void SyncTokensToSharedSettings()
        {
            Shared.Settings.AccessToken = _accessToken ?? "";
            Shared.Settings.RefreshToken = _refreshToken ?? "";
            Shared.SaveSettings();
        }
    }
}