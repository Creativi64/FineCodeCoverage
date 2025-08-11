using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.VSAbstractions.Store;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Vs.Settings.Store
{
    [Export(typeof(IReadOnlyUserSettingsStoreProvider))]
    internal sealed class ReadOnlyUserSettingsStoreProvider : IReadOnlyUserSettingsStoreProvider
    {
        private readonly AsyncLazy<ISettingsStore> _lazySettingsStore = new AsyncLazy<ISettingsStore>(
            async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                SettingsStore settingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
                return new SettingsStoreWrapper(settingsStore);
            },
            ThreadHelper.JoinableTaskFactory);

        public Task<ISettingsStore> ProvideAsync() => _lazySettingsStore.GetValueAsync();

        public ISettingsStore Provide() => _lazySettingsStore.GetValue();
    }
}
