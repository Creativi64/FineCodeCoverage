using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    internal static class VsDesignTimeColors
    {
        public static readonly DependencyProperty ThemeNameProperty =
            DependencyProperty.RegisterAttached(
                "ThemeName",
                typeof(string),
                typeof(VsDesignTimeColors),
                new PropertyMetadata(default(string), OnThemeNameChanged));

        public static void SetThemeName(DependencyObject element, string value) => element.SetValue(ThemeNameProperty, value);

        public static string GetThemeName(DependencyObject element) => (string)element.GetValue(ThemeNameProperty);

        private static void OnThemeNameChanged(DependencyObject root, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(root))
            {
                return;
            }

            string newThemeName = e.NewValue as string;
            if (!ThemeService.IsTheme(newThemeName))
            {
                return;
            }

            ThemesResourceDictionary themeResourceDictionary = GetOrAddThemeResourceDictionary(root as FrameworkElement);
            themeResourceDictionary.SetTheme(newThemeName);
        }

        private static ThemesResourceDictionary GetOrAddThemeResourceDictionary(FrameworkElement fe)
        {
            ResourceDictionary resources = fe.Resources;
            Collection<ResourceDictionary> mergedDictionaries = resources.MergedDictionaries;
            if (!(mergedDictionaries.FirstOrDefault(rd => rd is ThemesResourceDictionary) is ThemesResourceDictionary themeResourceDictionary))
            {
                themeResourceDictionary = new ThemesResourceDictionary();
                resources.MergedDictionaries.Add(themeResourceDictionary);
            }

            return themeResourceDictionary;
        }
    }
}
