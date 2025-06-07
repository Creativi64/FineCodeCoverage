using System;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Core.Utilities
{
    public static class ReflectionExtensions
    {
        public static TCustomAttribute[] GetTypedCustomAttributes<TCustomAttribute>(
            this ICustomAttributeProvider customAttributeProvider,
            bool inherit)
            where TCustomAttribute : Attribute
        {
            object[] attributes = customAttributeProvider.GetCustomAttributes(typeof(TCustomAttribute), inherit);
            return attributes as TCustomAttribute[];
        }

        public static PropertyInfo[] GetPublicProperties(this Type type)
            => !type.IsInterface
                ? type.GetProperties()
                : (new Type[] { type })
                   .Concat(type.GetInterfaces())
                   .SelectMany(i => i.GetProperties()).ToArray();
    }
}
