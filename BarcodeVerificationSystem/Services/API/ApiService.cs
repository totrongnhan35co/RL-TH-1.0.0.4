using BarcodeVerificationSystem.Model.Payload;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using BarcodeVerificationSystem.Services.Interface;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.View.CustomDialogs;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BarcodeVerificationSystem.Controller;
using System.Drawing;
using static BarcodeVerificationSystem.Model.Payload.DispatchingPayload.ResponseOrder;
using BarcodeVerificationSystem.Utils.UI;

namespace BarcodeVerificationSystem.Services
{
    public class ApiService : IApiService
    {
        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Callback để refresh token khi gặp 401.
        /// Trả về true nếu refresh thành công.
        /// </summary>
        public static Func<Task<bool>> RefreshTokenAsync { get; set; }

        private static async Task<bool> TryRefreshAndRetryAsync()
        {
            if (RefreshTokenAsync == null) return false;
            bool refreshed = await RefreshTokenAsync();
            if (refreshed && !string.IsNullOrEmpty(Shared.Settings.AccessToken))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Shared.Settings.AccessToken);
                return true;
            }
            return false;
        }

        public async Task<T> GetApiWithModelAsync<T>(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);

                response.EnsureSuccessStatusCode(); // throws if not 2xx

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var responseContent = await reader.ReadToEndAsync();
                    var result = JsonConvert.DeserializeObject<T>(responseContent);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR getting Api: {url}" + ex.Message);
                return default;
            };
        }

        //public async Task<T> PostApiDataAsync<T>(string apiUrl, object payload)
        //{
        //    if (!string.IsNullOrEmpty(Shared.Settings.AccessToken))
        //    {
        //        _client.DefaultRequestHeaders.Authorization =
        //            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Shared.Settings.AccessToken);
        //    }
        //    var jsonPayload = JsonConvert.SerializeObject(payload);

        //    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        //    //ShowDataMessage.ShowJson(payload);
        //    var response = await _client.PostAsync(apiUrl, content);

        //    response.EnsureSuccessStatusCode();

        //    using (var stream = await response.Content.ReadAsStreamAsync())
        //    using (var reader = new StreamReader(stream, Encoding.UTF8))
        //    {
        //        var responseContent = await reader.ReadToEndAsync();
        //        var result = JsonConvert.DeserializeObject<T>(responseContent);
        //        return result;
        //    }
        //}
        public async Task<T> PostApiDataAsync<T>(string apiUrl, object payload)
        {
            for (int attempt = 0; attempt <= 1; attempt++)
            {
                if (!string.IsNullOrEmpty(Shared.Settings.AccessToken))
                {
                    _client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Shared.Settings.AccessToken);
                }

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    bool isLegacyEndpoint = apiUrl != null &&
                        apiUrl.IndexOf("api/production/checked", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (isLegacyEndpoint)
                        return default(T);

                    if (attempt == 0 && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        if (await TryRefreshAndRetryAsync())
                            continue;
                    }

                    string responseBody = "";
                    try
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                            responseBody = await reader.ReadToEndAsync();
                    }
                    catch { }

                    response.EnsureSuccessStatusCode();
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var responseContent = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
            }

            return default(T);
        }
        public async Task<bool> PostApiDataAsync(string apiUrl, object payload)
        {
            for (int attempt = 0; attempt <= 1; attempt++)
            {
                try
                {
                    if (!string.IsNullOrEmpty(Shared.Settings.AccessToken))
                    {
                        _client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Shared.Settings.AccessToken);
                    }
                    var jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                    using (content)
                    {
                        var response = await _client.PostAsync(apiUrl, content, cts.Token);

                        if (!response.IsSuccessStatusCode)
                        {
                            if (attempt == 0 && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                if (await TryRefreshAndRetryAsync())
                                    continue;
                            }

                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                var responseBody = await reader.ReadToEndAsync();
                                Debug.WriteLine($"HTTP {response.StatusCode}: {responseBody}");
                            }
                        }

                        return response.IsSuccessStatusCode;
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Debug.WriteLine($"[Timeout] Request timed out: {ex.Message}");
                    return false;
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"[HttpRequest] {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Unexpected] {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        public async Task<(bool Success, T Data)> GetApiDataAsync<T>(string apiUrl)
        {
            for (int attempt = 0; attempt <= 1; attempt++)
            {
                try
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                    {
                        if (!string.IsNullOrEmpty(Shared.Settings.AccessToken))
                        {
                            _client.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Shared.Settings.AccessToken);
                        }

                        var response = await _client.GetAsync(apiUrl, cts.Token);

                        if (!response.IsSuccessStatusCode)
                        {
                            if (attempt == 0 && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                if (await TryRefreshAndRetryAsync())
                                    continue;
                            }

                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                var responseBody = await reader.ReadToEndAsync();
                                Debug.WriteLine($"HTTP {response.StatusCode}: {responseBody}");
                            }
                            return (false, default);
                        }

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            var jsonResponse = await reader.ReadToEndAsync();
                            var data = JsonConvert.DeserializeObject<T>(jsonResponse);
                            return (true, data);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Debug.WriteLine($"[Timeout] Request timed out: {ex.Message}");
                    return (false, default);
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"[HttpRequest] {ex.Message}");
                    return (false, default);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Unexpected] {ex.Message}");
                    return (false, default);
                }
            }

            return (false, default);
        }
        //public async Task<(bool Success, T Data)> GetApiDataAsync<T>(string apiUrl)
        //{
        //    try
        //    {
        //        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
        //        {
        //            var response = await _client.GetAsync(apiUrl, cts.Token);

        //            if (!response.IsSuccessStatusCode)
        //            {
        //                var responseBody = await response.Content.ReadAsStringAsync();
        //                Debug.WriteLine($"HTTP {response.StatusCode}: {responseBody}");
        //                return (false, default);
        //            }

        //            var jsonResponse = await response.Content.ReadAsStringAsync();
        //            var data = JsonConvert.DeserializeObject<T>(jsonResponse);
        //            return (true, data);
        //        }
        //    }
        //    catch (TaskCanceledException ex)
        //    {
        //        Debug.WriteLine($"[Timeout] Request timed out: {ex.Message}");
        //        return (false, default);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        Debug.WriteLine($"[HttpRequest] {ex.Message}");
        //        return (false, default);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"[Unexpected] {ex.Message}");
        //        return (false, default);
        //    }
        //}

        public async Task<T> PostCompressedDataAsync<T>(string url, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var bytes = Encoding.UTF8.GetBytes(json);

                using (var ms = new MemoryStream())
                {
                    using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                    {
                        gzip.Write(bytes, 0, bytes.Length);
                    }

                    var compressedBytes = ms.ToArray();
                    var content = new ByteArrayContent(compressedBytes);
                    content.Headers.ContentEncoding.Add("gzip");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await _client.PostAsync(url, content);
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var responseContent = await reader.ReadToEndAsync();
                        Console.WriteLine(responseContent);

                        var sb = new StringBuilder();
                        var result = JsonConvert.DeserializeObject<T>(responseContent);
                        return result;
                    } 
                }
            }
            catch (Exception ex)
            {
            }
            return default;
        }

        public void Dispose()
        {
            _client.Dispose();
            //GC.SuppressFinalize(this);

        }


    }
}


//public async Task<JArray> GetApiDataAsync(string apiUrl)
//{
//    var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
//    //request.Headers.Add("Content-Type", "application/json");

//    HttpResponseMessage response = await _client.SendAsync(request);
//    response.EnsureSuccessStatusCode();

//    string content = await response.Content.ReadAsStringAsync();
//    JObject json = JObject.Parse(content);

//    if ((string)json["status"] == "success")
//    {
//        return (JArray)json["data"];
//    }
//    else
//    {
//        throw new Exception("API returned error status.");
//    }
//}
