using Binance.Net.Objects.Models.Futures;

using MarinerX.Bot.Clients;
using MarinerX.Bot.Interfaces;
using MarinerX.Bot.Models;
using MarinerX.Bot.Systems;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MarinerX.Bot.Bots
{
    public class Bot : IBot
    {
        public string Name { get; set; }
        public string Description { get; set; }
        protected List<string> MonitorSymbols { get; set; } = new()
        {
            "AAVEUSDT",
            "ADAUSDT",
            "ALGOUSDT",
            "ALPHAUSDT",
            "ATOMUSDT",
            "AVAXUSDT",
            "AXSUSDT",
            "BALUSDT",
            "BANDUSDT",
            "BATUSDT",
            "BCHUSDT",
            "BELUSDT",
            "BLZUSDT",
            "BNBUSDT",
            "BTCUSDT",
            "COMPUSDT",
            "CRVUSDT",
            "CTKUSDT",
            "CVCUSDT",
            "DASHUSDT",
            "DOGEUSDT",
            "DOTUSDT",
            "EGLDUSDT",
            "ENJUSDT",
            "EOSUSDT",
            "ETCUSDT",
            "ETHUSDT",
            "FILUSDT",
            "FLMUSDT",
            "FTMUSDT",
            "GRTUSDT",
            "ICXUSDT",
            "IOSTUSDT",
            "IOTAUSDT",
            "KAVAUSDT",
            "KNCUSDT",
            "KSMUSDT",
            "LRCUSDT",
            "LTCUSDT",
            "MATICUSDT",
            "MKRUSDT",
            "NEARUSDT",
            "NEOUSDT",
            "OCEANUSDT",
            "OMGUSDT",
            "ONTUSDT",
            "QTUMUSDT",
            "RENUSDT",
            "RLCUSDT",
            "RSRUSDT",
            "RUNEUSDT",
            "SKLUSDT",
            "SNXUSDT",
            "SOLUSDT",
            "STORJUSDT",
            "SUSHIUSDT",
            "SXPUSDT",
            "THETAUSDT",
            "TOMOUSDT",
            "TRBUSDT",
            "TRXUSDT",
            "UNFIUSDT",
            "UNIUSDT",
            "VETUSDT",
            "WAVESUSDT",
            "XLMUSDT",
            "XMRUSDT",
            "XRPUSDT",
            "XTZUSDT",
            "YFIUSDT",
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
