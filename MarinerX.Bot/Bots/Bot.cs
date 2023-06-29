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
            "GALAUSDT",
            "ONEUSDT",
            "KLAYUSDT",
            "OCEANUSDT",
            "DUSKUSDT",
            "MASKUSDT",
            "GRTUSDT",
            "WAVESUSDT",
            "RENUSDT",
            "DOGEUSDT",
            "RVNUSDT",
            "GALUSDT",
            "CELRUSDT",
            "YFIUSDT",
            "ALGOUSDT",
            "LRCUSDT",
            "REEFUSDT",
            "CHZUSDT",
            "SKLUSDT",
            "FTMUSDT",
            "ADAUSDT",
            "BELUSDT",
            "NEARUSDT",
            "BTCUSDT",
            "LPTUSDT",
            "ZRXUSDT",
            "VETUSDT",
            "KAVAUSDT",
            "FILUSDT",
            "OPUSDT",
            "AVAXUSDT",
            "DENTUSDT",
            "ANKRUSDT",
            "CRVUSDT",
            "RSRUSDT",
            "AUDIOUSDT",
            "XTZUSDT"
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
