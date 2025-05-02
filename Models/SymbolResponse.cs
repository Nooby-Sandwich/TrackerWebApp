// Models/SymbolsResponse.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrackerWebApp.Models
{
    public class SymbolsResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("symbols")]
        public Dictionary<string, CurrencyDetail>? Symbols { get; set; }
    }

    public class CurrencyDetail
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }
}
