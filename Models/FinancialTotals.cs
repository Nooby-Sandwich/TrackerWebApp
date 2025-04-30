namespace TrackerWebApp.Models
{
    public class FinancialTotals
    {
        public int TotalBudgets { get; set; }
        public int TotalExpenses { get; set; }
        public decimal TotalBudgeted { get; set; }
        public decimal TotalSpent { get; set; }

        // 🔹 Add TotalIncome property
        public decimal TotalIncome { get; set; }

        // 🔹 Add NetBalance as a computed property (Income - Spent)
        public decimal NetBalance => TotalIncome - TotalSpent;
    }
}
