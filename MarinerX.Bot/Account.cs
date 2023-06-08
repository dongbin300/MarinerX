﻿using Binance.Net.Enums;

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
        public static List<BinancePosition> LongPositions => Positions.Where(p => p.PositionSide.Equals("Long")).ToList();
        public static List<BinancePosition> ShortPositions => Positions.Where(p => p.PositionSide.Equals("Short")).ToList();
        public static Action<string> AddHistory = default!;

        public static List<BinancePosition> MockPositions = new();
        public static List<BinancePosition> LongMockPositions => MockPositions.Where(p => p.PositionSide.Equals("Long")).ToList();
        public static List<BinancePosition> ShortMockPositions => MockPositions.Where(p => p.PositionSide.Equals("Short")).ToList();

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
    }
}
