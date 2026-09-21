using Cognex.InSight.Remoting.Serialization;
using Cognex.InSight.Web.Controls;
using Cognex.InSight.Web;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net;
using Newtonsoft.Json.Linq;
using static Cognex.Insight.MultiSyncRead.Models.DataTypes;

namespace Cognex.Insight.MultiSyncRead.Models
{
    public class ISSeries : ICognexISSeries
    {
        #region Variables Definition
        private string _ipAddress;
        private string _port;


        public bool IsConnected { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }


        internal readonly CvsInSight _inSight;
        internal CvsDisplay _CvsDisplay;
        internal event EventHandler AutoAddSufixEvent;
        //  internal readonly CameraModel _CameraModel = Shared.Settings.CameraList.FirstOrDefault();
        internal event EventHandler UpdateLabelStatusEvent;
        public bool _IsSymbol;
        private readonly string suf_code = ".Result00.String";
        private readonly string suf_text = ".ReadText";
        #endregion

        public ISSeries(string ip, string port)
        {
            _ipAddress = ip;
            _port = port;

            _inSight = new CvsInSight();
            _inSight.ConnectedChanged += InSight_ConnectedChanged;
            _inSight.ResultsChanged += InSight_ResultsChangedAsync;
        }
        public void StartGetData()
        {
            _cts_GetData?.Cancel();
            _cts_GetData = new CancellationTokenSource();
            PushData();
        }
        public void StopGetData()
        {
            _cts_GetData?.Cancel();
        }
        #region Functions

        public async Task FirstConnection()
        {
            try
            {
                var sessionInfo = new HmiSessionInfo
                {
                    SheetName = "Inspection",
                    CellNames = new string[1] { "A0:Z599" },
                    EnableQueuedResults = true,
                    IncludeCustomView = true,
                    AutoReady = true
                };
                await _inSight.Connect(_ipAddress + ":" + _port, "admin", "", sessionInfo);

                // Debug.WriteLine("The system reconnecttedadasdsfasfsdf");
                //  await _CvsDisplay.OnConnected();
                //  Shared.RaiseOnCameraStatusChangeEvent();
            }
            catch (Exception)
            {
            }
        }

        internal void AutoAddSuffixes(CameraModel cameraModel)
        {
            try
            {
                if (cameraModel == null) return;
                _IsSymbol = cameraModel.IsSymbol;
                for (int i = 0; i < 1; i++)
                {

                    var objectName = _CameraModel.ObjectName.Replace(".ReadText", "").Replace(".Result00.String", "");
                    if (objectName != null)
                    {
                        if (_IsSymbol) // Code
                        {
                            objectName = objectName.Replace(suf_code, "");
                            if (objectName.EndsWith(suf_code))
                            {
                                int suffixPosition = objectName.LastIndexOf(suf_code);  // If already exists, update only the non-suffix part
                                objectName = objectName.Substring(0, suffixPosition);
                            }
                            objectName += suf_code;
                        }
                        else
                        {
                            objectName = objectName.Replace(suf_text, "");
                            if (objectName.EndsWith(suf_text))
                            {
                                int suffixPosition = objectName.LastIndexOf(suf_text);  // If already exists, update only the non-suffix part
                                objectName = objectName.Substring(0, suffixPosition);
                            }
                            objectName += suf_text;
                        }
                        _CameraModel.ObjectName = objectName;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Events
        private async void InSight_ConnectedChanged(object sender, EventArgs e)
        {
            try
            {
                await Task.Delay(500); // wait for preparing connection !
                if (_inSight.Connected)
                {
                    IsConnected = true;
                    ModelNumber = _inSight.CameraInfo.ModelNumber;
                    SerialNumber = _inSight.CameraInfo.SerialNumber;
                }
                else
                {
                    IsConnected = false;
                    ModelNumber = string.Empty;
                    SerialNumber = string.Empty;
                }
            }
            catch (Exception)
            {
            }
        }

        public static ConcurrentQueue<(JToken, string)> _QueueResult = new ConcurrentQueue<(JToken, string)>();

        //Very Fast Event
        private async void InSight_ResultsChangedAsync(object sender, EventArgs e)
        {
            var cognexIs = sender as CvsInSight;
            await Task.Run(() =>
            {
                var imageUrl = cognexIs.GetMainImageUrl(300, 250);
                if (cognexIs.Online && Shared.OperationStatus == OperationStatus.Running)
                    _QueueResult.Enqueue((cognexIs.Results, imageUrl));
            });
            //await cognexIs.SendReady(); 
        }


        CancellationTokenSource _cts_GetData = new CancellationTokenSource();
        //Retrieve data
        private void PushData()
        {
            var cancellationToken = _cts_GetData.Token;
            var threadPush = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        if (_QueueResult.IsEmpty)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }
                        }
                        if (_QueueResult.TryDequeue(out var tokenResults))
                        {
                            if (tokenResults.Item1 == null)
                            {
                                Thread.Sleep(1);
                                continue;
                            }
                            ThreadPool.QueueUserWorkItem(state =>
                            {
                                ProcessData(tokenResults);
                            });
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
            })
            {
                IsBackground = true
            };
            threadPush.Start();
        }

        private void ProcessData((JToken, string) tokenResults)
        {
            try
            {
                var dictionary = tokenResults.Item1.ToObject<Dictionary<string, object>>();
                var tokenResults2 = dictionary["cells"] as JToken;
                var jsonArray = tokenResults2.ToObject<JArray>();
                var resultToken = jsonArray.Children<JObject>().FirstOrDefault(obj => (string)obj["name"] == _CameraModel.ObjectName); //Object_1.TotalCount
                var result = resultToken["data"]?.ToString();
                var featureImage = UtilityFunctions.GetImageFromUri(tokenResults.Item2); //await UtilityFunctions.DownloadAndConvertImage(tokenResults.Item2);
                var detectModel = new DetectModel
                {
                    Text = result,
                    Image = featureImage
                };
                Shared.RaiseOnCameraReadDataChangeEvent(detectModel); // Send data to UI
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
