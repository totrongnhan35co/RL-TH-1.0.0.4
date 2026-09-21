# THTrueMilk Mode 1 - Kiểm Tra Logic Đổi QR/NSX/HSD

## Câu Hỏi Cần Kiểm Tra

1. ✅ Mode 1 có đảm bảo **1 ngày đổi QR code 1 lần** không?
2. ✅ Có **đổi NSX và HSD khi qua ngày mới** không?
3. ✅ Khi đổi QR xong, có **gửi QR mới xuống máy in** không?
4. ✅ **Buffer** khi đổi QR được xử lý thế nào?

---

## 1. Đổi QR Code 1 Lần/Ngày

### ✅ ĐẢM BẢO - Guard 1 Ngày 1 Lần

**File:** `frmJobTHTrueMilk.cs` - `HandleMode1DeltaReset()`

```csharp
// Guard chính: chỉ đổi 1 lần/ngày
private DateTime _lastDeltaResetDate = DateTime.MinValue;

if (_lastDeltaResetDate.Date == DateTime.Today)
{
    ProjectLogger.WriteWarning($"[Mode1] Bỏ qua — đã reset hôm nay ({_lastDeltaResetDate:HH:mm:ss})");
    return; // ← Không đổi QR nữa
}
```

### Thời Điểm Đổi QR

```csharp
// Tính thời điểm reset: 23:59 - DeltaMinutes
// Ví dụ: DeltaMinutes = 15 → resetAt = 23:55
DateTime resetAt = DateTime.Today.AddDays(1).AddMinutes(-delta);

// Chỉ đổi khi đã tới giờ reset
if (DateTime.Now < resetAt) return; // Chưa tới giờ, chờ
```

### 3 Trường Hợp Đổi QR

| Trường Hợp | Điều Kiện | Guard |
|------------|-----------|-------|
| **Delta Reset** | Đã tới giờ reset (23:55) + chưa reset hôm nay | `_lastDeltaResetDate.Date != DateTime.Today` |
| **Day-Boundary** | QR từ ngày cũ (_currentBatchDate < Today) | Bỏ qua delta check, đổi ngay |
| **Cold Start** | Restart app, chưa có QR | Fetch ngay, không set `_lastDeltaResetDate` |

### ✅ Kết Luận
- **Đảm bảo 1 ngày đổi QR 1 lần** qua guard `_lastDeltaResetDate`
- Ngoại lệ: Day-Boundary (QR cũ từ hôm qua) → đổi ngay

---

## 2. Đổi NSX/HSD Khi Qua Ngày Mới

### ✅ ĐẢM BẢO - Timer Tick Mỗi 1 Giây

**File:** `frmMainTHTrueMilk.cs` - `TimerDateTime_Tick()`

```csharp
// Kiểm tra mỗi giây
if (DateTime.Now.Date != _lastDateForNsxHsd)
{
    // Qua ngày mới → Đổi NSX/HSD
    string today = DateTime.Now.ToString("dd MM yy");
    string hsdDate = DateTime.Now.AddMonths(6).ToString("dd MM yy");
    
    virtualList.UpdateNsxHsd(today, hsdDate);
    SaveCurrentDataToCsv();
    _lastDateForNsxHsd = DateTime.Now.Date;
}
```

### Quy Tắc NSX/HSD

| Trường | Giá Trị | Ví Dụ |
|--------|---------|-------|
| **NSX** | Ngày hiện tại (dd MM yy) | "12 06 26" |
| **HSD** | Ngày hiện tại + 6 tháng (dd MM yy) | "12 12 26" |

### ✅ Chỉ Đổi Cho Dòng Waiting

**File:** `PrintedCodeVirtualList.cs` - `UpdateNsxHsd()`

```csharp
public void UpdateNsxHsd(string nsx, string hsd)
{
    _nsx = nsx ?? "";
    _hsd = hsd ?? "";
    
    // Chỉ đổi cho dòng Waiting
    for (int i = 0; i < _totalCount; i++)
    {
        if (_status[i] == StatusWaiting) // ← Chỉ Waiting
        {
            _nsxPerRow[i] = _nsx;
            _hsdPerRow[i] = _hsd;
        }
    }
    // Dòng đã Printed/Duplicate/Sent → giữ nguyên NSX/HSD cũ
}
```

