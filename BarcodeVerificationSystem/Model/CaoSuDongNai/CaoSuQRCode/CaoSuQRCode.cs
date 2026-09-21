using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.CaoSuDongNai.CaoSuQRCode
{
    internal class CaoSuQRCode
    {
        private static string _url = Shared.Settings.DomainQRCode; 
        private static string _factoryId = "2"; 
        private static string _yearAndMonth = DateTime.Now.ToString("yyMM");
        public CaoSuQRCode(string url, string factoryId, string yearAndMonth)
        {
            _url = url;
            _factoryId = factoryId;
            _yearAndMonth = yearAndMonth;
        }
        public static string GenerateCode(string _randomCode) // Total: 48 Characters
        {
            return $"{_url}/{_randomCode}"; 
        }

        public static string GetUniqueCode(string _randomCode)
        {
            return $"{_randomCode}";
        }
    }
}
