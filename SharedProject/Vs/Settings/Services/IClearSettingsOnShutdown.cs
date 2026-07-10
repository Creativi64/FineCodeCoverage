using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Vs.Settings.Services
{
    internal interface IClearSettingsOnShutdown
    {
        AsyncLazy<bool> LazyShouldClearSettingsOnShutdown { get; }
    }
}
