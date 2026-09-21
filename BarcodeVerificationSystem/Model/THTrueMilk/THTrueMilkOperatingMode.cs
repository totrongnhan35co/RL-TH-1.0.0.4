using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.THTrueMilk
{
    /// <summary>Chế độ vận hành của hệ thống THTrueMilk Manufacturing.</summary>
    public enum THTrueMilkOperatingMode
    {
        /// <summary>Chế độ 1: 1 Batch in 1 QR Code</summary>
        BatchOneQrCode = 1,

        /// <summary>Chế độ 2: Tự động làm mới theo chu kỳ thời gian</summary>
        AutoRefreshByTime = 2,

        /// <summary>Chế độ 3: 1 sản phẩm in 1 QR Code</summary>
        ProductOneQrCode = 3,

        /// <summary>Chế độ 4: 1 Batch in 1 QR Code — không đổi QR</summary>
        BatchOneQrCodeNoChange = 4
    }

    public static class THTrueMilkOperatingModeExtensions
    {
        public static string ToDisplayString(this THTrueMilkOperatingMode mode)
        {
            switch (mode)
            {
                case THTrueMilkOperatingMode.BatchOneQrCode:
                    return "1 batch 1 qr code";
                case THTrueMilkOperatingMode.AutoRefreshByTime:
                    return "1 batch nhiều qr code";
                case THTrueMilkOperatingMode.ProductOneQrCode:
                    return "1 sản phẩm 1 qr code";
                case THTrueMilkOperatingMode.BatchOneQrCodeNoChange:
                    return "1 batch 1 qr code (không đổi)";
                default:
                    return mode.ToString();
            }
        }
    }
}
