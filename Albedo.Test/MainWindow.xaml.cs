using SkiaSharp;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Windows;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using System.IO;
using Binance.Net.Clients;
using System;

namespace Albedo.Test
{
    public class SKColoredText
    {
        public static SKColoredText NewLine { get; set; } = default!;

        public string Text { get; set; }
        public SKColor TextColor { get; set; }
        public SKColor BackgroundColor { get; set; }

        public SKColoredText(string text, SKColor fontColor)
        {
            Text = text;
            TextColor = fontColor;
            BackgroundColor = SKColors.Transparent;
        }

        public SKColoredText(string text, SKColor fontColor, SKColor backgroundColor)
        {
            Text = text;
            TextColor = fontColor;
            BackgroundColor = backgroundColor;
        }
    }

    public static class SkiaSharpExtension
    {
        public static void DrawColoredText(this SKCanvas canvas, IEnumerable<SKColoredText> text, float x, float y, SKFont font)
        {
            float startX = x;
            float currentX = x;
            float currentY = y;
            foreach (SKColoredText textItem in text)
            {
                if (textItem == SKColoredText.NewLine)
                {
                    currentY += font.Size;
                    currentX = startX;
                    continue;
                }

                var textBounds = new SKRect();
                var textPaint = new SKPaint() { Color = textItem.TextColor, TextSize = font.Size };
                textPaint.MeasureText(textItem.Text, ref textBounds);
                var backgroundRect = new SKRect(currentX - 5, currentY - textBounds.Height - 5, currentX + textBounds.Width + 10, currentY + 5);
                if(textItem.BackgroundColor != SKColors.Transparent)
                {
                    canvas.DrawRect(backgroundRect, new SKPaint() { Color = textItem.BackgroundColor });
                }

                canvas.DrawText(
                    textItem.Text,
                    currentX,
                    currentY,
                    font,
                    textPaint);

                currentX += textBounds.Width + 5;
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SKFont font = new SKFont(SKTypeface.FromFamilyName("Meiryo UI"), 12);
        SKPaint paint = new SKPaint() { Color = SKColors.White };
        SKPaint pathPaint = new SKPaint() { Style = SKPaintStyle.Fill, Color = SKColors.White };

        public MainWindow()
        {
            InitializeComponent();

            var data = File.ReadAllLines(@"C:\Users\Gaten\AppData\Roaming\Gaten\binance_api.txt");
            BinanceClient client = new BinanceClient(new Binance.Net.Objects.BinanceClientOptions
            {
                ApiCredentials = new Binance.Net.Objects.BinanceApiCredentials(data[0], data[1])
            });
            var result = client.UsdFuturesApi.Account.GetIncomeHistoryAsync(null, "REALIZED_PNL", DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(0), 1000);
            result.Wait();
            var __data = result.Result.Data;
        }


        private void Screen_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            //var text = new List<SKColoredText>
            //{
            //    new SKColoredText("TEST", SKColors.White, SKColors.Gray.WithAlpha(120)),
            //    new SKColoredText("CONTENT", SKColors.Red),
            //    SKColoredText.NewLine,
            //    new SKColoredText("TEST", SKColors.White),
            //    new SKColoredText("CONTENT", SKColors.Green),
            //};
            //canvas.DrawColoredText(text, 20, 20, font);

            var path = new SKPath();
            path.MoveTo(10, 200);
            path.LineTo(30, 150);
            path.LineTo(50, 250);
            path.LineTo(70, 420);
            path.LineTo(90, 400);
            path.LineTo(110, 360);

            path.LineTo(10, 400);
            path.LineTo(30, 420);
            path.LineTo(50, 250);
            path.LineTo(70, 200);
            path.LineTo(90, 150);
            path.LineTo(110, 130);

            canvas.DrawPath(path, pathPaint);
        }
    }
}
