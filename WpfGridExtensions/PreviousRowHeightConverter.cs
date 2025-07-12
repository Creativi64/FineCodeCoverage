using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfGridExtensions
{
    internal class PreviousRowHeightConverter : IMultiValueConverter
    {
        private static PreviousRowHeightConverter s_instance;

        public static PreviousRowHeightConverter Instance
            => s_instance ?? (s_instance = new PreviousRowHeightConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
            {
                return new GridLength(0);
            }

            bool? visible = values[0] as bool?;
            GridLength? height = values[1] as GridLength?;

            if (visible != true || height == null)
            {
                return new GridLength(0);
            }

            return height.Value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("VisibleToHeightConverter does not support ConvertBack.");
    }
}
