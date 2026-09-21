//using BarcodeVerificationSystem.Controller;
//using BarcodeVerificationSystem.Model;
//using BarcodeVerificationSystem.Model.Droco;
//using BarcodeVerificationSystem.Utils;
//using BarcodeVerificationSystem.Utils.CodeGeneration;
//using CommonVariable;
//using DesignUI.CuzAlert;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Windows.Controls;
//using DesignUI.CuzAlert;
//using System.Drawing;
//using System.Windows.Forms;

//namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco
//{
//    /// <summary>
//    /// Quản lý quy trình đóng gói 4 cấp offline cho Droco.
//    /// Sản phẩm (GS1) → Hộp → Thùng → Pallet.
//    /// Scanner quét thủ công tem Hộp/Thùng/Pallet.
//    /// </summary>
//    public class DrocoPackagingManager
//    {
//        private readonly object _lock = new object();
//        private readonly string _filePath;
//        private readonly char _us = '\x1F';
//        // Delegate xác nhận in, trả về true nếu người dùng đồng ý in
//        public Func<PackagingLevel, string, bool> ConfirmPrint { get; set; }
//        private int _pendingCodesForBox = 0;
//        private int _pendingBoxesForCarton = 0;
//        private int _pendingCartonsForPallet = 0;

//        public enum ScanWaitState
//        {
//            None,
//            WaitingForBoxScan,
//            WaitingForCartonScan,
//            WaitingForPalletScan
//        }

//        public ScanWaitState CurrentScanState { get; private set; } = ScanWaitState.None;

//        public string PendingBoxQrCode { get; private set; } = "";
//        public string PendingCartonQrCode { get; private set; } = "";
//        public string PendingPalletQrCode { get; private set; } = "";

//        // Danh sách mã sản phẩm GS1 đang chờ gán vào hộp
//        private readonly List<string> _pendingProductCodes = new List<string>();

//        // Danh sách QR hộp đang chờ gán vào thùng
//        private readonly List<string> _pendingBoxCodes = new List<string>();

//        // Danh sách QR thùng đang chờ gán vào pallet
//        private readonly List<string> _pendingCartonCodes = new List<string>();

//        // === Thống kê tổng (bao gồm cả dữ liệu đã lưu từ job cũ) ===
//        public int TotalBoxes { get; private set; } = 0;
//        public int TotalCartons { get; private set; } = 0;
//        public int TotalPallets { get; private set; } = 0;

//        // Events
//        public event EventHandler<PackagingEventArgs> OnBoxReady;
//        public event EventHandler<PackagingEventArgs> OnCartonReady;
//        public event EventHandler<PackagingEventArgs> OnPalletReady;
//        public event EventHandler<PackagingEventArgs> OnBoxCompleted;
//        public event EventHandler<PackagingEventArgs> OnCartonCompleted;
//        public event EventHandler<PackagingEventArgs> OnPalletCompleted;
//        public event EventHandler<PackagingEventArgs> OnScanMismatch;
//        public event EventHandler<PackagingEventArgs> OnStateRestored;

//        public DrocoPackagingManager()
//        {
//        }

//        /// <summary>
//        /// Khôi phục trạng thái đóng gói từ job đã lưu.
//        /// Gọi khi mở lại job cũ để tiếp tục đếm đúng.
//        /// </summary>
//        public void RestoreFromJob(JobModel job)
//        {
//            if (job == null) return;

//            lock (_lock)
//            {
//                // Reset tất cả counter
//                _pendingCodesForBox = 0;
//                _pendingBoxesForCarton = 0;
//                _pendingCartonsForPallet = 0;
//                _pendingProductCodes.Clear();
//                _pendingBoxCodes.Clear();
//                _pendingCartonCodes.Clear();
//                CurrentScanState = ScanWaitState.None;
//                PendingBoxQrCode = "";
//                PendingCartonQrCode = "";
//                PendingPalletQrCode = "";

//                // Đếm tổng Hộp đã xác nhận
//                int confirmedBoxes = 0;
//                if (job.BoxList != null)
//                {
//                    confirmedBoxes = job.BoxList.Count(b => b.IsSent);
//                    TotalBoxes = job.BoxList.Count;

//                    // Nếu có hộp chưa xác nhận (in rồi nhưng chưa quét) → chờ quét lại
//                    var unconfirmedBox = job.BoxList.LastOrDefault(b => !b.IsSent);
//                    if (unconfirmedBox != null)
//                    {
//                        PendingBoxQrCode = unconfirmedBox.QrCode;
//                        CurrentScanState = ScanWaitState.WaitingForBoxScan;
//                    }
//                }

//                // Đếm tổng Thùng đã xác nhận
//                int confirmedCartons = 0;
//                if (job.DrocoCartonList != null)
//                {
//                    confirmedCartons = job.DrocoCartonList.Count(c => c.IsSent);
//                    TotalCartons = job.DrocoCartonList.Count;

//                    // Nếu có thùng chưa xác nhận → chờ quét lại
//                    var unconfirmedCarton = job.DrocoCartonList.LastOrDefault(c => !c.IsSent);
//                    if (unconfirmedCarton != null && CurrentScanState == ScanWaitState.None)
//                    {
//                        PendingCartonQrCode = unconfirmedCarton.QrCode;
//                        CurrentScanState = ScanWaitState.WaitingForCartonScan;
//                    }
//                }

//                // Đếm tổng Pallet đã xác nhận
//                int confirmedPallets = 0;
//                if (job.DrocoPalletList != null)
//                {
//                    confirmedPallets = job.DrocoPalletList.Count(p => p.IsSent);
//                    TotalPallets = job.DrocoPalletList.Count;

//                    // Nếu có pallet chưa xác nhận → chờ quét lại
//                    var unconfirmedPallet = job.DrocoPalletList.LastOrDefault(p => !p.IsSent);
//                    if (unconfirmedPallet != null && CurrentScanState == ScanWaitState.None)
//                    {
//                        PendingPalletQrCode = unconfirmedPallet.QrCode;
//                        CurrentScanState = ScanWaitState.WaitingForPalletScan;
//                    }
//                }

//                // Tính số hộp đã xác nhận nhưng chưa gán vào thùng nào
//                int boxesInCartons = 0;
//                if (job.DrocoCartonList != null)
//                {
//                    boxesInCartons = job.DrocoCartonList.Sum(c => c.BoxCodes != null ? c.BoxCodes.Count : 0);
//                }
//                _pendingBoxesForCarton = confirmedBoxes - boxesInCartons;
//                if (_pendingBoxesForCarton < 0) _pendingBoxesForCarton = 0;

//                // Lấy danh sách hộp chưa gán vào thùng
//                if (_pendingBoxesForCarton > 0 && job.BoxList != null)
//                {
//                    var allBoxCodesInCartons = new HashSet<string>();
//                    if (job.DrocoCartonList != null)
//                    {
//                        foreach (var carton in job.DrocoCartonList)
//                        {
//                            if (carton.BoxCodes != null)
//                            {
//                                foreach (var code in carton.BoxCodes)
//                                    allBoxCodesInCartons.Add(code);
//                            }
//                        }
//                    }
//                    foreach (var box in job.BoxList.Where(b => b.IsSent && !allBoxCodesInCartons.Contains(b.QrCode)))
//                    {
//                        _pendingBoxCodes.Add(box.QrCode);
//                    }
//                }

//                // Tính số thùng đã xác nhận nhưng chưa gán vào pallet nào
//                int cartonsInPallets = 0;
//                if (job.DrocoPalletList != null)
//                {
//                    cartonsInPallets = job.DrocoPalletList.Sum(p => p.CartonCodes != null ? p.CartonCodes.Count : 0);
//                }
//                _pendingCartonsForPallet = confirmedCartons - cartonsInPallets;
//                if (_pendingCartonsForPallet < 0) _pendingCartonsForPallet = 0;

//                // Lấy danh sách thùng chưa gán vào pallet
//                if (_pendingCartonsForPallet > 0 && job.DrocoCartonList != null)
//                {
//                    var allCartonCodesInPallets = new HashSet<string>();
//                    if (job.DrocoPalletList != null)
//                    {
//                        foreach (var pallet in job.DrocoPalletList)
//                        {
//                            if (pallet.CartonCodes != null)
//                            {
//                                foreach (var code in pallet.CartonCodes)
//                                    allCartonCodesInPallets.Add(code);
//                            }
//                        }
//                    }
//                    foreach (var carton in job.DrocoCartonList.Where(c => c.IsSent && !allCartonCodesInPallets.Contains(c.QrCode)))
//                    {
//                        _pendingCartonCodes.Add(carton.QrCode);
//                    }
//                }

//                ProjectLogger.WriteError($"[Packaging] RestoreFromJob: Boxes={TotalBoxes}(confirmed={confirmedBoxes}, pendingForCarton={_pendingBoxesForCarton}), " +
//                    $"Cartons={TotalCartons}(confirmed={confirmedCartons}, pendingForPallet={_pendingCartonsForPallet}), " +
//                    $"Pallets={TotalPallets}(confirmed={confirmedPallets}), ScanState={CurrentScanState}");

//                OnStateRestored?.Invoke(this, new PackagingEventArgs
//                {
//                    Level = PackagingLevel.Box,
//                    Message = $"Khôi phục: {TotalBoxes} Hộp, {TotalCartons} Thùng, {TotalPallets} Pallet. " +
//                              $"Chờ gán: {_pendingBoxesForCarton} hộp→thùng, {_pendingCartonsForPallet} thùng→pallet. " +
//                              (CurrentScanState != ScanWaitState.None ? $"Đang chờ quét: {CurrentScanState}" : "")
//                });
//            }
//        }
//        public bool ProcessAnyScan(string scannedQrCode)
//        {
//            lock (_lock)
//            {
//                JobModel job = Shared.CurrentJob;
//                if (job == null) return false;

//                // 1. Quét hộp
//                var box = job.BoxList?.FirstOrDefault(b => b.QrCode == scannedQrCode && !b.IsSent);
//                if (box != null)
//                {
//                    return ProcessBoxScan(scannedQrCode);
//                }

