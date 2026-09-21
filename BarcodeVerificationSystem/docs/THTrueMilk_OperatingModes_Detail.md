# THTrueMilk - Chi Tiết 3 Chế Độ Vận Hành (Operating Modes)

## Tổng Quan

Hệ thống THTrueMilk hỗ trợ **3 chế độ vận hành** khác nhau, mỗi mode có cách đổi QR code, NSX (Ngày Sản Xuất), HSD (Hạn Sử Dụng) riêng biệt.

| Mode | Enum Value | Tên Hiển Thị | Mô Tả Ngắn |
|------|------------|--------------|-------------|
| **Mode 1** | `BatchOneQrCode = 1` | 1 Batch = 1 QR Code | 1 QR lặp lại cho cả batch, đổi khi sang ngày mới |
| **Mode 2** | `AutoRefreshByTime = 2` | 1 batch nhiều qr code | Tự động đổi QR theo chu kỳ thời gian cấu hình |
| **Mode 3** | `ProductOneQrCode = 3` | 1 sản phẩm 1 qr code | Mỗi sản phẩm có QR riêng (không dùng PrintedCodeVirtualList) |

---

## Mode 1: BatchOneQrCode (1 Batch = 1 QR Code)

### Nguyên Lý Hoạt Động
- **1 QR code duy nhất** được lặp lại cho tất cả sản phẩm trong batch
- QR thay đổi **khi sang ngày mới** (dựa trên `DeltaMinutes`)
- NSX/HSD thay đổi **khi qua ngày mới** (00:00)

### Cấu Hình
```csharp
public class RLinkSettings
{
    public int OperatingMode { get; set; } = 1;  // Mode 1
    public int DeltaMinutes { get; set; }          // 1-15 phút, thời gian trước 12h đêm để đổi QR
    public int BufferCount { get; set; }           // Số code buffer khi đổi QR
}
```

### Điều Kiện Đổi QR Code

#### 1. Delta Reset (Mỗi Ngày 1 Lần)
```
Thời điểm: 23:59 - DeltaMinutes (mặc định 23:55 nếu DeltaMinutes=15)
Điều kiện:
  - Job đang chạy (Running hoặc Processing)
  - Chưa reset hôm nay (_lastDeltaResetDate.Date != DateTime.Today)
  - Đã tới giờ reset (DateTime.Now >= resetAt)
```

**Logic chi tiết:**
```csharp
// Tính thời điểm reset
DateTime resetAt = DateTime.Today.AddDays(1).AddMinutes(-delta);
// Ví dụ: delta=15 → resetAt = 23:55 hôm nay

// Guard: chỉ reset 1 lần/ngày
if (_lastDeltaResetDate.Date == DateTime.Today) return;
if (DateTime.Now < resetAt) return;
```

#### 2. Day-Boundary (Qua Ngày Mới)
```
Điều kiện: _currentBatchDate.Date < DateTime.Today
→ QR từ ngày cũ, cần đổi sang QR mới cho ngày mới
→ Bỏ qua delta check, đổi ngay
```

#### 3. Cold Start (Khởi Động Lại)
```
Điều kiện: _currentBatchQrCode == null (chưa có QR)
→ Fetch QR từ DB ngay lập tức
→ Không set _lastDeltaResetDate (không tính là delta reset)
```

### Điều Kiện Đổi NSX/HSD

#### Qua Ngày Mới (Timer Tick Mỗi 1 Giây)
```csharp
// File: frmMainTHTrueMilk.cs - TimerDateTime_Tick
if (DateTime.Now.Date != _lastDateForNsxHsd)
{
    string today = DateTime.Now.ToString("dd MM yy");           // NSX = ngày hiện tại
    string hsdDate = DateTime.Now.AddMonths(6).ToString("dd MM yy"); // HSD = 6 tháng sau
    virtualList.UpdateNsxHsd(today, hsdDate);
    _lastDateForNsxHsd = DateTime.Now.Date;
}
```

**Quy tắc:**
- **NSX** = Ngày hiện tại (dd MM yy)
- **HSD** = Ngày hiện tại + 6 tháng (dd MM yy)
- **Chỉ đổi cho dòng Waiting** - dòng đã gửi/đã in giữ nguyên NSX/HSD cũ

### Luồng Đổi QR Khi Sang Ngày Mới

