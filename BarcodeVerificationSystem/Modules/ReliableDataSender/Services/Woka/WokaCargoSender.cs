using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Woka;
using BarcodeVerificationSystem.Model.Woka.Request;
using BarcodeVerificationSystem.Model.Woka.Response;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BarcodeVerificationSystem.Model.SyncDataParams;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Woka
{
    public class WokaCargoSender : ISenderService<VerificationDataEntry>
    {
        // ─── Fields ──────────────────────────────────────────────────────────────
        private readonly BlockingCollection<VerificationDataEntry> _queue;
        private readonly IStorageService<VerificationDataEntry> _storageService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly HttpClient _client = new HttpClient();
        private readonly string _endpoint;

        // Lỗi mạng / timeout → retry vô hạn, backoff: 5s → 10s → 30s → 60s
        private int _networkFailCount = 0;
        private static readonly int[] _retryDelaysMs = { 5000, 10000, 30000, 60000 };

        // Lỗi API (WMS trả code != 200) → giới hạn số lần retry
        private int _apiFailCount = 0;
        private const int MaxApiFailsBeforeSkip = 5;

        // Carton bị skip trong phiên hiện tại (API fail 5 lần) — restart sẽ retry
        private readonly HashSet<string> _skippedCartons = new HashSet<string>();

        // Timer giảm log [CARGO WAIT] — chỉ log mỗi 10s
        private DateTime _lastCargoWaitLog = DateTime.MinValue;

        // ─── Constructor ──────────────────────────────────────────────────────────
        public WokaCargoSender(
            BlockingCollection<VerificationDataEntry> queue,
            IStorageService<VerificationDataEntry> storageService,
            string endpoint)
        {
            _queue = queue;
            _storageService = storageService;
            _endpoint = endpoint;
            _client.Timeout = TimeSpan.FromSeconds(30);
            UpdateAuthHeader();
        }

        // ─── Public ───────────────────────────────────────────────────────────────
        public void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        // Tìm carton đã được storage map xong nhưng chưa gửi WOKA (bỏ qua carton đã skip 5 lần)
                        var currentCarton = Shared.CurrentJob.CartonList
                            .FirstOrDefault(x => x.IsSent && !x.IsCodeCartonSent && !_skippedCartons.Contains(x.QrCode));

                        if (currentCarton != null)
                            await SendCartonAsync(currentCarton);

                        await Task.Delay(100);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[CARGO CRASHED] WokaCargoSender dừng bất thường", ex);
                }
            }, _cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
            _queue.CompleteAdding();
        }

        // ─── Process ──────────────────────────────────────────────────────────────
        private async Task SendCartonAsync(CartonModel carton)
        {
            // ─── Kiểm tra mạng trước khi gửi ─────────────────────────────────────
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                await WaitForNetworkAsync(carton.QrCode);
                if (_cts.IsCancellationRequested) return;
            }

            // ─── Đọc mã của carton từ AllValues ──────────────────────────────────
            var codesOfCarton = GetCartonCodesFromAllValues(carton.QrCode);
            if (codesOfCarton.Count == 0)
            {
                if (DateTime.Now - _lastCargoWaitLog > TimeSpan.FromSeconds(10))
                {
                    ProjectLogger.WriteWarning(
                        $"[CARGO WAIT] carton={carton.QrCode} | chưa có mã trong AllValues, thử lại sau");
                    _lastCargoWaitLog = DateTime.Now;
                }
                return;
            }

            // ─── Ràng buộc: chỉ gửi khi đủ quy cách ──────────────────────────
            int palletSize = Shared.CurrentJob.NumberOfCodesInPallet;
            if (codesOfCarton.Count != palletSize)
            {
                if (DateTime.Now - _lastCargoWaitLog > TimeSpan.FromSeconds(10))
                {
                    ProjectLogger.WriteWarning(
                        $"[CARGO WAIT] carton={carton.QrCode} | codes={codesOfCarton.Count}/{palletSize}, chờ đủ quy cách mới gửi");
                    _lastCargoWaitLog = DateTime.Now;
                }
                return;
            }

            ProjectLogger.WriteInfo(
                $"[CARGO START] cargoCode={carton.QrCode} | codeCount={codesOfCarton.Count}");

            var rawCodes = new List<string>();
            var qrCodes = new List<string>();

            foreach (var code in codesOfCarton)
            {
                // Transform 1 lần duy nhất khi gửi API: raw → \x1D → base64
                qrCodes.Add(WokaDataNormalizer.ToWokaApiFormat(code));
                rawCodes.Add(code);
            }

            var request = new RequestCargo
            {
                cargoCode = carton.QrCode,
                qrCodeList = qrCodes,
                note = "Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            string url = Shared.Settings.ApiUrl + "/cargos";
            string sentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            LogCargoRequest(url, carton.QrCode, request, sentAt, rawCodes);

            // ─── Gửi HTTP ─────────────────────────────────────────────────────────
            bool isSuccess = false;
            string failType = "";
            string failDetail = "";

            try
            {
                isSuccess = await SendCargoRequestAsync(request, carton.QrCode);
                if (!isSuccess)
                {
                    failType = "API_FAILED";
                    failDetail = "Server returned code != 200";
                }
            }
            catch (TaskCanceledException tcEx)
            {
                failType = "TIMEOUT";
                failDetail = tcEx.Message;
            }
            catch (HttpRequestException httpEx)
            {
                failType = "NETWORK_ERROR";
                failDetail = httpEx.Message;
            }
            catch (Exception ex)
            {
                failType = "ERROR";
                failDetail = ex.Message;
            }

            // ─── Xử lý thất bại ──────────────────────────────────────────────────
            if (!isSuccess)
            {
                bool isNetworkError = failType == "TIMEOUT"
                   || failType == "NETWORK_ERROR";

                if (isNetworkError)
                {
                    _networkFailCount++;
                    _apiFailCount = 0;
                    int delayMs = _retryDelaysMs[Math.Min(_networkFailCount - 1, _retryDelaysMs.Length - 1)];
                    ProjectLogger.WriteError(
                        $"[CARGO {failType}] attempt=#{_networkFailCount} | cargoCode={carton.QrCode} | detail={failDetail} | retryAfter={delayMs}ms | policy=retry-vô-hạn");
                    await SafeDelayAsync(delayMs, carton.QrCode);
                }
                else
                {
                    _apiFailCount++;
                    _networkFailCount = 0;

                    if (_apiFailCount >= MaxApiFailsBeforeSkip)
                    {
                        ProjectLogger.WriteError(
                            $"[CARGO SKIP BATCH] Đã thử {_apiFailCount} lần, bỏ qua carton phiên này | cargoCode={carton.QrCode} | detail={failDetail}");
                        WriteCartonAuditLog(carton.QrCode, rawCodes, "SKIPPED_MAX_API_RETRY", failDetail);
                        _skippedCartons.Add(carton.QrCode);
                        _apiFailCount = 0;
                    }
                    else
                    {
                        int delayMs = _retryDelaysMs[Math.Min(_apiFailCount - 1, _retryDelaysMs.Length - 1)];
                        ProjectLogger.WriteError(
                            $"[CARGO {failType}] attempt=#{_apiFailCount}/{MaxApiFailsBeforeSkip} | cargoCode={carton.QrCode} | detail={failDetail} | retryAfter={delayMs}ms");
                        await SafeDelayAsync(delayMs, carton.QrCode);
                    }
                }
                return;
            }

            // ─── Thành công ───────────────────────────────────────────────────────
            _networkFailCount = 0;
            _apiFailCount = 0;

            carton.IsCodeCartonSent = true;
            Shared.CurrentJob.SaveFile();

            int remainingUnsent = Shared.CurrentJob.CartonList.Count(x => !x.IsCodeCartonSent);
            ProjectLogger.WriteInfo(
                $"[CARGO SUCCESS] cargoCode={carton.QrCode} | sent={codesOfCarton.Count} | remainingUnsent={remainingUnsent}");

            WriteCartonAuditLog(carton.QrCode, rawCodes, "SUCCESS", "");

            Shared.RaiseOnSyncDataParameterChangeEvent(new SyncDataParams(SyncDataType.SyncCodeInCarton));
        }

        // ─── HTTP ─────────────────────────────────────────────────────────────────
        private async Task<bool> SendCargoRequestAsync(RequestCargo request, string cargoQr)
        {
            UpdateAuthHeader();

            string url = Shared.Settings.ApiUrl + "/cargos";
            var stopwatch = Stopwatch.StartNew();

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, url))
            {
                httpRequest.Content = new StringContent(
                    JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                using (var response = await _client.SendAsync(httpRequest, _cts.Token))
                {
                    stopwatch.Stop();
                    int statusCode = (int)response.StatusCode;
                    bool httpSuccess = statusCode >= 200 && statusCode < 300;
                    string receivedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    string responseContent = "";
                    if (response.Content != null)
                    {
                        try
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                                responseContent = await reader.ReadToEndAsync();
                        }
                        catch (Exception readEx)
                        {
                            ProjectLogger.WriteWarning(
                                $"[CARGO READ] Không đọc được response | cargoCode={cargoQr} | {readEx.Message}");
                        }
                    }

                    LogCargoResponse(cargoQr, statusCode, responseContent, stopwatch.ElapsedMilliseconds, receivedAt);

                    if (string.IsNullOrEmpty(responseContent))
                        return httpSuccess;

                    try
                    {
                        var parsed = JsonConvert.DeserializeObject<ResponseCargo>(responseContent);
                        if (parsed == null) return httpSuccess;
                        if (parsed.code == 200) return true;

                        ProjectLogger.WriteError(
                            $"[CARGO API FAIL] code={parsed.code} | msg={parsed.message} | cargoCode={cargoQr}");
                        return false;
                    }
                    catch (Exception jsonEx)
                    {
                        ProjectLogger.WriteWarning(
                            $"[CARGO JSON PARSE FAIL] {jsonEx.Message} | cargoCode={cargoQr} | body={responseContent}");
                        return httpSuccess;
                    }
                }
            }
        }

        // ─── Network Recovery ─────────────────────────────────────────────────────
        private async Task WaitForNetworkAsync(string cargoQr)
        {
            ProjectLogger.WriteWarning(
                $"[CARGO OFFLINE] Mất kết nối mạng – tạm dừng gửi, chờ phục hồi | cargoCode={cargoQr}");

            int elapsedSec = 0;
            while (!NetworkInterface.GetIsNetworkAvailable() && !_cts.IsCancellationRequested)
            {
                // Log mỗi 30 giây để tránh spam
                if (elapsedSec % 30 == 0)
                    ProjectLogger.WriteWarning(
                        $"[CARGO OFFLINE] Vẫn chờ mạng... | elapsed={elapsedSec}s | cargoCode={cargoQr}");

                elapsedSec++;
                try { await Task.Delay(1000, _cts.Token); }
                catch (OperationCanceledException) { return; }
            }

            if (!_cts.IsCancellationRequested)
            {
                _networkFailCount = 0; // reset backoff khi mạng phục hồi
                ProjectLogger.WriteInfo(
                    $"[CARGO ONLINE] Mạng phục hồi – tiếp tục gửi ngay | cargoCode={cargoQr} | waitedSec={elapsedSec}");
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────
        // Trong WaitForNetworkAsync hoặc sau khi _networkFailCount lớn
        private async Task SafeDelayAsync(int delayMs, string cargoQr)
        {
            // Cảnh báo leo thang: sau 10 phút liên tiếp thất bại
            if (_networkFailCount == 10) // ~10 * 60s = 10 phút
            {
                ProjectLogger.WriteError(
                    $"[CARGO ALERT] ⚠️ Server không phản hồi hơn 10 phút! | cargoCode={cargoQr} | totalAttempts={_networkFailCount}");
                ProjectLogger.WriteWarning(
                    "[CARGO ALERT] Kiểm tra kết nối mạng và trạng thái WMS server ngay!");
            }

            try { await Task.Delay(delayMs, _cts.Token); }
            catch (OperationCanceledException)
            {
                ProjectLogger.WriteInfo(
                    $"[CARGO RETRY CANCEL] Stop() được gọi trong lúc chờ retry | cargoCode={cargoQr}");
            }
        }

        /// <summary>
        /// Đọc danh sách QR code của một carton từ AllValues file.
        /// Lấy các dòng có qr_pallet == carton QR và status == "mapped".
        /// Dùng Shared.WokaAllValueProcess để tránh tạo nhiều instance xung đột file.
        /// </summary>
        private List<string> GetCartonCodesFromAllValues(string cartonQr)
        {
            const int maxRetries = 3;
            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    if (Shared.WokaAllValueProcess == null)
                    {
                        ProjectLogger.WriteWarning($"[CARGO ALLVALUES] Shared.WokaAllValueProcess đang null, bỏ qua lần đọc này | carton={cartonQr}");
                        if (retry < maxRetries - 1) Thread.Sleep(1000);
                        continue;
                    }

                    var allValues = Shared.WokaAllValueProcess.GetAllValuePayload();
                    if (allValues?.qr_list == null || allValues.qr_list.Count == 0)
                    {
                        if (retry < maxRetries - 1) Thread.Sleep(1000);
                        continue;
                    }

                    return allValues.qr_list
                        .Where(q => q.qr_pallet == cartonQr && q.status == "mapped")
                        .Select(q => q.qrcode_value)
                        .ToList();
                }
                catch (IOException)
                {
                    if (retry < maxRetries - 1) Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError($"[CARGO ALLVALUES] Lỗi đọc AllValues cho carton={cartonQr}", ex);
                    if (retry < maxRetries - 1) Thread.Sleep(1000);
                }
            }
            return new List<string>();
        }

        private void LogCargoRequest(string url, string cargoQr, RequestCargo request, string sentAt, List<string> rawCodes)
        {
            try
            {
                ProjectLogger.WriteInfo(
                    $"[CARGO REQUEST] endpoint={url} | cargoCode={cargoQr} | codeCount={rawCodes.Count} | sentAt={sentAt}");
                ProjectLogger.WriteDebug(
                    $"[CARGO REQUEST PAYLOAD] cargoCode={cargoQr} | payload={JsonConvert.SerializeObject(request)}");
                string codeList = string.Join(" | ", rawCodes.Select((c, i) => $"[{i + 1}]{c}"));
                ProjectLogger.WriteDebug(
                    $"[CARGO CODE LIST] cargoCode={cargoQr} | codes={codeList}");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteWarning($"[CARGO LOG] Lỗi ghi request log: {ex.Message}");
            }
        }

        private void LogCargoResponse(string cargoQr, int statusCode, string body, long elapsedMs, string receivedAt)
        {
            string httpResult = statusCode >= 200 && statusCode < 300 ? "OK" : "FAIL";
            ProjectLogger.WriteInfo(
                $"[CARGO RESPONSE] cargoCode={cargoQr} | statusCode={statusCode}({httpResult}) | responseTime={elapsedMs}ms | receivedAt={receivedAt}");
            if (!string.IsNullOrEmpty(body))
                ProjectLogger.WriteDebug(
                    $"[CARGO RESPONSE BODY] cargoCode={cargoQr} | body={body}");
        }

        /// <summary>
        /// Audit log theo từng thùng (carton): ghi danh sách mã đã gửi để đối soát sau này.
        /// Prefix [CARGO AUDIT] dễ grep/filter trong log file.
        /// </summary>
        private void WriteCartonAuditLog(string cargoQr, List<string> rawCodes, string status, string failReason)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string reasonPart = string.IsNullOrEmpty(failReason) ? "" : $" | failReason={failReason}";
                ProjectLogger.WriteInfo(
                    $"[CARGO AUDIT] cargoCode={cargoQr} | status={status} | codeCount={rawCodes.Count} | at={timestamp}{reasonPart}");
                string codeList = string.Join(" | ", rawCodes.Select((c, i) => $"[{i + 1}]{c}"));
                ProjectLogger.WriteDebug(
                    $"[CARGO AUDIT CODES] cargoCode={cargoQr} | codes={codeList}");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteWarning($"[CARGO AUDIT] Lỗi ghi audit log: {ex.Message}");
            }
        }

        private void UpdateAuthHeader()
        {
            if (string.IsNullOrEmpty(Shared.Settings.WokaToken)) return;
            if (_client.DefaultRequestHeaders.Contains("app_info"))
                _client.DefaultRequestHeaders.Remove("app_info");
            _client.DefaultRequestHeaders.Add("app_info", Shared.Settings.WokaToken);
        }
    }
}


