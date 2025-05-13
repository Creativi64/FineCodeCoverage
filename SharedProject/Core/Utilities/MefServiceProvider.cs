using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class MefServiceProvider
    {
        private static IComponentModel vsComponentModel;
        private static bool triedGetVsComponentModel;

        // Property to override the default IComponentModel for testing purposes
        public static IComponentModel TestComponentModel { get; set; }

        private static IComponentModel VsComponentModel
        {
            get
            {
                if (triedGetVsComponentModel)
                {
                    return vsComponentModel;
                }
                vsComponentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                triedGetVsComponentModel = true;
                return vsComponentModel;
            }
        }

        public static T Get<T>() where T : class
        {
            // If a test-specific component model is set, use it
            var componentModel = TestComponentModel ?? VsComponentModel;
            return componentModel == null
                ? throw new InvalidOperationException("IComponentModel service not available.")
                : componentModel.GetService<T>();
        }

        public static IEnumerable<T> GetAll<T>() where T : class
        {
            // If a test-specific component model is set, use it
            var componentModel = TestComponentModel ?? VsComponentModel;
            return componentModel == null
                ? throw new InvalidOperationException("IComponentModel service not available.")
                : componentModel.GetExtensions<T>();
        }

    }

}
