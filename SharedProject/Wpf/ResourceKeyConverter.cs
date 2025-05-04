using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FineCodeCoverage.Wpf
{
    public class ResourceKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return Application.Current.Resources[value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
