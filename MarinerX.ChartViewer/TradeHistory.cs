using Binance.Net.Enums;

using CryptoModel.Backtests;

using System;

namespace MarinerX.ChartViewer
{
    internal class TradeHistory
    {
        public string Symbol { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }
        public PositionSide Side { get; set; }
        public PositionResult Result { get; set; }
        public decimal Income { get; set; }

        public TradeHistory(string symbol, DateTime entryTime, DateTime exitTime, PositionSide side, PositionResult result, decimal income)
        {
            Symbol = symbol;
            EntryTime = entryTime;
            ExitTime = exitTime;
            Side = side;
            Result = result;
            Income = income;
        }
    }
}