//    public class WokaCargoSender : ISenderService<VerificationDataEntry>
// ... (phần code cũ đã comment giữ nguyên bên dưới)


//    public class WokaCargoSender : ISenderService<VerificationDataEntry>
//    {
//        private readonly BlockingCollection<VerificationDataEntry> _queue;
//        private readonly IStorageService<VerificationDataEntry> _storageService;
//        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
//        private WokaApiService apiService = new WokaApiService();
//        HttpClient _client = new HttpClient();

//        //private ApiService apiService = new ApiService();

//        private readonly string _endpoint;
//        private readonly string _databasePath;

//        public WokaCargoSender(BlockingCollection<VerificationDataEntry> queue, IStorageService<VerificationDataEntry> storageService, string endpoint)
//        {
//            _queue = queue;
//            _storageService = storageService;
//            _endpoint = endpoint;

//            // Set the header once during initialization
//            if (!string.IsNullOrEmpty(Shared.Settings.WokaToken))
//            {
//                if (!_client.DefaultRequestHeaders.Contains("app_info"))
//                {
//                    _client.DefaultRequestHeaders.Add("app_info", Shared.Settings.WokaToken);
//                }
//            }
//        }

//        public void Start()
//        {
//            Task.Run(async () =>
//            {
//                try
//                {
//                    while (!_cts.IsCancellationRequested)
//                    {
//                        if (_queue.Count != 0 && Shared.CurrentJob.CartonList.Count(x => x.IsCodeCartonSent == false) > 0)
//                        {
//                            await ProcessEntryAsync(null); // process but DO NOT DELETE
//                        }

