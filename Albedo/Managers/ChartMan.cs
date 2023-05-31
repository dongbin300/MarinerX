using Albedo.Enums;
using Albedo.Extensions;
using Albedo.Utils;
using Albedo.Views;

using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;

using Bithumb.Net.Clients;

using Skender.Stock.Indicators;

using System;
using System.Linq;
using System.Reflection;

using Upbit.Net.Clients;

namespace Albedo.Managers
{
    public class ChartMan
    {
        #region Refresh Chart (Binance Refresh+Update / Upbit Refresh / Bithumb Refresh+Update)
        public static (ChartControl, int) RefreshBinanceChart(BinanceClient binanceClient, BinanceSocketClient binanceSocketClient, int subId, PairMarketType marketType) => marketType switch
        {
            PairMarketType.Spot => RefreshBinanceSpotChart(binanceClient, binanceSocketClient, subId),
            PairMarketType.Futures => RefreshBinanceFuturesChart(binanceClient, binanceSocketClient, subId),
            PairMarketType.CoinFutures => RefreshBinanceCoinFuturesChart(binanceClient, binanceSocketClient, subId),
            _ => (new ChartControl(), 0)
        };

        private static (ChartControl, int) RefreshBinanceSpotChart(BinanceClient binanceClient, BinanceSocketClient binanceSocketClient, int subId)
        {
            try
            {
                var chartControl = new ChartControl();

                var symbol = Common.Pair.Symbol;
                var interval = Common.ChartInterval.ToBinanceInterval();
                var klineResult = binanceClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, interval, null, null, Common.ChartLoadLimit);
                klineResult.Wait();
                var quotes = klineResult.Result.Data.Select(x => new Quote
                {
                    Date = x.OpenTime,
                    Open = x.OpenPrice,
                    High = x.HighPrice,
                    Low = x.LowPrice,
                    Close = x.ClosePrice,
                    Volume = x.Volume,
                }).ToList();
                if (Common.ChartInterval == CandleInterval.TenMinutes)
                {
                    quotes = quotes.Merge(CandleInterval.TenMinutes);
                }
                chartControl.Init(quotes);
                chartControl.ViewStartPosition = Math.Max(chartControl.ViewEndPosition - SettingsMan.DefaultCandleCount * chartControl.ItemFullWidth, 0);

                binanceSocketClient.UnsubscribeAsync(subId);
                var klineUpdateResult = binanceSocketClient.SpotApi.ExchangeData.SubscribeToKlineUpdatesAsync(symbol, interval, (obj) =>
                {
                    DispatcherService.Invoke(() =>
                    {
                        switch (Common.ChartInterval)
                        {
                            case CandleInterval.TenMinutes:
                                chartControl.UpdateQuote(new Quote
                                {
                                    Date = obj.Data.Data.OpenTime,
                                    Open = obj.Data.Data.OpenPrice,
                                    High = obj.Data.Data.HighPrice,
                                    Low = obj.Data.Data.LowPrice,
                                    Close = obj.Data.Data.ClosePrice,
                                    Volume = obj.Data.Data.Volume
                                }, Common.ChartInterval);
                                break;

                            default:
                                chartControl.UpdateQuote(new Quote
                                {
                                    Date = obj.Data.Data.OpenTime,
                                    Open = obj.Data.Data.OpenPrice,
                                    High = obj.Data.Data.HighPrice,
                                    Low = obj.Data.Data.LowPrice,
                                    Close = obj.Data.Data.ClosePrice,
                                    Volume = obj.Data.Data.Volume
                                });
                                break;
                        }
                    });
                });
                klineUpdateResult.Wait();
                subId = klineUpdateResult.Result.Data.Id;

                return (chartControl, subId);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
                return default!;
            }
        }

