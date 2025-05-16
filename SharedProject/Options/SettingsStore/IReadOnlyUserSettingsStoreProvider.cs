using Microsoft.VisualStudio.Settings;
using System.Threading.Tasks;

namespace FineCodeCoverage.Options
{
    internal interface IReadOnlyUserSettingsStoreProvider
    {
        Task<SettingsStore> ProvideAsync();
    }
}
