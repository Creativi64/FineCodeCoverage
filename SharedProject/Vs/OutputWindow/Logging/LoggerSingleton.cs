using System;
using FineCodeCoverage.VSAbstractions.OutputWindow;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class LoggerSingleton
    {
        private static readonly Lazy<ILogger> s_logger = new Lazy<ILogger>(() => MefServiceProvider.Get<ILogger>());

        public static ILogger Instance => s_logger.Value;
    }
}
