using System.Collections.Generic;
using System.Threading.Tasks;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
{
    public interface IRLinkLogService
    {
        Task SaveLogInAsync(
            string status, string rlinkStatus, string operatorUser, int qty,
            string lineId, string rlinkName, string jobName, string batch,
            string productId, string productName, string qrCode = "", List<QrDetailItem> qrDetail = null, int qrUsed = 0,
            string manufacturedDate = "", string expiryDate = "", string lastPrintedAt = "",
            string printerLastProductManufacturedDate = "");

        Task SaveLogCameraAsync(
            string status, string rlinkStatus, string operatorUser, int cameraOk, int cameraFail,
            string lineId, string rlinkName, string jobName, string batch,
            string productId, string productName, string qrCode = "", List<QrDetailItem> qrDetail = null,
            string frameInfo = "", string cameraManufacturedDate = "", string cameraExpiryDate = "",
            string cameraLastPacketReceivedAt = "", string cameraLastProductManufacturedDate = "");

        Task SaveLogCameraErrorAsync(
            string rlinkStatus, string operatorUser, string qrCode, string resultType,
            string lineId, string rlinkName, string jobName, string batch,
            string productId, string productName,
            string imagePath = "", string errorNsx = "", string errorHsd = "", string errorFrameInfo = "");

        /// <summary>Lưu QR codes được cấp phát (allocated) vào rlink_allocated_qr + bảng code.</summary>
        Task SaveAllocatedQrCodesAsync(
            List<string> codes, string jobName,
            string lineId, string batch);

        /// <summary>Lưu QR codes nhận được khi complete job vào bảng code (cùng bảng với allocated).</summary>
        Task SaveCompletedQrCodesAsync(
            List<string> codes, string lineId, string batch);
    }
}
