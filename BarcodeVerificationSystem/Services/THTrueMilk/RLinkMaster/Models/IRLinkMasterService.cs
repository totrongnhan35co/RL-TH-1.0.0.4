using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public interface IRLinkMasterService
    {
        // ── Kết nối ─────────────────────────────────────────────────
        /// <summary>GET /api/rlink/health — kiểm tra kết nối, không cần auth.</summary>
        Task<bool> PingAsync();
        /// <summary>GET /api/rlink/heartbeat?lineId=...&factoryCode=... — heartbeat nhẹ kèm thông tin line.</summary>
        Task<bool> SendHeartbeatAsync(string lineId, string factoryCode);
        /// <summary>True nếu đã đăng nhập thành công và có access token.</summary>
        bool IsAuthenticated { get; }
        /// <summary>Trạng thái kết nối đến R-Link Master (cập nhật sau mỗi lần gửi monitor).</summary>
        bool IsMasterConnected { get; }
        // ── Auth ─────────────────────────────────────────────────────
        /// <summary>POST /api/auth/login</summary>
        Task<LoginResult> LoginAsync(string username, string password);

        /// <summary>POST /api/auth/refresh-token</summary>
        Task<LoginResult> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Lưu credentials vào instance để TryRestoreAuthAsync có thể dùng khi cần re-login.
        /// Gọi ngay sau khi local-login thành công (offline mode).
        /// </summary>
        void StoreCredentials(string username, string password);

        /// <summary>GET /api/auth/accounts — lấy danh sách tài khoản.</summary>
        Task<List<AccountInfo>> GetAccountsAsync();

        // ── Lines ────────────────────────────────────────────────────
        /// <summary>GET /api/rlink/lines — lấy danh sách line, tùy chọn lọc theo nhà máy.</summary>
        Task<List<LineInfo>> GetLinesAsync(string factoryCode = null);

        /// <summary>POST /api/rlink/lines/status — chọn line_id, factory_code và đăng ký IP máy client.</summary>
        Task<bool> SetLineStatusAsync(string lineId, string machineIp = "", string factoryCode = "");

        /// <summary>POST /api/rlink/lines/deactivate — hủy gán line đang active.</summary>
        Task<bool> UnassignLineAsync(string lineId, string factoryCode = "");

        // ── Settings & Products ──────────────────────────────────────
        /// <summary>GET /api/rlink/settings/{line_id} — lấy toàn bộ tham số vận hành.</summary>
        Task<RLinkSettings> GetSettingsAsync(string lineId);

        /// <summary>GET /api/rlink/products — lấy danh mục sản phẩm.</summary>
        Task<List<ProductItem>> GetProductsAsync();

        // ── Monitor ──────────────────────────────────────────────────
        /// <summary>POST /api/rlink/monitor — gửi trạng thái R-Link.</summary>
        Task<bool> SendMonitorAsync(MonitorPayload payload);

        /// <summary>POST /api/rlink/log/camera — gửi log camera (Start/Run/Stop).</summary>
        Task<bool> SendLogCameraAsync(LogCameraPayload payload);

        /// <summary>POST /api/rlink/log/camera/error — gửi chi tiết lỗi camera từng mã.</summary>
        Task<bool> SendLogCameraErrorAsync(LogCameraErrorPayload payload);

        /// <summary>POST /api/rlink/log/status — gửi log trạng thái máy in 4 pha (start/running/stop/completed).</summary>
        Task<bool> SendLogStatusAsync(LogStatusPayload payload);

        // ── Job ──────────────────────────────────────────────────────
        /// <summary>Xác nhận hoàn thành job (endpoint TBD).</summary>
        Task<CompleteJobResult> CompleteJobAsync(CompleteJobRequest request);

        Task<bool> PostSyncReportAsync(SyncReportPayload report);
        Task<bool> CheckResyncRequestAsync(string lineId);

        // ── QR Bank ──────────────────────────────────────────────────
        /// <summary>
        /// POST /api/sim/qrbank/mark-used — thông báo R-Link Master danh sách QR đã
        /// được sử dụng (khi R-Link tạo job). Fire-and-forget, lỗi chỉ log warning.
        /// </summary>
        Task<bool> MarkQrUsedAsync(
            List<string> qrCodes,
            string jobName,
            string lineId,
            string lineName,
            string factoryCode,
            string batch,
            string productId,
            string prod = "",
            string exp = "",
            string printedAt = "");

        /// <summary>POST /api/rlink/mark-unused — báo R-Link Master QR đã được giải phóng (job complete 0 sp).</summary>
        Task<bool> MarkQrUnusedAsync(
            List<string> qrCodes,
            string jobName);

        // ── QR Config ─────────────────────────────────────────────
        /// <summary>GET /api/rlink/qr-config — lấy cấu hình QR (base_url, number_of_url).</summary>
        Task<QrConfig> GetQrConfigAsync();
    }
}