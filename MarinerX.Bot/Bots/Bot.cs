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
            "MASKUSDT",
            "GALUSDT",
            "OCEANUSDT",
            "GRTUSDT",
            "WOOUSDT",
            "DYDXUSDT",
            "ENSUSDT",
            "KAVAUSDT",
            "GTCUSDT",
            "GALAUSDT",
            "GMTUSDT",
            "FTMUSDT",
            "SOLUSDT",
            "LITUSDT",
            "OPUSDT",
            "FILUSDT",
            "FLOWUSDT",
            "UNFIUSDT",
            "IMXUSDT",
            "RENUSDT",
            "SANDUSDT",
            "RLCUSDT",
            "CHZUSDT",
            "DUSKUSDT",
            "VETUSDT",
            "REEFUSDT",
            "TRBUSDT",
            "ONEUSDT",
            "CTSIUSDT",
            "SFPUSDT",
            "HOTUSDT",
            "APEUSDT",
            "ROSEUSDT",
            "LINAUSDT",
            "SNXUSDT",
            "STORJUSDT",
            "BLZUSDT",
            "SUSHIUSDT",
            "BANDUSDT",
            "API3USDT",
            "RVNUSDT",
            "CTKUSDT",
            "ANKRUSDT",
            "DENTUSDT",
            "RAYUSDT",
            "AVAXUSDT",
            "BAKEUSDT",
            "KNCUSDT",
            "ENJUSDT",
            "WAVESUSDT",
            "NKNUSDT",
            "PEOPLEUSDT",
            "AUDIOUSDT",
            "NEARUSDT",
            "ARUSDT"
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
