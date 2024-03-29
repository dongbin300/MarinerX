﻿using Binance.Net.Clients;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        GlobalMouseEvents mouseEvents;

        List<string> MonitorSymbols { get; set; } = new()
        {
            "AAVEUSDT",
            "ALGOUSDT",
            "ALICEUSDT",
            "ALPHAUSDT",
            "ANKRUSDT",
            "ANTUSDT",
            "APEUSDT",
            "API3USDT",
            "ARPAUSDT",
            "ARUSDT",
            "ATAUSDT",
            "AUDIOUSDT",
            "AVAXUSDT",
            "AXSUSDT",
            "BAKEUSDT",
            "BALUSDT",
            "BELUSDT",
            "BLZUSDT",
            "C98USDT",
            "CHRUSDT",
            "CHZUSDT",
            "COMPUSDT",
            "COTIUSDT",
            "CRVUSDT",
            "CTSIUSDT",
            "DARUSDT",
            "DASHUSDT",
            "DUSKUSDT",
            "DYDXUSDT",
            "ENJUSDT",
            "ENSUSDT",
            "ETCUSDT",
            "FILUSDT",
            "FTMUSDT",
            "GALAUSDT",
            "GALUSDT",
            "GMTUSDT",
            "GRTUSDT",
            "GTCUSDT",
            "IMXUSDT",
            "JASMYUSDT",
            "KNCUSDT",
            "LINAUSDT",
            "LITUSDT",
            "LPTUSDT",
            "LRCUSDT",
            "MANAUSDT",
            "MASKUSDT",
            "MKRUSDT",
            "NKNUSDT",
            "OCEANUSDT",
            "OGNUSDT",
            "OMGUSDT",
            "ONEUSDT",
            "OPUSDT",
            "PEOPLEUSDT",
            "REEFUSDT",
            "RENUSDT",
            "RLCUSDT",
            "ROSEUSDT",
            "RSRUSDT",
            "RUNEUSDT",
            "RVNUSDT",
            "SANDUSDT",
            "SFPUSDT",
            "SKLUSDT",
            "SNXUSDT",
            "SOLUSDT",
            "STORJUSDT",
            "SUSHIUSDT",
            "THETAUSDT",
            "TOMOUSDT",
            "TRBUSDT",
            "UNFIUSDT",
            "WAVESUSDT",
            "WOOUSDT",
            "ZECUSDT",
            "ZENUSDT",
            "ZILUSDT",
            "ZRXUSDT"
        };

        public record Pnl(string symbol, decimal pnl);

        public MainWindow()
        {
            InitializeComponent();
            //List<Pnl> pnls = new List<Pnl>();

            //var data = File.ReadAllLines(@"C:\Users\Gaten\AppData\Roaming\Gaten\binance_api.txt");
            //BinanceClient client = new BinanceClient(new Binance.Net.Objects.BinanceClientOptions
            //{
            //    ApiCredentials = new Binance.Net.Objects.BinanceApiCredentials(data[0], data[1])
            //});
            //foreach(var symbol in MonitorSymbols)
            //{
            //    Thread.Sleep(250);
            //    var result = client.UsdFuturesApi.Account.GetIncomeHistoryAsync(symbol, "REALIZED_PNL", DateTime.Parse("2023-06-14"), null, 1000);
            //    result.Wait();
            //    var d = result.Result.Data;
            //    var pnl = Math.Round(d.Sum(x => x.Income), 2);
            //    pnls.Add(new Pnl(symbol, pnl));
            //}

            mouseEvents = new();
            mouseEvents.MouseLeftButtonDown += MouseEvents_MouseLeftButtonDown;
            mouseEvents.MouseLeftButtonUp += MouseEvents_MouseLeftButtonUp;
        }

        private void MouseEvents_MouseLeftButtonUp(object? sender, EventArgs e)
        {
            LogList.Items.Add("UP");
        }

        private void MouseEvents_MouseLeftButtonDown(object? sender, EventArgs e)
        {
            LogList.Items.Add("DOWN");
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
