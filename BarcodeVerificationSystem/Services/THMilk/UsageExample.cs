using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarcodeVerificationSystem.Services.THMilk;
using BarcodeVerificationSystem.Services.THMilk.Controller;

namespace BarcodeVerificationSystem.Services.THMilk
{
    /// <summary>
    /// Example usage of TH Milk API Service in your application
    /// </summary>
    public class UsageExample
    {
        private THMilkAPIHandler _apiHandler;

        public void InitializeService()
        {
            // Initialize the API handler on port 5001
            _apiHandler = new THMilkAPIHandler("http://*:5001/");

            // Subscribe to QR code received events
            _apiHandler.QRCodeReceived += OnQRCodeReceived;

            // Subscribe to API request events for logging
            _apiHandler.APIRequestReceived += OnAPIRequestReceived;

            // Start the server
            //_apiHandler.Start();

            Console.WriteLine("TH Milk API Service initialized and started");
        }

        private void OnQRCodeReceived(object sender, QRCodeReceivedEventArgs e)
        {
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine("QR Code Data Received:");
            Console.WriteLine($"  Code: {e.Code}");
            Console.WriteLine($"  Line IP: {e.LineIP}");
            Console.WriteLine($"  Line Name: {e.LineName}");
            Console.WriteLine($"  Authenticated User: {e.AuthenticatedUser}");
            Console.WriteLine($"  Received At: {e.ReceivedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("══════════════════════════════════════════");

            // TODO: Process the QR code data in your application
            // Examples:
            // - Store in database
            // - Send to PLC
            // - Trigger inspection
            // - Update UI
            // - Log to file

            ProcessQRCodeInProduction(e);
        }

        private void OnAPIRequestReceived(object sender, APIRequestEventArgs e)
        {
            // Log all API requests for monitoring
            Console.WriteLine($"[API] {e.Timestamp:HH:mm:ss} - {e.Method} {e.Path} from {e.ClientIP}");
        }

        private void ProcessQRCodeInProduction(QRCodeReceivedEventArgs qrData)
        {
            // Example implementation - customize based on your production needs

            try
            {
                // 1. Validate the QR code format
                if (!IsValidQRCodeFormat(qrData.Code))
                {
                    Console.WriteLine($"Invalid QR code format: {qrData.Code}");
                    return;
                }

                // 2. Extract information from QR code
                var productInfo = ExtractProductInfo(qrData.Code);
                Console.WriteLine($"Product Info: {productInfo}");

                // 3. Send to production line / PLC
                // SendToPLC(qrData.LineName, qrData.Code);

                // 4. Store in database
                // SaveToDatabase(qrData);

                // 5. Update UI
                // UpdateProductionUI(qrData);

                Console.WriteLine("QR code processed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing QR code: {ex.Message}");
            }
        }

        private bool IsValidQRCodeFormat(string code)
        {
            // Example validation - adjust based on your requirements
            // TH True Milk format: https://ndatrace.vn/01/8935217402816/21/hp9v3mzoio
            return !string.IsNullOrEmpty(code) && 
                   (code.StartsWith("http://") || code.StartsWith("https://"));
        }

        private string ExtractProductInfo(string code)
        {
            // Extract product information from QR code URL
            // This is a simple example - customize based on actual format
            try
            {
                var uri = new Uri(code);
                var segments = uri.Segments;
                return $"Product segments: {string.Join(", ", segments)}";
            }
            catch
            {
                return "Unable to parse QR code";
            }
        }

        public void StopService()
        {
            if (_apiHandler != null && _apiHandler.IsRunning)
            {
                _apiHandler.Stop();
                Console.WriteLine("TH Milk API Service stopped");
            }
        }

        // Example: How to use this in your main application
        public static void Main_Example()
        {
            var example = new UsageExample();
            
            // Initialize and start the service
            example.InitializeService();

            // Keep the application running
            Console.WriteLine("Press any key to stop the service...");
            Console.ReadKey();

            // Stop the service
            example.StopService();
        }
    }
}
