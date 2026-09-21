using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Apis.CaoSuDongNai
{
    public class ApiModel
    {
        static string url = Shared.Settings.ApiUrl;

        // auth
        private static string _loginUrl = $"{url}/api/auth/login";
        private static string _refreshTokenUrl = $"{url}/api/auth/refresh";

        // get product
        private static string _allProductsUrl = $"{url}/api/check-sync/get-all-products";

        // sync and check
        private static string _pushDatabaseUrl = $"{url}/api/check-sync/production-batch";
        private static string _updatePalletUrl = $"{url}/api/check-sync/update-bale-to-pallet-code";
        private static string _updateCodeCheckUrl = $"{url}/api/check-sync/verify";
        private static string _updateCodePrintUrl = $"{url}/api/check-sync/update-status-to-printed";
        private static string _updatedSyncOffline = $"{url}/api/check-sync/offline";

        public static string getLoginUrl()
        {
            return _loginUrl;
        }
        public static string getRefreshTokenUrl()
        {
            return _refreshTokenUrl;
        }
        public static string getPushDatabaseUrl()
        {
            return _pushDatabaseUrl;
        }
        public static string getUpdatePalletUrl()
        {
            return _updatePalletUrl;
        }
        public static string getUpdateCodeCheckUrl()
        {
            return _updateCodeCheckUrl;
        }
        public static string getUpdateCodePrintUrl()
        {
            return _updateCodePrintUrl;
        }
        public static string getAllProductsUrl()
        {
            return _allProductsUrl;
        }
        public static string getUpdatedSyncOffline()
        {
            return _updatedSyncOffline;
        }

    }
}
