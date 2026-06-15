using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Wpf
{
    /// <summary>
    /// A container ThemeResourceDictionary with a single theme specific ThemeResourceDictionary.
    /// ThemeResourceKeyType => SolidColorBrush or Color - Colors provided by ThemeService.
    /// </summary>
    internal sealed class ThemesResourceDictionary : ResourceDictionary
    {
        public sealed class ThemeResourceDictionary : ResourceDictionary
        {
            public ThemeResourceDictionary(string themeName)
                => ThemeService.GetResources(themeName).ToList()
                .ForEach(kvp => Add(kvp.Key, GetResource(kvp.Value, kvp.Key.KeyType)));
        }

        private static object GetResource(Color color, ThemeResourceKeyType themeResourceKeyType)
            => themeResourceKeyType.IsBrushType() ? new SolidColorBrush(color) : color;

        internal void SetTheme(string themeName)
        {
            MergedDictionaries.Clear();
            MergedDictionaries.Add(new ThemeResourceDictionary(themeName));
        }
    }
}
