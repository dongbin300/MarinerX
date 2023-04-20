namespace Albedo.Models
{
    public class Pair
    {
        public string Market { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal PriceChangePercent { get; set; }

        public Pair(string market, string symbol, decimal price, decimal priceChangePercent)
        {
            Market = market;
            Symbol = symbol;
            Price = price;
            PriceChangePercent = priceChangePercent;
        }
    }
}
