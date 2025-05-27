using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;
using System.Threading;

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
