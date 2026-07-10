using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfGridExtensions
{
    internal class MarginBottomConverter : IMultiValueConverter
    {
        private static MarginBottomConverter s_instance;

        public static MarginBottomConverter Instance => s_instance ?? (s_instance = new MarginBottomConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return new GridLength(0);
            }

            bool? visible = values[0] as bool?;
            double? perRowHeight = values[1] as double?;
            double? gridDefaultHeight = values[2] as double?;

            if (visible != true)
            {
                return new GridLength(0);
            }

            var height = perRowHeight ?? gridDefaultHeight ?? 0;
            return new GridLength(height);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
