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

namespace Albedo.Views
{
    /// <summary>
    /// ChartControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChartControl : UserControl
    {
        public CandleContent CandleContent { get; set; } = default!;
        public CandleAxisContent CandleAxisContent { get; set; } = default!;
        public VolumeContent VolumeContent { get; set; } = default!;
        public VolumeAxisContent VolumeAxisContent { get; set; } = default!;

        public List<Quote> Quotes = new();
        public List<Models.Indicator> Indicators = new();
        private int itemMargin = 1;

        public int Start = 0;
        public int End = 0;
        public int ViewCountMin = 10;
        public int ViewCountMax = 500;
        public int ViewCount => End - Start;
        public int TotalCount = 0;
        public Point CurrentMousePosition;

        public ChartControl()
        {
            InitializeComponent();
        }

        public void Init(List<Quote> quotes)
        {
            Quotes = quotes;
            Start = 0;
            End = quotes.Count;
            TotalCount = quotes.Count;

            CandleContent = new CandleContent();
            CandleAxisContent = new CandleAxisContent();
            VolumeContent = new VolumeContent();
            VolumeAxisContent = new VolumeAxisContent();
            CandleContent.ItemMargin = itemMargin;
            VolumeContent.ItemMargin = itemMargin;
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
                Start++;
                End++;
                TotalCount++;
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
            TotalCount = Quotes.Count;
            var additionalQuoteCount = Quotes.Count - preQuoteCount;
            Start += additionalQuoteCount;
            End += additionalQuoteCount;

            CalculateIndicators();
            InvalidateVisual();
        }

        public void CalculateIndicators()
        {
            var results = Quotes.GetEma(112);
            Indicators.Clear();
            foreach(var result in results)
            {
                var indicator = result.Ema == null ?
                    new Models.Indicator() { Date = result.Date, Value = 0 } :
                    new Models.Indicator() { Date = result.Date, Value = (decimal)result.Ema.Value };
                Indicators.Add(indicator);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ViewCount <= 0)
            {
                return;
            }

            base.OnRender(drawingContext);

            foreach (var radioButton in IntervalGrid.Children.OfType<RadioButton>().Where(radioButton => radioButton.CommandParameter.Equals(Settings.Default.Interval)))
            {
                radioButton.IsChecked = true;
                break;
            }

            if (TotalCount > 0 && Start <= 0)
            {
                Common.ChartAdditionalLoad.Invoke();
            }

            CandleContent.Quotes = CandleAxisContent.Quotes = Quotes;
            CandleContent.Indicators = Indicators;
            CandleContent.Start = CandleAxisContent.Start = Start;
            CandleContent.End = CandleAxisContent.End = End;
            VolumeContent.Quotes = VolumeAxisContent.Quotes = Quotes;
            VolumeContent.Start = VolumeAxisContent.Start = Start;
            VolumeContent.End = VolumeAxisContent.End = End;
            CandleContent.InvalidateVisual();
            CandleAxisContent.InvalidateVisual();
            VolumeContent.InvalidateVisual();
            VolumeAxisContent.InvalidateVisual();
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scaleUnit = Math.Max(1, ViewCount * Math.Abs(e.Delta) / 2000);
            if (e.Delta > 0) // Zoom-in
            {
                if (ViewCount <= ViewCountMin)
                {
                    return;
                }

                Start = Math.Min(TotalCount - ViewCountMin, Start + scaleUnit);
                End = Math.Max(ViewCountMin, End - scaleUnit);
            }
            else // Zoom-out
            {
                if (ViewCount >= ViewCountMax)
                {
                    return;
                }

                Start = Math.Max(0, Start - scaleUnit);
                End = Math.Min(TotalCount, End + scaleUnit);
            }

            InvalidateVisual();
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var itemWidth = CandleChart.ActualWidth / ViewCount;
                Vector diff = e.GetPosition(null) - CurrentMousePosition;
                var moveUnit = (int)(diff.X / itemWidth / 20);
                if (diff.X > 0) // Graph Move Left
                {
                    if (Start <= moveUnit) // Max Move Case
                    {
                        moveUnit = Start;
                    }
                    Start -= moveUnit;
                    End -= moveUnit;
                    InvalidateVisual();
                }
                else if (diff.X < 0) // Graph Move Right
                {
                    if (End >= TotalCount + moveUnit) // Max Move Case
                    {
                        moveUnit = End - TotalCount;
                    }
                    Start -= moveUnit;
                    End -= moveUnit;
                    InvalidateVisual();
                }
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentMousePosition = e.GetPosition(null);
            CaptureMouse();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }
    }
}
