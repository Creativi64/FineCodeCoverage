using System;
using System.ComponentModel.Composition;
using System.Reflection;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IReflectionService))]
    internal class ReflectionService : IReflectionService
    {
        public object GetPropertyValue(PropertyInfo propertyInfo, object instance)
        {
            return propertyInfo.GetValue(instance);
        }

        public PropertyInfo[] GetPublicProperties(Type type)
        {
            return type.GetPublicProperties();
        }

        public void SetPropertyValue(PropertyInfo propertyInfo, object instance, object value)
        {
            propertyInfo.SetValue(instance, value);
        }
    }
}
