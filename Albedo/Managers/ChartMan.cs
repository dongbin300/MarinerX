using Albedo.Extensions;
using Albedo.Utils;
using Albedo.Views;

using Binance.Net.Clients;
using Binance.Net.Enums;

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
        public static (ChartControl, int) RefreshBinanceSpotChart(BinanceClient binanceClient, BinanceSocketClient binanceSocketClient, int subId)
        {
            try
            {
                var chartControl = new ChartControl();
                var klineResult = binanceClient.SpotApi.ExchangeData.GetKlinesAsync(Common.Pair.Symbol, Common.ChartInterval, null, null, Common.ChartLoadLimit);
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

                binanceSocketClient.UnsubscribeAsync(subId);
                var klineUpdateResult = binanceSocketClient.SpotApi.ExchangeData.SubscribeToKlineUpdatesAsync(Common.Pair.Symbol, Common.ChartInterval, (obj) =>
                {
                    DispatcherService.Invoke(() =>
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

        public static (ChartControl, int) RefreshBinanceFuturesChart(BinanceClient binanceClient, BinanceSocketClient binanceSocketClient, int subId)
        {
            try
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

                binanceSocketClient.UsdFuturesStreams.UnsubscribeAsync(subId);
                var klineUpdateResult = binanceSocketClient.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(Common.Pair.Symbol, Common.ChartInterval, (obj) =>
                {
                    DispatcherService.Invoke(() =>
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

        public static (ChartControl, int) RefreshBinanceCoinFuturesChart(BinanceClient binanceClient, BinanceSocketClient binanceSocketClient, int subId)
        {
            try
            {
                var chartControl = new ChartControl();
                var klineResult = binanceClient.CoinFuturesApi.ExchangeData.GetKlinesAsync(Common.Pair.Symbol, Common.ChartInterval, null, null, Common.ChartLoadLimit);
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

                binanceSocketClient.CoinFuturesStreams.UnsubscribeAsync(subId);
                var klineUpdateResult = binanceSocketClient.CoinFuturesStreams.SubscribeToKlineUpdatesAsync(Common.Pair.Symbol, Common.ChartInterval, (obj) =>
                {
                    DispatcherService.Invoke(() =>
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
                    case KlineInterval.OneMinute:
                    case KlineInterval.FiveMinutes:
                    case KlineInterval.FifteenMinutes:
                    case KlineInterval.ThirtyMinutes:
                    case KlineInterval.OneHour:
                    case KlineInterval.FourHour:
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

                    case KlineInterval.OneDay:
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

                    case KlineInterval.OneWeek:
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

                    case KlineInterval.OneMonth:
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
                chartControl.ViewStartPosition = Math.Max(chartControl.ViewEndPosition - Common.ChartDefaultViewCount * chartControl.ItemFullWidth, 0);

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

                // 1주, 1월 캔들은 지원하지 않는다 (TODO)
                var candleResult = bithumbClient.Candlestick.GetCandlesticksAsync(symbol, paymentCurrency, interval);
                candleResult.Wait();
                chartControl.Init(candleResult.Result.data.Select(x => new Quote
                {
                    Date = x.dateTime,
                    Open = x.open,
                    High = x.high,
                    Low = x.low,
                    Close = x.close,
                    Volume = x.volume,
                }).ToList());
                chartControl.ViewStartPosition = Math.Max(chartControl.ViewEndPosition - Common.ChartDefaultViewCount * chartControl.ItemFullWidth, 0);

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
                            chartControl.UpdateQuote(interval, transaction.contPrice, transaction.contQty);
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

        public static void LoadAdditionalBinanceSpotChart(BinanceClient binanceClient, ChartControl chartControl)
        {
            try
            {
                var klineResult = binanceClient.SpotApi.ExchangeData.GetKlinesAsync(Common.Pair.Symbol, Common.ChartInterval, null, chartControl.Quotes[0].Date, Common.ChartLoadLimit);
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
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        public static void LoadAdditionalBinanceFuturesChart(BinanceClient binanceClient, ChartControl chartControl)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ChartMan), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        public static void LoadAdditionalBinanceCoinFuturesChart(BinanceClient binanceClient, ChartControl chartControl)
        {
            try
            {
                var klineResult = binanceClient.CoinFuturesApi.ExchangeData.GetKlinesAsync(Common.Pair.Symbol, Common.ChartInterval, null, chartControl.Quotes[0].Date, Common.ChartLoadLimit);
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
                switch (Common.ChartInterval)
                {
                    case KlineInterval.OneMinute:
                    case KlineInterval.FiveMinutes:
                    case KlineInterval.FifteenMinutes:
                    case KlineInterval.ThirtyMinutes:
                    case KlineInterval.OneHour:
                    case KlineInterval.FourHour:
                        var minuteCandleResult = upbitClient.QuotationCandles.GetMinutesCandlesAsync(Common.Pair.Symbol, Common.ChartInterval.ToUpbitMinuteInterval(), chartControl.Quotes[0].Date.KstToUtc(), Common.ChartUpbitLoadLimit);
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

                    case KlineInterval.OneDay:
                        var dayCandleResult = upbitClient.QuotationCandles.GetDaysCandlesAsync(Common.Pair.Symbol, chartControl.Quotes[0].Date.KstToUtc(), Common.ChartUpbitLoadLimit);
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

                    case KlineInterval.OneWeek:
                        var weekCandleResult = upbitClient.QuotationCandles.GetWeeksCandlesAsync(Common.Pair.Symbol, chartControl.Quotes[0].Date.KstToUtc(), Common.ChartUpbitLoadLimit);
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

                    case KlineInterval.OneMonth:
                        var monthCandleResult = upbitClient.QuotationCandles.GetMonthsCandlesAsync(Common.Pair.Symbol, chartControl.Quotes[0].Date.KstToUtc(), Common.ChartUpbitLoadLimit);
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

        public static void UpdateUpbitSpotChart(UpbitClient upbitClient, ChartControl chartControl)
        {
            var symbol = Common.Pair.Symbol;
            switch (Common.ChartInterval)
            {
                case KlineInterval.OneMinute:
                case KlineInterval.ThreeMinutes:
                case KlineInterval.FiveMinutes:
                case KlineInterval.ThirtyMinutes:
                case KlineInterval.OneHour:
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

                case KlineInterval.OneDay:
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

                case KlineInterval.OneWeek:
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

                case KlineInterval.OneMonth:
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
    }
}
