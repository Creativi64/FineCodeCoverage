using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IReadOnlyUserSettingsStoreProvider))]
    internal sealed class ReadOnlyUserSettingsStoreProvider : IReadOnlyUserSettingsStoreProvider
    {
        private SettingsStoreWrapper _settingsStore;

        public async System.Threading.Tasks.Task<ISettingsStore> ProvideAsync()
        {
            if (_settingsStore == null)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                var settingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
                _settingsStore = new SettingsStoreWrapper(settingsStore);
            }

            return _settingsStore;
        }
    }
}