        private static (ChartControl, int) RefreshBinanceFuturesChart(BinanceClient binanceClient, BinanceSocketClient binanceSocketClient, int subId)
        {
            try
            {
                var chartControl = new ChartControl();

                var symbol = Common.Pair.Symbol;
                var interval = Common.ChartInterval.ToBinanceInterval();
                var klineResult = binanceClient.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol, interval, null, null, Common.ChartLoadLimit);
                klineResult.Wait();
                var quotes = klineResult.Result.Data.Select(x => new Quote
                {
                    Date = x.OpenTime,
                    Open = x.OpenPrice,
                    High = x.HighPrice,
                    Low = x.LowPrice,
                    Close = x.ClosePrice,
                    Volume = x.Volume,
                }).ToList();
                if (Common.ChartInterval == CandleInterval.TenMinutes)
                {
                    quotes = quotes.Merge(CandleInterval.TenMinutes);
                }
                chartControl.Init(quotes);
                chartControl.ViewStartPosition = Math.Max(chartControl.ViewEndPosition - SettingsMan.DefaultCandleCount * chartControl.ItemFullWidth, 0);

                binanceSocketClient.UsdFuturesStreams.UnsubscribeAsync(subId);
                var klineUpdateResult = binanceSocketClient.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(symbol, interval, (obj) =>
                {
                    DispatcherService.Invoke(() =>
                    {
                        switch (Common.ChartInterval)
                        {
                            case CandleInterval.TenMinutes:
                                chartControl.UpdateQuote(new Quote
                                {
                                    Date = obj.Data.Data.OpenTime,
                                    Open = obj.Data.Data.OpenPrice,
                                    High = obj.Data.Data.HighPrice,
                                    Low = obj.Data.Data.LowPrice,
                                    Close = obj.Data.Data.ClosePrice,
                                    Volume = obj.Data.Data.Volume
                                }, Common.ChartInterval);
                                break;

                            default:
                                chartControl.UpdateQuote(new Quote
                                {
                                    Date = obj.Data.Data.OpenTime,
                                    Open = obj.Data.Data.OpenPrice,
                                    High = obj.Data.Data.HighPrice,
                                    Low = obj.Data.Data.LowPrice,
                                    Close = obj.Data.Data.ClosePrice,
                                    Volume = obj.Data.Data.Volume
                                });
                                break;
                        }
                    });
                });
                klineUpdateResult.Wait();
                subId = klineUpdateResult.Result.Data.Id;

                return (chartControl, subId);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
                return default!;
            }
        }

        private static (ChartControl, int) RefreshBinanceCoinFuturesChart(BinanceClient binanceClient, BinanceSocketClient binanceSocketClient, int subId)
        {
            try
            {
                var chartControl = new ChartControl();

                var symbol = Common.Pair.Symbol;
                var interval = Common.ChartInterval.ToBinanceInterval();
                var klineResult = binanceClient.CoinFuturesApi.ExchangeData.GetKlinesAsync(symbol, interval, null, null, Common.ChartLoadLimit);
                klineResult.Wait();
                var quotes = klineResult.Result.Data.Select(x => new Quote
                {
                    Date = x.OpenTime,
                    Open = x.OpenPrice,
                    High = x.HighPrice,
                    Low = x.LowPrice,
                    Close = x.ClosePrice,
                    Volume = x.Volume,
                }).ToList();
                if (Common.ChartInterval == CandleInterval.TenMinutes)
                {
                    quotes = quotes.Merge(CandleInterval.TenMinutes);
                }
                chartControl.Init(quotes);
                chartControl.ViewStartPosition = Math.Max(chartControl.ViewEndPosition - SettingsMan.DefaultCandleCount * chartControl.ItemFullWidth, 0);

                binanceSocketClient.CoinFuturesStreams.UnsubscribeAsync(subId);
                var klineUpdateResult = binanceSocketClient.CoinFuturesStreams.SubscribeToKlineUpdatesAsync(symbol, interval, (obj) =>
                {
                    DispatcherService.Invoke(() =>
                    {
                        switch (Common.ChartInterval)
                        {
                            case CandleInterval.TenMinutes:
                                chartControl.UpdateQuote(new Quote
                                {
                                    Date = obj.Data.Data.OpenTime,
                                    Open = obj.Data.Data.OpenPrice,
                                    High = obj.Data.Data.HighPrice,
                                    Low = obj.Data.Data.LowPrice,
                                    Close = obj.Data.Data.ClosePrice,
                                    Volume = obj.Data.Data.Volume
                                }, Common.ChartInterval);
                                break;

                            default:
                                chartControl.UpdateQuote(new Quote
                                {
                                    Date = obj.Data.Data.OpenTime,
                                    Open = obj.Data.Data.OpenPrice,
                                    High = obj.Data.Data.HighPrice,
                                    Low = obj.Data.Data.LowPrice,
                                    Close = obj.Data.Data.ClosePrice,
                                    Volume = obj.Data.Data.Volume
                                });
                                break;
                        }
                    });
                });
                klineUpdateResult.Wait();
                subId = klineUpdateResult.Result.Data.Id;

                return (chartControl, subId);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
                return default!;
            }
        }

        public static ChartControl RefreshUpbitSpotChart(UpbitClient upbitClient)
        {
            try
            {
                var chartControl = new ChartControl();

                var symbol = Common.Pair.Symbol;
                var defaultCount = Common.ChartUpbitLoadLimit;
                switch (Common.ChartInterval)
                {
                    case CandleInterval.OneMinute:
                    case CandleInterval.ThreeMinutes:
                    case CandleInterval.FiveMinutes:
                    case CandleInterval.FifteenMinutes:
                    case CandleInterval.ThirtyMinutes:
                    case CandleInterval.OneHour:
                        var minuteCandleResult = upbitClient.QuotationCandles.GetMinutesCandlesAsync(symbol, Common.ChartInterval.ToUpbitMinuteInterval(), null, defaultCount);
                        minuteCandleResult.Wait();
                        var minuteQuotes = minuteCandleResult.Result.Select(x => new Quote
                        {
                            Date = x.candle_date_time_kst,
                            Open = x.opening_price,
                            High = x.high_price,
                            Low = x.low_price,
                            Close = x.trade_price,
                            Volume = x.candle_acc_trade_volume,
                        }).ToList();
                        minuteQuotes.Reverse();
                        chartControl.Init(minuteQuotes);
                        break;

                    case CandleInterval.OneDay:
                        var dayCandleResult = upbitClient.QuotationCandles.GetDaysCandlesAsync(symbol, null, defaultCount);
                        dayCandleResult.Wait();
                        var dayQuotes = dayCandleResult.Result.Select(x => new Quote
                        {
                            Date = x.candle_date_time_kst,
                            Open = x.opening_price,
                            High = x.high_price,
                            Low = x.low_price,
                            Close = x.trade_price,
                            Volume = x.candle_acc_trade_volume,
                        }).ToList();
                        dayQuotes.Reverse();
                        chartControl.Init(dayQuotes);
                        break;

                    case CandleInterval.OneWeek:
                        var weekCandleResult = upbitClient.QuotationCandles.GetWeeksCandlesAsync(symbol, null, defaultCount);
                        weekCandleResult.Wait();
                        var weekQuotes = weekCandleResult.Result.Select(x => new Quote
                        {
                            Date = x.candle_date_time_kst,
                            Open = x.opening_price,
                            High = x.high_price,
                            Low = x.low_price,
                            Close = x.trade_price,
                            Volume = x.candle_acc_trade_volume,
                        }).ToList();
                        weekQuotes.Reverse();
                        chartControl.Init(weekQuotes);
                        break;

                    case CandleInterval.OneMonth:
                        var monthCandleResult = upbitClient.QuotationCandles.GetMonthsCandlesAsync(symbol, null, defaultCount);
                        monthCandleResult.Wait();
                        var monthQuotes = monthCandleResult.Result.Select(x => new Quote
                        {
                            Date = x.candle_date_time_kst,
                            Open = x.opening_price,
                            High = x.high_price,
                            Low = x.low_price,
                            Close = x.trade_price,
                            Volume = x.candle_acc_trade_volume,
                        }).ToList();
                        monthQuotes.Reverse();
                        chartControl.Init(monthQuotes);
                        break;

                }
                chartControl.ViewStartPosition = Math.Max(chartControl.ViewEndPosition - SettingsMan.DefaultCandleCount * chartControl.ItemFullWidth, 0);

                return chartControl;
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
                return default!;
            }
        }

        public static ChartControl RefreshBithumbSpotChart(BithumbClient bithumbClient, BithumbSocketClient bithumbSocketClient)
        {
            try
            {
                var chartControl = new ChartControl();

                var symbol = Common.Pair.Symbol;
                var paymentCurrency = Common.Pair.QuoteAsset.ToBithumbPaymentCurrency();
                var interval = Common.ChartInterval.ToBithumbInterval();
                // 빗썸은 캔들의 개수를 무조건 3001개 가져오고, 따로 설정할 수도 없고, 그 이전의 캔들도 가져올 수가 없다.
                // 그러므로, 빗썸의 차트 추가 로드는 무효화된다.
                var candleResult = bithumbClient.Candlestick.GetCandlesticksAsync(symbol, paymentCurrency, interval);
                candleResult.Wait();
                var quotes = candleResult.Result.data.Select(x => new Quote
                {
                    Date = x.dateTime,
                    Open = x.open,
                    High = x.high,
                    Low = x.low,
                    Close = x.close,
                    Volume = x.volume,
                }).ToList();
                if (Common.ChartInterval == CandleInterval.FifteenMinutes || Common.ChartInterval == CandleInterval.OneWeek || Common.ChartInterval == CandleInterval.OneMonth)
                {
                    quotes = quotes.Merge(Common.ChartInterval);
                }
                chartControl.Init(quotes);
                chartControl.ViewStartPosition = Math.Max(chartControl.ViewEndPosition - SettingsMan.DefaultCandleCount * chartControl.ItemFullWidth, 0);

                bithumbSocketClient.Streams.SubscribeToTransactionAsync(symbol, (obj) =>
                {
                    if (obj.content.list == null || !obj.content.list.Any())
                    {
                        return;
                    }

                    DispatcherService.Invoke(() =>
                    {
                        foreach (var transaction in obj.content.list)
                        {
                            chartControl.UpdateQuote(Common.ChartInterval, transaction.contPrice, transaction.contQty);
                        }
                    });
                });

                return chartControl;
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
                return default!;
            }
        }
        #endregion

        #region Additional Chart (Binance, Upbit)
        public static void LoadAdditionalBinanceChart(BinanceClient binanceClient, ChartControl chartControl, PairMarketType marketType)
        {
            switch (marketType)
            {
                case PairMarketType.Spot:
                    LoadAdditionalBinanceSpotChart(binanceClient, chartControl);
                    break;

                case PairMarketType.Futures:
                    LoadAdditionalBinanceFuturesChart(binanceClient, chartControl);
                    break;

                case PairMarketType.CoinFutures:
                    LoadAdditionalBinanceCoinFuturesChart(binanceClient, chartControl);
                    break;

                default:
                    break;
            }
        }

        private static void LoadAdditionalBinanceSpotChart(BinanceClient binanceClient, ChartControl chartControl)
        {
            try
            {
                var symbol = Common.Pair.Symbol;
                var interval = Common.ChartInterval.ToBinanceInterval();
                var klineResult = binanceClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, interval, null, chartControl.Quotes[0].Date, Common.ChartLoadLimit);
                klineResult.Wait();
                var quotes = klineResult.Result.Data.Select(x => new Quote
                {
                    Date = x.OpenTime,
                    Open = x.OpenPrice,
                    High = x.HighPrice,
                    Low = x.LowPrice,
                    Close = x.ClosePrice,
                    Volume = x.Volume,
                }).ToList();
                if (Common.ChartInterval == CandleInterval.TenMinutes)
                {
                    quotes = quotes.Merge(CandleInterval.TenMinutes);
                }
                chartControl.ConcatenateQuotes(quotes);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        private static void LoadAdditionalBinanceFuturesChart(BinanceClient binanceClient, ChartControl chartControl)
        {
            try
            {
                var symbol = Common.Pair.Symbol;
                var interval = Common.ChartInterval.ToBinanceInterval();
                var klineResult = binanceClient.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol, interval, null, chartControl.Quotes[0].Date, Common.ChartLoadLimit);
                klineResult.Wait();
                var quotes = klineResult.Result.Data.Select(x => new Quote
                {
                    Date = x.OpenTime,
                    Open = x.OpenPrice,
                    High = x.HighPrice,
                    Low = x.LowPrice,
                    Close = x.ClosePrice,
                    Volume = x.Volume,
                }).ToList();
                if (Common.ChartInterval == CandleInterval.TenMinutes)
                {
                    quotes = quotes.Merge(CandleInterval.TenMinutes);
                }
                chartControl.ConcatenateQuotes(quotes);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        private static void LoadAdditionalBinanceCoinFuturesChart(BinanceClient binanceClient, ChartControl chartControl)
        {
            try
            {
                var symbol = Common.Pair.Symbol;
                var interval = Common.ChartInterval.ToBinanceInterval();
                var klineResult = binanceClient.CoinFuturesApi.ExchangeData.GetKlinesAsync(symbol, interval, null, chartControl.Quotes[0].Date, Common.ChartLoadLimit);
                klineResult.Wait();
                var quotes = klineResult.Result.Data.Select(x => new Quote
                {
                    Date = x.OpenTime,
                    Open = x.OpenPrice,
                    High = x.HighPrice,
                    Low = x.LowPrice,
                    Close = x.ClosePrice,
                    Volume = x.Volume,
                }).ToList();
                if (Common.ChartInterval == CandleInterval.TenMinutes)
                {
                    quotes = quotes.Merge(CandleInterval.TenMinutes);
                }
                chartControl.ConcatenateQuotes(quotes);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        public static void LoadAdditionalUpbitSpotChart(UpbitClient upbitClient, ChartControl chartControl)
        {
            try
            {
                var symbol = Common.Pair.Symbol;
                switch (Common.ChartInterval)
                {
                    case CandleInterval.OneMinute:
                    case CandleInterval.FiveMinutes:
                    case CandleInterval.FifteenMinutes:
                    case CandleInterval.ThirtyMinutes:
                    case CandleInterval.OneHour:
                        var minuteCandleResult = upbitClient.QuotationCandles.GetMinutesCandlesAsync(symbol, Common.ChartInterval.ToUpbitMinuteInterval(), chartControl.Quotes[0].Date.KstToUtc(), Common.ChartUpbitLoadLimit);
                        minuteCandleResult.Wait();
                        var minuteQuotes = minuteCandleResult.Result.Select(x => new Quote
                        {
                            Date = x.candle_date_time_kst,
                            Open = x.opening_price,
                            High = x.high_price,
                            Low = x.low_price,
                            Close = x.trade_price,
                            Volume = x.candle_acc_trade_volume
                        }).ToList();
                        minuteQuotes.Reverse();
                        chartControl.ConcatenateQuotes(minuteQuotes);
                        break;

                    case CandleInterval.OneDay:
                        var dayCandleResult = upbitClient.QuotationCandles.GetDaysCandlesAsync(symbol, chartControl.Quotes[0].Date.KstToUtc(), Common.ChartUpbitLoadLimit);
                        dayCandleResult.Wait();
                        var dayQuotes = dayCandleResult.Result.Select(x => new Quote
                        {
                            Date = x.candle_date_time_kst,
                            Open = x.opening_price,
                            High = x.high_price,
                            Low = x.low_price,
                            Close = x.trade_price,
                            Volume = x.candle_acc_trade_volume
                        }).ToList();
                        dayQuotes.Reverse();
                        chartControl.ConcatenateQuotes(dayQuotes);
                        break;

                    case CandleInterval.OneWeek:
                        var weekCandleResult = upbitClient.QuotationCandles.GetWeeksCandlesAsync(symbol, chartControl.Quotes[0].Date.KstToUtc(), Common.ChartUpbitLoadLimit);
                        weekCandleResult.Wait();
                        var weekQuotes = weekCandleResult.Result.Select(x => new Quote
                        {
                            Date = x.candle_date_time_kst,
                            Open = x.opening_price,
                            High = x.high_price,
                            Low = x.low_price,
                            Close = x.trade_price,
                            Volume = x.candle_acc_trade_volume
                        }).ToList();
                        weekQuotes.Reverse();
                        chartControl.ConcatenateQuotes(weekQuotes);
                        break;

                    case CandleInterval.OneMonth:
                        var monthCandleResult = upbitClient.QuotationCandles.GetMonthsCandlesAsync(symbol, chartControl.Quotes[0].Date.KstToUtc(), Common.ChartUpbitLoadLimit);
                        monthCandleResult.Wait();
                        var monthQuotes = monthCandleResult.Result.Select(x => new Quote
                        {
                            Date = x.candle_date_time_kst,
                            Open = x.opening_price,
                            High = x.high_price,
                            Low = x.low_price,
                            Close = x.trade_price,
                            Volume = x.candle_acc_trade_volume
                        }).ToList();
                        monthQuotes.Reverse();
                        chartControl.ConcatenateQuotes(monthQuotes);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }
        #endregion

        #region Update chart (Upbit)
        public static void UpdateUpbitSpotChart(UpbitClient upbitClient, ChartControl chartControl)
        {
            var symbol = Common.Pair.Symbol;
            switch (Common.ChartInterval)
            {
                case CandleInterval.OneMinute:
                case CandleInterval.ThreeMinutes:
                case CandleInterval.FiveMinutes:
                case CandleInterval.ThirtyMinutes:
                case CandleInterval.OneHour:
                    var minuteCandleResult = upbitClient.QuotationCandles.GetMinutesCandlesAsync(symbol, Common.ChartInterval.ToUpbitMinuteInterval());
                    minuteCandleResult.Wait();
                    var minuteCandle = minuteCandleResult.Result.ElementAt(0);
                    chartControl.UpdateQuote(new Quote()
                    {
                        Date = minuteCandle.candle_date_time_kst,
                        Open = minuteCandle.opening_price,
                        High = minuteCandle.high_price,
                        Low = minuteCandle.low_price,
                        Close = minuteCandle.trade_price,
                        Volume = minuteCandle.candle_acc_trade_volume
                    });
                    break;

                case CandleInterval.OneDay:
                    var dayCandleResult = upbitClient.QuotationCandles.GetDaysCandlesAsync(symbol);
                    dayCandleResult.Wait();
                    var dayCandle = dayCandleResult.Result.ElementAt(0);
                    chartControl.UpdateQuote(new Quote()
                    {
                        Date = dayCandle.candle_date_time_kst,
                        Open = dayCandle.opening_price,
                        High = dayCandle.high_price,
                        Low = dayCandle.low_price,
                        Close = dayCandle.trade_price,
                        Volume = dayCandle.candle_acc_trade_volume
                    });
                    break;

                case CandleInterval.OneWeek:
                    var weekCandleResult = upbitClient.QuotationCandles.GetWeeksCandlesAsync(symbol);
                    weekCandleResult.Wait();
                    var weekCandle = weekCandleResult.Result.ElementAt(0);
                    chartControl.UpdateQuote(new Quote()
                    {
                        Date = weekCandle.candle_date_time_kst,
                        Open = weekCandle.opening_price,
                        High = weekCandle.high_price,
                        Low = weekCandle.low_price,
                        Close = weekCandle.trade_price,
                        Volume = weekCandle.candle_acc_trade_volume
                    });
                    break;

                case CandleInterval.OneMonth:
                    var monthCandleResult = upbitClient.QuotationCandles.GetMonthsCandlesAsync(symbol);
                    monthCandleResult.Wait();
                    var monthCandle = monthCandleResult.Result.ElementAt(0);
                    chartControl.UpdateQuote(new Quote()
                    {
                        Date = monthCandle.candle_date_time_kst,
                        Open = monthCandle.opening_price,
                        High = monthCandle.high_price,
                        Low = monthCandle.low_price,
                        Close = monthCandle.trade_price,
                        Volume = monthCandle.candle_acc_trade_volume
                    });
                    break;
            }
        }
        #endregion
    }
}
