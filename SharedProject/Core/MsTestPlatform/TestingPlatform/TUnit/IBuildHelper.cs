using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface IBuildHelper
    {
        Task<bool> BuildAsync(List<IVsHierarchy> projects, System.Threading.CancellationToken cancellationToken);
    }
}
