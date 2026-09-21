namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public static class ApiEndpoints
    {
        // ── Auth ──────────────────────────────────────────────────
        public const string AuthLogin        = "/api/rlink/auth/login";
        public const string AuthRefreshToken = "/api/rlink/auth/refresh-token";
        public const string AuthAccounts     = "/api/rlink/accounts/device";

        // ── Line / Settings ───────────────────────────────────────
        public const string LinesList        = "/api/rlink/lines";
        public const string LinesStatus      = "/api/rlink/lines/status";
        public const string LinesDeactivate  = "/api/rlink/lines/deactivate";
        public const string Settings         = "/api/rlink/settings"; // + /{lineId}
        public const string Products         = "/api/rlink/products";

        // ── Monitor ────────────────────────────────────────────────
        public const string Monitor          = "/api/rlink/monitor";
        public const string Health           = "/api/rlink/health";
        public const string Heartbeat        = " ";

        // ── Logs ───────────────────────────────────────────────────
        public const string LogSyncReport    = "/api/rlink/log/sync-report";
        public const string LogMarkUsed      = "/api/rlink/log/mark-used";
        public const string LogMarkUnused    = "/api/rlink/log/mark-unused";
        public const string LogResyncRequest = "/api/rlink/log/resync-request"; // + /{lineId}
        public const string LogCamera        = "/api/rlink/log/camera";
        public const string LogCameraError   = "/api/rlink/log/camera/error";
        public const string LogErrorImage    = "/api/rlink/log/error-image";
        public const string LogStatus        = "/api/rlink/log/status";
        public const string LogComplete      = "/api/rlink/log/complete";

        // ── QR Config ──────────────────────────────────────────────
        public const string QrConfig         = "/api/rlink/qr-config";
    }
}
