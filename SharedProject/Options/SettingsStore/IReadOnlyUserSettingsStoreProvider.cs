using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;

namespace FineCodeCoverage.Options
{
    internal interface IReadOnlyUserSettingsStoreProvider
    {
        Task<SettingsStore> ProvideAsync();
    }
}
