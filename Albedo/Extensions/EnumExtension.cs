using Albedo.Enums;

using Binance.Net.Enums;

using Bithumb.Net.Enums;

using Upbit.Net.Enums;

namespace Albedo.Extensions
{
    public static class EnumExtension
    {
        public static BithumbPaymentCurrency ToBithumbPaymentCurrency(this PairQuoteAsset quoteAsset) => quoteAsset switch
        {
            PairQuoteAsset.BTC => BithumbPaymentCurrency.BTC,
            _ => BithumbPaymentCurrency.KRW
        };

        /// <summary>
        /// 바이낸스 인터벌 - 1, 3, 5, 15, 30m, 1h, 1d, 1w, 1M
        /// 10m = 5m * 2
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static KlineInterval ToBinanceInterval(this CandleInterval interval) => interval switch
        {
            CandleInterval.OneMinute => KlineInterval.OneMinute,
            CandleInterval.ThreeMinutes => KlineInterval.ThreeMinutes,
            CandleInterval.FiveMinutes => KlineInterval.FiveMinutes,
            CandleInterval.TenMinutes => KlineInterval.FiveMinutes,
            CandleInterval.FifteenMinutes => KlineInterval.FifteenMinutes,
            CandleInterval.ThirtyMinutes => KlineInterval.ThirtyMinutes,
            CandleInterval.OneHour => KlineInterval.OneHour,
            CandleInterval.OneDay => KlineInterval.OneDay,
            CandleInterval.OneWeek => KlineInterval.OneWeek,
            CandleInterval.OneMonth => KlineInterval.OneMonth,
            _ => KlineInterval.OneMinute
        };

        /// <summary>
        /// 업비트 인터벌 - 1, 3, 5, 10, 15, 30m, 1h, 1d, 1w, 1M
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static UpbitMinuteInterval ToUpbitMinuteInterval(this CandleInterval interval) => interval switch
        {
            CandleInterval.OneMinute => UpbitMinuteInterval.OneMinute,
            CandleInterval.ThreeMinutes => UpbitMinuteInterval.ThreeMinutes,
            CandleInterval.FiveMinutes => UpbitMinuteInterval.FiveMinutes,
            CandleInterval.TenMinutes => UpbitMinuteInterval.TenMinutes,
            CandleInterval.FifteenMinutes => UpbitMinuteInterval.FifteenMinutes,
            CandleInterval.ThirtyMinutes => UpbitMinuteInterval.ThirtyMinutes,
            CandleInterval.OneHour => UpbitMinuteInterval.SixtyMinutes,
            _ => UpbitMinuteInterval.OneMinute
        };

        /// <summary>
        /// 빗썸 인터벌 - 1, 3, 5, 10, 30m, 1h, 1d
        /// 15m = 5m * 3,
        /// 1w = 1d * 7,
        /// 1M = 1d * 30 (TODO: 월별 그룹)
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static BithumbInterval ToBithumbInterval(this CandleInterval interval) => interval switch
        {
            CandleInterval.OneMinute => BithumbInterval.OneMinute,
            CandleInterval.ThreeMinutes => BithumbInterval.ThreeMinutes,
            CandleInterval.FiveMinutes => BithumbInterval.FiveMinutes,
            CandleInterval.TenMinutes => BithumbInterval.TenMinutes,
            CandleInterval.FifteenMinutes => BithumbInterval.FiveMinutes,
            CandleInterval.ThirtyMinutes => BithumbInterval.ThirtyMinutes,
            CandleInterval.OneHour => BithumbInterval.OneHour,
            CandleInterval.OneDay => BithumbInterval.OneDay,
            CandleInterval.OneWeek => BithumbInterval.OneDay,
            CandleInterval.OneMonth => BithumbInterval.OneDay,
            _ => BithumbInterval.OneMinute
        };
    }
}
