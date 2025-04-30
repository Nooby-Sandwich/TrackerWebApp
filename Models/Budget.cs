using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TrackerWebApp.Models;

namespace TrackerWebApp.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }
        public string UserId { get; set; } 

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Limit { get; set; }

        // Navigation property
        public ICollection<Expense> Expenses { get; set; }


    }
}