using System.Collections.Generic;
using System.Linq;

namespace Albedo.Utils
{
    public class UpbitSymbolMapper
    {
        static Dictionary<string, string> values = new()
        {

        };

        public static List<string> MarketIds => values.Keys.ToList();
        public static List<string> MarketKrws => MarketIds.FindAll(i => i.StartsWith("KRW"));
        public static List<string> MarketBtcs => MarketIds.FindAll(i => i.StartsWith("BTC"));
        public static List<string> MarketUsdts => MarketIds.FindAll(i => i.StartsWith("USDT"));

        public static void Add(string key, string value)
        {
            if (values.ContainsKey(key))
            {
                return;
            }

            values.Add(key, value);
        }

        public static string GetKoreanName(string marketId)
        {
            if (values.TryGetValue(marketId, out var name))
            {
                return name;
            }

            return string.Empty;
        }
    }
}
