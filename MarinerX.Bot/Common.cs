using Binance.Net.Enums;

using CryptoModel;

using MarinerX.Bot.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace MarinerX.Bot
{
    public class Common
    {
        public static readonly int NullIntValue = -39909;
        public static readonly double NullDoubleValue = -39909;
        public static readonly decimal NullDecimalValue = -39909;

        public static readonly string BinanceApiKeyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Down("Gaten", "binance_api.txt");

        public static readonly SolidColorBrush LongColor = new(Color.FromRgb(14, 203, 129));
        public static readonly SolidColorBrush ShortColor = new(Color.FromRgb(246, 70, 93));

        public static readonly KlineInterval BaseInterval = KlineInterval.ThirtyMinutes;
        public static readonly int BaseIntervalNumber = 1;

        public static List<SymbolDetail> SymbolDetails = new();

        public static void LoadSymbolDetail()
        {
            try
            {
                var data = File.ReadAllLines("Resources/symbol_detail.csv");

                SymbolDetails.Clear();
                for (int i = 1; i < data.Length; i++)
                {
                    var d = data[i].Split(',');
                    SymbolDetails.Add(new SymbolDetail
                    {
                        Symbol = d[0],
                        ListingDate = DateTime.Parse(d[2]),
                        MaxPrice = decimal.Parse(d[3]),
                        MinPrice = decimal.Parse(d[4]),
                        TickSize = decimal.Parse(d[5]),
                        MaxQuantity = decimal.Parse(d[6]),
                        MinQuantity = decimal.Parse(d[7]),
                        StepSize = decimal.Parse(d[8]),
                        PricePrecision = int.Parse(d[9]),
                        QuantityPrecision = int.Parse(d[10])
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(Common), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public static List<PairQuote> PairQuotes = new();
        public static List<BinancePosition> Positions = new();
        public static List<BinancePosition> LongPositions => Positions.Where(p => p.PositionSide.Equals("Long")).ToList();
        public static List<BinancePosition> ShortPositions => Positions.Where(p => p.PositionSide.Equals("Short")).ToList();
        public static List<BinanceOrder> Orders = new();
        public static Action<string, string> AddHistory = default!;

        public static List<BinancePosition> MockPositions = new();
        public static List<BinancePosition> LongMockPositions => MockPositions.Where(p => p.PositionSide.Equals("Long")).ToList();
        public static List<BinancePosition> ShortMockPositions => MockPositions.Where(p => p.PositionSide.Equals("Short")).ToList();

        public static bool IsPositioning(string symbol, PositionSide side)
        {
            return Positions.Any(p => p.Symbol.Equals(symbol) && p.PositionSide.Equals(side.ToString()));
        }

        public static bool IsLongPositioning(string symbol)
        {
            return LongPositions.Any(p => p.Symbol.Equals(symbol));
        }

        public static bool IsShortPositioning(string symbol)
        {
            return ShortPositions.Any(p => p.Symbol.Equals(symbol));
        }

        public static BinancePosition? GetPosition(string symbol, PositionSide side)
        {
            return Positions.Find(p => p.Symbol.Equals(symbol) && p.PositionSide.Equals(side.ToString()));
        }

        public static BinanceOrder? GetOrder(string symbol, PositionSide side, FuturesOrderType type)
        {
            return Orders.Find(o => o.Symbol.Equals(symbol) && o.Side.Equals(side) && o.Type.Equals(type));
        }

        public static bool IsMockPositioning(string symbol, PositionSide side)
        {
            return MockPositions.Any(p => p.Symbol.Equals(symbol) && p.PositionSide.Equals(side.ToString()));
        }

        public static BinancePosition? GetMockPosition(string symbol, PositionSide side)
        {
            return MockPositions.Find(p => p.Symbol.Equals(symbol) && p.PositionSide.Equals(side.ToString()));
        }
    }
}
