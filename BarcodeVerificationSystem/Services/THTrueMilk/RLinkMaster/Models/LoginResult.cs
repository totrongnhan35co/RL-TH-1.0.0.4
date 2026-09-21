using Newtonsoft.Json;
using System.Collections.Generic;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }

        [JsonProperty("resfreshToken")]
        public string RefreshToken { get; set; }

        public int TokenExpiresIn { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        public Dictionary<string, bool> Permissions { get; set; } = new Dictionary<string, bool>();
    }
}