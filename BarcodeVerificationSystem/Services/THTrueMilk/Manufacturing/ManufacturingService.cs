using BarcodeVerificationSystem.Model.Apis.THTrueMilk.Manufacturing;
using BarcodeVerificationSystem.Model.Payload;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using BarcodeVerificationSystem.Services;
using BarcodeVerificationSystem.Services.Interface;
using BarcodeVerificationSystem.Services.THTrueMilk.Interface;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing
{
    public class ManufacturingService : BarcodeVerificationSystem.Services.THTrueMilk.Interface.IManufacturingService
    {
        private readonly IApiService _apiService;
        public ManufacturingService()
        {
            _apiService = new ApiService();
        }

        public async Task<bool> PostMonitorDataAsync(MonitorPayload payload)
        {
            return await _apiService.PostApiDataAsync(ManufacturingApis.GetMonitorUrl(), payload);
        }

        public async Task<ResponseProcessOrder> GetProcessOrderAsync(string resourceCode, string plant, string device_name)
        {
            return await _apiService.GetApiWithModelAsync<ResponseProcessOrder>(ManufacturingApis.getProcessOrderInfoUrl(resourceCode, plant, device_name));
        }

        public async Task<ResponseReservation> GetReservationAsync(string plant, string material_doc, string device_name)
        {
            return await _apiService.GetApiWithModelAsync<ResponseReservation>(ManufacturingApis.getReservationUrl(plant, material_doc, device_name));
        }

        public async Task<ResponseBatchInfo> GetBatchInfoAsync(string plant, string process_order)
        {
            return await _apiService.GetApiWithModelAsync<ResponseBatchInfo>(ManufacturingApis.getBatchInfoUrl(plant, process_order));
        }

        public async Task<ResponseDestroyCodes> PostDestroyCodesAsync(RequestDestroyCodes payload)
        {
            return await _apiService.PostApiDataAsync<ResponseDestroyCodes>(ManufacturingApis.postDestroyCodesUrl(), payload);
        }

        public async Task<ResponseGeneratedCodes> PostGeneratedCodesAsync(RequestGeneratedCodes payload)
        {
            return await _apiService.PostCompressedDataAsync<ResponseGeneratedCodes>(ManufacturingApis.postGeneratedCodesUrl(), payload);
        }

        public async Task<ResponsePrintedAmount> PostPrintedAmountAsync(RequestPrintedAmount payload)
        {
            return await _apiService.PostApiDataAsync<ResponsePrintedAmount>(ManufacturingApis.postPrintedDataUrl(), payload);
        }

        public Task<ResponsePrinted> PostPrintedDataAsync(RequestPrinted payload)
        {
            return _apiService.PostApiDataAsync<ResponsePrinted>(ManufacturingApis.postPrintedDataUrl(), payload);
        }

        public async Task<ResponsePrinted> PostReservationAsync(RequestPrinted payload)
        {
            return await _apiService.PostApiDataAsync<ResponsePrinted>(ManufacturingApis.postReservationUrl(), payload);
        }

        //public Task<ResponseChecked> PostVerifiedDataAsync(RequestChecked payload)
        //{
        //    return _apiService.PostApiDataAsync<ResponseChecked>(ManufacturingApis.postCheckedDataUrl(), payload);
        //}
    }
}