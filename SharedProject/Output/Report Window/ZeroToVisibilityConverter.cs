using System.Windows.Data;
using System.Windows;
using System;
using System.Globalization;

namespace FineCodeCoverage.Output
{
    internal class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = false;

            if (parameter is string s && bool.TryParse(s, out var parsed))
            {
                invert = parsed;
            }

            bool isZero = value is double d && d == 0;

            return (isZero ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
