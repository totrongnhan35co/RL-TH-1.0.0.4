using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Droco
{
    public class DrocoQRFieldMapping
    {
        /// <summary>Bật chế độ sinh QR từ Excel (false = tự động như cũ)</summary>
        public bool UseExcelMode { get; set; } = false;

        // ── Index cột trong Excel (0-based, -1 = không dùng) ──────────────
        public int ProductColumnIndex { get; set; } = -1;
        public int BoxColumnIndex { get; set; } = -1;
        public int CartonColumnIndex { get; set; } = -1;
        public int PalletColumnIndex { get; set; } = -1;

        // ── Tên header đã lưu (để hiển thị lại khi mở Settings) ──────────
        public string ProductColumnName { get; set; } = "";
        public string BoxColumnName { get; set; } = "";
        public string CartonColumnName { get; set; } = "";
        public string PalletColumnName { get; set; } = "";
    }
}
