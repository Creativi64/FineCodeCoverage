using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Options
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IReadOnlyConfigSettingsStoreProvider))]
    internal class ReadOnlyConfigSettingsStoreProvider : IReadOnlyConfigSettingsStoreProvider
    {
        public AsyncLazy<SettingsStore> LazySettingsStore { get; } = new AsyncLazy<SettingsStore>(async () => {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                return settingsManager.GetReadOnlySettingsStore(SettingsScope.Configuration);
            },ThreadHelper.JoinableTaskFactory);
    }
}
