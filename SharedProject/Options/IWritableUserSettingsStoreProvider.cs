using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Options
{
    internal interface IWritableUserSettingsStoreProvider
    {
        //WritableSettingsStore Provide();
        AsyncLazy<WritableSettingsStore> LazySettingsStore { get; }
    }
}
