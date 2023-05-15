using System.Collections.Generic;

namespace Albedo.Utils
{
    public class BithumbSymbolMapper
    {
        static Dictionary<string, string> values = new()
        {
            { "BTC", "비트코인" }
        };

        public static string GetKoreanName(string englishName)
        {
            if (values.TryGetValue(englishName, out var name))
            {
                return name;
            }

            return string.Empty;
        }
    }
}
