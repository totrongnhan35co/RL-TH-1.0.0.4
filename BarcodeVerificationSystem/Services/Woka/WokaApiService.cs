using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.Apis.Woka;
using BarcodeVerificationSystem.Model.Woka.Request;
using BarcodeVerificationSystem.Model.Woka.Response;
using BarcodeVerificationSystem.Services.Woka.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.Woka
{
    public class WokaApiService : IWokaApiService
    {
        private ApiService _apiService = new ApiService();

        public Task<ResponseCancelList> PostCancelListAsync(RequestCancelList responseCancelList)
        {
            return _apiService.PostApiDataAsync<ResponseCancelList>(ApiModelWoka.getCancelListUrl(), responseCancelList); // $"http://127.0.0.1:5555/packaging-sessions/cancel-list" 
        }

        public Task<ResponseCargo> PostCargoAsync(RequestCargo requestCargo)
        {
            return _apiService.PostApiDataAsync<ResponseCargo>(ApiModelWoka.getCargorUrl(), requestCargo); //  ApiModelWoka.getCargorUrl() //$"http://127.0.0.1:5555/cargos" 
        }
    }
}
