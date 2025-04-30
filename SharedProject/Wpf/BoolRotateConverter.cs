using System;
using System.Globalization;
using System.Windows.Data;

namespace FineCodeCoverage.Wpf
{
    public class BoolRotateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == bool.Parse((string)parameter))
            {
                return 180;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
