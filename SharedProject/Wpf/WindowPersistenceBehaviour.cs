using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public static class WindowPersistenceBehaviour
    {
        public static bool GetEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);

        public static void SetEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(WindowPersistenceBehaviour), new PropertyMetadata(false, OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window && window.DataContext is IPersistWindowState persistWindowState))
            {
                return;
            }

            window.Closed += (sender, args) =>
            {
                WindowPersistence windowPersistence = new WindowPersistence
                {
                    Left = window.Left,
                    Top = window.Top,
                    Width = window.Width,
                    Height = window.Height,
                };
                persistWindowState.SetState(windowPersistence);
            };

            window.SourceInitialized += (_, __) =>
            {
                WindowPersistence windowPersistence = persistWindowState.GetState();
                if (windowPersistence == null)
                {
                    return;
                }

                window.Left = windowPersistence.Left;
                window.Top = windowPersistence.Top;
                window.Width = windowPersistence.Width;
                window.Height = windowPersistence.Height;
                window.WindowStartupLocation = WindowStartupLocation.Manual;
            };
        }
    }
}
