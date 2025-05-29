using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Output
{
    internal static class ImageThemingUtilitiesX
    {
        public static Color ThemeColor(Color color, Color backgroundColor)
        {
            var backgroundHsl = HslColor.FromColor(backgroundColor);
            var baseR = color.R;
            var baseG = color.G;
            var baseB = color.B;
            ImageThemingUtilities.ThemePixel(ref baseR, ref baseG, ref baseB, backgroundHsl);
            return Color.FromArgb(255, baseR, baseG, baseB);
        }
        public static SolidColorBrush ThemeColorToSolidBrush(Color color, Color backgroundColor)
            => new SolidColorBrush(ThemeColor(color, backgroundColor));
    }
}
