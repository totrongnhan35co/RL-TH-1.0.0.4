using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCode.Utils
{
    public static class YearCodeHelper
    {

        public static char GetYearCode(string year)
        {
            // Use a switch statement to map the string year to its corresponding character code.
            switch (year)
            {
                case "25": return '0';
                case "26": return '1';
                case "27": return '2';
                case "28": return '3';
                case "29": return '4';
                case "30": return '5';
                case "31": return '6';
                case "32": return '7';
                case "33": return '8';
                case "34": return '9';
                case "35": return 'B';
                case "36": return 'C';
                case "37": return 'D';
                case "38": return 'F';
                case "39": return 'G';
                case "40": return 'H';
                case "41": return 'J';
                case "42": return 'K';
                case "43": return 'L';
                case "44": return 'M';
                case "45": return 'N';
                case "46": return 'P';
                case "47": return 'Q';
                case "48": return 'R';
                case "49": return 'S';
                case "50": return 'T';
                case "51": return 'V';
                case "52": return 'X';
                case "53": return 'Y';
                case "54": return 'Z';
                default: return '?'; // Return '?' for unknown years
            }
        }
    }

}
