using Skender.Stock.Indicators;

namespace MarinerX.Bot.Models
{
    public class ChartInfo
    {
        public Quote Quote { get; set; }
        public double Supertrend1 { get; set; }
        public double Supertrend2 { get; set; }
        public double Supertrend3 { get; set; }

        public ChartInfo(Quote quote)
        {
            Quote = quote;
        }
    }
}
