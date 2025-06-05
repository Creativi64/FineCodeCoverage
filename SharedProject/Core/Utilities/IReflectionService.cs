using System;
using System.Reflection;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IReflectionService
    {
        void SetPropertyValue(PropertyInfo propertyInfo, object instance, object value);

        object GetPropertyValue(PropertyInfo propertyInfo, object instance);

        PropertyInfo[] GetPublicProperties(Type type);
    }
}
