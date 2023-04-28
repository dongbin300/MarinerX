using Albedo.Utils;
using Skender.Stock.Indicators;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Albedo.Views.Contents
{
    public class VolumeContent : ContentControl
    {
        public List<Quote> Quotes { get; set; } = new();
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
            var volumeMax = Quotes.Skip(Start).Take(ViewCount).Max(x => x.Volume);

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

            for (int i = Start; i < End; i++)
            {
                var quote = Quotes[i];
                var viewIndex = i - Start;

                // Draw Volume Histogram
                drawingContext.DrawRectangle(
                    quote.Open < quote.Close ? DrawingTools.LongBrush : DrawingTools.ShortBrush,
                    quote.Open < quote.Close ? DrawingTools.LongPen : DrawingTools.ShortPen,
                    new Rect(
                        new Point(itemWidth * viewIndex + ItemMargin, ActualHeight * (double)(1.0m - quote.Volume / volumeMax)),
                        new Point(itemWidth * (viewIndex + 1) - ItemMargin, ActualHeight)
                    ));
            }
        }
    }
}
