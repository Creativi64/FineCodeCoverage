using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Options
{
    internal interface IReadOnlyConfigSettingsStoreProvider
    {
        AsyncLazy<SettingsStore> LazySettingsStore { get; }
    }
}