using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Controller
{
    public abstract class BaseController
    {
        protected void Ok(HttpListenerResponse response, object data)
            => SendJsonResponse(response, data, 200);

        protected void BadRequest(HttpListenerResponse response, string message)
            => SendJsonResponse(response, new { error = message }, 400);

        protected void Unauthorized(HttpListenerResponse response, string message)
            => SendJsonResponse(response, new { error = message }, 401);

        protected void NotFound(HttpListenerResponse response, string message)
            => SendJsonResponse(response, new { error = message }, 404);

        protected void MethodNotAllowed(HttpListenerResponse response)
            => SendJsonResponse(response, new { error = "Method not allowed" }, 405);

        protected void UnprocessableEntity(HttpListenerResponse response, string message)
            => SendJsonResponse(response, new { error = message }, 422);

        protected void InternalServerError(HttpListenerResponse response, string message, Exception ex = null)
            => SendJsonResponse(response, new { error = message, detail = ex?.Message }, 500);

        protected bool ValidateMethod(HttpListenerRequest request, HttpListenerResponse response, string method)
        {
            if (!request.HttpMethod.Equals(method, StringComparison.OrdinalIgnoreCase))
            {
                MethodNotAllowed(response);
                return false;
            }
            return true;
        }

        protected void SendJsonResponse(HttpListenerResponse response, object data, int statusCode = 200)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data);
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                response.StatusCode = statusCode;
                response.ContentType = "application/json; charset=utf-8";
                response.ContentLength64 = buffer.Length;
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET,POST,OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type,Authorization");

                using (var stream = response.OutputStream)
                    stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BaseController] SendJsonResponse error: {ex.Message}");
            }
        }
    }
}