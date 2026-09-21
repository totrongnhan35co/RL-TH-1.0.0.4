# R-Link Master Service — Tài liệu tham chiếu (Client Layer)

> Module client gọi API của **R-Link Master Server** (hoặc **Simulator**) trong dự án THTrueMilk.
>
> **Đường dẫn folder**: `BarcodeVerificationSystem\Services\THTrueMilk\RLinkMaster\Models\`
>
> **Vai trò**: lớp HTTP client wrap toàn bộ endpoint server R-Link Master, hỗ trợ tự động refresh token / re-login khi gặp 401, cùng các DTO cho request/response.

---

## 📑 Mục lục
1. [Cấu trúc thư mục](#1-cấu-trúc-thư-mục)
2. [Interface & Service core](#2-interface--service-core)
3. [Factory & Singleton](#3-factory--singleton)
4. [DTO Models](#4-dto-models)
5. [Luồng auth & retry (401)](#5-luồng-auth--retry-401)
6. [Mapping endpoint ↔ method](#6-mapping-endpoint--method)
7. [Chuyển từ Simulator → R-Link Master thật](#7-chuyển-từ-simulator--r-link-master-thật)

---

## 1. Cấu trúc thư mục

    Services/THTrueMilk/RLinkMaster/
    └── Models/
        ├── IRLinkMasterService.cs        ← Interface (contract)
        ├── RLinkMasterService.cs         ← Implementation HTTP client
        ├── RLinkMasterServiceFactory.cs  ← Factory + Singleton + Reset
        │
        ├── LoginResult.cs                ← Auth response
        ├── AccountInfo.cs                ← Account list item
        ├── LineInfo.cs                   ← Line/Factory item
        ├── ProductItem.cs                ← Product catalog item
        ├── RLinkSettings.cs              ← Operating params
        │
        ├── MonitorPayload.cs             ← POST /monitor
        ├── LogPrintPayload.cs            ← POST /log/print
        ├── LogCameraPayload.cs           ← POST /log/camera
        ├── LogStatusPayload.cs           ← POST /log/status
        ├── ErrorImagePayload.cs          ← POST /log/error-image
        │
        ├── CompleteJobRequest.cs         ← POST /job/complete (request)
        └── CompleteJobResult.cs          ← POST /job/complete (response)

---

## 2. Interface & Service core

### 2.1 `IRLinkMasterService` — contract

| Nhóm | Method | Endpoint |
|---|---|---|
| **Connection** | `PingAsync()` | `GET /api/health` |
| | `IsAuthenticated` (prop) | — |
| **Auth** | `LoginAsync(username, password)` | `POST /api/auth/login` |
| | `RefreshTokenAsync(refreshToken)` | `POST /api/auth/refresh-token` |
| | `StoreCredentials(username, password)` | (lưu trong RAM) |
| | `GetAccountsAsync()` | `GET /api/auth/accounts` |
| **Lines** | `GetLinesAsync(factoryCode = null)` | `GET /api/rlink/lines` |
| | `SetLineStatusAsync(lineId, machineIp, factoryCode)` | `POST /api/rlink/lines/status` |
| | `UnassignLineAsync(lineId, factoryCode)` | `POST /api/rlink/lines/deactivate` |
| **Settings/Products** | `GetSettingsAsync(lineId)` | `GET /api/rlink/settings/{lineId}` |
| | `GetProductsAsync()` | `GET /api/rlink/products` |
| **Monitor** | `SendMonitorAsync(MonitorPayload)` | `POST /api/rlink/monitor` |
| **Log** | `SendLogPrintAsync(LogPrintPayload)` | `POST /api/rlink/log/print` |
| | `SendLogCameraAsync(LogCameraPayload)` | `POST /api/rlink/log/camera` |
| | `SendErrorImageAsync(ErrorImagePayload)` | `POST /api/rlink/log/error-image` |
| | `SendLogStatusAsync(LogStatusPayload)` | `POST /api/rlink/log/status` |
| **Job** | `CompleteJobAsync(CompleteJobRequest)` | `POST /api/job/complete` |

### 2.2 `RLinkMasterService` — implementation

#### Fields & State
| Field | Mô tả |
|---|---|
| `_http` (static `HttpClient`) | Client dùng chung, timeout 15s. |
| `_baseUrl` | URL gốc đã trim `/`. |
| `_accessToken`, `_refreshToken` | Token JWT hiện hành. |
| `_storedUsername`, `_storedPassword` | Backup credentials cho re-login. |
| `IsAuthenticated` | `true` khi `_accessToken` non-empty. |

#### Public methods

| Method | Hành vi |
|---|---|
| `RLinkMasterService(string baseUrl)` | Constructor, normalize URL bằng `TrimEnd('/')`. |
| `PingAsync()` | GET `/api/health`, không cần auth, log warning nếu fail, không throw. |
| `LoginAsync(u, p)` | POST `/api/auth/login`, lưu access/refresh token + credentials → trả `LoginResult`. |
| `RefreshTokenAsync(rt)` | POST `/api/auth/refresh-token`, cập nhật token mới. |
| `StoreCredentials(u, p)` | Set credentials cho trường hợp local-login offline (sau này có 401 sẽ tự re-login). |
| `GetAccountsAsync()` | GET `/api/auth/accounts` → `List<AccountInfo>` (trả list rỗng khi lỗi). |
| `GetLinesAsync(factory?)` | GET `/api/rlink/lines?factory_code=...` → `List<LineInfo>`. |
| `SetLineStatusAsync(lineId, ip, factory)` | POST `/api/rlink/lines/status` (line_id, factory_code, line_ip). |
| `UnassignLineAsync(lineId, factory)` | POST `/api/rlink/lines/deactivate`. |
| `GetSettingsAsync(lineId)` | GET `/api/rlink/settings/{lineId}` → `RLinkSettings`. |
| `GetProductsAsync()` | GET `/api/rlink/products` → `List<ProductItem>`. |
| `SendMonitorAsync(p)` | POST `/api/rlink/monitor`, log warning khi fail (không throw). |
| `SendLogPrintAsync(p)` | POST `/api/rlink/log/print`. |
| `SendLogCameraAsync(p)` | POST `/api/rlink/log/camera`. |
| `SendErrorImageAsync(p)` | POST `/api/rlink/log/error-image` (Base64 ảnh). |
| `SendLogStatusAsync(p)` | POST `/api/rlink/log/status` (4 pha start/running/stop/completed). |
| `CompleteJobAsync(req)` | POST `/api/job/complete` → `CompleteJobResult` (kèm `AllocatedQrCodes`). |

#### Internal helpers
| Member | Vai trò |
|---|---|
| `AccessToken` / `RefreshTokenValue` (internal) | Read-only access cho Factory để bảo toàn token khi `Reset`. |
| `SetTokens(at, rt)` (internal) | Inject lại token vào instance mới sau `Reset()`. |

#### Private helpers
| Method | Vai trò |
|---|---|
| `Truncate(s, max=500)` | Cắt chuỗi log để không flood file log. |
| `GetRawAsync(path)` | GET + auto retry sau khi `TryRestoreAuthAsync` nếu 401. |
| `PostRawAsync(path, body, auth=true)` | POST + auto retry như trên; log full request/response khi fail. |
| `TryRestoreAuthAsync()` | (1) Thử `RefreshTokenAsync`, (2) fallback `LoginAsync` với credentials đã lưu. |
| `BuildGet(path)` / `BuildPost(path, body, auth)` | Tạo `HttpRequestMessage` + `AttachToken`. |
| `AttachToken(req)` | Gắn header `Authorization: Bearer {token}`. |

---

## 3. Factory & Singleton

### `RLinkMasterServiceFactory` (static)

| Member | Vai trò |
|---|---|
| `const string DefaultSimulatorUrl = "http://192.168.15.70:5130"` | URL Simulator mặc định khi `Settings.ApiUrl` chưa cấu hình. |
| `static IRLinkMasterService Instance` | **Singleton** — toàn bộ project dùng chung 1 instance. |
| `static IRLinkMasterService Create()` (private) | Lazy-create từ `ResolveUrl()`, log URL ra Console. |
| `static string ResolveUrl()` (private) | **Logic chuyển sim/real**: `Shared.Settings.ApiUrl` nếu non-empty và **không** chứa `google.com` → dùng nó; ngược lại → `DefaultSimulatorUrl`. |
| `static void Reset()` | Tạo lại instance khi đổi URL trong Settings, **bảo toàn** `_accessToken` + `_refreshToken` sang instance mới. |

Cách dùng phổ biến trong code:

    var rlink = RLinkMasterServiceFactory.Instance;
    var lines = await rlink.GetLinesAsync();

---

## 4. DTO Models

### 4.1 Auth & Account

**`LoginResult`**

    public bool   IsSuccess { get; set; }
    public string Message { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public Dictionary<string, bool> Permissions { get; set; }

**`AccountInfo`**

    public string username { get; set; }
    public string full_name { get; set; }
    public string role { get; set; }
    public Dictionary<string, bool> permissions { get; set; }

### 4.2 Topology

**`LineInfo`**

    public string factory_code { get; set; }
    public string factory_name { get; set; }
    public string line_id { get; set; }
    public string line_name { get; set; }
    public string line_ip { get; set; }
    public bool   is_active { get; set; }

**`ProductItem`**

    public string ProductId { get; set; }
    public string ProductName { get; set; }

### 4.3 Operating params

**`RLinkSettings`**

    public int    OperatingMode { get; set; }   // 1=BatchOneQr | 2=AutoRefresh | 3=ProductOneQr
    public int    DeltaMinutes { get; set; }    // 1–15, dùng cho Mode 1
    public int    NMinutes { get; set; }        // dùng cho Mode 2
    public int    BufferCount { get; set; }     // số code buffer khi đổi QR
    public int    MonitoringIntervalMinutes { get; set; }
    public int    LogIntervalMinutes { get; set; }
    public int    QrThreshold { get; set; }     // ngưỡng cấp phát QR
    public string ErrorImageFolder { get; set; }
    public int    RetentionDays { get; set; } = 180;

### 4.4 Monitor & Log payloads

**`MonitorPayload`** (THTrueMilk variant — khác `Model\Nutifood\MonitorPayload.cs`)

    public string   RLinkName { get; set; }
    public string   LineId { get; set; }
    public bool     IsPrinterConnected { get; set; }
    public bool     IsCameraConnected { get; set; }
    public bool     IsPlcConnected { get; set; }
    public string   CurrentBatch { get; set; }
    public int      TotalQrAllocated { get; set; }
    public int      TotalQrUsed { get; set; }
    public int      TotalQrFailed { get; set; }
    public int      TotalProduced { get; set; }
    public string   LastErrorImageBase64 { get; set; }   // null nếu không có
    public DateTime Timestamp { get; set; } = DateTime.Now;

**`LogPrintPayload`**

    public string   line_id { get; set; }
    public string   job_name { get; set; }
    public string   batch { get; set; }
    public string   status { get; set; }      // "Start" | "Run" | "Stop"
    public int      qr_used { get; set; }
    public int      produced { get; set; }
    public DateTime timestamp { get; set; }

**`LogCameraPayload`**

    public string   line_id { get; set; }
    public string   rlink_name { get; set; }
    public string   job_name { get; set; }
    public string   batch { get; set; }
    public string   product_id { get; set; }
    public string   product_name { get; set; }
    public string   rlink_status { get; set; }
    public string   operator_user { get; set; }
    public int      camera_ok { get; set; }
    public int      camera_fail { get; set; }
    public int      total_check => camera_ok + camera_fail;
    public DateTime timestamp { get; set; }

**`LogStatusPayload`**

    public string   line_id { get; set; }
    public string   rlink_name { get; set; }
    public string   job_name { get; set; }
    public string   batch { get; set; }
    public string   product_id { get; set; }
    public string   product_name { get; set; }
    public string   status { get; set; }        // "start" | "running" | "stop" | "completed"
    public string   rlink_status { get; set; }  // "Idle" | "Running" | "Paused" | "Error"
    public string   operator_user { get; set; }
    public int      qr_used { get; set; }
    public int      produced { get; set; }
    public DateTime timestamp { get; set; }

**`ErrorImagePayload`**

    public string   line_id { get; set; }
    public string   job_name { get; set; }
    public string   batch { get; set; }
    public string   qr_code { get; set; }
    public string   image_base64 { get; set; }   // ảnh encode Base64
    public DateTime timestamp { get; set; }

### 4.5 Complete Job

**`CompleteJobRequest`**

    public string JobName { get; set; }
    public string RLinkName { get; set; }
    public int    QrUsedCount { get; set; }
    public int    QrTotalAllocated { get; set; }   // = _TotalCode
    public int    ProducedCount { get; set; }
    public int    OperatingMode { get; set; } = 3; // 1/2/3
    public int    BufferCount   { get; set; } = 1; // dành cho Mode 2

**`CompleteJobResult`**

    public bool         IsSuccess { get; set; }
    public string       Message { get; set; }
    public List<string> AllocatedQrCodes { get; set; } = new List<string>();

---

## 5. Luồng auth & retry (401)

Mọi gọi `GetRawAsync` / `PostRawAsync` đều có cơ chế:

1. Gửi request kèm Bearer `_accessToken`.
2. Nếu nhận `401 Unauthorized` → gọi `TryRestoreAuthAsync()`:
   - Bước 2a: nếu có `_refreshToken` → POST `/api/auth/refresh-token`. Thành công → cập nhật token mới.
   - Bước 2b: refresh fail và đã có `_storedUsername/Password` → POST `/api/auth/login` (re-login).
3. Sau khi auth khôi phục → gửi lại request **đúng 1 lần**.
4. Vẫn fail → log chi tiết REQUEST/RESPONSE rồi `EnsureSuccessStatusCode()` (throw).

Toàn bộ logic retry này **trong suốt** với caller — caller chỉ `await rlink.GetLinesAsync()` bình thường, không cần xử lý 401 thủ công.

---

## 6. Mapping endpoint ↔ method

> Liên kết với tài liệu API đầy đủ: [`docs/RLinkMaster_API.md`](RLinkMaster_API.md).

| Endpoint server | Method client |
|---|---|
| `GET /api/health` | `PingAsync` |
| `POST /api/auth/login` | `LoginAsync` |
| `POST /api/auth/refresh-token` | `RefreshTokenAsync` |
| `GET /api/auth/accounts` | `GetAccountsAsync` |
| `GET /api/rlink/lines` | `GetLinesAsync` |
| `POST /api/rlink/lines/status` | `SetLineStatusAsync` |
| `POST /api/rlink/lines/deactivate` | `UnassignLineAsync` |
| `GET /api/rlink/settings/{lineId}` | `GetSettingsAsync` |
| `GET /api/rlink/products` | `GetProductsAsync` |
| `POST /api/rlink/monitor` | `SendMonitorAsync` |
| `POST /api/rlink/log/print` | `SendLogPrintAsync` |
| `POST /api/rlink/log/camera` | `SendLogCameraAsync` |
| `POST /api/rlink/log/error-image` | `SendErrorImageAsync` |
| `POST /api/rlink/log/status` | `SendLogStatusAsync` |
| `POST /api/job/complete` | `CompleteJobAsync` |

---

## 7. Chuyển từ Simulator → R-Link Master thật

### 7.1 Cơ chế resolve URL hiện tại

Trong `RLinkMasterServiceFactory.ResolveUrl()`:

    string configured = Shared.Settings?.ApiUrl;
    bool isDefault = string.IsNullOrWhiteSpace(configured)
                  || configured.Contains("google.com");
    return isDefault ? DefaultSimulatorUrl : configured.TrimEnd('/');

**Quy tắc**:
- `Settings.ApiUrl` **rỗng** hoặc còn placeholder chứa `google.com` → dùng **Simulator** (`http://192.168.15.70:5130`).
- Ngược lại → dùng URL được cấu hình (R-Link Master thật).

