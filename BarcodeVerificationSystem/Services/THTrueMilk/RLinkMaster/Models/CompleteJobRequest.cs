using System;
using System.Collections.Generic;

    namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
    {
    public class CompleteJobRequest
    {
        public string JobName { get; set; }
        public string RLinkName { get; set; }
        public string LineId { get; set; }
        public int QrUsedCount { get; set; }

        public int QrThreshold { get; set; }
        public int QrRemainingInDb { get; set; }
        public int ProducedCount { get; set; }
        public string OperatorUser { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalPrint { get; set; }
        public int StatusGood { get; set; }
        public int StatusFail { get; set; }
        public int TotalCheck { get; set; }
        public int Product_A { get; set; }
        public int Product_B { get; set; }
        public int Product_F { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>Thống kê chi tiết theo QR code.</summary>
        public List<QrStatsItem> StatsArr { get; set; } = new List<QrStatsItem>();
    }

    /// <summary>Thống kê kiểm tra cho 1 QR code.</summary>
    public class QrStatsItem
    {
        public string QrCode { get; set; }
        public QrStatsDetail Stats { get; set; } = new QrStatsDetail();
    }

    /// <summary>Chi tiết thống kê kiểm tra.</summary>
    public class QrStatsDetail
    {
        public int TotalCheck { get; set; }
        public int TotalQRCheckPassed { get; set; }
        public int TotalQRCheckFailed { get; set; }
        public int TotalDateCheckPassed { get; set; }
        public int TotalDateCheckFailed { get; set; }
        public int TotalA { get; set; }
        public int TotalB { get; set; }
        public int TotalF { get; set; }
    }
}
