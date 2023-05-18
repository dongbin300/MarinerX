using Albedo.Enums;
using Albedo.Utils;

using Bithumb.Net.Enums;

using Skender.Stock.Indicators;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using static Albedo.Apis.WinApi;

namespace Albedo.Views
{
    /// <summary>
    /// ChartControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChartControl : UserControl
    {
        private System.Timers.Timer chartControlTimer = new System.Timers.Timer(5);

        public List<Quote> Quotes = new();
        public List<Models.Indicator> Indicators = new();
        public int TotalCount => Quotes.Count;

        public float ChartWidth => Quotes.Count * ItemFullWidth;
        public float ViewStartPosition { get; set; } = 0;
        public float ViewEndPosition { get; set; } = 0;
        public float ViewWidth => ViewEndPosition - ViewStartPosition;

        public int ItemFullWidth => Common.ChartItemFullWidth;
        public float ItemMarginPercent => Common.ChartItemMarginPercent;
        public float ItemWidth => ItemFullWidth * (1 - ItemMarginPercent);
        public float ItemMargin => ItemFullWidth * ItemMarginPercent;

        public int StartItemIndex => (int)(Quotes.Count * (ViewStartPosition / ChartWidth));
        public int EndItemIndex => (int)(Quotes.Count * (ViewEndPosition / ChartWidth));
        public int ViewItemCount => EndItemIndex - StartItemIndex + 1;

        public float ActualItemFullWidth => (float)ActualWidth / ViewItemCount;
        public float ActualItemWidth => ActualItemFullWidth * (1 - ItemMarginPercent);
        public float ActualItemMargin => ActualItemFullWidth * ItemMarginPercent;

        public int ViewCountMin = 10;
        public int ViewCountMax = 500;

        public Point StartMousePosition;

        public ChartControl()
        {
            InitializeComponent();
            chartControlTimer.Elapsed += ChartControlTimer_Elapsed;
        }

        public void Init(List<Quote> quotes)
        {
            Quotes = quotes;
            ViewStartPosition = ChartWidth - ItemFullWidth * 60;
            ViewEndPosition = ChartWidth;

            CalculateIndicators();
            Render();
        }

        #region Quote
        public void UpdateQuote(Quote quote)
        {
            var lastQuote = Quotes[^1];
            if (lastQuote.Date.Equals(quote.Date))
            {
                lastQuote.High = quote.High;
                lastQuote.Low = quote.Low;
                lastQuote.Close = quote.Close;
                lastQuote.Volume = quote.Volume;
            }
            else
            {
                Quotes.Add(quote);
                ViewStartPosition += ItemFullWidth;
                ViewEndPosition += ItemFullWidth;
            }

            Render();
        }

        /// <summary>
        /// Merge quote in real time
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="fromInterval"></param>
        /// <param name="toInterval"></param>
        public void UpdateQuote(Quote quote, CandleInterval toInterval)
        {
            var backtrackCount = 0;
            switch (toInterval)
            {
                case CandleInterval.OneWeek:
                    backtrackCount = (int)quote.Date.DayOfWeek;
                    break;

                case CandleInterval.OneMonth:
                    backtrackCount = quote.Date.Day - 1;
                    break;

                default:
                    backtrackCount = toInterval switch
                    {
                        CandleInterval.ThreeMinutes => quote.Date.Minute % 3, // 1m * 3
                        CandleInterval.FiveMinutes => quote.Date.Minute % 5, // 1m * 5
                        CandleInterval.TenMinutes => quote.Date.Minute % 2, // 5m * 2
                        CandleInterval.FifteenMinutes => quote.Date.Minute % 3, // 5m * 3
                        CandleInterval.ThirtyMinutes => quote.Date.Minute % 6, // 5m * 6 | 10m * 3 | 15m * 2
                        _ => 0
                    };
                    break;
            }

            var lastQuote = Quotes[^1];
            if (backtrackCount == 0)
            {
                if (lastQuote.Date.Equals(quote.Date)) // Update quote
                {
                    lastQuote.High = quote.High;
                    lastQuote.Low = quote.Low;
                    lastQuote.Close = quote.Close;
                    lastQuote.Volume = quote.Volume;
                }
                else // New quote
                {
                    Quotes.Add(quote);
                    ViewStartPosition += ItemFullWidth;
                    ViewEndPosition += ItemFullWidth;
                }
            }
            else // Merge with past quotes
            {
                var backtrackQuotes = Quotes.TakeLast(backtrackCount);
                lastQuote.High = Math.Max(backtrackQuotes.Max(x => x.High), quote.High);
                lastQuote.Low = Math.Min(backtrackQuotes.Min(x => x.Low), quote.Low);
                lastQuote.Close = quote.Close;
                lastQuote.Volume = backtrackQuotes.Sum(x => x.Volume) + quote.Volume;
            }

            Render();
        }

        /// <summary>
        /// Update quote whenever an order is placed (for Bithumb)
        /// </summary>
        /// <param name="price"></param>
        public void UpdateQuote(CandleInterval interval, decimal price, decimal volume)
        {
            var now = DateTime.Now;
            var lastQuote = Quotes[^1];
            var intervalSeconds = interval switch
            {
                CandleInterval.OneMinute => 60,
                CandleInterval.ThreeMinutes => 180,
                CandleInterval.FiveMinutes => 300,
                CandleInterval.TenMinutes => 600,
                CandleInterval.FifteenMinutes => 900,
                CandleInterval.ThirtyMinutes => 1800,
                CandleInterval.OneHour => 3600,
                CandleInterval.OneDay => 86400,
                CandleInterval.OneWeek => 604800,
                CandleInterval.OneMonth => 2592000,
                _ => 60
            };

            if ((now - lastQuote.Date).TotalSeconds >= intervalSeconds) // New quote
            {
                var quote = new Quote
                {
                    Date = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0),
                    Open = price,
                    High = price,
                    Low = price,
                    Close = price,
                    Volume = volume
                };
                Quotes.Add(quote);
                ViewStartPosition += ItemFullWidth;
                ViewEndPosition += ItemFullWidth;
            }
            else // Accumulate in last quote
            {
                lastQuote.High = Math.Max(lastQuote.High, price);
                lastQuote.Low = Math.Min(lastQuote.Low, price);
                lastQuote.Close = price;
                lastQuote.Volume += volume;
            }

            Render();
        }

