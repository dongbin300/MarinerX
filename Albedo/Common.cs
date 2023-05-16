﻿using Albedo.Enums;
using Albedo.Models;

using Binance.Net.Enums;

using System;

namespace Albedo
{
    public class Common
    {
        public static readonly int ChartLoadLimit = 600;
        public static readonly int ChartUpbitLoadLimit = 200;
        public static readonly int ChartDefaultViewCount = 120;

        public static readonly int ChartItemFullWidth = 100;
        public static readonly double ChartItemMarginPercent = 0.2;

        public static PairMarket SupportedMarket = PairMarket.Binance | PairMarket.Upbit | PairMarket.Bithumb;

        public static PairMarketModel CurrentSelectedPairMarket = new(PairMarket.None, "", "");
        public static PairMarketTypeModel CurrentSelectedPairMarketType = new(PairMarketType.None, "");
        public static PairQuoteAssetModel CurrentSelectedPairQuoteAsset = new(PairQuoteAsset.None, "");

        public static Pair Pair = default!;
        public static KlineInterval ChartInterval = KlineInterval.OneMinute;
        public static Action ChartRefresh = default!;
        public static Action ChartAdditionalLoad = default!;
        public static Action SearchKeywordChanged = default!;
        public static Action RefreshAllTickers = default!;
    }
}