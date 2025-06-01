using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace FineCodeCoverage.Wpf
{
    public class DependentPropertiesChangedNotifier<T> where T : FrameworkElement, IPropertyDependencyChanged
    {
        private readonly List<DependentPropertiesDescriptor> dependentPropertiesDescriptors;
        private readonly Dictionary<T, List<EventHandler>> handlers = new Dictionary<T, List<EventHandler>>();

        public DependentPropertiesChangedNotifier(List<DependentPropertiesDescriptor> dependentPropertiesDescriptors)
            => this.dependentPropertiesDescriptors = dependentPropertiesDescriptors;

        private static EventHandler CreateHandler(T instance, DependentPropertiesDescriptor dependentPropertiesDescriptor)
            => (_, __) =>
        {
            IEnumerable<string> dependentPropertyNames = dependentPropertiesDescriptor.GetDependentProperties();
            foreach (string propertyName in dependentPropertyNames)
            {
                instance.NotifyDependentPropertyChanged(propertyName);
            }
        };

        public void NotifyOfChanges(T instance)
        {
            var instanceHandlers = new List<EventHandler>();
            this.handlers.Add(instance, instanceHandlers);
            this.dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                EventHandler handler = CreateHandler(instance, dependentPropertiesDescriptor);
                instanceHandlers.Add(handler);
                dependentPropertiesDescriptor.AddValueChanged(instance, handler);
            });
        }

        public void RemoveNotificationOfChanges(T instance)
        {
            if (!this.handlers.ContainsKey(instance)) return;
            List<EventHandler> instanceHandlers = this.handlers[instance];
            int index = 0;
            this.dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                dependentPropertiesDescriptor.RemoveValueChanged(instance, instanceHandlers[index]);
                index++;
            });
            _ = this.handlers.Remove(instance);
            this.RemoveStale();
        }

        private void RemoveStale()
        {
            var removals = this.handlers.Keys.Where(k => k.DataContext != null && k.DataContext == BindingOperations.DisconnectedSource).ToList();
            removals.ForEach(removal => this.handlers.Remove(removal));
        }
    }
}
