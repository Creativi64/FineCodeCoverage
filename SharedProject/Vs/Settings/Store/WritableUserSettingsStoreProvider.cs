using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IWritableUserSettingsStoreProvider))]
    internal sealed class WritableUserSettingsStoreProvider : IWritableUserSettingsStoreProvider
    {
        public AsyncLazy<WritableSettingsStore> LazySettingsStore { get; } = new AsyncLazy<WritableSettingsStore>(
            async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                return settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            },
            ThreadHelper.JoinableTaskFactory);
    }
}
