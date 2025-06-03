using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Options
{
    internal interface IWritableUserSettingsStoreProvider
    {
        AsyncLazy<WritableSettingsStore> LazySettingsStore { get; }
    }
}