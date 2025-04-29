// File: TrackerWebApp/Models/DashboardViewModel.cs
using System;
using System.Collections.Generic;

namespace TrackerWebApp.Models
{
    public class DashboardViewModel
    {
        public FinancialTotals Totals { get; set; } = new FinancialTotals();
        public ChartData ChartData { get; set; } = new ChartData();
    }

    public class FinancialTotals
    {
        public int TotalBudgets { get; set; }
        public int TotalExpenses { get; set; }
        public decimal TotalBudgeted { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class ChartData
    {
        public List<BudgetExpense> ByBudget { get; set; } = new();
        public List<MonthlyExpense> Monthly { get; set; } = new();
        public List<BudgetComparison> BudgetVsActual { get; set; } = new();
    }

    public record BudgetExpense(string Category, decimal TotalSpent);
    public record MonthlyExpense(int Year, int Month, decimal Total);
    public record BudgetComparison(string Category, decimal Budgeted, decimal Spent);
}
