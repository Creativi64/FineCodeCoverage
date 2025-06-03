using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Utilities;

namespace FineCodeCoverage.Wpf
{
    public static class DependentPropertiesChangedNotifierBuilder
    {
        public static DependentPropertiesChangedNotifier<T> Build<T>() where T : FrameworkElement, IPropertyDependencyChanged
        {
            Type type = typeof(T);
            IDictionary<string, OneOrMany<string>> dependencies = BuildPropertyDependencies(type);
            List<DependencyProperty> dependencyProperties = GetDependencyProperties(type);
            var allDescriptorDependents = dependencies.Select(kvp =>
            {
                DependencyProperty dependedUponDp = dependencyProperties.FirstOrDefault(dp => dp.Name == kvp.Key);
                if (dependedUponDp == null) return null;
                var descriptor = DependencyPropertyDescriptor.FromProperty(dependedUponDp, type);
                return new DependentPropertiesDescriptor(descriptor, (changedPropertyName) => GetDependentProperties(changedPropertyName, dependencies));
            }).Where(dpd => dpd != null).ToList();
            return new DependentPropertiesChangedNotifier<T>(allDescriptorDependents);
        }

        private static List<DependencyProperty> GetDependencyProperties(Type type)
            => type.GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(f => f.FieldType == typeof(DependencyProperty))
                .Select(f => f.GetValue(null) as DependencyProperty).ToList();

        private static IDictionary<string, OneOrMany<string>> BuildPropertyDependencies(Type type)
        {
            IDictionary<string, OneOrMany<string>> map1 = null;
            IDictionary<string, OneOrMany<string>> map2 = null;
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var stringSet = new HashSet<string>(properties.Select(prop => prop.Name));
            foreach (PropertyInfo propertyInfo in properties)
            {
                foreach (string str in propertyInfo.GetCustomAttributes(typeof(DependsOnPropertyAttribute), true).Cast<DependsOnPropertyAttribute>().Select(attr => attr.PropertyName))
                {
                    if (!stringSet.Contains(str))
                        throw new DependsOnPropertyNotFoundException(propertyInfo.DeclaringType, propertyInfo.Name, str);
                    AddToMapValues(ref map2, str, propertyInfo.Name);
                    AddToMapValues(ref map1, propertyInfo.Name, str);
                }
            }

            ValidatePropertyDependencies(map1);
            return map2;

            void AddToMapValues(
              ref IDictionary<string, OneOrMany<string>> map,
              string key,
              string valueToAdd)
            {
                _ = (map ?? (map = new Dictionary<string, OneOrMany<string>>()))
                    .TryGetValue(key, out OneOrMany<string> oneOrMany);
                oneOrMany.Add(valueToAdd);
                map[key] = oneOrMany;
            }
        }

        private static void ValidatePropertyDependencies(
  IDictionary<string, OneOrMany<string>> propertyDependencies)
        {
            if (propertyDependencies == null)
                return;
            var allDependentProperties = new List<string>();
            foreach (string key in (IEnumerable<string>)propertyDependencies.Keys)
            {
                allDependentProperties.Clear();
                _ = AddDependentProperties(key, key, propertyDependencies, ref allDependentProperties);
            }
        }

        private static bool AddDependentProperties(
          string rootProperty,
          string property,
          IDictionary<string, OneOrMany<string>> propertyDependencies,
          ref List<string> allDependentProperties)
        {
            if (propertyDependencies == null || !propertyDependencies.TryGetValue(property, out OneOrMany<string> oneOrMany))
                return false;
            foreach (string str in oneOrMany)
            {
                if (allDependentProperties == null)
                    allDependentProperties = new List<string>();
                if (!allDependentProperties.Contains(str))
                {
                    allDependentProperties.Add(str);
                    if (str == rootProperty)
                        throw new CircularPropertyDependencyException(str, allDependentProperties.ToArray());
                    _ = AddDependentProperties(rootProperty, str, propertyDependencies, ref allDependentProperties);
                }
                else
                {
                    break;
                }
            }

            return true;
        }

        private static IEnumerable<string> GetDependentProperties(
            string property, IDictionary<string, OneOrMany<string>> _propertyDependencies)
        {
            List<string> allDependentProperties = null;
            return !AddDependentProperties(property, property, _propertyDependencies, ref allDependentProperties) ? Enumerable.Empty<string>() : allDependentProperties;
        }
    }
}