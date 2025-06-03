using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Output
{
    internal static class CoveragePercentageBarOptionsViewModelProvider
    {
        public static CoveragePercentageBarOptionsViewModel Instance { get; }
        static CoveragePercentageBarOptionsViewModelProvider()
            => Instance = MefServiceProvider.Get<CoveragePercentageBarOptionsViewModel>();
    }
}