using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme.Converters
{
    public class HyperlinkContainsImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
         => value is InlineCollection source &&
            source.FirstOrDefault() is InlineUIContainer inlineUiContainer &&
            inlineUiContainer.Child is Button button &&
            button.Content is Image;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
