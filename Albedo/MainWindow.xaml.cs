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
    /// 캔들 정보 컬러라이즈
    /// 인디케이터 수치 차트 상단 캔들 정보에 같이 표시
    /// 
    /// 설정 UI 및 버튼 추가
    /// - 라이트/다크 모드(추후)
    /// - 화면설정
    ///     - 인디케이터
    ///             - 볼밴
    ///                 - 기간
    ///                 - 편차
    ///                 - 라인색[중심선, 상한선, 하한선]
    ///                 - 굵기[중심선, 상한선, 하한선]
    ///             - 일목균형표
    ///                 - 기간[단기, 중기, 장기]
    ///                 - 구름대 표시 ON/OFF
    ///                 - 라인색[전환선(단기), 기준선(중기), 후행스팬(중기), 선행스팬1(단기,중기), 선행스팬2(장기,중기)]
    ///                 - 굵기[전환선(단기), 기준선(중기), 후행스팬(중기), 선행스팬1(단기,중기), 선행스팬2(장기,중기)]
    ///             - RSI
    ///                 - 기간
    ///                 - 과열
    ///                 - 침체
    ///                 - 라인색
    ///                 - 굵기
    /// 
    /// 코인 즐겨찾기
    /// - 메뉴 화면에서 코인 메뉴에 별표를 넣고, 클릭하면 즐겨찾기에 자동으로 추가, 다시 클릭하면 즐겨찾기 취소
    /// - 즐겨찾기는 모든 거래소, 모든 타입을 통합해서 보여줌(무조건 하나의 즐겨찾기만 존재)
    /// 
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
                Common.ArrangePairs();
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
                SettingsMan.Init();

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
                try
                {
                    binanceClient = new BinanceClient(new BinanceClientOptions
                    {
                        // API Key 없어도 잘 돌아감?
                        //ApiCredentials = new BinanceApiCredentials(Settings.Default.BinanceApiKey, Settings.Default.BinanceSecretKey)
                    });
                }
                catch
                {
                    MessageBox.Show("바이낸스 API 오류입니다.\n다시 시도해 주세요.");
                    throw;
                }

                try
                {
                    binanceSocketClient = new BinanceSocketClient(new BinanceSocketClientOptions
                    {
                        // API Key 없어도 잘 돌아감?
                        //ApiCredentials = new BinanceApiCredentials(Settings.Default.BinanceApiKey, Settings.Default.BinanceSecretKey)
                    });
                }
                catch
                {
                    MessageBox.Show("바이낸스 소켓 오류입니다.\n다시 시도해 주세요.");
                    throw;
                }
                
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
                try
                {
                    // API Key 없어도 잘 돌아감?
                    //bithumbClient = new BithumbClient(Settings.Default.BithumbApiKey, Settings.Default.BithumbSecretKey);
                    bithumbClient = new BithumbClient("", "");
                }
                catch
                {
                    MessageBox.Show("빗썸 API 오류입니다.\n다시 시도해 주세요.");
                    throw;
                }

                try
                {
                    bithumbSocketClient = new BithumbSocketClient();
                }
                catch
                {
                    MessageBox.Show("빗썸 소켓 오류입니다.\n다시 시도해 주세요.");
                    throw;
                }
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
                try
                {
                    // API Key 없으면 안 돌아감
                    upbitClient = new UpbitClient(Settings.Default.UpbitApiKey, Settings.Default.UpbitSecretKey);
                }
                catch
                {
                    MessageBox.Show("업비트 API 오류입니다.\n다시 시도해 주세요.");
                    throw;
                }
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

                // 코인 정리 이벤트
                Common.ArrangePairs = Menu.viewModel.ArrangePairs;

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
                        Common.ArrangePairs();
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
                        Common.ArrangePairs();
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
                        Common.ArrangePairs();
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
                            Common.ArrangePairs();
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
