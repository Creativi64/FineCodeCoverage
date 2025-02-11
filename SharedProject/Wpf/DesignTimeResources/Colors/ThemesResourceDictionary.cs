using System.Linq;
using System.Windows.Media;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Wpf
{
    public class ThemesResourceDictionary : ResourceDictionary
    {
        public class ThemeResourceDictionary : ResourceDictionary
        {
            public ThemeResourceDictionary(string themeName)
            {
                ThemeService.GetResources(themeName).ToList().ForEach(kvp => Add(kvp.Key, GetResource(kvp.Value, kvp.Key.KeyType)));
            }
        }

        private static object GetResource(Color color, ThemeResourceKeyType themeResourceKeyType)
        {
            if (themeResourceKeyType.IsBrushType())
            {
                return new SolidColorBrush(color);
            }
            return color;
        }

        internal void SetTheme(string themeName)
        {
            this.MergedDictionaries.Clear();
            this.MergedDictionaries.Add(new ThemeResourceDictionary(themeName));
        }
    }
}
