using Albedo.Models;

using Binance.Net.Enums;

using System;

namespace Albedo.Utils
{
    public class Common
    {
        public static readonly int ChartLoadLimit = 600;
        public static readonly int ChartDefaultViewCount = 120;

        public static Pair Pair = default!;
        public static KlineInterval ChartInterval = KlineInterval.OneMinute;
        public static Action<Pair> PairMenuClick = default!;
        public static Action ChartRefresh = default!;
        public static Action ChartAdditionalLoad = default!;
        public static Action SearchKeywordChanged = default!;
    }
}
