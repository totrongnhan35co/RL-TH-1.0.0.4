using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BarcodeVerificationSystem.Controller
{

    public class SecurityController
    {
        #region Encrypt
        private const int Keysize = 256;
        private const int DerivationIterations = 1000;
        public static string Encrypt(string plainText, string passPhrase)
        {
            byte[] saltStringBytes = Generate256BitsOfRandomEntropy();
            byte[] ivStringBytes = Generate256BitsOfRandomEntropy();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                byte[] keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {

            byte[] cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
          
            byte[] saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
           
            byte[] ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
           
            byte[] cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                byte[] keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        #endregion Encrypt

        #region Registry

        private static string _SubPath = "SYSTEM\\RYNCodeTracker\\";
        public static string SubPath
        {
            get { return _SubPath; }
            set { _SubPath = value; }
        }

        private static RegistryKey _RootRegistryPath = Registry.LocalMachine;
        public static RegistryKey RootRegistryPath
        {
            get { return _RootRegistryPath; }
            set { _RootRegistryPath = value; }
        }

        public static object ReadRegistry(string KeyName)
        {
            RegistryKey rk = RootRegistryPath;
            RegistryKey sk1 = rk.OpenSubKey(SubPath);

            if (sk1 == null)
            {
                return null;
            }
            else
            {
                try
                {
                    if (KeyName != "Administrator" && KeyName != "Operator" && KeyName != "Supporter")
                    {
                        return null;
                    }
                    return sk1.GetValue(KeyName.ToUpper());
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static bool WriteRegistry(string KeyName, object Value)
        {
            try
            {
                RegistryKey rk = RootRegistryPath;
                RegistryKey sk1 = rk.CreateSubKey(SubPath);
                sk1.SetValue(KeyName.ToUpper(), Value);
                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("access registry error: {0}", ex.Message);
#endif
                return false;
            }
        }

        #endregion Registry
    }
}
