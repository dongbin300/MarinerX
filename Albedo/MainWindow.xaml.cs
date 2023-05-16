using Albedo.Enums;
using Albedo.Mappers;
using Albedo.Models;
using Albedo.Utils;
using Albedo.Views;

using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;

using Bithumb.Net.Clients;

using Skender.Stock.Indicators;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

using Upbit.Net.Clients;

namespace Albedo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// 콤보박스 정렬
    /// 업비트 - KRW, BTC, USDT(한글 심볼)
    /// 빗썸 - KRW, BTC, 심볼을 한글로 매핑하는 도구
    /// 차트 복구
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
    /// 코인 정렬 기능(상승/하락률, 가나다순)
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
        BithumbClient bithumbClient = new();
        BithumbSocketClient bithumbSocketClient = new();
        UpbitClient upbitClient = new();
        System.Timers.Timer timer = new(1000);
        System.Timers.Timer upbitTimer = new(3000);

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
                timer.Start();
                upbitTimer.Start();
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }

            //var r1 = bithumbClient.Public.GetAllTickersAsync(Bithumb.Net.Enums.BithumbPaymentCurrency.KRW);
            //r1.Wait();
            //var r11 = r1.Result;

            //var r2 = bithumbClient.Public.GetAllTickersAsync(Bithumb.Net.Enums.BithumbPaymentCurrency.BTC);
            //r2.Wait();
            //var r21 = r2.Result;
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
        #endregion

        void InitSettings()
        {
            try
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
                var krwSymbols = bithumbClient.Public.GetAllTickersAsync(Bithumb.Net.Enums.BithumbPaymentCurrency.KRW);
                krwSymbols.Wait();
                foreach (var krwSymbol in krwSymbols.Result.data?.coins ?? default!)
                {
                    BithumbSymbolMapper.Add(krwSymbol.currency + "_KRW");
                }
                var btcSymbols = bithumbClient.Public.GetAllTickersAsync(Bithumb.Net.Enums.BithumbPaymentCurrency.BTC);
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
                bithumbSocketClient.Streams.SubscribeToTickerAsync(BithumbSymbolMapper.Symbols, Bithumb.Net.Enums.BithumbSocketTickInterval.OneDay, (obj) =>
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
