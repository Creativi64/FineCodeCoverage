using FineCodeCoverage.Output;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class LoggerSingleton
    {
        private static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() =>
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            return componentModel == null
                ? throw new InvalidOperationException("IComponentModel service not available.")
                : componentModel.GetService<ILogger>();
        });

        public static ILogger Instance => _logger.Value;
    }
}
