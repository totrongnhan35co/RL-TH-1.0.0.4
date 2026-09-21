using CommonVariable;
using Microsoft.Win32;
using System.Diagnostics;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace BarcodeVerificationSystem.Controller
{
    public static class UtilityFunctions
    {
        public static int BoolsToInt(bool[] bools)
        {
            int value = 0;
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                    value |= 1 << (bools.Length - i - 1);
            }
            return value;
        }

        public static bool[] IntToBools(int value, int numBools)
        {
            bool[] bools = new bool[numBools];
            for (int i = 0; i < numBools; i++)
            {
                bools[i] = (value & (1 << (numBools - i - 1))) != 0;
            }
            return bools;
        }

        public static Bitmap GetImageFromUri(string uri)
        {
            if(uri == "") return null;
            using (var webClient = new WebClient())
            {
                byte[] imageData = webClient.DownloadData(uri);
                using (var ms = new MemoryStream(imageData))
                {
                    var image = new Bitmap(ms);
                    return image;
                }
            }
        }

        #region  implement pre-fetching and caching
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly ConcurrentDictionary<string, Bitmap> _imageCache = new ConcurrentDictionary<string, Bitmap>();
        public static async Task PreFetchImagesAsync(IEnumerable<string> imageUrls)
        {
            var tasks = imageUrls.Select(async imageUrl =>
            {
                if (!_imageCache.ContainsKey(imageUrl))
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(imageUrl);
                        response.EnsureSuccessStatusCode();

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var image = new Bitmap(stream);
                            _imageCache[imageUrl] = image;
                        }
                    }
                    catch
                    {
                        // Handle exceptions as needed
                    }
                }
            });
            await Task.WhenAll(tasks);
        }
        public static Bitmap GetImageFromCache(string imageUrl)
        {
            if (_imageCache.TryGetValue(imageUrl, out Bitmap cachedImage))
            {
                return cachedImage;
            }
            return null;
        }
        #endregion implement pre-fetching and caching

        public static async Task<Image> DownloadAndConvertImage(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    byte[] imageData = await client.GetByteArrayAsync(url);
                    using (MemoryStream originalStream = new MemoryStream(imageData))
                    {
                        using (Image originalImage = Image.FromStream(originalStream))
                        {
                            return originalImage;
                        }
                    }
                }
                catch (Exception e)
                {
                   // Console.WriteLine("Error downloading or processing image: " + e.Message);
                    return null;
                }
            }
        }
        private static Image ConvertToLowQualityImage(Image image)
        {
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters encoderParameters = new EncoderParameters(1);
            EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 70L);  // Set quality to 50%
            encoderParameters.Param[0] = encoderParameter;

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, jpgEncoder, encoderParameters);
                ms.Position = 0;
                return Image.FromStream(ms);
            }
        }
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static void SaveBitmap(Bitmap bitmap, string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                memoryStream.Position = 0;
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    memoryStream.CopyTo(fileStream);
                }
            }
        }

        public static void SaveKeyToRegistry(string idPC)
        {
            string registryPath = @"SOFTWARE\RLinkHelpFolder";
            var key = Registry.LocalMachine.OpenSubKey(registryPath, true) ?? Registry.LocalMachine.CreateSubKey(registryPath);
            key.SetValue("IDPC", idPC);
            key.Close();
        }

        public static string GetKeyFromRegistry(string keyName)
        {
            string registryPath = @"SOFTWARE\RLinkHelpFolder";
            using (var key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    var value = key.GetValue(keyName);
                    if (value != null)
                    {
                        return value.ToString();
                    }
                }
            }
            return null;
        }


        public static void OpenDialogForImage(string dirPath)
        {
            try
            {
                using (var openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = dirPath;
                    openFileDialog.Filter = "Image files (*.bmp)|*.bmp";
                    openFileDialog.FilterIndex = 0;
                    openFileDialog.Multiselect = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFile = openFileDialog.FileName;
                        Process.Start("notepad.exe", selectedFile);
                    }
                }
            }
            catch (Exception) { }
        }

        public static string OpenDirectoryFileDatabase()
        {
            string selectedFolderPath = null;

            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    selectedFolderPath = dialog.SelectedPath;
                }
            }
            return selectedFolderPath;
        }

        public static bool IsVCRedistInstalled(string version)
        {
            try
            {
                string registryPath = $@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\{version}\VC\VCRedist";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        // Check for both x86 and x64 versions
                        var x86 = key.OpenSubKey("x86");
                        var x64 = key.OpenSubKey("x64");

                        if (x86 != null && (int)x86.GetValue("Installed") == 1)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
           
        }

    }
}
