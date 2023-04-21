using Albedo.Enums;
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
using System.Windows.Input;

namespace Albedo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// 인터벌 메뉴
    /// 과거차트 보기
    /// 인디케이터
    /// 차트 수치 표시
    /// 메뉴 클릭시 하이라이트(현재가 강조 표시 필요)
    /// 업비트, 빗썸 등등 추가
    /// 코인 메뉴 그룹화(L1: 거래소별(Market), L2: 타입별(Type; Spot;Index;Futures)
    /// 코인 즐겨찾기
    /// 
    /// 화면 설정(추후)
    /// 그림 그리기(추후 아마 안할듯)
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
                Menu.viewModel.UpdatePairInfo(new Pair(PairMarket.Binance, PairMarketType.Futures, item.Symbol, item.LastPrice, item.PriceChangePercent));
            }
        }

        public void ClearPairList()
        {
            Menu.MainGrid.RowDefinitions.Clear();
            Menu.MainGrid.Children.Clear();
        }

        public void RefreshPairList()
        {
            DispatcherService.Invoke(() =>
            {
                for (int i = 0; i < Menu.viewModel.Pairs.Count; i++)
                {
                    var pair = Menu.viewModel.Pairs[i];
                    if (pair.IsRendered)
                    {
                        var pairControl = LogicalTreeHelper.FindLogicalNode(Menu.MainGrid, $"{pair.Market}_{pair.MarketType}_{pair.Symbol}") as PairControl;
                        if (pairControl != null)
                        {
                            pairControl.viewModel.Price = pair.Price;
                            pairControl.viewModel.PriceChangePercent = pair.PriceChangePercent;
                        }
                    }
                    else
                    {
                        pair.IsRendered = true;
                        Menu.MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45) });
                        var pairControl = new PairControl(pair);

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
                            foreach (var element in Menu.MainGrid.Children)
                            {
                                if (element is not PairControl pairControl)
                                {
                                    continue;
                                }
                                pairControl.viewModel.IsSelected = false;
                            }
                            pairControl.viewModel.IsSelected = true;
                        };
                        pairControl.SetValue(Grid.RowProperty, i);
                        Menu.MainGrid.Children.Add(pairControl);
                    }
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Chart.Content is not ChartControl chartControl)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    if (chartControl.Start > 0)
                    {
                        chartControl.Start--;
                        chartControl.End--;
                        chartControl.InvalidateVisual();
                    }
                    break;

                case Key.Right:
                    if (chartControl.End + 1 < chartControl.TotalCount)
                    {
                        chartControl.Start++;
                        chartControl.End++;
                        chartControl.InvalidateVisual();
                    }
                    break;
            }
        }
    }
}
