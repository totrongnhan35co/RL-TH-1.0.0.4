# THTrueMilk - Tổng Hợp Tính Năng Đang Hoạt Động

## Tổng Quan

Hệ thống THTrueMilk là một phần của Barcode Verification System, được thiết kế để quản lý và giám sát quy trình sản xuất, in ấn, kiểm tra mã vạch và phân phối sản phẩm. Hệ thống tích hợp với R-Link Master API và QrBank Server để đảm bảo hoạt động liên tục cả online và offline.

---

## 1. Kiến Trúc Hệ Thống

### 1.1. Các Thành Phần Chính

| Thành Phần | Mô Tả |
|------------|-------|
| **RLinkMasterService** | Service chính giao tiếp với R-Link Master API |
| **RLinkMonitorService** | Gửi heartbeat và monitor định kỳ |
| **RLinkLogService** | Ghi log vào SQLite và PostgreSQL |
| **RLinkLogRetrySenderService** | Retry gửi log khi mất kết nối |
| **RLinkAutoCleanupService** | Tự động dọn dẹp dữ liệu cũ |
| **QrBankAPIHandler** | HTTP server nhận QR code từ QrBank |
| **ManufacturingService** | API service cho Manufacturing process |
| **DispatchingService** | API service cho Dispatching process |
| **LocalAccountStore** | Lưu và đồng bộ tài khoản |

### 1.2. Cơ Sở Dữ Liệu

#### SQLite (Local)
- **RLinkLog.db**: Lưu log offline khi mất kết nối PostgreSQL
- **Vị trí**: `%ProgramData%\RLinkData\RLinkLog.db`

#### PostgreSQL (Server)
- **tb_QRInventory**: Bảng chứa QR code từ QrBank
- **tb_PrintingLogs**: Log quá trình in
- **tb_CameraLogs**: Log kiểm tra camera
- **tb_ProductDefectLogs**: Log lỗi sản phẩm
- **tb_AllocatedQr**: QR đã phân bổ
- **tb_IssuanceQRLogs**: Lịch sử nhận QR
- **tb_Products**: Danh sách sản phẩm
- **tb_Setting**: Cấu hình hệ thống
- **tb_DeviceAccount**: Tài khoản thiết bị
- **tb_DevicePermission**: Phân quyền
- **tb_Configline**: Cấu hình dây chuyền
- **tb_CompletedJobLogs**: Log job hoàn thành

---

## 2. Tính Năng Chi Tiết

### 2.1. Xác Thực và Quản Lý Tài Khoản

#### Đăng Nhập Online
- **Endpoint**: `POST /api/rlink/auth/login`
- **Chức năng**: Đăng nhập với R-Link Master
- **Tự động refresh token** khi hết hạn
- **Lưu credentials** để re-login tự động

#### Đăng Nhập Offline
- **Lưu mật khẩu mã hóa** trong PostgreSQL local
- **Xác thực offline** khi mất kết nối
- **Đồng bộ tài khoản** từ R-Link Master về local

#### Đồng Bộ Tài Khoản
```csharp
// Đồng bộ từ R-Link Master về PostgreSQL và SQLite
LocalAccountStore.SyncAccountsFromRLinkAsync(accounts);
```

### 2.2. Quản Lý QR Code (QrBank)

#### QrBank HTTP Server
- **Port**: 5002 (mặc định)
- **Endpoints**:
  - `POST /api/qrbank/login` - Đăng nhập
  - `POST /api/qrbank/refresh-token` - Làm mới token
  - `POST /api/qrbank/data` - Nhận QR code
  - `GET /api/qrbank/ping` - Kiểm tra kết nối

#### Nhận QR Code
- **Hỗ trợ batch**: Nhận nhiều QR cùng lúc
- **Tự động tạo bảng** nếu chưa tồn tại
- **Xử lý trùng lặp**: Bỏ qua QR đã tồn tại
- **Lưu lịch sử**: Ghi nhận thời gian nhận, số lượng

#### Lưu Trữ QR
```sql
-- PostgreSQL: tb_QRInventory
INSERT INTO "tb_QRInventory" (qr, batch, line_id, line_name, factory_code, received_at)
VALUES (@qr, @batch, @line_id, @line_name, @factory_code, @received_at)
ON CONFLICT (qr) DO NOTHING
```

### 2.3. Ghi Log

#### Các Loại Log

##### 1. Log In (tb_PrintingLogs)
- **Mục đích**: Ghi nhận trạng thái job in
- **Trạng thái**: start, running, stop, completed
- **Dữ liệu**:
  - Thông tin job (job_name, batch, product)
  - Số lượng đã in (qty)
  - QR code đã sử dụng
  - Thời gian in cuối (last_printed_at)

