using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.CaoSuDongNai;
using BarcodeVerificationSystem.Model.Droco;
using BarcodeVerificationSystem.Model.Droco.Request;
using BarcodeVerificationSystem.Model.Droco.Response;
using BarcodeVerificationSystem.Model.Woka;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Services.Droco;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.CodeGeneration;
using Force.DeepCloner;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BarcodeVerificationSystem.Model.SyncDataParams;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco
{
    public class DrocoCargoStorage : ISenderService<VerificationDataEntry>
    {
        private readonly BlockingCollection<VerificationDataEntry> _queue;
        private readonly IStorageService<VerificationDataEntry> _storageService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private DrocoApiService apiService = new DrocoApiService();
        private readonly string _endpoint;
        private readonly string _databasePath;

        public DrocoCargoStorage(BlockingCollection<VerificationDataEntry> queue, IStorageService<VerificationDataEntry> storageService, string endpoint)
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
                    while (!_cts.IsCancellationRequested)
                    {
                        if (_queue.Count != 0)
                        {
                            await ProcessEntryAsync(null); // process but DO NOT DELETE
                        }

                        await Task.Delay(1);
                    }
                }, _cts.Token);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error occurred in Start DrocoCargoStorage: " + ex.Message);
            }

        }

        private async Task<string> ProcessEntryAsync(VerificationDataEntry entry)
        {
            var storageUpdate = new StorageUpdate();

            int palletSize = Shared.CurrentJob.NumberOfCodesInPallet;
            if (_queue.Count < palletSize) return "";

            string Now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            List<string> qr_codes = new List<string>();
            List<VerificationDataEntry> temp_qr_codes = new List<VerificationDataEntry>();
            if (Shared.DrocoAllValueProcess == null)
                Shared.DrocoAllValueProcess = new DrocoAllValueProcess(Shared.CurrentJob);

            if (Shared.CurrentJob.CartonList.Count(x => x.IsSent == false) == 0)
            {
                var list = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1);
                Shared.CurrentJob.CartonList.Add(new CartonModel() { QrCode = list[0] });
                Shared.CurrentJob.SaveFile();
                Shared.RaiseQrCodeCartonChangeEvent();
            }

            var CurrentPallet = Shared.CurrentJob.CartonList.LastOrDefault(x => x.IsSent == false);

            // ✅ Log 1 lần, bên ngoài vòng for
            ProjectLogger.WriteInfo($"[DrocoCargoStorage] ProcessEntryAsync: Pallet='{CurrentPallet?.QrCode}', Queue={_queue.Count}, PalletSize={palletSize}");

            try
            {
                var snapshot = _queue.ToArray();

                for (int i = 0; i < palletSize && i < snapshot.Length; i++)
                {
                    qr_codes.Add(snapshot[i].Code);
                    temp_qr_codes.Add(snapshot[i]);

                    var code = snapshot[i].DeepClone();
                    if (code.Code.Contains("\\F"))
                        code.Code = code.Code.Replace("\\F", "\x1D");

                    Shared.DrocoAllValueProcess.UpdatePallet(code.Code, CurrentPallet.QrCode, Now);
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

                CurrentPallet.IsSent = true;
                Shared.CurrentJob.SaveFile();
                ProjectLogger.WriteInfo($"[DrocoCargoStorage] Pallet '{CurrentPallet.QrCode}' completed with {temp_qr_codes.Count} QR codes.");

                var SyncDataParams = new SyncDataParams(SyncDataType.CodeInCarton) { };
                Shared.RaiseOnSyncDataParameterChangeEvent(SyncDataParams);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[DrocoCargoStorage] Error processing pallet '{CurrentPallet?.QrCode}': {ex.Message}", ex);
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