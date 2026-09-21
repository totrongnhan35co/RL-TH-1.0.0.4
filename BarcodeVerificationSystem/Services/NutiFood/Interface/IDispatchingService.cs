using BarcodeVerificationSystem.Model.Payload;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.Interface
{
    public interface IDispatchingService
    {
        Task<ResponseOrder> GetOrderInfoAsync(string orderID);
        Task<ResponseListRePrint> GetListReprintDataAsync();
        Task<ResponseCurrentPrintedCodeInfo> GetPrintedAmountDataAsync(RequestCheckCodeAmount payload);

        Task<ResponsePrinted> PostPrintedDataAsync(RequestPrinted payload);
        Task<bool> PostMonitorDataAsync(MonitorPayload payload);
        Task<ResponseDisposal> PostDestroyDataAsync(RequestListDisposal payload);

    }
}
