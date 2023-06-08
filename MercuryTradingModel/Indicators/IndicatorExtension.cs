using MercuryTradingModel.Charts;

using Skender.Stock.Indicators;

namespace MercuryTradingModel.Indicators
{
    public static class IndicatorExtension
    {
        public static IEnumerable<RiResult> GetRi(this IEnumerable<Quote> quotes, int period)
        {
            IList<RiResult> result = new List<RiResult>();
            var quoteList = quotes.ToList();

            result.Add(new RiResult(quoteList[0].Date, 0));
            for (int i = 1; i < quoteList.Count; i++)
            {
                var average = quoteList.CloseAverage(i, period);
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

        public static IEnumerable<RsiResult> GetRsiV2(this IEnumerable<Quote> quotes, int period = 14)
        {
            var result = new List<RsiResult>();
            var closeDelta = new List<double>();
            var quoteList = quotes.ToList();

            for (int i = 0; i < quoteList.Count; i++)
            {
                if (i == 0)
                    closeDelta.Add(0);
                else
                    closeDelta.Add((double)(quoteList[i].Close - quoteList[i - 1].Close));
            }

            var up = new List<double>();
            var down = new List<double>();

            for (int i = 0; i < closeDelta.Count; i++)
            {
                if (closeDelta[i] > 0)
                {
                    up.Add(closeDelta[i]);
                    down.Add(0);
                }
                else
                {
                    up.Add(0);
                    down.Add(-1 * closeDelta[i]);
                }
            }

            var maUp = new List<double?>();
            var maDown = new List<double?>();

            maUp = Sma(up, period);
            maDown = Sma(down, period);

            for (int i = 0; i < maUp.Count; i++)
            {
                if (maDown[i] != 0)
                    result.Add(new RsiResult(quoteList[i].Date) { Rsi = 100 - (100 / (1 + (maUp[i] / maDown[i]))) });
                else
                    result.Add(new RsiResult(quoteList[i].Date) { Rsi = 0 });
            }

            return result;
        }

        private static List<double?> Sma(List<double> values, int periods)
        {
            var sma = new List<double?>();

            for (int i = 0; i < values.Count; i++)
            {
                if (i < periods - 1)
                    sma.Add(null);
                else
                    sma.Add(values.GetRange(i - periods + 1, periods).Average());
            }

            return sma;
        }
    }
}
