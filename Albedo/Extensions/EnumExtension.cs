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

        public static KlineInterval ToBinanceInterval(this CandleInterval interval) => interval switch
        {
            CandleInterval.OneMinute => KlineInterval.OneMinute,
            CandleInterval.ThreeMinutes => KlineInterval.ThreeMinutes,
            CandleInterval.FiveMinutes => KlineInterval.FiveMinutes,
            CandleInterval.FifteenMinutes => KlineInterval.FifteenMinutes,
            CandleInterval.ThirtyMinutes => KlineInterval.ThirtyMinutes,
            CandleInterval.OneHour => KlineInterval.OneHour,
            CandleInterval.OneDay => KlineInterval.OneDay,
            CandleInterval.OneWeek => KlineInterval.OneWeek,
            CandleInterval.OneMonth => KlineInterval.OneMonth,
            _ => KlineInterval.OneMinute
        };

        public static UpbitMinuteInterval ToUpbitMinuteInterval(this CandleInterval interval) => interval switch
        {
            CandleInterval.OneMinute => UpbitMinuteInterval.OneMinute,
            CandleInterval.ThreeMinutes => UpbitMinuteInterval.ThreeMinutes,
            CandleInterval.FiveMinutes => UpbitMinuteInterval.FiveMinutes,
            CandleInterval.ThirtyMinutes => UpbitMinuteInterval.ThirtyMinutes,
            CandleInterval.OneHour => UpbitMinuteInterval.SixtyMinutes,
            _ => UpbitMinuteInterval.OneMinute
        };

        public static BithumbInterval ToBithumbInterval(this CandleInterval interval) => interval switch
        {
            CandleInterval.OneMinute => BithumbInterval.OneMinute,
            CandleInterval.ThreeMinutes => BithumbInterval.ThreeMinutes,
            CandleInterval.FiveMinutes => BithumbInterval.FiveMinutes,
            CandleInterval.ThirtyMinutes => BithumbInterval.ThirtyMinutes,
            CandleInterval.OneHour => BithumbInterval.OneHour,
            _ => BithumbInterval.OneMinute
        };
    }
}
