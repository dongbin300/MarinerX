using MercuryTradingModel.Extensions;

using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace MarinerX.Macro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int y = 2023; y <= 2023; y++)
            {
                for (int m = 1; m <= 1; m++)
                {
                    for (int d = 29; d <= 31; d++)
                    {
                        try
                        {
                            var prices = new List<string>();
                            var aggTradePath = PathUtil.TradePath.Down("BTCUSDT", $"BTCUSDT-aggTrades-{y}-{m:00}-{d:00}.csv");
                            var aggTradeData = File.ReadAllLines(aggTradePath);
                            int i = 0;
                            if (aggTradeData[i].StartsWith("agg"))
                            {
                                i++;
                            }
                            prices.Add(aggTradeData[i++].Split(',')[1]);
                            for (; i < aggTradeData.Length; i++)
                            {
                                var price = aggTradeData[i].Split(',')[1];
                                if (prices[^1] != price)
                                {
                                    prices.Add(price);
                                }
                            }

                            var priceSequencePath = PathUtil.PricePath.Down("BTCUSDT", $"BTCUSDT-prices_{y}-{m:00}-{d:00}.csv");
                            File.WriteAllLines(priceSequencePath, prices);
                        }
                        catch (FileNotFoundException)
                        {
                        }
                    }
                }
            }
        }
    }
}
