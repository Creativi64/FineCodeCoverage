using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    public interface ICoverletConsoleExecutor
    {
        Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings);
    }
}
