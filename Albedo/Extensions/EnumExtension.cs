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

        public static UpbitMinuteInterval ToUpbitMinuteInterval(this KlineInterval interval) => interval switch
        {
            KlineInterval.OneMinute => UpbitMinuteInterval.OneMinute,
            KlineInterval.ThreeMinutes => UpbitMinuteInterval.ThreeMinutes,
            KlineInterval.FiveMinutes => UpbitMinuteInterval.FiveMinutes,
            KlineInterval.ThirtyMinutes => UpbitMinuteInterval.ThirtyMinutes,
            KlineInterval.OneHour => UpbitMinuteInterval.SixtyMinutes,
            _ => UpbitMinuteInterval.OneMinute
        };

        public static BithumbInterval ToBithumbInterval(this KlineInterval interval) => interval switch
        {
            KlineInterval.OneMinute => BithumbInterval.OneMinute,
            KlineInterval.ThreeMinutes => BithumbInterval.ThreeMinutes,
            KlineInterval.FiveMinutes => BithumbInterval.FiveMinutes,
            KlineInterval.ThirtyMinutes => BithumbInterval.ThirtyMinutes,
            KlineInterval.OneHour => BithumbInterval.OneHour,
            _ => BithumbInterval.OneMinute
        };
    }
}
