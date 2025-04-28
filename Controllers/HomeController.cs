using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackerWebApp.Data;

namespace TrackerWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context) => _context = context;

        // GET: /
        public async Task<IActionResult> Index()
        {
            // Totals
            ViewData["TotalBudgets"] = await _context.Budgets.CountAsync();
            ViewData["TotalExpenses"] = await _context.Expenses.CountAsync();
            ViewData["TotalSpent"] = await _context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0m;
            ViewData["TotalBudgeted"] = await _context.Budgets.SumAsync(b => (decimal?)b.Limit) ?? 0m;

            // Pie: spending by budget
            var byBudget = await _context.Budgets
                .Select(b => new {
                    b.Name,
                    Total = (decimal?)b.Expenses.Sum(e => e.Amount) ?? 0m
                }).ToListAsync();
            ViewBag.ByBudgetJson = System.Text.Json.JsonSerializer.Serialize(byBudget);

            // Bar: monthly expenses (last 6 months)
            var sixMoAgo = DateTime.Today.AddMonths(-5);
            var monthly = await _context.Expenses
                .Where(e => e.Date >= new DateTime(sixMoAgo.Year, sixMoAgo.Month, 1))
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(e => e.Amount)
                }).ToListAsync();
            ViewBag.MonthlyJson = System.Text.Json.JsonSerializer.Serialize(monthly);

            // Bar: budget vs actual in a grouped bar
            var budVsAct = byBudget
                .Select(b => new {
                    b.Name,
                    Budgeted = (decimal?)_context.Budgets
                                   .Where(x => x.Name == b.Name)
                                   .Select(x => x.Limit)
                                   .FirstOrDefault() ?? 0m,
                    Spent = b.Total
                }).ToList();
            ViewBag.BudVsActJson = System.Text.Json.JsonSerializer.Serialize(budVsAct);

            return View();
        }
    }
}