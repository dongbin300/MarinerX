using System.Windows.Media;

namespace MarinerX.Bot.Models
{
    public class BinancePosition
    {
        public string Symbol { get; set; } = string.Empty;
        public string PositionSide { get; set; } = string.Empty;
        public SolidColorBrush PositionSideColor => PositionSide == "Long" ? Common.LongColor : Common.ShortColor;
        public string Margin { get; set; } = string.Empty;
        public string Pnl { get; set; } = string.Empty;
        public SolidColorBrush PnlColor => Pnl.StartsWith('+') ? Common.LongColor : Common.ShortColor;

        public BinancePosition(string symbol, string positionSide, string margin, string pnl)
        {
            Symbol = symbol;
            PositionSide = positionSide;
            Margin = margin;
            Pnl = pnl;
        }
    }
}
