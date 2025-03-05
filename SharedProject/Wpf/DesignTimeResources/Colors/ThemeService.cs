using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Linq;

namespace FineCodeCoverage.Wpf
{
    public static class ThemeService
    {
        private static readonly Dictionary<string, Dictionary<ThemeResourceKey, Color>> _themeColors = new Dictionary<string, Dictionary<ThemeResourceKey, Color>>();

        static ThemeService()
        {
            LoadThemes(@"C:\Users\tonyh\Downloads\themes.xml");
        }

        private static void LoadThemes(string path)
        {
            var root = XElement.Load(path);
            foreach (var themeElement in root.Elements())
            {
                var themeName = themeElement.Attribute("Name").Value;
                var themeColors = new Dictionary<ThemeResourceKey, Color>();
                foreach (var themeResourceKeyColorElement in themeElement.Elements())
                {
                    var color = (Color)ColorConverter.ConvertFromString(themeResourceKeyColorElement.Attribute("Color").Value);
                    var category = themeResourceKeyColorElement.Attribute("Category").Value;
                    var keyType = themeResourceKeyColorElement.Attribute("KeyType").Value;
                    var name = themeResourceKeyColorElement.Attribute("Name").Value;
                    var themeResourceKey = new ThemeResourceKey(new Guid(category), name, (ThemeResourceKeyType)Enum.Parse(typeof(ThemeResourceKeyType), keyType));
                    themeColors.Add(themeResourceKey, color);
                }
                _themeColors.Add(themeName, themeColors);
            }

        }
        public static bool IsTheme(string themeName)
        {
            return _themeColors.ContainsKey(themeName);
        }

        public static Dictionary<ThemeResourceKey, Color> GetResources(string themeName)
        {
            return _themeColors[themeName];
        }
    }
}
