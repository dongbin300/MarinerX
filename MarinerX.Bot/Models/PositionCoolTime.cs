using Binance.Net.Enums;

using System;

namespace MarinerX.Bot.Models
{
    public class PositionCoolTime
    {
        public bool IsEnable { get; set; }
        public string Symbol { get; set; }
        public PositionSide Side { get; set; }
        public DateTime CloseTime { get; set; }

        public PositionCoolTime(string symbol, PositionSide side, DateTime closeTime)
        {
            IsEnable = true;
            Symbol = symbol;
            Side = side;
            CloseTime = closeTime;
        }
    }
}
