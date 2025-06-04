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
            double? sizeOrMultiplier = values[0] as double?;
            double value = (double)values[1];
            return sizeOrMultiplier.HasValue
                ? sizeOrMultiplier.Value > 1 ? sizeOrMultiplier.Value : (object)(value * sizeOrMultiplier.Value)
                : DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
