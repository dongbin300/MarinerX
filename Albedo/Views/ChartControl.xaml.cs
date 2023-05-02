using Albedo.Utils;
using Albedo.Views.Contents;

using Skender.Stock.Indicators;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using static Albedo.Apis.WinApi;

namespace Albedo.Views
{
    /// <summary>
    /// ChartControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChartControl : UserControl
    {
        private System.Timers.Timer chartControlTimer = new System.Timers.Timer(25);

        public CandleContent CandleContent { get; set; } = default!;
        public CandleAxisContent CandleAxisContent { get; set; } = default!;
        public VolumeContent VolumeContent { get; set; } = default!;
        public VolumeAxisContent VolumeAxisContent { get; set; } = default!;

        public List<Quote> Quotes = new();
        public List<Models.Indicator> Indicators = new();
        public int TotalCount => Quotes.Count;

        public double ChartWidth => Quotes.Count * ItemFullWidth;
        public double ViewStartPosition { get; set; } = 0;
        public double ViewEndPosition { get; set; } = 0;
        public double ViewWidth => ViewEndPosition - ViewStartPosition;

        public int ItemFullWidth => Common.ChartItemFullWidth;
        public double ItemMarginPercent => Common.ChartItemMarginPercent;
        public double ItemWidth => ItemFullWidth * (1 - ItemMarginPercent);
        public double ItemMargin => ItemFullWidth * ItemMarginPercent;

        public int StartItemIndex => (int)(Quotes.Count * (ViewStartPosition / ChartWidth));
        public int EndItemIndex => (int)(Quotes.Count * (ViewEndPosition / ChartWidth));
        public int ViewItemCount => EndItemIndex - StartItemIndex + 1;

        public double ActualItemFullWidth => ActualWidth / ViewItemCount;
        public double ActualItemWidth => ActualItemFullWidth * (1 - ItemMarginPercent);
        public double ActualItemMargin => ActualItemFullWidth * ItemMarginPercent;

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

            CandleContent = new CandleContent();
            CandleAxisContent = new CandleAxisContent();
            VolumeContent = new VolumeContent();
            VolumeAxisContent = new VolumeAxisContent();
            CandleChart.Content = CandleContent;
            CandleChartAxis.Content = CandleAxisContent;
            VolumeChart.Content = VolumeContent;
            VolumeChartAxis.Content = VolumeAxisContent;

            CalculateIndicators();
            InvalidateVisual();
        }

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

            DispatcherService.Invoke(InvalidateVisual);
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
            InvalidateVisual();
        }

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

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ViewItemCount <= 1)
            {
                return;
            }

            base.OnRender(drawingContext);

            foreach (var radioButton in IntervalGrid.Children.OfType<RadioButton>().Where(radioButton => radioButton.CommandParameter.Equals(Settings.Default.Interval)))
            {
                radioButton.IsChecked = true;
                break;
            }

            if (TotalCount > 0 && ViewStartPosition <= ItemFullWidth)
            {
                Common.ChartAdditionalLoad.Invoke();
            }

            CandleContent.Quotes = CandleAxisContent.Quotes = Quotes;
            CandleContent.Indicators = Indicators;
            CandleContent.ViewStartPosition = CandleAxisContent.ViewStartPosition = ViewStartPosition;
            CandleContent.ViewEndPosition = CandleAxisContent.ViewEndPosition = ViewEndPosition;
            VolumeContent.Quotes = VolumeAxisContent.Quotes = Quotes;
            VolumeContent.ViewStartPosition = VolumeAxisContent.ViewStartPosition = ViewStartPosition;
            VolumeContent.ViewEndPosition = VolumeAxisContent.ViewEndPosition = ViewEndPosition;
            CandleContent.InvalidateVisual();
            CandleAxisContent.InvalidateVisual();
            VolumeContent.InvalidateVisual();
            VolumeAxisContent.InvalidateVisual();
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var unit = 300;
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

            InvalidateVisual();
        }

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
            StartMousePosition = currentMousePosition;
            var movePosition = diff.X / ActualItemFullWidth * ItemFullWidth;

            DispatcherService.Invoke(() =>
            {
                if (diff.X > 0) // Graph Move Left
                {
                    if (ViewStartPosition - movePosition >= 0)
                    {
                        ViewStartPosition -= movePosition; // to do
                        ViewEndPosition -= movePosition;
                        InvalidateVisual();
                    }
                }
                else if (diff.X < 0) // Graph Move Right
                {
                    if (ViewEndPosition - movePosition <= ChartWidth)
                    {
                        ViewStartPosition -= movePosition; // to do
                        ViewEndPosition -= movePosition;
                        InvalidateVisual();
                    }
                }
            });
        }
    }
}
