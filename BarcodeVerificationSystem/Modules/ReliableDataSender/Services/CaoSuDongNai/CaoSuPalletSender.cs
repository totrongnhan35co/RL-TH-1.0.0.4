using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Interfaces;
using BarcodeVerificationSystem.Modules.ReliableDataSender.Models;
using BarcodeVerificationSystem.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Utils;
using CommonVariable;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Response;
using BarcodeVerificationSystem.Utils.UI;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.CaoSuDongNai
{
    public class CaoSuPalletSender : ISenderService<VerificationDataEntry>
    {
        private readonly BlockingCollection<VerificationDataEntry> _queue;
        private readonly IStorageService<VerificationDataEntry> _storageService;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly CaoSuApiService apiService = new CaoSuApiService();
        private readonly string _endpoint;
        private readonly string _databasePath;

        public CaoSuPalletSender(BlockingCollection<VerificationDataEntry> queue, IStorageService<VerificationDataEntry> storageService, string endpoint)
        {
            _queue = queue;
            _storageService = storageService;
            _endpoint = endpoint;
        }

        //public void Start()
        //{
        //    try
        //    {
        //        Task.Run(async () =>
        //        {
        //            while (!_cts.IsCancellationRequested)
        //            {
        //                if (_queue.Count != 0 && Shared.CurrentJob.PalletList.Count(x => x.IsSent == false) > 0)
        //                {
        //                    await ProcessEntryAsync(null); // process but DO NOT DELETE
        //                }

        //                await Task.Delay(200);
        //            }
        //        }, _cts.Token);
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError("Error occurred in Start PrintingSenderService: " + ex.Message);
        //    }

        //}
        public void Start()
        {
            try
            {
                Task.Run(async () =>
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        try
                        {
                            double weight = Shared.CurrentJob?.productWeight ?? 0;
                            int palletSize = GetPalletSize(weight, Shared.CurrentJob?.NumberTotalsCode ?? 0);

                            // Kiểm tra đủ điều kiện trước khi vào ProcessEntryAsync
                            bool hasEnoughCodes = _queue.Count >= palletSize;
                            bool hasPallet = Shared.CurrentJob?.PalletList?
                                                .Any(x => x.IsSent == false) == true;

                            if (hasEnoughCodes && hasPallet)
                            {
                                await ProcessEntryAsync(null);
                            }
                        }
                        catch (Exception ex)
                        {
                            ProjectLogger.WriteError("CaoSuPalletSender loop error: " + ex.Message);
                        }

                        await Task.Delay(200);
                    }
                }, _cts.Token);
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error occurred in Start CaoSuPalletSender: " + ex.Message);
            }
        }
        //private async Task<string> ProcessEntryAsync(VerificationDataEntry entry)
        //{
        //    var storageUpdate = new StorageUpdate();

        //    double weight = Shared.CurrentJob.productWeight;
        //    int palletSize = weight > 30 ? 36 : 60;
        //    if (_queue.Count < palletSize) return ""; // && palletQueue.Count > 0

        //    string Now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    List<string> qr_codes = new List<string>();
        //    List<VerificationDataEntry> temp_qr_codes = new List<VerificationDataEntry>();
        //    var CurrentPallet = Shared.CurrentJob.PalletList.LastOrDefault(x => x.IsSent == false);

        //    try
        //    {
        //        var snapshot = _queue.ToArray();

        //        for (int i = 0; i < palletSize && i < snapshot.Length; i++)
        //        {
        //            qr_codes.Add(snapshot[i].Code);
        //            temp_qr_codes.Add(snapshot[i]);
        //            if (Shared.CaoSuAllValueProcess == null)
        //                Shared.CaoSuAllValueProcess = new CaoSuAllValueProcess(Shared.CurrentJob);
        //            Shared.CaoSuAllValueProcess.UpdatePallet(snapshot[i].Code, CurrentPallet.QrCode, Now);
        //        }

        //        var request = new RequestUpdatePallet()
        //        {
        //            qr_code_pallet = CurrentPallet.QrCode,
        //            qr_code = qr_codes.Select(code => new QrCode()
        //            {
        //                qrcode_value = code,
        //                execute_date = Now
        //            }).ToList()
        //        };
        //        //ShowDataMessage.SavePayloadToFile(request);

        //        if (!Shared.UserPermission.isOnline)
        //        {
        //            temp_qr_codes.ForEach(qr_code => {
        //                storageUpdate = new StorageUpdate()
        //                {
        //                    Id = qr_code.Id,
        //                    VerifiedStatus = qr_code.VerifiedStatus,
        //                    VerifiedDate = qr_code.VerifiedDate,
        //                    SaaSStatus = "success",
        //                    SaaSError = "Stored Offline",
        //                };
        //                _storageService.MarkCodeWithPalletAsSent(storageUpdate);
        //                _queue.Take();
        //            });
        //            CurrentPallet.IsSent = true;
        //            Shared.CurrentJob.SaveFile();
        //            return "";
        //        }

        //        ResponseBasic response = await apiService.PostUpdatePalletAsync(request);

        //        if (response.success) {
        //            temp_qr_codes.ForEach(qr_code => {
        //                storageUpdate = new StorageUpdate()
        //                {
        //                    Id = qr_code.Id,
        //                    VerifiedStatus = qr_code.VerifiedStatus,
        //                    VerifiedDate = qr_code.VerifiedDate,
        //                    SaaSStatus = "success",
        //                    SaaSError = response.message,
        //                };
        //                _storageService.MarkCodeWithPalletAsSent(storageUpdate);
        //                _queue.Take();
        //            });
        //            CurrentPallet.IsSent = true;
        //            Shared.CurrentJob.SaveFile();

        //            var syncDataModel = new SyncDataParams(SyncDataParams.SyncDataType.SentData, 1) { };
        //            syncDataModel.DataType = SyncDataParams.SyncDataType.PalletSuccess;
        //            Shared.RaiseOnSyncDataParameterChangeEvent(syncDataModel);
        //        }
        //        else
        //        {
        //            temp_qr_codes.ForEach(qr_code =>
        //            {
        //                storageUpdate = new StorageUpdate()
        //                {
        //                    Id = qr_code.Id,
        //                    VerifiedStatus = qr_code.VerifiedStatus,
        //                    VerifiedDate = qr_code.VerifiedDate,
        //                    SaaSStatus = "failed",
        //                    SaaSError = response.message,
        //                };
        //                _storageService.MarkCodeWithPalletAsFailed(storageUpdate);
        //            });
        //            ProjectLogger.WriteError($"Error occurred in {_endpoint}): " + Now + "_" + response.message + "_" + CurrentPallet.QrCode);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError($"Error occurred in {_endpoint}): " + Now + "_" + ex.Message + "_" + CurrentPallet.QrCode);
        //    }

        //    return "";
        //}
        private async Task<string> ProcessEntryAsync(VerificationDataEntry entry)
        {
            var storageUpdate = new StorageUpdate();

            // Tính palletSize từ NumberTotalsCode — không dùng weight vì có thể = 0
            // NumberTotalsCode = tổng mã của 1 pallet (được set khi tạo job)
            double weight = Shared.CurrentJob?.productWeight ?? 0;
            int palletSize = GetPalletSize(weight, Shared.CurrentJob?.NumberTotalsCode ?? 0);

            if (_queue.Count < palletSize) return "";

            var CurrentPallet = Shared.CurrentJob?.PalletList?.LastOrDefault(x => x.IsSent == false);
            if (CurrentPallet == null) return "";

            string Now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            List<VerificationDataEntry> temp_qr_codes = new List<VerificationDataEntry>();

            try
            {
                for (int i = 0; i < palletSize; i++)
                {
                    if (_queue.TryTake(out var item))
                    {
                        temp_qr_codes.Add(item);
                        if (Shared.CaoSuAllValueProcess == null)
                            Shared.CaoSuAllValueProcess = new CaoSuAllValueProcess(Shared.CurrentJob);
                        Shared.CaoSuAllValueProcess.UpdatePallet(item.Code, CurrentPallet.QrCode, Now);
                    }
                }

                if (temp_qr_codes.Count == 0) return "";

                var request = new RequestUpdatePallet()
                {
                    qr_code_pallet = CurrentPallet.QrCode,
                    qr_code = temp_qr_codes.Select(qr => new QrCode()
                    {
                        qrcode_value = qr.Code,
                        execute_date = Now
                    }).ToList()
                };

                if (!Shared.UserPermission.isOnline)
                {
                    temp_qr_codes.ForEach(qr =>
                    {
                        _storageService.MarkCodeWithPalletAsSent(new StorageUpdate()
                        {
                            Id = qr.Id,
                            VerifiedStatus = qr.VerifiedStatus,
                            VerifiedDate = qr.VerifiedDate,
                            SaaSStatus = "success",
                            SaaSError = "Stored Offline",
                        });
                    });
                    CurrentPallet.IsSent = true;
                    Shared.CurrentJob.SaveAsCheckedFile();
                    // Raise event để UI cập nhật CodeWithPallet (giống online)
                    var syncDataModel = new SyncDataParams(SyncDataParams.SyncDataType.SentData, 1);
                    syncDataModel.DataType = SyncDataParams.SyncDataType.PalletSuccess;
                    Shared.RaiseOnSyncDataParameterChangeEvent(syncDataModel);

                    return "";
                }


                ResponseBasic response = await apiService.PostUpdatePalletAsync(request);

                if (response.success)
                {
                    temp_qr_codes.ForEach(qr =>
                    {
                        _storageService.MarkCodeWithPalletAsSent(new StorageUpdate()
                        {
                            Id = qr.Id,
                            VerifiedStatus = qr.VerifiedStatus,
                            VerifiedDate = qr.VerifiedDate,
                            SaaSStatus = "success",
                            SaaSError = response.message,
                        });
                    });
                    CurrentPallet.IsSent = true;
                    Shared.CurrentJob.SaveAsCheckedFile();

                    var syncDataModel = new SyncDataParams(SyncDataParams.SyncDataType.SentData, 1);
                    syncDataModel.DataType = SyncDataParams.SyncDataType.PalletSuccess;
                    Shared.RaiseOnSyncDataParameterChangeEvent(syncDataModel);
                }
                else
                {
                    temp_qr_codes.ForEach(qr =>
                    {
                        _storageService.MarkCodeWithPalletAsFailed(new StorageUpdate()
                        {
                            Id = qr.Id,
                            VerifiedStatus = qr.VerifiedStatus,
                            VerifiedDate = qr.VerifiedDate,
                            SaaSStatus = "failed",
                            SaaSError = response.message,
                        });
                        _queue.Add(qr);
                    });
                    ProjectLogger.WriteError($"Pallet failed [{CurrentPallet.QrCode}]: {response.message}");
                }
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"Pallet exception [{CurrentPallet?.QrCode}]: {ex.Message}");
            }

            return "";
        }

        /// <summary>
        /// Tính palletSize đúng:
        /// - Nếu weight hợp lệ (> 0): dùng weight
        /// - Nếu weight = 0 nhưng NumberTotalsCode > 0: dùng NumberTotalsCode làm palletSize
        /// - Fallback: 36
        /// </summary>
        //private int GetPalletSize(double weight, double numberTotalsCode)
        //{
        //    if (weight > 0)
        //        return weight > 30 ? 36 : 60;

        //    if (numberTotalsCode > 0)
        //        return (int)numberTotalsCode;

        //    return 36; // fallback mặc định
        //}

        /// <summary>
        /// Tính số mã tối thiểu để xử lý 1 pallet:
        /// - weight > 30 kg → 36 mã/pallet
        /// - weight > 0 và <= 30 kg → 60 mã/pallet
        /// - weight = 0 → mặc định 36 mã/pallet
        /// KHÔNG dùng NumberTotalsCode (= tổng toàn job, ví dụ 144) vì sẽ gộp tất cả vào 1 pallet.
        /// </summary>
        private int GetPalletSize(double weight, double numberTotalsCode)
        {
            if (weight > 30) return 36;
            if (weight > 0) return 60;
            return 36; // fallback mặc định — KHÔNG dùng numberTotalsCode
        }
        public void Stop()
        {
            _cts.Cancel();
            _queue.CompleteAdding();
        }
    }

}
