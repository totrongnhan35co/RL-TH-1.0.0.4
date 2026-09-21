using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BarcodeVerificationSystem.Services.THMilk.Controller
{
    public abstract class BaseController
    {
        /// <summary>
        /// Send JSON response with status code (like ASP.NET Web API)
        /// </summary>
        protected void Ok(HttpListenerResponse response, object data)
        {
            SendJsonResponse(response, data, 200);
        }

        /// <summary>
        /// Send bad request response (400)
        /// </summary>
        protected void BadRequest(HttpListenerResponse response, string message)
        {
            SendJsonResponse(response, new { error = message }, 400);
        }

        /// <summary>
        /// Send unauthorized response (401)
        /// </summary>
        protected void Unauthorized(HttpListenerResponse response, string message)
        {
            SendJsonResponse(response, new { error = message }, 401);
        }

        /// <summary>
        /// Send not found response (404)
        /// </summary>
        protected void NotFound(HttpListenerResponse response, string message)
        { 
            SendJsonResponse(response, new { error = message }, 404);
        }

        /// <summary>
        /// Send method not allowed response (405)
        /// </summary>
        protected void MethodNotAllowed(HttpListenerResponse response)
        {
            SendJsonResponse(response, new { error = "Method not allowed" }, 405);
        }

        /// <summary>
        /// Send internal server error response (500)
        /// </summary>
        protected void InternalServerError(HttpListenerResponse response, string message, Exception ex = null)
        {
            var errorResponse = new
            {
                error = message,
                message = ex?.Message,
                details = ex?.StackTrace
            };
            SendJsonResponse(response, errorResponse, 500);
        }

        /// <summary>
        /// Send JSON response with custom status code
        /// </summary>
        protected void SendJsonResponse(HttpListenerResponse response, object data, int statusCode = 200)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data);
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                response.StatusCode = statusCode;
                response.ContentType = "application/json; charset=utf-8";
                response.ContentLength64 = buffer.Length;

                // Add CORS headers
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

                using (var output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BaseController] Error sending response: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate request method
        /// </summary>
        protected bool ValidateMethod(HttpListenerRequest request, HttpListenerResponse response, string expectedMethod)
        {
            if (request.HttpMethod != expectedMethod)
            {
                MethodNotAllowed(response);
                return false;
            }
            return true;
        }
    }
}
