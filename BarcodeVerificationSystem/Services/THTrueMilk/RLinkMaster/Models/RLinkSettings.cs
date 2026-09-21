using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public class RLinkSettings
    {
        public int OperatingMode { get; set; }
        public int DeltaMinutes { get; set; }
        public int NMinutes { get; set; }
        public int BufferCount { get; set; }
        public int MonitoringIntervalMinutes { get; set; }
        public int LogIntervalMinutes { get; set; }
        public int QrThreshold { get; set; }
        public string ErrorImageFolder { get; set; }
        public int MaxConsecutiveDefects { get; set; } = 5;
        public int RetentionDays { get; set; } = 180;
        /// <summary>Hệ số dự phòng từ R-Link Master — dùng khi tính số QR cần tạo job.</summary>
        [JsonProperty("reserve_factor")]
        public double ReserveFactor { get; set; } = 1.5;
    }
}