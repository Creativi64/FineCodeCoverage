using System;
using System.Collections.Generic;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public static class ThemeNameManager
    {
        public static readonly DependencyProperty ThemeNameProperty =
            DependencyProperty.RegisterAttached(
                "ThemeName",
                typeof(string),
                typeof(ThemeNameManager),
                new PropertyMetadata(default(string), OnThemeNameChanged));
        public static void SetThemeName(DependencyObject element, string value)
        {
            element.SetValue(ThemeNameProperty, value);
        }

        public static string GetThemeName(DependencyObject element)
        {
            return (string)element.GetValue(ThemeNameProperty);
        }

        private static void OnThemeNameChanged(DependencyObject root, DependencyPropertyChangedEventArgs e)
        {
            var newThemeName = e.NewValue as string;
            if (!ThemeService.IsTheme(newThemeName))
            {
                return;
            }

            NotifyListeners(root, newThemeName);
        }

        private static void NotifyListeners(DependencyObject root, string newThemeName)
        {
            var elementsToUpdate = LogicalTreeHelperExtensions.FindElementsWithAttachedProperty(root, ThemeChangedListenersProperty);

            foreach (var el in elementsToUpdate)
            {
                try
                {
                    var fe = el as FrameworkElement;
                    if (fe == null)
                    {
                        //Logger.Log($"Not a framework element - {el.GetType().FullName} ");
                        continue;
                    }

                    var listeners = GetThemeChangedListeners(el);
                    if (listeners != null)
                    {
                        listeners.ForEach(listener => listener.ThemeChanged(newThemeName));
                    }
                }
                catch (Exception exc)
                {
                    FileLogger.Log($"Error notifying listeners");
                    FileLogger.Log($"{exc.Message}");
                    FileLogger.Log($"{exc.StackTrace}");
                }
            };

        }

        public static readonly DependencyProperty ThemeChangedListenersProperty =
            DependencyProperty.RegisterAttached(
                "ThemeChangedListeners",
                typeof(List<IThemeChangeListener>),
                typeof(ThemeNameManager),
                new PropertyMetadata(null));

        private static void SetThemeChangedListeners(DependencyObject element, List<IThemeChangeListener> value)
        {
            element.SetValue(ThemeChangedListenersProperty, value);
        }

        private static List<IThemeChangeListener> GetThemeChangedListeners(DependencyObject element)
        {
            return (List<IThemeChangeListener>)element.GetValue(ThemeChangedListenersProperty);
        }

        public static void ListenFromThemeChanges(DependencyObject targetObject, IThemeChangeListener listener)
        {
            var listeners = GetThemeChangedListeners(targetObject);
            if (listeners == null)
            {
                listeners = new List<IThemeChangeListener>();
                SetThemeChangedListeners(targetObject, listeners);
            }
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
    }

}
