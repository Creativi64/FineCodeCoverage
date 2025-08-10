using System;
using FineCodeCoverage.Utilities.ComponentModel;
using FineCodeCoverage.VSAbstractions.OutputWindow;

namespace FineCodeCoverage.Vs.OutputWindow.Logging
{
    internal static class LoggerSingleton
    {
        private static readonly Lazy<ILogger> s_logger = new Lazy<ILogger>(() => MefServiceProvider.Get<ILogger>());

        public static ILogger Instance => s_logger.Value;
    }
}
