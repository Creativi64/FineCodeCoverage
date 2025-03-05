using System;
using System.Reflection;
using Microsoft.VisualStudio.ComponentModelHost;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output
{
    internal static class ReflectionMEFToolWindowContextProvider
    {
        private static readonly MethodInfo GetServiceMethod;
        static ReflectionMEFToolWindowContextProvider()
        {
            GetServiceMethod = typeof(IComponentModel).GetMethod("GetService");
        }
        public static IComponentModel ComponentModel { get; set; }
        public static TContext GetToolWindowContext<TToolWindowType, TContext>() => (TContext)GetToolWindowContext(typeof(TToolWindowType));
        public static object GetToolWindowContext(Type toolWindowType)
        {
            ConstructorInfo contextConstructor = toolWindowType.GetConstructors().First(c => c.GetParameters().Length == 1);
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
        private static Dictionary<Guid, bool> toolWindowsWithContext;
        private static void SetToolWindowsWithContext(Type packageType)
        {
            toolWindowsWithContext = new Dictionary<Guid, bool>();
            var provideToolWindowAttributes = packageType.GetCustomAttributes<ProvideToolWindowAttribute>();
            foreach(var provideToolWindowAttribute in provideToolWindowAttributes)
            {
                var toolWindowType = provideToolWindowAttribute.ToolType;
                toolWindowsWithContext.Add(toolWindowType.GUID, toolWindowType.GetConstructors().Any(c => c.GetParameters().Length == 1));
            }
        }

        public static bool IsToolWindowWithContext(Type packageType, Guid toolWindowType)
        {
            if(toolWindowsWithContext == null)
            {
                SetToolWindowsWithContext(packageType);
            }
            var isPackageToolWindow = toolWindowsWithContext.TryGetValue(toolWindowType, out bool isToolWindowWithContext);
            return isPackageToolWindow && isToolWindowWithContext;
        }
    }
}
