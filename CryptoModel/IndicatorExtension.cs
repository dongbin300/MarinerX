using CryptoModel.Indicators;
using CryptoModel.Scripts;

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

        public static IEnumerable<JmaSlopeResult> GetJmaSlope(this IEnumerable<Quote> quotes, int period = 14)
        {
            var result = new List<JmaSlopeResult>();
            var highValues = quotes.Select(q => (float)q.High).ToArray();
            var lowValues = quotes.Select(q => (float)q.Low).ToArray();
            var closeValues = quotes.Select(q => (float)q.Close).ToArray();

            for (int i = 0; i < 14; i++)
            {
                result.Add(new JmaSlopeResult(quotes.ElementAt(i).Date, 0));
            }

            for (int i = 14; i < quotes.Count(); i++)
            {
                var _highValues = new float[period];
                var _lowValues = new float[period];
                var _closeValues = new float[period];
                Array.Copy(highValues, i - period, _highValues, 0, period);
                Array.Copy(lowValues, i - period, _lowValues, 0, period);
                Array.Copy(closeValues, i - period, _closeValues, 0, period);

                var jmaLine = CustomScript.Jma(_closeValues);
                var jmaSlope = CustomScript.Angle(jmaLine, _highValues, _lowValues, _closeValues, period);
                result.Add(new JmaSlopeResult(quotes.ElementAt(i).Date, jmaSlope));
            }

            return result;
        }
    }
}
