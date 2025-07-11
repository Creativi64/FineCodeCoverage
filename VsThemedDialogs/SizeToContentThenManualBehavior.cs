using System;
using System.Windows;

namespace VsThemedDialogs
{
    public static class SizeToContentThenManualBehavior
    {
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.RegisterAttached(
                "Mode",
                typeof(SizeToContent),
                typeof(SizeToContentThenManualBehavior),
                new PropertyMetadata(SizeToContent.Manual, OnModeChanged));

        public static void SetMode(DependencyObject element, SizeToContent value) => element.SetValue(ModeProperty, value);

        public static SizeToContent GetMode(DependencyObject element) => (SizeToContent)element.GetValue(ModeProperty);

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window))
                return;

            var mode = (SizeToContent)e.NewValue;
            window.SizeToContent = mode;

            void handler(object sender, EventArgs args)
            {
                if (window.ActualWidth > 0 && window.ActualHeight > 0)
                {
                    window.Width = window.ActualWidth;
                    window.Height = window.ActualHeight;
                    window.SizeToContent = SizeToContent.Manual;

                    window.LayoutUpdated -= handler;
                }
            }

            window.LayoutUpdated += handler;
        }
    }
}
