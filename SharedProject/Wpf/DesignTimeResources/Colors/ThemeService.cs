using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Wpf
{
    public static class ThemeService
    {
        private static readonly Dictionary<string, Dictionary<ThemeResourceKey, Color>> _themeColors = new Dictionary<string, Dictionary<ThemeResourceKey, Color>>();

        static ThemeService() => LoadThemes(@"C:\Users\tonyh\Downloads\themes.xml");

        private static void LoadThemes(string path)
        {
            var root = XElement.Load(path);
            foreach (XElement themeElement in root.Elements())
            {
                string themeName = themeElement.Attribute("Name").Value;
                var themeColors = new Dictionary<ThemeResourceKey, Color>();
                foreach (XElement themeResourceKeyColorElement in themeElement.Elements())
                {
                    var color = (Color)ColorConverter.ConvertFromString(themeResourceKeyColorElement.Attribute("Color").Value);
                    string category = themeResourceKeyColorElement.Attribute("Category").Value;
                    string keyType = themeResourceKeyColorElement.Attribute("KeyType").Value;
                    string name = themeResourceKeyColorElement.Attribute("Name").Value;
                    var themeResourceKey = new ThemeResourceKey(new Guid(category), name, (ThemeResourceKeyType)Enum.Parse(typeof(ThemeResourceKeyType), keyType));
                    themeColors.Add(themeResourceKey, color);
                }

                _themeColors.Add(themeName, themeColors);
            }
        }

        public static bool IsTheme(string themeName) => _themeColors.ContainsKey(themeName);

        public static Dictionary<ThemeResourceKey, Color> GetResources(string themeName) => _themeColors[themeName];
    }
}