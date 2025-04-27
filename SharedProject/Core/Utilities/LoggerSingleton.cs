using FineCodeCoverage.Output;
using System;

namespace FineCodeCoverage.Core.Utilities
{
 internal static class LoggerSingleton
    {
        private static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() =>
        {
            return MefServiceProvider.Get<ILogger>();
        });

        public static ILogger Instance => _logger.Value;
    }
}