//                // 2. Quét thùng
//                var carton = job.DrocoCartonList?.FirstOrDefault(c => c.QrCode == scannedQrCode && !c.IsSent);
//                if (carton != null)
//                {
//                    return ProcessCartonScan(scannedQrCode);
//                }

//                // 3. Quét pallet
//                var pallet = job.DrocoPalletList?.FirstOrDefault(p => p.QrCode == scannedQrCode && !p.IsSent);
//                if (pallet != null)
//                {
//                    return ProcessPalletScan(scannedQrCode);
//                }

//                // 4. Nếu không phải mã nào hợp lệ
//                OnScanMismatch?.Invoke(this, new PackagingEventArgs
//                {
//                    Level = PackagingLevel.Box, // hoặc để trống
//                    QrCode = scannedQrCode,
//                    Message = $"QR code {scannedQrCode} không hợp lệ hoặc đã xác nhận!"

//                });

//                return false;
//            }
//        }







//        /// <summary>
//        /// Gọi khi có 1 mã sản phẩm valid mới (từ RSFP).
//        /// Trả về true nếu đủ hộp → cần chờ Scanner xác nhận.
//        /// </summary>
//        public bool AddValidCode(string qrCode)
//        {
//            lock (_lock)
//            {
//                int boxSize = GetBoxSize();
//                if (boxSize <= 0)
//                {
//                    ProjectLogger.WriteError($"[Packaging] GetBoxSize() = {boxSize}. NumberOfCodesInBox={Shared.CurrentJob.NumberOfCodesInBox}, NumberOfCodesInPallet={Shared.CurrentJob.NumberOfCodesInPallet}. Skipping AddValidCode.");
//                    return false;
//                }

//                _pendingProductCodes.Add(qrCode);
//                _pendingCodesForBox++;

//                ProjectLogger.WriteError($"[Packaging] AddValidCode: {_pendingCodesForBox}/{boxSize}");

//                if (_pendingCodesForBox >= boxSize)
//                {
//                    // 1. Sinh QR Hộp

//                    string boxPrefix = (TotalBoxes % 69 + 1).ToString("D2");
//                    string randomPart = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1)[0];
//                    randomPart = randomPart.Substring(2, 13); // Lấy 13 ký tự từ vị trí thứ 3 
//                    string boxQrCode = boxPrefix + randomPart; // Tổng cộng 15 ký tự // 13 ký tự random

//                    PendingBoxQrCode = boxQrCode;

//                    // 2. Tạo BoxModel & liên kết GS1 con
//                    JobModel job = Shared.CurrentJob;
//                    if (job.BoxList == null)
//                        job.BoxList = new List<BoxModel>();

//                    var newBox = new BoxModel
//                    {
//                        QrCode = boxQrCode,
//                        IsSent = false,
//                        sentToPrinter = false,
//                        ProductCodes = new List<string>(_pendingProductCodes)
//                    };
//                    job.BoxList.Add(newBox);


//                    // 3. Gán mã sản phẩm vào hộp trong AllValueProcess
//                    if (Shared.DrocoAllValueProcess != null)
//                    {
//                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
//                        foreach (var code in _pendingProductCodes)
//                        {
//                            string normalizedCode = code.Contains("\\F")
//                                ? code.Replace("\\F", "\x1D")
//                                : code;
//                            Shared.DrocoAllValueProcess.UpdateBox(normalizedCode, boxQrCode, now);
//                        }
//                    }

//                    // 4. In tem Hộp qua Zebra


//                    bool printed = PrintZebraLabel(boxQrCode, newBox);
//                    newBox.sentToPrinter = printed;

//                    job.SaveFile();

//                    // 5. Clear pending, chờ scanner xác nhận
//                    _pendingProductCodes.Clear();
//                    _pendingCodesForBox = 0;
//                    CurrentScanState = ScanWaitState.WaitingForBoxScan;

//                    OnBoxReady?.Invoke(this, new PackagingEventArgs
//                    {
//                        Level = PackagingLevel.Box,
//                        QrCode = boxQrCode,
//                        PendingCount = boxSize,
//                        ChildCodes = new List<string>(newBox.ProductCodes),
//                        Message = $"Đủ {boxSize} mã → QR Hộp: {boxQrCode}. Đã in tem, vui lòng quét xác nhận!"
//                    });
//                    return true;
//                }
//                return false;
//            }
//        }

//        /// <summary>
//        /// Gọi khi Scanner quét xác nhận mã Hộp.
//        /// </summary>
//        ///
//        public bool ProcessBoxScan(string scannedQrCode)
//        {
//            lock (_lock)
//            {
//                JobModel job = Shared.CurrentJob;
//                if (job.BoxList == null) job.BoxList = new List<BoxModel>();

//                var box = job.BoxList.FirstOrDefault(b => b.QrCode == scannedQrCode);

//                if (box != null && box.IsSent)
//                {
//                    OnScanMismatch?.Invoke(this, new PackagingEventArgs
//                    {
//                        Level = PackagingLevel.Box,
//                        QrCode = scannedQrCode,
//                        Message = $"Mã hộp {scannedQrCode} đã xác nhận trước đó!"
//                    });
//                    return false;
//                }

//                if (box == null)
//                {
//                    box = new BoxModel
//                    {
//                        QrCode = scannedQrCode,
//                        IsSent = true,
//                        sentToPrinter = false,
//                        ProductCodes = new List<string>()
//                    };
//                    job.BoxList.Add(box);
//                }
//                else
//                {
//                    box.IsSent = true;
//                    box.sentToPrinter = false;
//                }

//                job.SaveFile();

//                // Thông báo hoàn thành xác nhận hộp
//                OnBoxCompleted?.Invoke(this, new PackagingEventArgs
//                {
//                    Level = PackagingLevel.Box,
//                    QrCode = box.QrCode,
//                    Message = $"Hộp {box.QrCode} đã xác nhận thành công!",
//                    PendingCount = box.ProductCodes?.Count ?? 0,
//                    ChildCodes = new List<string>(box.ProductCodes ?? new List<string>())
//                });

//                // Xác nhận in từ UI
//                //bool allowPrint = true;
//                //if (ConfirmPrint != null)
//                //{
//                //    allowPrint = ConfirmPrint(PackagingLevel.Box, box.QrCode);
//                //}
//                //if (allowPrint)
//                //{
//                //    bool printed = PrintZebraLabel(box.QrCode, box);
//                //    box.sentToPrinter = printed;
//                //    job.SaveFile();
//                //}

//                // Tiếp tục logic đóng gói
//                _pendingBoxesForCarton++;
//                _pendingBoxCodes.Add(box.QrCode);

//                int boxesPerCarton = GetBoxesPerCarton();
//                if (boxesPerCarton > 0 && _pendingBoxesForCarton >= boxesPerCarton)
//                {
//                    // 2. Sinh QR Thùng
//                    string cartonPrefix = (70 + (TotalCartons % 20)).ToString("D2");
//                    string randomPart = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1)[0];
//                    randomPart = randomPart.Substring(2, 13);
//                    string cartonQrCode = cartonPrefix + randomPart;

//                    PendingCartonQrCode = cartonQrCode;

//                    if (job.DrocoCartonList == null) job.DrocoCartonList = new List<DrocoCartonModel>();

//                    var newCarton = new DrocoCartonModel
//                    {
//                        QrCode = cartonQrCode,
//                        IsSent = false,
//                        sentToPrinter = false,
//                        BoxCodes = new List<string>(_pendingBoxCodes)
//                    };
//                    job.DrocoCartonList.Add(newCarton);
//                    TotalCartons++;

//                    if (Shared.DrocoAllValueProcess != null)
//                    {
//                        foreach (var bCode in _pendingBoxCodes)
//                        {
//                            Shared.DrocoAllValueProcess.UpdateCarton(bCode, cartonQrCode);
//                        }
//                    }

//                    job.SaveFile();

//                    _pendingBoxCodes.Clear();
//                    _pendingBoxesForCarton = 0;
//                    CurrentScanState = ScanWaitState.WaitingForCartonScan;

//                    OnCartonReady?.Invoke(this, new PackagingEventArgs
//                    {
//                        Level = PackagingLevel.Carton,
//                        QrCode = cartonQrCode,
//                        PendingCount = boxesPerCarton,
//                        ChildCodes = new List<string>(newCarton.BoxCodes),
//                        Message = $"Đủ {boxesPerCarton} hộp -> Sinh QR Thùng: {cartonQrCode}. Vui lòng quét xác nhận Thùng!"
//                    });
//                }
//                else
//                {
//                    CurrentScanState = ScanWaitState.WaitingForBoxScan;
//                }

//                return true;
//            }
//        }

//        /// <summary>
//        /// Gọi khi Scanner quét xác nhận mã Thùng.
//        /// </summary>
//        public bool ProcessCartonScan(string scannedQrCode)
//        {
//            lock (_lock)
//            {
//                JobModel job = Shared.CurrentJob;

//                var carton = job.DrocoCartonList?.FirstOrDefault(c => c.QrCode == scannedQrCode && !c.IsSent);
//                if (carton == null)
//                {
//                    OnScanMismatch?.Invoke(this, new PackagingEventArgs
//                    {
//                        Level = PackagingLevel.Carton,
//                        QrCode = scannedQrCode,
//                        Message = $"Mã thùng {scannedQrCode} không hợp lệ hoặc đã xác nhận!"
//                    });
//                    return false;
//                }

//                carton.IsSent = true;
//                carton.sentToPrinter = false;
//                job.SaveFile();

//                // Thông báo xác nhận thành công, chờ xác nhận in
//                OnCartonCompleted?.Invoke(this, new PackagingEventArgs
//                {
//                    Level = PackagingLevel.Carton,
//                    QrCode = carton.QrCode,
//                    Message = $"Thùng {carton.QrCode} đã xác nhận thành công!"
//                });