### ✅ Kết Luận
- **Đảm bảo đổi NSX/HSD khi qua ngày mới** (00:00)
- **Chỉ đổi cho dòng Waiting** - dòng đã in/gửi giữ nguyên

---

## 3. Gửi QR Mới Xuống Máy In Sau Khi Đổi

### ✅ ĐẢM BẢO - Event Mode1QrCodeChanged

**File:** `frmJobTHTrueMilk.cs` - `HandleMode1DeltaReset()`

```csharp
// Sau khi đổi QR thành công
if (nextId >= 0 && !string.IsNullOrEmpty(nextQr) && nextQr != oldQr)
{
    // 1. Cập nhật QR mới vào RAM
    lock (_batchQrLock)
    {
        _currentBatchQrCode = nextQr;
        _currentBatchDate = DateTime.Now;
    }
    
    // 2. Đánh dấu is_used trong DB
    await MarkQrAsUsedAsync(ids, qrCodes);
    
    // 3. ★ GỌI EVENT để frmMainTHTrueMilk xử lý
    Mode1QrCodeChanged?.Invoke(this, nextQr);
    
    // 4. Kiểm tra stock QR còn đủ không
    _ = CheckQrStockAndAlertAsync(bypassCooldown: true);
}
```

### ✅ frmMainTHTrueMilk Nhận Event

**File:** `frmMainTHTrueMilk.cs` - `ParentForm_OnMode1QrCodeChanged()`

```csharp
private void ParentForm_OnMode1QrCodeChanged(object sender, string newQrCode)
{
    if (string.IsNullOrWhiteSpace(newQrCode)) return;
    
    var mode = _SelectedJob?.THJobOperatingMode;
    if (mode != THTrueMilkOperatingMode.BatchOneQrCode &&
        mode != THTrueMilkOperatingMode.AutoRefreshByTime)
        return;

    Console.WriteLine($"[Mode1/2] Nhận QR mới: {newQrCode} — bắt đầu rebuild database...");
    
    // ★ Rebuild database với QR mới
    _ = RebuildMode1DatabaseForNewDayAsync(newQrCode);
}
```

### ✅ Rebuild Database Với QR Mới

**File:** `frmMainTHTrueMilk.cs` - `RebuildMode1DatabaseForNewDayAsync()`

```csharp
private async Task RebuildMode1DatabaseForNewDayAsync(string newQrCode)
{
    await Task.Run(() =>
    {
        var virtualList = _PrintedCodeObtainFromFile as PrintedCodeVirtualList;
        
        // 1. Đổi QR cho tất cả dòng Waiting
        virtualList.UpdateQrCode(newQrCode);
        
        // 2. Ghi đè lên CSV (lưu file)
        SaveCurrentDataToCsv();
        
        // 3. Lưu job file
        _SelectedJob.SaveFile();
        
        // 4. Cập nhật UI
        SafeInvoke(this, () =>
        {
            dgvDatabase.Invalidate();
            CuzAlert.Show($"Mode 1: QR mới → {newQrCode}\nCập nhật {waitingCount} mã");
        });
    });
}
```

### ✅ Gửi Xuống Máy In (SendPODDataProductToPrinter)

**File:** `frmMainTHTrueMilk.cs` - `SendPODDataProductToPrinter()`

```csharp
// Luồng gửi data xuống máy in (chạy liên tục)
for (int codeIndex = startIndex; codeIndex < totalCount; codeIndex++)
{
    string[] codeModel;
    lock (_SyncObjCodeList)
    {
        codeModel = _PrintedCodeObtainFromFile[codeIndex];
    }
    
    // Lấy QR từ dòng data (đã được update QR mới)
    string data = string.Join(delimiter, codeModel.Skip(2).Take(codeModel.Length - 4));
    string command = $"DATA;{data}";
    
    // Gửi xuống máy in
    podController.Send(command);
}
```

### ✅ Kết Luận
- **Đảm bảo gửi QR mới xuống máy in** sau khi đổi
- Luồng gửi data chạy liên tục, tự động lấy QR mới từ database

