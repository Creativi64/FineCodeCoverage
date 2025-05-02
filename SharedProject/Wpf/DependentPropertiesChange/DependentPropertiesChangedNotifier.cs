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
        {
            this.dependentPropertiesDescriptors = dependentPropertiesDescriptors;
        }

        private EventHandler CreateHandler(T instance, DependentPropertiesDescriptor dependentPropertiesDescriptor)
        {
            EventHandler handler = (_, __) =>
            {
                var dependentPropertyNames = dependentPropertiesDescriptor.GetDependentProperties();
                foreach (var propertyName in dependentPropertyNames)
                {
                    instance.NotifyDependentPropertyChanged(propertyName);
                }
            };
            return handler;
        }

        public void NotifyOfChanges(T instance)
        {
            var instanceHandlers = new List<EventHandler>();
            handlers.Add(instance, instanceHandlers);
            this.dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                var handler = CreateHandler(instance, dependentPropertiesDescriptor);
                instanceHandlers.Add(handler);
                dependentPropertiesDescriptor.AddValueChanged(instance, handler);
            });
        }

        public void RemoveNotificationOfChanges(T instance)
        {
            if (!handlers.ContainsKey(instance)) return;
            var instanceHandlers = handlers[instance];
            var index = 0;
            this.dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                dependentPropertiesDescriptor.RemoveValueChanged(instance, instanceHandlers[index]);
                index++;
            });
            handlers.Remove(instance);
            RemoveStale();
        }

       private void RemoveStale()
        {
            var removals = handlers.Keys.Where(k => k.DataContext != null && k.DataContext == BindingOperations.DisconnectedSource).ToList();
            removals.ForEach(removal => handlers.Remove(removal));
        }
    }
}
