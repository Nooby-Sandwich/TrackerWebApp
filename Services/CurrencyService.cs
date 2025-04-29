
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TrackerWebApp.Models;

namespace TrackerWebApp.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _http;
        private const string ApiKey = "YOUR_API_KEY"; // configure in appsettings
        private const string BaseUrl = "https://api.exchangerate.host";

        public CurrencyService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Currency>> GetAllCurrenciesAsync()
        {
            // Example using exchangerate.host symbols endpoint
            var res = await _http.GetStringAsync($"{BaseUrl}/symbols");
            Debug.WriteLine(res);

            using var doc = JsonDocument.Parse(res);
            if (!doc.RootElement.TryGetProperty("symbols", out var symbolsObj))
                throw new InvalidOperationException("API response did not return a `symbols` object");

            var list = new List<Currency>();
            foreach (var prop in symbolsObj.EnumerateObject())
            {
                var desc = prop.Value.GetProperty("description").GetString() ?? prop.Name;
                list.Add(new Currency { Code = prop.Name, Name = desc });
            }
            return list;

        }

        public async Task<decimal> ConvertAsync(string from, string to, decimal amount)
        {
            var res = await _http.GetStringAsync($"{BaseUrl}/convert?from={from}&to={to}&amount={amount}");
            using var doc = JsonDocument.Parse(res);
            return doc.RootElement.GetProperty("result").GetDecimal();
        }
    }
}