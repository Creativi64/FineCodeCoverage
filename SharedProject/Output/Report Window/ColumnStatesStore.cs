using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Options;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IColumnStatesStore))]
    [Export(typeof(IClearSettingsOnShutdown))]
    internal class ColumnStatesStore : IColumnStatesStore, IClearSettingsOnShutdown
    {
        private readonly IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider;
        private const string ColumnStatesCollectionName = "FCCColumnStates";
        private const string ColumnStatesPropertyName = "FCCColumnStates";

        public bool ClearSettingsOnShutdown { get; set; }

        [ImportingConstructor]
        public ColumnStatesStore(IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider)
        {
            this.writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
        }
        public void SaveColumnStates(string columnStates)
        {
            if (ClearSettingsOnShutdown)
            {
                writableUserSettingsStoreProvider.Provide().DeleteCollection(ColumnStatesCollectionName);
                return;
            }
            EnsureCollection();
            writableUserSettingsStoreProvider.Provide().SetString(ColumnStatesCollectionName, ColumnStatesPropertyName, columnStates);
        }

        private void EnsureCollection()
        {
            if (!CollectionExists())
            {
                var store = writableUserSettingsStoreProvider.Provide();
                store.CreateCollection(ColumnStatesCollectionName);
            }
        }
        private bool CollectionExists()
        {
            var store = writableUserSettingsStoreProvider.Provide();
            return store.CollectionExists(ColumnStatesCollectionName);
        }
        public string GetColumnStates()
        {
            var store = writableUserSettingsStoreProvider.Provide();
            if (!CollectionExists())
            {
                return null;
            }
            return store.GetString(ColumnStatesCollectionName, ColumnStatesPropertyName);
        }
    }
}
