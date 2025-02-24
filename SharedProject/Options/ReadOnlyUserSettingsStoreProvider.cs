using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IReadOnlyUserSettingsStoreProvider))]
    internal class ReadOnlyUserSettingsStoreProvider : IReadOnlyUserSettingsStoreProvider
    {
        private SettingsStore settingsStore;
        public async System.Threading.Tasks.Task<SettingsStore> ProvideAsync()
        {
            if (settingsStore == null)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                return settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
            }
            return settingsStore;
        }
    }
}
