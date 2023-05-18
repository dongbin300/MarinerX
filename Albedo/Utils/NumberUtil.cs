using System;

namespace Albedo.Utils
{
    public class NumberUtil
    {
        public static int GetDecimalDigitsCount(decimal value)
        {
            var bits = decimal.GetBits(value);
            var decimalPart = new decimal(bits[0], bits[1], bits[2], false, (byte)((bits[3] >> 16) & 31));
            return (decimalPart - decimal.Truncate(decimalPart)).ToString().Length - 2;
        }

        public static decimal ToRoundedValue(decimal value)
        {
            return Math.Round(value, BitConverter.GetBytes(decimal.GetBits(value)[3])[2]);
        }

        public static string ToRoundedValueString(decimal value)
        {
            return ToRoundedValue(value).ToString("#,0.############################");
        }
    }
}
