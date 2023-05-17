using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace Albedo.Test
{
    public class ChartCanvas : SKElement
    {
        CandleContent candle;

        public ChartCanvas()
        {
            candle = new CandleContent();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            DispatcherService.Invoke(candle.Canvas.InvalidateVisual);
        }
    }
}
