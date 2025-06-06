using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Wpf
{
    /*
        Taken from Community.VisualStudio.Toolkit minus the additional resource dictionary
        https://github.com/VsixCommunity/Community.VisualStudio.Toolkit/blob/2fac3c44fdeff50c93908fe943b34ab194e340e3/src/toolkit/Community.VisualStudio.Toolkit.Shared/Themes/Themes.cs#L131
    */
    public static class Themes
    {
        private static readonly DependencyProperty s_originalBackgroundProperty = DependencyProperty.RegisterAttached("OriginalBackground", typeof(object), typeof(Themes));
        private static readonly DependencyProperty s_originalForegroundProperty = DependencyProperty.RegisterAttached("OriginalForeground", typeof(object), typeof(Themes));

        public static readonly DependencyProperty UseVsThemeProperty = DependencyProperty.RegisterAttached("UseVsTheme", typeof(bool), typeof(Themes), new PropertyMetadata(false, UseVsThemePropertyChanged));

        public static void SetUseVsTheme(UIElement element, bool value) => element.SetValue(UseVsThemeProperty, value);

        public static bool GetUseVsTheme(UIElement element) => (bool)element.GetValue(UseVsThemeProperty);

        private static void UseVsThemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d) || !(d is FrameworkElement element))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                OverrideProperty(element, Control.BackgroundProperty, s_originalBackgroundProperty, ThemedDialogColors.WindowPanelBrushKey);
                OverrideProperty(element, Control.ForegroundProperty, s_originalForegroundProperty, ThemedDialogColors.WindowPanelTextBrushKey);
                ThemedDialogStyleLoader.SetUseDefaultThemedDialogStyles(element, true);
                ImageThemingUtilities.SetThemeScrollBars(element, true);
            }
            else
            {
                ImageThemingUtilities.SetThemeScrollBars(element, null);
                ThemedDialogStyleLoader.SetUseDefaultThemedDialogStyles(element, false);
                RestoreProperty(element, Control.ForegroundProperty, s_originalForegroundProperty);
                RestoreProperty(element, Control.BackgroundProperty, s_originalBackgroundProperty);
            }
        }

        private static void OverrideProperty(FrameworkElement element, DependencyProperty property, DependencyProperty backup, object value)
        {
            if (!(element is Control control))
            {
                return;
            }

            object original = control.ReadLocalValue(property);

            if (!ReferenceEquals(value, DependencyProperty.UnsetValue))
            {
                control.SetValue(backup, original);
            }

            control.SetResourceReference(property, value);
        }

        private static void RestoreProperty(FrameworkElement element, DependencyProperty property, DependencyProperty backup)
        {
            if (!(element is Control control))
            {
                return;
            }

            object value = control.ReadLocalValue(backup);

            if (!ReferenceEquals(value, DependencyProperty.UnsetValue))
            {
                control.SetValue(property, value);
            }
            else
            {
                control.ClearValue(property);
            }

            control.ClearValue(backup);
        }
    }
}
