using BarcodeVerificationSystem.Controller;
using Newtonsoft.Json;
using Security;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem
{
    public class CameraListenerServer
    {
        private static HttpListener listener;

        public CameraListenerServer(string[] prefixes)
        {
            if (!HttpListener.IsSupported)
                throw new Exception("Not support HttpListener.");

            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");
            
            listener = new HttpListener();
            foreach (string prefix in prefixes)
                listener.Prefixes.Add(prefix);
        }


        public async Task StartAsync()
        {
            listener.Start();
            do
            {
                try
                { 
                    HttpListenerContext context = await listener.GetContextAsync();
                    await ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            while (listener.IsListening);
        }
        public static void Stop()
        {
            try { listener?.Stop(); } catch { }
        }
        static async Task ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            Stream outputstream = response.OutputStream;
            string url = request.Url.AbsolutePath + request.Url.Query;
            switch (url)
            {
                case "/stop":
                    {
                        listener.Stop();
                        Console.WriteLine("stop http");
                    }
                    break;

                case "/api/request?act=get_printer_info":
                    {
                        response.Headers.Add("Content-Type", "application/json");
                       var cameraBoxInfo = new ResponseGetInfoModel
                        {
                            Success = "true",
                            Message = "Get printer info successful",
                            data = new GetInfoDataResponseModel
                            {
                                Model = Properties.Settings.Default.SoftwareModel,
                                Uuid = FingerPrint.Value(),
                                Software = Properties.Settings.Default.SoftwareVersion,
                                // 0 Undefine
                                // 1 - Start
                                // 2 - stop
                                // 3 - Processing
                                // 4 - Resumer
                                // 5 - reset default
                                // 6 - Waiting data
                                PrintStatus = (int)Shared.OperStatus,
                                PrinterName = "My RLink"
                            }
                        };
                        string jsonstring = JsonConvert.SerializeObject(cameraBoxInfo);
                        byte[] buffer = Encoding.UTF8.GetBytes(jsonstring);
                        response.ContentLength64 = buffer.Length;
                        await outputstream.WriteAsync(buffer, 0, buffer.Length);

                    }
                    break;

                default:
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        byte[] buffer = Encoding.UTF8.GetBytes("NOT FOUND!");
                        response.ContentLength64 = buffer.Length;
                        await outputstream.WriteAsync(buffer, 0, buffer.Length);
                    }
                    break;
            }
            
            outputstream.Close();
        }
    }
}
