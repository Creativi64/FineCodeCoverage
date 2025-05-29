using System;
using System.Windows.Media;
using Microsoft.Internal.VisualStudio.PlatformUI;

// taken from https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.platformui.colorutilities?view=visualstudiosdk-2022
// not available in 2019

namespace FineCodeCoverage.Core.Utilities
{
    public static class ColorUtilities
    {
        private static readonly double WhiteLuminance = Colors.White.GetLuminance();
        private static readonly double BlackLuminance = Colors.Black.GetLuminance();
        private static readonly WeakValueDictionary<Color, SolidColorBrush> BrushCache = new WeakValueDictionary<Color, SolidColorBrush>();

        public static double GetContrastRatio(Color foreground, Color background) => ColorUtilities.GetContrastRatio(ColorUtilities.Blend(foreground, background).GetLuminance(), background.GetLuminance());

        public static ContrastComparisonResult CompareContrastWithBlackAndWhite(Color color)
        {
            double luminance = color.GetLuminance();
            double contrastRatio1 = ColorUtilities.GetContrastRatio(ColorUtilities.WhiteLuminance, luminance);
            double contrastRatio2 = ColorUtilities.GetContrastRatio(ColorUtilities.BlackLuminance, luminance);
            if (contrastRatio1 > contrastRatio2)
                return ContrastComparisonResult.ContrastHigherWithWhite;
            return contrastRatio2 <= contrastRatio1 ? ContrastComparisonResult.Equal : ContrastComparisonResult.ContrastHigherWithBlack;
        }

        public static Color Blend(Color foreground, Color background)
        {
            if (foreground.A == byte.MaxValue)
                return foreground;
            if (foreground.A == (byte)0)
                return background;
            double unitScaledForegroundAlpha = (double)foreground.A / (double)byte.MaxValue;
            return Color.FromArgb(byte.MaxValue, BlendChannel((int)foreground.R, (int)background.R), BlendChannel((int)foreground.G, (int)background.G), BlendChannel((int)foreground.B, (int)background.B));

            byte BlendChannel(int fg, int bg)
            {
                double num1 = (double)fg / (double)byte.MaxValue;
                double num2 = (double)bg / (double)byte.MaxValue;
                return Math.Max((byte)0, Math.Min(byte.MaxValue, (byte)(((num1 * unitScaledForegroundAlpha) + (num2 * (1.0 - unitScaledForegroundAlpha))) * (double)byte.MaxValue)));
            }
        }

        private static double GetContrastRatio(double luminance1, double luminance2)
        {
            double num = (luminance1 + 0.05) / (luminance2 + 0.05);
            return num < 1.0 ? 1.0 / num : num;
        }

        public static double GetLuminance(this Color color) => (0.2126 * ColorUtilities.ScaleComponentForLuminance((int)color.R)) + (447.0 / 625.0 * ColorUtilities.ScaleComponentForLuminance((int)color.G)) + (0.0722 * ColorUtilities.ScaleComponentForLuminance((int)color.B));

        private static double ScaleComponentForLuminance(int component)
        {
            double num = (double)component / (double)byte.MaxValue;
            return num > 0.03928 ? Math.Pow((num + 0.055) / 1.055, 2.4) : num / 12.92;
        }

        public static SolidColorBrush GetBrushFromCache(Color color)
        {
            if (ColorUtilities.BrushCache.TryGetValue(color, out SolidColorBrush brushFromCache1))
                return brushFromCache1;
            var brushFromCache2 = new SolidColorBrush(color);
            brushFromCache2.Freeze();
            ColorUtilities.BrushCache[color] = brushFromCache2;
            return brushFromCache2;
        }
    }

}
