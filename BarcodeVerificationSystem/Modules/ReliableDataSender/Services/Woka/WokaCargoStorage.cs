//using BarcodeVerificationSystem.Controller;
//using BarcodeVerificationSystem.Model;
//using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
//using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
//using BarcodeVerificationSystem.Utils;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using BarcodeVerificationSystem.Model.Woka.Request;
//using BarcodeVerificationSystem.Services.Woka;
//using BarcodeVerificationSystem.Model.Woka.Response;
//using BarcodeVerificationSystem.Utils.CodeGeneration;
//using BarcodeVerificationSystem.Model.CaoSuDongNai;
//using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai;
//using BarcodeVerificationSystem.Model.Woka;
//using static BarcodeVerificationSystem.Model.SyncDataParams;
//using Force.DeepCloner;

//namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Woka
//{
//    public class WokaCargoStorage : ISenderService<VerificationDataEntry>
//    {
//        private readonly BlockingCollection<VerificationDataEntry> _queue;
//        private readonly IStorageService<VerificationDataEntry> _storageService;
//        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
//        private WokaApiService apiService = new WokaApiService();
//        private readonly string _endpoint;
//        private readonly string _databasePath;

//        public WokaCargoStorage(BlockingCollection<VerificationDataEntry> queue, IStorageService<VerificationDataEntry> storageService, string endpoint)
//        {
//            _queue = queue;
//            _storageService = storageService;
//            _endpoint = endpoint;
//        }

//        public void Start()
//        {
//            try
//            {
//                Task.Run(async () =>
//                {
//                    while (!_cts.IsCancellationRequested)
//                    {
//                        if (_queue.Count != 0)
//                        {
//                            await ProcessEntryAsync(null); // process but DO NOT DELETE
//                        }

//                        await Task.Delay(1);
//                    }
//                }, _cts.Token);
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error occurred in Start PrintingSenderService: " + ex.Message);
//            }

//        }

//        private async Task<string> ProcessEntryAsync(VerificationDataEntry entry)
//        {
//            var storageUpdate = new StorageUpdate();

//            int palletSize = Shared.CurrentJob.NumberOfCodesInPallet;
//            if (_queue.Count < palletSize) return ""; // && palletQueue.Count > 0

//            string Now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//            List<string> qr_codes = new List<string>();
//            List<VerificationDataEntry> temp_qr_codes = new List<VerificationDataEntry>();
//            if (Shared.WokaAllValueProcess == null)
//                Shared.WokaAllValueProcess = new WokaAllValueProcess(Shared.CurrentJob);

//            if (Shared.CurrentJob.CartonList.Count(x => x.IsSent == false) == 0)
//            {
//                var list = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1);
//                Shared.CurrentJob.CartonList.Add(new CartonModel()
//                {
//                    QrCode = list[0],
//                });
//                Shared.CurrentJob.SaveFile();
//                Shared.RaiseQrCodeCartonChangeEvent();
//            }

//            var CurrentPallet = Shared.CurrentJob.CartonList.LastOrDefault(x => x.IsSent == false);

//            try
//            {
//                var snapshot = _queue.ToArray();

//                for (int i = 0; i < palletSize && i < snapshot.Length; i++)
//                {
//                    qr_codes.Add(snapshot[i].Code);
//                    temp_qr_codes.Add(snapshot[i]);

//                    var code = snapshot[i].DeepClone();
//                    if (code.Code.Contains("\\F"))
//                    {
//                        code.Code = code.Code.Replace("\\F", "\x1D"); // Replace with Group Separator (GS)
//                    }
//                    Shared.WokaAllValueProcess.UpdatePallet(code.Code, CurrentPallet.QrCode, Now);
//                }

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
//                CurrentPallet.IsSent = true;
//                Shared.CurrentJob.SaveFile();

//                var SyncDataParams = new SyncDataParams(SyncDataType.CodeInCarton){};
//                Shared.RaiseOnSyncDataParameterChangeEvent(SyncDataParams);

