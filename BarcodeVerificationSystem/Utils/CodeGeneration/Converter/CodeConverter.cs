using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCode.Utils
{
    public static class CodeConverter
    {

      
            // Ánh xạ ký tự sang số
            private static readonly Dictionary<char, int> _charToValue = new Dictionary<char, int>
            {
                ['0'] = 0,
                ['1'] = 1,
                ['2'] = 2,
                ['3'] = 3,
                ['4'] = 4,
                ['5'] = 5,
                ['6'] = 6,
                ['7'] = 7,
                ['8'] = 8,
                ['9'] = 9,
                ['B'] = 10,
                ['C'] = 11,
                ['D'] = 12,
                ['F'] = 13,
                ['G'] = 14,
                ['H'] = 15,
                ['J'] = 16,
                ['K'] = 17,
                ['L'] = 18,
                ['M'] = 19,
                ['N'] = 20,
                ['P'] = 21,
                ['Q'] = 22,
                ['R'] = 23,
                ['S'] = 24,
                ['T'] = 25,
                ['V'] = 26,
                ['X'] = 27,
                ['Y'] = 28,
                ['Z'] = 29
            };

            // Ánh xạ số ngược về ký tự
            private static readonly Dictionary<int, char> _valueToChar = _charToValue
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            // Trả về ký tự checksum từ chuỗi đầu vào
            public static char GetChecksumChar(string input, out int total)
            {
                total = 0;

                foreach (char c in input.ToUpper())
                {
                    if (_charToValue.TryGetValue(c, out int value))
                    {
                        total += value;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid character '{c}' in input.");
                    }
                }

                int remainder = total % 30;
                return _valueToChar.TryGetValue(remainder, out char checksum) ? checksum : '?';
            }
        }

}
