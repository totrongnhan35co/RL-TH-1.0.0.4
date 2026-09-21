# Kế hoạch: chẩn đoán Mode 1 không đổi QR lúc nửa đêm (RELEASE)

## Mục tiêu
Chỉ THÊM LOG CHẨN ĐOÁN, KHÔNG sửa logic. Chạy RELEASE 1 lần → đọc log `[MODECHK]` → xác định cổng nào chặn.

## Bối cảnh
Chuỗi đổi QR nằm ở form cha `frmJobTHTrueMilk.cs`:
`TimerMidnightReset_Tick` (dòng 1235) → `HandleMode1DeltaReset` (dòng 1290).
Có 5 guard thoát sớm "return im lặng". Log dưới đây làm lộ cổng nào bị chặn.

---

## CÁC CHỘI CHÈN LOG (dùng để bạn đọc trước khi tôi sửa)

### 1. Log tick timer — đầu `TimerMidnightReset_Tick` (`frmJobTHTrueMilk.cs` sau dòng 1237)
Xác minh timer có chạy. KHÔNG có dòng tick = timer chết do NRE ở InitUI/init.
```csharp
// ── [DIAG] Log tick timer để xác định timer có chạy không ──
if (DateTime.Now.Second % 30 == 0)
    ProjectLogger.WriteInfo(
        $"[MODECHK] tick | mode={mode} | OperStatus={Shared.OperStatus} | QR='{_currentBatchQrCode}' | _lastDeltaResetDate={_lastDeltaResetDate:HH:mm:ss}");
```

### 2. Guard OperStatus — `HandleMode1DeltaReset` (dòng 1294-1301)
```csharp
if (Shared.OperStatus != OperationStatus.Running &&
    Shared.OperStatus != OperationStatus.Processing)
{
    ProjectLogger.WriteInfo($"[MODECHK-GUARD] OperStatus={Shared.OperStatus} ≠ Running/Processing → SKIP | QR='{_currentBatchQrCode}'");
    _mode1GuardLogCount++;
    ...
}
```

### 3. Guard delta=0 (Release) — `HandleMode1DeltaReset` (dòng 1364-1368)
```csharp
if (delta <= 0)
{
    ProjectLogger.WriteWarning(
        $"[MODECHK-GUARD] delta={delta} (THJobDeltaMinutes={_JobModel?.THJobDeltaMinutes}, THDeltaMinutes={Shared.Settings.THDeltaMinutes}) → SKIP");
    return;
}
```

### 4. Guard 1 ngày 1 lần + chưa tới giờ — `HandleMode1DeltaReset` (dòng 1385-1396)
```csharp
if (_lastDeltaResetDate.Date == DateTime.Today)
{
    ProjectLogger.WriteInfo($"[MODECHK-GUARD] đã reset hôm nay (_lastDeltaResetDate={_lastDeltaResetDate:HH:mm:ss}) → chờ máy mới");
    return;
}
if (DateTime.Now < resetAt)
{
    ProjectLogger.WriteInfo($"[MODECHK-GUARD] chưa tới giờ reset: Now={DateTime.Now:HH:mm:ss} < resetAt={resetAt:HH:mm:ss} (delta={delta} giây)");
    _mode1GuardLogCount++;
    ...
    return;
}
```

### 5. Bọc try/catch quanh init quan trọng — `OnHandleCreated` (dòng 215-218)
```csharp
try { InitMidnightResetTimer(); } catch (Exception ex) { ProjectLogger.WriteError("[DIAG-Init] InitMidnightResetTimer: " + ex.ToString()); }
try { _ = LoadCurrentBatchQrFromDbAsync(); } catch (Exception ex) { ProjectLogger.WriteError("[DIAG-Init] LoadCurrentBatchQrFromDb: " + ex.ToString()); }
```

---

## CÁCH ĐỌC KẾT QUẢ (file log trong thư mục `Logs\` của app, lọc `[MODECHK]`)
- **Không có dòng `tick`** → timer không chạy → lỗi ở `InitUI`/init (NRE ngầm do UI edit).
- **Có `tick` + `GUARD OperStatus`** → job không Running/Processing.
- **`GUARD delta=0`** → chưa cấu hình `THDeltaMinutes`/`THJobDeltaMinutes`.
- **`GUARD đã reset hôm nay`** → chờ sang ngày mới.
- **`GUARD chưa tới giờ`** → resetAt chưa đến.
- **Có fetch QR thành công nhưng frmMain không đổi** → chuyển sang soi `RebuildMode1DatabaseForNewDayAsync` (frmMain).

## Lưu ý
- `delta` đang đếm theo GIÂY qua `AddSeconds(-delta)` dù tên field là `...Minutes`.
- Mặc định: `THJobDeltaMinutes = 0` (JobModel.cs:136), `Shared.Settings.THDeltaMinutes` mặc định 300.
