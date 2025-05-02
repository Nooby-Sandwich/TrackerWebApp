using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TrackerWebApp.Data;
using TrackerWebApp.Models;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1) load all budgets & expenses (for charts)
        var budgets = await _context.Budgets
            .Where(b => b.UserId == userId)
            .ToListAsync();

        var allExpenses = await _context.Expenses
            .Where(e => e.UserId == userId)
            .Include(e => e.Budget)
            .ToListAsync();

        // 2) compute monthly totals (for the top cards & chart1)
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var thisMonthSpent = allExpenses
            .Where(e => e.Date >= firstOfMonth)
            .Sum(e => e.Amount);

        var monthlyBudget = budgets.Sum(b => b.Limit);

        // 3) compute six-month series (for the 2×2 grid charts)
        var budgetVsActual = budgets
            .Select(b => new CategoryBudgetActual(
                b.Name,
                b.Limit,
                allExpenses.Where(e => e.BudgetId == b.BudgetId).Sum(e => e.Amount)
            )).ToList();

        var byCategory = allExpenses
            .GroupBy(e => e.Budget?.Name ?? "Uncategorized")
            .Select(g => new CategorySpend(g.Key, g.Sum(e => e.Amount)))
            .ToList();

        var monthlyTrend = allExpenses
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new MonthlySpend(g.Key.Year, g.Key.Month, g.Sum(e => e.Amount)))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        // 4) build ViewModel
        var totals = new FinancialTotals
        {
            TotalBudgeted = monthlyBudget,
            TotalSpent = thisMonthSpent,
            TotalBudgets = budgets.Count,
            TotalExpenses = allExpenses.Count
        };

        var vm = new DashboardViewModel
        {
            Totals = totals,
            ChartData = new ChartData
            {
                BudgetVsActual = budgetVsActual,
                ByCategory = byCategory,
                MonthlyTrend = monthlyTrend
            },
            PinnedCharts = new() { "ByCategory", "MonthlyTrend", "BudgetVsActual" }
        };

        return View(vm);
    }
}
