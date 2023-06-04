using Skender.Stock.Indicators;

namespace MercuryTradingModel.Indicators
{
    /// <summary>
    /// LSMA(Least Square Moving Average) Result
    /// </summary>
    public class LsmaResult : ResultBase
    {
        public double? Lsma { get; set; }

        public LsmaResult(DateTime date, double? lsma)
        {
            Date = date;
            Lsma = lsma;
        }
    }
}
