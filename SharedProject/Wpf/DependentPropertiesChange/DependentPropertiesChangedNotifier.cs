using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Wpf
{
    public class DependentPropertiesChangedNotifier<T> where T : IPropertyDependencyChanged
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
            var instanceHandlers = handlers[instance];
            var index = 0;
            this.dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                dependentPropertiesDescriptor.RemoveValueChanged(instance, instanceHandlers[index]);
                index++;
            });
            handlers.Remove(instance);
        }
    }

}
