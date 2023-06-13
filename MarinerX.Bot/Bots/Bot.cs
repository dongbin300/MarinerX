using MarinerX.Bot.Interfaces;

using System.Collections.Generic;

namespace MarinerX.Bot.Bots
{
    public class Bot : IBot
    {
        public string Name { get; set; }
        public string Description { get; set; }
        protected List<string> MonitorSymbols { get; set; } = new()
        {
            "AAVEUSDT",
            "ALGOUSDT",
            "ALICEUSDT",
            "ALPHAUSDT",
            "ANKRUSDT",
            "ANTUSDT",
            "APEUSDT",
            "API3USDT",
            "ARPAUSDT",
            "ARUSDT",
            "ATAUSDT",
            "AUDIOUSDT",
            "AVAXUSDT",
            "AXSUSDT",
            "BAKEUSDT",
            "BALUSDT",
            "BELUSDT",
            "BLZUSDT",
            "C98USDT",
            "CHRUSDT",
            "CHZUSDT",
            "COMPUSDT",
            "COTIUSDT",
            "CRVUSDT",
            "CTSIUSDT",
            "DARUSDT",
            "DASHUSDT",
            "DUSKUSDT",
            "DYDXUSDT",
            "ENJUSDT",
            "ENSUSDT",
            "ETCUSDT",
            "FILUSDT",
            "FTMUSDT",
            "GALAUSDT",
            "GALUSDT",
            "GMTUSDT",
            "GRTUSDT",
            "GTCUSDT",
            "IMXUSDT",
            "JASMYUSDT",
            "KNCUSDT",
            "LINAUSDT",
            "LITUSDT",
            "LPTUSDT",
            "LRCUSDT",
            "MANAUSDT",
            "MASKUSDT",
            "MKRUSDT",
            "NKNUSDT",
            "OCEANUSDT",
            "OGNUSDT",
            "OMGUSDT",
            "ONEUSDT",
            "OPUSDT",
            "PEOPLEUSDT",
            "REEFUSDT",
            "RENUSDT",
            "RLCUSDT",
            "ROSEUSDT",
            "RSRUSDT",
            "RUNEUSDT",
            "RVNUSDT",
            "SANDUSDT",
            "SFPUSDT",
            "SKLUSDT",
            "SNXUSDT",
            "SOLUSDT",
            "STORJUSDT",
            "SUSHIUSDT",
            "THETAUSDT",
            "TOMOUSDT",
            "TRBUSDT",
            "UNFIUSDT",
            "WAVESUSDT",
            "WOOUSDT",
            "ZECUSDT",
            "ZENUSDT",
            "ZILUSDT",
            "ZRXUSDT"
        };

        public Bot() : this("", "")
        {

        }

        public Bot(string name) : this(name, "")
        {

        }

        public Bot(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
