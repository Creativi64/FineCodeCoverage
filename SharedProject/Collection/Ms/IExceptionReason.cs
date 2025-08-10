using System;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface IExceptionReason
    {
        Exception Exception { get; }

        string Reason { get; }
    }
}
