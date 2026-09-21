using System;
using System.Net;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ReceiveModel;
using BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Model.ResponseModel;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;

namespace BarcodeVerificationSystem.Services.THTrueMilk.Manufacturing.Controller
{
    public class QrBankRefreshTokenController : BaseController
    {
        private const int NewTokenExpirySeconds = 3600;

        /// <summary>POST /api/qrbank/refresh-token</summary>
        public void HandleRefreshToken(HttpListenerRequest request, HttpListenerResponse response, string body)
        {
            try
            {
                if (!ValidateMethod(request, response, "POST")) return;

                if (string.IsNullOrWhiteSpace(body))
                { BadRequest(response, "Request body is required"); return; }

                QrBankReceiveRefreshToken data;
                try { data = JsonConvert.DeserializeObject<QrBankReceiveRefreshToken>(body); }
                catch { BadRequest(response, "Invalid JSON format"); return; }

                if (string.IsNullOrWhiteSpace(data?.refresh_token))
                { BadRequest(response, "refresh_token is required"); return; }

                // Xác thực refresh token
                if (!ValidateRefreshToken(data.refresh_token, out string username, out string error))
                { Unauthorized(response, error ?? "Invalid refresh token"); return; }

                string newAccessToken = GenerateAccessToken(username);
                string newRefreshToken = GenerateRefreshToken(username);

                Ok(response, new QrBankResponseLogin
                {
                    access_token = newAccessToken,
                    refresh_token = newRefreshToken
                });
                Console.WriteLine($"[QrBankRefreshTokenController] ✓ Refreshed token for: {username}");
            }
            catch (Exception ex)
            {
                InternalServerError(response, "Refresh token failed", ex);
            }
        }

        private static bool ValidateRefreshToken(string token, out string username, out string error)
        {
            username = null;
            error = null;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(QrBankLoginController.JwtSecret);

                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                var jwt = handler.ReadJwtToken(token);
                string t = jwt.Claims.FirstOrDefault(c => c.Type == "type")?.Value;

                if (t != "refresh")
                { error = "Token is not a refresh token"; return false; }

                username = jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                return true;
            }
            catch (SecurityTokenExpiredException)
            { error = "Refresh token has expired"; return false; }
            catch (Exception ex)
            { error = $"Token validation failed: {ex.Message}"; return false; }
        }

        private static string GenerateAccessToken(string username)
            => GenerateToken(username, "access");

        private static string GenerateRefreshToken(string username)
            => GenerateToken(username, "refresh");

        private static string GenerateToken(string username, string tokenType)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(QrBankLoginController.JwtSecret);

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
                Expires = DateTime.UtcNow.AddSeconds(NewTokenExpirySeconds),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            return handler.WriteToken(handler.CreateToken(descriptor));
        }
    }
}