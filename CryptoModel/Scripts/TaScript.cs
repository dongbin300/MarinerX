namespace CryptoModel.Scripts
{
    public class TaScript
    {
        public static readonly double NA = 0;

        public static double Na(double value)
        {
            return value == 0 ? 0 : value;
        }

        public static double Nz(double value, double replacement = 0)
        {
            return value == 0 ? replacement : value;
        }

        /// <summary>
        /// Get minimum value
        /// </summary>
        /// <param name="values"></param>
        /// <param name="count"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static double Min(double[] values, int count, int startIndex = 0)
        {
            double min = 999999999;
            for (int i = startIndex; i < startIndex + count; i++)
            {
                if (values[i] == NA)
                {
                    continue;
                }

                if (values[i] < min)
                {
                    min = values[i];
                }
            }
            return min;
        }

        /// <summary>
        /// Get maximum value
        /// </summary>
        /// <param name="values"></param>
        /// <param name="count"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static double Max(double[] values, int count, int startIndex = 0)
        {
            var max = values[startIndex];
            for (int i = startIndex + 1; i < startIndex + count; i++)
            {
                if (values[i] == NA)
                {
                    continue;
                }

                if (values[i] > max)
                {
                    max = values[i];
                }
            }
            return max;
        }

        /// <summary>
        /// Get previous change value
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double[] Change(double[] values)
        {
            var result = new double[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                if (i == 0)
                {
                    result[i] = NA;
                    continue;
                }

                result[i] = values[i] - values[i - 1];
            }
            return result;
        }

        /// <summary>
        /// Simple average of values[0]~values[count-1]
        /// </summary>
        /// <param name="values"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static double SAverage(double[] values, int count, int startIndex = 0)
        {
            double sum = 0;
            for (int i = startIndex; i < startIndex + count; i++)
            {
                sum += values[i];
            }
            return sum / count;
        }

        /// <summary>
        /// Relative average of values[0]~values[count-1]
        /// </summary>
        /// <param name="values"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static double RAverage(double[] values, int count)
        {
            return default!;
        }

        /// <summary>
        /// Recommend values is Quote.Close
        /// </summary>
        /// <param name="values"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static double[] Sma(double[] values, int period)
        {
            var result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (i < period - 1)
                {
                    result[i] = NA;
                    continue;
                }

                double sum = 0;
                for (int j = i - period + 1; j <= i; j++)
                {
                    sum += values[j];
                }
                result[i] = sum / period;
            }

            return result;
        }

        /// <summary>
        /// Recommend values is Quote.Close
        /// </summary>
        /// <param name="values"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static double[] Ema(double[] values, int period)
        {
            var result = new double[values.Length];
            double alpha = 2.0 / (period + 1);
            for (int i = 0; i < values.Length; i++)
            {
                if (i < period - 1)
                {
                    result[i] = NA;
                    continue;
                }

                if (i == period - 1)
                {
                    result[i] = SAverage(values, period);
                    continue;
                }

                result[i] = alpha * values[i] + (1 - alpha) * result[i - 1];
            }

            return result;
        }

        /// <summary>
        /// Recommend values is Quote.Close
        /// </summary>
        /// <param name="values"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static double[] Rma(double[] values, int period, int startIndex = 0)
        {
            var result = new double[values.Length];
            double alpha = 1.0 / period;
            for (int i = 0; i < values.Length; i++)
            {
                if (i < period - 1 + startIndex)
                {
                    result[i] = NA;
                    continue;
                }

                if (i == period - 1 + startIndex)
                {
                    result[i] = SAverage(values, period, startIndex);
                    continue;
                }

                result[i] = alpha * values[i] + (1 - alpha) * result[i - 1];
            }

            return result;
        }

        public static double[] Tr(double[] high, double[] low, double[] close)
        {
            var tr = new double[high.Length];

            for (int i = 0; i < high.Length; i++)
            {
                if (i == 0)
                {
                    tr[i] = high[i] - low[i];
                    continue;
                }

                tr[i] =
                    Math.Max(
                        Math.Max(high[i] - low[i], Math.Abs(high[i] - close[i - 1])),
                        Math.Abs(low[i] - close[i - 1])
                        );
            }

            return tr;
        }

        /// <summary>
        /// Average True Range
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static double[] Atr(double[] high, double[] low, double[] close, int period)
        {
            var tr = Tr(high, low, close);
            return Rma(tr, period);
        }

        /// <summary>
        /// Relative Strength Index
        /// </summary>
        /// <param name="values"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static double[] Rsi(double[] values, int period)
        {
            var rsi = new double[values.Length];
            var u = new double[values.Length];
            var d = new double[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                if (i == 0)
                {
                    u[i] = NA;
                    d[i] = NA;
                    continue;
                }

                u[i] = Math.Max(values[i] - values[i - 1], 0);
                d[i] = Math.Max(values[i - 1] - values[i], 0);
            }

            var uRma = Rma(u, period, 1);
            var dRma = Rma(d, period, 1);
            for (int i = period; i < values.Length; i++)
            {
                var rs = uRma[i] / dRma[i];
                rsi[i] = 100 - 100 / (1 + rs);
            }

            return rsi;
        }

        /// <summary>
        /// Stochastic
        /// </summary>
        /// <param name="values"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static double[] Stoch(double[] high, double[] low, double[] close, int period, int startIndex = 0)
        {
            var stoch = new double[high.Length];

            for (int i = 0; i < high.Length; i++)
            {
                if (i < period - 1 + startIndex)
                {
                    stoch[i] = NA;
                    continue;
                }

                var min = Min(low, period, i + 1 - period);
                var max = Max(high, period, i + 1 - period);
                stoch[i] = 100 * (close[i] - min) / (max - min);
            }

            return stoch;
        }

        /// <summary>
        /// Direction
        /// -1 - uptrend
        /// 1 - downtrend
        /// 
        /// atrPeriod 이전의 값들은 의미가 없는 것 같음
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="factor"></param>
        /// <param name="atrPeriod"></param>
        /// <returns></returns>
        public static (double[], double[]) Supertrend(double[] high, double[] low, double[] close, double factor, int atrPeriod)
        {
            var upperBand = new double[high.Length];
            var lowerBand = new double[high.Length];
            var supertrend = new double[high.Length];
            var direction = new double[high.Length];

            var atr = Atr(high, low, close, atrPeriod);
            for(int i = 0; i < high.Length; i++)
            {
                var mid = (high[i] + low[i]) / 2;
                upperBand[i] = mid + factor * atr[i];
                lowerBand[i] = mid - factor * atr[i];
                var prevUpperBand = i == 0 ? 0 : upperBand[i - 1];
                var prevLowerBand = i == 0 ? 0 : lowerBand[i - 1];
                var prevClose = i == 0 ? 0 : close[i - 1];
                var prevSupertrend = i == 0 ? 0 : supertrend[i - 1];

                lowerBand[i] = lowerBand[i] > prevLowerBand || prevClose < prevLowerBand ? lowerBand[i] : prevLowerBand;
                upperBand[i] = upperBand[i] < prevUpperBand || prevClose > prevUpperBand ? upperBand[i] : prevUpperBand;

                direction[i] =
                    i == 0 ? 1 :
                    prevSupertrend == prevUpperBand ? (close[i] > upperBand[i] ? -1 : 1) :
                    (close[i] < lowerBand[i] ? 1 : -1);

                supertrend[i] = direction[i] == -1 ? lowerBand[i] : upperBand[i];
            }

            return (supertrend, direction);
        }
    }
}
