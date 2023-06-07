using System;

namespace MarinerX.Bot.Extensions
{
    public static class DateTimeExtension
    {
        public static DateTime ToDateTime(this long timestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
        }
    }
}
