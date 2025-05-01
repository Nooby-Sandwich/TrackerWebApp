using System.Collections.Generic;

namespace TrackerWebApp.Models
{

    /// <summary>
    /// ViewModel for passing dashboard data to the Home/Index view
    /// </summary>
    public class DashboardViewModel
    {

        /// <summary>Total counts and sums</summary>
        public FinancialTotals Totals { get; set; } = new FinancialTotals();

        /// <summary>All available chart datasets</summary>
        public ChartData ChartData { get; set; } = new ChartData();

        /// <summary>User's pinned chart titles</summary>
        public List<string> PinnedCharts { get; set; } = new List<string>();
    }

     

    /// <summary>Aggregated data sets for each chart type</summary>
    public class ChartData
    {
        /// <summary>Category vs. actual spend comparison</summary>
        public List<CategoryBudgetActual> BudgetVsActual { get; set; } = new List<CategoryBudgetActual>();

        /// <summary>Total spending by category</summary>
        public List<CategorySpend> ByCategory { get; set; } = new List<CategorySpend>();

        /// <summary>Monthly spending trend</summary>
        public List<MonthlySpend> MonthlyTrend { get; set; } = new List<MonthlySpend>();
    }

    /// <summary>Budget vs. actual spend for a category</summary>
    public record CategoryBudgetActual(string Category, decimal Budgeted, decimal Actual);

    /// <summary>Total spend in a category</summary>
    public record CategorySpend(string Category, decimal Total);

    /// <summary>Spend for a given month</summary>
    public record MonthlySpend(int Year, int Month, decimal Total);
}
