using BarcodeVerificationSystem.Model.Apis.THTrueMilk.Dispatching;
using BarcodeVerificationSystem.Model.Payload;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.DispatchingPayload.Response;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services.Interface;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Dispatching
{
    public class DispatchingService : BarcodeVerificationSystem.Services.THTrueMilk.Interface.IDispatchingService
    {
        private readonly IApiService _apiService;
        public DispatchingService()
        {
            _apiService = new ApiService();
        }

        public async Task<ResponseListRePrint> GetListReprintDataAsync()
        {
            return await _apiService.GetApiWithModelAsync<ResponseListRePrint>(DispatchingApis.GetReprintCodesUrl());
        }

        public async Task<ResponseOrder> GetOrderInfoAsync(string orderID)
        {
            return await _apiService.GetApiWithModelAsync<ResponseOrder>(DispatchingApis.GetOrderInfoUrl(orderID));
        }

        public async Task<ResponseCurrentPrintedCodeInfo> GetPrintedAmountDataAsync(RequestCheckCodeAmount payload)
        {
            return await _apiService.PostApiDataAsync<ResponseCurrentPrintedCodeInfo>(DispatchingApis.GetCurrentPrintedCodeInfoUrl(), payload);
        }

        public async Task<ResponseDisposal> PostDestroyDataAsync(RequestListDisposal payload)
        {
            return await _apiService.PostApiDataAsync<ResponseDisposal>(DispatchingApis.GetDestroyCodesUrl(), payload);
        }

        public async Task<bool> PostMonitorDataAsync(MonitorPayload payload)
        {
            return await _apiService.PostApiDataAsync(DispatchingApis.GetMonitorUrl(), payload);
        }

        public async Task<ResponsePrinted> PostPrintedDataAsync(RequestPrinted payload)
        {
            return await _apiService.PostApiDataAsync<ResponsePrinted>(DispatchingApis.GetPrintedDataUrl(), payload);
        }
    }
}