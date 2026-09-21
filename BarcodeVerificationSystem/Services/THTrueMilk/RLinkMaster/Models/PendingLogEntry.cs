using System;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    /// <summary>
    /// Một bản ghi log chờ gửi, lưu trong SQLite pending_logs.
    /// </summary>
    public class PendingLogEntry
    {
        public int Id { get; set; }
        /// <summary>"status" | "camera" | "print" | "error_image"</summary>
        public string LogType { get; set; }
        public string PayloadJson { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}