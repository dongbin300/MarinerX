using System.Windows.Media;

namespace Albedo.Utils
{
    public class DrawingTools
    {
        public static Pen LongPen = new(new SolidColorBrush(Color.FromRgb(59, 207, 134)), 1.0);
        public static Pen ShortPen = new(new SolidColorBrush(Color.FromRgb(237, 49, 97)), 1.0);
        public static Brush LongBrush = new SolidColorBrush(Color.FromRgb(59, 207, 134));
        public static Brush ShortBrush = new SolidColorBrush(Color.FromRgb(237, 49, 97));
    }
}
