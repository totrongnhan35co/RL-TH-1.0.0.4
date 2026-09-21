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
    /// Login API Controller - handles /api/login endpoint
    /// </summary>
    public class LoginController : BaseController
    {
        private const string SECRET_KEY = "NUzfz%sqL%j&0sTMfi2oYPWnTlV8wYW6TJcIssOclIVTQpG";
        private const string JWT_SECRET = "THMilkSecretKeyForJWTTokenGeneration2026"; // Should be at least 32 characters
        private const int TOKEN_EXPIRY_SECONDS = 60;

        // Store valid credentials (in production, this should be in a database)
        private static Dictionary<string, string> _validCredentials = new Dictionary<string, string>
        {
            { "thtruemilk", "thtruemilk@#062026" }
        };

        /// <summary>
        /// POST /api/login
        /// Handles login request and returns JWT tokens
        /// </summary>
        public void HandleLogin(HttpListenerRequest request, HttpListenerResponse response, string body)
        {
            try
            {
                // Validate HTTP method
                if (!ValidateMethod(request, response, "POST"))
                    return;

                Console.WriteLine($"[LoginController] Processing login request");
                Console.WriteLine($"[LoginController] Body: {body}");

                // Validate and parse body
                if (string.IsNullOrWhiteSpace(body))
                {
                    BadRequest(response, "Request body is required");
                    return;
                }

                ReceiveLogin loginData;
                try
                {
                    loginData = JsonConvert.DeserializeObject<ReceiveLogin>(body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LoginController] JSON parse error: {ex.Message}");
                    BadRequest(response, "Invalid JSON format");
                    return;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(loginData?.username) || string.IsNullOrWhiteSpace(loginData?.password))
                {
                    BadRequest(response, "Username and password are required");
                    return;
                }

                if (string.IsNullOrWhiteSpace(loginData?.secret_key))
                {
                    BadRequest(response, "Secret Key are required");
                    return;
                }

                if (SECRET_KEY != loginData?.secret_key)
                {
                    BadRequest(response, "Secret Key is incorrect");
                    return;
                }


                // Validate credentials
                if (!_validCredentials.ContainsKey(loginData.username) || 
                    _validCredentials[loginData.username] != loginData.password)
                {
                    Console.WriteLine($"[LoginController] Invalid credentials for user: {loginData.username}");
                    Unauthorized(response, "Invalid username or password");
                    return;
                }

                // Generate tokens
                var accessToken = GenerateToken(loginData.username, "THTRUEMILK", "access");
                var refreshToken = GenerateToken(loginData.username, "THTRUEMILK", "refresh");

                var loginResponse = new ResponseLogin
                {
                    access_token = accessToken,
                    refresh_token = refreshToken
                };

                Console.WriteLine($"[LoginController] ✓ Login successful for user: {loginData.username}");
                Console.WriteLine($"[LoginController] Token generated (length: {accessToken?.Length})");

                // Return success response
                Ok(response, loginResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginController] ✗ Unexpected error: {ex.Message}");
                Console.WriteLine($"[LoginController] Stack: {ex.StackTrace}");
                InternalServerError(response, "Login failed", ex);
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

        public static bool ValidateToken(string token, out string username, out string error)
        {
            username = null;
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
                username = jwtToken.Claims.First(x => x.Type == "username").Value;

                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                error = "Token has expired";
                return false;
            }
            catch (Exception ex)
            {
                error = $"Token validation failed: {ex.Message}";
                return false;
            }
        }
    }
}
