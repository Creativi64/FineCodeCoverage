using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Threading;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IColumnStatesStore))]
    [Export(typeof(IClearSettingsOnShutdown))]
    internal class ColumnStatesStore : IColumnStatesStore, IClearSettingsOnShutdown
    {
        private readonly IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider;
        private readonly AsyncLazy<WritableSettingsStore> lazyUserSettingsStore;
        private const string ColumnStatesCollectionName = "FCCColumnStates";
        private const string ColumnStatesPropertyName = "FCCColumnStates";

        public bool ClearSettingsOnShutdown { get; set; }

        [ImportingConstructor]
        public ColumnStatesStore(IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider)
        {
            this.writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
            this.lazyUserSettingsStore = this.writableUserSettingsStoreProvider.LazySettingsStore;
        }

        private async Task DeleteCollectionAsync()
        {
            var store = await lazyUserSettingsStore.GetValueAsync();
            store.DeleteCollection(ColumnStatesCollectionName);
        }
        public async Task SaveColumnStatesAsync(string columnStates)
        {
            if (ClearSettingsOnShutdown)
            {
                await DeleteCollectionAsync();
                return;
            }
            await EnsureCollectionAsync();
            var store = await lazyUserSettingsStore.GetValueAsync();
            store.SetString(ColumnStatesCollectionName, ColumnStatesPropertyName, columnStates);
        }

        private async Task EnsureCollectionAsync()
        {
            if (!await CollectionExistsAsync())
            {
                var store = await lazyUserSettingsStore.GetValueAsync();
                store.CreateCollection(ColumnStatesCollectionName);
            }
        }
        private async Task<bool> CollectionExistsAsync()
        {
            var store = await lazyUserSettingsStore.GetValueAsync();
            return store.CollectionExists(ColumnStatesCollectionName);
        }
        public async Task<string> GetColumnStatesAsync()
        {
            if (await CollectionExistsAsync())
            {
                var store = await lazyUserSettingsStore.GetValueAsync();
                return store.GetString(ColumnStatesCollectionName, ColumnStatesPropertyName);
            }
            return null;

        }
    }
}
