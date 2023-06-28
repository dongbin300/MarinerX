using Binance.Net.Clients;

using CryptoModel;
using CryptoModel.Scripts;

using Skender.Stock.Indicators;

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
            var result = client.SpotApi.ExchangeData.GetKlinesAsync("SOLUSDT", Binance.Net.Enums.KlineInterval.OneMonth, null, null, 100);
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

            quotes = quotes.GetHeikinAshiCandle().ToList();

            double[] open = quotes.Select(x => (double)x.Open).ToArray();
            double[] high = quotes.Select(x => (double)x.High).ToArray();
            double[] low = quotes.Select(x => (double)x.Low).ToArray();
            double[] close = quotes.Select(x => (double)x.Close).ToArray();

            //var ts = CustomScript.TripleSupertrend(high, low, close, 10, 1, 11, 2, 12, 3);
            var sr = CustomScript.StochasticRsi(close, 3, 3, 14, 14);
        }
    }
}
