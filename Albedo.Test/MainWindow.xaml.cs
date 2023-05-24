using SkiaSharp;

using System.Collections.Generic;
using System.Windows;

namespace Albedo.Test
{
    public class SKColoredText
    {
        public static SKColoredText NewLine { get; set; } = default!;

        public string Text { get; set; }
        public SKColor TextColor { get; set; }

        public SKColoredText(string text, SKColor fontColor)
        {
            Text = text;
            TextColor = fontColor;
        }
    }

    public static class SkiaSharpExtension
    {
        public static void DrawStyledText(this SKCanvas canvas, IEnumerable<SKColoredText> text, float x, float y, SKFont font, float xMargin)
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

                canvas.DrawText(
                    textItem.Text,
                    currentX,
                    currentY,
                    font,
                    new SKPaint() { Color = textItem.TextColor });

                currentX += textItem.Text.Length * (font.Size + xMargin);
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Screen_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            var text = new List<SKColoredText>
            {
                new SKColoredText("TEST", SKColors.White),
                new SKColoredText("CONTENT", SKColors.Red),
                SKColoredText.NewLine,
                new SKColoredText("TEST", SKColors.White),
                new SKColoredText("CONTENT", SKColors.Green),
            };
            canvas.DrawStyledText(text, 20, 20, font, -4);
            //canvas.DrawText(text, 10, 10, font, paint);
        }
    }
}
