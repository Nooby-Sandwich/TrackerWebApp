// File: Controllers/HomeController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackerWebApp.Data;
using TrackerWebApp.Models;

namespace TrackerWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: /
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = new DashboardViewModel
                {
                    Totals = await GetFinancialTotalsAsync(),
                    ChartData = await GetChartDataAsync()
                };

                SetChartViewData(dashboardData.ChartData);
                ViewBag.BudgetsList = await _context.Budgets.OrderBy(b => b.Name).ToListAsync();

                return View(dashboardData);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading dashboard data";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: /Home/Setup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup([FromForm] decimal salary, [FromForm] string preset)
        {
            if (salary <= 0)
            {
                TempData["SetupError"] = "Invalid salary amount";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                await ClearBudgetsAsync();
                await CreateBudgetsAsync(salary, preset);
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Budget setup completed successfully";
            }
            catch (Exception)
            {
                TempData["SetupError"] = "Error configuring budgets";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Private Methods

        private async Task<FinancialTotals> GetFinancialTotalsAsync()
        {
            return new FinancialTotals
            {
                TotalBudgets = await _context.Budgets.CountAsync(),
                TotalExpenses = await _context.Expenses.CountAsync(),
                TotalBudgeted = await _context.Budgets.SumAsync(b => (decimal?)b.Limit) ?? 0m,
                TotalSpent = await _context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0m
            };
        }

        private async Task<ChartData> GetChartDataAsync()
        {
            var budgetExpenses = await GetBudgetExpensesAsync();
            var monthlyExpenses = await GetMonthlyExpensesAsync();
            var comparisons = GetBudgetVsActual(budgetExpenses);

            return new ChartData
            {
                ByBudget = budgetExpenses,
                Monthly = monthlyExpenses,
                BudgetVsActual = comparisons
            };
        }

        private async Task<List<BudgetExpense>> GetBudgetExpensesAsync()
        {
            return await _context.Budgets
                .Select(b => new BudgetExpense(b.Name, b.Expenses.Sum(e => e.Amount)))
                .ToListAsync();
        }

        private async Task<List<MonthlyExpense>> GetMonthlyExpensesAsync()
        {
            var sixMonthsAgo = DateTime.Today.AddMonths(-5);
            var startDate = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            return await _context.Expenses
                .Where(e => e.Date >= startDate)
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyExpense(g.Key.Year, g.Key.Month, g.Sum(e => e.Amount)))
                .ToListAsync();
        }

        private List<BudgetComparison> GetBudgetVsActual(IEnumerable<BudgetExpense> budgetExpenses)
        {
            return budgetExpenses.Select(b =>
            {
                var limit = _context.Budgets.FirstOrDefault(x => x.Name == b.Category)?.Limit ?? 0m;
                return new BudgetComparison(b.Category, limit, b.TotalSpent);
            }).ToList();
        }

        private void SetChartViewData(ChartData chartData)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            ViewData["ByBudgetJson"] = JsonSerializer.Serialize(chartData.ByBudget, options);
            ViewData["MonthlyJson"] = JsonSerializer.Serialize(chartData.Monthly, options);
            ViewData["BudgetVsActualJson"] = JsonSerializer.Serialize(chartData.BudgetVsActual, options);
        }

        private async Task ClearBudgetsAsync()
        {
            var budgets = await _context.Budgets.ToListAsync();
            _context.Budgets.RemoveRange(budgets);
            await _context.SaveChangesAsync();
        }

        private async Task CreateBudgetsAsync(decimal salary, string presetKey)
        {
            var preset = PresetProfile.GetPreset(presetKey);
            var newBudgets = preset.CalculateBudgets(salary);
            await _context.Budgets.AddRangeAsync(newBudgets);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
