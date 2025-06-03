using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

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

        private static IComponentModel ComponentModel
        {
            get
            {
                IComponentModel componentModel = TestComponentModel ?? VsComponentModel;
                return componentModel ?? throw new InvalidOperationException("IComponentModel service not available.");
            }
        }

        public static T Get<T>() where T : class => ComponentModel.GetService<T>();

        public static IEnumerable<T> GetAll<T>() where T : class => ComponentModel.GetExtensions<T>();

    }
}