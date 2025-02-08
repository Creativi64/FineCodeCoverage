using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xaml;
using System.Xml.Linq;


namespace FineCodeCoverage.Wpf
{
    public static class LogicalTreeHelperExtensions
    {
        public static IEnumerable<DependencyObject> FindElementsWithAttachedProperty(DependencyObject parent, DependencyProperty attachedProperty)
        {
            if (parent == null) yield break;

            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject dependencyObject)
                {
                    // Check if the attached property has been set
                    if (dependencyObject.GetValue(attachedProperty) != DependencyProperty.UnsetValue)
                    {
                        yield return dependencyObject;
                    }

                    // Recursive call
                    foreach (var descendant in FindElementsWithAttachedProperty(dependencyObject, attachedProperty))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }
    public class ThemeChangedEventArgs : EventArgs
    {
        public DependencyObject SourceElement { get; }
        public string NewThemeName { get; }

        public ThemeChangedEventArgs(DependencyObject sourceElement, string newThemeName)
        {
            SourceElement = sourceElement;
            NewThemeName = newThemeName;
        }
    }

    public static class ThemeManager
    {
        static ThemeManager()
        {
            Logger.Log("ThemeManager static ctor");
        }
        private static readonly List<WeakReference<Action<ThemeChangedEventArgs>>> _handlers = new List<WeakReference<Action<ThemeChangedEventArgs>>>();




        public static readonly DependencyProperty ThemeNameProperty =
            DependencyProperty.RegisterAttached(
                "ThemeName",
                typeof(string),
                typeof(ThemeManager),
                new PropertyMetadata(default(string), OnThemeNameChanged));

        public static void RegisterThemeChangedHandler(Action<ThemeChangedEventArgs> handler)
        {
            _handlers.Add(new WeakReference<Action<ThemeChangedEventArgs>>(handler));
        }

        public static void UnregisterThemeChangedHandler(Action<ThemeChangedEventArgs> handler)
        {
            _handlers.RemoveAll(reference =>
                reference.TryGetTarget(out var target) && target == handler);
        }

        public static void SetThemeName(DependencyObject element, string value)
        {
            element.SetValue(ThemeNameProperty, value);
        }

        public static string GetThemeName(DependencyObject element)
        {
            return (string)element.GetValue(ThemeNameProperty);
        }

        private static void OnThemeNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newThemeName = e.NewValue as string;
            if(String.IsNullOrEmpty(newThemeName))
            {
                return;
            }
            Logger.Log($"OnThemeNameChanged {newThemeName}");
            var elementsToUpdate = LogicalTreeHelperExtensions.FindElementsWithAttachedProperty(d, VsColorsDynamicResource2Extension.ThemedProperty).ToList();
            Logger.Log($"Found {elementsToUpdate.Count} elements to update");

            foreach(var el in elementsToUpdate)
            {
                try
                {
                    if(el == null)
                    {
                        //Logger.Log("Null element");
                        continue;
                    }
                    var fe = el as FrameworkElement;
                    if (fe == null)
                    {
                        //Logger.Log($"Not a framework element - {el.GetType().FullName} ");
                        continue;
                    }
                    
                    Logger.Log($"Updating all listeners");
                    var listeners = VsColorsDynamicResource2Extension.GetThemed(el);
                    if (listeners != null)
                    {
                        listeners.ForEach(listener => listener.Update(newThemeName));
                        Logger.Log($"Updated");
                    }
                    else
                    {
                        Logger.Log("Null to update");
                    }
                }
                catch (Exception exc)
                {
                    Logger.Log($"Error updating {exc.Message}");
                    Logger.Log($"Error updating {exc.StackTrace}");
                }
            };


            //var args = new ThemeChangedEventArgs(d, newTheme);

            //// Notify all valid handlers
            //Logger.Log($"Handlers before removing stale  - ${_handlers.Count}");
            //_handlers.RemoveAll(reference =>
            //    !reference.TryGetTarget(out var target));
            //Logger.Log($"Theme changed to {newTheme} - num handlers {_handlers.Count}");
            //foreach (var reference in _handlers)
            //{
            //    if (reference.TryGetTarget(out var target))
            //    {
            //        target(args);
            //    }
            //}
        }
    }


    //public static class ThemeResourceKeyConverter
    //{
    //    public static ThemesResourceKeyType Convert(Microsoft.VisualStudio.Shell.ThemeResourceKeyType themeResourceKeyType)
    //    {
    //        switch (themeResourceKeyType)
    //        {
    //            case Microsoft.VisualStudio.Shell.ThemeResourceKeyType.BackgroundBrush:
    //                return ThemesResourceKeyType.BackgroundBrush;
    //            case Microsoft.VisualStudio.Shell.ThemeResourceKeyType.ForegroundBrush:
    //                return ThemesResourceKeyType.ForegroundBrush;
    //            case Microsoft.VisualStudio.Shell.ThemeResourceKeyType.BackgroundColor:
    //                return ThemesResourceKeyType.BackgroundColor;
    //            case Microsoft.VisualStudio.Shell.ThemeResourceKeyType.ForegroundColor:
    //                return ThemesResourceKeyType.ForegroundColor;
    //            default:
    //                throw new ArgumentOutOfRangeException(nameof(themeResourceKeyType), themeResourceKeyType, null);
    //        }
    //    }
    //    public static ThemesResourceKey Convert(Microsoft.VisualStudio.Shell.ThemeResourceKey themeResourceKey)
    //    {
    //        return new ThemesResourceKey(
    //            themeResourceKey.Category,
    //            themeResourceKey.Name,
    //            Convert(themeResourceKey.KeyType)
    //        );
    //    }
    //}

