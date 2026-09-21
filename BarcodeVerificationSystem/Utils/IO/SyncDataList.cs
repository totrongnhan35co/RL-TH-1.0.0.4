using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Labels.ProjectLabel;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Modules.ReliableDataSender.SharedValues;
using CommonVariable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils
{
    internal class SyncDataList
    {
        public int STT { get; set; }
        public string MaCongViec { get; set; }
        public string MaPhieuSoanHang { get; set; }
        public string MaSanPham { get; set; }
        public int SoLuongCanXuat { get; set; }
        public int SoLuongDongBoSaaS { get; set; }
        public string SoLuongDongBoSAP { get; set; }
        public bool Hoanthanh { get; set; }
        public DateTime LastRunTime { get; set; } = DateTime.MinValue;

        public static List<SyncDataList> ReturnSyncDataList(List<string> _JobNameList,
                                                      string basePath = null)
        {
            string resolvedBase = string.IsNullOrWhiteSpace(basePath)
                ? CommVariables.PathJobsApp
                : basePath;

            List<SyncDataList> rows = new List<SyncDataList>();
            foreach (var x in _JobNameList)
            {
                try
                {
                    string fullPath = Path.Combine(resolvedBase.TrimEnd('\\'), x);
                    JobModel CurrentJob = JobModel.LoadFile(fullPath);

                    if (CurrentJob == null) continue;

                    if (ProjectLabel.IsCaoSuDongNai)
                        InitCaoSuProductSyncData(CurrentJob, rows, x);

                    if (ProjectLabel.IsNutrifood)
                        (Shared.Settings.IsManufacturingMode
                            ? (Action)(() => InitManufacturingSyncData(CurrentJob, rows, x))
                            : () => InitDispatchingSyncData(CurrentJob, rows, x))();
                }
                catch (Exception) { }
            }
            return rows;
        }
       private static int GetSentOrNotSentCountFromPathSentDataPrinted(string filePath)
{
    int count = 0;
    if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
    {
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        for (int i = 0; i < lines.Length; i++)
        {
            // Bỏ qua dòng header nếu có
            if (i == 0 && lines[0].ToLower().Contains("sentstatus")) continue;

            var cols = lines[i].Split(',');
            if (cols.Length > 8)
            {
                string status = cols[8].Trim();
                if (status.Equals("Sent", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("NotSent", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                  
                }
            }
        }
        
    }
    else
    {
        ProjectLogger.WriteInfo($"[SyncDataList] File not found or empty: {filePath}");
    }
    return count;
}
        private static void InitCaoSuProductSyncData(JobModel CurrentJob, List<SyncDataList> rows, string x)
        {
            // Dùng tên file thực tế (x) để detect JobCheck
            string fileNameOnly = Path.GetFileNameWithoutExtension(x);
            bool isJobCheck = fileNameOnly.StartsWith("CHECK_", StringComparison.OrdinalIgnoreCase);

            // ── SoLuongCanXuat: số mã in đã đồng bộ ─────────────────────────
            // ── SoLuongCanXuat: số mã in đã đồng bộ ─────────────────────────
            int soLuongIn = 0;

            if (!isJobCheck)
            {
                // TRƯỜNG HỢP JOB PRINT: Đếm trực tiếp từ log CSV
                if (!string.IsNullOrWhiteSpace(CurrentJob.PrintedResponePath))
                {
                    string sentDataPath = Path.Combine(CommVariables.PathSentDataPrinted, CurrentJob.PrintedResponePath);
                    var sentPrinted = ReadPrintedCodeData(sentDataPath);
                    soLuongIn = sentPrinted.Count(item =>
                        item.Length > PrintingValues.SaaSStatus &&
                        item[PrintingValues.SaaSStatus].Equals("success", StringComparison.OrdinalIgnoreCase));
                }

                // Fallback: lấy từ field đã lưu
                if (soLuongIn == 0)
                    soLuongIn = CurrentJob.NumberOfSaaSSentCodes;
            }
            else
            {
                if (CurrentJob.IsJobOnline)
                {
                    // ONLINE: đọc trực tiếp từ CSV để lấy giá trị thực tế
                    if (!string.IsNullOrWhiteSpace(CurrentJob.PrintedResponePath))
                    {
                        string sentDataPath = Path.Combine(
                            CommVariables.PathSentDataPrinted, CurrentJob.PrintedResponePath);
                        var sentPrinted = ReadPrintedCodeData(sentDataPath);
                        soLuongIn = sentPrinted.Count(item =>
                            item.Length > PrintingValues.SaaSStatus &&
                            item[PrintingValues.SaaSStatus].Equals("success", StringComparison.OrdinalIgnoreCase));
                    }

                    // Fallback nếu CSV chưa có dữ liệu
                    if (soLuongIn == 0)
                        soLuongIn = CurrentJob.NumberOfSaaSSentCodes;
                }
                else
                {
                    // OFFLINE: lấy từ field đã lưu trong .rvis
                    // Chưa sync → NumberOfSaaSSentCodes = 0 → đúng thực tế
                    soLuongIn = CurrentJob.NumberOfSaaSSentCodes;
                }
            }

            // ── SoLuongDongBoSaaS: số mã đã đồng bộ check ───────────────────
            int soLuongCheck = 0;
            if (isJobCheck && !string.IsNullOrWhiteSpace(CurrentJob.CheckedResultPath))
            {
                string sentCheckPath = Path.Combine(CommVariables.PathSentDataChecked, CurrentJob.CheckedResultPath);
                var sentChecked = ReadPrintedCodeData(sentCheckPath);
                soLuongCheck = sentChecked.Count(item =>
                    item.Length > VerifyingValues.SaaSStatus &&
                    item[VerifyingValues.SaaSStatus].Equals("success", StringComparison.OrdinalIgnoreCase));
            }

            if (soLuongCheck == 0)
                soLuongCheck = CurrentJob.NumberOfCheckSaaSSentCodes;

            // ── SoLuongDongBoSAP: cột pallet ─────────────────────────────────
            string soLuongDongBoSAP;
            bool hoanthanh;
            if (!isJobCheck)
            {
                soLuongDongBoSAP = "JobPrint";
                hoanthanh = false;
            }
            else
            {
                int soLuongPallet = 0;
                if (!string.IsNullOrWhiteSpace(CurrentJob.CheckedResultPath))
                {
                    string sentPalletPath = Path.Combine(CommVariables.PathSentDataPallet, CurrentJob.CheckedResultPath);
                    var sentPallet = ReadPrintedCodeData(sentPalletPath);
                    soLuongPallet = sentPallet.Count(item =>
                        item.Length > VerifyingValues.SentStatus &&
                        item[VerifyingValues.SaaSStatus].Equals("success", StringComparison.OrdinalIgnoreCase) &&
                        !item[VerifyingValues.SaaSError].Equals("Stored Offline", StringComparison.OrdinalIgnoreCase) &&
                        item[VerifyingValues.SentStatus].Equals("Sent", StringComparison.OrdinalIgnoreCase));
                }

                if (soLuongPallet == 0)
                    soLuongPallet = CurrentJob.NumberOfCodesInPallet;

                int totalMa = GetPalletTotal(CurrentJob);
                soLuongPallet = Math.Min(soLuongPallet, totalMa);

                soLuongDongBoSAP = $"{soLuongPallet} / {totalMa}";
                hoanthanh = totalMa > 0 && soLuongPallet >= totalMa;
            }

            rows.Add(new SyncDataList
            {
                STT = 1,
                MaCongViec = x,
                MaPhieuSoanHang = CurrentJob.LOTNumber,
                MaSanPham = CurrentJob.CaoSuProduct?.product_code
                                    ?? CurrentJob.LastCheckedProductCode ?? "",
                SoLuongCanXuat = soLuongIn,
                SoLuongDongBoSaaS = soLuongCheck,
                SoLuongDongBoSAP = soLuongDongBoSAP,
                Hoanthanh = hoanthanh,
                LastRunTime = CurrentJob.LastRunTime
            });
        }

        /// <summary>
        /// Tính tổng số mã max trên 1 pallet (chỉ dùng cho JobCheck).
        ///   productWeight > 30kg         → 144 mã
        ///   productWeight ≤ 30kg         → 240 mã
        ///   productWeight = 0, PrintJobWeightRange = ">30"  → 144
        ///   productWeight = 0, PrintJobWeightRange = "<=30" → 240
        ///   fallback → NumberTotalsCode, rồi 144
        /// </summary>
        private static int GetPalletTotal(JobModel job)
        {
            // Chỉ có 2 giá trị: 144 (weight > 30kg) hoặc 240 (weight ≤ 30kg)
            double weight = job.productWeight;
            if (weight > 0)
                return weight > 30 ? 144 : 240;

            string weightRange = job.PrintJobWeightRange ?? "";
            if (weightRange.Equals(">30", StringComparison.OrdinalIgnoreCase))
                return 144;

            // Mặc định: <=30 → 240
            return 240;
        }

        private static void InitManufacturingSyncData(JobModel CurrentJob, List<SyncDataList> rows, string x)
        {
            if (CurrentJob.IsProcessOrderMode)
                InitProcessOrderData(CurrentJob, rows, x);

            if (CurrentJob.IsReservationMode)
                InitReservationSyncData(CurrentJob, rows, x);
        }

        private static void InitReservationSyncData(JobModel CurrentJob, List<SyncDataList> rows, string x)
        {
            var payload = CurrentJob.ReservationItem;
            string sentDataPath = CommVariables.PathSentDataPrinted + CurrentJob.PrintedResponePath;
            var _SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);

            CurrentJob.NumberOfSAPSentCodes =
            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 &&
                            item[5].Equals("success", StringComparison.OrdinalIgnoreCase));

            CurrentJob.NumberOfSaaSSentCodes =
            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 &&
                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));

            rows.Add(new SyncDataList
            {
                STT = 1,
                MaCongViec = x,
                MaPhieuSoanHang = CurrentJob.Reservation.material_doc,
                MaSanPham = payload.material_number,
                SoLuongCanXuat = CurrentJob.NumberOfPrintedCodes,
                SoLuongDongBoSaaS = CurrentJob.NumberOfSaaSSentCodes,
                SoLuongDongBoSAP = CurrentJob.NumberOfSAPSentCodes.ToString(),
                Hoanthanh = CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes,
                LastRunTime = CurrentJob.LastRunTime
            });
        }

        private static void InitProcessOrderData(JobModel CurrentJob, List<SyncDataList> rows, string x)
        {
            var payload = CurrentJob.ProcessOrderItem;
            string sentDataPath = CommVariables.PathSentDataPrinted + CurrentJob.PrintedResponePath;
            var _SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);

            CurrentJob.NumberOfSAPSentCodes =
            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 &&
                            item[5].Equals("success", StringComparison.OrdinalIgnoreCase));

            CurrentJob.NumberOfSaaSSentCodes =
            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 &&
                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));

            rows.Add(new SyncDataList
            {
                STT = 1,
                MaCongViec = x,
                MaPhieuSoanHang = payload?.process_order,
                MaSanPham = payload.material_number,
                SoLuongCanXuat = CurrentJob.NumberOfPrintedCodes,
                SoLuongDongBoSaaS = CurrentJob.NumberOfSaaSSentCodes,
                SoLuongDongBoSAP = CurrentJob.NumberOfSAPSentCodes.ToString(),
                Hoanthanh = CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes,
                LastRunTime = CurrentJob.LastRunTime
            });
        }

        private static void InitDispatchingSyncData(JobModel CurrentJob, List<SyncDataList> rows, string x)
        {
            var payload = CurrentJob.DispatchingOrderPayload.payload;
            string sentDataPath = CommVariables.PathSentDataPrinted + CurrentJob.PrintedResponePath;
            var _SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);

            CurrentJob.NumberOfSAPSentCodes =
            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 &&
                            item[5].Equals("success", StringComparison.OrdinalIgnoreCase));

            CurrentJob.NumberOfSaaSSentCodes =
            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 &&
                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));

            rows.Add(new SyncDataList
            {
                STT = 1,
                MaCongViec = x,
                MaPhieuSoanHang = payload?.wms_number,
                MaSanPham = (payload?.items != null
                                             && CurrentJob.SelectedMaterialIndex >= 0
                                             && CurrentJob.SelectedMaterialIndex < payload.items.Count)
                                            ? payload.items[CurrentJob.SelectedMaterialIndex]?.material_number
                                            : (CurrentJob?.ProcessOrder?.material_number ?? ""),
                SoLuongCanXuat = CurrentJob.NumberOfPrintedCodes,
                SoLuongDongBoSaaS = CurrentJob.NumberOfSaaSSentCodes,
                SoLuongDongBoSAP = CurrentJob.NumberOfSAPSentCodes.ToString(),
                Hoanthanh = CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes,
                LastRunTime = CurrentJob.LastRunTime
            });
        }

        public static List<string[]> ReadPrintedCodeData(string fullFilePath, char delimiter = ',')
        {
            var result = new List<string[]>();

            if (string.IsNullOrWhiteSpace(fullFilePath) || !File.Exists(fullFilePath))
                return result;

            try
            {
                using (var fs = new FileStream(
                    fullFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var values = line.Split(delimiter);
                            result.Add(values);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file at {fullFilePath}: {ex.Message}");
            }

            return result;
        }
    }
}












