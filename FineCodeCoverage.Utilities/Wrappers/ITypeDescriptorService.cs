using System;
using System.ComponentModel;

namespace FineCodeCoverage.Utilities.Wrappers
{
    internal interface ITypeDescriptorService
    {
        void SetPropertyValue(PropertyDescriptor propertyDescriptor, object instance, object value);

        object GetPropertyValue(PropertyDescriptor propertyDescriptor, object instance);

        PropertyDescriptorCollection GetProperties(Type type);
    }
}
