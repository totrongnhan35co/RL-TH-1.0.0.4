using System;
using Microsoft.Win32;

namespace BarcodeVerificationSystem.Utils
{
    internal class RegistryHelper
    {
        public static void WriteValue(string valueName, string valueData)
        {
            string subKeyPath = @"Software\R-Link";

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subKeyPath))
            {
                if (key != null)
                {
                    key.SetValue(valueName, valueData);
                    Console.WriteLine("Value stored in registry successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to create or open registry key.");
                }
            }

            Console.ReadLine();
        }

        public static string ReadValue(string valueName)
        {
            string subKeyPath = @"Software\R-Link";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subKeyPath))
            {
                if (key != null)
                {
                    object value = key.GetValue(valueName);
                    if (value != null)
                    {
                        Console.WriteLine($"Value read from registry: {value}");
                        return value.ToString();
                    }
                    else
                    {
                        Console.WriteLine($"Value '{valueName}' not found in registry.");
                    }
                }
                else
                {
                    Console.WriteLine("Registry key not found.");
                }
            }

            return null;
        }

    }
}
