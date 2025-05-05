using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Output
{
    internal static class ShowIconsViewModelProvider
    {
        static ShowIconsViewModelProvider()
        {
            Instance = MefServiceProvider.Get<IShowIcons>();
        }
        public static IShowIcons Instance { get; }
    }
}
