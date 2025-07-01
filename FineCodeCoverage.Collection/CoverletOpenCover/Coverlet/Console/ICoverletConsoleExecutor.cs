using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverletOpenCover.Process;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.Console
{
    public interface ICoverletConsoleExecutor
    {
        Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings);
    }
}
