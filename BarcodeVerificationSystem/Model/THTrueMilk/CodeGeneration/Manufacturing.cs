using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BarcodeVerificationSystem.Model.THTrueMilk.CodeGeneration
{
    public class Manufacturing
    {
        public static string Url = Shared.Settings.ApiDomain; // 24 characters
        private static string _factoryId = "2"; // 2	Bình Dương - 3	Gia Lai - 4	Hưng Yên
        private static string _yearAndMonth = DateTime.Now.ToString("yyMM");
        public Manufacturing(string url, string factoryId, string yearAndMonth)
        {
            Url = url;
            _factoryId = factoryId;
            _yearAndMonth = yearAndMonth;
        }
        public static string GenerateCode(string _randomCode)
        {
            if(Shared.Settings.FactoryCode == "1210") _factoryId = "2";
            else if (Shared.Settings.FactoryCode == "1260") _factoryId = "3";
            else if (Shared.Settings.FactoryCode == "1240") _factoryId = "4";
            else if (Shared.Settings.FactoryCode == "1212") _factoryId = "5";


            return $"{Shared.Settings.ApiDomain}{_factoryId}{_yearAndMonth}{_randomCode}"; // Tong ky tu la 41
        }

        public static string GetUniqueCode(string _randomCode)
        {
            if (Shared.Settings.FactoryCode == "1210") _factoryId = "2";
            else if (Shared.Settings.FactoryCode == "1260") _factoryId = "3";
            else if (Shared.Settings.FactoryCode == "1240") _factoryId = "4";
            else if (Shared.Settings.FactoryCode == "1212") _factoryId = "5";

            return $"{_factoryId}{_yearAndMonth}{_randomCode}"; // Tong ky tu la 41
        }


        public static string GetHumanReadableCode(string code)
        {
            string randomCode = string.Empty;
            if (string.IsNullOrWhiteSpace(code) || code.Length != 41)
                return "";

            string urlPart = code.Substring(0, 24);
            string factoryIdPart = code.Substring(24, 1);
            string yearMonthPart = code.Substring(25, 4);
            string randomCodePart = code.Substring(28, 12);

            // Validate each segment's length and basic format
            if (urlPart.Length == 24 &&
                factoryIdPart.Length == 1 &&
                yearMonthPart.Length == 4 &&
                randomCodePart.Length == 12)
            {
                randomCode = randomCodePart;
                return randomCode;
            }

            return "";
        }
        // to take the random code from the generated code
        public static bool TryParse(string code, out string randomCode)
        {
            randomCode = string.Empty;

            if (string.IsNullOrWhiteSpace(code) || code.Length != 27)
                return false;

            string urlPart = code.Substring(0, 10);
            string factoryIdPart = code.Substring(10, 1);
            string yearMonthPart = code.Substring(11, 4);
            string randomCodePart = code.Substring(15, 12);

            // Validate each segment's length and basic format
            if (urlPart.Length == 10 &&
                factoryIdPart.Length == 1 &&
                yearMonthPart.Length == 4 &&
                randomCodePart.Length == 12)
            {
                randomCode = randomCodePart;
                return true;
            }

            return false;
        }
    }
}
