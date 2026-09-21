using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Utils.CodeGeneration.Helper
{
    public class LineConvertHelper
    {
        public static char GetLineCode(int lineNo)
        {
            // Use a switch statement to map the string year to its corresponding character code.
            switch (lineNo)
            {
                case 1: return '0';
                case 2: return '1';
                case 3: return '2';
                case 4: return '3';
                case 5: return '4';
                case 6: return '5';
                case 7: return '6';
                case 8: return '7';
                case 9: return '8';
                case 10: return '9';
                case 11: return 'B';
                case 12: return 'C';
                case 13: return 'D';
                case 14: return 'F';
                case 15: return 'G';
                case 16: return 'H';
                case 17: return 'J';
                case 18: return 'K';
                case 19: return 'L';
                case 20: return 'M';
                case 21: return 'N';
                case 22: return 'P';
                case 23: return 'Q';
                case 24: return 'R';
                case 25: return 'S';
                case 26: return 'T';
                case 27: return 'V';
                case 28: return 'X';
                case 29: return 'Y';
                case 30: return 'Z';
                default: return '?'; // Return '?' for unknown years
            }
        }
    }
}
