# CV-X (Keyence) — Hướng dẫn tích hợp `CvxCamera` vào THTrueMilk

> Driver: `BarcodeVerificationSystem\Controller\Camera\Keyence\CVX_EX.cs` (commented).
> Sau migrate → paste sang `CvxCamera.cs`, xóa `CVX_EX.cs`.

## Trạng thái pipeline

- ✅ Enum `CameraType.CV_X` đã có trong `CameraModel.cs`.
- ✅ `frmMainTHTrueMilk.cs` đã có `case CameraType.CV_X` (Shared_OnCameraReadDataChange + ChangePictureCamera).
- ✅ `FireRLinkCameraError()` camera-agnostic → CSV tự ghi đúng.
- ⚠️ CHỈ cần sửa `frmJobTHTrueMilk.cs` (Manufacturing).

---

## 1. `frmJobTHTrueMilk.cs` (Manufacturing) — BẮT BUỘC

Path: `BarcodeVerificationSystem\View\OtherProjects\THTrueMilkUI\Manufacturing\frmJobTHTrueMilk.cs`

### 1.1. Thêm using (đầu file)

    using BarcodeVerificationSystem.Controller.Camera.Keyence;

### 1.2. Thêm field (cùng chỗ DMCamera, ISSingleHandler)

    public CvxCamera CvxCamera; // Keyence CV-X TCP (text + image)

### 1.3. Thêm case trong `MonitorCameraConnection_CognexSupport()`

Vị trí: sau `case CameraType.ISDual:` trong `switch (cameraModel.CameraType)`.

    case CameraType.CV_X:
        if (CvxCamera == null)
        {
            int.TryParse(cameraModel.Port, out int cvxPort);
            if (cvxPort <= 0) cvxPort = 8500;

            CvxCamera = new CvxCamera(cameraModel.IP, cvxPort, 5000);
            // CvxCamera.ConfigureImageChannel(cvxPort + 1); // optional
            if (CvxCamera.Connect())
            {
                CvxCamera.StartListening();
                cameraModel.IsConnected = true;
            }
        }
        else if (!CvxCamera.IsConnected())
        {
            CvxCamera.Disconnect();
            if (CvxCamera.Connect())
            {
                CvxCamera.StartListening();
                cameraModel.IsConnected = true;
            }
            cameraModel.CountTimeReconnect++;
            if (cameraModel.CountTimeReconnect >= 2)
                cameraModel.CountTimeReconnect = 0;
        }
        else
        {
            cameraModel.IsConnected = true;
        }
        break;

### 1.4. Helper Dispose (cạnh `DisposeSingleHandler` / `DisposeMultiSyncHandler`)

    private void DisposeCvxCamera()
    {
        try
        {
            CvxCamera?.StopListening();
            CvxCamera?.Disconnect();
        }
        catch { }
        CvxCamera = null;
    }

### 1.5. Gọi `DisposeCvxCamera()` ở các điểm cleanup

Trong `MonitorCameraConnection_CognexSupport`, sửa block dispose camera khác:

    DisposeSingleHandler();
    DisposeMultiSyncHandler();
    DMCamera?.Disconnect();
    DisposeCvxCamera();   // <-- thêm

Trong `FormClosing` / cleanup tổng cũng thêm 1 lần.

---

## 2. `frmMainTHTrueMilk.cs` — KIỂM TRA (đã ready)

- `Shared_OnCameraReadDataChange`: verify có `case CameraType.CV_X` enqueue `detect` (giống `HIKROBOT`).
- `ChangePictureCamera()`: verify gán `pictureBoxPreview.Image` từ `detect.Image`, fallback Bitmap(100,100) khi null.
- `FireRLinkCameraError()`: KHÔNG cần sửa — agnostic theo CameraType.
- `BuildErrorImagePath` / `NewExportImageToFile`: ảnh NG lưu vào `THErrorImageFolder\<jobName>\...bmp`.

---

## 3. (Tùy chọn) `CameraModel.cs` — thêm `ImagePort`

    public int ImagePort { get; set; } = 0; // 0 = auto = Port + 1

Khi đó trong case CV_X mục 1.3:

    int imgPort = cameraModel.ImagePort > 0 ? cameraModel.ImagePort : cvxPort + 1;
    CvxCamera.ConfigureImageChannel(imgPort);

## 4. (Tùy chọn) `ucCameraSettings.cs` — UI nhập ImagePort

`case CameraType.CV_X` đã có (dòng 118-124). Nếu thêm field 3:
- Designer: thêm `NumericUpDown numCamImagePort`.
- Bind: `numCamImagePort.Value = _CameraModel.ImagePort;`
- Visible khi `KeyenceRad.Checked && CameraType == CV_X`.

---

## 5. Checklist

- [ ] Migrate `CVX_EX.cs` → `CvxCamera.cs` (uncomment + xóa file scratch).
- [ ] Build pass.
- [ ] Settings: brand Keyence + CameraType `CV_X` + IP + Port → Connect OK.
- [ ] Test giả lập: `CvxCamera.SimulateFrame("ABC", true, null)` → frmMain Valid.
- [ ] Test giả lập: `CvxCamera.SimulateFrame("ABC", false, null)` → frmMain Invalid + CSV `rlink_log_camera_error_*.csv` ghi 1 dòng.
- [ ] Test camera thật: text + image về cùng frame, CSV log đúng khi NG.
- [ ] Tắt image channel (port image sai): camera vẫn chạy text-only, CSV `image_path` rỗng.

---

## 6. Tham khảo nhanh — Format CV-X

| Channel | Port              | Format gửi từ CV-X                                                              |
|---------|-------------------|----------------------------------------------------------------------------------|
| Text    | `Port` (settings) | `<QR>,<OCR_STATUS>\r\n` — OCR_STATUS = OK/1/PASS/TRUE → Valid; còn lại → Invalid |
| Image   | `Port + 1`        | `[length:4 byte big-endian][image bytes JPG/BMP/PNG]`                            |

CV-X cấu hình "ghi ảnh xong → gửi text" để FIFO khớp 1-1 (mỗi text frame pair với 1 ảnh đầu queue).