### 7.2 Các bước chuyển từ Simulator → Server thật

**Bước 1 — Cấu hình URL trong Settings**

Mở UI Settings (`FrmSettingsTHTrueMilk`) hoặc sửa file cấu hình JSON, set:

    ApiUrl = http://<rlink-master-ip>:<port>

Ví dụ:

    ApiUrl = http://10.10.5.20:5130

Lưu ý:
- **Không** kết thúc bằng `/` (factory tự `TrimEnd('/')`).
- **Tránh** chứa chuỗi `google.com` (sẽ bị coi là placeholder và fallback về Simulator).

Hoặc set tạm trong code (debug):

    Shared.Settings.ApiUrl = "http://10.10.5.20:5130";
    SettingsController.Save();

**Bước 2 — Reset singleton để áp URL mới**

    RLinkMasterServiceFactory.Reset();

`Reset()` sẽ:
- Tạo lại `RLinkMasterService` mới với URL mới.
- Bảo toàn `_accessToken` + `_refreshToken` cũ → tránh phải login lại ngay.
- Lần gọi API kế tiếp nếu token cũ không hợp lệ trên server thật → tự động `TryRestoreAuthAsync` (refresh hoặc re-login với `_storedUsername/Password`).

**Bước 3 — Re-login (khuyến nghị)**

