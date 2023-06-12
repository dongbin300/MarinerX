namespace CryptoModel
{
    public static class Extension
    {
        public static string ToSignedString(this int value) => value >= 0 ? "+" + value : value.ToString();
        public static string ToSignedString(this double value) => value >= 0 ? "+" + value : value.ToString();
        public static string ToSignedString(this decimal value) => value >= 0 ? "+" + value : value.ToString();
        public static string ToSignedPercentString(this int value) => value >= 0 ? "+" + value + "%" : value + "%";
        public static string ToSignedPercentString(this double value) => value >= 0 ? "+" + value + "%" : value + "%";
        public static string ToSignedPercentString(this decimal value) => value >= 0 ? "+" + value + "%" : value + "%";
        public static double Round(this double value, int digit) => Math.Round(value, digit);
        public static decimal Round(this decimal value, int digit) => Math.Round(value, digit);
        public static int ToInt(this string value) => int.Parse(value);
        public static double ToDouble(this string value) => double.Parse(value);
        public static decimal ToDecimal(this string value) => decimal.Parse(value);
        public static DateTime ToDateTime(this string value) => DateTime.Parse(value);
        public static string Down(this string path, params string[] downPaths) => Path.Combine(path, Path.Combine(downPaths));
        public static void TryCreate(this string path)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, string.Empty);
            }
        }
        public static void TryCreateDirectory(this string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
