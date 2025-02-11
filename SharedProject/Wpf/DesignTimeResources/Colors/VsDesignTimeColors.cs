using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public static class VsDesignTimeColors
    {
        public static readonly DependencyProperty ThemeNameProperty =
            DependencyProperty.RegisterAttached(
                "ThemeName",
                typeof(string),
                typeof(VsDesignTimeColors),
                new PropertyMetadata(default(string), OnThemeNameChanged));
        public static void SetThemeName(DependencyObject element, string value)
        {
            element.SetValue(ThemeNameProperty, value);
        }

        public static string GetThemeName(DependencyObject element)
        {
            return (string)element.GetValue(ThemeNameProperty);
        }

        private static void OnThemeNameChanged(DependencyObject root, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(root))
            {
                return;
            }
            var newThemeName = e.NewValue as string;
            if (!ThemeService.IsTheme(newThemeName))
            {
                return;
            }

            var themeResourceDictionary = GetOrAddThemeResourceDictionary(root as FrameworkElement);
            themeResourceDictionary.SetTheme(newThemeName);
        }

        private static ThemesResourceDictionary GetOrAddThemeResourceDictionary(FrameworkElement fe)
        {
            var resources = fe.Resources;
            var mergedDictionaries = resources.MergedDictionaries;
            var themeResourceDictionary = mergedDictionaries.FirstOrDefault(rd => rd is ThemesResourceDictionary) as ThemesResourceDictionary;
            if (themeResourceDictionary == null)
            {
                themeResourceDictionary = new ThemesResourceDictionary();
                resources.MergedDictionaries.Add(themeResourceDictionary);
            }
            return themeResourceDictionary;
        }

    }

}
