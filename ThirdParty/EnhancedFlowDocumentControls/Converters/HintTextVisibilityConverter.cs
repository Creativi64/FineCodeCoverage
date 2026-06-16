using System;
using System.Globalization;
using System.Windows.Data;

namespace EnhancedFlowDocumentControls.Converters
{
    public sealed class HintTextVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null
                ? System.Windows.Visibility.Visible
                : value is string strValue
                ? strValue.Length == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed
                : (object)System.Windows.Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
