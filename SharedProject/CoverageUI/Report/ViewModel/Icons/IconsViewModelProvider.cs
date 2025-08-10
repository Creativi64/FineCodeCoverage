using FineCodeCoverage.Utilities.ComponentModel;

namespace FineCodeCoverage.Output
{
    internal static class IconsViewModelProvider
    {
        static IconsViewModelProvider() => Instance = MefServiceProvider.Get<IIconMeasurementOptions>();

        public static IIconMeasurementOptions Instance { get; }
    }
}
