namespace TrackerWebApp.Models
{
    public class TravelViewModel
    {
        public List<Currency> Currencies { get; set; } = new();
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? ConvertedAmount { get; set; }
    }
}