---

## 4. Xử Lý Buffer Khi Đổi QR

### Công Thức Tính Buffer

```csharp
// File: frmMainTHTrueMilk.cs - RebuildMode1DatabaseForNewDayAsync()

// 1. Runtime buffer: số mã đã gửi printer nhưng chưa in
int runtimeBuffer = Math.Max(0, _NumberOfSentPrinter - NumberPrinted);

// 2. Config buffer: từ settings
int configBuffer = _SelectedJob?.THJobBufferCount > 0
    ? _SelectedJob.THJobBufferCount
    : (Shared.Settings.THBufferCount > 0 ? Shared.Settings.THBufferCount : 0);

// 3. Ưu tiên runtime buffer nếu có
bufferCount = runtimeBuffer > 0 ? runtimeBuffer : configBuffer;
```

### Buffer Là Gì?

```
Buffer = Số mã đã gửi xuống máy in nhưng chưa in xong

Ví dụ:
- Đã gửi 1000 mã xuống printer
- Đã in 800 mã
- Buffer = 1000 - 800 = 200 mã

→ Khi đổi QR, 200 mã buffer vẫn dùng QR còn lại (QR cũ)
→ Chỉ đổi QR cho các dòng Waiting còn lại
```

### Logic Buffer Khi Đổi QR

**File:** `PrintedCodeVirtualList.cs` - `UpdateQrCode()`

```csharp
/// <summary>Đổi QR cho tất cả dòng Waiting. Mã đã gửi/đã in giữ nguyên QR cũ.</summary>
public void UpdateQrCode(string qrCode)
{
    _qrCode = qrCode ?? "";
    
    // Chỉ đổi QR cho dòng Waiting
    for (int i = 0; i < _totalCount; i++)
    {
        if (_status[i] == StatusWaiting) // ← Chỉ Waiting
        {
            _qrCodePerRow[i] = _qrCode;
        }
    }
    // Dòng đã Printed/Duplicate/Sent → giữ nguyên QR cũ (buffer)
}
```

### Ví Dụ Buffer

```
Tổng: 1000 sản phẩm
Đã in: 800
Buffer: 50 (đã gửi printer nhưng chưa in)
Waiting còn lại: 150

Khi đổi QR:
┌─────────────────────────────────────────────────────────────┐
│ Dòng 1-800:   Printed → Giữ QR cũ (QR_OLD)                │
│ Dòng 801-850: Buffer  → Giữ QR cũ (QR_OLD)                │
│ Dòng 851-1000: Waiting → Đổi sang QR mới (QR_NEW)         │
└─────────────────────────────────────────────────────────────┘
```

### ✅ Kết Luận
- **Buffer được giữ nguyên QR cũ** khi đổi QR
- **Chỉ đổi QR cho dòng Waiting** còn lại
- Runtime buffer ưu tiên hơn config buffer

---

## 5. Các Trường Hợp Ngoại Lệ

### 5.1 DB Trống (Không Có QR)

```csharp
// File: frmJobTHTrueMilk.cs - HandleMode1DeltaReset()
if (nextId >= 0 && !string.IsNullOrEmpty(nextQr) && nextQr != oldQr)
{
    // Đổi QR thành công
}
else
{
    ProjectLogger.WriteWarning(
        $"[Mode1] Không có QR mới — {(isColdStart ? "chờ QrBank đẩy dữ liệu" : $"tiếp tục dùng QR cũ '{oldQr}'")}");
}
```

**Xử lý:**
- Giữ QR cũ
- Log warning "chờ QrBank đẩy dữ liệu"
- Không đánh dấy is_used

### 5.2 PostgreSQL Offline

```csharp
// File: frmJobTHTrueMilk.cs - GetNextQrFromDatabaseAsync()
try
{
    // Thử PostgreSQL trước
    using (var conn = new Npgsql.NpgsqlConnection(connStr))
    {
        // ...
    }
}
catch (Exception ex)
{
    // Fallback sang SQLite
    var sqliteQr = RLinkLogService.FetchQrCodesFromSQLite(lineId, 1);
    if (sqliteQr != null && sqliteQr.Count > 0)
    {
        return (sqliteQr[0].id, sqliteQr[0].qrCode);
    }
    return (-1, string.Empty);
}
```

