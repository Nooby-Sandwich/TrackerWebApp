namespace TrackerWebApp.Models
{
    public class FinancialTotals
    {
        public int TotalBudgets { get; set; }
        public int TotalExpenses { get; set; }
        public decimal TotalBudgeted { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal NetBalance => TotalIncome - TotalSpent;
    }
}