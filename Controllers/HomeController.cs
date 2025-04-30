using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        var budgets = await _context.Budgets
            .Where(b => b.UserId == userId)
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => e.UserId == userId)
            .Include(e => e.Budget)
            .ToListAsync();

        var totals = new FinancialTotals
        {
            TotalBudgeted = budgets.Sum(b => b.Limit),
            TotalSpent = expenses.Sum(e => e.Amount),
            TotalBudgets = budgets.Count,
            TotalExpenses = expenses.Count
        };

        // FIXED: Changed b.Id to b.BudgetId
        var budgetVsActual = budgets.Select(b => new CategoryBudgetActual(
            b.Name,
            b.Limit,
            expenses.Where(e => e.BudgetId == b.BudgetId).Sum(e => e.Amount)
        )).ToList();

        var byCategory = expenses
            .GroupBy(e => e.Budget?.Name ?? "Uncategorized")
            .Select(g => new CategorySpend(g.Key, g.Sum(e => e.Amount)))
            .ToList();

        var monthlyTrend = expenses
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new MonthlySpend(g.Key.Year, g.Key.Month, g.Sum(e => e.Amount)))
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .ToList();

        var model = new DashboardViewModel
        {
            Totals = totals,
            ChartData = new ChartData
            {
                BudgetVsActual = budgetVsActual,
                ByCategory = byCategory,
                MonthlyTrend = monthlyTrend
            },
            PinnedCharts = new List<string> { "ByCategory", "MonthlyTrend", "BudgetVsActual" }
        };

        return View(model);
    }
}