using System;
using System.Collections.Generic;
using System.Windows;

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
}
