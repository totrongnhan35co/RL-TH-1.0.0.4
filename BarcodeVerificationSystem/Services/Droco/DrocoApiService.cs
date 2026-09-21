using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.Apis.Droco;
using BarcodeVerificationSystem.Model.Droco.Request;
using BarcodeVerificationSystem.Model.Droco.Response;
using BarcodeVerificationSystem.Services.Droco.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.Droco
{
    public class DrocoApiService : IDrocoApiService
    {
        private ApiService _apiService = new ApiService();

        public Task<ResponseCancelList> PostCancelListAsync(RequestCancelList responseCancelList)
        {
            return _apiService.PostApiDataAsync<ResponseCancelList>(ApiModelDroco.getCancelListUrl(), responseCancelList);
        }

        public Task<ResponseCargo> PostCargoAsync(RequestCargo requestCargo)
        {
            return _apiService.PostApiDataAsync<ResponseCargo>(ApiModelDroco.getCargorUrl(), requestCargo);
        }
    }
}