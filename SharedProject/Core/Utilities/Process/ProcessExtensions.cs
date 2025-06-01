using System;
using System.Diagnostics;
using System.Linq;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class ProcessExtensions
    {
        public static string GetOutput(this Process process)
            => string.Join(
                Environment.NewLine,
                new[]
                {
                    process.StandardOutput?.ReadToEnd(),
                    process.StandardError?.ReadToEnd()
                }.Where(x => !string.IsNullOrWhiteSpace(x))
            );
    }
}
