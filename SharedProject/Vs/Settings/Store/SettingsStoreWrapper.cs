using FineCodeCoverage.VSAbstractions.Store;
using Microsoft.VisualStudio.Settings;

namespace FineCodeCoverage.Options
{
    internal sealed class SettingsStoreWrapper : ISettingsStore
    {
        private readonly SettingsStore _settingsStore;

        public SettingsStoreWrapper(SettingsStore settingsStore) => _settingsStore = settingsStore;

        public bool CollectionExists(string collectionPath) => _settingsStore.CollectionExists(collectionPath);
    }
}
