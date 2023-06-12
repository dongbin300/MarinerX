using CryptoModel.Indicators;

using Skender.Stock.Indicators;

namespace CryptoModel
{
    public static class IndicatorExtension
    {
        public static IEnumerable<RiResult> GetRi(this IEnumerable<Quote> quotes, int period)
        {
            var result = new List<RiResult>();
            var quoteList = quotes.ToList();

            result.Add(new RiResult(quoteList[0].Date, 0));
            for (int i = 1; i < quoteList.Count; i++)
            {
                double sum = 0;
                var _period = Math.Min(period, i);
                for (int j = 0; j < _period; j++)
                {
                    sum += Convert.ToDouble(quoteList[i - j].Close);
                }
                var average = sum / _period;
                var ri = (Convert.ToDouble(quoteList[i].Close) - average) / average * 1000;
                result.Add(new RiResult(quoteList[i].Date, ri));
            }

            return result;
        }

        public static IEnumerable<LsmaResult> GetLsma(this IEnumerable<Quote> quotes, int period)
        {
            var pl = new List<double>();
            var result = new List<LsmaResult>();
            var quoteList = quotes.ToList();

            for (int i = 0; i < quoteList.Count; i++)
            {
                pl.Add((double)quoteList[i].Close);

                if (pl.Count >= period)
                {
                    double sumX = 0;
                    double sumY = 0;
                    double sumXY = 0;
                    double sumXX = 0;
                    double sumYY = 0;

                    for (int a = 1; a <= pl.Count; a++)
                    {
                        sumX += a;
                        sumY += pl[a - 1];
                        sumXY += pl[a - 1] * a;
                        sumXX += a * a;
                        sumYY += pl[a - 1] * pl[a - 1];
                    }

                    double m = (sumXY - sumX * sumY / period) / (sumXX - sumX * sumX / period);
                    double b = sumY / period - m * sumX / period;
                    result.Add(new LsmaResult(quoteList[i].Date, m * period + b));
                    pl = pl.Skip(1).ToList();
                }
                else
                {
                    result.Add(new LsmaResult(quoteList[i].Date, 0));
                }
            }

            return result;
        }
    }
}
