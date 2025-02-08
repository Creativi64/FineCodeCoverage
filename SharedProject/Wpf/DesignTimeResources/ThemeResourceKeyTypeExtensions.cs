using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Wpf
{
    public static class ThemeResourceKeyTypeExtensions
    {
        public static bool IsBrushType(this ThemeResourceKeyType themeResourceKeyType)
        {
            return themeResourceKeyType == ThemeResourceKeyType.BackgroundBrush || themeResourceKeyType == ThemeResourceKeyType.ForegroundBrush;
        }
    }
}
