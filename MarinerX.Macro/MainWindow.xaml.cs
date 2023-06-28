using MercuryTradingModel.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace MarinerX.Macro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient client;
        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).Down("aggTrades");
        string priceChangesBasePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).Down("priceChanges");

        public MainWindow()
        {
            InitializeComponent();
        }

        public async Task DownloadFileAsync(HttpClient client, string url, string localPath)
        {
            try
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);

                if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return;
                }

                using var stream = new FileStream(localPath, FileMode.CreateNew);
                await response.Content.CopyToAsync(stream).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client = new HttpClient();
            basePath.TryCreateDirectory();
            priceChangesBasePath.TryCreateDirectory();

            //
            //for (int y = 2023; y <= 2023; y++)
            //{
            //    for (int m = 1; m <= 1; m++)
            //    {
            //        for (int d = 29; d <= 31; d++)
            //        {
            //            try
            //            {
            //                var prices = new List<string>();
            //                var aggTradePath = PathUtil.TradePath.Down(symbol, $"{symbol}-aggTrades-{y}-{m:00}-{d:00}.csv");
            //                var aggTradeData = File.ReadAllLines(aggTradePath);
            //                int i = 0;
            //                if (aggTradeData[i].StartsWith("agg"))
            //                {
            //                    i++;
            //                }
            //                prices.Add(aggTradeData[i++].Split(',')[1]);
            //                for (; i < aggTradeData.Length; i++)
            //                {
            //                    var price = aggTradeData[i].Split(',')[1];
            //                    if (prices[^1] != price)
            //                    {
            //                        prices.Add(price);
            //                    }
            //                }

            //                var priceSequencePath = PathUtil.PricePath.Down("BTCUSDT", $"BTCUSDT-prices_{y}-{m:00}-{d:00}.csv");
            //                File.WriteAllLines(priceSequencePath, prices);
            //            }
            //            catch (FileNotFoundException)
            //            {
            //            }
            //        }
            //    }
            //}
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var symbol = SymbolTextBox.Text + "USDT";
                var path = basePath.Down(symbol);
                path.TryCreateDirectory();
                var startTime = new DateTime(int.Parse(YearTextBox.Text), int.Parse(MonthTextBox.Text), int.Parse(DayTextBox.Text));
                var totalDays = (DateTime.Today - startTime).TotalDays;
                for (int i = 0; i < totalDays; i++)
                {
                    try
                    {
                        var time = startTime.AddDays(i);
                        var url = $"https://data.binance.vision/data/futures/um/daily/aggTrades/{symbol}/{symbol}-aggTrades-{time:yyyy-MM-dd}.zip";
                        await DownloadFileAsync(client, url, path.Down($"{symbol}_{time:yyyy-MM-dd}.zip")).ConfigureAwait(false);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        record TimePrice(string timestamp, string price);

        private void ConvertToPriceChangesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var symbol = SymbolTextBox.Text + "USDT";
                var aggTradesPath = basePath.Down(symbol);
                var priceChangesPath = priceChangesBasePath.Down(symbol);
                priceChangesPath.TryCreateDirectory();
                var startTime = new DateTime(int.Parse(YearTextBox.Text), int.Parse(MonthTextBox.Text), int.Parse(DayTextBox.Text));
                var totalDays = (DateTime.Today - startTime).TotalDays;
                for (int i = 0; i < totalDays; i++)
                {
                    try
                    {
                        var time = startTime.AddDays(i);
                        var prices = new List<TimePrice>();
                        var aggTradePath = aggTradesPath.Down($"{symbol}-aggTrades-{time:yyyy-MM-dd}.csv");
                        var aggTradeData = File.ReadAllLines(aggTradePath);
                        int j = 0;
                        if (aggTradeData[j].StartsWith("agg"))
                        {
                            j++;
                        }
                        var seg = aggTradeData[j++].Split(',');
                        prices.Add(new TimePrice(seg[5], seg[1]));
                        for (; j < aggTradeData.Length; j++)
                        {
                            var _seg = aggTradeData[j].Split(',');
                            var price = _seg[1];
                            if (prices[^1].price != price)
                            {
                                prices.Add(new TimePrice(_seg[5], price));
                            }
                        }

                        var priceSequencePath = priceChangesPath.Down($"{symbol}_changePrices_{time:yyyy-MM-dd}.csv");
                        File.WriteAllLines(priceSequencePath, prices.Select(x=>x.timestamp + "," + x.price));
                    }
                    catch 
                    {
                    }
                }
            }
            catch
            {
            }
        }
    }
}
