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
        private const string ColumnStatesCollectionName = "FCCColumnStates";
        private const string ColumnStatesPropertyName = "FCCColumnStates";
        private readonly IWritableUserSettingsStoreProvider _writableUserSettingsStoreProvider;
        private readonly IClearSettingsOnShutdown _clearSettingsOnShutdown;
        private readonly AsyncLazy<WritableSettingsStore> _lazyUserSettingsStore;

        public bool ClearSettingsOnShutdown { get; set; }

        [ImportingConstructor]
        public ColumnStatesStore(
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IClearSettingsOnShutdown clearSettingsOnShutdown)
        {
            _writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
            _clearSettingsOnShutdown = clearSettingsOnShutdown;
            _lazyUserSettingsStore = _writableUserSettingsStoreProvider.LazySettingsStore;
        }

        private async Task DeleteCollectionAsync()
        {
            WritableSettingsStore store = await _lazyUserSettingsStore.GetValueAsync();
            _ = store.DeleteCollection(ColumnStatesCollectionName);
        }

        public async Task SaveColumnStatesAsync(string columnStates)
        {
            if (await _clearSettingsOnShutdown.LazyShouldClearSettingsOnShutdown.GetValueAsync())
            {
                await DeleteCollectionAsync();
                return;
            }

            await EnsureCollectionAsync();
            WritableSettingsStore store = await _lazyUserSettingsStore.GetValueAsync();
            store.SetString(ColumnStatesCollectionName, ColumnStatesPropertyName, columnStates);
        }

        private async Task EnsureCollectionAsync()
        {
            if (await CollectionExistsAsync())
            {
                return;
            }

            WritableSettingsStore store = await _lazyUserSettingsStore.GetValueAsync();
            store.CreateCollection(ColumnStatesCollectionName);
        }

        private async Task<bool> CollectionExistsAsync()
        {
            WritableSettingsStore store = await _lazyUserSettingsStore.GetValueAsync();
            return store.CollectionExists(ColumnStatesCollectionName);
        }

        public async Task<string> GetColumnStatesAsync()
        {
            if (!await CollectionExistsAsync())
            {
                return null;
            }

            WritableSettingsStore store = await _lazyUserSettingsStore.GetValueAsync();
            return store.GetString(ColumnStatesCollectionName, ColumnStatesPropertyName);
        }
    }
}
