using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.Interface
{
    public interface IApiService
    {
        Task<T> GetApiWithModelAsync<T>(string url);
        Task<T> PostApiDataAsync<T>(string apiUrl, object payload);
        Task<T> PostCompressedDataAsync<T>(string apiUrl, object payload);

        Task<bool> PostApiDataAsync(string apiUrl, object payload);
        Task<(bool Success, T Data)> GetApiDataAsync<T>(string apiUrl);

    }
}
