using EncrytionFile.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Windows.Shapes;

namespace BarcodeVerificationSystem.Controller
{
    public class DecryptionHardwareID
    {
        public static string _EncyptionPassword = "rynan_encrypt_remember";
        private static readonly string _EncrytionFilePassword = "RhapsodosZyl_ft_Tieunhan1st";
        public static void DecryptFile(string inputFile)
        {
            try
            {
                string password = _EncrytionFilePassword;
                byte[] key = CreateKey(password);
                var fsCrypt = new FileStream(inputFile, FileMode.Open);
                var RMCrypto = new RijndaelManaged
                {
                    BlockSize = 256,
                    KeySize = 256,
                    IV = key,
                    Key = key
                };

                using (var cs = new CryptoStream(fsCrypt, RMCrypto.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (var reader = new StreamReader(cs))
                    {
                       var hardwareIDList = new List<string>();

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            hardwareIDList.Add(string.Concat(line));
                        }
                        foreach (string hw in hardwareIDList)
                        {
                            var hardwareID = new HardwareIDModel
                            {
                                HardwareID = hw
                            };
                            Shared.listPCAllow.Add(hardwareID);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        //public static void DecryptFile_UUID(string inputFile)
        //{
        //    try
        //    {
        //        string password = _EncrytionFilePassword;
        //        byte[] key = CreateKey(password);

        //        using (var fsCrypt = new FileStream(inputFile, FileMode.Open))
        //        {
        //            using (var aes = Aes.Create())
        //            {
        //                aes.BlockSize = 128;
        //                aes.KeySize = 256;
        //                aes.IV = key.Take(aes.BlockSize / 8).ToArray();
        //                aes.Key = key;
        //                aes.Padding = PaddingMode.PKCS7; // Ensure padding mode is set
        //                Console.WriteLine($"Key: {BitConverter.ToString(key)}");
        //                Console.WriteLine($"IV: {BitConverter.ToString(aes.IV)}");
        //                using (var cs = new CryptoStream(fsCrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
        //                {
        //                    using (var reader = new StreamReader(cs))
        //                    {
        //                        var hardwareIDList = new List<string>();

        //                        while (!reader.EndOfStream)
        //                        {
        //                            string line = reader.ReadLine();
        //                            hardwareIDList.Add(string.Concat(line));
        //                        }
        //                        foreach (string hw in hardwareIDList)
        //                        {
        //                            var hardwareID = new HardwareIDModel
        //                            {
        //                                HardwareID = hw
        //                            };
        //                            Shared.listPCAllow.Add(hardwareID);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        // Handle exceptions as needed
        //    }
        //}
        public static void DecryptFile_UUID(string inputFile)
        {
            try
            {
                string password = _EncrytionFilePassword;
                byte[] key = CreateKey(password);

                using (var fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    using (var aes = Aes.Create())
                    {
                        aes.BlockSize = 128;
                        aes.KeySize = 256;
                        aes.IV = key.Take(aes.BlockSize / 8).ToArray();
                        aes.Key = key;
                        aes.Padding = PaddingMode.PKCS7;

                        using (var cs = new CryptoStream(fsCrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(cs))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();
                                    if (string.IsNullOrWhiteSpace(line)) continue;

                                    string uuid;

                                    if (line.Contains("|"))
                                    {
                                        // Format mới: "UUID|yyyy-MM-dd"
                                        var parts = line.Split('|');
                                        uuid = parts[0].Trim();

                                        if (DateTime.TryParse(parts[1].Trim(), out DateTime expiry))
                                        {
                                            // Hết hạn → bỏ qua
                                            if (DateTime.Today > expiry)
                                                continue;

                                            Shared.LicenseExpireDate = expiry;
                                        }
                                    }
                                    else
                                    {
                                        // Format cũ: chỉ UUID → vô thời hạn
                                        uuid = line.Trim();
                                        Shared.LicenseExpireDate = DateTime.MaxValue;
                                    }

                                    Shared.listPCAllow.Add(new HardwareIDModel { HardwareID = uuid });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        public static string GetUniqueID()
        {
            string cpuID = GetHardwareID("Win32_Processor", "ProcessorId");
            string biosID = GetHardwareID("Win32_BIOS", "SerialNumber");
            string diskID = GetHardwareID("Win32_DiskDrive", "SerialNumber");

            return cpuID + biosID + diskID;
        }

        private static string GetHardwareID(string wmiClass, string wmiProperty)
        {
            string result = "";
            ManagementClass mc = new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if (result == "")
                {
                    result = mo[wmiProperty].ToString();
                    break;
                }
            }

            return result;
        }

        private static readonly byte[] Salt = new byte[] { 10,20,30,40,50,60,70,80 };
        private static byte[] CreateKey(string password,int keyBytes = 32)
        {
            const int Iterations = 300;
            var keyGenerator = new Rfc2898DeriveBytes(password, Salt, Iterations, HashAlgorithmName.SHA256);
            return keyGenerator.GetBytes(keyBytes);
        }
    }
}
