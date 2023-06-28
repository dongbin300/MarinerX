using Skender.Stock.Indicators;

namespace CryptoModel.Indicators
{
    /// <summary>
    /// Time Segmented Volume Result
    /// </summary>
    public class TsvResult : ResultBase
    {
        public double Tsv { get; set; }

        public TsvResult(DateTime date, double tsv)
        {
            Date = date;
            Tsv = tsv;
        }
    }
}
