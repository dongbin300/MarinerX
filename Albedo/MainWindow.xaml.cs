using Albedo.Enums;
using Albedo.Models;
using Albedo.Utils;
using Albedo.Views;

using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;

using Skender.Stock.Indicators;

using System;
using System.IO;
using System.Linq;
using System.Windows;
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
    /// 금토일
    /// 수치 정보 툴팁
    /// 
    /// 월화
    /// 설정 UI 및 버튼 추가(API키 입력, 라이트/다크 모드, 화면설정)
    /// 
    /// 수
    /// 코인 즐겨찾기
    /// 
    /// 목금
    /// 인디케이터
    /// -이평(기간, 종류[단순sma,가중wma,지수ema], 라인색, 굵기), 볼밴, RSI 필수
    /// 
    /// 로깅
    /// 기능 정리 및 견적 및 사용 매뉴얼 작성
    /// 
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
            InitSettings();
            InitBinanceClient();
            InitAction();
            InitBinanceSocketStreams();

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

        void InitAction()
        {
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
                chartControl.ViewStartPosition = Math.Max(chartControl.ViewEndPosition - Common.ChartDefaultViewCount * chartControl.ItemFullWidth, 0);
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
            Common.SearchKeywordChanged = Menu.viewModel.SearchPair;
        }

        public void InitBinanceSocketStreams()
        {
            binanceSocketClient.SpotStreams.SubscribeToAllTickerUpdatesAsync((obj) =>
            {
                if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Binance && Common.CurrentSelectedPairMarketType.PairMarketType.ToString().ToUpper().StartsWith("SPOT"))
                {
                    var data = obj.Data;
                    foreach (var item in data)
                    {
                        if (item.Symbol.ToUpper().EndsWith(Common.CurrentSelectedPairMarketType.PairMarketType.ToString()[4..].ToUpper()))
                        {
                            DispatcherService.Invoke(() =>
                            {
                                Menu.viewModel.UpdatePairInfo(new Pair(PairMarket.Binance, PairMarketType.Spot, item.Symbol, item.LastPrice, item.PriceChangePercent));
                            });
                        }
                    }
                }
            });
            binanceSocketClient.UsdFuturesStreams.SubscribeToAllTickerUpdatesAsync((obj) =>
            {
                if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Binance && Common.CurrentSelectedPairMarketType.PairMarketType == PairMarketType.Futures)
                {
                    var data = obj.Data;
                    foreach (var item in data)
                    {
                        DispatcherService.Invoke(() =>
                        {
                            Menu.viewModel.UpdatePairInfo(new Pair(PairMarket.Binance, PairMarketType.Futures, item.Symbol, item.LastPrice, item.PriceChangePercent));
                        });
                    }
                }
            });
            binanceSocketClient.CoinFuturesStreams.SubscribeToAllTickerUpdatesAsync((obj) =>
            {
                if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Binance && Common.CurrentSelectedPairMarketType.PairMarketType == PairMarketType.CoinFutures)
                {
                    var data = obj.Data;
                    foreach (var item in data)
                    {
                        DispatcherService.Invoke(() =>
                        {
                            Menu.viewModel.UpdatePairInfo(new Pair(PairMarket.Binance, PairMarketType.CoinFutures, item.Symbol, item.LastPrice, item.PriceChangePercent));
                        });
                    }
                }
            });
        }

        public void RefreshPairList()
        {
            DispatcherService.Invoke(() =>
            {
                for (int i = 0; i < Menu.viewModel.PairControls.Count; i++)
                {
                    var pairControl = Menu.viewModel.PairControls[i];
                    if (pairControl.Pair.IsRendered) // 이미 추가된 코인
                    {
                        var _pairControl = Menu.viewModel.PairControls.First(p => p.Name.Equals($"{pairControl.Pair.Market}_{pairControl.Pair.MarketType}_{pairControl.Pair.Symbol}"));

                        if (_pairControl != null)
                        {
                            _pairControl.Pair.Price = pairControl.Pair.Price;
                            _pairControl.Pair.PriceChangePercent = pairControl.Pair.PriceChangePercent;
                        }
                    }
                    else // 새로 추가되는 코인
                    {
                        // 검색중일 경우 키워드에 포함되는 것만 표시
                        // 이걸 추가하지 않으면 검색중에 새로 추가되는 코인들이 나타남
                        if (pairControl.Pair.Symbol.Contains(Menu.viewModel.KeywordText))
                        {
                            pairControl.Pair.IsRendered = true;
                            var newPairControl = new PairControl();
                            newPairControl.Init(pairControl.Pair);
                            Menu.viewModel.PairControls.Add(newPairControl);
                        }
                    }
                }
                Menu.viewModel.SearchPair();
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
                    if (chartControl.ViewStartPosition > chartControl.ItemFullWidth)
                    {
                        chartControl.ViewStartPosition -= chartControl.ItemFullWidth;
                        chartControl.ViewEndPosition -= chartControl.ItemFullWidth;
                        chartControl.InvalidateVisual();
                    }
                    break;

                case Key.Right:
                    if (chartControl.ViewEndPosition + chartControl.ItemFullWidth <= chartControl.ChartWidth)
                    {
                        chartControl.ViewStartPosition += chartControl.ItemFullWidth;
                        chartControl.ViewEndPosition += chartControl.ItemFullWidth;
                        chartControl.InvalidateVisual();
                    }
                    break;
            }
        }
    }
}
