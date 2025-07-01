using Microsoft.VisualStudio.Settings;

namespace FineCodeCoverage.Options
{
    internal sealed class WritableSettingsStoreWrapper : IWritableSettingsStore
    {
        private readonly WritableSettingsStore _store;

        public WritableSettingsStoreWrapper(WritableSettingsStore store) => _store = store;

        public bool CollectionExists(string collectionPath) => _store.CollectionExists(collectionPath);

        public void CreateCollection(string collectionPath) => _store.CreateCollection(collectionPath);

        public bool DeleteCollection(string collectionPath) => _store.DeleteCollection(collectionPath);

        public bool GetBoolean(string collectionPath, string propertyName, bool defaultValue) => _store.GetBoolean(collectionPath, propertyName, defaultValue);

        public string GetString(string collectionPath, string propertyName) => _store.GetString(collectionPath, propertyName);

        public bool PropertyExists(string collectionPath, string propertyName) => _store.PropertyExists(collectionPath, propertyName);

        public void SetBoolean(string collectionPath, string propertyName, bool value) => _store.SetBoolean(collectionPath, propertyName, value);

        public void SetString(string collectionPath, string propertyName, string value) => _store.SetString(collectionPath, propertyName, value);
    }
}