//                //CuzAlert.Show(
//                //$"Thùng {carton.QrCode} đã xác nhận thành công!",
//                //Alert.enmType.Success,
//                //new Size(500, 120),
//                //new Point(300, 200), // hoặc vị trí phù hợp
//                //new Size(800, 600),  // hoặc kích thước form chính
//                //false // autoClose
//                //      // Không truyền timeout
//                //    );
//                //// Xác nhận in từ UI
//                //bool allowPrint = true;
//                //if (ConfirmPrint != null)
//                //{
//                //    allowPrint = ConfirmPrint(PackagingLevel.Carton, carton.QrCode);
//                //}
//                //if (allowPrint)
//                //{
//                //    bool printed = PrintZebraLabel(carton.QrCode, null);
//                //    carton.sentToPrinter = printed;
//                //    job.SaveFile();
//                //}

//                // Tiếp tục logic tạo pallet nếu đủ thùng
//                _pendingCartonsForPallet++;
//                _pendingCartonCodes.Add(carton.QrCode);
//                int cartonsPerPallet = GetCartonsPerPallet();
//                if (cartonsPerPallet > 0 && _pendingCartonsForPallet >= cartonsPerPallet)
//                {
//                    // 3. Sinh QR Pallet
//                    string palletPrefix = (90 + (TotalPallets % 10)).ToString("D2");
//                    string randomPart = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1)[0];
//                    randomPart = randomPart.Substring(2, 13);
//                    string palletQrCode = palletPrefix + randomPart;
//                    PendingPalletQrCode = palletQrCode;

//                    if (job.DrocoPalletList == null)
//                        job.DrocoPalletList = new List<DrocoPalletModel>();

//                    var newPallet = new DrocoPalletModel
//                    {
//                        QrCode = palletQrCode,
//                        IsSent = false,
//                        sentToPrinter = false,
//                        CartonCodes = new List<string>(_pendingCartonCodes)
//                    };
//                    job.DrocoPalletList.Add(newPallet);
//                    TotalPallets++;

//                    if (Shared.DrocoAllValueProcess != null)
//                    {
//                        foreach (var cCode in _pendingCartonCodes)
//                        {
//                            Shared.DrocoAllValueProcess.UpdatePalletForCarton(cCode, palletQrCode);
//                        }
//                    }

//                    job.SaveFile();

//                    _pendingCartonCodes.Clear();
//                    _pendingCartonsForPallet = 0;
//                    CurrentScanState = ScanWaitState.WaitingForPalletScan;

//                    OnPalletReady?.Invoke(this, new PackagingEventArgs
//                    {
//                        Level = PackagingLevel.Pallet,
//                        QrCode = palletQrCode,
//                        PendingCount = cartonsPerPallet,
//                        ChildCodes = new List<string>(newPallet.CartonCodes),
//                        Message = $"Đủ {cartonsPerPallet} thùng → QR Pallet: {palletQrCode}. Vui lòng quét xác nhận!"
//                    });
//                }
//                else
//                {
//                    var nextUnconfirmedCarton = job.DrocoCartonList.FirstOrDefault(c => !c.IsSent);
//                    if (nextUnconfirmedCarton != null)
//                    {
//                        CurrentScanState = ScanWaitState.WaitingForCartonScan;
//                        PendingCartonQrCode = "";
//                    }
//                    else
//                    {
//                        CurrentScanState = ScanWaitState.None;
//                        PendingCartonQrCode = "";
//                    }
//                }

//                return true;
//            }
//        }
//        public bool ProcessPalletScan(string scannedQrCode)
//        {
//            lock (_lock)
//            {
//                JobModel job = Shared.CurrentJob;

//                var pallet = job.DrocoPalletList?.FirstOrDefault(p => p.QrCode == scannedQrCode && !p.IsSent);
//                if (pallet == null)
//                {
//                    OnScanMismatch?.Invoke(this, new PackagingEventArgs
//                    {
//                        Level = PackagingLevel.Pallet,
//                        QrCode = scannedQrCode,
//                        Message = $"Mã pallet {scannedQrCode} không hợp lệ hoặc đã xác nhận!"
//                    });
//                    return false;
//                }

//                pallet.IsSent = true;
//                //pallet.sentToPrinter = false;
//                job.SaveFile();

//                //Thông báo xác nhận thành công, chờ xác nhận in
//                OnPalletCompleted?.Invoke(this, new PackagingEventArgs
//                {
//                    Level = PackagingLevel.Pallet,
//                    QrCode = pallet.QrCode,
//                    Message = $"Pallet {pallet.QrCode} đã xác nhận thành công!"
//                });
//                //CuzAlert.Show(
//                //         $"Pallet {pallet.QrCode} đã xác nhận thành công!",
//                //           Alert.enmType.Success,
//                //              new Size(500, 120),
//                //                 new Point(300, 200), // hoặc vị trí phù hợp
//                //                     new Size(800, 600),  // hoặc kích thước form chính
//                //                          true);

//                //// Xác nhận in từ UI
//                //bool allowPrint = true;
//                //if (ConfirmPrint != null)
//                //{
//                //    allowPrint = ConfirmPrint(PackagingLevel.Pallet, pallet.QrCode);
//                //}
//                //if (allowPrint)
//                //{
//                //    bool printed = PrintZebraLabel(pallet.QrCode, null);
//                //    pallet.sentToPrinter = printed;
//                //    job.SaveFile();
//                //}

//                var nextUnconfirmedPallet = job.DrocoPalletList.FirstOrDefault(p => !p.IsSent);
//                if (nextUnconfirmedPallet != null)
//                {
//                    CurrentScanState = ScanWaitState.WaitingForPalletScan;
//                    PendingPalletQrCode = "";
//                }
//                else
//                {
//                    CurrentScanState = ScanWaitState.None;
//                    PendingPalletQrCode = "";
//                }

//                return true;
//            }
//        }

//        /// <summary>
//        /// Xử lý mã lẻ khi kết thúc ca / Stop / Complete Job.
//        /// </summary>
//        public void FlushRemaining()
//        {
//            lock (_lock)
//            {
//                JobModel job = Shared.CurrentJob;

//                // === 1. Mã GS1 lẻ → Hộp không đầy ===
//                if (_pendingCodesForBox > 0)
//                {
//                    var generatedCodes = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1);
//                    string boxQrCode = generatedCodes[0];

//                    if (job.BoxList == null)
//                        job.BoxList = new List<BoxModel>();

//                    var newBox = new BoxModel
//                    {
//                        QrCode = boxQrCode,
//                        IsSent = true,
//                        sentToPrinter = false,
//                        ProductCodes = new List<string>(_pendingProductCodes)
//                    };
//                    job.BoxList.Add(newBox);
//                    TotalBoxes++;

//                    if (Shared.DrocoAllValueProcess != null)
//                    {
//                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//                        foreach (var code in _pendingProductCodes)
//                        {
//                            string normalizedCode = code.Contains("\\F")
//                                ? code.Replace("\\F", "\x1D")
//                                : code;
//                            Shared.DrocoAllValueProcess.UpdateBox(normalizedCode, boxQrCode, now);
//                        }
//                    }

//                    PrintZebraLabel(boxQrCode, newBox);

//                    int flushedCount = _pendingCodesForBox;
//                    _pendingProductCodes.Clear();
//                    _pendingCodesForBox = 0;
//                    _pendingBoxesForCarton++;
//                    _pendingBoxCodes.Add(boxQrCode);

//                    OnBoxReady?.Invoke(this, new PackagingEventArgs
//                    {
//                        Level = PackagingLevel.Box,
//                        QrCode = boxQrCode,
//                        PendingCount = flushedCount,
//                        ChildCodes = new List<string>(newBox.ProductCodes),
//                        Message = $"Hộp lẻ ({flushedCount} mã) → QR Hộp: {boxQrCode}. Đã in tem!"
//                    });
//                }

//                // === 2. Hộp lẻ → Thùng không đầy ===
//                if (_pendingBoxesForCarton > 0)
//                {
//                    var generatedCodes = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1);
//                    string cartonQrCode = generatedCodes[0];

//                    if (job.DrocoCartonList == null)
//                        job.DrocoCartonList = new List<DrocoCartonModel>();

//                    var newCarton = new DrocoCartonModel
//                    {
//                        QrCode = cartonQrCode,
//                        IsSent = true,
//                        sentToPrinter = false,
//                        BoxCodes = new List<string>(_pendingBoxCodes)
//                    };
//                    job.DrocoCartonList.Add(newCarton);
//                    TotalCartons++;

//                    if (Shared.DrocoAllValueProcess != null)
//                    {
//                        foreach (var bCode in _pendingBoxCodes)
//                        {
//                            Shared.DrocoAllValueProcess.UpdateCarton(bCode, cartonQrCode);
//                        }
//                    }

//                    PrintZebraLabel(cartonQrCode, null);

//                    int flushedBoxes = _pendingBoxesForCarton;
//                    _pendingBoxCodes.Clear();
//                    _pendingBoxesForCarton = 0;
//                    _pendingCartonsForPallet++;
//                    _pendingCartonCodes.Add(cartonQrCode);

//                    OnCartonReady?.Invoke(this, new PackagingEventArgs
//                    {
//                        Level = PackagingLevel.Carton,
//                        QrCode = cartonQrCode,
//                        PendingCount = flushedBoxes,
//                        ChildCodes = new List<string>(newCarton.BoxCodes),
//                        Message = $"Thùng lẻ ({flushedBoxes} hộp) → QR Thùng: {cartonQrCode}. Đã in tem!"
//                    });
//                }

//                // === 3. Thùng lẻ → Pallet không đầy ===
//                if (_pendingCartonsForPallet > 0)
//                {
//                    var generatedCodes = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1);
//                    string palletQrCode = generatedCodes[0];

//                    if (job.DrocoPalletList == null)
//                        job.DrocoPalletList = new List<DrocoPalletModel>();

