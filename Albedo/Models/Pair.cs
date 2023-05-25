using Albedo.Enums;
using Albedo.Managers;
using Albedo.Mappers;
using Albedo.Utils;

using CryptoExchange.Net.CommonObjects;

using System;
using System.Windows.Media.Imaging;

namespace Albedo.Models
{
    public class Pair
    {
        public PairMarket Market { get; set; }
        public PairMarketType MarketType { get; set; }
        public PairQuoteAsset QuoteAsset { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal PriceChangePercent { get; set; }
        public bool IsRendered { get; set; }
        public bool IsSelected { get; set; }

        public string Id => Market + "_" + MarketType + "_" + Symbol;
        public string MarketKorean => Market switch
        {
            PairMarket.Binance => "바이낸스",
            PairMarket.Upbit => "업비트",
            PairMarket.Bithumb => "빗썸",
            _ => ""
        };
        public string MarketTypeKorean => MarketType switch
        {
            PairMarketType.Spot => "현물",
            PairMarketType.Futures => "선물",
            PairMarketType.CoinFutures => "코인 선물",
            _ => ""
        };
        public string SymbolKorean => Market switch
        {
            PairMarket.Binance => Symbol,
            PairMarket.Upbit => UpbitSymbolMapper.GetKoreanName(Symbol),
            PairMarket.Bithumb => BithumbSymbolMapper.GetKoreanName(Symbol),
            _ => Symbol
        };
        public string PriceString => NumberUtil.ToRoundedValueString(Price) + " " + QuoteAsset;
        public string PriceChangePercentString => Math.Round(PriceChangePercent, 2) + "%";
        public BitmapImage MarketIcon => new (new Uri("pack://application:,,,/Albedo;component/Resources/" + Market switch
        {
            PairMarket.Binance => "binance.png",
            PairMarket.Upbit => "upbit.png",
            PairMarket.Bithumb => "bithumb.png",
            _ => ""
        }));
        public bool IsBullish => PriceChangePercent >= 0;
        public bool IsFavorites => SettingsMan.FavoritesList.Contains(Id);
        public BitmapImage FavoritesImage => new(new Uri("pack://application:,,,/Albedo;component/Resources/" + (IsFavorites ? "favorites-on.png" : "favorites-off.png")));

        public Pair(PairMarket market, PairMarketType marketType, PairQuoteAsset quoteAsset, string symbol, decimal price, decimal priceChangePercent)
        {
            if (market == PairMarket.None || marketType == PairMarketType.None)
            {
                return;
            }

            Market = market;
            MarketType = marketType;
            QuoteAsset = quoteAsset;
            Symbol = symbol;
            Price = price;
            PriceChangePercent = priceChangePercent;
        }
    }
}
