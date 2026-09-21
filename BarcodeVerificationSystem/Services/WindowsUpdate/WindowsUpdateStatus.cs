using System;

namespace BarcodeVerificationSystem.Services.WindowsUpdate
{
    /// <summary>
    /// Represents the status of Windows Update on the system
    /// </summary>
    public class WindowsUpdateStatus
    {
        public bool HasPendingUpdates { get; set; }
        public bool RequiresRestart { get; set; }
        public bool UpdateServiceRunning { get; set; }
        public bool CanAutomaticallyHandle { get; set; }
        public string StatusMessage { get; set; }
        public DateTime? LastCheckTime { get; set; }
        public int PendingUpdateCount { get; set; }
    }
}

