using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Apis.Woka
{
    public class ApiModelWoka
    {
        static readonly string url = Shared.Settings.ApiUrl;

        private static string _cargorUrl = $"{url}/cargos";
        private static string _cancelListUrl = $"{url}/packaging-sessions/cancel-list";

        public static string getCargorUrl()
        {
            return _cargorUrl;
        }

        public static string getCancelListUrl()
        {
            return _cancelListUrl;
        }
    }
}
