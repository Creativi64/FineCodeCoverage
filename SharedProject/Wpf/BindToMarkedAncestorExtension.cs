using System;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public class BindToMarkedAncestorExtension : MarkupExtension
    {
        public string Marker { get; set; }
        public string Path { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt))
                return null;

            if (!(pvt.TargetObject is FrameworkElement targetElement))
                return this;

            void ApplyBinding()
            {
                var ancestor = FindMarkedAncestor(targetElement);
                if (ancestor is FrameworkElement fe)
                {
                    var binding = new Binding(Path)
                    {
                        Source = fe.DataContext,
                        Mode = BindingMode.OneWay
                    };

                    BindingOperations.SetBinding(
                        (DependencyObject)pvt.TargetObject,
                        (DependencyProperty)pvt.TargetProperty,
                        binding);
                }
            }

            if (!targetElement.IsLoaded)
                targetElement.Loaded += (_, __) => ApplyBinding();
            else
                ApplyBinding();

            return DependencyProperty.UnsetValue;
        }

        private DependencyObject FindMarkedAncestor(DependencyObject start)
        {
            var current = start;
            while (current != null)
            {
                if (AncestorLocator.GetMarker(current) == Marker)
                    return current;

                current = LogicalTreeHelper.GetParent(current)
                       ?? VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }

}
