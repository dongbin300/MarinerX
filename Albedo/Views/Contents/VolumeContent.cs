using Albedo.Utils;

using Skender.Stock.Indicators;

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Albedo.Views.Contents
{
    public class VolumeContent : ContentControl
    {
        public List<Quote> Quotes { get; set; } = new();
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

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ViewItemCount <= 1)
            {
                return;
            }

            base.OnRender(drawingContext);

            var volumeMax = Quotes.Skip(StartItemIndex).Take(ViewItemCount).Max(x => x.Volume);

            // Draw Grid
            var gridLevel = 2; // 2등분
            for (int i = 0; i <= gridLevel; i++)
            {
                if (i > 0)
                {
                    drawingContext.DrawLine(
                                     DrawingTools.GridPen,
                                     new Point(0, ActualHeight * ((double)i / gridLevel)),
                                     new Point(ActualWidth, ActualHeight * ((double)i / gridLevel))
                                  );
                }
            }

            for (int i = StartItemIndex; i < EndItemIndex; i++)
            {
                var quote = Quotes[i];
                var viewIndex = i - StartItemIndex;

                // Draw Volume Histogram
                drawingContext.DrawRectangle(
                    quote.Open < quote.Close ? DrawingTools.LongBrush : DrawingTools.ShortBrush,
                    quote.Open < quote.Close ? DrawingTools.LongPen : DrawingTools.ShortPen,
                    new Rect(
                        new Point(ActualItemFullWidth * viewIndex + ActualItemMargin / 2, ActualHeight * (double)(1.0m - quote.Volume / volumeMax)),
                        new Point(ActualItemFullWidth * (viewIndex + 1) - ActualItemMargin / 2, ActualHeight)
                    ));
            }
        }
    }
}
