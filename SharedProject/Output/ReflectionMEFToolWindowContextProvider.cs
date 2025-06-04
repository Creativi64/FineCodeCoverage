using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output
{
    internal static class ReflectionMEFToolWindowContextProvider
    {
        private static readonly MethodInfo s_getServiceMethod;
        private static Dictionary<Guid, bool> s_toolWindowsWithContext;

        static ReflectionMEFToolWindowContextProvider()
            => s_getServiceMethod = typeof(IComponentModel).GetMethod("GetService");

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
                MethodInfo getService = s_getServiceMethod.MakeGenericMethod(propertyType);
                contextProperty.SetValue(context, getService.Invoke(ComponentModel, Array.Empty<object>()));
            }

            return context;
        }

        private static void SetToolWindowsWithContext(Type packageType)
        {
            s_toolWindowsWithContext = new Dictionary<Guid, bool>();
            IEnumerable<ProvideToolWindowAttribute> provideToolWindowAttributes = packageType.GetCustomAttributes<ProvideToolWindowAttribute>();
            foreach (ProvideToolWindowAttribute provideToolWindowAttribute in provideToolWindowAttributes)
            {
                Type toolWindowType = provideToolWindowAttribute.ToolType;
                s_toolWindowsWithContext.Add(toolWindowType.GUID, toolWindowType.GetConstructors().Any(c => c.GetParameters().Length == 1));
            }
        }

        public static bool IsToolWindowWithContext(Type packageType, Guid toolWindowType)
        {
            if (s_toolWindowsWithContext == null)
            {
                SetToolWindowsWithContext(packageType);
            }

            bool isPackageToolWindow = s_toolWindowsWithContext.TryGetValue(toolWindowType, out bool isToolWindowWithContext);
            return isPackageToolWindow && isToolWindowWithContext;
        }
    }
}
