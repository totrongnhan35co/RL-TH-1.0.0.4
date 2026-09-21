using FontAwesome.Sharp;
using GenCode.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace BarcodeVerificationSystem.Utils.CodeGeneration.Helper
{
    public class Base30Helper
    {
        // Table of characters base30: remove A, O, U, I, E, W
        private static readonly char[] Base30Chars = {
        '0','1','2','3','4','5','6','7','8','9',
        'B','C','D','F','G','H','J','K','L','M',
        'N','P','Q','R','S','T','V','X','Y','Z'
    };


        public static string EncodeToBase30_Loyaltly(int number)
        {
            List<char> result = new List<char>();

            if (number == 0)
            {
                result.Add(Base30Chars[0]);
            }
            //
            while (number > 0)
            {
                int remainder = number % 30;
                result.Add(Base30Chars[remainder]);
                number /= 30;
            }

            // Đảo ngược và đệm 0 nếu chưa đủ 6 ký tự
            result.Reverse();
            while (result.Count < 6)
                result.Insert(0, '0');

            return new string(result.ToArray());
        }

        public static string EncodeToBase30_5Chars(int number)
        {
            List<char> result = new List<char>();

            if (number == 0)
            {
                result.Add(Base30Chars[0]);
            }

            while (number > 0)
            {
                int remainder = number % 30;
                result.Add(Base30Chars[remainder]);
                number /= 30;
            }

            result.Reverse();

            while (result.Count < 5)
                result.Insert(0, '0');

            return new string(result.ToArray());
        }



        // Old way , not use
        public static string EncodeToBase30WithChecksum_Export(int number, string lineIndex)
        {
            // Step 1: Encode number to base30 (min 6 chars)
            List<char> result = new List<char>();
            if (number == 0)
            {
                result.Add(Base30Chars[0]);
            }

            int temp = number;
            while (temp > 0)
            {
                int remainder = temp % 30;
                result.Add(Base30Chars[remainder]);
                temp /= 30;
            }
            result.Reverse();
            while (result.Count < 6)
                result.Insert(0, '0');
            string base30Body = new string(result.ToArray());

            // Step 2: Add prefix (e.g., version/type code)
            string prefix = lineIndex; // You can define rules for this

            // Step 3: Add 3-char checksum (e.g., simple sum of digits)
            int checksumValue = (number * 31 + 17) % (int)Math.Pow(30, 3);  // Any formula
            char[] checksum = new char[3];
            for (int i = 2; i >= 0; i--)
            {
                checksum[i] = Base30Chars[checksumValue % 30];
                checksumValue /= 30;
            }

            // Final code =  prefix + 6 encoded + 3 checksum = (>=10) chars
            return prefix + base30Body + new string(checksum);
        }

    }
}
