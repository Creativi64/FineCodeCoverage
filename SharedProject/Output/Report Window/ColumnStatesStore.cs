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
        private readonly IWritableUserSettingsStoreProvider _writableUserSettingsStoreProvider;
        private readonly IClearSettingsOnShutdown _clearSettingsOnShutdown;
        private readonly AsyncLazy<WritableSettingsStore> _lazyUserSettingsStore;
        private const string ColumnStatesCollectionName = "FCCColumnStates";
        private const string ColumnStatesPropertyName = "FCCColumnStates";

        public bool ClearSettingsOnShutdown { get; set; }

        [ImportingConstructor]
        public ColumnStatesStore(
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IClearSettingsOnShutdown clearSettingsOnShutdown
            )
        {
            this._writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
            this._clearSettingsOnShutdown = clearSettingsOnShutdown;
            this._lazyUserSettingsStore = this._writableUserSettingsStoreProvider.LazySettingsStore;
        }

        private async Task DeleteCollectionAsync()
        {
            WritableSettingsStore store = await this._lazyUserSettingsStore.GetValueAsync();
            _ = store.DeleteCollection(ColumnStatesCollectionName);
        }
        public async Task SaveColumnStatesAsync(string columnStates)
        {
            if (await this._clearSettingsOnShutdown.LazyShouldClearSettingsOnShutdown.GetValueAsync())
            {
                await this.DeleteCollectionAsync();
                return;
            }

            await this.EnsureCollectionAsync();
            WritableSettingsStore store = await this._lazyUserSettingsStore.GetValueAsync();
            store.SetString(ColumnStatesCollectionName, ColumnStatesPropertyName, columnStates);
        }

        private async Task EnsureCollectionAsync()
        {
            if (!await this.CollectionExistsAsync())
            {
                WritableSettingsStore store = await this._lazyUserSettingsStore.GetValueAsync();
                store.CreateCollection(ColumnStatesCollectionName);
            }
        }
        private async Task<bool> CollectionExistsAsync()
        {
            WritableSettingsStore store = await this._lazyUserSettingsStore.GetValueAsync();
            return store.CollectionExists(ColumnStatesCollectionName);
        }
        public async Task<string> GetColumnStatesAsync()
        {
            if (await this.CollectionExistsAsync())
            {
                WritableSettingsStore store = await this._lazyUserSettingsStore.GetValueAsync();
                return store.GetString(ColumnStatesCollectionName, ColumnStatesPropertyName);
            }

            return null;
        }
    }
}