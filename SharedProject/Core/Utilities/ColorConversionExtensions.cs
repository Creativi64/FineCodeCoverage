using System.Windows.Media;

namespace FineCodeCoverage.Core.Utilities
{
    public static class ColorConversionExtensions
    {
        public static SolidColorBrush ToMediaBrush(this System.Drawing.Color color) => new SolidColorBrush(color.ToMediaColor());

        public static Color ToMediaColor(this System.Drawing.Color color) => System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);

    }
}