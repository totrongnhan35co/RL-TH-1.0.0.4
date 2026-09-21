using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UILanguage;

namespace BarcodeVerificationSystem.Utils.ExportData
{
    public class CustomExport
    {
        // Remove element from array by index or value
       public static T[] RemoveElement<T>(T[] array, T optionValue, int? optionIndex = null)
        {
            if (array == null || array.Length == 0)
                return array;

            if (optionIndex.HasValue) // Remove by index
            {
                int index = optionIndex.Value;
                if (index < 0 || index >= array.Length)
                    return array; // invalid index, return original

                return array.Where((val, i) => i != index).ToArray();
            }
            else // Remove by value
            {
                return array.Where(val => !val.Equals(optionValue)).ToArray();
            }
        }

      


    }
}
