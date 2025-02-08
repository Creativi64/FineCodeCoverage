using System;
using System.Windows.Data;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Wpf
{
    public static class DesignTimeVsColorsResourceProvider
    {
        public static object ProvideValue(ThemeResourceKey themeResourceKey,DependencyObject targetObject, IServiceProvider serviceProvider)
        {
            var designTimeVsColorsResource = new DesignTimeVsColorsResource(targetObject, themeResourceKey, "Light");
            var binding = new Binding(nameof(DesignTimeVsColorsResource.DesignTimeValue))
            {
                Source = designTimeVsColorsResource,
                Mode = BindingMode.OneWay
            };

            return binding.ProvideValue(serviceProvider);
        }

    }
}
