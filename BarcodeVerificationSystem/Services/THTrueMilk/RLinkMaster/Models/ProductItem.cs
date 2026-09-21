using System.Collections.Generic;
using Newtonsoft.Json;

namespace BarcodeVerificationSystem.Services.THTrueMilk.RLinkMaster.Models
{
    public class ProductItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        [JsonProperty("product_gtin")]
        public string ProductGtin { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
        public int? Volume { get; set; }
        public int? Exp { get; set; }
    }

    // ── Wrapper responses (GET endpoints dạng { success, message, total, data }) ──

    internal class ProductListResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int total { get; set; }
        public List<ProductItem> data { get; set; }
    }

    internal class LineListResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int total { get; set; }
        public List<LineInfo> data { get; set; }
    }

    internal class SettingsResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public RLinkSettings data { get; set; }
    }

    internal class AccountListResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int total { get; set; }
        public List<AccountInfo> data { get; set; }
    }

    public class QrConfig
    {
        [JsonProperty("base_url")]
        public string BaseUrl { get; set; }
        [JsonProperty("number_of_url")]
        public int NumberOfUrl { get; set; }
    }

    internal class QrConfigResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public QrConfig data { get; set; }
    }
}