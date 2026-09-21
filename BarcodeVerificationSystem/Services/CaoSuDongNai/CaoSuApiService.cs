using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.Apis.CaoSuDongNai;
using BarcodeVerificationSystem.Model.CaoSuDongNai;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Response;
using BarcodeVerificationSystem.Services.CaoSuDongNai.Interface;
using BarcodeVerificationSystem.Services.Interface;
using BarcodeVerificationSystem.Utils;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static BarcodeVerificationSystem.Model.Payload.DispatchingPayload.ResponseOrder;

namespace BarcodeVerificationSystem.Services.CaoSuDongNai
{
    public class CaoSuApiService : ICaoSuApiService
    {
        private ApiService apiService = new ApiService();
        public async Task<ResponseAllProducts> GetAllProductsAsync()
        {
            return (await apiService.GetApiDataAsync<ResponseAllProducts>(ApiModel.getAllProductsUrl())).Data;
        }

        public async Task<ResponseUpdateCodeCheck> PostCodeCheckAsync(RequestUpdateCodeCheck payload)
        {
            return await apiService.PostApiDataAsync<ResponseUpdateCodeCheck>(ApiModel.getUpdateCodeCheckUrl(), payload);
        }

        public async Task<ResponseBasic> PostCodePrintAsync(RequestUpdateCodePrint payload)
        {
         
            string jsonPayload = "";
            jsonPayload = JsonConvert.SerializeObject(payload);
            string url = ApiModel.getUpdateCodePrintUrl();
            // ── Log trước khi gửi ─────────────────────────────────────────
            //ProjectLogger.WriteInfo(
            //    $"[PostCodePrintAsync] ► POST {url}\n" +
            //    $"  AccessToken : {(string.IsNullOrEmpty(Shared.Settings.AccessToken) ? "MISSING!" : Shared.Settings.AccessToken.Substring(0, Math.Min(20, Shared.Settings.AccessToken.Length)) + "...")}\n" +
            //    $"  qr_list     : {payload?.qr_list?.Count ?? 0} items\n" +
            //    $"  REQUEST BODY: {jsonPayload}");
        
            return await apiService.PostApiDataAsync<ResponseBasic>(ApiModel.getUpdateCodePrintUrl(), payload);


        }

        public async Task<ResponseLogin> PostLoginAsync(RequestLogin payload)
        {
            var result = await apiService.PostApiDataAsync<ResponseLogin>(ApiModel.getLoginUrl(), payload);

            return result;
        }

        public async Task<ResponsePushDatabase> PostPushDatabaseAsync(RequestPushDatabase payload)
        {
            return await apiService.PostApiDataAsync<ResponsePushDatabase>(ApiModel.getPushDatabaseUrl(), payload);
        }

        //public async Task<ResponseSyncOffline> PostSyncOfflineAsync(RequestSyncOffline payload)
        //{
        //    return await apiService.PostApiDataAsync<ResponseSyncOffline>(ApiModel.getUpdatedSyncOffline(), payload);
        //}
        public async Task<ResponseSyncOffline> PostSyncOfflineAsync(RequestSyncOffline payload)
        {
            string url = ApiModel.getUpdatedSyncOffline();
            string jsonPayload = "";

            try
            {
                jsonPayload = JsonConvert.SerializeObject(payload);

                // ── Log trước khi gửi ─────────────────────────────────────────
                //ProjectLogger.WriteInfo(
                //    $"[PostSyncOffline] ► POST {url}\n" +
                //    $"  AccessToken : {(string.IsNullOrEmpty(Shared.Settings.AccessToken) ? "MISSING!" : Shared.Settings.AccessToken.Substring(0, Math.Min(20, Shared.Settings.AccessToken.Length)) + "...")}\n" +
                //    $"  qr_list     : {payload?.qr_list?.Count ?? 0} items\n" +
                //    $"  REQUEST BODY: {jsonPayload}");

                // ── Dùng HttpClient riêng, tránh static dirty state ───────────
                using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) })
                {
                    if (!string.IsNullOrEmpty(Shared.Settings.AccessToken))
                    {
                        httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue(
                                "Bearer", Shared.Settings.AccessToken);
                    }

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url, content);
                    var body = await response.Content.ReadAsStringAsync();

                    // ── Log sau khi nhận ──────────────────────────────────────
                    ProjectLogger.WriteInfo(
                        $"[PostSyncOffline] ◄ {(int)response.StatusCode} {response.StatusCode}\n" +
                        $"  RESPONSE BODY: {body}");

                    if (!response.IsSuccessStatusCode)
                    {
                        return new ResponseSyncOffline
                        {
                            success = false,
                            message = $"HTTP {(int)response.StatusCode}: {body}"
                        };
                    }

                    var result = JsonConvert.DeserializeObject<ResponseSyncOffline>(body);
                    return result ?? new ResponseSyncOffline { success = false, message = "Deserialize trả về null" };
                }
            }
            catch (TaskCanceledException)
            {
                ProjectLogger.WriteError($"[PostSyncOffline] ✗ TIMEOUT sau 60s — {url}");
                return new ResponseSyncOffline { success = false, message = "Request timeout (60s)" };
            }
            catch (HttpRequestException ex)
            {
                ProjectLogger.WriteError($"[PostSyncOffline] ✗ HTTP ERROR — {url}\n  {ex.Message}");
                return new ResponseSyncOffline { success = false, message = "HTTP Error: " + ex.Message };
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[PostSyncOffline] ✗ EXCEPTION — {url}\n  REQUEST: {jsonPayload}\n  ERROR: {ex.Message}");
                return new ResponseSyncOffline { success = false, message = ex.Message };
            }
        }
        public async Task<ResponseRefreshToken> PostRefreshTokenAsync()
        {
            var _httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(Shared.Settings.RefreshToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Shared.Settings.RefreshToken);
            }

            var response = await _httpClient.PostAsync(ApiModel.getRefreshTokenUrl(), null);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<ResponseRefreshToken>(responseContent);
            return result;
        }


        public async Task<ResponseBasic> PostUpdatePalletAsync(RequestUpdatePallet payload)
        {
            return await apiService.PostApiDataAsync<ResponseBasic>(ApiModel.getUpdatePalletUrl(), payload);
        }
    }

}