//using BarcodeVerificationSystem.Controller;
//using BarcodeVerificationSystem.Labels.ProjectLabel;
//using BarcodeVerificationSystem.Model;
//using CommonVariable;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BarcodeVerificationSystem.Utils
//{
//    internal class SyncDataList
//    {
//        public int STT { get; set; }
//        public string MaCongViec { get; set; }
//        public string MaPhieuSoanHang { get; set; }
//        public string MaSanPham { get; set; }
//        public int SoLuongCanXuat { get; set; }
//        public int SoLuongDongBoSaaS { get; set; }
//        public string SoLuongDongBoSAP { get; set; }
//        public bool Hoanthanh { get; set; }

//        public static List<SyncDataList> ReturnSyncDataList(List<string> _JobNameList)
//        {
//            List<SyncDataList> rows = new List<SyncDataList>();
//            foreach (var x in _JobNameList)
//            {
//                try
//                {
//                    JobModel CurrentJob = Shared.GetJob(x);

//                    if (ProjectLabel.IsCaoSuDongNai)
//                        InitCaoSuProductSyncData(CurrentJob, rows, x);

//                    if (ProjectLabel.IsNutrifood)
//                        (Shared.Settings.IsManufacturingMode ? (Action)(() => InitManufacturingSyncData(CurrentJob, rows, x))
//                                                                    : () => InitDispatchingSyncData(CurrentJob, rows, x))();
//                }
//                catch (Exception) { }

