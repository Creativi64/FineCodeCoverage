using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Wpf
{
    /// <summary>
    /// Loads embedded xml resource file. Creates dictionary per theme for lookup of colours by ThemeResourceKey.
    /// </summary>
    public static class ThemeService
    {
        private static readonly Dictionary<string, Dictionary<ThemeResourceKey, Color>> s_themeColors = new Dictionary<string, Dictionary<ThemeResourceKey, Color>>();

        static ThemeService() => LoadThemes();

        private static void LoadThemes()
        {
            XElement root = GetThemesElement();
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

                s_themeColors.Add(themeName, themeColors);
            }
        }

        private static Stream GetManifestResourceStream()
            => typeof(ThemeService).Assembly.GetManifestResourceStream("FineCodeCoverage.DesignTime.themes.xml");

        private static XElement GetThemesElement()
        {
            using (Stream stream = GetManifestResourceStream())
            {
                return stream == null ? throw new InvalidOperationException("Could not find the themes resource file.") : XElement.Load(stream);
            }
        }

        public static bool IsTheme(string themeName) => s_themeColors.ContainsKey(themeName);

        public static Dictionary<ThemeResourceKey, Color> GetResources(string themeName) => s_themeColors[themeName];
    }
}
