using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(ITypeDescriptorService))]
    internal class TypeDescriptorService : ITypeDescriptorService
    {
        public PropertyDescriptorCollection GetProperties(Type type)
        {
            return TypeDescriptor.GetProperties(type);
        }

        public object GetPropertyValue(PropertyDescriptor propertyDescriptor, object instance)
        {
            return propertyDescriptor.GetValue(instance);
        }

        public void SetPropertyValue(PropertyDescriptor propertyDescriptor, object instance, object value)
        {
            propertyDescriptor.SetValue(instance, value);
        }
    }

}
