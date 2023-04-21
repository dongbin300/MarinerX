using Albedo.Models;

using Binance.Net.Enums;

using System;

namespace Albedo.Utils
{
    public class Common
    {
        public static Pair Pair = default!;
        public static KlineInterval ChartInterval = KlineInterval.OneMinute;
        public static Action<Pair> PairMenuClick = default!;
        public static Action ChartRefresh = default!;
    }
}
