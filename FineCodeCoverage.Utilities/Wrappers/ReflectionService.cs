using System;
using System.ComponentModel.Composition;
using System.Reflection;
using FineCodeCoverage.Utilities.Extensions;

namespace FineCodeCoverage.Utilities.Wrappers
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
