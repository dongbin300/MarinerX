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
    public class CandleAxisContent : ContentControl
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

            var priceMax = Quotes.Skip(StartItemIndex).Take(ViewItemCount).Max(x => x.High);
            var priceMin = Quotes.Skip(StartItemIndex).Take(ViewItemCount).Min(x => x.Low);

            // Draw Grid
            var gridLevel = 4; // 4등분
            //var decimalDigitsCount = NumberUtil.GetDecimalDigitsCount(Quotes[StartItemIndex].Close); // 코인 가격의 소수 자릿수
            for (int i = 0; i <= gridLevel; i++)
            {
                //var gridPrice = Math.Round(priceMin + (priceMax - priceMin) * ((decimal)(gridLevel - i) / gridLevel), decimalDigitsCount);
                var gridPrice = NumberUtil.ToRoundedValue(priceMin + (priceMax - priceMin) * ((decimal)(gridLevel - i) / gridLevel));

                drawingContext.DrawText(
                    new FormattedText(gridPrice.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Meiryo UI"), 9, DrawingTools.GridBrush, VisualTreeHelper.GetDpi(this).PixelsPerDip),
                new Point(5, ActualHeight * ((double)i / gridLevel) - 7)
                );
            }

            // Draw Current Price Ticker
            var currentPriceTickerText = new FormattedText(Quotes[EndItemIndex - 1].Close.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Meiryo UI"), 10,
                    Quotes[EndItemIndex - 1].Open < Quotes[EndItemIndex - 1].Close ? DrawingTools.LongBrush : DrawingTools.ShortBrush,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
            currentPriceTickerText.SetFontWeight(FontWeights.Bold);
            drawingContext.DrawText(currentPriceTickerText,
                new Point(5, ActualHeight * (double)(1.0m - (Quotes[EndItemIndex - 1].Close - priceMin) / (priceMax - priceMin)) - 8)
                );
        }
    }
}
