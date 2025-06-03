using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletConsoleExecuteRequestProvider
    {
        Task<ExecuteRequest> GetExecuteRequestAsync(ICoverageProject project, string coverletSettings);
    }
}