using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Core.Initialization
{
    interface IClearSettingsOnShutdown
    {
        AsyncLazy<bool> LazyShouldClearSettingsOnShutdown { get; }
    }
}
