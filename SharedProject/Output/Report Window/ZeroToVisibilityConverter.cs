using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FineCodeCoverage.Output
{
    internal class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = false;

            if (parameter is string s && bool.TryParse(s, out bool parsed))
            {
                invert = parsed;
            }

            bool isZero = value is int i && i == 0;

            return (isZero ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
