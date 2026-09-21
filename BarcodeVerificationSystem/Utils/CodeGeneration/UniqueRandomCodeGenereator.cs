using BarcodeVerificationSystem.Controller;
using GenCode.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils.CodeGeneration
{
    public class UniqueRandomCodeGenereator
    {
        private static string _characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private int _codeLength = 12;
        private int _lineSettings = Shared.Settings.LineIndex;

        public static List<string> GenerateUniqueRandomCode(int quantity)
        {
            var result = new HashSet<string>(); // HashSet ensures uniqueness
            Random random = new Random();

            // Predefined parts
            string currentYear = DateCodeHelper.GetCurrentYearTwoDigits();  // e.g., "25"
            string currentMonth = DateCodeHelper.GetCurrentMonthTwoDigits(); // e.g., "04"
            char monthVal = MonthCodeHelper.GetMonthCode(currentMonth);     // e.g., 'D'
            char yearVal = YearCodeHelper.GetYearCode(currentYear);         // e.g., 'F'
            //string dayHourMinuteSecondVal = DateCodeHelper.GetCurrentDayHourMinuteSecond(); // e.g., "09143025"
                                                                                       
            string dayHourMinuteSecondVal = DateCodeHelper.GetCurrentDayHourMinuteSecond().Substring(0, 8); //nhan.to ---> dayHourMinuteSecondVal --> "05143425" (8 ký tự: ngày, giờ, phút, giây)

            // Line index from settings (assume it's 1–9, or adjust padding if needed)
            string lineIndexStr = Shared.Settings.LineIndex.ToString("D2"); // or PadLeft(2,'0') if you want "01"

            while (result.Count < quantity)
            {
                // Generate 2 random characters from _characters
                char randomChar1 = _characters[random.Next(_characters.Length)];
                char randomChar2 = _characters[random.Next(_characters.Length)];
                char randomChar3 = _characters[random.Next(_characters.Length)];

                string code = $"{lineIndexStr}{yearVal}{monthVal}{dayHourMinuteSecondVal}{randomChar1}{randomChar2}{randomChar3}"; // 15 ký tự

                result.Add(code); // HashSet automatically skips duplicates
            }

            return result.ToList();
        }
    }
}
