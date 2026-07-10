using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FineCodeCoverage.VSAbstractions.Store;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Vs.Settings.Store
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IReadOnlyConfigSettingsStoreProvider))]
    internal sealed class ReadOnlyConfigSettingsStoreProvider : IReadOnlyConfigSettingsStoreProvider
    {
        private readonly AsyncLazy<ISettingsStore> _lazySettingsStore = new AsyncLazy<ISettingsStore>(
            async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                SettingsStore settingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.Configuration);
                return new SettingsStoreWrapper(settingsStore);
            },
            ThreadHelper.JoinableTaskFactory);

        public Task<ISettingsStore> ProvideAsync() => _lazySettingsStore.GetValueAsync();

        public ISettingsStore Provide() => _lazySettingsStore.GetValue();
    }
}
