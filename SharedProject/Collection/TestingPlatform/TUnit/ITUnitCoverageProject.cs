using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitCoverageProject
    {
        string ExePath { get; }

        Task<string> GetConfigurationAsync(CancellationToken cancellationToken);

        ICoverageProject CoverageProject { get; }

        IVsHierarchy VsHierarchy { get; }

        bool HasCoverageExtension { get; }

        CommandLineParseResult CommandLineParseResult { get; }
    }
}
