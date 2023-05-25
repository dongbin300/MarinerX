using System;

namespace Albedo.Utils
{
    public class NumberUtil
    {
        public static int GetSignificantDigitCount(decimal value)
        {
            string valueString = value.ToString().TrimEnd('0');
            int decimalIndex = valueString.IndexOf('.');
            int significantDigits = valueString.Replace(".", "").Length;

            if (decimalIndex >= 0)
            {
                significantDigits -= decimalIndex;
            }

            return significantDigits;
        }

        public static string ToRoundedValueString(decimal value)
        {
            return value.ToString("#,0.############################");
        }
    }
}
