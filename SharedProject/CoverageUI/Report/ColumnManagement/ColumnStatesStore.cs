using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.VSAbstractions.Store;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IColumnStatesStore))]
    internal sealed class ColumnStatesStore : IColumnStatesStore
    {
        private const string ColumnStatesCollectionName = "FCCColumnStates";
        private const string ColumnStatesPropertyName = "FCCColumnStates";
        private readonly IWritableUserSettingsStoreProvider _writableUserSettingsStoreProvider;
        private readonly IClearSettingsOnShutdown _clearSettingsOnShutdown;

        public bool ClearSettingsOnShutdown { get; set; }

        [ImportingConstructor]
        public ColumnStatesStore(
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IClearSettingsOnShutdown clearSettingsOnShutdown)
        {
            _writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
            _clearSettingsOnShutdown = clearSettingsOnShutdown;
        }

        private async Task DeleteCollectionAsync()
        {
            IWritableSettingsStore store = await _writableUserSettingsStoreProvider.ProvideAsync();
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
            IWritableSettingsStore store = await _writableUserSettingsStoreProvider.ProvideAsync();
            store.SetString(ColumnStatesCollectionName, ColumnStatesPropertyName, columnStates);
        }

        private async Task EnsureCollectionAsync()
        {
            if (await CollectionExistsAsync())
            {
                return;
            }

            IWritableSettingsStore store = await _writableUserSettingsStoreProvider.ProvideAsync();
            store.CreateCollection(ColumnStatesCollectionName);
        }

        private async Task<bool> CollectionExistsAsync()
        {
            IWritableSettingsStore store = await _writableUserSettingsStoreProvider.ProvideAsync();
            return store.CollectionExists(ColumnStatesCollectionName);
        }

        public async Task<string> GetColumnStatesAsync()
        {
            if (!await CollectionExistsAsync())
            {
                return null;
            }

            IWritableSettingsStore store = await _writableUserSettingsStoreProvider.ProvideAsync();
            return store.GetString(ColumnStatesCollectionName, ColumnStatesPropertyName);
        }
    }
}
