using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitCoverageProject
    {
        string ExePath { get; }
        Task<string> GetConfigurationAsync(CancellationToken cancellationToken);
        ICoverageProject CoverageProject { get; }
        IVsHierarchy VsHierarchy { get; }
        bool HasCoverageExtension { get; }
    }
    internal interface ITUnitCoverageProjectFactory
    {
        Task<ITUnitCoverageProject> CreateCoverageProjectAsync(
            IVsHierarchy project,bool hasCoverageExtension,CancellationToken cancellationToken);
    }
}
