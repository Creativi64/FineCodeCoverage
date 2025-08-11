using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.VSAbstractions.Store;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Vs.Settings.Store
{
    [Export(typeof(IWritableUserSettingsStoreProvider))]
    internal sealed class WritableUserSettingsStoreProvider : IWritableUserSettingsStoreProvider
    {
        private readonly AsyncLazy<IWritableSettingsStore> _lazySettingsStore = new AsyncLazy<IWritableSettingsStore>(
            async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                WritableSettingsStore writableSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
                return new WritableSettingsStoreWrapper(writableSettingsStore);
            },
            ThreadHelper.JoinableTaskFactory);

        public Task<IWritableSettingsStore> ProvideAsync() => _lazySettingsStore.GetValueAsync();

        public IWritableSettingsStore Provide() => _lazySettingsStore.GetValue();
    }
}
