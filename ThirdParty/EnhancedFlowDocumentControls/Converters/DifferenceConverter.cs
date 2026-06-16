using System;
using System.Globalization;
using System.Windows.Data;

namespace EnhancedFlowDocumentControls.Converters
{
    public sealed class DifferenceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double from = (double)value;
            double difference;
            if (parameter is string strParam)
            {
                if (!double.TryParse(strParam, out difference))
                {
                    throw new ArgumentException("Parameter must be a valid double value.", nameof(parameter));
                }
            }
            else if (parameter is double doubleParam)
            {
                difference = doubleParam;
            }
            else if (parameter is int intParam)
            {
                difference = intParam;
            }
            else if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null.");
            }
            else
            {
                throw new ArgumentException("Parameter must be a string, double, or int.", nameof(parameter));
            }

            return from - difference;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