##### 2. Log Camera (tb_CameraLogs)
- **Mục đích**: Ghi nhận kết quả kiểm tra camera
- **Dữ liệu**:
  - Số lượng OK (status_good)
  - Số lượng FAIL (status_fail)
  - Tổng số kiểm tra (total_check)
  - Frame info

##### 3. Log Lỗi Camera (tb_ProductDefectLogs)
- **Mục đích**: Chi tiết lỗi phát hiện
- **Dữ liệu**:
  - QR code lỗi
  - Loại lỗi (error_type)
  - ảnh lỗi (image_path)
  - Frame info

#### Cơ Chế Ghi Log
1. **Ghi vào SQLite** (luôn thành công, offline-safe)
2. **Ghi vào PostgreSQL** (nếu online)
3. **Đánh dấu is_sent=0** cho log chưa gửi lên R-Link Master

### 2.4. Retry Gửi Log

#### RLinkLogRetrySenderService
- **Chu kỳ**: 60 giây (cấu hình được)
- **Hoạt động**:
  1. Quét log chưa gửi (is_sent=0) từ SQLite/PostgreSQL
  2. Gửi lên R-Link Master API
  3. Đánh dấu is_sent=1 nếu thành công
  4. Báo cáo sync status

#### API Endpoints Cho Log
| Endpoint | Mô Tả |
|----------|-------|
| `POST /api/rlink/log/status` | Gửi log in |
| `POST /api/rlink/log/camera` | Gửi log camera |
| `POST /api/rlink/log/camera/error` | Gửi log lỗi camera |
| `POST /api/rlink/log/sync-report` | Báo cáo đồng bộ |
| `POST /api/rlink/log/mark-used` | Đánh dấu QR đã dùng |
| `POST /api/rlink/log/mark-unused` | Bỏ đánh dấu QR |

### 2.5. Monitor và Heartbeat

#### RLinkMonitorService
- **Monitor Interval**: Cấu hình từ settings (mặc định 10s)
- **Heartbeat Interval**: 5 giây

#### Dữ Liệu Monitor
```csharp
public class MonitorPayload
{
    public string RLinkName { get; set; }
    public string LineId { get; set; }
    public string FactoryCode { get; set; }
    public string IpAddress { get; set; }
    public string Status { get; set; } // "running" hoặc "stop"
    public bool IsPrinterConnected { get; set; }
    public bool IsCameraConnected { get; set; }
    public bool IsPlcConnected { get; set; }
    public bool IsDatabaseConnected { get; set; }
    public string IpAddressPrinter { get; set; }
    public string IpAddressCamera { get; set; }
    public string IpAddressPlc { get; set; }
    public string OperatorUser { get; set; }
    public string ImageErrorFolder { get; set; }
    public int TotalImageErrorJob { get; set; }
    public int TotalImageError { get; set; }
    public DateTime Timestamp { get; set; }
}
```

#### API Endpoints
| Endpoint | Mô Tả |
|----------|-------|
| `POST /api/rlink/monitor` | Gửi monitor data |
| `GET /api/rlink/heartbeat` | Heartbeat |
| `GET /api/rlink/health` | Health check |

### 2.6. Quản Lý Sản Phẩm

#### Đồng Bộ Sản Phẩm
- **Nguồn**: R-Link Master API
- **Lưu trữ**: PostgreSQL (tb_Products) + SQLite mirror
- **Cập nhật**: Tự động khi login hoặc sync

#### Cấu Trúc Bảng
```sql
CREATE TABLE tb_Products (
    product_id   TEXT PRIMARY KEY,
    product_name TEXT,
    volume       INTEGER DEFAULT 0,
    exp          INTEGER DEFAULT 0,
    data_json    TEXT,
    updated_at   TIMESTAMPTZ DEFAULT NOW()
);
```

### 2.7. Quản Lý Cấu Hình

#### Cấu Hình Dây Chuyền (Settings)
- **Lưu trữ**: PostgreSQL (tb_Setting) + SQLite mirror
- **Theo line_id**: Mỗi dây chuyền có cấu hình riêng

#### Cấu Trúc
```sql
CREATE TABLE tb_Setting (
    line_id    TEXT PRIMARY KEY,
    data_json  TEXT,
    updated_at TIMESTAMPTZ DEFAULT NOW()
);
```

### 2.8. Tự Động Dọn Dẹp (RLinkAutoCleanupService)

#### Chạy Mỗi 6 Giờ
- **Xóa file job cũ** (.rvis) sau N ngày (mặc định 180 ngày)
- **Xóa ảnh lỗi cũ** sau N ngày
- **Xóa file dữ liệu job cũ** (checked, sent, printed)
- **Xóa log PostgreSQL cũ** sau N ngày

#### Cấu Hình
```csharp
// Shared.Settings.THRetentionDays (mặc định 180)
```

### 2.9. Manufacturing Process

