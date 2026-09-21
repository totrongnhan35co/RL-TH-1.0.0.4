using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Request;
using BarcodeVerificationSystem.Model.Payload.ManufacturingPayload.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.Interface
{
    public interface IManufacturingService
    {
        Task<ResponseProcessOrder> GetProcessOrderAsync(string resourceCode, string plant, string device_name);
        Task<ResponseReservation> GetReservationAsync(string plant, string material_doc, string device_name);
        Task<ResponseBatchInfo> GetBatchInfoAsync(string plant, string material_doc);

        Task<ResponsePrinted> PostPrintedDataAsync(RequestPrinted payload);
        Task<ResponseChecked> PostVerifiedDataAsync(RequestChecked payload);
        Task<ResponseGeneratedCodes> PostGeneratedCodesAsync(RequestGeneratedCodes payload);
        Task<ResponseDestroyCodes> PostDestroyCodesAsync(RequestDestroyCodes payload);
        Task<ResponsePrinted> PostReservationAsync(RequestPrinted payload);
        Task<ResponsePrintedAmount> PostPrintedAmountAsync(RequestPrintedAmount payload);
    }
}
