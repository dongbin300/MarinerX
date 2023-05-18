using Albedo.Enums;
using Albedo.Extensions;
using Albedo.Managers;
using Albedo.Mappers;
using Albedo.Models;
using Albedo.Utils;
using Albedo.Views;

using Binance.Net.Clients;
using Binance.Net.Objects;

using Bithumb.Net.Clients;
using Bithumb.Net.Enums;

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

using Upbit.Net.Clients;

namespace Albedo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// 지표 Y축 위치 버그
    /// 
    /// 수치 정보 툴팁
    /// 설정 UI 및 버튼 추가(API키 입력, 라이트/다크 모드, 화면설정)
    /// 코인 즐겨찾기
    /// 인디케이터
    /// -이평(기간, 종류[단순sma,가중wma,지수ema], 라인색, 굵기), 볼밴, RSI 필수
    /// 
    /// 코인 정렬 기능(상승/하락률, 가나다순)
    /// 
    /// 로깅
    /// 기능 정리 및 견적 및 사용 매뉴얼 작성
    /// </summary>
    public partial class MainWindow : Window
    {
        int subId = 0;
        BinanceClient binanceClient = new();
        BinanceSocketClient binanceSocketClient = new();
        BithumbClient bithumbClient = new();
        BithumbSocketClient bithumbSocketClient = new(); // for ticker
        BithumbSocketClient bithumbSocketClient2 = new(); // for transaction
        UpbitClient upbitClient = new();
        System.Timers.Timer timer = new(1000);
        System.Timers.Timer upbitTimer = new(3000);
        System.Timers.Timer upbitCandleTimer = new(1000);

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                InitSettings();

                InitBinanceClient();
                InitBithumbClient();
                InitUpbitClient();

                InitAction();

                InitBinanceSocketStreams();
                InitBithumbSocketStreams();

                timer.Elapsed += Timer_Elapsed;
                upbitTimer.Elapsed += UpbitTimer_Elapsed;
                upbitCandleTimer.Elapsed += UpbitCandleTimer_Elapsed;
                timer.Start();
                upbitTimer.Start();
                upbitCandleTimer.Start();
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        #region Window Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
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
                            chartControl.Render();
                        }
                        break;

                    case Key.Right:
                        if (chartControl.ViewEndPosition + chartControl.ItemFullWidth <= chartControl.ChartWidth)
                        {
                            chartControl.ViewStartPosition += chartControl.ItemFullWidth;
                            chartControl.ViewEndPosition += chartControl.ItemFullWidth;
                            chartControl.Render();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }
        #endregion

        #region Timer Event
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Menu.viewModel.SearchPair();
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        private void UpbitTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (Common.CurrentSelectedPairMarket.PairMarket != PairMarket.Upbit)
                {
                    return;
                }

                var symbols = Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset switch
                {
                    PairQuoteAsset.KRW => UpbitSymbolMapper.KrwSymbols,
                    PairQuoteAsset.BTC => UpbitSymbolMapper.BtcSymbols,
                    PairQuoteAsset.USDT => UpbitSymbolMapper.UsdtSymbols,
                    _ => UpbitSymbolMapper.Symbols,
                };
                var tickerResult = upbitClient.QuotationTickers.GetTickersAsync(symbols);
                tickerResult.Wait();
                foreach (var coin in tickerResult.Result)
                {
                    DispatcherService.Invoke(() =>
                    {
                        Menu.viewModel.UpdatePairInfo(new Pair(PairMarket.Upbit, PairMarketType.Spot, Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset, coin.market, coin.trade_price, coin.signed_change_rate * 100));
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        private void UpbitCandleTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (Common.CurrentSelectedPairMarket.PairMarket != PairMarket.Upbit)
                {
                    return;
                }

                DispatcherService.Invoke(() =>
                {
                    if (Chart.Content is not ChartControl chartControl)
                    {
                        return;
                    }

                    ChartMan.UpdateUpbitSpotChart(upbitClient, chartControl);
                });
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }
        #endregion

        void InitSettings()
        {
            try
            {
                Common.ChartInterval = Settings.Default.Interval switch
                {
                    "1분" => CandleInterval.OneMinute,
                    "3분" => CandleInterval.ThreeMinutes,
                    "5분" => CandleInterval.FiveMinutes,
                    "10분" => CandleInterval.TenMinutes,
                    "15분" => CandleInterval.FifteenMinutes,
                    "30분" => CandleInterval.ThirtyMinutes,
                    "1시간" => CandleInterval.OneHour,
                    "1일" => CandleInterval.OneDay,
                    "1주" => CandleInterval.OneWeek,
                    "1월" => CandleInterval.OneMonth,
                    _ => CandleInterval.OneMinute,
                };

                if (!Directory.Exists("Logs"))
                {
                    Directory.CreateDirectory("Logs");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
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
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        void InitBithumbClient()
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "bithumb_api.txt");
                var data = File.ReadAllLines(path);

                bithumbClient = new BithumbClient(data[0], data[1]);
                bithumbSocketClient = new BithumbSocketClient();
                var krwSymbols = bithumbClient.Public.GetAllTickersAsync(BithumbPaymentCurrency.KRW);
                krwSymbols.Wait();
                foreach (var krwSymbol in krwSymbols.Result.data?.coins ?? default!)
                {
                    BithumbSymbolMapper.Add(krwSymbol.currency + "_KRW");
                }
                var btcSymbols = bithumbClient.Public.GetAllTickersAsync(BithumbPaymentCurrency.BTC);
                btcSymbols.Wait();
                foreach (var btcSymbol in btcSymbols.Result.data?.coins ?? default!)
                {
                    BithumbSymbolMapper.Add(btcSymbol.currency + "_BTC");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        void InitUpbitClient()
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "upbit_api.txt");
                var data = File.ReadAllLines(path);

                upbitClient = new UpbitClient(data[0], data[1]);
                var marketList = upbitClient.QuotationMarketList.GetMarketListAsync();
                marketList.Wait();
                foreach (var market in marketList.Result)
                {
                    UpbitSymbolMapper.Add(market.market, market.korean_name);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        void InitAction()
        {
            try
            {
                // 차트 새로고침 이벤트
                Common.ChartRefresh = () =>
                {
                    switch (Common.CurrentSelectedPairMarket.PairMarket)
                    {
                        case PairMarket.Binance: // 바이낸스
                            switch (Common.CurrentSelectedPairMarketType.PairMarketType)
                            {
                                case PairMarketType.Spot: // 현물
                                    (var spotChartControl, var spotSubId) = ChartMan.RefreshBinanceSpotChart(binanceClient, binanceSocketClient, subId);
                                    subId = spotSubId;
                                    Chart.Content = spotChartControl;
                                    break;

                                case PairMarketType.Futures: // 선물
                                    (var futuresChartControl, var futuresSubId) = ChartMan.RefreshBinanceFuturesChart(binanceClient, binanceSocketClient, subId);
                                    subId = futuresSubId;
                                    Chart.Content = futuresChartControl;
                                    break;

                                case PairMarketType.CoinFutures: // 코인선물
                                    (var coinFuturesChartControl, var coinFuturesSubId) = ChartMan.RefreshBinanceCoinFuturesChart(binanceClient, binanceSocketClient, subId);
                                    subId = coinFuturesSubId;
                                    Chart.Content = coinFuturesChartControl;
                                    break;
                            }
                            break;

                        case PairMarket.Upbit: // 업비트
                            var upbitChartControl = ChartMan.RefreshUpbitSpotChart(upbitClient);
                            Chart.Content = upbitChartControl;
                            break;

                        case PairMarket.Bithumb: // 빗썸
                            var bithumbChartControl = ChartMan.RefreshBithumbSpotChart(bithumbClient, bithumbSocketClient2);
                            Chart.Content = bithumbChartControl;
                            break;
                    }
                };

                // 차트 추가 로드 이벤트
                Common.ChartAdditionalLoad = () =>
                {
                    if (Chart.Content is not ChartControl chartControl)
                    {
                        return;
                    }

                    switch (Common.CurrentSelectedPairMarket.PairMarket)
                    {
                        case PairMarket.Binance: // 바이낸스
                            switch (Common.CurrentSelectedPairMarketType.PairMarketType)
                            {
                                case PairMarketType.Spot: // 현물
                                    ChartMan.LoadAdditionalBinanceSpotChart(binanceClient, chartControl);
                                    break;

                                case PairMarketType.Futures: // 선물
                                    ChartMan.LoadAdditionalBinanceFuturesChart(binanceClient, chartControl);
                                    break;

                                case PairMarketType.CoinFutures: // 코인선물
                                    ChartMan.LoadAdditionalBinanceCoinFuturesChart(binanceClient, chartControl);
                                    break;
                            }
                            break;

                        case PairMarket.Upbit: // 업비트
                            ChartMan.LoadAdditionalUpbitSpotChart(upbitClient, chartControl);
                            break;

                        case PairMarket.Bithumb: // 빗썸
                            // 빗썸은 차트 추가 로드를 지원하지 않는다.
                            break;
                    }
                };

                // 검색 키워드 변경 이벤트
                Common.SearchKeywordChanged = Menu.viewModel.SearchPair;

                // 전체 현재가 최초 로드 이벤트
                Common.RefreshAllTickers = () =>
                {
                    if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Bithumb && Common.CurrentSelectedPairMarketType.PairMarketType == PairMarketType.Spot)
                    {
                        var paymentCurrency = Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset.ToBithumbPaymentCurrency();
                        var tickers = bithumbClient.Public.GetAllTickersAsync(paymentCurrency);
                        tickers.Wait();
                        foreach (var coin in tickers.Result.data?.coins ?? default!)
                        {
                            DispatcherService.Invoke(() =>
                            {
                                Menu.viewModel.UpdatePairInfo(new Pair(
                                    Common.CurrentSelectedPairMarket.PairMarket,
                                    Common.CurrentSelectedPairMarketType.PairMarketType,
                                    Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset,
                                    $"{coin.currency}_{paymentCurrency}", coin.closing_price, coin.fluctate_rate_24H));
                            });
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        void InitBinanceSocketStreams()
        {
            try
            {
                binanceSocketClient.SpotApi.ExchangeData.SubscribeToAllTickerUpdatesAsync((obj) =>
                {
                    if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Binance && Common.CurrentSelectedPairMarketType.PairMarketType == PairMarketType.Spot)
                    {
                        var data = obj.Data;
                        foreach (var item in data)
                        {
                            if (BinanceSymbolMapper.GetPairQuoteAsset(item.Symbol) == Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset)
                            {
                                DispatcherService.Invoke(() =>
                                {
                                    Menu.viewModel.UpdatePairInfo(new Pair(
                                        Common.CurrentSelectedPairMarket.PairMarket,
                                        Common.CurrentSelectedPairMarketType.PairMarketType,
                                        Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset,
                                        item.Symbol, item.LastPrice, item.PriceChangePercent));
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
                                Menu.viewModel.UpdatePairInfo(new Pair(
                                    Common.CurrentSelectedPairMarket.PairMarket,
                                    Common.CurrentSelectedPairMarketType.PairMarketType,
                                    Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset,
                                    item.Symbol, item.LastPrice, item.PriceChangePercent));
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
                                Menu.viewModel.UpdatePairInfo(new Pair(
                                    Common.CurrentSelectedPairMarket.PairMarket,
                                    Common.CurrentSelectedPairMarketType.PairMarketType,
                                    Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset,
                                    item.Symbol, item.LastPrice, item.PriceChangePercent));
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        void InitBithumbSocketStreams()
        {
            try
            {
                bithumbSocketClient.Streams.SubscribeToTickerAsync(BithumbSymbolMapper.Symbols, BithumbSocketTickInterval.OneDay, (obj) =>
                {
                    if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Bithumb)
                    {
                        var data = obj.content;

                        if (Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset == BithumbSymbolMapper.GetPairQuoteAsset(data.symbol))
                        {
                            DispatcherService.Invoke(() =>
                            {
                                Menu.viewModel.UpdatePairInfo(new Pair(
                                   Common.CurrentSelectedPairMarket.PairMarket,
                                   Common.CurrentSelectedPairMarketType.PairMarketType,
                                   Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset,
                                   data.symbol, data.closePrice, data.chgRate));
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }
    }
}
