using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FineCodeCoverage.Wpf
{
    public class DependentPropertiesDescriptor
    {
        private readonly DependencyPropertyDescriptor _dependencyPropertyDescriptor;
        private readonly Func<string, IEnumerable<string>> _getDependentProperties;

        public DependentPropertiesDescriptor(
            DependencyPropertyDescriptor dependencyPropertyDescriptor,
            Func<string, IEnumerable<string>> getDependentProperties
            )
        {
            _dependencyPropertyDescriptor = dependencyPropertyDescriptor;
            _getDependentProperties = getDependentProperties;
        }

        public void AddValueChanged(object instance, EventHandler handler)
            => _dependencyPropertyDescriptor.AddValueChanged(instance, handler);

        public void RemoveValueChanged(object instance, EventHandler handler)
            => _dependencyPropertyDescriptor.RemoveValueChanged(instance, handler);

        public IEnumerable<string> GetDependentProperties()
            => _getDependentProperties(_dependencyPropertyDescriptor.Name);
    }
}
