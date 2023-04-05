namespace MercuryTradingModel.Extensions
{
    public static class TimestampExtension
    {
        public static long DateTimeToTimeStamp(this DateTime value)
        {
            return ((DateTimeOffset)value).ToUnixTimeSeconds();
        }

        public static long DateTimeToTimeStampMilliseconds(this DateTime value)
        {
            return ((DateTimeOffset)value).ToUnixTimeMilliseconds();
        }

        public static DateTime TimeStampToDateTime(this long value)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(value).ToLocalTime();
            return dt;
        }

        public static DateTime TimeStampMillisecondsToDateTime(this long value)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddMilliseconds(value).ToLocalTime();
            return dt;
        }
    }
}
