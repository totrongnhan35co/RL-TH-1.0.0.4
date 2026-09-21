using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using BarcodeVerificationSystem.Controller;
using Newtonsoft.Json.Linq;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Services;
using CommonVariable;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using static BarcodeVerificationSystem.Model.Payload.DispatchingPayload.ResponseOrder;
using System.Windows.Forms;
using BarcodeVerificationSystem.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services
{
    public class CaoSuVerificationSender : ISenderService<VerificationDataEntry>
    {
        private readonly BlockingCollection<VerificationDataEntry> _queue;
        private readonly IStorageService<VerificationDataEntry> _storageService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly CaoSuApiService apiService = new CaoSuApiService();
        private readonly string _endpoint;
        private readonly string _databasePath;

        public CaoSuVerificationSender(BlockingCollection<VerificationDataEntry> queue, IStorageService<VerificationDataEntry> storageService, string endpoint)
        {
            _queue = queue;
            _storageService = storageService;
            _endpoint = endpoint;
        }

        public void Start()
        {
            try
            {
                Task.Run(async () =>
                {
                    foreach (var entry in _queue.GetConsumingEnumerable(_cts.Token))
                    {
                        _ = ProcessEntryAsync(entry); // fire-and-forget task
                        await Task.Delay(100); // optional small delay to avoid CPU spike
                    }
                }, _cts.Token);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error occurred in Start PrintingSenderService: " + ex.Message);
            }

        }

        private async Task ProcessEntryAsync(VerificationDataEntry entry)
        {
            var storageUpdate = new StorageUpdate();

            try
            {
                DateTime dt = DateTime.Parse(entry.VerifiedDate);
                var request = new RequestUpdateCodeCheck()
                {
                    production_batch_code = entry.LotNumber ?? string.Empty,
                    product_code = entry.ProductCode ?? string.Empty,
                    weight = entry.Weight ?? string.Empty,
                    qr_list = new List<QrCodeCheck>()
                    {
                        new QrCodeCheck()
                        {
                            qrcode_value = entry.Code,
                            status       = entry.VerifiedStatus.ToLower(),
                            execute_date = dt.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    }
                };

                if (Shared.UserPermission.isOnline)
                {
                    var ResponseChecked = await apiService.PostCodeCheckAsync(request);

                    entry.SaasStatus = ResponseChecked.success ? "success" : "failed";
                    entry.SaasError = ResponseChecked.message ?? string.Empty;

                    storageUpdate = new StorageUpdate()
                    {
                        Id = entry.Id,
                        VerifiedStatus = entry.VerifiedStatus,
                        VerifiedDate = entry.VerifiedDate,
                        SaaSStatus = entry.SaasStatus,
                        SAPStatus = entry.SAPStatus,
                        SaaSError = entry.SaasError,
                        SAPError = entry.SAPError
                    };

                    if (ResponseChecked.success)
                    {
                     
                        _storageService.MarkAsSent(storageUpdate);

                        var syncDataModel = new SyncDataParams(SyncDataParams.SyncDataType.SaaSSuccess, entry.Id);
                        Shared.RaiseOnSyncCheckDataParameterChangeEvent(syncDataModel);

                        syncDataModel.DataType = SyncDataParams.SyncDataType.SentSuccess;
                        Shared.RaiseOnSyncCheckDataParameterChangeEvent(syncDataModel);
                    }
                    else
                    {
                        ProjectLogger.WriteError($"Error occurred in {_endpoint}): " + entry.VerifiedDate + entry.SaasStatus + entry.SAPStatus + entry.SaasError + entry.SAPError);
                        _storageService.MarkAsFailed(storageUpdate);
                        _queue.Add(entry);
                    }
                }
                else
                {
                    _storageService.MarkAsFailed(storageUpdate);
                }
            }
            catch (Exception ex)
            {
                storageUpdate = new StorageUpdate()
                {
                    Id = entry.Id,
                    VerifiedStatus = entry.VerifiedStatus,
                    VerifiedDate = entry.VerifiedDate,
                    SaaSStatus = "failed",
                    SAPStatus = entry.SAPStatus,
                    SaaSError = "R-Link Processing Error",
                    SAPError = entry.SAPError
                };

                ProjectLogger.WriteError($"R-Link Processing Error CaoSuVerificationSender): " + entry.VerifiedDate + entry.SaasStatus + entry.SAPStatus + ex.Message + entry.SAPError);
                _storageService.MarkAsFailed(storageUpdate);
                _queue.Add(entry);
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            //_queue.CompleteAdding();
        }
    }

}
