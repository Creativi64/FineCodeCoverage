using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletConsoleExecutor
    {
		Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject,string coverletSettings);
    }
}
