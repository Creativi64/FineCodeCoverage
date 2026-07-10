using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ISolutionProjectsProvider
    {
        Task<bool> IsSolutionOpenAsync();

        Task<List<IVsHierarchy>> GetLoadedProjectsAsync(CancellationToken cancellationToken);
    }
}
