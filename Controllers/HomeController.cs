using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TrackerWebApp.Data;
using TrackerWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace TrackerWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            // Prepare dashboard essentials
            var totals = new FinancialTotals
            {
                TotalBudgets = await _context.Budgets.CountAsync(),
                TotalExpenses = await _context.Expenses.CountAsync(),
                TotalBudgeted = await _context.Budgets.SumAsync(b => (decimal?)b.Limit) ?? 0m,
                TotalSpent = await _context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0m
            };

            ViewBag.Budgets = await _context.Budgets.OrderBy(b => b.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetCurrency(string currency, string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(currency))
            {
                Response.Cookies.Append("SelectedCurrency", currency, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    IsEssential = true
                });
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
