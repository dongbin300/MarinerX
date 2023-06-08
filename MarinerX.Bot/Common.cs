using Binance.Net.Enums;

using MercuryTradingModel.Extensions;

using System;
using System.Windows.Media;

namespace MarinerX.Bot
{
    public class Common
    {
        public static readonly int NullIntValue = -39909;
        public static readonly double NullDoubleValue = -39909;
        public static readonly decimal NullDecimalValue = -39909;

        public static readonly string BinanceApiKeyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Down("Gaten", "binance_api.txt");

        public static readonly SolidColorBrush LongColor = new (Color.FromRgb(14, 203, 129));
        public static readonly SolidColorBrush ShortColor = new (Color.FromRgb(246, 70, 93));

        public static readonly KlineInterval BaseInterval = KlineInterval.OneMinute;
        public static readonly int BaseIntervalNumber = 1;
    }
}
