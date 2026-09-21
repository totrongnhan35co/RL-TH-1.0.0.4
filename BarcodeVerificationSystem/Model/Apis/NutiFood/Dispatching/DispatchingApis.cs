using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.Apis.Dispatching
{
    internal class DispatchingApis
    {
        static readonly string url = Shared.Settings.ApiUrl;
        static readonly string orderId = Shared.Settings.OrderId;
        static readonly string factoryCode = Shared.Settings.FactoryCode;

        private static string _getOrderInfoUrl = $"{url}/api/shipment/get"; // Okay
        private static string _sendGeneratedCodesUrl = $"{url}/api/shipment/pushDatabase"; // Okay
        private static string _printedDataUrl = $"{url}/api/shipment/printed";// Okay  
        private static string _monitorUrl = $"{url}/api/shipment/monitoring";// Okay
        private static string _getCurrentPrintedCodeInfo = $"{url}/api/shipment/getCurrentProcessing";// Okay
        private static string _getReprintCodesUrl = $"{url}/api/production/reprint/get_info";// Okay
        private static string _postReprintCodesUrl = $"{url}/api/production/reprint";// Okay
        private static string _destroyCodesUrl = $"{url}/api/shipment/destroy";// Okay

        public static string GetOrderInfoUrl(string wmsNumber) // checked error
        {
            string t = _getOrderInfoUrl + $"/?plant={factoryCode}&wms_number={wmsNumber}&device_name={Shared.Settings.RLinkName}"; 
            return _getOrderInfoUrl + $"/?plant={factoryCode}&wms_number={wmsNumber}&device_name={Shared.Settings.RLinkName}";
        }

        public static string GetSendGeneratedCodesUrl() // checked error
        {
            return _sendGeneratedCodesUrl; 
        }

        public static string GetPrintedDataUrl()
        {
            return _printedDataUrl;
        }

        public static string GetMonitorUrl()
        {
            return _monitorUrl;
        }
        public static string GetDestroyCodesUrl() // checked error
        {
            return _destroyCodesUrl;
        }

        public static string GetReprintCodesUrl() // checked error
        {
            return _getReprintCodesUrl; 
        }

        public static string GetSendReprintCodesUrl()
        {
            return _postReprintCodesUrl;
        }

        public static string GetCurrentPrintedCodeInfoUrl() 
        {
            return _getCurrentPrintedCodeInfo;
        }
    }
}


//private static string _confirmCompletionUrl = $"{url}/api/shipment/confirmcomplete";

//public static string GetConfirmCompletionUrl()
//{
//    return _confirmCompletionUrl;
//}