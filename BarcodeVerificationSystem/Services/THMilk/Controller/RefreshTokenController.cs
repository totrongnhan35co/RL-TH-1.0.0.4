using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BarcodeVerificationSystem.Services.THMilk.Model.ReceiveModel;
using BarcodeVerificationSystem.Services.THMilk.Model.ResponseModel;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace BarcodeVerificationSystem.Services.THMilk.Controller
{
    /// <summary>
    /// Refresh Token API Controller - handles /api/refresh-token endpoint
    /// </summary>
    public class RefreshTokenController : BaseController
    {
        private const string JWT_SECRET = "THMilkSecretKeyForJWTTokenGeneration2026";
        private const int TOKEN_EXPIRY_SECONDS = 60;

        /// <summary>
        /// POST /api/refresh-token
        /// Handles token refresh and returns new JWT tokens
        /// </summary>
        public void HandleRefreshToken(HttpListenerRequest request, HttpListenerResponse response, string body)
        {
            try
            {
                // Validate HTTP method
                if (!ValidateMethod(request, response, "POST"))
                    return;

                Console.WriteLine($"[RefreshTokenController] Processing refresh token request");

                // Validate and parse body
                if (string.IsNullOrWhiteSpace(body))
                {
                    BadRequest(response, "Request body is required");
                    return;
                }

                ReceiveRefreshToken refreshData;
                try
                {
                    refreshData = JsonConvert.DeserializeObject<ReceiveRefreshToken>(body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RefreshTokenController] JSON parse error: {ex.Message}");
                    BadRequest(response, "Invalid JSON format");
                    return;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(refreshData?.username) || 
                    string.IsNullOrWhiteSpace(refreshData?.refresh_token))
                {
                    BadRequest(response, "Username and refresh_token are required");
                    return;
                }

                // Validate the refresh token
                if (!ValidateRefreshToken(refreshData.refresh_token, refreshData.username, out string error))
                {
                    Console.WriteLine($"[RefreshTokenController] Token validation failed: {error}");
                    Unauthorized(response, error ?? "Invalid refresh token");
                    return;
                }

                // Generate new tokens
                var accessToken = GenerateToken(refreshData.username, "THTRUEMILK", "access");
                var newRefreshToken = GenerateToken(refreshData.username, "THTRUEMILK", "refresh");

                var tokenResponse = new ResponseLogin
                {
                    access_token = accessToken,
                    refresh_token = newRefreshToken
                };

                Console.WriteLine($"[RefreshTokenController] ✓ Token refreshed for user: {refreshData.username}");

                // Return success response
                Ok(response, tokenResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RefreshTokenController] ✗ Unexpected error: {ex.Message}");
                InternalServerError(response, "Token refresh failed", ex);
            }
        }

        private bool ValidateRefreshToken(string token, string expectedUsername, out string error)
        {
            error = null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(JWT_SECRET);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var username = jwtToken.Claims.FirstOrDefault(x => x.Type == "username")?.Value;
                var tokenType = jwtToken.Claims.FirstOrDefault(x => x.Type == "type")?.Value;

                // Verify it's a refresh token
                if (tokenType != "refresh")
                {
                    error = "Token is not a refresh token";
                    return false;
                }

                // Verify username matches
                if (username != expectedUsername)
                {
                    error = "Token username does not match";
                    return false;
                }

                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                error = "Refresh token has expired";
                return false;
            }
            catch (Exception ex)
            {
                error = $"Token validation failed: {ex.Message}";
                return false;
            }
        }

        private string GenerateToken(string username, string role, string tokenType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(JWT_SECRET);
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim("username", username),
                new Claim("role", role),
                new Claim("time", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())
            };

            if (tokenType == "refresh")
            {
                claims.Add(new Claim("type", "refresh"));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = now.AddSeconds(TOKEN_EXPIRY_SECONDS),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
