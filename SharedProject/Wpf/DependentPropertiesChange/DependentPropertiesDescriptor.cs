using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FineCodeCoverage.Wpf
{
    public class DependentPropertiesDescriptor
    {
        private readonly DependencyPropertyDescriptor dependencyPropertyDescriptor;
        private readonly Func<string, IEnumerable<string>> _getDependentProperties;
        public DependentPropertiesDescriptor(
            DependencyPropertyDescriptor dependencyPropertyDescriptor,
            Func<string, IEnumerable<string>> getDependentProperties
            )
        {
            this.dependencyPropertyDescriptor = dependencyPropertyDescriptor;
            this._getDependentProperties = getDependentProperties;
        }

        public void AddValueChanged(object instance, EventHandler handler)
            => this.dependencyPropertyDescriptor.AddValueChanged(instance, handler);

        public void RemoveValueChanged(object instance, EventHandler handler)
            => this.dependencyPropertyDescriptor.RemoveValueChanged(instance, handler);

        public IEnumerable<string> GetDependentProperties()
            => this._getDependentProperties(this.dependencyPropertyDescriptor.Name);
    }
}