**Xử lý:**
- Fallback sang SQLite
- Log thông tin fallback

### 5.3 QR Mới == QR Cũ

```csharp
if (nextQr != oldQr)
{
    // Đổi QR
}
else
{
    // Không đổi, tiếp tục dùng QR hiện tại
}
```

### 5.4 Job Không Chạy

```csharp
// File: frmJobTHTrueMilk.cs - HandleMode1DeltaReset()
if (Shared.OperStatus != OperationStatus.Running &&
    Shared.OperStatus != OperationStatus.Processing)
{
    // Không đổi QR
    return;
}
```

### 5.5 Restart App Giữa Ngày

```csharp
// File: frmJobTHTrueMilk.cs - LoadCurrentBatchQrFromDbAsync()
if (mode == THTrueMilkOperatingMode.BatchOneQrCode)
{
    // Mode 1: lấy QR is_used=TRUE mới nhất của hôm nay
    sql = $@"
        SELECT qr, batch, line_id
        FROM ""tb_QRInventory""
        WHERE is_used = TRUE
          AND line_id = @line_id
          AND used_at >= CURRENT_DATE
        ORDER BY used_at DESC
        LIMIT 1";
}
```

**Xử lý:**
- Load QR đang dùng từ DB
- Tiếp tục in với QR cũ

### 5.6 Restart Sang Ngày Mới

```csharp
// File: frmJobTHTrueMilk.cs - LoadCurrentBatchQrFromDbAsync()
if (mode == THTrueMilkOperatingMode.AutoRefreshByTime)
{
    // Mode 2: lấy QR is_used=FALSE gần nhất (FIFO)
    sql = $@"
        SELECT qr, batch, line_id
        FROM ""tb_QRInventory""
        WHERE is_used = FALSE
        ORDER BY id ASC
        LIMIT 1";
}
```

**Xử lý:**
- Cold start, fetch QR mới
- Reset countdown

### 5.7 DeltaMinutes = 0

```csharp
if (delta <= 0)
{
    ProjectLogger.WriteWarning("[Mode1] Bỏ qua — delta=0 (THDeltaMinutes chưa được cấu hình?)");
    return;
}
```

**Xử lý:**
- Bỏ qua delta reset
- Chỉ đổi khi day-boundary

---

## 6. Tổng Kết

| Yêu Cầu | Trạng Thái | Ghi Chú |
|---------|------------|---------|
| **1 ngày đổi QR 1 lần** | ✅ Đảm bảo | Guard `_lastDeltaResetDate` |
| **Đổi NSX/HSD khi qua ngày mới** | ✅ Đảm bảo | Timer tick mỗi 1s |
| **Gửi QR mới xuống máy in** | ✅ Đảm bảo | Event `Mode1QrCodeChanged` |
| **Buffer giữ QR cũ** | ✅ Đảm bảo | `UpdateQrCode()` chỉ đổi Waiting |
| **DB trống** | ✅ Xử lý | Giữ QR cũ, log warning |
| **PostgreSQL offline** | ✅ Xử lý | Fallback SQLite |
| **Job không chạy** | ✅ Xử lý | Không đổi QR |
| **Restart app** | ✅ Xử lý | Load QR từ DB |

---