//            }

//            return rows;
//        }

//        private static void InitCaoSuProductSyncData(JobModel CurrentJob, List<SyncDataList> rows, string x)
//        {
//            string sentDataPath = CommVariables.PathSentDataPrinted + CurrentJob.PrintedResponePath;
//            var _SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);

//            string sentCheckDataPath = CommVariables.PathSentDataChecked + CurrentJob.CheckedResultPath;
//            var _SentCheckCodeObtainFromFile = ReadPrintedCodeData(sentCheckDataPath);

//            string sentPalletDataPath = CommVariables.PathSentDataPallet + CurrentJob.CheckedResultPath;
//            var _PalletCodeObtainFromFile = ReadPrintedCodeData(sentPalletDataPath);

//            if (CurrentJob.IsJobOnline)
//            {
//                CurrentJob.NumberOfCheckSaaSSentCodes =
//                _SentCheckCodeObtainFromFile.Count(item => item.Length > 4 &&
//                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));

//                CurrentJob.NumberOfSaaSSentCodes =
//                _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 &&
//                                                      item[4].Equals("success", StringComparison.OrdinalIgnoreCase));

//                CurrentJob.NumberOfCodesInPallet = _PalletCodeObtainFromFile.Count(item => item.Length > 4 &&
//                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));
//            }

