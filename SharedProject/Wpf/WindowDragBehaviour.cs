using System.Windows;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public static class WindowDragBehaviour
    {
        public static readonly DependencyProperty IsDragHandleProperty =
            DependencyProperty.RegisterAttached(
                "IsDragHandle",
                typeof(bool),
                typeof(WindowDragBehaviour),
                new PropertyMetadata(false, OnIsDragHandleChanged));

        public static bool GetIsDragHandle(DependencyObject obj)
            => (bool)obj.GetValue(IsDragHandleProperty);

        public static void SetIsDragHandle(DependencyObject obj, bool value)
            => obj.SetValue(IsDragHandleProperty, value);

        private static void OnIsDragHandleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement element))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            }
            else
            {
                element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
            }
        }

        private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DependencyObject d))
            {
                return;
            }

            var window = Window.GetWindow(d);
            if (window == null)
            {
                return;
            }

            try
            {
                window.DragMove();
            }
            catch
            {
                /* ignored: DragMove throws if mouse is released */
            }
        }
    }

}
