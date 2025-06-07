using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Core.Initialization
{
    internal interface IClearSettingsOnShutdown
    {
        AsyncLazy<bool> LazyShouldClearSettingsOnShutdown { get; }
    }
}