//            int quantity = CurrentJob.productWeight > 30 ? 144 : 240;

//            rows.Add(new SyncDataList
//            {
//                STT = 1,
//                MaCongViec = x,
//                MaPhieuSoanHang = CurrentJob.LOTNumber,
//                MaSanPham = CurrentJob.CaoSuProduct.product_code,
//                SoLuongCanXuat = CurrentJob.NumberOfSaaSSentCodes, // So luong da in
//                SoLuongDongBoSaaS = CurrentJob.NumberOfCheckSaaSSentCodes, // So luong da kiem tra
//                SoLuongDongBoSAP = $"{CurrentJob.NumberOfCodesInPallet} / {quantity}",
//                Hoanthanh = quantity == CurrentJob.NumberOfCodesInPallet
//            });
//        }

//        private static void InitManufacturingSyncData(JobModel CurrentJob, List<SyncDataList> rows, string x)
//        {
//            if (CurrentJob.IsProcessOrderMode)
//                InitProcessOrderData(CurrentJob, rows, x);

//            if (CurrentJob.IsReservationMode)
//                InitReservationSyncData(CurrentJob, rows, x);
//        }

//        private static void InitReservationSyncData(JobModel CurrentJob, List<SyncDataList> rows, string x)
//        {
//            var payload = CurrentJob.ReservationItem;
//            string sentDataPath = CommVariables.PathSentDataPrinted + CurrentJob.PrintedResponePath;
//            var _SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);

//            CurrentJob.NumberOfSAPSentCodes =
//            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 &&
//                            item[5].Equals("success", StringComparison.OrdinalIgnoreCase));


