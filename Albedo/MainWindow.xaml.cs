using Albedo.Models;
using Albedo.Utils;
using Albedo.Views;

using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Objects;

using CryptoExchange.Net.Sockets;

using Skender.Stock.Indicators;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Albedo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int subId = 0;
        BinanceClient binanceClient = new();
        BinanceSocketClient binanceSocketClient = new();
        System.Timers.Timer timer = new System.Timers.Timer(1000);

        public MainWindow()
        {
            InitializeComponent();
            InitBinanceClient();

            binanceSocketClient.UsdFuturesStreams.SubscribeToAllTickerUpdatesAsync(BinanceAllTickerUpdates);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            RefreshPairList();
        }

        void InitBinanceClient()
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "binance_api.txt");
                var data = File.ReadAllLines(path);

                binanceClient = new BinanceClient(new BinanceClientOptions
                {
                    ApiCredentials = new BinanceApiCredentials(data[0], data[1])
                });
                binanceSocketClient = new BinanceSocketClient(new BinanceSocketClientOptions
                {
                    ApiCredentials = new BinanceApiCredentials(data[0], data[1])
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void BinanceAllTickerUpdates(DataEvent<IEnumerable<IBinance24HPrice>> obj)
        {
            var data = obj.Data;
            foreach (var item in data)
            {
                Menu.viewModel.UpdatePairInfo(new Pair("binance", item.Symbol, item.LastPrice, item.PriceChangePercent));
            }
        }

        public void RefreshPairList()
        {
            DispatcherService.Invoke(() =>
            {
                // [TODO] 코인 메뉴 업데이트 방식 최적화 필요
                Menu.MainGrid.RowDefinitions.Clear();
                Menu.MainGrid.Children.Clear();
                for (int i = 0; i < Menu.viewModel.Pairs.Count; i++)
                {
                    Menu.MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45) });
                    var pair = Menu.viewModel.Pairs[i];

                    var pairControl = new PairControl();
                    pairControl.Init(pair);

                    /* 
                     * 해당 코인 메뉴 클릭
                     * 최초 클릭 시 클라이언트로 120개 Kline을 가져오고
                     * 그 이후 소켓클라이언트로 실시간 Kline을 업데이트한다.
                     */
                    pairControl.PairClick = (_pair) =>
                    {
                        var chartControl = new ChartControl();
                        var klineResult = binanceClient.UsdFuturesApi.ExchangeData.GetKlinesAsync(_pair.Symbol, Binance.Net.Enums.KlineInterval.OneMinute, null, null, 120);
                        klineResult.Wait();
                        chartControl.Init(klineResult.Result.Data.Select(x => new Quote
                        {
                            Date = x.OpenTime,
                            Open = x.OpenPrice,
                            High = x.HighPrice,
                            Low = x.LowPrice,
                            Close = x.ClosePrice,
                            Volume = x.Volume,
                        }).ToList());
                        Chart.Content = chartControl;

                        binanceSocketClient.UsdFuturesStreams.UnsubscribeAsync(subId);
                        var klineUpdateResult = binanceSocketClient.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(_pair.Symbol, Binance.Net.Enums.KlineInterval.OneMinute, (obj) =>
                        {
                            chartControl.UpdateQuote(new Quote
                            {
                                Date = obj.Data.Data.OpenTime,
                                Open = obj.Data.Data.OpenPrice,
                                High = obj.Data.Data.HighPrice,
                                Low = obj.Data.Data.LowPrice,
                                Close = obj.Data.Data.ClosePrice,
                                Volume = obj.Data.Data.Volume
                            });
                        });
                        klineUpdateResult.Wait();
                        subId = klineUpdateResult.Result.Data.Id;
                    };

                    pairControl.SetValue(Grid.RowProperty, i);
                    Menu.MainGrid.Children.Add(pairControl);
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
