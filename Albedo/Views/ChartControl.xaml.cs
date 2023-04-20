using Albedo.Utils;

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
        public List<Quote> Quotes = new();

        private Pen longPen = new(new SolidColorBrush(Color.FromRgb(59, 207, 134)), 1.0);
        private Pen shortPen = new(new SolidColorBrush(Color.FromRgb(237, 49, 97)), 1.0);
        private Brush longBrush = new SolidColorBrush(Color.FromRgb(59, 207, 134));
        private Brush shortBrush = new SolidColorBrush(Color.FromRgb(237, 49, 97));
        private int candleMargin = 1;

        public int Start = 0;
        public int End = 0;
        public int ViewCountMin = 10;
        public int ViewCountMax = 1000;
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

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ViewCount <= 0)
            {
                return;
            }

            base.OnRender(drawingContext);

            var itemWidth = ActualWidth / ViewCount;
            var max = Quotes.Skip(Start).Take(ViewCount).Max(x => x.High);
            var min = Quotes.Skip(Start).Take(ViewCount).Min(x => x.Low);

            for (int i = Start; i < End; i++)
            {
                var quote = Quotes[i];
                var viewIndex = i - Start;

                // Draw Candle
                drawingContext.DrawLine(
                    quote.Open < quote.Close ? longPen : shortPen,
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.High - min) / (max - min))),
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.Low - min) / (max - min))));
                drawingContext.DrawRectangle(
                    quote.Open < quote.Close ? longBrush : shortBrush,
                    quote.Open < quote.Close ? longPen : shortPen,
                    new Rect(
                    new Point(itemWidth * viewIndex + candleMargin, ActualHeight * (double)(1.0m - (quote.Open - min) / (max - min))),
                    new Point(itemWidth * (viewIndex + 1) - candleMargin, ActualHeight * (double)(1.0m - (quote.Close - min) / (max - min)))
                    ));
            }
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
            var itemWidth = ActualWidth / ViewCount;
            Vector diff = e.GetPosition(Parent as Window) - CurrentMousePosition;
            if (IsMouseCaptured)
            {
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
            CurrentMousePosition = e.GetPosition(Parent as Window);
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
