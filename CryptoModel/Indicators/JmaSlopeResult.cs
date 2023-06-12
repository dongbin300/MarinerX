using Skender.Stock.Indicators;

namespace CryptoModel.Indicators
{
    public class JmaSlopeResult : ResultBase
    {
        public double? JmaSlope { get; set; }

        public JmaSlopeResult(DateTime date, double? jmaSlope)
        {
            Date = date;
            JmaSlope = jmaSlope;
        }
    }
}
