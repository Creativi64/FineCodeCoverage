using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
namespace FineCodeCoverage.Output
{
    public class SizeOrMultiplierConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var sizeOrMultiplier = values[0] as double?;
            var value = (double)values[1];
            if (sizeOrMultiplier.HasValue)
            {
                if (sizeOrMultiplier.Value > 1)
                    return sizeOrMultiplier.Value;

                return value * sizeOrMultiplier.Value;
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