//                        await Task.Delay(1);
//                    }
//                }
//                catch (OperationCanceledException)
//                {
//                    // Normal cancellation, ignore
//                }
//                catch (Exception ex)
//                {
//                    ProjectLogger.WriteError("Error occurred in WokaCargoSender background task: " + ex.Message);
//                }
//            }, _cts.Token);
//        }

//        private async Task<string> ProcessEntryAsync(VerificationDataEntry entry)
//        {
//            var storageUpdate = new StorageUpdate();

//            int palletSize = Shared.CurrentJob.NumberOfCodesInPallet;
//            if (_queue.Count < palletSize) return ""; // && palletQueue.Count > 0

//            string Now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//            List<string> qr_codes = new List<string>();
//            List<VerificationDataEntry> temp_qr_codes = new List<VerificationDataEntry>();

//            var CurrentCarton = Shared.CurrentJob.CartonList.FirstOrDefault(x => x.IsCodeCartonSent == false);

//            try
//            {
//                var snapshot = _queue.ToArray();

//                for (int i = 0; i < palletSize && i < snapshot.Length; i++)
//                {
//                    // Replace "\\F" with Group Separator (ASCII 29) and encode to Base64
//                    string code = snapshot[i].Code;
//                    if (code.Contains("\\F"))
//                    {
//                        code = code.Replace("\\F", "\x1D"); // Replace with Group Separator (GS)
//                    }
//                    string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
//                    qr_codes.Add(base64Code);
//                    temp_qr_codes.Add(snapshot[i]);
//                }

