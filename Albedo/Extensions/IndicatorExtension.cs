using Albedo.Enums;

namespace Albedo.Extensions
{
    public static class IndicatorExtension
    {
        public static float ToStrokeWidth(this LineWeight lineWeight)
        {
            return (int)lineWeight;
        }
    }
}
