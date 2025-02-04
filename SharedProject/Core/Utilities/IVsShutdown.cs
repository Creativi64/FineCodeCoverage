using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IVsShutdown
    {
        event EventHandler<EventArgs> Shutdown;
    }
}