//            CurrentJob.NumberOfSaaSSentCodes =
//            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 &&
//                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));

//            rows.Add(new SyncDataList
//            {
//                STT = 1,
//                MaCongViec = x,
//                MaPhieuSoanHang = CurrentJob.Reservation.material_doc,
//                MaSanPham = payload.material_number,
//                SoLuongCanXuat = CurrentJob.NumberOfPrintedCodes,
//                SoLuongDongBoSaaS = CurrentJob.NumberOfSaaSSentCodes,
//                SoLuongDongBoSAP = CurrentJob.NumberOfSAPSentCodes.ToString(),
//                Hoanthanh = CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes
//            });
//        }

//        private static void InitProcessOrderData(JobModel CurrentJob, List<SyncDataList> rows, string x)
//        {
//            var payload = CurrentJob.ProcessOrderItem;
//            string sentDataPath = CommVariables.PathSentDataPrinted + CurrentJob.PrintedResponePath;
//            var _SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);

//            CurrentJob.NumberOfSAPSentCodes =
//            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 &&
//                            item[5].Equals("success", StringComparison.OrdinalIgnoreCase));


//            CurrentJob.NumberOfSaaSSentCodes =
//            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 &&
//                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));

//            rows.Add(new SyncDataList
//            {
//                STT = 1,
//                MaCongViec = x,
//                MaPhieuSoanHang = payload?.process_order,
//                MaSanPham = payload.material_number,
//                SoLuongCanXuat = CurrentJob.NumberOfPrintedCodes,
//                SoLuongDongBoSaaS = CurrentJob.NumberOfSaaSSentCodes,
//                SoLuongDongBoSAP = CurrentJob.NumberOfSAPSentCodes.ToString(),
//                Hoanthanh = CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes
//            });
//        }

//        private static void InitDispatchingSyncData(JobModel CurrentJob, List<SyncDataList> rows, string x)
//        {

//            var payload = CurrentJob.DispatchingOrderPayload.payload;
//            string sentDataPath = CommVariables.PathSentDataPrinted + CurrentJob.PrintedResponePath;
//            var _SentPrintedCodeObtainFromFile = ReadPrintedCodeData(sentDataPath);

//            CurrentJob.NumberOfSAPSentCodes =
//            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 5 &&
//                            item[5].Equals("success", StringComparison.OrdinalIgnoreCase));


//            CurrentJob.NumberOfSaaSSentCodes =
//            _SentPrintedCodeObtainFromFile.Count(item => item.Length > 4 &&
//                                                  item[4].Equals("success", StringComparison.OrdinalIgnoreCase));

//            rows.Add(new SyncDataList
//            {
//                STT = 1,
//                MaCongViec = x,
//                MaPhieuSoanHang = payload?.wms_number,
//                MaSanPham = (payload?.items != null
//                                             && CurrentJob.SelectedMaterialIndex >= 0
//                                             && CurrentJob.SelectedMaterialIndex < payload.items.Count)
//                                            ? payload.items[CurrentJob.SelectedMaterialIndex]?.material_number
//                                            : (CurrentJob?.ProcessOrder?.material_number ?? ""),
//                SoLuongCanXuat = CurrentJob.NumberOfPrintedCodes,
//                SoLuongDongBoSaaS = CurrentJob.NumberOfSaaSSentCodes,
//                SoLuongDongBoSAP = CurrentJob.NumberOfSAPSentCodes.ToString(),
//                Hoanthanh = CurrentJob.NumberOfPrintedCodes == CurrentJob.NumberOfSAPSentCodes
//            });
//        }

//        public static List<string[]> ReadPrintedCodeData(string fullFilePath, char delimiter = ',')
//        {
//            var result = new List<string[]>();

//            if (string.IsNullOrWhiteSpace(fullFilePath) || !File.Exists(fullFilePath))
//                return result;

//            try
//            {
//                using (var fs = new FileStream(
//                    fullFilePath,
//                    FileMode.Open,
//                    FileAccess.Read,
//                    FileShare.ReadWrite)) // 👈 allow other readers and writers
//                using (var sr = new StreamReader(fs))
//                {
//                    string line;
//                    while ((line = sr.ReadLine()) != null)
//                    {
//                        if (!string.IsNullOrWhiteSpace(line))
//                        {
//                            var values = line.Split(delimiter);
//                            result.Add(values);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error reading file at {fullFilePath}: {ex.Message}");
//            }

//            return result;
//        }


//    }
//}