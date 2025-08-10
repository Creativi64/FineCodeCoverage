using FineCodeCoverage.Utilities.ComponentModel;

namespace FineCodeCoverage.Output
{
    internal static class ReportHeaderBrushesViewModelProvider
    {
        public static ReportHeaderBrushesViewModel Instance { get; }

        static ReportHeaderBrushesViewModelProvider() => Instance = MefServiceProvider.Get<ReportHeaderBrushesViewModel>();
    }
}
