// File: Controllers/HomeController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackerWebApp.Data;
using TrackerWebApp.Models;
using System.Text.Json;

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
                // Create the view model using external types
                var dashboardData = new DashboardViewModel
                {
                    Totals = await GetFinancialTotals(),
                    ChartData = await GetChartData()
                };

                AddViewDataForCharts(dashboardData.ChartData);
                ViewBag.BudgetsList = await _context.Budgets.OrderBy(b => b.Name).ToListAsync();
                return View(dashboardData);
            }
            catch
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
                using var transaction = await _context.Database.BeginTransactionAsync();
                await ClearExistingBudgets();
                await CreateNewBudgets(salary, preset);
                await transaction.CommitAsync();
                TempData["SuccessMessage"] = "Budget setup completed successfully";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["SetupError"] = "Error configuring budgets";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Private Methods
        private async Task<FinancialTotals> GetFinancialTotals()
        {
            return new FinancialTotals
            {
                TotalBudgets = await _context.Budgets.CountAsync(),
                TotalExpenses = await _context.Expenses.CountAsync(),
                TotalBudgeted = await _context.Budgets.SumAsync(b => (decimal?)b.Limit) ?? 0m,
                TotalSpent = await _context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0m
            };
        }

        private async Task<ChartData> GetChartData()
        {
            var byBudget = await GetBudgetExpenses();
            var monthly = await GetMonthlyExpenses();
            var budgetVsActual = CalculateBudgetVsActual(byBudget);

            return new ChartData
            {
                ByBudget = byBudget,
                Monthly = monthly,
                BudgetVsActual = budgetVsActual
            };
        }

        private async Task<List<BudgetExpense>> GetBudgetExpenses()
            => await _context.Budgets
                .Select(b => new BudgetExpense(b.Name, b.Expenses.Sum(e => e.Amount)))
                .ToListAsync();

        private async Task<List<MonthlyExpense>> GetMonthlyExpenses()
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

        private List<BudgetComparison> CalculateBudgetVsActual(List<BudgetExpense> budgetExpenses)
            => budgetExpenses
                .Select(b => new BudgetComparison(
                    b.Category,
                    _context.Budgets.FirstOrDefault(x => x.Name == b.Category)?.Limit ?? 0m,
                    b.TotalSpent))
                .ToList();

        private void AddViewDataForCharts(ChartData chartData)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            ViewData["ByBudgetJson"] = JsonSerializer.Serialize(chartData.ByBudget, options);
            ViewData["MonthlyJson"] = JsonSerializer.Serialize(chartData.Monthly, options);
            ViewData["BudgetVsActualJson"] = JsonSerializer.Serialize(chartData.BudgetVsActual, options);
        }

        private async Task ClearExistingBudgets()
        {
            var existing = await _context.Budgets.ToListAsync();
            _context.Budgets.RemoveRange(existing);
            await _context.SaveChangesAsync();
        }

        private async Task CreateNewBudgets(decimal salary, string presetKey)
        {
            var preset = PresetProfile.GetPreset(presetKey);
            var budgets = preset.CalculateBudgets(salary);
            await _context.Budgets.AddRangeAsync(budgets);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
