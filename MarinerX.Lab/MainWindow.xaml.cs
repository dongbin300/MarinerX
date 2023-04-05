using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;

using Skender.Stock.Indicators;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace MarinerX.Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten");
        readonly string FuturesDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "BinanceFuturesData");
        readonly string ApiKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "binance_api.txt");
        readonly string SymbolNamePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "BinanceFuturesData", "symbol_2023-03-29.txt");
        readonly string Path1m = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "BinanceFuturesData", "1m");
        readonly string Path1d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "BinanceFuturesData", "1D");
        string Path1mDate(string symbol, DateTime date) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "BinanceFuturesData", "1m", symbol, $"{symbol}_{date:yyyy-MM-dd}.csv");

        BinanceClient client = default!;
        BinanceSocketClient socketClient = default!;

        List<string> symbols = new ();

        public MainWindow()
        {
            InitializeComponent();
            var data = File.ReadAllLines(ApiKeyPath);
            client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new BinanceApiCredentials(data[0], data[1])
            });
            symbols = File.ReadAllLines(SymbolNamePath).ToList();
        }

        private void Lab1Button_Click(object sender, RoutedEventArgs e)
        {
            decimal btcusdtPrice = 0;
            Dictionary<string, decimal> transactionAmounts = new();
            foreach(var symbol in symbols)
            {
                var result = client.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol, Binance.Net.Enums.KlineInterval.OneDay, DateTime.Parse("2023-04-01"), null, 1);
                result.Wait();

                if(symbol == "BTCUSDT")
                {
                    btcusdtPrice = result.Result.Data.First().OpenPrice;
                }
                var transactionAmountBtc = result.Result.Data.First().QuoteVolume / btcusdtPrice;

                transactionAmounts.Add(symbol, transactionAmountBtc);
                Thread.Sleep(100);
            }
            var avg = transactionAmounts.Average(x => x.Value);
        }

        private void Lab2Button_Click(object sender, RoutedEventArgs e)
        {
            var symbol = "MATICUSDT";
            var date = DateTime.Parse("2023-01-01");
            var quotes = GetQuotesForOneDay(symbol, date);
            var rsi = ConvertCandle(KlineInterval.FiveMinutes, quotes).GetRsi().ToList();
            var macd = ConvertCandle(KlineInterval.FifteenMinutes, quotes).GetMacd().ToList();
        }

        private void Lab3Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public List<Quote> GetQuotesForOneDay(string symbol, DateTime date)
        {
            var data = File.ReadAllLines(Path1mDate(symbol, date));
            var quotes = new List<Quote>();

            foreach (var d in data)
            {
                var e = d.Split(',');
                quotes.Add(new Quote
                {
                    Date = DateTime.Parse(e[0]),
                    Open = decimal.Parse(e[1]),
                    High = decimal.Parse(e[2]),
                    Low = decimal.Parse(e[3]),
                    Close = decimal.Parse(e[4]),
                    Volume = decimal.Parse(e[5])
                });
            }

            return quotes;
        }

        public List<Quote> ConvertCandle(KlineInterval interval, List<Quote> quotes)
        {
            var newQuotes = new List<Quote>();

            int unitCount = interval switch
            {
                KlineInterval.ThreeMinutes => 3,
                KlineInterval.FiveMinutes => 5,
                KlineInterval.FifteenMinutes => 15,
                KlineInterval.ThirtyMinutes => 30,
                KlineInterval.OneHour => 60,
                KlineInterval.TwoHour => 120,
                KlineInterval.FourHour => 240,
                KlineInterval.SixHour => 360,
                KlineInterval.EightHour => 480,
                KlineInterval.TwelveHour => 720,
                KlineInterval.OneDay => 1440,
                _ => 1
            };

            for (int i = 0; i < quotes.Count; i += unitCount)
            {
                var targets = quotes.Skip(i).Take(unitCount).ToList();

                newQuotes.Add(new Quote
                {
                    Date = targets[0].Date,
                    Open = targets[0].Open,
                    High = targets.Max(t => t.High),
                    Low = targets.Min(t => t.Low),
                    Close = targets[^1].Close,
                    Volume = targets.Sum(t => t.Volume)
                });
            }

            return newQuotes;
        }
    }
}
