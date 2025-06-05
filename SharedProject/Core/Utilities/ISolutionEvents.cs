using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface ISolutionEvents
    {
        event EventHandler AfterClosing;

        event EventHandler AfterOpen;
    }
}
