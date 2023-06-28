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