Vì JWT secret giữa Simulator và Master thật **khác nhau**, token cũ chắc chắn invalid. Gọi rõ:

    var rlink = RLinkMasterServiceFactory.Instance;
    var login = await rlink.LoginAsync(username, password);
    if (!login.IsSuccess)
        MessageBox.Show($"Login fail: {login.Message}");

**Bước 4 — Verify ping & lấy lines**

    bool ok = await RLinkMasterServiceFactory.Instance.PingAsync();
    // → true nếu /api/health trả 200
    var lines = await RLinkMasterServiceFactory.Instance.GetLinesAsync();

**Bước 5 — Gán line cho máy hiện tại**

    await RLinkMasterServiceFactory.Instance.SetLineStatusAsync(
        lineId:      "LINE_01",
        machineIp:   GetLocalIp(),
        factoryCode: "FACT_HN");

### 7.3 Checklist khi đi production

| Mục | Sim | Real |
|---|---|---|
| `Settings.ApiUrl` | rỗng / placeholder | URL R-Link Master thật |
| Network | LAN với máy chạy Simulator (port 5130) | VPN/LAN tới server hạ tầng |
| Tài khoản | `admin/admin` (sim) hoặc theo `AccountStore` của sim | Tài khoản do IT cấp |
| JWT Secret | (sim) chỉ dùng nội bộ | Khác hoàn toàn — phải re-login |
| `DefaultSimulatorUrl` trong code | giữ nguyên (fallback) | giữ nguyên (fallback) |
| `Settings.RLinkName`, `factory_code`, `line_id` | mock | đúng theo Master Data |
| Firewall | mở 5130 vào máy sim | mở port server thật |

