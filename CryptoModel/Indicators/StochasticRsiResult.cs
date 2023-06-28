using Skender.Stock.Indicators;

namespace CryptoModel.Indicators
{
    /// <summary>
    /// Stochastic RSI Result
    /// </summary>
    public class StochasticRsiResult : ResultBase
    {
        public double K { get; set; }
        public double D { get; set; }

        public StochasticRsiResult(DateTime date, double k, double d)
        {
            Date = date;
            K = k;
            D = d;
        }
    }
}
