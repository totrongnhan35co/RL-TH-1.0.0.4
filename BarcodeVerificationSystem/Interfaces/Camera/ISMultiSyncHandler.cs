using BarcodeVerificationSystem.Controller.Camera;
using BarcodeVerificationSystem.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Controller
{

    public class ISMultiSyncHandler : IDisposable
    {
        #region Declaration
        public  ISCameraController _isCam1;
        public  ISCameraController _isCam2;
       
        public bool IsConnected { get; set; }
        private ConcurrentDictionary<int, DataISCamera> _camera1Data = new ConcurrentDictionary<int, DataISCamera>();
        private ConcurrentDictionary<int, DataISCamera> _camera2Data = new ConcurrentDictionary<int, DataISCamera>();
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
        private ConcurrentQueue<(DataISCamera cam1Data, DataISCamera cam2Data)> _processingQueue = new ConcurrentQueue<(DataISCamera, DataISCamera)>();
        internal readonly CameraModel _CameraModel = Shared.Settings.CameraList.FirstOrDefault();
        string _objectNameMaster = "";
        string _objectNameSlave = "";

        #endregion Declaration

        public bool CheckStatus()
        {
            if(_isCam1!=null && _isCam2 != null)
            {
                if (_isCam1.IsConnected && _isCam2.IsConnected)
                {
                    _CameraModel.IsConnected =true;
                    _CameraModel.SerialNumber = "Master: "+ _isCam1.SerialNumber + " - " + "Slave: "+ _isCam2.SerialNumber;
                    _CameraModel.Name = "Master: " + _isCam1.ModelNumber + " - " + "Slave: "+ _isCam2.ModelNumber;
                    IsConnected = true;
                    return true;
                }
                else
                {
                    _CameraModel.IsConnected = false;
                    _CameraModel.SerialNumber = "";
                    _CameraModel.Name = "";
                    IsConnected = false;
                    return  false;
                }
            }
            return false;
        }


        public ISMultiSyncHandler(string ip1, string port1, string ip2 = "80", string port2 = "80")
        {
            _isCam1 = new ISCameraController(ip1, port1, ISCameraController.Role.master);
            _isCam2 = new ISCameraController(ip2, port2, ISCameraController.Role.slave);
            _isCam1.ChangeStatusEvent += _isCam1_ChangeStatusEvent;
            _isCam2.ChangeStatusEvent += _isCam2_ChangeStatusEvent;
            //Shared.OnStopButtonClick -= Shared_OnStopButtonClickAsync;
            //Shared.OnStopButtonClick += Shared_OnStopButtonClickAsync;

            Shared.OnNextButtonEvent += Shared_OnNextButtonEvent;
            Shared.OnAddSuffix += Shared_OnAddSuffixAsync;

            Task.Run(() => ListenToCamera(_isCam1, _camera1Data));
            Task.Run(() => ListenToCamera(_isCam2, _camera2Data));
            Task.Run(() => CombineData());
        }

        private async void Shared_OnNextButtonEvent(object sender, EventArgs e)
        {
           await InitJob();
        }

        private  void Shared_OnAddSuffixAsync(object sender, EventArgs e)
        {
            var cameraModel = (CameraModel)sender;
            _objectNameMaster = ObjectNameAddedSuffixes(cameraModel, true);
            _objectNameSlave = ObjectNameAddedSuffixes(cameraModel, false);
          //  await InitJob();
        }

        private readonly string suf_code = ".Result00.String";
        private readonly string suf_text = ".ReadText";
      
        /// <summary>
        /// Get Object Name from user input and add suffixes for Json data
        /// </summary>
        /// <param name="cameraModel"></param>
        internal string ObjectNameAddedSuffixes(CameraModel cameraModel, bool isMaster = false)
        {

            string objectName ="";
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
        //private async void Shared_OnStopButtonClickAsync(object sender, EventArgs e)
        //{
        //    await InitJob();
        //}

        private void _isCam2_ChangeStatusEvent(object sender, EventArgs e)
        {
           // status1 = (bool)sender;
            CheckStatus();
        }

        private void _isCam1_ChangeStatusEvent(object sender, EventArgs e)
        {
            CheckStatus();
        }



        #region Functions
        private void ListenToCamera(ISCameraController camera, ConcurrentDictionary<int, DataISCamera> cameraData)
        {
            foreach (var data in camera._blockColResult.GetConsumingEnumerable())
            {
                cameraData.TryAdd(data.Index, data);
            }
        }

        private async Task CombineData()
        {
            while (true)
            {
                foreach (var index in _camera1Data.Keys)
                {
                    if (_camera2Data.ContainsKey(index))
                    {
                        if (_camera1Data.TryRemove(index, out var dataCam1) && _camera2Data.TryRemove(index, out var dataCam2))
                        {
                            _processingQueue.Enqueue((dataCam1, dataCam2));

                        }
                    }
                }
                await Task.Delay(1);

                if (!_processingQueue.IsEmpty)
                {
                    if (_processingQueue.TryDequeue(out var dataPair))
                    {
                        ProcessCombinedDataAsync(dataPair.cam1Data, dataPair.cam2Data);
                    }
                }
            }
        }

        private async void ProcessCombinedDataAsync(DataISCamera dataCam1, DataISCamera dataCam2)
        {
            try
            {
                var detectModel1 = dataCam1 != null ? await ProcessData(dataCam1, _objectNameMaster).ConfigureAwait(false) : null;
                var detectModel2 = dataCam2 != null ? await ProcessData(dataCam2, _objectNameMaster).ConfigureAwait(false) : null;
                var combinedDetectModel = await CombineResultsAsync(detectModel1, detectModel2).ConfigureAwait(false);
                if (combinedDetectModel != null)
                {
                    if (combinedDetectModel.Image == null)
                    {
                        Image image = new Bitmap(100, 100);
                    }
                    Shared.RaiseOnCameraReadDataChangeEvent(combinedDetectModel);
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Exception in ProcessCombinedDataAsync: {ex.Message}");
            }
        }
        
        public async Task FirtConnectionAsync()
        {
            await _isCam1.FirstConnection();
            await _isCam2.FirstConnection();
             await InitJob();
        }

        public async Task InitJob()
        {
            await _isCam1._inSight.SetSoftOnlineAsync(false);
            await _isCam2._inSight.SetSoftOnlineAsync(false);
            // Start Job name
            _ = _isCam1._inSight.LoadJob(_CameraModel.CameraJobNameMaster);//SlaveJob.jobx
            _ = _isCam2._inSight.LoadJob(_CameraModel.CameraJobNameSlave); // "Master.jobx"
            //Try to set the camera online (should be turn of Insight Vision Suite)
            await _isCam1._inSight.SetSoftOnlineAsync(true);
            await _isCam2._inSight.SetSoftOnlineAsync(true);
          
        }

        private async Task<DetectModel> CombineResultsAsync(DetectModel detectModel1, DetectModel detectModel2)
        {
            try
            {
                if (detectModel1 == null && detectModel2 == null)
                {
                    return null;
                }

                if (detectModel1 == null)
                {
                    return detectModel2;
                }

                if (detectModel2 == null)
                {
                    return detectModel1;
                }

                var combinedText = new StringBuilder();
                combinedText.Append(detectModel1.Text);
                combinedText.Append(detectModel2.Text);

                // Combine images asynchronously
                var combinedImage = await CombineImagesAsync(detectModel1.Image, detectModel2.Image).ConfigureAwait(false);
                return new DetectModel
                {
                    Text = combinedText.ToString(),
                    Image = combinedImage
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<Bitmap> CombineImagesAsync(Bitmap image1, Bitmap image2)
        {
            try
            {
                return await Task.Run(() =>
                {
                    if (image1 == null && image2 == null)
                    {
                        return null;
                    }

                    Bitmap resizedImage1 = null;
                    Bitmap resizedImage2 = null;

                    Parallel.Invoke(
                        () => { if (image1 != null) resizedImage1 = ResizeImage(image1, image1.Width / 2, image1.Height / 2); },
                        () => { if (image2 != null) resizedImage2 = ResizeImage(image2, image2.Width / 2, image2.Height / 2); }
                    );

                    if (resizedImage1 == null) return resizedImage2;
                    if (resizedImage2 == null) return resizedImage1;

                    int width = resizedImage1.Width + resizedImage2.Width;
                    int height = Math.Max(resizedImage1.Height, resizedImage2.Height);
                    Bitmap combinedImage = new Bitmap(width, height);

                    using (var g = Graphics.FromImage(combinedImage))
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                        g.DrawImage(resizedImage1, 0, 0);
                        g.DrawImage(resizedImage2, resizedImage1.Width, 0);
                    }
                    return combinedImage;
                }).ConfigureAwait(false);
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

        public async Task DisconnectAsync()
        {
            try
            {
                await _isCam1?._inSight?.Disconnect();
                await _isCam2?._inSight?.Disconnect();
            }
            catch (Exception)
            {
            }
          
        }
        private async Task<Bitmap> GetImageFromUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                //  Console.WriteLine("Error: URL is null or empty. Returning placeholder image.");
                return _placeholderImage; //Returns placeholder if URL is invalid
            }

            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

                    // Check if the HTTP request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        //Check if the content is an image
                        var contentType = response.Content.Headers.ContentType?.MediaType;
                        if (contentType == null || !supportedImageFormats.Contains(contentType))
                        {
                            //  Console.WriteLine($"Error: Unsupported or missing image format '{contentType}'. Returning placeholder image.");
                            return _placeholderImage;
                        }

                        //Check content size if applicable
                        var contentLength = response.Content.Headers.ContentLength ?? 0;
                        if (contentLength == 0)
                        {
                            //  Console.WriteLine("Error: Image content is empty. Returning placeholder image.");
                            return _placeholderImage;
                        }
                        else if (contentLength > maxSizeInBytes)
                        {
                            // Console.WriteLine("Error: Image size exceeds limit. Returning placeholder image.");
                            return _placeholderImage;
                        }

                        // Read image data from stream
                        using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            try
                            {
                                return new Bitmap(stream); // Create Bitmap object from stream
                            }
                            catch (Exception ex)
                            {
                                //  Console.WriteLine($"Error: Failed to create Bitmap from stream. {ex.Message}. Returning placeholder image.");
                                return _placeholderImage;
                            }
                        }
                    }
                    else
                    {
                        //   Console.WriteLine($"Error: HTTP request failed with status code {response.StatusCode}. Retrying...");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    //  Console.WriteLine($"Error: HTTP request exception (attempt {i + 1}). {httpEx.Message}. Retrying...");
                }
                catch (TaskCanceledException timeoutEx)
                {
                    // Console.WriteLine($"Error: Request timed out (attempt {i + 1}). {timeoutEx.Message}. Retrying...");
                }
                catch (Exception ex)
                {
                    //  Console.WriteLine($"Error: General exception (attempt {i + 1}). {ex.Message}. Retrying...");
                }

                // Delay before retrying
                await Task.Delay(1).ConfigureAwait(false);
            }

            // Console.WriteLine("Error: Failed to fetch image after all retries. Returning placeholder image.");
            return _placeholderImage; //Returns the placeholder if it fails after retries
        }

        private async Task<DetectModel> ProcessData(DataISCamera tokenResults,string objectName)
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

        internal void ManualTriggerAction()
        {
            try
            {
                _ = _isCam1?._inSight?.ManualAcquire();
                _ = _isCam2?._inSight?.ManualAcquire();
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

                if (_isCam2 != null)
                {
                    _isCam2.Dispose();
                    _isCam2 = null;
                }

                _camera1Data?.Clear();
                _camera2Data?.Clear();
                _processingQueue = null;
                Shared.OnAddSuffix -= Shared_OnAddSuffixAsync;
                Shared.OnNextButtonEvent -= Shared_OnNextButtonEvent;
                _httpClient?.Dispose();
                _placeholderImage?.Dispose();
            }

            // Free unmanaged resources here (if any)

            _disposed = true;
        }
        ~ISMultiSyncHandler()
        {
            Dispose(false);
        }
        #endregion Dispose Resources  

        #endregion Function
    }
}
