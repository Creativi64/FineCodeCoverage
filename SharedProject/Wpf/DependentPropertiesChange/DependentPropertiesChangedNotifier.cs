using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace FineCodeCoverage.Wpf
{
    public class DependentPropertiesChangedNotifier<T> where T : FrameworkElement, IPropertyDependencyChanged
    {
        private readonly List<DependentPropertiesDescriptor> _dependentPropertiesDescriptors;
        private readonly Dictionary<T, List<EventHandler>> _handlers = new Dictionary<T, List<EventHandler>>();

        public DependentPropertiesChangedNotifier(List<DependentPropertiesDescriptor> dependentPropertiesDescriptors)
            => this._dependentPropertiesDescriptors = dependentPropertiesDescriptors;

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
            this._handlers.Add(instance, instanceHandlers);
            this._dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                EventHandler handler = CreateHandler(instance, dependentPropertiesDescriptor);
                instanceHandlers.Add(handler);
                dependentPropertiesDescriptor.AddValueChanged(instance, handler);
            });
        }

        public void RemoveNotificationOfChanges(T instance)
        {
            if (!this._handlers.ContainsKey(instance)) return;
            List<EventHandler> instanceHandlers = this._handlers[instance];
            int index = 0;
            this._dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                dependentPropertiesDescriptor.RemoveValueChanged(instance, instanceHandlers[index]);
                index++;
            });
            _ = this._handlers.Remove(instance);
            this.RemoveStale();
        }

        private void RemoveStale()
        {
            var removals = this._handlers.Keys.Where(k => k.DataContext != null && k.DataContext == BindingOperations.DisconnectedSource).ToList();
            removals.ForEach(removal => this._handlers.Remove(removal));
        }
    }
}