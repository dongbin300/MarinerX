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
        public int Start { get; set; } = 0;
        public int End { get; set; } = 0;
        public int ViewCount => End - Start;

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
                var gridPrice = Math.Round(volumeMax * ((decimal)(gridLevel - i) / gridLevel), 0);

                drawingContext.DrawText(
                    new FormattedText(gridPrice.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Meiryo UI"), 9, DrawingTools.GridBrush, VisualTreeHelper.GetDpi(this).PixelsPerDip),
                new Point(5, ActualHeight * ((double)i / gridLevel) - 5)
                );
            }

            // Draw Current Volume Ticker
            var currentVolumeTickerText = new FormattedText(Quotes[End - 1].Close.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Meiryo UI"), 10,
                    Quotes[End - 1].Open < Quotes[End - 1].Close ? DrawingTools.LongBrush : DrawingTools.ShortBrush,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
            currentVolumeTickerText.SetFontWeight(FontWeights.Bold);
            drawingContext.DrawText(currentVolumeTickerText,
                new Point(5, ActualHeight * (double)(1.0m - (Quotes[End - 1].Close) / volumeMax) - 7)
                );
        }
    }
}
