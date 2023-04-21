using Albedo.Enums;

namespace Albedo.Models
{
    public class Pair
    {
        public PairMarket Market { get; set; }
        public PairMarketType MarketType { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal PriceChangePercent { get; set; }
        public bool IsRendered { get; set; }

        public Pair(PairMarket market, PairMarketType marketType, string symbol, decimal price, decimal priceChangePercent)
        {
            Market = market;
            MarketType = marketType;
            Symbol = symbol;
            Price = price;
            PriceChangePercent = priceChangePercent;
        }
    }
}
