using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Apis
{
    internal class ApiModel
    {
        static string url = Shared.Settings.ApiUrl;
        private static string _loginUrl = $"{url}/api/3C7937B217D14EBC803206646A17D896"; // Okay
        private static string _permissionUrl = $"{url}/api/D5A06FABB7774BF3B911B15E8CD104B4"; // Okay
        private static string _lineDispatchingName = $"{url}/api/shipment/get_line_settings"; // Okay
        private static string _lineManufacturingName = $"{url}/api/production/get_line_settings"; // Okay

        public static string getLoginUrl(string username, string password)
        {
            return _loginUrl + $"/{username}/{password}";
        }

        public static string getPermissionUrl(string maQuyen)
        {
            return _permissionUrl + $"/{maQuyen}";
        }

        public static string getDispatchingSettingsUrl(string RlinkName)
        {
            return _lineDispatchingName + $"/{RlinkName}";
        }

        public static string getManufacturingSettingsUrl(string RlinkName)
        {
            return _lineManufacturingName + $"/{RlinkName}";
        }

    }
}
