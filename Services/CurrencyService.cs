using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TrackerWebApp.Models;

namespace TrackerWebApp
    .Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _http;

        public CurrencyService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Currency>> GetAllCurrenciesAsync()
        {
            // Attempt to fetch and bind the symbols response
            var resp = await _http.GetFromJsonAsync<SymbolsResponse>(
                "symbols",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (resp == null)
            {
                // remote host didn’t return valid JSON at all
                Debug.WriteLine("[CurrencyService] symbols response was null");
                return new List<Currency>();
            }

            if (resp.Symbols == null || !resp.Success)
            {
                // symbols failed or returned null—fallback gracefully
                Debug.WriteLine($"[CurrencyService] symbols endpoint failed (success={resp.Success}); falling back to default list");

                // Option A: return an empty list
                // return new List<Currency>();

                // Option B: return a minimal hard-coded set so the dropdown still shows something:
                // inside CurrencyService.GetAllCurrenciesAsync fallback block…
return new List<Currency> {
    new Currency { Code = "USD", Name = "United States Dollar" },
    new Currency { Code = "EUR", Name = "Euro" },
    new Currency { Code = "GBP", Name = "British Pound" },
    new Currency { Code = "INR", Name = "Indian Rupee" },
    new Currency { Code = "JPY", Name = "Japanese Yen" },
    new Currency { Code = "CNY", Name = "Chinese Yuan" },
    new Currency { Code = "BRL", Name = "Brazilian Real" },
    new Currency { Code = "RUB", Name = "Russian Ruble" },
    new Currency { Code = "ZAR", Name = "South African Rand" },
    new Currency { Code = "EGP", Name = "Egyptian Pound" },
    new Currency { Code = "ETB", Name = "Ethiopian Birr" },
    new Currency { Code = "IDR", Name = "Indonesian Rupiah" },
    new Currency { Code = "IRR", Name = "Iranian Rial" },
    new Currency { Code = "AED", Name = "UAE Dirham" }
};
            }

            // Normal path: build from API data
            var list = new List<Currency>();
            foreach (var kv in resp.Symbols)
            {
                list.Add(new Currency
                {
                    Code = kv.Key,
                    Name = kv.Value.Description
                });
            }
            return list;
        }


        public async Task<decimal> ConvertAsync(string from, string to, decimal amount)
        {
            var url = $"convert?from={from}&to={to}&amount={amount}";
            using var doc = await _http.GetFromJsonAsync<JsonDocument>(url);

            if (doc == null)
                throw new InvalidOperationException("convert endpoint returned null JSON");

            if (!doc.RootElement.TryGetProperty("result", out var result))
            {
                Debug.WriteLine("[CurrencyService] Unexpected response: " + doc.RootElement.ToString());
                throw new InvalidOperationException("convert endpoint did not return `result`");
            }

            return result.GetDecimal();
        }

    }
}