## 7. Sơ Đồ Luồng Mode 1

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        MODE 1 - BatchOneQrCode                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ TimerMidnightReset (mỗi 1 giây)                                        │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                    │                                         │
│                                    ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ HandleMode1DeltaReset()                                                 │ │
│  │                                                                         │ │
│  │  1. Kiểm tra job đang chạy?                                            │ │
│  │     ├── Không → Bỏ qua                                                  │ │
│  │     └── Có → Tiếp tục                                                   │ │
│  │                                                                         │ │
│  │  2. Kiểm tra Cold Start? (chưa có QR)                                   │ │
│  │     ├── Có → Fetch QR ngay                                              │ │
│  │     └── Không → Tiếp tục                                                │ │
│  │                                                                         │ │
│  │  3. Kiểm tra Day-Boundary? (QR cũ từ hôm qua)                          │ │
│  │     ├── Có → Fetch QR ngay                                              │ │
│  │     └── Không → Tiếp tục                                                │ │
│  │                                                                         │ │
│  │  4. Kiểm tra Delta Reset?                                               │ │
│  │     ├── Đã reset hôm nay? → Bỏ qua                                      │ │
│  │     ├── Chưa tới giờ? → Bỏ qua                                          │ │
│  │     └── Tới giờ → Fetch QR mới                                          │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                    │                                         │
│                                    ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ GetNextQrFromDatabaseAsync()                                            │ │
│  │                                                                         │ │
│  │  1. Thử PostgreSQL                                                      │ │
│  │     ├── Thành công → Trả về QR                                          │ │
│  │     └── Lỗi → Fallback SQLite                                           │ │
│  │                                                                         │ │
│  │  2. Fallback SQLite                                                     │ │
│  │     ├── Có QR → Trả về QR                                               │ │
│  │     └── Không có → Trả về (-1, "")                                      │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                    │                                         │
│                                    ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ Cập nhật QR mới                                                         │ │
│  │                                                                         │ │
│  │  1. Cập nhật _currentBatchQrCode                                        │ │
│  │  2. MarkQrAsUsedAsync() → is_used = TRUE                                │ │
│  │  3. Mode1QrCodeChanged?.Invoke() → Gọi event                           │ │
│  │  4. CheckQrStockAndAlertAsync() → Kiểm tra stock                        │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                    │                                         │
│                                    ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ frmMainTHTrueMilk - ParentForm_OnMode1QrCodeChanged()                  │ │
│  │                                                                         │ │
│  │  1. RebuildMode1DatabaseForNewDayAsync()                                │ │
│  │     ├── UpdateQrCode() → Đổi QR cho Waiting                             │ │
│  │     ├── SaveCurrentDataToCsv() → Lưu file                               │ │
│  │     └── _SelectedJob.SaveFile() → Lưu job                               │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                    │                                         │
│                                    ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ SendPODDataProductToPrinter() - Luồng gửi liên tục                    │ │
│  │                                                                         │ │
│  │  for (int codeIndex = startIndex; codeIndex < totalCount; codeIndex++)  │ │
│  │  {                                                                      │ │
│  │      codeModel = _PrintedCodeObtainFromFile[codeIndex];                 │ │
│  │      data = codeModel[2]; // QR đã được update                          │ │
│  │      podController.Send($"DATA;{data}");                                │ │
│  │  }                                                                      │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 8. Liên Kết File Code

| File | Class | Method | Mô Tả |
|------|-------|--------|--------|
| `frmJobTHTrueMilk.cs` | `frmJobTHTrueMilk` | `HandleMode1DeltaReset()` | Logic đổi QR Mode 1 |
| `frmJobTHTrueMilk.cs` | `frmJobTHTrueMilk` | `GetNextQrFromDatabaseAsync()` | Lấy QR từ DB |
| `frmJobTHTrueMilk.cs` | `frmJobTHTrueMilk` | `LoadCurrentBatchQrFromDbAsync()` | Restore QR khi restart |
| `frmMainTHTrueMilk.cs` | `frmMainTHTrueMilk` | `ParentForm_OnMode1QrCodeChanged()` | Nhận event đổi QR |
| `frmMainTHTrueMilk.cs` | `frmMainTHTrueMilk` | `RebuildMode1DatabaseForNewDayAsync()` | Rebuild database với QR mới |
| `frmMainTHTrueMilk.cs` | `frmMainTHTrueMilk` | `TimerDateTime_Tick()` | Đổi NSX/HSD khi qua ngày mới |
| `frmMainTHTrueMilk.cs` | `frmMainTHTrueMilk` | `SendPODDataProductToPrinter()` | Gửi QR xuống máy in |
| `PrintedCodeVirtualList.cs` | `PrintedCodeVirtualList` | `UpdateQrCode()` | Đổi QR cho Waiting |
| `PrintedCodeVirtualList.cs` | `PrintedCodeVirtualList` | `UpdateNsxHsd()` | Đổi NSX/HSD cho Waiting |

---

*Cập nhật lần cuối: 2026-06-12*