### 7.4 Switch nhanh trong runtime (không restart app)

Helper:

    public static async Task<bool> SwitchRLinkUrlAsync(string newUrl,
                                                       string username,
                                                       string password)
    {
        Shared.Settings.ApiUrl = newUrl;
        SettingsController.Save();

        RLinkMasterServiceFactory.Reset();

        var rlink = RLinkMasterServiceFactory.Instance;
        if (!await rlink.PingAsync())
            return false;

        var login = await rlink.LoginAsync(username, password);
        return login.IsSuccess;
    }

Gọi trong UI khi đổi mode:

    // Switch sang Master thật
    await SwitchRLinkUrlAsync("http://10.10.5.20:5130", "operator01", "********");

    // Hoặc quay về Simulator
    await SwitchRLinkUrlAsync(RLinkMasterServiceFactory.DefaultSimulatorUrl, "admin", "admin");

### 7.5 Troubleshooting

| Triệu chứng | Nguyên nhân thường gặp | Cách xử lý |
|---|---|---|
| `Ping` luôn FALSE | Sai port / firewall chặn / máy chủ chưa start | Test bằng `curl http://<ip>:<port>/api/health`. |
| Vẫn gọi vào Simulator dù đã đổi | Quên `Reset()` sau khi đổi `ApiUrl` | Gọi `RLinkMasterServiceFactory.Reset()` ngay sau khi save settings. |
| 401 lặp vô tận | `_storedUsername/Password` chưa set + refresh token sai | Gọi `LoginAsync` thủ công lại, đảm bảo lưu credentials. |
| `ApiUrl` luôn fallback Sim | Chứa chuỗi `google.com` | Bỏ placeholder mặc định, dùng URL thật. |
| Token cũ không invalidate | JWT 2 server giống nhau (cùng secret) | Force `LoginAsync` sau `Reset()`. |
| Log không xuất hiện | `ProjectLogger` chưa init | Kiểm tra `Utils.ProjectLogger` được gọi `Initialize()` lúc app start. |
| `CompleteJobAsync` trả `AllocatedQrCodes` rỗng | Server hết QR / line chưa active | Dùng `SetLineStatusAsync` trước; check `qrbank/status` trên server. |

---

## Ghi chú maintain

- Khi server thêm endpoint mới → bổ sung method vào `IRLinkMasterService` trước, sau đó implement trong `RLinkMasterService`, cuối cùng cập nhật bảng mapping ở mục 6.
- Khi đổi shape DTO (rename field) → cần đồng bộ cả 2 phía (`BarcodeVerificationSystem\Services\THTrueMilk\RLinkMaster\Models\*` và `RLinkMasterSimulator\Models\*`).
- `MonitorPayload` của THTrueMilk **khác** `MonitorPayload` của Nutifood — đừng merge nhầm namespace.
- Tất cả method `Send*` đều "fail-silent" (return `false`, log warning) — không throw để không làm gián đoạn vận hành runtime; tuy nhiên `Login/Refresh/CompleteJob` thì trả về object có `IsSuccess` để caller xử lý logic.