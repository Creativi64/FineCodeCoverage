using System;
using System.ComponentModel.Composition;
using System.Reflection;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IReflectionService))]
    internal sealed class ReflectionService : IReflectionService
    {
        public object GetPropertyValue(PropertyInfo propertyInfo, object instance)
            => propertyInfo.GetValue(instance);

        public PropertyInfo[] GetPublicProperties(Type type)
            => type.GetPublicProperties();

        public void SetPropertyValue(PropertyInfo propertyInfo, object instance, object value)
            => propertyInfo.SetValue(instance, value);
    }
}
