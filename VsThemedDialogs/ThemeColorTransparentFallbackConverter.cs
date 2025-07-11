using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace VsThemedDialogs
{
    public class ThemeColorTransparentFallbackConverter : IValueConverter
    {
        public ThemeResourceKey ThemeResourceKey { get; set; }

        public ThemeResourceKey FallbackThemeResourceKey { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ThemeResourceKeyPair resourceKeyPair = GetResourceKeys(parameter);
            if (resourceKeyPair == null)
            {
                return null;
            }

            System.Drawing.Color color = VSColorTheme.GetThemedColor(resourceKeyPair.First);
            if (color.A == 0)
            {
                color = VSColorTheme.GetThemedColor(resourceKeyPair.Second);
            }

            return color.ToMediaColor();
        }

        private ThemeResourceKeyPair GetResourceKeys(object parameter)
        {
            if (!(parameter is ThemeResourceKeyPair resourceKeyPair))
            {
                if (ThemeResourceKey == null || FallbackThemeResourceKey == null)
                {
                    return null;
                }

                resourceKeyPair = new ThemeResourceKeyPair { First = ThemeResourceKey, Second = FallbackThemeResourceKey };
            }
            return resourceKeyPair;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
