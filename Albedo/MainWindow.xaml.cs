using Albedo.Enums;
using Albedo.Models;
using Albedo.Utils;
using Albedo.Views;

using Binance.Net.Clients;
using Binance.Net.Enums;
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
    /// 바이낸스: 1초, 1, 3, 5, 15, 30분, 1, 2, 4, 6, 8, 12시간, 1, 3일, 1주, 1월
    /// 업비트: 1, 3, 5, 15, 10, 30분, 1, 4시간, 1일, 1주, 1월
    /// 빗썸: 1, 5, 15, 30분, 1, 2, 4, 6, 12시간, 1일, 1주, 1월
    /// 공통: 1, 5, 15, 30분, 1, 4시간, 1일, 1주, 1월
    /// 
    /// 인디케이터
    /// -이평(기간, 종류[단순sma,가중wma,지수ema], 라인색, 굵기), 볼밴, RSI 필수
    /// 심볼 검색 기능
    /// 차트 수치, 그리드 표시 및 수치 정보 툴팁
    /// 업비트, 빗썸 등등 추가
    /// 코인 메뉴 그룹화(L1: 거래소별(Market), L2: 타입별(Type; Spot;Index;Futures)
    /// 코인 즐겨찾기
    /// 
    /// 라이트/다크 모드(추후)
    /// 화면 설정(추후)
    /// 그림 그리기(추후 아마 안할듯)
    /// 
    /// 로깅
    /// API KEY 입력 UI
    /// 기능 정리 및 견적
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
            InitSettings();
            InitBinanceClient();
            InitAction();

            binanceSocketClient.UsdFuturesStreams.SubscribeToAllTickerUpdatesAsync(BinanceAllTickerUpdates);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            RefreshPairList();
        }

        void InitSettings()
        {
            Common.ChartInterval = Settings.Default.Interval switch
            {
                "1분" => KlineInterval.OneMinute,
                "5분" => KlineInterval.FiveMinutes,
                "15분" => KlineInterval.FifteenMinutes,
                "30분" => KlineInterval.ThirtyMinutes,
                "1시간" => KlineInterval.OneHour,
                "4시간" => KlineInterval.FourHour,
                "1일" => KlineInterval.OneDay,
                "1주" => KlineInterval.OneWeek,
                "1월" => KlineInterval.OneMonth,
                _ => KlineInterval.OneMinute,
            };
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

        void InitAction()
        {
            // 코인 메뉴 클릭 이벤트
            Common.PairMenuClick = (pair) =>
            {
                Common.Pair = pair;
                Common.ChartRefresh();
                foreach (var element in Menu.MainGrid.Children)
                {
                    if (element is not PairControl _pairControl)
                    {
                        continue;
                    }
                    _pairControl.viewModel.IsSelected = false;
                }
                var pairControl = LogicalTreeHelper.FindLogicalNode(Menu.MainGrid, $"{pair.Market}_{pair.MarketType}_{pair.Symbol}") as PairControl;
                if (pairControl != null)
                {
                    pairControl.viewModel.IsSelected = true;
                }
            };

            // 차트 새로고침 이벤트
            Common.ChartRefresh = () =>
            {
                var chartControl = new ChartControl();
                var klineResult = binanceClient.UsdFuturesApi.ExchangeData.GetKlinesAsync(Common.Pair.Symbol, Common.ChartInterval, null, null, Common.ChartLoadLimit);
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
                chartControl.Start = Math.Max(chartControl.End - Common.ChartDefaultViewCount, 0);
                Chart.Content = chartControl;

                binanceSocketClient.UsdFuturesStreams.UnsubscribeAsync(subId);
                var klineUpdateResult = binanceSocketClient.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(Common.Pair.Symbol, Common.ChartInterval, (obj) =>
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

            // 차트 추가 로드 이벤트
            Common.ChartAdditionalLoad = () =>
            {
                if (Chart.Content is not ChartControl chartControl)
                {
                    return;
                }

                var klineResult = binanceClient.UsdFuturesApi.ExchangeData.GetKlinesAsync(Common.Pair.Symbol, Common.ChartInterval, null, chartControl.Quotes[0].Date, Common.ChartLoadLimit);
                klineResult.Wait();
                chartControl.ConcatenateQuotes(klineResult.Result.Data.Select(x => new Quote
                {
                    Date = x.OpenTime,
                    Open = x.OpenPrice,
                    High = x.HighPrice,
                    Low = x.LowPrice,
                    Close = x.ClosePrice,
                    Volume = x.Volume,
                }).ToList());
            };

            // 검색 키워드 변경 이벤트
            Common.SearchKeywordChanged = () =>
            {
                for (int i = 0; i < Menu.MainGrid.Children.Count; i++)
                {
                    var pairControl = (PairControl)Menu.MainGrid.Children[i];
                    if (pairControl.viewModel.Symbol.Contains(Menu.viewModel.KeywordText))
                    {
                        Menu.MainGrid.RowDefinitions[i].Height = new GridLength(45);
                        pairControl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Menu.MainGrid.RowDefinitions[i].Height = GridLength.Auto;
                        pairControl.Visibility = Visibility.Collapsed;
                    }
                }
            };
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
                    if (pair.IsRendered) // 이미 추가된 코인
                    {
                        var pairControl = LogicalTreeHelper.FindLogicalNode(Menu.MainGrid, $"{pair.Market}_{pair.MarketType}_{pair.Symbol}") as PairControl;
                        if (pairControl != null)
                        {
                            pairControl.viewModel.Price = pair.Price;
                            pairControl.viewModel.PriceChangePercent = pair.PriceChangePercent;
                        }
                    }
                    else // 새로 추가되는 코인
                    {
                        // 검색중일 경우 키워드에 포함되는 것만 표시
                        // 이걸 추가하지 않으면 검색중에 새로 추가되는 코인들이 나타남
                        if (pair.Symbol.Contains(Menu.viewModel.KeywordText)) 
                        {
                            pair.IsRendered = true;
                            Menu.MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45) });
                            var pairControl = new PairControl(pair);
                            pairControl.SetValue(Grid.RowProperty, i);
                            Menu.MainGrid.Children.Add(pairControl);
                        }
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
                    if (chartControl.End + 1 <= chartControl.TotalCount)
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
