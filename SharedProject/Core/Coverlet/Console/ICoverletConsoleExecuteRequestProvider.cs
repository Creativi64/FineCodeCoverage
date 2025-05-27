using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletConsoleExecuteRequestProvider
    {
        Task<ExecuteRequest> GetExecuteRequestAsync(ICoverageProject project, string coverletSettings);
    }
}
