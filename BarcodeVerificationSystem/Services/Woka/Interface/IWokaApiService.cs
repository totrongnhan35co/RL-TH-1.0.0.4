using BarcodeVerificationSystem.Model.Woka.Request;
using BarcodeVerificationSystem.Model.Woka.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.Woka.Interface
{
    public interface IWokaApiService
    {
        Task<ResponseCancelList> PostCancelListAsync(RequestCancelList responseCancelList);
        Task<ResponseCargo> PostCargoAsync(RequestCargo requestCargo);
    }
}
