using System;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface IExceptionReason
    {
        Exception Exception { get; }

        string Reason { get; }
    }
}