//                var request = new RequestCargo()
//                {
//                    cargoCode = CurrentCarton.QrCode,
//                    qrCodeList = qr_codes,
//                    note = "Date: " + Now
//                };

//                // Send request to server
//                bool isSuccess = false;
//                string responseContent = "";
//                int statusCode = 0;

//                // Ensure the header is set (update if token changed)
//                if (!string.IsNullOrEmpty(Shared.Settings.WokaToken))
//                {
//                    if (_client.DefaultRequestHeaders.Contains("app_info"))
//                    {
//                        _client.DefaultRequestHeaders.Remove("app_info");
//                    }
//                    _client.DefaultRequestHeaders.Add("app_info", Shared.Settings.WokaToken);
//                }

//                using (var testrequest = new HttpRequestMessage(HttpMethod.Post, Shared.Settings.ApiUrl + "/cargos"))
//                {

//                    //if (!string.IsNullOrEmpty(Shared.Settings.WokaToken))
//                    //{
//                    //    testrequest.Headers.Authorization =
//                    //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Shared.Settings.WokaToken);
//                    //}

//                    var jsonPayload = JsonConvert.SerializeObject(request);
//                    testrequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

//                    using (var responsee = await _client.SendAsync(testrequest))
//                    {
//                        // Check HTTP status code first (should be 201 Created)
//                        statusCode = (int)responsee.StatusCode;
//                        bool httpSuccess = statusCode >= 200 && statusCode < 300;