    public static class ThemeService
    {
        private static Dictionary<string, Dictionary<ThemeResourceKey, Color>> _themeColors = new Dictionary<string, Dictionary<ThemeResourceKey, Color>>();
        // will write out to file the number of times this is loaded
        static ThemeService()
        {
            Logger.Log("Loading xml");
            var root = XElement.Load(@"C:\Users\tonyh\Downloads\themes.xml");
            foreach (var themeElement in root.Elements())
            {
                var themeName = themeElement.Attribute("Name").Value;
                var themeColors = new Dictionary<ThemeResourceKey, Color>();
                foreach (var themeResourceKeyColorElement in themeElement.Elements())
                {
                    var color = (Color)ColorConverter.ConvertFromString(themeResourceKeyColorElement.Attribute("Color").Value);
                    var category = themeResourceKeyColorElement.Attribute("Category").Value;
                    var keyType = themeResourceKeyColorElement.Attribute("KeyType").Value;
                    var name = themeResourceKeyColorElement.Attribute("Name").Value;
                    var themeResourceKey = new ThemeResourceKey(new Guid(category), name, (ThemeResourceKeyType)Enum.Parse(typeof(ThemeResourceKeyType), keyType));
                    themeColors.Add(themeResourceKey, color);
                }
                _themeColors.Add(themeName, themeColors);
            }


        }
        public static Color GetColor(string themeName, ThemeResourceKey resourceKey)
        {
            return _themeColors[themeName][resourceKey];
        }
    }

    public static class Logger
    {
        public static void Clear()
        {
            System.IO.File.WriteAllText(@"C:\Users\tonyh\Downloads\log.txt", string.Empty);
        }
        public static void Log(string message)
        {
            System.IO.File.AppendAllText(@"C:\Users\tonyh\Downloads\log.txt", message + Environment.NewLine);
        }
    }

    public interface IUpdateMe
    {
        void Update(string newThemeName);
    }

    [TypeConverter(typeof(DynamicResourceExtensionConverter))]
    [MarkupExtensionReturnType(typeof(object))]
    public class VsColorsDynamicResource2Extension : DynamicResourceExtension, INotifyPropertyChanged, IUpdateMe
    {
        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged(string propertyName) =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty ThemedProperty =
            DependencyProperty.RegisterAttached(
                "Themed",
                typeof(List<IUpdateMe>),
                typeof(VsColorsDynamicResource2Extension),
                new PropertyMetadata(null));
        public static void SetThemed(DependencyObject element, List<IUpdateMe> value)
        {
            element.SetValue(ThemedProperty, value);
        }

        public static List<IUpdateMe> GetThemed(DependencyObject element)
        {
            return (List<IUpdateMe>)element.GetValue(ThemedProperty);
        }


        private Microsoft.VisualStudio.Shell.ThemeResourceKey _themeResourceKey;
        public VsColorsDynamicResource2Extension()
        {
        }

        /// <summary>
        ///  Constructor that takes the resource key that this is a static reference to.
        /// </summary>
        public VsColorsDynamicResource2Extension(
            object resourceKey) : base(resourceKey)
        {
            Logger.Log("VsColorsDynamicResource2Extension ctor");
            ThemeManager.RegisterThemeChangedHandler(ThemeManagerThemeChanged);
            _themeResourceKey = resourceKey as Microsoft.VisualStudio.Shell.ThemeResourceKey;
            if (_themeResourceKey == null)
            {
                throw new ArgumentException("resourceKey must be of type ThemeResourceKey");
            }
        }

        private void ThemeManagerThemeChanged(ThemeChangedEventArgs e)
        {
            Logger.Log($"Handled event!");
            var isAffectedByThemeChange = IsAffectedByThemeChange(e.SourceElement);

            if (isAffectedByThemeChange)
            {
                themeName = e.NewThemeName;
                OnValueChanged();
            }
        }

        private void OnValueChanged()
        {
            OnPropertyChanged(nameof(Value));
        }

        private bool IsAffectedByThemeChange(DependencyObject element)
        {
            // Traverse up the tree to check if the theme element is relevant
            if (!weakTargetObject.TryGetTarget(out var targetObject))
            {
                return false;
            }
            DependencyObject current = targetObject;
            while (current != null)
            {
                if (current == element)
                    return true;
                current = LogicalTreeHelper.GetParent(current);
            }
            return false;
        }

