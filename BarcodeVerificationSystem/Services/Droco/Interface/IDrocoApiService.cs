using BarcodeVerificationSystem.Model.Droco.Request;
using BarcodeVerificationSystem.Model.Droco.Response;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.Droco.Interface
{
    public interface IDrocoApiService
    {
        Task<ResponseCancelList> PostCancelListAsync(RequestCancelList responseCancelList);
        Task<ResponseCargo> PostCargoAsync(RequestCargo requestCargo);
    }
}