using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Model.Apis.Dispatching;
using BarcodeVerificationSystem.Model.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Policy;
using BarcodeVerificationSystem.Utils;
using BarcodeVerificationSystem.Services.Dispatching;
using Mysqlx.Crud;
using BarcodeVerificationSystem.Services.Manufacturing;
using System.Windows.Forms;
namespace BarcodeVerificationSystem.Services
{
    internal class MonitorSenderService
    {
        private static ManufacturingService manufacturingService = new ManufacturingService();

        public static async void SendParametersToServer()
        {
            await Task.Run(() => SendParametersToServerAsync());
        }

        private static async void SendParametersToServerAsync()
        {
            ApiService apiService = new ApiService();

            while (true)
            {
                try
                {
                    await sendParametersToServerAsync(apiService, true);
                    await Task.Delay(2000);
                }
                catch (Exception)
                {
                }
            }

        }

        public static async Task sendParametersToServerAsync(ApiService apiService, bool isSoftwareConnected)
        {
            try
            {
                string userCode = string.Empty;
                if (Shared.UserPermission.OnlineUserModel != null)
                {
                    userCode = Shared.UserPermission?.OnlineUserModel?.ma_tai_khoan;
                }

                var settings = Shared.Settings;
                var monitor = new MonitorPayload
                {
                    plant = settings.FactoryCode,
                    printed_codes_number = Shared.NumberPrinted,
                    generated_codes_number = Shared.TotalCodes,
                    sent_saas_codes = Shared.NumberOfSentSaaS,
                    sent_saas_check_codes = Shared.NumberOfCheckSentSaaS,
                    sent_sap_check_codes = Shared.NumberOfCheckSentSAP,
                    sent_sap_codes = Shared.NumberOfSentSAP,
                    is_software_connected = isSoftwareConnected,
                    resource_code = settings.LineId,
                    resource_name = settings.LineName,
                    ip_address_rlink = GetLocalIPv4Address(),
                    is_running = Shared.OperStatus == Model.OperationStatus.Running,
                    ip_address_printer = settings.PrinterList[0].IP,
                    ip_address_camera = settings.CameraList[0].IP,
                    is_printer_connected = settings.PrinterList[0].IsConnected,
                    is_camera_connected = settings.CameraList[0].IsConnected,
                    username = userCode,
                    timestamp = DateTime.Now
                };
                //new Form
                //{
                //    Text = "JSON Viewer",
                //    Width = 800,
                //    Height = 600,
                //    Controls = { new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both,
                //    Text = Newtonsoft.Json.JsonConvert.SerializeObject(monitor, Newtonsoft.Json.Formatting.Indented) } }
                //}.ShowDialog();

                var res = await manufacturingService.PostMonitorDataAsync(monitor);
            }
            catch (Exception ex)
            {
            }
        }

        private static string GetLocalIPv4Address()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ip = host
                    .AddressList
                    .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

                return ip?.ToString() ?? "No IPv4 address found";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }

}