        private string themeName;
        private string GetThemeName()
        {
            var defaultThemeName = "Light";
            if (themeName == null)
            {
                //if (true) { 
                if (!weakTargetObject.TryGetTarget(out var targetObject))
                {
                    return defaultThemeName;
                }
                DependencyObject current = targetObject;
                while (current != null)
                {
                    var themeName = ThemeManager.GetThemeName(current);
                    if (themeName != null)
                    {
                        return themeName;
                    }

                    current = LogicalTreeHelper.GetParent(current);
                }
                return defaultThemeName;
            }
            return themeName;

        }

        private object ProvideValueActual()
        {
            Logger.Log("ProvideValueActual**");
            var designTimeTheme = GetThemeName();
            Logger.Log($"Theme name {designTimeTheme}");
            var color = ThemeService.GetColor(designTimeTheme, _themeResourceKey);
            Logger.Log($"Color {color}");
            Logger.Log("");
            if (_themeResourceKey.KeyType == Microsoft.VisualStudio.Shell.ThemeResourceKeyType.ForegroundBrush || _themeResourceKey.KeyType == Microsoft.VisualStudio.Shell.ThemeResourceKeyType.BackgroundBrush)
            {
                return new SolidColorBrush(color);
            }
            return color;
        }

        public object Value => ProvideValueActual();

        private WeakReference<FrameworkElement> weakTargetObject;
        private WeakReference<DependencyProperty> weakTargetProperty;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Logger.Log("VsColorsDynamicResource2Extension ProvideValue");
            if (_themeResourceKey == null) throw new InvalidOperationException("ThemeResourceKey is null");

            var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var targetObject = provideValueTarget.TargetObject as FrameworkElement;
            var targetProperty = provideValueTarget.TargetProperty as DependencyProperty;
            var rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            var rootObject = rootObjectProvider.RootObject as DependencyObject;
            if (!DesignerProperties.GetIsInDesignMode(rootObject))
            {
                return base.ProvideValue(serviceProvider);
            }
            Logger.Log("VsColorsDynamicResource2Extension Designer ProvideValue");
            try
            {
                var listeners = GetThemed(targetObject);
                if (listeners == null)
                {
                    listeners = new List<IUpdateMe>();
                    SetThemed(targetObject, listeners);
                }
                if (!listeners.Contains(this))
                {
                    listeners.Add(this);
                }
            }
            catch (Exception exc)
            {
                Logger.Log($"Error updating {exc.Message}");
                Logger.Log($"Error updating {exc.StackTrace}");
            }
            weakTargetObject = new WeakReference<FrameworkElement>(targetObject);
            weakTargetProperty = new WeakReference<DependencyProperty>(targetProperty);
            var binding = new Binding(nameof(Value))
            {
                Source = this,
                Mode = BindingMode.OneWay
            };

            return binding.ProvideValue(serviceProvider);

        }

        public void Update(string newThemeName)
        {
            themeName = newThemeName;
            Logger.Log($"Update - {themeName}");
            OnValueChanged();
            //if (weakTargetObject.TryGetTarget(out var targetObject) && weakTargetProperty.TryGetTarget(out var targetProperty))
            //{
            //    Logger.Log("Invalidating property");
            //    targetObject.InvalidateProperty(targetProperty);

            //}
        }
    }

    [TypeConverter(typeof(DynamicResourceExtensionConverter))]
    [MarkupExtensionReturnType(typeof(object))]
    public class VsColorsDynamicResourceExtension : DynamicResourceExtension
    {
        private Microsoft.VisualStudio.Shell.ThemeResourceKey _themeResourceKey;
        public VsColorsDynamicResourceExtension()
        {
        }

        /// <summary>
        ///  Constructor that takes the resource key that this is a static reference to.
        /// </summary>
        public VsColorsDynamicResourceExtension(
            object resourceKey) : base(resourceKey)
        {
            _themeResourceKey = resourceKey as Microsoft.VisualStudio.Shell.ThemeResourceKey;
            if (_themeResourceKey == null)
            {
                throw new ArgumentException("resourceKey must be of type ThemeResourceKey");
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_themeResourceKey == null) throw new InvalidOperationException("ThemeResourceKey is null");
            var rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;

            if (DesignerProperties.GetIsInDesignMode(rootObjectProvider.RootObject as DependencyObject))
            {
                var designTimeTheme = GetDesignTimeTheme(serviceProvider);
                var color = ThemeService.GetColor(designTimeTheme, _themeResourceKey);

                if (_themeResourceKey.KeyType == Microsoft.VisualStudio.Shell.ThemeResourceKeyType.ForegroundBrush || _themeResourceKey.KeyType == Microsoft.VisualStudio.Shell.ThemeResourceKeyType.BackgroundBrush)
                {
                    return new SolidColorBrush(color);
                }
                return color;
            }
            else
            {
                return base.ProvideValue(serviceProvider);
            }
        }

        private static string GetDesignTimeTheme(IServiceProvider serviceProvider)
        {
            var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            if (provideValueTarget?.TargetObject is FrameworkElement element)
            {
                // Direct resource lookup for the theme
                if (element.TryFindResource("themeName") is string themeName)
                {
                    return themeName;
                }
            }

            return "Light"; // Default theme fallback
        }
    }
}

