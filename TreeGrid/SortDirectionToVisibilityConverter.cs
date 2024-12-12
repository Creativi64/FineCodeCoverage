using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TreeGrid
{
    public class SortDirectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ListSortDirection sortDirection)
            {
                if (parameter is string directionString)
                {
                    if (Enum.TryParse(directionString, out ListSortDirection direction))
                    {
                        return sortDirection == direction ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
