using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.Coverlet
{
    public interface ICoverletConsoleExecutor
    {
        Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings);
    }
}
