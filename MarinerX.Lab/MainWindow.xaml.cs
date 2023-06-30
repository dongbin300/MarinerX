using Binance.Net.Clients;
using Binance.Net.Objects;

using CryptoModel;

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

        public MainWindow()
        {
            InitializeComponent();

            var apiKey = File.ReadAllLines(BinanceApiKeyPath);
            var client = new BinanceClient(new BinanceClientOptions()
            {
                ApiCredentials = new BinanceApiCredentials(apiKey[0], apiKey[1])
            });

            //var res = client.UsdFuturesApi.Account.GetPositionInformationAsync("ETHUSDT");
            //res.Wait();
            //var dd = res.Result.Data;

            var result = client.UsdFuturesApi.ExchangeData.GetKlinesAsync("DENTUSDT", Binance.Net.Enums.KlineInterval.ThirtyMinutes, null, null, 120);
            result.Wait();
            var data = result.Result.Data;

            foreach (var item in data)
            {
                quotes.Add(new CryptoModel.Quote(item.OpenTime, item.OpenPrice, item.HighPrice, item.LowPrice, item.ClosePrice));
            }

            double[] open = quotes.Select(x => (double)x.Open).ToArray();
            double[] high = quotes.Select(x => (double)x.High).ToArray();
            double[] low = quotes.Select(x => (double)x.Low).ToArray();
            double[] close = quotes.Select(x => (double)x.Close).ToArray();

            var a = quotes.GetTripleSupertrend(10, 1.2, 10, 3, 10, 10);
            //var r = CustomScript.TripleSupertrend(high, low, close, 10, 1.2, 10, 3, 10, 10);
        }
    }
}
