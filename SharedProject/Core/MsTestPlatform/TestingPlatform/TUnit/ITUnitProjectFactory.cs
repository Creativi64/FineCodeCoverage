using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitProject : IDisposable
    {
        bool IsTUnit { get;} // probably will not change
        bool HasCoverageExtension { get;} // could change
        IVsHierarchy Hierarchy { get; }

        Task UpdateStateAsync();
    }

    internal interface ITUnitProjectFactory
    {
        ITUnitProject Create(IVsHierarchy project, ConfiguredProject unconfiguredProject);
    }
}
