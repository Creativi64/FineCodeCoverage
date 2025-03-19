using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitProjectsProvider
    {
        Task<List<IVsHierarchy>> GetTUnitProjectsWithCoverageExtensionAsync();
    }
}