//                    var newPallet = new DrocoPalletModel
//                    {
//                        QrCode = palletQrCode,
//                        IsSent = true,
//                        sentToPrinter = false,
//                        CartonCodes = new List<string>(_pendingCartonCodes)
//                    };
//                    job.DrocoPalletList.Add(newPallet);
//                    TotalPallets++;

//                    if (Shared.DrocoAllValueProcess != null)
//                    {
//                        foreach (var cCode in _pendingCartonCodes)
//                        {
//                            Shared.DrocoAllValueProcess.UpdatePalletForCarton(cCode, palletQrCode);
//                        }
//                    }

//                    PrintZebraLabel(palletQrCode, null);

//                    int flushedCartons = _pendingCartonsForPallet;
//                    _pendingCartonCodes.Clear();
//                    _pendingCartonsForPallet = 0;

//                    OnPalletReady?.Invoke(this, new PackagingEventArgs
//                    {
//                        Level = PackagingLevel.Pallet,
//                        QrCode = palletQrCode,
//                        PendingCount = flushedCartons,
//                        ChildCodes = new List<string>(newPallet.CartonCodes),
//                        Message = $"Pallet lẻ ({flushedCartons} thùng) → QR Pallet: {palletQrCode}. Đã in tem!"
//                    });
//                }

//                CurrentScanState = ScanWaitState.None;
//                PendingBoxQrCode = "";
//                PendingCartonQrCode = "";
//                PendingPalletQrCode = "";
//                job.SaveFile();
//            }
//        }

//        private bool PrintZebraLabel(string qrCode, BoxModel box)
//        {
//            try
//            {
//                if (Shared.Settings.ZebraPrinter.PODController.IsConnected())
//                {
//                    // Xác định loại QR theo prefix
//                    Shared.QRType qrType = Shared.QRType.Box;
//                    if (qrCode.StartsWith("90") || qrCode.StartsWith("91") || qrCode.StartsWith("92") || qrCode.StartsWith("93") || qrCode.StartsWith("94") ||
//                        qrCode.StartsWith("95") || qrCode.StartsWith("96") || qrCode.StartsWith("97") || qrCode.StartsWith("98") || qrCode.StartsWith("99"))
//                    {
//                        qrType = Shared.QRType.Pallet;
//                    }
//                    else if (qrCode.StartsWith("70") || qrCode.StartsWith("71") || qrCode.StartsWith("72") || qrCode.StartsWith("73") || qrCode.StartsWith("74") ||
//                             qrCode.StartsWith("75") || qrCode.StartsWith("76") || qrCode.StartsWith("77") || qrCode.StartsWith("78") || qrCode.StartsWith("79") ||
//                             qrCode.StartsWith("80") || qrCode.StartsWith("81") || qrCode.StartsWith("82") || qrCode.StartsWith("83") || qrCode.StartsWith("84") ||
//                             qrCode.StartsWith("85") || qrCode.StartsWith("86") || qrCode.StartsWith("87") || qrCode.StartsWith("88") || qrCode.StartsWith("89"))
//                    {
//                        qrType = Shared.QRType.Carton;
//                    }
//                    // Còn lại là Box

//                    Shared.PrintZebraDroco(qrCode, qrType);
//                    return true;
//                }
//                else
//                {
//                    ProjectLogger.WriteError("Zebra printer is not connected.");
//                    return false;
//                }
//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error printing Zebra label: " + ex.Message);
//                return false;
//            }
//        }

//        private int GetBoxSize()
//        {
//            var job = Shared.CurrentJob;
//            if (job == null)
//            {
//                ProjectLogger.WriteError($"[Packaging] GetBoxSize: Shared.CurrentJob is NULL!");
//                return 0;
//            }
//            int result = job.NumberOfCodesInBox > 0 ? job.NumberOfCodesInBox : job.NumberOfCodesInPallet;
//            ProjectLogger.WriteError($"[Packaging] GetBoxSize: NumberOfCodesInBox={job.NumberOfCodesInBox}, NumberOfCodesInPallet={job.NumberOfCodesInPallet}, result={result}");
//            return result;
//        }

//        private int GetBoxesPerCarton()
//        {
//            return Shared.CurrentJob.NumberOfBoxesInCarton;
//        }

//        private int GetCartonsPerPallet()
//        {
//            return Shared.CurrentJob.NumberOfCartonsInPallet;
//        }

//        /// <summary>
//        /// Xuất báo cáo cuối ca từ AllValueProcess.
//        /// </summary>
//        public void ExportEndOfShiftReport(string fileName)
//        {
//            try
//            {
//                ProjectLogger.WriteError($"[ExportEndOfShiftReport] Bắt đầu xuất báo cáo cuối ca: {fileName}");

//                if (Shared.DrocoAllValueProcess == null) return;

//                string csvPath = Shared.DrocoAllValueProcess.getFilePath();
//                if (!File.Exists(csvPath))
//                {
//                    ProjectLogger.WriteError($"[ExportEndOfShiftReport] Không tìm thấy file dữ liệu: {csvPath}");
//                    return;
//                }

//                var lines = File.ReadAllLines(csvPath);
//                var outputLines = new List<string>();

//                string header = lines.Length > 0
//                    ? lines[0].Replace(_us.ToString(), ",")
//                    : "Index,QRcode,Status,Created Time,Printed Time,Checked Time,Mapped Time,QRCode Box,QRCode Carton,QRCode Pallet";
//                outputLines.Add(header);

//                for (int i = 1; i < lines.Length; i++)
//                {
//                    outputLines.Add(lines[i].Replace(_us.ToString(), ","));
//                }

//                File.WriteAllLines(fileName, outputLines, new UTF8Encoding(true));
//                ProjectLogger.WriteError($"[ExportEndOfShiftReport] Xuất báo cáo cuối ca thành công: {fileName}");

//            }
//            catch (Exception ex)
//            {
//                ProjectLogger.WriteError("Error exporting end of shift report: " + ex.Message);
//            }
//        }
//    }

//    public enum PackagingLevel
//    {
//        Box,
//        Carton,
//        Pallet
//    }

//    public class PackagingEventArgs : EventArgs
//    {
//        public PackagingLevel Level { get; set; }
//        public string QrCode { get; set; }
//        public string Message { get; set; }
//        public int PendingCount { get; set; }

//        /// <summary>
//        /// Danh sách mã con được liên kết vào cấp này
//        /// </summary>
//        public List<string> ChildCodes { get; set; } = new List<string>();
//    }
//}


