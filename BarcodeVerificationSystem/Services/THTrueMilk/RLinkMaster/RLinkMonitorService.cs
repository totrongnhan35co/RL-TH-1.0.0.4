using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using BarcodeVerificationSystem.Utils;
using CommonVariable;
using System;
using System.IO;
using System.Threading;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
{
    /// <summary>
    /// Singleton: Gửi monitor data + check kết nối lên R-Link Master.
    /// - Timer 1: Gửi monitor data (interval từ setting)
    /// - Timer 2: Check kết nối mỗi 10s, cập nhật icon
    /// </summary>
    public class RLinkMonitorService
    {
        public static RLinkMonitorService Instance { get; private set; }
        private Timer _timer;
        private Timer _connectionCheckTimer;
        private Func<MonitorPayload> _snapshotFactory;
        private bool _loggedNotAuthenticated = false;
        private bool _lastConnectedState = false;
        private string _instanceName;

        /// <summary>Gọi khi trạng thái kết nối R-Link Master thay đổi (true = connected, false = disconnected).</summary>
        public Action<bool> OnConnectionChanged
        {
            get => _onConnectionChanged;
            set
            {
                _onConnectionChanged = value;
                if (value != null)
                {
                    try { value.Invoke(_lastConnectedState); }
                    catch { }
                }
            }
        }
        private Action<bool> _onConnectionChanged;

        private RLinkMonitorService(Func<MonitorPayload> snapshotFactory, string instanceName = "")
        {
            _snapshotFactory = snapshotFactory ?? throw new ArgumentNullException("snapshotFactory");
            _instanceName = instanceName ?? "";
        }

        /// <summary>Tạo hoặc cập nhật singleton instance. Trả về instance hiện tại.</summary>
        public static RLinkMonitorService Create(Func<MonitorPayload> snapshotFactory, string instanceName = "")
        {
            if (Instance == null)
            {
                Instance = new RLinkMonitorService(snapshotFactory, instanceName);
                ProjectLogger.WriteInfo($"[RLinkMonitor] Singleton created ({instanceName}).");
            }
            else
            {
                Instance.SetSnapshotFactory(snapshotFactory);
                Instance._instanceName = instanceName;
                ProjectLogger.WriteInfo($"[RLinkMonitor] Singleton updated → {instanceName}.");
            }
            return Instance;
        }

        /// <summary>Đổi snapshot factory — form nào active gọi hàm này.</summary>
        public void SetSnapshotFactory(Func<MonitorPayload> factory)
        {
            _snapshotFactory = factory ?? throw new ArgumentNullException("factory");
        }

        /// <summary>
        /// Bắt đầu gửi monitor + check kết nối định kỳ.
        /// - _timer: gửi monitor data (intervalSeconds từ setting)
        /// - _connectionCheckTimer: check kết nối mỗi 10s, cập nhật icon
        /// </summary>
        public void Start(int intervalSeconds = 10, int startDelaySeconds = 0)
        {
            int ms = Math.Max(5, intervalSeconds) * 1000;
            int delayMs = startDelaySeconds * 1000;

            if (_timer == null)
            {
                ProjectLogger.WriteInfo($"[RLinkMonitor] Bắt đầu gửi monitor định kỳ mỗi {intervalSeconds}s (delay {startDelaySeconds}s).");
                _timer = new Timer(_ => SendAsync(), null, delayMs, ms);
            }

            if (_connectionCheckTimer == null)
            {
                int checkMs = 10_000;
                Console.WriteLine($"[RLinkMonitor][{_instanceName}] Connection check timer created — fire sau 2s, mỗi 10s");
                _connectionCheckTimer = new Timer(_ => CheckConnectionAsync(), null, 2000, checkMs);
            }
        }

        /// <summary>Gửi ngay 1 snapshot không cần đợi timer.</summary>
        public void SendImmediate() => SendAsync();

        public void Stop()
        {
            if (_timer != null)
            {
                ProjectLogger.WriteInfo("[RLinkMonitor] Dừng gửi monitor.");
                _timer.Dispose();
                _timer = null;
            }
            if (_connectionCheckTimer != null)
            {
                _connectionCheckTimer.Dispose();
                _connectionCheckTimer = null;
            }
        }

        /// <summary>Gửi monitor data lên server (POST /api/rlink/monitor).</summary>
        private async void SendAsync()
        {
            try
            {
                var svc = RLinkMasterServiceFactory.Instance;
                if (svc == null) return;

                if (!svc.IsAuthenticated)
                {
                    _loggedNotAuthenticated = true;
                    return;
                }

                if (_loggedNotAuthenticated)
                {
                    _loggedNotAuthenticated = false;
                    ProjectLogger.WriteInfo("[RLinkMonitor] Đã đăng nhập — bắt đầu gửi monitor.");
                }

                MonitorPayload payload = _snapshotFactory.Invoke();
                if (payload == null) return;

                bool ok = await svc.SendMonitorAsync(payload).ConfigureAwait(false);
                if (!ok)
                    ProjectLogger.WriteWarning("[RLinkMonitor] Gửi monitor thất bại (server từ chối hoặc timeout).");
            }
            catch (Exception ex)
            {
                ProjectLogger.WriteError("[RLinkMonitor] SendAsync lỗi: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Check kết nối server mỗi 10s (GET /api/rlink/health).
        /// Cập nhật icon kết nối (green/red) trên form.
        /// </summary>
        private async void CheckConnectionAsync()
        {
            try
            {
                var svc = RLinkMasterServiceFactory.Instance;
                if (svc == null) return;

                bool ok = await svc.PingAsync().ConfigureAwait(false);

                if (ok)
                {
                    if (!_lastConnectedState)
                    {
                        _lastConnectedState = true;
                        OnConnectionChanged?.Invoke(true);
                    }
                }
                else
                {
                    if (_lastConnectedState)
                    {
                        _lastConnectedState = false;
                        OnConnectionChanged?.Invoke(false);
                    }
                }
            }
            catch
            {
                if (_lastConnectedState)
                {
                    _lastConnectedState = false;
                    OnConnectionChanged?.Invoke(false);
                }
            }
        }

    }
}
