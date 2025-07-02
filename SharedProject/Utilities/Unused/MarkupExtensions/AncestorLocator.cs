using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public static class AncestorLocator
    {
        public static readonly DependencyProperty MarkerProperty =
            DependencyProperty.RegisterAttached(
                "Marker", typeof(string), typeof(AncestorLocator), new PropertyMetadata(null));

        public static void SetMarker(DependencyObject element, string value)
            => element.SetValue(MarkerProperty, value);

        public static string GetMarker(DependencyObject element)
            => (string)element.GetValue(MarkerProperty);
    }
}
