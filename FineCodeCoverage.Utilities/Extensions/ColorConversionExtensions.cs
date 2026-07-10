using System.Windows.Media;

namespace FineCodeCoverage.Utilities.Extensions
{
    public static class ColorConversionExtensions
    {
        public static SolidColorBrush ToMediaBrush(this System.Drawing.Color color)
            => new SolidColorBrush(color.ToMediaColor());

        public static Color ToMediaColor(this System.Drawing.Color color)
            => Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
