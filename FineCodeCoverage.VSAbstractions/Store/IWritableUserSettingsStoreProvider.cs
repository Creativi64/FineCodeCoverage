using System.Threading.Tasks;

namespace FineCodeCoverage.VSAbstractions.Store
{
    public interface IWritableUserSettingsStoreProvider
    {
        Task<IWritableSettingsStore> ProvideAsync();
        IWritableSettingsStore Provide();
    }
}
