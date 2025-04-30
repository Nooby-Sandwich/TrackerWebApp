using System.ComponentModel.DataAnnotations;

namespace TrackerWebApp.Models
{
    public class Currency
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Code { get; set; }  // ISO 4217 currency code (e.g., "USD", "EUR")

        [Required]
        public string Name { get; set; }  // Descriptive name (e.g., "United States Dollar")
    }
}
