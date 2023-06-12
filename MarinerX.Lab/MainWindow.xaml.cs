using Binance.Net.Clients;

using CryptoModel;

using Skender.Stock.Indicators;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MarinerX.Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Quote> quotes = new();

        public MainWindow()
        {
            InitializeComponent();

            var client = new BinanceClient();
            var result = client.SpotApi.ExchangeData.GetKlinesAsync("XRPUSDT", Binance.Net.Enums.KlineInterval.FiveMinutes, DateTime.Parse("2023-06-11 15:00:00"), null, 100);
            result.Wait();
            var data = result.Result.Data;

            foreach (var item in data)
            {
                quotes.Add(new Quote
                {
                    Date = item.OpenTime,
                    Open = item.OpenPrice,
                    High = item.HighPrice,
                    Low = item.LowPrice,
                    Close = item.ClosePrice
                });
            }

            var jma = quotes.GetJmaSlope(14).Select(x => x.JmaSlope).ToList();
        }
    }
}
