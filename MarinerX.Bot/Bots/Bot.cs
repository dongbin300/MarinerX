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
            "ARUSDT",
            "ATAUSDT",
            "ATOMUSDT",
            "AUDIOUSDT",
            "AXSUSDT",
            "BAKEUSDT",
            "BALUSDT",
            "BANDUSDT",
            "BCHUSDT",
            "BELUSDT",
            "BLZUSDT",
            "C98USDT",
            "CELOUSDT",
            "CELRUSDT",
            "CHRUSDT",
            "CHZUSDT",
            "CKBUSDT",
            "COMPUSDT",
            "COTIUSDT",
            "CRVUSDT",
            "CTKUSDT",
            "CVCUSDT",
            "DASHUSDT",
            "DGBUSDT",
            "DOGEUSDT",
            "DUSKUSDT",
            "DYDXUSDT",
            "EGLDUSDT",
            "ENJUSDT",
            "ENSUSDT",
            "ETCUSDT",
            "FETUSDT",
            "FILUSDT",
            "FTMUSDT",
            "GALAUSDT",
            "GALUSDT",
            "GMTUSDT",
            "GRTUSDT",
            "GTCUSDT",
            "HBARUSDT",
            "HIGHUSDT",
            "HOTUSDT",
            "ICXUSDT",
            "INJUSDT",
            "IOTAUSDT",
            "IOTXUSDT",
            "JASMYUSDT",
            "KAVAUSDT",
            "KNCUSDT",
            "KSMUSDT",
            "LINAUSDT",
            "LITUSDT",
            "LPTUSDT",
            "LRCUSDT",
            "MASKUSDT",
            "MKRUSDT",
            "MTLUSDT",
            "NEARUSDT",
            "NEOUSDT",
            "NKNUSDT",
            "OCEANUSDT",
            "OGNUSDT",
            "OMGUSDT",
            "ONTUSDT",
            "OPUSDT",
            "QNTUSDT",
            "QTUMUSDT",
            "REEFUSDT",
            "RENUSDT",
            "RLCUSDT",
            "RUNEUSDT",
            "SFPUSDT",
            "SKLUSDT",
            "SNXUSDT",
            "SPELLUSDT",
            "STMXUSDT",
            "STORJUSDT",
            "SUSHIUSDT",
            "SXPUSDT",
            "THETAUSDT",
            "TRBUSDT",
            "TRXUSDT",
            "TUSDT",
            "UNFIUSDT",
            "VETUSDT",
            "WAVESUSDT",
            "XEMUSDT",
            "XLMUSDT",
            "XMRUSDT",
            "XTZUSDT",
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
