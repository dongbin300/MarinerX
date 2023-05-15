using Albedo.Enums;
using Albedo.Utils;

using System;
using System.Windows.Media.Imaging;

namespace Albedo.Models
{
    public class Pair
    {
        public PairMarket Market { get; set; }
        public string MarketKorean { get; set; } = string.Empty;
        public PairMarketType MarketType { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal PriceChangePercent { get; set; }
        public bool IsRendered { get; set; }
        public BitmapImage MarketIcon { get; set; } = default!;
        public bool IsSelected { get; set; }
        public string PriceString => NumberUtil.ToRoundedValue(Price).ToString("0.############################");
        public string PriceChangePercentString => Math.Round(PriceChangePercent, 2) + "%";
        public bool IsBullish => PriceChangePercent >= 0;

        public Pair(PairMarket market, PairMarketType marketType, string symbol, decimal price, decimal priceChangePercent)
        {
            if (market == PairMarket.None || marketType == PairMarketType.None)
            {
                return;
            }

            Market = market;
            MarketType = marketType;
            Symbol = symbol;
            Price = price;
            PriceChangePercent = priceChangePercent;

            MarketKorean = Market switch
            {
                PairMarket.Binance => "바이낸스",
                PairMarket.Upbit => "업비트",
                PairMarket.Bithumb => "빗썸",
                _ => ""
            };

            MarketIcon = new BitmapImage(new Uri("pack://application:,,,/Albedo;component/Resources/" + Market switch
            {
                PairMarket.Binance => "binance.png",
                PairMarket.Upbit => "upbit.png",
                PairMarket.Bithumb => "bithumb.png",
                _ => ""
            }));
        }
    }
}
