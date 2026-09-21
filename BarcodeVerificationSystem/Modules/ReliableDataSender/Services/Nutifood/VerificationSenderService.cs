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

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services
{
    public class VerificationSenderService : ISenderService<VerificationDataEntry>
    {
        private readonly BlockingCollection<VerificationDataEntry> _queue;
        private readonly IStorageService<VerificationDataEntry> _storageService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ApiService apiService = new ApiService();
        private readonly string _endpoint;
        private readonly string _databasePath;

        public VerificationSenderService(BlockingCollection<VerificationDataEntry> queue, IStorageService<VerificationDataEntry> storageService, string endpoint)
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
                string filePath = CommVariables.PathJobsApp + Shared.CurrentJob.FileName + Shared.Settings.JobFileExtension;
                var request = new RequestChecked();

                if (Shared.CurrentJob.IsProcessOrderMode)
                {
                    var payload = Shared.CurrentJob.ProcessOrderItem;

                    request = new RequestChecked
                    {
                        index_qr_code = entry.Id,
                        qr_code = entry.Code,
                        process_order = payload.process_order,
                        material_number = payload.material_number,
                        check_date = entry.VerifiedDate,
                        status = entry.VerifiedStatus,
                        print_type = "process_order",
                        batch = payload.batch_info[Shared.Settings.SelectedBatchIndex].batch,
                        mauf_date = payload.batch_info[Shared.Settings.SelectedBatchIndex].mauf_date,
                        expired_date = payload.batch_info[Shared.Settings.SelectedBatchIndex].expired_date,
                    };
                }

                if(Shared.CurrentJob.IsReservationMode)
                {
                    var payload = Shared.CurrentJob.ReservationItem;
                    request = new RequestChecked
                    {
                        index_qr_code = entry.Id,
                        qr_code = entry.Code,
                        material_number = payload.material_number,
                        check_date = entry.VerifiedDate,
                        status = entry.VerifiedStatus,
                        material_doc = Shared.CurrentJob.Reservation.material_doc,
                        print_type = "reservation",
                        batch = payload.batch,
                        mauf_date = payload.mauf_date,
                        expired_date = payload.expired_date,
                    };
                }
                //new Form
                //{
                //    Text = "JSON Viewer",
                //    Width = 800,
                //    Height = 600,
                //    Controls = { new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both,
                // Text = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented) } }
                //}.ShowDialog();


                if (Shared.UserPermission.isOnline)
                {
                    var ResponseChecked = await apiService.PostApiDataAsync<ResponseChecked>(_endpoint, request);

                    entry.SaasStatus = ResponseChecked.is_success ? "success" : "failed";
                    entry.SAPStatus = ResponseChecked.is_success_sap ? "success" : "failed";
                    entry.SaasError = ResponseChecked.message ?? string.Empty;
                    entry.SAPError = ResponseChecked.message_sap ?? string.Empty;

                    var syncDataModel = new SyncDataParams(SyncDataParams.SyncDataType.SentData, entry.Id) { };

                    if (ResponseChecked.is_success)
                    {
                        //Shared.CurrentJob.NumberOfCheckSaaSSentCodes++;
                        //Shared.CurrentJob.SaveFile(filePath); // Có thể không save ở đây nhưng khi đọc job phải đọc file lên và đếm lại.

                        syncDataModel.DataType = SyncDataParams.SyncDataType.SaaSSuccess;
                        Shared.RaiseOnSyncCheckDataParameterChangeEvent(syncDataModel);
                    }
                    if (ResponseChecked.is_success_sap)
                    {
                        //Shared.CurrentJob.NumberOfCheckSAPSentCodes++;
                        //Shared.CurrentJob.SaveFile(filePath);
                        syncDataModel.DataType = SyncDataParams.SyncDataType.SAPSuccess;
                        Shared.RaiseOnSyncCheckDataParameterChangeEvent(syncDataModel);
                    }


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

                    if(ResponseChecked.is_success && entry.VerifiedStatus != "Valid")
                    {
                        _storageService.MarkAsSent(storageUpdate);
                        syncDataModel.DataType = SyncDataParams.SyncDataType.SentSuccess;
                        Shared.RaiseOnSyncCheckDataParameterChangeEvent(syncDataModel);
                    }
                    else if (ResponseChecked.is_success && ResponseChecked.is_success_sap) // nho chinh khuc nay
                    {
                        _storageService.MarkAsSent(storageUpdate);
                        syncDataModel.DataType = SyncDataParams.SyncDataType.SentSuccess;
                        Shared.RaiseOnSyncCheckDataParameterChangeEvent(syncDataModel);
                    }
                    else
                    {
                        _storageService.MarkAsFailed(storageUpdate);
                        _queue.Add(entry);
                    }

                    if (!ResponseChecked.is_success_sap)
                    {
                        ProjectLogger.WriteError($"Error occurred in {_endpoint}): " + entry.VerifiedDate + entry.SaasStatus + entry.SAPStatus + entry.SaasError + entry.SAPError);
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
                    SaaSError = "R-Link Processing Error", // ex.Message
                    SAPError = entry.SAPError
                };

                ProjectLogger.WriteError($"R-Link Processing Error {_endpoint}): " + entry.VerifiedDate + entry.SaasStatus + entry.SAPStatus + ex.Message + entry.SAPError + ex.Message);
                //_storageService.AppendEntry(entry);
                _storageService.MarkAsFailed(storageUpdate);
                _queue.Add(entry);
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _queue.CompleteAdding();
        }
    }
}