```
TimerMidnightReset_Tick (mỗi 1s)
    ↓
HandleMode1DeltaReset()
    ↓
Kiểu tra điều kiện:
  ├── Cold Start? → Fetch QR ngay
  ├── Day-Boundary? → Fetch QR ngay  
  └── Delta Reset?
        ├── Đã reset hôm nay? → Bỏ qua
        ├── Chưa tới giờ? → Bỏ qua
        └── Tới giờ → Fetch QR mới
    ↓
GetNextQrFromDatabaseAsync()
    ├── PostgreSQL: SELECT qr FROM tb_QRInventory 
    │               WHERE is_used=FALSE ORDER BY id ASC LIMIT 1
    └── Fallback SQLite nếu PG lỗi
    ↓
MarkQrAsUsedAsync() → Đánh dấu is_used=TRUE
    ↓
Update QR cho tất cả dòng Waiting (PrintedCodeVirtualList.UpdateQrCode)
    ↓
NSX/HSD giữ nguyên (đổi khi TimerDateTime_Tick phát hiện qua ngày mới)
```

### Buffer Khi Đổi QR
```csharp
// Số dòng giữ nguyên QR cũ khi đổi sang QR mới
int bufferCount = runtimeBuffer > 0 ? runtimeBuffer : configBuffer;

// runtimeBuffer = số mã đã gửi printer nhưng chưa in
// configBuffer = THJobBufferCount hoặc THBufferCount từ settings
```

**Ví dụ:**
- Batch có 1000 sản phẩm
- Đã in 800, còn 200 Waiting
- Buffer = 50
- Khi đổi QR: 150 dòng Waiting đầu đổi QR mới, 50 dòng cuối giữ QR cũ

### Trường Hợp Ngoại Lệ Mode 1

| Tình Huống | Xử Lý |
|------------|-------|
| **DB trống (không có QR)** | Giữ QR cũ, log warning "chờ QrBank đẩy dữ liệu" |
| **PostgreSQL offline** | Fallback sang SQLite để lấy QR |
| **QR mới == QR cũ** | Không đổi, tiếp tục dùng QR hiện tại |
| **Job không chạy** | Không đổi QR (guard check Shared.OperStatus) |
| **Restart app giữa ngày** | Load QR từ DB: lấy QR is_used=TRUE mới nhất của hôm nay |
| **Restart sang ngày mới** | Cold start, fetch QR mới từ DB |
| **DeltaMinutes = 0** | Bỏ qua delta reset, chỉ đổi khi day-boundary |

---

## Mode 2: AutoRefreshByTime (1 Batch = Nhiều QR Code)

### Nguyên Lý Hoạt Động
- **Nhiều QR code** trong 1 batch
- QR thay đổi **theo chu kỳ thời gian** cấu hình (N phút)
- Mỗi QR có thể lặp lại nhiều lần trước khi đổi QR mới
- NSX/HSD thay đổi **khi qua ngày mới** (00:00)

### Cấu Hình
```csharp
public class RLinkSettings
{
    public int OperatingMode { get; set; } = 2;  // Mode 2
    public int NMinutes { get; set; }              // Chu kỳ đổi QR (phút)
    public int BufferCount { get; set; }           // Số code buffer
}
```

### Điều Kiện Đổi QR Code

#### 1. Hết Thời Gian (Countdown)
```
Điều kiện:
  - Job đang chạy (Running hoặc Processing)
  - _mode2RemainingSeconds <= 0 (đếm ngược về 0)
  
Countdown: _mode2RemainingSeconds = NMinutes * 60 (reset mỗi khi đổi QR)
```

#### 2. Cold Start (Khởi Động Lại)
```
Điều kiện: _currentBatchQrCode == null
→ Fetch QR từ DB ngay lập tức
→ Reset countdown
```

### Điều Kiện Đổi NSX/HSD

**Giống Mode 1:**
- Qua ngày mới (00:00)
- NSX = ngày hiện tại
- HSD = ngày hiện tại + 6 tháng
- Chỉ đổi cho dòng Waiting

### Luồng Đổi QR Theo Thời Gian

