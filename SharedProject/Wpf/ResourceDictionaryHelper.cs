using System;
using System.Reflection;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public static class ResourceDictionaryHelper
    {
        public static ResourceDictionary FromExecutingAssemembly(string resourcePath)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var name = executingAssembly.GetName().Name;
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/{name};component/{resourcePath}", UriKind.Absolute)
            };
            return resourceDictionary;
        }

        public static void AddFromExecutingAssembly(this ResourceDictionary resourceDictionary, string resourcePath)
        {

            resourceDictionary.MergedDictionaries.Add(FromExecutingAssemembly(resourcePath));
        }
    }
}
