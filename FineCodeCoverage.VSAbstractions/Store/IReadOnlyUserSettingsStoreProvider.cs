using System.Threading.Tasks;

namespace FineCodeCoverage.Options
{
    public interface IReadOnlyUserSettingsStoreProvider
    {
        Task<ISettingsStore> ProvideAsync();
    }
}