```
TimerMidnightReset_Tick (mỗi 1s)
    ↓
HandleMode2MidnightReset()
    ↓
Kiểm tra:
  ├── Cold Start? → Fetch QR ngay
  └── Countdown hết?
        ├── Job đang chạy? → Fetch QR mới
        └── Job dừng? → Bỏ qua
    ↓
GetNextQrFromDatabaseAsync()
    ├── PostgreSQL: SELECT qr FROM tb_QRInventory 
    │               WHERE is_used=FALSE ORDER BY id ASC LIMIT 1
    └── Fallback SQLite nếu PG lỗi
    ↓
MarkQrAsUsedAsync() → Đánh dấu is_used=TRUE
    ↓
ResetMode2Countdown() → Reset đếm ngược N phút
    ↓
Update QR cho tất cả dòng Waiting
```

### Trường Hợp Ngoại Lệ Mode 2

| Tình Huống | Xử Lý |
|------------|-------|
| **DB trống (không có QR)** | Giữ QR cũ, log warning "chờ QrBank đẩy dữ liệu" |
| **PostgreSQL offline** | Fallback sang SQLite để lấy QR |
| **QR mới == QR cũ** | Không đổi, tiếp tục dùng QR hiện tại |
| **Job không chạy** | Không đổi QR (guard check Shared.OperStatus) |
| **Restart app** | Load QR từ DB: ưu tiên QR is_used=FALSE, fallback is_used=TRUE hôm nay |
| **NMinutes = 0** | Mặc định 30 phút |

---

## Mode 3: ProductOneQrCode (1 Sản Phẩm = 1 QR Code)

### Nguyên Lý Hoạt Động
- **Mỗi sản phẩm có QR code riêng**
- Không dùng `PrintedCodeVirtualList` (vì mỗi dòng có QR khác nhau)
- Dùng `List<string[]>` thông thường
- Không có cơ chế đổi QR tự động theo thời gian

### Cấu Hình
```csharp
public class RLinkSettings
{
    public int OperatingMode { get; set; } = 3;  // Mode 3
}
```

### Đặc Điểm
- Mỗi dòng trong database có QR riêng
- Không có timer đổi QR
- Không có cơ chế buffer
- Phù hợp cho sản xuất 1-1 (1 sản phẩm = 1 mã duy nhất)

---

## So Sánh 3 Mode

| Đặc Điểm | Mode 1 | Mode 2 | Mode 3 |
|----------|--------|--------|--------|
| **Số QR trong batch** | 1 | Nhiều | Nhiều (bằng số SP) |
| **Cơ chế đổi QR** | Theo ngày (DeltaMinutes) | Theo thời gian (NMinutes) | Không có |
| **NSX/HSD** | Đổi khi qua ngày mới | Đổi khi qua ngày mới | Đổi khi qua ngày mới |
| **Data Structure** | PrintedCodeVirtualList | PrintedCodeVirtualList | List<string[]> |
| **Buffer khi đổi QR** | Có | Có | Không |
| **Timer** | TimerMidnightReset (1s) | TimerMidnightReset (1s) | Không |
| **QR trùng lặp** | Có (1 QR lặp nhiều lần) | Có (mỗi QR lặp vài lần) | Không (mỗi SP 1 QR) |

---

## PrintedCodeVirtualList - Cấu Trúc Dữ Liệu

### Mục Đích
- Lưu trữ database barcode dạng **virtual** cho Mode 1/2
- Chỉ giữ **1 prototype QR + NSX + HSD** + mảng byte status
- Tiết kiệm bộ nhớ: **3.5 MB cho 3.5M dòng** (thay vì >2 GB nếu dùng List<string[]>)

### Cấu Trúc
```csharp
internal class PrintedCodeVirtualList : IList<string[]>
{
    private readonly byte[] _status;      // 0=Waiting, 1=Printed, 2=Duplicate, 3=Reprint, 4=Sent
    private readonly string[] _sendTime;  // sparse - null nếu chưa gửi
    private readonly string[] _printTime; // sparse - null nếu chưa in
    
    // Current values (dùng cho reference)
    private string _qrCode;
    private string _nsx;
    private string _hsd;
    
    // Per-row storage - giữ lịch sử riêng cho từng dòng
    private readonly string[] _qrCodePerRow;
    private readonly string[] _nsxPerRow;
    private readonly string[] _hsdPerRow;
}
```

### Các Phương Thức Quan Trọng

#### UpdateQrCode(string qrCode)
```csharp
/// <summary>Đổi QR cho tất cả dòng Waiting. Mã đã gửi/đã in giữ nguyên QR cũ.</summary>
public void UpdateQrCode(string qrCode)
{
    _qrCode = qrCode ?? "";
    for (int i = 0; i < _totalCount; i++)
        if (_status[i] == StatusWaiting)
            _qrCodePerRow[i] = _qrCode;
}
```

