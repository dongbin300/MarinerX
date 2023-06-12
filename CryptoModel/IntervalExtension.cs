using Binance.Net.Enums;

namespace CryptoModel
{
    public static class IntervalExtension
    {
        public static KlineInterval ToKlineInterval(this string intervalString) => intervalString switch
        {
            "1m" => KlineInterval.OneMinute,
            "3m" => KlineInterval.ThreeMinutes,
            "5m" => KlineInterval.FiveMinutes,
            "15m" => KlineInterval.FifteenMinutes,
            "30m" => KlineInterval.ThirtyMinutes,
            "1H" => KlineInterval.OneHour,
            "2H" => KlineInterval.TwoHour,
            "4H" => KlineInterval.FourHour,
            "6H" => KlineInterval.SixHour,
            "8H" => KlineInterval.EightHour,
            "12H" => KlineInterval.TwelveHour,
            "1D" => KlineInterval.OneDay,
            "3D" => KlineInterval.ThreeDay,
            "1W" => KlineInterval.OneWeek,
            "1M" => KlineInterval.OneMonth,
            _ => KlineInterval.OneMinute
        };
    }
}
