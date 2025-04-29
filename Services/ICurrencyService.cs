using System.Collections.Generic;
using System.Threading.Tasks;
using TrackerWebApp.Models;

namespace TrackerWebApp.Services
{
    public interface ICurrencyService
    {
        Task<List<Currency>> GetAllCurrenciesAsync();
        Task<decimal> ConvertAsync(string from, string to, decimal amount);
    }
}