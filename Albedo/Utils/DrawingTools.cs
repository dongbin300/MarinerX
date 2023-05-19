using SkiaSharp;

namespace Albedo.Utils
{
    public class DrawingTools
    {
        public static SKFont GridTextFont = new SKFont(SKTypeface.FromFamilyName("Meiryo UI"), 9);
        public static SKFont CandleInfoFont = new SKFont(SKTypeface.FromFamilyName("Meiryo UI"), 11);
        public static SKFont CurrentTickerFont = new SKFont(SKTypeface.FromFamilyName("Meiryo UI", SKFontStyle.Bold), 10);

        public static SKPaint GridPaint = new() { Color = new SKColor(45, 45, 45) };
        public static SKPaint GridTextPaint = new() { Color = new SKColor(65, 65, 65) };
        public static SKPaint CandleInfoPaint = new() { Color = SKColors.White };
        public static SKPaint LongPaint = new() { Color = new SKColor(59, 207, 134) };
        public static SKPaint ShortPaint = new() { Color = new SKColor(237, 49, 97) };
        public static SKPaint CandlePointerPaint = new() { Color = new SKColor(255, 255, 255, 32) };
    }
}
