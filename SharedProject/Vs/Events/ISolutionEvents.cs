using System;

namespace FineCodeCoverage.Vs.Events
{
    internal interface ISolutionEvents
    {
        event EventHandler AfterClosing;

        event EventHandler AfterOpen;
    }
}
