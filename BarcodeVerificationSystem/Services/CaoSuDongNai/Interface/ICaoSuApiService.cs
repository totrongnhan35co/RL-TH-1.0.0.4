using System.Threading.Tasks;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Request;
using BarcodeVerificationSystem.Model.CaoSuDongNai.Response;

namespace BarcodeVerificationSystem.Services.CaoSuDongNai.Interface
{
    public interface ICaoSuApiService
    {
        Task<ResponseLogin> PostLoginAsync(RequestLogin payload);
        Task<ResponsePushDatabase> PostPushDatabaseAsync(RequestPushDatabase payload);
        Task<ResponseRefreshToken> PostRefreshTokenAsync();
        Task<ResponseBasic> PostUpdatePalletAsync(RequestUpdatePallet payload);
        Task<ResponseUpdateCodeCheck> PostCodeCheckAsync(RequestUpdateCodeCheck payload);
        Task<ResponseBasic> PostCodePrintAsync(RequestUpdateCodePrint payload);
        Task<ResponseAllProducts> GetAllProductsAsync();
    }
}
