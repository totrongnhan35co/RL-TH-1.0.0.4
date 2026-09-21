using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using BarcodeVerificationSystem.View;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload;
using System.Windows;
using BarcodeVerificationSystem.Model;
using CommonVariable;
using BarcodeVerificationSystem.Model.Apis.Dispatching;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Windows.Forms;
using BarcodeVerificationSystem.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;
using System.Collections.Generic;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services
{
    public class CaoSuPrintingSender : ISenderService<PrintingDataEntry>
    {
        private readonly BlockingCollection<PrintingDataEntry> _queue;
        private readonly IStorageService<PrintingDataEntry> _storageService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly CaoSuApiService apiService = new CaoSuApiService();

        private string _endpoint;

        public CaoSuPrintingSender(BlockingCollection<PrintingDataEntry> queue, IStorageService<PrintingDataEntry> storageService, string endpoint)
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

        private async Task ProcessEntryAsync(PrintingDataEntry entry)
        {
            var storageUpdate = new StorageUpdate();

            try
            {

                List<string> test = new List<string>();
                test.Add(entry.Code);
                var printedContent = new RequestUpdateCodePrint()
                {
                    qr_list = new List<QrCodePrint>()
                    {
                        new QrCodePrint()
                        {
                            qrcode_value = entry.Code,
                            execute_date = entry.PrintedDate
                        }
                    }
                };

                if (Shared.UserPermission.isOnline)
                {
                    var ResponsePrinted = await apiService.PostCodePrintAsync(printedContent);

                    entry.SaaSStatus = ResponsePrinted.success ? "success" : "failed";
                    entry.SaasError = ResponsePrinted.message ?? string.Empty;
                    var syncDataModel = new SyncDataParams(SyncDataParams.SyncDataType.SentData, entry.Id){};

                    if (ResponsePrinted.success)
                    {
                        Shared.CurrentJob.NumberOfSaaSSentCodes++;
                        Shared.CurrentJob.SaveFile(); // Có thể không save ở đây nhưng khi đọc job phải đọc file lên và đếm lại.
                        syncDataModel.DataType = SyncDataParams.SyncDataType.SaaSSuccess;
                        Shared.RaiseOnSyncDataParameterChangeEvent(syncDataModel);
                    }

                    storageUpdate = new StorageUpdate()
                    {
                        Id = entry.Id,
                        SaaSStatus = entry.SaaSStatus,
                        SAPStatus = entry.SAPStatus,
                        SaaSError = entry.SaasError,
                        SAPError = entry.SAPError,
                        PrintedDate = entry.PrintedDate
                    };

                    if (ResponsePrinted.success) // nho chinh khuc nay
                    {
                        _storageService.MarkAsSent(storageUpdate);
                    }
                    else
                    {
                        _storageService.MarkAsFailed(storageUpdate);
                        _queue.Add(entry);
                    }

                    if (!ResponsePrinted.success)
                    {
                        ProjectLogger.WriteError($"Error occurred in {_endpoint}): " + entry.PrintedDate + entry.SaaSStatus + entry.SAPStatus + entry.SaasError + entry.SAPError);
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
                    SaaSStatus = "failed",
                    SAPStatus = entry.SAPStatus,
                    SaaSError = "R-Link Processing Error", // ex.Message
                    SAPError = entry.SAPError,
                    PrintedDate = entry.PrintedDate
                };

                ProjectLogger.WriteError($"R-Link Processing Error {_endpoint}): " + entry.PrintedDate + entry.SaaSStatus + entry.SAPStatus + ex.Message + entry.SAPError + ex.Message);
                //_storageService.AppendEntry(entry);
                _storageService.MarkAsFailed(storageUpdate);
                _queue.Add(entry);
            }
        }

        public void Stop()
        {
            //_httpClient.Dispose();
            _cts.Cancel();
            //_queue.CompleteAdding();
        }

    }

}
