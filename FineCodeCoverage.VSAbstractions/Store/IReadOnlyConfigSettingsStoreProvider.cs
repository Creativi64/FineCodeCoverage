using System.Threading.Tasks;

namespace FineCodeCoverage.VSAbstractions.Store
{
    public interface IReadOnlyConfigSettingsStoreProvider
    {
        Task<ISettingsStore> ProvideAsync();
        ISettingsStore Provide();
    }
}
