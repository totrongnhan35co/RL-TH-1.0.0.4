using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UILanguage;

namespace BarcodeVerificationSystem.Utils
{
    internal class MaskData
    {
        public static string MaskString(string input)
        {
            return input == "" ? Lang.CannotDetect :
                   input.Length > 4 && Shared.Settings.MaskData
                   ? new string('*', input.Length - 4) + input.Substring(input.Length - 4)
                   : input;
        }
    }
}
