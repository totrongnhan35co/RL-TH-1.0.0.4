using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.CaoSuDongNai;
using BarcodeVerificationSystem.Model.Droco.Request;
using BarcodeVerificationSystem.Model.Droco.Response;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Services.Droco;
using BarcodeVerificationSystem.Utils.CodeGeneration;
using BarcodeVerificationSystem.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BarcodeVerificationSystem.Model.SyncDataParams;
using System.Windows;
using BarcodeVerificationSystem.Model.Apis.Droco;
using BarcodeVerificationSystem.Services;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco
{
    public class DrocoCargoSender : ISenderService<VerificationDataEntry>
    {
        private readonly BlockingCollection<VerificationDataEntry> _queue;
        private readonly IStorageService<VerificationDataEntry> _storageService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private DrocoApiService apiService = new DrocoApiService();
        HttpClient _client = new HttpClient();

        private readonly string _endpoint;
        private readonly string _databasePath;

        public DrocoCargoSender(BlockingCollection<VerificationDataEntry> queue, IStorageService<VerificationDataEntry> storageService, string endpoint)
        {
            _queue = queue;
            _storageService = storageService;
            _endpoint = endpoint;

            if (!string.IsNullOrEmpty(Shared.Settings.DrocoToken))
            {
                if (!_client.DefaultRequestHeaders.Contains("app_info"))
                {
                    _client.DefaultRequestHeaders.Add("app_info", Shared.Settings.DrocoToken);
                }
            }
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        if (_queue.Count != 0 && Shared.CurrentJob.CartonList.Count(x => x.IsCodeCartonSent == false) > 0)
                        {
                            await ProcessEntryAsync(null);
                        }

                        await Task.Delay(1);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation, ignore
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("Error occurred in DrocoCargoSender background task: " + ex.Message);
                }
            }, _cts.Token);
        }

        private async Task<string> ProcessEntryAsync(VerificationDataEntry entry)
        {
            var storageUpdate = new StorageUpdate();

            int palletSize = Shared.CurrentJob.NumberOfCodesInPallet;
            if (_queue.Count < palletSize) return "";

            string Now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            List<string> qr_codes = new List<string>();
            List<VerificationDataEntry> temp_qr_codes = new List<VerificationDataEntry>();

            var CurrentCarton = Shared.CurrentJob.CartonList.FirstOrDefault(x => x.IsCodeCartonSent == false);

            try
            {
                var snapshot = _queue.ToArray();

                for (int i = 0; i < palletSize && i < snapshot.Length; i++)
                {
                    string code = snapshot[i].Code;
                    if (code.Contains("\\F"))
                    {
                        code = code.Replace("\\F", "\x1D");
                    }
                    string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
                    qr_codes.Add(base64Code);
                    temp_qr_codes.Add(snapshot[i]);
                }

                var request = new RequestCargo()
                {
                    cargoCode = CurrentCarton.QrCode,
                    qrCodeList = qr_codes,
                    note = "Date: " + Now
                };

                bool isSuccess = false;
                string responseContent = "";
                int statusCode = 0;

                if (!string.IsNullOrEmpty(Shared.Settings.DrocoToken))
                {
                    if (_client.DefaultRequestHeaders.Contains("app_info"))
                    {
                        _client.DefaultRequestHeaders.Remove("app_info");
                    }
                    _client.DefaultRequestHeaders.Add("app_info", Shared.Settings.DrocoToken);
                }

                using (var testrequest = new HttpRequestMessage(HttpMethod.Post, Shared.Settings.ApiUrl + "/cargos"))
                {
                    var jsonPayload = JsonConvert.SerializeObject(request);
                    testrequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    using (var responsee = await _client.SendAsync(testrequest))
                    {
                        statusCode = (int)responsee.StatusCode;
                        bool httpSuccess = statusCode >= 200 && statusCode < 300;

                        if (httpSuccess)
                        {
                            ProjectLogger.WriteInfo($"HTTP Status OK - Status: {statusCode} ({responsee.StatusCode}) for cargo: {CurrentCarton?.QrCode}");
                        }
                        else
                        {
                            ProjectLogger.WriteError($"HTTP Status FAILED - Status: {statusCode} ({responsee.StatusCode}) for cargo: {CurrentCarton?.QrCode}");
                        }

                        if (responsee.Content != null)
                        {
                            try
                            {
                                using (var stream = await responsee.Content.ReadAsStreamAsync())
                                using (var reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    responseContent = await reader.ReadToEndAsync();
                                    ProjectLogger.WriteInfo($"Response content read successfully. Length: {responseContent?.Length ?? 0} bytes for cargo: {CurrentCarton?.QrCode}");

                                    if (!string.IsNullOrEmpty(responseContent))
                                    {
                                        ProjectLogger.WriteInfo($"Response content: {responseContent}");
                                    }
                                }
                            }
                            catch (Exception readEx)
                            {
                                ProjectLogger.WriteWarning($"Could not read response content ({readEx.GetType().Name}): {readEx.Message} for cargo: {CurrentCarton?.QrCode}");
                                if (readEx.InnerException != null)
                                {
                                    ProjectLogger.WriteWarning($"InnerException: {readEx.InnerException.GetType().Name} - {readEx.InnerException.Message}");
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            try
                            {
                                var responseCargo = JsonConvert.DeserializeObject<ResponseCargo>(responseContent);
                                if (responseCargo != null)
                                {
                                    ProjectLogger.WriteInfo($"Parsed JSON response - code: {responseCargo.code}, message: {responseCargo.message} for cargo: {CurrentCarton?.QrCode}");

                                    if (responseCargo.code == 200)
                                    {
                                        isSuccess = true;
                                        ProjectLogger.WriteInfo($"Cargo request SUCCESS - HTTP: {statusCode}, JSON code: {responseCargo.code} for cargo: {CurrentCarton?.QrCode}");
                                    }
                                    else
                                    {
                                        isSuccess = false;
                                        ProjectLogger.WriteError($"Cargo request FAILED - HTTP: {statusCode}, JSON code: {responseCargo.code}, message: {responseCargo.message} for cargo: {CurrentCarton?.QrCode}");
                                    }
                                }
                                else
                                {
                                    ProjectLogger.WriteWarning($"Failed to parse JSON response - responseCargo is null for cargo: {CurrentCarton?.QrCode}");
                                    isSuccess = httpSuccess;
                                }
                            }
                            catch (Exception jsonEx)
                            {
                                ProjectLogger.WriteWarning($"Failed to parse JSON response: {jsonEx.Message} for cargo: {CurrentCarton?.QrCode}");
                                ProjectLogger.WriteWarning($"Response content: {responseContent}");
                                isSuccess = httpSuccess;
                            }
                        }
                        else
                        {
                            ProjectLogger.WriteWarning($"No response content received. Using HTTP status as indicator for cargo: {CurrentCarton?.QrCode}");
                            isSuccess = httpSuccess;
                        }
                    }
                }

                if (!isSuccess)
                {
                    ProjectLogger.WriteError($"Cargo request failed for: {CurrentCarton?.QrCode}. HTTP Status: {statusCode}, Response: {responseContent}");
                    return "";
                }

                temp_qr_codes.ForEach(qr_code => {
                    storageUpdate = new StorageUpdate()
                    {
                        Id = qr_code.Id,
                        VerifiedStatus = qr_code.VerifiedStatus,
                        VerifiedDate = qr_code.VerifiedDate,
                        SaaSStatus = "success",
                        SaaSError = "",
                    };
                    _storageService.MarkCodeWithCartonAsSent(storageUpdate);
                    _queue.Take();
                });

                CurrentCarton.IsCodeCartonSent = true;
                Shared.CurrentJob.SaveFile();

                var SyncDataParams = new SyncDataParams(SyncDataType.SyncCodeInCarton) { };
                Shared.RaiseOnSyncDataParameterChangeEvent(SyncDataParams);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error in exception: __" + "Response false" + "___" + ApiModelDroco.getCargorUrl() + "_" + CurrentCarton.QrCode);
            }

            return "";
        }

        public void Stop()
        {
            _cts.Cancel();
            _queue.CompleteAdding();
        }
    }

}