        public void ConcatenateQuotes(List<Quote> quotes)
        {
            var preQuoteCount = Quotes.Count;
            for (int i = quotes.Count - 1; i >= 0; i--)
            {
                var quote = quotes[i];
                var _quote = Quotes.Find(q => q.Date.Equals(quote.Date));
                if (_quote == null)
                {
                    Quotes.Insert(0, quote);
                }
            }
            var additionalQuoteCount = Quotes.Count - preQuoteCount;
            ViewStartPosition += additionalQuoteCount * ItemFullWidth;
            ViewEndPosition += additionalQuoteCount * ItemFullWidth;

            CalculateIndicators();
            Render();
        }
        #endregion

        #region Indicator
        public void CalculateIndicators()
        {
            var results = Quotes.GetEma(112);
            Indicators.Clear();
            foreach (var result in results)
            {
                var indicator = result.Ema == null ?
                    new Models.Indicator() { Date = result.Date, Value = 0 } :
                    new Models.Indicator() { Date = result.Date, Value = (decimal)result.Ema.Value };
                Indicators.Add(indicator);
            }
        }
        #endregion

        #region User Event

        #region Zoom
        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var unit = (ViewEndPosition - ViewStartPosition) * 0.1f;
            if (e.Delta > 0) // Zoom-in
            {
                if (ViewItemCount <= ViewCountMin)
                {
                    return;
                }

                ViewStartPosition += unit;
                ViewEndPosition -= unit;
            }
            else // Zoom-out
            {
                if (ViewItemCount >= ViewCountMax)
                {
                    return;
                }

                ViewStartPosition = Math.Max(0, ViewStartPosition - unit);
                ViewEndPosition = Math.Min(ChartWidth, ViewEndPosition + unit);
            }

