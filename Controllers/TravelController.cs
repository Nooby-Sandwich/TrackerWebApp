using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackerWebApp.Models;
using TrackerWebApp.Services;

namespace TrackerWebApp.Controllers
{
    public class TravelController : Controller
    {
        private readonly ICurrencyService _currencyService;

        public TravelController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        // GET: /Travel
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new TravelViewModel
            {
                Currencies = await _currencyService.GetAllCurrenciesAsync()
            };
            return View(vm);
        }

        // POST: /Travel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TravelViewModel vm)
        {
            // always reload the list so the dropdown stays populated
            vm.Currencies = await _currencyService.GetAllCurrenciesAsync();

            if (!string.IsNullOrWhiteSpace(vm.FromCurrency)
                && !string.IsNullOrWhiteSpace(vm.ToCurrency)
                && vm.Amount > 0)
            {
                vm.ConvertedAmount = await _currencyService
                    .ConvertAsync(vm.FromCurrency, vm.ToCurrency, vm.Amount);
            }

            return View(vm);
        }
    }
}
