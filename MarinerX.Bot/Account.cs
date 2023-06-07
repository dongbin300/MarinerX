using Binance.Net.Enums;

using MarinerX.Bot.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MarinerX.Bot
{
    public class Account
    {
        public static List<PairQuote> PairQuotes = new();
        public static List<BinancePosition> Positions = new();
        public static List<BotHistory> BotHistories = new();

        public static List<BinancePosition> MockPositions = new();

        public static bool IsPositioning(string symbol, PositionSide side)
        {
            return Positions.Any(p => p.Symbol.Equals(symbol) && p.PositionSide.Equals(side.ToString()));
        }

        public static BinancePosition? GetPosition(string symbol, PositionSide side)
        {
            return Positions.Find(p => p.Symbol.Equals(symbol) && p.PositionSide.Equals(side.ToString()));
        }

        public static bool IsMockPositioning(string symbol, PositionSide side)
        {
            return MockPositions.Any(p => p.Symbol.Equals(symbol) && p.PositionSide.Equals(side.ToString()));
        }

        public static BinancePosition? GetMockPosition(string symbol, PositionSide side)
        {
            return MockPositions.Find(p => p.Symbol.Equals(symbol) && p.PositionSide.Equals(side.ToString()));
        }

        public static void AddHistory(string text)
        {
            BotHistories.Add(new BotHistory(DateTime.Now, text));
        }
    }
}