using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.Droco;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Utils.CodeGeneration;
using CommonVariable;
using DesignUI.CuzAlert;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.Modules.ReliableDataSender.Services.Droco
{
    /// <summary>
    /// Quản lý quy trình đóng gói 4 cấp offline cho Droco.
    /// Sản phẩm (GS1) → Hộp → Thùng → Pallet.
    /// Scanner quét thủ công tem Hộp/Thùng/Pallet.
    /// </summary>
    public class DrocoPackagingManager
    {
        private readonly object _lock = new object();
        private readonly string _filePath;
        private readonly char _us = '\x1F';

        // Delegate xác nhận in, trả về true nếu người dùng đồng ý in
        public Func<PackagingLevel, string, bool> ConfirmPrint { get; set; }

        private int _pendingCodesForBox = 0;
        /// <summary>Số mã GS1 đang chờ đủ 1 hộp (cho UI hiển thị "mã đã vào hộp: 5/20")</summary>
        public int PendingCodesForBoxCount => _pendingCodesForBox;
        private int _pendingBoxesForCarton = 0;
        private int _pendingCartonsForPallet = 0;

        public enum ScanWaitState
        {
            None,
            WaitingForBoxScan,
            WaitingForCartonScan,
            WaitingForPalletScan
        }

        public ScanWaitState CurrentScanState { get; private set; } = ScanWaitState.None;

        public string PendingBoxQrCode { get; private set; } = "";
        public string PendingCartonQrCode { get; private set; } = "";
        public string PendingPalletQrCode { get; private set; } = "";

        // Danh sách mã sản phẩm GS1 đang chờ gán vào hộp
        private readonly List<string> _pendingProductCodes = new List<string>();

        // Theo dõi tất cả mã sản phẩm đã từng được đếm vào hộp (chống duplicate camera chụp 2 lần)
        private readonly HashSet<string> _allProcessedProductCodes = new HashSet<string>();

        // Danh sách QR hộp đang chờ gán vào thùng
        private readonly List<string> _pendingBoxCodes = new List<string>();

        // Danh sách QR thùng đang chờ gán vào pallet
        private readonly List<string> _pendingCartonCodes = new List<string>();

        // === Thống kê tổng (bao gồm cả dữ liệu đã lưu từ job cũ) ===
        public int TotalBoxes { get; private set; } = 0;
        public int TotalCartons { get; private set; } = 0;
        public int TotalPallets { get; private set; } = 0;

        // ── [CHANGE 1] Lưu loại QR khi sinh ra → route chính xác khi quét tự do ──
        private readonly Dictionary<string, PackagingLevel> _qrLevelMap =
            new Dictionary<string, PackagingLevel>(StringComparer.OrdinalIgnoreCase);
        // ─────────────────────────────────────────────────────────────────────────

        // Events
        public event EventHandler<PackagingEventArgs> OnBoxReady;
        public event EventHandler<PackagingEventArgs> OnCartonReady;
        public event EventHandler<PackagingEventArgs> OnPalletReady;
        public event EventHandler<PackagingEventArgs> OnBoxCompleted;
        public event EventHandler<PackagingEventArgs> OnCartonCompleted;
        public event EventHandler<PackagingEventArgs> OnPalletCompleted;
        public event EventHandler<PackagingEventArgs> OnScanMismatch;
        public event EventHandler<PackagingEventArgs> OnStateRestored;

        public DrocoPackagingManager()
        {
        }

        // ── Helper: sinh QR theo chế độ (Excel hoặc tự động) ─────────────────

        /// <summary>
        /// Sinh QR code theo chế độ cấu hình:
        /// - UseExcelMode = true  → lấy từ ExcelCodeCache theo cột tương ứng
        /// - UseExcelMode = false → sinh tự động như cũ (UniqueRandomCodeGenereator)
        /// </summary>
        private string GenerateQrCode(PackagingLevel level)
        {
            var mapping = Shared.Settings.DrocoQRFieldMapping;

            if (mapping != null && mapping.UseExcelMode && Shared.ExcelCodeCache != null)
            {
                int colNumber = -1;
                switch (level)
                {
                    case PackagingLevel.Box: colNumber = mapping.BoxColumnIndex; break;
                    case PackagingLevel.Carton: colNumber = mapping.CartonColumnIndex; break;
                    case PackagingLevel.Pallet: colNumber = mapping.PalletColumnIndex; break;
                }

                int colIdx = colNumber - 1; // chuyển 1-based → 0-based

                if (colIdx >= 0)
                {
                    string excelCode = Shared.ExcelCodeCache.GetNextCode(colIdx);
                    if (!string.IsNullOrWhiteSpace(excelCode))
                    {
                        ProjectLogger.WriteError($"[Packaging] Excel mode → {level} QR: {excelCode} (cot {colNumber})");
                        return excelCode;
                    }

                    ProjectLogger.WriteError($"[Packaging] Excel mode: het ma cho {level} (cot {colNumber}), fallback tu dong.");
                }
            }

            // Fallback tự động
            string prefix;
            switch (level)
            {
                case PackagingLevel.Box: prefix = (TotalBoxes % 69 + 1).ToString("D2"); break;
                case PackagingLevel.Carton: prefix = (70 + TotalCartons % 20).ToString("D2"); break;
                case PackagingLevel.Pallet: prefix = (90 + TotalPallets % 10).ToString("D2"); break;
                default: prefix = "00"; break;
            }
            string randomPart = UniqueRandomCodeGenereator.GenerateUniqueRandomCode(1)[0];
            return prefix + randomPart.Substring(2, 13);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ── Helper: chuẩn hóa mã quét từ GS1 HRI format về raw format ──────────
        /// <summary>
        /// Chuyển đổi GS1 HRI format về raw:
        ///   (00)046071962297700016  →  00046071962297700016
        ///   (01)12345678901234      →  0112345678901234
        /// </summary>
        private string NormalizeScannedCode(string scannedCode)
        {
            if (string.IsNullOrEmpty(scannedCode)) return scannedCode;

            // Strip GS1 parenthesis AI: (XX) hoặc (XXXX) ở đầu chuỗi
            // Ví dụ: "(00)046071..." → "00046071..."
            var match = Regex.Match(scannedCode, @"^\((\d{2,4})\)(.+)$");
            if (match.Success)
                return match.Groups[1].Value + match.Groups[2].Value;

            return scannedCode;
        }
        /// <summary>
        /// Khôi phục trạng thái đóng gói từ job đã lưu.
        /// Gọi khi mở lại job cũ để tiếp tục đếm đúng.
        /// </summary>
        public void RestoreFromJob(JobModel job)
        {
            if (job == null) return;

            lock (_lock)
            {
                // Reset tất cả counter
                _pendingBoxesForCarton = 0;
                _pendingCartonsForPallet = 0;
                _pendingProductCodes.Clear();
                _pendingBoxCodes.Clear();
                _pendingCartonCodes.Clear();
                _allProcessedProductCodes.Clear();
                // Rebuild HashSet từ AllValues để preserve lịch sử đã xử lý (không mất counter)
                if (Shared.DrocoAllValueProcess != null)
                {
                    try
                    {
                        var payload = Shared.DrocoAllValueProcess.GetAllValuePayload();
                        foreach (var qr in payload.qr_list)
                        {
                            if (string.Equals(qr.status, "mapped", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(qr.status, "printed", StringComparison.OrdinalIgnoreCase))
                            {
                                _allProcessedProductCodes.Add(qr.qrcode_value);
                            }
                        }
                        ProjectLogger.WriteInfo($"[RestoreFromJob] Rebuilt _allProcessedProductCodes from AllValues: {_allProcessedProductCodes.Count} items");

                        // Rebuild _pendingProductCodes: mã đã verify (status="valid") nhưng chưa mapped/printed
                        foreach (var qr in payload.qr_list)
                        {
                            if (string.Equals(qr.status, "valid", StringComparison.OrdinalIgnoreCase)
                                && !_allProcessedProductCodes.Contains(qr.qrcode_value))
                            {
                                _allProcessedProductCodes.Add(qr.qrcode_value);
                                _pendingProductCodes.Add(qr.qrcode_value);
                            }
                        }
                        _pendingCodesForBox = _pendingProductCodes.Count;
                        ProjectLogger.WriteInfo($"[RestoreFromJob] Rebuilt _pendingProductCodes from AllValues: {_pendingCodesForBox} codes pending for box");
                    }
                    catch (Exception ex)
                    {
                        ProjectLogger.WriteError($"[RestoreFromJob] Error rebuilding from AllValues: {ex.Message}");
                    }
                }
                else
                {
                    ProjectLogger.WriteWarning("[RestoreFromJob] Shared.DrocoAllValueProcess is null, _allProcessedProductCodes will be empty");
                    _pendingCodesForBox = 0;
                }
                CurrentScanState = ScanWaitState.None;
                PendingBoxQrCode = "";
                PendingCartonQrCode = "";
                PendingPalletQrCode = "";

                // Đếm tổng Hộp đã xác nhận
                int confirmedBoxes = 0;
                if (job.BoxList != null)
                {
                    confirmedBoxes = job.BoxList.Count(b => b.IsSent);
                    TotalBoxes = job.BoxList.Count;

                    var unconfirmedBox = job.BoxList.LastOrDefault(b => !b.IsSent);
                    if (unconfirmedBox != null)
                    {
                        PendingBoxQrCode = unconfirmedBox.QrCode;
                        CurrentScanState = ScanWaitState.WaitingForBoxScan;
                    }
                }

                // Đếm tổng Thùng đã xác nhận
                int confirmedCartons = 0;
                int totalBoxesInCartons = 0;
                if (job.DrocoCartonList != null)
                {
                    confirmedCartons = job.DrocoCartonList.Count(c => c.IsSent);
                    TotalCartons = job.DrocoCartonList.Count;

                    // Đếm tổng số hộp đã nằm trong thùng
                    foreach (var c in job.DrocoCartonList)
                        totalBoxesInCartons += c.BoxCodes?.Count ?? 0;

                    var unconfirmedCarton = job.DrocoCartonList.LastOrDefault(c => !c.IsSent);
                    if (unconfirmedCarton != null && CurrentScanState == ScanWaitState.None)
                    {
                        PendingCartonQrCode = unconfirmedCarton.QrCode;
                        CurrentScanState = ScanWaitState.WaitingForCartonScan;
                    }
                }

                // Đếm tổng Pallet đã xác nhận
                int confirmedPallets = 0;
                if (job.DrocoPalletList != null)
                {
                    confirmedPallets = job.DrocoPalletList.Count(p => p.IsSent);
                    TotalPallets = job.DrocoPalletList.Count;

                    var unconfirmedPallet = job.DrocoPalletList.LastOrDefault(p => !p.IsSent);
                    if (unconfirmedPallet != null && CurrentScanState == ScanWaitState.None)
                    {
                        PendingPalletQrCode = unconfirmedPallet.QrCode;
                        CurrentScanState = ScanWaitState.WaitingForPalletScan;
                    }
                }

                // Tính số hộp đã xác nhận nhưng chưa gán vào thùng nào
                int boxesInCartons = 0;
                if (job.DrocoCartonList != null)
                {
                    boxesInCartons = job.DrocoCartonList.Sum(c => c.BoxCodes != null ? c.BoxCodes.Count : 0);
                }
                _pendingBoxesForCarton = confirmedBoxes - boxesInCartons;
                if (_pendingBoxesForCarton < 0) _pendingBoxesForCarton = 0;

                // Lấy danh sách hộp chưa gán vào thùng
                if (_pendingBoxesForCarton > 0 && job.BoxList != null)
                {
                    var allBoxCodesInCartons = new HashSet<string>();
                    if (job.DrocoCartonList != null)
                    {
                        foreach (var carton in job.DrocoCartonList)
                        {
                            if (carton.BoxCodes != null)
                            {
                                foreach (var code in carton.BoxCodes)
                                    allBoxCodesInCartons.Add(code);
                            }
                        }
                    }
                    foreach (var box in job.BoxList.Where(b => b.IsSent && !allBoxCodesInCartons.Contains(b.QrCode)))
                    {
                        _pendingBoxCodes.Add(box.QrCode);
                    }
                }

                // Tính số thùng đã xác nhận nhưng chưa gán vào pallet nào
                int cartonsInPallets = 0;
                if (job.DrocoPalletList != null)
                {
                    cartonsInPallets = job.DrocoPalletList.Sum(p => p.CartonCodes != null ? p.CartonCodes.Count : 0);
                }
                _pendingCartonsForPallet = confirmedCartons - cartonsInPallets;
                if (_pendingCartonsForPallet < 0) _pendingCartonsForPallet = 0;

                // Lấy danh sách thùng chưa gán vào pallet
                if (_pendingCartonsForPallet > 0 && job.DrocoCartonList != null)
                {
                    var allCartonCodesInPallets = new HashSet<string>();
                    if (job.DrocoPalletList != null)
                    {
                        foreach (var pallet in job.DrocoPalletList)
                        {
                            if (pallet.CartonCodes != null)
                            {
                                foreach (var code in pallet.CartonCodes)
                                    allCartonCodesInPallets.Add(code);
                            }
                        }
                    }
                    foreach (var carton in job.DrocoCartonList.Where(c => c.IsSent && !allCartonCodesInPallets.Contains(c.QrCode)))
                    {
                        _pendingCartonCodes.Add(carton.QrCode);
                    }
                }

                // ── [CHANGE 2] Xây dựng lại _qrLevelMap từ dữ liệu job đã lưu ──
                _qrLevelMap.Clear();
                if (job.BoxList != null)
                    foreach (var b in job.BoxList)
                        if (!string.IsNullOrEmpty(b.QrCode))
                            _qrLevelMap[b.QrCode] = PackagingLevel.Box;
                if (job.DrocoCartonList != null)
                    foreach (var c in job.DrocoCartonList)
                        if (!string.IsNullOrEmpty(c.QrCode))
                            _qrLevelMap[c.QrCode] = PackagingLevel.Carton;
                if (job.DrocoPalletList != null)
                    foreach (var p in job.DrocoPalletList)
                        if (!string.IsNullOrEmpty(p.QrCode))
                            _qrLevelMap[p.QrCode] = PackagingLevel.Pallet;
                ProjectLogger.WriteError($"[Packaging] RestoreFromJob: _qrLevelMap rebuilt with {_qrLevelMap.Count} entries.");
                // ──────────────────────────────────────────────────────────────────

                ProjectLogger.WriteError($"[Packaging] RestoreFromJob: Boxes={TotalBoxes}(confirmed={confirmedBoxes}, pendingForCarton={_pendingBoxesForCarton}), " +
                    $"Cartons={TotalCartons}(confirmed={confirmedCartons}, pendingForPallet={_pendingCartonsForPallet}), " +
                    $"Pallets={TotalPallets}(confirmed={confirmedPallets}), ScanState={CurrentScanState}");

                OnStateRestored?.Invoke(this, new PackagingEventArgs
                {
                    Level = PackagingLevel.Box,
                    Message = $"Khôi phục: {TotalBoxes} Hộp, {TotalCartons} Thùng, {TotalPallets} Pallet. " +
                              $"Chờ gán: {_pendingBoxesForCarton} hộp→thùng, {_pendingCartonsForPallet} thùng→pallet. " +
                              (CurrentScanState != ScanWaitState.None ? $"Đang chờ quét: {CurrentScanState}" : "")
                });
            }
        }

        // ── [CHANGE 3] ProcessAnyScan: route theo _qrLevelMap, quét thoải mái ──
        public bool ProcessAnyScan(string scannedQrCode)
        {
            // Normalize: (00)046071... → 00046071...
            scannedQrCode = NormalizeScannedCode(scannedQrCode?.Trim());

            lock (_lock)
            {
                JobModel job = Shared.CurrentJob;
                if (job == null) return false;

                // Tra cứu loại QR từ map (O(1)) — không phụ thuộc CurrentScanState
                PackagingLevel knownLevel;
                if (_qrLevelMap.TryGetValue(scannedQrCode, out knownLevel))
                {
                    switch (knownLevel)
                    {
                        case PackagingLevel.Box: return ProcessBoxScan(scannedQrCode);
                        case PackagingLevel.Carton: return ProcessCartonScan(scannedQrCode);
                        case PackagingLevel.Pallet: return ProcessPalletScan(scannedQrCode);
                    }
                }

                // Fallback: tìm tuần tự (dành cho mã chưa có trong map)
                var box = job.BoxList?.FirstOrDefault(b => b.QrCode == scannedQrCode && !b.IsSent);
                if (box != null) return ProcessBoxScan(scannedQrCode);

                var carton = job.DrocoCartonList?.FirstOrDefault(c => c.QrCode == scannedQrCode && !c.IsSent);
                if (carton != null) return ProcessCartonScan(scannedQrCode);

                var pallet = job.DrocoPalletList?.FirstOrDefault(p => p.QrCode == scannedQrCode && !p.IsSent);
                if (pallet != null) return ProcessPalletScan(scannedQrCode);

                OnScanMismatch?.Invoke(this, new PackagingEventArgs
                {
                    Level = PackagingLevel.Box,
                    QrCode = scannedQrCode,
                    Message = $"QR [{scannedQrCode}] không hợp lệ hoặc đã xác nhận!"
                });

                return false;
            }
        }
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gọi khi có 1 mã sản phẩm valid mới (từ RSFP).
        /// Trả về true nếu đủ hộp → cần chờ Scanner xác nhận.
        /// </summary>
        public bool AddValidCode(string qrCode)
        {
            lock (_lock)
            {
                int boxSize = GetBoxSize();
                if (boxSize <= 0)
                {
                    ProjectLogger.WriteError($"[Packaging] GetBoxSize() = {boxSize}. NumberOfCodesInBox={Shared.CurrentJob.NumberOfCodesInBox}, NumberOfCodesInPallet={Shared.CurrentJob.NumberOfCodesInPallet}. Skipping AddValidCode.");
                    return false;
                }

                // Kiểm tra duplicate — camera chụp 2 lần cùng 1 mã
                if (!_allProcessedProductCodes.Add(qrCode))
                {
                    ProjectLogger.WriteWarning($"[Packaging] DUPLICATE: Mã '{qrCode}' đã được đếm trước đó, bỏ qua!");
                    return false;
                }

                _pendingProductCodes.Add(qrCode);
                _pendingCodesForBox++;

                ProjectLogger.WriteInfo($"[Packaging] AddValidCode: {_pendingCodesForBox}/{boxSize}");

                if (_pendingCodesForBox >= boxSize)
                {
                    // Nếu overflow (pending > boxSize), chỉ lấy boxSize mã cho hộp hiện tại
                    var boxCodes = _pendingProductCodes.Take(boxSize).ToList();
                    var overflowCodes = _pendingProductCodes.Skip(boxSize).ToList();

                    string boxQrCode = GenerateQrCode(PackagingLevel.Box);
                    TotalBoxes++;
                    // ── [CHANGE 4] Lưu loại QR Box vào map ──
                    _qrLevelMap[boxQrCode] = PackagingLevel.Box;
                    // ─────────────────────────────────────────

                    PendingBoxQrCode = boxQrCode;

                    JobModel job = Shared.CurrentJob;
                    if (job.BoxList == null)
                        job.BoxList = new List<BoxModel>();

                    var newBox = new BoxModel
                    {
                        QrCode = boxQrCode,
                        IsSent = false,
                        sentToPrinter = false,
                        ProductCodes = new List<string>(boxCodes)
                    };
                    job.BoxList.Add(newBox);

                    // Ghi AllValues cho đúng boxSize mã
                    if (Shared.DrocoAllValueProcess != null)
                    {
                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        var normalizedCodes = boxCodes
                            .Select(c => c.Contains("\\F") ? c.Replace("\\F", "\x1D") : c)
                            .ToList();
                        Shared.DrocoAllValueProcess.UpdateBoxBatch(normalizedCodes, boxQrCode, now);
                        if (normalizedCodes.Count != boxSize)
                            ProjectLogger.WriteWarning($"[Packaging] BOX SIZE MISMATCH: Box '{boxQrCode}' has {normalizedCodes.Count} codes, expected {boxSize} (boxSize={boxSize})");
                        else if (overflowCodes.Count > 0)
                            ProjectLogger.WriteInfo($"[Packaging] Box '{boxQrCode}': {normalizedCodes.Count}/{boxSize} codes, {overflowCodes.Count} codes overflow → next box");
                        else
                            ProjectLogger.WriteInfo($"[Packaging] Box '{boxQrCode}' created with {normalizedCodes.Count}/{boxSize} GS1 codes");
                    }
                    bool printed = PrintZebraLabel(boxQrCode, PackagingLevel.Box);
                    newBox.sentToPrinter = printed;
                    ProjectLogger.WriteInfo($"[Packaging] Box ready: QR='{boxQrCode}', Products={newBox.ProductCodes.Count}, Printed={printed}");
                    job.SaveFile();

                    // Giữ lại overflow codes cho hộp tiếp theo
                    _pendingProductCodes.Clear();
                    if (overflowCodes.Count > 0)
                    {
                        _pendingProductCodes.AddRange(overflowCodes);
                        _pendingCodesForBox = overflowCodes.Count;
                        ProjectLogger.WriteInfo($"[Packaging] Kept {overflowCodes.Count} overflow codes for next box");
                    }
                    else
                    {
                        _pendingCodesForBox = 0;
                    }
                    CurrentScanState = ScanWaitState.WaitingForBoxScan;

                    OnBoxReady?.Invoke(this, new PackagingEventArgs
                    {
                        Level = PackagingLevel.Box,
                        QrCode = boxQrCode,
                        PendingCount = boxSize,
                        ChildCodes = new List<string>(newBox.ProductCodes),
                        Message = $"Đủ {boxSize} mã → QR Hộp: {boxQrCode}. Đã in tem, vui lòng quét xác nhận!"
                    });
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gọi khi Scanner quét xác nhận mã Hộp.
        /// </summary>
        public bool ProcessBoxScan(string scannedQrCode)
        {
            scannedQrCode = NormalizeScannedCode(scannedQrCode?.Trim());
            ProjectLogger.WriteInfo($"[Packaging] ProcessBoxScan: '{scannedQrCode}'");
            lock (_lock)
            {
                JobModel job = Shared.CurrentJob;
                if (job.BoxList == null) job.BoxList = new List<BoxModel>();

                var box = job.BoxList.FirstOrDefault(b => b.QrCode == scannedQrCode);

                if (box != null && box.IsSent)
                {
                    ProjectLogger.WriteWarning($"[Packaging] ProcessBoxScan: Box '{scannedQrCode}' already confirmed.");
                    OnScanMismatch?.Invoke(this, new PackagingEventArgs { Level = PackagingLevel.Box, QrCode = scannedQrCode, Message = $"Mã hộp {scannedQrCode} đã xác nhận trước đó!" });
                    return false;
                }

                if (box == null)
                {
                    ProjectLogger.WriteWarning($"[Packaging] ProcessBoxScan: Box '{scannedQrCode}' not found in BoxList.");
                    OnScanMismatch?.Invoke(this, new PackagingEventArgs { Level = PackagingLevel.Box, QrCode = scannedQrCode, Message = $"Mã hộp {scannedQrCode} không tồn tại trong danh sách! Vui lòng quét đúng tem hộp." });
                    return false;
                }

                box.IsSent = true;
                job.SaveFile();

                ProjectLogger.WriteInfo($"[Packaging] Box confirmed: '{box.QrCode}', Products={box.ProductCodes?.Count ?? 0}. PendingForCarton={_pendingBoxesForCarton + 1}/{GetBoxesPerCarton()}");

                OnBoxCompleted?.Invoke(this, new PackagingEventArgs
                {
                    Level = PackagingLevel.Box,
                    QrCode = box.QrCode,
                    Message = $"Hộp {box.QrCode} đã xác nhận thành công!",
                    PendingCount = box.ProductCodes?.Count ?? 0,
                    ChildCodes = new List<string>(box.ProductCodes ?? new List<string>())
                });

                _pendingBoxesForCarton++;
                _pendingBoxCodes.Add(box.QrCode);

                int boxesPerCarton = GetBoxesPerCarton();
                if (boxesPerCarton > 0 && _pendingBoxesForCarton >= boxesPerCarton)
                {
                    string cartonQrCode = GenerateQrCode(PackagingLevel.Carton);
                    TotalCartons++;
                    _qrLevelMap[cartonQrCode] = PackagingLevel.Carton;
                    PendingCartonQrCode = cartonQrCode;

                    if (job.DrocoCartonList == null) job.DrocoCartonList = new List<DrocoCartonModel>();

                    var newCarton = new DrocoCartonModel
                    {
                        QrCode = cartonQrCode,
                        IsSent = false,
                        sentToPrinter = false,
                        BoxCodes = new List<string>(_pendingBoxCodes)
                    };
                    job.DrocoCartonList.Add(newCarton);

                    if (Shared.DrocoAllValueProcess != null)
                    {
                        foreach (var bCode in _pendingBoxCodes)
                            Shared.DrocoAllValueProcess.UpdateCarton(bCode, cartonQrCode);
                    }

                    job.SaveFile();
                    ProjectLogger.WriteInfo($"[Packaging] Carton ready: QR='{cartonQrCode}', Boxes={newCarton.BoxCodes.Count}");

                    _pendingBoxCodes.Clear();
                    _pendingBoxesForCarton = 0;
                    CurrentScanState = ScanWaitState.WaitingForCartonScan;

                    OnCartonReady?.Invoke(this, new PackagingEventArgs
                    {
                        Level = PackagingLevel.Carton,
                        QrCode = cartonQrCode,
                        PendingCount = boxesPerCarton,
                        ChildCodes = new List<string>(newCarton.BoxCodes),
                        Message = $"Đủ {boxesPerCarton} hộp -> Sinh QR Thùng: {cartonQrCode}. Vui lòng quét xác nhận Thùng!"
                    });
                }
                else
                {
                    CurrentScanState = ScanWaitState.WaitingForBoxScan;
                    PendingBoxQrCode = "";
                }

                return true;
            }
        }

        /// <summary>
        /// Gọi khi Scanner quét xác nhận mã Thùng.
        /// </summary>
        public bool ProcessCartonScan(string scannedQrCode)
        {
            scannedQrCode = NormalizeScannedCode(scannedQrCode?.Trim());
            ProjectLogger.WriteInfo($"[Packaging] ProcessCartonScan: '{scannedQrCode}'");
            lock (_lock)
            {
                JobModel job = Shared.CurrentJob;

                var carton = job.DrocoCartonList?.FirstOrDefault(c => c.QrCode == scannedQrCode && !c.IsSent);
                if (carton == null)
                {
                    ProjectLogger.WriteWarning($"[Packaging] ProcessCartonScan: Carton '{scannedQrCode}' not found or already confirmed.");
                    OnScanMismatch?.Invoke(this, new PackagingEventArgs { Level = PackagingLevel.Carton, QrCode = scannedQrCode, Message = $"Mã thùng {scannedQrCode} không hợp lệ hoặc đã xác nhận!" });
                    return false;
                }

                carton.IsSent = true;
                job.SaveFile();

                ProjectLogger.WriteInfo($"[Packaging] Carton confirmed: '{carton.QrCode}'. PendingForPallet={_pendingCartonsForPallet + 1}/{GetCartonsPerPallet()}");

                OnCartonCompleted?.Invoke(this, new PackagingEventArgs { Level = PackagingLevel.Carton, QrCode = carton.QrCode, Message = $"Thùng {carton.QrCode} đã xác nhận thành công!" });

                _pendingCartonsForPallet++;
                _pendingCartonCodes.Add(carton.QrCode);
                int cartonsPerPallet = GetCartonsPerPallet();
                if (cartonsPerPallet > 0 && _pendingCartonsForPallet >= cartonsPerPallet)
                {
                    string palletQrCode = GenerateQrCode(PackagingLevel.Pallet);
                    TotalPallets++;
                    _qrLevelMap[palletQrCode] = PackagingLevel.Pallet;
                    PendingPalletQrCode = palletQrCode;

                    if (job.DrocoPalletList == null)
                        job.DrocoPalletList = new List<DrocoPalletModel>();

                    var newPallet = new DrocoPalletModel
                    {
                        QrCode = palletQrCode,
                        IsSent = false,
                        sentToPrinter = false,
                        CartonCodes = new List<string>(_pendingCartonCodes)
                    };
                    job.DrocoPalletList.Add(newPallet);

                    if (Shared.DrocoAllValueProcess != null)
                    {
                        foreach (var cCode in _pendingCartonCodes)
                            Shared.DrocoAllValueProcess.UpdatePalletForCarton(cCode, palletQrCode);
                    }

                    job.SaveFile();
                    ProjectLogger.WriteInfo($"[Packaging] Pallet ready: QR='{palletQrCode}', Cartons={newPallet.CartonCodes.Count}");

                    _pendingCartonCodes.Clear();
                    _pendingCartonsForPallet = 0;
                    CurrentScanState = ScanWaitState.WaitingForPalletScan;

                    OnPalletReady?.Invoke(this, new PackagingEventArgs
                    {
                        Level = PackagingLevel.Pallet,
                        QrCode = palletQrCode,
                        PendingCount = cartonsPerPallet,
                        ChildCodes = new List<string>(newPallet.CartonCodes),
                        Message = $"Đủ {cartonsPerPallet} thùng → QR Pallet: {palletQrCode}. Vui lòng quét xác nhận!"
                    });
                }
                else
                {
                    var nextUnconfirmedCarton = job.DrocoCartonList.FirstOrDefault(c => !c.IsSent);
                    if (nextUnconfirmedCarton != null)
                    {
                        CurrentScanState = ScanWaitState.WaitingForCartonScan;
                        PendingCartonQrCode = nextUnconfirmedCarton.QrCode;
                    }
                    else
                    {
                        CurrentScanState = ScanWaitState.None;
                        PendingCartonQrCode = "";
                    }
                }

                return true;
            }
        }
        public bool ProcessPalletScan(string scannedQrCode)
        {
            lock (_lock)
            {
                scannedQrCode = NormalizeScannedCode(scannedQrCode?.Trim());
                JobModel job = Shared.CurrentJob;

                var pallet = job.DrocoPalletList?.FirstOrDefault(p => p.QrCode == scannedQrCode && !p.IsSent);
                if (pallet == null)
                {
                    OnScanMismatch?.Invoke(this, new PackagingEventArgs
                    {
                        Level = PackagingLevel.Pallet,
                        QrCode = scannedQrCode,
                        Message = $"Mã pallet {scannedQrCode} không hợp lệ hoặc đã xác nhận!"
                    });
                    return false;
                }

                pallet.IsSent = true;
                job.SaveFile();

                OnPalletCompleted?.Invoke(this, new PackagingEventArgs
                {
                    Level = PackagingLevel.Pallet,
                    QrCode = pallet.QrCode,
                    Message = $"Pallet {pallet.QrCode} đã xác nhận thành công!"
                });

                var nextUnconfirmedPallet = job.DrocoPalletList.FirstOrDefault(p => !p.IsSent);
                if (nextUnconfirmedPallet != null)
                {
                    CurrentScanState = ScanWaitState.WaitingForPalletScan;
                    PendingPalletQrCode = nextUnconfirmedPallet.QrCode; 
                }
                else
                {
                    CurrentScanState = ScanWaitState.None;
                    PendingPalletQrCode = "";
                }

                return true;
            }
        }

        /// <summary>
        /// Xử lý mã lẻ khi kết thúc ca / Stop / Complete Job.
        /// </summary>
        public void FlushRemaining()
        {
            lock (_lock)
            {
                JobModel job = Shared.CurrentJob;

                // === 1. Mã GS1 lẻ → Hộp không đầy ===
                // === 1. Mã GS1 lẻ → Hộp không đầy ===
                if (_pendingCodesForBox > 0)
                {
                    string boxQrCode = GenerateQrCode(PackagingLevel.Box);
                    TotalBoxes++;
                    _qrLevelMap[boxQrCode] = PackagingLevel.Box;

                    if (job.BoxList == null)
                        job.BoxList = new List<BoxModel>();

                    var newBox = new BoxModel
                    {
                        QrCode = boxQrCode,
                        IsSent = true,
                        sentToPrinter = false,
                        ProductCodes = new List<string>(_pendingProductCodes)
                    };
                    job.BoxList.Add(newBox);

                    if (Shared.DrocoAllValueProcess != null)
                    {
                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        var normalizedCodes = _pendingProductCodes
                            .Select(c => c.Contains("\\F") ? c.Replace("\\F", "\x1D") : c)
                            .ToList();
                        Shared.DrocoAllValueProcess.UpdateBoxBatch(normalizedCodes, boxQrCode, now); // ✅ batch
                    }

                    PrintZebraLabel(boxQrCode, PackagingLevel.Box);

                    int flushedCount = _pendingCodesForBox;
                    if (flushedCount > 0)
                        ProjectLogger.WriteInfo($"[Packaging] FlushRemaining: Box '{boxQrCode}' created with {flushedCount} partial GS1 codes");
                    _pendingProductCodes.Clear();
                    _pendingCodesForBox = 0;
                    _pendingBoxesForCarton++;
                    _pendingBoxCodes.Add(boxQrCode);

                    OnBoxReady?.Invoke(this, new PackagingEventArgs
                    {
                        Level = PackagingLevel.Box,
                        QrCode = boxQrCode,
                        PendingCount = flushedCount,
                        ChildCodes = new List<string>(newBox.ProductCodes),
                        Message = $"Hộp lẻ ({flushedCount} mã) → QR Hộp: {boxQrCode}. Đã in tem!"
                    });
                }

                // === 2. Hộp lẻ → Thùng không đầy ===
                if (_pendingBoxesForCarton > 0)
                {
                    string cartonQrCode = GenerateQrCode(PackagingLevel.Carton);
                    TotalCartons++;
                    // ── [CHANGE 7b] ──
                    _qrLevelMap[cartonQrCode] = PackagingLevel.Carton;

                    if (job.DrocoCartonList == null)
                        job.DrocoCartonList = new List<DrocoCartonModel>();

                    var newCarton = new DrocoCartonModel
                    {
                        QrCode = cartonQrCode,
                        IsSent = true,
                        sentToPrinter = false,
                        BoxCodes = new List<string>(_pendingBoxCodes)
                    };
                    job.DrocoCartonList.Add(newCarton);

                    if (Shared.DrocoAllValueProcess != null)
                    {
                        foreach (var bCode in _pendingBoxCodes)
                        {
                            Shared.DrocoAllValueProcess.UpdateCarton(bCode, cartonQrCode);
                        }
                    }

                    PrintZebraLabel(cartonQrCode, PackagingLevel.Carton);

                    int flushedBoxes = _pendingBoxesForCarton;
                    _pendingBoxCodes.Clear();
                    _pendingBoxesForCarton = 0;
                    _pendingCartonsForPallet++;
                    _pendingCartonCodes.Add(cartonQrCode);

                    OnCartonReady?.Invoke(this, new PackagingEventArgs
                    {
                        Level = PackagingLevel.Carton,
                        QrCode = cartonQrCode,
                        PendingCount = flushedBoxes,
                        ChildCodes = new List<string>(newCarton.BoxCodes),
                        Message = $"Thùng lẻ ({flushedBoxes} hộp) → QR Thùng: {cartonQrCode}. Đã in tem!"
                    });
                }

                // === 3. Thùng lẻ → Pallet không đầy ===
                if (_pendingCartonsForPallet > 0)
                {
                    string palletQrCode = GenerateQrCode(PackagingLevel.Pallet);
                    TotalPallets++;
                    // ── [CHANGE 7c] ──
                    _qrLevelMap[palletQrCode] = PackagingLevel.Pallet;

                    if (job.DrocoPalletList == null)
                        job.DrocoPalletList = new List<DrocoPalletModel>();

                    var newPallet = new DrocoPalletModel
                    {
                        QrCode = palletQrCode,
                        IsSent = true,
                        sentToPrinter = false,
                        CartonCodes = new List<string>(_pendingCartonCodes)
                    };
                    job.DrocoPalletList.Add(newPallet);

                    if (Shared.DrocoAllValueProcess != null)
                    {
                        foreach (var cCode in _pendingCartonCodes)
                        {
                            Shared.DrocoAllValueProcess.UpdatePalletForCarton(cCode, palletQrCode);
                        }
                    }

                    PrintZebraLabel(palletQrCode, PackagingLevel.Pallet);

                    int flushedCartons = _pendingCartonsForPallet;
                    _pendingCartonCodes.Clear();
                    _pendingCartonsForPallet = 0;

                    OnPalletReady?.Invoke(this, new PackagingEventArgs
                    {
                        Level = PackagingLevel.Pallet,
                        QrCode = palletQrCode,
                        PendingCount = flushedCartons,
                        ChildCodes = new List<string>(newPallet.CartonCodes),
                        Message = $"Pallet lẻ ({flushedCartons} thùng) → QR Pallet: {palletQrCode}. Đã in tem!"
                    });
                }

                CurrentScanState = ScanWaitState.None;
                PendingBoxQrCode = "";
                PendingCartonQrCode = "";
                PendingPalletQrCode = "";
                job.SaveFile();
            }
        }

        //private bool PrintZebraLabel(string qrCode, BoxModel box)
        //{
        //    try
        //    {
        //        if (Shared.Settings.ZebraPrinter.PODController.IsConnected())
        //        {
        //            Shared.QRType qrType = Shared.QRType.Box;
        //            if (qrCode.StartsWith("90") || qrCode.StartsWith("91") || qrCode.StartsWith("92") || qrCode.StartsWith("93") || qrCode.StartsWith("94") ||
        //                qrCode.StartsWith("95") || qrCode.StartsWith("96") || qrCode.StartsWith("97") || qrCode.StartsWith("98") || qrCode.StartsWith("99"))
        //            {
        //                qrType = Shared.QRType.Pallet;
        //            }
        //            else if (qrCode.StartsWith("70") || qrCode.StartsWith("71") || qrCode.StartsWith("72") || qrCode.StartsWith("73") || qrCode.StartsWith("74") ||
        //                     qrCode.StartsWith("75") || qrCode.StartsWith("76") || qrCode.StartsWith("77") || qrCode.StartsWith("78") || qrCode.StartsWith("79") ||
        //                     qrCode.StartsWith("80") || qrCode.StartsWith("81") || qrCode.StartsWith("82") || qrCode.StartsWith("83") || qrCode.StartsWith("84") ||
        //                     qrCode.StartsWith("85") || qrCode.StartsWith("86") || qrCode.StartsWith("87") || qrCode.StartsWith("88") || qrCode.StartsWith("89"))
        //            {
        //                qrType = Shared.QRType.Carton;
        //            }

        //            Shared.PrintZebraDroco(qrCode, qrType);
        //            return true;
        //        }
        //        else
        //        {
        //            ProjectLogger.WriteError("Zebra printer is not connected.");
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectLogger.WriteError("Error printing Zebra label: " + ex.Message);
        //        return false;
        //    }
        //}
        private bool PrintZebraLabel(string qrCode, PackagingLevel level)
        {
            try
            {
                //  if (!Shared.Settings.ZebraPrinter.PODController.IsConnected())
                if (!Shared.IsZebraPrinterReady())
                {
                    ProjectLogger.WriteError($"[Packaging] PrintZebraLabel FAILED: Zebra not connected. Level={level}, QR='{qrCode}'");
                    return false;
                }

                Shared.QRType qrType;
                switch (level)
                {
                    case PackagingLevel.Carton: qrType = Shared.QRType.Carton; break;
                    case PackagingLevel.Pallet: qrType = Shared.QRType.Pallet; break;
                    default: qrType = Shared.QRType.Box; break;
                }

                Shared.PrintZebraDroco(qrCode, qrType);

                // ✅ Log đầy đủ khi in thành công — dùng để tra cứu sau này
                ProjectLogger.WriteInfo($"[Packaging] PRINTED: Level={level}, QR='{qrCode}', Time={DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                return true;
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError($"[Packaging] PrintZebraLabel EXCEPTION: Level={level}, QR='{qrCode}', Error={ex.Message}");
                return false;
            }
        }
        private int GetBoxSize()
        {
            var job = Shared.CurrentJob;
            if (job == null)
            {
                ProjectLogger.WriteError($"[Packaging] GetBoxSize: Shared.CurrentJob is NULL!");
                return 0;
            }
            int result = job.NumberOfCodesInBox;
            if (result <= 0)
            {
                ProjectLogger.WriteError($"[Packaging] GetBoxSize: NumberOfCodesInBox={job.NumberOfCodesInBox} is invalid, returning 0!");
                return 0;
            }
            ProjectLogger.WriteInfo($"[Packaging] GetBoxSize: NumberOfCodesInBox={job.NumberOfCodesInBox}, result={result}");
            return result;
        }

        private int GetBoxesPerCarton()
        {
            return Shared.CurrentJob.NumberOfBoxesInCarton;
        }

        private int GetCartonsPerPallet()
        {
            return Shared.CurrentJob.NumberOfCartonsInPallet;
        }

        /// <summary>
        /// Xuất báo cáo cuối ca từ AllValueProcess.
        /// </summary>
        public void ExportEndOfShiftReport(string fileName)
        {
            try
            {
                ProjectLogger.WriteError($"[ExportEndOfShiftReport] Bắt đầu xuất báo cáo cuối ca: {fileName}");

                if (Shared.DrocoAllValueProcess == null) return;

                string csvPath = Shared.DrocoAllValueProcess.getFilePath();
                if (!File.Exists(csvPath))
                {
                    ProjectLogger.WriteError($"[ExportEndOfShiftReport] Không tìm thấy file dữ liệu: {csvPath}");
                    return;
                }

                var lines = File.ReadAllLines(csvPath);
                var outputLines = new List<string>();

                string header = lines.Length > 0
                    ? lines[0].Replace(_us.ToString(), ",")
                    : "Index,QRcode,Status,Created Time,Printed Time,Checked Time,Mapped Time,QRCode Box,QRCode Carton,QRCode Pallet";
                outputLines.Add(header);

                for (int i = 1; i < lines.Length; i++)
                {
                    outputLines.Add(lines[i].Replace(_us.ToString(), ","));
                }

                File.WriteAllLines(fileName, outputLines, new UTF8Encoding(true));
                ProjectLogger.WriteError($"[ExportEndOfShiftReport] Xuất báo cáo cuối ca thành công: {fileName}");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("Error exporting end of shift report: " + ex.Message);
            }
        }
    }

    public enum PackagingLevel
    {
        Box,
        Carton,
        Pallet
    }

    public class PackagingEventArgs : EventArgs
    {
        public PackagingLevel Level { get; set; }
        public string QrCode { get; set; }
        public string Message { get; set; }
        public int PendingCount { get; set; }

        /// <summary>
        /// Danh sách mã con được liên kết vào cấp này
        /// </summary>
        public List<string> ChildCodes { get; set; } = new List<string>();
    }
}