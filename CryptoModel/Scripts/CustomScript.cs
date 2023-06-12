namespace CryptoModel.Scripts
{
    public class CustomScript
    {
        public static float[] Jma(float[] src)
        {
            var jma = new float[src.Length];
            var e0 = new float[src.Length];
            var e1 = new float[src.Length];
            var e2 = new float[src.Length];

            for (int i = 1; i < src.Length; i++)
            {
                e0[i] =
                    0.330579f * src[i] +
                    0.669421f * PineScript.Nz(e0, i - 1);

                e1[i] =
                    0.330579f * (src[i] - e0[i]) +
                    0.669421f * PineScript.Nz(e1, i - 1);

                e2[i] =
                    0.109282f * (e0[i] + 2 * e1[i] - PineScript.Nz(jma, i - 1)) +
                    0.448125f * PineScript.Nz(e2, i - 1);

                jma[i] = e2[i] + PineScript.Nz(jma, i - 1);
            }

            return jma;
        }

        public static float Angle(float[] src, float[] high, float[] low, float[] close, int period = 14)
        {
            return 57.2957764f * (float)Math.Atan((src[0] - src[1]) / TaScript.Atr(high, low, close, period)[0]);
        }
    }
}
