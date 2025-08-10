using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ITUnitProjectFactory
    {
        ITUnitProject Create(IVsHierarchy project, ConfiguredProject configuredProject);
    }
}