//                        if (httpSuccess)
//                        {
//                            ProjectLogger.WriteInfo($"HTTP Status OK - Status: {statusCode} ({responsee.StatusCode}) for cargo: {CurrentCarton?.QrCode}");
//                        }
//                        else
//                        {
//                            ProjectLogger.WriteError($"HTTP Status FAILED - Status: {statusCode} ({responsee.StatusCode}) for cargo: {CurrentCarton?.QrCode}");
//                        }

//                        // Note: isSuccess will be set after parsing JSON response

//                        // Read response content using ReadAsStreamAsync
//                        if (responsee.Content != null)
//                        {
//                            try
//                            {
//                                using (var stream = await responsee.Content.ReadAsStreamAsync())
//                                using (var reader = new StreamReader(stream, Encoding.UTF8))
//                                {
//                                    responseContent = await reader.ReadToEndAsync();
//                                    ProjectLogger.WriteInfo($"Response content read successfully. Length: {responseContent?.Length ?? 0} bytes for cargo: {CurrentCarton?.QrCode}");

//                                    if (!string.IsNullOrEmpty(responseContent))
//                                    {
//                                        ProjectLogger.WriteInfo($"Response content: {responseContent}");
//                                    }
//                                }
//                            }
//                            catch (Exception readEx)
//                            {
//                                ProjectLogger.WriteWarning($"Could not read response content ({readEx.GetType().Name}): {readEx.Message} for cargo: {CurrentCarton?.QrCode}");
//                                if (readEx.InnerException != null)
//                                {
//                                    ProjectLogger.WriteWarning($"InnerException: {readEx.InnerException.GetType().Name} - {readEx.InnerException.Message}");
//                                }
//                                // Continue even if reading fails - HTTP status check will determine success
//                            }
//                        }

