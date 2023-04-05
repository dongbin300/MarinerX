using MercuryTradingModel.Extensions;

using System;

namespace MarinerX.Macro
{
    public class PathUtil
    {
        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Down("Gaten", "BinanceFuturesData");
        public static string TradePath = BasePath.Down("trade");
        public static string PricePath = BasePath.Down("price");
    }
}