//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError($"Error occurred in {_endpoint}): " + Now + "_" + ex.Message + "_" + CurrentPallet.QrCode);
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
using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BarcodeVerificationSystem.Model.Woka.Request;
using BarcodeVerificationSystem.Services.Woka;
using BarcodeVerificationSystem.Model.Woka.Response;
using BarcodeVerificationSystem.Utils.CodeGeneration;
using BarcodeVerificationSystem.Model.CaoSuDongNai;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Model.Woka;
using static BarcodeVerificationSystem.Model.SyncDataParams;
using Force.DeepCloner;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Woka
{
    public class WokaCargoStorage : ISenderService<VerificationDataEntry>
    {
        private readonly BlockingCollection<VerificationDataEntry> _queue;
        private readonly IStorageService<VerificationDataEntry> _storageService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly string _endpoint;
        private readonly string _databasePath;

        // Timer giảm log [STORAGE WAIT] — chỉ log mỗi 1s
        private DateTime _lastStorageWaitLog = DateTime.MinValue;

        // Số lần retry cho UpdatePallet và delay giữa các lần retry
        private const int UpdatePalletMaxRetries = 5;
        private const int UpdatePalletRetryDelayMs = 100;

        // Sự kiện báo lỗi nghiêm trọng (ví dụ: không thể map mã sau nhiều lần retry)
        public static event EventHandler<string> OnFatalError;

        public WokaCargoStorage(
            BlockingCollection<VerificationDataEntry> queue,
            IStorageService<VerificationDataEntry> storageService,
            string endpoint)
        {
            _queue = queue;
            _storageService = storageService;
            _endpoint = endpoint;
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
                        if (_queue.Count != 0)
                            await ProcessEntryAsync();

                        await Task.Delay(1);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    ProjectLogger.WriteError("[STORAGE CRASHED] WokaCargoStorage dừng bất thường", ex);
                }
            }, _cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
            _queue.CompleteAdding();
        }

        // ─── Process ──────────────────────────────────────────────────────────────
        private async Task ProcessEntryAsync()
        {
            int palletSize = Shared.CurrentJob.NumberOfCodesInPallet;
            if (palletSize <= 0)
            {
                ProjectLogger.WriteError("[STORAGE ERROR] NumberOfCodesInPallet không hợp lệ");
                return;
            }

            // ── Khởi tạo WokaAllValueProcess nếu chưa có (chỉ 1 lần) ──────────────
            EnsureAllValueProcess();

            // ── Tạo QR carton mới nếu chưa có carton nào chờ xử lý ───────────────
            if (Shared.CurrentJob.CartonList.Count(x => !x.IsSent) == 0)
            {
                var newCodes = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1);
                string newQr = newCodes[0];

                Shared.CurrentJob.CartonList.Add(new CartonModel { QrCode = newQr });
                Shared.CurrentJob.SaveFile();
                Shared.RaiseQrCodeCartonChangeEvent();

                ProjectLogger.WriteInfo(
                    $"[STORAGE QR GENERATED] newCartonQr={newQr} | totalCartons={Shared.CurrentJob.CartonList.Count}");
            }

            var currentPallet = Shared.CurrentJob.CartonList.LastOrDefault(x => !x.IsSent);
            if (currentPallet == null)
            {
                ProjectLogger.WriteError("[STORAGE ERROR] Không tìm được carton chưa gửi sau khi tạo mới");
                return;
            }

            // ── Đếm số mã đã map cho carton đang dở (phục hồi sau crash) ───────────
            int existingCodes = GetMappedCount(currentPallet.QrCode);

            // Bắt buộc chính xác: không cho phép > palletSize
            if (existingCodes > palletSize)
            {
                string msg = $"[STORAGE FATAL] carton={currentPallet.QrCode} | existing={existingCodes} > palletSize={palletSize}. Cần kiểm tra file AllValues.";
                ProjectLogger.WriteError(msg);
                OnFatalError?.Invoke(this, msg);
                Stop();
                return;
            }

            if (existingCodes == palletSize)
            {
                // Recovery: carton đã đầy từ trước nhưng chưa đánh dấu
                ProjectLogger.WriteInfo($"[STORAGE RECOVERY] carton={currentPallet.QrCode} | đã đủ {existingCodes}/{palletSize}, đánh dấu hoàn thành");
                currentPallet.IsSent = true;
                Shared.CurrentJob.SaveFile();
                return;
            }

            int neededCodes = palletSize - existingCodes;
            if (_queue.Count < neededCodes)
            {
                if (DateTime.Now - _lastStorageWaitLog > TimeSpan.FromSeconds(1))
                {
                    ProjectLogger.WriteInfo(
                        $"[STORAGE WAIT] carton={currentPallet.QrCode} | có {existingCodes}/{palletSize}, cần thêm {neededCodes}, queue={_queue.Count}");
                    _lastStorageWaitLog = DateTime.Now;
                }
                return;
            }

            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            ProjectLogger.WriteInfo(
                $"[STORAGE START] queue={_queue.Count} | palletSize={palletSize} | existing={existingCodes} | needed={neededCodes}");

            try
            {
                var snapshot = _queue.ToArray();
                var tempEntries = new List<VerificationDataEntry>();

                // ── Map các mã còn thiếu vào carton, có retry từng mã ──────────────
                for (int i = 0; i < neededCodes && i < snapshot.Length; i++)
                {
                    var entry = snapshot[i];
                    string code = entry.Code;

                    bool mapped = TryUpdatePalletWithRetry(code, currentPallet.QrCode, now);
                    if (!mapped)
                    {
                        string msg = $"[STORAGE FATAL] Không thể map qrcode={code} vào carton={currentPallet.QrCode} sau {UpdatePalletMaxRetries} lần thử. Dừng cargo storage.";
                        ProjectLogger.WriteError(msg);
                        OnFatalError?.Invoke(this, msg);
                        Stop();
                        return;
                    }

                    tempEntries.Add(entry);
                }

                ProjectLogger.WriteInfo(
                    $"[STORAGE PALLET MAPPED] cartonQr={currentPallet.QrCode} | codes={tempEntries.Count}");

                // ── Verify số mã thực tế đã map trong AllValues ──────────────────
                int actualMapped = GetMappedCount(currentPallet.QrCode);
                if (actualMapped != palletSize)
                {
                    ProjectLogger.WriteWarning(
                        $"[STORAGE VERIFY FAILED] carton={currentPallet.QrCode} | actualMapped={actualMapped}/{palletSize}, không drain queue, chờ loop sau");
                    return;
                }

                // ── Đánh dấu đã lưu và drain queue ───────────────────────────────
                tempEntries.ForEach(entry =>
                {
                    var storageUpdate = new StorageUpdate
                    {
                        Id = entry.Id,
                        VerifiedStatus = entry.VerifiedStatus,
                        VerifiedDate = entry.VerifiedDate,
                        SaaSStatus = "success",
                        SaaSError = "",
                    };
                    _storageService.MarkCodeWithCartonAsSent(storageUpdate);
                    _queue.Take();
                });

                currentPallet.IsSent = true;
                Shared.CurrentJob.SaveFile();

                int remainingUnsent = Shared.CurrentJob.CartonList.Count(x => !x.IsSent);
                ProjectLogger.WriteInfo(
                    $"[STORAGE DONE] cartonQr={currentPallet.QrCode} | remainingUnsent={remainingUnsent}");

                Shared.RaiseOnSyncDataParameterChangeEvent(new SyncDataParams(SyncDataType.CodeInCarton));
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError(
                    $"[STORAGE ERROR] cartonQr={currentPallet?.QrCode} | {ex.Message}", ex);
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────
        private void EnsureAllValueProcess()
        {
            if (Shared.WokaAllValueProcess == null)
            {
                Shared.WokaAllValueProcess = new WokaAllValueProcess(Shared.CurrentJob);
                ProjectLogger.WriteInfo($"[STORAGE INIT] WokaAllValueProcess được khởi tạo | path={Shared.WokaAllValueProcess.getFilePath()}");
            }
        }

        private int GetMappedCount(string cartonQr)
        {
            try
            {
                var av = Shared.WokaAllValueProcess?.GetAllValuePayload();
                if (av?.qr_list != null)
                {
                    return av.qr_list.Count(q =>
                        q.qr_pallet == cartonQr && q.status == "mapped");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[STORAGE ERROR] Lỗi đọc AllValues khi đếm mapped count | carton={cartonQr} | {ex.Message}");
            }
            return 0;
        }

        private bool TryUpdatePalletWithRetry(string qrcode, string cartonQr, string mappedDate)
        {
            for (int retry = 0; retry < UpdatePalletMaxRetries; retry++)
            {
                try
                {
                    bool ok = Shared.WokaAllValueProcess.UpdatePallet(qrcode, cartonQr, mappedDate);
                    if (ok) return true;
                }
                catch (Exception ex)
                {
                    ProjectLogger.WriteWarning($"[STORAGE UPDATEPALLET RETRY] qrcode={qrcode} | carton={cartonQr} | retry={retry + 1}/{UpdatePalletMaxRetries} | {ex.Message}");
                }

                if (retry < UpdatePalletMaxRetries - 1)
                    Thread.Sleep(UpdatePalletRetryDelayMs * (retry + 1));
            }
            return false;
        }
    }
}