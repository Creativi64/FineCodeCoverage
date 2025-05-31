using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Wpf
{
    internal class ThemesResourceDictionary : ResourceDictionary
    {
        public class ThemeResourceDictionary : ResourceDictionary
        {
            public ThemeResourceDictionary(string themeName)
                => ThemeService.GetResources(themeName).ToList()
                .ForEach(kvp => this.Add(kvp.Key, GetResource(kvp.Value, kvp.Key.KeyType)));
        }

        private static object GetResource(Color color, ThemeResourceKeyType themeResourceKeyType)
            => themeResourceKeyType.IsBrushType() ? new SolidColorBrush(color) : (object)color;

        internal void SetTheme(string themeName)
        {
            this.MergedDictionaries.Clear();
            this.MergedDictionaries.Add(new ThemeResourceDictionary(themeName));
        }
    }
}
