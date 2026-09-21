using BarcodeVerificationSystem.Controller.Camera;
using BarcodeVerificationSystem.Model;
using Cognex.DataMan.CogNamer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Controller
{
    public class ISSingleHandler : IDisposable
    {
        #region Declaration
        public ISCameraController _isCam1;
        public bool IsConnected { get; set; }
        private ConcurrentDictionary<int, DataISCamera> _camera1Data = new ConcurrentDictionary<int, DataISCamera>();
        private readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(100) //Increase timeout to 200ms
        };
        private  readonly Bitmap _placeholderImage = new Bitmap(100, 100); //Default placeholder
        private  readonly long maxSizeInBytes = 5 * 1024 * 1024; //Image size limit (5MB)
        private  readonly HashSet<string> supportedImageFormats = new HashSet<string>
{
    "image/jpeg", "image/png", "image/bmp", "image/gif"
};
        private ConcurrentQueue<DataISCamera> _processingQueue = new ConcurrentQueue<DataISCamera>();
        internal readonly CameraModel _CameraModel = Shared.Settings.CameraList.FirstOrDefault();
        string _objectNameMaster = "";
        string _objectNameSlave = "";
        private readonly string suf_code = ".Result00.String";
        private readonly string suf_text = ".ReadText";


        #endregion Declaration
        public ISSingleHandler(string ip1, string port1)
        {
            _isCam1 = new ISCameraController(ip1, port1, ISCameraController.Role.master);
            _isCam1.ChangeStatusEvent += ISCam1_ChangeStatusEvent;
            //Shared.OnStopButtonClick -= Shared_OnStopButtonClickAsync;
            //Shared.OnStopButtonClick += Shared_OnStopButtonClickAsync;

            Shared.OnNextButtonEvent += Shared_OnNextButtonEventAsync;
            Shared.OnAddSuffix += Shared_OnAddSuffixAsync;

            Task.Run(() => ListenToCamera(_isCam1, _camera1Data));
            Task.Run(() => RetrieveData());
        }

        private async void Shared_OnNextButtonEventAsync(object sender, EventArgs e)
        {
            await InitJob();
        }
        #region Event Handler
        private void ISCam1_ChangeStatusEvent(object sender, EventArgs e)
        {
            CheckStatus();
        }
        //private async void Shared_OnStopButtonClickAsync(object sender, EventArgs e)
        //{
        //    await InitJob();
        //}
        private void Shared_OnAddSuffixAsync(object sender, EventArgs e)
        {
            var cameraModel = (CameraModel)sender;
            _objectNameMaster = ObjectNameAddedSuffixes(cameraModel, true);
          //  _objectNameSlave = ObjectNameAddedSuffixes(cameraModel, false);
          
        }
        #endregion

        public async Task FirtConnectionAsync()
        {
            await _isCam1.FirstConnection();
            await InitJob();
        }
        public bool CheckStatus()
        {
            if (_isCam1 != null)
            {
                if (_isCam1.IsConnected)
                {
                    _CameraModel.IsConnected = true;
                    _CameraModel.SerialNumber = "Single: " + _isCam1.SerialNumber;
                    _CameraModel.Name = "Single: " + _isCam1.ModelNumber;
                    IsConnected = true;
                    return true;
                }
                else
                {
                    _CameraModel.IsConnected = false;
                    _CameraModel.SerialNumber = "";
                    _CameraModel.Name = "";
                    IsConnected = false;
                    return false;
                }
            }
            return false;
        }
        public async Task InitJob()
        {
            await _isCam1._inSight.SetSoftOnlineAsync(false);
            // Start Job name
            _ = _isCam1._inSight.LoadJob(_CameraModel.CameraJobNameMaster);
          
            //Try to set the camera online (should be turn of Insight Vision Suite)
            await _isCam1._inSight.SetSoftOnlineAsync(true);
        }
        internal string ObjectNameAddedSuffixes(CameraModel cameraModel, bool isMaster = false)
        {

            string objectName = "";
            bool isSymbol = false;
            if (cameraModel == null) return objectName;
            if (isMaster)
            {
                objectName = cameraModel.ObjectNameMaster;
                isSymbol = cameraModel.IsSymbolMaster;
            }
            else
            {
                objectName = cameraModel.ObjectNameSlave;
                isSymbol = cameraModel.IsSymbolSlave;
            }
            try
            {
                for (int i = 0; i < 1; i++)
                {
                    var name = objectName.Replace(".ReadText", "").Replace(".Result00.String", "");
                    if (name != null)
                    {
                        if (isSymbol) // Code
                        {
                            name = name.Replace(suf_code, "");
                            if (name.EndsWith(suf_code))
                            {
                                int suffixPosition = name.LastIndexOf(suf_code);  // If already exists, update only the non-suffix part
                                name = name.Substring(0, suffixPosition);
                            }
                            name += suf_code;
                        }
                        else
                        {
                            name = name.Replace(suf_text, "");
                            if (name.EndsWith(suf_text))
                            {
                                int suffixPosition = name.LastIndexOf(suf_text);  // If already exists, update only the non-suffix part
                                name = name.Substring(0, suffixPosition);
                            }
                            name += suf_text;
                        }
                        objectName = name;
                    }
                }
                return objectName;
            }
            catch (Exception)
            {
                return objectName;
            }
        }
        private void ListenToCamera(ISCameraController camera, ConcurrentDictionary<int, DataISCamera> cameraData)
        {
            foreach (var data in camera._blockColResult.GetConsumingEnumerable())
            {
                cameraData.TryAdd(data.Index, data);
            }
        }
        private void RetrieveData()
        {
            while (true)
            {
                foreach (var index in _camera1Data.Keys)
                {
                    if (_camera1Data.TryRemove(index, out var dataCam1))
                    {
                        _processingQueue.Enqueue(dataCam1);
                    }
                }
             
                SpinWait.SpinUntil(() => false, 1); //To achieve a faster, low-latency wait,
                if (!_processingQueue.IsEmpty)
                {
                    if (_processingQueue.TryDequeue(out var dataPair))
                    {
                        ProcessCombinedDataAsync(dataPair);
                    }
                }
            }
        }
        private async void ProcessCombinedDataAsync(DataISCamera dataCam1)
        {
            try
            {
                var detectModel1 = dataCam1 != null ? await ProcessData(dataCam1, _objectNameMaster).ConfigureAwait(false) : null;
                var finalDetectModel = await ProcessResultsAsync(detectModel1).ConfigureAwait(false);
                if (finalDetectModel != null)
                {
                    if (finalDetectModel.Image == null)
                    {
                        var image = new Bitmap(100, 100);
                    }
                    Shared.RaiseOnCameraReadDataChangeEvent(finalDetectModel);
                }
            }
            catch (Exception)
            {
            }
        }
        private async Task<DetectModel> ProcessData(DataISCamera tokenResults, string objectName)
        {
            try
            {
                if (tokenResults == null || tokenResults.JTokenData == null)
                {
                    return null;
                }

                var dictionary = tokenResults.JTokenData.ToObject<Dictionary<string, object>>();
                if (!dictionary.TryGetValue("cells", out var tokenResults2) || !(tokenResults2 is JToken))
                {
                    return null;
                }
                var jsonArray = (tokenResults2 as JToken)?.ToObject<JArray>();
                if (jsonArray == null || !jsonArray.HasValues)
                {
                    return null;
                }
                var resultToken = jsonArray.Children<JObject>()
                                           .FirstOrDefault(obj => (string)obj["name"] == objectName);
                if (resultToken == null)
                {
                    return null;
                }

                var result = resultToken["data"]?.ToString(); // Get text result

                // Get Image From URL fastest 
                Bitmap featureImage = null;
                if (!string.IsNullOrEmpty(tokenResults.ImageUrl))
                {
                    featureImage = await GetImageFromUrlAsync(tokenResults.ImageUrl).ConfigureAwait(false);
                }

                return new DetectModel
                {
                    Text = result,
                    Image = featureImage
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
        private async Task<Bitmap> GetImageFromUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return _placeholderImage; //Returns placeholder if URL is invalid
            }

            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        var contentType = response.Content.Headers.ContentType?.MediaType;
                        if (contentType == null || !supportedImageFormats.Contains(contentType))
                        {
                            return _placeholderImage;
                        }
                        var contentLength = response.Content.Headers.ContentLength ?? 0;
                        if (contentLength == 0)
                        {
                            return _placeholderImage;
                        }
                        else if (contentLength > maxSizeInBytes)
                        {
                            return _placeholderImage;
                        }
                        using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            try
                            {
                                return new Bitmap(stream); // Create Bitmap object from stream
                            }
                            catch (Exception)
                            {
                                return _placeholderImage;
                            }
                        }
                    }
                }
                catch (HttpRequestException)
                {
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception)
                {
                }
                await Task.Delay(1).ConfigureAwait(false);
            }
            return _placeholderImage; //Returns the placeholder if it fails after retries
        }
        private async Task<DetectModel> ProcessResultsAsync(DetectModel detectModel1)
        {
            try
            {
                if (detectModel1 == null)
                {
                    return null;
                }
                var combinedText = new StringBuilder();
                combinedText.Append(detectModel1.Text);

                // drop images asynchronously
                  var dropImage = await CompressImagesAsync(detectModel1.Image).ConfigureAwait(false);
                //    var dropImage =  ResizeImage(detectModel1.Image, detectModel1.Image.Width / 2, detectModel1.Image.Height / 2);
                return new DetectModel
                {
                    Text = combinedText.ToString(),
                    Image =  dropImage
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
        private Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }
        private async Task<Bitmap> CompressImagesAsync(Bitmap image1)
        {
            try
            {
                return await Task.Run(() =>
                {
                    if (image1 == null)
                    {
                        return null;
                    }
                    Bitmap resizedImage1 = null;
                    resizedImage1 = ResizeImage(image1, image1.Width / 2, image1.Height / 2);
                    Parallel.Invoke(
                        () => { if (image1 != null) resizedImage1 = ResizeImage(image1, image1.Width / 2, image1.Height / 2); }
                    );

                     if (resizedImage1 == null) return null;

                    int width = resizedImage1.Width;
                    int height = resizedImage1.Height;
                    var reDrawImage = new Bitmap(width, height);
                    using (var g = Graphics.FromImage(reDrawImage))
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                        g.DrawImage(resizedImage1, 0, 0);
                    }
                    return reDrawImage;
                }).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Munual Trigger
        /// </summary>
        internal void ManualTriggerAction()
        {
            try
            {
                _ = _isCam1?._inSight?.ManualAcquire();
            }
            catch (Exception)
            {

            }

        }

        #region Dispose Resources  
        private bool _disposed = false; // To detect redundant calls
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose managed resources here
                // Free managed resources here
                if (_isCam1 != null)
                {
                    _isCam1.Dispose();
                    _isCam1 = null;
                }

               

                _camera1Data?.Clear();
              
                _processingQueue = null;
                Shared.OnNextButtonEvent -= Shared_OnNextButtonEventAsync;
                Shared.OnAddSuffix -= Shared_OnAddSuffixAsync;
                _httpClient?.Dispose();
                _placeholderImage?.Dispose();
            }

            // Free unmanaged resources here (if any)

            _disposed = true;
        }

        internal async Task DisconnectAsync()
        {
            try
            {
                await _isCam1?._inSight?.Disconnect();
            }
            catch (Exception)
            {
            }
        }

        ~ISSingleHandler()
        {
            Dispose(false);
        }
        #endregion Dispose Resources  

    }
}
