using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models;
using BarcodeVerificationSystem.Services;
using System;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster
{
    public static class RLinkMasterServiceFactory
    {
        /// <summary>URL mặc định trỏ đến R-Link Master Simulator.</summary>
        public const string DefaultSimulatorUrl = "http://192.168.15.70:5130";

        private static IRLinkMasterService _instance;

        public static IRLinkMasterService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Create();
                return _instance;
            }
        }

        private static IRLinkMasterService Create()
        {
            string url = ResolveUrl();
            Console.WriteLine($"[RLinkMasterServiceFactory] → {url}");
            var service = new RLinkMasterService(url);

            ApiService.RefreshTokenAsync = async () =>
            {
                var rlink = service as RLinkMasterService;
                if (rlink == null || string.IsNullOrEmpty(rlink.RefreshTokenValue))
                    return false;
                var result = await rlink.RefreshTokenAsync(rlink.RefreshTokenValue);
                return result.IsSuccess;
            };

            return service;
        }

        /// <summary>
        /// Ưu tiên: ApiUrl từ Settings → nếu chưa cấu hình thì dùng Simulator mặc định.
        /// </summary>
        private static string ResolveUrl()
        {
            var url = (Shared.Settings?.ApiUrl ?? string.Empty).TrimEnd('/');
            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine($"[RLinkMasterServiceFactory] ApiUrl chưa cấu hình → dùng mặc định: {DefaultSimulatorUrl}");
                url = DefaultSimulatorUrl;
            }
            return url;
        }

        /// <summary>
        /// Gọi khi đổi URL trong Settings để tạo lại instance mới.
        /// Token hiện tại được bảo toàn sang instance mới.
        /// </summary>
        public static void Reset()
        {
            var oldService = _instance as RLinkMasterService;
            _instance = null;

            if (oldService != null)
            {
                var newService = Create() as RLinkMasterService;
                if (newService != null)
                {
                    if (!string.IsNullOrEmpty(oldService.AccessToken))
                    {
                        newService.SetTokens(oldService.AccessToken, oldService.RefreshTokenValue);
                    }
                    if (!string.IsNullOrEmpty(oldService.StoredUsername)
                        && !string.IsNullOrEmpty(oldService.StoredPassword))
                    {
                        newService.StoreCredentials(oldService.StoredUsername, oldService.StoredPassword);
                    }
                }
                _instance = newService ?? Create();
            }
        }
    }
}