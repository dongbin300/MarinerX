using SkiaSharp;

using System.Windows.Controls;

namespace Albedo.Test
{
    /// <summary>
    /// CandleContent.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CandleContent : UserControl
    {
        public CandleContent()
        {
            InitializeComponent();
        }

        private void Canvas_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            canvas.DrawLine(1, 1, 100, 100, new SKPaint() { Color = SKColors.Yellow });
        }
    }
}
