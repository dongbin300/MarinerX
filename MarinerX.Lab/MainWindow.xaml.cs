using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;

using CryptoModel;
using CryptoModel.Maths;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MarinerX.Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string BinanceApiKeyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Down("Gaten", "binance_api.txt");
        List<Quote> quotes = new();
        public List<SymbolDetail> SymbolDetails = new();
        public Dictionary<string, decimal> Incomes = new Dictionary<string, decimal>();
        List<string> MonitorSymbols { get; set; } = new()
        {
            "OCEANUSDT",
            "WAVESUSDT",
            "ETCUSDT",
            "BELUSDT",
            "BCHUSDT",
            "COMPUSDT",
            "IMXUSDT",
            "HBARUSDT",
            "SFPUSDT",
            "STORJUSDT",
            "AAVEUSDT",
            "MTLUSDT",
            "UNFIUSDT",
            "SXPUSDT",
            "MASKUSDT",
            "NKNUSDT",
            "ADAUSDT",
            "FILUSDT",
            "ATOMUSDT",
            "MANAUSDT",
            "LPTUSDT",
            "ROSEUSDT",
            "ZECUSDT",
            "XRPUSDT",
            "ARUSDT",
            "CHZUSDT",
            "AXSUSDT",
            "TRXUSDT",
            "QTUMUSDT",
            "XMRUSDT",
            "ALPHAUSDT",
            "EGLDUSDT",
            "IOSTUSDT",
            "AVAXUSDT",
            "MKRUSDT",
            "ENSUSDT",
            "RLCUSDT",
            "YFIUSDT"
        };

        public MainWindow()
        {
            InitializeComponent();
            LoadSymbolDetail();

            var apiKey = File.ReadAllLines(BinanceApiKeyPath);
            var client = new BinanceClient(new BinanceClientOptions()
            {
                ApiCredentials = new BinanceApiCredentials(apiKey[0], apiKey[1])
            });

            //var _rr = client.UsdFuturesApi.Account.GetIncomeHistoryAsync(null, "REALIZED_PNL", DateTime.Parse("2023-07-10"), null, 1000);
            //_rr.Wait();
            //var data = _rr.Result.Data;
            //var data2 = data.GroupBy(x => x.Symbol);
            //foreach(var item in data2)
            //{
            //    Incomes.Add(item.Key, item.Sum(x => x.Income));
            //}
            //var totalIncome = data.Sum(x => x.Income);

            //decimal fee = 0;
            //foreach(var symbol in MonitorSymbols)
            //{
            //    var t = client.UsdFuturesApi.Trading.GetUserTradesAsync(symbol, DateTime.Parse("2023-07-10"), null, 1000);
            //    t.Wait();
            //    fee += t.Result.Data.Sum(x => x.Fee);
            //}
            //var est = totalIncome - fee;

            //var symbolString = "ARUSDT;OCEANUSDT;AXSUSDT;ETCUSDT;AAVEUSDT;WAVESUSDT;CHZUSDT;UNFIUSDT;DUSKUSDT;COMPUSDT;FILUSDT;BCHUSDT;GMTUSDT;SFPUSDT;COTIUSDT;RLCUSDT;GALUSDT;GALAUSDT;BANDUSDT;KNCUSDT;ENJUSDT;UNIUSDT;JASMYUSDT;STORJUSDT;MASKUSDT;MTLUSDT;ANKRUSDT;ADAUSDT;KSMUSDT;HBARUSDT;QTUMUSDT;MANAUSDT;ICXUSDT;ANTUSDT;IMXUSDT;LRCUSDT;ALPHAUSDT;FTMUSDT;DASHUSDT;LTCUSDT;SXPUSDT;AVAXUSDT;YFIUSDT;DOGEUSDT;SUSHIUSDT;SANDUSDT;XRPUSDT;BLZUSDT;SNXUSDT;ZECUSDT;OMGUSDT;XMRUSDT;ATOMUSDT;ROSEUSDT;GRTUSDT;RSRUSDT;LPTUSDT;BELUSDT;IOSTUSDT;BALUSDT;TRXUSDT;NEOUSDT;KAVAUSDT;BNBUSDT;ENSUSDT;BTCUSDT;NKNUSDT;ETHUSDT;THETAUSDT;SKLUSDT;RENUSDT;XLMUSDT;EGLDUSDT;WOOUSDT;SOLUSDT;ZENUSDT;MATICUSDT;MKRUSDT;CTKUSDT;DOTUSDT;OPUSDT;ARPAUSDT;TOMOUSDT";
            //var symbols = symbolString.Split(';');
            //var result = new List<string>();
            //foreach (var symbol in symbols)
            //{
            //    var detail = SymbolDetails.Find(x => x.Symbol.Equals(symbol));

            //    var r = client.UsdFuturesApi.ExchangeData.GetTickerAsync(symbol);
            //    r.Wait();
            //    var price = r.Result.Data.LastPrice;
            //    var nextPrice = price + detail.TickSize;
            //    var per = (nextPrice - price) / price * 100;
            //    if (per > 0.04m)
            //    {
            //        result.Add(symbol);
            //    }
            //}

            //var res = client.UsdFuturesApi.Account.GetPositionInformationAsync("ETHUSDT");
            //res.Wait();
            //var dd = res.Result.Data;

            var result = client.SpotApi.ExchangeData.GetKlinesAsync("SOLUSDT", KlineInterval.FiveMinutes, null, null, 200);
            result.Wait();
            var data = result.Result.Data;

            foreach (var item in data)
            {
                quotes.Add(new Quote(item.OpenTime, item.OpenPrice, item.HighPrice, item.LowPrice, item.ClosePrice));
            }

            double[] open = quotes.Select(x => (double)x.Open).ToArray();
            double[] high = quotes.Select(x => (double)x.High).ToArray();
            double[] low = quotes.Select(x => (double)x.Low).ToArray();
            double[] close = quotes.Select(x => (double)x.Close).ToArray();

            var a1 = quotes.GetBollingerBands(24, 4, QuoteType.High).Select(x => x.Upper);
            var a2 = quotes.GetBollingerBands(24, 4, QuoteType.Low).Select(x => x.Lower);

            var rsi = quotes.GetStochasticRsi(3, 3, 14, 14).Select(x=>x.K);
        }

        public void LoadSymbolDetail()
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
            catch
            {
            }
        }
    }
}
