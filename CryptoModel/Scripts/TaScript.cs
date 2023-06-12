using CryptoModel.Charts;

namespace CryptoModel.Scripts
{
    public class TaScript
    {
        /// <summary>
        /// Recommend values is Quote.Close
        /// </summary>
        /// <param name="values"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static float Sma(float[] values, int period)
        {
            float sum = 0;
            for (int i = 0; i < period; i++)
            {
                sum += values[i] / period;
            }
            return sum;
        }

        /// <summary>
        /// Recommend values is Quote.Close
        /// </summary>
        /// <param name="values"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static float Ema(float[] values, int period)
        {
            float alpha = 2.0f / (period + 1);
            var sum = new float[values.Length];

            for (int i = 1; i < values.Length; i++)
            {
                if (sum[i - 1] == 0)
                {
                    sum[i] = values[i];
                }
                else
                {
                    sum[i] = alpha * values[i] + (1 - alpha) * sum[i - 1];
                }
            }

            return sum[^1];
        }

        /// <summary>
        /// Recommend values is Quote.Close
        /// </summary>
        /// <param name="values"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static float[] Rma(float[] values, int period)
        {
            float alpha = 1.0f / period;
            var sum = new float[values.Length];

            for (int i = 1; i < values.Length; i++)
            {
                if (sum[i - 1] == 0)
                {
                    sum[i] = Sma(values, period);
                }
                else
                {
                    sum[i] = alpha * values[i] + (1 - alpha) * sum[i - 1];
                }
            }

            return sum;
        }

        public static float[] Atr(float[] high, float[] low, float[] close, int period)
        {
            var trueRange = new float[high.Length];

            for (int i = 1; i < high.Length; i++)
            {
                if (high[i - 1] == 0)
                {
                    trueRange[i] = high[i] - low[i];
                }
                else
                {
                    trueRange[i] = Math.Max(high[i] - low[i], Math.Max(Math.Abs(high[i] - close[i - 1]), Math.Abs(low[i] - close[i - 1])));
                }
            }

            return Rma(trueRange, period);
        }
    }
}
