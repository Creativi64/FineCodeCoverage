using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IColumnStatesStore))]
    internal class ColumnStatesStore : IColumnStatesStore
    {
        private readonly IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider;
        private readonly IClearSettingsOnShutdown clearSettingsOnShutdown;
        private readonly AsyncLazy<WritableSettingsStore> lazyUserSettingsStore;
        private const string ColumnStatesCollectionName = "FCCColumnStates";
        private const string ColumnStatesPropertyName = "FCCColumnStates";

        public bool ClearSettingsOnShutdown { get; set; }

        [ImportingConstructor]
        public ColumnStatesStore(
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IClearSettingsOnShutdown clearSettingsOnShutdown
            )
        {
            this.writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
            this.clearSettingsOnShutdown = clearSettingsOnShutdown;
            this.lazyUserSettingsStore = this.writableUserSettingsStoreProvider.LazySettingsStore;
        }

        private async Task DeleteCollectionAsync()
        {
            WritableSettingsStore store = await lazyUserSettingsStore.GetValueAsync();
            store.DeleteCollection(ColumnStatesCollectionName);
        }
        public async Task SaveColumnStatesAsync(string columnStates)
        {
            if (await clearSettingsOnShutdown.LazyShouldClearSettingsOnShutdown.GetValueAsync())
            {
                await DeleteCollectionAsync();
                return;
            }
            await EnsureCollectionAsync();
            WritableSettingsStore store = await lazyUserSettingsStore.GetValueAsync();
            store.SetString(ColumnStatesCollectionName, ColumnStatesPropertyName, columnStates);
        }

        private async Task EnsureCollectionAsync()
        {
            if (!await CollectionExistsAsync())
            {
                WritableSettingsStore store = await lazyUserSettingsStore.GetValueAsync();
                store.CreateCollection(ColumnStatesCollectionName);
            }
        }
        private async Task<bool> CollectionExistsAsync()
        {
            WritableSettingsStore store = await lazyUserSettingsStore.GetValueAsync();
            return store.CollectionExists(ColumnStatesCollectionName);
        }
        public async Task<string> GetColumnStatesAsync()
        {
            if (await CollectionExistsAsync())
            {
                WritableSettingsStore store = await lazyUserSettingsStore.GetValueAsync();
                return store.GetString(ColumnStatesCollectionName, ColumnStatesPropertyName);
            }
            return null;

        }
    }
}
