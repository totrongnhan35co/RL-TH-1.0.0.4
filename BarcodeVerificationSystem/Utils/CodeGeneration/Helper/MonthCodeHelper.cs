using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCode.Utils
{
    public static class MonthCodeHelper
    {
        public static char GetMonthCode(string month)
        {
            switch (month)
            {
                case "01": return 'B';
                case "02": return 'C';
                case "03": return 'D';
                case "04": return 'F';
                case "05": return 'G';
                case "06": return 'H';
                case "07": return 'J';
                case "08": return 'K';
                case "09": return 'L';
                case "10": return 'M';
                case "11": return 'N';
                case "12": return 'P';
                default: return '?'; // invalid month
            }
        }
    }

}
