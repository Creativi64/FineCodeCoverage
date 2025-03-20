using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitProject
    {
        //IVsHierarchy VsHierarchy { get; } // just need the Guid
        bool IsTUnit { get;} // probably will not change
        bool HasCoverageExtension { get;} // could change
        IVsHierarchy Hierarchy { get; }

        Task UpdateStateAsync(bool force);
    }

    internal interface ITUnitProjectFactory
    {
        ITUnitProject Create(IVsHierarchy project);
    }
}
