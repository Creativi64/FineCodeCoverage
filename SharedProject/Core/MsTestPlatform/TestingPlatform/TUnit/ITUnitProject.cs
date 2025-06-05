using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitProject : IDisposable
    {
        bool IsTUnit { get; } // probably will not change

        bool HasCoverageExtension { get; } // could change

        IVsHierarchy Hierarchy { get; }

        Task UpdateStateAsync(CancellationToken cancellationToken);

        CommandLineParseResult CommandLineParseResult { get; }
    }
}
