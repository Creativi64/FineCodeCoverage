using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace FineCodeCoverage.Wpf
{
    public class DesignTimeImageThemingUtilities
    {
        public static readonly DependencyProperty ImageBackgroundColorProperty =
            DependencyProperty.RegisterAttached(
                "ImageBackgroundColor",
                typeof(Color),
                typeof(DesignTimeImageThemingUtilities),
                new PropertyMetadata(Colors.Transparent, OnImageBackgroundColorChanged));

        private static void OnImageBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(d)) return;
            var newColor = (Color)e.NewValue;
            ImageThemingUtilities.SetImageBackgroundColor(d, newColor);
        }

        public static void SetImageBackgroundColor(DependencyObject element, Color value)
        {
            element.SetValue(ImageBackgroundColorProperty, value);
        }

        public static Color GetImageBackgroundColor(DependencyObject element)
        {
            return (Color)element.GetValue(ImageBackgroundColorProperty);
        }
    }
}
