using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BarcodeVerificationSystem.Services.THMilk.Model.ReceiveModel;
using BarcodeVerificationSystem.Services.THMilk.Model.ResponseModel;
using Newtonsoft.Json;

namespace BarcodeVerificationSystem.Services.THMilk.Controller
{
    /// <summary>
    /// QR Code API Controller - handles /api/code endpoint
    /// </summary>
    public class CodeController : BaseController
    {
        // Event to notify when QR code data is received
        public event EventHandler<QRCodeReceivedEventArgs> QRCodeReceived;

        /// <summary>
        /// POST /api/code
        /// Handles QR code data submission (requires authorization)
        /// </summary>
        public void HandleCodeData(HttpListenerRequest request, HttpListenerResponse response, string body)
        {
            try
            {
                // Validate HTTP method
                if (!ValidateMethod(request, response, "POST"))
                    return;

                Console.WriteLine($"[CodeController] Processing QR code data");

                // Get and validate authorization header
                string authHeader = request.Headers["Authorization"];
                if (string.IsNullOrWhiteSpace(authHeader))
                {
                    Unauthorized(response, "Missing Authorization header");
                    return;
                }

                // Extract token from header
                string token = authHeader;
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader.Substring(7).Trim();
                }

                // Validate the token
                if (!LoginController.ValidateToken(token, out string username, out string tokenError))
                {
                    Console.WriteLine($"[CodeController] Token validation failed: {tokenError}");
                    Unauthorized(response, tokenError ?? "Invalid or expired token");
                    return;
                }

                // Validate and parse body
                if (string.IsNullOrWhiteSpace(body))
                {
                    BadRequest(response, "Request body is required");
                    return;
                }

                ReceiveData codeData;
                try
                {
                    codeData = JsonConvert.DeserializeObject<ReceiveData>(body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CodeController] JSON parse error: {ex.Message}");
                    BadRequest(response, "Invalid JSON format");
                    return;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(codeData?.code))
                {
                    BadRequest(response, "Missing required field: code");
                    return;
                }

                if (string.IsNullOrWhiteSpace(codeData?.line_name))
                {
                    BadRequest(response, "Missing required field: line_name");
                    return;
                }

                // Process the QR code data
                OnQRCodeReceived(new QRCodeReceivedEventArgs
                {
                    Code = codeData.code,
                    LineIP = codeData.line_IP,
                    LineName = codeData.line_name,
                    ReceivedAt = DateTime.Now,
                    AuthenticatedUser = username
                });

                Console.WriteLine($"[CodeController] ✓ QR Code received: {codeData.code} for line: {codeData.line_name}");

                // Return success response
                Ok(response, new ResponseData
                {
                    is_success = true,
                    message = "Thành công"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CodeController] ✗ Unexpected error: {ex.Message}");
                InternalServerError(response, "Code processing failed", ex);
            }
        }

        protected virtual void OnQRCodeReceived(QRCodeReceivedEventArgs e)
        {
            QRCodeReceived?.Invoke(this, e);
        }
    }

    public class QRCodeReceivedEventArgs : EventArgs
    {
        public string Code { get; set; }
        public string LineIP { get; set; }
        public string LineName { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string AuthenticatedUser { get; set; }
    }
}
