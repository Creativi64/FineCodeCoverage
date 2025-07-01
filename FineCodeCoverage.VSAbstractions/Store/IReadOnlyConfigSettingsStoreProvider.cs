using System.Threading.Tasks;

namespace FineCodeCoverage.Options
{
    public interface IReadOnlyConfigSettingsStoreProvider
    {
        Task<ISettingsStore> ProvideAsync();
        ISettingsStore Provide();
    }
}
