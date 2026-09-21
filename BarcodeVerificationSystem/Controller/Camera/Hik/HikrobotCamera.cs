using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Utils;
using MvVSControlSDKNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BarcodeVerificationSystem.Controller.Camera.Hik
{
    public class CameraInfo
    {
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string DeviceIp { get; set; }
        public string MacAddress { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string PixelFormat { get; set; }
        public double FrameRate { get; set; }
        public string TriggerMode { get; set; }
        public string TriggerSource { get; set; }
        public string AcquisitionMode { get; set; }
        public double ExposureTime { get; set; }
        public string ExposureAuto { get; set; }
        public double Gain { get; set; }
        public string GainAuto { get; set; }
      
        public override string ToString()
        {
            return
                $"═══ THÔNG TIN CAMERA ═══\n" +
                $"  Model        : {ModelName}\n" +
                $"  Serial       : {SerialNumber}\n" +
                $"  Firmware     : {FirmwareVersion}\n" +
                $"  IP           : {DeviceIp}\n" +
                $"  MAC          : {MacAddress}\n" +
                $"─── THÔNG SỐ ẢNH ───────\n" +
                $"  Độ phân giải : {Width} x {Height}\n" +
                $"  Pixel Format : {PixelFormat}\n" +
                $"─── CHẾ ĐỘ HOẠT ĐỘNG ──\n" +
                $"  Acq. Mode    : {AcquisitionMode}\n" +
                $"─── PHƠI SÁNG & GAIN ───\n" +
                $"  Exposure     : {ExposureTime:F1} µs\n" +
                $"════════════════════════";
        }
    }

    public class HikrobotCamera
    {
        // ─── SDK (MvVSControlSDKNet) ──────────────────────────────
        private MV_VS_DEVICE_INFO_LIST _stDevInfoList = new MV_VS_DEVICE_INFO_LIST();
        private readonly CDevice _cDevice = new CDevice();

        // ─── State ────────────────────────────────────────────────
        private bool _isSdkOpen = false;
        private volatile bool _isGrabbing = false;      // loop flag (= m_bThreadRunning trong sample)
        private bool _isTriggerMode = false;
        private Thread _grabThread = null;
        private Bitmap _pcBitmap = null;
        private readonly object _bitmapLock = new object();

        public string CurrentIp { get; private set; }

        // ─── Latest Image ──────────────────────────────────────────
        private Bitmap _latestBitmap = null;
        private readonly object _latestBitmapLock = new object();

        // ─── Trigger pairing ──────────────────────────────────────
        private readonly object _triggerLock = new object();
        private Bitmap _lastTriggeredBitmap = null;
        private DateTime _lastTriggeredTime = DateTime.MinValue;
        private volatile bool _awaitingTriggerImage = false;
        private volatile bool _awaitingCodeResult = false;   // FIX: track trigger để báo no-read
        private readonly TimeSpan _triggerPairTimeout = TimeSpan.FromSeconds(3);
        // ─── OCR Filter ──────────────────────────────────────────
        /// <summary>Tên tool (strEnName) cần lọc từ chunk JSON. Rỗng = dùng obj_string.</summary>
        public string ObjectNameFilter { get; set; } = "";
        // ─── Chunk IDs ────────────────────────────────────────────
        private const uint CHUNK_RESULT_PORT = 60005537;

        // ─── Default password ─────────────────────────────────────
        private const string DEFAULT_PASSWORD = "Abc1234";

        // ─── Events ───────────────────────────────────────────────
        public event Action<Bitmap> OnImageReceived;
        public event Action<string> OnTextReceived;
        public event Action<string> OnQRCodeDecoded;
        public event Action<string> OnDebugLog;

        // ─── Properties ───────────────────────────────────────────
        public bool IsCameraGrabbing => _isGrabbing;
        public bool IsSdkOpen => _isSdkOpen;
        public CameraInfo LastCameraInfo { get; private set; }
        public bool IsConnected() => _isSdkOpen && _isGrabbing;

        /// <summary>No-op: MvVSControlSDKNet không cần Finalize toàn cục.</summary>
        public static void FinalizeSDK() { }

        #region ── Connection ──────────────────────────────────────

        public bool OpenCameraByIp(string cameraIp)
        {
            try
            {
                CurrentIp = cameraIp;

                if (!TryOpenSdk(cameraIp))
                {
                    DebugLog($"Không thể kết nối camera {cameraIp}.");
                    return false;
                }

                var cameraModel = Shared.GetCameraModelBasedOnIPAddress(cameraIp);
                if (cameraModel != null)
                {
                    cameraModel.IsConnected = true;

                    // Chuyển project nếu đã cấu hình tên solution
                    if (!string.IsNullOrEmpty(cameraModel.SolutionName))
                        SwitchSolution(cameraModel.SolutionName);

                    LastCameraInfo = GetCameraInfo();
                    if (LastCameraInfo != null)
                    {
                        cameraModel.Name = LastCameraInfo.ModelName ?? "";
                        cameraModel.SerialNumber = LastCameraInfo.SerialNumber ?? "";
                    }
                }

                Shared.RaiseOnCameraStatusChangeEvent();
                return true;
            }
            catch (Exception ex)
            {
                DebugLog($"OpenCamera lỗi: {ex.Message}");
                return false;
            }
        }
        /// <summary>Chuyển project/solution trên camera. Gọi sau khi SDK đã mở.</summary>
        public bool SwitchSolution(string solutionName)
        {
            if (!_isSdkOpen || string.IsNullOrEmpty(solutionName)) return false;
            try
            {
                CParam cParam = new CParam(_cDevice);
                int ret = cParam.SetStringValue("ScDeviceSwitchSolutionName", solutionName);
                if (ret != CErrorCode.MV_VS_OK)
                {
                    DebugLog($"SwitchSolution set name thất bại: 0x{ret:X}");
                    return false;
                }
                ret = cParam.SetCommandValue("ScDeviceSwitchSolution");
                DebugLog($"SwitchSolution '{solutionName}': 0x{ret:X}");
                return ret == CErrorCode.MV_VS_OK;
            }
            catch (Exception ex)
            {
                DebugLog($"SwitchSolution lỗi: {ex.Message}");
                return false;
            }
        }
        private bool TryOpenSdk(string cameraIp)
        {
            try
            {
                _stDevInfoList.nDeviceNum = 0;
                int nRet = CSystem.EnumDevices(ref _stDevInfoList);
                if (nRet != CErrorCode.MV_VS_OK || _stDevInfoList.nDeviceNum == 0)
                {
                    DebugLog("SDK: Không tìm thấy thiết bị nào.");
                    return false;
                }

                uint targetIp = ParseIpToUInt(cameraIp);
                if (targetIp == 0) return false;

                MV_VS_DEVICE_INFO stDevInfo = new MV_VS_DEVICE_INFO();
                bool bFound = false;
                for (uint i = 0; i < _stDevInfoList.nDeviceNum; i++)
                {
                    if (_stDevInfoList.pDeviceInfo[i] == IntPtr.Zero) continue;
                    stDevInfo = (MV_VS_DEVICE_INFO)Marshal.PtrToStructure(
                        _stDevInfoList.pDeviceInfo[i], typeof(MV_VS_DEVICE_INFO));
                    if (stDevInfo.nCurrentIp == targetIp) { bFound = true; break; }
                }

                if (!bFound)
                {
                    DebugLog($"SDK: Không tìm thấy camera IP {cameraIp}.");
                    return false;
                }

                nRet = _cDevice.CreateHandle(ref stDevInfo);
                if (nRet != CErrorCode.MV_VS_OK)
                {
                    DebugLog($"CreateHandle thất bại: 0x{nRet:X}");
                    return false;
                }

                nRet = _cDevice.Login("Admin", DEFAULT_PASSWORD);
                if (nRet != CErrorCode.MV_VS_OK)
                {
                    _cDevice.DestroyHandle();
                    DebugLog($"Login thất bại: 0x{nRet:X} — kiểm tra mật khẩu camera.");
                    return false;
                }

                _isSdkOpen = true;
                DebugLog($"SDK: Camera {stDevInfo.chModelName} đã mở thành công.");
                return true;
            }
            catch (Exception ex)
            {
                DebugLog($"TryOpenSdk lỗi: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            StopSdkGrab();

            if (_isSdkOpen)
            {
                try { _cDevice.Logout(); } catch { }
                try { _cDevice.DestroyHandle(); } catch { }
                _isSdkOpen = false;
                LastCameraInfo = null;
            }

            lock (_bitmapLock) { _pcBitmap?.Dispose(); _pcBitmap = null; }
            lock (_latestBitmapLock) { _latestBitmap?.Dispose(); _latestBitmap = null; }
            lock (_triggerLock)
            {
                _lastTriggeredBitmap?.Dispose();
                _lastTriggeredBitmap = null;
                _awaitingTriggerImage = false;
                _lastTriggeredTime = DateTime.MinValue;
            }
            _awaitingCodeResult = false;
            var cameraModel = Shared.GetCameraModelBasedOnIPAddress(CurrentIp);
            if (cameraModel != null) cameraModel.IsConnected = false;
            Shared.RaiseOnCameraStatusChangeEvent();
        }

        #endregion

        #region ── Grabbing ────────────────────────────────────────

        public bool StartSdkGrab()
        {
            if (!_isSdkOpen || _isGrabbing) return false;

            lock (_bitmapLock) { _pcBitmap?.Dispose(); _pcBitmap = null; }

            // 1. Khởi động grab thread trước (theo sample)
            _isGrabbing = true;
            _grabThread = new Thread(GrabThreadProcess)
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            _grabThread.Start();

            // 2. Bắt đầu stream
            CStream cStream = new CStream(_cDevice);
            int nRet = cStream.StartRun();
            if (nRet != CErrorCode.MV_VS_OK)
            {
                DebugLog($"StartRun thất bại: 0x{nRet:X}");
                _isGrabbing = false;
                _grabThread.Join(3000);
                return false;
            }

            DebugLog("SDK: Bắt đầu grab ảnh.");
            return true;
        }

        public void StopSdkGrab()
        {
            if (!_isGrabbing) return;

            // 1. Dừng stream trước (theo sample: StopRun → Sleep → set flag → Join)
            try
            {
                CStream cStream = new CStream(_cDevice);
                cStream.StopRun();
                Thread.Sleep(500);
            }
            catch { }

            // 2. Báo thread thoát và chờ
            _isGrabbing = false;
            _grabThread?.Join(3000);
        }

        private void GrabThreadProcess()
        {
            CStream cStream = new CStream(_cDevice);
            MV_VS_DATA stFrameData = new MV_VS_DATA();
            byte[] arrImageData = new byte[0];
            bool bIsFirstFrame = true;

            while (_isGrabbing)
            {
                int nRet = cStream.GetResultData(ref stFrameData, 300);
                if (nRet != CErrorCode.MV_VS_OK) continue;

                try
                {
                    // Khởi tạo Bitmap lần đầu
                    if (bIsFirstFrame && stFrameData.nImageWidth > 0 && stFrameData.nImageHeight > 0)
                    {
                        InitializeBitmap(ref stFrameData);
                        bIsFirstFrame = false;
                        DebugLog($"ChunkData ptr={stFrameData.pChunkData} len={stFrameData.nChunkDataLen}");
                    }

                    if (stFrameData.pImageData == IntPtr.Zero || stFrameData.nImageLen <= 0)
                        continue;

                    if (arrImageData.Length < stFrameData.nImageLen)
                        arrImageData = new byte[(int)stFrameData.nImageLen];
                    Marshal.Copy(stFrameData.pImageData, arrImageData, 0, (int)stFrameData.nImageLen);

                    Bitmap bmpResult = BuildBitmap(ref stFrameData, arrImageData);
                    if (bmpResult != null)
                    {
                        UpdateTriggerPairing(bmpResult);
                        UpdateLatestBitmap(bmpResult);
                        RaiseImageEvent(bmpResult);
                    }

                    // QR từ chunk data (camera decode on-chip)
                    ParseChunkData(ref stFrameData);

                    // FIX BUG: Camera không gửi chunk khi mã bị che (pChunkData = IntPtr.Zero)
                    // → ProcessQrResult không chạy → _awaitingCodeResult vẫn true → mất event
                    // → Kiểm tra: frame ảnh hợp lệ + đang chờ trigger result + không có chunk data
                    // → Fire no-read ngay để hệ thống nhận biết "không đọc được"
                    if (bmpResult != null
                        && _awaitingCodeResult
                        && (stFrameData.pChunkData == IntPtr.Zero || stFrameData.nChunkDataLen <= 0))
                    {
                        FireNoReadForMissingChunk();
                    }
                }
                catch { }
                finally
                {
                    cStream.ReleaseResultData(ref stFrameData);
                }
            }
        }

        private void InitializeBitmap(ref MV_VS_DATA stFrameData)
        {
            lock (_bitmapLock)
            {
                _pcBitmap?.Dispose();
                if (stFrameData.enPixelType == MvVSGvspPixelType.MVVS_PixelType_Mono8)
                {
                    _pcBitmap = new Bitmap((int)stFrameData.nImageWidth,
                        (int)stFrameData.nImageHeight, PixelFormat.Format8bppIndexed);
                    var pal = _pcBitmap.Palette;
                    for (int i = 0; i < 256; i++) pal.Entries[i] = Color.FromArgb(i, i, i);
                    _pcBitmap.Palette = pal;
                }
                else
                {
                    _pcBitmap = new Bitmap((int)stFrameData.nImageWidth,
                        (int)stFrameData.nImageHeight, PixelFormat.Format24bppRgb);
                }
            }
        }

        private Bitmap BuildBitmap(ref MV_VS_DATA stFrameData, byte[] arrImageData)
        {
            try
            {
                if (stFrameData.enPixelType == MvVSGvspPixelType.MVVS_PixelType_Jpeg)
                {
                    using (var ms = new MemoryStream(arrImageData, 0, (int)stFrameData.nImageLen))
                    using (var img = Image.FromStream(ms))
                        return new Bitmap(img);
                }

                lock (_bitmapLock)
                {
                    if (_pcBitmap == null) return null;
                    PixelFormat pf = stFrameData.enPixelType == MvVSGvspPixelType.MVVS_PixelType_Mono8
                        ? PixelFormat.Format8bppIndexed
                        : PixelFormat.Format24bppRgb;
                    BitmapData bd = _pcBitmap.LockBits(
                        new Rectangle(0, 0, _pcBitmap.Width, _pcBitmap.Height),
                        ImageLockMode.ReadWrite, pf);
                    Marshal.Copy(arrImageData, 0, bd.Scan0, (int)stFrameData.nImageLen);
                    _pcBitmap.UnlockBits(bd);
                    return (Bitmap)_pcBitmap.Clone();
                }
            }
            catch { return null; }
        }

        private void UpdateTriggerPairing(Bitmap bmp)
        {
            lock (_triggerLock)
            {
                if (!_awaitingTriggerImage) return;
                try
                {
                    _lastTriggeredBitmap?.Dispose();
                    _lastTriggeredBitmap = (Bitmap)bmp.Clone();
                    _lastTriggeredTime = DateTime.UtcNow;
                    _awaitingTriggerImage = false;
                }
                catch { _awaitingTriggerImage = false; }
            }
        }

        private void UpdateLatestBitmap(Bitmap bmp)
        {
            lock (_latestBitmapLock)
            {
                _latestBitmap?.Dispose();
                _latestBitmap = (Bitmap)bmp.Clone();
            }
        }

        private void RaiseImageEvent(Bitmap bmp)
        {
            OnImageReceived?.Invoke(bmp);
            //var cameraModel = Shared.GetCameraModelBasedOnIPAddress(CurrentIp);
            //Shared.RaiseOnCameraReadDataChangeEvent(new DetectModel
            //{
            //    Image = bmp,
            //    Text = "",
            //    RoleOfCamera = cameraModel?.RoleOfCamera ?? 0
            //});
        }

        #endregion

        #region ── Chunk Data → QR ─────────────────────────────────

        private void ParseChunkData(ref MV_VS_DATA stFrameData)
        {
            if (stFrameData.pChunkData == IntPtr.Zero || stFrameData.nChunkDataLen <= 0) return;

            byte[] arrChunkData = new byte[stFrameData.nChunkDataLen];
            Marshal.Copy(stFrameData.pChunkData, arrChunkData, 0, (int)stFrameData.nChunkDataLen);

            byte[] buf4 = new byte[4];
            uint nOffset = 0;

            while (stFrameData.nChunkDataLen > nOffset + 8)
            {
                Array.Copy(arrChunkData, (int)(stFrameData.nChunkDataLen - nOffset - 4), buf4, 0, 4);
                uint nChunkLen = BitConverter.ToUInt32(buf4.Reverse().ToArray(), 0);

                Array.Copy(arrChunkData, (int)(stFrameData.nChunkDataLen - nOffset - 8), buf4, 0, 4);
                uint nChunkId = BitConverter.ToUInt32(buf4.Reverse().ToArray(), 0);

                if (nChunkLen == 0 || nChunkLen > stFrameData.nChunkDataLen - nOffset - 8) break;

                if (nChunkId == CHUNK_RESULT_PORT)
                {
                    byte[] arrResult = new byte[nChunkLen];
                    Array.Copy(arrChunkData,
                        (int)(stFrameData.nChunkDataLen - nOffset - 8 - nChunkLen),
                        arrResult, 0, nChunkLen);
                    string json = Encoding.UTF8.GetString(arrResult).TrimEnd('\0');
                    DebugLog($"[CHUNK RAW JSON]:\n{json}");
                    ProcessQrResult(json);
                }

                nOffset += 8 + nChunkLen;
            }
        }


        /// <summary>
        /// Gọi khi frame ảnh hợp lệ đến nhưng không có chunk data.
        /// Camera không gửi chunk khi không detect được gì (mã bị che, ngoài vùng, v.v.)
        /// → Fire no-read event để hệ thống xử lý.
        /// </summary>
        private void FireNoReadForMissingChunk()
        {
            // Chỉ xử lý nếu đang chờ kết quả trigger
            if (!_awaitingCodeResult) return;
            _awaitingCodeResult = false;

            var cameraModel = Shared.GetCameraModelBasedOnIPAddress(CurrentIp);
            if (cameraModel == null) return;

            string noReadStr = cameraModel.NoReadOutputString ?? "";
            DebugLog($"[NoRead] Frame ảnh OK nhưng không có chunk data → mã bị che/ngoài vùng → '{noReadStr}'");

            OnQRCodeDecoded?.Invoke(noReadStr);
            OnTextReceived?.Invoke(noReadStr);
            Shared.RaiseOnCameraReadDataChangeEvent(new DetectModel
            {
                Image = GetPairedImage(),
                Text = noReadStr,
                RoleOfCamera = cameraModel.RoleOfCamera,
                ExtraFields = new Dictionary<string, string>()
            });
        }

        // FIX: Trim dấu ';' thừa cuối chuỗi
        // FIX: Mode-aware — OCROnly không cần barcode, BarcodeAndOCR extract cả 2
        // FIX: BarcodeAndOCR — raise event khi có OCR data dù barcode rỗng
        private void ProcessQrResult(string resultJson)
        {
            var cameraModel = Shared.GetCameraModelBasedOnIPAddress(CurrentIp);
            if (cameraModel == null)
            {
                DebugLog($"ProcessQrResult: Không tìm thấy CameraModel cho IP='{CurrentIp}'. Bỏ qua.");
                _awaitingCodeResult = false;
                return;
            }

            var mode = cameraModel.HikrobotCompareMode;
            string objectName = cameraModel.ObjectNameMaster ?? "";
            string noReadStr = cameraModel.NoReadOutputString ?? "";

            bool wasTriggered = _awaitingCodeResult;
            _awaitingCodeResult = false;

            // FIX: I/O trigger từ MVS solution — chunk data đến nhưng wasTriggered=false
            // (vì không qua TriggerFromSensor). Coi như "triggered" nếu đang Running/Processing.
            bool isIoTriggered = !wasTriggered
                && (Shared.OperStatus == OperationStatus.Running
                    || Shared.OperStatus == OperationStatus.Processing);
            bool isAnyTrigger = wasTriggered || isIoTriggered;

            DebugLog($"[ProcessQrResult] mode={mode}, triggered={wasTriggered}, ioTriggered={isIoTriggered}");

            string code;
            var extraFields = new Dictionary<string, string>();

            if (mode == HikrobotCompareMode.OCROnly)
            {
                if (cameraModel.OcrToolMappings?.Count > 0)
                {
                    extraFields = ExtractOcrFieldsByMappings(resultJson, cameraModel.OcrToolMappings);
                    code = string.Join(";", extraFields.Values.Where(v => !string.IsNullOrEmpty(v)));
                }
                else
                {
                    string ocrTool = string.IsNullOrEmpty(objectName) ? "obj_char_info_1" : objectName;
                    code = ExtractQrValue(resultJson, ocrTool);
                }

                if (string.IsNullOrEmpty(code) && extraFields.Count == 0)
                {
                    if (!isAnyTrigger) return;
                    code = noReadStr;
                }
            }
            else if (mode == HikrobotCompareMode.BarcodeAndOCR)
            {
                string barcodeTool = string.IsNullOrEmpty(objectName) ? "code_string" : objectName;
                code = (ExtractQrValue(resultJson, barcodeTool) ?? "").TrimEnd(';').Trim();

                if (cameraModel.OcrToolMappings?.Count > 0)
                {
                    extraFields = ExtractOcrFieldsByMappings(resultJson, cameraModel.OcrToolMappings);
                }
                else
                {
                    string ocrVal = ExtractQrValue(resultJson, "obj_char_info_1");
                    if (!string.IsNullOrEmpty(ocrVal))
                        extraFields["obj_char_info_1"] = ocrVal;
                }

                if (string.IsNullOrEmpty(code))
                {
                    bool hasOcrData = extraFields.Count > 0;
                    if (!isAnyTrigger && !hasOcrData) return;
                    code = noReadStr;
                }
            }
            else // BarcodeOnly
            {
                string barcodeTool = string.IsNullOrEmpty(objectName) ? "code_string" : objectName;
                code = (ExtractQrValue(resultJson, barcodeTool) ?? "").TrimEnd(';').Trim();

                if (string.IsNullOrEmpty(code))
                {
                    if (!isAnyTrigger) return;
                    code = noReadStr;
                }
            }

            code = (code ?? "").TrimEnd(';').Trim();
            DebugLog($"QR decode (chunk) [{mode}]: '{code}'" +
                     (extraFields.Count > 0 ? $" | OCR fields: {extraFields.Count}" : ""));
            OnQRCodeDecoded?.Invoke(code);
            OnTextReceived?.Invoke(code);

            Shared.RaiseOnCameraReadDataChangeEvent(new DetectModel
            {
                Image = GetPairedImage(),
                Text = code,
                RoleOfCamera = cameraModel.RoleOfCamera,
                ExtraFields = extraFields
            });
        }
        //private void ProcessQrResult(string resultJson)
        //{
        //    var cameraModel = Shared.GetCameraModelBasedOnIPAddress(CurrentIp);
        //    if (cameraModel == null)
        //    {
        //        DebugLog($"ProcessQrResult: Không tìm thấy CameraModel cho IP='{CurrentIp}'. Bỏ qua.");
        //        _awaitingCodeResult = false;
        //        return;
        //    }

        //    var mode = cameraModel.HikrobotCompareMode;
        //    string objectName = cameraModel.ObjectNameMaster ?? "";
        //    string noReadStr = cameraModel.NoReadOutputString ?? "";

        //    bool wasTriggered = _awaitingCodeResult;
        //    _awaitingCodeResult = false;

        //    DebugLog($"[ProcessQrResult] mode={mode}, objectName='{objectName}', triggered={wasTriggered}");

        //    string code;
        //    var extraFields = new Dictionary<string, string>();

        //    if (mode == HikrobotCompareMode.OCROnly)
        //    {
        //        if (cameraModel.OcrToolMappings?.Count > 0)
        //        {
        //            extraFields = ExtractOcrFieldsByMappings(resultJson, cameraModel.OcrToolMappings);
        //            code = string.Join(";", extraFields.Values.Where(v => !string.IsNullOrEmpty(v)));
        //        }
        //        else
        //        {
        //            string ocrTool = string.IsNullOrEmpty(objectName) ? "obj_char_info_1" : objectName;
        //            code = ExtractQrValue(resultJson, ocrTool);
        //        }

        //        if (string.IsNullOrEmpty(code) && extraFields.Count == 0)
        //        {
        //            if (!wasTriggered) return;
        //            code = noReadStr;
        //        }
        //    }
        //    else if (mode == HikrobotCompareMode.BarcodeAndOCR)
        //    {
        //        // Barcode: ObjectNameMaster hoặc default "code_string"
        //        string barcodeTool = string.IsNullOrEmpty(objectName) ? "code_string" : objectName;
        //        code = (ExtractQrValue(resultJson, barcodeTool) ?? "").TrimEnd(';').Trim();

        //        // OCR fields: ưu tiên OcrToolMappings đã cấu hình
        //        if (cameraModel.OcrToolMappings?.Count > 0)
        //        {
        //            extraFields = ExtractOcrFieldsByMappings(resultJson, cameraModel.OcrToolMappings);
        //        }
        //        else
        //        {
        //            // FIX BUG: Fallback — tự lấy obj_char_info_1 khi chưa cấu hình OcrToolMappings
        //            // Đây là lý do mode 3 chỉ nhận được barcode: không có mappings → không có OCR
        //            string ocrVal = ExtractQrValue(resultJson, "obj_char_info_1");
        //            if (!string.IsNullOrEmpty(ocrVal))
        //                extraFields["obj_char_info_1"] = ocrVal;
        //        }

        //        if (string.IsNullOrEmpty(code))
        //        {
        //            bool hasOcrData = extraFields.Count > 0;
        //            if (!wasTriggered && !hasOcrData) return;
        //            code = noReadStr;
        //        }
        //    }
        //    else // BarcodeOnly
        //    {
        //        string barcodeTool = string.IsNullOrEmpty(objectName) ? "code_string" : objectName;
        //        code = (ExtractQrValue(resultJson, barcodeTool) ?? "").TrimEnd(';').Trim();

        //        if (string.IsNullOrEmpty(code))
        //        {
        //            if (!wasTriggered) return;
        //            code = noReadStr;
        //        }
        //    }

        //    code = (code ?? "").TrimEnd(';').Trim();
        //    DebugLog($"QR decode (chunk) [{mode}]: '{code}'" +
        //             (extraFields.Count > 0 ? $" | OCR fields: {extraFields.Count}" : ""));
        //    OnQRCodeDecoded?.Invoke(code);
        //    OnTextReceived?.Invoke(code);

        //    Shared.RaiseOnCameraReadDataChangeEvent(new DetectModel
        //    {
        //        Image = GetPairedImage(),
        //        Text = code,
        //        RoleOfCamera = cameraModel.RoleOfCamera,
        //        ExtraFields = extraFields
        //    });
        //}




        private string ExtractQrValue(string json, string toolName = "")
        {
            if (string.IsNullOrEmpty(json)) return "";

            // FIX: toolName (từ ProcessQrResult, đọc live từ model mỗi lần) LUÔN ưu tiên hơn ObjectNameFilter
            // ObjectNameFilter chỉ là fallback khi không có toolName (external caller)
            // Thứ tự: toolName > ObjectNameFilter > obj_string mặc định
            string filterName = !string.IsNullOrEmpty(toolName) ? toolName
                              : !string.IsNullOrEmpty(ObjectNameFilter) ? ObjectNameFilter : "";

            if (!string.IsNullOrEmpty(filterName))
            {
                string pattern = "\"strEnName\"\\s*:\\s*\"" + Regex.Escape(filterName) + "\"";
                MatchCollection allMatches = Regex.Matches(json, pattern);
                if (allMatches.Count == 0) return "";

                var values = new List<string>();
                foreach (Match m in allMatches)
                {
                    int len = Math.Min(300, json.Length - m.Index);
                    string sub = json.Substring(m.Index, len);
                    var svMatch = Regex.Match(sub, "\"strValue\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"");
                    if (svMatch.Success)
                    {
                        string val = DecodeJsonEscape(svMatch.Groups[1].Value);
                        if (!string.IsNullOrEmpty(val))
                            values.Add(val);
                    }
                }
                return string.Join(";", values);
            }

            // Fallback: tìm "obj_string" đầu tiên
            var objMatch = Regex.Match(json, "\"strEnName\"\\s*:\\s*\"obj_string\"");
            if (objMatch.Success)
            {
                int len = Math.Min(300, json.Length - objMatch.Index);
                string sub = json.Substring(objMatch.Index, len);
                var svMatch = Regex.Match(sub, "\"strValue\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"");
                if (svMatch.Success) return DecodeJsonEscape(svMatch.Groups[1].Value);
            }

            return "";
        }
        /// <summary>
        /// Extract giá trị OCR từ chunk JSON chỉ cho những ToolKey được cấu hình.
        /// Dùng cùng pattern với ExtractQrValue (strEnName → strValue).
        /// </summary>
        private Dictionary<string, string> ExtractOcrFieldsByMappings(
            string json, List<OcrToolMapping> mappings)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(json) || mappings == null) return result;

            foreach (var mapping in mappings)
            {
                if (string.IsNullOrEmpty(mapping.ToolKey)) continue;

                var filterMatch = Regex.Match(json,
                    "\"strEnName\"\\s*:\\s*\"" + Regex.Escape(mapping.ToolKey) + "\"");
                if (!filterMatch.Success) continue;

                int len = Math.Min(300, json.Length - filterMatch.Index);
                string sub = json.Substring(filterMatch.Index, len);
                var svMatch = Regex.Match(sub, "\"strValue\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"");
                if (svMatch.Success)
                {
                    result[mapping.ToolKey] = DecodeJsonEscape(svMatch.Groups[1].Value);
                }
            }
            return result;
        }

        private Bitmap GetPairedImage()
        {
            lock (_triggerLock)
            {
                if (_lastTriggeredBitmap != null &&
                    (DateTime.UtcNow - _lastTriggeredTime) <= _triggerPairTimeout)
                {
                    var bmp = (Bitmap)_lastTriggeredBitmap.Clone();
                    _lastTriggeredBitmap.Dispose();
                    _lastTriggeredBitmap = null;
                    _lastTriggeredTime = DateTime.MinValue;
                    return bmp;
                }
            }
            lock (_latestBitmapLock)
            {
                return _latestBitmap != null ? (Bitmap)_latestBitmap.Clone() : null;
            }
        }

        /// <summary>
        /// R-Link gọi sau khi so sánh xong để điều khiển tín hiệu DO của camera.
        /// DO_0 = OK, DO_1 = NG — pulse rồi tự reset.
        /// Yêu cầu MVS: Line source của DO_0/DO_1 phải là "UserOutput" (không phải "Solution Result").
        /// </summary>
        public bool SetCompareOutput(bool isOk, int pulseDurationMs = 300)
        {
            if (!_isSdkOpen) return false;
            try
            {
                CParam cParam = new CParam(_cDevice);
                cParam.SetEnumValue("UserOutputSelector", 0u);
                cParam.SetBoolValue("UserOutputValue", isOk);
                cParam.SetEnumValue("UserOutputSelector", 1u);
                cParam.SetBoolValue("UserOutputValue", !isOk);
                DebugLog("SetCompareOutput: isOk=" + isOk + " → DO_0=" + isOk + ", DO_1=" + !isOk);

                if (pulseDurationMs > 0)
                {
                    new System.Threading.Timer(_ =>
                    {
                        try
                        {
                            if (!_isSdkOpen) return;
                            CParam cp = new CParam(_cDevice);
                            cp.SetEnumValue("UserOutputSelector", 0u);
                            cp.SetBoolValue("UserOutputValue", false);
                            cp.SetEnumValue("UserOutputSelector", 1u);
                            cp.SetBoolValue("UserOutputValue", false);
                        }
                        catch { }
                    }, null, pulseDurationMs, Timeout.Infinite);
                }
                return true;
            }
            catch (Exception ex)
            {
                DebugLog("SetCompareOutput lỗi: " + ex.Message);
                return false;
            }
        }

        private string DecodeJsonEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = Regex.Replace(s, @"\\u([0-9a-fA-F]{4})",
                m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
            return s.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t")
                    .Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\\/", "/");
                    //.Replace("\u001d", "\\f");
        }

        #endregion

        #region ── Trigger ─────────────────────────────────────────

        /// <summary>Đặt chế độ chạy. Gọi trước StartSdkGrab.</summary>
        /// <param name="enable">true = SingleFrame (0), false = Continuous (2)</param>
        //public bool SetTriggerMode(bool enable)
        //{
        //    _isTriggerMode = enable;
        //    if (!_isSdkOpen) return true;

        //    try
        //    {
        //        CParam cParam = new CParam(_cDevice);
        //        cParam.SetBoolValue("CommandImageMode", false);
        //        int ret = cParam.SetEnumValue("AcquisitionMode", enable ? 0u : 2u);
        //        cParam.SetIntValue("ModuleID", 0);
        //        DebugLog(enable ? "Trigger Mode: ON (SingleFrame)" : "Trigger Mode: OFF (Continuous)");
        //        return ret == CErrorCode.MV_VS_OK;
        //    }
        //    catch (Exception ex)
        //    {
        //        DebugLog($"SetTriggerMode lỗi: {ex.Message}");
        //        return false;
        //    }
        //}
        /// <summary>Đặt chế độ chạy. Gọi trước StartSdkGrab.</summary>
        /// <param name="enable">true = có trigger (SingleFrame SDK hoặc I/O), false = Continuous</param>
        public bool SetTriggerMode(bool enable)
        {
            _isTriggerMode = enable;
            if (!_isSdkOpen) return true;

            try
            {
                CParam cParam = new CParam(_cDevice);
                cParam.SetBoolValue("CommandImageMode", false);

                // FIX: Luôn dùng Continuous (2) thay vì SingleFrame (0)
                // SingleFrame yêu cầu SDK gửi AcquisitionStart → chặn I/O hardware trigger
                // Continuous: camera luôn grab, MVS solution xử lý decode khi I/O fires
                // AcquisitionStart (SoftwareTrigger) vẫn hoạt động trong Continuous mode
                int ret = cParam.SetEnumValue("AcquisitionMode", 2u); // Continuous
                cParam.SetIntValue("ModuleID", 0);
                DebugLog("AcquisitionMode: Continuous — I/O trigger qua MVS solution, Software trigger qua AcquisitionStart");
                return ret == CErrorCode.MV_VS_OK;
            }
            catch (Exception ex)
            {
                DebugLog($"SetTriggerMode lỗi: {ex.Message}");
                return false;
            }
        }
        //public bool SoftwareTrigger()
        //{
        //    if (!_isSdkOpen)
        //    {
        //        DebugLog("SoftwareTrigger: Camera chưa kết nối.");
        //        return false;
        //    }

        //    try
        //    {
        //        CParam cParam = new CParam(_cDevice);
        //        int ret = cParam.SetCommandValue("AcquisitionStart");
        //        if (ret == CErrorCode.MV_VS_OK)
        //        {
        //            DebugLog("SDK: AcquisitionStart sent.");
        //            return true;
        //        }
        //        DebugLog($"AcquisitionStart lỗi: 0x{ret:X}");
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        DebugLog($"SoftwareTrigger lỗi: {ex.Message}");
        //        return false;
        //    }
        //}

        //public bool TriggerFromSensor()
        //{
        //    lock (_triggerLock)
        //    {
        //        try
        //        {
        //            _lastTriggeredBitmap?.Dispose();
        //            _lastTriggeredBitmap = null;
        //            _lastTriggeredTime = DateTime.MinValue;
        //            _awaitingTriggerImage = _isSdkOpen && _isGrabbing;
        //            _awaitingCodeResult = _isSdkOpen && _isGrabbing; // FIX
        //        }
        //        catch
        //        {
        //            _awaitingTriggerImage = false;
        //            _awaitingCodeResult = false;
        //        }
        //    }

        //    bool sent = SoftwareTrigger();
        //    if (!sent)
        //    {
        //        lock (_triggerLock)
        //        {
        //            _awaitingTriggerImage = false;
        //            _awaitingCodeResult = false;
        //        }
        //    }
        //    return sent;
        //}
        public bool SoftwareTrigger()
        {
            if (!_isSdkOpen)
            {
                DebugLog("SoftwareTrigger: Camera chưa kết nối.");
                return false;
            }

            try
            {
                CParam cParam = new CParam(_cDevice);

                // FIX: Trigger Source = "Software Trigger" trong MVS → dùng TriggerSoftware (GenICam)
                // AcquisitionStart chỉ dùng để start continuous stream, KHÔNG fire software trigger
                int ret = cParam.SetCommandValue("TriggerSoftware");
                if (ret == CErrorCode.MV_VS_OK)
                {
                    DebugLog("SDK: TriggerSoftware sent.");
                    return true;
                }

                // Fallback: một số firmware dùng tên khác
                ret = cParam.SetCommandValue("ScDeviceSoftTrigger");
                if (ret == CErrorCode.MV_VS_OK)
                {
                    DebugLog("SDK: ScDeviceSoftTrigger sent.");
                    return true;
                }

                DebugLog($"SoftwareTrigger thất bại: TriggerSoftware=0x{ret:X}");
                return false;
            }
            catch (Exception ex)
            {
                DebugLog($"SoftwareTrigger lỗi: {ex.Message}");
                return false;
            }
        }

        public bool TriggerFromSensor()
        {
            lock (_triggerLock)
            {
                try
                {
                    _lastTriggeredBitmap?.Dispose();
                    _lastTriggeredBitmap = null;
                    _lastTriggeredTime = DateTime.MinValue;
                    _awaitingTriggerImage = _isSdkOpen && _isGrabbing;
                    _awaitingCodeResult = _isSdkOpen && _isGrabbing;
                }
                catch
                {
                    _awaitingTriggerImage = false;
                    _awaitingCodeResult = false;
                }
            }

            // Gửi TriggerSoftware — hoạt động khi camera cấu hình Trigger Source = Software Trigger
            // Không cần switch AcquisitionMode vì camera tự xử lý theo cấu hình MVS
            bool sent = SoftwareTrigger();
            if (!sent)
            {
                lock (_triggerLock)
                {
                    _awaitingTriggerImage = false;
                    _awaitingCodeResult = false;
                }
            }
            return sent;
        }
        #endregion

        #region ── Camera Info ──────────────────────────────────────

        public CameraInfo GetCameraInfo()
        {
            if (!_isSdkOpen) return null;

            var info = new CameraInfo { DeviceIp = CurrentIp };
            try
            {
                CParam cParam = new CParam(_cDevice);
                info.ModelName = SafeReadString(cParam, "DeviceModelName");
                info.SerialNumber = SafeReadString(cParam, "DeviceSerialNumber");
                info.FirmwareVersion = SafeReadString(cParam, "DeviceFirmwareVersion");
                info.Width = (int)SafeReadInt(cParam, "Width");
                info.Height = (int)SafeReadInt(cParam, "Height");
                info.ExposureTime = SafeReadFloat(cParam, "ExposureTime");
                info.AcquisitionMode = _isTriggerMode ? "SingleFrame" : "Continuous";
                DebugLog(info.ToString());
            }
            catch (Exception ex) { DebugLog($"GetCameraInfo lỗi: {ex.Message}"); }
            return info;
        }

        private string SafeReadString(CParam cParam, string node)
        {
            try
            {
                MV_VS_STRINGVALUE val = new MV_VS_STRINGVALUE();
                if (cParam.GetStringValue(node, ref val) != CErrorCode.MV_VS_OK) return "N/A";
                if (val.strValue == null) return "N/A";
                return Encoding.UTF8.GetString(val.strValue).TrimEnd('\0');
            }
            catch { return "N/A"; }
        }

        private long SafeReadInt(CParam cParam, string node)
        {
            try
            {
                MV_VS_INTVALUE val = new MV_VS_INTVALUE();
                return cParam.GetIntValue(node, ref val) == CErrorCode.MV_VS_OK
                    ? val.nCurValue : 0;
            }
            catch { return 0; }
        }

        private double SafeReadFloat(CParam cParam, string node)
        {
            try
            {
                MV_VS_FLOATVALUE val = new MV_VS_FLOATVALUE();
                return cParam.GetFloatValue(node, ref val) == CErrorCode.MV_VS_OK
                    ? val.fCurValue : 0.0;
            }
            catch { return 0.0; }
        }

        #endregion

        #region ── Helpers ──────────────────────────────────────────

        private uint ParseIpToUInt(string ip)
        {
            try
            {
                string[] p = ip.Split('.');
                if (p.Length != 4) return 0;
                return (uint.Parse(p[0]) << 24) | (uint.Parse(p[1]) << 16) |
                       (uint.Parse(p[2]) << 8) | uint.Parse(p[3]);
            }
            catch { return 0; }
        }

        private void DebugLog(string msg)
        {
            Trace.WriteLine($"[HikCamera] {msg}");
            OnDebugLog?.Invoke(msg);
        }

        #endregion
    }
}