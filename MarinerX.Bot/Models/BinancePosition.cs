﻿using System;
using System.Text;
using System.Windows.Media;

namespace MarinerX.Bot.Models
{
    public class BinancePosition
    {
        public string Symbol { get; set; } = string.Empty;
        public string PositionSide { get; set; } = string.Empty;
        public SolidColorBrush PositionSideColor => PositionSide == "Long" ? Common.LongColor : Common.ShortColor;
        public decimal Pnl { get; set; }
        public string PnlString => GetPnlString();
        public SolidColorBrush PnlColor => Pnl >= 0 ? Common.LongColor : Common.ShortColor;
        public decimal EntryPrice { get; set; }
        public decimal MarkPrice { get; set; }
        public decimal Quantity { get; set; }
        public int Leverage { get; set; }
        public decimal Margin => Math.Round(Math.Abs(MarkPrice * Quantity / Leverage), 3);
        public decimal Roe => Math.Round(Pnl / Math.Abs(MarkPrice * Quantity / Leverage) * 100, 2);

        public BinancePosition(string symbol, string positionSide, decimal pnl, decimal entryPrice, decimal markPrice, decimal quantity, int leverage)
        {
            Symbol = symbol;
            PositionSide = positionSide;
            Pnl = pnl;
            EntryPrice = entryPrice;
            MarkPrice = markPrice;
            Quantity = quantity;
            Leverage = leverage;
        }

        public string GetPnlString()
        {
            var builder = new StringBuilder();
            if(Pnl >= 0)
            {
                builder.Append('+');
            }
            builder.Append(Math.Round(Pnl, 3));
            builder.Append(" (");
            if(Roe >= 0)
            {
                builder.Append('+');
            }
            builder.Append(Math.Round(Pnl / Math.Abs(MarkPrice * Quantity / Leverage) * 100, 2));
            builder.Append("%)");

            return builder.ToString();
        }
    }
}
