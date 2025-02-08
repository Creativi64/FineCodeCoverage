using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using IServiceProvider = System.IServiceProvider;

namespace FineCodeCoverage.Wpf
{
    [TypeConverter(typeof(DynamicResourceExtensionConverter))]
    [MarkupExtensionReturnType(typeof(object))]
    public class VsColorsDynamicResourceExtension : DynamicResourceExtension
    {
        private ThemeResourceKey _themeResourceKey;

        public VsColorsDynamicResourceExtension()
        {
        }

        public VsColorsDynamicResourceExtension(
            object resourceKey) : base(resourceKey)
        {
            _themeResourceKey = resourceKey as ThemeResourceKey;
            if (_themeResourceKey == null)
            {
                throw new ArgumentException("resourceKey must be of type ThemeResourceKey");
            }
        }
        
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_themeResourceKey == null) throw new InvalidOperationException("ThemeResourceKey is null");

            var targetObject = GetTargetDependencyObject(serviceProvider);
            if (!DesignerProperties.GetIsInDesignMode(targetObject))
            {
                return base.ProvideValue(serviceProvider);
            }
            return DesignTimeVsColorsResourceProvider.ProvideValue(_themeResourceKey, targetObject, serviceProvider);
        }

        private static DependencyObject GetTargetDependencyObject(IServiceProvider serviceProvider)
        {
            var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            return provideValueTarget.TargetObject as DependencyObject;
        }
    }

}

