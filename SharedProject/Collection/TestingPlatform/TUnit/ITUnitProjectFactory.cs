using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitProjectFactory
    {
        ITUnitProject Create(IVsHierarchy project, ConfiguredProject configuredProject);
    }
}
