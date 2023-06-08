using Binance.Net.Enums;

using MarinerX.Bot.Models;

using MercuryTradingModel.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
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

        public static readonly SolidColorBrush LongColor = new (Color.FromRgb(14, 203, 129));
        public static readonly SolidColorBrush ShortColor = new (Color.FromRgb(246, 70, 93));

        public static readonly KlineInterval BaseInterval = KlineInterval.OneMinute;
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
    }
}
