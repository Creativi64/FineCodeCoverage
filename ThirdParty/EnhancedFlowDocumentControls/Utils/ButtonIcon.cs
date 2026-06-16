using System.Windows;
using System.Windows.Media;

namespace EnhancedFlowDocumentControls.Utils
{
    public static class ButtonIcon
    {
        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.RegisterAttached(
                "Geometry",
                typeof(Geometry),
                typeof(ButtonIcon),
                new FrameworkPropertyMetadata(null));

        public static void SetGeometry(UIElement element, Geometry value) => element.SetValue(GeometryProperty, value);

        public static Geometry GetGeometry(UIElement element) => (Geometry)element.GetValue(GeometryProperty);
    }

}
