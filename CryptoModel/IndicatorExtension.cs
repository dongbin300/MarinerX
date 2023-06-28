using CryptoModel.Indicators;
using CryptoModel.Scripts;

using Skender.Stock.Indicators;

namespace CryptoModel
{
    public static class IndicatorExtension
    {
        /// <summary>
        /// First Open
        /// = Candle(0).Open
        /// N Open
        /// = (Candle(-1).Open + Candle(-1).Close) / 2
        /// 
        /// Close
        /// = (Candle(0).Open + Candle(0).High + Candle(0).Low + Candle(0).Close) / 4
        /// 
        /// High
        /// = Max(Candle(0).High, Candle(0).Open, Candle(0).Close)
        /// 
        /// Low
        /// = Min(Candle(0).Low, Candle(0).Open, Candle(0).Close)
        /// </summary>
        /// <param name="quotes"></param>
        /// <returns></returns>
        public static IEnumerable<Quote> GetHeikinAshiCandle(this IEnumerable<Quote> quotes)
        {
            var result = new List<Quote>();

            var _q = quotes.ElementAt(0);
            result.Add(new Quote()
            {
                Date = _q.Date,
                Open = (_q.Open + _q.Close) / 2,
                High = Math.Max(_q.High, Math.Max(_q.Open, _q.Close)),
                Low = Math.Min(_q.Low, Math.Min(_q.Open, _q.Close)),
                Close = (_q.Open + _q.High + _q.Low + _q.Close) / 4
            });

            for (int i = 1; i < quotes.Count(); i++)
            {
                var q = quotes.ElementAt(i);
                var prevHa = result[i - 1];
                result.Add(new Quote()
                {
                    Date = q.Date,
                    Open = (prevHa.Open + prevHa.Close) / 2,
                    High = Math.Max(q.High, Math.Max(q.Open, q.Close)),
                    Low = Math.Min(q.Low, Math.Min(q.Open, q.Close)),
                    Close = (q.Open + q.High + q.Low + q.Close) / 4
                });
            }

            return result;
        }

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
                var _highValues = new double[period];
                var _lowValues = new double[period];
                var _closeValues = new double[period];
                Array.Copy(highValues, i - period, _highValues, 0, period);
                Array.Copy(lowValues, i - period, _lowValues, 0, period);
                Array.Copy(closeValues, i - period, _closeValues, 0, period);

                var jmaLine = 0; //CustomScript.Jma(_closeValues);
                var jmaSlope = 0;// CustomScript.Angle(jmaLine, _highValues, _lowValues, _closeValues, period);
                result.Add(new JmaSlopeResult(quotes.ElementAt(i).Date, jmaSlope));
            }

            return result;
        }

        public static IEnumerable<RsiResult> GetRsiV2(this IEnumerable<Quote> quotes, int period = 14)
        {
            var result = new List<RsiResult>();

            var values = quotes.Select(x => (double)x.Close).ToArray();
            var rsi = TaScript.Rsi(values, period);
            for (int i = 0; i < rsi.Length; i++)
            {
                result.Add(new RsiResult(quotes.ElementAt(i).Date) { Rsi = rsi[i] });
            }

            return result;
        }

        public static IEnumerable<StochasticRsiResult> GetStochasticRsi(this IEnumerable<Quote> quotes, int smoothK, int smoothD, int rsiPeriod, int stochasticPeriod)
        {
            var result = new List<StochasticRsiResult>();

            var values = quotes.Select(x => (double)x.Close).ToArray();
            (var k, var d) = CustomScript.StochasticRsi(values, smoothK, smoothD, rsiPeriod, stochasticPeriod);
            for (int i = 0; i < k.Length; i++)
            {
                result.Add(new StochasticRsiResult(quotes.ElementAt(i).Date, k[i], d[i]));
            }

            return result;
        }

        public static IEnumerable<EmaResult> GetEmaV2(this IEnumerable<Quote> quotes, int period)
        {
            var result = new List<EmaResult>();

            var values = quotes.Select(x => (double)x.Close).ToArray();
            var ema = TaScript.Ema(values, period);
            for (int i = 0; i < ema.Length; i++)
            {
                result.Add(new EmaResult(quotes.ElementAt(i).Date) { Ema = ema[i] });
            }

            return result;
        }

        public static IEnumerable<TripleSupertrendResult> GetTripleSupertrend(this IEnumerable<Quote> quotes, int atrPeriod1, double factor1, int atrPeriod2, double factor2, int atrPeriod3, double factor3)
        {
            var result = new List<TripleSupertrendResult>();

            var high = quotes.Select(x => (double)x.High).ToArray();
            var low = quotes.Select(x => (double)x.Low).ToArray();
            var close = quotes.Select(x => (double)x.Close).ToArray();
            (var supertrend1, var direction1, var supertrend2, var direction2, var supertrend3, var direction3) = CustomScript.TripleSupertrend(high, low, close, atrPeriod1, factor1, atrPeriod2, factor2, atrPeriod3, factor3);

            for (int i = 0; i < supertrend1.Length; i++)
            {
                var st1 = i >= atrPeriod1 - 1 ? -direction1[i] * supertrend1[i] : 0;
                var st2 = i >= atrPeriod2 - 1 ? -direction2[i] * supertrend2[i] : 0;
                var st3 = i >= atrPeriod3 - 1 ? -direction3[i] * supertrend3[i] : 0;
                var tripleSupertrend = new TripleSupertrendResult(quotes.ElementAt(i).Date, st1, st2, st3);
                result.Add(tripleSupertrend);
            }

            return result;
        }

        public static IEnumerable<TsvResult> GetTsv(this IEnumerable<Quote> quotes, int period)
        {
            var result = new List<TsvResult>();

            var close = quotes.Select(x => (double)x.Close).ToArray();
            var volume = quotes.Select(x => (double)x.Volume).ToArray();
            var tsv = CustomScript.TimeSegmentedVolume(close, volume, period);

            for (int i = 0; i < tsv.Length; i++)
            {
                result.Add(new TsvResult(quotes.ElementAt(i).Date, tsv[i]));
            }

            return result;
        }
    }
}
