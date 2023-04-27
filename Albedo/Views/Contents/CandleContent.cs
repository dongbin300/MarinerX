using Albedo.Utils;

using Skender.Stock.Indicators;

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Albedo.Views.Contents
{
    public class CandleContent : ContentControl
    {
        public List<Quote> Quotes { get; set; } = new();
        public List<Models.Indicator> Indicators { get; set; } = new();
        public int Start { get; set; } = 0;
        public int End { get; set; } = 0;
        public int ViewCount => End - Start;
        public int ItemMargin { get; set; } = 0;

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ViewCount <= 0)
            {
                return;
            }

            base.OnRender(drawingContext);

            var itemWidth = ActualWidth / ViewCount;
            var priceMax = Quotes.Skip(Start).Take(ViewCount).Max(x => x.High);
            var priceMin = Quotes.Skip(Start).Take(ViewCount).Min(x => x.Low);

            for (int i = Start; i < End; i++)
            {
                var quote = Quotes[i];
                var viewIndex = i - Start;

                // Draw Price Candlestick
                drawingContext.DrawLine(
                    quote.Open < quote.Close ? DrawingTools.LongPen : DrawingTools.ShortPen,
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.High - priceMin) / (priceMax - priceMin))),
                    new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (quote.Low - priceMin) / (priceMax - priceMin))));
                drawingContext.DrawRectangle(
                    quote.Open < quote.Close ? DrawingTools.LongBrush : DrawingTools.ShortBrush,
                    quote.Open < quote.Close ? DrawingTools.LongPen : DrawingTools.ShortPen,
                    new Rect(
                    new Point(itemWidth * viewIndex + ItemMargin, ActualHeight * (double)(1.0m - (quote.Open - priceMin) / (priceMax - priceMin))),
                    new Point(itemWidth * (viewIndex + 1) - ItemMargin, ActualHeight * (double)(1.0m - (quote.Close - priceMin) / (priceMax - priceMin)))
                    ));

                // Draw Indicators
                if (i < Indicators.Count && i >= 1)
                {
                    var preIndicator = Indicators[i - 1];
                    var indicator = Indicators[i];

                    if (preIndicator != null && indicator != null && preIndicator.Value != 0 && indicator.Value != 0)
                    {
                        drawingContext.DrawLine(
                            new Pen(new SolidColorBrush(Colors.Yellow), 1),
                            new Point(itemWidth * (viewIndex - 0.5), ActualHeight * (double)(1.0m - (preIndicator.Value - priceMin) / (priceMax - priceMin))),
                            new Point(itemWidth * (viewIndex + 0.5), ActualHeight * (double)(1.0m - (indicator.Value - priceMin) / (priceMax - priceMin)))
                            );
                    }
                }
            }
        }
    }
}
