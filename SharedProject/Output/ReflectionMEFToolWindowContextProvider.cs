using System;
using System.Reflection;
using Microsoft.VisualStudio.ComponentModelHost;
using System.Linq;

namespace FineCodeCoverage.Output
{
    internal static class ReflectionMEFToolWindowContextProvider
    {
        private static MethodInfo GetServiceMethod;
        static ReflectionMEFToolWindowContextProvider()
        {
            GetServiceMethod = typeof(IComponentModel).GetMethod("GetService");
        }
        public static IComponentModel ComponentModel { get; set; }
        public static TContext GetToolWindowContext<TToolWindowType, TContext>() => (TContext)GetToolWindowContext(typeof(TToolWindowType));
        public static object GetToolWindowContext(Type toolWindowType)
        {
            ConstructorInfo contextConstructor = toolWindowType.GetConstructors().Where(c => c.GetParameters().Length == 1).First();
            Type contextType = contextConstructor.GetParameters().First().ParameterType;
            object context = Activator.CreateInstance(contextType);
            foreach (PropertyInfo contextProperty in contextType.GetProperties())
            {
                Type propertyType = contextProperty.PropertyType;
                MethodInfo getService = GetServiceMethod.MakeGenericMethod(propertyType);
                contextProperty.SetValue(context, getService.Invoke(ComponentModel, new object[] { }));
            }

            return context;
        }

    }
}
