using System.Threading.Tasks;

namespace FineCodeCoverage.Options
{
    public interface IWritableUserSettingsStoreProvider
    {
        Task<IWritableSettingsStore> ProvideAsync();
        IWritableSettingsStore Provide();
    }
}
