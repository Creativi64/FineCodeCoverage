using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class MefServiceProvider
    {
        private static IComponentModel _testComponentModel;
        private static IComponentModel vsComponentModel;
        private static bool triedGetVsComponentModel;

        // Property to override the default IComponentModel for testing purposes
        public static IComponentModel TestComponentModel
        {
            get => _testComponentModel;
            set => _testComponentModel = value;
        }

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
            if (componentModel == null)
            {
                throw new InvalidOperationException("IComponentModel service not available.");
            }


            return componentModel.GetService<T>();
        }

    }

}
