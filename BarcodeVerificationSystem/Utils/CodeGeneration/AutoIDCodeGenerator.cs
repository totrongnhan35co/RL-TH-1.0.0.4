using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model;
using BarcodeVerificationSystem.Model.CodeGeneration;
using CommonVariable;
using GenCode.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils.CodeGeneration.Helper
{
    internal class AutoIDCodeGenerator
    {
        static Dictionary<string, int> autoIdPerMonth = new Dictionary<string, int>();
        private static string AutoIdStatePath = CommVariables.PathSettingsApp + "autoid_state.json";
        //string filePath =  + _JobModel.FileName + Shared.Settings.JobFileExtension;

        public static void SaveAutoIdState()
        {
            File.WriteAllText(AutoIdStatePath, JsonConvert.SerializeObject(autoIdPerMonth));
        }

        public static void LoadAutoIdState()
        {
            if (File.Exists(AutoIdStatePath))
                autoIdPerMonth = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(AutoIdStatePath));
            else
                autoIdPerMonth = new Dictionary<string, int>();
        }

        /// <summary>
        /// Auto gen code tracking by autoid , with line Number (1-n)
        /// Test :  1000000 / 283 ms
        /// </summary>
        /// <param name="lineNo"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static List<string> GenerateCodesWithAutoID(int quantity, string shipto, string shipment, string shiptoName)
        {
         
           LoadAutoIdState(); // Load old key when start new sesion gen
            string f = RegistryHelper.ReadValue("AutoIdStatePath") ?? "0";
            int CurrentValue = int.Parse(f);

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be > 0");

            int? surplusPercentage = Shared.Settings.AddQuantity;
            quantity = (int)((quantity * surplusPercentage) / 100) + quantity;

            int lineNo = Shared.Settings.LineIndex;

            // 1. Get the current + month code
            string currentYear = DateCodeHelper.GetCurrentYearTwoDigits();
            string currentMonth = DateCodeHelper.GetCurrentMonthTwoDigits();
            char yearCode = YearCodeHelper.GetYearCode(currentYear);
            char monthCode = MonthCodeHelper.GetMonthCode(currentMonth);
            char lineCode = LineConvertHelper.GetLineCode(lineNo); // 

            // 2. The only lock over time and line
            string key = $"{yearCode}-{monthCode}-{lineCode}";

            // 3. Get the current Autoid
            //if (!autoIdPerMonth.ContainsKey(key))
            //    autoIdPerMonth[key] = 0;
            if (!autoIdPerMonth.ContainsKey(key))
            {
                autoIdPerMonth[key] = 0;
                CurrentValue = 0;
                RegistryHelper.WriteValue("AutoIdStatePath", "0");
            }

            int currentAutoId = CurrentValue + 1; //  autoIdPerMonth[key]
            Shared.FirstGeneratedCodeIndex = currentAutoId; // Save the first code index for this session

            // 4.Check the limit
            if (currentAutoId + quantity > 10_000_000)
                throw new Exception("Exceeding the limit of 10 million codes in the month for this line");

            var dispatching = new Dispatching(shipto, shipment, shiptoName);
            // 5. Born a code list
            List<string> codes = new List<string>();
            for (int i = 0; i < quantity; i++)
            {
                int id = currentAutoId + i;
                string autoIdStr = id.ToString("D7");
                string code = $"{yearCode}{monthCode}{autoIdStr}{lineCode}";
                string fullCode = dispatching.GenerateCode(code) + "," + code + "," + dispatching.getShiptoNameCode() +
                    "," + dispatching.getShipmentCode() + "," + dispatching.getShiptoCode();
                codes.Add(fullCode);
            }

            // 6. Update Autoid after birth
            autoIdPerMonth[key] = currentAutoId + quantity;
            Shared.LastGeneratedCodeIndex = autoIdPerMonth[key] - 1; // Save the last code index for this session
            RegistryHelper.WriteValue("AutoIdStatePath", autoIdPerMonth[key].ToString());

            SaveAutoIdState(); // Save latest key

            return codes;
        }
    }
}