            Render();
        }
        #endregion

        #region Scroll
        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartMousePosition = GetCursorPosition();
            chartControlTimer.Start();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chartControlTimer.Stop();
        }

        private void ChartControlTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var currentMousePosition = GetCursorPosition();
            Vector diff = currentMousePosition - StartMousePosition;

            if (diff.X == 0)
            {
                return;
            }

            StartMousePosition = currentMousePosition;
            var movePosition = (float)diff.X / ActualItemFullWidth * ItemFullWidth * 1.3f;

            DispatcherService.Invoke(() =>
            {
                if (diff.X > 0) // Graph Move Left
                {
                    if (ViewStartPosition - movePosition < 0) // Reach left-end
                    {
                        movePosition = ViewStartPosition;
                    }
                    ViewStartPosition -= movePosition;
                    ViewEndPosition -= movePosition;
                    Render();
                }
                else if (diff.X < 0) // Graph Move Right
                {
                    if (ViewEndPosition - movePosition > ChartWidth) // Reach right-end
                    {
                        movePosition = ViewEndPosition - ChartWidth;
                    }
                    ViewStartPosition -= movePosition;
                    ViewEndPosition -= movePosition;
                    Render();
                }
            });
        }
        #endregion

        #endregion

        #region Main Render
        public void Render()
        {
            foreach (var radioButton in IntervalGrid.Children.OfType<RadioButton>().Where(radioButton => radioButton.CommandParameter.Equals(Settings.Default.Interval)))
            {
                radioButton.IsChecked = true;
                break;
            }

            if (TotalCount > 0 && ViewStartPosition <= ItemFullWidth)
            {
                Common.ChartAdditionalLoad.Invoke();
            }

            CandleChart.InvalidateVisual();
            CandleChartAxis.InvalidateVisual();
            VolumeChart.InvalidateVisual();
            VolumeChartAxis.InvalidateVisual();
        }
        #endregion

        #region Chart Content Render
        private void CandleChart_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (ViewItemCount <= 1)
            {
                return;
            }

            var actualWidth = (float)CandleChart.ActualWidth;
            var actualHeight = (float)CandleChart.ActualHeight;
            var actualItemFullWidth = actualWidth / ViewItemCount;

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            var priceMax = Quotes.Skip(StartItemIndex).Take(ViewItemCount).Max(x => x.High);
            var priceMin = Quotes.Skip(StartItemIndex).Take(ViewItemCount).Min(x => x.Low);

            // Draw Grid
            var gridLevel = 4; // 4등분
            for (int i = 0; i <= gridLevel; i++)
            {
                if (i > 0)
                {
                    canvas.DrawLine(
                                      new SKPoint(0, actualHeight * ((float)i / gridLevel)),
                                      new SKPoint(actualWidth, actualHeight * ((float)i / gridLevel)),
                                      DrawingTools.GridPaint
                                   );
                }
            }

            for (int i = StartItemIndex; i < EndItemIndex; i++)
            {
                var quote = Quotes[i];
                var viewIndex = i - StartItemIndex;

                // Draw Price Candlestick
                canvas.DrawLine(
                    new SKPoint(
                        actualItemFullWidth * (viewIndex + 0.5f),
                        actualHeight * (float)(1.0m - (quote.High - priceMin) / (priceMax - priceMin))),
                    new SKPoint(
                        actualItemFullWidth * (viewIndex + 0.5f),
                        actualHeight * (float)(1.0m - (quote.Low - priceMin) / (priceMax - priceMin))),
                    quote.Open < quote.Close ? DrawingTools.LongPaint : DrawingTools.ShortPaint);
                canvas.DrawRect(
                    new SKRect(
                        actualItemFullWidth * viewIndex + (float)ActualItemMargin / 2,
                        actualHeight * (float)(1.0m - (quote.Open - priceMin) / (priceMax - priceMin)),
                        actualItemFullWidth * (viewIndex + 1) - (float)ActualItemMargin / 2,
                        actualHeight * (float)(1.0m - (quote.Close - priceMin) / (priceMax - priceMin))
                        ),
                    quote.Open < quote.Close ? DrawingTools.LongPaint : DrawingTools.ShortPaint
                    );

                // Draw Indicators
                if (i < Indicators.Count && i >= 1)
                {
                    var preIndicator = Indicators[i - 1];
                    var indicator = Indicators[i];

                    if (preIndicator != null && indicator != null && preIndicator.Value != 0 && indicator.Value != 0)
                    {
                        canvas.DrawLine(
                            new SKPoint(
                                (float)ActualItemFullWidth * (viewIndex - 0.5f),
                                (float)ActualHeight * (float)(1.0m - (preIndicator.Value - priceMin) / (priceMax - priceMin))),
                            new SKPoint(
                                (float)ActualItemFullWidth * (viewIndex + 0.5f),
                                (float)ActualHeight * (float)(1.0m - (indicator.Value - priceMin) / (priceMax - priceMin))),
                            new SKPaint() { Color = SKColors.Yellow }
                            );
                    }
                }
            }
        }

        private void CandleChartAxis_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (ViewItemCount <= 1)
            {
                return;
            }

            var actualHeight = (float)CandleChartAxis.ActualHeight;

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            var priceMax = Quotes.Skip(StartItemIndex).Take(ViewItemCount).Max(x => x.High);
            var priceMin = Quotes.Skip(StartItemIndex).Take(ViewItemCount).Min(x => x.Low);

            // Draw Grid
            var gridLevel = 4; // 4등분
            for (int i = 0; i <= gridLevel; i++)
            {
                var gridPriceString = NumberUtil.ToRoundedValueString(priceMin + (priceMax - priceMin) * ((decimal)(gridLevel - i) / gridLevel));

                canvas.DrawText(
                    gridPriceString,
                    5,
                    actualHeight * ((float)i / gridLevel) - 7,
                    DrawingTools.GridTextFont,
                    DrawingTools.GridFontPaint);
            }

            // Draw Current Price Ticker
            canvas.DrawText(
                Quotes[EndItemIndex - 1].Close.ToString(),
                5,
                actualHeight * (float)(1.0m - (Quotes[EndItemIndex - 1].Close - priceMin) / (priceMax - priceMin)),
                DrawingTools.CurrentTickerFont,
                Quotes[EndItemIndex - 1].Open < Quotes[EndItemIndex - 1].Close ? DrawingTools.LongPaint : DrawingTools.ShortPaint
                );
        }

        private void VolumeChart_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (ViewItemCount <= 1)
            {
                return;
            }

            var actualWidth = (float)VolumeChart.ActualWidth;
            var actualHeight = (float)VolumeChart.ActualHeight;
            var actualItemFullWidth = actualWidth / ViewItemCount;

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            var volumeMax = Quotes.Skip(StartItemIndex).Take(ViewItemCount).Max(x => x.Volume);

            // Draw Grid
            var gridLevel = 2; // 2등분
            for (int i = 0; i <= gridLevel; i++)
            {
                if (i > 0)
                {
                    canvas.DrawLine(
                        new SKPoint(0, actualHeight * ((float)i / gridLevel)),
                        new SKPoint(actualWidth, actualHeight * ((float)i / gridLevel)),
                        DrawingTools.GridPaint
                        );
                }
            }

            for (int i = StartItemIndex; i < EndItemIndex; i++)
            {
                var quote = Quotes[i];
                var viewIndex = i - StartItemIndex;

                // Draw Volume Histogram
                canvas.DrawRect(
                    new SKRect(
                        actualItemFullWidth * viewIndex + ActualItemMargin / 2,
                        actualHeight * (float)(1.0m - quote.Volume / volumeMax),
                        actualItemFullWidth * (viewIndex + 1) - ActualItemMargin / 2,
                        actualHeight
                        ),
                    quote.Open < quote.Close ? DrawingTools.LongPaint : DrawingTools.ShortPaint
                    );
            }
        }

        private void VolumeChartAxis_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (ViewItemCount <= 1)
            {
                return;
            }

            var actualHeight = (float)VolumeChartAxis.ActualHeight;

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            var volumeMax = Quotes.Skip(StartItemIndex).Take(ViewItemCount).Max(x => x.Volume);

            // Draw Grid
            var gridLevel = 2; // 2등분
            for (int i = 0; i <= gridLevel; i++)
            {
                var gridPriceString = Math.Round(volumeMax * ((decimal)(gridLevel - i) / gridLevel), 0).ToString();

                canvas.DrawText(
                    gridPriceString,
                    5,
                    (actualHeight - 20) * ((float)i / gridLevel) - 7 + 10,
                    DrawingTools.GridTextFont,
                    DrawingTools.GridFontPaint
                    );
            }

            // Draw Current Volume Ticker
            canvas.DrawText(
                Quotes[EndItemIndex - 1].Volume.ToString(),
                5,
                (actualHeight - 20) * (float)(1.0m - Quotes[EndItemIndex - 1].Volume / volumeMax) - 8 + 10,
                DrawingTools.CurrentTickerFont,
                Quotes[EndItemIndex - 1].Open < Quotes[EndItemIndex - 1].Close ? DrawingTools.LongPaint : DrawingTools.ShortPaint
                );
        }
        #endregion
    }
}
