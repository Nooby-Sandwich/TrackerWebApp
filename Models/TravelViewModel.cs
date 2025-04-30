namespace TrackerWebApp.Models
{
    public class TravelViewModel
    {
        public List<Currency> Currencies { get; set; } = new();
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal? ConvertedAmount { get; set; }
    }

}
