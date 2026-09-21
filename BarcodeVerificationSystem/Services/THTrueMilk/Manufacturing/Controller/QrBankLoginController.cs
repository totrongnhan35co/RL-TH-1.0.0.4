using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ReceiveModel;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ResponseModel;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Controller
{
    public class QrBankLoginController : BaseController
    {
        private const string SecretKey = "NUzfz%sqL%j&0sTMfi2oYPWnTlV8wYW6TJcIssOclIVTQpG";
        internal const string JwtSecret = "QrBankTHTrueMilkSecretKeyJWT2026!"; // >= 32 ký tự
        private const int TokenExpirySeconds = 3600; // 1 giờ

        private static readonly Dictionary<string, string> ValidCredentials = new Dictionary<string, string>
        {
            { "qrbank", "qrbank@#062026" }
        };

        /// <summary>POST /api/qrbank/login</summary>
        public void HandleLogin(HttpListenerRequest request, HttpListenerResponse response, string body)
        {
            try
            {
                if (!ValidateMethod(request, response, "POST")) return;

                if (string.IsNullOrWhiteSpace(body))
                { BadRequest(response, "Request body is required"); return; }

                QrBankReceiveLogin loginData;
                try { loginData = JsonConvert.DeserializeObject<QrBankReceiveLogin>(body); }
                catch { BadRequest(response, "Invalid JSON format"); return; }

                if (string.IsNullOrWhiteSpace(loginData?.username) || string.IsNullOrWhiteSpace(loginData?.password))
                { BadRequest(response, "username and password are required"); return; }

                if (string.IsNullOrWhiteSpace(loginData.secret_key))
                { BadRequest(response, "secret_key is required"); return; }

                if (loginData.secret_key != SecretKey)
                { BadRequest(response, "secret_key is incorrect"); return; }

                if (!ValidCredentials.TryGetValue(loginData.username, out string pwd) || pwd != loginData.password)
                { Unauthorized(response, "Invalid username or password"); return; }

                Ok(response, new QrBankResponseLogin
                {
                    access_token = GenerateToken(loginData.username, "access"),
                    refresh_token = GenerateToken(loginData.username, "refresh")
                });
                Console.WriteLine($"[QrBankLoginController] ✓ Login: {loginData.username}");
            }
            catch (Exception ex)
            {
                InternalServerError(response, "Login failed", ex);
            }
        }

        private static string GenerateToken(string username, string tokenType)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(JwtSecret);

            var claims = new List<Claim>
            {
                new Claim("username", username),
                new Claim("role",     "QRBANK"),
                new Claim("type",     tokenType),
                new Claim("iat",      DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(TokenExpirySeconds),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        public static bool ValidateToken(string token, out string username, out string error)
        {
            username = null;
            error = null;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(JwtSecret);

                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validated);

                var jwt = (JwtSecurityToken)validated;
                username = jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value;

                // Từ chối refresh token dùng làm access token
                string type = jwt.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
                if (type == "refresh")
                { error = "Cannot use refresh token as access token"; return false; }

                return true;
            }
            catch (SecurityTokenExpiredException)
            { error = "Token has expired"; return false; }
            catch (Exception ex)
            { error = $"Token validation failed: {ex.Message}"; return false; }
        }
    }
}