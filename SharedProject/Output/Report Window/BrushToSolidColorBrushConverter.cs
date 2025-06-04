using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FineCodeCoverage.Output
{
    internal class BrushToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value as SolidColorBrush;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