#### ManufacturingService
- **PostMonitorDataAsync**: Gửi monitor data lên Manufacturing API
- **GetProcessOrderAsync**: Lấy thông tin process order
- **GetReservationAsync**: Lấy thông tin reservation
- **GetBatchInfoAsync**: Lấy thông tin batch
- **PostDestroyCodesAsync**: Gửi mã hủy
- **PostGeneratedCodesAsync**: Gửi mã đã tạo
- **PostPrintedAmountAsync**: Gửi số lượng đã in
- **PostPrintedDataAsync**: Gửi dữ liệu đã in
- **PostReservationAsync**: Gửi reservation

### 2.10. Dispatching Process

#### DispatchingService
- **GetListReprintDataAsync**: Lấy danh sách mã cần in lại
- **GetOrderInfoAsync**: Lấy thông tin đơn hàng
- **GetPrintedAmountDataAsync**: Lấy số lượng đã in
- **PostDestroyDataAsync**: Gửi dữ liệu hủy
- **PostMonitorDataAsync**: Gửi monitor data
- **PostPrintedDataAsync**: Gửi dữ liệu đã in

---

## 3. Luồng Hoạt Động

### 3.1. Khởi Động Hệ Thống

```
1. Khởi động ứng dụng
2. Load cấu hình từ settings
3. Khởi tạo RLinkMasterService
4. Đăng nhập R-Link Master (online/offline)
5. Đồng bộ tài khoản, sản phẩm, cấu hình
6. Khởi động RLinkMonitorService
7. Khởi động RLinkLogRetrySenderService
8. Khởi động RLinkAutoCleanupService
9. Khởi động QrBankAPIHandler (HTTP Server)
```

### 3.2. Nhận QR Code

```
QrBank Server → POST /api/qrbank/data
    ↓
QrBankAPIHandler
    ↓
QrBankDataController.HandleData()
    ↓
Validate token
    ↓
Parse JSON → List<QR>
    ↓
SaveQrToPostgres() → PostgreSQL (tb_QRInventory)
    ↓
InsertQrCodeToSQLiteStatic() → SQLite (code)
    ↓
SaveReceiveHistoryToPostgres() → PostgreSQL (tb_IssuanceQRLogs)
    ↓
SaveReceiveHistoryToSQLite() → SQLite (qr_receive_history)
    ↓
DataReceived event → UI update
```

### 3.3. Gửi Log

```
Sự kiện in/kiểm tra
    ↓
RLinkLogService.InsertLogInToSQLite() → SQLite
    ↓
RLinkLogService.InsertLogInToPostgres() → PostgreSQL
    ↓
RLinkLogRetrySenderService (mỗi 60s)
    ↓
GetUnsentLogIn() → Lấy log is_sent=0
    ↓
SendLogStatusAsync() → R-Link Master API
    ↓
MarkLogRowSent() → Đánh dấu is_sent=1
```

### 3.4. Xử Lý Mất Kết Nối

```
Mất kết nối R-Link Master
    ↓
Ghi log vào SQLite (offline-safe)
    ↓
RLinkLogRetrySenderService tiếp tục retry
    ↓
Kết nối lại thành công
    ↓
Đồng bộ log từ SQLite lên R-Link Master
    ↓
Đồng bộ từ R-Link Master về local
```

---

## 4. API Endpoints Tổng Hợp

### 4.1. R-Link Master API

| Method | Endpoint | Mô Tả |
|--------|----------|-------|
| POST | `/api/rlink/auth/login` | Đăng nhập |
| POST | `/api/rlink/auth/refresh-token` | Refresh token |
| GET | `/api/rlink/accounts/device` | Lấy danh sách tài khoản |
| GET | `/api/rlink/lines` | Lấy danh sách dây chuyền |
| POST | `/api/rlink/lines/status` | Cập nhật trạng thái dây chuyền |
| POST | `/api/rlink/lines/deactivate` | Hủy gán dây chuyền |
| GET | `/api/rlink/settings/{lineId}` | Lấy cấu hình |
| GET | `/api/rlink/products` | Lấy danh sách sản phẩm |
| POST | `/api/rlink/monitor` | Gửi monitor |
| GET | `/api/rlink/heartbeat` | Heartbeat |
| GET | `/api/rlink/health` | Health check |
| POST | `/api/rlink/log/sync-report` | Báo cáo đồng bộ |
| POST | `/api/rlink/log/mark-used` | Đánh dấu QR đã dùng |
| POST | `/api/rlink/log/mark-unused` | Bỏ đánh dấu QR |
| GET | `/api/rlink/log/resync-request/{lineId}` | Kiểm tra yêu cầu resync |
| POST | `/api/rlink/log/camera` | Gửi log camera |
| POST | `/api/rlink/log/camera/error` | Gửi log lỗi camera |
| POST | `/api/rlink/log/error-image` | Gửi ảnh lỗi |
| POST | `/api/rlink/log/status` | Gửi log status |
| POST | `/api/rlink/log/complete` | Hoàn thành job |

