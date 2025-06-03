using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Output
{
    internal static class IconsViewModelProvider
    {
        static IconsViewModelProvider() => Instance = MefServiceProvider.Get<IIconsOptions>();
        public static IIconsOptions Instance { get; }
    }
}