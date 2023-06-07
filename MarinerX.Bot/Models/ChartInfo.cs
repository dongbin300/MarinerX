using Skender.Stock.Indicators;

namespace MarinerX.Bot.Models
{
    public class ChartInfo
    {
        public Quote Quote { get; set; }
        public double Rsi { get; set; }
        public double Lsma10 { get; set; }
        public double Lsma30 { get; set; }

        public ChartInfo(Quote quote)
        {
            Quote = quote;
        }
    }
}
