using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ISolutionProjectsProvider
    {
        Task<bool> IsSolutionOpenAsync();

        Task<List<IVsHierarchy>> GetLoadedProjectsAsync(CancellationToken cancellationToken);
    }
}
