using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

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
                DependencyObject ancestor = this.FindMarkedAncestor(targetElement);
                if (!(ancestor is FrameworkElement fe))
                {
                    return;
                }

                var binding = new Binding(this.Path)
                {
                    Source = fe.DataContext,
                    Mode = BindingMode.OneWay
                };

                _ = BindingOperations.SetBinding(
                    (DependencyObject)pvt.TargetObject,
                    (DependencyProperty)pvt.TargetProperty,
                    binding);
            }

            if (!targetElement.IsLoaded)
                targetElement.Loaded += (_, __) => ApplyBinding();
            else
                ApplyBinding();

            return DependencyProperty.UnsetValue;
        }

        private DependencyObject FindMarkedAncestor(DependencyObject start)
        {
            DependencyObject current = start;
            while (current != null)
            {
                if (AncestorLocator.GetMarker(current) == this.Marker)
                    return current;

                current = LogicalTreeHelper.GetParent(current)
                       ?? VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}