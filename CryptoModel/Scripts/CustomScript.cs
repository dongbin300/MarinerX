namespace CryptoModel.Scripts
{
    public class CustomScript
    {
        //public static double[] Jma(double[] src)
        //{
        //    var jma = new double[src.Length];
        //    var e0 = new double[src.Length];
        //    var e1 = new double[src.Length];
        //    var e2 = new double[src.Length];

        //    for (int i = 1; i < src.Length; i++)
        //    {
        //        e0[i] =
        //            0.330579 * src[i] +
        //            0.669421 * PineScript.Nz(e0, i - 1);

        //        e1[i] =
        //            0.330579 * (src[i] - e0[i]) +
        //            0.669421 * PineScript.Nz(e1, i - 1);

        //        e2[i] =
        //            0.109282 * (e0[i] + 2 * e1[i] - PineScript.Nz(jma, i - 1)) +
        //            0.448125 * PineScript.Nz(e2, i - 1);

        //        jma[i] = e2[i] + PineScript.Nz(jma, i - 1);
        //    }

        //    return jma;
        //}

        //public static double Angle(double[] src, double[] high, double[] low, double[] close, int period = 14)
        //{
        //    return 57.2957764f * (double)Math.Atan((src[0] - src[1]) / TaScript.Atr(high, low, close, period)[0]);
        //}

        public static (double[], double[], double[], double[], double[], double[]) TripleSupertrend(double[] high, double[] low, double[] close, int atrPeriod1, double factor1, int atrPeriod2, double factor2, int atrPeriod3, double factor3)
        {
            (var supertrend1, var direction1) = TaScript.Supertrend(high, low, close, factor1, atrPeriod1);
            (var supertrend2, var direction2) = TaScript.Supertrend(high, low, close, factor2, atrPeriod2);
            (var supertrend3, var direction3) = TaScript.Supertrend(high, low, close, factor3, atrPeriod3);

            return (supertrend1, direction1, supertrend2, direction2, supertrend3, direction3);
        }

        public static (double[], double[]) StochasticRsi(double[] close, int smoothK, int smoothD, int rsiPeriod, int stochasticPeriod)
        {
            var rsi = TaScript.Rsi(close, rsiPeriod);
            var stoch = TaScript.Stoch(rsi, rsi, rsi, stochasticPeriod, 2);
            var k = TaScript.Sma(stoch, smoothK);
            var d = TaScript.Sma(k, smoothD);

            return (k, d);
        }

        /// <summary>
        /// Need more test
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="adxPeriod"></param>
        /// <param name="diPeriod"></param>
        /// <returns></returns>
        public static double[] Adx(double[] high, double[] low, double[] close, int adxPeriod, int diPeriod)
        {
            var adx = new double[high.Length];
            var up = new double[high.Length];
            var down = new double[high.Length];
            var plusDm = new double[high.Length];
            var minusDm = new double[high.Length];
            var trueRange = new double[high.Length];
            var plus = new double[high.Length];
            var minus = new double[high.Length];

            up = TaScript.Change(high);
            down = TaScript.Change(low).Select(x => -x).ToArray();
            for (int i = 0; i < high.Length; i++)
            {
                if (i == 0)
                {
                    plusDm[i] = TaScript.NA;
                    minusDm[i] = TaScript.NA;
                    continue;
                }

                plusDm[i] = (up[i] > down[i] && up[i] > 0) ? up[i] : 0;
                minusDm[i] = (up[i] < down[i] && down[i] > 0) ? down[i] : 0;
            }
            trueRange = TaScript.Atr(high, low, close, diPeriod);
            return trueRange;
        }

        public static double[] Smma(double[] values, int period)
        {
            var smma = new double[values.Length];

            smma[0] = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                smma[i] = (smma[i - 1] * (period - 1) + values[i]) / period;
            }

            return smma;
        }

        public static double[] Zlema(double[] values, int period)
        {
            var zlema = new double[values.Length];

            var ema1 = TaScript.Ema(values, period);
            var ema2 = TaScript.Ema(ema1, period);
            for (int i = 0; i < values.Length; i++)
            {
                zlema[i] = 2 * ema1[i] - ema2[i];
            }

            return zlema;
        }

        /// <summary>
        /// Impulse MACD
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static double[] Imacd(double[] high, double[] low, double[] close, int period)
        {
            var result = new double[high.Length];
            var hlc = new double[high.Length];

            for (int i = 0; i < high.Length; i++)
            {
                hlc[i] = (high[i] + low[i] + close[i]) / 3;
            }

            var hi = Smma(high, period);
            var lo = Smma(low, period);
            var mi = Zlema(hlc, period);

            for (int i = 0; i < high.Length; i++)
            {
                if (mi[i] > hi[i])
                {
                    result[i] = mi[i] - hi[i];
                }
                else if (mi[i] < lo[i])
                {
                    result[i] = mi[i] - lo[i];
                }
                else
                {
                    result[i] = 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Need more test
        /// </summary>
        /// <param name="close"></param>
        /// <param name="volume"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static double[] TimeSegmentedVolume(double[] close, double[] volume, int period)
        {
            var tsv = new double[close.Length];
            for (int i = 0; i < close.Length; i++)
            {
                if (i < period)
                {
                    tsv[i] = 0;
                    continue;
                }

                double sum = 0;
                for (int j = 0; j < period; j++)
                {
                    sum += volume[i - j] * (close[i - j] - close[i - j - 1]);
                }
                tsv[i] = sum;
            }
            return tsv;
        }
    }
}
