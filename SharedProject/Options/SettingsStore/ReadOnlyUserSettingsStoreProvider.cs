using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IReadOnlyUserSettingsStoreProvider))]
    internal class ReadOnlyUserSettingsStoreProvider : IReadOnlyUserSettingsStoreProvider
    {
        private SettingsStore _settingsStore;
        public async System.Threading.Tasks.Task<SettingsStore> ProvideAsync()
        {
            if (_settingsStore == null)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                _settingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
            }

            return _settingsStore;
        }
    }
}