### 4.2. QrBank API (Local Server)

| Method | Endpoint | Mô Tả |
|--------|----------|-------|
| POST | `/api/qrbank/login` | Đăng nhập QrBank |
| POST | `/api/qrbank/refresh-token` | Refresh token |
| POST | `/api/qrbank/data` | Nhận QR code |
| GET | `/api/qrbank/ping` | Kiểm tra kết nối |

---

## 5. Background Services

### 5.1. RLinkMonitorService
- **Loại**: Timer-based
- **Mục đích**: Gửi monitor và heartbeat định kỳ
- **Thread**: Main thread (Timer)

### 5.2. RLinkLogRetrySenderService
- **Loại**: Thread-based
- **Mục đích**: Retry gửi log chưa gửi
- **Thread**: Background thread
- **Priority**: BelowNormal

### 5.3. RLinkAutoCleanupService
- **Loại**: Thread-based
- **Mục đích**: Dọn dẹp dữ liệu cũ
- **Thread**: Background thread
- **Priority**: BelowNormal
- **Interval**: 6 giờ

### 5.4. QrBankAPIHandler
- **Loại**: HTTP Server
- **Mục đích**: Nhận QR code từ QrBank
- **Port**: 5002 (mặc định)

---

## 6. Xử Lý Lỗi Và Độ Bền

### 6.1. Cơ Chế Fallback

| Tình Huống | Xử Lý |
|------------|-------|
| PostgreSQL offline | Ghi vào SQLite, retry sau |
| R-Link Master offline | Lưu log local, retry định kỳ |
| Token hết hạn | Tự động refresh, re-login nếu cần |
| QR trùng lặp | Bỏ qua, ghi log |
| DB schema thay đổi | Tự động migrate cột mới |

### 6.2. Logging

- **ProjectLogger**: Ghi log vào file
- **Log Levels**: Info, Warning, Error, Debug
- **Log Location**: Cấu hình từ settings

---

## 7. Cấu Hình

### 7.1. Database Settings
```csharp
Shared.Settings.THLocalDbServer
Shared.Settings.THLocalDbPort
Shared.Settings.THLocalDbUsername
Shared.Settings.THLocalDbPassword
Shared.Settings.THLocalDbDatabase
```

### 7.2. Monitor Settings
```csharp
Shared.Settings.THMonitorInterval // giây
Shared.Settings.THRetentionDays   // ngày
Shared.Settings.THErrorImageFolder
```

### 7.3. Line Settings
```csharp
Shared.Settings.LineId
Shared.Settings.LineName
Shared.Settings.FactoryCode
Shared.Settings.RLinkName
```

---

## 8. Sơ Đồ Quan Hệ

```
┌─────────────────────────────────────────────────────────────────┐
│                    Barcode Verification System                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │   RLinkMaster │    │   QrBank     │    │  PostgreSQL  │       │
│  │   Service     │    │   API Handler│    │  Database    │       │
│  └──────┬───────┘    └──────┬───────┘    └──────┬───────┘       │
│         │                   │                   │               │
│  ┌──────┴───────┐    ┌──────┴───────┐    ┌──────┴───────┐       │
│  │   Monitor    │    │   QR Code    │    │   Log        │       │
│  │   Service    │    │   Controller │    │   Service    │       │
│  └──────┬───────┘    └──────┬───────┘    └──────┬───────┘       │
│         │                   │                   │               │
│  ┌──────┴───────┐    ┌──────┴───────┐    ┌──────┴───────┐       │
│  │   Retry      │    │   SQLite     │    │   Auto       │       │
│  │   Sender     │    │   Database   │    │   Cleanup    │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 9. Ghi Chú Quan Trọng

### 9.1. Thread Safety
- **SemaphoreSlim**: Sử dụng cho token refresh
- **Lock**: Sử dụng cho DB operations
- **ThreadPriority.BelowNormal**: Cho background services

### 9.2. Performance
- **WAL Mode**: SQLite sử dụng Write-Ahead Logging
- **Batch Insert**: Hỗ trợ insert nhiều QR cùng lúc
- **Index**: Đã tạo index cho các trường thường query

### 9.3. Migration
- **Tự động**: Hệ thống tự động thêm cột mới khi cần
- **Backward Compatible**: Hỗ trợ DB cũ

---

## 10. Liên Hệ Và Hỗ Trợ

- **Tài liệu API**: Xem file `RLinkMaster_API.md`
- **Service Reference**: Xem file `RLinkMaster_Service_Reference.md`
- **Forms Reference**: Xem file `THTrueMilk_Forms_Reference.md`

---

*Cập nhật lần cuối: 2026-06-11*
