using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Core.Utilities
{
    public static class ThemeResourceKeyExtensions
    {
        public static SolidColorBrush ToBrush(this ThemeResourceKey themeResourceKey)
            => VSColorTheme.GetThemedColor(themeResourceKey).ToMediaBrush();
        public static Color ToColor(this ThemeResourceKey themeResourceKey)
            => VSColorTheme.GetThemedColor(themeResourceKey).ToMediaColor();
    }
}