#### UpdateNsxHsd(string nsx, string hsd)
```csharp
/// <summary>Đổi NSX/HSD cho tất cả dòng Waiting. Mã đã gửi/đã in giữ nguyên NSX/HSD cũ.</summary>
public void UpdateNsxHsd(string nsx, string hsd)
{
    _nsx = nsx ?? "";
    _hsd = hsd ?? "";
    for (int i = 0; i < _totalCount; i++)
        if (_status[i] == StatusWaiting)
        {
            _nsxPerRow[i] = _nsx;
            _hsdPerRow[i] = _hsd;
        }
}
```

### Status Enum
| Status | Value | Mô Tả |
|--------|-------|-------|
| Waiting | 0 | Chờ in |
| Printed | 1 | Đã in |
| Duplicate | 2 | Trùng lặp |
| Reprint | 3 | In lại |
| Sent | 4 | Đã gửi |

---

## Quy Tắc Chung Khi Đổi QR/NSX/HSD

### 1. Chỉ Đổi Cho Dòng Waiting
- Dòng đã gửi/đã in **giữ nguyên** QR/NSX/HSD cũ
- Đảm bảo lịch sử đúng cho mã đã gửi/đã in

### 2. NSX/HSD Luôn Đổi Khi Qua Ngày Mới
```
NSX = DateTime.Now.ToString("dd MM yy")
HSD = DateTime.Now.AddMonths(6).ToString("dd MM yy")
```

### 3. Guard: 1 Ngày Chỉ Đổi 1 Lần (Mode 1 Delta Reset)
```csharp
if (_lastDeltaResetDate.Date == DateTime.Today) return;
```

### 4. Guard: Job Phải Đang Chạy
```csharp
if (Shared.OperStatus != OperationStatus.Running &&
    Shared.OperStatus != OperationStatus.Processing)
    return;
```

### 5. Fallback SQLite
```csharp
// Nếu PostgreSQL lỗi hoặc offline
var sqliteQr = RLinkLogService.FetchQrCodesFromSQLite(lineId, 1);
```

---

## Cấu Hình Trong Settings

```csharp
// File: SettingsModel.cs
public THTrueMilkOperatingMode THOperatingMode { get; set; } = THTrueMilkOperatingMode.BatchOneQrCode;

// File: RLinkSettings.cs (từ R-Link Master)
public int OperatingMode { get; set; }         // 1, 2, hoặc 3
public int DeltaMinutes { get; set; }          // Mode 1: 1-15 phút
public int NMinutes { get; set; }              // Mode 2: chu kỳ đổi QR
public int BufferCount { get; set; }           // Số code buffer
```

---

## UI Hiển Thị

### Form Cấu Hình (ucProductionTHSetting)
```
┌─────────────────────────────────────────┐
│ Chế độ vận hành:                        │
│                                         │
│ ○ 1 Batch = 1 QR Code (Mode 1)         │
│   └── DeltaMinutes: [15] phút           │
│                                         │
│ ○ Tự động làm mới theo thời gian (Mode 2)
│   └── NMinutes: [30] phút               │
│                                         │
│ ○ 1 Sản phẩm = 1 QR Code (Mode 3)      │
└─────────────────────────────────────────┘
```

### Thông Báo Khi Đổi QR
```
"Mode 1: QR mới → [QR_CODE]
 Cập nhật [X] mã | Buffer: [Y] mã"
```

---

## Liên Kết File Code

| File | Mô Tả |
|------|-------|
| `THTrueMilkOperatingMode.cs` | Định nghĩa enum 3 mode |
| `RLinkSettings.cs` | Cấu hình settings từ R-Link Master |
| `PrintedCodeVirtualList.cs` | Cấu trúc dữ liệu virtual cho Mode 1/2 |
| `frmJobTHTrueMilk.cs` | Logic đổi QR Mode 1 (HandleMode1DeltaReset) |
| `frmJobTHTrueMilk.cs` | Logic đổi QR Mode 2 (HandleMode2MidnightReset) |
| `frmMainTHTrueMilk.cs` | Logic đổi NSX/HSD (TimerDateTime_Tick) |
| `frmMainTHTrueMilk.cs` | Rebuild database khi đổi QR (RebuildMode1DatabaseForNewDayAsync) |

---

*Cập nhật lần cuối: 2026-06-11*
