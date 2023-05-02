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
    public class VolumeAxisContent : ContentControl
    {
        public List<Quote> Quotes { get; set; } = new();
        public double ChartWidth => Quotes.Count * ItemFullWidth;
        public double ViewStartPosition { get; set; } = 0;
        public double ViewEndPosition { get; set; } = 0;

        public int ItemFullWidth => Common.ChartItemFullWidth;

        public int StartItemIndex => (int)(Quotes.Count * (ViewStartPosition / ChartWidth));
        public int EndItemIndex => (int)(Quotes.Count * (ViewEndPosition / ChartWidth));
        public int ViewItemCount => EndItemIndex - StartItemIndex + 1;

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
                var gridPrice = Math.Round(volumeMax * ((decimal)(gridLevel - i) / gridLevel), 0);

                drawingContext.DrawText(
                    new FormattedText(gridPrice.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Meiryo UI"), 9, DrawingTools.GridBrush, VisualTreeHelper.GetDpi(this).PixelsPerDip),
                new Point(5, (ActualHeight - 20) * ((double)i / gridLevel) - 7 + 10)
                );
            }

            // Draw Current Volume Ticker
            var currentVolumeTickerText = new FormattedText(Quotes[EndItemIndex - 1].Volume.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Meiryo UI"), 10,
                    Quotes[EndItemIndex - 1].Open < Quotes[EndItemIndex - 1].Close ? DrawingTools.LongBrush : DrawingTools.ShortBrush,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
            currentVolumeTickerText.SetFontWeight(FontWeights.Bold);
            drawingContext.DrawText(currentVolumeTickerText,
                new Point(5, (ActualHeight - 20) * (double)(1.0m - Quotes[EndItemIndex - 1].Volume / volumeMax) - 8 + 10)
                );
        }
    }
}
