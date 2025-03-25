using FineCodeCoverage.Core.Initialization;
using NUnit.Framework;

namespace FineCodeCoverageTests.TestHelpers
{
    internal static class ExportsInitializable
    {
        public static void Should_Export_IInitializable<T>()
        {
            Assert.That(MEFExportHelper.IsAndExports<T, IInitializable>(), Is.True);
        }
    }
}