//                        // Parse JSON response and check the code field
//                        if (!string.IsNullOrEmpty(responseContent))
//                        {
//                            try
//                            {
//                                var responseCargo = JsonConvert.DeserializeObject<ResponseCargo>(responseContent);
//                                if (responseCargo != null)
//                                {
//                                    ProjectLogger.WriteInfo($"Parsed JSON response - code: {responseCargo.code}, message: {responseCargo.message} for cargo: {CurrentCarton?.QrCode}");

//                                    // Success only if HTTP status is OK AND JSON response code is 200
//                                    if (responseCargo.code == 200)
//                                    {
//                                        isSuccess = true;
//                                        ProjectLogger.WriteInfo($"Cargo request SUCCESS - HTTP: {statusCode}, JSON code: {responseCargo.code} for cargo: {CurrentCarton?.QrCode}");
//                                    }
//                                    else
//                                    {
//                                        isSuccess = false;
//                                        ProjectLogger.WriteError($"Cargo request FAILED - HTTP: {statusCode}, JSON code: {responseCargo.code}, message: {responseCargo.message} for cargo: {CurrentCarton?.QrCode}");
//                                    }
//                                }
//                                else
//                                {
//                                    ProjectLogger.WriteWarning($"Failed to parse JSON response - responseCargo is null for cargo: {CurrentCarton?.QrCode}");
//                                    // Fall back to HTTP status check
//                                    isSuccess = httpSuccess;
//                                }
//                            }
//                            catch (Exception jsonEx)
//                            {
//                                ProjectLogger.WriteWarning($"Failed to parse JSON response: {jsonEx.Message} for cargo: {CurrentCarton?.QrCode}");
//                                ProjectLogger.WriteWarning($"Response content: {responseContent}");
//                                // Fall back to HTTP status check
//                                isSuccess = httpSuccess;
//                            }
//                        }
//                        else
//                        {
//                            // No response content, use HTTP status as indicator
//                            ProjectLogger.WriteWarning($"No response content received. Using HTTP status as indicator for cargo: {CurrentCarton?.QrCode}");
//                            isSuccess = httpSuccess;
//                        }
//                    }
//                }

