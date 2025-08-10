using System;

namespace FineCodeCoverage.Vs.Events
{
    internal interface IVsShutdown
    {
        event EventHandler<EventArgs> Shutdown;
    }
}
