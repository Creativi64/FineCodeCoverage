using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace FineCodeCoverage.Wpf
{
    public class DependentPropertiesChangedNotifier<T>
        where T : FrameworkElement, IPropertyDependencyChanged
    {
        private readonly List<DependentPropertiesDescriptor> _dependentPropertiesDescriptors;
        private readonly Dictionary<T, List<EventHandler>> _handlers = new Dictionary<T, List<EventHandler>>();

        public DependentPropertiesChangedNotifier(List<DependentPropertiesDescriptor> dependentPropertiesDescriptors)
            => _dependentPropertiesDescriptors = dependentPropertiesDescriptors;

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
            _handlers.Add(instance, instanceHandlers);
            _dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                EventHandler handler = CreateHandler(instance, dependentPropertiesDescriptor);
                instanceHandlers.Add(handler);
                dependentPropertiesDescriptor.AddValueChanged(instance, handler);
            });
        }

        public void RemoveNotificationOfChanges(T instance)
        {
            if (!_handlers.ContainsKey(instance))
            {
                return;
            }

            List<EventHandler> instanceHandlers = _handlers[instance];
            int index = 0;
            _dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                dependentPropertiesDescriptor.RemoveValueChanged(instance, instanceHandlers[index]);
                index++;
            });
            _ = _handlers.Remove(instance);
            RemoveStale();
        }

        private void RemoveStale()
        {
            var removals = _handlers.Keys.Where(k => k.DataContext != null && k.DataContext == BindingOperations.DisconnectedSource).ToList();
            removals.ForEach(removal => _handlers.Remove(removal));
        }
    }
}
