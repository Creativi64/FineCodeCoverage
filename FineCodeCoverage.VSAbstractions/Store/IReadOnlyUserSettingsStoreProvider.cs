using System.Threading.Tasks;

namespace FineCodeCoverage.VSAbstractions.Store
{
    public interface IReadOnlyUserSettingsStoreProvider
    {
        Task<ISettingsStore> ProvideAsync();
    }
}