//                // Only process success if request was successful
//                if (!isSuccess)
//                {
//                    ProjectLogger.WriteError($"Cargo request failed for: {CurrentCarton?.QrCode}. HTTP Status: {statusCode}, Response: {responseContent}");
//                    return "";
//                }


//                // ResponseCargo response = await apiService.PostCargoAsync(request);


//                temp_qr_codes.ForEach(qr_code => {
//                    storageUpdate = new StorageUpdate()
//                    {
//                        Id = qr_code.Id,
//                        VerifiedStatus = qr_code.VerifiedStatus,
//                        VerifiedDate = qr_code.VerifiedDate,
//                        SaaSStatus = "success",
//                        SaaSError = "",
//                    };
//                    _storageService.MarkCodeWithCartonAsSent(storageUpdate);
//                    _queue.Take();
//                });

//                CurrentCarton.IsCodeCartonSent = true;
//                Shared.CurrentJob.SaveFile();

//                var SyncDataParams = new SyncDataParams(SyncDataType.SyncCodeInCarton) { };
//                Shared.RaiseOnSyncDataParameterChangeEvent(SyncDataParams);

//                //ShowDataMessage.SavePayloadToFile(request);

//                //ResponseCargo response = await apiService.PostCargoAsync(request);

//                //// Handle null response (API call failed)
//                //if (response == null)
//                //{
//                //    ProjectLogger.WriteError($"API call failed - null response for cargo: {CurrentCarton?.QrCode}");
//                //    temp_qr_codes.ForEach(qr_code =>
//                //    {
//                //        storageUpdate = new StorageUpdate()
//                //        {
//                //            Id = qr_code.Id,
//                //            VerifiedStatus = qr_code.VerifiedStatus,
//                //            VerifiedDate = qr_code.VerifiedDate,
//                //            SaaSStatus = "failed",
//                //            SaaSError = "API call failed - no response",
//                //        };
//                //        _storageService.MarkCodeWithCartonAsFailed(storageUpdate);
//                //    });
//                //    return "";
//                //}

//                //if (response.code == 200)
//                //{
//                //    temp_qr_codes.ForEach(qr_code =>
//                //    {
//                //        storageUpdate = new StorageUpdate()
//                //        {
//                //            Id = qr_code.Id,
//                //            VerifiedStatus = qr_code.VerifiedStatus,
//                //            VerifiedDate = qr_code.VerifiedDate,
//                //            SaaSStatus = "success",
//                //            SaaSError = response.message,
//                //        };
//                //        _storageService.MarkCodeWithCartonAsSent(storageUpdate);
//                //        _queue.Take();
//                //    });
//                //    CurrentCarton.IsCodeCartonSent = true;
//                //    Shared.CurrentJob.SaveFile();

//                //    var SyncDataParams = new SyncDataParams(SyncDataType.SyncCodeInCarton) { };
//                //    Shared.RaiseOnSyncDataParameterChangeEvent(SyncDataParams);
//                //}
//                //else
//                //{
//                //    temp_qr_codes.ForEach(qr_code =>
//                //    {
//                //        storageUpdate = new StorageUpdate()
//                //        {
//                //            Id = qr_code.Id,
//                //            VerifiedStatus = qr_code.VerifiedStatus,
//                //            VerifiedDate = qr_code.VerifiedDate,
//                //            SaaSStatus = "failed",
//                //            SaaSError = response.message,
//                //        };
//                //        _storageService.MarkCodeWithCartonAsFailed(storageUpdate);
//                //    });
//                //    ProjectLogger.WriteError($"Error in exception: __" + "Response false" + "___" + ApiModelWoka.getCargorUrl() + "_" + CurrentCarton.QrCode);

//                //}

//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error in exception: __" + "Response false" + "___" + ApiModelWoka.getCargorUrl() + "_" + CurrentCarton.QrCode);
//            }

//            return "";
//        }

//        public void Stop()
//        {
//            _cts.Cancel();
//            _queue.CompleteAdding();
//        }
//    }

//}
