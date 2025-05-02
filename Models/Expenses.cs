using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackerWebApp.Models
{
    public class Expense
    {
        [Key]
        public int ExpenseId { get; set; }

        public string UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        // Foreign key
        [ForeignKey("Budget")]
        public int BudgetId { get; set; }
        public Budget Budget { get; set; }
    }


}