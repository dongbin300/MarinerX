using Albedo.Enums;

using System.Windows.Media;

namespace Albedo.Models
{
    public class MaModel
    {
        public int Period { get; set; }
        public MaType Type { get; set; }
        public SolidColorBrush LineColor { get; set; } = default!;
        public int LineWeight { get; set; }

        public MaModel(int period, MaType type, SolidColorBrush lineColor, int lineWeight)
        {
            Period = period;
            Type = type;
            LineColor = lineColor;
            LineWeight = lineWeight;
        }
    }
}
