using Microsoft.AspNetCore.Authorization;
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

    [Authorize] // require login to view dashboards
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1) Load budgets & expenses for this user
        var budgets = await _context.Budgets
            .Where(b => b.UserId == userId)
            .ToListAsync();
        var expenses = await _context.Expenses
            .Where(e => e.UserId == userId)
            .ToListAsync();

        // 2) Compute totals
        var totals = new FinancialTotals
        {
            TotalBudgeted = budgets.Sum(b => b.Limit),
            TotalSpent = expenses.Sum(e => e.Amount),
            TotalBudgets = budgets.Count,
            TotalExpenses = expenses.Count
        };

        // 3) Chart data
        var chartData = new ChartData
        {
            BudgetVsActual = budgets.Select(b => new CategoryBudgetActual(
                b.Name,
                b.Limit,
                expenses.Where(e => e.BudgetId == b.BudgetId).Sum(e => e.Amount)
            )).ToList(),
            ByCategory = expenses
                .GroupBy(e => budgets.FirstOrDefault(b => b.BudgetId == e.BudgetId)?.Name ?? "Uncategorized")
                .Select(g => new CategorySpend(g.Key, g.Sum(e => e.Amount)))
                .ToList(),
            MonthlyTrend = expenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new MonthlySpend(g.Key.Year, g.Key.Month, g.Sum(e => e.Amount)))
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList()
        };

        // 4) Build VM
        var vm = new DashboardViewModel
        {
            Totals = totals,
            ChartData = chartData,
            PinnedCharts = new List<string> { "ByCategory", "MonthlyTrend", "BudgetVsActual" }
        };

        // 5) Also pass raw budgets for your quick-add dropdown
        ViewBag.Budgets = budgets;

        return View(vm);
    }
}
