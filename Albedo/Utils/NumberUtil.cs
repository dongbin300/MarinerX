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
    }
}
