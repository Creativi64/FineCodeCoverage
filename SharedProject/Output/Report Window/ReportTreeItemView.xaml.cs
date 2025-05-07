using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FineCodeCoverage.Output
{
    public class BoolToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1.0d : 0.0d;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (float)value == 1.0f;
        }
    }
    public partial class ReportTreeItemView : UserControl
    {
        public ReportTreeItemView()
        {
            InitializeComponent();
        }

        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register(nameof(ShowIcon), typeof(bool), typeof(ReportTreeItemView), new PropertyMetadata(true));

        public int IconSize
        {
            get { return (int)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(nameof(IconSize), typeof(int), typeof(ReportTreeItemView), new PropertyMetadata(16));

        public bool ThemedMonochromeIcons
        {
            get { return (bool)GetValue(ThemedMonochromeIconsProperty); }
            set { SetValue(ThemedMonochromeIconsProperty, value); }
        }

        public static readonly DependencyProperty ThemedMonochromeIconsProperty =
            DependencyProperty.Register(nameof(ThemedMonochromeIcons), typeof(bool), typeof(ReportTreeItemView), new PropertyMetadata(false));
    }
}
